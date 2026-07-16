namespace AdaptiveSopDdsop.Web.Domain;

public sealed record HistoryDistributionBucket(string Code, string Label, int Count, decimal Percent);

public sealed record HistoryInventoryPoint(
    int WeekOffset, string PeriodStartDate, decimal? EndingOnHand, decimal? OpenSupply,
    decimal? QualifiedDemand, decimal? NetFlow, decimal? TopOfRed, decimal? TopOfYellow,
    decimal? TopOfGreen, string Status, string Cause, string? ParameterSnapshotId, string EvidenceStatus);

public sealed record HistoryInventoryBufferView(
    string ControlPoint, string Sku, string Name, int DetailWindowWeeks,
    IReadOnlyList<HistoryInventoryPoint> Points,
    IReadOnlyList<HistoryDistributionBucket> Distribution,
    string EvidenceStatus);

public sealed record HistoryDdmrpSizingSnapshotView(
    string SnapshotId, string ControlPoint, string Sku, string Name,
    int EffectiveFromWeekOffset, int EffectiveThroughWeekOffset,
    SkuBufferSetting Setting, DdmrpSizingResult? Sizing,
    IReadOnlyList<BufferSizingLine> SizingLines, decimal? AverageOnHand,
    string SourceAuthority, string AsOfUtc, string EvidenceStatus);

public sealed record HistoryTimeBufferPoint(
    int WeekOffset, string PeriodStartDate, int? EarlyCount, int? GreenCount,
    int? YellowCount, int? RedCount, int? LateCount, decimal? AbnormalCost,
    string Cause, string EvidenceStatus);

public sealed record HistoryTimeBufferView(
    string BufferId, string ControlPoint, string ProtectedActivity,
    IReadOnlyList<HistoryTimeBufferPoint> Points,
    IReadOnlyList<HistoryDistributionBucket> Distribution,
    string EvidenceStatus);

public sealed record HistoryCapacityPoint(
    int WeekOffset, string PeriodStartDate, decimal? TheoreticalCapacity,
    decimal? StandardCapacity, decimal? DemonstratedCapacity,
    decimal? PlannedAvailableCapacity, decimal? CommittedLoad,
    decimal? ProtectionStart, decimal? ProtectiveCapacity,
    decimal? ConsumedProtection, decimal? RemainingProtection,
    string EvidenceStatus);

public sealed record HistoryCapacityBufferView(
    string ResourceCode, string ResourceName, string? ProtectedCcrResourceCode,
    string RelationshipRole, IReadOnlyList<HistoryCapacityPoint> Points,
    IReadOnlyList<HistoryDistributionBucket> Distribution, string EvidenceStatus);

public sealed record HistoryReviewProjection(
    IReadOnlyList<HistoryInventoryBufferView> InventoryBuffers,
    IReadOnlyList<HistoryDdmrpSizingSnapshotView> DdmrpSizingSnapshots,
    IReadOnlyList<HistoryTimeBufferView> TimeBuffers,
    IReadOnlyList<HistoryCapacityBufferView> CapacityBuffers);
