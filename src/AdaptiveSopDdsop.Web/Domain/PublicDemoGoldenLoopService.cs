using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AdaptiveSopDdsop.Web.Domain;

public sealed record PublicDemoGoldenLoopOptions(
    string PackagePath,
    string ExpectedPackageChecksum,
    string DdaeToSdbrPayloadPath,
    string PlanningRunFeedbackPath,
    string VarianceAnalysisFeedbackPath,
    string ValidationSummaryPath)
{
    public static PublicDemoGoldenLoopOptions Default { get; } = new(
        @"D:\Documents\DDAE_INTERFACE_CONTRACT\data\public-demo-golden-data-v1",
        "20ddd29cb082ba833ff617013257a7270c49b0a6eb1da8b97f5a7240ac900772",
        @"D:\Documents\DDAE_INTERFACE_CONTRACT\data\public-demo-golden-data-v1\handoff\ddae-to-sdbr\ddsop-config-inbound-v1-payload.json",
        @"D:\Documents\DDAE_INTERFACE_CONTRACT\data\public-demo-golden-data-v1\handoff\sdbr-to-ddae\planning-run-feedback.json",
        @"D:\Documents\DDAE_INTERFACE_CONTRACT\data\public-demo-golden-data-v1\handoff\sdbr-to-ddae\variance-analysis-feedback.json",
        @"D:\Documents\DDAE_INTERFACE_CONTRACT\data\public-demo-golden-data-v1\handoff\sdbr-to-ddae\validation-summary.json");
}

public sealed record PublicDemoGoldenLoopWorkspace(
    string ScenarioLabel,
    IReadOnlyList<string> EvidenceLabels,
    string MappingConfidence,
    string PackagePath,
    string ExpectedPackageChecksum,
    string? ManifestPackageChecksum,
    bool PackageChecksumMatches,
    bool PackageAvailable,
    IReadOnlyList<PublicDemoPackageFileSummary> PackageFiles,
    IReadOnlyList<PublicDemoReviewedMapping> ReviewedMappings,
    PublicDemoPackageContext PackageContext,
    PublicDemoSchedulingAdapterReadModel SchedulingAdapter,
    DdsopConfigInboundMessage PayloadPreview,
    PublicDemoHandoffState Handoff,
    IReadOnlyList<PublicDemoFeedbackFileState> Feedback,
    string NonClaimsSummary);

public sealed record PublicDemoPackageFileSummary(
    string FileName,
    string Role,
    int? RowCount,
    string? Checksum);

public sealed record PublicDemoReviewedMapping(
    string DemoObject,
    string Boundary,
    string AllowedUse,
    string ForbiddenUse);

public sealed record PublicDemoPackageContext(
    int ItemCount,
    int LocationCount,
    int ItemLocationCount,
    int UomCount,
    int BomLineCount,
    int WorkOrderCount,
    int RoutingOperationCount,
    int CapacityCount,
    int CrosswalkCount,
    string? SampleItem,
    string? SampleLocation,
    decimal SampleQuantity,
    string SampleUom);

public sealed record PublicDemoSchedulingAdapterReadModel(
    string AdapterProfileID,
    string ScenarioLabel,
    string MappingConfidence,
    string FeedbackBoundary,
    IReadOnlyList<PublicDemoSchedulingGovernancePolicy> GovernancePolicies,
    IReadOnlyList<PublicDemoAdapterMetadataItem> AdapterMetadata,
    IReadOnlyList<PublicDemoNonDdaeOwnedExecutionItem> NonDdaeOwnedExecutionMetadata);

public sealed record PublicDemoSchedulingGovernancePolicy(
    string PolicyArea,
    string DdaeResponsibility,
    string EvidenceFieldGroup,
    string RuleVersionID);

public sealed record PublicDemoAdapterMetadataItem(
    string FieldName,
    string Value,
    string Owner,
    string DdaeUse,
    string ForbiddenUse);

public sealed record PublicDemoNonDdaeOwnedExecutionItem(
    string ExecutionObject,
    string Owner,
    string DdaeDisplayUse,
    string ForbiddenUse);

