using System.Security.Cryptography;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AdaptiveSopDdsop.Web.Domain;

public sealed record DdsopConfigInboundContractRequest(
    int HorizonWeeks = 12,
    DateOnly? AnchorDate = null,
    string? ApprovedBy = null,
    string? SourceScenarioRunID = null,
    string? ChangeTicketID = null);

public sealed record DdsopConfigInboundMessage(
    string ContractID,
    string ContractVersion,
    string MessageID,
    string MessageType,
    string SourceSystem,
    string TargetSystem,
    string IdempotencyKey,
    string OccurredAt,
    DdsopOperatingModelConfiguration Payload);

public sealed record DdsopOperatingModelConfiguration(
    string OperatingModelConfigurationID,
    string ConfigurationVersion,
    string SchemaVersion,
    string Status,
    string EffectiveFrom,
    string? EffectiveTo,
    string TimeZone,
    DdsopScope Scope,
    DdsopApproval Approval,
    DdsopChangeReason ChangeReason,
    string? SupersedesConfigurationID,
    string? SourceScenarioRunID,
    string? AssumptionSetID,
    string? ChangeTicketID,
    string Fingerprint,
    DdsopSchedulingConfiguration SchedulingConfiguration,
    DdsopDdmrpConfiguration DDMRPConfiguration);

public sealed record DdsopScope(
    IReadOnlyList<string> PlantIDs,
    IReadOnlyList<string>? ProductFamilyIDs,
    IReadOnlyList<string>? ResourceGroupIDs,
    IReadOnlyList<string>? ItemLocationIDs);

public sealed record DdsopApproval(
    string ApprovedBy,
    string ApprovedAt,
    string ApprovalStatus);

public sealed record DdsopChangeReason(
    string ReasonCode,
    string Description);

public sealed record DdsopSchedulingConfiguration(
    string SchedulingConfigurationID,
    string SchedulingStrategyID,
    string ReleasePolicyVersionID,
    int FreezeWindowMinutes,
    int? NegotiableWindowMinutes,
    string ProtectedDueDatePolicy,
    string FiniteResourceScope,
    IReadOnlyList<DdsopControlPoint> ControlPoints,
    IReadOnlyList<DdsopTimeBufferProfile> TimeBufferProfiles,
    IReadOnlyList<DdsopTimeBufferAssignment> TimeBufferAssignments,
    IReadOnlyList<DdsopResourceSetting> ResourceSettings,
    IReadOnlyList<DdsopPartSchedulingSetting> PartSchedulingSettings);

public sealed record DdsopControlPoint(
    string ControlPointID,
    string ResourceID,
    string ControlPointType,
    bool FiniteScheduling,
    string? SequencePolicy);

public sealed record DdsopTimeBufferProfile(
    string ProfileID,
    string? Description,
    int TotalBufferMinutes,
    decimal GreenRatio,
    decimal YellowRatio,
    decimal RedRatio,
    int? LateThresholdMinutes);

public sealed record DdsopTimeBufferAssignment(
    string AssignmentID,
    string ProductID,
    string OrderType,
    string ControlPointID,
    string ProfileID);

public sealed record DdsopResourceSetting(
    string ResourceID,
    string Role,
    string CapacityMode,
    string CalendarID,
    decimal EfficiencyPercent);

public sealed record DdsopPartSchedulingSetting(
    string ProductID,
    string PrimaryRoutingID,
    IReadOnlyList<string>? AllowedAlternateResources,
    string SchedulingPriorityClass);

public sealed record DdsopDdmrpConfiguration(
    string DDMRPConfigurationID,
    string PlanningPriorityPolicyID,
    int? SpikeHorizonMinutes,
    string? SpikeQualificationMode,
    IReadOnlyList<DdsopDecouplingPoint> DecouplingPoints,
    IReadOnlyList<DdsopStockBufferProfile> StockBufferProfiles,
    IReadOnlyList<DdsopPartProfileAssignment> PartProfileAssignments,
    IReadOnlyList<DdsopAdjustmentFactor>? AdjustmentFactors);

public sealed record DdsopDecouplingPoint(
    string ItemID,
    string LocationID,
    string BufferProfileID,
    int DLTMinutes,
    decimal OrderMultipleQty,
    decimal MinimumOrderQty,
    int OrderCycleDays);

