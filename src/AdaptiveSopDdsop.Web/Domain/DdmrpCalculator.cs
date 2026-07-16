namespace AdaptiveSopDdsop.Web.Domain;

public sealed class DdmrpCalculator
{
    public static DdmrpSizingResult CalculateSizing(SkuBufferSetting sku, decimal? periodAdu = null)
    {
        if (sku.Adu <= 0m) throw new InvalidOperationException("ADU 必须大于零。");
        if (sku.DecoupledLeadTimeDays <= 0) throw new InvalidOperationException("DLT 必须大于零。");
        if (sku.LeadTimeFactor is null or <= 0m or > 1m) throw new InvalidOperationException("提前期因子必须在 0 到 1 之间。");
        if (sku.VariabilityFactor < 0m) throw new InvalidOperationException("波动因子不得小于零。");
        if (sku.DemandAdjustmentFactor <= 0m) throw new InvalidOperationException("DAF 必须大于零。");
        if (sku.ZoneAdjustmentFactor <= 0m) throw new InvalidOperationException("区域调整因子必须大于零。");
        if (sku.MinimumOrderQuantity < 0m) throw new InvalidOperationException("MOQ 不得小于零。");
        if (sku.OrderCycleDays <= 0) throw new InvalidOperationException("订货周期必须大于零。");

        var selectedAdu = Math.Max(sku.Adu, periodAdu ?? sku.Adu);
        var effectiveAdu = selectedAdu * sku.DemandAdjustmentFactor;
        var leadTimeDemand = effectiveAdu * sku.DecoupledLeadTimeDays;
        var leadTimeFactor = sku.LeadTimeFactor.Value;
        var redBase = leadTimeDemand * leadTimeFactor;
        var redSafety = redBase * sku.VariabilityFactor;
        var greenLeadTime = leadTimeDemand * leadTimeFactor;
        var greenMoq = sku.MinimumOrderQuantity;
        var greenOrderCycle = effectiveAdu * sku.OrderCycleDays;
        var greenDriver = greenOrderCycle >= greenLeadTime && greenOrderCycle >= greenMoq
            ? "OrderCycle"
            : greenMoq >= greenLeadTime ? "MinimumOrderQuantity" : "LeadTime";
        var zones = new BufferZones(
            decimal.Round((redBase + redSafety) * sku.ZoneAdjustmentFactor, 0),
            decimal.Round(leadTimeDemand * sku.ZoneAdjustmentFactor, 0),
            decimal.Round(Math.Max(greenLeadTime, Math.Max(greenMoq, greenOrderCycle)) * sku.ZoneAdjustmentFactor, 0));
        var evidence = sku.ParameterEvidenceStatus == "Complete" && !string.IsNullOrWhiteSpace(sku.ParameterSnapshotId)
            ? "Complete"
            : "EvidenceMissing";

        return new DdmrpSizingResult(
            selectedAdu,
            effectiveAdu,
            leadTimeDemand,
            leadTimeFactor,
            sku.VariabilityFactor,
            sku.ZoneAdjustmentFactor,
            redBase,
            redSafety,
            greenLeadTime,
            greenMoq,
            greenOrderCycle,
            greenDriver,
            zones,
            sku.ParameterSnapshotId,
            evidence);
    }

    public static BufferZones CalculateZones(SkuBufferSetting sku) => CalculateSizing(sku).Zones;

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
