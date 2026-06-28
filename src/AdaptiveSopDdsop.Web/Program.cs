using AdaptiveSopDdsop.Web.Data;
using AdaptiveSopDdsop.Web.Domain;
using AdaptiveSopDdsop.Web.NetworkStructureIntegration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton(SeedData.Create());
builder.Services.AddSingleton<DdmrpCalculator>();
builder.Services.AddSingleton<DdsopScenarioService>();
builder.Services.AddSingleton<IScenarioWorkspaceDataSource, SeedScenarioWorkspaceDataSource>();
builder.Services.AddSingleton<ScenarioRunPreviewService>();
builder.Services.AddSingleton<ProductFamilyDashboardService>();
builder.Services.AddSingleton<DdsopConfigInboundContractService>();
builder.Services.AddSingleton<DdsopFeedbackInboundLedger>();
builder.Services.AddSingleton<ProductionSupplierIdentitySourceInboundLedger>();
builder.Services.AddSingleton<ProductionInventoryQualityInboundLedger>();
builder.Services.AddSingleton<SdbrExecutionObjectEvidenceInboundLedger>();
builder.Services.AddNetworkStructureIntegration(builder.Configuration);
builder.Services.AddSingleton<RccpWorkspaceService>();
builder.Services.AddSingleton<ExceptionWorkspaceService>();
builder.Services.AddSingleton<BufferTrendWorkspaceService>();
builder.Services.AddSingleton<ConstraintWorkspaceService>();
builder.Services.AddSingleton<SupplierCollaborationWorkspaceService>();
builder.Services.AddSingleton(sp =>
{
    var environment = sp.GetRequiredService<IWebHostEnvironment>();
    var databasePath = Path.Combine(environment.ContentRootPath, "data", "ddae-scenario-runs.db");
    return new ScenarioRunPersistenceService(
        sp.GetRequiredService<ScenarioRunPreviewService>(),
        databasePath);
});
builder.Services.AddSingleton(sp =>
{
    var environment = sp.GetRequiredService<IWebHostEnvironment>();
    var databasePath = Path.Combine(environment.ContentRootPath, "data", "ddae-scenario-runs.db");
    return new MasterSettingsGovernanceService(
        sp.GetRequiredService<IScenarioWorkspaceDataSource>(),
        sp.GetRequiredService<ScenarioRunPreviewService>(),
        databasePath);
});

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

app.MapGet("/api/product-family-dashboard", (int? horizonWeeks, ProductFamilyDashboardService service) =>
{
    return Results.Ok(service.GetBaseline(horizonWeeks.GetValueOrDefault(12)));
});

app.MapGet("/api/integration-contracts/ddsop-config-inbound-v1", (
    int? horizonWeeks,
    string? approvedBy,
    string? sourceScenarioRunId,
    string? changeTicketId,
    DdsopConfigInboundContractService service) =>
{
    var message = service.Build(new DdsopConfigInboundContractRequest(
        horizonWeeks.GetValueOrDefault(12),
        new DateOnly(2026, 6, 1),
        approvedBy,
        sourceScenarioRunId,
        changeTicketId));
    return Results.Json(message, DdsopConfigInboundContractService.ContractJsonOptions);
});

app.MapPost("/api/integration-contracts/ddsop-feedback-outbound-v1", async (
    HttpRequest request,
    DdsopFeedbackInboundLedger ledger) =>
{
    using var reader = new StreamReader(request.Body);
    var rawPayload = await reader.ReadToEndAsync();
    var ack = ledger.Accept(rawPayload);
    return Results.Json(ack, DdsopConfigInboundContractService.ContractJsonOptions);
});

app.MapGet("/api/integration-contracts/ddsop-feedback-outbound-v1/ledger", (DdsopFeedbackInboundLedger ledger) =>
{
    return Results.Json(ledger.Records, DdsopConfigInboundContractService.ContractJsonOptions);
});

app.MapPost("/api/integration-contracts/production-supplier-identity-source-v1", async (
    HttpRequest request,
    ProductionSupplierIdentitySourceInboundLedger ledger) =>
{
    using var reader = new StreamReader(request.Body);
    var rawPayload = await reader.ReadToEndAsync();
    var ack = ledger.Accept(rawPayload);
    return Results.Json(ack, DdsopConfigInboundContractService.ContractJsonOptions);
});

