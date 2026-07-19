using AdaptiveSopDdsop.Web.Data;
using AdaptiveSopDdsop.Web.Domain;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

var tests = new (string Name, Action Run)[]
{
    ("Internal demo fact set conserves 52 weeks for all twelve SKUs", TestInternalDemoFactSetConservesAllSkuInventory),
    ("Internal demo demand profiles are not proportional copies", TestInternalDemoDemandProfilesAreDistinct),
    ("Internal demo inventory adjustments belong to named event weeks", TestInternalDemoInventoryAdjustmentsUseNamedEvents),
    ("Internal demo historical net flow has explicit varying supply and demand components", TestInternalDemoHistoricalNetFlowHasExplicitComponents),
    ("Internal demo receipt weeks keep only outstanding supply in end-of-week NFP", TestInternalDemoReceiptWeeksDoNotDoubleCountReceipts),
    ("Internal demo operating facts leave target NFP empty without parameter evidence", TestInternalDemoOperatingFactsDoNotRelabelActualNfpAsTarget),
    ("Scenario workspace exposes shared fact-set lineage", TestScenarioWorkspaceExposesSharedFactSetLineage),
    ("FPGA MOQ is five in master and scenario data", TestFpgaMoqIsFiveInMasterAndScenario),
    ("RCCP spreads projected order workload across four aggregate weeks", TestRccpSpreadsOrderWorkloadAcrossFourWeeks),
    ("RCCP keeps a middle required-week workload in the following four-week window", TestRccpKeepsMiddleWorkloadInForwardWindow),
    ("RCCP keeps terminal required-week workload in a complete four-week window", TestRccpKeepsTerminalWorkloadInCompleteWindow),
    ("Demo scenario baseline is inside credible feasibility ranges", TestDemoScenarioBaselineFeasibilityRanges),
    ("Demo scenario templates include reviewable and blocked candidates", TestDemoScenarioTemplatesCoverFeasibilityOutcomes),
    ("Demo inventory budget derives from frozen stock facts", TestDemoInventoryBudgetUsesFrozenFacts),
    ("Standard DDMRP sizing returns 80 120 70 with an explainable green driver", TestStandardDdmrpSizingReturns80_120_70),
    ("Standard DDMRP reference is calculated by an internal backend service", TestDdmrpStandardReferenceIsBackendCalculated),
    ("History review no longer owns the standard DDMRP reference", TestHistoryReviewNoLongerOwnsStandardDdmrpReference),
    ("Standard DDMRP reference API is internal and ordered outside protected integrations", TestDdmrpStandardReferenceApiIsInternalAndOrdered),
    ("DDMRP sizing rejects missing or illegal lead-time factors", TestDdmrpSizingRejectsIllegalLeadTimeFactor),
    ("Net flow position adds on hand and open supply then subtracts qualified demand", TestNetFlow),
    ("Planning recommendation replenishes to top of green at review time when net flow is at or below top of yellow", TestPlanningRecommendation),
    ("Promotion scenario increases ADU and working capital", TestPromotionScenario),
    ("Supply disruption lowers buffer health and creates expedite recommendation", TestSupplyDisruptionScenario),
    ("Planned shutdown creates capacity warning and management review action", TestShutdownScenario),
    ("Baseline data demonstrates red yellow green and over top of green buffer statuses with Chinese names", TestBaselineStatusVarietyAndChineseNames),
    ("Five-stage internal files do not reference protected contract types or endpoints", TestFiveStageServicesDoNotReferenceExternalContractTypesOrEndpoints),
    ("Desktop startup does not require Windows Event Log write access", TestDesktopStartupDoesNotRequireWindowsEventLog),
    ("Seed scale matches a credible satellite manufacturing demo", TestSeedScaleMatchesSatelliteManufacturingDemo),
    ("FPGA belongs only to its independent inventory control point", TestFpgaBelongsOnlyToIndependentInventoryControlPoint),
    ("Three independent inventory control points are explicit", TestThreeIndependentInventoryControlPointsAreExplicit),
    ("Capacity protection requires sequenced upstream evidence", TestCapacityProtectionRequiresSequencedUpstreamEvidence),
    ("Capacity protection is not inferred without sequence evidence", TestCapacityProtectionDoesNotInferWithoutSequenceEvidence),
    ("Scenario capacity protection excludes FPGA-only sequence evidence", TestScenarioCapacityProtectionExcludesFpgaSequenceEvidence),
    ("Capacity protection math uses 80 percent start", TestCapacityProtectionStartsAtEightyPercent),
    ("Capacity protection bands honor exact boundaries", TestCapacityProtectionBandsHonorBoundaries),
    ("Capacity protection separates consumption and overload", TestCapacityProtectionSeparatesConsumptionAndOverload),
    ("Reserve percent remains informational for fixed capacity protection", TestReservePercentDoesNotDriveCapacityProtection),
    ("Capacity protection requires upstream CCR evidence", TestCapacityProtectionRequiresUpstreamEvidence),
    ("CCR utilization is reference only", TestCcrUtilizationIsReferenceOnly),
    ("FPGA is excluded from capacity and time protection", TestFpgaIsInventoryOnlyControlPoint),
    ("History and future capacity measures agree", TestCapacityMeasuresAgreeAcrossViews),
    ("Capacity A layout pairs AIT upstream with HARNESS CCR", TestCapacityALayoutPairsUpstreamAndCcr),
    ("Deep red capacity remains in the red breach streak", TestDeepRedCapacityContinuesBreachStreak),
    ("Consolidated requirements are represented in validation data", TestConsolidatedRequirementsDataCoverage),
    ("History review follows cumulative lead time and exposes protection evidence", TestHistoryReviewUsesCumulativeLeadTimeAndProtectionEvidence),
    ("History review aggregates distinct twenty-six and fifty-two week facts", TestHistoryReviewAggregatesDistinctTwentySixAndFiftyTwoWeekFacts),
    ("History annual facts contain 52 irregular weeks", TestHistoryAnnualFactsContainFiftyTwoIrregularWeeks),
    ("History six month view is annual tail", TestHistorySixMonthViewIsAnnualTail),
    ("History facts avoid repeated fixture cycles", TestHistoryFactsAvoidRepeatedFixtureCycles),
    ("History event costs are explicit evidence", TestHistoryEventCostsAreExplicitEvidence),
    ("History magnitudes reconcile across views", TestHistoryMagnitudesReconcileAcrossViews),
    ("History facts use the shared twelve-SKU operating ledger", TestHistoryFactsUseSharedEnterpriseLedger),
    ("History facts retain one atomic shared fact-set lineage", TestHistoryFactsRetainAtomicSharedFactSetLineage),
    ("History buffer facts expose continuous stock movement", TestHistoryBufferFactsExposeContinuousMovement),
    ("History and scenario share the same fact-set cutoff", TestHistoryAndScenarioShareFactSetCutoff),
    ("History abnormal costs retain object ownership", TestHistoryAbnormalCostsRetainObjectOwnership),
    ("History events remain scoped to owned objects", TestHistoryEventsRemainScopedToOwnedObjects),
    ("History event display text is Chinese", TestHistoryEventDisplayTextIsChinese),
    ("History review projects stock time capacity and sizing views from explicit facts", TestHistoryReviewProjectsExplicitBufferViews),
    ("History operating outcomes no longer publish remaining protection", TestHistoryOperatingOutcomesDoNotOwnProtection),
    ("History capacity summary exposes balance minimum exhaustion and overload", TestHistoryCapacitySummaryExposesProtectionRisk),
    ("History capacity summary keeps exception counts empty for missing weekly evidence", TestHistoryCapacitySummaryDoesNotSubstituteMissingWeeklyEvidence),
    ("History capacity summaries average weekly protection consumption", TestHistoryCapacitySummariesAverageWeeklyProtectionConsumption),
    ("History capacity protection rejects self-referential resource evidence", TestHistoryCapacityProtectionRejectsSelfReference),
    ("Historical CCR role wins when a resource is also upstream", TestHistoricalCcrRoleWinsOverUpstreamRole),
    ("History review uses the effective historical parameter snapshot for every point", TestHistoryReviewUsesEffectiveParameterSnapshot),
    ("History inventory projection sizes weekly zones from rolling SKU demand", TestHistoryInventoryProjectionUsesRollingSkuDemand),
    ("History inventory evidence verifies stock and net-flow equations", TestHistoryInventoryEvidenceChecksBothEquations),
    ("History projection separates weekly event and parameter reason", TestHistoryProjectionSeparatesReasons),
    ("History projection does not publish target net-flow position", TestHistoryProjectionOmitsTargetNetFlow),
    ("History review preserves annual rolling context across range views", TestHistoryReviewPreservesAnnualRollingContextAcrossRanges),
    ("History time buffer projects only fully matched abnormal cost events", TestHistoryTimeBufferProjectsOnlyFullyMatchedCostEvents),
    ("History review exposes missing evidence instead of zero or current-parameter backfill", TestHistoryReviewDoesNotBackfillMissingEvidence),
    ("History facts expose versioned inventory time and capacity evidence", TestHistoryFactsExposeVersionedInventoryTimeAndCapacityEvidence),
    ("History time-buffer costs exclude FPGA event evidence", TestHistoryTimeBufferCostsExcludeFpgaEventEvidence),
    ("History capacity protection excludes FPGA routing evidence", TestHistoryCapacityProtectionExcludesFpgaRoutingEvidence),
    ("Historical outcomes use explicit facts and traceable costs", TestHistoricalOutcomesUseExplicitFactsAndTraceableCosts),
    ("Historical outcome costs use annual valid-event rules", TestHistoricalOutcomeCostsUseAnnualValidEventRules),
    ("Current baseline reconciles from the same historical fact set", TestCurrentBaselineReconcilesHistoryClosingBalances),
    ("Current baseline blocks missing history reconciliation", TestCurrentBaselineBlocksMissingHistoryReconciliation),
    ("Current baseline blocks unbalanced history reconciliation", TestCurrentBaselineBlocksUnbalancedHistoryReconciliation),
    ("Current baseline blocks incomplete reconciliation key coverage", TestCurrentBaselineBlocksIncompleteReconciliationKeyCoverage),
    ("Current baseline requires exact history reconciliation evidence section", TestCurrentBaselineRequiresExactHistoryReconciliationEvidenceSection),
    ("Current baseline recomputes reconciliation difference during freeze", TestCurrentBaselineRecomputesHistoryReconciliationDifference),
    ("Current baseline binds reconciliation balances to candidate payload", TestCurrentBaselineBindsReconciliationBalancesToCandidatePayload),
    ("Current baseline binds reconciliation lineage to candidate planning inputs", TestCurrentBaselineBindsReconciliationLineageToCandidatePlanningInputs),
    ("Current baseline blocks null reconciliation lines", TestCurrentBaselineBlocksNullHistoryReconciliationLines),
    ("Current baseline blocks null reconciliation line elements", TestCurrentBaselineBlocksNullHistoryReconciliationLineElements),
    ("Current baseline blocks non-complete reconciliation lineage", TestCurrentBaselineBlocksNonCompleteHistoryReconciliation),
    ("Current baseline blocks duplicate reconciliation keys", TestCurrentBaselineBlocksDuplicateHistoryReconciliationKey),
    ("Current baseline blocks unexpected reconciliation keys", TestCurrentBaselineBlocksUnexpectedHistoryReconciliationKey),
    ("Runtime seed registrations use the shared operating fact source", TestRuntimeSeedRegistrationsUseSharedFactSource),
    ("Current baseline exposes meeting snapshot KPIs with source and as-of evidence", TestCurrentBaselineExposesSnapshotKpisWithSourceAndAsOf),
    ("Current baseline rejects missing KPI evidence instead of freezing zero substitutes", TestCurrentBaselineRejectsMissingSnapshotKpiEvidence),
    ("Current baseline applies required evidence rules when section items are null or empty", TestCurrentBaselineAppliesRequiredRulesForEmptySectionItems),
    ("Current baseline blocks incomplete DDMRP sizing evidence", TestCurrentBaselineBlocksIncompleteDdmrpSizingEvidence),
    ("Legacy frozen baseline keeps missing lead-time factor visible and cannot be recalculated", TestLegacyFrozenBaselineKeepsMissingLeadTimeFactor),
    ("Current baseline UI follows item-level freeze blockers", TestCurrentBaselineUiFollowsItemLevelFreezeBlockers),
    ("Current baseline UI shows typed planning evidence without zero backfill", TestCurrentBaselineUiShowsTypedPlanningEvidenceWithoutZeroBackfill),
    ("Current baseline UI exposes one immutable history reconciliation card", TestCurrentBaselineUiExposesHistoryReconciliation),
    ("Current baseline executable fixture preserves blockers and explicit zero evidence", TestCurrentBaselineExecutableFixturePreservesBlockersAndZeroEvidence),
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
    ("DDMRP standard reference disclosure lazy loads in the standard harness", TestDdmrpStandardReferenceFixtureRunsInStandardHarness),
    ("History visual renderers use backend evidence without frontend formulas", TestHistoryVisualRenderersUseBackendEvidence),
    ("Future buffer charts use backend sizing and separate volatility", TestFutureBufferChartsUseBackendSizingAndSeparateVolatility),
    ("Future inventory flow charts separate NFP physical stock and volatility", TestFutureInventoryFlowChartsSeparatePhysicalEvidence),
    ("Five-stage business views translate internal codes without mojibake", TestBusinessViewsTranslateInternalCodesWithoutMojibake),
    ("Five-stage business views localize ordinary unit tokens", TestBusinessViewsLocalizeOrdinaryUnitTokens),
    ("RCCP peak load is explained as replenishment release pressure", TestRccpPeakLoadUsesReleasePressureWording),
    ("Five-stage generated business text uses Chinese ordinary wording", TestGeneratedBusinessTextUsesChineseOrdinaryWording),
    ("Five-stage UI has no external import or protocol input", TestFiveStageUiHasNoExternalImportOrProtocolInput),
    ("Future time-buffer evidence is consolidated into breach analysis", TestFutureTimeBufferEvidenceIsConsolidatedIntoBreachAnalysis),
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
    ("Seed planning evidence has 52 weeks", TestSeedPlanningEvidenceHasFiftyTwoWeeks),
    ("AV-COM-201 sizing is calibrated", TestAvCom201SizingIsCalibrated),
    ("Baseline derives typed planning evidence", TestBaselineDerivesTypedPlanningEvidence),
    ("Baseline freeze rejects incomplete planning evidence", TestBaselineFreezeRejectsIncompletePlanningEvidence),
    ("Frozen planning evidence round trips", TestFrozenPlanningEvidenceRoundTrips),
    ("Planning evidence accepts complete 52 weeks", TestPlanningEvidenceAcceptsCompleteCoverage),
    ("Planning evidence rejects demand gaps", TestPlanningEvidenceRejectsDemandGaps),
    ("Planning evidence rejects duplicate negative and expired rows", TestPlanningEvidenceRejectsInvalidRows),
    ("Planning evidence rejects receipt date mismatch", TestPlanningEvidenceRejectsReceiptDateMismatch),
    ("Planning evidence uses left-closed right-open week buckets", TestPlanningEvidenceUsesLeftClosedRightOpenBuckets),
    ("Planning evidence converts source timestamps in Asia Shanghai", TestPlanningEvidenceConvertsSourceTimestamps),
    ("Planning evidence preserves receipts outside coverage", TestPlanningEvidencePreservesOutsideCoverageReceipts),
    ("Frozen evidence rejects generated receipts", TestFrozenEvidenceRejectsGeneratedReceipts),
    ("Planning evidence requires supplier capacity DDMRP and open-supply mappings", TestPlanningEvidenceRequiresSupportingMappings),
    ("Planning evidence preserves legacy JSON", TestPlanningEvidencePreservesLegacyJson),
    ("Inventory flow conserves weekly quantity", TestInventoryFlowConservesWeeklyQuantity),
    ("Inventory flow fulfills oldest demand first", TestInventoryFlowFulfillsOldestDemandFirst),
    ("Simulated receipt respects DLT arrival", TestSimulatedReceiptRespectsDltArrival),
    ("Inventory flow separates receipt sources", TestInventoryFlowSeparatesReceiptSources),
    ("Prebuild receipt is counted once", TestPrebuildReceiptIsCountedOnce),
    ("Conflicting prebuild IDs are evidence missing", TestConflictingPrebuildIdsAreEvidenceMissing),
    ("Inventory flow scopes demand before ledger", TestInventoryFlowScopesDemandBeforeLedger),
    ("Zero new demand service is not applicable", TestZeroDemandServiceIsNotApplicable),
    ("Projection beyond coverage is evidence missing", TestProjectionBeyondCoverageIsEvidenceMissing),
    ("Supplier capacity constrains simulated receipts only", TestSupplierCapacityConstrainsSimulatedOnly),
    ("Supplier capacity allocates proportionally", TestSupplierCapacityAllocatesProportionally),
    ("Supplier capacity assigns rounding residual deterministically", TestSupplierCapacityAssignsRoundingResidualDeterministically),
    ("Supplier capacity never rounds fractional limits upward", TestSupplierCapacityNeverRoundsFractionalLimitUpward),
    ("Supplier capacity quantizes fractional source caps", TestSupplierCapacityQuantizesFractionalSourceCaps),
    ("Supplier capacity rounding never overallocates a source", TestSupplierCapacityRoundingNeverOverallocatesSource),
    ("Deferred simulated receipt carries forward", TestDeferredSimulatedReceiptCarriesForward),
    ("Frozen receipts remain fixed under capacity loss", TestFrozenReceiptsRemainFixed),
    ("Prebuild remains unchanged under supplier limit", TestPrebuildRemainsUnchanged),
    ("Missing constrained supply mapping is not unlimited", TestMissingConstrainedSupplyMappingIsEvidenceMissing),
    ("Missing constrained capacity week is not unlimited", TestMissingConstrainedCapacityWeekIsEvidenceMissing),
    ("Explicit not-applicable supplier capacity is unbounded", TestExplicitNotApplicableSupplierCapacityIsUnbounded),
    ("Inventory flow fields preserve legacy scenario JSON", TestInventoryFlowFieldsPreserveLegacyScenarioJson),
    ("Preview returns complete physical inventory flow", TestPreviewReturnsCompleteInventoryFlow),
    ("Physical flow drives inventory metrics and budget", TestPhysicalFlowDrivesMetricsAndBudget),
    ("Buffer signal separates planning and physical positions", TestBufferSignalSeparatesPlanningAndPhysicalPositions),
    ("Buffer signal omits physical position when evidence is missing", TestBufferSignalOmitsPhysicalPositionWhenEvidenceIsMissing),
    ("Buffer signal rejects cross-case physical evidence", TestBufferSignalRejectsCrossCasePhysicalEvidence),
    ("Incomplete physical flow never publishes inventory amounts", TestIncompletePhysicalFlowNeverPublishesInventoryAmounts),
    ("Buffer signal can recover NFP before physical receipt", TestBufferSignalShowsNfpRecoveryBeforePhysicalReceipt),
    ("Legacy preview keeps legacy reference labels", TestLegacyPreviewKeepsLegacyReference),
    ("Comparison omits physical delta when evidence missing", TestComparisonOmitsIncompletePhysicalDelta),
    ("Frozen comparison preserves baseline lineage and source evidence", TestFrozenComparisonPreservesBaselineLineageAndEvidence),
    ("Inventory flow result JSON round trips", TestInventoryFlowResultJsonRoundTrips),
    ("Scenario feasibility policy enforces shared hard limits and threshold boundaries", TestScenarioFeasibilityPolicyEnforcesSharedHardLimitsAndThresholdBoundaries),
    ("Scenario feasibility policy blocks missing evidence and is attached to previews", TestScenarioFeasibilityPolicyBlocksMissingEvidenceAndAttachesToPreview),
    ("Blocked evidence-complete scenario can be saved without approval", TestBlockedEvidenceCompleteScenarioCanBeSavedWithoutApproval),
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

static void TestInternalDemoFactSetConservesAllSkuInventory()
{
    var data = SeedData.Create();
    var facts = new SeedInternalDemoOperatingFactSource(data).Load();
    AssertEqual("DEMO-OPERATING-20260630-V1", facts.Header.FactSetId, "fact set id");
    AssertEqual("DemoFixture", facts.Header.SourceKind, "fact source kind");
    AssertEqual(12, facts.InventoryMovements.Select(item => item.Sku).Distinct().Count(), "ledger SKU count");
    AssertEqual(12 * 52, facts.InventoryMovements.Count, "ledger row count");

    foreach (var skuFacts in facts.InventoryMovements.GroupBy(item => item.Sku))
    {
        var ordered = skuFacts.OrderBy(item => item.WeekOffset).ToList();
        AssertEqual(52, ordered.Count, $"{skuFacts.Key} week count");
        for (var index = 0; index < ordered.Count; index++)
        {
            var point = ordered[index];
            AssertEqual(
                decimal.Round(point.OpeningOnHand + point.ActualReceipts - point.ActualConsumption + point.InventoryAdjustment, 2),
                point.EndingOnHand,
                $"{skuFacts.Key}/{point.WeekOffset} inventory conservation");
            if (index > 0)
            {
                AssertEqual(ordered[index - 1].EndingOnHand, point.OpeningOnHand,
                    $"{skuFacts.Key}/{point.WeekOffset} opening continuity");
            }
        }
    }
}

static void TestInternalDemoDemandProfilesAreDistinct()
{
    var facts = new SeedInternalDemoOperatingFactSource(SeedData.Create()).Load();
    var com = facts.InventoryMovements.Where(item => item.Sku == "AV-COM-201")
        .OrderBy(item => item.WeekOffset).Select(item => item.ActualDemand).ToList();
    var obc = facts.InventoryMovements.Where(item => item.Sku == "AV-OBC-202")
        .OrderBy(item => item.WeekOffset).Select(item => item.ActualDemand).ToList();
    var ratios = com.Zip(obc, (left, right) => right == 0m ? 0m : decimal.Round(left / right, 3)).Distinct().ToList();
    AssertTrue(ratios.Count > 4, "AV-COM and AV-OBC must not be fixed proportional copies");
}

static void TestInternalDemoInventoryAdjustmentsUseNamedEvents()
{
    var expected = new Dictionary<int, string>
    {
        [-46] = "DEMAND_CHANGE",
        [-39] = "IMPORT_DELAY",
        [-33] = "AIT_CAPACITY_LOSS",
        [-29] = "RECOVERY",
        [-21] = "DEMAND_PEAK",
        [-16] = "REWORK",
        [-11] = "SUPPLY_RECOVERY",
        [-6] = "TAKT_RECOVERY"
    };
    var facts = new SeedInternalDemoOperatingFactSource(SeedData.Create()).Load();
    var adjusted = facts.InventoryMovements.Where(item => item.InventoryAdjustment != 0m).ToList();
    AssertTrue(adjusted.Count > 0, "named inventory events must produce adjustments");
    foreach (var item in adjusted)
    {
        AssertTrue(!string.IsNullOrWhiteSpace(item.EventCode) && item.EventCode != "NONE", "adjustment must have an event code");
        AssertTrue(expected.TryGetValue(item.WeekOffset, out var eventCode) && eventCode == item.EventCode,
            $"{item.Sku}/{item.WeekOffset} adjustment must use the shared event registry");
    }
}

static void TestInternalDemoHistoricalNetFlowHasExplicitComponents()
{
    var facts = new SeedInternalDemoOperatingFactSource(SeedData.Create()).Load();
    var history = facts.InventoryMovements.Where(item => item.WeekOffset < -1).ToList();
    AssertTrue(history.Count > 0, "historical ledger must contain prior weeks");
    AssertTrue(history.Count(item => item.OpenSupply > 0m) > history.Count / 2,
        "most historical weeks must expose recorded open-supply evidence");
    AssertTrue(history.Count(item => item.QualifiedDemand > 0m) > history.Count / 2,
        "most historical weeks must expose recorded qualified-demand evidence");
    AssertTrue(history.Count(item => item.EndingNetFlow != item.EndingOnHand) > history.Count / 2,
        "historical NFP must not mechanically equal on-hand for most weeks");
    AssertTrue(history.Select(item => item.OpenSupply).Distinct().Count() > 12,
        "open supply must vary beyond a per-SKU constant");
    AssertTrue(history.Select(item => item.QualifiedDemand).Distinct().Count() > 12,
        "qualified demand must vary beyond a per-SKU constant");
    AssertTrue(history.All(item => item.EndingNetFlow == item.EndingOnHand + item.OpenSupply - item.QualifiedDemand),
        "historical NFP must reconcile to its physical components");
}

static void TestInternalDemoReceiptWeeksDoNotDoubleCountReceipts()
{
    var data = SeedData.Create();
    var facts = new SeedInternalDemoOperatingFactSource(data).Load();
    var skuIndexes = data.Skus
        .OrderBy(item => item.Sku, StringComparer.Ordinal)
        .Select((item, index) => (item.Sku, Index: index))
        .ToDictionary(item => item.Sku, item => item.Index, StringComparer.Ordinal);
    var receiptWeeks = facts.InventoryMovements
        .Where(item => item.WeekOffset < -1 && item.ActualReceipts > 0m)
        .ToList();
    AssertTrue(receiptWeeks.Count > 0, "historical ledger must contain receipt weeks");

    foreach (var item in receiptWeeks)
    {
        var sku = data.Skus.Single(candidate => candidate.Sku == item.Sku);
        var index = skuIndexes[item.Sku];
        var week = item.WeekOffset + 53;
        var cadence = 3 + index % 4;
        var receiptWeeksAway = cadence - week % cadence;
        var confirmedInbound = receiptWeeksAway <= 2
            ? sku.Adu * 7m * (0.72m + (index % 3) * 0.08m)
            : 0m;
        var openPurchaseCommitment = sku.Adu * 7m *
            (0.24m + ((week * ((index % 4) + 2) + index) % 6) * 0.09m);
        var expectedOutstandingSupply = decimal.Round(
            Math.Max(0m, confirmedInbound + openPurchaseCommitment +
                (item.WeekOffset == -39 && item.Sku == "AV-FPGA-203" ? -openPurchaseCommitment : 0m)),
            2,
            MidpointRounding.AwayFromZero);

        AssertEqual(expectedOutstandingSupply, item.OpenSupply,
            $"{item.Sku}/{item.WeekOffset} end-of-week outstanding supply");
        AssertTrue(item.OpenSupply < item.ActualReceipts,
            $"{item.Sku}/{item.WeekOffset} already-received quantity must not remain in open supply");
    }
}

static void TestInternalDemoOperatingFactsDoNotRelabelActualNfpAsTarget()
{
    var facts = new SeedInternalDemoOperatingFactSource(SeedData.Create()).Load();
    AssertTrue(facts.OperatingFacts.All(item => item.TargetNetFlowPosition is null),
        "target NFP must be empty when this source has no parameter evidence");
    AssertTrue(facts.OperatingFacts.All(item =>
            item.TargetNetFlowPosition != facts.InventoryMovements
                .Where(movement => movement.WeekOffset == item.WeekOffset)
                .Sum(movement => movement.EndingNetFlow)),
        "actual ending NFP must not be relabeled as a target");
}

static void TestScenarioWorkspaceExposesSharedFactSetLineage()
{
    var data = SeedData.Create();
    var facts = new SeedInternalDemoOperatingFactSource(data).Load();
    var scenario = new SeedScenarioWorkspaceDataSource(data)
        .Load(new ScenarioWorkspaceDataRequest(52, new DateOnly(2026, 6, 30)));
    AssertEqual(facts.Header.FactSetId, scenario.FactSetId, "scenario fact-set id");
    AssertEqual(facts.Header.HistoryThroughUtc, scenario.HistoryThroughUtc, "scenario history cutoff");
    AssertEqual(facts.Header.BaselineAsOfUtc, scenario.BaselineAsOfUtc, "scenario baseline cutoff");
}

static void TestFpgaMoqIsFiveInMasterAndScenario()
{
    var data = SeedData.Create();
    AssertEqual(5m, data.Skus.Single(item => item.Sku == "AV-FPGA-203").MinimumOrderQuantity, "master FPGA MOQ");
    var scenario = new SeedScenarioWorkspaceDataSource(data)
        .Load(new ScenarioWorkspaceDataRequest(52, new DateOnly(2026, 6, 30)));
    AssertEqual(5m, scenario.Skus.Single(item => item.Sku == "AV-FPGA-203").MinimumOrderQuantity, "scenario FPGA MOQ");
}

static void TestRccpSpreadsOrderWorkloadAcrossFourWeeks()
{
    var projections = DemandDrivenPlanningEngine.ProjectRoughCutCapacity(
        new[] { new ProjectedReplenishmentOrder("SKU-TEST", 1, 100m, 0m, "test") },
        new[] { new ResourceRouting("SKU-TEST", "RES-TEST", 1m) },
        new[] { new CapacityResource("RES-TEST", "测试资源", 100m, 1m) },
        4);

    AssertEqual(4, projections.Count, "four-week RCCP projection count");
    foreach (var projection in projections)
    {
        AssertEqual(25m, projection.RequiredCapacity, $"week {projection.Week} required workload");
    }
    AssertEqual(100m, projections.Sum(item => item.RequiredCapacity), "total allocated workload");
}

static void TestRccpKeepsTerminalWorkloadInCompleteWindow()
{
    var projections = DemandDrivenPlanningEngine.ProjectRoughCutCapacity(
        new[] { new ProjectedReplenishmentOrder("SKU-TEST", 12, 100m, 0m, "test") },
        new[] { new ResourceRouting("SKU-TEST", "RES-TEST", 1m) },
        new[] { new CapacityResource("RES-TEST", "测试资源", 100m, 1m) },
        12);

    AssertEqual(12, projections.Count, "twelve-week RCCP projection count");
    foreach (var projection in projections.Where(item => item.Week is >= 9 and <= 12))
    {
        AssertEqual(25m, projection.RequiredCapacity, $"week {projection.Week} terminal required workload");
    }
    AssertEqual(100m, projections.Sum(item => item.RequiredCapacity), "terminal total allocated workload");
}

static void TestRccpKeepsMiddleWorkloadInForwardWindow()
{
    var projections = DemandDrivenPlanningEngine.ProjectRoughCutCapacity(
        new[] { new ProjectedReplenishmentOrder("SKU-TEST", 6, 100m, 0m, "test") },
        new[] { new ResourceRouting("SKU-TEST", "RES-TEST", 1m) },
        new[] { new CapacityResource("RES-TEST", "测试资源", 100m, 1m) },
        12);

    foreach (var projection in projections.Where(item => item.Week is >= 6 and <= 9))
    {
        AssertEqual(25m, projection.RequiredCapacity, $"week {projection.Week} middle required workload");
    }
    AssertEqual(100m, projections.Sum(item => item.RequiredCapacity), "middle total allocated workload");
}

static void TestDemoScenarioBaselineFeasibilityRanges()
{
    var data = SeedData.Create();
    var expectedMoqs = new Dictionary<string, decimal>(StringComparer.Ordinal)
    {
        ["SAT-BUS-001"] = 2m, ["SAT-BUS-002"] = 2m, ["SAT-PROP-003"] = 6m,
        ["PAY-EO-101"] = 2m, ["PAY-SAR-102"] = 2m, ["AV-COM-201"] = 12m,
        ["AV-OBC-202"] = 8m, ["AV-FPGA-203"] = 5m, ["TC-MLI-301"] = 30m,
        ["TC-RAD-302"] = 20m, ["MECH-DEP-401"] = 8m, ["CBL-HAR-402"] = 30m
    };
    var expectedCapacities = new Dictionary<string, decimal>(StringComparer.Ordinal)
    {
        ["RES-AIT"] = 370m,
        ["RES-TVAC"] = 185m,
        ["RES-CLEAN"] = 120m,
        ["RES-HARNESS"] = 460m
    };

    foreach (var (sku, expectedMoq) in expectedMoqs)
    {
        AssertEqual(expectedMoq, data.Skus.Single(item => item.Sku == sku).MinimumOrderQuantity, $"{sku} MOQ");
    }
    foreach (var (resourceCode, expectedCapacity) in expectedCapacities)
    {
        AssertEqual(expectedCapacity, data.Resources.Single(item => item.Code == resourceCode).WeeklyAvailableUnits, $"{resourceCode} weekly capacity");
    }

    var preview = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(data))
        .Preview(new ScenarioRunPreviewRequest(12));
    var baseline = preview.Baseline.Metrics;
    var peakLoad = preview.Baseline.Plan.CapacityLoads.MaxBy(item => item.LoadPercent)!;
    AssertTrue(baseline.PeakLoadPercent <= 100m,
        $"baseline peak load must be feasible, got {baseline.PeakLoadPercent:0.0}% at {peakLoad.ResourceCode} week {peakLoad.Week} ({peakLoad.RequiredCapacity:0.####}/{peakLoad.AvailableCapacity:0.####})");
    AssertTrue(baseline.AverageLoadPercent is >= 30m and <= 85m, $"baseline average load must be credible, got {baseline.AverageLoadPercent:0.0}%");
    AssertTrue(baseline.FlowIndex >= 87m, $"baseline flow must remain healthy, got {baseline.FlowIndex:0.0}%");
    AssertTrue(baseline.AverageInventoryValue.HasValue, "baseline average inventory should be present");
}

static void TestDemoScenarioTemplatesCoverFeasibilityOutcomes()
{
    var data = SeedData.Create();
    var source = new SeedScenarioWorkspaceDataSource(data);
    var templates = source.Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1))).ScenarioTemplates;
    var service = new ScenarioRunPreviewService(source);
    var outcomes = templates.ToDictionary(
        item => item.TemplateId,
        item => service.Preview(new ScenarioRunPreviewRequest(12, item.TemplateId)).Scenario.Metrics,
        StringComparer.Ordinal);

    AssertTrue(outcomes.Values.Any(item => item.PeakLoadPercent <= 100m), "at least one built-in template should remain reviewable");
    var constrained = outcomes["TPL-CONSTRAINED"];
    AssertTrue(constrained.PeakLoadPercent > 100m || constrained.SupplyGap > 0m,
        "constrained template should remain an explicit blocked candidate");
}

static void TestDemoInventoryBudgetUsesFrozenFacts()
{
    var data = SeedData.Create();
    var workspace = new SeedScenarioWorkspaceDataSource(data)
        .Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1)));
    var skuByCode = data.Skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);
    var expectedByFamily = data.Skus
        .GroupBy(item => item.Family)
        .ToDictionary(
            group => group.Key,
            group => decimal.Round(workspace.Inventory
                .Where(item => group.Any(sku => sku.Sku == item.Sku))
                .Sum(item => (item.OnHand + item.OpenSupply) * skuByCode[item.Sku].UnitCost) * 1.10m, 0),
            StringComparer.Ordinal);

    foreach (var benchmark in workspace.BudgetBenchmarks)
    {
        AssertEqual(expectedByFamily[benchmark.Family], benchmark.BudgetInventoryValue,
            $"{benchmark.Family} week {benchmark.Week} frozen-inventory budget");
    }
}

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

static void TestDdmrpStandardReferenceIsBackendCalculated()
{
    var reference = new DdmrpStandardReferenceService().GetReference();

    AssertEqual(10m, reference.Inputs.Adu, "standard reference ADU");
    AssertEqual(12, reference.Inputs.DecoupledLeadTimeDays, "standard reference DLT");
    AssertEqual(0.5m, reference.Inputs.LeadTimeFactor, "standard reference lead-time factor");
    AssertEqual(0.33m, reference.Inputs.VariabilityFactor, "standard reference variability factor");
    AssertEqual(50m, reference.Inputs.MinimumOrderQuantity, "standard reference MOQ");
    AssertEqual(7, reference.Inputs.OrderCycleDays, "standard reference order cycle");
    AssertEqual(1m, reference.Inputs.DemandAdjustmentFactor, "standard reference DAF");
    AssertEqual(1m, reference.Inputs.ZoneAdjustmentFactor, "standard reference zone adjustment");
    AssertEqual(60m, reference.RedBase, "standard reference red base");
    AssertEqual(19.8m, reference.RedSafety, "standard reference red safety");
    AssertEqual(80m, reference.Zones.Red, "standard reference red zone");
    AssertEqual(120m, reference.Zones.Yellow, "standard reference yellow zone");
    AssertEqual(70m, reference.Zones.Green, "standard reference green zone");
    AssertEqual(270m, reference.TotalBuffer, "standard reference total buffer");
    AssertEqual("OrderCycle", reference.GreenDriver, "standard reference green driver");
    AssertEqual("DDAE 后端标准定容算例", reference.SourceAuthority, "standard reference source");
    AssertEqual("Complete", reference.EvidenceStatus, "standard reference evidence");
    AssertTrue(reference.Derivations.Count >= 10, "standard reference should expose backend derivation evidence");
}

static void TestHistoryReviewNoLongerOwnsStandardDdmrpReference()
{
    var result = new HistoryReviewWorkspaceService(
        new SeedHistoryOperatingFactSource(),
        new SeedScenarioWorkspaceDataSource(SeedData.Create())).GetReview(6);

    AssertTrue(result.StandardDdmrpReference is null, "history review should not load the standard reference");
    var constructorParameter = typeof(HistoryReviewWorkspace)
        .GetConstructors()
        .Single()
        .GetParameters()
        .Single(item => item.Name == "StandardDdmrpReference");
    AssertTrue(typeof(HistoryReviewWorkspace).GetConstructors().Single().GetParameters()
        .Last().Name == "CapacityProtectionSummary", "capacity protection summary is appended as the compatible workspace tail");
    AssertEqual(typeof(HistoryDdmrpSizingSnapshotView), constructorParameter.ParameterType, "legacy field type remains compatible");
    AssertTrue(constructorParameter.HasDefaultValue && constructorParameter.DefaultValue is null, "legacy field remains optional");

    var legacySnapshot = result.DdmrpSizingSnapshots!.First();
    var legacyJson = JsonSerializer.Serialize(
        result with { StandardDdmrpReference = legacySnapshot },
        new JsonSerializerOptions(JsonSerializerDefaults.Web));
    var legacyRoundTrip = JsonSerializer.Deserialize<HistoryReviewWorkspace>(
        legacyJson,
        new JsonSerializerOptions(JsonSerializerDefaults.Web));
    AssertEqual(legacySnapshot.SnapshotId, legacyRoundTrip!.StandardDdmrpReference!.SnapshotId, "legacy non-null reference JSON remains readable");
}

