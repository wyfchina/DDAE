namespace AdaptiveSopDdsop.NetworkStructure;

public sealed record NetworkItemMaster(
    string ItemCode,
    string ItemName,
    string ItemType,
    string Family,
    string LifecycleStatus,
    decimal UnitCost,
    string PlanningUom);

public sealed record NetworkBomHeader(
    string BomId,
    string ParentItemCode,
    string BomVersion,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string ReleaseStatus);

public sealed record NetworkBomLine(
    string BomId,
    string ParentItemCode,
    string ComponentItemCode,
    decimal QuantityPer,
    decimal ScrapFactor,
    string AlternateGroup);

public sealed record NetworkAlternateItem(
    string AlternateGroup,
    string PrimaryItemCode,
    string AlternateItemCode,
    int Priority,
    decimal SubstitutionRatio,
    string QualificationStatus);

public sealed record NetworkRoutingLine(
    string ItemCode,
    string ModelCode,
    string ProductFamily,
    string RoutingVersion,
    string OperationCode,
    string ResourceCode,
    decimal CapacityPerUnit,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo);

public sealed record NetworkSupplierSource(
    string ItemCode,
    string SupplierCode,
    string SupplierName,
    int Priority,
    decimal AllocationPercent,
    int LeadTimeDays,
    decimal LeadTimeVariabilityFactor,
    decimal CapacityPerWeek,
    decimal MinimumOrderQuantity,
    string QualificationStatus);

public sealed record NetworkInventoryLocation(
    string ItemCode,
    string LocationCode,
    string LocationName,
    string LocationType,
    string QualityStatus,
    string Owner,
    int? ShelfLifeDays,
    bool IsShared);

public sealed record NetworkBufferSetting(
    string ItemCode,
    bool IsDecouplingPoint,
    string InventoryBufferProfile,
    int TimeBufferDays,
    decimal MinimumOrderQuantity,
    int OrderCycleDays,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string Status);

public sealed record NetworkLeadTimeProfile(
    string ItemCode,
    string SourceType,
    int StandardLeadTimeDays,
    decimal VariabilityFactor,
    string AppliesBeforeItemCode);

public sealed record NetworkDataSet(
    IReadOnlyList<NetworkItemMaster> Items,
    IReadOnlyList<NetworkBomHeader> BomHeaders,
    IReadOnlyList<NetworkBomLine> BomLines,
    IReadOnlyList<NetworkAlternateItem> AlternateItems,
    IReadOnlyList<NetworkRoutingLine> RoutingLines,
    IReadOnlyList<NetworkSupplierSource> SupplierSources,
    IReadOnlyList<NetworkInventoryLocation> InventoryLocations,
    IReadOnlyList<NetworkBufferSetting> BufferSettings,
    IReadOnlyList<NetworkLeadTimeProfile> LeadTimeProfiles);

public sealed record NetworkStructureProductDataRequest(
    int HorizonWeeks,
    DateOnly AnchorDate,
    IReadOnlyList<string>? ItemFilter = null,
    IReadOnlyList<string>? FamilyFilter = null);

public sealed record NetworkStructureProductDataSet(
    NetworkStructureProductDataRequest Request,
    NetworkDataSet NetworkData);

public interface INetworkStructureProductDataSource
{
    NetworkStructureProductDataSet LoadNetworkStructure(NetworkStructureProductDataRequest request);
}
