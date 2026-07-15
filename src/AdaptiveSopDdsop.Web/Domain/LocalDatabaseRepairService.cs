using Microsoft.Data.Sqlite;

namespace AdaptiveSopDdsop.Web.Domain;

public sealed record LocalDatabaseRepairResult(
    string RepairId,
    bool WasAlreadyApplied,
    int DeletedCoordinationItems,
    int DeletedCoordinationAuditEvents,
    int RepairedBaselines,
    int AddedBaselineAuditEvents);

public interface ILocalDatabaseRepairService
{
    LocalDatabaseRepairResult Apply();
}

public sealed class LocalDatabaseRepairService : ILocalDatabaseRepairService
{
    private const string RepairId = "2026-07-15-smoke-mojibake-v1";
    private const string FirstCoordinationItemId = "09944ca75dfa4efab765d1481c860709";
    private const string SecondCoordinationItemId = "8aead2083210423db98e9f35924c7f8e";
    private const string TargetSnapshotNumber = "BASE-20260714-002";
    private const string OldCreatedBy = "Codex ??";
    private const string RepairedCreatedBy = "Codex 烟测";
    private const string SnapshotNoUpdateTriggerName = "trg_current_baseline_snapshots_no_update";

    private readonly string _databasePath;

    public LocalDatabaseRepairService(string databasePath)
    {
        _databasePath = databasePath;
    }

    public LocalDatabaseRepairResult Apply()
    {
        var directory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var connection = new SqliteConnection(
            new SqliteConnectionStringBuilder { DataSource = _databasePath }.ToString());
        connection.Open();
        using (var foreignKeys = connection.CreateCommand())
        {
            foreignKeys.CommandText = "PRAGMA foreign_keys = ON;";
            foreignKeys.ExecuteNonQuery();
        }

        using var transaction = connection.BeginTransaction();
        EnsureJournal(connection, transaction);
        if (WasAlreadyApplied(connection, transaction))
        {
            transaction.Commit();
            return EmptyResult(wasAlreadyApplied: true);
        }

        var deletedCoordinationAuditEvents = DeleteCoordinationAuditEvents(connection, transaction);
        var deletedCoordinationItems = DeleteCoordinationItems(connection, transaction);
        var (triggerName, triggerSql) = ReadSnapshotNoUpdateTrigger(connection, transaction);

        DropSnapshotNoUpdateTrigger(connection, transaction, triggerName);
        var repairedBaselines = RepairTargetBaseline(connection, transaction);
        if (repairedBaselines > 1)
        {
            throw new InvalidOperationException("Known local baseline repair matched more than one row.");
        }
        RestoreSnapshotNoUpdateTrigger(connection, transaction, triggerSql);

        var addedBaselineAuditEvents = repairedBaselines == 1
            ? AppendBaselineRepairAudit(connection, transaction)
            : 0;
        InsertJournal(
            connection,
            transaction,
            deletedCoordinationItems,
            deletedCoordinationAuditEvents,
            repairedBaselines,
            addedBaselineAuditEvents);
        transaction.Commit();

        return new LocalDatabaseRepairResult(
            RepairId,
            false,
            deletedCoordinationItems,
            deletedCoordinationAuditEvents,
            repairedBaselines,
            addedBaselineAuditEvents);
    }

    private static LocalDatabaseRepairResult EmptyResult(bool wasAlreadyApplied) =>
        new(RepairId, wasAlreadyApplied, 0, 0, 0, 0);

