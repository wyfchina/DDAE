using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace AdaptiveSopDdsop.Web.Domain;

public sealed record CoordinationItemCreateRequest(
    string Title,
    IReadOnlyList<string> ImpactObjects,
    string? RelatedScenarioRunId,
    string? RelatedMasterSettingChangeId,
    string ServiceImpact,
    string InventoryImpact,
    decimal? CashImpact,
    string RiskImpact,
    string DecisionRequired,
    string Owner,
    string DueDate,
    string EscalationLevel,
    string NextReviewDate,
    string CreatedBy,
    string? RelatedDdomPackageId = null);

public sealed record CoordinationStatusUpdateRequest(string Status, string UpdatedBy, string? Note);

public sealed record CoordinationDecisionUpdateRequest(string Decision, string Rationale, string UpdatedBy);

public sealed record CoordinationOutcomeUpdateRequest(string ActualOutcome, string UpdatedBy);

public sealed record CoordinationItem(
    string ItemId,
    string ItemNumber,
    string Title,
    IReadOnlyList<string> ImpactObjects,
    string? RelatedScenarioRunId,
    string? RelatedMasterSettingChangeId,
    string ServiceImpact,
    string InventoryImpact,
    decimal? CashImpact,
    string RiskImpact,
    string DecisionRequired,
    string Owner,
    string DueDate,
    string EscalationLevel,
    string NextReviewDate,
    string Status,
    string? Decision,
    string? DecisionRationale,
    string? ActualOutcome,
    string CreatedBy,
    string CreatedAtUtc,
    string UpdatedAtUtc,
    string? RelatedDdomPackageId = null);

public sealed record CoordinationAuditEvent(
    string EventId,
    string ItemId,
    int Sequence,
    string EventType,
    string Actor,
    string Message,
    string CreatedAtUtc);

