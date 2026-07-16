using AdaptiveSopDdsop.Web.Data;
using AdaptiveSopDdsop.Web.Domain;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

var tests = new (string Name, Action Run)[]
{
    ("Standard DDMRP sizing returns 80 120 70 with an explainable green driver", TestStandardDdmrpSizingReturns80_120_70),
    ("DDMRP sizing rejects missing or illegal lead-time factors", TestDdmrpSizingRejectsIllegalLeadTimeFactor),
    ("Net flow position adds on hand and open supply then subtracts qualified demand", TestNetFlow),
    ("Planning recommendation replenishes to top of green at review time when net flow is at or below top of yellow", TestPlanningRecommendation),
    ("Promotion scenario increases ADU and working capital", TestPromotionScenario),
    ("Supply disruption lowers buffer health and creates expedite recommendation", TestSupplyDisruptionScenario),
    ("Planned shutdown creates capacity warning and management review action", TestShutdownScenario),
    ("Baseline data demonstrates red yellow green and over top of green buffer statuses with Chinese names", TestBaselineStatusVarietyAndChineseNames),
    ("Five-stage internal files do not reference protected contract types or endpoints", TestFiveStageServicesDoNotReferenceExternalContractTypesOrEndpoints),
    ("Seed scale matches a credible satellite manufacturing demo", TestSeedScaleMatchesSatelliteManufacturingDemo),
    ("FPGA belongs only to its independent inventory control point", TestFpgaBelongsOnlyToIndependentInventoryControlPoint),
    ("Three independent inventory control points are explicit", TestThreeIndependentInventoryControlPointsAreExplicit),
    ("Capacity protection requires sequenced upstream evidence", TestCapacityProtectionRequiresSequencedUpstreamEvidence),
    ("Capacity protection is not inferred without sequence evidence", TestCapacityProtectionDoesNotInferWithoutSequenceEvidence),
    ("Scenario capacity protection excludes FPGA-only sequence evidence", TestScenarioCapacityProtectionExcludesFpgaSequenceEvidence),
    ("Consolidated requirements are represented in validation data", TestConsolidatedRequirementsDataCoverage),
    ("History review follows cumulative lead time and exposes protection evidence", TestHistoryReviewUsesCumulativeLeadTimeAndProtectionEvidence),
    ("History review aggregates distinct twenty-six and fifty-two week facts", TestHistoryReviewAggregatesDistinctTwentySixAndFiftyTwoWeekFacts),
    ("History review projects stock time capacity and sizing views from explicit facts", TestHistoryReviewProjectsExplicitBufferViews),
    ("History capacity summaries average weekly protection consumption", TestHistoryCapacitySummariesAverageWeeklyProtectionConsumption),
    ("History capacity protection rejects self-referential resource evidence", TestHistoryCapacityProtectionRejectsSelfReference),
    ("Historical CCR role wins when a resource is also upstream", TestHistoricalCcrRoleWinsOverUpstreamRole),
    ("History review uses the effective historical parameter snapshot for every point", TestHistoryReviewUsesEffectiveParameterSnapshot),
    ("History review exposes missing evidence instead of zero or current-parameter backfill", TestHistoryReviewDoesNotBackfillMissingEvidence),
    ("History facts expose versioned inventory time and capacity evidence", TestHistoryFactsExposeVersionedInventoryTimeAndCapacityEvidence),
    ("History time-buffer costs exclude FPGA event evidence", TestHistoryTimeBufferCostsExcludeFpgaEventEvidence),
    ("History capacity protection excludes FPGA routing evidence", TestHistoryCapacityProtectionExcludesFpgaRoutingEvidence),
    ("Historical outcomes use explicit facts and traceable costs", TestHistoricalOutcomesUseExplicitFactsAndTraceableCosts),
    ("Current baseline exposes meeting snapshot KPIs with source and as-of evidence", TestCurrentBaselineExposesSnapshotKpisWithSourceAndAsOf),
    ("Current baseline rejects missing KPI evidence instead of freezing zero substitutes", TestCurrentBaselineRejectsMissingSnapshotKpiEvidence),
    ("Current baseline applies required evidence rules when section items are null or empty", TestCurrentBaselineAppliesRequiredRulesForEmptySectionItems),
    ("Current baseline blocks incomplete DDMRP sizing evidence", TestCurrentBaselineBlocksIncompleteDdmrpSizingEvidence),
    ("Legacy frozen baseline keeps missing lead-time factor visible and cannot be recalculated", TestLegacyFrozenBaselineKeepsMissingLeadTimeFactor),
    ("Current baseline UI follows item-level freeze blockers", TestCurrentBaselineUiFollowsItemLevelFreezeBlockers),
    ("Time-buffer evidence rules control baseline freezing without live-data backfill", TestTimeBufferEvidenceRulesControlBaselineFreeze),
    ("Seed time-buffer progress covers every requested horizon week", TestSeedTimeBufferProgressCoversRequestedHorizon),
    ("Time-buffer baseline freeze rejects partial horizon evidence", TestTimeBufferBaselineFreezeRejectsPartialHorizonEvidence),
    ("Time-buffer baseline freeze rejects duplicate week evidence", TestTimeBufferBaselineFreezeRejectsDuplicateWeekEvidence),
    ("Time-buffer baseline rejects structurally inconsistent evidence before freezing", TestTimeBufferBaselineRejectsStructurallyInconsistentEvidence),
    ("Mixed time-buffer progress reports the actual missing evidence week", TestMixedTimeBufferProgressReportsActualMissingWeek),
    ("Current baseline freezes complete demo evidence as an immutable audited snapshot", TestCurrentBaselineFreezesCompleteEvidence),
    ("Current baseline rejects missing critical evidence", TestCurrentBaselineRejectsMissingCriticalEvidence),
    ("Current baseline incrementally migrates legacy audit payload evidence", TestCurrentBaselineMigratesLegacyAuditPayloadColumn),
    ("Scenario assumption source provides only manual and demo inputs", TestScenarioAssumptionSourceProvidesOnlyManualAndDemoInputs),
    ("Scenario assumption source rejects external protocol sources", TestScenarioAssumptionSourceRejectsExternalProtocolSources),
    ("Scenario assumption source rejects invalid manual dates", TestScenarioAssumptionSourceRejectsInvalidManualDates),
    ("Scenario comparison rejects tampered demo fixture payloads", TestScenarioComparisonRejectsTamperedDemoFixturePayloads),
    ("Scenario demo template demand disturbance changes backend results", TestScenarioDemoTemplateDemandDisturbanceChangesBackendResults),
    ("Scenario demo template supply disturbance changes backend results", TestScenarioDemoTemplateSupplyDisturbanceChangesBackendResults),
    ("Scenario demo template capacity disturbance changes backend results", TestScenarioDemoTemplateCapacityDisturbanceChangesBackendResults),
    ("Scenario comparison separates external events from response configurations on one frozen baseline", TestScenarioComparisonSeparatesExternalEventsAndResponses),
    ("Scenario supply response restores internal committed supply without external input", TestScenarioSupplyResponseRestoresCommittedSupply),
    ("Scenario comparison recalculates from the frozen snapshot instead of live inventory", TestScenarioComparisonUsesFrozenSnapshotValues),
    ("Frozen comparison save persists baseline scenario and response lineage", TestFrozenComparisonSavePersistsBaselineScenarioAndResponseLineage),
    ("Scenario assumption and frozen save APIs remain internal only", TestScenarioAssumptionAndFrozenSaveApisRemainInternalOnly),
    ("Protection analysis separates inventory time capacity and supply scopes", TestProtectionAnalysisSeparatesInventoryTimeCapacityAndSupply),
    ("Time buffer breach reports penetration recovery and unrecovered horizon", TestTimeBufferBreachReportsPenetrationRecoveryAndUnrecoveredHorizon),
    ("Time buffer missing evidence is not reported as zero", TestTimeBufferMissingEvidenceIsNotReportedAsZero),
    ("Time buffer requires explicit product scope evidence", TestTimeBufferRequiresExplicitProductScopeEvidence),
    ("Time buffer excludes injected FPGA scope evidence", TestTimeBufferExcludesInjectedFpgaScopeEvidence),
    ("Capacity buffer excludes injected FPGA routing evidence", TestCapacityBufferExcludesInjectedFpgaRoutingEvidence),
    ("Time buffer expands explicit family-only product scope", TestTimeBufferExpandsExplicitFamilyOnlyProductScope),
    ("Time buffer rejects unknown family scope evidence", TestTimeBufferRejectsUnknownFamilyScopeEvidence),
    ("Time buffer thresholds use raw penetration before display rounding", TestTimeBufferThresholdsUseRawPenetrationBeforeDisplayRounding),
    ("Supply risk is not classified as a DDOM buffer", TestSupplyRiskIsNotClassifiedAsDdomBuffer),
    ("Pure supply risk does not generate buffer governance from preview", TestPureSupplyRiskDoesNotGenerateBufferGovernanceFromPreview),
    ("Frozen pure supply risk does not generate buffer governance", TestFrozenPureSupplyRiskDoesNotGenerateBufferGovernance),
    ("FPGA never appears in time or capacity buffer results", TestFpgaNeverAppearsInTimeOrCapacityBufferResults),
    ("Protection breach analysis reports first red duration recovery and unrecovered horizon", TestProtectionBreachAnalysisReportsRecovery),
    ("Coordination ledger enforces workflow and audits creation status decision and outcome", TestCoordinationLedgerEnforcesWorkflowAndAuditsUpdates),
    ("Coordination ledger rejects invalid direct completion", TestCoordinationLedgerRejectsInvalidDirectCompletion),
    ("Known local smoke record repair is scoped audited and idempotent", TestKnownSmokeRecordRepairIsScopedAuditedAndIdempotent),
    ("SQLite round trips Chinese text without question marks", TestSqliteRoundTripsChineseWithoutQuestionMarks),
    ("Five-stage navigation preserves independent white-box and public demo validation pages", TestFiveStageNavigationPreservesValidationPages),
    ("Five-stage navigation uses hierarchical view switching", TestFiveStageNavigationUsesHierarchicalViewSwitching),
    ("Workspace navigation removes scroll observer and uses hash state", TestWorkspaceNavigationRemovesScrollObserverAndUsesHashState),
    ("Only the selected stage or child view is visible", TestOnlySelectedStageOrChildViewIsVisible),
    ("History review exposes four selectable visualization workspaces", TestHistoryReviewExposesSelectableVisualizationWorkspaces),
    ("History review retains range and selection state", TestHistoryReviewRetainsRangeAndSelectionState),
    ("History review request race fixture runs in the standard harness", TestHistoryReviewRequestRaceFixtureRunsInStandardHarness),
    ("History visual renderers use backend evidence without frontend formulas", TestHistoryVisualRenderersUseBackendEvidence),
    ("Future buffer charts use backend sizing and separate volatility", TestFutureBufferChartsUseBackendSizingAndSeparateVolatility),
    ("Five-stage business views translate internal codes without mojibake", TestBusinessViewsTranslateInternalCodesWithoutMojibake),
    ("Five-stage business views localize ordinary unit tokens", TestBusinessViewsLocalizeOrdinaryUnitTokens),
    ("RCCP peak load is explained as replenishment release pressure", TestRccpPeakLoadUsesReleasePressureWording),
    ("Five-stage generated business text uses Chinese ordinary wording", TestGeneratedBusinessTextUsesChineseOrdinaryWording),
    ("Five-stage UI has no external import or protocol input", TestFiveStageUiHasNoExternalImportOrProtocolInput),
    ("Time-buffer view uses backend results only", TestTimeBufferViewUsesBackendResultsOnly),
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
    ("Scenario Run Workspace script fetches workspace data", TestScenarioRunWorkspaceScriptFetchesWorkspaceData),
    ("Scenario Run Workspace script delegates business calculations to services", TestScenarioRunWorkspaceScriptDelegatesBusinessCalculationsToServices),
    ("Scenario workspace seed data covers baseline scenario use cases", TestScenarioWorkspaceSeedDataCoversUseCases),
    ("Scenario workspace exposes complete DDMRP parameter profiles", TestScenarioWorkspaceExposesCompleteDdmrpParameterProfiles),
    ("Scenario workspace adapter can map alternate source structures", TestScenarioWorkspaceAdapterCanMapAlternateSourceStructures),
    ("Scenario preview returns baseline and scenario results from data source", TestScenarioPreviewReturnsComparableResults),
    ("Scenario run persistence saves preview result and audit chain", TestScenarioRunPersistenceSavesPreviewResultAndAuditChain),
    ("Scenario Run Workspace exposes scenario save audit UI", TestScenarioRunWorkspaceExposesSaveAuditUi),
    ("Five-stage details expose readable bidirectional lineage navigation", TestFiveStageDetailsExposeReadableBidirectionalLineageNavigation),
    ("Master settings governance generates proposals from preview", TestMasterSettingsGovernanceGeneratesProposalsFromPreview),
    ("Master settings governance saves audits and advances status", TestMasterSettingsGovernanceSavesAuditsAndAdvancesStatus),
    ("Master settings governance preserves decision package metadata without auto effect", TestMasterSettingsGovernancePreservesDecisionPackageMetadata),
    ("Manual governance change requires baseline and allows no scenario", TestManualGovernanceChangeRequiresBaselineAndAllowsNoScenario),
    ("Manual governance change with scenario requires validated saved run", TestManualGovernanceChangeWithScenarioRequiresValidatedSavedRun),
    ("Scenario-derived governance change requires baseline and scenario", TestScenarioDerivedGovernanceChangeRequiresBaselineAndScenario),
    ("Unlinked historical records remain explicitly unlinked", TestUnlinkedHistoricalRecordsRemainExplicitlyUnlinked),
    ("Scenario change and coordination links are queryable both directions", TestScenarioChangeAndCoordinationLinksAreQueryableBothDirections),
    ("Baseline references expose runs changes and actions", TestBaselineReferencesExposeRunsChangesAndActions),
    ("Baseline references return all links beyond public page limit", TestBaselineReferencesReturnAllLinksBeyondPublicPageLimit),
    ("Lineage filters use indexable parameterized equality predicates", TestLineageFiltersUseIndexableParameterizedEqualityPredicates),
    ("Coordination outcome does not advance governance status", TestCoordinationOutcomeDoesNotAdvanceGovernanceStatus),
    ("Frozen comparison governance rejects reused run ID with different normalized request", TestFrozenComparisonGovernanceRejectsReusedRunIdWithDifferentRequest),
    ("Lineage endpoints expose read-only filters and validate saved comparison runs", TestLineageEndpointsExposeReadOnlyFiltersAndValidateSavedComparisonRuns),
    ("Scenario Run Workspace exposes master settings governance UI", TestScenarioRunWorkspaceExposesMasterSettingsGovernanceUi),
    ("Scenario preview applies pre-build capacity policy and supplier limits", TestScenarioPreviewAppliesScenarioParameters),
    ("Product RCCP workspace summarizes resources heatmap and detail", TestProductRccpWorkspaceSummarizesResourcesHeatmapAndDetail),
    ("Scenario preview returns product RCCP comparison", TestScenarioPreviewReturnsProductRccpComparison),
    ("Constraint workspace summarizes constrained and unconstrained capacity and supply", TestConstraintWorkspaceSummarizesCapacityAndSupply),
    ("Scenario preview returns constrained and unconstrained comparison", TestScenarioPreviewReturnsConstraintComparison),
    ("Scenario preview returns supplier collaboration drilldown", TestScenarioPreviewReturnsSupplierCollaborationDrilldown),
    ("Future buffer projection uses the same backend period sizing for orders and charts", TestFutureBufferTrendUsesBackendPeriodSizing),
    ("Buffer trend maps an order-cycle wait trace to an activity", TestBufferTrendMapsOrderCycleWaitToActivity),
    ("Buffer trend workspace summarizes KPIs heatmap and SKU detail", TestBufferTrendWorkspaceSummarizesKpisHeatmapAndDetail),
    ("Scenario preview returns graphical buffer trend comparison", TestScenarioPreviewReturnsBufferTrendComparison),
    ("Exception workspace detects variance signals and scenario presets", TestExceptionWorkspaceDetectsVarianceSignalsAndScenarioPresets),
    ("DDSOP-CONFIG-INBOUND-V1 payload and ACK interpreter stay contract-shaped", TestDdsopConfigInboundPayloadAndAckInterpreter),
    ("DDSOP-FEEDBACK-OUTBOUND-V1 ledger accepts SDBR fixture feedback without governance mutation", TestDdsopFeedbackInboundLedgerAcceptsSdbrFixtures),
    ("DDSOP-RUNTIME-PLANNING-INPUT-V1 generates DDAE-owned runtime package", TestDdsopRuntimePlanningInputGeneratesDdaeOwnedPackage),
    ("AdventureWorks scheduling adapter metadata stays non-DDAE-owned", TestAdventureWorksSchedulingAdapterMetadataStaysNonDdaeOwned),
    ("Contract repository path resolver prefers configured root", TestContractRepositoryPathResolverPrefersConfiguredRoot),
    ("Contract repository path resolver discovers sibling repository", TestContractRepositoryPathResolverDiscoversSiblingRepository),
    ("AdventureWorks product demo profile exposes DDAE governance read model", TestAdventureWorksProductDemoProfileExposesDdaeGovernanceReadModel),
    ("DDSOP-RUNTIME-PLANNING-INPUT-V1 correlates feedback through delivery ledger", TestDdsopRuntimePlanningInputCorrelatesFeedback),
    ("PUBLIC-DEMO-GOLDEN-DATA-V1 demo service writes handoff payload without production claims", TestPublicDemoGoldenLoopServiceWritesHandoffPayload),
    ("SDBR integration contract endpoints are exposed and old optimization endpoint is removed", TestIntegrationContractEndpointsAndRemovedOptimizationPath),
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

static void TestStandardDdmrpSizingReturns80_120_70()
{
    var sku = new SkuBufferSetting(
        "DDMRP-EXAMPLE", "标准定容算例", "测试", 10m, 12, 0.33m, 7, 50m, 1m, 100m,
        LeadTimeFactor: 0.5m,
        ParameterSnapshotId: "DDMRP-EXAMPLE-V1",
        ParameterEvidenceStatus: "Complete");

    var sizing = DdmrpCalculator.CalculateSizing(sku);

    AssertEqual(120m, sizing.LeadTimeDemand, "lead-time demand");
    AssertEqual(60m, sizing.RedBase, "red base");
    AssertEqual(19.8m, sizing.RedSafety, "red safety");
    AssertEqual(80m, sizing.Zones.Red, "red zone");
    AssertEqual(120m, sizing.Zones.Yellow, "yellow zone");
    AssertEqual(60m, sizing.GreenLeadTimeCandidate, "green lead-time candidate");
    AssertEqual(50m, sizing.GreenMoqCandidate, "green MOQ candidate");
    AssertEqual(70m, sizing.GreenOrderCycleCandidate, "green order-cycle candidate");
    AssertEqual("OrderCycle", sizing.GreenDriver, "green driver");
    AssertEqual(70m, sizing.Zones.Green, "green zone");
    AssertEqual(80m, sizing.Zones.TopOfRed, "top of red");
    AssertEqual(200m, sizing.Zones.TopOfYellow, "top of yellow");
    AssertEqual(270m, sizing.Zones.TopOfGreen, "top of green");
}

static void TestDdmrpSizingRejectsIllegalLeadTimeFactor()
{
    var missing = new SkuBufferSetting("MISSING-LTF", "缺少提前期因子", "测试", 10m, 12, 0.33m, 7, 50m, 1m, 100m);
    AssertInvalidOperationRejected(() => DdmrpCalculator.CalculateSizing(missing), "提前期因子");

    var zero = missing with { LeadTimeFactor = 0m };
    AssertInvalidOperationRejected(() => DdmrpCalculator.CalculateSizing(zero), "提前期因子");

    var greaterThanOne = missing with { LeadTimeFactor = 1.01m };
    AssertInvalidOperationRejected(() => DdmrpCalculator.CalculateSizing(greaterThanOne), "提前期因子");
}

static void AssertInvalidOperationRejected(Action action, string expectedMessage)
{
    try
    {
        action();
    }
    catch (InvalidOperationException exception)
    {
        AssertTrue(exception.Message.Contains(expectedMessage, StringComparison.Ordinal),
            $"rejection message should contain {expectedMessage}");
        return;
    }

    throw new InvalidOperationException($"expected InvalidOperationException containing {expectedMessage}");
}

static void TestNetFlow()
{
    var position = new InventoryPosition("SKU-AXLE-STD", 420, 300, 260);
    var netFlow = DdmrpCalculator.CalculateNetFlow(position);

    AssertEqual(460, netFlow, "net flow");
}

static void TestPlanningRecommendation()
{
    var sku = new SkuBufferSetting(
        "DDMRP-EXAMPLE", "标准定容算例", "测试", 10m, 12, 0.33m, 7, 50m, 1m, 100m,
        LeadTimeFactor: 0.5m,
        ParameterSnapshotId: "DDMRP-EXAMPLE-V1",
        ParameterEvidenceStatus: "Complete");
    var position = new InventoryPosition(sku.Sku, 50m, 0m, 0m);

    var recommendation = DdmrpCalculator.CalculateRecommendation(sku, position);

    AssertEqual("Order", recommendation.Action, "action");
    AssertEqual(220m, recommendation.OrderQuantity, "order quantity");
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

static void TestFiveStageServicesDoNotReferenceExternalContractTypesOrEndpoints()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var boundaryScript = Path.Combine(root, "scripts", "verify-protected-boundaries.ps1");
    AssertTrue(File.Exists(boundaryScript), "protected-boundary verification script should exist");
    var boundaryScriptText = File.ReadAllText(boundaryScript);
    AssertTrue(boundaryScriptText.Contains("[string]$Baseline = \"4e39ec5\"", StringComparison.Ordinal), "boundary script should default to baseline 4e39ec5");
    AssertTrue(boundaryScriptText.Contains("function Get-BracedBlock([string]$Text, [string]$Signature)", StringComparison.Ordinal), "boundary script should declare deterministic braced-block extraction");
    AssertTrue(boundaryScriptText.Contains("function Get-DelimitedBlock([string]$Text, [string]$StartMarker, [string]$EndMarker)", StringComparison.Ordinal), "boundary script should declare deterministic delimited-block extraction");
    AssertTrue(boundaryScriptText.Contains("git diff --exit-code", StringComparison.Ordinal), "boundary script should enforce whole-file equality through git diff --exit-code");
    var fiveStageFiles = new[]
    {
        Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "ProtectionPlanningModels.cs"),
        Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "Models.cs"),
        Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "ScenarioWorkspaceData.cs"),
        Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Data", "SeedData.cs"),
        Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Data", "SeedScenarioWorkspaceDataSource.cs"),
    };
    var protectedTokens = new[]
    {
        "DdsopConfigInboundContract",
        "DdsopRuntimePlanningInputContract",
        "SdbrExecutionObjectEvidenceContract",
        "PublicDemoGoldenLoopService",
        "NetworkScore",
        "network-scoring",
        "SDBR payload",
    };

    foreach (var file in fiveStageFiles)
    {
        AssertTrue(File.Exists(file), $"five-stage boundary file should exist: {Path.GetFileName(file)}");
        var text = File.ReadAllText(file);
        foreach (var token in protectedTokens)
        {
            AssertTrue(!text.Contains(token, StringComparison.Ordinal), $"{Path.GetFileName(file)} must not reference protected token '{token}'");
        }
    }
}

static void TestSeedScaleMatchesSatelliteManufacturingDemo()
{
    var data = SeedData.Create();
    var skus = data.Skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);
    var inventory = data.Inventory.ToDictionary(item => item.Sku, StringComparer.Ordinal);
    var expected = new Dictionary<string, (decimal OnHand, decimal OpenSupply, decimal QualifiedDemand, decimal Adu)>(StringComparer.Ordinal)
    {
        ["SAT-BUS-001"] = (1m, 1m, 2m, 0.20m),
        ["SAT-BUS-002"] = (3m, 1m, 1m, 0.12m),
        ["SAT-PROP-003"] = (12m, 3m, 4m, 0.80m),
        ["PAY-EO-101"] = (3m, 1m, 1m, 0.10m),
        ["PAY-SAR-102"] = (2m, 1m, 1m, 0.08m),
        ["AV-COM-201"] = (28m, 8m, 10m, 1.20m),
        ["AV-OBC-202"] = (20m, 6m, 8m, 0.80m),
        ["AV-FPGA-203"] = (22m, 4m, 11m, 0.18m),
        ["TC-MLI-301"] = (75m, 20m, 24m, 4.00m),
        ["TC-RAD-302"] = (48m, 12m, 16m, 2.50m),
        ["MECH-DEP-401"] = (12m, 3m, 4m, 0.60m),
        ["CBL-HAR-402"] = (120m, 30m, 36m, 5.00m),
    };

    var totalInventoryValue = data.Inventory.Sum(item => item.OnHand * skus[item.Sku].UnitCost);
    AssertTrue(
        totalInventoryValue is >= 50_000_000m and <= 100_000_000m,
        $"seed inventory value should be RMB 50-100 million, got {totalInventoryValue:N0}");

    AssertEqual(expected.Count, skus.Count, "seed SKU count");
    AssertEqual(expected.Count, inventory.Count, "seed inventory position count");
    foreach (var (skuCode, expectedValue) in expected)
    {
        var sku = skus[skuCode];
        var position = inventory[skuCode];
        AssertEqual(expectedValue.OnHand, position.OnHand, $"{skuCode} on hand");
        AssertEqual(expectedValue.OpenSupply, position.OpenSupply, $"{skuCode} open supply");
        AssertEqual(expectedValue.QualifiedDemand, position.QualifiedDemand, $"{skuCode} qualified demand");
        AssertEqual(expectedValue.Adu, sku.Adu, $"{skuCode} ADU");
    }

    var expectedWeeklyHours = new Dictionary<string, decimal>(StringComparer.Ordinal)
    {
        ["RES-AIT"] = 160m,
        ["RES-TVAC"] = 96m,
        ["RES-CLEAN"] = 120m,
        ["RES-HARNESS"] = 180m,
    };
    AssertEqual(expectedWeeklyHours.Count, data.Resources.Count, "standard-hour resource count");
    foreach (var resource in data.Resources)
    {
        AssertEqual(expectedWeeklyHours[resource.Code], resource.WeeklyAvailableUnits, $"{resource.Code} weekly standard hours");
        AssertEqual(1m, resource.UnitLoad, $"{resource.Code} standard-hour unit load");
    }

    decimal BaselineLoadPercent(string resourceCode)
    {
        var requiredHours = data.ResourceRoutings
            .Where(item => item.ResourceCode == resourceCode)
            .Sum(item => skus[item.Sku].Adu * 5m * item.CapacityPerUnit);
        return requiredHours * 100m / expectedWeeklyHours[resourceCode];
    }

    var harnessLoad = BaselineLoadPercent("RES-HARNESS");
    var aitLoad = BaselineLoadPercent("RES-AIT");
    var tvacLoad = BaselineLoadPercent("RES-TVAC");
    AssertTrue(harnessLoad is >= 95m and <= 105m, $"HARNESS baseline load should be 95%-105%, got {harnessLoad:0.0}%");
    AssertTrue(aitLoad is > 80m and <= 100m, $"AIT baseline load should be above its 80% protection start and at most 100%, got {aitLoad:0.0}%");
    AssertTrue(tvacLoad < 90m, $"TVAC baseline load should remain below 90%, got {tvacLoad:0.0}%");

    var workspace = new SeedScenarioWorkspaceDataSource(data)
        .Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1)));
    var tvacLossMultiplier = workspace.ResourceCalendar
        .Where(item => item.ResourceCode == "RES-TVAC")
        .Min(item => item.CapacityMultiplier);
    AssertTrue(tvacLoad / tvacLossMultiplier > 100m, "TVAC should exceed 100% only in the built-in capacity-loss scenario");
}

static void TestFpgaBelongsOnlyToIndependentInventoryControlPoint()
{
    var data = new SeedScenarioWorkspaceDataSource(SeedData.Create())
        .Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1)));
    var fpga = data.Skus.Single(item => item.Sku == "AV-FPGA-203");
    var masterTimeBuffer = data.MasterSettings.Single(item => item.SettingId == "MS-TB-001");

    AssertTrue(masterTimeBuffer.Target != "进口空间级 FPGA", "FPGA is still configured as the MS-TB-001 Time Buffer");
    AssertEqual("关键进口 FPGA 库存控制点", fpga.DecouplingPoint, "FPGA decoupling point");
    AssertTrue(data.Inventory.Any(item => item.Sku == fpga.Sku), "FPGA should retain an inventory position");
    AssertTrue(
        data.DdmrpParameters.Any(item => item.Sku == fpga.Sku && item.DecouplingPoint == fpga.DecouplingPoint && item.CompletenessStatus == "Complete"),
        "FPGA should retain complete inventory-buffer and control-point evidence");

    var timeBuffers = data.TimeBuffers ?? Array.Empty<TimeBufferDefinition>();
    AssertTrue(timeBuffers.Count > 0, "seed should expose the real time buffer definition");
    AssertTrue(
        timeBuffers.Any(item => item.BufferId == "MS-TB-001" && item.ControlPoint == "热真空试验准备控制点" && item.EvidenceStatus == "Complete"),
        "MS-TB-001 should expose complete heat-vacuum preparation control-point evidence");
    AssertTrue(
        timeBuffers.All(item =>
            !item.ControlPoint.Contains("FPGA", StringComparison.OrdinalIgnoreCase) &&
            !item.ProtectedActivity.Contains("FPGA", StringComparison.OrdinalIgnoreCase) &&
            !item.Applicability.Contains("FPGA", StringComparison.OrdinalIgnoreCase)),
        "FPGA must not appear in a time-buffer definition");
    var productScopes = data.TimeBufferProductScopes ?? Array.Empty<TimeBufferProductScope>();
    AssertTrue(productScopes.Count > 0, "time-buffer product scope should be explicit");
    AssertTrue(
        productScopes.All(item => item.EvidenceStatus == "Complete" && !item.Skus.Contains(fpga.Sku, StringComparer.Ordinal)),
        "FPGA must not appear in a time-buffer product scope");
    var progress = data.ControlPointProgress ?? Array.Empty<ControlPointProgressFact>();
    AssertTrue(progress.Count > 0, "time-buffer control-point progress should be explicit");
    AssertTrue(
        progress.All(item => item.BufferId == "MS-TB-001" && item.EvidenceStatus == "Complete"),
        "time-buffer control-point progress should carry explicit evidence status");
    AssertTrue(
        (data.CapacityProtections ?? Array.Empty<CapacityProtectionDefinition>())
            .All(item => !item.Applicability.Contains(fpga.Sku, StringComparison.OrdinalIgnoreCase)),
        "FPGA must not appear in a capacity-protection definition");
    AssertTrue(
        data.ResourceRoutings.Where(item => item.Sku == fpga.Sku).All(item => item.ProtectsCcrResourceCode is null),
        "FPGA routings must not claim capacity-protection consumption");

    AssertEqual("热真空试验准备控制点", masterTimeBuffer.Target, "MS-TB-001 target");
    AssertTrue(!masterTimeBuffer.CurrentValue.Contains("FPGA", StringComparison.OrdinalIgnoreCase), "MS-TB-001 current value must not describe FPGA inventory");
    AssertTrue(!masterTimeBuffer.ProposedValue.Contains("FPGA", StringComparison.OrdinalIgnoreCase), "MS-TB-001 proposed value must not describe FPGA inventory");

    var responseAdjustment = new TimeBufferResponseAdjustment("MS-TB-001", 5, 6, 1.5m, "增加准备班次");
    var parameters = new ScenarioRunParameterSet(TimeBufferAdjustments: new[] { responseAdjustment });
    AssertEqual(responseAdjustment, parameters.TimeBufferAdjustments!.Single(), "time-buffer response adjustment parameter");
    var externalDelay = new ExternalTimeDelay("MS-TB-001", 5, 6, 2m, "试验件到达延迟");
    var externalScenario = new ExternalScenarioDefinition("EXT-TIME-001", "时间延迟场景", TimeDelays: new[] { externalDelay });
    AssertEqual(externalDelay, externalScenario.TimeDelays!.Single(), "external time delay evidence");
}

static void TestThreeIndependentInventoryControlPointsAreExplicit()
{
    var data = new SeedScenarioWorkspaceDataSource(SeedData.Create())
        .Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1)));
    var expectedMembership = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["热控结构件库存控制点"] = new[] { "TC-MLI-301", "TC-RAD-302" },
        ["星载电子半成品库存控制点"] = new[] { "AV-COM-201", "AV-OBC-202" },
        ["关键进口 FPGA 库存控制点"] = new[] { "AV-FPGA-203" },
    };

    foreach (var (controlPoint, expectedSkus) in expectedMembership)
    {
        var actualSkus = data.Skus
            .Where(item => item.DecouplingPoint == controlPoint)
            .Select(item => item.Sku)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        AssertTrue(
            actualSkus.SequenceEqual(expectedSkus.OrderBy(item => item, StringComparer.Ordinal), StringComparer.Ordinal),
            $"{controlPoint} membership should be explicit and exclusive");
        AssertTrue(actualSkus.All(sku => data.Inventory.Any(item => item.Sku == sku)), $"{controlPoint} members should have inventory evidence");
    }

    AssertTrue(data.Skus.All(item => item.DecouplingPoint != "热控结构件超市"), "seed must not retain the old thermal-structure supermarket name");
    AssertTrue(data.Skus.All(item => item.DecouplingPoint != "星载电子半成品超市"), "seed must not retain the old avionics supermarket name");
    AssertTrue(
        data.Skus.Single(item => item.Sku == "AV-FPGA-203").DecouplingPoint != "星载电子半成品库存控制点",
        "FPGA must not also belong to the avionics semi-finished inventory control point");
}

static void TestCapacityProtectionRequiresSequencedUpstreamEvidence()
{
    var data = new SeedScenarioWorkspaceDataSource(SeedData.Create())
        .Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1)));
    var protections = data.CapacityProtections ?? Array.Empty<CapacityProtectionDefinition>();

    AssertTrue(
        data.ResourceRoutings.Any(item => item.OperationSequence > 1 && item.ProtectsCcrResourceCode is not null),
        "ResourceRouting lacks operation sequence and protected-CCR evidence");
    AssertEqual(1, protections.Count, "explicit capacity-protection definition count");
    var protection = protections.Single();
    AssertEqual("RES-AIT", protection.UpstreamResourceCode, "capacity-protection upstream resource");
    AssertEqual("RES-HARNESS", protection.ProtectedCcrResourceCode, "capacity-protection CCR");
    AssertEqual(20m, protection.ReservePercent, "AIT protection reserve percent");
    AssertEqual("Complete", protection.EvidenceStatus, "capacity-protection evidence status");
    AssertTrue(protection.UpstreamResourceCode != protection.ProtectedCcrResourceCode, "CCR unused capacity must not protect itself");

    var upstreamEvidence = data.ResourceRoutings
        .Where(item => item.ResourceCode == protection.UpstreamResourceCode)
        .Where(item => item.ProtectsCcrResourceCode == protection.ProtectedCcrResourceCode)
        .Where(item => item.EvidenceStatus == "Complete")
        .ToList();
    AssertTrue(upstreamEvidence.Count > 0, "capacity protection requires explicit upstream routing evidence");
    AssertTrue(
        upstreamEvidence.All(upstream => data.ResourceRoutings.Any(downstream =>
            downstream.Sku == upstream.Sku &&
            downstream.ResourceCode == protection.ProtectedCcrResourceCode &&
            downstream.OperationSequence > upstream.OperationSequence &&
            downstream.EvidenceStatus == "Complete")),
        "capacity protection requires a later protected-CCR operation for the same SKU");
}

static void TestCapacityProtectionDoesNotInferWithoutSequenceEvidence()
{
    var seed = SeedData.Create();
    var unsequencedSeed = seed with
    {
        ResourceRoutings = seed.ResourceRoutings
            .Select(item => item.ProtectsCcrResourceCode is null
                ? item
                : item with { OperationSequence = 0 })
            .ToList()
    };
    var unsequenced = new SeedScenarioWorkspaceDataSource(unsequencedSeed)
        .Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1)));
    AssertEqual(0, (unsequenced.CapacityProtections ?? Array.Empty<CapacityProtectionDefinition>()).Count, "unsequenced capacity-protection count");

    var seeded = new SeedScenarioWorkspaceDataSource(seed)
        .Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1)));
    AssertTrue(
        (seeded.CapacityProtections ?? Array.Empty<CapacityProtectionDefinition>())
            .All(item => item.UpstreamResourceCode != "RES-TVAC"),
        "TVAC spare capacity is an uncommitted margin, not inferred protection");
    AssertTrue(
        seed.FeasibilityChecks.All(item => !item.RequiredAction.Contains("热真空保护能力", StringComparison.Ordinal)),
        "TVAC feasibility guidance must describe uncommitted capacity margin rather than protection capacity");
    AssertTrue(
        (seeded.CapacityProtections ?? Array.Empty<CapacityProtectionDefinition>())
            .All(item => item.UpstreamResourceCode != item.ProtectedCcrResourceCode),
        "CCR unused capacity must never be inferred as its own protection consumption");
}

static void TestScenarioCapacityProtectionExcludesFpgaSequenceEvidence()
{
    var seed = SeedData.Create();
    var fpgaRoutes = new[]
    {
        new ResourceRouting("AV-FPGA-203", "RES-AIT", 1m, 1, "RES-HARNESS", "Complete"),
        new ResourceRouting("AV-FPGA-203", "RES-HARNESS", 1m, 2, null, "Complete"),
    };
    var request = new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1));
    var fpgaOnly = new SeedScenarioWorkspaceDataSource(seed with { ResourceRoutings = fpgaRoutes })
        .Load(request);
    var mixed = new SeedScenarioWorkspaceDataSource(seed with
        {
            ResourceRoutings = seed.ResourceRoutings.Concat(fpgaRoutes).ToList()
        })
        .Load(request);

    AssertEqual(
        0,
        (fpgaOnly.CapacityProtections ?? Array.Empty<CapacityProtectionDefinition>()).Count,
        "FPGA-only routing must not create capacity protection");
    var mixedProtections = mixed.CapacityProtections ?? Array.Empty<CapacityProtectionDefinition>();
    AssertEqual(1, mixedProtections.Count, "eligible mixed routing capacity-protection count");
    AssertEqual("RES-AIT", mixedProtections.Single().UpstreamResourceCode, "eligible mixed routing upstream resource");
    AssertEqual("RES-HARNESS", mixedProtections.Single().ProtectedCcrResourceCode, "eligible mixed routing protected CCR");
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

static void TestHistoryReviewUsesCumulativeLeadTimeAndProtectionEvidence()
{
    var historySource = new SeedHistoryOperatingFactSource();
    var scenarioSource = new SeedScenarioWorkspaceDataSource(SeedData.Create());
    var service = new HistoryReviewWorkspaceService(historySource, scenarioSource);

    var result = service.GetReview(6);
    var annual = service.GetReview(12);
    AssertTrue(
        result.MaximumCumulativeLeadTimeDays.HasValue &&
        result.DetailWindowWeeks.HasValue &&
        annual.MaximumCumulativeLeadTimeDays.HasValue &&
        annual.DetailWindowWeeks.HasValue,
        "complete historical parameters must expose lead-time metadata");
    var maximumLeadTimeDays = result.MaximumCumulativeLeadTimeDays.GetValueOrDefault();
    var detailWindowWeeks = result.DetailWindowWeeks.GetValueOrDefault();
    var expectedWeeks = (int)Math.Ceiling(maximumLeadTimeDays / 7m);

    AssertEqual(6, result.TrendMonths, "history trend months");
    AssertEqual(26, result.ObservedTrendWeeks, "six-month history should expose exactly 26 weekly observations");
    AssertEqual(12, annual.TrendMonths, "annual history trend months");
    AssertEqual(52, annual.ObservedTrendWeeks, "twelve-month history should expose exactly 52 weekly observations");
    AssertEqual(expectedWeeks, detailWindowWeeks, "history detail window should follow cumulative lead time");
    var serializedReview = JsonSerializer.SerializeToNode(
        result,
        new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
    var standardReference = serializedReview["standardDdmrpReference"];
    AssertTrue(standardReference is not null, "history review should expose an independent backend standard DDMRP reference");
    AssertEqual(80m, standardReference!["sizing"]!["zones"]!["red"]!.GetValue<decimal>(), "standard reference red zone");
    AssertEqual(120m, standardReference["sizing"]!["zones"]!["yellow"]!.GetValue<decimal>(), "standard reference yellow zone");
    AssertEqual(70m, standardReference["sizing"]!["zones"]!["green"]!.GetValue<decimal>(), "standard reference green zone");
    AssertEqual("OrderCycle", standardReference["sizing"]!["greenDriver"]!.GetValue<string>(), "standard reference green driver");
    AssertEqual(3, result.InventoryBuffers!.Select(item => item.ControlPoint).Distinct(StringComparer.Ordinal).Count(), "historical inventory control points should remain exactly three");
    AssertTrue(result.DdmrpSizingSnapshots!.All(item => item.SnapshotId != "DDMRP-EXAMPLE-V1"), "standard reference must not become a historical control-point snapshot");
    AssertTrue(detailWindowWeeks < result.ObservedTrendWeeks, "cumulative lead time must not truncate the 26-week operating trend");
    AssertTrue(result.OperatingOutcomes.ServiceLevelPercent is > 0, "history should expose operating outcomes");
    AssertTrue(result.ProtectionRelationships.Any(item => item.ProtectionType == "库存缓冲"), "history should expose inventory protection relationships");
    AssertTrue(result.ZoneResidence.Any(item => item.ObservedPeriods == detailWindowWeeks), "history should expose zone residence over the detail window");
    AssertTrue(result.ZoneResidence.All(item => Math.Abs(item.RedPercent + item.YellowPercent + item.GreenPercent + item.OverTopOfGreenPercent - 100m) <= 0.2m), "zone residence proportions should account for the observed window");
    AssertTrue(result.ZoneResidence.All(item => item.RedEntryCount >= 0), "history should count entries into the red zone");
    AssertTrue(result.ZoneResidence.All(item => !string.IsNullOrWhiteSpace(item.PrimaryCause)), "buffer residence should retain explicit historical causes");
    AssertTrue(result.CapacityProtection.Any(item =>
        item.TheoreticalCapacity is not null &&
        item.StandardCapacity is not null &&
        item.DemonstratedCapacity is not null &&
        item.TheoreticalCapacity >= item.StandardCapacity &&
        item.StandardCapacity >= item.DemonstratedCapacity), "history should distinguish capacity layers");

    var ait = result.CapacityProtection.Single(item => item.ResourceCode == "RES-AIT");
    AssertEqual("RES-HARNESS", ait.ProtectedCcrResourceCode, "AIT protected CCR resource");
    AssertTrue(ait.PlannedAvailableCapacity is not null && ait.CommittedLoad is not null, "AIT should carry explicit weekly capacity aggregates");
    var aitPoints = (result.CapacityBuffers ?? throw new InvalidOperationException("historical capacity projection is missing"))
        .Single(item => item.ResourceCode == "RES-AIT")
        .Points
        .Where(item => item.EvidenceStatus == "Complete")
        .ToList();
    var expectedProtection = decimal.Round(aitPoints.Average(item => item.ProtectiveCapacity!.Value), 1);
    var expectedConsumed = decimal.Round(aitPoints.Average(item => item.ConsumedProtection!.Value), 1);
    var expectedRemaining = decimal.Round(aitPoints.Average(item => item.RemainingProtection!.Value), 1);
    AssertEqual(expectedProtection, ait.ProtectiveCapacity, "AIT protective capacity formula");
    AssertEqual(expectedConsumed, ait.ConsumedProtection, "AIT average weekly consumed protection");
    AssertEqual(expectedRemaining, ait.RemainingProtection, "AIT average weekly remaining protection");

    var harness = result.CapacityProtection.Single(item => item.ResourceCode == "RES-HARNESS");
    AssertTrue(harness.ProtectiveCapacity is null && harness.ConsumedProtection is null && harness.RemainingProtection is null, "CCR itself should expose utilization rather than inferred self-protection");
    var tvac = result.CapacityProtection.Single(item => item.ResourceCode == "RES-TVAC");
    AssertTrue(tvac.ProtectiveCapacity is null && tvac.ProtectedCcrResourceCode is null, "TVAC must remain a scenario-potential CCR without a protection definition");

    var withoutHistoricalProtection = new HistoryReviewWorkspaceService(
        new CapacityProtectionRemovingHistoryOperatingFactSource(historySource),
        scenarioSource).GetReview(6);
    var unprotectedAit = withoutHistoricalProtection.CapacityProtection.Single(item => item.ResourceCode == "RES-AIT");
    AssertTrue(
        unprotectedAit.ProtectiveCapacity is null &&
        unprotectedAit.ConsumedProtection is null &&
        unprotectedAit.RemainingProtection is null &&
        unprotectedAit.ProtectedCcrResourceCode is null &&
        unprotectedAit.RelationshipRole != "UpstreamProtection" &&
        unprotectedAit.EvidenceStatus == "EvidenceMissing",
        "missing historical protection evidence must not be replaced by a scenario protection definition");
    AssertTrue(withoutHistoricalProtection.OperatingOutcomes.RemainingProtectionPercent is null, "missing historical protection evidence should leave the aggregate percentage empty");

    var withoutScenarioDefinition = new HistoryReviewWorkspaceService(
        historySource,
        new CapacityProtectionRemovingScenarioWorkspaceDataSource(SeedData.Create())).GetReview(6);
    AssertEqual(
        JsonSerializer.Serialize(result.CapacityProtection),
        JsonSerializer.Serialize(withoutScenarioDefinition.CapacityProtection),
        "removing only the scenario capacity definition must not alter historical capacity summaries");
    AssertEqual(
        JsonSerializer.Serialize(result.CapacityBuffers),
        JsonSerializer.Serialize(withoutScenarioDefinition.CapacityBuffers),
        "removing only the scenario capacity definition must not alter historical capacity projections");
    AssertTrue(result.ConstraintExposure.Any(item => item.ExposureType == "场景潜在 CCR"), "history should classify scenario potential CCR exposure");
}

static void TestHistoryReviewAggregatesDistinctTwentySixAndFiftyTwoWeekFacts()
{
    var historySource = new SeedHistoryOperatingFactSource();
    var service = new HistoryReviewWorkspaceService(
        historySource,
        new SeedScenarioWorkspaceDataSource(SeedData.Create()));
    var asOfDate = new DateOnly(2026, 6, 1);
    var recentFacts = historySource.Load(new HistoryFactRequest(26, asOfDate));
    var annualFacts = historySource.Load(new HistoryFactRequest(52, asOfDate));

    var recent = service.GetReview(6);
    var annual = service.GetReview(12);
    var recentOutcomes = recent.OperatingOutcomes;
    var annualOutcomes = annual.OperatingOutcomes;
    var failures = new List<string>();

    if (recentFacts.OperatingFacts.Select(item => item.WeekOffset).Distinct().Count() != 26 ||
        annualFacts.OperatingFacts.Select(item => item.WeekOffset).Distinct().Count() != 52)
    {
        failures.Add("explicit fact source did not return strict 26/52-week operating windows");
    }

    if (recentFacts.AbnormalCosts.Sum(item => item.CostAmount) != 420_000m ||
        annualFacts.AbnormalCosts.Sum(item => item.CostAmount) != 1_200_000m)
    {
        failures.Add("explicit abnormal-cost events did not total 420,000/1,200,000");
    }

    if (annualFacts.BufferFacts.Any(item => string.IsNullOrWhiteSpace(item.ExplicitCause)) ||
        annualFacts.CapacityFacts.Any(item => item.TheoreticalCapacity is null || item.StandardCapacity is null || item.DemonstratedCapacity is null || item.PlannedAvailableCapacity is null || item.CommittedLoad is null))
    {
        failures.Add("buffer causes or weekly capacity-layer facts were incomplete");
    }

    var expectedExposureTypes = new[] { "当前 CCR", "高负荷资源", "场景潜在 CCR", "事件型约束", "外部约束" };
    if (!expectedExposureTypes.All(type => annualFacts.ConstraintFacts.Any(item => item.ExposureType == type)) ||
        annualFacts.ConstraintFacts.Where(item => item.ExposureType != "场景潜在 CCR").Any(item => item.SourceKind != "HistoricalFact") ||
        annualFacts.ConstraintFacts.Where(item => item.ExposureType == "场景潜在 CCR").Any(item => item.SourceKind != "InternalScenarioDefinition"))
    {
        failures.Add("explicit constraint facts did not preserve all five source-owned classifications");
    }

    if (recent.ObservedTrendWeeks != 26 || annual.ObservedTrendWeeks != 52)
    {
        failures.Add($"expected strict 26/52-week windows, got {recent.ObservedTrendWeeks}/{annual.ObservedTrendWeeks}");
    }

    if (recentOutcomes.ServiceLevelPercent is not (>= 96.5m and <= 97.5m) ||
        annualOutcomes.ServiceLevelPercent is not (>= 95.5m and <= 96.5m))
    {
        failures.Add($"service ranges were {recentOutcomes.ServiceLevelPercent:0.0}%/{annualOutcomes.ServiceLevelPercent:0.0}%");
    }

    if (recentOutcomes.InventoryValue is not (>= 65_000_000m and <= 75_000_000m) ||
        annualOutcomes.InventoryValue is not (>= 72_000_000m and <= 82_000_000m))
    {
        failures.Add($"inventory ranges were {recentOutcomes.InventoryValue:0}/{annualOutcomes.InventoryValue:0}");
    }

    if (recentOutcomes.WorkInProcessUnits is not (>= 55m and <= 70m) ||
        annualOutcomes.WorkInProcessUnits is not (>= 65m and <= 80m))
    {
        failures.Add($"WIP ranges were {recentOutcomes.WorkInProcessUnits:0.0}/{annualOutcomes.WorkInProcessUnits:0.0}");
    }

    if (recentOutcomes.AverageFlowTimeDays is not (>= 17m and <= 20m) ||
        annualOutcomes.AverageFlowTimeDays is not (>= 20m and <= 24m))
    {
        failures.Add($"flow-time ranges were {recentOutcomes.AverageFlowTimeDays:0.0}/{annualOutcomes.AverageFlowTimeDays:0.0}");
    }

    if (recentOutcomes.CashOccupied is not (>= 78_000_000m and <= 90_000_000m) ||
        annualOutcomes.CashOccupied is not (>= 90_000_000m and <= 105_000_000m))
    {
        failures.Add($"cash ranges were {recentOutcomes.CashOccupied:0}/{annualOutcomes.CashOccupied:0}");
    }

    if (recentOutcomes.ExpediteCost != 420_000m || annualOutcomes.ExpediteCost != 1_200_000m)
    {
        failures.Add($"abnormal costs were {recentOutcomes.ExpediteCost:0}/{annualOutcomes.ExpediteCost:0}");
    }

    if (recentOutcomes.ServiceLevelPercent == annualOutcomes.ServiceLevelPercent &&
        recentOutcomes.InventoryValue == annualOutcomes.InventoryValue &&
        recentOutcomes.WorkInProcessUnits == annualOutcomes.WorkInProcessUnits &&
        recentOutcomes.AverageFlowTimeDays == annualOutcomes.AverageFlowTimeDays &&
        recentOutcomes.CashOccupied == annualOutcomes.CashOccupied &&
        recentOutcomes.ExpediteCost == annualOutcomes.ExpediteCost)
    {
        failures.Add("all six operating outcomes were identical across the two windows");
    }

    AssertTrue(failures.Count == 0, string.Join("; ", failures));
}

static void TestHistoryReviewProjectsExplicitBufferViews()
{
    var seed = SeedData.Create();
    var historySource = new SeedHistoryOperatingFactSource(seed);
    var service = new HistoryReviewWorkspaceService(
        historySource,
        new SeedScenarioWorkspaceDataSource(seed));
    var recent = service.GetReview(6);
    var annual = service.GetReview(12);
    var recentFacts = historySource.Load(new HistoryFactRequest(26, new DateOnly(2026, 6, 1)));
    var inventory = recent.InventoryBuffers ?? throw new InvalidOperationException("six-month inventory projection is missing");
    var sizing = recent.DdmrpSizingSnapshots ?? throw new InvalidOperationException("historical sizing projection is missing");
    var time = recent.TimeBuffers ?? throw new InvalidOperationException("six-month time-buffer projection is missing");
    var capacity = recent.CapacityBuffers ?? throw new InvalidOperationException("six-month capacity projection is missing");
    var annualInventory = annual.InventoryBuffers ?? throw new InvalidOperationException("annual inventory projection is missing");
    var annualSizing = annual.DdmrpSizingSnapshots ?? throw new InvalidOperationException("annual sizing projection is missing");
    var annualTime = annual.TimeBuffers ?? throw new InvalidOperationException("annual time-buffer projection is missing");
    var annualCapacity = annual.CapacityBuffers ?? throw new InvalidOperationException("annual capacity projection is missing");

    AssertEqual(5, inventory.Count, "six-month historical inventory material count");
    AssertTrue(inventory.All(item => item.Points.Count == 26), "every six-month inventory material must expose exactly 26 points");
    AssertTrue(
        annualInventory.Count > 0 &&
        annualSizing.Count > 0 &&
        annualTime.Count > 0 &&
        annualCapacity.Count > 0,
        "annual inventory sizing time and capacity projections must all be non-empty");
    AssertTrue(annualInventory.All(item => item.Points.Count == 52), "every annual inventory material must expose exactly 52 points");
    AssertTrue(time.Count > 0 && time.All(item => item.Points.Count == 26), "six-month time-buffer views must expose exactly 26 points");
    AssertTrue(annualTime.All(item => item.Points.Count == 52), "annual time-buffer views must expose exactly 52 points");
    AssertTrue(capacity.Count > 0 && capacity.All(item => item.Points.Count == 26), "six-month capacity views must expose exactly 26 points");
    AssertTrue(annualCapacity.All(item => item.Points.Count == 52), "annual capacity views must expose exactly 52 points");
    AssertTrue(
        inventory.All(item => item.Distribution.Select(bucket => bucket.Code).SequenceEqual(new[] { "Red", "Yellow", "Green", "OverTopOfGreen" }, StringComparer.Ordinal)),
        "inventory distributions must use the four deterministic zone buckets");
    AssertTrue(
        inventory.All(item => Math.Abs(item.Distribution.Sum(bucket => bucket.Percent) - 100m) <= 0.2m),
        "complete inventory distributions must total 100 percent");

    var sourceTimeFacts = (recentFacts.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>())
        .ToDictionary(item => (item.BufferId, item.WeekOffset));
    var sourceCosts = recentFacts.AbnormalCosts.ToDictionary(item => item.EventId, StringComparer.Ordinal);
    foreach (var view in time)
    {
        foreach (var point in view.Points)
        {
            var fact = sourceTimeFacts[(view.BufferId, point.WeekOffset)];
            AssertEqual(fact.EarlyCount, point.EarlyCount, $"{view.BufferId} week {point.WeekOffset} early count");
            AssertEqual(fact.GreenCount, point.GreenCount, $"{view.BufferId} week {point.WeekOffset} green count");
            AssertEqual(fact.YellowCount, point.YellowCount, $"{view.BufferId} week {point.WeekOffset} yellow count");
            AssertEqual(fact.RedCount, point.RedCount, $"{view.BufferId} week {point.WeekOffset} red count");
            AssertEqual(fact.LateCount, point.LateCount, $"{view.BufferId} week {point.WeekOffset} late count");
            var expectedCost = fact.AbnormalCostEventId is null
                ? null
                : (decimal?)sourceCosts[fact.AbnormalCostEventId].CostAmount;
            AssertEqual(expectedCost, point.AbnormalCost, $"{view.BufferId} week {point.WeekOffset} abnormal cost link");
        }
    }
    AssertTrue(
        time.All(item => item.Distribution.Select(bucket => bucket.Code).SequenceEqual(new[] { "Early", "Green", "Yellow", "Red", "Late" }, StringComparer.Ordinal) &&
            Math.Abs(item.Distribution.Sum(bucket => bucket.Percent) - 100m) <= 0.2m),
        "time distributions must preserve all five bands and total 100 percent");

    var upstreamProtection = capacity.Where(item => item.RelationshipRole == "UpstreamProtection").ToList();
    AssertEqual(1, upstreamProtection.Count, "historical upstream capacity-protection example count");
    AssertEqual("RES-AIT", upstreamProtection[0].ResourceCode, "historical upstream protection resource");
    AssertEqual("RES-HARNESS", upstreamProtection[0].ProtectedCcrResourceCode, "historical protected CCR resource");
    var ccr = capacity.Single(item => item.ResourceCode == "RES-HARNESS");
    AssertEqual("CcrUtilization", ccr.RelationshipRole, "CCR must expose utilization reference rather than self-protection");
    AssertTrue(ccr.Points.All(item => item.ProtectiveCapacity is null && item.ConsumedProtection is null && item.RemainingProtection is null), "CCR must not calculate self-protection");
    AssertTrue(
        capacity.All(item => item.Distribution.Select(bucket => bucket.Code).SequenceEqual(new[] { "Safe", "High", "NearLimit", "Overload" }, StringComparer.Ordinal) &&
            Math.Abs(item.Distribution.Sum(bucket => bucket.Percent) - 100m) <= 0.2m),
        "capacity distributions must use committed-load ratios and total 100 percent");

    AssertTrue(inventory.Any(item => item.Sku == "AV-FPGA-203"), "FPGA must remain in historical inventory");
    AssertTrue(sizing.Any(item => item.Sku == "AV-FPGA-203"), "FPGA must retain its historical inventory parameter snapshots");
    AssertTrue(
        time.All(item => !string.Join(" ", item.BufferId, item.ControlPoint, item.ProtectedActivity).Contains("FPGA", StringComparison.OrdinalIgnoreCase)) &&
        capacity.All(item => !string.Join(" ", item.ResourceCode, item.ResourceName, item.ProtectedCcrResourceCode).Contains("FPGA", StringComparison.OrdinalIgnoreCase)),
        "FPGA must not enter historical time or capacity views");
    AssertTrue(sizing.All(item => item.Sizing is not null && item.SizingLines.Count > 0 && item.AverageOnHand is not null), "complete historical parameter snapshots must expose sizing details and average on-hand evidence");
}

static void TestHistoryCapacitySummariesAverageWeeklyProtectionConsumption()
{
    var seed = SeedData.Create();
    var review = new HistoryReviewWorkspaceService(
        new SeedHistoryOperatingFactSource(seed),
        new SeedScenarioWorkspaceDataSource(seed)).GetReview(6);
    var view = (review.CapacityBuffers ?? throw new InvalidOperationException("historical capacity projection is missing"))
        .Single(item => item.ResourceCode == "RES-AIT");
    var completePoints = view.Points
        .Where(item => item.EvidenceStatus == "Complete")
        .ToList();
    var expectedProtective = decimal.Round(completePoints.Average(item => item.ProtectiveCapacity!.Value), 1);
    var expectedConsumed = decimal.Round(completePoints.Average(item => item.ConsumedProtection!.Value), 1);
    var expectedRemaining = decimal.Round(completePoints.Average(item => item.RemainingProtection!.Value), 1);
    var layer = review.CapacityProtection.Single(item => item.ResourceCode == "RES-AIT");

    AssertTrue(expectedConsumed > 0m, "seed history must exercise non-zero weekly protection consumption");
    AssertEqual<decimal?>(expectedProtective, layer.ProtectiveCapacity, "average weekly protective capacity");
    AssertEqual<decimal?>(expectedConsumed, layer.ConsumedProtection, "average weekly consumed protection");
    AssertEqual<decimal?>(expectedRemaining, layer.RemainingProtection, "average weekly remaining protection");
    AssertEqual<decimal?>(
        decimal.Round(expectedRemaining * 100m / expectedProtective, 1),
        review.OperatingOutcomes.RemainingProtectionPercent,
        "remaining protection percentage from weekly protection aggregates");
}

static void TestHistoryCapacityProtectionRejectsSelfReference()
{
    var seed = SeedData.Create();
    var selfReference = new HistoricalCapacityProtectionFact(
        "HIST-SELF-REF",
        "RES-AIT",
        "RES-AIT",
        10,
        20,
        20m,
        -52,
        -1,
        "Complete");
    var source = new CapacityProtectionTransformingHistoryOperatingFactSource(
        new SeedHistoryOperatingFactSource(seed),
        _ => new[] { selfReference });
    var review = new HistoryReviewWorkspaceService(
        source,
        new SeedScenarioWorkspaceDataSource(seed)).GetReview(6);
    var aitView = (review.CapacityBuffers ?? throw new InvalidOperationException("historical capacity projection is missing"))
        .Single(item => item.ResourceCode == "RES-AIT");
    var aitLayer = review.CapacityProtection.Single(item => item.ResourceCode == "RES-AIT");

    AssertTrue(aitView.RelationshipRole != "UpstreamProtection", "self-referential resource must not become upstream protection");
    AssertTrue(aitView.ProtectedCcrResourceCode is null, "self-referential resource must not expose a protected CCR");
    AssertTrue(
        aitView.Points.All(item => item.ProtectiveCapacity is null && item.ConsumedProtection is null && item.RemainingProtection is null),
        "self-referential evidence must not calculate protection values");
    AssertTrue(
        aitLayer.ProtectiveCapacity is null &&
        aitLayer.ConsumedProtection is null &&
        aitLayer.RemainingProtection is null &&
        aitLayer.EvidenceStatus == "EvidenceMissing",
        "self-referential capacity summary must remain missing evidence");
}

static void TestHistoricalCcrRoleWinsOverUpstreamRole()
{
    var seed = SeedData.Create();
    var secondRelationship = new HistoricalCapacityProtectionFact(
        "HIST-RES-HARNESS-RES-TVAC-V1",
        "RES-HARNESS",
        "RES-TVAC",
        20,
        30,
        15m,
        -52,
        -1,
        "Complete");
    var source = new CapacityProtectionTransformingHistoryOperatingFactSource(
        new SeedHistoryOperatingFactSource(seed),
        existing => existing.Concat(new[] { secondRelationship }).ToList());
    var review = new HistoryReviewWorkspaceService(
        source,
        new SeedScenarioWorkspaceDataSource(seed)).GetReview(6);
    var harness = (review.CapacityBuffers ?? throw new InvalidOperationException("historical capacity projection is missing"))
        .Single(item => item.ResourceCode == "RES-HARNESS");

    AssertEqual("CcrUtilization", harness.RelationshipRole, "CCR relationship role precedence");
    AssertTrue(harness.ProtectedCcrResourceCode is null, "CCR utilization view must not expose another protected CCR");
    AssertEqual("Complete", harness.EvidenceStatus, "CCR utilization evidence status");
    AssertTrue(
        harness.Points.All(item =>
            item.EvidenceStatus == "Complete" &&
            item.ProtectiveCapacity is null &&
            item.ConsumedProtection is null &&
            item.RemainingProtection is null),
        "CCR utilization view must stay complete without calculating upstream protection consumption");
}

static void TestHistoryReviewUsesEffectiveParameterSnapshot()
{
    const string sku = "AV-COM-201";
    var service = new HistoryReviewWorkspaceService(
        new SeedHistoryOperatingFactSource(),
        new SeedScenarioWorkspaceDataSource(SeedData.Create()));
    var annual = service.GetReview(12);
    var inventory = (annual.InventoryBuffers ?? throw new InvalidOperationException("annual inventory projection is missing"))
        .Single(item => item.Sku == sku);
    var priorPoint = inventory.Points.Single(item => item.WeekOffset == -27);
    var currentPoint = inventory.Points.Single(item => item.WeekOffset == -26);

    AssertEqual($"HIST-{sku}-V1", priorPoint.ParameterSnapshotId, "week -27 historical parameter snapshot");
    AssertEqual($"HIST-{sku}-V2", currentPoint.ParameterSnapshotId, "week -26 historical parameter snapshot");
    AssertTrue(
        priorPoint.TopOfRed != currentPoint.TopOfRed ||
        priorPoint.TopOfYellow != currentPoint.TopOfYellow ||
        priorPoint.TopOfGreen != currentPoint.TopOfGreen,
        "zone tops must change when the effective historical parameter snapshot changes");

    var snapshots = (annual.DdmrpSizingSnapshots ?? throw new InvalidOperationException("annual sizing projection is missing"))
        .Where(item => item.Sku == sku)
        .OrderBy(item => item.EffectiveFromWeekOffset)
        .ToList();
    AssertEqual(2, snapshots.Count, "annual historical snapshot count for selected material");
    AssertEqual($"HIST-{sku}-V1", snapshots[0].SnapshotId, "prior sizing snapshot ID");
    AssertEqual($"HIST-{sku}-V2", snapshots[1].SnapshotId, "current sizing snapshot ID");
    var priorSizing = snapshots[0].Sizing ?? throw new InvalidOperationException("prior sizing evidence is missing");
    var currentSizing = snapshots[1].Sizing ?? throw new InvalidOperationException("current sizing evidence is missing");
    AssertEqual<decimal?>(priorSizing.Zones.TopOfRed, priorPoint.TopOfRed, "week -27 top of red from prior sizing");
    AssertEqual<decimal?>(currentSizing.Zones.TopOfRed, currentPoint.TopOfRed, "week -26 top of red from current sizing");
    AssertEqual<decimal?>(priorSizing.Zones.TopOfGreen, priorPoint.TopOfGreen, "week -27 top of green from prior sizing");
    AssertEqual<decimal?>(currentSizing.Zones.TopOfGreen, currentPoint.TopOfGreen, "week -26 top of green from current sizing");

    var recent = service.GetReview(6);
    var invalidParameterReview = new HistoryReviewWorkspaceService(
        new InvalidHistoricalLeadTimeFactorFactSource(new SeedHistoryOperatingFactSource()),
        new SeedScenarioWorkspaceDataSource(SeedData.Create())).GetReview(6);
    var blankIdSnapshot = (invalidParameterReview.DdmrpSizingSnapshots ?? throw new InvalidOperationException("historical sizing projection is missing"))
        .Single(item => item.Sku == "AV-OBC-202");
    AssertTrue(
        blankIdSnapshot.Sizing is null &&
        blankIdSnapshot.SizingLines.Count == 0 &&
        blankIdSnapshot.EvidenceStatus == "EvidenceMissing",
        "blank historical snapshot IDs must not produce sizing results or sizing lines");
    AssertTrue(
        recent.MaximumCumulativeLeadTimeDays.HasValue &&
        recent.DetailWindowWeeks.HasValue &&
        invalidParameterReview.MaximumCumulativeLeadTimeDays.HasValue &&
        invalidParameterReview.DetailWindowWeeks.HasValue,
        "remaining complete historical sizing evidence must keep lead-time metadata available");
    AssertEqual(recent.MaximumCumulativeLeadTimeDays.GetValueOrDefault(), invalidParameterReview.MaximumCumulativeLeadTimeDays.GetValueOrDefault(), "invalid historical sizing evidence must not raise maximum cumulative lead time");
    AssertEqual(recent.DetailWindowWeeks.GetValueOrDefault(), invalidParameterReview.DetailWindowWeeks.GetValueOrDefault(), "invalid historical sizing evidence must not widen the detail window");
    var invalidParameterPoint = (invalidParameterReview.InventoryBuffers ?? throw new InvalidOperationException("inventory projection is missing"))
        .Single(item => item.Sku == sku)
        .Points.Single(item => item.WeekOffset == -1);
    AssertTrue(
        invalidParameterPoint.TopOfRed is null && invalidParameterPoint.EvidenceStatus == "EvidenceMissing",
        "invalid historical lead-time-factor evidence must not produce sizing zones");
}

static void TestHistoryReviewDoesNotBackfillMissingEvidence()
{
    const string missingSnapshotSku = "AV-COM-201";
    const int missingTimeWeek = -7;
    var seed = SeedData.Create();
    var gapSource = new HistoryEvidenceGapOperatingFactSource(
        new SeedHistoryOperatingFactSource(seed),
        $"HIST-{missingSnapshotSku}-V2",
        missingTimeWeek);
    var normal = new HistoryReviewWorkspaceService(
        gapSource,
        new SeedScenarioWorkspaceDataSource(seed)).GetReview(6);
    var poisoned = new HistoryReviewWorkspaceService(
        gapSource,
        new HistoricalQuantityPoisoningScenarioWorkspaceDataSource(seed)).GetReview(6);

    var withoutHistoricalParameters = new HistoryReviewWorkspaceService(
        new DdmrpParametersRemovingHistoryOperatingFactSource(new SeedHistoryOperatingFactSource(seed)),
        new SeedScenarioWorkspaceDataSource(seed)).GetReview(6);
    AssertTrue(
        withoutHistoricalParameters.MaximumCumulativeLeadTimeDays is null &&
        withoutHistoricalParameters.DetailWindowWeeks is null,
        "missing all historical DDMRP parameters must leave lead-time metadata unknown");
    var parameterlessInventory = withoutHistoricalParameters.InventoryBuffers ??
        throw new InvalidOperationException("parameterless inventory projection is missing");
    AssertTrue(
        parameterlessInventory.Count > 0 &&
        parameterlessInventory.All(item =>
            item.Points.Count == 26 &&
            item.EvidenceStatus == "EvidenceMissing" &&
            item.Points.All(point => point.EvidenceStatus == "EvidenceMissing")),
        "missing all historical DDMRP parameters must retain strict slots with explicit missing evidence");

    var missingInventoryView = (normal.InventoryBuffers ?? throw new InvalidOperationException("inventory projection is missing"))
        .Single(item => item.Sku == missingSnapshotSku);
    AssertTrue(
        missingInventoryView.Points.Count == 26 &&
        missingInventoryView.Points.All(item => item.EvidenceStatus == "EvidenceMissing"),
        "inventory projection must retain all slots when the effective SKU snapshot is missing");
    var missingInventoryPoint = missingInventoryView.Points.Single(item => item.WeekOffset == missingTimeWeek);
    AssertEqual("EvidenceMissing", missingInventoryPoint.EvidenceStatus, "inventory point with a missing historical parameter snapshot");
    AssertTrue(
        missingInventoryPoint.TopOfRed is null &&
        missingInventoryPoint.TopOfYellow is null &&
        missingInventoryPoint.TopOfGreen is null,
        "missing historical parameter evidence must leave all zone tops null");
    AssertTrue(
        normal.ZoneResidence.All(item => item.Sku != missingSnapshotSku),
        "zone residence must omit a SKU with no complete detail-window evidence");

    var missingTimePoint = (normal.TimeBuffers ?? throw new InvalidOperationException("time-buffer projection is missing"))
        .Single()
        .Points.Single(item => item.WeekOffset == missingTimeWeek);
    AssertEqual("EvidenceMissing", missingTimePoint.EvidenceStatus, "missing weekly time-buffer fact evidence");
    AssertTrue(
        missingTimePoint.EarlyCount is null &&
        missingTimePoint.GreenCount is null &&
        missingTimePoint.YellowCount is null &&
        missingTimePoint.RedCount is null &&
        missingTimePoint.LateCount is null &&
        missingTimePoint.AbnormalCost is null,
        "missing time-buffer facts must remain null rather than being backfilled as zero");

    AssertEqual(
        JsonSerializer.Serialize(normal.InventoryBuffers),
        JsonSerializer.Serialize(poisoned.InventoryBuffers),
        "scenario-poisoned current and future quantities must not alter historical inventory projections");
    AssertEqual(
        JsonSerializer.Serialize(normal.DdmrpSizingSnapshots),
        JsonSerializer.Serialize(poisoned.DdmrpSizingSnapshots),
        "scenario-poisoned current parameters must not alter historical sizing snapshots");
    AssertEqual(
        JsonSerializer.Serialize(normal.TimeBuffers),
        JsonSerializer.Serialize(poisoned.TimeBuffers),
        "scenario-poisoned current and future quantities must not alter historical time projections");
    AssertEqual(
        JsonSerializer.Serialize(normal.CapacityBuffers),
        JsonSerializer.Serialize(poisoned.CapacityBuffers),
        "scenario-poisoned current and future quantities must not alter historical capacity projections");

    var duplicateCostEvidence = new HistoryReviewWorkspaceService(
        new DuplicateCostEventHistoryOperatingFactSource(new SeedHistoryOperatingFactSource(seed)),
        new SeedScenarioWorkspaceDataSource(seed)).GetReview(6);
    var ambiguousCostPoint = (duplicateCostEvidence.TimeBuffers ?? throw new InvalidOperationException("time-buffer projection is missing"))
        .Single()
        .Points.Single(item => item.WeekOffset == -18);
    AssertTrue(
        ambiguousCostPoint.AbnormalCost is null && ambiguousCostPoint.EvidenceStatus == "EvidenceMissing",
        "an event ID that joins more than one historical cost event must remain missing evidence");

    var reconciliationFailure = new HistoryReviewWorkspaceService(
        new InventoryReconciliationPoisonHistoryOperatingFactSource(new SeedHistoryOperatingFactSource(seed)),
        new SeedScenarioWorkspaceDataSource(seed)).GetReview(6);
    var invalidInventoryPoint = (reconciliationFailure.InventoryBuffers ?? throw new InvalidOperationException("inventory projection is missing"))
        .Single(item => item.Sku == missingSnapshotSku)
        .Points.Single(item => item.WeekOffset == -8);
    AssertTrue(
        invalidInventoryPoint.TopOfRed is null &&
        invalidInventoryPoint.TopOfYellow is null &&
        invalidInventoryPoint.TopOfGreen is null &&
        invalidInventoryPoint.EvidenceStatus == "EvidenceMissing",
        "an unreconciled inventory point must not expose zone tops from an otherwise valid snapshot");
}

static void TestHistoryFactsExposeVersionedInventoryTimeAndCapacityEvidence()
{
    var data = SeedData.Create();
    var historySource = new SeedHistoryOperatingFactSource(data);
    var asOfDate = new DateOnly(2026, 6, 1);
    var recent = historySource.Load(new HistoryFactRequest(26, asOfDate));
    var annual = historySource.Load(new HistoryFactRequest(52, asOfDate));
    var expectedInventorySkus = new[]
    {
        "AV-COM-201",
        "AV-FPGA-203",
        "AV-OBC-202",
        "TC-MLI-301",
        "TC-RAD-302",
    };

    AssertEqual(26, recent.BufferFacts.Select(item => item.WeekOffset).Distinct().Count(), "recent buffer weeks");
    AssertEqual(52, annual.BufferFacts.Select(item => item.WeekOffset).Distinct().Count(), "annual buffer weeks");
    AssertEqual(expectedInventorySkus.Length * 52, annual.BufferFacts.Count, "annual buffer fact count");
    AssertTrue(
        annual.BufferFacts.Select(item => item.Sku).Distinct().OrderBy(item => item, StringComparer.Ordinal)
            .SequenceEqual(expectedInventorySkus, StringComparer.Ordinal),
        "history should contain exactly the five specified inventory SKUs");
    AssertTrue(
        expectedInventorySkus.All(sku => annual.BufferFacts.Count(item => item.Sku == sku) == 52),
        "every historical inventory SKU should cover 52 weeks");
    AssertTrue(
        annual.BufferFacts.All(item => item.WeekOffset < 0 && item.EndingOnHand is not null && item.OpenSupply is not null && item.QualifiedDemand is not null),
        "inventory components should be explicit and strictly historical");
    AssertTrue(
        annual.BufferFacts.All(item => item.EndingNetFlow == item.EndingOnHand + item.OpenSupply - item.QualifiedDemand),
        "inventory components should reconcile to net flow");

    var parameterFacts = annual.DdmrpParameterFacts ?? Array.Empty<HistoricalDdmrpParameterFact>();
    var recentParameterFacts = recent.DdmrpParameterFacts ?? Array.Empty<HistoricalDdmrpParameterFact>();
    AssertEqual(expectedInventorySkus.Length * 2, parameterFacts.Count, "annual DDMRP parameter version count");
    AssertEqual(expectedInventorySkus.Length, recentParameterFacts.Count, "recent effective DDMRP parameter version count");
    AssertTrue(parameterFacts.Select(item => item.SnapshotId).Distinct().Count() >= 2, "history should contain at least two parameter versions");
    AssertTrue(
        parameterFacts.All(item => item.EffectiveFromWeekOffset < 0 && item.EffectiveThroughWeekOffset < 0),
        "parameter versions should remain strictly historical");
    foreach (var sku in expectedInventorySkus)
    {
        var sourceSetting = data.Skus.Single(item => item.Sku == sku);
        var versions = parameterFacts.Where(item => item.Sku == sku).OrderBy(item => item.EffectiveFromWeekOffset).ToList();
        AssertEqual(2, versions.Count, $"{sku} parameter version count");
        AssertEqual($"HIST-{sku}-V1", versions[0].SnapshotId, $"{sku} prior snapshot ID");
        AssertEqual(-52, versions[0].EffectiveFromWeekOffset, $"{sku} prior snapshot start");
        AssertEqual(-27, versions[0].EffectiveThroughWeekOffset, $"{sku} prior snapshot end");
        AssertEqual($"HIST-{sku}-V2", versions[1].SnapshotId, $"{sku} current snapshot ID");
        AssertEqual(-26, versions[1].EffectiveFromWeekOffset, $"{sku} current snapshot start");
        AssertEqual(-1, versions[1].EffectiveThroughWeekOffset, $"{sku} current snapshot end");
        AssertTrue(versions.All(item => item.ControlPoint == sourceSetting.DecouplingPoint), $"{sku} control point should come from registered validation data");
        AssertTrue(versions.All(item => item.AsOfUtc == "2026-06-01T23:59:59Z"), $"{sku} immutable parameter cutoff");
    }

    AssertTrue(
        annual.BufferFacts.All(buffer => parameterFacts.Count(parameter =>
            parameter.Sku == buffer.Sku &&
            parameter.ControlPoint == buffer.ControlPoint &&
            parameter.SnapshotId == buffer.ParameterSnapshotId &&
            parameter.EffectiveFromWeekOffset <= buffer.WeekOffset &&
            buffer.WeekOffset <= parameter.EffectiveThroughWeekOffset) == 1),
        "every historical buffer point should reference exactly one effective parameter snapshot");

    var timeBufferFacts = annual.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>();
    AssertEqual(52, timeBufferFacts.Select(item => item.WeekOffset).Distinct().Count(), "time-buffer weeks");
    AssertTrue(timeBufferFacts.All(item => item.WeekOffset < 0 && item.BufferId == "MS-TB-001"), "time-buffer facts should be strictly historical and source-owned");
    AssertTrue(timeBufferFacts.Any(item => item.EarlyCount > 0), "time buffer should contain early samples");
    AssertTrue(timeBufferFacts.Any(item => item.GreenCount > 0), "time buffer should contain green samples");
    AssertTrue(timeBufferFacts.Any(item => item.YellowCount > 0), "time buffer should contain yellow samples");
    AssertTrue(timeBufferFacts.Any(item => item.RedCount > 0), "time buffer should contain red samples");
    AssertTrue(timeBufferFacts.Any(item => item.LateCount > 0), "time buffer should contain late samples");
    var linkedCostFacts = timeBufferFacts.Where(item => item.AbnormalCost is not null).ToList();
    AssertTrue(linkedCostFacts.Count > 0, "time-buffer history should link explicit abnormal-cost events");
    AssertTrue(
        linkedCostFacts.All(item => item.AbnormalCostEventId is not null && annual.AbnormalCosts.Count(cost =>
            cost.EventId == item.AbnormalCostEventId && cost.WeekOffset == item.WeekOffset && cost.CostAmount == item.AbnormalCost) == 1),
        "time-buffer cost facts should reconcile to their linked event");

    var protectionFacts = annual.CapacityProtectionFacts ?? Array.Empty<HistoricalCapacityProtectionFact>();
    AssertTrue(protectionFacts.All(item => item.EffectiveFromWeekOffset < 0 && item.EffectiveThroughWeekOffset < 0), "capacity protection should remain strictly historical");
    AssertTrue(protectionFacts.Any(item =>
        item.UpstreamResourceCode == "RES-AIT" &&
        item.ProtectedCcrResourceCode == "RES-HARNESS" &&
        item.UpstreamOperationSequence < item.CcrOperationSequence &&
        item.ReservePercent == 20m),
        "historical sequence evidence should protect the CCR from upstream");
    var aitLoadPercentages = annual.CapacityFacts
        .Where(item => item.ResourceCode == "RES-AIT" && item.PlannedAvailableCapacity > 0m && item.CommittedLoad is not null)
        .Select(item => decimal.Round(item.CommittedLoad!.Value * 100m / item.PlannedAvailableCapacity!.Value, 0))
        .Distinct()
        .ToList();
    AssertTrue(new[] { 52m, 68m, 86m, 104m }.All(target => aitLoadPercentages.Contains(target)), "capacity facts should sample safe, high-load, near-limit and overload categories");

    var unsequencedData = data with
    {
        ResourceRoutings = data.ResourceRoutings
            .Select(item => item.ProtectsCcrResourceCode is null ? item : item with { OperationSequence = 0 })
            .ToList()
    };
    var unsequencedFacts = new SeedHistoryOperatingFactSource(unsequencedData)
        .Load(new HistoryFactRequest(52, asOfDate));
    AssertEqual(0, (unsequencedFacts.CapacityProtectionFacts ?? Array.Empty<HistoricalCapacityProtectionFact>()).Count, "unsequenced capacity protection fact count");

    AssertTrue(annual.BufferFacts.Any(item => item.Sku == "AV-FPGA-203"), "FPGA should remain in inventory history");
    AssertTrue(
        timeBufferFacts.All(item => !string.Join(" ", item.BufferId, item.ControlPoint, item.ProtectedActivity, item.ExplicitCause).Contains("FPGA", StringComparison.OrdinalIgnoreCase)) &&
        protectionFacts.All(item => !string.Join(" ", item.SnapshotId, item.UpstreamResourceCode, item.ProtectedCcrResourceCode).Contains("FPGA", StringComparison.OrdinalIgnoreCase)),
        "FPGA must not enter time-buffer or capacity-protection facts");
}

static void TestHistoryTimeBufferCostsExcludeFpgaEventEvidence()
{
    var facts = new SeedHistoryOperatingFactSource(SeedData.Create())
        .Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 1)));
    var costsById = facts.AbnormalCosts.ToDictionary(item => item.EventId, StringComparer.Ordinal);
    var linkedEvents = (facts.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>())
        .Where(item => item.AbnormalCostEventId is not null)
        .Select(item => costsById[item.AbnormalCostEventId!])
        .ToList();

    AssertTrue(linkedEvents.Count > 0, "time-buffer history should retain eligible abnormal-cost links");
    AssertTrue(
        linkedEvents.All(item => !item.Cause.Contains("FPGA", StringComparison.OrdinalIgnoreCase)),
        "time-buffer cost links must not lead back to FPGA evidence");
}

static void TestHistoryCapacityProtectionExcludesFpgaRoutingEvidence()
{
    var data = SeedData.Create();
    var fpgaRoutes = new[]
    {
        new ResourceRouting("AV-FPGA-203", "RES-AIT", 1m, 1, "RES-HARNESS", "Complete"),
        new ResourceRouting("AV-FPGA-203", "RES-HARNESS", 1m, 2, null, "Complete"),
    };
    var mixed = new SeedHistoryOperatingFactSource(data with
        {
            ResourceRoutings = data.ResourceRoutings.Concat(fpgaRoutes).ToList()
        })
        .Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 1)));
    var mixedProtection = (mixed.CapacityProtectionFacts ?? Array.Empty<HistoricalCapacityProtectionFact>()).Single();
    var fpgaOnly = new SeedHistoryOperatingFactSource(data with { ResourceRoutings = fpgaRoutes })
        .Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 1)));

    AssertTrue(
        mixedProtection.UpstreamOperationSequence == 10 &&
        mixedProtection.CcrOperationSequence == 20 &&
        (fpgaOnly.CapacityProtectionFacts ?? Array.Empty<HistoricalCapacityProtectionFact>()).Count == 0,
        "capacity protection must retain eligible sequence evidence and reject FPGA-only routing evidence");
}

static void TestHistoricalOutcomesUseExplicitFactsAndTraceableCosts()
{
    var seed = SeedData.Create();
    var historySource = new SeedHistoryOperatingFactSource();
    var normalSource = new SeedScenarioWorkspaceDataSource(seed);
    var poisonedSource = new HistoricalQuantityPoisoningScenarioWorkspaceDataSource(seed);
    var normal = new HistoryReviewWorkspaceService(historySource, normalSource).GetReview(6);
    var poisoned = new HistoryReviewWorkspaceService(historySource, poisonedSource).GetReview(6);
    var factSet = historySource.Load(new HistoryFactRequest(26, new DateOnly(2026, 6, 1)));
    var failures = new List<string>();

    if (normal.OperatingOutcomes.CashOccupied == normal.OperatingOutcomes.InventoryValue)
    {
        failures.Add("cash occupied was copied from inventory value");
    }

    if (normal.OperatingOutcomes.ExpediteCost != factSet.AbnormalCosts.Sum(item => item.CostAmount))
    {
        failures.Add($"historical cost {normal.OperatingOutcomes.ExpediteCost:0} did not equal explicit event sum {factSet.AbnormalCosts.Sum(item => item.CostAmount):0}");
    }

    if (normal.OperatingOutcomes != poisoned.OperatingOutcomes ||
        !normal.ZoneResidence.SequenceEqual(poisoned.ZoneResidence) ||
        !normal.CapacityProtection.SequenceEqual(poisoned.CapacityProtection) ||
        !normal.ConstraintExposure.SequenceEqual(poisoned.ConstraintExposure))
    {
        failures.Add("current/future scenario quantities changed explicit historical facts or aggregates");
    }

    if (normal.EvidenceLabel.Contains("当前占用", StringComparison.Ordinal) ||
        factSet.EvidenceLabel.Contains("当前占用", StringComparison.Ordinal) ||
        factSet.SourceAuthority.Contains("当前占用", StringComparison.Ordinal))
    {
        failures.Add("history evidence used current-occupation semantics");
    }

    var missingCash = new HistoryReviewWorkspaceService(
        new MissingCashHistoryOperatingFactSource(historySource),
        normalSource).GetReview(6);
    if (missingCash.OperatingOutcomes.CashOccupied is not null ||
        missingCash.OperatingOutcomes.EvidenceStatus != "EvidenceMissing")
    {
        failures.Add("missing cash facts were disguised as zero or complete evidence");
    }

    AssertTrue(failures.Count == 0, string.Join("; ", failures));
}

static void TestCurrentBaselineExposesSnapshotKpisWithSourceAndAsOf()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-current-baseline-kpis-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var service = new CurrentBaselineService(new SeedCurrentBaselineDataSource(validationData), databasePath);
        var candidate = service.GetCandidate();
        var kpis = candidate.Payload.Kpis;

        AssertTrue(kpis is not null, "candidate should expose meeting snapshot KPIs");
        AssertTrue(kpis!.ServiceLevelPercent is > 0m and <= 100m, "snapshot service level should be evidence-backed");
        AssertTrue(!string.IsNullOrWhiteSpace(kpis.ServiceWindow), "snapshot service level should name its statistical window");
        AssertEqual(
            validationData.Inventory.Join(validationData.Skus, item => item.Sku, sku => sku.Sku, (item, sku) => item.OnHand * sku.UnitCost).Sum(),
            kpis.InventoryValue,
            "snapshot inventory value");
        AssertEqual(candidate.Payload.WorkInProcess.Sum(item => item.Quantity), kpis.WorkInProcessUnits, "snapshot WIP units");
        AssertEqual(candidate.Payload.Backlog.Sum(item => item.Quantity), kpis.BacklogUnits, "snapshot backlog units");
        AssertTrue(kpis.SupplyCoverageWeeks is > 0m, "snapshot supply coverage should be present");
        AssertTrue(kpis.PeakResourceLoadPercent is > 0m, "snapshot peak resource load should be present");
        AssertEqual(candidate.AsOfUtc, kpis.AsOfUtc, "snapshot KPI as-of time");
        AssertEqual("Complete", kpis.EvidenceStatus, "snapshot KPI evidence status");

        var kpiSection = candidate.Sections.Single(item => item.SectionCode == "CURRENT_KPIS");
        AssertEqual(kpis.SourceAuthority, kpiSection.SourceAuthority, "KPI source authority");
        AssertEqual(kpis.AsOfUtc, kpiSection.AsOfUtc, "KPI section as-of time");
        AssertEqual("DemoFixture", kpiSection.EvidenceLabel, "KPI evidence label");

        var frozen = service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", "meeting snapshot KPIs"));
        var loaded = service.GetDetail(frozen.SnapshotId)!;
        AssertEqual(
            JsonSerializer.Serialize(frozen.Payload.Kpis),
            JsonSerializer.Serialize(loaded.Payload.Kpis),
            "frozen KPI JSON round trip");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestCurrentBaselineRejectsMissingSnapshotKpiEvidence()
{
    var databasePaths = new List<string>();
    try
    {
        var baseData = SeedData.Create();
        var basePlanningInputs = new SeedScenarioWorkspaceDataSource(baseData)
            .Load(new ScenarioWorkspaceDataRequest(52, new DateOnly(2026, 6, 1)));
        var skuWithoutCost = basePlanningInputs.Inventory.First().Sku;
        var cases = new List<(
            string Name,
            ValidationData Data,
            ScenarioWorkspaceDataSet PlanningInputs,
            Func<BaselineKpiSnapshot, decimal?> MissingValue)>
        {
            (
                "empty WIP",
                baseData with { ResourceRoutings = Array.Empty<ResourceRouting>() },
                basePlanningInputs,
                kpis => kpis.WorkInProcessUnits),
            (
                "empty backlog",
                baseData with { Demand = baseData.Demand.Where(item => item.Week != 1).ToList() },
                basePlanningInputs,
                kpis => kpis.BacklogUnits),
            (
                "missing resource evidence",
                baseData,
                basePlanningInputs with { Resources = Array.Empty<CapacityResource>() },
                kpis => kpis.PeakResourceLoadPercent),
            (
                "partial inventory cost mapping",
                baseData,
                basePlanningInputs with
                {
                    Skus = basePlanningInputs.Skus.Where(item => item.Sku != skuWithoutCost).ToList()
                },
                kpis => kpis.InventoryValue),
            (
                "empty inventory coverage",
                baseData,
                basePlanningInputs with { Inventory = Array.Empty<InventoryPosition>() },
                kpis => kpis.SupplyCoverageWeeks),
            (
                "empty routing peak load",
                baseData,
                basePlanningInputs with { ResourceRoutings = Array.Empty<ResourceRouting>() },
                kpis => kpis.PeakResourceLoadPercent),
            (
                "mismatched routed week-1 demand peak load",
                baseData,
                basePlanningInputs with
                {
                    Demand = new[] { new WeeklyDemand("UNROUTED-SKU", 1, 100m) }
                },
                kpis => kpis.PeakResourceLoadPercent)
        };
        var failures = new List<string>();

        foreach (var item in cases)
        {
            var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-current-baseline-missing-kpi-{Guid.NewGuid():N}.db");
            databasePaths.Add(databasePath);
            var service = new CurrentBaselineService(
                new SeedCurrentBaselineDataSource(item.Data, new StaticScenarioWorkspaceDataSource(item.PlanningInputs)),
                databasePath);
            var candidate = service.GetCandidate();
            var kpis = candidate.Payload.Kpis!;
            if (item.MissingValue(kpis) is not null)
            {
                failures.Add($"{item.Name} was converted to {item.MissingValue(kpis)} instead of null");
            }
            if (kpis.EvidenceStatus != "EvidenceMissing")
            {
                failures.Add($"{item.Name} left KPI evidence status as {kpis.EvidenceStatus}");
            }

            var kpiSection = candidate.Sections.Single(section => section.SectionCode == "CURRENT_KPIS");
            if (!kpiSection.IsRequired || kpiSection.CompletenessStatus != "EvidenceMissing")
            {
                failures.Add($"{item.Name} left CURRENT_KPIS freeze-ready");
            }

            var rejected = false;
            try
            {
                service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", item.Name));
            }
            catch (ArgumentException ex)
            {
                rejected = ex.Message.Contains("CURRENT_KPIS", StringComparison.Ordinal);
            }
            if (!rejected)
            {
                failures.Add($"{item.Name} did not block freezing through CURRENT_KPIS");
            }
        }

        AssertTrue(failures.Count == 0, string.Join("; ", failures));
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var databasePath in databasePaths)
        {
            DeleteSqliteFiles(databasePath);
        }
    }
}

static void TestCurrentBaselineAppliesRequiredRulesForEmptySectionItems()
{
    var databasePaths = new List<string>();
    try
    {
        var complete = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
        var cases = new List<(
            string SectionCode,
            bool IsRequired,
            IReadOnlyList<BaselineEvidenceItem>? Items,
            bool ShouldReject)>
        {
            ("REQUIRED_NOT_APPLICABLE_NULL", true, null, true),
            ("REQUIRED_NOT_APPLICABLE_EMPTY", true, Array.Empty<BaselineEvidenceItem>(), true),
            ("OPTIONAL_NOT_APPLICABLE_NULL", false, null, false),
            ("OPTIONAL_NOT_APPLICABLE_EMPTY", false, Array.Empty<BaselineEvidenceItem>(), false)
        };
        var failures = new List<string>();

        foreach (var item in cases)
        {
            var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-current-baseline-empty-items-{Guid.NewGuid():N}.db");
            databasePaths.Add(databasePath);
            var section = new BaselineEvidenceSection(
                item.SectionCode,
                item.SectionCode,
                "ReviewFixture",
                complete.AsOfUtc,
                "NotApplicable",
                "NotApplicable",
                0,
                "DemoFixture",
                item.IsRequired,
                "No evidence applies to this review fixture.",
                item.Items);
            var candidate = complete with { Sections = complete.Sections.Append(section).ToList() };
            var service = new CurrentBaselineService(new FixedCurrentBaselineDataSource(candidate), databasePath);
            var rejected = false;
            try
            {
                service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", item.SectionCode));
            }
            catch (ArgumentException ex)
            {
                rejected = ex.Message.Contains(item.SectionCode, StringComparison.Ordinal);
            }

            if (rejected != item.ShouldReject)
            {
                failures.Add($"{item.SectionCode} expected reject={item.ShouldReject} but observed reject={rejected}");
            }
        }

        AssertTrue(failures.Count == 0, string.Join("; ", failures));
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var databasePath in databasePaths)
        {
            DeleteSqliteFiles(databasePath);
        }
    }
}

static void TestCurrentBaselineBlocksIncompleteDdmrpSizingEvidence()
{
    var validationData = SeedData.Create();
    var scenarioSource = new LeadTimeFactorRemovingScenarioWorkspaceDataSource(
        new SeedScenarioWorkspaceDataSource(validationData));
    var candidate = new SeedCurrentBaselineDataSource(validationData, scenarioSource).GetCandidate();

    var section = candidate.Sections.Single(item => item.SectionCode == "DDMRP_SIZING");
    AssertTrue(section.IsRequired, "DDMRP sizing evidence should be required for a new baseline");
    AssertEqual("EvidenceMissing", section.CompletenessStatus, "DDMRP sizing section completeness");
    var missingSku = candidate.Payload.PlanningInputs!.Skus.Single(item => item.LeadTimeFactor is null);
    var blockingItem = section.Items!.Single(item => item.ItemKey == missingSku.Sku);
    AssertEqual("EvidenceMissing", blockingItem.CompletenessStatus, "missing DDMRP sizing item completeness");
    AssertTrue(blockingItem.BlocksFreeze, "missing DDMRP sizing evidence should block freezing");
    AssertTrue(
        blockingItem.MissingReason?.Contains("提前期因子", StringComparison.Ordinal) == true,
        "missing DDMRP sizing evidence should explain the lead-time factor gap");
}

static void TestLegacyFrozenBaselineKeepsMissingLeadTimeFactor()
{
    var validationData = SeedData.Create();
    var candidate = new SeedCurrentBaselineDataSource(validationData).GetCandidate();
    var completeSnapshot = new CurrentBaselineSnapshot(
        "LEGACY-SNAPSHOT",
        "BASE-LEGACY-001",
        "Frozen",
        candidate.AsOfUtc,
        candidate.MasterSettingVersion,
        "legacy reader",
        null,
        "2026-06-30T08:00:00.0000000+00:00",
        candidate.Sections,
        candidate.Payload,
        candidate.EvidenceLabel);
    var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    var legacyJson = JsonNode.Parse(JsonSerializer.Serialize(completeSnapshot, jsonOptions))!.AsObject();
    var skuNodes = legacyJson["payload"]!["planningInputs"]!["skus"]!.AsArray();
    foreach (var skuNode in skuNodes)
    {
        var sku = skuNode!.AsObject();
        AssertTrue(sku.Remove("leadTimeFactor"), "complete snapshot should contain leadTimeFactor");
        AssertTrue(sku.Remove("parameterSnapshotId"), "complete snapshot should contain parameterSnapshotId");
        AssertTrue(sku.Remove("parameterEvidenceStatus"), "complete snapshot should contain parameterEvidenceStatus");
    }

    var restored = JsonSerializer.Deserialize<CurrentBaselineSnapshot>(legacyJson.ToJsonString(jsonOptions), jsonOptions);
    AssertTrue(restored?.Payload.PlanningInputs is not null, "legacy frozen baseline JSON should remain readable");
    AssertTrue(
        restored!.Payload.PlanningInputs!.Skus.All(item => item.LeadTimeFactor is null),
        "missing legacy lead-time factors should remain visible as null");

    AssertInvalidOperationRejected(
        () => new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(validationData))
            .LoadFrozenWorkspaceData(new ScenarioRunPreviewRequest(52), restored),
        "旧版本缺少提前期因子");
}

static void TestCurrentBaselineUiFollowsItemLevelFreezeBlockers()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    AssertTrue(
        script.Contains("function baselineFreezeBlockingIssues(", StringComparison.Ordinal),
        "baseline UI should centralize the same item-level blocking rule used by the backend");
    var blockingRule = SourceFunctionBody(script, "baselineFreezeBlockingIssues");
    AssertTrue(
        blockingRule.Contains("section.items", StringComparison.Ordinal) &&
        blockingRule.Contains("item.blocksFreeze", StringComparison.Ordinal) &&
        blockingRule.Contains("item.freshnessStatus === \"Fresh\"", StringComparison.Ordinal) &&
        blockingRule.Contains("item.completenessStatus === \"Complete\"", StringComparison.Ordinal),
        "baseline UI blocking rule should inspect the serialized item blockers and item evidence state");

    var renderRule = SourceFunctionBody(script, "renderCurrentBaselineWorkspace");
    AssertTrue(
        renderRule.Contains("baselineFreezeBlockingIssues(candidate.sections)", StringComparison.Ordinal),
        "baseline renderer should derive chip and button state from item-level blockers");
    AssertTrue(
        !renderRule.Contains("item.isRequired &&", StringComparison.Ordinal),
        "baseline renderer must not treat a nonblocking missing item as a required-section freeze blocker");
}

static void TestTimeBufferEvidenceRulesControlBaselineFreeze()
{
    var databasePaths = new List<string>();
    string NewDatabasePath(string suffix)
    {
        var path = Path.Combine(Path.GetTempPath(), $"ddae-current-baseline-{suffix}-{Guid.NewGuid():N}.db");
        databasePaths.Add(path);
        return path;
    }

    try
    {
        var validationData = SeedData.Create();
        var planningInputs = new SeedScenarioWorkspaceDataSource(validationData)
            .Load(new ScenarioWorkspaceDataRequest(52, new DateOnly(2026, 6, 1)));
        var timeBuffer = planningInputs.TimeBuffers!.Single();
        var progress = planningInputs.ControlPointProgress!.First(item => item.BufferId == timeBuffer.BufferId);
        var timeSectionCodes = new[] { "TIME_BUFFER_DEFINITIONS", "TIME_BUFFER_PRODUCT_SCOPES", "CONTROL_POINT_PROGRESS" };

        var notApplicableInputs = planningInputs with
        {
            TimeBuffers = Array.Empty<TimeBufferDefinition>(),
            TimeBufferProductScopes = Array.Empty<TimeBufferProductScope>(),
            ControlPointProgress = Array.Empty<ControlPointProgressFact>()
        };
        var notApplicableService = new CurrentBaselineService(
            new SeedCurrentBaselineDataSource(validationData, new StaticScenarioWorkspaceDataSource(notApplicableInputs)),
            NewDatabasePath("time-na"));
        var notApplicable = notApplicableService.GetCandidate();
        AssertTrue(
            notApplicable.Sections.Where(item => timeSectionCodes.Contains(item.SectionCode, StringComparer.Ordinal)).All(item =>
                item.FreshnessStatus == "NotApplicable" &&
                item.CompletenessStatus == "NotApplicable" &&
                item.Items is { Count: 0 }),
            "unconfigured time buffers should be explicitly not applicable with no blocking items");
        AssertEqual(
            "NotApplicable",
            notApplicable.Payload.AnalysisAvailability!.Single(item => item.AnalysisCode == "TimeBuffer").Status,
            "unconfigured time-buffer availability");
        notApplicableService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", "time buffer not applicable"));

        var nonCriticalInputs = planningInputs with
        {
            TimeBuffers = new[] { timeBuffer with { IsCritical = false } },
            ControlPointProgress = new[] { progress with { ObservedDelayDays = null, EvidenceStatus = "EvidenceMissing" } }
        };
        var nonCriticalService = new CurrentBaselineService(
            new SeedCurrentBaselineDataSource(validationData, new StaticScenarioWorkspaceDataSource(nonCriticalInputs)),
            NewDatabasePath("time-noncritical"));
        var nonCritical = nonCriticalService.GetCandidate();
        var nonCriticalProgressSection = nonCritical.Sections.Single(item => item.SectionCode == "CONTROL_POINT_PROGRESS");
        var nonCriticalProgressItem = nonCriticalProgressSection.Items!.Single(item => item.ItemKey == timeBuffer.BufferId);
        AssertEqual("EvidenceMissing", nonCriticalProgressItem.CompletenessStatus, "noncritical progress completeness");
        AssertTrue(!nonCriticalProgressItem.BlocksFreeze, "noncritical missing progress must not block freezing");
        AssertEqual(
            "EvidenceMissing",
            nonCritical.Payload.AnalysisAvailability!.Single(item => item.AnalysisCode == "TimeBuffer").Status,
            "noncritical missing evidence availability");

        var frozen = nonCriticalService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", "noncritical missing time evidence"));
        var loaded = nonCriticalService.GetDetail(frozen.SnapshotId)!;
        var liveSource = new TrackingScenarioWorkspaceDataSource(validationData);
        var frozenWorkspace = new ScenarioRunPreviewService(liveSource)
            .LoadFrozenWorkspaceData(new ScenarioRunPreviewRequest(52), loaded);
        AssertEqual(0, liveSource.LoadCount, "frozen preview must not call the live scenario source");
        AssertTrue(
            frozenWorkspace.ControlPointProgress!.Single(item => item.BufferId == timeBuffer.BufferId).ObservedDelayDays is null,
            "missing frozen delay evidence must remain null rather than becoming zero");
        AssertEqual(
            JsonSerializer.Serialize(loaded.Payload.PlanningInputs!.CapacityProtections),
            JsonSerializer.Serialize(frozenWorkspace.CapacityProtections),
            "frozen capacity-protection evidence");
        AssertEqual(
            JsonSerializer.Serialize(loaded.Payload.PlanningInputs.TimeBuffers),
            JsonSerializer.Serialize(frozenWorkspace.TimeBuffers),
            "frozen time-buffer definitions");
        AssertEqual(
            JsonSerializer.Serialize(loaded.Payload.PlanningInputs.TimeBufferProductScopes),
            JsonSerializer.Serialize(frozenWorkspace.TimeBufferProductScopes),
            "frozen time-buffer product scopes");
        AssertEqual(
            JsonSerializer.Serialize(loaded.Payload.PlanningInputs.ControlPointProgress),
            JsonSerializer.Serialize(frozenWorkspace.ControlPointProgress),
            "frozen control-point progress");

        var criticalMissingInputs = nonCriticalInputs with
        {
            TimeBuffers = new[] { timeBuffer with { IsCritical = true } }
        };
        var criticalMissingService = new CurrentBaselineService(
            new SeedCurrentBaselineDataSource(validationData, new StaticScenarioWorkspaceDataSource(criticalMissingInputs)),
            NewDatabasePath("time-critical-missing"));
        var criticalMissingRejected = false;
        try
        {
            criticalMissingService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", null));
        }
        catch (ArgumentException ex)
        {
            criticalMissingRejected =
                ex.Message.Contains($"CONTROL_POINT_PROGRESS/{timeBuffer.BufferId}", StringComparison.Ordinal) &&
                ex.Message.Contains("ObservedDelayDays", StringComparison.Ordinal);
        }
        AssertTrue(criticalMissingRejected, "critical missing progress should block freezing with section item and reason");

        var criticalStaleInputs = planningInputs with
        {
            ControlPointProgress = new[] { progress with { EvidenceStatus = "Stale" } }
        };
        var criticalStaleService = new CurrentBaselineService(
            new SeedCurrentBaselineDataSource(validationData, new StaticScenarioWorkspaceDataSource(criticalStaleInputs)),
            NewDatabasePath("time-critical-stale"));
        var criticalStaleRejected = false;
        try
        {
            criticalStaleService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", null));
        }
        catch (ArgumentException ex)
        {
            criticalStaleRejected = ex.Message.Contains($"CONTROL_POINT_PROGRESS/{timeBuffer.BufferId}", StringComparison.Ordinal);
        }
        AssertTrue(criticalStaleRejected, "critical stale progress should block freezing");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var databasePath in databasePaths)
        {
            DeleteSqliteFiles(databasePath);
        }
    }
}

static void TestSeedTimeBufferProgressCoversRequestedHorizon()
{
    foreach (var horizonWeeks in new[] { 1, 12, 52 })
    {
        var data = new SeedScenarioWorkspaceDataSource(SeedData.Create())
            .Load(new ScenarioWorkspaceDataRequest(horizonWeeks, new DateOnly(2026, 6, 1)));
        var definition = data.TimeBuffers!.Single();
        var progress = data.ControlPointProgress!
            .Where(item => item.BufferId == definition.BufferId)
            .OrderBy(item => item.Week)
            .ToList();

        AssertEqual(horizonWeeks, progress.Count, $"time-buffer progress row count for {horizonWeeks}-week horizon");
        AssertEqual(
            string.Join('|', Enumerable.Range(1, horizonWeeks)),
            string.Join('|', progress.Select(item => item.Week)),
            $"time-buffer progress weeks for {horizonWeeks}-week horizon");
        AssertTrue(
            progress.All(item => item.ObservedDelayDays.HasValue && item.EvidenceStatus == "Complete"),
            $"time-buffer progress should carry explicit complete evidence for {horizonWeeks}-week horizon");

        var analysis = new TimeBufferProtectionAnalyzer().Analyze(
            data,
            new ExternalScenarioDefinition("EXT-TIME-SEED", "演示时间缓冲证据"),
            null,
            horizonWeeks);
        AssertTrue(
            analysis.Projection.Count == horizonWeeks &&
            analysis.Projection.All(item => item.EvidenceStatus == "Complete" && item.PenetrationPercent.HasValue),
            $"seed time-buffer analysis should be calculable for {horizonWeeks}-week horizon");
    }
}

static void TestTimeBufferBaselineFreezeRejectsPartialHorizonEvidence()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-current-baseline-partial-time-horizon-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var planningInputs = new SeedScenarioWorkspaceDataSource(validationData)
            .Load(new ScenarioWorkspaceDataRequest(52, new DateOnly(2026, 6, 1)));
        var definition = planningInputs.TimeBuffers!.Single();
        var partialInputs = planningInputs with
        {
            ControlPointProgress = planningInputs.ControlPointProgress!
                .Where(item => item.BufferId == definition.BufferId && item.Week <= 3)
                .ToList()
        };
        var service = new CurrentBaselineService(
            new SeedCurrentBaselineDataSource(validationData, new StaticScenarioWorkspaceDataSource(partialInputs)),
            databasePath);
        var candidate = service.GetCandidate();
        var progressItem = candidate.Sections
            .Single(item => item.SectionCode == "CONTROL_POINT_PROGRESS")
            .Items!
            .Single(item => item.ItemKey == definition.BufferId);

        AssertEqual("EvidenceMissing", progressItem.CompletenessStatus, "partial-horizon progress completeness");
        AssertTrue(
            progressItem.MissingReason!.Contains("Week 4", StringComparison.Ordinal),
            "partial-horizon progress should identify the first missing week");

        var rejected = false;
        try
        {
            service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", "partial horizon must not freeze"));
        }
        catch (ArgumentException ex)
        {
            rejected = ex.Message.Contains($"CONTROL_POINT_PROGRESS/{definition.BufferId}", StringComparison.Ordinal) &&
                ex.Message.Contains("Week 4", StringComparison.Ordinal);
        }
        AssertTrue(rejected, "critical partial-horizon progress must block freezing");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestTimeBufferBaselineFreezeRejectsDuplicateWeekEvidence()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-current-baseline-duplicate-time-week-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var planningInputs = new SeedScenarioWorkspaceDataSource(validationData)
            .Load(new ScenarioWorkspaceDataRequest(4, new DateOnly(2026, 6, 1)));
        var definition = planningInputs.TimeBuffers!.Single();
        var duplicate = planningInputs.ControlPointProgress!.Single(item => item.BufferId == definition.BufferId && item.Week == 2);
        var duplicateInputs = planningInputs with
        {
            ControlPointProgress = planningInputs.ControlPointProgress!.Concat(new[] { duplicate }).ToList()
        };
        var service = new CurrentBaselineService(
            new SeedCurrentBaselineDataSource(validationData, new StaticScenarioWorkspaceDataSource(duplicateInputs)),
            databasePath);
        var progressItem = service.GetCandidate().Sections
            .Single(item => item.SectionCode == "CONTROL_POINT_PROGRESS")
            .Items!
            .Single(item => item.ItemKey == definition.BufferId);

        AssertEqual("EvidenceMissing", progressItem.CompletenessStatus, "duplicate-week progress completeness");
        AssertTrue(
            progressItem.MissingReason!.Contains("Week 2", StringComparison.Ordinal) &&
            progressItem.MissingReason.Contains("duplicate", StringComparison.OrdinalIgnoreCase),
            "duplicate progress evidence should identify the duplicated week");

        var rejected = false;
        try
        {
            service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", "duplicate week must not freeze"));
        }
        catch (ArgumentException ex)
        {
            rejected = ex.Message.Contains($"CONTROL_POINT_PROGRESS/{definition.BufferId}", StringComparison.Ordinal) &&
                ex.Message.Contains("Week 2", StringComparison.Ordinal);
        }
        AssertTrue(rejected, "critical duplicate-week progress must block freezing");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestTimeBufferBaselineRejectsStructurallyInconsistentEvidence()
{
    var validationData = SeedData.Create();
    var planningInputs = new SeedScenarioWorkspaceDataSource(validationData)
        .Load(new ScenarioWorkspaceDataRequest(4, new DateOnly(2026, 6, 1)));
    var definition = planningInputs.TimeBuffers!.Single();
    var scope = planningInputs.TimeBufferProductScopes!.Single(item => item.BufferId == definition.BufferId);
    var knownSku = planningInputs.Skus.First(item => item.Sku != "AV-FPGA-203").Sku;
    AssertTrue(definition.IsCritical, "structural evidence tests require a critical seed time buffer");

    var variants = new[]
    {
        (
            Name: "duplicate-definition",
            Inputs: planningInputs with
            {
                TimeBuffers = planningInputs.TimeBuffers!.Concat(new[] { definition }).ToList()
            },
            SectionCode: "TIME_BUFFER_DEFINITIONS",
            ExpectedReason: "duplicate",
            AnalyzerMustReject: false),
        (
            Name: "duplicate-scope",
            Inputs: planningInputs with
            {
                TimeBufferProductScopes = planningInputs.TimeBufferProductScopes!.Concat(new[] { scope }).ToList()
            },
            SectionCode: "TIME_BUFFER_PRODUCT_SCOPES",
            ExpectedReason: "duplicate",
            AnalyzerMustReject: true),
        (
            Name: "unknown-definition-status",
            Inputs: planningInputs with
            {
                TimeBuffers = new[] { definition with { EvidenceStatus = "Verified" } }
            },
            SectionCode: "TIME_BUFFER_DEFINITIONS",
            ExpectedReason: "EvidenceStatus=Verified",
            AnalyzerMustReject: true),
        (
            Name: "unknown-scope-status",
            Inputs: planningInputs with
            {
                TimeBufferProductScopes = new[] { scope with { EvidenceStatus = "Verified" } }
            },
            SectionCode: "TIME_BUFFER_PRODUCT_SCOPES",
            ExpectedReason: "EvidenceStatus=Verified",
            AnalyzerMustReject: true),
        (
            Name: "unknown-sku-scope",
            Inputs: planningInputs with
            {
                TimeBufferProductScopes = new[]
                {
                    scope with { Skus = scope.Skus.Concat(new[] { "UNKNOWN-SKU" }).ToList() }
                }
            },
            SectionCode: "TIME_BUFFER_PRODUCT_SCOPES",
            ExpectedReason: "Unknown SKU",
            AnalyzerMustReject: true),
        (
            Name: "orphan-scope",
            Inputs: planningInputs with
            {
                TimeBufferProductScopes = planningInputs.TimeBufferProductScopes!.Concat(new[]
                {
                    new TimeBufferProductScope("UNKNOWN-TIME-BUFFER", Array.Empty<string>(), new[] { knownSku }, "Complete")
                }).ToList()
            },
            SectionCode: "TIME_BUFFER_PRODUCT_SCOPES",
            ExpectedReason: "undefined time-buffer definition",
            AnalyzerMustReject: false),
        (
            Name: "orphan-progress",
            Inputs: planningInputs with
            {
                ControlPointProgress = planningInputs.ControlPointProgress!.Concat(new[]
                {
                    new ControlPointProgressFact("UNKNOWN-TIME-BUFFER", 1, 0m, "orphan evidence", "Complete")
                }).ToList()
            },
            SectionCode: "CONTROL_POINT_PROGRESS",
            ExpectedReason: "undefined time-buffer definition",
            AnalyzerMustReject: false)
    };

    foreach (var variant in variants)
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-current-baseline-time-structure-{variant.Name}-{Guid.NewGuid():N}.db");
        try
        {
            if (variant.AnalyzerMustReject)
            {
                var analysis = new TimeBufferProtectionAnalyzer().Analyze(
                    variant.Inputs,
                    new ExternalScenarioDefinition($"EXT-{variant.Name}", variant.Name),
                    null,
                    4);
                AssertTrue(
                    analysis.Breaches.Any(item => item.EvidenceStatus == "EvidenceMissing"),
                    $"{variant.Name} should reproduce the downstream analyzer evidence failure");
            }

            var service = new CurrentBaselineService(
                new SeedCurrentBaselineDataSource(validationData, new StaticScenarioWorkspaceDataSource(variant.Inputs)),
                databasePath);
            var candidate = service.GetCandidate();
            var section = candidate.Sections.Single(item => item.SectionCode == variant.SectionCode);
            AssertTrue(
                section.Items!.Any(item =>
                    item.BlocksFreeze &&
                    item.CompletenessStatus == "EvidenceMissing" &&
                    item.MissingReason?.Contains(variant.ExpectedReason, StringComparison.OrdinalIgnoreCase) == true),
                $"{variant.Name} should be a blocking candidate item with reason containing {variant.ExpectedReason}");

            var rejected = false;
            try
            {
                service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", variant.Name));
            }
            catch (ArgumentException ex)
            {
                rejected = ex.Message.Contains(variant.SectionCode, StringComparison.Ordinal) &&
                    ex.Message.Contains(variant.ExpectedReason, StringComparison.OrdinalIgnoreCase);
            }
            AssertTrue(rejected, $"{variant.Name} must be rejected before an immutable baseline is frozen");
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            DeleteSqliteFiles(databasePath);
        }
    }
}

static void TestMixedTimeBufferProgressReportsActualMissingWeek()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-current-baseline-mixed-progress-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var planningInputs = new SeedScenarioWorkspaceDataSource(validationData)
            .Load(new ScenarioWorkspaceDataRequest(52, new DateOnly(2026, 6, 1)));
        var definition = planningInputs.TimeBuffers!.Single();
        var mixedProgress = new[]
        {
            new ControlPointProgressFact(definition.BufferId, 1, 0.5m, "on plan", "Complete"),
            new ControlPointProgressFact(definition.BufferId, 2, 1.5m, "source evidence missing", "EvidenceMissing"),
            new ControlPointProgressFact(definition.BufferId, 3, 0m, "recovered", "Complete")
        };
        var mixedInputs = planningInputs with { ControlPointProgress = mixedProgress };
        var service = new CurrentBaselineService(
            new SeedCurrentBaselineDataSource(validationData, new StaticScenarioWorkspaceDataSource(mixedInputs)),
            databasePath);
        var candidate = service.GetCandidate();
        var progressItem = candidate.Sections
            .Single(section => section.SectionCode == "CONTROL_POINT_PROGRESS")
            .Items!
            .Single(item => item.ItemKey == definition.BufferId);

        AssertEqual("EvidenceMissing", progressItem.CompletenessStatus, "mixed progress completeness");
        AssertTrue(
            progressItem.MissingReason?.Contains("Week 2", StringComparison.Ordinal) == true &&
            progressItem.MissingReason.Contains("EvidenceMissing", StringComparison.Ordinal),
            $"mixed progress should report the actual abnormal week and status, got {progressItem.MissingReason}");

        var rejectedWithActualReason = false;
        try
        {
            service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", "mixed progress evidence"));
        }
        catch (ArgumentException ex)
        {
            rejectedWithActualReason =
                ex.Message.Contains($"CONTROL_POINT_PROGRESS/{definition.BufferId}", StringComparison.Ordinal) &&
                ex.Message.Contains("Week 2", StringComparison.Ordinal) &&
                ex.Message.Contains("EvidenceMissing", StringComparison.Ordinal);
        }
        AssertTrue(rejectedWithActualReason, "critical mixed progress should block freezing with section item week and actual missing status");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestCurrentBaselineFreezesCompleteEvidence()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-current-baseline-{Guid.NewGuid():N}.db");
    try
    {
        var source = new SeedCurrentBaselineDataSource(SeedData.Create());
        var service = new CurrentBaselineService(source, databasePath);

        var candidate = service.GetCandidate();
        AssertTrue(candidate.Sections.All(item => item.CompletenessStatus == "Complete"), "seed baseline sections should be complete");
        AssertTrue(candidate.Sections.All(item => item.FreshnessStatus == "Fresh"), "seed baseline sections should be fresh");
        AssertTrue(candidate.Sections.All(item => item.EvidenceLabel == "DemoFixture"), "seed baseline evidence should remain demo-labelled");
        AssertTrue(candidate.Payload.PlanningInputs is not null, "candidate must freeze the typed planning inputs needed for reproducible recalculation");

        var frozen = service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "月度例会基线"));
        var loaded = service.GetDetail(frozen.SnapshotId);
        var audit = service.GetAuditEvents(frozen.SnapshotId);
        string LoadRawPayloadJson()
        {
            using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT payload_json FROM current_baseline_snapshots WHERE snapshot_id = $snapshot_id;";
            command.Parameters.AddWithValue("$snapshot_id", frozen.SnapshotId);
            return Convert.ToString(command.ExecuteScalar())!;
        }
        var firstPayloadBeforeNextFreeze = LoadRawPayloadJson();
        var nextVersion = service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "下一版本"));
        var firstPayloadAfterNextFreeze = LoadRawPayloadJson();

        AssertEqual("Frozen", frozen.Status, "baseline status");
        AssertTrue(loaded is not null, "frozen baseline should be retrievable");
        AssertEqual(frozen.SnapshotId, loaded!.SnapshotId, "baseline snapshot identity");
        AssertTrue(service.List(10).Count == 2, "baseline list should contain immutable versions");
        AssertTrue(nextVersion.SnapshotNumber.EndsWith("-002", StringComparison.Ordinal), "baseline version number should increment");
        AssertTrue(nextVersion.SnapshotId != frozen.SnapshotId, "new freeze should create a new immutable snapshot");
        AssertTrue(audit.Any(item => item.EventType == "BaselineFrozen"), "baseline freeze should be audited");
        AssertEqual(firstPayloadBeforeNextFreeze, firstPayloadAfterNextFreeze, "later freezes must not rewrite an older payload byte");
        var frozenAudit = audit.Single(item => item.EventType == "BaselineFrozen");
        AssertTrue(!string.IsNullOrWhiteSpace(frozenAudit.PayloadJson), "freeze audit should contain structured evidence payload");
        using var auditDocument = JsonDocument.Parse(frozenAudit.PayloadJson!);
        AssertEqual(candidate.CandidateId, auditDocument.RootElement.GetProperty("candidateId").GetString(), "freeze audit candidate");
        AssertEqual(frozen.SnapshotNumber, auditDocument.RootElement.GetProperty("snapshotNumber").GetString(), "freeze audit snapshot number");
        AssertEqual(frozen.CreatedBy, auditDocument.RootElement.GetProperty("actor").GetString(), "freeze audit actor");
        AssertEqual("Frozen", auditDocument.RootElement.GetProperty("result").GetString(), "freeze audit result");

        var updateBlocked = false;
        var deleteBlocked = false;
        using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}"))
        {
            connection.Open();
            using (var update = connection.CreateCommand())
            {
                update.CommandText = "UPDATE current_baseline_snapshots SET status = 'Changed' WHERE snapshot_id = $snapshot_id;";
                update.Parameters.AddWithValue("$snapshot_id", frozen.SnapshotId);
                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Microsoft.Data.Sqlite.SqliteException)
                {
                    updateBlocked = true;
                }
            }
            using (var delete = connection.CreateCommand())
            {
                delete.CommandText = "DELETE FROM current_baseline_snapshots WHERE snapshot_id = $snapshot_id;";
                delete.Parameters.AddWithValue("$snapshot_id", frozen.SnapshotId);
                try
                {
                    delete.ExecuteNonQuery();
                }
                catch (Microsoft.Data.Sqlite.SqliteException)
                {
                    deleteBlocked = true;
                }
            }
        }
        AssertTrue(updateBlocked && deleteBlocked, "SQLite must enforce frozen snapshot immutability outside the application service");
        AssertEqual("Frozen", service.GetDetail(frozen.SnapshotId)!.Status, "blocked direct SQL must not mutate the frozen snapshot");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestCurrentBaselineRejectsMissingCriticalEvidence()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-current-baseline-{Guid.NewGuid():N}.db");
    try
    {
        var complete = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
        var incomplete = complete with
        {
            Sections = complete.Sections
                .Select(item => item.SectionCode == "WIP" ? item with { CompletenessStatus = "Missing", ItemCount = 0 } : item)
                .ToList()
        };
        var service = new CurrentBaselineService(new FixedCurrentBaselineDataSource(incomplete), databasePath);

        var rejected = false;
        try
        {
            service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", null));
        }
        catch (ArgumentException ex)
        {
            rejected = ex.Message.Contains("WIP", StringComparison.Ordinal);
        }

        AssertTrue(rejected, "missing critical baseline evidence should block freezing");
        AssertTrue(service.List(10).Count == 0, "rejected baseline should not be persisted");

        var missingPlanningInputs = complete with { Payload = complete.Payload with { PlanningInputs = null } };
        var missingPlanningService = new CurrentBaselineService(new FixedCurrentBaselineDataSource(missingPlanningInputs), databasePath);
        var missingPlanningRejected = false;
        try
        {
            missingPlanningService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", null));
        }
        catch (ArgumentException ex)
        {
            missingPlanningRejected = ex.Message.Contains("类型化计划输入", StringComparison.Ordinal);
        }
        AssertTrue(missingPlanningRejected, "new snapshots without typed planning inputs must not be frozen");

        var staleEvidence = complete with
        {
            Sections = complete.Sections
                .Select(item => item.SectionCode == "WIP" ? item with { FreshnessStatus = "Stale" } : item)
                .ToList()
        };
        var staleEvidenceService = new CurrentBaselineService(new FixedCurrentBaselineDataSource(staleEvidence), databasePath);
        var staleEvidenceRejected = false;
        try
        {
            staleEvidenceService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", null));
        }
        catch (ArgumentException ex)
        {
            staleEvidenceRejected = ex.Message.Contains("WIP", StringComparison.Ordinal);
        }
        AssertTrue(staleEvidenceRejected, "stale required baseline evidence should block freezing");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestCurrentBaselineMigratesLegacyAuditPayloadColumn()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-current-baseline-legacy-{Guid.NewGuid():N}.db");
    try
    {
        using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE current_baseline_snapshots (
                    snapshot_id TEXT PRIMARY KEY,
                    snapshot_number TEXT NOT NULL UNIQUE,
                    status TEXT NOT NULL,
                    as_of_utc TEXT NOT NULL,
                    master_setting_version TEXT NOT NULL,
                    created_by TEXT NOT NULL,
                    note TEXT NULL,
                    created_at_utc TEXT NOT NULL,
                    sections_json TEXT NOT NULL,
                    payload_json TEXT NOT NULL,
                    evidence_label TEXT NOT NULL
                );
                CREATE TABLE current_baseline_audit_events (
                    event_id TEXT PRIMARY KEY,
                    snapshot_id TEXT NOT NULL,
                    sequence INTEGER NOT NULL,
                    event_type TEXT NOT NULL,
                    message TEXT NOT NULL,
                    created_at_utc TEXT NOT NULL,
                    UNIQUE(snapshot_id, sequence)
                );
                INSERT INTO current_baseline_audit_events (
                    event_id, snapshot_id, sequence, event_type, message, created_at_utc)
                VALUES ('LEGACY-EVENT', 'LEGACY-SNAPSHOT', 1, 'BaselineFrozen', 'legacy audit', '2026-06-30T08:00:00.0000000+00:00');
                """;
            command.ExecuteNonQuery();
        }

        var service = new CurrentBaselineService(new SeedCurrentBaselineDataSource(SeedData.Create()), databasePath);
        var legacyAudit = service.GetAuditEvents("LEGACY-SNAPSHOT").Single();
        AssertTrue(legacyAudit.PayloadJson is null, "legacy audit rows should remain readable with null structured payload");

        using var migratedConnection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}");
        migratedConnection.Open();
        using var pragma = migratedConnection.CreateCommand();
        pragma.CommandText = "PRAGMA table_info(current_baseline_audit_events);";
        using var reader = pragma.ExecuteReader();
        var columns = new List<string>();
        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }
        AssertTrue(columns.Contains("payload_json", StringComparer.Ordinal), "legacy audit table should gain payload_json additively");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestScenarioAssumptionSourceProvidesOnlyManualAndDemoInputs()
{
    var source = new SeedScenarioAssumptionSource();
    var templates = source.GetTemplates();
    var repeatedTemplates = source.GetTemplates();

    AssertTrue(templates.Count > 0, "the internal assumption source should expose at least one demo fixture");
    AssertEqual(templates.Count, templates.Select(item => item.TemplateId).Distinct(StringComparer.Ordinal).Count(), "demo template IDs should be unique");
    AssertTrue(
        templates.Select(item => $"{item.TemplateId}@{item.TemplateVersion}")
            .SequenceEqual(repeatedTemplates.Select(item => $"{item.TemplateId}@{item.TemplateVersion}")),
        "demo template identity should be stable across reads");

    foreach (var template in templates)
    {
        AssertTrue(!string.IsNullOrWhiteSpace(template.TemplateId), "demo template ID should be explicit");
        AssertTrue(!string.IsNullOrWhiteSpace(template.TemplateVersion), "demo template version should be explicit");
        AssertTrue(!string.IsNullOrWhiteSpace(template.EvidenceLabel), "demo template evidence label should be explicit");
        AssertTrue(template.ExternalScenario.Metadata is not null, "demo fixture should carry source metadata");
        AssertEqual("DemoFixture", template.ExternalScenario.Metadata!.SourceKind, "demo fixture source kind");
        AssertEqual(template.TemplateId, template.ExternalScenario.Metadata.TemplateId!, "demo fixture metadata template ID");
        AssertEqual(template.TemplateVersion, template.ExternalScenario.Metadata.TemplateVersion!, "demo fixture metadata template version");
        AssertEqual(template.EvidenceLabel, template.ExternalScenario.Metadata.EvidenceLabel, "demo fixture metadata evidence label");
        AssertEqual(template.TemplateId, source.GetTemplate(template.TemplateId)!.TemplateId, "demo fixture should be retrievable by stable ID");
        source.Validate(template.ExternalScenario.Metadata);
    }

    source.Validate(new ScenarioAssumptionMetadata(
        " manual ",
        null,
        null,
        "DDS&OP 计划员",
        "2026-07-15T08:00:00Z",
        "2026-07-16",
        "2026-09-30",
        "客户会议确认的人工场景假设",
        "人工录入：客户会议纪要"));
}

static void TestScenarioAssumptionSourceRejectsExternalProtocolSources()
{
    var source = new SeedScenarioAssumptionSource();
    var forbiddenSources = new[]
    {
        "ExternalImport", "File", "Csv", "Json", "Network", "NetworkScore", "SDBR", "DDOM", "Contract", "Unknown"
    };

    foreach (var sourceKind in forbiddenSources)
    {
        var rejected = false;
        try
        {
            source.Validate(new ScenarioAssumptionMetadata(
                sourceKind,
                "EXTERNAL-TEMPLATE",
                "v1",
                "外部连接器",
                "2026-07-15T08:00:00Z",
                "2026-07-16",
                "2026-09-30",
                "外部来源不得进入 DDAE 场景假设",
                "外部协议证据"));
        }
        catch (ArgumentException)
        {
            rejected = true;
        }

        AssertTrue(rejected, $"source kind {sourceKind} should be rejected");
    }
}

static void TestScenarioAssumptionSourceRejectsInvalidManualDates()
{
    var source = new SeedScenarioAssumptionSource();
    var valid = new ScenarioAssumptionMetadata(
        "Manual",
        null,
        null,
        "DDS&OP 计划员",
        "2026-07-15T08:00:00Z",
        "2026-07-16",
        "2026-09-30",
        "客户会议确认的人工场景假设",
        "人工录入：客户会议纪要");
    source.Validate(valid);

    var invalidCases = new (string Name, ScenarioAssumptionMetadata Metadata)[]
    {
        ("unparseable recorded time", valid with { RecordedAtUtc = "not-a-time" }),
        ("non-UTC recorded offset", valid with { RecordedAtUtc = "2026-07-15T08:00:00+08:00" }),
        ("invalid effective from", valid with { EffectiveFrom = "2026-02-30" }),
        ("invalid effective through", valid with { EffectiveThrough = "2026-09-31" }),
        ("non-canonical effective date", valid with { EffectiveFrom = "2026/07/16" }),
        ("inverted effective range", valid with { EffectiveFrom = "2026-10-01", EffectiveThrough = "2026-09-30" })
    };

    foreach (var invalidCase in invalidCases)
    {
        var rejected = false;
        try
        {
            source.Validate(invalidCase.Metadata);
        }
        catch (ArgumentException)
        {
            rejected = true;
        }

        AssertTrue(rejected, $"manual metadata should reject {invalidCase.Name}");
    }
}

static void TestScenarioComparisonRejectsTamperedDemoFixturePayloads()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-demo-provenance-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var baselineService = new CurrentBaselineService(new SeedCurrentBaselineDataSource(validationData), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "演示模板来源校验"));
        var source = new SeedScenarioAssumptionSource();
        var service = new ScenarioComparisonService(
            baselineService,
            new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(validationData)),
            source);
        var template = source.GetTemplates().Single();
        var canonicalScenario = template.ExternalScenario with
        {
            Metadata = template.ExternalScenario.Metadata! with { SourceKind = " demofixture " }
        };

        var canonical = service.Compare(new ScenarioComparisonRequest(
            frozen.SnapshotId,
            canonicalScenario,
            Array.Empty<ResponseConfiguration>(),
            12));
        AssertEqual(template.ExternalScenario.ScenarioId, canonical.ExternalScenario.ScenarioId, "canonical demo fixture scenario ID");

        var mutations = new (string Name, Func<ExternalScenarioDefinition, ExternalScenarioDefinition> Apply)[]
        {
            ("scenario ID", scenario => scenario with { ScenarioId = $"{scenario.ScenarioId}-TAMPERED" }),
            ("scenario name", scenario => scenario with { Name = $"{scenario.Name}-TAMPERED" }),
            ("demand changes", scenario => scenario with
            {
                DemandChanges = (scenario.DemandChanges ?? Array.Empty<ExternalDemandChange>())
                    .Append(new ExternalDemandChange(null, "星载电子", 1, 2, 9m, "篡改需求"))
                    .ToList()
            }),
            ("supply risks", scenario => scenario with
            {
                SupplyRisks = (scenario.SupplyRisks ?? Array.Empty<ExternalSupplyRisk>())
                    .Append(new ExternalSupplyRisk("Microchip Space", "进口空间级 FPGA", 1, 2, 0.1m, "篡改供应"))
                    .ToList()
            }),
            ("capacity losses", scenario => scenario with
            {
                CapacityLosses = (scenario.CapacityLosses ?? Array.Empty<ExternalCapacityLoss>())
                    .Append(new ExternalCapacityLoss("RES-TVAC", 1, 2, 0.1m, "篡改能力"))
                    .ToList()
            }),
            ("known events", scenario => scenario with
            {
                KnownEvents = (scenario.KnownEvents ?? Array.Empty<ExternalKnownEvent>())
                    .Append(new ExternalKnownEvent("EVENT-TAMPERED", "篡改事件", 1, 2))
                    .ToList()
            }),
            ("time delays", scenario => scenario with
            {
                TimeDelays = (scenario.TimeDelays ?? Array.Empty<ExternalTimeDelay>())
                    .Append(new ExternalTimeDelay("TIME-TAMPERED", 1, 2, 5m, "篡改时间事件"))
                    .ToList()
            })
        };

        foreach (var mutation in mutations)
        {
            var rejected = false;
            try
            {
                service.Compare(new ScenarioComparisonRequest(
                    frozen.SnapshotId,
                    mutation.Apply(canonicalScenario),
                    Array.Empty<ResponseConfiguration>(),
                    12));
            }
            catch (ArgumentException)
            {
                rejected = true;
            }

            AssertTrue(rejected, $"demo fixture should reject tampered {mutation.Name}");
        }
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestScenarioDemoTemplateDemandDisturbanceChangesBackendResults()
{
    var comparison = CreateScenarioDemoTemplateEffectComparison("Demand");
    var change = comparison.Template.ExternalScenario.DemandChanges?
        .SingleOrDefault(item => item.Family == "星载电子");
    AssertTrue(change is not null, "demo template demand change should target the real 星载电子 family");
    var demandChange = change!;
    var familySkus = SeedData.Create().Skus
        .Where(item => item.Family == "星载电子")
        .Select(item => item.Sku)
        .ToHashSet(StringComparer.Ordinal);

    var controlDemand = comparison.Control.NoResponse.Preview.Scenario.Plan.BufferProjections
        .Where(item => familySkus.Contains(item.Sku) && item.Week >= demandChange.StartWeek && item.Week <= demandChange.EndWeek)
        .Sum(item => item.Demand);
    var disturbedDemand = comparison.Disturbed.NoResponse.Preview.Scenario.Plan.BufferProjections
        .Where(item => familySkus.Contains(item.Sku) && item.Week >= demandChange.StartWeek && item.Week <= demandChange.EndWeek)
        .Sum(item => item.Demand);

    AssertTrue(controlDemand > 0m, "frozen control should contain 星载电子 demand");
    AssertTrue(disturbedDemand > controlDemand, "demo demand disturbance should increase backend family demand");
}

static void TestScenarioDemoTemplateSupplyDisturbanceChangesBackendResults()
{
    var comparison = CreateScenarioDemoTemplateEffectComparison("Supply");
    var risk = comparison.Template.ExternalScenario.SupplyRisks?
        .SingleOrDefault(item => item.Supplier == "Microchip Space" && item.MaterialFamily == "进口空间级 FPGA");
    AssertTrue(risk is not null, "demo template supply risk should target the real Microchip Space / 进口空间级 FPGA pair");
    var supplyRisk = risk!;

    var controlCells = comparison.Control.NoResponse.Preview.Scenario.Constraints.SupplyCells
        .Where(item => item.Supplier == supplyRisk.Supplier
            && item.MaterialFamily == supplyRisk.MaterialFamily
            && item.Week >= supplyRisk.StartWeek
            && item.Week <= supplyRisk.EndWeek)
        .ToList();
    var disturbedCells = comparison.Disturbed.NoResponse.Preview.Scenario.Constraints.SupplyCells
        .Where(item => item.Supplier == supplyRisk.Supplier
            && item.MaterialFamily == supplyRisk.MaterialFamily
            && item.Week >= supplyRisk.StartWeek
            && item.Week <= supplyRisk.EndWeek)
        .ToList();

    AssertTrue(controlCells.Count > 0 && disturbedCells.Count == controlCells.Count, "frozen comparison should expose the real supplier/material weekly cells");
    AssertTrue(
        disturbedCells.Sum(item => item.ConstrainedAvailable) < controlCells.Sum(item => item.ConstrainedAvailable),
        "demo supply disturbance should reduce backend constrained supplier capacity");
}

static void TestScenarioDemoTemplateCapacityDisturbanceChangesBackendResults()
{
    var comparison = CreateScenarioDemoTemplateEffectComparison("Capacity");
    var loss = comparison.Template.ExternalScenario.CapacityLosses?
        .SingleOrDefault(item => item.ResourceCode == "RES-TVAC");
    AssertTrue(loss is not null, "demo template capacity loss should target the real RES-TVAC resource");
    var capacityLoss = loss!;

    var controlCells = comparison.Control.NoResponse.Preview.Scenario.Constraints.CapacityCells
        .Where(item => item.ResourceCode == capacityLoss.ResourceCode
            && item.Week >= capacityLoss.StartWeek
            && item.Week <= capacityLoss.EndWeek)
        .ToList();
    var disturbedCells = comparison.Disturbed.NoResponse.Preview.Scenario.Constraints.CapacityCells
        .Where(item => item.ResourceCode == capacityLoss.ResourceCode
            && item.Week >= capacityLoss.StartWeek
            && item.Week <= capacityLoss.EndWeek)
        .ToList();

    AssertTrue(controlCells.Count > 0 && disturbedCells.Count == controlCells.Count, "frozen comparison should expose the real RES-TVAC weekly cells");
    AssertTrue(
        disturbedCells.Sum(item => item.ConstrainedAvailable) < controlCells.Sum(item => item.ConstrainedAvailable),
        "demo capacity disturbance should reduce backend constrained resource capacity");
}

static (ScenarioAssumptionTemplate Template, ScenarioComparisonResult Control, ScenarioComparisonResult Disturbed)
    CreateScenarioDemoTemplateEffectComparison(string disturbanceKind)
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-demo-effects-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var baselineService = new CurrentBaselineService(new SeedCurrentBaselineDataSource(validationData), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "演示模板效果验证"));
        var source = new SeedScenarioAssumptionSource();
        var service = new ScenarioComparisonService(
            baselineService,
            new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(validationData)),
            source);
        var template = source.GetTemplates().Single();
        var controlScenario = new ExternalScenarioDefinition(
            "EXT-NO-DISTURBANCE",
            "无扰动对照",
            Metadata: new ScenarioAssumptionMetadata(
                "Manual",
                null,
                null,
                "DDS&OP 计划员",
                "2026-07-15T08:00:00Z",
                "2026-07-16",
                "2026-09-30",
                "用于验证内置模板的后端业务效果",
                "人工录入：无扰动对照"));
        var control = service.Compare(new ScenarioComparisonRequest(
            frozen.SnapshotId,
            controlScenario,
            Array.Empty<ResponseConfiguration>(),
            12));
        var manualMetadata = controlScenario.Metadata! with
        {
            Rationale = $"隔离验证模板 {disturbanceKind} 业务扰动",
            EvidenceLabel = $"人工录入：模板 {disturbanceKind} 隔离测试"
        };
        var isolatedScenario = disturbanceKind switch
        {
            "Demand" => new ExternalScenarioDefinition(
                "EXT-MANUAL-DEMAND-ONLY",
                "仅需求扰动",
                DemandChanges: template.ExternalScenario.DemandChanges,
                Metadata: manualMetadata),
            "Supply" => new ExternalScenarioDefinition(
                "EXT-MANUAL-SUPPLY-ONLY",
                "仅供应扰动",
                SupplyRisks: template.ExternalScenario.SupplyRisks,
                Metadata: manualMetadata),
            "Capacity" => new ExternalScenarioDefinition(
                "EXT-MANUAL-CAPACITY-ONLY",
                "仅能力扰动",
                CapacityLosses: template.ExternalScenario.CapacityLosses,
                Metadata: manualMetadata),
            _ => throw new ArgumentException("unknown disturbance kind", nameof(disturbanceKind))
        };
        var disturbed = service.Compare(new ScenarioComparisonRequest(
            frozen.SnapshotId,
            isolatedScenario,
            Array.Empty<ResponseConfiguration>(),
            12));

        return (template, control, disturbed);
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestScenarioComparisonSeparatesExternalEventsAndResponses()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-scenario-compare-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var baselineService = new CurrentBaselineService(new SeedCurrentBaselineDataSource(validationData), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "场景比较来源基线"));
        var previewService = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(validationData));
        var service = new ScenarioComparisonService(baselineService, previewService, new SeedScenarioAssumptionSource());
        var externalScenario = new ExternalScenarioDefinition(
            "EXT-SUPPLY-CAPACITY",
            "需求上升与供应能力风险",
            new[] { new ExternalDemandChange(null, "精益液压件", 2, 6, 1.35m, "客户需求上升") },
            new[] { new ExternalSupplyRisk("华东铸件", "铸件", 3, 8, 0.55m, "供应商产出风险") },
            new[] { new ExternalCapacityLoss("R-MIX", 3, 6, 0.65m, "设备能力损失") },
            new[] { new ExternalKnownEvent("EVENT-001", "客户促销窗口", 2, 6) },
            Metadata: new ScenarioAssumptionMetadata(
                "Manual",
                null,
                null,
                "DDS&OP 计划员",
                "2026-07-15T08:00:00Z",
                "2026-07-16",
                "2026-09-30",
                "客户与供应商会议确认",
                "人工录入：DDS&OP 场景会议"));
        var responses = new[]
        {
            new ResponseConfiguration(
                "RESP-CAPACITY",
                "临时能力",
                new ScenarioRunParameterSet(
                    CapacityAdjustments: Enumerable.Range(3, 4)
                        .Select(week => new ResourceCapacityAdjustment("R-MIX", week, 1.35m, "临时能力响应"))
                        .ToList())),
            new ResponseConfiguration(
                "RESP-POLICY",
                "MOQ 与订货周期覆盖",
                new ScenarioRunParameterSet(
                    SkuPolicyOverrides: new[] { new SkuPolicyOverride("HYD-100", 80m, 7) }))
        };

        var result = service.Compare(new ScenarioComparisonRequest(frozen.SnapshotId, externalScenario, responses, 12));

        AssertEqual(frozen.SnapshotId, result.BaselineSnapshotId, "comparison baseline identity");
        AssertEqual("NO_RESPONSE", result.NoResponse.ResponseId, "comparison should include no-response case");
        AssertEqual(2, result.ResponseCases.Count, "comparison response case count");
        AssertTrue(result.AllCases.All(item => item.ExternalScenarioId == externalScenario.ScenarioId), "all cases should share the external scenario");
        AssertTrue(result.NoResponse.Preview.Request.Parameters is null, "no-response case must not carry enterprise responses");
        AssertTrue(result.ResponseCases.All(item => item.Preview.Request.Parameters is not null), "response cases should carry response configuration only");
        AssertTrue(result.AllCases.All(item => item.Preview.Request.ExternalScenario?.Metadata?.SourceKind == "Manual"), "all cases should retain the validated manual source metadata");
        AssertTrue(
            result.AllCases
                .SelectMany(item => item.Breaches)
                .Select(item => item.ScopeType)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .SequenceEqual(new[] { "CapacityBuffer", "InventoryBuffer", "SupplyRisk", "TimeBuffer" }),
            "inventory time capacity and supply analyses should expose the four fixed scopes");

        var missingMetadataRejected = false;
        try
        {
            service.Compare(new ScenarioComparisonRequest(
                frozen.SnapshotId,
                externalScenario with { Metadata = null },
                responses,
                12));
        }
        catch (ArgumentException ex)
        {
            missingMetadataRejected = ex.Message.Contains("场景来源缺失；仅允许人工录入或演示模板", StringComparison.Ordinal);
        }
        AssertTrue(missingMetadataRejected, "comparison should reject a scenario whose source metadata is missing");

        foreach (var invalidResponses in new[]
        {
            new[] { responses[0], responses[0] with { Name = "重复标识" } },
            new[] { responses[0] with { ResponseId = "NO_RESPONSE" } }
        })
        {
            var rejected = false;
            try
            {
                service.Compare(new ScenarioComparisonRequest(frozen.SnapshotId, externalScenario, invalidResponses, 12));
            }
            catch (ArgumentException)
            {
                rejected = true;
            }
            AssertTrue(rejected, "response IDs must be unique and must not use the reserved NO_RESPONSE ID");
        }
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestScenarioSupplyResponseRestoresCommittedSupply()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-supply-response-{Guid.NewGuid():N}.db");
    try
    {
        var data = SeedData.Create();
        var workspaceData = new SeedScenarioWorkspaceDataSource(data)
            .Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1)));
        var baselineService = new CurrentBaselineService(new SeedCurrentBaselineDataSource(data), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "供应响应来源基线"));
        var assumptionSource = new SeedScenarioAssumptionSource();
        var externalScenario = assumptionSource.GetTemplates().Single().ExternalScenario;
        var risks = externalScenario.SupplyRisks ?? Array.Empty<ExternalSupplyRisk>();
        var service = new ScenarioComparisonService(
            baselineService,
            new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(data)),
            assumptionSource);
        var restoredCommitments = workspaceData.SupplierCapacityWindows
            .Where(item => risks.Any(risk =>
                risk.Supplier == item.Supplier &&
                risk.MaterialFamily == item.MaterialFamily &&
                item.Week >= risk.StartWeek &&
                item.Week <= risk.EndWeek))
            .Select(item => new SupplierCapacityLimit(item.Supplier, item.MaterialFamily, item.Week, item.Week, item.CommittedCapacity))
            .ToList();
        var result = service.Compare(new ScenarioComparisonRequest(
            frozen.SnapshotId,
            externalScenario,
            new[]
            {
                new ResponseConfiguration(
                    "RESP-SUPPLY-RECOVERY",
                    "供应响应",
                    new ScenarioRunParameterSet(SupplierCapacityLimits: restoredCommitments))
            },
            12));

        var response = result.ResponseCases.Single();
        AssertTrue(response.Preview.Request.Parameters?.SupplierCapacityLimits?.Count > 0, "supply response should remain an explicit internal response configuration");
        AssertTrue(response.Preview.Scenario.Metrics.SupplyGap < result.NoResponse.Preview.Scenario.Metrics.SupplyGap, "restored committed supply should reduce the externally disturbed supply gap");
        AssertTrue(response.Preview.Trace.Any(item => item.Message.Contains("供应响应", StringComparison.Ordinal)), "backend white-box trace should retain the supply-response evidence");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestScenarioComparisonUsesFrozenSnapshotValues()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-scenario-compare-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var targetDefinition = validationData.Skus.First();
        var addedDefinition = targetDefinition with { Sku = "LIVE-ONLY-SKU", Name = "冻结后新增对象", Adu = targetDefinition.Adu * 8m };
        var driftedData = validationData with
        {
            Skus = validationData.Skus
                .Select(item => item.Sku == targetDefinition.Sku ? item with { Adu = item.Adu * 6m, DecoupledLeadTimeDays = item.DecoupledLeadTimeDays + 20 } : item)
                .Append(addedDefinition)
                .ToList(),
            Inventory = validationData.Inventory.Append(new InventoryPosition(addedDefinition.Sku, 50m, 0m, 10m)).ToList(),
            Demand = validationData.Demand
                .Select(item => item.Sku == targetDefinition.Sku ? item with { BaselineDemand = item.BaselineDemand * 4m } : item)
                .Concat(Enumerable.Range(1, 4).Select(week => new WeeklyDemand(addedDefinition.Sku, week, 500m)))
                .ToList(),
            ResourceRoutings = validationData.ResourceRoutings
                .Select(item => item.Sku == targetDefinition.Sku ? item with { CapacityPerUnit = item.CapacityPerUnit * 5m } : item)
                .Append(new ResourceRouting(addedDefinition.Sku, validationData.Resources.First().Code, 4m))
                .ToList(),
            SupplierItemSources = validationData.SupplierItemSources
                .Append(new SupplierItemSource("LIVE-SUPPLIER", addedDefinition.Sku, addedDefinition.Family, addedDefinition.UnitCost))
                .ToList()
        };
        var liveSource = new TrackingScenarioWorkspaceDataSource(driftedData);
        var previewService = new ScenarioRunPreviewService(liveSource);
        var candidate = new SeedCurrentBaselineDataSource(validationData).GetCandidate();
        var target = candidate.Payload.Inventory.First();
        var frozenPosition = target with { OnHand = target.OnHand + 10_000m };
        var frozenCandidate = candidate with
        {
            Payload = candidate.Payload with
            {
                Inventory = candidate.Payload.Inventory.Select(item => item.Sku == target.Sku ? frozenPosition : item).ToList()
            }
        };
        var baselineService = new CurrentBaselineService(new FixedCurrentBaselineDataSource(frozenCandidate), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "冻结库存来源验证"));
        var comparison = new ScenarioComparisonService(baselineService, previewService, new SeedScenarioAssumptionSource()).Compare(new ScenarioComparisonRequest(
            frozen.SnapshotId,
            new ExternalScenarioDefinition(
                "EXT-FROZEN-SOURCE",
                "无额外扰动",
                Metadata: new ScenarioAssumptionMetadata(
                    "Manual",
                    null,
                    null,
                    "DDS&OP 计划员",
                    "2026-07-15T08:00:00Z",
                    "2026-07-16",
                    "2026-09-30",
                    "冻结库存来源验证",
                    "人工录入：冻结来源测试")),
            Array.Empty<ResponseConfiguration>(),
            4));
        AssertEqual(0, liveSource.LoadCount, "frozen comparison must not call the live workspace data source");
        var livePreview = previewService.Preview(new ScenarioRunPreviewRequest(4));
        var expectedStartNetFlow = DdmrpCalculator.CalculateNetFlow(frozenPosition);
        var frozenStartNetFlow = comparison.NoResponse.Preview.Baseline.Plan.BufferProjections
            .Single(item => item.Sku == target.Sku && item.Week == 1)
            .StartNetFlow;
        var liveStartNetFlow = livePreview.Baseline.Plan.BufferProjections
            .Single(item => item.Sku == target.Sku && item.Week == 1)
            .StartNetFlow;
        var frozenDetail = comparison.NoResponse.Preview.Scenario.BufferTrend.SkuDetails.Single(item => item.Sku == target.Sku);
        var liveDetail = livePreview.Scenario.BufferTrend.SkuDetails.Single(item => item.Sku == target.Sku);
        var frozenDemand = comparison.NoResponse.Preview.Scenario.Plan.BufferProjections.Single(item => item.Sku == target.Sku && item.Week == 1).Demand;
        var liveDemand = livePreview.Scenario.Plan.BufferProjections.Single(item => item.Sku == target.Sku && item.Week == 1).Demand;
        var expectedDemand = candidate.Payload.PlanningInputs!.Demand.Single(item => item.Sku == target.Sku && item.Week == 1).BaselineDemand;

        AssertEqual(decimal.Round(expectedStartNetFlow, 0), frozenStartNetFlow, "comparison frozen inventory start net flow");
        AssertTrue(frozenStartNetFlow != liveStartNetFlow, "comparison must not silently reload live inventory after baseline freeze");
        AssertEqual(targetDefinition.Adu, frozenDetail.Adu, "comparison should use frozen DDMRP parameters");
        AssertTrue(frozenDetail.Adu != liveDetail.Adu, "live DDMRP drift must not change a frozen comparison");
        AssertEqual(expectedDemand, frozenDemand, "comparison should use frozen demand inputs");
        AssertTrue(frozenDemand != liveDemand, "live demand drift must not change a frozen comparison");
        AssertTrue(comparison.NoResponse.Preview.Scenario.Plan.BufferProjections.All(item => item.Sku != addedDefinition.Sku), "new live SKU must not enter an older frozen snapshot");
        AssertTrue(livePreview.Scenario.Plan.BufferProjections.Any(item => item.Sku == addedDefinition.Sku), "control preview should expose the live-only SKU");
        AssertTrue(comparison.NoResponse.Preview.Trace.Any(item => item.Stage == "FrozenBaseline"), "comparison trace should identify frozen baseline recalculation");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestFrozenComparisonSavePersistsBaselineScenarioAndResponseLineage()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-frozen-scenario-runs-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var baselineService = new CurrentBaselineService(new SeedCurrentBaselineDataSource(validationData), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "冻结场景保存来源基线"));
        var liveSource = new TrackingScenarioWorkspaceDataSource(validationData);
        var previewService = new ScenarioRunPreviewService(liveSource);
        var comparisonService = new ScenarioComparisonService(
            baselineService,
            previewService,
            new SeedScenarioAssumptionSource());
        var persistence = new ScenarioRunPersistenceService(previewService, comparisonService, databasePath);
        var comparisonRequest = new ScenarioComparisonRequest(
            frozen.SnapshotId,
            new ExternalScenarioDefinition(
                "EXT-FROZEN-LINEAGE",
                "冻结场景血缘验证",
                DemandChanges: new[]
                {
                    new ExternalDemandChange(null, "精益液压件", 2, 6, 1.25m, "客户需求上升")
                },
                SupplyRisks: new[]
                {
                    new ExternalSupplyRisk("华东铸件", "铸件", 3, 8, 0.70m, "供应商产出风险")
                },
                Metadata: new ScenarioAssumptionMetadata(
                    "Manual",
                    null,
                    null,
                    "DDS&OP 计划员",
                    "2026-07-15T08:00:00Z",
                    "2026-07-16",
                    "2026-09-30",
                    "客户与供应商会议确认",
                    "人工录入：冻结场景保存测试")),
            new[]
            {
                new ResponseConfiguration(
                    "RESP-CAPACITY",
                    "临时能力",
                    new ScenarioRunParameterSet(
                        CapacityAdjustments: Enumerable.Range(3, 4)
                            .Select(week => new ResourceCapacityAdjustment("R-MIX", week, 1.25m, "临时能力响应"))
                            .ToList())),
                new ResponseConfiguration(
                    "RESP-POLICY",
                    "策略覆盖",
                    new ScenarioRunParameterSet(
                        SkuPolicyOverrides: new[] { new SkuPolicyOverride("HYD-100", 80m, 7) }))
            },
            12);
        var frozenComparison = comparisonService.Compare(comparisonRequest);
        var responseIds = new[] { "NO_RESPONSE", "RESP-CAPACITY", "RESP-POLICY" };

        var saved = responseIds
            .Select(responseId => persistence.SaveFrozenComparison(new ScenarioComparisonSaveRequest(
                comparisonRequest,
                responseId,
                $"冻结场景 {responseId}",
                "保存冻结比较结果与真实血缘",
                "DDS&OP 计划员")))
            .ToList();

        AssertEqual(0, liveSource.LoadCount, "frozen comparison save must not reload the live scenario workspace source");
        AssertEqual(saved.Count, saved.Select(item => item.RunId).Distinct(StringComparer.Ordinal).Count(), "each frozen comparison save should return a distinct real run ID");
        AssertTrue(saved.All(item => Guid.TryParseExact(item.RunId, "N", out _)), "frozen comparison save should return GUID run IDs");
        AssertTrue(saved.All(item => item.Status == "Saved" && item.ApprovalStatus == "NotSubmitted"), "frozen saves must not change governance status");

        IScenarioRunLineageReader lineageReader = persistence;
        var summaries = saved.Select(item => lineageReader.GetSummary(item.RunId)!).ToList();
        AssertTrue(summaries.All(item => item is not null), "saved frozen runs should be available through the lineage reader");
        AssertTrue(summaries.All(item => item.BaselineSnapshotId == frozen.SnapshotId), "saved cases should share the frozen baseline ID");
        AssertTrue(summaries.All(item => item.ExternalScenarioId == comparisonRequest.ExternalScenario.ScenarioId), "saved cases should share the external scenario ID");
        AssertTrue(summaries.Select(item => item.ResponseId).SequenceEqual(responseIds), "saved cases should retain the selected response IDs");

        foreach (var summary in summaries)
        {
            var expectedCase = frozenComparison.AllCases.Single(item => item.ResponseId == summary.ResponseId);
            var detail = persistence.GetDetail(summary.RunId);
            AssertTrue(detail is not null, "saved frozen preview should be readable");
            AssertTrue(detail!.Result.IsPersisted, "saved frozen preview should be marked persisted");
            AssertEqual(expectedCase.Preview.Scenario.Metrics.ServiceLevelPercent, detail.Result.Scenario.Metrics.ServiceLevelPercent, "saved frozen preview service level");
            AssertEqual(expectedCase.Preview.Scenario.Metrics.AverageInventoryValue, detail.Result.Scenario.Metrics.AverageInventoryValue, "saved frozen preview inventory value");
            AssertEqual(expectedCase.Preview.Request.Parameters is null, detail.Request.Parameters is null, "saved frozen preview should retain only the selected response configuration");
            var expectedAnalysis = expectedCase.Preview.ProtectionAnalysis;
            var savedAnalysis = detail.Result.ProtectionAnalysis;
            AssertTrue(expectedAnalysis is not null && savedAnalysis is not null, "saved frozen preview should retain backend protection analysis");
            AssertEqual(expectedAnalysis!.Breaches.Count, savedAnalysis!.Breaches.Count, "saved frozen protection breach count");
            AssertTrue(
                expectedAnalysis.Breaches.Zip(savedAnalysis.Breaches).All(pair =>
                    pair.First.ScopeType == pair.Second.ScopeType &&
                    pair.First.Target == pair.Second.Target &&
                    pair.First.IsBreached == pair.Second.IsBreached &&
                    pair.First.EarliestRedWeek == pair.Second.EarliestRedWeek &&
                    pair.First.ConsecutiveRiskWeeks == pair.Second.ConsecutiveRiskWeeks &&
                    pair.First.RecoveryWeek == pair.Second.RecoveryWeek &&
                    pair.First.IsUnrecovered == pair.Second.IsUnrecovered &&
                    pair.First.AffectedProducts.SequenceEqual(pair.Second.AffectedProducts) &&
                    pair.First.PrimaryCause == pair.Second.PrimaryCause &&
                    pair.First.BufferSize == pair.Second.BufferSize &&
                    pair.First.MaximumPenetrationPercent == pair.Second.MaximumPenetrationPercent &&
                    pair.First.Unit == pair.Second.Unit &&
                    pair.First.EvidenceStatus == pair.Second.EvidenceStatus),
                "saved frozen protection breaches field equality");
            AssertTrue(expectedAnalysis.TimeBufferProjection.SequenceEqual(savedAnalysis.TimeBufferProjection), "saved frozen time-buffer projection object equality");
            AssertTrue(expectedAnalysis.CapacityProtectionProjection.SequenceEqual(savedAnalysis.CapacityProtectionProjection), "saved frozen capacity-protection projection object equality");
            AssertEqual(
                JsonSerializer.Serialize(expectedAnalysis.Breaches),
                JsonSerializer.Serialize(savedAnalysis.Breaches),
                "saved frozen protection breaches JSON equality");
            AssertEqual(
                JsonSerializer.Serialize(expectedAnalysis.TimeBufferProjection),
                JsonSerializer.Serialize(savedAnalysis.TimeBufferProjection),
                "saved frozen time-buffer projection JSON equality");
            AssertEqual(
                JsonSerializer.Serialize(expectedAnalysis.CapacityProtectionProjection),
                JsonSerializer.Serialize(savedAnalysis.CapacityProtectionProjection),
                "saved frozen capacity-protection projection JSON equality");
        }

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info(scenario_runs);";
        using var reader = command.ExecuteReader();
        var columns = new HashSet<string>(StringComparer.Ordinal);
        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }
        AssertTrue(
            new[] { "baseline_snapshot_id", "external_scenario_id", "response_id" }.All(columns.Contains),
            "scenario_runs should add frozen lineage columns");

        var unknownResponseRejected = false;
        try
        {
            persistence.SaveFrozenComparison(new ScenarioComparisonSaveRequest(
                comparisonRequest,
                "RESP-NOT-DEFINED",
                "无效响应",
                null,
                "DDS&OP 计划员"));
        }
        catch (ArgumentException)
        {
            unknownResponseRejected = true;
        }
        AssertTrue(unknownResponseRejected, "frozen comparison save should select exactly one existing response case");
        AssertEqual(3, persistence.List(50).Count, "an invalid response selection must not create a run");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestScenarioAssumptionAndFrozenSaveApisRemainInternalOnly()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var program = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Program.cs"));

    AssertTrue(
        program.Contains("app.MapGet(\"/api/scenario-assumptions/templates\"", StringComparison.Ordinal),
        "internal demo assumption templates endpoint should be exposed");
    AssertTrue(
        program.Contains("app.MapPost(\"/api/scenario-runs/compare/save\"", StringComparison.Ordinal),
        "frozen comparison save endpoint should be exposed");
    AssertTrue(
        program.Contains("AddSingleton<IScenarioRunLineageReader>", StringComparison.Ordinal),
        "the read-only lineage interface should resolve to the scenario persistence singleton");
    AssertTrue(
        !program.Contains("/api/scenario-assumptions/import", StringComparison.Ordinal)
        && !program.Contains("/api/scenario-assumptions/network", StringComparison.Ordinal)
        && !program.Contains("/api/scenario-assumptions/protocol", StringComparison.Ordinal),
        "scenario assumptions must not expose import network or protocol endpoints");
}

static void TestProtectionBreachAnalysisReportsRecovery()
{
    const int horizonWeeks = 4;
    var frozenData = LoadTask5ProtectionData(horizonWeeks);
    var preview = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()))
        .Preview(new ScenarioRunPreviewRequest(horizonWeeks));
    var sku = preview.Scenario.BufferTrend.WeeklyCells.First().Sku;
    var supplier = preview.Scenario.Constraints.SupplyCells.First().Supplier;
    var materialFamily = preview.Scenario.Constraints.SupplyCells.First().MaterialFamily;
    var buffer = preview.Scenario.BufferTrend with
    {
        WeeklyCells = preview.Scenario.BufferTrend.WeeklyCells
            .Select(item => item.Sku == sku ? item with { Status = item.Week is 2 or 3 ? "Red" : "Green" } : item with { Status = "Green" })
            .ToList()
    };
    var constraints = preview.Scenario.Constraints with
    {
        CapacityCells = preview.Scenario.Constraints.CapacityCells
            .Select(item => item.ResourceCode == "RES-AIT"
                ? item with
                {
                    UnconstrainedRequired = item.Week switch
                    {
                        1 => 70m,
                        2 => 90m,
                        3 => 110m,
                        _ => 70m
                    },
                    ConstrainedAvailable = 100m,
                    Status = "Green"
                }
                : item)
            .ToList(),
        SupplyCells = preview.Scenario.Constraints.SupplyCells
            .Select(item => item.Supplier == supplier && item.MaterialFamily == materialFamily ? item with { Status = item.Week == 1 ? "Red" : "Green" } : item with { Status = "Green" })
            .ToList()
    };
    var controlled = preview with { Scenario = preview.Scenario with { BufferTrend = buffer, Constraints = constraints } };

    var inventory = new InventoryProtectionAnalyzer().Analyze(controlled.Scenario)
        .Single(item => item.ScopeType == "InventoryBuffer" && item.Target == sku);
    var supply = new SupplyRiskAnalyzer().Analyze(controlled.Scenario)
        .Single(item => item.ScopeType == "SupplyRisk" && item.Target == $"{supplier}/{materialFamily}");
    var capacityAnalysis = new CapacityBufferProtectionAnalyzer().Analyze(frozenData, controlled.Scenario, horizonWeeks);
    var capacity = capacityAnalysis.Breaches.Single(item => item.ScopeType == "CapacityBuffer" && item.Target == "RES-AIT");
    var capacityWeek2 = capacityAnalysis.Projection.Single(item => item.Week == 2);
    var capacityWeek3 = capacityAnalysis.Projection.Single(item => item.Week == 3);

    AssertEqual(2, inventory.EarliestRedWeek, "inventory first red week");
    AssertEqual(2, inventory.ConsecutiveRiskWeeks, "inventory consecutive risk duration");
    AssertEqual(4, inventory.RecoveryWeek, "inventory recovery week");
    AssertEqual(2, supply.RecoveryWeek, "supply recovery week");
    AssertEqual<decimal?>(20m, capacityWeek2.ProtectionCapacity, "AIT capacity protection size");
    AssertEqual<decimal?>(10m, capacityWeek2.ConsumedProtection, "AIT partial protection consumption");
    AssertEqual<decimal?>(10m, capacityWeek2.RemainingProtection, "AIT remaining protection after partial consumption");
    AssertEqual("Yellow", capacityWeek2.Status, "AIT partial protection status");
    AssertEqual<decimal?>(20m, capacityWeek3.ConsumedProtection, "AIT full protection consumption");
    AssertEqual<decimal?>(0m, capacityWeek3.RemainingProtection, "AIT exhausted protection");
    AssertEqual("Red", capacityWeek3.Status, "AIT exhausted protection status");
    AssertEqual(3, capacity.EarliestRedWeek, "capacity-buffer first exhausted week");
    AssertEqual(4, capacity.RecoveryWeek, "capacity-buffer recovery week");
    AssertTrue(!capacity.IsUnrecovered, "capacity protection should recover when upstream load falls below the protection band");
    AssertTrue(
        capacityAnalysis.Projection.All(item =>
            item.UpstreamResourceCode == "RES-AIT" &&
            item.ProtectedCcrResourceCode == "RES-HARNESS"),
        "capacity buffer must contain only the explicit sequenced AIT to HARNESS protection");
    AssertTrue(
        !capacity.AffectedProducts.Contains("AV-FPGA-203", StringComparer.Ordinal),
        "FPGA must not enter AIT to HARNESS capacity protection scope");

    var repeatedBuffer = preview.Scenario.BufferTrend with
    {
        WeeklyCells = preview.Scenario.BufferTrend.WeeklyCells
            .Select(item => item.Sku == sku ? item with { Status = item.Week is 1 or 3 or 4 ? "Red" : "Green" } : item with { Status = "Green" })
            .ToList()
    };
    var repeated = new InventoryProtectionAnalyzer().Analyze(preview.Scenario with { BufferTrend = repeatedBuffer })
        .Single(item => item.ScopeType == "InventoryBuffer" && item.Target == sku);
    AssertEqual(1, repeated.EarliestRedWeek, "repeated breach should preserve the earliest red week");
    AssertEqual(2, repeated.ConsecutiveRiskWeeks, "repeated breach should report the maximum red streak");
    AssertTrue(repeated.IsUnrecovered && repeated.RecoveryWeek is null, "a final red episode must remain unrecovered even after an earlier recovery");

    var unrecoveredConstraints = constraints with
    {
        CapacityCells = constraints.CapacityCells
            .Select(item => item.ResourceCode == "RES-AIT" && item.Week == 4
                ? item with { UnconstrainedRequired = 110m }
                : item)
            .ToList()
    };
    var unrecoveredCapacity = new CapacityBufferProtectionAnalyzer().Analyze(
            frozenData,
            preview.Scenario with { Constraints = unrecoveredConstraints },
            horizonWeeks)
        .Breaches.Single(item => item.Target == "RES-AIT");
    AssertEqual(2, unrecoveredCapacity.ConsecutiveRiskWeeks, "capacity-buffer final red streak");
    AssertTrue(
        unrecoveredCapacity.IsUnrecovered && unrecoveredCapacity.RecoveryWeek is null,
        "capacity-buffer breach through the horizon should remain unrecovered");
}

static void TestProtectionAnalysisSeparatesInventoryTimeCapacityAndSupply()
{
    var breaches = CreateScenarioDemoTemplateEffectComparison("Supply").Disturbed.NoResponse.Breaches;
    var scopes = breaches
        .Select(item => item.ScopeType)
        .Distinct(StringComparer.Ordinal)
        .OrderBy(item => item, StringComparer.Ordinal)
        .ToList();

    AssertEqual(
        "CapacityBuffer|InventoryBuffer|SupplyRisk|TimeBuffer",
        string.Join('|', scopes),
        "fixed protection-analysis scopes");
}

static void TestTimeBufferBreachReportsPenetrationRecoveryAndUnrecoveredHorizon()
{
    const int horizonWeeks = 4;
    var frozenData = LoadTask5ProtectionData(horizonWeeks);
    var definition = frozenData.TimeBuffers!.Single();
    frozenData = frozenData with
    {
        ControlPointProgress = new[]
        {
            new ControlPointProgressFact(definition.BufferId, 1, 1m, "基线准备延迟", "Complete"),
            new ControlPointProgressFact(definition.BufferId, 2, 1m, "基线准备延迟", "Complete"),
            new ControlPointProgressFact(definition.BufferId, 3, 0m, "按计划", "Complete"),
            new ControlPointProgressFact(definition.BufferId, 4, 0m, "按计划", "Complete")
        }
    };
    var recoveredScenario = new ExternalScenarioDefinition(
        "EXT-TIME-RECOVERED",
        "时间缓冲恢复场景",
        TimeDelays: new[] { new ExternalTimeDelay(definition.BufferId, 1, 2, 3m, "试验件到达延迟") });
    var response = new ScenarioRunParameterSet(
        TimeBufferAdjustments: new[] { new TimeBufferResponseAdjustment(definition.BufferId, 2, 2, 4m, "增加准备班次") });
    var analyzer = new TimeBufferProtectionAnalyzer();

    var recovered = analyzer.Analyze(frozenData, recoveredScenario, response, horizonWeeks);
    var recoveredBreach = recovered.Breaches.Single(item => item.Target == definition.BufferId);
    var firstWeek = recovered.Projection.Single(item => item.BufferId == definition.BufferId && item.Week == 1);
    var recoveryWeek = recovered.Projection.Single(item => item.BufferId == definition.BufferId && item.Week == 2);

    AssertEqual<decimal?>(4m, firstWeek.DelayDays, "time-buffer net delay in first red week");
    AssertEqual<decimal?>(133.3m, firstWeek.PenetrationPercent, "time-buffer maximum penetration percent");
    AssertEqual("Red", firstWeek.Status, "time-buffer first-week status");
    AssertEqual<decimal?>(0m, recoveryWeek.DelayDays, "time-buffer response recovery delay");
    AssertEqual("Green", recoveryWeek.Status, "time-buffer recovered status");
    AssertEqual(1, recoveredBreach.EarliestRedWeek, "time-buffer earliest red week");
    AssertEqual(1, recoveredBreach.ConsecutiveRiskWeeks, "time-buffer maximum red streak");
    AssertEqual(2, recoveredBreach.RecoveryWeek, "time-buffer recovery week");
    AssertTrue(!recoveredBreach.IsUnrecovered, "recovered time buffer should not remain open through the horizon");
    AssertEqual<decimal?>(definition.BufferDays, recoveredBreach.BufferSize, "time-buffer size");
    AssertEqual<decimal?>(133.3m, recoveredBreach.MaximumPenetrationPercent, "time-buffer maximum breach penetration");

    var previewWithResponse = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()))
        .Preview(new ScenarioRunPreviewRequest(horizonWeeks, Parameters: response));
    AssertEqual(
        response.TimeBufferAdjustments!.Single(),
        previewWithResponse.Request.Parameters!.TimeBufferAdjustments!.Single(),
        "scenario preview should retain the time-buffer response adjustment for frozen comparison analysis");

    var unrecoveredScenario = new ExternalScenarioDefinition(
        "EXT-TIME-UNRECOVERED",
        "时间缓冲未恢复场景",
        TimeDelays: new[] { new ExternalTimeDelay(definition.BufferId, 3, 4, 4m, "展望期末持续延迟") });
    var unrecovered = analyzer.Analyze(frozenData, unrecoveredScenario, null, horizonWeeks)
        .Breaches.Single(item => item.Target == definition.BufferId);

    AssertEqual(3, unrecovered.EarliestRedWeek, "unrecovered time-buffer earliest red week");
    AssertEqual(2, unrecovered.ConsecutiveRiskWeeks, "unrecovered time-buffer red streak");
    AssertTrue(unrecovered.IsUnrecovered && unrecovered.RecoveryWeek is null, "horizon-ending time-buffer breach should remain unrecovered");
}

static void TestTimeBufferMissingEvidenceIsNotReportedAsZero()
{
    const int horizonWeeks = 2;
    var frozenData = LoadTask5ProtectionData(horizonWeeks);
    var definition = frozenData.TimeBuffers!.Single();
    frozenData = frozenData with
    {
        ControlPointProgress = new[]
        {
            new ControlPointProgressFact(definition.BufferId, 1, null, "来源证据缺失", "EvidenceMissing")
        }
    };
    var analyzer = new TimeBufferProtectionAnalyzer();
    var analysis = analyzer.Analyze(
        frozenData,
        new ExternalScenarioDefinition("EXT-TIME-MISSING", "时间证据缺失"),
        null,
        horizonWeeks);
    var breach = analysis.Breaches.Single(item => item.Target == definition.BufferId);

    AssertEqual(horizonWeeks, analysis.Projection.Count, "missing-evidence time-buffer projection length");
    AssertTrue(
        analysis.Projection.All(item =>
            item.EvidenceStatus == "EvidenceMissing" &&
            item.Status == "EvidenceMissing" &&
            item.DelayDays is null &&
            item.PenetrationPercent is null),
        "missing progress evidence must remain null rather than becoming zero");
    AssertEqual("EvidenceMissing", breach.EvidenceStatus, "missing time-buffer evidence status");
    AssertTrue(breach.MaximumPenetrationPercent is null, "missing time-buffer penetration must remain null");

    var notApplicable = analyzer.Analyze(
        frozenData with { TimeBuffers = Array.Empty<TimeBufferDefinition>() },
        new ExternalScenarioDefinition("EXT-TIME-NA", "无时间缓冲定义"),
        null,
        horizonWeeks);
    var notApplicableBreach = notApplicable.Breaches.Single();
    AssertEqual("NotApplicable", notApplicableBreach.EvidenceStatus, "missing time-buffer definition applicability");
    AssertTrue(notApplicable.Projection.Count == 0 && notApplicableBreach.BufferSize is null, "not-applicable time buffer must not fabricate numeric projection");
}

static void TestTimeBufferRequiresExplicitProductScopeEvidence()
{
    const int horizonWeeks = 2;
    var frozenData = LoadTask5ProtectionData(horizonWeeks);
    var definition = frozenData.TimeBuffers!.Single();
    frozenData = frozenData with
    {
        ControlPointProgress = new[]
        {
            new ControlPointProgressFact(definition.BufferId, 1, 0m, "按计划", "Complete"),
            new ControlPointProgressFact(definition.BufferId, 2, 0m, "按计划", "Complete")
        },
        TimeBufferProductScopes = Array.Empty<TimeBufferProductScope>()
    };

    var analysis = new TimeBufferProtectionAnalyzer().Analyze(
        frozenData,
        new ExternalScenarioDefinition("EXT-TIME-NO-SCOPE", "产品范围证据缺失"),
        null,
        horizonWeeks);
    var breach = analysis.Breaches.Single(item => item.Target == definition.BufferId);

    AssertEqual("EvidenceMissing", breach.EvidenceStatus, "missing explicit time-buffer product scope status");
    AssertTrue(breach.AffectedProducts.Count == 0, "missing product scope must not infer products from names or routings");
    AssertEqual(horizonWeeks, analysis.Projection.Count, "missing-scope time-buffer projection length");
    AssertTrue(
        analysis.Projection.All(item => item.DelayDays is null && item.PenetrationPercent is null && item.EvidenceStatus == "EvidenceMissing"),
        "missing product scope must suppress calculated penetration instead of reporting zero");
}

static void TestTimeBufferExcludesInjectedFpgaScopeEvidence()
{
    const int horizonWeeks = 2;
    var frozenData = LoadTask5ProtectionData(horizonWeeks);
    var definition = frozenData.TimeBuffers!.Single();
    var validSku = frozenData.TimeBufferProductScopes!.Single().Skus.First();
    var analyzer = new TimeBufferProtectionAnalyzer();
    var scenario = new ExternalScenarioDefinition("EXT-TIME-FPGA-INJECTION", "时间范围 FPGA 注入");

    var mixed = analyzer.Analyze(
        frozenData with
        {
            TimeBufferProductScopes = new[]
            {
                new TimeBufferProductScope(
                    definition.BufferId,
                    Array.Empty<string>(),
                    new[] { validSku, "AV-FPGA-203" },
                    "Complete")
            }
        },
        scenario,
        null,
        horizonWeeks);
    var mixedBreach = mixed.Breaches.Single(item => item.Target == definition.BufferId);
    AssertEqual("Complete", mixedBreach.EvidenceStatus, "mixed explicit time-buffer scope evidence");
    AssertTrue(
        mixedBreach.AffectedProducts.SequenceEqual(new[] { validSku }),
        "time-buffer affected products must discard injected FPGA while retaining explicit valid products");

    var fpgaOnly = analyzer.Analyze(
        frozenData with
        {
            TimeBufferProductScopes = new[]
            {
                new TimeBufferProductScope(
                    definition.BufferId,
                    Array.Empty<string>(),
                    new[] { "AV-FPGA-203" },
                    "Complete")
            }
        },
        scenario,
        null,
        horizonWeeks);
    var fpgaOnlyBreach = fpgaOnly.Breaches.Single(item => item.Target == definition.BufferId);
    AssertEqual("EvidenceMissing", fpgaOnlyBreach.EvidenceStatus, "FPGA-only time-buffer scope evidence");
    AssertTrue(fpgaOnlyBreach.AffectedProducts.Count == 0, "FPGA-only time-buffer scope must have no affected products");
    AssertTrue(
        fpgaOnly.Projection.All(item => item.DelayDays is null && item.PenetrationPercent is null),
        "FPGA-only time-buffer scope must not calculate numeric projection values");
}

static void TestCapacityBufferExcludesInjectedFpgaRoutingEvidence()
{
    const int horizonWeeks = 2;
    var frozenData = LoadTask5ProtectionData(horizonWeeks);
    var preview = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()))
        .Preview(new ScenarioRunPreviewRequest(horizonWeeks));
    var fpgaRoutes = new[]
    {
        new ResourceRouting("AV-FPGA-203", "RES-AIT", 1m, 1, "RES-HARNESS", "Complete"),
        new ResourceRouting("AV-FPGA-203", "RES-HARNESS", 1m, 2, null, "Complete")
    };
    var analyzer = new CapacityBufferProtectionAnalyzer();

    var mixed = analyzer.Analyze(
        frozenData with { ResourceRoutings = frozenData.ResourceRoutings.Concat(fpgaRoutes).ToList() },
        preview.Scenario,
        horizonWeeks);
    var mixedBreach = mixed.Breaches.Single(item => item.Target == "RES-AIT");
    AssertEqual("Complete", mixedBreach.EvidenceStatus, "mixed capacity-routing evidence");
    AssertTrue(
        mixedBreach.AffectedProducts.Count > 0 &&
        !mixedBreach.AffectedProducts.Contains("AV-FPGA-203", StringComparer.Ordinal),
        "capacity protection must discard injected FPGA while retaining valid AIT to HARNESS products");

    var fpgaOnly = analyzer.Analyze(
        frozenData with { ResourceRoutings = fpgaRoutes },
        preview.Scenario,
        horizonWeeks);
    var fpgaOnlyBreach = fpgaOnly.Breaches.Single(item => item.Target == "RES-AIT");
    AssertEqual("EvidenceMissing", fpgaOnlyBreach.EvidenceStatus, "FPGA-only capacity-routing evidence");
    AssertTrue(fpgaOnlyBreach.AffectedProducts.Count == 0, "FPGA-only capacity routing must have no affected products");
    AssertTrue(
        fpgaOnly.Projection.All(item =>
            item.PlannedAvailableCapacity is null &&
            item.CommittedLoad is null &&
            item.ProtectionCapacity is null &&
            item.ConsumedProtection is null &&
            item.RemainingProtection is null),
        "FPGA-only capacity routing must not calculate numeric protection values");
}

static void TestTimeBufferExpandsExplicitFamilyOnlyProductScope()
{
    const int horizonWeeks = 2;
    var frozenData = LoadTask5ProtectionData(horizonWeeks);
    var definition = frozenData.TimeBuffers!.Single();
    const string scopedFamily = "卫星平台";
    var expectedProducts = frozenData.Skus
        .Where(item => item.Family == scopedFamily && item.Sku != "AV-FPGA-203")
        .Select(item => item.Sku)
        .OrderBy(item => item, StringComparer.Ordinal)
        .ToList();
    var analysis = new TimeBufferProtectionAnalyzer().Analyze(
        frozenData with
        {
            TimeBufferProductScopes = new[]
            {
                new TimeBufferProductScope(
                    definition.BufferId,
                    new[] { scopedFamily },
                    Array.Empty<string>(),
                    "Complete")
            }
        },
        new ExternalScenarioDefinition("EXT-TIME-FAMILY", "显式产品族时间范围"),
        null,
        horizonWeeks);
    var breach = analysis.Breaches.Single(item => item.Target == definition.BufferId);

    AssertTrue(expectedProducts.Count > 0, "family-only test should use a frozen family with products");
    AssertEqual("Complete", breach.EvidenceStatus, "family-only time-buffer scope evidence");
    AssertTrue(
        breach.AffectedProducts.SequenceEqual(expectedProducts),
        "family-only time-buffer scope should expand through frozen SKU-family mappings");
}

static void TestTimeBufferRejectsUnknownFamilyScopeEvidence()
{
    const int horizonWeeks = 2;
    var frozenData = LoadTask5ProtectionData(horizonWeeks);
    var definition = frozenData.TimeBuffers!.Single();
    var explicitValidSku = frozenData.TimeBufferProductScopes!.Single().Skus.First();
    var analysis = new TimeBufferProtectionAnalyzer().Analyze(
        frozenData with
        {
            TimeBufferProductScopes = new[]
            {
                new TimeBufferProductScope(
                    definition.BufferId,
                    new[] { "UNKNOWN-FAMILY" },
                    new[] { explicitValidSku },
                    "Complete")
            }
        },
        new ExternalScenarioDefinition("EXT-TIME-UNKNOWN-FAMILY", "未知产品族时间范围"),
        null,
        horizonWeeks);
    var breach = analysis.Breaches.Single(item => item.Target == definition.BufferId);

    AssertEqual("EvidenceMissing", breach.EvidenceStatus, "unknown family time-buffer scope evidence");
    AssertTrue(breach.AffectedProducts.Count == 0, "unknown family evidence must suppress all affected products");
    AssertTrue(
        analysis.Projection.All(item => item.DelayDays is null && item.PenetrationPercent is null),
        "unknown family evidence must suppress calculated time-buffer values");
}

static void TestTimeBufferThresholdsUseRawPenetrationBeforeDisplayRounding()
{
    const int horizonWeeks = 6;
    var frozenData = LoadTask5ProtectionData(horizonWeeks);
    var definition = frozenData.TimeBuffers!.Single() with { BufferDays = 100m };
    var rawPenetrations = new[] { 66.999m, 67m, 67.001m, 99.999m, 100m, 100.001m };
    frozenData = frozenData with
    {
        TimeBuffers = new[] { definition },
        ControlPointProgress = rawPenetrations
            .Select((penetration, index) => new ControlPointProgressFact(
                definition.BufferId,
                index + 1,
                penetration,
                "阈值边界证据",
                "Complete"))
            .ToList()
    };

    var projection = new TimeBufferProtectionAnalyzer().Analyze(
            frozenData,
            new ExternalScenarioDefinition("EXT-TIME-THRESHOLDS", "时间缓冲阈值边界"),
            null,
            horizonWeeks)
        .Projection
        .OrderBy(item => item.Week)
        .ToList();

    AssertEqual(
        "Green|Yellow|Yellow|Yellow|Red|Red",
        string.Join('|', projection.Select(item => item.Status)),
        "raw time-buffer threshold statuses");
    AssertEqual(
        "67.0|67.0|67.0|100.0|100.0|100.0",
        string.Join('|', projection.Select(item => item.PenetrationPercent!.Value.ToString("0.0"))),
        "rounded time-buffer penetration display values");
}

static ScenarioWorkspaceDataSet LoadTask5ProtectionData(int horizonWeeks)
{
    return new SeedScenarioWorkspaceDataSource(SeedData.Create()).Load(
        new ScenarioWorkspaceDataRequest(horizonWeeks, new DateOnly(2026, 6, 1)));
}

static void TestSupplyRiskIsNotClassifiedAsDdomBuffer()
{
    var breaches = CreateScenarioDemoTemplateEffectComparison("Supply").Disturbed.NoResponse.Breaches;
    var supplyTarget = "Microchip Space/进口空间级 FPGA";
    var supplyResults = breaches.Where(item => item.Target == supplyTarget).ToList();

    AssertTrue(supplyResults.Count > 0, "supply disturbance should produce a supplier/material risk result");
    AssertTrue(
        supplyResults.All(item => item.ScopeType == "SupplyRisk"),
        "supplier/material risk must use the SupplyRisk scope instead of a DDOM buffer scope");
}

static void TestPureSupplyRiskDoesNotGenerateBufferGovernanceFromPreview()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-supply-governance-{Guid.NewGuid():N}.db");
    try
    {
        var source = new SeedScenarioWorkspaceDataSource(SeedData.Create());
        var previewService = new ScenarioRunPreviewService(source);
        var request = new ScenarioRunPreviewRequest(
            12,
            ExternalScenario: new ExternalScenarioDefinition(
                "EXT-SUPPLY-ONLY",
                "纯供应风险",
                SupplyRisks: new[]
                {
                    new ExternalSupplyRisk("Microchip Space", "进口空间级 FPGA", 1, 12, 0m, "供应承诺中断")
                }));
        var preview = previewService.Preview(request);
        AssertTrue(preview.Scenario.Metrics.SupplyGap > 0m, "pure supply-risk preview should reproduce a supply gap");

        var proposals = new MasterSettingsGovernanceService(source, previewService, databasePath)
            .ProposeFromPreview(request)
            .Proposals;

        AssertTrue(
            proposals.All(item => !IsSupplyGapDerivedBufferProposal(item)),
            "pure supply risk must not generate a Time/Capacity Buffer governance proposal");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestFrozenPureSupplyRiskDoesNotGenerateBufferGovernance()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-frozen-supply-governance-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var source = new SeedScenarioWorkspaceDataSource(validationData);
        var previewService = new ScenarioRunPreviewService(source);
        var baselineService = new CurrentBaselineService(new SeedCurrentBaselineDataSource(validationData), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "纯供应风险治理边界"));
        var externalScenario = new ExternalScenarioDefinition(
            "EXT-FROZEN-SUPPLY-ONLY",
            "冻结纯供应风险",
            SupplyRisks: new[]
            {
                new ExternalSupplyRisk("Microchip Space", "进口空间级 FPGA", 1, 4, 0m, "供应承诺中断")
            },
            Metadata: new ScenarioAssumptionMetadata(
                "Manual",
                null,
                null,
                "DDS&OP 计划员",
                "2026-07-15T08:00:00Z",
                "2026-07-16",
                "2026-08-31",
                "验证纯供应风险不会派生缓冲治理建议",
                "人工录入：供应风险治理边界"));
        var comparison = new ScenarioComparisonService(
                baselineService,
                previewService,
                new SeedScenarioAssumptionSource())
            .Compare(new ScenarioComparisonRequest(
                frozen.SnapshotId,
                externalScenario,
                Array.Empty<ResponseConfiguration>(),
                4));
        AssertTrue(comparison.NoResponse.Preview.Scenario.Metrics.SupplyGap > 0m, "frozen pure supply-risk comparison should reproduce a supply gap");

        var savedRun = new ScenarioRunSummary(
            "RUN-FROZEN-SUPPLY-ONLY", "SR-20260715-0003", "冻结纯供应风险", null, "DDS&OP 计划员", "Saved", "NotSubmitted",
            "2026-07-15T08:00:00Z", 4, null, null, 98m, 1m, 1_000_000m, 90m, 1m, 0, 0,
            frozen.SnapshotId, comparison.NoResponse.ExternalScenarioId, comparison.NoResponse.ResponseId);
        var savedDetail = new ScenarioRunDetail(
            savedRun,
            comparison.NoResponse.Preview.Request,
            comparison.NoResponse.Preview with { IsPersisted = true });
        var proposals = new MasterSettingsGovernanceService(
                source,
                previewService,
                new FixedScenarioRunLineageReader(savedDetail),
                databasePath)
            .ProposeFromFrozenComparison(
                comparison.NoResponse,
                frozen,
                savedRun.RunId,
                new GovernanceDecisionContext())
            .Proposals;

        AssertTrue(
            proposals.All(item => !IsSupplyGapDerivedBufferProposal(item)),
            "frozen pure supply risk must not generate a Time/Capacity Buffer governance proposal");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static bool IsSupplyGapDerivedBufferProposal(MasterSettingChangeRequest proposal)
{
    return proposal.SettingType is "Time Buffer" or "Capacity Buffer" &&
        (proposal.Target.Contains("供应缺口", StringComparison.Ordinal) ||
         proposal.Rationale.Any(reason => reason.Contains("供应缺口", StringComparison.Ordinal)));
}

static void TestFpgaNeverAppearsInTimeOrCapacityBufferResults()
{
    var breaches = CreateScenarioDemoTemplateEffectComparison("Supply").Disturbed.NoResponse.Breaches;
    var protectedResults = breaches
        .Where(item => item.ScopeType is "TimeBuffer" or "CapacityBuffer")
        .ToList();

    AssertTrue(protectedResults.Count > 0, "time and capacity buffer analyses should both be present");
    AssertTrue(
        protectedResults.All(item =>
            item.Target != "AV-FPGA-203" &&
            !item.Target.Contains("FPGA", StringComparison.OrdinalIgnoreCase) &&
            !item.AffectedProducts.Contains("AV-FPGA-203", StringComparer.Ordinal)),
        "AV-FPGA-203 must remain an independent inventory control point");
}

static void TestCoordinationLedgerEnforcesWorkflowAndAuditsUpdates()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-coordination-{Guid.NewGuid():N}.db");
    try
    {
        var service = new CoordinationLedgerService(databasePath);
        var created = service.Create(new CoordinationItemCreateRequest(
            "铸件供应风险需要跨部门决策",
            new[] { "HYD-100", "华东铸件" },
            "RUN-001",
            "CHANGE-001",
            "服务风险上升",
            "红区周期增加",
            125000m,
            "供应连续性",
            "决定是否启用第二来源",
            "供应链经理",
            "2026-07-20",
            "S&OP 执行层",
            "2026-07-17",
            "DDS&OP 计划员"));

        var inProgress = service.UpdateStatus(created.ItemId, new CoordinationStatusUpdateRequest("InProgress", "供应链经理", "已启动评审"));
        var escalated = service.UpdateStatus(created.ItemId, new CoordinationStatusUpdateRequest("Escalated", "供应链经理", "需要执行委员会裁决"));
        var resumed = service.UpdateStatus(created.ItemId, new CoordinationStatusUpdateRequest("InProgress", "执行委员会", "已明确决策边界"));
        var decided = service.RecordDecision(created.ItemId, new CoordinationDecisionUpdateRequest("启用第二来源并限定四周", "兼顾服务与现金风险", "执行委员会"));
        var outcome = service.RecordOutcome(created.ItemId, new CoordinationOutcomeUpdateRequest("两周后红区次数下降，服务恢复至目标带", "DDS&OP 计划员"));
        var completed = service.UpdateStatus(created.ItemId, new CoordinationStatusUpdateRequest("Completed", "DDS&OP 计划员", "验证点已通过"));
        var detail = service.GetDetail(created.ItemId);
        var audit = service.GetAuditEvents(created.ItemId);

        AssertEqual("Open", created.Status, "coordination initial status");
        AssertEqual("InProgress", inProgress.Status, "coordination in-progress status");
        AssertEqual("Escalated", escalated.Status, "coordination escalated status");
        AssertEqual("InProgress", resumed.Status, "escalated item may resume");
        AssertEqual("启用第二来源并限定四周", decided.Decision, "coordination decision");
        AssertTrue(outcome.ActualOutcome?.Contains("服务恢复", StringComparison.Ordinal) == true, "coordination actual outcome");
        AssertEqual("Completed", completed.Status, "coordination completed status");
        AssertEqual("RUN-001", detail!.RelatedScenarioRunId, "coordination scenario link");
        AssertEqual("CHANGE-001", detail.RelatedMasterSettingChangeId, "coordination governance link");
        AssertTrue(service.List(20).Any(item => item.ItemId == created.ItemId), "coordination list should contain item");
        AssertTrue(new[] { "CoordinationItemCreated", "StatusChanged", "DecisionRecorded", "OutcomeRecorded" }
            .All(eventType => audit.Any(item => item.EventType == eventType)), "coordination audit should cover all update types");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestCoordinationLedgerRejectsInvalidDirectCompletion()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-coordination-{Guid.NewGuid():N}.db");
    try
    {
        var service = new CoordinationLedgerService(databasePath);
        var created = service.Create(new CoordinationItemCreateRequest(
            "能力保护复查",
            new[] { "R-MIX" },
            null,
            null,
            "服务影响待确认",
            "库存影响待确认",
            null,
            "能力保护",
            "确认临时能力是否持续",
            "运营经理",
            "2026-07-21",
            "部门层",
            "2026-07-18",
            "DDS&OP 计划员"));
        var rejected = false;
        try
        {
            service.UpdateStatus(created.ItemId, new CoordinationStatusUpdateRequest("Completed", "运营经理", null));
        }
        catch (ArgumentException)
        {
            rejected = true;
        }

        AssertTrue(rejected, "Open item must not jump directly to Completed");
        AssertEqual("Open", service.GetDetail(created.ItemId)!.Status, "invalid transition must not mutate item");

        var missingRejected = false;
        try
        {
            service.RecordDecision("missing-item", new CoordinationDecisionUpdateRequest("决策", "理由", "运营经理"));
        }
        catch (KeyNotFoundException)
        {
            missingRejected = true;
        }
        AssertTrue(missingRejected, "missing coordination item should use the not-found exception path");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestKnownSmokeRecordRepairIsScopedAuditedAndIdempotent()
{
    const string repairId = "2026-07-15-known-local-smoke-repair-v1";
    const string firstTargetItemId = "09944ca75dfa4efab765d1481c860709";
    const string secondTargetItemId = "8aead2083210423db98e9f35924c7f8e";
    const string similarItemId = "09944ca75dfa4efab765d1481c860708";
    const string targetSnapshotId = "baseline-smoke-target";
    const string targetSnapshotNumber = "BASE-20260714-002";
    const string existingAuditMessage = "原始中文审计：不得改写。";
    const string existingBusinessNote = "客户确认：保留原备注";
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-local-repair-{Guid.NewGuid():N}.db");
    var controlDatabasePath = Path.Combine(Path.GetTempPath(), $"ddae-local-repair-control-{Guid.NewGuid():N}.db");
    var failureDatabasePath = Path.Combine(Path.GetTempPath(), $"ddae-local-repair-rollback-{Guid.NewGuid():N}.db");
    try
    {
        var services = EnsureInternalSqliteSchemas(databasePath);
        var normalItem = services.Coordination.Create(new CoordinationItemCreateRequest(
            "正常中文协调事项",
            new[] { "用户数据" },
            null,
            null,
            "服务正常",
            "库存正常",
            null,
            "无额外风险",
            "保持观察",
            "王经理",
            "2026-07-25",
            "部门层",
            "2026-07-20",
            "李计划员"));
        SeedRepairCoordinationItem(databasePath, firstTargetItemId, "SMOKE-001", "已知烟测乱码一", "烟测人员", "Codex ??",
            "烟测审计一", "烟测审计二");
        SeedRepairCoordinationItem(databasePath, secondTargetItemId, "SMOKE-002", "已知烟测乱码二", "烟测人员", "Codex ??",
            "烟测审计三");
        SeedRepairCoordinationItem(databasePath, similarItemId, "CONTROL-001", "只差一字符的正常事项", "陈经理", "周计划员",
            "相似 ID 审计必须保留");
        SeedRepairBaseline(databasePath, targetSnapshotId, targetSnapshotNumber, "Codex ??", existingAuditMessage, 7, existingBusinessNote);
        SeedRepairBaseline(databasePath, "baseline-same-old-value", "BASE-20260714-003", "Codex ??", null, null);
        SeedRepairBaseline(databasePath, "baseline-normal-chinese", "BASE-20260714-004", "王计划员", null, null);

        var triggerSqlBefore = Convert.ToString(ReadSqliteScalar(
            databasePath,
            "SELECT sql FROM sqlite_master WHERE type = 'trigger' AND name = $name;",
            ("$name", "trg_current_baseline_snapshots_no_update")))!;
        AssertTrue(!string.IsNullOrWhiteSpace(triggerSqlBefore), "baseline no-update trigger should exist before repair");

        ILocalDatabaseRepairService service = new LocalDatabaseRepairService(databasePath);
        var constructors = typeof(LocalDatabaseRepairService).GetConstructors();
        AssertEqual(1, constructors.Length, "local repair service public constructor count");
        AssertTrue(
            constructors[0].GetParameters() is [{ ParameterType: var parameterType }] && parameterType == typeof(string),
            "local repair service constructor should accept only databasePath");
        var result = service.Apply();

        AssertEqual(repairId, result.RepairId, "fixed local repair ID");
        AssertTrue(!result.WasAlreadyApplied, "first repair should apply");
        AssertEqual(2, result.DeletedCoordinationItems, "deleted exact coordination item count");
        AssertEqual(3, result.DeletedCoordinationAuditEvents, "deleted exact coordination audit count");
        AssertEqual(1, result.RepairedBaselines, "repaired baseline count");
        AssertEqual(1, result.AddedBaselineAuditEvents, "added corrective baseline audit count");

        AssertEqual(0L, Convert.ToInt64(ReadSqliteScalar(databasePath,
            "SELECT COUNT(*) FROM coordination_items WHERE item_id IN ($first, $second);",
            ("$first", firstTargetItemId), ("$second", secondTargetItemId))), "only exact smoke items should be deleted");
        AssertEqual(0L, Convert.ToInt64(ReadSqliteScalar(databasePath,
            "SELECT COUNT(*) FROM coordination_item_audit_events WHERE item_id IN ($first, $second);",
            ("$first", firstTargetItemId), ("$second", secondTargetItemId))), "all exact smoke item audits should be deleted");
        AssertEqual(1L, Convert.ToInt64(ReadSqliteScalar(databasePath,
            "SELECT COUNT(*) FROM coordination_items WHERE item_id = $item_id;", ("$item_id", similarItemId))), "similar coordination item should remain");
        AssertEqual(1L, Convert.ToInt64(ReadSqliteScalar(databasePath,
            "SELECT COUNT(*) FROM coordination_item_audit_events WHERE item_id = $item_id;", ("$item_id", similarItemId))), "similar coordination audit should remain");
        AssertTrue(services.Coordination.GetDetail(normalItem.ItemId) is not null, "normal coordination item should remain");
        AssertEqual(1, services.Coordination.GetAuditEvents(normalItem.ItemId).Count, "normal coordination audit should remain");

        AssertEqual("Codex 烟测", Convert.ToString(ReadSqliteScalar(databasePath,
            "SELECT created_by FROM current_baseline_snapshots WHERE snapshot_id = $snapshot_id;", ("$snapshot_id", targetSnapshotId))),
            "double-condition target baseline creator");
        AssertEqual(existingBusinessNote, Convert.ToString(ReadSqliteScalar(databasePath,
            "SELECT note FROM current_baseline_snapshots WHERE snapshot_id = $snapshot_id;", ("$snapshot_id", targetSnapshotId))),
            "target baseline business note must remain unchanged");
        AssertEqual("Codex ??", Convert.ToString(ReadSqliteScalar(databasePath,
            "SELECT created_by FROM current_baseline_snapshots WHERE snapshot_id = $snapshot_id;", ("$snapshot_id", "baseline-same-old-value"))),
            "same old creator on another snapshot number should remain");
        AssertEqual("王计划员", Convert.ToString(ReadSqliteScalar(databasePath,
            "SELECT created_by FROM current_baseline_snapshots WHERE snapshot_id = $snapshot_id;", ("$snapshot_id", "baseline-normal-chinese"))),
            "normal Chinese baseline should remain");
        AssertEqual(existingAuditMessage, Convert.ToString(ReadSqliteScalar(databasePath,
            "SELECT message FROM current_baseline_audit_events WHERE snapshot_id = $snapshot_id AND sequence = 7;", ("$snapshot_id", targetSnapshotId))),
            "existing baseline audit message should remain byte-for-byte");
        AssertEqual(1L, Convert.ToInt64(ReadSqliteScalar(databasePath,
            "SELECT COUNT(*) FROM current_baseline_audit_events WHERE snapshot_id = $snapshot_id AND event_type = 'DataRepairApplied';",
            ("$snapshot_id", targetSnapshotId))), "one corrective baseline audit should be appended");
        var correctiveAuditMessage = Convert.ToString(ReadSqliteScalar(databasePath,
            "SELECT message FROM current_baseline_audit_events WHERE snapshot_id = $snapshot_id AND event_type = 'DataRepairApplied';",
            ("$snapshot_id", targetSnapshotId)))!;
        AssertTrue(!correctiveAuditMessage.Contains("??", StringComparison.Ordinal),
            "corrective baseline audit must not repeat the known consecutive-question-mark corruption");
        AssertTrue(!correctiveAuditMessage.Contains('\uFFFD'),
            "corrective baseline audit must not contain the Unicode replacement character");
        AssertEqual(8L, Convert.ToInt64(ReadSqliteScalar(databasePath,
            "SELECT sequence FROM current_baseline_audit_events WHERE snapshot_id = $snapshot_id AND event_type = 'DataRepairApplied';",
            ("$snapshot_id", targetSnapshotId))), "corrective audit sequence should follow the current maximum");
        AssertEqual(1L, Convert.ToInt64(ReadSqliteScalar(databasePath,
            "SELECT COUNT(*) FROM local_data_repairs WHERE repair_id = $repair_id;", ("$repair_id", repairId))), "repair journal should be written");
        AssertEqual(triggerSqlBefore, Convert.ToString(ReadSqliteScalar(
            databasePath,
            "SELECT sql FROM sqlite_master WHERE type = 'trigger' AND name = $name;",
            ("$name", "trg_current_baseline_snapshots_no_update"))), "no-update trigger SQL should be restored exactly");
        AssertEqual(4L, Convert.ToInt64(ReadSqliteScalar(databasePath,
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'trigger' AND name LIKE 'trg_current_baseline%';")),
            "the other immutable baseline triggers should remain untouched");
        AssertBaselineUpdateTriggerBlocks(databasePath, targetSnapshotId);

        var firstState = ReadRepairDatabaseState(databasePath);
        var secondResult = service.Apply();
        AssertEqual(repairId, secondResult.RepairId, "idempotent repair ID");
        AssertTrue(secondResult.WasAlreadyApplied, "second repair should report already applied");
        AssertEqual(0, secondResult.DeletedCoordinationItems, "second repair deleted coordination items");
        AssertEqual(0, secondResult.DeletedCoordinationAuditEvents, "second repair deleted coordination audits");
        AssertEqual(0, secondResult.RepairedBaselines, "second repair repaired baselines");
        AssertEqual(0, secondResult.AddedBaselineAuditEvents, "second repair added baseline audits");
        AssertEqual(firstState, ReadRepairDatabaseState(databasePath), "second repair should leave database contents unchanged");

        _ = EnsureInternalSqliteSchemas(failureDatabasePath);
        SeedRepairCoordinationItem(failureDatabasePath, firstTargetItemId, "ROLLBACK-SMOKE-001", "回滚烟测事项一", "烟测人员", "Codex ??",
            "回滚审计一", "回滚审计二");
        SeedRepairCoordinationItem(failureDatabasePath, secondTargetItemId, "ROLLBACK-SMOKE-002", "回滚烟测事项二", "烟测人员", "Codex ??",
            "回滚审计三");
        SeedRepairBaseline(failureDatabasePath, targetSnapshotId, targetSnapshotNumber, "Codex ??", existingAuditMessage, 7);
        var failureTriggerSqlBefore = Convert.ToString(ReadSqliteScalar(
            failureDatabasePath,
            "SELECT sql FROM sqlite_master WHERE type = 'trigger' AND name = $name;",
            ("$name", "trg_current_baseline_snapshots_no_update")))!;
        using (var failureConnection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={failureDatabasePath}"))
        {
            failureConnection.Open();
            using var failureTrigger = failureConnection.CreateCommand();
            failureTrigger.CommandText = """
                CREATE TRIGGER trg_test_block_data_repair_audit
                BEFORE INSERT ON current_baseline_audit_events
                WHEN NEW.event_type = 'DataRepairApplied'
                BEGIN
                    SELECT RAISE(ABORT, 'test blocks corrective audit');
                END;
                """;
            failureTrigger.ExecuteNonQuery();
        }
        string? repairFailureMessage = null;
        try
        {
            new LocalDatabaseRepairService(failureDatabasePath).Apply();
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex)
        {
            repairFailureMessage = ex.Message;
        }
        AssertTrue(repairFailureMessage?.Contains("test blocks corrective audit", StringComparison.Ordinal) == true,
            "the test-only corrective-audit trigger should be the exact repair failure");
        AssertEqual(2L, Convert.ToInt64(ReadSqliteScalar(failureDatabasePath,
            "SELECT COUNT(*) FROM coordination_items WHERE item_id IN ($first, $second);",
            ("$first", firstTargetItemId), ("$second", secondTargetItemId))),
            "failed repair should roll back both coordination item deletions");
        AssertEqual(3L, Convert.ToInt64(ReadSqliteScalar(failureDatabasePath,
            "SELECT COUNT(*) FROM coordination_item_audit_events WHERE item_id IN ($first, $second);",
            ("$first", firstTargetItemId), ("$second", secondTargetItemId))),
            "failed repair should roll back all coordination audit deletions");
        AssertEqual("Codex ??", Convert.ToString(ReadSqliteScalar(failureDatabasePath,
            "SELECT created_by FROM current_baseline_snapshots WHERE snapshot_id = $snapshot_id;", ("$snapshot_id", targetSnapshotId))),
            "failed repair should roll back baseline creator update");
        AssertEqual(existingAuditMessage, Convert.ToString(ReadSqliteScalar(failureDatabasePath,
            "SELECT message FROM current_baseline_audit_events WHERE snapshot_id = $snapshot_id AND sequence = 7;", ("$snapshot_id", targetSnapshotId))),
            "failed repair should preserve the existing baseline audit");
        AssertEqual(0L, Convert.ToInt64(ReadSqliteScalar(failureDatabasePath,
            "SELECT COUNT(*) FROM current_baseline_audit_events WHERE event_type = 'DataRepairApplied';")),
            "failed repair should not leave a corrective baseline audit");
        AssertEqual(failureTriggerSqlBefore, Convert.ToString(ReadSqliteScalar(
            failureDatabasePath,
            "SELECT sql FROM sqlite_master WHERE type = 'trigger' AND name = $name;",
            ("$name", "trg_current_baseline_snapshots_no_update"))),
            "failed repair should roll back to the original no-update trigger SQL");
        AssertBaselineUpdateTriggerBlocks(failureDatabasePath, targetSnapshotId);
        AssertEqual(0L, Convert.ToInt64(ReadSqliteScalar(failureDatabasePath,
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'local_data_repairs';")),
            "failed first repair should roll back journal table creation and leave no repair row");

        _ = EnsureInternalSqliteSchemas(controlDatabasePath);
        SeedRepairBaseline(controlDatabasePath, "baseline-same-number-normal", targetSnapshotNumber, "赵计划员", "正常审计必须保留", 3);
        var controlTriggerSqlBefore = Convert.ToString(ReadSqliteScalar(
            controlDatabasePath,
            "SELECT sql FROM sqlite_master WHERE type = 'trigger' AND name = $name;",
            ("$name", "trg_current_baseline_snapshots_no_update")))!;
        var controlService = new LocalDatabaseRepairService(controlDatabasePath);
        var controlFirst = controlService.Apply();
        AssertTrue(!controlFirst.WasAlreadyApplied, "zero-match control repair should still journal its first attempt");
        AssertEqual(0, controlFirst.DeletedCoordinationItems, "zero-match control deleted coordination items");
        AssertEqual(0, controlFirst.DeletedCoordinationAuditEvents, "zero-match control deleted coordination audits");
        AssertEqual(0, controlFirst.RepairedBaselines, "same-number normal creator should not be repaired");
        AssertEqual(0, controlFirst.AddedBaselineAuditEvents, "same-number normal creator should not receive corrective audit");
        AssertEqual("赵计划员", Convert.ToString(ReadSqliteScalar(controlDatabasePath,
            "SELECT created_by FROM current_baseline_snapshots WHERE snapshot_id = $snapshot_id;", ("$snapshot_id", "baseline-same-number-normal"))),
            "same-number normal Chinese creator should remain");
        AssertEqual("正常审计必须保留", Convert.ToString(ReadSqliteScalar(controlDatabasePath,
            "SELECT message FROM current_baseline_audit_events WHERE snapshot_id = $snapshot_id AND sequence = 3;", ("$snapshot_id", "baseline-same-number-normal"))),
            "zero-match control audit should remain");
        AssertEqual(0L, Convert.ToInt64(ReadSqliteScalar(controlDatabasePath,
            "SELECT COUNT(*) FROM current_baseline_audit_events WHERE event_type = 'DataRepairApplied';")),
            "zero-match control should not append corrective audit");
        AssertEqual(1L, Convert.ToInt64(ReadSqliteScalar(controlDatabasePath,
            "SELECT COUNT(*) FROM local_data_repairs WHERE repair_id = $repair_id;", ("$repair_id", repairId))),
            "zero-match control should write journal");
        AssertEqual(controlTriggerSqlBefore, Convert.ToString(ReadSqliteScalar(
            controlDatabasePath,
            "SELECT sql FROM sqlite_master WHERE type = 'trigger' AND name = $name;",
            ("$name", "trg_current_baseline_snapshots_no_update"))), "zero-match repair should restore no-update trigger SQL exactly");
        var controlFirstState = ReadRepairDatabaseState(controlDatabasePath);
        var controlSecond = controlService.Apply();
        AssertTrue(controlSecond.WasAlreadyApplied, "zero-match control second repair should report already applied");
        AssertEqual(0, controlSecond.DeletedCoordinationItems, "zero-match control second deleted coordination items");
        AssertEqual(0, controlSecond.DeletedCoordinationAuditEvents, "zero-match control second deleted coordination audits");
        AssertEqual(0, controlSecond.RepairedBaselines, "zero-match control second repaired baselines");
        AssertEqual(0, controlSecond.AddedBaselineAuditEvents, "zero-match control second added baseline audits");
        AssertEqual(controlFirstState, ReadRepairDatabaseState(controlDatabasePath), "zero-match control second repair should leave database contents unchanged");

        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var program = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Program.cs"));
        var buildIndex = program.IndexOf("var app = builder.Build();", StringComparison.Ordinal);
        var baselineResolveIndex = program.IndexOf("GetRequiredService<CurrentBaselineService>()", buildIndex, StringComparison.Ordinal);
        var coordinationResolveIndex = program.IndexOf("GetRequiredService<CoordinationLedgerService>()", baselineResolveIndex, StringComparison.Ordinal);
        var scenarioResolveIndex = program.IndexOf("GetRequiredService<ScenarioRunPersistenceService>()", coordinationResolveIndex, StringComparison.Ordinal);
        var governanceResolveIndex = program.IndexOf("GetRequiredService<MasterSettingsGovernanceService>()", scenarioResolveIndex, StringComparison.Ordinal);
        var repairResolveIndex = program.IndexOf("GetRequiredService<ILocalDatabaseRepairService>()", governanceResolveIndex, StringComparison.Ordinal);
        var repairApplyIndex = program.IndexOf("repair.Apply();", repairResolveIndex, StringComparison.Ordinal);
        AssertTrue(program.Contains("AddSingleton<ILocalDatabaseRepairService>", StringComparison.Ordinal), "local repair service should be registered as its interface");
        AssertTrue(buildIndex >= 0 && baselineResolveIndex > buildIndex && coordinationResolveIndex > baselineResolveIndex
            && scenarioResolveIndex > coordinationResolveIndex && governanceResolveIndex > scenarioResolveIndex
            && repairResolveIndex > governanceResolveIndex && repairApplyIndex > repairResolveIndex,
            "startup should explicitly resolve four SQLite singletons in order before one repair Apply");
        AssertTrue(!program.Contains("MapGet(\"/api/local-data-repair", StringComparison.Ordinal)
            && !program.Contains("MapPost(\"/api/local-data-repair", StringComparison.Ordinal),
            "local repair must not expose an API endpoint");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
        DeleteSqliteFiles(controlDatabasePath);
        DeleteSqliteFiles(failureDatabasePath);
    }
}

static void TestSqliteRoundTripsChineseWithoutQuestionMarks()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-unicode-round-trip-{Guid.NewGuid():N}.db");
    try
    {
        var services = EnsureInternalSqliteSchemas(databasePath);
        var service = services.Coordination;
        var created = service.Create(new CoordinationItemCreateRequest(
            "跨部门协调：星载设备补货",
            new[] { "星载设备", "华东供应商" },
            null,
            null,
            "服务水平保持在目标带",
            "库存维持健康区间",
            86000m,
            "供应连续性风险",
            "确认第二来源启用窗口",
            "供应链经理",
            "2026-07-22",
            "执行层",
            "2026-07-19",
            "DDS&OP 计划员"));
        service.RecordDecision(created.ItemId, new CoordinationDecisionUpdateRequest(
            "启用第二来源并限定四周",
            "兼顾服务、库存与现金风险",
            "执行委员会"));
        service.RecordOutcome(created.ItemId, new CoordinationOutcomeUpdateRequest(
            "两周后缺料风险下降，交付恢复稳定",
            "DDS&OP 计划员"));
        var loaded = service.GetDetail(created.ItemId)!;
        var frozen = services.Baseline.Freeze(new CurrentBaselineFreezeRequest("张计划员", "月度例会中文基线"));
        var loadedBaseline = services.Baseline.GetDetail(frozen.SnapshotId)!;

        AssertEqual("跨部门协调：星载设备补货", loaded.Title, "coordination Chinese title round trip");
        AssertEqual("供应链经理", loaded.Owner, "coordination Chinese owner round trip");
        AssertEqual("启用第二来源并限定四周", loaded.Decision, "coordination Chinese decision round trip");
        AssertEqual("兼顾服务、库存与现金风险", loaded.DecisionRationale, "coordination Chinese rationale round trip");
        AssertEqual("两周后缺料风险下降，交付恢复稳定", loaded.ActualOutcome, "coordination Chinese outcome round trip");
        AssertEqual("DDS&OP 计划员", loaded.CreatedBy, "coordination Chinese creator round trip");
        AssertEqual("张计划员", loadedBaseline.CreatedBy, "baseline Chinese creator round trip");
        foreach (var value in new[] { loaded.Title, loaded.Owner, loaded.Decision!, loaded.DecisionRationale!, loaded.ActualOutcome!, loaded.CreatedBy, loadedBaseline.CreatedBy })
        {
            AssertTrue(!value.Contains('\uFFFD'), "round-tripped Chinese must not contain the Unicode replacement character");
            AssertTrue(!value.Contains("??", StringComparison.Ordinal), "round-tripped Chinese must not contain consecutive question marks");
        }
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestFiveStageNavigationPreservesValidationPages()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    var historyNav = page.IndexOf("data-stage-route=\"#history-review-panel\"", StringComparison.Ordinal);
    var baselineNav = page.IndexOf("data-stage-route=\"#current-baseline-panel\"", StringComparison.Ordinal);
    var futureNav = page.IndexOf("data-stage-route=\"#future-scenario-panel\"", StringComparison.Ordinal);
    var ddomNav = page.IndexOf("data-stage-route=\"#ddom-decision-panel\"", StringComparison.Ordinal);
    var coordinationNav = page.IndexOf("data-stage-route=\"#coordination-panel\"", StringComparison.Ordinal);
    var validationGroup = page.IndexOf("验证与追踪", StringComparison.Ordinal);
    var traceNav = page.IndexOf("href=\"#trace-panel\"", StringComparison.Ordinal);
    var publicDemoNav = page.IndexOf("href=\"#public-demo-golden-loop-panel\"", StringComparison.Ordinal);

    AssertTrue(historyNav >= 0 && historyNav < baselineNav && baselineNav < futureNav && futureNav < ddomNav && ddomNav < coordinationNav, "five primary workspaces should exist in the required order");
    AssertTrue(coordinationNav < validationGroup && validationGroup < traceNav && traceNav < publicDemoNav, "validation group should follow the five workspaces and public demo should be last");
    AssertTrue(page.Contains("主业务流程", StringComparison.Ordinal), "sidebar should label primary workflow group");
    AssertTrue(page.Contains("id=\"history-review-panel\"", StringComparison.Ordinal), "history review workspace should exist");
    AssertTrue(page.Contains("id=\"current-baseline-panel\"", StringComparison.Ordinal), "current baseline workspace should exist");
    AssertTrue(page.Contains("id=\"future-scenario-panel\"", StringComparison.Ordinal), "future scenario workspace should exist");
    AssertTrue(page.Contains("id=\"ddom-decision-panel\"", StringComparison.Ordinal), "DDOM decision workspace should exist");
    AssertTrue(page.Contains("id=\"coordination-panel\"", StringComparison.Ordinal), "coordination workspace should exist");
    AssertTrue(page.Contains("id=\"trace-panel\"", StringComparison.Ordinal) && page.Contains("id=\"trace-list\"", StringComparison.Ordinal), "white-box trace page and trace DOM should remain");
    AssertTrue(page.Contains("id=\"public-demo-golden-loop-panel\"", StringComparison.Ordinal), "public demo page should remain independent");
    AssertTrue(page.Contains("id=\"refresh-public-demo\"", StringComparison.Ordinal) && page.Contains("id=\"write-public-demo-payload\"", StringComparison.Ordinal), "public demo buttons should remain");
    AssertTrue(script.Contains("/api/public-demo-golden-loop", StringComparison.Ordinal) && script.Contains("/api/public-demo-golden-loop/write-payload", StringComparison.Ordinal), "public demo API behavior should remain");
    AssertTrue(script.Contains("/api/history-review", StringComparison.Ordinal), "history UI should load history API");
    AssertTrue(script.Contains("/api/current-baselines/candidate", StringComparison.Ordinal), "baseline UI should load candidate API");
    AssertTrue(script.Contains("/api/scenario-runs/compare", StringComparison.Ordinal), "future UI should use scenario comparison API");
    AssertTrue(script.Contains("/api/coordination-items", StringComparison.Ordinal), "coordination UI should use ledger API");
    AssertTrue(script.Contains("item.name", StringComparison.Ordinal) && script.Contains("item.sourceAuthority", StringComparison.Ordinal), "baseline evidence UI should use the serialized section field names");
    AssertTrue(script.Contains("Promise.allSettled", StringComparison.Ordinal), "optional five-stage data loads should be isolated from validation pages");
    var loadWorkspace = script.IndexOf("async function loadWorkspace()", StringComparison.Ordinal);
    var coreRender = script.IndexOf("applyFilters();", loadWorkspace, StringComparison.Ordinal);
    var optionalSettlement = script.IndexOf("fiveStageDataPromise.then", loadWorkspace, StringComparison.Ordinal);
    AssertTrue(loadWorkspace >= 0 && coreRender > loadWorkspace && coreRender < optionalSettlement, "core and validation pages must render before optional five-stage requests settle");
    AssertTrue(page.Contains("id=\"governance-owner\"", StringComparison.Ordinal) && page.Contains("id=\"governance-effective-through\"", StringComparison.Ordinal), "DDOM decision workspace should collect governance ownership and expiry metadata");
    AssertTrue(script.Contains("/api/master-settings/proposals/from-comparison", StringComparison.Ordinal), "DDOM governance proposals should be generated from the frozen comparison endpoint");
    AssertTrue(!page.Contains("id=\"auto-adopt\"", StringComparison.Ordinal) && !page.Contains("id=\"auto-approve\"", StringComparison.Ordinal) && !page.Contains("id=\"auto-effect\"", StringComparison.Ordinal), "UI must not expose automatic governance operations");
}

static void TestFiveStageNavigationUsesHierarchicalViewSwitching()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var css = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "css", "site.css"));
    var navigationStart = page.IndexOf("<nav class=\"nav-list\"", StringComparison.Ordinal);
    var navigationEnd = page.IndexOf("</nav>", navigationStart, StringComparison.Ordinal);

    AssertTrue(navigationStart >= 0 && navigationEnd > navigationStart, "page should contain the left navigation");
    var navigation = page.Substring(navigationStart, navigationEnd - navigationStart);
    var stages = new (string Route, string Label, string SubmenuId, (string Route, string Label)[] Children)[]
    {
        ("#history-review-panel", "历史回顾", "history-review-submenu", new[]
        {
            ("#history-review-panel/operating-results", "经营结果"),
            ("#history-review-panel/buffer-performance", "缓冲表现"),
            ("#history-review-panel/sizing-trace", "定容追溯"),
            ("#history-review-panel/capacity-constraints", "能力约束")
        }),
        ("#current-baseline-panel", "当前状态基线", "current-baseline-submenu", new[]
        {
            ("#current-baseline-panel/meeting-snapshot", "会前快照"),
            ("#current-baseline-panel/evidence-review", "证据检查"),
            ("#current-baseline-panel/version-freeze", "版本冻结"),
            ("#current-baseline-panel/audit-records", "审计记录")
        }),
        ("#future-scenario-panel", "未来场景模拟", "future-scenario-submenu", new[]
        {
            ("#future-scenario-panel/scenario-config", "场景配置"),
            ("#future-scenario-panel/plan-comparison", "方案比较"),
            ("#future-scenario-panel/inventory-buffer", "库存缓冲"),
            ("#future-scenario-panel/time-buffer", "时间缓冲"),
            ("#future-scenario-panel/capacity-buffer", "能力缓冲"),
            ("#future-scenario-panel/supply-risk", "供应风险"),
            ("#future-scenario-panel/breach-analysis", "击穿分析")
        }),
        ("#ddom-decision-panel", "DDOM 配置决策", "ddom-decision-submenu", new[]
        {
            ("#ddom-decision-panel/structure-settings", "结构设置"),
            ("#ddom-decision-panel/parameter-decision", "参数决策"),
            ("#ddom-decision-panel/temporary-adjustment", "临时调整"),
            ("#ddom-decision-panel/change-records", "变更记录")
        }),
        ("#coordination-panel", "行动和决策", "coordination-submenu", new[]
        {
            ("#coordination-panel/issue-list", "问题清单"),
            ("#coordination-panel/action-tracking", "行动跟踪"),
            ("#coordination-panel/decision-records", "决策记录"),
            ("#coordination-panel/outcome-validation", "结果验证")
        })
    };

    AssertEqual(5, CountExactOccurrences(navigation, "class=\"nav-stage-group\""), "primary navigation stage group count");
    AssertEqual(5, CountExactOccurrences(navigation, "class=\"nav-stage-toggle nav-item\""), "primary navigation stage toggle count");
    AssertEqual(23, CountExactOccurrences(navigation, "class=\"nav-subitem\""), "secondary navigation item count");

    var previousStagePosition = -1;
    foreach (var stage in stages)
    {
        var stageMarker = $"data-stage-route=\"{stage.Route}\"";
        var stagePosition = navigation.IndexOf(stageMarker, StringComparison.Ordinal);
        AssertTrue(stagePosition > previousStagePosition, $"stage {stage.Label} should appear in the required order");
        previousStagePosition = stagePosition;

        var toggleTag = OpeningTagContaining(navigation, stageMarker);
        AssertTrue(toggleTag.StartsWith("<button ", StringComparison.Ordinal), $"stage {stage.Label} should use a button");
        AssertTrue(toggleTag.Contains("aria-expanded=", StringComparison.Ordinal), $"stage {stage.Label} should expose expanded state");
        AssertTrue(toggleTag.Contains($"aria-controls=\"{stage.SubmenuId}\"", StringComparison.Ordinal), $"stage {stage.Label} should control its submenu");
        AssertTrue(navigation.Contains($"id=\"{stage.SubmenuId}\" class=\"nav-submenu\"", StringComparison.Ordinal), $"stage {stage.Label} should have a unique submenu");
        AssertTrue(navigation.Contains($">{stage.Label}</span>", StringComparison.Ordinal), $"stage {stage.Label} should use the approved title");

        var previousChildPosition = stagePosition;
        foreach (var child in stage.Children)
        {
            AssertTrue(child.Label.Length <= 6, $"secondary title {child.Label} should not exceed six Chinese characters");
            var childMarker = $"href=\"{child.Route}\"";
            var childPosition = navigation.IndexOf(childMarker, previousChildPosition, StringComparison.Ordinal);
            AssertTrue(childPosition > previousChildPosition, $"secondary route {child.Route} should appear in the required order");
            AssertTrue(navigation.IndexOf($">{child.Label}</a>", childPosition, StringComparison.Ordinal) >= childPosition, $"secondary route {child.Route} should use its approved title");
            previousChildPosition = childPosition;
        }
    }

    var validationGroup = navigation.IndexOf("验证与追踪", previousStagePosition, StringComparison.Ordinal);
    var traceNavigation = navigation.IndexOf("href=\"#trace-panel\"", validationGroup, StringComparison.Ordinal);
    var publicDemoNavigation = navigation.IndexOf("href=\"#public-demo-golden-loop-panel\"", traceNavigation, StringComparison.Ordinal);
    AssertTrue(validationGroup > previousStagePosition && traceNavigation > validationGroup && publicDemoNavigation > traceNavigation, "validation entries should follow all five stage groups");
    AssertTrue(navigation.IndexOf("class=\"nav-item\"", publicDemoNavigation + 1, StringComparison.Ordinal) < 0, "public demo should remain the last navigation item");
    foreach (var selector in new[] { ".nav-stage-group", ".nav-submenu", ".nav-subitem", ".nav-stage-indicator", ".workspace-breadcrumb" })
    {
        AssertTrue(css.Contains(selector, StringComparison.Ordinal), $"CSS should style hierarchical navigation selector {selector}");
    }
    AssertTrue(css.Contains("overflow-y: auto", StringComparison.Ordinal), "the selected workspace view should scroll inside the application area");
    AssertTrue(css.Contains(".nav-subitem:focus-visible", StringComparison.Ordinal), "secondary navigation should expose a visible keyboard focus state");
}

static void TestWorkspaceNavigationRemovesScrollObserverAndUsesHashState()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));
    var expectedFunctions = new[]
    {
        "parseWorkspaceRoute",
        "formatWorkspaceHash",
        "resolveWorkspaceRoute",
        "navigateWorkspace",
        "applyWorkspaceRoute",
        "setExpandedStageNavigation",
        "setActiveWorkspaceNavigation",
        "renderWorkspaceBreadcrumb",
        "handleWorkspaceHashChange"
    };

    AssertTrue(script.Contains("workspaceRoutes = Object.freeze", StringComparison.Ordinal), "workspace routes should be a frozen explicit registry");
    foreach (var functionName in expectedFunctions)
    {
        AssertTrue(script.Contains($"function {functionName}(", StringComparison.Ordinal), $"script should implement {functionName}");
    }

    AssertTrue(script.Contains("addEventListener(\"hashchange\", handleWorkspaceHashChange)", StringComparison.Ordinal), "workspace route should react to browser hash changes");
    AssertTrue(script.Contains("history.replaceState", StringComparison.Ordinal), "workspace route should canonicalize defaults and aliases without adding history entries");
    AssertTrue(script.Contains("navigateWorkspace(\"future-scenario-panel\", \"scenario-config\", false)", StringComparison.Ordinal), "exception action should navigate to future scenario configuration");
    AssertTrue(script.Contains("navigateWorkspace(\"ddom-decision-panel\", \"parameter-decision\", false)", StringComparison.Ordinal), "governance action should navigate to DDOM parameter decision");
    AssertTrue(script.Contains("workspace.scrollTop = 0", StringComparison.Ordinal), "every workspace view switch should reset the actual scrolling container");
    AssertTrue(script.Contains("\"#overview-panel\": \"#ddom-decision-panel/structure-settings\"", StringComparison.Ordinal), "legacy overview hash should have a read-only canonical alias");
    AssertTrue(script.Contains("\"#saved-scenarios-panel\": \"#coordination-panel/action-tracking\"", StringComparison.Ordinal), "legacy saved scenario hash should point to action tracking");
    AssertEqual(1, CountExactOccurrences(script, "requiredHostId: \"saved-scenarios-panel\""), "only white-box trace should require the saved scenarios host");
    AssertEqual(29, CountExactOccurrences(script, "requiredHostId: null"), "all other workspace routes should be host independent");

    foreach (var forbidden in new[] { "state.activeTab", "normalizeWorkspaceFlow", "IntersectionObserver", "setActiveNav", "function activateTab(", "[data-tab]" })
    {
        AssertTrue(!script.Contains(forbidden, StringComparison.Ordinal), $"hash navigation should remove legacy runtime {forbidden}");
    }
}

static void TestOnlySelectedStageOrChildViewIsVisible()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));
    var routes = new (string Hash, string TargetId)[]
    {
        ("#history-review-panel", "history-review-panel"),
        ("#history-review-panel/operating-results", "history-operating-results-view"),
        ("#history-review-panel/buffer-performance", "history-buffer-performance-view"),
        ("#history-review-panel/sizing-trace", "history-sizing-trace-view"),
        ("#history-review-panel/capacity-constraints", "history-capacity-constraints-view"),
        ("#current-baseline-panel", "current-baseline-panel"),
        ("#current-baseline-panel/meeting-snapshot", "baseline-meeting-snapshot-view"),
        ("#current-baseline-panel/evidence-review", "baseline-evidence-review-view"),
        ("#current-baseline-panel/version-freeze", "baseline-version-freeze-view"),
        ("#current-baseline-panel/audit-records", "baseline-audit-records-view"),
        ("#future-scenario-panel", "future-scenario-panel"),
        ("#future-scenario-panel/scenario-config", "scenario-run-panel"),
        ("#future-scenario-panel/plan-comparison", "scenario-comparison"),
        ("#future-scenario-panel/inventory-buffer", "buffer-trend-panel"),
        ("#future-scenario-panel/time-buffer", "time-buffer-panel"),
        ("#future-scenario-panel/capacity-buffer", "rccp-panel"),
        ("#future-scenario-panel/supply-risk", "projected-supply-panel"),
        ("#future-scenario-panel/breach-analysis", "variance-panel"),
        ("#ddom-decision-panel", "ddom-decision-panel"),
        ("#ddom-decision-panel/structure-settings", "ddom-structure-settings-view"),
        ("#ddom-decision-panel/parameter-decision", "ddom-parameter-decision-view"),
        ("#ddom-decision-panel/temporary-adjustment", "ddom-temporary-adjustment-view"),
        ("#ddom-decision-panel/change-records", "ddom-change-records-view"),
        ("#coordination-panel", "coordination-panel"),
        ("#coordination-panel/issue-list", "coordination-issue-list-view"),
        ("#coordination-panel/action-tracking", "coordination-action-tracking-view"),
        ("#coordination-panel/decision-records", "coordination-decision-records-view"),
        ("#coordination-panel/outcome-validation", "coordination-outcome-validation-view"),
        ("#trace-panel", "trace-panel"),
        ("#public-demo-golden-loop-panel", "public-demo-golden-loop-panel")
    };

    AssertEqual(30, routes.Length, "canonical workspace route count");
    AssertEqual(30, routes.Select(item => item.TargetId).Distinct(StringComparer.Ordinal).Count(), "canonical workspace target uniqueness");
    foreach (var route in routes)
    {
        AssertEqual(1, CountExactOccurrences(page, $" id=\"{route.TargetId}\""), $"target {route.TargetId} should have one DOM ID");
        AssertTrue(script.Contains($"\"{route.Hash}\"", StringComparison.Ordinal), $"route registry should contain {route.Hash}");
        AssertTrue(script.Contains($"targetId: \"{route.TargetId}\"", StringComparison.Ordinal), $"route registry should target {route.TargetId}");

        var openingTag = OpeningTagContaining(page, $" id=\"{route.TargetId}\"");
        AssertTrue(openingTag.Contains(" hidden", StringComparison.Ordinal), $"route target {route.TargetId} should be hidden before route application");
        if (route.TargetId is not "trace-panel" and not "public-demo-golden-loop-panel")
        {
            AssertTrue(openingTag.Contains("workspace-route-view", StringComparison.Ordinal), $"business target {route.TargetId} should be a route view");
            AssertTrue(openingTag.Contains($"data-workspace-route=\"{route.Hash}\"", StringComparison.Ordinal), $"business target {route.TargetId} should expose its canonical route");
        }
        else
        {
            AssertTrue(!openingTag.Contains("workspace-route-view", StringComparison.Ordinal) && !openingTag.Contains("data-workspace-route", StringComparison.Ordinal), $"protected target {route.TargetId} should keep its existing route-neutral opening tag");
        }
    }

    AssertTrue(page.Contains("<section id=\"saved-scenarios-panel\" class=\"workspace-route-host\" hidden>", StringComparison.Ordinal), "trace should remain inside its statically declared hidden host");
    var hostStart = page.IndexOf("<section id=\"saved-scenarios-panel\" class=\"workspace-route-host\" hidden>", StringComparison.Ordinal);
    var traceStart = page.IndexOf("<section id=\"trace-panel\" class=\"schedule-panel\" data-tab-panel hidden>", StringComparison.Ordinal);
    AssertTrue(hostStart >= 0 && traceStart > hostStart, "trace host should open immediately before the protected trace section");
    var hostPrefix = page.Substring(hostStart, traceStart - hostStart);
    foreach (var movedTarget in new[] { "buffer-trend-panel", "rccp-panel", "projected-supply-panel", "variance-panel" })
    {
        AssertTrue(!hostPrefix.Contains($"id=\"{movedTarget}\"", StringComparison.Ordinal), $"{movedTarget} should be a top-level route view rather than a trace host child");
    }

    var applyRouteBody = SourceFunctionBody(script, "applyWorkspaceRoute");
    var closeFocusedPanel = applyRouteBody.IndexOf("closeFocusedPanel()", StringComparison.Ordinal);
    var closeWorkspaceDrawer = applyRouteBody.IndexOf("closeWorkspaceDrawer()", StringComparison.Ordinal);
    var hideTargets = applyRouteBody.IndexOf("workspaceTargetIds.forEach", StringComparison.Ordinal);
    var clearNavigation = applyRouteBody.IndexOf("setActiveWorkspaceNavigation(null)", StringComparison.Ordinal);
    var hideHosts = applyRouteBody.IndexOf("querySelectorAll(\".workspace-route-host\")", StringComparison.Ordinal);
    var showRequiredHost = applyRouteBody.IndexOf("requiredHost.hidden = false", StringComparison.Ordinal);
    var showTarget = applyRouteBody.IndexOf("target.hidden = false", StringComparison.Ordinal);
    AssertTrue(closeFocusedPanel >= 0 && closeWorkspaceDrawer > closeFocusedPanel && hideTargets > closeWorkspaceDrawer, "route application should restore the focused panel and close the detail drawer before switching targets");
    AssertTrue(hideTargets >= 0 && clearNavigation > hideTargets && hideHosts > clearNavigation && showRequiredHost > hideHosts && showTarget > showRequiredHost, "route application should hide all targets, clear navigation, hide hosts, then show the required host and selected target");
    AssertTrue(applyRouteBody.Contains("target.hidden = true", StringComparison.Ordinal), "route application should explicitly hide every canonical target");
    AssertTrue(applyRouteBody.Contains("host.hidden = true", StringComparison.Ordinal), "route application should hide all route hosts first");

    var showContentBody = SourceFunctionBody(script, "showWorkspaceContent");
    AssertTrue(showContentBody.Contains("applyWorkspaceRoute", StringComparison.Ordinal), "workspace loading should reapply only the current route");
    AssertTrue(!showContentBody.Contains(".workspace-section", StringComparison.Ordinal), "workspace loading should not reveal all workspace sections");
}

static void TestHistoryReviewExposesSelectableVisualizationWorkspaces()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var css = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "css", "site.css"));
    var historyStart = page.IndexOf("id=\"history-review-panel\"", StringComparison.Ordinal);
    var traceStart = page.IndexOf("id=\"trace-panel\"", historyStart, StringComparison.Ordinal);

    AssertTrue(historyStart >= 0 && traceStart > historyStart, "history workspaces should remain before the protected trace page");
    AssertTrue(page.Contains("经营结果、缓冲表现、定容追溯和能力约束分别查看", StringComparison.Ordinal),
        "history stage guidance should name all four child views");
    foreach (var historyTargetId in new[]
    {
        "history-buffer-overview",
        "history-inventory-control-point-options",
        "history-inventory-sku-options",
        "history-inventory-chart",
        "history-time-buffer-options",
        "history-time-buffer-chart",
        "history-sizing-trace-view",
        "history-sizing-control-point-options",
        "history-sizing-sku-options",
        "history-sizing-snapshot-options",
        "history-ddmrp-input-summary",
        "history-ddmrp-sizing-body",
        "history-ddmrp-zone-chart",
        "history-standard-ddmrp-input-summary",
        "history-standard-ddmrp-zone-chart",
        "history-capacity-resource-options",
        "history-capacity-buffer-chart",
        "buffer-volatility-chart"
    })
    {
        AssertEqual(1, CountExactOccurrences(page, $" id=\"{historyTargetId}\""), $"history target {historyTargetId} should have one DOM ID");
        var targetPosition = page.IndexOf($" id=\"{historyTargetId}\"", historyStart, StringComparison.Ordinal);
        AssertTrue(targetPosition > historyStart && targetPosition < traceStart, $"history target {historyTargetId} should precede protected trace markup");
    }

    AssertEqual(4, CountExactOccurrences(page, "data-history-range-months=\"6\""), "each history view should expose the six-month range");
    AssertEqual(4, CountExactOccurrences(page, "data-history-range-months=\"12\""), "each history view should expose the twelve-month range");
    var historySelectionStart = css.IndexOf(".history-selector-group .inventory-option:hover", StringComparison.Ordinal);
    var historySelectionEnd = historySelectionStart < 0 ? -1 : css.IndexOf('}', historySelectionStart);
    AssertTrue(historySelectionStart >= 0 && historySelectionEnd > historySelectionStart,
        "history selectors should override the legacy blue inventory selection palette");
    var historySelectionCss = css.Substring(historySelectionStart, historySelectionEnd - historySelectionStart);
    AssertTrue(
        historySelectionCss.Contains("border-color: var(--color-primary)", StringComparison.Ordinal) &&
        historySelectionCss.Contains("background: var(--color-primary-soft)", StringComparison.Ordinal) &&
        historySelectionCss.Contains("color: var(--color-primary-strong)", StringComparison.Ordinal),
        "history selector hover and selected states should use the green navigation palette");
}

static void TestHistoryReviewRetainsRangeAndSelectionState()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    foreach (var stateField in new[]
    {
        "historyTrendMonths: 6",
        "historyRequestGeneration: 0",
        "workspaceErrorSource: null",
        "historySelection:",
        "inventoryControlPoint: null",
        "inventorySku: null",
        "timeBufferId: null",
        "sizingControlPoint: null",
        "sizingSku: null",
        "sizingSnapshotId: null",
        "capacityResourceCode: null"
    })
    {
        AssertTrue(script.Contains(stateField, StringComparison.Ordinal), $"history selection state should include {stateField}");
    }
    AssertTrue(script.Contains("state.historyTrendMonths = trendMonths", StringComparison.Ordinal), "history load should retain the selected range");
    AssertTrue(script.Contains("syncHistorySelectionState(history)", StringComparison.Ordinal), "history load should normalize selectable objects");
    AssertTrue(script.Contains("renderHistoryWorkspaceOptions(history)", StringComparison.Ordinal), "history load should populate selector containers");
    AssertTrue(script.Contains("closest(\"[data-history-range-months]\")", StringComparison.Ordinal), "history range controls should use one repeated-button handler");
    var loadWorkspaceBody = SourceFunctionBody(script, "loadWorkspace");
    AssertTrue(loadWorkspaceBody.Contains("loadHistoryReview()", StringComparison.Ordinal), "workspace refresh should preserve the selected history range");
    AssertTrue(!loadWorkspaceBody.Contains("loadHistoryReview(6)", StringComparison.Ordinal), "workspace refresh should not reset history to six months");

    var staleRequestBody = SourceFunctionBody(script, "isStaleHistoryRequest");
    AssertTrue(staleRequestBody.Contains("return requestGeneration !== currentGeneration", StringComparison.Ordinal),
        "history generation comparison should remain a pure helper shared by success and rejection paths");

    var loadHistoryBody = SourceFunctionBody(script, "loadHistoryReview");
    var generationStart = loadHistoryBody.IndexOf("const requestGeneration = ++state.historyRequestGeneration", StringComparison.Ordinal);
    var tryStart = loadHistoryBody.IndexOf("try {", generationStart, StringComparison.Ordinal);
    var responseFetch = loadHistoryBody.IndexOf("const response = await fetch", StringComparison.Ordinal);
    var responseStatus = loadHistoryBody.IndexOf("if (!response.ok)", StringComparison.Ordinal);
    var responsePayload = loadHistoryBody.IndexOf("const history = await response.json()", StringComparison.Ordinal);
    var successStaleGuard = loadHistoryBody.IndexOf("if (isStaleHistoryRequest(requestGeneration, state.historyRequestGeneration)) return", responsePayload, StringComparison.Ordinal);
    var rangeCommit = loadHistoryBody.IndexOf("state.historyTrendMonths = trendMonths", StringComparison.Ordinal);
    var clearHistoryError = loadHistoryBody.IndexOf("clearWorkspaceError(\"history-review\")", StringComparison.Ordinal);
    var renderCommit = loadHistoryBody.IndexOf("renderHistoryReview(history)", StringComparison.Ordinal);
    var catchStart = loadHistoryBody.IndexOf("catch (error)", renderCommit, StringComparison.Ordinal);
    var rejectionStaleGuard = loadHistoryBody.IndexOf("if (isStaleHistoryRequest(requestGeneration, state.historyRequestGeneration)) return", catchStart, StringComparison.Ordinal);
    var currentRethrow = loadHistoryBody.IndexOf("throw error", rejectionStaleGuard, StringComparison.Ordinal);
    AssertTrue(generationStart >= 0 && tryStart > generationStart && responseFetch > tryStart && responseStatus > responseFetch && responsePayload > responseStatus,
        "history fetch, response validation, and JSON parsing should share one stale-aware try block");
    AssertTrue(successStaleGuard > responsePayload && rangeCommit > successStaleGuard && clearHistoryError > rangeCommit && renderCommit > clearHistoryError,
        "only the latest successful history response should commit range state, clear its own error, and render data");
    AssertTrue(catchStart > renderCommit && rejectionStaleGuard > catchStart && currentRethrow > rejectionStaleGuard,
        "obsolete history rejections should become no-ops while the current generation still rethrows");

    AssertTrue(script.Contains("function showWorkspaceError(error, source = \"workspace\")", StringComparison.Ordinal),
        "workspace errors should record their source");
    var showErrorBody = SourceFunctionBody(script, "showWorkspaceError");
    AssertTrue(showErrorBody.Contains("state.workspaceErrorSource = source", StringComparison.Ordinal),
        "workspace error ownership should be retained for scoped recovery");
    var clearErrorBody = SourceFunctionBody(script, "clearWorkspaceError");
    AssertTrue(clearErrorBody.Contains("if (state.workspaceErrorSource !== source) return", StringComparison.Ordinal),
        "workspace recovery should ignore errors owned by another workspace");
    AssertTrue(clearErrorBody.Contains("state.workspaceErrorSource = null", StringComparison.Ordinal),
        "workspace recovery should release matching error ownership");
    AssertTrue(script.Contains(".catch(error => showWorkspaceError(error, \"history-review\"))", StringComparison.Ordinal),
        "history range failures should display a history-owned error");
}

static void TestHistoryVisualRenderersUseBackendEvidence()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));
    var program = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Program.cs"));
    var fixture = File.ReadAllText(Path.Combine(root, "tests", "AdaptiveSopDdsop.Tests", "Js", "history-buffer-renderers.fixture.mjs"));

    foreach (var stateField in new[]
    {
        "historyTrendMonths: 6",
        "selectedHistoryControlPoint: null",
        "selectedHistoryInventorySku: null",
        "selectedHistorySizingSnapshot: null",
        "selectedHistoryTimeBufferId: null",
        "selectedHistoryCapacityResource: null"
    })
    {
        AssertTrue(script.Contains(stateField, StringComparison.Ordinal), $"history visual state should include {stateField}");
    }

    foreach (var renderer in new[]
    {
        "renderHistoryBufferOverview",
        "renderHistoryInventoryBuffer",
        "renderHistoryDdmrpSizingTrace",
        "renderHistoryStandardDdmrpReference",
        "renderHistoryTimeBuffer",
        "renderHistoryCapacityBuffer",
        "renderHistoryDdmrpZoneSvg",
        "historyControlPointLabel",
        "contiguousEvidenceSegments",
        "buildLinearAreaPath"
    })
    {
        AssertTrue(script.Contains($"function {renderer}(", StringComparison.Ordinal), $"history UI should expose {renderer}");
    }

    var historyReviewBody = SourceFunctionBody(script, "renderHistoryReview");
    foreach (var renderer in new[]
    {
        "renderHistoryBufferOverview(history)",
        "renderHistoryInventoryBuffer(history)",
        "renderHistoryDdmrpSizingTrace(history)",
        "renderHistoryTimeBuffer(history)",
        "renderHistoryCapacityBuffer(history)"
    })
    {
        AssertTrue(historyReviewBody.Contains(renderer, StringComparison.Ordinal), $"history review should invoke {renderer}");
    }

    var inventoryBody = SourceFunctionBody(script, "renderHistoryInventoryBuffer");
    foreach (var backendField in new[] { "point.topOfRed", "point.topOfYellow", "point.topOfGreen", "point.endingOnHand", "point.netFlow" })
    {
        AssertTrue(inventoryBody.Contains(backendField, StringComparison.Ordinal), $"inventory history must read backend {backendField}");
    }
    AssertTrue(inventoryBody.Contains("contiguousEvidenceSegments", StringComparison.Ordinal),
        "inventory history should split paths at evidence gaps");

    var sizingBody = SourceFunctionBody(script, "renderHistoryDdmrpSizingTrace");
    AssertTrue(sizingBody.Contains("item.sizingLines", StringComparison.Ordinal),
        "historical DDMRP table should render backend sizing lines");
    AssertTrue(sizingBody.Contains("renderHistoryDdmrpZoneSvg(item)", StringComparison.Ordinal),
        "historical DDMRP trace should render the backend zone result");
    foreach (var forbidden in new[] { "leadTimeFactor *", "variabilityFactor *", "Math.max(item.minimumOrderQuantity", "\"EvidenceMissing\"", "\"Trace\"" })
    {
        AssertTrue(!sizingBody.Contains(forbidden, StringComparison.Ordinal), $"historical DDMRP renderer must not contain {forbidden}");
    }

    var standardBody = SourceFunctionBody(script, "renderHistoryStandardDdmrpReference");
    foreach (var backendField in new[]
    {
        "history.standardDdmrpReference",
        "item.setting.adu",
        "item.setting.decoupledLeadTimeDays",
        "item.setting.leadTimeFactor",
        "item.setting.variabilityFactor",
        "item.setting.orderCycleDays",
        "item.setting.minimumOrderQuantity",
        "item.sizing.zones.red",
        "item.sizing.zones.yellow",
        "item.sizing.zones.green",
        "item.sizing.greenDriver"
    })
    {
        AssertTrue(standardBody.Contains(backendField, StringComparison.Ordinal), $"standard DDMRP reference must read backend {backendField}");
    }

    var zoneBody = SourceFunctionBody(script, "renderHistoryDdmrpZoneSvg");
    foreach (var backendField in new[] { "item.sizing.zones.red", "item.sizing.zones.yellow", "item.sizing.zones.green", "item.averageOnHand", "item.effectiveFromWeekOffset", "item.effectiveThroughWeekOffset", "item.asOfUtc", "item.sizing.greenDriver" })
    {
        AssertTrue(zoneBody.Contains(backendField, StringComparison.Ordinal), $"zone SVG must read backend {backendField}");
    }

    var timeBody = SourceFunctionBody(script, "renderHistoryTimeBuffer");
    foreach (var backendField in new[] { "point.earlyCount", "point.greenCount", "point.yellowCount", "point.redCount", "point.lateCount", "point.abnormalCost" })
    {
        AssertTrue(timeBody.Contains(backendField, StringComparison.Ordinal), $"time-buffer history must read backend {backendField}");
    }
    AssertTrue(timeBody.Contains("contiguousEvidenceSegments", StringComparison.Ordinal),
        "time-buffer cost line should split at evidence gaps");

    var capacityBody = SourceFunctionBody(script, "renderHistoryCapacityBuffer");
    foreach (var backendField in new[] { "point.committedLoad", "point.theoreticalCapacity", "point.standardCapacity", "point.demonstratedCapacity", "point.plannedAvailableCapacity", "point.protectionStart" })
    {
        AssertTrue(capacityBody.Contains(backendField, StringComparison.Ordinal), $"capacity history must read backend {backendField}");
    }
    AssertTrue(capacityBody.Contains("item.relationshipRole === \"UpstreamProtection\"", StringComparison.Ordinal),
        "only upstream protection resources should show protective consumption");
    AssertTrue(capacityBody.Contains("CCR 利用率参照", StringComparison.Ordinal),
        "CCR utilization history should be labelled as a reference");

    foreach (var selector in new[]
    {
        "history-control-point",
        "history-inventory-sku",
        "history-sizing-snapshot",
        "history-time-buffer-id",
        "history-capacity-resource"
    })
    {
        AssertTrue(script.Contains(selector, StringComparison.Ordinal), $"history UI should expose {selector} selectors");
    }
    foreach (var selectionBehavior in new[]
    {
        "state.selectedHistoryControlPoint = controlPoint.dataset.historyControlPoint",
        "state.selectedHistoryInventorySku = inventorySku.dataset.historyInventorySku",
        "state.selectedHistorySizingSnapshot = sizingSnapshot.dataset.historySizingSnapshot",
        "state.selectedHistoryTimeBufferId = timeBuffer.dataset.historyTimeBufferId",
        "state.selectedHistoryCapacityResource = capacityResource.dataset.historyCapacityResource",
        "renderHistoryInventoryBuffer(state.historyReview)",
        "renderHistoryDdmrpSizingTrace(state.historyReview)",
        "renderHistoryTimeBuffer(state.historyReview)",
        "renderHistoryCapacityBuffer(state.historyReview)"
    })
    {
        AssertTrue(script.Contains(selectionBehavior, StringComparison.Ordinal), $"history selector behavior should include {selectionBehavior}");
    }
    AssertTrue(script.Contains("item.resourceCode === \"RES-AIT\" && item.relationshipRole === \"UpstreamProtection\"", StringComparison.Ordinal),
        "AIT upstream protection should be the default capacity history resource");

    var snapshotDetailBody = SourceFunctionBody(script, "openBaselineSnapshotDetail");
    AssertTrue(snapshotDetailBody.Contains("/api/current-baselines/${snapshotId}", StringComparison.Ordinal),
        "baseline details should read the selected frozen snapshot endpoint");
    AssertTrue(snapshotDetailBody.Contains("旧版本缺少提前期因子；该快照保持只读，不能用于重算", StringComparison.Ordinal),
        "legacy snapshot details should expose the exact missing-evidence warning");
    AssertTrue(!snapshotDetailBody.Contains("state.currentBaselineCandidate", StringComparison.Ordinal),
        "legacy snapshot details must not substitute the current candidate");
    AssertTrue(program.Contains("app.MapGet(\"/api/current-baselines/{snapshotId}\"", StringComparison.Ordinal),
        "baseline details should expose the selected frozen snapshot endpoint");
    AssertTrue(fixture.Contains("export async function runHistoryBufferRendererFixtures", StringComparison.Ordinal)
        && fixture.Contains("new vm.Script(source", StringComparison.Ordinal)
        && fixture.Contains("renderHistoryReview(__historyFixture)", StringComparison.Ordinal),
        "runtime fixture should compile and execute the real app.js renderers against a backend-shaped history DTO");

    var seed = SeedData.Create();
    var history = new HistoryReviewWorkspaceService(
        new SeedHistoryOperatingFactSource(seed),
        new SeedScenarioWorkspaceDataSource(seed)).GetReview(6);
    var alternateSetting = history.StandardDdmrpReference!.Setting with
    {
        Adu = 13m,
        ParameterSnapshotId = "DDMRP-EXAMPLE-V2",
    };
    var alternateSizing = DdmrpCalculator.CalculateSizing(alternateSetting);
    var alternateHistory = history with
    {
        StandardDdmrpReference = history.StandardDdmrpReference with
        {
            SnapshotId = alternateSetting.ParameterSnapshotId,
            Setting = alternateSetting,
            Sizing = alternateSizing,
            SizingLines = DdmrpSizingExplanation.Build(alternateSizing),
        },
    };
    RunHistoryBufferRendererFixture(root, history, alternateHistory);
}

static void TestFutureBufferChartsUseBackendSizingAndSeparateVolatility()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));
    var styles = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "css", "site.css"));

    AssertEqual(1, page.Split("id=\"buffer-trend-chart\"", StringSplitOptions.None).Length - 1,
        "upper buffer chart host count");
    AssertEqual(1, page.Split("id=\"buffer-volatility-chart\"", StringSplitOptions.None).Length - 1,
        "lower volatility chart host count");
    AssertTrue(styles.Contains(".buffer-volatility-svg { min-width: 900px;", StringComparison.Ordinal),
        "upper and lower future charts should use the same minimum plot width");
    AssertTrue(styles.Contains(".history-buffer-svg { display: block; min-width: 760px;", StringComparison.Ordinal),
        "historical SVGs should retain their independent 760px minimum width");

    var trend = new BufferTrendWorkspaceService(new SeedScenarioWorkspaceDataSource(SeedData.Create())).GetBaseline(12);
    RunFutureBufferChartFixture(root, trend);
    AssertTrue(styles.Contains(".buffer-zone-evidence-marker", StringComparison.Ordinal),
        "singleton zone evidence should have an explicit visible marker style");
    AssertTrue(styles.Contains(".buffer-zone-evidence-note", StringComparison.Ordinal),
        "missing zone evidence should have an explicit visible warning style");

    var trendBody = SourceFunctionBody(script, "renderBufferTrendChart");
    AssertTrue(trendBody.Contains("buildMonotoneAreaPath", StringComparison.Ordinal),
        "upper chart should use shape-preserving stacked area paths");
    foreach (var forbidden in new[] { "demand-pulse-bar", "pulseTop", "topOfRed) * 0.5", "topOfRed * 0.5" })
    {
        AssertTrue(!trendBody.Contains(forbidden, StringComparison.Ordinal),
            $"upper chart must not contain {forbidden}");
    }

    var volatilityBody = SourceFunctionBody(script, "renderBufferVolatilityChart");
    AssertTrue(volatilityBody.Contains("item.demand", StringComparison.Ordinal),
        "lower chart should read backend planned demand");
    AssertTrue(volatilityBody.Contains("item.demandSpikeThreshold", StringComparison.Ordinal),
        "lower chart should read the backend spike threshold");
    AssertTrue(volatilityBody.Contains("尖峰阈值证据缺失", StringComparison.Ordinal),
        "lower chart should expose missing threshold evidence");

    var workspaceBody = SourceFunctionBody(script, "renderBufferTrendWorkspace");
    var upperRender = workspaceBody.IndexOf("renderBufferTrendChart(detail)", StringComparison.Ordinal);
    var lowerRender = workspaceBody.IndexOf("renderBufferVolatilityChart(detail)", StringComparison.Ordinal);
    AssertTrue(upperRender >= 0 && lowerRender > upperRender,
        "workspace should render the upper buffer chart before the lower volatility chart");

    var partsBody = SourceFunctionBody(script, "monotonePathParts");
    AssertTrue(partsBody.Contains("Math.sign(delta[index - 1]) !== Math.sign(delta[index])", StringComparison.Ordinal)
        && partsBody.Contains("slopes[index] = 0", StringComparison.Ordinal),
        "monotone helper should clamp direction changes to a zero slope");
    AssertTrue(partsBody.Contains("firstWeight / delta[index - 1] + secondWeight / delta[index]", StringComparison.Ordinal),
        "monotone helper should use a weighted harmonic mean");
    AssertTrue(SourceFunctionBody(script, "buildMonotoneAreaPath").Contains(" Z`", StringComparison.Ordinal),
        "monotone area path should close the stacked band");

    AssertTrue(page.Contains("动态红黄绿缓冲带", StringComparison.Ordinal),
        "upper chart should have a Chinese dynamic-buffer label");
    AssertTrue(page.Contains("需求波动", StringComparison.Ordinal),
        "lower chart should have a Chinese volatility label");
    AssertTrue(!page.Contains("订单尖峰阈值", StringComparison.Ordinal),
        "static page should not retain the old spike-threshold label");
    AssertTrue(!page.Contains("需求脉冲", StringComparison.Ordinal),
        "static page should not retain the old demand-pulse label");
}

static void RunHistoryBufferRendererFixture(
    string root,
    HistoryReviewWorkspace history,
    HistoryReviewWorkspace alternateHistory)
{
    var fixturePath = Path.Combine(root, "tests", "AdaptiveSopDdsop.Tests", "Js", "history-buffer-renderers.fixture.mjs");
    var dtoPath = Path.Combine(Path.GetTempPath(), $"history-review-{Guid.NewGuid():N}.json");
    var alternateDtoPath = Path.Combine(Path.GetTempPath(), $"history-review-alternate-{Guid.NewGuid():N}.json");
    File.WriteAllText(
        dtoPath,
        JsonSerializer.Serialize(history, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    File.WriteAllText(
        alternateDtoPath,
        JsonSerializer.Serialize(alternateHistory, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

    try
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = FindNodeExecutable(),
                WorkingDirectory = root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            },
        };
        process.StartInfo.ArgumentList.Add(fixturePath);
        process.StartInfo.ArgumentList.Add(dtoPath);
        process.StartInfo.ArgumentList.Add(alternateDtoPath);
        process.Start();
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(30_000))
        {
            process.Kill(entireProcessTree: true);
            throw new InvalidOperationException("Node renderer fixture timed out after 30 seconds");
        }
        Task.WaitAll(standardOutput, standardError);
        var output = standardOutput.Result;
        var error = standardError.Result;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Node renderer fixture failed with exit code {process.ExitCode}: {error}{Environment.NewLine}{output}");
        }
        AssertTrue(output.Contains("renderer fixture groups passed", StringComparison.Ordinal),
            $"Node renderer fixture did not report completion: {output}");
        AssertTrue(output.Contains("alternate backend sizing drives standard renderer", StringComparison.Ordinal),
            $"Node renderer fixture did not prove backend-driven alternate sizing: {output}");
    }
    finally
    {
        File.Delete(dtoPath);
        File.Delete(alternateDtoPath);
    }
}

static void RunFutureBufferChartFixture(string root, BufferTrendWorkspaceResult trend)
{
    var fixturePath = Path.Combine(root, "tests", "AdaptiveSopDdsop.Tests", "Js", "future-buffer-charts.fixture.mjs");
    var dtoPath = Path.Combine(Path.GetTempPath(), $"future-buffer-trend-{Guid.NewGuid():N}.json");
    File.WriteAllText(
        dtoPath,
        JsonSerializer.Serialize(trend, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

    try
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = FindNodeExecutable(),
                WorkingDirectory = root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            },
        };
        process.StartInfo.ArgumentList.Add(fixturePath);
        process.StartInfo.ArgumentList.Add(dtoPath);
        process.Start();
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(30_000))
        {
            process.Kill(entireProcessTree: true);
            throw new InvalidOperationException("Node future-buffer chart fixture timed out after 30 seconds");
        }
        Task.WaitAll(standardOutput, standardError);
        var output = standardOutput.Result;
        var error = standardError.Result;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Node future-buffer chart fixture failed with exit code {process.ExitCode}: {error}{Environment.NewLine}{output}");
        }
        AssertTrue(output.Contains("future buffer chart fixture groups passed", StringComparison.Ordinal),
            $"Node future-buffer chart fixture did not report completion: {output}");
    }
    finally
    {
        File.Delete(dtoPath);
    }
}

static void TestHistoryReviewRequestRaceFixtureRunsInStandardHarness()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var fixturePath = Path.Combine(root, "tests", "AdaptiveSopDdsop.Tests", "Js", "history-review-loader-race.fixture.mjs");

    using var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = FindNodeExecutable(),
            WorkingDirectory = root,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        },
    };
    process.StartInfo.ArgumentList.Add(fixturePath);
    process.Start();
    var standardOutput = process.StandardOutput.ReadToEndAsync();
    var standardError = process.StandardError.ReadToEndAsync();
    if (!process.WaitForExit(30_000))
    {
        process.Kill(entireProcessTree: true);
        throw new InvalidOperationException("Node history request race fixture timed out after 30 seconds");
    }
    Task.WaitAll(standardOutput, standardError);
    var output = standardOutput.Result;
    var error = standardError.Result;
    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException(
            $"Node history request race fixture failed with exit code {process.ExitCode}: {error}{Environment.NewLine}{output}");
    }
    AssertTrue(output.Contains("history request race fixture groups passed", StringComparison.Ordinal),
        $"Node history request race fixture did not report completion: {output}");
}

static string FindNodeExecutable()
{
    var executableName = OperatingSystem.IsWindows() ? "node.exe" : "node";
    var candidates = new List<string?>
    {
        Environment.GetEnvironmentVariable("NODE_BINARY"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenAI", "Codex", "bin", executableName),
        OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "nodejs", executableName)
            : null,
    };
    candidates.AddRange((Environment.GetEnvironmentVariable("PATH") ?? "")
        .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
        .Select(path => Path.Combine(Environment.ExpandEnvironmentVariables(path.Trim()), executableName)));

    var executable = candidates
        .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
        .FirstOrDefault(candidate => File.Exists(candidate));
    if (executable is not null)
    {
        return executable;
    }

    var runtimeRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OpenAI",
        "Codex",
        "runtimes",
        "cua_node");
    if (Directory.Exists(runtimeRoot))
    {
        executable = Directory.EnumerateFiles(runtimeRoot, executableName, SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
        if (executable is not null)
        {
            return executable;
        }
    }

    throw new InvalidOperationException(
        "Node.js is required for the historical renderer fixture. Install Node, expose it on PATH, or set NODE_BINARY.");
}

static void TestScenarioRunWorkspaceReplacesTeachingPageShell()
{
    var pagePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml");
    var page = File.ReadAllText(Path.GetFullPath(pagePath));
    var scriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js");
    var script = File.ReadAllText(Path.GetFullPath(scriptPath));

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
    AssertTrue(page.Contains(">击穿分析</a>", StringComparison.Ordinal), "navigation should expose exception analysis as a future-scenario child");
    AssertTrue(page.Contains(">能力缓冲</a>", StringComparison.Ordinal), "navigation should expose capacity protection as a future-scenario child");
    AssertTrue(page.Contains(">供应风险</a>", StringComparison.Ordinal), "navigation should expose supply risk as a future-scenario child");
    AssertTrue(page.Contains(">行动跟踪</a>", StringComparison.Ordinal), "navigation should expose saved scenario evidence under action tracking");
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
    AssertTrue(page.Contains("id=\"network-structure-entry-card\"", StringComparison.Ordinal), "DDS&OP main should expose a minimal network structure scoring entry card");
    AssertTrue(page.Contains("打开独立产品", StringComparison.Ordinal), "network entry card should link to the independent product workspace");
    AssertTrue(!page.Contains("从 DDS&OP 传入", StringComparison.Ordinal) && !page.Contains("网络评分返回", StringComparison.Ordinal), "network entry card should remain opaque inside DDAE");
    AssertTrue(page.Contains("id=\"public-demo-golden-loop-panel\"", StringComparison.Ordinal), "homepage should expose public demo golden loop panel");
    AssertTrue(page.Contains("公开演示闭环", StringComparison.Ordinal), "homepage should expose public demo golden loop navigation label");
    AssertTrue(page.IndexOf("href=\"#public-demo-golden-loop-panel\"", StringComparison.Ordinal) > page.IndexOf("href=\"#trace-panel\"", StringComparison.Ordinal), "public demo navigation item should be last in the left navigation");
    AssertTrue(page.IndexOf("id=\"public-demo-golden-loop-panel\"", StringComparison.Ordinal) > page.IndexOf("id=\"trace-panel\"", StringComparison.Ordinal), "public demo content section should be last in the page flow");
    AssertTrue(script.IndexOf("\"public-demo-golden-loop-panel\"", StringComparison.Ordinal) > script.IndexOf("\"trace-panel\"", StringComparison.Ordinal), "public demo runtime ordering should be last in the page flow");
    AssertTrue(page.Contains("DemoFixture / ReviewedEvidence / Controlled Contract Golden Loop Demo / MappingConfidence = PublicDemoOnly", StringComparison.Ordinal), "public demo panel should show required evidence labels");
    AssertTrue(page.Contains("id=\"write-public-demo-payload\"", StringComparison.Ordinal), "public demo panel should expose payload handoff button");
    AssertTrue(page.Contains("id=\"public-demo-feedback-body\"", StringComparison.Ordinal), "public demo panel should expose feedback interpretation table");
    AssertTrue(page.Contains("id=\"public-demo-scheduling-governance\"", StringComparison.Ordinal), "public demo panel should expose scheduling governance adapter read model");
    AssertTrue(page.Contains("id=\"public-demo-adapter-metadata-body\"", StringComparison.Ordinal), "public demo panel should expose adapter metadata table");
    AssertTrue(page.Contains("id=\"public-demo-adapter-boundary-list\"", StringComparison.Ordinal), "public demo panel should expose adapter boundary list");
    AssertTrue(page.Contains("排程治理与适配器审计", StringComparison.Ordinal), "public demo panel should expose scheduling governance audit label");
    AssertTrue(page.Contains("非 DDAE 执行权威", StringComparison.Ordinal), "public demo panel should label adapter metadata as non-DDAE-owned");
    AssertTrue(page.Contains("id=\"public-demo-business-user-view\"", StringComparison.Ordinal), "public demo panel should expose bottom business user demo view");
    AssertTrue(page.Contains("业务用户演示视图", StringComparison.Ordinal), "public demo panel should label the business user view");
    AssertTrue(page.Contains("从 DDS&OP 治理到 SDBR 评审反馈", StringComparison.Ordinal), "public demo business view should explain governance to feedback flow");
    AssertTrue(page.Contains("id=\"product-demo-profile-summary\"", StringComparison.Ordinal), "public demo panel should expose AdventureWorks ProductDemo profile summary");
    AssertTrue(page.Contains("id=\"product-demo-ddae-authority-body\"", StringComparison.Ordinal), "public demo panel should expose DDAE DemoAuthority rows");
    AssertTrue(page.Contains("id=\"product-demo-panel-policy-body\"", StringComparison.Ordinal), "public demo panel should expose ProductDemo panel policy rows");
    AssertTrue(page.Contains("id=\"product-demo-validation-list\"", StringComparison.Ordinal), "public demo panel should expose ProductDemo validation list");
    AssertTrue(page.Contains("id=\"product-demo-scope-summary\"", StringComparison.Ordinal), "public demo panel should explain ProductDemo coverage scope");
    AssertTrue(page.Contains("ADVENTUREWORKS_PRODUCT_DEMO_V1 档案", StringComparison.Ordinal), "public demo panel should expose AdventureWorks ProductDemo label");
    AssertTrue(page.Contains("不回退 DDAE_CORE_SAMPLE", StringComparison.Ordinal), "public demo panel should explain fallback is blocked");
    AssertTrue(!page.Contains("id=\"optimization-panel\"", StringComparison.Ordinal), "old scenario optimization panel should be removed from DDS&OP main");
    AssertTrue(!page.Contains("id=\"optimization-solver-select\"", StringComparison.Ordinal), "old solver selector should not remain in scenario run");
    AssertTrue(!page.Contains("id=\"run-optimization\"", StringComparison.Ordinal), "old optimization button should be removed from scenario run");
    AssertTrue(!page.Contains("id=\"optimization-status\"", StringComparison.Ordinal), "old optimization status should be removed from scenario run");
    AssertTrue(!page.Contains("id=\"optimization-recommendation-list\"", StringComparison.Ordinal), "old optimization recommendation list should be removed from scenario run");
    AssertTrue(page.Contains("id=\"multi-scenario-comparison-body\"", StringComparison.Ordinal), "scenario comparison should expose multi-scenario comparison body");
    AssertTrue(page.Contains("id=\"candidate-impact-matrix-body\"", StringComparison.Ordinal), "scenario comparison should expose candidate impact matrix body");
    AssertTrue(page.Contains("多方案比较", StringComparison.Ordinal), "scenario comparison should show multi-scenario comparison label");
    AssertTrue(page.Contains("候选动作影响矩阵", StringComparison.Ordinal), "scenario comparison should show candidate impact matrix label");
    AssertTrue(!page.Contains("生成优化推荐", StringComparison.Ordinal), "scenario run should not show the removed optimization recommendation action");
    AssertTrue(!page.Contains(">自动采纳<", StringComparison.Ordinal), "scenario run should not expose automatic adoption action");
    AssertTrue(!page.Contains(">自动审批<", StringComparison.Ordinal), "scenario run should not expose automatic approval action");
    AssertTrue(!page.Contains(">自动保存<", StringComparison.Ordinal), "scenario run should not expose automatic save action");

    AssertTrue(script.Contains("本次产品演示覆盖范围", StringComparison.Ordinal), "script should render ProductDemo coverage summary");
    AssertTrue(script.Contains("来自公开演示数据包样例", StringComparison.Ordinal), "public demo KPI should display an object from the package sample instead of a legacy fixture mapping");
    AssertTrue(script.Contains("本区尚未接入 AdventureWorks 产品演示数据", StringComparison.Ordinal), "script should replace technical placeholder wording with business wording");
    var cssPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AdaptiveSopDdsop.Web", "wwwroot", "css", "site.css");
    var css = File.ReadAllText(Path.GetFullPath(cssPath));
    AssertTrue(script.Contains("const previewFieldHelp", StringComparison.Ordinal), "script should define preview field help dictionary");
    AssertTrue(script.Contains("const navigationHelp", StringComparison.Ordinal), "script should define navigation help dictionary");
    AssertTrue(script.Contains("/api/adventureworks-product-demo-v1", StringComparison.Ordinal), "script should load AdventureWorks ProductDemo profile endpoint");
    AssertTrue(script.Contains("renderAdventureWorksProductDemo", StringComparison.Ordinal), "script should render AdventureWorks ProductDemo read model");
    AssertTrue(script.Contains("不自动改主设置", StringComparison.Ordinal), "script should explain feedback and network candidates do not mutate master settings");
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
    AssertTrue(script.Contains("管理取舍", StringComparison.Ordinal), "script should expose management trade-off labels");
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
    AssertTrue(script.Contains("applyWorkspaceRoute", StringComparison.Ordinal), "script should switch the workspace through canonical hash routes");
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
    AssertTrue(!script.Contains("/api/scenario-runs/optimize", StringComparison.Ordinal), "script should not call the removed optimization API");
    AssertTrue(!script.Contains("applyOptimizationRecommendation", StringComparison.Ordinal), "script should not keep the removed recommendation apply path");
    AssertTrue(!css.Contains(".optimization-recommendation-list", StringComparison.Ordinal), "CSS should not keep the removed optimization recommendation layout");
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
    var sku = new SkuBufferSetting(
        "SKU-PLAN-001", "Plan Item", "Planning", 100, 5, 1.5m, 3, 700, 10m, 1000,
        LeadTimeFactor: 0.6m,
        ParameterSnapshotId: "SKU-PLAN-001-V1",
        ParameterEvidenceStatus: "Complete");
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
    var sizing = weekOne.Sizing ?? throw new InvalidOperationException("week one should carry period sizing");
    var expectedOrderQuantity = sizing.Zones.TopOfGreen - weekOne.EndNetFlowBeforeReplenishment;

    AssertEqual("Red", weekOne.BufferStatus, "week one projected buffer status");
    AssertEqual(900, weekOne.StartNetFlow, "week one start net flow");
    AssertEqual(300, weekOne.EndNetFlowBeforeReplenishment, "week one end net flow before replenishment");
    AssertEqual(expectedOrderQuantity, order.Quantity, "week one replenishment quantity");
    AssertEqual(
        $"净流动量 {weekOne.EndNetFlowBeforeReplenishment:0} 位于黄区上沿 {sizing.Zones.TopOfYellow:0} 及以下，且本周为订货周期复核点，补货到绿区上沿 {sizing.Zones.TopOfGreen:0}。",
        calculationTrace.Explanation,
        "calculation trace");
}

static void TestTimePhasedBufferProjectionWaitsForOrderCycleReview()
{
    var sku = new SkuBufferSetting(
        "SKU-CYCLE-001", "Cycle Item", "Planning", 100, 5, 1.5m, 14, 700, 10m, 1000,
        LeadTimeFactor: 0.6m,
        ParameterSnapshotId: "SKU-CYCLE-001-V1",
        ParameterEvidenceStatus: "Complete");
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
    var weekThree = run.BufferProjections.Single(point => point.Sku == sku.Sku && point.Week == 3);
    var weekThreeOrder = run.ReplenishmentOrders.Single(order => order.Sku == sku.Sku && order.Week == 3);
    var weekTwoTrace = run.Traces.Single(item => item.Sku == sku.Sku && item.Week == 2);
    var weekTwoSizing = weekTwo.Sizing ?? throw new InvalidOperationException("week two should carry period sizing");
    var weekThreeSizing = weekThree.Sizing ?? throw new InvalidOperationException("week three should carry period sizing");

    AssertEqual("Yellow", weekTwo.BufferStatus, "week two should enter yellow");
    AssertTrue(!run.ReplenishmentOrders.Any(order => order.Sku == sku.Sku && order.Week == 2), "week two should wait for the order cycle review");
    AssertEqual(weekThreeSizing.Zones.TopOfGreen - weekThree.EndNetFlowBeforeReplenishment, weekThreeOrder.Quantity, "week three replenishment quantity");
    AssertEqual(
        $"净流动量 {weekTwo.EndNetFlowBeforeReplenishment:0} 位于黄区上沿 {weekTwoSizing.Zones.TopOfYellow:0} 及以下，但本周不是订货周期复核点，暂不生成补货订单。",
        weekTwoTrace.Explanation,
        "week two trace should explain order cycle waiting");
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
        "current-baseline-kpis",
        "baseline-reference-list",
        "future-assumption-mode",
        "future-assumption-template",
        "future-assumption-entered-by",
        "future-assumption-effective-from",
        "future-assumption-effective-through",
        "future-assumption-evidence",
        "external-time-control-point",
        "external-time-delay-days",
        "future-comparison-save-name",
        "future-comparison-save-created-by",
        "save-future-comparison",
        "future-comparison-save-status",
        "governance-baseline-id",
        "coordination-related-scenario",
        "coordination-related-change",
        "coordination-lineage-list",
        "time-buffer-evidence-chip",
        "time-buffer-kpis",
        "time-buffer-summary-body",
        "time-buffer-weekly-grid",
        "trace-panel"
    };

    foreach (var id in requiredIds)
    {
    AssertTrue(page.Contains($"id=\"{id}\"", StringComparison.Ordinal), $"page should expose {id}");
    }

    AssertTrue(page.Contains("缓冲 / 库存趋势", StringComparison.Ordinal), "page should expose graphical buffer trend label");
    AssertTrue(page.Contains("库存选项", StringComparison.Ordinal), "page should expose left-side inventory options");
    AssertTrue(page.Contains("动态红黄绿缓冲带", StringComparison.Ordinal), "page should expose dynamic mountain-style buffer bands");
    AssertTrue(page.Contains("净流动量位置", StringComparison.Ordinal), "page should expose net flow position label");
    AssertTrue(page.Contains("预计库存水位", StringComparison.Ordinal), "page should expose projected inventory level label");
    AssertTrue(page.Contains("目标库存", StringComparison.Ordinal), "page should expose target inventory label");
    AssertTrue(page.Contains("时间相位 ADU", StringComparison.Ordinal), "page should expose time-phased ADU label");
    AssertTrue(page.Contains("需求波动", StringComparison.Ordinal), "page should expose the independent demand-volatility label");
    AssertTrue(page.Contains("单 SKU 仿真工作台", StringComparison.Ordinal), "page should expose single SKU simulation workbench");
    AssertTrue(page.Contains("活动列表", StringComparison.Ordinal), "page should expose SKU activity list");
    AssertTrue(page.Contains("缓冲定容", StringComparison.Ordinal), "page should expose Chinese buffer sizing label");
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
    var sku = new SkuBufferSetting(
        "SKU-PEAK-001", "Peak Item", "Planning", 100, 5, 1.5m, 3, 700, 10m, 1000,
        LeadTimeFactor: 0.6m,
        ParameterSnapshotId: "SKU-PEAK-001-V1",
        ParameterEvidenceStatus: "Complete");
    var baseSizing = DdmrpCalculator.CalculateSizing(sku);
    var position = new InventoryPosition(sku.Sku, baseSizing.Zones.TopOfGreen, 0, 0);
    var demand = new[]
    {
        new WeeklyDemand(sku.Sku, 1, 0),
        new WeeklyDemand(sku.Sku, 2, 0),
        new WeeklyDemand(sku.Sku, 3, 1000),
        new WeeklyDemand(sku.Sku, 4, 1000),
    };
    var peakDemand = demand.Single(item => item.Week == 3).BaselineDemand;
    var peakSizing = DdmrpCalculator.CalculateSizing(sku, peakDemand / 5m);
    var campaign = new PrebuildCampaign(
        "PB-001",
        sku.Sku,
        1,
        3,
        4,
        peakSizing.Zones.TopOfGreen + peakDemand - baseSizing.Zones.TopOfGreen);

    var run = DemandDrivenPlanningEngine.ProjectBuffers(
        new[] { sku },
        new[] { position },
        demand,
        horizonWeeks: 4,
        prebuildCampaigns: new[] { campaign });

    var prebuildOrder = run.ReplenishmentOrders.Single(order => order.Sku == sku.Sku && order.Week == 1);
    var weekOne = run.BufferProjections.Single(point => point.Sku == sku.Sku && point.Week == 1);
    var weekThree = run.BufferProjections.Single(point => point.Sku == sku.Sku && point.Week == 3);
    var weekThreeTrace = run.Traces.Single(item => item.Sku == sku.Sku && item.Week == 3);
    var weekOneSizing = weekOne.Sizing ?? throw new InvalidOperationException("week one should carry period sizing");
    var weekThreeSizing = weekThree.Sizing ?? throw new InvalidOperationException("week three should carry period sizing");

    AssertEqual("PrebuildCampaign", prebuildOrder.Trigger, "prebuild trigger");
    AssertEqual(campaign.Quantity, prebuildOrder.Quantity, "prebuild quantity");
    AssertEqual(weekOneSizing.Zones.TopOfGreen + campaign.Quantity, weekOne.StartNetFlow, "prebuild should raise week one start net flow");
    AssertEqual(weekOne.StartNetFlow, weekThree.StartNetFlow, "prebuild should protect the start of the peak week");
    AssertEqual(weekThreeSizing.Zones.TopOfGreen + weekThree.Demand, weekThree.StartNetFlow, "prebuild should cover peak demand above the period target");
    AssertEqual(weekThreeSizing.Zones.TopOfGreen, weekThree.EndNetFlowBeforeReplenishment, "week three protected net flow");
    AssertTrue(!run.ReplenishmentOrders.Any(order => order.Sku == sku.Sku && order.Week == 3), "prebuild should prevent peak-week replenishment");
    AssertEqual(
        $"净流动量 {weekThree.EndNetFlowBeforeReplenishment:0} 高于黄区上沿 {weekThreeSizing.Zones.TopOfYellow:0}，不生成补货订单。",
        weekThreeTrace.Explanation,
        "peak-week calculation trace");
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

static void TestScenarioRunWorkspaceScriptFetchesWorkspaceData()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    AssertTrue(script.Contains("/api/scenario-workspace-data?horizonWeeks=12", StringComparison.Ordinal), "script should fetch scenario workspace data");
    AssertTrue(script.Contains("/api/product-family-dashboard?horizonWeeks=12", StringComparison.Ordinal), "script should fetch product family dashboard data");
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
    AssertTrue(!script.Contains("IntersectionObserver", StringComparison.Ordinal), "left navigation should no longer infer state from page scrolling");
    AssertTrue(script.Contains("handleWorkspaceHashChange", StringComparison.Ordinal), "left navigation should follow explicit hash route state");
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
    AssertTrue(data.DdmrpParameters.All(item => item.LeadTimeFactor is > 0m and <= 1m), "all DDMRP profiles should expose a legal lead-time factor");
    AssertTrue(data.DdmrpParameters.All(item => !string.IsNullOrWhiteSpace(item.ParameterSnapshotId)), "all DDMRP profiles should expose a parameter snapshot");
    AssertTrue(data.DdmrpParameters.All(item => item.EvidenceStatus == "Complete"), "all DDMRP profiles should expose complete sizing evidence");
    AssertTrue(data.DdmrpParameters.All(item => item.Sizing is not null), "all DDMRP profiles should expose the unified sizing result");
    AssertTrue(data.DdmrpParameters.All(item => item.SizingLines is { Count: 11 }), "all DDMRP profiles should expose the shared eleven-line sizing explanation");
    AssertTrue(data.DdmrpParameters.All(item => item.EffectiveFromWeek >= 1 && item.EffectiveThroughWeek >= item.EffectiveFromWeek), "all DDMRP profiles should expose effective window");
    AssertTrue(data.DdmrpParameters.Any(item => item.DemandAdjustmentFactor != 1m || item.ZoneAdjustmentFactor != 1m), "seed data should include non-default DAF or zone adjustments");

    foreach (var sku in data.Skus)
    {
        var sizing = DdmrpCalculator.CalculateSizing(sku);
        var profile = data.DdmrpParameters.Single(item => item.Sku == sku.Sku);
        AssertEqual(sku.LeadTimeFactor, profile.LeadTimeFactor, $"lead-time factor for {sku.Sku}");
        AssertEqual(sku.ParameterSnapshotId, profile.ParameterSnapshotId, $"parameter snapshot for {sku.Sku}");
        AssertEqual(sizing.EvidenceStatus, profile.EvidenceStatus, $"evidence status for {sku.Sku}");
        AssertEqual(sizing, profile.Sizing!, $"unified sizing result for {sku.Sku}");
        AssertEqual(sizing.Zones.TopOfRed, profile.TopOfRed, $"top of red for {sku.Sku}");
        AssertEqual(sizing.Zones.TopOfYellow, profile.TopOfYellow, $"top of yellow for {sku.Sku}");
        AssertEqual(sizing.Zones.TopOfGreen, profile.TopOfGreen, $"top of green for {sku.Sku}");

        var sizingLines = profile.SizingLines!;
        var redBaseLine = sizingLines.Single(item => item.Component == "红区基础");
        var greenLine = sizingLines.Single(item => item.Component == "绿区");
        var totalLine = sizingLines.Single(item => item.Component == "总缓冲");
        AssertEqual("提前期需求 × 提前期因子", redBaseLine.Formula, $"red-base formula for {sku.Sku}");
        AssertEqual(decimal.Round(sizing.RedBase, 1), redBaseLine.Value, $"red-base explanation value for {sku.Sku}");
        AssertEqual("max（三个候选）× 区域调整", greenLine.Formula, $"green formula for {sku.Sku}");
        AssertEqual(sizing.Zones.Green, greenLine.Value, $"green explanation value for {sku.Sku}");
        AssertEqual(sizing.Zones.TopOfGreen, totalLine.Value, $"total-buffer explanation value for {sku.Sku}");
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
    AssertTrue(result.Trace.Any(item => item.Message.Contains("流速优先", StringComparison.Ordinal)), "preview trace should show the adoption constraint mode in Chinese");
    AssertTrue(!result.Trace.Any(item => item.Message.Contains("FlowFirst", StringComparison.Ordinal)), "preview trace should not expose the internal adoption constraint code");
    AssertTrue(result.Trace.Any(item => item.Message.Contains("需求驱动计划引擎", StringComparison.Ordinal)), "preview should trace shared engine use in Chinese");

    var defaultResult = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()))
        .Preview(new ScenarioRunPreviewRequest(12));
    AssertTrue(defaultResult.Trace.Any(item => item.Message.Contains("综合平衡", StringComparison.Ordinal)), "default preview trace should localize the balanced adoption mode");
    AssertTrue(!defaultResult.Trace.Any(item => item.Message.Contains("Balanced", StringComparison.Ordinal)), "default preview trace should not expose the internal balanced code");
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
    AssertTrue(page.Contains("id=\"scenario-comparison\"", StringComparison.Ordinal), "page should expose saved scenarios under stage 03 plan comparison");
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
        AssertTrue(proposals.Proposals.Any(item => item.Rationale.Any(reason => reason.Contains("场景预览", StringComparison.Ordinal))), "proposals should explain preview origin in Chinese");
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

        var saved = service.SaveChange(new MasterSettingChangeSaveRequest(
            "计划员",
            proposal with { SourceBaselineId = "BASELINE-MANUAL-AUDIT", CreationMethod = "Manual" }));

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

static void TestFiveStageDetailsExposeReadableBidirectionalLineageNavigation()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    var scenarioViewStart = page.IndexOf("id=\"scenario-comparison\"", StringComparison.Ordinal);
    var scenarioViewEnd = page.IndexOf("id=\"coordination-action-tracking-view\"", scenarioViewStart, StringComparison.Ordinal);
    var scenarioDetail = page.IndexOf("id=\"scenario-lineage-list\"", scenarioViewStart, StringComparison.Ordinal);
    AssertTrue(scenarioViewStart >= 0 && scenarioDetail > scenarioViewStart && scenarioDetail < scenarioViewEnd,
        "saved scenario detail and lineage should belong to stage 03 plan comparison");
    AssertTrue(page.Contains("id=\"scenario-detail-summary\"", StringComparison.Ordinal),
        "scenario detail should expose a readable summary");
    AssertTrue(page.Contains("id=\"master-setting-lineage-list\"", StringComparison.Ordinal),
        "master-setting detail should expose linked scenarios and actions");
    AssertTrue(page.Contains("id=\"coordination-lineage-list\"", StringComparison.Ordinal),
        "coordination detail should expose linked scenario and change records");

    foreach (var functionName in new[]
    {
        "renderScenarioLineage",
        "renderMasterSettingLineage",
        "renderCoordinationLineage",
        "openScenarioLineageDetail",
        "openMasterSettingLineageDetail",
        "openCoordinationLineageDetail"
    })
    {
        AssertTrue(script.Contains($"function {functionName}(", StringComparison.Ordinal)
            || script.Contains($"async function {functionName}(", StringComparison.Ordinal),
            $"script should implement {functionName}");
    }

    AssertTrue(script.Contains("/api/master-settings/changes?limit=50&sourceScenarioRunId=", StringComparison.Ordinal),
        "scenario detail should query changes by source scenario run");
    AssertTrue(script.Contains("/api/coordination-items?limit=50&relatedScenarioRunId=", StringComparison.Ordinal),
        "scenario detail should query actions by related scenario run");
    AssertTrue(script.Contains("/api/coordination-items?limit=50&relatedMasterSettingChangeId=", StringComparison.Ordinal),
        "change detail should query actions by related master-setting change");
    AssertTrue(script.Contains("data-lineage-scenario-run-id", StringComparison.Ordinal),
        "lineage should expose scenario jump controls");
    AssertTrue(script.Contains("data-lineage-master-change-id", StringComparison.Ordinal),
        "lineage should expose master-setting jump controls");
    AssertTrue(script.Contains("data-lineage-coordination-item-id", StringComparison.Ordinal),
        "lineage should expose coordination jump controls");
    AssertTrue(script.Contains("navigateWorkspace(\"future-scenario-panel\", \"plan-comparison\", false)", StringComparison.Ordinal),
        "scenario lineage jump should switch to the stage 03 detail view");
    AssertTrue(script.Contains("navigateWorkspace(\"ddom-decision-panel\", \"change-records\", false)", StringComparison.Ordinal),
        "change lineage jump should switch to the stage 04 detail view");
    AssertTrue(script.Contains("navigateWorkspace(\"coordination-panel\", \"action-tracking\", false)", StringComparison.Ordinal),
        "action lineage jump should switch to the stage 05 detail view");
    foreach (var readableField in new[] { "runNumber", "changeNumber", "itemNumber", ".name", ".target", ".title" })
    {
        AssertTrue(script.Contains(readableField, StringComparison.Ordinal),
            $"lineage rendering should include readable field {readableField}");
    }
}

static void TestBusinessViewsTranslateInternalCodesWithoutMojibake()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));
    var businessStart = page.IndexOf("<section id=\"history-review-panel\"", StringComparison.Ordinal);
    var protectedStart = page.IndexOf("id=\"saved-scenarios-panel\"", businessStart, StringComparison.Ordinal);
    AssertTrue(businessStart >= 0 && protectedStart > businessStart, "business region should precede protected validation pages");
    var businessPage = page.Substring(businessStart, protectedStart - businessStart);
    var actualHistory = new HistoryReviewWorkspaceService(
        new SeedHistoryOperatingFactSource(),
        new SeedScenarioWorkspaceDataSource(SeedData.Create())).GetReview(6);
    AssertTrue(actualHistory.EvidenceLabel.Contains("DemoFixture /", StringComparison.Ordinal)
        && actualHistory.EvidenceLabel.Contains("SourceAuthority=", StringComparison.Ordinal)
        && actualHistory.EvidenceLabel.Contains("AsOf=", StringComparison.Ordinal),
        "real history evidence should exercise the composite internal label shape handled by the UI");

    foreach (var helper in new[] { "baselineSourceLabel", "baselineActorLabel", "freshnessLabel", "completenessLabel", "baselineStatusLabel", "coordinationStatusLabel", "breachScopeLabel", "metricOrEvidenceMissing" })
    {
        AssertTrue(script.Contains($"function {helper}(", StringComparison.Ordinal), $"business script should centralize {helper}");
    }

    foreach (var mapping in new[]
    {
        "DemoFixture: \"演示数据\"", "Fresh: \"截止时间有效\"", "Complete: \"完整\"", "Frozen: \"已冻结\"",
        "Open: \"待处理\"", "InProgress: \"进行中\"", "Escalated: \"已升级\"", "Completed: \"已完成\"",
        "InventoryBuffer: \"库存缓冲\"", "TimeBuffer: \"时间缓冲\"", "CapacityBuffer: \"能力缓冲\"", "SupplyRisk: \"供应风险\""
    })
    {
        AssertTrue(script.Contains(mapping, StringComparison.Ordinal), $"business translation should contain {mapping}");
    }

    foreach (var forbidden in new[] { "当前占用", "库存口径", "红色供应窗口", "缓冲 sizing", ">Trace<", "??", "\uFFFD", "EvidenceMissing", "UpstreamProtection", "Trace" })
    {
        AssertTrue(!businessPage.Contains(forbidden, StringComparison.Ordinal), $"business page should not expose {forbidden}");
    }
    AssertTrue(businessPage.Contains("在制品", StringComparison.Ordinal), "business page should use the Chinese WIP label");
    AssertTrue(businessPage.Contains("缓冲定容", StringComparison.Ordinal), "business page should use the approved buffer-sizing label");
    AssertTrue(businessPage.Contains("追踪", StringComparison.Ordinal), "business page should use the Chinese trace label");
    AssertTrue(script.Contains("normalized.includes(\"DemoFixture\")", StringComparison.Ordinal), "composite history evidence should translate DemoFixture instead of leaking its internal key");
    AssertTrue(script.Contains("historyEvidenceSummary(history.evidenceLabel)", StringComparison.Ordinal), "history renderer should invoke the composite evidence translator on the real API field");
    AssertTrue(script.Contains("baselineActorLabel(item.createdBy)", StringComparison.Ordinal), "known local smoke actors should be localized only at display time");
    AssertTrue(script.Contains("function masterSettingDisplayValue(changeId, field, value)", StringComparison.Ordinal)
        && script.Contains("历史烟测目标（非业务数据）", StringComparison.Ordinal)
        && script.Contains("masterSettingDisplayValue(summary.changeId, \"auditMessage\"", StringComparison.Ordinal),
        "known legacy DDOM smoke values should be localized only at display time while retaining their audit record");
    AssertTrue(script.Contains("function historyEvidenceSummary(", StringComparison.Ordinal)
        && script.Contains("SourceAuthority=", StringComparison.Ordinal)
        && script.Contains("来源：", StringComparison.Ordinal)
        && script.Contains("截止：", StringComparison.Ordinal),
        "history evidence should preserve source and as-of meaning with Chinese labels");
    foreach (var sectionMapping in new[]
    {
        "\"Time-buffer definitions\": \"时间缓冲定义\"",
        "\"Time-buffer product scopes\": \"时间缓冲产品范围\"",
        "\"Time-buffer control-point progress\": \"时间缓冲控制点进度\""
    })
    {
        AssertTrue(script.Contains(sectionMapping, StringComparison.Ordinal), $"baseline section should translate {sectionMapping}");
    }
    AssertTrue(script.Contains(".replace(/\\bCurrent\\b/g, \"当前\")", StringComparison.Ordinal)
        && script.Contains(".replace(/\\bReviewed\\b/g, \"已评审\")", StringComparison.Ordinal)
        && script.Contains(".replace(/\\bApproved\\b/g, \"已批准\")", StringComparison.Ordinal),
        "business evidence should translate governance evidence states");
    AssertTrue(script.Contains("历史烟测记录已由纠正审计保留", StringComparison.Ordinal), "legacy corrupt audit display should be masked without changing stored data");
    AssertTrue(script.Contains("DataRepairApplied", StringComparison.Ordinal), "repair audit event should have a business label");
    AssertTrue(script.Contains("Governance: \"治理\"", StringComparison.Ordinal)
        && script.Contains("Impact: \"影响\"", StringComparison.Ordinal),
        "governance audit stages should use Chinese business labels");
    AssertTrue(script.Contains("rollbackCondition: \"历史烟测回滚条件\"", StringComparison.Ordinal)
        && script.Contains("masterSettingDisplayValue(summary.changeId, \"rollbackCondition\"", StringComparison.Ordinal),
        "known legacy rollback text should be localized only at display time");
    AssertTrue(script.Contains("item.textContent.replaceAll(\"红色供应窗口\", \"供应能力不足周\")", StringComparison.Ordinal)
        && script.Contains("localizeLegacyTraceWording();", StringComparison.Ordinal),
        "white-box trace should describe supplier-capacity shortage weeks in plain business language without changing the protected renderer");
}

static void TestBusinessViewsLocalizeOrdinaryUnitTokens()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    AssertTrue(script.Contains("function businessUnitLabel(", StringComparison.Ordinal), "business UI should centralize ordinary unit translation");
    foreach (var mapping in new[]
    {
        "units: \"件\"",
        "\"units/week\": \"件/周\"",
        "factor: \"倍\"",
        "days: \"天\""
    })
    {
        AssertTrue(script.Contains(mapping, StringComparison.Ordinal), $"business unit translation should contain {mapping}");
    }

    AssertTrue(
        script.Contains("businessUnitLabel(action.unit)", StringComparison.Ordinal),
        "scenario cards should render localized action units");
    AssertTrue(
        !script.Contains("${number(action.value)} ${action.unit}", StringComparison.Ordinal),
        "scenario cards must not render raw ordinary English units");
}

static void TestRccpPeakLoadUsesReleasePressureWording()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    AssertTrue(page.Contains("补货释放峰值", StringComparison.Ordinal), "RCCP tables should name the metric as replenishment release peak load");
    AssertTrue(
        page.Contains("补货订单释放周压力，不等于设备持续利用率", StringComparison.Ordinal),
        "RCCP view should explain why a release-week peak can exceed sustained utilization");
    AssertTrue(
        script.Contains("补货释放峰值", StringComparison.Ordinal),
        "scenario KPI cards should use replenishment release peak wording");
}

static void TestGeneratedBusinessTextUsesChineseOrdinaryWording()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var relativeFiles = new[]
    {
        Path.Combine("src", "AdaptiveSopDdsop.Web", "Data", "SeedScenarioWorkspaceDataSource.cs"),
        Path.Combine("src", "AdaptiveSopDdsop.Web", "Domain", "BufferTrendWorkspaceService.cs"),
        Path.Combine("src", "AdaptiveSopDdsop.Web", "Domain", "ExceptionWorkspaceService.cs"),
        Path.Combine("src", "AdaptiveSopDdsop.Web", "Domain", "MasterSettingsGovernanceService.cs")
    };
    var generatedTextSources = string.Join('\n', relativeFiles.Select(file => File.ReadAllText(Path.Combine(root, file))));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    foreach (var forbidden in new[]
    {
        "Scenario Preview",
        "Pre-build",
        "缓冲 sizing",
        "Zone Adjustment Factor",
        "订货周期 {policy.OrderCycleDays}d",
        "订货周期 {action.Value:0}d",
        "{action.Value:0} {action.Unit}"
    })
    {
        AssertTrue(
            !generatedTextSources.Contains(forbidden, StringComparison.Ordinal),
            $"generated five-stage business text should not expose {forbidden}");
    }

    foreach (var translation in new[]
    {
        ".replace(/\\bdemonstrated ADU\\b/g, \"经验证 ADU\")",
        ".replace(/\\bZone (?=\\d)/g, \"区域调整因子 \")",
        ".replace(/\\bVF (?=\\d)/g, \"变异因子 \")"
    })
    {
        AssertTrue(script.Contains(translation, StringComparison.Ordinal), $"business renderer should contain {translation}");
    }
    AssertTrue(script.Contains("businessEvidenceLabel(item.aduSource)", StringComparison.Ordinal), "DDMRP source evidence should be localized only at display time");
    AssertTrue(script.Contains("businessEvidenceLabel(item.currentValue)", StringComparison.Ordinal), "current DDOM values should be localized only at display time");
    AssertTrue(
        script.Contains("fillSelect(selectors.risk, \"风险\"", StringComparison.Ordinal) &&
        script.Contains("statusLabel)", StringComparison.Ordinal),
        "risk filter options should render Chinese status labels while retaining internal status values");
    foreach (var stage in new[] { "FrozenBaseline", "ExternalScenario", "ResponseConfiguration", "MasterSettings", "Trace" })
    {
        AssertTrue(script.Contains($"{stage}: \"", StringComparison.Ordinal), $"white-box trace stage {stage} should have a Chinese display mapping");
    }
    AssertTrue(script.Contains(".replace(/\\bBalanced\\b/g, \"综合平衡\")", StringComparison.Ordinal), "white-box trace should localize the default adoption mode");
    AssertTrue(script.Contains(".replace(/\\btrace\\b/gi, \"追踪记录\")", StringComparison.Ordinal), "persisted trace messages should not expose an ordinary English trace token");
    AssertTrue(script.Contains("item.textContent.replaceAll(\"红色供应窗口\", \"供应能力不足周\")", StringComparison.Ordinal), "protected white-box trace should localize the legacy supply-window wording without changing its protected renderer");
}

static void TestFiveStageUiHasNoExternalImportOrProtocolInput()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));
    var businessStart = page.IndexOf("id=\"history-review-panel\"", StringComparison.Ordinal);
    var protectedStart = page.IndexOf("id=\"saved-scenarios-panel\"", businessStart, StringComparison.Ordinal);
    var businessPage = page.Substring(businessStart, protectedStart - businessStart);

    foreach (var forbidden in new[]
    {
        "type=\"file\"", "CSV 导入", "JSON 导入", "external-import", "network-scoring-input", "sdbr-input",
        "SDBR", "id=\"auto-adopt\"", "id=\"auto-approve\"", "id=\"auto-effect\"", "id=\"auto-save\""
    })
    {
        AssertTrue(!businessPage.Contains(forbidden, StringComparison.OrdinalIgnoreCase), $"five-stage business UI should not contain {forbidden}");
    }

    AssertTrue(businessPage.Contains("id=\"future-assumption-mode\"", StringComparison.Ordinal), "scenario source must be chosen explicitly");
    AssertTrue(businessPage.Contains("value=\"Manual\"", StringComparison.Ordinal) && businessPage.Contains("value=\"DemoFixture\"", StringComparison.Ordinal), "only manual and internal demo inputs should be offered");
    AssertTrue(businessPage.Contains("id=\"response-supply-recovery\"", StringComparison.Ordinal), "enterprise response options should include an internal supply response");
    AssertTrue(script.Contains("RESP-SUPPLY-RECOVERY", StringComparison.Ordinal)
        && script.Contains("supplierCapacityLimits", StringComparison.Ordinal),
        "supply response should use the existing internal supplier-capacity parameter set");
    AssertTrue(script.Contains("state.futureComparisonBaseline?.payload?.planningInputs?.supplierCapacityWindows", StringComparison.Ordinal),
        "supply response commitments should come from the selected frozen baseline instead of mutable live workspace data");
    var baselineFetch = script.IndexOf("const baselineResponse = await fetch(`/api/current-baselines/", StringComparison.Ordinal);
    var frozenBaselineAssignment = script.IndexOf("state.futureComparisonBaseline = await baselineResponse.json();", baselineFetch, StringComparison.Ordinal);
    var comparisonRequestBuild = script.IndexOf("const request = buildScenarioComparisonRequest();", frozenBaselineAssignment, StringComparison.Ordinal);
    AssertTrue(baselineFetch >= 0 && frozenBaselineAssignment > baselineFetch && comparisonRequestBuild > frozenBaselineAssignment,
        "scenario comparison must load the selected frozen baseline before building response configurations");
    AssertTrue(script.Contains("/api/scenario-assumptions/templates", StringComparison.Ordinal), "demo assumptions should come from the internal template endpoint");
    AssertTrue(script.Contains("/api/scenario-runs/compare/save", StringComparison.Ordinal), "comparison must be saved explicitly through the internal save endpoint");
    AssertTrue(script.Contains("sourceScenarioRunId: savedRun.runId", StringComparison.Ordinal), "scenario-derived governance must use the real saved run id");
    AssertTrue(!script.Contains("`${state.futureComparisonRequest.externalScenario.scenarioId}/${byId(\"governance-response-id\").value}`", StringComparison.Ordinal), "external scenario and response ids must never be fabricated into a run id");
}

static void TestTimeBufferViewUsesBackendResultsOnly()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));
    foreach (var id in new[] { "time-buffer-panel", "time-buffer-evidence-chip", "time-buffer-kpis", "time-buffer-summary-body", "time-buffer-weekly-grid" })
    {
        AssertTrue(page.Contains($"id=\"{id}\"", StringComparison.Ordinal), $"time-buffer page should expose {id}");
    }

    AssertTrue(script.Contains("function renderTimeBufferView(", StringComparison.Ordinal), "time-buffer page should have a dedicated renderer");
    AssertTrue(script.Contains("renderTimeBufferView(result, state.futureComparisonBaseline)", StringComparison.Ordinal), "future comparison renderer should invoke the time-buffer renderer with frozen-baseline evidence");
    foreach (var backendField in new[] { ".timeBufferProjection", ".breaches", ".maximumPenetrationPercent", ".earliestRedWeek", ".consecutiveRiskWeeks", ".recoveryWeek", ".isUnrecovered" })
    {
        AssertTrue(script.Contains(backendField, StringComparison.Ordinal), $"time-buffer renderer should use backend field {backendField}");
    }
    AssertTrue(script.Contains("planningInputs.timeBuffers", StringComparison.Ordinal), "time-buffer renderer should use frozen baseline protection definitions");
    AssertTrue(!script.Contains("penetrationPercent =", StringComparison.Ordinal) && !script.Contains("delayDays /", StringComparison.Ordinal) && !script.Contains("/ point.bufferDays", StringComparison.Ordinal), "front end must not calculate time-buffer penetration");
    AssertTrue(script.Contains("const evidenceComplete = item.evidenceStatus === \"Complete\"", StringComparison.Ordinal), "time-buffer display should distinguish missing evidence from a complete non-breach");
    AssertTrue(script.Contains("item.evidenceStatus === \"NotApplicable\" ? \"不适用\" : \"证据缺失\"", StringComparison.Ordinal), "time-buffer display should distinguish not applicable from missing evidence");
    AssertTrue(script.Contains("!item.isBreached ? \"不适用\"", StringComparison.Ordinal), "a complete non-breach should show recovery as not applicable");
    AssertTrue(script.Contains("breach.evidenceStatus === \"Complete\" || breach.evidenceStatus === \"NotApplicable\"", StringComparison.Ordinal), "comparison cards should accept a legitimate not-applicable scope without reporting missing evidence");
    AssertTrue(script.Contains("allBreachEvidenceNotApplicable ? \"不适用\"", StringComparison.Ordinal), "comparison cards should label an entirely not-applicable breach set explicitly");
    AssertTrue(script.Contains("const breachEvidenceAvailable = item.evidenceStatus === \"Complete\"", StringComparison.Ordinal), "all future breach rows should branch on backend evidence status");
    AssertTrue(script.Contains("!breachEvidenceAvailable ? unavailableEvidence", StringComparison.Ordinal), "future breach rows must show evidence state before non-breach values");
    AssertTrue(script.Contains("[\"击穿记录\"", StringComparison.Ordinal) && script.Contains("[\"未恢复记录\"", StringComparison.Ordinal), "time-buffer KPIs should summarize unambiguous backend record counts");
    AssertTrue(!script.Contains("const firstBreach =", StringComparison.Ordinal), "time-buffer KPIs must not label the first array item as the global earliest or maximum result");
}

static void TestManualGovernanceChangeRequiresBaselineAndAllowsNoScenario()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-manual-governance-{Guid.NewGuid():N}.db");
    try
    {
        var source = new SeedScenarioWorkspaceDataSource(SeedData.Create());
        var preview = new ScenarioRunPreviewService(source);
        var service = new MasterSettingsGovernanceService(source, preview, databasePath);
        var proposal = service.ProposeFromPreview(new ScenarioRunPreviewRequest(
                12,
                Parameters: new ScenarioRunParameterSet(
                    SkuPolicyOverrides: new[] { new SkuPolicyOverride("AV-FPGA-203", MinimumOrderQuantity: 500) })))
            .Proposals
            .First();

        var missingBaselineRequest = proposal with
        {
            CreationMethod = "Manual",
            SourceBaselineId = null,
            SourceScenarioRunId = null
        };

        var rejected = false;
        try
        {
            service.SaveChange(new MasterSettingChangeSaveRequest("DDS&OP 计划员", missingBaselineRequest));
        }
        catch (ArgumentException)
        {
            rejected = true;
        }
        AssertTrue(rejected, "manual governance change without a source baseline must be rejected");

        var saved = service.SaveChange(new MasterSettingChangeSaveRequest(
            "DDS&OP 计划员",
            proposal with
            {
                CreationMethod = "Manual",
                SourceBaselineId = "BASELINE-MANUAL-001",
                SourceScenarioRunId = null
            }));

        AssertEqual("Proposed", saved.Status, "manual governance change initial status");
        AssertTrue(saved.Summary.SourceScenarioRunId is null, "manual governance change may omit scenario lineage");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestManualGovernanceChangeWithScenarioRequiresValidatedSavedRun()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-manual-run-governance-{Guid.NewGuid():N}.db");
    try
    {
        var validRun = new ScenarioRunSummary(
            "RUN-MANUAL-SAVED", "SR-20260715-0200", "手工变更来源 run", null, "DDS&OP 计划员", "Saved", "NotSubmitted",
            "2026-07-15T08:00:00Z", 12, null, null, 98m, 1m, 1_000_000m, 90m, 0m, 0, 1,
            "BASELINE-MANUAL-RUN", "EXT-MANUAL-RUN", "RESP-MANUAL-RUN");
        var unsavedRun = validRun with { RunId = "RUN-MANUAL-DRAFT", Status = "Draft" };
        var mismatchedBaselineRun = validRun with { RunId = "RUN-MANUAL-WRONG-BASELINE", BaselineSnapshotId = "BASELINE-OTHER" };
        var missingResponseRun = validRun with { RunId = "RUN-MANUAL-NO-RESPONSE", ResponseId = null };
        var source = new SeedScenarioWorkspaceDataSource(SeedData.Create());
        var preview = new ScenarioRunPreviewService(source);
        var service = new MasterSettingsGovernanceService(
            source,
            preview,
            new FixedScenarioRunLineageReader(validRun, unsavedRun, mismatchedBaselineRun, missingResponseRun),
            databasePath);
        var proposal = service.ProposeFromPreview(new ScenarioRunPreviewRequest(
                12,
                Parameters: new ScenarioRunParameterSet(
                    SkuPolicyOverrides: new[] { new SkuPolicyOverride("AV-FPGA-203", MinimumOrderQuantity: 500) })))
            .Proposals
            .First();

        MasterSettingChangeRequest Manual(string runId) => proposal with
        {
            CreationMethod = "Manual",
            SourceBaselineId = validRun.BaselineSnapshotId,
            SourceScenarioRunId = runId
        };

        AssertArgumentRejected(
            () => service.SaveChange(new MasterSettingChangeSaveRequest("DDS&OP 计划员", Manual("RUN-MANUAL-MISSING"))),
            "manual change with unknown run");
        AssertArgumentRejected(
            () => service.SaveChange(new MasterSettingChangeSaveRequest("DDS&OP 计划员", Manual(unsavedRun.RunId))),
            "manual change with non-saved run");
        AssertArgumentRejected(
            () => service.SaveChange(new MasterSettingChangeSaveRequest("DDS&OP 计划员", Manual(mismatchedBaselineRun.RunId))),
            "manual change with mismatched run baseline");
        AssertArgumentRejected(
            () => service.SaveChange(new MasterSettingChangeSaveRequest("DDS&OP 计划员", Manual(missingResponseRun.RunId))),
            "manual change with run missing frozen-comparison response");

        var saved = service.SaveChange(new MasterSettingChangeSaveRequest("DDS&OP 计划员", Manual(validRun.RunId)));
        AssertEqual("Manual", saved.Summary.CreationMethod, "validated manual change creation method");
        AssertEqual(validRun.RunId, saved.Summary.SourceScenarioRunId, "validated manual change run lineage");
        AssertEqual(validRun.BaselineSnapshotId, saved.Summary.SourceBaselineId, "validated manual change baseline lineage");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestScenarioDerivedGovernanceChangeRequiresBaselineAndScenario()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-scenario-derived-governance-{Guid.NewGuid():N}.db");
    try
    {
        var validRun = new ScenarioRunSummary(
            "RUN-SAVED-001", "SR-20260715-0001", "冻结比较响应", null, "DDS&OP 计划员", "Saved", "NotSubmitted",
            "2026-07-15T08:00:00Z", 12, null, null, 98m, 1m, 1_000_000m, 90m, 0m, 0, 1,
            "BASELINE-001", "EXT-001", "RESP-001");
        var mismatchedBaselineRun = validRun with { RunId = "RUN-WRONG-BASELINE", BaselineSnapshotId = "BASELINE-OTHER" };
        var missingResponseRun = validRun with { RunId = "RUN-NO-RESPONSE", ResponseId = null };
        var unsavedRun = validRun with { RunId = "RUN-NOT-SAVED", Status = "Draft" };
        var lineageReader = new FixedScenarioRunLineageReader(validRun, mismatchedBaselineRun, missingResponseRun, unsavedRun);
        var source = new SeedScenarioWorkspaceDataSource(SeedData.Create());
        var preview = new ScenarioRunPreviewService(source);
        var service = new MasterSettingsGovernanceService(source, preview, lineageReader, databasePath);
        var proposal = service.ProposeFromPreview(new ScenarioRunPreviewRequest(
                12,
                Parameters: new ScenarioRunParameterSet(
                    SkuPolicyOverrides: new[] { new SkuPolicyOverride("AV-FPGA-203", MinimumOrderQuantity: 500) })))
            .Proposals
            .First();

        MasterSettingChangeRequest Derived(string? baselineId, string? runId) => proposal with
        {
            SourceBaselineId = baselineId,
            SourceScenarioRunId = runId,
            CreationMethod = "ScenarioDerived",
            Status = "Effective"
        };

        AssertArgumentRejected(
            () => service.SaveChange(new MasterSettingChangeSaveRequest("DDS&OP 计划员", Derived(null, validRun.RunId))),
            "scenario-derived change without baseline");
        AssertArgumentRejected(
            () => service.SaveChange(new MasterSettingChangeSaveRequest("DDS&OP 计划员", Derived("BASELINE-001", null))),
            "scenario-derived change without run");
        AssertArgumentRejected(
            () => service.SaveChange(new MasterSettingChangeSaveRequest("DDS&OP 计划员", Derived("BASELINE-001", "RUN-MISSING"))),
            "scenario-derived change with unknown run");
        AssertArgumentRejected(
            () => service.SaveChange(new MasterSettingChangeSaveRequest("DDS&OP 计划员", Derived("BASELINE-001", mismatchedBaselineRun.RunId))),
            "scenario-derived change with mismatched baseline");
        AssertArgumentRejected(
            () => service.SaveChange(new MasterSettingChangeSaveRequest("DDS&OP 计划员", Derived("BASELINE-001", missingResponseRun.RunId))),
            "scenario-derived change with unsaved comparison response");
        AssertArgumentRejected(
            () => service.SaveChange(new MasterSettingChangeSaveRequest("DDS&OP 计划员", Derived("BASELINE-001", unsavedRun.RunId))),
            "scenario-derived change with a non-saved run");
        AssertArgumentRejected(
            () => service.SaveChange(new MasterSettingChangeSaveRequest(
                "DDS&OP 计划员",
                proposal with { CreationMethod = "Legacy", SourceBaselineId = "BASELINE-001" })),
            "new Legacy governance change");
        AssertArgumentRejected(
            () => service.SaveChange(new MasterSettingChangeSaveRequest(
                "DDS&OP 计划员",
                proposal with { CreationMethod = "Imported", SourceBaselineId = "BASELINE-001" })),
            "unsupported governance creation method");

        var saved = service.SaveChange(new MasterSettingChangeSaveRequest(
            "DDS&OP 计划员",
            Derived("BASELINE-001", validRun.RunId)));
        var detail = service.GetDetail(saved.ChangeId)!;

        AssertEqual("Proposed", saved.Status, "scenario-derived change must always start proposed");
        AssertEqual("BASELINE-001", saved.Summary.SourceBaselineId, "scenario-derived summary baseline lineage");
        AssertEqual(validRun.RunId, saved.Summary.SourceScenarioRunId, "scenario-derived summary run lineage");
        AssertEqual("ScenarioDerived", saved.Summary.CreationMethod, "scenario-derived summary creation method");
        AssertEqual("BASELINE-001", detail.Summary.SourceBaselineId, "persisted scenario-derived baseline lineage");
        AssertEqual("ScenarioDerived", detail.Summary.CreationMethod, "persisted scenario-derived creation method");
        AssertEqual("Proposed", detail.Proposal.Status, "persisted proposal must remain proposed");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestUnlinkedHistoricalRecordsRemainExplicitlyUnlinked()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-legacy-governance-{Guid.NewGuid():N}.db");
    try
    {
        var proposal = new MasterSettingChangeRequest(
            null, null, "Inventory Buffer", "历史缓冲", "100", "120", "历史记录", "下周期", "Proposed",
            1m, 0m, "Yellow", new[] { "迁移前记录" });
        using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE master_setting_changes (
                    change_id TEXT PRIMARY KEY,
                    change_number TEXT NOT NULL UNIQUE,
                    source_scenario_run_id TEXT NULL,
                    source_template_id TEXT NULL,
                    setting_type TEXT NOT NULL,
                    target TEXT NOT NULL,
                    current_value TEXT NOT NULL,
                    proposed_value TEXT NOT NULL,
                    trigger TEXT NOT NULL,
                    effective_window TEXT NOT NULL,
                    status TEXT NOT NULL,
                    service_impact REAL NOT NULL,
                    cash_impact REAL NOT NULL,
                    risk_level TEXT NOT NULL,
                    created_by TEXT NOT NULL,
                    created_at_utc TEXT NOT NULL,
                    proposal_json TEXT NOT NULL,
                    impact_json TEXT NOT NULL
                );
                INSERT INTO master_setting_changes (
                    change_id, change_number, source_scenario_run_id, source_template_id, setting_type, target,
                    current_value, proposed_value, trigger, effective_window, status, service_impact, cash_impact,
                    risk_level, created_by, created_at_utc, proposal_json, impact_json)
                VALUES (
                    $change_id, $change_number, NULL, NULL, $setting_type, $target,
                    $current_value, $proposed_value, $trigger, $effective_window, $status, $service_impact, $cash_impact,
                    $risk_level, $created_by, $created_at_utc, $proposal_json, $impact_json);
                """;
            command.Parameters.AddWithValue("$change_id", "LEGACY-CHANGE-001");
            command.Parameters.AddWithValue("$change_number", "MSG-LEGACY-0001");
            command.Parameters.AddWithValue("$setting_type", proposal.SettingType);
            command.Parameters.AddWithValue("$target", proposal.Target);
            command.Parameters.AddWithValue("$current_value", proposal.CurrentValue);
            command.Parameters.AddWithValue("$proposed_value", proposal.ProposedValue);
            command.Parameters.AddWithValue("$trigger", proposal.Trigger);
            command.Parameters.AddWithValue("$effective_window", proposal.EffectiveWindow);
            command.Parameters.AddWithValue("$status", proposal.Status);
            command.Parameters.AddWithValue("$service_impact", proposal.ServiceImpact);
            command.Parameters.AddWithValue("$cash_impact", proposal.CashImpact);
            command.Parameters.AddWithValue("$risk_level", proposal.RiskLevel);
            command.Parameters.AddWithValue("$created_by", "历史用户");
            command.Parameters.AddWithValue("$created_at_utc", "2026-01-01T00:00:00Z");
            command.Parameters.AddWithValue("$proposal_json", JsonSerializer.Serialize(proposal));
            command.Parameters.AddWithValue("$impact_json", JsonSerializer.Serialize(new MasterSettingChangeImpact(1m, 0m, "Yellow", "历史")));
            command.ExecuteNonQuery();
        }

        var source = new SeedScenarioWorkspaceDataSource(SeedData.Create());
        var service = new MasterSettingsGovernanceService(source, new ScenarioRunPreviewService(source), databasePath);
        var legacy = service.ListChanges(10).Single();

        AssertTrue(legacy.SourceScenarioRunId is null, "legacy scenario link must remain explicitly unlinked");
        AssertTrue(legacy.SourceBaselineId is null, "legacy baseline link must remain explicitly unlinked");
        AssertEqual("Legacy", legacy.CreationMethod, "legacy creation method after additive migration");

        using var verify = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}");
        verify.Open();
        using var columnsCommand = verify.CreateCommand();
        columnsCommand.CommandText = "PRAGMA table_info(master_setting_changes);";
        using var columnsReader = columnsCommand.ExecuteReader();
        var columns = new HashSet<string>(StringComparer.Ordinal);
        while (columnsReader.Read())
        {
            columns.Add(columnsReader.GetString(1));
        }
        AssertTrue(columns.Contains("source_baseline_id"), "legacy table should add source_baseline_id");
        AssertTrue(columns.Contains("creation_method"), "legacy table should add creation_method");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestScenarioChangeAndCoordinationLinksAreQueryableBothDirections()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-bidirectional-lineage-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var source = new SeedScenarioWorkspaceDataSource(validationData);
        var preview = new ScenarioRunPreviewService(source);
        var baselineService = new CurrentBaselineService(new SeedCurrentBaselineDataSource(validationData), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "双向血缘基线"));
        var comparisonService = new ScenarioComparisonService(baselineService, preview, new SeedScenarioAssumptionSource());
        var runs = new ScenarioRunPersistenceService(preview, comparisonService, databasePath);
        var comparisonRequest = CreateLineageComparisonRequest(frozen.SnapshotId);
        var runA = runs.SaveFrozenComparison(new ScenarioComparisonSaveRequest(
            comparisonRequest, "RESP-LINEAGE-A", "血缘响应 A", null, "DDS&OP 计划员"));
        var runB = runs.SaveFrozenComparison(new ScenarioComparisonSaveRequest(
            comparisonRequest, "RESP-LINEAGE-B", "血缘响应 B", null, "DDS&OP 计划员"));
        var governance = new MasterSettingsGovernanceService(source, preview, runs, databasePath);
        var changeA = SaveScenarioDerivedChange(governance, frozen.SnapshotId, runA.RunId, "能力保护 A");
        var changeB = SaveScenarioDerivedChange(governance, frozen.SnapshotId, runA.RunId, "能力保护 B");
        var coordination = new CoordinationLedgerService(databasePath);
        var itemA = CreateLineageCoordinationItem(coordination, "行动 A", runA.RunId, changeA.ChangeId);
        var itemB = CreateLineageCoordinationItem(coordination, "行动 B", runA.RunId, changeA.ChangeId);
        var itemC = CreateLineageCoordinationItem(coordination, "行动 C", runA.RunId, changeB.ChangeId);

        var runsFromBaselineAndScenario = runs.List(50, frozen.SnapshotId, comparisonRequest.ExternalScenario.ScenarioId);
        AssertEqual(2, runsFromBaselineAndScenario.Count, "runs filtered by baseline and external scenario");
        AssertTrue(runsFromBaselineAndScenario.Select(item => item.RunId).ToHashSet(StringComparer.Ordinal)
            .SetEquals(new[] { runA.RunId, runB.RunId }), "baseline/scenario query should return both saved runs");
        AssertEqual(0, runs.List(50, frozen.SnapshotId, "' OR 1=1 --").Count, "scenario filter must remain a SQL parameter");

        var changesFromRun = governance.ListChanges(50, frozen.SnapshotId, runA.RunId);
        AssertEqual(2, changesFromRun.Count, "changes filtered by baseline and run");
        AssertTrue(changesFromRun.Select(item => item.ChangeId).ToHashSet(StringComparer.Ordinal)
            .SetEquals(new[] { changeA.ChangeId, changeB.ChangeId }), "run query should return both linked changes");
        AssertEqual(0, governance.ListChanges(50, "' OR 1=1 --", null).Count, "baseline filter must remain a SQL parameter");

        var coordinationFromRun = coordination.List(50, runA.RunId, null);
        AssertEqual(3, coordinationFromRun.Count, "coordination items filtered by run");
        AssertTrue(coordinationFromRun.Select(item => item.ItemId).ToHashSet(StringComparer.Ordinal)
            .SetEquals(new[] { itemA.ItemId, itemB.ItemId, itemC.ItemId }), "run query should return all linked coordination items");
        AssertEqual(2, coordination.List(50, null, changeA.ChangeId).Count, "one change may link to multiple coordination items");
        AssertEqual(2, coordination.List(50, runA.RunId, changeA.ChangeId).Count, "combined run/change filter");
        AssertEqual(0, coordination.List(50, "' OR 1=1 --", null).Count, "coordination filter must remain a SQL parameter");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestBaselineReferencesExposeRunsChangesAndActions()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-baseline-references-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var source = new SeedScenarioWorkspaceDataSource(validationData);
        var preview = new ScenarioRunPreviewService(source);
        var baselineService = new CurrentBaselineService(new SeedCurrentBaselineDataSource(validationData), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "基线引用查询"));
        var comparisonService = new ScenarioComparisonService(baselineService, preview, new SeedScenarioAssumptionSource());
        var runs = new ScenarioRunPersistenceService(preview, comparisonService, databasePath);
        var comparisonRequest = CreateLineageComparisonRequest(frozen.SnapshotId);
        var savedRun = runs.SaveFrozenComparison(new ScenarioComparisonSaveRequest(
            comparisonRequest, "RESP-LINEAGE-A", "基线引用响应", null, "DDS&OP 计划员"));
        var governance = new MasterSettingsGovernanceService(source, preview, runs, databasePath);
        var change = SaveScenarioDerivedChange(governance, frozen.SnapshotId, savedRun.RunId, "基线引用变更");
        var coordination = new CoordinationLedgerService(databasePath);
        var linkedByBoth = CreateLineageCoordinationItem(coordination, "同时关联 run 与 change", savedRun.RunId, change.ChangeId);
        var linkedByRun = CreateLineageCoordinationItem(coordination, "仅关联 run", savedRun.RunId, null);
        var linkedByChange = CreateLineageCoordinationItem(coordination, "仅关联 change", null, change.ChangeId);
        var unlinked = CreateLineageCoordinationItem(coordination, "未关联历史证据", null, null);
        var query = new BaselineLineageQueryService(runs, governance, coordination);

        var result = query.Get(frozen.SnapshotId);

        AssertEqual(frozen.SnapshotId, result.BaselineSnapshotId, "baseline lineage identity");
        AssertEqual(1, result.ScenarioRuns.Count, "baseline lineage run count");
        AssertEqual(savedRun.RunId, result.ScenarioRuns.Single().RunId, "baseline lineage run");
        AssertEqual(1, result.MasterSettingChanges.Count, "baseline lineage change count");
        AssertEqual(change.ChangeId, result.MasterSettingChanges.Single().ChangeId, "baseline lineage change");
        AssertEqual(3, result.CoordinationItems.Count, "baseline lineage coordination count after deduplication");
        AssertTrue(result.CoordinationItems.Select(item => item.ItemId).ToHashSet(StringComparer.Ordinal)
            .SetEquals(new[] { linkedByBoth.ItemId, linkedByRun.ItemId, linkedByChange.ItemId }),
            "baseline lineage should include direct run/change actions once each");
        AssertTrue(result.CoordinationItems.All(item => item.ItemId != unlinked.ItemId),
            "unlinked evidence must not be guessed into baseline references");
        AssertTrue(result.CoordinationItems.Select(item => item.CreatedAtUtc)
            .SequenceEqual(result.CoordinationItems.Select(item => item.CreatedAtUtc).OrderBy(value => value, StringComparer.Ordinal)),
            "baseline lineage coordination items should be sorted by creation time");

        var empty = query.Get("BASELINE-NOT-FOUND");
        AssertEqual(0, empty.ScenarioRuns.Count, "empty baseline run references");
        AssertEqual(0, empty.MasterSettingChanges.Count, "empty baseline change references");
        AssertEqual(0, empty.CoordinationItems.Count, "empty baseline coordination references");
        AssertArgumentRejected(() => query.Get(" "), "blank baseline lineage query");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestBaselineReferencesReturnAllLinksBeyondPublicPageLimit()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-complete-baseline-references-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var source = new SeedScenarioWorkspaceDataSource(validationData);
        var preview = new ScenarioRunPreviewService(source);
        var baselineService = new CurrentBaselineService(new SeedCurrentBaselineDataSource(validationData), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "超过公开分页上限的完整引用"));
        var comparisonService = new ScenarioComparisonService(baselineService, preview, new SeedScenarioAssumptionSource());
        var runs = new ScenarioRunPersistenceService(preview, comparisonService, databasePath);
        var comparisonRequest = CreateLineageComparisonRequest(frozen.SnapshotId);
        var savedRun = runs.SaveFrozenComparison(new ScenarioComparisonSaveRequest(
            comparisonRequest, "RESP-LINEAGE-A", "完整血缘模板 run", null, "DDS&OP 计划员"));
        var governance = new MasterSettingsGovernanceService(source, preview, runs, databasePath);
        var change = SaveScenarioDerivedChange(governance, frozen.SnapshotId, savedRun.RunId, "完整血缘模板 change");
        var coordination = new CoordinationLedgerService(databasePath);
        var item = CreateLineageCoordinationItem(coordination, "完整血缘模板 item", savedRun.RunId, change.ChangeId);
        CloneLineageRows(databasePath, savedRun.RunId, change.ChangeId, item.ItemId, 200);
        var query = new BaselineLineageQueryService(runs, governance, coordination);

        var result = query.Get(frozen.SnapshotId);

        AssertEqual(201, result.ScenarioRuns.Count, "baseline references must not truncate scenario runs at public API limit");
        AssertEqual(201, result.MasterSettingChanges.Count, "baseline references must not truncate changes at public API limit");
        AssertEqual(201, result.CoordinationItems.Count, "baseline references must not truncate coordination at public API limit");
        AssertEqual(200, runs.List(500, frozen.SnapshotId, null).Count, "public scenario-run API limit must remain 200");
        AssertEqual(200, governance.ListChanges(500, frozen.SnapshotId, null).Count, "public governance API limit must remain 200");
        AssertEqual(200, coordination.List(500, savedRun.RunId, change.ChangeId).Count, "public coordination API limit must remain 200");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestLineageFiltersUseIndexableParameterizedEqualityPredicates()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var cases = new[]
    {
        (
            FileName: "ScenarioRunPersistenceService.cs",
            Filters: new[]
            {
                (Variable: "baselineFilter", Parameter: "$baseline_snapshot_id", Predicate: "baseline_snapshot_id = $baseline_snapshot_id"),
                (Variable: "externalScenarioFilter", Parameter: "$external_scenario_id", Predicate: "external_scenario_id = $external_scenario_id")
            }),
        (
            FileName: "MasterSettingsGovernanceService.cs",
            Filters: new[]
            {
                (Variable: "baselineFilter", Parameter: "$source_baseline_id", Predicate: "source_baseline_id = $source_baseline_id"),
                (Variable: "scenarioRunFilter", Parameter: "$source_scenario_run_id", Predicate: "source_scenario_run_id = $source_scenario_run_id")
            }),
        (
            FileName: "CoordinationLedgerService.cs",
            Filters: new[]
            {
                (Variable: "scenarioRunFilter", Parameter: "$related_scenario_run_id", Predicate: "related_scenario_run_id = $related_scenario_run_id"),
                (Variable: "masterSettingChangeFilter", Parameter: "$related_master_setting_change_id", Predicate: "related_master_setting_change_id = $related_master_setting_change_id")
            })
    };

    foreach (var testCase in cases)
    {
        var path = Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", testCase.FileName);
        var source = File.ReadAllText(path);
        AssertTrue(!source.Contains(" IS NULL OR ", StringComparison.Ordinal),
            $"{testCase.FileName} must not hide lineage equality behind nullable OR predicates");
        AssertTrue(source.Contains("string.Join(\" AND \", predicates)", StringComparison.Ordinal),
            $"{testCase.FileName} should compose only the active fixed predicate fragments");

        foreach (var filter in testCase.Filters)
        {
            AssertTrue(source.Contains($"if ({filter.Variable} is not null)", StringComparison.Ordinal),
                $"{testCase.FileName} should include {filter.Predicate} only when its filter is present");
            AssertTrue(source.Contains($"predicates.Add(\"{filter.Predicate}\")", StringComparison.Ordinal),
                $"{testCase.FileName} should use the fixed equality predicate {filter.Predicate}");
            AssertTrue(source.Contains($"AddWithValue(\"{filter.Parameter}\", {filter.Variable})", StringComparison.Ordinal),
                $"{testCase.FileName} should bind {filter.Parameter} as a parameter value");
            AssertTrue(!source.Contains($"{{{filter.Variable}}}", StringComparison.Ordinal),
                $"{testCase.FileName} must not interpolate user-controlled filter values into SQL");
        }
    }
}

static void TestCoordinationOutcomeDoesNotAdvanceGovernanceStatus()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-non-automation-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var source = new SeedScenarioWorkspaceDataSource(validationData);
        var preview = new ScenarioRunPreviewService(source);
        var baselineService = new CurrentBaselineService(new SeedCurrentBaselineDataSource(validationData), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "非自动化验证"));
        var comparisonService = new ScenarioComparisonService(baselineService, preview, new SeedScenarioAssumptionSource());
        var runs = new ScenarioRunPersistenceService(preview, comparisonService, databasePath);
        var comparisonRequest = CreateLineageComparisonRequest(frozen.SnapshotId);
        var savedRun = runs.SaveFrozenComparison(new ScenarioComparisonSaveRequest(
            comparisonRequest, "RESP-LINEAGE-A", "非自动化响应", null, "DDS&OP 计划员"));
        var governance = new MasterSettingsGovernanceService(source, preview, runs, databasePath);
        var change = SaveScenarioDerivedChange(governance, frozen.SnapshotId, savedRun.RunId, "非自动化变更");
        var coordination = new CoordinationLedgerService(databasePath);
        var item = CreateLineageCoordinationItem(coordination, "记录实际效果", savedRun.RunId, change.ChangeId);
        var baselineBefore = JsonSerializer.Serialize(baselineService.GetDetail(frozen.SnapshotId));
        var runBefore = JsonSerializer.Serialize(runs.GetDetail(savedRun.RunId));
        var governanceBefore = governance.GetDetail(change.ChangeId)!;
        var governanceAuditCountBefore = governance.GetAuditEvents(change.ChangeId).Count;

        var decided = coordination.RecordDecision(item.ItemId, new CoordinationDecisionUpdateRequest(
            "继续观察", "等待下一 DDS&OP 周期人工确认", "运营经理"));
        var withOutcome = coordination.RecordOutcome(item.ItemId, new CoordinationOutcomeUpdateRequest(
            "服务水平上升 0.8pp，待下周期复核", "运营经理"));

        AssertEqual("Open", decided.Status, "recording a decision must not advance coordination status");
        AssertEqual("Open", withOutcome.Status, "recording an outcome must not advance coordination status");
        AssertTrue(!string.IsNullOrWhiteSpace(withOutcome.ActualOutcome), "coordination item should retain actual outcome evidence");
        AssertEqual(governanceBefore.Summary.Status, governance.GetDetail(change.ChangeId)!.Summary.Status,
            "coordination outcome must not advance governance status");
        AssertEqual("Proposed", governance.GetDetail(change.ChangeId)!.Summary.Status,
            "linked governance change must remain proposed");
        AssertEqual(governanceAuditCountBefore, governance.GetAuditEvents(change.ChangeId).Count,
            "coordination evidence must not append governance audit transitions");
        AssertEqual(baselineBefore, JsonSerializer.Serialize(baselineService.GetDetail(frozen.SnapshotId)),
            "coordination outcome must not mutate frozen baseline");
        AssertEqual(runBefore, JsonSerializer.Serialize(runs.GetDetail(savedRun.RunId)),
            "coordination outcome must not mutate saved scenario run");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestLineageEndpointsExposeReadOnlyFiltersAndValidateSavedComparisonRuns()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-lineage-api-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var source = new SeedScenarioWorkspaceDataSource(validationData);
        var preview = new ScenarioRunPreviewService(source);
        var baselineService = new CurrentBaselineService(new SeedCurrentBaselineDataSource(validationData), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "血缘 API 验证"));
        var comparisonService = new ScenarioComparisonService(baselineService, preview, new SeedScenarioAssumptionSource());
        var comparisonRequest = CreateLineageComparisonRequest(frozen.SnapshotId);
        var selected = comparisonService.Compare(comparisonRequest).ResponseCases
            .Single(item => item.ResponseId == "RESP-LINEAGE-A");
        var validRun = new ScenarioRunSummary(
            "RUN-API-VALID", "SR-20260715-0100", "API 真实 run", null, "DDS&OP 计划员", "Saved", "NotSubmitted",
            "2026-07-15T08:00:00Z", 6, null, null, 98m, 1m, 1_000_000m, 90m, 0m, 0, 1,
            frozen.SnapshotId, selected.ExternalScenarioId, selected.ResponseId);
        var wrongBaseline = validRun with { RunId = "RUN-API-WRONG-BASELINE", BaselineSnapshotId = "BASELINE-OTHER" };
        var wrongScenario = validRun with { RunId = "RUN-API-WRONG-SCENARIO", ExternalScenarioId = "EXT-OTHER" };
        var wrongResponse = validRun with { RunId = "RUN-API-WRONG-RESPONSE", ResponseId = "RESP-LINEAGE-B" };
        var unsaved = validRun with { RunId = "RUN-API-NOT-SAVED", Status = "Draft" };
        var validDetail = new ScenarioRunDetail(
            validRun,
            selected.Preview.Request,
            selected.Preview with { IsPersisted = true });
        var governance = new MasterSettingsGovernanceService(
            source,
            preview,
            new FixedScenarioRunLineageReader(validDetail, wrongBaseline, wrongScenario, wrongResponse, unsaved),
            databasePath);

        var generated = governance.ProposeFromFrozenComparison(
            selected,
            frozen,
            validRun.RunId,
            new GovernanceDecisionContext(Owner: "运营经理"));
        AssertTrue(generated.Proposals.Count > 0, "valid saved comparison run should generate governance proposals");
        AssertTrue(generated.Proposals.All(item => item.SourceBaselineId == frozen.SnapshotId),
            "generated proposals should retain validated baseline");
        AssertTrue(generated.Proposals.All(item => item.SourceScenarioRunId == validRun.RunId),
            "generated proposals should retain real saved run ID");
        AssertTrue(generated.Proposals.All(item => item.CreationMethod == "ScenarioDerived"),
            "generated proposals should be scenario-derived");

        AssertArgumentRejected(
            () => governance.ProposeFromFrozenComparison(selected, frozen, "RUN-API-MISSING", new GovernanceDecisionContext()),
            "unknown frozen-comparison run");
        AssertArgumentRejected(
            () => governance.ProposeFromFrozenComparison(selected, frozen, wrongBaseline.RunId, new GovernanceDecisionContext()),
            "frozen-comparison run from another baseline");
        AssertArgumentRejected(
            () => governance.ProposeFromFrozenComparison(selected, frozen, wrongScenario.RunId, new GovernanceDecisionContext()),
            "frozen-comparison run from another external scenario");
        AssertArgumentRejected(
            () => governance.ProposeFromFrozenComparison(selected, frozen, wrongResponse.RunId, new GovernanceDecisionContext()),
            "frozen-comparison run from another response");
        AssertArgumentRejected(
            () => governance.ProposeFromFrozenComparison(selected, frozen, unsaved.RunId, new GovernanceDecisionContext()),
            "frozen-comparison run that is not saved");

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var dto = new FrozenComparisonGovernanceProposalRequest(
            comparisonRequest,
            selected.ResponseId,
            new GovernanceDecisionContext(Owner: "运营经理"),
            validRun.RunId);
        var roundTrip = JsonSerializer.Deserialize<FrozenComparisonGovernanceProposalRequest>(
            JsonSerializer.Serialize(dto, jsonOptions),
            jsonOptions)!;
        AssertEqual(validRun.RunId, roundTrip.SourceScenarioRunId, "frozen-comparison request run ID round trip");

        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var program = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Program.cs"));
        AssertTrue(program.Contains("/api/current-baselines/{snapshotId}/references", StringComparison.Ordinal),
            "baseline references endpoint should be exposed");
        AssertTrue(program.Contains("string? baselineSnapshotId", StringComparison.Ordinal)
            && program.Contains("string? externalScenarioId", StringComparison.Ordinal),
            "scenario run endpoint should expose lineage filters");
        AssertTrue(program.Contains("string? sourceBaselineId", StringComparison.Ordinal)
            && program.Contains("string? sourceScenarioRunId", StringComparison.Ordinal),
            "governance endpoint should expose lineage filters");
        AssertTrue(program.Contains("string? relatedScenarioRunId", StringComparison.Ordinal)
            && program.Contains("string? relatedMasterSettingChangeId", StringComparison.Ordinal),
            "coordination endpoint should expose lineage filters");
        AssertTrue(program.Contains("AddSingleton<IBaselineLineageQueryService, BaselineLineageQueryService>", StringComparison.Ordinal),
            "baseline lineage query service should be registered");
        AssertTrue(!program.Contains("MapPost(\"/api/current-baselines/{snapshotId}/references", StringComparison.Ordinal),
            "baseline references endpoint must remain read-only");
        AssertTrue(!program.Contains("MapPost(\"/api/master-settings/changes/{changeId}/approve", StringComparison.Ordinal)
            && !program.Contains("MapPost(\"/api/current-baselines/{snapshotId}/publish", StringComparison.Ordinal)
            && !program.Contains("MapPost(\"/api/coordination-items/{itemId}/forward", StringComparison.Ordinal),
            "lineage work must not add approval publish or forwarding automation");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestFrozenComparisonGovernanceRejectsReusedRunIdWithDifferentRequest()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-governance-run-request-{Guid.NewGuid():N}.db");
    try
    {
        var validationData = SeedData.Create();
        var source = new SeedScenarioWorkspaceDataSource(validationData);
        var preview = new ScenarioRunPreviewService(source);
        var baselineService = new CurrentBaselineService(new SeedCurrentBaselineDataSource(validationData), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "治理运行请求一致性"));
        var comparisonService = new ScenarioComparisonService(baselineService, preview, new SeedScenarioAssumptionSource());
        var originalRequest = CreateLineageComparisonRequest(frozen.SnapshotId);
        var runs = new ScenarioRunPersistenceService(preview, comparisonService, databasePath);
        var saved = runs.SaveFrozenComparison(new ScenarioComparisonSaveRequest(
            originalRequest,
            "RESP-LINEAGE-A",
            "治理血缘源运行",
            "保存规范化请求",
            "DDS&OP 计划员"));
        var governance = new MasterSettingsGovernanceService(source, preview, runs, databasePath);

        var alteredResponseRequest = originalRequest with
        {
            ResponseOptions = originalRequest.ResponseOptions!
                .Select(option => option.ResponseId == "RESP-LINEAGE-A"
                    ? option with
                    {
                        Parameters = new ScenarioRunParameterSet(
                            CapacityAdjustments: new[]
                            {
                                new ResourceCapacityAdjustment("RES-TVAC", 3, 1.35m, "篡改临时能力")
                            })
                    }
                    : option)
                .ToList()
        };
        var alteredScenarioRequest = originalRequest with
        {
            ExternalScenario = originalRequest.ExternalScenario with
            {
                DemandChanges = new[]
                {
                    new ExternalDemandChange(null, "星载电子", 2, 5, 1.35m, "篡改需求变化")
                }
            }
        };

        foreach (var tamperedRequest in new[] { alteredResponseRequest, alteredScenarioRequest })
        {
            var selected = comparisonService.Compare(tamperedRequest).ResponseCases
                .Single(item => item.ResponseId == "RESP-LINEAGE-A");
            AssertArgumentRejected(
                () => governance.ProposeFromFrozenComparison(
                    selected,
                    frozen,
                    saved.RunId,
                    new GovernanceDecisionContext()),
                "saved run ID reused with different normalized request");
        }
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static ScenarioComparisonRequest CreateLineageComparisonRequest(string baselineSnapshotId)
{
    return new ScenarioComparisonRequest(
        baselineSnapshotId,
        new ExternalScenarioDefinition(
            "EXT-LINEAGE-001",
            "血缘查询场景",
            DemandChanges: new[] { new ExternalDemandChange(null, "星载电子", 2, 5, 1.10m, "血缘查询需求变化") },
            Metadata: new ScenarioAssumptionMetadata(
                "Manual", null, null, "DDS&OP 计划员", "2026-07-15T08:00:00Z",
                "2026-07-16", "2026-09-30", "血缘查询测试", "人工录入：血缘查询")),
        new[]
        {
            new ResponseConfiguration(
                "RESP-LINEAGE-A",
                "能力响应 A",
                new ScenarioRunParameterSet(
                    CapacityAdjustments: new[] { new ResourceCapacityAdjustment("RES-TVAC", 3, 1.10m, "临时能力 A") })),
            new ResponseConfiguration(
                "RESP-LINEAGE-B",
                "库存响应 B",
                new ScenarioRunParameterSet(
                    SkuPolicyOverrides: new[] { new SkuPolicyOverride("AV-FPGA-203", MinimumOrderQuantity: 520) }))
        },
        6);
}

static MasterSettingChangeSaveResponse SaveScenarioDerivedChange(
    MasterSettingsGovernanceService service,
    string baselineSnapshotId,
    string runId,
    string target)
{
    return service.SaveChange(new MasterSettingChangeSaveRequest(
        "DDS&OP 计划员",
        new MasterSettingChangeRequest(
            SourceScenarioRunId: runId,
            SourceTemplateId: null,
            SettingType: "Capacity Buffer",
            Target: target,
            CurrentValue: "当前能力边界",
            ProposedValue: "提升保护能力 10%",
            Trigger: "冻结比较响应",
            EffectiveWindow: "下一 DDS&OP 周期",
            Status: "Approved",
            ServiceImpact: 1m,
            CashImpact: 100_000m,
            RiskLevel: "Yellow",
            Rationale: new[] { "基于已保存的冻结比较 run" },
            SourceBaselineId: baselineSnapshotId,
            CreationMethod: "ScenarioDerived")));
}

static CoordinationItem CreateLineageCoordinationItem(
    CoordinationLedgerService service,
    string title,
    string? runId,
    string? changeId)
{
    return service.Create(new CoordinationItemCreateRequest(
        title,
        new[] { "能力保护" },
        runId,
        changeId,
        "服务影响 +1pp",
        "库存影响可控",
        100_000m,
        "Yellow",
        "需人工确认",
        "运营经理",
        "2026-07-31",
        "L1",
        "2026-08-07",
        "DDS&OP 计划员"));
}

static void CloneLineageRows(
    string databasePath,
    string templateRunId,
    string templateChangeId,
    string templateItemId,
    int cloneCount)
{
    using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}");
    connection.Open();
    using var transaction = connection.BeginTransaction();

    using var cloneRun = connection.CreateCommand();
    cloneRun.Transaction = transaction;
    cloneRun.CommandText = """
        INSERT INTO scenario_runs (
            run_id, run_number, name, description, created_by, status, approval_status, created_at_utc,
            horizon_weeks, template_id, adoption_constraint_mode, request_json, result_json,
            service_level_percent, flow_index, average_inventory_value, peak_load_percent,
            supply_gap, red_sku_count, replenishment_order_count,
            baseline_snapshot_id, external_scenario_id, response_id)
        SELECT
            $new_run_id, $new_run_number, name, description, created_by, status, approval_status, created_at_utc,
            horizon_weeks, template_id, adoption_constraint_mode, request_json, result_json,
            service_level_percent, flow_index, average_inventory_value, peak_load_percent,
            supply_gap, red_sku_count, replenishment_order_count,
            baseline_snapshot_id, external_scenario_id, response_id
        FROM scenario_runs
        WHERE run_id = $template_run_id;
        """;
    cloneRun.Parameters.AddWithValue("$new_run_id", string.Empty);
    cloneRun.Parameters.AddWithValue("$new_run_number", string.Empty);
    cloneRun.Parameters.AddWithValue("$template_run_id", templateRunId);

    using var cloneChange = connection.CreateCommand();
    cloneChange.Transaction = transaction;
    cloneChange.CommandText = """
        INSERT INTO master_setting_changes (
            change_id, change_number, source_scenario_run_id, source_template_id,
            setting_type, target, current_value, proposed_value, trigger, effective_window,
            status, service_impact, cash_impact, risk_level, created_by, created_at_utc,
            source_baseline_id, creation_method, proposal_json, impact_json)
        SELECT
            $new_change_id, $new_change_number, $source_run_id, source_template_id,
            setting_type, target, current_value, proposed_value, trigger, effective_window,
            status, service_impact, cash_impact, risk_level, created_by, created_at_utc,
            source_baseline_id, creation_method, proposal_json, impact_json
        FROM master_setting_changes
        WHERE change_id = $template_change_id;
        """;
    cloneChange.Parameters.AddWithValue("$new_change_id", string.Empty);
    cloneChange.Parameters.AddWithValue("$new_change_number", string.Empty);
    cloneChange.Parameters.AddWithValue("$source_run_id", templateRunId);
    cloneChange.Parameters.AddWithValue("$template_change_id", templateChangeId);

    using var cloneItem = connection.CreateCommand();
    cloneItem.Transaction = transaction;
    cloneItem.CommandText = """
        INSERT INTO coordination_items (
            item_id, item_number, title, impact_objects_json, related_scenario_run_id,
            related_master_setting_change_id, service_impact, inventory_impact, cash_impact,
            risk_impact, decision_required, owner, due_date, escalation_level, next_review_date,
            status, decision, decision_rationale, actual_outcome, created_by, created_at_utc, updated_at_utc)
        SELECT
            $new_item_id, $new_item_number, title, impact_objects_json, $related_run_id,
            $related_change_id, service_impact, inventory_impact, cash_impact,
            risk_impact, decision_required, owner, due_date, escalation_level, next_review_date,
            status, decision, decision_rationale, actual_outcome, created_by, created_at_utc, updated_at_utc
        FROM coordination_items
        WHERE item_id = $template_item_id;
        """;
    cloneItem.Parameters.AddWithValue("$new_item_id", string.Empty);
    cloneItem.Parameters.AddWithValue("$new_item_number", string.Empty);
    cloneItem.Parameters.AddWithValue("$related_run_id", templateRunId);
    cloneItem.Parameters.AddWithValue("$related_change_id", templateChangeId);
    cloneItem.Parameters.AddWithValue("$template_item_id", templateItemId);

    for (var index = 1; index <= cloneCount; index++)
    {
        cloneRun.Parameters["$new_run_id"].Value = $"RUN-LINEAGE-CLONE-{index:0000}";
        cloneRun.Parameters["$new_run_number"].Value = $"SR-CLONE-{index:0000}";
        cloneRun.ExecuteNonQuery();

        cloneChange.Parameters["$new_change_id"].Value = $"CHANGE-LINEAGE-CLONE-{index:0000}";
        cloneChange.Parameters["$new_change_number"].Value = $"MSG-CLONE-{index:0000}";
        cloneChange.ExecuteNonQuery();

        cloneItem.Parameters["$new_item_id"].Value = $"ITEM-LINEAGE-CLONE-{index:0000}";
        cloneItem.Parameters["$new_item_number"].Value = $"ISSUE-CLONE-{index:0000}";
        cloneItem.ExecuteNonQuery();
    }

    transaction.Commit();
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
    AssertTrue(page.Contains("从已保存比较生成建议", StringComparison.Ordinal) && page.Contains("从人工预览生成建议", StringComparison.Ordinal), "page should separate saved-comparison and manual proposal generation actions");
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

static void TestScenarioPreviewAppliesScenarioParameters()
{
    var service = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));
    var request = new ScenarioRunPreviewRequest(
        12,
        Parameters: new ScenarioRunParameterSet(
            PrebuildCampaigns: new[] { new PrebuildCampaign("PB-TEST", "AV-FPGA-203", 1, 6, 8, 1) },
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
    AssertTrue(result.SkuDetails.Any(item => item.Activities.Any(activity => activity.ActivityType == "补货订单生成")), "activities should explain replenishment decisions");
    AssertTrue(result.SkuDetails.Any(item => item.BufferSizing.Any(line => line.Formula.Contains("ADU", StringComparison.Ordinal))), "buffer sizing should expose DDMRP formulas");
    AssertTrue(result.SkuDetails.Any(item => item.Sku == result.SelectedSku), "selected SKU should exist in detail");
    AssertTrue(result.Series.Any(item => item.Status is "Red" or "Yellow" or "Green" or "Blue"), "series should expose display statuses");
    AssertTrue(result.Series.All(item => !string.IsNullOrWhiteSpace(item.PeriodStartDate)), "series should expose real time labels");
    AssertTrue(result.SkuDetails.Any(item => item.Series.Select(point => point.TopOfGreen).Distinct().Count() > 1), "time-phased DDMRP zones should vary across weeks");
    AssertTrue(result.Series.All(item => item.TopOfGreen > item.TopOfYellow && item.TopOfYellow > item.TopOfRed), "series should expose DDMRP zone tops");
}

static void TestFutureBufferTrendUsesBackendPeriodSizing()
{
    var data = new SeedScenarioWorkspaceDataSource(SeedData.Create())
        .Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1)));
    var run = DemandDrivenPlanningEngine.ProjectBuffers(data.Skus, data.Inventory, data.Demand, 12);
    var plan = new DemandDrivenPlanResult(run.BufferProjections, run.ReplenishmentOrders,
        Array.Empty<CapacityLoadProjection>(), Array.Empty<ProjectedSupplyRequirement>(), run.Traces);
    var trend = BufferTrendWorkspaceService.Build(data, "baseline", "鍩哄噯鏂规", data.Skus, plan);
    var detail = trend.SkuDetails.Single(item => item.Sku == "TC-MLI-301");

    AssertTrue(detail.Series.All(item => item.Sizing is not null), "every trend point should carry backend sizing");
    AssertTrue(detail.Series.Select(item => (item.TopOfRed, item.TopOfYellow, item.TopOfGreen)).Distinct().Count() >= 3,
        "planned demand should create at least three zone combinations");
    AssertTrue(detail.Series.All(item => item.TopOfRed <= item.TopOfYellow && item.TopOfYellow <= item.TopOfGreen),
        "zone tops should stay ordered");
    AssertTrue(detail.Series[3].Sizing!.PeriodAdu > detail.Series[4].Sizing!.PeriodAdu,
        "week four demand peak should exceed the following trough");
    AssertTrue(detail.Series.All(item => item.DemandSpikeThreshold is > 0m),
        "demand spike threshold should come from the backend");

    foreach (var point in detail.Series)
    {
        AssertEqual(point.Sizing!.Zones.TopOfRed, point.TopOfRed, $"top of red week {point.Week}");
        AssertEqual(point.Sizing.Zones.TopOfYellow, point.TopOfYellow, $"top of yellow week {point.Week}");
        AssertEqual(point.Sizing.Zones.TopOfGreen, point.TopOfGreen, $"top of green week {point.Week}");
    }
}

static void TestBufferTrendMapsOrderCycleWaitToActivity()
{
    var sku = new SkuBufferSetting(
        "SKU-CYCLE-ACTIVITY", "Cycle Activity Item", "Planning", 100, 5, 1.5m, 14, 700, 10m, 1000,
        LeadTimeFactor: 0.6m,
        ParameterSnapshotId: "SKU-CYCLE-ACTIVITY-V1",
        ParameterEvidenceStatus: "Complete");
    var inventory = new[] { new InventoryPosition(sku.Sku, 1800, 0, 0) };
    var demand = new[]
    {
        new WeeklyDemand(sku.Sku, 1, 300),
        new WeeklyDemand(sku.Sku, 2, 400),
        new WeeklyDemand(sku.Sku, 3, 100),
    };
    var data = new SeedScenarioWorkspaceDataSource(SeedData.Create())
        .Load(new ScenarioWorkspaceDataRequest(3, new DateOnly(2026, 6, 1))) with
    {
        Skus = new[] { sku },
        Inventory = inventory,
        Demand = demand,
    };
    var run = DemandDrivenPlanningEngine.ProjectBuffers(data.Skus, data.Inventory, data.Demand, 3);
    var plan = new DemandDrivenPlanResult(run.BufferProjections, run.ReplenishmentOrders,
        Array.Empty<CapacityLoadProjection>(), Array.Empty<ProjectedSupplyRequirement>(), run.Traces);
    var trend = BufferTrendWorkspaceService.Build(data, "cycle-activity", "订货周期活动验证", data.Skus, plan);
    var weekTwo = run.BufferProjections.Single(point => point.Sku == sku.Sku && point.Week == 2);
    var weekTwoTrace = run.Traces.Single(item => item.Sku == sku.Sku && item.Week == 2);
    var waitActivity = trend.SkuDetails.Single().Activities
        .Single(item => item.Week == 2 && item.ActivityType == "订货周期复核");

    AssertTrue(weekTwo.Sizing is not null, "wait projection should carry period sizing");
    AssertTrue(!run.ReplenishmentOrders.Any(order => order.Sku == sku.Sku && order.Week == 2),
        "non-review week should not create a replenishment order");
    AssertEqual("等待", waitActivity.Direction, "wait activity direction");
    AssertEqual(weekTwoTrace.Explanation, waitActivity.TriggerReason, "wait activity should preserve the backend trace");
    AssertEqual(weekTwo.EndNetFlowBeforeReplenishment, waitActivity.ResultingNetFlow, "wait activity net flow");
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

static void TestDdsopConfigInboundPayloadAndAckInterpreter()
{
    var service = new DdsopConfigInboundContractService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));
    var message = service.Build(new DdsopConfigInboundContractRequest(
        12,
        new DateOnly(2026, 6, 26),
        "S&OP 经理",
        "SR-20260626-0001",
        "CHG-20260626-001"));

    AssertEqual("DDSOP-CONFIG-INBOUND-V1", message.ContractID, "config contract id");
    AssertEqual("1.0.0", message.ContractVersion, "config contract version");
    AssertEqual("DDAE", message.SourceSystem, "config source system");
    AssertEqual("SDBR", message.TargetSystem, "config target system");
    AssertEqual("Approved", message.Payload.Status, "config status");
    AssertEqual("Approved", message.Payload.Approval.ApprovalStatus, "approval status");
    AssertTrue(message.Payload.SchedulingConfiguration.ControlPoints.Count > 0, "config should include control points");
    AssertTrue(message.Payload.DDMRPConfiguration.DecouplingPoints.Count > 0, "config should include DDMRP decoupling points");
    AssertTrue(message.Payload.Fingerprint.StartsWith("sha256:", StringComparison.Ordinal), "config should expose a fingerprint");

    var interpreter = new DdsopConfigInboundAckInterpreter();
    var accepted = interpreter.Interpret("""
        {
          "ContractID": "DDSOP-CONFIG-INBOUND-V1",
          "ContractVersion": "1.0.0",
          "OriginalMessageID": "DDAE-MSG-001",
          "IdempotencyKey": "DDAE:DDAE-MSG-001",
          "ProcessingStatus": "Accepted",
          "UsableForPlanningRun": true,
          "AcceptedConfigurationID": "DDSOP-OMC-20260626-A",
          "Fingerprint": "sha256:240afdcce3131250675342ec370f3aac4b3fd6d499d86fa6d6ee467a1832fe87",
          "PendingReferences": [],
          "Errors": []
        }
        """);
    var pending = interpreter.Interpret("""
        {
          "ContractID": "DDSOP-CONFIG-INBOUND-V1",
          "ContractVersion": "1.0.0",
          "OriginalMessageID": "DDAE-MSG-001",
          "IdempotencyKey": "DDAE:DDAE-MSG-001",
          "ProcessingStatus": "AcceptedPendingReferences",
          "UsableForPlanningRun": false,
          "AcceptedConfigurationID": null,
          "Fingerprint": null,
          "PendingReferences": [
            { "Field": "Payload.SchedulingConfiguration.ControlPoints[0].ResourceID", "ReferenceID": "RES-001", "ReferenceType": "Resource" }
          ],
          "Errors": []
        }
        """);
    var rejected = interpreter.Interpret("""
        {
          "ContractID": "DDSOP-CONFIG-INBOUND-V1",
          "ContractVersion": "1.0.0",
          "OriginalMessageID": "DDAE-MSG-001",
          "IdempotencyKey": "DDAE:DDAE-MSG-001",
          "ProcessingStatus": "Rejected",
          "UsableForPlanningRun": false,
          "AcceptedConfigurationID": null,
          "Fingerprint": null,
          "PendingReferences": [],
          "Errors": [
            { "Code": "REQUIRED_FIELD_MISSING", "Message": "Approval is required.", "Field": "Payload.Approval" }
          ]
        }
        """);
    var duplicate = interpreter.Interpret("""
        {
          "ContractID": "DDSOP-CONFIG-INBOUND-V1",
          "ContractVersion": "1.0.0",
          "OriginalMessageID": "DDAE-MSG-001",
          "IdempotencyKey": "DDAE:DDAE-MSG-001",
          "ProcessingStatus": "Duplicate",
          "UsableForPlanningRun": false,
          "AcceptedConfigurationID": null,
          "Fingerprint": null,
          "PendingReferences": [],
          "Errors": []
        }
        """);

    AssertEqual("Accepted", accepted.ProcessingStatus, "accepted ACK");
    AssertTrue(accepted.UsableForPlanningRun, "accepted ACK should be usable");
    AssertEqual("AcceptedPendingReferences", pending.ProcessingStatus, "pending ACK");
    AssertTrue(!pending.UsableForPlanningRun && pending.PendingReferences.Count == 1, "pending ACK should expose unresolved references");
    AssertEqual("Rejected", rejected.ProcessingStatus, "rejected ACK");
    AssertTrue(rejected.Errors.Count == 1, "rejected ACK should expose errors");
    AssertEqual("Duplicate", duplicate.ProcessingStatus, "duplicate ACK");
}

static void TestDdsopFeedbackInboundLedgerAcceptsSdbrFixtures()
{
    var fixtureRoot = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Fixtures");
    var planningJson = File.ReadAllText(Path.GetFullPath(Path.Combine(fixtureRoot, "sdbr-actual-planning-run-feedback.json")));
    var varianceJson = File.ReadAllText(Path.GetFullPath(Path.Combine(fixtureRoot, "sdbr-actual-variance-analysis-feedback.json")));
    var ledger = new DdsopFeedbackInboundLedger();

    var planningAck = ledger.Accept(planningJson);
    var varianceAck = ledger.Accept(varianceJson);
    var duplicateAck = ledger.Accept(planningJson);

    AssertEqual("DDSOP-FEEDBACK-OUTBOUND-V1", planningAck.ContractID, "feedback ACK contract id");
    AssertEqual("Accepted", planningAck.ProcessingStatus, "planning feedback should be accepted");
    AssertEqual("Accepted", varianceAck.ProcessingStatus, "variance feedback should be accepted");
    AssertEqual("Duplicate", duplicateAck.ProcessingStatus, "duplicate feedback should return duplicate ACK");
    AssertEqual(2, ledger.Records.Count, "duplicate should not add another ledger record");
    AssertTrue(ledger.Records.Any(item => item.RawPayload == planningJson), "ledger should preserve planning feedback raw payload");
    AssertTrue(ledger.Records.Any(item => item.RawPayload == varianceJson), "ledger should preserve variance feedback raw payload");
    AssertTrue(ledger.Records.All(item => item.Interpretation.ApprovedConfigurationChangeCount == 0), "feedback should not become approved master-setting changes");
}

static void TestDdsopRuntimePlanningInputGeneratesDdaeOwnedPackage()
{
    var configService = new DdsopConfigInboundContractService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));
    var service = new DdsopRuntimePlanningInputContractService(configService);
    var message = service.Build(new DdsopRuntimePlanningInputRequest(
        12,
        new DateOnly(2026, 6, 26),
        "S&OP 经理",
        "SR-20260626-0001",
        "CHG-20260626-001"));
    var refs = message.Payload.ParameterAuthorityEvidence.ParameterEvidenceRefs;
    var fieldGroups = refs.Select(item => item.FieldGroup).ToHashSet(StringComparer.Ordinal);
    var json = JsonSerializer.Serialize(message, DdsopConfigInboundContractService.ContractJsonOptions);

    AssertEqual("DDSOP-RUNTIME-PLANNING-INPUT-V1", message.ContractID, "runtime planning input contract id");
    AssertEqual("0.1.0-draft", message.ContractVersion, "runtime planning input contract version");
    AssertTrue(message.TargetSystem.Contains("SDBR"), "runtime planning input should target SDBR");
    AssertEqual("RuntimePlanningInputPackagePublished", message.MessageType, "runtime package message type");
    AssertEqual("Reviewed", message.Payload.PackageIdentity.PackageStatus, "package status should be reviewed by default");
    AssertEqual("DDMRPAndBoundedScheduling", message.Payload.PackageIdentity.ExecutionMode, "default execution mode");
    AssertEqual("PublicDemoOnly", message.Payload.PackageIdentity.MappingConfidence, "default mapping confidence");
    AssertEqual("ControlledContractGoldenLoopDemo", message.Payload.PackageIdentity.ScenarioLabel, "default scenario label");
    AssertEqual("DDSOP-CONFIG-INBOUND-V1", message.Payload.FrozenDdsopConfiguration.SourceConfigurationContractID, "source configuration contract id");
    AssertTrue(message.Payload.FrozenDdsopConfiguration.OperatingModelFingerprint.StartsWith("sha256:", StringComparison.Ordinal), "frozen fingerprint");
    AssertEqual("DDAE-DDMRP-FORMULA-V1", message.Payload.ParameterAuthorityEvidence.DDMRPFormulaVersionID, "DDMRP formula version");
    AssertEqual("DDAE-SCHEDULING-RULE-V1", message.Payload.ParameterAuthorityEvidence.SchedulingRuleVersionID, "scheduling rule version");

    foreach (var required in new[]
             {
                 "ADU", "DLT", "VariabilityFactor", "MOQ", "OrderCycle", "BufferZones", "DecouplingPoint",
                 "BufferProfile", "UOM", "AdjustmentFactor", "ControlPoint", "TimeBuffer", "ResourcePolicy",
                 "CalendarPolicy", "ReleasePolicy"
             })
    {
        AssertTrue(fieldGroups.Contains(required), $"parameter authority evidence should cover {required}");
    }

    AssertTrue(refs.All(item => item.ProductionAuthorityStatus == "PublicDemoOnly"), "parameter evidence should stay public demo only");
    AssertTrue(message.Payload.ConsumerRules.ReadOnlyFrozenInputs.Contains("OPERATING_MODEL_CONFIGURATION_ID"), "consumer rules should freeze OMC id");
    AssertTrue(message.Payload.ConsumerRules.ReadOnlyFrozenInputs.Contains("DDMRP_DLT_MOQ_ORDER_CYCLE"), "consumer rules should freeze DDMRP policy");
    AssertTrue(message.Payload.ConsumerRules.SDBRDerivedRuntimeSignals.Contains("SCHEDULE_FEASIBILITY"), "consumer rules should allow SDBR schedule feasibility signals");
    AssertTrue(message.Payload.ConsumerRules.ForbiddenMutations.Contains("PROMOTE_RUNTIME_FEEDBACK_TO_APPROVED_MASTER_SETTING"), "consumer rules should forbid feedback promotion");
    AssertEqual("DDSOP-FEEDBACK-OUTBOUND-V1", message.Payload.OutputExpectations.FeedbackContractID, "feedback expectation contract id");
    AssertEqual("DeliveryLedger", message.Payload.OutputExpectations.FeedbackCorrelationMode, "feedback correlation mode");
    AssertTrue(message.Payload.RuntimeEvidenceSnapshot is not null, "DDMRP execution should include runtime evidence");
    AssertTrue(message.Payload.ExecutableSchedulingInputs is not null, "bounded scheduling execution should include executable inputs");
    AssertEqual("ADVENTUREWORKS-BOUNDED-SCHEDULING-ADAPTER-PROFILE-V1", message.Payload.ExecutableSchedulingInputs!.AdapterProfileID, "AdventureWorks adapter profile id");
    AssertEqual("AW-CAPACITY-UNIT-FIXTURE-ONE-UNIT-PER-RESOURCE-WINDOW", message.Payload.ExecutableSchedulingInputs.CapacityUnitNormalizationRuleID, "capacity normalization rule");
    AssertEqual("OmittedForPublicDemo", message.Payload.ExecutableSchedulingInputs.MaterialConstraintsMode, "material constraints mode");
    AssertEqual("NoSetupRulesApplied", message.Payload.ExecutableSchedulingInputs.SetupChangeoverMode, "setup changeover mode");
    AssertTrue(message.Payload.RuntimeEvidenceSnapshot!.InventoryPositions.All(item => item.EvidenceRefs.Count > 0), "inventory evidence refs");
    AssertTrue(message.Payload.ExecutableSchedulingInputs.WorkOrders.All(item => item.EvidenceRefs.Count > 0), "work order evidence refs");
    AssertTrue(message.Payload.ExecutableSchedulingInputs.ResourceCalendars.All(item => item.EvidenceRefs.Any(evidence => evidence.SourceAuthority.Contains("SDBR public demo calendar fixture", StringComparison.Ordinal))), "resource calendars should be marked as SDBR-owned fixture evidence");
    AssertEqual(0, message.Payload.ExecutableSchedulingInputs.MaterialConstraints.Count, "active public demo should omit material constraints");
    AssertTrue(!json.Contains("IncludedWithPublicDemoEvidence", StringComparison.Ordinal), "runtime package should not display candidate material evidence as active mode");
    AssertTrue(!json.Contains("ProductionValidated", StringComparison.Ordinal), "runtime package should not claim production validation");
    AssertTrue(!json.Contains("Business Golden Loop Readiness", StringComparison.Ordinal), "runtime package should not claim business golden loop readiness");
}

static void TestAdventureWorksSchedulingAdapterMetadataStaysNonDdaeOwned()
{
    var service = new PublicDemoGoldenLoopService(new PublicDemoGoldenLoopOptions(
        Path.GetTempPath(),
        "unused",
        Path.Combine(Path.GetTempPath(), "unused-ddae-to-sdbr.json"),
        Path.Combine(Path.GetTempPath(), "unused-planning-feedback.json"),
        Path.Combine(Path.GetTempPath(), "unused-variance-feedback.json"),
        Path.Combine(Path.GetTempPath(), "unused-validation-summary.json")));

    var workspace = service.GetWorkspace();
    var adapter = workspace.SchedulingAdapter;

    AssertEqual("ADVENTUREWORKS-BOUNDED-SCHEDULING-ADAPTER-PROFILE-V1", adapter.AdapterProfileID, "adapter profile read model");
    AssertEqual("PublicDemoOnly", adapter.MappingConfidence, "adapter mapping confidence");
    AssertTrue(adapter.GovernancePolicies.Any(item => item.PolicyArea == "控制点策略" && item.DdaeResponsibility.Contains("治理意图", StringComparison.Ordinal)), "DDAE should publish control point governance policy");
    AssertTrue(adapter.AdapterMetadata.Any(item => item.FieldName == "AdapterProfileID" && item.ForbiddenUse.Contains("DDAE executable routing authority", StringComparison.Ordinal)), "adapter metadata should forbid DDAE executable routing authority");
    AssertTrue(adapter.AdapterMetadata.Any(item => item.FieldName == "MaterialConstraintsMode" && item.Value == "OmittedForPublicDemo" && item.ForbiddenUse.Contains("active material-feasible scheduling", StringComparison.Ordinal)), "active material constraints mode should be omitted and non-authoritative");
    AssertTrue(adapter.NonDdaeOwnedExecutionMetadata.Any(item => item.ExecutionObject.Contains("资源日历", StringComparison.Ordinal) && item.ForbiddenUse.Contains("must not author executable calendars", StringComparison.Ordinal)), "resource calendars should stay non-DDAE-owned");
    AssertTrue(adapter.FeedbackBoundary.Contains("不能修改 DDAE 已批准运营模型", StringComparison.Ordinal), "feedback should stay governance context");
}

static void TestAdventureWorksProductDemoProfileExposesDdaeGovernanceReadModel()
{
    var service = new AdventureWorksProductDemoProfileService();
    var workspace = service.GetWorkspace();

    AssertEqual("ADVENTUREWORKS_PRODUCT_DEMO_V1", workspace.Profile.ProfileID, "product demo profile id");
    AssertEqual("ProductDemoMode", workspace.Profile.Mode, "product demo mode");
    AssertEqual("ProductDemoOnly", workspace.Profile.MappingConfidence, "product demo mapping confidence");
    AssertEqual("PUBLIC-DEMO-GOLDEN-DATA-V1", workspace.Profile.BasePackageID, "product demo base package");
    AssertTrue(workspace.DdaeAuthorityRows.Any(item => item.GroupName == "DDMRPBufferSettings" && item.BusinessObject == "AW-PRODUCT-747"), "DDAE DemoAuthority should expose DDMRP buffer settings");
    AssertTrue(workspace.DdaeAuthorityRows.Any(item => item.GroupName == "ControlPointGovernance" && item.ValueSummary.Contains("AW-LOCATION-10", StringComparison.Ordinal)), "DDAE DemoAuthority should expose control point governance");
    AssertTrue(workspace.DdaeAuthorityRows.All(item => !string.IsNullOrWhiteSpace(item.SourceClass) && !string.IsNullOrWhiteSpace(item.EvidenceRef)), "DDAE DemoAuthority rows should carry source class and evidence refs");
    AssertTrue(workspace.PanelPolicies.Any(item => item.PanelID == "scenario-run-panel" && item.Handling == "Placeholder"), "unadapted scenario run panel should be placeholder");
    AssertTrue(workspace.PanelPolicies.Any(item => item.PanelID == "public-demo-golden-loop-panel" && item.Handling == "ProductDemoMode"), "public demo loop should be ProductDemoMode");
    AssertTrue(workspace.PlaceholderPanels.Any(item => item.Contains("场景运行", StringComparison.Ordinal)), "placeholder panels should use Chinese business labels");
    AssertTrue(workspace.Validation.All(item => item.Status == "通过"), "ProductDemo profile validation should pass for the reviewed draft fixture");
    AssertTrue(workspace.FallbackToCoreSampleBlocked, "DDAE core sample fallback should be blocked");
    AssertTrue(workspace.FeedbackMutationBlocked, "SDBR feedback should not mutate approved master settings");
    AssertTrue(workspace.NetworkCandidateMutationBlocked, "Network candidates should not mutate approved master settings");
    AssertTrue(workspace.NonClaims.Any(item => item.Contains("不声明 ProductionValidated", StringComparison.Ordinal)), "non-claims should preserve ProductionValidated boundary");
}

static void TestContractRepositoryPathResolverPrefersConfiguredRoot()
{
    var root = Path.Combine(Path.GetTempPath(), $"ddae-contract-root-{Guid.NewGuid():N}");
    var configuredRoot = Path.Combine(root, "configured-contract");
    Directory.CreateDirectory(configuredRoot);
    try
    {
        var resolved = ContractRepositoryPathResolver.Resolve(root, configuredRoot);

        AssertEqual(Path.GetFullPath(configuredRoot), resolved, "configured contract repository root");
    }
    finally
    {
        Directory.Delete(root, recursive: true);
    }
}

static void TestContractRepositoryPathResolverDiscoversSiblingRepository()
{
    var workspace = Path.Combine(Path.GetTempPath(), $"ddae-contract-sibling-{Guid.NewGuid():N}");
    var applicationRoot = Path.Combine(workspace, "DDAE", "src", "AdaptiveSopDdsop.Web");
    var contractRoot = Path.Combine(workspace, "DDAE_INTERFACE_CONTRACT");
    Directory.CreateDirectory(applicationRoot);
    Directory.CreateDirectory(contractRoot);
    try
    {
        var resolved = ContractRepositoryPathResolver.Resolve(applicationRoot, configuredRoot: null);

        AssertEqual(Path.GetFullPath(contractRoot), resolved, "sibling contract repository root");
    }
    finally
    {
        Directory.Delete(workspace, recursive: true);
    }
}

static void TestDdsopRuntimePlanningInputCorrelatesFeedback()
{
    var configService = new DdsopConfigInboundContractService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));
    var service = new DdsopRuntimePlanningInputContractService(configService);
    var message = service.Build(new DdsopRuntimePlanningInputRequest(12, new DateOnly(2026, 6, 26)));
    var runtimeLedger = new DdsopRuntimeDeliveryLedger();
    var feedbackLedger = new DdsopFeedbackInboundLedger();
    var deliveryId = message.Payload.OutputExpectations.DeliveryLedgerCorrelationID;
    runtimeLedger.RegisterPackage(message);

    var feedbackJson = $$"""
        {
          "ContractID": "DDSOP-FEEDBACK-OUTBOUND-V1",
          "ContractVersion": "1.0.0",
          "MessageID": "SDBR-MSG-RPI-FEEDBACK-001",
          "MessageType": "PlanningRunFeedbackPublished",
          "SourceSystem": "SDBR",
          "TargetSystem": "DDAE",
          "IdempotencyKey": "SDBR:RPI-FEEDBACK:001",
          "Payload": {
            "FeedbackType": "PlanningRunFeedback",
            "PlanningRunID": "SDBR-RUN-RPI-001",
            "OperatingModelConfigurationID": "{{message.Payload.FrozenDdsopConfiguration.OperatingModelConfigurationID}}",
            "OperatingModelFingerprint": "{{message.Payload.FrozenDdsopConfiguration.OperatingModelFingerprint}}",
            "MasterDataVersionID": "PUBLIC-DEMO-GOLDEN-DATA-V1",
            "OperationalStateSnapshotID": "DDAE-OPS-SNAPSHOT-RPI-001",
            "RunStatus": "Completed",
            "SolverStatus": "Feasible",
            "OperationalMetrics": { "OverallStatus": "Green" },
            "RecommendedDDSOPReviewTopics": [
              { "Topic": "Review buffer policy", "IsApprovedConfigurationChange": false }
            ],
            "DataCoverageIssues": []
          }
        }
        """;
    var ack = feedbackLedger.Accept(feedbackJson);
    var record = feedbackLedger.Records.Single(item => item.IdempotencyKey == ack.IdempotencyKey);
    var correlated = runtimeLedger.CorrelateFeedback(deliveryId, record);

    AssertEqual("Accepted", ack.ProcessingStatus, "runtime feedback should be accepted");
    AssertTrue(correlated is not null, "runtime ledger should correlate feedback by delivery id");
    AssertEqual(message.Payload.PackageIdentity.RuntimePlanningInputPackageID, correlated!.RuntimePlanningInputPackageID, "runtime package id should stay on ledger");
    AssertEqual(1, correlated.FeedbackCorrelations.Count, "one feedback correlation should be recorded");
    AssertEqual("SDBR-RUN-RPI-001", correlated.FeedbackCorrelations[0].PlanningRunID, "planning run should be correlated");
    AssertEqual(0, feedbackLedger.Records.Single().Interpretation.ApprovedConfigurationChangeCount, "feedback should remain review context");
}

static void TestPublicDemoGoldenLoopServiceWritesHandoffPayload()
{
    var root = Path.Combine(Path.GetTempPath(), $"ddae-public-demo-{Guid.NewGuid():N}");
    var handoffRoot = Path.Combine(root, "handoff");
    Directory.CreateDirectory(root);
    try
    {
        WriteJson(root, "manifest.json", """
            {
              "PackageID": "PUBLIC-DEMO-GOLDEN-DATA-V1",
              "PackageChecksum": "test-checksum",
              "PackageFrozenAt": "2026-06-29T16:48:06+08:00",
              "FileRoleMap": {
                "items.json": "Demo item/product master proxy",
                "locations.json": "Demo location/work-center/resource proxy",
                "item-locations.json": "Demo item-location and quantity proxy",
                "uoms.json": "Demo UOM and item-UOM proxy",
                "boms.json": "Demo material-structure proxy",
                "work-orders.json": "Demo work-order proxy",
                "routings.json": "Demo routing/operation proxy",
                "capacities.json": "Demo capacity/calendar proxy",
                "crosswalk.json": "Demo DDAE/SDBR object crosswalk",
                "non-claims.md": "Package-level non-claims"
              },
              "RowCountsByFile": {
                "items.json": 1,
                "locations.json": 1,
                "item-locations.json": 1,
                "uoms.json": 1,
                "boms.json": 1,
                "work-orders.json": 1,
                "routings.json": 1,
                "capacities.json": 1,
                "crosswalk.json": 1,
                "non-claims.md": null
              },
              "ChecksumsByFile": {
                "items.json": "i",
                "locations.json": "l",
                "item-locations.json": "il",
                "uoms.json": "u",
                "boms.json": "b",
                "work-orders.json": "w",
                "routings.json": "r",
                "capacities.json": "c",
                "crosswalk.json": "x",
                "non-claims.md": "n"
              }
            }
            """);
        WriteJson(root, "items.json", """[{ "DemoItemID": "AW-PRODUCT-1", "Name": "Adjustable Race" }]""");
        WriteJson(root, "locations.json", """[{ "DemoLocationID": "AW-LOCATION-1", "Name": "Tool Crib" }]""");
        WriteJson(root, "item-locations.json", """[{ "DemoItemID": "AW-PRODUCT-1", "DemoLocationID": "AW-LOCATION-1", "Quantity": 408, "QuantityUom": "EA" }]""");
        WriteJson(root, "capacities.json", """[{ "DemoLocationID": "AW-LOCATION-1", "Availability": 96.0 }]""");
        foreach (var file in new[] { "uoms.json", "boms.json", "work-orders.json", "routings.json", "crosswalk.json" })
        {
            WriteJson(root, file, "[]");
        }

        File.WriteAllText(Path.Combine(root, "non-claims.md"), "No production validation confidence");
        var options = new PublicDemoGoldenLoopOptions(
            root,
            "test-checksum",
            Path.Combine(handoffRoot, "ddae-to-sdbr", "ddsop-config-inbound-v1-payload.json"),
            Path.Combine(handoffRoot, "sdbr-to-ddae", "planning-run-feedback.json"),
            Path.Combine(handoffRoot, "sdbr-to-ddae", "variance-analysis-feedback.json"),
            Path.Combine(handoffRoot, "sdbr-to-ddae", "validation-summary.json"));
        var service = new PublicDemoGoldenLoopService(options);

        var workspace = service.GetWorkspace();
        var write = service.WritePayload();
        var payloadJson = File.ReadAllText(options.DdaeToSdbrPayloadPath);

        AssertTrue(workspace.PackageChecksumMatches, "public demo package checksum should match expected checksum");
        AssertEqual("PublicDemoOnly", workspace.MappingConfidence, "public demo mapping confidence");
        AssertEqual("ADVENTUREWORKS-BOUNDED-SCHEDULING-ADAPTER-PROFILE-V1", workspace.SchedulingAdapter.AdapterProfileID, "public demo should expose AdventureWorks adapter profile");
        AssertTrue(workspace.SchedulingAdapter.AdapterMetadata.Any(item => item.FieldName == "MaterialConstraintsMode" && item.Value == "OmittedForPublicDemo" && item.ForbiddenUse.Contains("active material-feasible scheduling", StringComparison.Ordinal)), "public demo adapter metadata should keep active material feasibility out of DDAE authority");
        AssertTrue(workspace.SchedulingAdapter.NonDdaeOwnedExecutionMetadata.Any(item => item.ExecutionObject.Contains("可执行 routing", StringComparison.Ordinal)), "public demo should explain executable routing is non-DDAE-owned");
        AssertEqual("AW-PRODUCT-1", workspace.PackageContext.SampleItem, "public demo sample item should come from package item-location data");
        AssertEqual("AW-LOCATION-1", workspace.PackageContext.SampleLocation, "public demo sample location should come from package item-location data");
        AssertTrue(workspace.ReviewedMappings.Any(item => item.DemoObject == "AW-PRODUCT-1 @ AW-LOCATION-1"), "public demo reviewed mappings should use package sample object");
        AssertTrue(!workspace.ReviewedMappings.Any(item => item.DemoObject.Contains("PART-FPGA-SPACE", StringComparison.Ordinal) || item.DemoObject.Contains("WH-ELEC-QA", StringComparison.Ordinal)), "public demo reviewed mappings should not expose legacy satellite fixture objects");
        AssertEqual("DDSOP-CONFIG-INBOUND-V1", write.Payload.ContractID, "public demo payload contract id");
        AssertEqual("DDSOP-OMC-PUBLIC-DEMO-V1", write.Payload.Payload.OperatingModelConfigurationID, "public demo OMC id");
        AssertEqual("MODEL_RESTRUCTURE", write.Payload.Payload.ChangeReason.ReasonCode, "public demo change reason should use schema enum");
        AssertTrue(write.Payload.Payload.ChangeReason.Description.Contains("DemoFixture / ReviewedEvidence / Controlled Contract Golden Loop Demo / MappingConfidence = PublicDemoOnly", StringComparison.Ordinal), "public demo labels should stay in change reason description");
        AssertTrue(write.Payload.Payload.SchedulingConfiguration.ControlPoints.Any(item => item.ResourceID == "AW-LOCATION-1"), "public demo payload should include package sample location control point");
        AssertTrue(write.Payload.Payload.DDMRPConfiguration.DecouplingPoints.Any(item => item.ItemID == "AW-PRODUCT-1" && item.LocationID == "AW-LOCATION-1"), "public demo payload should include package sample item-location decoupling point");
        AssertTrue(!payloadJson.Contains("PART-FPGA-SPACE", StringComparison.Ordinal), "public demo payload should not include legacy satellite fixture item");
        AssertTrue(!payloadJson.Contains("WH-ELEC-QA", StringComparison.Ordinal), "public demo payload should not include legacy satellite fixture location");
        AssertTrue(File.Exists(options.DdaeToSdbrPayloadPath), "public demo payload should be written to handoff path");
        AssertTrue(!payloadJson.Contains("CONTROLLED_CONTRACT_DEMO", StringComparison.Ordinal), "public demo payload should not use invalid reason code");
        AssertTrue(!payloadJson.Contains("ProductionValidated", StringComparison.Ordinal), "public demo payload should not claim ProductionValidated");
        AssertTrue(!payloadJson.Contains("Business Golden Loop Readiness", StringComparison.Ordinal), "public demo payload should not claim Business Golden Loop readiness");

        Directory.CreateDirectory(Path.GetDirectoryName(options.PlanningRunFeedbackPath)!);
        File.WriteAllText(options.PlanningRunFeedbackPath, """
            {
              "ContractID": "DDSOP-FEEDBACK-OUTBOUND-V1",
              "ContractVersion": "1.0.0",
              "MessageID": "SDBR-MSG-PLANNING-RUN-FEEDBACK-PUBLIC-DEMO-SDBR-RUN-EDE8051B20A6",
              "MessageType": "PlanningRunFeedbackPublished",
              "SourceSystem": "SDBR",
              "TargetSystem": "DDAE",
              "IdempotencyKey": "SDBR:PlanningRunFeedback:PUBLIC-DEMO-SDBR-RUN-EDE8051B20A6",
              "Payload": {
                "FeedbackType": "PlanningRunFeedback",
                "PlanningRunID": "PUBLIC-DEMO-SDBR-RUN-EDE8051B20A6",
                "OperatingModelConfigurationID": "DDSOP-OMC-PUBLIC-DEMO-V1",
                "OperatingModelFingerprint": "sha256:demo-fingerprint",
                "MasterDataVersionID": "PUBLIC-DEMO-GOLDEN-DATA-V1",
                "OperationalStateSnapshotID": "PUBLIC-DEMO-GOLDEN-DATA-V1-SNAPSHOT",
                "RunStatus": "Completed",
                "SolverStatus": "Feasible",
                "OperationalMetrics": { "OverallStatus": "Green" },
                "RecommendedDDSOPReviewTopics": [],
                "DataCoverageIssues": []
              }
            }
            """);
        File.WriteAllText(options.VarianceAnalysisFeedbackPath, """
            {
              "ContractID": "DDSOP-FEEDBACK-OUTBOUND-V1",
              "ContractVersion": "1.0.0",
              "MessageID": "SDBR-MSG-VARIANCE-FEEDBACK-PUBLIC-DEMO-SDBR-RUN-EDE8051B20A6",
              "MessageType": "VarianceAnalysisFeedbackPublished",
              "SourceSystem": "SDBR",
              "TargetSystem": "DDAE",
              "IdempotencyKey": "SDBR:VarianceAnalysisFeedback:PUBLIC-DEMO-SDBR-RUN-EDE8051B20A6",
              "Payload": {
                "FeedbackType": "VarianceAnalysisFeedback",
                "PlanningRunID": "PUBLIC-DEMO-SDBR-RUN-EDE8051B20A6",
                "OperatingModelConfigurationID": "DDSOP-OMC-PUBLIC-DEMO-V1",
                "OperatingModelFingerprint": "sha256:demo-fingerprint",
                "MasterDataVersionID": "PUBLIC-DEMO-GOLDEN-DATA-V1",
                "OperationalStateSnapshotID": "PUBLIC-DEMO-GOLDEN-DATA-V1-SNAPSHOT",
                "OverallStatus": "Green",
                "ReliabilityStatus": "Green",
                "SpeedStatus": "Green",
                "StabilityStatus": "Green",
                "RecommendedDDSOPReviewTopics": [],
                "DataCoverageIssues": []
              }
            }
            """);
        File.WriteAllText(options.ValidationSummaryPath, """
            {
              "DemoRunID": "PUBLIC-DEMO-SDBR-RUN-EDE8051B20A6",
              "FrozenConfiguration": {
                "OperatingModelConfigurationID": "DDSOP-OMC-PUBLIC-DEMO-V1",
                "OperatingModelFingerprint": "sha256:demo-fingerprint"
              },
              "Labels": [
                "DemoFixture",
                "ReviewedEvidence",
                "Controlled Contract Golden Loop Demo",
                "MappingConfidence = PublicDemoOnly"
              ],
              "MappingConfidence": "PublicDemoOnly",
              "RunStatus": "Completed",
              "ValidationStatus": "AcceptedForDemo"
            }
            """);
        var feedbackWorkspace = service.GetWorkspace();
        var planningFeedback = feedbackWorkspace.Feedback.Single(item => item.FeedbackName == "PlanningRunFeedback");
        var varianceFeedback = feedbackWorkspace.Feedback.Single(item => item.FeedbackName == "VarianceAnalysisFeedback");
        var validationSummary = feedbackWorkspace.Feedback.Single(item => item.FeedbackName == "ValidationSummary");
        AssertEqual("PUBLIC-DEMO-SDBR-RUN-EDE8051B20A6", planningFeedback.PlanningRunID, "public demo page should interpret planning run id");
        AssertEqual("Completed", planningFeedback.RunStatus, "public demo page should interpret planning run status");
        AssertEqual("Feasible", planningFeedback.SolverStatus, "public demo page should interpret solver status");
        AssertEqual("sha256:demo-fingerprint", planningFeedback.OperatingModelFingerprint, "public demo page should interpret frozen fingerprint");
        AssertEqual("Green", varianceFeedback.OverallStatus, "public demo page should interpret variance overall status");
        AssertEqual("Green", varianceFeedback.ReliabilityStatus, "public demo page should interpret reliability status");
        AssertEqual("Green", varianceFeedback.SpeedStatus, "public demo page should interpret speed status");
        AssertEqual("Green", varianceFeedback.StabilityStatus, "public demo page should interpret stability status");
        AssertEqual("PublicDemoOnly", validationSummary.MappingConfidence, "public demo page should interpret validation mapping confidence");
    }
    finally
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }
}

static void TestIntegrationContractEndpointsAndRemovedOptimizationPath()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var program = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Program.cs"));

    AssertTrue(program.Contains("AddSingleton<DdsopConfigInboundContractService>", StringComparison.Ordinal), "config contract service should be registered");
    AssertTrue(program.Contains("AddSingleton<DdsopFeedbackInboundLedger>", StringComparison.Ordinal), "feedback ledger should be registered");
    AssertTrue(program.Contains("AddSingleton<DdsopRuntimePlanningInputContractService>", StringComparison.Ordinal), "runtime planning input service should be registered");
    AssertTrue(program.Contains("AddSingleton<DdsopRuntimeDeliveryLedger>", StringComparison.Ordinal), "runtime delivery ledger should be registered");
    AssertTrue(program.Contains("AddSingleton<PublicDemoGoldenLoopService>", StringComparison.Ordinal), "public demo golden loop service should be registered");
    AssertTrue(program.Contains("AddSingleton<AdventureWorksProductDemoProfileService>", StringComparison.Ordinal), "AdventureWorks ProductDemo profile service should be registered");
    AssertTrue(program.Contains("AddSingleton<ProductionSupplierIdentitySourceInboundLedger>", StringComparison.Ordinal), "supplier identity ledger should be registered");
    AssertTrue(program.Contains("AddSingleton<ProductionInventoryQualityInboundLedger>", StringComparison.Ordinal), "inventory quality ledger should be registered");
    AssertTrue(program.Contains("AddSingleton<SdbrExecutionObjectEvidenceInboundLedger>", StringComparison.Ordinal), "execution evidence ledger should be registered");
    AssertTrue(program.Contains("/api/integration-contracts/ddsop-config-inbound-v1", StringComparison.Ordinal), "config endpoint should be exposed");
    AssertTrue(program.Contains("/api/integration-contracts/ddsop-feedback-outbound-v1", StringComparison.Ordinal), "feedback endpoint should be exposed");
    AssertTrue(program.Contains("/api/integration-contracts/ddsop-runtime-planning-input-v1", StringComparison.Ordinal), "runtime planning input endpoint should be exposed");
    AssertTrue(program.Contains("/api/integration-contracts/ddsop-runtime-planning-input-v1/{deliveryLedgerCorrelationId}/feedback", StringComparison.Ordinal), "runtime feedback correlation endpoint should be exposed");
    AssertTrue(program.Contains("/api/public-demo-golden-loop", StringComparison.Ordinal), "public demo golden loop workspace endpoint should be exposed");
    AssertTrue(program.Contains("/api/adventureworks-product-demo-v1", StringComparison.Ordinal), "AdventureWorks ProductDemo endpoint should be exposed");
    AssertTrue(program.Contains("/api/public-demo-golden-loop/write-payload", StringComparison.Ordinal), "public demo payload handoff endpoint should be exposed");
    AssertTrue(program.Contains("/api/integration-contracts/production-supplier-identity-source-v1", StringComparison.Ordinal), "supplier identity endpoint should be exposed");
    AssertTrue(program.Contains("/api/integration-contracts/production-inventory-quality-evidence-v1", StringComparison.Ordinal), "inventory quality endpoint should be exposed");
    AssertTrue(program.Contains("/api/integration-contracts/sdbr-execution-object-evidence-v1", StringComparison.Ordinal), "execution evidence endpoint should be exposed");
    AssertTrue(program.Contains("AddSingleton<HistoryReviewWorkspaceService>", StringComparison.Ordinal), "history review service should be registered");
    AssertTrue(program.Contains("ICurrentBaselineDataSource, SeedCurrentBaselineDataSource", StringComparison.Ordinal), "current baseline source should be registered");
    AssertTrue(program.Contains("/api/history-review", StringComparison.Ordinal), "history review endpoint should be exposed");
    AssertTrue(program.Contains("/api/current-baselines/candidate", StringComparison.Ordinal), "current baseline candidate endpoint should be exposed");
    AssertTrue(program.Contains("/api/current-baselines/{snapshotId}/audit", StringComparison.Ordinal), "current baseline audit endpoint should be exposed");
    AssertTrue(program.Contains("/api/scenario-runs/compare", StringComparison.Ordinal), "scenario comparison endpoint should be exposed");
    AssertTrue(program.Contains("/api/coordination-items/{itemId}/status", StringComparison.Ordinal), "coordination status endpoint should be exposed");
    AssertTrue(program.Contains("/api/coordination-items/{itemId}/decision", StringComparison.Ordinal), "coordination decision endpoint should be exposed");
    AssertTrue(program.Contains("/api/coordination-items/{itemId}/outcome", StringComparison.Ordinal), "coordination outcome endpoint should be exposed");
    AssertTrue(program.Contains("/api/coordination-items/{itemId}/audit", StringComparison.Ordinal), "coordination audit endpoint should be exposed");
    AssertTrue(!program.Contains("/api/scenario-runs/optimize", StringComparison.Ordinal), "old scenario optimization endpoint should be removed from main");
    AssertTrue(!program.Contains("ScenarioOptimizationService", StringComparison.Ordinal), "old scenario optimization service should not be registered");
    AssertTrue(!program.Contains("IOptimizationSolver", StringComparison.Ordinal), "solver adapter should not be wired into DDS&OP main");
}

static void WriteJson(string directory, string fileName, string json)
{
    File.WriteAllText(Path.Combine(directory, fileName), json);
}

static int CountExactOccurrences(string source, string value)
{
    var count = 0;
    var offset = 0;
    while ((offset = source.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
    {
        count++;
        offset += value.Length;
    }

    return count;
}

static string OpeningTagContaining(string source, string marker)
{
    var markerIndex = source.IndexOf(marker, StringComparison.Ordinal);
    if (markerIndex < 0)
    {
        return string.Empty;
    }

    var tagStart = source.LastIndexOf('<', markerIndex);
    var tagEnd = source.IndexOf('>', markerIndex);
    return tagStart >= 0 && tagEnd > tagStart
        ? source.Substring(tagStart, tagEnd - tagStart + 1)
        : string.Empty;
}

static string SourceFunctionBody(string source, string functionName)
{
    var functionStart = source.IndexOf($"function {functionName}(", StringComparison.Ordinal);
    if (functionStart < 0)
    {
        return string.Empty;
    }

    var bodyStart = source.IndexOf('{', functionStart);
    if (bodyStart < 0)
    {
        return string.Empty;
    }

    var depth = 0;
    for (var index = bodyStart; index < source.Length; index++)
    {
        if (source[index] == '{')
        {
            depth++;
        }
        else if (source[index] == '}' && --depth == 0)
        {
            return source.Substring(bodyStart + 1, index - bodyStart - 1);
        }
    }

    return string.Empty;
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

static (CurrentBaselineService Baseline, CoordinationLedgerService Coordination) EnsureInternalSqliteSchemas(string databasePath)
{
    var seed = SeedData.Create();
    var workspaceDataSource = new SeedScenarioWorkspaceDataSource(seed);
    var baseline = new CurrentBaselineService(new SeedCurrentBaselineDataSource(seed), databasePath);
    var coordination = new CoordinationLedgerService(databasePath);
    var preview = new ScenarioRunPreviewService(workspaceDataSource);
    var comparison = new ScenarioComparisonService(baseline, preview, new SeedScenarioAssumptionSource());
    var persistence = new ScenarioRunPersistenceService(preview, comparison, databasePath);
    _ = new MasterSettingsGovernanceService(workspaceDataSource, preview, persistence, databasePath);
    return (baseline, coordination);
}

static void SeedRepairCoordinationItem(
    string databasePath,
    string itemId,
    string itemNumber,
    string title,
    string owner,
    string createdBy,
    params string[] auditMessages)
{
    using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}");
    connection.Open();
    using (var foreignKeys = connection.CreateCommand())
    {
        foreignKeys.CommandText = "PRAGMA foreign_keys = ON;";
        foreignKeys.ExecuteNonQuery();
    }
    using (var command = connection.CreateCommand())
    {
        command.CommandText = """
            INSERT INTO coordination_items (
                item_id, item_number, title, impact_objects_json, related_scenario_run_id,
                related_master_setting_change_id, service_impact, inventory_impact, cash_impact,
                risk_impact, decision_required, owner, due_date, escalation_level, next_review_date,
                status, decision, decision_rationale, actual_outcome, created_by, created_at_utc, updated_at_utc)
            VALUES (
                $item_id, $item_number, $title, $impact_objects_json, NULL,
                NULL, $service_impact, $inventory_impact, NULL,
                $risk_impact, $decision_required, $owner, $due_date, $escalation_level, $next_review_date,
                'Open', NULL, NULL, NULL, $created_by, $created_at_utc, $updated_at_utc);
            """;
        command.Parameters.AddWithValue("$item_id", itemId);
        command.Parameters.AddWithValue("$item_number", itemNumber);
        command.Parameters.AddWithValue("$title", title);
        command.Parameters.AddWithValue("$impact_objects_json", JsonSerializer.Serialize(new[] { "烟测控制范围" }));
        command.Parameters.AddWithValue("$service_impact", "服务影响待处理");
        command.Parameters.AddWithValue("$inventory_impact", "库存影响待处理");
        command.Parameters.AddWithValue("$risk_impact", "本地烟测数据");
        command.Parameters.AddWithValue("$decision_required", "清理精确烟测记录");
        command.Parameters.AddWithValue("$owner", owner);
        command.Parameters.AddWithValue("$due_date", "2026-07-18");
        command.Parameters.AddWithValue("$escalation_level", "本地烟测");
        command.Parameters.AddWithValue("$next_review_date", "2026-07-17");
        command.Parameters.AddWithValue("$created_by", createdBy);
        command.Parameters.AddWithValue("$created_at_utc", "2026-07-14T08:00:00.0000000+00:00");
        command.Parameters.AddWithValue("$updated_at_utc", "2026-07-14T08:00:00.0000000+00:00");
        command.ExecuteNonQuery();
    }

    for (var index = 0; index < auditMessages.Length; index++)
    {
        using var audit = connection.CreateCommand();
        audit.CommandText = """
            INSERT INTO coordination_item_audit_events (
                event_id, item_id, sequence, event_type, actor, message, created_at_utc)
            VALUES ($event_id, $item_id, $sequence, 'SmokeAudit', $actor, $message, $created_at_utc);
            """;
        audit.Parameters.AddWithValue("$event_id", $"{itemId}-audit-{index + 1}");
        audit.Parameters.AddWithValue("$item_id", itemId);
        audit.Parameters.AddWithValue("$sequence", index + 1);
        audit.Parameters.AddWithValue("$actor", createdBy);
        audit.Parameters.AddWithValue("$message", auditMessages[index]);
        audit.Parameters.AddWithValue("$created_at_utc", "2026-07-14T08:05:00.0000000+00:00");
        audit.ExecuteNonQuery();
    }
}

static void SeedRepairBaseline(
    string databasePath,
    string snapshotId,
    string snapshotNumber,
    string createdBy,
    string? auditMessage,
    int? auditSequence,
    string? note = null)
{
    using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}");
    connection.Open();
    using (var foreignKeys = connection.CreateCommand())
    {
        foreignKeys.CommandText = "PRAGMA foreign_keys = ON;";
        foreignKeys.ExecuteNonQuery();
    }
    using (var command = connection.CreateCommand())
    {
        command.CommandText = """
            INSERT INTO current_baseline_snapshots (
                snapshot_id, snapshot_number, status, as_of_utc, master_setting_version,
                created_by, note, created_at_utc, sections_json, payload_json, evidence_label)
            VALUES (
                $snapshot_id, $snapshot_number, 'Frozen', $as_of_utc, 'MASTER-SMOKE-V1',
                $created_by, $note, $created_at_utc, '[]', '{}', 'LocalSmoke');
            """;
        command.Parameters.AddWithValue("$snapshot_id", snapshotId);
        command.Parameters.AddWithValue("$snapshot_number", snapshotNumber);
        command.Parameters.AddWithValue("$created_by", createdBy);
        command.Parameters.AddWithValue("$note", note is null ? DBNull.Value : note);
        command.Parameters.AddWithValue("$as_of_utc", "2026-07-14T08:00:00.0000000+00:00");
        command.Parameters.AddWithValue("$created_at_utc", "2026-07-14T08:00:00.0000000+00:00");
        command.ExecuteNonQuery();
    }

    if (auditMessage is null || auditSequence is null)
    {
        return;
    }

    using var audit = connection.CreateCommand();
    audit.CommandText = """
        INSERT INTO current_baseline_audit_events (
            event_id, snapshot_id, sequence, event_type, message, created_at_utc, payload_json)
        VALUES ($event_id, $snapshot_id, $sequence, 'BaselineFrozen', $message, $created_at_utc, NULL);
        """;
    audit.Parameters.AddWithValue("$event_id", $"{snapshotId}-audit-{auditSequence.Value}");
    audit.Parameters.AddWithValue("$snapshot_id", snapshotId);
    audit.Parameters.AddWithValue("$sequence", auditSequence.Value);
    audit.Parameters.AddWithValue("$message", auditMessage);
    audit.Parameters.AddWithValue("$created_at_utc", "2026-07-14T08:01:00.0000000+00:00");
    audit.ExecuteNonQuery();
}

static object? ReadSqliteScalar(
    string databasePath,
    string sql,
    params (string Name, object? Value)[] parameters)
{
    using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}");
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = sql;
    foreach (var parameter in parameters)
    {
        command.Parameters.AddWithValue(parameter.Name, parameter.Value ?? DBNull.Value);
    }
    return command.ExecuteScalar();
}

static string ReadRepairDatabaseState(string databasePath)
{
    var queries = new[]
    {
        "SELECT * FROM coordination_items ORDER BY item_id;",
        "SELECT * FROM coordination_item_audit_events ORDER BY item_id, sequence;",
        "SELECT * FROM current_baseline_snapshots ORDER BY snapshot_id;",
        "SELECT * FROM current_baseline_audit_events ORDER BY snapshot_id, sequence;",
        "SELECT * FROM local_data_repairs ORDER BY repair_id;",
        "SELECT name, sql FROM sqlite_master WHERE type = 'trigger' AND name LIKE 'trg_current_baseline%' ORDER BY name;"
    };
    var rows = new List<string>();
    using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}");
    connection.Open();
    foreach (var query in queries)
    {
        rows.Add(query);
        using var command = connection.CreateCommand();
        command.CommandText = query;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var values = new string[reader.FieldCount];
            for (var index = 0; index < reader.FieldCount; index++)
            {
                values[index] = reader.IsDBNull(index) ? "<NULL>" : Convert.ToString(reader.GetValue(index))!;
            }
            rows.Add(JsonSerializer.Serialize(values));
        }
    }
    return string.Join("\n", rows);
}

static void AssertBaselineUpdateTriggerBlocks(string databasePath, string snapshotId)
{
    var blocked = false;
    using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}");
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = "UPDATE current_baseline_snapshots SET created_by = $created_by WHERE snapshot_id = $snapshot_id;";
    command.Parameters.AddWithValue("$created_by", "不应写入");
    command.Parameters.AddWithValue("$snapshot_id", snapshotId);
    try
    {
        command.ExecuteNonQuery();
    }
    catch (Microsoft.Data.Sqlite.SqliteException)
    {
        blocked = true;
    }
    AssertTrue(blocked, "restored baseline no-update trigger should still enforce immutability");
}

static void TestMasterSettingsGovernancePreservesDecisionPackageMetadata()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-master-settings-{Guid.NewGuid():N}.db");
    try
    {
        var source = new SeedScenarioWorkspaceDataSource(SeedData.Create());
        var baselineService = new CurrentBaselineService(new SeedCurrentBaselineDataSource(SeedData.Create()), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("DDS&OP 计划员", "治理来源基线"));
        var comparisonService = new ScenarioComparisonService(
            baselineService,
            new ScenarioRunPreviewService(source),
            new SeedScenarioAssumptionSource());
        var comparison = comparisonService.Compare(new ScenarioComparisonRequest(
            frozen.SnapshotId,
            new ExternalScenarioDefinition(
                "RUN-001",
                "治理来源场景",
                Metadata: new ScenarioAssumptionMetadata(
                    "Manual",
                    null,
                    null,
                    "DDS&OP 计划员",
                    "2026-07-15T08:00:00Z",
                    "2026-07-20",
                    "2026-08-16",
                    "治理来源场景",
                    "人工录入：治理测试")),
            new[] { new ResponseConfiguration("RESP-001", "临时能力", new ScenarioRunParameterSet(CapacityAdjustments: new[] { new ResourceCapacityAdjustment("RES-TVAC", 3, 1.2m, "临时能力") })) },
            12));
        var selectedComparison = comparison.ResponseCases.Single();
        var savedRun = new ScenarioRunSummary(
            "RUN-SAVED-GOV-001", "SR-20260715-0002", "治理来源场景", null, "DDS&OP 计划员", "Saved", "NotSubmitted",
            "2026-07-15T08:00:00Z", 12, null, null, 98m, 1m, 1_000_000m, 90m, 0m, 0, 1,
            frozen.SnapshotId, comparison.ExternalScenario.ScenarioId, "RESP-001");
        var savedDetail = new ScenarioRunDetail(
            savedRun,
            selectedComparison.Preview.Request,
            selectedComparison.Preview with { IsPersisted = true });
        var service = new MasterSettingsGovernanceService(
            source,
            new ScenarioRunPreviewService(source),
            new FixedScenarioRunLineageReader(savedDetail),
            databasePath);
        var generated = service.ProposeFromFrozenComparison(
                selectedComparison,
                frozen,
                savedRun.RunId,
                new GovernanceDecisionContext(
                    frozen.SnapshotId,
                    savedRun.RunId,
                    "运营经理",
                    "执行委员会",
                    "2026-07-20",
                    "2026-08-16",
                    "2026-08-03",
                    "能力红区减少两周",
                    "服务未改善或现金占用超过上限"))
            .Proposals
            .First();

        AssertEqual(frozen.SnapshotId, generated.SourceBaselineId, "generated proposal source baseline");
        AssertEqual(savedRun.RunId, generated.SourceScenarioRunId, "generated proposal source scenario");
        AssertEqual("ScenarioDerived", generated.CreationMethod, "generated proposal creation method");
        AssertEqual("运营经理", generated.Owner, "generated proposal owner");
        AssertEqual("执行委员会", generated.Approver, "generated proposal approver");
        AssertEqual("2026-08-16", generated.EffectiveThrough, "generated proposal expiry");

        var saved = service.SaveChange(new MasterSettingChangeSaveRequest("DDS&OP 计划员", generated));
        var detail = service.GetDetail(saved.ChangeId)!;

        AssertEqual("TemporaryAdjustment", detail.Proposal.ChangeCategory, "governance change category");
        AssertEqual(frozen.SnapshotId, detail.Proposal.SourceBaselineId, "governance source baseline");
        AssertEqual(savedRun.RunId, detail.Proposal.SourceScenarioRunId, "governance source scenario");
        AssertEqual("运营经理", detail.Proposal.Owner, "governance owner");
        AssertEqual("执行委员会", detail.Proposal.Approver, "governance approver");
        AssertEqual("Proposed", saved.Status, "saved decision package must remain proposed");
        AssertTrue(service.GetAuditEvents(saved.ChangeId).All(item => item.EventType != "StatusChanged"), "saving must not auto approve or auto effect the package");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void AssertArgumentRejected(Action action, string label)
{
    try
    {
        action();
    }
    catch (ArgumentException)
    {
        return;
    }

    throw new InvalidOperationException($"{label} should be rejected");
}

internal sealed record LegacyScenarioSource(ValidationData Data);

internal sealed class HistoricalQuantityPoisoningScenarioWorkspaceDataSource : IScenarioWorkspaceDataSource
{
    private readonly SeedScenarioWorkspaceDataSource _inner;

    public HistoricalQuantityPoisoningScenarioWorkspaceDataSource(ValidationData data)
    {
        _inner = new SeedScenarioWorkspaceDataSource(data);
    }

    public ScenarioWorkspaceDataSet Load(ScenarioWorkspaceDataRequest request)
    {
        var data = _inner.Load(request);
        return data with
        {
            Inventory = data.Inventory
                .Select(item => item with { OnHand = item.OnHand + 99_000_000m, OpenSupply = item.OpenSupply + 88_000_000m })
                .ToList(),
            Demand = data.Demand
                .Select(item => item with { BaselineDemand = item.BaselineDemand * 100m })
                .ToList(),
            HistoricalDemand = data.HistoricalDemand
                .Select(item => item with { ActualDemand = 9_999_999m, ForecastDemand = 1m, ServiceLevelPercent = 1m, EndingNetFlow = -9_999_999m })
                .ToList(),
            Resources = data.Resources
                .Select(item => item with { WeeklyAvailableUnits = item.WeeklyAvailableUnits * 100m })
                .ToList(),
            DdmrpParameters = data.DdmrpParameters
                .Select(item => item with
                {
                    Adu = item.Adu * 100m,
                    DecoupledLeadTimeDays = item.DecoupledLeadTimeDays * 10,
                    TopOfRed = item.TopOfRed * 100m,
                    TopOfYellow = item.TopOfYellow * 100m,
                    TopOfGreen = item.TopOfGreen * 100m,
                    Sizing = null
                })
                .ToList(),
            CapacityProtections = (data.CapacityProtections ?? Array.Empty<CapacityProtectionDefinition>())
                .Select(item => item with { ReservePercent = 99m })
                .ToList(),
            TimeBuffers = (data.TimeBuffers ?? Array.Empty<TimeBufferDefinition>())
                .Select(item => item with { BufferDays = item.BufferDays * 100m })
                .ToList(),
            ResourceCalendar = data.ResourceCalendar
                .Select(item => item with { CapacityMultiplier = 0.01m, CalendarNote = "future poison" })
                .ToList(),
            SupplierCapacityWindows = data.SupplierCapacityWindows
                .Select(item => item with { CommittedCapacity = 0m, RiskStatus = "Red" })
                .ToList(),
            ScenarioTemplates = data.ScenarioTemplates
                .Select(item => item with
                {
                    Actions = item.Actions
                        .Select(action => action with { Value = 0.01m })
                        .ToList()
                })
                .ToList()
        };
    }
}

internal sealed class CapacityProtectionRemovingHistoryOperatingFactSource : IHistoryOperatingFactSource
{
    private readonly IHistoryOperatingFactSource _inner;

    public CapacityProtectionRemovingHistoryOperatingFactSource(IHistoryOperatingFactSource inner)
    {
        _inner = inner;
    }

    public HistoryFactSet Load(HistoryFactRequest request)
    {
        var facts = _inner.Load(request);
        return facts with { CapacityProtectionFacts = Array.Empty<HistoricalCapacityProtectionFact>() };
    }
}

internal sealed class CapacityProtectionTransformingHistoryOperatingFactSource : IHistoryOperatingFactSource
{
    private readonly IHistoryOperatingFactSource _inner;
    private readonly Func<IReadOnlyList<HistoricalCapacityProtectionFact>, IReadOnlyList<HistoricalCapacityProtectionFact>> _transform;

    public CapacityProtectionTransformingHistoryOperatingFactSource(
        IHistoryOperatingFactSource inner,
        Func<IReadOnlyList<HistoricalCapacityProtectionFact>, IReadOnlyList<HistoricalCapacityProtectionFact>> transform)
    {
        _inner = inner;
        _transform = transform;
    }

    public HistoryFactSet Load(HistoryFactRequest request)
    {
        var facts = _inner.Load(request);
        var existing = facts.CapacityProtectionFacts ?? Array.Empty<HistoricalCapacityProtectionFact>();
        return facts with { CapacityProtectionFacts = _transform(existing) };
    }
}

internal sealed class DdmrpParametersRemovingHistoryOperatingFactSource : IHistoryOperatingFactSource
{
    private readonly IHistoryOperatingFactSource _inner;

    public DdmrpParametersRemovingHistoryOperatingFactSource(IHistoryOperatingFactSource inner)
    {
        _inner = inner;
    }

    public HistoryFactSet Load(HistoryFactRequest request)
    {
        var facts = _inner.Load(request);
        return facts with { DdmrpParameterFacts = Array.Empty<HistoricalDdmrpParameterFact>() };
    }
}

internal sealed class HistoryEvidenceGapOperatingFactSource : IHistoryOperatingFactSource
{
    private readonly IHistoryOperatingFactSource _inner;
    private readonly string _removedSnapshotId;
    private readonly int _removedTimeWeek;

    public HistoryEvidenceGapOperatingFactSource(
        IHistoryOperatingFactSource inner,
        string removedSnapshotId,
        int removedTimeWeek)
    {
        _inner = inner;
        _removedSnapshotId = removedSnapshotId;
        _removedTimeWeek = removedTimeWeek;
    }

    public HistoryFactSet Load(HistoryFactRequest request)
    {
        var facts = _inner.Load(request);
        return facts with
        {
            DdmrpParameterFacts = (facts.DdmrpParameterFacts ?? Array.Empty<HistoricalDdmrpParameterFact>())
                .Where(item => item.SnapshotId != _removedSnapshotId)
                .ToList(),
            TimeBufferFacts = (facts.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>())
                .Where(item => item.WeekOffset != _removedTimeWeek)
                .ToList()
        };
    }
}

internal sealed class DuplicateCostEventHistoryOperatingFactSource : IHistoryOperatingFactSource
{
    private readonly IHistoryOperatingFactSource _inner;

    public DuplicateCostEventHistoryOperatingFactSource(IHistoryOperatingFactSource inner)
    {
        _inner = inner;
    }

    public HistoryFactSet Load(HistoryFactRequest request)
    {
        var facts = _inner.Load(request);
        var linked = facts.AbnormalCosts.Single(item => item.EventId == "HAC-2026-002");
        return facts with
        {
            AbnormalCosts = facts.AbnormalCosts
                .Append(linked with { WeekOffset = linked.WeekOffset - 1 })
                .ToList()
        };
    }
}

internal sealed class InventoryReconciliationPoisonHistoryOperatingFactSource : IHistoryOperatingFactSource
{
    private readonly IHistoryOperatingFactSource _inner;

    public InventoryReconciliationPoisonHistoryOperatingFactSource(IHistoryOperatingFactSource inner)
    {
        _inner = inner;
    }

    public HistoryFactSet Load(HistoryFactRequest request)
    {
        var facts = _inner.Load(request);
        return facts with
        {
            BufferFacts = facts.BufferFacts
                .Select(item => item.Sku == "AV-COM-201" && item.WeekOffset == -8
                    ? item with { EndingNetFlow = item.EndingNetFlow + 1m }
                    : item)
                .ToList()
        };
    }
}

internal sealed class InvalidHistoricalLeadTimeFactorFactSource : IHistoryOperatingFactSource
{
    private readonly IHistoryOperatingFactSource _inner;

    public InvalidHistoricalLeadTimeFactorFactSource(IHistoryOperatingFactSource inner)
    {
        _inner = inner;
    }

    public HistoryFactSet Load(HistoryFactRequest request)
    {
        var facts = _inner.Load(request);
        return facts with
        {
            DdmrpParameterFacts = (facts.DdmrpParameterFacts ?? Array.Empty<HistoricalDdmrpParameterFact>())
                .Select(item => item.EffectiveThroughWeekOffset != -1
                    ? item
                    : item.Sku == "AV-COM-201"
                        ? item with
                        {
                            Setting = item.Setting with
                            {
                                DecoupledLeadTimeDays = 350,
                                LeadTimeFactor = null
                            }
                        }
                        : item.Sku == "AV-OBC-202"
                            ? item with
                            {
                                SnapshotId = string.Empty,
                                Setting = item.Setting with { ParameterSnapshotId = string.Empty }
                            }
                            : item)
                .ToList()
        };
    }
}

internal sealed class CapacityProtectionRemovingScenarioWorkspaceDataSource : IScenarioWorkspaceDataSource
{
    private readonly SeedScenarioWorkspaceDataSource _inner;

    public CapacityProtectionRemovingScenarioWorkspaceDataSource(ValidationData data)
    {
        _inner = new SeedScenarioWorkspaceDataSource(data);
    }

    public ScenarioWorkspaceDataSet Load(ScenarioWorkspaceDataRequest request)
    {
        var data = _inner.Load(request);
        return data with { CapacityProtections = Array.Empty<CapacityProtectionDefinition>() };
    }
}

internal sealed class MissingCashHistoryOperatingFactSource : IHistoryOperatingFactSource
{
    private readonly IHistoryOperatingFactSource _inner;

    public MissingCashHistoryOperatingFactSource(IHistoryOperatingFactSource inner)
    {
        _inner = inner;
    }

    public HistoryFactSet Load(HistoryFactRequest request)
    {
        var facts = _inner.Load(request);
        return facts with
        {
            OperatingFacts = facts.OperatingFacts
                .Select(item => item with { CashOccupied = null })
                .ToList()
        };
    }
}

internal sealed class FixedCurrentBaselineDataSource : ICurrentBaselineDataSource
{
    private readonly CurrentBaselineCandidate _candidate;

    public FixedCurrentBaselineDataSource(CurrentBaselineCandidate candidate)
    {
        _candidate = candidate;
    }

    public CurrentBaselineCandidate GetCandidate() => _candidate;
}

internal sealed class FixedScenarioRunLineageReader : IScenarioRunLineageReader
{
    private readonly IReadOnlyDictionary<string, ScenarioRunSummary> _runs;
    private readonly IReadOnlyDictionary<string, ScenarioRunDetail> _details;

    public FixedScenarioRunLineageReader(params ScenarioRunSummary[] runs)
    {
        _runs = runs.ToDictionary(item => item.RunId, StringComparer.Ordinal);
        _details = new Dictionary<string, ScenarioRunDetail>(StringComparer.Ordinal);
    }

    public FixedScenarioRunLineageReader(ScenarioRunDetail detail, params ScenarioRunSummary[] runs)
    {
        _runs = runs
            .Append(detail.Summary)
            .ToDictionary(item => item.RunId, StringComparer.Ordinal);
        _details = new Dictionary<string, ScenarioRunDetail>(StringComparer.Ordinal)
        {
            [detail.Summary.RunId] = detail
        };
    }

    public ScenarioRunSummary? GetSummary(string runId) =>
        _runs.TryGetValue(runId, out var summary) ? summary : null;

    public ScenarioRunDetail? GetDetail(string runId) =>
        _details.TryGetValue(runId, out var detail) ? detail : null;
}

internal sealed class StaticScenarioWorkspaceDataSource : IScenarioWorkspaceDataSource
{
    private readonly ScenarioWorkspaceDataSet _data;

    public StaticScenarioWorkspaceDataSource(ScenarioWorkspaceDataSet data)
    {
        _data = data;
    }

    public ScenarioWorkspaceDataSet Load(ScenarioWorkspaceDataRequest request) => _data with { Request = request };
}

internal sealed class LeadTimeFactorRemovingScenarioWorkspaceDataSource : IScenarioWorkspaceDataSource
{
    private readonly IScenarioWorkspaceDataSource _inner;

    public LeadTimeFactorRemovingScenarioWorkspaceDataSource(IScenarioWorkspaceDataSource inner)
    {
        _inner = inner;
    }

    public ScenarioWorkspaceDataSet Load(ScenarioWorkspaceDataRequest request)
    {
        var data = _inner.Load(request);
        var firstSku = data.Skus.First();
        return data with
        {
            Skus = data.Skus
                .Select(item => item.Sku == firstSku.Sku ? item with { LeadTimeFactor = null } : item)
                .ToList()
        };
    }
}

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
        var sku = new SkuBufferSetting(
            "AV-FPGA-EX", "空间级 FPGA 异常件", "星载电子", 100, 5, 1.5m, 7, 500, 1000, 1000,
            LeadTimeFactor: 0.6m,
            ParameterSnapshotId: "AV-FPGA-EX-V1",
            ParameterEvidenceStatus: "Complete");
        var sizing = DdmrpCalculator.CalculateSizing(sku);
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
                    sizing.Zones.TopOfRed,
                    sizing.Zones.TopOfYellow,
                    sizing.Zones.TopOfGreen,
                    1,
                    12,
                    "Current",
                    "Complete",
                    "测试参数完整。",
                    sku.LeadTimeFactor,
                    sku.ParameterSnapshotId,
                    sizing.EvidenceStatus,
                    sizing,
                    DdmrpSizingExplanation.Build(sizing))
            },
            Array.Empty<MasterSetting>(),
            Array.Empty<BusinessGuardrail>());
    }
}
