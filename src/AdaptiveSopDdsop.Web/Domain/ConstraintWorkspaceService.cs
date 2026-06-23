namespace AdaptiveSopDdsop.Web.Domain;

public sealed class ConstraintWorkspaceService
{
    private readonly IScenarioWorkspaceDataSource _dataSource;

    public ConstraintWorkspaceService(IScenarioWorkspaceDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public ConstraintWorkspaceResult GetBaseline(int horizonWeeks)
    {
        var horizon = Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52);
        var data = _dataSource.Load(new ScenarioWorkspaceDataRequest(horizon, new DateOnly(2026, 6, 1)));
        var bufferRun = DemandDrivenPlanningEngine.ProjectBuffers(data.Skus, data.Inventory, data.Demand, horizon);
        var capacityLoads = DemandDrivenPlanningEngine.ProjectRoughCutCapacity(
            bufferRun.ReplenishmentOrders,
            data.ResourceRoutings,
            data.Resources,
            horizon);
        var supplyRequirements = DemandDrivenPlanningEngine.ProjectSupplyRequirements(
            bufferRun.ReplenishmentOrders,
            data.SupplierItemSources);
        var supplierCapacity = CompareSupplierCapacity(
            data.SupplierCapacityWindows,
            supplyRequirements,
            Array.Empty<SupplierCapacityLimit>());
        var plan = new DemandDrivenPlanResult(
            bufferRun.BufferProjections,
            bufferRun.ReplenishmentOrders,
            capacityLoads,
            supplyRequirements,
            bufferRun.Traces);

        return Build(data, "baseline", "基准约束对比", plan, supplierCapacity);
    }

    public static ConstraintWorkspaceResult Build(
        ScenarioWorkspaceDataSet data,
        string caseId,
        string name,
        DemandDrivenPlanResult plan,
        IReadOnlyList<SupplierCapacityComparison> supplierCapacity)
    {
        var capacityCells = plan.CapacityLoads
            .Select(load =>
            {
                var variance = decimal.Round(load.RequiredCapacity - load.AvailableCapacity, 1);
                return new CapacityConstraintCell(
                    load.ResourceCode,
                    load.ResourceName,
                    load.Week,
                    load.RequiredCapacity,
                    load.AvailableCapacity,
                    variance,
                    decimal.Round(Math.Max(0m, variance), 1),
                    load.LoadPercent,
                    load.Status);
            })
            .OrderBy(item => item.ResourceCode)
            .ThenBy(item => item.Week)
            .ToList();

        var capacitySummaries = data.Resources
            .Select(resource => BuildCapacitySummary(resource, capacityCells.Where(item => item.ResourceCode == resource.Code).ToList()))
            .OrderByDescending(item => item.PeakLoadPercent)
            .ThenBy(item => item.ResourceCode)
            .ToList();

        var supplyCells = supplierCapacity
            .Select(item =>
            {
                var variance = decimal.Round(item.RequiredQuantity - item.CommittedCapacity, 0);
                var loadPercent = item.CommittedCapacity <= 0m
                    ? (item.RequiredQuantity > 0m ? 999m : 0m)
                    : decimal.Round(item.RequiredQuantity * 100m / item.CommittedCapacity, 1);
                var status = item.Gap > 0m ? "Red" : loadPercent > 85m ? "Yellow" : item.RiskStatus;
                return new SupplyConstraintCell(
                    item.Supplier,
                    item.MaterialFamily,
                    item.Week,
                    item.RequiredQuantity,
                    item.CommittedCapacity,
                    variance,
                    item.Gap,
                    loadPercent,
                    status);
            })
            .OrderBy(item => item.Supplier)
            .ThenBy(item => item.MaterialFamily)
            .ThenBy(item => item.Week)
            .ToList();

        var supplySummaries = supplyCells
            .GroupBy(item => new { item.Supplier, item.MaterialFamily })
            .Select(group => BuildSupplySummary(group.Key.Supplier, group.Key.MaterialFamily, group.ToList()))
            .OrderByDescending(item => item.TotalGap)
            .ThenBy(item => item.Supplier)
            .ThenBy(item => item.MaterialFamily)
            .ToList();

        var recommendations = BuildRecommendations(capacitySummaries, capacityCells, supplySummaries);
        var trace = BuildTrace(data, plan, capacityCells, supplyCells, recommendations);

        return new ConstraintWorkspaceResult(
            caseId,
            name,
            data.Request.HorizonWeeks,
            capacitySummaries,
            capacityCells,
            supplySummaries,
            supplyCells,
            recommendations,
            trace,
            decimal.Round(capacityCells.Sum(item => item.Gap), 1),
            decimal.Round(supplyCells.Sum(item => item.Gap), 0),
            capacityCells.Count(item => item.Status == "Red"),
            supplyCells.Count(item => item.Status == "Red"));
    }

