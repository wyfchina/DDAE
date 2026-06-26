using AdaptiveSopDdsop.NetworkStructure;

using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.NetworkStructureIntegration;

public sealed class CandidateActionCombinationService
{
    private static readonly CombinationProfile[] DefaultProfiles =
    [
        new("ServiceFirst", "服务优先"),
        new("CashFirst", "库存资金优先"),
        new("CapacityFirst", "产能平衡优先")
    ];

    private readonly NetworkScenarioValidationService _validationService;
    private readonly IDdsopWhiteBoxScenarioGateway _whiteBoxGateway;
    private readonly CandidateActionCombinationSelector _selector;

    public CandidateActionCombinationService(
        NetworkScenarioValidationService validationService,
        IDdsopWhiteBoxScenarioGateway whiteBoxGateway,
        CandidateActionCombinationSelector selector)
    {
        _validationService = validationService;
        _whiteBoxGateway = whiteBoxGateway;
        _selector = selector;
    }

    public CandidateActionCombinationResult Select(CandidateActionCombinationRequest request)
    {
        var horizon = Math.Clamp(request.HorizonWeeks <= 0 ? 12 : request.HorizonWeeks, 1, 52);
        var maxActions = Math.Clamp(request.MaxActionsPerCombination <= 0 ? 3 : request.MaxActionsPerCombination, 1, 5);
        var combinationCount = Math.Clamp(request.CombinationCount <= 0 ? 3 : request.CombinationCount, 1, 3);
        var baseline = _whiteBoxGateway.Recalculate(new ScenarioRunPreviewRequest(horizon));
        var validation = _validationService.Validate(horizon);
        var allowedIds = request.CandidateIds is null || request.CandidateIds.Count == 0
            ? null
            : request.CandidateIds.ToHashSet(StringComparer.Ordinal);
        var validations = validation.Validations
            .Where(item => allowedIds is null || allowedIds.Contains(item.CandidateId))
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .Take(12)
            .ToList();
        var profiles = SelectProfiles(request.TargetMode, combinationCount);
        var combinations = new List<CandidateActionCombination>();
        var comparisons = new List<CombinationComparison>();
        var impactMatrix = new List<OptimizationCandidateImpact>();
        var solverName = request.SolverName ?? "Gurobi";
        var trace = new List<ScenarioAuditTrace>
        {
            new("CandidateCombination", $"读取 {validations.Count} 个已验证候选动作，求解器只选择候选动作组合，不生成计划。", "Information"),
            new("CandidateCombination", "组合被选中后必须回到 Scenario Preview 白盒引擎重新计算库存、RCCP、供应缺口和约束结果。", "Information")
        };

        foreach (var profile in profiles)
        {
            var candidates = validations
                .Select(item => BuildCandidate(profile, item))
                .Where(candidate => candidate.SolverCandidate.ObjectiveValue > 0m)
                .OrderByDescending(candidate => candidate.SolverCandidate.ObjectiveValue)
                .Take(12)
                .ToList();
            impactMatrix.AddRange(candidates.Select(candidate => candidate.Impact));

            var selection = _selector.Select(new CandidateActionSelectionProfile(
                profile.ProfileId,
                profile.ProfileName,
                maxActions,
                Math.Max(0m, baseline.Scenario.Metrics.AverageInventoryValue * 0.20m),
                Math.Max(250_000m, baseline.Scenario.Metrics.AverageInventoryValue * 0.08m),
                candidates.Select(candidate => candidate.SolverCandidate).ToList()),
                request.SolverName);
            solverName = selection.SolverName;

            if (selection.Status is OptimizationSolverStatus.Unavailable or OptimizationSolverStatus.Error)
            {
                trace.Add(new("CandidateCombination", selection.Message, "Warning"));
                return new CandidateActionCombinationResult(
                    selection.Status,
                    selection.SolverName,
                    selection.Message,
                    impactMatrix,
                    comparisons,
                    Array.Empty<CandidateActionCombination>(),
                    trace,
                    IsPersisted: false);
            }

            var selected = candidates
                .Where(candidate => selection.SelectedCandidateIds.Contains(candidate.SolverCandidate.CandidateId, StringComparer.Ordinal))
                .ToList();
            var previewRequest = MergeCandidateParameters(horizon, selected);
            var preview = _whiteBoxGateway.Recalculate(previewRequest);
            var selectedActions = selected
                .Select(candidate =>
                {
                    return new SelectedCandidateAction(
                        candidate.SolverCandidate.CandidateId,
                        candidate.SolverCandidate.ActionType,
                        candidate.SolverCandidate.Target,
                        candidate.SolverCandidate.EstimatedCost,
                        candidate.Impact,
                        candidate.SolverCandidate.Explanation,
                        candidate.Validation.Evidence);
                })
                .ToList();
            var comparison = BuildComparison(profile, baseline, preview, selectedActions);
            comparisons.Add(comparison);

            combinations.Add(new CandidateActionCombination(
                profile.ProfileId,
                profile.ProfileName,
                BuildSummary(profile, selectedActions, comparison),
                selection.Status,
                selection.Message,
                selection.ObjectiveValue,
                previewRequest,
                preview,
                comparison,
                selectedActions.Sum(action => action.EstimatedCost),
                selectedActions,
                BuildTrace(selection, selectedActions)));
        }

        var status = combinations.Any(item => item.SolverStatus == OptimizationSolverStatus.Optimal)
            ? OptimizationSolverStatus.Optimal
            : combinations.Any(item => item.SolverStatus == OptimizationSolverStatus.Feasible)
                ? OptimizationSolverStatus.Feasible
                : OptimizationSolverStatus.Infeasible;
        trace.Add(new("CandidateCombination", $"返回 {combinations.Count} 个候选动作组合；结果未保存、未审批、未写回 DDOM。", "Information"));

        return new CandidateActionCombinationResult(
            status,
            solverName,
            combinations.Count == 0 ? "没有生成可行的候选动作组合。" : "候选动作组合已生成。",
            impactMatrix
                .GroupBy(item => item.CandidateId, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToList(),
            comparisons,
            combinations,
            trace,
            IsPersisted: false);
    }

    private static IReadOnlyList<CombinationProfile> SelectProfiles(string? targetMode, int count)
    {
        if (!string.IsNullOrWhiteSpace(targetMode))
        {
            var matched = DefaultProfiles.FirstOrDefault(item => item.ProfileId.Equals(targetMode, StringComparison.OrdinalIgnoreCase));
            if (matched is not null)
            {
                return [matched];
            }
        }

        return DefaultProfiles.Take(count).ToList();
    }

    private static CandidateActionDraft BuildCandidate(CombinationProfile profile, NetworkScenarioValidationItem validation)
    {
        var impact = new OptimizationCandidateImpact(
            CandidateId(profile, validation),
            validation.RecommendedSettingType,
            validation.Target,
            ServiceImpact(validation),
            validation.AverageInventoryValueDelta,
            validation.RccpPeakLoadDelta,
            validation.SupplyGapDelta,
            validation.ReplenishmentOrderCountDelta,
            EstimateCost(validation),
            CostBasis(validation),
            ConstraintNote(validation),
            FeasibilityStatus(validation));
        var objective = ProfileObjective(profile.ProfileId, validation, impact);

        var solverCandidate = new OptimizationCandidate(
            impact.CandidateId,
            impact.ActionType,
            impact.Target,
            ConflictKey(validation),
            Math.Max(0.001m, decimal.Round(objective - impact.EstimatedCost / 100000m, 3)),
            Math.Max(0m, validation.AverageInventoryValueDelta),
            impact.EstimatedCost,
            impact.ConstraintNote,
            impact.FeasibilityStatus,
            validation.ValidationSummary);

        return new CandidateActionDraft(
            solverCandidate,
            impact,
            validation.WhiteBoxRecalculationRequest?.Parameters ?? new ScenarioRunParameterSet(),
            validation);
    }

    private static ScenarioRunPreviewRequest MergeCandidateParameters(int horizon, IReadOnlyList<CandidateActionDraft> selected)
    {
        return new ScenarioRunPreviewRequest(
            horizon,
            TemplateId: "TPL-CANDIDATE-COMBO",
            Parameters: new ScenarioRunParameterSet(
                selected.SelectMany(candidate => candidate.Parameters.PrebuildCampaigns ?? Array.Empty<PrebuildCampaign>()).ToList(),
                selected.SelectMany(candidate => candidate.Parameters.CapacityAdjustments ?? Array.Empty<ResourceCapacityAdjustment>()).ToList(),
                selected.SelectMany(candidate => candidate.Parameters.SkuPolicyOverrides ?? Array.Empty<SkuPolicyOverride>()).ToList(),
                selected.SelectMany(candidate => candidate.Parameters.SupplierCapacityLimits ?? Array.Empty<SupplierCapacityLimit>()).ToList()),
            AdoptionConstraintMode: "CandidateCombination");
    }

    private static CombinationComparison BuildComparison(
        CombinationProfile profile,
        ScenarioRunPreviewResult baseline,
        ScenarioRunPreviewResult preview,
        IReadOnlyList<SelectedCandidateAction> actions)
    {
        return new CombinationComparison(
            profile.ProfileId,
            profile.ProfileName,
            preview.Comparison.ServiceLevelDelta,
            preview.Comparison.FlowIndexDelta,
            preview.Comparison.AverageInventoryValueDelta,
            preview.Comparison.PeakLoadPercentDelta,
            preview.Comparison.RedSkuCountDelta,
            preview.Comparison.SupplyGapDelta,
            preview.Comparison.ReplenishmentOrderCountDelta,
            actions.Sum(action => action.EstimatedCost),
            ManagementDecision(profile, baseline, preview, actions));
    }

    private static IReadOnlyList<ScenarioAuditTrace> BuildTrace(
        CandidateActionSelectionResult selection,
        IReadOnlyList<SelectedCandidateAction> actions)
    {
        return new[]
        {
            new ScenarioAuditTrace("CandidateCombination", $"{selection.ProfileName} 使用 {selection.SolverName}，状态 {selection.Status}。", "Information"),
            new ScenarioAuditTrace("CandidateCombination", actions.Count == 0 ? "求解器未选择正收益候选动作。" : $"选中 {actions.Count} 个候选动作：{string.Join("，", actions.Select(item => item.Target))}。", "Information"),
            new ScenarioAuditTrace("WhiteBoxRecalculation", "已用选中动作重新运行白盒 Scenario Preview，求解器结果没有直接写入计划。", "Information")
        };
    }

    private static string BuildSummary(
        CombinationProfile profile,
        IReadOnlyList<SelectedCandidateAction> actions,
        CombinationComparison comparison)
    {
        return $"{profile.ProfileName}：选择 {actions.Count} 个动作，红区 SKU 变化 {comparison.RedSkuCountDelta:+#;-#;0}，峰值负荷变化 {comparison.PeakLoadPercentDelta:+0.#;-0.#;0}pp，供应缺口变化 {comparison.SupplyGapDelta:+#;-#;0}。";
    }

    private static decimal ProfileObjective(string profileId, NetworkScenarioValidationItem validation, OptimizationCandidateImpact impact)
    {
        return profileId switch
        {
            "ServiceFirst" => validation.Score * 0.35m + ServiceImpact(validation) * 0.40m + Math.Max(0m, -validation.SupplyGapDelta) / 100m + Math.Max(0, -validation.RedWeekDelta) * 8m,
            "CashFirst" => validation.Score * 0.30m + Math.Max(0m, -validation.AverageInventoryValueDelta) / 50000m + Math.Max(0, -validation.ReplenishmentOrderCountDelta) * 4m,
            "CapacityFirst" => validation.Score * 0.30m + Math.Max(0m, -validation.RccpPeakLoadDelta) * 2m + Math.Max(0, -validation.RccpRedWeekDelta) * 8m,
            _ => validation.Score * 0.30m + impact.ServiceImpactPercent * 0.25m,
        };
    }

    private static decimal ServiceImpact(NetworkScenarioValidationItem validation)
    {
        return decimal.Round(Math.Max(0m, -validation.RedWeekDelta) * 0.7m + Math.Max(0m, -validation.SupplyGapDelta) / 1000m + Math.Max(0m, -validation.RccpPeakLoadDelta) * 0.05m, 2);
    }

    private static decimal EstimateCost(NetworkScenarioValidationItem validation)
    {
        var inventoryCost = Math.Max(0m, validation.AverageInventoryValueDelta) * 0.08m;
        var capacityCost = Math.Max(0m, -validation.RccpPeakLoadDelta) * 4000m;
        var supplyCost = Math.Max(0m, -validation.SupplyGapDelta) * 250m;
        var policyCost = validation.RecommendedSettingType is "库存缓冲" or "解耦点" ? 30_000m : 0m;
        return decimal.Round(Math.Max(12_000m, inventoryCost + capacityCost + supplyCost + policyCost), 0);
    }

    private static string CostBasis(NetworkScenarioValidationItem validation)
    {
        return $"基于库存变化 {validation.AverageInventoryValueDelta:0}、RCCP 峰值变化 {validation.RccpPeakLoadDelta:+0.#;-0.#;0}pp、供应缺口变化 {validation.SupplyGapDelta:+#;-#;0} 估算。";
    }

    private static string ConstraintNote(NetworkScenarioValidationItem validation)
    {
        return validation.Severity == "Red"
            ? "红色候选必须经过管理层取舍，组合选择器只能建议。"
            : "黄色候选可进入 DDS&OP 方案比较。";
    }

    private static string FeasibilityStatus(NetworkScenarioValidationItem validation)
    {
        return validation.AverageInventoryValueDelta > 0m && validation.SupplyGapDelta > 0m
            ? "需要专项评审"
            : "可进入白盒重算";
    }

    private static string ConflictKey(NetworkScenarioValidationItem validation)
    {
        return $"{validation.RecommendedSettingType}:{validation.Target}";
    }

    private static string CandidateId(CombinationProfile profile, NetworkScenarioValidationItem validation)
    {
        return $"{profile.ProfileId}-{validation.CandidateId}";
    }

    private static string ManagementDecision(
        CombinationProfile profile,
        ScenarioRunPreviewResult baseline,
        ScenarioRunPreviewResult preview,
        IReadOnlyList<SelectedCandidateAction> actions)
    {
        if (actions.Count == 0)
        {
            return "没有正收益动作，保持当前方案并升级人工评审。";
        }

        if (preview.Scenario.Metrics.SupplyGap > baseline.Scenario.Metrics.SupplyGap)
        {
            return "供应缺口扩大，需要管理取舍或供应专项协调。";
        }

        return profile.ProfileId switch
        {
            "ServiceFirst" => "服务优先组合可进入方案比较，但需确认库存资金影响。",
            "CashFirst" => "库存资金优先组合可进入方案比较，但需确认服务与供应风险。",
            "CapacityFirst" => "产能平衡优先组合可进入方案比较，但需确认客户交付承诺。",
            _ => "组合可进入方案比较。"
        };
    }

    private sealed record CombinationProfile(string ProfileId, string ProfileName);

    private sealed record CandidateActionDraft(
        OptimizationCandidate SolverCandidate,
        OptimizationCandidateImpact Impact,
        ScenarioRunParameterSet Parameters,
        NetworkScenarioValidationItem Validation);
}
