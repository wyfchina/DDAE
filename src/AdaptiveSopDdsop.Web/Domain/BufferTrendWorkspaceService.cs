namespace AdaptiveSopDdsop.Web.Domain;

public sealed class BufferTrendWorkspaceService
{
    private readonly IScenarioWorkspaceDataSource _dataSource;

    public BufferTrendWorkspaceService(IScenarioWorkspaceDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public BufferTrendWorkspaceResult GetBaseline(int horizonWeeks)
    {
        var horizon = Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52);
        var data = _dataSource.Load(new ScenarioWorkspaceDataRequest(horizon, new DateOnly(2026, 6, 1)));
        var bufferRun = DemandDrivenPlanningEngine.ProjectBuffers(data.Skus, data.Inventory, data.Demand, horizon);
        var plan = new DemandDrivenPlanResult(
            bufferRun.BufferProjections,
            bufferRun.ReplenishmentOrders,
            Array.Empty<CapacityLoadProjection>(),
            Array.Empty<ProjectedSupplyRequirement>(),
            bufferRun.Traces);

        return Build(data, "baseline", "基准方案", data.Skus, plan);
    }

    public static BufferTrendWorkspaceResult Build(
        ScenarioWorkspaceDataSet data,
        string caseId,
        string name,
        IReadOnlyList<SkuBufferSetting> skus,
        DemandDrivenPlanResult plan)
    {
        var skuMap = skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);
        var zoneMap = skus.ToDictionary(item => item.Sku, DdmrpCalculator.CalculateZones, StringComparer.Ordinal);
        var orderMap = plan.ReplenishmentOrders
            .GroupBy(item => (item.Sku, item.Week))
            .ToDictionary(
                group => group.Key,
                group => new
                {
                    Quantity = group.Sum(item => item.Quantity),
                    IsPrebuild = group.Any(item => item.Trigger == "PrebuildCampaign")
                });

        var series = plan.BufferProjections
            .Where(point => skuMap.ContainsKey(point.Sku))
            .Select(point =>
            {
                var sku = skuMap[point.Sku];
                var timePhasedAdu = CalculateTimePhasedAdu(point);
                var timePhasedZones = CalculateTimePhasedZones(sku, timePhasedAdu);
                var targetInventory = (timePhasedZones.TopOfYellow + timePhasedZones.TopOfGreen) / 2m;
                orderMap.TryGetValue((point.Sku, point.Week), out var order);
                var status = TrendStatus(point, timePhasedZones);
                return new BufferTrendSeriesPoint(
                    point.Sku,
                    point.Week,
                    data.Request.AnchorDate.AddDays((point.Week - 1) * 7).ToString("yyyy/M/d", System.Globalization.CultureInfo.InvariantCulture),
                    decimal.Round(timePhasedAdu, 1),
                    point.StartNetFlow,
                    point.Demand,
                    point.EndNetFlowBeforeReplenishment,
                    point.EndNetFlowAfterReplenishment,
                    timePhasedZones.TopOfRed,
                    timePhasedZones.TopOfYellow,
                    timePhasedZones.TopOfGreen,
                    decimal.Round(targetInventory, 0),
                    decimal.Round(point.EndNetFlowAfterReplenishment * sku.UnitCost, 0),
                    decimal.Round(order?.Quantity ?? 0m, 0),
                    (order?.Quantity ?? 0m) > 0,
                    order?.IsPrebuild ?? false,
                    status);
            })
            .OrderBy(item => item.Sku)
            .ThenBy(item => item.Week)
            .ToList();

        var weeklyCells = series
            .Select(point =>
            {
                var sku = skuMap[point.Sku];
                return new BufferWeeklyCell(
                    point.Sku,
                    sku.Name,
                    sku.Family,
                    point.Week,
                    point.EndNetFlowAfterReplenishment,
                    point.InventoryValue,
                    point.Status);
            })
            .ToList();

        var familySummaries = weeklyCells
            .GroupBy(item => item.Family)
            .Select(group => new BufferFamilySummary(
                group.Key,
                decimal.Round(group.Average(item => item.InventoryValue), 0),
                group.Count(item => item.Status == "Red"),
                group.Count(item => item.Status == "Yellow"),
                group.Count(item => item.Status == "Blue"),
                plan.ReplenishmentOrders.Count(order =>
                    skuMap.TryGetValue(order.Sku, out var sku) && sku.Family == group.Key)))
            .OrderByDescending(item => item.RedWeekCount)
            .ThenByDescending(item => item.YellowWeekCount)
            .ThenBy(item => item.Family)
            .ToList();

        var zoneBands = skus
            .Select(sku =>
            {
                var zones = zoneMap[sku.Sku];
                return new BufferZoneBand(sku.Sku, decimal.Round(zones.TopOfRed, 0), decimal.Round(zones.TopOfYellow, 0), decimal.Round(zones.TopOfGreen, 0));
            })
            .OrderBy(item => item.Sku)
            .ToList();

