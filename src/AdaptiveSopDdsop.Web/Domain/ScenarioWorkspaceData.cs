namespace AdaptiveSopDdsop.Web.Domain;

public sealed record ScenarioWorkspaceDataRequest(
    int HorizonWeeks,
    DateOnly AnchorDate,
    IReadOnlyList<string>? SkuFilter = null,
    IReadOnlyList<string>? FamilyFilter = null);

public sealed record HistoricalDemandActual(
    string Sku,
    int WeekOffset,
    decimal ActualDemand,
    decimal ForecastDemand,
    decimal ServiceLevelPercent,
    decimal EndingNetFlow);

public sealed record ExceptionSignal(
    string Sku,
    int WeekOffset,
    decimal ActualDemand,
    decimal ForecastDemand,
    decimal DemandVariancePercent,
    decimal ServiceLevelPercent,
    decimal EndingNetFlow,
    string Reason,
    string Severity);

public sealed record ExceptionScenarioPreset(
    string TemplateId,
    string Label,
    string ActionHint);

public sealed record ExceptionSkuSummary(
    string Sku,
    string Name,
    string Family,
    int LatestExceptionWeekOffset,
    decimal MaxDemandVariancePercent,
    decimal LowestServiceLevelPercent,
    decimal LowestNetFlow,
    int ExceptionCount,
    string Severity,
    string PrimaryReason,
    string RecommendedTemplateId,
    string RecommendedAction,
    IReadOnlyList<ExceptionSignal> Signals,
    IReadOnlyList<ExceptionScenarioPreset> ScenarioPresets);

public sealed record ExceptionWorkspaceResult(
    int HorizonWeeks,
    IReadOnlyList<ExceptionSkuSummary> Exceptions,
    int RedSkuCount,
    int YellowSkuCount,
    int DemandSpikeCount,
    int ServiceLossCount,
    int BufferRiskCount,
    string? AppliedSku);

public sealed record BudgetBenchmark(
    string Family,
    int Week,
    decimal BudgetRevenue,
    decimal LastYearRevenue,
    decimal BudgetInventoryValue,
    decimal LastYearInventoryValue);

public sealed record ResourceCalendarEntry(
    string ResourceCode,
    int Week,
    decimal CapacityMultiplier,
    string CalendarNote);

public sealed record SupplierCapacityWindow(
    string Supplier,
    string MaterialFamily,
    int Week,
    decimal CommittedCapacity,
    int LeadTimeDays,
    string RiskStatus);

public sealed record ScenarioTemplateAction(
    string ActionType,
    string Target,
    int StartWeek,
    int EndWeek,
    decimal Value,
    string Unit);

public sealed record ScenarioTemplate(
    string TemplateId,
    string Name,
    string Purpose,
    IReadOnlyList<ScenarioTemplateAction> Actions);

public sealed record BusinessGuardrail(
    string Metric,
    decimal YellowLimit,
    decimal RedLimit,
    string Unit,
    string DecisionRule);

public sealed record SkuPolicyOverride(
    string Sku,
    decimal? MinimumOrderQuantity = null,
    int? OrderCycleDays = null);

public sealed record SupplierCapacityLimit(
    string Supplier,
    string MaterialFamily,
    int StartWeek,
    int EndWeek,
    decimal CommittedCapacity);

public sealed record ScenarioRunParameterSet(
    IReadOnlyList<PrebuildCampaign>? PrebuildCampaigns = null,
    IReadOnlyList<ResourceCapacityAdjustment>? CapacityAdjustments = null,
    IReadOnlyList<SkuPolicyOverride>? SkuPolicyOverrides = null,
    IReadOnlyList<SupplierCapacityLimit>? SupplierCapacityLimits = null);

public sealed record ScenarioRunPreviewRequest(
    int HorizonWeeks = 12,
    string? TemplateId = null,
    IReadOnlyList<string>? SkuFilter = null,
    IReadOnlyList<string>? FamilyFilter = null,
    ScenarioRunParameterSet? Parameters = null,
    string? AdoptionConstraintMode = null);