public sealed record PublicDemoHandoffState(
    string DdaeToSdbrPayloadPath,
    bool PayloadWritten,
    string? PayloadWrittenAt,
    string? PayloadMessageID,
    string? OperatingModelConfigurationID,
    string? OperatingModelFingerprint);

public sealed record PublicDemoFeedbackFileState(
    string FeedbackName,
    string Path,
    bool Exists,
    string ProcessingStatus,
    string? MessageID,
    string? PlanningRunID,
    string? OperatingModelConfigurationID,
    string? OperatingModelFingerprint,
    string? OverallStatus,
    string? RunStatus,
    string? SolverStatus,
    string? ReliabilityStatus,
    string? SpeedStatus,
    string? StabilityStatus,
    string? ValidationStatus,
    string? MappingConfidence,
    IReadOnlyList<string>? Labels,
    int ReviewTopicCount,
    int DataCoverageIssueCount,
    string Message,
    int RawPayloadLength);

public sealed record PublicDemoPayloadWriteResult(
    string ScenarioLabel,
    string MappingConfidence,
    string PayloadPath,
    string WrittenAt,
    DdsopConfigInboundMessage Payload,
    PublicDemoHandoffState Handoff);

public sealed class PublicDemoGoldenLoopService
{
    private const string ScenarioLabel = "Controlled Contract Golden Loop Demo";
    private const string MappingConfidence = "PublicDemoOnly";
    private readonly PublicDemoGoldenLoopOptions _options;
    private readonly DdsopFeedbackOutboundInterpreter _feedbackInterpreter = new();

    public PublicDemoGoldenLoopService()
        : this(PublicDemoGoldenLoopOptions.Default)
    {
    }

    public PublicDemoGoldenLoopService(PublicDemoGoldenLoopOptions options)
    {
        _options = options;
    }

    public PublicDemoGoldenLoopWorkspace GetWorkspace()
    {
        var manifest = ReadJsonObject("manifest.json");
        var payload = BuildPayload(manifest);
        var context = BuildPackageContext(manifest);
        var packageFiles = BuildPackageFileSummaries(manifest);
        var nonClaims = ReadTextIfExists("non-claims.md");
        return new PublicDemoGoldenLoopWorkspace(
            ScenarioLabel,
            new[] { "DemoFixture", "ReviewedEvidence", ScenarioLabel },
            MappingConfidence,
            _options.PackagePath,
            _options.ExpectedPackageChecksum,
            GetString(manifest, "PackageChecksum"),
            string.Equals(GetString(manifest, "PackageChecksum"), _options.ExpectedPackageChecksum, StringComparison.Ordinal),
            Directory.Exists(_options.PackagePath) && File.Exists(Path.Combine(_options.PackagePath, "manifest.json")),
            packageFiles,
            BuildReviewedMappings(),
            context,
            BuildSchedulingAdapterReadModel(),
            payload,
            ReadHandoffState(),
            ReadFeedbackStates(),
            SummarizeNonClaims(nonClaims));
    }

    public PublicDemoPayloadWriteResult WritePayload()
    {
        var manifest = ReadJsonObject("manifest.json");
        var payload = BuildPayload(manifest);
        var json = JsonSerializer.Serialize(payload, DdsopConfigInboundContractService.ContractJsonOptions);
        var directory = Path.GetDirectoryName(_options.DdaeToSdbrPayloadPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_options.DdaeToSdbrPayloadPath, json);
        var writtenAt = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:sszzz");
        return new PublicDemoPayloadWriteResult(
            ScenarioLabel,
            MappingConfidence,
            _options.DdaeToSdbrPayloadPath,
            writtenAt,
            payload,
            ReadHandoffState(writtenAt, payload));
    }

