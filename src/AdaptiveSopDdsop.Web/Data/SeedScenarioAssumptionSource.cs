using AdaptiveSopDdsop.Web.Domain;
using System.Globalization;

namespace AdaptiveSopDdsop.Web.Data;

public sealed class SeedScenarioAssumptionSource : IScenarioAssumptionSource
{
    private const string DemoTemplateId = "DDAE-DEMO-SUPPLY-CAPACITY-001";
    private const string DemoTemplateVersion = "1.1.0";
    private const string DemoEvidenceLabel = "DDAE 内置演示假设；非外部事实输入";

    private readonly IReadOnlyList<ScenarioAssumptionTemplate> _templates;

    public SeedScenarioAssumptionSource()
    {
        var metadata = new ScenarioAssumptionMetadata(
            "DemoFixture",
            DemoTemplateId,
            DemoTemplateVersion,
            "DDAE 内置演示",
            "2026-07-15T00:00:00Z",
            "2026-07-15",
            "2026-09-30",
            "用于演示未来需求、供应和能力扰动的进程内假设",
            DemoEvidenceLabel);
        var scenario = new ExternalScenarioDefinition(
            "EXT-DEMO-SUPPLY-CAPACITY",
            "内置演示：需求上升与供应能力风险",
            DemandChanges: new[]
            {
                new ExternalDemandChange(null, "星载电子", 2, 6, 1.35m, "内置演示星载电子需求上升")
            },
            SupplyRisks: new[]
            {
                new ExternalSupplyRisk("Microchip Space", "进口空间级 FPGA", 3, 8, 0.05m, "内置演示进口 FPGA 供应严重受限")
            },
            CapacityLosses: new[]
            {
                new ExternalCapacityLoss("RES-TVAC", 3, 6, 0.50m, "内置演示热真空能力损失")
            },
            KnownEvents: new[]
            {
                new ExternalKnownEvent("EVENT-DEMO-001", "内置演示客户窗口", 2, 6)
            },
            Metadata: metadata);

        _templates = new[]
        {
            new ScenarioAssumptionTemplate(
                DemoTemplateId,
                DemoTemplateVersion,
                "需求上升与供应能力风险",
                scenario,
                DemoEvidenceLabel)
        };
    }

    public IReadOnlyList<ScenarioAssumptionTemplate> GetTemplates() => _templates;

    public ScenarioAssumptionTemplate? GetTemplate(string templateId)
    {
        if (string.IsNullOrWhiteSpace(templateId))
        {
            return null;
        }

        return _templates.SingleOrDefault(item => string.Equals(item.TemplateId, templateId.Trim(), StringComparison.Ordinal));
    }

    public void Validate(ScenarioAssumptionMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        var sourceKind = metadata.SourceKind?.Trim();
        if (string.Equals(sourceKind, "Manual", StringComparison.OrdinalIgnoreCase))
        {
            ValidateManual(metadata);
            return;
        }

        if (string.Equals(sourceKind, "DemoFixture", StringComparison.OrdinalIgnoreCase))
        {
            ValidateDemoFixture(metadata);
            return;
        }

        throw new ArgumentException("场景来源仅允许 Manual 或 DemoFixture。", nameof(metadata));
    }

    private static void ValidateManual(ScenarioAssumptionMetadata metadata)
    {
        Require(metadata.RecordedBy, "人工场景必须提供记录人。", metadata);
        Require(metadata.RecordedAtUtc, "人工场景必须提供记录时间。", metadata);
        Require(metadata.EffectiveFrom, "人工场景必须提供有效期开始日期。", metadata);
        Require(metadata.EffectiveThrough, "人工场景必须提供有效期结束日期。", metadata);
        Require(metadata.Rationale, "人工场景必须提供理由。", metadata);
        Require(metadata.EvidenceLabel, "人工场景必须提供证据标签。", metadata);

        if (!DateTimeOffset.TryParse(
                metadata.RecordedAtUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var recordedAt)
            || recordedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("人工场景记录时间必须是可解析的 UTC 时间。", nameof(metadata));
        }
        if (!DateOnly.TryParseExact(
                metadata.EffectiveFrom,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var effectiveFrom))
        {
            throw new ArgumentException("人工场景有效期开始日期必须使用 yyyy-MM-dd。", nameof(metadata));
        }
        if (!DateOnly.TryParseExact(
                metadata.EffectiveThrough,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var effectiveThrough))
        {
            throw new ArgumentException("人工场景有效期结束日期必须使用 yyyy-MM-dd。", nameof(metadata));
        }
        if (effectiveFrom > effectiveThrough)
        {
            throw new ArgumentException("人工场景有效期开始日期不能晚于结束日期。", nameof(metadata));
        }
    }

    private void ValidateDemoFixture(ScenarioAssumptionMetadata metadata)
    {
        var template = GetTemplate(metadata.TemplateId ?? string.Empty)
            ?? throw new ArgumentException("演示模板不存在。", nameof(metadata));
        if (!string.Equals(template.TemplateVersion, metadata.TemplateVersion?.Trim(), StringComparison.Ordinal))
        {
            throw new ArgumentException("演示模板版本不匹配。", nameof(metadata));
        }
        if (!string.Equals(template.EvidenceLabel, metadata.EvidenceLabel?.Trim(), StringComparison.Ordinal))
        {
            throw new ArgumentException("演示模板证据标签不匹配。", nameof(metadata));
        }
    }

    private static void Require(string? value, string message, ScenarioAssumptionMetadata metadata)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message, nameof(metadata));
        }
    }
}
