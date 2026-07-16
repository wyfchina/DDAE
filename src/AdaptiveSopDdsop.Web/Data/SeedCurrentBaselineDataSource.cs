using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.Data;

public sealed class SeedCurrentBaselineDataSource : ICurrentBaselineDataSource
{
    private readonly ValidationData _data;
    private readonly IScenarioWorkspaceDataSource _scenarioDataSource;

    public SeedCurrentBaselineDataSource()
        : this(SeedData.Create())
    {
    }

    public SeedCurrentBaselineDataSource(ValidationData data)
        : this(data, new SeedScenarioWorkspaceDataSource(data))
    {
    }

    public SeedCurrentBaselineDataSource(ValidationData data, IScenarioWorkspaceDataSource scenarioDataSource)
    {
        _data = data;
        _scenarioDataSource = scenarioDataSource;
    }

    public CurrentBaselineCandidate GetCandidate()
    {
        var asOf = new DateTimeOffset(2026, 6, 30, 8, 0, 0, TimeSpan.Zero).ToString("O");
        var inTransit = _data.Inventory.Select(item => new BaselineTransitItem(item.Sku, item.OpenSupply, item.OpenSupply > 0 ? "Confirmed" : "None")).ToList();
        var backlog = _data.Demand.Where(item => item.Week == 1).Select(item => new BaselineBacklogItem(item.Sku, item.Week, item.BaselineDemand, "ConfirmedDemand")).ToList();
        var wip = _data.ResourceRoutings.Select(route => new BaselineWipItem(
            route.ResourceCode,
            route.Sku,
            decimal.Round(_data.Demand.Where(item => item.Sku == route.Sku && item.Week == 1).Sum(item => item.BaselineDemand) * 0.35m, 0),
            "DemoObserved")).ToList();
        var supplier = _data.SupplierConstraints.Select(item => new BaselineSupplierCommitment(
            item.Supplier, item.MaterialFamily, item.MonthlyCapacity, item.LeadTimeDays, item.RiskStatus)).ToList();
        var resources = _data.Resources.Select(item => new BaselineResourceAvailability(item.Code, item.Name, item.WeeklyAvailableUnits, "StandardCalendar")).ToList();
        var adjustments = _data.KnownEvents.Where(item => item.Status != "Closed").Select(item => new BaselineTemporaryAdjustment(
            item.EventId, item.Name, item.Window, item.AppliesTo, item.Status)).ToList();
        var planningInputs = _scenarioDataSource.Load(new ScenarioWorkspaceDataRequest(
            52,
            new DateOnly(2026, 6, 1)));
        var ddmrpSizingItems = planningInputs.Skus.Select(item =>
        {
            var complete = item.LeadTimeFactor is > 0m and <= 1m &&
                !string.IsNullOrWhiteSpace(item.ParameterSnapshotId) &&
                item.ParameterEvidenceStatus == "Complete";
            return new BaselineEvidenceItem(
                item.Sku,
                $"{item.Sku} {item.Name}",
                "Fresh",
                complete ? "Complete" : "EvidenceMissing",
                true,
                complete ? null : "缺少提前期因子、参数快照号或完整证据");
        }).ToList();
        var kpis = BuildKpis(asOf, planningInputs, backlog, wip);
        var timeBufferEvidence = BuildTimeBufferEvidence(asOf, planningInputs);
        var capacityProtectionCount = planningInputs.CapacityProtections?.Count ?? 0;
        var analysisAvailability = new List<BaselineAnalysisAvailability>
        {
            new("InventoryBuffer", _data.Inventory.Count > 0 ? "Complete" : "EvidenceMissing", "Inventory control-point evidence is frozen in PlanningInputs."),
            timeBufferEvidence.Availability,
            new("CapacityBuffer", capacityProtectionCount > 0 ? "Complete" : "EvidenceMissing", capacityProtectionCount > 0
                ? "Sequenced upstream capacity-protection evidence is available."
                : "Sequenced upstream capacity-protection evidence is missing."),
            new("SupplyRisk", supplier.Count > 0 ? "Complete" : "EvidenceMissing", "Supplier commitments are frozen independently from protection buffers.")
        };
        var payload = new CurrentBaselinePayload(
            _data.Inventory,
            inTransit,
            backlog,
            wip,
            supplier,
            resources,
            adjustments,
            _data.MasterSettings,
            planningInputs,
            kpis,
            analysisAvailability);
        var sections = new List<BaselineEvidenceSection>
        {
            Section("CURRENT_KPIS", "Meeting snapshot KPIs", kpis.SourceAuthority, asOf, 7,
                completenessStatus: kpis.EvidenceStatus),
            Section("INVENTORY", "当前库存与净流量位置", "DDAE Demo Inventory Adapter", asOf, _data.Inventory.Count),
            Section("IN_TRANSIT", "在途与开放供应", "DDAE Demo Supply Adapter", asOf, inTransit.Count),
            Section("BACKLOG", "未结与积压需求", "DDAE Demo Demand Adapter", asOf, backlog.Count),
            Section("WIP", "在制品与控制点队列", "DDAE Demo WIP Evidence", asOf, wip.Count),
            Section("SUPPLIER_COMMITMENTS", "供应商最新承诺", "DDAE Demo Supplier Evidence", asOf, supplier.Count),
            Section("RESOURCE_AVAILABILITY", "资源可用能力", "DDAE Demo Capacity Evidence", asOf, resources.Count),
            Section("TEMPORARY_ADJUSTMENTS", "已生效临时措施", "DDAE Demo Governance", asOf, adjustments.Count, required: false),
            Section("MASTER_SETTINGS", "当前 DDOM 参数版本", "DDAE Governance", asOf, _data.MasterSettings.Count),
            Section("PLANNING_INPUTS", "白盒重算类型化输入", "DDAE Demo Planning Snapshot", asOf,
                planningInputs.Skus.Count + planningInputs.Demand.Count + planningInputs.ResourceRoutings.Count + planningInputs.SupplierItemSources.Count),
            Section("ROUTING_SEQUENCE", "Sequenced resource-routing evidence", "DDAE Demo Planning Snapshot", asOf,
                planningInputs.ResourceRoutings.Count, missingReason: "Resource-routing sequence evidence is missing."),
            Section("CAPACITY_PROTECTION", "Upstream capacity-protection evidence", "DDAE Demo Planning Snapshot", asOf,
                capacityProtectionCount, missingReason: "Sequenced upstream capacity-protection evidence is missing.")
        };
        sections.Add(DdmrpSizingSection(asOf, ddmrpSizingItems));
        sections.AddRange(timeBufferEvidence.Sections);
        return new CurrentBaselineCandidate("BASE-CANDIDATE-DEMO-20260630", asOf, "DEMO-MS-2026-06", sections, payload, "DemoFixture");
    }