    private DdsopConfigInboundMessage BuildPayload(JsonObject manifest)
    {
        var packageFrozenAt = GetString(manifest, "PackageFrozenAt") ?? "2026-06-29T16:48:06+08:00";
        var rowCounts = manifest["RowCountsByFile"] as JsonObject ?? new JsonObject();
        var inventoryQuantity = ReadFirstDecimal("item-locations.json", "Quantity", 100m);
        var topOfRed = Math.Max(10m, decimal.Round(inventoryQuantity * 0.35m, 2));
        var topOfYellow = Math.Max(topOfRed, decimal.Round(inventoryQuantity * 0.70m, 2));
        var topOfGreen = Math.Max(topOfYellow, decimal.Round(inventoryQuantity * 1.05m, 2));
        var availability = ReadFirstDecimal("capacities.json", "Availability", 96m);
        var efficiency = availability <= 0 ? 100m : Math.Clamp(availability, 1m, 200m);
        var approvalAt = packageFrozenAt;

        var payloadWithoutFingerprint = new DdsopOperatingModelConfiguration(
            "DDSOP-OMC-PUBLIC-DEMO-V1",
            "PUBLIC-DEMO-2026.06-A",
            "1.0.0",
            "Approved",
            "2026-06-30T00:00:00+08:00",
            null,
            "Asia/Shanghai",
            new DdsopScope(
                new[] { "PUBLIC-DEMO-PLANT" },
                new[] { "PUBLIC-DEMO-GOLDEN-DATA-V1" },
                new[] { "WH-ELEC-QA" },
                new[] { "PART-FPGA-SPACE@WH-ELEC-QA" }),
            new DdsopApproval(
                "ddsop-controlled-demo",
                approvalAt,
                "Approved"),
            new DdsopChangeReason(
                "MODEL_RESTRUCTURE",
                "DemoFixture / ReviewedEvidence / Controlled Contract Golden Loop Demo / MappingConfidence = PublicDemoOnly. Not production authority."),
            null,
            "PUBLIC-DEMO-GOLDEN-DATA-V1",
            $"PUBLIC-DEMO-ROWS-I{GetInt(rowCounts, "items.json")}-B{GetInt(rowCounts, "boms.json")}-R{GetInt(rowCounts, "routings.json")}",
            "PUBLIC-DEMO-GOLDEN-DATA-V1",
            string.Empty,
            new DdsopSchedulingConfiguration(
                "DDSOP-SCH-PUBLIC-DEMO-V1",
                "SCH-STRATEGY-CONTROLLED-DEMO",
                "DBR-RELEASE-POLICY-PUBLIC-DEMO-V1",
                1440,
                2880,
                "PromiseDate",
                "ControlPointsOnly",
                new[]
                {
                    new DdsopControlPoint(
                        "CP-WH-ELEC-QA-DEMO",
                        "WH-ELEC-QA",
                        "CapacityBuffer",
                        true,
                        "BufferPriorityFirst")
                },
                new[]
                {
                    new DdsopTimeBufferProfile(
                        "TB-PART-FPGA-SPACE-DEMO",
                        "Reviewed public demo time buffer for PART-FPGA-SPACE at WH-ELEC-QA.",
                        10080,
                        0.33m,
                        0.33m,
                        0.34m,
                        0)
                },
                new[]
                {
                    new DdsopTimeBufferAssignment(
                        "TBA-PART-FPGA-SPACE-DEMO",
                        "PART-FPGA-SPACE",
                        "MTS",
                        "CP-WH-ELEC-QA-DEMO",
                        "TB-PART-FPGA-SPACE-DEMO")
                },
                new[]
                {
                    new DdsopResourceSetting(
                        "WH-ELEC-QA",
                        "BufferedResource",
                        "Finite",
                        "CAL-PUBLIC-DEMO-8X5",
                        efficiency)
                },
                new[]
                {
                    new DdsopPartSchedulingSetting(
                        "PART-FPGA-SPACE",
                        "ROUTE-PUBLIC-DEMO-PART-FPGA-SPACE",
                        Array.Empty<string>(),
                        "DemoReviewed")
                }),
            new DdsopDdmrpConfiguration(
                "DDSOP-DDMRP-PUBLIC-DEMO-V1",
                "BUFFER-PRIORITY-PUBLIC-DEMO",
                10080,
                "ProvidedByDDSOP",
                new[]
                {
                    new DdsopDecouplingPoint(
                        "PART-FPGA-SPACE",
                        "WH-ELEC-QA",
                        "SB-PART-FPGA-SPACE-DEMO",
                        10080,
                        1m,
                        1m,
                        7)
                },
                new[]
                {
                    new DdsopStockBufferProfile(
                        "SB-PART-FPGA-SPACE-DEMO",
                        topOfRed,
                        topOfYellow,
                        topOfGreen,
                        "EA")
                },
                new[]
                {
                    new DdsopPartProfileAssignment(
                        "PART-FPGA-SPACE",
                        "WH-ELEC-QA",
                        "SB-PART-FPGA-SPACE-DEMO")
                },
                Array.Empty<DdsopAdjustmentFactor>()));

        var fingerprint = DdsopConfigInboundContractService.ComputeFingerprint(payloadWithoutFingerprint);
        var payload = payloadWithoutFingerprint with { Fingerprint = fingerprint };
        return new DdsopConfigInboundMessage(
            "DDSOP-CONFIG-INBOUND-V1",
            "1.0.0",
            "DDAE-MSG-PUBLIC-DEMO-GL-001",
            "OperatingModelConfigurationPublished",
            "DDAE",
            "SDBR",
            "DDAE:PUBLIC-DEMO-GOLDEN-DATA-V1:DDSOP-CONFIG-INBOUND-V1",
            packageFrozenAt,
            payload);
    }

