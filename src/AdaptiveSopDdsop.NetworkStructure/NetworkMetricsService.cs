namespace AdaptiveSopDdsop.NetworkStructure;

public sealed class NetworkMetricsService
{
    private static readonly DateOnly AnchorDate = new(2026, 6, 1);

    private readonly INetworkMetricsDataSource _dataSource;

    public NetworkMetricsService(INetworkMetricsDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public NetworkMetricsWorkspaceResult GetBaseline(int horizonWeeks)
    {
        var horizon = Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52);
        var data = _dataSource.LoadNetworkMetrics(horizon, AnchorDate);
        var network = data.NetworkData;
        var itemByCode = network.Items.ToDictionary(item => item.ItemCode, StringComparer.Ordinal);
        var activeHeaderIds = network.BomHeaders
            .Where(header => header.ReleaseStatus == "Released"
                && header.EffectiveFrom <= data.AnchorDate
                && (header.EffectiveTo is null || header.EffectiveTo >= data.AnchorDate))
            .Select(header => header.BomId)
            .ToHashSet(StringComparer.Ordinal);
        var activeLines = network.BomLines
            .Where(line => activeHeaderIds.Contains(line.BomId))
            .ToList();
        var childrenByParent = activeLines
            .GroupBy(line => line.ParentItemCode, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var parentsByChild = activeLines
            .GroupBy(line => line.ComponentItemCode, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var maxUnitCost = Math.Max(1m, network.Items.Select(item => item.UnitCost).DefaultIfEmpty(1m).Max());

        var drafts = network.Items
            .Select(item => BuildDraft(
                item,
                data,
                activeLines,
                childrenByParent,
                parentsByChild,
                itemByCode,
                maxUnitCost))
            .ToList();
        var maxDownstream = Math.Max(1m, drafts.Max(item => item.DownstreamRaw));
        var maxQuantity = Math.Max(1m, drafts.Max(item => item.QuantityRaw));
        var maxLeadTime = Math.Max(1m, drafts.Max(item => item.LeadTimeRaw));
        var maxInventory = Math.Max(1m, drafts.Max(item => item.InventoryRaw));

        var metrics = drafts
            .Select(item => item.ToMetric(maxDownstream, maxQuantity, maxLeadTime, maxInventory))
            .OrderByDescending(item => item.DownstreamCoverageScore + item.QuantityImpactScore + item.SupplyRiskScore + item.ResourceConstraintScore)
            .ThenBy(item => item.ItemCode, StringComparer.Ordinal)
            .ToList();

        return new NetworkMetricsWorkspaceResult(
            horizon,
            "NetworkMetrics-V1",
            metrics,
            metrics.FirstOrDefault()?.ItemCode ?? string.Empty);
    }

    private static MetricDraft BuildDraft(
        NetworkItemMaster item,
        NetworkMetricsDataSet data,
        IReadOnlyList<NetworkBomLine> activeLines,
        IReadOnlyDictionary<string, List<NetworkBomLine>> childrenByParent,
        IReadOnlyDictionary<string, List<NetworkBomLine>> parentsByChild,
        IReadOnlyDictionary<string, NetworkItemMaster> itemByCode,
        decimal maxUnitCost)
    {
        var downstreamPaths = Traverse(item.ItemCode, parentsByChild, line => line.ParentItemCode, itemByCode, 8);
        var upstreamPaths = Traverse(item.ItemCode, childrenByParent, line => line.ComponentItemCode, itemByCode, 8);
        var relatedCodes = new HashSet<string>(
            downstreamPaths.SelectMany(path => path.ItemCodes).Concat(upstreamPaths.SelectMany(path => path.ItemCodes)).Append(item.ItemCode),
            StringComparer.Ordinal);
        var downstreamItems = downstreamPaths
            .Select(path => itemByCode.GetValueOrDefault(path.LeafItemCode))
            .OfType<NetworkItemMaster>()
            .ToList();
        var finishedGoods = downstreamItems
            .Where(node => node.ItemType == "FinishedGood")
            .Select(node => node.ItemCode)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (item.ItemType == "FinishedGood")
        {
            finishedGoods.Add(item.ItemCode);
        }

        var familyCount = downstreamItems
            .Where(node => node.ItemType is "FinishedGood" or "Subassembly")
            .Select(node => node.Family)
            .Append(item.Family)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var downstreamRaw = downstreamPaths.Count + finishedGoods.Count * 2m + familyCount;
        var downstreamEvidence = downstreamPaths
            .SelectMany(path => EvidenceForPath("BomLine", path, activeLines, "下游覆盖路径"))
            .Take(16)
            .ToList();

        var quantityRaw = downstreamPaths
            .Where(path => itemByCode.GetValueOrDefault(path.LeafItemCode)?.ItemType == "FinishedGood")
            .Sum(path => path.CumulativeQuantity * SkuAdu(data, path.LeafItemCode));
        if (item.ItemType == "FinishedGood")
        {
            quantityRaw += SkuAdu(data, item.ItemCode);
        }
        var quantityEvidence = downstreamPaths
            .Where(path => itemByCode.GetValueOrDefault(path.LeafItemCode)?.ItemType == "FinishedGood")
            .SelectMany(path => EvidenceForPath("BomLine", path, activeLines, $"累计用量 {path.CumulativeQuantity:0.####}"))
            .Take(16)
            .ToList();

        var upstreamCodes = upstreamPaths.SelectMany(path => path.ItemCodes).Append(item.ItemCode).Distinct(StringComparer.Ordinal).ToList();
        var supplierSources = data.NetworkData.SupplierSources.Where(source => upstreamCodes.Contains(source.ItemCode, StringComparer.Ordinal)).ToList();
        var leadProfiles = data.NetworkData.LeadTimeProfiles.Where(profile => upstreamCodes.Contains(profile.ItemCode, StringComparer.Ordinal) || upstreamCodes.Contains(profile.AppliesBeforeItemCode, StringComparer.Ordinal)).ToList();
        var bufferSettings = data.NetworkData.BufferSettings.Where(setting => upstreamCodes.Contains(setting.ItemCode, StringComparer.Ordinal)).ToList();
        var leadTimeRaw = supplierSources.Select(source => (decimal)source.LeadTimeDays).DefaultIfEmpty(0m).Sum()
            + leadProfiles.Sum(profile => profile.StandardLeadTimeDays * profile.VariabilityFactor)
            + bufferSettings.Sum(setting => setting.TimeBufferDays);
        var leadEvidence = supplierSources.Select(source => Evidence(
                "SupplierSource",
                SupplierKey(source),
                source.ItemCode,
                source.SupplierCode,
                $"供应提前期 {source.LeadTimeDays} 天，波动率 {source.LeadTimeVariabilityFactor:0.##}",
                source.LeadTimeDays,
                source.LeadTimeDays))
            .Concat(leadProfiles.Select(profile => Evidence(
                "LeadTimeProfile",
                $"{profile.ItemCode}|{profile.SourceType}|{profile.AppliesBeforeItemCode}",
                profile.ItemCode,
                profile.AppliesBeforeItemCode,
                $"提前期 profile {profile.StandardLeadTimeDays} 天，波动率 {profile.VariabilityFactor:0.##}",
                profile.StandardLeadTimeDays,
                profile.StandardLeadTimeDays * profile.VariabilityFactor)))
            .Concat(bufferSettings.Select(setting => Evidence(
                "BufferSetting",
                $"{setting.ItemCode}|{setting.InventoryBufferProfile}|{setting.EffectiveFrom:yyyy-MM-dd}",
                setting.ItemCode,
                setting.InventoryBufferProfile,
                $"时间缓冲 {setting.TimeBufferDays} 天",
                setting.TimeBufferDays,
                setting.TimeBufferDays)))
            .Take(18)
            .ToList();

        var supplyRiskRaw = supplierSources.Count == 0 && item.ItemType is "PurchasedPart" or "RawMaterial"
            ? 90m
            : supplierSources.Select(source => SupplierRisk(source, data.SupplierCapacity)).DefaultIfEmpty(0m).Max();
        var supplyEvidence = supplierSources.Select(source => Evidence(
                "SupplierSource",
                SupplierKey(source),
                source.ItemCode,
                source.SupplierCode,
                $"供应商 {source.SupplierName}，资格 {source.QualificationStatus}，周能力 {source.CapacityPerWeek:0.#}",
                source.CapacityPerWeek,
                SupplierRisk(source, data.SupplierCapacity)))
            .Take(12)
            .ToList();

        var routes = data.NetworkData.RoutingLines
            .Where(route => relatedCodes.Contains(route.ItemCode))
            .ToList();
        var loadByResource = data.ResourceLoads
            .GroupBy(load => load.ResourceCode, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Max(load => load.LoadPercent), StringComparer.Ordinal);
        var resourceRaw = routes
            .Select(route => loadByResource.GetValueOrDefault(route.ResourceCode, route.CapacityPerUnit * 100m))
            .DefaultIfEmpty(0m)
            .Max();
        var routingEvidence = routes.Select(route => Evidence(
                "RoutingLine",
                $"{route.ItemCode}|{route.RoutingVersion}|{route.OperationCode}|{route.ResourceCode}",
                route.ItemCode,
                route.ResourceCode,
                $"{route.OperationCode} 使用 {route.ResourceCode}，单位资源消耗 {route.CapacityPerUnit:0.####}",
                route.CapacityPerUnit,
                loadByResource.GetValueOrDefault(route.ResourceCode, route.CapacityPerUnit * 100m)))
            .Take(16)
            .ToList();

        var itemBuffers = data.NetworkData.BufferSettings.Where(setting => setting.ItemCode == item.ItemCode).ToList();
        var itemLocations = data.NetworkData.InventoryLocations.Where(location => location.ItemCode == item.ItemCode).ToList();
        var moq = itemBuffers.Select(setting => setting.MinimumOrderQuantity).DefaultIfEmpty(supplierSources.Select(source => source.MinimumOrderQuantity).DefaultIfEmpty(1m).Max()).Max();
        var inventoryRaw = item.UnitCost * Math.Max(1m, moq) * Math.Max(1m, downstreamPaths.Select(path => path.CumulativeQuantity).DefaultIfEmpty(1m).Max());
        var inventoryEvidence = itemBuffers.Select(setting => Evidence(
                "BufferSetting",
                $"{setting.ItemCode}|{setting.InventoryBufferProfile}|{setting.EffectiveFrom:yyyy-MM-dd}",
                setting.ItemCode,
                setting.InventoryBufferProfile,
                $"MOQ {setting.MinimumOrderQuantity:0.#}，订货周期 {setting.OrderCycleDays} 天",
                setting.MinimumOrderQuantity,
                setting.MinimumOrderQuantity * item.UnitCost))
            .Concat(itemLocations.Select(location => Evidence(
                "InventoryLocation",
                $"{location.ItemCode}|{location.LocationCode}|{location.QualityStatus}",
                location.ItemCode,
                location.LocationCode,
                $"{location.LocationName} / {location.LocationType} / {location.QualityStatus}",
                location.ShelfLifeDays ?? 0,
                item.UnitCost)))
            .Take(12)
            .ToList();

        return new MetricDraft(
            item,
            downstreamRaw,
            quantityRaw,
            leadTimeRaw,
            supplyRiskRaw,
            resourceRaw,
            inventoryRaw,
            downstreamEvidence,
            quantityEvidence,
            leadEvidence,
            supplyEvidence,
            routingEvidence,
            inventoryEvidence);
    }

    private static IReadOnlyList<PathTrace> Traverse(
        string start,
        IReadOnlyDictionary<string, List<NetworkBomLine>> adjacency,
        Func<NetworkBomLine, string> nextSelector,
        IReadOnlyDictionary<string, NetworkItemMaster> itemByCode,
        int maxDepth)
    {
        var paths = new List<PathTrace>();
        Walk(start, new[] { start }.ToList(), 1m, 0);
        return paths;

        void Walk(string current, List<string> path, decimal cumulativeQuantity, int depth)
        {
            if (depth >= maxDepth || !adjacency.TryGetValue(current, out var lines))
            {
                return;
            }

            foreach (var line in lines)
            {
                var next = nextSelector(line);
                var effectiveQuantity = line.QuantityPer * (1m + line.ScrapFactor);
                var nextQuantity = cumulativeQuantity * effectiveQuantity;
                var nextPath = path.Concat(new[] { next }).ToList();
                paths.Add(new PathTrace(
                    nextPath,
                    string.Join(" -> ", nextPath.Select(code => itemByCode.GetValueOrDefault(code)?.ItemName ?? code)),
                    next,
                    decimal.Round(nextQuantity, 4)));

                if (!path.Contains(next, StringComparer.Ordinal))
                {
                    Walk(next, nextPath, nextQuantity, depth + 1);
                }
            }
        }
    }

    private static IEnumerable<NetworkMetricEvidence> EvidenceForPath(
        string evidenceType,
        PathTrace path,
        IReadOnlyList<NetworkBomLine> activeLines,
        string descriptionPrefix)
    {
        for (var index = 0; index < path.ItemCodes.Count - 1; index++)
        {
            var from = path.ItemCodes[index];
            var to = path.ItemCodes[index + 1];
            var line = activeLines.FirstOrDefault(candidate =>
                (candidate.ParentItemCode == from && candidate.ComponentItemCode == to)
                || (candidate.ComponentItemCode == from && candidate.ParentItemCode == to));
            if (line is null)
            {
                continue;
            }

            yield return Evidence(
                evidenceType,
                BomLineKey(line),
                from,
                to,
                $"{descriptionPrefix}：{path.PathText}",
                line.QuantityPer,
                path.CumulativeQuantity);
        }
    }

    private static NetworkMetricEvidence Evidence(
        string evidenceType,
        string evidenceKey,
        string itemCode,
        string relatedCode,
        string description,
        decimal quantity,
        decimal scoreContribution)
    {
        return new NetworkMetricEvidence(
            evidenceType,
            evidenceKey,
            itemCode,
            relatedCode,
            description,
            decimal.Round(quantity, 4),
            decimal.Round(scoreContribution, 2));
    }

    private static string BomLineKey(NetworkBomLine line)
    {
        return $"{line.BomId}|{line.ParentItemCode}|{line.ComponentItemCode}";
    }

    private static string SupplierKey(NetworkSupplierSource source)
    {
        return $"{source.ItemCode}|{source.SupplierCode}";
    }

    private static decimal SkuAdu(NetworkMetricsDataSet data, string sku)
    {
        return data.SkuSignals.FirstOrDefault(item => item.Sku == sku)?.Adu ?? 0m;
    }

    private static decimal SupplierRisk(NetworkSupplierSource source, IReadOnlyList<NetworkMetricSupplierCapacitySignal> supplierCapacity)
    {
        var capacityRisk = supplierCapacity
            .Where(item => item.Supplier == source.SupplierName || item.Supplier == source.SupplierCode)
            .Select(item => item.RiskStatus switch
            {
                "Red" => 100m,
                "Yellow" => 70m,
                "Green" => 25m,
                _ => 0m,
            })
            .DefaultIfEmpty(0m)
            .Max();
        var leadTimeRisk = Normalize(source.LeadTimeDays, 126m);
        var variabilityRisk = Normalize(source.LeadTimeVariabilityFactor - 1m, 1m);
        var qualificationRisk = source.QualificationStatus == "Qualified" ? 0m : 65m;
        var capacityBaseRisk = source.CapacityPerWeek <= 0 ? 80m : 0m;
        return Clamp(capacityRisk * 0.35m + leadTimeRisk * 0.25m + variabilityRisk * 0.20m + qualificationRisk * 0.15m + capacityBaseRisk * 0.05m);
    }

    private static decimal Normalize(decimal value, decimal max)
    {
        return max <= 0m ? 0m : Clamp(value * 100m / max);
    }

    private static decimal Clamp(decimal value)
    {
        return Math.Min(100m, Math.Max(0m, value));
    }

    private sealed record PathTrace(
        IReadOnlyList<string> ItemCodes,
        string PathText,
        string LeafItemCode,
        decimal CumulativeQuantity);

    private sealed record MetricDraft(
        NetworkItemMaster Item,
        decimal DownstreamRaw,
        decimal QuantityRaw,
        decimal LeadTimeRaw,
        decimal SupplyRiskRaw,
        decimal ResourceRaw,
        decimal InventoryRaw,
        IReadOnlyList<NetworkMetricEvidence> DownstreamEvidence,
        IReadOnlyList<NetworkMetricEvidence> QuantityEvidence,
        IReadOnlyList<NetworkMetricEvidence> LeadEvidence,
        IReadOnlyList<NetworkMetricEvidence> SupplyEvidence,
        IReadOnlyList<NetworkMetricEvidence> RoutingEvidence,
        IReadOnlyList<NetworkMetricEvidence> InventoryEvidence)
    {
        public NetworkItemMetric ToMetric(decimal maxDownstream, decimal maxQuantity, decimal maxLeadTime, decimal maxInventory)
        {
            var downstreamScore = Normalize(DownstreamRaw, maxDownstream);
            var quantityScore = Normalize(QuantityRaw, maxQuantity);
            var leadTimeScore = Normalize(LeadTimeRaw, maxLeadTime);
            var resourceScore = Clamp(ResourceRaw);
            var inventoryScore = Normalize(InventoryRaw, maxInventory);

            return new NetworkItemMetric(
                Item.ItemCode,
                Item.ItemName,
                Item.ItemType,
                Item.Family,
                decimal.Round(downstreamScore, 1),
                decimal.Round(quantityScore, 1),
                decimal.Round(leadTimeScore, 1),
                decimal.Round(Clamp(SupplyRiskRaw), 1),
                decimal.Round(resourceScore, 1),
                decimal.Round(inventoryScore, 1),
                new NetworkMetricBreakdown(decimal.Round(downstreamScore, 1), decimal.Round(DownstreamRaw, 2), $"下游覆盖原始值 {DownstreamRaw:0.##}，来自父项、成品 SKU、产品族和路径数量。", DownstreamEvidence),
                new NetworkMetricBreakdown(decimal.Round(quantityScore, 1), decimal.Round(QuantityRaw, 2), $"数量影响原始值 {QuantityRaw:0.##}，来自 BOM 累计用量与下游 ADU。", QuantityEvidence),
                new NetworkMetricBreakdown(decimal.Round(leadTimeScore, 1), decimal.Round(LeadTimeRaw, 2), $"累计提前期原始值 {LeadTimeRaw:0.##} 天，来自供应提前期、时间缓冲和 lead time profile。", LeadEvidence),
                new NetworkMetricBreakdown(decimal.Round(Clamp(SupplyRiskRaw), 1), decimal.Round(SupplyRiskRaw, 2), $"供应风险原始值 {SupplyRiskRaw:0.##}，来自供应资格、能力、提前期和波动率。", SupplyEvidence),
                new NetworkMetricBreakdown(decimal.Round(resourceScore, 1), decimal.Round(ResourceRaw, 2), $"资源约束原始值 {ResourceRaw:0.##}%，来自 routing 与 RCCP 负荷。", RoutingEvidence),
                new NetworkMetricBreakdown(decimal.Round(inventoryScore, 1), decimal.Round(InventoryRaw, 2), $"库存代价原始值 {InventoryRaw:0.##}，来自单位成本、MOQ、缓冲和库存位置。", InventoryEvidence),
                $"{Item.ItemName}：下游覆盖 {downstreamScore:0.#}，数量影响 {quantityScore:0.#}，提前期 {leadTimeScore:0.#}，供应风险 {SupplyRiskRaw:0.#}，资源约束 {resourceScore:0.#}，库存代价 {inventoryScore:0.#}。");
        }
    }
}
