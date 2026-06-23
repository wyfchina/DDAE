using AdaptiveSopDdsop.Web.Data;
using AdaptiveSopDdsop.Web.Domain;

var tests = new (string Name, Action Run)[]
{
    ("DDMRP buffer zones follow ADU, DLT, variability, and MOQ rules", TestBufferZones),
    ("Net flow position adds on hand and open supply then subtracts qualified demand", TestNetFlow),
    ("Planning recommendation replenishes to top of green at review time when net flow is at or below top of yellow", TestPlanningRecommendation),
    ("Promotion scenario increases ADU and working capital", TestPromotionScenario),
    ("Supply disruption lowers buffer health and creates expedite recommendation", TestSupplyDisruptionScenario),
    ("Planned shutdown creates capacity warning and management review action", TestShutdownScenario),
    ("Baseline data demonstrates red yellow green and over top of green buffer statuses with Chinese names", TestBaselineStatusVarietyAndChineseNames),
    ("Consolidated requirements are represented in validation data", TestConsolidatedRequirementsDataCoverage),
    ("Scenario Run Workspace replaces teaching page shell", TestScenarioRunWorkspaceReplacesTeachingPageShell),
    ("Scenario exceeding AS&OP guardrails is blocked from adoption", TestAsopGuardrailBlocksExcessiveScenario),
    ("Moderate scenario is routed to integrated reconciliation", TestAsopGuardrailRoutesModerateScenario),
    ("Time phased buffer projection creates replenishment order and Chinese trace at an order review point", TestTimePhasedBufferProjectionCreatesReplenishmentTrace),
    ("Time phased buffer projection waits for order cycle review before replenishment", TestTimePhasedBufferProjectionWaitsForOrderCycleReview),
    ("Demand driven RCCP uses projected replenishment orders instead of forecast demand", TestDemandDrivenRccpUsesProjectedReplenishmentOrders),
    ("Scenario service exposes white box demand driven plan run", TestScenarioServiceExposesWhiteBoxDemandDrivenPlanRun),
    ("Scenario Run Workspace exposes required panels", TestScenarioRunWorkspaceExposesRequiredPanels),
    ("Pre-build campaign moves replenishment before a future peak", TestPrebuildCampaignMovesReplenishmentBeforeFuturePeak),
    ("Resource calendar adjustment changes RCCP available capacity", TestResourceCalendarAdjustmentChangesRccpCapacity),
    ("Projected supply requirements aggregate replenishment by supplier", TestProjectedSupplyRequirementsAggregateBySupplier),
    ("Supplier collaboration workspace summarizes supplier demand drilldown", TestSupplierCollaborationWorkspaceSummarizesSupplierDrilldown),
    ("Supplier collaboration explains yellow and red status reasons", TestSupplierCollaborationExplainsStatusReasons),
    ("Scenario Run Workspace script fetches workspace data", TestScenarioRunWorkspaceScriptFetchesWorkspaceData),
    ("Scenario Run Workspace script delegates business calculations to services", TestScenarioRunWorkspaceScriptDelegatesBusinessCalculationsToServices),
    ("Scenario workspace seed data covers baseline scenario use cases", TestScenarioWorkspaceSeedDataCoversUseCases),
    ("Scenario workspace adapter can map alternate source structures", TestScenarioWorkspaceAdapterCanMapAlternateSourceStructures),
    ("Scenario preview returns baseline and scenario results from data source", TestScenarioPreviewReturnsComparableResults),
    ("Scenario preview applies pre-build capacity policy and supplier limits", TestScenarioPreviewAppliesScenarioParameters),
    ("Product RCCP workspace summarizes resources heatmap and detail", TestProductRccpWorkspaceSummarizesResourcesHeatmapAndDetail),
    ("Scenario preview returns product RCCP comparison", TestScenarioPreviewReturnsProductRccpComparison),
    ("Constraint workspace summarizes constrained and unconstrained capacity and supply", TestConstraintWorkspaceSummarizesCapacityAndSupply),
    ("Scenario preview returns constrained and unconstrained comparison", TestScenarioPreviewReturnsConstraintComparison),
    ("Scenario preview returns supplier collaboration drilldown", TestScenarioPreviewReturnsSupplierCollaborationDrilldown),
    ("Buffer trend workspace summarizes KPIs heatmap and SKU detail", TestBufferTrendWorkspaceSummarizesKpisHeatmapAndDetail),
    ("Scenario preview returns graphical buffer trend comparison", TestScenarioPreviewReturnsBufferTrendComparison),
    ("Exception workspace detects variance signals and scenario presets", TestExceptionWorkspaceDetectsVarianceSignalsAndScenarioPresets),
};

var failed = 0;
foreach (var test in tests)
{
    try
    {
        test.Run();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.WriteLine($"FAIL {test.Name}: {ex.Message}");
    }
}

if (failed > 0)
{
    Console.WriteLine($"{failed} test(s) failed.");
    Environment.Exit(1);
}

Console.WriteLine($"{tests.Length} test(s) passed.");

static void TestBufferZones()
{
    var sku = new SkuBufferSetting("SKU-AXLE-STD", "Axle Standard", "Mobility", 100, 5, 1.5m, 3, 700, 12.5m, 1200);
    var zones = DdmrpCalculator.CalculateZones(sku);

    AssertEqual(750, zones.Red, "red zone");
    AssertEqual(500, zones.Yellow, "yellow zone");
    AssertEqual(700, zones.Green, "green zone");
    AssertEqual(1950, zones.TopOfGreen, "top of green");
}

static void TestNetFlow()
{
    var position = new InventoryPosition("SKU-AXLE-STD", 420, 300, 260);
    var netFlow = DdmrpCalculator.CalculateNetFlow(position);

    AssertEqual(460, netFlow, "net flow");
}

static void TestPlanningRecommendation()
{
    var sku = new SkuBufferSetting("SKU-AXLE-STD", "Axle Standard", "Mobility", 100, 5, 1.5m, 3, 700, 12.5m, 1200);
    var position = new InventoryPosition("SKU-AXLE-STD", 420, 300, 260);
    var recommendation = DdmrpCalculator.CalculateRecommendation(sku, position);

    AssertEqual("Order", recommendation.Action, "action");
    AssertEqual(1490, recommendation.OrderQuantity, "order quantity");
    AssertEqual("Red", recommendation.BufferStatus, "buffer status");
}

static void TestPromotionScenario()
{
    var service = new DdsopScenarioService(SeedData.Create(), new DdmrpCalculator());
    var baseline = service.Evaluate(new ScenarioInput());
    var promotion = service.Evaluate(new ScenarioInput(PromotionPercent: 20));

    AssertTrue(promotion.TotalWorkingCapital > baseline.TotalWorkingCapital, "promotion should increase working capital");
    AssertTrue(promotion.TotalAdu > baseline.TotalAdu, "promotion should increase ADU");
}

static void TestSupplyDisruptionScenario()
{
    var service = new DdsopScenarioService(SeedData.Create(), new DdmrpCalculator());
    var result = service.Evaluate(new ScenarioInput(SupplyDisruptionWeeks: 3));

    AssertTrue(result.BufferHealthPercent < 80, "supply disruption should lower buffer health");
    AssertContains(result.ManagementActions, "催交", "expedite action");
}

