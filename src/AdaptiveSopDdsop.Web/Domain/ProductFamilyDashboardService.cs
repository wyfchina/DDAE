namespace AdaptiveSopDdsop.Web.Domain;

public sealed class ProductFamilyDashboardService
{
    private readonly IScenarioWorkspaceDataSource _dataSource;

    public ProductFamilyDashboardService(IScenarioWorkspaceDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public ProductFamilyDashboardResult GetBaseline(int horizonWeeks)
    {
        var horizon = Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52);
        var data = _dataSource.Load(new ScenarioWorkspaceDataRequest(horizon, new DateOnly(2026, 6, 1)));
        var bufferRun = DemandDrivenPlanningEngine.ProjectBuffers(data.Skus, data.Inventory, data.Demand, horizon);
        var inventoryFlow = InventoryFlowProjectionService.Project(
            data,
            "baseline",
            data.Skus,
            data.Demand,
            bufferRun.ReplenishmentOrders,
            Array.Empty<PrebuildCampaign>(),
            Array.Empty<SupplierCapacityLimit>());
        var capacityLoads = DemandDrivenPlanningEngine.ProjectRoughCutCapacity(
            bufferRun.ReplenishmentOrders,
            data.ResourceRoutings,
            data.Resources,
            horizon);
        var supplyRequirements = DemandDrivenPlanningEngine.ProjectSupplyRequirements(
            bufferRun.ReplenishmentOrders,
            data.SupplierItemSources);
        var supplierCapacity = ConstraintWorkspaceService.CompareSupplierCapacity(
            data.SupplierCapacityWindows,
            supplyRequirements,
            Array.Empty<SupplierCapacityLimit>());
        var plan = new DemandDrivenPlanResult(
            bufferRun.BufferProjections,
            bufferRun.ReplenishmentOrders,
            capacityLoads,
            supplyRequirements,
            bufferRun.Traces);

        return Build(
            data,
            "baseline",
            "基准方案",
            data.Skus,
            plan,
            supplierCapacity,
            CompareBudget(data, "baseline", data.Skus, bufferRun.BufferProjections, inventoryFlow),
            inventoryFlow);
    }

    public static ProductFamilyDashboardResult Build(
        ScenarioWorkspaceDataSet data,
        string caseId,
        string name,
        IReadOnlyList<SkuBufferSetting> skus,
        DemandDrivenPlanResult plan,
        IReadOnlyList<SupplierCapacityComparison> supplierCapacity,
        IReadOnlyList<BudgetComparison> budget,
        InventoryFlowProjectionResult? inventoryFlow = null)
    {
        var expectedPhysicalKeys = plan.BufferProjections
            .Select(item => (item.Sku, item.Week))
            .ToList();
        var physicalPoints = InventoryFlowEvidenceValidator.BuildCompletePointMap(
            caseId,
            inventoryFlow,
            expectedPhysicalKeys);
        var physicalComplete = physicalPoints.Count == expectedPhysicalKeys.Count && expectedPhysicalKeys.Count > 0;
        var families = data.Families
            .Where(family => skus.Any(sku => sku.Family == family.Code))
            .OrderBy(family => family.Code, StringComparer.Ordinal)
            .ToList();
        var weeklyCells = families
            .SelectMany(family => Enumerable.Range(1, data.Request.HorizonWeeks)
                .Select(week => BuildWeeklyCell(
                    data,
                    family,
                    skus,
                    plan,
                    supplierCapacity,
                    budget,
                    physicalPoints,
                    physicalComplete,
                    week)))
            .ToList();
        var summaries = families
            .Select(family => BuildSummary(
                data,
                family,
                skus,
                plan,
                weeklyCells.Where(item => item.Family == family.Code).ToList(),
                physicalPoints,
                physicalComplete))
            .OrderByDescending(item => StatusRank(item.Status))
            .ThenByDescending(item => item.SupplyGap + item.CapacityGap)
            .ThenByDescending(item => item.RedWeekCount)
            .ThenBy(item => item.Family, StringComparer.Ordinal)
            .ToList();
        var details = families
            .Select(family => BuildDetail(
                data,
                family,
                skus,
                plan,
                supplierCapacity,
                weeklyCells.Where(item => item.Family == family.Code).ToList(),
                physicalPoints,
                physicalComplete))
            .ToList();
        var selectedFamily = summaries.FirstOrDefault()?.Family ?? families.FirstOrDefault()?.Code ?? string.Empty;

        return new ProductFamilyDashboardResult(
            caseId,
            name,
            data.Request.HorizonWeeks,
            summaries,
            weeklyCells,
            details,
            InitialComparison(physicalComplete),
            selectedFamily);
    }

