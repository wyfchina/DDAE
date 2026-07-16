using AdaptiveSopDdsop.Web.Domain;
using System.Globalization;

namespace AdaptiveSopDdsop.Web.Data;

public sealed class SeedHistoryOperatingFactSource : IHistoryOperatingFactSource
{
    private static readonly IReadOnlyList<WeeklyOperatingFact> OperatingFacts = BuildOperatingFacts();
    private static readonly IReadOnlyList<WeeklyCapacityFact> CapacityFacts = BuildCapacityFacts();
    private static readonly IReadOnlyList<HistoryConstraintFact> ConstraintFacts = BuildConstraintFacts();
    private static readonly IReadOnlyList<HistoryAbnormalCostEvent> AbnormalCosts = BuildAbnormalCosts();
    private static readonly string[] HistoricalInventorySkus =
    {
        "AV-COM-201",
        "AV-OBC-202",
        "AV-FPGA-203",
        "TC-MLI-301",
        "TC-RAD-302",
    };
    private static readonly HashSet<string> TimeBufferCostEventIds = new(StringComparer.Ordinal)
    {
        "HAC-2026-002",
        "HAC-2025-003",
    };

    private readonly ValidationData _data;
    private readonly IReadOnlyList<WeeklyBufferFact> _bufferFacts;
    private readonly IReadOnlyList<WeeklyTimeBufferFact> _timeBufferFacts;
    private readonly IReadOnlyList<HistoricalDdmrpParameterFact> _ddmrpParameterFacts;
    private readonly IReadOnlyList<HistoricalCapacityProtectionFact> _capacityProtectionFacts;

    public SeedHistoryOperatingFactSource() : this(SeedData.Create()) { }

    public SeedHistoryOperatingFactSource(ValidationData data)
    {
        _data = data;
        _ddmrpParameterFacts = BuildDdmrpParameterFacts(data.Skus);
        _bufferFacts = BuildBufferFacts(_ddmrpParameterFacts);
        _timeBufferFacts = BuildTimeBufferFacts();
        _capacityProtectionFacts = BuildCapacityProtectionFacts();
    }

