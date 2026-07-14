namespace AdaptiveSopDdsop.Web.Domain;

public sealed class ScenarioRunPreviewService
{
    private readonly IScenarioWorkspaceDataSource _dataSource;

    public ScenarioRunPreviewService(IScenarioWorkspaceDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public ScenarioRunPreviewResult Preview(ScenarioRunPreviewRequest request) => PreviewInternal(request, null);

    public ScenarioRunPreviewResult PreviewAgainstFrozenBaseline(
        ScenarioRunPreviewRequest request,
        CurrentBaselineSnapshot frozenBaseline) => PreviewInternal(request, frozenBaseline);

    public ScenarioWorkspaceDataSet LoadFrozenWorkspaceData(
        ScenarioRunPreviewRequest request,
        CurrentBaselineSnapshot frozenBaseline)
    {
        var horizonWeeks = Math.Clamp(request.HorizonWeeks <= 0 ? 12 : request.HorizonWeeks, 1, 52);
        var planningInputs = frozenBaseline.Payload.PlanningInputs
            ?? throw new InvalidOperationException("该冻结基线不含完整类型化计划输入，不能用于可复现重算；请从当前候选生成新版本。");
        var scoped = ScopeFrozenPlanningInputs(
            planningInputs,
            new ScenarioWorkspaceDataRequest(
                horizonWeeks,
                planningInputs.Request.AnchorDate,
                request.SkuFilter,
                request.FamilyFilter));
        return ApplyFrozenBaseline(scoped, frozenBaseline);
    }

    private ScenarioRunPreviewResult PreviewInternal(
        ScenarioRunPreviewRequest request,
        CurrentBaselineSnapshot? frozenBaseline)
    {
        var horizonWeeks = Math.Clamp(request.HorizonWeeks <= 0 ? 12 : request.HorizonWeeks, 1, 52);
        var data = frozenBaseline is null
            ? _dataSource.Load(new ScenarioWorkspaceDataRequest(
                horizonWeeks,
                new DateOnly(2026, 6, 1),
                request.SkuFilter,
                request.FamilyFilter))
            : LoadFrozenWorkspaceData(request with { HorizonWeeks = horizonWeeks }, frozenBaseline);
        var parameters = MergeTemplateParameters(data, request.TemplateId, request.Parameters);

        var baseline = BuildCase("baseline", "基准方案", data, data.Skus, data.Demand, Array.Empty<PrebuildCampaign>(), Array.Empty<ResourceCapacityAdjustment>(), Array.Empty<SupplierCapacityLimit>());
        var scenarioSkus = ApplySkuPolicyOverrides(data.Skus, parameters.SkuPolicyOverrides ?? Array.Empty<SkuPolicyOverride>());
        var scenarioDemand = ApplyExternalDemandChanges(
            ApplyDemandEvents(data, request.TemplateId),
            data.Skus,
            request.ExternalScenario?.DemandChanges ?? Array.Empty<ExternalDemandChange>());
        var capacityAdjustments = BuildExternalCapacityAdjustments(request.ExternalScenario)
            .Concat(parameters.CapacityAdjustments ?? Array.Empty<ResourceCapacityAdjustment>())
            .ToList();
        var supplierCapacityLimits = BuildExternalSupplierLimits(data, request.ExternalScenario)
            .Concat(parameters.SupplierCapacityLimits ?? Array.Empty<SupplierCapacityLimit>())
            .ToList();
        var scenario = BuildCase(
            "scenario",
            request.ExternalScenario?.Name ?? ScenarioName(data, request.TemplateId),
            data,
            scenarioSkus,
            scenarioDemand,
            parameters.PrebuildCampaigns ?? Array.Empty<PrebuildCampaign>(),
            capacityAdjustments,
            supplierCapacityLimits);

        var bufferTrendComparison = BufferTrendWorkspaceService.Compare(baseline.BufferTrend, scenario.BufferTrend);
        scenario = scenario with
        {
            ProductFamilyDashboard = ProductFamilyDashboardService.WithComparison(
                scenario.ProductFamilyDashboard,
                ProductFamilyDashboardService.Compare(baseline.ProductFamilyDashboard, scenario.ProductFamilyDashboard)),
            BufferTrend = BufferTrendWorkspaceService.WithComparison(scenario.BufferTrend, bufferTrendComparison)
        };
        var trace = BuildAuditTrace(data, request, parameters, baseline, scenario).ToList();
        if (frozenBaseline is not null)
        {
            trace.Insert(0, new ScenarioAuditTrace(
                "FrozenBaseline",
                $"使用不可变基线 {frozenBaseline.SnapshotNumber}（截止 {frozenBaseline.AsOfUtc}，主设置 {frozenBaseline.MasterSettingVersion}）重算；未重新读取实时库存、能力或供应承诺。",
                "Information"));
        }

        return new ScenarioRunPreviewResult(
            request with { HorizonWeeks = horizonWeeks, Parameters = parameters },
            baseline,
            scenario,
            Compare(baseline.Metrics, scenario.Metrics),
            RccpWorkspaceService.Compare(baseline.Rccp, scenario.Rccp),
            trace,
            IsPersisted: false);
    }

    private static ScenarioWorkspaceDataSet ApplyFrozenBaseline(
        ScenarioWorkspaceDataSet data,
        CurrentBaselineSnapshot frozenBaseline)
    {
        var inventory = frozenBaseline.Payload.Inventory
            .Where(item => data.Skus.Any(sku => sku.Sku == item.Sku))
            .ToList();
        var missingInventory = data.Skus
            .Where(sku => inventory.All(item => item.Sku != sku.Sku))
            .Select(sku => sku.Sku)
            .ToList();
        if (missingInventory.Count > 0)
        {
            throw new ArgumentException($"冻结基线缺少库存证据：{string.Join("、", missingInventory)}。", nameof(frozenBaseline));
        }

        var availability = frozenBaseline.Payload.ResourceAvailability
            .ToDictionary(item => item.ResourceCode, StringComparer.Ordinal);
        var resources = data.Resources.Select(resource => availability.TryGetValue(resource.Code, out var frozen)
            ? resource with { WeeklyAvailableUnits = frozen.AvailableCapacity }
            : resource).ToList();
        var missingResources = data.Resources
            .Where(resource => !availability.ContainsKey(resource.Code))
            .Select(resource => resource.Code)
            .ToList();
        if (missingResources.Count > 0)
        {
            throw new ArgumentException($"冻结基线缺少资源能力证据：{string.Join("、", missingResources)}。", nameof(frozenBaseline));
        }

        var supplierWindows = frozenBaseline.Payload.SupplierCommitments
            .SelectMany(commitment => Enumerable.Range(1, data.Request.HorizonWeeks)
                .Select(week => new SupplierCapacityWindow(
                    commitment.Supplier,
                    commitment.MaterialFamily,
                    week,
                    commitment.Quantity,
                    commitment.LeadTimeDays,
                    commitment.RiskStatus)))
            .ToList();
        if (supplierWindows.Count == 0)
        {
            throw new ArgumentException("冻结基线缺少供应承诺证据。", nameof(frozenBaseline));
        }

        return data with
        {
            Inventory = inventory,
            Resources = resources,
            SupplierCapacityWindows = supplierWindows,
            MasterSettings = frozenBaseline.Payload.MasterSettings
        };
    }

    private static ScenarioWorkspaceDataSet ScopeFrozenPlanningInputs(
        ScenarioWorkspaceDataSet data,
        ScenarioWorkspaceDataRequest request)
    {
        var skus = data.Skus
            .Where(item => request.SkuFilter is not { Count: > 0 } || request.SkuFilter.Contains(item.Sku, StringComparer.Ordinal))
            .Where(item => request.FamilyFilter is not { Count: > 0 } || request.FamilyFilter.Contains(item.Family, StringComparer.Ordinal))
            .ToList();
        var skuCodes = skus.Select(item => item.Sku).ToHashSet(StringComparer.Ordinal);
        var familyCodes = skus.Select(item => item.Family).ToHashSet(StringComparer.Ordinal);
        var routings = data.ResourceRoutings.Where(item => skuCodes.Contains(item.Sku)).ToList();
        var resourceCodes = routings.Select(item => item.ResourceCode).ToHashSet(StringComparer.Ordinal);
        var sources = data.SupplierItemSources.Where(item => skuCodes.Contains(item.Sku)).ToList();
        var sourceKeys = sources.Select(item => $"{item.Supplier}|{item.MaterialFamily}").ToHashSet(StringComparer.Ordinal);

        return data with
        {
            Request = request,
            Families = data.Families.Where(item => familyCodes.Contains(item.Code)).ToList(),
            Skus = skus,
            Inventory = data.Inventory.Where(item => skuCodes.Contains(item.Sku)).ToList(),
            Demand = data.Demand.Where(item => skuCodes.Contains(item.Sku) && item.Week <= request.HorizonWeeks).ToList(),
            Resources = data.Resources.Where(item => resourceCodes.Contains(item.Code)).ToList(),
            ResourceRoutings = routings,
            SupplierItemSources = sources,
            HistoricalDemand = data.HistoricalDemand.Where(item => skuCodes.Contains(item.Sku)).ToList(),
            BudgetBenchmarks = data.BudgetBenchmarks.Where(item => familyCodes.Contains(item.Family) && item.Week <= request.HorizonWeeks).ToList(),
            ResourceCalendar = data.ResourceCalendar.Where(item => resourceCodes.Contains(item.ResourceCode) && item.Week <= request.HorizonWeeks).ToList(),
            SupplierCapacityWindows = data.SupplierCapacityWindows
                .Where(item => sourceKeys.Contains($"{item.Supplier}|{item.MaterialFamily}") && item.Week <= request.HorizonWeeks)
                .ToList(),
            DdmrpParameters = data.DdmrpParameters.Where(item => skuCodes.Contains(item.Sku)).ToList(),
            CapacityProtections = data.CapacityProtections,
            TimeBuffers = data.TimeBuffers,
            ControlPointProgress = data.ControlPointProgress,
            TimeBufferProductScopes = data.TimeBufferProductScopes
        };
    }

    private static ScenarioRunPreviewCase BuildCase(
        string caseId,
        string name,
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<SkuBufferSetting> skus,
        IReadOnlyList<WeeklyDemand> demand,
        IReadOnlyList<PrebuildCampaign> prebuildCampaigns,
        IReadOnlyList<ResourceCapacityAdjustment> capacityAdjustments,
        IReadOnlyList<SupplierCapacityLimit> supplierCapacityLimits)
    {
        var bufferRun = DemandDrivenPlanningEngine.ProjectBuffers(
            skus,
            data.Inventory,
            demand,
            data.Request.HorizonWeeks,
            prebuildCampaigns);
        var capacityLoads = DemandDrivenPlanningEngine.ProjectRoughCutCapacity(
            bufferRun.ReplenishmentOrders,
            data.ResourceRoutings,
            data.Resources,
            data.Request.HorizonWeeks,
            capacityAdjustments);
        var supplyRequirements = DemandDrivenPlanningEngine.ProjectSupplyRequirements(
            bufferRun.ReplenishmentOrders,
            data.SupplierItemSources);
        var plan = new DemandDrivenPlanResult(
            bufferRun.BufferProjections,
            bufferRun.ReplenishmentOrders,
            capacityLoads,
            supplyRequirements,
            bufferRun.Traces);
        var supplierCapacity = ConstraintWorkspaceService.CompareSupplierCapacity(data.SupplierCapacityWindows, supplyRequirements, supplierCapacityLimits);
        var budget = CompareBudget(data, skus, bufferRun.BufferProjections);
        var bufferTrend = BufferTrendWorkspaceService.Build(data, caseId, name, skus, plan);
        var productFamilyDashboard = ProductFamilyDashboardService.Build(
            data,
            caseId,
            $"{name} 产品族看板",
            skus,
            plan,
            supplierCapacity,
            budget);
        var rccp = RccpWorkspaceService.Build(data, caseId, $"{name} RCCP", plan);
        var constraints = ConstraintWorkspaceService.Build(data, caseId, $"{name} 受限 / 不受限", plan, supplierCapacity);
        var supplierCollaboration = SupplierCollaborationWorkspaceService.Build(
            data,
            caseId,
            $"{name} 供应商需求钻取",
            bufferRun.ReplenishmentOrders,
            supplierCapacity);

        return new ScenarioRunPreviewCase(
            caseId,
            name,
            plan,
            CalculateMetrics(data, skus, bufferRun.BufferProjections, bufferRun.ReplenishmentOrders, capacityLoads, supplierCapacity),
            productFamilyDashboard,
            bufferTrend,
            rccp,
            constraints,
            supplierCollaboration,
            supplierCapacity,
            budget);
    }

    private static ScenarioRunParameterSet MergeTemplateParameters(
        ScenarioWorkspaceDataSet data,
        string? templateId,
        ScenarioRunParameterSet? requestParameters)
    {
        var template = data.ScenarioTemplates.FirstOrDefault(item => item.TemplateId == templateId);
        var prebuild = new List<PrebuildCampaign>(requestParameters?.PrebuildCampaigns ?? Array.Empty<PrebuildCampaign>());
        var capacity = new List<ResourceCapacityAdjustment>(requestParameters?.CapacityAdjustments ?? Array.Empty<ResourceCapacityAdjustment>());
        var policies = new List<SkuPolicyOverride>(requestParameters?.SkuPolicyOverrides ?? Array.Empty<SkuPolicyOverride>());
        var supplierLimits = new List<SupplierCapacityLimit>(requestParameters?.SupplierCapacityLimits ?? Array.Empty<SupplierCapacityLimit>());

        if (template is not null)
        {
            foreach (var action in template.Actions)
            {
                if (action.ActionType == "Prebuild" && !prebuild.Any(item => item.Sku == action.Target && item.BuildWeek == action.StartWeek))
                {
                    prebuild.Add(new PrebuildCampaign($"TPL-{template.TemplateId}", action.Target, action.StartWeek, action.StartWeek, action.EndWeek, action.Value));
                }

                if (action.ActionType == "CapacityMultiplier" && !capacity.Any(item => item.ResourceCode == action.Target && item.Week >= action.StartWeek && item.Week <= action.EndWeek))
                {
                    capacity.AddRange(Enumerable.Range(action.StartWeek, action.EndWeek - action.StartWeek + 1)
                        .Select(week => new ResourceCapacityAdjustment(action.Target, week, action.Value, template.Name)));
                }

                if (action.ActionType == "MoqOverride" && !policies.Any(item => item.Sku == action.Target && item.MinimumOrderQuantity.HasValue))
                {
                    policies.Add(new SkuPolicyOverride(action.Target, MinimumOrderQuantity: action.Value));
                }

                if (action.ActionType == "OrderCycleOverride" && !policies.Any(item => item.Sku == action.Target && item.OrderCycleDays.HasValue))
                {
                    policies.Add(new SkuPolicyOverride(action.Target, OrderCycleDays: decimal.ToInt32(decimal.Round(action.Value, 0))));
                }

                if (action.ActionType == "SupplierCapacityLimit" && !supplierLimits.Any(item => item.MaterialFamily == action.Target && item.StartWeek == action.StartWeek))
                {
                    var matchingWindow = data.SupplierCapacityWindows.FirstOrDefault(item => item.MaterialFamily == action.Target);
                    supplierLimits.Add(new SupplierCapacityLimit(matchingWindow?.Supplier ?? "未指定供应商", action.Target, action.StartWeek, action.EndWeek, action.Value));
                }
            }
        }

        return new ScenarioRunParameterSet(prebuild, capacity, policies, supplierLimits);
    }

    private static IReadOnlyList<SkuBufferSetting> ApplySkuPolicyOverrides(
        IReadOnlyList<SkuBufferSetting> skus,
        IReadOnlyList<SkuPolicyOverride> overrides)
    {
        return skus.Select(sku =>
        {
            var matches = overrides.Where(item => item.Sku == sku.Sku).ToList();
            if (matches.Count == 0)
            {
                return sku;
            }

            var moq = matches.LastOrDefault(item => item.MinimumOrderQuantity.HasValue)?.MinimumOrderQuantity ?? sku.MinimumOrderQuantity;
            var cycle = matches.LastOrDefault(item => item.OrderCycleDays.HasValue)?.OrderCycleDays ?? sku.OrderCycleDays;
            return sku with { MinimumOrderQuantity = moq, OrderCycleDays = cycle };
        }).ToList();
    }

    private static IReadOnlyList<WeeklyDemand> ApplyDemandEvents(ScenarioWorkspaceDataSet data, string? templateId)
    {
        var template = data.ScenarioTemplates.FirstOrDefault(item => item.TemplateId == templateId);
        if (template is null || !template.Actions.Any(item => item.ActionType == "DemandEvent"))
        {
            return data.Demand;
        }

        var skuFamilies = data.Skus.ToDictionary(item => item.Sku, item => item.Family, StringComparer.Ordinal);
        var events = template.Actions.Where(item => item.ActionType == "DemandEvent").ToList();
        return data.Demand.Select(point =>
        {
            var factor = events
                .Where(item => point.Week >= item.StartWeek && point.Week <= item.EndWeek)
                .Where(item => item.Target == point.Sku || (skuFamilies.TryGetValue(point.Sku, out var family) && item.Target == family))
                .Select(item => item.Value)
                .DefaultIfEmpty(1m)
                .Aggregate(1m, (current, next) => current * next);
            return point with { BaselineDemand = decimal.Round(point.BaselineDemand * factor, 0) };
        }).ToList();
    }

    private static IReadOnlyList<WeeklyDemand> ApplyExternalDemandChanges(
        IReadOnlyList<WeeklyDemand> demand,
        IReadOnlyList<SkuBufferSetting> skus,
        IReadOnlyList<ExternalDemandChange> changes)
    {
        if (changes.Count == 0)
        {
            return demand;
        }

        var families = skus.ToDictionary(item => item.Sku, item => item.Family, StringComparer.Ordinal);
        return demand.Select(point =>
        {
            var multiplier = changes
                .Where(item => point.Week >= item.StartWeek && point.Week <= item.EndWeek)
                .Where(item =>
                    (string.IsNullOrWhiteSpace(item.Sku) && string.IsNullOrWhiteSpace(item.Family)) ||
                    item.Sku == point.Sku ||
                    (!string.IsNullOrWhiteSpace(item.Family) && families.GetValueOrDefault(point.Sku) == item.Family))
                .Select(item => Math.Max(0m, item.DemandMultiplier))
                .DefaultIfEmpty(1m)
                .Aggregate(1m, (current, next) => current * next);
            return point with { BaselineDemand = decimal.Round(point.BaselineDemand * multiplier, 0) };
        }).ToList();
    }

    private static IReadOnlyList<ResourceCapacityAdjustment> BuildExternalCapacityAdjustments(
        ExternalScenarioDefinition? externalScenario)
    {
        return (externalScenario?.CapacityLosses ?? Array.Empty<ExternalCapacityLoss>())
            .SelectMany(item => Enumerable.Range(
                    Math.Max(1, item.StartWeek),
                    Math.Max(0, item.EndWeek - Math.Max(1, item.StartWeek) + 1))
                .Select(week => new ResourceCapacityAdjustment(
                    item.ResourceCode,
                    week,
                    Math.Max(0m, item.AvailableCapacityMultiplier),
                    $"外部场景：{item.Reason}")))
            .ToList();
    }

    private static IReadOnlyList<SupplierCapacityLimit> BuildExternalSupplierLimits(
        ScenarioWorkspaceDataSet data,
        ExternalScenarioDefinition? externalScenario)
    {
        return (externalScenario?.SupplyRisks ?? Array.Empty<ExternalSupplyRisk>())
            .SelectMany(risk => Enumerable.Range(
                    Math.Max(1, risk.StartWeek),
                    Math.Max(0, risk.EndWeek - Math.Max(1, risk.StartWeek) + 1))
                .Select(week =>
                {
                    var committed = data.SupplierCapacityWindows
                        .Where(item => item.Supplier == risk.Supplier && item.MaterialFamily == risk.MaterialFamily && item.Week == week)
                        .Select(item => item.CommittedCapacity)
                        .DefaultIfEmpty(0m)
                        .Last();
                    return new SupplierCapacityLimit(
                        risk.Supplier,
                        risk.MaterialFamily,
                        week,
                        week,
                        decimal.Round(committed * Math.Max(0m, risk.AvailableCapacityMultiplier), 0));
                }))
            .ToList();
    }

    private static IReadOnlyList<BudgetComparison> CompareBudget(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<SkuBufferSetting> skus,
        IReadOnlyList<BufferProjectionPoint> projections)
    {
        var skuMap = skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);
        return data.BudgetBenchmarks.Select(benchmark =>
        {
            var projectedInventory = projections
                .Where(item => item.Week == benchmark.Week)
                .Where(item => skuMap.TryGetValue(item.Sku, out var sku) && sku.Family == benchmark.Family)
                .Sum(item => item.EndNetFlowAfterReplenishment * skuMap[item.Sku].UnitCost);
            return new BudgetComparison(
                benchmark.Family,
                benchmark.Week,
                benchmark.BudgetRevenue,
                benchmark.LastYearRevenue,
                benchmark.BudgetInventoryValue,
                benchmark.LastYearInventoryValue,
                decimal.Round(projectedInventory, 0),
                decimal.Round(projectedInventory - benchmark.BudgetInventoryValue, 0));
        }).ToList();
    }

