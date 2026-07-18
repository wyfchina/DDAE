namespace AdaptiveSopDdsop.Web.Domain;

public sealed record HistoryOperatingOutcomes(
    decimal? ServiceLevelPercent,
    decimal? InventoryValue,
    decimal? WorkInProcessUnits,
    decimal? AverageFlowTimeDays,
    decimal? CashOccupied,
    decimal? ExpediteCost,
    decimal? RemainingProtectionPercent,
    string EvidenceStatus);

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
    string? ProtectedCcrResourceCode,
    string RelationshipRole,
    decimal? TheoreticalCapacity,
    decimal? StandardCapacity,
    decimal? DemonstratedCapacity,
    decimal? PlannedAvailableCapacity,
    decimal? CommittedLoad,
    decimal? ProtectiveCapacity,
    decimal? ConsumedProtection,
    decimal? RemainingProtection,
    string LossReason,
    string EvidenceStatus);

public sealed record ConstraintExposureItem(
    string ExposureType,
    string Target,
    string Status,
    decimal? LoadPercent,
    string Evidence,
    string EvidenceStatus);

public sealed record HistoryReviewWorkspace(
    int? MaximumCumulativeLeadTimeDays,
    int? DetailWindowWeeks,
    int TrendMonths,
    int ObservedTrendWeeks,
    HistoryOperatingOutcomes OperatingOutcomes,
    IReadOnlyList<ControlPointProtectionRelationship> ProtectionRelationships,
    IReadOnlyList<BufferZoneResidenceSummary> ZoneResidence,
    IReadOnlyList<CapacityProtectionLayer> CapacityProtection,
    IReadOnlyList<ConstraintExposureItem> ConstraintExposure,
    string EvidenceLabel,
    IReadOnlyList<HistoryInventoryBufferView>? InventoryBuffers = null,
    IReadOnlyList<HistoryDdmrpSizingSnapshotView>? DdmrpSizingSnapshots = null,
    IReadOnlyList<HistoryTimeBufferView>? TimeBuffers = null,
    IReadOnlyList<HistoryCapacityBufferView>? CapacityBuffers = null,
    HistoryDdmrpSizingSnapshotView? StandardDdmrpReference = null,
    string FactSetId = "",
    string HistoryThroughUtc = "");

public sealed class HistoryReviewWorkspaceService
{
    private const string Complete = "Complete";
    private const string EvidenceMissing = "EvidenceMissing";

    private readonly IHistoryOperatingFactSource _historyFactSource;
    private readonly IScenarioWorkspaceDataSource _scenarioDataSource;

    public HistoryReviewWorkspaceService(
        IHistoryOperatingFactSource historyFactSource,
        IScenarioWorkspaceDataSource scenarioDataSource)
    {
        _historyFactSource = historyFactSource;
        _scenarioDataSource = scenarioDataSource;
    }

    public HistoryReviewWorkspace GetReview(int trendMonths = 6)
    {
        var normalizedMonths = trendMonths >= 12 ? 12 : 6;
        var requestedWeeks = normalizedMonths == 12 ? 52 : 26;
        var asOfDate = new DateOnly(2026, 6, 30);
        var annualHistory = _historyFactSource.Load(new HistoryFactRequest(52, asOfDate));
        var history = SliceAnnualHistory(annualHistory, requestedWeeks);
        var definitions = _scenarioDataSource.Load(new ScenarioWorkspaceDataRequest(requestedWeeks, asOfDate));
        var historicalParameters = history.DdmrpParameterFacts ?? Array.Empty<HistoricalDdmrpParameterFact>();
        var maximumLeadTime = historicalParameters
            .Where(item =>
                HistoryReviewProjectionBuilder.HasCompleteSizingEvidence(item) &&
                item.EffectiveFromWeekOffset <= -1 &&
                item.EffectiveThroughWeekOffset >= -requestedWeeks)
            .Select(item => (int?)item.Setting.DecoupledLeadTimeDays)
            .Max();
        var detailWeeks = maximumLeadTime.HasValue
            ? Math.Clamp((int)Math.Ceiling(maximumLeadTime.Value / 7m), 1, requestedWeeks)
            : (int?)null;
        var projectionDetailWeeks = detailWeeks ?? requestedWeeks;
        var projection = HistoryReviewProjectionBuilder.Build(
            history,
            definitions,
            projectionDetailWeeks,
            annualHistory.BufferFacts,
            annualHistory.AbnormalCosts);
        var zoneResidence = BuildZoneResidence(projection.InventoryBuffers, projectionDetailWeeks);
        var capacity = BuildCapacityProtection(projection.CapacityBuffers, history.CapacityFacts);
        var relationships = BuildProtectionRelationships(projection, capacity, definitions.Resources);
        var exposure = BuildConstraintExposure(history.ConstraintFacts, definitions.Resources);
        var outcomes = BuildOperatingOutcomes(
            history,
            annualHistory.AbnormalCosts,
            capacity);
        var observedTrendWeeks = history.OperatingFacts
            .Select(item => item.WeekOffset)
            .Distinct()
            .Count();

        return new HistoryReviewWorkspace(
            maximumLeadTime,
            detailWeeks,
            normalizedMonths,
            observedTrendWeeks,
            outcomes,
            relationships,
            zoneResidence,
            capacity,
            exposure,
            $"{history.EvidenceLabel} / SelectedObjectDetail={(detailWeeks.HasValue ? $"{detailWeeks.Value}-week cumulative-lead-time" : "EvidenceMissing")} / SourceAuthority={history.SourceAuthority} / AsOf={history.AsOfUtc}",
            projection.InventoryBuffers,
            projection.DdmrpSizingSnapshots,
            projection.TimeBuffers,
            projection.CapacityBuffers,
            FactSetId: history.FactSetId,
            HistoryThroughUtc: history.AsOfUtc);
    }