    public static ProductFamilyDashboardComparison Compare(
        ProductFamilyDashboardResult baseline,
        ProductFamilyDashboardResult scenario,
        InventoryFlowProjectionResult? baselineInventoryFlow = null,
        InventoryFlowProjectionResult? scenarioInventoryFlow = null)
    {
        var physicalComplete = HasCompleteInventoryAmounts(baseline) &&
            HasCompleteInventoryAmounts(scenario) &&
            baselineInventoryFlow is { Status: "Complete", Summary: not null, Points: not null } &&
            scenarioInventoryFlow is { Status: "Complete", Summary: not null, Points: not null } &&
            string.Equals(baselineInventoryFlow.CaseId, baseline.CaseId, StringComparison.Ordinal) &&
            string.Equals(scenarioInventoryFlow.CaseId, scenario.CaseId, StringComparison.Ordinal);
        var serviceDelta = decimal.Round(Average(scenario.Summaries, item => item.ServiceLevelPercent) - Average(baseline.Summaries, item => item.ServiceLevelPercent), 1);
        decimal? inventoryDelta = physicalComplete
            ? decimal.Round(
                AverageNullable(scenario.Summaries, item => item.AverageInventoryValue)!.Value -
                AverageNullable(baseline.Summaries, item => item.AverageInventoryValue)!.Value,
                0)
            : null;
        decimal? budgetDelta = physicalComplete
            ? decimal.Round(
                scenario.Summaries.Sum(item => item.BudgetInventoryVariance!.Value) -
                baseline.Summaries.Sum(item => item.BudgetInventoryVariance!.Value),
                0)
            : null;
        return new ProductFamilyDashboardComparison(
            serviceDelta,
            decimal.Round(Average(scenario.Summaries, item => item.FlowIndex) - Average(baseline.Summaries, item => item.FlowIndex), 1),
            inventoryDelta,
            decimal.Round(scenario.Summaries.Sum(item => item.SupplyGap) - baseline.Summaries.Sum(item => item.SupplyGap), 0),
            decimal.Round(scenario.Summaries.Sum(item => item.CapacityGap) - baseline.Summaries.Sum(item => item.CapacityGap), 1),
            scenario.Summaries.Sum(item => item.RedWeekCount) - baseline.Summaries.Sum(item => item.RedWeekCount),
            budgetDelta,
            physicalComplete ? serviceDelta : null,
            inventoryDelta,
            budgetDelta,
            physicalComplete ? "Complete" : "EvidenceMissing",
            physicalComplete
                ? "Both product-family cases use complete physical inventory projections."
                : "Physical product-family deltas are omitted because at least one case lacks complete inventory-flow evidence.");
    }

    public static ProductFamilyDashboardResult WithComparison(
        ProductFamilyDashboardResult dashboard,
        ProductFamilyDashboardComparison comparison)
    {
        return dashboard with { Comparison = comparison };
    }

