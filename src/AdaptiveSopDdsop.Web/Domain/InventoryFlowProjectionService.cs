namespace AdaptiveSopDdsop.Web.Domain;

public static class InventoryFlowProjectionService
{
    private const string Complete = "Complete";
    private const string EvidenceMissing = "EvidenceMissing";
    private const string NotApplicable = "NotApplicable";
    private const string OutsideHorizon = "OutsideHorizon";
    private const string SimulatedReplenishment = "SimulatedReplenishment";
    private const string PrebuildResponse = "PrebuildResponse";
    private const int QuantityOutputPrecision = 0;

    public static InventoryFlowProjectionResult Project(
        ScenarioWorkspaceDataSet data,
        string caseId,
        IReadOnlyList<SkuBufferSetting> skus,
        IReadOnlyList<WeeklyDemand> demand,
        IReadOnlyList<ProjectedReplenishmentOrder> orders,
        IReadOnlyList<PrebuildCampaign> prebuild,
        IReadOnlyList<SupplierCapacityLimit> supplierLimits,
        string? baselineSnapshotId = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(caseId);
        ArgumentNullException.ThrowIfNull(skus);
        ArgumentNullException.ThrowIfNull(demand);
        ArgumentNullException.ThrowIfNull(orders);
        ArgumentNullException.ThrowIfNull(prebuild);
        ArgumentNullException.ThrowIfNull(supplierLimits);

        var horizonWeeks = data.Request.HorizonWeeks;
        var skuCodes = skus.Select(item => item.Sku).ToHashSet(StringComparer.Ordinal);
        var scopedData = ScopeValidationData(data, skus, demand, skuCodes);
        var validation = PlanningEvidenceValidator.ValidateForProjection(scopedData, horizonWeeks);
        var issues = validation.Issues.ToList();
        ValidateProjectionInputs(skus, orders, prebuild, skuCodes, issues);

        if (issues.Any(item => item.BlocksProjection))
        {
            return MissingEvidenceResult(caseId, baselineSnapshotId, horizonWeeks, issues);
        }

        var trace = new List<InventoryFlowTrace>
        {
            new(
                "ValidatedInputs",
                null,
                null,
                null,
                FormattableString.Invariant(
                    $"Validated {skus.Count} SKU(s) for {horizonWeeks} week(s); baseline={baselineSnapshotId ?? "none"}."))
        };
        var receiptLog = BuildReceiptLog(
            scopedData,
            skus,
            orders,
            prebuild,
            supplierLimits,
            horizonWeeks,
            issues,
            trace);
        if (issues.Any(item => item.BlocksProjection))
        {
            return MissingEvidenceResult(caseId, baselineSnapshotId, horizonWeeks, issues);
        }

        var points = BuildPoints(scopedData, skus, demand, receiptLog, horizonWeeks, trace);
        var skuSummaries = BuildSkuSummaries(points, receiptLog);
        var summary = BuildSummary(points, receiptLog, horizonWeeks);

        return new InventoryFlowProjectionResult(
            caseId,
            Complete,
            points,
            receiptLog,
            skuSummaries,
            summary,
            trace,
            issues,
            baselineSnapshotId);
    }

    private static ScenarioWorkspaceDataSet ScopeValidationData(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<SkuBufferSetting> skus,
        IReadOnlyList<WeeklyDemand> demand,
        IReadOnlySet<string> skuCodes)
    {
        return data with
        {
            Skus = skus,
            Inventory = data.Inventory.Where(item => skuCodes.Contains(item.Sku)).ToList(),
            Demand = demand,
            SupplierItemSources = data.SupplierItemSources.Where(item => skuCodes.Contains(item.Sku)).ToList(),
            DdmrpParameters = data.DdmrpParameters.Where(item => skuCodes.Contains(item.Sku)).ToList(),
            ConfirmedReceipts = data.ConfirmedReceipts?
                .Where(item => skuCodes.Contains(item.Sku))
                .ToList(),
            OpeningBacklog = data.OpeningBacklog?
                .Where(item => skuCodes.Contains(item.Sku))
                .ToList()
        };
    }

