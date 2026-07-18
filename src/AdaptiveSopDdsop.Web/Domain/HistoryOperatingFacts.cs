namespace AdaptiveSopDdsop.Web.Domain;

public sealed record HistoryFactRequest(int Weeks, DateOnly AsOfDate);

public sealed record WeeklyOperatingFact(
    int WeekOffset,
    decimal? ServiceLevelPercent,
    decimal? InventoryValue,
    decimal? WorkInProcessUnits,
    decimal? AverageFlowTimeDays,
    decimal? CashOccupied,
    string EvidenceStatus,
    decimal? ActualDemand = null,
    decimal? DemandSpikeThreshold = null,
    decimal? TargetNetFlowPosition = null);

public sealed record WeeklyBufferFact(
    string Sku,
    int WeekOffset,
    decimal? EndingNetFlow,
    string ExplicitCause,
    string EvidenceStatus,
    decimal? EndingOnHand = null,
    decimal? OpenSupply = null,
    decimal? QualifiedDemand = null,
    string ControlPoint = "",
    string? ParameterSnapshotId = null,
    decimal? DemandSpikeThreshold = null,
    decimal? TargetNetFlowPosition = null,
    decimal? OpeningOnHand = null,
    decimal? ActualReceipts = null,
    decimal? ActualConsumption = null,
    decimal? InventoryAdjustment = null,
    decimal? ActualDemand = null,
    string? ParameterChangeReason = null);

public sealed record WeeklyTimeBufferFact(
    string BufferId,
    string ControlPoint,
    string ProtectedActivity,
    int WeekOffset,
    int? EarlyCount,
    int? GreenCount,
    int? YellowCount,
    int? RedCount,
    int? LateCount,
    decimal? AbnormalCost,
    string? AbnormalCostEventId,
    string ExplicitCause,
    string EvidenceStatus);

public sealed record HistoricalDdmrpParameterFact(
    string SnapshotId,
    string Sku,
    string Name,
    string ControlPoint,
    int EffectiveFromWeekOffset,
    int EffectiveThroughWeekOffset,
    SkuBufferSetting Setting,
    string SourceAuthority,
    string AsOfUtc,
    string EvidenceStatus,
    string? ChangeReason = null);

public sealed record HistoricalCapacityProtectionFact(
    string SnapshotId,
    string UpstreamResourceCode,
    string ProtectedCcrResourceCode,
    int UpstreamOperationSequence,
    int CcrOperationSequence,
    decimal ReservePercent,
    int EffectiveFromWeekOffset,
    int EffectiveThroughWeekOffset,
    string EvidenceStatus);

public sealed record WeeklyCapacityFact(
    string ResourceCode,
    int WeekOffset,
    decimal? TheoreticalCapacity,
    decimal? StandardCapacity,
    decimal? DemonstratedCapacity,
    decimal? PlannedAvailableCapacity,
    decimal? CommittedLoad,
    string LossReason,
    string EvidenceStatus);

public sealed record HistoryConstraintFact(
    string ExposureType,
    string Target,
    int? WeekOffset,
    string Status,
    decimal? LoadPercent,
    string Evidence,
    string SourceKind,
    string EvidenceStatus);

public sealed record HistoryAbnormalCostEvent(
    string EventId,
    int WeekOffset,
    decimal CostAmount,
    string CostType,
    string Cause,
    string EvidenceStatus,
    string? TargetType = null,
    string? TargetId = null,
    string? ControlPoint = null,
    string? SourceAuthority = null);

public sealed record HistoryFactSet(
    HistoryFactRequest Request,
    IReadOnlyList<WeeklyOperatingFact> OperatingFacts,
    IReadOnlyList<WeeklyBufferFact> BufferFacts,
    IReadOnlyList<WeeklyCapacityFact> CapacityFacts,
    IReadOnlyList<HistoryConstraintFact> ConstraintFacts,
    IReadOnlyList<HistoryAbnormalCostEvent> AbnormalCosts,
    string SourceAuthority,
    string AsOfUtc,
    string EvidenceLabel,
    IReadOnlyList<WeeklyTimeBufferFact>? TimeBufferFacts = null,
    IReadOnlyList<HistoricalDdmrpParameterFact>? DdmrpParameterFacts = null,
    IReadOnlyList<HistoricalCapacityProtectionFact>? CapacityProtectionFacts = null,
    string FactSetId = "");

public interface IHistoryOperatingFactSource
{
    HistoryFactSet Load(HistoryFactRequest request);
}