        var selectedSku = SelectRiskSku(series, skuMap);
        var details = skus
            .Select(sku =>
            {
                var zone = zoneBands.First(item => item.Sku == sku.Sku);
                return new BufferSkuDetail(
                    sku.Sku,
                    sku.Name,
                    sku.Family,
                    sku.Adu,
                    sku.DecoupledLeadTimeDays,
                    sku.MinimumOrderQuantity,
                    sku.OrderCycleDays,
                    sku.UnitCost,
                    zone,
                    series.Where(item => item.Sku == sku.Sku).OrderBy(item => item.Week).ToList(),
                    plan.ReplenishmentOrders.Where(item => item.Sku == sku.Sku).OrderBy(item => item.Week).ToList(),
                    plan.Traces.Where(item => item.Sku == sku.Sku).OrderBy(item => item.Week).ToList());
            })
            .OrderBy(item => item.Sku)
            .ToList();

        return new BufferTrendWorkspaceResult(
            caseId,
            name,
            data.Request.HorizonWeeks,
            CalculateKpis(series, plan.ReplenishmentOrders, 0m),
            series,
            zoneBands,
            new BufferTrendComparison(0m, 0m, 0, 0, 0m),
            familySummaries,
            weeklyCells,
            details,
            selectedSku);
    }

    public static BufferTrendComparison Compare(BufferTrendWorkspaceResult baseline, BufferTrendWorkspaceResult scenario)
    {
        return new BufferTrendComparison(
            decimal.Round(scenario.Kpis.AverageInventoryValue - baseline.Kpis.AverageInventoryValue, 0),
            decimal.Round(scenario.Kpis.PeakInventoryValue - baseline.Kpis.PeakInventoryValue, 0),
            scenario.Series.Count(item => item.Status == "Red") - baseline.Series.Count(item => item.Status == "Red"),
            scenario.Kpis.ReplenishmentOrderCount - baseline.Kpis.ReplenishmentOrderCount,
            decimal.Round(
                scenario.Series.Sum(item => item.ReplenishmentQuantity) - baseline.Series.Sum(item => item.ReplenishmentQuantity),
                0));
    }

    public static BufferTrendWorkspaceResult WithComparison(
        BufferTrendWorkspaceResult trend,
        BufferTrendComparison comparison)
    {
        return trend with
        {
            Comparison = comparison,
            Kpis = trend.Kpis with { InventoryValueDelta = comparison.AverageInventoryValueDelta }
        };
    }

    private static BufferTrendKpis CalculateKpis(
        IReadOnlyList<BufferTrendSeriesPoint> series,
        IReadOnlyList<ProjectedReplenishmentOrder> orders,
        decimal inventoryValueDelta)
    {
        return new BufferTrendKpis(
            series.Where(item => item.Status == "Red").Select(item => item.Sku).Distinct().Count(),
            series.Where(item => item.Status == "Yellow").Select(item => item.Sku).Distinct().Count(),
            series.Count(item => item.EndNetFlowBeforeReplenishment <= 0),
            series.Count == 0 ? 0m : decimal.Round(series.Average(item => item.InventoryValue), 0),
            series.Count == 0 ? 0m : decimal.Round(series.Max(item => item.InventoryValue), 0),
            orders.Count,
            decimal.Round(inventoryValueDelta, 0));
    }

    private static string TrendStatus(BufferProjectionPoint point, BufferZones zones)
    {
        if (point.EndNetFlowBeforeReplenishment <= zones.TopOfRed)
        {
            return "Red";
        }

        if (point.EndNetFlowBeforeReplenishment <= zones.TopOfYellow)
        {
            return "Yellow";
        }

        return point.EndNetFlowAfterReplenishment > zones.TopOfGreen ? "Blue" : "Green";
    }

    private static decimal CalculateTimePhasedAdu(BufferProjectionPoint point)
    {
        // Seed demand is modeled as one five-workday planning bucket. Time-phased ADU lets
        // the display follow the same DDMRP zone equations while still showing weekly changes.
        return Math.Max(1m, point.Demand / 5m);
    }

    private static BufferZones CalculateTimePhasedZones(SkuBufferSetting sku, decimal timePhasedAdu)
    {
        var red = timePhasedAdu * sku.DecoupledLeadTimeDays * sku.VariabilityFactor;
        var yellow = timePhasedAdu * sku.DecoupledLeadTimeDays;
        var green = Math.Max(sku.MinimumOrderQuantity, timePhasedAdu * sku.OrderCycleDays);
        return new BufferZones(
            decimal.Round(red, 0),
            decimal.Round(yellow, 0),
            decimal.Round(green, 0));
    }

    private static string SelectRiskSku(
        IReadOnlyList<BufferTrendSeriesPoint> series,
        IReadOnlyDictionary<string, SkuBufferSetting> skuMap)
    {
        return series
            .GroupBy(item => item.Sku)
            .Select(group => new
            {
                Sku = group.Key,
                RedWeeks = group.Count(item => item.Status == "Red"),
                YellowWeeks = group.Count(item => item.Status == "Yellow"),
                AverageInventoryValue = group.Average(item => item.InventoryValue)
            })
            .OrderByDescending(item => item.RedWeeks)
            .ThenByDescending(item => item.YellowWeeks)
            .ThenByDescending(item => item.AverageInventoryValue)
            .ThenBy(item => skuMap.TryGetValue(item.Sku, out var sku) ? sku.Name : item.Sku)
            .Select(item => item.Sku)
            .FirstOrDefault() ?? skuMap.Keys.OrderBy(item => item).FirstOrDefault() ?? string.Empty;
    }
}
