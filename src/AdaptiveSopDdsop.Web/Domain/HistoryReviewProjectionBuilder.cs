using System.Globalization;

namespace AdaptiveSopDdsop.Web.Domain;

public static class HistoryReviewProjectionBuilder
{
    private const string Complete = "Complete";
    private const string EvidenceMissing = "EvidenceMissing";

    public static HistoryReviewProjection Build(
        HistoryFactSet facts,
        ScenarioWorkspaceDataSet definitions,
        int detailWindowWeeks)
    {
        var weeks = Math.Clamp(facts.Request.Weeks, 1, 52);
        var normalizedDetailWindow = Math.Clamp(detailWindowWeeks, 1, weeks);
        var parameterFacts = facts.DdmrpParameterFacts ?? Array.Empty<HistoricalDdmrpParameterFact>();
        var inventory = BuildInventoryBuffers(facts, parameterFacts, weeks, normalizedDetailWindow);
        var sizing = BuildSizingSnapshots(parameterFacts, inventory);
        var time = BuildTimeBuffers(facts, weeks);
        var capacity = BuildCapacityBuffers(facts, definitions.Resources, weeks);

        return new HistoryReviewProjection(inventory, sizing, time, capacity);
    }

    private static IReadOnlyList<HistoryInventoryBufferView> BuildInventoryBuffers(
        HistoryFactSet facts,
        IReadOnlyList<HistoricalDdmrpParameterFact> parameterFacts,
        int weeks,
        int detailWindowWeeks)
    {
        var skus = facts.BufferFacts
            .Select(item => item.Sku)
            .Concat(parameterFacts.Select(item => item.Sku))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();

        return skus.Select(sku =>
        {
            var skuFacts = facts.BufferFacts
                .Where(item => item.Sku == sku)
                .ToList();
            var skuParameters = parameterFacts
                .Where(item => item.Sku == sku)
                .OrderBy(item => item.EffectiveFromWeekOffset)
                .ThenBy(item => item.SnapshotId, StringComparer.Ordinal)
                .ToList();
            var controlPoint = skuFacts
                .Select(item => item.ControlPoint)
                .Concat(skuParameters.Select(item => item.ControlPoint))
                .FirstOrDefault(item => !string.IsNullOrWhiteSpace(item)) ?? string.Empty;
            var name = skuParameters
                .Select(item => item.Name)
                .FirstOrDefault(item => !string.IsNullOrWhiteSpace(item)) ?? sku;
            var factsByWeek = skuFacts
                .GroupBy(item => item.WeekOffset)
                .ToDictionary(group => group.Key, group => group.ToList());

            var points = HistoricalWeeks(weeks)
                .Select(weekOffset => BuildInventoryPoint(
                    facts,
                    sku,
                    controlPoint,
                    weekOffset,
                    factsByWeek.GetValueOrDefault(weekOffset) ?? new List<WeeklyBufferFact>(),
                    skuParameters))
                .ToList();
            var distribution = BuildBuckets(
                new[]
                {
                    (Code: "Red", Label: "红区"),
                    (Code: "Yellow", Label: "黄区"),
                    (Code: "Green", Label: "绿区"),
                    (Code: "OverTopOfGreen", Label: "超绿区"),
                },
                points
                    .Where(item => item.EvidenceStatus == Complete)
                    .Select(item => item.Status));
            var evidenceStatus = points.All(item => item.EvidenceStatus == Complete)
                ? Complete
                : EvidenceMissing;

            return new HistoryInventoryBufferView(
                controlPoint,
                sku,
                name,
                detailWindowWeeks,
                points,
                distribution,
                evidenceStatus);
        }).ToList();
    }