static void TestShutdownScenario()
{
    var service = new DdsopScenarioService(SeedData.Create(), new DdmrpCalculator());
    var result = service.Evaluate(new ScenarioInput(PlannedShutdownDays: 5));

    AssertTrue(result.CapacityUtilizationPercent > 100, "shutdown should overload remaining capacity");
    AssertContains(result.ManagementActions, "管理评审", "management review escalation");
}

static void TestBaselineStatusVarietyAndChineseNames()
{
    var service = new DdsopScenarioService(SeedData.Create(), new DdmrpCalculator());
    var result = service.Evaluate(new ScenarioInput());
    var statuses = result.Skus.Select(sku => sku.BufferStatus).ToHashSet(StringComparer.OrdinalIgnoreCase);

    AssertTrue(statuses.Contains("Red"), "baseline should contain red buffer status");
    AssertTrue(statuses.Contains("Yellow"), "baseline should contain yellow buffer status");
    AssertTrue(statuses.Contains("Green"), "baseline should contain green buffer status");
    AssertTrue(statuses.Contains("OverTopOfGreen"), "baseline should contain over top of green buffer status");
    AssertTrue(result.Skus.Any(sku => ContainsChinese(sku.Name) || ContainsChinese(sku.Family)), "baseline should contain Chinese family or SKU names");
}

static void TestConsolidatedRequirementsDataCoverage()
{
    var data = SeedData.Create();

    AssertTrue(data.StrategicMonths.Count >= 24, "strategic horizon should cover at least 24 months");
    AssertTrue(data.AsopSteps.Count == 7, "AS&OP should expose seven steps");
    AssertTrue(data.DdsopElements.Count == 6, "DDS&OP should expose six elements");
    AssertTrue(data.PortfolioItems.Count >= 6, "portfolio should include NPI and lifecycle items");
    AssertTrue(data.FinancialProjections.Count >= data.Families.Count * 24, "financial plan should project families across horizon");
    AssertTrue(data.ResourceProfiles.Count >= 6, "resource profile/RRP should include multiple resources");
    AssertTrue(data.SupplierConstraints.Count >= 3, "supplier constraints should be represented");
    AssertTrue(data.CapitalRequirements.Count >= 3, "capital requirements should be represented");
    AssertTrue(data.MasterSettings.Count >= data.Skus.Count, "master settings should cover decoupled SKU/material positions");
    AssertTrue(data.KnownEvents.Count >= 4, "known events should drive DAF/zone adjustments");
    AssertTrue(data.DdomFeedback.Count >= 12, "DDOM feedback should include historical health observations");
    AssertTrue(data.TacticalOpportunities.Count >= 3, "tactical exploitation opportunities should be represented");
    AssertTrue(data.StrategicRecommendations.Count >= 3, "strategic recommendations should be represented");
    AssertTrue(data.FeasibilityChecks.Count >= 3, "strategic projection feasibility checks should be represented");
    AssertTrue(data.SkillBuffers.Count >= 3, "DDSM skill buffers should be represented");
}

static void TestScenarioRunWorkspaceReplacesTeachingPageShell()
{
    var pagePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml");
    var page = File.ReadAllText(Path.GetFullPath(pagePath));

    AssertTrue(page.Contains("id=\"scenario-workspace-app\"", StringComparison.Ordinal), "homepage should expose Scenario Run Workspace shell");
    AssertTrue(page.Contains("需求驱动 S&OP 场景运行工作台", StringComparison.Ordinal), "homepage should be Chinese Scenario Run Workspace");
    AssertTrue(!page.Contains("class=\"hero\"", StringComparison.Ordinal), "homepage should no longer render teaching hero");
    AssertTrue(!page.Contains("Pre-build", StringComparison.Ordinal), "homepage should not expose English pre-build labels");
    AssertTrue(!page.Contains("Budget / Last Year", StringComparison.Ordinal), "homepage should use Chinese budget labels");
    AssertTrue(!page.Contains("Demand Driven RCCP", StringComparison.Ordinal), "homepage should use Chinese RCCP labels");
    AssertTrue(!page.Contains("Projected Supply", StringComparison.Ordinal), "homepage should use Chinese supply labels");
    AssertTrue(!page.Contains("Variance Analysis", StringComparison.Ordinal), "homepage should use Chinese exception labels");
    AssertTrue(!page.Contains("Calculation Trace", StringComparison.Ordinal), "homepage should use Chinese trace labels");
    AssertTrue(page.Contains("id=\"order-cycle-override\" type=\"number\" min=\"1\"", StringComparison.Ordinal), "order cycle override should not allow zero");
    AssertTrue(page.Contains("id=\"supplier-limit-start-week\"", StringComparison.Ordinal), "supplier limit should expose a start week");
    AssertTrue(page.Contains("id=\"supplier-limit-end-week\"", StringComparison.Ordinal), "supplier limit should expose an end week");
    AssertTrue(page.Contains("id=\"adoption-constraint-select\"", StringComparison.Ordinal), "scenario run should expose customizable adoption constraints");
    AssertTrue(page.Contains("流速优先", StringComparison.Ordinal), "adoption constraints should include a flow-first mode");

    var scriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js");
    var script = File.ReadAllText(Path.GetFullPath(scriptPath));
    AssertTrue(script.Contains("syncSkuPolicyDefaults", StringComparison.Ordinal), "script should sync SKU order cycle defaults");
    AssertTrue(script.Contains("syncSupplierLimitDefaults", StringComparison.Ordinal), "script should sync supplier limit defaults");
    AssertTrue(script.Contains("startWeek: supplierStartWeek", StringComparison.Ordinal), "supplier limit payload should use selected start week");
    AssertTrue(script.Contains("endWeek: supplierEndWeek", StringComparison.Ordinal), "supplier limit payload should use selected end week");
    AssertTrue(script.Contains("adoptionConstraintMode", StringComparison.Ordinal), "preview payload should include adoption constraint mode");
    AssertTrue(script.Contains("targetFlowIndex", StringComparison.Ordinal), "script should expose target flow in the workspace");
    AssertTrue(script.Contains("evaluateAdoption", StringComparison.Ordinal), "script should evaluate preview against the selected adoption constraint");
}

static void TestAsopGuardrailBlocksExcessiveScenario()
{
    var service = new DdsopScenarioService(SeedData.Create(), new DdmrpCalculator());
    var result = service.Evaluate(new ScenarioInput(PromotionPercent: 40, SupplyDisruptionWeeks: 6, PlannedShutdownDays: 8, NewProductWeeklyDemand: 600));

    AssertEqual("Blocked", result.Guardrail.Status, "guardrail status");
    AssertTrue(result.Guardrail.IsAdoptionBlocked, "blocked scenario should not be adoptable by DDS&OP");
    AssertTrue(result.Guardrail.Checks.Any(check => check.Status == "Red"), "blocked scenario should contain red checks");
    AssertContains(result.ManagementActions, "阻断采纳", "blocked adoption action");
}

static void TestAsopGuardrailRoutesModerateScenario()
{
    var service = new DdsopScenarioService(SeedData.Create(), new DdmrpCalculator());
    var result = service.Evaluate(new ScenarioInput(PlannedShutdownDays: 3));

    AssertEqual("Reconcile", result.Guardrail.Status, "guardrail status");
    AssertTrue(!result.Guardrail.IsAdoptionBlocked, "moderate scenario should not be blocked");
    AssertTrue(result.Guardrail.Checks.Any(check => check.Status == "Yellow"), "moderate scenario should contain yellow checks");
    AssertContains(result.ManagementActions, "集成协调", "integrated reconciliation action");
}