static void TestDdmrpStandardReferenceApiIsInternalAndOrdered()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var program = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Program.cs"));
    var service = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "DdmrpStandardReferenceService.cs"));
    var historyIndex = program.IndexOf("app.MapGet(\"/api/history-review\"", StringComparison.Ordinal);
    var referenceIndex = program.IndexOf("app.MapGet(\"/api/ddmrp-standard-reference\"", StringComparison.Ordinal);
    var baselineIndex = program.IndexOf("app.MapGet(\"/api/current-baselines/candidate\"", StringComparison.Ordinal);

    AssertTrue(program.Contains("AddSingleton<DdmrpStandardReferenceService>()", StringComparison.Ordinal), "standard reference service should be registered");
    AssertTrue(historyIndex >= 0 && referenceIndex > historyIndex && baselineIndex > referenceIndex,
        "standard reference endpoint should follow history and precede current baseline");
    foreach (var protectedToken in new[]
    {
        "DdsopConfigInboundContract",
        "DdsopRuntimePlanningInputContract",
        "SdbrExecutionObjectEvidenceContract",
        "PublicDemoGoldenLoopService",
        "NetworkScore",
        "network-scoring",
        "SDBR payload",
    })
    {
        AssertTrue(!service.Contains(protectedToken, StringComparison.Ordinal),
            $"standard reference service must not reference protected token '{protectedToken}'");
    }
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
        Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "PlanningEvidenceModels.cs"),
        Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "PlanningEvidenceValidator.cs"),
        Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "InventoryFlowProjectionModels.cs"),
        Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "InventoryFlowProjectionService.cs"),
        Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "CapacityProtectionMath.cs"),
        Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Domain", "DdmrpStandardReferenceService.cs"),
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

    var webProgram = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Program.cs"));
    const string historyEndpointMarker = "app.MapGet(\"/api/history-review\"";
    const string referenceEndpointMarker = "app.MapGet(\"/api/ddmrp-standard-reference\"";
    const string integrationStartMarker = "app.MapGet(\"/api/integration-contracts/ddsop-config-inbound-v1\"";
    var historyEndpointIndex = webProgram.IndexOf(historyEndpointMarker, StringComparison.Ordinal);
    var referenceEndpointIndex = webProgram.IndexOf(referenceEndpointMarker, StringComparison.Ordinal);
    var integrationStartIndex = webProgram.IndexOf(integrationStartMarker, StringComparison.Ordinal);
    AssertEqual(1, CountExactOccurrences(webProgram, referenceEndpointMarker), "standard reference GET endpoint count");
    AssertTrue(integrationStartIndex >= 0 && historyEndpointIndex > integrationStartIndex,
        "integration-contract endpoint block should end at the history endpoint marker");
    var integrationContractBlock = webProgram.Substring(integrationStartIndex, historyEndpointIndex - integrationStartIndex);
    AssertTrue(referenceEndpointIndex > historyEndpointIndex,
        "standard reference endpoint should follow the history endpoint marker");
    AssertTrue(!integrationContractBlock.Contains("/api/ddmrp-standard-reference", StringComparison.Ordinal),
        "standard reference endpoint must remain outside the integration-contract endpoint block");
    foreach (var forbiddenVerb in new[] { "MapPost", "MapPut", "MapPatch", "MapDelete", "MapMethods" })
    {
        AssertTrue(!webProgram.Contains($"app.{forbiddenVerb}(\"/api/ddmrp-standard-reference\"", StringComparison.Ordinal),
            $"standard reference endpoint must not use {forbiddenVerb}");
    }

    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var businessStart = page.IndexOf("<section id=\"history-review-panel\"", StringComparison.Ordinal);
    var validationStart = page.IndexOf("id=\"saved-scenarios-panel\"", businessStart, StringComparison.Ordinal);
    AssertTrue(businessStart >= 0 && validationStart > businessStart,
        "five-stage visible-text region should precede validation pages");
    var businessMarkup = page.Substring(businessStart, validationStart - businessStart);
    var visibleBusinessText = System.Net.WebUtility.HtmlDecode(
        System.Text.RegularExpressions.Regex.Replace(businessMarkup, "<[^>]+>", " "));
    foreach (var internalCode in new[] { "DemoFixture", "Open", "InProgress", "Escalated", "Completed" })
    {
        AssertTrue(!visibleBusinessText.Contains(internalCode, StringComparison.Ordinal),
            $"five-stage visible text must localize internal code {internalCode} without changing HTML values");
    }
    AssertTrue(businessMarkup.Contains("value=\"DemoFixture\"", StringComparison.Ordinal),
        "internal demo source value must remain unchanged in the form contract");
}

static void TestDesktopStartupDoesNotRequireWindowsEventLog()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var settingsPath = Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "appsettings.Development.json");
    var settings = JsonNode.Parse(File.ReadAllText(settingsPath));
    var eventLogLevel = settings?["Logging"]?["EventLog"]?["LogLevel"]?["Default"]?.GetValue<string>();

    AssertEqual("None", eventLogLevel,
        "desktop app should keep console logging while disabling the privileged Windows Event Log provider");
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
        ["RES-AIT"] = 370m,
        ["RES-TVAC"] = 185m,
        ["RES-CLEAN"] = 120m,
        ["RES-HARNESS"] = 460m,
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
    AssertTrue(harnessLoad is >= 30m and <= 45m, $"HARNESS baseline load should be 30%-45%, got {harnessLoad:0.0}%");
    AssertTrue(aitLoad is >= 30m and <= 45m, $"AIT baseline load should be 30%-45%, got {aitLoad:0.0}%");
    AssertTrue(tvacLoad is >= 35m and <= 40m, $"TVAC baseline load should be 35%-40%, got {tvacLoad:0.0}%");

    var workspace = new SeedScenarioWorkspaceDataSource(data)
        .Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1)));
    var tvacLossMultiplier = workspace.ResourceCalendar
        .Where(item => item.ResourceCode == "RES-TVAC")
        .Min(item => item.CapacityMultiplier);
    AssertTrue(tvacLoad / tvacLossMultiplier < 100m, "TVAC should remain below 100% after the built-in capacity-loss calibration");
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

static void TestCapacityProtectionStartsAtEightyPercent()
{
    var measure = InvokeCapacityProtectionMath("CalculateUpstream", 100m, 85m, true, "Complete");

    AssertEqual<decimal?>(85m, CapacityMeasureValue<decimal?>(measure, "UtilizationPercent"), "upstream utilization percent");
    AssertEqual<decimal?>(80m, CapacityMeasureValue<decimal?>(measure, "ProtectionStart"), "protection start at eighty percent");
    AssertEqual<decimal?>(20m, CapacityMeasureValue<decimal?>(measure, "ProtectionCapacity"), "twenty percent protection capacity");
    AssertEqual<decimal?>(5m, CapacityMeasureValue<decimal?>(measure, "ConsumedProtection"), "consumed protection above eighty percent");
    AssertEqual<decimal?>(15m, CapacityMeasureValue<decimal?>(measure, "RemainingProtection"), "remaining protection");
}

static void TestCapacityProtectionBandsHonorBoundaries()
{
    AssertEqual("Green", CapacityMeasureValue<string>(InvokeCapacityProtectionMath("CalculateUpstream", 100m, 60m, true, "Complete"), "UtilizationBand"), "sixty percent band");
    AssertEqual("Yellow", CapacityMeasureValue<string>(InvokeCapacityProtectionMath("CalculateUpstream", 100m, 80m, true, "Complete"), "UtilizationBand"), "eighty percent band");
    AssertEqual("Red", CapacityMeasureValue<string>(InvokeCapacityProtectionMath("CalculateUpstream", 100m, 100m, true, "Complete"), "UtilizationBand"), "one-hundred percent band");
    AssertEqual("DeepRed", CapacityMeasureValue<string>(InvokeCapacityProtectionMath("CalculateUpstream", 100m, 100.1m, true, "Complete"), "UtilizationBand"), "over one-hundred percent band");
}

static void TestCapacityProtectionSeparatesConsumptionAndOverload()
{
    var measure = InvokeCapacityProtectionMath("CalculateUpstream", 100m, 112m, true, "Complete");

    AssertEqual<decimal?>(20m, CapacityMeasureValue<decimal?>(measure, "ConsumedProtection"), "protection consumption is capped at protection capacity");
    AssertEqual<decimal?>(0m, CapacityMeasureValue<decimal?>(measure, "RemainingProtection"), "exhausted protection has zero remaining");
    AssertEqual<decimal?>(12m, CapacityMeasureValue<decimal?>(measure, "Overload"), "overload is separated from consumed protection");
    AssertEqual("DeepRed", CapacityMeasureValue<string>(measure, "UtilizationBand"), "overload band");
}

static void TestReservePercentDoesNotDriveCapacityProtection()
{
    const int horizonWeeks = 1;
    var seed = SeedData.Create();
    var preview = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(seed))
        .Preview(new ScenarioRunPreviewRequest(horizonWeeks));
    var constraints = preview.Scenario.Constraints with
    {
        CapacityCells = preview.Scenario.Constraints.CapacityCells
            .Select(item => item.ResourceCode == "RES-AIT"
                ? item with { ConstrainedAvailable = 100m, UnconstrainedRequired = 85m }
                : item)
            .ToList()
    };
    var futureData = LoadTask5ProtectionData(horizonWeeks) with
    {
        CapacityProtections = LoadTask5ProtectionData(horizonWeeks).CapacityProtections!
            .Select(item => item with { ReservePercent = 99m })
            .ToList()
    };
    var futurePoint = new CapacityBufferProtectionAnalyzer()
        .Analyze(futureData, preview.Scenario with { Constraints = constraints }, horizonWeeks)
        .Projection.Single(item => item.UpstreamResourceCode == "RES-AIT");
    var futureMeasure = CapacityMeasureFrom(futurePoint);

    AssertEqual<decimal?>(80m, CapacityMeasureValue<decimal?>(futureMeasure, "ProtectionStart"), "future start ignores informational reserve percent");
    AssertEqual<decimal?>(20m, CapacityMeasureValue<decimal?>(futureMeasure, "ProtectionCapacity"), "future capacity ignores informational reserve percent");

    foreach (var reservePercent in new[] { 0m, 99m })
    {
        var historySource = new CapacityProtectionTransformingHistoryOperatingFactSource(
            new SeedHistoryOperatingFactSource(seed),
            existing => existing.Select(item => item with { ReservePercent = reservePercent }).ToList());
        var history = new HistoryReviewWorkspaceService(
            historySource,
            new SeedScenarioWorkspaceDataSource(seed)).GetReview(6);
        var historyPoint = (history.CapacityBuffers ?? throw new InvalidOperationException("historical capacity projection is missing"))
            .Single(item => item.ResourceCode == "RES-AIT")
            .Points.First(item => item.PlannedAvailableCapacity is > 0m);
        var historyMeasure = CapacityMeasureFrom(historyPoint);
        var expectedStart = historyPoint.PlannedAvailableCapacity * 0.80m;
        var expectedCapacity = historyPoint.PlannedAvailableCapacity - expectedStart;

        AssertEqual("Complete", CapacityMeasureValue<string>(historyMeasure, "EvidenceStatus"), $"history reserve {reservePercent} remains informational");
        AssertEqual(expectedStart, CapacityMeasureValue<decimal?>(historyMeasure, "ProtectionStart"), $"history reserve {reservePercent} fixed start");
        AssertEqual(expectedCapacity, CapacityMeasureValue<decimal?>(historyMeasure, "ProtectionCapacity"), $"history reserve {reservePercent} fixed capacity");
    }
}

static void TestCapacityProtectionRequiresUpstreamEvidence()
{
    foreach (var measure in new[]
    {
        InvokeCapacityProtectionMath("CalculateUpstream", 100m, 85m, false, "Complete"),
        InvokeCapacityProtectionMath("CalculateUpstream", 0m, 0m, true, "Complete"),
        InvokeCapacityProtectionMath("CalculateUpstream", -1m, 0m, true, "Complete"),
        InvokeCapacityProtectionMath("CalculateUpstream", null, 85m, true, "Complete"),
        InvokeCapacityProtectionMath("CalculateUpstream", 100m, null, true, "Complete"),
        InvokeCapacityProtectionMath("CalculateUpstream", 100m, -1m, true, "Complete"),
        InvokeCapacityProtectionMath("CalculateUpstream", 100m, 85m, true, "EvidenceMissing"),
    })
    {
        AssertEqual("EvidenceMissing", CapacityMeasureValue<string>(measure, "EvidenceStatus"), "missing upstream evidence status");
        AssertEqual("EvidenceMissing", CapacityMeasureValue<string>(measure, "UtilizationBand"), "missing upstream evidence band");
        AssertTrue(CapacityMeasureValue<object?>(measure, "UtilizationPercent") is null, "missing evidence must not backfill utilization with zero");
        AssertTrue(CapacityMeasureValue<object?>(measure, "ProtectionCapacity") is null, "missing evidence must not backfill protection with zero");
        AssertTrue(CapacityMeasureValue<object?>(measure, "Overload") is null, "missing evidence must not backfill overload with zero");
        AssertTrue(!string.IsNullOrWhiteSpace(CapacityMeasureValue<string?>(measure, "EvidenceIssue")), "missing evidence must explain the issue");
    }

    var frozenData = LoadTask5ProtectionData(1);
    var definition = frozenData.CapacityProtections!.Single() with
    {
        UpstreamResourceCode = "RES-AIT",
        ProtectedCcrResourceCode = "RES-AIT"
    };
    var selfReferentialData = frozenData with
    {
        CapacityProtections = new[] { definition },
        ResourceRoutings = new[]
        {
            new ResourceRouting("TC-MLI-301", "RES-AIT", 1m, 1, "RES-AIT", "Complete"),
            new ResourceRouting("TC-MLI-301", "RES-AIT", 1m, 2, null, "Complete")
        }
    };
    var preview = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()))
        .Preview(new ScenarioRunPreviewRequest(1));
    var selfReferential = new CapacityBufferProtectionAnalyzer()
        .Analyze(selfReferentialData, preview.Scenario, 1);
    AssertEqual("EvidenceMissing", selfReferential.Projection.Single().EvidenceStatus, "self-referential CCR routing must not calculate upstream protection");
    AssertTrue(selfReferential.Projection.Single().Measure?.ProtectionCapacity is null, "self-referential CCR routing must leave derived protection nullable");
}

static void TestCcrUtilizationIsReferenceOnly()
{
    var measure = InvokeCapacityProtectionMath("CalculateCcrReference", 100m, 85m, "Complete");

    AssertEqual<decimal?>(85m, CapacityMeasureValue<decimal?>(measure, "UtilizationPercent"), "CCR utilization reference");
    AssertEqual("Red", CapacityMeasureValue<string>(measure, "UtilizationBand"), "CCR utilization reference band");
    AssertTrue(CapacityMeasureValue<object?>(measure, "ProtectionStart") is null, "CCR protection start is not applicable");
    AssertTrue(CapacityMeasureValue<object?>(measure, "ProtectionCapacity") is null, "CCR protection capacity is not applicable");
    AssertTrue(CapacityMeasureValue<object?>(measure, "ConsumedProtection") is null, "CCR protection consumption is not applicable");
    AssertTrue(CapacityMeasureValue<object?>(measure, "RemainingProtection") is null, "CCR remaining protection is not applicable");
    AssertTrue(CapacityMeasureValue<object?>(measure, "Overload") is null, "CCR protection overload is not applicable");
}

static void TestFpgaIsInventoryOnlyControlPoint()
{
    var preview = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()))
        .Preview(new ScenarioRunPreviewRequest(12));
    var frozenData = LoadTask5ProtectionData(12);
    var analysis = ProtectionBreachAnalyzer.Analyze(
        frozenData,
        new ExternalScenarioDefinition("EXT-NONE", "无外部扰动"),
        null,
        preview.Scenario,
        12);
    var history = new HistoryReviewWorkspaceService(
        new SeedHistoryOperatingFactSource(SeedData.Create()),
        new SeedScenarioWorkspaceDataSource(SeedData.Create())).GetReview(6);

    AssertTrue(preview.Scenario.BufferTrend.WeeklyCells.Any(item => item.Sku == "AV-FPGA-203"), "FPGA must remain in inventory projection");
    AssertTrue(analysis.TimeBufferProjection.All(item => !item.ControlPoint.Contains("FPGA", StringComparison.OrdinalIgnoreCase)), "FPGA must stay out of time protection");
    AssertTrue(analysis.CapacityProtectionProjection.All(item => !item.UpstreamResourceCode.Contains("FPGA", StringComparison.OrdinalIgnoreCase)), "FPGA must stay out of future capacity protection");
    AssertTrue((history.CapacityBuffers ?? Array.Empty<HistoryCapacityBufferView>()).All(item =>
        !string.Join(" ", item.ResourceCode, item.ResourceName, item.ProtectedCcrResourceCode).Contains("FPGA", StringComparison.OrdinalIgnoreCase)), "FPGA must stay out of historical capacity protection");
}

static void TestCapacityMeasuresAgreeAcrossViews()
{
    var seed = SeedData.Create();
    var history = new HistoryReviewWorkspaceService(
        new SeedHistoryOperatingFactSource(seed),
        new SeedScenarioWorkspaceDataSource(seed)).GetReview(6);
    var historyPoint = (history.CapacityBuffers ?? throw new InvalidOperationException("historical capacity projection is missing"))
        .Single(item => item.ResourceCode == "RES-AIT")
        .Points
        .First(item => item.EvidenceStatus == "Complete");
    var historyMeasure = CapacityMeasureFrom(historyPoint);

    var preview = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(seed))
        .Preview(new ScenarioRunPreviewRequest(1));
    var upstreamLoad = preview.Scenario.Plan.CapacityLoads.Single(item => item.ResourceCode == "RES-AIT" && item.Week == 1);
    var ccrLoad = preview.Scenario.Plan.CapacityLoads.Single(item => item.ResourceCode == "RES-HARNESS" && item.Week == 1);
    AssertEqual("UpstreamProtection", MappedValue<string>(upstreamLoad, "RelationshipRole"), "scenario AIT relationship role");
    AssertEqual("RES-HARNESS", MappedValue<string>(upstreamLoad, "ProtectedCcrResourceCode"), "scenario AIT protected CCR");
    AssertTrue(MappedValue<object?>(upstreamLoad, "CapacityProtectionMeasure") is not null, "scenario AIT must carry the backend measure");
    AssertEqual("CcrUtilization", MappedValue<string>(ccrLoad, "RelationshipRole"), "scenario HARNESS relationship role");
    var ccrMeasure = MappedValue<object?>(ccrLoad, "CapacityProtectionMeasure")
        ?? throw new InvalidOperationException("scenario HARNESS CCR measure is missing");
    AssertTrue(CapacityMeasureValue<object?>(ccrMeasure, "ProtectionCapacity") is null, "scenario HARNESS must remain utilization reference only");
    var constraints = preview.Scenario.Constraints with
    {
        CapacityCells = preview.Scenario.Constraints.CapacityCells
            .Select(item => item.ResourceCode == "RES-AIT" && item.Week == 1
                ? item with
                {
                    ConstrainedAvailable = historyPoint.PlannedAvailableCapacity!.Value,
                    UnconstrainedRequired = historyPoint.CommittedLoad!.Value
                }
                : item)
            .ToList()
    };
    var futurePoint = new CapacityBufferProtectionAnalyzer()
        .Analyze(LoadTask5ProtectionData(1), preview.Scenario with { Constraints = constraints }, 1)
        .Projection.Single(item => item.UpstreamResourceCode == "RES-AIT" && item.Week == 1);
    var futureMeasure = CapacityMeasureFrom(futurePoint);

    AssertEqual(JsonSerializer.Serialize(historyMeasure), JsonSerializer.Serialize(futureMeasure), "history and future shared capacity measure");
    AssertEqual(historyPoint.ProtectionStart, CapacityMeasureValue<decimal?>(futureMeasure, "ProtectionStart"), "history and future protection-start mapping");
    AssertEqual(historyPoint.ProtectiveCapacity, futurePoint.ProtectionCapacity, "history and future legacy protection-capacity mapping");
    AssertEqual(historyPoint.ConsumedProtection, futurePoint.ConsumedProtection, "history and future legacy consumed-protection mapping");
    AssertEqual(historyPoint.RemainingProtection, futurePoint.RemainingProtection, "history and future legacy remaining-protection mapping");

    var legacyHistoryJson = JsonNode.Parse(JsonSerializer.Serialize(historyPoint))!.AsObject();
    legacyHistoryJson.Remove("Measure");
    var legacyHistory = JsonSerializer.Deserialize<HistoryCapacityPoint>(legacyHistoryJson.ToJsonString());
    AssertTrue(legacyHistory is not null && legacyHistory.Measure is null, "legacy history JSON without the appended measure remains readable");
    var legacyFutureJson = JsonNode.Parse(JsonSerializer.Serialize(futurePoint))!.AsObject();
    legacyFutureJson.Remove("Measure");
    var legacyFuture = JsonSerializer.Deserialize<CapacityProtectionProjectionPoint>(legacyFutureJson.ToJsonString());
    AssertTrue(legacyFuture is not null && legacyFuture.Measure is null, "legacy future JSON without the appended measure remains readable");
    var legacyLoadJson = JsonNode.Parse(JsonSerializer.Serialize(upstreamLoad))!.AsObject();
    legacyLoadJson.Remove("RelationshipRole");
    legacyLoadJson.Remove("ProtectedCcrResourceCode");
    legacyLoadJson.Remove("CapacityProtectionMeasure");
    var legacyLoad = JsonSerializer.Deserialize<CapacityLoadProjection>(legacyLoadJson.ToJsonString());
    AssertTrue(
        legacyLoad is not null &&
        legacyLoad.RelationshipRole is null &&
        legacyLoad.ProtectedCcrResourceCode is null &&
        legacyLoad.CapacityProtectionMeasure is null,
        "legacy capacity-load JSON without appended relationship fields remains readable");
}

static void TestCapacityALayoutPairsUpstreamAndCcr()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    AssertTrue(page.Contains("id=\"history-capacity-protection-pair\"", StringComparison.Ordinal), "capacity A layout needs a paired upstream and CCR host");
    AssertTrue(!page.Contains("id=\"history-capacity-band-distribution\"", StringComparison.Ordinal), "capacity history must remove the old all-period distribution wrapper");
    AssertTrue(!page.Contains("id=\"history-capacity-utilization-distribution\"", StringComparison.Ordinal), "capacity history must remove the old distribution subpanel");
    AssertTrue(page.Contains("id=\"history-capacity-protection-kpis\"", StringComparison.Ordinal), "capacity history needs backend protection KPI host");
    AssertTrue(page.Contains("id=\"history-capacity-buffer-chart\"", StringComparison.Ordinal), "capacity history needs the stable composite chart host");
    AssertTrue(page.Contains("id=\"history-capacity-upstream-card\"", StringComparison.Ordinal), "capacity A layout needs the upstream role card");
    AssertTrue(page.Contains("id=\"history-capacity-ccr-card\"", StringComparison.Ordinal), "capacity A layout needs the CCR reference card");
    AssertTrue(script.Contains("function resolveHistoryCapacityPair(history)", StringComparison.Ordinal), "capacity history must share one upstream/CCR resolver");
    AssertTrue(script.Contains("function buildHistoryCapacityFrequency(values, axisMaximum, binWidth = 10)", StringComparison.Ordinal), "capacity history must build display-only empirical frequency bins");
    AssertTrue(script.Contains("history-capacity-period-observations", StringComparison.Ordinal), "capacity history needs a weekly utilization subpanel");
    AssertTrue(script.Contains("history-capacity-empirical-distribution", StringComparison.Ordinal), "capacity history needs an empirical frequency subpanel");
    AssertTrue(script.Contains("measure.utilizationBand", StringComparison.Ordinal), "capacity A layout must use the backend utilization band");
    var pairBody = SourceFunctionBody(script, "renderHistoryCapacityProtectionPair");
    AssertTrue(pairBody.Contains("upstreamMeasure?.protectionStart", StringComparison.Ordinal), "upstream A card must show the backend eighty-percent protection start");
    var ccrCardStart = pairBody.IndexOf("history-capacity-ccr-card", StringComparison.Ordinal);
    var ccrCardEnd = pairBody.Length;
    AssertTrue(ccrCardStart >= 0 && ccrCardEnd > ccrCardStart, "CCR A card source boundaries");
    var ccrCardBody = pairBody[ccrCardStart..ccrCardEnd];
    AssertTrue(
        !ccrCardBody.Contains("保护能力", StringComparison.Ordinal) &&
        !ccrCardBody.Contains("已消耗保护", StringComparison.Ordinal) &&
        !ccrCardBody.Contains("剩余保护", StringComparison.Ordinal) &&
        !ccrCardBody.Contains("超载保护", StringComparison.Ordinal),
        "CCR A card must not render protection fields, including not-applicable placeholders");
    AssertTrue(!page.Contains("<th>保护消耗</th>", StringComparison.Ordinal), "future CCR reference table must not expose a protection-consumption column");
    AssertTrue(SourceFunctionBody(script, "renderHistoryCapacityBuffer").Contains("axisMaximum = Math.max(120", StringComparison.Ordinal), "history composite must retain the prescribed display axis floor");
    AssertTrue(!SourceFunctionBody(script, "renderFutureCapacityProtection").Contains("<= 60", StringComparison.Ordinal), "future renderer must not recalculate capacity thresholds");
}

static void TestDeepRedCapacityContinuesBreachStreak()
{
    const int horizonWeeks = 4;
    var preview = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()))
        .Preview(new ScenarioRunPreviewRequest(horizonWeeks));
    var constraints = preview.Scenario.Constraints with
    {
        CapacityCells = preview.Scenario.Constraints.CapacityCells
            .Select(item => item.ResourceCode == "RES-AIT"
                ? item with
                {
                    ConstrainedAvailable = 100m,
                    UnconstrainedRequired = item.Week switch
                    {
                        1 => 70m,
                        2 => 100m,
                        3 => 110m,
                        _ => 70m
                    }
                }
                : item)
            .ToList()
    };
    var analysis = new CapacityBufferProtectionAnalyzer().Analyze(
        LoadTask5ProtectionData(horizonWeeks),
        preview.Scenario with { Constraints = constraints },
        horizonWeeks);
    var breach = analysis.Breaches.Single(item => item.Target == "RES-AIT");

    AssertEqual("Red", analysis.Projection.Single(item => item.Week == 2).Status, "capacity at exactly one hundred percent");
    var overloadedPoint = analysis.Projection.Single(item => item.Week == 3);
    AssertEqual("Red", overloadedPoint.Status, "legacy protection status stays exhausted red above one hundred percent");
    AssertEqual("DeepRed", CapacityMeasureValue<string>(CapacityMeasureFrom(overloadedPoint), "UtilizationBand"), "backend utilization band above one hundred percent");
    AssertEqual(2, breach.EarliestRedWeek, "mixed red/deep-red earliest week");
    AssertEqual(2, breach.ConsecutiveRiskWeeks, "deep red must continue an existing red streak");
    AssertEqual(4, breach.RecoveryWeek, "mixed red/deep-red recovery week");

    var calculatorType = typeof(DdmrpCalculator).Assembly.GetType("AdaptiveSopDdsop.Web.Domain.StatusSeriesBreachCalculator");
    var calculate = calculatorType?.GetMethod("Calculate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    AssertTrue(calculate is not null, "status-series breach calculator is missing");
    ProtectionBreachResult CalculateStatusSeries(IReadOnlyList<(int Week, string Status)> values) =>
        (ProtectionBreachResult)(calculate!.Invoke(null, new object?[]
        {
            "CapacityBuffer",
            "RES-AIT",
            values,
            new[] { "TC-MLI-301" },
            "上游能力保护耗尽",
            null,
            null,
            "capacity",
            "Complete"
        }) ?? throw new InvalidOperationException("status-series breach calculation returned null"));
    var mixedStatusSeries = CalculateStatusSeries(new[]
    {
        (1, "Red"),
        (2, "DeepRed"),
        (3, "Red"),
        (4, "Green")
    });
    AssertEqual(3, mixedStatusSeries.ConsecutiveRiskWeeks, "red deep-red red must remain one risk streak");
    AssertEqual(4, mixedStatusSeries.RecoveryWeek, "red deep-red red recovery week");
    var endingDeepRed = CalculateStatusSeries(new[]
    {
        (1, "Green"),
        (2, "Red"),
        (3, "DeepRed")
    });
    AssertTrue(endingDeepRed.IsUnrecovered && endingDeepRed.RecoveryWeek is null, "ending deep red must remain unrecovered");
}

static object InvokeCapacityProtectionMath(string methodName, params object?[] arguments)
{
    var type = typeof(DdmrpCalculator).Assembly.GetType("AdaptiveSopDdsop.Web.Domain.CapacityProtectionMath");
    AssertTrue(type is not null, "shared CapacityProtectionMath backend type is missing");
    var method = type!.GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
    AssertTrue(method is not null, $"CapacityProtectionMath.{methodName} is missing");
    return method!.Invoke(null, arguments) ?? throw new InvalidOperationException($"CapacityProtectionMath.{methodName} returned null");
}

static object CapacityMeasureFrom(object mappedPoint)
{
    var property = mappedPoint.GetType().GetProperty("Measure");
    AssertTrue(property is not null, $"{mappedPoint.GetType().Name} must append the shared Measure DTO field");
    return property!.GetValue(mappedPoint) ?? throw new InvalidOperationException($"{mappedPoint.GetType().Name}.Measure is missing");
}

static T CapacityMeasureValue<T>(object measure, string propertyName)
{
    var property = measure.GetType().GetProperty(propertyName);
    AssertTrue(property is not null, $"CapacityProtectionMeasure.{propertyName} is missing");
    var value = property!.GetValue(measure);
    return value is null ? default! : (T)value;
}

static T MappedValue<T>(object mappedObject, string propertyName)
{
    var property = mappedObject.GetType().GetProperty(propertyName);
    AssertTrue(property is not null, $"{mappedObject.GetType().Name}.{propertyName} is missing");
    var value = property!.GetValue(mappedObject);
    return value is null ? default! : (T)value;
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
    AssertTrue(standardReference is null, "history review should keep the legacy standard reference field empty");
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

    if (recentOutcomes.ServiceLevelPercent is null || annualOutcomes.ServiceLevelPercent is null)
    {
        failures.Add("shared operating ledger did not provide service evidence");
    }

    if (recentOutcomes.InventoryValue is not (>= 65_000_000m and <= 75_000_000m) ||
        annualOutcomes.InventoryValue is not (>= 72_000_000m and <= 82_000_000m))
    {
        failures.Add($"inventory ranges were {recentOutcomes.InventoryValue:0}/{annualOutcomes.InventoryValue:0}");
    }

    if (recentOutcomes.WorkInProcessUnits is null || annualOutcomes.WorkInProcessUnits is null)
    {
        failures.Add("shared operating ledger did not provide WIP evidence");
    }

    if (recentOutcomes.AverageFlowTimeDays is null || annualOutcomes.AverageFlowTimeDays is null)
    {
        failures.Add("shared operating ledger did not provide flow-time evidence");
    }

    if (recentOutcomes.CashOccupied is null || annualOutcomes.CashOccupied is null)
    {
        failures.Add("shared operating ledger did not provide cash evidence");
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

static void TestHistoryAnnualFactsContainFiftyTwoIrregularWeeks()
{
    var facts = new SeedHistoryOperatingFactSource(SeedData.Create())
        .Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 1)));
    var expectedWeeks = Enumerable.Range(-52, 52).ToList();
    var operatingWeeks = facts.OperatingFacts
        .Select(item => item.WeekOffset)
        .OrderBy(item => item)
        .ToList();

    AssertEqual(52, facts.Request.Weeks, "annual history request weeks");
    AssertEqual(52, facts.OperatingFacts.Count, "annual operating fact count");
    AssertTrue(operatingWeeks.SequenceEqual(expectedWeeks), "annual history must contain every offset from -52 through -1 exactly once");
    AssertTrue(
        typeof(WeeklyOperatingFact).GetProperty("ActualDemand") is not null &&
        typeof(WeeklyOperatingFact).GetProperty("DemandSpikeThreshold") is not null &&
        typeof(WeeklyOperatingFact).GetProperty("TargetNetFlowPosition") is not null,
        "weekly operating facts must expose optional demand, spike-threshold and target-NFP evidence");
    AssertTrue(
        facts.OperatingFacts.All(item =>
            ReadOptionalHistoryDecimal(item, "ActualDemand") is > 0m &&
            ReadOptionalHistoryDecimal(item, "DemandSpikeThreshold") is > 0m &&
            ReadOptionalHistoryDecimal(item, "TargetNetFlowPosition") is null),
        "all 52 complete annual weeks must carry actual magnitude evidence and no target NFP");
    AssertTrue(facts.BufferFacts.All(item => item.TargetNetFlowPosition is null),
        "all buffer facts must retain the compatibility target NFP property as null");
    AssertTrue(
        facts.OperatingFacts
            .Select(item => ReadOptionalHistoryDecimal(item, "ActualDemand"))
            .Distinct()
            .Count() >= 40,
        "annual demand evidence must be irregular rather than a short repeated cycle");

    var expectedEvents = ExpectedHistoryEventNames();
    var visibleCauses = facts.BufferFacts.Select(item => item.ExplicitCause)
        .Concat(facts.CapacityFacts.Select(item => item.LossReason))
        .Concat((facts.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>()).Select(item => item.ExplicitCause))
        .ToList();
    foreach (var expected in expectedEvents)
    {
        AssertTrue(
            visibleCauses.Contains(expected.Value, StringComparer.Ordinal),
            $"named annual event at week {expected.Key}");
    }
}

static void TestHistorySixMonthViewIsAnnualTail()
{
    var seed = SeedData.Create();
    var source = new SeedHistoryOperatingFactSource(seed);
    var asOfDate = new DateOnly(2026, 6, 1);
    var annual = source.Load(new HistoryFactRequest(52, asOfDate));
    var recent = source.Load(new HistoryFactRequest(26, asOfDate));
    bool InRecent(int weekOffset) => weekOffset is >= -26 and <= -1;

    AssertEqual(
        JsonSerializer.Serialize(annual.OperatingFacts.Where(item => InRecent(item.WeekOffset)).ToList()),
        JsonSerializer.Serialize(recent.OperatingFacts),
        "six-month operating facts must be the annual trailing 26 weeks");
    AssertEqual(
        JsonSerializer.Serialize(annual.BufferFacts.Where(item => InRecent(item.WeekOffset)).ToList()),
        JsonSerializer.Serialize(recent.BufferFacts),
        "six-month buffer facts must be the annual trailing 26 weeks");
    AssertEqual(
        JsonSerializer.Serialize(annual.CapacityFacts.Where(item => InRecent(item.WeekOffset)).ToList()),
        JsonSerializer.Serialize(recent.CapacityFacts),
        "six-month capacity facts must be the annual trailing 26 weeks");
    AssertEqual(
        JsonSerializer.Serialize((annual.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>()).Where(item => InRecent(item.WeekOffset)).ToList()),
        JsonSerializer.Serialize(recent.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>()),
        "six-month time-buffer facts must be the annual trailing 26 weeks");
    AssertEqual(
        JsonSerializer.Serialize(annual.AbnormalCosts.Where(item => InRecent(item.WeekOffset)).ToList()),
        JsonSerializer.Serialize(recent.AbnormalCosts),
        "six-month abnormal costs must be the annual trailing 26 weeks");

    var recordingSource = new RecordingHistoryOperatingFactSource(source);
    var service = new HistoryReviewWorkspaceService(
        recordingSource,
        new SeedScenarioWorkspaceDataSource(seed));
    var recentReview = service.GetReview(6);
    AssertEqual(1, recordingSource.Requests.Count, "six-month review history load count");
    AssertEqual(52, recordingSource.Requests.Single().Weeks, "six-month review must load the annual fact set before slicing");
    AssertEqual(26, recentReview.ObservedTrendWeeks, "six-month review annual-tail observation count");
    AssertEqual(3, recentReview.DetailWindowWeeks.GetValueOrDefault(), "selected-object cumulative-lead-time detail window");
    AssertTrue(
        recentReview.EvidenceLabel.Contains("TrendWindow=trailing-26-of-52", StringComparison.Ordinal) &&
        recentReview.EvidenceLabel.Contains("SelectedObjectDetail=3-week cumulative-lead-time", StringComparison.Ordinal),
        "evidence label must distinguish the 26-week trend from the selected object's 3-week detail window");
}

static void TestHistoryFactsAvoidRepeatedFixtureCycles()
{
    var facts = new SeedHistoryOperatingFactSource(SeedData.Create())
        .Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 1)));
    var recentBuffer = facts.BufferFacts
        .Where(item => item.Sku == "AV-COM-201" && item.WeekOffset >= -24)
        .OrderBy(item => item.WeekOffset)
        .Select(item => item.EndingNetFlow)
        .ToList();
    var aitLoadRatios = facts.CapacityFacts
        .Where(item => item.ResourceCode == "RES-AIT")
        .OrderBy(item => item.WeekOffset)
        .Select(item => item.PlannedAvailableCapacity is > 0m && item.CommittedLoad.HasValue
            ? decimal.Round(item.CommittedLoad.Value * 100m / item.PlannedAvailableCapacity.Value, 1)
            : (decimal?)null)
        .ToList();
    var timeCounts = (facts.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>())
        .OrderBy(item => item.WeekOffset)
        .Select(item => $"{item.EarlyCount}|{item.GreenCount}|{item.YellowCount}|{item.RedCount}|{item.LateCount}")
        .ToList();

    AssertTrue(!RepeatsHistoryCycle(recentBuffer, 8), "buffer facts must not repeat an eight-week fixture cycle");
    AssertTrue(!RepeatsHistoryCycle(aitLoadRatios, 4), "capacity facts must not repeat a four-week fixture cycle");
    AssertTrue(!RepeatsHistoryCycle(timeCounts, 8), "time-buffer facts must not repeat an eight-week fixture cycle");

    var expectedEvents = ExpectedHistoryEventNames();
    var allowedEventNames = expectedEvents.Values.Append("无事件").ToHashSet(StringComparer.Ordinal);
    var timeFacts = facts.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>();
    AssertTrue(
        timeFacts.All(item => allowedEventNames.Contains(item.ExplicitCause)),
        "every weekly time fact must carry a named event or explicit no-event status");
    AssertTrue(
        facts.BufferFacts.All(item => allowedEventNames.Contains(item.ExplicitCause)),
        "every weekly buffer fact must carry a scoped named event or explicit no-event status");
    AssertTrue(
        facts.CapacityFacts.All(item => allowedEventNames.Contains(item.LossReason)),
        "every weekly capacity fact must carry a scoped named event or explicit no-event status");
}