public sealed class CoordinationLedgerService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> AllowedTransitions =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            ["Open"] = new HashSet<string>(new[] { "InProgress", "Escalated" }, StringComparer.Ordinal),
            ["InProgress"] = new HashSet<string>(new[] { "Completed", "Escalated" }, StringComparer.Ordinal),
            ["Escalated"] = new HashSet<string>(new[] { "InProgress" }, StringComparer.Ordinal),
            ["Completed"] = new HashSet<string>(StringComparer.Ordinal)
        };

    private readonly string _databasePath;
    private readonly DdomChangePackageService? _ddomChangePackages;

    public CoordinationLedgerService(string databasePath, DdomChangePackageService? ddomChangePackages = null)
    {
        _databasePath = databasePath;
        _ddomChangePackages = ddomChangePackages;
        EnsureSchema();
    }

    public CoordinationItem Create(CoordinationItemCreateRequest request)
    {
        ValidateRequired(request.Title, "问题标题");
        ValidateRequired(request.Owner, "负责人");
        ValidateRequired(request.CreatedBy, "创建人");
        ValidateRequired(request.DecisionRequired, "决策要求");
        var relatedDdomPackageId = Normalize(request.RelatedDdomPackageId);
        if (relatedDdomPackageId is not null)
        {
            if (_ddomChangePackages is null)
            {
                throw new InvalidOperationException("DDOM 变更包验证服务不可用。");
            }
            if (!_ddomChangePackages.Exists(relatedDdomPackageId))
            {
                throw new KeyNotFoundException($"关联的 DDOM 变更包不存在：{relatedDdomPackageId}。");
            }
        }

        var now = DateTimeOffset.UtcNow.ToString("O");
        var itemId = Guid.NewGuid().ToString("N");
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        var sequence = NextItemSequence(connection, transaction);
        var item = new CoordinationItem(
            itemId,
            $"ISSUE-{DateTimeOffset.UtcNow:yyyyMMdd}-{sequence:000}",
            request.Title.Trim(),
            request.ImpactObjects.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()).Distinct(StringComparer.Ordinal).ToList(),
            Normalize(request.RelatedScenarioRunId),
            Normalize(request.RelatedMasterSettingChangeId),
            request.ServiceImpact.Trim(),
            request.InventoryImpact.Trim(),
            request.CashImpact,
            request.RiskImpact.Trim(),
            request.DecisionRequired.Trim(),
            request.Owner.Trim(),
            request.DueDate,
            request.EscalationLevel.Trim(),
            request.NextReviewDate,
            "Open",
            null,
            null,
            null,
            request.CreatedBy.Trim(),
            now,
            now,
            relatedDdomPackageId);

        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO coordination_items (
                    item_id, item_number, title, impact_objects_json, related_scenario_run_id,
                    related_master_setting_change_id, related_ddom_package_id, service_impact, inventory_impact, cash_impact,
                    risk_impact, decision_required, owner, due_date, escalation_level, next_review_date,
                    status, decision, decision_rationale, actual_outcome, created_by, created_at_utc, updated_at_utc)
                VALUES (
                    $item_id, $item_number, $title, $impact_objects_json, $related_scenario_run_id,
                    $related_master_setting_change_id, $related_ddom_package_id, $service_impact, $inventory_impact, $cash_impact,
                    $risk_impact, $decision_required, $owner, $due_date, $escalation_level, $next_review_date,
                    $status, NULL, NULL, NULL, $created_by, $created_at_utc, $updated_at_utc);
                """;
            AddItemParameters(command, item);
            command.ExecuteNonQuery();
        }
        InsertAudit(connection, transaction, itemId, "CoordinationItemCreated", request.CreatedBy.Trim(), $"创建协调事项 {item.ItemNumber}。", now);
        transaction.Commit();
        return item;
    }

    public IReadOnlyList<CoordinationItem> List(
        int limit,
        string? relatedScenarioRunId = null,
        string? relatedMasterSettingChangeId = null,
        string? relatedDdomPackageId = null)
    {
        return QueryList(
            Math.Clamp(limit, 1, 200),
            relatedScenarioRunId,
            relatedMasterSettingChangeId,
            relatedDdomPackageId);
    }

    internal IReadOnlyList<CoordinationItem> ListByLineage(
        string? relatedScenarioRunId = null,
        string? relatedMasterSettingChangeId = null,
        string? relatedDdomPackageId = null) =>
        QueryList(null, relatedScenarioRunId, relatedMasterSettingChangeId, relatedDdomPackageId);

    private IReadOnlyList<CoordinationItem> QueryList(
        int? limit,
        string? relatedScenarioRunId,
        string? relatedMasterSettingChangeId,
        string? relatedDdomPackageId)
    {
        var scenarioRunFilter = Normalize(relatedScenarioRunId);
        var masterSettingChangeFilter = Normalize(relatedMasterSettingChangeId);
        var ddomPackageFilter = Normalize(relatedDdomPackageId);
        var predicates = new List<string>();
        if (scenarioRunFilter is not null)
        {
            predicates.Add("related_scenario_run_id = $related_scenario_run_id");
        }
        if (masterSettingChangeFilter is not null)
        {
            predicates.Add("related_master_setting_change_id = $related_master_setting_change_id");
        }
        if (ddomPackageFilter is not null)
        {
            predicates.Add("related_ddom_package_id = $related_ddom_package_id");
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        var whereClause = predicates.Count == 0
            ? string.Empty
            : $"WHERE {string.Join(" AND ", predicates)}";
        var limitClause = limit.HasValue ? "LIMIT $limit" : string.Empty;
        command.CommandText = $"""
            {SelectItemSql}
            {whereClause}
            ORDER BY created_at_utc DESC
            {limitClause};
            """;
        if (scenarioRunFilter is not null)
        {
            command.Parameters.AddWithValue("$related_scenario_run_id", scenarioRunFilter);
        }
        if (masterSettingChangeFilter is not null)
        {
            command.Parameters.AddWithValue("$related_master_setting_change_id", masterSettingChangeFilter);
        }
        if (ddomPackageFilter is not null)
        {
            command.Parameters.AddWithValue("$related_ddom_package_id", ddomPackageFilter);
        }
        if (limit.HasValue)
        {
            command.Parameters.AddWithValue("$limit", limit.Value);
        }
        using var reader = command.ExecuteReader();
        var results = new List<CoordinationItem>();
        while (reader.Read())
        {
            results.Add(ReadItem(reader));
        }
        return results;
    }

    public CoordinationItem? GetDetail(string itemId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"{SelectItemSql} WHERE item_id = $item_id;";
        command.Parameters.AddWithValue("$item_id", itemId);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadItem(reader) : null;
    }

    public CoordinationItem UpdateStatus(string itemId, CoordinationStatusUpdateRequest request)
    {
        ValidateRequired(request.UpdatedBy, "更新人");
        var now = DateTimeOffset.UtcNow.ToString("O");
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        var currentStatus = ReadStatus(connection, transaction, itemId)
            ?? throw new KeyNotFoundException($"协调事项 {itemId} 不存在。");
        if (!AllowedTransitions.TryGetValue(currentStatus, out var allowed) || !allowed.Contains(request.Status))
        {
            throw new ArgumentException($"不允许从 {currentStatus} 转为 {request.Status}。", nameof(request));
        }

        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "UPDATE coordination_items SET status = $status, updated_at_utc = $updated_at_utc WHERE item_id = $item_id AND status = $expected_status;";
            command.Parameters.AddWithValue("$status", request.Status);
            command.Parameters.AddWithValue("$updated_at_utc", now);
            command.Parameters.AddWithValue("$item_id", itemId);
            command.Parameters.AddWithValue("$expected_status", currentStatus);
            if (command.ExecuteNonQuery() != 1)
            {
                throw new InvalidOperationException("协调事项状态已被其他操作更新，请刷新后重试。");
            }
        }
        var note = string.IsNullOrWhiteSpace(request.Note) ? string.Empty : $"；{request.Note.Trim()}";
        InsertAudit(connection, transaction, itemId, "StatusChanged", request.UpdatedBy.Trim(), $"状态 {currentStatus} → {request.Status}{note}", now);
        transaction.Commit();
        return RequireItem(itemId);
    }

    public CoordinationItem RecordDecision(string itemId, CoordinationDecisionUpdateRequest request)
    {
        RequireItem(itemId);
        ValidateRequired(request.Decision, "决策内容");
        ValidateRequired(request.Rationale, "决策理由");
        ValidateRequired(request.UpdatedBy, "更新人");
        return UpdateTextFields(
            itemId,
            "decision = $value, decision_rationale = $secondary",
            request.Decision.Trim(),
            request.Rationale.Trim(),
            "DecisionRecorded",
            request.UpdatedBy.Trim(),
            $"记录决策：{request.Decision.Trim()}");
    }

    public CoordinationItem RecordOutcome(string itemId, CoordinationOutcomeUpdateRequest request)
    {
        RequireItem(itemId);
        ValidateRequired(request.ActualOutcome, "实际效果");
        ValidateRequired(request.UpdatedBy, "更新人");
        return UpdateTextFields(
            itemId,
            "actual_outcome = $value",
            request.ActualOutcome.Trim(),
            null,
            "OutcomeRecorded",
            request.UpdatedBy.Trim(),
            $"记录实际效果：{request.ActualOutcome.Trim()}");
    }

    public IReadOnlyList<CoordinationAuditEvent> GetAuditEvents(string itemId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT event_id, item_id, sequence, event_type, actor, message, created_at_utc
            FROM coordination_item_audit_events
            WHERE item_id = $item_id
            ORDER BY sequence;
            """;
        command.Parameters.AddWithValue("$item_id", itemId);
        using var reader = command.ExecuteReader();
        var results = new List<CoordinationAuditEvent>();
        while (reader.Read())
        {
            results.Add(new CoordinationAuditEvent(
                reader.GetString(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3),
                reader.GetString(4), reader.GetString(5), reader.GetString(6)));
        }
        return results;
    }

    private CoordinationItem UpdateTextFields(
        string itemId,
        string assignment,
        string value,
        string? secondary,
        string eventType,
        string actor,
        string message)
    {
        var now = DateTimeOffset.UtcNow.ToString("O");
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = $"UPDATE coordination_items SET {assignment}, updated_at_utc = $updated_at_utc WHERE item_id = $item_id;";
            command.Parameters.AddWithValue("$value", value);
            if (secondary is not null)
            {
                command.Parameters.AddWithValue("$secondary", secondary);
            }
            command.Parameters.AddWithValue("$updated_at_utc", now);
            command.Parameters.AddWithValue("$item_id", itemId);
            command.ExecuteNonQuery();
        }
        InsertAudit(connection, transaction, itemId, eventType, actor, message, now);
        transaction.Commit();
        return RequireItem(itemId);
    }

    private CoordinationItem RequireItem(string itemId)
    {
        return GetDetail(itemId) ?? throw new KeyNotFoundException($"协调事项 {itemId} 不存在。");
    }

    private void EnsureSchema()
    {
        var directory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS coordination_items (
                item_id TEXT PRIMARY KEY,
                item_number TEXT NOT NULL UNIQUE,
                title TEXT NOT NULL,
                impact_objects_json TEXT NOT NULL,
                related_scenario_run_id TEXT NULL,
                related_master_setting_change_id TEXT NULL,
                service_impact TEXT NOT NULL,
                inventory_impact TEXT NOT NULL,
                cash_impact REAL NULL,
                risk_impact TEXT NOT NULL,
                decision_required TEXT NOT NULL,
                owner TEXT NOT NULL,
                due_date TEXT NOT NULL,
                escalation_level TEXT NOT NULL,
                next_review_date TEXT NOT NULL,
                status TEXT NOT NULL,
                decision TEXT NULL,
                decision_rationale TEXT NULL,
                actual_outcome TEXT NULL,
                created_by TEXT NOT NULL,
                created_at_utc TEXT NOT NULL,
                updated_at_utc TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS coordination_item_audit_events (
                event_id TEXT PRIMARY KEY,
                item_id TEXT NOT NULL,
                sequence INTEGER NOT NULL,
                event_type TEXT NOT NULL,
                actor TEXT NOT NULL,
                message TEXT NOT NULL,
                created_at_utc TEXT NOT NULL,
                UNIQUE(item_id, sequence)
            );
            CREATE INDEX IF NOT EXISTS ix_coordination_status_due ON coordination_items(status, due_date);
            CREATE INDEX IF NOT EXISTS ix_coordination_related_scenario_run ON coordination_items(related_scenario_run_id);
            CREATE INDEX IF NOT EXISTS ix_coordination_related_master_setting_change ON coordination_items(related_master_setting_change_id);
            CREATE INDEX IF NOT EXISTS ix_coordination_audit_sequence ON coordination_item_audit_events(item_id, sequence);
            """;
        command.ExecuteNonQuery();
        EnsureDdomPackageColumn(connection);
        using var indexCommand = connection.CreateCommand();
        indexCommand.CommandText = "CREATE INDEX IF NOT EXISTS ix_coordination_items_ddom_package ON coordination_items(related_ddom_package_id);";
        indexCommand.ExecuteNonQuery();
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _databasePath }.ToString());
        connection.Open();
        return connection;
    }

    private static int NextItemSequence(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(*) + 1 FROM coordination_items;";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static string? ReadStatus(SqliteConnection connection, SqliteTransaction transaction, string itemId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT status FROM coordination_items WHERE item_id = $item_id;";
        command.Parameters.AddWithValue("$item_id", itemId);
        return command.ExecuteScalar() as string;
    }

    private static void InsertAudit(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string itemId,
        string eventType,
        string actor,
        string message,
        string now)
    {
        using var sequenceCommand = connection.CreateCommand();
        sequenceCommand.Transaction = transaction;
        sequenceCommand.CommandText = "SELECT COUNT(*) + 1 FROM coordination_item_audit_events WHERE item_id = $item_id;";
        sequenceCommand.Parameters.AddWithValue("$item_id", itemId);
        var sequence = Convert.ToInt32(sequenceCommand.ExecuteScalar());

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO coordination_item_audit_events (event_id, item_id, sequence, event_type, actor, message, created_at_utc)
            VALUES ($event_id, $item_id, $sequence, $event_type, $actor, $message, $created_at_utc);
            """;
        command.Parameters.AddWithValue("$event_id", Guid.NewGuid().ToString("N"));
        command.Parameters.AddWithValue("$item_id", itemId);
        command.Parameters.AddWithValue("$sequence", sequence);
        command.Parameters.AddWithValue("$event_type", eventType);
        command.Parameters.AddWithValue("$actor", actor);
        command.Parameters.AddWithValue("$message", message);
        command.Parameters.AddWithValue("$created_at_utc", now);
        command.ExecuteNonQuery();
    }

    private static void AddItemParameters(SqliteCommand command, CoordinationItem item)
    {
        command.Parameters.AddWithValue("$item_id", item.ItemId);
        command.Parameters.AddWithValue("$item_number", item.ItemNumber);
        command.Parameters.AddWithValue("$title", item.Title);
        command.Parameters.AddWithValue("$impact_objects_json", JsonSerializer.Serialize(item.ImpactObjects, JsonOptions));
        command.Parameters.AddWithValue("$related_scenario_run_id", (object?)item.RelatedScenarioRunId ?? DBNull.Value);
        command.Parameters.AddWithValue("$related_master_setting_change_id", (object?)item.RelatedMasterSettingChangeId ?? DBNull.Value);
        command.Parameters.AddWithValue("$related_ddom_package_id", (object?)item.RelatedDdomPackageId ?? DBNull.Value);
        command.Parameters.AddWithValue("$service_impact", item.ServiceImpact);
        command.Parameters.AddWithValue("$inventory_impact", item.InventoryImpact);
        command.Parameters.AddWithValue("$cash_impact", (object?)item.CashImpact ?? DBNull.Value);
        command.Parameters.AddWithValue("$risk_impact", item.RiskImpact);
        command.Parameters.AddWithValue("$decision_required", item.DecisionRequired);
        command.Parameters.AddWithValue("$owner", item.Owner);
        command.Parameters.AddWithValue("$due_date", item.DueDate);
        command.Parameters.AddWithValue("$escalation_level", item.EscalationLevel);
        command.Parameters.AddWithValue("$next_review_date", item.NextReviewDate);
        command.Parameters.AddWithValue("$status", item.Status);
        command.Parameters.AddWithValue("$created_by", item.CreatedBy);
        command.Parameters.AddWithValue("$created_at_utc", item.CreatedAtUtc);
        command.Parameters.AddWithValue("$updated_at_utc", item.UpdatedAtUtc);
    }

    private static CoordinationItem ReadItem(SqliteDataReader reader)
    {
        return new CoordinationItem(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            JsonSerializer.Deserialize<List<string>>(reader.GetString(3), JsonOptions) ?? new(),
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.GetString(7),
            reader.GetString(8),
            reader.IsDBNull(9) ? null : reader.GetDecimal(9),
            reader.GetString(10),
            reader.GetString(11),
            reader.GetString(12),
            reader.GetString(13),
            reader.GetString(14),
            reader.GetString(15),
            reader.GetString(16),
            reader.IsDBNull(17) ? null : reader.GetString(17),
            reader.IsDBNull(18) ? null : reader.GetString(18),
            reader.IsDBNull(19) ? null : reader.GetString(19),
            reader.GetString(20),
            reader.GetString(21),
            reader.GetString(22),
            reader.IsDBNull(6) ? null : reader.GetString(6));
    }

    private const string SelectItemSql = """
        SELECT item_id, item_number, title, impact_objects_json, related_scenario_run_id,
               related_master_setting_change_id, related_ddom_package_id, service_impact, inventory_impact, cash_impact,
               risk_impact, decision_required, owner, due_date, escalation_level, next_review_date,
               status, decision, decision_rationale, actual_outcome, created_by, created_at_utc, updated_at_utc
        FROM coordination_items
        """;

    private static void EnsureDdomPackageColumn(SqliteConnection connection)
    {
        using var existsCommand = connection.CreateCommand();
        existsCommand.CommandText = "SELECT COUNT(*) FROM pragma_table_info('coordination_items') WHERE name = 'related_ddom_package_id';";
        if (Convert.ToInt32(existsCommand.ExecuteScalar()) != 0) return;
        using var alterCommand = connection.CreateCommand();
        alterCommand.CommandText = "ALTER TABLE coordination_items ADD COLUMN related_ddom_package_id TEXT NULL;";
        alterCommand.ExecuteNonQuery();
    }

    private static void ValidateRequired(string? value, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{label}不能为空。");
        }
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
