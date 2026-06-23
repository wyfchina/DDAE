namespace AdaptiveSopDdsop.Web.Domain;

public sealed class RccpWorkspaceService
{
    private readonly IScenarioWorkspaceDataSource _dataSource;

    public RccpWorkspaceService(IScenarioWorkspaceDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public RccpWorkspaceResult GetBaseline(int horizonWeeks)
    {
        var horizon = Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52);
        var data = _dataSource.Load(new ScenarioWorkspaceDataRequest(horizon, new DateOnly(2026, 6, 1)));
        var bufferRun = DemandDrivenPlanningEngine.ProjectBuffers(data.Skus, data.Inventory, data.Demand, horizon);
        var capacityLoads = DemandDrivenPlanningEngine.ProjectRoughCutCapacity(
            bufferRun.ReplenishmentOrders,
            data.ResourceRoutings,
            data.Resources,
            horizon);
        var plan = new DemandDrivenPlanResult(
            bufferRun.BufferProjections,
            bufferRun.ReplenishmentOrders,
            capacityLoads,
            DemandDrivenPlanningEngine.ProjectSupplyRequirements(bufferRun.ReplenishmentOrders, data.SupplierItemSources),
            bufferRun.Traces);

        return Build(data, "baseline", "Baseline RCCP", plan);
    }

    public static RccpWorkspaceResult Build(
        ScenarioWorkspaceDataSet data,
        string caseId,
        string name,
        DemandDrivenPlanResult plan)
    {
        var cells = plan.CapacityLoads
            .Select(load => new RccpWeeklyCell(
                load.ResourceCode,
                load.ResourceName,
                load.Week,
                load.RequiredCapacity,
                load.AvailableCapacity,
                decimal.Round(load.RequiredCapacity - load.AvailableCapacity, 1),
                load.LoadPercent,
                load.Status))
            .OrderBy(cell => cell.ResourceCode)
            .ThenBy(cell => cell.Week)
            .ToList();

        var summaries = data.Resources
            .Select(resource => BuildSummary(resource, cells.Where(cell => cell.ResourceCode == resource.Code).ToList()))
            .OrderByDescending(summary => summary.PeakLoadPercent)
            .ThenBy(summary => summary.ResourceCode)
            .ToList();

        var details = data.Resources
            .Select(resource => BuildDetail(data, resource, cells, plan.ReplenishmentOrders))
            .ToList();

        var redWeeks = cells.Count(cell => cell.LoadPercent > 100m);
        var maxGap = cells.Count == 0 ? 0m : cells.Max(cell => Math.Max(0m, cell.Variance));
        var releasable = cells.Sum(cell => Math.Max(0m, cell.AvailableCapacity * 0.85m - cell.RequiredCapacity));

        return new RccpWorkspaceResult(
            caseId,
            name,
            data.Request.HorizonWeeks,
            summaries,
            cells,
            details,
            summaries.Count == 0 ? 0m : summaries.Max(item => item.PeakLoadPercent),
            summaries.Count == 0 ? 0m : decimal.Round(summaries.Average(item => item.AverageLoadPercent), 1),
            summaries.Count(item => item.Status == "Red"),
            redWeeks,
            decimal.Round(maxGap, 1),
            decimal.Round(releasable, 1),
            decimal.Round(cells.Sum(cell => Math.Max(0m, cell.Variance)), 1));
    }

    public static RccpComparison Compare(RccpWorkspaceResult baseline, RccpWorkspaceResult scenario)
    {
        return new RccpComparison(
            decimal.Round(scenario.MaxPeakLoadPercent - baseline.MaxPeakLoadPercent, 1),
            decimal.Round(scenario.AverageLoadPercent - baseline.AverageLoadPercent, 1),
            scenario.RedWeekCount - baseline.RedWeekCount,
            decimal.Round(scenario.ConstrainedGap - baseline.ConstrainedGap, 1));
    }

    private static RccpResourceSummary BuildSummary(CapacityResource resource, IReadOnlyList<RccpWeeklyCell> cells)
    {
        var average = cells.Count == 0 ? 0m : decimal.Round(cells.Average(item => item.LoadPercent), 1);
        var peak = cells.Count == 0 ? 0m : cells.Max(item => item.LoadPercent);
        var overloadWeeks = cells.Count(item => item.LoadPercent > 100m);
        var maxGap = cells.Count == 0 ? 0m : cells.Max(item => Math.Max(0m, item.Variance));
        var status = peak > 100m ? "Red" : peak > 85m ? "Yellow" : "Green";

        return new RccpResourceSummary(
            resource.Code,
            resource.Name,
            ResourceType(resource),
            average,
            peak,
            overloadWeeks,
            decimal.Round(maxGap, 1),
            status,
            RecommendSummaryAction(average, peak, overloadWeeks, cells));
    }

