namespace AdaptiveSopDdsop.Web.Domain;

public sealed class SupplierCollaborationWorkspaceService
{
    private readonly IScenarioWorkspaceDataSource _dataSource;

    public SupplierCollaborationWorkspaceService(IScenarioWorkspaceDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public SupplierCollaborationWorkspaceResult GetBaseline(int horizonWeeks)
    {
        var horizon = Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52);
        var data = _dataSource.Load(new ScenarioWorkspaceDataRequest(horizon, new DateOnly(2026, 6, 1)));
        var bufferRun = DemandDrivenPlanningEngine.ProjectBuffers(data.Skus, data.Inventory, data.Demand, horizon);
        var supplyRequirements = DemandDrivenPlanningEngine.ProjectSupplyRequirements(bufferRun.ReplenishmentOrders, data.SupplierItemSources);
        var supplierCapacity = ConstraintWorkspaceService.CompareSupplierCapacity(
            data.SupplierCapacityWindows,
            supplyRequirements,
            Array.Empty<SupplierCapacityLimit>());

        return Build(data, "baseline", "基准供应商需求钻取", bufferRun.ReplenishmentOrders, supplierCapacity);
    }

    public static SupplierCollaborationWorkspaceResult Build(
        ScenarioWorkspaceDataSet data,
        string caseId,
        string name,
        IReadOnlyList<ProjectedReplenishmentOrder> replenishmentOrders,
        IReadOnlyList<SupplierCapacityComparison> supplierCapacity)
    {
        var weeklyCells = BuildWeeklyCells(data, supplierCapacity);
        var skuRequirements = BuildSkuRequirements(data, replenishmentOrders);
        var summaries = weeklyCells
            .GroupBy(item => item.Supplier)
            .Select(group => BuildSummary(group.Key, group.ToList(), skuRequirements))
            .OrderByDescending(item => item.TotalGap)
            .ThenByDescending(item => item.RedWeekCount)
            .ThenBy(item => item.Supplier)
            .ToList();
        var actions = BuildActions(summaries);
        var trace = BuildTrace(data, replenishmentOrders, supplierCapacity, weeklyCells, skuRequirements, actions);
        var selectedSupplier = summaries.FirstOrDefault()?.Supplier ?? "";

        return new SupplierCollaborationWorkspaceResult(
            caseId,
            name,
            data.Request.HorizonWeeks,
            summaries,
            weeklyCells,
            skuRequirements,
            actions,
            trace,
            decimal.Round(weeklyCells.Sum(item => item.Gap), 0),
            summaries.Count(item => item.Status == "Red"),
            summaries.Count(item => item.Status == "Yellow"),
            weeklyCells.Count(item => item.Gap > 0m),
            skuRequirements.Select(item => item.Sku).Distinct(StringComparer.Ordinal).Count(),
            selectedSupplier);
    }

    private static IReadOnlyList<SupplierCollaborationWeeklyCell> BuildWeeklyCells(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<SupplierCapacityComparison> supplierCapacity)
    {
        var suppliers = data.SupplierItemSources
            .Select(item => item.Supplier)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var cells = new List<SupplierCollaborationWeeklyCell>();

        foreach (var supplier in suppliers)
        {
            for (var week = 1; week <= data.Request.HorizonWeeks; week++)
            {
                var comparisons = supplierCapacity
                    .Where(item => item.Supplier == supplier && item.Week == week)
                    .ToList();
                var windows = data.SupplierCapacityWindows
                    .Where(item => item.Supplier == supplier && item.Week == week)
                    .ToList();
                var required = comparisons.Sum(item => item.RequiredQuantity);
                var available = comparisons.Count > 0
                    ? comparisons.Sum(item => item.CommittedCapacity)
                    : windows.Sum(item => item.CommittedCapacity);
                var variance = decimal.Round(required - available, 0);
                var gap = decimal.Round(Math.Max(0m, variance), 0);
                var loadPercent = available <= 0m
                    ? (required > 0m ? 999m : 0m)
                    : decimal.Round(required * 100m / available, 1);
                var hasRisk = comparisons.Any(item => item.RiskStatus is "Yellow" or "Red") ||
                    windows.Any(item => item.RiskStatus is "Yellow" or "Red");
                var status = gap > 0m
                    ? "Red"
                    : loadPercent > 85m || hasRisk
                        ? "Yellow"
                        : "Green";
                var statusReason = gap > 0m
                    ? $"存在供应缺口 {gap:0}。"
                    : loadPercent > 85m
                        ? $"负荷率 {loadPercent:0.0}% ，接近供应能力上限。"
                        : hasRisk
                            ? "供应能力窗口带有风险标记，需滚动确认承诺能力。"
                            : "供应承诺能力覆盖当前不受限需求。";

                cells.Add(new SupplierCollaborationWeeklyCell(
                    supplier,
                    week,
                    decimal.Round(required, 0),
                    decimal.Round(available, 0),
                    variance,
                    gap,
                    loadPercent,
                    status,
                    statusReason));
            }
        }

        return cells
            .OrderBy(item => item.Supplier)
            .ThenBy(item => item.Week)
            .ToList();
    }

