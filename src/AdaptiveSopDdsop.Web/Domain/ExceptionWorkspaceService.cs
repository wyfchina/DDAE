namespace AdaptiveSopDdsop.Web.Domain;

public sealed class ExceptionWorkspaceService
{
    private const decimal DemandSpikeThresholdPercent = 12m;
    private const decimal ServiceLossThresholdPercent = 95m;

    private readonly IScenarioWorkspaceDataSource _dataSource;

    public ExceptionWorkspaceService(IScenarioWorkspaceDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public ExceptionWorkspaceResult GetExceptions(int horizonWeeks)
    {
        var horizon = Math.Clamp(horizonWeeks <= 0 ? 12 : horizonWeeks, 1, 52);
        var data = _dataSource.Load(new ScenarioWorkspaceDataRequest(horizon, new DateOnly(2026, 6, 1)));
        var exceptions = Build(data)
            .OrderBy(item => item.Severity == "Red" ? 0 : 1)
            .ThenByDescending(item => item.ExceptionCount)
            .ThenByDescending(item => item.LatestExceptionWeekOffset)
            .ThenByDescending(item => item.MaxDemandVariancePercent)
            .ToList();

        return new ExceptionWorkspaceResult(
            horizon,
            exceptions,
            exceptions.Count(item => item.Severity == "Red"),
            exceptions.Count(item => item.Severity == "Yellow"),
            exceptions.Sum(item => item.Signals.Count(signal => signal.Reason == "DemandSpike")),
            exceptions.Sum(item => item.Signals.Count(signal => signal.Reason == "ServiceLoss")),
            exceptions.Sum(item => item.Signals.Count(signal => signal.Reason == "BufferRisk")),
            AppliedSku: null);
    }

    private static IReadOnlyList<ExceptionSkuSummary> Build(ScenarioWorkspaceDataSet data)
    {
        var skuMap = data.Skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);
        var supplyRiskFamilies = data.SupplierCapacityWindows
            .Where(item => item.RiskStatus == "Red")
            .Select(item => item.MaterialFamily)
            .ToHashSet(StringComparer.Ordinal);
        var skuSupplyRisk = data.SupplierItemSources
            .Where(item => supplyRiskFamilies.Contains(item.MaterialFamily))
            .Select(item => item.Sku)
            .ToHashSet(StringComparer.Ordinal);

        return data.HistoricalDemand
            .GroupBy(item => item.Sku)
            .Select(group =>
            {
                var sku = skuMap[group.Key];
                var zones = DdmrpCalculator.CalculateZones(sku);
                var signals = group
                    .SelectMany(point => BuildSignals(point, zones))
                    .OrderByDescending(item => item.WeekOffset)
                    .ThenBy(item => item.Reason)
                    .ToList();

                if (signals.Count == 0)
                {
                    return null;
                }

                var hasRed = signals.Any(item => item.Severity == "Red");
                var primary = PrimaryReason(signals);
                var presets = BuildPresets(sku, signals, skuSupplyRisk.Contains(sku.Sku));
                var recommended = presets.First();

                return new ExceptionSkuSummary(
                    sku.Sku,
                    sku.Name,
                    sku.Family,
                    signals.Max(item => item.WeekOffset),
                    signals.Max(item => item.DemandVariancePercent),
                    signals.Min(item => item.ServiceLevelPercent),
                    signals.Min(item => item.EndingNetFlow),
                    signals.Count,
                    hasRed ? "Red" : "Yellow",
                    primary,
                    recommended.TemplateId,
                    recommended.ActionHint,
                    signals,
                    presets);
            })
            .Where(item => item is not null)
            .Cast<ExceptionSkuSummary>()
            .ToList();
    }

    private static IReadOnlyList<ExceptionSignal> BuildSignals(HistoricalDemandActual point, BufferZones zones)
    {
        var signals = new List<ExceptionSignal>();
        var variancePercent = point.ForecastDemand <= 0
            ? 0m
            : decimal.Round((point.ActualDemand - point.ForecastDemand) * 100m / point.ForecastDemand, 1);
        var demandSpike = variancePercent > DemandSpikeThresholdPercent;
        var serviceLoss = point.ServiceLevelPercent < ServiceLossThresholdPercent;
        var bufferRisk = point.EndingNetFlow <= zones.TopOfYellow;
        var redBuffer = point.EndingNetFlow <= zones.TopOfRed;

        if (demandSpike)
        {
            signals.Add(new ExceptionSignal(
                point.Sku,
                point.WeekOffset,
                point.ActualDemand,
                point.ForecastDemand,
                variancePercent,
                point.ServiceLevelPercent,
                point.EndingNetFlow,
                "DemandSpike",
                serviceLoss ? "Red" : "Yellow"));
        }

        if (serviceLoss)
        {
            signals.Add(new ExceptionSignal(
                point.Sku,
                point.WeekOffset,
                point.ActualDemand,
                point.ForecastDemand,
                variancePercent,
                point.ServiceLevelPercent,
                point.EndingNetFlow,
                "ServiceLoss",
                demandSpike ? "Red" : "Yellow"));
        }

        if (bufferRisk)
        {
            signals.Add(new ExceptionSignal(
                point.Sku,
                point.WeekOffset,
                point.ActualDemand,
                point.ForecastDemand,
                variancePercent,
                point.ServiceLevelPercent,
                point.EndingNetFlow,
                "BufferRisk",
                redBuffer ? "Red" : "Yellow"));
        }

        return signals;
    }

    private static IReadOnlyList<ExceptionScenarioPreset> BuildPresets(
        SkuBufferSetting sku,
        IReadOnlyList<ExceptionSignal> signals,
        bool hasSupplyRisk)
    {
        var presets = new List<ExceptionScenarioPreset>();
        if (signals.Any(item => item.Reason == "DemandSpike"))
        {
            presets.Add(new("TPL-PREBUILD-PEAK", "需求尖峰提前建库", "用 Pre-build 测试把尖峰补货压力前移。"));
        }

        if (signals.Any(item => item.Reason is "ServiceLoss" or "BufferRisk"))
        {
            presets.Add(new("TPL-ORDER-POLICY", "补货策略调整", "测试 MOQ 与订货周期对服务和库存水位的影响。"));
        }

        if (sku.Family == "星载电子" && hasSupplyRisk)
        {
            presets.Add(new("TPL-CONSTRAINED", "供应受限预演", "同步检查关键进口器件供应约束对交付的影响。"));
        }

        if (presets.Count == 0)
        {
            presets.Add(new("TPL-ORDER-POLICY", "补货策略调整", "从补货参数开始检查异常恢复路径。"));
        }

        return presets
            .GroupBy(item => item.TemplateId)
            .Select(group => group.First())
            .ToList();
    }

    private static string PrimaryReason(IReadOnlyList<ExceptionSignal> signals)
    {
        if (signals.Any(item => item.Severity == "Red" && item.Reason == "BufferRisk"))
        {
            return "BufferRisk";
        }

        if (signals.Any(item => item.Reason == "DemandSpike"))
        {
            return "DemandSpike";
        }

        if (signals.Any(item => item.Reason == "ServiceLoss"))
        {
            return "ServiceLoss";
        }

        return "BufferRisk";
    }
}
