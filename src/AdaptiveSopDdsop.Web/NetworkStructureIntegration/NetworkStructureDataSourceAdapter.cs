using AdaptiveSopDdsop.NetworkStructure;

using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.NetworkStructureIntegration;

public sealed class NetworkStructureDataSourceAdapter : INetworkStructureDataSource
{
    private readonly IScenarioWorkspaceDataSource _scenarioDataSource;

    public NetworkStructureDataSourceAdapter(IScenarioWorkspaceDataSource scenarioDataSource)
    {
        _scenarioDataSource = scenarioDataSource;
    }

    public NetworkStructureDataSet LoadNetworkStructure(NetworkStructureDataRequest request)
    {
        var scenarioRequest = new ScenarioWorkspaceDataRequest(
            request.HorizonWeeks,
            request.AnchorDate,
            request.SkuFilter,
            request.FamilyFilter);
        return FromScenarioWorkspace(_scenarioDataSource.Load(scenarioRequest));
    }

    public static NetworkStructureDataSet FromScenarioWorkspace(ScenarioWorkspaceDataSet data)
    {
        return new NetworkStructureDataSet(
            new NetworkStructureDataRequest(
                data.Request.HorizonWeeks,
                data.Request.AnchorDate,
                data.Request.SkuFilter,
                data.Request.FamilyFilter),
            BuildNetworkData(data.Skus),
            new NetworkStructureRuntimeSignals(
                data.Families,
                data.Skus,
                data.Demand,
                data.Resources,
                data.ResourceRoutings,
                data.SupplierItemSources,
                data.SupplierCapacityWindows,
                data.Inventory));
    }

    private static NetworkDataSet BuildNetworkData(IReadOnlyList<SkuBufferSetting> skus)
    {
        var finishedGoods = skus
            .Select(sku => new NetworkFinishedGoodSeedInput(
                sku.Sku,
                sku.Name,
                sku.Family,
                sku.UnitCost,
                sku.BufferProfile,
                sku.DecoupledLeadTimeDays,
                sku.MinimumOrderQuantity,
                sku.OrderCycleDays,
                sku.ParameterStatus))
            .ToList();

        return SatelliteManufacturingNetworkSeedData.Build(finishedGoods);
    }
}
