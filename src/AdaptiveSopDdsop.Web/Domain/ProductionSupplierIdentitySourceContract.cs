using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AdaptiveSopDdsop.Web.Domain;

public sealed record ProductionSupplierIdentitySourceAck(
    string ContractID,
    string ContractVersion,
    string AckID,
    string MessageID,
    string EvidencePackageID,
    string ConsumerSystem,
    string AckStatus,
    string AckAt,
    IReadOnlyList<ProductionSupplierIdentitySourceAckError> Errors);

public sealed record ProductionSupplierIdentitySourceAckError(
    string ErrorCode,
    string ErrorMessage,
    bool Retryable);

public sealed record ProductionSupplierIdentitySourceInterpretation(
    string ContractID,
    string ContractVersion,
    string MessageID,
    string IdempotencyKey,
    string EvidencePackageID,
    string EvidenceStatus,
    string EvidenceConfidence,
    string SupplierID,
    string ItemID,
    string SupplierSourceRelationID,
    string Status,
    bool AllowsAutomaticMasterSettingUpdate,
    bool RequiresSeparateDdaeApproval,
    bool MutatedDdaeGovernance,
    IReadOnlyList<ProductionSupplierIdentitySourceAckError> Errors,
    string Message);

public sealed record ProductionSupplierIdentitySourceLedgerRecord(
    string MessageID,
    string IdempotencyKey,
    string EvidencePackageID,
    string AckStatus,
    string ReceivedAt,
    string PayloadFingerprint,
    string RawPayload,
    ProductionSupplierIdentitySourceInterpretation Interpretation,
    ProductionSupplierIdentitySourceAck Ack);

public sealed class ProductionSupplierIdentitySourceInboundLedger
{
    public const string ContractId = "PRODUCTION-SUPPLIER-IDENTITY-SOURCE-V1";
    public const string ContractVersion = "1.0.0";
    public const string ConsumerSystem = "DDAE";

    private static readonly HashSet<string> AckStatuses = new(StringComparer.Ordinal)
    {
        "Accepted",
        "Rejected",
        "Duplicate",
        "DeadLettered"
    };

    private static readonly HashSet<string> ErrorCodes = new(StringComparer.Ordinal)
    {
        "MISSING_SOURCE_AUTHORITY",
        "UNKNOWN_SUPPLIER",
        "UNKNOWN_ITEM",
        "UNKNOWN_LOCATION",
        "UNKNOWN_SOURCE_RELATION",
        "INVALID_EFFECTIVE_WINDOW",
        "UNAPPROVED_SOURCE",
        "STALE_VERSION",
        "CONFLICTING_SOURCE",
        "UNSUPPORTED_UOM",
        "CONTRACT_SCOPE_VIOLATION",
        "GOVERNANCE_AUTO_UPDATE_FORBIDDEN",
        "IDEMPOTENCY_CONFLICT"
    };

    private readonly object _sync = new();
    private readonly Func<DateTimeOffset> _clock;
    private readonly Dictionary<string, ProductionSupplierIdentitySourceLedgerRecord> _recordsByIdempotencyKey = new(StringComparer.Ordinal);
    private readonly List<ProductionSupplierIdentitySourceLedgerRecord> _records = new();

    public ProductionSupplierIdentitySourceInboundLedger()
        : this(() => DateTimeOffset.UtcNow)
    {
    }

    public ProductionSupplierIdentitySourceInboundLedger(Func<DateTimeOffset> clock)
    {
        _clock = clock;
    }

    public IReadOnlyList<ProductionSupplierIdentitySourceLedgerRecord> Records
    {
        get
        {
            lock (_sync)
            {
                return _records.ToList();
            }
        }
    }