    private PublicDemoPackageContext BuildPackageContext(JsonObject manifest)
    {
        var rows = manifest["RowCountsByFile"] as JsonObject ?? new JsonObject();
        var firstItem = ReadFirstObject("items.json");
        var firstLocation = ReadFirstObject("locations.json");
        return new PublicDemoPackageContext(
            GetInt(rows, "items.json"),
            GetInt(rows, "locations.json"),
            GetInt(rows, "item-locations.json"),
            GetInt(rows, "uoms.json"),
            GetInt(rows, "boms.json"),
            GetInt(rows, "work-orders.json"),
            GetInt(rows, "routings.json"),
            GetInt(rows, "capacities.json"),
            GetInt(rows, "crosswalk.json"),
            GetString(firstItem, "Name"),
            GetString(firstLocation, "Name"),
            ReadFirstDecimal("item-locations.json", "Quantity", 0m),
            ReadFirstString("item-locations.json", "QuantityUom", "EA"));
    }

    private IReadOnlyList<PublicDemoPackageFileSummary> BuildPackageFileSummaries(JsonObject manifest)
    {
        var roles = manifest["FileRoleMap"] as JsonObject ?? new JsonObject();
        var rowCounts = manifest["RowCountsByFile"] as JsonObject ?? new JsonObject();
        var checksums = manifest["ChecksumsByFile"] as JsonObject ?? new JsonObject();
        return roles
            .Select(item => new PublicDemoPackageFileSummary(
                item.Key,
                item.Value?.GetValue<string>() ?? string.Empty,
                rowCounts.TryGetPropertyValue(item.Key, out var countNode) && countNode is not null && countNode.GetValueKind() == JsonValueKind.Number
                    ? countNode.GetValue<int>()
                    : null,
                checksums.TryGetPropertyValue(item.Key, out var checksumNode) ? checksumNode?.GetValue<string>() : null))
            .OrderBy(item => item.FileName, StringComparer.Ordinal)
            .ToList();
    }

    private PublicDemoHandoffState ReadHandoffState(string? writtenAt = null, DdsopConfigInboundMessage? payload = null)
    {
        if (payload is null && File.Exists(_options.DdaeToSdbrPayloadPath))
        {
            try
            {
                payload = JsonSerializer.Deserialize<DdsopConfigInboundMessage>(
                    File.ReadAllText(_options.DdaeToSdbrPayloadPath),
                    DdsopConfigInboundContractService.ContractJsonOptions);
            }
            catch (JsonException)
            {
                payload = null;
            }
        }

        return new PublicDemoHandoffState(
            _options.DdaeToSdbrPayloadPath,
            File.Exists(_options.DdaeToSdbrPayloadPath),
            writtenAt ?? (File.Exists(_options.DdaeToSdbrPayloadPath) ? File.GetLastWriteTimeUtc(_options.DdaeToSdbrPayloadPath).ToString("yyyy-MM-dd'T'HH:mm:sszzz") : null),
            payload?.MessageID,
            payload?.Payload.OperatingModelConfigurationID,
            payload?.Payload.Fingerprint);
    }