static void TestTimePhasedBufferProjectionCreatesReplenishmentTrace()
{
    var sku = new SkuBufferSetting("SKU-PLAN-001", "Plan Item", "Planning", 100, 5, 1.5m, 3, 700, 10m, 1000);
    var position = new InventoryPosition(sku.Sku, 900, 0, 0);
    var demand = new[]
    {
        new WeeklyDemand(sku.Sku, 1, 600),
        new WeeklyDemand(sku.Sku, 2, 200),
    };

    var run = DemandDrivenPlanningEngine.ProjectBuffers(
        new[] { sku },
        new[] { position },
        demand,
        horizonWeeks: 2);

    var weekOne = run.BufferProjections.Single(point => point.Sku == sku.Sku && point.Week == 1);
    var order = run.ReplenishmentOrders.Single(order => order.Sku == sku.Sku && order.Week == 1);
    var calculationTrace = run.Traces.Single(item => item.Sku == sku.Sku && item.Week == 1);

    AssertEqual("Red", weekOne.BufferStatus, "week one projected buffer status");
    AssertEqual(900, weekOne.StartNetFlow, "week one start net flow");
    AssertEqual(300, weekOne.EndNetFlowBeforeReplenishment, "week one end net flow before replenishment");
    AssertEqual(1650, order.Quantity, "week one replenishment quantity");
    AssertEqual("净流动量 300 位于黄区上沿 1250 及以下，且本周为订货周期复核点，补货到绿区上沿 1950。", calculationTrace.Explanation, "calculation trace");
}

static void TestTimePhasedBufferProjectionWaitsForOrderCycleReview()
{
    var sku = new SkuBufferSetting("SKU-CYCLE-001", "Cycle Item", "Planning", 100, 5, 1.5m, 14, 700, 10m, 1000);
    var position = new InventoryPosition(sku.Sku, 1800, 0, 0);
    var demand = new[]
    {
        new WeeklyDemand(sku.Sku, 1, 300),
        new WeeklyDemand(sku.Sku, 2, 400),
        new WeeklyDemand(sku.Sku, 3, 100),
    };

    var run = DemandDrivenPlanningEngine.ProjectBuffers(
        new[] { sku },
        new[] { position },
        demand,
        horizonWeeks: 3);

    var weekTwo = run.BufferProjections.Single(point => point.Sku == sku.Sku && point.Week == 2);
    var weekThreeOrder = run.ReplenishmentOrders.Single(order => order.Sku == sku.Sku && order.Week == 3);
    var weekTwoTrace = run.Traces.Single(item => item.Sku == sku.Sku && item.Week == 2);

    AssertEqual("Yellow", weekTwo.BufferStatus, "week two should enter yellow");
    AssertTrue(!run.ReplenishmentOrders.Any(order => order.Sku == sku.Sku && order.Week == 2), "week two should wait for the order cycle review");
    AssertEqual(1650, weekThreeOrder.Quantity, "week three replenishment quantity");
    AssertTrue(weekTwoTrace.Explanation.Contains("不是订货周期复核点", StringComparison.Ordinal), "week two trace should explain order cycle waiting");
}

static void TestDemandDrivenRccpUsesProjectedReplenishmentOrders()
{
    var order = new ProjectedReplenishmentOrder("SKU-PLAN-001", 1, 1650, 16_500, "BelowTopOfYellow");
    var routings = new[]
    {
        new ResourceRouting("SKU-PLAN-001", "LINE-1", 0.5m),
    };
    var resources = new[]
    {
        new CapacityResource("LINE-1", "Line 1", 800, 1),
    };

    var load = DemandDrivenPlanningEngine.ProjectRoughCutCapacity(
        new[] { order },
        routings,
        resources,
        horizonWeeks: 1);

    var lineOne = load.Single(item => item.ResourceCode == "LINE-1" && item.Week == 1);
    AssertEqual(825, lineOne.RequiredCapacity, "projected capacity load");
    AssertEqual(103.1m, lineOne.LoadPercent, "projected capacity load percent");
    AssertEqual("Red", lineOne.Status, "projected capacity status");
}

static void TestScenarioServiceExposesWhiteBoxDemandDrivenPlanRun()
{
    var service = new DdsopScenarioService(SeedData.Create(), new DdmrpCalculator());

    var plan = service.EvaluateDemandDrivenPlan(horizonWeeks: 12);

    AssertTrue(plan.BufferProjections.Count >= SeedData.Create().Skus.Count * 12, "plan should include weekly buffer projections by SKU");
    AssertTrue(plan.ReplenishmentOrders.Count > 0, "plan should include projected replenishment orders");
    AssertTrue(plan.CapacityLoads.Count > 0, "plan should include demand driven RCCP loads");
    AssertTrue(plan.Traces.Any(item => item.Explanation.Contains("黄区上沿", StringComparison.Ordinal)), "plan should include white box calculation traces");
}

static void TestScenarioRunWorkspaceExposesRequiredPanels()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));

    var requiredIds = new[]
    {
        "workspace-kpis",
        "scenario-template-list",
        "scenario-comparison",
        "run-preview",
        "budget-comparison-body",
        "buffer-trend-kpis",
        "buffer-inventory-options",
        "buffer-trend-chart",
        "buffer-comparison-strip",
        "buffer-trend-heatmap",
        "buffer-family-summary-body",
        "buffer-sku-metadata",
        "buffer-replenishment-body",
        "buffer-trace-list",
        "rccp-kpis",
        "rccp-resource-summary-body",
        "rccp-heatmap",
        "constraint-capacity-summary-body",
        "constraint-heatmap",
        "constraint-resource-detail",
        "constraint-gap-chart",
        "constraint-action-list",
        "constraint-trace-list",
        "rccp-resource-detail",
        "rccp-action-list",
        "buffer-trend-panel",
        "rccp-panel",
        "projected-supply-panel",
        "supplier-collaboration-kpis",
        "supplier-summary-body",
        "supplier-weekly-grid",
        "supplier-detail-panel",
        "supplier-sku-requirement-body",
        "supplier-action-list",
        "variance-panel",
        "exception-kpis",
        "exception-summary-body",
        "exception-signal-body",
        "apply-exception-to-scenario",
        "trace-panel"
    };

    foreach (var id in requiredIds)
    {
    AssertTrue(page.Contains($"id=\"{id}\"", StringComparison.Ordinal), $"page should expose {id}");
    }

    AssertTrue(page.Contains("缓冲 / 库存趋势", StringComparison.Ordinal), "page should expose graphical buffer trend label");
    AssertTrue(page.Contains("库存选项", StringComparison.Ordinal), "page should expose left-side inventory options");
    AssertTrue(page.Contains("红 / 黄 / 绿山形缓冲区", StringComparison.Ordinal), "page should expose mountain-style buffer bands");
    AssertTrue(page.Contains("净流动量位置", StringComparison.Ordinal), "page should expose net flow position label");
    AssertTrue(page.Contains("预计库存水位", StringComparison.Ordinal), "page should expose projected inventory level label");
    AssertTrue(page.Contains("目标库存", StringComparison.Ordinal), "page should expose target inventory label");
    AssertTrue(page.Contains("时间相位 ADU", StringComparison.Ordinal), "page should expose time-phased ADU label");
    AssertTrue(page.Contains("需求脉冲", StringComparison.Ordinal), "page should expose demand pulse label");
    AssertTrue(page.Contains("受限 / 不受限", StringComparison.Ordinal), "page should expose constrained versus unconstrained label");
    AssertTrue(page.Contains("资源约束对比", StringComparison.Ordinal), "page should expose constraint summary label");
    AssertTrue(page.Contains("不受限需求", StringComparison.Ordinal), "page should expose unconstrained supply label");
    AssertTrue(page.Contains("受限能力", StringComparison.Ordinal), "page should expose constrained capacity label");
    AssertTrue(page.Contains("供应商需求钻取", StringComparison.Ordinal), "page should expose supplier drilldown label");
    AssertTrue(page.Contains("受影响 SKU", StringComparison.Ordinal), "page should expose affected SKU label");
    AssertTrue(!page.Contains("补货点", StringComparison.Ordinal), "page should not describe every yellow penetration as a replenishment point");
}

