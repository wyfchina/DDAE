using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdaptiveSopDdsop.Web.Domain;

public sealed record DdsopRuntimePlanningInputRequest(
    int HorizonWeeks = 12,
    DateOnly? AnchorDate = null,
    string? ApprovedBy = null,
    string? SourceScenarioRunID = null,
    string? ChangeTicketID = null,
    string? RuntimePlanningInputPackageID = null,
    string? DeliveryLedgerCorrelationID = null,
    string ExecutionMode = "DDMRPAndBoundedScheduling",
    string PackageStatus = "Reviewed",
    string MappingConfidence = "PublicDemoOnly",
    string ScenarioLabel = "ControlledContractGoldenLoopDemo");

public sealed record DdsopRuntimePlanningInputMessage(
    string ContractID,
    string ContractVersion,
    string MessageID,
    string MessageType,
    string SourceSystem,
    IReadOnlyList<string> TargetSystem,
    string IdempotencyKey,
    string OccurredAt,
    DdsopRuntimePlanningInputPackage Payload);

public sealed record DdsopRuntimePlanningInputPackage(
    DdsopRuntimePackageIdentity PackageIdentity,
    DdsopFrozenDdsopConfiguration FrozenDdsopConfiguration,
    DdsopParameterAuthorityEvidence ParameterAuthorityEvidence,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DdsopRuntimeEvidenceSnapshot? RuntimeEvidenceSnapshot,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DdsopExecutableSchedulingInputs? ExecutableSchedulingInputs,
    DdsopRuntimeConsumerRules ConsumerRules,
    DdsopRuntimeOutputExpectations OutputExpectations);

public sealed record DdsopRuntimePackageIdentity(
    string RuntimePlanningInputPackageID,
    string PackageVersion,
    string PackageStatus,
    string ExecutionMode,
    string MappingConfidence,
    string ScenarioLabel);

public sealed record DdsopFrozenDdsopConfiguration(
    string OperatingModelConfigurationID,
    string OperatingModelFingerprint,
    string ConfigurationVersion,
    string SourceConfigurationContractID,
    string ConfigStatus,
    string EffectiveFrom,
    string? EffectiveTo,
    string TimeZone,
    string SchedulingConfigurationID,
    string DDMRPConfigurationID);

public sealed record DdsopParameterAuthorityEvidence(
    string DDMRPFormulaVersionID,
    string SchedulingRuleVersionID,
    string ApprovalEvidenceID,
    string ApprovedBy,
    string ApprovedAt,
    string EffectivePolicyID,
    IReadOnlyList<DdsopParameterEvidenceRef> ParameterEvidenceRefs);

public sealed record DdsopParameterEvidenceRef(
    string FieldGroup,
    string EvidenceID,
    string SourceAuthority,
    string CalculationStatus,
    string ProductionAuthorityStatus,
    string Applicability,
    string? NotApplicableReason);

public sealed record DdsopRuntimeEvidenceSnapshot(
    string OperationalStateSnapshotID,
    string SnapshotAt,
    IReadOnlyList<DdsopRuntimeInventoryPosition> InventoryPositions,
    IReadOnlyList<DdsopRuntimeDemandSignal> DemandSignals,
    IReadOnlyList<DdsopRuntimeOpenSupplySignal> OpenSupplySignals,
    IReadOnlyList<DdsopRuntimeEvidenceRef> QualityEvidenceRefs);

public sealed record DdsopRuntimeInventoryPosition(
    string ItemID,
    string LocationID,
    string UnitOfMeasure,
    decimal OnHandQty,
    decimal AllocatedQty,
    decimal AvailableQty,
    string QualityState,
    IReadOnlyList<DdsopRuntimeEvidenceRef> EvidenceRefs);

public sealed record DdsopRuntimeDemandSignal(
    string DemandID,
    string ItemID,
    string LocationID,
    string DueAt,
    decimal Quantity,
    string UnitOfMeasure,
    string DemandType,
    string SpikeQualificationStatus,
    string SpikeQualificationMode,
    string? SpikeQualificationEvidenceID,
    IReadOnlyList<DdsopRuntimeEvidenceRef> EvidenceRefs);

