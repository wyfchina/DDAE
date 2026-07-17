namespace AdaptiveSopDdsop.Web.Domain;

public static class InventoryFlowProjectionService
{
    private const string Complete = "Complete";
    private const string EvidenceMissing = "EvidenceMissing";
    private const string NotApplicable = "NotApplicable";
    private const string SimulatedReplenishment = "SimulatedReplenishment";
    private const string PrebuildResponse = "PrebuildResponse";

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

        // Supplier limits are accepted here so Task 4 can constrain only simulated
        // receipts without changing this public projection contract.
        _ = supplierLimits;

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
            horizonWeeks,
            trace);
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
        int horizonWeeks,
        ICollection<InventoryFlowTrace> trace)
    {
        var candidates = new List<ReceiptCandidate>();
        var skuByCode = skus.ToDictionary(item => item.Sku, StringComparer.Ordinal);

        foreach (var receipt in (data.ConfirmedReceipts ?? Array.Empty<ConfirmedReceiptEvidence>())
                     .OrderBy(item => item.Sku, StringComparer.Ordinal)
                     .ThenBy(item => item.ExpectedReceiptWeek)
                     .ThenBy(item => item.ReceiptId, StringComparer.Ordinal))
        {
            candidates.Add(new ReceiptCandidate(
                receipt.Sku,
                receipt.ReceiptType,
                receipt.ReceiptId,
                null,
                receipt.ExpectedReceiptWeek,
                receipt.Quantity,
                receipt.EvidenceStatus,
                "FrozenBaseline",
                $"Frozen receipt {receipt.SourceReference} arrives in authoritative week {receipt.ExpectedReceiptWeek}."));
        }

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
            candidates.Add(new ReceiptCandidate(
                order.Sku,
                SimulatedReplenishment,
                $"SIM-{order.Sku}-W{order.Week:D3}-{index + 1:D3}",
                order.Week,
                arrivalWeek,
                order.Quantity,
                Complete,
                "SimulationAssumption",
                $"Recommendation week {order.Week} plus {receiptDelayWeeks} DLT week(s) arrives in week {arrivalWeek}."));
        }

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

            candidates.Add(new ReceiptCandidate(
                campaign.Sku,
                PrebuildResponse,
                campaign.CampaignId,
                null,
                campaign.BuildWeek,
                campaign.Quantity,
                Complete,
                "ResponseAssumption",
                $"Prebuild response is received once in configured completion week {campaign.BuildWeek}."));
        }

        var receiptLog = candidates
            .Select(candidate =>
            {
                var insideHorizon = candidate.ArrivalWeek <= horizonWeeks;
                return new InventoryReceiptLogEntry(
                    candidate.Sku,
                    candidate.SourceKind,
                    candidate.SourceId,
                    candidate.RecommendationWeek,
                    candidate.ArrivalWeek,
                    candidate.Quantity,
                    insideHorizon ? candidate.Quantity : 0m,
                    0m,
                    insideHorizon ? 0m : candidate.Quantity,
                    candidate.EvidenceStatus,
                    candidate.EvidenceSource,
                    candidate.Explanation);
            })
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

    private sealed record ReceiptCandidate(
        string Sku,
        string SourceKind,
        string SourceId,
        int? RecommendationWeek,
        int ArrivalWeek,
        decimal Quantity,
        string EvidenceStatus,
        string EvidenceSource,
        string Explanation);
}
