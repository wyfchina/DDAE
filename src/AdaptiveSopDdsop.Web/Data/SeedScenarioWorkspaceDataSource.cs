using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.Data;

public sealed class SeedScenarioWorkspaceDataSource : IScenarioWorkspaceDataSource
{
    private readonly ValidationData _data;

    public SeedScenarioWorkspaceDataSource()
        : this(SeedData.Create())
    {
    }

    public SeedScenarioWorkspaceDataSource(ValidationData data)
    {
        _data = data;
    }

    public ScenarioWorkspaceDataSet Load(ScenarioWorkspaceDataRequest request)
    {
        var horizonWeeks = Math.Clamp(request.HorizonWeeks, 1, 52);
        var scopedSkus = FilterSkus(request);
        var skuCodes = scopedSkus.Select(item => item.Sku).ToHashSet(StringComparer.Ordinal);
        var familyCodes = scopedSkus.Select(item => item.Family).ToHashSet(StringComparer.Ordinal);
        var scopedRequest = request with { HorizonWeeks = horizonWeeks };

        var demand = _data.Demand
            .Where(item => skuCodes.Contains(item.Sku) && item.Week <= horizonWeeks)
            .ToList();
        var inventory = _data.Inventory
            .Where(item => skuCodes.Contains(item.Sku))
            .ToList();
        var routings = _data.ResourceRoutings
            .Where(item => skuCodes.Contains(item.Sku))
            .ToList();
        var resourceCodes = routings.Select(item => item.ResourceCode).ToHashSet(StringComparer.Ordinal);
        var resources = _data.Resources
            .Where(item => resourceCodes.Contains(item.Code))
            .ToList();
        var sources = _data.SupplierItemSources
            .Where(item => skuCodes.Contains(item.Sku))
            .ToList();

        return new ScenarioWorkspaceDataSet(
            scopedRequest,
            _data.Families.Where(item => familyCodes.Contains(item.Code)).ToList(),
            scopedSkus,
            inventory,
            demand,
            resources,
            routings,
            sources,
            BuildHistoricalDemand(scopedSkus, horizonWeeks),
            BuildBudgetBenchmarks(scopedSkus, horizonWeeks),
            BuildResourceCalendar(resources, horizonWeeks),
            BuildSupplierCapacityWindows(sources, horizonWeeks),
            BuildScenarioTemplates(scopedSkus, resources),
            BuildDdmrpParameters(scopedSkus),
            _data.MasterSettings
                .Where(item => scopedSkus.Any(sku => item.SettingId == $"MS-{sku.Sku}" || item.Target == sku.Name)
                    || item.SettingId.StartsWith("MS-DP", StringComparison.Ordinal)
                    || item.SettingType is "Time Buffer" or "Capacity Buffer")
                .ToList(),
            BuildGuardrails(),
            BuildCapacityProtections(routings),
            BuildTimeBuffers(),
            BuildControlPointProgress(horizonWeeks),
            BuildTimeBufferProductScopes(scopedSkus));
    }

    private static IReadOnlyList<CapacityProtectionDefinition> BuildCapacityProtections(
        IReadOnlyList<ResourceRouting> routings)
    {
        const string upstreamResourceCode = "RES-AIT";
        const string protectedCcrResourceCode = "RES-HARNESS";
        var hasCompleteSequenceEvidence = routings
            .Where(item =>
                ProtectionProductEligibility.IsEligible(item.Sku) &&
                item.ResourceCode == upstreamResourceCode &&
                item.ProtectsCcrResourceCode == protectedCcrResourceCode &&
                item.OperationSequence > 0 &&
                item.EvidenceStatus == "Complete")
            .Any(upstream => routings.Any(downstream =>
                downstream.Sku == upstream.Sku &&
                ProtectionProductEligibility.IsEligible(downstream.Sku) &&
                downstream.ResourceCode == protectedCcrResourceCode &&
                downstream.OperationSequence > upstream.OperationSequence &&
                downstream.EvidenceStatus == "Complete"));

        return hasCompleteSequenceEvidence
            ? new[]
            {
                new CapacityProtectionDefinition(
                    "CAP-PROT-AIT-HARNESS-001",
                    upstreamResourceCode,
                    protectedCcrResourceCode,
                    20m,
                    "热控结构件经 AIT 前置工序进入 HARNESS 当前 CCR 的顺序路径",
                    "Complete")
            }
            : Array.Empty<CapacityProtectionDefinition>();
    }