    private static ProductFamilyWeeklyCell BuildWeeklyCell(
        ScenarioWorkspaceDataSet data,
        ProductFamily family,
        IReadOnlyList<SkuBufferSetting> skus,
        DemandDrivenPlanResult plan,
        IReadOnlyList<SupplierCapacityComparison> supplierCapacity,
        IReadOnlyList<BudgetComparison> budget,
        IReadOnlyDictionary<(string Sku, int Week), InventoryFlowPoint> physicalPoints,
        bool physicalComplete,
        int week)
    {
        var familySkus = skus.Where(item => item.Family == family.Code).ToList();
        var familySkuSet = familySkus.Select(item => item.Sku).ToHashSet(StringComparer.Ordinal);
        decimal? inventoryValue = physicalComplete
            ? physicalPoints.Values
                .Where(item => item.Week == week && familySkuSet.Contains(item.Sku))
                .Sum(item => item.EndingInventoryValue)
            : null;
        var redSkuCount = plan.BufferProjections
            .Where(item => item.Week == week && familySkuSet.Contains(item.Sku) && item.BufferStatus == "Red")
            .Select(item => item.Sku)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var yellowSkuCount = plan.BufferProjections
            .Where(item => item.Week == week && familySkuSet.Contains(item.Sku) && item.BufferStatus == "Yellow")
            .Select(item => item.Sku)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var capacityGap = FamilyCapacityGap(family.Code, familySkuSet, data.ResourceRoutings, plan.ReplenishmentOrders, data.Resources, week);
        var peakLoad = FamilyPeakLoadPercent(familySkuSet, data.ResourceRoutings, plan.ReplenishmentOrders, data.Resources, week);
        var supplyGap = FamilySupplyGap(familySkuSet, data.SupplierItemSources, plan.ReplenishmentOrders, supplierCapacity, week);
        var budgetRows = budget
            .Where(item => item.Family == family.Code && item.Week == week)
            .ToList();
        decimal? budgetVariance = !physicalComplete
            ? null
            : budgetRows.Count == 0
                ? 0m
                : budgetRows.All(item => item.BudgetInventoryVariance.HasValue)
                    ? budgetRows.Sum(item => item.BudgetInventoryVariance!.Value)
                    : null;
        var demand = data.Demand
            .Where(item => item.Week == week && familySkuSet.Contains(item.Sku))
            .Sum(item => item.BaselineDemand);
        var replenishmentQuantity = plan.ReplenishmentOrders
            .Where(item => item.Week == week && familySkuSet.Contains(item.Sku))
            .Sum(item => item.Quantity);
        var status = redSkuCount > 0 || supplyGap > 0m || capacityGap > 0m
            ? "Red"
            : yellowSkuCount > 0 || budgetVariance is > 0m || peakLoad > 85m
                ? "Yellow"
                : "Green";

        return new ProductFamilyWeeklyCell(
            family.Code,
            week,
            decimal.Round(demand, 0),
            decimal.Round(replenishmentQuantity, 0),
            inventoryValue.HasValue ? decimal.Round(inventoryValue.Value, 0) : null,
            redSkuCount,
            yellowSkuCount,
            decimal.Round(supplyGap, 0),
            decimal.Round(capacityGap, 1),
            decimal.Round(peakLoad, 1),
            budgetVariance.HasValue ? decimal.Round(budgetVariance.Value, 0) : null,
            status);
    }

