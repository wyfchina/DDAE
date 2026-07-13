using System.Text.Json.Nodes;

namespace AdaptiveSopDdsop.Web.Domain;

public sealed record AdventureWorksProductDemoProfileOptions(string ContractPath)
{
    public static AdventureWorksProductDemoProfileOptions Default { get; } = new(
        Path.Combine(
            ContractRepositoryPathResolver.ResolveDefault(),
            "contracts",
            "adventureworks-product-demo-v1"));
}

public sealed record AdventureWorksProductDemoWorkspace(
    AdventureWorksProductDemoProfileSummary Profile,
    IReadOnlyList<AdventureWorksProductDemoAuthorityRow> DdaeAuthorityRows,
    IReadOnlyList<AdventureWorksProductDemoPanelPolicy> PanelPolicies,
    IReadOnlyList<AdventureWorksProductDemoValidationItem> Validation,
    IReadOnlyList<string> PlaceholderPanels,
    IReadOnlyList<string> NonClaims,
    bool FallbackToCoreSampleBlocked,
    bool FeedbackMutationBlocked,
    bool NetworkCandidateMutationBlocked);

public sealed record AdventureWorksProductDemoProfileSummary(
    string ContractID,
    string ProfileID,
    string Mode,
    string ProductStatus,
    string ScenarioLabel,
    string MappingConfidence,
    string BasePackageID,
    string BasePackageChecksum,
    string DemoAuthorityPackageID,
    string DemoAuthorityVersion,
    string DdaeAdapterVersion,
    string PanelPolicyDefault);

public sealed record AdventureWorksProductDemoAuthorityRow(
    string GroupName,
    string BusinessObject,
    string ValueSummary,
    string SourceClass,
    string EvidenceRef,
    string Owner,
    string EffectiveFrom,
    string Compatibility);

public sealed record AdventureWorksProductDemoPanelPolicy(
    string PanelID,
    string Handling,
    string DisplayLabel,
    string PlaceholderText);

public sealed record AdventureWorksProductDemoValidationItem(
    string Rule,
    string Status,
    string Message,
    string EvidenceRef);

