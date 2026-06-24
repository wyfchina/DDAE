namespace AdaptiveSopDdsop.Web.Domain;

public sealed class ScenarioOptimizationService
{
    private static readonly OptimizationProfile[] DefaultProfiles =
    [
        new("ServiceFirst", "服务优先"),
        new("CashFirst", "库存资金优先"),
        new("CapacityFirst", "产能平衡优先")
    ];

    private readonly IScenarioWorkspaceDataSource _dataSource;
    private readonly ScenarioRunPreviewService _previewService;
    private readonly IReadOnlyDictionary<string, IOptimizationSolver> _solvers;

    public ScenarioOptimizationService(
        IScenarioWorkspaceDataSource dataSource,
        ScenarioRunPreviewService previewService,
        IEnumerable<IOptimizationSolver> solvers)
    {
        _dataSource = dataSource;
        _previewService = previewService;
        _solvers = solvers.ToDictionary(solver => solver.SolverName, StringComparer.OrdinalIgnoreCase);
    }

    public ScenarioOptimizationResponse Optimize(ScenarioOptimizationRequest request)
    {
        var baseRequest = NormalizeBaseRequest(request.BaseRequest);
        var recommendationCount = Math.Clamp(request.RecommendationCount <= 0 ? 3 : request.RecommendationCount, 1, 3);
        var maxActions = Math.Clamp(request.MaxActionsPerRecommendation <= 0 ? 3 : request.MaxActionsPerRecommendation, 1, 5);
        var baseline = _previewService.Preview(baseRequest);
        var solver = ResolveSolver(request.SolverName);
        var data = _dataSource.Load(new ScenarioWorkspaceDataRequest(
            baseline.Request.HorizonWeeks,
            new DateOnly(2026, 6, 1),
            baseline.Request.SkuFilter,
            baseline.Request.FamilyFilter));
        var seeds = BuildCandidateSeeds(data, baseline)
            .Take(18)
            .ToList();
        var profiles = SelectProfiles(request.TargetMode, recommendationCount);
        var recommendations = new List<ScenarioOptimizationRecommendation>();
        var candidateImpactMatrix = new List<OptimizationCandidateImpact>();
        var scenarioComparisons = new List<ScenarioOptimizationComparison>();
        var responseTrace = new List<ScenarioAuditTrace>
        {
            new("Optimization", $"生成 {seeds.Count} 个候选动作，使用 {solver.SolverName} 选择推荐组合。", "Information"),
            new("Optimization", "候选动作影响矩阵由单动作白盒预览估算；求解器只选择候选动作组合，最终库存、RCCP、供应和约束结果仍由白盒预览引擎重新计算。", "Information")
        };

        foreach (var profile in profiles)
        {
            var candidates = EstimateCandidates(profile, baseRequest, baseline, seeds);
            var problem = new OptimizationProblem(
                $"DD-SOP-{profile.ProfileId}-{Guid.NewGuid():N}",
                profile.ProfileId,
                maxActions,
                Math.Max(0m, baseline.Scenario.Metrics.AverageInventoryValue * 0.2m),
                Math.Max(250_000m, baseline.Scenario.Metrics.AverageInventoryValue * 0.08m),
                candidates);
            var solution = solver.Solve(problem);
            candidateImpactMatrix.AddRange(candidates.Select(candidate => candidate.Impact));

            if (solution.Status is OptimizationSolverStatus.Unavailable or OptimizationSolverStatus.Error)
            {
                responseTrace.Add(new(solution.SolverName, solution.Message, "Warning"));
                return new ScenarioOptimizationResponse(
                    solution.Status,
                    solution.SolverName,
                    solution.Message,
                    candidateImpactMatrix,
                    scenarioComparisons,
                    Array.Empty<ScenarioOptimizationRecommendation>(),
                    responseTrace,
                    IsPersisted: false);
            }

            var selectedCandidates = candidates
                .Where(candidate => solution.SelectedCandidateIds.Contains(candidate.CandidateId, StringComparer.Ordinal))
                .ToList();
            var previewRequest = MergeCandidateParameters(baseRequest, selectedCandidates.Select(candidate => candidate.Parameters));
            var preview = _previewService.Preview(previewRequest);
            var actions = selectedCandidates
                .Select(candidate => new ScenarioOptimizationAction(
                    candidate.CandidateId,
                    candidate.ActionType,
                    candidate.Target,
                    candidate.EstimatedCost,
                    candidate.Impact,
                    candidate.Explanation))
                .ToList();
            var comparison = BuildScenarioOptimizationComparison(profile, baseline, preview, actions);
            scenarioComparisons.Add(comparison);

            recommendations.Add(new ScenarioOptimizationRecommendation(
                profile.ProfileId,
                profile.Name,
                BuildRecommendationSummary(profile, baseline, preview, actions),
                solution.Status,
                solution.Message,
                solution.ObjectiveValue,
                previewRequest,
                preview,
                comparison,
                actions.Sum(action => action.EstimatedCost),
                actions,
                BuildRecommendationTrace(profile, solution, actions)));
        }

        var responseStatus = recommendations.Any(item => item.SolverStatus == OptimizationSolverStatus.Optimal)
            ? OptimizationSolverStatus.Optimal
            : recommendations.Any(item => item.SolverStatus == OptimizationSolverStatus.Feasible)
                ? OptimizationSolverStatus.Feasible
                : OptimizationSolverStatus.Infeasible;

        responseTrace.Add(new("Optimization", $"返回 {recommendations.Count} 个推荐方案；推荐不会自动采纳、不会保存、不会提交审批。", "Information"));

        return new ScenarioOptimizationResponse(
            responseStatus,
            solver.SolverName,
            recommendations.Count == 0 ? "没有生成推荐方案。" : "优化推荐已生成。",
            candidateImpactMatrix
                .GroupBy(item => item.CandidateId, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderByDescending(item => item.ServiceImpactPercent)
                .ThenBy(item => item.EstimatedCost)
                .ToList(),
            scenarioComparisons,
            recommendations,
            responseTrace,
            IsPersisted: false);
    }

    private IOptimizationSolver ResolveSolver(string? solverName)
    {
        if (!string.IsNullOrWhiteSpace(solverName) && _solvers.TryGetValue(solverName, out var requested))
        {
            return requested;
        }

        if (_solvers.TryGetValue("Gurobi", out var gurobi))
        {
            return gurobi;
        }

        return _solvers.Values.First();
    }

    private IReadOnlyList<OptimizationCandidate> EstimateCandidates(
        OptimizationProfile profile,
        ScenarioRunPreviewRequest baseRequest,
        ScenarioRunPreviewResult baseline,
        IReadOnlyList<OptimizationCandidateSeed> seeds)
    {
        var candidates = new List<OptimizationCandidate>();
        foreach (var seed in seeds)
        {
            var previewRequest = MergeCandidateParameters(baseRequest, [seed.Parameters]);
            var candidatePreview = _previewService.Preview(previewRequest);
            var objective = Score(profile.ProfileId, baseline, candidatePreview, seed.ActionType);
            var inventoryDelta = candidatePreview.Scenario.Metrics.AverageInventoryValue - baseline.Scenario.Metrics.AverageInventoryValue;
            var estimatedCost = EstimateActionCost(seed, baseline, candidatePreview);
            var impact = BuildCandidateImpact(
                $"{profile.ProfileId}-{seed.CandidateId}",
                seed,
                baseline,
                candidatePreview,
                estimatedCost);
            var adjustedObjective = objective - estimatedCost / 100000m;

            candidates.Add(new OptimizationCandidate(
                $"{profile.ProfileId}-{seed.CandidateId}",
                seed.ActionType,
                seed.Target,
                seed.ConflictKey,
                Math.Max(0.001m, decimal.Round(adjustedObjective, 3)),
                decimal.Round(Math.Max(0m, inventoryDelta), 0),
                estimatedCost,
                impact,
                impact.ConstraintNote,
                impact.FeasibilityStatus,
                seed.Parameters,
                seed.Explanation));
        }

        return candidates
            .OrderByDescending(candidate => candidate.ObjectiveValue)
            .ThenBy(candidate => candidate.InventoryDelta)
            .Take(12)
            .ToList();
    }

    private static IReadOnlyList<OptimizationCandidateSeed> BuildCandidateSeeds(
        ScenarioWorkspaceDataSet data,
        ScenarioRunPreviewResult baseline)
    {
        var seeds = new List<OptimizationCandidateSeed>();
        var skuMap = data.Skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);

        foreach (var group in baseline.Scenario.BufferTrend.WeeklyCells
                     .Where(cell => cell.Status is "Red" or "Yellow")
                     .GroupBy(cell => cell.Sku)
                     .OrderByDescending(group => group.Count(cell => cell.Status == "Red"))
                     .ThenByDescending(group => group.Count())
                     .Take(5))
        {
            if (!skuMap.TryGetValue(group.Key, out var sku))
            {
                continue;
            }

            var firstRiskWeek = group.Min(cell => cell.Week);
            var buildWeek = Math.Max(1, firstRiskWeek - 1);
            var quantity = decimal.Round(Math.Max(sku.MinimumOrderQuantity, sku.Adu * 7m), 0);
            seeds.Add(new OptimizationCandidateSeed(
                $"PREBUILD-{sku.Sku}-{firstRiskWeek}",
                "提前建库",
                sku.Sku,
                $"SKU:{sku.Sku}:BUFFER",
                new ScenarioRunParameterSet(
                    PrebuildCampaigns: [new PrebuildCampaign($"OPT-PREBUILD-{sku.Sku}-{firstRiskWeek}", sku.Sku, buildWeek, firstRiskWeek, Math.Min(data.Request.HorizonWeeks, firstRiskWeek + 2), quantity)]),
                $"{sku.Name} 在第 {firstRiskWeek} 周进入风险区，候选动作是在第 {buildWeek} 周提前建库 {quantity:0}。"));
        }

        foreach (var summary in baseline.Scenario.Rccp.ResourceSummaries
                     .Where(item => item.Status is "Red" or "Yellow")
                     .OrderByDescending(item => item.PeakLoadPercent)
                     .Take(5))
        {
            var peakCell = baseline.Scenario.Rccp.WeeklyCells
                .Where(cell => cell.ResourceCode == summary.ResourceCode)
                .OrderByDescending(cell => cell.LoadPercent)
                .FirstOrDefault();
            if (peakCell is null)
            {
                continue;
            }

            var multiplier = peakCell.LoadPercent > 120m ? 1.3m : peakCell.LoadPercent > 100m ? 1.2m : 1.1m;
            seeds.Add(new OptimizationCandidateSeed(
                $"CAPACITY-{summary.ResourceCode}-{peakCell.Week}",
                "产能缓冲",
                summary.ResourceCode,
                $"RESOURCE:{summary.ResourceCode}:W{peakCell.Week}",
                new ScenarioRunParameterSet(
                    CapacityAdjustments: [new ResourceCapacityAdjustment(summary.ResourceCode, peakCell.Week, multiplier, "优化推荐候选")]),
                $"{summary.ResourceName} 峰值负荷 {summary.PeakLoadPercent:0.#}%，候选动作是在第 {peakCell.Week} 周调整能力倍率到 {multiplier:0.##}。"));
        }

        foreach (var summary in baseline.Scenario.Constraints.SupplySummaries
                     .Where(item => item.Status is "Red" or "Yellow")
                     .OrderByDescending(item => item.TotalGap)
                     .Take(5))
        {
            var pressureCell = baseline.Scenario.Constraints.SupplyCells
                .Where(cell => cell.Supplier == summary.Supplier && cell.MaterialFamily == summary.MaterialFamily)
                .OrderByDescending(cell => cell.Gap)
                .ThenByDescending(cell => cell.LoadPercent)
                .FirstOrDefault();
            if (pressureCell is null)
            {
                continue;
            }

            var committed = decimal.Round(Math.Max(pressureCell.ConstrainedAvailable, pressureCell.UnconstrainedRequired), 0);
            seeds.Add(new OptimizationCandidateSeed(
                $"SUPPLY-{summary.Supplier}-{summary.MaterialFamily}-{pressureCell.Week}",
                "供应承诺",
                $"{summary.Supplier} / {summary.MaterialFamily}",
                $"SUPPLIER:{summary.Supplier}:{summary.MaterialFamily}:W{pressureCell.Week}",
                new ScenarioRunParameterSet(
                    SupplierCapacityLimits: [new SupplierCapacityLimit(summary.Supplier, summary.MaterialFamily, pressureCell.Week, pressureCell.Week, committed)]),
                $"{summary.Supplier} 的 {summary.MaterialFamily} 在第 {pressureCell.Week} 周供应压力最高，候选动作是确认承诺能力 {committed:0}。"));
        }

        foreach (var group in baseline.Scenario.BufferTrend.WeeklyCells
                     .Where(cell => cell.Status == "Red")
                     .GroupBy(cell => cell.Sku)
                     .OrderByDescending(group => group.Count())
                     .Take(4))
        {
            if (!skuMap.TryGetValue(group.Key, out var sku) || sku.OrderCycleDays <= 1)
            {
                continue;
            }

            var proposedCycle = Math.Max(1, sku.OrderCycleDays / 2);
            seeds.Add(new OptimizationCandidateSeed(
                $"POLICY-{sku.Sku}-{proposedCycle}",
                "订货策略",
                sku.Sku,
                $"SKU:{sku.Sku}:POLICY",
                new ScenarioRunParameterSet(
                    SkuPolicyOverrides: [new SkuPolicyOverride(sku.Sku, OrderCycleDays: proposedCycle)]),
                $"{sku.Name} 多次进入红区，候选动作是将订货周期从 {sku.OrderCycleDays} 天调整为 {proposedCycle} 天。"));
        }

        return seeds;
    }