public sealed record ScenarioPreviewMetrics(
    decimal ServiceLevelPercent,
    decimal FlowIndex,
    decimal AverageInventoryValue,
    decimal PeakLoadPercent,
    decimal AverageLoadPercent,
    int RedSkuCount,
    decimal SupplyGap,
    decimal ReplenishmentValue,
    int ReplenishmentOrderCount);

public sealed record SupplierCapacityComparison(
    string Supplier,
    string MaterialFamily,
    int Week,
    decimal RequiredQuantity,
    decimal CommittedCapacity,
    decimal Gap,
    string RiskStatus);

public sealed record BudgetComparison(
    string Family,
    int Week,
    decimal BudgetRevenue,
    decimal LastYearRevenue,
    decimal BudgetInventoryValue,
    decimal LastYearInventoryValue,
    decimal ProjectedInventoryValue,
    decimal BudgetInventoryVariance);

public sealed record RccpResourceSummary(
    string ResourceCode,
    string ResourceName,
    string ResourceType,
    decimal AverageLoadPercent,
    decimal PeakLoadPercent,
    int OverloadWeeks,
    decimal MaxCapacityGap,
    string Status,
    string RecommendedAction);

public sealed record RccpWeeklyCell(
    string ResourceCode,
    string ResourceName,
    int Week,
    decimal RequiredCapacity,
    decimal AvailableCapacity,
    decimal Variance,
    decimal LoadPercent,
    string Status);

public sealed record RccpSkuContribution(
    string ResourceCode,
    string Sku,
    string SkuName,
    string Family,
    int Week,
    decimal OrderQuantity,
    decimal CapacityPerUnit,
    decimal RequiredCapacity,
    string Trigger);

public sealed record RccpActionRecommendation(
    string ResourceCode,
    string ActionType,
    string Message,
    string Severity);

public sealed record RccpResourceDetail(
    string ResourceCode,
    string ResourceName,
    IReadOnlyList<RccpWeeklyCell> WeeklyLoad,
    IReadOnlyList<RccpSkuContribution> SkuContributions,
    IReadOnlyList<ProjectedReplenishmentOrder> ReplenishmentOrders,
    IReadOnlyList<RccpActionRecommendation> Recommendations);

public sealed record RccpComparison(
    decimal PeakLoadDelta,
    decimal AverageLoadDelta,
    int RedWeekDelta,
    decimal CapacityGapDelta);

public sealed record RccpWorkspaceResult(
    string CaseId,
    string Name,
    int HorizonWeeks,
    IReadOnlyList<RccpResourceSummary> ResourceSummaries,
    IReadOnlyList<RccpWeeklyCell> WeeklyCells,
    IReadOnlyList<RccpResourceDetail> ResourceDetails,
    decimal MaxPeakLoadPercent,
    decimal AverageLoadPercent,
    int RedResourceCount,
    int RedWeekCount,
    decimal MaxCapacityGap,
    decimal ReleasableCapacity,
    decimal ConstrainedGap);

public sealed record CapacityConstraintCell(
    string ResourceCode,
    string ResourceName,
    int Week,
    decimal UnconstrainedRequired,
    decimal ConstrainedAvailable,
    decimal Variance,
    decimal Gap,
    decimal LoadPercent,
    string Status);

public sealed record CapacityConstraintSummary(
    string ResourceCode,
    string ResourceName,
    decimal AverageLoadPercent,
    decimal PeakLoadPercent,
    int OverloadWeeks,
    decimal MaxGap,
    decimal TotalGap,
    string Status,
    string RecommendedAction);

public sealed record SupplyConstraintCell(
    string Supplier,
    string MaterialFamily,
    int Week,
    decimal UnconstrainedRequired,
    decimal ConstrainedAvailable,
    decimal Variance,
    decimal Gap,
    decimal LoadPercent,
    string Status);

public sealed record SupplyConstraintSummary(
    string Supplier,
    string MaterialFamily,
    decimal TotalUnconstrainedRequired,
    decimal TotalConstrainedAvailable,
    decimal TotalGap,
    int GapWeeks,
    string Status,
    string RecommendedAction);

public sealed record ConstraintActionRecommendation(
    string ScopeType,
    string Target,
    string ActionType,
    string Message,
    string Severity);