    private static void ValidateProjectionInputs(
        IReadOnlyList<SkuBufferSetting> skus,
        IReadOnlyList<ProjectedReplenishmentOrder> orders,
        IReadOnlyList<PrebuildCampaign> prebuild,
        IReadOnlySet<string> skuCodes,
        ICollection<PlanningEvidenceIssue> issues)
    {
        foreach (var duplicate in skus
                     .GroupBy(item => item.Sku, StringComparer.Ordinal)
                     .Where(group => group.Count() > 1))
        {
            AddProjectionIssue(issues, "ProjectionSku", duplicate.Key, null, "DuplicateProjectionSku", null);
        }

        foreach (var sku in skus)
        {
            if (sku.DecoupledLeadTimeDays <= 0)
            {
                AddProjectionIssue(issues, "ProjectionSku", sku.Sku, null, "InvalidDecoupledLeadTime", null);
            }

            if (sku.UnitCost < 0m)
            {
                AddProjectionIssue(issues, "ProjectionSku", sku.Sku, null, "NegativeUnitCost", null);
            }
        }

        foreach (var order in orders)
        {
            if (!skuCodes.Contains(order.Sku))
            {
                AddProjectionIssue(issues, "SimulatedReplenishment", order.Sku, order.Week, "UnknownOrderSku", null);
            }

            if (order.Week < 1)
            {
                AddProjectionIssue(issues, "SimulatedReplenishment", order.Sku, order.Week, "InvalidRecommendationWeek", null);
            }

            if (order.Quantity < 0m)
            {
                AddProjectionIssue(issues, "SimulatedReplenishment", order.Sku, order.Week, "NegativeOrderQuantity", null);
            }
        }

        foreach (var campaign in prebuild)
        {
            if (string.IsNullOrWhiteSpace(campaign.CampaignId))
            {
                AddProjectionIssue(issues, "PrebuildResponse", campaign.Sku, campaign.BuildWeek, "MissingCampaignId", null);
            }

            if (!skuCodes.Contains(campaign.Sku))
            {
                AddProjectionIssue(issues, "PrebuildResponse", campaign.Sku, campaign.BuildWeek, "UnknownCampaignSku", campaign.CampaignId);
            }

            if (campaign.BuildWeek < 1)
            {
                AddProjectionIssue(issues, "PrebuildResponse", campaign.Sku, campaign.BuildWeek, "InvalidBuildWeek", campaign.CampaignId);
            }

            if (campaign.Quantity < 0m)
            {
                AddProjectionIssue(issues, "PrebuildResponse", campaign.Sku, campaign.BuildWeek, "NegativeCampaignQuantity", campaign.CampaignId);
            }
        }

        foreach (var campaignGroup in prebuild
                     .Where(item => !string.IsNullOrWhiteSpace(item.CampaignId))
                     .GroupBy(item => item.CampaignId, StringComparer.Ordinal)
                     .Where(group => group.Skip(1).Any(item => item != group.First())))
        {
            var campaign = campaignGroup.First();
            AddProjectionIssue(
                issues,
                "PrebuildResponse",
                campaign.Sku,
                campaign.BuildWeek,
                "ConflictingCampaignId",
                campaign.CampaignId);
        }
    }

