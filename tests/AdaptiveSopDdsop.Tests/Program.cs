using AdaptiveSopDdsop.NetworkStructure;
using AdaptiveSopDdsop.Web.Data;
using AdaptiveSopDdsop.Web.Domain;
using AdaptiveSopDdsop.Web.NetworkStructureIntegration;
using System.Text.Json;
using System.Text.Json.Nodes;

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
    ("Product family dashboard summarizes management view", TestProductFamilyDashboardSummarizesManagementView),
    ("Scenario preview returns product family dashboard comparison", TestScenarioPreviewReturnsProductFamilyDashboardComparison),
    ("Network structure scoring creates white box control point candidates", TestNetworkStructureScoringCreatesWhiteBoxCandidates),
    ("Scenario Run Workspace exposes network structure scoring UI", TestScenarioRunWorkspaceExposesNetworkScoringUi),
    ("Network metrics service calculates traceable Phase 4 metrics", TestNetworkMetricsServiceCalculatesTraceableMetrics),
    ("Network graph service expands upstream and downstream impact scope", TestNetworkGraphServiceExpandsImpactScope),
    ("Network graph service reports master data validation issues", TestNetworkGraphServiceReportsValidationIssues),
    ("Scenario Run Workspace exposes network graph UI", TestScenarioRunWorkspaceExposesNetworkGraphUi),
    ("Network scenario validation outputs candidate impact deltas", TestNetworkScenarioValidationOutputsCandidateImpactDeltas),
    ("Scenario Run Workspace exposes network scenario validation UI", TestScenarioRunWorkspaceExposesNetworkScenarioValidationUi),
    ("Scenario Run Workspace script fetches workspace data", TestScenarioRunWorkspaceScriptFetchesWorkspaceData),
    ("Scenario Run Workspace script delegates business calculations to services", TestScenarioRunWorkspaceScriptDelegatesBusinessCalculationsToServices),
    ("Scenario workspace seed data covers baseline scenario use cases", TestScenarioWorkspaceSeedDataCoversUseCases),
    ("Network structure adapter exposes network structure V2 data model", TestNetworkStructureAdapterExposesNetworkStructureV2DataModel),
    ("Network structure product exposes independent data source boundary", TestNetworkStructureProductExposesIndependentDataSourceBoundary),
    ("Network structure product owns pure network data contracts", TestNetworkStructureProductOwnsPureNetworkDataContracts),
    ("Network structure product exposes standalone host boundary", TestNetworkStructureProductExposesStandaloneHostBoundary),
    ("Scenario workspace exposes complete DDMRP parameter profiles", TestScenarioWorkspaceExposesCompleteDdmrpParameterProfiles),
    ("Scenario workspace adapter can map alternate source structures", TestScenarioWorkspaceAdapterCanMapAlternateSourceStructures),
    ("Scenario preview returns baseline and scenario results from data source", TestScenarioPreviewReturnsComparableResults),
    ("Scenario run persistence saves preview result and audit chain", TestScenarioRunPersistenceSavesPreviewResultAndAuditChain),
    ("Scenario Run Workspace exposes scenario save audit UI", TestScenarioRunWorkspaceExposesSaveAuditUi),
    ("Master settings governance generates proposals from preview", TestMasterSettingsGovernanceGeneratesProposalsFromPreview),
    ("Master settings governance saves audits and advances status", TestMasterSettingsGovernanceSavesAuditsAndAdvancesStatus),
    ("Scenario Run Workspace exposes master settings governance UI", TestScenarioRunWorkspaceExposesMasterSettingsGovernanceUi),
    ("DDSOP-CONFIG-INBOUND-V1 contract exports current reviewed draft payload", TestDdsopConfigInboundContractExportsReviewedDraftPayload),
    ("DDSOP-CONFIG-INBOUND-V1 contract fingerprint is canonical and stable", TestDdsopConfigInboundContractFingerprintIsCanonicalAndStable),
    ("DDSOP-CONFIG-INBOUND-V1 ACK interpreter handles accepted rejected pending and duplicate", TestDdsopConfigInboundAckInterpreterHandlesStatuses),
    ("DDSOP-FEEDBACK-OUTBOUND-V1 payloads can be interpreted without governance mutation", TestDdsopFeedbackOutboundPayloadsCanBeInterpreted),
    ("DDSOP-FEEDBACK-OUTBOUND-V1 inbound ledger preserves raw payload and is idempotent", TestDdsopFeedbackInboundLedgerPreservesRawPayloadAndIdempotency),
    ("DDSOP-FEEDBACK-OUTBOUND-V1 SDBR generated payloads can be accepted", TestDdsopFeedbackInboundLedgerAcceptsSdbrGeneratedPayloads),
    ("DDSOP-FEEDBACK-OUTBOUND-V1 inbound endpoint is exposed", TestDdsopFeedbackInboundEndpointIsExposed),
    ("PRODUCTION-SUPPLIER-IDENTITY-SOURCE-V1 consumer accepts reviewed source without governance mutation", TestProductionSupplierIdentitySourceAcceptsReviewedSourceWithoutGovernanceMutation),
    ("PRODUCTION-SUPPLIER-IDENTITY-SOURCE-V1 rejects governance auto update and contradictory terms", TestProductionSupplierIdentitySourceRejectsGovernanceBoundaryViolations),
    ("PRODUCTION-SUPPLIER-IDENTITY-SOURCE-V1 ledger detects duplicates and idempotency conflicts", TestProductionSupplierIdentitySourceLedgerDetectsDuplicateAndConflict),
    ("PRODUCTION-SUPPLIER-IDENTITY-SOURCE-V1 inbound endpoint is exposed", TestProductionSupplierIdentitySourceEndpointIsExposed),
    ("PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1 consumer accepts reviewed context without governance mutation", TestProductionInventoryQualityEvidenceAcceptsReviewedContextWithoutGovernanceMutation),
    ("PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1 rejects contract business rule violations", TestProductionInventoryQualityEvidenceRejectsBusinessRuleViolations),
    ("PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1 ledger detects duplicates and idempotency conflicts", TestProductionInventoryQualityEvidenceLedgerDetectsDuplicateAndConflict),
    ("PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1 inbound endpoint is exposed", TestProductionInventoryQualityEvidenceEndpointIsExposed),
    ("SDBR-EXECUTION-OBJECT-EVIDENCE-V1 consumer accepts reviewed execution context without governance mutation", TestSdbrExecutionObjectEvidenceAcceptsReviewedContextWithoutGovernanceMutation),
    ("SDBR-EXECUTION-OBJECT-EVIDENCE-V1 rejects boundary and validation violations", TestSdbrExecutionObjectEvidenceRejectsBoundaryAndValidationViolations),
    ("SDBR-EXECUTION-OBJECT-EVIDENCE-V1 accepts reconciled late captured fixture", TestSdbrExecutionObjectEvidenceAcceptsReconciledLateCapture),
    ("SDBR-EXECUTION-OBJECT-EVIDENCE-V1 rejects material consumption without inventory quality evidence", TestSdbrExecutionObjectEvidenceRejectsConsumptionWithoutInventoryQuality),
    ("SDBR-EXECUTION-OBJECT-EVIDENCE-V1 ledger detects duplicates and idempotency conflicts", TestSdbrExecutionObjectEvidenceLedgerDetectsDuplicateAndConflict),
    ("SDBR-EXECUTION-OBJECT-EVIDENCE-V1 inbound endpoint is exposed", TestSdbrExecutionObjectEvidenceEndpointIsExposed),
    ("Scenario preview applies pre-build capacity policy and supplier limits", TestScenarioPreviewAppliesScenarioParameters),
    ("Product RCCP workspace summarizes resources heatmap and detail", TestProductRccpWorkspaceSummarizesResourcesHeatmapAndDetail),
    ("Scenario preview returns product RCCP comparison", TestScenarioPreviewReturnsProductRccpComparison),
    ("Constraint workspace summarizes constrained and unconstrained capacity and supply", TestConstraintWorkspaceSummarizesCapacityAndSupply),
    ("Scenario preview returns constrained and unconstrained comparison", TestScenarioPreviewReturnsConstraintComparison),
    ("Scenario preview returns supplier collaboration drilldown", TestScenarioPreviewReturnsSupplierCollaborationDrilldown),
    ("Buffer trend workspace summarizes KPIs heatmap and SKU detail", TestBufferTrendWorkspaceSummarizesKpisHeatmapAndDetail),
    ("Scenario preview returns graphical buffer trend comparison", TestScenarioPreviewReturnsBufferTrendComparison),
    ("Exception workspace detects variance signals and scenario presets", TestExceptionWorkspaceDetectsVarianceSignalsAndScenarioPresets),
    ("Candidate action combination service uses solver adapter and returns white box combinations", TestCandidateActionCombinationServiceUsesSolverAdapter),
    ("Gurobi optimization solver solves toy problem or reports unavailable", TestGurobiOptimizationSolverToyProblem),
    ("OR-Tools optimization solver solves toy problem", TestOrToolsOptimizationSolverSolvesToyProblem),
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
    var indexPage = File.ReadAllText(Path.GetFullPath(pagePath));
    var networkPartialPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AdaptiveSopDdsop.NetworkStructure.Web", "Pages", "Shared", "_NetworkStructureWorkspace.cshtml");
    var networkPartial = File.ReadAllText(Path.GetFullPath(networkPartialPath));
    var layoutPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AdaptiveSopDdsop.Web", "Pages", "Shared", "_Layout.cshtml");
    var layoutPage = File.ReadAllText(Path.GetFullPath(layoutPath));
    var networkLayoutPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AdaptiveSopDdsop.NetworkStructure.Web", "Pages", "Shared", "_NetworkStructureLayout.cshtml");
    var networkLayoutPage = File.ReadAllText(Path.GetFullPath(networkLayoutPath));
    var standalonePagePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AdaptiveSopDdsop.NetworkStructure.Web", "Pages", "NetworkStructure.cshtml");
    var standalonePage = File.ReadAllText(Path.GetFullPath(standalonePagePath));
    var page = indexPage;
    var networkProductPage = standalonePage + networkPartial;
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var indexModel = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml.cs"));
    var webProject = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "AdaptiveSopDdsop.Web.csproj"));
    var appsettings = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "appsettings.json"));

    AssertTrue(page.Contains("id=\"scenario-workspace-app\"", StringComparison.Ordinal), "homepage should expose Scenario Run Workspace shell");
    AssertTrue(page.Contains("id=\"product-family-dashboard-panel\"", StringComparison.Ordinal), "homepage should expose product family dashboard panel");
    AssertTrue(page.Contains("id=\"product-family-kpis\"", StringComparison.Ordinal), "homepage should expose product family KPI strip");
    AssertTrue(page.Contains("id=\"product-family-card-grid\"", StringComparison.Ordinal), "homepage should expose product family cards");
    AssertTrue(page.Contains("id=\"product-family-weekly-grid\"", StringComparison.Ordinal), "homepage should expose product family weekly grid");
    AssertTrue(page.Contains("id=\"product-family-detail-panel\"", StringComparison.Ordinal), "homepage should expose selected product family detail");
    AssertTrue(page.Contains("产品族看板", StringComparison.Ordinal), "homepage should expose Chinese product family dashboard label");
    AssertTrue(page.Contains("产品族总览", StringComparison.Ordinal), "homepage should expose product family overview label");
    AssertTrue(page.Contains("周度风险网格", StringComparison.Ordinal), "homepage should expose product family weekly risk grid label");
    AssertTrue(page.Contains("选中产品族详情", StringComparison.Ordinal), "homepage should expose selected product family detail label");
    AssertTrue(page.Contains("需求驱动 S&OP 场景运行工作台", StringComparison.Ordinal), "homepage should be Chinese Scenario Run Workspace");
    AssertTrue(!page.Contains("class=\"hero\"", StringComparison.Ordinal), "homepage should no longer render teaching hero");
    AssertTrue(!page.Contains("Pre-build", StringComparison.Ordinal), "homepage should not expose English pre-build labels");
    AssertTrue(!page.Contains("Budget / Last Year", StringComparison.Ordinal), "homepage should use Chinese budget labels");
    AssertTrue(!page.Contains("Demand Driven RCCP", StringComparison.Ordinal), "homepage should use Chinese RCCP labels");
    AssertTrue(!page.Contains("Projected Supply", StringComparison.Ordinal), "homepage should use Chinese supply labels");
    AssertTrue(!page.Contains("Variance Analysis", StringComparison.Ordinal), "homepage should use Chinese exception labels");
    AssertTrue(!page.Contains("Calculation Trace", StringComparison.Ordinal), "homepage should use Chinese trace labels");
    AssertTrue(!page.Contains("Current / Proposed / Reviewed / Approved / Effective / Expired", StringComparison.Ordinal), "homepage should not expose English governance status chain");
    AssertTrue(!page.Contains("DDOM Master Settings", StringComparison.Ordinal), "homepage should not expose English master settings heading");
    AssertTrue(page.Contains("当前 / 待评审 / 已评审 / 已批准 / 已生效 / 已失效", StringComparison.Ordinal), "homepage should show Chinese governance status chain");
    AssertTrue(page.Contains(">异常识别<", StringComparison.Ordinal), "navigation should expose exception-first workflow");
    AssertTrue(page.Contains(">RCCP 与约束<", StringComparison.Ordinal), "navigation should expose RCCP and constraints workflow");
    AssertTrue(page.Contains(">供应商需求<", StringComparison.Ordinal), "navigation should expose supplier demand workflow");
    AssertTrue(page.Contains(">场景留痕<", StringComparison.Ordinal), "navigation should expose scenario audit workflow");
    AssertTrue(page.Contains(">白盒追踪<", StringComparison.Ordinal), "navigation should expose white-box trace workflow");
    AssertTrue(page.Contains("id=\"order-cycle-override\" type=\"number\" min=\"1\"", StringComparison.Ordinal), "order cycle override should not allow zero");
    AssertTrue(page.Contains("id=\"supplier-limit-start-week\"", StringComparison.Ordinal), "supplier limit should expose a start week");
    AssertTrue(page.Contains("id=\"supplier-limit-end-week\"", StringComparison.Ordinal), "supplier limit should expose an end week");
    AssertTrue(page.Contains("id=\"adoption-constraint-select\"", StringComparison.Ordinal), "scenario run should expose customizable adoption constraints");
    AssertTrue(page.Contains("id=\"ddmrp-completeness-chip\"", StringComparison.Ordinal), "data readiness should expose DDMRP parameter completeness chip");
    AssertTrue(page.Contains("id=\"ddmrp-parameter-body\"", StringComparison.Ordinal), "data readiness should expose DDMRP parameter table");
    AssertTrue(page.Contains("id=\"ddmrp-toggle-all\"", StringComparison.Ordinal), "DDMRP parameter table should expose view all toggle");
    AssertTrue(page.Contains("id=\"ddmrp-missing-only\"", StringComparison.Ordinal), "DDMRP parameter table should expose missing-only toggle");
    AssertTrue(page.Contains("id=\"workspace-focus-layer\"", StringComparison.Ordinal), "page should expose focused panel layer");
    AssertTrue(page.Contains("id=\"workspace-detail-drawer\"", StringComparison.Ordinal), "page should expose detail drawer");
    AssertTrue(page.Contains("参数详情", StringComparison.Ordinal), "page should expose parameter detail label");
    AssertTrue(page.Contains("DDMRP 参数", StringComparison.Ordinal), "page should expose DDMRP parameter governance labels");
    AssertTrue(page.Contains("参数完整性", StringComparison.Ordinal), "page should expose parameter completeness labels");
    AssertTrue(!page.Contains("拖拽排序", StringComparison.Ordinal), "page should not expose drag sorting in first UX version");
    AssertTrue(!page.Contains("自由布局", StringComparison.Ordinal), "page should not expose free layout in first UX version");
    AssertTrue(page.Contains("流速优先", StringComparison.Ordinal), "adoption constraints should include a flow-first mode");
    AssertTrue(indexPage.Contains("href=\"@Model.NetworkStructureProductUrl\"", StringComparison.Ordinal), "homepage should link to the configured independent network structure product");
    AssertTrue(indexModel.Contains("NetworkStructureProductUrl", StringComparison.Ordinal), "homepage model should expose configured network product URL");
    AssertTrue(appsettings.Contains("\"ProductUrl\": \"http://127.0.0.1:5296/network-structure\"", StringComparison.Ordinal), "DDS&OP configuration should point to the standalone network product by default");
    AssertTrue(!webProject.Contains("AdaptiveSopDdsop.NetworkStructure.Web.csproj", StringComparison.Ordinal), "DDS&OP web should not reference the network structure UI package");
    AssertTrue(indexPage.Contains("id=\"network-structure-entry-card\"", StringComparison.Ordinal), "homepage should expose network structure as an overview entry card");
    AssertTrue(indexPage.Contains("网络结构评分已从 DDS&OP 主流程拆出", StringComparison.Ordinal), "homepage should explain network scoring is separated from DDS&OP flow");
    AssertTrue(!indexPage.Contains("href=\"#network-structure-scoring-panel\"", StringComparison.Ordinal), "homepage navigation should not treat network structure scoring as a DDS&OP flow step");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "NetworkStructure.cshtml")), "DDS&OP web project should not physically own the network structure page");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Shared", "_NetworkStructureLayout.cshtml")), "DDS&OP web project should not physically own the network structure layout");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Shared", "_NetworkStructureWorkspace.cshtml")), "DDS&OP web project should not physically own the network structure partial");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "css", "network-structure-workspace.css")), "DDS&OP web project should not physically own the network structure stylesheet");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "network-structure-shell.js")), "DDS&OP web project should not physically own the network structure shell script");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "network-structure-workspace.js")), "DDS&OP web project should not physically own the network structure workspace script");
    AssertTrue(File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure.Web", "AdaptiveSopDdsop.NetworkStructure.Web.csproj")), "network structure web package should exist as a separate project");
    AssertTrue(!indexPage.Contains("<partial name=\"_NetworkStructureWorkspace\" />", StringComparison.Ordinal), "homepage should not mount the full network structure product workspace");
    AssertTrue(!indexPage.Contains("~/css/network-structure-workspace.css", StringComparison.Ordinal), "homepage should not load network structure product stylesheet");
    AssertTrue(!indexPage.Contains("~/js/network-structure-workspace.js", StringComparison.Ordinal), "homepage should not load network structure product script");
    AssertTrue(layoutPage.Contains("RenderSectionAsync(\"Styles\"", StringComparison.Ordinal), "layout should expose page-level style section");
    AssertTrue(!layoutPage.Contains("~/css/network-structure-workspace.css", StringComparison.Ordinal), "global layout should not load network structure product stylesheet by default");
    AssertTrue(networkPartial.Contains("NetworkStructureMode", StringComparison.Ordinal), "network partial should support standalone display mode");
    AssertTrue(networkPartial.Contains("network-kpi-strip", StringComparison.Ordinal), "network partial should use product-specific KPI strip semantics");
    AssertTrue(networkPartial.Contains("network-summary-grid", StringComparison.Ordinal), "network partial should use product-specific summary grid semantics");
    AssertTrue(networkPartial.Contains("network-filter-bar", StringComparison.Ordinal), "network partial should use product-specific filter bar semantics");
    AssertTrue(networkPartial.Contains("network-detail-grid", StringComparison.Ordinal), "network partial should use product-specific detail grid semantics");
    AssertTrue(networkPartial.Contains("network-detail-summary", StringComparison.Ordinal), "network partial should use product-specific detail summary semantics");
    AssertTrue(networkPartial.Contains("network-actions", StringComparison.Ordinal), "network partial should use product-specific action row semantics");
    AssertTrue(!networkPartial.Contains("rccp-kpis", StringComparison.Ordinal), "network partial should not borrow DDS&OP RCCP KPI classes");
    AssertTrue(!networkPartial.Contains("rccp-detail-grid", StringComparison.Ordinal), "network partial should not borrow DDS&OP RCCP detail classes");
    AssertTrue(!networkPartial.Contains("product-family-card-grid", StringComparison.Ordinal), "network partial should not borrow DDS&OP product family classes");
    AssertTrue(!networkPartial.Contains("result-context-bar", StringComparison.Ordinal), "network partial should not borrow DDS&OP result context classes");
    AssertTrue(!networkPartial.Contains("preview-actions", StringComparison.Ordinal), "network partial should not borrow DDS&OP preview action classes");
    AssertTrue(!networkPartial.Contains("buffer-sku-metadata", StringComparison.Ordinal), "network partial should not borrow DDS&OP buffer detail classes");
    AssertTrue(standalonePage.Contains("@page \"/network-structure\"", StringComparison.Ordinal), "network structure product should expose an independent route");
    AssertTrue(standalonePage.Contains("Layout = \"_NetworkStructureLayout\"", StringComparison.Ordinal), "standalone network page should use a product-specific layout");
    AssertTrue(standalonePage.Contains("@await Html.PartialAsync(\"_NetworkStructureWorkspace\")", StringComparison.Ordinal), "standalone network page should render the workspace partial through Razor, not an unavailable tag helper");
    AssertTrue(!standalonePage.Contains("<partial name=\"_NetworkStructureWorkspace\"", StringComparison.Ordinal), "standalone network page should not rely on partial tag helper without RCL view imports");
    AssertTrue(standalonePage.Contains("id=\"network-structure-app\"", StringComparison.Ordinal), "standalone network page should expose independent product shell");
    AssertTrue(standalonePage.Contains("返回业务平台", StringComparison.Ordinal), "standalone network page should use a neutral return label");
    AssertTrue(!standalonePage.Contains("返回 DDS&OP", StringComparison.Ordinal), "standalone network page should not present itself as a DDS&OP child page");
    AssertTrue(standalonePage.Contains("网络结构评分工作台", StringComparison.Ordinal), "standalone network page should use product-specific Chinese title");
    AssertTrue(networkLayoutPage.Contains("~/_content/AdaptiveSopDdsop.NetworkStructure.Web/css/network-structure-workspace.css", StringComparison.Ordinal), "standalone network layout should load network structure product stylesheet from the network Web package");
    AssertTrue(!networkLayoutPage.Contains("~/css/site.css", StringComparison.Ordinal), "standalone network layout should not load DDS&OP site stylesheet");
    AssertTrue(networkLayoutPage.Contains("network-structure-product-body", StringComparison.Ordinal), "standalone network layout should expose product-specific body class");
    AssertTrue(standalonePage.Contains("~/_content/AdaptiveSopDdsop.NetworkStructure.Web/js/network-structure-shell.js", StringComparison.Ordinal), "standalone network page should load lightweight network shell script from the network Web package");
    AssertTrue(standalonePage.Contains("~/_content/AdaptiveSopDdsop.NetworkStructure.Web/js/network-structure-workspace.js", StringComparison.Ordinal), "standalone network page should load network workspace module from the network Web package");
    AssertTrue(!standalonePage.Contains("~/js/app.js", StringComparison.Ordinal), "standalone network page should not load the external scenario workspace shell script");
    AssertTrue(standalonePage.Contains("id=\"network-workspace-focus-layer\"", StringComparison.Ordinal), "standalone network page should own its focused panel layer");
    AssertTrue(!indexPage.Contains("id=\"candidate-action-combination-panel\"", StringComparison.Ordinal), "homepage should not inline network structure product details");
    AssertTrue(!page.Contains("id=\"optimization-panel\"", StringComparison.Ordinal), "scenario run should not expose old optimization panel");
    AssertTrue(!page.Contains("id=\"optimization-solver-select\"", StringComparison.Ordinal), "scenario run should not expose old solver selector");
    AssertTrue(networkProductPage.Contains("id=\"candidate-action-combination-panel\"", StringComparison.Ordinal), "network scoring should expose candidate action combination panel");
    AssertTrue(networkProductPage.Contains("id=\"candidate-combination-solver-select\"", StringComparison.Ordinal), "candidate combination selector should expose solver selector");
    AssertTrue(networkProductPage.Contains("<option value=\"Gurobi\">Gurobi</option>", StringComparison.Ordinal), "solver selector should include Gurobi");
    AssertTrue(networkProductPage.Contains("<option value=\"OR-Tools\">OR-Tools</option>", StringComparison.Ordinal), "solver selector should include OR-Tools");
    AssertTrue(networkProductPage.Contains("id=\"select-candidate-combinations\"", StringComparison.Ordinal), "network scoring should expose candidate combination button");
    AssertTrue(networkProductPage.Contains("id=\"candidate-combination-status\"", StringComparison.Ordinal), "network scoring should expose solver status");
    AssertTrue(networkProductPage.Contains("id=\"candidate-combination-list\"", StringComparison.Ordinal), "network scoring should expose candidate combination list");
    AssertTrue(networkProductPage.Contains("id=\"network-capability-panel\"", StringComparison.Ordinal), "network product should expose product capability boundary panel");
    AssertTrue(networkProductPage.Contains("id=\"network-capability-list\"", StringComparison.Ordinal), "network product should expose independent capability list");
    AssertTrue(networkProductPage.Contains("id=\"network-external-dependency-list\"", StringComparison.Ordinal), "network product should expose external dependency list");
    AssertTrue(networkProductPage.Contains("id=\"network-boundary-list\"", StringComparison.Ordinal), "network product should expose boundary statements");
    AssertTrue(networkProductPage.Contains("产品能力边界", StringComparison.Ordinal), "network product should show product capability boundary label");
    AssertTrue(networkProductPage.Contains("外部白盒依赖", StringComparison.Ordinal), "network product should show external white-box dependency label");
    AssertTrue(networkProductPage.Contains("id=\"network-metrics-body\"", StringComparison.Ordinal), "network scoring should expose Phase 4 network metrics table");
    AssertTrue(networkProductPage.Contains("id=\"network-metric-evidence-list\"", StringComparison.Ordinal), "network metrics should expose evidence chain");
    AssertTrue(networkProductPage.Contains("id=\"network-graph-map\"", StringComparison.Ordinal), "network scoring should expose controlled material relationship graph");
    AssertTrue(networkProductPage.Contains("id=\"network-graph-direction-select\"", StringComparison.Ordinal), "network graph should expose direction filter");
    AssertTrue(networkProductPage.Contains("id=\"network-graph-depth-select\"", StringComparison.Ordinal), "network graph should expose depth filter");
    AssertTrue(networkProductPage.Contains("id=\"network-graph-risk-only\"", StringComparison.Ordinal), "network graph should expose risk-only filter");
    AssertTrue(networkProductPage.Contains(">资源路线<", StringComparison.Ordinal), "network graph should use Chinese routing label");
    AssertTrue(!networkProductPage.Contains(">Routing<", StringComparison.Ordinal), "network graph should not expose English routing label");
    AssertTrue(!networkProductPage.Contains("lead time profile", StringComparison.OrdinalIgnoreCase), "network metrics copy should not expose English lead time profile label");
    AssertTrue(page.Contains("id=\"multi-scenario-comparison-body\"", StringComparison.Ordinal), "scenario comparison should expose multi-scenario comparison body");
    AssertTrue(page.Contains("id=\"candidate-impact-matrix-body\"", StringComparison.Ordinal), "scenario comparison should expose candidate impact matrix body");
    AssertTrue(page.Contains("多方案比较", StringComparison.Ordinal), "scenario comparison should show multi-scenario comparison label");
    AssertTrue(page.Contains("候选动作影响矩阵", StringComparison.Ordinal), "scenario comparison should show candidate impact matrix label");
    AssertTrue(networkProductPage.Contains("候选动作组合选择", StringComparison.Ordinal), "network product should show candidate combination selection label");
    AssertTrue(networkProductPage.Contains("选择候选组合", StringComparison.Ordinal), "network product should show candidate combination selection action");
    AssertTrue(networkProductPage.Contains("组合选择器", StringComparison.Ordinal), "network product should show solver as a combination selector");
    AssertTrue(!page.Contains("优化推荐", StringComparison.Ordinal), "scenario run should not expose the old solver recommendation label");
    AssertTrue(!page.Contains("生成优化推荐", StringComparison.Ordinal), "scenario run should not expose the old solver recommendation action");
    AssertTrue(networkProductPage.Contains("不自动采纳、不保存、不审批", StringComparison.Ordinal), "candidate combinations should not be directly applied");
    AssertTrue(!page.Contains(">自动采纳<", StringComparison.Ordinal), "scenario run should not expose automatic adoption action");
    AssertTrue(!page.Contains(">自动审批<", StringComparison.Ordinal), "scenario run should not expose automatic approval action");
    AssertTrue(!page.Contains(">自动保存<", StringComparison.Ordinal), "scenario run should not expose automatic save action");

    var scriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js");
    var appScript = File.ReadAllText(Path.GetFullPath(scriptPath));
    var networkScriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AdaptiveSopDdsop.NetworkStructure.Web", "wwwroot", "js", "network-structure-workspace.js");
    var networkScript = File.ReadAllText(Path.GetFullPath(networkScriptPath));
    var networkShellPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AdaptiveSopDdsop.NetworkStructure.Web", "wwwroot", "js", "network-structure-shell.js");
    var networkShell = File.ReadAllText(Path.GetFullPath(networkShellPath));
    var script = appScript;
    AssertTrue(networkScript.Contains("ItemWithoutRouting: \"成品或半成品缺少资源路线\"", StringComparison.Ordinal), "network script should translate item-without-routing validation code");
    AssertTrue(networkScript.Contains("BufferWithoutExecutableLocation: \"解耦点缺少可执行库存位置\"", StringComparison.Ordinal), "network script should translate buffer executable-location validation code");
    AssertTrue(networkScript.Contains("SupplierSource: \"供应来源\"", StringComparison.Ordinal), "network script should translate supplier-source evidence type");
    AssertTrue(networkScript.Contains("LeadTimeProfile: \"提前期档案\"", StringComparison.Ordinal), "network script should translate lead-time profile evidence type");
    AssertTrue(networkScript.Contains("BufferSetting: \"缓冲设置\"", StringComparison.Ordinal), "network script should translate buffer-setting evidence type");
    AssertTrue(networkScript.Contains("function solverStatusName", StringComparison.Ordinal), "network script should translate solver statuses before display");
    AssertTrue(networkScript.Contains("/api/network-structure-capabilities", StringComparison.Ordinal), "network module should fetch product capability boundary API");
    AssertTrue(networkScript.Contains("function renderNetworkCapabilities", StringComparison.Ordinal), "network module should render product capability boundary");
    AssertTrue(networkScript.Contains("renderCapabilities: renderNetworkCapabilities", StringComparison.Ordinal), "network module should export capability rendering through its public API");
    AssertTrue(networkScript.Contains("businessText(item.summary)", StringComparison.Ordinal), "network script should translate network metric summary text before display");
    AssertTrue(networkScript.Contains("businessText(item.validationSummary)", StringComparison.Ordinal), "network script should translate scenario validation summary before display");
    AssertTrue(networkScript.Contains("businessText(action.actionType)", StringComparison.Ordinal), "network script should translate candidate action type before display");
    AssertTrue(!appScript.Contains("NetworkStructureProductWorkspace", StringComparison.Ordinal), "main app shell should not initialize or render the network structure product module");
    AssertTrue(!appScript.Contains("window.NetworkStructureProductHost", StringComparison.Ordinal), "scenario workspace shell should not register a network product host adapter");
    AssertTrue(networkShell.Contains("window.NetworkStructureProductHost", StringComparison.Ordinal), "standalone network shell should expose the neutral host adapter contract");
    AssertTrue(!appScript.Contains("function renderNetworkStructureScoring", StringComparison.Ordinal), "main app shell should not own network structure render functions");
    AssertTrue(!appScript.Contains("NetworkStructureProductWorkspace?.renderScoring", StringComparison.Ordinal), "main app shell should not render network scoring through the module API");
    AssertTrue(!appScript.Contains("NetworkStructureProductWorkspace?.loadData", StringComparison.Ordinal), "main app shell should not load network product data");
    AssertTrue(!appScript.Contains("renderNetworkStructureScoring(state.networkScoring)", StringComparison.Ordinal), "main app shell should not call network internals directly");
    AssertTrue(!appScript.Contains("loadNetworkGraph(state.selectedNetworkItem", StringComparison.Ordinal), "main app shell should not call network graph internals directly");
    AssertTrue(networkShell.Contains("NetworkStructureProductWorkspace?.renderScoring", StringComparison.Ordinal), "standalone network shell should render network scoring through the same module API");
    AssertTrue(networkShell.Contains("NetworkStructureProductWorkspace?.renderCapabilities", StringComparison.Ordinal), "standalone network shell should render product capabilities through the same module API");
    AssertTrue(networkShell.Contains("NetworkStructureProductWorkspace?.loadData", StringComparison.Ordinal), "standalone network shell should load network product data through the same module API");
    AssertTrue(networkShell.Contains("includeNetworkData: true", StringComparison.Ordinal), "standalone network shell should ask the network module to load its narrow data packet");
    AssertTrue(!networkShell.Contains("renderNetworkStructureScoring(state.networkScoring)", StringComparison.Ordinal), "standalone network shell should not call network internals directly");
    AssertTrue(!networkShell.Contains("loadNetworkGraph(state.selectedNetworkItem", StringComparison.Ordinal), "standalone network shell should not call network graph internals directly");
    AssertTrue(networkScript.Contains("function renderNetworkStructureScoring", StringComparison.Ordinal), "network structure script should own network structure render functions");
    AssertTrue(networkScript.Contains("renderScoring: renderNetworkStructureScoring", StringComparison.Ordinal), "network structure module should export scoring rendering through its public API");
    AssertTrue(networkScript.Contains("loadGraph: loadNetworkGraph", StringComparison.Ordinal), "network structure module should export graph loading through its public API");
    AssertTrue(networkScript.Contains("loadData: loadNetworkStructureWorkspaceData", StringComparison.Ordinal), "network structure module should export network data loading through its public API");
    AssertTrue(networkScript.Contains("initializeNetworkCollapsiblePanels", StringComparison.Ordinal), "network structure module should own standalone collapsible panel behavior");
    AssertTrue(networkScript.Contains("openNetworkFocusedPanel", StringComparison.Ordinal), "network structure module should own standalone focused panel behavior");
    AssertTrue(networkScript.Contains("data-network-collapse-panel", StringComparison.Ordinal), "network structure module should use product-specific collapse attributes");
    AssertTrue(networkScript.Contains("data-network-focus-panel", StringComparison.Ordinal), "network structure module should use product-specific focus attributes");
    AssertTrue(networkScript.Contains("panel.dataset.collapsePanel !== undefined", StringComparison.Ordinal), "network structure module should avoid double-enhancing panels already managed by DDS&OP");
    AssertTrue(networkScript.Contains("function networkHost", StringComparison.Ordinal), "network structure script should access shell helpers through its host adapter");
    AssertTrue(networkScript.Contains("networkHost().state", StringComparison.Ordinal), "network structure script should read state through the host adapter");
    AssertTrue(!networkScript.Contains("state.", StringComparison.Ordinal), "network structure script should not directly depend on DDS&OP lexical state");
    AssertTrue(!networkScript.Contains("byId(", StringComparison.Ordinal), "network structure script should not directly depend on DDS&OP byId helper");
    AssertTrue(!networkScript.Contains("valueOr(", StringComparison.Ordinal), "network structure script should not directly depend on DDS&OP valueOr helper");
    AssertTrue(!networkScript.Contains("showWorkspaceError(error)", StringComparison.Ordinal), "network structure script should not directly call the external workspace error helper");
    AssertTrue(networkScript.Contains("networkShowWorkspaceError(error)", StringComparison.Ordinal), "network structure script should route errors through the host adapter");
    AssertTrue(networkScript.Contains("networkRenderMultiScenarioComparison(result)", StringComparison.Ordinal), "network structure script should route scenario comparison rendering through the host adapter");
    AssertTrue(networkScript.Contains("window.NetworkStructureProductWorkspace", StringComparison.Ordinal), "network structure script should expose a product-neutral module initializer");
    AssertTrue(!networkScript.Contains("DdaeNetworkStructure", StringComparison.Ordinal), "network structure script should not expose product-family-specific branded globals");
    AssertTrue(!networkShell.Contains("DdaeNetworkStructure", StringComparison.Ordinal), "network structure shell should not expose product-family-specific branded globals");
    AssertTrue(networkShell.Contains("function loadNetworkStructureProduct", StringComparison.Ordinal), "standalone network shell should load network product data without the external scenario workspace app");
    AssertTrue(!appScript.Contains("/api/network-structure-scoring", StringComparison.Ordinal), "main app shell should not own network scoring API addresses");
    AssertTrue(!appScript.Contains("/api/network-metrics", StringComparison.Ordinal), "main app shell should not own network metrics API addresses");
    AssertTrue(!appScript.Contains("/api/network-scenario-validation", StringComparison.Ordinal), "main app shell should not own network scenario validation API addresses");
    AssertTrue(!networkShell.Contains("/api/network-structure-data", StringComparison.Ordinal), "standalone network shell should not own network data API addresses");
    AssertTrue(!networkShell.Contains("/api/network-structure-scoring", StringComparison.Ordinal), "standalone network shell should not own network scoring API addresses");
    AssertTrue(!networkShell.Contains("/api/network-metrics", StringComparison.Ordinal), "standalone network shell should not own network metrics API addresses");
    AssertTrue(!networkShell.Contains("/api/network-scenario-validation", StringComparison.Ordinal), "standalone network shell should not own network scenario validation API addresses");
    AssertTrue(networkScript.Contains("/api/network-structure-data?", StringComparison.Ordinal), "network module should fetch pure network structure data when standalone mode asks for it");
    AssertTrue(networkScript.Contains("/api/network-structure-capabilities", StringComparison.Ordinal), "network module should fetch product capability boundaries");
    AssertTrue(!networkShell.Contains("/api/scenario-workspace-data", StringComparison.Ordinal), "standalone network shell should not fetch full scenario workspace data");
    AssertTrue(networkScript.Contains("/api/network-structure-scoring?", StringComparison.Ordinal), "network module should fetch network scoring");
    AssertTrue(networkScript.Contains("/api/network-metrics?", StringComparison.Ordinal), "network module should fetch network metrics");
    AssertTrue(networkScript.Contains("/api/network-scenario-validation?", StringComparison.Ordinal), "network module should fetch network scenario validation");
    AssertTrue(!networkShell.Contains("/api/product-family-dashboard", StringComparison.Ordinal), "standalone network shell should not fetch external product family dashboard");
    AssertTrue(!networkShell.Contains("/api/rccp-workspace", StringComparison.Ordinal), "standalone network shell should not fetch external RCCP workspace");
    var cssPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AdaptiveSopDdsop.Web", "wwwroot", "css", "site.css");
    var shellCss = File.ReadAllText(Path.GetFullPath(cssPath));
    var networkCssPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AdaptiveSopDdsop.NetworkStructure.Web", "wwwroot", "css", "network-structure-workspace.css");
    var networkCss = File.ReadAllText(Path.GetFullPath(networkCssPath));
    var css = shellCss + networkCss;
    AssertTrue(!shellCss.Contains(".network-graph-map", StringComparison.Ordinal), "main shell stylesheet should not own network graph styles");
    AssertTrue(!shellCss.Contains(".candidate-combination-list", StringComparison.Ordinal), "main shell stylesheet should not own candidate combination styles");
    AssertTrue(!shellCss.Contains(".network-product-shell", StringComparison.Ordinal), "main shell stylesheet should not own standalone network product shell styles");
    AssertTrue(networkCss.Contains(".network-graph-map", StringComparison.Ordinal), "network stylesheet should own network graph styles");
    AssertTrue(networkCss.Contains(".candidate-combination-list", StringComparison.Ordinal), "network stylesheet should own candidate combination styles");
    AssertTrue(networkCss.Contains(".network-product-shell", StringComparison.Ordinal), "network stylesheet should own standalone network product shell styles");
    AssertTrue(networkCss.Contains(".network-structure-product-body", StringComparison.Ordinal), "network stylesheet should own standalone body styles");
    AssertTrue(networkCss.Contains(".button", StringComparison.Ordinal), "network stylesheet should own standalone button styles");
    AssertTrue(networkCss.Contains(".status-chip", StringComparison.Ordinal), "network stylesheet should own standalone status chip styles");
    AssertTrue(networkCss.Contains(".data-table", StringComparison.Ordinal), "network stylesheet should own standalone table styles");
    AssertTrue(networkCss.Contains(".panel-heading", StringComparison.Ordinal), "network stylesheet should own standalone panel heading styles");
    AssertTrue(networkCss.Contains(".network-focus-backdrop", StringComparison.Ordinal), "network stylesheet should own standalone focused panel backdrop");
    AssertTrue(networkCss.Contains(".network-collapse-toggle", StringComparison.Ordinal), "network stylesheet should own standalone collapse heading styles");
    AssertTrue(networkCss.Contains(".network-panel-action-button", StringComparison.Ordinal), "network stylesheet should own standalone focus action styles");
    AssertTrue(networkCss.Contains(".network-kpi-strip", StringComparison.Ordinal), "network stylesheet should own network KPI strip styles");
    AssertTrue(networkCss.Contains(".network-summary-grid", StringComparison.Ordinal), "network stylesheet should own network summary grid styles");
    AssertTrue(networkCss.Contains(".network-capability-card", StringComparison.Ordinal), "network stylesheet should own product capability card styles");
    AssertTrue(networkCss.Contains(".network-boundary-list", StringComparison.Ordinal), "network stylesheet should own product boundary list styles");
    AssertTrue(networkCss.Contains(".network-filter-bar", StringComparison.Ordinal), "network stylesheet should own network filter bar styles");
    AssertTrue(networkCss.Contains(".network-detail-grid", StringComparison.Ordinal), "network stylesheet should own network detail grid styles");
    AssertTrue(networkCss.Contains(".network-detail-summary", StringComparison.Ordinal), "network stylesheet should own network detail summary styles");
    AssertTrue(networkCss.Contains(".network-actions", StringComparison.Ordinal), "network stylesheet should own network action row styles");
    AssertTrue(!networkCss.Contains(".rccp-kpis", StringComparison.Ordinal), "network stylesheet should not define DDS&OP RCCP KPI classes");
    AssertTrue(!networkCss.Contains(".rccp-detail-grid", StringComparison.Ordinal), "network stylesheet should not define DDS&OP RCCP detail classes");
    AssertTrue(!networkCss.Contains(".product-family-card-grid", StringComparison.Ordinal), "network stylesheet should not define DDS&OP product family classes");
    AssertTrue(!networkCss.Contains(".result-context-bar", StringComparison.Ordinal), "network stylesheet should not define DDS&OP result context classes");
    AssertTrue(!networkCss.Contains(".preview-actions", StringComparison.Ordinal), "network stylesheet should not define DDS&OP preview action classes");
    AssertTrue(!networkCss.Contains(".buffer-sku-metadata", StringComparison.Ordinal), "network stylesheet should not define DDS&OP buffer detail classes");
    AssertTrue(networkCss.Contains("#candidate-action-combination-panel.is-focused-panel", StringComparison.Ordinal), "network stylesheet should expand candidate combinations by id in focused view");
    AssertTrue(script.Contains("const previewFieldHelp", StringComparison.Ordinal), "script should define preview field help dictionary");
    AssertTrue(script.Contains("const navigationHelp", StringComparison.Ordinal), "script should define navigation help dictionary");
    AssertTrue(script.Contains("renderDdmrpParameterCompleteness", StringComparison.Ordinal), "script should render DDMRP parameter completeness");
    AssertTrue(script.Contains("data.ddmrpParameters", StringComparison.Ordinal), "script should consume DDMRP parameter profiles from API");
    AssertTrue(script.Contains("validationMessage", StringComparison.Ordinal), "script should display DDMRP parameter validation messages");
    AssertTrue(script.Contains("initializePanelWorkspaceActions", StringComparison.Ordinal), "script should initialize focused panel actions");
    AssertTrue(script.Contains("openFocusedPanel", StringComparison.Ordinal), "script should open focused panel view");
    AssertTrue(script.Contains("closeFocusedPanel", StringComparison.Ordinal), "script should close focused panel view");
    AssertTrue(script.Contains("focusedPanelWasExpanded", StringComparison.Ordinal), "focused panel should remember original collapse state");
    AssertTrue(script.Contains("if (!wasExpanded) return", StringComparison.Ordinal), "collapsed panels should not enter focused view");
    AssertTrue(script.Contains("action.hidden = !expanded", StringComparison.Ordinal), "focused action should only appear for expanded panels");
    AssertTrue(script.Contains("collapseState.set(state.focusedPanelCollapseKey", StringComparison.Ordinal), "focused panel should restore collapse state after closing");
    AssertTrue(script.Contains("if (state.focusedPanel === panel) return", StringComparison.Ordinal), "focused panel heading should not collapse while focused");
    AssertTrue(css.Contains(".is-focused-panel > .collapse-toggle .collapse-indicator", StringComparison.Ordinal), "focused panel should hide collapse indicator while exit focus is visible");
    AssertTrue(script.Contains("initializeResizableTables", StringComparison.Ordinal), "script should initialize resizable table containers");
    AssertTrue(script.Contains("openWorkspaceDrawer", StringComparison.Ordinal), "script should open workspace detail drawer");
    AssertTrue(script.Contains("data-ddmrp-sku", StringComparison.Ordinal), "script should attach DDMRP row detail hooks");
    AssertTrue(script.Contains("data-guardrail-index", StringComparison.Ordinal), "script should attach guardrail row detail hooks");
    AssertTrue(script.Contains("state.data?.ddmrpParameters", StringComparison.Ordinal), "DDMRP drawer should read from state data");
    AssertTrue(script.Contains("state.data?.guardrails", StringComparison.Ordinal), "guardrail drawer should read from state data");
    AssertTrue(script.Contains("renderMultiScenarioComparison", StringComparison.Ordinal), "script should render multi-scenario comparison");
    AssertTrue(script.Contains("candidateImpactMatrix", StringComparison.Ordinal), "script should consume candidate impact matrix");
    AssertTrue(script.Contains("combinationComparisons", StringComparison.Ordinal), "script should consume combination comparisons");
    AssertTrue(!appScript.Contains("solverName", StringComparison.Ordinal), "DDS&OP main script should not own candidate-combination solver controls");
    AssertTrue(networkScript.Contains("solverName", StringComparison.Ordinal), "network module should send selected solver to candidate action combination API");
    AssertTrue(networkScript.Contains("renderNetworkGraphMap", StringComparison.Ordinal), "network module should render controlled local material graph");
    AssertTrue(networkScript.Contains("data-network-graph-node", StringComparison.Ordinal), "network module should link graph nodes to selected material");
    AssertTrue(networkScript.Contains("networkGraphDirection", StringComparison.Ordinal), "network module should support graph direction filtering");
    AssertTrue(networkScript.Contains("networkGraphRiskOnly", StringComparison.Ordinal), "network module should support risk-only graph filtering");
    AssertTrue(networkScript.Contains("itemTypeName", StringComparison.Ordinal), "network module should translate network item types");
    AssertTrue(networkScript.Contains("evidenceTypeName", StringComparison.Ordinal), "network module should translate network evidence types");
    AssertTrue(networkScript.Contains("validationRuleName", StringComparison.Ordinal), "network module should translate network validation rule codes");
    AssertTrue(networkScript.Contains("businessText", StringComparison.Ordinal), "network module should translate backend evidence text for UI display");
    AssertTrue(networkScript.Contains("正在选择候选动作组合", StringComparison.Ordinal), "network module should show candidate combination selection progress");
    AssertTrue(!networkScript.Contains("正在生成优化推荐", StringComparison.Ordinal), "network module should not show old solver recommendation progress");
    AssertTrue(!networkScript.Contains("没有优化推荐", StringComparison.Ordinal), "network module should not show old solver recommendation empty state");
    AssertTrue(networkScript.Contains("管理取舍", StringComparison.Ordinal), "network module should expose management trade-off labels");
    AssertTrue(css.Contains("overflow-x: auto", StringComparison.Ordinal) && css.Contains("scroll-snap-type: x proximity", StringComparison.Ordinal), "candidate combinations should expand horizontally");
    AssertTrue(css.Contains(".is-focused-panel .candidate-combination-list", StringComparison.Ordinal) && css.Contains("repeat(3, minmax(320px, 1fr))", StringComparison.Ordinal), "focused candidate combinations should expand across the right side");
    AssertTrue(css.Contains("width: calc(100vw - 48px)", StringComparison.Ordinal), "focused panel should use the available viewport width");
    AssertTrue(!script.Contains("cloneNode", StringComparison.Ordinal), "focused panel should move existing DOM rather than clone stable nodes");
    AssertTrue(script.Contains("initializeCollapsiblePanels", StringComparison.Ordinal), "script should initialize collapsible workspace panels");
    AssertTrue(script.Contains("dataset.collapsePanel", StringComparison.Ordinal), "script should add collapse panel data attribute");
    AssertTrue(script.Contains("dataset.collapseToggle", StringComparison.Ordinal), "script should add collapse toggle data attribute");
    AssertTrue(script.Contains("dataset.collapseBody", StringComparison.Ordinal), "script should add collapse body data attribute");
    AssertTrue(script.Contains("aria-expanded", StringComparison.Ordinal), "collapse toggles should expose aria-expanded");
    AssertTrue(script.Contains("body.hidden", StringComparison.Ordinal), "collapse toggles should use hidden body state");
    AssertTrue(script.Contains("item.setAttribute(\"title\", help)", StringComparison.Ordinal), "navigation help should use title tooltip");
    AssertTrue(!script.Contains("item.insertAdjacentHTML(\"beforeend\", helpTrigger(help))", StringComparison.Ordinal), "navigation should not insert question mark help triggers");
    AssertTrue(css.Contains(".collapsible-panel", StringComparison.Ordinal), "CSS should style collapsible panels");
    AssertTrue(css.Contains(".collapse-toggle", StringComparison.Ordinal), "CSS should style collapse toggles");
    AssertTrue(css.Contains(".collapse-body", StringComparison.Ordinal), "CSS should style collapse body");
    AssertTrue(css.Contains(".is-focused-panel", StringComparison.Ordinal), "CSS should style focused panels");
    AssertTrue(css.Contains(".workspace-drawer", StringComparison.Ordinal), "CSS should style workspace drawer");
    AssertTrue(css.Contains(".resizable-table-shell", StringComparison.Ordinal), "CSS should style resizable table shell");
    AssertTrue(css.Contains("overflow-x: auto", StringComparison.Ordinal), "wide tables should keep horizontal scroll inside containers");
    AssertTrue(!css.Contains(".nav-item > .help-trigger", StringComparison.Ordinal), "navigation should not keep question mark trigger styles");
    AssertTrue(script.Contains("供应限制开始周", StringComparison.Ordinal), "script should explain supplier limit start week");
    AssertTrue(script.Contains("供应承诺能力", StringComparison.Ordinal), "script should explain supplier committed capacity");
    AssertTrue(script.Contains("订货周期覆盖值", StringComparison.Ordinal), "script should explain order cycle override");
    AssertTrue(script.Contains("normalizeWorkspaceFlow", StringComparison.Ordinal), "script should normalize page order to the business workflow");
    AssertTrue(script.Contains("masterSettingTypeLabel", StringComparison.Ordinal), "script should translate master setting types");
    AssertTrue(script.Contains("auditEventLabel", StringComparison.Ordinal), "script should translate audit event labels");
    AssertTrue(script.Contains("syncSkuPolicyDefaults", StringComparison.Ordinal), "script should sync SKU order cycle defaults");
    AssertTrue(script.Contains("syncSupplierLimitDefaults", StringComparison.Ordinal), "script should sync supplier limit defaults");
    AssertTrue(script.Contains("startWeek: supplierStartWeek", StringComparison.Ordinal), "supplier limit payload should use selected start week");
    AssertTrue(script.Contains("endWeek: supplierEndWeek", StringComparison.Ordinal), "supplier limit payload should use selected end week");
    AssertTrue(script.Contains("adoptionConstraintMode", StringComparison.Ordinal), "preview payload should include adoption constraint mode");
    AssertTrue(script.Contains("targetFlowIndex", StringComparison.Ordinal), "script should expose target flow in the workspace");
    AssertTrue(script.Contains("evaluateAdoption", StringComparison.Ordinal), "script should evaluate preview against the selected adoption constraint");
    AssertTrue(script.Contains("违反规则", StringComparison.Ordinal), "script should show which adoption rule is violated");
    AssertTrue(script.Contains("adoption-rule-list", StringComparison.Ordinal), "script should render adoption rule details");
    AssertTrue(script.Contains("服务红线", StringComparison.Ordinal), "script should explain service guardrail violations");
    AssertTrue(script.Contains("供应硬约束", StringComparison.Ordinal), "script should explain supply guardrail violations");
    AssertTrue(!appScript.Contains("/api/network-metrics?", StringComparison.Ordinal), "DDS&OP main script should not call network metrics API");
    AssertTrue(networkScript.Contains("/api/network-metrics?", StringComparison.Ordinal), "network module should call network metrics API");
    AssertTrue(!appScript.Contains("/api/candidate-action-combinations/select", StringComparison.Ordinal), "DDS&OP main script should not call candidate action combination API");
    AssertTrue(networkScript.Contains("/api/candidate-action-combinations/select", StringComparison.Ordinal), "network module should call candidate action combination API");
    AssertTrue(!appScript.Contains("/api/scenario-runs/optimize", StringComparison.Ordinal), "DDS&OP main script should not call old scenario optimization API");
    AssertTrue(!networkScript.Contains("/api/scenario-runs/optimize", StringComparison.Ordinal), "network module should not call old scenario optimization API");
    AssertTrue(!appScript.Contains("applyOptimizationRecommendation", StringComparison.Ordinal), "DDS&OP main script should not bring solver results directly into scenario controls");
    AssertTrue(!networkScript.Contains("applyOptimizationRecommendation", StringComparison.Ordinal), "network module should not bring solver results directly into scenario controls");
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
        "single-sku-workbench",
        "single-sku-activity-body",
        "single-sku-attribute-body",
        "single-sku-sizing-body",
        "single-sku-bom-body",
        "single-sku-order-body",
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
    AssertTrue(page.Contains("单 SKU 仿真工作台", StringComparison.Ordinal), "page should expose single SKU simulation workbench");
    AssertTrue(page.Contains("活动列表", StringComparison.Ordinal), "page should expose SKU activity list");
    AssertTrue(page.Contains("缓冲 sizing", StringComparison.Ordinal), "page should expose buffer sizing");
    AssertTrue(page.Contains("BOM", StringComparison.Ordinal), "page should expose BOM detail");
    AssertTrue(page.Contains("订单明细", StringComparison.Ordinal), "page should expose order detail");
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

static void TestProductFamilyDashboardSummarizesManagementView()
{
    var source = new TrackingScenarioWorkspaceDataSource(SeedData.Create());
    var service = new ProductFamilyDashboardService(source);

    var result = service.GetBaseline(12);

    AssertTrue(source.LoadCount == 1, "product family dashboard should read through IScenarioWorkspaceDataSource");
    AssertTrue(result.Summaries.Count > 0, "dashboard should summarize product families");
    AssertTrue(result.WeeklyCells.Count == result.Summaries.Count * 12, "family weekly grid should cover every family and week");
    AssertTrue(result.Summaries.All(item => item.SkuCount > 0), "family summaries should expose SKU count");
    AssertTrue(result.Summaries.Any(item => item.AverageInventoryValue > 0), "family summaries should expose inventory value");
    AssertTrue(result.Summaries.Any(item => item.ReplenishmentOrderCount > 0), "family summaries should expose replenishment orders");
    AssertTrue(result.Summaries.All(item => item.TargetServiceLevel > 0 && item.TargetFlowIndex > 0), "family summaries should expose service and flow targets");
    AssertTrue(result.Details.Any(item => item.RiskItems.Count > 0), "family details should expose risk items");
    AssertTrue(result.Details.Any(item => item.Recommendations.Count > 0), "family details should expose action recommendations");
    AssertTrue(result.Details.Any(item => item.RccpContributions.Count > 0), "family details should expose RCCP contributions");
    AssertTrue(result.Details.Any(item => item.SupplierRequirements.Count > 0), "family details should expose supplier requirements");
    AssertTrue(result.Summaries.Any(item => item.Family == result.SelectedFamily), "selected family should exist in summaries");
}

static void TestScenarioPreviewReturnsProductFamilyDashboardComparison()
{
    var service = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));

    var result = service.Preview(new ScenarioRunPreviewRequest(
        12,
        "TPL-PREBUILD-PEAK",
        Parameters: new ScenarioRunParameterSet(
            PrebuildCampaigns: new[] { new PrebuildCampaign("PB-FAMILY", "AV-FPGA-203", 1, 6, 8, 300) },
            SupplierCapacityLimits: new[] { new SupplierCapacityLimit("Microchip Space", "进口空间级 FPGA", 1, 12, 1) })));

    AssertEqual("baseline", result.Baseline.ProductFamilyDashboard.CaseId, "baseline family dashboard case id");
    AssertEqual("scenario", result.Scenario.ProductFamilyDashboard.CaseId, "scenario family dashboard case id");
    AssertTrue(result.Baseline.ProductFamilyDashboard.Summaries.Count > 0, "baseline should include family summaries");
    AssertTrue(result.Scenario.ProductFamilyDashboard.Summaries.Count > 0, "scenario should include family summaries");
    AssertTrue(result.Scenario.ProductFamilyDashboard.Comparison.SupplyGapDelta != 0m || result.Scenario.ProductFamilyDashboard.Comparison.AverageInventoryValueDelta != 0m, "scenario family dashboard should include comparison deltas");
    AssertTrue(result.Scenario.ProductFamilyDashboard.Details.Any(item => item.RiskItems.Any()), "scenario family dashboard should expose family risks");
}

