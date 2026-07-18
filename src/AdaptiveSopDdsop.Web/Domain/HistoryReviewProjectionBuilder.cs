using System.Globalization;

namespace AdaptiveSopDdsop.Web.Domain;

public static class HistoryReviewProjectionBuilder
{
    private const string Complete = "Complete";
    private const string EvidenceMissing = "EvidenceMissing";

    public static HistoryReviewProjection Build(
        HistoryFactSet facts,
        ScenarioWorkspaceDataSet definitions,
        int detailWindowWeeks,
        IReadOnlyList<WeeklyBufferFact>? rollingBufferFacts = null,
        IReadOnlyList<HistoryAbnormalCostEvent>? abnormalCostLedger = null)
    {
        var weeks = Math.Clamp(facts.Request.Weeks, 1, 52);
        var normalizedDetailWindow = Math.Clamp(detailWindowWeeks, 1, weeks);
        var parameterFacts = facts.DdmrpParameterFacts ?? Array.Empty<HistoricalDdmrpParameterFact>();
        var inventory = BuildInventoryBuffers(
            facts,
            parameterFacts,
            rollingBufferFacts ?? facts.BufferFacts,
            weeks,
            normalizedDetailWindow);
        var sizing = BuildSizingSnapshots(parameterFacts, inventory);
        var time = BuildTimeBuffers(
            facts,
            weeks,
            abnormalCostLedger ?? facts.AbnormalCosts);
        var capacity = BuildCapacityBuffers(facts, definitions.Resources, weeks);

        return new HistoryReviewProjection(inventory, sizing, time, capacity);
    }

