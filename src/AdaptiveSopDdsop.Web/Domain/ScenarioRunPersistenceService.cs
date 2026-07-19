using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace AdaptiveSopDdsop.Web.Domain;

public interface IScenarioRunLineageReader
{
    ScenarioRunSummary? GetSummary(string runId);
    ScenarioRunDetail? GetDetail(string runId);
}

public sealed class ScenarioRunPersistenceService : IScenarioRunLineageReader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly string _databasePath;
    private readonly ScenarioRunPreviewService _previewService;
    private readonly ScenarioComparisonService? _comparisonService;

    public ScenarioRunPersistenceService(ScenarioRunPreviewService previewService, string databasePath)
    {
        _previewService = previewService;
        _databasePath = databasePath;
        EnsureCreated();
    }

    public ScenarioRunPersistenceService(
        ScenarioRunPreviewService previewService,
        ScenarioComparisonService comparisonService,
        string databasePath)
        : this(previewService, databasePath)
    {
        _comparisonService = comparisonService;
    }

    public ScenarioRunSaveResponse Save(ScenarioRunSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("场景名称不能为空。", nameof(request));
        }

        var runId = Guid.NewGuid().ToString("N");
        var createdAt = DateTimeOffset.UtcNow;
        var createdAtText = createdAt.ToString("O");
        var runNumber = $"SR-{createdAt:yyyyMMdd}-{NextSequence():0000}";
        var createdBy = string.IsNullOrWhiteSpace(request.CreatedBy) ? "计划员" : request.CreatedBy.Trim();
        var preview = _previewService.Preview(request.PreviewRequest) with { IsPersisted = true };
        EnsurePhysicalInventoryEvidence(preview);
        var summary = BuildSummary(runId, runNumber, request, createdBy, createdAtText, preview);
        var requestJson = JsonSerializer.Serialize(request.PreviewRequest, JsonOptions);
        var resultJson = JsonSerializer.Serialize(preview, JsonOptions);
        var auditEvents = BuildAuditEvents(runId, requestJson, preview, createdAt);

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO scenario_runs (
                    run_id, run_number, name, description, created_by, status, approval_status, created_at_utc,
                    horizon_weeks, template_id, adoption_constraint_mode, request_json, result_json,
                    service_level_percent, flow_index, average_inventory_value, peak_load_percent,
                    supply_gap, red_sku_count, replenishment_order_count,
                    baseline_snapshot_id, external_scenario_id, response_id, feasibility_status, candidate_status,
                    selected_by, selected_at_utc, selection_note)
                VALUES (
                    $run_id, $run_number, $name, $description, $created_by, $status, $approval_status, $created_at_utc,
                    $horizon_weeks, $template_id, $adoption_constraint_mode, $request_json, $result_json,
                    $service_level_percent, $flow_index, $average_inventory_value, $peak_load_percent,
                    $supply_gap, $red_sku_count, $replenishment_order_count,
                    $baseline_snapshot_id, $external_scenario_id, $response_id, $feasibility_status, $candidate_status,
                    $selected_by, $selected_at_utc, $selection_note);
                """;
            AddParameters(command, summary);
            command.Parameters.AddWithValue("$request_json", requestJson);
            command.Parameters.AddWithValue("$result_json", resultJson);
            command.ExecuteNonQuery();
        }

        foreach (var auditEvent in auditEvents)
        {
            InsertAuditEvent(connection, transaction, auditEvent);
        }

        transaction.Commit();
        return new ScenarioRunSaveResponse(runId, runNumber, "Saved", "NotSubmitted", true, summary);
    }

    public ScenarioRunSaveResponse SaveFrozenComparison(ScenarioComparisonSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("场景名称不能为空。", nameof(request));
        }
        if (string.IsNullOrWhiteSpace(request.ResponseId))
        {
            throw new ArgumentException("响应方案标识不能为空。", nameof(request));
        }

        var comparisonService = _comparisonService
            ?? throw new InvalidOperationException("冻结比较保存需要后端场景比较服务。");
        var comparison = comparisonService.Compare(request.Comparison);
        var selectedCase = comparison.AllCases.SingleOrDefault(item =>
            string.Equals(item.ResponseId, request.ResponseId, StringComparison.Ordinal));
        if (selectedCase is null)
        {
            throw new ArgumentException("所选响应方案不存在于本次冻结比较。", nameof(request));
        }

        var runId = Guid.NewGuid().ToString("N");
        var createdAt = DateTimeOffset.UtcNow;
        var createdAtText = createdAt.ToString("O");
        var runNumber = $"SR-{createdAt:yyyyMMdd}-{NextSequence():0000}";
        var createdBy = string.IsNullOrWhiteSpace(request.CreatedBy) ? "计划员" : request.CreatedBy.Trim();
        var preview = selectedCase.Preview with { IsPersisted = true };
        EnsurePhysicalInventoryEvidence(preview);
        var saveRequest = new ScenarioRunSaveRequest(
            request.Name,
            request.Description,
            request.CreatedBy,
            preview.Request);
        var summary = BuildSummary(runId, runNumber, saveRequest, createdBy, createdAtText, preview) with
        {
            BaselineSnapshotId = comparison.BaselineSnapshotId,
            ExternalScenarioId = selectedCase.ExternalScenarioId,
            ResponseId = selectedCase.ResponseId
        };
        var requestJson = JsonSerializer.Serialize(preview.Request, JsonOptions);
        var resultJson = JsonSerializer.Serialize(preview, JsonOptions);
        var auditEvents = BuildAuditEvents(runId, requestJson, preview, createdAt);

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO scenario_runs (
                    run_id, run_number, name, description, created_by, status, approval_status, created_at_utc,
                    horizon_weeks, template_id, adoption_constraint_mode, request_json, result_json,
                    service_level_percent, flow_index, average_inventory_value, peak_load_percent,
                    supply_gap, red_sku_count, replenishment_order_count,
                    baseline_snapshot_id, external_scenario_id, response_id, feasibility_status, candidate_status,
                    selected_by, selected_at_utc, selection_note)
                VALUES (
                    $run_id, $run_number, $name, $description, $created_by, $status, $approval_status, $created_at_utc,
                    $horizon_weeks, $template_id, $adoption_constraint_mode, $request_json, $result_json,
                    $service_level_percent, $flow_index, $average_inventory_value, $peak_load_percent,
                    $supply_gap, $red_sku_count, $replenishment_order_count,
                    $baseline_snapshot_id, $external_scenario_id, $response_id, $feasibility_status, $candidate_status,
                    $selected_by, $selected_at_utc, $selection_note);
                """;
            AddParameters(command, summary);
            command.Parameters.AddWithValue("$request_json", requestJson);
            command.Parameters.AddWithValue("$result_json", resultJson);
            command.ExecuteNonQuery();
        }

        foreach (var auditEvent in auditEvents)
        {
            InsertAuditEvent(connection, transaction, auditEvent);
        }

        transaction.Commit();
        return new ScenarioRunSaveResponse(runId, runNumber, "Saved", "NotSubmitted", true, summary);
    }

    public ScenarioCandidateSelectionResponse UpdateCandidateStatus(
        string runId,
        ScenarioCandidateSelectionRequest request)
    {
        if (string.IsNullOrWhiteSpace(runId))
        {
            throw new ArgumentException("场景运行标识不能为空。", nameof(runId));
        }
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            throw new ArgumentException("候选方案状态不能为空。", nameof(request));
        }

        var requestedStatus = request.Status.Trim();
        var actor = string.IsNullOrWhiteSpace(request.UpdatedBy) ? "计划员" : request.UpdatedBy.Trim();
        var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        var now = DateTimeOffset.UtcNow;
        var nowText = now.ToString("O");

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        var current = GetSummary(connection, transaction, runId)
            ?? throw new KeyNotFoundException("未找到场景运行记录。");

        if (requestedStatus == "Selected")
        {
            EnsureCanSelect(current);
            var selectedSiblings = GetSelectedSiblings(connection, transaction, current);
            foreach (var sibling in selectedSiblings)
            {
                UpdateCandidateStatus(connection, transaction, sibling.RunId, "Superseded", null, null, null);
                InsertSelectionAuditEvent(connection, transaction, sibling.RunId, "CandidateSuperseded", actor, note, nowText,
                    "同一基线和外部场景下已选候选方案被人工选择的新方案替代。");
            }

            UpdateCandidateStatus(connection, transaction, current.RunId, "Selected", actor, nowText, note);
            InsertSelectionAuditEvent(connection, transaction, current.RunId, "CandidateSelected", actor, note, nowText,
                "候选方案已由人工明确选定。");
            transaction.Commit();
            return new ScenarioCandidateSelectionResponse(
                current with { CandidateStatus = "Selected", SelectedBy = actor, SelectedAtUtc = nowText, SelectionNote = note },
                true);
        }

        if (requestedStatus == "Withdrawn")
        {
            if (current.CandidateStatus != "Selected")
            {
                throw new ArgumentException("只有已选定的候选方案可以撤回。", nameof(request));
            }

            UpdateCandidateStatus(connection, transaction, current.RunId, "Withdrawn", null, null, null);
            InsertSelectionAuditEvent(connection, transaction, current.RunId, "CandidateWithdrawn", actor, note, nowText,
                "已选定候选方案已由人工撤回。");
            transaction.Commit();
            return new ScenarioCandidateSelectionResponse(current with { CandidateStatus = "Withdrawn" }, true);
        }

        throw new ArgumentException("候选方案只能人工选定或从已选定状态撤回。", nameof(request));
    }

    public IReadOnlyList<ScenarioRunSummary> List(
        int limit,
        string? baselineSnapshotId = null,
        string? externalScenarioId = null)
    {
        var boundedLimit = Math.Clamp(limit <= 0 ? 50 : limit, 1, 200);
        return QueryList(boundedLimit, baselineSnapshotId, externalScenarioId);
    }

    internal IReadOnlyList<ScenarioRunSummary> ListByLineage(
        string? baselineSnapshotId = null,
        string? externalScenarioId = null) =>
        QueryList(null, baselineSnapshotId, externalScenarioId);

    private IReadOnlyList<ScenarioRunSummary> QueryList(
        int? limit,
        string? baselineSnapshotId,
        string? externalScenarioId)
    {
        var baselineFilter = NormalizeFilter(baselineSnapshotId);
        var externalScenarioFilter = NormalizeFilter(externalScenarioId);
        var predicates = new List<string>();
        if (baselineFilter is not null)
        {
            predicates.Add("baseline_snapshot_id = $baseline_snapshot_id");
        }
        if (externalScenarioFilter is not null)
        {
            predicates.Add("external_scenario_id = $external_scenario_id");
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        var whereClause = predicates.Count == 0
            ? string.Empty
            : $"WHERE {string.Join(" AND ", predicates)}";
        var limitClause = limit.HasValue ? "LIMIT $limit" : string.Empty;
        command.CommandText = $"""
            SELECT run_id, run_number, name, description, created_by, status, approval_status, created_at_utc,
                   horizon_weeks, template_id, adoption_constraint_mode, service_level_percent, flow_index,
                   average_inventory_value, peak_load_percent, supply_gap, red_sku_count, replenishment_order_count,
                   baseline_snapshot_id, external_scenario_id, response_id, feasibility_status, candidate_status,
                   selected_by, selected_at_utc, selection_note
            FROM scenario_runs
            {whereClause}
            ORDER BY created_at_utc DESC
            {limitClause};
            """;
        if (baselineFilter is not null)
        {
            command.Parameters.AddWithValue("$baseline_snapshot_id", baselineFilter);
        }
        if (externalScenarioFilter is not null)
        {
            command.Parameters.AddWithValue("$external_scenario_id", externalScenarioFilter);
        }
        if (limit.HasValue)
        {
            command.Parameters.AddWithValue("$limit", limit.Value);
        }

        using var reader = command.ExecuteReader();
        var results = new List<ScenarioRunSummary>();
        while (reader.Read())
        {
            results.Add(ReadSummary(reader));
        }

        return results;
    }

    public ScenarioRunSummary? GetSummary(string runId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT run_id, run_number, name, description, created_by, status, approval_status, created_at_utc,
                   horizon_weeks, template_id, adoption_constraint_mode, service_level_percent, flow_index,
                   average_inventory_value, peak_load_percent, supply_gap, red_sku_count, replenishment_order_count,
                   baseline_snapshot_id, external_scenario_id, response_id, feasibility_status, candidate_status,
                   selected_by, selected_at_utc, selection_note
            FROM scenario_runs
            WHERE run_id = $run_id;
            """;
        command.Parameters.AddWithValue("$run_id", runId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadSummary(reader) : null;
    }

    public ScenarioRunDetail? GetDetail(string runId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT run_id, run_number, name, description, created_by, status, approval_status, created_at_utc,
                   horizon_weeks, template_id, adoption_constraint_mode, service_level_percent, flow_index,
                   average_inventory_value, peak_load_percent, supply_gap, red_sku_count, replenishment_order_count,
                   baseline_snapshot_id, external_scenario_id, response_id, feasibility_status, candidate_status,
                   selected_by, selected_at_utc, selection_note, request_json, result_json
            FROM scenario_runs
            WHERE run_id = $run_id;
            """;
        command.Parameters.AddWithValue("$run_id", runId);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        var summary = ReadSummary(reader);
        var requestJson = reader.GetString(26);
        var resultJson = reader.GetString(27);
        var request = JsonSerializer.Deserialize<ScenarioRunPreviewRequest>(requestJson, JsonOptions)
            ?? new ScenarioRunPreviewRequest();
        var result = ScenarioRunPreviewService.RestoreLegacyInventoryEvidence(
            JsonSerializer.Deserialize<ScenarioRunPreviewResult>(resultJson, JsonOptions)
                ?? _previewService.Preview(request),
            summary.BaselineSnapshotId);
        return new ScenarioRunDetail(summary, request, result);
    }

    public IReadOnlyList<ScenarioRunAuditEvent> GetAuditEvents(string runId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT event_id, run_id, sequence, event_type, stage, severity, message, payload_json, created_at_utc
            FROM scenario_run_audit_events
            WHERE run_id = $run_id
            ORDER BY sequence;
            """;
        command.Parameters.AddWithValue("$run_id", runId);

        using var reader = command.ExecuteReader();
        var results = new List<ScenarioRunAuditEvent>();
        while (reader.Read())
        {
            results.Add(new ScenarioRunAuditEvent(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.GetString(8)));
        }

        return results;
    }

    private static void EnsureCanSelect(ScenarioRunSummary summary)
    {
        if (summary.Status != "Saved")
        {
            throw new ArgumentException("只有已保存的冻结比较候选方案可以选定。", nameof(summary));
        }
        if (summary.CandidateStatus != "Candidate")
        {
            throw new ArgumentException("只有候选状态的方案可以选定。", nameof(summary));
        }
        if (string.IsNullOrWhiteSpace(summary.BaselineSnapshotId) ||
            string.IsNullOrWhiteSpace(summary.ExternalScenarioId) ||
            string.IsNullOrWhiteSpace(summary.ResponseId))
        {
            throw new ArgumentException("候选方案缺少冻结比较血缘，不能选定。", nameof(summary));
        }
        if (summary.FeasibilityStatus == "Blocked")
        {
            throw new InvalidOperationException("可行性结果为 Blocked 的候选方案不能选定。");
        }
        if (summary.FeasibilityStatus is not ("Adoptable" or "Reconcile"))
        {
            throw new ArgumentException("候选方案缺少可选定的后端可行性结果。", nameof(summary));
        }
    }

    private static ScenarioRunSummary? GetSummary(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string runId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT run_id, run_number, name, description, created_by, status, approval_status, created_at_utc,
                   horizon_weeks, template_id, adoption_constraint_mode, service_level_percent, flow_index,
                   average_inventory_value, peak_load_percent, supply_gap, red_sku_count, replenishment_order_count,
                   baseline_snapshot_id, external_scenario_id, response_id, feasibility_status, candidate_status,
                   selected_by, selected_at_utc, selection_note
            FROM scenario_runs
            WHERE run_id = $run_id;
            """;
        command.Parameters.AddWithValue("$run_id", runId);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadSummary(reader) : null;
    }

    private static IReadOnlyList<ScenarioRunSummary> GetSelectedSiblings(
        SqliteConnection connection,
        SqliteTransaction transaction,
        ScenarioRunSummary selected)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT run_id, run_number, name, description, created_by, status, approval_status, created_at_utc,
                   horizon_weeks, template_id, adoption_constraint_mode, service_level_percent, flow_index,
                   average_inventory_value, peak_load_percent, supply_gap, red_sku_count, replenishment_order_count,
                   baseline_snapshot_id, external_scenario_id, response_id, feasibility_status, candidate_status,
                   selected_by, selected_at_utc, selection_note
            FROM scenario_runs
            WHERE baseline_snapshot_id = $baseline_snapshot_id
              AND external_scenario_id = $external_scenario_id
              AND candidate_status = 'Selected'
              AND run_id <> $run_id;
            """;
        command.Parameters.AddWithValue("$baseline_snapshot_id", selected.BaselineSnapshotId!);
        command.Parameters.AddWithValue("$external_scenario_id", selected.ExternalScenarioId!);
        command.Parameters.AddWithValue("$run_id", selected.RunId);
        using var reader = command.ExecuteReader();
        var siblings = new List<ScenarioRunSummary>();
        while (reader.Read())
        {
            siblings.Add(ReadSummary(reader));
        }
        return siblings;
    }

    private static void UpdateCandidateStatus(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string runId,
        string candidateStatus,
        string? selectedBy,
        string? selectedAtUtc,
        string? selectionNote)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE scenario_runs
            SET candidate_status = $candidate_status,
                selected_by = COALESCE($selected_by, selected_by),
                selected_at_utc = COALESCE($selected_at_utc, selected_at_utc),
                selection_note = COALESCE($selection_note, selection_note)
            WHERE run_id = $run_id;
            """;
        command.Parameters.AddWithValue("$candidate_status", candidateStatus);
        command.Parameters.AddWithValue("$selected_by", (object?)selectedBy ?? DBNull.Value);
        command.Parameters.AddWithValue("$selected_at_utc", (object?)selectedAtUtc ?? DBNull.Value);
        command.Parameters.AddWithValue("$selection_note", (object?)selectionNote ?? DBNull.Value);
        command.Parameters.AddWithValue("$run_id", runId);
        if (command.ExecuteNonQuery() != 1)
        {
            throw new KeyNotFoundException("未找到场景运行记录。");
        }
    }

    private static void InsertSelectionAuditEvent(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string runId,
        string eventType,
        string actor,
        string? note,
        string createdAtUtc,
        string message)
    {
        var sequence = NextAuditSequence(connection, transaction, runId);
        var payload = JsonSerializer.Serialize(new { actor, note }, JsonOptions);
        InsertAuditEvent(connection, transaction, new ScenarioRunAuditEvent(
            Guid.NewGuid().ToString("N"),
            runId,
            sequence,
            eventType,
            "CandidateSelection",
            "Information",
            message,
            payload,
            createdAtUtc));
    }

    private static int NextAuditSequence(SqliteConnection connection, SqliteTransaction transaction, string runId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COALESCE(MAX(sequence), 0) + 1 FROM scenario_run_audit_events WHERE run_id = $run_id;";
        command.Parameters.AddWithValue("$run_id", runId);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private void EnsureCreated()
    {
        var directory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS scenario_runs (
                run_id TEXT PRIMARY KEY,
                run_number TEXT NOT NULL UNIQUE,
                name TEXT NOT NULL,
                description TEXT NULL,
                created_by TEXT NOT NULL,
                status TEXT NOT NULL,
                approval_status TEXT NOT NULL,
                created_at_utc TEXT NOT NULL,
                horizon_weeks INTEGER NOT NULL,
                template_id TEXT NULL,
                adoption_constraint_mode TEXT NULL,
                request_json TEXT NOT NULL,
                result_json TEXT NOT NULL,
                service_level_percent REAL NOT NULL,
                flow_index REAL NOT NULL,
                average_inventory_value REAL NOT NULL,
                peak_load_percent REAL NOT NULL,
                supply_gap REAL NOT NULL,
                red_sku_count INTEGER NOT NULL,
                replenishment_order_count INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS scenario_run_audit_events (
                event_id TEXT PRIMARY KEY,
                run_id TEXT NOT NULL,
                sequence INTEGER NOT NULL,
                event_type TEXT NOT NULL,
                stage TEXT NOT NULL,
                severity TEXT NOT NULL,
                message TEXT NOT NULL,
                payload_json TEXT NULL,
                created_at_utc TEXT NOT NULL,
                FOREIGN KEY(run_id) REFERENCES scenario_runs(run_id)
            );

            CREATE INDEX IF NOT EXISTS ix_scenario_runs_created_at ON scenario_runs(created_at_utc DESC);
            CREATE INDEX IF NOT EXISTS ix_scenario_run_audit_run_sequence ON scenario_run_audit_events(run_id, sequence);
            """;
        command.ExecuteNonQuery();
        EnsureColumn(connection, "baseline_snapshot_id", "TEXT NULL");
        EnsureColumn(connection, "external_scenario_id", "TEXT NULL");
        EnsureColumn(connection, "response_id", "TEXT NULL");
        EnsureColumn(connection, "feasibility_status", "TEXT NOT NULL DEFAULT 'Legacy'");
        EnsureColumn(connection, "candidate_status", "TEXT NOT NULL DEFAULT 'Candidate'");
        EnsureColumn(connection, "selected_by", "TEXT NULL");
        EnsureColumn(connection, "selected_at_utc", "TEXT NULL");
        EnsureColumn(connection, "selection_note", "TEXT NULL");

        using var lineageIndexes = connection.CreateCommand();
        lineageIndexes.CommandText = """
            CREATE INDEX IF NOT EXISTS ix_scenario_runs_baseline_snapshot ON scenario_runs(baseline_snapshot_id);
            CREATE INDEX IF NOT EXISTS ix_scenario_runs_external_scenario ON scenario_runs(external_scenario_id);
            """;
        lineageIndexes.ExecuteNonQuery();
    }

    private static void EnsureColumn(SqliteConnection connection, string columnName, string definition)
    {
        using (var inspect = connection.CreateCommand())
        {
            inspect.CommandText = "PRAGMA table_info(scenario_runs);";
            using var reader = inspect.ExecuteReader();
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.Ordinal))
                {
                    return;
                }
            }
        }

        using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE scenario_runs ADD COLUMN {columnName} {definition};";
        alter.ExecuteNonQuery();
    }

    private int NextSequence()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) + 1 FROM scenario_runs;";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private SqliteConnection OpenConnection()
    {
        var builder = new SqliteConnectionStringBuilder { DataSource = _databasePath };
        var connection = new SqliteConnection(builder.ToString());
        connection.Open();
        return connection;
    }

    private static ScenarioRunSummary BuildSummary(
        string runId,
        string runNumber,
        ScenarioRunSaveRequest request,
        string createdBy,
        string createdAtUtc,
        ScenarioRunPreviewResult preview)
    {
        var metrics = preview.Scenario.Metrics;
        return new ScenarioRunSummary(
            runId,
            runNumber,
            request.Name.Trim(),
            string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            createdBy,
            "Saved",
            "NotSubmitted",
            createdAtUtc,
            preview.Request.HorizonWeeks,
            preview.Request.TemplateId,
            preview.Request.AdoptionConstraintMode,
            metrics.ServiceLevelPercent,
            metrics.FlowIndex,
            metrics.AverageInventoryValue!.Value,
            metrics.PeakLoadPercent,
            metrics.SupplyGap,
            metrics.RedSkuCount,
            metrics.ReplenishmentOrderCount,
            FeasibilityStatus: preview.Feasibility?.Status ?? "Legacy");
    }

    private static void EnsurePhysicalInventoryEvidence(ScenarioRunPreviewResult preview)
    {
        var baselineExpectedKeys = preview.Baseline.Plan.BufferProjections
            .Select(item => (item.Sku, item.Week))
            .ToList();
        var scenarioExpectedKeys = preview.Scenario.Plan.BufferProjections
            .Select(item => (item.Sku, item.Week))
            .ToList();
        if (!preview.Baseline.Metrics.AverageInventoryValue.HasValue ||
            !preview.Scenario.Metrics.AverageInventoryValue.HasValue ||
            !InventoryFlowEvidenceValidator.IsComplete(
                preview.Baseline.CaseId,
                preview.Baseline.InventoryFlow,
                baselineExpectedKeys) ||
            !InventoryFlowEvidenceValidator.IsComplete(
                preview.Scenario.CaseId,
                preview.Scenario.InventoryFlow,
                scenarioExpectedKeys))
        {
            throw new InvalidOperationException("物理库存投影证据不完整，场景仅可预览，不得保存或进入治理决策。");
        }
    }

    private static IReadOnlyList<ScenarioRunAuditEvent> BuildAuditEvents(
        string runId,
        string requestJson,
        ScenarioRunPreviewResult preview,
        DateTimeOffset createdAt)
    {
        var createdAtText = createdAt.ToString("O");
        return new[]
        {
            new ScenarioRunAuditEvent(Guid.NewGuid().ToString("N"), runId, 1, "RunRequested", "Data", "Information", "收到场景保存请求。", requestJson, createdAtText),
            new ScenarioRunAuditEvent(Guid.NewGuid().ToString("N"), runId, 2, "PreviewRecalculated", "Engine", "Information", "后端已按保存请求重新运行 Scenario Preview。", null, createdAtText),
            new ScenarioRunAuditEvent(Guid.NewGuid().ToString("N"), runId, 3, "TraceCaptured", "Trace", "Information", $"已保存 {preview.Trace.Count} 条预览审计 trace 和 {preview.Scenario.Plan.Traces.Count} 条计划计算 trace。", JsonSerializer.Serialize(preview.Trace, JsonOptions), createdAtText),
            new ScenarioRunAuditEvent(Guid.NewGuid().ToString("N"), runId, 4, "RunSaved", "Persistence", "Information", "场景运行记录已保存，审批状态为未提交。", null, createdAtText)
        };
    }

    private static void AddParameters(SqliteCommand command, ScenarioRunSummary summary)
    {
        command.Parameters.AddWithValue("$run_id", summary.RunId);
        command.Parameters.AddWithValue("$run_number", summary.RunNumber);
        command.Parameters.AddWithValue("$name", summary.Name);
        command.Parameters.AddWithValue("$description", (object?)summary.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("$created_by", summary.CreatedBy);
        command.Parameters.AddWithValue("$status", summary.Status);
        command.Parameters.AddWithValue("$approval_status", summary.ApprovalStatus);
        command.Parameters.AddWithValue("$created_at_utc", summary.CreatedAtUtc);
        command.Parameters.AddWithValue("$horizon_weeks", summary.HorizonWeeks);
        command.Parameters.AddWithValue("$template_id", (object?)summary.TemplateId ?? DBNull.Value);
        command.Parameters.AddWithValue("$adoption_constraint_mode", (object?)summary.AdoptionConstraintMode ?? DBNull.Value);
        command.Parameters.AddWithValue("$service_level_percent", summary.ServiceLevelPercent);
        command.Parameters.AddWithValue("$flow_index", summary.FlowIndex);
        command.Parameters.AddWithValue("$average_inventory_value", summary.AverageInventoryValue);
        command.Parameters.AddWithValue("$peak_load_percent", summary.PeakLoadPercent);
        command.Parameters.AddWithValue("$supply_gap", summary.SupplyGap);
        command.Parameters.AddWithValue("$red_sku_count", summary.RedSkuCount);
        command.Parameters.AddWithValue("$replenishment_order_count", summary.ReplenishmentOrderCount);
        command.Parameters.AddWithValue("$baseline_snapshot_id", (object?)summary.BaselineSnapshotId ?? DBNull.Value);
        command.Parameters.AddWithValue("$external_scenario_id", (object?)summary.ExternalScenarioId ?? DBNull.Value);
        command.Parameters.AddWithValue("$response_id", (object?)summary.ResponseId ?? DBNull.Value);
        command.Parameters.AddWithValue("$feasibility_status", summary.FeasibilityStatus);
        command.Parameters.AddWithValue("$candidate_status", summary.CandidateStatus);
        command.Parameters.AddWithValue("$selected_by", (object?)summary.SelectedBy ?? DBNull.Value);
        command.Parameters.AddWithValue("$selected_at_utc", (object?)summary.SelectedAtUtc ?? DBNull.Value);
        command.Parameters.AddWithValue("$selection_note", (object?)summary.SelectionNote ?? DBNull.Value);
    }

    private static void InsertAuditEvent(SqliteConnection connection, SqliteTransaction transaction, ScenarioRunAuditEvent auditEvent)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO scenario_run_audit_events (
                event_id, run_id, sequence, event_type, stage, severity, message, payload_json, created_at_utc)
            VALUES (
                $event_id, $run_id, $sequence, $event_type, $stage, $severity, $message, $payload_json, $created_at_utc);
            """;
        command.Parameters.AddWithValue("$event_id", auditEvent.EventId);
        command.Parameters.AddWithValue("$run_id", auditEvent.RunId);
        command.Parameters.AddWithValue("$sequence", auditEvent.Sequence);
        command.Parameters.AddWithValue("$event_type", auditEvent.EventType);
        command.Parameters.AddWithValue("$stage", auditEvent.Stage);
        command.Parameters.AddWithValue("$severity", auditEvent.Severity);
        command.Parameters.AddWithValue("$message", auditEvent.Message);
        command.Parameters.AddWithValue("$payload_json", (object?)auditEvent.PayloadJson ?? DBNull.Value);
        command.Parameters.AddWithValue("$created_at_utc", auditEvent.CreatedAtUtc);
        command.ExecuteNonQuery();
    }

    private static ScenarioRunSummary ReadSummary(SqliteDataReader reader)
    {
        return new ScenarioRunSummary(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.GetString(6),
            reader.GetString(7),
            reader.GetInt32(8),
            reader.IsDBNull(9) ? null : reader.GetString(9),
            reader.IsDBNull(10) ? null : reader.GetString(10),
            reader.GetDecimal(11),
            reader.GetDecimal(12),
            reader.GetDecimal(13),
            reader.GetDecimal(14),
            reader.GetDecimal(15),
            reader.GetInt32(16),
            reader.GetInt32(17),
            reader.IsDBNull(18) ? null : reader.GetString(18),
            reader.IsDBNull(19) ? null : reader.GetString(19),
            reader.IsDBNull(20) ? null : reader.GetString(20),
            reader.IsDBNull(21) ? "Legacy" : reader.GetString(21),
            reader.IsDBNull(22) ? "Candidate" : reader.GetString(22),
            reader.IsDBNull(23) ? null : reader.GetString(23),
            reader.IsDBNull(24) ? null : reader.GetString(24),
            reader.IsDBNull(25) ? null : reader.GetString(25));
    }

    private static string? NormalizeFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