    private static ProductFamilySummary BuildSummary(
        ScenarioWorkspaceDataSet data,
        ProductFamily family,
        IReadOnlyList<SkuBufferSetting> skus,
        DemandDrivenPlanResult plan,
        IReadOnlyList<ProductFamilyWeeklyCell> weeklyCells,
        IReadOnlyDictionary<(string Sku, int Week), InventoryFlowPoint> physicalPoints,
        bool physicalComplete)
    {
        var familySkus = skus.Where(item => item.Family == family.Code).ToList();
        var familySkuSet = familySkus.Select(item => item.Sku).ToHashSet(StringComparer.Ordinal);
        var legacyService = data.HistoricalDemand
            .Where(item => familySkuSet.Contains(item.Sku))
            .Select(item => item.ServiceLevelPercent)
            .DefaultIfEmpty(family.TargetServiceLevel)
            .Average();
        var familyPhysicalPoints = physicalPoints.Values
            .Where(item => familySkuSet.Contains(item.Sku))
            .ToList();
        var physicalDemand = familyPhysicalPoints.Sum(item => item.Demand);
        var service = physicalComplete
            ? physicalDemand == 0m
                ? 0m
                : familyPhysicalPoints.Sum(item => item.FulfilledNewDemandOnTime) * 100m / physicalDemand
            : legacyService;
        var redSkuCount = plan.BufferProjections
            .Where(item => familySkuSet.Contains(item.Sku) && item.BufferStatus == "Red")
            .Select(item => item.Sku)
            .Distinct(StringComparer.Ordinal)
            .Count();
        decimal? averageInventory = physicalComplete && weeklyCells.Count > 0 && weeklyCells.All(item => item.InventoryValue.HasValue)
            ? weeklyCells.Average(item => item.InventoryValue!.Value)
            : null;
        decimal? peakInventory = physicalComplete && weeklyCells.Count > 0 && weeklyCells.All(item => item.InventoryValue.HasValue)
            ? weeklyCells.Max(item => item.InventoryValue!.Value)
            : null;
        decimal? budgetInventoryVariance = physicalComplete && weeklyCells.All(item => item.BudgetInventoryVariance.HasValue)
            ? weeklyCells.Sum(item => item.BudgetInventoryVariance!.Value)
            : null;
        var peakLoad = weeklyCells.Count == 0 ? 0m : weeklyCells.Max(item => item.PeakLoadPercent);
        var flowIndex = CalculateFamilyFlowIndex(family, legacyService, weeklyCells);
        var status = weeklyCells.Any(item => item.Status == "Red")
            ? "Red"
            : weeklyCells.Any(item => item.Status == "Yellow")
                ? "Yellow"
                : "Green";
        var replenishmentOrders = plan.ReplenishmentOrders
            .Where(item => familySkuSet.Contains(item.Sku))
            .ToList();

        return new ProductFamilySummary(
            family.Code,
            family.Name,
            familySkus.Count,
            family.TargetServiceLevel,
            family.TargetFlowIndex,
            decimal.Round(service, 1),
            flowIndex,
            averageInventory.HasValue ? decimal.Round(averageInventory.Value, 0) : null,
            peakInventory.HasValue ? decimal.Round(peakInventory.Value, 0) : null,
            redSkuCount,
            weeklyCells.Count(item => item.Status == "Red"),
            weeklyCells.Count(item => item.Status == "Yellow"),
            replenishmentOrders.Count,
            decimal.Round(replenishmentOrders.Sum(item => item.Value), 0),
            decimal.Round(weeklyCells.Sum(item => item.SupplyGap), 0),
            decimal.Round(weeklyCells.Sum(item => item.CapacityGap), 1),
            decimal.Round(peakLoad, 1),
            budgetInventoryVariance.HasValue ? decimal.Round(budgetInventoryVariance.Value, 0) : null,
            status,
            RecommendSummaryAction(status, weeklyCells, redSkuCount));
    }

    private static ProductFamilyDetail BuildDetail(
        ScenarioWorkspaceDataSet data,
        ProductFamily family,
        IReadOnlyList<SkuBufferSetting> skus,
        DemandDrivenPlanResult plan,
        IReadOnlyList<SupplierCapacityComparison> supplierCapacity,
        IReadOnlyList<ProductFamilyWeeklyCell> weeklyCells,
        IReadOnlyDictionary<(string Sku, int Week), InventoryFlowPoint> physicalPoints,
        bool physicalComplete)
    {
        var familySkus = skus.Where(item => item.Family == family.Code).ToList();
        var familySkuSet = familySkus.Select(item => item.Sku).ToHashSet(StringComparer.Ordinal);
        var riskItems = BuildRisks(data, family.Code, familySkuSet, plan, supplierCapacity, weeklyCells);
        var recommendations = BuildRecommendations(family.Code, weeklyCells, riskItems);

        return new ProductFamilyDetail(
            family.Code,
            family.Name,
            weeklyCells,
            riskItems,
            recommendations,
            BuildBufferSummaries(family.Code, plan, familySkus, physicalPoints, physicalComplete),
            BuildRccpContributions(data, familySkuSet, plan),
            BuildSupplierRequirements(data, familySkuSet, plan));
    }

