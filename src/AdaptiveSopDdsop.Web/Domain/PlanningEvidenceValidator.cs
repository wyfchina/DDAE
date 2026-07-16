using System.Globalization;

namespace AdaptiveSopDdsop.Web.Domain;

public static class PlanningEvidenceValidator
{
    private const int FrozenCoverageWeeks = 52;
    private const decimal OpenSupplyTolerance = 0.01m;
    private static readonly HashSet<string> AllowedReceiptTypes = new(StringComparer.Ordinal)
    {
        "ConfirmedInTransit",
        "ConfirmedOpenSupply"
    };
    private static readonly TimeZoneInfo ShanghaiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");

    public static PlanningEvidenceValidationResult ValidateForFreeze(ScenarioWorkspaceDataSet data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return Validate(data, FrozenCoverageWeeks, isFreeze: true);
    }

    public static PlanningEvidenceValidationResult ValidateForProjection(
        ScenarioWorkspaceDataSet data,
        int requestedHorizonWeeks)
    {
        ArgumentNullException.ThrowIfNull(data);
        return Validate(data, requestedHorizonWeeks, isFreeze: false);
    }

    public static int WeekForDate(DateOnly anchor, DateOnly date)
    {
        var days = date.DayNumber - anchor.DayNumber;
        return days < 0 ? 0 : days / 7 + 1;
    }