    private static ScenarioRunPreviewRequest MergeCandidateParameters(
        ScenarioRunPreviewRequest request,
        IEnumerable<ScenarioRunParameterSet> additions)
    {
        var prebuild = new List<PrebuildCampaign>(request.Parameters?.PrebuildCampaigns ?? Array.Empty<PrebuildCampaign>());
        var capacity = new List<ResourceCapacityAdjustment>(request.Parameters?.CapacityAdjustments ?? Array.Empty<ResourceCapacityAdjustment>());
        var policies = new List<SkuPolicyOverride>(request.Parameters?.SkuPolicyOverrides ?? Array.Empty<SkuPolicyOverride>());
        var supplierLimits = new List<SupplierCapacityLimit>(request.Parameters?.SupplierCapacityLimits ?? Array.Empty<SupplierCapacityLimit>());

        foreach (var addition in additions)
        {
            prebuild.AddRange(addition.PrebuildCampaigns ?? Array.Empty<PrebuildCampaign>());
            capacity.AddRange(addition.CapacityAdjustments ?? Array.Empty<ResourceCapacityAdjustment>());
            policies.AddRange(addition.SkuPolicyOverrides ?? Array.Empty<SkuPolicyOverride>());
            supplierLimits.AddRange(addition.SupplierCapacityLimits ?? Array.Empty<SupplierCapacityLimit>());
        }

        return request with
        {
            Parameters = new ScenarioRunParameterSet(prebuild, capacity, policies, supplierLimits)
        };
    }

