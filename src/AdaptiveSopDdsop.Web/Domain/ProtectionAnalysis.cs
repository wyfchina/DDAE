namespace AdaptiveSopDdsop.Web.Domain;

internal static class ProtectionProductEligibility
{
    internal const string InventoryOnlyFpgaSku = "AV-FPGA-203";

    internal static bool IsEligible(string sku) =>
        !string.Equals(sku, InventoryOnlyFpgaSku, StringComparison.Ordinal);
}

public sealed record ProtectionBreachResult(
    string ScopeType,
    string Target,
    bool IsBreached,
    int? EarliestRedWeek,
    int ConsecutiveRiskWeeks,
    int? RecoveryWeek,
    bool IsUnrecovered,
    IReadOnlyList<string> AffectedProducts,
    string PrimaryCause,
    decimal? BufferSize = null,
    decimal? MaximumPenetrationPercent = null,
    string? Unit = null,
    string EvidenceStatus = "Complete");

public sealed record TimeBufferProjectionPoint(
    string BufferId,
    string ControlPoint,
    int Week,
    decimal? DelayDays,
    decimal BufferDays,
    decimal? PenetrationPercent,
    string Status,
    string EvidenceStatus,
    string Cause);

public sealed record CapacityProtectionProjectionPoint(
    string ProtectionId,
    string UpstreamResourceCode,
    string ProtectedCcrResourceCode,
    int Week,
    decimal? PlannedAvailableCapacity,
    decimal? CommittedLoad,
    decimal? ProtectionCapacity,
    decimal? ConsumedProtection,
    decimal? RemainingProtection,
    string Status,
    string EvidenceStatus);

public sealed record ProtectionScopeAnalysis<TPoint>(
    IReadOnlyList<TPoint> Projection,
    IReadOnlyList<ProtectionBreachResult> Breaches);

public interface ITimeBufferProtectionAnalyzer
{
    ProtectionScopeAnalysis<TimeBufferProjectionPoint> Analyze(
        ScenarioWorkspaceDataSet frozenData,
        ExternalScenarioDefinition externalScenario,
        ScenarioRunParameterSet? response,
        int horizonWeeks);
}

public interface ICapacityBufferProtectionAnalyzer
{
    ProtectionScopeAnalysis<CapacityProtectionProjectionPoint> Analyze(
        ScenarioWorkspaceDataSet frozenData,
        ScenarioRunPreviewCase previewCase,
        int horizonWeeks);
}

public sealed record ProtectionAnalysisResult(
    IReadOnlyList<ProtectionBreachResult> Breaches,
    IReadOnlyList<TimeBufferProjectionPoint> TimeBufferProjection,
    IReadOnlyList<CapacityProtectionProjectionPoint> CapacityProtectionProjection);

public sealed class InventoryProtectionAnalyzer
{
    public IReadOnlyList<ProtectionBreachResult> Analyze(ScenarioRunPreviewCase previewCase)
    {
        var results = previewCase.BufferTrend.WeeklyCells
            .GroupBy(item => item.Sku, StringComparer.Ordinal)
            .Select(group => StatusSeriesBreachCalculator.Calculate(
                "InventoryBuffer",
                group.Key,
                group.Select(item => (item.Week, item.Status)),
                new[] { group.Key },
                "库存保护带被需求与补货时序击穿"))
            .ToList();

        if (results.Count == 0)
        {
            results.Add(NotCalculated("InventoryBuffer", "无库存对象", "EvidenceMissing"));
        }

        return results;
    }

    internal static ProtectionBreachResult NotCalculated(string scopeType, string target, string evidenceStatus)
    {
        var cause = evidenceStatus == "NotApplicable"
            ? "没有适用定义"
            : "没有可计算证据，未按零处理";
        return new ProtectionBreachResult(
            scopeType,
            target,
            false,
            null,
            0,
            null,
            false,
            Array.Empty<string>(),
            cause,
            EvidenceStatus: evidenceStatus);
    }
}

