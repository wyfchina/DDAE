namespace AdaptiveSopDdsop.Web.Domain;

public sealed record HistoryOperatingOutcomes(
    decimal ServiceLevelPercent,
    decimal InventoryValue,
    decimal WorkInProcessUnits,
    decimal AverageFlowTimeDays,
    decimal CashOccupied,
    decimal ExpediteCost,
    decimal RemainingProtectionPercent);

public sealed record ControlPointProtectionRelationship(
    string ControlPoint,
    string ProtectedObject,
    string ProtectionType,
    string DesignStatus,
    string AvailabilityStatus,
    string EffectivenessStatus,
    string Evidence);

public sealed record BufferZoneResidenceSummary(
    string Sku,
    string Name,
    int ObservedPeriods,
    int RedPeriods,
    int YellowPeriods,
    int GreenPeriods,
    int OverTopOfGreenPeriods,
    decimal RedPercent,
    decimal YellowPercent,
    decimal GreenPercent,
    decimal OverTopOfGreenPercent,
    int RedEntryCount,
    int MaximumRedStreak,
    int? RecoveryPeriods,
    string PrimaryCause);

public sealed record CapacityProtectionLayer(
    string ResourceCode,
    string ResourceName,
    decimal TheoreticalCapacity,
    decimal StandardCapacity,
    decimal DemonstratedCapacity,
    decimal PlannedAvailableCapacity,
    decimal CommittedLoad,
    decimal ProtectiveCapacity,
    decimal ConsumedProtection,
    decimal RemainingProtection,
    string LossReason);

public sealed record ConstraintExposureItem(
    string ExposureType,
    string Target,
    string Status,
    decimal LoadPercent,
    string Evidence);

public sealed record HistoryReviewWorkspace(
    int MaximumCumulativeLeadTimeDays,
    int DetailWindowWeeks,
    int TrendMonths,
    int ObservedTrendWeeks,
    HistoryOperatingOutcomes OperatingOutcomes,
    IReadOnlyList<ControlPointProtectionRelationship> ProtectionRelationships,
    IReadOnlyList<BufferZoneResidenceSummary> ZoneResidence,
    IReadOnlyList<CapacityProtectionLayer> CapacityProtection,
    IReadOnlyList<ConstraintExposureItem> ConstraintExposure,
    string EvidenceLabel);

public sealed class HistoryReviewWorkspaceService
{
    private readonly IScenarioWorkspaceDataSource _dataSource;