    private static IReadOnlyList<ProductFamilyRiskItem> BuildRisks(
        ScenarioWorkspaceDataSet data,
        string family,
        HashSet<string> familySkuSet,
        DemandDrivenPlanResult plan,
        IReadOnlyList<SupplierCapacityComparison> supplierCapacity,
        IReadOnlyList<ProductFamilyWeeklyCell> weeklyCells)
    {
        var bufferRisks = plan.BufferProjections
            .Where(item => familySkuSet.Contains(item.Sku) && item.BufferStatus is "Red" or "Yellow")
            .Select(item => new ProductFamilyRiskItem("缓冲", item.Sku, item.Week, item.BufferStatus == "Red" ? "净流动量穿透红区" : "净流动量进入黄区", item.BufferStatus));
        var capacityRisks = weeklyCells
            .Where(item => item.CapacityGap > 0m || item.PeakLoadPercent > 85m)
            .Select(item => new ProductFamilyRiskItem("RCCP", family, item.Week, item.CapacityGap > 0m ? $"产能缺口 {item.CapacityGap:0.#}" : $"补货释放峰值 {item.PeakLoadPercent:0.#}%", item.CapacityGap > 0m ? "Red" : "Yellow"));
        var supplyKeys = data.SupplierItemSources
            .Where(item => familySkuSet.Contains(item.Sku))
            .Select(item => (item.Supplier, item.MaterialFamily))
            .Distinct()
            .ToHashSet();
        var supplyRisks = supplierCapacity
            .Where(item => supplyKeys.Contains((item.Supplier, item.MaterialFamily)) && item.RiskStatus is "Red" or "Yellow")
            .Select(item => new ProductFamilyRiskItem("供应", $"{item.Supplier} / {item.MaterialFamily}", item.Week, item.Gap > 0m ? $"供应缺口 {item.Gap:0}" : "供应窗口接近上限", item.RiskStatus));

        return bufferRisks
            .Concat(capacityRisks)
            .Concat(supplyRisks)
            .OrderByDescending(item => StatusRank(item.Severity))
            .ThenBy(item => item.Week)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .Take(24)
            .ToList();
    }

    private static IReadOnlyList<ProductFamilyActionRecommendation> BuildRecommendations(
        string family,
        IReadOnlyList<ProductFamilyWeeklyCell> weeklyCells,
        IReadOnlyList<ProductFamilyRiskItem> riskItems)
    {
        var recommendations = new List<ProductFamilyActionRecommendation>();
        if (weeklyCells.Any(item => item.SupplyGap > 0m))
        {
            recommendations.Add(new ProductFamilyActionRecommendation(family, "供应协调", "供应缺口已影响该产品族，优先与供应商确认承诺能力或替代来源。", "Red"));
        }

        if (weeklyCells.Any(item => item.CapacityGap > 0m))
        {
            recommendations.Add(new ProductFamilyActionRecommendation(family, "产能协调", "RCCP 已出现产能缺口，评估增班、外协或需求取舍。", "Red"));
        }

        if (riskItems.Any(item => item.Scope == "缓冲" && item.Severity == "Red"))
        {
            recommendations.Add(new ProductFamilyActionRecommendation(family, "库存缓冲", "存在红区 SKU，检查提前建库、MOQ 或订货周期策略。", "Red"));
        }

        if (recommendations.Count == 0 && weeklyCells.Any(item => item.Status == "Yellow"))
        {
            recommendations.Add(new ProductFamilyActionRecommendation(family, "会议关注", "产品族处于黄区，建议在 DDS&OP 会议中观察趋势，不立即采纳主设置变更。", "Yellow"));
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add(new ProductFamilyActionRecommendation(family, "保持", "服务、库存、供应与产能均处于可接受区间，可作为承接增量需求的候选产品族。", "Green"));
        }

        return recommendations;
    }