static void TestPrebuildCampaignMovesReplenishmentBeforeFuturePeak()
{
    var sku = new SkuBufferSetting("SKU-PEAK-001", "Peak Item", "Planning", 100, 5, 1.5m, 3, 700, 10m, 1000);
    var position = new InventoryPosition(sku.Sku, 1950, 0, 0);
    var demand = new[]
    {
        new WeeklyDemand(sku.Sku, 1, 0),
        new WeeklyDemand(sku.Sku, 2, 0),
        new WeeklyDemand(sku.Sku, 3, 1000),
        new WeeklyDemand(sku.Sku, 4, 1000),
    };
    var campaign = new PrebuildCampaign("PB-001", sku.Sku, 1, 3, 4, 1000);

    var run = DemandDrivenPlanningEngine.ProjectBuffers(
        new[] { sku },
        new[] { position },
        demand,
        horizonWeeks: 4,
        prebuildCampaigns: new[] { campaign });

    var prebuildOrder = run.ReplenishmentOrders.Single(order => order.Sku == sku.Sku && order.Week == 1);
    var weekThreeOrderExists = run.ReplenishmentOrders.Any(order => order.Sku == sku.Sku && order.Week == 3);
    var weekThree = run.BufferProjections.Single(point => point.Sku == sku.Sku && point.Week == 3);

    AssertEqual("PrebuildCampaign", prebuildOrder.Trigger, "prebuild trigger");
    AssertEqual(1000, prebuildOrder.Quantity, "prebuild quantity");
    AssertTrue(!weekThreeOrderExists, "prebuild should prevent peak-week replenishment");
    AssertEqual(1950, weekThree.EndNetFlowBeforeReplenishment, "week three protected net flow");
}

static void TestResourceCalendarAdjustmentChangesRccpCapacity()
{
    var order = new ProjectedReplenishmentOrder("SKU-PLAN-001", 1, 1650, 16_500, "BelowTopOfYellow");
    var routings = new[]
    {
        new ResourceRouting("SKU-PLAN-001", "LINE-1", 0.5m),
    };
    var resources = new[]
    {
        new CapacityResource("LINE-1", "Line 1", 800, 1),
    };
    var adjustment = new ResourceCapacityAdjustment("LINE-1", 1, 1.5m, "12-hour shift");

    var load = DemandDrivenPlanningEngine.ProjectRoughCutCapacity(
        new[] { order },
        routings,
        resources,
        horizonWeeks: 1,
        capacityAdjustments: new[] { adjustment });

    var lineOne = load.Single(item => item.ResourceCode == "LINE-1" && item.Week == 1);
    AssertEqual(1200, lineOne.AvailableCapacity, "adjusted capacity");
    AssertEqual(68.8m, lineOne.LoadPercent, "adjusted load percent");
    AssertEqual("Green", lineOne.Status, "adjusted status");
}

static void TestProjectedSupplyRequirementsAggregateBySupplier()
{
    var orders = new[]
    {
        new ProjectedReplenishmentOrder("SKU-A", 1, 100, 1_000, "BelowTopOfYellow"),
        new ProjectedReplenishmentOrder("SKU-B", 1, 40, 800, "BelowTopOfYellow"),
        new ProjectedReplenishmentOrder("SKU-A", 2, 50, 500, "BelowTopOfYellow"),
    };
    var sources = new[]
    {
        new SupplierItemSource("Concentrate Co", "SKU-A", "Concentrates", 10),
        new SupplierItemSource("Concentrate Co", "SKU-B", "Concentrates", 20),
    };

    var requirements = DemandDrivenPlanningEngine.ProjectSupplyRequirements(orders, sources);

    var weekOne = requirements.Single(item => item.Supplier == "Concentrate Co" && item.Week == 1);
    AssertEqual("Concentrates", weekOne.MaterialFamily, "material family");
    AssertEqual(140, weekOne.RequiredQuantity, "week one supplier quantity");
    AssertEqual(1800, weekOne.ProjectedValue, "week one supplier value");
}

static void TestSupplierCollaborationWorkspaceSummarizesSupplierDrilldown()
{
    var source = new TrackingScenarioWorkspaceDataSource(SeedData.Create());
    var service = new SupplierCollaborationWorkspaceService(source);

    var result = service.GetBaseline(12);

    AssertTrue(source.LoadCount == 1, "supplier collaboration service should read through IScenarioWorkspaceDataSource");
    AssertTrue(result.Summaries.Count > 0, "supplier collaboration should summarize suppliers");
    AssertTrue(result.WeeklyCells.Count == result.Summaries.Count * 12, "supplier weekly grid should cover every supplier week");
    AssertTrue(result.WeeklyCells.All(item => item.Gap == Math.Max(0m, item.Variance)), "supplier gap should never be negative");
    AssertTrue(result.Summaries.Any(item => item.TotalUnconstrainedRequired >= 0 && item.TotalConstrainedAvailable >= 0), "supplier summary should expose demand and capacity");
    AssertTrue(result.SkuRequirements.Count > 0, "supplier drilldown should include SKU requirements");
    AssertTrue(result.SkuRequirements.Any(item => item.OrderQuantity > 0 && item.ProjectedValue > 0), "SKU requirements should trace replenishment quantity and value");
    AssertTrue(result.Actions.Count > 0, "supplier collaboration should include recommended actions");
    AssertTrue(result.Trace.Any(item => item.Message.Contains("SKU", StringComparison.Ordinal)), "supplier trace should explain SKU demand contribution");
    AssertTrue(result.Summaries.Any(item => item.Supplier == result.SelectedSupplier), "selected supplier should exist in summaries");
}