    private static ScenarioRunPreviewRequest NormalizeBaseRequest(ScenarioRunPreviewRequest request)
    {
        return request with
        {
            HorizonWeeks = Math.Clamp(request.HorizonWeeks <= 0 ? 12 : request.HorizonWeeks, 1, 52),
            AdoptionConstraintMode = string.IsNullOrWhiteSpace(request.AdoptionConstraintMode)
                ? "Balanced"
                : request.AdoptionConstraintMode
        };
    }

    private static IReadOnlyList<OptimizationProfile> SelectProfiles(string? targetMode, int recommendationCount)
    {
        if (!string.IsNullOrWhiteSpace(targetMode))
        {
            var matched = DefaultProfiles.FirstOrDefault(item => item.ProfileId == targetMode);
            if (matched is not null)
            {
                return DefaultProfiles
                    .OrderByDescending(item => item.ProfileId == matched.ProfileId)
                    .Take(recommendationCount)
                    .ToList();
            }
        }

        return DefaultProfiles.Take(recommendationCount).ToList();
    }

    private static decimal Score(
        string profileId,
        ScenarioRunPreviewResult baseline,
        ScenarioRunPreviewResult candidate,
        string actionType)
    {
        var baseMetrics = baseline.Scenario.Metrics;
        var candidateMetrics = candidate.Scenario.Metrics;
        var redSkuReduction = baseMetrics.RedSkuCount - candidateMetrics.RedSkuCount;
        var supplyGapReduction = baseMetrics.SupplyGap - candidateMetrics.SupplyGap;
        var inventoryReduction = baseMetrics.AverageInventoryValue - candidateMetrics.AverageInventoryValue;
        var replenishmentReduction = baseMetrics.ReplenishmentValue - candidateMetrics.ReplenishmentValue;
        var peakReduction = baseMetrics.PeakLoadPercent - candidateMetrics.PeakLoadPercent;
        var capacityGapReduction = baseline.Scenario.Rccp.ConstrainedGap - candidate.Scenario.Rccp.ConstrainedGap;
        var flowGain = candidateMetrics.FlowIndex - baseMetrics.FlowIndex;
        var actionBias = actionType switch
        {
            "供应承诺" => 20m,
            "产能缓冲" => 16m,
            "提前建库" => 12m,
            "订货策略" => 8m,
            _ => 0m
        };

        return profileId switch
        {
            "CashFirst" => inventoryReduction / 100000m + replenishmentReduction / 100000m - Math.Max(0, -redSkuReduction) * 120m + actionBias,
            "CapacityFirst" => peakReduction * 25m + capacityGapReduction / 10m + supplyGapReduction / 100m + actionBias,
            _ => redSkuReduction * 180m + supplyGapReduction / 100m + flowGain * 15m + peakReduction * 8m + actionBias
        };
    }

