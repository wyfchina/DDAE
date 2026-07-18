using AdaptiveSopDdsop.Web.Domain;
using System.Globalization;

namespace AdaptiveSopDdsop.Web.Data;

public sealed class SeedHistoryOperatingFactSource : IHistoryOperatingFactSource
{
    private static readonly IReadOnlyList<HistoryConstraintFact> ConstraintFacts = BuildConstraintFacts();
    private static readonly string[] HistoricalInventorySkus =
    {
        "AV-COM-201",
        "AV-OBC-202",
        "AV-FPGA-203",
        "TC-MLI-301",
        "TC-RAD-302",
    };
    private static readonly IReadOnlyList<HistoricalOpeningInventoryFact> HistoricalOpeningInventoryFacts =
    [
        new("AV-COM-201", 28m, 5_040_000m, "2025-06-02T00:00:00Z", "Complete"),
        new("AV-OBC-202", 20m, 6_400_000m, "2025-06-02T00:00:00Z", "Complete"),
        new("AV-FPGA-203", 22m, 11_880_000m, "2025-06-02T00:00:00Z", "Complete"),
        new("TC-MLI-301", 75m, 3_150_000m, "2025-06-02T00:00:00Z", "Complete"),
        new("TC-RAD-302", 48m, 4_128_000m, "2025-06-02T00:00:00Z", "Complete"),
    ];
    private static readonly IReadOnlyDictionary<int, IReadOnlySet<string>> InventoryEventOwners =
        new Dictionary<int, IReadOnlySet<string>>
    {
        [-46] = new HashSet<string>(["AV-COM-201", "AV-OBC-202"], StringComparer.Ordinal),
        [-39] = new HashSet<string>(["AV-FPGA-203"], StringComparer.Ordinal),
        [-21] = new HashSet<string>(["AV-COM-201", "AV-OBC-202", "TC-MLI-301"], StringComparer.Ordinal),
        [-11] = new HashSet<string>(["AV-FPGA-203"], StringComparer.Ordinal),
    };
    private static readonly IReadOnlyDictionary<int, IReadOnlySet<string>> CapacityEventOwners =
        new Dictionary<int, IReadOnlySet<string>>
    {
        [-33] = new HashSet<string>(["RES-AIT"], StringComparer.Ordinal),
        [-29] = new HashSet<string>(["RES-AIT"], StringComparer.Ordinal),
        [-16] = new HashSet<string>(["RES-AIT", "RES-CLEAN"], StringComparer.Ordinal),
        [-6] = new HashSet<string>(["RES-AIT", "RES-CLEAN"], StringComparer.Ordinal),
    };
    private static readonly IReadOnlyDictionary<int, NamedHistoryEvent> NamedEvents =
        new Dictionary<int, NamedHistoryEvent>
    {
        [-46] = new("需求变化", 1.36m, -2_000_000m, -0.8m, 5m, 1.5m, 0.88m, "HAC-2025-004", 240_000m, "需求应对", "需求对象", "星载电子", "星载电子需求控制点", "DDAE 演示历史事实台账"),
        [-39] = new("进口延迟", 1.12m, -7_000_000m, -2.0m, 8m, 3m, 0.45m, "HAC-2025-003", 180_000m, "加急运输", "库存控制点", "AV-FPGA-203", "关键进口 FPGA 库存控制点", "DDAE 演示历史事实台账"),
        [-33] = new("AIT 能力损失", 1.18m, -4_000_000m, -2.5m, 12m, 4m, 0.82m, "HAC-2026-001", 360_000m, "临时能力", "能力对象", "RES-AIT", "AIT 总装集成大厅", "DDAE 演示历史事实台账"),
        [-29] = new("恢复", 0.94m, 2_000_000m, 0.3m, -4m, -1m, 1.22m, null, null, null),
        [-21] = new("需求峰值", 1.72m, -6_000_000m, -1.8m, 9m, 2.5m, 0.86m, null, null, null),
        [-16] = new("返工", 1.08m, -3_000_000m, -2.2m, 14m, 4m, 0.92m, "HAC-2026-002", 420_000m, "返工费用", "时间缓冲", "MS-TB-001", "热真空试验准备控制点", "DDAE 演示历史事实台账"),
        [-11] = new("供应恢复", 0.92m, 3_000_000m, 0.4m, -5m, -1m, 1.38m, null, null, null),
        [-6] = new("节拍恢复", 0.88m, 2_000_000m, 0.5m, -7m, -1.5m, 1.18m, null, null, null),
    };
    private static readonly IReadOnlyList<AnnualWeekProfile> AnnualProfiles = BuildAnnualProfiles();