    private static IReadOnlyList<BufferFamilySummary> BuildBufferSummaries(
        string family,
        DemandDrivenPlanResult plan,
        IReadOnlyList<SkuBufferSetting> familySkus,
        IReadOnlyDictionary<(string Sku, int Week), InventoryFlowPoint> physicalPoints,
        bool physicalComplete)
    {
        var familySkuSet = familySkus.Select(item => item.Sku).ToHashSet(StringComparer.Ordinal);
        var inventoryValues = plan.BufferProjections
            .Where(item => familySkuSet.Contains(item.Sku))
            .Select(point => new
            {
                point.BufferStatus,
                Value = physicalComplete
                    ? physicalPoints[(point.Sku, point.Week)].EndingInventoryValue
                    : (decimal?)null
            })
            .ToList();

        return new[]
        {
            new BufferFamilySummary(
                family,
                physicalComplete && inventoryValues.Count > 0
                    ? decimal.Round(inventoryValues.Average(item => item.Value!.Value), 0)
                    : null,
                inventoryValues.Count(item => item.BufferStatus == "Red"),
                inventoryValues.Count(item => item.BufferStatus == "Yellow"),
                inventoryValues.Count(item => item.BufferStatus == "OverTopOfGreen"),
                plan.ReplenishmentOrders.Count(item => familySkuSet.Contains(item.Sku)))
        };
    }

