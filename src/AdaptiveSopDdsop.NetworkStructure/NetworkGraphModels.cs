namespace AdaptiveSopDdsop.NetworkStructure;

public sealed record NetworkGraphDataSet(
    NetworkDataSet NetworkData,
    DateOnly AnchorDate);

public interface INetworkGraphDataSource
{
    NetworkGraphDataSet LoadNetworkGraph(DateOnly anchorDate);
}

public sealed record NetworkGraphNode(
    string ItemCode,
    string ItemName,
    string ItemType,
    string Family,
    string LifecycleStatus,
    decimal UnitCost,
    string PlanningUom,
    bool IsDecouplingPoint,
    bool HasInventoryLocation,
    bool HasSupplierSource,
    bool HasRouting);

public sealed record NetworkGraphEdge(
    string BomId,
    string ParentItemCode,
    string ParentItemName,
    string ComponentItemCode,
    string ComponentItemName,
    decimal QuantityPer,
    decimal ScrapFactor,
    decimal EffectiveQuantity,
    string AlternateGroup);

public sealed record NetworkImpactPath(
    string Direction,
    IReadOnlyList<string> ItemCodes,
    string PathText,
    int Depth,
    decimal CumulativeQuantity,
    string LeafItemCode,
    string LeafItemName,
    bool HasBuffer,
    bool LeafHasInventoryLocation,
    bool LeafHasSupplierSource,
    bool LeafHasRouting);

public sealed record NetworkImpactScope(
    string Direction,
    int NodeCount,
    int PathCount,
    int MaxDepth,
    IReadOnlyList<NetworkGraphNode> Nodes,
    IReadOnlyList<NetworkImpactPath> Paths);

public sealed record NetworkValidationIssue(
    string Severity,
    string RuleCode,
    string ItemCode,
    string ItemName,
    string Message,
    string Evidence);

public sealed record NetworkValidationReport(
    int RedCount,
    int YellowCount,
    int InfoCount,
    IReadOnlyList<NetworkValidationIssue> Issues);

public sealed record NetworkGraphWorkspaceResult(
    string SelectedItemCode,
    string SelectedItemName,
    int MaxDepth,
    IReadOnlyList<NetworkGraphNode> Nodes,
    IReadOnlyList<NetworkGraphEdge> Edges,
    NetworkImpactScope Upstream,
    NetworkImpactScope Downstream,
    NetworkValidationReport ValidationReport);
