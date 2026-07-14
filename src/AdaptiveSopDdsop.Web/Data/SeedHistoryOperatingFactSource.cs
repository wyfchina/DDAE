using AdaptiveSopDdsop.Web.Domain;
using System.Globalization;

namespace AdaptiveSopDdsop.Web.Data;

public sealed class SeedHistoryOperatingFactSource : IHistoryOperatingFactSource
{
    private static readonly IReadOnlyList<WeeklyOperatingFact> OperatingFacts = BuildOperatingFacts();
    private static readonly IReadOnlyList<WeeklyBufferFact> BufferFacts = BuildBufferFacts();
    private static readonly IReadOnlyList<WeeklyCapacityFact> CapacityFacts = BuildCapacityFacts();
    private static readonly IReadOnlyList<HistoryConstraintFact> ConstraintFacts = BuildConstraintFacts();
    private static readonly IReadOnlyList<HistoryAbnormalCostEvent> AbnormalCosts = BuildAbnormalCosts();

    public HistoryFactSet Load(HistoryFactRequest request)
    {
        var weeks = Math.Clamp(request.Weeks, 1, 52);
        var normalizedRequest = request with { Weeks = weeks };
        bool InWindow(int weekOffset) => weekOffset < 0 && Math.Abs(weekOffset) <= weeks;

        return new HistoryFactSet(
            normalizedRequest,
            OperatingFacts.Where(item => InWindow(item.WeekOffset)).ToList(),
            BufferFacts.Where(item => InWindow(item.WeekOffset)).ToList(),
            CapacityFacts.Where(item => InWindow(item.WeekOffset)).ToList(),
            ConstraintFacts.Where(item => item.WeekOffset is null || InWindow(item.WeekOffset.Value)).ToList(),
            AbnormalCosts.Where(item => InWindow(item.WeekOffset)).ToList(),
            "DDAE DemoFixture explicit historical operating ledger",
            $"{request.AsOfDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}T23:59:59Z",
            $"DemoFixture / Explicit52WeekHistory / {weeks}-week historical window");
    }

    private static IReadOnlyList<WeeklyOperatingFact> BuildOperatingFacts()
    {
        return Enumerable.Range(1, 52)
            .Select(week =>
            {
                var age = (week - 1) / 51m;
                return new WeeklyOperatingFact(
                    -week,
                    decimal.Round(98.2m - 4.4m * age, 1),
                    decimal.Round(65_000_000m + 30_000_000m * age, 0),
                    decimal.Round(55m + 40m * age, 1),
                    decimal.Round(17m + 12m * age, 1),
                    decimal.Round(78_000_000m + 47_000_000m * age, 0),
                    "Complete");
            })
            .ToList();
    }

    private static IReadOnlyList<WeeklyBufferFact> BuildBufferFacts()
    {
        var buffers = new (string Sku, decimal ReferenceFlow)[]
        {
            ("AV-COM-201", 7.2m),
            ("AV-OBC-202", 6.4m),
            ("AV-FPGA-203", 2.9m),
            ("TC-MLI-301", 20m),
            ("TC-RAD-302", 15m),
        };

        return buffers
            .SelectMany(buffer => Enumerable.Range(1, 52).Select(week =>
            {
                var factor = (week % 8) switch
                {
                    0 => 0.55m,
                    1 => 0.75m,
                    2 => 1.05m,
                    3 => 1.35m,
                    4 => 1.75m,
                    5 => 2.10m,
                    6 => 1.55m,
                    _ => 1.15m,
                };
                var cause = factor <= 0.75m
                    ? "供应交期延迟"
                    : week % 11 == 0
                        ? "需求超预期"
                        : factor >= 2m
                            ? "集中补货到货"
                            : "计划补充与消耗";
                return new WeeklyBufferFact(
                    buffer.Sku,
                    -week,
                    decimal.Round(buffer.ReferenceFlow * factor, 1),
                    cause,
                    "Complete");
            }))
            .ToList();
    }

