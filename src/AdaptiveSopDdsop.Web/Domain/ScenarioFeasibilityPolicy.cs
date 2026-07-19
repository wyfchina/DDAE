namespace AdaptiveSopDdsop.Web.Domain;

public sealed record ScenarioFeasibilityCheck(
    string Code,
    string Metric,
    decimal? Actual,
    decimal? YellowLimit,
    decimal? RedLimit,
    string Unit,
    string Status,
    string Message);

public sealed record ScenarioFeasibilityAssessment(
    string Status,
    string Label,
    bool IsBlocked,
    string ConstraintMode,
    IReadOnlyList<ScenarioFeasibilityCheck> Checks,
    IReadOnlyList<string> Violations,
    IReadOnlyList<string> CoordinationItems);

public static class ScenarioFeasibilityPolicy
{
    public static ScenarioFeasibilityAssessment Evaluate(
        ScenarioRunPreviewResult result,
        ScenarioWorkspaceDataSet data)
    {
        var baselineKeys = result.Baseline.Plan.BufferProjections.Select(item => (item.Sku, item.Week)).ToList();
        var scenarioKeys = result.Scenario.Plan.BufferProjections.Select(item => (item.Sku, item.Week)).ToList();
        var evidenceComplete = InventoryFlowEvidenceValidator.IsComplete(
                result.Baseline.CaseId,
                result.Baseline.InventoryFlow,
                baselineKeys) &&
            InventoryFlowEvidenceValidator.IsComplete(
                result.Scenario.CaseId,
                result.Scenario.InventoryFlow,
                scenarioKeys) &&
            result.Baseline.Metrics.AverageInventoryValue.HasValue &&
            result.Scenario.Metrics.AverageInventoryValue.HasValue;
        var targetService = data.Families.Count == 0 ? 95m : data.Families.Average(item => item.TargetServiceLevel);
        var serviceReference = Math.Max(targetService, result.Baseline.Metrics.ServiceLevelPercent);
        var serviceLoss = Math.Max(0m, serviceReference - result.Scenario.Metrics.ServiceLevelPercent);
        var supplyRequired = result.Scenario.SupplierCapacity.Sum(item => item.RequiredQuantity);
        var supplyGapRatio = supplyRequired <= 0m
            ? 0m
            : Math.Max(0m, result.Scenario.SupplierCapacity.Sum(item => item.Gap)) * 100m / supplyRequired;
        var baselineInventory = result.Baseline.Metrics.AverageInventoryValue ?? 0m;
        var scenarioInventory = result.Scenario.Metrics.AverageInventoryValue ?? 0m;
        var inventoryIncrease = baselineInventory <= 0m
            ? 0m
            : Math.Max(0m, scenarioInventory - baselineInventory) * 100m / baselineInventory;
        var redDuration = MaximumConsecutiveRedWeeks(result.Scenario.Plan.BufferProjections);
        var targetFlow = data.Families.Count == 0 ? 85m : data.Families.Average(item => item.TargetFlowIndex);
        var flowGap = Math.Max(0m, targetFlow - result.Scenario.Metrics.FlowIndex);

        var checks = new Dictionary<string, ScenarioFeasibilityCheck>(StringComparer.Ordinal)
        {
            ["Evidence"] = new(
                "Evidence", "Physical inventory evidence", null, null, null, "", evidenceComplete ? "Green" : "Red",
                evidenceComplete ? "Physical inventory projections are complete." : "Physical inventory projection evidence is incomplete."),
            ["Service"] = CreateThresholdCheck("Service", "Service target/baseline loss", serviceLoss, 1m, 3m, "pp"),
            ["Capacity"] = CreateThresholdCheck("Capacity", "Peak capacity load", result.Scenario.Metrics.PeakLoadPercent, 85m, 100m, "%"),
            ["Supply"] = CreateThresholdCheck("Supply", "Supply gap / required supply", supplyGapRatio, 5m, 15m, "%"),
            ["Inventory"] = CreateThresholdCheck("Inventory", "Average inventory increase vs baseline", inventoryIncrease, 5m, 12m, "%"),
            ["RedDuration"] = CreateThresholdCheck("RedDuration", "Maximum consecutive red weeks", redDuration, 1m, 3m, "weeks"),
            ["Flow"] = new(
                "Flow", "Flow target gap", flowGap, 0m, null, "pp",
                flowGap > 0m ? "Yellow" : "Green",
                flowGap > 0m ? "Flow gap requires coordination; it is not a hard feasibility block." : "Flow target is met.")
        };

        var constraintMode = NormalizeMode(result.Request.AdoptionConstraintMode);
        var orderedChecks = CheckOrder(constraintMode).Select(code => checks[code]).ToList();
        var violations = orderedChecks.Where(item => item.Status == "Red").Select(item => item.Message).ToList();
        var coordinationItems = orderedChecks.Where(item => item.Status == "Yellow").Select(item => item.Message).ToList();
        var isBlocked = violations.Count > 0;
        var status = isBlocked ? "Blocked" : coordinationItems.Count > 0 ? "Reconcile" : "Adoptable";
        var label = status switch
        {
            "Blocked" => "阻断候选",
            "Reconcile" => "需要协调",
            _ => "可作为候选"
        };

        return new ScenarioFeasibilityAssessment(
            status,
            label,
            isBlocked,
            constraintMode,
            orderedChecks,
            violations,
            coordinationItems);
    }