    private static HistoryInventoryPoint BuildInventoryPoint(
        HistoryFactSet facts,
        string sku,
        string controlPoint,
        int weekOffset,
        IReadOnlyList<WeeklyBufferFact> matchingFacts,
        IReadOnlyList<HistoricalDdmrpParameterFact> parameterFacts)
    {
        if (matchingFacts.Count != 1)
        {
            return new HistoryInventoryPoint(
                weekOffset,
                PeriodStartDate(facts, weekOffset),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                EvidenceMissing,
                matchingFacts.Count == 0 ? "历史库存事实缺失" : "历史库存事实重复",
                null,
                EvidenceMissing);
        }

        var fact = matchingFacts[0];
        var quantitiesComplete = fact.EvidenceStatus == Complete &&
            fact.EndingOnHand.HasValue &&
            fact.OpenSupply.HasValue &&
            fact.QualifiedDemand.HasValue &&
            fact.EndingNetFlow.HasValue &&
            Math.Abs(
                fact.EndingOnHand.Value + fact.OpenSupply.Value - fact.QualifiedDemand.Value -
                fact.EndingNetFlow.Value) <= 0.1m;
        var snapshots = string.IsNullOrWhiteSpace(fact.ParameterSnapshotId)
            ? new List<HistoricalDdmrpParameterFact>()
            : parameterFacts
                .Where(item =>
                    item.SnapshotId == fact.ParameterSnapshotId &&
                    item.Sku == sku &&
                    item.ControlPoint == controlPoint &&
                    item.EffectiveFromWeekOffset <= weekOffset &&
                    weekOffset <= item.EffectiveThroughWeekOffset)
                .ToList();
        var sizing = snapshots.Count == 1 ? CalculateSizingOrNull(snapshots[0]) : null;
        var complete = quantitiesComplete && sizing is not null;

        return new HistoryInventoryPoint(
            weekOffset,
            PeriodStartDate(facts, weekOffset),
            fact.EndingOnHand,
            fact.OpenSupply,
            fact.QualifiedDemand,
            fact.EndingNetFlow,
            complete ? sizing!.Zones.TopOfRed : null,
            complete ? sizing!.Zones.TopOfYellow : null,
            complete ? sizing!.Zones.TopOfGreen : null,
            complete ? DdmrpCalculator.GetBufferStatus(fact.EndingNetFlow!.Value, sizing!.Zones) : EvidenceMissing,
            string.IsNullOrWhiteSpace(fact.ExplicitCause) ? "历史原因事实缺失" : fact.ExplicitCause,
            fact.ParameterSnapshotId,
            complete ? Complete : EvidenceMissing);
    }

    private static IReadOnlyList<HistoryDdmrpSizingSnapshotView> BuildSizingSnapshots(
        IReadOnlyList<HistoricalDdmrpParameterFact> parameterFacts,
        IReadOnlyList<HistoryInventoryBufferView> inventory)
    {
        return parameterFacts
            .OrderBy(item => item.Sku, StringComparer.Ordinal)
            .ThenBy(item => item.EffectiveFromWeekOffset)
            .ThenBy(item => item.SnapshotId, StringComparer.Ordinal)
            .Select(snapshot =>
            {
                var sizing = CalculateSizingOrNull(snapshot);
                var onHand = inventory
                    .Where(item => item.Sku == snapshot.Sku && item.ControlPoint == snapshot.ControlPoint)
                    .SelectMany(item => item.Points)
                    .Where(item =>
                        item.EvidenceStatus == Complete &&
                        item.ParameterSnapshotId == snapshot.SnapshotId &&
                        item.EndingOnHand.HasValue)
                    .Select(item => item.EndingOnHand!.Value)
                    .ToList();
                decimal? averageOnHand = onHand.Count == 0
                    ? null
                    : decimal.Round(onHand.Average(), 1);
                var evidenceStatus = sizing is not null && averageOnHand is not null
                    ? Complete
                    : EvidenceMissing;

                return new HistoryDdmrpSizingSnapshotView(
                    snapshot.SnapshotId,
                    snapshot.ControlPoint,
                    snapshot.Sku,
                    snapshot.Name,
                    snapshot.EffectiveFromWeekOffset,
                    snapshot.EffectiveThroughWeekOffset,
                    snapshot.Setting,
                    sizing,
                    sizing is null ? Array.Empty<BufferSizingLine>() : DdmrpSizingExplanation.Build(sizing),
                    averageOnHand,
                    snapshot.SourceAuthority,
                    snapshot.AsOfUtc,
                    evidenceStatus);
            })
            .ToList();
    }

