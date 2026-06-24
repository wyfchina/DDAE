namespace AdaptiveSopDdsop.Web.Domain;

public sealed class BufferTrendWorkspaceService
{
    private readonly IScenarioWorkspaceDataSource _dataSource;

    public BufferTrendWorkspaceService(IScenarioWorkspaceDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public BufferTrendWorkspaceResult GetBaseline(int horizonWeeks)
    {
        var horizon = Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52);
        var data = _dataSource.Load(new ScenarioWorkspaceDataRequest(horizon, new DateOnly(2026, 6, 1)));
        var bufferRun = DemandDrivenPlanningEngine.ProjectBuffers(data.Skus, data.Inventory, data.Demand, horizon);
        var plan = new DemandDrivenPlanResult(
            bufferRun.BufferProjections,
            bufferRun.ReplenishmentOrders,
            Array.Empty<CapacityLoadProjection>(),
            Array.Empty<ProjectedSupplyRequirement>(),
            bufferRun.Traces);

        return Build(data, "baseline", "基准方案", data.Skus, plan);
    }

    public static BufferTrendWorkspaceResult Build(
        ScenarioWorkspaceDataSet data,
        string caseId,
        string name,
        IReadOnlyList<SkuBufferSetting> skus,
        DemandDrivenPlanResult plan)
    {
        var skuMap = skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);
        var zoneMap = skus.ToDictionary(item => item.Sku, DdmrpCalculator.CalculateZones, StringComparer.Ordinal);
        var orderMap = plan.ReplenishmentOrders
            .GroupBy(item => (item.Sku, item.Week))
            .ToDictionary(
                group => group.Key,
                group => new
                {
                    Quantity = group.Sum(item => item.Quantity),
                    IsPrebuild = group.Any(item => item.Trigger == "PrebuildCampaign")
                });

        var series = plan.BufferProjections
            .Where(point => skuMap.ContainsKey(point.Sku))
            .Select(point =>
            {
                var sku = skuMap[point.Sku];
                var timePhasedAdu = CalculateTimePhasedAdu(point);
                var timePhasedZones = CalculateTimePhasedZones(sku, timePhasedAdu);
                var targetInventory = (timePhasedZones.TopOfYellow + timePhasedZones.TopOfGreen) / 2m;
                orderMap.TryGetValue((point.Sku, point.Week), out var order);
                var status = TrendStatus(point, timePhasedZones);
                return new BufferTrendSeriesPoint(
                    point.Sku,
                    point.Week,
                    data.Request.AnchorDate.AddDays((point.Week - 1) * 7).ToString("yyyy/M/d", System.Globalization.CultureInfo.InvariantCulture),
                    decimal.Round(timePhasedAdu, 1),
                    point.StartNetFlow,
                    point.Demand,
                    point.EndNetFlowBeforeReplenishment,
                    point.EndNetFlowAfterReplenishment,
                    timePhasedZones.TopOfRed,
                    timePhasedZones.TopOfYellow,
                    timePhasedZones.TopOfGreen,
                    decimal.Round(targetInventory, 0),
                    decimal.Round(point.EndNetFlowAfterReplenishment * sku.UnitCost, 0),
                    decimal.Round(order?.Quantity ?? 0m, 0),
                    (order?.Quantity ?? 0m) > 0,
                    order?.IsPrebuild ?? false,
                    status);
            })
            .OrderBy(item => item.Sku)
            .ThenBy(item => item.Week)
            .ToList();

        var weeklyCells = series
            .Select(point =>
            {
                var sku = skuMap[point.Sku];
                return new BufferWeeklyCell(
                    point.Sku,
                    sku.Name,
                    sku.Family,
                    point.Week,
                    point.EndNetFlowAfterReplenishment,
                    point.InventoryValue,
                    point.Status);
            })
            .ToList();