    private static void EnsureJournal(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS local_data_repairs (
                repair_id TEXT PRIMARY KEY,
                applied_at_utc TEXT NOT NULL,
                deleted_coordination_items INTEGER NOT NULL,
                deleted_coordination_audit_events INTEGER NOT NULL,
                repaired_baselines INTEGER NOT NULL,
                added_baseline_audit_events INTEGER NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    private static bool WasAlreadyApplied(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT 1 FROM local_data_repairs WHERE repair_id = $repair_id;";
        command.Parameters.AddWithValue("$repair_id", RepairId);
        return command.ExecuteScalar() is not null;
    }

    private static int DeleteCoordinationAuditEvents(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            DELETE FROM coordination_item_audit_events
            WHERE item_id IN ($first_item_id, $second_item_id);
            """;
        AddCoordinationItemIdParameters(command);
        return command.ExecuteNonQuery();
    }

    private static int DeleteCoordinationItems(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            DELETE FROM coordination_items
            WHERE item_id IN ($first_item_id, $second_item_id);
            """;
        AddCoordinationItemIdParameters(command);
        return command.ExecuteNonQuery();
    }

    private static void AddCoordinationItemIdParameters(SqliteCommand command)
    {
        command.Parameters.AddWithValue("$first_item_id", FirstCoordinationItemId);
        command.Parameters.AddWithValue("$second_item_id", SecondCoordinationItemId);
    }

    private static (string Name, string Sql) ReadSnapshotNoUpdateTrigger(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT name, sql
            FROM sqlite_master
            WHERE type = 'trigger' AND name = $trigger_name;
            """;
        command.Parameters.AddWithValue("$trigger_name", SnapshotNoUpdateTriggerName);
        using var reader = command.ExecuteReader();
        if (!reader.Read() || reader.IsDBNull(1))
        {
            throw new InvalidOperationException($"Required trigger {SnapshotNoUpdateTriggerName} was not found.");
        }

        var name = reader.GetString(0);
        var sql = reader.GetString(1);
        if (!string.Equals(name, SnapshotNoUpdateTriggerName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Unexpected baseline no-update trigger was selected.");
        }
        if (reader.Read())
        {
            throw new InvalidOperationException("Multiple baseline no-update triggers matched the fixed repair target.");
        }
        return (name, sql);
    }

    private static void DropSnapshotNoUpdateTrigger(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string triggerName)
    {
        if (!string.Equals(triggerName, SnapshotNoUpdateTriggerName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to drop an unexpected trigger.");
        }

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "DROP TRIGGER trg_current_baseline_snapshots_no_update;";
        command.ExecuteNonQuery();
    }

    private static int RepairTargetBaseline(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE current_baseline_snapshots
            SET created_by = $repaired_created_by
            WHERE snapshot_number = $snapshot_number
              AND created_by = $old_created_by;
            """;
        command.Parameters.AddWithValue("$repaired_created_by", RepairedCreatedBy);
        command.Parameters.AddWithValue("$snapshot_number", TargetSnapshotNumber);
        command.Parameters.AddWithValue("$old_created_by", OldCreatedBy);
        return command.ExecuteNonQuery();
    }

    private static void RestoreSnapshotNoUpdateTrigger(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string triggerSql)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = triggerSql;
        command.ExecuteNonQuery();
    }

    private static int AppendBaselineRepairAudit(SqliteConnection connection, SqliteTransaction transaction)
    {
        string snapshotId;
        using (var snapshotCommand = connection.CreateCommand())
        {
            snapshotCommand.Transaction = transaction;
            snapshotCommand.CommandText = """
                SELECT snapshot_id
                FROM current_baseline_snapshots
                WHERE snapshot_number = $snapshot_number
                  AND created_by = $repaired_created_by;
                """;
            snapshotCommand.Parameters.AddWithValue("$snapshot_number", TargetSnapshotNumber);
            snapshotCommand.Parameters.AddWithValue("$repaired_created_by", RepairedCreatedBy);
            snapshotId = Convert.ToString(snapshotCommand.ExecuteScalar())
                ?? throw new InvalidOperationException("Repaired baseline snapshot could not be read.");
        }

        long sequence;
        using (var sequenceCommand = connection.CreateCommand())
        {
            sequenceCommand.Transaction = transaction;
            sequenceCommand.CommandText = """
                SELECT COALESCE(MAX(sequence), 0) + 1
                FROM current_baseline_audit_events
                WHERE snapshot_id = $snapshot_id;
                """;
            sequenceCommand.Parameters.AddWithValue("$snapshot_id", snapshotId);
            sequence = Convert.ToInt64(sequenceCommand.ExecuteScalar());
        }

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO current_baseline_audit_events (
                event_id, snapshot_id, sequence, event_type, message, created_at_utc, payload_json)
            VALUES (
                $event_id, $snapshot_id, $sequence, $event_type, $message, $created_at_utc, NULL);
            """;
        command.Parameters.AddWithValue("$event_id", Guid.NewGuid().ToString("N"));
        command.Parameters.AddWithValue("$snapshot_id", snapshotId);
        command.Parameters.AddWithValue("$sequence", sequence);
        command.Parameters.AddWithValue("$event_type", "DataRepairApplied");
        command.Parameters.AddWithValue(
            "$message",
            "已将已知本地烟测创建人乱码修复为 Codex 烟测。");
        command.Parameters.AddWithValue("$created_at_utc", DateTimeOffset.UtcNow.ToString("O"));
        return command.ExecuteNonQuery();
    }

    private static void InsertJournal(
        SqliteConnection connection,
        SqliteTransaction transaction,
        int deletedCoordinationItems,
        int deletedCoordinationAuditEvents,
        int repairedBaselines,
        int addedBaselineAuditEvents)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO local_data_repairs (
                repair_id, applied_at_utc, deleted_coordination_items,
                deleted_coordination_audit_events, repaired_baselines, added_baseline_audit_events)
            VALUES (
                $repair_id, $applied_at_utc, $deleted_coordination_items,
                $deleted_coordination_audit_events, $repaired_baselines, $added_baseline_audit_events);
            """;
        command.Parameters.AddWithValue("$repair_id", RepairId);
        command.Parameters.AddWithValue("$applied_at_utc", DateTimeOffset.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$deleted_coordination_items", deletedCoordinationItems);
        command.Parameters.AddWithValue("$deleted_coordination_audit_events", deletedCoordinationAuditEvents);
        command.Parameters.AddWithValue("$repaired_baselines", repairedBaselines);
        command.Parameters.AddWithValue("$added_baseline_audit_events", addedBaselineAuditEvents);
        command.ExecuteNonQuery();
    }
}
