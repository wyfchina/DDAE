using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AdaptiveSopDdsop.Web.Domain;

public sealed record SdbrExecutionObjectEvidenceAck(
    string ContractID,
    string ContractVersion,
    string AckID,
    string MessageID,
    string IdempotencyKey,
    string ConsumerSystem,
    string AckStatus,
    string? ErrorCode,
    string? ErrorMessage,
    string ReceivedAt,
    bool Retryable,
    string TraceableID);

public sealed record SdbrExecutionObjectEvidenceAckError(
    string ErrorCode,
    string ErrorMessage,
    bool Retryable);

public sealed record SdbrExecutionObjectEvidenceInterpretation(
    string ContractID,
    string ContractVersion,
    string MessageID,
    string IdempotencyKey,
    string EvidencePackageID,
    string EvidenceVersion,
    string EvidenceStatus,
    string EvidenceConfidence,
    string PlanningRunID,
    string OperatingModelConfigurationID,
    string OperatingModelFingerprint,
    string SchedulingConfigurationID,
    string DDMRPConfigurationID,
    string MasterDataVersionID,
    string OperationalStateSnapshotID,
    string ScheduleFingerprint,
    string WorkOrderID,
    string RoutingID,
    string RoutingAuthority,
    string TraceableID,
    string Status,
    bool AllowsAutomaticOperatingModelUpdate,
    bool AllowsAutomaticMasterSettingUpdate,
    bool AllowsAutomaticBufferUpdate,
    bool AllowsAutomaticSupplierSourceFactUpdate,
    bool AllowsAutomaticLeadTimeUpdate,
    bool AllowsAutomaticMOQUpdate,
    bool AllowsAutomaticOrderCycleUpdate,
    bool RequiresSeparateDdaeApproval,
    bool MutatedDdaeGovernance,
    IReadOnlyList<SdbrExecutionObjectEvidenceAckError> Errors,
    string Message);

public sealed record SdbrExecutionObjectEvidenceLedgerRecord(
    string MessageID,
    string IdempotencyKey,
    string EvidencePackageID,
    string EvidenceVersion,
    string AckStatus,
    string? PrimaryErrorCode,
    string ReceivedAt,
    string PayloadFingerprint,
    string RawPayload,
    SdbrExecutionObjectEvidenceInterpretation Interpretation,
    SdbrExecutionObjectEvidenceAck Ack);

public sealed class SdbrExecutionObjectEvidenceInboundLedger
{
    public const string ContractId = "SDBR-EXECUTION-OBJECT-EVIDENCE-V1";
    public const string ContractVersion = "1.0.0";
    public const string ConsumerSystem = "DDAE";

    private static readonly HashSet<string> AckStatuses = new(StringComparer.Ordinal)
    {
        "Accepted",
        "Duplicate",
        "Rejected",
        "DeadLettered"
    };

    private static readonly HashSet<string> ErrorCodes = new(StringComparer.Ordinal)
    {
        "MISSING_WORK_ORDER",
        "UNKNOWN_WORK_ORDER",
        "UNKNOWN_ROUTING",
        "UNKNOWN_OPERATION",
        "OPERATION_SEQUENCE_INVALID",
        "UNKNOWN_RESOURCE",
        "MISSING_PLANNING_RUN",
        "MISSING_FROZEN_CONFIG",
        "FROZEN_CONFIG_MISMATCH",
        "MISSING_INVENTORY_QUALITY_EVIDENCE",
        "INSUFFICIENT_RELEASED_MATERIAL",
        "INVALID_QUANTITY",
        "UNSUPPORTED_UOM",
        "INVALID_TIMESTAMP",
        "EVENT_ORDER_INVALID",
        "INVALID_STATE_TRANSITION",
        "STALE_VERSION",
        "CONFLICTING_EVENT",
        "REVERSAL_TARGET_NOT_FOUND",
        "SUPERSESSION_TARGET_NOT_FOUND",
        "CONTRACT_SCOPE_VIOLATION",
        "GOVERNANCE_AUTO_UPDATE_FORBIDDEN",
        "IDEMPOTENCY_CONFLICT"
    };

    private static readonly HashSet<string> KnownFixtureWorkOrders = new(StringComparer.Ordinal)
    {
        "WO-SUB-AVIONICS-COMPUTE-001"
    };

    private static readonly HashSet<string> KnownFixtureRoutings = new(StringComparer.Ordinal)
    {
        "SDBR-ROUTE-SUB-AVIONICS-COMPUTE-001"
    };

    private static readonly HashSet<string> KnownFixtureOperations = new(StringComparer.Ordinal)
    {
        "OP-INSTALL-FPGA-SPACE-001"
    };

    private static readonly HashSet<string> KnownFixtureResources = new(StringComparer.Ordinal)
    {
        "RES-AVIONICS-BENCH-001"
    };

    private static readonly HashSet<string> SupportedUoms = new(StringComparer.Ordinal)
    {
        "EA"
    };