    private static BaselineEvidenceSection DdmrpSizingSection(
        string asOf,
        IReadOnlyList<BaselineEvidenceItem> items)
    {
        var complete = items.Count > 0 && items.All(item =>
            item.FreshnessStatus == "Fresh" && item.CompletenessStatus == "Complete");
        var missingReason = string.Join("；", items
            .Where(item => item.FreshnessStatus != "Fresh" || item.CompletenessStatus != "Complete")
            .Select(item => $"{item.ItemKey}：{item.MissingReason}"));
        return new BaselineEvidenceSection(
            "DDMRP_SIZING",
            "DDMRP 定容证据",
            "DDAE Demo Planning Snapshot",
            asOf,
            "Fresh",
            complete ? "Complete" : "EvidenceMissing",
            items.Count,
            "DemoFixture",
            true,
            string.IsNullOrWhiteSpace(missingReason) ? null : missingReason,
            items);
    }

    private static BaselineKpiSnapshot BuildKpis(
        string asOf,
        ScenarioWorkspaceDataSet planningInputs,
        IReadOnlyList<BaselineBacklogItem> backlog,
        IReadOnlyList<BaselineWipItem> workInProcess)
    {
        var historicalService = planningInputs.HistoricalDemand.Count == 0
            ? (decimal?)null
            : decimal.Round(planningInputs.HistoricalDemand.Average(item => item.ServiceLevelPercent), 1);
        var serviceWindow = planningInputs.HistoricalDemand.Count == 0
            ? string.Empty
            : $"{planningInputs.HistoricalDemand.Select(item => item.WeekOffset).Distinct().Count()}-week rolling window";
        var inventoryValue = InventoryValue(planningInputs);
        var workInProcessUnits = workInProcess.Count == 0
            ? (decimal?)null
            : workInProcess.Sum(item => item.Quantity);
        var backlogUnits = backlog.Count == 0
            ? (decimal?)null
            : backlog.Sum(item => item.Quantity);
        var weekOneDemandFacts = planningInputs.Demand
            .Where(item =>
                item.Week == 1 &&
                !string.IsNullOrWhiteSpace(item.Sku) &&
                item.BaselineDemand >= 0m)
            .ToList();
        var weeklyDemand = weekOneDemandFacts.Sum(item => item.BaselineDemand);
        var supplyUnits = planningInputs.Inventory.Sum(item => item.OnHand + item.OpenSupply);
        var hasInventoryEvidence = planningInputs.Inventory.Count > 0 &&
            planningInputs.Inventory.All(item => !string.IsNullOrWhiteSpace(item.Sku));
        var coverage = !hasInventoryEvidence || weekOneDemandFacts.Count == 0 || weeklyDemand <= 0m
            ? (decimal?)null
            : decimal.Round(supplyUnits / weeklyDemand, 1);

        var availableResources = planningInputs.Resources
            .Where(resource => resource.WeeklyAvailableUnits > 0m)
            .ToList();
        var availableResourceCodes = availableResources
            .Select(resource => resource.Code)
            .ToHashSet(StringComparer.Ordinal);
        var skuCodes = planningInputs.Skus.Select(item => item.Sku).ToHashSet(StringComparer.Ordinal);
        var demandBySku = weekOneDemandFacts
            .GroupBy(item => item.Sku, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.BaselineDemand), StringComparer.Ordinal);
        var routings = planningInputs.ResourceRoutings;
        var hasConsistentLoadEvidence = availableResources.Count > 0 &&
            availableResourceCodes.Count == availableResources.Count &&
            routings.Count > 0 &&
            routings.All(route =>
                availableResourceCodes.Contains(route.ResourceCode) &&
                skuCodes.Contains(route.Sku) &&
                route.CapacityPerUnit > 0m &&
                route.OperationSequence > 0 &&
                IsCompleteEvidence(route.EvidenceStatus) &&
                demandBySku.ContainsKey(route.Sku)) &&
            availableResources.All(resource => routings.Any(route => route.ResourceCode == resource.Code));
        var peakLoad = hasConsistentLoadEvidence
            ? availableResources.Max(resource => decimal.Round(
                routings
                    .Where(route => route.ResourceCode == resource.Code)
                    .Sum(route => demandBySku[route.Sku] * route.CapacityPerUnit) * 100m /
                resource.WeeklyAvailableUnits,
                1))
            : (decimal?)null;
        var evidenceStatus = historicalService.HasValue &&
            !string.IsNullOrWhiteSpace(serviceWindow) &&
            inventoryValue.HasValue &&
            workInProcessUnits.HasValue &&
            backlogUnits.HasValue &&
            coverage.HasValue &&
            peakLoad.HasValue
            ? "Complete"
            : "EvidenceMissing";

