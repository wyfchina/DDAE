namespace AdaptiveSopDdsop.NetworkStructure;

public sealed record NetworkScoringFamilySignal(
    string Code,
    string Name,
    decimal TargetServiceLevel,
    decimal TargetFlowIndex);

public sealed record NetworkScoringSkuSignal(
    string Sku,
    string Name,
    string Family,
    decimal Adu,
    int DecoupledLeadTimeDays,
    decimal VariabilityFactor,
    int OrderCycleDays,
    decimal MinimumOrderQuantity,
    decimal UnitCost,
    decimal TopOfRed,
    decimal TopOfYellow,
    decimal TopOfGreen);

public sealed record NetworkScoringDemandSignal(
    string Sku,
    int Week,
    decimal BaselineDemand);

public sealed record NetworkScoringResourceSignal(
    string Code,
    string Name);

public sealed record NetworkScoringRoutingSignal(
    string Sku,
    string ResourceCode,
    decimal CapacityPerUnit);

public sealed record NetworkScoringSupplierItemSignal(
    string Supplier,
    string Sku,
    string MaterialFamily,
    decimal UnitCost,
    int LeadTimeDays);

public sealed record NetworkScoringBufferProjectionSignal(
    string Sku,
    int Week,
    string BufferStatus);

public sealed record NetworkScoringResourceLoadSignal(
    string ResourceCode,
    decimal LoadPercent);

public sealed record NetworkScoringSupplyRequirementSignal(
    string Supplier,
    string MaterialFamily,
    int Week,
    decimal RequiredQuantity);

public sealed record NetworkScoringSupplierCapacitySignal(
    string Supplier,
    string MaterialFamily,
    string RiskStatus,
    decimal Gap);

public sealed record NetworkScoringDataSet(
    int HorizonWeeks,
    DateOnly AnchorDate,
    NetworkDataSet NetworkData,
    IReadOnlyList<NetworkScoringFamilySignal> Families,
    IReadOnlyList<NetworkScoringSkuSignal> Skus,
    IReadOnlyList<NetworkScoringDemandSignal> Demand,
    IReadOnlyList<NetworkScoringResourceSignal> Resources,
    IReadOnlyList<NetworkScoringRoutingSignal> ResourceRoutings,
    IReadOnlyList<NetworkScoringSupplierItemSignal> SupplierItemSources,
    IReadOnlyList<NetworkScoringBufferProjectionSignal> BufferProjections,
    IReadOnlyList<NetworkScoringResourceLoadSignal> CapacityLoads,
    IReadOnlyList<NetworkScoringSupplyRequirementSignal> SupplyRequirements,
    IReadOnlyList<NetworkScoringSupplierCapacitySignal> SupplierCapacity);

public interface INetworkScoringDataSource
{
    NetworkScoringDataSet LoadNetworkScoring(int horizonWeeks, DateOnly anchorDate);
}

public sealed record NetworkStructureFactorWeight(
    string Factor,
    decimal Weight,
    string Explanation);

public sealed record NetworkStructureCandidate(
    string CandidateId,
    string TargetType,
    string Target,
    string TargetName,
    string Family,
    string RecommendedSettingType,
    decimal Score,
    string Severity,
    decimal ReuseScore,
    decimal LeadTimeScore,
    decimal DemandVariabilityScore,
    decimal SupplyRiskScore,
    decimal ResourceConstraintScore,
    decimal InventoryCostPenalty,
    decimal ServiceImpactScore,
    string Rationale,
    string RecommendedAction,
    IReadOnlyList<string> Evidence,
    string NotAdoptingRisk,
    decimal QuantityImpactScore = 0m);

public sealed record NetworkStructureScoreSummary(
    string RecommendedSettingType,
    int CandidateCount,
    decimal AverageScore,
    decimal TopScore,
    string TopTarget,
    string Summary);

public sealed record NetworkStructureRecommendation(
    string Target,
    string RecommendedSettingType,
    string Severity,
    string Message);

public sealed record NetworkStructureScoringResult(
    int HorizonWeeks,
    string ModelVersion,
    string ScoringScope,
    IReadOnlyList<NetworkStructureFactorWeight> FactorWeights,
    IReadOnlyList<NetworkStructureScoreSummary> Summaries,
    IReadOnlyList<NetworkStructureCandidate> Candidates,
    IReadOnlyList<NetworkStructureRecommendation> Recommendations,
    string SelectedCandidateId);
