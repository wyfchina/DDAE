namespace AdaptiveSopDdsop.Web.Domain;

public static class DemandDrivenPlanningEngine
{
    public static DemandDrivenPlanRun ProjectBuffers(
        IReadOnlyList<SkuBufferSetting> skus,
        IReadOnlyList<InventoryPosition> inventory,
        IReadOnlyList<WeeklyDemand> demand,
        int horizonWeeks,
        IReadOnlyList<PrebuildCampaign>? prebuildCampaigns = null)
    {
        var projections = new List<BufferProjectionPoint>();
        var orders = new List<ProjectedReplenishmentOrder>();
        var traces = new List<PlanningTrace>();
        prebuildCampaigns ??= Array.Empty<PrebuildCampaign>();

        foreach (var sku in skus)
        {
            var position = inventory.First(item => item.Sku == sku.Sku);
            var zones = DdmrpCalculator.CalculateZones(sku);
            var netFlow = DdmrpCalculator.CalculateNetFlow(position);
            var orderCycleWeeks = Math.Max(1, (int)Math.Ceiling(sku.OrderCycleDays / 7m));

            for (var week = 1; week <= horizonWeeks; week++)
            {
                var startNetFlow = netFlow;
                foreach (var campaign in prebuildCampaigns.Where(item => item.Sku == sku.Sku && item.BuildWeek == week))
                {
                    startNetFlow += campaign.Quantity;
                    orders.Add(new ProjectedReplenishmentOrder(
                        sku.Sku,
                        week,
                        decimal.Round(campaign.Quantity, 0),
                        decimal.Round(campaign.Quantity * sku.UnitCost, 2),
                        "PrebuildCampaign"));
                    traces.Add(new PlanningTrace(
                        sku.Sku,
                        week,
                        $"提前建库 {campaign.CampaignId} 在第 {week} 周增加 {campaign.Quantity:0}，保护第 {campaign.ProtectFromWeek}-{campaign.ProtectThroughWeek} 周。"));
                }

                var weeklyDemand = demand
                    .Where(item => item.Sku == sku.Sku && item.Week == week)
                    .Sum(item => item.BaselineDemand);
                var endBeforeReplenishment = startNetFlow - weeklyDemand;
                var status = DdmrpCalculator.GetBufferStatus(endBeforeReplenishment, zones);
                var isOrderReviewWeek = (week - 1) % orderCycleWeeks == 0;
                var orderQuantity = isOrderReviewWeek && endBeforeReplenishment <= zones.TopOfYellow
                    ? zones.TopOfGreen - endBeforeReplenishment
                    : 0;
                var endAfterReplenishment = endBeforeReplenishment + orderQuantity;

                projections.Add(new BufferProjectionPoint(
                    sku.Sku,
                    week,
                    decimal.Round(startNetFlow, 0),
                    decimal.Round(weeklyDemand, 0),
                    decimal.Round(endBeforeReplenishment, 0),
                    decimal.Round(endAfterReplenishment, 0),
                    status));

                if (orderQuantity > 0)
                {
                    orders.Add(new ProjectedReplenishmentOrder(
                        sku.Sku,
                        week,
                        decimal.Round(orderQuantity, 0),
                        decimal.Round(orderQuantity * sku.UnitCost, 2),
                        "BelowTopOfYellow"));
                    traces.Add(new PlanningTrace(
                        sku.Sku,
                        week,
                        $"净流动量 {endBeforeReplenishment:0} 位于黄区上沿 {zones.TopOfYellow:0} 及以下，且本周为订货周期复核点，补货到绿区上沿 {zones.TopOfGreen:0}。"));
                }
                else if (endBeforeReplenishment <= zones.TopOfYellow)
                {
                    traces.Add(new PlanningTrace(
                        sku.Sku,
                        week,
                        $"净流动量 {endBeforeReplenishment:0} 位于黄区上沿 {zones.TopOfYellow:0} 及以下，但本周不是订货周期复核点，暂不生成补货订单。"));
                }
                else
                {
                    traces.Add(new PlanningTrace(
                        sku.Sku,
                        week,
                        $"净流动量 {endBeforeReplenishment:0} 高于黄区上沿 {zones.TopOfYellow:0}，不生成补货订单。"));
                }

                netFlow = endAfterReplenishment;
            }
        }

        return new DemandDrivenPlanRun(projections, orders, traces);
    }

    public static IReadOnlyList<CapacityLoadProjection> ProjectRoughCutCapacity(
        IReadOnlyList<ProjectedReplenishmentOrder> orders,
        IReadOnlyList<ResourceRouting> routings,
        IReadOnlyList<CapacityResource> resources,
        int horizonWeeks,
        IReadOnlyList<ResourceCapacityAdjustment>? capacityAdjustments = null)
    {
        var projections = new List<CapacityLoadProjection>();
        capacityAdjustments ??= Array.Empty<ResourceCapacityAdjustment>();

        for (var week = 1; week <= horizonWeeks; week++)
        {
            foreach (var resource in resources)
            {
                var required = orders
                    .Where(order => order.Week == week)
                    .Join(
                        routings.Where(routing => routing.ResourceCode == resource.Code),
                        order => order.Sku,
                        routing => routing.Sku,
                        (order, routing) => order.Quantity * routing.CapacityPerUnit)
                    .Sum();
                var capacityMultiplier = capacityAdjustments
                    .Where(item => item.ResourceCode == resource.Code && item.Week == week)
                    .Select(item => item.CapacityMultiplier)
                    .DefaultIfEmpty(1m)
                    .Aggregate(1m, (current, next) => current * next);
                var available = resource.WeeklyAvailableUnits / Math.Max(resource.UnitLoad, 0.0001m) * capacityMultiplier;
                var loadPercent = available <= 0 ? 999 : decimal.Round(required * 100m / available, 1);
                var status = loadPercent > 100 ? "Red" : loadPercent > 85 ? "Yellow" : "Green";

                projections.Add(new CapacityLoadProjection(
                    resource.Code,
                    resource.Name,
                    week,
                    decimal.Round(required, 1),
                    decimal.Round(available, 1),
                    loadPercent,
                    status));
            }
        }

        return projections;
    }

    public static IReadOnlyList<ProjectedSupplyRequirement> ProjectSupplyRequirements(
        IReadOnlyList<ProjectedReplenishmentOrder> orders,
        IReadOnlyList<SupplierItemSource> sources)
    {
        return orders
            .Join(
                sources,
                order => order.Sku,
                source => source.Sku,
                (order, source) => new
                {
                    source.Supplier,
                    source.MaterialFamily,
                    order.Week,
                    order.Quantity,
                    Value = order.Quantity * source.UnitCost
                })
            .GroupBy(item => new { item.Supplier, item.MaterialFamily, item.Week })
            .Select(group => new ProjectedSupplyRequirement(
                group.Key.Supplier,
                group.Key.MaterialFamily,
                group.Key.Week,
                decimal.Round(group.Sum(item => item.Quantity), 0),
                decimal.Round(group.Sum(item => item.Value), 2)))
            .OrderBy(item => item.Supplier)
            .ThenBy(item => item.MaterialFamily)
            .ThenBy(item => item.Week)
            .ToList();
    }
}
