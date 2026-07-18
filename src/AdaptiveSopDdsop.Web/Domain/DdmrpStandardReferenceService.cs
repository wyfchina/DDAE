namespace AdaptiveSopDdsop.Web.Domain;

public sealed record DdmrpStandardReferenceInputs(
    decimal Adu,
    int DecoupledLeadTimeDays,
    decimal LeadTimeFactor,
    decimal VariabilityFactor,
    decimal MinimumOrderQuantity,
    int OrderCycleDays,
    decimal DemandAdjustmentFactor,
    decimal ZoneAdjustmentFactor);

public sealed record DdmrpStandardReferenceView(
    string ReferenceId,
    string Name,
    DdmrpStandardReferenceInputs Inputs,
    decimal RedBase,
    decimal RedSafety,
    BufferZones Zones,
    decimal TotalBuffer,
    string GreenDriver,
    IReadOnlyList<BufferSizingLine> Derivations,
    string SourceAuthority,
    string EvidenceStatus);

public sealed class DdmrpStandardReferenceService
{
    private const string SourceAuthority = "DDAE 后端标准定容算例";

    public DdmrpStandardReferenceView GetReference()
    {
        var setting = new SkuBufferSetting(
            "DDMRP-EXAMPLE",
            "标准定容算例",
            "标准算例",
            10m,
            12,
            0.33m,
            7,
            50m,
            1m,
            100m,
            DecouplingPoint: "标准定容参考",
            BufferProfile: "标准定容算例",
            AduSource: SourceAuthority,
            DltSource: SourceAuthority,
            DemandAdjustmentFactor: 1m,
            ZoneAdjustmentFactor: 1m,
            LeadTimeFactor: 0.5m,
            ParameterSnapshotId: "DDMRP-EXAMPLE-V1",
            ParameterEvidenceStatus: "Complete");
        var sizing = DdmrpCalculator.CalculateSizing(setting);

        return new DdmrpStandardReferenceView(
            setting.ParameterSnapshotId,
            setting.Name,
            new DdmrpStandardReferenceInputs(
                setting.Adu,
                setting.DecoupledLeadTimeDays,
                setting.LeadTimeFactor!.Value,
                setting.VariabilityFactor,
                setting.MinimumOrderQuantity,
                setting.OrderCycleDays,
                setting.DemandAdjustmentFactor,
                setting.ZoneAdjustmentFactor),
            sizing.RedBase,
            sizing.RedSafety,
            sizing.Zones,
            sizing.Zones.TopOfGreen,
            sizing.GreenDriver,
            DdmrpSizingExplanation.Build(sizing),
            SourceAuthority,
            sizing.EvidenceStatus);
    }
}