    public static DateOnly BusinessDateForSourceTimestamp(string sourceTimestampUtc)
    {
        var timestamp = DateTimeOffset.Parse(
            sourceTimestampUtc,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        var businessTimestamp = TimeZoneInfo.ConvertTime(timestamp, ShanghaiTimeZone);
        return DateOnly.FromDateTime(businessTimestamp.DateTime);
    }

    private static PlanningEvidenceValidationResult Validate(
        ScenarioWorkspaceDataSet data,
        int requestedHorizonWeeks,
        bool isFreeze)
    {
        var issues = new List<PlanningEvidenceIssue>();
        var horizonWeeks = requestedHorizonWeeks;
        if (requestedHorizonWeeks is < 1 or > FrozenCoverageWeeks)
        {
            AddBlockingIssue(issues, "Coverage", null, requestedHorizonWeeks, "InvalidHorizon", null);
            horizonWeeks = Math.Clamp(requestedHorizonWeeks, 1, FrozenCoverageWeeks);
        }

        ValidateCoverage(data, horizonWeeks, isFreeze, issues);

        var skus = (data.Skus ?? Array.Empty<SkuBufferSetting>())
            .GroupBy(item => item.Sku, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();
        if (skus.Count == 0)
        {
            AddBlockingIssue(issues, "Workspace", null, null, "MissingSkuEvidence", null);
        }

        ValidateInventory(data, skus, issues);
        ValidateOpeningBacklog(data, skus, issues);
        ValidateDemand(data, skus, horizonWeeks, issues);
        ValidateReceipts(data, skus, horizonWeeks, issues);
        ValidateDdmrpParameters(data, skus, horizonWeeks, issues);

        var blocked = isFreeze
            ? issues.Any(item => item.BlocksFreeze)
            : issues.Any(item => item.BlocksProjection);
        return new PlanningEvidenceValidationResult(blocked ? "Incomplete" : "Complete", issues);
    }

    private static void ValidateCoverage(
        ScenarioWorkspaceDataSet data,
        int horizonWeeks,
        bool isFreeze,
        List<PlanningEvidenceIssue> issues)
    {
        var coverage = data.PlanningEvidenceCoverage;
        if (coverage is null)
        {
            AddBlockingIssue(issues, "Coverage", null, null, "MissingPlanningEvidenceCoverage", null);
            return;
        }

        if (coverage.AnchorDate != data.Request.AnchorDate)
        {
            AddBlockingIssue(issues, "Coverage", null, null, "AnchorDateMismatch", null);
        }

        var invalidRange = isFreeze
            ? coverage.CoverageFromWeek != 1 || coverage.CoverageThroughWeek != FrozenCoverageWeeks
            : coverage.CoverageFromWeek != 1 || coverage.CoverageThroughWeek < horizonWeeks;
        if (invalidRange)
        {
            AddBlockingIssue(issues, "Coverage", null, horizonWeeks, "IncompleteCoverage", null);
        }

        if (!IsComplete(coverage.EvidenceStatus))
        {
            AddBlockingIssue(issues, "Coverage", null, null, "CoverageEvidenceNotComplete", null);
        }
    }

    private static void ValidateInventory(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<SkuBufferSetting> skus,
        List<PlanningEvidenceIssue> issues)
    {
        var inventory = data.Inventory ?? Array.Empty<InventoryPosition>();
        foreach (var sku in skus)
        {
            var rows = inventory.Where(item => item.Sku == sku.Sku).ToList();
            if (rows.Count == 0)
            {
                AddBlockingIssue(issues, "Inventory", sku.Sku, null, "MissingInventory", null);
                continue;
            }

            if (rows.Count > 1)
            {
                AddBlockingIssue(issues, "Inventory", sku.Sku, null, "DuplicateInventory", null);
            }

            foreach (var row in rows.Where(item => item.OnHand < 0m || item.OpenSupply < 0m || item.QualifiedDemand < 0m))
            {
                AddBlockingIssue(issues, "Inventory", sku.Sku, null, "NegativeInventoryQuantity", null);
            }
        }
    }

    private static void ValidateOpeningBacklog(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<SkuBufferSetting> skus,
        List<PlanningEvidenceIssue> issues)
    {
        if (data.OpeningBacklog is null)
        {
            AddBlockingIssue(issues, "OpeningBacklog", null, null, "MissingOpeningBacklogEvidence", null);
        }

        var backlog = data.OpeningBacklog ?? Array.Empty<OpeningBacklogEvidence>();
        foreach (var sku in skus)
        {
            var rows = backlog.Where(item => item.Sku == sku.Sku).ToList();
            if (rows.Count == 0)
            {
                AddBlockingIssue(issues, "OpeningBacklog", sku.Sku, null, "MissingOpeningBacklog", null);
                continue;
            }

            if (rows.Count > 1)
            {
                AddBlockingIssue(issues, "OpeningBacklog", sku.Sku, null, "DuplicateOpeningBacklog", null);
            }

            foreach (var row in rows)
            {
                if (row.Quantity < 0m)
                {
                    AddBlockingIssue(issues, "OpeningBacklog", sku.Sku, null, "NegativeOpeningBacklogQuantity", row.BacklogId);
                }

                if (string.IsNullOrWhiteSpace(row.BacklogId) ||
                    string.IsNullOrWhiteSpace(row.SourceReference) ||
                    string.IsNullOrWhiteSpace(row.EvidenceLabel))
                {
                    AddBlockingIssue(issues, "OpeningBacklog", sku.Sku, null, "IncompleteOpeningBacklogEvidence", row.BacklogId);
                }

                if (!IsComplete(row.EvidenceStatus))
                {
                    AddBlockingIssue(issues, "OpeningBacklog", sku.Sku, null, "OpeningBacklogEvidenceNotComplete", row.BacklogId);
                }

                if (!IsTimestamp(row.AsOfUtc))
                {
                    AddBlockingIssue(issues, "OpeningBacklog", sku.Sku, null, "InvalidAsOfUtc", row.BacklogId);
                }
            }
        }
    }

    private static void ValidateDemand(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<SkuBufferSetting> skus,
        int horizonWeeks,
        List<PlanningEvidenceIssue> issues)
    {
        var demand = data.Demand ?? Array.Empty<WeeklyDemand>();
        foreach (var sku in skus)
        {
            var skuDemand = demand
                .Where(item => item.Sku == sku.Sku && item.Week >= 1 && item.Week <= horizonWeeks)
                .GroupBy(item => item.Week)
                .ToDictionary(group => group.Key, group => group.ToList());

            for (var week = 1; week <= horizonWeeks; week++)
            {
                if (!skuDemand.TryGetValue(week, out var rows))
                {
                    AddBlockingIssue(issues, "Demand", sku.Sku, week, "MissingDemand", null);
                    continue;
                }

                if (rows.Count > 1)
                {
                    AddBlockingIssue(issues, "Demand", sku.Sku, week, "DuplicateDemand", null);
                }

                if (rows.Any(item => item.BaselineDemand < 0m))
                {
                    AddBlockingIssue(issues, "Demand", sku.Sku, week, "NegativeDemand", null);
                }
            }
        }
    }

    private static void ValidateReceipts(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<SkuBufferSetting> skus,
        int horizonWeeks,
        List<PlanningEvidenceIssue> issues)
    {
        if (data.ConfirmedReceipts is null)
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", null, null, "MissingConfirmedReceipts", null);
        }

        var receipts = data.ConfirmedReceipts ?? Array.Empty<ConfirmedReceiptEvidence>();
        foreach (var duplicate in receipts
                     .Where(item => !string.IsNullOrWhiteSpace(item.ReceiptId))
                     .GroupBy(item => item.ReceiptId, StringComparer.Ordinal)
                     .Where(group => group.Count() > 1))
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", null, null, "DuplicateReceiptId", duplicate.Key);
        }

        var skuCodes = skus.Select(item => item.Sku).ToHashSet(StringComparer.Ordinal);
        foreach (var receipt in receipts)
        {
            ValidateReceiptRow(data, receipt, skuCodes, horizonWeeks, issues);
        }

        var inventory = data.Inventory ?? Array.Empty<InventoryPosition>();
        foreach (var sku in skus)
        {
            var inventoryRows = inventory.Where(item => item.Sku == sku.Sku).ToList();
            if (inventoryRows.Count != 1)
            {
                continue;
            }

            var confirmedTotal = receipts
                .Where(item => item.Sku == sku.Sku)
                .Sum(item => item.Quantity);
            if (Math.Abs(confirmedTotal - inventoryRows[0].OpenSupply) > OpenSupplyTolerance)
            {
                AddBlockingIssue(issues, "ConfirmedReceipt", sku.Sku, null, "OpenSupplyMismatch", null);
            }
        }
    }