    private static IReadOnlyList<InventoryReceiptLogEntry> BuildReceiptLog(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<SkuBufferSetting> skus,
        IReadOnlyList<ProjectedReplenishmentOrder> orders,
        IReadOnlyList<PrebuildCampaign> prebuild,
        IReadOnlyList<SupplierCapacityLimit> supplierLimits,
        int horizonWeeks,
        ICollection<PlanningEvidenceIssue> issues,
        ICollection<InventoryFlowTrace> trace)
    {
        var receiptLog = new List<InventoryReceiptLogEntry>();

        foreach (var receipt in (data.ConfirmedReceipts ?? Array.Empty<ConfirmedReceiptEvidence>())
                     .OrderBy(item => item.Sku, StringComparer.Ordinal)
                     .ThenBy(item => item.ExpectedReceiptWeek)
                     .ThenBy(item => item.ReceiptId, StringComparer.Ordinal))
        {
            var insideHorizon = receipt.ExpectedReceiptWeek <= horizonWeeks;
            receiptLog.Add(new InventoryReceiptLogEntry(
                receipt.Sku,
                receipt.ReceiptType,
                receipt.ReceiptId,
                null,
                receipt.ExpectedReceiptWeek,
                receipt.Quantity,
                insideHorizon ? receipt.Quantity : 0m,
                0m,
                insideHorizon ? 0m : receipt.Quantity,
                receipt.EvidenceStatus,
                "FrozenBaseline",
                $"Frozen receipt {receipt.SourceReference} arrives in authoritative week {receipt.ExpectedReceiptWeek}.",
                receipt.Supplier,
                receipt.MaterialFamily));
        }

        var skuByCode = skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);
        var simulatedCandidates = new List<SimulatedReceiptCandidate>();
        var simulatedOrders = orders
            .Where(item => !string.Equals(item.Trigger, "PrebuildCampaign", StringComparison.Ordinal))
            .OrderBy(item => item.Sku, StringComparer.Ordinal)
            .ThenBy(item => item.Week)
            .ThenBy(item => item.Trigger, StringComparer.Ordinal)
            .ThenBy(item => item.Quantity)
            .ThenBy(item => item.Value)
            .ToList();
        for (var index = 0; index < simulatedOrders.Count; index++)
        {
            var order = simulatedOrders[index];
            var sku = skuByCode[order.Sku];
            var receiptDelayWeeks = Math.Max(1, (int)Math.Ceiling(sku.DecoupledLeadTimeDays / 7m));
            var arrivalWeek = order.Week + receiptDelayWeeks;
            var sourceId = $"SIM-{order.Sku}-W{order.Week:D3}-{index + 1:D3}";
            var mappings = data.SupplierItemSources
                .Where(item => item.Sku == order.Sku)
                .Select(item => (item.Supplier, item.MaterialFamily))
                .Distinct()
                .ToList();
            if (mappings.Count == 0 ||
                mappings.Any(item => string.IsNullOrWhiteSpace(item.Supplier) || string.IsNullOrWhiteSpace(item.MaterialFamily)))
            {
                AddProjectionIssue(
                    issues,
                    "SupplierCapacity",
                    order.Sku,
                    arrivalWeek,
                    "MissingConstrainedSupplyMapping",
                    sourceId);
                continue;
            }

            if (mappings.Count > 1)
            {
                AddProjectionIssue(
                    issues,
                    "SupplierCapacity",
                    order.Sku,
                    arrivalWeek,
                    "AmbiguousConstrainedSupplyMapping",
                    sourceId);
                continue;
            }

            var mapping = mappings[0];
            simulatedCandidates.Add(new SimulatedReceiptCandidate(
                order.Sku,
                sourceId,
                order.Week,
                arrivalWeek,
                order.Quantity,
                mapping.Supplier,
                mapping.MaterialFamily,
                $"Recommendation week {order.Week} plus {receiptDelayWeeks} DLT week(s) arrives in week {arrivalWeek}."));
        }

        AllocateSimulatedReceipts(
            data,
            simulatedCandidates,
            supplierLimits,
            horizonWeeks,
            receiptLog,
            issues,
            trace);

        foreach (var campaignGroup in prebuild
                     .GroupBy(item => item.CampaignId, StringComparer.Ordinal)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var campaign = campaignGroup
                .OrderBy(item => item.Sku, StringComparer.Ordinal)
                .ThenBy(item => item.BuildWeek)
                .ThenBy(item => item.Quantity)
                .First();
            if (campaignGroup.Count() > 1)
            {
                trace.Add(new InventoryFlowTrace(
                    "InputDeduplication",
                    campaign.Sku,
                    campaign.BuildWeek,
                    campaign.CampaignId,
                    $"Prebuild campaign {campaign.CampaignId} appeared {campaignGroup.Count()} times and was counted once."));
            }

            var insideHorizon = campaign.BuildWeek <= horizonWeeks;
            receiptLog.Add(new InventoryReceiptLogEntry(
                campaign.Sku,
                PrebuildResponse,
                campaign.CampaignId,
                null,
                campaign.BuildWeek,
                campaign.Quantity,
                insideHorizon ? campaign.Quantity : 0m,
                0m,
                insideHorizon ? 0m : campaign.Quantity,
                Complete,
                "ResponseAssumption",
                $"Prebuild response is received once in configured completion week {campaign.BuildWeek}."));
        }

