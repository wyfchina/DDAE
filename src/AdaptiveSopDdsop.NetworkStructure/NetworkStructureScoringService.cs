namespace AdaptiveSopDdsop.NetworkStructure;

public sealed class NetworkStructureScoringService
{
    private static readonly IReadOnlyList<NetworkStructureFactorWeight> FactorWeights =
    [
        new("下游覆盖度", 0.18m, "优先使用 BOM 图下游父项、成品 SKU、产品族和路径数量，SKU / 资源代理作为补充。"),
        new("累计提前期", 0.17m, "使用供应提前期、时间缓冲和提前期档案衡量保护窗口。"),
        new("数量影响度", 0.14m, "使用 BOM 路径累计用量与下游 ADU / 需求规模衡量该物料对网络数量的放大影响。"),
        new("供应风险", 0.18m, "使用图中采购件供应来源、供应商能力窗口红黄绿、供应提前期和供应缺口衡量风险。"),
        new("资源约束", 0.18m, "使用预计补货订单折算后的资源负荷信号衡量约束影响。"),
        new("服务影响", 0.15m, "使用下游成品覆盖、产品族目标服务水平、ADU 和未来需求规模衡量该点对交付承诺的影响。"),
        new("库存成本惩罚", -0.10m, "高单位成本物料会降低库存缓冲优先级，转向时间缓冲或管理取舍。"),
    ];

    private readonly INetworkScoringDataSource _dataSource;
    private readonly NetworkMetricsService _metricsService;

    public NetworkStructureScoringService(INetworkScoringDataSource dataSource, NetworkMetricsService metricsService)
    {
        _dataSource = dataSource;
        _metricsService = metricsService;
    }

    public NetworkStructureScoringResult GetBaseline(int horizonWeeks)
    {
        var horizon = Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52);
        var data = _dataSource.LoadNetworkScoring(horizon, new DateOnly(2026, 6, 1));
        var networkMetrics = _metricsService.GetBaseline(horizon);
        var candidates = BuildGraphMaterialCandidates(data, networkMetrics)
            .Concat(BuildSkuCandidates(data))
            .Concat(BuildResourceCandidates(data))
            .Concat(BuildSupplierCandidates(data))
            .Concat(BuildFamilyDecouplingCandidates(data))
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

        var summaries = candidates
            .GroupBy(item => item.RecommendedSettingType, StringComparer.Ordinal)
            .Select(group =>
            {
                var top = group.OrderByDescending(item => item.Score).First();
                return new NetworkStructureScoreSummary(
                    group.Key,
                    group.Count(),
                    decimal.Round(group.Average(item => item.Score), 1),
                    top.Score,
                    top.Target,
                    BuildSummaryText(group.Key, top));
            })
            .OrderByDescending(item => item.TopScore)
            .ToList();

        var recommendations = candidates
            .Take(8)
            .Select(item => new NetworkStructureRecommendation(
                item.Target,
                item.RecommendedSettingType,
                item.Severity,
                item.RecommendedAction))
            .ToList();