    private static OptimizationCandidateImpact BuildCandidateImpact(
        string candidateId,
        OptimizationCandidateSeed seed,
        ScenarioRunPreviewResult baseline,
        ScenarioRunPreviewResult candidate,
        decimal estimatedCost)
    {
        var baseMetrics = baseline.Scenario.Metrics;
        var candidateMetrics = candidate.Scenario.Metrics;
        var inventoryImpact = candidateMetrics.AverageInventoryValue - baseMetrics.AverageInventoryValue;
        var supplyImpact = candidateMetrics.SupplyGap - baseMetrics.SupplyGap;
        var peakImpact = candidateMetrics.PeakLoadPercent - baseMetrics.PeakLoadPercent;
        var serviceImpact = candidateMetrics.ServiceLevelPercent - baseMetrics.ServiceLevelPercent;
        var orderImpact = candidateMetrics.ReplenishmentOrderCount - baseMetrics.ReplenishmentOrderCount;
        var feasibilityStatus = BuildFeasibilityStatus(seed.ActionType, inventoryImpact, supplyImpact, peakImpact, baseMetrics.AverageInventoryValue);

        return new OptimizationCandidateImpact(
            candidateId,
            seed.ActionType,
            seed.Target,
            decimal.Round(serviceImpact, 2),
            decimal.Round(inventoryImpact, 0),
            decimal.Round(peakImpact, 2),
            decimal.Round(supplyImpact, 0),
            orderImpact,
            decimal.Round(estimatedCost, 0),
            BuildCostBasis(seed.ActionType),
            BuildConstraintNote(seed.ActionType),
            feasibilityStatus);
    }

