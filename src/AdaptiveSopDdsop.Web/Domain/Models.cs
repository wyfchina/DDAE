namespace AdaptiveSopDdsop.Web.Domain;

public sealed record SkuBufferSetting(
    string Sku,
    string Name,
    string Family,
    decimal Adu,
    int DecoupledLeadTimeDays,
    decimal VariabilityFactor,
    int OrderCycleDays,
    decimal MinimumOrderQuantity,
    decimal UnitCost,
    decimal WeeklyCapacityUnits);

public sealed record InventoryPosition(
    string Sku,
    decimal OnHand,
    decimal OpenSupply,
    decimal QualifiedDemand);

public sealed record BufferZones(
    decimal Red,
    decimal Yellow,
    decimal Green)
{
    public decimal TopOfRed => Red;
    public decimal TopOfYellow => Red + Yellow;
    public decimal TopOfGreen => Red + Yellow + Green;
}

public sealed record PlanningRecommendation(
    string Sku,
    string Action,
    decimal NetFlowPosition,
    decimal OrderQuantity,
    string BufferStatus,
    decimal WorkingCapital);

public sealed record BufferProjectionPoint(
    string Sku,
    int Week,
    decimal StartNetFlow,
    decimal Demand,
    decimal EndNetFlowBeforeReplenishment,
    decimal EndNetFlowAfterReplenishment,
    string BufferStatus);

public sealed record ProjectedReplenishmentOrder(
    string Sku,
    int Week,
    decimal Quantity,
    decimal Value,
    string Trigger);

public sealed record PrebuildCampaign(
    string CampaignId,
    string Sku,
    int BuildWeek,
    int ProtectFromWeek,
    int ProtectThroughWeek,
    decimal Quantity);

public sealed record PlanningTrace(
    string Sku,
    int Week,
    string Explanation);

public sealed record DemandDrivenPlanRun(
    IReadOnlyList<BufferProjectionPoint> BufferProjections,
    IReadOnlyList<ProjectedReplenishmentOrder> ReplenishmentOrders,
    IReadOnlyList<PlanningTrace> Traces);

public sealed record ResourceRouting(
    string Sku,
    string ResourceCode,
    decimal CapacityPerUnit);

public sealed record ResourceCapacityAdjustment(
    string ResourceCode,
    int Week,
    decimal CapacityMultiplier,
    string Reason);

public sealed record SupplierItemSource(
    string Supplier,
    string Sku,
    string MaterialFamily,
    decimal UnitCost);

public sealed record ProjectedSupplyRequirement(
    string Supplier,
    string MaterialFamily,
    int Week,
    decimal RequiredQuantity,
    decimal ProjectedValue);

public sealed record CapacityLoadProjection(
    string ResourceCode,
    string ResourceName,
    int Week,
    decimal RequiredCapacity,
    decimal AvailableCapacity,
    decimal LoadPercent,
    string Status);

public sealed record DemandDrivenPlanResult(
    IReadOnlyList<BufferProjectionPoint> BufferProjections,
    IReadOnlyList<ProjectedReplenishmentOrder> ReplenishmentOrders,
    IReadOnlyList<CapacityLoadProjection> CapacityLoads,
    IReadOnlyList<ProjectedSupplyRequirement> SupplyRequirements,
    IReadOnlyList<PlanningTrace> Traces);

public sealed record ProductFamily(
    string Code,
    string Name,
    decimal TargetServiceLevel,
    decimal TargetFlowIndex,
    decimal RevenuePerUnit);

public sealed record WeeklyDemand(
    string Sku,
    int Week,
    decimal BaselineDemand);

public sealed record CapacityResource(
    string Code,
    string Name,
    decimal WeeklyAvailableUnits,
    decimal UnitLoad);

public sealed record ValidationData(
    IReadOnlyList<ProductFamily> Families,
    IReadOnlyList<SkuBufferSetting> Skus,
    IReadOnlyList<InventoryPosition> Inventory,
    IReadOnlyList<WeeklyDemand> Demand,
    IReadOnlyList<CapacityResource> Resources,
    IReadOnlyList<ResourceRouting> ResourceRoutings,
    IReadOnlyList<SupplierItemSource> SupplierItemSources,
    IReadOnlyList<StrategicMonth> StrategicMonths,
    IReadOnlyList<ProcessStep> AsopSteps,
    IReadOnlyList<ProcessStep> DdsopElements,
    IReadOnlyList<PortfolioItem> PortfolioItems,
    IReadOnlyList<FinancialProjection> FinancialProjections,
    IReadOnlyList<ResourceProfile> ResourceProfiles,
    IReadOnlyList<SupplierConstraint> SupplierConstraints,
    IReadOnlyList<CapitalRequirement> CapitalRequirements,
    IReadOnlyList<MasterSetting> MasterSettings,
    IReadOnlyList<KnownEvent> KnownEvents,
    IReadOnlyList<DdomFeedbackPoint> DdomFeedback,
    IReadOnlyList<TacticalOpportunity> TacticalOpportunities,
    IReadOnlyList<StrategicRecommendation> StrategicRecommendations,
    IReadOnlyList<FeasibilityCheck> FeasibilityChecks,
    IReadOnlyList<SkillBuffer> SkillBuffers);

