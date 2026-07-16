namespace AdaptiveSopDdsop.Web.Domain;

public static class DdmrpSizingExplanation
{
    public static IReadOnlyList<BufferSizingLine> Build(DdmrpSizingResult sizing)
    {
        var greenDriver = sizing.GreenDriver switch
        {
            "OrderCycle" => "订货周期",
            "MinimumOrderQuantity" => "MOQ",
            "LeadTime" => "提前期",
            _ => "证据缺失"
        };
        return new List<BufferSizingLine>
        {
            new("有效 ADU", "ADU × DAF", decimal.Round(sizing.EffectiveAdu, 1), "使用当期日均需求和需求调整因子。"),
            new("提前期需求", "有效 ADU × DLT", decimal.Round(sizing.LeadTimeDemand, 1), "覆盖解耦提前期内的需求。"),
            new("红区基础", "提前期需求 × 提前期因子", decimal.Round(sizing.RedBase, 1), "形成红区基础保护。"),
            new("红区安全", "红区基础 × 波动因子", decimal.Round(sizing.RedSafety, 1), "吸收需求与供应波动。"),
            new("红区", "（红区基础 + 红区安全）× 区域调整", sizing.Zones.Red, "红区厚度。"),
            new("黄区", "提前期需求 × 区域调整", sizing.Zones.Yellow, "黄区厚度。"),
            new("绿区提前期候选", "提前期需求 × 提前期因子", decimal.Round(sizing.GreenLeadTimeCandidate, 1), "提前期候选。"),
            new("绿区 MOQ 候选", "MOQ", decimal.Round(sizing.GreenMoqCandidate, 1), "最小订货量候选。"),
            new("绿区订货周期候选", "有效 ADU × 订货周期", decimal.Round(sizing.GreenOrderCycleCandidate, 1), "订货周期候选。"),
            new("绿区", "max（三个候选）× 区域调整", sizing.Zones.Green, $"决定项：{greenDriver}。"),
            new("总缓冲", "红区 + 黄区 + 绿区", sizing.Zones.TopOfGreen, "绿区上沿。")
        };
    }
}
