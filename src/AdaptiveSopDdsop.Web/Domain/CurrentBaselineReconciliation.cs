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
        if (reconciliation.EvidenceStatus != "Complete")
        {
            issues.Add("对账总体证据不完整");
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
            var expected = line.HistoryClosingBalance + line.IntervalIncrease
                - line.IntervalDecrease + line.Adjustment;
            var recomputedDifference = decimal.Round(line.BaselineBalance - expected, 2);
            if (line.EvidenceStatus != "Complete")
            {
                issues.Add($"{line.MetricCode}/{line.ItemKey} 证据不完整");
            }
            if (Math.Abs(recomputedDifference) > 0.01m)
            {
                issues.Add($"{line.MetricCode}/{line.ItemKey} 重算差异为 {recomputedDifference}");
            }
            if (Math.Abs(line.Difference - recomputedDifference) > 0.01m)
            {
                issues.Add($"{line.MetricCode}/{line.ItemKey} 报送差异与重算不一致");
            }
        }

        return issues;
    }

    public static IReadOnlyList<string> Validate(
        CurrentBaselineHistoryReconciliation? reconciliation,
        IReadOnlyCollection<string> expectedSkuKeys,
        IReadOnlyCollection<string> expectedResourceCodes)
    {
        var issues = Validate(reconciliation).ToList();
        if (reconciliation is null || reconciliation.Lines is null || reconciliation.Lines.Count == 0)
        {
            return issues;
        }

        var skuKeys = expectedSkuKeys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .ToList();
        var resourceCodes = expectedResourceCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .ToList();
        if (skuKeys.Distinct(StringComparer.Ordinal).Count() != skuKeys.Count)
        {
            issues.Add("候选库存 SKU 键重复");
        }
        if (resourceCodes.Distinct(StringComparer.Ordinal).Count() != resourceCodes.Count)
        {
            issues.Add("候选资源能力键重复");
        }
        if (skuKeys.Distinct(StringComparer.Ordinal).Count() != 12)
        {
            issues.Add("当前基线必须包含 12 个库存 SKU 对账键");
        }

        var expectedKeys = skuKeys
            .Select(key => (MetricCode: "ON_HAND", ItemKey: key))
            .Append((MetricCode: "INVENTORY_VALUE", ItemKey: "ALL"))
            .Append((MetricCode: "WORK_IN_PROCESS", ItemKey: "ALL"))
            .Append((MetricCode: "BACKLOG", ItemKey: "ALL"))
            .Concat(resourceCodes.Select(code => (MetricCode: "RESOURCE_AVAILABLE_CAPACITY", ItemKey: code)))
            .ToHashSet();
        var actualKeys = reconciliation.Lines
            .Select(line => (MetricCode: line.MetricCode, ItemKey: line.ItemKey))
            .ToHashSet();
        foreach (var missing in expectedKeys.Except(actualKeys))
        {
            issues.Add($"缺少必需对账键：{missing.MetricCode}/{missing.ItemKey}");
        }
        foreach (var unexpected in actualKeys.Except(expectedKeys))
        {
            issues.Add($"存在非预期对账键：{unexpected.MetricCode}/{unexpected.ItemKey}");
        }
        if (reconciliation.Lines.Count(line => line.MetricCode == "ON_HAND") != 12)
        {
            issues.Add("ON_HAND 对账行必须恰好为 12 行");
        }

        return issues;
    }
}
