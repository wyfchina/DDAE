using AdaptiveSopDdsop.NetworkStructure;

using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.NetworkStructureIntegration;

public sealed class NetworkCandidateRecalculationRequestBuilder
{
    public ScenarioRunPreviewRequest Build(
        ScenarioWorkspaceDataSet data,
        NetworkDataSet networkData,
        NetworkStructureCandidate candidate,
        int horizon)
    {
        var sku = ResolveSku(data, networkData, candidate);
        var prebuild = new List<PrebuildCampaign>();
        var capacity = new List<ResourceCapacityAdjustment>();
        var policies = new List<SkuPolicyOverride>();
        var supplierLimits = new List<SupplierCapacityLimit>();

        if (sku is not null && candidate.RecommendedSettingType is "库存缓冲" or "解耦点")
        {
            var shorterCycle = Math.Max(1, sku.OrderCycleDays - 2);
            policies.Add(new SkuPolicyOverride(
                sku.Sku,
                MinimumOrderQuantity: Math.Max(sku.MinimumOrderQuantity, decimal.Round(sku.Adu * 2m, 0)),
                OrderCycleDays: shorterCycle));
            prebuild.Add(new PrebuildCampaign(
                $"VAL-{candidate.CandidateId}",
                sku.Sku,
                1,
                4,
                Math.Min(8, horizon),
                decimal.Round(Math.Max(sku.MinimumOrderQuantity, sku.Adu * 2m), 0)));
        }

        if (sku is not null && candidate.RecommendedSettingType == "时间缓冲")
        {
            policies.Add(new SkuPolicyOverride(
                sku.Sku,
                OrderCycleDays: Math.Max(1, sku.OrderCycleDays - 1)));
            AddSupplierCapacityLift(data, sku, supplierLimits, horizon);
        }

        if (candidate.RecommendedSettingType == "能力缓冲")
        {
            var resource = ResolveResource(data, networkData, candidate, sku);
            if (!string.IsNullOrWhiteSpace(resource))
            {
                foreach (var week in Enumerable.Range(4, Math.Min(3, Math.Max(1, horizon - 3))))
                {
                    capacity.Add(new ResourceCapacityAdjustment(resource, week, 1.15m, $"网络候选验证：{candidate.TargetName}"));
                }
            }
        }

        if (candidate.RecommendedSettingType is "供应主设置" or "时间缓冲" && sku is not null)
        {
            AddSupplierCapacityLift(data, sku, supplierLimits, horizon);
        }

        return new ScenarioRunPreviewRequest(
            horizon,
            TemplateId: null,
            SkuFilter: sku is null ? null : new[] { sku.Sku },
            FamilyFilter: sku is null ? null : new[] { sku.Family },
            Parameters: new ScenarioRunParameterSet(
                prebuild,
                capacity,
                policies,
                supplierLimits),
            AdoptionConstraintMode: "Balanced");
    }

    private static SkuBufferSetting? ResolveSku(
        ScenarioWorkspaceDataSet data,
        NetworkDataSet networkData,
        NetworkStructureCandidate candidate)
    {
        var direct = data.Skus.FirstOrDefault(item => item.Sku == candidate.Target);
        if (direct is not null)
        {
            return direct;
        }

        if (candidate.TargetType == "供应商 / 物料族")
        {
            var parts = candidate.Target.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var materialFamily = parts.Length >= 2 ? parts[1] : candidate.Target;
            var source = data.SupplierItemSources.FirstOrDefault(item => item.MaterialFamily == materialFamily);
            return source is null ? null : data.Skus.FirstOrDefault(item => item.Sku == source.Sku);
        }

        var activeHeaderIds = networkData.BomHeaders
            .Where(header => header.ReleaseStatus == "Released"
                && header.EffectiveFrom <= data.Request.AnchorDate
                && (header.EffectiveTo is null || header.EffectiveTo >= data.Request.AnchorDate))
            .Select(header => header.BomId)
            .ToHashSet(StringComparer.Ordinal);
        var parentsByChild = networkData.BomLines
            .Where(line => activeHeaderIds.Contains(line.BomId))
            .GroupBy(line => line.ComponentItemCode, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(line => line.ParentItemCode).Distinct(StringComparer.Ordinal).ToList(), StringComparer.Ordinal);
        var downstream = TraverseParents(candidate.Target, parentsByChild, 6);
        return data.Skus
            .Where(sku => downstream.Contains(sku.Sku) || sku.Family == candidate.Family)
            .OrderByDescending(sku => downstream.Contains(sku.Sku))
            .ThenByDescending(sku => sku.Adu)
            .FirstOrDefault();
    }

    private static string? ResolveResource(
        ScenarioWorkspaceDataSet data,
        NetworkDataSet networkData,
        NetworkStructureCandidate candidate,
        SkuBufferSetting? sku)
    {
        if (candidate.TargetType == "资源")
        {
            return candidate.Target;
        }

        var networkResource = networkData.RoutingLines
            .Where(route => route.ItemCode == candidate.Target)
            .Select(route => route.ResourceCode)
            .FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(networkResource))
        {
            return networkResource;
        }

        return sku is null
            ? data.Resources.FirstOrDefault()?.Code
            : data.ResourceRoutings.FirstOrDefault(route => route.Sku == sku.Sku)?.ResourceCode;
    }

    private static void AddSupplierCapacityLift(
        ScenarioWorkspaceDataSet data,
        SkuBufferSetting sku,
        List<SupplierCapacityLimit> supplierLimits,
        int horizon)
    {
        var source = data.SupplierItemSources.FirstOrDefault(item => item.Sku == sku.Sku);
        if (source is null)
        {
            return;
        }

        var windows = data.SupplierCapacityWindows
            .Where(item => item.Supplier == source.Supplier && item.MaterialFamily == source.MaterialFamily)
            .ToList();
        if (windows.Count == 0)
        {
            return;
        }

        var lifted = decimal.Round(windows.Max(item => item.CommittedCapacity) * 1.20m, 0);
        supplierLimits.Add(new SupplierCapacityLimit(
            source.Supplier,
            source.MaterialFamily,
            1,
            horizon,
            lifted));
    }

    private static IReadOnlySet<string> TraverseParents(
        string start,
        IReadOnlyDictionary<string, List<string>> parentsByChild,
        int maxDepth)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        Walk(start, 0, new HashSet<string>(StringComparer.Ordinal) { start });
        return result;

        void Walk(string current, int depth, HashSet<string> path)
        {
            if (depth >= maxDepth || !parentsByChild.TryGetValue(current, out var parents))
            {
                return;
            }

            foreach (var parent in parents)
            {
                if (!result.Add(parent) || path.Contains(parent))
                {
                    continue;
                }

                var nextPath = new HashSet<string>(path, StringComparer.Ordinal) { parent };
                Walk(parent, depth + 1, nextPath);
            }
        }
    }
}