    private static ScenarioPreviewMetrics CalculateMetrics(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<SkuBufferSetting> skus,
        IReadOnlyList<BufferProjectionPoint> projections,
        IReadOnlyList<ProjectedReplenishmentOrder> orders,
        IReadOnlyList<CapacityLoadProjection> loads,
        IReadOnlyList<SupplierCapacityComparison> supplierCapacity)
    {
        var service = data.HistoricalDemand.Count == 0 ? 0m : decimal.Round(data.HistoricalDemand.Average(item => item.ServiceLevelPercent), 1);
        var healthyProjectionCount = projections.Count(item => item.BufferStatus is "Green" or "OverTopOfGreen");
        var bufferHealth = projections.Count == 0 ? 0m : decimal.Round(healthyProjectionCount * 100m / projections.Count, 1);
        var averageInventory = projections.Count == 0
            ? 0m
            : projections.Join(skus, point => point.Sku, sku => sku.Sku, (point, sku) => point.EndNetFlowAfterReplenishment * sku.UnitCost).Average();
        var peakLoad = loads.Count == 0 ? 0m : loads.Max(item => item.LoadPercent);
        var averageLoad = loads.Count == 0 ? 0m : decimal.Round(loads.Average(item => item.LoadPercent), 1);
        var flowIndex = CalculateFlowIndex(bufferHealth, averageLoad);
        var redSkuCount = projections.Where(item => item.BufferStatus == "Red").Select(item => item.Sku).Distinct().Count();
        var supplyGap = supplierCapacity.Sum(item => item.Gap);
        var replenishmentValue = orders.Sum(item => item.Value);

        return new ScenarioPreviewMetrics(
            service,
            flowIndex,
            decimal.Round(averageInventory, 0),
            decimal.Round(peakLoad, 1),
            averageLoad,
            redSkuCount,
            decimal.Round(supplyGap, 0),
            decimal.Round(replenishmentValue, 0),
            orders.Count);
    }