    private static ScenarioOptimizationComparison BuildScenarioOptimizationComparison(
        OptimizationProfile profile,
        ScenarioRunPreviewResult baseline,
        ScenarioRunPreviewResult preview,
        IReadOnlyList<ScenarioOptimizationAction> actions)
    {
        var baseMetrics = baseline.Scenario.Metrics;
        var metrics = preview.Scenario.Metrics;
        var estimatedCost = actions.Sum(action => action.EstimatedCost);
        var supplyGapDelta = metrics.SupplyGap - baseMetrics.SupplyGap;
        var peakDelta = metrics.PeakLoadPercent - baseMetrics.PeakLoadPercent;
        var inventoryDelta = metrics.AverageInventoryValue - baseMetrics.AverageInventoryValue;

        return new ScenarioOptimizationComparison(
            profile.ProfileId,
            profile.Name,
            decimal.Round(metrics.ServiceLevelPercent - baseMetrics.ServiceLevelPercent, 2),
            decimal.Round(metrics.FlowIndex - baseMetrics.FlowIndex, 2),
            decimal.Round(inventoryDelta, 0),
            decimal.Round(peakDelta, 2),
            metrics.RedSkuCount - baseMetrics.RedSkuCount,
            decimal.Round(supplyGapDelta, 0),
            metrics.ReplenishmentOrderCount - baseMetrics.ReplenishmentOrderCount,
            decimal.Round(estimatedCost, 0),
            BuildManagementDecision(preview, estimatedCost, inventoryDelta));
    }