public sealed class SupplyRiskAnalyzer
{
    public IReadOnlyList<ProtectionBreachResult> Analyze(ScenarioRunPreviewCase previewCase)
    {
        var results = previewCase.Constraints.SupplyCells
            .GroupBy(item => new { item.Supplier, item.MaterialFamily })
            .Select(group =>
            {
                var affected = previewCase.SupplierCollaboration.SkuRequirements
                    .Where(item => item.Supplier == group.Key.Supplier && item.MaterialFamily == group.Key.MaterialFamily)
                    .Select(item => item.Sku)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(item => item, StringComparer.Ordinal)
                    .ToList();
                return StatusSeriesBreachCalculator.Calculate(
                    "SupplyRisk",
                    $"{group.Key.Supplier}/{group.Key.MaterialFamily}",
                    group.Select(item => (item.Week, item.Status)),
                    affected,
                    "不受限需求超过供应商承诺能力");
            })
            .ToList();

        if (results.Count == 0)
        {
            results.Add(InventoryProtectionAnalyzer.NotCalculated("SupplyRisk", "无供应对象", "EvidenceMissing"));
        }

        return results;
    }
}

public sealed class TimeBufferProtectionAnalyzer : ITimeBufferProtectionAnalyzer
{
    public ProtectionScopeAnalysis<TimeBufferProjectionPoint> Analyze(
        ScenarioWorkspaceDataSet frozenData,
        ExternalScenarioDefinition externalScenario,
        ScenarioRunParameterSet? response,
        int horizonWeeks)
    {
        var definitions = frozenData.TimeBuffers ?? Array.Empty<TimeBufferDefinition>();
        if (definitions.Count == 0)
        {
            return new ProtectionScopeAnalysis<TimeBufferProjectionPoint>(
                Array.Empty<TimeBufferProjectionPoint>(),
                new[] { InventoryProtectionAnalyzer.NotCalculated("TimeBuffer", "无时间缓冲定义", "NotApplicable") });
        }

        var scopeRows = frozenData.TimeBufferProductScopes ?? Array.Empty<TimeBufferProductScope>();
        var progressRows = frozenData.ControlPointProgress ?? Array.Empty<ControlPointProgressFact>();
        var horizon = Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52);
        var projection = new List<TimeBufferProjectionPoint>();
        var breaches = new List<ProtectionBreachResult>();

        foreach (var definition in definitions)
        {
            var matchingScopes = scopeRows.Where(item => item.BufferId == definition.BufferId).ToList();
            var productScope = matchingScopes.Count == 1 ? matchingScopes[0] : null;
            var productResolution = ResolveAffectedProducts(frozenData, productScope);
            var progressByWeek = progressRows
                .Where(item => item.BufferId == definition.BufferId && item.Week >= 1 && item.Week <= horizon)
                .GroupBy(item => item.Week)
                .ToDictionary(group => group.Key, group => group.ToList());
            var progressComplete = Enumerable.Range(1, horizon).All(week =>
                progressByWeek.TryGetValue(week, out var facts) &&
                facts.Count == 1 &&
                facts[0].ObservedDelayDays.HasValue &&
                facts[0].EvidenceStatus == "Complete");
            var scopeComplete = productResolution.EvidenceComplete;
            var evidenceComplete = definition.EvidenceStatus == "Complete" &&
                definition.BufferDays > 0m &&
                progressComplete &&
                scopeComplete;

            if (!evidenceComplete)
            {
                foreach (var week in Enumerable.Range(1, horizon))
                {
                    projection.Add(new TimeBufferProjectionPoint(
                        definition.BufferId,
                        definition.ControlPoint,
                        week,
                        null,
                        definition.BufferDays,
                        null,
                        "EvidenceMissing",
                        "EvidenceMissing",
                        MissingTimeEvidenceCause(definition, scopeComplete, progressByWeek, week)));
                }

                breaches.Add(new ProtectionBreachResult(
                    "TimeBuffer",
                    definition.BufferId,
                    false,
                    null,
                    0,
                    null,
                    false,
                    Array.Empty<string>(),
                    "时间缓冲证据缺失，未按零处理",
                    definition.BufferDays > 0m ? definition.BufferDays : null,
                    null,
                    "days",
                    "EvidenceMissing"));
                continue;
            }

            var bufferProjection = Enumerable.Range(1, horizon)
                .Select(week =>
                {
                    var progress = progressByWeek[week][0];
                    var scenarioDelay = (externalScenario.TimeDelays ?? Array.Empty<ExternalTimeDelay>())
                        .Where(item => item.BufferId == definition.BufferId && week >= item.StartWeek && week <= item.EndWeek)
                        .Sum(item => item.DelayDays);
                    var recoveredDays = (response?.TimeBufferAdjustments ?? Array.Empty<TimeBufferResponseAdjustment>())
                        .Where(item => item.BufferId == definition.BufferId && week >= item.StartWeek && week <= item.EndWeek)
                        .Sum(item => item.RecoveredDays);
                    var rawNetDelay = Math.Max(
                        0m,
                        progress.ObservedDelayDays!.Value + scenarioDelay - recoveredDays);
                    var rawPenetration = rawNetDelay * 100m / definition.BufferDays;
                    var netDelay = decimal.Round(rawNetDelay, 1);
                    var penetration = decimal.Round(rawPenetration, 1);
                    var status = rawPenetration >= 100m
                        ? "Red"
                        : rawPenetration >= 67m
                            ? "Yellow"
                            : "Green";
                    return new TimeBufferProjectionPoint(
                        definition.BufferId,
                        definition.ControlPoint,
                        week,
                        netDelay,
                        definition.BufferDays,
                        penetration,
                        status,
                        "Complete",
                        BuildTimeCause(progress, externalScenario, response, week));
                })
                .ToList();
            projection.AddRange(bufferProjection);

            var maximumPenetration = bufferProjection.Max(item => item.PenetrationPercent!.Value);
            var breach = StatusSeriesBreachCalculator.Calculate(
                "TimeBuffer",
                definition.BufferId,
                bufferProjection.Select(item => (item.Week, item.Status)),
                productResolution.AffectedProducts,
                "控制点净延迟侵入时间缓冲红区",
                definition.BufferDays,
                maximumPenetration,
                "days");
            breaches.Add(breach);
        }

