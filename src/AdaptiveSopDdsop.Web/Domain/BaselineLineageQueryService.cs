namespace AdaptiveSopDdsop.Web.Domain;

public sealed record BaselineLineageResult(
    string BaselineSnapshotId,
    IReadOnlyList<ScenarioRunSummary> ScenarioRuns,
    IReadOnlyList<MasterSettingChangeSummary> MasterSettingChanges,
    IReadOnlyList<CoordinationItem> CoordinationItems);

public interface IBaselineLineageQueryService
{
    BaselineLineageResult Get(string baselineSnapshotId);
}

public sealed class BaselineLineageQueryService : IBaselineLineageQueryService
{
    private readonly ScenarioRunPersistenceService _scenarioRuns;
    private readonly MasterSettingsGovernanceService _masterSettingsGovernance;
    private readonly CoordinationLedgerService _coordinationLedger;

    public BaselineLineageQueryService(
        ScenarioRunPersistenceService scenarioRuns,
        MasterSettingsGovernanceService masterSettingsGovernance,
        CoordinationLedgerService coordinationLedger)
    {
        _scenarioRuns = scenarioRuns;
        _masterSettingsGovernance = masterSettingsGovernance;
        _coordinationLedger = coordinationLedger;
    }

    public BaselineLineageResult Get(string baselineSnapshotId)
    {
        if (string.IsNullOrWhiteSpace(baselineSnapshotId))
        {
            throw new ArgumentException("冻结基线标识不能为空。", nameof(baselineSnapshotId));
        }

        var normalizedBaselineId = baselineSnapshotId.Trim();
        var runs = _scenarioRuns.ListByLineage(normalizedBaselineId, null);
        var changes = _masterSettingsGovernance.ListChangesByLineage(normalizedBaselineId, null);
        var coordinationItems = runs
            .SelectMany(run => _coordinationLedger.ListByLineage(run.RunId, null))
            .Concat(changes.SelectMany(change => _coordinationLedger.ListByLineage(null, change.ChangeId)))
            .GroupBy(item => item.ItemId, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.CreatedAtUtc, StringComparer.Ordinal)
            .ThenBy(item => item.ItemId, StringComparer.Ordinal)
            .ToList();

        return new BaselineLineageResult(normalizedBaselineId, runs, changes, coordinationItems);
    }
}