    private static IReadOnlyList<TimeBufferDefinition> BuildTimeBuffers()
    {
        return new[]
        {
            new TimeBufferDefinition(
                "MS-TB-001",
                "热真空试验准备控制点",
                "热真空试验",
                3m,
                true,
                "卫星平台与有效载荷的热真空试验准备",
                "Complete")
        };
    }

    private static IReadOnlyList<ControlPointProgressFact> BuildControlPointProgress(int horizonWeeks)
    {
        return Enumerable.Range(1, horizonWeeks)
            .Select(week => week switch
            {
                1 => new ControlPointProgressFact("MS-TB-001", week, 0.5m, "试验件按计划进入准备窗口", "Complete"),
                2 => new ControlPointProgressFact("MS-TB-001", week, 1.0m, "洁净转运窗口调整", "Complete"),
                3 => new ControlPointProgressFact("MS-TB-001", week, 0m, "准备工作恢复计划节奏", "Complete"),
                _ => new ControlPointProgressFact("MS-TB-001", week, 0m, "展望期内按计划，无基线延迟", "Complete")
            })
            .ToList();
    }

    private static IReadOnlyList<TimeBufferProductScope> BuildTimeBufferProductScopes(
        IReadOnlyList<SkuBufferSetting> skus)
    {
        var scopedSkus = skus
            .Where(item => item.Family is "卫星平台" or "有效载荷")
            .ToList();
        return new[]
        {
            new TimeBufferProductScope(
                "MS-TB-001",
                scopedSkus.Select(item => item.Family).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
                scopedSkus.Select(item => item.Sku).OrderBy(item => item, StringComparer.Ordinal).ToList(),
                "Complete")
        };
    }

    private static IReadOnlyList<DdmrpParameterProfile> BuildDdmrpParameters(IReadOnlyList<SkuBufferSetting> skus)
    {
        return skus
            .Select(sku =>
            {
                var sizing = DdmrpCalculator.CalculateSizing(sku);
                var missing = new List<string>();
                if (string.IsNullOrWhiteSpace(sku.DecouplingPoint))
                {
                    missing.Add("解耦点");
                }

                if (string.IsNullOrWhiteSpace(sku.BufferProfile))
                {
                    missing.Add("缓冲配置档案");
                }

                if (sku.Adu <= 0)
                {
                    missing.Add("ADU");
                }

                if (sku.DecoupledLeadTimeDays <= 0)
                {
                    missing.Add("DLT");
                }

                if (sku.VariabilityFactor <= 0)
                {
                    missing.Add("波动因子");
                }

                if (sku.LeadTimeFactor is null or <= 0m or > 1m)
                {
                    missing.Add("提前期因子");
                }

                if (string.IsNullOrWhiteSpace(sku.ParameterSnapshotId))
                {
                    missing.Add("参数快照");
                }

                if (sku.ParameterEvidenceStatus != "Complete")
                {
                    missing.Add("参数证据");
                }

                if (sku.OrderCycleDays <= 0)
                {
                    missing.Add("订货周期");
                }

                if (sku.MinimumOrderQuantity < 0)
                {
                    missing.Add("MOQ");
                }

                var completenessStatus = missing.Count == 0 ? "Complete" : "Incomplete";
                var validationMessage = missing.Count == 0
                    ? "DDMRP 参数完整，可用于缓冲定容、补货投影、RCCP 和主设置治理。"
                    : $"缺少 {string.Join("、", missing)}，不得进入正式主设置下传。";

                return new DdmrpParameterProfile(
                    sku.Sku,
                    sku.Name,
                    sku.Family,
                    sku.DecouplingPoint,
                    sku.BufferProfile,
                    sku.Adu,
                    sku.AduSource,
                    sku.AduCalculationWindowDays,
                    sku.DecoupledLeadTimeDays,
                    sku.DltSource,
                    sku.VariabilityFactor,
                    sku.DemandAdjustmentFactor,
                    sku.ZoneAdjustmentFactor,
                    sku.MinimumOrderQuantity,
                    sku.OrderCycleDays,
                    sku.UnitCost,
                    sku.WeeklyCapacityUnits,
                    sizing.Zones.TopOfRed,
                    sizing.Zones.TopOfYellow,
                    sizing.Zones.TopOfGreen,
                    sku.EffectiveFromWeek,
                    sku.EffectiveThroughWeek,
                    sku.ParameterStatus,
                    completenessStatus,
                    validationMessage,
                    sku.LeadTimeFactor,
                    sku.ParameterSnapshotId,
                    sizing.EvidenceStatus,
                    sizing,
                    DdmrpSizingExplanation.Build(sizing));
            })
            .OrderBy(item => item.Family)
            .ThenBy(item => item.Sku)
            .ToList();
    }