        var familySummaries = weeklyCells
            .GroupBy(item => item.Family)
            .Select(group => new BufferFamilySummary(
                group.Key,
                decimal.Round(group.Average(item => item.InventoryValue), 0),
                group.Count(item => item.Status == "Red"),
                group.Count(item => item.Status == "Yellow"),
                group.Count(item => item.Status == "Blue"),
                plan.ReplenishmentOrders.Count(order =>
                    skuMap.TryGetValue(order.Sku, out var sku) && sku.Family == group.Key)))
            .OrderByDescending(item => item.RedWeekCount)
            .ThenByDescending(item => item.YellowWeekCount)
            .ThenBy(item => item.Family)
            .ToList();

        var zoneBands = skus
            .Select(sku =>
            {
                var zones = zoneMap[sku.Sku];
                return new BufferZoneBand(sku.Sku, decimal.Round(zones.TopOfRed, 0), decimal.Round(zones.TopOfYellow, 0), decimal.Round(zones.TopOfGreen, 0));
            })
            .OrderBy(item => item.Sku)
            .ToList();

        var selectedSku = SelectRiskSku(series, skuMap);
        var details = skus
            .Select(sku =>
            {
                var zone = zoneBands.First(item => item.Sku == sku.Sku);
                return new BufferSkuDetail(
                    sku.Sku,
                    sku.Name,
                    sku.Family,
                    sku.Adu,
                    sku.DecoupledLeadTimeDays,
                    sku.MinimumOrderQuantity,
                    sku.OrderCycleDays,
                    sku.UnitCost,
                    zone,
                    series.Where(item => item.Sku == sku.Sku).OrderBy(item => item.Week).ToList(),
                    plan.ReplenishmentOrders.Where(item => item.Sku == sku.Sku).OrderBy(item => item.Week).ToList(),
                    plan.Traces.Where(item => item.Sku == sku.Sku).OrderBy(item => item.Week).ToList(),
                    BuildActivities(data, sku, series.Where(item => item.Sku == sku.Sku).OrderBy(item => item.Week).ToList(), plan),
                    BuildAttributes(data, sku, zoneMap[sku.Sku]),
                    BuildBufferSizing(sku, zoneMap[sku.Sku]),
                    BuildBom(data, sku, series.Where(item => item.Sku == sku.Sku).OrderBy(item => item.Week).ToList()),
                    BuildOrderDetails(data, sku, plan));
            })
            .OrderBy(item => item.Sku)
            .ToList();