    private static IReadOnlyList<HistoryInventoryBufferView> BuildInventoryBuffers(
        HistoryFactSet facts,
        IReadOnlyList<HistoricalDdmrpParameterFact> parameterFacts,
        IReadOnlyList<WeeklyBufferFact> rollingBufferFacts,
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
            var skuRollingFacts = rollingBufferFacts
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
                    skuRollingFacts,
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
        IReadOnlyList<WeeklyBufferFact> skuFacts,
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
                EvidenceMissing,
                EvidenceChecks: MissingInventoryEvidenceChecks(
                    matchingFacts.Count == 0 ? "历史库存事实缺失" : "历史库存事实重复"));
        }

        var fact = matchingFacts[0];
        var sourceFieldsComplete = fact.EvidenceStatus == Complete &&
            fact.OpeningOnHand.HasValue &&
            fact.ActualReceipts.HasValue &&
            fact.ActualConsumption.HasValue &&
            fact.InventoryAdjustment.HasValue &&
            fact.EndingOnHand.HasValue &&
            fact.OpenSupply.HasValue &&
            fact.QualifiedDemand.HasValue &&
            fact.EndingNetFlow.HasValue;
        var precedingFacts = skuFacts
            .Where(item => item.WeekOffset == weekOffset - 1)
            .ToList();
        var continuityValid = precedingFacts.Count == 0
            ? weekOffset == -facts.Request.Weeks &&
                fact.EvidenceStatus == Complete && fact.OpeningOnHand.HasValue
            : precedingFacts.Count == 1 &&
                precedingFacts[0].EvidenceStatus == Complete &&
                precedingFacts[0].EndingOnHand.HasValue &&
                fact.OpeningOnHand.HasValue &&
                Math.Abs(fact.OpeningOnHand.GetValueOrDefault() - precedingFacts[0].EndingOnHand.GetValueOrDefault()) <= 0.1m;
        var inventoryEquationValid = sourceFieldsComplete &&
            Math.Abs(
                fact.EndingOnHand.GetValueOrDefault() -
                (fact.OpeningOnHand.GetValueOrDefault() + fact.ActualReceipts.GetValueOrDefault() -
                 fact.ActualConsumption.GetValueOrDefault() + fact.InventoryAdjustment.GetValueOrDefault())) <= 0.1m;
        var netFlowEquationValid = sourceFieldsComplete &&
            Math.Abs(
                fact.EndingOnHand.GetValueOrDefault() + fact.OpenSupply.GetValueOrDefault() - fact.QualifiedDemand.GetValueOrDefault() -
                fact.EndingNetFlow.GetValueOrDefault()) <= 0.1m;
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
        var rollingAdu = CalculateRollingHistoricalAdu(skuFacts, weekOffset);
        var sizing = snapshots.Count == 1 && rollingAdu.HasValue
            ? CalculateSizingOrNull(snapshots[0] with
            {
                Setting = snapshots[0].Setting with { Adu = rollingAdu.Value }
            })
            : null;
        var actualDemand = fact.EvidenceStatus == Complete && fact.ActualDemand is >= 0m
            ? fact.ActualDemand
            : null;
        var demandSpikeThreshold = fact.EvidenceStatus == Complete && fact.DemandSpikeThreshold is > 0m
            ? fact.DemandSpikeThreshold
            : null;
        var checks = new List<HistoryEvidenceCheck>
        {
            Check("SourceFields", "数量字段完整", sourceFieldsComplete, "期初、收货、消耗、调整、期末、开放供应和合格需求"),
            Check("InventoryContinuity", "跨周库存连续", continuityValid, "本周期初等于上周期末"),
            Check("InventoryEquation", "库存结转恒等式", inventoryEquationValid, "期末=期初+收货-消耗+调整"),
            Check("NetFlowEquation", "净流量恒等式", netFlowEquationValid, "净流量=期末在手+开放供应-合格需求"),
            Check("ParameterSnapshot", "唯一参数快照", snapshots.Count == 1, fact.ParameterSnapshotId ?? "快照缺失"),
            Check("Sizing", "定容可复算", sizing is not null, sizing is null ? "定容证据缺失" : "后端定容完成"),
            Check("DemandEvidence", "需求与尖峰阈值", actualDemand.HasValue && demandSpikeThreshold.HasValue, "SKU级历史需求事实"),
        };
        var complete = checks.All(item => item.Status == Complete);
        var weeklyEvent = string.IsNullOrWhiteSpace(fact.ExplicitCause) ? null : fact.ExplicitCause;
        var parameterChangeReason = snapshots.Count == 1
            ? snapshots[0].ChangeReason
            : null;

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
            complete ? Complete : EvidenceMissing,
            actualDemand,
            demandSpikeThreshold,
            null,
            weeklyEvent,
            parameterChangeReason,
            checks,
            fact.OpeningOnHand,
            fact.ActualReceipts,
            fact.ActualConsumption,
            fact.InventoryAdjustment);
    }

    private static HistoryEvidenceCheck Check(string code, string label, bool complete, string detail) =>
        new(code, label, complete ? Complete : EvidenceMissing, detail);

    private static IReadOnlyList<HistoryEvidenceCheck> MissingInventoryEvidenceChecks(string detail) =>
        new[]
        {
            Check("SourceFields", "数量字段完整", false, detail),
            Check("InventoryContinuity", "跨周库存连续", false, detail),
            Check("InventoryEquation", "库存结转恒等式", false, detail),
            Check("NetFlowEquation", "净流量恒等式", false, detail),
            Check("ParameterSnapshot", "唯一参数快照", false, detail),
            Check("Sizing", "定容可复算", false, detail),
            Check("DemandEvidence", "需求与尖峰阈值", false, detail),
        };

    private static decimal? CalculateRollingHistoricalAdu(
        IReadOnlyList<WeeklyBufferFact> skuFacts,
        int weekOffset)
    {
        var historicalFacts = skuFacts
            .Where(item => item.WeekOffset <= weekOffset)
            .OrderBy(item => item.WeekOffset)
            .ToList();
        if (historicalFacts.Count == 0)
        {
            return null;
        }

        var firstWeek = historicalFacts[0].WeekOffset;
        var windowStart = Math.Max(firstWeek, weekOffset - 12);
        var weeklyDemand = Enumerable.Range(windowStart, weekOffset - windowStart + 1)
            .Select(offset => historicalFacts.Where(item => item.WeekOffset == offset).ToList())
            .ToList();
        if (weeklyDemand.Any(items =>
                items.Count != 1 ||
                items[0].EvidenceStatus != Complete ||
                !items[0].ActualDemand.HasValue ||
                items[0].ActualDemand.GetValueOrDefault() < 0m))
        {
            return null;
        }

        var rollingAdu = decimal.Round(
            weeklyDemand.Average(items => items[0].ActualDemand!.Value) / 7m,
            2,
            MidpointRounding.AwayFromZero);
        return rollingAdu > 0m ? rollingAdu : null;
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
                    evidenceStatus,
                    snapshot.ChangeReason);
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
        int weeks,
        IReadOnlyList<HistoryAbnormalCostEvent> abnormalCostLedger)
    {
        var validAbnormalCostEvents = SelectValidAbnormalCostEvents(
            facts.AbnormalCosts,
            abnormalCostLedger);
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
                    validAbnormalCostEvents,
                    bufferId,
                    controlPoint,
                    weekOffset,
                    factsByWeek.GetValueOrDefault(weekOffset) ?? new List<WeeklyTimeBufferFact>()))
                .ToList();
            var abnormalCostEvents = BuildGlobalAbnormalCostEventViews(
                facts,
                weeks,
                validAbnormalCostEvents);
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
                evidenceStatus,
                abnormalCostEvents);
        }).ToList();
    }

    private static HistoryTimeBufferPoint BuildTimePoint(
        HistoryFactSet facts,
        IReadOnlyList<HistoryAbnormalCostEvent> validAbnormalCostEvents,
        string bufferId,
        string controlPoint,
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
        var hasNoCostEvidence = fact.AbnormalCostEventId is null && fact.AbnormalCost is null;
        var matchedCostEvent = BuildAbnormalCostEventView(
            facts,
            validAbnormalCostEvents,
            bufferId,
            controlPoint,
            weekOffset,
            matchingFacts);
        var costComplete = hasNoCostEvidence || matchedCostEvent is not null;
        var complete = countsComplete && costComplete;

        return new HistoryTimeBufferPoint(
            weekOffset,
            PeriodStartDate(facts, weekOffset),
            fact.EarlyCount,
            fact.GreenCount,
            fact.YellowCount,
            fact.RedCount,
            fact.LateCount,
            matchedCostEvent?.CostAmount,
            string.IsNullOrWhiteSpace(fact.ExplicitCause) ? "历史原因事实缺失" : fact.ExplicitCause,
            complete ? Complete : EvidenceMissing);
    }

    private static HistoryAbnormalCostEventView? BuildAbnormalCostEventView(
        HistoryFactSet facts,
        IReadOnlyList<HistoryAbnormalCostEvent> validAbnormalCostEvents,
        string bufferId,
        string controlPoint,
        int weekOffset,
        IReadOnlyList<WeeklyTimeBufferFact> matchingFacts)
    {
        if (matchingFacts.Count != 1)
        {
            return null;
        }

        var fact = matchingFacts[0];
        if (fact.BufferId != bufferId ||
            fact.ControlPoint != controlPoint ||
            fact.EvidenceStatus != Complete ||
            string.IsNullOrWhiteSpace(fact.AbnormalCostEventId) ||
            !fact.AbnormalCost.HasValue)
        {
            return null;
        }

        var candidates = validAbnormalCostEvents
            .Where(item => item.EventId == fact.AbnormalCostEventId)
            .ToList();
        if (candidates.Count != 1)
        {
            return null;
        }

        var costEvent = candidates[0];
        var completeMatch = costEvent.WeekOffset == weekOffset &&
            costEvent.CostAmount == fact.AbnormalCost.Value &&
            HasCompleteAbnormalCostEventEvidence(costEvent) &&
            costEvent.TargetType == "时间缓冲" &&
            costEvent.TargetId == bufferId &&
            costEvent.ControlPoint == controlPoint &&
            string.Equals(costEvent.Cause, fact.ExplicitCause, StringComparison.Ordinal);
        if (!completeMatch)
        {
            return null;
        }

        return new HistoryAbnormalCostEventView(
            costEvent.EventId,
            costEvent.WeekOffset,
            PeriodStartDate(facts, costEvent.WeekOffset),
            costEvent.CostAmount,
            costEvent.CostType,
            costEvent.Cause,
            costEvent.TargetType!,
            costEvent.TargetId!,
            costEvent.ControlPoint!,
            costEvent.SourceAuthority!,
            costEvent.EvidenceStatus);
    }

    private static IReadOnlyList<HistoryAbnormalCostEventView> BuildGlobalAbnormalCostEventViews(
        HistoryFactSet facts,
        int weeks,
        IReadOnlyList<HistoryAbnormalCostEvent> validAbnormalCostEvents)
    {
        var validWeeks = HistoricalWeeks(weeks).ToHashSet();
        return validAbnormalCostEvents
            .Where(item => validWeeks.Contains(item.WeekOffset))
            .Select(item => new HistoryAbnormalCostEventView(
                item.EventId,
                item.WeekOffset,
                PeriodStartDate(facts, item.WeekOffset),
                item.CostAmount,
                item.CostType,
                item.Cause,
                item.TargetType!,
                item.TargetId!,
                item.ControlPoint!,
                item.SourceAuthority!,
                item.EvidenceStatus))
            .ToList();
    }

    internal static IReadOnlyList<HistoryAbnormalCostEvent> SelectValidAbnormalCostEvents(
        IReadOnlyList<HistoryAbnormalCostEvent> windowEvents,
        IReadOnlyList<HistoryAbnormalCostEvent> annualLedger)
    {
        var uniqueLedgerEvents = annualLedger
            .Where(item => !string.IsNullOrWhiteSpace(item.EventId))
            .GroupBy(item => item.EventId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
        return windowEvents
            .Where(item =>
                !string.IsNullOrWhiteSpace(item.EventId) &&
                uniqueLedgerEvents.TryGetValue(item.EventId, out var ledgerEvent) &&
                ledgerEvent == item &&
                HasCompleteAbnormalCostEventEvidence(item))
            .OrderBy(item => item.WeekOffset)
            .ThenBy(item => item.EventId, StringComparer.Ordinal)
            .ToList();
    }

    private static bool HasCompleteAbnormalCostEventEvidence(HistoryAbnormalCostEvent item) =>
        item.EvidenceStatus == Complete &&
        item.CostAmount > 0m &&
        !string.IsNullOrWhiteSpace(item.EventId) &&
        !string.IsNullOrWhiteSpace(item.CostType) &&
        !string.IsNullOrWhiteSpace(item.Cause) &&
        !string.IsNullOrWhiteSpace(item.TargetType) &&
        !string.IsNullOrWhiteSpace(item.TargetId) &&
        !string.IsNullOrWhiteSpace(item.ControlPoint) &&
        !string.IsNullOrWhiteSpace(item.SourceAuthority);

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
            var relationshipRole = isCcr
                ? "CcrUtilization"
                : isUpstream ? "UpstreamProtection" : "ObservedResource";
            var protectedCcr = relationshipRole == "UpstreamProtection" ? upstreamRelationships[0] : null;
            var relationshipEvidenceComplete = relationshipRole == "CcrUtilization" || upstreamFacts.Count == 0 || isUpstream;
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
                .Where(item => item.Measure?.EvidenceStatus == Complete)
                .Select(item => item.Measure!.UtilizationBand);
            var distribution = BuildBuckets(
                new[]
                {
                    (Code: "Green", Label: "绿区（0–60%）"),
                    (Code: "Yellow", Label: "黄区（>60–80%）"),
                    (Code: "Red", Label: "红区（>80–100%）"),
                    (Code: "DeepRed", Label: "深红区（>100%）"),
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
                EvidenceMissing,
                relationshipRole == "UpstreamProtection"
                    ? CapacityProtectionMath.CalculateUpstream(null, null, relationshipEvidenceComplete, EvidenceMissing)
                    : CapacityProtectionMath.CalculateCcrReference(null, null, EvidenceMissing));
        }

        var fact = matchingFacts[0];
        var capacityComplete = fact.EvidenceStatus == Complete &&
            fact.TheoreticalCapacity is >= 0m &&
            fact.StandardCapacity is >= 0m &&
            fact.DemonstratedCapacity is >= 0m &&
            fact.PlannedAvailableCapacity is > 0m &&
            fact.CommittedLoad is >= 0m;
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
        }

        var measureEvidenceStatus = capacityComplete && protectionComplete ? Complete : EvidenceMissing;
        var measure = relationshipRole == "UpstreamProtection"
            ? CapacityProtectionMath.CalculateUpstream(
                fact.PlannedAvailableCapacity,
                fact.CommittedLoad,
                protectionComplete,
                measureEvidenceStatus)
            : CapacityProtectionMath.CalculateCcrReference(
                fact.PlannedAvailableCapacity,
                fact.CommittedLoad,
                capacityComplete && relationshipEvidenceComplete ? Complete : EvidenceMissing);

        return new HistoryCapacityPoint(
            weekOffset,
            PeriodStartDate(facts, weekOffset),
            fact.TheoreticalCapacity,
            fact.StandardCapacity,
            fact.DemonstratedCapacity,
            fact.PlannedAvailableCapacity,
            fact.CommittedLoad,
            measure.ProtectionStart,
            measure.ProtectionCapacity,
            measure.ConsumedProtection,
            measure.RemainingProtection,
            measure.EvidenceStatus,
            measure);
    }

    private static bool HasValidProtectionStructure(HistoricalCapacityProtectionFact fact) =>
        fact.EvidenceStatus == Complete &&
        !string.Equals(fact.UpstreamResourceCode, fact.ProtectedCcrResourceCode, StringComparison.Ordinal) &&
        fact.UpstreamOperationSequence > 0 &&
        fact.CcrOperationSequence > fact.UpstreamOperationSequence &&
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
