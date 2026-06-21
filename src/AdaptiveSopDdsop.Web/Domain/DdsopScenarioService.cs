namespace AdaptiveSopDdsop.Web.Domain;

public sealed class DdsopScenarioService
{
    private readonly ValidationData _data;
    private readonly DdmrpCalculator _calculator;

    public DdsopScenarioService(ValidationData data, DdmrpCalculator calculator)
    {
        _data = data;
        _calculator = calculator;
    }

    public ScenarioResult Evaluate(ScenarioInput input)
    {
        var promotionMultiplier = 1 + input.PromotionPercent / 100m;
        var disruptionDemandDays = Math.Max(0, input.SupplyDisruptionWeeks) * 5;
        var shutdownCapacityLoss = Math.Clamp(input.PlannedShutdownDays / 5m, 0m, 1m);

        var results = new List<SkuScenarioResult>();
        foreach (var sku in _data.Skus)
        {
            var scenarioAdu = sku.Adu * promotionMultiplier;
            if (sku.Sku.Contains("NPI", StringComparison.OrdinalIgnoreCase))
            {
                scenarioAdu += input.NewProductWeeklyDemand / 5m;
            }

            var adjustedSku = sku with { Adu = scenarioAdu };
            var basePosition = _data.Inventory.First(x => x.Sku == sku.Sku);
            var disruptedPosition = basePosition with
            {
                OpenSupply = input.SupplyDisruptionWeeks > 0 ? basePosition.OpenSupply * 0.35m : basePosition.OpenSupply,
                QualifiedDemand = basePosition.QualifiedDemand + scenarioAdu * disruptionDemandDays
            };

            var zones = DdmrpCalculator.CalculateZones(adjustedSku);
            var recommendation = DdmrpCalculator.CalculateRecommendation(adjustedSku, disruptedPosition);
            results.Add(new SkuScenarioResult(
                sku.Sku,
                sku.Name,
                sku.Family,
                decimal.Round(scenarioAdu, 1),
                zones,
                recommendation.NetFlowPosition,
                recommendation.BufferStatus,
                recommendation.OrderQuantity,
                recommendation.WorkingCapital));
        }

        var metrics = CalculateScenarioMetrics(results, shutdownCapacityLoss);
        var baseline = CalculateBaselineMetrics();
        var guardrail = BuildGuardrail(
            input,
            metrics.WorkingCapital,
            baseline.WorkingCapital,
            metrics.BufferHealth,
            baseline.BufferHealth,
            metrics.Utilization,
            metrics.Service,
            baseline.Service,
            metrics.FlowIndex,
            baseline.FlowIndex);

        return new ScenarioResult(
            input,
            results,
            decimal.Round(metrics.TotalAdu, 1),
            decimal.Round(metrics.WorkingCapital, 2),
            metrics.BufferHealth,
            metrics.Utilization,
            decimal.Round(metrics.Service, 1),
            decimal.Round(metrics.FlowIndex, 1),
            guardrail,
            BuildActions(input, results, metrics.BufferHealth, metrics.Utilization, metrics.WorkingCapital, guardrail));
    }

    public ValidationData GetValidationData() => _data;

    public DemandDrivenPlanResult EvaluateDemandDrivenPlan(int horizonWeeks)
    {
        var bufferRun = DemandDrivenPlanningEngine.ProjectBuffers(
            _data.Skus,
            _data.Inventory,
            _data.Demand,
            horizonWeeks);
        var capacityLoads = DemandDrivenPlanningEngine.ProjectRoughCutCapacity(
            bufferRun.ReplenishmentOrders,
            _data.ResourceRoutings,
            _data.Resources,
            horizonWeeks);
        var supplyRequirements = DemandDrivenPlanningEngine.ProjectSupplyRequirements(
            bufferRun.ReplenishmentOrders,
            _data.SupplierItemSources);

        return new DemandDrivenPlanResult(
            bufferRun.BufferProjections,
            bufferRun.ReplenishmentOrders,
            capacityLoads,
            supplyRequirements,
            bufferRun.Traces);
    }

