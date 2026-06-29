namespace AdaptiveSopDdsop.NetworkStructure;

public sealed record NetworkStructureProductCapabilities(
    string ProductName,
    string DeploymentMode,
    IReadOnlyList<NetworkStructureCapability> Capabilities,
    IReadOnlyList<NetworkStructureExternalDependency> ExternalDependencies,
    IReadOnlyList<string> Boundaries);

public sealed record NetworkStructureCapability(
    string Code,
    string Name,
    string Description,
    bool IsAvailable);

public sealed record NetworkStructureExternalDependency(
    string Code,
    string Name,
    string Purpose,
    bool IsRequiredForStandaloneHost);

public static class NetworkStructureProductCapabilityCatalog
{
    public static NetworkStructureProductCapabilities CreateStandaloneHost()
    {
        return new NetworkStructureProductCapabilities(
            "网络结构评分",
            "独立 Host",
            new[]
            {
                new NetworkStructureCapability("NetworkData", "网络主数据", "读取物料、BOM、替代料、资源路线、供应来源、库存位置、缓冲设置和提前期档案。", true),
                new NetworkStructureCapability("NetworkGraph", "物料网络展开", "按物料展开上游组件、下游父项、路径累计用量和主数据校验报告。", true),
                new NetworkStructureCapability("NetworkMetrics", "网络指标计算", "计算下游覆盖度、数量影响度、累计提前期、供应风险、资源约束和库存代价，并输出证据链。", true),
                new NetworkStructureCapability("NetworkScoring", "网络结构评分", "生成控制点、解耦点、库存缓冲、时间缓冲和能力缓冲候选及不采纳风险。", true),
                new NetworkStructureCapability("CandidateCombination", "候选动作组合选择（后续）", "当前不在网络结构评分工作台暴露；未来应放在外部多方案比较阶段，先筛选候选组合，再回到白盒引擎重算。", false),
                new NetworkStructureCapability("WhiteBoxRecalculation", "白盒场景回算", "库存金额、红区周、补货订单、RCCP 和供应缺口变化必须由外部白盒引擎验证。", false),
            },
            new[]
            {
                new NetworkStructureExternalDependency("ExternalPreview", "外部白盒场景回算引擎", "验证候选动作组合对库存、RCCP、供应缺口和补货订单的影响。", true),
                new NetworkStructureExternalDependency("MasterGovernance", "外部主设置治理系统", "保存、评审、批准并下传主设置变更。", true),
            },
            new[]
            {
                "本 Host 不生成执行计划。",
                "本 Host 不保存场景、不审批、不下传主设置。",
                "本 Host 不执行候选动作组合选择；求解器后续只作为外部多方案比较阶段的候选筛选器。",
                "任何候选组合必须交给外部白盒引擎重算后才能比较。",
                "网络结构评分可以独立部署，外部系统只通过边界契约传入运行信号和接收候选证据。"
            });
    }
}
