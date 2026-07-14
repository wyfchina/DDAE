namespace AdaptiveSopDdsop.Web.Domain;

public sealed record ResponseConfiguration(
    string ResponseId,
    string Name,
    ScenarioRunParameterSet Parameters);

public sealed record ScenarioComparisonRequest(
    string BaselineSnapshotId,
    ExternalScenarioDefinition ExternalScenario,
    IReadOnlyList<ResponseConfiguration>? ResponseOptions = null,
    int HorizonWeeks = 12);

public sealed record FrozenComparisonGovernanceProposalRequest(
    ScenarioComparisonRequest Comparison,
    string ResponseId,
    GovernanceDecisionContext? GovernanceContext = null);

public sealed record ProtectionBreachResult(
    string ScopeType,
    string Target,
    bool IsBreached,
    int? EarliestRedWeek,
    int ConsecutiveRiskWeeks,
    int? RecoveryWeek,
    bool IsUnrecovered,
    IReadOnlyList<string> AffectedProducts,
    string PrimaryCause);

public sealed record ScenarioComparisonCase(
    string ResponseId,
    string Name,
    string ExternalScenarioId,
    ScenarioRunPreviewResult Preview,
    IReadOnlyList<ProtectionBreachResult> Breaches);

public sealed record ScenarioComparisonResult(
    string BaselineSnapshotId,
    string BaselineSnapshotNumber,
    ExternalScenarioDefinition ExternalScenario,
    ScenarioComparisonCase NoResponse,
    IReadOnlyList<ScenarioComparisonCase> ResponseCases)
{
    public IReadOnlyList<ScenarioComparisonCase> AllCases => new[] { NoResponse }.Concat(ResponseCases).ToList();
}

public sealed class ScenarioComparisonService
{
    private readonly CurrentBaselineService _baselineService;
    private readonly ScenarioRunPreviewService _previewService;

    public ScenarioComparisonService(CurrentBaselineService baselineService, ScenarioRunPreviewService previewService)
    {
        _baselineService = baselineService;
        _previewService = previewService;
    }