static void TestHistoryEventCostsAreExplicitEvidence()
{
    var source = new SeedHistoryOperatingFactSource(SeedData.Create());
    var annual = source.Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 1)));
    var recent = source.Load(new HistoryFactRequest(26, new DateOnly(2026, 6, 1)));
    var namedEvents = ExpectedHistoryEventNames();
    var expectedAmounts = new[] { 180_000m, 240_000m, 360_000m, 420_000m };
    var actualAmounts = annual.AbnormalCosts.Select(item => item.CostAmount).OrderBy(item => item).ToList();

    AssertEqual(4, annual.AbnormalCosts.Count, "annual abnormal cost event count");
    AssertTrue(actualAmounts.SequenceEqual(expectedAmounts), "annual abnormal costs must use the four explicit evidence amounts");
    AssertEqual(1_200_000m, annual.AbnormalCosts.Sum(item => item.CostAmount), "annual explicit abnormal cost total");
    AssertEqual(420_000m, recent.AbnormalCosts.Sum(item => item.CostAmount), "trailing-six-month explicit abnormal cost total");
    AssertTrue(
        annual.AbnormalCosts.All(item =>
            item.EvidenceStatus == "Complete" &&
            namedEvents.TryGetValue(item.WeekOffset, out var eventName) &&
            item.Cause == eventName),
        "each abnormal cost must link to one named evidence-bearing event");

    var timeBufferCostsByWeek = annual.AbnormalCosts
        .Where(item => ReadRequiredRecordString(item, "TargetType") == "时间缓冲" &&
                       ReadRequiredRecordString(item, "TargetId") == "MS-TB-001")
        .ToDictionary(item => item.WeekOffset);
    foreach (var timeFact in annual.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>())
    {
        if (timeBufferCostsByWeek.TryGetValue(timeFact.WeekOffset, out var cost))
        {
            AssertEqual(cost.EventId, timeFact.AbnormalCostEventId!, $"week {timeFact.WeekOffset} abnormal cost event link");
            AssertEqual(cost.CostAmount, timeFact.AbnormalCost!.Value, $"week {timeFact.WeekOffset} abnormal cost amount link");
        }
        else
        {
            AssertTrue(
                timeFact.AbnormalCostEventId is null && timeFact.AbnormalCost is null,
                $"week {timeFact.WeekOffset} without cost evidence must be explicitly cost-free");
        }
    }
}

static void TestHistoryMagnitudesReconcileAcrossViews()
{
    var seed = SeedData.Create();
    var facts = new SeedHistoryOperatingFactSource(seed)
        .Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 1)));
    var shared = new SeedInternalDemoOperatingFactSource(seed).Load();
    var skuCosts = seed.Skus.ToDictionary(item => item.Sku, item => item.UnitCost, StringComparer.Ordinal);
    var operatingByWeek = facts.OperatingFacts.ToDictionary(item => item.WeekOffset);

    foreach (var operating in facts.OperatingFacts)
    {
        var buffers = facts.BufferFacts.Where(item => item.WeekOffset == operating.WeekOffset).ToList();
        var sharedMovements = shared.InventoryMovements.Where(item => item.WeekOffset == operating.WeekOffset).ToList();
        var expectedDemand = sharedMovements.Sum(item => item.ActualDemand);
        var expectedThreshold = sharedMovements.Average(item => item.DemandSpikeThreshold);
        var expectedInventoryValue = decimal.Round(sharedMovements.Sum(item => item.EndingOnHand * skuCosts[item.Sku]), 0);
        AssertEqual(expectedDemand, ReadRequiredHistoryDecimal(operating, "ActualDemand"), $"week {operating.WeekOffset} actual demand reconciliation");
        AssertEqual(expectedThreshold, ReadRequiredHistoryDecimal(operating, "DemandSpikeThreshold"), $"week {operating.WeekOffset} demand spike threshold reconciliation");
        AssertEqual(expectedInventoryValue, operating.InventoryValue!.Value, $"week {operating.WeekOffset} inventory value reconciliation");
        AssertTrue(ReadRequiredHistoryDecimal(operating, "DemandSpikeThreshold") > 0m, $"week {operating.WeekOffset} demand spike threshold evidence");
        AssertTrue(
            buffers.All(item => item.EndingNetFlow == item.EndingOnHand + item.OpenSupply - item.QualifiedDemand),
            $"week {operating.WeekOffset} component NFP reconciliation");
    }

    var peak = operatingByWeek[-21];
    AssertTrue(
        ReadRequiredHistoryDecimal(peak, "ActualDemand") > ReadRequiredHistoryDecimal(peak, "DemandSpikeThreshold"),
        "named peak-demand week must exceed its explicit spike threshold");
    AssertTrue(
        facts.BufferFacts.Where(item => item.WeekOffset == -39).Sum(item => item.EndingNetFlow) <
        facts.BufferFacts.Where(item => item.WeekOffset == -11).Sum(item => item.EndingNetFlow),
        "supply recovery must restore actual ending NFP after the import delay");

    var aitCapacity = facts.CapacityFacts
        .Where(item => item.ResourceCode == "RES-AIT")
        .ToDictionary(item => item.WeekOffset);
    AssertTrue(aitCapacity[-33].PlannedAvailableCapacity < aitCapacity[-29].PlannedAvailableCapacity,
        "AIT recovery must restore planned available capacity");
    var timeFacts = (facts.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>())
        .ToDictionary(item => item.WeekOffset);
    AssertTrue(timeFacts[-16].LateCount > timeFacts[-6].LateCount,
        "pacing recovery must reduce late time-buffer observations after rework");
}

static void TestHistoryFactsUseSharedEnterpriseLedger()
{
    var data = SeedData.Create();
    var shared = new SeedInternalDemoOperatingFactSource(data);
    var source = new SeedHistoryOperatingFactSource(data, shared);
    var facts = source.Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 30)));
    var sharedFacts = shared.Load();
    AssertEqual(sharedFacts.Header.FactSetId, facts.FactSetId, "history fact-set id");
    AssertEqual(52, facts.OperatingFacts.Count, "annual operating count");
    var lastWeek = facts.OperatingFacts.Single(item => item.WeekOffset == -1);
    var expectedInventory = sharedFacts.InventoryMovements
        .Where(item => item.WeekOffset == -1)
        .Join(data.Skus, movement => movement.Sku, sku => sku.Sku,
            (movement, sku) => movement.EndingOnHand * sku.UnitCost).Sum();
    AssertEqual(decimal.Round(expectedInventory, 0), lastWeek.InventoryValue!.Value, "twelve-SKU inventory value");
}

static void TestHistoryFactsRetainAtomicSharedFactSetLineage()
{
    var data = SeedData.Create();
    var initial = new SeedInternalDemoOperatingFactSource(data).Load() with
    {
        Header = new InternalDemoFactSetHeader(
            "FACT-SET-FIRST",
            "DemoFixture",
            "First shared fact set",
            "2026-06-30T00:00:00.0000000+00:00",
            "2026-06-30T08:00:00.0000000+00:00"),
    };
    var later = initial with
    {
        Header = initial.Header with { FactSetId = "FACT-SET-LATER", HistoryThroughUtc = "2030-01-01T00:00:00.0000000+00:00" },
        OperatingFacts = initial.OperatingFacts.Select(item => item with { InventoryValue = 1m }).ToList(),
    };
    var shared = new CountingInternalDemoOperatingFactSource(initial, later);
    var source = new SeedHistoryOperatingFactSource(data, shared);
    var facts = source.Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 30)));

    AssertEqual(1, shared.LoadCount, "history source must load the shared fact set once");
    AssertEqual(initial.Header.FactSetId, facts.FactSetId, "captured fact-set id");
    AssertEqual(initial.Header.HistoryThroughUtc, facts.AsOfUtc, "captured history cutoff");
    AssertTrue(facts.EvidenceLabel.Contains(initial.Header.FactSetId, StringComparison.Ordinal),
        "captured evidence label lineage");
    AssertEqual(
        initial.OperatingFacts.Single(item => item.WeekOffset == -1).InventoryValue,
        facts.OperatingFacts.Single(item => item.WeekOffset == -1).InventoryValue,
        "captured operating facts");
}

static void TestHistoryBufferFactsExposeContinuousMovement()
{
    var source = new SeedHistoryOperatingFactSource(SeedData.Create());
    var facts = source.Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 30)));
    foreach (var group in facts.BufferFacts.GroupBy(item => item.Sku))
    {
        var ordered = group.OrderBy(item => item.WeekOffset).ToList();
        for (var index = 0; index < ordered.Count; index++)
        {
            var point = ordered[index];
            AssertTrue(point.OpeningOnHand.HasValue && point.ActualReceipts.HasValue &&
                point.ActualConsumption.HasValue && point.InventoryAdjustment.HasValue,
                $"{group.Key}/{point.WeekOffset} movement fields");
            AssertEqual(point.EndingOnHand!.Value,
                point.OpeningOnHand!.Value + point.ActualReceipts!.Value -
                point.ActualConsumption!.Value + point.InventoryAdjustment!.Value,
                $"{group.Key}/{point.WeekOffset} movement equation");
            if (index > 0) AssertEqual(ordered[index - 1].EndingOnHand, point.OpeningOnHand,
                $"{group.Key}/{point.WeekOffset} cross-week continuity");
        }
    }
}

static void TestHistoryAndScenarioShareFactSetCutoff()
{
    var data = SeedData.Create();
    var shared = new SeedInternalDemoOperatingFactSource(data).Load();
    var historySource = new SeedHistoryOperatingFactSource(data, new SeedInternalDemoOperatingFactSource(data));
    var facts = historySource.Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 30)));
    var scenario = new SeedScenarioWorkspaceDataSource(data).Load(
        new ScenarioWorkspaceDataRequest(52, new DateOnly(2026, 6, 30)));
    AssertEqual(shared.Header.HistoryThroughUtc, facts.AsOfUtc, "history cutoff");
    AssertTrue(scenario.HistoricalDemand.All(item => shared.HistoricalDemand.Contains(item)),
        "scenario historical demand must use shared fact-set values");

    var service = new HistoryReviewWorkspaceService(historySource, new SeedScenarioWorkspaceDataSource(data));
    foreach (var review in new[] { service.GetReview(6), service.GetReview(12) })
    {
        AssertEqual("DEMO-OPERATING-20260630-V1", review.FactSetId, "workspace fact-set id");
        AssertEqual("2026-06-30T00:00:00.0000000+00:00", review.HistoryThroughUtc, "workspace history cutoff");
    }
}

static void TestHistoryAbnormalCostsRetainObjectOwnership()
{
    var facts = new SeedHistoryOperatingFactSource(SeedData.Create())
        .Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 1)));
    var costs = facts.AbnormalCosts.ToDictionary(item => item.WeekOffset);

    AssertEqual("需求对象", ReadRequiredRecordString(costs[-46], "TargetType"), "demand-change cost target type");
    AssertEqual("星载电子", ReadRequiredRecordString(costs[-46], "TargetId"), "demand-change cost target");
    AssertEqual("库存控制点", ReadRequiredRecordString(costs[-39], "TargetType"), "import-delay cost target type");
    AssertEqual("AV-FPGA-203", ReadRequiredRecordString(costs[-39], "TargetId"), "import-delay cost target");
    AssertEqual("关键进口 FPGA 库存控制点", ReadRequiredRecordString(costs[-39], "ControlPoint"), "import-delay control point");
    AssertEqual("能力对象", ReadRequiredRecordString(costs[-33], "TargetType"), "AIT capacity-loss target type");
    AssertEqual("RES-AIT", ReadRequiredRecordString(costs[-33], "TargetId"), "AIT capacity-loss target");
    AssertEqual("时间缓冲", ReadRequiredRecordString(costs[-16], "TargetType"), "rework cost target type");
    AssertEqual("MS-TB-001", ReadRequiredRecordString(costs[-16], "TargetId"), "rework cost target");
    AssertTrue(costs.Values.All(item => ReadRequiredRecordString(item, "SourceAuthority") == "DDAE 演示历史事实台账"),
        "every abnormal cost must name its historical source authority");

    var timeFacts = facts.TimeBufferFacts ?? throw new InvalidOperationException("historical time-buffer evidence is missing");
    var linkedCosts = timeFacts.Where(item => item.AbnormalCostEventId is not null).ToList();
    AssertEqual(1, linkedCosts.Count, "only the rework cost belongs to the heat-vacuum time buffer");
    AssertEqual(-16, linkedCosts.Single().WeekOffset, "heat-vacuum time-buffer cost week");
    AssertEqual("HAC-2026-002", linkedCosts.Single().AbnormalCostEventId!, "heat-vacuum time-buffer cost event");
    AssertTrue(timeFacts.Single(item => item.WeekOffset == -39).AbnormalCostEventId is null,
        "the FPGA inventory-control event must not be attached to the heat-vacuum time buffer");

    AssertTrue(
        facts.ConstraintFacts.Any(item => item.WeekOffset == -16 && item.SourceKind == "HistoricalFact" && item.Evidence.Contains("返工", StringComparison.Ordinal)),
        "event constraint observation must align with the named rework week");
    AssertTrue(
        facts.ConstraintFacts.Any(item => item.WeekOffset == -39 && item.SourceKind == "HistoricalFact" && item.Target.Contains("FPGA", StringComparison.Ordinal)),
        "external constraint observation must align with the named FPGA import-delay week");
}

static void TestHistoryEventsRemainScopedToOwnedObjects()
{
    var facts = new SeedHistoryOperatingFactSource(SeedData.Create())
        .Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 1)));

    var importBuffers = facts.BufferFacts.Where(item => item.WeekOffset == -39).ToList();
    AssertEqual("进口延迟", importBuffers.Single(item => item.Sku == "AV-FPGA-203").ExplicitCause, "FPGA import-delay cause");
    AssertTrue(importBuffers.Where(item => item.Sku != "AV-FPGA-203").All(item => item.ExplicitCause == "无事件"),
        "the FPGA import delay must not spread to other inventory control points");
    AssertTrue(facts.CapacityFacts.Where(item => item.WeekOffset == -39).All(item => item.LossReason == "无事件"),
        "the FPGA import delay must not spread to capacity resources");

    var aitLossCapacity = facts.CapacityFacts.Where(item => item.WeekOffset == -33).ToList();
    AssertEqual("AIT 能力损失", aitLossCapacity.Single(item => item.ResourceCode == "RES-AIT").LossReason, "AIT capacity-loss cause");
    AssertTrue(aitLossCapacity.Where(item => item.ResourceCode != "RES-AIT").All(item => item.LossReason == "无事件"),
        "the AIT capacity loss must not spread to other resources");
    AssertTrue(facts.BufferFacts.Where(item => item.WeekOffset == -33).All(item => item.ExplicitCause == "无事件"),
        "the AIT capacity loss must not spread to inventory control points");

    var expectedBufferOwnership = new Dictionary<int, IReadOnlySet<string>>
    {
        [-46] = new HashSet<string>(["AV-COM-201", "AV-OBC-202"], StringComparer.Ordinal),
        [-39] = new HashSet<string>(["AV-FPGA-203"], StringComparer.Ordinal),
        [-21] = new HashSet<string>(["TC-MLI-301"], StringComparer.Ordinal),
        [-11] = new HashSet<string>(["AV-FPGA-203"], StringComparer.Ordinal),
    };
    foreach (var ownership in expectedBufferOwnership)
    {
        var eventName = ExpectedHistoryEventNames()[ownership.Key];
        var points = facts.BufferFacts.Where(item => item.WeekOffset == ownership.Key).ToList();
        AssertTrue(points.Where(item => ownership.Value.Contains(item.Sku)).All(item => item.ExplicitCause == eventName),
            $"week {ownership.Key} owned inventory event");
        AssertTrue(points.Where(item => !ownership.Value.Contains(item.Sku)).All(item => item.ExplicitCause == "无事件"),
            $"week {ownership.Key} unowned inventory objects");
    }

    var expectedCapacityOwnership = new Dictionary<int, IReadOnlySet<string>>
    {
        [-33] = new HashSet<string>(["RES-AIT"], StringComparer.Ordinal),
        [-29] = new HashSet<string>(["RES-AIT"], StringComparer.Ordinal),
        [-16] = new HashSet<string>(["RES-AIT", "RES-CLEAN"], StringComparer.Ordinal),
        [-6] = new HashSet<string>(["RES-AIT", "RES-CLEAN"], StringComparer.Ordinal),
    };
    foreach (var ownership in expectedCapacityOwnership)
    {
        var eventName = ExpectedHistoryEventNames()[ownership.Key];
        var points = facts.CapacityFacts.Where(item => item.WeekOffset == ownership.Key).ToList();
        AssertTrue(points.Where(item => ownership.Value.Contains(item.ResourceCode)).All(item => item.LossReason == eventName),
            $"week {ownership.Key} owned capacity event");
        AssertTrue(points.Where(item => !ownership.Value.Contains(item.ResourceCode)).All(item => item.LossReason == "无事件"),
            $"week {ownership.Key} unowned capacity objects");
    }

    var timeFacts = facts.TimeBufferFacts ?? throw new InvalidOperationException("historical time-buffer evidence is missing");
    AssertTrue(timeFacts.All(item => item.WeekOffset == -16 ? item.ExplicitCause == "返工" : item.ExplicitCause == "无事件"),
        "the heat-vacuum time buffer must own only the rework event");
}

static void TestHistoryEventDisplayTextIsChinese()
{
    var facts = new SeedHistoryOperatingFactSource(SeedData.Create())
        .Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 1)));
    var expectedEvents = ExpectedHistoryEventNames();
    var forbidden = new[]
    {
        "DemandChange", "ImportDelay", "AitCapacityLoss", "Recovery", "PeakDemand",
        "Rework", "SupplyRecovery", "PacingRecovery", "NoEvent", "Demand response",
        "Expedite transport", "Temporary capacity",
    };
    var visibleText = facts.BufferFacts.Select(item => item.ExplicitCause)
        .Concat(facts.CapacityFacts.Select(item => item.LossReason))
        .Concat((facts.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>()).Select(item => item.ExplicitCause))
        .Concat(facts.AbnormalCosts.SelectMany(item => new[] { item.Cause, item.CostType }))
        .ToList();

    AssertTrue(expectedEvents.Values.All(name => visibleText.Contains(name, StringComparer.Ordinal)),
        "all named annual events must have Chinese display text");
    AssertTrue(visibleText.Contains("无事件", StringComparer.Ordinal), "ordinary weeks must display the Chinese no-event label");
    AssertTrue(
        visibleText.All(text => forbidden.All(token => !text.Contains(token, StringComparison.Ordinal))),
        "ordinary event and cost labels must not expose English fixture tokens");
}

static IReadOnlyDictionary<int, string> ExpectedHistoryEventNames() => new Dictionary<int, string>
{
    [-46] = "需求变化",
    [-39] = "进口延迟",
    [-33] = "AIT 能力损失",
    [-29] = "恢复",
    [-21] = "需求峰值",
    [-16] = "返工",
    [-11] = "供应恢复",
    [-6] = "节拍恢复",
};

static string ReadRequiredRecordString(object fact, string propertyName) =>
    fact.GetType().GetProperty(propertyName)?.GetValue(fact) is string value && !string.IsNullOrWhiteSpace(value)
        ? value
        : throw new InvalidOperationException($"{fact.GetType().Name} is missing {propertyName} evidence");

static decimal? ReadOptionalHistoryDecimal(WeeklyOperatingFact fact, string propertyName) =>
    typeof(WeeklyOperatingFact).GetProperty(propertyName)?.GetValue(fact) is decimal value
        ? value
        : null;

static decimal ReadRequiredHistoryDecimal(WeeklyOperatingFact fact, string propertyName) =>
    ReadOptionalHistoryDecimal(fact, propertyName)
        ?? throw new InvalidOperationException($"weekly operating fact is missing {propertyName} evidence");

static bool RepeatsHistoryCycle<T>(IReadOnlyList<T> values, int period)
{
    if (values.Count <= period)
    {
        return false;
    }

    return Enumerable.Range(period, values.Count - period)
        .All(index => EqualityComparer<T>.Default.Equals(values[index], values[index - period]));
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
        capacity.All(item => item.Distribution.Select(bucket => bucket.Code).SequenceEqual(new[] { "Green", "Yellow", "Red", "DeepRed" }, StringComparer.Ordinal) &&
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
    var summary = review.CapacityProtectionSummary
        ?? throw new InvalidOperationException("historical capacity protection summary is missing");
    AssertEqual<decimal?>(
        expectedProtective,
        summary.AverageProtectionBand,
        "summary average weekly protection band");
    AssertEqual<decimal?>(
        expectedRemaining,
        summary.AverageUnusedProtection,
        "summary average weekly unused protection");
}

static void TestHistoryOperatingOutcomesDoNotOwnProtection()
{
    var review = new HistoryReviewWorkspaceService(
        new SeedHistoryOperatingFactSource(SeedData.Create()),
        new SeedScenarioWorkspaceDataSource(SeedData.Create())).GetReview(6);
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));
    var historyReviewBody = SourceFunctionBody(script, "renderHistoryReview");
    var kpiStart = historyReviewBody.IndexOf("byId(\"history-review-kpis\")", StringComparison.Ordinal);
    var kpiEnd = historyReviewBody.IndexOf("].map(item => stageKpi", kpiStart, StringComparison.Ordinal);
    var operatingKpiList = kpiStart >= 0 && kpiEnd > kpiStart
        ? historyReviewBody.Substring(kpiStart, kpiEnd - kpiStart)
        : string.Empty;

    AssertTrue(review.OperatingOutcomes.RemainingProtectionPercent is null,
        "operating outcomes must retain the compatible property as null");
    AssertTrue(!operatingKpiList.Contains("剩余保护", StringComparison.Ordinal),
        "historical operating-result KPIs must not render remaining protection");
    AssertTrue(!operatingKpiList.Contains("outcomes.remainingProtectionPercent", StringComparison.Ordinal),
        "historical operating-result KPIs must not read remaining protection");
}

static void TestHistoryCapacitySummaryExposesProtectionRisk()
{
    var review = new HistoryReviewWorkspaceService(
        new SeedHistoryOperatingFactSource(SeedData.Create()),
        new SeedScenarioWorkspaceDataSource(SeedData.Create())).GetReview(6);
    var summary = review.CapacityProtectionSummary
        ?? throw new InvalidOperationException("historical capacity protection summary is missing");

    AssertEqual("RES-AIT", summary.ResourceCode, "summary upstream resource");
    AssertEqual("RES-HARNESS", summary.ProtectedCcrResourceCode, "summary protected CCR resource");
    AssertEqual<decimal?>(31.5m, summary.AverageProtectionBand, "summary average protection band");
    AssertEqual<decimal?>(29.2m, summary.AverageUnusedProtection, "summary average unused protection");
    AssertEqual<decimal?>(92.6m, summary.BalancePercent, "summary protection balance");
    AssertEqual<decimal?>(0m, summary.MinimumBalancePercent, "summary minimum weekly balance");
    AssertTrue(summary.ExhaustedWeekCount > 0, "summary should expose at least one exhausted week");
    AssertTrue(summary.OverloadWeekCount > 0, "summary should expose at least one overload week");
    AssertEqual("Complete", summary.EvidenceStatus, "summary evidence status");
}

static void TestHistoryCapacitySummaryDoesNotSubstituteMissingWeeklyEvidence()
{
    var source = new CapacityFactTransformingHistoryOperatingFactSource(
        new SeedHistoryOperatingFactSource(SeedData.Create()),
        facts => facts.Select(item => item.ResourceCode == "RES-AIT" && item.WeekOffset == -1
            ? item with { PlannedAvailableCapacity = null }
            : item).ToList());
    var review = new HistoryReviewWorkspaceService(
        source,
        new SeedScenarioWorkspaceDataSource(SeedData.Create())).GetReview(6);
    var summary = review.CapacityProtectionSummary
        ?? throw new InvalidOperationException("historical capacity protection summary is missing");

    AssertEqual("EvidenceMissing", summary.EvidenceStatus, "missing weekly capacity evidence status");
    AssertTrue(
        summary.AverageProtectionBand is null &&
        summary.AverageUnusedProtection is null &&
        summary.BalancePercent is null &&
        summary.MinimumBalancePercent is null,
        "missing weekly capacity evidence must leave summary quantities null");
    AssertEqual<int?>(null, summary.ExhaustedWeekCount, "missing weekly capacity evidence exhausted-week count");
    AssertEqual<int?>(null, summary.OverloadWeekCount, "missing weekly capacity evidence overload-week count");
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
    var facts = new SeedHistoryOperatingFactSource()
        .Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 1)));
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
    AssertEqual<string?>(null, snapshots[0].ParameterChangeReason, "V1 sizing snapshot must retain its missing source reason");
    AssertEqual("DDMRP 参数快照更新", snapshots[1].ParameterChangeReason, "V2 sizing snapshot must carry its seeded source reason");
    using (var snapshotJson = JsonDocument.Parse(JsonSerializer.Serialize(
        snapshots[1], new JsonSerializerOptions(JsonSerializerDefaults.Web))))
    {
        AssertEqual("DDMRP 参数快照更新", snapshotJson.RootElement
            .GetProperty("parameterChangeReason").GetString(),
            "history API snapshot JSON must serialize the source parameter-change reason");
    }
    var priorSizing = snapshots[0].Sizing ?? throw new InvalidOperationException("prior sizing evidence is missing");
    var currentSizing = snapshots[1].Sizing ?? throw new InvalidOperationException("current sizing evidence is missing");
    var expectedPrior = ExpectedRollingHistorySizing(facts, sku, -27);
    var expectedCurrent = ExpectedRollingHistorySizing(facts, sku, -26);
    AssertEqual<decimal?>(expectedPrior.Zones.TopOfRed, priorPoint.TopOfRed, "week -27 rolling top of red from prior snapshot");
    AssertEqual<decimal?>(expectedCurrent.Zones.TopOfRed, currentPoint.TopOfRed, "week -26 rolling top of red from current snapshot");
    AssertEqual<decimal?>(expectedPrior.Zones.TopOfGreen, priorPoint.TopOfGreen, "week -27 rolling top of green from prior snapshot");
    AssertEqual<decimal?>(expectedCurrent.Zones.TopOfGreen, currentPoint.TopOfGreen, "week -26 rolling top of green from current snapshot");
    AssertTrue(
        priorPoint.TopOfGreen != priorSizing.Zones.TopOfGreen || currentPoint.TopOfGreen != currentSizing.Zones.TopOfGreen,
        "weekly historical zones must not stay flat at the registered V1/V2 snapshot ADU");

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
        .Points.Single(item => item.WeekOffset == -16);
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
        invalidInventoryPoint.TargetNetFlowPosition is null &&
        invalidInventoryPoint.EvidenceStatus == "EvidenceMissing",
        "an unreconciled inventory point must not expose zone tops or target NFP from an otherwise valid snapshot");
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

    if (normal.OperatingOutcomes.CashOccupied is null)
    {
        failures.Add("shared operating ledger did not provide cash evidence");
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

static void TestHistoricalOutcomeCostsUseAnnualValidEventRules()
{
    var seed = SeedData.Create();
    var service = new HistoryReviewWorkspaceService(
        new InvalidAbnormalCostLedgerHistoryOperatingFactSource(new SeedHistoryOperatingFactSource(seed)),
        new SeedScenarioWorkspaceDataSource(seed));
    var recent = service.GetReview(6);
    var annual = service.GetReview(12);

    AssertEqual<decimal?>(
        50_000m,
        recent.OperatingOutcomes.ExpediteCost,
        "six-month cost excludes annual duplicate IDs, non-positive values, missing metadata and incomplete evidence");
    AssertEqual<decimal?>(
        50_000m,
        annual.OperatingOutcomes.ExpediteCost,
        "annual cost excludes duplicate IDs, non-positive values, missing metadata and incomplete evidence");
    AssertEqual(
        annual.OperatingOutcomes.ExpediteCost,
        recent.OperatingOutcomes.ExpediteCost,
        "overlapping six-month and annual valid-event cost basis");
}

static void TestCurrentBaselineReconcilesHistoryClosingBalances()
{
    var data = SeedData.Create();
    var facts = new SeedInternalDemoOperatingFactSource(data).Load();
    var candidate = new SeedCurrentBaselineDataSource(data).GetCandidate();
    var reconciliation = candidate.Payload.HistoryReconciliation;

    AssertTrue(reconciliation is not null, "current baseline should expose history reconciliation lineage");
    AssertEqual(facts.Header.FactSetId, reconciliation!.FactSetId, "reconciliation fact-set id");
    AssertTrue(
        DateTimeOffset.Parse(reconciliation.HistoryThroughUtc) < DateTimeOffset.Parse(reconciliation.BaselineAsOfUtc),
        "history cutoff must precede the baseline cutoff");
    AssertEqual(12, reconciliation.Lines.Count(item => item.MetricCode == "ON_HAND"), "SKU quantity reconciliation count");
    AssertEqual(1, reconciliation.Lines.Count(item => item.MetricCode == "INVENTORY_VALUE" && item.ItemKey == "ALL"), "inventory value reconciliation count");
    AssertEqual(1, reconciliation.Lines.Count(item => item.MetricCode == "WORK_IN_PROCESS" && item.ItemKey == "ALL"), "WIP reconciliation count");
    AssertEqual(1, reconciliation.Lines.Count(item => item.MetricCode == "BACKLOG" && item.ItemKey == "ALL"), "backlog reconciliation count");
    AssertEqual(data.Resources.Count, reconciliation.Lines.Count(item => item.MetricCode == "RESOURCE_AVAILABLE_CAPACITY"), "resource capacity reconciliation count");
    AssertTrue(reconciliation.Lines.All(item => item.EvidenceStatus == "Complete" && item.Difference == 0m), "all shared balance bridges should reconcile exactly");
    AssertEqual("Complete", reconciliation.EvidenceStatus, "reconciliation evidence status");

    var section = candidate.Sections.Single(item => item.SectionCode == "HISTORY_RECONCILIATION");
    AssertTrue(section.IsRequired, "history reconciliation evidence should be required");
    AssertEqual("Complete", section.CompletenessStatus, "history reconciliation section completeness");
    AssertEqual(reconciliation.Lines.Count, section.Items!.Count, "history reconciliation evidence item count");
    AssertTrue(section.Items.All(item => item.CompletenessStatus == "Complete" && !item.BlocksFreeze), "balanced reconciliation evidence should not block freeze");
}

static void TestCurrentBaselineBlocksMissingHistoryReconciliation()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-baseline-missing-reconciliation-{Guid.NewGuid():N}.db");
    try
    {
        var complete = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
        var candidate = complete with { Payload = complete.Payload with { HistoryReconciliation = null } };
        var service = new CurrentBaselineService(new FixedCurrentBaselineDataSource(candidate), databasePath);
        var rejected = false;
        try
        {
            service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", null));
        }
        catch (ArgumentException ex)
        {
            rejected = ex.Message.Contains("历史期末与当前基线对账失败", StringComparison.Ordinal);
        }

        AssertTrue(rejected, "missing history reconciliation should reject freezing");
        AssertEqual(0, service.List(10).Count, "missing reconciliation must not persist a snapshot");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestCurrentBaselineBlocksUnbalancedHistoryReconciliation()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-baseline-unbalanced-reconciliation-{Guid.NewGuid():N}.db");
    try
    {
        var complete = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
        var reconciliation = complete.Payload.HistoryReconciliation! with
        {
            Lines = complete.Payload.HistoryReconciliation!.Lines
                .Select((item, index) => index == 0 ? item with { Difference = item.Difference + 1m } : item)
                .ToList()
        };
        var candidate = complete with { Payload = complete.Payload with { HistoryReconciliation = reconciliation } };
        var service = new CurrentBaselineService(new FixedCurrentBaselineDataSource(candidate), databasePath);
        var rejected = false;
        try
        {
            service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", null));
        }
        catch (ArgumentException ex)
        {
            rejected = ex.Message.Contains("历史期末与当前基线对账失败", StringComparison.Ordinal);
        }

        AssertTrue(rejected, "unbalanced history reconciliation should reject freezing");
        AssertEqual(0, service.List(10).Count, "unbalanced reconciliation must not persist a snapshot");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestCurrentBaselineBlocksIncompleteReconciliationKeyCoverage()
{
    var complete = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
    var reconciliation = complete.Payload.HistoryReconciliation!;
    var requiredLines = new List<(string Name, BaselineReconciliationLine Line)>
    {
        ("ON_HAND", reconciliation.Lines.First(item => item.MetricCode == "ON_HAND")),
        ("INVENTORY_VALUE", reconciliation.Lines.Single(item => item.MetricCode == "INVENTORY_VALUE" && item.ItemKey == "ALL")),
        ("WORK_IN_PROCESS", reconciliation.Lines.Single(item => item.MetricCode == "WORK_IN_PROCESS" && item.ItemKey == "ALL")),
        ("BACKLOG", reconciliation.Lines.Single(item => item.MetricCode == "BACKLOG" && item.ItemKey == "ALL"))
    };
    requiredLines.AddRange(reconciliation.Lines
        .Where(item => item.MetricCode == "RESOURCE_AVAILABLE_CAPACITY")
        .Select(item => ($"RESOURCE_AVAILABLE_CAPACITY/{item.ItemKey}", item)));

    foreach (var (name, removed) in requiredLines)
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-baseline-reconciliation-coverage-{Guid.NewGuid():N}.db");
        try
        {
            var candidate = complete with
            {
                Payload = complete.Payload with
                {
                    HistoryReconciliation = reconciliation with
                    {
                        Lines = reconciliation.Lines.Where(item => item != removed).ToList()
                    }
                }
            };
            var service = new CurrentBaselineService(new FixedCurrentBaselineDataSource(candidate), databasePath);
            var rejected = false;
            try
            {
                service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", name));
            }
            catch (ArgumentException ex)
            {
                rejected = ex.Message.Contains("历史期末与当前基线对账失败", StringComparison.Ordinal);
            }

            AssertTrue(rejected, $"missing {name} reconciliation key should reject freezing");
            AssertEqual(0, service.List(10).Count, $"missing {name} must not persist a snapshot");
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            DeleteSqliteFiles(databasePath);
        }
    }
}

static void TestCurrentBaselineRequiresExactHistoryReconciliationEvidenceSection()
{
    var complete = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
    var section = complete.Sections.Single(item => item.SectionCode == "HISTORY_RECONCILIATION");
    var cases = new List<(string Name, CurrentBaselineCandidate Candidate)>
    {
        ("missing section", complete with { Sections = complete.Sections.Where(item => item.SectionCode != "HISTORY_RECONCILIATION").ToList() }),
        ("duplicate section", complete with { Sections = complete.Sections.Append(section).ToList() }),
        ("missing evidence item", complete with
        {
            Sections = complete.Sections.Select(item => item.SectionCode == "HISTORY_RECONCILIATION"
                ? item with { Items = item.Items!.Skip(1).ToList() }
                : item).ToList()
        }),
        ("unexpected evidence item", complete with
        {
            Sections = complete.Sections.Select(item => item.SectionCode == "HISTORY_RECONCILIATION"
                ? item with { Items = item.Items!.Append(new BaselineEvidenceItem("UNEXPECTED", "unexpected", "Fresh", "Complete", false)).ToList() }
                : item).ToList()
        })
    };

    foreach (var (name, candidate) in cases)
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-baseline-reconciliation-section-{Guid.NewGuid():N}.db");
        try
        {
            var service = new CurrentBaselineService(new FixedCurrentBaselineDataSource(candidate), databasePath);
            var rejected = false;
            try
            {
                service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", name));
            }
            catch (ArgumentException ex)
            {
                rejected = ex.Message.Contains("历史期末与当前基线对账失败", StringComparison.Ordinal);
            }

            AssertTrue(rejected, $"{name} should reject freezing");
            AssertEqual(0, service.List(10).Count, $"{name} must not persist a snapshot");
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            DeleteSqliteFiles(databasePath);
        }
    }
}

static void TestCurrentBaselineRecomputesHistoryReconciliationDifference()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-baseline-reconciliation-math-{Guid.NewGuid():N}.db");
    try
    {
        var complete = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
        var reconciliation = complete.Payload.HistoryReconciliation! with
        {
            Lines = complete.Payload.HistoryReconciliation!.Lines
                .Select((item, index) => index == 0 ? item with { BaselineBalance = item.BaselineBalance + 1m } : item)
                .ToList()
        };
        var candidate = complete with { Payload = complete.Payload with { HistoryReconciliation = reconciliation } };
        var service = new CurrentBaselineService(new FixedCurrentBaselineDataSource(candidate), databasePath);
        var rejected = false;
        try
        {
            service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", null));
        }
        catch (ArgumentException ex)
        {
            rejected = ex.Message.Contains("历史期末与当前基线对账失败", StringComparison.Ordinal);
        }

        AssertTrue(rejected, "freeze should recompute and reject a tampered baseline balance");
        AssertEqual(0, service.List(10).Count, "tampered reconciliation must not persist a snapshot");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestCurrentBaselineBindsReconciliationBalancesToCandidatePayload()
{
    var complete = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
    var inventorySku = complete.Payload.Inventory[0].Sku;
    var resourceCode = complete.Payload.ResourceAvailability[0].ResourceCode;
    var cases = new List<(string Name, CurrentBaselineCandidate Candidate)>
    {
        ("ON_HAND", complete with
        {
            Payload = complete.Payload with
            {
                Inventory = complete.Payload.Inventory
                    .Select(item => item.Sku == inventorySku ? item with { OnHand = item.OnHand + 1m } : item)
                    .ToList()
            }
        }),
        ("INVENTORY_VALUE", complete with
        {
            Payload = complete.Payload with
            {
                PlanningInputs = complete.Payload.PlanningInputs! with
                {
                    Skus = complete.Payload.PlanningInputs!.Skus
                        .Select((item, index) => index == 0 ? item with { UnitCost = item.UnitCost + 1m } : item)
                        .ToList()
                }
            }
        }),
        ("WORK_IN_PROCESS", complete with
        {
            Payload = complete.Payload with
            {
                WorkInProcess = complete.Payload.WorkInProcess
                    .Select((item, index) => index == 0 ? item with { Quantity = item.Quantity + 1m } : item)
                    .ToList()
            }
        }),
        ("BACKLOG", complete with
        {
            Payload = complete.Payload with
            {
                Backlog = complete.Payload.Backlog
                    .Select((item, index) => index == 0 ? item with { Quantity = item.Quantity + 1m } : item)
                    .ToList()
            }
        }),
        ("RESOURCE_AVAILABLE_CAPACITY", complete with
        {
            Payload = complete.Payload with
            {
                ResourceAvailability = complete.Payload.ResourceAvailability
                    .Select(item => item.ResourceCode == resourceCode
                        ? item with { AvailableCapacity = item.AvailableCapacity + 1m }
                        : item)
                    .ToList()
            }
        })
    };

    foreach (var (name, candidate) in cases)
    {
        AssertHistoryReconciliationFreezeIsBlocked(candidate, $"candidate payload {name} mismatch", name);
    }
}

static void TestCurrentBaselineBindsReconciliationLineageToCandidatePlanningInputs()
{
    var complete = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
    var reconciliation = complete.Payload.HistoryReconciliation!;
    var historyThrough = DateTimeOffset.Parse(reconciliation.HistoryThroughUtc);
    var baselineAsOf = DateTimeOffset.Parse(reconciliation.BaselineAsOfUtc);
    var cases = new List<(string Name, string ExpectedIssue, CurrentBaselineCandidate Candidate)>
    {
        ("reconciliation fact set", "事实集标识", complete with
        {
            Payload = complete.Payload with
            {
                HistoryReconciliation = reconciliation with { FactSetId = $"{reconciliation.FactSetId}-TAMPERED" }
            }
        }),
        ("reconciliation history cutoff", "历史截止时间", complete with
        {
            Payload = complete.Payload with
            {
                HistoryReconciliation = reconciliation with
                {
                    HistoryThroughUtc = historyThrough.AddMinutes(-1).ToString("O")
                }
            }
        }),
        ("reconciliation baseline cutoff", "基线时点", complete with
        {
            Payload = complete.Payload with
            {
                HistoryReconciliation = reconciliation with
                {
                    BaselineAsOfUtc = baselineAsOf.AddMinutes(1).ToString("O")
                }
            }
        }),
        ("candidate as-of cutoff", "候选基线时点", complete with { AsOfUtc = baselineAsOf.AddMinutes(1).ToString("O") }),
        ("planning fact set", "事实集标识", complete with
        {
            Payload = complete.Payload with
            {
                PlanningInputs = complete.Payload.PlanningInputs! with
                {
                    FactSetId = $"{complete.Payload.PlanningInputs!.FactSetId}-TAMPERED"
                }
            }
        }),
        ("planning history cutoff", "历史截止时间", complete with
        {
            Payload = complete.Payload with
            {
                PlanningInputs = complete.Payload.PlanningInputs! with
                {
                    HistoryThroughUtc = historyThrough.AddMinutes(-1).ToString("O")
                }
            }
        }),
        ("planning baseline cutoff", "基线时点", complete with
        {
            Payload = complete.Payload with
            {
                PlanningInputs = complete.Payload.PlanningInputs! with
                {
                    BaselineAsOfUtc = baselineAsOf.AddMinutes(1).ToString("O")
                }
            }
        })
    };

    foreach (var (name, expectedIssue, candidate) in cases)
    {
        AssertHistoryReconciliationFreezeIsBlocked(candidate, $"candidate lineage {name} mismatch", expectedIssue);
    }
}

static void TestCurrentBaselineBlocksNullHistoryReconciliationLines()
{
    var complete = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
    var candidate = complete with
    {
        Payload = complete.Payload with
        {
            HistoryReconciliation = complete.Payload.HistoryReconciliation! with { Lines = null! }
        }
    };
    AssertHistoryReconciliationFreezeIsBlocked(candidate, "null reconciliation lines");
}

static void TestCurrentBaselineBlocksNullHistoryReconciliationLineElements()
{
    var complete = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
    var reconciliation = complete.Payload.HistoryReconciliation!;
    var linesWithNullElement = reconciliation.Lines.Append(null!).ToList();
    var candidate = complete with
    {
        Payload = complete.Payload with
        {
            HistoryReconciliation = reconciliation with { Lines = linesWithNullElement }
        }
    };

    AssertHistoryReconciliationFreezeIsBlocked(candidate, "null reconciliation line element", "对账行包含空元素");
}

static void TestCurrentBaselineBlocksNonCompleteHistoryReconciliation()
{
    var complete = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
    var candidate = complete with
    {
        Payload = complete.Payload with
        {
            HistoryReconciliation = complete.Payload.HistoryReconciliation! with { EvidenceStatus = "EvidenceMissing" }
        }
    };
    AssertHistoryReconciliationFreezeIsBlocked(candidate, "non-complete reconciliation lineage");
}

static void TestCurrentBaselineBlocksDuplicateHistoryReconciliationKey()
{
    var complete = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
    var reconciliation = complete.Payload.HistoryReconciliation!;
    var candidate = complete with
    {
        Payload = complete.Payload with
        {
            HistoryReconciliation = reconciliation with { Lines = reconciliation.Lines.Append(reconciliation.Lines[0]).ToList() }
        }
    };
    AssertHistoryReconciliationFreezeIsBlocked(candidate, "duplicate reconciliation key");
}

static void TestCurrentBaselineBlocksUnexpectedHistoryReconciliationKey()
{
    var complete = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
    var reconciliation = complete.Payload.HistoryReconciliation!;
    var extra = new BaselineReconciliationLine("ON_HAND", "UNEXPECTED-SKU", 0m, 0m, 0m, 0m, 0m, 0m, "Complete");
    var candidate = complete with
    {
        Payload = complete.Payload with
        {
            HistoryReconciliation = reconciliation with { Lines = reconciliation.Lines.Append(extra).ToList() }
        },
        Sections = complete.Sections.Select(item => item.SectionCode == "HISTORY_RECONCILIATION"
            ? item with
            {
                Items = item.Items!.Append(new BaselineEvidenceItem("ON_HAND/UNEXPECTED-SKU", "unexpected", "Fresh", "Complete", false)).ToList(),
                ItemCount = item.ItemCount + 1
            }
            : item).ToList()
    };
    AssertHistoryReconciliationFreezeIsBlocked(candidate, "unexpected reconciliation key");
}

static void AssertHistoryReconciliationFreezeIsBlocked(
    CurrentBaselineCandidate candidate,
    string caseName,
    string? expectedIssue = null)
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-baseline-reconciliation-{Guid.NewGuid():N}.db");
    try
    {
        var service = new CurrentBaselineService(new FixedCurrentBaselineDataSource(candidate), databasePath);
        var rejected = false;
        try
        {
            service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", caseName));
        }
        catch (ArgumentException ex)
        {
            rejected = ex.Message.Contains("历史期末与当前基线对账失败", StringComparison.Ordinal) &&
                (expectedIssue is null || ex.Message.Contains(expectedIssue, StringComparison.Ordinal));
        }

        AssertTrue(rejected, $"{caseName} should reject freezing with the reconciliation validation error");
        AssertEqual(0, service.List(10).Count, $"{caseName} must not persist a snapshot");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestRuntimeSeedRegistrationsUseSharedFactSource()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var program = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Program.cs"));
    foreach (var registration in new[]
    {
        "AddSingleton<IScenarioWorkspaceDataSource>(sp =>",
        "AddSingleton<IHistoryOperatingFactSource>(sp =>",
        "AddSingleton<ICurrentBaselineDataSource>(sp =>"
    })
    {
        var start = program.IndexOf(registration, StringComparison.Ordinal);
        var end = start < 0 ? -1 : program.IndexOf("));", start, StringComparison.Ordinal);
        AssertTrue(start >= 0 && end > start && program[start..end].Contains("GetRequiredService<IInternalDemoOperatingFactSource>()", StringComparison.Ordinal),
            $"{registration} should resolve the shared fact source");
    }

    var data = SeedData.Create();
    var source = new SeedInternalDemoOperatingFactSource(data);
    var facts = source.Load();
    var scenarioSource = new SeedScenarioWorkspaceDataSource(data, source);
    var historySource = new SeedHistoryOperatingFactSource(data, source);
    var scenario = scenarioSource.Load(new ScenarioWorkspaceDataRequest(52, new DateOnly(2026, 6, 30)));
    var history = new HistoryReviewWorkspaceService(historySource, scenarioSource).GetReview(12);
    var candidate = new SeedCurrentBaselineDataSource(data, scenarioSource, source).GetCandidate();
    var reconciliation = candidate.Payload.HistoryReconciliation!;

    AssertEqual(facts.Header.FactSetId, scenario.FactSetId, "scenario runtime fact-set id");
    AssertEqual(facts.Header.FactSetId, history.FactSetId, "history runtime fact-set id");
    AssertEqual(facts.Header.FactSetId, reconciliation.FactSetId, "baseline runtime fact-set id");
    AssertEqual(facts.Header.HistoryThroughUtc, scenario.HistoryThroughUtc, "scenario runtime history cutoff");
    AssertEqual(facts.Header.HistoryThroughUtc, history.HistoryThroughUtc, "history runtime history cutoff");
    AssertEqual(facts.Header.HistoryThroughUtc, reconciliation.HistoryThroughUtc, "baseline runtime history cutoff");
    AssertEqual(facts.Header.BaselineAsOfUtc, scenario.BaselineAsOfUtc, "scenario runtime baseline cutoff");
    AssertEqual(facts.Header.BaselineAsOfUtc, reconciliation.BaselineAsOfUtc, "baseline runtime baseline cutoff");
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
        var inventoryBridge = new SeedInternalDemoOperatingFactSource(validationData).Load().BalanceBridges
            .Single(item => item.MetricCode == "INVENTORY_VALUE" && item.ItemKey == "ALL");
        AssertEqual(
            inventoryBridge.HistoryClosingBalance + inventoryBridge.IntervalIncrease - inventoryBridge.IntervalDecrease + inventoryBridge.Adjustment,
            inventoryBridge.BaselineBalance,
            "shared inventory bridge reconciliation");
        AssertEqual(inventoryBridge.BaselineBalance, kpis.InventoryValue, "snapshot inventory value");
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
            .Load(new ScenarioWorkspaceDataRequest(52, new DateOnly(2026, 6, 30)));
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
                baseData,
                basePlanningInputs with { OpeningBacklog = Array.Empty<OpeningBacklogEvidence>() },
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
                rejected = ex.Message.Contains("CURRENT_KPIS", StringComparison.Ordinal) ||
                    ex.Message.Contains("历史期末与当前基线对账失败", StringComparison.Ordinal);
            }
            if (!rejected)
            {
                failures.Add($"{item.Name} did not block freezing through a required evidence validator");
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
        renderRule.Contains("baselineCandidateFreezeBlockingIssues(candidate)", StringComparison.Ordinal),
        "baseline renderer should combine item-level blockers with required typed planning input presence");
    AssertTrue(
        !renderRule.Contains("item.isRequired &&", StringComparison.Ordinal),
        "baseline renderer must not treat a nonblocking missing item as a required-section freeze blocker");
}

static void TestHistoryInventoryProjectionUsesRollingSkuDemand()
{
    const string sku = "AV-COM-201";
    var seed = SeedData.Create();
    var facts = new SeedHistoryOperatingFactSource(seed)
        .Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 1)));
    var definitions = new SeedScenarioWorkspaceDataSource(seed)
        .Load(new ScenarioWorkspaceDataRequest(52, new DateOnly(2026, 6, 1)));
    var projection = HistoryReviewProjectionBuilder.Build(facts, definitions, 3);
    var view = projection.InventoryBuffers.Single(item => item.Sku == sku);
    var sourceByWeek = facts.BufferFacts
        .Where(item => item.Sku == sku)
        .ToDictionary(item => item.WeekOffset);

    AssertEqual(52, view.Points.Count, "annual rolling inventory point count");
    foreach (var point in view.Points)
    {
        var source = sourceByWeek[point.WeekOffset];
        var expected = ExpectedRollingHistorySizing(facts, sku, point.WeekOffset);
        AssertEqual(source.ActualDemand, point.ActualDemand, $"week {point.WeekOffset} actual demand projection");
        AssertEqual(source.DemandSpikeThreshold, point.DemandSpikeThreshold, $"week {point.WeekOffset} demand threshold projection");
        AssertEqual<decimal?>(expected.Zones.TopOfRed, point.TopOfRed, $"week {point.WeekOffset} rolling top of red");
        AssertEqual<decimal?>(expected.Zones.TopOfYellow, point.TopOfYellow, $"week {point.WeekOffset} rolling top of yellow");
        AssertEqual<decimal?>(expected.Zones.TopOfGreen, point.TopOfGreen, $"week {point.WeekOffset} rolling top of green");
    }

    var distinctZoneHeights = view.Points
        .Select(point => (point.TopOfRed, point.TopOfYellow, point.TopOfGreen))
        .Distinct()
        .Count();
    AssertTrue(distinctZoneHeights >= 3, "52-week history must contain at least three traceable weekly zone-height sets");
    var recentFacts = new SeedHistoryOperatingFactSource(seed)
        .Load(new HistoryFactRequest(26, new DateOnly(2026, 6, 1)));
    var recentView = HistoryReviewProjectionBuilder.Build(recentFacts, definitions, 3)
        .InventoryBuffers.Single(item => item.Sku == sku);
    AssertTrue(
        recentView.Points
            .Select(point => (point.TopOfRed, point.TopOfYellow, point.TopOfGreen))
            .Distinct()
            .Count() >= 3,
        "26-week history must contain at least three traceable weekly zone-height sets");

    var poisonedOperatingFacts = facts with
    {
        OperatingFacts = facts.OperatingFacts
            .Select(item => item with { ActualDemand = 999_999m, DemandSpikeThreshold = 1m, TargetNetFlowPosition = 1m })
            .ToList()
    };
    var poisonedProjection = HistoryReviewProjectionBuilder.Build(poisonedOperatingFacts, definitions, 3);
    AssertEqual(
        JsonSerializer.Serialize(projection.InventoryBuffers),
        JsonSerializer.Serialize(poisonedProjection.InventoryBuffers),
        "per-SKU history projection must not read global weekly operating facts");

    var qualifiedDemandPoisonedFacts = facts with
    {
        BufferFacts = facts.BufferFacts
            .Select(item => item.Sku == sku
                ? item with
                {
                    QualifiedDemand = item.QualifiedDemand + 999m,
                    EndingNetFlow = item.EndingNetFlow - 999m
                }
                : item)
            .ToList()
    };
    var qualifiedDemandPoisonedView = HistoryReviewProjectionBuilder.Build(qualifiedDemandPoisonedFacts, definitions, 3)
        .InventoryBuffers.Single(item => item.Sku == sku);
    AssertTrue(
        view.Points.Zip(qualifiedDemandPoisonedView.Points)
            .All(pair => (pair.First.TopOfRed, pair.First.TopOfYellow, pair.First.TopOfGreen) ==
                (pair.Second.TopOfRed, pair.Second.TopOfYellow, pair.Second.TopOfGreen)),
        "qualified-demand poison must not change rolling historical ADU zones");

    var zeroDemandFacts = facts with
    {
        BufferFacts = facts.BufferFacts
            .Select(item => item.Sku == sku && item.WeekOffset == -5
                ? item with
                {
                    ActualDemand = 0m
                }
                : item)
            .ToList()
    };
    var zeroDemandPoint = HistoryReviewProjectionBuilder.Build(zeroDemandFacts, definitions, 3)
        .InventoryBuffers.Single(item => item.Sku == sku)
        .Points.Single(item => item.WeekOffset == -5);
    AssertEqual<decimal?>(0m, zeroDemandPoint.ActualDemand, "explicit zero weekly demand evidence");
    AssertTrue(
        zeroDemandPoint.TopOfGreen is > 0m && zeroDemandPoint.EvidenceStatus == "Complete",
        "an explicit zero-demand week must remain valid rolling evidence rather than becoming a gap");
}

