using AdaptiveSopDdsop.NetworkStructure;

using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.NetworkStructureIntegration;

public sealed class NetworkScenarioValidationService
{
    private readonly IScenarioWorkspaceDataSource _dataSource;
    private readonly INetworkStructureDataSource _networkDataSource;
    private readonly NetworkStructureScoringService _scoringService;
    private readonly NetworkCandidateRecalculationRequestBuilder _requestBuilder;
    private readonly IDdsopWhiteBoxScenarioGateway _whiteBoxGateway;

    public NetworkScenarioValidationService(
        IScenarioWorkspaceDataSource dataSource,
        INetworkStructureDataSource networkDataSource,
        NetworkStructureScoringService scoringService,
        NetworkCandidateRecalculationRequestBuilder requestBuilder,
        IDdsopWhiteBoxScenarioGateway whiteBoxGateway)
    {
        _dataSource = dataSource;
        _networkDataSource = networkDataSource;
        _scoringService = scoringService;
        _requestBuilder = requestBuilder;
        _whiteBoxGateway = whiteBoxGateway;
    }

    public NetworkScenarioValidationResult Validate(int horizonWeeks)
    {
        var horizon = Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52);
        var anchorDate = new DateOnly(2026, 6, 1);
        var data = _dataSource.Load(new ScenarioWorkspaceDataRequest(horizon, anchorDate));
        var networkData = _networkDataSource.LoadNetworkStructure(new NetworkStructureDataRequest(horizon, anchorDate)).NetworkData;
        var scoring = _scoringService.GetBaseline(horizon);
        var validations = scoring.Candidates
            .Where(candidate => candidate.Severity is "Red" or "Yellow" || candidate.CandidateId.StartsWith("NET-", StringComparison.Ordinal))
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Target, StringComparer.Ordinal)
            .Take(10)
            .Select(candidate => ValidateCandidate(data, networkData, candidate, horizon))
            .ToList();

        return new NetworkScenarioValidationResult(
            horizon,
            "NetworkScenarioValidation-V1",
            validations,
            validations.FirstOrDefault()?.CandidateId ?? string.Empty);
    }

    private NetworkScenarioValidationItem ValidateCandidate(
        ScenarioWorkspaceDataSet data,
        NetworkDataSet networkData,
        NetworkStructureCandidate candidate,
        int horizon)
    {
        var request = _requestBuilder.Build(data, networkData, candidate, horizon);
        var preview = _whiteBoxGateway.Recalculate(request);
        var bufferComparison = preview.Scenario.BufferTrend.Comparison;
        var summary = BuildSummary(candidate, preview);

        return new NetworkScenarioValidationItem(
            candidate.CandidateId,
            candidate.Target,
            candidate.TargetName,
            candidate.RecommendedSettingType,
            candidate.Score,
            candidate.Severity,
            preview.Comparison.AverageInventoryValueDelta,
            bufferComparison.RedWeekDelta,
            preview.Comparison.ReplenishmentOrderCountDelta,
            bufferComparison.ReplenishmentQuantityDelta,
            preview.RccpComparison.PeakLoadDelta,
            preview.RccpComparison.AverageLoadDelta,
            preview.RccpComparison.RedWeekDelta,
            preview.Comparison.SupplyGapDelta,
            summary,
            new[]
            {
                $"场景请求：{DescribeRequest(request)}",
                $"库存金额变化：{preview.Comparison.AverageInventoryValueDelta:0}",
                $"红区周变化：{bufferComparison.RedWeekDelta}",
            $"补货订单变化：{preview.Comparison.ReplenishmentOrderCountDelta}",
            $"RCCP 峰值变化：{preview.RccpComparison.PeakLoadDelta:0.#}pp",
            $"供应缺口变化：{preview.Comparison.SupplyGapDelta:0}",
            },
            request);
    }

    private static string DescribeRequest(ScenarioRunPreviewRequest request)
    {
        var parameters = request.Parameters;
        return $"SKU {string.Join(",", request.SkuFilter ?? Array.Empty<string>())}；提前建库 {parameters?.PrebuildCampaigns?.Count ?? 0}；产能调整 {parameters?.CapacityAdjustments?.Count ?? 0}；策略覆盖 {parameters?.SkuPolicyOverrides?.Count ?? 0}；供应承诺 {parameters?.SupplierCapacityLimits?.Count ?? 0}";
    }

    private static string BuildSummary(
        NetworkStructureCandidate candidate,
        ScenarioRunPreviewResult preview)
    {
        var inventory = preview.Comparison.AverageInventoryValueDelta;
        var redWeeks = preview.Scenario.BufferTrend.Comparison.RedWeekDelta;
        var rccp = preview.RccpComparison.PeakLoadDelta;
        var supply = preview.Comparison.SupplyGapDelta;
        return $"{candidate.TargetName} 验证结果：库存金额 {inventory:0}，红区周 {redWeeks:+#;-#;0}，RCCP 峰值 {rccp:+0.#;-0.#;0}pp，供应缺口 {supply:+#;-#;0}。";
    }
}