app.MapGet("/api/integration-contracts/production-supplier-identity-source-v1/ledger", (ProductionSupplierIdentitySourceInboundLedger ledger) =>
{
    return Results.Json(ledger.Records, DdsopConfigInboundContractService.ContractJsonOptions);
});

app.MapPost("/api/integration-contracts/production-inventory-quality-evidence-v1", async (
    HttpRequest request,
    ProductionInventoryQualityInboundLedger ledger) =>
{
    using var reader = new StreamReader(request.Body);
    var rawPayload = await reader.ReadToEndAsync();
    var ack = ledger.Accept(rawPayload);
    return Results.Json(ack, DdsopConfigInboundContractService.ContractJsonOptions);
});

app.MapGet("/api/integration-contracts/production-inventory-quality-evidence-v1/ledger", (ProductionInventoryQualityInboundLedger ledger) =>
{
    return Results.Json(ledger.Records, DdsopConfigInboundContractService.ContractJsonOptions);
});

app.MapPost("/api/integration-contracts/sdbr-execution-object-evidence-v1", async (
    HttpRequest request,
    SdbrExecutionObjectEvidenceInboundLedger ledger) =>
{
    using var reader = new StreamReader(request.Body);
    var rawPayload = await reader.ReadToEndAsync();
    var ack = ledger.Accept(rawPayload);
    return Results.Json(ack, DdsopConfigInboundContractService.ContractJsonOptions);
});

app.MapGet("/api/integration-contracts/sdbr-execution-object-evidence-v1/ledger", (SdbrExecutionObjectEvidenceInboundLedger ledger) =>
{
    return Results.Json(ledger.Records, DdsopConfigInboundContractService.ContractJsonOptions);
});

app.MapNetworkStructureEndpoints();

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

app.MapPost("/api/scenario-runs", (ScenarioRunSaveRequest request, ScenarioRunPersistenceService service) =>
{
    try
    {
        return Results.Ok(service.Save(request));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapGet("/api/scenario-runs", (int? limit, ScenarioRunPersistenceService service) =>
{
    return Results.Ok(service.List(limit.GetValueOrDefault(50)));
});

app.MapGet("/api/scenario-runs/{runId}", (string runId, ScenarioRunPersistenceService service) =>
{
    var detail = service.GetDetail(runId);
    return detail is null ? Results.NotFound() : Results.Ok(detail);
});

app.MapGet("/api/scenario-runs/{runId}/audit", (string runId, ScenarioRunPersistenceService service) =>
{
    return Results.Ok(service.GetAuditEvents(runId));
});

app.MapGet("/api/master-settings-workspace", (int? limit, MasterSettingsGovernanceService service) =>
{
    return Results.Ok(service.GetWorkspace(limit.GetValueOrDefault(50)));
});

app.MapPost("/api/master-settings/proposals/from-preview", (ScenarioRunPreviewRequest request, MasterSettingsGovernanceService service) =>
{
    return Results.Ok(service.ProposeFromPreview(request));
});

app.MapPost("/api/master-settings/changes", (MasterSettingChangeSaveRequest request, MasterSettingsGovernanceService service) =>
{
    try
    {
        return Results.Ok(service.SaveChange(request));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapGet("/api/master-settings/changes", (int? limit, MasterSettingsGovernanceService service) =>
{
    return Results.Ok(service.ListChanges(limit.GetValueOrDefault(50)));
});

app.MapGet("/api/master-settings/changes/{changeId}", (string changeId, MasterSettingsGovernanceService service) =>
{
    var detail = service.GetDetail(changeId);
    return detail is null ? Results.NotFound() : Results.Ok(detail);
});

app.MapGet("/api/master-settings/changes/{changeId}/audit", (string changeId, MasterSettingsGovernanceService service) =>
{
    return Results.Ok(service.GetAuditEvents(changeId));
});

app.MapPost("/api/master-settings/changes/{changeId}/status", (string changeId, MasterSettingStatusUpdateRequest request, MasterSettingsGovernanceService service) =>
{
    try
    {
        return Results.Ok(service.UpdateStatus(changeId, request));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.Run();