static void TestSupplierCollaborationExplainsStatusReasons()
{
    var service = new SupplierCollaborationWorkspaceService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));

    var result = service.GetBaseline(12);
    var yellowCell = result.WeeklyCells.FirstOrDefault(item => item.Status == "Yellow" && item.Gap == 0m);
    var supplierSummary = result.Summaries.FirstOrDefault(item => item.Status == "Yellow");

    AssertTrue(result.WeeklyCells.All(item => !string.IsNullOrWhiteSpace(item.StatusReason)), "supplier weekly cells should explain status reasons");
    AssertTrue(result.Summaries.All(item => !string.IsNullOrWhiteSpace(item.StatusReason)), "supplier summaries should explain status reasons");
    AssertTrue(yellowCell is not null, "seed data should include a yellow supplier cell without a shortage");
    AssertTrue(
        yellowCell!.StatusReason.Contains("风险", StringComparison.Ordinal) ||
        yellowCell.StatusReason.Contains("接近", StringComparison.Ordinal),
        "yellow supplier cell should explain risk or capacity proximity");
    AssertTrue(supplierSummary is not null, "seed data should include a yellow supplier summary");
    AssertTrue(supplierSummary!.RecommendedAction.Contains("确认", StringComparison.Ordinal), "yellow supplier summary should recommend capacity confirmation");
}

static void TestScenarioRunWorkspaceScriptFetchesWorkspaceData()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    AssertTrue(script.Contains("/api/scenario-workspace-data?horizonWeeks=12", StringComparison.Ordinal), "script should fetch scenario workspace data");
    AssertTrue(script.Contains("/api/rccp-workspace?horizonWeeks=12", StringComparison.Ordinal), "script should fetch product RCCP workspace data");
    AssertTrue(script.Contains("/api/constraint-workspace?horizonWeeks=12", StringComparison.Ordinal), "script should fetch constrained versus unconstrained workspace data");
    AssertTrue(script.Contains("/api/buffer-trend-workspace?horizonWeeks=12", StringComparison.Ordinal), "script should fetch graphical buffer trend workspace data");
    AssertTrue(script.Contains("/api/exception-workspace?horizonWeeks=12", StringComparison.Ordinal), "script should fetch exception workspace data");
    AssertTrue(script.Contains("/api/supplier-collaboration-workspace?horizonWeeks=12", StringComparison.Ordinal), "script should fetch supplier collaboration workspace data");
    AssertTrue(script.Contains("/api/scenario-runs/preview", StringComparison.Ordinal), "script should call scenario preview API");
    AssertTrue(script.Contains("预览结果，未保存", StringComparison.Ordinal), "script should label preview results as unsaved");
    AssertTrue(script.Contains("renderProductRccp", StringComparison.Ordinal), "script should render product RCCP workspace");
    AssertTrue(script.Contains("renderConstraintWorkspace", StringComparison.Ordinal), "script should render constrained versus unconstrained workspace");
    AssertTrue(script.Contains("data-constraint-resource", StringComparison.Ordinal), "script should switch selected constraint resource");
    AssertTrue(script.Contains("renderBufferTrendWorkspace", StringComparison.Ordinal), "script should render graphical buffer trend workspace");
    AssertTrue(script.Contains("renderBufferInventoryOptions", StringComparison.Ordinal), "script should render left-side inventory options");
    AssertTrue(script.Contains("data-buffer-family", StringComparison.Ordinal), "script should switch buffer SKU by product family option");
    AssertTrue(script.Contains("data-buffer-sku", StringComparison.Ordinal), "script should switch selected buffer SKU from heatmap");
    AssertTrue(script.Contains("applyExceptionToScenario", StringComparison.Ordinal), "script should bring exception SKU into scenario configuration");
    AssertTrue(script.Contains("previewControls.sku.value", StringComparison.Ordinal), "script should set preview SKU from exception row");
    AssertTrue(script.Contains("selectors.sku.value", StringComparison.Ordinal), "script should synchronize global SKU filter from exception row");
    AssertTrue(script.Contains("previewControls.template.value", StringComparison.Ordinal), "script should set scenario template from exception row");
    AssertTrue(script.Contains("renderSupplierCollaborationWorkspace", StringComparison.Ordinal), "script should render supplier drilldown workspace");
    AssertTrue(script.Contains("data-supplier", StringComparison.Ordinal), "script should switch selected supplier");
    AssertTrue(script.Contains("applyFilters", StringComparison.Ordinal), "script should support client-side filters");
}

static void TestScenarioRunWorkspaceScriptDelegatesBusinessCalculationsToServices()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    AssertTrue(!script.Contains("function calculateBufferTrend", StringComparison.Ordinal), "front-end should not recalculate buffer trend business logic");
    AssertTrue(!script.Contains("function calculateResourceLoads", StringComparison.Ordinal), "front-end should not recalculate RCCP business logic");
    AssertTrue(!script.Contains("function calculateProjectedSupply", StringComparison.Ordinal), "front-end should not recalculate supplier demand business logic");
    AssertTrue(!script.Contains("??", StringComparison.Ordinal), "front-end script should avoid syntax that broke the browser smoke path");
    AssertTrue(script.Contains("前端只做筛选和展示", StringComparison.Ordinal), "trace should state that business calculations come from services");
    AssertTrue(script.Contains("尚未运行预览，变化按 0 显示", StringComparison.Ordinal), "buffer comparison should explain zero deltas before preview");
    AssertTrue(script.Contains("statusReason", StringComparison.Ordinal), "supplier UI should render status reasons");
}

static void TestScenarioWorkspaceSeedDataCoversUseCases()
{
    var source = new SeedScenarioWorkspaceDataSource(SeedData.Create());

    var data = source.Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1)));

    AssertTrue(data.Skus.Count >= 8, "workspace data should include enough SKUs for scenario comparison");
    AssertTrue(data.Families.Any(item => item.Code == "卫星平台"), "workspace data should use satellite manufacturing product families");
    AssertTrue(data.Inventory.Count == data.Skus.Count, "workspace data should include current inventory for each SKU");
    AssertTrue(data.Demand.Count >= data.Skus.Count * 12, "workspace data should include weekly demand across the horizon");
    AssertTrue(data.ResourceRoutings.Count > 0, "workspace data should include SKU to resource routings");
    AssertTrue(data.SupplierItemSources.Count == data.Skus.Count, "workspace data should include supplier item sources");
    AssertTrue(data.HistoricalDemand.Count >= data.Skus.Count * 4, "workspace data should include historical actual demand");
    AssertTrue(data.BudgetBenchmarks.Count >= data.Families.Count * 12, "workspace data should include budget and last-year comparisons");
    AssertTrue(data.ResourceCalendar.Any(item => item.CapacityMultiplier != 1m), "workspace data should include calendar capacity exceptions");
    AssertTrue(data.SupplierCapacityWindows.Any(item => item.RiskStatus == "Red"), "workspace data should include supplier risk windows");
    AssertTrue(data.Guardrails.Count >= 5, "workspace data should include business guardrails");
    AssertTrue(data.ScenarioTemplates.Any(template => ContainsChinese(template.Name)), "scenario templates should be Chinese UI-ready");

    var actionTypes = data.ScenarioTemplates
        .SelectMany(template => template.Actions)
        .Select(action => action.ActionType)
        .ToHashSet(StringComparer.Ordinal);

    AssertTrue(actionTypes.Contains("Prebuild"), "scenario templates should cover pre-build campaigns");
    AssertTrue(actionTypes.Contains("CapacityMultiplier"), "scenario templates should cover capacity adjustments");
    AssertTrue(actionTypes.Contains("MoqOverride"), "scenario templates should cover MOQ overrides");
    AssertTrue(actionTypes.Contains("OrderCycleOverride"), "scenario templates should cover order cycle overrides");
    AssertTrue(actionTypes.Contains("SupplierCapacityLimit"), "scenario templates should cover constrained supply cases");
}