public sealed record DdsopRuntimeOpenSupplySignal(
    string SupplyID,
    string ItemID,
    string LocationID,
    string ExpectedAt,
    decimal Quantity,
    string UnitOfMeasure,
    string SupplyStatus,
    IReadOnlyList<DdsopRuntimeEvidenceRef> EvidenceRefs);

public sealed record DdsopExecutableSchedulingInputs(
    string MasterDataVersionID,
    string AdapterProfileID,
    string CapacityUnitNormalizationRuleID,
    string MaterialConstraintsMode,
    string SetupChangeoverMode,
    IReadOnlyList<DdsopRuntimeWorkOrder> WorkOrders,
    IReadOnlyList<DdsopRuntimeRouting> Routings,
    IReadOnlyList<DdsopRuntimeOperation> Operations,
    IReadOnlyList<DdsopRuntimeResourceCalendar> ResourceCalendars,
    IReadOnlyList<DdsopRuntimeMaterialConstraint> MaterialConstraints,
    IReadOnlyList<DdsopRuntimeEvidenceRef> SetupChangeoverRules);

public sealed record DdsopRuntimeWorkOrder(
    string WorkOrderID,
    string ProductID,
    string RoutingID,
    decimal Quantity,
    string UnitOfMeasure,
    string? EarliestReleaseAt,
    string DueAt,
    int Priority,
    IReadOnlyList<DdsopRuntimeEvidenceRef> EvidenceRefs);

public sealed record DdsopRuntimeRouting(
    string RoutingID,
    string ProductID,
    IReadOnlyList<string> OperationIDs,
    IReadOnlyList<DdsopRuntimeEvidenceRef> EvidenceRefs);

public sealed record DdsopRuntimeOperation(
    string OperationID,
    int Sequence,
    string ResourceID,
    IReadOnlyList<string> AlternateResourceIDs,
    int DurationMinutes,
    IReadOnlyList<DdsopRuntimeEvidenceRef> EvidenceRefs);

public sealed record DdsopRuntimeResourceCalendar(
    string ResourceID,
    string CalendarID,
    IReadOnlyList<DdsopRuntimeCapacityWindow> CapacityWindows,
    IReadOnlyList<DdsopRuntimeEvidenceRef> EvidenceRefs);

public sealed record DdsopRuntimeCapacityWindow(
    string StartAt,
    string EndAt,
    decimal CapacityUnits);

public sealed record DdsopRuntimeMaterialConstraint(
    string ItemID,
    string LocationID,
    decimal AvailableQty,
    string UnitOfMeasure,
    IReadOnlyList<DdsopRuntimeEvidenceRef> EvidenceRefs);

public sealed record DdsopRuntimeEvidenceRef(
    string EvidenceID,
    string SourceAuthority,
    string SourceRecordID,
    string SourceObservedAt);

public sealed record DdsopRuntimeConsumerRules(
    IReadOnlyList<string> ReadOnlyFrozenInputs,
    IReadOnlyList<string> SDBRDerivedRuntimeSignals,
    IReadOnlyList<string> ForbiddenMutations);

public sealed record DdsopRuntimeOutputExpectations(
    string FeedbackContractID,
    bool PlanningRunFeedbackRequired,
    bool VarianceAnalysisFeedbackRequired,
    string RuntimePlanningInputPackageID,
    string FeedbackCorrelationMode,
    string DeliveryLedgerCorrelationID,
    string ReplayPolicy,
    string DeadLetterPolicy);

public sealed record DdsopRuntimeFeedbackCorrelation(
    string OriginalMessageID,
    string IdempotencyKey,
    string ProcessingStatus,
    string ReceivedAt,
    string FeedbackType,
    string? PlanningRunID);

public sealed record DdsopRuntimeDeliveryLedgerRecord(
    string RuntimePlanningInputPackageID,
    string DeliveryLedgerCorrelationID,
    string OperatingModelConfigurationID,
    string OperatingModelFingerprint,
    string CreatedAt,
    IReadOnlyList<DdsopRuntimeFeedbackCorrelation> FeedbackCorrelations);