    private static void ValidateReceiptRow(
        ScenarioWorkspaceDataSet data,
        ConfirmedReceiptEvidence receipt,
        IReadOnlySet<string> skuCodes,
        int horizonWeeks,
        List<PlanningEvidenceIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(receipt.ReceiptId))
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "MissingReceiptId", null);
        }

        if (!skuCodes.Contains(receipt.Sku))
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "UnknownReceiptSku", receipt.ReceiptId);
        }

        if (receipt.Quantity < 0m)
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "NegativeReceiptQuantity", receipt.ReceiptId);
        }

        if (receipt.ExpectedReceiptWeek < 1)
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "InvalidReceiptWeek", receipt.ReceiptId);
        }

        if (receipt.ExpectedReceiptDate is null)
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "MissingExpectedReceiptDate", receipt.ReceiptId);
        }
        else if (WeekForDate(data.Request.AnchorDate, receipt.ExpectedReceiptDate.Value) != receipt.ExpectedReceiptWeek)
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "ReceiptDateWeekMismatch", receipt.ReceiptId);
        }

        if (!AllowedReceiptTypes.Contains(receipt.ReceiptType))
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "UnsupportedReceiptType", receipt.ReceiptId);
        }

        if (receipt.ConfirmationStatus != "Confirmed")
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "ReceiptNotConfirmed", receipt.ReceiptId);
        }

        if (!IsComplete(receipt.EvidenceStatus))
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "ReceiptEvidenceNotComplete", receipt.ReceiptId);
        }

        if (string.IsNullOrWhiteSpace(receipt.SourceReference) ||
            string.IsNullOrWhiteSpace(receipt.SupplySourceType) ||
            string.IsNullOrWhiteSpace(receipt.EvidenceLabel))
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "IncompleteReceiptEvidence", receipt.ReceiptId);
        }

        if (!IsTimestamp(receipt.AsOfUtc))
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "InvalidAsOfUtc", receipt.ReceiptId);
        }

        ValidateSourceTimestamp(receipt, issues);
        ValidateSupplierMapping(data, receipt, horizonWeeks, issues);

        if (receipt.ExpectedReceiptWeek > horizonWeeks)
        {
            issues.Add(new PlanningEvidenceIssue(
                "ConfirmedReceipt",
                receipt.Sku,
                receipt.ExpectedReceiptWeek,
                "OutsideCoverage",
                receipt.ReceiptId,
                BlocksFreeze: false,
                BlocksProjection: false));
        }
    }

    private static void ValidateSourceTimestamp(
        ConfirmedReceiptEvidence receipt,
        List<PlanningEvidenceIssue> issues)
    {
        if (receipt.SourceTimestampUtc is null)
        {
            return;
        }

        DateOnly businessDate;
        try
        {
            businessDate = BusinessDateForSourceTimestamp(receipt.SourceTimestampUtc);
        }
        catch (FormatException)
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "InvalidSourceTimestampUtc", receipt.ReceiptId);
            return;
        }

        if (receipt.ExpectedReceiptDate is not null && businessDate != receipt.ExpectedReceiptDate.Value)
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "SourceTimestampDateMismatch", receipt.ReceiptId);
        }
    }

    private static void ValidateSupplierMapping(
        ScenarioWorkspaceDataSet data,
        ConfirmedReceiptEvidence receipt,
        int horizonWeeks,
        List<PlanningEvidenceIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(receipt.Supplier))
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "MissingSupplier", receipt.ReceiptId);
        }

        if (string.IsNullOrWhiteSpace(receipt.MaterialFamily))
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "MissingMaterialFamily", receipt.ReceiptId);
        }

        if (string.IsNullOrWhiteSpace(receipt.Supplier) || string.IsNullOrWhiteSpace(receipt.MaterialFamily))
        {
            return;
        }

        var hasItemSource = (data.SupplierItemSources ?? Array.Empty<SupplierItemSource>()).Any(item =>
            item.Sku == receipt.Sku &&
            item.Supplier == receipt.Supplier &&
            item.MaterialFamily == receipt.MaterialFamily);
        if (!hasItemSource)
        {
            AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "MissingSupplierItemSource", receipt.ReceiptId);
        }

        if (receipt.ExpectedReceiptWeek is >= 1 && receipt.ExpectedReceiptWeek <= horizonWeeks)
        {
            var hasCapacity = (data.SupplierCapacityWindows ?? Array.Empty<SupplierCapacityWindow>()).Any(item =>
                item.Supplier == receipt.Supplier &&
                item.MaterialFamily == receipt.MaterialFamily &&
                item.Week == receipt.ExpectedReceiptWeek);
            if (!hasCapacity)
            {
                AddBlockingIssue(issues, "ConfirmedReceipt", receipt.Sku, receipt.ExpectedReceiptWeek, "MissingSupplierCapacityWindow", receipt.ReceiptId);
            }
        }
    }

    private static void ValidateDdmrpParameters(
        ScenarioWorkspaceDataSet data,
        IReadOnlyList<SkuBufferSetting> skus,
        int horizonWeeks,
        List<PlanningEvidenceIssue> issues)
    {
        var parameters = data.DdmrpParameters ?? Array.Empty<DdmrpParameterProfile>();
        foreach (var sku in skus)
        {
            var rows = parameters.Where(item => item.Sku == sku.Sku).ToList();
            if (rows.Count == 0)
            {
                AddBlockingIssue(issues, "DdmrpParameters", sku.Sku, null, "MissingDdmrpParameters", null);
                continue;
            }

            if (rows.Count > 1)
            {
                AddBlockingIssue(issues, "DdmrpParameters", sku.Sku, null, "DuplicateDdmrpParameters", null);
            }

            if (rows.Any(item => !HasCompleteDdmrpParameters(item, horizonWeeks)))
            {
                AddBlockingIssue(issues, "DdmrpParameters", sku.Sku, null, "IncompleteDdmrpParameters", null);
            }
        }
    }

    private static bool HasCompleteDdmrpParameters(DdmrpParameterProfile item, int horizonWeeks)
    {
        return !string.IsNullOrWhiteSpace(item.DecouplingPoint) &&
               !string.IsNullOrWhiteSpace(item.BufferProfile) &&
               item.Adu > 0m &&
               !string.IsNullOrWhiteSpace(item.AduSource) &&
               item.AduCalculationWindowDays > 0 &&
               item.DecoupledLeadTimeDays > 0 &&
               !string.IsNullOrWhiteSpace(item.DltSource) &&
               item.VariabilityFactor > 0m &&
               item.DemandAdjustmentFactor > 0m &&
               item.ZoneAdjustmentFactor > 0m &&
               item.MinimumOrderQuantity >= 0m &&
               item.OrderCycleDays > 0 &&
               item.UnitCost >= 0m &&
               item.WeeklyCapacityUnits > 0m &&
               item.TopOfRed > 0m &&
               item.TopOfYellow > item.TopOfRed &&
               item.TopOfGreen > item.TopOfYellow &&
               item.EffectiveFromWeek <= 1 &&
               item.EffectiveThroughWeek >= horizonWeeks &&
               !string.IsNullOrWhiteSpace(item.ParameterStatus) &&
               item.CompletenessStatus == "Complete" &&
               item.LeadTimeFactor is > 0m and <= 1m &&
               !string.IsNullOrWhiteSpace(item.ParameterSnapshotId) &&
               item.EvidenceStatus == "Complete" &&
               item.Sizing is not null &&
               item.Sizing.EvidenceStatus == "Complete" &&
               item.SizingLines is { Count: > 0 };
    }

    private static bool IsComplete(string? evidenceStatus) => evidenceStatus == "Complete";

    private static bool IsTimestamp(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               DateTimeOffset.TryParse(
                   value,
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                   out _);
    }

    private static void AddBlockingIssue(
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
            BlocksFreeze: true,
            BlocksProjection: true));
    }
}