    public HistoryFactSet Load(HistoryFactRequest request)
    {
        var weeks = Math.Clamp(request.Weeks, 1, 52);
        var normalizedRequest = request with { Weeks = weeks };
        bool InWindow(int weekOffset) => weekOffset < 0 && Math.Abs(weekOffset) <= weeks;
        bool HistoricalRangeOverlapsWindow(int effectiveFromWeekOffset, int effectiveThroughWeekOffset) =>
            effectiveFromWeekOffset <= effectiveThroughWeekOffset &&
            effectiveFromWeekOffset < 0 &&
            effectiveThroughWeekOffset < 0 &&
            effectiveThroughWeekOffset >= -weeks;

        return new HistoryFactSet(
            normalizedRequest,
            OperatingFacts.Where(item => InWindow(item.WeekOffset)).ToList(),
            _bufferFacts.Where(item => InWindow(item.WeekOffset)).ToList(),
            CapacityFacts.Where(item => InWindow(item.WeekOffset)).ToList(),
            ConstraintFacts.Where(item => item.WeekOffset is null || InWindow(item.WeekOffset.Value)).ToList(),
            AbnormalCosts.Where(item => InWindow(item.WeekOffset)).ToList(),
            "DDAE DemoFixture explicit historical operating ledger",
            $"{request.AsOfDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}T23:59:59Z",
            $"DemoFixture / Explicit52WeekHistory / {weeks}-week historical window",
            _timeBufferFacts.Where(item => InWindow(item.WeekOffset)).ToList(),
            _ddmrpParameterFacts.Where(item => HistoricalRangeOverlapsWindow(item.EffectiveFromWeekOffset, item.EffectiveThroughWeekOffset)).ToList(),
            _capacityProtectionFacts.Where(item => HistoricalRangeOverlapsWindow(item.EffectiveFromWeekOffset, item.EffectiveThroughWeekOffset)).ToList());
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

    private static IReadOnlyList<HistoricalDdmrpParameterFact> BuildDdmrpParameterFacts(
        IReadOnlyList<SkuBufferSetting> skus)
    {
        const string fixtureCutoff = "2026-06-01T23:59:59Z";
        const string sourceAuthority = "DDAE DemoFixture registered validation data";

        return HistoricalInventorySkus
            .SelectMany(sku =>
            {
                var sourceSetting = skus.Single(item => item.Sku == sku);
                var currentSetting = sourceSetting with
                {
                    ParameterSnapshotId = $"HIST-{sourceSetting.Sku}-V2",
                    ParameterEvidenceStatus = "Complete"
                };
                var priorSetting = sourceSetting with
                {
                    Adu = decimal.Round(sourceSetting.Adu * 1.12m, 2),
                    LeadTimeFactor = Math.Min(1m, sourceSetting.LeadTimeFactor!.Value + 0.10m),
                    VariabilityFactor = Math.Min(1m, sourceSetting.VariabilityFactor + 0.10m),
                    ParameterSnapshotId = $"HIST-{sourceSetting.Sku}-V1",
                    ParameterEvidenceStatus = "Complete"
                };

                return new[]
                {
                    new HistoricalDdmrpParameterFact(
                        priorSetting.ParameterSnapshotId,
                        priorSetting.Sku,
                        priorSetting.Name,
                        priorSetting.DecouplingPoint,
                        -52,
                        -27,
                        priorSetting,
                        sourceAuthority,
                        fixtureCutoff,
                        "Complete"),
                    new HistoricalDdmrpParameterFact(
                        currentSetting.ParameterSnapshotId,
                        currentSetting.Sku,
                        currentSetting.Name,
                        currentSetting.DecouplingPoint,
                        -26,
                        -1,
                        currentSetting,
                        sourceAuthority,
                        fixtureCutoff,
                        "Complete"),
                };
            })
            .ToList();
    }

    private static IReadOnlyList<WeeklyBufferFact> BuildBufferFacts(
        IReadOnlyList<HistoricalDdmrpParameterFact> parameterFacts)
    {
        return HistoricalInventorySkus
            .SelectMany(sku => Enumerable.Range(1, 52).Select(week =>
            {
                var weekOffset = -week;
                var parameter = parameterFacts.Single(item =>
                    item.Sku == sku &&
                    item.EffectiveFromWeekOffset <= weekOffset &&
                    weekOffset <= item.EffectiveThroughWeekOffset);
                var sizing = DdmrpCalculator.CalculateSizing(parameter.Setting);
                var targetNetFlow = (week % 8) switch
                {
                    0 => sizing.Zones.TopOfRed * 0.55m,
                    1 => sizing.Zones.TopOfRed * 0.90m,
                    2 => (sizing.Zones.TopOfRed + sizing.Zones.TopOfYellow) / 2m,
                    3 => sizing.Zones.TopOfYellow * 0.95m,
                    4 => (sizing.Zones.TopOfYellow + sizing.Zones.TopOfGreen) / 2m,
                    5 => sizing.Zones.TopOfGreen * 0.85m,
                    6 => sizing.Zones.TopOfGreen * 1.12m,
                    _ => sizing.Zones.TopOfGreen * 0.72m
                };
                var endingOnHand = decimal.Round(targetNetFlow * 0.72m, 1);
                var openSupply = decimal.Round(targetNetFlow * 0.38m, 1);
                var qualifiedDemand = decimal.Round(endingOnHand + openSupply - targetNetFlow, 1);
                var endingNetFlow = endingOnHand + openSupply - qualifiedDemand;
                var cause = targetNetFlow <= sizing.Zones.TopOfRed
                    ? "供应交期延迟"
                    : week % 11 == 0
                        ? "需求超预期"
                        : targetNetFlow > sizing.Zones.TopOfGreen
                            ? "集中补货到货"
                            : "计划补充与消耗";

                return new WeeklyBufferFact(
                    sku,
                    weekOffset,
                    endingNetFlow,
                    cause,
                    "Complete",
                    endingOnHand,
                    openSupply,
                    qualifiedDemand,
                    parameter.ControlPoint,
                    parameter.SnapshotId);
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
                var plannedAvailableCapacity = decimal.Round(resource.RecentPlannedCapacity - resource.PlannedAgeDelta * age, 1);
                var committedLoadRatio = (week % 4) switch
                {
                    0 => 0.52m,
                    1 => 0.68m,
                    2 => 0.86m,
                    _ => 1.04m,
                };
                return new WeeklyCapacityFact(
                    resource.ResourceCode,
                    -week,
                    resource.TheoreticalCapacity,
                    resource.StandardCapacity,
                    decimal.Round(resource.RecentDemonstratedCapacity - resource.DemonstratedAgeDelta * age, 1),
                    plannedAvailableCapacity,
                    decimal.Round(plannedAvailableCapacity * committedLoadRatio, 1),
                    resource.LossReason,
                    "Complete");
            }))
            .ToList();
    }

    private static IReadOnlyList<WeeklyTimeBufferFact> BuildTimeBufferFacts()
    {
        return Enumerable.Range(1, 52)
            .Select(week =>
            {
                var weekOffset = -week;
                var counts = (week % 8) switch
                {
                    0 => (Early: 2, Green: 6, Yellow: 2, Red: 1, Late: 0),
                    1 => (Early: 1, Green: 8, Yellow: 1, Red: 0, Late: 0),
                    2 => (Early: 0, Green: 7, Yellow: 3, Red: 1, Late: 0),
                    3 => (Early: 0, Green: 5, Yellow: 3, Red: 2, Late: 1),
                    4 => (Early: 1, Green: 6, Yellow: 2, Red: 1, Late: 1),
                    5 => (Early: 3, Green: 6, Yellow: 1, Red: 0, Late: 0),
                    6 => (Early: 0, Green: 4, Yellow: 3, Red: 2, Late: 2),
                    _ => (Early: 1, Green: 7, Yellow: 2, Red: 0, Late: 1),
                };
                var costEvent = AbnormalCosts.SingleOrDefault(item =>
                    item.WeekOffset == weekOffset && TimeBufferCostEventIds.Contains(item.EventId));
                var cause = costEvent is not null
                    ? "异常成本事件已关联"
                    : counts.Late > 0
                        ? "试验件到位偏晚"
                        : counts.Red > 0
                            ? "准备活动进入红色时间带"
                            : "准备活动按窗口推进";

                return new WeeklyTimeBufferFact(
                    "MS-TB-001",
                    "热真空试验准备控制点",
                    "试验件到位与热真空窗口准备",
                    weekOffset,
                    counts.Early,
                    counts.Green,
                    counts.Yellow,
                    counts.Red,
                    counts.Late,
                    costEvent?.CostAmount,
                    costEvent?.EventId,
                    cause,
                    "Complete");
            })
            .ToList();
    }

    private IReadOnlyList<HistoricalCapacityProtectionFact> BuildCapacityProtectionFacts()
    {
        var sequenceEvidence = new List<(ResourceRouting Upstream, ResourceRouting Ccr)>();
        foreach (var upstream in _data.ResourceRoutings.Where(item =>
                     ProtectionProductEligibility.IsEligible(item.Sku) &&
                     item.ResourceCode == "RES-AIT" &&
                     item.ProtectsCcrResourceCode == "RES-HARNESS" &&
                     item.OperationSequence > 0 &&
                     item.EvidenceStatus == "Complete"))
        {
            var ccr = _data.ResourceRoutings
                .Where(item =>
                    item.Sku == upstream.Sku &&
                    item.ResourceCode == upstream.ProtectsCcrResourceCode &&
                    item.OperationSequence > upstream.OperationSequence &&
                    item.EvidenceStatus == "Complete")
                .OrderBy(item => item.OperationSequence)
                .FirstOrDefault();
            if (ccr is not null)
            {
                sequenceEvidence.Add((upstream, ccr));
            }
        }

        if (sequenceEvidence.Count == 0)
        {
            return Array.Empty<HistoricalCapacityProtectionFact>();
        }

        var selected = sequenceEvidence
            .OrderBy(item => item.Upstream.OperationSequence)
            .ThenBy(item => item.Ccr.OperationSequence)
            .First();
        return new[]
        {
            new HistoricalCapacityProtectionFact(
                "HIST-RES-AIT-RES-HARNESS-V1",
                selected.Upstream.ResourceCode,
                selected.Ccr.ResourceCode,
                selected.Upstream.OperationSequence,
                selected.Ccr.OperationSequence,
                20m,
                -52,
                -1,
                "Complete"),
        };
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