    private static RccpResourceDetail BuildDetail(
        ScenarioWorkspaceDataSet data,
        CapacityResource resource,
        IReadOnlyList<RccpWeeklyCell> cells,
        IReadOnlyList<ProjectedReplenishmentOrder> orders)
    {
        var skuMap = data.Skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);
        var resourceRoutings = data.ResourceRoutings
            .Where(item => item.ResourceCode == resource.Code)
            .ToList();
        var contributions = orders
            .Join(
                resourceRoutings,
                order => order.Sku,
                routing => routing.Sku,
                (order, routing) =>
                {
                    skuMap.TryGetValue(order.Sku, out var sku);
                    return new RccpSkuContribution(
                        resource.Code,
                        order.Sku,
                        sku?.Name ?? order.Sku,
                        sku?.Family ?? "未分类",
                        order.Week,
                        order.Quantity,
                        routing.CapacityPerUnit,
                        decimal.Round(order.Quantity * routing.CapacityPerUnit, 1),
                        order.Trigger);
                })
            .OrderByDescending(item => item.RequiredCapacity)
            .ThenBy(item => item.Week)
            .ToList();
        var weekly = cells
            .Where(item => item.ResourceCode == resource.Code)
            .OrderBy(item => item.Week)
            .ToList();

        return new RccpResourceDetail(
            resource.Code,
            resource.Name,
            weekly,
            contributions,
            orders.Where(order => resourceRoutings.Any(routing => routing.Sku == order.Sku)).OrderBy(order => order.Week).ToList(),
            BuildRecommendations(resource.Code, weekly));
    }

    private static IReadOnlyList<RccpActionRecommendation> BuildRecommendations(
        string resourceCode,
        IReadOnlyList<RccpWeeklyCell> weekly)
    {
        if (weekly.Count == 0)
        {
            return Array.Empty<RccpActionRecommendation>();
        }

        var recommendations = new List<RccpActionRecommendation>();
        var peak = weekly.Max(item => item.LoadPercent);
        var average = weekly.Average(item => item.LoadPercent);
        var overloadWeeks = weekly.Where(item => item.LoadPercent > 100m).Select(item => item.Week).OrderBy(item => item).ToList();
        var hasConsecutiveOverload = overloadWeeks.Zip(overloadWeeks.Skip(1), (left, right) => right - left).Any(delta => delta == 1);
        var hasPrebuildWindow = overloadWeeks.Any(week => weekly.Any(item => item.Week < week && item.LoadPercent < 70m));

        if (hasConsecutiveOverload || peak > 120m)
        {
            recommendations.Add(new(resourceCode, "CapacityRelief", "连续超载或峰值超过 120%，建议评估增班、外协或需求取舍。", "Red"));
        }

        if (overloadWeeks.Count == 1 && hasPrebuildWindow)
        {
            recommendations.Add(new(resourceCode, "Prebuild", "存在单周尖峰且前置周有余量，建议测试提前建库削峰。", "Yellow"));
        }

        if (average > 85m && peak <= 100m)
        {
            recommendations.Add(new(resourceCode, "CalendarPolicy", "平均负载偏高但峰值未超载，建议调整资源日历、班次或维护窗口。", "Yellow"));
        }

        if (peak < 70m)
        {
            recommendations.Add(new(resourceCode, "DemandUpside", "负载低于 70%，在缓冲健康前提下可吸收增量需求。", "Green"));
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add(new(resourceCode, "Monitor", "资源负载处于可控区间，保持监控并关注周度波动。", "Green"));
        }

        return recommendations;
    }

    private static string RecommendSummaryAction(
        decimal average,
        decimal peak,
        int overloadWeeks,
        IReadOnlyList<RccpWeeklyCell> cells)
    {
        var hasConsecutiveOverload = cells
            .Where(item => item.LoadPercent > 100m)
            .Select(item => item.Week)
            .OrderBy(item => item)
            .Zip(cells.Where(item => item.LoadPercent > 100m).Select(item => item.Week).OrderBy(item => item).Skip(1), (left, right) => right - left)
            .Any(delta => delta == 1);

        if (hasConsecutiveOverload || peak > 120m)
        {
            return "增班 / 外协 / 需求取舍";
        }

        if (overloadWeeks == 1 && cells.Any(item => item.LoadPercent < 70m))
        {
            return "提前建库削峰";
        }

        if (average > 85m && peak <= 100m)
        {
            return "调整日历与班次";
        }

        if (peak < 70m)
        {
            return "可吸收增量需求";
        }

        return "持续监控";
    }

    private static string ResourceType(CapacityResource resource)
    {
        if (resource.Code.Contains("TVAC", StringComparison.OrdinalIgnoreCase) || resource.Name.Contains("试验", StringComparison.OrdinalIgnoreCase))
        {
            return "试验资源";
        }

        if (resource.Name.Contains("装配", StringComparison.OrdinalIgnoreCase) || resource.Name.Contains("集成", StringComparison.OrdinalIgnoreCase))
        {
            return "装配集成";
        }

        if (resource.Name.Contains("工位", StringComparison.OrdinalIgnoreCase))
        {
            return "工位资源";
        }

        return "关键资源";
    }
}