    public static IReadOnlyList<SupplierCapacityComparison> CompareSupplierCapacity(
        IReadOnlyList<SupplierCapacityWindow> windows,
        IReadOnlyList<ProjectedSupplyRequirement> requirements,
        IReadOnlyList<SupplierCapacityLimit> limits)
    {
        return requirements.Select(requirement =>
        {
            var window = windows.FirstOrDefault(item =>
                item.Supplier == requirement.Supplier &&
                item.MaterialFamily == requirement.MaterialFamily &&
                item.Week == requirement.Week);
            var limit = limits.LastOrDefault(item =>
                item.MaterialFamily == requirement.MaterialFamily &&
                (item.Supplier == requirement.Supplier || item.Supplier == "未指定供应商") &&
                requirement.Week >= item.StartWeek &&
                requirement.Week <= item.EndWeek);
            var committed = limit?.CommittedCapacity ?? window?.CommittedCapacity ?? 0m;
            var gap = Math.Max(0, requirement.RequiredQuantity - committed);
            var risk = gap > 0
                ? "Red"
                : window?.RiskStatus ?? "Green";

            return new SupplierCapacityComparison(
                requirement.Supplier,
                requirement.MaterialFamily,
                requirement.Week,
                requirement.RequiredQuantity,
                committed,
                decimal.Round(gap, 0),
                risk);
        }).ToList();
    }

    private static CapacityConstraintSummary BuildCapacitySummary(
        CapacityResource resource,
        IReadOnlyList<CapacityConstraintCell> cells)
    {
        var average = cells.Count == 0 ? 0m : decimal.Round(cells.Average(item => item.LoadPercent), 1);
        var peak = cells.Count == 0 ? 0m : cells.Max(item => item.LoadPercent);
        var overloadWeeks = cells.Count(item => item.LoadPercent > 100m);
        var maxGap = cells.Count == 0 ? 0m : cells.Max(item => item.Gap);
        var totalGap = cells.Sum(item => item.Gap);
        var status = peak > 100m ? "Red" : peak > 85m ? "Yellow" : "Green";

        return new CapacityConstraintSummary(
            resource.Code,
            resource.Name,
            average,
            peak,
            overloadWeeks,
            decimal.Round(maxGap, 1),
            decimal.Round(totalGap, 1),
            status,
            RecommendCapacityAction(average, peak, overloadWeeks, cells));
    }

    private static SupplyConstraintSummary BuildSupplySummary(
        string supplier,
        string materialFamily,
        IReadOnlyList<SupplyConstraintCell> cells)
    {
        var totalRequired = cells.Sum(item => item.UnconstrainedRequired);
        var totalAvailable = cells.Sum(item => item.ConstrainedAvailable);
        var totalGap = cells.Sum(item => item.Gap);
        var gapWeeks = cells.Count(item => item.Gap > 0m);
        var status = totalGap > 0m ? "Red" : cells.Any(item => item.Status == "Yellow") ? "Yellow" : "Green";

        return new SupplyConstraintSummary(
            supplier,
            materialFamily,
            decimal.Round(totalRequired, 0),
            decimal.Round(totalAvailable, 0),
            decimal.Round(totalGap, 0),
            gapWeeks,
            status,
            totalGap > 0m ? "供应商协调 / 替代料 / 提前下单 / 需求取舍" : "供应能力可覆盖不受限需求");
    }