    private static IReadOnlyList<WeeklyCapacityFact> BuildCapacityFacts()
    {
        var resources = new[]
        {
            new CapacityFixture("RES-AIT", 200m, 180m, 165m, 5m, 160m, 6m, 146m, 8m, "换线与质量复验损失"),
            new CapacityFixture("RES-HARNESS", 220m, 200m, 185m, 7m, 180m, 6m, 180m, 10m, "关键技师与线束工位约束"),
            new CapacityFixture("RES-TVAC", 120m, 105m, 100m, 5m, 96m, 5m, 78m, 6m, "舱体维护与校准损失"),
            new CapacityFixture("RES-CLEAN", 150m, 135m, 125m, 4m, 120m, 4m, 104m, 4m, "洁净切换与等待损失"),
        };

        return resources
            .SelectMany(resource => Enumerable.Range(1, 52).Select(week =>
            {
                var age = (week - 1) / 51m;
                return new WeeklyCapacityFact(
                    resource.ResourceCode,
                    -week,
                    resource.TheoreticalCapacity,
                    resource.StandardCapacity,
                    decimal.Round(resource.RecentDemonstratedCapacity - resource.DemonstratedAgeDelta * age, 1),
                    decimal.Round(resource.RecentPlannedCapacity - resource.PlannedAgeDelta * age, 1),
                    decimal.Round(resource.RecentCommittedLoad + resource.CommittedAgeDelta * age, 1),
                    resource.LossReason,
                    "Complete");
            }))
            .ToList();
    }

    private static IReadOnlyList<HistoryConstraintFact> BuildConstraintFacts()
    {
        return new[]
        {
            new HistoryConstraintFact("当前 CCR", "RES-HARNESS", -3, "Red", 99.1m, "线束工位历史承诺负荷持续接近经验证能力", "HistoricalFact", "Complete"),
            new HistoryConstraintFact("高负荷资源", "RES-AIT", -5, "Yellow", 92.4m, "AIT 上游保护已被部分消耗", "HistoricalFact", "Complete"),
            new HistoryConstraintFact("场景潜在 CCR", "RES-TVAC", null, "Yellow", 86.5m, "热真空能力损失情景下可能转化为 CCR", "InternalScenarioDefinition", "Complete"),
            new HistoryConstraintFact("事件型约束", "RES-CLEAN", -9, "Yellow", 88.2m, "洁净切换和质量复验造成短时能力损失", "HistoricalFact", "Complete"),
            new HistoryConstraintFact("外部约束", "Microchip Space / 进口空间级 FPGA", -12, "Yellow", null, "供应承诺与进口提前期历史证据", "HistoricalFact", "Complete"),
        };
    }

    private static IReadOnlyList<HistoryAbnormalCostEvent> BuildAbnormalCosts()
    {
        return new[]
        {
            new HistoryAbnormalCostEvent("HAC-2026-001", -4, 180_000m, "加急运输", "进口空间级 FPGA 交期恢复", "Complete"),
            new HistoryAbnormalCostEvent("HAC-2026-002", -18, 240_000m, "临时外协", "线束工位短时负荷超过经验证能力", "Complete"),
            new HistoryAbnormalCostEvent("HAC-2025-003", -34, 360_000m, "返工", "洁净装配质量复验", "Complete"),
            new HistoryAbnormalCostEvent("HAC-2025-004", -47, 420_000m, "替代料认证", "关键进口器件替代方案认证", "Complete"),
        };
    }

    private sealed record CapacityFixture(
        string ResourceCode,
        decimal TheoreticalCapacity,
        decimal StandardCapacity,
        decimal RecentDemonstratedCapacity,
        decimal DemonstratedAgeDelta,
        decimal RecentPlannedCapacity,
        decimal PlannedAgeDelta,
        decimal RecentCommittedLoad,
        decimal CommittedAgeDelta,
        string LossReason);
}
