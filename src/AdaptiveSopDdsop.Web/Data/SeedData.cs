using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.Data;

public static class SeedData
{
    public static ValidationData Create()
    {
        var families = new List<ProductFamily>
        {
            new("卫星平台", "小卫星平台与服务舱产品族", 96, 88, 1_280_000),
            new("有效载荷", "遥感与通信载荷产品族", 95, 84, 1_760_000),
            new("星载电子", "星载电子与电缆产品族", 97, 90, 680_000),
            new("热控结构", "热控结构与展开机构产品族", 96, 86, 520_000),
        };

        var skus = new List<SkuBufferSetting>
        {
            new("SAT-BUS-001", "标准小卫星平台", "卫星平台", 18, 8, 1.5m, 4, 80, 820_000m, 210),
            new("SAT-BUS-002", "高机动卫星平台", "卫星平台", 12, 10, 1.6m, 5, 60, 1_180_000m, 160),
            new("SAT-PROP-003", "电推进模块", "卫星平台", 26, 7, 1.4m, 4, 90, 260_000m, 260),
            new("PAY-EO-101", "高分辨率光学载荷", "有效载荷", 10, 12, 1.9m, 6, 45, 1_960_000m, 120),
            new("PAY-SAR-102", "合成孔径雷达载荷", "有效载荷", 7, 14, 2.1m, 6, 35, 2_850_000m, 90),
            new("AV-COM-201", "星载通信机", "星载电子", 42, 6, 1.4m, 4, 160, 180_000m, 520),
            new("AV-OBC-202", "星务计算机", "星载电子", 30, 8, 1.7m, 5, 120, 320_000m, 360),
            new("AV-FPGA-203", "进口空间级 FPGA 板", "星载电子", 16, 16, 2.2m, 6, 80, 540_000m, 180),
            new("TC-MLI-301", "多层隔热组件", "热控结构", 72, 5, 1.4m, 4, 260, 42_000m, 760),
            new("TC-RAD-302", "蜂窝散热板", "热控结构", 54, 6, 1.5m, 4, 220, 86_000m, 620),
            new("MECH-DEP-401", "太阳翼展开机构", "热控结构", 24, 11, 1.9m, 5, 100, 420_000m, 240),
            new("CBL-HAR-402", "星上电缆束套件", "星载电子", 96, 4, 1.2m, 3, 360, 38_000m, 980),
        }.Select(ApplyDdmrpParameterProfile).ToList();

        var inventory = new List<InventoryPosition>
        {
            new("SAT-BUS-001", 92, 42, 36),
            new("SAT-BUS-002", 460, 90, 28),
            new("SAT-PROP-003", 260, 90, 84),
            new("PAY-EO-101", 280, 24, 42),
            new("PAY-SAR-102", 74, 14, 28),
            new("AV-COM-201", 210, 86, 98),
            new("AV-OBC-202", 238, 56, 74),
            new("AV-FPGA-203", 126, 18, 82),
            new("TC-MLI-301", 980, 200, 220),
            new("TC-RAD-302", 460, 120, 180),
            new("MECH-DEP-401", 168, 28, 76),
            new("CBL-HAR-402", 1360, 320, 310),
        };

        var demandShape = new[] { 0.72m, 0.92m, 1.18m, 1.48m, 0.80m, 1.05m, 1.32m, 1.62m, 0.76m, 0.98m, 1.24m, 1.52m };
        var demand = skus
            .SelectMany(sku => Enumerable.Range(1, 12)
                .Select(week => new WeeklyDemand(
                    sku.Sku,
                    week,
                    decimal.Round(sku.Adu * 5m * demandShape[week - 1] * FamilyDemandFactor(sku.Family, week), 0))))
            .ToList();

        var resources = new List<CapacityResource>
        {
            new("RES-AIT", "AIT 总装集成大厅", 1380, 1.00m),
            new("RES-TVAC", "热真空试验舱", 920, 0.85m),
            new("RES-CLEAN", "洁净载荷装配间", 1180, 0.72m),
            new("RES-HARNESS", "星上电缆束工位", 1680, 0.68m),
        };

        var resourceRoutings = skus.SelectMany(sku => sku.Family switch
        {
            "星载电子" => new[]
            {
                new ResourceRouting(sku.Sku, "RES-HARNESS", 0.52m),
                new ResourceRouting(sku.Sku, "RES-TVAC", 0.20m),
            },
            "有效载荷" => new[]
            {
                new ResourceRouting(sku.Sku, "RES-CLEAN", 0.70m),
                new ResourceRouting(sku.Sku, "RES-TVAC", 0.42m),
            },
            "热控结构" => new[]
            {
                new ResourceRouting(sku.Sku, "RES-AIT", 0.30m),
                new ResourceRouting(sku.Sku, "RES-HARNESS", 0.18m),
            },
            _ => new[]
            {
                new ResourceRouting(sku.Sku, "RES-AIT", 0.56m),
                new ResourceRouting(sku.Sku, "RES-TVAC", 0.24m),
            }
        }).ToList();

        var supplierItemSources = skus.Select(sku =>
        {
            var supplier = sku.Sku.StartsWith("AV-FPGA", StringComparison.Ordinal)
                ? "Microchip Space"
                : sku.Family switch
                {
                    "星载电子" => "航天九院电子",
                    "有效载荷" => "中科光电",
                    "热控结构" => "航天复材",
                    _ => "航天平台总装厂"
                };
            var family = sku.Sku.StartsWith("AV-FPGA", StringComparison.Ordinal)
                ? "进口空间级 FPGA"
                : sku.Family;
            return new SupplierItemSource(supplier, sku.Sku, family, sku.UnitCost);
        }).ToList();

        var strategicMonths = Enumerable.Range(1, 36)
            .Select(index =>
            {
                var month = new DateOnly(2026, 6, 1).AddMonths(index - 1);
                return new StrategicMonth(index, $"{month:yyyy-MM}", month.Year, month.Month);
            })
            .ToList();

        var asopSteps = new List<ProcessStep>
        {
            new(1, "PORTFOLIO", "产品组合与新活动审查", "CEO / 市场 / 研发", "确认卫星平台、载荷、型号生命周期和星座批产计划是否支撑长期增长。", "组合决策与型号缺口"),
            new(2, "DEMAND", "需求计划审查", "销售 / 市场", "形成按产品族、轨道任务和客户星座批次聚合的需求范围。", "需求范围与交付风险"),
            new(3, "SUPPLY", "供应计划审查", "运营 / 采购 / 制造", "用 AIT、热真空、洁净装配和关键器件约束检查可行性。", "供应可行性与资源缺口"),
            new(4, "FINANCIAL", "财务计划审查", "CFO / 财务", "把卫星交付计划货币化为收入、贡献毛利、现金和 ROI。", "财务缺口与投资边界"),
            new(5, "RECON", "集成协调", "总经理 / 集成负责人", "把需求、供应、财务、型号组合冲突拉到同一张取舍表。", "Single Game Plan 候选方案"),
            new(6, "DDSOP", "DDS&OP 战术衔接", "供应链 / 运营模型团队", "把战略参数转成 DDOM Master Settings，并回传运营现实。", "主设置变更与可行性反馈"),
            new(7, "REVIEW", "管理层决策评审", "CEO / C-Level", "批准公司级一个计划和超战术权限事项。", "Single Game Plan 与决策记录"),
        };

        var ddsopElements = new List<ProcessStep>
        {
            new(1, "TACTICAL_REVIEW", "战术回顾", "DDS&OP", "审计过去周期内 DDOM 的可靠性、稳定性、流速表现。", "Variance Analysis"),
            new(2, "TACTICAL_PROJECTION", "战术投影", "DDS&OP", "评估近期发射窗口、器件到货、试验资源对缓冲、能力和现金的影响。", "Tactical Projection"),
            new(3, "CONFIG", "战术配置与对齐", "DDS&OP", "调整解耦点、DAF、库存/时间/产能缓冲等主设置。", "Master Settings Change"),
            new(4, "EXPLOIT", "战术开拓", "DDS&OP / 销售 / 物流", "利用短期富余试验能力或器件窗口换取流动红利。", "Tactical Opportunity"),
            new(5, "RECOMMEND", "战略建议", "DDS&OP", "将超出战术权限的系统性缺陷升级到 AS&OP。", "Strategic Recommendation"),
            new(6, "PROJECT", "战略投影", "DDS&OP", "对远期星座批产情景做运营可行性和 DDOM 承压校验。", "Feasibility Check"),
        };

        var portfolioItems = new List<PortfolioItem>
        {
            new("NPI-001", "SAR 小卫星批产平台", "有效载荷", "Validation", "批准小批验证", 180_000_000, 34, "Yellow", "进口空间级 FPGA DLT 高，需提前配置缓冲"),
            new("NPI-002", "高通量通信载荷", "有效载荷", "Development", "继续开发", 220_000_000, 41, "Green", "洁净载荷装配间为主要约束"),
            new("NPI-003", "轻量化平台结构舱", "卫星平台", "Concept", "进入立项评审", 260_000_000, 38, "Yellow", "AIT 与热真空试验能力待校验"),
            new("EOL-101", "旧版星务计算机", "星载电子", "End-of-Life", "最后采购并清库存", 32_000_000, 18, "Red", "器件停产且库存周转慢"),
            new("MAT-201", "成熟 MLI 隔热组件", "热控结构", "Mature", "维持并降库存", 125_000_000, 29, "Green", "缓冲超绿，可释放现金"),
            new("DECL-301", "旧款通信机", "星载电子", "Decline", "合并规格", 48_000_000, 21, "Yellow", "长尾 SKU 占用热真空资源"),
        };

        var financialProjections = strategicMonths
            .SelectMany(month => families.Select(family =>
            {
                var baseUnits = family.Code switch
                {
                    "卫星平台" => 42m,
                    "有效载荷" => 24m,
                    "星载电子" => 128m,
                    "热控结构" => 186m,
                    _ => 60m
                };
                var growth = 1 + month.Index * 0.012m;
                var units = decimal.Round(baseUnits * growth, 0);
                var revenue = units * family.RevenuePerUnit;
                var margin = revenue * (family.Code == "有效载荷" ? 0.29m : family.Code == "星载电子" ? 0.36m : 0.31m);
                var workingCapital = revenue * (family.Code == "星载电子" ? 0.20m : 0.15m);
                var roi = decimal.Round((margin - workingCapital * 0.03m) * 100m / Math.Max(workingCapital, 1), 1);
                var cashGap = month.Index > 18 && family.Code == "星载电子" ? 6_500_000 : month.Index > 24 ? 4_200_000 : 0;
                return new FinancialProjection(month.Index, month.Label, family.Code, units, decimal.Round(revenue, 0), decimal.Round(margin, 0), decimal.Round(workingCapital, 0), roi, cashGap);
            }))
            .ToList();

        var resourceProfiles = strategicMonths
            .Take(24)
            .SelectMany(month => new[]
            {
                ResourceRow(month, "AIT 总装集成大厅", "卫星平台", 840 + month.Index * 22, 1380),
                ResourceRow(month, "热真空试验舱", "有效载荷", 680 + month.Index * 26, 920),
                ResourceRow(month, "洁净载荷装配间", "有效载荷", 720 + month.Index * 24, 1180),
                ResourceRow(month, "星上电缆束工位", "星载电子", 980 + month.Index * 34, 1680),
            })
            .ToList();

        var supplierConstraints = new List<SupplierConstraint>
        {
            new("Microchip Space", "进口空间级 FPGA", 720, 980, 84, "Red", "锁定长周期采购并开发国产替代板卡"),
            new("中科光电", "光学载荷组件", 420, 390, 56, "Yellow", "锁定 16 周滚动承诺"),
            new("航天复材", "蜂窝板与热控材料", 2600, 2180, 21, "Green", "维持月度产能预约"),
            new("航天九院电子", "星载电子单机", 1680, 1820, 35, "Yellow", "增加环境应力筛选窗口"),
        };

        var capitalRequirements = new List<CapitalRequirement>
        {
            new("CAP-001", "新增中型热真空试验舱", "2027-03", "热真空试验舱", 48_000_000, 260, 28, 22, "Submitted"),
            new("CAP-002", "AIT 第三班次试验团队", "2026-11", "AIT 总装集成大厅", 12_000_000, 180, 34, 9, "Proposed"),
            new("CAP-003", "洁净载荷装配间扩容", "2027-06", "洁净载荷装配间", 36_000_000, 220, 19, 28, "Evaluate"),
            new("CAP-004", "星载电缆半成品超市扩建", "2027-01", "仓储空间", 22_000_000, 320, 24, 18, "Proposed"),
        };

        var knownEvents = new List<KnownEvent>
        {
            new("EVT-001", "Launch Window", "Q4 星座发射窗口集中交付", "2026-10 至 2026-12", "卫星平台 / 有效载荷", 1.18m, 1.10m, "销售", "Approved"),
            new("EVT-002", "Planned Shutdown", "热真空试验舱年度校准", "2026-09 第 2 周", "热真空试验舱", 1.00m, 1.22m, "制造", "Approved"),
            new("EVT-003", "NPI Ramp", "SAR 小卫星批产爬坡", "2027-01 至 2027-06", "NPI-001", 1.35m, 1.25m, "研发/运营", "Proposed"),
            new("EVT-004", "Supplier Disruption", "进口空间级 FPGA 出口许可延迟", "2026-08 至 2026-09", "进口空间级 FPGA", 1.00m, 1.30m, "采购", "Reviewed"),
            new("EVT-005", "Regional Surge", "低轨通信客户追加星座批次", "2027-04 至 2027-08", "星载电子", 1.16m, 1.08m, "销售", "Draft"),
        };

        var masterSettings = skus.Select(sku =>
        {
            var proposedDaf = sku.Family == "星载电子" ? "DAF 1.18 / Zone 1.10" : sku.Family == "有效载荷" ? "DAF 1.08 / Zone 1.05" : "DAF 1.00 / Zone 1.00";
            return new MasterSetting(
                $"MS-{sku.Sku}",
                "Inventory Buffer",
                sku.Name,
                FormatDdmrpCurrentValue(sku),
                $"{proposedDaf}，{sku.DecouplingPoint}，{sku.BufferProfile}",
                sku.Family == "星载电子" ? "发射窗口/进口器件风险" : "滚动需求与实际 ADU 偏差",
                "2026-08 至 2026-12",
                sku.Family == "星载电子" ? "Proposed" : "Current",
                sku.Family == "星载电子" ? 2.4m : 0.6m,
                sku.Family == "星载电子" ? 1_800_000 : 350_000);
        }).Concat(new[]
        {
            new MasterSetting("MS-DP-001", "Decoupling Point", "星载电子半成品解耦点", "未设置", "设置星载电子半成品 Buffer", "星座批产长期大单", "2027-01 生效", "Proposed", 3.2m, 1_116_500),
            new MasterSetting("MS-TB-001", "Time Buffer", "进口空间级 FPGA", "保护 8 周", "保护 12 周 + Act/Late 阈值", "出口许可延迟与长交期", "2026-08 至 2026-10", "Reviewed", 2.0m, 2_400_000),
            new MasterSetting("MS-CB-001", "Capacity Buffer", "热真空试验舱", "保留 5%", "保留 12% + 周末班边界", "试验舱校准与发射窗口叠加", "2026-09 至 2026-12", "Proposed", 2.8m, 3_100_000),
        }).ToList();

        var ddomFeedback = Enumerable.Range(1, 18)
            .SelectMany(period => new[]
            {
                Feedback(period, "进口空间级 FPGA", 92 - period % 4, 78 - period % 5, 74 - period % 6, 1 + period % 3, period % 4 == 0 ? 1 : 0, 2 + period % 3, period % 5 == 0 ? 1 : 0, 15 + period, 91 + period % 8, "Supply"),
                Feedback(period, "热真空试验舱", 96 - period % 3, 82 - period % 4, 79 - period % 5, period % 4, 0, 1 + period % 2, period % 6 == 0 ? 1 : 0, 10 + period, 84 + period % 14, "Capacity"),
                Feedback(period, "星上电缆束套件", 94 - period % 5, 86 - period % 6, 83 - period % 4, period % 2, 0, period % 3, 0, 88 + period, 76 + period % 12, "Demand"),
            })
            .ToList();

        var tacticalOpportunities = new List<TacticalOpportunity>
        {
            new("TO-001", "利用 MLI 超绿缓冲释放现金", "TC-MLI-301 超绿区持续 5 周", 0, 0, -1_800_000, 1.5m, "服务风险低", "Candidate"),
            new("TO-002", "热真空周末短促试验批", "发射窗口前两周仍有 11% 保护能力", 12_500_000, 3_900_000, 950_000, 2.8m, "需控制加班成本", "Evaluate"),
            new("TO-003", "航天复材慢运替代部分空运", "服务风险低且缓冲恢复", 0, 0, -2_600_000, 1.1m, "Late 警报不能超过阈值", "Approved"),
            new("TO-004", "高毛利载荷抢单窗口", "洁净装配间绿区且 SAR 载荷毛利高", 18_000_000, 6_200_000, 1_400_000, 3.4m, "需销售确认客户交付窗口", "Candidate"),
        };

        var strategicRecommendations = new List<StrategicRecommendation>
        {
            new("SR-001", "Capital", "新增中型热真空试验舱", "热真空 2027-Q1 后连续红区", 48_000_000, 20_500_000, 28, 5.4m, "COO/CFO", "Submitted"),
            new("SR-002", "Engineering", "国产空间级 FPGA 替代设计", "进口 FPGA 反复 Act/Late", 9_000_000, 14_500_000, 35, 4.1m, "CTO/VP Engineering", "Proposed"),
            new("SR-003", "Capacity", "AIT 第三班次", "平台批产与发射窗口重叠", 12_000_000, 9_800_000, 34, 3.2m, "COO/HR", "Evaluate"),
            new("SR-004", "Supplier", "开发第二家光学载荷组件供应商", "单一来源影响两个星座批次", 6_500_000, 7_400_000, 22, 2.6m, "VP Supply Chain", "Draft"),
        };

        var feasibilityChecks = new List<FeasibilityCheck>
        {
            new("低轨星座长期大单", "2027-01 至 2028-12", 118, 109, 18_600_000, 420, 95, 23_000_000, "有条件可行", "新增星载电子解耦点并批准热真空扩容"),
            new("SAR 小卫星全新系列", "2027-03 至 2028-12", 126, 114, 22_400_000, 510, 138, 31_000_000, "不可行", "先完成 FPGA 国产替代和洁净间扩建"),
            new("通信客户追加星座批次", "2026-11 至 2027-12", 92, 86, 7_400_000, 160, 0, 4_200_000, "可行", "启用 DAF 1.16 并锁定热真空保护能力"),
            new("不新增资本投资的高服务承诺", "2027-01 至 2028-06", 135, 122, 29_000_000, 620, 210, 46_000_000, "不可行", "必须降低服务承诺或批准固定资产"),
        };

        var skillBuffers = new List<SkillBuffer>
        {
            new("热真空试验团队", "热真空试验方案与故障复测", 4, 6, "Yellow", "培养 2 名多能工并建立周末班资格"),
            new("AIT 总装团队", "星箭接口与总装集成", 3, 5, "Red", "安排认证培训并外部招聘 1 人"),
            new("供应链计划组", "DDOM Master Settings 管理", 2, 4, "Yellow", "完成 DDI/DDS&OP 方法培训"),
            new("研发工程组", "空间级器件替代设计", 5, 5, "Green", "维持当前能力"),
        };
        return new ValidationData(
            families,
            skus,
            inventory,
            demand,
            resources,
            resourceRoutings,
            supplierItemSources,
            strategicMonths,
            asopSteps,
            ddsopElements,
            portfolioItems,
            financialProjections,
            resourceProfiles,
            supplierConstraints,
            capitalRequirements,
            masterSettings,
            knownEvents,
            ddomFeedback,
            tacticalOpportunities,
            strategicRecommendations,
            feasibilityChecks,
            skillBuffers);
    }