static void TestNetworkStructureScoringCreatesWhiteBoxCandidates()
{
    var source = new TrackingScenarioWorkspaceDataSource(SeedData.Create());
    var service = new NetworkStructureScoringService(source, new NetworkMetricsService(source));

    var result = service.GetBaseline(12);
    var settingTypes = result.Candidates.Select(item => item.RecommendedSettingType).ToHashSet(StringComparer.Ordinal);

    AssertTrue(source.LoadCount >= 1, "network structure scoring should read through INetworkStructureDataSource");
    AssertEqual("NetworkScore-V2", result.ModelVersion, "network scoring model version");
    AssertTrue(result.Candidates.Count >= result.HorizonWeeks, "network scoring should return a broad candidate set");
    AssertTrue(settingTypes.Contains("库存缓冲"), "network scoring should include inventory buffer candidates");
    AssertTrue(settingTypes.Contains("时间缓冲"), "network scoring should include time buffer candidates");
    AssertTrue(settingTypes.Contains("能力缓冲"), "network scoring should include capacity buffer candidates");
    AssertTrue(settingTypes.Contains("解耦点"), "network scoring should include decoupling point candidates");
    AssertTrue(result.Candidates.Any(item => item.CandidateId.StartsWith("NET-", StringComparison.Ordinal) && item.TargetType == "物料节点"), "V2 scoring should include material graph candidates");
    AssertTrue(result.Candidates.Any(item => item.TargetType == "SKU" && item.Evidence.Any(evidence => evidence.Contains("工艺路线", StringComparison.Ordinal))), "SKU candidates should explain routing evidence in Chinese business terms");
    AssertTrue(result.Candidates.Any(item => item.Evidence.Any(evidence => evidence.Contains("BOM 行", StringComparison.Ordinal))), "V2 scoring should include traceable BOM line evidence in Chinese business terms");
    AssertTrue(result.Candidates.Any(item => item.QuantityImpactScore > 0m), "V2 scoring should include quantity impact score from Phase 4 metrics");
    AssertTrue(result.Candidates.All(item => !string.IsNullOrWhiteSpace(item.Rationale)), "candidates should include white-box explanation");
    AssertTrue(result.Candidates.All(item => !string.IsNullOrWhiteSpace(item.NotAdoptingRisk)), "candidates should include non-adoption risk");
    AssertTrue(result.Candidates.Any(item => item.SupplyRiskScore > 0), "network scoring should include supply risk score");
    AssertTrue(result.Candidates.Any(item => item.ResourceConstraintScore > 0), "network scoring should include resource constraint score");
    AssertTrue(result.FactorWeights.Any(item => item.Factor == "下游覆盖度"), "network scoring should expose downstream coverage factor weight");
    AssertTrue(result.Recommendations.Count > 0, "network scoring should return management recommendations");
}

static void TestNetworkMetricsServiceCalculatesTraceableMetrics()
{
    var source = new TrackingScenarioWorkspaceDataSource(SeedData.Create());
    var service = new NetworkMetricsService(source);

    var result = service.GetBaseline(12);
    var fpga = result.ItemMetrics.FirstOrDefault(item => item.ItemCode == "PART-FPGA-SPACE");

    AssertEqual(1, source.LoadCount, "network metrics should read through INetworkStructureDataSource");
    AssertEqual("NetworkMetrics-V1", result.ModelVersion, "network metrics model version");
    AssertTrue(result.ItemMetrics.Count > 0, "network metrics should return item metrics");
    AssertTrue(fpga is not null, "network metrics should include imported space FPGA");
    AssertTrue(fpga!.DownstreamCoverageScore > 0m, "downstream coverage should be scored");
    AssertTrue(fpga.QuantityImpactScore > 0m, "quantity impact should be scored");
    AssertTrue(fpga.CumulativeLeadTimeScore > 0m, "cumulative lead time should be scored");
    AssertTrue(fpga.SupplyRiskScore > 0m, "supply risk should be scored");
    AssertTrue(fpga.ResourceConstraintScore > 0m, "resource constraint should be scored");
    AssertTrue(fpga.InventoryCostScore > 0m, "inventory cost should be scored");
    AssertTrue(fpga.DownstreamCoverage.Evidence.Any(item => item.EvidenceType == "BomLine" && item.EvidenceKey.Contains("|", StringComparison.Ordinal)), "downstream coverage should trace to BOM lines");
    AssertTrue(fpga.QuantityImpact.Evidence.Any(item => item.EvidenceType == "BomLine" && item.EvidenceKey.Contains("BOM-", StringComparison.Ordinal)), "quantity impact should trace to BOM lines");
    AssertTrue(fpga.CumulativeLeadTime.Evidence.Any(item => item.EvidenceType is "SupplierSource" or "LeadTimeProfile" or "BufferSetting"), "lead time should trace to supplier, lead time profile, or buffer setting");
    AssertTrue(fpga.SupplyRisk.Evidence.Any(item => item.EvidenceType == "SupplierSource" && item.EvidenceKey.Contains("PART-FPGA-SPACE", StringComparison.Ordinal)), "supply risk should trace to supplier source");
    AssertTrue(result.ItemMetrics.Any(item => item.ResourceConstraint.Evidence.Any(evidence => evidence.EvidenceType == "RoutingLine" && evidence.EvidenceKey.Split('|').Length == 4)), "resource constraint should trace to routing line keys");
    AssertTrue(result.ItemMetrics.Any(item => item.InventoryCost.Evidence.Any(evidence => evidence.EvidenceType is "BufferSetting" or "InventoryLocation")), "inventory cost should trace to buffer setting or inventory location");
}