static void TestHistoryInventoryEvidenceChecksBothEquations()
{
    const string sku = "AV-COM-201";
    const int poisonedWeek = -8;
    var seed = SeedData.Create();
    var review = new HistoryReviewWorkspaceService(
        new OpeningOnHandContinuityPoisonHistoryOperatingFactSource(
            new SeedHistoryOperatingFactSource(seed), sku, poisonedWeek),
        new SeedScenarioWorkspaceDataSource(seed)).GetReview(12);
    var points = (review.InventoryBuffers ?? throw new InvalidOperationException("inventory projection is missing"))
        .Single(item => item.Sku == sku)
        .Points;
    var earliest = points.Single(item => item.WeekOffset == -52);
    var poisoned = points.Single(item => item.WeekOffset == poisonedWeek);

    AssertTrue(
        earliest.OpeningOnHand.HasValue &&
        earliest.EvidenceChecks?.Single(item => item.Code == "InventoryContinuity").Status == "Complete",
        "the earliest annual point must retain explicit historical opening evidence");
    AssertEqual("EvidenceMissing", poisoned.EvidenceStatus, "poisoned opening stock evidence status");
    AssertEqual(
        "EvidenceMissing",
        poisoned.EvidenceChecks?.Single(item => item.Code == "InventoryContinuity").Status,
        "poisoned opening stock continuity evidence");
    AssertTrue(
        points.Where(item => item.WeekOffset != poisonedWeek).All(item => item.EvidenceStatus == "Complete"),
        "only the point with poisoned opening stock should lose evidence");
    AssertTrue(
        poisoned.EvidenceChecks?.Single(item => item.Code == "InventoryEquation").Status == "EvidenceMissing" &&
        poisoned.EvidenceChecks?.Single(item => item.Code == "NetFlowEquation").Status == "Complete",
        "stock and net-flow equations should be independently evidenced");
}

static void TestHistoryProjectionSeparatesReasons()
{
    const string sku = "AV-COM-201";
    const int transitionWeek = -26;
    var seed = SeedData.Create();
    var review = new HistoryReviewWorkspaceService(
        new SeedHistoryOperatingFactSource(seed),
        new SeedScenarioWorkspaceDataSource(seed)).GetReview(12);
    var points = review.InventoryBuffers!.Single(item => item.Sku == sku).Points;
    var point = points.Single(item => item.WeekOffset == transitionWeek);

    AssertEqual("无事件", point.WeeklyEvent, "normal weekly event evidence");
    AssertEqual("DDMRP 参数快照更新", point.ParameterChangeReason, "seeded parameter-change reason");
    AssertTrue(
        !string.IsNullOrWhiteSpace(point.ParameterChangeReason) &&
        point.WeeklyEvent != point.ParameterChangeReason,
        "normal weekly event and snapshot parameter-change reason must remain distinct evidence");
    AssertEqual<string?>(null, points.Single(item => item.WeekOffset == -27).ParameterChangeReason,
        "V1 must not invent a parameter-change reason without source evidence");
}

static void TestHistoryProjectionOmitsTargetNetFlow()
{
    var data = SeedData.Create();
    var review = new HistoryReviewWorkspaceService(
        new SeedHistoryOperatingFactSource(data),
        new SeedScenarioWorkspaceDataSource(data)).GetReview(6);
    AssertTrue(review.InventoryBuffers!.SelectMany(item => item.Points)
        .All(point => point.TargetNetFlowPosition is null),
        "target net-flow position must remain absent");
}

static void TestHistoryReviewPreservesAnnualRollingContextAcrossRanges()
{
    var seed = SeedData.Create();
    var service = new HistoryReviewWorkspaceService(
        new SeedHistoryOperatingFactSource(seed),
        new SeedScenarioWorkspaceDataSource(seed));
    var recent = service.GetReview(6);
    var annual = service.GetReview(12);
    var recentViews = recent.InventoryBuffers ?? throw new InvalidOperationException("six-month inventory projection is missing");
    var annualViews = (annual.InventoryBuffers ?? throw new InvalidOperationException("annual inventory projection is missing"))
        .ToDictionary(item => item.Sku, StringComparer.Ordinal);
    AssertTrue(recentViews.Count > 0, "six-month inventory projection must contain at least one SKU");
    AssertEqual(annualViews.Count, recentViews.Count, "six-month and annual inventory SKU count");

    foreach (var recentView in recentViews)
    {
        AssertEqual(26, recentView.Points.Count, $"{recentView.Sku} six-month point count");
        var annualView = annualViews[recentView.Sku];
        AssertEqual(52, annualView.Points.Count, $"{recentView.Sku} annual point count");
        var annualByWeek = annualView.Points.ToDictionary(item => item.WeekOffset);
        foreach (var recentPoint in recentView.Points)
        {
            var annualPoint = annualByWeek[recentPoint.WeekOffset];
            AssertEqual(annualPoint.TopOfRed, recentPoint.TopOfRed, $"{recentView.Sku} week {recentPoint.WeekOffset} top of red across views");
            AssertEqual(annualPoint.TopOfYellow, recentPoint.TopOfYellow, $"{recentView.Sku} week {recentPoint.WeekOffset} top of yellow across views");
            AssertEqual(annualPoint.TopOfGreen, recentPoint.TopOfGreen, $"{recentView.Sku} week {recentPoint.WeekOffset} top of green across views");
            AssertEqual(annualPoint.EndingOnHand, recentPoint.EndingOnHand, $"{recentView.Sku} week {recentPoint.WeekOffset} ending on-hand across views");
            AssertEqual(annualPoint.NetFlow, recentPoint.NetFlow, $"{recentView.Sku} week {recentPoint.WeekOffset} net-flow across views");
            AssertEqual(annualPoint.ActualDemand, recentPoint.ActualDemand, $"{recentView.Sku} week {recentPoint.WeekOffset} actual demand across views");
            AssertEqual(annualPoint.DemandSpikeThreshold, recentPoint.DemandSpikeThreshold, $"{recentView.Sku} week {recentPoint.WeekOffset} demand threshold across views");
        }

    }

    var representativeView = recentViews.Single(item => item.Sku == "AV-COM-201");
    var representativeZoneHeights = representativeView.Points
        .Select(point => (point.TopOfRed, point.TopOfYellow, point.TopOfGreen))
        .Distinct()
        .ToList();
    AssertTrue(
        representativeZoneHeights.Count >= 2,
        "the representative six-month inventory view must retain multiple rolling zone-height sets from actual demand evidence");
}

static void TestHistoryTimeBufferProjectsOnlyFullyMatchedCostEvents()
{
    var seed = SeedData.Create();
    var facts = new SeedHistoryOperatingFactSource(seed)
        .Load(new HistoryFactRequest(26, new DateOnly(2026, 6, 1)));
    var definitions = new SeedScenarioWorkspaceDataSource(seed)
        .Load(new ScenarioWorkspaceDataRequest(26, new DateOnly(2026, 6, 1)));
    var projected = HistoryReviewProjectionBuilder.Build(facts, definitions, 3).TimeBuffers.Single();
    var costEvent = projected.AbnormalCostEvents?.Single()
        ?? throw new InvalidOperationException("fully matched time-buffer cost event is missing");

    AssertEqual("HAC-2026-002", costEvent.EventId, "time-buffer event ID");
    AssertEqual(-16, costEvent.WeekOffset, "time-buffer event week");
    AssertEqual("2026-02-09", costEvent.PeriodStartDate, "time-buffer event date");
    AssertEqual(420_000m, costEvent.CostAmount, "time-buffer event amount");
    AssertEqual("返工费用", costEvent.CostType, "time-buffer event cost type");
    AssertEqual("返工", costEvent.Cause, "time-buffer event reason");
    AssertEqual("时间缓冲", costEvent.TargetType, "time-buffer event target type");
    AssertEqual("MS-TB-001", costEvent.TargetId, "time-buffer event target");
    AssertEqual("热真空试验准备控制点", costEvent.ControlPoint, "time-buffer event control point");
    AssertEqual("DDAE 演示历史事实台账", costEvent.SourceAuthority, "time-buffer event source");
    AssertEqual("Complete", costEvent.EvidenceStatus, "time-buffer event evidence status");

    var annualFacts = new SeedHistoryOperatingFactSource(seed)
        .Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 1)));
    var annualView = HistoryReviewProjectionBuilder.Build(annualFacts, definitions, 3).TimeBuffers.Single();
    var annualEvents = (annualView.AbnormalCostEvents ?? throw new InvalidOperationException("annual cost-event strip is missing"))
        .ToDictionary(item => item.EventId, StringComparer.Ordinal);
    AssertEqual(4, annualEvents.Count, "annual global abnormal-cost event count");
    AssertEqual(("需求对象", "星载电子", "星载电子需求控制点"),
        (annualEvents["HAC-2025-004"].TargetType, annualEvents["HAC-2025-004"].TargetId, annualEvents["HAC-2025-004"].ControlPoint),
        "demand event ownership");
    AssertEqual(("库存控制点", "AV-FPGA-203", "关键进口 FPGA 库存控制点"),
        (annualEvents["HAC-2025-003"].TargetType, annualEvents["HAC-2025-003"].TargetId, annualEvents["HAC-2025-003"].ControlPoint),
        "inventory event ownership");
    AssertEqual(("能力对象", "RES-AIT", "AIT 总装集成大厅"),
        (annualEvents["HAC-2026-001"].TargetType, annualEvents["HAC-2026-001"].TargetId, annualEvents["HAC-2026-001"].ControlPoint),
        "capacity event ownership");
    AssertEqual(("时间缓冲", "MS-TB-001", "热真空试验准备控制点"),
        (annualEvents["HAC-2026-002"].TargetType, annualEvents["HAC-2026-002"].TargetId, annualEvents["HAC-2026-002"].ControlPoint),
        "time event ownership");
    var annualCostPoints = annualView.Points
        .Where(point => point.AbnormalCost.HasValue)
        .ToList();
    AssertEqual(
        1,
        annualCostPoints.Count,
        "global abnormal-cost events must not be attached to unrelated time-status points");
    AssertEqual(-16, annualCostPoints.Single().WeekOffset, "fully matched time cost point week");
    AssertEqual<decimal?>(420_000m, annualCostPoints.Single().AbnormalCost, "fully matched time cost point amount");

    var legacyView = new HistoryTimeBufferView(
        "LEGACY-TB",
        "旧控制点",
        "旧保护活动",
        Array.Empty<HistoryTimeBufferPoint>(),
        Array.Empty<HistoryDistributionBucket>(),
        "Complete");
    AssertTrue(legacyView.AbnormalCostEvents is null, "legacy time-buffer construction must retain a null optional event collection");

    var noCostFacts = new SeedHistoryOperatingFactSource(seed)
        .Load(new HistoryFactRequest(5, new DateOnly(2026, 6, 1)));
    var noCostView = HistoryReviewProjectionBuilder.Build(noCostFacts, definitions, 3).TimeBuffers.Single();
    AssertTrue(
        noCostView.EvidenceStatus == "Complete" &&
        noCostView.AbnormalCostEvents is { Count: 0 },
        "a complete period without abnormal costs must return an empty event collection");

    var mismatched = facts with
    {
        AbnormalCosts = facts.AbnormalCosts
            .Select(item => item.EventId == "HAC-2026-002" ? item with { TargetId = "OTHER-BUFFER" } : item)
            .ToList()
    };
    var mismatchedView = HistoryReviewProjectionBuilder.Build(mismatched, definitions, 3).TimeBuffers.Single();
    AssertEqual(1, mismatchedView.AbnormalCostEvents?.Count ?? 0, "object-mismatched global cost event count");
    AssertEqual("OTHER-BUFFER", mismatchedView.AbnormalCostEvents!.Single().TargetId, "object-mismatched global event ownership");
    AssertEqual(
        "EvidenceMissing",
        mismatchedView.Points.Single(item => item.WeekOffset == -16).EvidenceStatus,
        "object-mismatched time-buffer event evidence");
    AssertEqual<decimal?>(
        null,
        mismatchedView.Points.Single(item => item.WeekOffset == -16).AbnormalCost,
        "object-mismatched time-buffer event amount");

    var linkedEvent = facts.AbnormalCosts.Single(item => item.EventId == "HAC-2026-002");
    var invalidLinks = new (string Label, HistoryFactSet Facts, int ExpectedEventCount)[]
    {
        ("duplicate event ID", facts with
        {
            AbnormalCosts = facts.AbnormalCosts.Append(linkedEvent with { WeekOffset = -15 }).ToList()
        }, 0),
        ("wrong event week", facts with
        {
            AbnormalCosts = facts.AbnormalCosts.Select(item => item.EventId == linkedEvent.EventId
                ? item with { WeekOffset = -15 }
                : item).ToList()
        }, 1),
        ("wrong event amount", facts with
        {
            AbnormalCosts = facts.AbnormalCosts.Select(item => item.EventId == linkedEvent.EventId
                ? item with { CostAmount = item.CostAmount + 1m }
                : item).ToList()
        }, 1),
        ("wrong event control point", facts with
        {
            AbnormalCosts = facts.AbnormalCosts.Select(item => item.EventId == linkedEvent.EventId
                ? item with { ControlPoint = "OTHER-CONTROL-POINT" }
                : item).ToList()
        }, 1),
        ("wrong event cause", facts with
        {
            AbnormalCosts = facts.AbnormalCosts.Select(item => item.EventId == linkedEvent.EventId
                ? item with { Cause = "原因冲突" }
                : item).ToList()
        }, 1),
        ("wrong weekly-fact control point", facts with
        {
            TimeBufferFacts = (facts.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>())
                .Select(item => item.WeekOffset == -16
                    ? item with { ControlPoint = "OTHER-CONTROL-POINT" }
                    : item)
                .ToList()
        }, 1),
        ("missing event metadata", facts with
        {
            AbnormalCosts = facts.AbnormalCosts.Select(item => item.EventId == linkedEvent.EventId
                ? item with { SourceAuthority = null }
                : item).ToList()
        }, 0),
        ("missing event cost type", facts with
        {
            AbnormalCosts = facts.AbnormalCosts.Select(item => item.EventId == linkedEvent.EventId
                ? item with { CostType = string.Empty }
                : item).ToList()
        }, 0),
        ("missing event cause", facts with
        {
            AbnormalCosts = facts.AbnormalCosts.Select(item => item.EventId == linkedEvent.EventId
                ? item with { Cause = string.Empty }
                : item).ToList()
        }, 0),
        ("missing event target type", facts with
        {
            AbnormalCosts = facts.AbnormalCosts.Select(item => item.EventId == linkedEvent.EventId
                ? item with { TargetType = null }
                : item).ToList()
        }, 0),
        ("missing event target", facts with
        {
            AbnormalCosts = facts.AbnormalCosts.Select(item => item.EventId == linkedEvent.EventId
                ? item with { TargetId = null }
                : item).ToList()
        }, 0),
        ("missing event control point", facts with
        {
            AbnormalCosts = facts.AbnormalCosts.Select(item => item.EventId == linkedEvent.EventId
                ? item with { ControlPoint = null }
                : item).ToList()
        }, 0),
        ("non-positive event amount", facts with
        {
            AbnormalCosts = facts.AbnormalCosts.Select(item => item.EventId == linkedEvent.EventId
                ? item with { CostAmount = -1m }
                : item).ToList(),
            TimeBufferFacts = (facts.TimeBufferFacts ?? Array.Empty<WeeklyTimeBufferFact>())
                .Select(item => item.WeekOffset == -16
                    ? item with { AbnormalCost = -1m }
                    : item)
                .ToList()
        }, 0),
    };
    foreach (var invalid in invalidLinks)
    {
        var invalidView = HistoryReviewProjectionBuilder.Build(invalid.Facts, definitions, 3).TimeBuffers.Single();
        AssertEqual(invalid.ExpectedEventCount, invalidView.AbnormalCostEvents?.Count ?? 0, $"{invalid.Label} event count");
        AssertEqual(
            "EvidenceMissing",
            invalidView.Points.Single(item => item.WeekOffset == -16).EvidenceStatus,
            $"{invalid.Label} point evidence");
        AssertEqual<decimal?>(
            null,
            invalidView.Points.Single(item => item.WeekOffset == -16).AbnormalCost,
            $"{invalid.Label} point abnormal cost");
    }
}

static DdmrpSizingResult ExpectedRollingHistorySizing(HistoryFactSet facts, string sku, int weekOffset)
{
    var weeklyDemand = facts.BufferFacts
        .Where(item =>
            item.Sku == sku &&
            item.WeekOffset <= weekOffset &&
            item.WeekOffset >= weekOffset - 12 &&
            item.EvidenceStatus == "Complete" &&
            item.ActualDemand.HasValue)
        .OrderBy(item => item.WeekOffset)
        .Select(item => item.ActualDemand!.Value)
        .ToList();
    var historicalAdu = decimal.Round(weeklyDemand.Average() / 7m, 2, MidpointRounding.AwayFromZero);
    var parameter = (facts.DdmrpParameterFacts ?? Array.Empty<HistoricalDdmrpParameterFact>()).Single(item =>
        item.Sku == sku &&
        item.EffectiveFromWeekOffset <= weekOffset &&
        weekOffset <= item.EffectiveThroughWeekOffset);
    return DdmrpCalculator.CalculateSizing(parameter.Setting with { Adu = historicalAdu });
}

static void TestCurrentBaselineUiShowsTypedPlanningEvidenceWithoutZeroBackfill()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    var meetingSnapshotStart = page.IndexOf("id=\"baseline-meeting-snapshot-view\"", StringComparison.Ordinal);
    var nextWorkspaceStart = page.IndexOf("id=\"data-readiness-panel\"", meetingSnapshotStart, StringComparison.Ordinal);
    AssertTrue(meetingSnapshotStart >= 0 && nextWorkspaceStart > meetingSnapshotStart,
        "current-baseline meeting snapshot should bound its planning evidence area");

    foreach (var (id, heading) in new[]
    {
        ("baseline-coverage-evidence", "覆盖证据"),
        ("baseline-receipt-evidence", "确认到货"),
        ("baseline-backlog-evidence", "期初积压")
    })
    {
        var sectionStart = page.IndexOf($"id=\"{id}\"", meetingSnapshotStart, StringComparison.Ordinal);
        AssertTrue(sectionStart > meetingSnapshotStart && sectionStart < nextWorkspaceStart,
            $"{id} should be a current-baseline sibling evidence section");
        var sectionEnd = page.IndexOf("</section>", sectionStart, StringComparison.Ordinal);
        AssertTrue(sectionEnd > sectionStart, $"{id} should have a closing section tag");
        var section = page.Substring(sectionStart, sectionEnd - sectionStart);
        AssertTrue(section.Contains($"<h2>{heading}</h2>", StringComparison.Ordinal),
            $"{id} should use the required Chinese heading");
        AssertTrue(heading.Length <= 6, $"{id} heading should be at most six Chinese characters");
    }

    foreach (var receiptColumn in new[] { "SKU", "数量", "预计周", "类型", "来源", "确认", "截止", "证据状态" })
    {
        AssertTrue(page.Contains($"<th>{receiptColumn}</th>", StringComparison.Ordinal),
            $"confirmed receipt evidence should expose {receiptColumn}");
    }
    foreach (var backlogColumn in new[] { "行", "SKU", "数量", "新鲜度", "完整性", "阻断项" })
    {
        AssertTrue(page.Contains($"<th>{backlogColumn}</th>", StringComparison.Ordinal),
            $"opening backlog evidence should expose {backlogColumn}");
    }

    var renderEvidence = SourceFunctionBody(script, "renderBaselinePlanningEvidence");
    AssertTrue(
        renderEvidence.Contains("planningInputs.planningEvidenceCoverage", StringComparison.Ordinal) &&
        renderEvidence.Contains("planningInputs.confirmedReceipts", StringComparison.Ordinal) &&
        renderEvidence.Contains("planningInputs.openingBacklog", StringComparison.Ordinal),
        "baseline evidence renderer should read the three typed backend DTO fields");
    AssertTrue(
        renderEvidence.Contains("candidateId", StringComparison.Ordinal) &&
        renderEvidence.Contains("masterSettingVersion", StringComparison.Ordinal) &&
        renderEvidence.Contains("snapshotNumber", StringComparison.Ordinal) &&
        renderEvidence.Contains("不可变", StringComparison.Ordinal),
        "candidate and frozen baseline contexts should expose candidate/snapshot identity, version and immutability");
    AssertTrue(
        renderEvidence.Contains("后端未提供 SKU", StringComparison.Ordinal) &&
        renderEvidence.Contains("后端未提供数量", StringComparison.Ordinal) &&
        renderEvidence.Contains("后端未提供预计周", StringComparison.Ordinal),
        "missing receipt identity, quantity and week should explain the absent backend evidence");

    var evidenceValue = SourceFunctionBody(script, "baselineEvidenceValue");
    AssertTrue(
        evidenceValue.Contains("value === null", StringComparison.Ordinal) &&
        evidenceValue.Contains("value === undefined", StringComparison.Ordinal) &&
        evidenceValue.Contains("value === \"\"", StringComparison.Ordinal) &&
        evidenceValue.Contains("证据缺失", StringComparison.Ordinal) &&
        evidenceValue.Contains("reason", StringComparison.Ordinal),
        "typed evidence fields should distinguish missing values and show their reason");
    AssertTrue(!evidenceValue.Contains("if (!value)", StringComparison.Ordinal),
        "explicit numeric zero must not be mistaken for missing evidence");

    var evidenceNumber = SourceFunctionBody(script, "baselineEvidenceNumber");
    AssertTrue(evidenceNumber.Contains("numberFormat.format(Number(item))", StringComparison.Ordinal),
        "an explicitly recorded complete zero should render as truthful zero");

    var typedEvidenceFunctions = renderEvidence + evidenceValue + evidenceNumber;
    AssertTrue(
        !typedEvidenceFunctions.Contains("|| 0", StringComparison.Ordinal) &&
        !typedEvidenceFunctions.Contains("?? 0", StringComparison.Ordinal) &&
        !typedEvidenceFunctions.Contains("valueOr(value, 0)", StringComparison.Ordinal),
        "typed baseline evidence rendering must not backfill missing backend values with zero");

    var frozenDetail = SourceFunctionBody(script, "openBaselineSnapshotDetail");
    AssertTrue(
        frozenDetail.Contains("renderBaselinePlanningEvidence(snapshot", StringComparison.Ordinal) &&
        !frozenDetail.Contains("state.currentBaselineCandidate", StringComparison.Ordinal),
        "frozen evidence should render only the selected immutable snapshot DTO");
    AssertTrue(script.Contains("DemoFixture: \"演示数据\"", StringComparison.Ordinal),
        "ordinary demo source codes should be localized without changing stored values");
}

static void TestCurrentBaselineUiExposesHistoryReconciliation()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));

    foreach (var id in new[] { "baseline-history-reconciliation", "baseline-history-reconciliation-body" })
    {
        AssertEqual(1, page.Split($"id=\"{id}\"", StringSplitOptions.None).Length - 1,
            $"current baseline page should expose exactly one {id} host");
    }

    var renderRule = SourceFunctionBody(script, "renderBaselineHistoryReconciliation");
    AssertTrue(
        renderRule.Contains("historyReconciliation", StringComparison.Ordinal) &&
        renderRule.Contains("旧版本未保存历史衔接证据", StringComparison.Ordinal) &&
        renderRule.Contains("differenceReason", StringComparison.Ordinal),
        "history reconciliation renderer should preserve backend lineage and explicit legacy evidence absence");
    AssertTrue(
        !renderRule.Contains("historyClosingBalance +", StringComparison.Ordinal) &&
        !renderRule.Contains("baselineBalance -", StringComparison.Ordinal),
        "history reconciliation UI must display backend balances and differences without recomputing them");

    var blockingRule = SourceFunctionBody(script, "baselineCandidateFreezeBlockingIssues");
    AssertTrue(
        blockingRule.Contains("historyReconciliation", StringComparison.Ordinal) &&
        blockingRule.Contains("Math.abs", StringComparison.Ordinal) &&
        blockingRule.Contains("Date.parse", StringComparison.Ordinal),
        "client freeze mirror should reject missing, unbalanced, and invalid-cutoff reconciliation evidence");
}