public sealed class DdsopRuntimePlanningInputContractService
{
    public const string AdventureWorksAdapterProfileID = "ADVENTUREWORKS-BOUNDED-SCHEDULING-ADAPTER-PROFILE-V1";
    public const string AdventureWorksCapacityUnitNormalizationRuleID = "AW-CAPACITY-UNIT-FIXTURE-ONE-UNIT-PER-RESOURCE-WINDOW";
    public const string AdventureWorksMaterialConstraintsMode = "OmittedForPublicDemo";
    public const string AdventureWorksSetupChangeoverMode = "NoSetupRulesApplied";

    private const string DdaeGovernanceEvidenceAuthority = "DDAE PublicDemoOnly controlled fixture governance evidence";
    private const string NonDdaeSchedulingEvidenceAuthority = "SDBR public demo bounded scheduling adapter fixture; non-DDAE-owned execution metadata";
    private const string NonDdaeCalendarEvidenceAuthority = "SDBR public demo calendar fixture; DDAE displays only and does not author executable capacity windows";

    private static readonly string[] ParameterFieldGroups =
    {
        "ADU",
        "DLT",
        "VariabilityFactor",
        "MOQ",
        "OrderCycle",
        "BufferZones",
        "DecouplingPoint",
        "BufferProfile",
        "UOM",
        "AdjustmentFactor",
        "ControlPoint",
        "TimeBuffer",
        "ResourcePolicy",
        "CalendarPolicy",
        "ReleasePolicy"
    };

    private readonly DdsopConfigInboundContractService _configService;

    public DdsopRuntimePlanningInputContractService(DdsopConfigInboundContractService configService)
    {
        _configService = configService;
    }

    public DdsopRuntimePlanningInputMessage Build(DdsopRuntimePlanningInputRequest request)
    {
        var configMessage = _configService.Build(new DdsopConfigInboundContractRequest(
            request.HorizonWeeks,
            request.AnchorDate,
            request.ApprovedBy,
            request.SourceScenarioRunID,
            request.ChangeTicketID));
        var config = configMessage.Payload;
        var dateToken = (request.AnchorDate ?? new DateOnly(2026, 6, 26)).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var packageId = string.IsNullOrWhiteSpace(request.RuntimePlanningInputPackageID)
            ? $"DDAE-RPI-{dateToken}-001"
            : request.RuntimePlanningInputPackageID;
        var correlationId = string.IsNullOrWhiteSpace(request.DeliveryLedgerCorrelationID)
            ? $"DDAE-RPI-DELIVERY-{dateToken}-001"
            : request.DeliveryLedgerCorrelationID;
        var frozen = new DdsopFrozenDdsopConfiguration(
            config.OperatingModelConfigurationID,
            config.Fingerprint,
            config.ConfigurationVersion,
            "DDSOP-CONFIG-INBOUND-V1",
            config.Status,
            config.EffectiveFrom,
            config.EffectiveTo,
            config.TimeZone,
            config.SchedulingConfiguration.SchedulingConfigurationID,
            config.DDMRPConfiguration.DDMRPConfigurationID);
        var evidence = new DdsopParameterAuthorityEvidence(
            "DDAE-DDMRP-FORMULA-V1",
            "DDAE-SCHEDULING-RULE-V1",
            $"APPROVAL-{config.OperatingModelConfigurationID}",
            config.Approval.ApprovedBy,
            config.Approval.ApprovedAt,
            $"EFFECTIVITY-{config.OperatingModelConfigurationID}",
            BuildParameterEvidence(config));
        var identity = new DdsopRuntimePackageIdentity(
            packageId,
            "1",
            request.PackageStatus,
            request.ExecutionMode,
            request.MappingConfidence,
            request.ScenarioLabel);
        var runtimeEvidence = RequiresRuntimeEvidence(request.ExecutionMode)
            ? BuildRuntimeEvidence(config, dateToken)
            : null;
        var executableInputs = RequiresExecutableInputs(request.ExecutionMode)
            ? BuildExecutableInputs(config, dateToken)
            : null;
        var outputExpectations = new DdsopRuntimeOutputExpectations(
            "DDSOP-FEEDBACK-OUTBOUND-V1",
            true,
            true,
            packageId,
            "DeliveryLedger",
            correlationId,
            "Duplicate idempotency keys must reuse the original feedback result.",
            "Rejected or uninterpretable feedback must be retained for governance review.");

        return new DdsopRuntimePlanningInputMessage(
            "DDSOP-RUNTIME-PLANNING-INPUT-V1",
            "0.1.0-draft",
            $"DDAE-MSG-RPI-{dateToken}-001",
            "RuntimePlanningInputPackagePublished",
            "DDAE",
            new[] { "SDBR" },
            $"DDAE:RPI:{packageId}",
            configMessage.OccurredAt,
            new DdsopRuntimePlanningInputPackage(
                identity,
                frozen,
                evidence,
                runtimeEvidence,
                executableInputs,
                BuildConsumerRules(),
                outputExpectations));
    }