    internal static bool HasCompleteSizingEvidence(HistoricalDdmrpParameterFact snapshot)
    {
        var setting = snapshot.Setting;
        return snapshot.EvidenceStatus == Complete &&
            !string.IsNullOrWhiteSpace(snapshot.SnapshotId) &&
            !string.IsNullOrWhiteSpace(setting.ParameterSnapshotId) &&
            snapshot.EffectiveFromWeekOffset <= snapshot.EffectiveThroughWeekOffset &&
            snapshot.EffectiveFromWeekOffset < 0 &&
            snapshot.EffectiveThroughWeekOffset < 0 &&
            setting.ParameterEvidenceStatus == Complete &&
            setting.ParameterSnapshotId == snapshot.SnapshotId &&
            setting.Sku == snapshot.Sku &&
            setting.DecouplingPoint == snapshot.ControlPoint &&
            setting.Adu > 0m &&
            setting.DecoupledLeadTimeDays > 0 &&
            setting.LeadTimeFactor is > 0m and <= 1m &&
            setting.VariabilityFactor >= 0m &&
            setting.DemandAdjustmentFactor > 0m &&
            setting.ZoneAdjustmentFactor > 0m &&
            setting.MinimumOrderQuantity >= 0m &&
            setting.OrderCycleDays > 0;
    }

    private static DdmrpSizingResult? CalculateSizingOrNull(HistoricalDdmrpParameterFact snapshot) =>
        HasCompleteSizingEvidence(snapshot)
            ? DdmrpCalculator.CalculateSizing(snapshot.Setting)
            : null;

    private static IReadOnlyList<HistoryTimeBufferView> BuildTimeBuffers(
        HistoryFactSet facts,
        int weeks)
    {
        var eligibleFacts = (facts.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>())
            .Where(IsEligibleTimeFact)
            .ToList();
        var bufferIds = eligibleFacts
            .Select(item => item.BufferId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();

        return bufferIds.Select(bufferId =>
        {
            var bufferFacts = eligibleFacts.Where(item => item.BufferId == bufferId).ToList();
            var controlPoint = bufferFacts
                .Select(item => item.ControlPoint)
                .FirstOrDefault(item => !string.IsNullOrWhiteSpace(item)) ?? string.Empty;
            var protectedActivity = bufferFacts
                .Select(item => item.ProtectedActivity)
                .FirstOrDefault(item => !string.IsNullOrWhiteSpace(item)) ?? string.Empty;
            var factsByWeek = bufferFacts
                .GroupBy(item => item.WeekOffset)
                .ToDictionary(group => group.Key, group => group.ToList());
            var points = HistoricalWeeks(weeks)
                .Select(weekOffset => BuildTimePoint(
                    facts,
                    weekOffset,
                    factsByWeek.GetValueOrDefault(weekOffset) ?? new List<WeeklyTimeBufferFact>()))
                .ToList();
            var completePoints = points.Where(item => item.EvidenceStatus == Complete).ToList();
            var countByCode = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["Early"] = completePoints.Sum(item => item.EarlyCount!.Value),
                ["Green"] = completePoints.Sum(item => item.GreenCount!.Value),
                ["Yellow"] = completePoints.Sum(item => item.YellowCount!.Value),
                ["Red"] = completePoints.Sum(item => item.RedCount!.Value),
                ["Late"] = completePoints.Sum(item => item.LateCount!.Value),
            };
            var distribution = BuildBuckets(
                new[]
                {
                    (Code: "Early", Label: "提前"),
                    (Code: "Green", Label: "绿色"),
                    (Code: "Yellow", Label: "黄色"),
                    (Code: "Red", Label: "红色"),
                    (Code: "Late", Label: "延迟"),
                },
                countByCode);
            var evidenceStatus = points.All(item => item.EvidenceStatus == Complete) && countByCode.Values.Sum() > 0
                ? Complete
                : EvidenceMissing;

            return new HistoryTimeBufferView(
                bufferId,
                controlPoint,
                protectedActivity,
                points,
                distribution,
                evidenceStatus);
        }).ToList();
    }