public sealed record DdsopStockBufferProfile(
    string BufferProfileID,
    decimal TopOfRed,
    decimal TopOfYellow,
    decimal TopOfGreen,
    string UnitOfMeasure);

public sealed record DdsopPartProfileAssignment(
    string ItemID,
    string LocationID,
    string BufferProfileID);

public sealed record DdsopAdjustmentFactor(
    string AdjustmentID,
    string ItemID,
    string LocationID,
    string FactorType,
    string EffectiveFrom,
    string? EffectiveTo,
    decimal Multiplier);

public sealed record DdsopFeedbackInterpretation(
    string ContractID,
    string ContractVersion,
    string MessageID,
    string MessageType,
    string SourceSystem,
    string TargetSystem,
    string IdempotencyKey,
    string Status,
    string? FeedbackType,
    string? PlanningRunID,
    string? OperatingModelConfigurationID,
    string? OperatingModelFingerprint,
    string? MasterDataVersionID,
    string? OperationalStateSnapshotID,
    string? OverallStatus,
    int ReviewTopicCount,
    int DataCoverageIssueCount,
    int ApprovedConfigurationChangeCount,
    string Message);

public sealed record DdsopContractError(
    string Code,
    string Message,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Field = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Severity = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyDictionary<string, object?>? Evidence = null);

public sealed record DdsopPendingReference(
    string Field,
    string ReferenceID,
    string ReferenceType,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Message = null);

public sealed record DdsopConfigInboundAckInterpretation(
    string ContractID,
    string ContractVersion,
    string OriginalMessageID,
    string IdempotencyKey,
    string ProcessingStatus,
    bool UsableForPlanningRun,
    string? AcceptedConfigurationID,
    string? Fingerprint,
    IReadOnlyList<DdsopPendingReference> PendingReferences,
    IReadOnlyList<DdsopContractError> Errors,
    string Message);

public sealed record DdsopFeedbackOutboundAck(
    string ContractID,
    string ContractVersion,
    string OriginalMessageID,
    string IdempotencyKey,
    string ProcessingStatus,
    string ReceivedAt,
    string? LinkedOperatingModelConfigurationID,
    string? LinkedOperatingModelFingerprint,
    string? LinkedPlanningRunID,
    IReadOnlyList<DdsopContractError> Errors);

public sealed record DdsopFeedbackLedgerRecord(
    string OriginalMessageID,
    string IdempotencyKey,
    string ProcessingStatus,
    string ReceivedAt,
    string RawPayload,
    DdsopFeedbackInterpretation Interpretation,
    DdsopFeedbackOutboundAck Ack);

