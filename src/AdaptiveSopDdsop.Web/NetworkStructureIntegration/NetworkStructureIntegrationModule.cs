using AdaptiveSopDdsop.NetworkStructure;

using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.NetworkStructureIntegration;

public static class NetworkStructureIntegrationModule
{
    public static IServiceCollection AddNetworkStructureIntegration(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        var whiteBoxGatewaySection = configuration?.GetSection("NetworkStructure:WhiteBoxGateway");
        var whiteBoxGatewayOptions = new DdsopWhiteBoxGatewayOptions
        {
            Mode = whiteBoxGatewaySection?["Mode"] ?? "Local",
            BaseUrl = whiteBoxGatewaySection?["BaseUrl"],
            PreviewEndpoint = whiteBoxGatewaySection?["PreviewEndpoint"] ?? "/api/scenario-runs/preview"
        };

        services.AddSingleton<INetworkStructureDataSource, NetworkStructureDataSourceAdapter>();
        services.AddSingleton<IOptimizationSolver, GurobiOptimizationSolver>();
        services.AddSingleton<IOptimizationSolver, OrToolsOptimizationSolver>();
        services.AddSingleton<CandidateActionCombinationSelector>();
        services.AddSingleton<INetworkMetricsDataSource, DdsopNetworkMetricsDataSource>();
        services.AddSingleton<NetworkMetricsService>();
        services.AddSingleton<INetworkScoringDataSource, DdsopNetworkScoringDataSource>();
        services.AddSingleton<NetworkStructureScoringService>();
        services.AddSingleton<INetworkGraphDataSource, DdsopNetworkGraphDataSource>();
        services.AddSingleton<NetworkGraphService>();
        services.AddSingleton(whiteBoxGatewayOptions);
        if (string.Equals(whiteBoxGatewayOptions.Mode, "Http", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IDdsopWhiteBoxScenarioGateway>(sp =>
            {
                if (string.IsNullOrWhiteSpace(whiteBoxGatewayOptions.BaseUrl))
                {
                    throw new InvalidOperationException("NetworkStructure:WhiteBoxGateway:BaseUrl is required when Mode is Http.");
                }

                return new HttpDdsopWhiteBoxScenarioGateway(
                    new HttpClient
                    {
                        BaseAddress = new Uri(whiteBoxGatewayOptions.BaseUrl, UriKind.Absolute)
                    },
                    whiteBoxGatewayOptions);
            });
        }
        else
        {
            services.AddSingleton<IDdsopWhiteBoxScenarioGateway, LocalDdsopWhiteBoxScenarioGateway>();
        }
        services.AddSingleton<NetworkCandidateRecalculationRequestBuilder>();
        services.AddSingleton<NetworkScenarioValidationService>();
        services.AddSingleton<CandidateActionCombinationService>();

        return services;
    }

    public static IEndpointRouteBuilder MapNetworkStructureEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/network-structure-capabilities", () =>
        {
            return Results.Ok(NetworkStructureProductCapabilityCatalog.CreateStandaloneHost());
        });

        app.MapGet("/api/network-structure-scoring", (int? horizonWeeks, NetworkStructureScoringService service) =>
        {
            return Results.Ok(service.GetBaseline(horizonWeeks.GetValueOrDefault(12)));
        });

        app.MapGet("/api/network-structure-data", (int? horizonWeeks, INetworkStructureDataSource dataSource) =>
        {
            var request = new NetworkStructureDataRequest(
                horizonWeeks.GetValueOrDefault(12),
                new DateOnly(2026, 6, 1));
            return Results.Ok(dataSource.LoadNetworkStructure(request));
        });

        app.MapGet("/api/network-metrics", (int? horizonWeeks, NetworkMetricsService service) =>
        {
            return Results.Ok(service.GetBaseline(horizonWeeks.GetValueOrDefault(12)));
        });

        app.MapGet("/api/network-graph", (string? itemCode, int? maxDepth, NetworkGraphService service) =>
        {
            return Results.Ok(service.GetGraph(itemCode, maxDepth.GetValueOrDefault(6)));
        });

        app.MapGet("/api/network-scenario-validation", (int? horizonWeeks, NetworkScenarioValidationService service) =>
        {
            return Results.Ok(service.Validate(horizonWeeks.GetValueOrDefault(12)));
        });

        app.MapPost("/api/candidate-action-combinations/select", (CandidateActionCombinationRequest request, CandidateActionCombinationService service) =>
        {
            return Results.Ok(service.Select(request));
        });

        return app;
    }
}