    private static IReadOnlyList<DdsopParameterEvidenceRef> BuildParameterEvidence(DdsopOperatingModelConfiguration config)
    {
        return ParameterFieldGroups
            .Select(fieldGroup => new DdsopParameterEvidenceRef(
                fieldGroup,
                $"DDAE-PARAM-EVIDENCE-{fieldGroup}-{config.OperatingModelConfigurationID}",
                "DDAE approved operating model governance",
                CalculationStatusFor(fieldGroup),
                "PublicDemoOnly",
                "Applicable",
                null))
            .ToList();
    }

    private static string CalculationStatusFor(string fieldGroup)
    {
        return fieldGroup switch
        {
            "BufferZones" => "Calculated",
            "ControlPoint" or "TimeBuffer" or "ResourcePolicy" or "CalendarPolicy" or "ReleasePolicy" => "Derived",
            "DecouplingPoint" or "BufferProfile" or "UOM" => "Imported",
            "AdjustmentFactor" => "ManualGoverned",
            _ => "FixtureSeeded"
        };
    }

    private static DdsopRuntimeEvidenceSnapshot BuildRuntimeEvidence(DdsopOperatingModelConfiguration config, string dateToken)
    {
        var point = config.DDMRPConfiguration.DecouplingPoints.First();
        var profile = config.DDMRPConfiguration.StockBufferProfiles.First(item => item.BufferProfileID == point.BufferProfileID);
        var observedAt = config.EffectiveFrom;
        var dueAt = AddDays(config.EffectiveFrom, 7);
        var evidence = Evidence("RUNTIME", $"{point.ItemID}@{point.LocationID}", observedAt);
        var available = Math.Max(1m, profile.TopOfYellow);

        return new DdsopRuntimeEvidenceSnapshot(
            $"DDAE-OPS-SNAPSHOT-{dateToken}-PUBLIC-DEMO",
            observedAt,
            new[]
            {
                new DdsopRuntimeInventoryPosition(
                    point.ItemID,
                    point.LocationID,
                    profile.UnitOfMeasure,
                    available,
                    0m,
                    available,
                    "Released",
                    evidence)
            },
            new[]
            {
                new DdsopRuntimeDemandSignal(
                    $"DDAE-DEMAND-{dateToken}-001",
                    point.ItemID,
                    point.LocationID,
                    dueAt,
                    Math.Max(1m, point.MinimumOrderQty),
                    profile.UnitOfMeasure,
                    "WorkOrderDemand",
                    "NotApplicable",
                    "NotApplicable",
                    null,
                    evidence)
            },
            new[]
            {
                new DdsopRuntimeOpenSupplySignal(
                    $"DDAE-SUPPLY-{dateToken}-001",
                    point.ItemID,
                    point.LocationID,
                    AddDays(config.EffectiveFrom, 3),
                    Math.Max(1m, point.MinimumOrderQty),
                    profile.UnitOfMeasure,
                    "Planned",
                    evidence)
            },
            Evidence("QUALITY", $"{point.ItemID}@{point.LocationID}", observedAt));
    }

