using AdaptiveSopDdsop.Web.Data;
using AdaptiveSopDdsop.Web.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton(SeedData.Create());
builder.Services.AddSingleton<DdmrpCalculator>();
builder.Services.AddSingleton<DdsopScenarioService>();
builder.Services.AddSingleton<IScenarioWorkspaceDataSource, SeedScenarioWorkspaceDataSource>();
builder.Services.AddSingleton<ScenarioRunPreviewService>();
builder.Services.AddSingleton<RccpWorkspaceService>();
builder.Services.AddSingleton<ExceptionWorkspaceService>();
builder.Services.AddSingleton<BufferTrendWorkspaceService>();
builder.Services.AddSingleton<ConstraintWorkspaceService>();
builder.Services.AddSingleton<SupplierCollaborationWorkspaceService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapGet("/api/validation-data", (DdsopScenarioService service) =>
{
    return Results.Ok(service.GetValidationData());
});

app.MapPost("/api/scenario", (ScenarioInput input, DdsopScenarioService service) =>
{
    return Results.Ok(service.Evaluate(input));
});

app.MapGet("/api/demand-driven-plan", (int? horizonWeeks, DdsopScenarioService service) =>
{
    return Results.Ok(service.EvaluateDemandDrivenPlan(horizonWeeks.GetValueOrDefault(12)));
});

app.MapGet("/api/scenario-workspace-data", (int? horizonWeeks, IScenarioWorkspaceDataSource dataSource) =>
{
    var request = new ScenarioWorkspaceDataRequest(
        horizonWeeks.GetValueOrDefault(12),
        new DateOnly(2026, 6, 1));
    return Results.Ok(dataSource.Load(request));
});

app.MapGet("/api/rccp-workspace", (int? horizonWeeks, RccpWorkspaceService service) =>
{
    return Results.Ok(service.GetBaseline(horizonWeeks.GetValueOrDefault(12)));
});

app.MapGet("/api/exception-workspace", (int? horizonWeeks, ExceptionWorkspaceService service) =>
{
    return Results.Ok(service.GetExceptions(horizonWeeks.GetValueOrDefault(12)));
});

app.MapGet("/api/buffer-trend-workspace", (int? horizonWeeks, BufferTrendWorkspaceService service) =>
{
    return Results.Ok(service.GetBaseline(horizonWeeks.GetValueOrDefault(12)));
});

app.MapGet("/api/constraint-workspace", (int? horizonWeeks, ConstraintWorkspaceService service) =>
{
    return Results.Ok(service.GetBaseline(horizonWeeks.GetValueOrDefault(12)));
});

app.MapGet("/api/supplier-collaboration-workspace", (int? horizonWeeks, SupplierCollaborationWorkspaceService service) =>
{
    return Results.Ok(service.GetBaseline(horizonWeeks.GetValueOrDefault(12)));
});

app.MapPost("/api/scenario-runs/preview", (ScenarioRunPreviewRequest request, ScenarioRunPreviewService service) =>
{
    return Results.Ok(service.Preview(request));
});

app.Run();