    private static SkuBufferSetting ApplyDdmrpParameterProfile(SkuBufferSetting sku)
    {
        var daf = sku.Family switch
        {
            "星载电子" => 1.18m,
            "有效载荷" => 1.08m,
            "卫星平台" => 1.04m,
            _ => 1.00m
        };
        var zoneAdjustment = sku.Family switch
        {
            "星载电子" => 1.10m,
            "有效载荷" => 1.05m,
            "卫星平台" => 1.03m,
            _ => 1.00m
        };
        var decouplingPoint = sku.Family switch
        {
            "星载电子" => "星载电子半成品超市",
            "有效载荷" => "载荷洁净装配前缓冲",
            "卫星平台" => "平台总装前解耦点",
            _ => "热控结构件超市"
        };
        var profile = sku.Family switch
        {
            "星载电子" => "长 DLT 高变异库存缓冲",
            "有效载荷" => "关键载荷保护缓冲",
            "卫星平台" => "平台总装节奏缓冲",
            _ => "结构件标准补货缓冲"
        };
        var aduSource = sku.Family == "星载电子"
            ? "90 天 demonstrated ADU + 发射窗口 DAF"
            : "90 天滚动 demonstrated ADU";
        var dltSource = sku.Sku.StartsWith("AV-FPGA", StringComparison.Ordinal)
            ? "供应商承诺 DLT + 出口许可风险"
            : "供应链主数据 + DDOM 执行反馈";

        return sku with
        {
            DecouplingPoint = decouplingPoint,
            BufferProfile = profile,
            AduSource = aduSource,
            AduCalculationWindowDays = 90,
            DltSource = dltSource,
            DemandAdjustmentFactor = daf,
            ZoneAdjustmentFactor = zoneAdjustment,
            EffectiveFromWeek = 1,
            EffectiveThroughWeek = 12,
            ParameterStatus = sku.Family == "星载电子" ? "Proposed" : "Current"
        };
    }