    private static IReadOnlyList<RccpSkuContribution> BuildRccpContributions(
        ScenarioWorkspaceDataSet data,
        HashSet<string> familySkuSet,
        DemandDrivenPlanResult plan)
    {
        var skuMap = data.Skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);
        return plan.ReplenishmentOrders
            .Where(order => familySkuSet.Contains(order.Sku))
            .Join(data.ResourceRoutings, order => order.Sku, routing => routing.Sku, (order, routing) =>
            {
                var sku = skuMap[order.Sku];
                return new RccpSkuContribution(
                    routing.ResourceCode,
                    sku.Sku,
                    sku.Name,
                    sku.Family,
                    order.Week,
                    order.Quantity,
                    routing.CapacityPerUnit,
                    decimal.Round(order.Quantity * routing.CapacityPerUnit, 1),
                    order.Trigger);
            })
            .OrderByDescending(item => item.RequiredCapacity)
            .ThenBy(item => item.Week)
            .Take(24)
            .ToList();
    }

    private static IReadOnlyList<SupplierSkuRequirement> BuildSupplierRequirements(
        ScenarioWorkspaceDataSet data,
        HashSet<string> familySkuSet,
        DemandDrivenPlanResult plan)
    {
        var skuMap = data.Skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);
        return plan.ReplenishmentOrders
            .Where(order => familySkuSet.Contains(order.Sku))
            .Join(data.SupplierItemSources, order => order.Sku, source => source.Sku, (order, source) =>
            {
                var sku = skuMap[order.Sku];
                return new SupplierSkuRequirement(
                    source.Supplier,
                    source.MaterialFamily,
                    sku.Sku,
                    sku.Name,
                    sku.Family,
                    order.Week,
                    order.Quantity,
                    decimal.Round(order.Quantity * source.UnitCost, 0),
                    order.Trigger);
            })
            .OrderByDescending(item => item.ProjectedValue)
            .ThenBy(item => item.Week)
            .Take(24)
            .ToList();
    }

    private static decimal FamilyCapacityGap(
        string family,
        HashSet<string> familySkuSet,
        IReadOnlyList<ResourceRouting> routings,
        IReadOnlyList<ProjectedReplenishmentOrder> orders,
        IReadOnlyList<CapacityResource> resources,
        int week)
    {
        var requiredByResource = orders
            .Where(item => item.Week == week && familySkuSet.Contains(item.Sku))
            .Join(routings, order => order.Sku, routing => routing.Sku, (order, routing) => new
            {
                routing.ResourceCode,
                Required = order.Quantity * routing.CapacityPerUnit
            })
            .GroupBy(item => item.ResourceCode)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Required), StringComparer.Ordinal);

        return requiredByResource.Sum(item =>
        {
            var resource = resources.FirstOrDefault(candidate => candidate.Code == item.Key);
            var available = resource is null ? 0m : resource.WeeklyAvailableUnits / Math.Max(resource.UnitLoad, 0.0001m);
            return Math.Max(0m, item.Value - available);
        });
    }

    private static decimal FamilyPeakLoadPercent(
        HashSet<string> familySkuSet,
        IReadOnlyList<ResourceRouting> routings,
        IReadOnlyList<ProjectedReplenishmentOrder> orders,
        IReadOnlyList<CapacityResource> resources,
        int week)
    {
        var requiredByResource = orders
            .Where(item => item.Week == week && familySkuSet.Contains(item.Sku))
            .Join(routings, order => order.Sku, routing => routing.Sku, (order, routing) => new
            {
                routing.ResourceCode,
                Required = order.Quantity * routing.CapacityPerUnit
            })
            .GroupBy(item => item.ResourceCode);

        return requiredByResource
            .Select(group =>
            {
                var resource = resources.FirstOrDefault(candidate => candidate.Code == group.Key);
                var available = resource is null ? 0m : resource.WeeklyAvailableUnits / Math.Max(resource.UnitLoad, 0.0001m);
                return available <= 0m ? 999m : decimal.Round(group.Sum(item => item.Required) * 100m / available, 1);
            })
            .DefaultIfEmpty(0m)
            .Max();
    }

    private static decimal FamilySupplyGap(
        HashSet<string> familySkuSet,
        IReadOnlyList<SupplierItemSource> sources,
        IReadOnlyList<ProjectedReplenishmentOrder> orders,
        IReadOnlyList<SupplierCapacityComparison> supplierCapacity,
        int week)
    {
        var keys = sources
            .Where(item => familySkuSet.Contains(item.Sku))
            .Select(item => (item.Supplier, item.MaterialFamily))
            .Distinct()
            .ToHashSet();

        return supplierCapacity
            .Where(item => item.Week == week && keys.Contains((item.Supplier, item.MaterialFamily)))
            .Sum(item =>
            {
                var familyRequired = orders
                    .Where(order => order.Week == week && familySkuSet.Contains(order.Sku))
                    .Join(
                        sources.Where(source => source.Supplier == item.Supplier && source.MaterialFamily == item.MaterialFamily),
                        order => order.Sku,
                        source => source.Sku,
                        (order, _) => order.Quantity)
                    .Sum();
                return item.RequiredQuantity <= 0m
                    ? 0m
                    : item.Gap * familyRequired / item.RequiredQuantity;
            });
    }

    private static IReadOnlyList<BudgetComparison> CompareBudget(
        ScenarioWorkspaceDataSet data,
        string caseId,
        IReadOnlyList<SkuBufferSetting> skus,
        IReadOnlyList<BufferProjectionPoint> projections,
        InventoryFlowProjectionResult? inventoryFlow = null)
    {
        var skuMap = skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);
        var physicalPoints = InventoryFlowEvidenceValidator.BuildCompletePointMap(
            caseId,
            inventoryFlow,
            projections.Select(item => (item.Sku, item.Week)).ToList());
        var physicalComplete = physicalPoints.Count == projections.Count && projections.Count > 0;
        return data.BudgetBenchmarks.Select(benchmark =>
        {
            decimal? projectedInventory = physicalComplete
                ? physicalPoints.Values
                    .Where(item => item.Week == benchmark.Week)
                    .Where(item => skuMap.TryGetValue(item.Sku, out var sku) && sku.Family == benchmark.Family)
                    .Sum(item => item.EndingInventoryValue)
                : null;
            return new BudgetComparison(
                benchmark.Family,
                benchmark.Week,
                benchmark.BudgetRevenue,
                benchmark.LastYearRevenue,
                benchmark.BudgetInventoryValue,
                benchmark.LastYearInventoryValue,
                projectedInventory.HasValue ? decimal.Round(projectedInventory.Value, 0) : null,
                projectedInventory.HasValue
                    ? decimal.Round(projectedInventory.Value - benchmark.BudgetInventoryValue, 0)
                    : null);
        }).ToList();
    }

    private static decimal CalculateFamilyFlowIndex(
        ProductFamily family,
        decimal service,
        IReadOnlyList<ProductFamilyWeeklyCell> weeklyCells)
    {
        var redPenalty = weeklyCells.Count(item => item.Status == "Red") * 1.6m;
        var yellowPenalty = weeklyCells.Count(item => item.Status == "Yellow") * 0.6m;
        var servicePenalty = Math.Max(0m, family.TargetServiceLevel - service) * 0.8m;
        var peakPenalty = Math.Max(0m, weeklyCells.Select(item => item.PeakLoadPercent).DefaultIfEmpty(0m).Max() - 85m) * 0.25m;
        return Math.Clamp(decimal.Round(100m - redPenalty - yellowPenalty - servicePenalty - peakPenalty, 1), 40m, 100m);
    }

    private static string RecommendSummaryAction(string status, IReadOnlyList<ProductFamilyWeeklyCell> cells, int redSkuCount)
    {
        if (cells.Any(item => item.SupplyGap > 0m))
        {
            return "优先供应协调";
        }

        if (cells.Any(item => item.CapacityGap > 0m))
        {
            return "评审产能缓冲";
        }

        if (redSkuCount > 0)
        {
            return "检查库存缓冲参数";
        }

        if (status == "Yellow")
        {
            return "会议观察并准备备选动作";
        }

        return "保持当前主设置";
    }

    private static decimal Average(IReadOnlyList<ProductFamilySummary> summaries, Func<ProductFamilySummary, decimal> selector)
    {
        return summaries.Count == 0 ? 0m : summaries.Average(selector);
    }

    private static decimal? AverageNullable(
        IReadOnlyList<ProductFamilySummary> summaries,
        Func<ProductFamilySummary, decimal?> selector)
    {
        var values = summaries.Select(selector).ToList();
        return values.Count > 0 && values.All(item => item.HasValue)
            ? values.Average(item => item!.Value)
            : null;
    }

    private static bool HasCompleteInventoryAmounts(ProductFamilyDashboardResult dashboard) =>
        dashboard.Summaries.Count > 0 &&
        dashboard.Summaries.All(item =>
            item.AverageInventoryValue.HasValue &&
            item.PeakInventoryValue.HasValue &&
            item.BudgetInventoryVariance.HasValue) &&
        dashboard.WeeklyCells.Count > 0 &&
        dashboard.WeeklyCells.All(item =>
            item.InventoryValue.HasValue &&
            item.BudgetInventoryVariance.HasValue) &&
        dashboard.Details.SelectMany(item => item.BufferSummaries)
            .All(item => item.AverageInventoryValue.HasValue);

    private static ProductFamilyDashboardComparison InitialComparison(bool physicalComplete)
    {
        return new ProductFamilyDashboardComparison(
            0m,
            0m,
            physicalComplete ? 0m : null,
            0m,
            0m,
            0,
            physicalComplete ? 0m : null,
            physicalComplete ? 0m : null,
            physicalComplete ? 0m : null,
            physicalComplete ? 0m : null,
            physicalComplete ? "Complete" : "EvidenceMissing",
            physicalComplete
                ? "Reference case uses a complete physical inventory projection; physical deltas are zero."
                : "Reference case lacks complete inventory-flow evidence; physical deltas are omitted.");
    }

    private static int StatusRank(string status)
    {
        return status switch
        {
            "Red" => 3,
            "Yellow" => 2,
            "Green" => 1,
            _ => 0
        };
    }
}