    private IReadOnlyList<PublicDemoFeedbackFileState> ReadFeedbackStates()
    {
        return new[]
        {
            ReadFeedbackState("PlanningRunFeedback", _options.PlanningRunFeedbackPath),
            ReadFeedbackState("VarianceAnalysisFeedback", _options.VarianceAnalysisFeedbackPath),
            ReadValidationSummaryState()
        };
    }

    private PublicDemoFeedbackFileState ReadFeedbackState(string name, string path)
    {
        if (!File.Exists(path))
        {
            return new PublicDemoFeedbackFileState(name, path, false, "等待回传", null, null, null, null, null, null, null, null, null, null, null, null, Array.Empty<string>(), 0, 0, "尚未发现 SDBR handoff 文件。", 0);
        }

        var raw = File.ReadAllText(path);
        try
        {
            var interpretation = _feedbackInterpreter.Interpret(raw);
            using var document = JsonDocument.Parse(raw);
            var payload = document.RootElement.TryGetProperty("Payload", out var payloadElement) && payloadElement.ValueKind == JsonValueKind.Object
                ? payloadElement
                : default;
            return new PublicDemoFeedbackFileState(
                name,
                path,
                true,
                interpretation.Status,
                interpretation.MessageID,
                interpretation.PlanningRunID,
                interpretation.OperatingModelConfigurationID,
                interpretation.OperatingModelFingerprint,
                interpretation.OverallStatus,
                GetString(payload, "RunStatus"),
                GetString(payload, "SolverStatus"),
                GetString(payload, "ReliabilityStatus"),
                GetString(payload, "SpeedStatus"),
                GetString(payload, "StabilityStatus"),
                null,
                null,
                Array.Empty<string>(),
                interpretation.ReviewTopicCount,
                interpretation.DataCoverageIssueCount,
                interpretation.Message,
                raw.Length);
        }
        catch (JsonException ex)
        {
            return new PublicDemoFeedbackFileState(name, path, true, "无法解释", null, null, null, null, null, null, null, null, null, null, null, null, Array.Empty<string>(), 0, 0, ex.Message, raw.Length);
        }
    }

    private PublicDemoFeedbackFileState ReadValidationSummaryState()
    {
        var path = _options.ValidationSummaryPath;
        if (!File.Exists(path))
        {
            return new PublicDemoFeedbackFileState("ValidationSummary", path, false, "可选文件未回传", null, null, null, null, null, null, null, null, null, null, null, null, Array.Empty<string>(), 0, 0, "SDBR validation summary 是可选 handoff。", 0);
        }

        var raw = File.ReadAllText(path);
        try
        {
            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;
            var frozen = root.TryGetProperty("FrozenConfiguration", out var frozenElement) && frozenElement.ValueKind == JsonValueKind.Object
                ? frozenElement
                : default;
            var validationStatus = GetString(root, "ValidationStatus");
            return new PublicDemoFeedbackFileState(
                "ValidationSummary",
                path,
                true,
                validationStatus ?? "已读取",
                null,
                GetString(root, "DemoRunID"),
                GetString(frozen, "OperatingModelConfigurationID"),
                GetString(frozen, "OperatingModelFingerprint"),
                validationStatus,
                GetString(root, "RunStatus"),
                null,
                null,
                null,
                null,
                validationStatus,
                GetString(root, "MappingConfidence"),
                ReadStringArray(root, "Labels"),
                0,
                0,
                "已读取 SDBR 可选 validation summary。",
                raw.Length);
        }
        catch (JsonException ex)
        {
            return new PublicDemoFeedbackFileState("ValidationSummary", path, true, "无法解释", null, null, null, null, null, null, null, null, null, null, null, null, Array.Empty<string>(), 0, 0, ex.Message, raw.Length);
        }
    }