static void TestScenarioWorkspaceAdapterCanMapAlternateSourceStructures()
{
    var adapter = new FakeLegacyScenarioWorkspaceAdapter();
    var source = new LegacyScenarioSource(SeedData.Create());

    var data = adapter.Map(source, new ScenarioWorkspaceDataRequest(8, new DateOnly(2026, 6, 1), FamilyFilter: new[] { "星载电子" }));

    AssertTrue(data.Skus.All(item => item.Family == "星载电子"), "adapter should honor family filters");
    AssertTrue(data.Demand.All(item => item.Week <= 8), "adapter should honor requested horizon");
    AssertTrue(data.ScenarioTemplates.Count > 0, "adapter should return scenario-ready templates");
}

static void TestScenarioPreviewReturnsComparableResults()
{
    var source = new TrackingScenarioWorkspaceDataSource(SeedData.Create());
    var service = new ScenarioRunPreviewService(source);

    var result = service.Preview(new ScenarioRunPreviewRequest(12, "TPL-PREBUILD-PEAK", AdoptionConstraintMode: "FlowFirst"));

    AssertTrue(source.LoadCount == 1, "preview service should read through IScenarioWorkspaceDataSource");
    AssertEqual("baseline", result.Baseline.CaseId, "baseline case id");
    AssertEqual("scenario", result.Scenario.CaseId, "scenario case id");
    AssertTrue(!result.IsPersisted, "preview should not be persisted");
    AssertTrue(result.Baseline.Plan.BufferProjections.Count > 0, "baseline should include buffer projections");
    AssertTrue(result.Scenario.Plan.BufferProjections.Count > 0, "scenario should include buffer projections");
    AssertTrue(result.Scenario.Metrics.FlowIndex > 0, "scenario should expose a flow index");
    AssertTrue(result.Comparison.FlowIndexDelta == result.Scenario.Metrics.FlowIndex - result.Baseline.Metrics.FlowIndex, "comparison should include flow index delta");
    AssertEqual("FlowFirst", result.Request.AdoptionConstraintMode!, "preview should preserve adoption constraint mode");
    AssertTrue(result.Trace.Any(item => item.Message.Contains("FlowFirst", StringComparison.Ordinal)), "preview trace should include adoption constraint mode");
    AssertTrue(result.Trace.Any(item => item.Message.Contains("需求驱动计划引擎", StringComparison.Ordinal)), "preview should trace shared engine use in Chinese");
}

static void TestScenarioPreviewAppliesScenarioParameters()
{
    var service = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));
    var request = new ScenarioRunPreviewRequest(
        12,
        Parameters: new ScenarioRunParameterSet(
            PrebuildCampaigns: new[] { new PrebuildCampaign("PB-TEST", "AV-FPGA-203", 1, 6, 8, 300) },
            CapacityAdjustments: new[] { new ResourceCapacityAdjustment("RES-TVAC", 1, 1.5m, "test capacity relief") },
            SkuPolicyOverrides: new[] { new SkuPolicyOverride("AV-FPGA-203", MinimumOrderQuantity: 500, OrderCycleDays: 10) },
            SupplierCapacityLimits: new[] { new SupplierCapacityLimit("Microchip Space", "进口空间级 FPGA", 1, 12, 1) }));

    var result = service.Preview(request);

    AssertTrue(result.Scenario.Plan.ReplenishmentOrders.Any(item => item.Trigger == "PrebuildCampaign"), "scenario should include pre-build order");
    var baselineTvac = result.Baseline.Plan.CapacityLoads.Single(item => item.ResourceCode == "RES-TVAC" && item.Week == 1);
    var scenarioTvac = result.Scenario.Plan.CapacityLoads.Single(item => item.ResourceCode == "RES-TVAC" && item.Week == 1);
    AssertTrue(scenarioTvac.AvailableCapacity > baselineTvac.AvailableCapacity, "capacity multiplier should increase available capacity");
    AssertTrue(result.Scenario.Plan.Traces.Any(item => item.Sku == "AV-FPGA-203" && item.Explanation.Contains("绿区上沿", StringComparison.Ordinal)), "MOQ/order cycle override should affect buffer trace");
    AssertTrue(result.Scenario.SupplierCapacity.Any(item => item.Gap > 0 && item.RiskStatus == "Red"), "supplier capacity limit should create red gap");
    AssertTrue(result.Comparison.SupplyGapDelta > 0, "supplier gap should increase in scenario");
}

static void TestProductRccpWorkspaceSummarizesResourcesHeatmapAndDetail()
{
    var source = new TrackingScenarioWorkspaceDataSource(SeedData.Create());
    var service = new RccpWorkspaceService(source);

    var result = service.GetBaseline(12);

    AssertTrue(source.LoadCount == 1, "RCCP service should read through IScenarioWorkspaceDataSource");
    AssertTrue(result.ResourceSummaries.Count > 0, "RCCP should summarize resources");
    AssertTrue(result.WeeklyCells.Count == result.ResourceSummaries.Count * 12, "heatmap should cover every resource week");
    AssertTrue(result.ResourceDetails.Count == result.ResourceSummaries.Count, "detail should exist for every resource");
    AssertTrue(result.ResourceSummaries.Any(item => item.PeakLoadPercent >= item.AverageLoadPercent), "summary should calculate peak and average load");
    AssertTrue(result.WeeklyCells.Any(item => item.Variance == item.RequiredCapacity - item.AvailableCapacity), "heatmap should expose capacity variance");
    AssertTrue(result.ResourceDetails.Any(item => item.SkuContributions.Count > 0), "detail should include SKU contributions");
    AssertTrue(result.ResourceDetails.All(item => item.Recommendations.Count > 0), "detail should include action recommendations");
}

static void TestScenarioPreviewReturnsProductRccpComparison()
{
    var service = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));
    var request = new ScenarioRunPreviewRequest(
        12,
        Parameters: new ScenarioRunParameterSet(
            CapacityAdjustments: new[] { new ResourceCapacityAdjustment("RES-TVAC", 1, 1.5m, "test capacity relief") },
            PrebuildCampaigns: new[] { new PrebuildCampaign("PB-RCCP", "AV-FPGA-203", 1, 6, 8, 300) }));

    var result = service.Preview(request);

    var baselineTvac = result.Baseline.Rccp.WeeklyCells.Single(item => item.ResourceCode == "RES-TVAC" && item.Week == 1);
    var scenarioTvac = result.Scenario.Rccp.WeeklyCells.Single(item => item.ResourceCode == "RES-TVAC" && item.Week == 1);

    AssertTrue(result.Baseline.Rccp.ResourceSummaries.Count > 0, "baseline preview should include product RCCP summary");
    AssertTrue(result.Scenario.Rccp.ResourceDetails.Any(item => item.SkuContributions.Any(contribution => contribution.Trigger == "PrebuildCampaign")), "scenario RCCP detail should include pre-build contribution");
    AssertTrue(scenarioTvac.AvailableCapacity > baselineTvac.AvailableCapacity, "capacity multiplier should change RCCP heatmap capacity");
    AssertEqual(
        decimal.Round(result.Scenario.Rccp.MaxPeakLoadPercent - result.Baseline.Rccp.MaxPeakLoadPercent, 1),
        result.RccpComparison.PeakLoadDelta,
        "RCCP peak delta");
    AssertEqual(
        result.Scenario.Rccp.RedWeekCount - result.Baseline.Rccp.RedWeekCount,
        result.RccpComparison.RedWeekDelta,
        "RCCP red week delta");
}

