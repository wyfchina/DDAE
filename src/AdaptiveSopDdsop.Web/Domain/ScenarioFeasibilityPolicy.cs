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
        var serviceLoss = WorstFamilyServiceLoss(result, data);
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
                "Evidence", "实体库存证据", null, null, null, "无", evidenceComplete ? "Green" : "Red",
                evidenceComplete ? "实体库存投影证据完整。" : "实体库存投影证据不完整。"),
            ["Service"] = CreateThresholdCheck("Service", "最差产品族服务目标/基准损失", serviceLoss, 1m, 3m, "百分点"),
            ["Capacity"] = CreateThresholdCheck("Capacity", "峰值产能负荷", result.Scenario.Metrics.PeakLoadPercent, 85m, 100m, "%"),
            ["Supply"] = CreateThresholdCheck("Supply", "供应缺口/需求供应量", supplyGapRatio, 5m, 15m, "%"),
            ["Inventory"] = CreateThresholdCheck("Inventory", "相对基准的平均库存增幅", inventoryIncrease, 5m, 12m, "%"),
            ["RedDuration"] = CreateThresholdCheck("RedDuration", "最大连续红区周数", redDuration, 1m, 3m, "周"),
            ["Flow"] = new(
                "Flow", "流速目标差距", flowGap, 0m, null, "百分点",
                flowGap > 0m ? "Yellow" : "Green",
                flowGap > 0m ? "流速差距需要协调，但不构成可行性硬阻断。" : "流速目标已满足。")
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
            "Red" => $"{metric}为 {actual:0.##}{unit}，超过硬性红线 {redLimit:0.##}{unit}。",
            "Yellow" => $"{metric}为 {actual:0.##}{unit}，超过协调黄线 {yellowLimit:0.##}{unit}。",
            _ => $"{metric}处于可行范围内。"
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

    private static decimal WorstFamilyServiceLoss(ScenarioRunPreviewResult result, ScenarioWorkspaceDataSet data)
    {
        var scenarioFamilies = result.Scenario.ProductFamilyDashboard.Summaries;
        if (scenarioFamilies.Count == 0)
        {
            var targetService = data.Families.Count == 0 ? 95m : data.Families.Average(item => item.TargetServiceLevel);
            var serviceReference = Math.Max(targetService, result.Baseline.Metrics.ServiceLevelPercent);
            return Math.Max(0m, serviceReference - result.Scenario.Metrics.ServiceLevelPercent);
        }

        var baselineServices = result.Baseline.ProductFamilyDashboard.Summaries
            .ToDictionary(item => item.Family, item => item.ServiceLevelPercent, StringComparer.Ordinal);
        return scenarioFamilies
            .Select(family =>
            {
                var baselineService = baselineServices.TryGetValue(family.Family, out var value)
                    ? value
                    : family.TargetServiceLevel;
                var reference = Math.Max(family.TargetServiceLevel, baselineService);
                return Math.Max(0m, reference - family.ServiceLevelPercent);
            })
            .DefaultIfEmpty(0m)
            .Max();
    }

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
