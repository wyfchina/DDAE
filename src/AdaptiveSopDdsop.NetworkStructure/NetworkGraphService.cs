namespace AdaptiveSopDdsop.NetworkStructure;

public sealed class NetworkGraphService
{
    private static readonly DateOnly AnchorDate = new(2026, 6, 1);

    private readonly INetworkGraphDataSource _dataSource;

    public NetworkGraphService(INetworkGraphDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public NetworkGraphWorkspaceResult GetGraph(string? itemCode, int maxDepth)
    {
        var depth = Math.Clamp(maxDepth <= 0 ? 6 : maxDepth, 1, 12);
        var data = _dataSource.LoadNetworkGraph(AnchorDate);
        var network = data.NetworkData;
        var itemByCode = network.Items.ToDictionary(item => item.ItemCode, StringComparer.Ordinal);
        var selectedCode = string.IsNullOrWhiteSpace(itemCode)
            ? DefaultItemCode(network)
            : itemCode.Trim();
        if (!itemByCode.ContainsKey(selectedCode))
        {
            selectedCode = DefaultItemCode(network);
        }

        var activeHeaders = network.BomHeaders
            .Where(header => header.ReleaseStatus == "Released"
                && header.EffectiveFrom <= data.AnchorDate
                && (header.EffectiveTo is null || header.EffectiveTo >= data.AnchorDate))
            .ToList();
        var activeHeaderIds = activeHeaders.Select(header => header.BomId).ToHashSet(StringComparer.Ordinal);
        var activeLines = network.BomLines
            .Where(line => activeHeaderIds.Contains(line.BomId))
            .ToList();
        var nodes = network.Items
            .Select(item => BuildNode(item, network))
            .OrderBy(item => item.ItemCode, StringComparer.Ordinal)
            .ToList();
        var nodeByCode = nodes.ToDictionary(item => item.ItemCode, StringComparer.Ordinal);
        var edges = activeLines
            .Select(line => BuildEdge(line, itemByCode))
            .ToList();
        var upstream = BuildScope("上游", selectedCode, activeLines, itemByCode, nodeByCode, depth, line => line.ParentItemCode, line => line.ComponentItemCode);
        var downstream = BuildScope("下游", selectedCode, activeLines, itemByCode, nodeByCode, depth, line => line.ComponentItemCode, line => line.ParentItemCode);
        var validation = Validate(network, activeHeaders, activeLines, data.AnchorDate);
        var selectedItem = itemByCode.GetValueOrDefault(selectedCode);

        return new NetworkGraphWorkspaceResult(
            selectedCode,
            selectedItem?.ItemName ?? selectedCode,
            depth,
            nodes,
            edges,
            upstream,
            downstream,
            validation);
    }

    private static string DefaultItemCode(NetworkDataSet network)
    {
        return network.Items.Any(item => item.ItemCode == "PART-FPGA-SPACE")
            ? "PART-FPGA-SPACE"
            : network.Items.FirstOrDefault()?.ItemCode ?? string.Empty;
    }

    private static NetworkGraphNode BuildNode(NetworkItemMaster item, NetworkDataSet network)
    {
        return new NetworkGraphNode(
            item.ItemCode,
            item.ItemName,
            item.ItemType,
            item.Family,
            item.LifecycleStatus,
            item.UnitCost,
            item.PlanningUom,
            network.BufferSettings.Any(setting => setting.ItemCode == item.ItemCode && setting.IsDecouplingPoint),
            network.InventoryLocations.Any(location => location.ItemCode == item.ItemCode),
            network.SupplierSources.Any(source => source.ItemCode == item.ItemCode),
            network.RoutingLines.Any(route => route.ItemCode == item.ItemCode));
    }

    private static NetworkGraphEdge BuildEdge(
        NetworkBomLine line,
        IReadOnlyDictionary<string, NetworkItemMaster> itemByCode)
    {
        var parentName = itemByCode.GetValueOrDefault(line.ParentItemCode)?.ItemName ?? line.ParentItemCode;
        var componentName = itemByCode.GetValueOrDefault(line.ComponentItemCode)?.ItemName ?? line.ComponentItemCode;
        var effectiveQuantity = line.QuantityPer * (1m + line.ScrapFactor);
        return new NetworkGraphEdge(
            line.BomId,
            line.ParentItemCode,
            parentName,
            line.ComponentItemCode,
            componentName,
            line.QuantityPer,
            line.ScrapFactor,
            decimal.Round(effectiveQuantity, 4),
            line.AlternateGroup);
    }

    private static NetworkImpactScope BuildScope(
        string direction,
        string selectedCode,
        IReadOnlyList<NetworkBomLine> activeLines,
        IReadOnlyDictionary<string, NetworkItemMaster> itemByCode,
        IReadOnlyDictionary<string, NetworkGraphNode> nodeByCode,
        int maxDepth,
        Func<NetworkBomLine, string> fromSelector,
        Func<NetworkBomLine, string> toSelector)
    {
        var adjacency = activeLines
            .GroupBy(fromSelector, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var paths = new List<NetworkImpactPath>();
        var nodeCodes = new HashSet<string>(StringComparer.Ordinal) { selectedCode };

        Walk(selectedCode, new List<string> { selectedCode }, 1m, 0);

        var nodes = nodeCodes
            .Where(nodeByCode.ContainsKey)
            .Select(code => nodeByCode[code])
            .OrderBy(item => item.ItemCode, StringComparer.Ordinal)
            .ToList();

        return new NetworkImpactScope(
            direction,
            nodes.Count,
            paths.Count,
            paths.Count == 0 ? 0 : paths.Max(item => item.Depth),
            nodes,
            paths.OrderBy(item => item.Depth).ThenBy(item => item.PathText, StringComparer.Ordinal).ToList());

        void Walk(string currentCode, List<string> path, decimal cumulativeQuantity, int depth)
        {
            if (depth >= maxDepth || !adjacency.TryGetValue(currentCode, out var lines))
            {
                return;
            }

            foreach (var line in lines)
            {
                var nextCode = toSelector(line);
                var effectiveQuantity = line.QuantityPer * (1m + line.ScrapFactor);
                var nextQuantity = cumulativeQuantity * effectiveQuantity;
                var nextPath = path.Concat(new[] { nextCode }).ToList();
                nodeCodes.Add(nextCode);
                var leaf = nodeByCode.GetValueOrDefault(nextCode);
                paths.Add(new NetworkImpactPath(
                    direction,
                    nextPath,
                    BuildPathText(nextPath, itemByCode),
                    depth + 1,
                    decimal.Round(nextQuantity, 4),
                    nextCode,
                    itemByCode.GetValueOrDefault(nextCode)?.ItemName ?? nextCode,
                    leaf?.IsDecouplingPoint ?? false,
                    leaf?.HasInventoryLocation ?? false,
                    leaf?.HasSupplierSource ?? false,
                    leaf?.HasRouting ?? false));

                if (!path.Contains(nextCode, StringComparer.Ordinal))
                {
                    Walk(nextCode, nextPath, nextQuantity, depth + 1);
                }
            }
        }
    }

    private static string BuildPathText(
        IReadOnlyList<string> path,
        IReadOnlyDictionary<string, NetworkItemMaster> itemByCode)
    {
        return string.Join(" -> ", path.Select(code => itemByCode.GetValueOrDefault(code)?.ItemName ?? code));
    }

    private static NetworkValidationReport Validate(
        NetworkDataSet network,
        IReadOnlyList<NetworkBomHeader> activeHeaders,
        IReadOnlyList<NetworkBomLine> activeLines,
        DateOnly anchorDate)
    {
        var issues = new List<NetworkValidationIssue>();
        var itemByCode = network.Items.ToDictionary(item => item.ItemCode, StringComparer.Ordinal);
        var allHeaderIds = network.BomHeaders.Select(header => header.BomId).ToHashSet(StringComparer.Ordinal);
        var activeHeaderIds = activeHeaders.Select(header => header.BomId).ToHashSet(StringComparer.Ordinal);
        var usedItems = new HashSet<string>(StringComparer.Ordinal);

        foreach (var header in network.BomHeaders.Where(header => !itemByCode.ContainsKey(header.ParentItemCode)))
        {
            issues.Add(Issue("Red", "MissingBomParent", header.ParentItemCode, header.ParentItemCode, "BOM header 的父项物料不存在。", header.BomId));
        }

        foreach (var line in network.BomLines.Where(line => !allHeaderIds.Contains(line.BomId)))
        {
            issues.Add(Issue("Info", "BomLineWithoutHeader", line.ParentItemCode, line.ComponentItemCode, "BOM line 未绑定 BOM header。", line.BomId));
        }

        foreach (var line in activeLines)
        {
            usedItems.Add(line.ParentItemCode);
            usedItems.Add(line.ComponentItemCode);
            if (!itemByCode.ContainsKey(line.ComponentItemCode))
            {
                issues.Add(Issue("Red", "MissingBomComponent", line.ComponentItemCode, line.ComponentItemCode, "BOM line 引用的组件物料不存在。", line.BomId));
            }
        }

        foreach (var buffer in network.BufferSettings.Where(setting => !itemByCode.ContainsKey(setting.ItemCode)))
        {
            issues.Add(Issue("Red", "MissingBufferItem", buffer.ItemCode, buffer.ItemCode, "Buffer setting 指向不存在的物料。", buffer.InventoryBufferProfile));
        }

        foreach (var cycle in DetectCycles(activeLines))
        {
            issues.Add(Issue("Red", "BomCycle", cycle[0], itemByCode.GetValueOrDefault(cycle[0])?.ItemName ?? cycle[0], "BOM 图存在循环，已阻止递归继续展开。", string.Join(" -> ", cycle)));
        }

        foreach (var item in network.Items.Where(item => item.ItemType == "PurchasedPart" && !network.SupplierSources.Any(source => source.ItemCode == item.ItemCode)))
        {
            issues.Add(Issue("Yellow", "PurchasedPartWithoutSupplier", item.ItemCode, item.ItemName, "采购件没有供应来源。", item.ItemType));
        }

        foreach (var item in network.Items.Where(item => item.ItemType is "FinishedGood" or "Subassembly" && !network.RoutingLines.Any(route => route.ItemCode == item.ItemCode)))
        {
            issues.Add(Issue("Yellow", "ItemWithoutRouting", item.ItemCode, item.ItemName, "成品或半成品没有 routing。", item.ItemType));
        }

        foreach (var buffer in network.BufferSettings.Where(setting => setting.IsDecouplingPoint && itemByCode.ContainsKey(setting.ItemCode)))
        {
            var executable = network.InventoryLocations.Any(location =>
                location.ItemCode == buffer.ItemCode
                && location.QualityStatus == "Qualified"
                && location.LocationType is "WipSupermarket" or "QualifiedStock" or "LineSide");
            if (!executable)
            {
                var item = itemByCode[buffer.ItemCode];
                issues.Add(Issue("Yellow", "BufferWithoutExecutableLocation", item.ItemCode, item.ItemName, "解耦点物料没有可执行库存位置。", buffer.InventoryBufferProfile));
            }
        }

        foreach (var alternate in network.AlternateItems)
        {
            if (!itemByCode.ContainsKey(alternate.PrimaryItemCode) || !itemByCode.ContainsKey(alternate.AlternateItemCode))
            {
                issues.Add(Issue("Yellow", "MissingAlternateItem", alternate.PrimaryItemCode, alternate.AlternateItemCode, "替代料主料或备料不存在。", alternate.AlternateGroup));
            }
        }

        foreach (var item in network.Items.Where(item => !usedItems.Contains(item.ItemCode) && !network.AlternateItems.Any(alt => alt.AlternateItemCode == item.ItemCode || alt.PrimaryItemCode == item.ItemCode)))
        {
            issues.Add(Issue("Info", "IsolatedItem", item.ItemCode, item.ItemName, "物料未被当前生效 BOM 使用。", item.ItemType));
        }

        foreach (var header in network.BomHeaders.Where(header => !activeHeaderIds.Contains(header.BomId)))
        {
            var reason = header.ReleaseStatus != "Released"
                ? $"发布状态 {header.ReleaseStatus}"
                : header.EffectiveFrom > anchorDate
                    ? $"未来生效 {header.EffectiveFrom:yyyy-MM-dd}"
                    : $"已过期 {header.EffectiveTo:yyyy-MM-dd}";
            issues.Add(Issue("Info", "InactiveBomIgnored", header.ParentItemCode, itemByCode.GetValueOrDefault(header.ParentItemCode)?.ItemName ?? header.ParentItemCode, "过期或未来生效 BOM 被本次图构建忽略。", $"{header.BomId} / {reason}"));
        }

        return new NetworkValidationReport(
            issues.Count(item => item.Severity == "Red"),
            issues.Count(item => item.Severity == "Yellow"),
            issues.Count(item => item.Severity == "Info"),
            issues.OrderBy(item => SeverityOrder(item.Severity)).ThenBy(item => item.RuleCode, StringComparer.Ordinal).ToList());
    }

    private static IReadOnlyList<IReadOnlyList<string>> DetectCycles(IReadOnlyList<NetworkBomLine> activeLines)
    {
        var adjacency = activeLines
            .GroupBy(line => line.ParentItemCode, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(line => line.ComponentItemCode).Distinct(StringComparer.Ordinal).ToList(), StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var stack = new List<string>();
        var cycles = new List<IReadOnlyList<string>>();
        var cycleKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var node in adjacency.Keys.OrderBy(item => item, StringComparer.Ordinal))
        {
            Visit(node);
        }

        return cycles;

        void Visit(string node)
        {
            if (stack.Contains(node, StringComparer.Ordinal))
            {
                var index = stack.FindIndex(item => string.Equals(item, node, StringComparison.Ordinal));
                var cycle = stack.Skip(index).Concat(new[] { node }).ToList();
                var key = string.Join("|", cycle);
                if (cycleKeys.Add(key))
                {
                    cycles.Add(cycle);
                }
                return;
            }

            if (!visited.Add(node))
            {
                return;
            }

            stack.Add(node);
            if (adjacency.TryGetValue(node, out var children))
            {
                foreach (var child in children)
                {
                    Visit(child);
                }
            }
            stack.RemoveAt(stack.Count - 1);
        }
    }

    private static NetworkValidationIssue Issue(
        string severity,
        string ruleCode,
        string itemCode,
        string itemName,
        string message,
        string evidence)
    {
        return new NetworkValidationIssue(severity, ruleCode, itemCode, itemName, message, evidence);
    }

    private static int SeverityOrder(string severity)
    {
        return severity switch
        {
            "Red" => 0,
            "Yellow" => 1,
            _ => 2,
        };
    }
}
