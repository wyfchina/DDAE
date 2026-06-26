using AdaptiveSopDdsop.NetworkStructure;

public sealed class StandaloneNetworkStructureDataSource :
    INetworkStructureProductDataSource,
    INetworkGraphDataSource,
    INetworkMetricsDataSource,
    INetworkScoringDataSource
{
    public static readonly DateOnly AnchorDate = new(2026, 6, 1);

    private static readonly IReadOnlyList<StandaloneSkuSignal> FinishedGoods =
    [
        new("SAT-BUS-001", "标准小卫星平台", "卫星平台", 18m, 850_000m, "平台总装节奏缓冲", 12, 18, 10),
        new("SAT-BUS-002", "高机动卫星平台", "卫星平台", 12m, 1_050_000m, "平台总装节奏缓冲", 10, 21, 10),
        new("PAY-EO-101", "高分辨率光学载荷", "有效载荷", 9m, 2_600_000m, "光学载荷战略缓冲", 8, 28, 14),
        new("PAY-SAR-102", "合成孔径雷达载荷", "有效载荷", 7m, 3_200_000m, "SAR 载荷战略缓冲", 6, 35, 14),
        new("AV-FPGA-203", "进口空间级 FPGA 板", "星载电子", 42m, 540_000m, "进口关键件补货缓冲", 30, 42, 7),
        new("AV-OBC-202", "星务计算机", "星载电子", 28m, 680_000m, "星务电子补货缓冲", 18, 35, 7),
        new("AV-COM-201", "星载通信机", "星载电子", 24m, 620_000m, "星载通信补货缓冲", 16, 28, 7),
        new("TC-MLI-301", "多层隔热组件", "热控结构", 72m, 92_000m, "结构件标准补货缓冲", 40, 18, 7),
        new("TC-RAD-302", "蜂窝散热板", "热控结构", 54m, 180_000m, "结构件标准补货缓冲", 30, 21, 7),
        new("MECH-DEP-401", "太阳翼展开机构", "热控结构", 24m, 360_000m, "机构件受控缓冲", 12, 30, 14),
        new("CBL-HAR-402", "星上电缆束套件", "星载电子", 96m, 118_000m, "电缆束半成品超市", 80, 14, 5),
        new("CBL-CON-403", "空间级连接器包", "星载电子", 140m, 46_000m, "连接器供应缓冲", 100, 45, 7)
    ];

    public NetworkDataSet NetworkData { get; } = SatelliteManufacturingNetworkSeedData.Build(
        FinishedGoods
            .Select(item => new NetworkFinishedGoodSeedInput(
                item.Sku,
                item.Name,
                item.Family,
                item.UnitCost,
                item.BufferProfile,
                item.DecoupledLeadTimeDays,
                item.MinimumOrderQuantity,
                item.OrderCycleDays,
                "Complete"))
            .ToList());

    public NetworkStructureProductDataSet LoadNetworkStructure(NetworkStructureProductDataRequest request)
    {
        var horizon = Math.Clamp(request.HorizonWeeks <= 0 ? 12 : request.HorizonWeeks, 1, 52);
        return new NetworkStructureProductDataSet(
            request with
            {
                HorizonWeeks = horizon,
                AnchorDate = request.AnchorDate == default ? AnchorDate : request.AnchorDate
            },
            NetworkData);
    }

    public NetworkGraphDataSet LoadNetworkGraph(DateOnly anchorDate)
    {
        return new NetworkGraphDataSet(NetworkData, anchorDate);
    }

    public NetworkMetricsDataSet LoadNetworkMetrics(int horizonWeeks, DateOnly anchorDate)
    {
        return new NetworkMetricsDataSet(
            Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52),
            anchorDate,
            NetworkData,
            FinishedGoods.Select(item => new NetworkMetricSkuSignal(item.Sku, item.Adu)).ToList(),
            BuildResourceLoads(),
            BuildSupplierCapacitySignals());
    }

    public NetworkScoringDataSet LoadNetworkScoring(int horizonWeeks, DateOnly anchorDate)
    {
        var horizon = Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52);
        var skuSignals = FinishedGoods.Select(BuildSkuSignal).ToList();
        var routingSignals = NetworkData.RoutingLines
            .Select(item => new NetworkScoringRoutingSignal(item.ItemCode, item.ResourceCode, item.CapacityPerUnit))
            .ToList();
        var resourceLoads = BuildResourceLoads()
            .Select(item => new NetworkScoringResourceLoadSignal(item.ResourceCode, item.LoadPercent))
            .ToList();
        var supplyRequirements = BuildSupplyRequirements(horizon);
        var supplierCapacity = BuildSupplierCapacitySignals()
            .Select(item => new NetworkScoringSupplierCapacitySignal(
                item.Supplier,
                MaterialFamilyForSupplier(item.Supplier),
                item.RiskStatus,
                item.RiskStatus == "Red" ? 320m : item.RiskStatus == "Yellow" ? 90m : 0m))
            .ToList();

        return new NetworkScoringDataSet(
            horizon,
            anchorDate,
            NetworkData,
            BuildFamilySignals(),
            skuSignals,
            BuildDemandSignals(horizon),
            NetworkData.RoutingLines
                .Select(item => item.ResourceCode)
                .Distinct(StringComparer.Ordinal)
                .Select(code => new NetworkScoringResourceSignal(code, code))
                .ToList(),
            routingSignals,
            NetworkData.SupplierSources
                .Select(item => new NetworkScoringSupplierItemSignal(
                    item.SupplierName,
                    item.ItemCode,
                    NetworkData.Items.FirstOrDefault(master => master.ItemCode == item.ItemCode)?.Family ?? "未分类",
                    NetworkData.Items.FirstOrDefault(master => master.ItemCode == item.ItemCode)?.UnitCost ?? 0m,
                    item.LeadTimeDays))
                .ToList(),
            BuildBufferProjectionSignals(horizon),
            resourceLoads,
            supplyRequirements,
            supplierCapacity);
    }

    private static IReadOnlyList<NetworkScoringFamilySignal> BuildFamilySignals()
    {
        return FinishedGoods
            .GroupBy(item => item.Family, StringComparer.Ordinal)
            .Select(group => new NetworkScoringFamilySignal(group.Key, group.Key, 95m, group.Key == "星载电子" ? 87m : 82m))
            .ToList();
    }

    private static NetworkScoringSkuSignal BuildSkuSignal(StandaloneSkuSignal item)
    {
        var topOfRed = item.Adu * item.DecoupledLeadTimeDays * 0.45m;
        var topOfYellow = topOfRed + item.Adu * item.DecoupledLeadTimeDays * 0.55m;
        var topOfGreen = topOfYellow + Math.Max(item.MinimumOrderQuantity, item.Adu * item.OrderCycleDays);

        return new NetworkScoringSkuSignal(
            item.Sku,
            item.Name,
            item.Family,
            item.Adu,
            item.DecoupledLeadTimeDays,
            item.Family == "星载电子" ? 1.65m : 1.35m,
            item.OrderCycleDays,
            item.MinimumOrderQuantity,
            item.UnitCost,
            decimal.Round(topOfRed, 1),
            decimal.Round(topOfYellow, 1),
            decimal.Round(topOfGreen, 1));
    }

    private static IReadOnlyList<NetworkScoringDemandSignal> BuildDemandSignals(int horizonWeeks)
    {
        return FinishedGoods
            .SelectMany(item => Enumerable.Range(1, horizonWeeks).Select(week =>
            {
                var pulse = week is 4 or 8 ? 1.35m : week is 6 ? 0.75m : 1m;
                return new NetworkScoringDemandSignal(item.Sku, week, decimal.Round(item.Adu * 5m * pulse, 1));
            }))
            .ToList();
    }

    private static IReadOnlyList<NetworkScoringBufferProjectionSignal> BuildBufferProjectionSignals(int horizonWeeks)
    {
        return FinishedGoods
            .SelectMany(item => Enumerable.Range(1, horizonWeeks).Select(week =>
            {
                var status = item.Family == "星载电子" && week is 1 or 6 or 8
                    ? "Red"
                    : item.Family == "有效载荷" && week is 5 or 9
                        ? "Yellow"
                        : "Green";
                return new NetworkScoringBufferProjectionSignal(item.Sku, week, status);
            }))
            .ToList();
    }

    private static IReadOnlyList<NetworkMetricResourceLoadSignal> BuildResourceLoads()
    {
        return
        [
            new("RES-AIT", 88),
            new("RES-HARNESS", 121),
            new("RES-TVAC", 109),
            new("RES-RF", 96),
            new("RES-OPTICAL", 84),
            new("RES-THERMAL", 79)
        ];
    }

    private static IReadOnlyList<NetworkMetricSupplierCapacitySignal> BuildSupplierCapacitySignals()
    {
        return
        [
            new("Microchip Space", "Yellow"),
            new("航天九院电子", "Green"),
            new("航天复材", "Green"),
            new("中科光电", "Yellow"),
            new("结构热控材料厂", "Green")
        ];
    }

    private IReadOnlyList<NetworkScoringSupplyRequirementSignal> BuildSupplyRequirements(int horizonWeeks)
    {
        return NetworkData.SupplierSources
            .SelectMany(source => Enumerable.Range(1, horizonWeeks).Select(week =>
            {
                var family = NetworkData.Items.FirstOrDefault(item => item.ItemCode == source.ItemCode)?.Family ?? "未分类";
                var required = (week % 4 == 0 ? 1.25m : 1m) * Math.Max(1m, source.MinimumOrderQuantity * 0.35m);
                return new NetworkScoringSupplyRequirementSignal(source.SupplierName, family, week, decimal.Round(required, 1));
            }))
            .ToList();
    }

    private string MaterialFamilyForSupplier(string supplier)
    {
        return NetworkData.SupplierSources
            .Where(item => item.SupplierName == supplier)
            .Select(item => NetworkData.Items.FirstOrDefault(master => master.ItemCode == item.ItemCode)?.Family)
            .FirstOrDefault(item => !string.IsNullOrWhiteSpace(item))
            ?? "未分类";
    }

    private sealed record StandaloneSkuSignal(
        string Sku,
        string Name,
        string Family,
        decimal Adu,
        decimal UnitCost,
        string BufferProfile,
        decimal MinimumOrderQuantity,
        int DecoupledLeadTimeDays,
        int OrderCycleDays);
}