    private static IReadOnlyList<ConstraintActionRecommendation> BuildRecommendations(
        IReadOnlyList<CapacityConstraintSummary> capacitySummaries,
        IReadOnlyList<CapacityConstraintCell> capacityCells,
        IReadOnlyList<SupplyConstraintSummary> supplySummaries)
    {
        var recommendations = new List<ConstraintActionRecommendation>();

        foreach (var summary in capacitySummaries)
        {
            var resourceCells = capacityCells.Where(item => item.ResourceCode == summary.ResourceCode).OrderBy(item => item.Week).ToList();
            var overloadedWeeks = resourceCells.Where(item => item.LoadPercent > 100m).Select(item => item.Week).ToList();
            var consecutive = overloadedWeeks.Zip(overloadedWeeks.Skip(1), (left, right) => right - left).Any(delta => delta == 1);
            var hasPrebuildWindow = overloadedWeeks.Any(week => resourceCells.Any(item => item.Week < week && item.LoadPercent < 70m));

            if (consecutive || summary.PeakLoadPercent > 120m)
            {
                recommendations.Add(new("资源", summary.ResourceCode, "CapacityRelief", $"{summary.ResourceName} 连续超载或峰值超过 120%，建议增班、外协或需求取舍。", "Red"));
            }
            else if (summary.OverloadWeeks == 1 && hasPrebuildWindow)
            {
                recommendations.Add(new("资源", summary.ResourceCode, "Prebuild", $"{summary.ResourceName} 存在单周尖峰且前置周有余量，建议测试提前建库削峰。", "Yellow"));
            }
            else if (summary.AverageLoadPercent > 85m && summary.PeakLoadPercent <= 100m)
            {
                recommendations.Add(new("资源", summary.ResourceCode, "CalendarPolicy", $"{summary.ResourceName} 平均负荷偏高，建议调整资源日历或班次。", "Yellow"));
            }
        }

        foreach (var summary in supplySummaries.Where(item => item.TotalGap > 0m))
        {
            recommendations.Add(new("供应", $"{summary.Supplier} / {summary.MaterialFamily}", "SupplierCoordination", $"{summary.Supplier} 的 {summary.MaterialFamily} 存在供应缺口 {summary.TotalGap:0}，建议供应商协调、替代料、提前下单或需求取舍。", "Red"));
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add(new("全局", "约束视图", "Monitor", "受限能力可覆盖当前不受限需求，保持监控。", "Green"));
        }

        return recommendations;
    }

    private static string RecommendCapacityAction(
        decimal average,
        decimal peak,
        int overloadWeeks,
        IReadOnlyList<CapacityConstraintCell> cells)
    {
        var overloadWeekList = cells.Where(item => item.LoadPercent > 100m).Select(item => item.Week).OrderBy(item => item).ToList();
        var consecutive = overloadWeekList.Zip(overloadWeekList.Skip(1), (left, right) => right - left).Any(delta => delta == 1);

        if (consecutive || peak > 120m)
        {
            return "增班 / 外协 / 需求取舍";
        }

        if (overloadWeeks == 1 && cells.Any(item => item.LoadPercent < 70m))
        {
            return "提前建库";
        }

        if (average > 85m && peak <= 100m)
        {
            return "调整资源日历";
        }

        return peak < 70m ? "可吸收增量需求" : "持续监控";
    }

    private static IReadOnlyList<ConstraintAuditTrace> BuildTrace(
        ScenarioWorkspaceDataSet data,
        DemandDrivenPlanResult plan,
        IReadOnlyList<CapacityConstraintCell> capacityCells,
        IReadOnlyList<SupplyConstraintCell> supplyCells,
        IReadOnlyList<ConstraintActionRecommendation> recommendations)
    {
        var capacityGapWeeks = capacityCells.Count(item => item.Gap > 0m);
        var supplyGapWeeks = supplyCells.Count(item => item.Gap > 0m);
        return new List<ConstraintAuditTrace>
        {
            new("Demand", $"由 {plan.ReplenishmentOrders.Count} 条预计补货订单形成不受限资源负荷与供应需求。", "Information"),
            new("Capacity", $"读取 {data.Resources.Count} 个资源形成受限能力，资源缺口周 {capacityGapWeeks} 个。", capacityGapWeeks > 0 ? "Warning" : "Information"),
            new("Supply", $"读取 {data.SupplierCapacityWindows.Count} 条供应窗口形成受限供应能力，供应缺口周 {supplyGapWeeks} 个。", supplyGapWeeks > 0 ? "Warning" : "Information"),
            new("Action", $"触发 {recommendations.Count} 条约束动作建议。", recommendations.Any(item => item.Severity == "Red") ? "Warning" : "Information")
        };
    }
}
