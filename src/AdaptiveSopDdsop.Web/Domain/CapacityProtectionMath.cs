namespace AdaptiveSopDdsop.Web.Domain;

public sealed record CapacityProtectionMeasure(
    decimal? UtilizationPercent,
    decimal? ProtectionStart,
    decimal? ProtectionCapacity,
    decimal? ConsumedProtection,
    decimal? RemainingProtection,
    decimal? Overload,
    string UtilizationBand,
    string EvidenceStatus,
    string? EvidenceIssue);

public static class CapacityProtectionMath
{
    private const string Complete = "Complete";
    private const string EvidenceMissing = "EvidenceMissing";
    private const decimal ProtectionStartPercent = 80m;

    public static CapacityProtectionMeasure CalculateUpstream(
        decimal? plannedAvailableCapacity,
        decimal? committedLoad,
        bool hasLaterProtectedCcrRouting,
        string evidenceStatus)
    {
        var issue = ValidateEvidence(
            plannedAvailableCapacity,
            committedLoad,
            evidenceStatus,
            hasLaterProtectedCcrRouting);
        if (issue is not null)
        {
            return Missing(issue);
        }

        var planned = plannedAvailableCapacity!.Value;
        var load = committedLoad!.Value;
        var utilization = load * 100m / planned;
        var protectionStart = planned * ProtectionStartPercent / 100m;
        var protectionCapacity = planned - protectionStart;
        var consumedProtection = Math.Clamp(load - protectionStart, 0m, protectionCapacity);
        var remainingProtection = Math.Max(0m, protectionCapacity - consumedProtection);
        var overload = Math.Max(0m, load - planned);

        return new CapacityProtectionMeasure(
            utilization,
            protectionStart,
            protectionCapacity,
            consumedProtection,
            remainingProtection,
            overload,
            Classify(utilization),
            Complete,
            null);
    }

    public static CapacityProtectionMeasure CalculateCcrReference(
        decimal? plannedAvailableCapacity,
        decimal? committedLoad,
        string evidenceStatus)
    {
        var issue = ValidateEvidence(
            plannedAvailableCapacity,
            committedLoad,
            evidenceStatus,
            hasLaterProtectedCcrRouting: true);
        if (issue is not null)
        {
            return Missing(issue);
        }

        var utilization = committedLoad!.Value * 100m / plannedAvailableCapacity!.Value;
        return new CapacityProtectionMeasure(
            utilization,
            null,
            null,
            null,
            null,
            null,
            Classify(utilization),
            Complete,
            null);
    }

    private static string? ValidateEvidence(
        decimal? plannedAvailableCapacity,
        decimal? committedLoad,
        string evidenceStatus,
        bool hasLaterProtectedCcrRouting)
    {
        if (!string.Equals(evidenceStatus, Complete, StringComparison.Ordinal))
        {
            return "能力证据不完整";
        }

        if (!hasLaterProtectedCcrRouting)
        {
            return "缺少同一产品后续受保护 CCR 工序证据";
        }

        if (plannedAvailableCapacity is null or <= 0m)
        {
            return "计划可用能力缺失或无效";
        }

        if (committedLoad is null or < 0m)
        {
            return "承诺负荷缺失或无效";
        }

        return null;
    }

    private static string Classify(decimal utilizationPercent) => utilizationPercent switch
    {
        <= 60m => "Green",
        <= 80m => "Yellow",
        <= 100m => "Red",
        _ => "DeepRed"
    };

    private static CapacityProtectionMeasure Missing(string issue) => new(
        null,
        null,
        null,
        null,
        null,
        null,
        EvidenceMissing,
        EvidenceMissing,
        issue);
}