    private IReadOnlyList<PublicDemoReviewedMapping> BuildReviewedMappings()
    {
        return new[]
        {
            new PublicDemoReviewedMapping(
                "PART-FPGA-SPACE",
                "Demo item identity only; not production item master authority.",
                "Controlled Contract Golden Loop Demo",
                "Not production item master authority; no automatic master-data update."),
            new PublicDemoReviewedMapping(
                "WH-ELEC-QA",
                "Demo location identity only; not production location or item-location authority.",
                "Controlled Contract Golden Loop Demo",
                "Not production location authority; not production inventory authority."),
            new PublicDemoReviewedMapping(
                "EA",
                "Demo UOM semantics only; not enterprise UOM authority.",
                "Controlled Contract Golden Loop Demo",
                "Not enterprise UOM authority."),
            new PublicDemoReviewedMapping(
                "PART-FPGA-SPACE @ WH-ELEC-QA",
                "Demo item-location context only.",
                "Controlled Contract Golden Loop Demo",
                "Not production item-location authority.")
        };
    }

    private static PublicDemoSchedulingAdapterReadModel BuildSchedulingAdapterReadModel()
    {
        return new PublicDemoSchedulingAdapterReadModel(
            DdsopRuntimePlanningInputContractService.AdventureWorksAdapterProfileID,
            ScenarioLabel,
            MappingConfidence,
            "SDBR feedback is interpreted as review/governance context only; it cannot mutate approved DDAE operating model, master settings, buffers, lead time, MOQ, order cycle, or supplier-source facts.",
            new[]
            {
                new PublicDemoSchedulingGovernancePolicy("控制点策略", "发布 DDS&OP 治理意图和冻结证据，不发布可执行工艺路线。", "ControlPoint", "DDAE-SCHEDULING-RULE-V1"),
                new PublicDemoSchedulingGovernancePolicy("资源角色策略", "标识资源角色和保护边界，不发布资源日历或能力窗口。", "ResourcePolicy", "DDAE-SCHEDULING-RULE-V1"),
                new PublicDemoSchedulingGovernancePolicy("释放策略", "发布计划释放规则和只读消费约束，不创建工单执行状态。", "ReleasePolicy", "DDAE-SCHEDULING-RULE-V1"),
                new PublicDemoSchedulingGovernancePolicy("时间缓冲策略", "发布时间缓冲治理口径，不证明生产提前期绩效。", "TimeBuffer", "DDAE-SCHEDULING-RULE-V1"),
                new PublicDemoSchedulingGovernancePolicy("优先级策略", "发布 DDS&OP 优先级偏好，不替代 SDBR 派工规则。", "ReleasePolicy", "DDAE-SCHEDULING-RULE-V1"),
                new PublicDemoSchedulingGovernancePolicy("计划窗口", "发布有效期和计划窗口边界，不生成可执行排程日历。", "EffectivePolicyID", "DDAE-SCHEDULING-RULE-V1"),
                new PublicDemoSchedulingGovernancePolicy("批准与生效证据", "保留批准人、批准时间和生效策略证据。", "ApprovalEvidenceID", "DDAE-SCHEDULING-RULE-V1")
            },
            new[]
            {
                new PublicDemoAdapterMetadataItem("AdapterProfileID", DdsopRuntimePlanningInputContractService.AdventureWorksAdapterProfileID, "Contract Agent / SDBR adapter", "DDAE display and audit only.", "Cannot be used as DDAE executable routing authority."),
                new PublicDemoAdapterMetadataItem("MaterialConstraintsMode", DdsopRuntimePlanningInputContractService.AdventureWorksMaterialConstraintsMode, "SDBR adapter", "当前页面级演示链路省略物料约束；如后续保留候选物料证据，也只能作为未执行的评审上下文。", "Cannot be interpreted as active material-feasible scheduling or production material feasibility authority."),
                new PublicDemoAdapterMetadataItem("CapacityUnitNormalizationRuleID", DdsopRuntimePlanningInputContractService.AdventureWorksCapacityUnitNormalizationRuleID, "SDBR adapter", "DDAE may display normalization rule for audit trace.", "Cannot be used as DDAE capacity calendar authority."),
                new PublicDemoAdapterMetadataItem("SetupChangeoverMode", DdsopRuntimePlanningInputContractService.AdventureWorksSetupChangeoverMode, "SDBR adapter", "DDAE may display setup/changeover handling mode.", "Cannot be used as DDAE setup/changeover execution rule authority.")
            },
            new[]
            {
                new PublicDemoNonDdaeOwnedExecutionItem("资源日历 / 能力窗口", "SDBR / adapter fixture", "只读展示与审计。", "DDAE must not author executable calendars or capacity windows."),
                new PublicDemoNonDdaeOwnedExecutionItem("工序时长", "SDBR / adapter fixture", "只读解释 bounded scheduling 输出。", "DDAE must not own operation durations."),
                new PublicDemoNonDdaeOwnedExecutionItem("可执行 routing 主数据", "SDBR / ERP/MES adapter", "只读解释 routing path signature。", "DDAE routing governance is not executable routing master."),
                new PublicDemoNonDdaeOwnedExecutionItem("工单执行状态", "SDBR / MES", "只读解释 feedback。", "DDAE must not create work-order execution state."),
                new PublicDemoNonDdaeOwnedExecutionItem("物料可行性权威", "WMS/QMS/SDBR adapter", "当前 active demo 显示为 OmittedForPublicDemo；候选物料证据只可作为未执行/非生产可行性上下文。", "DDAE must not claim active material-feasible scheduling or production material feasibility authority.")
            });
    }

