using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace AdaptiveSopDdsop.Web.Domain;

public sealed class MasterSettingsGovernanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private static readonly IReadOnlyDictionary<string, string> AllowedTransitions = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Proposed"] = "Reviewed",
        ["Reviewed"] = "Approved",
        ["Approved"] = "Effective",
        ["Effective"] = "Expired"
    };

    private readonly string _databasePath;
    private readonly IScenarioWorkspaceDataSource _dataSource;
    private readonly ScenarioRunPreviewService _previewService;

    public MasterSettingsGovernanceService(
        IScenarioWorkspaceDataSource dataSource,
        ScenarioRunPreviewService previewService,
        string databasePath)
    {
        _dataSource = dataSource;
        _previewService = previewService;
        _databasePath = databasePath;
        EnsureCreated();
    }

    public MasterSettingsWorkspaceResult GetWorkspace(int limit = 50)
    {
        var data = LoadData();
        var recentChanges = ListChanges(limit);
        var statusCounts = data.MasterSettings
            .Select(item => item.Status)
            .Concat(recentChanges.Select(item => item.Status))
            .GroupBy(status => status, StringComparer.Ordinal)
            .Select(group => new MasterSettingStatusCount(group.Key, group.Count()))
            .OrderBy(item => StatusRank(item.Status))
            .ToList();
        var typeCounts = data.MasterSettings
            .Select(item => item.SettingType)
            .Concat(recentChanges.Select(item => item.SettingType))
            .GroupBy(type => type, StringComparer.Ordinal)
            .Select(group => new MasterSettingTypeCount(group.Key, group.Count()))
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.SettingType, StringComparer.Ordinal)
            .ToList();

        return new MasterSettingsWorkspaceResult(
            data.MasterSettings.Count + recentChanges.Count,
            data.MasterSettings.Count(item => item.Status is "Proposed" or "Reviewed") + recentChanges.Count(item => item.Status is "Proposed" or "Reviewed"),
            data.MasterSettings.Count(item => item.Status == "Approved") + recentChanges.Count(item => item.Status == "Approved"),
            data.MasterSettings.Count(item => item.Status == "Effective") + recentChanges.Count(item => item.Status == "Effective"),
            recentChanges.Count(item => item.RiskLevel == "Red") + data.MasterSettings.Count(item => item.ServiceImpact >= 2m || item.CashImpact >= 2_000_000m),
            data.MasterSettings.Sum(item => item.ServiceImpact) + recentChanges.Sum(item => item.ServiceImpact),
            data.MasterSettings.Sum(item => item.CashImpact) + recentChanges.Sum(item => item.CashImpact),
            data.MasterSettings,
            statusCounts,
            typeCounts,
            recentChanges);
    }

    public MasterSettingProposalResponse ProposeFromPreview(ScenarioRunPreviewRequest request)
    {
        var safeRequest = request.HorizonWeeks <= 0 ? request with { HorizonWeeks = 12 } : request;
        var preview = _previewService.Preview(safeRequest);
        var data = LoadData(safeRequest);
        var proposals = new List<MasterSettingChangeRequest>();

        proposals.AddRange(BuildTemplateActionProposals(data, safeRequest));
        proposals.AddRange(BuildParameterProposals(data, safeRequest));
        proposals.AddRange(BuildSystemSuggestedProposals(data, preview));

        var deduped = proposals
            .GroupBy(item => $"{item.SettingType}|{item.Target}|{item.ProposedValue}", StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();

        var trace = preview.Trace
            .Concat(new[]
            {
                new ScenarioAuditTrace("MasterSettings", $"从场景预览生成 {deduped.Count} 条主设置变更建议。", "Information")
            })
            .ToList();

        return new MasterSettingProposalResponse(safeRequest, deduped, trace);
    }

    public MasterSettingChangeSaveResponse SaveChange(MasterSettingChangeSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Change.SettingType) || string.IsNullOrWhiteSpace(request.Change.Target))
        {
            throw new ArgumentException("主设置变更类型和目标不能为空。", nameof(request));
        }

        var createdAt = DateTimeOffset.UtcNow;
        var createdAtText = createdAt.ToString("O");
        var changeId = Guid.NewGuid().ToString("N");
        var changeNumber = $"MSG-{createdAt:yyyyMMdd}-{NextChangeSequence():0000}";
        var createdBy = string.IsNullOrWhiteSpace(request.CreatedBy) ? "计划员" : request.CreatedBy.Trim();
        var status = NormalizeInitialStatus(request.Change.Status);
        var proposal = request.Change with { Status = status };
        var impact = new MasterSettingChangeImpact(proposal.ServiceImpact, proposal.CashImpact, proposal.RiskLevel, string.Join("；", proposal.Rationale));
        var summary = BuildSummary(changeId, changeNumber, createdBy, createdAtText, proposal);
        var proposalJson = JsonSerializer.Serialize(proposal, JsonOptions);
        var impactJson = JsonSerializer.Serialize(impact, JsonOptions);

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO master_setting_changes (
                    change_id, change_number, source_scenario_run_id, source_template_id,
                    setting_type, target, current_value, proposed_value, trigger, effective_window,
                    status, service_impact, cash_impact, risk_level, created_by, created_at_utc,
                    proposal_json, impact_json)
                VALUES (
                    $change_id, $change_number, $source_scenario_run_id, $source_template_id,
                    $setting_type, $target, $current_value, $proposed_value, $trigger, $effective_window,
                    $status, $service_impact, $cash_impact, $risk_level, $created_by, $created_at_utc,
                    $proposal_json, $impact_json);
                """;
            AddChangeParameters(command, summary);
            command.Parameters.AddWithValue("$proposal_json", proposalJson);
            command.Parameters.AddWithValue("$impact_json", impactJson);
            command.ExecuteNonQuery();
        }

        foreach (var auditEvent in BuildSaveAuditEvents(changeId, proposalJson, impactJson, createdAt))
        {
            InsertAuditEvent(connection, transaction, auditEvent);
        }

        transaction.Commit();
        return new MasterSettingChangeSaveResponse(changeId, changeNumber, status, true, summary);
    }

    public IReadOnlyList<MasterSettingChangeSummary> ListChanges(int limit)
    {
        var boundedLimit = Math.Clamp(limit <= 0 ? 50 : limit, 1, 200);
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT change_id, change_number, source_scenario_run_id, source_template_id,
                   setting_type, target, current_value, proposed_value, trigger, effective_window,
                   status, service_impact, cash_impact, risk_level, created_by, created_at_utc
            FROM master_setting_changes
            ORDER BY created_at_utc DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", boundedLimit);

        using var reader = command.ExecuteReader();
        var results = new List<MasterSettingChangeSummary>();
        while (reader.Read())
        {
            results.Add(ReadSummary(reader));
        }

        return results;
    }

    public MasterSettingChangeDetail? GetDetail(string changeId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT change_id, change_number, source_scenario_run_id, source_template_id,
                   setting_type, target, current_value, proposed_value, trigger, effective_window,
                   status, service_impact, cash_impact, risk_level, created_by, created_at_utc,
                   proposal_json, impact_json
            FROM master_setting_changes
            WHERE change_id = $change_id;
            """;
        command.Parameters.AddWithValue("$change_id", changeId);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        var summary = ReadSummary(reader);
        var proposal = JsonSerializer.Deserialize<MasterSettingChangeRequest>(reader.GetString(16), JsonOptions)
            ?? new MasterSettingChangeRequest(null, null, summary.SettingType, summary.Target, summary.CurrentValue, summary.ProposedValue, summary.Trigger, summary.EffectiveWindow, summary.Status, summary.ServiceImpact, summary.CashImpact, summary.RiskLevel, Array.Empty<string>());
        var impact = JsonSerializer.Deserialize<MasterSettingChangeImpact>(reader.GetString(17), JsonOptions)
            ?? new MasterSettingChangeImpact(summary.ServiceImpact, summary.CashImpact, summary.RiskLevel, summary.Trigger);
        return new MasterSettingChangeDetail(summary, proposal, impact);
    }

    public IReadOnlyList<MasterSettingChangeAuditEvent> GetAuditEvents(string changeId)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT event_id, change_id, sequence, event_type, stage, severity, message, payload_json, created_at_utc
            FROM master_setting_change_audit_events
            WHERE change_id = $change_id
            ORDER BY sequence;
            """;
        command.Parameters.AddWithValue("$change_id", changeId);

        using var reader = command.ExecuteReader();
        var results = new List<MasterSettingChangeAuditEvent>();
        while (reader.Read())
        {
            results.Add(new MasterSettingChangeAuditEvent(
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

    public MasterSettingChangeSummary UpdateStatus(string changeId, MasterSettingStatusUpdateRequest request)
    {
        var detail = GetDetail(changeId) ?? throw new ArgumentException("主设置变更不存在。", nameof(changeId));
        var currentStatus = detail.Summary.Status;
        if (!AllowedTransitions.TryGetValue(currentStatus, out var allowedNext) || request.Status != allowedNext)
        {
            throw new ArgumentException($"状态只能从 {currentStatus} 流转到 {allowedNext ?? "终态"}。", nameof(request));
        }

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE master_setting_changes
                SET status = $status
                WHERE change_id = $change_id;
                """;
            command.Parameters.AddWithValue("$status", request.Status);
            command.Parameters.AddWithValue("$change_id", changeId);
            command.ExecuteNonQuery();
        }

        var updatedBy = string.IsNullOrWhiteSpace(request.UpdatedBy) ? "计划员" : request.UpdatedBy.Trim();
        var message = $"主设置变更状态由 {currentStatus} 流转为 {request.Status}。操作者：{updatedBy}。{request.Note ?? string.Empty}".Trim();
        var audit = new MasterSettingChangeAuditEvent(
            Guid.NewGuid().ToString("N"),
            changeId,
            NextAuditSequence(connection, transaction, changeId),
            "StatusChanged",
            "Governance",
            "Information",
            message,
            JsonSerializer.Serialize(request, JsonOptions),
            DateTimeOffset.UtcNow.ToString("O"));
        InsertAuditEvent(connection, transaction, audit);
        transaction.Commit();

        return GetDetail(changeId)?.Summary ?? throw new InvalidOperationException("状态更新后无法读取主设置变更。");
    }

    private ScenarioWorkspaceDataSet LoadData(ScenarioRunPreviewRequest? request = null)
    {
        return _dataSource.Load(new ScenarioWorkspaceDataRequest(
            Math.Clamp(request?.HorizonWeeks ?? 12, 1, 52),
            new DateOnly(2026, 6, 1),
            request?.SkuFilter,
            request?.FamilyFilter));
    }

    private static IEnumerable<MasterSettingChangeRequest> BuildTemplateActionProposals(
        ScenarioWorkspaceDataSet data,
        ScenarioRunPreviewRequest request)
    {
        var template = data.ScenarioTemplates.FirstOrDefault(item => item.TemplateId == request.TemplateId);
        if (template is null)
        {
            yield break;
        }

        foreach (var action in template.Actions)
        {
            foreach (var proposal in ProposalFromAction(data, request, action, "ScenarioTemplate"))
            {
                yield return proposal;
            }
        }
    }

    private static IEnumerable<MasterSettingChangeRequest> BuildParameterProposals(
        ScenarioWorkspaceDataSet data,
        ScenarioRunPreviewRequest request)
    {
        var parameters = request.Parameters ?? new ScenarioRunParameterSet();

        foreach (var policy in parameters.SkuPolicyOverrides ?? Array.Empty<SkuPolicyOverride>())
        {
            var sku = data.Skus.FirstOrDefault(item => item.Sku == policy.Sku);
            if (sku is null)
            {
                continue;
            }

            var proposedParts = new List<string>();
            if (policy.MinimumOrderQuantity.HasValue)
            {
                proposedParts.Add($"MOQ {policy.MinimumOrderQuantity:0}");
            }
            if (policy.OrderCycleDays.HasValue)
            {
                proposedParts.Add($"订货周期 {policy.OrderCycleDays}d");
            }

            yield return BuildProposal(
                request,
                "Inventory Buffer",
                sku.Name,
                SkuCurrentValue(sku),
                proposedParts.Count == 0 ? SkuCurrentValue(sku) : string.Join(", ", proposedParts),
                "场景预览覆盖 SKU 补货策略。",
                "下一轮 DDS&OP 生效窗口",
                1.4m,
                sku.UnitCost * Math.Max(policy.MinimumOrderQuantity ?? 0m, sku.MinimumOrderQuantity) * 0.08m,
                "Yellow",
                "SkuPolicyOverride");
        }

        foreach (var campaign in parameters.PrebuildCampaigns ?? Array.Empty<PrebuildCampaign>())
        {
            var sku = data.Skus.FirstOrDefault(item => item.Sku == campaign.Sku);
            if (sku is null)
            {
                continue;
            }

            yield return BuildProposal(
                request,
                "Inventory Buffer",
                sku.Name,
                SkuCurrentValue(sku),
                $"提前建库 {campaign.Quantity:0}，保护第 {campaign.ProtectFromWeek}-{campaign.ProtectThroughWeek} 周",
                "场景预览使用提前建库吸收未来峰值。",
                $"第 {campaign.BuildWeek}-{campaign.ProtectThroughWeek} 周",
                1.8m,
                sku.UnitCost * campaign.Quantity,
                campaign.Quantity > sku.MinimumOrderQuantity * 2m ? "Red" : "Yellow",
                "PrebuildCampaign");
        }

        foreach (var adjustment in parameters.CapacityAdjustments ?? Array.Empty<ResourceCapacityAdjustment>())
        {
            var resource = data.Resources.FirstOrDefault(item => item.Code == adjustment.ResourceCode);
            if (resource is null)
            {
                continue;
            }

            yield return BuildProposal(
                request,
                "Capacity Buffer",
                resource.Name,
                $"周可用能力 {resource.WeeklyAvailableUnits:0.#}，单位负荷 {resource.UnitLoad:0.00}",
                $"第 {adjustment.Week} 周能力倍率 {adjustment.CapacityMultiplier:0.00}",
                adjustment.Reason,
                $"第 {adjustment.Week} 周",
                adjustment.CapacityMultiplier > 1 ? 2.2m : -1.2m,
                adjustment.CapacityMultiplier > 1 ? 680_000m : 0m,
                adjustment.CapacityMultiplier < 0.75m || adjustment.CapacityMultiplier > 1.35m ? "Red" : "Yellow",
                "ResourceCapacityAdjustment");
        }

        foreach (var limit in parameters.SupplierCapacityLimits ?? Array.Empty<SupplierCapacityLimit>())
        {
            var windows = data.SupplierCapacityWindows
                .Where(item => item.Supplier == limit.Supplier && item.MaterialFamily == limit.MaterialFamily)
                .ToList();
            var currentCapacity = windows.Count == 0 ? 0m : windows.Average(item => item.CommittedCapacity);

            yield return BuildProposal(
                request,
                "Supplier Master Setting",
                $"{limit.Supplier} / {limit.MaterialFamily}",
                $"平均承诺能力 {currentCapacity:0.#}",
                $"第 {limit.StartWeek}-{limit.EndWeek} 周承诺能力 {limit.CommittedCapacity:0.#}",
                "场景预览设置供应能力限制。",
                $"第 {limit.StartWeek}-{limit.EndWeek} 周",
                limit.CommittedCapacity < currentCapacity ? -1.5m : 1.2m,
                0m,
                limit.CommittedCapacity < currentCapacity * 0.7m ? "Red" : "Yellow",
                "SupplierCapacityLimit");
        }
    }

    private static IEnumerable<MasterSettingChangeRequest> BuildSystemSuggestedProposals(
        ScenarioWorkspaceDataSet data,
        ScenarioRunPreviewResult preview)
    {
        if (preview.Scenario.Metrics.RedSkuCount > 0)
        {
            var redSku = preview.Scenario.BufferTrend.WeeklyCells.FirstOrDefault(item => item.Status == "Red")?.Sku;
            var sku = data.Skus.FirstOrDefault(item => item.Sku == redSku) ?? data.Skus.FirstOrDefault();
            if (sku is not null)
            {
                yield return BuildProposal(
                    preview.Request,
                    "Inventory Buffer",
                    sku.Name,
                    SkuCurrentValue(sku),
                    "重审 ADU / DLT / VF / MOQ / 订货周期",
                    "预览结果仍存在红区 SKU。",
                    "下一轮 DDS&OP 生效窗口",
                    2.0m,
                    sku.UnitCost * sku.MinimumOrderQuantity,
                    "Red",
                    "SystemSuggested");
            }
        }

        if (preview.Scenario.Metrics.SupplyGap > 0)
        {
            yield return BuildProposal(
                preview.Request,
                "Time Buffer",
                "供应缺口保护窗口",
                "按当前供应承诺执行",
                "增加供应保护提前期、Act/Late 阈值和替代供应策略",
                $"预览供应缺口 {preview.Scenario.Metrics.SupplyGap:0.#}。",
                "缺口周之前",
                1.8m,
                preview.Scenario.Metrics.SupplyGap * 1000m,
                "Red",
                "SystemSuggested");
        }

        foreach (var resource in preview.Scenario.Rccp.ResourceSummaries.Where(item => item.Status == "Red").Take(2))
        {
            yield return BuildProposal(
                preview.Request,
                "Capacity Buffer",
                resource.ResourceName,
                "按当前资源日历与保护能力执行",
                $"设置保护能力边界，峰值负荷 {resource.PeakLoadPercent:0.#}%",
                "RCCP 预览存在红区资源。",
                "超载周之前",
                2.4m,
                Math.Max(0, resource.MaxCapacityGap) * 1000m,
                resource.PeakLoadPercent > 120m ? "Red" : "Yellow",
                "SystemSuggested");
        }
    }

    private static IEnumerable<MasterSettingChangeRequest> ProposalFromAction(
        ScenarioWorkspaceDataSet data,
        ScenarioRunPreviewRequest request,
        ScenarioTemplateAction action,
        string source)
    {
        if (action.ActionType is "MoqOverride" or "OrderCycleOverride")
        {
            var sku = data.Skus.FirstOrDefault(item => item.Sku == action.Target);
            if (sku is not null)
            {
                var proposed = action.ActionType == "MoqOverride"
                    ? $"MOQ {action.Value:0}"
                    : $"订货周期 {action.Value:0}d";
                yield return BuildProposal(request, "Inventory Buffer", sku.Name, SkuCurrentValue(sku), proposed, "模板建议调整补货策略。", $"第 {action.StartWeek}-{action.EndWeek} 周", 1.2m, sku.UnitCost * action.Value * 0.06m, "Yellow", source);
            }
        }
        else if (action.ActionType == "Prebuild")
        {
            var sku = data.Skus.FirstOrDefault(item => item.Sku == action.Target);
            if (sku is not null)
            {
                yield return BuildProposal(request, "Inventory Buffer", sku.Name, SkuCurrentValue(sku), $"提前建库 {action.Value:0} {action.Unit}", "模板建议提前建库保护未来窗口。", $"第 {action.StartWeek}-{action.EndWeek} 周", 1.5m, sku.UnitCost * action.Value, "Yellow", source);
            }
        }
        else if (action.ActionType == "CapacityMultiplier")
        {
            var resource = data.Resources.FirstOrDefault(item => item.Code == action.Target);
            if (resource is not null)
            {
                yield return BuildProposal(request, "Capacity Buffer", resource.Name, $"周可用能力 {resource.WeeklyAvailableUnits:0.#}，单位负荷 {resource.UnitLoad:0.00}", $"第 {action.StartWeek}-{action.EndWeek} 周能力倍率 {action.Value:0.00}", "模板建议调整资源能力边界。", $"第 {action.StartWeek}-{action.EndWeek} 周", action.Value > 1 ? 2.0m : -1.0m, action.Value > 1 ? 520_000m : 0m, action.Value < 0.75m ? "Red" : "Yellow", source);
            }
        }
        else if (action.ActionType == "SupplierCapacityLimit")
        {
            yield return BuildProposal(request, "Supplier Master Setting", action.Target, "按当前供应商承诺能力", $"第 {action.StartWeek}-{action.EndWeek} 周承诺能力 {action.Value:0.#} {action.Unit}", "模板建议设置供应约束窗口。", $"第 {action.StartWeek}-{action.EndWeek} 周", -1.2m, 0m, "Yellow", source);
        }
    }

    private static MasterSettingChangeRequest BuildProposal(
        ScenarioRunPreviewRequest request,
        string settingType,
        string target,
        string currentValue,
        string proposedValue,
        string trigger,
        string effectiveWindow,
        decimal serviceImpact,
        decimal cashImpact,
        string riskLevel,
        string source)
    {
        return new MasterSettingChangeRequest(
            null,
            request.TemplateId,
            settingType,
            target,
            currentValue,
            proposedValue,
            trigger,
            effectiveWindow,
            "Proposed",
            decimal.Round(serviceImpact, 1),
            decimal.Round(cashImpact, 0),
            riskLevel,
            new[] { source, "由 Scenario Preview 生成，保存时作为主设置治理记录留痕。" });
    }

    private static string SkuCurrentValue(SkuBufferSetting sku)
    {
        return $"ADU {sku.Adu:0.#}, DLT {sku.DecoupledLeadTimeDays}d, VF {sku.VariabilityFactor:0.0}, MOQ {sku.MinimumOrderQuantity:0}, 订货周期 {sku.OrderCycleDays}d";
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
            CREATE TABLE IF NOT EXISTS master_setting_changes (
                change_id TEXT PRIMARY KEY,
                change_number TEXT NOT NULL UNIQUE,
                source_scenario_run_id TEXT NULL,
                source_template_id TEXT NULL,
                setting_type TEXT NOT NULL,
                target TEXT NOT NULL,
                current_value TEXT NOT NULL,
                proposed_value TEXT NOT NULL,
                trigger TEXT NOT NULL,
                effective_window TEXT NOT NULL,
                status TEXT NOT NULL,
                service_impact REAL NOT NULL,
                cash_impact REAL NOT NULL,
                risk_level TEXT NOT NULL,
                created_by TEXT NOT NULL,
                created_at_utc TEXT NOT NULL,
                proposal_json TEXT NOT NULL,
                impact_json TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS master_setting_change_audit_events (
                event_id TEXT PRIMARY KEY,
                change_id TEXT NOT NULL,
                sequence INTEGER NOT NULL,
                event_type TEXT NOT NULL,
                stage TEXT NOT NULL,
                severity TEXT NOT NULL,
                message TEXT NOT NULL,
                payload_json TEXT NULL,
                created_at_utc TEXT NOT NULL,
                FOREIGN KEY(change_id) REFERENCES master_setting_changes(change_id)
            );

            CREATE INDEX IF NOT EXISTS ix_master_setting_changes_created_at ON master_setting_changes(created_at_utc DESC);
            CREATE INDEX IF NOT EXISTS ix_master_setting_change_audit_sequence ON master_setting_change_audit_events(change_id, sequence);
            """;
        command.ExecuteNonQuery();
    }

    private int NextChangeSequence()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) + 1 FROM master_setting_changes;";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static int NextAuditSequence(SqliteConnection connection, SqliteTransaction transaction, string changeId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(*) + 1 FROM master_setting_change_audit_events WHERE change_id = $change_id;";
        command.Parameters.AddWithValue("$change_id", changeId);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private SqliteConnection OpenConnection()
    {
        var builder = new SqliteConnectionStringBuilder { DataSource = _databasePath };
        var connection = new SqliteConnection(builder.ToString());
        connection.Open();
        return connection;
    }

    private static string NormalizeInitialStatus(string status)
    {
        return status is "Proposed" or "Reviewed" or "Approved" or "Effective" or "Expired"
            ? status
            : "Proposed";
    }

    private static MasterSettingChangeSummary BuildSummary(
        string changeId,
        string changeNumber,
        string createdBy,
        string createdAtUtc,
        MasterSettingChangeRequest proposal)
    {
        return new MasterSettingChangeSummary(
            changeId,
            changeNumber,
            proposal.SourceScenarioRunId,
            proposal.SourceTemplateId,
            proposal.SettingType,
            proposal.Target,
            proposal.CurrentValue,
            proposal.ProposedValue,
            proposal.Trigger,
            proposal.EffectiveWindow,
            proposal.Status,
            proposal.ServiceImpact,
            proposal.CashImpact,
            proposal.RiskLevel,
            createdBy,
            createdAtUtc);
    }

    private static IReadOnlyList<MasterSettingChangeAuditEvent> BuildSaveAuditEvents(
        string changeId,
        string proposalJson,
        string impactJson,
        DateTimeOffset createdAt)
    {
        var createdAtText = createdAt.ToString("O");
        return new[]
        {
            new MasterSettingChangeAuditEvent(Guid.NewGuid().ToString("N"), changeId, 1, "ChangeProposed", "Governance", "Information", "收到主设置变更建议。", proposalJson, createdAtText),
            new MasterSettingChangeAuditEvent(Guid.NewGuid().ToString("N"), changeId, 2, "PreviewRecalculated", "Engine", "Information", "主设置建议来自服务端重新运行的 Scenario Preview。", null, createdAtText),
            new MasterSettingChangeAuditEvent(Guid.NewGuid().ToString("N"), changeId, 3, "ImpactCaptured", "Impact", "Information", "已保存服务、现金与风险影响快照。", impactJson, createdAtText),
            new MasterSettingChangeAuditEvent(Guid.NewGuid().ToString("N"), changeId, 4, "ChangeSaved", "Persistence", "Information", "主设置变更请求已保存，等待治理状态流转。", null, createdAtText)
        };
    }

    private static void AddChangeParameters(SqliteCommand command, MasterSettingChangeSummary summary)
    {
        command.Parameters.AddWithValue("$change_id", summary.ChangeId);
        command.Parameters.AddWithValue("$change_number", summary.ChangeNumber);
        command.Parameters.AddWithValue("$source_scenario_run_id", (object?)summary.SourceScenarioRunId ?? DBNull.Value);
        command.Parameters.AddWithValue("$source_template_id", (object?)summary.SourceTemplateId ?? DBNull.Value);
        command.Parameters.AddWithValue("$setting_type", summary.SettingType);
        command.Parameters.AddWithValue("$target", summary.Target);
        command.Parameters.AddWithValue("$current_value", summary.CurrentValue);
        command.Parameters.AddWithValue("$proposed_value", summary.ProposedValue);
        command.Parameters.AddWithValue("$trigger", summary.Trigger);
        command.Parameters.AddWithValue("$effective_window", summary.EffectiveWindow);
        command.Parameters.AddWithValue("$status", summary.Status);
        command.Parameters.AddWithValue("$service_impact", summary.ServiceImpact);
        command.Parameters.AddWithValue("$cash_impact", summary.CashImpact);
        command.Parameters.AddWithValue("$risk_level", summary.RiskLevel);
        command.Parameters.AddWithValue("$created_by", summary.CreatedBy);
        command.Parameters.AddWithValue("$created_at_utc", summary.CreatedAtUtc);
    }

    private static void InsertAuditEvent(SqliteConnection connection, SqliteTransaction transaction, MasterSettingChangeAuditEvent auditEvent)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO master_setting_change_audit_events (
                event_id, change_id, sequence, event_type, stage, severity, message, payload_json, created_at_utc)
            VALUES (
                $event_id, $change_id, $sequence, $event_type, $stage, $severity, $message, $payload_json, $created_at_utc);
            """;
        command.Parameters.AddWithValue("$event_id", auditEvent.EventId);
        command.Parameters.AddWithValue("$change_id", auditEvent.ChangeId);
        command.Parameters.AddWithValue("$sequence", auditEvent.Sequence);
        command.Parameters.AddWithValue("$event_type", auditEvent.EventType);
        command.Parameters.AddWithValue("$stage", auditEvent.Stage);
        command.Parameters.AddWithValue("$severity", auditEvent.Severity);
        command.Parameters.AddWithValue("$message", auditEvent.Message);
        command.Parameters.AddWithValue("$payload_json", (object?)auditEvent.PayloadJson ?? DBNull.Value);
        command.Parameters.AddWithValue("$created_at_utc", auditEvent.CreatedAtUtc);
        command.ExecuteNonQuery();
    }

    private static MasterSettingChangeSummary ReadSummary(SqliteDataReader reader)
    {
        return new MasterSettingChangeSummary(
            reader.GetString(0),
            reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.GetString(6),
            reader.GetString(7),
            reader.GetString(8),
            reader.GetString(9),
            reader.GetString(10),
            reader.GetDecimal(11),
            reader.GetDecimal(12),
            reader.GetString(13),
            reader.GetString(14),
            reader.GetString(15));
    }

    private static int StatusRank(string status)
    {
        return status switch
        {
            "Current" => 0,
            "Proposed" => 1,
            "Reviewed" => 2,
            "Approved" => 3,
            "Effective" => 4,
            "Expired" => 5,
            _ => 99
        };
    }
}