    private static HistoryTimeBufferPoint BuildTimePoint(
        HistoryFactSet facts,
        int weekOffset,
        IReadOnlyList<WeeklyTimeBufferFact> matchingFacts)
    {
        if (matchingFacts.Count != 1)
        {
            return new HistoryTimeBufferPoint(
                weekOffset,
                PeriodStartDate(facts, weekOffset),
                null,
                null,
                null,
                null,
                null,
                null,
                matchingFacts.Count == 0 ? "历史时间缓冲事实缺失" : "历史时间缓冲事实重复",
                EvidenceMissing);
        }

        var fact = matchingFacts[0];
        var countsComplete = fact.EvidenceStatus == Complete &&
            fact.EarlyCount is >= 0 &&
            fact.GreenCount is >= 0 &&
            fact.YellowCount is >= 0 &&
            fact.RedCount is >= 0 &&
            fact.LateCount is >= 0;
        var costEvents = string.IsNullOrWhiteSpace(fact.AbnormalCostEventId)
            ? new List<HistoryAbnormalCostEvent>()
            : facts.AbnormalCosts
                .Where(item => item.EventId == fact.AbnormalCostEventId)
                .ToList();
        var hasNoCostEvidence = fact.AbnormalCostEventId is null && fact.AbnormalCost is null;
        var hasMatchedCostEvidence = fact.AbnormalCostEventId is not null &&
            fact.AbnormalCost.HasValue &&
            costEvents.Count == 1 &&
            costEvents[0].WeekOffset == weekOffset &&
            costEvents[0].EvidenceStatus == Complete &&
            fact.AbnormalCost.Value == costEvents[0].CostAmount;
        var costComplete = hasNoCostEvidence || hasMatchedCostEvidence;
        var complete = countsComplete && costComplete;

        return new HistoryTimeBufferPoint(
            weekOffset,
            PeriodStartDate(facts, weekOffset),
            fact.EarlyCount,
            fact.GreenCount,
            fact.YellowCount,
            fact.RedCount,
            fact.LateCount,
            hasMatchedCostEvidence ? costEvents[0].CostAmount : null,
            string.IsNullOrWhiteSpace(fact.ExplicitCause) ? "历史原因事实缺失" : fact.ExplicitCause,
            complete ? Complete : EvidenceMissing);
    }