    private static ScenarioComparisonMetrics Compare(ScenarioPreviewMetrics baseline, ScenarioPreviewMetrics scenario)
    {
        return new ScenarioComparisonMetrics(
            decimal.Round(scenario.ServiceLevelPercent - baseline.ServiceLevelPercent, 1),
            decimal.Round(scenario.FlowIndex - baseline.FlowIndex, 1),
            scenario.AverageInventoryValue - baseline.AverageInventoryValue,
            scenario.PeakLoadPercent - baseline.PeakLoadPercent,
            scenario.AverageLoadPercent - baseline.AverageLoadPercent,
            scenario.RedSkuCount - baseline.RedSkuCount,
            scenario.SupplyGap - baseline.SupplyGap,
            scenario.ReplenishmentValue - baseline.ReplenishmentValue,
            scenario.ReplenishmentOrderCount - baseline.ReplenishmentOrderCount);
    }

    private static IReadOnlyList<ScenarioAuditTrace> BuildAuditTrace(
        ScenarioWorkspaceDataSet data,
        ScenarioRunPreviewRequest request,
        ScenarioRunParameterSet parameters,
        ScenarioRunPreviewCase baseline,
        ScenarioRunPreviewCase scenario)
    {
        var trace = new List<ScenarioAuditTrace>
        {
            new("Data", $"读取 {data.Skus.Count} 个 SKU、{data.Resources.Count} 个资源、{data.SupplierItemSources.Count} 条供应来源。", "Information"),
            new("Scenario", $"模板 {request.TemplateId ?? "无"}；采纳口径 {request.AdoptionConstraintMode ?? "Balanced"}；提前建库 {parameters.PrebuildCampaigns?.Count ?? 0} 条；产能调整 {parameters.CapacityAdjustments?.Count ?? 0} 条；SKU 策略调整 {parameters.SkuPolicyOverrides?.Count ?? 0} 条。", "Information"),
            new("Engine", "基准方案与预览方案均复用需求驱动计划引擎，未复制业务逻辑。", "Information"),
            new("Result", $"峰值负荷变化 {scenario.Metrics.PeakLoadPercent - baseline.Metrics.PeakLoadPercent:0.#}pp，供应缺口变化 {scenario.Metrics.SupplyGap - baseline.Metrics.SupplyGap:0}。", scenario.Metrics.SupplyGap > baseline.Metrics.SupplyGap ? "Warning" : "Information"),
            new("Persistence", "本次为预览结果，未保存、未审批、未调用优化求解器。", "Information")
        };
        if (request.ExternalScenario is not null)
        {
            trace.Insert(1, new ScenarioAuditTrace(
                "ExternalScenario",
                $"外部场景 {request.ExternalScenario.Name}：需求变化 {request.ExternalScenario.DemandChanges?.Count ?? 0}，供应风险 {request.ExternalScenario.SupplyRisks?.Count ?? 0}，能力损失 {request.ExternalScenario.CapacityLosses?.Count ?? 0}，已知事件 {request.ExternalScenario.KnownEvents?.Count ?? 0}。",
                "Information"));
        }
        trace.Insert(2, new ScenarioAuditTrace(
            "ResponseConfiguration",
            $"企业响应：提前建库 {parameters.PrebuildCampaigns?.Count ?? 0}，临时能力 {parameters.CapacityAdjustments?.Count ?? 0}，主参数覆盖 {parameters.SkuPolicyOverrides?.Count ?? 0}，供应响应 {parameters.SupplierCapacityLimits?.Count ?? 0}。",
            "Information"));
        return trace;
    }

    private static string ScenarioName(ScenarioWorkspaceDataSet data, string? templateId)
    {
        return data.ScenarioTemplates.FirstOrDefault(item => item.TemplateId == templateId)?.Name ?? "手工场景预览";
    }

    private static decimal CalculateFlowIndex(decimal bufferHealth, decimal utilization)
    {
        return Math.Clamp(
            decimal.Round(100m - Math.Max(0, utilization - 85m) * 0.6m - Math.Max(0, 90m - bufferHealth) * 0.35m, 1),
            40m,
            100m);
    }
}