static void TestScenarioRunWorkspaceExposesNetworkScoringUi()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var indexPage = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var networkPartial = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure.Web", "Pages", "Shared", "_NetworkStructureWorkspace.cshtml"));
    var standalonePage = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure.Web", "Pages", "NetworkStructure.cshtml"));
    var networkProductPage = standalonePage + networkPartial;
    var appScript = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));
    var networkScript = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure.Web", "wwwroot", "js", "network-structure-workspace.js"));
    var indexModel = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml.cs"));
    var script = networkScript;

    AssertTrue(indexPage.Contains("href=\"@Model.NetworkStructureProductUrl\"", StringComparison.Ordinal), "homepage should link to the configured independent network structure workspace");
    AssertTrue(indexModel.Contains("NetworkStructure:ProductUrl", StringComparison.Ordinal), "homepage model should read network product URL from configuration");
    AssertTrue(!indexPage.Contains("<partial name=\"_NetworkStructureWorkspace\" />", StringComparison.Ordinal), "homepage should not mount network structure workspace");
    AssertTrue(!indexPage.Contains("id=\"network-score-candidate-body\"", StringComparison.Ordinal), "homepage should not inline network structure workspace details");
    AssertTrue(networkProductPage.Contains("id=\"network-structure-scoring-panel\"", StringComparison.Ordinal), "network product should expose network structure scoring panel");
    AssertTrue(networkProductPage.Contains("id=\"network-scoring-kpis\"", StringComparison.Ordinal), "network product should expose network scoring KPIs");
    AssertTrue(networkProductPage.Contains("id=\"network-score-candidate-body\"", StringComparison.Ordinal), "network product should expose network scoring candidate table");
    AssertTrue(networkProductPage.Contains("控制点 / 缓冲点候选", StringComparison.Ordinal), "network product should expose Chinese control point candidate label");
    AssertTrue(indexPage.Contains("id=\"network-structure-entry-card\"", StringComparison.Ordinal), "homepage should expose network scoring as an overview entry card");
    AssertTrue(!indexPage.Contains("href=\"#network-structure-scoring-panel\"", StringComparison.Ordinal), "homepage navigation should not expose network scoring as an in-flow section");
    AssertTrue(!appScript.Contains("NetworkStructureProductWorkspace?.initialize()", StringComparison.Ordinal), "main app shell should not initialize network structure workspace");
    AssertTrue(!appScript.Contains("DdaeNetworkStructureWorkspace", StringComparison.Ordinal), "main app shell should not reference old product-family-specific network globals");
    AssertTrue(!appScript.Contains("function renderNetworkStructureScoring", StringComparison.Ordinal), "main app shell should not own network structure scoring renderer");
    AssertTrue(networkScript.Contains("function renderNetworkStructureScoring", StringComparison.Ordinal), "network structure script should own scoring renderer");
    AssertTrue(networkScript.Contains("/api/network-structure-scoring?", StringComparison.Ordinal), "network module should fetch network structure scoring data");
    AssertTrue(script.Contains("renderNetworkStructureScoring", StringComparison.Ordinal), "script should render network structure scoring");
    AssertTrue(script.Contains("data-network-candidate", StringComparison.Ordinal), "script should switch selected network scoring candidate");
    AssertTrue(script.Contains("不采纳风险", StringComparison.Ordinal), "script should render non-adoption risk for network candidates");
    AssertTrue(script.Contains("candidate.notAdoptingRisk", StringComparison.Ordinal), "script should consume non-adoption risk from scoring API");
    AssertTrue(networkProductPage.Contains("网络指标计算", StringComparison.Ordinal), "network product should expose Phase 4 network metrics label");
    AssertTrue(networkProductPage.Contains("下游覆盖度", StringComparison.Ordinal), "network product should expose downstream coverage label");
    AssertTrue(networkProductPage.Contains("数量影响度", StringComparison.Ordinal), "network product should expose quantity impact label");
    AssertTrue(networkProductPage.Contains("累计提前期", StringComparison.Ordinal), "network product should expose cumulative lead time label");
    AssertTrue(networkProductPage.Contains("供应风险", StringComparison.Ordinal), "network product should expose supply risk label");
    AssertTrue(networkProductPage.Contains("资源约束", StringComparison.Ordinal), "network product should expose resource constraint label");
    AssertTrue(networkProductPage.Contains("库存代价", StringComparison.Ordinal), "network product should expose inventory cost label");
    AssertTrue(networkProductPage.Contains("证据链", StringComparison.Ordinal), "network product should expose evidence chain label");
    AssertTrue(networkScript.Contains("/api/network-metrics?", StringComparison.Ordinal), "network module should fetch network metrics");
    AssertTrue(script.Contains("renderNetworkMetrics", StringComparison.Ordinal), "script should render network metrics");
    AssertTrue(script.Contains("data-network-metric-item", StringComparison.Ordinal), "script should switch selected network metric item");
}

static void TestNetworkGraphServiceExpandsImpactScope()
{
    var source = new TrackingScenarioWorkspaceDataSource(SeedData.Create());
    var service = new NetworkGraphService(source);

    var fpga = service.GetGraph("PART-FPGA-SPACE", 6);
    var platform = service.GetGraph("SAT-BUS-001", 6);
    var limited = service.GetGraph("SAT-BUS-001", 1);
    var cumulative = new NetworkGraphService(new NetworkGraphFixtureDataSource(BuildCumulativeNetworkData()))
        .GetGraph("FG-A", 6);

    AssertEqual(3, source.LoadCount, "network graph should read through INetworkStructureDataSource");
    AssertEqual("PART-FPGA-SPACE", fpga.SelectedItemCode, "selected network graph item");
    AssertTrue(fpga.Downstream.Paths.Any(item => item.ItemCodes.Contains("SUB-AVIONICS-COMPUTE") && item.ItemCodes.Contains("SAT-BUS-001")), "FPGA downstream should reach satellite platform finished goods");
    AssertTrue(fpga.Downstream.Paths.Any(item => item.ItemCodes.Contains("SUB-PAYLOAD-EO-FOCAL")), "FPGA downstream should reach payload subassemblies");
    AssertTrue(platform.Upstream.Paths.Any(item => item.ItemCodes.Contains("SUB-SAT-BUS-CORE") && item.ItemCodes.Contains("PART-FPGA-SPACE")), "platform upstream should reach avionics compute and FPGA");
    AssertTrue(platform.Upstream.Paths.Any(item => item.ItemCodes.Contains("SUB-HARNESS-TESTED") && item.ItemCodes.Contains("PART-CONNECTOR")), "platform upstream should reach harness and connector chain");
    AssertTrue(limited.Upstream.Paths.All(item => item.Depth <= 1), "max depth should limit traversal");

    var leaf = cumulative.Upstream.Paths.Single(item => item.LeafItemCode == "PART-C");
    AssertTrue(Math.Abs(leaf.CumulativeQuantity - 7.92m) < 0.001m, "cumulative quantity should multiply quantity and scrap across levels");
}

static void TestNetworkGraphServiceReportsValidationIssues()
{
    var source = new NetworkGraphFixtureDataSource(BuildInvalidNetworkData());
    var service = new NetworkGraphService(source);

    var result = service.GetGraph("FG-A", 6);
    var rules = result.ValidationReport.Issues.Select(item => item.RuleCode).ToHashSet(StringComparer.Ordinal);

    AssertEqual(1, source.LoadCount, "validation graph should read through data source");
    AssertTrue(result.ValidationReport.RedCount >= 3, "invalid network should produce red validation issues");
    AssertTrue(rules.Contains("MissingBomComponent"), "validation should report missing BOM component");
    AssertTrue(rules.Contains("MissingBomParent"), "validation should report missing BOM parent");
    AssertTrue(rules.Contains("MissingBufferItem"), "validation should report missing buffer item");
    AssertTrue(rules.Contains("BomCycle"), "validation should report BOM cycles");
    AssertTrue(rules.Contains("PurchasedPartWithoutSupplier"), "validation should report purchased part without supplier");
    AssertTrue(rules.Contains("ItemWithoutRouting"), "validation should report item without routing");
    AssertTrue(rules.Contains("BufferWithoutExecutableLocation"), "validation should report decoupling point without executable inventory location");
    AssertTrue(rules.Contains("MissingAlternateItem"), "validation should report missing alternate item");
    AssertTrue(rules.Contains("BomLineWithoutHeader"), "validation should report BOM line without header");
    AssertTrue(result.Upstream.Paths.Count > 0, "invalid network traversal should still return bounded paths");
}

static void TestScenarioRunWorkspaceExposesNetworkGraphUi()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure.Web", "Pages", "NetworkStructure.cshtml"))
        + File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure.Web", "Pages", "Shared", "_NetworkStructureWorkspace.cshtml"));
    var appScript = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));
    var networkScript = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure.Web", "wwwroot", "js", "network-structure-workspace.js"));

    AssertTrue(page.Contains("物料网络展开", StringComparison.Ordinal), "network product should expose material network expansion label");
    AssertTrue(page.Contains("上游影响", StringComparison.Ordinal), "network product should expose upstream impact label");
    AssertTrue(page.Contains("下游影响", StringComparison.Ordinal), "network product should expose downstream impact label");
    AssertTrue(page.Contains("校验报告", StringComparison.Ordinal), "network product should expose validation report label");
    AssertTrue(page.Contains("物料关系图", StringComparison.Ordinal), "network product should expose material relationship graph label");
    AssertTrue(page.Contains("只看风险节点", StringComparison.Ordinal), "network product should expose risk-only graph filter");
    AssertTrue(page.Contains("id=\"network-graph-item-select\"", StringComparison.Ordinal), "network product should expose network graph item selector");
    AssertTrue(page.Contains("id=\"network-graph-map\"", StringComparison.Ordinal), "network product should expose network graph map container");
    AssertTrue(!appScript.Contains("function renderNetworkGraph", StringComparison.Ordinal), "main app shell should not own network graph renderer");
    AssertTrue(networkScript.Contains("function renderNetworkGraph", StringComparison.Ordinal), "network structure script should own network graph renderer");
    AssertTrue(!appScript.Contains("/api/network-graph", StringComparison.Ordinal), "main app shell should not call network graph API");
    AssertTrue(networkScript.Contains("/api/network-graph", StringComparison.Ordinal), "network module should call network graph API");
    AssertTrue(networkScript.Contains("renderNetworkGraph", StringComparison.Ordinal), "network module should render network graph");
    AssertTrue(networkScript.Contains("renderNetworkGraphMap", StringComparison.Ordinal), "network module should render local material relationship graph");
    AssertTrue(networkScript.Contains("network-graph-item-select", StringComparison.Ordinal), "network module should refresh graph from item selector");
    AssertTrue(networkScript.Contains("validationRuleName(issue.ruleCode)", StringComparison.Ordinal), "network module should translate validation rule codes before display");
    AssertTrue(networkScript.Contains("evidenceTypeName(item.evidenceType)", StringComparison.Ordinal), "network module should translate metric evidence type before display");
}

static void TestNetworkScenarioValidationOutputsCandidateImpactDeltas()
{
    var source = new TrackingScenarioWorkspaceDataSource(SeedData.Create());
    var scoring = new NetworkStructureScoringService(source, new NetworkMetricsService(source));
    var preview = new ScenarioRunPreviewService(source);
    var whiteBoxGateway = new LocalDdsopWhiteBoxScenarioGateway(preview);
    var requestBuilder = new NetworkCandidateRecalculationRequestBuilder();
    var service = new NetworkScenarioValidationService(source, source, scoring, requestBuilder, whiteBoxGateway);

    var result = service.Validate(12);

    AssertTrue(source.LoadCount > 0, "network scenario validation should read through IScenarioWorkspaceDataSource");
    AssertEqual("NetworkScenarioValidation-V1", result.ModelVersion, "network scenario validation model version");
    AssertTrue(result.Validations.Count > 0, "network scenario validation should return candidate validations");
    AssertTrue(result.Validations.All(item => !string.IsNullOrWhiteSpace(item.CandidateId)), "validation item should keep candidate id");
    AssertTrue(result.Validations.All(item => !string.IsNullOrWhiteSpace(item.ValidationSummary)), "validation item should include summary");
    AssertTrue(result.Validations.All(item => item.Evidence.Any(evidence => evidence.Contains("库存金额变化", StringComparison.Ordinal))), "validation evidence should include inventory value delta");
    AssertTrue(result.Validations.All(item => item.Evidence.Any(evidence => evidence.Contains("红区周变化", StringComparison.Ordinal))), "validation evidence should include red week delta");
    AssertTrue(result.Validations.All(item => item.Evidence.Any(evidence => evidence.Contains("补货订单变化", StringComparison.Ordinal))), "validation evidence should include replenishment order delta");
    AssertTrue(result.Validations.All(item => item.Evidence.Any(evidence => evidence.Contains("RCCP 峰值变化", StringComparison.Ordinal))), "validation evidence should include RCCP delta");
    AssertTrue(result.Validations.All(item => item.Evidence.Any(evidence => evidence.Contains("供应缺口变化", StringComparison.Ordinal))), "validation evidence should include supply gap delta");
    AssertTrue(result.Validations.Any(item =>
        item.AverageInventoryValueDelta != 0m
        || item.RedWeekDelta != 0
        || item.ReplenishmentOrderCountDelta != 0
        || item.RccpPeakLoadDelta != 0m
        || item.SupplyGapDelta != 0m), "at least one validation should change a scenario KPI");
}

static void TestScenarioRunWorkspaceExposesNetworkScenarioValidationUi()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure.Web", "Pages", "NetworkStructure.cshtml"))
        + File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure.Web", "Pages", "Shared", "_NetworkStructureWorkspace.cshtml"));
    var appScript = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));
    var networkScript = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure.Web", "wwwroot", "js", "network-structure-workspace.js"));

    AssertTrue(page.Contains("场景验证", StringComparison.Ordinal), "network product should expose network scenario validation label");
    AssertTrue(page.Contains("id=\"network-scenario-validation-body\"", StringComparison.Ordinal), "network product should expose network scenario validation table");
    AssertTrue(page.Contains("库存金额变化", StringComparison.Ordinal), "network product should expose inventory value delta label");
    AssertTrue(page.Contains("红区周变化", StringComparison.Ordinal), "network product should expose red week delta label");
    AssertTrue(page.Contains("补货订单变化", StringComparison.Ordinal), "network product should expose replenishment order delta label");
    AssertTrue(page.Contains("RCCP 峰值变化", StringComparison.Ordinal), "network product should expose RCCP delta label");
    AssertTrue(page.Contains("供应缺口变化", StringComparison.Ordinal), "network product should expose supply gap delta label");
    AssertTrue(!appScript.Contains("function renderNetworkScenarioValidation", StringComparison.Ordinal), "main app shell should not own network scenario validation renderer");
    AssertTrue(networkScript.Contains("function renderNetworkScenarioValidation", StringComparison.Ordinal), "network structure script should own scenario validation renderer");
    AssertTrue(networkScript.Contains("/api/network-scenario-validation?", StringComparison.Ordinal), "network module should call network scenario validation API");
    AssertTrue(networkScript.Contains("renderNetworkScenarioValidation", StringComparison.Ordinal), "network module should render network scenario validation");
}

static void TestScenarioRunWorkspaceScriptFetchesWorkspaceData()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var appScript = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));
    var networkScript = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure.Web", "wwwroot", "js", "network-structure-workspace.js"));
    var script = appScript + networkScript;

    AssertTrue(script.Contains("/api/scenario-workspace-data?horizonWeeks=12", StringComparison.Ordinal), "script should fetch scenario workspace data");
    AssertTrue(script.Contains("/api/product-family-dashboard?horizonWeeks=12", StringComparison.Ordinal), "script should fetch product family dashboard data");
    AssertTrue(!appScript.Contains("/api/network-structure-scoring", StringComparison.Ordinal), "DDS&OP script should not fetch network structure scoring data");
    AssertTrue(!appScript.Contains("/api/network-graph", StringComparison.Ordinal), "DDS&OP script should not fetch material network graph data");
    AssertTrue(!appScript.Contains("/api/network-scenario-validation", StringComparison.Ordinal), "DDS&OP script should not fetch network scenario validation data");
    AssertTrue(networkScript.Contains("/api/network-structure-scoring?", StringComparison.Ordinal), "network module should fetch network structure scoring data");
    AssertTrue(networkScript.Contains("/api/network-graph", StringComparison.Ordinal), "network module should fetch material network graph data");
    AssertTrue(networkScript.Contains("/api/network-scenario-validation?", StringComparison.Ordinal), "network module should fetch network scenario validation data");
    AssertTrue(!appScript.Contains("NetworkStructureProductWorkspace?.initialize()", StringComparison.Ordinal), "main app shell should not initialize the network structure script");
    AssertTrue(!appScript.Contains("DdaeNetworkStructureWorkspace", StringComparison.Ordinal), "main app shell should not reference old product-family-specific network globals");
    AssertTrue(!appScript.Contains("function loadNetworkGraph", StringComparison.Ordinal), "main app shell should not own network graph loading");
    AssertTrue(networkScript.Contains("function loadNetworkGraph", StringComparison.Ordinal), "network structure script should own network graph loading");
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
    AssertTrue(script.Contains("renderSingleSkuSimulation", StringComparison.Ordinal), "script should render single SKU simulation workspace");
    AssertTrue(script.Contains("single-sku-activity-body", StringComparison.Ordinal), "script should render single SKU activity list");
    AssertTrue(script.Contains("single-sku-sizing-body", StringComparison.Ordinal), "script should render single SKU buffer sizing");
    AssertTrue(script.Contains("single-sku-bom-body", StringComparison.Ordinal), "script should render single SKU BOM");
    AssertTrue(script.Contains("single-sku-order-body", StringComparison.Ordinal), "script should render single SKU order details");
    AssertTrue(script.Contains("data-buffer-family", StringComparison.Ordinal), "script should switch buffer SKU by product family option");
    AssertTrue(script.Contains("data-buffer-sku", StringComparison.Ordinal), "script should switch selected buffer SKU from heatmap");
    AssertTrue(script.Contains("applyExceptionToScenario", StringComparison.Ordinal), "script should bring exception SKU into scenario configuration");
    AssertTrue(script.Contains("previewControls.sku.value", StringComparison.Ordinal), "script should set preview SKU from exception row");
    AssertTrue(script.Contains("selectors.sku.value", StringComparison.Ordinal), "script should synchronize global SKU filter from exception row");
    AssertTrue(script.Contains("previewControls.template.value", StringComparison.Ordinal), "script should set scenario template from exception row");
    AssertTrue(script.Contains("renderSupplierCollaborationWorkspace", StringComparison.Ordinal), "script should render supplier drilldown workspace");
    AssertTrue(script.Contains("data-supplier", StringComparison.Ordinal), "script should switch selected supplier");
    AssertTrue(script.Contains("renderProductFamilyDashboard", StringComparison.Ordinal), "script should render product family dashboard");
    AssertTrue(script.Contains("data-product-family", StringComparison.Ordinal), "script should switch selected product family");
    AssertTrue(script.Contains("data-product-family-reset", StringComparison.Ordinal), "product family dashboard should expose a reset action");
    AssertTrue(!script.Contains("selectors.family.value = state.selectedProductFamily", StringComparison.Ordinal), "product family card click should not hide other family cards by applying the global filter");
    AssertTrue(script.Contains("IntersectionObserver", StringComparison.Ordinal), "left navigation should observe right-side scroll position");
    AssertTrue(script.Contains("setActiveNav", StringComparison.Ordinal), "right-side scroll should update active navigation item");
    AssertTrue(script.Contains("data-family-link-week", StringComparison.Ordinal), "product family detail rows should expose linked week keys");
    AssertTrue(script.Contains("productFamilyLinkMatches", StringComparison.Ordinal), "product family detail should link risk RCCP and supply rows");
    AssertTrue(script.Contains("selectedProductFamilyLink", StringComparison.Ordinal), "product family detail should keep linked row selection");
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

static void TestNetworkStructureAdapterExposesNetworkStructureV2DataModel()
{
    var source = new SeedScenarioWorkspaceDataSource(SeedData.Create());

    var data = new NetworkStructureDataSourceAdapter(source)
        .LoadNetworkStructure(new NetworkStructureDataRequest(12, new DateOnly(2026, 6, 1)));
    var network = data.NetworkData;

    AssertTrue(network.Items.Any(item => item.ItemType == "FinishedGood"), "network data should include finished goods");
    AssertTrue(network.Items.Any(item => item.ItemType == "Subassembly"), "network data should include subassemblies");
    AssertTrue(network.Items.Any(item => item.ItemType == "PurchasedPart"), "network data should include purchased parts");
    AssertTrue(network.Items.Any(item => item.ItemType == "RawMaterial"), "network data should include raw materials");
    AssertTrue(network.BomHeaders.Any(item => item.ReleaseStatus == "Released" && item.EffectiveFrom <= data.Request.AnchorDate), "BOM headers should express released effective versions");
    AssertTrue(network.BomLines.Any(item => item.ParentItemCode == "AV-FPGA-203" && item.ComponentItemCode == "PART-FPGA-SPACE"), "BOM lines should express parent and component items");
    AssertTrue(network.BomLines.Any(item => !string.IsNullOrWhiteSpace(item.AlternateGroup)), "BOM lines should reference alternate groups");
    var bomPairs = network.BomLines
        .Select(item => (item.ParentItemCode, item.ComponentItemCode))
        .ToHashSet();
    AssertTrue(
        bomPairs.Contains(("SAT-BUS-001", "SUB-SAT-BUS-CORE"))
            && bomPairs.Contains(("SUB-SAT-BUS-CORE", "SUB-AVIONICS-BAY"))
            && bomPairs.Contains(("SUB-AVIONICS-BAY", "SUB-AVIONICS-COMPUTE"))
            && bomPairs.Contains(("SUB-AVIONICS-COMPUTE", "PART-FPGA-SPACE")),
        "network seed should expose a 4-level satellite platform to FPGA chain");
    AssertTrue(
        bomPairs.Contains(("CBL-HAR-402", "SUB-HARNESS-TESTED"))
            && bomPairs.Contains(("SUB-HARNESS-TESTED", "SUB-HARNESS-LOOM"))
            && bomPairs.Contains(("SUB-HARNESS-LOOM", "PART-CABLE"))
            && bomPairs.Contains(("SUB-HARNESS-LOOM", "PART-CONNECTOR")),
        "network seed should expose a multi-level harness chain");
    AssertTrue(
        network.BomLines
            .Where(item => item.ComponentItemCode == "PART-FPGA-SPACE")
            .Select(item => item.QuantityPer)
            .Distinct()
            .Count() >= 3,
        "reused FPGA material should have different quantities across product families");
    AssertTrue(
        network.AlternateItems.Any(item => item.AlternateGroup == "ALT-FPGA" && item.AlternateItemCode == "PART-FPGA-DOMESTIC-ALT" && item.QualificationStatus == "EngineeringReview")
            && network.AlternateItems.Any(item => item.AlternateGroup == "ALT-FPGA" && item.AlternateItemCode == "PART-FPGA-HI-REL-ALT" && item.QualificationStatus == "Qualified"),
        "FPGA alternates should model realistic domestic and high-reliability substitutes");
    AssertTrue(network.AlternateItems.Any(item => item.Priority > 1 && !string.IsNullOrWhiteSpace(item.QualificationStatus)), "alternate items should express priority and qualification");
    AssertTrue(network.RoutingLines.Any(item => item.ModelCode == "AV-FPGA-203" && item.ResourceCode == "RES-TVAC"), "routing lines should express model-specific routing");
    AssertTrue(network.RoutingLines.Select(item => item.RoutingVersion).Distinct(StringComparer.Ordinal).Count() >= 2, "routing lines should support multiple routing versions");
    AssertTrue(
        network.RoutingLines
            .Where(item => item.ItemCode == "SUB-AVIONICS-COMPUTE")
            .Select(item => item.ModelCode)
            .Distinct(StringComparer.Ordinal)
            .Count() >= 2,
        "same subassembly should support model-specific routing differences");
    AssertTrue(network.SupplierSources.GroupBy(item => item.ItemCode).Any(group => group.Count() > 1), "supplier sources should allow multiple suppliers for one item");
    AssertEqual(
        100m,
        network.SupplierSources.Where(item => item.ItemCode == "PART-FPGA-SPACE").Sum(item => item.AllocationPercent),
        "FPGA supplier allocation percent");
    AssertTrue(network.SupplierSources.Any(item => item.Priority == 1 && item.LeadTimeDays > 0 && item.LeadTimeVariabilityFactor > 1m && item.CapacityPerWeek > 0), "supplier sources should expose lead time, variability, and capacity");
    AssertTrue(network.InventoryLocations.Any(item => item.ItemCode == "SUB-AVIONICS-BAY" && item.LocationType == "WipSupermarket" && item.IsShared), "inventory locations should express executable WIP buffer positions");
    AssertTrue(network.InventoryLocations.Any(item => item.QualityStatus == "PendingRelease"), "inventory locations should express quality status");
    AssertTrue(network.InventoryLocations.Any(item => item.LocationType == "LineSide"), "inventory locations should include line-side execution positions");
    AssertTrue(network.InventoryLocations.Any(item => item.LocationCode == "WH-MATERIAL-CONTROL" && item.ShelfLifeDays is > 0), "inventory locations should include controlled material storage");
    AssertTrue(network.BufferSettings.Any(item => item.ItemCode == "PART-FPGA-SPACE" && item.IsDecouplingPoint && item.TimeBufferDays > 0), "buffer settings should express material decoupling point and time buffer");
    AssertTrue(network.BufferSettings.All(item => item.OrderCycleDays > 0 && item.MinimumOrderQuantity >= 0), "buffer settings should keep executable MOQ and order cycle");
    var itemCodes = network.Items.Select(item => item.ItemCode).ToHashSet(StringComparer.Ordinal);
    AssertTrue(network.BufferSettings.All(item => itemCodes.Contains(item.ItemCode)), "buffer settings should point only to material item nodes");
    AssertTrue(network.LeadTimeProfiles.Any(item => item.SourceType == "Supplier" && item.StandardLeadTimeDays > 0 && item.AppliesBeforeItemCode.Length > 0), "lead time profiles should support time buffer scoring");
    AssertTrue(network.LeadTimeProfiles.Any(item => item.SourceType == "TimeBuffer" && item.ItemCode == "SUB-PAYLOAD-EO-FOCAL"), "lead time profiles should include material time buffers before control points");
}

