using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.Data;

public sealed class SeedInternalDemoOperatingFactSource : IInternalDemoOperatingFactSource
{
    private const string FactSetId = "DEMO-OPERATING-20260630-V1";
    private const string HistoryThroughUtc = "2026-06-30T00:00:00.0000000+00:00";
    private const string BaselineAsOfUtc = "2026-06-30T08:00:00.0000000+00:00";
    private static readonly IReadOnlyDictionary<int, string> NamedInventoryEvents =
        new Dictionary<int, string>
        {
            [-46] = "DEMAND_CHANGE",
            [-39] = "IMPORT_DELAY",
            [-33] = "AIT_CAPACITY_LOSS",
            [-29] = "RECOVERY",
            [-21] = "DEMAND_PEAK",
            [-16] = "REWORK",
            [-11] = "SUPPLY_RECOVERY",
            [-6] = "TAKT_RECOVERY"
        };

    private readonly ValidationData _data;
    private readonly Lazy<InternalDemoOperatingFactSet> _facts;

    public SeedInternalDemoOperatingFactSource(ValidationData data)
    {
        _data = data;
        _facts = new Lazy<InternalDemoOperatingFactSet>(Build);
    }

    public InternalDemoOperatingFactSet Load() => _facts.Value;

    private InternalDemoOperatingFactSet Build()
    {
        var ledger = BuildInventoryLedger();
        var operatingFacts = BuildOperatingFacts(ledger);
        var historicalDemand = BuildHistoricalDemand(ledger);
        var latestBySku = ledger
            .Where(item => item.WeekOffset == -1)
            .ToDictionary(item => item.Sku, StringComparer.Ordinal);
        var baselineInventory = _data.Inventory
            .OrderBy(item => item.Sku, StringComparer.Ordinal)
            .Select(item =>
            {
                var latest = latestBySku[item.Sku];
                return new InventoryPosition(item.Sku, latest.EndingOnHand, item.OpenSupply, item.QualifiedDemand);
            })
            .ToList();
        var baselineBacklog = _data.Skus
            .OrderBy(item => item.Sku, StringComparer.Ordinal)
            .Select(item => new OpeningBacklogEvidence(
                $"DEMO-OPENING-BACKLOG-{item.Sku}",
                item.Sku,
                item.Sku switch
                {
                    "PAY-SAR-102" => 1m,
                    "AV-FPGA-203" => 2m,
                    "CBL-HAR-402" => 8m,
                    _ => 0m
                },
                $"DEMO-CUSTOMER-BACKLOG-{item.Sku}",
                "Complete",
                BaselineAsOfUtc,
                "DemoFixture"))
            .ToList();
        var baselineWip = operatingFacts.Single(item => item.WeekOffset == -1).WorkInProcessUnits ?? 0m;
        var bridges = BuildBalanceBridges(baselineInventory, baselineBacklog, baselineWip, operatingFacts);

        return new InternalDemoOperatingFactSet(
            new InternalDemoFactSetHeader(
                FactSetId,
                "DemoFixture",
                "DDAE Internal Operating Fact Set",
                HistoryThroughUtc,
                BaselineAsOfUtc),
            ledger,
            operatingFacts,
            historicalDemand,
            baselineInventory,
            baselineBacklog,
            baselineWip,
            bridges);
    }

    private IReadOnlyList<WeeklyInventoryMovementFact> BuildInventoryLedger()
    {
        var baselineBySku = _data.Inventory.ToDictionary(item => item.Sku, StringComparer.Ordinal);
        var ledger = new List<WeeklyInventoryMovementFact>();

        foreach (var (sku, index) in _data.Skus.OrderBy(item => item.Sku, StringComparer.Ordinal).Select((item, ordinal) => (item, ordinal)))
        {
            var demands = new decimal[52];
            var receipts = new decimal[52];
            var adjustments = new decimal[52];
            var eventCodes = new string[52];
            var phase = index * 2;
            var cadence = 3 + index % 4;

            for (var week = 1; week <= 52; week++)
            {
                var seasonal = 1m
                    + 0.16m * (decimal)Math.Sin((week + phase) * Math.PI * 2d / 13d)
                    + 0.07m * (decimal)Math.Cos((week * ((index % 4) + 2) + phase) * Math.PI * 2d / 17d);
                var spike = (week + index * 3) % 17 == 0 ? 0.42m + (index % 3) * 0.06m : 0m;
                demands[week - 1] = RoundQuantity(sku.Adu * 7m * Math.Max(0.35m, seasonal + spike));
                receipts[week - 1] = week % cadence == 0
                    ? RoundQuantity(sku.Adu * 7m * cadence * (0.94m + (index % 3) * 0.04m))
                    : 0m;
                var weekOffset = week - 53;
                eventCodes[week - 1] = NamedInventoryEvents.TryGetValue(weekOffset, out var eventCode)
                    ? eventCode
                    : "NONE";
                adjustments[week - 1] = InventoryAdjustment(sku.Sku, weekOffset, index);
            }

            // Historical receipts are recorded facts.  The opening balance is the amount required to
            // preserve that observed history and reconcile the final historical closing to the frozen baseline.
            var opening = RoundSignedQuantity(
                baselineBySku[sku.Sku].OnHand + demands.Sum() - receipts.Sum() - adjustments.Sum());
            var sizing = DdmrpCalculator.CalculateSizing(sku);
            opening = Math.Max(opening, sizing.Zones.TopOfRed);

            for (var week = 1; week <= 52; week++)
            {
                var offset = week - 53;
                var adjustment = adjustments[week - 1];
                var physicallyAvailable = Math.Max(0m, opening + receipts[week - 1] + adjustment);
                var consumption = Math.Min(demands[week - 1], physicallyAvailable);
                var ending = RoundSignedQuantity(opening + receipts[week - 1] - consumption + adjustment);
                var isLatest = offset == -1;
                var inventory = baselineBySku[sku.Sku];
                var openSupply = isLatest ? inventory.OpenSupply : 0m;
                var qualifiedDemand = isLatest ? inventory.QualifiedDemand : 0m;
                var threshold = RoundQuantity(sku.Adu * 7m * (1.30m + (index % 3) * 0.05m));

                ledger.Add(new WeeklyInventoryMovementFact(
                    sku.Sku,
                    offset,
                    opening,
                    receipts[week - 1],
                    demands[week - 1],
                    consumption,
                    adjustment,
                    ending,
                    openSupply,
                    qualifiedDemand,
                    RoundSignedQuantity(ending + openSupply - qualifiedDemand),
                    threshold,
                    adjustment == 0m ? "NONE" : eventCodes[week - 1],
                    "Complete"));
                opening = ending;
            }
        }

        return ledger;
    }