        return new BufferTrendWorkspaceResult(
            caseId,
            name,
            data.Request.HorizonWeeks,
            CalculateKpis(series, plan.ReplenishmentOrders, 0m),
            series,
            zoneBands,
            new BufferTrendComparison(0m, 0m, 0, 0, 0m),
            familySummaries,
            weeklyCells,
            details,
            selectedSku);
    }

    public static BufferTrendComparison Compare(BufferTrendWorkspaceResult baseline, BufferTrendWorkspaceResult scenario)
    {
        return new BufferTrendComparison(
            decimal.Round(scenario.Kpis.AverageInventoryValue - baseline.Kpis.AverageInventoryValue, 0),
            decimal.Round(scenario.Kpis.PeakInventoryValue - baseline.Kpis.PeakInventoryValue, 0),
            scenario.Series.Count(item => item.Status == "Red") - baseline.Series.Count(item => item.Status == "Red"),
            scenario.Kpis.ReplenishmentOrderCount - baseline.Kpis.ReplenishmentOrderCount,
            decimal.Round(
                scenario.Series.Sum(item => item.ReplenishmentQuantity) - baseline.Series.Sum(item => item.ReplenishmentQuantity),
                0));
    }

    public static BufferTrendWorkspaceResult WithComparison(
        BufferTrendWorkspaceResult trend,
        BufferTrendComparison comparison)
    {
        return trend with
        {
            Comparison = comparison,
            Kpis = trend.Kpis with { InventoryValueDelta = comparison.AverageInventoryValueDelta }
        };
    }

    private static BufferTrendKpis CalculateKpis(
        IReadOnlyList<BufferTrendSeriesPoint> series,
        IReadOnlyList<ProjectedReplenishmentOrder> orders,
        decimal inventoryValueDelta)
    {
        return new BufferTrendKpis(
            series.Where(item => item.Status == "Red").Select(item => item.Sku).Distinct().Count(),
            series.Where(item => item.Status == "Yellow").Select(item => item.Sku).Distinct().Count(),
            series.Count(item => item.EndNetFlowBeforeReplenishment <= 0),
            series.Count == 0 ? 0m : decimal.Round(series.Average(item => item.InventoryValue), 0),
            series.Count == 0 ? 0m : decimal.Round(series.Max(item => item.InventoryValue), 0),
            orders.Count,
            decimal.Round(inventoryValueDelta, 0));
    }

    private static string TrendStatus(BufferProjectionPoint point, BufferZones zones)
    {
        if (point.EndNetFlowBeforeReplenishment <= zones.TopOfRed)
        {
            return "Red";
        }

        if (point.EndNetFlowBeforeReplenishment <= zones.TopOfYellow)
        {
            return "Yellow";
        }

        return point.EndNetFlowAfterReplenishment > zones.TopOfGreen ? "Blue" : "Green";
    }

    private static decimal CalculateTimePhasedAdu(BufferProjectionPoint point)
    {
        // Seed demand is modeled as one five-workday planning bucket. Time-phased ADU lets
        // the display follow the same DDMRP zone equations while still showing weekly changes.
        return Math.Max(1m, point.Demand / 5m);
    }

    private static BufferZones CalculateTimePhasedZones(SkuBufferSetting sku, decimal timePhasedAdu)
    {
        var effectiveAdu = timePhasedAdu * Math.Max(0.01m, sku.DemandAdjustmentFactor);
        var zoneAdjustment = Math.Max(0.01m, sku.ZoneAdjustmentFactor);
        var red = effectiveAdu * sku.DecoupledLeadTimeDays * sku.VariabilityFactor * zoneAdjustment;
        var yellow = effectiveAdu * sku.DecoupledLeadTimeDays * zoneAdjustment;
        var green = Math.Max(sku.MinimumOrderQuantity, effectiveAdu * sku.OrderCycleDays) * zoneAdjustment;
        return new BufferZones(
            decimal.Round(red, 0),
            decimal.Round(yellow, 0),
            decimal.Round(green, 0));
    }

    private static string SelectRiskSku(
        IReadOnlyList<BufferTrendSeriesPoint> series,
        IReadOnlyDictionary<string, SkuBufferSetting> skuMap)
    {
        return series
            .GroupBy(item => item.Sku)
            .Select(group => new
            {
                Sku = group.Key,
                RedWeeks = group.Count(item => item.Status == "Red"),
                YellowWeeks = group.Count(item => item.Status == "Yellow"),
                AverageInventoryValue = group.Average(item => item.InventoryValue)
            })
            .OrderByDescending(item => item.RedWeeks)
            .ThenByDescending(item => item.YellowWeeks)
            .ThenByDescending(item => item.AverageInventoryValue)
            .ThenBy(item => skuMap.TryGetValue(item.Sku, out var sku) ? sku.Name : item.Sku)
            .Select(item => item.Sku)
            .FirstOrDefault() ?? skuMap.Keys.OrderBy(item => item).FirstOrDefault() ?? string.Empty;
    }

    private static IReadOnlyList<SingleSkuSimulationActivity> BuildActivities(
        ScenarioWorkspaceDataSet data,
        SkuBufferSetting sku,
        IReadOnlyList<BufferTrendSeriesPoint> skuSeries,
        DemandDrivenPlanResult plan)
    {
        var traceMap = plan.Traces
            .Where(item => item.Sku == sku.Sku)
            .GroupBy(item => item.Week)
            .ToDictionary(group => group.Key, group => string.Join("；", group.Select(item => item.Explanation)));
        var replenishmentMap = plan.ReplenishmentOrders
            .Where(item => item.Sku == sku.Sku)
            .GroupBy(item => item.Week)
            .ToDictionary(group => group.Key, group => group.ToList());
        var activities = new List<SingleSkuSimulationActivity>();

        foreach (var point in skuSeries)
        {
            activities.Add(new SingleSkuSimulationActivity(
                point.Week,
                point.PeriodStartDate,
                "需求消耗",
                decimal.Round(point.Demand, 0),
                "减少净流动量",
                "未来周度需求",
                "本周需求从期初净流动量中扣减。",
                decimal.Round(point.EndNetFlowBeforeReplenishment, 0),
                point.Status,
                sku.Sku));

            if (traceMap.TryGetValue(point.Week, out var trace) && point.EndNetFlowBeforeReplenishment <= point.TopOfYellow && !point.IsReplenishment)
            {
                activities.Add(new SingleSkuSimulationActivity(
                    point.Week,
                    point.PeriodStartDate,
                    "订货周期复核",
                    0,
                    "等待",
                    "DDMRP 补货策略",
                    trace,
                    decimal.Round(point.EndNetFlowBeforeReplenishment, 0),
                    point.Status,
                    $"周期 {sku.OrderCycleDays} 天"));
            }

            if (!replenishmentMap.TryGetValue(point.Week, out var orders))
            {
                continue;
            }

            foreach (var order in orders)
            {
                activities.Add(new SingleSkuSimulationActivity(
                    point.Week,
                    point.PeriodStartDate,
                    order.Trigger == "PrebuildCampaign" ? "提前建库" : "补货订单生成",
                    decimal.Round(order.Quantity, 0),
                    "增加净流动量",
                    order.Trigger == "PrebuildCampaign" ? "场景动作" : "DDMRP 净流动量方程",
                    order.Trigger == "PrebuildCampaign"
                        ? "场景要求在峰值前提前释放补货。"
                        : "净流动量低于黄区上沿，并到达订货周期复核点。",
                    decimal.Round(point.EndNetFlowAfterReplenishment, 0),
                    point.Status,
                    $"RO-{sku.Sku}-{point.Week:00}"));
            }
        }

        return activities
            .OrderBy(item => item.Week)
            .ThenBy(item => ActivitySort(item.ActivityType))
            .ToList();
    }

    private static IReadOnlyList<SingleSkuAttribute> BuildAttributes(
        ScenarioWorkspaceDataSet data,
        SkuBufferSetting sku,
        BufferZones zones)
    {
        var inventory = data.Inventory.FirstOrDefault(item => item.Sku == sku.Sku);
        var source = data.SupplierItemSources.FirstOrDefault(item => item.Sku == sku.Sku);
        var routings = data.ResourceRoutings.Where(item => item.Sku == sku.Sku).ToList();
        var masterSetting = data.MasterSettings.FirstOrDefault(item => item.SettingId == $"MS-{sku.Sku}" || item.Target == sku.Name);
        var netFlow = (inventory?.OnHand ?? 0m) + (inventory?.OpenSupply ?? 0m) - (inventory?.QualifiedDemand ?? 0m);

        var attributes = new List<SingleSkuAttribute>
        {
            new("基础属性", "SKU", sku.Sku, "当前单 SKU 仿真的物料编码。"),
            new("基础属性", "名称", sku.Name, "当前单 SKU 仿真的物料名称。"),
            new("基础属性", "产品族", sku.Family, "用于产品族汇总、预算对照和筛选。"),
            new("DDMRP 参数", "解耦点", string.IsNullOrWhiteSpace(sku.DecouplingPoint) ? "-" : sku.DecouplingPoint, "该库存缓冲在 DDOM 网络中保护的位置。"),
            new("DDMRP 参数", "缓冲配置档案", sku.BufferProfile, "用于解释该 SKU 的红黄绿区 sizing 策略。"),
            new("DDMRP 参数", "ADU 来源", $"{sku.AduSource} / {sku.AduCalculationWindowDays} 天窗口", "ADU 应来自执行反馈或经批准的计划口径。"),
            new("DDMRP 参数", "DLT 来源", sku.DltSource, "解耦提前期来源，决定红区和黄区保护厚度。"),
            new("DDMRP 参数", "DAF", decimal.Round(sku.DemandAdjustmentFactor, 2).ToString(System.Globalization.CultureInfo.InvariantCulture), "需求调整因子，用于把已知业务事件折算进缓冲 sizing。"),
            new("DDMRP 参数", "区域调整因子", decimal.Round(sku.ZoneAdjustmentFactor, 2).ToString(System.Globalization.CultureInfo.InvariantCulture), "用于临时拉伸或压缩红黄绿区厚度。"),
            new("DDMRP 参数", "生效窗口", $"第 {sku.EffectiveFromWeek}-{sku.EffectiveThroughWeek} 周", "该参数档案在场景投影中的生效范围。"),
            new("DDMRP 参数", "治理状态", GovernanceStatusLabel(sku.ParameterStatus), "主设置治理状态。"),
            new("库存状态", "现有库存", decimal.Round(inventory?.OnHand ?? 0m, 0).ToString(System.Globalization.CultureInfo.InvariantCulture), "当前可用库存。"),
            new("库存状态", "开放供应", decimal.Round(inventory?.OpenSupply ?? 0m, 0).ToString(System.Globalization.CultureInfo.InvariantCulture), "已释放但尚未进入库存的供应。"),
            new("库存状态", "合格需求", decimal.Round(inventory?.QualifiedDemand ?? 0m, 0).ToString(System.Globalization.CultureInfo.InvariantCulture), "会进入净流动量方程的需求。"),
            new("库存状态", "当前净流动量", decimal.Round(netFlow, 0).ToString(System.Globalization.CultureInfo.InvariantCulture), "现有库存 + 开放供应 - 合格需求。"),
            new("缓冲参数", "ADU", decimal.Round(sku.Adu, 1).ToString(System.Globalization.CultureInfo.InvariantCulture), "平均日用量，是红黄绿区 sizing 的基础输入。"),
            new("缓冲参数", "DLT", $"{sku.DecoupledLeadTimeDays} 天", "解耦提前期，用于计算红区和黄区。"),
            new("缓冲参数", "变异因子", decimal.Round(sku.VariabilityFactor, 2).ToString(System.Globalization.CultureInfo.InvariantCulture), "需求与供应波动保护因子。"),
            new("缓冲参数", "MOQ", decimal.Round(sku.MinimumOrderQuantity, 0).ToString(System.Globalization.CultureInfo.InvariantCulture), "最小订货量，影响绿区大小和补货批量。"),
            new("缓冲参数", "订货周期", $"{sku.OrderCycleDays} 天", "只有到达复核周期时才会生成补货建议。"),
            new("缓冲参数", "红 / 黄 / 绿上沿", $"{decimal.Round(zones.TopOfRed, 0)} / {decimal.Round(zones.TopOfYellow, 0)} / {decimal.Round(zones.TopOfGreen, 0)}", "按 DDMRP sizing 公式计算的缓冲区边界。"),
            new("成本与供应", "单位成本", decimal.Round(sku.UnitCost, 0).ToString(System.Globalization.CultureInfo.InvariantCulture), "用于库存金额、补货金额和现金影响。"),
            new("成本与供应", "主供应商", source?.Supplier ?? "-", "当前 SKU 的供应需求归属。"),
            new("成本与供应", "物料族", source?.MaterialFamily ?? sku.Family, "供应商钻取和供应能力限制的聚合维度。"),
            new("资源占用", "关键资源", routings.Count == 0 ? "-" : string.Join("，", routings.Select(item => item.ResourceCode)), "补货订单会通过这些资源路由折算 RCCP 负荷。")
        };

        if (masterSetting is not null)
        {
            attributes.Add(new SingleSkuAttribute("治理状态", "当前主设置", masterSetting.CurrentValue, "来自 DDOM 主设置基线。"));
            attributes.Add(new SingleSkuAttribute("治理状态", "建议主设置", masterSetting.ProposedValue, "用于解释后续主设置治理建议。"));
        }

        return attributes;
    }

    private static IReadOnlyList<BufferSizingLine> BuildBufferSizing(SkuBufferSetting sku, BufferZones zones)
    {
        var effectiveAdu = sku.Adu * Math.Max(0.01m, sku.DemandAdjustmentFactor);
        var zoneAdjustment = Math.Max(0.01m, sku.ZoneAdjustmentFactor);
        var redBase = effectiveAdu * sku.DecoupledLeadTimeDays * zoneAdjustment;
        var redSafety = Math.Max(0m, zones.Red - redBase);
        return new List<BufferSizingLine>
        {
            new("有效 ADU", "ADU × DAF", decimal.Round(effectiveAdu, 1), "把已知需求事件折算到缓冲 sizing 的日均需求。"),
            new("区域调整", "Zone Adjustment Factor", decimal.Round(zoneAdjustment, 2), "对红黄绿区厚度进行临时拉伸或压缩。"),
            new("红区基础", "ADU × DAF × DLT × 区域调整因子", decimal.Round(redBase, 0), "覆盖解耦提前期内的基础消耗。"),
            new("红区安全", "红区基础 × (变异因子 - 1)", decimal.Round(redSafety, 0), "覆盖需求与供应波动。"),
            new("红区上沿", "红区基础 + 红区安全", decimal.Round(zones.TopOfRed, 0), "净流动量跌破该线时为红区风险。"),
            new("黄区大小", "ADU × DAF × DLT × 区域调整因子", decimal.Round(zones.Yellow, 0), "覆盖补货提前期内的常规需求。"),
            new("黄区上沿", "红区上沿 + 黄区大小", decimal.Round(zones.TopOfYellow, 0), "进入黄区后，仍需等到订货周期复核点才生成补货。"),
            new("绿区大小", "max(MOQ, ADU × DAF × 订货周期) × 区域调整因子", decimal.Round(zones.Green, 0), "决定补货目标和订单节奏。"),
            new("绿区上沿", "黄区上沿 + 绿区大小", decimal.Round(zones.TopOfGreen, 0), "补货建议通常补到该水位。"),
            new("目标库存", "(黄区上沿 + 绿区上沿) / 2", decimal.Round((zones.TopOfYellow + zones.TopOfGreen) / 2m, 0), "用于图形中目标库存点的管理参照。")
        };
    }

    private static IReadOnlyList<SingleSkuBomComponent> BuildBom(
        ScenarioWorkspaceDataSet data,
        SkuBufferSetting sku,
        IReadOnlyList<BufferTrendSeriesPoint> skuSeries)
    {
        var source = data.SupplierItemSources.FirstOrDefault(item => item.Sku == sku.Sku);
        var averageStatus = skuSeries.Any(item => item.Status == "Red")
            ? "Red"
            : skuSeries.Any(item => item.Status == "Yellow") ? "Yellow" : "Green";

        var components = sku.Family switch
        {
            "有效载荷" => new[]
            {
                ("OPT-DET", "空间级探测器组件", "采购件", 1m, source?.Supplier ?? "中科光电", 56, "光学载荷关键长周期件"),
                ("PAY-FPGA", "载荷处理 FPGA 板", "采购件", 2m, "Microchip Space", 84, "进口器件需关注供应承诺"),
                ("PAY-HAR", "载荷线缆束", "内制件", 1m, "航天平台总装厂", 14, "消耗星上电缆束工位")
            },
            "星载电子" => new[]
            {
                ("AV-FPGA", "空间级 FPGA / 处理板", "采购件", sku.Sku.StartsWith("AV-FPGA", StringComparison.Ordinal) ? 1m : 0.4m, "Microchip Space", 84, "进口器件供应风险"),
                ("AV-PCBA", "星载电子 PCBA", "内制件", 1m, "航天九院电子", 35, "环境应力筛选窗口"),
                ("CBL-HAR", "星上电缆束", "内制件", 1.2m, "航天平台总装厂", 21, "线缆束工位负荷")
            },
            "热控结构" => new[]
            {
                ("TC-FILM", "多层隔热膜", "采购件", 3m, "航天复材", 21, "复材供应能力"),
                ("TC-PANEL", "蜂窝板结构件", "采购件", 1m, "航天复材", 28, "热控结构长周期物料"),
                ("TC-FIX", "展开机构紧固件", "采购件", 8m, "航天平台总装厂", 14, "通用紧固件")
            },
            _ => new[]
            {
                ("BUS-STRUCT", "平台结构舱", "内制件", 1m, "航天平台总装厂", 28, "AIT 装配前置件"),
                ("BUS-POWER", "电源与电推进组件", "采购件", 1m, "航天九院电子", 42, "星载电源交付窗口"),
                ("BUS-HAR", "平台电缆束", "内制件", 1m, "航天平台总装厂", 21, "线缆束工位")
            }
        };

        return components
            .Select((item, index) => new SingleSkuBomComponent(
                item.Item1,
                item.Item2,
                1,
                item.Item3,
                item.Item4,
                item.Item5,
                item.Item6,
                index == 0 ? averageStatus : "Green",
                item.Item7))
            .ToList();
    }

    private static IReadOnlyList<SingleSkuOrderDetail> BuildOrderDetails(
        ScenarioWorkspaceDataSet data,
        SkuBufferSetting sku,
        DemandDrivenPlanResult plan)
    {
        var source = data.SupplierItemSources.FirstOrDefault(item => item.Sku == sku.Sku);
        var routing = data.ResourceRoutings.FirstOrDefault(item => item.Sku == sku.Sku);
        var resource = routing is null ? null : data.Resources.FirstOrDefault(item => item.Code == routing.ResourceCode);
        var traces = plan.Traces
            .Where(item => item.Sku == sku.Sku)
            .GroupBy(item => item.Week)
            .ToDictionary(group => group.Key, group => string.Join("；", group.Select(item => item.Explanation)));
        var demandOrders = data.Demand
            .Where(item => item.Sku == sku.Sku)
            .Select(item => new SingleSkuOrderDetail(
                $"DMD-{sku.Sku}-{item.Week:00}",
                "需求订单",
                item.Week,
                item.Week,
                item.Week,
                decimal.Round(item.BaselineDemand, 0),
                decimal.Round(item.BaselineDemand * sku.UnitCost, 0),
                "预计需求",
                "未来需求消耗",
                source?.Supplier ?? "-",
                resource?.Name ?? routing?.ResourceCode ?? "-",
                0m,
                0m,
                "作为合格需求进入净流动量投影。"));
        var replenishmentOrders = plan.ReplenishmentOrders
            .Where(item => item.Sku == sku.Sku)
            .Select(item =>
            {
                var capacityLoad = decimal.Round(item.Quantity * (routing?.CapacityPerUnit ?? 0m), 1);
                var committed = data.SupplierCapacityWindows
                    .Where(window => window.Supplier == (source?.Supplier ?? string.Empty)
                        && window.MaterialFamily == (source?.MaterialFamily ?? sku.Family)
                        && window.Week == item.Week)
                    .Select(window => window.CommittedCapacity)
                    .DefaultIfEmpty(item.Quantity)
                    .Min();
                return new SingleSkuOrderDetail(
                    $"RO-{sku.Sku}-{item.Week:00}",
                    item.Trigger == "PrebuildCampaign" ? "提前建库订单" : "补货订单",
                    item.Week,
                    Math.Max(1, item.Week - (int)Math.Ceiling(sku.DecoupledLeadTimeDays / 7m)),
                    item.Week,
                    decimal.Round(item.Quantity, 0),
                    decimal.Round(item.Value, 0),
                    item.Trigger == "PrebuildCampaign" ? "场景建议" : "计划建议",
                    item.Trigger == "PrebuildCampaign" ? "提前建库" : "订货周期复核",
                    source?.Supplier ?? "-",
                    resource?.Name ?? routing?.ResourceCode ?? "-",
                    capacityLoad,
                    decimal.Round(Math.Max(0m, item.Quantity - committed), 0),
                    traces.TryGetValue(item.Week, out var trace) ? trace : "由补货投影生成。");
            });

        return demandOrders
            .Concat(replenishmentOrders)
            .OrderBy(item => item.Week)
            .ThenBy(item => item.OrderType)
            .ToList();
    }

    private static int ActivitySort(string activityType)
    {
        return activityType switch
        {
            "需求消耗" => 0,
            "订货周期复核" => 1,
            "补货订单生成" => 2,
            "提前建库" => 3,
            _ => 9
        };
    }

    private static string GovernanceStatusLabel(string status)
    {
        return status switch
        {
            "Current" => "当前",
            "Proposed" => "待评审",
            "Reviewed" => "已评审",
            "Approved" => "已批准",
            "Effective" => "已生效",
            "Expired" => "已失效",
            _ => status
        };
    }
}
