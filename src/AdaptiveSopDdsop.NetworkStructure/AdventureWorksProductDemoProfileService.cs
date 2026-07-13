using System.Text.Json.Nodes;

namespace AdaptiveSopDdsop.NetworkStructure;

public sealed record NetworkProductDemoProfileOptions(string ContractPath)
{
    public static NetworkProductDemoProfileOptions Default { get; } = new(
        Path.Combine(
            ContractRepositoryPathResolver.ResolveDefault(),
            "contracts",
            "adventureworks-product-demo-v1"));
}

public sealed record NetworkProductDemoWorkspace(
    NetworkProductDemoProfileSummary Profile,
    IReadOnlyList<NetworkProductDemoAuthorityRow> NetworkAuthorityRows,
    IReadOnlyList<NetworkProductDemoPanelPolicy> PanelPolicies,
    IReadOnlyList<NetworkProductDemoValidationItem> Validation,
    IReadOnlyList<string> CandidateGuards,
    IReadOnlyList<string> NonClaims,
    bool FallbackToStandaloneSampleBlocked,
    bool RecommendationOnly,
    bool ExternalWhiteBoxRecalculationRequired);

public sealed record NetworkProductDemoProfileSummary(
    string ContractID,
    string ProfileID,
    string Mode,
    string ProductStatus,
    string ScenarioLabel,
    string MappingConfidence,
    string BasePackageID,
    string DemoAuthorityPackageID,
    string DemoAuthorityVersion,
    string NetworkAdapterVersion,
    string PanelPolicyDefault);

public sealed record NetworkProductDemoAuthorityRow(
    string GroupName,
    string BusinessObject,
    string ValueSummary,
    string SourceClass,
    string EvidenceRef,
    string Owner,
    string EffectiveFrom,
    string Compatibility);

public sealed record NetworkProductDemoPanelPolicy(
    string ViewID,
    string Handling,
    string DisplayLabel,
    string PlaceholderText);

public sealed record NetworkProductDemoValidationItem(
    string Rule,
    string Status,
    string Message,
    string EvidenceRef);