public sealed class DdsopConfigInboundContractService
{
    public static readonly JsonSerializerOptions ContractJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = false
    };

    private static readonly DateOnly DefaultAnchorDate = new(2026, 6, 26);
    private readonly IScenarioWorkspaceDataSource _dataSource;

    public DdsopConfigInboundContractService(IScenarioWorkspaceDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public DdsopConfigInboundMessage Build(DdsopConfigInboundContractRequest request)
    {
        var horizonWeeks = Math.Clamp(request.HorizonWeeks, 1, 52);
        var anchorDate = request.AnchorDate ?? DefaultAnchorDate;
        var workspace = _dataSource.Load(new ScenarioWorkspaceDataRequest(horizonWeeks, anchorDate));
        var occurredAt = ToShanghaiOffset(anchorDate, 16, 0, 0);
        var approvedAt = occurredAt.AddMinutes(-15);
        var effectiveFrom = ToShanghaiOffset(anchorDate.AddDays(1), 0, 0, 0);
        var dateToken = anchorDate.ToString("yyyyMMdd");
        var configurationVersion = $"{anchorDate:yyyy.MM}-A";
        var operatingModelId = $"DDSOP-OMC-{dateToken}-A";
        var messageId = $"DDAE-MSG-OMC-{dateToken}-001";

        var payloadWithoutFingerprint = new DdsopOperatingModelConfiguration(
            operatingModelId,
            configurationVersion,
            "1.0.0",
            "Approved",
            FormatDateTime(effectiveFrom),
            null,
            "Asia/Shanghai",
            BuildScope(workspace),
            new DdsopApproval(
                string.IsNullOrWhiteSpace(request.ApprovedBy) ? "ddsop-governance-board" : request.ApprovedBy,
                FormatDateTime(approvedAt),
                "Approved"),
            new DdsopChangeReason(
                "CAPACITY_CONSTRAINT",
                "DDS&OP approved operating model configuration for demand-driven buffer, control point, and capacity boundary governance."),
            null,
            request.SourceScenarioRunID,
            $"ASSUMPTION-{dateToken}-A",
            request.ChangeTicketID,
            string.Empty,
            BuildSchedulingConfiguration(workspace, dateToken, configurationVersion),
            BuildDdmrpConfiguration(workspace, dateToken));
        var fingerprint = ComputeFingerprint(payloadWithoutFingerprint);
        var payload = payloadWithoutFingerprint with { Fingerprint = fingerprint };

        return new DdsopConfigInboundMessage(
            "DDSOP-CONFIG-INBOUND-V1",
            "1.0.0",
            messageId,
            "OperatingModelConfigurationPublished",
            "DDAE",
            "SDBR",
            $"DDAE:{messageId}",
            FormatDateTime(occurredAt),
            payload);
    }

    public static string ComputeFingerprint(DdsopOperatingModelConfiguration payload)
    {
        var node = JsonSerializer.SerializeToNode(payload, ContractJsonOptions)
            ?? throw new InvalidOperationException("Payload could not be serialized for fingerprinting.");
        if (node is not JsonObject payloadObject)
        {
            throw new InvalidOperationException("Payload fingerprint source must be a JSON object.");
        }

        payloadObject.Remove(nameof(DdsopOperatingModelConfiguration.Fingerprint));
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
               {
                   Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                   Indented = false
               }))
        {
            WriteCanonicalJson(writer, payloadObject);
        }

        var hash = SHA256.HashData(stream.ToArray());
        return $"sha256:{Convert.ToHexString(hash).ToLowerInvariant()}";
    }

    private static DdsopSchedulingConfiguration BuildSchedulingConfiguration(
        ScenarioWorkspaceDataSet workspace,
        string dateToken,
        string configurationVersion)
    {
        var resources = workspace.Resources
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .ToList();
        var routedResourceCodes = workspace.ResourceRoutings
            .Select(item => item.ResourceCode)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        if (routedResourceCodes.Count == 0 && resources.Count > 0)
        {
            routedResourceCodes.Add(resources[0].Code);
        }

        var resourceByCode = resources.ToDictionary(item => item.Code, StringComparer.Ordinal);
        var controlPoints = routedResourceCodes
            .Select(resourceCode =>
            {
                resourceByCode.TryGetValue(resourceCode, out var resource);
                var type = resource is not null && resource.UnitLoad >= 0.85m
                    ? "Constraint"
                    : "CapacityBuffer";
                return new DdsopControlPoint(
                    $"CP-{Slug(resourceCode)}",
                    resourceCode,
                    type,
                    true,
                    "BufferPriorityFirst");
            })
            .ToList();
        if (controlPoints.Count == 0)
        {
            controlPoints.Add(new DdsopControlPoint("CP-DDAE-DEFAULT", "DDAE-DEFAULT-RESOURCE", "CapacityBuffer", true, "BufferPriorityFirst"));
        }

        var controlPointByResource = controlPoints.ToDictionary(item => item.ResourceID, item => item.ControlPointID, StringComparer.Ordinal);
        var profileByFamily = workspace.DdmrpParameters
            .GroupBy(item => item.Family)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group =>
            {
                var averageDltDays = group.Average(item => (decimal)item.DecoupledLeadTimeDays);
                var totalBufferMinutes = Math.Max(60, (int)Math.Round(averageDltDays * 1440m * 0.1m, MidpointRounding.AwayFromZero));
                return new DdsopTimeBufferProfile(
                    $"TB-{Slug(group.Key)}",
                    $"{group.Key} time buffer profile approved by DDS&OP.",
                    totalBufferMinutes,
                    0.33m,
                    0.33m,
                    0.34m,
                    0);
            })
            .ToList();
        if (profileByFamily.Count == 0)
        {
            profileByFamily.Add(new DdsopTimeBufferProfile("TB-DDAE-DEFAULT", "Default DDS&OP time buffer profile.", 480, 0.33m, 0.33m, 0.34m, 0));
        }

        var profileIdByFamily = profileByFamily
            .ToDictionary(item => item.ProfileID.Replace("TB-", "", StringComparison.Ordinal), item => item.ProfileID, StringComparer.Ordinal);
        var firstControlPointId = controlPoints[0].ControlPointID;
        var firstRoutingBySku = workspace.ResourceRoutings
            .GroupBy(item => item.Sku)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var assignments = workspace.Skus
            .OrderBy(item => item.Sku, StringComparer.Ordinal)
            .Select(sku =>
            {
                firstRoutingBySku.TryGetValue(sku.Sku, out var routing);
                var controlPointId = routing is not null
                    ? controlPointByResource.GetValueOrDefault(routing.ResourceCode, firstControlPointId)
                    : firstControlPointId;
                return new DdsopTimeBufferAssignment(
                    $"TBA-{Slug(sku.Sku)}",
                    sku.Sku,
                    "MTS",
                    controlPointId,
                    profileIdByFamily.GetValueOrDefault(Slug(sku.Family), profileByFamily[0].ProfileID));
            })
            .ToList();

        var resourceSettings = resources.Count == 0
            ? new List<DdsopResourceSetting>
            {
                new("DDAE-DEFAULT-RESOURCE", "Resource", "Finite", "CAL-DDAE-DEFAULT-RESOURCE", 100m)
            }
            : resources.Select(resource =>
            {
                var isControlPoint = controlPointByResource.ContainsKey(resource.Code);
                var role = isControlPoint && resource.UnitLoad >= 0.85m
                    ? "Constraint"
                    : isControlPoint
                        ? "BufferedResource"
                        : "Resource";
                return new DdsopResourceSetting(
                    resource.Code,
                    role,
                    "Finite",
                    $"CAL-{Slug(resource.Code)}",
                    100m);
            }).ToList();

        var routingsBySku = workspace.ResourceRoutings
            .GroupBy(item => item.Sku)
            .ToDictionary(
                group => group.Key,
                group => group.Select(route => route.ResourceCode).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
                StringComparer.Ordinal);
        var partSettings = workspace.Skus
            .OrderBy(item => item.Sku, StringComparer.Ordinal)
            .Select(sku =>
            {
                routingsBySku.TryGetValue(sku.Sku, out var resourceCodes);
                resourceCodes ??= new List<string>();
                return new DdsopPartSchedulingSetting(
                    sku.Sku,
                    $"ROUTE-{Slug(sku.Sku)}",
                    resourceCodes.Skip(1).ToList(),
                    "Standard");
            })
            .ToList();

        return new DdsopSchedulingConfiguration(
            $"DDSOP-SCH-{dateToken}-A",
            "SCH-STRATEGY-DDOM-FLOW-001",
            $"DBR-RELEASE-POLICY-{dateToken}-A",
            1440,
            2880,
            "PromiseDate",
            "ControlPointsOnly",
            controlPoints,
            profileByFamily,
            assignments,
            resourceSettings,
            partSettings);
    }

    private static DdsopDdmrpConfiguration BuildDdmrpConfiguration(
        ScenarioWorkspaceDataSet workspace,
        string dateToken)
    {
        var profiles = workspace.DdmrpParameters
            .OrderBy(item => item.Sku, StringComparer.Ordinal)
            .Select(item => new DdsopStockBufferProfile(
                BufferProfileId(item),
                item.TopOfRed,
                Math.Max(item.TopOfYellow, item.TopOfRed),
                Math.Max(item.TopOfGreen, Math.Max(item.TopOfYellow, item.TopOfRed)),
                "EA"))
            .ToList();
        var decouplingPoints = workspace.DdmrpParameters
            .OrderBy(item => item.Sku, StringComparer.Ordinal)
            .Select(item => new DdsopDecouplingPoint(
                item.Sku,
                LocationId(item),
                BufferProfileId(item),
                item.DecoupledLeadTimeDays * 1440,
                item.MinimumOrderQuantity,
                item.MinimumOrderQuantity,
                Math.Max(item.OrderCycleDays, 1)))
            .ToList();
        var assignments = workspace.DdmrpParameters
            .OrderBy(item => item.Sku, StringComparer.Ordinal)
            .Select(item => new DdsopPartProfileAssignment(
                item.Sku,
                LocationId(item),
                BufferProfileId(item)))
            .ToList();
        var adjustmentFactors = workspace.DdmrpParameters
            .SelectMany(item =>
            {
                var entries = new List<DdsopAdjustmentFactor>();
                var effectiveFrom = FormatDateTime(ToShanghaiOffset(new DateOnly(2026, 6, 27), 0, 0, 0));
                if (item.DemandAdjustmentFactor != 1m)
                {
                    entries.Add(new DdsopAdjustmentFactor(
                        $"AF-{Slug(item.Sku)}-DAF",
                        item.Sku,
                        LocationId(item),
                        "DemandAdjustment",
                        effectiveFrom,
                        null,
                        item.DemandAdjustmentFactor));
                }

                if (item.ZoneAdjustmentFactor != 1m)
                {
                    entries.Add(new DdsopAdjustmentFactor(
                        $"AF-{Slug(item.Sku)}-ZONE",
                        item.Sku,
                        LocationId(item),
                        "ZoneAdjustment",
                        effectiveFrom,
                        null,
                        item.ZoneAdjustmentFactor));
                }

                return entries;
            })
            .OrderBy(item => item.AdjustmentID, StringComparer.Ordinal)
            .ToList();

        return new DdsopDdmrpConfiguration(
            $"DDSOP-DDMRP-{dateToken}-A",
            "DDMRP-PRIORITY-RED-YELLOW-GREEN-001",
            10080,
            "ProvidedByDDSOP",
            decouplingPoints,
            profiles,
            assignments,
            adjustmentFactors);
    }

    private static DdsopScope BuildScope(ScenarioWorkspaceDataSet workspace)
    {
        var itemLocationIds = workspace.DdmrpParameters
            .Select(item => $"{item.Sku}@{LocationId(item)}")
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        return new DdsopScope(
            new[] { "PLANT-DDAE-SAT" },
            workspace.Families.Select(item => item.Code).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            workspace.Resources.Select(item => item.Code).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            itemLocationIds);
    }

    private static void WriteCanonicalJson(Utf8JsonWriter writer, JsonNode? node)
    {
        switch (node)
        {
            case null:
                writer.WriteNullValue();
                return;
            case JsonObject jsonObject:
                writer.WriteStartObject();
                foreach (var property in jsonObject.OrderBy(item => item.Key, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Key);
                    WriteCanonicalJson(writer, property.Value);
                }

                writer.WriteEndObject();
                return;
            case JsonArray jsonArray:
                writer.WriteStartArray();
                foreach (var item in jsonArray)
                {
                    WriteCanonicalJson(writer, item);
                }

                writer.WriteEndArray();
                return;
            case JsonValue jsonValue:
                WriteCanonicalValue(writer, jsonValue);
                return;
            default:
                node.WriteTo(writer);
                return;
        }
    }

    private static void WriteCanonicalValue(Utf8JsonWriter writer, JsonValue value)
    {
        if (value.TryGetValue<string>(out var stringValue))
        {
            writer.WriteStringValue(stringValue);
        }
        else if (value.TryGetValue<bool>(out var boolValue))
        {
            writer.WriteBooleanValue(boolValue);
        }
        else if (value.TryGetValue<int>(out var intValue))
        {
            writer.WriteNumberValue(intValue);
        }
        else if (value.TryGetValue<long>(out var longValue))
        {
            writer.WriteNumberValue(longValue);
        }
        else if (value.TryGetValue<decimal>(out var decimalValue))
        {
            writer.WriteRawValue(NormalizeDecimal(decimalValue), skipInputValidation: true);
        }
        else if (value.TryGetValue<double>(out var doubleValue))
        {
            writer.WriteRawValue(doubleValue.ToString("G17", CultureInfo.InvariantCulture), skipInputValidation: true);
        }
        else
        {
            value.WriteTo(writer);
        }
    }

    private static string NormalizeDecimal(decimal value)
    {
        return value == decimal.Zero
            ? "0"
            : value.ToString("0.#############################", CultureInfo.InvariantCulture);
    }

    private static DateTimeOffset ToShanghaiOffset(DateOnly date, int hour, int minute, int second)
    {
        return new DateTimeOffset(date.Year, date.Month, date.Day, hour, minute, second, TimeSpan.FromHours(8));
    }

    private static string FormatDateTime(DateTimeOffset value)
    {
        return value.ToString("yyyy-MM-dd'T'HH:mm:sszzz");
    }

    private static string BufferProfileId(DdmrpParameterProfile item)
    {
        return $"SB-{Slug(item.Sku)}";
    }

    private static string LocationId(DdmrpParameterProfile item)
    {
        var basis = string.IsNullOrWhiteSpace(item.DecouplingPoint)
            ? item.Family
            : item.DecouplingPoint;
        return $"LOC-{Slug(basis)}";
    }

    private static string Slug(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToUpperInvariant(character));
            }
            else if (builder.Length == 0 || builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        return builder.ToString().Trim('-');
    }
}

