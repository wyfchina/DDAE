using AdaptiveSopDdsop.NetworkStructure;

using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.NetworkStructureIntegration;

// DDS&OP integration contract for feeding the independent network structure product.
// It intentionally stays outside ScenarioWorkspaceDataSet so the scenario workspace
// model does not own the network product boundary.
public sealed record NetworkStructureRuntimeSignals(
    IReadOnlyList<ProductFamily> Families,
    IReadOnlyList<SkuBufferSetting> Skus,
    IReadOnlyList<WeeklyDemand> Demand,
    IReadOnlyList<CapacityResource> Resources,
    IReadOnlyList<ResourceRouting> ResourceRoutings,
    IReadOnlyList<SupplierItemSource> SupplierItemSources,
    IReadOnlyList<SupplierCapacityWindow> SupplierCapacityWindows,
    IReadOnlyList<InventoryPosition> Inventory);

public sealed record NetworkStructureDataRequest(
    int HorizonWeeks,
    DateOnly AnchorDate,
    IReadOnlyList<string>? SkuFilter = null,
    IReadOnlyList<string>? FamilyFilter = null);

public sealed record NetworkStructureDataSet(
    NetworkStructureDataRequest Request,
    NetworkDataSet NetworkData,
    NetworkStructureRuntimeSignals RuntimeSignals)
{
    public IReadOnlyList<ProductFamily> Families => RuntimeSignals.Families;
    public IReadOnlyList<SkuBufferSetting> Skus => RuntimeSignals.Skus;
    public IReadOnlyList<WeeklyDemand> Demand => RuntimeSignals.Demand;
    public IReadOnlyList<CapacityResource> Resources => RuntimeSignals.Resources;
    public IReadOnlyList<ResourceRouting> ResourceRoutings => RuntimeSignals.ResourceRoutings;
    public IReadOnlyList<SupplierItemSource> SupplierItemSources => RuntimeSignals.SupplierItemSources;
    public IReadOnlyList<SupplierCapacityWindow> SupplierCapacityWindows => RuntimeSignals.SupplierCapacityWindows;
    public IReadOnlyList<InventoryPosition> Inventory => RuntimeSignals.Inventory;
}

public interface INetworkStructureDataSource
{
    NetworkStructureDataSet LoadNetworkStructure(NetworkStructureDataRequest request);
}

// Cross-product contract: network structure scoring can request DDS&OP white-box
// recalculation through this gateway without depending on the concrete preview service.
public interface IDdsopWhiteBoxScenarioGateway
{
    ScenarioRunPreviewResult Recalculate(ScenarioRunPreviewRequest request);
}

public sealed record DdsopWhiteBoxGatewayOptions
{
    public string Mode { get; init; } = "Local";

    public string? BaseUrl { get; init; }

    public string PreviewEndpoint { get; init; } = "/api/scenario-runs/preview";
}

public sealed record NetworkScenarioValidationItem(
    string CandidateId,
    string Target,
    string TargetName,
    string RecommendedSettingType,
    decimal Score,
    string Severity,
    decimal AverageInventoryValueDelta,
    int RedWeekDelta,
    int ReplenishmentOrderCountDelta,
    decimal ReplenishmentQuantityDelta,
    decimal RccpPeakLoadDelta,
    decimal RccpAverageLoadDelta,
    int RccpRedWeekDelta,
    decimal SupplyGapDelta,
    string ValidationSummary,
    IReadOnlyList<string> Evidence,
    ScenarioRunPreviewRequest? WhiteBoxRecalculationRequest = null);

public sealed record NetworkScenarioValidationResult(
    int HorizonWeeks,
    string ModelVersion,
    IReadOnlyList<NetworkScenarioValidationItem> Validations,
    string SelectedCandidateId);

public sealed record SelectedCandidateAction(
    string CandidateId,
    string ActionType,
    string Target,
    decimal EstimatedCost,
    OptimizationCandidateImpact Impact,
    string Explanation,
    IReadOnlyList<string> Evidence);

public sealed record CombinationComparison(
    string ProfileId,
    string ProfileName,
    decimal ServiceLevelDelta,
    decimal FlowIndexDelta,
    decimal AverageInventoryValueDelta,
    decimal PeakLoadPercentDelta,
    int RedSkuCountDelta,
    decimal SupplyGapDelta,
    int ReplenishmentOrderCountDelta,
    decimal EstimatedActionCost,
    string ManagementDecision);

public sealed record CandidateActionCombination(
    string ProfileId,
    string ProfileName,
    string Summary,
    OptimizationSolverStatus SolverStatus,
    string SolverMessage,
    decimal ObjectiveValue,
    ScenarioRunPreviewRequest WhiteBoxRecalculationRequest,
    ScenarioRunPreviewResult WhiteBoxPreviewResult,
    CombinationComparison Comparison,
    decimal EstimatedActionCost,
    IReadOnlyList<SelectedCandidateAction> SelectedActions,
    IReadOnlyList<ScenarioAuditTrace> Trace);

public sealed record CandidateActionCombinationResult(
    OptimizationSolverStatus SolverStatus,
    string SolverName,
    string Message,
    IReadOnlyList<OptimizationCandidateImpact> CandidateImpactMatrix,
    IReadOnlyList<CombinationComparison> CombinationComparisons,
    IReadOnlyList<CandidateActionCombination> Combinations,
    IReadOnlyList<ScenarioAuditTrace> Trace,
    bool IsPersisted);