        return new NetworkStructureScoringResult(
            horizon,
            "NetworkScore-V2",
            "当前版本消费 Phase 4 网络指标计算结果，并结合外部场景运行系统提供的缓冲、资源、供应与库存位置运行信号形成白盒候选评分。",
            FactorWeights,
            summaries,
            candidates,
            recommendations,
            candidates.FirstOrDefault()?.CandidateId ?? string.Empty);
    }

    private static IReadOnlyList<NetworkStructureCandidate> BuildGraphMaterialCandidates(
        NetworkScoringDataSet data,
        NetworkMetricsWorkspaceResult networkMetrics)
    {
        var network = data.NetworkData;
        var maxCost = Math.Max(1m, network.Items.Select(item => item.UnitCost).DefaultIfEmpty(1m).Max());
        var metricByCode = networkMetrics.ItemMetrics.ToDictionary(item => item.ItemCode, StringComparer.Ordinal);

        return network.Items
            .Where(item => item.ItemType is "Subassembly" or "PurchasedPart" or "RawMaterial")
            .Where(item => metricByCode.ContainsKey(item.ItemCode))
            .Select(item =>
            {
                var metric = metricByCode[item.ItemCode];
                var hasBuffer = network.BufferSettings.Any(setting => setting.ItemCode == item.ItemCode && setting.IsDecouplingPoint);
                var hasExecutableLocation = network.InventoryLocations.Any(location =>
                    location.ItemCode == item.ItemCode
                    && location.QualityStatus == "Qualified"
                    && location.LocationType is "WipSupermarket" or "QualifiedStock" or "LineSide");
                var downstreamScore = metric.DownstreamCoverageScore;
                var quantityScore = metric.QuantityImpactScore;
                var leadTimeScore = metric.CumulativeLeadTimeScore;
                var supplyRisk = metric.SupplyRiskScore;
                var resourceConstraint = metric.ResourceConstraintScore;
                var serviceImpact = ClampScore(downstreamScore * 0.55m + quantityScore * 0.45m);
                var costPenalty = Normalize(item.UnitCost, maxCost);
                var score = Score(downstreamScore, leadTimeScore, quantityScore, supplyRisk, ClampScore(resourceConstraint), serviceImpact, costPenalty);
                var settingType = RecommendSettingType(item, hasBuffer, hasExecutableLocation, supplyRisk, resourceConstraint, (int)Math.Round(metric.DownstreamCoverage.RawValue, MidpointRounding.AwayFromZero));
                var severity = Severity(score);

                return new NetworkStructureCandidate(
                    $"NET-{item.ItemCode}",
                    "物料节点",
                    item.ItemCode,
                    item.ItemName,
                    item.Family,
                    settingType,
                    score,
                    severity,
                    decimal.Round(downstreamScore, 1),
                    decimal.Round(leadTimeScore, 1),
                    decimal.Round(quantityScore, 1),
                    decimal.Round(supplyRisk, 1),
                    decimal.Round(ClampScore(resourceConstraint), 1),
                    decimal.Round(costPenalty, 1),
                    decimal.Round(serviceImpact, 1),
                    $"{item.ItemName} 下游覆盖 {metric.DownstreamCoverage.RawValue:0.##}、数量影响 {metric.QuantityImpact.RawValue:0.##}、累计提前期 {metric.CumulativeLeadTime.RawValue:0.##} 天，适合作为 {settingType} 候选。",
                    RecommendedGraphAction(item, settingType, hasExecutableLocation),
                    MetricEvidence(metric),
                    NotAdoptingRisk(item, settingType, severity, (int)Math.Round(metric.DownstreamCoverage.RawValue, MidpointRounding.AwayFromZero), supplyRisk, resourceConstraint),
                    decimal.Round(quantityScore, 1));
            })
            .Where(item => item.Score >= 25m || item.SupplyRiskScore >= 70m || item.ReuseScore >= 35m)
            .ToList();
    }

    private static IReadOnlyList<NetworkStructureCandidate> BuildSkuCandidates(NetworkScoringDataSet data)
    {
        var maxLeadTime = Math.Max(1m, data.Skus.Select(item => (decimal)item.DecoupledLeadTimeDays).DefaultIfEmpty(1m).Max());
        var maxAdu = Math.Max(1m, data.Skus.Select(item => item.Adu).DefaultIfEmpty(1m).Max());
        var maxCost = Math.Max(1m, data.Skus.Select(item => item.UnitCost).DefaultIfEmpty(1m).Max());
        var maxFamilySkuCount = Math.Max(1, data.Families.Select(family => data.Skus.Count(sku => sku.Family == family.Code)).DefaultIfEmpty(1).Max());
        var capacityByResource = data.CapacityLoads
            .GroupBy(item => item.ResourceCode, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Max(item => item.LoadPercent), StringComparer.Ordinal);
        var supplyRiskBySource = data.SupplierCapacity
            .GroupBy(item => (item.Supplier, item.MaterialFamily))
            .ToDictionary(group => group.Key, group => group.Max(item => RiskScore(item.RiskStatus)), EqualityComparer<(string Supplier, string MaterialFamily)>.Default);

        return data.Skus.Select(sku =>
        {
            var familySkuCount = data.Skus.Count(item => item.Family == sku.Family);
            var source = data.SupplierItemSources.FirstOrDefault(item => item.Sku == sku.Sku);
            var sourceSkuCount = source is null
                ? 1
                : data.SupplierItemSources.Count(item => item.Supplier == source.Supplier && item.MaterialFamily == source.MaterialFamily);
            var skuResources = data.ResourceRoutings.Where(item => item.Sku == sku.Sku).ToList();
            var sharedResourceCount = skuResources
                .Select(item => item.ResourceCode)
                .Distinct(StringComparer.Ordinal)
                .Sum(resource => data.ResourceRoutings.Select(item => item.Sku).Distinct(StringComparer.Ordinal).Count(s => data.ResourceRoutings.Any(r => r.Sku == s && r.ResourceCode == resource)));
            var reuseScore = ClampScore(
                Normalize(familySkuCount, maxFamilySkuCount) * 0.45m
                + Normalize(sourceSkuCount, Math.Max(1, data.Skus.Count)) * 0.30m
                + Normalize(sharedResourceCount, Math.Max(1, data.Skus.Count * Math.Max(1, skuResources.Count))) * 0.25m);
            var demandValues = data.Demand.Where(item => item.Sku == sku.Sku).Select(item => item.BaselineDemand).ToList();
            var demandRangeScore = demandValues.Count == 0
                ? 0m
                : Normalize(demandValues.Max() - demandValues.Min(), Math.Max(1m, demandValues.Average()));
            var variabilityScore = ClampScore(Normalize(sku.VariabilityFactor - 1m, 1.4m) * 0.70m + demandRangeScore * 0.30m);
            var supplyRisk = source is null
                ? 0m
                : supplyRiskBySource.GetValueOrDefault((source.Supplier, source.MaterialFamily), 0m);
            var resourceConstraint = skuResources.Count == 0
                ? 0m
                : ClampScore(skuResources.Max(route => capacityByResource.GetValueOrDefault(route.ResourceCode, 0m)));
            var serviceImpact = ClampScore(Normalize(sku.Adu, maxAdu) * 0.45m + Normalize(sku.UnitCost, maxCost) * 0.25m + FamilyTargetFlow(data, sku.Family) * 0.30m);
            var costPenalty = Normalize(sku.UnitCost, maxCost);
            var score = Score(reuseScore, Normalize(sku.DecoupledLeadTimeDays, maxLeadTime), variabilityScore, supplyRisk, resourceConstraint, serviceImpact, costPenalty);
            var settingType = supplyRisk >= 75m && sku.UnitCost >= maxCost * 0.35m ? "时间缓冲" : "库存缓冲";
            var redWeeks = data.BufferProjections.Count(item => item.Sku == sku.Sku && item.BufferStatus == "Red");

            return new NetworkStructureCandidate(
                $"SKU-{sku.Sku}",
                "SKU",
                sku.Sku,
                sku.Name,
                sku.Family,
                settingType,
                score,
                Severity(score),
                decimal.Round(reuseScore, 1),
                decimal.Round(Normalize(sku.DecoupledLeadTimeDays, maxLeadTime), 1),
                decimal.Round(variabilityScore, 1),
                decimal.Round(supplyRisk, 1),
                decimal.Round(resourceConstraint, 1),
                decimal.Round(costPenalty, 1),
                decimal.Round(serviceImpact, 1),
                $"{sku.Name} DLT {sku.DecoupledLeadTimeDays} 天、变异 {sku.VariabilityFactor:0.0}、红区周 {redWeeks}，适合作为{settingType}候选。",
                settingType == "时间缓冲"
                    ? $"建议为 {sku.Name} 增加供应保护时间，并在主设置治理中确认 Act/Late 阈值。"
                    : $"建议重审 {sku.Name} 的库存缓冲、MOQ、订货周期和红黄绿区。",
                new[]
                {
                    $"缓冲上沿：红 {sku.TopOfRed:0.#} / 黄 {sku.TopOfYellow:0.#} / 绿 {sku.TopOfGreen:0.#}",
                    source is null ? "无供应来源映射" : $"供应来源：{source.Supplier} / {source.MaterialFamily}",
                    skuResources.Count == 0 ? "无工艺路线映射" : $"工艺路线：{string.Join("，", skuResources.Select(item => $"{item.ResourceCode} × {item.CapacityPerUnit:0.##}"))}",
                },
                redWeeks > 0
                    ? $"不采纳将继续保留 {redWeeks} 个红区穿透记录，可能导致服务损失或紧急补货。"
                    : $"不采纳会保留当前 {settingType} 风险，后续需求波动时仍需人工复核。");
        }).ToList();
    }

    private static IReadOnlyList<NetworkStructureCandidate> BuildResourceCandidates(NetworkScoringDataSet data)
    {
        return data.Resources.Select(resource =>
        {
            var loads = data.CapacityLoads.Where(item => item.ResourceCode == resource.Code).ToList();
            var peakLoad = loads.Count == 0 ? 0m : loads.Max(item => item.LoadPercent);
            var overloadWeeks = loads.Count(item => item.LoadPercent > 100m);
            var yellowWeeks = loads.Count(item => item.LoadPercent > 85m && item.LoadPercent <= 100m);
            var sharedSkuCount = data.ResourceRoutings.Where(item => item.ResourceCode == resource.Code).Select(item => item.Sku).Distinct(StringComparer.Ordinal).Count();
            var familyCount = data.ResourceRoutings
                .Where(item => item.ResourceCode == resource.Code)
                .Join(data.Skus, route => route.Sku, sku => sku.Sku, (_, sku) => sku.Family)
                .Distinct(StringComparer.Ordinal)
                .Count();
            var reuseScore = ClampScore(Normalize(sharedSkuCount, Math.Max(1, data.Skus.Count)) * 0.65m + Normalize(familyCount, Math.Max(1, data.Families.Count)) * 0.35m);
            var resourceConstraint = ClampScore(peakLoad);
            var serviceImpact = ClampScore(reuseScore * 0.55m + Normalize(overloadWeeks + yellowWeeks, data.HorizonWeeks) * 0.45m);
            var score = Score(reuseScore, 30m, 35m, 20m, resourceConstraint, serviceImpact, 25m);

            return new NetworkStructureCandidate(
                $"RES-{resource.Code}",
                "资源",
                resource.Code,
                resource.Name,
                "跨产品族",
                "能力缓冲",
                score,
                Severity(score),
                decimal.Round(reuseScore, 1),
                30m,
                35m,
                20m,
                decimal.Round(resourceConstraint, 1),
                25m,
                decimal.Round(serviceImpact, 1),
                $"{resource.Name} 峰值负荷 {peakLoad:0.#}%，覆盖 {sharedSkuCount} 个 SKU，是能力缓冲候选。",
                overloadWeeks > 0
                    ? $"建议为 {resource.Name} 设置能力缓冲边界，并把红色周升级为增班、外协或需求取舍。"
                    : $"建议为 {resource.Name} 设置黄色预警能力边界，保护未来峰值周。",
                new[]
                {
                    $"峰值负荷 {peakLoad:0.#}%",
                    $"红色超载周 {overloadWeeks}，黄色预警周 {yellowWeeks}",
                    $"影响产品族 {familyCount} 个，SKU {sharedSkuCount} 个",
                },
                overloadWeeks > 0
                    ? $"不采纳将使 {resource.Name} 的 {overloadWeeks} 个红色超载周继续存在，管理评审需另行做需求取舍。"
                    : $"不采纳将保留黄色能力预警，峰值周可能缺少提前保护动作。");
        }).ToList();
    }

    private static IReadOnlyList<NetworkStructureCandidate> BuildSupplierCandidates(NetworkScoringDataSet data)
    {
        return data.SupplierItemSources
            .GroupBy(item => (item.Supplier, item.MaterialFamily))
            .Select(group =>
            {
                var key = group.Key;
                var comparisons = data.SupplierCapacity.Where(item => item.Supplier == key.Supplier && item.MaterialFamily == key.MaterialFamily).ToList();
                var risk = comparisons.Select(item => RiskScore(item.RiskStatus)).DefaultIfEmpty(0m).Max();
                var totalGap = comparisons.Sum(item => item.Gap);
                var leadTime = group.Select(item => item.LeadTimeDays).DefaultIfEmpty(0).Max();
                var skuCount = group.Select(item => item.Sku).Distinct(StringComparer.Ordinal).Count();
                var demand = data.SupplyRequirements.Where(item => item.Supplier == key.Supplier && item.MaterialFamily == key.MaterialFamily).Sum(item => item.RequiredQuantity);
                var totalDemand = Math.Max(1m, data.SupplyRequirements.Sum(item => item.RequiredQuantity));
                var reuseScore = Normalize(skuCount, Math.Max(1, data.Skus.Count));
                var leadTimeScore = Normalize(leadTime, Math.Max(1, data.SupplierItemSources.Select(item => item.LeadTimeDays).DefaultIfEmpty(1).Max()));
                var serviceImpact = ClampScore(Normalize(demand, totalDemand) * 0.65m + reuseScore * 0.35m);
                var score = Score(reuseScore, leadTimeScore, 50m, risk, 35m, serviceImpact, 20m);

                return new NetworkStructureCandidate(
                    $"SUP-{key.Supplier}-{key.MaterialFamily}",
                    "供应商 / 物料族",
                    $"{key.Supplier} / {key.MaterialFamily}",
                    $"{key.Supplier} / {key.MaterialFamily}",
                    "跨产品族",
                    totalGap > 0m || risk >= 75m ? "时间缓冲" : "供应主设置",
                    score,
                    Severity(score),
                    decimal.Round(reuseScore, 1),
                    decimal.Round(leadTimeScore, 1),
                    50m,
                    decimal.Round(risk, 1),
                    35m,
                    20m,
                    decimal.Round(serviceImpact, 1),
                    $"{key.Supplier} / {key.MaterialFamily} 影响 {skuCount} 个 SKU，供应缺口 {totalGap:0.#}，适合作为供应与时间缓冲候选。",
                    totalGap > 0m
                        ? $"建议锁定 {key.Supplier} 的周度承诺能力，并在缺口周之前增加时间缓冲。"
                        : $"建议把 {key.Supplier} 的供应承诺窗口纳入主设置治理。",
                    new[]
                    {
                        $"供应提前期 {leadTime} 天",
                        $"影响 SKU {skuCount} 个",
                        $"供应缺口 {totalGap:0.#}",
                    },
                    totalGap > 0m
                        ? $"不采纳将保留 {totalGap:0.#} 的供应缺口，可能导致相关 SKU 补货计划无法兑现。"
                        : $"不采纳将缺少供应承诺窗口治理，供应波动出现时只能临时协调。");
            })
            .ToList();
    }

    private static IReadOnlyList<NetworkStructureCandidate> BuildFamilyDecouplingCandidates(NetworkScoringDataSet data)
    {
        return data.Families.Select(family =>
        {
            var skus = data.Skus.Where(item => item.Family == family.Code).ToList();
            var skuSet = skus.Select(item => item.Sku).ToHashSet(StringComparer.Ordinal);
            var avgLead = skus.Select(item => (decimal)item.DecoupledLeadTimeDays).DefaultIfEmpty(0m).Average();
            var avgVariability = skus.Select(item => item.VariabilityFactor).DefaultIfEmpty(1m).Average();
            var redWeeks = data.BufferProjections.Count(item => skuSet.Contains(item.Sku) && item.BufferStatus == "Red");
            var resources = data.ResourceRoutings.Where(item => skuSet.Contains(item.Sku)).Select(item => item.ResourceCode).Distinct(StringComparer.Ordinal).ToList();
            var resourceConstraint = resources.Select(resource => data.CapacityLoads.Where(item => item.ResourceCode == resource).Select(item => item.LoadPercent).DefaultIfEmpty(0m).Max()).DefaultIfEmpty(0m).Max();
            var supplyRisk = data.SupplierCapacity
                .Where(item => data.SupplierItemSources.Any(source => skuSet.Contains(source.Sku) && source.Supplier == item.Supplier && source.MaterialFamily == item.MaterialFamily))
                .Select(item => RiskScore(item.RiskStatus))
                .DefaultIfEmpty(0m)
                .Max();
            var reuseScore = ClampScore(Normalize(skus.Count, Math.Max(1, data.Skus.Count)) * 0.50m + Normalize(resources.Count, Math.Max(1, data.Resources.Count)) * 0.50m);
            var leadTimeScore = Normalize(avgLead, Math.Max(1m, data.Skus.Select(item => (decimal)item.DecoupledLeadTimeDays).DefaultIfEmpty(1m).Max()));
            var variabilityScore = Normalize(avgVariability - 1m, 1.4m);
            var serviceImpact = ClampScore(Normalize(family.TargetServiceLevel, 100m) * 0.40m + Normalize(family.TargetFlowIndex, 100m) * 0.35m + Normalize(redWeeks, Math.Max(1, data.HorizonWeeks * Math.Max(1, skus.Count))) * 0.25m);
            var score = Score(reuseScore, leadTimeScore, variabilityScore, supplyRisk, ClampScore(resourceConstraint), serviceImpact, 30m);

            return new NetworkStructureCandidate(
                $"FAM-{family.Code}",
                "产品族",
                family.Code,
                family.Name,
                family.Code,
                "解耦点",
                score,
                Severity(score),
                decimal.Round(reuseScore, 1),
                decimal.Round(leadTimeScore, 1),
                decimal.Round(variabilityScore, 1),
                decimal.Round(supplyRisk, 1),
                decimal.Round(ClampScore(resourceConstraint), 1),
                30m,
                decimal.Round(serviceImpact, 1),
                $"{family.Name} 覆盖 {skus.Count} 个 SKU、{resources.Count} 类关键资源，适合作为解耦点候选评估。",
                $"建议评估 {family.Code} 半成品或关键组件是否应作为控制点 / 解耦点，并进入 DDOM 主设置治理。",
                new[]
                {
                    $"产品族目标服务 {family.TargetServiceLevel:0.#}%，目标流速 {family.TargetFlowIndex:0.#}%",
                    $"红区穿透记录 {redWeeks} 条",
                    $"关联资源：{string.Join("，", resources)}",
                },
                redWeeks > 0
                    ? $"不采纳将使 {family.Name} 的红区穿透继续分散在 SKU 层处理，难以及时形成产品族级主设置变更。"
                    : $"不采纳将保留当前产品族解耦点假设，后续网络评分无法沉淀为治理动作。");
        }).ToList();
    }

    private static string BuildSummaryText(string settingType, NetworkStructureCandidate top)
    {
        return $"{settingType}候选 {top.Target} 得分最高，主要原因：{top.Rationale}";
    }

    private static IReadOnlyList<string> MetricEvidence(NetworkItemMetric metric)
    {
        return new[]
        {
            ("下游覆盖", metric.DownstreamCoverage),
            ("数量影响", metric.QuantityImpact),
            ("累计提前期", metric.CumulativeLeadTime),
            ("供应风险", metric.SupplyRisk),
            ("资源约束", metric.ResourceConstraint),
            ("库存代价", metric.InventoryCost),
        }
        .SelectMany(group => group.Item2.Evidence
            .Take(3)
            .Select(evidence => $"{group.Item1} / {EvidenceTypeLabel(evidence.EvidenceType)} / {evidence.EvidenceKey}：{evidence.Description}"))
        .DefaultIfEmpty($"{metric.ItemName} 暂无细项证据，需补齐 BOM、供应或工艺路线主数据。")
        .Take(12)
        .ToList();
    }

    private static string EvidenceTypeLabel(string evidenceType)
    {
        return evidenceType switch
        {
            "BomLine" => "BOM 行",
            "SupplierSource" => "供应来源",
            "RoutingLine" => "工艺路线行",
            "LeadTimeProfile" => "提前期档案",
            "BufferSetting" => "缓冲设置",
            "InventoryLocation" => "库存位置",
            _ => evidenceType,
        };
    }

    private static string RecommendSettingType(
        NetworkItemMaster item,
        bool hasBuffer,
        bool hasExecutableLocation,
        decimal supplyRisk,
        decimal resourceConstraint,
        int downstreamPathCount)
    {
        if (!hasBuffer && hasExecutableLocation && downstreamPathCount >= 3)
        {
            return "解耦点";
        }

        if (supplyRisk >= 70m && item.ItemType is "PurchasedPart" or "RawMaterial")
        {
            return "时间缓冲";
        }

        if (hasExecutableLocation && item.ItemType is "Subassembly" or "PurchasedPart")
        {
            return "库存缓冲";
        }

        if (resourceConstraint >= 75m && item.ItemType == "Subassembly")
        {
            return "能力缓冲";
        }

        return "只监控";
    }

    private static string RecommendedGraphAction(NetworkItemMaster item, string settingType, bool hasExecutableLocation)
    {
        return settingType switch
        {
            "解耦点" => hasExecutableLocation
                ? $"建议评审 {item.ItemName} 是否作为物料解耦点，并把库存位置、MOQ、订货周期纳入 DDOM 主设置治理。"
                : $"建议先补齐 {item.ItemName} 的可执行库存位置，再评审是否成为物料解耦点。",
            "库存缓冲" => $"建议重审 {item.ItemName} 的库存缓冲区、MOQ、订货周期和库存责任位置。",
            "时间缓冲" => $"建议为 {item.ItemName} 设置或调整供应时间缓冲，并确认供应承诺窗口。",
            "能力缓冲" => $"建议把 {item.ItemName} 相关工艺资源纳入能力缓冲评审。",
            _ => $"建议保留 {item.ItemName} 为网络监控点，后续随供应或需求变化再升级。",
        };
    }

    private static string NotAdoptingRisk(
        NetworkItemMaster item,
        string settingType,
        string severity,
        int finishedGoodCount,
        decimal supplyRisk,
        decimal resourceConstraint)
    {
        if (severity == "Red")
        {
            return $"不采纳将使 {item.ItemName} 对 {finishedGoodCount} 个下游成品的影响继续缺少主设置保护；供应风险 {supplyRisk:0.#}、资源约束 {resourceConstraint:0.#} 仍需管理评审人工兜底。";
        }

        if (settingType == "只监控")
        {
            return $"不采纳不会立即改变计划，但 {item.ItemName} 将只保留为监控对象，后续异常需要重新进入网络评分。";
        }

        return $"不采纳将保留 {item.ItemName} 的 {settingType} 候选状态，后续波动出现时可能只能通过临时催交、增班或供应协调处理。";
    }

    private static decimal Score(
        decimal reuse,
        decimal leadTime,
        decimal variability,
        decimal supplyRisk,
        decimal resourceConstraint,
        decimal serviceImpact,
        decimal inventoryCostPenalty)
    {
        return decimal.Round(ClampScore(
            reuse * 0.18m
            + leadTime * 0.17m
            + variability * 0.14m
            + supplyRisk * 0.18m
            + resourceConstraint * 0.18m
            + serviceImpact * 0.15m
            - inventoryCostPenalty * 0.10m), 1);
    }

    private static decimal RiskScore(string status)
    {
        return status switch
        {
            "Red" => 100m,
            "Yellow" => 70m,
            "Green" => 25m,
            _ => 0m,
        };
    }

    private static string Severity(decimal score)
    {
        return score >= 75m ? "Red" : score >= 55m ? "Yellow" : "Green";
    }

    private static decimal FamilyTargetFlow(NetworkScoringDataSet data, string family)
    {
        var target = data.Families.FirstOrDefault(item => item.Code == family)?.TargetFlowIndex ?? 80m;
        return Normalize(target, 100m);
    }

    private static decimal Normalize(decimal value, decimal max)
    {
        return max <= 0m ? 0m : ClampScore(value * 100m / max);
    }

    private static decimal ClampScore(decimal value)
    {
        return Math.Min(100m, Math.Max(0m, value));
    }
}