    private static ScenarioFeasibilityCheck CreateThresholdCheck(
        string code,
        string metric,
        decimal actual,
        decimal yellowLimit,
        decimal redLimit,
        string unit)
    {
        var status = actual > redLimit ? "Red" : actual > yellowLimit ? "Yellow" : "Green";
        var message = status switch
        {
            "Red" => $"{metric} {actual:0.##}{unit} exceeds the hard limit {redLimit:0.##}{unit}.",
            "Yellow" => $"{metric} {actual:0.##}{unit} exceeds the coordination limit {yellowLimit:0.##}{unit}.",
            _ => $"{metric} is within the feasible range."
        };
        return new ScenarioFeasibilityCheck(code, metric, actual, yellowLimit, redLimit, unit, status, message);
    }

    private static int MaximumConsecutiveRedWeeks(IReadOnlyList<BufferProjectionPoint> projections) => projections
        .GroupBy(item => item.Sku, StringComparer.Ordinal)
        .Select(group =>
        {
            var maximum = 0;
            var current = 0;
            var previousWeek = 0;
            foreach (var point in group.OrderBy(item => item.Week))
            {
                current = point.BufferStatus == "Red" && (previousWeek == 0 || point.Week == previousWeek + 1)
                    ? current + 1
                    : point.BufferStatus == "Red" ? 1 : 0;
                maximum = Math.Max(maximum, current);
                previousWeek = point.Week;
            }
            return maximum;
        })
        .DefaultIfEmpty(0)
        .Max();

    private static string NormalizeMode(string? mode) => mode switch
    {
        "ServiceFirst" or "FlowFirst" or "CashFirst" or "CapacityFirst" or "SupplyFirst" => mode,
        _ => "Balanced"
    };

    private static IReadOnlyList<string> CheckOrder(string mode) => mode switch
    {
        "ServiceFirst" => new[] { "Service", "Evidence", "Capacity", "Supply", "Inventory", "RedDuration", "Flow" },
        "FlowFirst" => new[] { "Flow", "Evidence", "Service", "Capacity", "Supply", "Inventory", "RedDuration" },
        "CashFirst" => new[] { "Inventory", "Evidence", "Service", "Capacity", "Supply", "RedDuration", "Flow" },
        "CapacityFirst" => new[] { "Capacity", "Evidence", "Service", "Supply", "Inventory", "RedDuration", "Flow" },
        "SupplyFirst" => new[] { "Supply", "Evidence", "Service", "Capacity", "Inventory", "RedDuration", "Flow" },
        _ => new[] { "Evidence", "Service", "Capacity", "Supply", "Inventory", "RedDuration", "Flow" }
    };
}
