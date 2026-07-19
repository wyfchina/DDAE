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
    private readonly IScenarioRunLineageReader? _scenarioRunLineageReader;

    public MasterSettingsGovernanceService(
        IScenarioWorkspaceDataSource dataSource,
        ScenarioRunPreviewService previewService,
        string databasePath)
        : this(dataSource, previewService, null, databasePath)
    {
    }

    public MasterSettingsGovernanceService(
        IScenarioWorkspaceDataSource dataSource,
        ScenarioRunPreviewService previewService,
        IScenarioRunLineageReader? scenarioRunLineageReader,
        string databasePath)
    {
        _dataSource = dataSource;
        _previewService = previewService;
        _scenarioRunLineageReader = scenarioRunLineageReader;
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
        return BuildProposalResponse(safeRequest, preview, data);
    }

    public MasterSettingProposalResponse ProposeFromFrozenComparison(
        ScenarioComparisonCase comparisonCase,
        CurrentBaselineSnapshot frozenBaseline,
        string sourceScenarioRunId,
        GovernanceDecisionContext governanceContext)
    {
        var savedRun = RequireFrozenComparisonRun(
            sourceScenarioRunId,
            frozenBaseline.SnapshotId,
            comparisonCase.ExternalScenarioId,
            comparisonCase.ResponseId);
        EnsureSameNormalizedRunRequest(comparisonCase.Preview.Request, savedRun.Request);
        var context = governanceContext with
        {
            SourceBaselineId = frozenBaseline.SnapshotId,
            SourceScenarioRunId = sourceScenarioRunId.Trim()
        };
        var request = savedRun.Request with { GovernanceContext = context };
        var preview = _previewService.PreviewAgainstFrozenBaseline(request, frozenBaseline);
        var data = _previewService.LoadFrozenWorkspaceData(preview.Request, frozenBaseline);
        return BuildProposalResponse(preview.Request, preview, data);
    }

    public MasterSettingProposalResponse ProposeFromSavedRun(
        string runId,
        CurrentBaselineSnapshot baseline,
        GovernanceDecisionContext context)
    {
        var preview = PreviewSavedRunAgainstFrozenBaseline(runId, baseline, context);
        var data = _previewService.LoadFrozenWorkspaceData(preview.Request, baseline);
        return BuildProposalResponse(preview.Request, preview, data);
    }

    public ScenarioRunPreviewResult PreviewSavedRunAgainstFrozenBaseline(
        string runId,
        CurrentBaselineSnapshot baseline,
        GovernanceDecisionContext context)
    {
        if (string.IsNullOrWhiteSpace(runId))
        {
            throw new ArgumentException("必须提供已保存的冻结比较 run 标识。", nameof(runId));
        }

        var summary = _scenarioRunLineageReader?.GetSummary(runId.Trim())
            ?? throw new ArgumentException("冻结比较 run 不存在。", nameof(runId));
        if (!string.Equals(summary.Status, "Saved", StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(summary.BaselineSnapshotId)
            || string.IsNullOrWhiteSpace(summary.ExternalScenarioId)
            || string.IsNullOrWhiteSpace(summary.ResponseId))
        {
            throw new ArgumentException("DDOM 建议必须来自已保存的冻结比较 run。", nameof(runId));
        }
        if (!string.Equals(summary.BaselineSnapshotId, baseline.SnapshotId, StringComparison.Ordinal))
        {
            throw new ArgumentException("保存 run 的冻结基线与当前包来源不一致。", nameof(runId));
        }

        var savedRun = RequireFrozenComparisonRun(
            runId,
            baseline.SnapshotId,
            summary.ExternalScenarioId,
            summary.ResponseId);
        var governedContext = context with
        {
            SourceBaselineId = baseline.SnapshotId,
            SourceScenarioRunId = runId.Trim()
        };
        var request = savedRun.Request with { GovernanceContext = governedContext };
        return _previewService.PreviewAgainstFrozenBaseline(request, baseline);
    }

    internal ScenarioRunPreviewResult PreviewFrozenPackageRequest(
        ScenarioRunPreviewRequest finalRequest,
        CurrentBaselineSnapshot baseline) =>
        _previewService.PreviewAgainstFrozenBaseline(finalRequest, baseline);

    private static MasterSettingProposalResponse BuildProposalResponse(
        ScenarioRunPreviewRequest safeRequest,
        ScenarioRunPreviewResult preview,
        ScenarioWorkspaceDataSet data)
    {
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

        var proposal = ValidateCreationMetadata(request.Change);

        var createdAt = DateTimeOffset.UtcNow;
        var createdAtText = createdAt.ToString("O");
        var changeId = Guid.NewGuid().ToString("N");
        var changeNumber = $"MSG-{createdAt:yyyyMMdd}-{NextChangeSequence():0000}";
        var createdBy = string.IsNullOrWhiteSpace(request.CreatedBy) ? "计划员" : request.CreatedBy.Trim();
        const string status = "Proposed";
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
                    source_baseline_id, creation_method, proposal_json, impact_json)
                VALUES (
                    $change_id, $change_number, $source_scenario_run_id, $source_template_id,
                    $setting_type, $target, $current_value, $proposed_value, $trigger, $effective_window,
                    $status, $service_impact, $cash_impact, $risk_level, $created_by, $created_at_utc,
                    $source_baseline_id, $creation_method, $proposal_json, $impact_json);
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

    public IReadOnlyList<MasterSettingChangeSummary> ListChanges(
        int limit,
        string? sourceBaselineId = null,
        string? sourceScenarioRunId = null)
    {
        var boundedLimit = Math.Clamp(limit <= 0 ? 50 : limit, 1, 200);
        return QueryChanges(boundedLimit, sourceBaselineId, sourceScenarioRunId);
    }

    internal IReadOnlyList<MasterSettingChangeSummary> ListChangesByLineage(
        string? sourceBaselineId = null,
        string? sourceScenarioRunId = null) =>
        QueryChanges(null, sourceBaselineId, sourceScenarioRunId);

    private IReadOnlyList<MasterSettingChangeSummary> QueryChanges(
        int? limit,
        string? sourceBaselineId,
        string? sourceScenarioRunId)
    {
        var baselineFilter = NormalizeFilter(sourceBaselineId);
        var scenarioRunFilter = NormalizeFilter(sourceScenarioRunId);
        var predicates = new List<string>();
        if (baselineFilter is not null)
        {
            predicates.Add("source_baseline_id = $source_baseline_id");
        }
        if (scenarioRunFilter is not null)
        {
            predicates.Add("source_scenario_run_id = $source_scenario_run_id");
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        var whereClause = predicates.Count == 0
            ? string.Empty
            : $"WHERE {string.Join(" AND ", predicates)}";
        var limitClause = limit.HasValue ? "LIMIT $limit" : string.Empty;
        command.CommandText = $"""
            SELECT change_id, change_number, source_scenario_run_id, source_template_id,
                   setting_type, target, current_value, proposed_value, trigger, effective_window,
                   status, service_impact, cash_impact, risk_level, created_by, created_at_utc,
                   source_baseline_id, creation_method
            FROM master_setting_changes
            {whereClause}
            ORDER BY created_at_utc DESC
            {limitClause};
            """;
        if (baselineFilter is not null)
        {
            command.Parameters.AddWithValue("$source_baseline_id", baselineFilter);
        }
        if (scenarioRunFilter is not null)
        {
            command.Parameters.AddWithValue("$source_scenario_run_id", scenarioRunFilter);
        }
        if (limit.HasValue)
        {
            command.Parameters.AddWithValue("$limit", limit.Value);
        }

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
                   source_baseline_id, creation_method, proposal_json, impact_json
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
        var proposal = JsonSerializer.Deserialize<MasterSettingChangeRequest>(reader.GetString(18), JsonOptions)
            ?? new MasterSettingChangeRequest(null, null, summary.SettingType, summary.Target, summary.CurrentValue, summary.ProposedValue, summary.Trigger, summary.EffectiveWindow, summary.Status, summary.ServiceImpact, summary.CashImpact, summary.RiskLevel, Array.Empty<string>());
        var impact = JsonSerializer.Deserialize<MasterSettingChangeImpact>(reader.GetString(19), JsonOptions)
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
        if (!string.IsNullOrWhiteSpace(detail.Summary.SourceScenarioRunId))
        {
            throw new InvalidOperationException("场景派生变更必须通过 DDOM 变更包治理，不能推进单条主设置记录。");
        }
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
                proposedParts.Add($"订货周期 {policy.OrderCycleDays} 天");
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
                    "重审 ADU / DLT / 提前期因子 / 波动因子 / MOQ / 订货周期",
                    "预览结果仍存在红区 SKU。",
                    "下一轮 DDS&OP 生效窗口",
                    2.0m,
                    sku.UnitCost * sku.MinimumOrderQuantity,
                    "Red",
                    "SystemSuggested");
            }
        }

        foreach (var resource in preview.Scenario.Rccp.ResourceSummaries.Where(item => item.Status == "Red").Take(2))
        {
            yield return BuildProposal(
                preview.Request,
                "Capacity Buffer",
                resource.ResourceName,
                "按当前资源日历与保护能力执行",
                $"设置保护能力边界，补货释放峰值 {resource.PeakLoadPercent:0.#}%",
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
                    : $"订货周期 {action.Value:0} 天";
                yield return BuildProposal(request, "Inventory Buffer", sku.Name, SkuCurrentValue(sku), proposed, "模板建议调整补货策略。", $"第 {action.StartWeek}-{action.EndWeek} 周", 1.2m, sku.UnitCost * action.Value * 0.06m, "Yellow", source);
            }
        }
        else if (action.ActionType == "Prebuild")
        {
            var sku = data.Skus.FirstOrDefault(item => item.Sku == action.Target);
            if (sku is not null)
            {
                yield return BuildProposal(request, "Inventory Buffer", sku.Name, SkuCurrentValue(sku), $"提前建库 {action.Value:0} {BusinessUnitLabel(action.Unit)}", "模板建议提前建库保护未来窗口。", $"第 {action.StartWeek}-{action.EndWeek} 周", 1.5m, sku.UnitCost * action.Value, "Yellow", source);
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
            yield return BuildProposal(request, "Supplier Master Setting", action.Target, "按当前供应商承诺能力", $"第 {action.StartWeek}-{action.EndWeek} 周承诺能力 {action.Value:0.#} {BusinessUnitLabel(action.Unit)}", "模板建议设置供应约束窗口。", $"第 {action.StartWeek}-{action.EndWeek} 周", -1.2m, 0m, "Yellow", source);
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
        var changeCategory = source is "PrebuildCampaign" or "ResourceCapacityAdjustment" or "SupplierCapacityLimit"
            ? "TemporaryAdjustment"
            : "MasterParameter";
        var context = request.GovernanceContext;
        return new MasterSettingChangeRequest(
            context?.SourceScenarioRunId,
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
            new[] { source, "由场景预览生成，保存时作为主设置治理记录留痕。" },
            ChangeCategory: changeCategory,
            SourceBaselineId: context?.SourceBaselineId,
            Owner: context?.Owner,
            Approver: context?.Approver,
            EffectiveFrom: string.IsNullOrWhiteSpace(context?.EffectiveFrom) ? effectiveWindow : context.EffectiveFrom,
            EffectiveThrough: context?.EffectiveThrough,
            ReviewOn: string.IsNullOrWhiteSpace(context?.ReviewOn) ? "下一次 DDS&OP 复查点" : context.ReviewOn,
            ExpectedEffect: string.IsNullOrWhiteSpace(context?.ExpectedEffect) ? $"服务影响 {serviceImpact:0.0}pp；现金影响 {cashImpact:0}" : context.ExpectedEffect,
            RollbackCondition: string.IsNullOrWhiteSpace(context?.RollbackCondition) ? "实际效果偏离预期或保护能力恶化时人工回滚" : context.RollbackCondition,
            CreationMethod: string.IsNullOrWhiteSpace(context?.SourceScenarioRunId) ? "Manual" : "ScenarioDerived");
    }

    private static string SkuCurrentValue(SkuBufferSetting sku)
    {
        return $"ADU {sku.Adu:0.#}, DLT {sku.DecoupledLeadTimeDays} 天, 提前期因子 {sku.LeadTimeFactor:0.00}, 波动因子 {sku.VariabilityFactor:0.00}, MOQ {sku.MinimumOrderQuantity:0}, 订货周期 {sku.OrderCycleDays} 天";
    }

    private static string BusinessUnitLabel(string unit) => unit switch
    {
        "units" => "件",
        "units/week" => "件/周",
        "factor" => "倍",
        "days" => "天",
        _ => "单位未定义"
    };

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
                source_baseline_id TEXT NULL,
                creation_method TEXT NOT NULL DEFAULT 'Legacy',
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
        EnsureColumn(connection, "source_baseline_id", "TEXT NULL");
        EnsureColumn(connection, "creation_method", "TEXT NOT NULL DEFAULT 'Legacy'");

        using var lineageIndexes = connection.CreateCommand();
        lineageIndexes.CommandText = """
            CREATE INDEX IF NOT EXISTS ix_master_setting_changes_source_baseline ON master_setting_changes(source_baseline_id);
            CREATE INDEX IF NOT EXISTS ix_master_setting_changes_source_scenario_run ON master_setting_changes(source_scenario_run_id);
            """;
        lineageIndexes.ExecuteNonQuery();
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

    private MasterSettingChangeRequest ValidateCreationMetadata(MasterSettingChangeRequest change)
    {
        if (!string.IsNullOrWhiteSpace(change.SourceScenarioRunId))
        {
            throw new InvalidOperationException("场景派生变更必须通过 DDOM 变更包，不能新建单条主设置记录。");
        }
        if (change.CreationMethod is not ("Manual" or "ScenarioDerived"))
        {
            throw new ArgumentException("创建方式只能为 Manual 或 ScenarioDerived；Legacy 仅用于读取历史记录。", nameof(change));
        }
        if (string.IsNullOrWhiteSpace(change.SourceBaselineId))
        {
            throw new ArgumentException("主设置变更必须关联来源冻结基线。", nameof(change));
        }

        if (change.CreationMethod == "ScenarioDerived")
        {
            if (string.IsNullOrWhiteSpace(change.SourceScenarioRunId))
            {
                throw new ArgumentException("场景派生变更必须关联已保存的冻结比较 run。", nameof(change));
            }
        }

        return change with
        {
            SourceBaselineId = change.SourceBaselineId.Trim(),
            SourceScenarioRunId = string.IsNullOrWhiteSpace(change.SourceScenarioRunId) ? null : change.SourceScenarioRunId.Trim(),
            Status = "Proposed"
        };
    }

    private ScenarioRunDetail RequireFrozenComparisonRun(
        string sourceScenarioRunId,
        string baselineSnapshotId,
        string externalScenarioId,
        string responseId)
    {
        if (string.IsNullOrWhiteSpace(sourceScenarioRunId))
        {
            throw new ArgumentException("必须提供已保存的冻结比较 run 标识。", nameof(sourceScenarioRunId));
        }

        var run = _scenarioRunLineageReader?.GetSummary(sourceScenarioRunId.Trim())
            ?? throw new ArgumentException("冻结比较 run 不存在。", nameof(sourceScenarioRunId));
        if (!string.Equals(run.Status, "Saved", StringComparison.Ordinal))
        {
            throw new ArgumentException("冻结比较 run 尚未保存。", nameof(sourceScenarioRunId));
        }
        if (!string.Equals(run.BaselineSnapshotId, baselineSnapshotId, StringComparison.Ordinal))
        {
            throw new ArgumentException("冻结比较 run 不属于请求的基线。", nameof(sourceScenarioRunId));
        }
        if (!string.Equals(run.ExternalScenarioId, externalScenarioId, StringComparison.Ordinal))
        {
            throw new ArgumentException("冻结比较 run 不属于请求的外部场景。", nameof(sourceScenarioRunId));
        }
        if (string.IsNullOrWhiteSpace(run.ResponseId)
            || !string.Equals(run.ResponseId, responseId, StringComparison.Ordinal))
        {
            throw new ArgumentException("冻结比较 run 不属于请求的响应方案。", nameof(sourceScenarioRunId));
        }

        return _scenarioRunLineageReader?.GetDetail(sourceScenarioRunId.Trim())
            ?? throw new ArgumentException("冻结比较 run 缺少可复现的持久化请求。", nameof(sourceScenarioRunId));
    }

    private static void EnsureSameNormalizedRunRequest(
        ScenarioRunPreviewRequest submittedRequest,
        ScenarioRunPreviewRequest persistedRequest)
    {
        var submittedJson = JsonSerializer.Serialize(submittedRequest, JsonOptions);
        var persistedJson = JsonSerializer.Serialize(persistedRequest, JsonOptions);
        if (!string.Equals(submittedJson, persistedJson, StringComparison.Ordinal))
        {
            throw new ArgumentException("冻结比较内容与已保存 run 的规范化请求不一致。", nameof(submittedRequest));
        }
    }

    private static void EnsureColumn(SqliteConnection connection, string columnName, string definition)
    {
        using (var inspect = connection.CreateCommand())
        {
            inspect.CommandText = "PRAGMA table_info(master_setting_changes);";
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
        alter.CommandText = $"ALTER TABLE master_setting_changes ADD COLUMN {columnName} {definition};";
        alter.ExecuteNonQuery();
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
            createdAtUtc,
            proposal.SourceBaselineId,
            proposal.CreationMethod);
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
            new MasterSettingChangeAuditEvent(Guid.NewGuid().ToString("N"), changeId, 2, "PreviewRecalculated", "Engine", "Information", "主设置建议来自服务端重新运行的场景预览。", null, createdAtText),
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
        command.Parameters.AddWithValue("$source_baseline_id", (object?)summary.SourceBaselineId ?? DBNull.Value);
        command.Parameters.AddWithValue("$creation_method", summary.CreationMethod);
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
            reader.GetString(15),
            reader.IsDBNull(16) ? null : reader.GetString(16),
            reader.GetString(17));
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

    private static string? NormalizeFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