static void TestNetworkStructureProductExposesIndependentDataSourceBoundary()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var model = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "ScenarioWorkspaceData.cs"));
    var integrationRoot = Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "NetworkStructureIntegration");
    var integrationContracts = File.ReadAllText(Path.Combine(integrationRoot, "NetworkStructureIntegrationContracts.cs"));
    var globalUsingsPath = Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "GlobalUsings.cs");
    var seedSource = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Data", "SeedScenarioWorkspaceDataSource.cs"));
    var adapter = File.ReadAllText(Path.Combine(integrationRoot, "NetworkStructureDataSourceAdapter.cs"));
    var graphService = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkGraphService.cs"));
    var graphModels = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkGraphModels.cs"));
    var graphAdapter = File.ReadAllText(Path.Combine(integrationRoot, "DdsopNetworkGraphDataSource.cs"));
    var metricsService = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkMetricsService.cs"));
    var metricsModels = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkMetricsModels.cs"));
    var metricsAdapter = File.ReadAllText(Path.Combine(integrationRoot, "DdsopNetworkMetricsDataSource.cs"));
    var scoringService = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkStructureScoringService.cs"));
    var scoringModels = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkScoringModels.cs"));
    var scoringAdapter = File.ReadAllText(Path.Combine(integrationRoot, "DdsopNetworkScoringDataSource.cs"));
    var optimizationModels = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkOptimizationModels.cs"));
    var combinationService = File.ReadAllText(Path.Combine(integrationRoot, "CandidateActionCombinationService.cs"));
    var validationService = File.ReadAllText(Path.Combine(integrationRoot, "NetworkScenarioValidationService.cs"));
    var recalculationBuilder = File.ReadAllText(Path.Combine(integrationRoot, "NetworkCandidateRecalculationRequestBuilder.cs"));
    var whiteBoxGateway = File.ReadAllText(Path.Combine(integrationRoot, "LocalDdsopWhiteBoxScenarioGateway.cs"));
    var httpWhiteBoxGateway = File.ReadAllText(Path.Combine(integrationRoot, "HttpDdsopWhiteBoxScenarioGateway.cs"));
    var integrationModule = File.ReadAllText(Path.Combine(integrationRoot, "NetworkStructureIntegrationModule.cs"));
    var program = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Program.cs"));
    var appsettings = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "appsettings.json"));
    var boundary = File.ReadAllText(Path.Combine(root, "docs", "network-structure-scoring-product-boundary.md"));

    AssertTrue(!model.Contains("public sealed record NetworkStructureDataSet", StringComparison.Ordinal), "scenario workspace data model should not own network structure integration package");
    AssertTrue(Directory.Exists(integrationRoot), "DDS&OP network integration files should live in a dedicated integration boundary folder");
    AssertTrue(integrationContracts.Contains("namespace AdaptiveSopDdsop.Web.NetworkStructureIntegration", StringComparison.Ordinal), "network integration contracts should use a dedicated integration namespace");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "NetworkStructureIntegrationContracts.cs")), "DDS&OP domain folder should not own network integration contracts");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "NetworkStructureDataSourceAdapter.cs")), "DDS&OP domain folder should not own network data source adapter");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "CandidateActionCombinationService.cs")), "DDS&OP domain folder should not own candidate combination integration service");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "NetworkScenarioValidationService.cs")), "DDS&OP domain folder should not own network validation integration service");
    AssertTrue(!model.Contains("using AdaptiveSopDdsop.NetworkStructure", StringComparison.Ordinal), "scenario workspace data model should not reference network structure product namespace");
    AssertTrue(!model.Contains("NetworkDataSet NetworkData", StringComparison.Ordinal), "scenario workspace data model should not carry network data snapshots");
    AssertTrue(!File.Exists(globalUsingsPath), "DDS&OP web project should not expose network structure namespace through global using");
    AssertTrue(!model.Contains("public interface INetworkStructureDataSource", StringComparison.Ordinal), "scenario workspace data model should not own network structure data source interface");
    AssertTrue(!model.Contains("public sealed record NetworkScenarioValidationItem", StringComparison.Ordinal), "scenario workspace data model should not own network scenario validation contract");
    AssertTrue(!model.Contains("public sealed record CandidateActionCombinationRequest", StringComparison.Ordinal), "scenario workspace data model should not own candidate action combination contract");
    AssertTrue(!model.Contains("public sealed record OptimizationCandidateImpact", StringComparison.Ordinal), "scenario workspace data model should not own candidate impact matrix contract");
    AssertTrue(integrationContracts.Contains("public sealed record NetworkStructureDataSet", StringComparison.Ordinal), "network structure integration layer should define a DDS&OP-to-network data package");
    AssertTrue(integrationContracts.Contains("using AdaptiveSopDdsop.NetworkStructure", StringComparison.Ordinal), "network integration contract should explicitly reference network structure product types");
    AssertTrue(integrationContracts.Contains("public sealed record NetworkStructureDataRequest", StringComparison.Ordinal), "network structure integration layer should define its own request model");
    AssertTrue(integrationContracts.Contains("public sealed record NetworkStructureRuntimeSignals", StringComparison.Ordinal), "network structure integration layer should define runtime signal input");
    AssertTrue(integrationContracts.Contains("public interface INetworkStructureDataSource", StringComparison.Ordinal), "network structure integration layer should define a narrow data source interface");
    AssertTrue(integrationContracts.Contains("public interface IDdsopWhiteBoxScenarioGateway", StringComparison.Ordinal), "network structure integration layer should define a DDS&OP white-box gateway contract");
    AssertTrue(integrationContracts.Contains("Recalculate(ScenarioRunPreviewRequest request)", StringComparison.Ordinal), "white-box gateway should expose an explicit recalculation operation");
    AssertTrue(integrationContracts.Contains("public sealed record DdsopWhiteBoxGatewayOptions", StringComparison.Ordinal), "network structure integration layer should define white-box gateway options");
    AssertTrue(integrationContracts.Contains("public sealed record NetworkScenarioValidationItem", StringComparison.Ordinal), "network structure integration layer should own network scenario validation contract");
    AssertTrue(!integrationContracts.Contains("public sealed record CandidateActionCombinationRequest", StringComparison.Ordinal), "DDS&OP integration layer should not own the pure candidate action combination request contract");
    AssertTrue(!integrationContracts.Contains("ScenarioWorkspaceDataRequest", StringComparison.Ordinal), "network structure data source contract should not expose DDS&OP scenario request model");
    AssertTrue(integrationContracts.Contains("outside ScenarioWorkspaceDataSet", StringComparison.Ordinal), "network integration contract should document why it is separate from scenario workspace data");
    AssertTrue(!seedSource.Contains("INetworkStructureDataSource", StringComparison.Ordinal), "seed workspace source should not directly implement the network structure product interface");
    AssertTrue(!seedSource.Contains("LoadNetworkStructure", StringComparison.Ordinal), "seed source should not expose network structure load method directly");
    AssertTrue(adapter.Contains("IScenarioWorkspaceDataSource", StringComparison.Ordinal), "adapter should wrap DDS&OP workspace data source instead of requiring seed source to implement network interface");
    AssertTrue(adapter.Contains("using AdaptiveSopDdsop.NetworkStructure", StringComparison.Ordinal), "adapter should explicitly reference network structure namespace only where needed");
    AssertTrue(adapter.Contains("LoadNetworkStructure", StringComparison.Ordinal), "adapter should expose network structure load method");
    AssertTrue(adapter.Contains("FromScenarioWorkspace", StringComparison.Ordinal), "adapter should map DDS&OP workspace data into network structure product data");
    AssertTrue(graphService.Contains("INetworkGraphDataSource", StringComparison.Ordinal), "network graph core service should depend on graph-specific network data source");
    AssertTrue(graphModels.Contains("public interface INetworkGraphDataSource", StringComparison.Ordinal), "network graph data source contract should live in network structure product");
    AssertTrue(graphAdapter.Contains("DdsopNetworkGraphDataSource", StringComparison.Ordinal), "DDS&OP should provide a network graph data adapter");
    AssertTrue(metricsService.Contains("INetworkMetricsDataSource", StringComparison.Ordinal), "network metrics core service should depend on metrics-specific network data source");
    AssertTrue(metricsModels.Contains("public interface INetworkMetricsDataSource", StringComparison.Ordinal), "network metrics data source contract should live in network structure product");
    AssertTrue(metricsAdapter.Contains("DdsopNetworkMetricsDataSource", StringComparison.Ordinal), "DDS&OP should provide a network metrics data adapter");
    AssertTrue(scoringService.Contains("INetworkScoringDataSource", StringComparison.Ordinal), "network scoring core service should depend on scoring-specific network data source");
    AssertTrue(scoringModels.Contains("public interface INetworkScoringDataSource", StringComparison.Ordinal), "network scoring data source contract should live in network structure product");
    AssertTrue(scoringAdapter.Contains("DdsopNetworkScoringDataSource", StringComparison.Ordinal), "DDS&OP should provide a network scoring data adapter");
    AssertTrue(!graphService.Contains("IScenarioWorkspaceDataSource", StringComparison.Ordinal), "network graph core service should not depend on DDS&OP workspace data source");
    AssertTrue(!graphService.Contains("ScenarioWorkspaceDataRequest", StringComparison.Ordinal), "network graph core service should not depend on DDS&OP request model");
    AssertTrue(!metricsService.Contains("IScenarioWorkspaceDataSource", StringComparison.Ordinal), "network metrics core service should not depend on DDS&OP workspace data source");
    AssertTrue(!metricsService.Contains("DemandDrivenPlanningEngine", StringComparison.Ordinal), "network metrics core service should not call DDS&OP planning engine");
    AssertTrue(!metricsService.Contains("ScenarioWorkspaceDataRequest", StringComparison.Ordinal), "network metrics core service should not depend on DDS&OP request model");
    AssertTrue(!scoringService.Contains("IScenarioWorkspaceDataSource", StringComparison.Ordinal), "network scoring core service should not depend on DDS&OP workspace data source");
    AssertTrue(!scoringService.Contains("DemandDrivenPlanningEngine", StringComparison.Ordinal), "network scoring core service should not call DDS&OP planning engine");
    AssertTrue(!scoringService.Contains("ScenarioWorkspaceDataRequest", StringComparison.Ordinal), "network scoring core service should not depend on DDS&OP request model");
    AssertTrue(optimizationModels.Contains("public sealed class CandidateActionCombinationSelector", StringComparison.Ordinal), "network structure product should own pure candidate action combination selection");
    AssertTrue(optimizationModels.Contains("public sealed record CandidateActionCombinationRequest", StringComparison.Ordinal), "network structure product should own the pure candidate action combination request contract");
    AssertTrue(optimizationModels.Contains("public sealed record OptimizationCandidateImpact", StringComparison.Ordinal), "network structure product should own candidate impact matrix contract");
    AssertTrue(optimizationModels.Contains("CandidateActionSelectionProfile", StringComparison.Ordinal), "network optimization model should expose selection profiles");
    AssertTrue(optimizationModels.Contains("CandidateActionSelectionResult", StringComparison.Ordinal), "network optimization model should expose selection results");
    AssertTrue(!optimizationModels.Contains("ScenarioRunPreview", StringComparison.Ordinal), "network optimization selector should not depend on DDS&OP scenario preview");
    AssertTrue(combinationService.Contains("CandidateActionCombinationSelector", StringComparison.Ordinal), "DDS&OP combination service should delegate pure solver selection to network structure selector");
    AssertTrue(combinationService.Contains("IDdsopWhiteBoxScenarioGateway", StringComparison.Ordinal), "DDS&OP combination service should use DDS&OP white-box gateway");
    AssertTrue(!combinationService.Contains("IOptimizationSolver", StringComparison.Ordinal), "DDS&OP combination service should not directly depend on solver adapters");
    AssertTrue(!combinationService.Contains("ScenarioRunPreviewService", StringComparison.Ordinal), "DDS&OP combination service should not directly depend on concrete preview service");
    AssertTrue(whiteBoxGateway.Contains("ScenarioRunPreviewService", StringComparison.Ordinal), "local white-box gateway should be the only integration adapter that calls concrete preview service");
    AssertTrue(whiteBoxGateway.Contains("public sealed class LocalDdsopWhiteBoxScenarioGateway", StringComparison.Ordinal), "local white-box gateway should have an explicit implementation class");
    AssertTrue(httpWhiteBoxGateway.Contains("public sealed class HttpDdsopWhiteBoxScenarioGateway", StringComparison.Ordinal), "HTTP white-box gateway should exist for cross-process integration");
    AssertTrue(httpWhiteBoxGateway.Contains("HttpClient", StringComparison.Ordinal), "HTTP white-box gateway should call DDS&OP through HttpClient");
    AssertTrue(httpWhiteBoxGateway.Contains("PreviewEndpoint", StringComparison.Ordinal), "HTTP white-box gateway should use configured preview endpoint");
    AssertTrue(httpWhiteBoxGateway.Contains("DDS&OP 白盒重算网关", StringComparison.Ordinal), "HTTP white-box gateway should expose Chinese operational errors");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "NetworkStructureScoringService.cs")), "DDS&OP web domain should not still own network scoring service");
    AssertTrue(validationService.Contains("IScenarioWorkspaceDataSource", StringComparison.Ordinal), "network scenario validation should remain in DDS&OP integration layer");
    AssertTrue(validationService.Contains("IDdsopWhiteBoxScenarioGateway", StringComparison.Ordinal), "network scenario validation should use DDS&OP white-box gateway");
    AssertTrue(!validationService.Contains("ScenarioRunPreviewService", StringComparison.Ordinal), "network scenario validation should not directly depend on concrete preview service");
    AssertTrue(validationService.Contains("NetworkCandidateRecalculationRequestBuilder", StringComparison.Ordinal), "network scenario validation should delegate candidate-to-preview request construction");
    AssertTrue(!validationService.Contains("BuildPreviewRequest", StringComparison.Ordinal), "network scenario validation should not own preview request construction details");
    AssertTrue(!validationService.Contains("TraverseParents", StringComparison.Ordinal), "network scenario validation should not own network traversal helper logic");
    AssertTrue(recalculationBuilder.Contains("ScenarioRunPreviewRequest", StringComparison.Ordinal), "candidate recalculation builder should produce DDS&OP preview requests");
    AssertTrue(recalculationBuilder.Contains("NetworkStructureCandidate", StringComparison.Ordinal), "candidate recalculation builder should consume network structure candidates");
    AssertTrue(recalculationBuilder.Contains("TraverseParents", StringComparison.Ordinal), "candidate recalculation builder should own downstream candidate resolution traversal");
    AssertTrue(integrationModule.Contains("AddNetworkStructureIntegration", StringComparison.Ordinal), "network integration module should own network structure service registration");
    AssertTrue(integrationModule.Contains("MapNetworkStructureEndpoints", StringComparison.Ordinal), "network integration module should own network structure endpoint mapping");
    AssertTrue(integrationModule.Contains("INetworkStructureDataSource", StringComparison.Ordinal), "network integration module should register independent network structure data source");
    AssertTrue(integrationModule.Contains("NetworkStructureDataSourceAdapter", StringComparison.Ordinal), "network integration module should register an explicit DDS&OP-to-network adapter");
    AssertTrue(integrationModule.Contains("NetworkCandidateRecalculationRequestBuilder", StringComparison.Ordinal), "network integration module should register candidate recalculation request builder");
    AssertTrue(integrationModule.Contains("IDdsopWhiteBoxScenarioGateway", StringComparison.Ordinal), "network integration module should register white-box gateway interface");
    AssertTrue(integrationModule.Contains("LocalDdsopWhiteBoxScenarioGateway", StringComparison.Ordinal), "network integration module should register local DDS&OP white-box gateway implementation");
    AssertTrue(integrationModule.Contains("HttpDdsopWhiteBoxScenarioGateway", StringComparison.Ordinal), "network integration module should support HTTP white-box gateway implementation");
    AssertTrue(integrationModule.Contains("NetworkStructure:WhiteBoxGateway", StringComparison.Ordinal), "network integration module should read white-box gateway configuration");
    AssertTrue(integrationModule.Contains("Mode", StringComparison.Ordinal), "network integration module should switch gateway mode from configuration");
    AssertTrue(integrationModule.Contains("BaseUrl is required", StringComparison.Ordinal), "HTTP white-box gateway should require a base URL");
    AssertTrue(program.Contains("AddNetworkStructureIntegration(builder.Configuration)", StringComparison.Ordinal), "Program should pass configuration into network structure integration module");
    AssertTrue(appsettings.Contains("\"WhiteBoxGateway\"", StringComparison.Ordinal), "appsettings should expose white-box gateway configuration");
    AssertTrue(appsettings.Contains("\"Mode\": \"Local\"", StringComparison.Ordinal), "default white-box gateway mode should remain local");
    AssertTrue(appsettings.Contains("\"PreviewEndpoint\": \"/api/scenario-runs/preview\"", StringComparison.Ordinal), "appsettings should expose preview endpoint for HTTP gateway");
    AssertTrue(!integrationModule.Contains("(INetworkStructureDataSource)", StringComparison.Ordinal), "network integration module should not cast DDS&OP data source into network product interface");
    AssertTrue(integrationModule.Contains("/api/network-structure-capabilities", StringComparison.Ordinal), "network integration module should expose product capability API");
    AssertTrue(integrationModule.Contains("NetworkStructureProductCapabilityCatalog.CreateStandaloneHost()", StringComparison.Ordinal), "network integration module should reuse network product capability catalog");
    AssertTrue(integrationModule.Contains("/api/network-structure-data", StringComparison.Ordinal), "network integration module should expose independent network structure data API");
    AssertTrue(integrationModule.Contains("/api/candidate-action-combinations/select", StringComparison.Ordinal), "network integration module should expose candidate action combination API");
    AssertTrue(program.Contains("AddNetworkStructureIntegration", StringComparison.Ordinal), "Program should mount network structure integration module");
    AssertTrue(program.Contains("MapNetworkStructureEndpoints", StringComparison.Ordinal), "Program should map network structure endpoint module");
    AssertTrue(!program.Contains("/api/network-structure-data", StringComparison.Ordinal), "Program should not directly map network structure data API");
    AssertTrue(boundary.Contains("网络结构评分产品拆分边界", StringComparison.Ordinal), "boundary document should describe product split");
    AssertTrue(boundary.Contains("NetworkStructure.Core", StringComparison.Ordinal), "boundary document should describe target core package");
    AssertTrue(boundary.Contains("Ddsop.Integration.NetworkStructure", StringComparison.Ordinal), "boundary document should separate DDS&OP integration layer");

    var source = new SeedScenarioWorkspaceDataSource(SeedData.Create());
    INetworkStructureDataSource networkSource = new NetworkStructureDataSourceAdapter(source);
    var request = new NetworkStructureDataRequest(12, new DateOnly(2026, 6, 1));
    var networkData = networkSource.LoadNetworkStructure(request);
    var workspace = source.Load(new ScenarioWorkspaceDataRequest(request.HorizonWeeks, request.AnchorDate));
    var mapped = NetworkStructureDataSourceAdapter.FromScenarioWorkspace(workspace);

    AssertEqual(request.HorizonWeeks, networkData.Request.HorizonWeeks, "network structure request horizon");
    AssertTrue(networkData.NetworkData.Items.Count > 0, "network structure data should include item network");
    AssertTrue(networkData.RuntimeSignals.Skus.Count > 0, "network structure runtime signals should include SKU demand signals");
    AssertTrue(networkData.RuntimeSignals.ResourceRoutings.Count > 0, "network structure runtime signals should include resource route signals");
    AssertTrue(networkData.RuntimeSignals.SupplierCapacityWindows.Count > 0, "network structure runtime signals should include supplier capacity signals");
    AssertTrue(!workspace.GetType().GetProperties().Any(property => property.Name == "NetworkData"), "scenario workspace dataset should not expose network data property");
    AssertEqual(networkData.NetworkData.Items.Count, mapped.NetworkData.Items.Count, "adapter should build stable network item count from DDS&OP runtime signals");
    AssertEqual(workspace.Skus.Count, mapped.RuntimeSignals.Skus.Count, "adapter should preserve SKU runtime signal count");
}

static void TestNetworkStructureProductOwnsPureNetworkDataContracts()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var solution = File.ReadAllText(Path.Combine(root, "AdaptiveSopDdsop.sln"));
    var webProject = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "AdaptiveSopDdsop.Web.csproj"));
    var testsProject = File.ReadAllText(Path.Combine(root, "tests", "AdaptiveSopDdsop.Tests", "AdaptiveSopDdsop.Tests.csproj"));
    var webModels = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "Models.cs"));
    var coreProject = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "AdaptiveSopDdsop.NetworkStructure.csproj"));
    var coreModels = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkDataSet.cs"));
    var graphModels = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkGraphModels.cs"));
    var graphService = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkGraphService.cs"));
    var metricsModels = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkMetricsModels.cs"));
    var metricsService = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkMetricsService.cs"));
    var scoringModels = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkScoringModels.cs"));
    var scoringService = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkStructureScoringService.cs"));
    var seedFactory = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "SatelliteManufacturingNetworkSeedData.cs"));
    var seedData = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Data", "SeedData.cs"));
    var adapter = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "NetworkStructureIntegration", "NetworkStructureDataSourceAdapter.cs"));
    var optimizationModels = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "NetworkOptimizationModels.cs"));
    var gurobiSolver = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "GurobiOptimizationSolver.cs"));
    var orToolsSolver = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure", "OrToolsOptimizationSolver.cs"));
    var webGlobalUsingsPath = Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "GlobalUsings.cs");
    var networkCoreText = string.Join(
        Environment.NewLine,
        Directory.GetFiles(Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure"), "*.cs")
            .Select(File.ReadAllText));

    AssertTrue(solution.Contains("AdaptiveSopDdsop.NetworkStructure", StringComparison.Ordinal), "solution should include independent network structure project");
    AssertTrue(coreProject.Contains("<TargetFramework>net9.0</TargetFramework>", StringComparison.Ordinal), "network structure project should target net9");
    AssertTrue(webProject.Contains("AdaptiveSopDdsop.NetworkStructure.csproj", StringComparison.Ordinal), "DDS&OP web should reference network structure core project");
    AssertTrue(!webProject.Contains("AdaptiveSopDdsop.NetworkStructure.Web.csproj", StringComparison.Ordinal), "DDS&OP web should not physically host the network structure web package");
    AssertTrue(testsProject.Contains("AdaptiveSopDdsop.NetworkStructure.csproj", StringComparison.Ordinal), "tests should reference network structure core project directly");
    AssertTrue(!File.Exists(webGlobalUsingsPath), "DDS&OP web should not import network structure contracts through global using");
    AssertTrue(coreModels.Contains("namespace AdaptiveSopDdsop.NetworkStructure", StringComparison.Ordinal), "network data contracts should live outside web domain namespace");
    AssertTrue(coreModels.Contains("public sealed record NetworkItemMaster", StringComparison.Ordinal), "network structure product should own item master contract");
    AssertTrue(coreModels.Contains("public sealed record NetworkBomHeader", StringComparison.Ordinal), "network structure product should own BOM header contract");
    AssertTrue(coreModels.Contains("public sealed record NetworkBomLine", StringComparison.Ordinal), "network structure product should own BOM line contract");
    AssertTrue(coreModels.Contains("public sealed record NetworkRoutingLine", StringComparison.Ordinal), "network structure product should own routing contract");
    AssertTrue(coreModels.Contains("public sealed record NetworkSupplierSource", StringComparison.Ordinal), "network structure product should own supplier source contract");
    AssertTrue(coreModels.Contains("public sealed record NetworkDataSet", StringComparison.Ordinal), "network structure product should own network dataset contract");
    AssertTrue(coreModels.Contains("public sealed record NetworkStructureProductDataRequest", StringComparison.Ordinal), "network structure product should own pure product data request contract");
    AssertTrue(coreModels.Contains("public sealed record NetworkStructureProductDataSet", StringComparison.Ordinal), "network structure product should own pure product data response contract");
    AssertTrue(coreModels.Contains("public interface INetworkStructureProductDataSource", StringComparison.Ordinal), "network structure product should own pure product data source contract");
    AssertTrue(graphModels.Contains("public sealed record NetworkGraphWorkspaceResult", StringComparison.Ordinal), "network structure product should own graph result contract");
    AssertTrue(graphService.Contains("public sealed class NetworkGraphService", StringComparison.Ordinal), "network structure product should own graph construction service");
    AssertTrue(metricsModels.Contains("public sealed record NetworkMetricsWorkspaceResult", StringComparison.Ordinal), "network structure product should own metrics result contract");
    AssertTrue(metricsService.Contains("public sealed class NetworkMetricsService", StringComparison.Ordinal), "network structure product should own metrics service");
    AssertTrue(scoringModels.Contains("public sealed record NetworkStructureScoringResult", StringComparison.Ordinal), "network structure product should own scoring result contract");
    AssertTrue(scoringService.Contains("public sealed class NetworkStructureScoringService", StringComparison.Ordinal), "network structure product should own scoring service");
    AssertTrue(networkCoreText.Contains("public sealed record NetworkStructureProductCapabilities", StringComparison.Ordinal), "network structure product should own capability response contract");
    AssertTrue(networkCoreText.Contains("public sealed record NetworkStructureCapability", StringComparison.Ordinal), "network structure product should own capability item contract");
    AssertTrue(networkCoreText.Contains("public sealed record NetworkStructureExternalDependency", StringComparison.Ordinal), "network structure product should own external dependency contract");
    AssertTrue(networkCoreText.Contains("public static class NetworkStructureProductCapabilityCatalog", StringComparison.Ordinal), "network structure product should own capability catalog");
    AssertTrue(networkCoreText.Contains("外部白盒场景回算引擎", StringComparison.Ordinal), "network structure product should identify external white-box dependency in the capability catalog");
    AssertTrue(networkCoreText.Contains("本 Host 不生成执行计划", StringComparison.Ordinal), "network structure product should state planning boundary in the capability catalog");
    AssertTrue(seedFactory.Contains("public sealed record NetworkFinishedGoodSeedInput", StringComparison.Ordinal), "network structure product should own seed input contract for demo finished goods");
    AssertTrue(seedFactory.Contains("public static class SatelliteManufacturingNetworkSeedData", StringComparison.Ordinal), "network structure product should own satellite manufacturing network seed factory");
    AssertTrue(seedFactory.Contains("SUB-AVIONICS-COMPUTE", StringComparison.Ordinal), "network seed factory should own satellite subassembly network shape");
    AssertTrue(seedFactory.Contains("PART-FPGA-SPACE", StringComparison.Ordinal), "network seed factory should own critical purchased part network shape");
    AssertTrue(seedFactory.Contains("new List<NetworkBomLine>", StringComparison.Ordinal), "network seed factory should own BOM line construction");
    AssertTrue(seedFactory.Contains("BOM-SUB-AVIONICS-COMPUTE-A", StringComparison.Ordinal), "network seed factory should own multi-level BOM line relationships");
    AssertTrue(!seedFactory.Contains("SkuBufferSetting", StringComparison.Ordinal), "network seed factory should not depend on DDS&OP SKU buffer type");
    AssertTrue(!seedFactory.Contains("AdaptiveSopDdsop.Web", StringComparison.Ordinal), "network seed factory should not depend on DDS&OP web namespace");
    AssertTrue(!seedData.Contains("SatelliteManufacturingNetworkSeedData.Build", StringComparison.Ordinal), "DDS&OP seed should not construct network seed snapshots");
    AssertTrue(!seedData.Contains("NetworkFinishedGoodSeedInput", StringComparison.Ordinal), "DDS&OP seed should not depend on network finished-good seed input contract");
    AssertTrue(adapter.Contains("SatelliteManufacturingNetworkSeedData.Build", StringComparison.Ordinal), "DDS&OP-to-network adapter should delegate network seed construction to network structure product");
    AssertTrue(adapter.Contains("NetworkFinishedGoodSeedInput", StringComparison.Ordinal), "DDS&OP-to-network adapter should pass only finished-good seed input to network structure product");
    AssertTrue(!seedData.Contains("new NetworkItemMaster", StringComparison.Ordinal), "DDS&OP seed should not own network item construction");
    AssertTrue(!seedData.Contains("new NetworkBomHeader", StringComparison.Ordinal), "DDS&OP seed should not own network BOM header construction");
    AssertTrue(!seedData.Contains("new NetworkBomLine", StringComparison.Ordinal), "DDS&OP seed should not own network BOM line construction");
    AssertTrue(!seedData.Contains("PART-FPGA-SPACE", StringComparison.Ordinal), "DDS&OP seed should not own satellite network demo item codes");
    AssertTrue(optimizationModels.Contains("public interface IOptimizationSolver", StringComparison.Ordinal), "network structure product should own solver adapter contract");
    AssertTrue(gurobiSolver.Contains("public sealed class GurobiOptimizationSolver", StringComparison.Ordinal), "network structure product should own Gurobi solver adapter");
    AssertTrue(orToolsSolver.Contains("public sealed class OrToolsOptimizationSolver", StringComparison.Ordinal), "network structure product should own OR-Tools solver adapter");
    AssertTrue(coreProject.Contains("Gurobi.Optimizer", StringComparison.Ordinal), "network structure project should own Gurobi dependency");
    AssertTrue(!webProject.Contains("Gurobi.Optimizer", StringComparison.Ordinal), "DDS&OP web project should not directly reference Gurobi");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "NetworkGraphService.cs")), "DDS&OP web domain should not still own network graph service");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "NetworkMetricsService.cs")), "DDS&OP web domain should not still own network metrics service");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "NetworkStructureScoringService.cs")), "DDS&OP web domain should not still own network scoring service");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "GurobiOptimizationSolver.cs")), "DDS&OP web domain should not still own Gurobi solver adapter");
    AssertTrue(!File.Exists(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "OrToolsOptimizationSolver.cs")), "DDS&OP web domain should not still own OR-Tools solver adapter");
    AssertTrue(!webModels.Contains("public sealed record NetworkItemMaster", StringComparison.Ordinal), "web domain should not still own item master contract");
    AssertTrue(!webModels.Contains("public sealed record NetworkGraphWorkspaceResult", StringComparison.Ordinal), "web domain should not still own graph result contract");
    AssertTrue(!webModels.Contains("public sealed record NetworkMetricsWorkspaceResult", StringComparison.Ordinal), "web domain should not still own metrics result contract");
    AssertTrue(!webModels.Contains("public sealed record NetworkStructureScoringResult", StringComparison.Ordinal), "web domain should not still own scoring result contract");
    AssertTrue(!webModels.Contains("public interface IOptimizationSolver", StringComparison.Ordinal), "web domain should not still own solver adapter contract");
    AssertTrue(!webModels.Contains("public sealed record NetworkBomLine", StringComparison.Ordinal), "web domain should not still own BOM line contract");
    AssertTrue(!webModels.Contains("public sealed record NetworkDataSet", StringComparison.Ordinal), "web domain should not still own network dataset contract");
    AssertTrue(!webModels.Contains("public sealed record NetworkStructureProductDataSet", StringComparison.Ordinal), "web domain should not own pure network product data response contract");
    AssertTrue(!webModels.Contains("public interface INetworkStructureProductDataSource", StringComparison.Ordinal), "web domain should not own pure network product data source contract");
    AssertTrue(!networkCoreText.Contains("DDS&OP", StringComparison.Ordinal), "network structure core should use neutral product language instead of hard-coding DDS&OP");
}

static void TestNetworkStructureProductExposesStandaloneHostBoundary()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var solution = File.ReadAllText(Path.Combine(root, "AdaptiveSopDdsop.sln"));
    var hostProjectPath = Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure.Host", "AdaptiveSopDdsop.NetworkStructure.Host.csproj");
    var hostProgramPath = Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure.Host", "Program.cs");
    var standaloneDataSourcePath = Path.Combine(root, "src", "AdaptiveSopDdsop.NetworkStructure.Host", "StandaloneNetworkStructureDataSource.cs");
    var hostProject = File.ReadAllText(hostProjectPath);
    var hostProgram = File.ReadAllText(hostProgramPath);
    var standaloneDataSource = File.ReadAllText(standaloneDataSourcePath);

    AssertTrue(File.Exists(hostProjectPath), "network structure standalone host project should exist");
    AssertTrue(solution.Contains("AdaptiveSopDdsop.NetworkStructure.Host", StringComparison.Ordinal), "solution should include network structure standalone host");
    AssertTrue(hostProject.Contains("Microsoft.NET.Sdk.Web", StringComparison.Ordinal), "standalone host should be an ASP.NET web host");
    AssertTrue(hostProject.Contains("AdaptiveSopDdsop.NetworkStructure.csproj", StringComparison.Ordinal), "standalone host should reference network structure core");
    AssertTrue(hostProject.Contains("AdaptiveSopDdsop.NetworkStructure.Web.csproj", StringComparison.Ordinal), "standalone host should reference network structure web package");
    AssertTrue(!hostProject.Contains("AdaptiveSopDdsop.Web.csproj", StringComparison.Ordinal), "standalone host should not reference DDS&OP web project");
    AssertTrue(hostProgram.Contains("UseStaticWebAssets", StringComparison.Ordinal), "standalone host should enable Razor class library static web assets");
    AssertTrue(hostProgram.Contains("AddRazorPages", StringComparison.Ordinal), "standalone host should serve network Razor pages");
    AssertTrue(hostProgram.Contains("MapRazorPages", StringComparison.Ordinal), "standalone host should map network Razor pages");
    AssertTrue(hostProgram.Contains("Results.Redirect(\"/network-structure\")", StringComparison.Ordinal), "standalone host root should enter the network structure workspace");
    AssertTrue(hostProgram.Contains("StandaloneNetworkStructureDataSource", StringComparison.Ordinal), "standalone host should use its own network data source");
    AssertTrue(hostProgram.Contains("INetworkStructureProductDataSource", StringComparison.Ordinal), "standalone host should register pure network product data source");
    AssertTrue(hostProgram.Contains("NetworkStructureProductDataRequest", StringComparison.Ordinal), "standalone host should load network data through product data request contract");
    AssertTrue(hostProgram.Contains("/api/network-structure-capabilities", StringComparison.Ordinal), "standalone host should expose product capability API");
    AssertTrue(hostProgram.Contains("NetworkStructureProductCapabilityCatalog.CreateStandaloneHost()", StringComparison.Ordinal), "standalone host should return the core product capability catalog");
    AssertTrue(hostProgram.Contains("NetworkGraphService", StringComparison.Ordinal), "standalone host should register graph service");
    AssertTrue(hostProgram.Contains("NetworkMetricsService", StringComparison.Ordinal), "standalone host should register metrics service");
    AssertTrue(hostProgram.Contains("NetworkStructureScoringService", StringComparison.Ordinal), "standalone host should register scoring service");
    AssertTrue(hostProgram.Contains("/api/network-structure-data", StringComparison.Ordinal), "standalone host should expose pure network data API");
    AssertTrue(hostProgram.Contains("/api/network-structure-scoring", StringComparison.Ordinal), "standalone host should expose network scoring API");
    AssertTrue(hostProgram.Contains("/api/network-metrics", StringComparison.Ordinal), "standalone host should expose network metrics API");
    AssertTrue(hostProgram.Contains("/api/network-graph", StringComparison.Ordinal), "standalone host should expose network graph API");
    AssertTrue(hostProgram.Contains("/api/network-scenario-validation", StringComparison.Ordinal), "standalone host should explicitly expose external validation boundary response");
    AssertTrue(hostProgram.Contains("/api/candidate-action-combinations/select", StringComparison.Ordinal), "standalone host should explicitly expose candidate-combination boundary response");
    AssertTrue(hostProgram.Contains("不执行外部白盒场景重算", StringComparison.Ordinal), "standalone host should state that external white-box recalculation is outside its boundary");
    AssertTrue(hostProgram.Contains("必须回到外部白盒引擎重算", StringComparison.Ordinal), "standalone host should state candidate combinations must return to an external white-box engine");
    AssertTrue(!hostProgram.Contains("DDS&OP", StringComparison.Ordinal), "standalone host program should use neutral external-system boundary language");
    AssertTrue(!hostProgram.Contains("NoDdsopPreview", StringComparison.Ordinal), "standalone host program should not expose Ddsop-specific model names");
    AssertTrue(!hostProgram.Contains("AdaptiveSopDdsop.Web", StringComparison.Ordinal), "standalone host program should not import DDS&OP web namespace");
    AssertTrue(!hostProgram.Contains("ScenarioRunPreviewService", StringComparison.Ordinal), "standalone host should not directly run DDS&OP scenario preview");
    AssertTrue(!hostProgram.Contains("IScenarioWorkspaceDataSource", StringComparison.Ordinal), "standalone host should not depend on DDS&OP workspace data source");
    AssertTrue(!hostProgram.Contains("SeedData.Create", StringComparison.Ordinal), "standalone host should not use DDS&OP seed data");
    AssertTrue(!hostProgram.Contains("DemandDrivenPlanningEngine", StringComparison.Ordinal), "standalone host should not call DDS&OP planning engine");
    AssertTrue(!hostProgram.Contains("AddNetworkStructureIntegration", StringComparison.Ordinal), "standalone host should not mount DDS&OP integration module");
    AssertTrue(!hostProgram.Contains("MapNetworkStructureEndpoints", StringComparison.Ordinal), "standalone host should not mount DDS&OP integration endpoints");
    AssertTrue(standaloneDataSource.Contains("INetworkGraphDataSource", StringComparison.Ordinal), "standalone data source should implement graph data source contract");
    AssertTrue(standaloneDataSource.Contains("INetworkStructureProductDataSource", StringComparison.Ordinal), "standalone data source should implement product data source contract");
    AssertTrue(standaloneDataSource.Contains("NetworkStructureProductDataSet LoadNetworkStructure", StringComparison.Ordinal), "standalone data source should expose pure network product data load method");
    AssertTrue(standaloneDataSource.Contains("INetworkMetricsDataSource", StringComparison.Ordinal), "standalone data source should implement metrics data source contract");
    AssertTrue(standaloneDataSource.Contains("INetworkScoringDataSource", StringComparison.Ordinal), "standalone data source should implement scoring data source contract");
    AssertTrue(standaloneDataSource.Contains("SatelliteManufacturingNetworkSeedData.Build", StringComparison.Ordinal), "standalone data source should use network product seed factory");
    AssertTrue(!standaloneDataSource.Contains("AdaptiveSopDdsop.Web", StringComparison.Ordinal), "standalone data source should not import DDS&OP web namespace");
    AssertTrue(!standaloneDataSource.Contains("ScenarioWorkspaceDataSet", StringComparison.Ordinal), "standalone data source should not return DDS&OP workspace data");
}