static void TestCurrentBaselineExecutableFixturePreservesBlockersAndZeroEvidence()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var fixturePath = Path.Combine(root, "tests", "AdaptiveSopDdsop.Tests", "Js", "baseline-planning-evidence.fixture.mjs");

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
        throw new InvalidOperationException("Node baseline planning evidence fixture timed out after 30 seconds");
    }
    Task.WaitAll(standardOutput, standardError);
    var output = standardOutput.Result;
    var error = standardError.Result;
    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException(
            $"Node baseline planning evidence fixture failed with exit code {process.ExitCode}: {error}{Environment.NewLine}{output}");
    }
    AssertTrue(output.Contains("baseline planning evidence fixture groups passed", StringComparison.Ordinal),
        $"Node baseline planning evidence fixture did not report completion: {output}");
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
            .Load(new ScenarioWorkspaceDataRequest(52, new DateOnly(2026, 6, 30)));
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
            .Load(new ScenarioWorkspaceDataRequest(52, new DateOnly(2026, 6, 30)));
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
            .Load(new ScenarioWorkspaceDataRequest(52, new DateOnly(2026, 6, 30)));
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
        var externalScenario = assumptionSource.GetTemplates().Single().ExternalScenario with
        {
            DemandChanges = (assumptionSource.GetTemplates().Single().ExternalScenario.DemandChanges ?? Array.Empty<ExternalDemandChange>())
                .Append(new ExternalDemandChange(
                    "AV-FPGA-203",
                    null,
                    3,
                    8,
                    100m,
                    "供应响应测试：在受风险窗口触发 FPGA 补货"))
                .ToList(),
            Metadata = new ScenarioAssumptionMetadata(
                "Manual",
                null,
                null,
                "DDS&OP 计划员",
                "2026-06-30T08:00:00.0000000+00:00",
                "2026-06-30",
                "2026-09-30",
                "供应响应测试的明确 FPGA 需求增量",
                "TestFixture")
        };
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
        var inventoryDelta = frozenPosition.OnHand - target.OnHand;
        var unitCost = candidate.Payload.PlanningInputs!.Skus.Single(item => item.Sku == target.Sku).UnitCost;
        var inventoryValueDelta = inventoryDelta * unitCost;
        var reconciliation = candidate.Payload.HistoryReconciliation! with
        {
            Lines = candidate.Payload.HistoryReconciliation!.Lines.Select(line =>
                line.MetricCode == "ON_HAND" && line.ItemKey == target.Sku
                    ? line with
                    {
                        IntervalIncrease = line.IntervalIncrease + inventoryDelta,
                        BaselineBalance = line.BaselineBalance + inventoryDelta
                    }
                    : line.MetricCode == "INVENTORY_VALUE" && line.ItemKey == "ALL"
                        ? line with
                        {
                            IntervalIncrease = line.IntervalIncrease + inventoryValueDelta,
                            BaselineBalance = line.BaselineBalance + inventoryValueDelta
                        }
                        : line).ToList()
        };
        var frozenCandidate = candidate with
        {
            Payload = candidate.Payload with
            {
                Inventory = candidate.Payload.Inventory.Select(item => item.Sku == target.Sku ? frozenPosition : item).ToList(),
                Kpis = candidate.Payload.Kpis! with
                {
                    InventoryValue = candidate.Payload.Kpis!.InventoryValue + inventoryValueDelta
                },
                HistoryReconciliation = reconciliation
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
    AssertEqual(22, CountExactOccurrences(navigation, "class=\"nav-subitem\""), "secondary navigation item count");

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
    AssertEqual(28, CountExactOccurrences(script, "requiredHostId: null"), "all other workspace routes should be host independent");

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

    AssertEqual(29, routes.Length, "canonical workspace route count");
    AssertEqual(29, routes.Select(item => item.TargetId).Distinct(StringComparer.Ordinal).Count(), "canonical workspace target uniqueness");
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
        "history-inventory-position-chart",
        "history-inventory-volatility-chart",
        "history-inventory-evidence-detail",
        "history-time-buffer-options",
        "history-time-buffer-chart",
        "history-time-status-chart",
        "history-time-cost-strip",
        "history-sizing-trace-view",
        "history-sizing-control-point-options",
        "history-sizing-sku-options",
        "history-sizing-snapshot-options",
        "history-ddmrp-input-summary",
        "history-ddmrp-sizing-body",
        "history-ddmrp-zone-chart",
        "history-capacity-resource-options",
        "history-capacity-protection-kpis",
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
    AssertTrue(page.Contains("26 周/52 周历史", StringComparison.Ordinal),
        "history buffer headings should distinguish the trend range from the selected-object detail window");
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
        "selectedHistoryInventoryWeekOffset: null",
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
        "renderHistoryInventoryPositionChart",
        "renderHistoryInventoryVolatilityChart",
        "renderHistoryInventoryEvidenceDetail",
        "historyDemandAxisMaximum",
        "renderHistoryDdmrpSizingTrace",
        "renderHistoryTimeBuffer",
        "renderHistoryTimeStatusChart",
        "renderHistoryTimeCostStrip",
        "renderHistoryCapacityBuffer",
        "resolveHistoryCapacityPair",
        "renderHistoryCapacityProtectionKpis",
        "buildHistoryCapacityFrequency",
        "renderHistoryDdmrpZoneSvg",
        "historyControlPointLabel",
        "historyWeekXScale",
        "contiguousEvidenceSegments",
        "buildMonotonePath",
        "buildMonotoneAreaPath"
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
    AssertTrue(inventoryBody.Contains("historyWeekXScale", StringComparison.Ordinal)
        && inventoryBody.Contains("renderHistoryInventoryPositionChart", StringComparison.Ordinal)
        && inventoryBody.Contains("renderHistoryInventoryVolatilityChart", StringComparison.Ordinal),
        "inventory history should pass one shared weekly x mapping to its position and volatility charts");

    var inventoryPositionBody = SourceFunctionBody(script, "renderHistoryInventoryPositionChart");
    foreach (var backendField in new[] { "point.topOfRed", "point.topOfYellow", "point.topOfGreen", "point.endingOnHand", "point.netFlow", "point.openSupply", "point.qualifiedDemand" })
    {
        AssertTrue(inventoryPositionBody.Contains(backendField, StringComparison.Ordinal), $"inventory position history must read backend {backendField}");
    }
    AssertTrue(inventoryPositionBody.Contains("contiguousEvidenceSegments", StringComparison.Ordinal)
        && inventoryPositionBody.Contains("buildMonotoneAreaPath", StringComparison.Ordinal)
        && inventoryPositionBody.Contains("buildMonotonePath", StringComparison.Ordinal)
        && inventoryPositionBody.Contains("history-evidence-gap", StringComparison.Ordinal),
        "inventory position history should render monotone evidence segments and explicit gaps");
    AssertTrue(inventoryPositionBody.Contains("point.parameterSnapshotId", StringComparison.Ordinal),
        "weekly inventory evidence must identify the effective historical parameter snapshot across V1/V2");
    AssertTrue(!inventoryPositionBody.Contains("targetNetFlowPosition", StringComparison.Ordinal)
        && !inventoryPositionBody.Contains("is-target-nfp", StringComparison.Ordinal),
        "inventory position history must not retain target-NFP scaling or rendering semantics");
    var evidenceDetailBody = SourceFunctionBody(script, "renderHistoryInventoryEvidenceDetail");
    foreach (var backendField in new[] { "point.evidenceChecks", "point.weeklyEvent", "point.parameterSnapshotId", "point.parameterChangeReason" })
    {
        AssertTrue(evidenceDetailBody.Contains(backendField, StringComparison.Ordinal), $"weekly detail should render backend {backendField}");
    }
    AssertTrue(evidenceDetailBody.Contains("escapeHtml", StringComparison.Ordinal), "weekly evidence detail must escape backend text");
    AssertTrue(inventoryPositionBody.Contains("周历史趋势", StringComparison.Ordinal)
        && inventoryPositionBody.Contains("累计提前期详细证据窗口：${number(item.detailWindowWeeks)} 周", StringComparison.Ordinal)
        && !inventoryPositionBody.Contains("周证据", StringComparison.Ordinal),
        "inventory trend and selected-object detail window should use unambiguous labels");

    var inventoryVolatilityBody = SourceFunctionBody(script, "renderHistoryInventoryVolatilityChart");
    foreach (var backendField in new[] { "point.actualDemand", "point.demandSpikeThreshold" })
    {
        AssertTrue(inventoryVolatilityBody.Contains(backendField, StringComparison.Ordinal), $"inventory volatility history must read backend {backendField}");
    }
    AssertTrue(inventoryVolatilityBody.Contains("contiguousEvidenceSegments", StringComparison.Ordinal)
        && inventoryVolatilityBody.Contains("buildMonotoneAreaPath", StringComparison.Ordinal)
        && inventoryVolatilityBody.Contains("buildMonotonePath", StringComparison.Ordinal)
        && inventoryVolatilityBody.Contains("history-evidence-gap", StringComparison.Ordinal),
        "inventory volatility should split backend demand and threshold evidence at explicit gaps");
    AssertTrue(inventoryVolatilityBody.Contains("demandAxisMaximum", StringComparison.Ordinal)
        && inventoryVolatilityBody.Contains("data-history-demand-axis-max", StringComparison.Ordinal),
        "inventory volatility should use and publish the control-point shared demand axis");
    var demandAxisBody = SourceFunctionBody(script, "historyDemandAxisMaximum");
    AssertTrue(demandAxisBody.Contains("history.inventoryBuffers", StringComparison.Ordinal)
        && demandAxisBody.Contains("item.controlPoint === controlPoint", StringComparison.Ordinal),
        "demand axis maximum must aggregate all SKU evidence for the selected control point");

    var sizingBody = SourceFunctionBody(script, "renderHistoryDdmrpSizingTrace");
    AssertTrue(sizingBody.Contains("item.sizingLines", StringComparison.Ordinal),
        "historical DDMRP table should render backend sizing lines");
    AssertTrue(sizingBody.Contains("setting.dltSource", StringComparison.Ordinal)
        && !sizingBody.Contains("decoupledLeadTimeSource", StringComparison.Ordinal),
        "historical DDMRP trace must render the serialized backend DLT source without nonexistent fallbacks");
    AssertTrue(sizingBody.Contains("item.parameterChangeReason", StringComparison.Ordinal),
        "historical DDMRP trace must render the serialized snapshot parameter-change reason");
    AssertTrue(sizingBody.Contains("renderHistoryDdmrpZoneSvg(item)", StringComparison.Ordinal),
        "historical DDMRP trace should render the backend zone result");
    foreach (var forbidden in new[] { "leadTimeFactor *", "variabilityFactor *", "Math.max(item.minimumOrderQuantity", "\"EvidenceMissing\"", "\"Trace\"" })
    {
        AssertTrue(!sizingBody.Contains(forbidden, StringComparison.Ordinal), $"historical DDMRP renderer must not contain {forbidden}");
    }

    AssertTrue(!script.Contains("function renderHistoryStandardDdmrpReference(", StringComparison.Ordinal)
        && !historyReviewBody.Contains("standardDdmrpReference", StringComparison.Ordinal),
        "history renderer should not own the independent standard reference");

    var zoneBody = SourceFunctionBody(script, "renderHistoryDdmrpZoneSvg");
    foreach (var backendField in new[] { "item.sizing.zones.red", "item.sizing.zones.yellow", "item.sizing.zones.green", "item.averageOnHand", "item.effectiveFromWeekOffset", "item.effectiveThroughWeekOffset", "item.asOfUtc", "item.sizing.greenDriver" })
    {
        AssertTrue(zoneBody.Contains(backendField, StringComparison.Ordinal), $"zone SVG must read backend {backendField}");
    }

    var timeBody = SourceFunctionBody(script, "renderHistoryTimeBuffer");
    AssertTrue(timeBody.Contains("renderHistoryTimeStatusChart", StringComparison.Ordinal)
        && timeBody.Contains("renderHistoryTimeCostStrip", StringComparison.Ordinal),
        "time-buffer history should render status and cost evidence in separate hosts");
    var timeStatusBody = SourceFunctionBody(script, "renderHistoryTimeStatusChart");
    foreach (var backendField in new[] { "point.earlyCount", "point.greenCount", "point.yellowCount", "point.redCount", "point.lateCount" })
    {
        AssertTrue(timeStatusBody.Contains(backendField, StringComparison.Ordinal), $"time-buffer status history must read backend {backendField}");
    }
    AssertTrue(!timeStatusBody.Contains("point.abnormalCost", StringComparison.Ordinal)
        && !timeStatusBody.Contains("history-cost-line", StringComparison.Ordinal)
        && !timeStatusBody.Contains("history-cost-marker", StringComparison.Ordinal),
        "time status chart must not contain a cost series, marker or secondary scale");
    var timeCostBody = SourceFunctionBody(script, "renderHistoryTimeCostStrip");
    foreach (var backendField in new[] { "item.abnormalCostEvents", "event.periodStartDate", "event.costAmount", "event.costType", "event.cause", "event.controlPoint", "event.targetType", "event.targetId", "event.sourceAuthority", "event.evidenceStatus" })
    {
        AssertTrue(timeCostBody.Contains(backendField, StringComparison.Ordinal), $"time cost strip must read backend {backendField}");
    }
    AssertTrue(timeCostBody.Contains("本窗口无异常费用事实", StringComparison.Ordinal),
        "time cost strip should distinguish no events from a zero-cost substitute");

    var capacityBody = SourceFunctionBody(script, "renderHistoryCapacityBuffer");
    foreach (var backendField in new[] { "point.committedLoad", "point.theoreticalCapacity", "point.standardCapacity", "point.demonstratedCapacity", "point.plannedAvailableCapacity", "point.measure.utilizationPercent" })
    {
        AssertTrue(capacityBody.Contains(backendField, StringComparison.Ordinal), $"capacity history must read backend {backendField}");
    }
    AssertTrue(capacityBody.Contains("axisMaximum = Math.max(120", StringComparison.Ordinal)
        && capacityBody.Contains("buildHistoryCapacityFrequency", StringComparison.Ordinal)
        && capacityBody.Contains("buildMonotonePath", StringComparison.Ordinal),
        "capacity history should use the prescribed display axis and empirical frequency curve");
    var capacityPairBody = SourceFunctionBody(script, "resolveHistoryCapacityPair");
    AssertTrue(capacityPairBody.Contains("item.relationshipRole === \"UpstreamProtection\"")
        && capacityPairBody.Contains("item.resourceCode === upstream.protectedCcrResourceCode"),
        "capacity pair resolver must select upstream protection and its named CCR reference");
    AssertTrue(SourceFunctionBody(script, "renderHistoryCapacityProtectionPair").Contains("CCR 利用率参照", StringComparison.Ordinal),
        "CCR utilization history should be labelled as a reference");

    foreach (var selector in new[]
    {
        "history-control-point",
        "history-inventory-sku",
        "history-sizing-snapshot",
        "history-time-buffer-id",
        "history-capacity-resource",
        "history-inventory-week"
    })
    {
        AssertTrue(script.Contains(selector, StringComparison.Ordinal), $"history UI should expose {selector} selectors");
    }
    foreach (var selectionBehavior in new[]
    {
        "state.selectedHistoryControlPoint = controlPoint.dataset.historyControlPoint",
        "state.selectedHistoryInventorySku = inventorySku.dataset.historyInventorySku",
        "state.selectedHistoryInventoryWeekOffset = Number(inventoryWeek.dataset.historyInventoryWeek)",
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
        && fixture.Contains("renderHistoryReview(__historyFixture)", StringComparison.Ordinal)
        && fixture.Contains("createStandaloneHistoryReview", StringComparison.Ordinal),
        "runtime fixture should compile and execute the real app.js renderers against a backend-shaped history DTO");

    var seed = SeedData.Create();
    var historyService = new HistoryReviewWorkspaceService(
        new SeedHistoryOperatingFactSource(seed),
        new SeedScenarioWorkspaceDataSource(seed));
    var history = historyService.GetReview(6);
    var annualHistory = historyService.GetReview(12);
    RunHistoryBufferRendererFixture(root, history, annualHistory);
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

static void TestFutureInventoryFlowChartsSeparatePhysicalEvidence()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));
    var styles = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "css", "site.css"));

    foreach (var id in new[]
    {
        "buffer-trend-chart",
        "inventory-flow-evidence",
        "inventory-flow-chart",
        "buffer-volatility-chart",
    })
    {
        AssertEqual(1, page.Split($"id=\"{id}\"", StringSplitOptions.None).Length - 1,
            $"future inventory evidence host {id}");
    }

    AssertTrue(page.Contains(">库存位置<", StringComparison.Ordinal)
        && page.Contains(">净流量与在手库存<", StringComparison.Ordinal),
        "the upper panel should identify both planning and physical positions");
    AssertTrue(page.Contains(">物理库存<", StringComparison.Ordinal),
        "the physical projection should have a concise Chinese heading");
    AssertTrue(page.Contains(">需求波动<", StringComparison.Ordinal),
        "volatility should remain a separate panel");
    AssertTrue(page.Contains("id=\"buffer-case-select\"", StringComparison.Ordinal)
        && page.Contains("id=\"buffer-week-range-select\"", StringComparison.Ordinal),
        "one compact selection strip should expose case and week range controls");

    var workspaceBody = SourceFunctionBody(script, "renderBufferTrendWorkspace");
    AssertTrue(workspaceBody.Contains("renderBufferTrendChart", StringComparison.Ordinal)
        && workspaceBody.Contains("renderInventoryFlowChart", StringComparison.Ordinal)
        && workspaceBody.Contains("renderBufferVolatilityChart", StringComparison.Ordinal),
        "one workspace render should synchronize NFP physical stock and volatility");

    var physicalBody = SourceFunctionBody(script, "renderInventoryFlowChart");
    foreach (var backendField in new[]
    {
        "endingOnHand",
        "frozenReceiptQuantity",
        "simulatedReceiptQuantity",
        "prebuildReceiptQuantity",
        "endingBacklog",
    })
    {
        AssertTrue(physicalBody.Contains(backendField, StringComparison.Ordinal),
            $"physical renderer should map backend field {backendField}");
    }
    foreach (var forbidden in new[]
    {
        "openingOnHand +",
        "openingOnHand -",
        "endingOnHand =",
        "demand -",
        "totalFulfilledDemand",
    })
    {
        AssertTrue(!physicalBody.Contains(forbidden, StringComparison.Ordinal),
            $"JavaScript must not reconstruct physical conservation with {forbidden}");
    }

    AssertTrue(styles.Contains(".inventory-flow-chart", StringComparison.Ordinal)
        && styles.Contains(".physical-on-hand-line", StringComparison.Ordinal)
        && styles.Contains(".physical-frozen-receipt", StringComparison.Ordinal)
        && styles.Contains(".physical-simulated-receipt", StringComparison.Ordinal)
        && styles.Contains(".physical-prebuild-receipt", StringComparison.Ordinal)
        && styles.Contains(".physical-ending-backlog", StringComparison.Ordinal),
        "physical evidence should have fixed field-to-color styles");

    var nfpBody = SourceFunctionBody(script, "renderBufferTrendChart");
    AssertTrue(nfpBody.Contains("data-field=\"endNetFlowBeforeReplenishment\"", StringComparison.Ordinal)
        && nfpBody.Contains("data-field=\"endNetFlowAfterReplenishment\"", StringComparison.Ordinal)
        && nfpBody.Contains("data-field=\"physicalPosition.endingOnHand\"", StringComparison.Ordinal)
        && !nfpBody.Contains("targetInventory", StringComparison.Ordinal),
        "upper chart should map pre/post NFP and optional physical on-hand without target inventory");
    AssertTrue(styles.Contains(".buffer-net-flow-line { stroke: #111827;", StringComparison.Ordinal),
        "pre-replenishment NFP should use the fixed black line");
    AssertTrue(styles.Contains(".buffer-preview-line { stroke: #4f8bd6;", StringComparison.Ordinal)
        && styles.Contains(".buffer-inventory-line { stroke: #4f8bd6;", StringComparison.Ordinal),
        "selected post-replenishment NFP should use the fixed blue line");
    AssertTrue(styles.Contains(".buffer-baseline-line { stroke: #8a939f;", StringComparison.Ordinal),
        "baseline post-replenishment comparison should use the fixed gray line");
    AssertTrue(styles.Contains(".buffer-on-hand-line { stroke: #286fa8;", StringComparison.Ordinal),
        "upper physical on-hand position should use its own visible line style");
    foreach (var label in new[] { "补货前净流量位置（下单判断）", "补货后净流量位置（已释放供应）", "期末在手库存（执行风险）" })
    {
        AssertTrue(page.Contains(label, StringComparison.Ordinal), $"future inventory terminology should include {label}");
    }
    AssertTrue(script.Contains("function whiteBoxTraceRecords(previewCase)", StringComparison.Ordinal)
        && script.Contains("function focusWhiteBoxTraceRecord(recordKey)", StringComparison.Ordinal)
        && script.Contains("event.target.closest(\"[data-white-box-record]\")", StringComparison.Ordinal),
        "white-box links should resolve and focus actual plan trace records through the registered click handler");
    AssertTrue(!script.Contains("`${whiteBoxCaseId}:${detail.sku}`", StringComparison.Ordinal),
        "future inventory detail must not invent case:SKU white-box identifiers");

    var preview = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()))
        .Preview(new ScenarioRunPreviewRequest(12));
    RunFutureInventoryFlowChartFixture(root, preview);
}