        return new ProtectionScopeAnalysis<TimeBufferProjectionPoint>(
            projection,
            breaches);
    }

    private static string MissingTimeEvidenceCause(
        TimeBufferDefinition definition,
        bool scopeComplete,
        IReadOnlyDictionary<int, List<ControlPointProgressFact>> progressByWeek,
        int week)
    {
        if (definition.EvidenceStatus != "Complete" || definition.BufferDays <= 0m)
        {
            return "时间缓冲定义证据缺失";
        }
        if (!scopeComplete)
        {
            return "时间缓冲产品范围证据缺失";
        }
        if (!progressByWeek.TryGetValue(week, out var facts) || facts.Count != 1)
        {
            return $"第 {week} 周控制点进展证据缺失";
        }
        return facts[0].EvidenceStatus == "Complete" && facts[0].ObservedDelayDays.HasValue
            ? "其他周控制点进展证据缺失"
            : $"第 {week} 周控制点进展证据缺失";
    }

    private static (bool EvidenceComplete, IReadOnlyList<string> AffectedProducts) ResolveAffectedProducts(
        ScenarioWorkspaceDataSet frozenData,
        TimeBufferProductScope? productScope)
    {
        if (productScope is null || productScope.EvidenceStatus != "Complete")
        {
            return (false, Array.Empty<string>());
        }

        var knownSkus = frozenData.Skus
            .Select(item => item.Sku)
            .ToHashSet(StringComparer.Ordinal);
        var knownFamilies = frozenData.Skus
            .Select(item => item.Family)
            .ToHashSet(StringComparer.Ordinal);
        if (productScope.Skus.Any(item => !knownSkus.Contains(item)) ||
            productScope.ProductFamilies.Any(item => !knownFamilies.Contains(item)))
        {
            return (false, Array.Empty<string>());
        }

        var scopedFamilies = productScope.ProductFamilies.ToHashSet(StringComparer.Ordinal);
        var affectedProducts = productScope.Skus
            .Concat(frozenData.Skus
                .Where(item => scopedFamilies.Contains(item.Family))
                .Select(item => item.Sku))
            .Where(ProtectionProductEligibility.IsEligible)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        return affectedProducts.Count > 0
            ? (true, affectedProducts)
            : (false, Array.Empty<string>());
    }

    private static string BuildTimeCause(
        ControlPointProgressFact progress,
        ExternalScenarioDefinition externalScenario,
        ScenarioRunParameterSet? response,
        int week)
    {
        var causes = new List<string>();
        if (!string.IsNullOrWhiteSpace(progress.Cause))
        {
            causes.Add(progress.Cause);
        }
        causes.AddRange((externalScenario.TimeDelays ?? Array.Empty<ExternalTimeDelay>())
            .Where(item => item.BufferId == progress.BufferId && week >= item.StartWeek && week <= item.EndWeek)
            .Select(item => item.Reason));
        causes.AddRange((response?.TimeBufferAdjustments ?? Array.Empty<TimeBufferResponseAdjustment>())
            .Where(item => item.BufferId == progress.BufferId && week >= item.StartWeek && week <= item.EndWeek)
            .Select(item => item.Reason));
        return string.Join("；", causes.Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal));
    }
}

