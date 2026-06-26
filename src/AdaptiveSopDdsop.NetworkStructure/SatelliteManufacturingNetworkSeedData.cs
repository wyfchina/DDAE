namespace AdaptiveSopDdsop.NetworkStructure;

public sealed record NetworkFinishedGoodSeedInput(
    string ItemCode,
    string ItemName,
    string Family,
    decimal UnitCost,
    string BufferProfile,
    int DecoupledLeadTimeDays,
    decimal MinimumOrderQuantity,
    int OrderCycleDays,
    string ParameterStatus);

public static class SatelliteManufacturingNetworkSeedData
{
    public static NetworkDataSet Build(IReadOnlyList<NetworkFinishedGoodSeedInput> skus)
    {
        var effectiveFrom = new DateOnly(2026, 6, 1);
        var items = skus
            .Select(sku => new NetworkItemMaster(sku.ItemCode, sku.ItemName, "FinishedGood", sku.Family, "Active", sku.UnitCost, "EA"))
            .Concat(new[]
            {
                new NetworkItemMaster("SUB-SAT-BUS-CORE", "平台核心舱半成品", "Subassembly", "卫星平台", "Active", 720_000m, "EA"),
                new NetworkItemMaster("SUB-PLATFORM-BUS", "平台总装半成品", "Subassembly", "卫星平台", "Active", 640_000m, "EA"),
                new NetworkItemMaster("SUB-SAT-POWER", "平台电源分系统", "Subassembly", "卫星平台", "Active", 280_000m, "EA"),
                new NetworkItemMaster("SUB-SAT-ADCS", "姿控执行分系统", "Subassembly", "卫星平台", "Active", 360_000m, "EA"),
                new NetworkItemMaster("SUB-PAYLOAD-OPTICAL", "载荷装调半成品", "Subassembly", "有效载荷", "Active", 1_120_000m, "EA"),
                new NetworkItemMaster("SUB-PAYLOAD-EO-FOCAL", "光学焦平面组件", "Subassembly", "有效载荷", "Active", 860_000m, "EA"),
                new NetworkItemMaster("SUB-PAYLOAD-SAR-RF", "SAR 射频前端组件", "Subassembly", "有效载荷", "Active", 980_000m, "EA"),
                new NetworkItemMaster("SUB-AVIONICS-BAY", "星载电子半成品", "Subassembly", "星载电子", "Active", 460_000m, "EA"),
                new NetworkItemMaster("SUB-AVIONICS-COMPUTE", "星载计算模块", "Subassembly", "星载电子", "Active", 520_000m, "EA"),
                new NetworkItemMaster("SUB-AVIONICS-RF", "星载射频处理模块", "Subassembly", "星载电子", "Active", 410_000m, "EA"),
                new NetworkItemMaster("SUB-HARNESS-KIT", "星上电缆束套件半成品", "Subassembly", "星载电子", "Active", 92_000m, "EA"),
                new NetworkItemMaster("SUB-HARNESS-TESTED", "测试后电缆束套件", "Subassembly", "星载电子", "Active", 118_000m, "EA"),
                new NetworkItemMaster("SUB-HARNESS-LOOM", "线束组件", "Subassembly", "星载电子", "Active", 64_000m, "EA"),
                new NetworkItemMaster("SUB-THERMAL-KIT", "热控结构组件半成品", "Subassembly", "热控结构", "Active", 180_000m, "EA"),
                new NetworkItemMaster("SUB-THERMAL-MLI", "MLI 热控包覆组件", "Subassembly", "热控结构", "Active", 92_000m, "EA"),
                new NetworkItemMaster("SUB-THERMAL-PANEL", "热控结构散热板组件", "Subassembly", "热控结构", "Active", 210_000m, "EA"),
                new NetworkItemMaster("SUB-DEPLOYMENT-MECH", "展开机构组件", "Subassembly", "热控结构", "Active", 260_000m, "EA"),
                new NetworkItemMaster("PART-FPGA-SPACE", "进口空间级 FPGA", "PurchasedPart", "星载电子", "Active", 210_000m, "EA"),
                new NetworkItemMaster("PART-FPGA-DOMESTIC-ALT", "国产空间级 FPGA 备选", "PurchasedPart", "星载电子", "EngineeringReview", 184_000m, "EA"),
                new NetworkItemMaster("PART-FPGA-HI-REL-ALT", "高可靠 FPGA 备选", "PurchasedPart", "星载电子", "Qualified", 238_000m, "EA"),
                new NetworkItemMaster("PART-OBC-CPU", "抗辐照星务处理器", "PurchasedPart", "星载电子", "Active", 96_000m, "EA"),
                new NetworkItemMaster("PART-RAD-MEMORY", "抗辐照存储器", "PurchasedPart", "星载电子", "Active", 36_000m, "EA"),
                new NetworkItemMaster("PART-RF-MODULE", "星载射频模块", "PurchasedPart", "星载电子", "Active", 72_000m, "EA"),
                new NetworkItemMaster("PART-OPTICS-DETECTOR", "空间级光电探测器", "PurchasedPart", "有效载荷", "Active", 680_000m, "EA"),
                new NetworkItemMaster("PART-OPTICS-DETECTOR-ALT", "空间级探测器备选", "PurchasedPart", "有效载荷", "EngineeringReview", 620_000m, "EA"),
                new NetworkItemMaster("PART-LENS-SET", "空间光学镜头组", "PurchasedPart", "有效载荷", "Active", 420_000m, "EA"),
                new NetworkItemMaster("PART-SAR-TR-MODULE", "SAR T/R 组件", "PurchasedPart", "有效载荷", "Active", 740_000m, "EA"),
                new NetworkItemMaster("PART-WAVEGUIDE", "SAR 波导组件", "PurchasedPart", "有效载荷", "Active", 86_000m, "EA"),
                new NetworkItemMaster("PART-HONEYCOMB", "蜂窝结构板", "PurchasedPart", "热控结构", "Active", 38_000m, "EA"),
                new NetworkItemMaster("PART-MLI-FILM", "多层隔热膜材料", "RawMaterial", "热控结构", "Active", 6_500m, "M2"),
                new NetworkItemMaster("PART-HEATPIPE", "航天热管", "PurchasedPart", "热控结构", "Active", 24_000m, "EA"),
                new NetworkItemMaster("PART-THERMAL-ADHESIVE", "热控导热胶", "RawMaterial", "热控结构", "Active", 1_200m, "KG"),
                new NetworkItemMaster("PART-CABLE", "航天级线缆", "RawMaterial", "星载电子", "Active", 1_800m, "M"),
                new NetworkItemMaster("PART-CONNECTOR", "微矩形连接器", "PurchasedPart", "星载电子", "Active", 4_600m, "EA"),
                new NetworkItemMaster("PART-CONNECTOR-ALT", "微矩形连接器备选", "PurchasedPart", "星载电子", "Qualified", 4_900m, "EA"),
                new NetworkItemMaster("PART-SHIELDING-BRAID", "线缆屏蔽编织层", "RawMaterial", "星载电子", "Active", 620m, "M"),
                new NetworkItemMaster("PART-POWER-PCDU", "电源控制分配单元", "PurchasedPart", "卫星平台", "Active", 188_000m, "EA"),
                new NetworkItemMaster("PART-REACTION-WHEEL", "反作用飞轮", "PurchasedPart", "卫星平台", "Active", 132_000m, "EA"),
                new NetworkItemMaster("PART-STAR-TRACKER", "星敏感器", "PurchasedPart", "卫星平台", "Active", 156_000m, "EA"),
                new NetworkItemMaster("PART-ACTUATOR", "展开机构作动器", "PurchasedPart", "热控结构", "Active", 160_000m, "EA"),
            })
            .ToList();

        var bomHeaders = new List<NetworkBomHeader>
        {
            new("BOM-SAT-BUS-001-A", "SAT-BUS-001", "A", effectiveFrom, null, "Released"),
            new("BOM-SAT-BUS-002-A", "SAT-BUS-002", "A", effectiveFrom, null, "Released"),
            new("BOM-SAT-PROP-003-A", "SAT-PROP-003", "A", effectiveFrom, null, "Released"),
            new("BOM-PAY-EO-101-B", "PAY-EO-101", "B", effectiveFrom, null, "Released"),
            new("BOM-PAY-SAR-102-B", "PAY-SAR-102", "B", effectiveFrom, null, "Released"),
            new("BOM-AV-COM-201-A", "AV-COM-201", "A", effectiveFrom, null, "Released"),
            new("BOM-AV-OBC-202-A", "AV-OBC-202", "A", effectiveFrom, null, "Released"),
            new("BOM-AV-FPGA-203-A", "AV-FPGA-203", "A", effectiveFrom, null, "Released"),
            new("BOM-TC-MLI-301-A", "TC-MLI-301", "A", effectiveFrom, null, "Released"),
            new("BOM-TC-RAD-302-A", "TC-RAD-302", "A", effectiveFrom, null, "Released"),
            new("BOM-MECH-DEP-401-A", "MECH-DEP-401", "A", effectiveFrom, null, "Released"),
            new("BOM-CBL-HAR-402-A", "CBL-HAR-402", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-SAT-BUS-CORE-A", "SUB-SAT-BUS-CORE", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-PLATFORM-BUS-A", "SUB-PLATFORM-BUS", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-SAT-POWER-A", "SUB-SAT-POWER", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-SAT-ADCS-A", "SUB-SAT-ADCS", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-PAYLOAD-EO-FOCAL-A", "SUB-PAYLOAD-EO-FOCAL", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-PAYLOAD-SAR-RF-A", "SUB-PAYLOAD-SAR-RF", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-AVIONICS-A", "SUB-AVIONICS-BAY", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-AVIONICS-COMPUTE-A", "SUB-AVIONICS-COMPUTE", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-AVIONICS-RF-A", "SUB-AVIONICS-RF", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-HARNESS-A", "SUB-HARNESS-KIT", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-HARNESS-TESTED-A", "SUB-HARNESS-TESTED", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-HARNESS-LOOM-A", "SUB-HARNESS-LOOM", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-THERMAL-A", "SUB-THERMAL-KIT", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-THERMAL-MLI-A", "SUB-THERMAL-MLI", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-THERMAL-PANEL-A", "SUB-THERMAL-PANEL", "A", effectiveFrom, null, "Released"),
            new("BOM-SUB-DEPLOYMENT-MECH-A", "SUB-DEPLOYMENT-MECH", "A", effectiveFrom, null, "Released"),
        };

        var bomLines = new List<NetworkBomLine>
        {
            new("BOM-SAT-BUS-001-A", "SAT-BUS-001", "SUB-SAT-BUS-CORE", 1, 0.02m, ""),
            new("BOM-SAT-BUS-001-A", "SAT-BUS-001", "SUB-SAT-POWER", 1, 0.02m, ""),
            new("BOM-SAT-BUS-001-A", "SAT-BUS-001", "SUB-SAT-ADCS", 1, 0.02m, ""),
            new("BOM-SAT-BUS-001-A", "SAT-BUS-001", "SUB-HARNESS-TESTED", 2, 0.03m, ""),
            new("BOM-SAT-BUS-002-A", "SAT-BUS-002", "SUB-SAT-BUS-CORE", 1, 0.02m, ""),
            new("BOM-SAT-BUS-002-A", "SAT-BUS-002", "SUB-SAT-POWER", 2, 0.02m, ""),
            new("BOM-SAT-BUS-002-A", "SAT-BUS-002", "SUB-SAT-ADCS", 1, 0.02m, ""),
            new("BOM-SAT-BUS-002-A", "SAT-BUS-002", "SUB-HARNESS-TESTED", 3, 0.03m, ""),
            new("BOM-SAT-PROP-003-A", "SAT-PROP-003", "SUB-PLATFORM-BUS", 1, 0.02m, ""),
            new("BOM-SAT-PROP-003-A", "SAT-PROP-003", "SUB-HARNESS-TESTED", 1, 0.03m, ""),
            new("BOM-PAY-EO-101-B", "PAY-EO-101", "SUB-PAYLOAD-OPTICAL", 1, 0.01m, ""),
            new("BOM-PAY-EO-101-B", "PAY-EO-101", "SUB-HARNESS-TESTED", 1, 0.03m, ""),
            new("BOM-PAY-SAR-102-B", "PAY-SAR-102", "SUB-PAYLOAD-SAR-RF", 1, 0.01m, ""),
            new("BOM-PAY-SAR-102-B", "PAY-SAR-102", "SUB-HARNESS-TESTED", 2, 0.03m, ""),
            new("BOM-AV-COM-201-A", "AV-COM-201", "SUB-AVIONICS-RF", 1, 0.01m, ""),
            new("BOM-AV-COM-201-A", "AV-COM-201", "SUB-HARNESS-LOOM", 1, 0.02m, ""),
            new("BOM-AV-OBC-202-A", "AV-OBC-202", "SUB-AVIONICS-COMPUTE", 1, 0.01m, ""),
            new("BOM-AV-OBC-202-A", "AV-OBC-202", "SUB-HARNESS-LOOM", 1, 0.02m, ""),
            new("BOM-AV-FPGA-203-A", "AV-FPGA-203", "PART-FPGA-SPACE", 2, 0.05m, "ALT-FPGA"),
            new("BOM-AV-FPGA-203-A", "AV-FPGA-203", "SUB-AVIONICS-COMPUTE", 1, 0.01m, ""),
            new("BOM-AV-FPGA-203-A", "AV-FPGA-203", "SUB-HARNESS-LOOM", 1, 0.02m, ""),
            new("BOM-TC-MLI-301-A", "TC-MLI-301", "SUB-THERMAL-KIT", 1, 0.02m, ""),
            new("BOM-TC-RAD-302-A", "TC-RAD-302", "SUB-THERMAL-PANEL", 1, 0.03m, ""),
            new("BOM-MECH-DEP-401-A", "MECH-DEP-401", "SUB-DEPLOYMENT-MECH", 1, 0.03m, ""),
            new("BOM-CBL-HAR-402-A", "CBL-HAR-402", "SUB-HARNESS-TESTED", 1, 0.02m, ""),
            new("BOM-SUB-SAT-BUS-CORE-A", "SUB-SAT-BUS-CORE", "SUB-PLATFORM-BUS", 1, 0.02m, ""),
            new("BOM-SUB-SAT-BUS-CORE-A", "SUB-SAT-BUS-CORE", "SUB-AVIONICS-BAY", 1, 0.02m, ""),
            new("BOM-SUB-SAT-BUS-CORE-A", "SUB-SAT-BUS-CORE", "PART-HONEYCOMB", 4, 0.04m, ""),
            new("BOM-SUB-PLATFORM-BUS-A", "SUB-PLATFORM-BUS", "SUB-AVIONICS-COMPUTE", 1, 0.02m, ""),
            new("BOM-SUB-PLATFORM-BUS-A", "SUB-PLATFORM-BUS", "SUB-HARNESS-LOOM", 2, 0.03m, ""),
            new("BOM-SUB-SAT-POWER-A", "SUB-SAT-POWER", "PART-POWER-PCDU", 1, 0.02m, ""),
            new("BOM-SUB-SAT-POWER-A", "SUB-SAT-POWER", "SUB-HARNESS-LOOM", 1, 0.03m, ""),
            new("BOM-SUB-SAT-ADCS-A", "SUB-SAT-ADCS", "PART-REACTION-WHEEL", 4, 0.02m, ""),
            new("BOM-SUB-SAT-ADCS-A", "SUB-SAT-ADCS", "PART-STAR-TRACKER", 2, 0.02m, ""),
            new("BOM-SUB-SAT-ADCS-A", "SUB-SAT-ADCS", "SUB-HARNESS-LOOM", 1, 0.03m, ""),
            new("BOM-SUB-PAYLOAD-EO-FOCAL-A", "SUB-PAYLOAD-EO-FOCAL", "PART-OPTICS-DETECTOR", 2, 0.04m, "ALT-OPTICS-DETECTOR"),
            new("BOM-SUB-PAYLOAD-EO-FOCAL-A", "SUB-PAYLOAD-EO-FOCAL", "PART-LENS-SET", 1, 0.03m, ""),
            new("BOM-SUB-PAYLOAD-EO-FOCAL-A", "SUB-PAYLOAD-EO-FOCAL", "PART-FPGA-SPACE", 1, 0.04m, "ALT-FPGA"),
            new("BOM-SUB-PAYLOAD-SAR-RF-A", "SUB-PAYLOAD-SAR-RF", "PART-SAR-TR-MODULE", 8, 0.04m, ""),
            new("BOM-SUB-PAYLOAD-SAR-RF-A", "SUB-PAYLOAD-SAR-RF", "PART-WAVEGUIDE", 4, 0.03m, ""),
            new("BOM-SUB-PAYLOAD-SAR-RF-A", "SUB-PAYLOAD-SAR-RF", "PART-FPGA-SPACE", 2, 0.05m, "ALT-FPGA"),
            new("BOM-SUB-AVIONICS-A", "SUB-AVIONICS-BAY", "SUB-AVIONICS-COMPUTE", 1, 0.02m, ""),
            new("BOM-SUB-AVIONICS-A", "SUB-AVIONICS-BAY", "SUB-AVIONICS-RF", 1, 0.02m, ""),
            new("BOM-SUB-AVIONICS-A", "SUB-AVIONICS-BAY", "PART-RAD-MEMORY", 2, 0.03m, ""),
            new("BOM-SUB-AVIONICS-COMPUTE-A", "SUB-AVIONICS-COMPUTE", "PART-FPGA-SPACE", 3, 0.05m, "ALT-FPGA"),
            new("BOM-SUB-AVIONICS-COMPUTE-A", "SUB-AVIONICS-COMPUTE", "PART-OBC-CPU", 1, 0.02m, ""),
            new("BOM-SUB-AVIONICS-COMPUTE-A", "SUB-AVIONICS-COMPUTE", "PART-RAD-MEMORY", 4, 0.03m, ""),
            new("BOM-SUB-AVIONICS-RF-A", "SUB-AVIONICS-RF", "PART-RF-MODULE", 2, 0.02m, ""),
            new("BOM-SUB-AVIONICS-RF-A", "SUB-AVIONICS-RF", "PART-FPGA-SPACE", 1, 0.05m, "ALT-FPGA"),
            new("BOM-SUB-HARNESS-A", "SUB-HARNESS-KIT", "SUB-HARNESS-TESTED", 1, 0.02m, ""),
            new("BOM-SUB-HARNESS-TESTED-A", "SUB-HARNESS-TESTED", "SUB-HARNESS-LOOM", 1, 0.02m, ""),
            new("BOM-SUB-HARNESS-TESTED-A", "SUB-HARNESS-TESTED", "PART-CONNECTOR", 8, 0.04m, "ALT-CONNECTOR"),
            new("BOM-SUB-HARNESS-LOOM-A", "SUB-HARNESS-LOOM", "PART-CABLE", 42, 0.06m, ""),
            new("BOM-SUB-HARNESS-LOOM-A", "SUB-HARNESS-LOOM", "PART-CONNECTOR", 12, 0.04m, "ALT-CONNECTOR"),
            new("BOM-SUB-HARNESS-LOOM-A", "SUB-HARNESS-LOOM", "PART-SHIELDING-BRAID", 45, 0.06m, ""),
            new("BOM-SUB-THERMAL-A", "SUB-THERMAL-KIT", "SUB-THERMAL-MLI", 1, 0.02m, ""),
            new("BOM-SUB-THERMAL-A", "SUB-THERMAL-KIT", "SUB-THERMAL-PANEL", 1, 0.02m, ""),
            new("BOM-SUB-THERMAL-MLI-A", "SUB-THERMAL-MLI", "PART-MLI-FILM", 18, 0.08m, "ALT-THERMAL-FILM"),
            new("BOM-SUB-THERMAL-MLI-A", "SUB-THERMAL-MLI", "PART-THERMAL-ADHESIVE", 3, 0.05m, ""),
            new("BOM-SUB-THERMAL-PANEL-A", "SUB-THERMAL-PANEL", "PART-HONEYCOMB", 2, 0.05m, ""),
            new("BOM-SUB-THERMAL-PANEL-A", "SUB-THERMAL-PANEL", "PART-HEATPIPE", 6, 0.03m, ""),
            new("BOM-SUB-DEPLOYMENT-MECH-A", "SUB-DEPLOYMENT-MECH", "PART-ACTUATOR", 2, 0.03m, ""),
            new("BOM-SUB-DEPLOYMENT-MECH-A", "SUB-DEPLOYMENT-MECH", "PART-HONEYCOMB", 1, 0.03m, ""),
        };

        var alternateItems = new List<NetworkAlternateItem>
        {
            new("ALT-FPGA", "PART-FPGA-SPACE", "PART-FPGA-DOMESTIC-ALT", 2, 1.00m, "EngineeringReview"),
            new("ALT-FPGA", "PART-FPGA-SPACE", "PART-FPGA-HI-REL-ALT", 3, 1.00m, "Qualified"),
            new("ALT-CONNECTOR", "PART-CONNECTOR", "PART-CONNECTOR-ALT", 2, 1.00m, "Qualified"),
            new("ALT-OPTICS-DETECTOR", "PART-OPTICS-DETECTOR", "PART-OPTICS-DETECTOR-ALT", 2, 1.00m, "EngineeringReview"),
            new("ALT-THERMAL-FILM", "PART-MLI-FILM", "PART-THERMAL-ADHESIVE", 3, 0.18m, "EngineeringReview"),
        };

        var routingLines = skus.SelectMany(sku => sku.Family switch
        {
            "星载电子" => new[]
            {
                new NetworkRoutingLine(sku.ItemCode, sku.ItemCode, sku.Family, "R1", "线束装联", "RES-HARNESS", 0.52m, effectiveFrom, null),
                new NetworkRoutingLine(sku.ItemCode, sku.ItemCode, sku.Family, "R1", "环境试验", "RES-TVAC", 0.20m, effectiveFrom, null),
            },
            "有效载荷" => new[]
            {
                new NetworkRoutingLine(sku.ItemCode, sku.ItemCode, sku.Family, "R2", "洁净装配", "RES-CLEAN", 0.70m, effectiveFrom, null),
                new NetworkRoutingLine(sku.ItemCode, sku.ItemCode, sku.Family, "R2", "热真空试验", "RES-TVAC", 0.42m, effectiveFrom, null),
            },
            "热控结构" => new[]
            {
                new NetworkRoutingLine(sku.ItemCode, sku.ItemCode, sku.Family, "R1", "总装集成", "RES-AIT", 0.30m, effectiveFrom, null),
                new NetworkRoutingLine(sku.ItemCode, sku.ItemCode, sku.Family, "R1", "线束接口", "RES-HARNESS", 0.18m, effectiveFrom, null),
            },
            _ => new[]
            {
                new NetworkRoutingLine(sku.ItemCode, sku.ItemCode, sku.Family, "R1", "总装集成", "RES-AIT", 0.56m, effectiveFrom, null),
                new NetworkRoutingLine(sku.ItemCode, sku.ItemCode, sku.Family, "R1", "热真空试验", "RES-TVAC", 0.24m, effectiveFrom, null),
            }
        }).Concat(new[]
        {
            new NetworkRoutingLine("SUB-AVIONICS-BAY", "AVIONICS", "星载电子", "R1", "电子装联", "RES-HARNESS", 0.35m, effectiveFrom, null),
            new NetworkRoutingLine("SUB-AVIONICS-COMPUTE", "AV-FPGA-203", "星载电子", "R1", "板级装联", "RES-HARNESS", 0.42m, effectiveFrom, null),
            new NetworkRoutingLine("SUB-AVIONICS-COMPUTE", "SAT-BUS-001", "卫星平台", "R2", "平台电子集成", "RES-AIT", 0.28m, effectiveFrom, null),
            new NetworkRoutingLine("SUB-AVIONICS-RF", "AV-COM-201", "星载电子", "R1", "射频调测", "RES-CLEAN", 0.26m, effectiveFrom, null),
            new NetworkRoutingLine("SUB-HARNESS-KIT", "HARNESS", "星载电子", "R1", "线缆制备", "RES-HARNESS", 0.18m, effectiveFrom, null),
            new NetworkRoutingLine("SUB-HARNESS-TESTED", "HARNESS", "星载电子", "R1", "线束测试", "RES-HARNESS", 0.22m, effectiveFrom, null),
            new NetworkRoutingLine("SUB-HARNESS-LOOM", "HARNESS", "星载电子", "R1", "线束成型", "RES-HARNESS", 0.14m, effectiveFrom, null),
            new NetworkRoutingLine("SUB-PAYLOAD-OPTICAL", "PAYLOAD", "有效载荷", "R2", "洁净装调", "RES-CLEAN", 0.55m, effectiveFrom, null),
            new NetworkRoutingLine("SUB-PAYLOAD-EO-FOCAL", "PAY-EO-101", "有效载荷", "R2", "焦平面装调", "RES-CLEAN", 0.62m, effectiveFrom, null),
            new NetworkRoutingLine("SUB-PAYLOAD-SAR-RF", "PAY-SAR-102", "有效载荷", "R2", "SAR 射频调测", "RES-TVAC", 0.48m, effectiveFrom, null),
            new NetworkRoutingLine("SUB-THERMAL-MLI", "TC-MLI-301", "热控结构", "R1", "MLI 包覆", "RES-AIT", 0.16m, effectiveFrom, null),
            new NetworkRoutingLine("SUB-THERMAL-PANEL", "TC-RAD-302", "热控结构", "R1", "热管钎焊", "RES-CLEAN", 0.24m, effectiveFrom, null),
            new NetworkRoutingLine("SUB-DEPLOYMENT-MECH", "MECH-DEP-401", "热控结构", "R1", "展开机构装调", "RES-AIT", 0.34m, effectiveFrom, null),
        }).ToList();

        var supplierSources = new List<NetworkSupplierSource>
        {
            new("PART-FPGA-SPACE", "SUP-MICROCHIP", "Microchip Space", 1, 70, 112, 1.45m, 1700, 80, "Qualified"),
            new("PART-FPGA-SPACE", "SUP-DOMESTIC-FPGA", "国产空间器件验证线", 2, 20, 84, 1.25m, 420, 40, "EngineeringReview"),
            new("PART-FPGA-SPACE", "SUP-HI-REL", "高可靠器件备供", 3, 10, 126, 1.55m, 260, 20, "Qualified"),
            new("PART-FPGA-DOMESTIC-ALT", "SUP-DOMESTIC-FPGA", "国产空间器件验证线", 1, 100, 84, 1.25m, 420, 40, "EngineeringReview"),
            new("PART-FPGA-HI-REL-ALT", "SUP-HI-REL", "高可靠器件备供", 1, 100, 126, 1.55m, 260, 20, "Qualified"),
            new("PART-OBC-CPU", "SUP-9TH-ELEC", "航天九院电子", 1, 70, 56, 1.20m, 1400, 60, "Qualified"),
            new("PART-OBC-CPU", "SUP-DOMESTIC-FPGA", "国产空间器件验证线", 2, 30, 70, 1.25m, 600, 40, "Qualified"),
            new("PART-RAD-MEMORY", "SUP-9TH-ELEC", "航天九院电子", 1, 100, 49, 1.18m, 1600, 80, "Qualified"),
            new("PART-RF-MODULE", "SUP-9TH-ELEC", "航天九院电子", 1, 100, 49, 1.15m, 1200, 50, "Qualified"),
            new("PART-OPTICS-DETECTOR", "SUP-CAS-OPTICS", "中科光电", 1, 80, 84, 1.30m, 680, 20, "Qualified"),
            new("PART-OPTICS-DETECTOR", "SUP-OPTICS-ALT", "华东光电备供", 2, 20, 98, 1.40m, 160, 10, "EngineeringReview"),
            new("PART-OPTICS-DETECTOR-ALT", "SUP-OPTICS-ALT", "华东光电备供", 1, 100, 98, 1.40m, 160, 10, "EngineeringReview"),
            new("PART-LENS-SET", "SUP-CAS-OPTICS", "中科光电", 1, 100, 70, 1.22m, 360, 12, "Qualified"),
            new("PART-SAR-TR-MODULE", "SUP-CAS-OPTICS", "中科光电", 1, 100, 91, 1.35m, 520, 16, "Qualified"),
            new("PART-WAVEGUIDE", "SUP-CAS-OPTICS", "中科光电", 1, 100, 42, 1.16m, 960, 40, "Qualified"),
            new("PART-HONEYCOMB", "SUP-COMPOSITE", "航天复材", 1, 100, 28, 1.10m, 5200, 120, "Qualified"),
            new("PART-MLI-FILM", "SUP-COMPOSITE", "航天复材", 1, 100, 21, 1.08m, 8800, 200, "Qualified"),
            new("PART-HEATPIPE", "SUP-THERMAL", "热控器件厂", 1, 100, 35, 1.14m, 1800, 60, "Qualified"),
            new("PART-THERMAL-ADHESIVE", "SUP-CHEM", "航天胶黏材料", 1, 100, 28, 1.18m, 2200, 80, "Qualified"),
            new("PART-CABLE", "SUP-9TH-ELEC", "航天九院电子", 1, 100, 35, 1.12m, 6400, 300, "Qualified"),
            new("PART-CONNECTOR", "SUP-9TH-ELEC", "航天九院电子", 1, 75, 42, 1.18m, 5200, 200, "Qualified"),
            new("PART-CONNECTOR", "SUP-ALT-CONN", "华东连接器厂", 2, 25, 49, 1.22m, 1100, 100, "Qualified"),
            new("PART-CONNECTOR-ALT", "SUP-ALT-CONN", "华东连接器厂", 1, 100, 49, 1.22m, 1100, 100, "Qualified"),
            new("PART-SHIELDING-BRAID", "SUP-9TH-ELEC", "航天九院电子", 1, 100, 28, 1.10m, 8200, 400, "Qualified"),
            new("PART-POWER-PCDU", "SUP-POWER", "空间电源事业部", 1, 100, 63, 1.24m, 760, 20, "Qualified"),
            new("PART-REACTION-WHEEL", "SUP-ADCS", "姿控执行机构厂", 1, 100, 70, 1.28m, 620, 16, "Qualified"),
            new("PART-STAR-TRACKER", "SUP-ADCS", "姿控执行机构厂", 1, 100, 77, 1.32m, 480, 12, "Qualified"),
            new("PART-ACTUATOR", "SUP-MECH", "航天精密机电", 1, 100, 63, 1.28m, 540, 20, "Qualified"),
        };

        var inventoryLocations = new List<NetworkInventoryLocation>
        {
            new("SUB-SAT-BUS-CORE", "WH-AIT-WIP", "AIT 平台核心舱暂存区", "WipSupermarket", "Qualified", "总装中心", 365, false),
            new("SUB-AVIONICS-BAY", "WH-ELEC-WIP", "电子装联半成品库", "WipSupermarket", "Qualified", "电子装联中心", 365, true),
            new("SUB-AVIONICS-COMPUTE", "WH-ELEC-WIP", "星载计算模块超市", "WipSupermarket", "Qualified", "电子装联中心", 365, true),
            new("SUB-HARNESS-KIT", "WH-HARNESS-WIP", "星上电缆束半成品库", "WipSupermarket", "Qualified", "线束工位", 540, true),
            new("SUB-HARNESS-TESTED", "WH-HARNESS-QA", "测试后电缆束合格区", "WipSupermarket", "Qualified", "线束工位", 540, true),
            new("SUB-PAYLOAD-OPTICAL", "WH-CLEAN-WIP", "洁净载荷装调暂存区", "WipSupermarket", "Qualified", "载荷装配间", 180, false),
            new("SUB-PAYLOAD-EO-FOCAL", "WH-CLEAN-WIP", "光学焦平面洁净暂存区", "WipSupermarket", "Qualified", "载荷装配间", 180, false),
            new("SUB-THERMAL-KIT", "WH-THERMAL-WIP", "热控结构件超市", "WipSupermarket", "Qualified", "热控结构班组", 720, true),
            new("SUB-THERMAL-MLI", "WH-THERMAL-WIP", "MLI 包覆组件超市", "WipSupermarket", "Qualified", "热控结构班组", 540, true),
            new("PART-FPGA-SPACE", "WH-IQC", "进口器件待检区", "Inspection", "PendingRelease", "质量部", 730, false),
            new("PART-FPGA-SPACE", "WH-ELEC-QA", "空间级器件合格库", "QualifiedStock", "Qualified", "供应链", 730, true),
            new("PART-CONNECTOR", "WH-IQC", "连接器待检区", "Inspection", "PendingRelease", "质量部", 540, false),
            new("PART-CABLE", "LINE-HARNESS", "线束线边库", "LineSide", "Qualified", "线束工位", null, true),
            new("PART-CONNECTOR", "LINE-HARNESS", "连接器线边库", "LineSide", "Qualified", "线束工位", null, true),
            new("PART-MLI-FILM", "WH-COMPOSITE", "复材与热控材料库", "QualifiedStock", "Qualified", "供应链", 540, true),
            new("PART-THERMAL-ADHESIVE", "WH-MATERIAL-CONTROL", "受控胶黏材料库", "QualifiedStock", "Qualified", "供应链", 180, false),
        };

        var bufferSettings = skus.Select(sku => new NetworkBufferSetting(
                sku.ItemCode,
                true,
                sku.BufferProfile,
                sku.ItemCode.StartsWith("AV-FPGA", StringComparison.Ordinal) ? 56 : sku.DecoupledLeadTimeDays,
                sku.MinimumOrderQuantity,
                sku.OrderCycleDays,
                effectiveFrom,
                null,
                sku.ParameterStatus))
            .Concat(new[]
            {
                new NetworkBufferSetting("SUB-AVIONICS-BAY", true, "星载电子半成品库存缓冲", 28, 80, 5, effectiveFrom, null, "Proposed"),
                new NetworkBufferSetting("SUB-AVIONICS-COMPUTE", true, "星载计算模块库存缓冲", 35, 60, 5, effectiveFrom, null, "Proposed"),
                new NetworkBufferSetting("SUB-HARNESS-TESTED", true, "测试后线束套件流动缓冲", 14, 160, 3, effectiveFrom, null, "Current"),
                new NetworkBufferSetting("SUB-PAYLOAD-EO-FOCAL", true, "光学焦平面装调前时间缓冲", 28, 16, 7, effectiveFrom, null, "Proposed"),
                new NetworkBufferSetting("SUB-THERMAL-MLI", true, "热控 MLI 包覆组件缓冲", 21, 80, 5, effectiveFrom, null, "Proposed"),
                new NetworkBufferSetting("PART-FPGA-SPACE", true, "进口器件时间缓冲", 84, 80, 7, effectiveFrom, null, "Reviewed"),
            })
            .ToList();

        var leadTimeProfiles = supplierSources
            .Select(source => new NetworkLeadTimeProfile(
                source.ItemCode,
                "Supplier",
                source.LeadTimeDays,
                source.LeadTimeVariabilityFactor,
                source.ItemCode))
            .Concat(bufferSettings
                .Where(setting => setting.TimeBufferDays > 0)
                .Select(setting => new NetworkLeadTimeProfile(
                    setting.ItemCode,
                    "TimeBuffer",
                    setting.TimeBufferDays,
                    1.00m,
                    setting.ItemCode)))
            .ToList();

        return new NetworkDataSet(
            items,
            bomHeaders,
            bomLines,
            alternateItems,
            routingLines,
            supplierSources,
            inventoryLocations,
            bufferSettings,
            leadTimeProfiles);
    }

}