static void RunHistoryBufferRendererFixture(
    string root,
    HistoryReviewWorkspace history,
    HistoryReviewWorkspace annualHistory)
{
    var fixturePath = Path.Combine(root, "tests", "AdaptiveSopDdsop.Tests", "Js", "history-buffer-renderers.fixture.mjs");
    var dtoPath = Path.Combine(Path.GetTempPath(), $"history-review-{Guid.NewGuid():N}.json");
    var annualDtoPath = Path.Combine(Path.GetTempPath(), $"history-review-annual-{Guid.NewGuid():N}.json");
    File.WriteAllText(
        dtoPath,
        JsonSerializer.Serialize(history, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    File.WriteAllText(
        annualDtoPath,
        JsonSerializer.Serialize(annualHistory, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

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
        process.StartInfo.ArgumentList.Add(annualDtoPath);
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
        AssertTrue(output.Contains("history renderer no longer owns standard reference", StringComparison.Ordinal),
            $"Node renderer fixture did not prove the standard reference moved out of history: {output}");
    }
    finally
    {
        File.Delete(dtoPath);
        File.Delete(annualDtoPath);
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

static void RunFutureInventoryFlowChartFixture(string root, ScenarioRunPreviewResult preview)
{
    var fixturePath = Path.Combine(root, "tests", "AdaptiveSopDdsop.Tests", "Js", "future-inventory-flow-charts.fixture.mjs");
    var dtoPath = Path.Combine(Path.GetTempPath(), $"future-inventory-flow-{Guid.NewGuid():N}.json");
    File.WriteAllText(
        dtoPath,
        JsonSerializer.Serialize(preview, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

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
            throw new InvalidOperationException("Node future inventory-flow chart fixture timed out after 30 seconds");
        }
        Task.WaitAll(standardOutput, standardError);
        var output = standardOutput.Result;
        var error = standardError.Result;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Node future inventory-flow chart fixture failed with exit code {process.ExitCode}: {error}{Environment.NewLine}{output}");
        }
        AssertTrue(output.Contains("future inventory flow chart fixture groups passed", StringComparison.Ordinal),
            $"Node future inventory-flow chart fixture did not report completion: {output}");
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

static void TestDdmrpStandardReferenceFixtureRunsInStandardHarness()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var fixturePath = Path.Combine(root, "tests", "AdaptiveSopDdsop.Tests", "Js", "ddmrp-standard-reference.fixture.mjs");

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
        throw new InvalidOperationException("Node DDMRP standard reference fixture timed out after 30 seconds");
    }
    Task.WaitAll(standardOutput, standardError);
    var output = standardOutput.Result;
    var error = standardError.Result;
    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException(
            $"Node DDMRP standard reference fixture failed with exit code {process.ExitCode}: {error}{Environment.NewLine}{output}");
    }
    AssertTrue(output.Contains("6/6 DDMRP standard reference fixture groups passed", StringComparison.Ordinal),
        $"Node DDMRP standard reference fixture did not report completion: {output}");
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
        "time-buffer-breach-detail",
        "time-buffer-breach-select",
        "time-buffer-breach-evidence-chip",
        "time-buffer-breach-summary",
        "time-buffer-breach-weekly-grid",
        "trace-panel"
    };

    foreach (var id in requiredIds)
    {
    AssertTrue(page.Contains($"id=\"{id}\"", StringComparison.Ordinal), $"page should expose {id}");
    }

    AssertTrue(page.Contains("缓冲 / 库存趋势", StringComparison.Ordinal), "page should expose graphical buffer trend label");
    AssertTrue(page.Contains("库存选项", StringComparison.Ordinal), "page should expose left-side inventory options");
    AssertTrue(page.Contains("动态红黄绿缓冲带", StringComparison.Ordinal), "page should expose dynamic mountain-style buffer bands");
    AssertTrue(page.Contains("补货前净流量位置（下单判断）", StringComparison.Ordinal), "page should expose pre-replenishment net flow position label");
    AssertTrue(page.Contains("期末在手库存（执行风险）", StringComparison.Ordinal), "page should expose physical on-hand risk label");
    AssertTrue(!page.Contains("目标库存", StringComparison.Ordinal), "page should not expose a target inventory label");
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
    AssertTrue(result.Summaries.Any(item => item.AverageInventoryValue is > 0m), "family summaries should expose inventory value");
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
    AssertTrue(result.Scenario.ProductFamilyDashboard.Comparison.SupplyGapDelta != 0m ||
        result.Scenario.ProductFamilyDashboard.Comparison.AverageInventoryValueDelta is not null and not 0m,
        "scenario family dashboard should include comparison deltas");
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

static void TestSeedPlanningEvidenceHasFiftyTwoWeeks()
{
    var seed = SeedData.Create();
    var expectedWeeks = Enumerable.Range(1, 52).ToArray();

    foreach (var sku in seed.Skus)
    {
        var annualDemand = seed.Demand
            .Where(item => item.Sku == sku.Sku)
            .OrderBy(item => item.Week)
            .ToList();
        var values = annualDemand.Select(item => item.BaselineDemand).ToList();

        AssertEqual(52, annualDemand.Count, $"annual demand row count for {sku.Sku}");
        AssertTrue(annualDemand.Select(item => item.Week).SequenceEqual(expectedWeeks),
            $"annual demand must explicitly cover weeks 1..52 for {sku.Sku}");
        AssertTrue(values.Any(item => item == 0m), $"annual demand must retain explicit zero weeks for {sku.Sku}");
        AssertTrue(!RepeatsEvery(values, 4), $"annual demand must not repeat a four-week template for {sku.Sku}");
        AssertTrue(!RepeatsEvery(values, 8), $"annual demand must not repeat an eight-week template for {sku.Sku}");
    }

    var anchor = new DateOnly(2026, 6, 30);
    var shortHorizon = new SeedScenarioWorkspaceDataSource(seed)
        .Load(new ScenarioWorkspaceDataRequest(8, anchor));

    AssertEqual(seed.Skus.Count * 8, shortHorizon.Demand.Count, "short-horizon active demand row count");
    AssertTrue(shortHorizon.Demand.All(item => item.Week is >= 1 and <= 8),
        "short-horizon load should crop active demand to the requested weeks");
    AssertTrue(shortHorizon.DdmrpParameters.All(item => item.EffectiveThroughWeek >= 52),
        "governed DDMRP parameters should cover the full annual fixture");
    AssertEqual(anchor, shortHorizon.PlanningEvidenceCoverage!.AnchorDate, "planning evidence anchor");
    AssertEqual(1, shortHorizon.PlanningEvidenceCoverage.CoverageFromWeek, "planning evidence first week");
    AssertEqual(52, shortHorizon.PlanningEvidenceCoverage.CoverageThroughWeek, "planning evidence last week");
    AssertEqual("Complete", shortHorizon.PlanningEvidenceCoverage.EvidenceStatus, "planning evidence coverage status");
    AssertTrue(shortHorizon.ConfirmedReceipts is { Count: > 0 },
        "short-horizon loads should retain the complete frozen receipt fact set");
    AssertEqual(seed.Skus.Count, shortHorizon.OpeningBacklog!.Count,
        "short-horizon loads should retain one opening-backlog row per governed SKU");

    foreach (var window in shortHorizon.SupplierCapacityWindows
                 .GroupBy(item => new { item.Supplier, item.MaterialFamily }))
    {
        var rows = window.OrderBy(item => item.Week).ToList();
        AssertEqual(52, rows.Count, $"annual supplier commitment row count for {window.Key.Supplier}/{window.Key.MaterialFamily}");
        AssertTrue(rows.Select(item => item.Week).SequenceEqual(expectedWeeks),
            $"supplier commitments must uniquely cover weeks 1..52 for {window.Key.Supplier}/{window.Key.MaterialFamily}");
        AssertTrue(rows.All(item => item.CommittedCapacity >= 0m),
            $"supplier commitments must be nonnegative for {window.Key.Supplier}/{window.Key.MaterialFamily}");
    }

    static bool RepeatsEvery(IReadOnlyList<decimal> values, int period) =>
        values.Count > period && values.Skip(period).Select((value, index) => value == values[index % period]).All(item => item);
}

static void TestAvCom201SizingIsCalibrated()
{
    var sku = SeedData.Create().Skus.Single(item => item.Sku == "AV-COM-201");
    var zones = DdmrpCalculator.CalculateZones(sku);

    AssertEqual(12m, sku.MinimumOrderQuantity, "AV-COM-201 MOQ");
    AssertEqual(8m, zones.Red, "AV-COM-201 standard red zone");
    AssertEqual(9m, zones.Yellow, "AV-COM-201 standard yellow zone");
    AssertEqual(13m, zones.Green, "AV-COM-201 standard green zone");
}

static void TestBaselineDerivesTypedPlanningEvidence()
{
    var seed = SeedData.Create();
    var source = new SeedCurrentBaselineDataSource(seed);
    var candidate = source.GetCandidate();
    var planning = candidate.Payload.PlanningInputs;

    AssertTrue(planning is not null, "baseline candidate should expose typed planning inputs");
    AssertTrue(planning!.ConfirmedReceipts is { Count: > 0 }, "baseline candidate should expose confirmed receipts");
    AssertEqual(seed.Skus.Count, planning.OpeningBacklog!.Count, "baseline opening-backlog row count");
    AssertTrue(planning.PlanningEvidenceCoverage is not null, "baseline candidate should expose planning evidence coverage");
    AssertEqual(
        PlanningEvidenceValidator.BusinessDateForSourceTimestamp(candidate.AsOfUtc),
        planning.Request.AnchorDate,
        "baseline anchor should be the Asia/Shanghai business date of the source cutoff");
    AssertEqual(planning.Request.AnchorDate, planning.PlanningEvidenceCoverage!.AnchorDate, "coverage and request anchor");

    var expectedNonzeroBacklog = new Dictionary<string, decimal>(StringComparer.Ordinal)
    {
        ["PAY-SAR-102"] = 1m,
        ["AV-FPGA-203"] = 2m,
        ["CBL-HAR-402"] = 8m
    };
    var actualNonzeroBacklog = planning.OpeningBacklog
        .Where(item => item.Quantity != 0m)
        .ToDictionary(item => item.Sku, item => item.Quantity, StringComparer.Ordinal);
    AssertTrue(actualNonzeroBacklog.Count == expectedNonzeroBacklog.Count &&
               expectedNonzeroBacklog.All(item => actualNonzeroBacklog.TryGetValue(item.Key, out var quantity) && quantity == item.Value),
        "opening backlog should preserve only the three explicit nonzero demo facts");

    var skuByCode = planning.Skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);
    var sourceBySku = planning.SupplierItemSources.ToDictionary(item => item.Sku, StringComparer.Ordinal);
    foreach (var receipt in planning.ConfirmedReceipts!)
    {
        var sku = skuByCode[receipt.Sku];
        var expectedWeek = Math.Max(1, (int)Math.Ceiling(sku.DecoupledLeadTimeDays / 7m));
        var expectedDate = planning.Request.AnchorDate.AddDays(7 * (expectedWeek - 1) + 2);
        var supplierSource = sourceBySku[receipt.Sku];

        AssertEqual(expectedWeek, receipt.ExpectedReceiptWeek, $"authoritative receipt week for {receipt.Sku}");
        AssertEqual(expectedDate, receipt.ExpectedReceiptDate, $"receipt date inside authoritative bucket for {receipt.Sku}");
        AssertEqual(expectedWeek <= 2 ? "ConfirmedInTransit" : "ConfirmedOpenSupply", receipt.ReceiptType,
            $"receipt type for {receipt.Sku}");
        AssertEqual("Confirmed", receipt.ConfirmationStatus, $"receipt confirmation status for {receipt.Sku}");
        AssertEqual("Complete", receipt.EvidenceStatus, $"receipt evidence status for {receipt.Sku}");
        AssertEqual("DemoFixture", receipt.EvidenceLabel, $"receipt evidence label for {receipt.Sku}");
        AssertEqual(supplierSource.Supplier, receipt.Supplier, $"receipt supplier mapping for {receipt.Sku}");
        AssertEqual(supplierSource.MaterialFamily, receipt.MaterialFamily, $"receipt material mapping for {receipt.Sku}");
    }
    AssertEqual(planning.ConfirmedReceipts.Count,
        planning.ConfirmedReceipts.Select(item => item.ReceiptId).Distinct(StringComparer.Ordinal).Count(),
        "receipt IDs should be unique");
    AssertEqual(planning.ConfirmedReceipts.Count,
        planning.ConfirmedReceipts.Select(item => item.SourceReference).Distinct(StringComparer.Ordinal).Count(),
        "receipt source references should be unique");
    foreach (var inventory in planning.Inventory)
    {
        AssertEqual(inventory.OpenSupply,
            planning.ConfirmedReceipts.Where(item => item.Sku == inventory.Sku).Sum(item => item.Quantity),
            $"confirmed receipts reconcile to open supply for {inventory.Sku}");
    }

    var fpgaReceipt = planning.ConfirmedReceipts.Single(item => item.Sku == "AV-FPGA-203");
    var fpgaBacklog = planning.OpeningBacklog.Single(item => item.Sku == "AV-FPGA-203");
    var alteredPlanning = planning with
    {
        ConfirmedReceipts = planning.ConfirmedReceipts
            .Select(item => item.Sku == fpgaReceipt.Sku ? item with { Quantity = item.Quantity + 7m } : item)
            .ToList(),
        OpeningBacklog = planning.OpeningBacklog
            .Select(item => item.Sku == fpgaBacklog.Sku ? item with { Quantity = item.Quantity + 5m } : item)
            .ToList()
    };
    var derivedCandidate = new SeedCurrentBaselineDataSource(seed, new StaticScenarioWorkspaceDataSource(alteredPlanning)).GetCandidate();
    AssertEqual(fpgaReceipt.Quantity + 7m,
        derivedCandidate.Payload.InTransit.Single(item => item.Sku == fpgaReceipt.Sku).Quantity,
        "transit summary should be derived from confirmed receipt evidence");
    AssertEqual(fpgaBacklog.Quantity + 5m,
        derivedCandidate.Payload.Backlog.Single(item => item.Sku == fpgaBacklog.Sku).Quantity,
        "backlog summary should be derived from opening-backlog evidence");
    AssertTrue(derivedCandidate.Payload.Backlog.Single(item => item.Sku == fpgaBacklog.Sku).Quantity !=
               seed.Inventory.Single(item => item.Sku == fpgaBacklog.Sku).QualifiedDemand,
        "backlog summary should not fall back to InventoryPosition.QualifiedDemand");

    foreach (var sectionCode in new[] { "CONFIRMED_RECEIPTS", "OPENING_BACKLOG", "PLANNING_EVIDENCE_COVERAGE" })
    {
        var section = candidate.Sections.Single(item => item.SectionCode == sectionCode);
        AssertTrue(!string.IsNullOrWhiteSpace(section.SourceAuthority), $"{sectionCode} source authority");
        AssertEqual(candidate.AsOfUtc, section.AsOfUtc, $"{sectionCode} cutoff");
        AssertEqual("Fresh", section.FreshnessStatus, $"{sectionCode} freshness");
        AssertEqual("Complete", section.CompletenessStatus, $"{sectionCode} completeness");
        AssertEqual("DemoFixture", section.EvidenceLabel, $"{sectionCode} evidence label");
    }
}

static void TestBaselineFreezeRejectsIncompletePlanningEvidence()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-incomplete-planning-evidence-{Guid.NewGuid():N}.db");
    try
    {
        var candidate = new SeedCurrentBaselineDataSource(SeedData.Create()).GetCandidate();
        var completePlanning = BuildCompletePlanningEvidenceData();
        var incompletePlanning = completePlanning with
        {
            PlanningEvidenceCoverage = completePlanning.PlanningEvidenceCoverage! with { CoverageThroughWeek = 12 }
        };
        var incompleteCandidate = candidate with
        {
            Payload = candidate.Payload with { PlanningInputs = incompletePlanning }
        };
        var service = new CurrentBaselineService(new FixedCurrentBaselineDataSource(incompleteCandidate), databasePath);

        var rejected = false;
        try
        {
            service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", "incomplete planning evidence"));
        }
        catch (ArgumentException ex)
        {
            rejected = ex.Message.Contains("IncompleteCoverage", StringComparison.Ordinal);
        }

        AssertTrue(rejected, "planning evidence validation should reject incomplete annual coverage");
        AssertEqual(0, service.List(10).Count, "rejected planning evidence must not create a snapshot");
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM current_baseline_audit_events;";
        AssertEqual(0L, Convert.ToInt64(command.ExecuteScalar()), "rejected planning evidence must not create a failure audit row");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestFrozenPlanningEvidenceRoundTrips()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-planning-evidence-round-trip-{Guid.NewGuid():N}.db");
    try
    {
        var service = new CurrentBaselineService(new SeedCurrentBaselineDataSource(SeedData.Create()), databasePath);
        var candidate = service.GetCandidate();
        var candidatePlanning = candidate.Payload.PlanningInputs!;

        var frozen = service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", "freeze typed annual evidence"));
        var next = service.Freeze(new CurrentBaselineFreezeRequest("DDS&OP planner", "freeze next typed annual evidence version"));
        var loaded = service.GetDetail(frozen.SnapshotId)!;
        var loadedPlanning = loaded.Payload.PlanningInputs;

        AssertTrue(loadedPlanning is not null, "frozen snapshot should retain typed planning inputs");
        AssertEqual(JsonSerializer.Serialize(candidatePlanning.ConfirmedReceipts),
            JsonSerializer.Serialize(loadedPlanning!.ConfirmedReceipts),
            "confirmed receipt evidence JSON round trip");
        AssertEqual(JsonSerializer.Serialize(candidatePlanning.OpeningBacklog),
            JsonSerializer.Serialize(loadedPlanning.OpeningBacklog),
            "opening backlog evidence JSON round trip");
        AssertEqual(JsonSerializer.Serialize(candidatePlanning.PlanningEvidenceCoverage),
            JsonSerializer.Serialize(loadedPlanning.PlanningEvidenceCoverage),
            "planning evidence coverage JSON round trip");
        AssertTrue(loadedPlanning.Skus.All(sku =>
                loadedPlanning.Demand.Count(item => item.Sku == sku.Sku) == 52),
            "frozen candidate should retain all 52 demand weeks per governed SKU");
        AssertTrue(next.SnapshotNumber.EndsWith("-002", StringComparison.Ordinal),
            "successful planning evidence freezes should increment the immutable baseline version");
        AssertEqual(1, service.GetAuditEvents(frozen.SnapshotId).Count(item => item.EventType == "BaselineFrozen"),
            "successful planning evidence freeze should use the existing audit chain");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestPlanningEvidenceAcceptsCompleteCoverage()
{
    var data = BuildCompletePlanningEvidenceData();

    var freeze = PlanningEvidenceValidator.ValidateForFreeze(data);
    var projection = PlanningEvidenceValidator.ValidateForProjection(data, 12);

    AssertEqual("Complete", freeze.Status, "complete freeze evidence status");
    AssertTrue(!freeze.Issues.Any(item => item.BlocksFreeze), "complete freeze evidence should not have blockers");
    AssertEqual("Complete", projection.Status, "complete projection evidence status");
    AssertTrue(!projection.Issues.Any(item => item.BlocksProjection), "complete projection evidence should not have blockers");
    AssertEqual(52, data.Demand.Count, "explicit demand week count");
}

static void TestPlanningEvidenceRejectsDemandGaps()
{
    var data = BuildCompletePlanningEvidenceData();
    var missingDemand = data with
    {
        Demand = data.Demand.Where(item => item.Week != 17).ToList()
    };

    var result = PlanningEvidenceValidator.ValidateForFreeze(missingDemand);

    AssertEqual("Incomplete", result.Status, "demand gap validation status");
    AssertTrue(result.Issues.Any(item =>
        item.Scope == "Demand" &&
        item.Sku == data.Skus.Single().Sku &&
        item.Week == 17 &&
        item.Reason == "MissingDemand" &&
        item.BlocksFreeze &&
        item.BlocksProjection), "missing demand week should be explicit and blocking");

    var missingOpeningRows = data with
    {
        Inventory = Array.Empty<InventoryPosition>(),
        OpeningBacklog = Array.Empty<OpeningBacklogEvidence>()
    };
    var missingResult = PlanningEvidenceValidator.ValidateForFreeze(missingOpeningRows);
    AssertTrue(missingResult.Issues.Any(item => item.Scope == "Inventory" && item.Reason == "MissingInventory"),
        "missing inventory should be reported instead of treated as zero");
    AssertTrue(missingResult.Issues.Any(item => item.Scope == "OpeningBacklog" && item.Reason == "MissingOpeningBacklog"),
        "missing opening backlog should be reported instead of treated as zero");
}

static void TestPlanningEvidenceRejectsInvalidRows()
{
    var data = BuildCompletePlanningEvidenceData();
    var duplicateDemand = data.Demand.Single(item => item.Week == 4);
    var firstReceipt = data.ConfirmedReceipts![0];
    var invalid = data with
    {
        Inventory = data.Inventory.Concat(data.Inventory).ToList(),
        Demand = data.Demand
            .Select(item => item.Week == 6 ? item with { BaselineDemand = -1m } : item)
            .Append(duplicateDemand)
            .ToList(),
        OpeningBacklog = new[]
        {
            data.OpeningBacklog!.Single() with { EvidenceStatus = "Expired" },
            data.OpeningBacklog!.Single() with { BacklogId = "BACKLOG-DUPLICATE" }
        },
        ConfirmedReceipts = data.ConfirmedReceipts!
            .Select((item, index) => index == 0 ? item with { EvidenceStatus = "Stale" } : item)
            .Append(firstReceipt with { Quantity = -1m })
            .ToList()
    };

    var result = PlanningEvidenceValidator.ValidateForFreeze(invalid);
    var reasons = result.Issues.Select(item => item.Reason).ToHashSet(StringComparer.Ordinal);

    AssertEqual("Incomplete", result.Status, "invalid row validation status");
    AssertTrue(reasons.Contains("DuplicateInventory"), "duplicate inventory should be rejected");
    AssertTrue(reasons.Contains("DuplicateDemand"), "duplicate demand should be rejected");
    AssertTrue(reasons.Contains("NegativeDemand"), "negative demand should be rejected");
    AssertTrue(reasons.Contains("DuplicateOpeningBacklog"), "duplicate opening backlog should be rejected");
    AssertTrue(reasons.Contains("OpeningBacklogEvidenceNotComplete"), "expired backlog evidence should be rejected");
    AssertTrue(reasons.Contains("DuplicateReceiptId"), "duplicate receipt ids should be rejected");
    AssertTrue(reasons.Contains("NegativeReceiptQuantity"), "negative receipt quantities should be rejected");
    AssertTrue(reasons.Contains("ReceiptEvidenceNotComplete"), "stale receipt evidence should be rejected");
}

static void TestPlanningEvidenceRejectsReceiptDateMismatch()
{
    var data = BuildCompletePlanningEvidenceData();
    var receipts = data.ConfirmedReceipts!.ToList();
    receipts[0] = receipts[0] with { ExpectedReceiptDate = data.Request.AnchorDate.AddDays(14) };

    var result = PlanningEvidenceValidator.ValidateForFreeze(data with { ConfirmedReceipts = receipts });

    AssertTrue(result.Issues.Any(item =>
        item.Scope == "ConfirmedReceipt" &&
        item.SourceId == receipts[0].ReceiptId &&
        item.Reason == "ReceiptDateWeekMismatch" &&
        item.BlocksFreeze), "receipt date and authoritative week mismatch should block freeze");
}

static void TestPlanningEvidenceUsesLeftClosedRightOpenBuckets()
{
    var anchor = new DateOnly(2026, 6, 1);

    AssertEqual(0, PlanningEvidenceValidator.WeekForDate(anchor, anchor.AddDays(-1)), "date before anchor week");
    AssertEqual(1, PlanningEvidenceValidator.WeekForDate(anchor, anchor), "left boundary week");
    AssertEqual(1, PlanningEvidenceValidator.WeekForDate(anchor, anchor.AddDays(6)), "last day before right boundary");
    AssertEqual(2, PlanningEvidenceValidator.WeekForDate(anchor, anchor.AddDays(7)), "right boundary enters next week");

    var data = BuildCompletePlanningEvidenceData();
    var receipts = data.ConfirmedReceipts!.ToList();
    receipts[0] = receipts[0] with
    {
        ExpectedReceiptWeek = 1,
        ExpectedReceiptDate = anchor.AddDays(6),
        SourceTimestampUtc = "2026-06-06T16:30:00Z"
    };
    var result = PlanningEvidenceValidator.ValidateForFreeze(data with { ConfirmedReceipts = receipts });
    AssertTrue(!result.Issues.Any(item => item.SourceId == receipts[0].ReceiptId && item.Reason == "ReceiptDateWeekMismatch"),
        "a date before the right boundary should remain in the prior week");
}

static void TestPlanningEvidenceConvertsSourceTimestamps()
{
    var timestamp = "2026-06-07T16:30:00Z";
    var businessDate = PlanningEvidenceValidator.BusinessDateForSourceTimestamp(timestamp);

    AssertEqual(new DateOnly(2026, 6, 8), businessDate, "Asia/Shanghai business date");

    var data = BuildCompletePlanningEvidenceData();
    AssertEqual(timestamp, data.ConfirmedReceipts![0].SourceTimestampUtc!, "original source timestamp should be retained");

    var receipts = data.ConfirmedReceipts!.ToList();
    receipts[0] = receipts[0] with { SourceTimestampUtc = "not-a-timestamp" };
    var invalid = PlanningEvidenceValidator.ValidateForFreeze(data with { ConfirmedReceipts = receipts });
    AssertTrue(invalid.Issues.Any(item => item.SourceId == receipts[0].ReceiptId && item.Reason == "InvalidSourceTimestampUtc"),
        "invalid source timestamps should form a blocking issue instead of being interpreted in local time");
}

static void TestPlanningEvidencePreservesOutsideCoverageReceipts()
{
    var data = BuildCompletePlanningEvidenceData();
    var outside = data.ConfirmedReceipts!.Single(item => item.ExpectedReceiptWeek == 53);

    var result = PlanningEvidenceValidator.ValidateForFreeze(data);
    var note = result.Issues.Single(item => item.SourceId == outside.ReceiptId && item.Reason == "OutsideCoverage");

    AssertEqual("Complete", result.Status, "outside-coverage note should not make complete evidence incomplete");
    AssertTrue(!note.BlocksFreeze && !note.BlocksProjection, "outside-coverage note should be non-blocking");
    AssertEqual(53, data.ConfirmedReceipts!.Single(item => item.ReceiptId == outside.ReceiptId).ExpectedReceiptWeek,
        "validation should preserve the authoritative outside-coverage week");

    var inventory = data.Inventory
        .Select(item => item with { OpenSupply = item.OpenSupply - outside.Quantity })
        .ToList();
    var mismatch = PlanningEvidenceValidator.ValidateForFreeze(data with { Inventory = inventory });
    AssertTrue(mismatch.Issues.Any(item => item.Reason == "OpenSupplyMismatch"),
        "outside-coverage receipts should remain in open-supply reconciliation");
}

static void TestFrozenEvidenceRejectsGeneratedReceipts()
{
    var data = BuildCompletePlanningEvidenceData();
    var receipts = data.ConfirmedReceipts!.ToList();
    receipts[0] = receipts[0] with { ReceiptType = "Generated" };

    var result = PlanningEvidenceValidator.ValidateForFreeze(data with { ConfirmedReceipts = receipts });

    AssertTrue(result.Issues.Any(item =>
        item.Scope == "ConfirmedReceipt" &&
        item.SourceId == receipts[0].ReceiptId &&
        item.Reason == "UnsupportedReceiptType" &&
        item.BlocksFreeze), "generated receipts must not enter frozen planning evidence");
}

static void TestPlanningEvidenceRequiresSupportingMappings()
{
    var data = BuildCompletePlanningEvidenceData();
    var invalid = data with
    {
        Inventory = data.Inventory.Select(item => item with { OpenSupply = item.OpenSupply + 1m }).ToList(),
        SupplierItemSources = Array.Empty<SupplierItemSource>(),
        SupplierCapacityWindows = data.SupplierCapacityWindows.Where(item => item.Week != 2).ToList(),
        DdmrpParameters = data.DdmrpParameters
            .Select(item => item with { CompletenessStatus = "Incomplete", EvidenceStatus = "Expired" })
            .ToList()
    };

    var result = PlanningEvidenceValidator.ValidateForFreeze(invalid);
    var reasons = result.Issues.Select(item => item.Reason).ToHashSet(StringComparer.Ordinal);

    AssertTrue(reasons.Contains("MissingSupplierItemSource"), "receipt should require an explicit SKU supplier mapping");
    AssertTrue(reasons.Contains("MissingSupplierCapacityWindow"), "in-coverage receipt should require a weekly capacity mapping");
    AssertTrue(reasons.Contains("OpenSupplyMismatch"), "confirmed receipt total should match open supply within tolerance");
    AssertTrue(reasons.Contains("IncompleteDdmrpParameters"), "incomplete DDMRP parameters should block validation");
}

static void TestPlanningEvidencePreservesLegacyJson()
{
    var data = BuildCompletePlanningEvidenceData() with
    {
        ConfirmedReceipts = null,
        OpeningBacklog = null,
        PlanningEvidenceCoverage = null
    };
    var legacyJson = JsonNode.Parse(JsonSerializer.Serialize(data))!.AsObject();
    legacyJson.Remove(nameof(ScenarioWorkspaceDataSet.ConfirmedReceipts));
    legacyJson.Remove(nameof(ScenarioWorkspaceDataSet.OpeningBacklog));
    legacyJson.Remove(nameof(ScenarioWorkspaceDataSet.PlanningEvidenceCoverage));

    var roundTrip = JsonSerializer.Deserialize<ScenarioWorkspaceDataSet>(legacyJson.ToJsonString())
        ?? throw new InvalidOperationException("legacy workspace JSON should deserialize");

    AssertTrue(roundTrip.ConfirmedReceipts is null, "legacy JSON should default confirmed receipts to null");
    AssertTrue(roundTrip.OpeningBacklog is null, "legacy JSON should default opening backlog to null");
    AssertTrue(roundTrip.PlanningEvidenceCoverage is null, "legacy JSON should default planning evidence coverage to null");

    var result = PlanningEvidenceValidator.ValidateForFreeze(roundTrip);
    AssertEqual("Incomplete", result.Status, "legacy missing evidence validation status");
    AssertTrue(result.Issues.Any(item => item.Reason == "MissingConfirmedReceipts"),
        "legacy missing receipt evidence should be explicit");
    AssertTrue(result.Issues.Any(item => item.Reason == "MissingOpeningBacklogEvidence"),
        "legacy missing backlog evidence should be explicit");
    AssertTrue(result.Issues.Any(item => item.Reason == "MissingPlanningEvidenceCoverage"),
        "legacy missing coverage evidence should be explicit");
}

static ScenarioWorkspaceDataSet BuildCompletePlanningEvidenceData()
{
    var anchor = new DateOnly(2026, 6, 1);
    var request = new ScenarioWorkspaceDataRequest(52, anchor, SkuFilter: new[] { "SAT-BUS-001" });
    var workspace = new SeedScenarioWorkspaceDataSource(SeedData.Create()).Load(request);
    var sku = workspace.Skus.Single();
    var source = workspace.SupplierItemSources.Single();

    return workspace with
    {
        Inventory = new[] { new InventoryPosition(sku.Sku, 8m, 10m, 2m) },
        Demand = Enumerable.Range(1, 52)
            .Select(week => new WeeklyDemand(sku.Sku, week, week % 5 + 1m))
            .ToList(),
        SupplierCapacityWindows = Enumerable.Range(1, 52)
            .Select(week => new SupplierCapacityWindow(source.Supplier, source.MaterialFamily, week, 20m, 14, "Green"))
            .ToList(),
        DdmrpParameters = workspace.DdmrpParameters
            .Select(item => item with { EffectiveThroughWeek = 52 })
            .ToList(),
        ConfirmedReceipts = new[]
        {
            new ConfirmedReceiptEvidence(
                "REC-IN-2",
                sku.Sku,
                4m,
                2,
                anchor.AddDays(7),
                "ConfirmedInTransit",
                "PO-1001",
                "PurchaseOrder",
                source.Supplier,
                source.MaterialFamily,
                "Confirmed",
                "Complete",
                "2026-06-01T00:00:00Z",
                "confirmed purchase-order receipt",
                "2026-06-07T16:30:00Z"),
            new ConfirmedReceiptEvidence(
                "REC-OUT-53",
                sku.Sku,
                6m,
                53,
                anchor.AddDays(364),
                "ConfirmedOpenSupply",
                "PO-1053",
                "PurchaseOrder",
                source.Supplier,
                source.MaterialFamily,
                "Confirmed",
                "Complete",
                "2026-06-01T00:00:00Z",
                "confirmed receipt beyond demand coverage",
                "2027-05-30T16:30:00Z")
        },
        OpeningBacklog = new[]
        {
            new OpeningBacklogEvidence(
                "BACKLOG-OPEN-1",
                sku.Sku,
                2m,
                "ORDER-1000",
                "Complete",
                "2026-06-01T00:00:00Z",
                "opening customer backlog")
        },
        PlanningEvidenceCoverage = new PlanningEvidenceCoverage(anchor, 1, 52, "Complete")
    };
}

static void TestInventoryFlowConservesWeeklyQuantity()
{
    var (data, sku) = BuildInventoryFlowFixture(
        new[] { 5m, 4m },
        openingOnHand: 10m,
        openingBacklog: 3m,
        qualifiedDemand: 999m,
        frozenReceipts: new[] { ("REC-W2", 2, 2m, "ConfirmedInTransit") });

    var result = InventoryFlowProjectionService.Project(
        data,
        "conservation",
        new[] { sku },
        data.Demand,
        Array.Empty<ProjectedReplenishmentOrder>(),
        Array.Empty<PrebuildCampaign>(),
        Array.Empty<SupplierCapacityLimit>(),
        "BASE-CONSERVATION");

    AssertEqual("Complete", result.Status, "inventory flow status");
    AssertEqual("BASE-CONSERVATION", result.BaselineSnapshotId!, "inventory flow baseline lineage");
    AssertEqual(2, result.Points.Count, "inventory flow point count");

    foreach (var point in result.Points)
    {
        var receipts = point.FrozenReceiptQuantity + point.SimulatedReceiptQuantity + point.PrebuildReceiptQuantity;
        AssertEqual(
            point.OpeningOnHand + receipts - point.TotalFulfilledDemand,
            point.EndingOnHand,
            $"on-hand conservation for week {point.Week}");
        AssertEqual(
            point.OpeningBacklog + point.Demand - point.TotalFulfilledDemand,
            point.EndingBacklog,
            $"backlog conservation for week {point.Week}");
        AssertTrue(point.EndingOnHand >= 0m, $"ending on hand must be nonnegative for week {point.Week}");
        AssertTrue(point.EndingBacklog >= 0m, $"ending backlog must be nonnegative for week {point.Week}");
    }

    AssertEqual(10m, result.Points[0].OpeningOnHand, "qualified demand must not be deducted from physical opening on hand");
    AssertEqual(result.Points[0].EndingOnHand, result.Points[1].OpeningOnHand, "weekly on-hand continuity");
    AssertEqual(result.Points[0].EndingBacklog, result.Points[1].OpeningBacklog, "weekly backlog continuity");
    AssertEqual(20m, result.Points[0].EndingInventoryValue, "weekly inventory value from physical on hand");
    AssertEqual(0m, result.Points[1].EndingInventoryValue, "ending inventory value from physical on hand");
    AssertEqual(12m, result.Summary!.TotalFulfilledQuantity, "summary total fulfilled quantity");
    AssertEqual(10m, result.Summary.AverageInventoryValue, "summary average weekly physical inventory value");
    AssertEqual(20m, result.Summary.PeakInventoryValue, "summary peak weekly physical inventory value");
    AssertEqual(0m, result.Summary.EndingInventoryValue, "summary ending physical inventory value");
    AssertEqual(0m, result.Summary.EndingBacklog, "summary ending backlog");
    AssertEqual(1, result.Summary.BacklogRecoveryWeek!.Value, "summary backlog recovery week");
    AssertEqual(2m, result.Summary.FrozenReceiptQuantity, "summary frozen receipt quantity");
}

static void TestInventoryFlowFulfillsOldestDemandFirst()
{
    var (data, sku) = BuildInventoryFlowFixture(
        new[] { 4m },
        openingOnHand: 4m,
        openingBacklog: 3m);

    var result = ProjectInventoryFlow(data, sku);
    var point = result.Points.Single();

    AssertEqual(3m, point.FulfilledOpeningBacklog, "old backlog fulfilled before new demand");
    AssertEqual(1m, point.FulfilledNewDemandOnTime, "new demand fulfilled after old backlog");
    AssertEqual(4m, point.TotalFulfilledDemand, "total fulfilled demand");
    AssertEqual(3m, point.EndingBacklog, "unfulfilled current demand becomes ending backlog");
    AssertEqual(25m, point.WeeklyServicePercent!.Value, "weekly on-time service percent");
    AssertEqual(25m, result.Summary!.OnTimeServicePercent!.Value, "summary on-time service percent");
}

static void TestSimulatedReceiptRespectsDltArrival()
{
    var (data, sku) = BuildInventoryFlowFixture(
        new[] { 0m, 0m, 0m, 0m },
        openingOnHand: 0m,
        openingBacklog: 0m,
        dltDays: 8);
    var order = new ProjectedReplenishmentOrder(sku.Sku, 1, 7m, 70m, "TopOfGreen");
    var outsideOrder = new ProjectedReplenishmentOrder(sku.Sku, 4, 5m, 50m, "TopOfGreen");

    var result = InventoryFlowProjectionService.Project(
        data,
        "dlt-arrival",
        new[] { sku },
        data.Demand,
        new[] { outsideOrder, order },
        Array.Empty<PrebuildCampaign>(),
        Array.Empty<SupplierCapacityLimit>());

    var log = result.ReceiptLog.Single(item => item.SourceKind == "SimulatedReplenishment" && item.RecommendationWeek == 1);
    var outsideLog = result.ReceiptLog.Single(item => item.SourceKind == "SimulatedReplenishment" && item.RecommendationWeek == 4);
    AssertEqual(1, log.RecommendationWeek!.Value, "simulated receipt recommendation week");
    AssertEqual(3, log.ArrivalWeek, "simulated receipt arrival after ceil DLT weeks");
    AssertEqual("SimulationAssumption", log.EvidenceSource, "simulated receipt evidence source");
    AssertTrue(result.Points.Where(item => item.Week < 3).All(item => item.SimulatedReceiptQuantity == 0m),
        "simulated receipt must not arrive before DLT");
    AssertEqual(7m, result.Points.Single(item => item.Week == 3).SimulatedReceiptQuantity,
        "simulated receipt quantity in DLT arrival week");
    AssertEqual(6, outsideLog.ArrivalWeek, "outside-horizon simulated receipt retains calculated arrival week");
    AssertEqual(0m, outsideLog.AcceptedQuantity, "outside-horizon receipt is not fabricated inside the projection");
    AssertEqual(5m, outsideLog.OutsideHorizonQuantity, "outside-horizon receipt log quantity");
    AssertEqual(5m, result.Summary!.OutsideHorizonQuantity, "outside-horizon summary quantity");
}

static void TestInventoryFlowSeparatesReceiptSources()
{
    var (data, sku) = BuildInventoryFlowFixture(
        new[] { 0m, 0m, 0m },
        openingOnHand: 0m,
        openingBacklog: 0m,
        dltDays: 7,
        frozenReceipts: new[] { ("REC-FROZEN", 1, 2m, "ConfirmedInTransit") });
    var order = new ProjectedReplenishmentOrder(sku.Sku, 1, 3m, 30m, "TopOfGreen");
    var campaign = new PrebuildCampaign("PREBUILD-1", sku.Sku, 3, 3, 3, 4m);

    var result = InventoryFlowProjectionService.Project(
        data,
        "receipt-sources",
        new[] { sku },
        data.Demand,
        new[] { order },
        new[] { campaign },
        Array.Empty<SupplierCapacityLimit>());

    AssertEqual(2m, result.Points.Single(item => item.Week == 1).FrozenReceiptQuantity, "frozen receipt bucket");
    AssertEqual(3m, result.Points.Single(item => item.Week == 2).SimulatedReceiptQuantity, "simulated receipt bucket");
    AssertEqual(4m, result.Points.Single(item => item.Week == 3).PrebuildReceiptQuantity, "prebuild receipt bucket");
    AssertTrue(result.ReceiptLog.Any(item => item.SourceKind == "ConfirmedInTransit" && item.EvidenceStatus == "Complete"),
        "receipt log should retain frozen receipt type and evidence");
    AssertTrue(result.ReceiptLog.Any(item => item.SourceKind == "SimulatedReplenishment" && item.EvidenceSource == "SimulationAssumption"),
        "receipt log should distinguish simulated assumptions");
    AssertTrue(result.ReceiptLog.Any(item => item.SourceKind == "PrebuildResponse" && item.EvidenceSource == "ResponseAssumption"),
        "receipt log should distinguish response assumptions");
    AssertEqual(2m, result.Summary!.FrozenReceiptQuantity, "summary frozen source total");
    AssertEqual(3m, result.Summary.SimulatedReceiptQuantity, "summary simulated source total");
    AssertEqual(4m, result.Summary.PrebuildReceiptQuantity, "summary prebuild source total");
}

static void TestPrebuildReceiptIsCountedOnce()
{
    var (data, sku) = BuildInventoryFlowFixture(
        new[] { 0m, 0m, 0m },
        openingOnHand: 0m,
        openingBacklog: 0m,
        dltDays: 7);
    var generatedSignal = new ProjectedReplenishmentOrder(sku.Sku, 1, 5m, 50m, "PrebuildCampaign");
    var campaign = new PrebuildCampaign("PREBUILD-DEDUPE", sku.Sku, 2, 2, 3, 5m);

    var result = InventoryFlowProjectionService.Project(
        data,
        "prebuild-once",
        new[] { sku },
        data.Demand,
        new[] { generatedSignal },
        new[] { campaign, campaign },
        Array.Empty<SupplierCapacityLimit>());

    AssertEqual(5m, result.Points.Sum(item => item.PrebuildReceiptQuantity), "prebuild quantity counted once");
    AssertEqual(0m, result.Points.Sum(item => item.SimulatedReceiptQuantity), "prebuild signal excluded from simulated receipts");
    AssertEqual(5m, result.Summary!.PrebuildReceiptQuantity, "prebuild summary counts one response quantity");
    AssertEqual(1, result.ReceiptLog.Count(item => item.SourceKind == "PrebuildResponse"), "one prebuild receipt log entry");
    AssertEqual(0, result.ReceiptLog.Count(item => item.SourceKind == "SimulatedReplenishment"), "no simulated log for prebuild signal");
}

static void TestConflictingPrebuildIdsAreEvidenceMissing()
{
    var (data, sku) = BuildInventoryFlowFixture(
        new[] { 0m, 0m },
        openingOnHand: 0m,
        openingBacklog: 0m);
    var first = new PrebuildCampaign("PREBUILD-CONFLICT", sku.Sku, 1, 1, 2, 2m);
    var conflicting = first with { BuildWeek = 2, Quantity = 3m };

    var result = InventoryFlowProjectionService.Project(
        data,
        "prebuild-conflict",
        new[] { sku },
        data.Demand,
        Array.Empty<ProjectedReplenishmentOrder>(),
        new[] { first, conflicting },
        Array.Empty<SupplierCapacityLimit>());

    AssertEqual("EvidenceMissing", result.Status, "conflicting campaign ID status");
    AssertEqual(0, result.Points.Count, "conflicting campaign ID must not fabricate points");
    AssertTrue(result.Issues.Any(item =>
            item.Scope == "PrebuildResponse" &&
            item.SourceId == first.CampaignId &&
            item.Reason == "ConflictingCampaignId" &&
            item.BlocksProjection),
        "conflicting campaign ID should be a blocking projection issue");
}

static void TestInventoryFlowScopesDemandBeforeLedger()
{
    var (data, sku) = BuildInventoryFlowFixture(
        new[] { 1m, 1m },
        openingOnHand: 2m,
        openingBacklog: 0m);
    var outside = new WeeklyDemand(sku.Sku, 53, 99m);
    var suppliedDemand = data.Demand.Append(outside).Append(outside).ToList();

    var result = InventoryFlowProjectionService.Project(
        data,
        "scoped-demand",
        new[] { sku },
        suppliedDemand,
        Array.Empty<ProjectedReplenishmentOrder>(),
        Array.Empty<PrebuildCampaign>(),
        Array.Empty<SupplierCapacityLimit>());

    AssertEqual("Complete", result.Status, "out-of-horizon demand rows should not invalidate active projection");
    AssertEqual(2, result.Points.Count, "projection should contain only active-horizon demand buckets");
    AssertEqual(2m, result.Summary!.TotalNewDemandQuantity, "summary should exclude demand outside active horizon");
}

static void TestZeroDemandServiceIsNotApplicable()
{
    var (data, sku) = BuildInventoryFlowFixture(
        new[] { 0m, 0m },
        openingOnHand: 1m,
        openingBacklog: 0m);

    var result = ProjectInventoryFlow(data, sku);

    AssertTrue(result.Summary!.OnTimeServicePercent is null, "zero-demand summary service percent should be null");
    AssertEqual("NotApplicable", result.Summary.ServiceStatus, "zero-demand summary service status");
    AssertTrue(result.SkuSummaries.Single().OnTimeServicePercent is null, "zero-demand SKU service percent should be null");
    AssertEqual("NotApplicable", result.SkuSummaries.Single().ServiceStatus, "zero-demand SKU service status");
    AssertTrue(result.Points.All(item => item.WeeklyServicePercent is null && item.WeeklyServiceStatus == "NotApplicable"),
        "zero-demand weekly service should be not applicable");
}

static void TestProjectionBeyondCoverageIsEvidenceMissing()
{
    var (complete, sku) = BuildInventoryFlowFixture(
        new[] { 1m, 1m, 1m },
        openingOnHand: 3m,
        openingBacklog: 0m);
    var incomplete = complete with
    {
        PlanningEvidenceCoverage = complete.PlanningEvidenceCoverage! with { CoverageThroughWeek = 2 }
    };

    var result = ProjectInventoryFlow(incomplete, sku);

    AssertEqual("EvidenceMissing", result.Status, "coverage failure projection status");
    AssertEqual(0, result.Points.Count, "coverage failure must not fabricate points");
    AssertTrue(result.Summary is null, "coverage failure must not fabricate summary");
    AssertTrue(result.Issues.Any(item => item.Reason == "IncompleteCoverage" && item.BlocksProjection),
        "coverage failure should expose the blocking planning evidence issue");
}

static void TestSupplierCapacityConstrainsSimulatedOnly()
{
    var (data, sku) = BuildInventoryFlowFixture(
        new[] { 0m, 0m, 0m },
        openingOnHand: 0m,
        openingBacklog: 0m,
        dltDays: 7,
        frozenReceipts: new[] { ("REC-CAPACITY-FROZEN", 2, 7m, "ConfirmedInTransit") });
    var source = data.SupplierItemSources.Single();
    var order = new ProjectedReplenishmentOrder(sku.Sku, 1, 5m, 50m, "TopOfGreen");
    var prebuild = new PrebuildCampaign("PREBUILD-CAPACITY", sku.Sku, 2, 2, 2, 4m);
    var limit = new SupplierCapacityLimit(source.Supplier, source.MaterialFamily, 2, 2, 3m);

    var result = InventoryFlowProjectionService.Project(
        data,
        "supplier-capacity-simulated-only",
        new[] { sku },
        data.Demand,
        new[] { order },
        new[] { prebuild },
        new[] { limit });

    AssertEqual("Complete", result.Status, "supplier-capacity projection status");
    AssertEqual(7m, result.Points.Single(item => item.Week == 2).FrozenReceiptQuantity,
        "supplier limit must not reduce a frozen receipt");
    AssertEqual(4m, result.Points.Single(item => item.Week == 2).PrebuildReceiptQuantity,
        "supplier limit must not reduce prebuild");
    AssertEqual(3m, result.Points.Single(item => item.Week == 2).SimulatedReceiptQuantity,
        "supplier limit constrains only the simulated receipt in the capped week");
    AssertEqual(2m, result.Points.Single(item => item.Week == 3).SimulatedReceiptQuantity,
        "deferred simulated receipt arrives in the next available week");

    var frozenLog = result.ReceiptLog.Single(item => item.SourceKind == "ConfirmedInTransit");
    var prebuildLog = result.ReceiptLog.Single(item => item.SourceKind == "PrebuildResponse");
    var simulatedLog = result.ReceiptLog.Single(item =>
        item.SourceKind == "SimulatedReplenishment" && item.ArrivalWeek == 2);
    AssertEqual(7m, frozenLog.AcceptedQuantity, "frozen source-log accepted quantity");
    AssertEqual(0m, frozenLog.DeferredQuantity, "frozen source-log deferred quantity");
    AssertEqual(4m, prebuildLog.AcceptedQuantity, "prebuild source-log accepted quantity");
    AssertEqual(0m, prebuildLog.DeferredQuantity, "prebuild source-log deferred quantity");
    AssertEqual(3m, simulatedLog.AcceptedQuantity, "simulated source-log capped quantity");
    AssertEqual(2m, simulatedLog.DeferredQuantity, "simulated source-log deferred quantity");
}

static void TestSupplierCapacityAllocatesProportionally()
{
    var (data, skus) = BuildSharedSupplierInventoryFlowFixture(2, 3, 100m);
    var source = data.SupplierItemSources.First();
    var orders = new[]
    {
        new ProjectedReplenishmentOrder(skus[0].Sku, 1, 80m, 800m, "TopOfGreen"),
        new ProjectedReplenishmentOrder(skus[1].Sku, 1, 20m, 200m, "TopOfGreen")
    };
    var limits = new[]
    {
        new SupplierCapacityLimit(source.Supplier, source.MaterialFamily, 2, 2, 50m),
        new SupplierCapacityLimit(source.Supplier, source.MaterialFamily, 3, 3, 100m)
    };

    var result = InventoryFlowProjectionService.Project(
        data,
        "supplier-capacity-proportional",
        skus,
        data.Demand,
        orders,
        Array.Empty<PrebuildCampaign>(),
        limits);

    var first = result.ReceiptLog.Single(item =>
        item.SourceKind == "SimulatedReplenishment" && item.Sku == skus[0].Sku && item.ArrivalWeek == 2);
    var second = result.ReceiptLog.Single(item =>
        item.SourceKind == "SimulatedReplenishment" && item.Sku == skus[1].Sku && item.ArrivalWeek == 2);
    AssertEqual(40m, first.AcceptedQuantity, "80-share accepted quantity under capacity 50");
    AssertEqual(10m, second.AcceptedQuantity, "20-share accepted quantity under capacity 50");
    AssertEqual(40m, first.DeferredQuantity, "80-share deferred quantity under capacity 50");
    AssertEqual(10m, second.DeferredQuantity, "20-share deferred quantity under capacity 50");
    AssertEqual(50m, result.Points.Where(item => item.Week == 2).Sum(item => item.SimulatedReceiptQuantity),
        "same supplier/material/arrival group must conserve weekly capacity");
    AssertTrue(result.Trace.Any(item =>
            item.Stage == "SupplierCapacityAllocation" &&
            item.Week == 2 &&
            item.Explanation.Contains(source.Supplier, StringComparison.Ordinal) &&
            item.Explanation.Contains(source.MaterialFamily, StringComparison.Ordinal) &&
            item.Explanation.Contains("capacity=50", StringComparison.Ordinal)),
        "proportional allocation should retain supplier, material family and capacity trace");
}

static void TestSupplierCapacityAssignsRoundingResidualDeterministically()
{
    var (data, skus) = BuildSharedSupplierInventoryFlowFixture(2, 2, 10m);
    var source = data.SupplierItemSources.First();
    var orders = new[]
    {
        new ProjectedReplenishmentOrder(skus[1].Sku, 1, 1m, 10m, "TopOfGreen"),
        new ProjectedReplenishmentOrder(skus[0].Sku, 1, 1m, 10m, "TopOfGreen")
    };
    var limit = new SupplierCapacityLimit(source.Supplier, source.MaterialFamily, 2, 2, 1m);

    var result = InventoryFlowProjectionService.Project(
        data,
        "supplier-capacity-rounding",
        skus,
        data.Demand,
        orders,
        Array.Empty<PrebuildCampaign>(),
        new[] { limit });

    var allocated = result.ReceiptLog
        .Where(item => item.SourceKind == "SimulatedReplenishment" && item.ArrivalWeek == 2)
        .OrderBy(item => item.SourceId, StringComparer.Ordinal)
        .ToList();
    AssertEqual(0m, allocated[0].AcceptedQuantity, "first stable source receives rounded proportional share");
    AssertEqual(1m, allocated[1].AcceptedQuantity, "last stable source receives the final residual");
    AssertEqual(1m, allocated.Sum(item => item.AcceptedQuantity), "rounded allocation must equal available capacity");
    AssertTrue(result.Trace.Any(item =>
            item.Stage == "SupplierCapacityRounding" &&
            item.SourceId == allocated[1].SourceId &&
            item.Explanation.Contains("residual=1", StringComparison.Ordinal)),
        "rounding residual assignment must be stable and traceable");
}

static void TestSupplierCapacityNeverRoundsFractionalLimitUpward()
{
    var (data, sku) = BuildInventoryFlowFixture(
        new[] { 0m, 0m },
        openingOnHand: 0m,
        openingBacklog: 0m,
        dltDays: 7);
    var source = data.SupplierItemSources.Single();
    var order = new ProjectedReplenishmentOrder(sku.Sku, 1, 1m, 10m, "TopOfGreen");
    var limit = new SupplierCapacityLimit(source.Supplier, source.MaterialFamily, 2, 2, 0.6m);

    var result = InventoryFlowProjectionService.Project(
        data,
        "supplier-capacity-fractional-limit",
        new[] { sku },
        data.Demand,
        new[] { order },
        Array.Empty<PrebuildCampaign>(),
        new[] { limit });

    var allocated = result.ReceiptLog.Single(item =>
        item.SourceKind == "SimulatedReplenishment" && item.ArrivalWeek == 2);
    AssertEqual(0m, allocated.AcceptedQuantity,
        "capacity below the minimum output unit must not be rounded up");
    AssertEqual(1m, allocated.DeferredQuantity,
        "quantity rejected by capacity precision must be deferred");
    AssertTrue(allocated.AcceptedQuantity <= 0.6m,
        "accepted group total must not exceed raw supplier capacity");
    AssertEqual(decimal.Truncate(allocated.AcceptedQuantity), allocated.AcceptedQuantity,
        "accepted quantity must conform to zero-decimal output precision");
    AssertEqual(decimal.Truncate(allocated.RoundingResidual), allocated.RoundingResidual,
        "rounding residual must conform to zero-decimal output precision");
}

static void TestSupplierCapacityQuantizesFractionalSourceCaps()
{
    var (data, skus) = BuildSharedSupplierInventoryFlowFixture(2, 2, 10m);
    var source = data.SupplierItemSources.First();
    var orders = skus
        .Select(sku => new ProjectedReplenishmentOrder(sku.Sku, 1, 0.6m, 6m, "TopOfGreen"))
        .ToList();
    var limit = new SupplierCapacityLimit(source.Supplier, source.MaterialFamily, 2, 2, 0.6m);

    var result = InventoryFlowProjectionService.Project(
        data,
        "supplier-capacity-fractional-source-caps",
        skus,
        data.Demand,
        orders,
        Array.Empty<PrebuildCampaign>(),
        new[] { limit });

    var allocated = result.ReceiptLog
        .Where(item => item.SourceKind == "SimulatedReplenishment" && item.ArrivalWeek == 2)
        .ToList();
    AssertTrue(allocated.All(item => item.AcceptedQuantity == decimal.Truncate(item.AcceptedQuantity)),
        "every accepted quantity must conform to zero-decimal output precision");
    AssertTrue(allocated.All(item => item.RoundingResidual == decimal.Truncate(item.RoundingResidual)),
        "every rounding residual must conform to zero-decimal output precision");
    AssertTrue(allocated.All(item => item.AcceptedQuantity <= item.RequestedQuantity),
        "no source may receive more than it requested");
    AssertTrue(allocated.Sum(item => item.AcceptedQuantity) <= 0.6m,
        "accepted group total must not exceed raw supplier capacity");
    AssertEqual(0m, allocated.Sum(item => item.AcceptedQuantity),
        "fractional source caps below the minimum output unit cannot accept a whole unit");
}

static void TestSupplierCapacityRoundingNeverOverallocatesSource()
{
    var (data, skus) = BuildSharedSupplierInventoryFlowFixture(7, 2, 250m);
    var source = data.SupplierItemSources.First();
    var requested = new[] { 35m, 20m, 23m, 2m, 93m, 23m, 1m };
    var stableSkus = skus
        .OrderBy(item => $"SIM-{item.Sku}-W", StringComparer.Ordinal)
        .ToList();
    var orders = stableSkus
        .Select((sku, index) => new ProjectedReplenishmentOrder(
            sku.Sku,
            1,
            requested[index],
            requested[index] * 10m,
            "TopOfGreen"))
        .ToList();
    var limit = new SupplierCapacityLimit(source.Supplier, source.MaterialFamily, 2, 2, 191m);

    var result = InventoryFlowProjectionService.Project(
        data,
        "supplier-capacity-rounding-bounds",
        skus,
        data.Demand,
        orders,
        Array.Empty<PrebuildCampaign>(),
        new[] { limit });

    var allocated = result.ReceiptLog
        .Where(item => item.SourceKind == "SimulatedReplenishment" && item.ArrivalWeek == 2)
        .OrderBy(item => item.SourceId, StringComparer.Ordinal)
        .ToList();
    AssertTrue(allocated.Select((item, index) => item.AcceptedQuantity <= requested[index]).All(item => item),
        "rounding residual must never accept more than a source requested");
    AssertTrue(allocated.All(item => item.AcceptedQuantity >= 0m && item.DeferredQuantity >= 0m),
        "rounding residual must keep accepted and deferred quantities nonnegative");
    AssertEqual(191m, allocated.Sum(item => item.AcceptedQuantity),
        "bounded rounded allocations must still conserve available capacity");
}

static void TestDeferredSimulatedReceiptCarriesForward()
{
    var (data, sku) = BuildInventoryFlowFixture(
        new[] { 0m, 0m, 0m },
        openingOnHand: 0m,
        openingBacklog: 0m,
        dltDays: 7);
    var source = data.SupplierItemSources.Single();
    var order = new ProjectedReplenishmentOrder(sku.Sku, 1, 10m, 100m, "TopOfGreen");
    var limits = new[]
    {
        new SupplierCapacityLimit(source.Supplier, source.MaterialFamily, 2, 2, 4m),
        new SupplierCapacityLimit(source.Supplier, source.MaterialFamily, 3, 3, 4m)
    };

    var result = InventoryFlowProjectionService.Project(
        data,
        "supplier-capacity-carry",
        new[] { sku },
        data.Demand,
        new[] { order },
        Array.Empty<PrebuildCampaign>(),
        limits);

    var simulated = result.ReceiptLog
        .Where(item => item.SourceKind == "SimulatedReplenishment")
        .OrderBy(item => item.ArrivalWeek)
        .ToList();
    AssertEqual(3, simulated.Count, "deferred source-log row count including horizon exit");
    AssertEqual(4m, simulated[0].AcceptedQuantity, "initial constrained arrival accepted quantity");
    AssertEqual(6m, simulated[0].DeferredQuantity, "initial constrained arrival deferred quantity");
    AssertEqual(4m, simulated[1].AcceptedQuantity, "next-week carried arrival accepted quantity");
    AssertEqual(2m, simulated[1].DeferredQuantity, "next-week carried arrival deferred quantity");
    AssertEqual(4, simulated[2].ArrivalWeek, "unaccepted carry retains next authoritative attempt week");
    AssertEqual(0m, simulated[2].AcceptedQuantity, "outside-horizon carry is not accepted inside projection");
    AssertEqual(2m, simulated[2].OutsideHorizonQuantity, "outside-horizon carry quantity");
    AssertEqual("OutsideHorizon", simulated[2].EvidenceStatus, "outside-horizon carry evidence status");
    AssertEqual(4m, result.Points.Single(item => item.Week == 2).SimulatedReceiptQuantity,
        "week two simulated receipt from accepted allocation");
    AssertEqual(4m, result.Points.Single(item => item.Week == 3).SimulatedReceiptQuantity,
        "week three simulated receipt from carry allocation");
    AssertEqual(2m, result.Summary!.OutsideHorizonQuantity, "outside-horizon carry summary quantity");
}

static void TestFrozenReceiptsRemainFixed()
{
    var (data, sku) = BuildInventoryFlowFixture(
        new[] { 0m, 0m },
        openingOnHand: 0m,
        openingBacklog: 0m,
        frozenReceipts: new[] { ("REC-FIXED-CAPACITY", 2, 9m, "ConfirmedOpenSupply") });
    var source = data.SupplierItemSources.Single();
    var zeroLimit = new SupplierCapacityLimit(source.Supplier, source.MaterialFamily, 2, 2, 0m);

    var result = InventoryFlowProjectionService.Project(
        data,
        "frozen-receipt-fixed",
        new[] { sku },
        data.Demand,
        Array.Empty<ProjectedReplenishmentOrder>(),
        Array.Empty<PrebuildCampaign>(),
        new[] { zeroLimit });

    var receipt = result.ReceiptLog.Single(item => item.SourceKind == "ConfirmedOpenSupply");
    AssertEqual(2, receipt.ArrivalWeek, "frozen receipt authoritative week");
    AssertEqual(9m, receipt.AcceptedQuantity, "frozen receipt must not be reduced by zero capacity");
    AssertEqual(0m, receipt.DeferredQuantity, "frozen receipt must not be deferred by zero capacity");
    AssertEqual(9m, result.Points.Single(item => item.Week == 2).FrozenReceiptQuantity,
        "frozen receipt remains in its fixed physical bucket");
}

static void TestPrebuildRemainsUnchanged()
{
    var (data, sku) = BuildInventoryFlowFixture(
        new[] { 0m, 0m },
        openingOnHand: 0m,
        openingBacklog: 0m);
    var source = data.SupplierItemSources.Single();
    var zeroLimit = new SupplierCapacityLimit(source.Supplier, source.MaterialFamily, 2, 2, 0m);
    var prebuild = new PrebuildCampaign("PREBUILD-FIXED-CAPACITY", sku.Sku, 2, 2, 2, 9m);
    var outsidePrebuild = new PrebuildCampaign("PREBUILD-OUTSIDE-CAPACITY", sku.Sku, 3, 3, 3, 5m);

    var result = InventoryFlowProjectionService.Project(
        data,
        "prebuild-fixed",
        new[] { sku },
        data.Demand,
        Array.Empty<ProjectedReplenishmentOrder>(),
        new[] { outsidePrebuild, prebuild },
        new[] { zeroLimit });

    var receipt = result.ReceiptLog.Single(item => item.SourceId == prebuild.CampaignId);
    var outsideReceipt = result.ReceiptLog.Single(item => item.SourceId == outsidePrebuild.CampaignId);
    AssertEqual(2, receipt.ArrivalWeek, "prebuild configured completion week");
    AssertEqual(9m, receipt.AcceptedQuantity, "prebuild must not be reduced by zero supplier capacity");
    AssertEqual(0m, receipt.DeferredQuantity, "prebuild must not be deferred by zero supplier capacity");
    AssertEqual(9m, result.Points.Single(item => item.Week == 2).PrebuildReceiptQuantity,
        "prebuild remains in its configured physical bucket");
    AssertEqual(3, outsideReceipt.ArrivalWeek, "outside prebuild retains its configured completion week");
    AssertEqual(5m, outsideReceipt.OutsideHorizonQuantity, "outside prebuild retains its configured quantity");
    AssertEqual("Complete", outsideReceipt.EvidenceStatus,
        "supplier allocation must not relabel prebuild response evidence");
}

static void TestMissingConstrainedSupplyMappingIsEvidenceMissing()
{
    var (complete, sku) = BuildInventoryFlowFixture(
        new[] { 0m, 0m },
        openingOnHand: 0m,
        openingBacklog: 0m);
    var source = complete.SupplierItemSources.Single();
    var missingMapping = complete with { SupplierItemSources = Array.Empty<SupplierItemSource>() };
    var order = new ProjectedReplenishmentOrder(sku.Sku, 1, 5m, 50m, "TopOfGreen");
    var limit = new SupplierCapacityLimit(source.Supplier, source.MaterialFamily, 2, 2, 3m);

    var result = InventoryFlowProjectionService.Project(
        missingMapping,
        "missing-simulated-supply-mapping",
        new[] { sku },
        missingMapping.Demand,
        new[] { order },
        Array.Empty<PrebuildCampaign>(),
        new[] { limit });

    AssertEqual("EvidenceMissing", result.Status, "missing constrained supply mapping status");
    AssertEqual(0, result.Points.Count, "missing constrained supply mapping must not fabricate a projection");
    AssertTrue(result.Issues.Any(item =>
            item.Scope == "SupplierCapacity" &&
            item.Sku == sku.Sku &&
            item.Week == 2 &&
            item.Reason == "MissingConstrainedSupplyMapping" &&
            item.BlocksProjection),
        "missing constrained supply mapping should be a blocking issue");
}

static void TestMissingConstrainedCapacityWeekIsEvidenceMissing()
{
    var (complete, sku) = BuildInventoryFlowFixture(
        new[] { 0m, 0m },
        openingOnHand: 0m,
        openingBacklog: 0m);
    var source = complete.SupplierItemSources.Single();
    var missingWeek = complete with
    {
        SupplierCapacityWindows = complete.SupplierCapacityWindows.Where(item => item.Week != 2).ToList()
    };
    var order = new ProjectedReplenishmentOrder(sku.Sku, 1, 5m, 50m, "TopOfGreen");
    var limit = new SupplierCapacityLimit(source.Supplier, source.MaterialFamily, 2, 2, 3m);

    var result = InventoryFlowProjectionService.Project(
        missingWeek,
        "missing-simulated-capacity-week",
        new[] { sku },
        missingWeek.Demand,
        new[] { order },
        Array.Empty<PrebuildCampaign>(),
        new[] { limit });

    AssertEqual("EvidenceMissing", result.Status, "missing constrained capacity week status");
    AssertEqual(0, result.Points.Count, "missing constrained capacity week must not fabricate a projection");
    AssertTrue(result.Issues.Any(item =>
            item.Scope == "SupplierCapacity" &&
            item.Sku == sku.Sku &&
            item.Week == 2 &&
            item.Reason == "MissingConstrainedCapacityWeek" &&
            item.BlocksProjection),
        "missing constrained capacity week should be a blocking issue");
}

static void TestExplicitNotApplicableSupplierCapacityIsUnbounded()
{
    var (complete, sku) = BuildInventoryFlowFixture(
        new[] { 0m, 0m },
        openingOnHand: 0m,
        openingBacklog: 0m);
    var explicitlyUnbounded = complete with
    {
        SupplierCapacityWindows = complete.SupplierCapacityWindows
            .Select(item => item.Week == 2
                ? item with { CommittedCapacity = 0m, RiskStatus = "NotApplicable" }
                : item)
            .ToList()
    };
    var order = new ProjectedReplenishmentOrder(sku.Sku, 1, 25m, 250m, "TopOfGreen");

    var result = InventoryFlowProjectionService.Project(
        explicitlyUnbounded,
        "not-applicable-supplier-capacity",
        new[] { sku },
        explicitlyUnbounded.Demand,
        new[] { order },
        Array.Empty<PrebuildCampaign>(),
        Array.Empty<SupplierCapacityLimit>());

    var receipt = result.ReceiptLog.Single(item => item.SourceKind == "SimulatedReplenishment");
    AssertEqual("Complete", result.Status, "explicit not-applicable capacity projection status");
    AssertEqual(25m, receipt.AcceptedQuantity, "explicit not-applicable source is unbounded");
    AssertEqual(0m, receipt.DeferredQuantity, "explicit not-applicable source has no capacity deferral");
    AssertEqual("NotApplicable", receipt.EvidenceStatus, "explicit unbounded source-log evidence status");
}

static void TestInventoryFlowFieldsPreserveLegacyScenarioJson()
{
    var preview = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()))
        .Preview(new ScenarioRunPreviewRequest(2));
    var legacyJson = JsonNode.Parse(JsonSerializer.Serialize(preview))!.AsObject();
    foreach (var caseName in new[] { nameof(ScenarioRunPreviewResult.Baseline), nameof(ScenarioRunPreviewResult.Scenario) })
    {
        var previewCase = legacyJson[caseName]!.AsObject();
        previewCase.Remove(nameof(ScenarioRunPreviewCase.InventoryFlow));
        previewCase.Remove(nameof(ScenarioRunPreviewCase.ScenarioMetricEvidence));
    }

    var roundTrip = JsonSerializer.Deserialize<ScenarioRunPreviewResult>(legacyJson.ToJsonString())
        ?? throw new InvalidOperationException("legacy scenario preview JSON should deserialize");

    AssertTrue(roundTrip.Baseline.InventoryFlow is null, "legacy baseline inventory flow should default to null");
    AssertTrue(roundTrip.Baseline.ScenarioMetricEvidence is null, "legacy baseline metric evidence should default to null");
    AssertTrue(roundTrip.Scenario.InventoryFlow is null, "legacy scenario inventory flow should default to null");
    AssertTrue(roundTrip.Scenario.ScenarioMetricEvidence is null, "legacy scenario metric evidence should default to null");
}

static void TestPreviewReturnsCompleteInventoryFlow()
{
    var data = BuildCompletePlanningEvidenceData();
    var result = new ScenarioRunPreviewService(new StaticScenarioWorkspaceDataSource(data))
        .Preview(new ScenarioRunPreviewRequest(6));

    foreach (var previewCase in new[] { result.Baseline, result.Scenario })
    {
        var flow = previewCase.InventoryFlow;
        AssertTrue(flow is not null, $"{previewCase.CaseId} should expose an inventory flow result");
        AssertEqual("Complete", flow!.Status, $"{previewCase.CaseId} inventory flow status");
        AssertEqual(6, flow.Points.Count, $"{previewCase.CaseId} should expose one physical point per requested week");
        AssertTrue(flow.Summary is not null, $"{previewCase.CaseId} should expose a physical summary");
        AssertTrue(flow.Trace.Any(item => item.Stage == "ValidatedInputs"), $"{previewCase.CaseId} should retain projection trace");
        AssertTrue(
            previewCase.ScenarioMetricEvidence?.Any(item =>
                item.JsonPath == "metrics.serviceLevelPercent" &&
                item.EvidenceStatus == "Complete" &&
                item.Source == "PhysicalProjection" &&
                item.ProjectionCaseId == previewCase.CaseId) == true,
            $"{previewCase.CaseId} service metric should point to the physical projection");
    }
}

static void TestPhysicalFlowDrivesMetricsAndBudget()
{
    var data = BuildCompletePlanningEvidenceData();
    var result = new ScenarioRunPreviewService(new StaticScenarioWorkspaceDataSource(data))
        .Preview(new ScenarioRunPreviewRequest(6));
    var previewCase = result.Scenario;
    var flow = previewCase.InventoryFlow ?? throw new InvalidOperationException("physical flow should be present");
    var summary = flow.Summary ?? throw new InvalidOperationException("physical summary should be present");
    var skuMap = data.Skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);

    AssertEqual(decimal.Round(summary.OnTimeServicePercent ?? 0m, 1), previewCase.Metrics.ServiceLevelPercent,
        "top-level service should use physical on-time fulfillment");
    AssertEqual(decimal.Round(summary.AverageInventoryValue, 0), previewCase.Metrics.AverageInventoryValue,
        "top-level inventory should use weekly physical inventory value");

    AssertTrue(previewCase.Budget.Count > 0, "physical budget comparison should have rows");
    foreach (var budget in previewCase.Budget)
    {
        var expected = flow.Points
            .Where(item => item.Week == budget.Week && skuMap[item.Sku].Family == budget.Family)
            .Sum(item => item.EndingInventoryValue);
        AssertEqual(decimal.Round(expected, 0), budget.ProjectedInventoryValue,
            $"budget projected inventory {budget.Family} week {budget.Week}");
        AssertEqual(decimal.Round(expected - budget.BudgetInventoryValue, 0), budget.BudgetInventoryVariance,
            $"budget inventory variance {budget.Family} week {budget.Week}");
    }

    foreach (var cell in previewCase.ProductFamilyDashboard.WeeklyCells)
    {
        var expected = flow.Points
            .Where(item => item.Week == cell.Week && skuMap[item.Sku].Family == cell.Family)
            .Sum(item => item.EndingInventoryValue);
        AssertEqual(decimal.Round(expected, 0), cell.InventoryValue,
            $"product-family weekly physical inventory {cell.Family} week {cell.Week}");
    }
    AssertTrue(
        previewCase.ProductFamilyDashboard.Details
            .SelectMany(item => item.WeeklyCells)
            .All(detailCell => previewCase.ProductFamilyDashboard.WeeklyCells.Any(cell => cell == detailCell)),
        "product-family detail cells should reuse the physical weekly cells");

    foreach (var family in previewCase.ProductFamilyDashboard.Summaries)
    {
        var familyPoints = flow.Points.Where(item => skuMap[item.Sku].Family == family.Family).ToList();
        var weeklyInventory = familyPoints.GroupBy(item => item.Week).Select(group => group.Sum(item => item.EndingInventoryValue)).ToList();
        var demand = familyPoints.Sum(item => item.Demand);
        var fulfilled = familyPoints.Sum(item => item.FulfilledNewDemandOnTime);
        var expectedService = demand == 0m ? 0m : decimal.Round(fulfilled * 100m / demand, 1);
        AssertEqual(expectedService, family.ServiceLevelPercent, $"product-family physical service {family.Family}");
        AssertEqual(decimal.Round(weeklyInventory.Average(), 0), family.AverageInventoryValue,
            $"product-family average physical inventory {family.Family}");
        AssertEqual(decimal.Round(weeklyInventory.Max(), 0), family.PeakInventoryValue,
            $"product-family peak physical inventory {family.Family}");
    }

    foreach (var point in previewCase.BufferTrend.Series)
    {
        var physical = flow.Points.Single(item => item.Sku == point.Sku && item.Week == point.Week);
        AssertTrue(point.PhysicalPosition is not null, $"buffer trend physical position {point.Sku} week {point.Week}");
        AssertEqual(physical.EndingOnHand, point.PhysicalPosition!.EndingOnHand,
            $"buffer trend physical on-hand join {point.Sku} week {point.Week}");
        AssertEqual(physical.EndingBacklog, point.PhysicalPosition.EndingBacklog,
            $"buffer trend physical backlog join {point.Sku} week {point.Week}");
        AssertEqual(decimal.Round(physical.EndingInventoryValue, 0), point.InventoryValue,
            $"buffer trend physical inventory {point.Sku} week {point.Week}");
    }
    AssertEqual(
        decimal.Round(previewCase.BufferTrend.Series.Average(item => item.InventoryValue!.Value), 0),
        previewCase.BufferTrend.Kpis.AverageInventoryValue,
        "buffer trend average should aggregate physical inventory values");
    AssertEqual(
        decimal.Round(previewCase.BufferTrend.Series.Max(item => item.InventoryValue!.Value), 0),
        previewCase.BufferTrend.Kpis.PeakInventoryValue,
        "buffer trend peak should aggregate physical inventory values");

    var evidence = previewCase.ScenarioMetricEvidence ?? throw new InvalidOperationException("physical metric evidence should be present");
    foreach (var path in RequiredPhysicalScenarioMetricPaths())
    {
        AssertTrue(evidence.Any(item =>
                item.JsonPath == path &&
                item.EvidenceStatus == "Complete" &&
                item.Source == "PhysicalProjection" &&
                item.ProjectionCaseId == previewCase.CaseId),
            $"physical evidence should cover {path}");
    }
    AssertTrue(evidence.Count(item => item.JsonPath == "metrics.averageInventoryValue") >= 2,
        "inventory and cash occupation should have separate path-addressed evidence entries");
}

static void TestBufferSignalSeparatesPlanningAndPhysicalPositions()
{
    var data = BuildCompletePlanningEvidenceData();
    var preview = new ScenarioRunPreviewService(new StaticScenarioWorkspaceDataSource(data))
        .Preview(new ScenarioRunPreviewRequest(6)).Scenario;
    var flow = preview.InventoryFlow ?? throw new InvalidOperationException("complete physical flow should be present");

    foreach (var point in preview.BufferTrend.Series)
    {
        var physical = flow.Points.Single(item => item.Sku == point.Sku && item.Week == point.Week);
        AssertTrue(point.PhysicalPosition is not null, $"{point.Sku} week {point.Week} should retain complete physical evidence");
        AssertEqual(physical.EndingOnHand, point.PhysicalPosition!.EndingOnHand, $"physical ending on hand {point.Sku} week {point.Week}");
        AssertEqual(physical.EndingBacklog, point.PhysicalPosition.EndingBacklog, $"physical ending backlog {point.Sku} week {point.Week}");
        AssertEqual(DdmrpCalculator.GetPositionStatus(physical.EndingOnHand, point.Sizing!.Zones), point.PhysicalPosition.OnHandStatus,
            $"physical on-hand status should use this week's zones {point.Sku} week {point.Week}");
        AssertEqual(DdmrpCalculator.GetPositionStatus(point.EndNetFlowBeforeReplenishment, point.Sizing.Zones), point.Status,
            $"compatible status should remain the pre-replenishment NFP status {point.Sku} week {point.Week}");
    }

    AssertTrue(preview.BufferTrend.Kpis.OnHandRedSkuCount.HasValue, "complete physical evidence should expose on-hand red SKU KPI");
    AssertTrue(preview.BufferTrend.Kpis.OnHandYellowSkuCount.HasValue, "complete physical evidence should expose on-hand yellow SKU KPI");
    AssertTrue(preview.BufferTrend.Kpis.OnHandStockoutWeekCount.HasValue, "complete physical evidence should expose on-hand shortage KPI");

    var duplicate = flow.Points[0];
    var duplicateFlow = flow with { Points = flow.Points.Concat(new[] { duplicate }).ToList() };
    var duplicateTrend = BufferTrendWorkspaceService.Build(data, preview.CaseId, preview.Name, data.Skus, preview.Plan, duplicateFlow);
    AssertBufferTrendPhysicalEvidenceMissing(duplicateTrend, "duplicate physical flow key");

    var partialFlow = flow with { Points = flow.Points.Skip(1).ToList() };
    var partialTrend = BufferTrendWorkspaceService.Build(data, preview.CaseId, preview.Name, data.Skus, preview.Plan, partialFlow);
    AssertBufferTrendPhysicalEvidenceMissing(partialTrend, "missing one physical flow key");
    var partialDashboard = ProductFamilyDashboardService.Build(
        data,
        preview.CaseId,
        preview.Name,
        data.Skus,
        preview.Plan,
        preview.SupplierCapacity,
        preview.Budget,
        partialFlow);
    AssertTrue(partialDashboard.WeeklyCells.All(item =>
            item.InventoryValue is null && item.BudgetInventoryVariance is null),
        "one missing physical key must invalidate product-family weekly inventory amounts");
    AssertTrue(partialDashboard.Summaries.All(item =>
            item.AverageInventoryValue is null &&
            item.PeakInventoryValue is null &&
            item.BudgetInventoryVariance is null),
        "one missing physical key must invalidate product-family inventory summaries");
    AssertTrue(partialDashboard.Details.SelectMany(item => item.BufferSummaries)
            .All(item => item.AverageInventoryValue is null),
        "one missing physical key must invalidate product-family buffer summaries");
}

static void TestBufferSignalOmitsPhysicalPositionWhenEvidenceIsMissing()
{
    var incomplete = BuildCompletePlanningEvidenceData() with
    {
        ConfirmedReceipts = null,
        OpeningBacklog = null,
        PlanningEvidenceCoverage = null
    };
    var preview = new ScenarioRunPreviewService(new StaticScenarioWorkspaceDataSource(incomplete))
        .Preview(new ScenarioRunPreviewRequest(6)).Scenario;

    AssertEqual("EvidenceMissing", preview.InventoryFlow?.Status ?? string.Empty, "missing-flow test should use incomplete inventory flow");
    AssertTrue(preview.BufferTrend.Series.All(item => item.PhysicalPosition is null),
        "missing physical flow must not be coerced into a zero inventory position");
    AssertTrue(preview.BufferTrend.Kpis.OnHandRedSkuCount is null &&
        preview.BufferTrend.Kpis.OnHandYellowSkuCount is null &&
        preview.BufferTrend.Kpis.OnHandStockoutWeekCount is null,
        "missing physical flow must retain nullable physical KPIs rather than zeroes");
    AssertBufferTrendPhysicalEvidenceMissing(preview.BufferTrend, "missing inventory-flow evidence");
}

static void TestBufferSignalRejectsCrossCasePhysicalEvidence()
{
    var data = BuildCompletePlanningEvidenceData();
    var preview = new ScenarioRunPreviewService(new StaticScenarioWorkspaceDataSource(data))
        .Preview(new ScenarioRunPreviewRequest(6)).Scenario;
    var crossCaseFlow = (preview.InventoryFlow ?? throw new InvalidOperationException("complete physical flow should be present")) with
    {
        CaseId = "another-case"
    };
    var trend = BufferTrendWorkspaceService.Build(data, preview.CaseId, preview.Name, data.Skus, preview.Plan, crossCaseFlow);

    AssertTrue(trend.Series.All(item => item.PhysicalPosition is null),
        "a complete inventory flow for another case must not populate this case's physical positions");
    AssertTrue(trend.Kpis.OnHandRedSkuCount is null &&
        trend.Kpis.OnHandYellowSkuCount is null &&
        trend.Kpis.OnHandStockoutWeekCount is null,
        "cross-case physical evidence must leave all physical KPIs nullable");
    AssertBufferTrendPhysicalEvidenceMissing(trend, "cross-case physical evidence");
}

static void TestIncompletePhysicalFlowNeverPublishesInventoryAmounts()
{
    var incomplete = BuildCompletePlanningEvidenceData() with
    {
        ConfirmedReceipts = null,
        OpeningBacklog = null,
        PlanningEvidenceCoverage = null
    };
    var previewService = new ScenarioRunPreviewService(new StaticScenarioWorkspaceDataSource(incomplete));
    var result = previewService.Preview(new ScenarioRunPreviewRequest(6));
    var scenario = JsonSerializer.SerializeToNode(
        result.Scenario,
        new JsonSerializerOptions(JsonSerializerDefaults.Web))!.AsObject();

    AssertTrue(scenario["metrics"]!["averageInventoryValue"] is null,
        "missing physical flow must not publish NFP value as scenario average inventory");
    AssertTrue(scenario["budget"]!.AsArray().All(item =>
            item!["projectedInventoryValue"] is null && item["budgetInventoryVariance"] is null),
        "missing physical flow must not publish NFP value as budget inventory");
    var dashboard = scenario["productFamilyDashboard"]!.AsObject();
    AssertTrue(dashboard["weeklyCells"]!.AsArray().All(item =>
            item!["inventoryValue"] is null && item["budgetInventoryVariance"] is null),
        "missing physical flow must not publish NFP value in product-family weekly cells");
    AssertTrue(dashboard["summaries"]!.AsArray().All(item =>
            item!["averageInventoryValue"] is null &&
            item["peakInventoryValue"] is null &&
            item["budgetInventoryVariance"] is null),
        "missing physical flow must not publish product-family inventory summaries");
    AssertTrue(dashboard["details"]!.AsArray()
            .SelectMany(item => item!["bufferSummaries"]!.AsArray())
            .All(item => item!["averageInventoryValue"] is null),
        "missing physical flow must not publish NFP value in product-family buffer summaries");
    AssertTrue(dashboard["comparison"]!["averageInventoryValueDelta"] is null &&
        dashboard["comparison"]!["budgetInventoryVarianceDelta"] is null,
        "missing physical flow must not publish product-family inventory deltas");
    var comparison = JsonSerializer.SerializeToNode(
        result.Comparison,
        new JsonSerializerOptions(JsonSerializerDefaults.Web))!.AsObject();
    AssertTrue(comparison["averageInventoryValueDelta"] is null,
        "missing physical flow must not publish a scenario inventory delta");

    var completeResult = new ScenarioRunPreviewService(
            new StaticScenarioWorkspaceDataSource(BuildCompletePlanningEvidenceData()))
        .Preview(new ScenarioRunPreviewRequest(6));
    var completeFlow = completeResult.Scenario.InventoryFlow
        ?? throw new InvalidOperationException("complete persistence guard fixture should expose physical flow");
    AssertPersistencePhysicalGuardRejects(
        completeResult with
        {
            Scenario = completeResult.Scenario with
            {
                InventoryFlow = completeFlow with { Points = completeFlow.Points.Skip(1).ToList() }
            }
        },
        "partial SKU-week coverage");
    AssertPersistencePhysicalGuardRejects(
        completeResult with
        {
            Scenario = completeResult.Scenario with
            {
                InventoryFlow = completeFlow with
                {
                    Points = completeFlow.Points.Concat(new[] { completeFlow.Points[0] }).ToList()
                }
            }
        },
        "duplicate SKU-week coverage");
    AssertPersistencePhysicalGuardRejects(
        completeResult with
        {
            Scenario = completeResult.Scenario with
            {
                InventoryFlow = completeFlow with { CaseId = "cross-case" }
            }
        },
        "cross-case physical flow");

    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-incomplete-physical-save-{Guid.NewGuid():N}.db");
    try
    {
        var persistence = new ScenarioRunPersistenceService(previewService, databasePath);
        var blocked = false;
        try
        {
            persistence.Save(new ScenarioRunSaveRequest(
                "缺失物理库存证据",
                null,
                "planner",
                new ScenarioRunPreviewRequest(6)));
        }
        catch (InvalidOperationException error)
        {
            blocked = error.Message.Contains("物理库存投影证据不完整", StringComparison.Ordinal);
        }
        AssertTrue(blocked, "missing physical inventory evidence must block scenario persistence explicitly");
        AssertEqual(0, persistence.List(10).Count, "blocked incomplete scenario must not create a run row");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void AssertPersistencePhysicalGuardRejects(ScenarioRunPreviewResult result, string caseName)
{
    var guard = typeof(ScenarioRunPersistenceService).GetMethod(
        "EnsurePhysicalInventoryEvidence",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
        ?? throw new InvalidOperationException("physical persistence guard should exist");
    var blocked = false;
    try
    {
        guard.Invoke(null, new object[] { result });
    }
    catch (System.Reflection.TargetInvocationException error)
        when (error.InnerException is InvalidOperationException inner &&
              inner.Message.Contains("物理库存投影证据不完整", StringComparison.Ordinal))
    {
        blocked = true;
    }
    AssertTrue(blocked, $"{caseName} must be rejected by the authoritative persistence guard");
}

static void AssertBufferTrendPhysicalEvidenceMissing(BufferTrendWorkspaceResult trend, string caseName)
{
    AssertTrue(trend.Series.All(item => item.PhysicalPosition is null),
        $"{caseName}: no physical on-hand position may survive an incomplete evidence envelope");
    AssertTrue(trend.Series.All(item => item.InventoryValue is null),
        $"{caseName}: inventory value must not fall back to net-flow value");
    AssertTrue(trend.WeeklyCells.All(item => item.InventoryValue is null),
        $"{caseName}: weekly physical inventory values must remain missing");
    AssertTrue(trend.FamilySummaries.All(item => item.AverageInventoryValue is null),
        $"{caseName}: family physical inventory summaries must remain missing");
    AssertTrue(trend.Kpis.AverageInventoryValue is null &&
        trend.Kpis.PeakInventoryValue is null &&
        trend.Kpis.InventoryValueDelta is null &&
        trend.Kpis.OnHandRedSkuCount is null &&
        trend.Kpis.OnHandYellowSkuCount is null &&
        trend.Kpis.OnHandStockoutWeekCount is null,
        $"{caseName}: aggregate physical KPIs must remain missing");
    AssertEqual("EvidenceMissing", trend.Comparison.PhysicalDeltaEvidenceStatus ?? string.Empty,
        $"{caseName}: physical comparison evidence status");
    AssertTrue(trend.Comparison.AverageInventoryValueDelta is null &&
        trend.Comparison.PeakInventoryValueDelta is null &&
        trend.Comparison.PhysicalAverageInventoryValueDelta is null &&
        trend.Comparison.PhysicalPeakInventoryValueDelta is null,
        $"{caseName}: physical inventory deltas must remain missing");
}

static void TestBufferSignalShowsNfpRecoveryBeforePhysicalReceipt()
{
    var (data, sku) = BuildInventoryFlowFixture(
        new[] { 5m, 5m, 0m, 0m, 0m },
        openingOnHand: 0m,
        openingBacklog: 0m,
        dltDays: 21);
    var sizing = DdmrpCalculator.CalculateSizing(sku);
    var projections = Enumerable.Range(1, 5)
        .Select(week => new BufferProjectionPoint(
            sku.Sku, week, 0m, 5m, 0m, sizing.Zones.TopOfGreen,
            "Green", sizing))
        .ToList();
    var orders = new[] { new ProjectedReplenishmentOrder(sku.Sku, 1, sizing.Zones.TopOfGreen, 0m, "TopOfGreen") };
    var plan = new DemandDrivenPlanResult(projections, orders, Array.Empty<CapacityLoadProjection>(), Array.Empty<ProjectedSupplyRequirement>(), Array.Empty<PlanningTrace>());
    var flow = InventoryFlowProjectionService.Project(data, "long-dlt", new[] { sku }, data.Demand, orders,
        Array.Empty<PrebuildCampaign>(), Array.Empty<SupplierCapacityLimit>());
    var trend = BufferTrendWorkspaceService.Build(data, "long-dlt", "long DLT", new[] { sku }, plan, flow);

    var receiptArrivalWeek = flow.ReceiptLog
        .Where(item => item.SourceKind == "SimulatedReplenishment" && item.RecommendationWeek == 1)
        .Select(item => item.ArrivalWeek)
        .Min();
    AssertTrue(trend.Series.Any(point =>
            point.Week < receiptArrivalWeek &&
            point.EndNetFlowAfterReplenishment >= point.TopOfYellow &&
            (point.PhysicalPosition!.OnHandStatus == "Red" || point.PhysicalPosition.EndingBacklog > 0m)),
        "NFP may recover after order release while physical stock remains red or backlogged before the long-DLT receipt");
}

static void TestLegacyPreviewKeepsLegacyReference()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-legacy-physical-preview-{Guid.NewGuid():N}.db");
    try
    {
        var previewService = new ScenarioRunPreviewService(new StaticScenarioWorkspaceDataSource(BuildCompletePlanningEvidenceData()));
        var persistence = new ScenarioRunPersistenceService(previewService, databasePath);
        var saved = persistence.Save(new ScenarioRunSaveRequest(
            "legacy physical preview",
            "remove optional physical result fields",
            "planner",
            new ScenarioRunPreviewRequest(6)));
        var before = persistence.GetDetail(saved.RunId) ?? throw new InvalidOperationException("saved preview should be readable");
        var legacyAverageInventory = before.Result.Scenario.Metrics.AverageInventoryValue;
        var legacyService = before.Result.Scenario.Metrics.ServiceLevelPercent;

        string resultJson;
        using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}"))
        {
            connection.Open();
            using (var read = connection.CreateCommand())
            {
                read.CommandText = "SELECT result_json FROM scenario_runs WHERE run_id = $run_id;";
                read.Parameters.AddWithValue("$run_id", saved.RunId);
                resultJson = (string?)read.ExecuteScalar() ?? throw new InvalidOperationException("result_json should exist");
            }

            var root = JsonNode.Parse(resultJson)?.AsObject()
                ?? throw new InvalidOperationException("result_json should be valid JSON");
            foreach (var caseName in new[] { "baseline", "scenario" })
            {
                var previewCase = root[caseName]!.AsObject();
                previewCase.Remove("inventoryFlow");
                previewCase.Remove("scenarioMetricEvidence");
                RemovePhysicalComparisonFields(previewCase["productFamilyDashboard"]!["comparison"]!.AsObject());
                RemovePhysicalComparisonFields(previewCase["bufferTrend"]!["comparison"]!.AsObject());
            }
            RemovePhysicalComparisonFields(root["comparison"]!.AsObject());

            using var update = connection.CreateCommand();
            update.CommandText = "UPDATE scenario_runs SET result_json = $result_json WHERE run_id = $run_id;";
            update.Parameters.AddWithValue("$result_json", root.ToJsonString());
            update.Parameters.AddWithValue("$run_id", saved.RunId);
            update.ExecuteNonQuery();
        }

        var legacy = persistence.GetDetail(saved.RunId) ?? throw new InvalidOperationException("legacy preview should be readable");
        AssertTrue(legacyAverageInventory.HasValue,
            "complete saved setup should start with a physical inventory amount");
        AssertTrue(legacy.Result.Scenario.Metrics.AverageInventoryValue is null,
            "legacy results without physical flow evidence must omit the old compatibility inventory amount");
        AssertEqual(legacyService, legacy.Result.Scenario.Metrics.ServiceLevelPercent,
            "legacy compatibility service value should be retained");
        foreach (var previewCase in new[] { legacy.Result.Baseline, legacy.Result.Scenario })
        {
            AssertEqual("EvidenceMissing", previewCase.InventoryFlow?.Status ?? string.Empty,
                $"{previewCase.CaseId} legacy inventory flow status");
            var evidence = previewCase.ScenarioMetricEvidence ?? throw new InvalidOperationException("legacy evidence labels should be present");
            foreach (var path in RequiredPhysicalScenarioMetricPaths())
            {
                AssertTrue(evidence.Any(item =>
                        item.JsonPath == path &&
                        item.EvidenceStatus == "EvidenceMissing" &&
                        item.Source == "LegacyReference"),
                    $"legacy evidence should label {path}");
            }
            AssertTrue(!evidence.Any(item => item.Source == "PhysicalProjection"),
                "legacy compatibility values must not be presented as new physical facts");
            AssertTrue(previewCase.Budget.All(item =>
                    item.ProjectedInventoryValue is null && item.BudgetInventoryVariance is null),
                "legacy results without physical flow must omit budget inventory amounts");
            AssertTrue(previewCase.ProductFamilyDashboard.Summaries.All(item =>
                    item.AverageInventoryValue is null && item.PeakInventoryValue is null),
                "legacy results without physical flow must omit product-family inventory amounts");
            AssertBufferTrendPhysicalEvidenceMissing(previewCase.BufferTrend, $"{previewCase.CaseId} legacy flow");
        }
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestComparisonOmitsIncompletePhysicalDelta()
{
    var incomplete = BuildCompletePlanningEvidenceData() with
    {
        ConfirmedReceipts = null,
        OpeningBacklog = null,
        PlanningEvidenceCoverage = null
    };
    var result = new ScenarioRunPreviewService(new StaticScenarioWorkspaceDataSource(incomplete))
        .Preview(new ScenarioRunPreviewRequest(6));

    AssertEqual("EvidenceMissing", result.Baseline.InventoryFlow?.Status ?? string.Empty, "incomplete baseline physical status");
    AssertEqual("EvidenceMissing", result.Scenario.InventoryFlow?.Status ?? string.Empty, "incomplete scenario physical status");
    AssertTrue(result.Scenario.Metrics.AverageInventoryValue is null,
        "incomplete physical flow must not publish a compatibility inventory amount");
    var evidence = result.Scenario.ScenarioMetricEvidence
        ?? throw new InvalidOperationException("incomplete physical preview should expose metric evidence");
    var serviceEvidence = evidence.Single(item => item.JsonPath == "metrics.serviceLevelPercent");
    AssertEqual("HistoricalReference", serviceEvidence.Source,
        "service fallback should be labeled as a historical reference");
    AssertTrue(serviceEvidence.Explanation.Contains("historical", StringComparison.OrdinalIgnoreCase),
        "service fallback explanation should identify its historical basis");
    var inventoryEvidence = evidence.First(item => item.JsonPath == "metrics.averageInventoryValue");
    AssertEqual("MissingEvidence", inventoryEvidence.Source,
        "missing inventory amount should be labeled as missing physical evidence");
    AssertTrue(inventoryEvidence.Explanation.Contains("omitted", StringComparison.OrdinalIgnoreCase),
        "missing inventory explanation should state that the amount is omitted");

    var json = JsonSerializer.SerializeToNode(result, new JsonSerializerOptions(JsonSerializerDefaults.Web))!.AsObject();
    AssertMissingPhysicalDelta(
        json["comparison"]!.AsObject(),
        "physicalServiceLevelDelta",
        "physicalAverageInventoryValueDelta");
    var scenario = json["scenario"]!.AsObject();
    AssertMissingPhysicalDelta(
        scenario["productFamilyDashboard"]!["comparison"]!.AsObject(),
        "physicalServiceLevelDelta",
        "physicalAverageInventoryValueDelta",
        "physicalBudgetInventoryVarianceDelta");
    AssertMissingPhysicalDelta(
        scenario["bufferTrend"]!["comparison"]!.AsObject(),
        "physicalAverageInventoryValueDelta",
        "physicalPeakInventoryValueDelta");

    var poisoned = result with
    {
        Comparison = result.Comparison with
        {
            PhysicalServiceLevelDelta = 12m,
            PhysicalDeltaEvidenceStatus = "Complete"
        },
        Baseline = result.Baseline with
        {
            ProductFamilyDashboard = result.Baseline.ProductFamilyDashboard with
            {
                Comparison = result.Baseline.ProductFamilyDashboard.Comparison with
                {
                    PhysicalServiceLevelDelta = 12m,
                    PhysicalDeltaEvidenceStatus = "Complete"
                }
            }
        },
        Scenario = result.Scenario with
        {
            ProductFamilyDashboard = result.Scenario.ProductFamilyDashboard with
            {
                Comparison = result.Scenario.ProductFamilyDashboard.Comparison with
                {
                    PhysicalServiceLevelDelta = 12m,
                    PhysicalDeltaEvidenceStatus = "Complete"
                }
            }
        }
    };
    var restore = typeof(ScenarioRunPreviewService).GetMethod(
        "RestoreLegacyInventoryEvidence",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
        ?? throw new InvalidOperationException("legacy inventory evidence restoration should exist");
    var restored = (ScenarioRunPreviewResult?)restore.Invoke(null, new object?[] { poisoned, null })
        ?? throw new InvalidOperationException("legacy inventory evidence restoration should return a result");
    AssertTrue(restored.Comparison.PhysicalServiceLevelDelta is null,
        "incomplete restored comparison must clear a stale physical service delta");
    AssertTrue(restored.Baseline.ProductFamilyDashboard.Comparison.PhysicalServiceLevelDelta is null &&
        restored.Scenario.ProductFamilyDashboard.Comparison.PhysicalServiceLevelDelta is null,
        "incomplete restored product-family comparisons must clear stale physical service deltas");
}

static void TestFrozenComparisonPreservesBaselineLineageAndEvidence()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-frozen-physical-lineage-{Guid.NewGuid():N}.db");
    try
    {
        var seed = SeedData.Create();
        var baselineService = new CurrentBaselineService(new SeedCurrentBaselineDataSource(seed), databasePath);
        var frozen = baselineService.Freeze(new CurrentBaselineFreezeRequest("planner", "physical lineage"));
        var source = new TrackingScenarioWorkspaceDataSource(seed);
        var previewService = new ScenarioRunPreviewService(source);
        var assumptions = new SeedScenarioAssumptionSource();
        var comparison = new ScenarioComparisonService(baselineService, previewService, assumptions).Compare(
            new ScenarioComparisonRequest(
                frozen.SnapshotId,
                assumptions.GetTemplates().First().ExternalScenario,
                Array.Empty<ResponseConfiguration>(),
                6));

        var frozenPlanning = frozen.Payload.PlanningInputs
            ?? throw new InvalidOperationException("frozen planning inputs should be present");
        AssertTrue(frozenPlanning.Demand.GroupBy(item => item.Sku).All(group => group.Count() == 52),
            "immutable frozen payload should retain all 52 demand weeks");
        AssertTrue(frozenPlanning.PlanningEvidenceCoverage is { CoverageFromWeek: 1, CoverageThroughWeek: 52 },
            "immutable frozen payload should retain 1..52 coverage");
        AssertEqual(0, source.LoadCount, "frozen comparison must not reload live planning inputs");

        var frozenBaselineCase = comparison.NoResponse.Preview.Baseline;
        var legacyCommitmentWindows = frozen.Payload.SupplierCommitments
            .SelectMany(commitment => Enumerable.Range(1, 6)
                .Select(week => new SupplierCapacityWindow(
                    commitment.Supplier,
                    commitment.MaterialFamily,
                    week,
                    commitment.Quantity,
                    commitment.LeadTimeDays,
                    commitment.RiskStatus)))
            .ToList();
        var expectedLegacySupply = ConstraintWorkspaceService.CompareSupplierCapacity(
            legacyCommitmentWindows,
            frozenBaselineCase.Plan.SupplyRequirements,
            Array.Empty<SupplierCapacityLimit>());
        AssertEqual(
            JsonSerializer.Serialize(expectedLegacySupply),
            JsonSerializer.Serialize(frozenBaselineCase.SupplierCapacity),
            "physical supplier windows must not change frozen legacy supply calculations");

        foreach (var previewCase in comparison.AllCases.SelectMany(item => new[] { item.Preview.Baseline, item.Preview.Scenario }))
        {
            var flow = previewCase.InventoryFlow ?? throw new InvalidOperationException("frozen comparison should expose physical flow");
            AssertEqual(
                "Complete",
                flow.Status,
                $"{previewCase.CaseId} frozen flow status; issues={string.Join(",", flow.Issues.Select(item => $"{item.Scope}:{item.Sku}:{item.Week}:{item.Reason}"))}");
            AssertEqual(frozen.SnapshotId, flow.BaselineSnapshotId!, $"{previewCase.CaseId} physical baseline lineage");
            AssertTrue(flow.Points.All(item => item.Week <= 6), "active comparison points should respect the requested horizon");
            var frozenReceiptIds = flow.ReceiptLog
                .Where(item => item.EvidenceSource == "FrozenBaseline")
                .Select(item => item.SourceId)
                .ToHashSet(StringComparer.Ordinal);
            AssertTrue((frozenPlanning.ConfirmedReceipts ?? Array.Empty<ConfirmedReceiptEvidence>())
                    .All(item => frozenReceiptIds.Contains(item.ReceiptId)),
                "projection receipt log should retain every frozen confirmed receipt, including outside the active horizon");
            AssertTrue(previewCase.ScenarioMetricEvidence?.All(item =>
                    item.BaselineSnapshotId == frozen.SnapshotId &&
                    item.ProjectionCaseId == previewCase.CaseId &&
                    item.Source == "PhysicalProjection") == true,
                "frozen metric evidence should retain baseline and projection lineage");
        }
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static void TestInventoryFlowResultJsonRoundTrips()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-physical-result-json-{Guid.NewGuid():N}.db");
    try
    {
        var previewService = new ScenarioRunPreviewService(new StaticScenarioWorkspaceDataSource(BuildCompletePlanningEvidenceData()));
        var persistence = new ScenarioRunPersistenceService(previewService, databasePath);
        var request = new ScenarioRunPreviewRequest(
            6,
            Parameters: new ScenarioRunParameterSet(
                PrebuildCampaigns: new[] { new PrebuildCampaign("PB-ROUNDTRIP", "SAT-BUS-001", 1, 2, 3, 2m) }));
        var expected = previewService.Preview(request);
        var saved = persistence.Save(new ScenarioRunSaveRequest("physical JSON", "round trip", "planner", request));
        var detail = persistence.GetDetail(saved.RunId) ?? throw new InvalidOperationException("saved physical preview should be readable");

        AssertEqual(
            JsonSerializer.Serialize(expected.Baseline.InventoryFlow),
            JsonSerializer.Serialize(detail.Result.Baseline.InventoryFlow),
            "baseline inventory flow result_json round trip");
        AssertEqual(
            JsonSerializer.Serialize(expected.Scenario.InventoryFlow),
            JsonSerializer.Serialize(detail.Result.Scenario.InventoryFlow),
            "scenario inventory flow result_json round trip");
        AssertEqual(
            JsonSerializer.Serialize(expected.Scenario.ScenarioMetricEvidence),
            JsonSerializer.Serialize(detail.Result.Scenario.ScenarioMetricEvidence),
            "scenario metric evidence result_json round trip");

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={databasePath}");
        connection.Open();
        using (var read = connection.CreateCommand())
        {
            read.CommandText = "SELECT result_json FROM scenario_runs WHERE run_id = $run_id;";
            read.Parameters.AddWithValue("$run_id", saved.RunId);
            var root = JsonNode.Parse((string?)read.ExecuteScalar() ?? string.Empty)?.AsObject()
                ?? throw new InvalidOperationException("persisted result_json should be valid");
            AssertTrue(root["scenario"]?["inventoryFlow"] is JsonObject,
                "optional inventory flow should be written only inside result_json");
            AssertTrue(root["scenario"]?["scenarioMetricEvidence"] is JsonArray,
                "path-addressed metric evidence should be written only inside result_json");
        }
        using (var schema = connection.CreateCommand())
        {
            schema.CommandText = "PRAGMA table_info(scenario_runs);";
            using var reader = schema.ExecuteReader();
            var columns = new List<string>();
            while (reader.Read())
            {
                columns.Add(reader.GetString(1));
            }
            AssertTrue(!columns.Any(item => item.Contains("inventory_flow", StringComparison.OrdinalIgnoreCase)),
                "physical result persistence must not add an inventory-flow column or migration");
        }
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        DeleteSqliteFiles(databasePath);
    }
}

static IReadOnlyList<string> RequiredPhysicalScenarioMetricPaths() => new[]
{
    "metrics.serviceLevelPercent",
    "metrics.averageInventoryValue",
    "budget[*].projectedInventoryValue",
    "budget[*].budgetInventoryVariance",
    "productFamilyDashboard.summaries[*].serviceLevelPercent",
    "productFamilyDashboard.summaries[*].averageInventoryValue",
    "productFamilyDashboard.summaries[*].peakInventoryValue",
    "productFamilyDashboard.summaries[*].budgetInventoryVariance",
    "productFamilyDashboard.details[*].weeklyCells[*].inventoryValue",
    "productFamilyDashboard.details[*].weeklyCells[*].budgetInventoryVariance",
    "productFamilyDashboard.weeklyCells[*].inventoryValue",
    "productFamilyDashboard.weeklyCells[*].budgetInventoryVariance",
    "bufferTrend.kpis.averageInventoryValue",
    "bufferTrend.kpis.peakInventoryValue",
    "bufferTrend.kpis.inventoryValueDelta",
    "bufferTrend.series[*].inventoryValue",
    "bufferTrend.familySummaries[*].averageInventoryValue",
    "bufferTrend.weeklyCells[*].inventoryValue"
};

static void RemovePhysicalComparisonFields(JsonObject comparison)
{
    foreach (var field in new[]
    {
        "physicalServiceLevelDelta",
        "physicalAverageInventoryValueDelta",
        "physicalPeakInventoryValueDelta",
        "physicalBudgetInventoryVarianceDelta",
        "physicalDeltaEvidenceStatus",
        "physicalDeltaExplanation"
    })
    {
        comparison.Remove(field);
    }
}

static void AssertMissingPhysicalDelta(JsonObject comparison, params string[] fields)
{
    foreach (var field in fields)
    {
        AssertTrue(comparison.ContainsKey(field), $"comparison should expose optional {field}");
        AssertTrue(comparison[field] is null, $"{field} should be null when either physical projection is incomplete");
    }
    AssertEqual("EvidenceMissing", comparison["physicalDeltaEvidenceStatus"]?.GetValue<string>() ?? string.Empty,
        "physical delta evidence status");
    AssertTrue(!string.IsNullOrWhiteSpace(comparison["physicalDeltaExplanation"]?.GetValue<string>()),
        "missing physical delta should carry an explanation");
}

static InventoryFlowProjectionResult ProjectInventoryFlow(ScenarioWorkspaceDataSet data, SkuBufferSetting sku) =>
    InventoryFlowProjectionService.Project(
        data,
        "inventory-flow-test",
        new[] { sku },
        data.Demand,
        Array.Empty<ProjectedReplenishmentOrder>(),
        Array.Empty<PrebuildCampaign>(),
        Array.Empty<SupplierCapacityLimit>());

static (ScenarioWorkspaceDataSet Data, SkuBufferSetting Sku) BuildInventoryFlowFixture(
    IReadOnlyList<decimal> weeklyDemand,
    decimal openingOnHand,
    decimal openingBacklog,
    int dltDays = 7,
    decimal unitCost = 10m,
    decimal qualifiedDemand = 0m,
    IReadOnlyList<(string Id, int Week, decimal Quantity, string ReceiptType)>? frozenReceipts = null)
{
    var source = BuildCompletePlanningEvidenceData();
    var anchor = source.Request.AnchorDate;
    var sku = source.Skus.Single() with
    {
        DecoupledLeadTimeDays = dltDays,
        UnitCost = unitCost
    };
    var sizing = DdmrpCalculator.CalculateSizing(sku);
    var receipts = (frozenReceipts ?? Array.Empty<(string Id, int Week, decimal Quantity, string ReceiptType)>())
        .Select(item => new ConfirmedReceiptEvidence(
            item.Id,
            sku.Sku,
            item.Quantity,
            item.Week,
            anchor.AddDays(7 * (item.Week - 1) + 1),
            item.ReceiptType,
            $"PO-{item.Id}",
            "PurchaseOrder",
            source.SupplierItemSources.Single().Supplier,
            source.SupplierItemSources.Single().MaterialFamily,
            "Confirmed",
            "Complete",
            "2026-06-01T00:00:00Z",
            "test frozen receipt"))
        .ToList();
    var demand = weeklyDemand
        .Select((quantity, index) => new WeeklyDemand(sku.Sku, index + 1, quantity))
        .ToList();

    return (source with
    {
        Request = new ScenarioWorkspaceDataRequest(weeklyDemand.Count, anchor, SkuFilter: new[] { sku.Sku }),
        Skus = new[] { sku },
        Inventory = new[] { new InventoryPosition(sku.Sku, openingOnHand, receipts.Sum(item => item.Quantity), qualifiedDemand) },
        Demand = demand,
        DdmrpParameters = source.DdmrpParameters
            .Select(item => item with
            {
                DecoupledLeadTimeDays = dltDays,
                UnitCost = unitCost,
                TopOfRed = sizing.Zones.TopOfRed,
                TopOfYellow = sizing.Zones.TopOfYellow,
                TopOfGreen = sizing.Zones.TopOfGreen,
                Sizing = sizing,
                SizingLines = DdmrpSizingExplanation.Build(sizing)
            })
            .ToList(),
        ConfirmedReceipts = receipts,
        OpeningBacklog = new[]
        {
            new OpeningBacklogEvidence(
                "BACKLOG-FLOW",
                sku.Sku,
                openingBacklog,
                "ORDER-FLOW",
                "Complete",
                "2026-06-01T00:00:00Z",
                "test opening backlog")
        }
    }, sku);
}

static (ScenarioWorkspaceDataSet Data, IReadOnlyList<SkuBufferSetting> Skus) BuildSharedSupplierInventoryFlowFixture(
    int skuCount,
    int horizonWeeks,
    decimal weeklyCapacity,
    string riskStatus = "Green")
{
    var (template, firstSku) = BuildInventoryFlowFixture(
        Enumerable.Repeat(0m, horizonWeeks).ToList(),
        openingOnHand: 0m,
        openingBacklog: 0m,
        dltDays: 7);
    var skus = Enumerable.Range(0, skuCount)
        .Select(index => index == 0
            ? firstSku
            : firstSku with
            {
                Sku = $"{firstSku.Sku}-{(char)('A' + index)}",
                Name = $"{firstSku.Name} {index + 1}"
            })
        .ToList();
    var source = template.SupplierItemSources.Single();
    var parameter = template.DdmrpParameters.Single();
    var backlog = template.OpeningBacklog!.Single();

    return (template with
    {
        Request = template.Request with { SkuFilter = skus.Select(item => item.Sku).ToList() },
        Skus = skus,
        Inventory = skus
            .Select(item => new InventoryPosition(item.Sku, 0m, 0m, 0m))
            .ToList(),
        Demand = skus
            .SelectMany(item => Enumerable.Range(1, horizonWeeks)
                .Select(week => new WeeklyDemand(item.Sku, week, 0m)))
            .ToList(),
        SupplierItemSources = skus
            .Select(item => new SupplierItemSource(source.Supplier, item.Sku, source.MaterialFamily, source.UnitCost))
            .ToList(),
        SupplierCapacityWindows = Enumerable.Range(1, horizonWeeks)
            .Select(week => new SupplierCapacityWindow(
                source.Supplier,
                source.MaterialFamily,
                week,
                weeklyCapacity,
                7,
                riskStatus))
            .ToList(),
        DdmrpParameters = skus
            .Select(item => parameter with { Sku = item.Sku, Name = item.Name })
            .ToList(),
        ConfirmedReceipts = Array.Empty<ConfirmedReceiptEvidence>(),
        OpeningBacklog = skus
            .Select(item => backlog with
            {
                BacklogId = $"BACKLOG-{item.Sku}",
                Sku = item.Sku,
                Quantity = 0m,
                SourceReference = $"ORDER-{item.Sku}"
            })
            .ToList()
    }, skus);
}

static void TestScenarioFeasibilityPolicyEnforcesSharedHardLimitsAndThresholdBoundaries()
{
    var (data, preview) = CreateScenarioFeasibilityFixture();
    var deepRed = preview with
    {
        Scenario = preview.Scenario with
        {
            Metrics = preview.Scenario.Metrics with { PeakLoadPercent = 100.1m }
        }
    };

    foreach (var mode in new[] { "Balanced", "ServiceFirst", "FlowFirst", "CashFirst", "CapacityFirst", "SupplyFirst" })
    {
        var assessment = ScenarioFeasibilityPolicy.Evaluate(deepRed with
        {
            Request = deepRed.Request with { AdoptionConstraintMode = mode }
        }, data);
        AssertTrue(assessment.IsBlocked, $"{mode} must not bypass a hard red line");
        AssertEqual("Blocked", assessment.Status, $"{mode} hard-red status");
    }

    var balanced = ScenarioFeasibilityPolicy.Evaluate(deepRed with
    {
        Request = deepRed.Request with { AdoptionConstraintMode = "Balanced" }
    }, data);
    var supplyFirst = ScenarioFeasibilityPolicy.Evaluate(deepRed with
    {
        Request = deepRed.Request with { AdoptionConstraintMode = "SupplyFirst" }
    }, data);
    AssertEqual(balanced.IsBlocked, supplyFirst.IsBlocked, "priority mode must not change a hard result");
    AssertTrue(balanced.Checks[0].Code != supplyFirst.Checks[0].Code,
        "priority mode should only change the review ordering");

    var capacityAtLimit = ScenarioFeasibilityPolicy.Evaluate(preview with
    {
        Scenario = preview.Scenario with { Metrics = preview.Scenario.Metrics with { PeakLoadPercent = 100m } }
    }, data);
    var capacityOverLimit = ScenarioFeasibilityPolicy.Evaluate(deepRed, data);
    AssertTrue(capacityAtLimit.Checks.Single(item => item.Code == "Capacity").Status != "Red",
        "peak load 100 must not be red");
    AssertEqual("Red", capacityOverLimit.Checks.Single(item => item.Code == "Capacity").Status,
        "peak load 100.1 must be red");

    var supplyAtLimit = ScenarioFeasibilityPolicy.Evaluate(WithSupplyGap(preview, 15m), data);
    var supplyOverLimit = ScenarioFeasibilityPolicy.Evaluate(WithSupplyGap(preview, 15.1m), data);
    AssertTrue(supplyAtLimit.Checks.Single(item => item.Code == "Supply").Status != "Red",
        "supply gap ratio 15% must not be red");
    AssertEqual("Red", supplyOverLimit.Checks.Single(item => item.Code == "Supply").Status,
        "supply gap ratio above 15% must be red");

    var baselineInventory = preview.Baseline.Metrics.AverageInventoryValue!.Value;
    var inventoryAtLimit = ScenarioFeasibilityPolicy.Evaluate(preview with
    {
        Scenario = preview.Scenario with
        {
            Metrics = preview.Scenario.Metrics with { AverageInventoryValue = baselineInventory * 1.12m }
        }
    }, data);
    var inventoryOverLimit = ScenarioFeasibilityPolicy.Evaluate(preview with
    {
        Scenario = preview.Scenario with
        {
            Metrics = preview.Scenario.Metrics with { AverageInventoryValue = baselineInventory * 1.121m }
        }
    }, data);
    AssertTrue(inventoryAtLimit.Checks.Single(item => item.Code == "Inventory").Status != "Red",
        "inventory increase 12% must not be red");
    AssertEqual("Red", inventoryOverLimit.Checks.Single(item => item.Code == "Inventory").Status,
        "inventory increase above 12% must be red");

    var threeRedWeeks = ScenarioFeasibilityPolicy.Evaluate(WithConsecutiveRedWeeks(preview, 3), data);
    var fourRedWeeks = ScenarioFeasibilityPolicy.Evaluate(WithConsecutiveRedWeeks(preview, 4), data);
    AssertTrue(threeRedWeeks.Checks.Single(item => item.Code == "RedDuration").Status != "Red",
        "three consecutive red weeks must not be red by duration alone");
    AssertEqual("Red", fourRedWeeks.Checks.Single(item => item.Code == "RedDuration").Status,
        "four consecutive red weeks must be red");
}

static void TestScenarioFeasibilityPolicyBlocksMissingEvidenceAndAttachesToPreview()
{
    var (data, preview) = CreateScenarioFeasibilityFixture();
    var incomplete = preview with
    {
        Scenario = preview.Scenario with
        {
            InventoryFlow = preview.Scenario.InventoryFlow! with { Status = "EvidenceMissing" }
        }
    };
    var assessment = ScenarioFeasibilityPolicy.Evaluate(incomplete, data);
    AssertTrue(assessment.IsBlocked, "missing physical inventory evidence must be blocked");
    AssertEqual("Red", assessment.Checks.Single(item => item.Code == "Evidence").Status,
        "missing evidence must be a red check");

    AssertTrue(preview.Feasibility is not null, "every backend preview must carry the feasibility assessment");
    AssertEqual(preview.Feasibility!.Status, ScenarioFeasibilityPolicy.Evaluate(preview, data).Status,
        "preview feasibility must come from the common backend policy");
}

static void TestBlockedEvidenceCompleteScenarioCanBeSavedWithoutApproval()
{
    var databasePath = Path.Combine(Path.GetTempPath(), $"ddae-feasibility-{Guid.NewGuid():N}.db");
    try
    {
        var previewService = new ScenarioRunPreviewService(new SeedScenarioWorkspaceDataSource(SeedData.Create()));
        var persistence = new ScenarioRunPersistenceService(previewService, databasePath);
        var request = new ScenarioRunPreviewRequest(12, "TPL-CONSTRAINED", AdoptionConstraintMode: "SupplyFirst");
        var preview = previewService.Preview(request);

        AssertTrue(preview.Feasibility is { IsBlocked: true },
            "the constrained evidence-complete preview must be blocked by feasibility without becoming unsaveable");
        var saved = persistence.Save(new ScenarioRunSaveRequest("阻断候选保存", "验证阻断候选仍仅保存", "计划员", request));

        AssertEqual("Saved", saved.Status, "blocked candidate save status");
        AssertEqual("NotSubmitted", saved.ApprovalStatus, "blocked candidate must not be submitted or approved");
        AssertEqual("Blocked", saved.Summary.FeasibilityStatus, "saved summary feasibility status");
        AssertTrue(persistence.GetDetail(saved.RunId)!.Result.Feasibility is { IsBlocked: true },
            "persisted preview must retain the backend assessment");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            var path = $"{databasePath}{suffix}";
            if (File.Exists(path)) File.Delete(path);
        }
    }
}

static (ScenarioWorkspaceDataSet Data, ScenarioRunPreviewResult Preview) CreateScenarioFeasibilityFixture()
{
    var source = new SeedScenarioWorkspaceDataSource(SeedData.Create());
    var request = new ScenarioRunPreviewRequest(12);
    var data = source.Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1)));
    var preview = new ScenarioRunPreviewService(source).Preview(request);
    return (data, preview);
}

static ScenarioRunPreviewResult WithSupplyGap(ScenarioRunPreviewResult preview, decimal gapPercent) => preview with
{
    Scenario = preview.Scenario with
    {
        SupplierCapacity = new[]
        {
            new SupplierCapacityComparison("test supplier", "test material", 1, 100m, 100m - gapPercent, gapPercent, "Test")
        }
    }
};

static ScenarioRunPreviewResult WithConsecutiveRedWeeks(ScenarioRunPreviewResult preview, int redWeeks)
{
    var sku = preview.Scenario.Plan.BufferProjections[0].Sku;
    var plan = preview.Scenario.Plan with
    {
        BufferProjections = preview.Scenario.Plan.BufferProjections
            .Select(item => item with
            {
                BufferStatus = item.Sku == sku && item.Week <= redWeeks ? "Red" : "Green"
            })
            .ToList()
    };
    return preview with { Scenario = preview.Scenario with { Plan = plan } };
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

static void TestFutureTimeBufferEvidenceIsConsolidatedIntoBreachAnalysis()
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var page = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml"));
    var script = File.ReadAllText(Path.Combine(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js"));
    AssertTrue(!page.Contains("href=\"#future-scenario-panel/time-buffer\"", StringComparison.Ordinal), "future navigation should not expose an independent time-buffer route");
    AssertTrue(!page.Contains("id=\"time-buffer-panel\"", StringComparison.Ordinal), "future time-buffer page shell should be removed");
    AssertTrue(!script.Contains("\"#future-scenario-panel/time-buffer\"", StringComparison.Ordinal), "workspace registry should not expose an independent time-buffer route");

    var varianceStart = page.IndexOf("id=\"variance-panel\"", StringComparison.Ordinal);
    var futureBreachBody = page.IndexOf("id=\"future-breach-body\"", varianceStart, StringComparison.Ordinal);
    var exceptionKpis = page.IndexOf("id=\"exception-kpis\"", futureBreachBody, StringComparison.Ordinal);
    AssertTrue(varianceStart >= 0 && futureBreachBody > varianceStart && exceptionKpis > futureBreachBody, "breach-analysis page should contain its breach table and exception workspace");
    var consolidatedHost = page.Substring(futureBreachBody, exceptionKpis - futureBreachBody);
    foreach (var id in new[] { "time-buffer-breach-detail", "time-buffer-breach-select", "time-buffer-breach-evidence-chip", "time-buffer-breach-summary", "time-buffer-breach-weekly-grid" })
    {
        AssertTrue(consolidatedHost.Contains($"id=\"{id}\"", StringComparison.Ordinal), $"breach analysis should embed time-buffer evidence host {id}");
    }

    AssertTrue(!script.Contains("function renderTimeBufferView(", StringComparison.Ordinal), "independent time-buffer page renderer should be removed");
    AssertTrue(script.Contains("function renderTimeBufferBreachEvidence(", StringComparison.Ordinal), "breach analysis should have a consolidated time-buffer evidence renderer");
    AssertTrue(script.Contains("renderTimeBufferBreachEvidence(result, state.futureComparisonBaseline)", StringComparison.Ordinal), "future comparison renderer should invoke consolidated time-buffer evidence with frozen-baseline evidence");
    var renderer = SourceFunctionBody(script, "renderTimeBufferBreachEvidence");
    foreach (var backendSource in new[] { ".breaches", ".timeBufferProjection", "planningInputs.timeBuffers" })
    {
        AssertTrue(renderer.Contains(backendSource, StringComparison.Ordinal), $"consolidated renderer should read backend source {backendSource}");
    }
    foreach (var backendField in new[] { ".timeBufferProjection", ".breaches", ".maximumPenetrationPercent", ".earliestRedWeek", ".consecutiveRiskWeeks", ".recoveryWeek", ".isUnrecovered" })
    {
        AssertTrue(renderer.Contains(backendField, StringComparison.Ordinal), $"consolidated renderer should use backend field {backendField}");
    }
    AssertTrue(renderer.Contains("`${item.responseId}|${breach.target}`", StringComparison.Ordinal), "selector key should combine case response ID and buffer ID");
    AssertTrue(script.Contains("selectedTimeBufferBreachKey", StringComparison.Ordinal), "selected time-buffer evidence key should be retained in state");
    AssertTrue(script.Contains("byId(\"time-buffer-breach-select\").addEventListener(\"change\"", StringComparison.Ordinal), "time-buffer evidence selector should bind once in event registration");
    AssertTrue(!script.Contains("penetrationPercent =", StringComparison.Ordinal) && !script.Contains("delayDays /", StringComparison.Ordinal) && !script.Contains("/ point.bufferDays", StringComparison.Ordinal), "front end must not calculate time-buffer penetration");
    AssertTrue(renderer.Contains("evidenceStatus === \"Complete\"", StringComparison.Ordinal), "time-buffer display should recognize complete backend evidence");
    AssertTrue(renderer.Contains("evidenceStatus === \"NotApplicable\"", StringComparison.Ordinal), "time-buffer display should distinguish not applicable");
    AssertTrue(renderer.Contains("证据缺失", StringComparison.Ordinal), "time-buffer display should distinguish missing evidence");
    AssertTrue(renderer.Contains("!selected.breach.isBreached ? \"不适用\"", StringComparison.Ordinal), "a complete non-breach should keep recovery explicitly not applicable");
    foreach (var envelopeCriterion in new[]
    {
        "const definitionMatches = definitions.filter(item => item.bufferId === breach.target)",
        "const projectionWeekKeys = points.map(point => point.week)",
        "const projectionWeeksUnique = new Set(projectionWeekKeys).size === projectionWeekKeys.length",
        "const expectedHorizonWeeks = Number(item.preview?.request?.horizonWeeks)",
        "const horizonIsValid = Number.isInteger(expectedHorizonWeeks) && expectedHorizonWeeks > 0",
        "const firstMissingWeek = missingWeekCandidate === undefined ? null : missingWeekCandidate",
        "const projectionCoversHorizon = horizonIsValid",
        "definitionMatches.length === 1",
        "definitionMatches[0].evidenceStatus === \"Complete\"",
        "projectionCoversHorizon",
        "points.every(point => point.evidenceStatus === \"Complete\")",
    })
    {
        AssertTrue(renderer.Contains(envelopeCriterion, StringComparison.Ordinal), $"effective time-buffer evidence should require {envelopeCriterion}");
    }
    AssertTrue(renderer.Contains("breach.evidenceStatus === \"NotApplicable\"", StringComparison.Ordinal)
        && renderer.Contains("effectiveEvidenceStatus: \"NotApplicable\"", StringComparison.Ordinal),
        "not-applicable backend results should remain not applicable without requiring definition or projection evidence");
    AssertTrue(renderer.Contains("effectiveEvidenceStatus: completeEnvelope ? \"Complete\" : \"EvidenceMissing\"", StringComparison.Ordinal),
        "all non-not-applicable results should use the strict evidence envelope");
    AssertTrue(renderer.Contains("item.effectiveEvidenceStatus === \"Complete\"", StringComparison.Ordinal),
        "selection priority should use effective complete evidence instead of the breach flag alone");
    AssertTrue(renderer.Contains("const evidenceStatus = selected.effectiveEvidenceStatus", StringComparison.Ordinal),
        "chip and summary should use the effective evidence envelope");
    AssertTrue(renderer.Contains("selected.hasDuplicateWeeks", StringComparison.Ordinal)
        && renderer.Contains("周度证据包含重复周", StringComparison.Ordinal),
        "duplicate weeks should show an explicit evidence-missing diagnostic instead of repeated matrix columns");
    AssertTrue(renderer.Contains("周度证据缺少第 ${firstMissingWeek} 周", StringComparison.Ordinal),
        "partial horizons should identify the first missing evidence week");
    var notApplicableMatrix = renderer.IndexOf("if (evidenceStatus === \"NotApplicable\")", StringComparison.Ordinal);
    var duplicateMatrix = renderer.IndexOf("if (selected.hasDuplicateWeeks)", StringComparison.Ordinal);
    AssertTrue(notApplicableMatrix >= 0 && duplicateMatrix > notApplicableMatrix,
        "not-applicable evidence should remain not applicable even when irrelevant projection rows are malformed");
    AssertTrue(renderer.Contains("point.evidenceStatus === \"Complete\"", StringComparison.Ordinal),
        "partial backend rows should visibly retain their own evidence status");
    AssertTrue(script.Contains("breach.evidenceStatus === \"Complete\" || breach.evidenceStatus === \"NotApplicable\"", StringComparison.Ordinal), "comparison cards should accept a legitimate not-applicable scope without reporting missing evidence");
    AssertTrue(script.Contains("allBreachEvidenceNotApplicable ? \"不适用\"", StringComparison.Ordinal), "comparison cards should label an entirely not-applicable breach set explicitly");
    AssertTrue(script.Contains("const breachEvidenceAvailable = item.evidenceStatus === \"Complete\"", StringComparison.Ordinal), "all future breach rows should branch on backend evidence status");
    AssertTrue(script.Contains("!breachEvidenceAvailable ? unavailableEvidence", StringComparison.Ordinal), "future breach rows must show evidence state before non-breach values");
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
    AssertTrue(result.Kpis.AverageInventoryValue is > 0m, "buffer trend should calculate average inventory value");
    AssertTrue(result.Kpis.PeakInventoryValue.HasValue && result.Kpis.AverageInventoryValue.HasValue &&
        result.Kpis.PeakInventoryValue.Value >= result.Kpis.AverageInventoryValue.Value,
        "peak inventory should be at least average inventory");
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
    AssertTrue(result.Series.Any(item => item.Status is "Red" or "Yellow" or "Green" or "OverTopOfGreen"), "series should expose pre-replenishment position statuses");
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
    AssertTrue(program.Contains("AddSingleton<ICurrentBaselineDataSource>(sp =>", StringComparison.Ordinal), "current baseline source should be registered");
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

internal sealed class RecordingHistoryOperatingFactSource : IHistoryOperatingFactSource
{
    private readonly IHistoryOperatingFactSource _inner;

    public RecordingHistoryOperatingFactSource(IHistoryOperatingFactSource inner)
    {
        _inner = inner;
    }

    public List<HistoryFactRequest> Requests { get; } = new();

    public HistoryFactSet Load(HistoryFactRequest request)
    {
        Requests.Add(request);
        return _inner.Load(request);
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
                .Append(linked with { WeekOffset = -46 })
                .ToList()
        };
    }
}

internal sealed class InvalidAbnormalCostLedgerHistoryOperatingFactSource : IHistoryOperatingFactSource
{
    private readonly IHistoryOperatingFactSource _inner;

    public InvalidAbnormalCostLedgerHistoryOperatingFactSource(IHistoryOperatingFactSource inner)
    {
        _inner = inner;
    }

    public HistoryFactSet Load(HistoryFactRequest request)
    {
        var facts = _inner.Load(request);
        var linked = facts.AbnormalCosts.Single(item => item.EventId == "HAC-2026-002");
        var invalidated = facts.AbnormalCosts
            .Select(item => item.EventId switch
            {
                "HAC-2025-004" => item with { CostAmount = -1m },
                "HAC-2025-003" => item with { SourceAuthority = null },
                "HAC-2026-001" => item with { EvidenceStatus = "EvidenceMissing" },
                _ => item,
            })
            .ToList();
        return facts with
        {
            AbnormalCosts = invalidated
                .Append(linked with { WeekOffset = -46 })
                .Append(new HistoryAbnormalCostEvent(
                    "HAC-VALID-RECENT",
                    -8,
                    50_000m,
                    "额外物流费用",
                    "有效事件",
                    "Complete",
                    "需求对象",
                    "星载电子",
                    "星载电子需求控制点",
                    "DDAE 演示历史事实台账"))
                .ToList()
        };
    }
}

internal sealed class OpeningOnHandContinuityPoisonHistoryOperatingFactSource : IHistoryOperatingFactSource
{
    private readonly IHistoryOperatingFactSource _inner;
    private readonly string _sku;
    private readonly int _weekOffset;

    public OpeningOnHandContinuityPoisonHistoryOperatingFactSource(
        IHistoryOperatingFactSource inner,
        string sku,
        int weekOffset)
    {
        _inner = inner;
        _sku = sku;
        _weekOffset = weekOffset;
    }

    public HistoryFactSet Load(HistoryFactRequest request)
    {
        var facts = _inner.Load(request);
        return facts with
        {
            BufferFacts = facts.BufferFacts
                .Select(item => item.Sku == _sku && item.WeekOffset == _weekOffset
                    ? item with { OpeningOnHand = item.OpeningOnHand + 1m }
                    : item)
                .ToList()
        };
    }
}

internal sealed class CapacityFactTransformingHistoryOperatingFactSource : IHistoryOperatingFactSource
{
    private readonly IHistoryOperatingFactSource _inner;
    private readonly Func<IReadOnlyList<WeeklyCapacityFact>, IReadOnlyList<WeeklyCapacityFact>> _transform;

    public CapacityFactTransformingHistoryOperatingFactSource(
        IHistoryOperatingFactSource inner,
        Func<IReadOnlyList<WeeklyCapacityFact>, IReadOnlyList<WeeklyCapacityFact>> transform)
    {
        _inner = inner;
        _transform = transform;
    }

    public HistoryFactSet Load(HistoryFactRequest request)
    {
        var facts = _inner.Load(request);
        return facts with { CapacityFacts = _transform(facts.CapacityFacts) };
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

internal sealed class CountingInternalDemoOperatingFactSource : IInternalDemoOperatingFactSource
{
    private readonly InternalDemoOperatingFactSet _initial;
    private readonly InternalDemoOperatingFactSet _later;

    public CountingInternalDemoOperatingFactSource(
        InternalDemoOperatingFactSet initial,
        InternalDemoOperatingFactSet later)
    {
        _initial = initial;
        _later = later;
    }

    public int LoadCount { get; private set; }

    public InternalDemoOperatingFactSet Load()
    {
        LoadCount++;
        return LoadCount == 1 ? _initial : _later;
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
