using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.NetworkStructureIntegration;

public sealed class HttpDdsopWhiteBoxScenarioGateway : IDdsopWhiteBoxScenarioGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient;
    private readonly DdsopWhiteBoxGatewayOptions _options;

    public HttpDdsopWhiteBoxScenarioGateway(HttpClient httpClient, DdsopWhiteBoxGatewayOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public ScenarioRunPreviewResult Recalculate(ScenarioRunPreviewRequest request)
    {
        var endpoint = string.IsNullOrWhiteSpace(_options.PreviewEndpoint)
            ? "/api/scenario-runs/preview"
            : _options.PreviewEndpoint;
        using var content = new StringContent(JsonSerializer.Serialize(request, JsonOptions), Encoding.UTF8, "application/json");
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = _httpClient.Send(httpRequest);
        response.EnsureSuccessStatusCode();
        using var stream = response.Content.ReadAsStream();
        return JsonSerializer.Deserialize<ScenarioRunPreviewResult>(stream, JsonOptions)
            ?? throw new InvalidOperationException("DDS&OP 白盒重算网关返回空结果。");
    }
}