    private readonly ValidationData _data;
    private readonly IReadOnlyList<WeeklyOperatingFact> _operatingFacts;
    private readonly IReadOnlyList<WeeklyBufferFact> _bufferFacts;
    private readonly IReadOnlyList<WeeklyTimeBufferFact> _timeBufferFacts;
    private readonly IReadOnlyList<WeeklyCapacityFact> _capacityFacts;
    private readonly IReadOnlyList<HistoryAbnormalCostEvent> _abnormalCosts;
    private readonly IReadOnlyList<HistoricalDdmrpParameterFact> _ddmrpParameterFacts;
    private readonly IReadOnlyList<HistoricalCapacityProtectionFact> _capacityProtectionFacts;

    public SeedHistoryOperatingFactSource() : this(SeedData.Create()) { }

    public SeedHistoryOperatingFactSource(ValidationData data)
    {
        _data = data;
        _ddmrpParameterFacts = BuildDdmrpParameterFacts(data.Skus);
        _bufferFacts = BuildBufferFacts(_ddmrpParameterFacts, AnnualProfiles);
        _operatingFacts = BuildOperatingFacts(_bufferFacts, AnnualProfiles);
        _abnormalCosts = BuildAbnormalCosts();
        _timeBufferFacts = BuildTimeBufferFacts(AnnualProfiles, _abnormalCosts);
        _capacityFacts = BuildCapacityFacts(AnnualProfiles);
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
            _operatingFacts.Where(item => InWindow(item.WeekOffset)).ToList(),
            _bufferFacts.Where(item => InWindow(item.WeekOffset)).ToList(),
            _capacityFacts.Where(item => InWindow(item.WeekOffset)).ToList(),
            ConstraintFacts.Where(item => item.WeekOffset is null || InWindow(item.WeekOffset.Value)).ToList(),
            _abnormalCosts.Where(item => InWindow(item.WeekOffset)).ToList(),
            "DDAE DemoFixture explicit historical operating ledger",
            $"{request.AsOfDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}T23:59:59Z",
            $"DemoFixture / TraceableIrregularAnnualHistory / AnnualSource=52 / ViewWeeks={weeks} / HistoricalInventoryOpeningAsOf=2025-06-02T00:00:00Z",
            _timeBufferFacts.Where(item => InWindow(item.WeekOffset)).ToList(),
            _ddmrpParameterFacts.Where(item => HistoricalRangeOverlapsWindow(item.EffectiveFromWeekOffset, item.EffectiveThroughWeekOffset)).ToList(),
            _capacityProtectionFacts.Where(item => HistoricalRangeOverlapsWindow(item.EffectiveFromWeekOffset, item.EffectiveThroughWeekOffset)).ToList());
    }

    private static IReadOnlyList<AnnualWeekProfile> BuildAnnualProfiles()
    {
        var irregularDemandOrder = new[]
        {
            8, 23, 3, 31, 14, 42, 5, 27, 18, 49, 11, 36, 1,
            25, 44, 7, 20, 33, 12, 47, 29, 0, 39, 16, 51, 22,
            9, 34, 6, 45, 15, 30, 2, 41, 19, 50, 10, 28, 4,
            38, 17, 46, 13, 32, 24, 48, 21, 35, 26, 43, 37, 40,
        };

        return Enumerable.Range(0, 52)
            .Select(index =>
            {
                var weekOffset = index - 52;
                var baseDemandFactor = 0.82m + irregularDemandOrder[index] * 0.009m;
                NamedEvents.TryGetValue(weekOffset, out var namedEvent);
                var demandFactor = namedEvent?.DemandFactor ?? baseDemandFactor;
                var progress = index / 51m;
                var baseInventoryValue = 86_000_000m - 18_000_000m * progress;
                var inventoryValueTarget = baseInventoryValue - (demandFactor - 1m) * 2_000_000m +
                    (namedEvent?.InventoryValueAdjustment ?? 0m);
                return new AnnualWeekProfile(
                    weekOffset,
                    demandFactor,
                    decimal.Round(inventoryValueTarget, 0),
                    namedEvent);
            })
            .ToList();
    }

    private static IReadOnlyList<WeeklyOperatingFact> BuildOperatingFacts(
        IReadOnlyList<WeeklyBufferFact> bufferFacts,
        IReadOnlyList<AnnualWeekProfile> profiles)
    {
        var historicalUnitCostBySku = HistoricalOpeningInventoryFacts.ToDictionary(
            item => item.Sku,
            item => item.OpeningInventoryValue / item.OpeningQuantity,
            StringComparer.Ordinal);
        var buffersByWeek = bufferFacts
            .GroupBy(item => item.WeekOffset)
            .ToDictionary(group => group.Key, group => group.ToList());

        return profiles.Select(profile =>
        {
            var progress = (profile.WeekOffset + 52) / 51m;
            var weeklyBuffers = buffersByWeek[profile.WeekOffset];
            var actualDemand = weeklyBuffers.Sum(item => item.QualifiedDemand ?? 0m);
            var demandSpikeThreshold = weeklyBuffers.Sum(item => item.DemandSpikeThreshold ?? 0m);
            var targetNetFlow = weeklyBuffers.Sum(item => item.TargetNetFlowPosition ?? 0m);
            var inventoryValue = decimal.Round(
                weeklyBuffers.Sum(item => (item.EndingOnHand ?? 0m) * historicalUnitCostBySku[item.Sku]),
                0);
            var eventData = profile.Event;
            var service = decimal.Round(
                95.0m + 2.4m * progress + (eventData?.ServiceAdjustment ?? 0m),
                1);
            var workInProcess = decimal.Round(
                80m - 22m * progress + (profile.DemandFactor - 1m) * 6m +
                (eventData?.WorkInProcessAdjustment ?? 0m),
                1);
            var flowTime = decimal.Round(
                24.5m - 6.5m * progress + (eventData?.FlowTimeAdjustment ?? 0m),
                1);
            var cashFactor = 1.18m + (1m - progress) * 0.04m;
            var cashOccupied = decimal.Round(inventoryValue * cashFactor + workInProcess * 55_000m, 0);
            return new WeeklyOperatingFact(
                profile.WeekOffset,
                service,
                inventoryValue,
                workInProcess,
                flowTime,
                cashOccupied,
                "Complete",
                actualDemand,
                demandSpikeThreshold,
                targetNetFlow);
        }).ToList();
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
                var openingInventory = HistoricalOpeningInventoryFacts.Single(item => item.Sku == sku);
                var historicalUnitCost = openingInventory.OpeningInventoryValue / openingInventory.OpeningQuantity;
                var currentSetting = sourceSetting with
                {
                    UnitCost = historicalUnitCost,
                    ParameterSnapshotId = $"HIST-{sourceSetting.Sku}-V2",
                    ParameterEvidenceStatus = "Complete"
                };
                var priorSetting = currentSetting with
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
        IReadOnlyList<HistoricalDdmrpParameterFact> parameterFacts,
        IReadOnlyList<AnnualWeekProfile> profiles)
    {
        var openingFacts = HistoricalOpeningInventoryFacts.ToDictionary(item => item.Sku, StringComparer.Ordinal);
        var totalOpeningValue = openingFacts.Values.Sum(item => item.OpeningInventoryValue);

        return profiles.SelectMany(profile => HistoricalInventorySkus.Select(sku =>
        {
            var parameter = parameterFacts.Single(item =>
                item.Sku == sku &&
                item.EffectiveFromWeekOffset <= profile.WeekOffset &&
                profile.WeekOffset <= item.EffectiveThroughWeekOffset);
            var openingFact = openingFacts[sku];
            var historicalUnitCost = openingFact.OpeningInventoryValue / openingFact.OpeningQuantity;
            var inventoryShare = openingFacts[sku].OpeningInventoryValue / totalOpeningValue;
            var endingOnHand = decimal.Round(
                profile.InventoryValueTarget * inventoryShare / historicalUnitCost,
                2);
            var qualifiedDemand = decimal.Round(parameter.Setting.Adu * 7m * profile.DemandFactor, 2);
            var demandSpikeThreshold = decimal.Round(parameter.Setting.Adu * 7m * 1.35m, 2);
            var sizing = DdmrpCalculator.CalculateSizing(parameter.Setting);
            var targetNetFlowPosition = decimal.Round(
                (sizing.Zones.TopOfYellow + sizing.Zones.TopOfGreen) / 2m,
                2);
            var normalSupplyFactor = Math.Clamp(1.06m - (profile.DemandFactor - 1m) * 0.18m, 0.86m, 1.22m);
            var ownsEvent = InventoryEventOwners.TryGetValue(profile.WeekOffset, out var eventOwners) &&
                eventOwners.Contains(sku);
            var supplyFactor = ownsEvent && profile.Event is not null
                ? profile.Event.SupplyFactor
                : normalSupplyFactor;
            if (profile.WeekOffset == -39 && sku == "AV-FPGA-203")
            {
                supplyFactor = 0.18m;
            }

            var openSupply = decimal.Round(qualifiedDemand * supplyFactor, 2);
            var endingNetFlow = endingOnHand + openSupply - qualifiedDemand;
            return new WeeklyBufferFact(
                sku,
                profile.WeekOffset,
                endingNetFlow,
                ownsEvent ? profile.EventName : "无事件",
                "Complete",
                endingOnHand,
                openSupply,
                qualifiedDemand,
                parameter.ControlPoint,
                parameter.SnapshotId,
                demandSpikeThreshold,
                targetNetFlowPosition);
        })).ToList();
    }

    private static IReadOnlyList<WeeklyCapacityFact> BuildCapacityFacts(
        IReadOnlyList<AnnualWeekProfile> profiles)
    {
        var resources = new[]
        {
            new CapacityFixture("RES-AIT", 200m, 180m, 165m, 5m, 160m, 6m, 146m, 8m, "换线与质量复验损失"),
            new CapacityFixture("RES-HARNESS", 220m, 200m, 185m, 7m, 180m, 6m, 180m, 10m, "关键技师与线束工位约束"),
            new CapacityFixture("RES-TVAC", 120m, 105m, 100m, 5m, 96m, 5m, 78m, 6m, "舱体维护与校准损失"),
            new CapacityFixture("RES-CLEAN", 150m, 135m, 125m, 4m, 120m, 4m, 104m, 4m, "洁净切换与等待损失"),
        };

        return profiles.SelectMany(profile => resources.Select(resource =>
        {
            var progress = (profile.WeekOffset + 52) / 51m;
            var demonstrated = resource.RecentDemonstratedCapacity -
                resource.DemonstratedAgeDelta * (1m - progress);
            var plannedAvailableCapacity = resource.RecentPlannedCapacity -
                resource.PlannedAgeDelta * (1m - progress);
            var capacityFactor = 1m;
            if (profile.WeekOffset == -33 && resource.ResourceCode == "RES-AIT")
            {
                capacityFactor = 0.68m;
            }
            else if (profile.WeekOffset == -29 && resource.ResourceCode == "RES-AIT")
            {
                capacityFactor = 1.04m;
            }
            else if (profile.WeekOffset == -16 && resource.ResourceCode is "RES-AIT" or "RES-CLEAN")
            {
                capacityFactor = 0.82m;
            }
            else if (profile.WeekOffset == -6 && resource.ResourceCode is "RES-AIT" or "RES-CLEAN")
            {
                capacityFactor = 1.03m;
            }

            demonstrated = decimal.Round(demonstrated * Math.Min(1m, capacityFactor + 0.12m), 1);
            plannedAvailableCapacity = decimal.Round(plannedAvailableCapacity * capacityFactor, 1);
            var resourceLoadShift = resource.ResourceCode switch
            {
                "RES-HARNESS" => 0.08m,
                "RES-TVAC" => -0.04m,
                "RES-CLEAN" => 0.02m,
                _ => 0m,
            };
            var loadRatio = Math.Clamp(
                0.55m + (profile.DemandFactor - 0.82m) * 0.70m + resourceLoadShift,
                0.48m,
                1.12m);
            if (resource.ResourceCode == "RES-AIT")
            {
                loadRatio = profile.WeekOffset switch
                {
                    -52 => 0.52m,
                    -46 => 0.68m,
                    -33 => 1.04m,
                    -29 => 0.86m,
                    _ => loadRatio,
                };
            }

            var ownsEvent = CapacityEventOwners.TryGetValue(profile.WeekOffset, out var eventOwners) &&
                eventOwners.Contains(resource.ResourceCode);
            return new WeeklyCapacityFact(
                resource.ResourceCode,
                profile.WeekOffset,
                resource.TheoreticalCapacity,
                resource.StandardCapacity,
                demonstrated,
                plannedAvailableCapacity,
                decimal.Round(plannedAvailableCapacity * loadRatio, 1),
                ownsEvent ? profile.EventName : "无事件",
                "Complete");
        })).ToList();
    }

    private static IReadOnlyList<WeeklyTimeBufferFact> BuildTimeBufferFacts(
        IReadOnlyList<AnnualWeekProfile> profiles,
        IReadOnlyList<HistoryAbnormalCostEvent> abnormalCosts)
    {
        return profiles.Select(profile =>
        {
            var early = profile.DemandFactor < 0.92m ? 2 : profile.DemandFactor < 1m ? 1 : 0;
            var late = profile.DemandFactor > 1.20m ? 2 : profile.DemandFactor > 1.10m ? 1 : 0;
            var red = profile.DemandFactor > 1.16m ? 2 : profile.DemandFactor > 1.02m ? 1 : 0;
            var yellow = profile.DemandFactor >= 1m ? 2 : 1;
            var green = Math.Max(3, 11 - early - late - red - yellow);
            var counts = profile.WeekOffset switch
            {
                -46 => (Early: 0, Green: 4, Yellow: 4, Red: 2, Late: 1),
                -39 => (Early: 0, Green: 4, Yellow: 3, Red: 2, Late: 2),
                -33 => (Early: 0, Green: 3, Yellow: 3, Red: 3, Late: 2),
                -29 => (Early: 1, Green: 7, Yellow: 2, Red: 1, Late: 0),
                -21 => (Early: 0, Green: 3, Yellow: 3, Red: 3, Late: 3),
                -16 => (Early: 0, Green: 2, Yellow: 3, Red: 3, Late: 3),
                -11 => (Early: 2, Green: 7, Yellow: 2, Red: 0, Late: 0),
                -6 => (Early: 2, Green: 8, Yellow: 1, Red: 0, Late: 0),
                _ => (Early: early, Green: green, Yellow: yellow, Red: red, Late: late),
            };
            var costEvent = abnormalCosts.SingleOrDefault(item =>
                item.WeekOffset == profile.WeekOffset &&
                item.TargetType == "时间缓冲" &&
                item.TargetId == "MS-TB-001");
            var explicitCause = profile.Event is { TargetType: "时间缓冲", TargetId: "MS-TB-001" }
                ? profile.EventName
                : "无事件";

            return new WeeklyTimeBufferFact(
                "MS-TB-001",
                "热真空试验准备控制点",
                "试验件到位与热真空窗口准备",
                profile.WeekOffset,
                counts.Early,
                counts.Green,
                counts.Yellow,
                counts.Red,
                counts.Late,
                costEvent?.CostAmount,
                costEvent?.EventId,
                explicitCause,
                "Complete");
        }).ToList();
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
            new HistoryConstraintFact("事件型约束", "RES-CLEAN", -16, "Yellow", 88.2m, "返工和质量复验造成短时能力损失", "HistoricalFact", "Complete"),
            new HistoryConstraintFact("外部约束", "Microchip Space / 进口空间级 FPGA", -39, "Yellow", null, "FPGA 进口延迟与供应承诺历史证据", "HistoricalFact", "Complete"),
        };
    }

    private static IReadOnlyList<HistoryAbnormalCostEvent> BuildAbnormalCosts()
    {
        return NamedEvents
            .Where(item => item.Value is { EventId: not null, CostAmount: not null, CostType: not null })
            .OrderBy(item => item.Key)
            .Select(item => new HistoryAbnormalCostEvent(
                item.Value.EventId!,
                item.Key,
                item.Value.CostAmount!.Value,
                item.Value.CostType!,
                item.Value.Name,
                "Complete",
                item.Value.TargetType,
                item.Value.TargetId,
                item.Value.ControlPoint,
                item.Value.SourceAuthority))
            .ToList();
    }

    private sealed record NamedHistoryEvent(
        string Name,
        decimal DemandFactor,
        decimal InventoryValueAdjustment,
        decimal ServiceAdjustment,
        decimal WorkInProcessAdjustment,
        decimal FlowTimeAdjustment,
        decimal SupplyFactor,
        string? EventId,
        decimal? CostAmount,
        string? CostType,
        string? TargetType = null,
        string? TargetId = null,
        string? ControlPoint = null,
        string? SourceAuthority = null);

    private sealed record AnnualWeekProfile(
        int WeekOffset,
        decimal DemandFactor,
        decimal InventoryValueTarget,
        NamedHistoryEvent? Event)
    {
        public string EventName => Event?.Name ?? "无事件";
    }

    private sealed record HistoricalOpeningInventoryFact(
        string Sku,
        decimal OpeningQuantity,
        decimal OpeningInventoryValue,
        string AsOfUtc,
        string EvidenceStatus);

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
