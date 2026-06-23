namespace AdaptiveSopDdsop.Web.Domain;

public sealed class ScenarioRunPreviewService
{
    private readonly IScenarioWorkspaceDataSource _dataSource;

    public ScenarioRunPreviewService(IScenarioWorkspaceDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public ScenarioRunPreviewResult Preview(ScenarioRunPreviewRequest request)
    {
        var horizonWeeks = Math.Clamp(request.HorizonWeeks <= 0 ? 12 : request.HorizonWeeks, 1, 52);
        var data = _dataSource.Load(new ScenarioWorkspaceDataRequest(
            horizonWeeks,
            new DateOnly(2026, 6, 1),
            request.SkuFilter,
            request.FamilyFilter));
        var parameters = MergeTemplateParameters(data, request.TemplateId, request.Parameters);

        var baseline = BuildCase("baseline", "基准方案", data, data.Skus, data.Demand, Array.Empty<PrebuildCampaign>(), Array.Empty<ResourceCapacityAdjustment>(), Array.Empty<SupplierCapacityLimit>());
        var scenarioSkus = ApplySkuPolicyOverrides(data.Skus, parameters.SkuPolicyOverrides ?? Array.Empty<SkuPolicyOverride>());
        var scenarioDemand = ApplyDemandEvents(data, request.TemplateId);
        var scenario = BuildCase(
            "scenario",
            ScenarioName(data, request.TemplateId),
            data,
            scenarioSkus,
            scenarioDemand,
            parameters.PrebuildCampaigns ?? Array.Empty<PrebuildCampaign>(),
            parameters.CapacityAdjustments ?? Array.Empty<ResourceCapacityAdjustment>(),
            parameters.SupplierCapacityLimits ?? Array.Empty<SupplierCapacityLimit>());

        var bufferTrendComparison = BufferTrendWorkspaceService.Compare(baseline.BufferTrend, scenario.BufferTrend);
        scenario = scenario with
        {
            BufferTrend = BufferTrendWorkspaceService.WithComparison(scenario.BufferTrend, bufferTrendComparison)
        };
        var trace = BuildAuditTrace(data, request, parameters, baseline, scenario);

        return new ScenarioRunPreviewResult(
            request with { HorizonWeeks = horizonWeeks, Parameters = parameters },
            baseline,
            scenario,
            Compare(baseline.Metrics, scenario.Metrics),
            RccpWorkspaceService.Compare(baseline.Rccp, scenario.Rccp),
            trace,
            IsPersisted: false);
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
        return new List<ScenarioAuditTrace>
        {
            new("Data", $"读取 {data.Skus.Count} 个 SKU、{data.Resources.Count} 个资源、{data.SupplierItemSources.Count} 条供应来源。", "Information"),
            new("Scenario", $"模板 {request.TemplateId ?? "无"}；采纳口径 {request.AdoptionConstraintMode ?? "Balanced"}；提前建库 {parameters.PrebuildCampaigns?.Count ?? 0} 条；产能调整 {parameters.CapacityAdjustments?.Count ?? 0} 条；SKU 策略调整 {parameters.SkuPolicyOverrides?.Count ?? 0} 条。", "Information"),
            new("Engine", "基准方案与预览方案均复用需求驱动计划引擎，未复制业务逻辑。", "Information"),
            new("Result", $"峰值负荷变化 {scenario.Metrics.PeakLoadPercent - baseline.Metrics.PeakLoadPercent:0.#}pp，供应缺口变化 {scenario.Metrics.SupplyGap - baseline.Metrics.SupplyGap:0}。", scenario.Metrics.SupplyGap > baseline.Metrics.SupplyGap ? "Warning" : "Information"),
            new("Persistence", "本次为预览结果，未保存、未审批、未调用优化求解器。", "Information")
        };
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
