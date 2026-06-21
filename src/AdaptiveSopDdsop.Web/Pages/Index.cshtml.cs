using System.Text.Json;
using AdaptiveSopDdsop.Web.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdaptiveSopDdsop.Web.Pages;

public class IndexModel : PageModel
{
    private readonly DdsopScenarioService _scenarioService;

    public IndexModel(DdsopScenarioService scenarioService)
    {
        _scenarioService = scenarioService;
    }

    public string InitialResultJson { get; private set; } = "{}";
    public string ValidationDataJson { get; private set; } = "{}";

    public void OnGet()
    {
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        InitialResultJson = JsonSerializer.Serialize(_scenarioService.Evaluate(new ScenarioInput()), jsonOptions);
        ValidationDataJson = JsonSerializer.Serialize(_scenarioService.GetValidationData(), jsonOptions);
    }
}
