# Adaptive S&OP + DDS&OP Application Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a practical ASP.NET Core C# web application that demonstrates Adaptive S&OP and DDS&OP as a two-level management loop with auditable validation data and real DDMRP-style calculations.

**Architecture:** A .NET 9 solution with a Razor Pages web app, a focused domain calculation layer, seeded JSON validation data, API endpoints for scenario calculations, and xUnit tests for the core algorithms. The UI is a management cockpit: Adaptive S&OP seven-step navigation at the top level, DDS&OP scenario lab in the middle, and DDOM/DDMRP parameter projection underneath.

**Tech Stack:** .NET 9, ASP.NET Core Razor Pages, C#, xUnit, JSON seed data, vanilla JavaScript, CSS.

---

## File Structure

- `AdaptiveSopDdsop.sln`: solution file.
- `src/AdaptiveSopDdsop.Web/AdaptiveSopDdsop.Web.csproj`: web app project.
- `src/AdaptiveSopDdsop.Web/Program.cs`: service registration, routing, static file setup.
- `src/AdaptiveSopDdsop.Web/Domain/Models.cs`: scenario, SKU, buffer, resource, finance, result records.
- `src/AdaptiveSopDdsop.Web/Domain/DdmrpCalculator.cs`: buffer zones, net flow, order recommendations, working capital.
- `src/AdaptiveSopDdsop.Web/Domain/DdsopScenarioService.cs`: scenario application and management recommendations.
- `src/AdaptiveSopDdsop.Web/Data/SeedData.cs`: embedded practical manufacturing dataset.
- `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`: cockpit UI.
- `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml.cs`: page model with initial data serialization.
- `src/AdaptiveSopDdsop.Web/Pages/Shared/_Layout.cshtml`: base layout.
- `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`: responsive dashboard styling.
- `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`: scenario controls and API refresh.
- `tests/AdaptiveSopDdsop.Tests/AdaptiveSopDdsop.Tests.csproj`: test project.
- `tests/AdaptiveSopDdsop.Tests/DdmrpCalculatorTests.cs`: DDMRP calculation tests.
- `tests/AdaptiveSopDdsop.Tests/DdsopScenarioServiceTests.cs`: scenario/recommendation tests.

## Tasks

### Task 1: Create Solution Skeleton

**Files:**
- Create: `AdaptiveSopDdsop.sln`
- Create: `src/AdaptiveSopDdsop.Web/AdaptiveSopDdsop.Web.csproj`
- Create: `tests/AdaptiveSopDdsop.Tests/AdaptiveSopDdsop.Tests.csproj`

- [ ] Run `dotnet new sln -n AdaptiveSopDdsop`.
- [ ] Run `dotnet new webapp -n AdaptiveSopDdsop.Web -o src/AdaptiveSopDdsop.Web --framework net9.0`.
- [ ] Run `dotnet new xunit -n AdaptiveSopDdsop.Tests -o tests/AdaptiveSopDdsop.Tests --framework net9.0`.
- [ ] Add both projects to the solution and reference the web project from the test project.
- [ ] Run `dotnet test AdaptiveSopDdsop.sln`; expected: default template tests pass.

### Task 2: TDD DDMRP Core Calculations

**Files:**
- Create: `tests/AdaptiveSopDdsop.Tests/DdmrpCalculatorTests.cs`
- Create: `src/AdaptiveSopDdsop.Web/Domain/Models.cs`
- Create: `src/AdaptiveSopDdsop.Web/Domain/DdmrpCalculator.cs`

- [ ] Write failing tests for buffer zone sizing: red = ADU * DLT * variability factor, yellow = ADU * DLT, green = max(MOQ, ADU * order cycle).
- [ ] Write failing tests for net flow position: on-hand + open supply - qualified demand.
- [ ] Write failing tests for planned order recommendation when net flow is below top of yellow.
- [ ] Implement minimal records and calculator methods to pass.
- [ ] Run `dotnet test AdaptiveSopDdsop.sln`; expected: all DDMRP tests pass.

### Task 3: TDD DDS&OP Scenario Service

**Files:**
- Create: `tests/AdaptiveSopDdsop.Tests/DdsopScenarioServiceTests.cs`
- Create: `src/AdaptiveSopDdsop.Web/Domain/DdsopScenarioService.cs`
- Create: `src/AdaptiveSopDdsop.Web/Data/SeedData.cs`

- [ ] Write failing test that a promotion scenario increases ADU and working capital.
- [ ] Write failing test that a supply disruption lowers buffer health and creates an expedite recommendation.
- [ ] Write failing test that a planned shutdown creates a capacity warning and management review action.
- [ ] Implement seed data and scenario service to pass.
- [ ] Run `dotnet test AdaptiveSopDdsop.sln`; expected: all scenario tests pass.

### Task 4: Web API and Page Model

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Program.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml.cs`

- [ ] Add singleton services for seed data, calculator, and scenario service.
- [ ] Add `GET /api/validation-data` to expose families, SKUs, resources, and baseline weekly demand.
- [ ] Add `POST /api/scenario` to calculate scenario results from JSON input.
- [ ] Bind initial baseline result in `IndexModel`.
- [ ] Run `dotnet test AdaptiveSopDdsop.sln`; expected: tests still pass.

### Task 5: Practical Management UI

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Shared/_Layout.cshtml`
- Create: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`
- Create: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`

- [ ] Build a cockpit with Adaptive S&OP seven steps, KPI strip, buffer health table, scenario lab, DDOM settings, and management review list.
- [ ] Make scenario controls for promotion percent, supply disruption weeks, planned shutdown days, and new product demand.
- [ ] Refresh results through `/api/scenario` without page reload.
- [ ] Add visible validation data tables so numbers can be audited.
- [ ] Run `dotnet run --project src/AdaptiveSopDdsop.Web --urls http://127.0.0.1:5188`.

### Task 6: Verification

**Files:**
- No production edits unless verification reveals a defect.

- [ ] Run `dotnet test AdaptiveSopDdsop.sln`.
- [ ] Request `http://127.0.0.1:5188/api/validation-data`; expected: JSON contains 3 product families and 8 SKUs.
- [ ] Request scenario API with a promotion and supply disruption; expected: buffer/working-capital/recommendation values change.
- [ ] Open `http://127.0.0.1:5188` in browser and verify no console errors, UI is readable, and scenario controls update results.

