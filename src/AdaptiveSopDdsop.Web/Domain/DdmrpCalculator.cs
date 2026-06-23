namespace AdaptiveSopDdsop.Web.Domain;

public sealed class DdmrpCalculator
{
    public static BufferZones CalculateZones(SkuBufferSetting sku)
    {
        var yellow = sku.Adu * sku.DecoupledLeadTimeDays;
        var red = yellow * sku.VariabilityFactor;
        var green = Math.Max(sku.MinimumOrderQuantity, sku.Adu * sku.OrderCycleDays);
        return new BufferZones(decimal.Round(red, 0), decimal.Round(yellow, 0), decimal.Round(green, 0));
    }

    public static decimal CalculateNetFlow(InventoryPosition position)
    {
        return position.OnHand + position.OpenSupply - position.QualifiedDemand;
    }

    public static PlanningRecommendation CalculateRecommendation(SkuBufferSetting sku, InventoryPosition position)
    {
        var zones = CalculateZones(sku);
        var netFlow = CalculateNetFlow(position);
        var status = GetBufferStatus(netFlow, zones);
        var shouldOrder = netFlow <= zones.TopOfYellow;
        var quantity = shouldOrder ? zones.TopOfGreen - netFlow : 0;
        var action = shouldOrder ? "Order" : "Observe";
        return new PlanningRecommendation(
            sku.Sku,
            action,
            netFlow,
            decimal.Round(quantity, 0),
            status,
            decimal.Round(quantity * sku.UnitCost, 2));
    }

    public static string GetBufferStatus(decimal netFlowPosition, BufferZones zones)
    {
        if (netFlowPosition <= zones.TopOfRed)
        {
            return "Red";
        }

        if (netFlowPosition <= zones.TopOfYellow)
        {
            return "Yellow";
        }

        if (netFlowPosition <= zones.TopOfGreen)
        {
            return "Green";
        }

        return "OverTopOfGreen";
    }
}
