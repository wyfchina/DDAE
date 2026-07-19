using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace AdaptiveSopDdsop.Web.Domain;

public sealed class DdomChangePackageService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = false };
    private static readonly IReadOnlyDictionary<string, string> AllowedTransitions = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Submitted"] = "Reviewed",
        ["Reviewed"] = "Approved",
        ["Approved"] = "Effective",
        ["Effective"] = "Expired"
    };

    private readonly CurrentBaselineService _baselineService;
    private readonly MasterSettingsGovernanceService _governanceService;
    private readonly IScenarioRunLineageReader _lineageReader;
    private readonly string _databasePath;

    public DdomChangePackageService(
        CurrentBaselineService baselineService,
        MasterSettingsGovernanceService governanceService,
        IScenarioRunLineageReader lineageReader,
        string databasePath)
    {
        _baselineService = baselineService;
        _governanceService = governanceService;
        _lineageReader = lineageReader;
        _databasePath = databasePath;
        EnsureCreated();
    }

    public DdomChangePackageSummary Create(DdomChangePackageCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SourceScenarioRunId) || string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("DDOM 变更包必须提供已选定场景 run 和名称。", nameof(request));
        }

        var run = _lineageReader.GetSummary(request.SourceScenarioRunId.Trim())
            ?? throw new KeyNotFoundException("来源场景运行不存在。");
        EnsureSelectedFrozenRun(run);
        var baseline = _baselineService.GetDetail(run.BaselineSnapshotId!)
            ?? throw new KeyNotFoundException("来源冻结基线不存在。");
        var context = (request.GovernanceContext ?? new GovernanceDecisionContext()) with
        {
            SourceBaselineId = baseline.SnapshotId,
            SourceScenarioRunId = run.RunId
        };
        if (string.IsNullOrWhiteSpace(context.Approver))
        {
            throw new InvalidOperationException("DDOM 变更包必须在创建时指定审批人。");
        }
        var generated = _governanceService.ProposeFromSavedRun(run.RunId, baseline, context);
        var now = DateTimeOffset.UtcNow;
        var packageId = Guid.NewGuid().ToString("N");
        var createdBy = NormalizeActor(request.CreatedBy);
        var owner = string.IsNullOrWhiteSpace(context.Owner) ? createdBy : context.Owner.Trim();
        var approver = context.Approver.Trim();
        var finalRequestJson = Serialize(generated.Request);
        var finalParametersJson = Serialize(generated.Request.Parameters);
        var contextJson = Serialize(context);
        var fingerprint = Fingerprint(finalRequestJson);
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction(deferred: false);
        var packageNumber = $"DDOM-{now:yyyyMMdd}-{NextPackageSequence(connection, transaction, now):0000}";
        var summary = new DdomChangePackageSummary(
            packageId, packageNumber, request.Name.Trim(), baseline.SnapshotId, run.RunId,
            run.ExternalScenarioId!, run.ResponseId!, "Draft", "NotRun", run.FeasibilityStatus,
            owner, approver, createdBy, now.ToString("O"), null);
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO ddom_change_packages (
                    package_id, package_number, name, description, source_baseline_id, source_scenario_run_id,
                    external_scenario_id, response_id, status, validation_status, feasibility_status,
                    final_request_json, final_parameters_json, input_fingerprint, governance_context_json,
                    owner, approver, effective_from, effective_through, review_on, expected_effect, rollback_condition,
                    created_by, created_at_utc, validated_at_utc)
                VALUES (
                    $package_id, $package_number, $name, $description, $source_baseline_id, $source_scenario_run_id,
                    $external_scenario_id, $response_id, $status, $validation_status, $feasibility_status,
                    $final_request_json, $final_parameters_json, $input_fingerprint, $governance_context_json,
                    $owner, $approver, $effective_from, $effective_through, $review_on, $expected_effect, $rollback_condition,
                    $created_by, $created_at_utc, NULL);
                """;
            AddHeaderParameters(command, summary, request.Description, finalRequestJson, finalParametersJson, fingerprint, contextJson, context);
            command.ExecuteNonQuery();
        }

        var sequence = 1;
        foreach (var proposal in generated.Proposals)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO ddom_change_package_lines (line_id, package_id, sequence, setting_type, target, current_value, proposed_value, proposal_json)
                VALUES ($line_id, $package_id, $sequence, $setting_type, $target, $current_value, $proposed_value, $proposal_json);
                """;
            command.Parameters.AddWithValue("$line_id", Guid.NewGuid().ToString("N"));
            command.Parameters.AddWithValue("$package_id", packageId);
            command.Parameters.AddWithValue("$sequence", sequence++);
            command.Parameters.AddWithValue("$setting_type", proposal.SettingType);
            command.Parameters.AddWithValue("$target", proposal.Target);
            command.Parameters.AddWithValue("$current_value", proposal.CurrentValue);
            command.Parameters.AddWithValue("$proposed_value", proposal.ProposedValue);
            command.Parameters.AddWithValue("$proposal_json", Serialize(proposal));
            command.ExecuteNonQuery();
        }
        InsertAudit(connection, transaction, packageId, "PackageCreated", "Governance", "Information", "DDOM 变更包已创建为 Draft。", Serialize(new { actor = createdBy, note = NormalizeNote(request.Description), description = request.Description, runId = run.RunId, fingerprint }), now);
        transaction.Commit();
        return summary;
    }

    public IReadOnlyList<DdomChangePackageSummary> List(int limit = 50)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT package_id, package_number, name, source_baseline_id, source_scenario_run_id, external_scenario_id, response_id,
                   status, validation_status, feasibility_status, owner, approver, created_by, created_at_utc, validated_at_utc
            FROM ddom_change_packages ORDER BY created_at_utc DESC LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit <= 0 ? 50 : limit, 1, 200));
        using var reader = command.ExecuteReader();
        var results = new List<DdomChangePackageSummary>();
        while (reader.Read()) results.Add(ReadSummary(reader));
        return results;
    }

    public bool Exists(string packageId)
    {
        if (string.IsNullOrWhiteSpace(packageId)) return false;
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT package_id FROM ddom_change_packages WHERE package_id = $package_id LIMIT 1;";
        command.Parameters.AddWithValue("$package_id", packageId.Trim());
        return command.ExecuteScalar() is not null;
    }

    internal IReadOnlyList<DdomChangePackageSummary> ListByBaseline(string baselineSnapshotId)
    {
        if (string.IsNullOrWhiteSpace(baselineSnapshotId))
        {
            throw new ArgumentException("冻结基线标识不能为空。", nameof(baselineSnapshotId));
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT package_id, package_number, name, source_baseline_id, source_scenario_run_id, external_scenario_id, response_id,
                   status, validation_status, feasibility_status, owner, approver, created_by, created_at_utc, validated_at_utc
            FROM ddom_change_packages
            WHERE source_baseline_id = $source_baseline_id
            ORDER BY created_at_utc DESC, package_id DESC;
            """;
        command.Parameters.AddWithValue("$source_baseline_id", baselineSnapshotId.Trim());
        using var reader = command.ExecuteReader();
        var results = new List<DdomChangePackageSummary>();
        while (reader.Read()) results.Add(ReadSummary(reader));
        return results;
    }

    public DdomChangePackageDetail? GetDetail(string packageId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT package_id, package_number, name, source_baseline_id, source_scenario_run_id, external_scenario_id, response_id,
                   status, validation_status, feasibility_status, owner, approver, created_by, created_at_utc, validated_at_utc,
                   description, final_request_json, final_parameters_json, input_fingerprint, governance_context_json
            FROM ddom_change_packages WHERE package_id = $package_id;
            """;
        command.Parameters.AddWithValue("$package_id", packageId);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        var summary = ReadSummary(reader);
        var finalRequest = Deserialize<ScenarioRunPreviewRequest>(reader.GetString(16)) ?? new ScenarioRunPreviewRequest();
        var finalParameters = Deserialize<ScenarioRunParameterSet>(reader.GetString(17));
        var context = Deserialize<GovernanceDecisionContext>(reader.GetString(19)) ?? new GovernanceDecisionContext();
        var lines = GetLines(connection, packageId);
        return new DdomChangePackageDetail(summary, reader.IsDBNull(15) ? null : reader.GetString(15), finalRequest, finalParameters, reader.GetString(18), context, lines, GetLatestValidation(connection, packageId));
    }

    public IReadOnlyList<DdomChangePackageAuditEvent> GetAuditEvents(string packageId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT event_id, package_id, sequence, event_type, stage, severity, message, payload_json, created_at_utc
            FROM ddom_change_package_audit_events WHERE package_id = $package_id ORDER BY sequence;
            """;
        command.Parameters.AddWithValue("$package_id", packageId);
        using var reader = command.ExecuteReader();
        var results = new List<DdomChangePackageAuditEvent>();
        while (reader.Read()) results.Add(new DdomChangePackageAuditEvent(reader.GetString(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.IsDBNull(7) ? null : reader.GetString(7), reader.GetString(8)));
        return results;
    }

    private static DdomChangePackageValidation? GetLatestValidation(SqliteConnection connection, string packageId)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT validation_id, package_id, validation_status, feasibility_status, input_fingerprint,
                   failure_reasons_json, validated_by, validated_at_utc
            FROM ddom_change_package_validations WHERE package_id = $package_id
            ORDER BY validated_at_utc DESC, validation_id DESC LIMIT 1;
            """;
        command.Parameters.AddWithValue("$package_id", packageId);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        var reasons = Deserialize<IReadOnlyList<string>>(reader.GetString(5)) ?? Array.Empty<string>();
        return new DdomChangePackageValidation(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reasons, reader.GetString(6), reader.GetString(7));
    }

    public DdomChangePackageSummary Submit(string packageId, DdomPackageActionRequest request)
    {
        var header = RequireHeader(packageId);
        if (header.Summary.Status != "Draft") throw new InvalidOperationException("只有 Draft 变更包可以提交。");
        return UpdateHeaderStatus(header, "Submitted", NormalizeActor(request.UpdatedBy), request.Note, "PackageSubmitted", "提交评审");
    }

    public DdomChangePackageValidation Validate(string packageId, DdomPackageActionRequest request)
    {
        var header = RequireHeader(packageId);
        if (header.Summary.Status != "Submitted") throw new InvalidOperationException("只有已提交的变更包可以运行白盒验证。");
        if (header.Summary.ValidationStatus != "NotRun") throw new InvalidOperationException("当前不可重复运行白盒验证；请基于修订场景创建新变更包。");
        var baseline = _baselineService.GetDetail(header.Summary.SourceBaselineId)
            ?? throw new KeyNotFoundException("来源冻结基线不存在。");
        var finalRequest = Deserialize<ScenarioRunPreviewRequest>(header.FinalRequestJson)
            ?? throw new InvalidOperationException("包最终请求无法恢复，不能验证。");
        if (!string.Equals(Fingerprint(header.FinalRequestJson), header.Fingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("包最终输入已变化，不能使用过期验证。 ");
        var preview = _governanceService.PreviewFrozenPackageRequest(finalRequest, baseline);
        var feasibility = preview.Feasibility ?? throw new InvalidOperationException("白盒预览未产生可行性结论。");
        var validationStatus = feasibility.Status is "Adoptable" or "Reconcile" ? "Passed" : "Failed";
        var reasons = feasibility.Checks.Where(item => item.Status == "Red").Select(item => item.Message).ToList();
        if (reasons.Count == 0 && validationStatus == "Failed") reasons.Add("白盒可行性验证未通过。");
        var now = DateTimeOffset.UtcNow;
        var actor = NormalizeActor(request.UpdatedBy);
        var note = NormalizeNote(request.Note);
        var validation = new DdomChangePackageValidation(Guid.NewGuid().ToString("N"), packageId, validationStatus, feasibility.Status, header.Fingerprint, reasons, actor, now.ToString("O"));
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction(deferred: false);
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "UPDATE ddom_change_packages SET validation_status = $validation_status, feasibility_status = $feasibility_status, validated_at_utc = $validated_at_utc WHERE package_id = $package_id AND status = $expected_status AND validation_status = $expected_validation_status;";
            command.Parameters.AddWithValue("$validation_status", validation.ValidationStatus);
            command.Parameters.AddWithValue("$feasibility_status", validation.FeasibilityStatus);
            command.Parameters.AddWithValue("$validated_at_utc", validation.ValidatedAtUtc);
            command.Parameters.AddWithValue("$package_id", packageId);
            command.Parameters.AddWithValue("$expected_status", "Submitted");
            command.Parameters.AddWithValue("$expected_validation_status", "NotRun");
            if (command.ExecuteNonQuery() != 1)
                throw new InvalidOperationException("变更包状态已被其他操作更新，请刷新后重试。");
        }
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO ddom_change_package_validations (
                    validation_id, package_id, input_fingerprint, request_json, result_json, feasibility_status,
                    validation_status, failure_reasons_json, trace_json, validated_by, validated_at_utc)
                VALUES ($validation_id, $package_id, $input_fingerprint, $request_json, $result_json, $feasibility_status,
                    $validation_status, $failure_reasons_json, $trace_json, $validated_by, $validated_at_utc);
                """;
            command.Parameters.AddWithValue("$validation_id", validation.ValidationId);
            command.Parameters.AddWithValue("$package_id", packageId);
            command.Parameters.AddWithValue("$input_fingerprint", validation.InputFingerprint);
            command.Parameters.AddWithValue("$request_json", Serialize(preview.Request));
            command.Parameters.AddWithValue("$result_json", Serialize(preview));
            command.Parameters.AddWithValue("$feasibility_status", validation.FeasibilityStatus);
            command.Parameters.AddWithValue("$validation_status", validation.ValidationStatus);
            command.Parameters.AddWithValue("$failure_reasons_json", Serialize(validation.FailureReasons));
            command.Parameters.AddWithValue("$trace_json", Serialize(preview.Trace));
            command.Parameters.AddWithValue("$validated_by", validation.ValidatedBy);
            command.Parameters.AddWithValue("$validated_at_utc", validation.ValidatedAtUtc);
            command.ExecuteNonQuery();
        }
        InsertAudit(connection, transaction, packageId, "WhiteBoxRecalculated", "Engine", "Information", "已从冻结基线和保存 run 白盒复算。", Serialize(new { actor, note, validation.ValidationId }), now);
        InsertAudit(connection, transaction, packageId, validationStatus == "Passed" ? "ValidationPassed" : "ValidationFailed", "Validation", validationStatus == "Passed" ? "Information" : "Warning", validationStatus == "Passed" ? "白盒验证通过，等待人工评审。" : "白盒验证失败，包保持 Submitted。", Serialize(new { actor, note, validation }), now);
        transaction.Commit();
        return validation;
    }

    public DdomChangePackageSummary UpdateStatus(string packageId, DdomPackageStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Status)) throw new ArgumentException("目标状态不能为空。", nameof(request));
        var header = RequireHeader(packageId);
        var target = request.Status.Trim();
        if (!AllowedTransitions.TryGetValue(header.Summary.Status, out var allowed) || !string.Equals(allowed, target, StringComparison.Ordinal))
            throw new InvalidOperationException($"状态只能从 {header.Summary.Status} 流转到 {allowed ?? "终态"}。");
        if (target is "Reviewed" or "Approved" or "Effective") EnsureValidationGate(header);
        var actor = NormalizeActor(request.UpdatedBy, requireExplicit: target == "Approved");
        if (target == "Approved" && string.IsNullOrWhiteSpace(header.Summary.Approver))
            throw new InvalidOperationException("DDOM 变更包缺少配置审批人，不能批准。");
        if (target == "Approved" && !string.Equals(actor, header.Summary.Approver, StringComparison.Ordinal))
            throw new InvalidOperationException($"批准人必须是配置的审批人：{header.Summary.Approver}。");
        if (target == "Effective") EnsureEffectiveMetadata(header.Context);
        var eventType = target switch
        {
            "Reviewed" => "PackageReviewed",
            "Approved" => "PackageApproved",
            "Effective" => "PackageEffective",
            "Expired" => "PackageExpired",
            _ => "PackageStatusChanged"
        };
        return UpdateHeaderStatus(header, target, actor, request.Note, eventType, $"人工状态流转为 {target}");
    }

    private Header RequireHeader(string packageId)
    {
        if (string.IsNullOrWhiteSpace(packageId)) throw new ArgumentException("变更包标识不能为空。", nameof(packageId));
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT package_id, package_number, name, source_baseline_id, source_scenario_run_id, external_scenario_id, response_id,
                   status, validation_status, feasibility_status, owner, approver, created_by, created_at_utc, validated_at_utc,
                   description, final_request_json, final_parameters_json, input_fingerprint, governance_context_json
            FROM ddom_change_packages WHERE package_id = $package_id;
            """;
        command.Parameters.AddWithValue("$package_id", packageId);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) throw new KeyNotFoundException("DDOM 变更包不存在。");
        return new Header(ReadSummary(reader), reader.GetString(16), reader.GetString(17), reader.GetString(18), Deserialize<GovernanceDecisionContext>(reader.GetString(19)) ?? new GovernanceDecisionContext());
    }

    private DdomChangePackageSummary UpdateHeaderStatus(Header header, string status, string actor, string? note, string eventType, string message)
    {
        var now = DateTimeOffset.UtcNow;
        var normalizedNote = NormalizeNote(note);
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction(deferred: false);
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "UPDATE ddom_change_packages SET status = $status WHERE package_id = $package_id AND status = $expected_status;";
            command.Parameters.AddWithValue("$status", status);
            command.Parameters.AddWithValue("$package_id", header.Summary.PackageId);
            command.Parameters.AddWithValue("$expected_status", header.Summary.Status);
            if (command.ExecuteNonQuery() != 1)
                throw new InvalidOperationException("变更包状态已被其他操作更新，请刷新后重试。");
        }
        InsertAudit(connection, transaction, header.Summary.PackageId, eventType, "Governance", "Information", $"{message}。操作者：{actor}。{normalizedNote ?? string.Empty}".Trim(), Serialize(new { actor, note = normalizedNote, status }), now);
        transaction.Commit();
        return header.Summary with { Status = status };
    }

    private void EnsureValidationGate(Header header)
    {
        if (header.Summary.ValidationStatus != "Passed" || header.Summary.FeasibilityStatus is not ("Adoptable" or "Reconcile"))
            throw new InvalidOperationException("最新白盒验证尚未通过，不能推进治理状态。");
        if (!string.Equals(Fingerprint(header.FinalRequestJson), header.Fingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("包最终输入已变化，与最新验证指纹不一致。");
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT input_fingerprint FROM ddom_change_package_validations WHERE package_id = $package_id ORDER BY validated_at_utc DESC, validation_id DESC LIMIT 1;";
        command.Parameters.AddWithValue("$package_id", header.Summary.PackageId);
        var latest = command.ExecuteScalar() as string;
        if (!string.Equals(latest, header.Fingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("最新验证不对应当前包输入指纹。");
    }

    private static void EnsureSelectedFrozenRun(ScenarioRunSummary run)
    {
        if (run.Status != "Saved" || run.CandidateStatus != "Selected")
            throw new InvalidOperationException("DDOM 变更包只能来自已选定且已保存的场景 run。");
        if (string.IsNullOrWhiteSpace(run.BaselineSnapshotId) || string.IsNullOrWhiteSpace(run.ExternalScenarioId) || string.IsNullOrWhiteSpace(run.ResponseId))
            throw new InvalidOperationException("已选定场景 run 缺少冻结比较血缘。");
    }

    private static void EnsureEffectiveMetadata(GovernanceDecisionContext context)
    {
        if (string.IsNullOrWhiteSpace(context.EffectiveFrom) || string.IsNullOrWhiteSpace(context.ReviewOn) || string.IsNullOrWhiteSpace(context.RollbackCondition))
            throw new InvalidOperationException("生效需要完整的生效日期、复查日期和回滚条件。");
    }

    private static string NormalizeActor(string? actor, bool requireExplicit = false)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            if (requireExplicit) throw new InvalidOperationException("该治理操作必须提供显式操作者。");
            return "计划员";
        }
        return actor.Trim();
    }

    private static string? NormalizeNote(string? note) => string.IsNullOrWhiteSpace(note) ? null : note.Trim();

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);
    private static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, JsonOptions);
    private static string Fingerprint(string canonicalWebJson) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalWebJson))).ToLowerInvariant();

    private IReadOnlyList<DdomChangePackageLine> GetLines(SqliteConnection connection, string packageId)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT line_id, package_id, sequence, proposal_json FROM ddom_change_package_lines WHERE package_id = $package_id ORDER BY sequence;";
        command.Parameters.AddWithValue("$package_id", packageId);
        using var reader = command.ExecuteReader();
        var lines = new List<DdomChangePackageLine>();
        while (reader.Read())
        {
            var proposal = Deserialize<MasterSettingChangeRequest>(reader.GetString(3));
            if (proposal is not null) lines.Add(new DdomChangePackageLine(reader.GetString(0), reader.GetString(1), reader.GetInt32(2), proposal));
        }
        return lines;
    }

    private static DdomChangePackageSummary ReadSummary(SqliteDataReader reader) => new(
        reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6),
        reader.GetString(7), reader.GetString(8), reader.GetString(9), reader.GetString(10), reader.GetString(11), reader.GetString(12), reader.GetString(13), reader.IsDBNull(14) ? null : reader.GetString(14));

    private static void AddHeaderParameters(SqliteCommand command, DdomChangePackageSummary summary, string? description, string finalRequestJson, string finalParametersJson, string fingerprint, string contextJson, GovernanceDecisionContext context)
    {
        command.Parameters.AddWithValue("$package_id", summary.PackageId); command.Parameters.AddWithValue("$package_number", summary.PackageNumber); command.Parameters.AddWithValue("$name", summary.Name); command.Parameters.AddWithValue("$description", (object?)description?.Trim() ?? DBNull.Value);
        command.Parameters.AddWithValue("$source_baseline_id", summary.SourceBaselineId); command.Parameters.AddWithValue("$source_scenario_run_id", summary.SourceScenarioRunId); command.Parameters.AddWithValue("$external_scenario_id", summary.ExternalScenarioId); command.Parameters.AddWithValue("$response_id", summary.ResponseId);
        command.Parameters.AddWithValue("$status", summary.Status); command.Parameters.AddWithValue("$validation_status", summary.ValidationStatus); command.Parameters.AddWithValue("$feasibility_status", summary.FeasibilityStatus); command.Parameters.AddWithValue("$final_request_json", finalRequestJson); command.Parameters.AddWithValue("$final_parameters_json", finalParametersJson); command.Parameters.AddWithValue("$input_fingerprint", fingerprint); command.Parameters.AddWithValue("$governance_context_json", contextJson);
        command.Parameters.AddWithValue("$owner", summary.Owner); command.Parameters.AddWithValue("$approver", summary.Approver); command.Parameters.AddWithValue("$effective_from", (object?)context.EffectiveFrom ?? DBNull.Value); command.Parameters.AddWithValue("$effective_through", (object?)context.EffectiveThrough ?? DBNull.Value); command.Parameters.AddWithValue("$review_on", (object?)context.ReviewOn ?? DBNull.Value); command.Parameters.AddWithValue("$expected_effect", (object?)context.ExpectedEffect ?? DBNull.Value); command.Parameters.AddWithValue("$rollback_condition", (object?)context.RollbackCondition ?? DBNull.Value);
        command.Parameters.AddWithValue("$created_by", summary.CreatedBy); command.Parameters.AddWithValue("$created_at_utc", summary.CreatedAtUtc);
    }

    private static void InsertAudit(SqliteConnection connection, SqliteTransaction transaction, string packageId, string eventType, string stage, string severity, string message, string? payload, DateTimeOffset createdAt)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO ddom_change_package_audit_events (event_id, package_id, sequence, event_type, stage, severity, message, payload_json, created_at_utc)
            VALUES ($event_id, $package_id, (SELECT COALESCE(MAX(sequence), 0) + 1 FROM ddom_change_package_audit_events WHERE package_id = $package_id), $event_type, $stage, $severity, $message, $payload_json, $created_at_utc);
            """;
        command.Parameters.AddWithValue("$event_id", Guid.NewGuid().ToString("N")); command.Parameters.AddWithValue("$package_id", packageId); command.Parameters.AddWithValue("$event_type", eventType); command.Parameters.AddWithValue("$stage", stage); command.Parameters.AddWithValue("$severity", severity); command.Parameters.AddWithValue("$message", message); command.Parameters.AddWithValue("$payload_json", (object?)payload ?? DBNull.Value); command.Parameters.AddWithValue("$created_at_utc", createdAt.ToString("O"));
        command.ExecuteNonQuery();
    }

    private static int NextPackageSequence(SqliteConnection connection, SqliteTransaction transaction, DateTimeOffset now)
    {
        var sequenceDate = now.ToString("yyyyMMdd");
        var prefix = $"DDOM-{sequenceDate}-";
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO ddom_change_package_number_sequences (sequence_date, next_value)
            VALUES (
                $sequence_date,
                (SELECT COALESCE(MAX(CAST(substr(package_number, length($prefix) + 1) AS INTEGER)), 0) + 2
                 FROM ddom_change_packages WHERE package_number LIKE $prefix || '%'))
            ON CONFLICT(sequence_date) DO UPDATE SET next_value = next_value + 1
            RETURNING next_value - 1;
            """;
        command.Parameters.AddWithValue("$sequence_date", sequenceDate);
        command.Parameters.AddWithValue("$prefix", prefix);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _databasePath, DefaultTimeout = 30 }.ToString()); connection.Open(); return connection;
    }

    private void EnsureCreated()
    {
        var directory = Path.GetDirectoryName(_databasePath); if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        using var connection = OpenConnection(); using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS ddom_change_packages (
                package_id TEXT PRIMARY KEY, package_number TEXT NOT NULL UNIQUE, name TEXT NOT NULL, description TEXT NULL,
                source_baseline_id TEXT NOT NULL, source_scenario_run_id TEXT NOT NULL, external_scenario_id TEXT NOT NULL, response_id TEXT NOT NULL,
                status TEXT NOT NULL, validation_status TEXT NOT NULL, feasibility_status TEXT NOT NULL,
                final_request_json TEXT NOT NULL, final_parameters_json TEXT NOT NULL, input_fingerprint TEXT NOT NULL, governance_context_json TEXT NOT NULL,
                owner TEXT NOT NULL, approver TEXT NOT NULL, effective_from TEXT NULL, effective_through TEXT NULL, review_on TEXT NULL, expected_effect TEXT NULL, rollback_condition TEXT NULL,
                created_by TEXT NOT NULL, created_at_utc TEXT NOT NULL, validated_at_utc TEXT NULL
            );
            CREATE TABLE IF NOT EXISTS ddom_change_package_number_sequences (
                sequence_date TEXT PRIMARY KEY, next_value INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS ddom_change_package_lines (
                line_id TEXT PRIMARY KEY, package_id TEXT NOT NULL, sequence INTEGER NOT NULL, setting_type TEXT NOT NULL, target TEXT NOT NULL,
                current_value TEXT NOT NULL, proposed_value TEXT NOT NULL, proposal_json TEXT NOT NULL,
                FOREIGN KEY(package_id) REFERENCES ddom_change_packages(package_id)
            );
            CREATE TABLE IF NOT EXISTS ddom_change_package_validations (
                validation_id TEXT PRIMARY KEY, package_id TEXT NOT NULL, input_fingerprint TEXT NOT NULL, request_json TEXT NOT NULL, result_json TEXT NOT NULL,
                feasibility_status TEXT NOT NULL, validation_status TEXT NOT NULL, failure_reasons_json TEXT NOT NULL, trace_json TEXT NOT NULL,
                validated_by TEXT NOT NULL, validated_at_utc TEXT NOT NULL, FOREIGN KEY(package_id) REFERENCES ddom_change_packages(package_id)
            );
            CREATE TABLE IF NOT EXISTS ddom_change_package_audit_events (
                event_id TEXT PRIMARY KEY, package_id TEXT NOT NULL, sequence INTEGER NOT NULL, event_type TEXT NOT NULL, stage TEXT NOT NULL,
                severity TEXT NOT NULL, message TEXT NOT NULL, payload_json TEXT NULL, created_at_utc TEXT NOT NULL,
                FOREIGN KEY(package_id) REFERENCES ddom_change_packages(package_id)
            );
            CREATE INDEX IF NOT EXISTS ix_ddom_change_packages_created_at ON ddom_change_packages(created_at_utc DESC);
            CREATE INDEX IF NOT EXISTS ix_ddom_change_package_lines_sequence ON ddom_change_package_lines(package_id, sequence);
            CREATE INDEX IF NOT EXISTS ix_ddom_change_package_validations_latest ON ddom_change_package_validations(package_id, validated_at_utc DESC);
            CREATE INDEX IF NOT EXISTS ix_ddom_change_package_audit_sequence ON ddom_change_package_audit_events(package_id, sequence);
            """;
        command.ExecuteNonQuery();
    }

    private sealed record Header(DdomChangePackageSummary Summary, string FinalRequestJson, string FinalParametersJson, string Fingerprint, GovernanceDecisionContext Context);
}