    public ProductionSupplierIdentitySourceAck Accept(string rawPayload)
    {
        var receivedAt = FormatDateTime(_clock());
        var interpretation = Interpret(rawPayload);
        var fingerprint = ComputePayloadFingerprint(rawPayload);

        lock (_sync)
        {
            if (!string.IsNullOrWhiteSpace(interpretation.IdempotencyKey)
                && _recordsByIdempotencyKey.TryGetValue(interpretation.IdempotencyKey, out var existing))
            {
                if (string.Equals(existing.PayloadFingerprint, fingerprint, StringComparison.Ordinal))
                {
                    return BuildAck(interpretation, "Duplicate", receivedAt, Array.Empty<ProductionSupplierIdentitySourceAckError>());
                }

                var conflictError = Error(
                    "IDEMPOTENCY_CONFLICT",
                    "IdempotencyKey 已存在，但 payload fingerprint 不一致，必须进入死信处理。",
                    retryable: false);
                var conflictInterpretation = interpretation with
                {
                    Status = "DeadLettered",
                    Errors = new[] { conflictError },
                    Message = "重复 IdempotencyKey 与既有 payload 不一致。"
                };
                var conflictAck = BuildAck(conflictInterpretation, "DeadLettered", receivedAt, conflictInterpretation.Errors);
                var conflictRecord = new ProductionSupplierIdentitySourceLedgerRecord(
                    conflictInterpretation.MessageID,
                    conflictInterpretation.IdempotencyKey,
                    conflictInterpretation.EvidencePackageID,
                    conflictAck.AckStatus,
                    receivedAt,
                    fingerprint,
                    rawPayload,
                    conflictInterpretation,
                    conflictAck);
                _records.Add(conflictRecord);
                return conflictAck;
            }

            var ack = BuildAck(interpretation, interpretation.Status, receivedAt, interpretation.Errors);
            var record = new ProductionSupplierIdentitySourceLedgerRecord(
                interpretation.MessageID,
                interpretation.IdempotencyKey,
                interpretation.EvidencePackageID,
                ack.AckStatus,
                receivedAt,
                fingerprint,
                rawPayload,
                interpretation,
                ack);
            _records.Add(record);

            if (!string.IsNullOrWhiteSpace(interpretation.IdempotencyKey))
            {
                _recordsByIdempotencyKey[interpretation.IdempotencyKey] = record;
            }

            return ack;
        }
    }

    public ProductionSupplierIdentitySourceInterpretation Interpret(string rawPayload)
    {
        JsonObject? root;
        try
        {
            root = JsonNode.Parse(rawPayload)?.AsObject();
        }
        catch (JsonException)
        {
            var parseError = Error("CONTRACT_SCOPE_VIOLATION", "payload 不是有效 JSON。", retryable: false);
            return EmptyInterpretation("Rejected", new[] { parseError }, "payload 不是有效 JSON。");
        }

        if (root is null)
        {
            var parseError = Error("CONTRACT_SCOPE_VIOLATION", "payload 必须是 JSON object。", retryable: false);
            return EmptyInterpretation("Rejected", new[] { parseError }, "payload 必须是 JSON object。");
        }

        var payload = root["Payload"]?.AsObject();
        var errors = new List<ProductionSupplierIdentitySourceAckError>();

        var contractId = RequiredString(root, "ContractID", "CONTRACT_SCOPE_VIOLATION", errors);
        var contractVersion = RequiredString(root, "ContractVersion", "CONTRACT_SCOPE_VIOLATION", errors);
        var messageId = RequiredString(root, "MessageID", "CONTRACT_SCOPE_VIOLATION", errors);
        var idempotencyKey = RequiredString(root, "IdempotencyKey", "CONTRACT_SCOPE_VIOLATION", errors);
        RequiredString(root, "ProducerSystem", "CONTRACT_SCOPE_VIOLATION", errors);
        var occurredAt = RequiredString(root, "OccurredAt", "CONTRACT_SCOPE_VIOLATION", errors);
        var timeZone = RequiredString(root, "TimeZone", "CONTRACT_SCOPE_VIOLATION", errors);

        if (!string.Equals(contractId, ContractId, StringComparison.Ordinal))
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", $"ContractID 必须为 {ContractId}。", retryable: false));
        }

