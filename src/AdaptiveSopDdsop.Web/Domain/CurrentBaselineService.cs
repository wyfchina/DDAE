using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace AdaptiveSopDdsop.Web.Domain;

public sealed record BaselineEvidenceSection(
    string SectionCode,
    string Name,
    string SourceAuthority,
    string AsOfUtc,
    string FreshnessStatus,
    string CompletenessStatus,
    int ItemCount,
    string EvidenceLabel,
    bool IsRequired,
    string? MissingReason = null,
    IReadOnlyList<BaselineEvidenceItem>? Items = null);

public sealed record BaselineKpiSnapshot(
    decimal? ServiceLevelPercent,
    string ServiceWindow,
    decimal? InventoryValue,
    decimal? WorkInProcessUnits,
    decimal? BacklogUnits,
    decimal? SupplyCoverageWeeks,
    decimal? PeakResourceLoadPercent,
    string SourceAuthority,
    string AsOfUtc,
    string EvidenceStatus);

public sealed record BaselineAnalysisAvailability(
    string AnalysisCode,
    string Status,
    string Reason);

public sealed record BaselineEvidenceItem(
    string ItemKey,
    string Name,
    string FreshnessStatus,
    string CompletenessStatus,
    bool BlocksFreeze,
    string? MissingReason = null);

public sealed record BaselineTransitItem(string Sku, decimal Quantity, string Status);
public sealed record BaselineBacklogItem(string Sku, int Week, decimal Quantity, string Status);
public sealed record BaselineWipItem(string ResourceCode, string Sku, decimal Quantity, string Status);
public sealed record BaselineSupplierCommitment(string Supplier, string MaterialFamily, decimal Quantity, int LeadTimeDays, string RiskStatus);
public sealed record BaselineResourceAvailability(string ResourceCode, string ResourceName, decimal AvailableCapacity, string CalendarStatus);
public sealed record BaselineTemporaryAdjustment(string AdjustmentId, string Name, string Window, string AppliesTo, string Status);

public sealed record CurrentBaselinePayload(
    IReadOnlyList<InventoryPosition> Inventory,
    IReadOnlyList<BaselineTransitItem> InTransit,
    IReadOnlyList<BaselineBacklogItem> Backlog,
    IReadOnlyList<BaselineWipItem> WorkInProcess,
    IReadOnlyList<BaselineSupplierCommitment> SupplierCommitments,
    IReadOnlyList<BaselineResourceAvailability> ResourceAvailability,
    IReadOnlyList<BaselineTemporaryAdjustment> ActiveTemporaryAdjustments,
    IReadOnlyList<MasterSetting> MasterSettings,
    ScenarioWorkspaceDataSet? PlanningInputs = null,
    BaselineKpiSnapshot? Kpis = null,
    IReadOnlyList<BaselineAnalysisAvailability>? AnalysisAvailability = null);

public sealed record CurrentBaselineCandidate(
    string CandidateId,
    string AsOfUtc,
    string MasterSettingVersion,
    IReadOnlyList<BaselineEvidenceSection> Sections,
    CurrentBaselinePayload Payload,
    string EvidenceLabel);

public sealed record CurrentBaselineFreezeRequest(string CreatedBy, string? Note);

public sealed record CurrentBaselineSnapshot(
    string SnapshotId,
    string SnapshotNumber,
    string Status,
    string AsOfUtc,
    string MasterSettingVersion,
    string CreatedBy,
    string? Note,
    string CreatedAtUtc,
    IReadOnlyList<BaselineEvidenceSection> Sections,
    CurrentBaselinePayload Payload,
    string EvidenceLabel);

public sealed record CurrentBaselineSummary(
    string SnapshotId,
    string SnapshotNumber,
    string Status,
    string AsOfUtc,
    string MasterSettingVersion,
    string CreatedBy,
    string CreatedAtUtc,
    int CompleteSectionCount,
    int SectionCount,
    string EvidenceLabel);

public sealed record CurrentBaselineAuditEvent(
    string EventId,
    string SnapshotId,
    int Sequence,
    string EventType,
    string Message,
    string CreatedAtUtc,
    string? PayloadJson = null);

public interface ICurrentBaselineDataSource
{
    CurrentBaselineCandidate GetCandidate();
}