public sealed class CapacityBufferProtectionAnalyzer : ICapacityBufferProtectionAnalyzer
{
    public ProtectionScopeAnalysis<CapacityProtectionProjectionPoint> Analyze(
        ScenarioWorkspaceDataSet frozenData,
        ScenarioRunPreviewCase previewCase,
        int horizonWeeks)
    {
        var definitions = frozenData.CapacityProtections ?? Array.Empty<CapacityProtectionDefinition>();
        if (definitions.Count == 0)
        {
            return new ProtectionScopeAnalysis<CapacityProtectionProjectionPoint>(
                Array.Empty<CapacityProtectionProjectionPoint>(),
                new[] { InventoryProtectionAnalyzer.NotCalculated("CapacityBuffer", "无能力保护定义", "NotApplicable") });
        }

        var horizon = Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52);
        var projection = new List<CapacityProtectionProjectionPoint>();
        var breaches = new List<ProtectionBreachResult>();

        foreach (var definition in definitions)
        {
            var affectedProducts = FindProtectedProducts(frozenData.ResourceRoutings, definition);
            var cellsByWeek = previewCase.Constraints.CapacityCells
                .Where(item => item.ResourceCode == definition.UpstreamResourceCode && item.Week >= 1 && item.Week <= horizon)
                .GroupBy(item => item.Week)
                .ToDictionary(group => group.Key, group => group.ToList());
            var evidenceComplete = definition.EvidenceStatus == "Complete" &&
                definition.ReservePercent > 0m &&
                affectedProducts.Count > 0 &&
                Enumerable.Range(1, horizon).All(week =>
                    cellsByWeek.TryGetValue(week, out var cells) && cells.Count == 1);

            if (!evidenceComplete)
            {
                foreach (var week in Enumerable.Range(1, horizon))
                {
                    projection.Add(new CapacityProtectionProjectionPoint(
                        definition.ProtectionId,
                        definition.UpstreamResourceCode,
                        definition.ProtectedCcrResourceCode,
                        week,
                        null,
                        null,
                        null,
                        null,
                        null,
                        "EvidenceMissing",
                        "EvidenceMissing"));
                }
                breaches.Add(new ProtectionBreachResult(
                    "CapacityBuffer",
                    definition.UpstreamResourceCode,
                    false,
                    null,
                    0,
                    null,
                    false,
                    Array.Empty<string>(),
                    "能力保护定义、顺序或周度能力证据缺失，未按零处理",
                    Unit: "capacity",
                    EvidenceStatus: "EvidenceMissing"));
                continue;
            }

            var reservePercent = Math.Clamp(definition.ReservePercent, 0m, 100m);
            var definitionProjection = Enumerable.Range(1, horizon)
                .Select(week =>
                {
                    var cell = cellsByWeek[week][0];
                    var plannedAvailable = Math.Max(0m, cell.ConstrainedAvailable);
                    var committedLoad = Math.Max(0m, cell.UnconstrainedRequired);
                    var protectionCapacity = decimal.Round(plannedAvailable * reservePercent / 100m, 1);
                    var protectionStart = plannedAvailable - protectionCapacity;
                    var consumedProtection = decimal.Round(
                        Math.Clamp(committedLoad - protectionStart, 0m, protectionCapacity),
                        1);
                    var remainingProtection = decimal.Round(protectionCapacity - consumedProtection, 1);
                    var status = remainingProtection <= 0m
                        ? "Red"
                        : consumedProtection > 0m
                            ? "Yellow"
                            : "Green";
                    return new CapacityProtectionProjectionPoint(
                        definition.ProtectionId,
                        definition.UpstreamResourceCode,
                        definition.ProtectedCcrResourceCode,
                        week,
                        plannedAvailable,
                        committedLoad,
                        protectionCapacity,
                        consumedProtection,
                        remainingProtection,
                        status,
                        "Complete");
                })
                .ToList();
            projection.AddRange(definitionProjection);

            var maximumPenetration = definitionProjection.Max(item =>
                item.ProtectionCapacity > 0m
                    ? decimal.Round(item.ConsumedProtection!.Value * 100m / item.ProtectionCapacity.Value, 1)
                    : 0m);
            breaches.Add(StatusSeriesBreachCalculator.Calculate(
                "CapacityBuffer",
                definition.UpstreamResourceCode,
                definitionProjection.Select(item => (item.Week, item.Status)),
                affectedProducts,
                $"{definition.UpstreamResourceCode} 上游能力保护被承诺负荷耗尽",
                definitionProjection.Max(item => item.ProtectionCapacity),
                maximumPenetration,
                "capacity"));
        }

