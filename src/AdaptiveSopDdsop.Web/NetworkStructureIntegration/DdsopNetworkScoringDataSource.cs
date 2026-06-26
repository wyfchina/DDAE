using AdaptiveSopDdsop.NetworkStructure;

using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.NetworkStructureIntegration;

public sealed class DdsopNetworkScoringDataSource : INetworkScoringDataSource
{
    private readonly INetworkStructureDataSource _dataSource;

    public DdsopNetworkScoringDataSource(INetworkStructureDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public NetworkScoringDataSet LoadNetworkScoring(int horizonWeeks, DateOnly anchorDate)
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

        return new NetworkScoringDataSet(
            horizon,
            data.Request.AnchorDate,
            data.NetworkData,
            data.Families
                .Select(item => new NetworkScoringFamilySignal(
                    item.Code,
                    item.Name,
                    item.TargetServiceLevel,
                    item.TargetFlowIndex))
                .ToList(),
            data.Skus
                .Select(item =>
                {
                    var zones = DdmrpCalculator.CalculateZones(item);
                    return new NetworkScoringSkuSignal(
                        item.Sku,
                        item.Name,
                        item.Family,
                        item.Adu,
                        item.DecoupledLeadTimeDays,
                        item.VariabilityFactor,
                        item.OrderCycleDays,
                        item.MinimumOrderQuantity,
                        item.UnitCost,
                        zones.TopOfRed,
                        zones.TopOfYellow,
                        zones.TopOfGreen);
                })
                .ToList(),
            data.Demand
                .Select(item => new NetworkScoringDemandSignal(
                    item.Sku,
                    item.Week,
                    item.BaselineDemand))
                .ToList(),
            data.Resources
                .Select(item => new NetworkScoringResourceSignal(
                    item.Code,
                    item.Name))
                .ToList(),
            data.ResourceRoutings
                .Select(item => new NetworkScoringRoutingSignal(
                    item.Sku,
                    item.ResourceCode,
                    item.CapacityPerUnit))
                .ToList(),
            data.SupplierItemSources
                .Select(item => new NetworkScoringSupplierItemSignal(
                    item.Supplier,
                    item.Sku,
                    item.MaterialFamily,
                    item.UnitCost,
                    LeadTimeDays(data, item)))
                .ToList(),
            planRun.BufferProjections
                .Select(item => new NetworkScoringBufferProjectionSignal(
                    item.Sku,
                    item.Week,
                    item.BufferStatus))
                .ToList(),
            capacityLoads
                .Select(item => new NetworkScoringResourceLoadSignal(
                    item.ResourceCode,
                    item.LoadPercent))
                .ToList(),
            supplyRequirements
                .Select(item => new NetworkScoringSupplyRequirementSignal(
                    item.Supplier,
                    item.MaterialFamily,
                    item.Week,
                    item.RequiredQuantity))
                .ToList(),
            supplierCapacity
                .Select(item => new NetworkScoringSupplierCapacitySignal(
                    item.Supplier,
                    item.MaterialFamily,
                    item.RiskStatus,
                    item.Gap))
                .ToList());
    }

    private static int LeadTimeDays(NetworkStructureDataSet data, SupplierItemSource source)
    {
        var supplier = data.NetworkData.SupplierSources.FirstOrDefault(item =>
            item.ItemCode == source.Sku
            && (item.SupplierName == source.Supplier || item.SupplierCode == source.Supplier));
        if (supplier is not null)
        {
            return supplier.LeadTimeDays;
        }

        return data.SupplierCapacityWindows
            .Where(item => item.Supplier == source.Supplier && item.MaterialFamily == source.MaterialFamily)
            .Select(item => item.LeadTimeDays)
            .DefaultIfEmpty(0)
            .Max();
    }
}