public sealed class DdsopConfigInboundAckInterpreter
{
    public DdsopConfigInboundAckInterpretation Interpret(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var contractId = GetString(root, "ContractID") ?? string.Empty;
        var contractVersion = GetString(root, "ContractVersion") ?? string.Empty;
        var originalMessageId = GetString(root, "OriginalMessageID") ?? string.Empty;
        var idempotencyKey = GetString(root, "IdempotencyKey") ?? string.Empty;
        var status = GetString(root, "ProcessingStatus") ?? string.Empty;
        var usable = root.TryGetProperty("UsableForPlanningRun", out var usableElement)
            && usableElement.ValueKind is JsonValueKind.True;
        var acceptedConfigurationId = GetString(root, "AcceptedConfigurationID");
        var fingerprint = GetString(root, "Fingerprint");
        var pendingReferences = ReadPendingReferences(root);
        var errors = ReadErrors(root);

        var message = status switch
        {
            "Accepted" => usable
                ? "SDBR 已接受配置，且可用于 Planning Run。"
                : "SDBR 已接受配置，但 ACK 标记为不可用于 Planning Run。",
            "AcceptedPendingReferences" => "SDBR 已接收配置，但仍有待解析引用，暂不可用于 Planning Run。",
            "Rejected" => "SDBR 已拒绝配置，需要按 ACK 错误修正后重发。",
            "Duplicate" => "SDBR 识别为重复 IdempotencyKey，应复用原处理结果，不重复发布。",
            "DeadLettered" => "SDBR 已将消息转入死信，需要人工处理。",
            _ => "ACK 状态无法按当前契约解释。"
        };

        return new DdsopConfigInboundAckInterpretation(
            contractId,
            contractVersion,
            originalMessageId,
            idempotencyKey,
            status,
            usable,
            acceptedConfigurationId,
            fingerprint,
            pendingReferences,
            errors,
            message);
    }