        return new ProtectionScopeAnalysis<CapacityProtectionProjectionPoint>(
            projection,
            breaches);
    }

    private static IReadOnlyList<string> FindProtectedProducts(
        IReadOnlyList<ResourceRouting> routings,
        CapacityProtectionDefinition definition)
    {
        return routings
            .Where(item =>
                item.ResourceCode == definition.UpstreamResourceCode &&
                item.ProtectsCcrResourceCode == definition.ProtectedCcrResourceCode &&
                item.OperationSequence > 0 &&
                item.EvidenceStatus == "Complete")
            .Where(upstream => routings.Any(downstream =>
                downstream.Sku == upstream.Sku &&
                downstream.ResourceCode == definition.ProtectedCcrResourceCode &&
                downstream.OperationSequence > upstream.OperationSequence &&
                downstream.EvidenceStatus == "Complete"))
            .Select(item => item.Sku)
            .Where(ProtectionProductEligibility.IsEligible)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
    }
}

public static class ProtectionBreachAnalyzer
{
    public static ProtectionAnalysisResult Analyze(
        ScenarioWorkspaceDataSet frozenData,
        ExternalScenarioDefinition externalScenario,
        ScenarioRunParameterSet? response,
        ScenarioRunPreviewCase previewCase,
        int horizonWeeks)
    {
        var inventory = new InventoryProtectionAnalyzer().Analyze(previewCase);
        var time = new TimeBufferProtectionAnalyzer().Analyze(frozenData, externalScenario, response, horizonWeeks);
        var capacity = new CapacityBufferProtectionAnalyzer().Analyze(frozenData, previewCase, horizonWeeks);
        var supply = new SupplyRiskAnalyzer().Analyze(previewCase);
        return new ProtectionAnalysisResult(
            inventory.Concat(time.Breaches).Concat(capacity.Breaches).Concat(supply).ToList(),
            time.Projection,
            capacity.Projection);
    }
}

internal static class StatusSeriesBreachCalculator
{
    internal static ProtectionBreachResult Calculate(
        string scopeType,
        string target,
        IEnumerable<(int Week, string Status)> values,
        IReadOnlyList<string> affectedProducts,
        string primaryCause,
        decimal? bufferSize = null,
        decimal? maximumPenetrationPercent = null,
        string? unit = null,
        string evidenceStatus = "Complete")
    {
        var series = values.OrderBy(item => item.Week).ToList();
        var firstRedIndex = series.FindIndex(item => item.Status == "Red");
        if (firstRedIndex < 0)
        {
            return new ProtectionBreachResult(
                scopeType,
                target,
                false,
                null,
                0,
                null,
                false,
                affectedProducts,
                "展望期内未发生保护击穿",
                bufferSize,
                maximumPenetrationPercent,
                unit,
                evidenceStatus);
        }

        var maximumDuration = 0;
        var currentDuration = 0;
        foreach (var point in series)
        {
            if (point.Status == "Red")
            {
                currentDuration++;
                maximumDuration = Math.Max(maximumDuration, currentDuration);
            }
            else
            {
                currentDuration = 0;
            }
        }

        var lastRedIndex = series.FindLastIndex(item => item.Status == "Red");
        var recoveryIndex = lastRedIndex + 1;
        var recoveryWeek = recoveryIndex < series.Count ? series[recoveryIndex].Week : (int?)null;
        return new ProtectionBreachResult(
            scopeType,
            target,
            true,
            series[firstRedIndex].Week,
            maximumDuration,
            recoveryWeek,
            recoveryWeek is null,
            affectedProducts,
            primaryCause,
            bufferSize,
            maximumPenetrationPercent,
            unit,
            evidenceStatus);
    }
}