        return new BaselineKpiSnapshot(
            historicalService,
            serviceWindow,
            inventoryValue,
            workInProcessUnits,
            backlogUnits,
            coverage,
            peakLoad,
            "DDAE Demo Meeting Snapshot",
            asOf,
            evidenceStatus);

        decimal? InventoryValue(ScenarioWorkspaceDataSet data)
        {
            if (data.Inventory.Count == 0 || data.Skus.Count == 0)
            {
                return null;
            }

            var costs = data.Skus
                .GroupBy(item => item.Sku, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count() == 1 && group.Single().UnitCost > 0m
                        ? (decimal?)group.Single().UnitCost
                        : null,
                    StringComparer.Ordinal);
            if (data.Inventory.Any(item => !costs.TryGetValue(item.Sku, out var unitCost) || !unitCost.HasValue))
            {
                return null;
            }

            return data.Inventory.Sum(item => item.OnHand * costs[item.Sku]!.Value);
        }
    }

    private static TimeBufferEvidence BuildTimeBufferEvidence(string asOf, ScenarioWorkspaceDataSet planningInputs)
    {
        var definitions = planningInputs.TimeBuffers ?? Array.Empty<TimeBufferDefinition>();
        var scopes = planningInputs.TimeBufferProductScopes ?? Array.Empty<TimeBufferProductScope>();
        var progress = planningInputs.ControlPointProgress ?? Array.Empty<ControlPointProgressFact>();
        if (definitions.Count == 0 && scopes.Count == 0 && progress.Count == 0)
        {
            var notApplicable = new[]
            {
                NotApplicableTimeSection("TIME_BUFFER_DEFINITIONS", "Time-buffer definitions", asOf),
                NotApplicableTimeSection("TIME_BUFFER_PRODUCT_SCOPES", "Time-buffer product scopes", asOf),
                NotApplicableTimeSection("CONTROL_POINT_PROGRESS", "Time-buffer control-point progress", asOf)
            };
            return new TimeBufferEvidence(
                notApplicable,
                new BaselineAnalysisAvailability("TimeBuffer", "NotApplicable", "No time-buffer definitions are configured."));
        }

        var horizonWeeks = Math.Clamp(planningInputs.Request.HorizonWeeks, 1, 52);
        var definitionGroups = definitions
            .GroupBy(item => item.BufferId, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToList();
        var knownDefinitionIds = definitionGroups
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);
        var knownSkus = planningInputs.Skus.Select(item => item.Sku).ToHashSet(StringComparer.Ordinal);
        var knownFamilies = planningInputs.Skus.Select(item => item.Family).ToHashSet(StringComparer.Ordinal);

        var definitionItems = definitionGroups.Select(group =>
        {
            var definition = group.First();
            var duplicate = group.Count() != 1;
            var completeness = !duplicate && definition.BufferDays > 0m && definition.EvidenceStatus == "Complete"
                ? "Complete"
                : "EvidenceMissing";
            var evidenceStatus = group
                .Select(item => item.EvidenceStatus)
                .FirstOrDefault(item => Freshness(item) != "Fresh") ?? definition.EvidenceStatus;
            var explicitReason = duplicate
                ? $"Duplicate time-buffer definitions ({group.Count()} rows)"
                : definition.BufferDays > 0m
                    ? null
                    : "BufferDays is missing or invalid";
            return new BaselineEvidenceItem(
                definition.BufferId,
                definition.ControlPoint,
                Freshness(evidenceStatus),
                completeness,
                duplicate || definition.IsCritical,
                EvidenceReason(evidenceStatus, completeness, explicitReason));
        }).ToList();

        var scopeItems = definitionGroups.Select(group =>
        {
            var definition = group.First();
            var matchingScopes = scopes.Where(item => item.BufferId == definition.BufferId).ToList();
            var scope = matchingScopes.Count == 1 ? matchingScopes[0] : null;
            string? scopeIssue = matchingScopes.Count switch
            {
                0 => "Product scope evidence is missing",
                > 1 => $"Duplicate product scope evidence ({matchingScopes.Count} rows)",
                _ => null
            };
            if (scopeIssue is null && scope!.EvidenceStatus != "Complete")
            {
                scopeIssue = $"EvidenceStatus={scope.EvidenceStatus ?? "Missing"}";
            }
            if (scopeIssue is null)
            {
                var unknownSkus = scope!.Skus.Where(item => !knownSkus.Contains(item)).Distinct(StringComparer.Ordinal).ToList();
                var unknownFamilies = scope.ProductFamilies.Where(item => !knownFamilies.Contains(item)).Distinct(StringComparer.Ordinal).ToList();
                if (unknownSkus.Count > 0)
                {
                    scopeIssue = $"Unknown SKU scope evidence: {string.Join(", ", unknownSkus)}";
                }
                else if (unknownFamilies.Count > 0)
                {
                    scopeIssue = $"Unknown product-family scope evidence: {string.Join(", ", unknownFamilies)}";
                }
                else
                {
                    var scopedFamilies = scope.ProductFamilies.ToHashSet(StringComparer.Ordinal);
                    var affectedProducts = scope.Skus
                        .Concat(planningInputs.Skus
                            .Where(item => scopedFamilies.Contains(item.Family))
                            .Select(item => item.Sku))
                        .Where(ProtectionProductEligibility.IsEligible)
                        .Distinct(StringComparer.Ordinal)
                        .ToList();
                    if (affectedProducts.Count == 0)
                    {
                        scopeIssue = "Product scope does not resolve to an eligible product";
                    }
                }
            }

            var evidenceStatus = matchingScopes
                .Select(item => item.EvidenceStatus)
                .FirstOrDefault(item => Freshness(item) != "Fresh") ?? scope?.EvidenceStatus;
            var completeness = scopeIssue is null ? "Complete" : "EvidenceMissing";
            return new BaselineEvidenceItem(
                definition.BufferId,
                $"{definition.ControlPoint} product scope",
                Freshness(evidenceStatus),
                completeness,
                definition.IsCritical,
                EvidenceReason(evidenceStatus, completeness, scopeIssue));
        }).ToList();
        scopeItems.AddRange(scopes
            .Where(item => !knownDefinitionIds.Contains(item.BufferId))
            .GroupBy(item => item.BufferId, StringComparer.Ordinal)
            .Select(group => new BaselineEvidenceItem(
                $"UNKNOWN:{group.Key}",
                $"{group.Key} product scope",
                Freshness(group.Select(item => item.EvidenceStatus).FirstOrDefault(item => Freshness(item) != "Fresh")),
                "EvidenceMissing",
                true,
                "Product scope references an undefined time-buffer definition")));

        var progressItems = definitionGroups.Select(group =>
        {
            var definition = group.First();
            var facts = progress.Where(item => item.BufferId == definition.BufferId).ToList();
            var horizonFacts = facts.Where(item => item.Week >= 1 && item.Week <= horizonWeeks).ToList();
            var factsByWeek = horizonFacts
                .GroupBy(item => item.Week)
                .ToDictionary(group => group.Key, group => group.ToList());
            var factIssues = new List<string>();
            foreach (var week in Enumerable.Range(1, horizonWeeks))
            {
                if (!factsByWeek.TryGetValue(week, out var weeklyFacts) || weeklyFacts.Count == 0)
                {
                    factIssues.Add($"Week {week}: Control-point progress evidence is missing");
                    continue;
                }
                if (weeklyFacts.Count != 1)
                {
                    factIssues.Add($"Week {week}: duplicate control-point progress evidence ({weeklyFacts.Count} rows)");
                    continue;
                }

                var fact = weeklyFacts[0];
                if (!fact.ObservedDelayDays.HasValue)
                {
                    factIssues.Add($"Week {fact.Week}: ObservedDelayDays is missing");
                }
                if (fact.EvidenceStatus != "Complete")
                {
                    factIssues.Add($"Week {fact.Week}: EvidenceStatus={fact.EvidenceStatus ?? "Missing"}");
                }
            }
            var completeness = factIssues.Count == 0 ? "Complete" : "EvidenceMissing";
            var aggregateFreshnessStatus = horizonFacts
                .Select(item => item.EvidenceStatus)
                .FirstOrDefault(item => Freshness(item) != "Fresh");
            var missingReason = factIssues.Count > 0 ? string.Join("; ", factIssues) : null;
            return new BaselineEvidenceItem(
                definition.BufferId,
                $"{definition.ControlPoint} progress",
                Freshness(aggregateFreshnessStatus),
                completeness,
                definition.IsCritical,
                EvidenceReason(aggregateFreshnessStatus, completeness, missingReason));
        }).ToList();
        progressItems.AddRange(progress
            .Where(item => !knownDefinitionIds.Contains(item.BufferId))
            .GroupBy(item => item.BufferId, StringComparer.Ordinal)
            .Select(group => new BaselineEvidenceItem(
                $"UNKNOWN:{group.Key}",
                $"{group.Key} progress",
                Freshness(group.Select(item => item.EvidenceStatus).FirstOrDefault(item => Freshness(item) != "Fresh")),
                "EvidenceMissing",
                true,
                "Control-point progress references an undefined time-buffer definition")));

        var sections = new[]
        {
            TimeSection("TIME_BUFFER_DEFINITIONS", "Time-buffer definitions", asOf, definitionItems),
            TimeSection("TIME_BUFFER_PRODUCT_SCOPES", "Time-buffer product scopes", asOf, scopeItems),
            TimeSection("CONTROL_POINT_PROGRESS", "Time-buffer control-point progress", asOf, progressItems)
        };
        var evidenceMissing = sections.SelectMany(item => item.Items ?? Array.Empty<BaselineEvidenceItem>())
            .Any(item => item.FreshnessStatus != "Fresh" || item.CompletenessStatus != "Complete");
        return new TimeBufferEvidence(
            sections,
            new BaselineAnalysisAvailability(
                "TimeBuffer",
                evidenceMissing ? "EvidenceMissing" : "Complete",
                evidenceMissing
                    ? "At least one configured time buffer lacks fresh, complete evidence."
                    : "All configured time buffers have fresh definition, scope, and progress evidence."));
    }

    private static BaselineEvidenceSection TimeSection(
        string code,
        string name,
        string asOf,
        IReadOnlyList<BaselineEvidenceItem> items)
    {
        var freshness = items.All(item => item.FreshnessStatus == "Fresh") ? "Fresh" : "Stale";
        var completeness = items.All(item => item.CompletenessStatus == "Complete") ? "Complete" : "EvidenceMissing";
        var missingReason = string.Join("; ", items
            .Where(item => item.FreshnessStatus != "Fresh" || item.CompletenessStatus != "Complete")
            .Select(item => $"{item.ItemKey}: {item.MissingReason}"));
        return new BaselineEvidenceSection(
            code,
            name,
            "DDAE Demo Time Evidence",
            asOf,
            freshness,
            completeness,
            items.Count,
            "DemoFixture",
            true,
            string.IsNullOrWhiteSpace(missingReason) ? null : missingReason,
            items);
    }

    private static BaselineEvidenceSection NotApplicableTimeSection(string code, string name, string asOf)
    {
        return new BaselineEvidenceSection(
            code,
            name,
            "DDAE Demo Time Evidence",
            asOf,
            "NotApplicable",
            "NotApplicable",
            0,
            "DemoFixture",
            false,
            "No time-buffer definitions are configured.",
            Array.Empty<BaselineEvidenceItem>());
    }

    private static string Freshness(string? evidenceStatus) => evidenceStatus is "Stale" or "Expired"
        ? "Stale"
        : evidenceStatus == "NotApplicable"
            ? "NotApplicable"
            : "Fresh";

    private static bool IsCompleteEvidence(string? evidenceStatus) =>
        !string.IsNullOrWhiteSpace(evidenceStatus) &&
        evidenceStatus is not ("EvidenceMissing" or "Missing" or "Incomplete" or "NotApplicable");

    private static string? EvidenceReason(string? evidenceStatus, string completeness, string? explicitReason)
    {
        if (!string.IsNullOrWhiteSpace(explicitReason))
        {
            return explicitReason;
        }

        if (evidenceStatus is "Stale" or "Expired")
        {
            return $"EvidenceStatus={evidenceStatus}";
        }

        return completeness == "Complete" ? null : $"EvidenceStatus={evidenceStatus ?? "Missing"}";
    }

    private static BaselineEvidenceSection Section(
        string code,
        string name,
        string source,
        string asOf,
        int count,
        bool required = true,
        string? missingReason = null,
        string? completenessStatus = null)
    {
        var completeness = completenessStatus ?? (count > 0 || !required ? "Complete" : "EvidenceMissing");
        return new BaselineEvidenceSection(
            code,
            name,
            source,
            asOf,
            "Fresh",
            completeness,
            count,
            "DemoFixture",
            required,
            completeness == "Complete" ? null : missingReason);
    }

    private sealed record TimeBufferEvidence(
        IReadOnlyList<BaselineEvidenceSection> Sections,
        BaselineAnalysisAvailability Availability);
}