    private static IReadOnlyList<DdsopPendingReference> ReadPendingReferences(JsonElement root)
    {
        if (!root.TryGetProperty("PendingReferences", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<DdsopPendingReference>();
        }

        return items.EnumerateArray()
            .Select(item => new DdsopPendingReference(
                GetString(item, "Field") ?? string.Empty,
                GetString(item, "ReferenceID") ?? string.Empty,
                GetString(item, "ReferenceType") ?? string.Empty,
                GetString(item, "Message")))
            .ToList();
    }

    private static IReadOnlyList<DdsopContractError> ReadErrors(JsonElement root)
    {
        if (!root.TryGetProperty("Errors", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<DdsopContractError>();
        }

        return items.EnumerateArray()
            .Select(item => new DdsopContractError(
                GetString(item, "Code") ?? string.Empty,
                GetString(item, "Message") ?? string.Empty,
                GetString(item, "Field"),
                GetString(item, "Severity")))
            .ToList();
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }
}

public sealed class DdsopFeedbackOutboundInterpreter
{
    public DdsopFeedbackInterpretation Interpret(string json, ISet<string>? receivedIdempotencyKeys = null)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var contractId = GetString(root, "ContractID");
        var contractVersion = GetString(root, "ContractVersion");
        var messageId = GetString(root, "MessageID");
        var messageType = GetString(root, "MessageType");
        var sourceSystem = GetString(root, "SourceSystem");
        var targetSystem = GetString(root, "TargetSystem");
        var idempotencyKey = GetString(root, "IdempotencyKey");

        if (receivedIdempotencyKeys is not null && idempotencyKey is not null && receivedIdempotencyKeys.Contains(idempotencyKey))
        {
            return new DdsopFeedbackInterpretation(
                contractId ?? string.Empty,
                contractVersion ?? string.Empty,
                messageId ?? string.Empty,
                messageType ?? string.Empty,
                sourceSystem ?? string.Empty,
                targetSystem ?? string.Empty,
                idempotencyKey,
                "Duplicate",
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                0,
                0,
                0,
                "Duplicate idempotency key.");
        }

        if (!root.TryGetProperty("Payload", out var payload) || payload.ValueKind != JsonValueKind.Object)
        {
            return Rejected(contractId, contractVersion, messageId, messageType, sourceSystem, targetSystem, idempotencyKey, "Payload is missing.");
        }

        var feedbackType = GetString(payload, "FeedbackType");
        var planningRunId = GetString(payload, "PlanningRunID");
        var operatingModelId = GetString(payload, "OperatingModelConfigurationID");
        var fingerprint = GetString(payload, "OperatingModelFingerprint");
        var masterDataVersionId = GetString(payload, "MasterDataVersionID");
        var snapshotId = GetString(payload, "OperationalStateSnapshotID");

        if (string.IsNullOrWhiteSpace(planningRunId) || string.IsNullOrWhiteSpace(operatingModelId))
        {
            return Rejected(contractId, contractVersion, messageId, messageType, sourceSystem, targetSystem, idempotencyKey, "PlanningRunID or OperatingModelConfigurationID is missing.");
        }

        var overallStatus = feedbackType switch
        {
            "PlanningRunFeedback" when payload.TryGetProperty("OperationalMetrics", out var metrics) => GetString(metrics, "OverallStatus"),
            "VarianceAnalysisFeedback" => GetString(payload, "OverallStatus"),
            _ => null
        };
        var reviewTopics = CountArray(payload, "RecommendedDDSOPReviewTopics");
        var coverageIssues = CountArray(payload, "DataCoverageIssues");
        var approvedChanges = CountApprovedConfigurationChanges(payload);

        return new DdsopFeedbackInterpretation(
            contractId ?? string.Empty,
            contractVersion ?? string.Empty,
            messageId ?? string.Empty,
            messageType ?? string.Empty,
            sourceSystem ?? string.Empty,
            targetSystem ?? string.Empty,
            idempotencyKey ?? string.Empty,
            "Accepted",
            feedbackType,
            planningRunId,
            operatingModelId,
            fingerprint,
            masterDataVersionId,
            snapshotId,
            overallStatus,
            reviewTopics,
            coverageIssues,
            approvedChanges,
            approvedChanges == 0
                ? "Feedback interpreted without creating approved configuration changes."
                : "Feedback contains an unexpected approved configuration change flag.");
    }

    private static DdsopFeedbackInterpretation Rejected(
        string? contractId,
        string? contractVersion,
        string? messageId,
        string? messageType,
        string? sourceSystem,
        string? targetSystem,
        string? idempotencyKey,
        string message)
    {
        return new DdsopFeedbackInterpretation(
            contractId ?? string.Empty,
            contractVersion ?? string.Empty,
            messageId ?? string.Empty,
            messageType ?? string.Empty,
            sourceSystem ?? string.Empty,
            targetSystem ?? string.Empty,
            idempotencyKey ?? string.Empty,
            "Rejected",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            0,
            0,
            0,
            message);
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static int CountArray(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.GetArrayLength()
            : 0;
    }

    private static int CountApprovedConfigurationChanges(JsonElement payload)
    {
        if (!payload.TryGetProperty("RecommendedDDSOPReviewTopics", out var topics) || topics.ValueKind != JsonValueKind.Array)
        {
            return 0;
        }

        var count = 0;
        foreach (var topic in topics.EnumerateArray())
        {
            if (topic.TryGetProperty("IsApprovedConfigurationChange", out var flag)
                && flag.ValueKind == JsonValueKind.True)
            {
                count++;
            }
        }

        return count;
    }
}

public sealed class DdsopFeedbackInboundLedger
{
    private readonly DdsopFeedbackOutboundInterpreter _interpreter = new();
    private readonly object _sync = new();
    private readonly List<DdsopFeedbackLedgerRecord> _records = new();
    private readonly Dictionary<string, DdsopFeedbackLedgerRecord> _recordsByIdempotencyKey = new(StringComparer.Ordinal);

    public IReadOnlyList<DdsopFeedbackLedgerRecord> Records
    {
        get
        {
            lock (_sync)
            {
                return _records.ToList();
            }
        }
    }

    public DdsopFeedbackOutboundAck Accept(string rawPayload)
    {
        DdsopFeedbackInterpretation interpretation;
        try
        {
            interpretation = _interpreter.Interpret(rawPayload);
        }
        catch (JsonException ex)
        {
            return BuildRejectedAck(
                "UNKNOWN",
                "UNKNOWN",
                null,
                null,
                null,
                new DdsopContractError(
                    "PAYLOAD_NOT_INTERPRETABLE",
                    "Feedback payload could not be parsed as JSON.",
                    Severity: "Error",
                    Evidence: new Dictionary<string, object?> { ["Exception"] = ex.Message }));
        }

        var idempotencyKey = string.IsNullOrWhiteSpace(interpretation.IdempotencyKey)
            ? "UNKNOWN"
            : interpretation.IdempotencyKey;
        var messageId = string.IsNullOrWhiteSpace(interpretation.MessageID)
            ? "UNKNOWN"
            : interpretation.MessageID;

        lock (_sync)
        {
            if (_recordsByIdempotencyKey.TryGetValue(idempotencyKey, out var existing))
            {
                return new DdsopFeedbackOutboundAck(
                    "DDSOP-FEEDBACK-OUTBOUND-V1",
                    "1.0.0",
                    existing.OriginalMessageID,
                    existing.IdempotencyKey,
                    "Duplicate",
                    FormatReceivedAt(DateTimeOffset.UtcNow),
                    existing.Ack.LinkedOperatingModelConfigurationID,
                    existing.Ack.LinkedOperatingModelFingerprint,
                    existing.Ack.LinkedPlanningRunID,
                    Array.Empty<DdsopContractError>());
            }

            var errors = BuildContractErrors(interpretation);
            var status = errors.Count == 0 ? "Accepted" : "Rejected";
            var ack = new DdsopFeedbackOutboundAck(
                "DDSOP-FEEDBACK-OUTBOUND-V1",
                "1.0.0",
                messageId,
                idempotencyKey,
                status,
                FormatReceivedAt(DateTimeOffset.UtcNow),
                status == "Accepted" ? interpretation.OperatingModelConfigurationID : null,
                status == "Accepted" ? interpretation.OperatingModelFingerprint : null,
                status == "Accepted" ? interpretation.PlanningRunID : null,
                errors);
            var record = new DdsopFeedbackLedgerRecord(
                messageId,
                idempotencyKey,
                status,
                ack.ReceivedAt,
                rawPayload,
                interpretation,
                ack);

            _records.Add(record);
            _recordsByIdempotencyKey[idempotencyKey] = record;
            return ack;
        }
    }

    private static IReadOnlyList<DdsopContractError> BuildContractErrors(DdsopFeedbackInterpretation interpretation)
    {
        var errors = new List<DdsopContractError>();
        if (interpretation.Status == "Rejected")
        {
            if (string.IsNullOrWhiteSpace(interpretation.PlanningRunID))
            {
                errors.Add(new DdsopContractError(
                    "REQUIRED_FIELD_MISSING",
                    "PlanningRunID is required by DDSOP-FEEDBACK-OUTBOUND-V1.",
                    "Payload.PlanningRunID",
                    "Error"));
            }

            if (string.IsNullOrWhiteSpace(interpretation.OperatingModelConfigurationID))
            {
                errors.Add(new DdsopContractError(
                    "REQUIRED_FIELD_MISSING",
                    "OperatingModelConfigurationID is required by DDSOP-FEEDBACK-OUTBOUND-V1.",
                    "Payload.OperatingModelConfigurationID",
                    "Error"));
            }

            if (errors.Count == 0)
            {
                errors.Add(new DdsopContractError(
                    "PAYLOAD_NOT_INTERPRETABLE",
                    interpretation.Message,
                    Severity: "Error"));
            }
        }

        return errors;
    }

    private static DdsopFeedbackOutboundAck BuildRejectedAck(
        string originalMessageId,
        string idempotencyKey,
        string? operatingModelConfigurationId,
        string? operatingModelFingerprint,
        string? planningRunId,
        DdsopContractError error)
    {
        return new DdsopFeedbackOutboundAck(
            "DDSOP-FEEDBACK-OUTBOUND-V1",
            "1.0.0",
            originalMessageId,
            idempotencyKey,
            "Rejected",
            FormatReceivedAt(DateTimeOffset.UtcNow),
            operatingModelConfigurationId,
            operatingModelFingerprint,
            planningRunId,
            new[] { error });
    }

    private static string FormatReceivedAt(DateTimeOffset value)
    {
        return value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture);
    }
}
