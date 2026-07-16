namespace AdaptiveSopDdsop.Web.Domain;

public sealed record PlanningEvidenceCoverage(
    DateOnly AnchorDate,
    int CoverageFromWeek,
    int CoverageThroughWeek,
    string EvidenceStatus);

public sealed record ConfirmedReceiptEvidence(
    string ReceiptId,
    string Sku,
    decimal Quantity,
    int ExpectedReceiptWeek,
    DateOnly? ExpectedReceiptDate,
    string ReceiptType,
    string SourceReference,
    string SupplySourceType,
    string? Supplier,
    string? MaterialFamily,
    string ConfirmationStatus,
    string EvidenceStatus,
    string AsOfUtc,
    string EvidenceLabel,
    string? SourceTimestampUtc = null);

public sealed record OpeningBacklogEvidence(
    string BacklogId,
    string Sku,
    decimal Quantity,
    string SourceReference,
    string EvidenceStatus,
    string AsOfUtc,
    string EvidenceLabel);

public sealed record PlanningEvidenceIssue(
    string Scope,
    string? Sku,
    int? Week,
    string Reason,
    string? SourceId,
    bool BlocksFreeze,
    bool BlocksProjection);

public sealed record PlanningEvidenceValidationResult(
    string Status,
    IReadOnlyList<PlanningEvidenceIssue> Issues);