    private static DdsopExecutableSchedulingInputs BuildExecutableInputs(DdsopOperatingModelConfiguration config, string dateToken)
    {
        var part = config.SchedulingConfiguration.PartSchedulingSettings.First();
        var resource = config.SchedulingConfiguration.ResourceSettings.First();
        var operationId = $"OP-{Slug(part.ProductID)}-001";
        var evidence = Evidence("SCHEDULING", part.ProductID, config.EffectiveFrom, NonDdaeSchedulingEvidenceAuthority);
        var startAt = AddHours(config.EffectiveFrom, 8);
        var endAt = AddHours(config.EffectiveFrom, 16);
        var resourceCalendars = new List<DdsopRuntimeResourceCalendar>
        {
            BuildResourceCalendar(resource.ResourceID, resource.CalendarID, startAt, endAt, config.EffectiveFrom)
        };
        foreach (var alternateResourceId in part.AllowedAlternateResources ?? Array.Empty<string>())
        {
            if (resourceCalendars.All(item => item.ResourceID != alternateResourceId))
            {
                resourceCalendars.Add(BuildResourceCalendar(
                    alternateResourceId,
                    $"CAL-{alternateResourceId}",
                    startAt,
                    endAt,
                    config.EffectiveFrom));
            }
        }

        return new DdsopExecutableSchedulingInputs(
            $"DDAE-RUNTIME-MDV-{dateToken}-PUBLIC-DEMO",
            AdventureWorksAdapterProfileID,
            AdventureWorksCapacityUnitNormalizationRuleID,
            AdventureWorksMaterialConstraintsMode,
            AdventureWorksSetupChangeoverMode,
            new[]
            {
                new DdsopRuntimeWorkOrder(
                    $"DDAE-WO-{dateToken}-001",
                    part.ProductID,
                    part.PrimaryRoutingID,
                    1m,
                    "EA",
                    config.EffectiveFrom,
                    AddDays(config.EffectiveFrom, 7),
                    1,
                    evidence)
            },
            new[]
            {
                new DdsopRuntimeRouting(
                    part.PrimaryRoutingID,
                    part.ProductID,
                    new[] { operationId },
                    evidence)
            },
            new[]
            {
                new DdsopRuntimeOperation(
                    operationId,
                    1,
                    resource.ResourceID,
                    part.AllowedAlternateResources ?? Array.Empty<string>(),
                    60,
                    evidence)
            },
            resourceCalendars,
            Array.Empty<DdsopRuntimeMaterialConstraint>(),
            Evidence("SETUP", part.ProductID, config.EffectiveFrom, NonDdaeSchedulingEvidenceAuthority));
    }

    private static DdsopRuntimeResourceCalendar BuildResourceCalendar(
        string resourceId,
        string calendarId,
        string startAt,
        string endAt,
        string observedAt)
    {
        return new DdsopRuntimeResourceCalendar(
            resourceId,
            calendarId,
            new[] { new DdsopRuntimeCapacityWindow(startAt, endAt, 8m) },
            Evidence("CALENDAR", resourceId, observedAt, NonDdaeCalendarEvidenceAuthority));
    }

    private static DdsopRuntimeConsumerRules BuildConsumerRules()
    {
        return new DdsopRuntimeConsumerRules(
            new[]
            {
                "OPERATING_MODEL_CONFIGURATION_ID",
                "OPERATING_MODEL_FINGERPRINT",
                "SCHEDULING_CONFIGURATION_ID",
                "DDMRP_CONFIGURATION_ID",
                "DDSOP_DDMRP_MASTER_SETTINGS",
                "DDSOP_SCHEDULING_POLICY_SETTINGS",
                "DDMRP_BUFFER_TOPS",
                "DDMRP_DLT_MOQ_ORDER_CYCLE",
                "SCHEDULING_CONTROL_POINTS_TIME_BUFFERS"
            },
            new[]
            {
                "NET_FLOW_POSITION",
                "BUFFER_STATUS",
                "QUALIFIED_SPIKE_DEMAND",
                "MATERIAL_AVAILABILITY_STATUS",
                "RELEASE_AUTHORIZATION_STATE",
                "SCHEDULE_FEASIBILITY",
                "INFEASIBILITY_CAUSES",
                "DISPATCH_RECOMMENDATION"
            },
            new[]
            {
                "MUTATE_OPERATING_MODEL_CONFIGURATION_ID",
                "MUTATE_OPERATING_MODEL_FINGERPRINT",
                "RECALCULATE_DDAE_BUFFER_TOPS",
                "MUTATE_DDAE_DLT_MOQ_ORDER_CYCLE",
                "MUTATE_DDAE_SCHEDULING_POLICY",
                "PROMOTE_RUNTIME_FEEDBACK_TO_APPROVED_MASTER_SETTING"
            });
    }