    private IReadOnlyList<SkuBufferSetting> FilterSkus(ScenarioWorkspaceDataRequest request)
    {
        var skus = _data.Skus.AsEnumerable();

        if (request.SkuFilter is { Count: > 0 })
        {
            var skuFilter = request.SkuFilter.ToHashSet(StringComparer.Ordinal);
            skus = skus.Where(item => skuFilter.Contains(item.Sku));
        }

        if (request.FamilyFilter is { Count: > 0 })
        {
            var familyFilter = request.FamilyFilter.ToHashSet(StringComparer.Ordinal);
            skus = skus.Where(item => familyFilter.Contains(item.Family));
        }

        return skus.ToList();
    }

    private static IReadOnlyList<HistoricalDemandActual> BuildHistoricalDemand(
        IReadOnlyList<SkuBufferSetting> skus,
        int horizonWeeks)
    {
        var historyWeeks = Math.Min(52, Math.Max(4, horizonWeeks));
        return skus
            .SelectMany(sku => Enumerable.Range(1, historyWeeks).Select(week =>
            {
                var pattern = (week % 4) switch
                {
                    0 => 1.18m,
                    1 => 0.92m,
                    2 => 1.04m,
                    _ => 0.98m
                };
                var forecast = decimal.Round(sku.Adu * 5m, 0);
                var actual = decimal.Round(forecast * pattern, 0);
                var service = pattern > 1.12m && sku.Family is "星载电子" ? 92.5m : 97.2m;
                var netFlow = decimal.Round(sku.Adu * sku.DecoupledLeadTimeDays * (1.9m - (week % 3) * 0.18m), 0);
                return new HistoricalDemandActual(sku.Sku, -week, actual, forecast, service, netFlow);
            }))
            .ToList();
    }

    private static IReadOnlyList<BudgetBenchmark> BuildBudgetBenchmarks(
        IReadOnlyList<SkuBufferSetting> skus,
        int horizonWeeks)
    {
        return skus
            .GroupBy(item => item.Family)
            .SelectMany(group => Enumerable.Range(1, horizonWeeks).Select(week =>
            {
                var weeklyRevenue = group.Sum(sku => sku.Adu * 5m * sku.UnitCost * 1.35m);
                var budgetRevenue = decimal.Round(weeklyRevenue * (1 + week * 0.006m), 0);
                var lastYearRevenue = decimal.Round(budgetRevenue * 0.91m, 0);
                var budgetInventory = decimal.Round(group.Sum(sku => sku.Adu * sku.DecoupledLeadTimeDays * sku.UnitCost * 1.4m), 0);
                var lastYearInventory = decimal.Round(budgetInventory * 0.96m, 0);
                return new BudgetBenchmark(group.Key, week, budgetRevenue, lastYearRevenue, budgetInventory, lastYearInventory);
            }))
            .ToList();
    }

    private static IReadOnlyList<ResourceCalendarEntry> BuildResourceCalendar(
        IReadOnlyList<CapacityResource> resources,
        int horizonWeeks)
    {
        return resources
            .SelectMany(resource => Enumerable.Range(1, horizonWeeks).Select(week =>
            {
                var multiplier = resource.Code == "RES-TVAC" && week == 5
                    ? 0.55m
                    : resource.Code == "RES-AIT" && week is >= 8 and <= 9
                        ? 1.25m
                        : 1.00m;
                var note = multiplier < 1
                    ? "计划检修"
                    : multiplier > 1
                        ? "临时增班"
                        : "标准日历";
                return new ResourceCalendarEntry(resource.Code, week, multiplier, note);
            }))
            .ToList();
    }