    private static HistoryFactSet SliceAnnualHistory(HistoryFactSet annual, int requestedWeeks)
    {
        var weeks = Math.Clamp(requestedWeeks, 1, 52);
        bool InWindow(int weekOffset) => weekOffset is < 0 && weekOffset >= -weeks;
        bool HistoricalRangeOverlapsWindow(int effectiveFromWeekOffset, int effectiveThroughWeekOffset) =>
            effectiveFromWeekOffset <= effectiveThroughWeekOffset &&
            effectiveFromWeekOffset < 0 &&
            effectiveThroughWeekOffset < 0 &&
            effectiveThroughWeekOffset >= -weeks;
        var trendLabel = weeks == 52
            ? "TrendWindow=annual-52"
            : $"TrendWindow=trailing-{weeks}-of-52";

        return annual with
        {
            Request = annual.Request with { Weeks = weeks },
            OperatingFacts = annual.OperatingFacts.Where(item => InWindow(item.WeekOffset)).ToList(),
            BufferFacts = annual.BufferFacts.Where(item => InWindow(item.WeekOffset)).ToList(),
            CapacityFacts = annual.CapacityFacts.Where(item => InWindow(item.WeekOffset)).ToList(),
            ConstraintFacts = annual.ConstraintFacts
                .Where(item => item.WeekOffset is null || InWindow(item.WeekOffset.Value))
                .ToList(),
            AbnormalCosts = annual.AbnormalCosts.Where(item => InWindow(item.WeekOffset)).ToList(),
            EvidenceLabel = $"{annual.EvidenceLabel} / {trendLabel}",
            TimeBufferFacts = (annual.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>())
                .Where(item => InWindow(item.WeekOffset))
                .ToList(),
            DdmrpParameterFacts = (annual.DdmrpParameterFacts ?? Array.Empty<HistoricalDdmrpParameterFact>())
                .Where(item => HistoricalRangeOverlapsWindow(item.EffectiveFromWeekOffset, item.EffectiveThroughWeekOffset))
                .ToList(),
            CapacityProtectionFacts = (annual.CapacityProtectionFacts ?? Array.Empty<HistoricalCapacityProtectionFact>())
                .Where(item => HistoricalRangeOverlapsWindow(item.EffectiveFromWeekOffset, item.EffectiveThroughWeekOffset))
                .ToList(),
        };
    }

    private static HistoryOperatingOutcomes BuildOperatingOutcomes(
        HistoryFactSet history,
        IReadOnlyList<HistoryAbnormalCostEvent> annualAbnormalCostLedger,
        IReadOnlyList<CapacityProtectionLayer> capacity)
    {
        var service = AverageOrNull(history.OperatingFacts, item => item.ServiceLevelPercent, 1);
        var inventory = AverageOrNull(history.OperatingFacts, item => item.InventoryValue, 0);
        var workInProcess = AverageOrNull(history.OperatingFacts, item => item.WorkInProcessUnits, 1);
        var flowTime = AverageOrNull(history.OperatingFacts, item => item.AverageFlowTimeDays, 1);
        var cash = AverageOrNull(history.OperatingFacts, item => item.CashOccupied, 0);
        var costEvents = HistoryReviewProjectionBuilder.SelectValidAbnormalCostEvents(
            history.AbnormalCosts,
            annualAbnormalCostLedger);
        decimal? abnormalCost = costEvents.Count == 0
            ? null
            : decimal.Round(costEvents.Sum(item => item.CostAmount), 0);

        var protectedLayers = capacity
            .Where(item =>
                item.RelationshipRole == "UpstreamProtection" &&
                item.EvidenceStatus == Complete &&
                item.ProtectiveCapacity is > 0 &&
                item.RemainingProtection is not null)
            .ToList();
        decimal? remainingProtectionPercent = protectedLayers.Count == 0
            ? null
            : decimal.Round(
                protectedLayers.Sum(item => item.RemainingProtection!.Value) * 100m /
                protectedLayers.Sum(item => item.ProtectiveCapacity!.Value),
                1);

        var evidenceStatus = new[]
        {
            service,
            inventory,
            workInProcess,
            flowTime,
            cash,
            abnormalCost,
            remainingProtectionPercent,
        }.All(item => item is not null)
            ? Complete
            : EvidenceMissing;

        return new HistoryOperatingOutcomes(
            service,
            inventory,
            workInProcess,
            flowTime,
            cash,
            abnormalCost,
            remainingProtectionPercent,
            evidenceStatus);
    }