    public ScenarioComparisonResult Compare(ScenarioComparisonRequest request)
    {
        var baseline = _baselineService.GetDetail(request.BaselineSnapshotId)
            ?? throw new ArgumentException("来源基线不存在。", nameof(request));
        if (!string.Equals(baseline.Status, "Frozen", StringComparison.Ordinal))
        {
            throw new ArgumentException("场景比较只能使用已冻结基线。", nameof(request));
        }
        if (string.IsNullOrWhiteSpace(request.ExternalScenario?.ScenarioId))
        {
            throw new ArgumentException("外部场景标识不能为空。", nameof(request));
        }
        var responseOptions = request.ResponseOptions ?? Array.Empty<ResponseConfiguration>();
        if (responseOptions.Any(option => string.IsNullOrWhiteSpace(option.ResponseId) || string.IsNullOrWhiteSpace(option.Name)))
        {
            throw new ArgumentException("响应方案必须提供标识和名称。", nameof(request));
        }
        if (responseOptions.Any(option => string.Equals(option.ResponseId, "NO_RESPONSE", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("响应方案不能使用保留标识 NO_RESPONSE。", nameof(request));
        }
        if (responseOptions.GroupBy(option => option.ResponseId, StringComparer.Ordinal).Any(group => group.Count() > 1))
        {
            throw new ArgumentException("同一次比较中的响应方案标识必须唯一。", nameof(request));
        }

        var horizonWeeks = Math.Clamp(request.HorizonWeeks <= 0 ? 12 : request.HorizonWeeks, 1, 52);
        var noResponsePreview = _previewService.PreviewAgainstFrozenBaseline(
            new ScenarioRunPreviewRequest(
                horizonWeeks,
                ExternalScenario: request.ExternalScenario),
            baseline);
        noResponsePreview = noResponsePreview with
        {
            Request = noResponsePreview.Request with { Parameters = null }
        };
        var noResponse = BuildCase("NO_RESPONSE", "不采取措施", request.ExternalScenario.ScenarioId, noResponsePreview);

        var responseCases = responseOptions
            .Select(option =>
            {
                var preview = _previewService.PreviewAgainstFrozenBaseline(
                    new ScenarioRunPreviewRequest(
                        horizonWeeks,
                        Parameters: option.Parameters,
                        ExternalScenario: request.ExternalScenario),
                    baseline);
                return BuildCase(option.ResponseId, option.Name, request.ExternalScenario.ScenarioId, preview);
            })
            .ToList();

        return new ScenarioComparisonResult(
            baseline.SnapshotId,
            baseline.SnapshotNumber,
            request.ExternalScenario,
            noResponse,
            responseCases);
    }

    private static ScenarioComparisonCase BuildCase(
        string responseId,
        string name,
        string externalScenarioId,
        ScenarioRunPreviewResult preview)
    {
        return new ScenarioComparisonCase(
            responseId,
            name,
            externalScenarioId,
            preview,
            ProtectionBreachAnalyzer.Analyze(preview.Scenario));
    }
}

public static class ProtectionBreachAnalyzer
{
    public static IReadOnlyList<ProtectionBreachResult> Analyze(ScenarioRunPreviewCase scenario)
    {
        var results = new List<ProtectionBreachResult>();
        results.AddRange(scenario.BufferTrend.WeeklyCells
            .GroupBy(item => item.Sku, StringComparer.Ordinal)
            .Select(group => AnalyzeSeries(
                "Inventory",
                group.Key,
                group.Select(item => (item.Week, item.Status)),
                new[] { group.Key },
                "库存保护带被需求与补货时序击穿")));

        var allProducts = scenario.Plan.ReplenishmentOrders
            .Select(item => item.Sku)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        results.AddRange(scenario.Constraints.CapacityCells
            .GroupBy(item => item.ResourceCode, StringComparer.Ordinal)
            .Select(group => AnalyzeSeries(
                "Capacity",
                group.Key,
                group.Select(item => (item.Week, item.Status)),
                allProducts,
                "承诺负荷超过计划可用能力与能力保护")));

        results.AddRange(scenario.Constraints.SupplyCells
            .GroupBy(item => new { item.Supplier, item.MaterialFamily })
            .Select(group =>
            {
                var affected = scenario.SupplierCollaboration.SkuRequirements
                    .Where(item => item.Supplier == group.Key.Supplier && item.MaterialFamily == group.Key.MaterialFamily)
                    .Select(item => item.Sku)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(item => item, StringComparer.Ordinal)
                    .ToList();
                return AnalyzeSeries(
                    "Supply",
                    $"{group.Key.Supplier}/{group.Key.MaterialFamily}",
                    group.Select(item => (item.Week, item.Status)),
                    affected,
                    "不受限需求超过供应商承诺能力");
            }));

        EnsureScope(results, "Inventory", "无库存对象");
        EnsureScope(results, "Capacity", "无能力对象");
        EnsureScope(results, "Supply", "无供应对象");
        return results;
    }

    private static ProtectionBreachResult AnalyzeSeries(
        string scopeType,
        string target,
        IEnumerable<(int Week, string Status)> values,
        IReadOnlyList<string> affectedProducts,
        string primaryCause)
    {
        var series = values.OrderBy(item => item.Week).ToList();
        var firstRedIndex = series.FindIndex(item => item.Status == "Red");
        if (firstRedIndex < 0)
        {
            return new ProtectionBreachResult(
                scopeType, target, false, null, 0, null, false, affectedProducts, "展望期内未发生保护击穿");
        }

        var maximumDuration = 0;
        var currentDuration = 0;
        for (var index = 0; index < series.Count; index++)
        {
            if (series[index].Status == "Red")
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
            primaryCause);
    }

    private static void EnsureScope(List<ProtectionBreachResult> results, string scopeType, string target)
    {
        if (results.All(item => item.ScopeType != scopeType))
        {
            results.Add(new ProtectionBreachResult(
                scopeType, target, false, null, 0, null, false, Array.Empty<string>(), "没有可计算证据，未按零处理"));
        }
    }
}