static void TestScenarioWorkspaceExposesCompleteDdmrpParameterProfiles()
{
    var source = new SeedScenarioWorkspaceDataSource(SeedData.Create());

    var data = source.Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1)));

    AssertEqual(data.Skus.Count, data.DdmrpParameters.Count, "DDMRP parameter profile count");
    AssertTrue(data.DdmrpParameters.All(item => item.CompletenessStatus == "Complete"), "all DDMRP profiles should be complete");
    AssertTrue(data.DdmrpParameters.All(item => !string.IsNullOrWhiteSpace(item.DecouplingPoint)), "all DDMRP profiles should expose decoupling point");
    AssertTrue(data.DdmrpParameters.All(item => !string.IsNullOrWhiteSpace(item.BufferProfile)), "all DDMRP profiles should expose buffer profile");
    AssertTrue(data.DdmrpParameters.All(item => item.Adu > 0 && item.DecoupledLeadTimeDays > 0), "all DDMRP profiles should expose ADU and DLT");
    AssertTrue(data.DdmrpParameters.All(item => item.DemandAdjustmentFactor > 0 && item.ZoneAdjustmentFactor > 0), "all DDMRP profiles should expose DAF and zone adjustment");
    AssertTrue(data.DdmrpParameters.All(item => item.EffectiveFromWeek >= 1 && item.EffectiveThroughWeek >= item.EffectiveFromWeek), "all DDMRP profiles should expose effective window");
    AssertTrue(data.DdmrpParameters.Any(item => item.DemandAdjustmentFactor != 1m || item.ZoneAdjustmentFactor != 1m), "seed data should include non-default DAF or zone adjustments");

    foreach (var sku in data.Skus)
    {
        var zones = DdmrpCalculator.CalculateZones(sku);
        var profile = data.DdmrpParameters.Single(item => item.Sku == sku.Sku);
        AssertEqual(zones.TopOfRed, profile.TopOfRed, $"top of red for {sku.Sku}");
        AssertEqual(zones.TopOfYellow, profile.TopOfYellow, $"top of yellow for {sku.Sku}");
        AssertEqual(zones.TopOfGreen, profile.TopOfGreen, $"top of green for {sku.Sku}");
    }
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

static void TestScenarioRunPersistenceSavesPreviewResultAndAuditChain()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-scenario-runs-{Guid.NewGuid():N}.db");
    try
    {
        var previewService = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));
        var persistence = new ScenarioRunPersistenceService(previewService, databasePath);
        var previewRequest = new ScenarioRunPreviewRequest(
            12,
            "TPL-PREBUILD-PEAK",
            AdoptionConstraintMode: "FlowFirst",
            Parameters: new ScenarioRunParameterSet(
                PrebuildCampaigns: new[] { new PrebuildCampaign("PB-SAVE", "AV-FPGA-203", 1, 6, 8, 300) }));

        var previewOnly = previewService.Preview(previewRequest);
        AssertTrue(!previewOnly.IsPersisted, "preview API result should remain non-persistent");
        AssertEqual(0, persistence.List(50).Count, "preview should not write scenario run records");

        var saved = persistence.Save(new ScenarioRunSaveRequest("星载电子提前建库", "保存审计测试", "计划员", previewRequest));

        AssertTrue(Guid.TryParseExact(saved.RunId, "N", out _), "saved run should return a GUID run id");
        AssertTrue(saved.RunNumber.StartsWith("SR-", StringComparison.Ordinal), "saved run should return a readable run number");
        AssertEqual("Saved", saved.Status, "scenario status");
        AssertEqual("NotSubmitted", saved.ApprovalStatus, "approval status");
        AssertTrue(saved.IsPersisted, "saved response should mark result as persisted");

        var summaries = persistence.List(50);
        AssertEqual(1, summaries.Count, "saved list count");
        AssertEqual(saved.RunId, summaries[0].RunId, "saved list run id");

        var detail = persistence.GetDetail(saved.RunId);
        AssertTrue(detail is not null, "saved detail should be readable");
        AssertTrue(detail!.Result.IsPersisted, "saved detail result should be persisted");
        AssertEqual("TPL-PREBUILD-PEAK", detail.Request.TemplateId!, "saved request template");
        AssertTrue(detail.Result.Baseline.Plan.BufferProjections.Count > 0, "saved result should include baseline plan");
        AssertTrue(detail.Result.Scenario.Plan.BufferProjections.Count > 0, "saved result should include scenario plan");
        AssertTrue(detail.Result.Trace.Count > 0, "saved result should include preview trace");

        var audit = persistence.GetAuditEvents(saved.RunId);
        AssertEqual(4, audit.Count, "audit event count");
        AssertEqual("RunRequested", audit[0].EventType, "audit event 1");
        AssertEqual("PreviewRecalculated", audit[1].EventType, "audit event 2");
        AssertEqual("TraceCaptured", audit[2].EventType, "audit event 3");
        AssertEqual("RunSaved", audit[3].EventType, "audit event 4");
        AssertTrue(audit.Select(item => item.Sequence).SequenceEqual(new[] { 1, 2, 3, 4 }), "audit sequence should be append-only order");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }
        if (File.Exists($"{databasePath}-wal"))
        {
            File.Delete($"{databasePath}-wal");
        }
        if (File.Exists($"{databasePath}-shm"))
        {
            File.Delete($"{databasePath}-shm");
        }
    }
}

static void TestScenarioRunWorkspaceExposesSaveAuditUi()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    AssertTrue(page.Contains("id=\"scenario-save-panel\"", StringComparison.Ordinal), "page should expose scenario save panel");
    AssertTrue(page.Contains("id=\"save-scenario\"", StringComparison.Ordinal), "page should expose save scenario button");
    AssertTrue(page.Contains("id=\"saved-scenarios-panel\"", StringComparison.Ordinal), "page should expose saved scenarios panel");
    AssertTrue(page.Contains("id=\"scenario-audit-list\"", StringComparison.Ordinal), "page should expose audit chain list");
    AssertTrue(page.Contains("保存场景", StringComparison.Ordinal), "page should use Chinese save label");
    AssertTrue(page.Contains("已保存场景", StringComparison.Ordinal), "page should use Chinese saved scenario label");
    AssertTrue(page.Contains("审计链", StringComparison.Ordinal), "page should use Chinese audit chain label");
    AssertTrue(!page.Contains(">提交审批<", StringComparison.Ordinal), "first persistence version should not expose approval submission button");

    AssertTrue(script.Contains("POST", StringComparison.Ordinal) && script.Contains("/api/scenario-runs", StringComparison.Ordinal), "script should post scenario save request");
    AssertTrue(script.Contains("/api/scenario-runs?limit=50", StringComparison.Ordinal), "script should load saved scenario runs");
    AssertTrue(script.Contains("/audit", StringComparison.Ordinal), "script should load audit chain");
    AssertTrue(script.Contains("已保存，未提交审批", StringComparison.Ordinal), "script should show saved but not submitted status");
    AssertTrue(script.Contains("previewRequest: state.preview.request", StringComparison.Ordinal), "script should save only the preview request");
}

static void TestMasterSettingsGovernanceGeneratesProposalsFromPreview()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-master-settings-{Guid.NewGuid():N}.db");
    try
    {
        var source = new TrackingScenarioWorkspaceDataSource(SeedData.Create());
        var preview = new ScenarioRunPreviewService(source);
        var service = new MasterSettingsGovernanceService(source, preview, databasePath);

        var workspace = service.GetWorkspace();
        AssertTrue(source.LoadCount == 1, "workspace should read master settings through IScenarioWorkspaceDataSource");
        AssertTrue(workspace.CurrentSettings.Count > 0, "workspace should expose current master settings");
        AssertTrue(workspace.StatusCounts.Count > 0, "workspace should expose status counts");
        AssertTrue(workspace.TypeCounts.Count > 0, "workspace should expose type counts");

        var request = new ScenarioRunPreviewRequest(
            12,
            "TPL-ORDER-POLICY",
            Parameters: new ScenarioRunParameterSet(
                PrebuildCampaigns: new[] { new PrebuildCampaign("PB-MSG", "AV-FPGA-203", 1, 6, 8, 300) },
                CapacityAdjustments: new[] { new ResourceCapacityAdjustment("RES-TVAC", 1, 1.25m, "治理测试") },
                SkuPolicyOverrides: new[] { new SkuPolicyOverride("AV-FPGA-203", MinimumOrderQuantity: 500, OrderCycleDays: 10) },
                SupplierCapacityLimits: new[] { new SupplierCapacityLimit("Microchip Space", "进口空间级 FPGA", 1, 12, 1) }));

        var proposals = service.ProposeFromPreview(request);

        AssertTrue(source.LoadCount >= 3, "proposal generation should rerun preview and reload data through data source");
        AssertTrue(proposals.Proposals.Any(item => item.SettingType == "Inventory Buffer"), "MOQ/order cycle/prebuild should create inventory buffer proposals");
        AssertTrue(proposals.Proposals.Any(item => item.SettingType == "Capacity Buffer"), "capacity multiplier should create capacity buffer proposals");
        AssertTrue(proposals.Proposals.Any(item => item.SettingType is "Supplier Master Setting" or "Time Buffer"), "supplier limit should create supplier or time buffer proposals");
        AssertTrue(proposals.Proposals.Any(item => item.Rationale.Any(reason => reason.Contains("Scenario Preview", StringComparison.Ordinal))), "proposals should explain preview origin");
        AssertTrue(proposals.Trace.Any(item => item.Stage == "MasterSettings"), "proposal response should include master settings trace");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestMasterSettingsGovernanceSavesAuditsAndAdvancesStatus()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-master-settings-{Guid.NewGuid():N}.db");
    try
    {
        var source = new SeedScenarioWorkspaceDataSource(SeedData.Create());
        var preview = new ScenarioRunPreviewService(source);
        var service = new MasterSettingsGovernanceService(source, preview, databasePath);
        var proposal = service.ProposeFromPreview(new ScenarioRunPreviewRequest(
            12,
            Parameters: new ScenarioRunParameterSet(
                SkuPolicyOverrides: new[] { new SkuPolicyOverride("AV-FPGA-203", MinimumOrderQuantity: 500, OrderCycleDays: 10) })))
            .Proposals
            .First(item => item.SettingType == "Inventory Buffer");

        var saved = service.SaveChange(new MasterSettingChangeSaveRequest("计划员", proposal));

        AssertTrue(Guid.TryParseExact(saved.ChangeId, "N", out _), "saved change should return a GUID change id");
        AssertTrue(saved.ChangeNumber.StartsWith("MSG-", StringComparison.Ordinal), "saved change should return readable number");
        AssertEqual("Proposed", saved.Status, "initial governance status");
        AssertTrue(saved.IsPersisted, "saved change should be persisted");

        var list = service.ListChanges(50);
        AssertEqual(1, list.Count, "change list count");
        var detail = service.GetDetail(saved.ChangeId);
        AssertTrue(detail is not null, "saved change detail should be readable");
        AssertEqual("Inventory Buffer", detail!.Summary.SettingType, "saved setting type");
        AssertTrue(detail.Proposal.Rationale.Count > 0, "saved proposal should keep rationale");

        var audit = service.GetAuditEvents(saved.ChangeId);
        AssertEqual(4, audit.Count, "save audit event count");
        AssertEqual("ChangeProposed", audit[0].EventType, "audit event 1");
        AssertEqual("PreviewRecalculated", audit[1].EventType, "audit event 2");
        AssertEqual("ImpactCaptured", audit[2].EventType, "audit event 3");
        AssertEqual("ChangeSaved", audit[3].EventType, "audit event 4");

        var reviewed = service.UpdateStatus(saved.ChangeId, new MasterSettingStatusUpdateRequest("Reviewed", "计划员", "测试流转"));
        AssertEqual("Reviewed", reviewed.Status, "status should advance to reviewed");
        AssertTrue(service.GetAuditEvents(saved.ChangeId).Any(item => item.EventType == "StatusChanged"), "status change should append audit event");

        var invalidTransitionRejected = false;
        try
        {
            service.UpdateStatus(saved.ChangeId, new MasterSettingStatusUpdateRequest("Effective", "计划员", "非法跳转"));
        }
        catch (ArgumentException)
        {
            invalidTransitionRejected = true;
        }
        AssertTrue(invalidTransitionRejected, "status transition should only follow allowed sequence");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestScenarioRunWorkspaceExposesMasterSettingsGovernanceUi()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    AssertTrue(page.Contains("主设置治理", StringComparison.Ordinal), "page should expose master settings governance nav");
    AssertTrue(page.Contains("id=\"master-settings-panel\"", StringComparison.Ordinal), "page should expose master settings panel");
    AssertTrue(page.Contains("id=\"master-settings-kpis\"", StringComparison.Ordinal), "page should expose master settings KPIs");
    AssertTrue(page.Contains("id=\"master-setting-board\"", StringComparison.Ordinal), "page should expose master setting board");
    AssertTrue(page.Contains("id=\"master-setting-detail\"", StringComparison.Ordinal), "page should expose master setting detail");
    AssertTrue(page.Contains("id=\"master-setting-audit-list\"", StringComparison.Ordinal), "page should expose master setting audit chain");
    AssertTrue(page.Contains("生成主设置变更建议", StringComparison.Ordinal), "page should expose proposal generation action");
    AssertTrue(page.Contains("当前主设置", StringComparison.Ordinal), "page should expose current master settings");
    AssertTrue(page.Contains("DDOM 主设置", StringComparison.Ordinal), "page should expose Chinese DDOM master settings label");
    AssertTrue(!page.Contains("Inventory Buffer", StringComparison.Ordinal), "page should not expose English inventory buffer label");
    AssertTrue(!page.Contains("MPS 输出", StringComparison.Ordinal), "page should not expose MPS output");
    AssertTrue(!page.Contains("生产排程", StringComparison.Ordinal), "page should not expose production scheduling");
    AssertTrue(!page.Contains("推送 DDOM", StringComparison.Ordinal), "page should not expose DDOM push");

    AssertTrue(script.Contains("/api/master-settings-workspace", StringComparison.Ordinal), "script should call master settings workspace API");
    AssertTrue(script.Contains("/api/master-settings/proposals/from-preview", StringComparison.Ordinal), "script should call proposal API");
    AssertTrue(script.Contains("/api/master-settings/changes", StringComparison.Ordinal), "script should call change persistence API");
    AssertTrue(script.Contains("advanceMasterSettingStatus", StringComparison.Ordinal), "script should support governed status advance");
}

static void TestDdsopConfigInboundContractExportsReviewedDraftPayload()
{
    var service = new DdsopConfigInboundContractService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));
    var message = service.Build(new DdsopConfigInboundContractRequest(
        12,
        new DateOnly(2026, 6, 26),
        "S&OP 经理",
        "SR-20260626-0001",
        "CHG-20260626-001"));

    AssertEqual("DDSOP-CONFIG-INBOUND-V1", message.ContractID, "contract id");
    AssertEqual("1.0.0", message.ContractVersion, "contract version");
    AssertEqual("OperatingModelConfigurationPublished", message.MessageType, "message type");
    AssertEqual("DDAE", message.SourceSystem, "contract source system");
    AssertEqual("SDBR", message.TargetSystem, "contract target system");
    AssertEqual($"DDAE:{message.MessageID}", message.IdempotencyKey, "idempotency key");

    var payload = message.Payload;
    AssertTrue(payload.OperatingModelConfigurationID.StartsWith("DDSOP-OMC-", StringComparison.Ordinal), "operating model configuration id");
    AssertEqual("2026.06-A", payload.ConfigurationVersion, "configuration version");
    AssertEqual("1.0.0", payload.SchemaVersion, "schema version");
    AssertEqual("Approved", payload.Status, "approved status");
    AssertEqual("Asia/Shanghai", payload.TimeZone, "time zone");
    AssertEqual("S&OP 经理", payload.Approval.ApprovedBy, "approved by");
    AssertEqual("Approved", payload.Approval.ApprovalStatus, "approval status");
    AssertEqual("CAPACITY_CONSTRAINT", payload.ChangeReason.ReasonCode, "change reason code");
    AssertEqual("SR-20260626-0001", payload.SourceScenarioRunID!, "source scenario run");
    AssertEqual("CHG-20260626-001", payload.ChangeTicketID!, "change ticket");
    AssertTrue(payload.Fingerprint.StartsWith("sha256:", StringComparison.Ordinal) && payload.Fingerprint.Length == 71, "fingerprint should be sha256 prefixed lower hex");

    AssertTrue(payload.SchedulingConfiguration.ControlPoints.Count > 0, "scheduling configuration should expose control points");
    AssertTrue(payload.SchedulingConfiguration.TimeBufferProfiles.Count > 0, "scheduling configuration should expose time buffer profiles");
    AssertTrue(payload.SchedulingConfiguration.TimeBufferAssignments.Count > 0, "scheduling configuration should expose time buffer assignments");
    AssertTrue(payload.SchedulingConfiguration.ResourceSettings.Count > 0, "scheduling configuration should expose resource settings");
    AssertTrue(payload.SchedulingConfiguration.PartSchedulingSettings.Count > 0, "scheduling configuration should expose part scheduling settings");
    AssertEqual("PromiseDate", payload.SchedulingConfiguration.ProtectedDueDatePolicy, "protected due date policy");
    AssertEqual("ControlPointsOnly", payload.SchedulingConfiguration.FiniteResourceScope, "finite resource scope");
    foreach (var timeBuffer in payload.SchedulingConfiguration.TimeBufferProfiles)
    {
        AssertEqual(1.00m, timeBuffer.GreenRatio + timeBuffer.YellowRatio + timeBuffer.RedRatio, $"time buffer ratio for {timeBuffer.ProfileID}");
    }

    AssertTrue(payload.DDMRPConfiguration.DecouplingPoints.Count > 0, "DDMRP configuration should expose decoupling points");
    AssertTrue(payload.DDMRPConfiguration.StockBufferProfiles.Count > 0, "DDMRP configuration should expose stock buffer profiles");
    AssertTrue(payload.DDMRPConfiguration.PartProfileAssignments.Count > 0, "DDMRP configuration should expose part profile assignments");
    AssertEqual("DDMRP-PRIORITY-RED-YELLOW-GREEN-001", payload.DDMRPConfiguration.PlanningPriorityPolicyID, "planning priority policy");
    AssertEqual("ProvidedByDDSOP", payload.DDMRPConfiguration.SpikeQualificationMode!, "spike qualification mode");
    var fpgaBuffer = payload.DDMRPConfiguration.StockBufferProfiles.First(item => item.BufferProfileID.Contains("AV-FPGA-203", StringComparison.Ordinal));
    AssertTrue(fpgaBuffer.TopOfRed > 0, "stock buffer profile should expose top of red");
    AssertTrue(fpgaBuffer.TopOfRed <= fpgaBuffer.TopOfYellow, "stock buffer profile should satisfy top of red/yellow order");
    AssertTrue(fpgaBuffer.TopOfYellow <= fpgaBuffer.TopOfGreen, "stock buffer profile should satisfy top of yellow/green order");
    AssertEqual("EA", fpgaBuffer.UnitOfMeasure, "unit of measure");
    var fpgaDecouplingPoint = payload.DDMRPConfiguration.DecouplingPoints.First(item => item.ItemID == "AV-FPGA-203");
    AssertTrue(fpgaDecouplingPoint.DLTMinutes > 0, "decoupling point should expose DLT minutes");
    AssertTrue(fpgaDecouplingPoint.MinimumOrderQty > 0, "decoupling point should expose minimum order quantity");
    AssertTrue(fpgaDecouplingPoint.OrderCycleDays > 0, "decoupling point should expose non-zero order cycle days");

    AssertTrue(payload.Scope.PlantIDs.Contains("PLANT-DDAE-SAT", StringComparer.Ordinal), "scope should expose plant IDs");
    AssertTrue(payload.Scope.ProductFamilyIDs is { Count: > 0 }, "scope should expose product family IDs");
    AssertTrue(payload.Scope.ResourceGroupIDs is { Count: > 0 }, "scope should expose resource group IDs");
    AssertTrue(payload.Scope.ItemLocationIDs is { Count: > 0 }, "scope should expose item location IDs");
}

static void TestDdsopConfigInboundContractFingerprintIsCanonicalAndStable()
{
    var service = new DdsopConfigInboundContractService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));
    var request = new DdsopConfigInboundContractRequest(12, new DateOnly(2026, 6, 26), "S&OP 经理");
    var message = service.Build(request);
    var secondMessage = service.Build(request);
    var recomputedFingerprint = DdsopConfigInboundContractService.ComputeFingerprint(message.Payload);

    AssertEqual(message.Payload.Fingerprint, recomputedFingerprint, "fingerprint should be recomputable from payload");
    AssertEqual(message.Payload.Fingerprint, secondMessage.Payload.Fingerprint, "fingerprint should be stable when payload is unchanged");

    var json = JsonSerializer.Serialize(message, DdsopConfigInboundContractService.ContractJsonOptions);

    AssertTrue(json.Contains("\"ContractID\"", StringComparison.Ordinal), "contract JSON should keep ContractID");
    AssertTrue(json.Contains("\"ContractVersion\"", StringComparison.Ordinal), "contract JSON should keep ContractVersion");
    AssertTrue(json.Contains("\"TargetSystem\"", StringComparison.Ordinal), "contract JSON should keep TargetSystem");
    AssertTrue(json.Contains("\"OccurredAt\"", StringComparison.Ordinal), "contract JSON should keep OccurredAt");
    AssertTrue(json.Contains("\"OperatingModelConfigurationID\"", StringComparison.Ordinal), "contract JSON should keep OperatingModelConfigurationID");
    AssertTrue(json.Contains("\"Approval\"", StringComparison.Ordinal), "contract JSON should keep Approval");
    AssertTrue(json.Contains("\"ChangeReason\"", StringComparison.Ordinal), "contract JSON should keep ChangeReason");
    AssertTrue(json.Contains("\"ControlPoints\"", StringComparison.Ordinal), "contract JSON should keep ControlPoints");
    AssertTrue(json.Contains("\"DecouplingPoints\"", StringComparison.Ordinal), "contract JSON should keep DecouplingPoints");
    AssertTrue(json.Contains("\"TopOfRed\"", StringComparison.Ordinal), "contract JSON should keep TopOfRed");
    AssertTrue(json.Contains("\"UnitOfMeasure\"", StringComparison.Ordinal), "contract JSON should keep UnitOfMeasure");
    AssertTrue(!json.Contains("\"contractID\"", StringComparison.Ordinal), "contract JSON should not camel-case ContractID");
    AssertTrue(!json.Contains("\"operatingModelConfigurationID\"", StringComparison.Ordinal), "contract JSON should not camel-case OperatingModelConfigurationID");
    AssertTrue(!json.Contains("\"ConsumerSystem\"", StringComparison.Ordinal), "contract JSON should not expose removed ConsumerSystem");
    AssertTrue(!json.Contains("\"CreatedAt\"", StringComparison.Ordinal), "contract JSON should not expose removed CreatedAt");
    AssertTrue(!json.Contains("\"ExpectedAcknowledgementStatuses\"", StringComparison.Ordinal), "contract JSON should not expose removed acknowledgement status list");
    AssertTrue(!json.Contains("\"Checksum\"", StringComparison.Ordinal), "contract JSON should not expose removed checksum");
    AssertTrue(!json.Contains("\"SpikeHorizonMinutes\":", StringComparison.Ordinal) || json.Contains("\"DDMRPConfiguration\"", StringComparison.Ordinal), "spike horizon should only exist under DDMRPConfiguration");
}

static void TestDdsopConfigInboundAckInterpreterHandlesStatuses()
{
    var contractRoot = @"D:\Documents\DDAE_INTERFACE_CONTRACT\contracts\ddsop-config-inbound-v1\examples";
    var acceptedJson = File.ReadAllText(Path.Combine(contractRoot, "ack-accepted.json"));
    var pendingJson = File.ReadAllText(Path.Combine(contractRoot, "ack-pending-references.json"));
    var rejectedJson = File.ReadAllText(Path.Combine(contractRoot, "ack-rejected-missing-approval.json"));
    var duplicateNode = JsonNode.Parse(acceptedJson)!.AsObject();
    duplicateNode["ProcessingStatus"] = "Duplicate";
    duplicateNode["UsableForPlanningRun"] = false;
    duplicateNode["AcceptedConfigurationID"] = null;
    duplicateNode["Fingerprint"] = null;
    var duplicateJson = duplicateNode.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions);

    var interpreter = new DdsopConfigInboundAckInterpreter();
    var accepted = interpreter.Interpret(acceptedJson);
    var pending = interpreter.Interpret(pendingJson);
    var rejected = interpreter.Interpret(rejectedJson);
    var duplicate = interpreter.Interpret(duplicateJson);

    AssertEqual("Accepted", accepted.ProcessingStatus, "accepted ack status");
    AssertTrue(accepted.UsableForPlanningRun, "accepted ack usable for planning run");
    AssertTrue(accepted.Message.Contains("可用于 Planning Run", StringComparison.Ordinal), "accepted ack message");
    AssertEqual("AcceptedPendingReferences", pending.ProcessingStatus, "pending references ack status");
    AssertTrue(!pending.UsableForPlanningRun, "pending references ack should not be usable");
    AssertTrue(pending.PendingReferences.Count > 0, "pending references ack should expose pending references");
    AssertEqual("Rejected", rejected.ProcessingStatus, "rejected ack status");
    AssertTrue(rejected.Errors.Count > 0, "rejected ack should expose contract errors");
    AssertEqual("Duplicate", duplicate.ProcessingStatus, "duplicate ack status");
    AssertTrue(duplicate.Message.Contains("重复 IdempotencyKey", StringComparison.Ordinal), "duplicate ack message");
}

static void TestDdsopFeedbackOutboundPayloadsCanBeInterpreted()
{
    var contractRoot = @"D:\Documents\DDAE_INTERFACE_CONTRACT\contracts\ddsop-feedback-outbound-v1\examples";
    var planningJson = File.ReadAllText(Path.Combine(contractRoot, "golden-planning-run-feedback.json"));
    var varianceJson = File.ReadAllText(Path.Combine(contractRoot, "golden-variance-analysis-feedback.json"));

    var interpreter = new DdsopFeedbackOutboundInterpreter();
    var planning = interpreter.Interpret(planningJson);
    var variance = interpreter.Interpret(varianceJson);
    var duplicate = interpreter.Interpret(planningJson, new HashSet<string>(StringComparer.Ordinal) { "SDBR:SDBR-MSG-PRF-20260626-001" });

    AssertEqual("Accepted", planning.Status, "planning feedback interpretation status");
    AssertEqual("PlanningRunFeedback", planning.FeedbackType!, "planning feedback type");
    AssertEqual("RUN-20260626-001", planning.PlanningRunID!, "planning run id");
    AssertEqual("DDSOP-OMC-20260626-A", planning.OperatingModelConfigurationID!, "planning operating model id");
    AssertTrue(planning.OperatingModelFingerprint!.StartsWith("sha256:", StringComparison.Ordinal), "planning fingerprint");
    AssertEqual("Yellow", planning.OverallStatus!, "planning overall status");
    AssertEqual(0, planning.ApprovedConfigurationChangeCount, "planning feedback should not create approved configuration changes");

    AssertEqual("Accepted", variance.Status, "variance feedback interpretation status");
    AssertEqual("VarianceAnalysisFeedback", variance.FeedbackType!, "variance feedback type");
    AssertEqual("RUN-20260626-001", variance.PlanningRunID!, "variance planning run id");
    AssertEqual("DDSOP-OMC-20260626-A", variance.OperatingModelConfigurationID!, "variance operating model id");
    AssertEqual("Yellow", variance.OverallStatus!, "variance overall status");
    AssertEqual(1, variance.ReviewTopicCount, "variance review topic count");
    AssertEqual(1, variance.DataCoverageIssueCount, "variance data coverage issue count");
    AssertEqual(0, variance.ApprovedConfigurationChangeCount, "variance feedback should not create approved configuration changes");
    AssertEqual("Duplicate", duplicate.Status, "duplicate idempotency key should be reported");
}

