namespace AdaptiveSopDdsop.Web.Domain;

public sealed record BaselineReconciliationLine(
    string MetricCode,
    string ItemKey,
    decimal HistoryClosingBalance,
    decimal IntervalIncrease,
    decimal IntervalDecrease,
    decimal Adjustment,
    decimal BaselineBalance,
    decimal Difference,
    string EvidenceStatus,
    string? DifferenceReason = null);

public sealed record CurrentBaselineHistoryReconciliation(
    string FactSetId,
    string HistoryThroughUtc,
    string BaselineAsOfUtc,
    string ScopeLabel,
    IReadOnlyList<BaselineReconciliationLine> Lines,
    string EvidenceStatus);

public static class CurrentBaselineReconciliation
{
    public static CurrentBaselineHistoryReconciliation Build(InternalDemoOperatingFactSet facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        var lines = facts.BalanceBridges.Select(bridge =>
        {
            var expected = bridge.HistoryClosingBalance + bridge.IntervalIncrease
                - bridge.IntervalDecrease + bridge.Adjustment;
            var difference = decimal.Round(bridge.BaselineBalance - expected, 2);
            var complete = bridge.EvidenceStatus == "Complete" && Math.Abs(difference) <= 0.01m;
            return new BaselineReconciliationLine(
                bridge.MetricCode,
                bridge.ItemKey,
                bridge.HistoryClosingBalance,
                bridge.IntervalIncrease,
                bridge.IntervalDecrease,
                bridge.Adjustment,
                bridge.BaselineBalance,
                difference,
                complete ? "Complete" : "EvidenceMissing",
                complete
                    ? null
                    : bridge.EvidenceStatus != "Complete"
                        ? $"EvidenceStatus={bridge.EvidenceStatus}"
                        : $"Difference={difference}");
        }).ToList();
        var complete = lines.Count > 0 && lines.All(line =>
            line.EvidenceStatus == "Complete" && Math.Abs(line.Difference) <= 0.01m);

        return new CurrentBaselineHistoryReconciliation(
            facts.Header.FactSetId,
            facts.Header.HistoryThroughUtc,
            facts.Header.BaselineAsOfUtc,
            "Historical closing balances to current baseline",
            lines,
            complete ? "Complete" : "EvidenceMissing");
    }

    public static IReadOnlyList<string> Validate(CurrentBaselineHistoryReconciliation? reconciliation)
    {
        var issues = new List<string>();
        if (reconciliation is null)
        {
            issues.Add("缺少历史期末与当前基线对账血缘");
            return issues;
        }

        if (string.IsNullOrWhiteSpace(reconciliation.FactSetId))
        {
            issues.Add("对账事实集标识为空");
        }

        if (!DateTimeOffset.TryParse(reconciliation.HistoryThroughUtc, out var historyThrough) ||
            !DateTimeOffset.TryParse(reconciliation.BaselineAsOfUtc, out var baselineAsOf) ||
            historyThrough >= baselineAsOf)
        {
            issues.Add("历史截止时间必须早于基线时点");
        }

        if (reconciliation.Lines is null || reconciliation.Lines.Count == 0)
        {
            issues.Add("对账行为空");
            return issues;
        }

        foreach (var duplicate in reconciliation.Lines
                     .GroupBy(line => (line.MetricCode, line.ItemKey))
                     .Where(group => group.Count() > 1))
        {
            issues.Add($"重复对账行：{duplicate.Key.MetricCode}/{duplicate.Key.ItemKey}");
        }

        foreach (var line in reconciliation.Lines)
        {
            if (line.EvidenceStatus != "Complete")
            {
                issues.Add($"{line.MetricCode}/{line.ItemKey} 证据不完整");
            }
            if (Math.Abs(line.Difference) > 0.01m)
            {
                issues.Add($"{line.MetricCode}/{line.ItemKey} 差异为 {line.Difference}");
            }
        }

        return issues;
    }
}
