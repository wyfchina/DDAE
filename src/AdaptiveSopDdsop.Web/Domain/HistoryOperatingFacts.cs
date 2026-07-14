namespace AdaptiveSopDdsop.Web.Domain;

public sealed record HistoryFactRequest(int Weeks, DateOnly AsOfDate);

public sealed record WeeklyOperatingFact(
    int WeekOffset,
    decimal? ServiceLevelPercent,
    decimal? InventoryValue,
    decimal? WorkInProcessUnits,
    decimal? AverageFlowTimeDays,
    decimal? CashOccupied,
    string EvidenceStatus);

public sealed record WeeklyBufferFact(
    string Sku,
    int WeekOffset,
    decimal? EndingNetFlow,
    string ExplicitCause,
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
    string EvidenceStatus);

public sealed record HistoryFactSet(
    HistoryFactRequest Request,
    IReadOnlyList<WeeklyOperatingFact> OperatingFacts,
    IReadOnlyList<WeeklyBufferFact> BufferFacts,
    IReadOnlyList<WeeklyCapacityFact> CapacityFacts,
    IReadOnlyList<HistoryConstraintFact> ConstraintFacts,
    IReadOnlyList<HistoryAbnormalCostEvent> AbnormalCosts,
    string SourceAuthority,
    string AsOfUtc,
    string EvidenceLabel);

public interface IHistoryOperatingFactSource
{
    HistoryFactSet Load(HistoryFactRequest request);
}
