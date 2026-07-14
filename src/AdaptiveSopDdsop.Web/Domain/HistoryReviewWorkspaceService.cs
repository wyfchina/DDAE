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
        var asOfDate = new DateOnly(2026, 6, 1);
        var history = _historyFactSource.Load(new HistoryFactRequest(requestedWeeks, asOfDate));
        var definitions = _scenarioDataSource.Load(new ScenarioWorkspaceDataRequest(requestedWeeks, asOfDate));
        var maximumLeadTime = definitions.DdmrpParameters
            .Where(item => item.DecoupledLeadTimeDays > 0)
            .Select(item => item.DecoupledLeadTimeDays)
            .DefaultIfEmpty(7)
            .Max();
        var detailWeeks = Math.Clamp((int)Math.Ceiling(maximumLeadTime / 7m), 1, requestedWeeks);
        var zoneResidence = BuildZoneResidence(history.BufferFacts, definitions.DdmrpParameters, detailWeeks);
        var capacity = BuildCapacityProtection(history.CapacityFacts, definitions);
        var relationships = BuildProtectionRelationships(definitions, capacity);
        var exposure = BuildConstraintExposure(history.ConstraintFacts, definitions.Resources);
        var outcomes = BuildOperatingOutcomes(history, capacity);
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
            $"{history.EvidenceLabel} / SourceAuthority={history.SourceAuthority} / AsOf={history.AsOfUtc}");
    }

    private static HistoryOperatingOutcomes BuildOperatingOutcomes(
        HistoryFactSet history,
        IReadOnlyList<CapacityProtectionLayer> capacity)
    {
        var service = AverageOrNull(history.OperatingFacts, item => item.ServiceLevelPercent, 1);
        var inventory = AverageOrNull(history.OperatingFacts, item => item.InventoryValue, 0);
        var workInProcess = AverageOrNull(history.OperatingFacts, item => item.WorkInProcessUnits, 1);
        var flowTime = AverageOrNull(history.OperatingFacts, item => item.AverageFlowTimeDays, 1);
        var cash = AverageOrNull(history.OperatingFacts, item => item.CashOccupied, 0);
        var costEvents = history.AbnormalCosts
            .Where(item => item.EvidenceStatus == Complete)
            .ToList();
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
        IReadOnlyList<WeeklyBufferFact> facts,
        IReadOnlyList<DdmrpParameterProfile> parameters,
        int detailWeeks)
    {
        var parameterBySku = parameters.ToDictionary(item => item.Sku, StringComparer.Ordinal);
        return facts
            .Where(item =>
                item.WeekOffset < 0 &&
                Math.Abs(item.WeekOffset) <= detailWeeks &&
                item.EndingNetFlow is not null &&
                item.EvidenceStatus == Complete &&
                parameterBySku.ContainsKey(item.Sku))
            .GroupBy(item => item.Sku, StringComparer.Ordinal)
            .Select(group =>
            {
                var parameter = parameterBySku[group.Key];
                var ordered = group.OrderBy(item => item.WeekOffset).ToList();
                var statuses = ordered
                    .Select(item => Zone(item.EndingNetFlow!.Value, parameter))
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
                    .Where(item => !string.IsNullOrWhiteSpace(item.ExplicitCause))
                    .GroupBy(item => item.ExplicitCause, StringComparer.Ordinal)
                    .OrderByDescending(causeGroup => causeGroup.Count())
                    .ThenBy(causeGroup => causeGroup.Key, StringComparer.Ordinal)
                    .Select(causeGroup => causeGroup.Key)
                    .FirstOrDefault() ?? "证据缺失";
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
                    MaximumStreak(statuses, "Red"),
                    recovery,
                    primaryCause);
            })
            .OrderByDescending(item => item.RedPeriods)
            .ThenBy(item => item.Sku, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<CapacityProtectionLayer> BuildCapacityProtection(
        IReadOnlyList<WeeklyCapacityFact> facts,
        ScenarioWorkspaceDataSet definitions)
    {
        var resourceNames = definitions.Resources
            .ToDictionary(item => item.Code, item => item.Name, StringComparer.Ordinal);
        var factGroups = facts
            .GroupBy(item => item.ResourceCode, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var resourceCodes = resourceNames.Keys
            .Concat(factGroups.Keys)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var protectionDefinitions = definitions.CapacityProtections ?? Array.Empty<CapacityProtectionDefinition>();
        var claimedCcrCodes = definitions.ResourceRoutings
            .Where(item => !string.IsNullOrWhiteSpace(item.ProtectsCcrResourceCode))
            .Select(item => item.ProtectsCcrResourceCode!)
            .Concat(protectionDefinitions.Select(item => item.ProtectedCcrResourceCode))
            .ToHashSet(StringComparer.Ordinal);

        return resourceCodes.Select(resourceCode =>
        {
            var resourceFacts = factGroups.GetValueOrDefault(resourceCode) ?? new List<WeeklyCapacityFact>();
            var theoretical = CapacityAverage(resourceFacts, item => item.TheoreticalCapacity);
            var standard = CapacityAverage(resourceFacts, item => item.StandardCapacity);
            var demonstrated = CapacityAverage(resourceFacts, item => item.DemonstratedCapacity);
            var planned = CapacityAverage(resourceFacts, item => item.PlannedAvailableCapacity);
            var committed = CapacityAverage(resourceFacts, item => item.CommittedLoad);
            var lossReason = resourceFacts
                .Where(item => item.EvidenceStatus == Complete && !string.IsNullOrWhiteSpace(item.LossReason))
                .Select(item => item.LossReason)
                .Distinct(StringComparer.Ordinal)
                .DefaultIfEmpty("证据缺失")
                .Aggregate((left, right) => $"{left}；{right}");
            var definition = protectionDefinitions.FirstOrDefault(item => item.UpstreamResourceCode == resourceCode);
            var routingClaim = definitions.ResourceRoutings
                .FirstOrDefault(item =>
                    item.ResourceCode == resourceCode &&
                    !string.IsNullOrWhiteSpace(item.ProtectsCcrResourceCode));
            var protectedCcrCode = definition?.ProtectedCcrResourceCode ?? routingClaim?.ProtectsCcrResourceCode;
            var hasProtectionClaim = definition is not null || routingClaim is not null;
            var hasCompleteDefinition = definition is not null &&
                definition.EvidenceStatus == Complete &&
                HasCompleteSequenceEvidence(definitions.ResourceRoutings, definition);
            var capacityEvidenceComplete = theoretical is not null &&
                standard is not null &&
                demonstrated is not null &&
                planned is not null &&
                committed is not null;

            decimal? protective = null;
            decimal? consumed = null;
            decimal? remaining = null;
            if (hasCompleteDefinition && planned is not null && committed is not null)
            {
                var reservePercent = Math.Clamp(definition!.ReservePercent, 0m, 100m);
                protective = decimal.Round(planned.Value * reservePercent / 100m, 1);
                var protectionStart = planned.Value - protective.Value;
                consumed = decimal.Round(
                    Math.Clamp(committed.Value - protectionStart, 0m, protective.Value),
                    1);
                remaining = decimal.Round(protective.Value - consumed.Value, 1);
            }

            var relationshipRole = hasProtectionClaim
                ? "UpstreamProtection"
                : claimedCcrCodes.Contains(resourceCode)
                    ? "CcrUtilization"
                    : "ObservedResource";
            var evidenceStatus = capacityEvidenceComplete && (!hasProtectionClaim || hasCompleteDefinition)
                ? Complete
                : EvidenceMissing;

            return new CapacityProtectionLayer(
                resourceCode,
                resourceNames.GetValueOrDefault(resourceCode, resourceCode),
                protectedCcrCode,
                relationshipRole,
                theoretical,
                standard,
                demonstrated,
                planned,
                committed,
                protective,
                consumed,
                remaining,
                lossReason,
                evidenceStatus);
        }).ToList();
    }

    private static decimal? CapacityAverage(
        IReadOnlyList<WeeklyCapacityFact> facts,
        Func<WeeklyCapacityFact, decimal?> selector)
    {
        var values = facts
            .Where(item => item.EvidenceStatus == Complete)
            .Select(selector)
            .Where(item => item.HasValue)
            .Select(item => item!.Value)
            .ToList();
        return values.Count == 0 ? null : decimal.Round(values.Average(), 1);
    }

    private static bool HasCompleteSequenceEvidence(
        IReadOnlyList<ResourceRouting> routings,
        CapacityProtectionDefinition definition)
    {
        return routings
            .Where(item =>
                item.ResourceCode == definition.UpstreamResourceCode &&
                item.ProtectsCcrResourceCode == definition.ProtectedCcrResourceCode &&
                item.OperationSequence > 0 &&
                item.EvidenceStatus == Complete)
            .Any(upstream => routings.Any(downstream =>
                downstream.Sku == upstream.Sku &&
                downstream.ResourceCode == definition.ProtectedCcrResourceCode &&
                downstream.OperationSequence > upstream.OperationSequence &&
                downstream.EvidenceStatus == Complete));
    }

    private static IReadOnlyList<ControlPointProtectionRelationship> BuildProtectionRelationships(
        ScenarioWorkspaceDataSet definitions,
        IReadOnlyList<CapacityProtectionLayer> capacity)
    {
        var results = definitions.DdmrpParameters
            .Select(item => new ControlPointProtectionRelationship(
                item.DecouplingPoint,
                item.Sku,
                "库存缓冲",
                string.IsNullOrWhiteSpace(item.BufferProfile) ? "证据缺失" : "有保护设计",
                item.TopOfGreen > 0 ? "保护可用" : "证据缺失",
                item.CompletenessStatus == Complete ? "保护有效" : "待验证",
                $"{item.BufferProfile} / {item.ParameterStatus} / {item.CompletenessStatus}"))
            .ToList();

        results.AddRange((definitions.TimeBuffers ?? Array.Empty<TimeBufferDefinition>())
            .Select(item => new ControlPointProtectionRelationship(
                item.ControlPoint,
                item.ProtectedActivity,
                "时间缓冲",
                item.EvidenceStatus == Complete ? "有保护设计" : "证据缺失",
                item.BufferDays > 0 ? "保护可用" : "证据缺失",
                item.EvidenceStatus == Complete ? "保护有效" : "待验证",
                $"{item.BufferDays:0.0} 天 / {item.Applicability} / {item.EvidenceStatus}")));

        var resourceNames = definitions.Resources
            .ToDictionary(item => item.Code, item => item.Name, StringComparer.Ordinal);
        results.AddRange(capacity
            .Where(item => item.RelationshipRole == "UpstreamProtection")
            .Select(item => new ControlPointProtectionRelationship(
                item.ResourceCode,
                item.ProtectedCcrResourceCode is null
                    ? "证据缺失"
                    : resourceNames.GetValueOrDefault(item.ProtectedCcrResourceCode, item.ProtectedCcrResourceCode),
                "产能保护",
                item.EvidenceStatus == Complete ? "有保护设计" : "证据缺失",
                item.RemainingProtection is > 0 ? "保护可用" : item.RemainingProtection is null ? "证据缺失" : "保护不足",
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