public sealed record ConstraintAuditTrace(
    string Stage,
    string Message,
    string Severity);

public sealed record ConstraintWorkspaceResult(
    string CaseId,
    string Name,
    int HorizonWeeks,
    IReadOnlyList<CapacityConstraintSummary> CapacitySummaries,
    IReadOnlyList<CapacityConstraintCell> CapacityCells,
    IReadOnlyList<SupplyConstraintSummary> SupplySummaries,
    IReadOnlyList<SupplyConstraintCell> SupplyCells,
    IReadOnlyList<ConstraintActionRecommendation> Recommendations,
    IReadOnlyList<ConstraintAuditTrace> Trace,
    decimal TotalCapacityGap,
    decimal TotalSupplyGap,
    int RedCapacityWeekCount,
    int RedSupplyWeekCount);

public sealed record SupplierCollaborationSummary(
    string Supplier,
    decimal TotalUnconstrainedRequired,
    decimal TotalConstrainedAvailable,
    decimal TotalGap,
    int GapWeeks,
    int AffectedSkuCount,
    int RedWeekCount,
    int YellowWeekCount,
    string Status,
    string RecommendedAction,
    string StatusReason);

public sealed record SupplierCollaborationWeeklyCell(
    string Supplier,
    int Week,
    decimal UnconstrainedRequired,
    decimal ConstrainedAvailable,
    decimal Variance,
    decimal Gap,
    decimal LoadPercent,
    string Status,
    string StatusReason);

public sealed record SupplierSkuRequirement(
    string Supplier,
    string MaterialFamily,
    string Sku,
    string SkuName,
    string Family,
    int Week,
    decimal OrderQuantity,
    decimal ProjectedValue,
    string Trigger);

public sealed record SupplierCollaborationAction(
    string Supplier,
    string ActionType,
    string Message,
    string Severity);

public sealed record SupplierCollaborationTrace(
    string Stage,
    string Message,
    string Severity);

public sealed record SupplierCollaborationWorkspaceResult(
    string CaseId,
    string Name,
    int HorizonWeeks,
    IReadOnlyList<SupplierCollaborationSummary> Summaries,
    IReadOnlyList<SupplierCollaborationWeeklyCell> WeeklyCells,
    IReadOnlyList<SupplierSkuRequirement> SkuRequirements,
    IReadOnlyList<SupplierCollaborationAction> Actions,
    IReadOnlyList<SupplierCollaborationTrace> Trace,
    decimal TotalSupplyGap,
    int RedSupplierCount,
    int YellowSupplierCount,
    int GapWeekCount,
    int AffectedSkuCount,
    string SelectedSupplier);

public sealed record BufferTrendKpis(
    int RedSkuCount,
    int YellowSkuCount,
    int ShortageCount,
    decimal AverageInventoryValue,
    decimal PeakInventoryValue,
    int ReplenishmentOrderCount,
    decimal InventoryValueDelta);

public sealed record BufferTrendSeriesPoint(
    string Sku,
    int Week,
    string PeriodStartDate,
    decimal TimePhasedAdu,
    decimal StartNetFlow,
    decimal Demand,
    decimal EndNetFlowBeforeReplenishment,
    decimal EndNetFlowAfterReplenishment,
    decimal TopOfRed,
    decimal TopOfYellow,
    decimal TopOfGreen,
    decimal TargetInventory,
    decimal InventoryValue,
    decimal ReplenishmentQuantity,
    bool IsReplenishment,
    bool IsPrebuild,
    string Status);

public sealed record BufferZoneBand(
    string Sku,
    decimal TopOfRed,
    decimal TopOfYellow,
    decimal TopOfGreen);

public sealed record BufferTrendComparison(
    decimal AverageInventoryValueDelta,
    decimal PeakInventoryValueDelta,
    int RedWeekDelta,
    int ReplenishmentOrderCountDelta,
    decimal ReplenishmentQuantityDelta);

public sealed record BufferFamilySummary(
    string Family,
    decimal AverageInventoryValue,
    int RedWeekCount,
    int YellowWeekCount,
    int OverGreenWeekCount,
    int ReplenishmentOrderCount);