    private static decimal EstimateActionCost(
        OptimizationCandidateSeed seed,
        ScenarioRunPreviewResult baseline,
        ScenarioRunPreviewResult candidate)
    {
        var metrics = candidate.Scenario.Metrics;
        var baseMetrics = baseline.Scenario.Metrics;
        var inventoryIncrease = Math.Max(0m, metrics.AverageInventoryValue - baseMetrics.AverageInventoryValue);
        var supplyGapReduction = Math.Max(0m, baseMetrics.SupplyGap - metrics.SupplyGap);
        var capacityGapReduction = Math.Max(0m, baseline.Scenario.Rccp.ConstrainedGap - candidate.Scenario.Rccp.ConstrainedGap);

        return seed.ActionType switch
        {
            "提前建库" => Math.Max(20_000m, inventoryIncrease * 0.08m),
            "产能缓冲" => Math.Max(60_000m, capacityGapReduction * 900m),
            "供应承诺" => Math.Max(50_000m, supplyGapReduction * 3_000m),
            "订货策略" => 30_000m,
            _ => 10_000m
        };
    }

    private static string BuildCostBasis(string actionType)
    {
        return actionType switch
        {
            "提前建库" => "按新增平均库存资金占用估算",
            "产能缓冲" => "按能力缺口改善折算加班/外协成本",
            "供应承诺" => "按供应缺口改善折算供应协调成本",
            "订货策略" => "按补货频率调整的一次性治理成本",
            _ => "按候选动作治理成本估算"
        };
    }

    private static string BuildConstraintNote(string actionType)
    {
        return actionType switch
        {
            "提前建库" => "受库存空间、现金上限和提前释放窗口约束",
            "产能缓冲" => "受最大加班能力、资源日历和 DDOM 日能力约束",
            "供应承诺" => "受供应商最大承诺、采购提前期和替代料策略约束",
            "订货策略" => "受主设置治理、MOQ、订货周期和补货频率约束",
            _ => "受管理授权与执行窗口约束"
        };
    }