    private static readonly IReadOnlyDictionary<string, string> ReviewedFixtureFrozenBaseline =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["PlanningRunID"] = "SDBR-PLAN-RUN-20260628-001",
            ["OperatingModelConfigurationID"] = "OMC-SAT-BUS-001-20260628-001",
            ["OperatingModelFingerprint"] = "sha256:reviewed-fixture-operating-model",
            ["SchedulingConfigurationID"] = "SCH-SAT-BUS-001-20260628-001",
            ["DDMRPConfigurationID"] = "DDMRP-SAT-BUS-001-20260628-001",
            ["MasterDataVersionID"] = "MDV-SDBR-20260628-001",
            ["OperationalStateSnapshotID"] = "OSS-SDBR-20260628-001",
            ["ScheduleFingerprint"] = "sha256:reviewed-fixture-schedule"
        };

    private static readonly HashSet<string> OutOfScopeProductionAuthorityMarkers = new(StringComparer.Ordinal)
    {
        "IsProductionValidated",
        "BusinessGoldenLoopReady",
        "AllowsProductionValidation",
        "ClaimsSupplierExecution",
        "ClaimsDeliveryPerformance",
        "ClaimsLeadTimePerformance",
        "ClaimsProductionInventoryAuthority",
        "ClaimsProductionQualityAuthority"
    };

    private readonly object _sync = new();
    private readonly Func<DateTimeOffset> _clock;
    private readonly Dictionary<string, SdbrExecutionObjectEvidenceLedgerRecord> _recordsByIdempotencyKey = new(StringComparer.Ordinal);
    private readonly List<SdbrExecutionObjectEvidenceLedgerRecord> _records = new();

    public SdbrExecutionObjectEvidenceInboundLedger()
        : this(() => DateTimeOffset.UtcNow)
    {
    }

    public SdbrExecutionObjectEvidenceInboundLedger(Func<DateTimeOffset> clock)
    {
        _clock = clock;
    }

    public IReadOnlyList<SdbrExecutionObjectEvidenceLedgerRecord> Records
    {
        get
        {
            lock (_sync)
            {
                return _records.ToList();
            }
        }
    }

    public SdbrExecutionObjectEvidenceAck Accept(string rawPayload)
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
                    return BuildAck(interpretation, "Duplicate", receivedAt, null);
                }

                var conflictError = Error(
                    "IDEMPOTENCY_CONFLICT",
                    "同一 IdempotencyKey 已存在，但 canonical payload 不一致，必须进入死信处理。",
                    retryable: false);
                var conflictInterpretation = interpretation with
                {
                    Status = "DeadLettered",
                    Errors = new[] { conflictError },
                    Message = "重复 IdempotencyKey 与既有 payload 不一致；未更新 DDAE 主设置。"
                };
                var conflictAck = BuildAck(conflictInterpretation, "DeadLettered", receivedAt, conflictError);
                _records.Add(new SdbrExecutionObjectEvidenceLedgerRecord(
                    conflictInterpretation.MessageID,
                    conflictInterpretation.IdempotencyKey,
                    conflictInterpretation.EvidencePackageID,
                    conflictInterpretation.EvidenceVersion,
                    conflictAck.AckStatus,
                    conflictAck.ErrorCode,
                    receivedAt,
                    fingerprint,
                    rawPayload,
                    conflictInterpretation,
                    conflictAck));
                return conflictAck;
            }

            var primaryError = interpretation.Errors.FirstOrDefault();
            var ack = BuildAck(interpretation, interpretation.Status, receivedAt, primaryError);
            var record = new SdbrExecutionObjectEvidenceLedgerRecord(
                interpretation.MessageID,
                interpretation.IdempotencyKey,
                interpretation.EvidencePackageID,
                interpretation.EvidenceVersion,
                ack.AckStatus,
                ack.ErrorCode,
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

    public SdbrExecutionObjectEvidenceInterpretation Interpret(string rawPayload)
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
        var errors = new List<SdbrExecutionObjectEvidenceAckError>();

        var contractId = RequiredString(root, "ContractID", "CONTRACT_SCOPE_VIOLATION", errors);
        var contractVersion = RequiredString(root, "ContractVersion", "CONTRACT_SCOPE_VIOLATION", errors);
        var messageId = RequiredString(root, "MessageID", "CONTRACT_SCOPE_VIOLATION", errors);
        var idempotencyKey = RequiredString(root, "IdempotencyKey", "CONTRACT_SCOPE_VIOLATION", errors);
        RequiredString(root, "ProducerSystem", "CONTRACT_SCOPE_VIOLATION", errors);
        ValidateConsumerSystems(root, errors);
        ValidateOffsetTimestamp(StringValue(root, "OccurredAt"), "OccurredAt", errors);
        RequiredString(root, "TimeZone", "CONTRACT_SCOPE_VIOLATION", errors);

        if (!string.Equals(contractId, ContractId, StringComparison.Ordinal))
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", $"ContractID 必须为 {ContractId}。", retryable: false));
        }

        if (!string.Equals(contractVersion, ContractVersion, StringComparison.Ordinal))
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", $"ContractVersion 必须为 {ContractVersion}。", retryable: false));
        }

        if (payload is null)
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "Payload 必须存在。", retryable: false));
            return EmptyInterpretation("Rejected", errors, "Payload 缺失；未更新 DDAE 主设置。") with
            {
                ContractID = contractId,
                ContractVersion = contractVersion,
                MessageID = messageId,
                IdempotencyKey = idempotencyKey
            };
        }

        var evidencePackageId = RequiredString(payload, "EvidencePackageID", "CONTRACT_SCOPE_VIOLATION", errors);
        var evidenceVersion = RequiredString(payload, "EvidenceVersion", "CONTRACT_SCOPE_VIOLATION", errors);
        var evidenceStatus = RequiredString(payload, "EvidenceStatus", "CONTRACT_SCOPE_VIOLATION", errors);
        var evidenceConfidence = RequiredString(payload, "EvidenceConfidence", "CONTRACT_SCOPE_VIOLATION", errors);
        var planningContext = payload["PlanningContext"]?.AsObject();
        var workOrder = payload["WorkOrder"]?.AsObject();
        var routing = payload["Routing"]?.AsObject();
        var operations = payload["Operations"] as JsonArray;
        var governanceBoundary = payload["DDAEGovernanceBoundary"]?.AsObject();
        var traceability = payload["Traceability"]?.AsObject();

        var planningRunId = string.Empty;
        var operatingModelConfigurationId = string.Empty;
        var operatingModelFingerprint = string.Empty;
        var schedulingConfigurationId = string.Empty;
        var ddmrpConfigurationId = string.Empty;
        var masterDataVersionId = string.Empty;
        var operationalStateSnapshotId = string.Empty;
        var scheduleFingerprint = string.Empty;
        ValidatePlanningContext(
            planningContext,
            errors,
            out planningRunId,
            out operatingModelConfigurationId,
            out operatingModelFingerprint,
            out schedulingConfigurationId,
            out ddmrpConfigurationId,
            out masterDataVersionId,
            out operationalStateSnapshotId,
            out scheduleFingerprint);

        var workOrderId = ValidateWorkOrder(workOrder, errors);
        var routingId = ValidateRouting(routing, errors);
        var routingAuthority = routing is null ? string.Empty : StringValue(routing, "RoutingAuthority");
        ValidateOperations(operations, workOrder, errors);
        ValidateMaterialRequirements(payload["MaterialRequirements"] as JsonArray, errors);
        ValidateMaterialIssues(payload["MaterialIssues"] as JsonArray, errors);
        ValidateMaterialConsumptions(payload["MaterialConsumptions"] as JsonArray, errors);
        ValidateGovernanceBoundary(governanceBoundary, errors);
        var traceableId = ValidateTraceability(traceability, errors);
        ValidateSupersession(payload["Supersession"]?.AsObject(), errors);
        ValidateNonClaims(payload["NonClaims"], errors);
        ValidateScopeMarkers(payload, errors);

        if (string.Equals(evidenceConfidence, "ProductionValidatedReserved", StringComparison.Ordinal))
        {
            errors.Add(Error(
                "CONTRACT_SCOPE_VIOLATION",
                "EvidenceConfidence = ProductionValidatedReserved 在 V1 中必须拒绝或死信，不能作为正常 payload 接收。",
                retryable: false));
        }

        var status = DetermineStatus(errors);
        var message = status switch
        {
            "Accepted" => "SDBR 执行对象证据已作为受控评审上下文接收；未更新 DDAE 配置或主设置。",
            "DeadLettered" => "SDBR 执行对象证据进入死信；未更新 DDAE 配置或主设置。",
            _ => "SDBR 执行对象证据被拒绝；未更新 DDAE 配置或主设置。"
        };

        return new SdbrExecutionObjectEvidenceInterpretation(
            contractId,
            contractVersion,
            messageId,
            idempotencyKey,
            evidencePackageId,
            evidenceVersion,
            evidenceStatus,
            evidenceConfidence,
            planningRunId,
            operatingModelConfigurationId,
            operatingModelFingerprint,
            schedulingConfigurationId,
            ddmrpConfigurationId,
            masterDataVersionId,
            operationalStateSnapshotId,
            scheduleFingerprint,
            workOrderId,
            routingId,
            routingAuthority,
            traceableId,
            status,
            BoolValue(governanceBoundary, "AllowsAutomaticOperatingModelUpdate") == true,
            BoolValue(governanceBoundary, "AllowsAutomaticMasterSettingUpdate") == true,
            BoolValue(governanceBoundary, "AllowsAutomaticBufferUpdate") == true,
            BoolValue(governanceBoundary, "AllowsAutomaticSupplierSourceFactUpdate") == true,
            BoolValue(governanceBoundary, "AllowsAutomaticLeadTimeUpdate") == true,
            BoolValue(governanceBoundary, "AllowsAutomaticMOQUpdate") == true,
            BoolValue(governanceBoundary, "AllowsAutomaticOrderCycleUpdate") == true,
            BoolValue(governanceBoundary, "RequiresSeparateDDAEApproval") == true,
            MutatedDdaeGovernance: false,
            errors,
            message);
    }

    public static void ValidateAckShape(SdbrExecutionObjectEvidenceAck ack)
    {
        if (!string.Equals(ack.ContractID, ContractId, StringComparison.Ordinal)
            || !string.Equals(ack.ContractVersion, ContractVersion, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(ack.AckID)
            || string.IsNullOrWhiteSpace(ack.MessageID)
            || string.IsNullOrWhiteSpace(ack.IdempotencyKey)
            || !string.Equals(ack.ConsumerSystem, ConsumerSystem, StringComparison.Ordinal)
            || !AckStatuses.Contains(ack.AckStatus)
            || !HasIsoOffset(ack.ReceivedAt)
            || string.IsNullOrWhiteSpace(ack.TraceableID))
        {
            throw new InvalidOperationException("ACK shape does not satisfy SDBR-EXECUTION-OBJECT-EVIDENCE-V1 ACK contract.");
        }

        if (ack.AckStatus is "Accepted" or "Duplicate")
        {
            if (ack.ErrorCode is not null || ack.ErrorMessage is not null || ack.Retryable)
            {
                throw new InvalidOperationException("Accepted/Duplicate ACK must not carry an error.");
            }
        }
        else if (ack.ErrorCode is null || !ErrorCodes.Contains(ack.ErrorCode) || string.IsNullOrWhiteSpace(ack.ErrorMessage))
        {
            throw new InvalidOperationException($"ACK error is not contract-shaped: {ack.ErrorCode}");
        }
    }

    private static void ValidatePlanningContext(
        JsonObject? planningContext,
        ICollection<SdbrExecutionObjectEvidenceAckError> errors,
        out string planningRunId,
        out string operatingModelConfigurationId,
        out string operatingModelFingerprint,
        out string schedulingConfigurationId,
        out string ddmrpConfigurationId,
        out string masterDataVersionId,
        out string operationalStateSnapshotId,
        out string scheduleFingerprint)
    {
        planningRunId = string.Empty;
        operatingModelConfigurationId = string.Empty;
        operatingModelFingerprint = string.Empty;
        schedulingConfigurationId = string.Empty;
        ddmrpConfigurationId = string.Empty;
        masterDataVersionId = string.Empty;
        operationalStateSnapshotId = string.Empty;
        scheduleFingerprint = string.Empty;

        if (planningContext is null)
        {
            errors.Add(Error("MISSING_PLANNING_RUN", "PlanningContext 缺失。", retryable: false));
            errors.Add(Error("MISSING_FROZEN_CONFIG", "frozen DDS&OP configuration references 缺失。", retryable: false));
            return;
        }

        planningRunId = RequiredString(planningContext, "PlanningRunID", "MISSING_PLANNING_RUN", errors);
        operatingModelConfigurationId = RequiredString(planningContext, "OperatingModelConfigurationID", "MISSING_FROZEN_CONFIG", errors);
        operatingModelFingerprint = RequiredString(planningContext, "OperatingModelFingerprint", "MISSING_FROZEN_CONFIG", errors);
        schedulingConfigurationId = RequiredString(planningContext, "SchedulingConfigurationID", "MISSING_FROZEN_CONFIG", errors);
        ddmrpConfigurationId = RequiredString(planningContext, "DDMRPConfigurationID", "MISSING_FROZEN_CONFIG", errors);
        masterDataVersionId = RequiredString(planningContext, "MasterDataVersionID", "MISSING_FROZEN_CONFIG", errors);
        operationalStateSnapshotId = RequiredString(planningContext, "OperationalStateSnapshotID", "MISSING_FROZEN_CONFIG", errors);
        scheduleFingerprint = RequiredString(planningContext, "ScheduleFingerprint", "MISSING_FROZEN_CONFIG", errors);

        var submittedValues = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["PlanningRunID"] = planningRunId,
            ["OperatingModelConfigurationID"] = operatingModelConfigurationId,
            ["OperatingModelFingerprint"] = operatingModelFingerprint,
            ["SchedulingConfigurationID"] = schedulingConfigurationId,
            ["DDMRPConfigurationID"] = ddmrpConfigurationId,
            ["MasterDataVersionID"] = masterDataVersionId,
            ["OperationalStateSnapshotID"] = operationalStateSnapshotId,
            ["ScheduleFingerprint"] = scheduleFingerprint
        };

        foreach (var (fieldName, expectedValue) in ReviewedFixtureFrozenBaseline)
        {
            if (submittedValues.TryGetValue(fieldName, out var actualValue)
                && !string.IsNullOrWhiteSpace(actualValue)
                && !string.Equals(actualValue, expectedValue, StringComparison.Ordinal))
            {
                errors.Add(Error(
                    "FROZEN_CONFIG_MISMATCH",
                    $"{fieldName} 与 reviewed fixture frozen baseline 不一致。",
                    retryable: true));
            }
        }
    }

    private static string ValidateWorkOrder(JsonObject? workOrder, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (workOrder is null)
        {
            errors.Add(Error("MISSING_WORK_ORDER", "WorkOrder 缺失。", retryable: false));
            return string.Empty;
        }

        var workOrderId = RequiredString(workOrder, "WorkOrderID", "MISSING_WORK_ORDER", errors);
        RequiredString(workOrder, "WorkOrderVersion", "MISSING_WORK_ORDER", errors);
        var workOrderStatus = RequiredString(workOrder, "WorkOrderStatus", "MISSING_WORK_ORDER", errors);
        RequiredString(workOrder, "ProductID", "MISSING_WORK_ORDER", errors);
        RequiredString(workOrder, "ItemID", "MISSING_WORK_ORDER", errors);
        RequiredString(workOrder, "LocationID", "MISSING_WORK_ORDER", errors);
        ValidateNonNegativeNumber(workOrder, "RequiredQty", "INVALID_QUANTITY", errors);
        ValidateNonNegativeNumber(workOrder, "CompletedQty", "INVALID_QUANTITY", errors);
        ValidateNonNegativeNumber(workOrder, "RemainingQty", "INVALID_QUANTITY", errors);
        ValidateNonNegativeNumber(workOrder, "ScrapQty", "INVALID_QUANTITY", errors);
        ValidateNonNegativeNumber(workOrder, "RejectQty", "INVALID_QUANTITY", errors);
        ValidateUom(StringValue(workOrder, "QuantityUOM"), errors);
        ValidateOffsetTimestamp(StringValue(workOrder, "RecordedAt"), "RecordedAt", errors);
        ValidateLateCapture(workOrder, workOrderStatus, "work order", errors);

        if (!string.IsNullOrWhiteSpace(workOrderId) && !KnownFixtureWorkOrders.Contains(workOrderId))
        {
            errors.Add(Error("UNKNOWN_WORK_ORDER", "当前 scoped implementation 只接受受控 fixture work order。", retryable: true));
        }

        return workOrderId;
    }

    private static string ValidateRouting(JsonObject? routing, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (routing is null)
        {
            errors.Add(Error("UNKNOWN_ROUTING", "Routing 缺失。", retryable: true));
            return string.Empty;
        }

        var routingId = RequiredString(routing, "RoutingID", "UNKNOWN_ROUTING", errors);
        RequiredString(routing, "RoutingVersion", "UNKNOWN_ROUTING", errors);
        var routingAuthority = RequiredString(routing, "RoutingAuthority", "UNKNOWN_ROUTING", errors);
        RequiredString(routing, "RoutingSelectionReason", "UNKNOWN_ROUTING", errors);

        if (!string.Equals(routingAuthority, "SDBR_EXECUTABLE_ROUTING", StringComparison.Ordinal))
        {
            errors.Add(Error("UNKNOWN_ROUTING", "RoutingAuthority 必须为 SDBR_EXECUTABLE_ROUTING。", retryable: false));
        }

        if (!string.IsNullOrWhiteSpace(routingId) && !KnownFixtureRoutings.Contains(routingId))
        {
            errors.Add(Error("UNKNOWN_ROUTING", "当前 scoped implementation 只接受受控 fixture executable routing。", retryable: true));
        }

        if (routingId.StartsWith("DDAE-", StringComparison.OrdinalIgnoreCase)
            || routingId.Contains("PRIMARY-ROUTING", StringComparison.OrdinalIgnoreCase)
            || routingId.Contains("CONTROL-POINT", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(Error(
                "UNKNOWN_ROUTING",
                "DDAE PrimaryRoutingID、NetworkRoutingLine、ResourceRouting、CapacityResource 或 ControlPointID 不能作为 SDBR executable routing proof。",
                retryable: false));
        }

        return routingId;
    }

    private static void ValidateOperations(JsonArray? operations, JsonObject? workOrder, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (operations is null || operations.Count == 0)
        {
            errors.Add(Error("UNKNOWN_OPERATION", "Operations 缺失。", retryable: true));
            return;
        }

        var previousSequence = 0;
        foreach (var item in operations)
        {
            if (item is not JsonObject operation)
            {
                errors.Add(Error("UNKNOWN_OPERATION", "Operation 必须是 object。", retryable: false));
                continue;
            }

            var operationId = RequiredString(operation, "OperationID", "UNKNOWN_OPERATION", errors);
            var operationSequence = IntValue(operation, "OperationSequence");
            if (operationSequence <= 0 || operationSequence <= previousSequence)
            {
                errors.Add(Error("OPERATION_SEQUENCE_INVALID", "OperationSequence 必须为正数且按 routing 顺序递增。", retryable: false));
            }

            previousSequence = Math.Max(previousSequence, operationSequence);
            var operationStatus = RequiredString(operation, "OperationStatus", "UNKNOWN_OPERATION", errors);
            var resourceId = RequiredString(operation, "ResourceID", "UNKNOWN_RESOURCE", errors);
            RequiredString(operation, "WorkCenterID", "UNKNOWN_RESOURCE", errors);
            RequiredString(operation, "OperationCode", "UNKNOWN_OPERATION", errors);
            RequiredString(operation, "OperationName", "UNKNOWN_OPERATION", errors);
            ValidateOffsetTimestamp(StringValue(operation, "RecordedAt"), "RecordedAt", errors);
            ValidateLateCapture(operation, operationStatus, "operation", errors);

            if (!string.IsNullOrWhiteSpace(operationId) && !KnownFixtureOperations.Contains(operationId))
            {
                errors.Add(Error("UNKNOWN_OPERATION", "当前 scoped implementation 只接受受控 fixture operation。", retryable: true));
            }

            if (!string.IsNullOrWhiteSpace(resourceId) && !KnownFixtureResources.Contains(resourceId))
            {
                errors.Add(Error("UNKNOWN_RESOURCE", "当前 scoped implementation 只接受受控 fixture resource。", retryable: true));
            }
        }

        if (string.Equals(StringValue(workOrder, "WorkOrderStatus"), "Completed", StringComparison.Ordinal)
            && !operations.OfType<JsonObject>().Any(op => StringValue(op, "OperationStatus") is "Started" or "Completed"))
        {
            ValidateCompletedLateCapture(workOrder, "work order", errors);
        }
    }

    private static void ValidateMaterialRequirements(JsonArray? requirements, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (requirements is null)
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "MaterialRequirements 必须存在，可为空数组。", retryable: false));
            return;
        }

        foreach (var item in requirements)
        {
            if (item is not JsonObject requirement)
            {
                errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "MaterialRequirement 必须是 object。", retryable: false));
                continue;
            }

            RequiredString(requirement, "MaterialRequirementID", "CONTRACT_SCOPE_VIOLATION", errors);
            RequiredString(requirement, "ConsumedItemID", "CONTRACT_SCOPE_VIOLATION", errors);
            RequiredString(requirement, "ConsumedLocationID", "CONTRACT_SCOPE_VIOLATION", errors);
            ValidateOffsetTimestamp(StringValue(requirement, "RequiredAt"), "RequiredAt", errors);
            ValidateNonNegativeNumber(requirement, "RequiredQty", "INVALID_QUANTITY", errors);
            ValidateUom(StringValue(requirement, "QuantityUOM"), errors);

            if (requirement.ContainsKey("IssuedQty") || requirement.ContainsKey("ConsumedQty"))
            {
                errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "MaterialRequirement 不能被解释为发料或消耗证明。", retryable: false));
            }
        }
    }

    private static void ValidateMaterialIssues(JsonArray? issues, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (issues is null)
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "MaterialIssues 必须存在，可为空数组。", retryable: false));
            return;
        }

        foreach (var item in issues)
        {
            if (item is not JsonObject issue)
            {
                errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "MaterialIssue 必须是 object。", retryable: false));
                continue;
            }

            RequiredString(issue, "IssueEventID", "CONTRACT_SCOPE_VIOLATION", errors);
            ValidateNonNegativeNumber(issue, "IssuedQty", "INVALID_QUANTITY", errors);
            ValidateUom(StringValue(issue, "QuantityUOM"), errors);
            ValidateOffsetTimestamp(StringValue(issue, "IssuedAt"), "IssuedAt", errors);
            if (string.IsNullOrWhiteSpace(StringValue(issue, "InventoryQualityEvidencePackageID"))
                && string.IsNullOrWhiteSpace(StringValue(issue, "IssueAuthorityReferenceID")))
            {
                errors.Add(Error("MISSING_INVENTORY_QUALITY_EVIDENCE", "MaterialIssue 必须直接引用库存质量证据或 accepted issue authority。", retryable: false));
            }
        }
    }

    private static void ValidateMaterialConsumptions(JsonArray? consumptions, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (consumptions is null)
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "MaterialConsumptions 必须存在，可为空数组。", retryable: false));
            return;
        }

        foreach (var item in consumptions)
        {
            if (item is not JsonObject consumption)
            {
                errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "MaterialConsumption 必须是 object。", retryable: false));
                continue;
            }

            RequiredString(consumption, "ConsumptionEventID", "CONTRACT_SCOPE_VIOLATION", errors);
            ValidateNonNegativeNumber(consumption, "ConsumedQty", "INVALID_QUANTITY", errors);
            ValidateNonNegativeNumber(consumption, "ScrapQty", "INVALID_QUANTITY", errors);
            ValidateNonNegativeNumber(consumption, "RejectQty", "INVALID_QUANTITY", errors);
            ValidateNonNegativeNumber(consumption, "RemainingQty", "INVALID_QUANTITY", errors);
            ValidateUom(StringValue(consumption, "QuantityUOM"), errors);
            ValidateOffsetTimestamp(StringValue(consumption, "ConsumedAt"), "ConsumedAt", errors);
            if (string.IsNullOrWhiteSpace(StringValue(consumption, "InventoryQualityEvidencePackageID"))
                && string.IsNullOrWhiteSpace(StringValue(consumption, "ConsumptionAuthorityReferenceID")))
            {
                errors.Add(Error("MISSING_INVENTORY_QUALITY_EVIDENCE", "MaterialConsumption 必须直接引用库存质量证据或 accepted consumption authority。", retryable: false));
            }
        }
    }

    private static void ValidateGovernanceBoundary(JsonObject? governanceBoundary, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (governanceBoundary is null)
        {
            errors.Add(Error("GOVERNANCE_AUTO_UPDATE_FORBIDDEN", "DDAEGovernanceBoundary 缺失。", retryable: false));
            return;
        }

        var forbiddenFlags = new[]
        {
            "AllowsAutomaticOperatingModelUpdate",
            "AllowsAutomaticMasterSettingUpdate",
            "AllowsAutomaticBufferUpdate",
            "AllowsAutomaticSupplierSourceFactUpdate",
            "AllowsAutomaticLeadTimeUpdate",
            "AllowsAutomaticMOQUpdate",
            "AllowsAutomaticOrderCycleUpdate"
        };

        foreach (var flag in forbiddenFlags)
        {
            if (BoolValue(governanceBoundary, flag) != false)
            {
                errors.Add(Error("GOVERNANCE_AUTO_UPDATE_FORBIDDEN", $"{flag} 必须为 false。", retryable: false));
            }
        }

        if (BoolValue(governanceBoundary, "RequiresSeparateDDAEApproval") != true)
        {
            errors.Add(Error("GOVERNANCE_AUTO_UPDATE_FORBIDDEN", "RequiresSeparateDDAEApproval 必须为 true。", retryable: false));
        }
    }

    private static string ValidateTraceability(JsonObject? traceability, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (traceability is null)
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "Traceability 缺失。", retryable: false));
            return "UNKNOWN-TRACE";
        }

        RequiredString(traceability, "SourceSystem", "CONTRACT_SCOPE_VIOLATION", errors);
        RequiredString(traceability, "SourceDocumentType", "CONTRACT_SCOPE_VIOLATION", errors);
        RequiredString(traceability, "SourceDocumentID", "CONTRACT_SCOPE_VIOLATION", errors);
        RequiredString(traceability, "SourceRecordID", "CONTRACT_SCOPE_VIOLATION", errors);
        var traceableId = RequiredString(traceability, "TraceableID", "CONTRACT_SCOPE_VIOLATION", errors);
        ValidateOffsetTimestamp(StringValue(traceability, "RegisteredAt"), "RegisteredAt", errors);
        return string.IsNullOrWhiteSpace(traceableId) ? "UNKNOWN-TRACE" : traceableId;
    }

    private static void ValidateSupersession(JsonObject? supersession, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (supersession is null)
        {
            errors.Add(Error("SUPERSESSION_TARGET_NOT_FOUND", "Supersession object 缺失。", retryable: true));
            return;
        }

        var hasCorrectionReason = !string.IsNullOrWhiteSpace(StringValue(supersession, "CorrectionReason"));
        var hasCorrectedEvent = !string.IsNullOrWhiteSpace(StringValue(supersession, "CorrectsExecutionEventID"));
        if (hasCorrectionReason != hasCorrectedEvent)
        {
            errors.Add(Error("SUPERSESSION_TARGET_NOT_FOUND", "Correction 必须同时包含 reason 与 CorrectsExecutionEventID。", retryable: true));
        }

        var hasReversalReason = !string.IsNullOrWhiteSpace(StringValue(supersession, "ReversalReason"));
        var hasReversalEvent = !string.IsNullOrWhiteSpace(StringValue(supersession, "ReversesExecutionEventID"));
        if (hasReversalReason != hasReversalEvent)
        {
            errors.Add(Error("REVERSAL_TARGET_NOT_FOUND", "Reversal 必须同时包含 reason 与 ReversesExecutionEventID。", retryable: true));
        }
    }

    private static void ValidateNonClaims(JsonNode? nonClaimsNode, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (nonClaimsNode is not JsonArray nonClaims || nonClaims.Count == 0)
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "NonClaims 必须存在，用于限定非声明范围。", retryable: false));
        }
    }

    private static void ValidateScopeMarkers(JsonObject payload, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        foreach (var marker in FindOutOfScopeProductionAuthorityMarkers(payload))
        {
            errors.Add(Error(
                "CONTRACT_SCOPE_VIOLATION",
                $"该 contract 不允许生产权威声明标记 {marker}。",
                retryable: false));
        }
    }

    private static IEnumerable<string> FindOutOfScopeProductionAuthorityMarkers(JsonNode? node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject)
            {
                if (OutOfScopeProductionAuthorityMarkers.Contains(property.Key))
                {
                    yield return property.Key;
                }

                foreach (var nested in FindOutOfScopeProductionAuthorityMarkers(property.Value))
                {
                    yield return nested;
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                foreach (var nested in FindOutOfScopeProductionAuthorityMarkers(item))
                {
                    yield return nested;
                }
            }
        }
    }

    private static void ValidateLateCapture(JsonObject node, string status, string scope, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        RequiredString(node, "EventCaptureMode", "INVALID_STATE_TRANSITION", errors);
        ValidateOffsetTimestamp(StringValue(node, "RecordedAt"), "RecordedAt", errors);

        if (string.Equals(status, "Completed", StringComparison.Ordinal))
        {
            ValidateCompletedLateCapture(node, scope, errors);
        }
    }

    private static void ValidateCompletedLateCapture(JsonObject? node, string scope, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (node is null)
        {
            return;
        }

        if (!string.Equals(StringValue(node, "EventCaptureMode"), "LateCaptured", StringComparison.Ordinal))
        {
            errors.Add(Error("EVENT_ORDER_INVALID", $"{scope} Completed 缺少 prior Started evidence 时必须使用 LateCaptured。", retryable: false));
            return;
        }

        if (string.IsNullOrWhiteSpace(StringValue(node, "LateCaptureReason"))
            || string.IsNullOrWhiteSpace(StringValue(node, "ReconciliationReferenceID"))
            || string.IsNullOrWhiteSpace(StringValue(node, "ObservedAt")))
        {
            errors.Add(Error("EVENT_ORDER_INVALID", $"{scope} LateCaptured 必须提供 reason、reconciliation reference、ObservedAt 和 RecordedAt。", retryable: false));
        }

        ValidateOffsetTimestamp(StringValue(node, "ObservedAt"), "ObservedAt", errors);
    }

    private static void ValidateUom(string value, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (string.IsNullOrWhiteSpace(value) || !SupportedUoms.Contains(value))
        {
            errors.Add(Error("UNSUPPORTED_UOM", "UOM 缺失或不受当前 fixture consumer 支持。", retryable: false));
        }
    }

    private static void ValidateNonNegativeNumber(JsonObject node, string propertyName, string errorCode, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        var value = node[propertyName];
        if (value is null)
        {
            errors.Add(Error(errorCode, $"{propertyName} 缺失。", retryable: false));
            return;
        }

        try
        {
            if (value.GetValue<decimal>() < 0)
            {
                errors.Add(Error(errorCode, $"{propertyName} 不能为负数。", retryable: false));
            }
        }
        catch (InvalidOperationException)
        {
            errors.Add(Error(errorCode, $"{propertyName} 必须是数值。", retryable: false));
        }
    }

    private static void ValidateConsumerSystems(JsonObject root, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (root["ConsumerSystems"] is not JsonArray consumers
            || !consumers.Any(item => string.Equals(item?.GetValue<string>(), ConsumerSystem, StringComparison.Ordinal)))
        {
            errors.Add(Error("CONTRACT_SCOPE_VIOLATION", "ConsumerSystems 必须包含 DDAE。", retryable: false));
        }
    }

    private static string DetermineStatus(IReadOnlyCollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (errors.Count == 0)
        {
            return "Accepted";
        }

        return errors.Any(item => item.ErrorCode is "UNKNOWN_WORK_ORDER"
            or "UNKNOWN_ROUTING"
            or "UNKNOWN_OPERATION"
            or "UNKNOWN_RESOURCE"
            or "IDEMPOTENCY_CONFLICT"
            or "REVERSAL_TARGET_NOT_FOUND"
            or "SUPERSESSION_TARGET_NOT_FOUND"
            or "CONFLICTING_EVENT"
            or "STALE_VERSION")
            ? "DeadLettered"
            : "Rejected";
    }

    private static SdbrExecutionObjectEvidenceAck BuildAck(
        SdbrExecutionObjectEvidenceInterpretation interpretation,
        string status,
        string receivedAt,
        SdbrExecutionObjectEvidenceAckError? primaryError)
    {
        var ack = new SdbrExecutionObjectEvidenceAck(
            ContractId,
            ContractVersion,
            $"DDAE-ACK-SDBR-EOE-{Slug(interpretation.MessageID)}-{status.ToUpperInvariant()}",
            string.IsNullOrWhiteSpace(interpretation.MessageID) ? "UNKNOWN-MESSAGE" : interpretation.MessageID,
            string.IsNullOrWhiteSpace(interpretation.IdempotencyKey) ? "UNKNOWN-IDEMPOTENCY-KEY" : interpretation.IdempotencyKey,
            ConsumerSystem,
            status,
            primaryError?.ErrorCode,
            primaryError?.ErrorMessage,
            receivedAt,
            primaryError?.Retryable ?? false,
            string.IsNullOrWhiteSpace(interpretation.TraceableID) ? "UNKNOWN-TRACE" : interpretation.TraceableID);
        ValidateAckShape(ack);
        return ack;
    }

    private static SdbrExecutionObjectEvidenceInterpretation EmptyInterpretation(
        string status,
        IReadOnlyList<SdbrExecutionObjectEvidenceAckError> errors,
        string message)
    {
        return new SdbrExecutionObjectEvidenceInterpretation(
            ContractId,
            ContractVersion,
            "UNKNOWN-MESSAGE",
            "UNKNOWN-IDEMPOTENCY-KEY",
            "UNKNOWN-EVIDENCE-PACKAGE",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "UNKNOWN-TRACE",
            status,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            true,
            false,
            errors,
            message);
    }

    private static string RequiredString(JsonObject? node, string propertyName, string errorCode, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        var value = StringValue(node, propertyName);
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(Error(errorCode, $"{propertyName} 缺失。", retryable: false));
        }

        return value;
    }

    private static string StringValue(JsonObject? node, string propertyName)
    {
        return node?[propertyName]?.GetValue<string>() ?? string.Empty;
    }

    private static int IntValue(JsonObject node, string propertyName)
    {
        var value = node[propertyName];
        if (value is null)
        {
            return 0;
        }

        try
        {
            return value.GetValue<int>();
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
    }

    private static bool? BoolValue(JsonObject? node, string propertyName)
    {
        var value = node?[propertyName];
        return value is null ? null : value.GetValue<bool>();
    }

    private static SdbrExecutionObjectEvidenceAckError Error(string code, string message, bool retryable)
    {
        return new SdbrExecutionObjectEvidenceAckError(code, message, retryable);
    }

    private static void ValidateOffsetTimestamp(string? value, string fieldName, ICollection<SdbrExecutionObjectEvidenceAckError> errors)
    {
        if (!HasIsoOffset(value))
        {
            errors.Add(Error("INVALID_TIMESTAMP", $"{fieldName} 必须是带 UTC offset 的 ISO 时间。", retryable: false));
        }
    }

    private static bool HasIsoOffset(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
            && (value.EndsWith("Z", StringComparison.Ordinal)
                || (value.Length >= 6 && (value[^6] == '+' || value[^6] == '-')));
    }

    private static string FormatDateTime(DateTimeOffset value)
    {
        return value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture);
    }

    private static string Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "UNKNOWN";
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) ? char.ToUpperInvariant(character) : '-');
        }

        return builder.ToString().Trim('-');
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
            default:
                node.WriteTo(writer);
                return;
        }
    }
}