    private static SupplierCollaborationSummary BuildSummary(
        string supplier,
        IReadOnlyList<SupplierCollaborationWeeklyCell> cells,
        IReadOnlyList<SupplierSkuRequirement> skuRequirements)
    {
        var totalRequired = cells.Sum(item => item.UnconstrainedRequired);
        var totalAvailable = cells.Sum(item => item.ConstrainedAvailable);
        var totalGap = cells.Sum(item => item.Gap);
        var gapWeeks = cells.Count(item => item.Gap > 0m);
        var redWeeks = cells.Count(item => item.Status == "Red");
        var yellowWeeks = cells.Count(item => item.Status == "Yellow");
        var affectedSkus = skuRequirements
            .Where(item => item.Supplier == supplier)
            .Select(item => item.Sku)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var status = totalGap > 0m ? "Red" : yellowWeeks > 0 ? "Yellow" : "Green";
        var statusReason = status == "Red"
            ? $"累计供应缺口 {totalGap:0}，共有 {gapWeeks} 个缺口周。"
            : status == "Yellow"
                ? $"{yellowWeeks} 个周度窗口接近供应能力或带风险标记。"
                : "供应承诺能力覆盖当前不受限需求。";

        return new SupplierCollaborationSummary(
            supplier,
            decimal.Round(totalRequired, 0),
            decimal.Round(totalAvailable, 0),
            decimal.Round(totalGap, 0),
            gapWeeks,
            affectedSkus,
            redWeeks,
            yellowWeeks,
            status,
            status == "Red"
                ? "供应商协调 / 替代料 / 提前下单"
                : status == "Yellow"
                    ? "滚动确认承诺能力"
                    : "持续监控",
            statusReason);
    }

    private static IReadOnlyList<SupplierSkuRequirement> BuildSkuRequirements(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<ProjectedReplenishmentOrder> replenishmentOrders)
    {
        var skuMap = data.Skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);
        return replenishmentOrders
            .Join(
                data.SupplierItemSources,
                order => order.Sku,
                source => source.Sku,
                (order, source) =>
                {
                    skuMap.TryGetValue(order.Sku, out var sku);
                    return new SupplierSkuRequirement(
                        source.Supplier,
                        source.MaterialFamily,
                        order.Sku,
                        sku?.Name ?? order.Sku,
                        sku?.Family ?? "",
                        order.Week,
                        order.Quantity,
                        order.Value,
                        order.Trigger);
                })
            .OrderBy(item => item.Supplier)
            .ThenBy(item => item.Week)
            .ThenBy(item => item.MaterialFamily)
            .ThenBy(item => item.Sku)
            .ToList();
    }

    private static IReadOnlyList<SupplierCollaborationAction> BuildActions(
        IReadOnlyList<SupplierCollaborationSummary> summaries)
    {
        var actions = summaries
            .Where(item => item.Status != "Green")
            .Select(item => new SupplierCollaborationAction(
                item.Supplier,
                item.Status == "Red" ? "SupplierCoordination" : "CapacityConfirmation",
                item.Status == "Red"
                    ? $"{item.Supplier} 存在供应缺口 {item.TotalGap:0}，建议供应商协调、替代料或提前下单。"
                    : $"{item.Supplier} 接近供应能力上限，建议滚动确认承诺能力。",
                item.Status))
            .ToList();

        return actions.Count > 0
            ? actions
            : new[] { new SupplierCollaborationAction("全部供应商", "Monitor", "供应承诺能力覆盖当前不受限需求，保持监控。", "Green") };
    }

    private static IReadOnlyList<SupplierCollaborationTrace> BuildTrace(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<ProjectedReplenishmentOrder> replenishmentOrders,
        IReadOnlyList<SupplierCapacityComparison> supplierCapacity,
        IReadOnlyList<SupplierCollaborationWeeklyCell> weeklyCells,
        IReadOnlyList<SupplierSkuRequirement> skuRequirements,
        IReadOnlyList<SupplierCollaborationAction> actions)
    {
        return new[]
        {
            new SupplierCollaborationTrace("Demand", $"由 {replenishmentOrders.Count} 条预计补货订单形成供应商不受限需求。", "Information"),
            new SupplierCollaborationTrace("Supply", $"读取 {data.SupplierCapacityWindows.Count} 条供应能力窗口，并生成 {supplierCapacity.Count} 条供应能力对比。", "Information"),
            new SupplierCollaborationTrace("Detail", $"生成 {weeklyCells.Count} 个供应商周度格和 {skuRequirements.Count} 条 SKU 需求贡献。", "Information"),
            new SupplierCollaborationTrace("Action", $"触发 {actions.Count} 条供应商建议动作。", actions.Any(item => item.Severity == "Red") ? "Warning" : "Information")
        };
    }
}