    private static string BuildFeasibilityStatus(
        string actionType,
        decimal inventoryImpact,
        decimal supplyGapImpact,
        decimal peakLoadImpact,
        decimal baselineInventory)
    {
        if (inventoryImpact > baselineInventory * 0.2m || supplyGapImpact > 0m)
        {
            return "需要管理取舍";
        }

        if (actionType == "产能缓冲" && peakLoadImpact > 0m)
        {
            return "需复核资源窗口";
        }

        if (inventoryImpact > baselineInventory * 0.1m)
        {
            return "需复核现金边界";
        }

        return "可进入方案评审";
    }

    private static string BuildManagementDecision(
        ScenarioRunPreviewResult preview,
        decimal estimatedCost,
        decimal inventoryDelta)
    {
        if (preview.Trace.Any(item => item.Severity == "Critical") || preview.Scenario.Metrics.SupplyGap > 0m)
        {
            return "需要管理取舍";
        }

        if (estimatedCost > 1_000_000m || inventoryDelta > 1_000_000m)
        {
            return "需复核现金边界";
        }

        if (preview.Scenario.Metrics.RedSkuCount > 0 || preview.Scenario.Rccp.RedResourceCount > 0)
        {
            return "需专项评审";
        }

        return "可进入方案评审";
    }

    private static string BuildRecommendationSummary(
        OptimizationProfile profile,
        ScenarioRunPreviewResult baseline,
        ScenarioRunPreviewResult preview,
        IReadOnlyList<ScenarioOptimizationAction> actions)
    {
        if (actions.Count == 0)
        {
            return $"{profile.Name}：未选择动作，当前候选组合没有明显收益。";
        }

        var comparison = preview.Scenario.Metrics;
        var baseMetrics = baseline.Scenario.Metrics;
        return $"{profile.Name}：选择 {actions.Count} 个动作，红区 SKU 变化 {comparison.RedSkuCount - baseMetrics.RedSkuCount}，峰值负荷变化 {comparison.PeakLoadPercent - baseMetrics.PeakLoadPercent:0.#}pp，供应缺口变化 {comparison.SupplyGap - baseMetrics.SupplyGap:0}。";
    }

    private static IReadOnlyList<ScenarioAuditTrace> BuildRecommendationTrace(
        OptimizationProfile profile,
        OptimizationSolution solution,
        IReadOnlyList<ScenarioOptimizationAction> actions)
    {
        return new List<ScenarioAuditTrace>
        {
            new(solution.SolverName, $"{profile.Name} 使用 {solution.SolverName} 选择候选动作组合。", "Information"),
            new(solution.SolverName, $"求解状态：{solution.Status}；目标值：{solution.ObjectiveValue:0.###}。", solution.Status is OptimizationSolverStatus.Optimal or OptimizationSolverStatus.Feasible ? "Information" : "Warning"),
            new("Action", actions.Count == 0 ? "没有动作被选中。" : $"选中动作：{string.Join("；", actions.Select(item => $"{item.ActionType}-{item.Target}"))}。", actions.Count == 0 ? "Warning" : "Information"),
            new("ImpactMatrix", actions.Count == 0 ? "没有选中动作，因此无需组合影响复核。" : $"选中动作估算成本 {actions.Sum(item => item.EstimatedCost):0}；每个动作的服务、库存、负荷、供应和成本来自候选动作影响矩阵。", "Information"),
            new("Boundary", "推荐方案未自动采纳、未保存、未提交审批；用户仍需运行预览并决定是否保存。", "Information")
        };
    }

    private sealed record OptimizationProfile(string ProfileId, string Name);

    private sealed record OptimizationCandidateSeed(
        string CandidateId,
        string ActionType,
        string Target,
        string ConflictKey,
        ScenarioRunParameterSet Parameters,
        string Explanation);
}