    public HistoryReviewWorkspaceService(IScenarioWorkspaceDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public HistoryReviewWorkspace GetReview(int trendMonths = 6)
    {
        var normalizedMonths = trendMonths >= 12 ? 12 : 6;
        var requestedWeeks = normalizedMonths == 12 ? 52 : 26;
        var data = _dataSource.Load(new ScenarioWorkspaceDataRequest(requestedWeeks, new DateOnly(2026, 6, 1)));
        var maximumLeadTime = data.DdmrpParameters.Count == 0 ? 7 : data.DdmrpParameters.Max(item => item.DecoupledLeadTimeDays);
        var detailWeeks = Math.Clamp((int)Math.Ceiling(maximumLeadTime / 7m), 1, requestedWeeks);
        var zoneResidence = BuildZoneResidence(data, detailWeeks);
        var capacity = BuildCapacityProtection(data, detailWeeks);
        var relationships = BuildProtectionRelationships(data, capacity);
        var exposure = BuildConstraintExposure(data, capacity);

        var unitCost = data.Skus.ToDictionary(item => item.Sku, item => item.UnitCost, StringComparer.Ordinal);
        var inventoryValue = data.Inventory.Sum(item => item.OnHand * unitCost.GetValueOrDefault(item.Sku));
        var wip = data.Inventory.Sum(item => item.OpenSupply);
        var service = data.HistoricalDemand.Count == 0 ? 0m : decimal.Round(data.HistoricalDemand.Average(item => item.ServiceLevelPercent), 1);
        var remainingProtection = capacity.Count == 0 || capacity.Sum(item => item.ProtectiveCapacity) == 0
            ? 0m
            : decimal.Round(capacity.Sum(item => item.RemainingProtection) * 100m / capacity.Sum(item => item.ProtectiveCapacity), 1);
        var averageFlowTime = data.DdmrpParameters.Count == 0
            ? 0m
            : decimal.Round(data.DdmrpParameters.Average(item => (decimal)item.DecoupledLeadTimeDays), 1);
        var outcomes = new HistoryOperatingOutcomes(
            service,
            decimal.Round(inventoryValue, 0),
            decimal.Round(wip, 0),
            averageFlowTime,
            decimal.Round(inventoryValue, 0),
            decimal.Round(data.SupplierCapacityWindows.Count(item => item.RiskStatus == "Red") * 2500m, 0),
            remainingProtection);

        return new HistoryReviewWorkspace(
            maximumLeadTime,
            detailWeeks,
            normalizedMonths,
            data.HistoricalDemand.Select(item => Math.Abs(item.WeekOffset)).DefaultIfEmpty(0).Max(),
            outcomes,
            relationships,
            zoneResidence,
            capacity,
            exposure,
            "DemoFixture");
    }

    private static IReadOnlyList<BufferZoneResidenceSummary> BuildZoneResidence(ScenarioWorkspaceDataSet data, int detailWeeks)
    {
        var parameters = data.DdmrpParameters.ToDictionary(item => item.Sku, StringComparer.Ordinal);
        return data.HistoricalDemand
            .Where(item => Math.Abs(item.WeekOffset) <= detailWeeks)
            .GroupBy(item => item.Sku)
            .Select(group =>
            {
                var parameter = parameters[group.Key];
                var ordered = group.OrderBy(item => item.WeekOffset).ToList();
                var statuses = ordered.Select(item => Zone(item.EndingNetFlow, parameter)).ToList();
                var maxRedStreak = MaximumStreak(statuses, "Red");
                var redEntries = Enumerable.Range(0, statuses.Count)
                    .Where(index => statuses[index] == "Red" && (index == 0 || statuses[index - 1] != "Red"))
                    .ToList();
                var lastRedEntry = redEntries.LastOrDefault(-1);
                var recoveryIndex = lastRedEntry < 0
                    ? -1
                    : Enumerable.Range(lastRedEntry + 1, Math.Max(0, statuses.Count - lastRedEntry - 1))
                        .FirstOrDefault(index => statuses[index] is "Green" or "OverTopOfGreen", -1);
                int? recovery = recoveryIndex < 0 ? null : recoveryIndex - lastRedEntry + 1;
                var cause = ordered.Any(item => item.ActualDemand > item.ForecastDemand * 1.1m)
                    ? "需求高于预测"
                    : "补充与保护能力偏差";
                decimal Share(string status) => statuses.Count == 0
                    ? 0m
                    : decimal.Round(statuses.Count(item => item == status) * 100m / statuses.Count, 1);
                return new BufferZoneResidenceSummary(
                    group.Key,
                    parameter.Name,
                    statuses.Count,
                    statuses.Count(item => item == "Red"),
                    statuses.Count(item => item == "Yellow"),
                    statuses.Count(item => item == "Green"),
                    statuses.Count(item => item == "OverTopOfGreen"),
                    Share("Red"),
                    Share("Yellow"),
                    Share("Green"),
                    Share("OverTopOfGreen"),
                    redEntries.Count,
                    maxRedStreak,
                    recovery,
                    cause);
            })
            .OrderByDescending(item => item.RedPeriods)
            .ThenBy(item => item.Sku)
            .ToList();
    }

    private static IReadOnlyList<CapacityProtectionLayer> BuildCapacityProtection(ScenarioWorkspaceDataSet data, int detailWeeks)
    {
        var demandBySku = data.Demand
            .Where(item => item.Week <= detailWeeks)
            .GroupBy(item => item.Sku)
            .ToDictionary(group => group.Key, group => group.Average(item => item.BaselineDemand), StringComparer.Ordinal);

        return data.Resources.Select(resource =>
        {
            var standard = resource.WeeklyAvailableUnits;
            var theoretical = decimal.Round(standard * 1.10m, 1);
            var calendar = data.ResourceCalendar.Where(item => item.ResourceCode == resource.Code && item.Week <= detailWeeks).ToList();
            var demonstrated = decimal.Round(standard * (calendar.Count == 0 ? 1m : calendar.Average(item => Math.Min(item.CapacityMultiplier, 1m))) * 0.97m, 1);
            var planned = decimal.Round(standard * (calendar.Count == 0 ? 1m : calendar.Average(item => item.CapacityMultiplier)), 1);
            var committed = data.ResourceRoutings
                .Where(item => item.ResourceCode == resource.Code)
                .Sum(item => demandBySku.GetValueOrDefault(item.Sku) * item.CapacityPerUnit);
            committed = decimal.Round(committed, 1);
            var protective = decimal.Round(Math.Max(0m, planned - committed * 0.85m), 1);
            var consumed = decimal.Round(Math.Min(protective, Math.Max(0m, committed - demonstrated * 0.85m)), 1);
            var remaining = decimal.Round(Math.Max(0m, protective - consumed), 1);
            var loss = calendar.Any(item => item.CapacityMultiplier < 1m)
                ? string.Join("；", calendar.Where(item => item.CapacityMultiplier < 1m).Select(item => item.CalendarNote).Distinct())
                : "无已知能力损失";
            return new CapacityProtectionLayer(resource.Code, resource.Name, theoretical, standard, demonstrated, planned, committed, protective, consumed, remaining, loss);
        }).ToList();
    }

    private static IReadOnlyList<ControlPointProtectionRelationship> BuildProtectionRelationships(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<CapacityProtectionLayer> capacity)
    {
        var results = data.DdmrpParameters.Select(item => new ControlPointProtectionRelationship(
            item.DecouplingPoint,
            item.Sku,
            "库存缓冲",
            string.IsNullOrWhiteSpace(item.BufferProfile) ? "证据缺失" : "有保护设计",
            item.TopOfGreen > 0 ? "保护可用" : "证据缺失",
            item.CompletenessStatus == "Complete" ? "保护有效" : "待验证",
            $"{item.BufferProfile} / {item.ParameterStatus}"))
            .ToList();

        results.AddRange(data.MasterSettings
            .Where(item => item.SettingType == "Time Buffer")
            .Select(item => new ControlPointProtectionRelationship(item.Target, item.Target, "时间缓冲", "有保护设计", "保护可用", item.Status == "Effective" ? "保护有效" : "待验证", item.CurrentValue)));
        results.AddRange(capacity.Select(item => new ControlPointProtectionRelationship(
            item.ResourceCode,
            item.ResourceName,
            "产能保护",
            item.ProtectiveCapacity > 0 ? "有保护设计" : "证据缺失",
            item.RemainingProtection > 0 ? "保护可用" : "保护不足",
            item.CommittedLoad <= item.DemonstratedCapacity ? "保护有效" : "保护被穿透",
            item.LossReason)));
        return results;
    }

    private static IReadOnlyList<ConstraintExposureItem> BuildConstraintExposure(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<CapacityProtectionLayer> capacity)
    {
        var items = capacity.Select(item =>
        {
            var load = item.DemonstratedCapacity == 0 ? 0m : decimal.Round(item.CommittedLoad * 100m / item.DemonstratedCapacity, 1);
            var type = load >= 100m ? "当前 CCR" : load >= 90m ? "高负荷资源" : item.LossReason != "无已知能力损失" ? "事件型约束" : "受保护资源";
            var status = load >= 100m ? "Red" : load >= 90m ? "Yellow" : "Green";
            return new ConstraintExposureItem(type, item.ResourceName, status, load, item.LossReason);
        }).ToList();
        items.AddRange(data.SupplierCapacityWindows
            .Where(item => item.RiskStatus is "Red" or "Yellow")
            .GroupBy(item => new { item.Supplier, item.MaterialFamily, item.RiskStatus })
            .Select(group => new ConstraintExposureItem("外部约束", $"{group.Key.Supplier} / {group.Key.MaterialFamily}", group.Key.RiskStatus, 0m, "供应承诺与提前期证据")));
        items.AddRange(data.ScenarioTemplates
            .SelectMany(template => template.Actions
                .Where(action => action.ActionType == "CapacityMultiplier" && action.Value < 1m)
                .Select(action => new { Template = template.Name, Action = action }))
            .GroupBy(item => item.Action.Target, StringComparer.Ordinal)
            .Select(group =>
            {
                var resourceName = data.Resources.FirstOrDefault(item => item.Code == group.Key)?.Name ?? group.Key;
                return new ConstraintExposureItem(
                    "场景潜在 CCR",
                    resourceName,
                    "Yellow",
                    0m,
                    string.Join("；", group.Select(item => $"{item.Template}：能力倍率 {item.Action.Value:0.00}").Distinct()));
            }));
        return items;
    }

    private static string Zone(decimal netFlow, DdmrpParameterProfile parameter) => netFlow <= parameter.TopOfRed
        ? "Red"
        : netFlow <= parameter.TopOfYellow
            ? "Yellow"
            : netFlow <= parameter.TopOfGreen
                ? "Green"
                : "OverTopOfGreen";

    private static int MaximumStreak(IReadOnlyList<string> values, string target)
    {
        var current = 0;
        var maximum = 0;
        foreach (var value in values)
        {
            current = value == target ? current + 1 : 0;
            maximum = Math.Max(maximum, current);
        }
        return maximum;
    }
}
