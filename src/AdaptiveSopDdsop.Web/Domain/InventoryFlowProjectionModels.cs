namespace AdaptiveSopDdsop.Web.Domain;

public sealed record InventoryFlowPoint(
    string Sku,
    int Week,
    decimal OpeningOnHand,
    decimal OpeningBacklog,
    decimal Demand,
    decimal FrozenReceiptQuantity,
    decimal SimulatedReceiptQuantity,
    decimal PrebuildReceiptQuantity,
    decimal FulfilledOpeningBacklog,
    decimal FulfilledNewDemandOnTime,
    decimal TotalFulfilledDemand,
    decimal EndingOnHand,
    decimal EndingBacklog,
    decimal EndingInventoryValue,
    decimal? WeeklyServicePercent,
    string WeeklyServiceStatus);

public sealed record InventoryReceiptLogEntry(
    string Sku,
    string SourceKind,
    string SourceId,
    int? RecommendationWeek,
    int ArrivalWeek,
    decimal RequestedQuantity,
    decimal AcceptedQuantity,
    decimal DeferredQuantity,
    decimal OutsideHorizonQuantity,
    string EvidenceStatus,
    string EvidenceSource,
    string Explanation,
    string? Supplier = null,
    string? MaterialFamily = null,
    decimal? CapacityLimit = null,
    decimal RoundingResidual = 0m);

public sealed record InventoryFlowSkuSummary(
    string Sku,
    decimal? OnTimeServicePercent,
    string ServiceStatus,
    decimal TotalNewDemandQuantity,
    decimal FulfilledNewDemandOnTime,
    decimal TotalFulfilledQuantity,
    decimal AverageInventoryValue,
    decimal PeakInventoryValue,
    decimal EndingInventoryValue,
    decimal EndingBacklog,
    int? BacklogRecoveryWeek,
    decimal FrozenReceiptQuantity,
    decimal SimulatedReceiptQuantity,
    decimal PrebuildReceiptQuantity,
    decimal OutsideHorizonQuantity);

public sealed record InventoryFlowSummary(
    decimal? OnTimeServicePercent,
    string ServiceStatus,
    decimal TotalNewDemandQuantity,
    decimal FulfilledNewDemandOnTime,
    decimal TotalFulfilledQuantity,
    decimal AverageInventoryValue,
    decimal PeakInventoryValue,
    decimal EndingInventoryValue,
    decimal EndingBacklog,
    int? BacklogRecoveryWeek,
    decimal FrozenReceiptQuantity,
    decimal SimulatedReceiptQuantity,
    decimal PrebuildReceiptQuantity,
    decimal OutsideHorizonQuantity);

public sealed record InventoryFlowTrace(
    string Stage,
    string? Sku,
    int? Week,
    string? SourceId,
    string Explanation);

public sealed record InventoryFlowProjectionResult(
    string CaseId,
    string Status,
    IReadOnlyList<InventoryFlowPoint> Points,
    IReadOnlyList<InventoryReceiptLogEntry> ReceiptLog,
    IReadOnlyList<InventoryFlowSkuSummary> SkuSummaries,
    InventoryFlowSummary? Summary,
    IReadOnlyList<InventoryFlowTrace> Trace,
    IReadOnlyList<PlanningEvidenceIssue> Issues,
    string? BaselineSnapshotId = null);

public sealed record ScenarioMetricEvidence(
    string JsonPath,
    string EvidenceStatus,
    string Source,
    string Explanation,
    string? ProjectionCaseId = null,
    string? BaselineSnapshotId = null);