    private static IReadOnlyList<HistoryCapacityBufferView> BuildCapacityBuffers(
        HistoryFactSet facts,
        IReadOnlyList<CapacityResource> resources,
        int weeks)
    {
        var protectionFacts = (facts.CapacityProtectionFacts ?? Array.Empty<HistoricalCapacityProtectionFact>())
            .Where(item => IsEligibleIdentifier(item.UpstreamResourceCode) && IsEligibleIdentifier(item.ProtectedCcrResourceCode))
            .ToList();
        var validProtectionFacts = protectionFacts
            .Where(HasValidProtectionStructure)
            .ToList();
        var resourceNames = resources
            .GroupBy(item => item.Code, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().Name, StringComparer.Ordinal);
        var resourceCodes = facts.CapacityFacts
            .Select(item => item.ResourceCode)
            .Concat(protectionFacts.Select(item => item.UpstreamResourceCode))
            .Concat(protectionFacts.Select(item => item.ProtectedCcrResourceCode))
            .Where(IsEligibleIdentifier)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();

        return resourceCodes.Select(resourceCode =>
        {
            var resourceFacts = facts.CapacityFacts
                .Where(item => item.ResourceCode == resourceCode)
                .ToList();
            var factsByWeek = resourceFacts
                .GroupBy(item => item.WeekOffset)
                .ToDictionary(group => group.Key, group => group.ToList());
            var upstreamFacts = protectionFacts
                .Where(item => item.UpstreamResourceCode == resourceCode)
                .ToList();
            var upstreamRelationships = validProtectionFacts
                .Where(item => item.UpstreamResourceCode == resourceCode)
                .Select(item => item.ProtectedCcrResourceCode)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            var isUpstream = upstreamRelationships.Count == 1;
            var isCcr = validProtectionFacts.Any(item => item.ProtectedCcrResourceCode == resourceCode);
            var relationshipRole = isUpstream
                ? "UpstreamProtection"
                : isCcr ? "CcrUtilization" : "ObservedResource";
            var protectedCcr = isUpstream ? upstreamRelationships[0] : null;
            var relationshipEvidenceComplete = upstreamFacts.Count == 0 || isUpstream;
            var points = HistoricalWeeks(weeks)
                .Select(weekOffset => BuildCapacityPoint(
                    facts,
                    weekOffset,
                    factsByWeek.GetValueOrDefault(weekOffset) ?? new List<WeeklyCapacityFact>(),
                    relationshipRole,
                    protectedCcr,
                    validProtectionFacts,
                    relationshipEvidenceComplete))
                .ToList();
            var distributionCodes = points
                .Where(item => item.EvidenceStatus == Complete && item.PlannedAvailableCapacity is > 0m && item.CommittedLoad.HasValue)
                .Select(item =>
                {
                    var ratio = item.CommittedLoad!.Value * 100m / item.PlannedAvailableCapacity!.Value;
                    return ratio <= 60m
                        ? "Safe"
                        : ratio <= 80m
                            ? "High"
                            : ratio <= 100m ? "NearLimit" : "Overload";
                });
            var distribution = BuildBuckets(
                new[]
                {
                    (Code: "Safe", Label: "安全"),
                    (Code: "High", Label: "高负荷"),
                    (Code: "NearLimit", Label: "接近上限"),
                    (Code: "Overload", Label: "超负荷"),
                },
                distributionCodes);
            var evidenceStatus = protectionFacts.Count > 0 &&
                points.All(item => item.EvidenceStatus == Complete)
                ? Complete
                : EvidenceMissing;

            return new HistoryCapacityBufferView(
                resourceCode,
                resourceNames.GetValueOrDefault(resourceCode, resourceCode),
                protectedCcr,
                relationshipRole,
                points,
                distribution,
                evidenceStatus);
        }).ToList();
    }

