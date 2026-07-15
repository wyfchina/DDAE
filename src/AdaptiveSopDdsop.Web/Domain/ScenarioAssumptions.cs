namespace AdaptiveSopDdsop.Web.Domain;

public sealed record ScenarioAssumptionMetadata(
    string SourceKind,
    string? TemplateId,
    string? TemplateVersion,
    string RecordedBy,
    string RecordedAtUtc,
    string EffectiveFrom,
    string EffectiveThrough,
    string Rationale,
    string EvidenceLabel);

public sealed record ScenarioAssumptionTemplate(
    string TemplateId,
    string TemplateVersion,
    string Name,
    ExternalScenarioDefinition ExternalScenario,
    string EvidenceLabel);

public interface IScenarioAssumptionSource
{
    IReadOnlyList<ScenarioAssumptionTemplate> GetTemplates();

    ScenarioAssumptionTemplate? GetTemplate(string templateId);

    void Validate(ScenarioAssumptionMetadata metadata);
}