public sealed class CurrentBaselineService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ICurrentBaselineDataSource _dataSource;
    private readonly string _databasePath;

    public CurrentBaselineService(ICurrentBaselineDataSource dataSource, string databasePath)
    {
        _dataSource = dataSource;
        _databasePath = databasePath;
        EnsureSchema();
    }

    public CurrentBaselineCandidate GetCandidate() => _dataSource.GetCandidate();

    public CurrentBaselineSnapshot Freeze(CurrentBaselineFreezeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CreatedBy))
        {
            throw new ArgumentException("冻结人不能为空。", nameof(request));
        }

        var candidate = _dataSource.GetCandidate();
        var blockingEvidence = FindEvidenceIssues(candidate.Sections, blockingOnly: true);
        if (blockingEvidence.Count > 0)
        {
            throw new ArgumentException($"关键基线证据不完整或已过期：{string.Join("; ", blockingEvidence)}。", nameof(request));
        }
        if (candidate.Payload.PlanningInputs is null)
        {
            throw new ArgumentException("关键基线证据不完整：缺少白盒重算所需的类型化计划输入。", nameof(request));
        }

        var now = DateTimeOffset.UtcNow.ToString("O");
        var snapshotId = Guid.NewGuid().ToString("N");
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        var sequence = NextSnapshotSequence(connection, transaction);
        var snapshotNumber = $"BASE-{DateTimeOffset.UtcNow:yyyyMMdd}-{sequence:000}";
        var snapshot = new CurrentBaselineSnapshot(
            snapshotId,
            snapshotNumber,
            "Frozen",
            candidate.AsOfUtc,
            candidate.MasterSettingVersion,
            request.CreatedBy.Trim(),
            string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            now,
            candidate.Sections,
            candidate.Payload,
            candidate.EvidenceLabel);
        var allEvidenceIssues = FindEvidenceIssues(candidate.Sections, blockingOnly: false);
        var completeEvidence = candidate.Sections
            .SelectMany(section => section.Items is { Count: > 0 }
                ? section.Items
                    .Where(item => item.FreshnessStatus == "Fresh" && item.CompletenessStatus == "Complete")
                    .Select(item => $"{section.SectionCode}/{item.ItemKey}")
                : section.FreshnessStatus == "Fresh" && section.CompletenessStatus == "Complete"
                    ? new[] { section.SectionCode }
                    : Array.Empty<string>())
            .ToList();
        var auditPayload = JsonSerializer.Serialize(new
        {
            candidateId = candidate.CandidateId,
            snapshotNumber,
            actor = snapshot.CreatedBy,
            createdAtUtc = now,
            completeEvidence,
            missingEvidence = allEvidenceIssues,
            result = "Frozen"
        }, JsonOptions);

        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO current_baseline_snapshots (
                    snapshot_id, snapshot_number, status, as_of_utc, master_setting_version,
                    created_by, note, created_at_utc, sections_json, payload_json, evidence_label)
                VALUES (
                    $snapshot_id, $snapshot_number, $status, $as_of_utc, $master_setting_version,
                    $created_by, $note, $created_at_utc, $sections_json, $payload_json, $evidence_label);
                """;
            command.Parameters.AddWithValue("$snapshot_id", snapshot.SnapshotId);
            command.Parameters.AddWithValue("$snapshot_number", snapshot.SnapshotNumber);
            command.Parameters.AddWithValue("$status", snapshot.Status);
            command.Parameters.AddWithValue("$as_of_utc", snapshot.AsOfUtc);
            command.Parameters.AddWithValue("$master_setting_version", snapshot.MasterSettingVersion);
            command.Parameters.AddWithValue("$created_by", snapshot.CreatedBy);
            command.Parameters.AddWithValue("$note", (object?)snapshot.Note ?? DBNull.Value);
            command.Parameters.AddWithValue("$created_at_utc", snapshot.CreatedAtUtc);
            command.Parameters.AddWithValue("$sections_json", JsonSerializer.Serialize(snapshot.Sections, JsonOptions));
            command.Parameters.AddWithValue("$payload_json", JsonSerializer.Serialize(snapshot.Payload, JsonOptions));
            command.Parameters.AddWithValue("$evidence_label", snapshot.EvidenceLabel);
            command.ExecuteNonQuery();
        }

        InsertAudit(connection, transaction, new CurrentBaselineAuditEvent(
            Guid.NewGuid().ToString("N"), snapshotId, 1, "BaselineFrozen",
            $"候选 {candidate.CandidateId} 已冻结为 {snapshotNumber}；完整证据 {completeEvidence.Count} 项，缺失或过期证据 {allEvidenceIssues.Count} 项。",
            now,
            auditPayload));
        transaction.Commit();
        return snapshot;
    }

    public IReadOnlyList<CurrentBaselineSummary> List(int limit)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT snapshot_id, snapshot_number, status, as_of_utc, master_setting_version,
                   created_by, created_at_utc, sections_json, evidence_label
            FROM current_baseline_snapshots
            ORDER BY created_at_utc DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, 200));
        using var reader = command.ExecuteReader();
        var results = new List<CurrentBaselineSummary>();
        while (reader.Read())
        {
            var sections = JsonSerializer.Deserialize<List<BaselineEvidenceSection>>(reader.GetString(7), JsonOptions) ?? new();
            results.Add(new CurrentBaselineSummary(
                reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
                reader.GetString(5), reader.GetString(6), sections.Count(item => item.CompletenessStatus == "Complete"),
                sections.Count, reader.GetString(8)));
        }
        return results;
    }

    public CurrentBaselineSnapshot? GetDetail(string snapshotId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT snapshot_id, snapshot_number, status, as_of_utc, master_setting_version,
                   created_by, note, created_at_utc, sections_json, payload_json, evidence_label
            FROM current_baseline_snapshots
            WHERE snapshot_id = $snapshot_id;
            """;
        command.Parameters.AddWithValue("$snapshot_id", snapshotId);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        var sections = JsonSerializer.Deserialize<List<BaselineEvidenceSection>>(reader.GetString(8), JsonOptions) ?? new();
        var payload = JsonSerializer.Deserialize<CurrentBaselinePayload>(reader.GetString(9), JsonOptions)
            ?? throw new InvalidOperationException("基线 payload 无法读取。");
        return new CurrentBaselineSnapshot(
            reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
            reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6), reader.GetString(7), sections, payload, reader.GetString(10));
    }

    public IReadOnlyList<CurrentBaselineAuditEvent> GetAuditEvents(string snapshotId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT event_id, snapshot_id, sequence, event_type, message, created_at_utc, payload_json
            FROM current_baseline_audit_events
            WHERE snapshot_id = $snapshot_id
            ORDER BY sequence;
            """;
        command.Parameters.AddWithValue("$snapshot_id", snapshotId);
        using var reader = command.ExecuteReader();
        var results = new List<CurrentBaselineAuditEvent>();
        while (reader.Read())
        {
            results.Add(new CurrentBaselineAuditEvent(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6)));
        }
        return results;
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
            CREATE TABLE IF NOT EXISTS current_baseline_snapshots (
                snapshot_id TEXT PRIMARY KEY,
                snapshot_number TEXT NOT NULL UNIQUE,
                status TEXT NOT NULL,
                as_of_utc TEXT NOT NULL,
                master_setting_version TEXT NOT NULL,
                created_by TEXT NOT NULL,
                note TEXT NULL,
                created_at_utc TEXT NOT NULL,
                sections_json TEXT NOT NULL,
                payload_json TEXT NOT NULL,
                evidence_label TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS current_baseline_audit_events (
                event_id TEXT PRIMARY KEY,
                snapshot_id TEXT NOT NULL,
                sequence INTEGER NOT NULL,
                event_type TEXT NOT NULL,
                message TEXT NOT NULL,
                created_at_utc TEXT NOT NULL,
                payload_json TEXT NULL,
                UNIQUE(snapshot_id, sequence),
                FOREIGN KEY(snapshot_id) REFERENCES current_baseline_snapshots(snapshot_id) ON DELETE RESTRICT
            );
            CREATE INDEX IF NOT EXISTS ix_current_baseline_created_at ON current_baseline_snapshots(created_at_utc DESC);
            CREATE INDEX IF NOT EXISTS ix_current_baseline_audit_sequence ON current_baseline_audit_events(snapshot_id, sequence);
            CREATE TRIGGER IF NOT EXISTS trg_current_baseline_snapshots_no_update
            BEFORE UPDATE ON current_baseline_snapshots
            BEGIN
                SELECT RAISE(ABORT, 'Frozen baseline snapshots are immutable');
            END;
            CREATE TRIGGER IF NOT EXISTS trg_current_baseline_snapshots_no_delete
            BEFORE DELETE ON current_baseline_snapshots
            BEGIN
                SELECT RAISE(ABORT, 'Frozen baseline snapshots are immutable');
            END;
            CREATE TRIGGER IF NOT EXISTS trg_current_baseline_audit_no_update
            BEFORE UPDATE ON current_baseline_audit_events
            BEGIN
                SELECT RAISE(ABORT, 'Baseline audit events are append-only');
            END;
            CREATE TRIGGER IF NOT EXISTS trg_current_baseline_audit_no_delete
            BEFORE DELETE ON current_baseline_audit_events
            BEGIN
                SELECT RAISE(ABORT, 'Baseline audit events are append-only');
            END;
            """;
        command.ExecuteNonQuery();
        EnsureColumn(connection, "current_baseline_audit_events", "payload_json", "TEXT NULL");
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _databasePath }.ToString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();
        return connection;
    }

    private static int NextSnapshotSequence(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(*) + 1 FROM current_baseline_snapshots;";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static void InsertAudit(SqliteConnection connection, SqliteTransaction transaction, CurrentBaselineAuditEvent item)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO current_baseline_audit_events (event_id, snapshot_id, sequence, event_type, message, created_at_utc, payload_json)
            VALUES ($event_id, $snapshot_id, $sequence, $event_type, $message, $created_at_utc, $payload_json);
            """;
        command.Parameters.AddWithValue("$event_id", item.EventId);
        command.Parameters.AddWithValue("$snapshot_id", item.SnapshotId);
        command.Parameters.AddWithValue("$sequence", item.Sequence);
        command.Parameters.AddWithValue("$event_type", item.EventType);
        command.Parameters.AddWithValue("$message", item.Message);
        command.Parameters.AddWithValue("$created_at_utc", item.CreatedAtUtc);
        command.Parameters.AddWithValue("$payload_json", (object?)item.PayloadJson ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    private static IReadOnlyList<string> FindEvidenceIssues(
        IReadOnlyList<BaselineEvidenceSection> sections,
        bool blockingOnly)
    {
        var issues = new List<string>();
        foreach (var section in sections)
        {
            if (section.Items is { Count: > 0 })
            {
                foreach (var item in section.Items)
                {
                    var complete = item.FreshnessStatus == "Fresh" && item.CompletenessStatus == "Complete";
                    if (complete || (blockingOnly && !item.BlocksFreeze))
                    {
                        continue;
                    }

                    var reason = string.IsNullOrWhiteSpace(item.MissingReason)
                        ? $"Freshness={item.FreshnessStatus},Completeness={item.CompletenessStatus}"
                        : item.MissingReason;
                    issues.Add($"{section.SectionCode}/{item.ItemKey}/{reason}");
                }

                continue;
            }

            var notApplicable = section.FreshnessStatus == "NotApplicable" && section.CompletenessStatus == "NotApplicable";
            var completeSection = section.FreshnessStatus == "Fresh" && section.CompletenessStatus == "Complete";
            if (completeSection ||
                (!section.IsRequired && notApplicable) ||
                (blockingOnly && !section.IsRequired))
            {
                continue;
            }

            var sectionReason = string.IsNullOrWhiteSpace(section.MissingReason)
                ? $"Freshness={section.FreshnessStatus},Completeness={section.CompletenessStatus}"
                : section.MissingReason;
            issues.Add($"{section.SectionCode}/{sectionReason}");
        }

        return issues;
    }

    private static void EnsureColumn(SqliteConnection connection, string tableName, string columnName, string declaration)
    {
        using var pragma = connection.CreateCommand();
        pragma.CommandText = $"PRAGMA table_info({tableName});";
        using var reader = pragma.ExecuteReader();
        var exists = false;
        while (reader.Read())
        {
            if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
            {
                exists = true;
                break;
            }
        }

        reader.Close();
        if (exists)
        {
            return;
        }

        using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {declaration};";
        alter.ExecuteNonQuery();
    }
}
