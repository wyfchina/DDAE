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

public sealed record ScenarioComparisonCase(
    string ResponseId,
    string Name,
    string ExternalScenarioId,
    ScenarioRunPreviewResult Preview,
    IReadOnlyList<ProtectionBreachResult> Breaches,
    IReadOnlyList<TimeBufferProjectionPoint>? TimeBufferProjection = null,
    IReadOnlyList<CapacityProtectionProjectionPoint>? CapacityProtectionProjection = null);

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
    private readonly IScenarioAssumptionSource _assumptionSource;

    public ScenarioComparisonService(
        CurrentBaselineService baselineService,
        ScenarioRunPreviewService previewService,
        IScenarioAssumptionSource assumptionSource)
    {
        _baselineService = baselineService;
        _previewService = previewService;
        _assumptionSource = assumptionSource;
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
        if (request.ExternalScenario.Metadata is null)
        {
            throw new ArgumentException("场景来源缺失；仅允许人工录入或演示模板", nameof(request));
        }
        _assumptionSource.Validate(request.ExternalScenario.Metadata);
        if (string.Equals(
                request.ExternalScenario.Metadata.SourceKind?.Trim(),
                "DemoFixture",
                StringComparison.OrdinalIgnoreCase))
        {
            var template = _assumptionSource.GetTemplate(request.ExternalScenario.Metadata.TemplateId ?? string.Empty)
                ?? throw new ArgumentException("演示模板不存在。", nameof(request));
            if (!HasCanonicalDemoPayload(request.ExternalScenario, template.ExternalScenario))
            {
                throw new ArgumentException("演示模板业务载荷与规范模板不一致。", nameof(request));
            }
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
        var frozenData = _previewService.LoadFrozenWorkspaceData(
            new ScenarioRunPreviewRequest(horizonWeeks),
            baseline);
        var noResponsePreview = _previewService.PreviewAgainstFrozenBaseline(
            new ScenarioRunPreviewRequest(
                horizonWeeks,
                ExternalScenario: request.ExternalScenario),
            baseline);
        noResponsePreview = noResponsePreview with
        {
            Request = noResponsePreview.Request with { Parameters = null }
        };
        var noResponse = BuildCase(
            "NO_RESPONSE",
            "不采取措施",
            request.ExternalScenario,
            null,
            noResponsePreview,
            frozenData,
            horizonWeeks);

        var responseCases = responseOptions
            .Select(option =>
            {
                var preview = _previewService.PreviewAgainstFrozenBaseline(
                    new ScenarioRunPreviewRequest(
                        horizonWeeks,
                        Parameters: option.Parameters,
                        ExternalScenario: request.ExternalScenario),
                    baseline);
                return BuildCase(
                    option.ResponseId,
                    option.Name,
                    request.ExternalScenario,
                    option.Parameters,
                    preview,
                    frozenData,
                    horizonWeeks);
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
        ExternalScenarioDefinition externalScenario,
        ScenarioRunParameterSet? response,
        ScenarioRunPreviewResult preview,
        ScenarioWorkspaceDataSet frozenData,
        int horizonWeeks)
    {
        var analysis = ProtectionBreachAnalyzer.Analyze(
            frozenData,
            externalScenario,
            response,
            preview.Scenario,
            horizonWeeks);
        return new ScenarioComparisonCase(
            responseId,
            name,
            externalScenario.ScenarioId,
            preview,
            analysis.Breaches,
            analysis.TimeBufferProjection,
            analysis.CapacityProtectionProjection);
    }

    private static bool HasCanonicalDemoPayload(
        ExternalScenarioDefinition candidate,
        ExternalScenarioDefinition canonical)
    {
        return string.Equals(candidate.ScenarioId, canonical.ScenarioId, StringComparison.Ordinal)
            && string.Equals(candidate.Name, canonical.Name, StringComparison.Ordinal)
            && SequenceEqual(candidate.DemandChanges, canonical.DemandChanges)
            && SequenceEqual(candidate.SupplyRisks, canonical.SupplyRisks)
            && SequenceEqual(candidate.CapacityLosses, canonical.CapacityLosses)
            && SequenceEqual(candidate.KnownEvents, canonical.KnownEvents)
            && SequenceEqual(candidate.TimeDelays, canonical.TimeDelays);
    }

    private static bool SequenceEqual<T>(IReadOnlyList<T>? candidate, IReadOnlyList<T>? canonical)
    {
        if (candidate is null || canonical is null)
        {
            return candidate is null && canonical is null;
        }

        return candidate.SequenceEqual(canonical);
    }
}
