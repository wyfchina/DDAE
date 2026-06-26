namespace AdaptiveSopDdsop.NetworkStructure;

public sealed record NetworkMetricSkuSignal(
    string Sku,
    decimal Adu);

public sealed record NetworkMetricResourceLoadSignal(
    string ResourceCode,
    decimal LoadPercent);

public sealed record NetworkMetricSupplierCapacitySignal(
    string Supplier,
    string RiskStatus);

public sealed record NetworkMetricsDataSet(
    int HorizonWeeks,
    DateOnly AnchorDate,
    NetworkDataSet NetworkData,
    IReadOnlyList<NetworkMetricSkuSignal> SkuSignals,
    IReadOnlyList<NetworkMetricResourceLoadSignal> ResourceLoads,
    IReadOnlyList<NetworkMetricSupplierCapacitySignal> SupplierCapacity);

public interface INetworkMetricsDataSource
{
    NetworkMetricsDataSet LoadNetworkMetrics(int horizonWeeks, DateOnly anchorDate);
}

public sealed record NetworkMetricEvidence(
    string EvidenceType,
    string EvidenceKey,
    string ItemCode,
    string RelatedCode,
    string Description,
    decimal Quantity,
    decimal ScoreContribution);

public sealed record NetworkMetricBreakdown(
    decimal Score,
    decimal RawValue,
    string Explanation,
    IReadOnlyList<NetworkMetricEvidence> Evidence);

public sealed record NetworkItemMetric(
    string ItemCode,
    string ItemName,
    string ItemType,
    string Family,
    decimal DownstreamCoverageScore,
    decimal QuantityImpactScore,
    decimal CumulativeLeadTimeScore,
    decimal SupplyRiskScore,
    decimal ResourceConstraintScore,
    decimal InventoryCostScore,
    NetworkMetricBreakdown DownstreamCoverage,
    NetworkMetricBreakdown QuantityImpact,
    NetworkMetricBreakdown CumulativeLeadTime,
    NetworkMetricBreakdown SupplyRisk,
    NetworkMetricBreakdown ResourceConstraint,
    NetworkMetricBreakdown InventoryCost,
    string Summary);

public sealed record NetworkMetricsWorkspaceResult(
    int HorizonWeeks,
    string ModelVersion,
    IReadOnlyList<NetworkItemMetric> ItemMetrics,
    string SelectedItemCode);
