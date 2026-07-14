using AdaptiveSopDdsop.Web.Domain;

namespace AdaptiveSopDdsop.Web.Data;

public sealed class SeedCurrentBaselineDataSource : ICurrentBaselineDataSource
{
    private readonly ValidationData _data;

    public SeedCurrentBaselineDataSource()
        : this(SeedData.Create())
    {
    }

    public SeedCurrentBaselineDataSource(ValidationData data)
    {
        _data = data;
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
        var planningInputs = new SeedScenarioWorkspaceDataSource(_data).Load(new ScenarioWorkspaceDataRequest(
            52,
            new DateOnly(2026, 6, 1)));
        var payload = new CurrentBaselinePayload(_data.Inventory, inTransit, backlog, wip, supplier, resources, adjustments, _data.MasterSettings, planningInputs);
        var sections = new List<BaselineEvidenceSection>
        {
            Section("INVENTORY", "当前库存与净流量位置", "DDAE Demo Inventory Adapter", asOf, _data.Inventory.Count),
            Section("IN_TRANSIT", "在途与开放供应", "DDAE Demo Supply Adapter", asOf, inTransit.Count),
            Section("BACKLOG", "未结与积压需求", "DDAE Demo Demand Adapter", asOf, backlog.Count),
            Section("WIP", "在制品与控制点队列", "DDAE Demo WIP Evidence", asOf, wip.Count),
            Section("SUPPLIER_COMMITMENTS", "供应商最新承诺", "DDAE Demo Supplier Evidence", asOf, supplier.Count),
            Section("RESOURCE_AVAILABILITY", "资源可用能力", "DDAE Demo Capacity Evidence", asOf, resources.Count),
            Section("TEMPORARY_ADJUSTMENTS", "已生效临时措施", "DDAE Demo Governance", asOf, adjustments.Count, required: false),
            Section("MASTER_SETTINGS", "当前 DDOM 参数版本", "DDAE Governance", asOf, _data.MasterSettings.Count),
            Section("PLANNING_INPUTS", "白盒重算类型化输入", "DDAE Demo Planning Snapshot", asOf,
                planningInputs.Skus.Count + planningInputs.Demand.Count + planningInputs.ResourceRoutings.Count + planningInputs.SupplierItemSources.Count)
        };
        return new CurrentBaselineCandidate("BASE-CANDIDATE-DEMO-20260630", asOf, "DEMO-MS-2026-06", sections, payload, "DemoFixture");
    }

    private static BaselineEvidenceSection Section(string code, string name, string source, string asOf, int count, bool required = true)
    {
        var completeness = count > 0 || !required ? "Complete" : "Missing";
        return new BaselineEvidenceSection(code, name, source, asOf, "Fresh", completeness, count, "DemoFixture", required);
    }
}