public sealed record StrategicMonth(
    int Index,
    string Label,
    int Year,
    int Month);

public sealed record ProcessStep(
    int Sequence,
    string Code,
    string Name,
    string Owner,
    string Purpose,
    string PrimaryOutput);

public sealed record PortfolioItem(
    string Code,
    string Name,
    string Family,
    string LifecycleStage,
    string Decision,
    decimal TargetRevenue,
    decimal ContributionMarginPercent,
    string HealthStatus,
    string RiskNote);

public sealed record FinancialProjection(
    int MonthIndex,
    string MonthLabel,
    string Family,
    decimal DemandUnits,
    decimal Revenue,
    decimal ContributionMargin,
    decimal WorkingCapital,
    decimal RoiPercent,
    decimal CashGap);

public sealed record ResourceProfile(
    int MonthIndex,
    string MonthLabel,
    string Resource,
    string Family,
    decimal RequiredUnits,
    decimal AvailableUnits,
    decimal LoadPercent,
    string Status);

public sealed record SupplierConstraint(
    string Supplier,
    string MaterialFamily,
    decimal MonthlyCapacity,
    decimal MonthlyRequirement,
    int LeadTimeDays,
    string RiskStatus,
    string Mitigation);

public sealed record CapitalRequirement(
    string Code,
    string Name,
    string TriggerMonth,
    string Constraint,
    decimal Investment,
    decimal CapacityIncrease,
    decimal RoiPercent,
    int PaybackMonths,
    string DecisionStatus);

public sealed record MasterSetting(
    string SettingId,
    string SettingType,
    string Target,
    string CurrentValue,
    string ProposedValue,
    string Trigger,
    string EffectiveWindow,
    string Status,
    decimal ServiceImpact,
    decimal CashImpact);

public sealed record KnownEvent(
    string EventId,
    string EventType,
    string Name,
    string Window,
    string AppliesTo,
    decimal DemandAdjustmentFactor,
    decimal ZoneAdjustmentFactor,
    string Owner,
    string Status);

public sealed record DdomFeedbackPoint(
    string Period,
    string Target,
    decimal Reliability,
    decimal Stability,
    decimal Velocity,
    int RedPenetrations,
    int BlackPenetrations,
    int ActAlerts,
    int LateAlerts,
    decimal DemonstratedAdu,
    decimal DemonstratedResourceLoad,
    string RootCause);

public sealed record TacticalOpportunity(
    string OpportunityId,
    string Name,
    string Trigger,
    decimal IncrementalRevenue,
    decimal ContributionMargin,
    decimal VariableCost,
    decimal FlowDelta,
    string Risk,
    string Status);

public sealed record StrategicRecommendation(
    string RecommendationId,
    string Type,
    string Name,
    string TriggerSignal,
    decimal Investment,
    decimal ContributionMarginDelta,
    decimal RoiDelta,
    decimal FlowDelta,
    string DecisionOwner,
    string Status);

public sealed record FeasibilityCheck(
    string Scenario,
    string Horizon,
    decimal DdomToleranceUsePercent,
    decimal PacingResourceLoadPercent,
    decimal WorkingCapitalRequirement,
    decimal SpaceRequirementPallets,
    decimal CapacityGap,
    decimal LostRoiOpportunity,
    string Status,
    string RequiredAction);

public sealed record SkillBuffer(
    string Team,
    string CriticalSkill,
    int CertifiedPeople,
    int RequiredPeople,
    string Status,
    string TrainingAction);

public sealed record ScenarioInput(
    decimal PromotionPercent = 0,
    int SupplyDisruptionWeeks = 0,
    int PlannedShutdownDays = 0,
    decimal NewProductWeeklyDemand = 0);

public sealed record SkuScenarioResult(
    string Sku,
    string Name,
    string Family,
    decimal Adu,
    BufferZones Zones,
    decimal NetFlowPosition,
    string BufferStatus,
    decimal PlannedOrder,
    decimal WorkingCapital);

public sealed record ScenarioResult(
    ScenarioInput Input,
    IReadOnlyList<SkuScenarioResult> Skus,
    decimal TotalAdu,
    decimal TotalWorkingCapital,
    decimal BufferHealthPercent,
    decimal CapacityUtilizationPercent,
    decimal ServiceProjectionPercent,
    decimal FlowIndex,
    GuardrailResult Guardrail,
    IReadOnlyList<string> ManagementActions);

public sealed record GuardrailResult(
    string Status,
    string StatusLabel,
    bool IsAdoptionBlocked,
    string Decision,
    IReadOnlyList<GuardrailCheck> Checks);

public sealed record GuardrailCheck(
    string Metric,
    decimal Value,
    string Unit,
    decimal YellowLimit,
    decimal RedLimit,
    string Status,
    string Message);
