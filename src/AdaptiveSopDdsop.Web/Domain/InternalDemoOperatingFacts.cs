namespace AdaptiveSopDdsop.Web.Domain;

public sealed record InternalDemoFactSetHeader(
    string FactSetId,
    string SourceKind,
    string SourceAuthority,
    string HistoryThroughUtc,
    string BaselineAsOfUtc);

public sealed record WeeklyInventoryMovementFact(
    string Sku,
    int WeekOffset,
    decimal OpeningOnHand,
    decimal ActualReceipts,
    decimal ActualDemand,
    decimal ActualConsumption,
    decimal InventoryAdjustment,
    decimal EndingOnHand,
    decimal OpenSupply,
    decimal QualifiedDemand,
    decimal EndingNetFlow,
    decimal DemandSpikeThreshold,
    string EventCode,
    string EvidenceStatus);

public sealed record OperatingBalanceBridgeFact(
    string MetricCode,
    string ItemKey,
    decimal HistoryClosingBalance,
    decimal IntervalIncrease,
    decimal IntervalDecrease,
    decimal Adjustment,
    decimal BaselineBalance,
    string EvidenceStatus);

public sealed record InternalDemoOperatingFactSet(
    InternalDemoFactSetHeader Header,
    IReadOnlyList<WeeklyInventoryMovementFact> InventoryMovements,
    IReadOnlyList<WeeklyOperatingFact> OperatingFacts,
    IReadOnlyList<HistoricalDemandActual> HistoricalDemand,
    IReadOnlyList<InventoryPosition> BaselineInventory,
    IReadOnlyList<OpeningBacklogEvidence> BaselineBacklog,
    decimal BaselineWorkInProcessUnits,
    IReadOnlyList<OperatingBalanceBridgeFact> BalanceBridges);

public interface IInternalDemoOperatingFactSource
{
    InternalDemoOperatingFactSet Load();
}