    private JsonObject ReadJsonObject(string fileName)
    {
        var path = Path.Combine(_options.PackagePath, fileName);
        if (!File.Exists(path))
        {
            return new JsonObject();
        }

        return JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? new JsonObject();
    }

    private JsonObject ReadFirstObject(string fileName)
    {
        var path = Path.Combine(_options.PackagePath, fileName);
        if (!File.Exists(path))
        {
            return new JsonObject();
        }

        var node = JsonNode.Parse(File.ReadAllText(path));
        return node is JsonArray { Count: > 0 } array && array[0] is JsonObject first
            ? first
            : new JsonObject();
    }

    private decimal ReadFirstDecimal(string fileName, string propertyName, decimal fallback)
    {
        var first = ReadFirstObject(fileName);
        return first.TryGetPropertyValue(propertyName, out var node) && node is not null && node.GetValueKind() == JsonValueKind.Number
            ? node.GetValue<decimal>()
            : fallback;
    }

    private string ReadFirstString(string fileName, string propertyName, string fallback)
    {
        var first = ReadFirstObject(fileName);
        return GetString(first, propertyName) ?? fallback;
    }

    private string ReadTextIfExists(string fileName)
    {
        var path = Path.Combine(_options.PackagePath, fileName);
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static string SummarizeNonClaims(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "non-claims.md 未读取到；页面仍禁止生产验证和 Business Golden Loop readiness 声明。";
        }

        return "公开演示包仅用于 Controlled Contract Golden Loop Demo；不代表生产验证、Business Golden Loop readiness、生产权威或自动主数据更新。";
    }

    private static string? GetString(JsonObject obj, string propertyName)
    {
        return obj.TryGetPropertyValue(propertyName, out var node) && node is not null && node.GetValueKind() == JsonValueKind.String
            ? node.GetValue<string>()
            : null;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object
               && element.TryGetProperty(propertyName, out var value)
               && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object
            || !element.TryGetProperty(propertyName, out var value)
            || value.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return value.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
    }

    private static int GetInt(JsonObject obj, string propertyName)
    {
        return obj.TryGetPropertyValue(propertyName, out var node) && node is not null && node.GetValueKind() == JsonValueKind.Number
            ? node.GetValue<int>()
            : 0;
    }

    public static string ComputeFileSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