    private static IReadOnlyList<SupplierCapacityWindow> BuildSupplierCapacityWindows(
        IReadOnlyList<SupplierItemSource> sources,
        int horizonWeeks)
    {
        return sources
            .GroupBy(item => new { item.Supplier, item.MaterialFamily })
            .SelectMany(group => Enumerable.Range(1, horizonWeeks).Select(week =>
            {
                var isImportedFpga = group.Key.MaterialFamily == "进口空间级 FPGA";
                var committed = isImportedFpga
                    ? 1700m - (week is >= 6 and <= 8 ? 420m : 0m)
                    : 5200m;
                var risk = isImportedFpga && week is >= 6 and <= 8
                    ? "Red"
                    : isImportedFpga
                        ? "Yellow"
                        : "Green";
                var leadTime = isImportedFpga ? 84 : 21;
                return new SupplierCapacityWindow(group.Key.Supplier, group.Key.MaterialFamily, week, committed, leadTime, risk);
            }))
            .ToList();
    }

    private static IReadOnlyList<ScenarioTemplate> BuildScenarioTemplates(
        IReadOnlyList<SkuBufferSetting> skus,
        IReadOnlyList<CapacityResource> resources)
    {
        if (skus.Count == 0)
        {
            return Array.Empty<ScenarioTemplate>();
        }

        var peakSku = skus.FirstOrDefault(item => item.Family == "星载电子") ?? skus.First();
        var resource = resources.FirstOrDefault(item => item.Code == "RES-TVAC")
            ?? resources.FirstOrDefault()
            ?? new CapacityResource("RES-DEFAULT", "默认资源", 1, 1);

        return new List<ScenarioTemplate>
        {
            new(
                "TPL-PREBUILD-PEAK",
                "促销峰值提前建库",
                "用淡季库存吸收未来需求峰值，验证削峰填谷效果。",
                new[]
                {
                    new ScenarioTemplateAction("Prebuild", peakSku.Sku, 2, 2, peakSku.MinimumOrderQuantity, "units"),
                    new ScenarioTemplateAction("DemandEvent", peakSku.Family, 6, 8, 1.18m, "factor")
                }),
            new(
                "TPL-CAPACITY-RELIEF",
                "瓶颈资源临时增班",
                "验证加班、增班或外协是否足以消除 RCCP 红区。",
                new[]
                {
                    new ScenarioTemplateAction("CapacityMultiplier", resource.Code, 6, 8, 1.25m, "factor")
                }),
            new(
                "TPL-ORDER-POLICY",
                "MOQ 与订货周期调整",
                "比较订单频率、平均库存和服务风险之间的取舍。",
                new[]
                {
                    new ScenarioTemplateAction("MoqOverride", peakSku.Sku, 1, 12, peakSku.MinimumOrderQuantity * 1.2m, "units"),
                    new ScenarioTemplateAction("OrderCycleOverride", peakSku.Sku, 1, 12, 7m, "days")
                }),
            new(
                "TPL-CONSTRAINED",
                "受限与不受限计划对比",
                "在供应和资源约束下验证业务计划能否达成。",
                new[]
                {
                    new ScenarioTemplateAction("SupplierCapacityLimit", "进口空间级 FPGA", 6, 8, 1280m, "units/week"),
                    new ScenarioTemplateAction("CapacityMultiplier", resource.Code, 5, 5, 0.55m, "factor")
                })
        };
    }

    private static IReadOnlyList<BusinessGuardrail> BuildGuardrails()
    {
        return new List<BusinessGuardrail>
        {
            new("服务水平损失", 1m, 3m, "百分点", "红色时不得由 DDS&OP 直接采纳，必须升级到集成协调评审。"),
            new("营运资金增幅", 5m, 12m, "%", "超过预算红线时需要财务确认现金占用。"),
            new("资源负载率", 85m, 100m, "%", "超过 100% 时必须提出产能、外协或需求取舍动作。"),
            new("供应缺口", 5m, 15m, "%", "红色供应缺口需要供应商协同或客户分配规则。"),
            new("红区穿透周数", 1m, 3m, "周", "连续红区需要重审 DDOM 主设置。")
        };
    }
}