    private (decimal TotalAdu, decimal WorkingCapital, decimal BufferHealth, decimal Utilization, decimal Service, decimal FlowIndex) CalculateScenarioMetrics(
        IReadOnlyList<SkuScenarioResult> results,
        decimal shutdownCapacityLoss)
    {
        var totalAdu = results.Sum(x => x.Adu);
        var workingCapital = results.Sum(x => x.WorkingCapital);
        var healthy = results.Count(x => x.BufferStatus is "Green" or "OverTopOfGreen");
        var bufferHealth = results.Count == 0 ? 0 : decimal.Round(healthy * 100m / results.Count, 1);
        var totalLoad = totalAdu * 5m;
        var totalCapacity = _data.Resources.Sum(r => r.WeeklyAvailableUnits / Math.Max(r.UnitLoad, 0.0001m));
        var effectiveCapacity = totalCapacity * (1 - shutdownCapacityLoss);
        var utilization = effectiveCapacity <= 0 ? 999 : decimal.Round(totalLoad * 100m / effectiveCapacity, 1);
        var service = CalculateService(bufferHealth, utilization);
        var flowIndex = CalculateFlowIndex(bufferHealth, utilization);

        return (totalAdu, workingCapital, bufferHealth, utilization, service, flowIndex);
    }

    private (decimal WorkingCapital, decimal BufferHealth, decimal Utilization, decimal Service, decimal FlowIndex) CalculateBaselineMetrics()
    {
        var baselineResults = _data.Skus.Select(sku =>
        {
            var position = _data.Inventory.First(x => x.Sku == sku.Sku);
            return DdmrpCalculator.CalculateRecommendation(sku, position);
        }).ToList();

        var healthy = baselineResults.Count(x => x.BufferStatus is "Green" or "OverTopOfGreen");
        var bufferHealth = baselineResults.Count == 0 ? 0 : decimal.Round(healthy * 100m / baselineResults.Count, 1);
        var workingCapital = baselineResults.Sum(x => x.WorkingCapital);
        var totalAdu = _data.Skus.Sum(x => x.Adu);
        var totalLoad = totalAdu * 5m;
        var totalCapacity = _data.Resources.Sum(r => r.WeeklyAvailableUnits / Math.Max(r.UnitLoad, 0.0001m));
        var utilization = totalCapacity <= 0 ? 999 : decimal.Round(totalLoad * 100m / totalCapacity, 1);
        var service = CalculateService(bufferHealth, utilization);
        var flowIndex = CalculateFlowIndex(bufferHealth, utilization);

        return (workingCapital, bufferHealth, utilization, service, flowIndex);
    }

    private static decimal CalculateService(decimal bufferHealth, decimal utilization)
    {
        return Math.Clamp(98m - Math.Max(0, utilization - 95m) * 0.35m - Math.Max(0, 80m - bufferHealth) * 0.25m, 70m, 99m);
    }

    private static decimal CalculateFlowIndex(decimal bufferHealth, decimal utilization)
    {
        return Math.Clamp(100m - Math.Max(0, utilization - 85m) * 0.6m - Math.Max(0, 90m - bufferHealth) * 0.35m, 40m, 100m);
    }