    private static bool RequiresRuntimeEvidence(string executionMode)
    {
        return executionMode is "DDMRPExecution" or "DDMRPAndBoundedScheduling";
    }

    private static bool RequiresExecutableInputs(string executionMode)
    {
        return executionMode is "BoundedProductionScheduling" or "DDMRPAndBoundedScheduling";
    }

    private static IReadOnlyList<DdsopRuntimeEvidenceRef> Evidence(
        string category,
        string sourceRecordId,
        string observedAt,
        string sourceAuthority = DdaeGovernanceEvidenceAuthority)
    {
        return new[]
        {
            new DdsopRuntimeEvidenceRef(
                $"DDAE-{category}-EVIDENCE-{Slug(sourceRecordId)}",
                sourceAuthority,
                sourceRecordId,
                observedAt)
        };
    }

    private static string AddDays(string dateTime, int days)
    {
        return ParseDateTime(dateTime).AddDays(days).ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture);
    }

    private static string AddHours(string dateTime, int hours)
    {
        return ParseDateTime(dateTime).AddHours(hours).ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture);
    }

    private static DateTimeOffset ParseDateTime(string dateTime)
    {
        return DateTimeOffset.Parse(dateTime, CultureInfo.InvariantCulture);
    }

    private static string Slug(string value)
    {
        var chars = value
            .Select(ch => char.IsLetterOrDigit(ch) ? char.ToUpperInvariant(ch) : '-')
            .ToArray();
        return new string(chars).Trim('-');
    }
}

public sealed class DdsopRuntimeDeliveryLedger
{
    private readonly object _sync = new();
    private readonly Dictionary<string, DdsopRuntimeDeliveryLedgerRecord> _recordsByCorrelationId = new(StringComparer.Ordinal);

    public IReadOnlyList<DdsopRuntimeDeliveryLedgerRecord> Records
    {
        get
        {
            lock (_sync)
            {
                return _recordsByCorrelationId.Values.ToList();
            }
        }
    }

    public DdsopRuntimeDeliveryLedgerRecord RegisterPackage(DdsopRuntimePlanningInputMessage message)
    {
        var expectations = message.Payload.OutputExpectations;
        var frozen = message.Payload.FrozenDdsopConfiguration;
        lock (_sync)
        {
            if (_recordsByCorrelationId.TryGetValue(expectations.DeliveryLedgerCorrelationID, out var existing))
            {
                return existing;
            }

            var record = new DdsopRuntimeDeliveryLedgerRecord(
                expectations.RuntimePlanningInputPackageID,
                expectations.DeliveryLedgerCorrelationID,
                frozen.OperatingModelConfigurationID,
                frozen.OperatingModelFingerprint,
                message.OccurredAt,
                Array.Empty<DdsopRuntimeFeedbackCorrelation>());
            _recordsByCorrelationId[expectations.DeliveryLedgerCorrelationID] = record;
            return record;
        }
    }

    public DdsopRuntimeDeliveryLedgerRecord? CorrelateFeedback(string deliveryLedgerCorrelationId, DdsopFeedbackLedgerRecord feedbackRecord)
    {
        lock (_sync)
        {
            if (!_recordsByCorrelationId.TryGetValue(deliveryLedgerCorrelationId, out var existing))
            {
                return null;
            }

            var correlations = existing.FeedbackCorrelations.ToList();
            if (correlations.All(item => item.IdempotencyKey != feedbackRecord.IdempotencyKey))
            {
                correlations.Add(new DdsopRuntimeFeedbackCorrelation(
                    feedbackRecord.OriginalMessageID,
                    feedbackRecord.IdempotencyKey,
                    feedbackRecord.ProcessingStatus,
                    feedbackRecord.ReceivedAt,
                    feedbackRecord.Interpretation.FeedbackType ?? "Unknown",
                    feedbackRecord.Interpretation.PlanningRunID));
            }

            var updated = existing with { FeedbackCorrelations = correlations };
            _recordsByCorrelationId[deliveryLedgerCorrelationId] = updated;
            return updated;
        }
    }
}