public sealed class AdventureWorksProductDemoProfileService
{
    private static readonly IReadOnlyDictionary<string, string> ViewLabels = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["capability-boundary"] = "产品能力边界",
        ["material-network-graph"] = "物料网络图",
        ["network-metrics"] = "网络指标计算",
        ["candidate-list"] = "候选清单",
        ["candidate-detail-evidence-chain"] = "候选证据链",
        ["validation-report"] = "校验报告",
        ["scenario-validation"] = "场景验证"
    };

    private readonly NetworkProductDemoProfileOptions _options;

    public AdventureWorksProductDemoProfileService()
        : this(NetworkProductDemoProfileOptions.Default)
    {
    }

    public AdventureWorksProductDemoProfileService(NetworkProductDemoProfileOptions options)
    {
        _options = options;
    }

    public NetworkProductDemoWorkspace GetWorkspace()
    {
        var manifest = ReadExample("adventureworks-product-demo-v1-profile-manifest.example.json");
        var authority = ReadExample("demo-authority-extension.example.json");
        var profile = BuildProfile(manifest);
        var rows = BuildNetworkAuthorityRows(authority);
        var policies = BuildPanelPolicies(manifest);

        return new NetworkProductDemoWorkspace(
            profile,
            rows,
            policies,
            BuildValidation(profile, rows, policies),
            new[]
            {
                "候选必须带来源类型和证据编号。",
                "缺少供应来源、提前期、需求代理、缓冲档案或风险代理时阻断候选生成。",
                "候选只作为 recommendation-only，不能自动采纳。",
                "候选进入外部治理平台后必须由白盒场景回算重新验证。"
            },
            new[]
            {
                "不声明 ProductionValidated。",
                "不声明 Business Golden Loop Readiness。",
                "不创建 SDBR 可执行 routing、operation、work order 或 supplier execution instruction。",
                "候选不会自动修改外部治理平台主设置。"
            },
            FallbackToStandaloneSampleBlocked: true,
            RecommendationOnly: true,
            ExternalWhiteBoxRecalculationRequired: true);
    }

    private JsonObject ReadExample(string fileName)
    {
        var path = Path.Combine(_options.ContractPath, "examples", fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"AdventureWorks ProductDemo contract example not found: {path}", path);
        }

        return JsonNode.Parse(File.ReadAllText(path))?.AsObject()
            ?? throw new InvalidOperationException($"Invalid AdventureWorks ProductDemo JSON example: {path}");
    }

    private static NetworkProductDemoProfileSummary BuildProfile(JsonObject manifest)
    {
        var basePackage = manifest["BasePackageReference"]?.AsObject();
        var demoAuthority = manifest["DemoAuthorityExtension"]?.AsObject();
        var adapterVersions = manifest["AdapterVersionByProduct"]?.AsObject();
        var defaults = manifest["PanelPolicyDefault"]?.AsObject();

        return new NetworkProductDemoProfileSummary(
            GetString(manifest, "ContractID"),
            GetString(manifest, "ProfileID"),
            GetString(manifest, "Mode"),
            GetString(manifest, "ProductStatus"),
            GetString(manifest, "ScenarioLabel"),
            GetString(manifest, "MappingConfidence"),
            GetString(basePackage, "BasePackageID"),
            GetString(demoAuthority, "DemoAuthorityPackageID"),
            GetString(demoAuthority, "DemoAuthorityVersion"),
            GetString(adapterVersions, "NetworkStructureScoring"),
            GetString(defaults, "NetworkStructureScoring"));
    }

    private static IReadOnlyList<NetworkProductDemoAuthorityRow> BuildNetworkAuthorityRows(JsonObject authority)
    {
        var network = authority["NetworkScoringAuthority"]?.AsObject();
        if (network is null)
        {
            return Array.Empty<NetworkProductDemoAuthorityRow>();
        }

        var rows = new List<NetworkProductDemoAuthorityRow>();
        foreach (var group in network)
        {
            if (group.Value is not JsonArray array)
            {
                continue;
            }

            foreach (var row in array.OfType<JsonObject>())
            {
                rows.Add(new NetworkProductDemoAuthorityRow(
                    group.Key,
                    BusinessObject(group.Key, row),
                    ValueSummary(group.Key, row),
                    GetString(row, "SourceClass"),
                    GetString(row, "EvidenceRef"),
                    GetString(row, "Owner"),
                    GetString(row, "EffectiveFrom"),
                    GetString(row, "Compatibility")));
            }
        }

        return rows;
    }

    private static IReadOnlyList<NetworkProductDemoPanelPolicy> BuildPanelPolicies(JsonObject manifest)
    {
        return (manifest["PanelPolicy"]?.AsArray() ?? new JsonArray())
            .OfType<JsonObject>()
            .Where(item => string.Equals(GetString(item, "Product"), "NetworkStructureScoring", StringComparison.Ordinal))
            .Select(item =>
            {
                var viewId = GetString(item, "PanelID");
                return new NetworkProductDemoPanelPolicy(
                    viewId,
                    GetString(item, "ProductModeHandling"),
                    ViewLabels.TryGetValue(viewId, out var label) ? label : viewId,
                    GetString(item, "PlaceholderText"));
            })
            .ToArray();
    }

    private static IReadOnlyList<NetworkProductDemoValidationItem> BuildValidation(
        NetworkProductDemoProfileSummary profile,
        IReadOnlyList<NetworkProductDemoAuthorityRow> rows,
        IReadOnlyList<NetworkProductDemoPanelPolicy> policies)
    {
        var requiredGroups = new[]
        {
            "SupplierSourceAssignments",
            "SupplierCapacityWindows",
            "LeadTimeProfiles",
            "VariabilityProfiles",
            "CapacityResourceLoadProxies",
            "ServiceTargets",
            "DemandProxies",
            "BufferProfiles",
            "RiskProxies"
        };
        var presentGroups = rows.Select(item => item.GroupName).ToHashSet(StringComparer.Ordinal);
        var validation = new List<NetworkProductDemoValidationItem>
        {
            new("ProfileID", profile.ProfileID == "ADVENTUREWORKS_PRODUCT_DEMO_V1" ? "通过" : "阻断", profile.ProfileID, profile.ContractID),
            new("Mode", profile.Mode == "ProductDemoMode" ? "通过" : "阻断", profile.Mode, profile.ProfileID),
            new("MappingConfidence", profile.MappingConfidence == "ProductDemoOnly" ? "通过" : "阻断", profile.MappingConfidence, profile.ProfileID),
            new("Network fallback", "通过", "未适配视图必须占位，不允许回退 NETWORK_STRUCTURE_STANDALONE_SAMPLE。", "PanelPolicyDefault.NetworkStructureScoring")
        };

        validation.AddRange(requiredGroups.Select(group => new NetworkProductDemoValidationItem(
            $"NetworkScoringAuthority.{group}",
            presentGroups.Contains(group) ? "通过" : "阻断",
            presentGroups.Contains(group) ? "网络评分证据行已提供，且带来源类型和证据编号。" : "缺少候选生成所需证据。",
            group)));

        validation.Add(new NetworkProductDemoValidationItem(
            "Scenario validation placeholder",
            policies.Any(item => item.ViewID == "scenario-validation" && item.Handling == "Placeholder") ? "通过" : "阻断",
            "场景验证保留占位，候选需进入外部治理平台做白盒回算，且保持 recommendation-only。",
            "PanelPolicy"));

        return validation;
    }

    private static string BusinessObject(string groupName, JsonObject row)
    {
        return groupName switch
        {
            "SupplierSourceAssignments" => $"{GetString(row, "ItemID")} / {GetString(row, "SupplierID")}",
            "SupplierCapacityWindows" => $"{GetString(row, "SupplierID")} / {GetString(row, "ItemOrFamilyID")}",
            "LeadTimeProfiles" => GetString(row, "ItemID"),
            "VariabilityProfiles" => GetString(row, "ItemID"),
            "CapacityResourceLoadProxies" => $"{GetString(row, "ProductFamilyID")} / {GetString(row, "ResourceID")}",
            "ServiceTargets" => GetString(row, "ProductFamilyID"),
            "DemandProxies" => GetString(row, "ItemID"),
            "BufferProfiles" => GetString(row, "ItemID"),
            "RiskProxies" => GetString(row, "ItemID"),
            _ => GetString(row, "EvidenceRef")
        };
    }

    private static string ValueSummary(string groupName, JsonObject row)
    {
        return groupName switch
        {
            "SupplierSourceAssignments" => $"分配 {GetString(row, "Allocation")}，资格 {GetString(row, "QualificationStatus")}",
            "SupplierCapacityWindows" => $"承诺能力 {GetString(row, "CommittedCapacity")}，窗口 {GetString(row, "WindowStart")} -> {GetString(row, "WindowEnd")}",
            "LeadTimeProfiles" => $"提前期 {GetString(row, "StandardLeadTimeDays")} 天，时间缓冲依据 {GetString(row, "TimeBufferBasis")}",
            "VariabilityProfiles" => $"需求波动 {GetString(row, "DemandVariability")}，供应波动 {GetString(row, "SupplyVariability")}",
            "CapacityResourceLoadProxies" => $"资源 {GetString(row, "ResourceID")}，角色 {GetString(row, "ResourceRole")}",
            "ServiceTargets" => ServiceTargetSummary(row),
            "DemandProxies" => $"ADU {GetString(row, "ADU")}，窗口 {GetString(row, "DemandHorizonDays")} 天",
            "BufferProfiles" => $"缓冲档案 {GetString(row, "InventoryBufferProfile")}，当前状态 {GetString(row, "CurrentDecouplingStatus")}",
            "RiskProxies" => $"库存风险 {GetString(row, "InventoryAvailabilityRisk")}，质量风险 {GetString(row, "QualityRisk")}",
            _ => GetString(row, "EvidenceRef")
        };
    }

    private static string ServiceTargetSummary(JsonObject row)
    {
        var target = GetString(row, "ServiceTarget");
        if (string.IsNullOrWhiteSpace(target))
        {
            target = GetString(row, "ServiceLevelTarget");
        }

        return string.IsNullOrWhiteSpace(target)
            ? "服务目标值待补全，仅保留评分证据占位。"
            : $"服务目标 {target}，流速目标 {GetString(row, "FlowTargetDays")} 天";
    }

    private static string GetString(JsonObject? node, string property)
    {
        if (node is null || !node.TryGetPropertyValue(property, out var value) || value is null)
        {
            return string.Empty;
        }

        return value switch
        {
            JsonValue jsonValue => jsonValue.ToJsonString().Trim('"'),
            _ => value.ToJsonString()
        };
    }
}