static void TestDdsopFeedbackInboundLedgerPreservesRawPayloadAndIdempotency()
{
    var contractRoot = @"D:\Documents\DDAE_INTERFACE_CONTRACT\contracts\ddsop-feedback-outbound-v1\examples";
    var planningJson = File.ReadAllText(Path.Combine(contractRoot, "golden-planning-run-feedback.json"));
    var ledger = new DdsopFeedbackInboundLedger();

    var accepted = ledger.Accept(planningJson);
    var duplicate = ledger.Accept(planningJson);

    AssertEqual("DDSOP-FEEDBACK-OUTBOUND-V1", accepted.ContractID, "feedback ack contract id");
    AssertEqual("1.0.0", accepted.ContractVersion, "feedback ack contract version");
    AssertEqual("Accepted", accepted.ProcessingStatus, "feedback ack accepted status");
    AssertEqual("RUN-20260626-001", accepted.LinkedPlanningRunID!, "linked planning run id");
    AssertEqual("DDSOP-OMC-20260626-A", accepted.LinkedOperatingModelConfigurationID!, "linked operating model id");
    AssertTrue(accepted.LinkedOperatingModelFingerprint!.StartsWith("sha256:", StringComparison.Ordinal), "linked fingerprint");
    AssertEqual(0, accepted.Errors.Count, "accepted feedback ack errors");
    AssertEqual("Duplicate", duplicate.ProcessingStatus, "duplicate feedback ack status");
    AssertEqual(1, ledger.Records.Count, "duplicate should not create a second ledger record");
    AssertEqual(planningJson, ledger.Records[0].RawPayload, "ledger should preserve raw payload exactly");
    AssertEqual(0, ledger.Records[0].Interpretation.ApprovedConfigurationChangeCount, "ledger should not create approved master setting changes");

    var missingNode = JsonNode.Parse(planningJson)!.AsObject();
    missingNode["IdempotencyKey"] = "SDBR:PlanningRunFeedback:MissingPlanningRunID";
    missingNode["Payload"]!.AsObject().Remove("PlanningRunID");
    var missingLedger = new DdsopFeedbackInboundLedger();
    var rejected = missingLedger.Accept(missingNode.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", rejected.ProcessingStatus, "missing PlanningRunID should be rejected");
    AssertTrue(rejected.Errors.Any(item => item.Code == "REQUIRED_FIELD_MISSING" && item.Field == "Payload.PlanningRunID"), "missing PlanningRunID should use contract error code");

    var missingConfigNode = JsonNode.Parse(planningJson)!.AsObject();
    missingConfigNode["IdempotencyKey"] = "SDBR:PlanningRunFeedback:MissingOperatingModelConfigurationID";
    missingConfigNode["Payload"]!.AsObject().Remove("OperatingModelConfigurationID");
    var rejectedMissingConfig = new DdsopFeedbackInboundLedger().Accept(missingConfigNode.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", rejectedMissingConfig.ProcessingStatus, "missing OperatingModelConfigurationID should be rejected");
    AssertTrue(rejectedMissingConfig.Errors.Any(item => item.Code == "REQUIRED_FIELD_MISSING" && item.Field == "Payload.OperatingModelConfigurationID"), "missing OperatingModelConfigurationID should use contract error code");
}

static void TestDdsopFeedbackInboundLedgerAcceptsSdbrGeneratedPayloads()
{
    var fixtureRoot = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Fixtures");
    var planningJson = File.ReadAllText(Path.GetFullPath(Path.Combine(fixtureRoot, "sdbr-actual-planning-run-feedback.json")));
    var varianceJson = File.ReadAllText(Path.GetFullPath(Path.Combine(fixtureRoot, "sdbr-actual-variance-analysis-feedback.json")));
    var ledger = new DdsopFeedbackInboundLedger();

    var planningAck = ledger.Accept(planningJson);
    var varianceAck = ledger.Accept(varianceJson);

    AssertEqual("Accepted", planningAck.ProcessingStatus, "SDBR generated planning feedback should be accepted");
    AssertEqual("Accepted", varianceAck.ProcessingStatus, "SDBR generated variance feedback should be accepted");
    AssertEqual("TST-RUN-DDSOP-FREEZE-001", planningAck.LinkedPlanningRunID!, "SDBR planning run id");
    AssertEqual("TST-RUN-DDSOP-FREEZE-001", varianceAck.LinkedPlanningRunID!, "SDBR variance run id");
    AssertEqual("DDSOP-OMC-20260626-A", planningAck.LinkedOperatingModelConfigurationID!, "SDBR planning operating model id");
    AssertEqual("DDSOP-OMC-20260626-A", varianceAck.LinkedOperatingModelConfigurationID!, "SDBR variance operating model id");
    AssertEqual(2, ledger.Records.Count, "SDBR generated payloads should be stored in ledger");
    AssertTrue(ledger.Records.All(item => item.RawPayload.Contains("\"ContractID\":\"DDSOP-FEEDBACK-OUTBOUND-V1\"", StringComparison.Ordinal)), "ledger should store raw SDBR payloads");
    AssertTrue(ledger.Records.All(item => item.Interpretation.ApprovedConfigurationChangeCount == 0), "SDBR review topics should not become approved master setting changes");
}

static void TestDdsopFeedbackInboundEndpointIsExposed()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var program = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Program.cs"));
    var domain = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "DdsopConfigInboundContract.cs"));

    AssertTrue(program.Contains("AddSingleton<DdsopFeedbackInboundLedger>", StringComparison.Ordinal), "feedback ledger should be registered");
    AssertTrue(program.Contains("/api/integration-contracts/ddsop-feedback-outbound-v1", StringComparison.Ordinal), "feedback inbound endpoint should be exposed");
    AssertTrue(program.Contains("/api/integration-contracts/ddsop-feedback-outbound-v1/ledger", StringComparison.Ordinal), "feedback ledger endpoint should be exposed");
    AssertTrue(domain.Contains("public sealed class DdsopConfigInboundAckInterpreter", StringComparison.Ordinal), "config ack interpreter should exist");
    AssertTrue(domain.Contains("public sealed class DdsopFeedbackInboundLedger", StringComparison.Ordinal), "feedback ledger should exist");
    AssertTrue(domain.Contains("Duplicate", StringComparison.Ordinal), "feedback ledger should support duplicate ACK status");
    AssertTrue(domain.Contains("REQUIRED_FIELD_MISSING", StringComparison.Ordinal), "feedback ledger should return contract missing field code");
    AssertTrue(!domain.Contains("MasterSettingChangeSaveRequest", StringComparison.Ordinal), "feedback ledger should not save master setting changes");
}

static void TestProductionSupplierIdentitySourceAcceptsReviewedSourceWithoutGovernanceMutation()
{
    var rawPayload = ProductionSupplierIdentitySourceReviewedJson();
    var ledger = new ProductionSupplierIdentitySourceInboundLedger(() => new DateTimeOffset(2026, 6, 28, 9, 30, 0, TimeSpan.Zero));

    var ack = ledger.Accept(rawPayload);

    ProductionSupplierIdentitySourceInboundLedger.ValidateAckShape(ack);
    AssertEqual("PRODUCTION-SUPPLIER-IDENTITY-SOURCE-V1", ack.ContractID, "supplier identity ACK contract id");
    AssertEqual("1.0.0", ack.ContractVersion, "supplier identity ACK contract version");
    AssertEqual("Accepted", ack.AckStatus, "reviewed supplier identity source should be accepted");
    AssertEqual("DDAE", ack.ConsumerSystem, "consumer system");
    AssertEqual("PSISV1-MSG-SUP-MICROCHIP-PART-FPGA-SPACE-20260628-001", ack.MessageID, "message id");
    AssertEqual("PSIS-EP-SUP-MICROCHIP-PART-FPGA-SPACE-20260628-001", ack.EvidencePackageID, "evidence package id");
    AssertEqual(0, ack.Errors.Count, "accepted ACK should not include errors");
    AssertEqual(1, ledger.Records.Count, "accepted payload should be stored in ledger");
    AssertEqual(rawPayload, ledger.Records[0].RawPayload, "ledger must preserve raw payload exactly");
    AssertEqual("sha256:", ledger.Records[0].PayloadFingerprint[..7], "ledger should store deterministic payload fingerprint");
    AssertEqual("Reviewed", ledger.Records[0].Interpretation.EvidenceConfidence, "evidence confidence should remain reviewed");
    AssertTrue(!ledger.Records[0].Interpretation.AllowsAutomaticMasterSettingUpdate, "contract should not allow automatic DDAE master setting update");
    AssertTrue(ledger.Records[0].Interpretation.RequiresSeparateDdaeApproval, "contract should require separate DDAE approval");
    AssertTrue(!ledger.Records[0].Interpretation.MutatedDdaeGovernance, "consumer must not mutate DDAE governance");
}