        receiptLog = receiptLog
            .OrderBy(item => item.Sku, StringComparer.Ordinal)
            .ThenBy(item => item.ArrivalWeek)
            .ThenBy(item => item.SourceId, StringComparer.Ordinal)
            .ToList();

        foreach (var receipt in receiptLog)
        {
            trace.Add(new InventoryFlowTrace(
                "ReceiptAllocation",
                receipt.Sku,
                receipt.ArrivalWeek,
                receipt.SourceId,
                FormattableString.Invariant(
                    $"{receipt.SourceKind}: requested={receipt.RequestedQuantity}; accepted={receipt.AcceptedQuantity}; deferred={receipt.DeferredQuantity}; outsideHorizon={receipt.OutsideHorizonQuantity}.")));
        }

        return receiptLog;
    }

    private static void AllocateSimulatedReceipts(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<SimulatedReceiptCandidate> candidates,
        IReadOnlyList<SupplierCapacityLimit> supplierLimits,
        int horizonWeeks,
        ICollection<InventoryReceiptLogEntry> receiptLog,
        ICollection<PlanningEvidenceIssue> issues,
        ICollection<InventoryFlowTrace> trace)
    {
        var scheduled = candidates
            .Where(item => item.ArrivalWeek <= horizonWeeks)
            .GroupBy(item => item.ArrivalWeek)
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var candidate in candidates
                     .Where(item => item.ArrivalWeek > horizonWeeks)
                     .OrderBy(item => item.ArrivalWeek)
                     .ThenBy(item => item.SourceId, StringComparer.Ordinal))
        {
            AddOutsideHorizonReceipt(candidate, receiptLog, trace);
        }

        for (var week = 1; week <= horizonWeeks; week++)
        {
            if (!scheduled.TryGetValue(week, out var weeklyCandidates))
            {
                continue;
            }

            foreach (var group in weeklyCandidates
                         .GroupBy(item => (item.Supplier, item.MaterialFamily))
                         .OrderBy(item => item.Key.Supplier, StringComparer.Ordinal)
                         .ThenBy(item => item.Key.MaterialFamily, StringComparer.Ordinal))
            {
                var stableCandidates = group
                    .OrderBy(item => item.SourceId, StringComparer.Ordinal)
                    .ToList();
                var windows = data.SupplierCapacityWindows
                    .Where(item =>
                        item.Supplier == group.Key.Supplier &&
                        item.MaterialFamily == group.Key.MaterialFamily &&
                        item.Week == week)
                    .ToList();
                if (windows.Count == 0)
                {
                    foreach (var candidate in stableCandidates)
                    {
                        AddProjectionIssue(
                            issues,
                            "SupplierCapacity",
                            candidate.Sku,
                            week,
                            "MissingConstrainedCapacityWeek",
                            candidate.SourceId);
                    }

                    continue;
                }

                if (windows.Count > 1)
                {
                    foreach (var candidate in stableCandidates)
                    {
                        AddProjectionIssue(
                            issues,
                            "SupplierCapacity",
                            candidate.Sku,
                            week,
                            "AmbiguousConstrainedCapacityWeek",
                            candidate.SourceId);
                    }

                    continue;
                }

                var window = windows[0];
                if (string.Equals(window.RiskStatus, NotApplicable, StringComparison.Ordinal))
                {
                    AllocateUnboundedGroup(stableCandidates, week, receiptLog, trace);
                    continue;
                }

                var combinedLimit = supplierLimits.LastOrDefault(item =>
                    item.Supplier == group.Key.Supplier &&
                    item.MaterialFamily == group.Key.MaterialFamily &&
                    week >= item.StartWeek &&
                    week <= item.EndWeek);
                var capacity = combinedLimit?.CommittedCapacity ?? window.CommittedCapacity;
                if (capacity < 0m)
                {
                    foreach (var candidate in stableCandidates)
                    {
                        AddProjectionIssue(
                            issues,
                            "SupplierCapacity",
                            candidate.Sku,
                            week,
                            "NegativeConstrainedCapacity",
                            candidate.SourceId);
                    }

                    continue;
                }

                AllocateConstrainedGroup(
                    stableCandidates,
                    week,
                    capacity,
                    horizonWeeks,
                    scheduled,
                    receiptLog,
                    trace);
            }
        }
    }

    private static void AllocateUnboundedGroup(
        IReadOnlyList<SimulatedReceiptCandidate> candidates,
        int week,
        ICollection<InventoryReceiptLogEntry> receiptLog,
        ICollection<InventoryFlowTrace> trace)
    {
        foreach (var candidate in candidates)
        {
            receiptLog.Add(new InventoryReceiptLogEntry(
                candidate.Sku,
                SimulatedReplenishment,
                candidate.SourceId,
                candidate.RecommendationWeek,
                week,
                candidate.Quantity,
                candidate.Quantity,
                0m,
                0m,
                NotApplicable,
                "SimulationAssumption",
                $"{candidate.Explanation} Supplier capacity is explicitly not applicable for this internal/unconstrained source.",
                candidate.Supplier,
                candidate.MaterialFamily));
        }

        trace.Add(new InventoryFlowTrace(
            "SupplierCapacityAllocation",
            null,
            week,
            null,
            FormattableString.Invariant(
                $"{candidates[0].Supplier}/{candidates[0].MaterialFamily}: requested={candidates.Sum(item => item.Quantity)}; capacity=unbounded; accepted={candidates.Sum(item => item.Quantity)}; deferred=0; status={NotApplicable}.")));
    }

    private static void AllocateConstrainedGroup(
        IReadOnlyList<SimulatedReceiptCandidate> candidates,
        int week,
        decimal capacity,
        int horizonWeeks,
        IDictionary<int, List<SimulatedReceiptCandidate>> scheduled,
        ICollection<InventoryReceiptLogEntry> receiptLog,
        ICollection<InventoryFlowTrace> trace)
    {
        var totalRequested = candidates.Sum(item => item.Quantity);
        var acceptedBySource = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var residualBySource = new Dictionary<string, decimal>(StringComparer.Ordinal);
        if (totalRequested <= capacity || totalRequested == 0m)
        {
            foreach (var candidate in candidates)
            {
                acceptedBySource[candidate.SourceId] = candidate.Quantity;
                residualBySource[candidate.SourceId] = 0m;
            }
        }
        else
        {
            var available = Math.Max(0m, Math.Min(capacity, totalRequested));
            var roundedAvailable = Math.Min(totalRequested, RoundQuantity(available));
            foreach (var candidate in candidates)
            {
                var rawShare = available * candidate.Quantity / totalRequested;
                var accepted = Math.Min(candidate.Quantity, Math.Max(0m, RoundQuantity(rawShare)));
                acceptedBySource[candidate.SourceId] = accepted;
                residualBySource[candidate.SourceId] = 0m;
            }

            var residual = roundedAvailable - acceptedBySource.Values.Sum();
            var unassignedResidual = residual;
            for (var index = candidates.Count - 1; index >= 0 && unassignedResidual != 0m; index--)
            {
                var candidate = candidates[index];
                var accepted = acceptedBySource[candidate.SourceId];
                var adjustment = unassignedResidual > 0m
                    ? Math.Min(unassignedResidual, candidate.Quantity - accepted)
                    : -Math.Min(-unassignedResidual, accepted);
                acceptedBySource[candidate.SourceId] += adjustment;
                residualBySource[candidate.SourceId] += adjustment;
                unassignedResidual -= adjustment;
            }

            if (unassignedResidual != 0m)
            {
                throw new InvalidOperationException("Rounded supplier allocation could not conserve the available quantity.");
            }

            var last = candidates[^1];
            trace.Add(new InventoryFlowTrace(
                "SupplierCapacityRounding",
                last.Sku,
                week,
                last.SourceId,
                FormattableString.Invariant(
                    $"outputPrecision={QuantityOutputPrecision}; residual={residual}; assignedTo={last.SourceId}; assignedAmount={residualBySource[last.SourceId]}; overflowAdjustment={residual - residualBySource[last.SourceId]}; allocatedTotal={acceptedBySource.Values.Sum()}.")));
        }

        var totalAccepted = acceptedBySource.Values.Sum();
        trace.Add(new InventoryFlowTrace(
            "SupplierCapacityAllocation",
            null,
            week,
            null,
            FormattableString.Invariant(
                $"{candidates[0].Supplier}/{candidates[0].MaterialFamily}: requested={totalRequested}; capacity={capacity}; accepted={totalAccepted}; deferred={totalRequested - totalAccepted}; status={Complete}.")));

        foreach (var candidate in candidates)
        {
            var accepted = acceptedBySource[candidate.SourceId];
            var deferred = candidate.Quantity - accepted;
            receiptLog.Add(new InventoryReceiptLogEntry(
                candidate.Sku,
                SimulatedReplenishment,
                candidate.SourceId,
                candidate.RecommendationWeek,
                week,
                candidate.Quantity,
                accepted,
                deferred,
                0m,
                Complete,
                "SimulationAssumption",
                FormattableString.Invariant(
                    $"{candidate.Explanation} Supplier capacity allocation in week {week}: capacity={capacity}; requested={candidate.Quantity}; accepted={accepted}; deferred={deferred}."),
                candidate.Supplier,
                candidate.MaterialFamily,
                capacity,
                residualBySource[candidate.SourceId]));

            if (deferred <= 0m)
            {
                continue;
            }

            var carry = candidate with { ArrivalWeek = week + 1, Quantity = deferred };
            if (week < horizonWeeks)
            {
                if (!scheduled.TryGetValue(week + 1, out var nextWeek))
                {
                    nextWeek = new List<SimulatedReceiptCandidate>();
                    scheduled[week + 1] = nextWeek;
                }

                nextWeek.Add(carry);
                trace.Add(new InventoryFlowTrace(
                    "SupplierCapacityCarryForward",
                    candidate.Sku,
                    week,
                    candidate.SourceId,
                    FormattableString.Invariant($"deferred={deferred}; carriesToWeek={week + 1}.")));
            }
            else
            {
                AddOutsideHorizonReceipt(carry, receiptLog, trace);
            }
        }
    }

    private static void AddOutsideHorizonReceipt(
        SimulatedReceiptCandidate candidate,
        ICollection<InventoryReceiptLogEntry> receiptLog,
        ICollection<InventoryFlowTrace> trace)
    {
        receiptLog.Add(new InventoryReceiptLogEntry(
            candidate.Sku,
            SimulatedReplenishment,
            candidate.SourceId,
            candidate.RecommendationWeek,
            candidate.ArrivalWeek,
            candidate.Quantity,
            0m,
            0m,
            candidate.Quantity,
            OutsideHorizon,
            "SimulationAssumption",
            $"{candidate.Explanation} Quantity remains outside the projection horizon.",
            candidate.Supplier,
            candidate.MaterialFamily));
        trace.Add(new InventoryFlowTrace(
            "SupplierCapacityCarryForward",
            candidate.Sku,
            candidate.ArrivalWeek,
            candidate.SourceId,
            FormattableString.Invariant($"outsideHorizon={candidate.Quantity}; nextAttemptWeek={candidate.ArrivalWeek}.")));
    }

    private static decimal RoundQuantity(decimal value) =>
        decimal.Round(value, QuantityOutputPrecision);

    private static IReadOnlyList<InventoryFlowPoint> BuildPoints(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<SkuBufferSetting> skus,
        IReadOnlyList<WeeklyDemand> demand,
        IReadOnlyList<InventoryReceiptLogEntry> receiptLog,
        int horizonWeeks,
        ICollection<InventoryFlowTrace> trace)
    {
        var points = new List<InventoryFlowPoint>();
        var skuCodes = skus.Select(item => item.Sku).ToHashSet(StringComparer.Ordinal);
        var demandBySkuWeek = demand
            .Where(item => skuCodes.Contains(item.Sku) && item.Week >= 1 && item.Week <= horizonWeeks)
            .ToDictionary(item => (item.Sku, item.Week), item => item.BaselineDemand);
        var receiptsBySkuWeek = receiptLog
            .Where(item => item.AcceptedQuantity > 0m)
            .GroupBy(item => (item.Sku, item.ArrivalWeek))
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var sku in skus.OrderBy(item => item.Sku, StringComparer.Ordinal))
        {
            var openingOnHand = data.Inventory.Single(item => item.Sku == sku.Sku).OnHand;
            var openingBacklog = data.OpeningBacklog!.Single(item => item.Sku == sku.Sku).Quantity;

            for (var week = 1; week <= horizonWeeks; week++)
            {
                var currentDemand = demandBySkuWeek[(sku.Sku, week)];
                receiptsBySkuWeek.TryGetValue((sku.Sku, week), out var weeklyReceipts);
                weeklyReceipts ??= new List<InventoryReceiptLogEntry>();
                var frozenReceipts = weeklyReceipts
                    .Where(item => item.SourceKind is "ConfirmedInTransit" or "ConfirmedOpenSupply")
                    .Sum(item => item.AcceptedQuantity);
                var simulatedReceipts = weeklyReceipts
                    .Where(item => item.SourceKind == SimulatedReplenishment)
                    .Sum(item => item.AcceptedQuantity);
                var prebuildReceipts = weeklyReceipts
                    .Where(item => item.SourceKind == PrebuildResponse)
                    .Sum(item => item.AcceptedQuantity);

                var due = openingBacklog + currentDemand;
                var available = openingOnHand + frozenReceipts + simulatedReceipts + prebuildReceipts;
                var fulfilled = Math.Min(due, available);
                var fulfilledOpeningBacklog = Math.Min(openingBacklog, fulfilled);
                var fulfilledNewDemandOnTime = Math.Min(
                    currentDemand,
                    Math.Max(0m, fulfilled - fulfilledOpeningBacklog));
                var endingBacklog = Math.Max(0m, due - fulfilled);
                var endingOnHand = Math.Max(0m, available - fulfilled);
                var weeklyServicePercent = ServicePercent(currentDemand, fulfilledNewDemandOnTime);
                var weeklyServiceStatus = currentDemand == 0m ? NotApplicable : Complete;

                points.Add(new InventoryFlowPoint(
                    sku.Sku,
                    week,
                    openingOnHand,
                    openingBacklog,
                    currentDemand,
                    frozenReceipts,
                    simulatedReceipts,
                    prebuildReceipts,
                    fulfilledOpeningBacklog,
                    fulfilledNewDemandOnTime,
                    fulfilled,
                    endingOnHand,
                    endingBacklog,
                    endingOnHand * sku.UnitCost,
                    weeklyServicePercent,
                    weeklyServiceStatus));
                trace.Add(new InventoryFlowTrace(
                    "BucketEquation",
                    sku.Sku,
                    week,
                    null,
                    FormattableString.Invariant(
                        $"due={openingBacklog}+{currentDemand}={due}; available={openingOnHand}+{frozenReceipts}+{simulatedReceipts}+{prebuildReceipts}={available}; fulfilled={fulfilled}; endingOnHand={endingOnHand}; endingBacklog={endingBacklog}; oldBacklogFirst={fulfilledOpeningBacklog}; newDemandOnTime={fulfilledNewDemandOnTime}.")));

                openingOnHand = endingOnHand;
                openingBacklog = endingBacklog;
            }
        }

        return points;
    }

    private static IReadOnlyList<InventoryFlowSkuSummary> BuildSkuSummaries(
        IReadOnlyList<InventoryFlowPoint> points,
        IReadOnlyList<InventoryReceiptLogEntry> receiptLog)
    {
        return points
            .GroupBy(item => item.Sku, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group =>
            {
                var skuPoints = group.OrderBy(item => item.Week).ToList();
                var skuReceipts = receiptLog.Where(item => item.Sku == group.Key).ToList();
                var totalDemand = skuPoints.Sum(item => item.Demand);
                var fulfilledNewDemand = skuPoints.Sum(item => item.FulfilledNewDemandOnTime);
                return new InventoryFlowSkuSummary(
                    group.Key,
                    ServicePercent(totalDemand, fulfilledNewDemand),
                    totalDemand == 0m ? NotApplicable : Complete,
                    totalDemand,
                    fulfilledNewDemand,
                    skuPoints.Sum(item => item.TotalFulfilledDemand),
                    skuPoints.Average(item => item.EndingInventoryValue),
                    skuPoints.Max(item => item.EndingInventoryValue),
                    skuPoints[^1].EndingInventoryValue,
                    skuPoints[^1].EndingBacklog,
                    BacklogRecoveryWeek(skuPoints),
                    skuPoints.Sum(item => item.FrozenReceiptQuantity),
                    skuPoints.Sum(item => item.SimulatedReceiptQuantity),
                    skuPoints.Sum(item => item.PrebuildReceiptQuantity),
                    skuReceipts.Sum(item => item.OutsideHorizonQuantity));
            })
            .ToList();
    }

    private static InventoryFlowSummary BuildSummary(
        IReadOnlyList<InventoryFlowPoint> points,
        IReadOnlyList<InventoryReceiptLogEntry> receiptLog,
        int horizonWeeks)
    {
        var weeklyInventoryValues = points
            .GroupBy(item => item.Week)
            .OrderBy(group => group.Key)
            .Select(group => group.Sum(item => item.EndingInventoryValue))
            .ToList();
        var totalDemand = points.Sum(item => item.Demand);
        var fulfilledNewDemand = points.Sum(item => item.FulfilledNewDemandOnTime);
        var endingPoints = points.Where(item => item.Week == horizonWeeks).ToList();

        return new InventoryFlowSummary(
            ServicePercent(totalDemand, fulfilledNewDemand),
            totalDemand == 0m ? NotApplicable : Complete,
            totalDemand,
            fulfilledNewDemand,
            points.Sum(item => item.TotalFulfilledDemand),
            weeklyInventoryValues.Average(),
            weeklyInventoryValues.Max(),
            endingPoints.Sum(item => item.EndingInventoryValue),
            endingPoints.Sum(item => item.EndingBacklog),
            OverallBacklogRecoveryWeek(points),
            points.Sum(item => item.FrozenReceiptQuantity),
            points.Sum(item => item.SimulatedReceiptQuantity),
            points.Sum(item => item.PrebuildReceiptQuantity),
            receiptLog.Sum(item => item.OutsideHorizonQuantity));
    }

    private static int? BacklogRecoveryWeek(IReadOnlyList<InventoryFlowPoint> points) =>
        points
            .Where(item => item.OpeningBacklog > 0m && item.EndingBacklog == 0m)
            .Select(item => (int?)item.Week)
            .FirstOrDefault();

    private static int? OverallBacklogRecoveryWeek(IReadOnlyList<InventoryFlowPoint> points)
    {
        foreach (var week in points.GroupBy(item => item.Week).OrderBy(group => group.Key))
        {
            if (week.Any(item => item.OpeningBacklog > 0m) && week.All(item => item.EndingBacklog == 0m))
            {
                return week.Key;
            }
        }

        return null;
    }

    private static decimal? ServicePercent(decimal demand, decimal fulfilledOnTime) =>
        demand == 0m ? null : fulfilledOnTime * 100m / demand;

    private static InventoryFlowProjectionResult MissingEvidenceResult(
        string caseId,
        string? baselineSnapshotId,
        int horizonWeeks,
        IReadOnlyList<PlanningEvidenceIssue> issues)
    {
        return new InventoryFlowProjectionResult(
            caseId,
            EvidenceMissing,
            Array.Empty<InventoryFlowPoint>(),
            Array.Empty<InventoryReceiptLogEntry>(),
            Array.Empty<InventoryFlowSkuSummary>(),
            null,
            new[]
            {
                new InventoryFlowTrace(
                    "Validation",
                    null,
                    null,
                    null,
                    $"Physical projection was not produced for horizon {horizonWeeks}; {issues.Count(item => item.BlocksProjection)} blocking issue(s) remain.")
            },
            issues,
            baselineSnapshotId);
    }

    private static void AddProjectionIssue(
        ICollection<PlanningEvidenceIssue> issues,
        string scope,
        string? sku,
        int? week,
        string reason,
        string? sourceId)
    {
        issues.Add(new PlanningEvidenceIssue(
            scope,
            sku,
            week,
            reason,
            sourceId,
            BlocksFreeze: false,
            BlocksProjection: true));
    }

    private sealed record SimulatedReceiptCandidate(
        string Sku,
        string SourceId,
        int RecommendationWeek,
        int ArrivalWeek,
        decimal Quantity,
        string Supplier,
        string MaterialFamily,
        string Explanation);
}