    private static decimal? AverageOrNull(
        IReadOnlyList<WeeklyOperatingFact> facts,
        Func<WeeklyOperatingFact, decimal?> selector,
        int decimals)
    {
        var values = facts
            .Where(item => item.EvidenceStatus == Complete)
            .Select(selector)
            .Where(item => item.HasValue)
            .Select(item => item!.Value)
            .ToList();
        return values.Count == 0 ? null : decimal.Round(values.Average(), decimals);
    }

    private static IReadOnlyList<BufferZoneResidenceSummary> BuildZoneResidence(
        IReadOnlyList<HistoryInventoryBufferView> inventory,
        int detailWeeks)
    {
        return inventory
            .Select(view =>
            {
                var ordered = view.Points
                    .Where(item =>
                        item.WeekOffset < 0 &&
                        Math.Abs(item.WeekOffset) <= detailWeeks &&
                        item.EvidenceStatus == Complete)
                    .OrderBy(item => item.WeekOffset)
                    .ToList();
                if (ordered.Count == 0)
                {
                    return null;
                }

                var statuses = ordered
                    .Select(item => item.Status)
                    .ToList();
                var redEntries = Enumerable.Range(0, statuses.Count)
                    .Where(index => statuses[index] == "Red" && (index == 0 || statuses[index - 1] != "Red"))
                    .ToList();
                var lastRedEntry = redEntries.LastOrDefault(-1);
                var recoveryIndex = lastRedEntry < 0
                    ? -1
                    : Enumerable.Range(lastRedEntry + 1, Math.Max(0, statuses.Count - lastRedEntry - 1))
                        .FirstOrDefault(index => statuses[index] is "Green" or "OverTopOfGreen", -1);
                int? recovery = recoveryIndex < 0 ? null : recoveryIndex - lastRedEntry;
                var primaryCause = ordered
                    .Where(item => !string.IsNullOrWhiteSpace(item.Cause))
                    .GroupBy(item => item.Cause, StringComparer.Ordinal)
                    .OrderByDescending(causeGroup => causeGroup.Count())
                    .ThenBy(causeGroup => causeGroup.Key, StringComparer.Ordinal)
                    .Select(causeGroup => causeGroup.Key)
                    .FirstOrDefault() ?? "证据缺失";
                decimal Share(string status) => statuses.Count == 0
                    ? 0m
                    : decimal.Round(statuses.Count(item => item == status) * 100m / statuses.Count, 1);

                return new BufferZoneResidenceSummary(
                    view.Sku,
                    view.Name,
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
                    MaximumStreak(statuses, "Red"),
                    recovery,
                    primaryCause);
            })
            .OfType<BufferZoneResidenceSummary>()
            .OrderByDescending(item => item.RedPeriods)
            .ThenBy(item => item.Sku, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<CapacityProtectionLayer> BuildCapacityProtection(
        IReadOnlyList<HistoryCapacityBufferView> capacityViews,
        IReadOnlyList<WeeklyCapacityFact> facts)
    {
        return capacityViews
            .Select(view =>
            {
                var completePoints = view.Points
                    .Where(item => item.EvidenceStatus == Complete)
                    .ToList();
                var lossReasons = facts
                    .Where(item =>
                        item.ResourceCode == view.ResourceCode &&
                        item.EvidenceStatus == Complete &&
                        !string.IsNullOrWhiteSpace(item.LossReason))
                    .Select(item => item.LossReason)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(item => item, StringComparer.Ordinal)
                    .ToList();
                var theoretical = CapacityPointAverage(completePoints, item => item.TheoreticalCapacity);
                var standard = CapacityPointAverage(completePoints, item => item.StandardCapacity);
                var demonstrated = CapacityPointAverage(completePoints, item => item.DemonstratedCapacity);
                var planned = CapacityPointAverage(completePoints, item => item.PlannedAvailableCapacity);
                var committed = CapacityPointAverage(completePoints, item => item.CommittedLoad);
                var protective = CapacityPointAverage(completePoints, item => item.ProtectiveCapacity);
                var consumed = CapacityPointAverage(completePoints, item => item.ConsumedProtection);
                var remaining = CapacityPointAverage(completePoints, item => item.RemainingProtection);

                return new CapacityProtectionLayer(
                    view.ResourceCode,
                    view.ResourceName,
                    view.ProtectedCcrResourceCode,
                    view.RelationshipRole,
                    theoretical,
                    standard,
                    demonstrated,
                    planned,
                    committed,
                    protective,
                    consumed,
                    remaining,
                    lossReasons.Count == 0 ? "证据缺失" : string.Join("；", lossReasons),
                    view.EvidenceStatus);
            })
            .ToList();
    }

    private static decimal? CapacityPointAverage(
        IReadOnlyList<HistoryCapacityPoint> points,
        Func<HistoryCapacityPoint, decimal?> selector)
    {
        var values = points
            .Select(selector)
            .Where(item => item.HasValue)
            .Select(item => item!.Value)
            .ToList();
        return values.Count == 0 ? null : decimal.Round(values.Average(), 1);
    }

    private static IReadOnlyList<ControlPointProtectionRelationship> BuildProtectionRelationships(
        HistoryReviewProjection projection,
        IReadOnlyList<CapacityProtectionLayer> capacity,
        IReadOnlyList<CapacityResource> resources)
    {
        var results = projection.InventoryBuffers
            .Select(view =>
            {
                var snapshotIds = view.Points
                    .Where(item => !string.IsNullOrWhiteSpace(item.ParameterSnapshotId))
                    .Select(item => item.ParameterSnapshotId!)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(item => item, StringComparer.Ordinal);
                return new ControlPointProtectionRelationship(
                    view.ControlPoint,
                    view.Sku,
                    "库存缓冲",
                    view.EvidenceStatus == Complete ? "有保护设计" : "证据缺失",
                    view.Points.Any(item => item.EvidenceStatus == Complete && item.TopOfGreen is > 0m) ? "保护可用" : "证据缺失",
                    view.EvidenceStatus == Complete ? "保护有效" : "待验证",
                    $"{string.Join(" / ", snapshotIds)} / {view.EvidenceStatus}");
            })
            .ToList();

        results.AddRange(projection.TimeBuffers.Select(view =>
            new ControlPointProtectionRelationship(
                view.ControlPoint,
                view.ProtectedActivity,
                "时间缓冲",
                view.EvidenceStatus == Complete ? "有保护设计" : "证据缺失",
                view.Points.Any(item => item.EvidenceStatus == Complete) ? "保护可用" : "证据缺失",
                view.EvidenceStatus == Complete ? "保护有效" : "待验证",
                $"{view.BufferId} / {view.EvidenceStatus}")));

        var resourceNames = resources
            .GroupBy(item => item.Code, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().Name, StringComparer.Ordinal);
        results.AddRange(capacity
            .Where(item => item.RelationshipRole == "UpstreamProtection")
            .Select(item => new ControlPointProtectionRelationship(
                item.ResourceCode,
                item.ProtectedCcrResourceCode is null
                    ? "证据缺失"
                    : resourceNames.GetValueOrDefault(item.ProtectedCcrResourceCode, item.ProtectedCcrResourceCode),
                "产能保护",
                item.EvidenceStatus == Complete ? "有保护设计" : "证据缺失",
                item.RemainingProtection is > 0m ? "保护可用" : item.RemainingProtection is null ? "证据缺失" : "保护不足",
                item.CommittedLoad is not null && item.PlannedAvailableCapacity is not null
                    ? item.CommittedLoad <= item.PlannedAvailableCapacity ? "保护有效" : "保护被穿透"
                    : "待验证",
                $"{item.ResourceName} -> {item.ProtectedCcrResourceCode ?? "证据缺失"} / {item.LossReason} / {item.EvidenceStatus}")));

        return results;
    }

    private static IReadOnlyList<ConstraintExposureItem> BuildConstraintExposure(
        IReadOnlyList<HistoryConstraintFact> facts,
        IReadOnlyList<CapacityResource> resources)
    {
        var resourceNames = resources.ToDictionary(item => item.Code, item => item.Name, StringComparer.Ordinal);
        return facts
            .Select(item => new ConstraintExposureItem(
                item.ExposureType,
                resourceNames.GetValueOrDefault(item.Target, item.Target),
                item.Status,
                item.LoadPercent,
                item.Evidence,
                item.EvidenceStatus))
            .ToList();
    }

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