static void TestConstraintWorkspaceSummarizesCapacityAndSupply()
{
    var source = new TrackingScenarioWorkspaceDataSource(SeedData.Create());
    var service = new ConstraintWorkspaceService(source);

    var result = service.GetBaseline(12);

    AssertTrue(source.LoadCount == 1, "constraint service should read through IScenarioWorkspaceDataSource");
    AssertTrue(result.CapacitySummaries.Count > 0, "constraint workspace should summarize constrained capacity");
    AssertTrue(result.CapacityCells.Count == result.CapacitySummaries.Count * 12, "capacity constraint cells should cover every resource week");
    AssertTrue(result.CapacityCells.Any(item => item.Variance == item.UnconstrainedRequired - item.ConstrainedAvailable), "capacity variance should compare unconstrained and constrained values");
    AssertTrue(result.CapacityCells.All(item => item.Gap == Math.Max(0m, item.Variance)), "capacity gap should never be negative");
    AssertTrue(result.CapacityCells.Any(item => item.Status is "Green" or "Yellow" or "Red"), "capacity cells should expose display status");
    AssertTrue(result.SupplySummaries.Count > 0, "constraint workspace should summarize constrained supply");
    AssertTrue(result.SupplyCells.Any(item => item.Gap >= 0m), "supply cells should expose non-negative gap");
    AssertTrue(result.Recommendations.Count > 0, "constraint workspace should expose action recommendations");
    AssertTrue(result.Trace.Any(item => item.Message.Contains("不受限", StringComparison.Ordinal)), "constraint trace should explain unconstrained demand");
}

static void TestScenarioPreviewReturnsConstraintComparison()
{
    var service = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));
    var request = new ScenarioRunPreviewRequest(
        12,
        Parameters: new ScenarioRunParameterSet(
            CapacityAdjustments: new[] { new ResourceCapacityAdjustment("RES-TVAC", 1, 1.5m, "test capacity relief") },
            PrebuildCampaigns: new[] { new PrebuildCampaign("PB-CONSTRAINT", "AV-FPGA-203", 1, 6, 8, 300) },
            SupplierCapacityLimits: new[] { new SupplierCapacityLimit("Microchip Space", "进口空间级 FPGA", 1, 12, 1) }));

    var result = service.Preview(request);

    var baselineTvac = result.Baseline.Constraints.CapacityCells.Single(item => item.ResourceCode == "RES-TVAC" && item.Week == 1);
    var scenarioTvac = result.Scenario.Constraints.CapacityCells.Single(item => item.ResourceCode == "RES-TVAC" && item.Week == 1);

    AssertTrue(result.Baseline.Constraints.CapacityCells.Count > 0, "baseline preview should include constraint capacity cells");
    AssertTrue(result.Scenario.Constraints.SupplyCells.Any(item => item.Gap > 0m && item.Status == "Red"), "supplier limit should create red constrained supply gap");
    AssertTrue(scenarioTvac.ConstrainedAvailable > baselineTvac.ConstrainedAvailable, "capacity multiplier should change constrained available capacity");
    AssertTrue(result.Scenario.Plan.ReplenishmentOrders.Any(item => item.Trigger == "PrebuildCampaign"), "constraint preview should not remove original pre-build orders");
    AssertTrue(result.Scenario.Constraints.Trace.Any(item => item.Stage == "Action"), "constraint preview should include audit trace");
}

static void TestScenarioPreviewReturnsSupplierCollaborationDrilldown()
{
    var service = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));
    var request = new ScenarioRunPreviewRequest(
        12,
        Parameters: new ScenarioRunParameterSet(
            PrebuildCampaigns: new[] { new PrebuildCampaign("PB-SUPPLY", "AV-FPGA-203", 1, 6, 8, 300) },
            SupplierCapacityLimits: new[] { new SupplierCapacityLimit("Microchip Space", "进口空间级 FPGA", 1, 12, 1) }));

    var result = service.Preview(request);

    AssertTrue(result.Baseline.SupplierCollaboration.Summaries.Count > 0, "baseline preview should include supplier drilldown summaries");
    AssertTrue(result.Scenario.SupplierCollaboration.WeeklyCells.Any(item => item.Supplier == "Microchip Space" && item.Gap > 0m && item.Status == "Red"), "supplier limit should create red supplier drilldown cell");
    AssertTrue(result.Scenario.SupplierCollaboration.SkuRequirements.Any(item => item.Supplier == "Microchip Space" && item.Sku == "AV-FPGA-203" && item.Trigger == "PrebuildCampaign"), "pre-build should appear in supplier SKU drilldown");
    AssertTrue(result.Scenario.SupplierCollaboration.Actions.Any(item => item.Supplier == "Microchip Space" && item.Severity == "Red"), "supplier drilldown should include red supplier action");
    AssertTrue(result.Scenario.SupplierCollaboration.Trace.Any(item => item.Message.Contains("SKU", StringComparison.Ordinal)), "supplier drilldown should include trace");
}

static void TestBufferTrendWorkspaceSummarizesKpisHeatmapAndDetail()
{
    var source = new TrackingScenarioWorkspaceDataSource(SeedData.Create());
    var service = new BufferTrendWorkspaceService(source);

    var result = service.GetBaseline(12);

    AssertTrue(source.LoadCount == 1, "buffer trend service should read through IScenarioWorkspaceDataSource");
    AssertTrue(result.Kpis.AverageInventoryValue > 0, "buffer trend should calculate average inventory value");
    AssertTrue(result.Kpis.PeakInventoryValue >= result.Kpis.AverageInventoryValue, "peak inventory should be at least average inventory");
    AssertTrue(result.WeeklyCells.Count == result.SkuDetails.Count * 12, "heatmap should cover every SKU week");
    AssertTrue(result.FamilySummaries.Count > 0, "buffer trend should summarize product families");
    AssertTrue(result.SkuDetails.Any(item => item.Series.Count == 12 && item.Zone.TopOfGreen > item.Zone.TopOfYellow), "SKU detail should include zones and series");
    AssertTrue(result.SkuDetails.Any(item => item.ReplenishmentOrders.Count > 0), "SKU detail should include replenishment orders");
    AssertTrue(result.SkuDetails.Any(item => item.Traces.Count > 0), "SKU detail should include calculation trace");
    AssertTrue(result.SkuDetails.Any(item => item.Sku == result.SelectedSku), "selected SKU should exist in detail");
    AssertTrue(result.Series.Any(item => item.Status is "Red" or "Yellow" or "Green" or "Blue"), "series should expose display statuses");
    AssertTrue(result.Series.All(item => !string.IsNullOrWhiteSpace(item.PeriodStartDate)), "series should expose real time labels");
    AssertTrue(result.SkuDetails.Any(item => item.Series.Select(point => point.TopOfGreen).Distinct().Count() > 1), "time-phased DDMRP zones should vary across weeks");
    AssertTrue(result.Series.All(item => item.TopOfGreen > item.TopOfYellow && item.TopOfYellow > item.TopOfRed), "series should expose DDMRP zone tops");
}

