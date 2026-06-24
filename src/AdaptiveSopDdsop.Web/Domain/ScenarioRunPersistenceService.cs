using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace AdaptiveSopDdsop.Web.Domain;

public sealed class ScenarioRunPersistenceService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly string _databasePath;
    private readonly ScenarioRunPreviewService _previewService;

    public ScenarioRunPersistenceService(ScenarioRunPreviewService previewService, string databasePath)
    {
        _previewService = previewService;
        _databasePath = databasePath;
        EnsureCreated();
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
                    supply_gap, red_sku_count, replenishment_order_count)
                VALUES (
                    $run_id, $run_number, $name, $description, $created_by, $status, $approval_status, $created_at_utc,
                    $horizon_weeks, $template_id, $adoption_constraint_mode, $request_json, $result_json,
                    $service_level_percent, $flow_index, $average_inventory_value, $peak_load_percent,
                    $supply_gap, $red_sku_count, $replenishment_order_count);
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

    public IReadOnlyList<ScenarioRunSummary> List(int limit)
    {
        var boundedLimit = Math.Clamp(limit <= 0 ? 50 : limit, 1, 200);
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT run_id, run_number, name, description, created_by, status, approval_status, created_at_utc,
                   horizon_weeks, template_id, adoption_constraint_mode, service_level_percent, flow_index,
                   average_inventory_value, peak_load_percent, supply_gap, red_sku_count, replenishment_order_count
            FROM scenario_runs
            ORDER BY created_at_utc DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", boundedLimit);

        using var reader = command.ExecuteReader();
        var results = new List<ScenarioRunSummary>();
        while (reader.Read())
        {
            results.Add(ReadSummary(reader));
        }

        return results;
    }

    public ScenarioRunDetail? GetDetail(string runId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT run_id, run_number, name, description, created_by, status, approval_status, created_at_utc,
                   horizon_weeks, template_id, adoption_constraint_mode, service_level_percent, flow_index,
                   average_inventory_value, peak_load_percent, supply_gap, red_sku_count, replenishment_order_count,
                   request_json, result_json
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
        var requestJson = reader.GetString(18);
        var resultJson = reader.GetString(19);
        var request = JsonSerializer.Deserialize<ScenarioRunPreviewRequest>(requestJson, JsonOptions)
            ?? new ScenarioRunPreviewRequest();
        var result = JsonSerializer.Deserialize<ScenarioRunPreviewResult>(resultJson, JsonOptions)
            ?? _previewService.Preview(request);
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
            metrics.AverageInventoryValue,
            metrics.PeakLoadPercent,
            metrics.SupplyGap,
            metrics.RedSkuCount,
            metrics.ReplenishmentOrderCount);
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
            reader.GetInt32(17));
    }
}
