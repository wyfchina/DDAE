using AdaptiveSopDdsop.NetworkStructure;

using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.NetworkStructureIntegration;

public sealed class DdsopNetworkMetricsDataSource : INetworkMetricsDataSource
{
    private readonly INetworkStructureDataSource _dataSource;

    public DdsopNetworkMetricsDataSource(INetworkStructureDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public NetworkMetricsDataSet LoadNetworkMetrics(int horizonWeeks, DateOnly anchorDate)
    {
        var horizon = Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52);
        var data = _dataSource.LoadNetworkStructure(new NetworkStructureDataRequest(horizon, anchorDate));
        var planRun = DemandDrivenPlanningEngine.ProjectBuffers(data.Skus, data.Inventory, data.Demand, horizon);
        var capacityLoads = DemandDrivenPlanningEngine.ProjectRoughCutCapacity(
            planRun.ReplenishmentOrders,
            data.ResourceRoutings,
            data.Resources,
            horizon);
        var supplyRequirements = DemandDrivenPlanningEngine.ProjectSupplyRequirements(
            planRun.ReplenishmentOrders,
            data.SupplierItemSources);
        var supplierCapacity = ConstraintWorkspaceService.CompareSupplierCapacity(
            data.SupplierCapacityWindows,
            supplyRequirements,
            Array.Empty<SupplierCapacityLimit>());

        return new NetworkMetricsDataSet(
            horizon,
            data.Request.AnchorDate,
            data.NetworkData,
            data.Skus.Select(item => new NetworkMetricSkuSignal(item.Sku, item.Adu)).ToList(),
            capacityLoads.Select(item => new NetworkMetricResourceLoadSignal(item.ResourceCode, item.LoadPercent)).ToList(),
            supplierCapacity.Select(item => new NetworkMetricSupplierCapacitySignal(item.Supplier, item.RiskStatus)).ToList());
    }
}
