using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.Data;

public static class SeedData
{
    public static ValidationData Create()
    {
        var families = new List<ProductFamily>
        {
            new("电驱系统", "电驱动平台产品族", 96, 88, 360),
            new("高压电池", "高压储能产品族", 95, 84, 520),
            new("智能控制", "智能控制器产品族", 97, 90, 240),
            new("线束连接", "高压线束与连接产品族", 96, 86, 180),
        };

        var skus = new List<SkuBufferSetting>
        {
            new("DD-STD-001", "标准电驱总成", "电驱系统", 100, 5, 1.5m, 3, 700, 12.5m, 1200),
            new("DD-HD-002", "重载电驱总成", "电驱系统", 82, 6, 1.6m, 4, 600, 18.2m, 900),
            new("DD-WHL-003", "轮端驱动套件", "电驱系统", 145, 4, 1.3m, 3, 800, 7.4m, 1600),
            new("DC-48V-101", "48V 高压电池包", "高压电池", 64, 8, 1.8m, 5, 500, 96m, 650),
            new("DC-72V-102", "72V 高压电池包", "高压电池", 40, 9, 1.9m, 5, 350, 144m, 420),
            new("ZK-BASIC-201", "基础型整车控制器", "智能控制", 130, 5, 1.4m, 4, 650, 22m, 1300),
            new("ZK-PRO-202", "高阶域控制器", "智能控制", 72, 7, 1.7m, 4, 420, 36m, 760),
            new("ZK-NPI-203", "新品传感器包", "智能控制", 28, 10, 2.0m, 5, 240, 48m, 300),
            new("XS-HV-301", "高压主线束", "线束连接", 118, 6, 1.5m, 4, 620, 16.8m, 1100),
            new("XS-LV-302", "低压车身线束", "线束连接", 165, 4, 1.2m, 3, 760, 8.6m, 1700),
            new("LJ-TE-401", "进口高压连接器", "线束连接", 92, 10, 2.1m, 5, 500, 38m, 520),
            new("LJ-CU-402", "国产铜导线包", "线束连接", 210, 3, 1.1m, 2, 900, 5.2m, 2200),
        };

        var inventory = new List<InventoryPosition>
        {
            new("DD-STD-001", 420, 300, 260),
            new("DD-HD-002", 820, 420, 240),
            new("DD-WHL-003", 1550, 360, 410),
            new("DC-48V-101", 2260, 180, 240),
            new("DC-72V-102", 410, 80, 160),
            new("ZK-BASIC-201", 760, 260, 330),
            new("ZK-PRO-202", 1060, 210, 190),
            new("ZK-NPI-203", 180, 60, 80),
            new("XS-HV-301", 980, 260, 360),
            new("XS-LV-302", 1960, 460, 520),
            new("LJ-TE-401", 620, 90, 340),
            new("LJ-CU-402", 2580, 600, 480),
        };

        var demand = skus
            .SelectMany(sku => Enumerable.Range(1, 12)
                .Select(week => new WeeklyDemand(
                    sku.Sku,
                    week,
                    decimal.Round(sku.Adu * 5m * (1 + ((week % 4) - 1.5m) * 0.04m), 0))))
            .ToList();

        var resources = new List<CapacityResource>
        {
            new("RES-ASSY", "总装单元", 6500, 1.00m),
            new("RES-TEST", "终检测试台", 5200, 0.82m),
            new("RES-PACK", "包装与发运准备", 7000, 0.65m),
            new("RES-KOMAX", "KOMAX 自动压接线", 5800, 0.74m),
        };

        var resourceRoutings = skus.SelectMany(sku => sku.Family switch
        {
            "线束连接" => new[]
            {
                new ResourceRouting(sku.Sku, "RES-KOMAX", 0.56m),
                new ResourceRouting(sku.Sku, "RES-PACK", 0.18m),
            },
            "智能控制" => new[]
            {
                new ResourceRouting(sku.Sku, "RES-TEST", 0.48m),
                new ResourceRouting(sku.Sku, "RES-PACK", 0.12m),
            },
            "高压电池" => new[]
            {
                new ResourceRouting(sku.Sku, "RES-ASSY", 0.42m),
                new ResourceRouting(sku.Sku, "RES-TEST", 0.36m),
            },
            _ => new[]
            {
                new ResourceRouting(sku.Sku, "RES-ASSY", 0.38m),
                new ResourceRouting(sku.Sku, "RES-TEST", 0.22m),
            }
        }).ToList();

        var supplierItemSources = skus.Select(sku =>
        {
            var supplier = sku.Sku.StartsWith("LJ-", StringComparison.Ordinal)
                ? "泰科电子"
                : sku.Family switch
                {
                    "线束连接" => "华东铜材",
                    "智能控制" => "莫仕连接系统",
                    "高压电池" => "华南注塑",
                    _ => "华南注塑"
                };
            var family = sku.Sku.StartsWith("LJ-", StringComparison.Ordinal)
                ? "进口高压连接器"
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
            new(1, "PORTFOLIO", "产品组合与新活动审查", "CEO / 市场 / 研发", "确认 NPI、生命周期与 SKU 理性化是否支撑长期增长。", "组合决策与创新缺口"),
            new(2, "DEMAND", "需求计划审查", "销售 / 市场", "形成按产品族、区域和情景聚合的需求范围。", "需求范围与需求风险"),
            new(3, "SUPPLY", "供应计划审查", "运营 / 采购 / 制造", "用资源剖面、供应商约束和资本需求检查可行性。", "供应可行性与资源缺口"),
            new(4, "FINANCIAL", "财务计划审查", "CFO / 财务", "把实物计划货币化为收入、贡献毛利、现金和 ROI。", "财务缺口与投资边界"),
            new(5, "RECON", "集成协调", "总经理 / 集成负责人", "把需求、供应、财务、组合冲突拉到同一张取舍表。", "Single Game Plan 候选方案"),
            new(6, "DDSOP", "DDS&OP 战术衔接", "供应链 / 运营模型团队", "把战略参数转成 DDOM Master Settings，并回传运营现实。", "主设置变更与可行性反馈"),
            new(7, "REVIEW", "管理层决策评审", "CEO / C-Level", "批准公司级一个计划和超战术权限事项。", "Single Game Plan 与决策记录"),
        };

        var ddsopElements = new List<ProcessStep>
        {
            new(1, "TACTICAL_REVIEW", "战术回顾", "DDS&OP", "审计过去周期内 DDOM 的可靠性、稳定性、流速表现。", "Variance Analysis"),
            new(2, "TACTICAL_PROJECTION", "战术投影", "DDS&OP", "评估近期已知事件对缓冲、能力和现金的影响。", "Tactical Projection"),
            new(3, "CONFIG", "战术配置与对齐", "DDS&OP", "调整解耦点、DAF、库存/时间/产能缓冲等主设置。", "Master Settings Change"),
            new(4, "EXPLOIT", "战术开拓", "DDS&OP / 销售 / 物流", "利用短期富余能力或成本窗口换取流动红利。", "Tactical Opportunity"),
            new(5, "RECOMMEND", "战略建议", "DDS&OP", "将超出战术权限的系统性缺陷升级到 AS&OP。", "Strategic Recommendation"),
            new(6, "PROJECT", "战略投影", "DDS&OP", "对远期战略情景做运营可行性和 DDOM 承压校验。", "Feasibility Check"),
        };

        var portfolioItems = new List<PortfolioItem>
        {
            new("NPI-001", "800V 高压线束平台", "线束连接", "Validation", "批准小批验证", 18_000_000, 34, "Yellow", "进口连接器 DLT 高，需提前配置缓冲"),
            new("NPI-002", "域控制器 Pro Max", "智能控制", "Development", "继续开发", 22_000_000, 41, "Green", "软件资源为主要约束"),
            new("NPI-003", "轻量化电驱总成", "电驱系统", "Concept", "进入立项评审", 26_000_000, 38, "Yellow", "KOMAX 与测试台能力待校验"),
            new("EOL-101", "旧款 48V 电池包", "高压电池", "End-of-Life", "最后采购并清库存", 3_200_000, 18, "Red", "低毛利且库存周转慢"),
            new("MAT-201", "成熟低压车身线束", "线束连接", "Mature", "维持并降库存", 12_500_000, 29, "Green", "缓冲超绿，可释放现金"),
            new("DECL-301", "基础型控制器旧版", "智能控制", "Decline", "合并规格", 4_800_000, 21, "Yellow", "长尾 SKU 占用测试资源"),
        };

        var financialProjections = strategicMonths
            .SelectMany(month => families.Select(family =>
            {
                var baseUnits = family.Code switch
                {
                    "电驱系统" => 1180m,
                    "高压电池" => 640m,
                    "智能控制" => 1120m,
                    "线束连接" => 1680m,
                    _ => 800m
                };
                var growth = 1 + month.Index * 0.012m;
                var units = decimal.Round(baseUnits * growth, 0);
                var revenue = units * family.RevenuePerUnit;
                var margin = revenue * (family.Code == "高压电池" ? 0.26m : family.Code == "智能控制" ? 0.36m : 0.31m);
                var workingCapital = revenue * (family.Code == "线束连接" ? 0.18m : 0.14m);
                var roi = decimal.Round((margin - workingCapital * 0.03m) * 100m / Math.Max(workingCapital, 1), 1);
                var cashGap = month.Index > 18 && family.Code == "线束连接" ? 650_000 : month.Index > 24 ? 420_000 : 0;
                return new FinancialProjection(month.Index, month.Label, family.Code, units, decimal.Round(revenue, 0), decimal.Round(margin, 0), decimal.Round(workingCapital, 0), roi, cashGap);
            }))
            .ToList();

        var resourceProfiles = strategicMonths
            .Take(24)
            .SelectMany(month => new[]
            {
                ResourceRow(month, "KOMAX 自动压接线", "线束连接", 4200 + month.Index * 95, 5800),
                ResourceRow(month, "终检测试台", "智能控制", 3600 + month.Index * 70, 5200),
                ResourceRow(month, "电池老化房", "高压电池", 2200 + month.Index * 80, 3600),
                ResourceRow(month, "电驱总装单元", "电驱系统", 3900 + month.Index * 65, 6500),
            })
            .ToList();

        var supplierConstraints = new List<SupplierConstraint>
        {
            new("泰科电子", "进口高压连接器", 8600, 10400, 56, "Red", "开发第二来源并设置战略安全缓冲"),
            new("莫仕连接系统", "高速信号连接器", 6200, 5700, 49, "Yellow", "锁定 12 周滚动承诺"),
            new("华东铜材", "国产铜导线", 24000, 19800, 14, "Green", "维持月度产能预约"),
            new("华南注塑", "连接器壳体", 16800, 17600, 21, "Yellow", "增加周末模具维护窗口"),
        };

        var capitalRequirements = new List<CapitalRequirement>
        {
            new("CAP-001", "KOMAX 自动压接线扩容", "2027-03", "KOMAX 自动压接线", 4_800_000, 1800, 28, 22, "Submitted"),
            new("CAP-002", "第三班次测试团队", "2026-11", "终检测试台", 1_200_000, 900, 34, 9, "Proposed"),
            new("CAP-003", "高压电池老化房新增库位", "2027-06", "电池老化房", 3_600_000, 1200, 19, 28, "Evaluate"),
            new("CAP-004", "线束半成品超市扩建", "2027-01", "仓储空间", 2_200_000, 1600, 24, 18, "Proposed"),
        };

        var knownEvents = new List<KnownEvent>
        {
            new("EVT-001", "Promotion", "Q4 欧洲渠道促销", "2026-10 至 2026-12", "线束连接 / 电驱系统", 1.18m, 1.10m, "销售", "Approved"),
            new("EVT-002", "Planned Shutdown", "KOMAX 年度检修", "2026-09 第 2 周", "KOMAX 自动压接线", 1.00m, 1.22m, "制造", "Approved"),
            new("EVT-003", "NPI Ramp", "800V 高压线束爬坡", "2027-01 至 2027-06", "NPI-001", 1.35m, 1.25m, "研发/运营", "Proposed"),
            new("EVT-004", "Supplier Disruption", "泰科进口件港口拥堵", "2026-08 至 2026-09", "进口高压连接器", 1.00m, 1.30m, "采购", "Reviewed"),
            new("EVT-005", "Regional Surge", "华南新能源客户放量", "2027-04 至 2027-08", "智能控制", 1.16m, 1.08m, "销售", "Draft"),
        };

        var masterSettings = skus.Select(sku =>
        {
            var proposedDaf = sku.Family == "线束连接" ? "DAF 1.18 / Zone 1.10" : sku.Family == "高压电池" ? "DAF 1.08 / Zone 1.05" : "DAF 1.00 / Zone 1.00";
            return new MasterSetting(
                $"MS-{sku.Sku}",
                "Inventory Buffer",
                sku.Name,
                $"ADU {sku.Adu:0.#}, DLT {sku.DecoupledLeadTimeDays}d, VF {sku.VariabilityFactor:0.0}",
                proposedDaf,
                sku.Family == "线束连接" ? "促销/进口件风险" : "滚动需求与实际 ADU 偏差",
                "2026-08 至 2026-12",
                sku.Family == "线束连接" ? "Proposed" : "Current",
                sku.Family == "线束连接" ? 2.4m : 0.6m,
                sku.Family == "线束连接" ? 180_000 : 35_000);
        }).Concat(new[]
        {
            new MasterSetting("MS-DP-001", "Decoupling Point", "SAA 半成品解耦点", "未设置", "设置国内 SAA Buffer", "欧洲渠道长期大单", "2027-01 生效", "Proposed", 3.2m, 111_650),
            new MasterSetting("MS-TB-001", "Time Buffer", "泰科进口连接器", "保护 8 周", "保护 10 周 + Act/Late 阈值", "港口拥堵与长交期", "2026-08 至 2026-10", "Reviewed", 2.0m, 240_000),
            new MasterSetting("MS-CB-001", "Capacity Buffer", "KOMAX 自动压接线", "保留 5%", "保留 12% + 周末班边界", "计划检修与促销叠加", "2026-09 至 2026-12", "Proposed", 2.8m, 310_000),
        }).ToList();

        var ddomFeedback = Enumerable.Range(1, 18)
            .SelectMany(period => new[]
            {
                Feedback(period, "进口高压连接器", 92 - period % 4, 78 - period % 5, 74 - period % 6, 1 + period % 3, period % 4 == 0 ? 1 : 0, 2 + period % 3, period % 5 == 0 ? 1 : 0, 88 + period, 91 + period % 8, "Supply"),
                Feedback(period, "KOMAX 自动压接线", 96 - period % 3, 82 - period % 4, 79 - period % 5, period % 4, 0, 1 + period % 2, period % 6 == 0 ? 1 : 0, 116 + period, 84 + period % 14, "Capacity"),
                Feedback(period, "高压主线束", 94 - period % 5, 86 - period % 6, 83 - period % 4, period % 2, 0, period % 3, 0, 112 + period, 76 + period % 12, "Demand"),
            })
            .ToList();

        var tacticalOpportunities = new List<TacticalOpportunity>
        {
            new("TO-001", "利用低压线束超绿缓冲释放现金", "XS-LV-302 超绿区持续 5 周", 0, 0, -180_000, 1.5m, "服务风险低", "Candidate"),
            new("TO-002", "KOMAX 周末短促压接批", "促销前两周仍有 11% 保护产能", 1_250_000, 390_000, 95_000, 2.8m, "需控制加班成本", "Evaluate"),
            new("TO-003", "华东慢运替代部分空运", "服务风险低且缓冲恢复", 0, 0, -260_000, 1.1m, "Late 警报不能超过阈值", "Approved"),
            new("TO-004", "高毛利控制器抢单窗口", "终检测试台绿区且域控制器毛利高", 1_800_000, 620_000, 140_000, 3.4m, "需销售确认客户交期", "Candidate"),
        };

        var strategicRecommendations = new List<StrategicRecommendation>
        {
            new("SR-001", "Capital", "购买第二条 KOMAX 自动压接线", "KOMAX 2027-Q1 后连续红区", 4_800_000, 2_050_000, 28, 5.4m, "COO/CFO", "Submitted"),
            new("SR-002", "Engineering", "重设计进口高压连接器替代方案", "泰科连接器反复 Act/Late", 900_000, 1_450_000, 35, 4.1m, "CTO/VP Engineering", "Proposed"),
            new("SR-003", "Capacity", "终检测试台第三班次", "域控制器 NPI 与促销重叠", 1_200_000, 980_000, 34, 3.2m, "COO/HR", "Evaluate"),
            new("SR-004", "Supplier", "开发莫仕第二来源", "单一来源影响两个产品族", 650_000, 740_000, 22, 2.6m, "VP Supply Chain", "Draft"),
        };

        var feasibilityChecks = new List<FeasibilityCheck>
        {
            new("欧洲渠道长期大单", "2027-01 至 2028-12", 118, 109, 1_860_000, 420, 950, 2_300_000, "有条件可行", "新增 SAA 解耦点并批准 KOMAX 扩容"),
            new("800V 高压线束全新系列", "2027-03 至 2028-12", 126, 114, 2_240_000, 510, 1380, 3_100_000, "不可行", "先完成连接器工程替代和仓储扩建"),
            new("华南新能源客户放量", "2026-11 至 2027-12", 92, 86, 740_000, 160, 0, 420_000, "可行", "启用 DAF 1.16 并锁定测试台保护产能"),
            new("不新增资本投资的高服务承诺", "2027-01 至 2028-06", 135, 122, 2_900_000, 620, 2100, 4_600_000, "不可行", "必须降低服务承诺或批准固定资产"),
        };

        var skillBuffers = new List<SkillBuffer>
        {
            new("KOMAX 压接班组", "自动压接换模与维护", 4, 6, "Yellow", "培养 2 名多能工并建立周末班资格"),
            new("终检测试团队", "高压安全测试", 3, 5, "Red", "安排认证培训并外部招聘 1 人"),
            new("供应链计划组", "DDOM Master Settings 管理", 2, 4, "Yellow", "完成 DDI/DDS&OP 方法培训"),
            new("研发工程组", "连接器替代设计", 5, 5, "Green", "维持当前能力"),
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
}