public sealed record BufferWeeklyCell(
    string Sku,
    string SkuName,
    string Family,
    int Week,
    decimal EndNetFlow,
    decimal InventoryValue,
    string Status);

public sealed record BufferSkuDetail(
    string Sku,
    string Name,
    string Family,
    decimal Adu,
    int DecoupledLeadTimeDays,
    decimal MinimumOrderQuantity,
    int OrderCycleDays,
    decimal UnitCost,
    BufferZoneBand Zone,
    IReadOnlyList<BufferTrendSeriesPoint> Series,
    IReadOnlyList<ProjectedReplenishmentOrder> ReplenishmentOrders,
    IReadOnlyList<PlanningTrace> Traces);

public sealed record BufferTrendWorkspaceResult(
    string CaseId,
    string Name,
    int HorizonWeeks,
    BufferTrendKpis Kpis,
    IReadOnlyList<BufferTrendSeriesPoint> Series,
    IReadOnlyList<BufferZoneBand> ZoneBands,
    BufferTrendComparison Comparison,
    IReadOnlyList<BufferFamilySummary> FamilySummaries,
    IReadOnlyList<BufferWeeklyCell> WeeklyCells,
    IReadOnlyList<BufferSkuDetail> SkuDetails,
    string SelectedSku);

public sealed record ScenarioRunPreviewCase(
    string CaseId,
    string Name,
    DemandDrivenPlanResult Plan,
    ScenarioPreviewMetrics Metrics,
    BufferTrendWorkspaceResult BufferTrend,
    RccpWorkspaceResult Rccp,
    ConstraintWorkspaceResult Constraints,
    SupplierCollaborationWorkspaceResult SupplierCollaboration,
    IReadOnlyList<SupplierCapacityComparison> SupplierCapacity,
    IReadOnlyList<BudgetComparison> Budget);

public sealed record ScenarioComparisonMetrics(
    decimal ServiceLevelDelta,
    decimal FlowIndexDelta,
    decimal AverageInventoryValueDelta,
    decimal PeakLoadPercentDelta,
    decimal AverageLoadPercentDelta,
    int RedSkuCountDelta,
    decimal SupplyGapDelta,
    decimal ReplenishmentValueDelta,
    int ReplenishmentOrderCountDelta);

public sealed record ScenarioAuditTrace(
    string Stage,
    string Message,
    string Severity);

public sealed record ScenarioRunPreviewResult(
    ScenarioRunPreviewRequest Request,
    ScenarioRunPreviewCase Baseline,
    ScenarioRunPreviewCase Scenario,
    ScenarioComparisonMetrics Comparison,
    RccpComparison RccpComparison,
    IReadOnlyList<ScenarioAuditTrace> Trace,
    bool IsPersisted);

public sealed record ScenarioWorkspaceDataSet(
    ScenarioWorkspaceDataRequest Request,
    IReadOnlyList<ProductFamily> Families,
    IReadOnlyList<SkuBufferSetting> Skus,
    IReadOnlyList<InventoryPosition> Inventory,
    IReadOnlyList<WeeklyDemand> Demand,
    IReadOnlyList<CapacityResource> Resources,
    IReadOnlyList<ResourceRouting> ResourceRoutings,
    IReadOnlyList<SupplierItemSource> SupplierItemSources,
    IReadOnlyList<HistoricalDemandActual> HistoricalDemand,
    IReadOnlyList<BudgetBenchmark> BudgetBenchmarks,
    IReadOnlyList<ResourceCalendarEntry> ResourceCalendar,
    IReadOnlyList<SupplierCapacityWindow> SupplierCapacityWindows,
    IReadOnlyList<ScenarioTemplate> ScenarioTemplates,
    IReadOnlyList<BusinessGuardrail> Guardrails);

public interface IScenarioWorkspaceDataSource
{
    ScenarioWorkspaceDataSet Load(ScenarioWorkspaceDataRequest request);
}

public interface IScenarioWorkspaceDataAdapter<in TSource>
{
    ScenarioWorkspaceDataSet Map(TSource source, ScenarioWorkspaceDataRequest request);
}