static void TestScenarioPreviewReturnsBufferTrendComparison()
{
    var service = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));
    var request = new ScenarioRunPreviewRequest(
        12,
        Parameters: new ScenarioRunParameterSet(
            PrebuildCampaigns: new[] { new PrebuildCampaign("PB-BUFFER", "AV-FPGA-203", 1, 6, 8, 300) },
            SkuPolicyOverrides: new[] { new SkuPolicyOverride("AV-FPGA-203", MinimumOrderQuantity: 500, OrderCycleDays: 10) }));

    var result = service.Preview(request);
    var scenarioDetail = result.Scenario.BufferTrend.SkuDetails.Single(item => item.Sku == "AV-FPGA-203");

    AssertTrue(result.Baseline.BufferTrend.Series.Count > 0, "baseline preview should include graphical buffer trend series");
    AssertTrue(result.Scenario.BufferTrend.WeeklyCells.Count > 0, "scenario preview should include graphical buffer heatmap cells");
    AssertTrue(scenarioDetail.Series.Any(item => item.IsPrebuild), "pre-build should appear as a buffer trend point");
    AssertTrue(scenarioDetail.ReplenishmentOrders.Any(item => item.Trigger == "PrebuildCampaign"), "pre-build order should appear in SKU detail");
    AssertEqual(
        result.Scenario.BufferTrend.Comparison.AverageInventoryValueDelta,
        result.Scenario.BufferTrend.Kpis.InventoryValueDelta,
        "buffer trend KPI inventory delta");
    AssertEqual(
        result.Scenario.BufferTrend.Kpis.ReplenishmentOrderCount - result.Baseline.BufferTrend.Kpis.ReplenishmentOrderCount,
        result.Scenario.BufferTrend.Comparison.ReplenishmentOrderCountDelta,
        "buffer trend replenishment order delta");
}

static void TestExceptionWorkspaceDetectsVarianceSignalsAndScenarioPresets()
{
    var source = new FixedScenarioWorkspaceDataSource();
    var service = new ExceptionWorkspaceService(source);

    var result = service.GetExceptions(12);
    var summary = result.Exceptions.Single(item => item.Sku == "AV-FPGA-EX");

    AssertTrue(source.LoadCount == 1, "exception workspace should read through IScenarioWorkspaceDataSource");
    AssertEqual(1, result.RedSkuCount, "red exception SKU count");
    AssertTrue(result.DemandSpikeCount > 0, "demand spike should be counted");
    AssertTrue(result.ServiceLossCount > 0, "service loss should be counted");
    AssertTrue(result.BufferRiskCount > 0, "buffer risk should be counted");
    AssertEqual("Red", summary.Severity, "exception severity");
    AssertTrue(summary.Signals.Any(item => item.Reason == "DemandSpike"), "demand variance above threshold should create demand spike signal");
    AssertTrue(summary.Signals.Any(item => item.Reason == "ServiceLoss"), "service below threshold should create service loss signal");
    AssertTrue(summary.Signals.Any(item => item.Reason == "BufferRisk"), "net flow below buffer threshold should create buffer risk signal");
    AssertEqual("TPL-PREBUILD-PEAK", summary.RecommendedTemplateId, "demand spike should recommend prebuild first");
    AssertTrue(summary.ScenarioPresets.Any(item => item.TemplateId == "TPL-ORDER-POLICY"), "service or buffer risk should offer order policy preset");
    AssertTrue(summary.ScenarioPresets.Any(item => item.TemplateId == "TPL-CONSTRAINED"), "star electronics with supply risk should offer constrained preset");
}

static void AssertEqual<T>(T expected, T actual, string label)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{label}: expected {expected}, got {actual}");
    }
}

static void AssertTrue(bool condition, string label)
{
    if (!condition)
    {
        throw new InvalidOperationException(label);
    }
}

static void AssertContains(IEnumerable<string> values, string expectedText, string label)
{
    if (!values.Any(value => value.Contains(expectedText, StringComparison.OrdinalIgnoreCase)))
    {
        throw new InvalidOperationException($"{label}: expected text containing '{expectedText}'");
    }
}

static bool ContainsChinese(string value)
{
    return value.Any(ch => ch >= '\u4e00' && ch <= '\u9fff');
}

internal sealed record LegacyScenarioSource(ValidationData Data);

internal sealed class FakeLegacyScenarioWorkspaceAdapter : IScenarioWorkspaceDataAdapter<LegacyScenarioSource>
{
    public ScenarioWorkspaceDataSet Map(LegacyScenarioSource source, ScenarioWorkspaceDataRequest request)
    {
        return new SeedScenarioWorkspaceDataSource(source.Data).Load(request);
    }
}

internal sealed class TrackingScenarioWorkspaceDataSource : IScenarioWorkspaceDataSource
{
    private readonly SeedScenarioWorkspaceDataSource _inner;

    public TrackingScenarioWorkspaceDataSource(ValidationData data)
    {
        _inner = new SeedScenarioWorkspaceDataSource(data);
    }

    public int LoadCount { get; private set; }

    public ScenarioWorkspaceDataSet Load(ScenarioWorkspaceDataRequest request)
    {
        LoadCount++;
        return _inner.Load(request);
    }
}

internal sealed class FixedScenarioWorkspaceDataSource : IScenarioWorkspaceDataSource
{
    public int LoadCount { get; private set; }

    public ScenarioWorkspaceDataSet Load(ScenarioWorkspaceDataRequest request)
    {
        LoadCount++;
        var sku = new SkuBufferSetting("AV-FPGA-EX", "空间级 FPGA 异常件", "星载电子", 100, 5, 1.5m, 7, 500, 1000, 1000);
        return new ScenarioWorkspaceDataSet(
            request,
            new[] { new ProductFamily("星载电子", "星载电子", 98m, 1.1m, 10_000m) },
            new[] { sku },
            new[] { new InventoryPosition(sku.Sku, 900, 0, 0) },
            new[] { new WeeklyDemand(sku.Sku, 1, 500) },
            Array.Empty<CapacityResource>(),
            Array.Empty<ResourceRouting>(),
            new[] { new SupplierItemSource("Microchip Space", sku.Sku, "进口空间级 FPGA", 1000) },
            new[]
            {
                new HistoricalDemandActual(sku.Sku, -1, 590, 500, 92.5m, 700),
                new HistoricalDemandActual(sku.Sku, -2, 500, 500, 97.2m, 1600)
            },
            Array.Empty<BudgetBenchmark>(),
            Array.Empty<ResourceCalendarEntry>(),
            new[] { new SupplierCapacityWindow("Microchip Space", "进口空间级 FPGA", 1, 1, 90, "Red") },
            new[]
            {
                new ScenarioTemplate("TPL-PREBUILD-PEAK", "促销峰值提前建库", "测试提前建库", Array.Empty<ScenarioTemplateAction>()),
                new ScenarioTemplate("TPL-ORDER-POLICY", "MOQ 与订货周期调整", "测试补货策略", Array.Empty<ScenarioTemplateAction>()),
                new ScenarioTemplate("TPL-CONSTRAINED", "受限与不受限计划对比", "测试供应约束", Array.Empty<ScenarioTemplateAction>())
            },
            Array.Empty<BusinessGuardrail>());
    }
}
