using AdaptiveSopDdsop.NetworkStructure;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<StandaloneNetworkStructureDataSource>();
builder.Services.AddSingleton<INetworkStructureProductDataSource>(sp => sp.GetRequiredService<StandaloneNetworkStructureDataSource>());
builder.Services.AddSingleton<INetworkGraphDataSource>(sp => sp.GetRequiredService<StandaloneNetworkStructureDataSource>());
builder.Services.AddSingleton<INetworkMetricsDataSource>(sp => sp.GetRequiredService<StandaloneNetworkStructureDataSource>());
builder.Services.AddSingleton<INetworkScoringDataSource>(sp => sp.GetRequiredService<StandaloneNetworkStructureDataSource>());
builder.Services.AddSingleton<NetworkGraphService>();
builder.Services.AddSingleton<NetworkMetricsService>();
builder.Services.AddSingleton<NetworkStructureScoringService>();

var app = builder.Build();

app.UseStaticFiles();
app.MapRazorPages();

app.MapGet("/", () => Results.Redirect("/network-structure"));

app.MapGet("/api/network-structure-capabilities", () =>
{
    return Results.Ok(NetworkStructureProductCapabilityCatalog.CreateStandaloneHost());
});

app.MapGet("/api/network-structure-data", (int? horizonWeeks, INetworkStructureProductDataSource dataSource) =>
{
    var horizon = Math.Clamp(horizonWeeks.GetValueOrDefault(12), 1, 52);
    return Results.Ok(dataSource.LoadNetworkStructure(new NetworkStructureProductDataRequest(
        horizon,
        StandaloneNetworkStructureDataSource.AnchorDate)));
});

app.MapGet("/api/network-structure-scoring", (int? horizonWeeks, NetworkStructureScoringService service) =>
{
    return Results.Ok(service.GetBaseline(horizonWeeks.GetValueOrDefault(12)));
});

app.MapGet("/api/network-metrics", (int? horizonWeeks, NetworkMetricsService service) =>
{
    return Results.Ok(service.GetBaseline(horizonWeeks.GetValueOrDefault(12)));
});

app.MapGet("/api/network-graph", (string? itemCode, int? maxDepth, NetworkGraphService service) =>
{
    return Results.Ok(service.GetGraph(itemCode, maxDepth.GetValueOrDefault(6)));
});

app.MapGet("/api/network-scenario-validation", (int? horizonWeeks) =>
{
    return Results.Ok(new
    {
        horizonWeeks = Math.Clamp(horizonWeeks.GetValueOrDefault(12), 1, 52),
        modelVersion = "NetworkStandalone-NoExternalPreview",
        validations = Array.Empty<object>(),
        selectedCandidateId = string.Empty,
        message = "独立网络结构评分 host 不执行外部白盒场景重算；如需验证库存、RCCP 和供应缺口变化，请连接外部场景运行集成层。"
    });
});

app.MapPost("/api/candidate-action-combinations/select", (CandidateActionCombinationRequest request) =>
{
    return Results.Ok(new
    {
        solverStatus = "Unavailable",
        solverName = string.IsNullOrWhiteSpace(request.SolverName) ? "Gurobi" : request.SolverName,
        message = "独立网络结构评分 host 不执行候选组合采纳；候选动作组合必须回到外部白盒引擎重算后才能比较。",
        candidateImpactMatrix = Array.Empty<object>(),
        combinationComparisons = Array.Empty<object>(),
        combinations = Array.Empty<object>(),
        trace = new[]
        {
            new
            {
                stage = "StandaloneBoundary",
                message = "网络结构评分 host 只负责网络图、指标和候选证据；不生成外部执行计划。",
                severity = "Information"
            }
        },
        isPersisted = false
    });
});

app.Run();