static void TestProductionSupplierIdentitySourceRejectsGovernanceBoundaryViolations()
{
    var autoUpdate = JsonNode.Parse(ProductionSupplierIdentitySourceReviewedJson())!.AsObject();
    autoUpdate["MessageID"] = "PSS-MSG-PART-FPGA-SPACE-AUTO-UPDATE";
    autoUpdate["IdempotencyKey"] = "PRODUCTION-SUPPLIER-IDENTITY-SOURCE-V1:ContractAgentAcceptedSource:SRC-PART-FPGA-SPACE:20260628-A:auto";
    autoUpdate["Payload"]!["DDAEGovernanceBoundary"]!["AllowsAutomaticMasterSettingUpdate"] = true;
    var autoUpdateAck = new ProductionSupplierIdentitySourceInboundLedger().Accept(autoUpdate.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", autoUpdateAck.AckStatus, "automatic governance update must be rejected");
    AssertTrue(autoUpdateAck.Errors.Any(item => item.ErrorCode == "GOVERNANCE_AUTO_UPDATE_FORBIDDEN"), "automatic governance update should use contract error code");

    var contradictoryTerms = JsonNode.Parse(ProductionSupplierIdentitySourceReviewedJson())!.AsObject();
    contradictoryTerms["MessageID"] = "PSS-MSG-PART-FPGA-SPACE-CONTRADICTORY";
    contradictoryTerms["IdempotencyKey"] = "PRODUCTION-SUPPLIER-IDENTITY-SOURCE-V1:ContractAgentAcceptedSource:SRC-PART-FPGA-SPACE:20260628-A:contradictory";
    contradictoryTerms["Payload"]!["SourceTerms"]!["TermsAreProductionAuthoritative"] = true;
    contradictoryTerms["Payload"]!["SourceTerms"]!["DDAEPlanningAssumptionOnly"] = true;
    var contradictoryAck = new ProductionSupplierIdentitySourceInboundLedger().Accept(contradictoryTerms.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", contradictoryAck.AckStatus, "contradictory source terms must be rejected");
    AssertTrue(contradictoryAck.Errors.Any(item => item.ErrorCode == "CONTRACT_SCOPE_VIOLATION"), "contradictory source terms should use scope violation code");

    var productionValidated = JsonNode.Parse(ProductionSupplierIdentitySourceReviewedJson())!.AsObject();
    productionValidated["MessageID"] = "PSS-MSG-PART-FPGA-SPACE-PRODUCTION-VALIDATED";
    productionValidated["IdempotencyKey"] = "PRODUCTION-SUPPLIER-IDENTITY-SOURCE-V1:ContractAgentAcceptedSource:SRC-PART-FPGA-SPACE:20260628-A:production-validated";
    productionValidated["Payload"]!["EvidenceConfidence"] = "ProductionValidatedReserved";
    var productionValidatedAck = new ProductionSupplierIdentitySourceInboundLedger().Accept(productionValidated.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", productionValidatedAck.AckStatus, "ProductionValidatedReserved must not be accepted by DDAE");
    AssertTrue(productionValidatedAck.Errors.Any(item => item.ErrorCode == "CONTRACT_SCOPE_VIOLATION"), "ProductionValidatedReserved should use scope violation code");

    var missingAuthority = JsonNode.Parse(ProductionSupplierIdentitySourceReviewedJson())!.AsObject();
    missingAuthority["MessageID"] = "PSS-MSG-PART-FPGA-SPACE-MISSING-AUTHORITY";
    missingAuthority["IdempotencyKey"] = "PRODUCTION-SUPPLIER-IDENTITY-SOURCE-V1:ContractAgentAcceptedSource:SRC-PART-FPGA-SPACE:20260628-A:missing-authority";
    missingAuthority["Payload"]!.AsObject().Remove("SourceAuthority");
    var missingAuthorityAck = new ProductionSupplierIdentitySourceInboundLedger().Accept(missingAuthority.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("DeadLettered", missingAuthorityAck.AckStatus, "missing source authority must be dead-lettered");
    AssertTrue(missingAuthorityAck.Errors.Any(item => item.ErrorCode == "MISSING_SOURCE_AUTHORITY"), "missing source authority should use contract error code");

    var invalidWindow = JsonNode.Parse(ProductionSupplierIdentitySourceReviewedJson())!.AsObject();
    invalidWindow["MessageID"] = "PSS-MSG-PART-FPGA-SPACE-INVALID-WINDOW";
    invalidWindow["IdempotencyKey"] = "PRODUCTION-SUPPLIER-IDENTITY-SOURCE-V1:ContractAgentAcceptedSource:SRC-PART-FPGA-SPACE:20260628-A:invalid-window";
    invalidWindow["Payload"]!["SupplierSourceRelation"]!["EffectiveTo"] = "2026-06-27T00:00:00+08:00";
    var invalidWindowAck = new ProductionSupplierIdentitySourceInboundLedger().Accept(invalidWindow.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("DeadLettered", invalidWindowAck.AckStatus, "EffectiveTo earlier than EffectiveFrom must be dead-lettered");
    AssertTrue(invalidWindowAck.Errors.Any(item => item.ErrorCode == "INVALID_EFFECTIVE_WINDOW"), "invalid effective window should use contract error code");
}

static void TestProductionSupplierIdentitySourceLedgerDetectsDuplicateAndConflict()
{
    var rawPayload = ProductionSupplierIdentitySourceReviewedJson();
    var ledger = new ProductionSupplierIdentitySourceInboundLedger();

    var accepted = ledger.Accept(rawPayload);
    var duplicate = ledger.Accept(rawPayload);

    AssertEqual("Accepted", accepted.AckStatus, "first payload should be accepted");
    AssertEqual("Duplicate", duplicate.AckStatus, "same IdempotencyKey and same fingerprint should return duplicate");
    AssertEqual(1, ledger.Records.Count, "duplicate should not create another ledger record");

    var conflictNode = JsonNode.Parse(rawPayload)!.AsObject();
    conflictNode["Payload"]!["SupplierSourceRelation"]!["AllocationPercent"] = 80;
    var conflict = ledger.Accept(conflictNode.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("DeadLettered", conflict.AckStatus, "same IdempotencyKey with different fingerprint should be dead-lettered");
    AssertTrue(conflict.Errors.Any(item => item.ErrorCode == "IDEMPOTENCY_CONFLICT"), "idempotency conflict should use contract error code");
    AssertEqual(2, ledger.Records.Count, "conflict should be retained as audit evidence");
}

static void TestProductionSupplierIdentitySourceEndpointIsExposed()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var program = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Program.cs"));
    var domain = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "ProductionSupplierIdentitySourceContract.cs"));

    AssertTrue(program.Contains("AddSingleton<ProductionSupplierIdentitySourceInboundLedger>", StringComparison.Ordinal), "supplier identity ledger should be registered");
    AssertTrue(program.Contains("/api/integration-contracts/production-supplier-identity-source-v1", StringComparison.Ordinal), "supplier identity inbound endpoint should be exposed");
    AssertTrue(program.Contains("/api/integration-contracts/production-supplier-identity-source-v1/ledger", StringComparison.Ordinal), "supplier identity ledger endpoint should be exposed");
    AssertTrue(domain.Contains("public sealed class ProductionSupplierIdentitySourceInboundLedger", StringComparison.Ordinal), "supplier identity ledger should exist");
    AssertTrue(domain.Contains("ProductionValidatedReserved", StringComparison.Ordinal), "consumer should explicitly reject reserved ProductionValidated confidence");
    AssertTrue(domain.Contains("GOVERNANCE_AUTO_UPDATE_FORBIDDEN", StringComparison.Ordinal), "consumer should enforce governance auto-update boundary");
    AssertTrue(domain.Contains("IDEMPOTENCY_CONFLICT", StringComparison.Ordinal), "consumer should enforce idempotency conflict handling");
    AssertTrue(!domain.Contains("MasterSettingsGovernanceService", StringComparison.Ordinal), "supplier identity consumer must not depend on master settings governance service");
    AssertTrue(!domain.Contains("MasterSettingChangeSaveRequest", StringComparison.Ordinal), "supplier identity consumer must not save master setting changes");
}

static void TestProductionInventoryQualityEvidenceAcceptsReviewedContextWithoutGovernanceMutation()
{
    var rawPayload = ProductionInventoryQualityEvidenceReviewedJson();
    var ledger = new ProductionInventoryQualityInboundLedger(() => new DateTimeOffset(2026, 6, 28, 4, 0, 0, TimeSpan.Zero));

    var ack = ledger.Accept(rawPayload);

    ProductionInventoryQualityInboundLedger.ValidateAckShape(ack);
    AssertEqual("PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1", ack.ContractID, "inventory quality ACK contract id");
    AssertEqual("1.0.0", ack.ContractVersion, "inventory quality ACK contract version");
    AssertEqual("Accepted", ack.AckStatus, "reviewed inventory quality evidence should be accepted as controlled context");
    AssertEqual("DDAE", ack.ConsumerSystem, "consumer system");
    AssertEqual("PIQEV1-MSG-PART-FPGA-SPACE-20260628-001", ack.MessageID, "message id");
    AssertEqual("PIQE-PART-FPGA-SPACE-20260628-001", ack.EvidencePackageID, "evidence package id");
    AssertEqual(0, ack.Errors.Count, "accepted ACK should not include errors");
    AssertEqual(1, ledger.Records.Count, "accepted payload should be stored in ledger");
    AssertEqual(rawPayload, ledger.Records[0].RawPayload, "ledger must preserve raw payload exactly");
    AssertEqual("sha256:", ledger.Records[0].PayloadFingerprint[..7], "ledger should store deterministic payload fingerprint");
    AssertEqual("Reviewed", ledger.Records[0].Interpretation.EvidenceConfidence, "reviewed example must remain reviewed context");
    AssertEqual("PART-FPGA-SPACE", ledger.Records[0].Interpretation.ItemID, "item id should be interpreted");
    AssertEqual("WH-ELEC-QA", ledger.Records[0].Interpretation.LocationID, "location id should be interpreted");
    AssertTrue(!ledger.Records[0].Interpretation.AllowsAutomaticMasterSettingUpdate, "contract should not allow automatic DDAE master setting update");
    AssertTrue(ledger.Records[0].Interpretation.RequiresSeparateDdaeApproval, "contract should require separate DDAE approval");
    AssertTrue(!ledger.Records[0].Interpretation.MutatedDdaeGovernance, "consumer must not mutate DDAE governance");
    AssertTrue(!ledger.Records[0].Interpretation.IsProductionValidated, "ACK success must not become production validation");

    var acceptedAckExample = JsonNode.Parse(ProductionInventoryQualityAcceptedAckJson())!.AsObject();
    var exampleAck = new ProductionInventoryQualityAck(
        acceptedAckExample["ContractID"]!.GetValue<string>(),
        acceptedAckExample["ContractVersion"]!.GetValue<string>(),
        acceptedAckExample["AckID"]!.GetValue<string>(),
        acceptedAckExample["MessageID"]!.GetValue<string>(),
        acceptedAckExample["EvidencePackageID"]!.GetValue<string>(),
        acceptedAckExample["ConsumerSystem"]!.GetValue<string>(),
        acceptedAckExample["AckStatus"]!.GetValue<string>(),
        acceptedAckExample["AckAt"]!.GetValue<string>(),
        Array.Empty<ProductionInventoryQualityAckError>());
    ProductionInventoryQualityInboundLedger.ValidateAckShape(exampleAck);
}

static void TestProductionInventoryQualityEvidenceRejectsBusinessRuleViolations()
{
    var missingInventoryAuthority = JsonNode.Parse(ProductionInventoryQualityEvidenceReviewedJson())!.AsObject();
    missingInventoryAuthority["MessageID"] = "PIQE-MSG-MISSING-INVENTORY-AUTHORITY";
    missingInventoryAuthority["IdempotencyKey"] = "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1:missing-inventory-authority";
    missingInventoryAuthority["Payload"]!["InventoryAuthority"] = null;
    var missingInventoryAck = new ProductionInventoryQualityInboundLedger().Accept(missingInventoryAuthority.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", missingInventoryAck.AckStatus, "InventorySnapshot without authority must be rejected");
    AssertTrue(missingInventoryAck.Errors.Any(item => item.ErrorCode == "MISSING_INVENTORY_AUTHORITY"), "missing inventory authority should use contract error code");

    var missingQualityAuthority = JsonNode.Parse(ProductionInventoryQualityEvidenceReviewedJson())!.AsObject();
    missingQualityAuthority["MessageID"] = "PIQE-MSG-MISSING-QUALITY-AUTHORITY";
    missingQualityAuthority["IdempotencyKey"] = "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1:missing-quality-authority";
    missingQualityAuthority["Payload"]!["QualityAuthority"] = null;
    var missingQualityAck = new ProductionInventoryQualityInboundLedger().Accept(missingQualityAuthority.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", missingQualityAck.AckStatus, "QualityReleased without authority must be rejected");
    AssertTrue(missingQualityAck.Errors.Any(item => item.ErrorCode == "MISSING_QUALITY_AUTHORITY"), "missing quality authority should use contract error code");

    var unknownItem = JsonNode.Parse(ProductionInventoryQualityEvidenceReviewedJson())!.AsObject();
    unknownItem["MessageID"] = "PIQE-MSG-UNKNOWN-ITEM";
    unknownItem["IdempotencyKey"] = "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1:unknown-item";
    unknownItem["Payload"]!["ItemLocation"]!["ItemID"] = "PART-UNKNOWN";
    var unknownItemAck = new ProductionInventoryQualityInboundLedger().Accept(unknownItem.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", unknownItemAck.AckStatus, "unknown item must be rejected");
    AssertTrue(unknownItemAck.Errors.Any(item => item.ErrorCode == "UNKNOWN_ITEM"), "unknown item should use contract error code");

    var unsupportedUom = JsonNode.Parse(ProductionInventoryQualityEvidenceReviewedJson())!.AsObject();
    unsupportedUom["MessageID"] = "PIQE-MSG-UNSUPPORTED-UOM";
    unsupportedUom["IdempotencyKey"] = "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1:unsupported-uom";
    unsupportedUom["Payload"]!["ItemLocation"]!["QuantityUOM"] = "BOX";
    var unsupportedUomAck = new ProductionInventoryQualityInboundLedger().Accept(unsupportedUom.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", unsupportedUomAck.AckStatus, "unsupported UOM must be rejected");
    AssertTrue(unsupportedUomAck.Errors.Any(item => item.ErrorCode == "UNSUPPORTED_UOM"), "unsupported UOM should use contract error code");

    var invalidQuantity = JsonNode.Parse(ProductionInventoryQualityEvidenceReviewedJson())!.AsObject();
    invalidQuantity["MessageID"] = "PIQE-MSG-INVALID-QUANTITY";
    invalidQuantity["IdempotencyKey"] = "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1:invalid-quantity";
    invalidQuantity["Payload"]!["InventorySnapshot"]!["AvailableQty"] = -1;
    var invalidQuantityAck = new ProductionInventoryQualityInboundLedger().Accept(invalidQuantity.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", invalidQuantityAck.AckStatus, "negative inventory quantity must be rejected");
    AssertTrue(invalidQuantityAck.Errors.Any(item => item.ErrorCode == "INVALID_QUANTITY"), "invalid quantity should use contract error code");

    var invalidTimestamp = JsonNode.Parse(ProductionInventoryQualityEvidenceReviewedJson())!.AsObject();
    invalidTimestamp["MessageID"] = "PIQE-MSG-INVALID-TIMESTAMP";
    invalidTimestamp["IdempotencyKey"] = "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1:invalid-timestamp";
    invalidTimestamp["OccurredAt"] = "2026-06-28T11:30:00";
    var invalidTimestampAck = new ProductionInventoryQualityInboundLedger().Accept(invalidTimestamp.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", invalidTimestampAck.AckStatus, "timezone-free timestamp must be rejected");
    AssertTrue(invalidTimestampAck.Errors.Any(item => item.ErrorCode == "INVALID_TIMESTAMP"), "invalid timestamp should use contract error code");

    var autoUpdate = JsonNode.Parse(ProductionInventoryQualityEvidenceReviewedJson())!.AsObject();
    autoUpdate["MessageID"] = "PIQE-MSG-AUTO-UPDATE";
    autoUpdate["IdempotencyKey"] = "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1:auto-update";
    autoUpdate["Payload"]!["DDAEGovernanceBoundary"]!["AllowsAutomaticMasterSettingUpdate"] = true;
    var autoUpdateAck = new ProductionInventoryQualityInboundLedger().Accept(autoUpdate.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", autoUpdateAck.AckStatus, "automatic DDAE governance update must be rejected");
    AssertTrue(autoUpdateAck.Errors.Any(item => item.ErrorCode == "GOVERNANCE_AUTO_UPDATE_FORBIDDEN"), "automatic update should use governance error code");

    var productionValidated = JsonNode.Parse(ProductionInventoryQualityEvidenceReviewedJson())!.AsObject();
    productionValidated["MessageID"] = "PIQE-MSG-PRODUCTION-VALIDATED";
    productionValidated["IdempotencyKey"] = "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1:production-validated";
    productionValidated["Payload"]!["EvidenceConfidence"] = "ProductionValidatedReserved";
    var productionValidatedAck = new ProductionInventoryQualityInboundLedger().Accept(productionValidated.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", productionValidatedAck.AckStatus, "ProductionValidatedReserved must not be accepted");
    AssertTrue(productionValidatedAck.Errors.Any(item => item.ErrorCode == "CONTRACT_SCOPE_VIOLATION"), "reserved production validation should use scope violation");

    var reversal = JsonNode.Parse(ProductionInventoryQualityEvidenceReviewedJson())!.AsObject();
    reversal["MessageID"] = "PIQE-MSG-REVERSAL-MISSING-TARGET";
    reversal["IdempotencyKey"] = "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1:reversal-missing-target";
    reversal["Payload"]!["StockMovements"]![0]!["MovementType"] = "Reversal";
    reversal["Payload"]!["StockMovements"]![0]!["MovementState"] = "Reversed";
    reversal["Payload"]!["StockMovements"]![0]!["ReversesMovementID"] = null;
    var reversalAck = new ProductionInventoryQualityInboundLedger().Accept(reversal.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("DeadLettered", reversalAck.AckStatus, "reversal without target must be dead-lettered");
    AssertTrue(reversalAck.Errors.Any(item => item.ErrorCode == "MOVEMENT_REVERSAL_TARGET_NOT_FOUND"), "reversal without target should use contract error code");

    var supersession = JsonNode.Parse(ProductionInventoryQualityEvidenceReviewedJson())!.AsObject();
    supersession["MessageID"] = "PIQE-MSG-SUPERSESSION-MISSING-TARGET";
    supersession["IdempotencyKey"] = "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1:supersession-missing-target";
    supersession["Payload"]!["EvidenceStatus"] = "Corrected";
    var supersessionAck = new ProductionInventoryQualityInboundLedger().Accept(supersession.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("DeadLettered", supersessionAck.AckStatus, "corrected evidence without predecessor must be dead-lettered");
    AssertTrue(supersessionAck.Errors.Any(item => item.ErrorCode == "SUPERSESSION_TARGET_NOT_FOUND"), "missing predecessor should use contract error code");

    var scopeViolation = JsonNode.Parse(ProductionInventoryQualityEvidenceReviewedJson())!.AsObject();
    scopeViolation["MessageID"] = "PIQE-MSG-SCOPE-VIOLATION";
    scopeViolation["IdempotencyKey"] = "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1:scope-violation";
    scopeViolation["Payload"]!["Routing"] = "not allowed";
    var scopeViolationAck = new ProductionInventoryQualityInboundLedger().Accept(scopeViolation.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", scopeViolationAck.AckStatus, "routing proof claim must be rejected");
    AssertTrue(scopeViolationAck.Errors.Any(item => item.ErrorCode == "CONTRACT_SCOPE_VIOLATION"), "out-of-scope claim should use scope violation");
}

static void TestProductionInventoryQualityEvidenceLedgerDetectsDuplicateAndConflict()
{
    var rawPayload = ProductionInventoryQualityEvidenceReviewedJson();
    var ledger = new ProductionInventoryQualityInboundLedger();

    var accepted = ledger.Accept(rawPayload);
    var duplicate = ledger.Accept(rawPayload);

    AssertEqual("Accepted", accepted.AckStatus, "first payload should be accepted");
    AssertEqual("Duplicate", duplicate.AckStatus, "same IdempotencyKey and same fingerprint should return duplicate");
    AssertEqual(1, ledger.Records.Count, "duplicate should not create another ledger record");

    var conflictNode = JsonNode.Parse(rawPayload)!.AsObject();
    conflictNode["Payload"]!["InventorySnapshot"]!["AvailableQty"] = 119;
    var conflict = ledger.Accept(conflictNode.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("DeadLettered", conflict.AckStatus, "same IdempotencyKey with different fingerprint should be dead-lettered");
    AssertTrue(conflict.Errors.Any(item => item.ErrorCode == "IDEMPOTENCY_CONFLICT"), "idempotency conflict should use contract error code");
    AssertEqual(2, ledger.Records.Count, "conflict should be retained as audit evidence");
}

static void TestProductionInventoryQualityEvidenceEndpointIsExposed()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var program = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Program.cs"));
    var domain = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "ProductionInventoryQualityEvidenceContract.cs"));

    AssertTrue(program.Contains("AddSingleton<ProductionInventoryQualityInboundLedger>", StringComparison.Ordinal), "inventory quality ledger should be registered");
    AssertTrue(program.Contains("/api/integration-contracts/production-inventory-quality-evidence-v1", StringComparison.Ordinal), "inventory quality inbound endpoint should be exposed");
    AssertTrue(program.Contains("/api/integration-contracts/production-inventory-quality-evidence-v1/ledger", StringComparison.Ordinal), "inventory quality ledger endpoint should be exposed");
    AssertTrue(domain.Contains("public sealed class ProductionInventoryQualityInboundLedger", StringComparison.Ordinal), "inventory quality ledger should exist");
    AssertTrue(domain.Contains("MISSING_INVENTORY_AUTHORITY", StringComparison.Ordinal), "consumer should enforce inventory authority rule");
    AssertTrue(domain.Contains("MISSING_QUALITY_AUTHORITY", StringComparison.Ordinal), "consumer should enforce quality authority rule");
    AssertTrue(domain.Contains("ProductionValidatedReserved", StringComparison.Ordinal), "consumer should explicitly reject reserved ProductionValidated confidence");
    AssertTrue(domain.Contains("GOVERNANCE_AUTO_UPDATE_FORBIDDEN", StringComparison.Ordinal), "consumer should enforce governance auto-update boundary");
    AssertTrue(domain.Contains("IDEMPOTENCY_CONFLICT", StringComparison.Ordinal), "consumer should enforce idempotency conflict handling");
    AssertTrue(!domain.Contains("MasterSettingsGovernanceService", StringComparison.Ordinal), "inventory quality consumer must not depend on master settings governance service");
    AssertTrue(!domain.Contains("MasterSettingChangeSaveRequest", StringComparison.Ordinal), "inventory quality consumer must not save master setting changes");
}

static void TestSdbrExecutionObjectEvidenceAcceptsReviewedContextWithoutGovernanceMutation()
{
    var rawPayload = SdbrExecutionObjectEvidenceReviewedJson();
    var ledger = new SdbrExecutionObjectEvidenceInboundLedger(() => new DateTimeOffset(2026, 6, 28, 2, 0, 0, TimeSpan.Zero));

    var ack = ledger.Accept(rawPayload);

    SdbrExecutionObjectEvidenceInboundLedger.ValidateAckShape(ack);
    AssertEqual("SDBR-EXECUTION-OBJECT-EVIDENCE-V1", ack.ContractID, "execution object ACK contract id");
    AssertEqual("1.0.0", ack.ContractVersion, "execution object ACK contract version");
    AssertEqual("Accepted", ack.AckStatus, "reviewed execution object evidence should be accepted as controlled context");
    AssertEqual("DDAE", ack.ConsumerSystem, "consumer system");
    AssertEqual("MSG-SDBR-EOE-20260628-001", ack.MessageID, "message id");
    AssertEqual("TRACE-SDBR-EOE-20260628-001", ack.TraceableID, "traceable id");
    AssertEqual(null, ack.ErrorCode, "accepted ACK should not include error code");
    AssertEqual(1, ledger.Records.Count, "accepted payload should be stored in ledger");
    AssertEqual(rawPayload, ledger.Records[0].RawPayload, "ledger must preserve raw payload exactly");
    AssertEqual("sha256:", ledger.Records[0].PayloadFingerprint[..7], "ledger should store deterministic payload fingerprint");
    AssertEqual("Reviewed", ledger.Records[0].Interpretation.EvidenceStatus, "fixture must remain reviewed context");
    AssertEqual("Reviewed", ledger.Records[0].Interpretation.EvidenceConfidence, "fixture must remain reviewed confidence");
    AssertEqual("SDBR-PLAN-RUN-20260628-001", ledger.Records[0].Interpretation.PlanningRunID, "planning run should be interpreted");
    AssertEqual("OMC-SAT-BUS-001-20260628-001", ledger.Records[0].Interpretation.OperatingModelConfigurationID, "operating model config should be interpreted");
    AssertEqual("WO-SUB-AVIONICS-COMPUTE-001", ledger.Records[0].Interpretation.WorkOrderID, "work order should be interpreted");
    AssertEqual("SDBR_EXECUTABLE_ROUTING", ledger.Records[0].Interpretation.RoutingAuthority, "routing authority should be interpreted");
    AssertTrue(!ledger.Records[0].Interpretation.AllowsAutomaticOperatingModelUpdate, "contract should not allow automatic operating model update");
    AssertTrue(!ledger.Records[0].Interpretation.AllowsAutomaticMasterSettingUpdate, "contract should not allow automatic master setting update");
    AssertTrue(!ledger.Records[0].Interpretation.AllowsAutomaticBufferUpdate, "contract should not allow automatic buffer update");
    AssertTrue(!ledger.Records[0].Interpretation.AllowsAutomaticSupplierSourceFactUpdate, "contract should not allow automatic supplier source update");
    AssertTrue(!ledger.Records[0].Interpretation.AllowsAutomaticLeadTimeUpdate, "contract should not allow automatic lead time update");
    AssertTrue(!ledger.Records[0].Interpretation.AllowsAutomaticMOQUpdate, "contract should not allow automatic MOQ update");
    AssertTrue(!ledger.Records[0].Interpretation.AllowsAutomaticOrderCycleUpdate, "contract should not allow automatic order cycle update");
    AssertTrue(ledger.Records[0].Interpretation.RequiresSeparateDdaeApproval, "contract should require separate DDAE approval");
    AssertTrue(!ledger.Records[0].Interpretation.MutatedDdaeGovernance, "consumer must not mutate DDAE governance");

    var acceptedAckExample = JsonNode.Parse(SdbrExecutionObjectAcceptedAckJson())!.AsObject();
    var exampleAck = new SdbrExecutionObjectEvidenceAck(
        acceptedAckExample["ContractID"]!.GetValue<string>(),
        acceptedAckExample["ContractVersion"]!.GetValue<string>(),
        acceptedAckExample["AckID"]!.GetValue<string>(),
        acceptedAckExample["MessageID"]!.GetValue<string>(),
        acceptedAckExample["IdempotencyKey"]!.GetValue<string>(),
        acceptedAckExample["ConsumerSystem"]!.GetValue<string>(),
        acceptedAckExample["AckStatus"]!.GetValue<string>(),
        acceptedAckExample["ErrorCode"]?.GetValue<string>(),
        acceptedAckExample["ErrorMessage"]?.GetValue<string>(),
        acceptedAckExample["ReceivedAt"]!.GetValue<string>(),
        acceptedAckExample["Retryable"]!.GetValue<bool>(),
        acceptedAckExample["TraceableID"]!.GetValue<string>());
    SdbrExecutionObjectEvidenceInboundLedger.ValidateAckShape(exampleAck);
}

static void TestSdbrExecutionObjectEvidenceRejectsBoundaryAndValidationViolations()
{
    var missingWorkOrder = JsonNode.Parse(SdbrExecutionObjectEvidenceReviewedJson())!.AsObject();
    missingWorkOrder["MessageID"] = "MSG-SDBR-EOE-MISSING-WORK-ORDER";
    missingWorkOrder["IdempotencyKey"] = "SDBR-EXECUTION-OBJECT-EVIDENCE-V1:missing-work-order";
    missingWorkOrder["Payload"]!["WorkOrder"] = null;
    var missingWorkOrderAck = new SdbrExecutionObjectEvidenceInboundLedger().Accept(missingWorkOrder.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", missingWorkOrderAck.AckStatus, "missing work order must be rejected");
    AssertEqual("MISSING_WORK_ORDER", missingWorkOrderAck.ErrorCode, "missing work order should use contract error");

    var badRouting = JsonNode.Parse(SdbrExecutionObjectEvidenceReviewedJson())!.AsObject();
    badRouting["MessageID"] = "MSG-SDBR-EOE-BAD-ROUTING";
    badRouting["IdempotencyKey"] = "SDBR-EXECUTION-OBJECT-EVIDENCE-V1:bad-routing";
    badRouting["Payload"]!["Routing"]!["RoutingID"] = "DDAE-PRIMARY-ROUTING-001";
    badRouting["Payload"]!["Routing"]!["RoutingAuthority"] = "DDAE_PLANNING_ROUTING";
    var badRoutingAck = new SdbrExecutionObjectEvidenceInboundLedger().Accept(badRouting.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("DeadLettered", badRoutingAck.AckStatus, "DDAE planning routing cannot be executable SDBR routing proof");
    AssertEqual("UNKNOWN_ROUTING", badRoutingAck.ErrorCode, "bad routing should use contract error");

    var missingPlanningRun = JsonNode.Parse(SdbrExecutionObjectEvidenceReviewedJson())!.AsObject();
    missingPlanningRun["MessageID"] = "MSG-SDBR-EOE-MISSING-PLANNING-RUN";
    missingPlanningRun["IdempotencyKey"] = "SDBR-EXECUTION-OBJECT-EVIDENCE-V1:missing-planning-run";
    missingPlanningRun["Payload"]!["PlanningContext"]!["PlanningRunID"] = "";
    var missingPlanningRunAck = new SdbrExecutionObjectEvidenceInboundLedger().Accept(missingPlanningRun.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", missingPlanningRunAck.AckStatus, "missing PlanningRunID must be rejected");
    AssertEqual("MISSING_PLANNING_RUN", missingPlanningRunAck.ErrorCode, "missing planning run should use contract error");

    var missingFrozenConfig = JsonNode.Parse(SdbrExecutionObjectEvidenceReviewedJson())!.AsObject();
    missingFrozenConfig["MessageID"] = "MSG-SDBR-EOE-MISSING-FROZEN-CONFIG";
    missingFrozenConfig["IdempotencyKey"] = "SDBR-EXECUTION-OBJECT-EVIDENCE-V1:missing-frozen-config";
    missingFrozenConfig["Payload"]!["PlanningContext"]!["OperatingModelConfigurationID"] = "";
    var missingFrozenConfigAck = new SdbrExecutionObjectEvidenceInboundLedger().Accept(missingFrozenConfig.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", missingFrozenConfigAck.AckStatus, "missing frozen config must be rejected");
    AssertEqual("MISSING_FROZEN_CONFIG", missingFrozenConfigAck.ErrorCode, "missing frozen config should use contract error");

    var frozenMismatch = JsonNode.Parse(SdbrExecutionObjectEvidenceReviewedJson())!.AsObject();
    frozenMismatch["MessageID"] = "MSG-SDBR-EOE-FROZEN-CONFIG-MISMATCH";
    frozenMismatch["IdempotencyKey"] = "SDBR-EXECUTION-OBJECT-EVIDENCE-V1:frozen-config-mismatch";
    frozenMismatch["Payload"]!["PlanningContext"]!["OperatingModelFingerprint"] = "sha256:not-the-reviewed-fixture-baseline";
    var frozenMismatchAck = new SdbrExecutionObjectEvidenceInboundLedger().Accept(frozenMismatch.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", frozenMismatchAck.AckStatus, "changed frozen config field must be rejected");
    AssertEqual("FROZEN_CONFIG_MISMATCH", frozenMismatchAck.ErrorCode, "changed frozen config should use mismatch error");

    var materialRequirementAsIssue = JsonNode.Parse(SdbrExecutionObjectEvidenceReviewedJson())!.AsObject();
    materialRequirementAsIssue["MessageID"] = "MSG-SDBR-EOE-MATERIAL-REQ-AS-ISSUE";
    materialRequirementAsIssue["IdempotencyKey"] = "SDBR-EXECUTION-OBJECT-EVIDENCE-V1:material-req-as-issue";
    materialRequirementAsIssue["Payload"]!["MaterialRequirements"]![0]!["IssuedQty"] = 63;
    var materialRequirementAsIssueAck = new SdbrExecutionObjectEvidenceInboundLedger().Accept(materialRequirementAsIssue.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", materialRequirementAsIssueAck.AckStatus, "material requirement must not be treated as issue proof");
    AssertEqual("CONTRACT_SCOPE_VIOLATION", materialRequirementAsIssueAck.ErrorCode, "requirement-as-issue should use scope violation");

    var issueWithoutInventoryQuality = JsonNode.Parse(SdbrExecutionObjectEvidenceReviewedJson())!.AsObject();
    issueWithoutInventoryQuality["MessageID"] = "MSG-SDBR-EOE-ISSUE-MISSING-IQ";
    issueWithoutInventoryQuality["IdempotencyKey"] = "SDBR-EXECUTION-OBJECT-EVIDENCE-V1:issue-missing-iq";
    issueWithoutInventoryQuality["Payload"]!["MaterialIssues"] = new JsonArray
    {
        new JsonObject
        {
            ["IssueEventID"] = "ISSUE-PART-FPGA-SPACE-001",
            ["IssueState"] = "Issued",
            ["ConsumedItemID"] = "PART-FPGA-SPACE",
            ["IssueLocationID"] = "WH-ELEC-QA",
            ["LotID"] = "LOT-FPGA-001",
            ["SerialID"] = null,
            ["BatchID"] = null,
            ["IssuedQty"] = 1,
            ["QuantityUOM"] = "EA",
            ["UOMAuthority"] = "SDBR-MASTER-DATA",
            ["IssuedAt"] = "2026-07-15T08:00:00+08:00",
            ["IssueSourceSystem"] = "SDBR",
            ["IssueDocumentID"] = "ISSUE-DOC-001",
            ["InventoryQualityEvidencePackageID"] = null,
            ["InventoryQualityEvidenceVersion"] = null,
            ["IssueAuthorityReferenceID"] = null,
            ["ReversesIssueEventID"] = null
        }
    };
    var issueWithoutInventoryQualityAck = new SdbrExecutionObjectEvidenceInboundLedger().Accept(issueWithoutInventoryQuality.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", issueWithoutInventoryQualityAck.AckStatus, "issue without inventory-quality or authority reference must be rejected");
    AssertEqual("MISSING_INVENTORY_QUALITY_EVIDENCE", issueWithoutInventoryQualityAck.ErrorCode, "missing issue dependency should use contract error");

    var completedWithoutLateCapture = JsonNode.Parse(SdbrExecutionObjectEvidenceReviewedJson())!.AsObject();
    completedWithoutLateCapture["MessageID"] = "MSG-SDBR-EOE-COMPLETED-WITHOUT-LATE-CAPTURE";
    completedWithoutLateCapture["IdempotencyKey"] = "SDBR-EXECUTION-OBJECT-EVIDENCE-V1:completed-without-late-capture";
    completedWithoutLateCapture["Payload"]!["WorkOrder"]!["WorkOrderStatus"] = "Completed";
    completedWithoutLateCapture["Payload"]!["WorkOrder"]!["CompletedQty"] = 1;
    completedWithoutLateCapture["Payload"]!["WorkOrder"]!["RemainingQty"] = 0;
    var completedWithoutLateCaptureAck = new SdbrExecutionObjectEvidenceInboundLedger().Accept(completedWithoutLateCapture.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", completedWithoutLateCaptureAck.AckStatus, "completed without started evidence and late-capture reconciliation must be rejected");
    AssertEqual("EVENT_ORDER_INVALID", completedWithoutLateCaptureAck.ErrorCode, "late-capture violation should use event order error");

    var reservedConfidence = JsonNode.Parse(SdbrExecutionObjectEvidenceReviewedJson())!.AsObject();
    reservedConfidence["MessageID"] = "MSG-SDBR-EOE-PRODUCTION-VALIDATED-RESERVED";
    reservedConfidence["IdempotencyKey"] = "SDBR-EXECUTION-OBJECT-EVIDENCE-V1:production-validated-reserved";
    reservedConfidence["Payload"]!["EvidenceConfidence"] = "ProductionValidatedReserved";
    var reservedConfidenceAck = new SdbrExecutionObjectEvidenceInboundLedger().Accept(reservedConfidence.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", reservedConfidenceAck.AckStatus, "ProductionValidatedReserved must not be accepted in V1");
    AssertEqual("CONTRACT_SCOPE_VIOLATION", reservedConfidenceAck.ErrorCode, "reserved production validation should use scope violation");

    var autoUpdate = JsonNode.Parse(SdbrExecutionObjectEvidenceReviewedJson())!.AsObject();
    autoUpdate["MessageID"] = "MSG-SDBR-EOE-AUTO-UPDATE";
    autoUpdate["IdempotencyKey"] = "SDBR-EXECUTION-OBJECT-EVIDENCE-V1:auto-update";
    autoUpdate["Payload"]!["DDAEGovernanceBoundary"]!["AllowsAutomaticMasterSettingUpdate"] = true;
    var autoUpdateAck = new SdbrExecutionObjectEvidenceInboundLedger().Accept(autoUpdate.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", autoUpdateAck.AckStatus, "automatic DDAE governance update must be rejected");
    AssertEqual("GOVERNANCE_AUTO_UPDATE_FORBIDDEN", autoUpdateAck.ErrorCode, "automatic update should use governance error");

    foreach (var marker in new[]
             {
                 "IsProductionValidated",
                 "BusinessGoldenLoopReady",
                 "AllowsProductionValidation",
                 "ClaimsSupplierExecution",
                 "ClaimsDeliveryPerformance",
                 "ClaimsLeadTimePerformance",
                 "ClaimsProductionInventoryAuthority",
                 "ClaimsProductionQualityAuthority"
             })
    {
        var markerClaim = JsonNode.Parse(SdbrExecutionObjectEvidenceReviewedJson())!.AsObject();
        markerClaim["MessageID"] = $"MSG-SDBR-EOE-{marker.ToUpperInvariant()}";
        markerClaim["IdempotencyKey"] = $"SDBR-EXECUTION-OBJECT-EVIDENCE-V1:{marker}";
        markerClaim["Payload"]![marker] = true;
        var markerClaimAck = new SdbrExecutionObjectEvidenceInboundLedger().Accept(markerClaim.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

        AssertEqual("Rejected", markerClaimAck.AckStatus, $"{marker} must be rejected");
        AssertEqual("CONTRACT_SCOPE_VIOLATION", markerClaimAck.ErrorCode, $"{marker} should use scope violation");
    }
}

static void TestSdbrExecutionObjectEvidenceAcceptsReconciledLateCapture()
{
    var lateCaptured = JsonNode.Parse(SdbrExecutionObjectEvidenceReviewedJson())!.AsObject();
    lateCaptured["MessageID"] = "MSG-SDBR-EOE-LATE-CAPTURED-COMPLETE";
    lateCaptured["IdempotencyKey"] = "SDBR-EXECUTION-OBJECT-EVIDENCE-V1:late-captured-complete";
    lateCaptured["Payload"]!["WorkOrder"]!["WorkOrderStatus"] = "Completed";
    lateCaptured["Payload"]!["WorkOrder"]!["CompletedQty"] = 1;
    lateCaptured["Payload"]!["WorkOrder"]!["RemainingQty"] = 0;
    lateCaptured["Payload"]!["WorkOrder"]!["EventCaptureMode"] = "LateCaptured";
    lateCaptured["Payload"]!["WorkOrder"]!["LateCaptureReason"] = "Reviewed fixture reconciliation record supplied by SDBR.";
    lateCaptured["Payload"]!["WorkOrder"]!["ReconciliationReferenceID"] = "RECON-SDBR-EOE-20260628-001";
    lateCaptured["Payload"]!["WorkOrder"]!["ObservedAt"] = "2026-07-16T18:00:00+08:00";
    lateCaptured["Payload"]!["WorkOrder"]!["RecordedAt"] = "2026-07-16T18:15:00+08:00";

    var ack = new SdbrExecutionObjectEvidenceInboundLedger().Accept(lateCaptured.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Accepted", ack.AckStatus, "complete late-captured fixture should be accepted as reviewed context only");
    AssertEqual(null, ack.ErrorCode, "accepted late-captured fixture should not carry an error");
}

static void TestSdbrExecutionObjectEvidenceRejectsConsumptionWithoutInventoryQuality()
{
    var consumptionWithoutInventoryQuality = JsonNode.Parse(SdbrExecutionObjectEvidenceReviewedJson())!.AsObject();
    consumptionWithoutInventoryQuality["MessageID"] = "MSG-SDBR-EOE-CONSUMPTION-MISSING-IQ";
    consumptionWithoutInventoryQuality["IdempotencyKey"] = "SDBR-EXECUTION-OBJECT-EVIDENCE-V1:consumption-missing-iq";
    consumptionWithoutInventoryQuality["Payload"]!["MaterialConsumptions"] = new JsonArray
    {
        new JsonObject
        {
            ["ConsumptionEventID"] = "CONS-PART-FPGA-SPACE-001",
            ["ConsumptionState"] = "Consumed",
            ["ConsumedByOperationID"] = "OP-INSTALL-FPGA-SPACE-001",
            ["ConsumedItemID"] = "PART-FPGA-SPACE",
            ["ConsumedLocationID"] = "WH-ELEC-QA",
            ["ConsumedQty"] = 1,
            ["ScrapQty"] = 0,
            ["RejectQty"] = 0,
            ["RemainingQty"] = 62,
            ["QuantityUOM"] = "EA",
            ["UOMAuthority"] = "SDBR-MASTER-DATA",
            ["ConsumedAt"] = "2026-07-15T10:00:00+08:00",
            ["ConsumptionSourceSystem"] = "SDBR",
            ["ConsumptionMethod"] = "ReviewedFixtureOnly",
            ["InventoryQualityEvidencePackageID"] = null,
            ["InventoryQualityEvidenceVersion"] = null,
            ["ConsumptionAuthorityReferenceID"] = null,
            ["ReversesConsumptionEventID"] = null
        }
    };

    var ack = new SdbrExecutionObjectEvidenceInboundLedger().Accept(consumptionWithoutInventoryQuality.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("Rejected", ack.AckStatus, "consumption without inventory-quality or accepted authority reference must be rejected");
    AssertEqual("MISSING_INVENTORY_QUALITY_EVIDENCE", ack.ErrorCode, "missing consumption dependency should use contract error");
}

static void TestSdbrExecutionObjectEvidenceLedgerDetectsDuplicateAndConflict()
{
    var rawPayload = SdbrExecutionObjectEvidenceReviewedJson();
    var ledger = new SdbrExecutionObjectEvidenceInboundLedger();

    var accepted = ledger.Accept(rawPayload);
    var duplicate = ledger.Accept(rawPayload);

    AssertEqual("Accepted", accepted.AckStatus, "first payload should be accepted");
    AssertEqual("Duplicate", duplicate.AckStatus, "same IdempotencyKey and same fingerprint should return duplicate");
    AssertEqual(1, ledger.Records.Count, "duplicate should not create another ledger record");

    var conflictNode = JsonNode.Parse(rawPayload)!.AsObject();
    conflictNode["Payload"]!["WorkOrder"]!["RequiredQty"] = 2;
    var conflict = ledger.Accept(conflictNode.ToJsonString(DdsopConfigInboundContractService.ContractJsonOptions));

    AssertEqual("DeadLettered", conflict.AckStatus, "same IdempotencyKey with different fingerprint should be dead-lettered");
    AssertEqual("IDEMPOTENCY_CONFLICT", conflict.ErrorCode, "idempotency conflict should use contract error code");
    AssertEqual(2, ledger.Records.Count, "conflict should be retained as audit evidence");
}

static void TestSdbrExecutionObjectEvidenceEndpointIsExposed()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var program = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Program.cs"));
    var domain = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "SdbrExecutionObjectEvidenceContract.cs"));

    AssertTrue(program.Contains("AddSingleton<SdbrExecutionObjectEvidenceInboundLedger>", StringComparison.Ordinal), "execution object ledger should be registered");
    AssertTrue(program.Contains("/api/integration-contracts/sdbr-execution-object-evidence-v1", StringComparison.Ordinal), "execution object inbound endpoint should be exposed");
    AssertTrue(program.Contains("/api/integration-contracts/sdbr-execution-object-evidence-v1/ledger", StringComparison.Ordinal), "execution object ledger endpoint should be exposed");
    AssertTrue(domain.Contains("public sealed class SdbrExecutionObjectEvidenceInboundLedger", StringComparison.Ordinal), "execution object ledger should exist");
    AssertTrue(domain.Contains("SDBR_EXECUTABLE_ROUTING", StringComparison.Ordinal), "consumer should enforce SDBR executable routing authority");
    AssertTrue(domain.Contains("ProductionValidatedReserved", StringComparison.Ordinal), "consumer should reject reserved ProductionValidated confidence");
    AssertTrue(domain.Contains("MISSING_INVENTORY_QUALITY_EVIDENCE", StringComparison.Ordinal), "consumer should enforce issue/consumption dependency");
    AssertTrue(domain.Contains("EventCaptureMode", StringComparison.Ordinal), "consumer should enforce late-capture fields");
    AssertTrue(domain.Contains("GOVERNANCE_AUTO_UPDATE_FORBIDDEN", StringComparison.Ordinal), "consumer should enforce governance auto-update boundary");
    AssertTrue(domain.Contains("IDEMPOTENCY_CONFLICT", StringComparison.Ordinal), "consumer should enforce idempotency conflict handling");
    AssertTrue(!domain.Contains("MasterSettingsGovernanceService", StringComparison.Ordinal), "execution object consumer must not depend on master settings governance service");
    AssertTrue(!domain.Contains("MasterSettingChangeSaveRequest", StringComparison.Ordinal), "execution object consumer must not save master setting changes");
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
    AssertTrue(result.SkuDetails.All(item => item.Activities.Count > 0), "SKU detail should include simulation activities");
    AssertTrue(result.SkuDetails.All(item => item.Attributes.Count > 0), "SKU detail should include SKU attributes");
    AssertTrue(result.SkuDetails.All(item => item.BufferSizing.Count >= 7), "SKU detail should include buffer sizing lines");
    AssertTrue(result.SkuDetails.All(item => item.Bom.Count > 0), "SKU detail should include BOM components");
    AssertTrue(result.SkuDetails.All(item => item.OrderDetails.Count > 0), "SKU detail should include order details");
    AssertTrue(result.SkuDetails.Any(item => item.Activities.Any(activity => activity.ActivityType == "订货周期复核")), "activities should explain order cycle review waits");
    AssertTrue(result.SkuDetails.Any(item => item.BufferSizing.Any(line => line.Formula.Contains("ADU", StringComparison.Ordinal))), "buffer sizing should expose DDMRP formulas");
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
    AssertTrue(scenarioDetail.Activities.Any(item => item.ActivityType == "提前建库"), "pre-build should appear in single SKU activities");
    AssertTrue(scenarioDetail.OrderDetails.Any(item => item.OrderType == "提前建库订单"), "pre-build should appear in single SKU order details");
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

static void TestCandidateActionCombinationServiceUsesSolverAdapter()
{
    var source = new TrackingScenarioWorkspaceDataSource(SeedData.Create());
    var preview = new ScenarioRunPreviewService(source);
    var whiteBoxGateway = new LocalDdsopWhiteBoxScenarioGateway(preview);
    var scoring = new NetworkStructureScoringService(source, new NetworkMetricsService(source));
    var requestBuilder = new NetworkCandidateRecalculationRequestBuilder();
    var validation = new NetworkScenarioValidationService(source, source, scoring, requestBuilder, whiteBoxGateway);
    var solver = new CapturingOptimizationSolver();
    var selector = new CandidateActionCombinationSelector(new[] { solver });
    var service = new CandidateActionCombinationService(validation, whiteBoxGateway, selector);

    var result = service.Select(new CandidateActionCombinationRequest(
        12,
        SolverName: "FakeSolver"));

    AssertTrue(source.LoadCount > 0, "candidate combination service should load data through IScenarioWorkspaceDataSource");
    AssertEqual(3, solver.CallCount, "solver should be called once per combination profile");
    AssertEqual(3, result.Combinations.Count, "combination selection should return three management profiles");
    AssertTrue(result.Combinations.All(item => item.WhiteBoxPreviewResult is not null), "candidate combination should include recalculated preview result");
    AssertTrue(result.Combinations.All(item => item.WhiteBoxRecalculationRequest.Parameters is not null), "candidate combination should include runnable white-box recalculation request");
    AssertTrue(result.Combinations.Select(item => item.ProfileId).ToHashSet().SetEquals(new[] { "ServiceFirst", "CashFirst", "CapacityFirst" }), "candidate combination profiles should cover service cash and capacity");
    AssertTrue(result.CandidateImpactMatrix.Count > 0, "combination selection should expose candidate action impact matrix");
    AssertTrue(result.CandidateImpactMatrix.All(item => item.EstimatedCost >= 0m && !string.IsNullOrWhiteSpace(item.ConstraintNote)), "candidate matrix should include cost and constraints");
    AssertEqual(result.Combinations.Count, result.CombinationComparisons.Count, "combination selection should expose one comparison per candidate combination");
    AssertTrue(result.Combinations.All(item => item.Comparison is not null && item.EstimatedActionCost >= 0m), "candidate combination should include comparison and estimated cost");
    AssertTrue(result.Combinations.All(item => item.Trace.Any(trace => trace.Message.Contains("白盒", StringComparison.Ordinal))), "combination trace should state white-box recalculation");
    AssertTrue(result.Trace.Any(item => item.Message.Contains("求解器只选择候选动作组合", StringComparison.Ordinal)), "trace should state that solver only selects action combinations");
    AssertTrue(result.Message.Contains("候选动作组合", StringComparison.Ordinal), "service should not return old solver recommendation wording");
    AssertTrue(!result.Message.Contains("优化推荐", StringComparison.Ordinal), "service should not return old solver recommendation wording");
    AssertTrue(!result.IsPersisted, "candidate combination should not be persisted");
    AssertTrue(solver.LastProblem?.Candidates.Count > 0, "solver problem should include candidates");
    AssertTrue(solver.LastProblem?.CostBudget > 0m, "solver problem should include cost budget boundary");
}

static void TestOrToolsOptimizationSolverSolvesToyProblem()
{
    var solver = new OrToolsOptimizationSolver();
    var problem = new OptimizationProblem(
        "toy-ortools-problem",
        "ServiceFirst",
        1,
        10,
        100,
        new[]
        {
            new OptimizationCandidate("low", "测试动作", "A", "A", 1, 0, 1, "测试约束", "可进入方案评审", "低收益动作"),
            new OptimizationCandidate("high", "测试动作", "B", "B", 10, 0, 1, "测试约束", "可进入方案评审", "高收益动作")
        });
    var result = solver.Solve(problem);

    AssertEqual("OR-Tools", result.SolverName, "OR-Tools solver name");
    AssertTrue(result.Status is OptimizationSolverStatus.Optimal or OptimizationSolverStatus.Feasible, "OR-Tools adapter should solve toy problem");
    AssertContains(result.SelectedCandidateIds, "high", "OR-Tools adapter should choose highest value candidate");
}

static void TestGurobiOptimizationSolverToyProblem()
{
    var solver = new GurobiOptimizationSolver();
    var problem = new OptimizationProblem(
        "toy-problem",
        "ServiceFirst",
        1,
        10,
        100,
        new[]
        {
            new OptimizationCandidate("low", "测试动作", "A", "A", 1, 0, 1, "测试约束", "可进入方案评审", "低收益动作"),
            new OptimizationCandidate("high", "测试动作", "B", "B", 10, 0, 1, "测试约束", "可进入方案评审", "高收益动作")
        });
    var result = solver.Solve(problem);

    if (result.Status == OptimizationSolverStatus.Unavailable)
    {
        AssertContains(new[] { result.Message }, "Gurobi", "unavailable result should explain Gurobi availability");
        return;
    }

    AssertTrue(result.Status is OptimizationSolverStatus.Optimal or OptimizationSolverStatus.Feasible, "toy problem should solve when Gurobi is available");
    AssertContains(result.SelectedCandidateIds, "high", "solver should choose highest value candidate");
}

static string ProductionSupplierIdentitySourceReviewedJson()
{
    var contractExamplePath = @"D:\Documents\DDAE_INTERFACE_CONTRACT\contracts\production-supplier-identity-source-v1\examples\sup-microchip-part-fpga-space-reviewed.json";
    if (File.Exists(contractExamplePath))
    {
        return File.ReadAllText(contractExamplePath);
    }

    return """
        {
          "ContractID": "PRODUCTION-SUPPLIER-IDENTITY-SOURCE-V1",
          "ContractVersion": "1.0.0",
          "MessageID": "PSISV1-MSG-SUP-MICROCHIP-PART-FPGA-SPACE-20260628-001",
          "IdempotencyKey": "PRODUCTION-SUPPLIER-IDENTITY-SOURCE-V1:ContractAgentAcceptedSource:PSISR-SUP-MICROCHIP-PART-FPGA-SPACE-001:1:2026-06-28T00:00:00+08:00",
          "ProducerSystem": "ContractAgentAcceptedSource",
          "ConsumerSystems": ["DDAE", "SDBR", "ContractAgent"],
          "OccurredAt": "2026-06-28T10:30:00+08:00",
          "TimeZone": "Asia/Shanghai",
          "Payload": {
            "EvidencePackageID": "PSIS-EP-SUP-MICROCHIP-PART-FPGA-SPACE-20260628-001",
            "EvidenceVersion": "1",
            "EvidenceStatus": "Reviewed",
            "EvidenceConfidence": "Reviewed",
            "SourceAuthority": {
              "AuthoritySystemID": "ContractAgentAcceptedSource",
              "AuthoritySystemType": "ContractAgentAcceptedSource",
              "AuthorityRecordID": "CTRL-SUPPLIER-SOURCE-REVIEW-20260628-001",
              "AuthorityOwner": "Contract Agent controlled review",
              "AuthorityConfidence": "Reviewed"
            },
            "SupplierIdentity": {
              "SupplierID": "SUP-MICROCHIP",
              "SupplierName": "Microchip Space",
              "SupplierIdentityStatus": "Reviewed",
              "SupplierIdentityAuthority": "ContractAgentAcceptedSource",
              "ApprovedSupplierStatus": "Reviewed"
            },
            "SupplierSourceRelation": {
              "SupplierSourceRelationID": "PSISR-SUP-MICROCHIP-PART-FPGA-SPACE-001",
              "SupplierID": "SUP-MICROCHIP",
              "ItemID": "PART-FPGA-SPACE",
              "LocationID": "WH-ELEC-QA",
              "ProgramID": "SAT-BUS-001",
              "SourceType": "Primary",
              "EligibilityStatus": "Reviewed",
              "EffectiveFrom": "2026-06-28T00:00:00+08:00",
              "EffectiveTo": null,
              "SourcePriority": 1
            },
            "SourceTerms": {
              "SourceTermsAuthority": "ContractAgentAcceptedSource",
              "UOM": "EA",
              "UOMAuthority": "ContractAgentAcceptedSource",
              "TermsAreProductionAuthoritative": false,
              "DDAEPlanningAssumptionOnly": true,
              "LeadTimeDays": 30,
              "MOQ": 1,
              "OrderCycleDays": 7
            },
            "DDAEGovernanceBoundary": {
              "AllowsAutomaticMasterSettingUpdate": false,
              "MayCreateReviewTopic": true,
              "MayCreateGovernanceProposal": false,
              "RequiresSeparateDDAEApproval": true
            },
            "Traceability": {
              "SourceSystem": "ContractAgentAcceptedSource",
              "SourceDocumentType": "ControlledSupplierSourceReview",
              "SourceDocumentID": "CTRL-SUPPLIER-SOURCE-REVIEW-20260628-001",
              "SourceDocumentLineID": "001",
              "SourceRecordID": "USER-SUPPLIER-SOURCE-REVIEW-20260628-001",
              "TraceableID": "TRACE-SUP-MICROCHIP-PART-FPGA-SPACE-SOURCE-20260628-001",
              "RegisteredAt": "2026-06-28T10:30:00+08:00",
              "ApprovedAt": null,
              "ApprovedBy": null
            },
            "Supersession": {
              "SupersedesEvidencePackageID": null,
              "SupersedesEvidenceVersion": null,
              "CorrectionReason": null,
              "SupersessionReason": null
            },
            "NonClaims": [
              "NotProductionValidated",
              "NotInventoryBalanceProof",
              "NotQualityReleaseProof",
              "NotSupplierExecutionProof",
              "NotDeliveryPerformanceProof",
              "NotLeadTimePerformanceProof",
              "NoAutomaticDDAEMasterSettingUpdate",
              "NotBusinessGoldenLoopReady"
            ]
          }
        }
        """;
}

static string ProductionInventoryQualityEvidenceReviewedJson()
{
    var contractExamplePath = @"D:\Documents\DDAE_INTERFACE_CONTRACT\contracts\production-inventory-quality-evidence-v1\examples\part-fpga-space-reviewed-inventory-quality.json";
    if (File.Exists(contractExamplePath))
    {
        return File.ReadAllText(contractExamplePath);
    }

    return """
        {
          "ContractID": "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1",
          "ContractVersion": "1.0.0",
          "MessageID": "PIQEV1-MSG-PART-FPGA-SPACE-20260628-001",
          "IdempotencyKey": "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1:ContractAgentAcceptedSource:PIQE-PART-FPGA-SPACE-20260628-001:1:SDBR-FPGA-MATERIAL-AVAILABILITY-SNAPSHOT-20260627-001",
          "ProducerSystem": "ContractAgentAcceptedSource",
          "ConsumerSystems": ["DDAE", "SDBR", "ContractAgent"],
          "OccurredAt": "2026-06-28T11:30:00+08:00",
          "TimeZone": "Asia/Shanghai",
          "Payload": {
            "EvidencePackageID": "PIQE-PART-FPGA-SPACE-20260628-001",
            "EvidenceVersion": "1",
            "EvidenceStatus": "Reviewed",
            "EvidenceConfidence": "Reviewed",
            "InventoryAuthority": {
              "AuthoritySystemID": "ContractAgentAcceptedSource",
              "AuthoritySystemType": "ContractAgentAcceptedSource",
              "AuthorityRecordID": "CTRL-INVENTORY-QUALITY-REVIEW-20260628-001",
              "AuthorityOwner": "Contract Agent controlled review",
              "AuthorityConfidence": "Reviewed"
            },
            "QualityAuthority": {
              "AuthoritySystemID": "ContractAgentAcceptedSource",
              "AuthoritySystemType": "ContractAgentAcceptedSource",
              "AuthorityRecordID": "CTRL-QA-RELEASE-FPGA-20260712-001",
              "AuthorityOwner": "Contract Agent controlled review",
              "AuthorityConfidence": "Reviewed"
            },
            "ItemLocation": {
              "ItemID": "PART-FPGA-SPACE",
              "LocationID": "WH-ELEC-QA",
              "SublocationID": null,
              "LotID": null,
              "SerialID": null,
              "ProgramID": "SAT-BUS-001",
              "QuantityUOM": "EA",
              "UOMAuthority": "ContractAgentAcceptedSource",
              "ItemLocationStatus": "Reviewed"
            },
            "InventorySnapshot": {
              "SnapshotID": "SDBR-FPGA-MATERIAL-AVAILABILITY-SNAPSHOT-20260627-001",
              "SnapshotTimestamp": "2026-06-27T20:00:00+08:00",
              "OnHandQty": 120,
              "AvailableQty": 120,
              "AllocatedQty": 30,
              "AvailableAfterAllocationQty": 90,
              "InboundQty": 80,
              "InspectionQty": 0,
              "QuarantineQty": 0,
              "QualityReleasedQty": 80,
              "RejectedQty": 0,
              "BlockedQty": 0,
              "ReservedQty": 0
            },
            "StockMovements": [
              {
                "MovementID": "PIQE-MOVE-PART-FPGA-SPACE-QA-RELEASE-20260712-001",
                "MovementType": "InspectionRelease",
                "MovementState": "QualityReleased",
                "MovementQty": 80,
                "MovementUOM": "EA",
                "MovementTimestamp": "2026-07-12T15:00:00+08:00",
                "FromLocationID": "WH-IQC",
                "ToLocationID": "WH-ELEC-QA",
                "ReversesMovementID": null
              }
            ],
            "QualityEvidence": {
              "InspectionID": "CTRL-RECEIPT-FPGA-20260710-001",
              "InspectionStatus": "QualityReleased",
              "QualityReleaseID": "CTRL-QA-RELEASE-FPGA-20260712-001",
              "QualityReleaseStatus": "QualityReleased",
              "QualityReleasedAt": "2026-07-12T15:00:00+08:00",
              "RejectedAt": null,
              "RejectedReason": null
            },
            "DDAEGovernanceBoundary": {
              "AllowsAutomaticMasterSettingUpdate": false,
              "MayCreateReviewTopic": true,
              "MayCreateGovernanceProposal": false,
              "RequiresSeparateDDAEApproval": true
            },
            "Traceability": {
              "SourceSystem": "ContractAgentAcceptedSource",
              "SourceDocumentType": "ControlledInventoryQualityReview",
              "SourceDocumentID": "CTRL-INVENTORY-QUALITY-REVIEW-20260628-001",
              "SourceDocumentLineID": "001",
              "SourceRecordID": "USER-INVENTORY-QUALITY-REVIEW-20260628-001",
              "TraceableID": "TRACE-PART-FPGA-SPACE-WH-ELEC-QA-INVENTORY-QUALITY-20260628-001",
              "RegisteredAt": "2026-06-28T11:30:00+08:00"
            },
            "Supersession": {
              "SupersedesEvidencePackageID": null,
              "SupersedesEvidenceVersion": null,
              "CorrectionReason": null,
              "SnapshotReplacementScope": null,
              "ReversalReason": null
            },
            "NonClaims": [
              "NotProductionValidated",
              "NotSupplierSourceApproval",
              "NotSupplierExecutionProof",
              "NotDeliveryPerformanceProof",
              "NotLeadTimePerformanceProof",
              "NotWorkOrderRoutingOperationProof",
              "NotMaterialConsumptionProof",
              "NoAutomaticDDAEMasterSettingUpdate",
              "NotBusinessGoldenLoopReady"
            ]
          }
        }
        """;
}

static string SdbrExecutionObjectEvidenceReviewedJson()
{
    var contractExamplePath = @"D:\Documents\DDAE_INTERFACE_CONTRACT\contracts\sdbr-execution-object-evidence-v1\examples\sub-avionics-compute-part-fpga-space-reviewed.json";
    if (File.Exists(contractExamplePath))
    {
        return File.ReadAllText(contractExamplePath);
    }

    return """
        {
          "ContractID": "SDBR-EXECUTION-OBJECT-EVIDENCE-V1",
          "ContractVersion": "1.0.0",
          "MessageID": "MSG-SDBR-EOE-20260628-001",
          "IdempotencyKey": "SDBR-EXECUTION-OBJECT-EVIDENCE-V1:SDBR:EOE-SUB-AVIONICS-COMPUTE-20260628-001:1.0.0:WorkOrder:WO-SUB-AVIONICS-COMPUTE-001:2026-06-28T10:00:00+08:00",
          "ProducerSystem": "SDBR",
          "ConsumerSystems": ["DDAE", "ContractAgent"],
          "OccurredAt": "2026-06-28T10:00:00+08:00",
          "TimeZone": "Asia/Shanghai",
          "Payload": {
            "EvidencePackageID": "EOE-SUB-AVIONICS-COMPUTE-20260628-001",
            "EvidenceVersion": "1.0.0",
            "EvidenceStatus": "Reviewed",
            "EvidenceConfidence": "Reviewed",
            "PlanningContext": {
              "PlanningRunID": "SDBR-PLAN-RUN-20260628-001",
              "OperatingModelConfigurationID": "OMC-SAT-BUS-001-20260628-001",
              "OperatingModelFingerprint": "sha256:reviewed-fixture-operating-model",
              "SchedulingConfigurationID": "SCH-SAT-BUS-001-20260628-001",
              "DDMRPConfigurationID": "DDMRP-SAT-BUS-001-20260628-001",
              "MasterDataVersionID": "MDV-SDBR-20260628-001",
              "OperationalStateSnapshotID": "OSS-SDBR-20260628-001",
              "ScheduleFingerprint": "sha256:reviewed-fixture-schedule"
            },
            "WorkOrder": {
              "WorkOrderID": "WO-SUB-AVIONICS-COMPUTE-001",
              "WorkOrderVersion": "1.0.0",
              "WorkOrderType": "Assembly",
              "WorkOrderStatus": "Frozen",
              "ProductID": "SUB-AVIONICS-COMPUTE",
              "ItemID": "SUB-AVIONICS-COMPUTE",
              "LocationID": "ASSY-LINE-AVIONICS",
              "RequiredQty": 1,
              "CompletedQty": 0,
              "RemainingQty": 1,
              "ScrapQty": 0,
              "RejectQty": 0,
              "QuantityUOM": "EA",
              "UOMAuthority": "SDBR-MASTER-DATA",
              "PlannedStartAt": "2026-07-15T08:00:00+08:00",
              "PlannedFinishAt": "2026-07-16T18:00:00+08:00",
              "DueAt": "2026-07-18T18:00:00+08:00",
              "EventCaptureMode": "Normal",
              "LateCaptureReason": null,
              "ReconciliationReferenceID": null,
              "ObservedAt": null,
              "RecordedAt": "2026-06-28T10:00:00+08:00"
            },
            "Routing": {
              "RoutingID": "SDBR-ROUTE-SUB-AVIONICS-COMPUTE-001",
              "RoutingVersion": "1.0.0",
              "RoutingAuthority": "SDBR_EXECUTABLE_ROUTING",
              "RoutingSelectionReason": "Reviewed fixture routing for contract validation only",
              "PrimaryRoutingID": "DDAE-PRIMARY-ROUTING-TRACE-ONLY",
              "AlternateRoutingID": null
            },
            "Operations": [
              {
                "OperationID": "OP-INSTALL-FPGA-SPACE-001",
                "OperationSequence": 10,
                "OperationCode": "INSTALL-FPGA",
                "OperationName": "Install space-grade FPGA into avionics compute assembly",
                "OperationStatus": "Frozen",
                "ResourceID": "RES-AVIONICS-BENCH-001",
                "WorkCenterID": "WC-AVIONICS-ASSEMBLY",
                "ControlPointID": "CP-AVIONICS-COMPUTE-TRACE-ONLY",
                "PlannedOperationStartAt": "2026-07-15T08:00:00+08:00",
                "PlannedOperationFinishAt": "2026-07-15T12:00:00+08:00",
                "ActualOperationStartAt": null,
                "ActualOperationFinishAt": null,
                "EventCaptureMode": "Normal",
                "LateCaptureReason": null,
                "ReconciliationReferenceID": null,
                "ObservedAt": null,
                "RecordedAt": "2026-06-28T10:00:00+08:00"
              }
            ],
            "Release": {
              "ReleaseID": "REL-WO-SUB-AVIONICS-COMPUTE-001",
              "ReleaseDecisionID": "RELDEC-WO-SUB-AVIONICS-COMPUTE-001",
              "ReleaseState": "ReleaseCandidate",
              "ReleaseAuthorizedAt": null,
              "ReleaseAuthorizedBy": null,
              "ReleaseBlockReasons": ["Reviewed fixture only; no production release authority"]
            },
            "Dispatch": {
              "DispatchPackageID": "DISP-PKG-WO-SUB-AVIONICS-COMPUTE-001",
              "DispatchQueueID": "DQ-WC-AVIONICS-ASSEMBLY-001",
              "DispatchState": "DispatchSuggested",
              "DispatchSuggestedAt": "2026-07-15T07:30:00+08:00",
              "DispatchIssuedAt": null,
              "DispatchAcceptedAt": null,
              "DispatchRejectedAt": null,
              "DispatchReason": "Reviewed fixture dispatch suggestion only",
              "DispatchPriorityRank": 1
            },
            "MaterialRequirements": [
              {
                "MaterialRequirementID": "MR-SUB-AVIONICS-COMPUTE-PART-FPGA-SPACE-001",
                "ConsumedItemID": "PART-FPGA-SPACE",
                "ConsumedLocationID": "WH-ELEC-QA",
                "RequiredAt": "2026-07-15T08:00:00+08:00",
                "RequiredQty": 63,
                "QuantityUOM": "EA",
                "UOMAuthority": "SDBR-MASTER-DATA",
                "InventoryQualityEvidencePackageID": "PIQE-PART-FPGA-SPACE-WH-ELEC-QA-20260628-001"
              }
            ],
            "MaterialIssues": [],
            "MaterialConsumptions": [],
            "DDAEGovernanceBoundary": {
              "AllowsAutomaticOperatingModelUpdate": false,
              "AllowsAutomaticMasterSettingUpdate": false,
              "AllowsAutomaticBufferUpdate": false,
              "AllowsAutomaticSupplierSourceFactUpdate": false,
              "AllowsAutomaticLeadTimeUpdate": false,
              "AllowsAutomaticMOQUpdate": false,
              "AllowsAutomaticOrderCycleUpdate": false,
              "MayCreateReviewTopic": true,
              "MayCreateGovernanceProposal": true,
              "RequiresSeparateDDAEApproval": true
            },
            "Traceability": {
              "SourceSystem": "SDBR",
              "SourceDocumentType": "ReviewedFixture",
              "SourceDocumentID": "SDBR-EOE-FIXTURE-20260628-001",
              "SourceDocumentLineID": "1",
              "SourceRecordID": "SDBR-EOE-REC-20260628-001",
              "TraceableID": "TRACE-SDBR-EOE-20260628-001",
              "RegisteredAt": "2026-06-28T10:00:00+08:00"
            },
            "Supersession": {
              "SupersedesEvidencePackageID": null,
              "SupersedesEvidenceVersion": null,
              "CorrectsExecutionEventID": null,
              "CorrectionReason": null,
              "ReversesExecutionEventID": null,
              "ReversalReason": null
            },
            "NonClaims": [
              "Reviewed fixture only",
              "Not source-authoritative production execution evidence",
              "Not production material issue evidence",
              "Not production material consumption evidence",
              "Not production inventory authority",
              "Not production quality authority",
              "Not automatic DDAE master-setting update",
              "Not ProductionValidated",
              "Not Business Golden Loop readiness"
            ]
          }
        }
        """;
}

static string SdbrExecutionObjectAcceptedAckJson()
{
    var contractExamplePath = @"D:\Documents\DDAE_INTERFACE_CONTRACT\contracts\sdbr-execution-object-evidence-v1\examples\ack-accepted-reviewed.json";
    if (File.Exists(contractExamplePath))
    {
        return File.ReadAllText(contractExamplePath);
    }

    return """
        {
          "ContractID": "SDBR-EXECUTION-OBJECT-EVIDENCE-V1",
          "ContractVersion": "1.0.0",
          "AckID": "ACK-DDAE-SDBR-EOE-20260628-001",
          "MessageID": "MSG-SDBR-EOE-20260628-001",
          "IdempotencyKey": "SDBR-EXECUTION-OBJECT-EVIDENCE-V1:SDBR:EOE-SUB-AVIONICS-COMPUTE-20260628-001:1.0.0:WorkOrder:WO-SUB-AVIONICS-COMPUTE-001:2026-06-28T10:00:00+08:00",
          "ConsumerSystem": "DDAE",
          "AckStatus": "Accepted",
          "ErrorCode": null,
          "ErrorMessage": null,
          "ReceivedAt": "2026-06-28T10:00:05+08:00",
          "Retryable": false,
          "TraceableID": "TRACE-SDBR-EOE-20260628-001"
        }
        """;
}

static string ProductionInventoryQualityAcceptedAckJson()
{
    var contractExamplePath = @"D:\Documents\DDAE_INTERFACE_CONTRACT\contracts\production-inventory-quality-evidence-v1\examples\ack-accepted-reviewed.json";
    if (File.Exists(contractExamplePath))
    {
        return File.ReadAllText(contractExamplePath);
    }

    return """
        {
          "ContractID": "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1",
          "ContractVersion": "1.0.0",
          "AckID": "PIQEV1-ACK-DDAE-PART-FPGA-SPACE-20260628-001",
          "MessageID": "PIQEV1-MSG-PART-FPGA-SPACE-20260628-001",
          "EvidencePackageID": "PIQE-PART-FPGA-SPACE-20260628-001",
          "ConsumerSystem": "DDAE",
          "AckStatus": "Accepted",
          "AckAt": "2026-06-28T11:31:00+08:00",
          "Errors": []
        }
        """;
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

static void DeleteSqliteFiles(string databasePath)
{
    foreach (var file in new[] { databasePath, $"{databasePath}-wal", $"{databasePath}-shm" })
    {
        if (File.Exists(file))
        {
            File.Delete(file);
        }
    }
}

static NetworkDataSet BuildCumulativeNetworkData()
{
    var effectiveFrom = new DateOnly(2026, 6, 1);
    return new NetworkDataSet(
        new[]
        {
            new NetworkItemMaster("FG-A", "成品 A", "FinishedGood", "测试族", "Active", 100m, "EA"),
            new NetworkItemMaster("SUB-B", "半成品 B", "Subassembly", "测试族", "Active", 40m, "EA"),
            new NetworkItemMaster("PART-C", "采购件 C", "PurchasedPart", "测试族", "Active", 10m, "EA"),
        },
        new[]
        {
            new NetworkBomHeader("BOM-FG-A", "FG-A", "A", effectiveFrom, null, "Released"),
            new NetworkBomHeader("BOM-SUB-B", "SUB-B", "A", effectiveFrom, null, "Released"),
        },
        new[]
        {
            new NetworkBomLine("BOM-FG-A", "FG-A", "SUB-B", 2m, 0.10m, ""),
            new NetworkBomLine("BOM-SUB-B", "SUB-B", "PART-C", 3m, 0.20m, ""),
        },
        Array.Empty<NetworkAlternateItem>(),
        new[]
        {
            new NetworkRoutingLine("FG-A", "FG-A", "测试族", "R1", "总装", "RES-AIT", 0.10m, effectiveFrom, null),
            new NetworkRoutingLine("SUB-B", "FG-A", "测试族", "R1", "半成品装配", "RES-AIT", 0.05m, effectiveFrom, null),
        },
        new[] { new NetworkSupplierSource("PART-C", "SUP-C", "供应商 C", 1, 100m, 14, 1.10m, 100m, 10m, "Qualified") },
        new[] { new NetworkInventoryLocation("SUB-B", "WH-WIP", "半成品库", "WipSupermarket", "Qualified", "测试", 365, true) },
        new[] { new NetworkBufferSetting("SUB-B", true, "测试缓冲", 7, 10m, 3, effectiveFrom, null, "Current") },
        new[] { new NetworkLeadTimeProfile("PART-C", "Supplier", 14, 1.10m, "PART-C") });
}

static NetworkDataSet BuildInvalidNetworkData()
{
    var effectiveFrom = new DateOnly(2026, 6, 1);
    return new NetworkDataSet(
        new[]
        {
            new NetworkItemMaster("FG-A", "成品 A", "FinishedGood", "测试族", "Active", 100m, "EA"),
            new NetworkItemMaster("SUB-B", "半成品 B", "Subassembly", "测试族", "Active", 40m, "EA"),
            new NetworkItemMaster("PART-C", "采购件 C", "PurchasedPart", "测试族", "Active", 10m, "EA"),
            new NetworkItemMaster("PART-D", "无供应采购件 D", "PurchasedPart", "测试族", "Active", 12m, "EA"),
            new NetworkItemMaster("SUB-E", "无库存解耦点 E", "Subassembly", "测试族", "Active", 22m, "EA"),
        },
        new[]
        {
            new NetworkBomHeader("BOM-FG-A", "FG-A", "A", effectiveFrom, null, "Released"),
            new NetworkBomHeader("BOM-SUB-B", "SUB-B", "A", effectiveFrom, null, "Released"),
            new NetworkBomHeader("BOM-PART-C", "PART-C", "A", effectiveFrom, null, "Released"),
            new NetworkBomHeader("BOM-MISSING-PARENT", "MISSING-PARENT", "A", effectiveFrom, null, "Released"),
        },
        new[]
        {
            new NetworkBomLine("BOM-FG-A", "FG-A", "SUB-B", 2m, 0.10m, ""),
            new NetworkBomLine("BOM-SUB-B", "SUB-B", "PART-C", 3m, 0.20m, ""),
            new NetworkBomLine("BOM-PART-C", "PART-C", "FG-A", 1m, 0m, ""),
            new NetworkBomLine("BOM-FG-A", "FG-A", "MISSING-COMPONENT", 1m, 0m, ""),
            new NetworkBomLine("BOM-ORPHAN", "FG-A", "PART-D", 1m, 0m, ""),
        },
        new[] { new NetworkAlternateItem("ALT-C", "PART-C", "ALT-MISSING", 2, 1m, "EngineeringReview") },
        new[] { new NetworkRoutingLine("FG-A", "FG-A", "测试族", "R1", "总装", "RES-AIT", 0.10m, effectiveFrom, null) },
        new[] { new NetworkSupplierSource("PART-C", "SUP-C", "供应商 C", 1, 100m, 14, 1.10m, 100m, 10m, "Qualified") },
        Array.Empty<NetworkInventoryLocation>(),
        new[]
        {
            new NetworkBufferSetting("SUB-E", true, "无库存位置缓冲", 7, 10m, 3, effectiveFrom, null, "Current"),
            new NetworkBufferSetting("MISSING-BUFFER", true, "缺失物料缓冲", 7, 10m, 3, effectiveFrom, null, "Current"),
        },
        new[] { new NetworkLeadTimeProfile("PART-C", "Supplier", 14, 1.10m, "PART-C") });
}

internal sealed class NetworkGraphFixtureDataSource : IScenarioWorkspaceDataSource, INetworkStructureDataSource, INetworkGraphDataSource, INetworkMetricsDataSource, INetworkScoringDataSource
{
    private readonly NetworkDataSet _networkData;

    public NetworkGraphFixtureDataSource(NetworkDataSet networkData)
    {
        _networkData = networkData;
    }

    public int LoadCount { get; private set; }

    public ScenarioWorkspaceDataSet Load(ScenarioWorkspaceDataRequest request)
    {
        LoadCount++;
        return new ScenarioWorkspaceDataSet(
            request,
            Array.Empty<ProductFamily>(),
            Array.Empty<SkuBufferSetting>(),
            Array.Empty<InventoryPosition>(),
            Array.Empty<WeeklyDemand>(),
            Array.Empty<CapacityResource>(),
            Array.Empty<ResourceRouting>(),
            Array.Empty<SupplierItemSource>(),
            Array.Empty<HistoricalDemandActual>(),
            Array.Empty<BudgetBenchmark>(),
            Array.Empty<ResourceCalendarEntry>(),
            Array.Empty<SupplierCapacityWindow>(),
            Array.Empty<ScenarioTemplate>(),
            Array.Empty<DdmrpParameterProfile>(),
            Array.Empty<MasterSetting>(),
            Array.Empty<BusinessGuardrail>());
    }

    public NetworkStructureDataSet LoadNetworkStructure(NetworkStructureDataRequest request)
    {
        return new NetworkStructureDataSet(
            request,
            _networkData,
            new NetworkStructureRuntimeSignals(
                Array.Empty<ProductFamily>(),
                Array.Empty<SkuBufferSetting>(),
                Array.Empty<WeeklyDemand>(),
                Array.Empty<CapacityResource>(),
                Array.Empty<ResourceRouting>(),
                Array.Empty<SupplierItemSource>(),
                Array.Empty<SupplierCapacityWindow>(),
                Array.Empty<InventoryPosition>()));
    }

    public NetworkGraphDataSet LoadNetworkGraph(DateOnly anchorDate)
    {
        LoadCount++;
        return new NetworkGraphDataSet(_networkData, anchorDate);
    }

    public NetworkMetricsDataSet LoadNetworkMetrics(int horizonWeeks, DateOnly anchorDate)
    {
        LoadCount++;
        return new NetworkMetricsDataSet(
            Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52),
            anchorDate,
            _networkData,
            Array.Empty<NetworkMetricSkuSignal>(),
            Array.Empty<NetworkMetricResourceLoadSignal>(),
            Array.Empty<NetworkMetricSupplierCapacitySignal>());
    }

    public NetworkScoringDataSet LoadNetworkScoring(int horizonWeeks, DateOnly anchorDate)
    {
        LoadCount++;
        return new NetworkScoringDataSet(
            Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52),
            anchorDate,
            _networkData,
            Array.Empty<NetworkScoringFamilySignal>(),
            Array.Empty<NetworkScoringSkuSignal>(),
            Array.Empty<NetworkScoringDemandSignal>(),
            Array.Empty<NetworkScoringResourceSignal>(),
            Array.Empty<NetworkScoringRoutingSignal>(),
            Array.Empty<NetworkScoringSupplierItemSignal>(),
            Array.Empty<NetworkScoringBufferProjectionSignal>(),
            Array.Empty<NetworkScoringResourceLoadSignal>(),
            Array.Empty<NetworkScoringSupplyRequirementSignal>(),
            Array.Empty<NetworkScoringSupplierCapacitySignal>());
    }
}

internal sealed record LegacyScenarioSource(ValidationData Data);

internal sealed class FakeLegacyScenarioWorkspaceAdapter : IScenarioWorkspaceDataAdapter<LegacyScenarioSource>
{
    public ScenarioWorkspaceDataSet Map(LegacyScenarioSource source, ScenarioWorkspaceDataRequest request)
    {
        return new SeedScenarioWorkspaceDataSource(source.Data).Load(request);
    }
}

internal sealed class TrackingScenarioWorkspaceDataSource : IScenarioWorkspaceDataSource, INetworkStructureDataSource, INetworkGraphDataSource, INetworkMetricsDataSource, INetworkScoringDataSource
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

    public NetworkStructureDataSet LoadNetworkStructure(NetworkStructureDataRequest request)
    {
        return NetworkStructureDataSourceAdapter.FromScenarioWorkspace(
            Load(new ScenarioWorkspaceDataRequest(
                request.HorizonWeeks,
                request.AnchorDate,
                request.SkuFilter,
                request.FamilyFilter)));
    }

    public NetworkGraphDataSet LoadNetworkGraph(DateOnly anchorDate)
    {
        var data = LoadNetworkStructure(new NetworkStructureDataRequest(12, anchorDate));
        return new NetworkGraphDataSet(data.NetworkData, data.Request.AnchorDate);
    }

    public NetworkMetricsDataSet LoadNetworkMetrics(int horizonWeeks, DateOnly anchorDate)
    {
        return new DdsopNetworkMetricsDataSource(this).LoadNetworkMetrics(horizonWeeks, anchorDate);
    }

    public NetworkScoringDataSet LoadNetworkScoring(int horizonWeeks, DateOnly anchorDate)
    {
        return new DdsopNetworkScoringDataSource(this).LoadNetworkScoring(horizonWeeks, anchorDate);
    }
}

internal sealed class CapturingOptimizationSolver : IOptimizationSolver
{
    public string SolverName => "FakeSolver";

    public int CallCount { get; private set; }

    public OptimizationProblem? LastProblem { get; private set; }

    public OptimizationSolution Solve(OptimizationProblem problem)
    {
        CallCount++;
        LastProblem = problem;
        var selected = problem.Candidates
            .OrderByDescending(candidate => candidate.ObjectiveValue)
            .Take(Math.Max(1, Math.Min(problem.MaxSelectedCandidates, problem.Candidates.Count)))
            .Select(candidate => candidate.CandidateId)
            .ToList();

        return new OptimizationSolution(
            OptimizationSolverStatus.Optimal,
            "FakeSolver",
            "测试求解器已选择候选动作。",
            selected.Count,
            selected);
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
            new[]
            {
                new DdmrpParameterProfile(
                    sku.Sku,
                    sku.Name,
                    sku.Family,
                    "测试解耦点",
                    "测试缓冲档案",
                    sku.Adu,
                    sku.AduSource,
                    sku.AduCalculationWindowDays,
                    sku.DecoupledLeadTimeDays,
                    sku.DltSource,
                    sku.VariabilityFactor,
                    sku.DemandAdjustmentFactor,
                    sku.ZoneAdjustmentFactor,
                    sku.MinimumOrderQuantity,
                    sku.OrderCycleDays,
                    sku.UnitCost,
                    sku.WeeklyCapacityUnits,
                    750,
                    1250,
                    1950,
                    1,
                    12,
                    "Current",
                    "Complete",
                    "测试参数完整。")
            },
            Array.Empty<MasterSetting>(),
            Array.Empty<BusinessGuardrail>());
    }
}