    private static GuardrailResult BuildGuardrail(
        ScenarioInput input,
        decimal workingCapital,
        decimal baselineWorkingCapital,
        decimal bufferHealth,
        decimal baselineBufferHealth,
        decimal utilization,
        decimal service,
        decimal baselineService,
        decimal flowIndex,
        decimal baselineFlowIndex)
    {
        var workingCapitalDelta = baselineWorkingCapital <= 0
            ? 0
            : decimal.Round((workingCapital - baselineWorkingCapital) * 100m / baselineWorkingCapital, 1);
        var serviceLoss = decimal.Round(Math.Max(0, baselineService - service), 1);
        var flowLoss = decimal.Round(Math.Max(0, baselineFlowIndex - flowIndex), 1);
        var bufferHealthLoss = decimal.Round(Math.Max(0, baselineBufferHealth - bufferHealth), 1);

        var checks = new List<GuardrailCheck>
        {
            HigherIsWorse("服务水平损失", serviceLoss, "百分点", 1m, 3m, "相对 AS&OP 基线的服务损失超过容差时，需要重新确认客户承诺。"),
            HigherIsWorse("Flow Index 损失", flowLoss, "分", 2m, 6m, "流动性相对基线恶化，需评估库存、产能和交付承诺的取舍。"),
            HigherIsWorse("缓冲健康度损失", bufferHealthLoss, "百分点", 8m, 20m, "DDOM 防线相对基线明显变弱时，需要重审主设置。"),
            HigherIsWorse("瓶颈资源负载", utilization, "%", 85m, 100m, "超过 100% 时必须升级产能、外协、组合或交付承诺。"),
            HigherIsWorse("营运资金增幅", workingCapitalDelta, "%", 5m, 12m, "超过预算栅栏时需 CFO 与集成协调确认现金占用。"),
            HigherIsWorse("需求调整幅度", input.PromotionPercent, "%", 8m, 20m, "需求上调超过战术容差时需回到需求审查与集成协调。"),
            HigherIsWorse("供应中断窗口", input.SupplyDisruptionWeeks, "周", 1m, 3m, "进入采购/供应风险红线后需管理层确认客户分配规则。"),
            HigherIsWorse("计划停线窗口", input.PlannedShutdownDays, "天", 2m, 5m, "停线超过保护能力时需升级产能与服务承诺决策。"),
        };

        var status = checks.Any(check => check.Status == "Red")
            ? "Blocked"
            : checks.Any(check => check.Status == "Yellow")
                ? "Reconcile"
                : "WithinFence";

        var label = status switch
        {
            "Blocked" => "阻断采纳",
            "Reconcile" => "进入集成协调",
            _ => "DDS&OP 可处理"
        };

        var decision = status switch
        {
            "Blocked" => "超出 AS&OP 授权栅栏，DDS&OP 只能模拟和提交建议，不得直接应用主设置。",
            "Reconcile" => "存在黄色超限项，需要进入 Integrated Reconciliation 后再决定是否采纳。",
            _ => "情景仍在战术授权范围内，可由 DDS&OP 调整 DDOM 主设置。"
        };

        return new GuardrailResult(status, label, status == "Blocked", decision, checks);
    }

    private static GuardrailCheck HigherIsWorse(string metric, decimal value, string unit, decimal yellowLimit, decimal redLimit, string message)
    {
        var status = value >= redLimit ? "Red" : value >= yellowLimit ? "Yellow" : "Green";
        return new GuardrailCheck(metric, value, unit, yellowLimit, redLimit, status, message);
    }

    private static IReadOnlyList<string> BuildActions(
        ScenarioInput input,
        IReadOnlyList<SkuScenarioResult> results,
        decimal bufferHealth,
        decimal utilization,
        decimal workingCapital,
        GuardrailResult guardrail)
    {
        var actions = new List<string>();

        if (guardrail.IsAdoptionBlocked)
        {
            actions.Add($"AS&OP 阻断采纳：{guardrail.Decision}");
        }
        else if (guardrail.Status == "Reconcile")
        {
            actions.Add($"集成协调：{guardrail.Decision}");
        }
        else
        {
            actions.Add($"授权通过：{guardrail.Decision}");
        }

        var redCount = results.Count(x => x.BufferStatus == "Red");
        if (redCount > 0)
        {
            actions.Add($"DDS&OP 催交：{redCount} 个缓冲进入红区，优先催交开放供应并检查实际需求尖峰。");
        }

        if (input.PromotionPercent > 0)
        {
            actions.Add($"情景评估：促销需求上调 {input.PromotionPercent:0.#}%，同步影响 ADU、缓冲区、现金占用和 AS&OP 财务指标。");
        }

        if (input.SupplyDisruptionWeeks > 0)
        {
            actions.Add($"供应风险：供应中断 {input.SupplyDisruptionWeeks} 周，DDS&OP 应评估替代供应、空运和客户分配规则。");
        }

        if (utilization > 100)
        {
            actions.Add($"管理评审：能力利用率 {utilization:0.#}% 超过 100%，需升级到 Adaptive S&OP 决策产能、组合或服务承诺。");
        }

        if (bufferHealth < 80)
        {
            actions.Add($"模型重构：缓冲健康度 {bufferHealth:0.#}% 低于 80%，建议重审解耦点、DLT 和 variability factor。");
        }

        if (workingCapital > 50000)
        {
            actions.Add($"财务联动：建议订单形成 {workingCapital:C0} 现金占用，需进入财务计划和 Integrated Reconciliation。");
        }

        return actions;
    }
}
