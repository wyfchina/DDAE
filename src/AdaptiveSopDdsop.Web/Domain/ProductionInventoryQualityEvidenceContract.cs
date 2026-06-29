using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AdaptiveSopDdsop.Web.Domain;

public sealed record ProductionInventoryQualityAck(
    string ContractID,
    string ContractVersion,
    string AckID,
    string MessageID,
    string EvidencePackageID,
    string ConsumerSystem,
    string AckStatus,
    string AckAt,
    IReadOnlyList<ProductionInventoryQualityAckError> Errors);

public sealed record ProductionInventoryQualityAckError(
    string ErrorCode,
    string ErrorMessage,
    bool Retryable);

public sealed record ProductionInventoryQualityInterpretation(
    string ContractID,
    string ContractVersion,
    string MessageID,
    string IdempotencyKey,
    string EvidencePackageID,
    string EvidenceStatus,
    string EvidenceConfidence,
    string ItemID,
    string LocationID,
    string QuantityUom,
    string Status,
    bool AllowsAutomaticMasterSettingUpdate,
    bool RequiresSeparateDdaeApproval,
    bool MutatedDdaeGovernance,
    bool IsProductionValidated,
    IReadOnlyList<ProductionInventoryQualityAckError> Errors,
    string Message);

public sealed record ProductionInventoryQualityLedgerRecord(
    string MessageID,
    string IdempotencyKey,
    string EvidencePackageID,
    string AckStatus,
    string ReceivedAt,
    string PayloadFingerprint,
    string RawPayload,
    ProductionInventoryQualityInterpretation Interpretation,
    ProductionInventoryQualityAck Ack);