    private IReadOnlyList<WeeklyOperatingFact> BuildOperatingFacts(IReadOnlyList<WeeklyInventoryMovementFact> ledger)
    {
        var costs = _data.Skus.ToDictionary(item => item.Sku, item => item.UnitCost, StringComparer.Ordinal);
        return ledger
            .GroupBy(item => item.WeekOffset)
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var demand = group.Sum(item => item.ActualDemand);
                var consumption = group.Sum(item => item.ActualConsumption);
                var service = demand > 0m ? decimal.Round(100m * consumption / demand, 2) : 100m;
                var inventoryValue = decimal.Round(group.Sum(item => item.EndingOnHand * costs[item.Sku]), 2);
                var wip = RoundQuantity(group.Sum(item => item.ActualConsumption) * 0.35m);
                return new WeeklyOperatingFact(
                    group.Key,
                    service,
                    inventoryValue,
                    wip,
                    7m,
                    inventoryValue,
                    "Complete",
                    demand,
                    group.Average(item => item.DemandSpikeThreshold),
                    group.Sum(item => item.EndingNetFlow));
            })
            .ToList();
    }

    private IReadOnlyList<HistoricalDemandActual> BuildHistoricalDemand(IReadOnlyList<WeeklyInventoryMovementFact> ledger) =>
        ledger
            .OrderBy(item => item.Sku, StringComparer.Ordinal)
            .ThenBy(item => item.WeekOffset)
            .Select(item => new HistoricalDemandActual(
                item.Sku,
                item.WeekOffset,
                item.ActualDemand,
                item.ActualDemand,
                item.ActualDemand > 0m
                    ? decimal.Round(100m * item.ActualConsumption / item.ActualDemand, 2)
                    : 100m,
                item.EndingNetFlow))
            .ToList();

    private IReadOnlyList<OperatingBalanceBridgeFact> BuildBalanceBridges(
        IReadOnlyList<InventoryPosition> inventory,
        IReadOnlyList<OpeningBacklogEvidence> backlog,
        decimal wip,
        IReadOnlyList<WeeklyOperatingFact> operatingFacts)
    {
        var closingInventoryValue = operatingFacts.Single(item => item.WeekOffset == -1).InventoryValue ?? 0m;
        var bridges = inventory
            .Select(item => BalanceBridge("ON_HAND", item.Sku, item.OnHand))
            .Append(BalanceBridge("INVENTORY_VALUE", "ALL", closingInventoryValue))
            .Append(BalanceBridge("WORK_IN_PROCESS", "ALL", wip))
            .Append(BalanceBridge("BACKLOG", "ALL", backlog.Sum(item => item.Quantity)))
            .Concat(_data.Resources.Select(item =>
                BalanceBridge("RESOURCE_AVAILABLE_CAPACITY", item.Code, item.WeeklyAvailableUnits)))
            .ToList();
        return bridges;
    }

    private static OperatingBalanceBridgeFact BalanceBridge(string metricCode, string itemKey, decimal balance) =>
        new(metricCode, itemKey, balance, 0m, 0m, 0m, balance, "Complete");

    private static decimal InventoryAdjustment(string sku, int weekOffset, int skuIndex) =>
        (weekOffset, sku) switch
        {
            (-46, "AV-COM-201") => -4m,
            (-46, "AV-OBC-202") => -3m,
            (-39, "AV-FPGA-203") => -2m,
            (-33, "SAT-BUS-001") => -2m,
            (-29, "SAT-BUS-001") => 2m,
            (-21, "TC-MLI-301") => -8m,
            (-16, "PAY-EO-101") => -1m,
            (-11, "AV-FPGA-203") => 3m,
            (-6, "CBL-HAR-402") => 12m,
            _ => 0m
        };

    private static decimal RoundQuantity(decimal value) => decimal.Round(Math.Max(0m, value), 2, MidpointRounding.AwayFromZero);

    private static decimal RoundSignedQuantity(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