public sealed class AdventureWorksProductDemoProfileService
{
    private static readonly IReadOnlyDictionary<string, string> PanelLabels = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["overview-panel"] = "总览",
        ["product-family-dashboard-panel"] = "产品族看板",
        ["data-readiness-panel"] = "数据准备",
        ["variance-panel"] = "异常识别",
        ["scenario-run-panel"] = "场景运行",
        ["scenario-comparison"] = "方案比较",
        ["buffer-trend-panel"] = "缓冲 / 库存趋势",
        ["rccp-panel"] = "RCCP 与约束",
        ["projected-supply-panel"] = "供应商需求",
        ["saved-scenarios-panel"] = "场景留痕",
        ["master-settings-panel"] = "主设置治理",
        ["trace-panel"] = "白盒追踪",
        ["public-demo-golden-loop-panel"] = "公开演示闭环"
    };

    private readonly AdventureWorksProductDemoProfileOptions _options;

    public AdventureWorksProductDemoProfileService()
        : this(AdventureWorksProductDemoProfileOptions.Default)
    {
    }

    public AdventureWorksProductDemoProfileService(AdventureWorksProductDemoProfileOptions options)
    {
        _options = options;
    }

    public AdventureWorksProductDemoWorkspace GetWorkspace()
    {
        var manifest = ReadExample("adventureworks-product-demo-v1-profile-manifest.example.json");
        var authority = ReadExample("demo-authority-extension.example.json");
        var profile = BuildProfile(manifest);
        var rows = BuildAuthorityRows(authority);
        var policies = BuildPanelPolicies(manifest);
        var placeholders = policies
            .Where(item => string.Equals(item.Handling, "Placeholder", StringComparison.Ordinal))
            .Select(item => $"{item.DisplayLabel}（{item.PanelID}）")
            .ToArray();

        return new AdventureWorksProductDemoWorkspace(
            profile,
            rows,
            policies,
            BuildValidation(profile, rows, policies),
            placeholders,
            new[]
            {
                "不声明 ProductionValidated。",
                "不声明 Business Golden Loop Readiness。",
                "不拥有生产 routing、资源日历、工单执行或物料可行性权威。",
                "SDBR feedback 与 Network candidates 只能作为评审上下文，不能自动修改已批准主设置。"
            },
            FallbackToCoreSampleBlocked: true,
            FeedbackMutationBlocked: true,
            NetworkCandidateMutationBlocked: true);
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

    private static AdventureWorksProductDemoProfileSummary BuildProfile(JsonObject manifest)
    {
        var basePackage = manifest["BasePackageReference"]?.AsObject();
        var demoAuthority = manifest["DemoAuthorityExtension"]?.AsObject();
        var adapterVersions = manifest["AdapterVersionByProduct"]?.AsObject();
        var defaults = manifest["PanelPolicyDefault"]?.AsObject();

        return new AdventureWorksProductDemoProfileSummary(
            GetString(manifest, "ContractID"),
            GetString(manifest, "ProfileID"),
            GetString(manifest, "Mode"),
            GetString(manifest, "ProductStatus"),
            GetString(manifest, "ScenarioLabel"),
            GetString(manifest, "MappingConfidence"),
            GetString(basePackage, "BasePackageID"),
            GetString(basePackage, "BasePackageChecksum"),
            GetString(demoAuthority, "DemoAuthorityPackageID"),
            GetString(demoAuthority, "DemoAuthorityVersion"),
            GetString(adapterVersions, "DDAE"),
            GetString(defaults, "DDAE"));
    }

    private static IReadOnlyList<AdventureWorksProductDemoAuthorityRow> BuildAuthorityRows(JsonObject authority)
    {
        var ddae = authority["DDAEGovernanceAuthority"]?.AsObject();
        if (ddae is null)
        {
            return Array.Empty<AdventureWorksProductDemoAuthorityRow>();
        }

        var rows = new List<AdventureWorksProductDemoAuthorityRow>();
        foreach (var group in ddae)
        {
            if (group.Value is not JsonArray array)
            {
                continue;
            }

            foreach (var row in array.OfType<JsonObject>())
            {
                rows.Add(new AdventureWorksProductDemoAuthorityRow(
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

    private static IReadOnlyList<AdventureWorksProductDemoPanelPolicy> BuildPanelPolicies(JsonObject manifest)
    {
        var policy = manifest["PanelPolicy"]?.AsArray() ?? new JsonArray();
        return policy
            .OfType<JsonObject>()
            .Where(item => string.Equals(GetString(item, "Product"), "DDAE", StringComparison.Ordinal))
            .Select(item =>
            {
                var panelId = GetString(item, "PanelID");
                var handling = GetString(item, "ProductModeHandling");
                return new AdventureWorksProductDemoPanelPolicy(
                    panelId,
                    handling,
                    PanelLabels.TryGetValue(panelId, out var label) ? label : panelId,
                    GetString(item, "PlaceholderText"));
            })
            .ToArray();
    }

    private static IReadOnlyList<AdventureWorksProductDemoValidationItem> BuildValidation(
        AdventureWorksProductDemoProfileSummary profile,
        IReadOnlyList<AdventureWorksProductDemoAuthorityRow> rows,
        IReadOnlyList<AdventureWorksProductDemoPanelPolicy> policies)
    {
        var requiredGroups = new[]
        {
            "ServiceTargets",
            "DemandProxies",
            "DDMRPBufferSettings",
            "PlanningWindows",
            "ControlPointGovernance",
            "ResourceRoleGovernance",
            "ReleasePolicies",
            "PriorityPolicies",
            "ApprovalEvidence",
            "EffectivityEvidence",
            "RuleVersions"
        };
        var presentGroups = rows.Select(item => item.GroupName).ToHashSet(StringComparer.Ordinal);
        var validation = new List<AdventureWorksProductDemoValidationItem>
        {
            new("ProfileID", profile.ProfileID == "ADVENTUREWORKS_PRODUCT_DEMO_V1" ? "通过" : "阻断", profile.ProfileID, profile.ContractID),
            new("Mode", profile.Mode == "ProductDemoMode" ? "通过" : "阻断", profile.Mode, profile.ProfileID),
            new("MappingConfidence", profile.MappingConfidence == "ProductDemoOnly" ? "通过" : "阻断", profile.MappingConfidence, profile.ProfileID),
            new("DDAE fallback", "通过", "未适配面板必须占位，不允许回退 DDAE_CORE_SAMPLE。", "PanelPolicyDefault.DDAE")
        };

        validation.AddRange(requiredGroups.Select(group => new AdventureWorksProductDemoValidationItem(
            $"DDAEGovernanceAuthority.{group}",
            presentGroups.Contains(group) ? "通过" : "阻断",
            presentGroups.Contains(group) ? "治理行已提供，且带 SourceClass / EvidenceRef。" : "缺少必需治理行。",
            group)));

        validation.Add(new AdventureWorksProductDemoValidationItem(
            "PanelPolicy placeholders",
            policies.Any(item => item.Handling == "Placeholder") ? "通过" : "阻断",
            "场景运行、方案比较、缓冲趋势、RCCP 与供应商需求保留占位。",
            "PanelPolicy"));

        return validation;
    }

    private static string BusinessObject(string groupName, JsonObject row)
    {
        return groupName switch
        {
            "ServiceTargets" => GetString(row, "ProductFamilyID"),
            "DemandProxies" => GetString(row, "ItemID"),
            "DDMRPBufferSettings" => GetString(row, "ItemID"),
            "PlanningWindows" => $"{GetString(row, "HorizonStart")} -> {GetString(row, "HorizonEnd")}",
            "ControlPointGovernance" => GetString(row, "ControlPointID"),
            "ResourceRoleGovernance" => GetString(row, "ResourceID"),
            "ReleasePolicies" => GetString(row, "ReleasePolicyID"),
            "PriorityPolicies" => GetString(row, "PriorityPolicyID"),
            "ApprovalEvidence" => GetString(row, "ApprovalID"),
            "EffectivityEvidence" => GetString(row, "PolicyID"),
            "RuleVersions" => GetString(row, "DDMRPFormulaVersionID"),
            _ => GetString(row, "EvidenceRef")
        };
    }

    private static string ValueSummary(string groupName, JsonObject row)
    {
        return groupName switch
        {
            "ServiceTargets" => $"服务目标 {GetString(row, "ServiceLevelTarget")}，流速目标 {GetString(row, "FlowTargetDays")} 天",
            "DemandProxies" => $"ADU {GetString(row, "ADU")}，需求窗口 {GetString(row, "DemandHorizonDays")} 天",
            "DDMRPBufferSettings" => $"DLT {GetString(row, "DLTDays")} 天，DAF {GetString(row, "DAF")}，MOQ {GetString(row, "MOQ")}，订货周期 {GetString(row, "OrderCycleDays")} 天",
            "PlanningWindows" => $"冻结 {GetString(row, "FreezeWindowDays")} 天，弹性 {GetString(row, "FlexWindowDays")} 天",
            "ControlPointGovernance" => $"控制点类型 {GetString(row, "ControlPointType")}，资源 {GetString(row, "ResourceID")}",
            "ResourceRoleGovernance" => $"资源角色 {GetString(row, "ResourceRoleClass")}，约束策略 {GetString(row, "ConstraintPolicy")}",
            "ReleasePolicies" => $"释放规则 {GetString(row, "ReleaseRule")}，节奏 {GetString(row, "ReviewRhythm")}",
            "PriorityPolicies" => $"优先级规则 {GetString(row, "PriorityRule")}",
            "ApprovalEvidence" => $"{GetString(row, "ApprovalStatus")} / {GetString(row, "ApprovedBy")}",
            "EffectivityEvidence" => $"生效 {GetString(row, "EffectivePolicyFrom")}",
            "RuleVersions" => $"{GetString(row, "DDMRPFormulaVersionID")} / {GetString(row, "SchedulingGovernanceRuleVersionID")}",
            _ => GetString(row, "EvidenceRef")
        };
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