public sealed class ProductionInventoryQualityInboundLedger
{
    public const string ContractId = "PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1";
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
        "MISSING_INVENTORY_AUTHORITY",
        "MISSING_QUALITY_AUTHORITY",
        "UNKNOWN_ITEM",
        "UNKNOWN_LOCATION",
        "UNKNOWN_LOT_OR_SERIAL",
        "UNSUPPORTED_UOM",
        "INVALID_QUANTITY",
        "INVALID_TIMESTAMP",
        "STALE_SNAPSHOT",
        "CONFLICTING_MOVEMENT",
        "INVALID_STATE_TRANSITION",
        "MOVEMENT_REVERSAL_TARGET_NOT_FOUND",
        "SUPERSESSION_TARGET_NOT_FOUND",
        "CONTRACT_SCOPE_VIOLATION",
        "GOVERNANCE_AUTO_UPDATE_FORBIDDEN",
        "IDEMPOTENCY_CONFLICT"
    };

    private static readonly HashSet<string> LifecycleStates = new(StringComparer.Ordinal)
    {
        "Candidate",
        "Reviewed",
        "SourceAuthoritative",
        "Corrected",
        "Superseded",
        "Rejected",
        "DeadLettered"
    };

    private static readonly HashSet<string> EvidenceConfidences = new(StringComparer.Ordinal)
    {
        "Controlled",
        "Reviewed",
        "SourceAuthoritative",
        "ProductionValidatedReserved"
    };

    private static readonly HashSet<string> MovementTypes = new(StringComparer.Ordinal)
    {
        "Receipt",
        "Transfer",
        "InspectionReceipt",
        "InspectionRelease",
        "Quarantine",
        "Rejection",
        "Adjustment",
        "Reversal",
        "Correction",
        "SnapshotReplacement"
    };

    private static readonly HashSet<string> OperationalStates = new(StringComparer.Ordinal)
    {
        "Received",
        "InInspection",
        "Quarantine",
        "QualityReleased",
        "Rejected",
        "Available",
        "Allocated",
        "Blocked",
        "Transferred",
        "Adjusted",
        "Reversed"
    };

    private static readonly HashSet<string> KnownItems = new(StringComparer.Ordinal)
    {
        "PART-FPGA-SPACE"
    };

    private static readonly HashSet<string> KnownLocations = new(StringComparer.Ordinal)
    {
        "WH-ELEC-QA",
        "WH-IQC"
    };

    private static readonly HashSet<string> SupportedUoms = new(StringComparer.Ordinal)
    {
        "EA"
    };

    private static readonly string[] OutOfScopeMarkers =
    {
        "SupplierSourceApproval",
        "SupplierExecution",
        "DeliveryPerformance",
        "LeadTimePerformance",
        "SupplierCapacity",
        "WorkOrder",
        "Routing",
        "Operation",
        "MaterialIssue",
        "MaterialConsumption",
        "ProductionValidated",
        "BusinessGoldenLoopReady"
    };

    private readonly object _sync = new();
    private readonly Func<DateTimeOffset> _clock;
    private readonly Dictionary<string, ProductionInventoryQualityLedgerRecord> _recordsByIdempotencyKey = new(StringComparer.Ordinal);
    private readonly List<ProductionInventoryQualityLedgerRecord> _records = new();

    public ProductionInventoryQualityInboundLedger()
        : this(() => DateTimeOffset.UtcNow)
    {
    }

    public ProductionInventoryQualityInboundLedger(Func<DateTimeOffset> clock)
    {
        _clock = clock;
    }

    public IReadOnlyList<ProductionInventoryQualityLedgerRecord> Records
    {
        get
        {
            lock (_sync)
            {
                return _records.ToList();
            }
        }
    }

    public ProductionInventoryQualityAck Accept(string rawPayload)
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
                    return BuildAck(interpretation, "Duplicate", receivedAt, Array.Empty<ProductionInventoryQualityAckError>());
                }

                var conflictError = Error(
                    "IDEMPOTENCY_CONFLICT",
                    "同一 IdempotencyKey 已存在，但 payload fingerprint 不一致，必须进入死信处理。",
                    retryable: false);
                var conflictInterpretation = interpretation with
                {
                    Status = "DeadLettered",
                    Errors = new[] { conflictError },
                    Message = "重复 IdempotencyKey 与既有 payload 不一致。"
                };
                var conflictAck = BuildAck(conflictInterpretation, "DeadLettered", receivedAt, conflictInterpretation.Errors);
                _records.Add(new ProductionInventoryQualityLedgerRecord(
                    conflictInterpretation.MessageID,
                    conflictInterpretation.IdempotencyKey,
                    conflictInterpretation.EvidencePackageID,
                    conflictAck.AckStatus,
                    receivedAt,
                    fingerprint,
                    rawPayload,
                    conflictInterpretation,
                    conflictAck));
                return conflictAck;
            }

            var ack = BuildAck(interpretation, interpretation.Status, receivedAt, interpretation.Errors);
            var record = new ProductionInventoryQualityLedgerRecord(
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

    public ProductionInventoryQualityInterpretation Interpret(string rawPayload)
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
        var errors = new List<ProductionInventoryQualityAckError>();

        var contractId = RequiredString(root, "ContractID", "CONTRACT_SCOPE_VIOLATION", errors);
        var contractVersion = RequiredString(root, "ContractVersion", "CONTRACT_SCOPE_VIOLATION", errors);
        var messageId = RequiredString(root, "MessageID", "CONTRACT_SCOPE_VIOLATION", errors);
        var idempotencyKey = RequiredString(root, "IdempotencyKey", "CONTRACT_SCOPE_VIOLATION", errors);
        RequiredString(root, "ProducerSystem", "CONTRACT_SCOPE_VIOLATION", errors);
        ValidateConsumerSystems(root, errors);
        var occurredAt = RequiredString(root, "OccurredAt", "INVALID_TIMESTAMP", errors);
        var timeZone = RequiredString(root, "TimeZone", "INVALID_TIMESTAMP", errors);

        if (!string.Equals(contractId, ContractId, StringComparison.Ordinal))
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", $"ContractID 必须为 {ContractId}。", retryable: false));
        }

        if (!string.Equals(contractVersion, ContractVersion, StringComparison.Ordinal))
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", $"ContractVersion 必须为 {ContractVersion}。", retryable: false));
        }

        ValidateOffsetTimestamp(occurredAt, "OccurredAt", errors);
        if (string.IsNullOrWhiteSpace(timeZone))
        {
            errors.Add(Error("INVALID_TIMESTAMP", "TimeZone 必须存在。", retryable: false));
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
        if (!LifecycleStates.Contains(evidenceStatus))
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "EvidenceStatus 不属于契约允许状态。", retryable: false));
        }

        if (!EvidenceConfidences.Contains(evidenceConfidence))
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "EvidenceConfidence 不属于契约允许状态。", retryable: false));
        }

        if (string.Equals(evidenceConfidence, "ProductionValidatedReserved", StringComparison.Ordinal))
        {
            errors.Add(Error(
                "CONTRACT_SCOPE_VIOLATION",
                "ProductionValidatedReserved 在本 Reviewed Draft 中保留，DDAE 不得接受为生产验证。",
                retryable: false));
        }

        var inventoryAuthority = payload["InventoryAuthority"]?.AsObject();
        var qualityAuthority = payload["QualityAuthority"]?.AsObject();
        ValidateAuthority(inventoryAuthority, "InventoryAuthority", "MISSING_INVENTORY_AUTHORITY", errors);
        ValidateAuthority(qualityAuthority, "QualityAuthority", "MISSING_QUALITY_AUTHORITY", errors);

        var itemLocation = payload["ItemLocation"]?.AsObject();
        var inventorySnapshot = payload["InventorySnapshot"]?.AsObject();
        var stockMovements = payload["StockMovements"] as JsonArray;
        var qualityEvidence = payload["QualityEvidence"]?.AsObject();
        var governanceBoundary = payload["DDAEGovernanceBoundary"]?.AsObject();
        var traceability = payload["Traceability"]?.AsObject();
        var supersession = payload["Supersession"]?.AsObject();

        var itemId = string.Empty;
        var locationId = string.Empty;
        var quantityUom = string.Empty;
        if (itemLocation is null)
        {
            errors.Add(Error("UNKNOWN_ITEM", "ItemLocation 缺失。", retryable: false));
        }
        else
        {
            itemId = RequiredString(itemLocation, "ItemID", "UNKNOWN_ITEM", errors);
            locationId = RequiredString(itemLocation, "LocationID", "UNKNOWN_LOCATION", errors);
            quantityUom = RequiredString(itemLocation, "QuantityUOM", "UNSUPPORTED_UOM", errors);
            RequiredString(itemLocation, "UOMAuthority", "UNSUPPORTED_UOM", errors);
            RequiredString(itemLocation, "ItemLocationStatus", "CONTRACT_SCOPE_VIOLATION", errors);
            if (!string.IsNullOrWhiteSpace(itemId) && !KnownItems.Contains(itemId))
            {
                errors.Add(Error("UNKNOWN_ITEM", $"ItemID 无法按 DDAE 控制 fixture 解析：{itemId}。", retryable: true));
            }

            if (!string.IsNullOrWhiteSpace(locationId) && !KnownLocations.Contains(locationId))
            {
                errors.Add(Error("UNKNOWN_LOCATION", $"LocationID 无法按 DDAE 控制 fixture 解析：{locationId}。", retryable: true));
            }

            if (!string.IsNullOrWhiteSpace(quantityUom) && !SupportedUoms.Contains(quantityUom))
            {
                errors.Add(Error("UNSUPPORTED_UOM", $"QuantityUOM 不受支持：{quantityUom}。", retryable: true));
            }
        }

        if (inventorySnapshot is not null)
        {
            if (inventoryAuthority is null)
            {
                errors.Add(Error("MISSING_INVENTORY_AUTHORITY", "InventorySnapshot 存在时 InventoryAuthority 必须存在。", retryable: true));
            }

            ValidateInventorySnapshot(inventorySnapshot, errors);
        }

        ValidateStockMovements(stockMovements, inventoryAuthority, qualityAuthority, errors);
        ValidateQualityEvidence(qualityEvidence, qualityAuthority, errors);
        ValidateGovernanceBoundary(governanceBoundary, errors);
        ValidateTraceability(traceability, errors);
        ValidateSupersession(evidenceStatus, supersession, errors);
        ValidateNonClaims(payload["NonClaims"], errors);
        ValidateScopeMarkers(payload, errors);

        var allowsAutoUpdate = governanceBoundary is not null && BoolValue(governanceBoundary, "AllowsAutomaticMasterSettingUpdate") == true;
        var requiresSeparateApproval = governanceBoundary is not null && BoolValue(governanceBoundary, "RequiresSeparateDDAEApproval") == true;
        var status = DetermineStatus(errors);
        var message = status switch
        {
            "Accepted" => "库存/质量证据已按治理解释接收；未更新 DDAE 主设置、缓冲、供应来源或运行模型。",
            "DeadLettered" => "库存/质量证据存在死信级问题；未更新 DDAE 主设置、缓冲、供应来源或运行模型。",
            _ => "库存/质量证据被拒绝；未更新 DDAE 主设置、缓冲、供应来源或运行模型。"
        };

        return new ProductionInventoryQualityInterpretation(
            contractId,
            contractVersion,
            messageId,
            idempotencyKey,
            evidencePackageId,
            evidenceStatus,
            evidenceConfidence,
            itemId,
            locationId,
            quantityUom,
            status,
            allowsAutoUpdate,
            requiresSeparateApproval,
            MutatedDdaeGovernance: false,
            IsProductionValidated: false,
            errors,
            message);
    }

    public static void ValidateAckShape(ProductionInventoryQualityAck ack)
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
            throw new InvalidOperationException("ACK shape does not satisfy PRODUCTION-INVENTORY-QUALITY-EVIDENCE-V1 ACK contract.");
        }

        foreach (var error in ack.Errors)
        {
            if (!ErrorCodes.Contains(error.ErrorCode) || string.IsNullOrWhiteSpace(error.ErrorMessage))
            {
                throw new InvalidOperationException($"ACK error is not contract-shaped: {error.ErrorCode}");
            }
        }
    }

    private static void ValidateConsumerSystems(JsonObject root, ICollection<ProductionInventoryQualityAckError> errors)
    {
        if (root["ConsumerSystems"] is not JsonArray consumers || consumers.Count == 0)
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "ConsumerSystems 必须存在且非空。", retryable: false));
        }
    }

    private static void ValidateAuthority(JsonObject? authority, string name, string errorCode, ICollection<ProductionInventoryQualityAckError> errors)
    {
        if (authority is null)
        {
            return;
        }

        RequiredString(authority, "AuthoritySystemID", errorCode, errors);
        RequiredString(authority, "AuthoritySystemType", errorCode, errors);
        RequiredString(authority, "AuthorityRecordID", errorCode, errors);
        RequiredString(authority, "AuthorityOwner", errorCode, errors);
        RequiredString(authority, "AuthorityConfidence", errorCode, errors);
        if (string.IsNullOrWhiteSpace(StringValue(authority, "AuthorityConfidence")))
        {
            errors.Add(Error(errorCode, $"{name}.AuthorityConfidence 缺失。", retryable: false));
        }
    }

    private static void ValidateInventorySnapshot(JsonObject snapshot, ICollection<ProductionInventoryQualityAckError> errors)
    {
        RequiredString(snapshot, "SnapshotID", "CONTRACT_SCOPE_VIOLATION", errors);
        ValidateOffsetTimestamp(RequiredString(snapshot, "SnapshotTimestamp", "INVALID_TIMESTAMP", errors), "SnapshotTimestamp", errors);

        var quantityFields = new[]
        {
            "OnHandQty",
            "AvailableQty",
            "AllocatedQty",
            "AvailableAfterAllocationQty",
            "InboundQty",
            "InspectionQty",
            "QuarantineQty",
            "QualityReleasedQty",
            "RejectedQty",
            "BlockedQty",
            "ReservedQty"
        };
        foreach (var field in quantityFields)
        {
            var value = NumberValue(snapshot, field);
            if (value is null || value < 0)
            {
                errors.Add(Error("INVALID_QUANTITY", $"{field} 必须为非负数。", retryable: false));
            }
        }

        if (NumberValue(snapshot, "AvailableAfterAllocationQty") is { } availableAfterAllocation
            && NumberValue(snapshot, "AvailableQty") is { } available
            && NumberValue(snapshot, "AllocatedQty") is { } allocated
            && availableAfterAllocation > available)
        {
            errors.Add(Error("INVALID_QUANTITY", "AvailableAfterAllocationQty 不能大于 AvailableQty。", retryable: false));
        }

        var timestamp = StringValue(snapshot, "SnapshotTimestamp");
        if (DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.None, out var snapshotTime)
            && snapshotTime < new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        {
            errors.Add(Error("STALE_SNAPSHOT", "SnapshotTimestamp 早于当前 contract fixture 允许窗口。", retryable: true));
        }
    }

    private static void ValidateStockMovements(
        JsonArray? stockMovements,
        JsonObject? inventoryAuthority,
        JsonObject? qualityAuthority,
        ICollection<ProductionInventoryQualityAckError> errors)
    {
        if (stockMovements is null)
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "StockMovements 必须存在。", retryable: false));
            return;
        }

        var movementIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var movementNode in stockMovements.OfType<JsonObject>())
        {
            if (inventoryAuthority is null)
            {
                errors.Add(Error("MISSING_INVENTORY_AUTHORITY", "StockMovements 存在时 InventoryAuthority 必须存在。", retryable: true));
            }

            var movementId = RequiredString(movementNode, "MovementID", "CONFLICTING_MOVEMENT", errors);
            if (!string.IsNullOrWhiteSpace(movementId) && !movementIds.Add(movementId))
            {
                errors.Add(Error("CONFLICTING_MOVEMENT", $"MovementID 在同一 payload 中重复：{movementId}。", retryable: false));
            }

            var movementType = RequiredString(movementNode, "MovementType", "INVALID_STATE_TRANSITION", errors);
            var movementState = RequiredString(movementNode, "MovementState", "INVALID_STATE_TRANSITION", errors);
            if (!MovementTypes.Contains(movementType))
            {
                errors.Add(Error("INVALID_STATE_TRANSITION", $"MovementType 不属于契约允许值：{movementType}。", retryable: false));
            }

            if (!OperationalStates.Contains(movementState))
            {
                errors.Add(Error("INVALID_STATE_TRANSITION", $"MovementState 不属于契约允许值：{movementState}。", retryable: false));
            }

            if (NumberValue(movementNode, "MovementQty") is not { } movementQty || movementQty < 0)
            {
                errors.Add(Error("INVALID_QUANTITY", "MovementQty 必须为非负数。", retryable: false));
            }

            var movementUom = RequiredString(movementNode, "MovementUOM", "UNSUPPORTED_UOM", errors);
            if (!string.IsNullOrWhiteSpace(movementUom) && !SupportedUoms.Contains(movementUom))
            {
                errors.Add(Error("UNSUPPORTED_UOM", $"MovementUOM 不受支持：{movementUom}。", retryable: true));
            }

            ValidateOffsetTimestamp(RequiredString(movementNode, "MovementTimestamp", "INVALID_TIMESTAMP", errors), "MovementTimestamp", errors);

            if (string.Equals(movementType, "InspectionRelease", StringComparison.Ordinal))
            {
                if (qualityAuthority is null)
                {
                    errors.Add(Error("MISSING_QUALITY_AUTHORITY", "MovementType = InspectionRelease 时 QualityAuthority 必须存在。", retryable: true));
                }

                if (!string.Equals(movementState, "QualityReleased", StringComparison.Ordinal))
                {
                    errors.Add(Error("INVALID_STATE_TRANSITION", "InspectionRelease 必须对应 QualityReleased 状态。", retryable: false));
                }
            }

            if (string.Equals(movementType, "Reversal", StringComparison.Ordinal)
                && string.IsNullOrWhiteSpace(StringValue(movementNode, "ReversesMovementID")))
            {
                errors.Add(Error("MOVEMENT_REVERSAL_TARGET_NOT_FOUND", "Reversal 必须引用 ReversesMovementID。", retryable: true));
            }
        }
    }

    private static void ValidateQualityEvidence(JsonObject? qualityEvidence, JsonObject? qualityAuthority, ICollection<ProductionInventoryQualityAckError> errors)
    {
        if (qualityEvidence is null)
        {
            return;
        }

        var qualityReleaseStatus = RequiredString(qualityEvidence, "QualityReleaseStatus", "MISSING_QUALITY_AUTHORITY", errors);
        var inspectionStatus = RequiredString(qualityEvidence, "InspectionStatus", "MISSING_QUALITY_AUTHORITY", errors);
        if (string.Equals(qualityReleaseStatus, "QualityReleased", StringComparison.Ordinal))
        {
            if (qualityAuthority is null)
            {
                errors.Add(Error("MISSING_QUALITY_AUTHORITY", "QualityReleased 证据必须存在 QualityAuthority。", retryable: true));
            }

            ValidateOffsetTimestamp(RequiredString(qualityEvidence, "QualityReleasedAt", "INVALID_TIMESTAMP", errors), "QualityReleasedAt", errors);
            RequiredString(qualityEvidence, "QualityReleaseID", "MISSING_QUALITY_AUTHORITY", errors);
        }

        if (string.Equals(inspectionStatus, "Rejected", StringComparison.Ordinal))
        {
            if (qualityAuthority is null)
            {
                errors.Add(Error("MISSING_QUALITY_AUTHORITY", "Rejected 质量证据必须存在 QualityAuthority。", retryable: true));
            }

            ValidateOffsetTimestamp(StringValue(qualityEvidence, "RejectedAt"), "RejectedAt", errors);
        }
    }

    private static void ValidateGovernanceBoundary(JsonObject? governanceBoundary, ICollection<ProductionInventoryQualityAckError> errors)
    {
        if (governanceBoundary is null)
        {
            errors.Add(Error("GOVERNANCE_AUTO_UPDATE_FORBIDDEN", "DDAEGovernanceBoundary 缺失。", retryable: false));
            return;
        }

        if (BoolValue(governanceBoundary, "AllowsAutomaticMasterSettingUpdate") != false)
        {
            errors.Add(Error("GOVERNANCE_AUTO_UPDATE_FORBIDDEN", "AllowsAutomaticMasterSettingUpdate 必须为 false。", retryable: false));
        }

        if (BoolValue(governanceBoundary, "RequiresSeparateDDAEApproval") != true)
        {
            errors.Add(Error("GOVERNANCE_AUTO_UPDATE_FORBIDDEN", "RequiresSeparateDDAEApproval 必须为 true。", retryable: false));
        }
    }

    private static void ValidateTraceability(JsonObject? traceability, ICollection<ProductionInventoryQualityAckError> errors)
    {
        if (traceability is null)
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "Traceability 缺失。", retryable: false));
            return;
        }

        RequiredString(traceability, "SourceSystem", "CONTRACT_SCOPE_VIOLATION", errors);
        RequiredString(traceability, "SourceDocumentType", "CONTRACT_SCOPE_VIOLATION", errors);
        RequiredString(traceability, "SourceDocumentID", "CONTRACT_SCOPE_VIOLATION", errors);
        RequiredString(traceability, "SourceRecordID", "CONTRACT_SCOPE_VIOLATION", errors);
        RequiredString(traceability, "TraceableID", "CONTRACT_SCOPE_VIOLATION", errors);
        ValidateOffsetTimestamp(RequiredString(traceability, "RegisteredAt", "INVALID_TIMESTAMP", errors), "RegisteredAt", errors);
    }

    private static void ValidateSupersession(string evidenceStatus, JsonObject? supersession, ICollection<ProductionInventoryQualityAckError> errors)
    {
        if (supersession is null)
        {
            errors.Add(Error("SUPERSESSION_TARGET_NOT_FOUND", "Supersession 缺失。", retryable: true));
            return;
        }

        if (evidenceStatus is "Corrected" or "Superseded")
        {
            if (string.IsNullOrWhiteSpace(StringValue(supersession, "SupersedesEvidencePackageID"))
                || string.IsNullOrWhiteSpace(StringValue(supersession, "SupersedesEvidenceVersion")))
            {
                errors.Add(Error("SUPERSESSION_TARGET_NOT_FOUND", "Corrected/Superseded 必须引用前序 EvidencePackageID 与 EvidenceVersion。", retryable: true));
            }
        }
    }

    private static void ValidateNonClaims(JsonNode? nonClaimsNode, ICollection<ProductionInventoryQualityAckError> errors)
    {
        if (nonClaimsNode is not JsonArray nonClaims || nonClaims.Count == 0)
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "NonClaims 必须存在，用于限定非声明范围。", retryable: false));
            return;
        }

        if (nonClaims.Any(item => item?.GetValue<string>().Contains("NoAutomaticDDAEMasterSettingUpdate", StringComparison.OrdinalIgnoreCase) == true) == false)
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "NonClaims 必须保留 NoAutomaticDDAEMasterSettingUpdate。", retryable: false));
        }
    }

    private static void ValidateScopeMarkers(JsonObject payload, ICollection<ProductionInventoryQualityAckError> errors)
    {
        foreach (var marker in OutOfScopeMarkers)
        {
            if (payload.ContainsKey(marker))
            {
                errors.Add(Error("CONTRACT_SCOPE_VIOLATION", $"payload 包含契约外声明字段：{marker}。", retryable: false));
            }
        }
    }

    private static string DetermineStatus(IReadOnlyCollection<ProductionInventoryQualityAckError> errors)
    {
        if (errors.Count == 0)
        {
            return "Accepted";
        }

        return errors.Any(item => item.ErrorCode is "IDEMPOTENCY_CONFLICT" or "CONFLICTING_MOVEMENT" or "MOVEMENT_REVERSAL_TARGET_NOT_FOUND" or "SUPERSESSION_TARGET_NOT_FOUND")
            ? "DeadLettered"
            : "Rejected";
    }

    private static ProductionInventoryQualityAck BuildAck(
        ProductionInventoryQualityInterpretation interpretation,
        string status,
        string ackAt,
        IReadOnlyList<ProductionInventoryQualityAckError> errors)
    {
        var ack = new ProductionInventoryQualityAck(
            ContractId,
            ContractVersion,
            $"DDAE-PIQE-ACK-{Slug(interpretation.MessageID)}-{status.ToUpperInvariant()}",
            string.IsNullOrWhiteSpace(interpretation.MessageID) ? "UNKNOWN-MESSAGE" : interpretation.MessageID,
            string.IsNullOrWhiteSpace(interpretation.EvidencePackageID) ? "UNKNOWN-EVIDENCE-PACKAGE" : interpretation.EvidencePackageID,
            ConsumerSystem,
            status,
            ackAt,
            errors);
        ValidateAckShape(ack);
        return ack;
    }

    private static ProductionInventoryQualityInterpretation EmptyInterpretation(
        string status,
        IReadOnlyList<ProductionInventoryQualityAckError> errors,
        string message)
    {
        return new ProductionInventoryQualityInterpretation(
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
            IsProductionValidated: false,
            errors,
            message);
    }

    private static string RequiredString(JsonObject node, string propertyName, string errorCode, ICollection<ProductionInventoryQualityAckError> errors)
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
        var value = node[propertyName];
        return value is null || value.GetValueKind() == JsonValueKind.Null
            ? string.Empty
            : value.GetValue<string>() ?? string.Empty;
    }

    private static decimal? NumberValue(JsonObject node, string propertyName)
    {
        var value = node[propertyName];
        if (value is null || value.GetValueKind() == JsonValueKind.Null)
        {
            return null;
        }

        return value.GetValue<decimal>();
    }

    private static bool? BoolValue(JsonObject node, string propertyName)
    {
        var value = node[propertyName];
        return value is null || value.GetValueKind() == JsonValueKind.Null ? null : value.GetValue<bool>();
    }

    private static ProductionInventoryQualityAckError Error(string code, string message, bool retryable)
    {
        return new ProductionInventoryQualityAckError(code, message, retryable);
    }

    private static void ValidateOffsetTimestamp(string? value, string fieldName, ICollection<ProductionInventoryQualityAckError> errors)
    {
        if (!HasIsoOffset(value))
        {
            errors.Add(Error("INVALID_TIMESTAMP", $"{fieldName} 必须是带 UTC offset 的 ISO 时间。", retryable: false));
        }
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
