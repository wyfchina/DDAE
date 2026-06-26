using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.NetworkStructureIntegration;

public sealed class LocalDdsopWhiteBoxScenarioGateway : IDdsopWhiteBoxScenarioGateway
{
    private readonly ScenarioRunPreviewService _previewService;

    public LocalDdsopWhiteBoxScenarioGateway(ScenarioRunPreviewService previewService)
    {
        _previewService = previewService;
    }

    public ScenarioRunPreviewResult Recalculate(ScenarioRunPreviewRequest request)
    {
        return _previewService.Preview(request);
    }
}
