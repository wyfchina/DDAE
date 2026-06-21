using AdaptiveSopDdsop.Web.Data;
using AdaptiveSopDdsop.Web.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton(SeedData.Create());
builder.Services.AddSingleton<DdmrpCalculator>();
builder.Services.AddSingleton<DdsopScenarioService>();

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

app.Run();