    private static HistoryCapacityPoint BuildCapacityPoint(
        HistoryFactSet facts,
        int weekOffset,
        IReadOnlyList<WeeklyCapacityFact> matchingFacts,
        string relationshipRole,
        string? protectedCcr,
        IReadOnlyList<HistoricalCapacityProtectionFact> protectionFacts,
        bool relationshipEvidenceComplete)
    {
        if (matchingFacts.Count != 1)
        {
            return new HistoryCapacityPoint(
                weekOffset,
                PeriodStartDate(facts, weekOffset),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                EvidenceMissing);
        }

        var fact = matchingFacts[0];
        var capacityComplete = fact.EvidenceStatus == Complete &&
            fact.TheoreticalCapacity is >= 0m &&
            fact.StandardCapacity is >= 0m &&
            fact.DemonstratedCapacity is >= 0m &&
            fact.PlannedAvailableCapacity is > 0m &&
            fact.CommittedLoad is >= 0m;
        decimal? protectionStart = null;
        decimal? protective = null;
        decimal? consumed = null;
        decimal? remaining = null;
        var protectionComplete = relationshipEvidenceComplete;

        if (relationshipRole == "UpstreamProtection")
        {
            var effectiveProtection = protectionFacts
                .Where(item =>
                    item.UpstreamResourceCode == fact.ResourceCode &&
                    item.ProtectedCcrResourceCode == protectedCcr &&
                    item.EffectiveFromWeekOffset <= weekOffset &&
                    weekOffset <= item.EffectiveThroughWeekOffset)
                .ToList();
            protectionComplete = effectiveProtection.Count == 1;
            if (capacityComplete && protectionComplete)
            {
                var planned = fact.PlannedAvailableCapacity!.Value;
                protective = planned * effectiveProtection[0].ReservePercent / 100m;
                protectionStart = planned - protective.Value;
                consumed = Math.Clamp(fact.CommittedLoad!.Value - protectionStart.Value, 0m, protective.Value);
                remaining = protective.Value - consumed.Value;
            }
        }

        return new HistoryCapacityPoint(
            weekOffset,
            PeriodStartDate(facts, weekOffset),
            fact.TheoreticalCapacity,
            fact.StandardCapacity,
            fact.DemonstratedCapacity,
            fact.PlannedAvailableCapacity,
            fact.CommittedLoad,
            protectionStart,
            protective,
            consumed,
            remaining,
            capacityComplete && protectionComplete ? Complete : EvidenceMissing);
    }

    private static bool HasValidProtectionStructure(HistoricalCapacityProtectionFact fact) =>
        fact.EvidenceStatus == Complete &&
        fact.UpstreamOperationSequence > 0 &&
        fact.CcrOperationSequence > fact.UpstreamOperationSequence &&
        fact.ReservePercent is > 0m and <= 100m &&
        fact.EffectiveFromWeekOffset <= fact.EffectiveThroughWeekOffset &&
        fact.EffectiveFromWeekOffset < 0 &&
        fact.EffectiveThroughWeekOffset < 0;

    private static bool IsEligibleTimeFact(WeeklyTimeBufferFact fact) =>
        IsEligibleIdentifier(fact.BufferId) &&
        IsEligibleIdentifier(fact.ControlPoint) &&
        IsEligibleIdentifier(fact.ProtectedActivity);

    private static bool IsEligibleIdentifier(string value) =>
        ProtectionProductEligibility.IsEligible(value) &&
        !value.Contains(ProtectionProductEligibility.InventoryOnlyFpgaSku, StringComparison.Ordinal) &&
        !value.Contains("FPGA", StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<int> HistoricalWeeks(int weeks) =>
        Enumerable.Range(1, weeks)
            .Select(item => -item)
            .OrderBy(item => item)
            .ToList();

    private static string PeriodStartDate(HistoryFactSet facts, int weekOffset) =>
        facts.Request.AsOfDate
            .AddDays(weekOffset * 7)
            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static IReadOnlyList<HistoryDistributionBucket> BuildBuckets(
        IReadOnlyList<(string Code, string Label)> buckets,
        IEnumerable<string> observations)
    {
        var countByCode = observations
            .GroupBy(item => item, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        return BuildBuckets(buckets, countByCode);
    }

    private static IReadOnlyList<HistoryDistributionBucket> BuildBuckets(
        IReadOnlyList<(string Code, string Label)> buckets,
        IReadOnlyDictionary<string, int> countByCode)
    {
        var total = countByCode.Values.Sum();
        return buckets
            .Select(bucket =>
            {
                var count = countByCode.GetValueOrDefault(bucket.Code);
                var percent = total == 0
                    ? 0m
                    : decimal.Round(count * 100m / total, 1, MidpointRounding.AwayFromZero);
                return new HistoryDistributionBucket(bucket.Code, bucket.Label, count, percent);
            })
            .ToList();
    }
}