    private static string FormatDdmrpCurrentValue(SkuBufferSetting sku)
    {
        return $"解耦点 {sku.DecouplingPoint}，ADU {sku.Adu:0.#}，DLT {sku.DecoupledLeadTimeDays} 天，VF {sku.VariabilityFactor:0.00}，DAF {sku.DemandAdjustmentFactor:0.00}，Zone {sku.ZoneAdjustmentFactor:0.00}，MOQ {sku.MinimumOrderQuantity:0}，订货周期 {sku.OrderCycleDays} 天";
    }

    private static ResourceProfile ResourceRow(StrategicMonth month, string resource, string family, decimal required, decimal available)
    {
        var load = decimal.Round(required * 100m / available, 1);
        var status = load > 100 ? "Red" : load > 85 ? "Yellow" : "Green";
        return new ResourceProfile(month.Index, month.Label, resource, family, required, available, load, status);
    }

    private static DdomFeedbackPoint Feedback(
        int period,
        string target,
        decimal reliability,
        decimal stability,
        decimal velocity,
        int red,
        int black,
        int act,
        int late,
        decimal adu,
        decimal load,
        string rootCause)
    {
        return new DdomFeedbackPoint(
            $"W-{period:00}",
            target,
            reliability,
            stability,
            velocity,
            red,
            black,
            act,
            late,
            adu,
            load,
            rootCause);
    }

    private static decimal FamilyDemandFactor(string family, int week)
    {
        return family switch
        {
            "星载电子" when week is 4 or 8 or 12 => 1.18m,
            "有效载荷" when week is 3 or 7 or 11 => 1.15m,
            "热控结构" when week is 2 or 6 or 10 => 1.10m,
            "卫星平台" when week is 4 or 8 or 12 => 1.08m,
            _ => 1m
        };
    }
}
