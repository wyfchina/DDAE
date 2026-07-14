namespace AdaptiveSopDdsop.Web.Domain;

public sealed record TimeBufferDefinition(
    string BufferId,
    string ControlPoint,
    string ProtectedActivity,
    decimal BufferDays,
    bool IsCritical,
    string Applicability,
    string EvidenceStatus);

public sealed record TimeBufferProductScope(
    string BufferId,
    IReadOnlyList<string> ProductFamilies,
    IReadOnlyList<string> Skus,
    string EvidenceStatus);

public sealed record ControlPointProgressFact(
    string BufferId,
    int Week,
    decimal? ObservedDelayDays,
    string Cause,
    string EvidenceStatus);

public sealed record CapacityProtectionDefinition(
    string ProtectionId,
    string UpstreamResourceCode,
    string ProtectedCcrResourceCode,
    decimal ReservePercent,
    string Applicability,
    string EvidenceStatus);

public sealed record ExternalTimeDelay(
    string BufferId,
    int StartWeek,
    int EndWeek,
    decimal DelayDays,
    string Reason);

public sealed record TimeBufferResponseAdjustment(
    string BufferId,
    int StartWeek,
    int EndWeek,
    decimal RecoveredDays,
    string Reason);