        if (!string.Equals(contractVersion, ContractVersion, StringComparison.Ordinal))
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", $"ContractVersion 必须为 {ContractVersion}。", retryable: false));
        }

        if (!HasIsoOffset(occurredAt))
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "OccurredAt 必须是带 UTC offset 的 ISO 时间。", retryable: false));
        }

        if (string.IsNullOrWhiteSpace(timeZone))
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "TimeZone 必须存在。", retryable: false));
        }

        if (payload is null)
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "Payload 必须存在。", retryable: false));
            return EmptyInterpretation("Rejected", errors, "Payload 缺失。") with
            {
                ContractID = contractId,
                ContractVersion = contractVersion,
                MessageID = messageId,
                IdempotencyKey = idempotencyKey
            };
        }

        var evidencePackageId = RequiredString(payload, "EvidencePackageID", "CONTRACT_SCOPE_VIOLATION", errors);
        RequiredString(payload, "EvidenceVersion", "CONTRACT_SCOPE_VIOLATION", errors);
        var evidenceStatus = RequiredString(payload, "EvidenceStatus", "CONTRACT_SCOPE_VIOLATION", errors);
        var evidenceConfidence = RequiredString(payload, "EvidenceConfidence", "CONTRACT_SCOPE_VIOLATION", errors);
        var sourceAuthority = payload["SourceAuthority"]?.AsObject();
        var supplierIdentity = payload["SupplierIdentity"]?.AsObject();
        var supplierSourceRelation = payload["SupplierSourceRelation"]?.AsObject();
        var sourceTerms = payload["SourceTerms"]?.AsObject();
        var governanceBoundary = payload["DDAEGovernanceBoundary"]?.AsObject();

        if (sourceAuthority is null)
        {
            errors.Add(Error("MISSING_SOURCE_AUTHORITY", "SourceAuthority 缺失，无法作为供应商身份来源证据。", retryable: false));
        }
        else
        {
            RequiredString(sourceAuthority, "AuthoritySystemID", "MISSING_SOURCE_AUTHORITY", errors);
            RequiredString(sourceAuthority, "AuthoritySystemType", "MISSING_SOURCE_AUTHORITY", errors);
            RequiredString(sourceAuthority, "AuthorityRecordID", "MISSING_SOURCE_AUTHORITY", errors);
            RequiredString(sourceAuthority, "AuthorityOwner", "MISSING_SOURCE_AUTHORITY", errors);
            RequiredString(sourceAuthority, "AuthorityConfidence", "MISSING_SOURCE_AUTHORITY", errors);
        }

        var supplierId = string.Empty;
        if (supplierIdentity is null)
        {
            errors.Add(Error("UNKNOWN_SUPPLIER", "SupplierIdentity 缺失。", retryable: false));
        }
        else
        {
            supplierId = RequiredString(supplierIdentity, "SupplierID", "UNKNOWN_SUPPLIER", errors);
            RequiredString(supplierIdentity, "SupplierName", "UNKNOWN_SUPPLIER", errors);
            RequiredString(supplierIdentity, "SupplierIdentityStatus", "UNKNOWN_SUPPLIER", errors);
            RequiredString(supplierIdentity, "SupplierIdentityAuthority", "UNKNOWN_SUPPLIER", errors);
            RequiredString(supplierIdentity, "ApprovedSupplierStatus", "UNAPPROVED_SOURCE", errors);
        }

        var itemId = supplierSourceRelation is null
            ? string.Empty
            : RequiredString(supplierSourceRelation, "ItemID", "UNKNOWN_ITEM", errors);
        var relationId = supplierSourceRelation is null
            ? string.Empty
            : RequiredString(supplierSourceRelation, "SupplierSourceRelationID", "UNKNOWN_SOURCE_RELATION", errors);

        if (supplierSourceRelation is null)
        {
            errors.Add(Error("UNKNOWN_SOURCE_RELATION", "SupplierSourceRelation 缺失。", retryable: false));
        }
        else
        {
            RequiredString(supplierSourceRelation, "SupplierID", "UNKNOWN_SUPPLIER", errors);
            RequiredString(supplierSourceRelation, "LocationID", "UNKNOWN_LOCATION", errors);
            RequiredString(supplierSourceRelation, "SourceType", "UNKNOWN_SOURCE_RELATION", errors);
            RequiredString(supplierSourceRelation, "EligibilityStatus", "UNAPPROVED_SOURCE", errors);
            RequiredString(supplierSourceRelation, "EffectiveFrom", "INVALID_EFFECTIVE_WINDOW", errors);
            ValidateEffectiveWindow(supplierSourceRelation, errors);
        }

        if (sourceTerms is null)
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "SourceTerms 缺失。", retryable: false));
        }
        else
        {
            var termsAreProductionAuthoritative = BoolValue(sourceTerms, "TermsAreProductionAuthoritative");
            var planningAssumptionOnly = BoolValue(sourceTerms, "DDAEPlanningAssumptionOnly");
            RequiredString(sourceTerms, "SourceTermsAuthority", "CONTRACT_SCOPE_VIOLATION", errors);
            RequiredString(sourceTerms, "UOM", "UNSUPPORTED_UOM", errors);
            RequiredString(sourceTerms, "UOMAuthority", "UNSUPPORTED_UOM", errors);
            if (termsAreProductionAuthoritative == true && planningAssumptionOnly == true)
            {
                errors.Add(Error(
                    "CONTRACT_SCOPE_VIOLATION",
                    "TermsAreProductionAuthoritative 与 DDAEPlanningAssumptionOnly 不能同时为 true。",
                    retryable: false));
            }
        }

        var allowsAutoUpdate = governanceBoundary is not null && BoolValue(governanceBoundary, "AllowsAutomaticMasterSettingUpdate") == true;
        var requiresSeparateApproval = governanceBoundary is not null && BoolValue(governanceBoundary, "RequiresSeparateDDAEApproval") == true;
        if (governanceBoundary is null)
        {
            errors.Add(Error("GOVERNANCE_AUTO_UPDATE_FORBIDDEN", "DDAEGovernanceBoundary 缺失。", retryable: false));
        }
        else
        {
            if (allowsAutoUpdate)
            {
                errors.Add(Error(
                    "GOVERNANCE_AUTO_UPDATE_FORBIDDEN",
                    "DDAE 不允许由该 contract 自动更新主设置或运行模型。",
                    retryable: false));
            }

            if (!requiresSeparateApproval)
            {
                errors.Add(Error(
                    "GOVERNANCE_AUTO_UPDATE_FORBIDDEN",
                    "RequiresSeparateDDAEApproval 必须为 true。",
                    retryable: false));
            }
        }

        if (string.Equals(evidenceConfidence, "ProductionValidatedReserved", StringComparison.Ordinal))
        {
            errors.Add(Error(
                "CONTRACT_SCOPE_VIOLATION",
                "ProductionValidatedReserved 在本 Reviewed Draft 中保留，DDAE 不得接受为生产验证。",
                retryable: false));
        }

        if (payload["NonClaims"] is not JsonArray nonClaims || nonClaims.Count == 0)
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "NonClaims 必须存在，用于限定非声明范围。", retryable: false));
        }

        var status = DetermineStatus(errors);
        var message = status switch
        {
            "Accepted" => "供应商身份来源证据已按治理解释接收；未更新 DDAE 主设置。",
            "DeadLettered" => "供应商身份来源证据存在死信级问题；未更新 DDAE 主设置。",
            _ => "供应商身份来源证据被拒绝；未更新 DDAE 主设置。"
        };

        return new ProductionSupplierIdentitySourceInterpretation(
            contractId,
            contractVersion,
            messageId,
            idempotencyKey,
            evidencePackageId,
            evidenceStatus,
            evidenceConfidence,
            supplierId,
            itemId,
            relationId,
            status,
            allowsAutoUpdate,
            requiresSeparateApproval,
            MutatedDdaeGovernance: false,
            errors,
            message);
    }

    public static void ValidateAckShape(ProductionSupplierIdentitySourceAck ack)
    {
        if (!string.Equals(ack.ContractID, ContractId, StringComparison.Ordinal)
            || !string.Equals(ack.ContractVersion, ContractVersion, StringComparison.Ordinal)
            || !AckStatuses.Contains(ack.AckStatus)
            || string.IsNullOrWhiteSpace(ack.AckID)
            || string.IsNullOrWhiteSpace(ack.MessageID)
            || string.IsNullOrWhiteSpace(ack.EvidencePackageID)
            || string.IsNullOrWhiteSpace(ack.ConsumerSystem)
            || !HasIsoOffset(ack.AckAt))
        {
            throw new InvalidOperationException("ACK shape does not satisfy PRODUCTION-SUPPLIER-IDENTITY-SOURCE-V1 ACK contract.");
        }

        foreach (var error in ack.Errors)
        {
            if (!ErrorCodes.Contains(error.ErrorCode) || string.IsNullOrWhiteSpace(error.ErrorMessage))
            {
                throw new InvalidOperationException($"ACK error is not contract-shaped: {error.ErrorCode}");
            }
        }
    }

    private static string DetermineStatus(IReadOnlyCollection<ProductionSupplierIdentitySourceAckError> errors)
    {
        if (errors.Count == 0)
        {
            return "Accepted";
        }

        return errors.Any(item => item.ErrorCode is "MISSING_SOURCE_AUTHORITY" or "INVALID_EFFECTIVE_WINDOW" or "IDEMPOTENCY_CONFLICT")
            ? "DeadLettered"
            : "Rejected";
    }

    private static void ValidateEffectiveWindow(JsonObject supplierSourceRelation, ICollection<ProductionSupplierIdentitySourceAckError> errors)
    {
        var from = StringValue(supplierSourceRelation, "EffectiveFrom");
        var to = StringValue(supplierSourceRelation, "EffectiveTo");
        if (!HasIsoOffset(from))
        {
            errors.Add(Error("INVALID_EFFECTIVE_WINDOW", "EffectiveFrom 必须是带 UTC offset 的 ISO 时间。", retryable: false));
            return;
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            return;
        }

        if (!DateTimeOffset.TryParse(from, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromTime)
            || !DateTimeOffset.TryParse(to, CultureInfo.InvariantCulture, DateTimeStyles.None, out var toTime)
            || toTime < fromTime)
        {
            errors.Add(Error("INVALID_EFFECTIVE_WINDOW", "EffectiveTo 必须晚于或等于 EffectiveFrom。", retryable: false));
        }
    }

    private static ProductionSupplierIdentitySourceAck BuildAck(
        ProductionSupplierIdentitySourceInterpretation interpretation,
        string status,
        string ackAt,
        IReadOnlyList<ProductionSupplierIdentitySourceAckError> errors)
    {
        var ack = new ProductionSupplierIdentitySourceAck(
            ContractId,
            ContractVersion,
            $"DDAE-ACK-{Slug(interpretation.MessageID)}-{status.ToUpperInvariant()}",
            string.IsNullOrWhiteSpace(interpretation.MessageID) ? "UNKNOWN-MESSAGE" : interpretation.MessageID,
            string.IsNullOrWhiteSpace(interpretation.EvidencePackageID) ? "UNKNOWN-EVIDENCE-PACKAGE" : interpretation.EvidencePackageID,
            ConsumerSystem,
            status,
            ackAt,
            errors);
        ValidateAckShape(ack);
        return ack;
    }

    private static ProductionSupplierIdentitySourceInterpretation EmptyInterpretation(
        string status,
        IReadOnlyList<ProductionSupplierIdentitySourceAckError> errors,
        string message)
    {
        return new ProductionSupplierIdentitySourceInterpretation(
            ContractId,
            ContractVersion,
            "UNKNOWN-MESSAGE",
            string.Empty,
            "UNKNOWN-EVIDENCE-PACKAGE",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            status,
            AllowsAutomaticMasterSettingUpdate: false,
            RequiresSeparateDdaeApproval: true,
            MutatedDdaeGovernance: false,
            errors,
            message);
    }

    private static string RequiredString(JsonObject node, string propertyName, string errorCode, ICollection<ProductionSupplierIdentitySourceAckError> errors)
    {
        var value = StringValue(node, propertyName);
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(Error(errorCode, $"{propertyName} 缺失。", retryable: false));
        }

        return value;
    }

    private static string StringValue(JsonObject node, string propertyName)
    {
        return node[propertyName]?.GetValue<string>() ?? string.Empty;
    }

    private static bool? BoolValue(JsonObject node, string propertyName)
    {
        var value = node[propertyName];
        return value is null ? null : value.GetValue<bool>();
    }

    private static ProductionSupplierIdentitySourceAckError Error(string code, string message, bool retryable)
    {
        return new ProductionSupplierIdentitySourceAckError(code, message, retryable);
    }

    private static string ComputePayloadFingerprint(string rawPayload)
    {
        JsonNode? node;
        try
        {
            node = JsonNode.Parse(rawPayload);
        }
        catch (JsonException)
        {
            node = JsonValue.Create(rawPayload);
        }

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
               {
                   Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                   Indented = false
               }))
        {
            WriteCanonicalJson(writer, node);
        }

        var hash = SHA256.HashData(stream.ToArray());
        return $"sha256:{Convert.ToHexString(hash).ToLowerInvariant()}";
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
            writer.WriteRawValue(decimalValue.ToString("0.#############################", CultureInfo.InvariantCulture), skipInputValidation: true);
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

    private static bool HasIsoOffset(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
            && (value.Contains('+', StringComparison.Ordinal) || value.EndsWith('Z') || value.LastIndexOf('-') > "yyyy-MM-dd".Length);
    }

    private static string FormatDateTime(DateTimeOffset value)
    {
        return value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
    }

    private static string Slug(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "UNKNOWN";
        }

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value.ToUpperInvariant())
        {
            builder.Append(char.IsLetterOrDigit(ch) ? ch : '-');
        }

        return builder.ToString().Trim('-');
    }
}
