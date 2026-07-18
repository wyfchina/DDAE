# DDAE 历史事实、库存缓冲与产能保护修正 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在不触碰外部协议的前提下，让历史经营、库存缓冲、产能保护和当前基线共享一套可复算的内部事实，修正未来库存的双位置语义，并把重复的未来时间缓冲页面合并进统一击穿分析。

**Architecture:** 新增一个 DDAE 内部演示事实源，统一生成 12 个 SKU 的 52 周经营事实、连续库存台账、SKU 级需求序列和基线时点余额；历史、当前基线和未来场景 seed 适配器读取同一个事实集。后端继续承担 DDMRP、库存恒等式、证据完整性、在手库存位置和保护分析；前端只选择、聚合和绘制后端事实，不复制业务公式。未来库存主图关联净流量和物理在手两条权威链，未来时间缓冲独立页的唯一证据并入统一击穿分析。

**Tech Stack:** .NET 9 / C#、ASP.NET Core Razor Pages、SQLite JSON、原生 JavaScript、HTML/CSS、手写 SVG、现有控制台测试 harness、Node.js renderer fixtures。

## Global Constraints

- 只修改 `C:\Users\吴一帆\Documents\DDAE`；不修改同级 `DDAE_INTERFACE_CONTRACT` 和 `DDAE-NetworkStructure`。
- 不修改任何 SDBR 字段、状态、ACK、错误码、JSON 形状、端点、样例、fixtures 或契约测试。
- 不新增外部导入、网络评分、DDOM/SDBR 协议、完整执行排程、自动采纳、自动审批、自动生效或自动发布。
- FPGA 只属于独立库存控制点；不进入时间缓冲或能力保护。
- `AV-FPGA-203` 的 `MinimumOrderQuantity` 在历史 V1/V2、当前基线和未来场景统一为 `5`。
- 能力颜色固定为：`0–60%` 绿、`>60–80%` 黄、`>80–100%` 红、`>100%` 深红。
- 产能保护起点为计划可用能力的 `80%`；保护带宽为 `20%`。该参数是当前内部管理规则，不描述为不可变行业常数。
- 产能保护只对具有有效“上游资源 → 后续 CCR”工序顺序证据的上游资源计算；CCR 自身只显示利用率参照。
- 历史缓冲不再使用或展示“目标净流量位置”。为旧内部 JSON 保留的尾部可选字段必须返回 `null`。
- 未来缓冲主图也不展示自定义目标库存点；净流量位置和期末在手库存位置必须分别来自后端规划投影与物理库存投影。
- 删除的只是未来场景中的独立时间缓冲导航和页面；冻结证据、场景输入、后端分析、保存结果、统一击穿分析和历史时间缓冲全部保留。
- 新 record 参数只追加在尾部并提供默认值，保持旧冻结 SQLite JSON 和旧测试构造器可读。
- 不删除或重建 SQLite 表；继续使用现有快照 JSON 和只追加审计链。
- 不引入前端图表依赖；继续使用现有 HTML、CSS 和 SVG。
- `#trace-panel`、白盒追踪、公开演示闭环及其 DOM、按钮、API 和业务用户视图保持原样。
- 每个任务先写失败测试，再做最小实现，再运行完整 harness，最后形成独立提交。

---

## File Map

### Create

- `src/AdaptiveSopDdsop.Web/Domain/InternalDemoOperatingFacts.cs`：统一事实集 header、周库存移动、余额桥接和读取接口。
- `src/AdaptiveSopDdsop.Web/Data/SeedInternalDemoOperatingFactSource.cs`：确定性 12 SKU / 52 周内部演示事实生成器。
- `src/AdaptiveSopDdsop.Web/Domain/CurrentBaselineReconciliation.cs`：历史期末到当前基线的对账 DTO 和纯验证器。

### Modify

- `src/AdaptiveSopDdsop.Web/Data/SeedData.cs`
- `src/AdaptiveSopDdsop.Web/Data/SeedHistoryOperatingFactSource.cs`
- `src/AdaptiveSopDdsop.Web/Data/SeedScenarioWorkspaceDataSource.cs`
- `src/AdaptiveSopDdsop.Web/Data/SeedCurrentBaselineDataSource.cs`
- `src/AdaptiveSopDdsop.Web/Domain/HistoryOperatingFacts.cs`
- `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewModels.cs`
- `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewProjectionBuilder.cs`
- `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewWorkspaceService.cs`
- `src/AdaptiveSopDdsop.Web/Domain/CurrentBaselineService.cs`
- `src/AdaptiveSopDdsop.Web/Domain/DdmrpCalculator.cs`
- `src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs`
- `src/AdaptiveSopDdsop.Web/Domain/BufferTrendWorkspaceService.cs`
- `src/AdaptiveSopDdsop.Web/Program.cs`（只改内部 DI 注册）
- `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`
- `tests/AdaptiveSopDdsop.Tests/Program.cs`
- `tests/AdaptiveSopDdsop.Tests/Js/history-buffer-renderers.fixture.mjs`
- `tests/AdaptiveSopDdsop.Tests/Js/baseline-planning-evidence.fixture.mjs`
- `tests/AdaptiveSopDdsop.Tests/Js/future-buffer-charts.fixture.mjs`
- `tests/AdaptiveSopDdsop.Tests/Js/future-inventory-flow-charts.fixture.mjs`

### Never Modify

- `src/AdaptiveSopDdsop.Web/Domain/DdsopConfigInboundContract.cs`
- `src/AdaptiveSopDdsop.Web/Domain/DdsopRuntimePlanningInputContract.cs`
- `src/AdaptiveSopDdsop.Web/Domain/ProductionInventoryQualityEvidenceContract.cs`
- `src/AdaptiveSopDdsop.Web/Domain/ProductionSupplierIdentitySourceContract.cs`
- `src/AdaptiveSopDdsop.Web/Domain/SdbrExecutionObjectEvidenceContract.cs`
- `src/AdaptiveSopDdsop.Web/Domain/PublicDemoGoldenLoopService.cs`
- `src/AdaptiveSopDdsop.Web/Domain/AdventureWorksProductDemoProfileService.cs`
- `src/AdaptiveSopDdsop.Web/Domain/ContractRepositoryPathResolver.cs`
- `tests/AdaptiveSopDdsop.Tests/Fixtures/*.json`
- `Program.cs` 中 integration-contract 端点块
- `Index.cshtml` 中 white-box/public-demo 区块
- `app.js` 中 white-box/public-demo 请求与渲染函数

---

### Task 1: 建立统一内部事实源并统一 FPGA MOQ

**Files:**
- Create: `src/AdaptiveSopDdsop.Web/Domain/InternalDemoOperatingFacts.cs`
- Create: `src/AdaptiveSopDdsop.Web/Data/SeedInternalDemoOperatingFactSource.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs:886-909`
- Modify: `src/AdaptiveSopDdsop.Web/Data/SeedData.cs:26`
- Modify: `src/AdaptiveSopDdsop.Web/Data/SeedScenarioWorkspaceDataSource.cs:5-63,342-363`
- Modify: `src/AdaptiveSopDdsop.Web/Program.cs:8-17`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**
- Produces: `IInternalDemoOperatingFactSource.Load() -> InternalDemoOperatingFactSet`.
- Produces: one immutable fact set with `FactSetId=DEMO-OPERATING-20260630-V1`.
- Uses `SourceKind=DemoFixture`, `SourceAuthority=DDAE Internal Operating Fact Set`, `HistoryThroughUtc=2026-06-30T00:00:00.0000000+00:00` and `BaselineAsOfUtc=2026-06-30T08:00:00.0000000+00:00`.
- Consumed later by: `SeedHistoryOperatingFactSource` and `SeedCurrentBaselineDataSource`.

- [ ] **Step 1: Register failing fact-set and MOQ tests**

Add these entries to the test registry:

```csharp
("Internal demo fact set conserves 52 weeks for all twelve SKUs", TestInternalDemoFactSetConservesAllSkuInventory),
("Internal demo demand profiles are not proportional copies", TestInternalDemoDemandProfilesAreDistinct),
("Internal demo inventory adjustments belong to named event weeks", TestInternalDemoInventoryAdjustmentsUseNamedEvents),
("Scenario workspace exposes shared fact-set lineage", TestScenarioWorkspaceExposesSharedFactSetLineage),
("FPGA MOQ is five in master and scenario data", TestFpgaMoqIsFiveInMasterAndScenario),
```

Add complete test bodies:

```csharp
static void TestInternalDemoFactSetConservesAllSkuInventory()
{
    var data = SeedData.Create();
    var facts = new SeedInternalDemoOperatingFactSource(data).Load();
    AssertEqual("DEMO-OPERATING-20260630-V1", facts.Header.FactSetId, "fact set id");
    AssertEqual("DemoFixture", facts.Header.SourceKind, "fact source kind");
    AssertEqual(12, facts.InventoryMovements.Select(item => item.Sku).Distinct().Count(), "ledger SKU count");
    AssertEqual(12 * 52, facts.InventoryMovements.Count, "ledger row count");

    foreach (var skuFacts in facts.InventoryMovements.GroupBy(item => item.Sku))
    {
        var ordered = skuFacts.OrderBy(item => item.WeekOffset).ToList();
        AssertEqual(52, ordered.Count, $"{skuFacts.Key} week count");
        for (var index = 0; index < ordered.Count; index++)
        {
            var point = ordered[index];
            AssertEqual(
                decimal.Round(point.OpeningOnHand + point.ActualReceipts - point.ActualConsumption + point.InventoryAdjustment, 2),
                point.EndingOnHand,
                $"{skuFacts.Key}/{point.WeekOffset} inventory conservation");
            if (index > 0)
            {
                AssertEqual(ordered[index - 1].EndingOnHand, point.OpeningOnHand,
                    $"{skuFacts.Key}/{point.WeekOffset} opening continuity");
            }
        }
    }
}

static void TestInternalDemoDemandProfilesAreDistinct()
{
    var facts = new SeedInternalDemoOperatingFactSource(SeedData.Create()).Load();
    var com = facts.InventoryMovements.Where(item => item.Sku == "AV-COM-201")
        .OrderBy(item => item.WeekOffset).Select(item => item.ActualDemand).ToList();
    var obc = facts.InventoryMovements.Where(item => item.Sku == "AV-OBC-202")
        .OrderBy(item => item.WeekOffset).Select(item => item.ActualDemand).ToList();
    var ratios = com.Zip(obc, (left, right) => right == 0m ? 0m : decimal.Round(left / right, 3)).Distinct().ToList();
    AssertTrue(ratios.Count > 4, "AV-COM and AV-OBC must not be fixed proportional copies");
}

static void TestFpgaMoqIsFiveInMasterAndScenario()
{
    var data = SeedData.Create();
    AssertEqual(5m, data.Skus.Single(item => item.Sku == "AV-FPGA-203").MinimumOrderQuantity, "master FPGA MOQ");
    var scenario = new SeedScenarioWorkspaceDataSource(data)
        .Load(new ScenarioWorkspaceDataRequest(52, new DateOnly(2026, 6, 30)));
    AssertEqual(5m, scenario.Skus.Single(item => item.Sku == "AV-FPGA-203").MinimumOrderQuantity, "scenario FPGA MOQ");
}
```

`TestInternalDemoInventoryAdjustmentsUseNamedEvents` must assert every non-zero `InventoryAdjustment` has a non-empty, non-`NONE` `EventCode`, and every event code/offset belongs to this explicit shared registry:

```text
-46 DEMAND_CHANGE
-39 IMPORT_DELAY
-33 AIT_CAPACITY_LOSS
-29 RECOVERY
-21 DEMAND_PEAK
-16 REWORK
-11 SUPPLY_RECOVERY
-6  TAKT_RECOVERY
```

The shared source owns this registry; history adapters consume its codes/labels instead of maintaining a second private week list for inventory events.

`TestScenarioWorkspaceExposesSharedFactSetLineage` loads the seed scenario adapter and asserts its public result exposes the same fact-set ID, history cutoff and baseline cutoff as the shared header.

- [ ] **Step 2: Run the harness and verify red state**

Run:

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: compilation fails because `SeedInternalDemoOperatingFactSource` and the fact-set records do not exist; the MOQ assertion would also fail with `Expected 5, actual 80` after the types compile.

- [ ] **Step 3: Add exact fact-set records**

Create `InternalDemoOperatingFacts.cs` with these public contracts:

```csharp
namespace AdaptiveSopDdsop.Web.Domain;

public sealed record InternalDemoFactSetHeader(
    string FactSetId,
    string SourceKind,
    string SourceAuthority,
    string HistoryThroughUtc,
    string BaselineAsOfUtc);

public sealed record WeeklyInventoryMovementFact(
    string Sku,
    int WeekOffset,
    decimal OpeningOnHand,
    decimal ActualReceipts,
    decimal ActualDemand,
    decimal ActualConsumption,
    decimal InventoryAdjustment,
    decimal EndingOnHand,
    decimal OpenSupply,
    decimal QualifiedDemand,
    decimal EndingNetFlow,
    decimal DemandSpikeThreshold,
    string EventCode,
    string EvidenceStatus);

public sealed record OperatingBalanceBridgeFact(
    string MetricCode,
    string ItemKey,
    decimal HistoryClosingBalance,
    decimal IntervalIncrease,
    decimal IntervalDecrease,
    decimal Adjustment,
    decimal BaselineBalance,
    string EvidenceStatus);

public sealed record InternalDemoOperatingFactSet(
    InternalDemoFactSetHeader Header,
    IReadOnlyList<WeeklyInventoryMovementFact> InventoryMovements,
    IReadOnlyList<WeeklyOperatingFact> OperatingFacts,
    IReadOnlyList<HistoricalDemandActual> HistoricalDemand,
    IReadOnlyList<InventoryPosition> BaselineInventory,
    IReadOnlyList<OpeningBacklogEvidence> BaselineBacklog,
    decimal BaselineWorkInProcessUnits,
    IReadOnlyList<OperatingBalanceBridgeFact> BalanceBridges);

public interface IInternalDemoOperatingFactSource
{
    InternalDemoOperatingFactSet Load();
}
```

Append compatible public tail fields to `ScenarioWorkspaceDataSet`:

```csharp
string FactSetId = "",
string HistoryThroughUtc = "",
string BaselineAsOfUtc = ""
```

`SeedScenarioWorkspaceDataSource.Load` must set all three from the shared header. Alternate/legacy adapters may leave the defaults empty and must remain readable.

Do not add SDBR, Network or contract types to this file.

- [ ] **Step 4: Implement the deterministic source and scenario adapter**

Create `SeedInternalDemoOperatingFactSource` with these constructors and helpers:

```csharp
public sealed class SeedInternalDemoOperatingFactSource : IInternalDemoOperatingFactSource
{
    private readonly ValidationData _data;
    private readonly Lazy<InternalDemoOperatingFactSet> _facts;

    public SeedInternalDemoOperatingFactSource(ValidationData data)
    {
        _data = data;
        _facts = new Lazy<InternalDemoOperatingFactSet>(Build);
    }

    public InternalDemoOperatingFactSet Load() => _facts.Value;

    private InternalDemoOperatingFactSet Build();
    private IReadOnlyList<WeeklyInventoryMovementFact> BuildInventoryLedger();
    private IReadOnlyList<WeeklyOperatingFact> BuildOperatingFacts(IReadOnlyList<WeeklyInventoryMovementFact> ledger);
    private IReadOnlyList<HistoricalDemandActual> BuildHistoricalDemand(IReadOnlyList<WeeklyInventoryMovementFact> ledger);
    private static decimal RoundQuantity(decimal value) => decimal.Round(Math.Max(0m, value), 2, MidpointRounding.AwayFromZero);
}
```

`BuildInventoryLedger` must use the SKU's stable ordinal in `data.Skus.OrderBy(item => item.Sku)` rather than `string.GetHashCode()`. For chronological ordinal `week=1..52`, store `WeekOffset=week-53` so the facts cover exactly `-52..-1`. For SKU index `i`, use deterministic, non-proportional demand:

```csharp
var phase = i * 2;
var seasonal = 1m
    + 0.16m * (decimal)Math.Sin((week + phase) * Math.PI * 2d / 13d)
    + 0.07m * (decimal)Math.Cos((week * ((i % 4) + 2) + phase) * Math.PI * 2d / 17d);
var spike = (week + i * 3) % 17 == 0 ? 0.42m + (i % 3) * 0.06m : 0m;
var actualDemand = RoundQuantity(sku.Adu * 7m * Math.Max(0.35m, seasonal + spike));
var threshold = RoundQuantity(sku.Adu * 7m * (1.30m + (i % 3) * 0.05m));
```

Treat receipts as recorded historical facts, not generated recommendations. Use cadence `3 + i % 4`; on a receipt week record `sku.Adu * 7 * cadence * (0.94 + (i % 3) * 0.04)`, otherwise zero. Compute available physical stock after the signed adjustment, consume no more than that stock, and roll the ending balance into the next opening:

```csharp
var physicallyAvailable = Math.Max(0m, OpeningOnHand + ActualReceipts + InventoryAdjustment);
ActualConsumption = Math.Min(ActualDemand, physicallyAvailable);
EndingOnHand = OpeningOnHand + ActualReceipts - ActualConsumption + InventoryAdjustment;
EndingNetFlow = EndingOnHand + OpenSupply - QualifiedDemand;
```

Seed initial balances from the backend DDMRP sizing result around the yellow/red boundary; allow receipt weeks to recover temporarily to green. Use only explicit, deterministic adjustments attached to existing named historical event weeks. Build 52 `WeeklyOperatingFact` rows from all 12 SKU ledgers; inventory value is `sum(EndingOnHand × UnitCost)`, not a target-value back-calculation. Build `HistoricalDemandActual.ServiceLevelPercent` as `100 × ActualConsumption / ActualDemand` when demand is positive, and `100` for explicit zero demand.

Build `BalanceBridges` for every SKU on-hand quantity, aggregate inventory value, aggregate WIP, aggregate backlog and every resource's available capacity. The seed fact set must contain a comparable historical closing value for each of these lines and derive its baseline value through explicit interval increase/decrease/adjustment, so every seed difference is exactly zero. `BaselineBacklog` and `BaselineWorkInProcessUnits` must equal the corresponding bridge baseline balances.

Set `AV-FPGA-203` MOQ to `5` in `SeedData.cs`. Update `SeedScenarioWorkspaceDataSource` to accept the shared source:

```csharp
public SeedScenarioWorkspaceDataSource(ValidationData data)
    : this(data, new SeedInternalDemoOperatingFactSource(data)) { }

public SeedScenarioWorkspaceDataSource(
    ValidationData data,
    IInternalDemoOperatingFactSource operatingFacts)
{
    _data = data;
    _operatingFacts = operatingFacts;
}
```

Use `operatingFacts.Load().BaselineInventory` instead of `_data.Inventory`, filtered `HistoricalDemand` instead of `BuildHistoricalDemand`, and shared `BaselineBacklog` instead of the independent backlog generator. Keep demand scenarios, resources, routings and response templates unchanged.

Register one singleton in `Program.cs` and inject it into the internal seed adapters. Do not edit the endpoint block:

```csharp
builder.Services.AddSingleton<IInternalDemoOperatingFactSource>(sp =>
    new SeedInternalDemoOperatingFactSource(sp.GetRequiredService<ValidationData>()));
builder.Services.AddSingleton<IScenarioWorkspaceDataSource>(sp =>
    new SeedScenarioWorkspaceDataSource(
        sp.GetRequiredService<ValidationData>(),
        sp.GetRequiredService<IInternalDemoOperatingFactSource>()));
```

This task registers the shared singleton and scenario adapter. Task 2 must add the history adapter registration using the same singleton, and Task 4 must add the current-baseline adapter registration using that singleton. After Task 4, all three runtime adapters must resolve the same `IInternalDemoOperatingFactSource` instance; no runtime adapter may fall back to its convenience constructor.

- [ ] **Step 5: Verify and commit**

Run:

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Domain/InternalDemoOperatingFacts.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs src/AdaptiveSopDdsop.Web/Data/SeedInternalDemoOperatingFactSource.cs src/AdaptiveSopDdsop.Web/Data/SeedData.cs src/AdaptiveSopDdsop.Web/Data/SeedScenarioWorkspaceDataSource.cs src/AdaptiveSopDdsop.Web/Program.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: unify internal operating facts"
```

Expected: the harness ends with all registered tests passed; no warning or whitespace error appears.

---

### Task 2: 从统一台账生成历史经营和库存缓冲事实

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Data/SeedHistoryOperatingFactSource.cs:6-180,183-289`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryOperatingFacts.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewWorkspaceService.cs:96-145`
- Modify: `src/AdaptiveSopDdsop.Web/Program.cs:8-20` (internal DI registration only)
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**
- Consumes: `IInternalDemoOperatingFactSource` from Task 1.
- Produces: `HistoryFactSet.FactSetId`, `HistoryReviewWorkspace.FactSetId/HistoryThroughUtc`, continuous `WeeklyBufferFact` movements and 12-SKU enterprise operating totals.

- [ ] **Step 1: Replace conflicting historical independence tests with failing lineage tests**

Replace `TestHistoryFactsIgnoreCurrentInventoryPositions`, `TestHistoryFactsIgnoreCurrentSkuUnitCosts` and `TestHistoryTargetNfpDerivesFromParameterEvidence` registry entries with:

```csharp
("History facts use the shared twelve-SKU operating ledger", TestHistoryFactsUseSharedEnterpriseLedger),
("History buffer facts expose continuous stock movement", TestHistoryBufferFactsExposeContinuousMovement),
("History and scenario share the same fact-set cutoff", TestHistoryAndScenarioShareFactSetCutoff),
```

Add:

```csharp
static void TestHistoryFactsUseSharedEnterpriseLedger()
{
    var data = SeedData.Create();
    var shared = new SeedInternalDemoOperatingFactSource(data);
    var source = new SeedHistoryOperatingFactSource(data, shared);
    var facts = source.Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 30)));
    var sharedFacts = shared.Load();
    AssertEqual(sharedFacts.Header.FactSetId, facts.FactSetId, "history fact-set id");
    AssertEqual(52, facts.OperatingFacts.Count, "annual operating count");
    var lastWeek = facts.OperatingFacts.Single(item => item.WeekOffset == -1);
    var expectedInventory = sharedFacts.InventoryMovements
        .Where(item => item.WeekOffset == -1)
        .Join(data.Skus, movement => movement.Sku, sku => sku.Sku,
            (movement, sku) => movement.EndingOnHand * sku.UnitCost).Sum();
    AssertEqual(decimal.Round(expectedInventory, 0), lastWeek.InventoryValue!.Value, "twelve-SKU inventory value");
}

static void TestHistoryBufferFactsExposeContinuousMovement()
{
    var source = new SeedHistoryOperatingFactSource(SeedData.Create());
    var facts = source.Load(new HistoryFactRequest(52, new DateOnly(2026, 6, 30)));
    foreach (var group in facts.BufferFacts.GroupBy(item => item.Sku))
    {
        var ordered = group.OrderBy(item => item.WeekOffset).ToList();
        for (var index = 0; index < ordered.Count; index++)
        {
            var point = ordered[index];
            AssertTrue(point.OpeningOnHand.HasValue && point.ActualReceipts.HasValue &&
                point.ActualConsumption.HasValue && point.InventoryAdjustment.HasValue,
                $"{group.Key}/{point.WeekOffset} movement fields");
            AssertEqual(point.EndingOnHand!.Value,
                point.OpeningOnHand!.Value + point.ActualReceipts!.Value -
                point.ActualConsumption!.Value + point.InventoryAdjustment!.Value,
                $"{group.Key}/{point.WeekOffset} movement equation");
            if (index > 0) AssertEqual(ordered[index - 1].EndingOnHand, point.OpeningOnHand,
                $"{group.Key}/{point.WeekOffset} cross-week continuity");
        }
    }
}
```

`TestHistoryAndScenarioShareFactSetCutoff` must assert history `AsOfUtc` equals the shared header's `HistoryThroughUtc`, and scenario `HistoricalDemand` values equal the same fact-set values for the requested SKUs.

Add an API/workspace lineage assertion: both 6- and 12-month `HistoryReviewWorkspace` results must expose `FactSetId=DEMO-OPERATING-20260630-V1` and `HistoryThroughUtc=2026-06-30T00:00:00.0000000+00:00`.

Revise `TestHistoryMagnitudesReconcileAcrossViews`: compute enterprise inventory value from the shared 12-SKU ledger, retain the net-flow and demand-threshold assertions, and remove the target-NFP sum. Revise `TestHistoryAnnualFactsContainFiftyTwoIrregularWeeks` so the compatibility `TargetNetFlowPosition` property still exists but its value is null on every operating and buffer fact; keep the positive actual-demand and spike-threshold assertions.

- [ ] **Step 2: Run the harness and verify red state**

Run the full test executable. Expected: compile errors for new `WeeklyBufferFact` fields and the new `SeedHistoryOperatingFactSource` constructor.

- [ ] **Step 3: Append movement and lineage fields without breaking old JSON**

Append to `WeeklyBufferFact`:

```csharp
decimal? OpeningOnHand = null,
decimal? ActualReceipts = null,
decimal? ActualConsumption = null,
decimal? InventoryAdjustment = null,
decimal? ActualDemand = null,
string? ParameterChangeReason = null
```

Append to `HistoryFactSet`:

```csharp
string FactSetId = ""
```

Append compatible tail fields to `HistoryReviewWorkspace`:

```csharp
string FactSetId = "",
string HistoryThroughUtc = ""
```

Keep `TargetNetFlowPosition` as an older optional tail property for now, but every newly generated fact must set it to `null`.

- [ ] **Step 4: Adapt history generation to the shared source**

Add constructors:

```csharp
public SeedHistoryOperatingFactSource(ValidationData data)
    : this(data, new SeedInternalDemoOperatingFactSource(data)) { }

public SeedHistoryOperatingFactSource(
    ValidationData data,
    IInternalDemoOperatingFactSource operatingFacts)
{
    _data = data;
    _operatingFacts = operatingFacts;
    _abnormalCosts = BuildAbnormalCosts();
    _capacityProtectionFacts = BuildCapacityProtectionFacts();
}
```

Use the shared `OperatingFacts` for enterprise outcomes. Map only the existing buffered SKU set `AV-COM-201`, `AV-OBC-202`, `AV-FPGA-203`, `TC-MLI-301`, `TC-RAD-302` into `WeeklyBufferFact`; do not turn the other seven SKUs into DDMRP control points.

Map movement fields directly and compute no quantities in the adapter:

```csharp
new WeeklyBufferFact(
    movement.Sku,
    movement.WeekOffset,
    movement.EndingNetFlow,
    BusinessEventLabel(movement.EventCode),
    movement.EvidenceStatus,
    movement.EndingOnHand,
    movement.OpenSupply,
    movement.QualifiedDemand,
    ControlPointFor(movement.Sku),
    SnapshotIdFor(movement.Sku, movement.WeekOffset),
    movement.DemandSpikeThreshold,
    null,
    movement.OpeningOnHand,
    movement.ActualReceipts,
    movement.ActualConsumption,
    movement.InventoryAdjustment,
    movement.ActualDemand,
    ParameterChangeReasonFor(movement.Sku, movement.WeekOffset));
```

Use `2026-06-30` as the shared history cutoff in `HistoryReviewWorkspaceService`, matching the fact-set header and baseline business date. Ensure V1 and V2 parameter snapshots inherit FPGA MOQ `5` from `SeedData`; do not override MOQ inside history code.

Pass `facts.FactSetId` and `facts.AsOfUtc` through the workspace/API result. Register `IHistoryOperatingFactSource` in `Program.cs` with the already-registered shared singleton:

```csharp
builder.Services.AddSingleton<IHistoryOperatingFactSource>(sp =>
    new SeedHistoryOperatingFactSource(
        sp.GetRequiredService<ValidationData>(),
        sp.GetRequiredService<IInternalDemoOperatingFactSource>()));
```

- [ ] **Step 5: Verify and commit**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Data/SeedHistoryOperatingFactSource.cs src/AdaptiveSopDdsop.Web/Domain/HistoryOperatingFacts.cs src/AdaptiveSopDdsop.Web/Domain/HistoryReviewWorkspaceService.cs src/AdaptiveSopDdsop.Web/Program.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: derive history from continuous facts"
```

Expected: all tests pass; 6- and 12-month windows remain distinct because they slice different tails of the same 52-week ledger.

---

### Task 3: 分离参数原因、业务事件和逐项证据

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryOperatingFacts.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewModels.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewProjectionBuilder.cs:111-195,232-303`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**
- Consumes: movement fields from Task 2.
- Produces: `HistoryInventoryPoint.EvidenceChecks`, `WeeklyEvent`, `ParameterChangeReason` and a null target NFP.

- [ ] **Step 1: Add failing projection evidence tests**

Register:

```csharp
("History inventory evidence verifies stock and net-flow equations", TestHistoryInventoryEvidenceChecksBothEquations),
("History projection separates weekly event and parameter reason", TestHistoryProjectionSeparatesReasons),
("History projection does not publish target net-flow position", TestHistoryProjectionOmitsTargetNetFlow),
```

The first test must mutate one `OpeningOnHand` through a test source and assert only the affected point becomes `EvidenceMissing`, with an `InventoryContinuity` check in missing state. The second must assert the V2 transition week has both a weekly event label and a distinct parameter-change reason. Extend `TestHistoryInventoryProjectionUsesRollingSkuDemand` so its rolling ADU expected value is calculated from `WeeklyBufferFact.ActualDemand`; deliberately change `QualifiedDemand` in a test copy and assert zones do not change. The third test is:

```csharp
static void TestHistoryProjectionOmitsTargetNetFlow()
{
    var data = SeedData.Create();
    var review = new HistoryReviewWorkspaceService(
        new SeedHistoryOperatingFactSource(data),
        new SeedScenarioWorkspaceDataSource(data)).GetReview(6);
    AssertTrue(review.InventoryBuffers!.SelectMany(item => item.Points)
        .All(point => point.TargetNetFlowPosition is null),
        "target net-flow position must remain absent");
}
```

Update all existing target-NFP assertions by function name:

- `TestHistoryReviewDoesNotBackfillMissingEvidence`: retain its null assertion.
- `TestHistoryInventoryProjectionUsesRollingSkuDemand`: remove target-NFP constructor mutations and expected-value assertions; keep rolling ADU and zone assertions.
- `TestHistoryReviewPreservesAnnualRollingContextAcrossRanges`: stop comparing target NFP and continue comparing zone, on-hand, NFP, actual demand and threshold facts.
- `TestHistoryVisualRenderersUseBackendEvidence`: remove `point.targetNetFlowPosition` from the required renderer field list.
- `history-buffer-renderers.fixture.mjs`: set all compatibility target fields to null and remove every visible target-series expectation.

- [ ] **Step 2: Run red test**

Expected: compile errors for `EvidenceChecks`, `WeeklyEvent` and poison-source fields; the old projector also returns a non-null target NFP.

- [ ] **Step 3: Add explicit evidence records and compatible tails**

Add:

```csharp
public sealed record HistoryEvidenceCheck(
    string Code,
    string Label,
    string Status,
    string Detail);
```

Append to `HistoricalDdmrpParameterFact`:

```csharp
string? ChangeReason = null
```

Append to `HistoryInventoryPoint` after the existing optional target field:

```csharp
string? WeeklyEvent = null,
string? ParameterChangeReason = null,
IReadOnlyList<HistoryEvidenceCheck>? EvidenceChecks = null,
decimal? OpeningOnHand = null,
decimal? ActualReceipts = null,
decimal? ActualConsumption = null,
decimal? InventoryAdjustment = null
```

- [ ] **Step 4: Build evidence only in the backend projector**

Inside `BuildInventoryPoint`, construct these checks:

```csharp
var checks = new List<HistoryEvidenceCheck>
{
    Check("SourceFields", "数量字段完整", sourceFieldsComplete, "期初、收货、消耗、调整、期末、开放供应和合格需求"),
    Check("InventoryContinuity", "跨周库存连续", continuityValid, "本周期初等于上周期末"),
    Check("InventoryEquation", "库存结转恒等式", inventoryEquationValid, "期末=期初+收货-消耗+调整"),
    Check("NetFlowEquation", "净流量恒等式", netFlowEquationValid, "净流量=期末在手+开放供应-合格需求"),
    Check("ParameterSnapshot", "唯一参数快照", snapshots.Count == 1, fact.ParameterSnapshotId ?? "快照缺失"),
    Check("Sizing", "定容可复算", sizing is not null, sizing is null ? "定容证据缺失" : "后端定容完成"),
    Check("DemandEvidence", "需求与尖峰阈值", actualDemand.HasValue && demandSpikeThreshold.HasValue, "SKU级历史需求事实"),
};
```

`Check` returns `Complete` or `EvidenceMissing`; the point is complete only when every check is complete. For the earliest retained annual point, `InventoryContinuity` validates its explicit historical opening evidence; every later point compares its opening to the immediately preceding ending. Set `ActualDemand` from `fact.ActualDemand`, never from `QualifiedDemand`. `CalculateRollingHistoricalAdu` must use the explicit `ActualDemand` series as its sole demand magnitude input; `QualifiedDemand` remains only the subtraction term in the net-flow equation. Remove the midpoint target calculation entirely and always pass `null` to `TargetNetFlowPosition`. Use `fact.ExplicitCause` for `WeeklyEvent`, and the matched snapshot's `ChangeReason` for `ParameterChangeReason`. Do not treat “无事件” as parameter-change evidence.

- [ ] **Step 5: Verify and commit**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Domain/HistoryOperatingFacts.cs src/AdaptiveSopDdsop.Web/Domain/HistoryReviewModels.cs src/AdaptiveSopDdsop.Web/Domain/HistoryReviewProjectionBuilder.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: expose historical evidence checks"
```

Expected: poisoned continuity fails explicitly; ordinary points remain complete; no target NFP is produced.

---

### Task 4: 建立历史期末到当前基线的余额对账

**Files:**
- Create: `src/AdaptiveSopDdsop.Web/Domain/CurrentBaselineReconciliation.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/CurrentBaselineService.cs:19-70,96-160`
- Modify: `src/AdaptiveSopDdsop.Web/Data/SeedCurrentBaselineDataSource.cs:5-138,163-284`
- Modify: `src/AdaptiveSopDdsop.Web/Program.cs:12-18`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**
- Consumes: shared fact header, baseline inventory and balance bridges.
- Produces: optional `CurrentBaselinePayload.HistoryReconciliation` and required evidence section `HISTORY_RECONCILIATION`.

- [ ] **Step 1: Add failing reconciliation and freeze tests**

Register:

```csharp
("Current baseline reconciles from the same historical fact set", TestCurrentBaselineReconcilesHistoryClosingBalances),
("Current baseline blocks missing history reconciliation", TestCurrentBaselineBlocksMissingHistoryReconciliation),
("Current baseline blocks unbalanced history reconciliation", TestCurrentBaselineBlocksUnbalancedHistoryReconciliation),
```

The happy-path test must assert 12 SKU quantity lines plus exactly one aggregate inventory-value line, one aggregate WIP line, one aggregate backlog line and one line for every resource's available capacity. It must assert identical fact-set ID, `HistoryThroughUtc < BaselineAsOfUtc`, every required metric code, `Difference=0`, and section completeness. The two blocking tests use `FixedCurrentBaselineDataSource` with `HistoryReconciliation=null` or one line changed by `+1m`, then assert `CurrentBaselineService.Freeze` throws and creates no snapshot.

- [ ] **Step 2: Run red test**

Expected: compile errors for reconciliation DTOs and payload property.

- [ ] **Step 3: Create exact reconciliation model and validator**

Create:

```csharp
namespace AdaptiveSopDdsop.Web.Domain;

public sealed record BaselineReconciliationLine(
    string MetricCode,
    string ItemKey,
    decimal HistoryClosingBalance,
    decimal IntervalIncrease,
    decimal IntervalDecrease,
    decimal Adjustment,
    decimal BaselineBalance,
    decimal Difference,
    string EvidenceStatus,
    string? DifferenceReason = null);

public sealed record CurrentBaselineHistoryReconciliation(
    string FactSetId,
    string HistoryThroughUtc,
    string BaselineAsOfUtc,
    string ScopeLabel,
    IReadOnlyList<BaselineReconciliationLine> Lines,
    string EvidenceStatus);

public static class CurrentBaselineReconciliation
{
    public static CurrentBaselineHistoryReconciliation Build(InternalDemoOperatingFactSet facts);
    public static IReadOnlyList<string> Validate(CurrentBaselineHistoryReconciliation? reconciliation);
}
```

`Build` maps each `OperatingBalanceBridgeFact` and computes:

```csharp
var expected = line.HistoryClosingBalance + line.IntervalIncrease
    - line.IntervalDecrease + line.Adjustment;
var difference = decimal.Round(line.BaselineBalance - expected, 2);
```

`Validate` rejects null lineage, empty fact-set ID, mismatched cutoff order, zero lines, duplicate `MetricCode/ItemKey`, non-complete lines and `abs(Difference)>0.01m`.

- [ ] **Step 4: Append lineage to baseline JSON and required evidence**

Append to `CurrentBaselinePayload`:

```csharp
CurrentBaselineHistoryReconciliation? HistoryReconciliation = null
```

Inject `IInternalDemoOperatingFactSource` into `SeedCurrentBaselineDataSource`, preserving convenience constructors. Use shared `BaselineInventory`, `BaselineBacklog`, `HistoricalDemand`, `BaselineWorkInProcessUnits` and exact header `BaselineAsOfUtc=2026-06-30T08:00:00.0000000+00:00`; never retain a separate hard-coded timestamp. Allocate the shared aggregate WIP target across existing resource/SKU routings by their positive routing-time weights, then correct the final row for any rounding difference. Build a required `HISTORY_RECONCILIATION` section with one `BaselineEvidenceItem` per line. The section source must be `DDAE Internal Operating Fact Set`, evidence label `DemoFixture`, and must block freeze for every unbalanced line.

At the start of `CurrentBaselineService.Freeze`, after candidate retrieval and before SQLite connection creation, call:

```csharp
var reconciliationIssues = CurrentBaselineReconciliation.Validate(candidate.Payload.HistoryReconciliation);
if (reconciliationIssues.Count > 0)
{
    throw new ArgumentException(
        $"历史期末与当前基线对账失败：{string.Join("; ", reconciliationIssues)}。",
        nameof(request));
}
```

Build reconciliation lines for all 12 SKU quantities, aggregate inventory value, aggregate WIP, aggregate backlog and each resource's available capacity. When a metric has no comparable historical closing fact, use `EvidenceMissing` with a reason; do not invent zero and do not mark the candidate complete.

Update the runtime registration so `ICurrentBaselineDataSource` receives the same shared singleton and already-registered scenario adapter:

```csharp
builder.Services.AddSingleton<ICurrentBaselineDataSource>(sp =>
    new SeedCurrentBaselineDataSource(
        sp.GetRequiredService<ValidationData>(),
        sp.GetRequiredService<IScenarioWorkspaceDataSource>(),
        sp.GetRequiredService<IInternalDemoOperatingFactSource>()));
```

After this registration, add `TestRuntimeSeedRegistrationsUseSharedFactSource`. It reads the real `src/AdaptiveSopDdsop.Web/Program.cs` and asserts the three runtime registrations for `IScenarioWorkspaceDataSource`, `IHistoryOperatingFactSource` and `ICurrentBaselineDataSource` each resolve `IInternalDemoOperatingFactSource`; it must not build a copied test-only service collection. The behavioral half compares the public results from the three seed adapters: `ScenarioWorkspaceDataSet`, `HistoryReviewWorkspace` and `CurrentBaselineCandidate` must expose the same fact-set ID and exact header cutoffs. Task 11 repeats this comparison through the real runtime APIs.

- [ ] **Step 5: Verify and commit**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Domain/CurrentBaselineReconciliation.cs src/AdaptiveSopDdsop.Web/Domain/CurrentBaselineService.cs src/AdaptiveSopDdsop.Web/Data/SeedCurrentBaselineDataSource.cs src/AdaptiveSopDdsop.Web/Program.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: reconcile history into current baseline"
```

Expected: complete seed candidate freezes; missing or unbalanced lineage is rejected before persistence.

---

### Task 5: 将产能保护从经营 KPI 改为能力约束证据

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewWorkspaceService.cs:3-78,188-240,322-379`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewModels.cs`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js` (`renderHistoryReview` operating KPI list only)
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**
- Consumes: existing `HistoryCapacityPoint.Measure` produced by backend `CapacityProtectionMath`.
- Produces: `HistoryCapacityProtectionSummary` with average, minimum and exception-week metrics.

- [ ] **Step 1: Add failing capacity summary tests**

Register:

```csharp
("History operating outcomes no longer publish remaining protection", TestHistoryOperatingOutcomesDoNotOwnProtection),
("History capacity summary exposes balance minimum exhaustion and overload", TestHistoryCapacitySummaryExposesProtectionRisk),
```

Use the existing AIT/HARNESS seed. Assert average protection band `31.5`, average unused protection `29.2`, balance `92.7%`, at least one exhausted week, at least one overload week, and minimum balance `0%`. Assert `OperatingOutcomes.RemainingProtectionPercent` is null. Add a static renderer assertion that the operating-result KPI list contains no `剩余保护` label and does not read `outcomes.remainingProtectionPercent`.

Update `TestHistoryCapacitySummariesAverageWeeklyProtectionConsumption` to compare its calculated weekly averages with `review.CapacityProtectionSummary`, not `review.OperatingOutcomes`. Keep the layer-average checks because the evidence table still exposes them.

- [ ] **Step 2: Run red test**

Expected: `HistoryCapacityProtectionSummary` is missing and the operating outcome still returns `92.7`.

- [ ] **Step 3: Add the summary DTO as a compatible workspace tail**

```csharp
public sealed record HistoryCapacityProtectionSummary(
    string ResourceCode,
    string? ProtectedCcrResourceCode,
    decimal? AverageProtectionBand,
    decimal? AverageUnusedProtection,
    decimal? BalancePercent,
    decimal? MinimumBalancePercent,
    int ExhaustedWeekCount,
    int OverloadWeekCount,
    string EvidenceStatus);
```

Append to `HistoryReviewWorkspace`:

```csharp
HistoryCapacityProtectionSummary? CapacityProtectionSummary = null
```

Retain the older `HistoryOperatingOutcomes.RemainingProtectionPercent` property for JSON compatibility but set it to null and exclude it from the outcome completeness decision.

Remove the “剩余保护” item entirely from the historical operating-result KPI renderer. Do not replace it with a missing-value card. Its successor metrics are rendered only in the capacity-constraint view in Task 7.

- [ ] **Step 4: Build the summary from complete weekly upstream measures**

For the selected valid upstream relationship only:

```csharp
AverageProtectionBand = Average(point.Measure.ProtectionCapacity);
AverageUnusedProtection = Average(point.Measure.RemainingProtection);
BalancePercent = Sum(point.Measure.RemainingProtection) * 100m
    / Sum(point.Measure.ProtectionCapacity);
MinimumBalancePercent = Min(point.Measure.RemainingProtection * 100m
    / point.Measure.ProtectionCapacity);
ExhaustedWeekCount = Count(point.Measure.RemainingProtection == 0m);
OverloadWeekCount = Count(point.Measure.Overload > 0m);
```

Round capacity quantities and percentages to one decimal. Missing planned capacity, relationship evidence or weekly measure yields `EvidenceMissing`; never substitute zero.

- [ ] **Step 5: Verify and commit**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Domain/HistoryReviewWorkspaceService.cs src/AdaptiveSopDdsop.Web/Domain/HistoryReviewModels.cs src/AdaptiveSopDdsop.Web/wwwroot/js/app.js tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: expose upstream protection balance evidence"
```

---

### Task 6: 修正历史库存图、SKU 波动和证据详情

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml:141-189`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js:23-27,4327-4465,4529-4767,4839-4873,6364-6407`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css:1660-1749,1809-1815`
- Modify: `tests/AdaptiveSopDdsop.Tests/Program.cs:5856-6171`
- Modify: `tests/AdaptiveSopDdsop.Tests/Js/history-buffer-renderers.fixture.mjs`

**Interfaces:**
- Consumes: `HistoryInventoryPoint` evidence fields from Task 3.
- Produces: one selectable weekly evidence detail and a comparable control-point demand axis.

- [ ] **Step 1: Make renderer fixtures fail on the new semantics**

Update the fixture DTO to contain two SKUs under one control point with different demand shapes, shared axis range, distinct weekly events and evidence checks. Add assertions:

```js
const comAxisMaximum = runtime.elements.get("history-inventory-volatility-chart")
  .innerHTML.match(/data-history-demand-axis-max="([^"]+)"/)[1];
const comDemandPath = runtime.elements.get("history-inventory-volatility-chart")
  .innerHTML.match(/class="history-demand-area"[^>]*d="([^"]+)"/)[1];
clickHistorySelector(runtime, "[data-history-inventory-sku]", { historyInventorySku: "AV-OBC-202" });
const obcChart = runtime.elements.get("history-inventory-volatility-chart").innerHTML;
const obcAxisMaximum = obcChart.match(/data-history-demand-axis-max="([^"]+)"/)[1];
const obcDemandPath = obcChart.match(/class="history-demand-area"[^>]*d="([^"]+)"/)[1];
const evidenceDetail = runtime.elements.get("history-inventory-evidence-detail").innerHTML;
assert.ok(!positionChart.includes("目标净流量"));
assert.ok(!positionChart.includes("is-target-nfp"));
assert.ok(positionChart.includes("期末在手库存"));
assert.ok(volatilityChart.includes("AV-COM-201"));
assert.ok(volatilityChart.includes("星载通信机"));
assert.equal(comAxisMaximum, obcAxisMaximum);
assert.notEqual(comDemandPath, obcDemandPath);
assert.ok(evidenceDetail.includes("库存结转恒等式"));
assert.ok(evidenceDetail.includes("参数变更原因"));
```

Update `TestHistoryVisualRenderersUseBackendEvidence` to assert the script reads `point.evidenceChecks` and does not contain the midpoint target formula. Update the selectable-workspace test to require one `history-inventory-evidence-detail` DOM ID.

- [ ] **Step 2: Run the Node fixture and full harness in red state**

```powershell
node .\tests\AdaptiveSopDdsop.Tests\Js\history-buffer-renderers.fixture.mjs
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: fixture fails because the target line remains, the title omits SKU and the evidence host does not exist.

- [ ] **Step 3: Add one evidence-detail host and state field**

After `#history-inventory-chart`, add:

```html
<section id="history-inventory-evidence-detail" class="history-inventory-evidence-detail" aria-live="polite">
    <div class="table-empty"><strong>请选择具有完整证据的历史周</strong></div>
</section>
```

Add `selectedHistoryInventoryWeekOffset` to state. `syncHistorySelectionState` defaults it to the selected SKU's latest valid week. Control-point or SKU change resets it; `[data-history-inventory-week]` updates it and re-renders only the inventory buffer view.

- [ ] **Step 4: Remove target NFP and render backend evidence**

In `renderHistoryInventoryPositionChart`:

- remove target value from the scale;
- remove target segments, paths, markers and gap checks;
- remove the target legend and table column;
- rename visible “期末现有量” to “期末在手库存”;
- add `开放供应` and `合格需求` table columns;
- add `data-history-inventory-week` to each row.

Add:

```js
function historyDemandAxisMaximum(history, controlPoint) {
  const values = (history.inventoryBuffers || [])
    .filter(item => item.controlPoint === controlPoint)
    .flatMap(item => (item.points || []).flatMap(point => [point.actualDemand, point.demandSpikeThreshold]))
    .filter(isFiniteHistoryValue)
    .map(Number);
  return values.length ? Math.ceil(Math.max(...values) * 1.08) : null;
}

function renderHistoryInventoryEvidenceDetail(history, item, point) {
  const host = byId("history-inventory-evidence-detail");
  if (!point) {
    host.innerHTML = `<div class="table-empty"><strong>证据缺失</strong></div>`;
    return;
  }
  const checks = (point.evidenceChecks || []).map(check => `
    <li class="history-evidence-check ${check.status === "Complete" ? "is-complete" : "is-missing"}">
      <strong>${escapeHtml(check.label)}</strong><span>${escapeHtml(check.detail)}</span>
    </li>`).join("");
  host.innerHTML = `<div class="history-chart-heading"><strong>${escapeHtml(point.periodStartDate)} · ${escapeHtml(item.sku)}</strong><span>${escapeHtml(evidenceStatusLabel(point.evidenceStatus))}</span></div>
    <dl class="history-zone-metadata"><div><span>参数快照</span><strong>${escapeHtml(point.parameterSnapshotId || "证据缺失")}</strong></div><div><span>当周事件</span><strong>${escapeHtml(point.weeklyEvent || "无事件")}</strong></div><div><span>参数变更原因</span><strong>${escapeHtml(point.parameterChangeReason || "本周无参数变更")}</strong></div></dl>
    <ul class="history-evidence-check-list">${checks}</ul>`;
}
```

Pass `historyDemandAxisMaximum(history, item.controlPoint)` into the volatility renderer; set `data-history-demand-axis-max` on its SVG and include control point, SKU, name and observed weeks in the title. Expand `renderHistoryDdmrpSizingTrace` with MOQ, DAF, zone adjustment, ADU/DLT sources, change reason and effective weeks; keep formulas from `item.sizingLines` only.

Remove target-NFP CSS selectors and add evidence-detail/check styles. Escape all backend text.

- [ ] **Step 5: Verify and commit**

```powershell
node .\tests\AdaptiveSopDdsop.Tests\Js\history-buffer-renderers.fixture.mjs
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests/Program.cs tests/AdaptiveSopDdsop.Tests/Js/history-buffer-renderers.fixture.mjs
git commit -m "feat: clarify historical inventory evidence"
```

Expected: SKU title and curve change together; same-control-point axis stays equal; target NFP is absent; missing evidence is explicit.

---

### Task 7: 用双联产能缓冲图替换旧期间能力图

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml:226-267,406`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js:4567-4586,5087-5297`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css:1541-1581,1707-1788,1809-1815`
- Modify: `tests/AdaptiveSopDdsop.Tests/Program.cs:1022-1050,5856-6171`
- Modify: `tests/AdaptiveSopDdsop.Tests/Js/history-buffer-renderers.fixture.mjs`

**Interfaces:**
- Consumes: weekly `point.measure.utilizationPercent` and `history.capacityProtectionSummary`.
- Produces: one history-only composite visualization; leaves future scenario distribution unchanged.

- [ ] **Step 1: Add failing DOM and renderer assertions**

Reverse the old static assertion so these IDs must not exist:

```text
history-capacity-band-distribution
history-capacity-utilization-distribution
```

Require:

```text
history-capacity-protection-kpis
history-capacity-buffer-chart
history-capacity-period-observations
history-capacity-empirical-distribution
```

Fixture assertions must find all four fixed zone labels, weekly utilization observations, an average marker, a peak marker, an empirical curve and risk notes. It must not find old theoretical/standard/demonstrated/planned paths inside the main SVG. It must still find those values in the evidence table.

- [ ] **Step 2: Run red tests**

Run the renderer fixture and full harness. Expected: old distribution DOM exists; the new KPI and composite subpanels are absent.

- [ ] **Step 3: Change only the history DOM**

Delete the `#history-capacity-band-distribution` wrapper from `Index.cshtml`. Keep the upstream and CCR cards. Add before the pair:

```html
<div id="history-capacity-protection-kpis" class="rccp-kpis" aria-label="上游产能保护期间指标"></div>
```

Keep `#history-capacity-buffer-chart` as the stable outer host. Do not delete `renderCapacityBandDistribution` or shared `.history-capacity-distribution` CSS because the future scenario page still uses them.

- [ ] **Step 4: Render the selected upstream resource as two coordinated panels**

Extract one resolver used by the pair cards, KPI cards and chart:

```js
function resolveHistoryCapacityPair(history) {
  const upstream = (history.capacityBuffers || []).find(item => item.relationshipRole === "UpstreamProtection") || null;
  const ccr = upstream?.protectedCcrResourceCode
    ? (history.capacityBuffers || []).find(item => item.resourceCode === upstream.protectedCcrResourceCode) || null
    : null;
  return { upstream, ccr };
}
```

Render KPIs using backend summary labels:

```text
上游保护带余额率
最低余额率
保护耗尽周数
超载周数
```

Replace `renderHistoryCapacityBuffer` with:

```js
function buildHistoryCapacityFrequency(values, axisMaximum, binWidth = 10) {
  const binCount = Math.max(1, Math.ceil(axisMaximum / binWidth));
  const bins = Array.from({ length: binCount }, (_, index) => ({
    from: index * binWidth,
    through: (index + 1) * binWidth,
    count: 0,
  }));
  values.forEach(value => {
    const index = Math.min(bins.length - 1, Math.max(0, Math.floor(Number(value) / binWidth)));
    bins[index].count += 1;
  });
  return bins;
}
```

The left SVG uses week on the x-axis and utilization percent on the y-axis. Draw fixed horizontal backgrounds for 0–60, 60–80, 80–100 and 100–axisMaximum, then weekly bars from zero to `measure.utilizationPercent`. Set `axisMaximum=max(120, ceil(peak/10)*10)`. Draw average line and peak marker from the same points.

The right SVG uses utilization percent on the x-axis and bin count on the y-axis. Connect bin-centre/count points with the existing monotone path helper to form an empirical frequency curve; label it as historical frequency, not a probability forecast. Shade the same four vertical bands. Add low-side note “可用于吸收波动或扩大产量的余量” and red/deep-red note “可能成为流程干扰点的风险”.

The detail table retains theoretical, standard, demonstrated, planned and committed values. Rename columns to “保护带宽、已用保护、未用保护余量”. CCR selection remains a utilization reference and never receives protection values.

Add responsive `.history-capacity-composite` grid styles; collapse to one column below 980px. Remove only history-specific old load/multiline selectors.

- [ ] **Step 5: Verify and commit**

```powershell
node .\tests\AdaptiveSopDdsop.Tests\Js\history-buffer-renderers.fixture.mjs
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests/Program.cs tests/AdaptiveSopDdsop.Tests/Js/history-buffer-renderers.fixture.mjs
git commit -m "feat: render historical capacity buffer distribution"
```

Expected: one composite history chart replaces both old charts; future capacity distribution tests remain green.

---

### Task 8: 在当前基线展示历史衔接并修正指标口径

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml:444-469`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js:5453-5616,5665-5680`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css:1607,1810-1875`
- Modify: `tests/AdaptiveSopDdsop.Tests/Program.cs:2406-3159`
- Modify: `tests/AdaptiveSopDdsop.Tests/Js/baseline-planning-evidence.fixture.mjs`

**Interfaces:**
- Consumes: `payload.historyReconciliation` from Task 4.
- Produces: candidate and frozen-snapshot reconciliation cards without changing any baseline API route.

- [ ] **Step 1: Add failing baseline renderer assertions**

Extend the baseline fixture with one complete lineage, one lineage whose line is marked complete but has `difference=1`, and one lineage whose `historyThroughUtc` is later than `baselineAsOfUtc`. Assert:

```js
assert.ok(context.includes("DEMO-OPERATING-20260630-V1"));
assert.ok(context.includes("最近历史截止"));
assert.ok(context.includes("基线截止"));
assert.ok(context.includes("差异 0"));
assert.equal(runtime.elements.get("freeze-current-baseline").disabled, false);
assert.equal(differenceRuntime.elements.get("freeze-current-baseline").disabled, true);
assert.equal(reversedCutoffRuntime.elements.get("freeze-current-baseline").disabled, true);
assert.ok(runtime.elements.get("current-baseline-kpis").innerHTML.includes("截至会前的 52 周滚动实际"));
```

Add static uniqueness checks for `baseline-history-reconciliation` and `baseline-history-reconciliation-body`.

- [ ] **Step 2: Run red fixture and harness**

Expected: missing DOM host and missing lineage rendering; poisoned client candidate is not yet disabled by the UI helper.

- [ ] **Step 3: Add the reconciliation card**

After `#baseline-planning-evidence-context`, add:

```html
<section id="baseline-history-reconciliation" class="stage-card baseline-history-reconciliation" aria-live="polite">
    <div class="panel-heading compact-heading"><div><span class="panel-kicker">历史衔接</span><h2>最近历史期末到会前基线</h2></div></div>
    <div id="baseline-history-reconciliation-summary" class="baseline-evidence-strip"></div>
    <div class="table-scroll"><table class="data-table"><thead><tr><th>指标</th><th>对象</th><th>历史期末</th><th>增加</th><th>减少</th><th>调整</th><th>基线</th><th>差异</th><th>证据</th></tr></thead><tbody id="baseline-history-reconciliation-body"></tbody></table></div>
</section>
```

- [ ] **Step 4: Render and validate candidate lineage**

Add `renderBaselineHistoryReconciliation(baseline)` and call it from both candidate and frozen snapshot render paths. It must only format backend values and must not recompute balances in JS. Extend `baselineCandidateFreezeBlockingIssues` so absent lineage, non-complete status, any non-complete line, any non-finite or `abs(difference)>0.01`, an invalid timestamp, or `historyThroughUtc >= baselineAsOfUtc` blocks the client button, matching backend enforcement. This client check is a safety mirror only; the backend remains authoritative.

Change KPI subtitles:

- service level: `截至会前的 52 周滚动实际`;
- inventory: `会前截止时点余额`;
- WIP: use backend source label and display `演示推导值` only if the evidence source says it is derived;
- peak load: `冻结计划输入的展望期峰值`, not a point-in-time balance.

Show fact set, history cutoff, baseline cutoff, scope, line count and reconciliation status. Escape all source labels and difference reasons. Frozen snapshots with legacy null lineage display “旧版本未保存历史衔接证据” and remain immutable; they are not silently backfilled.

- [ ] **Step 5: Verify and commit**

```powershell
node .\tests\AdaptiveSopDdsop.Tests\Js\baseline-planning-evidence.fixture.mjs
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests/Program.cs tests/AdaptiveSopDdsop.Tests/Js/baseline-planning-evidence.fixture.mjs
git commit -m "feat: show baseline history reconciliation"
```

---

### Task 9: 在未来库存缓冲中分离规划位置和在手库存位置

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Domain/DdmrpCalculator.cs:54-96`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs:442-504`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/BufferTrendWorkspaceService.cs:35-177,219-247`
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml:776-869`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js:2000-2535`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css` only for the new on-hand line and legend
- Modify: `tests/AdaptiveSopDdsop.Tests/Program.cs`
- Modify: `tests/AdaptiveSopDdsop.Tests/Js/future-buffer-charts.fixture.mjs`
- Modify: `tests/AdaptiveSopDdsop.Tests/Js/future-inventory-flow-charts.fixture.mjs`

**Interfaces:**
- Consumes: the existing backend `InventoryFlowProjectionResult` already built for the same case.
- Produces: optional tail `BufferTrendSeriesPoint.PhysicalPosition` and optional physical KPI fields; old JSON remains readable.
- Keeps: the separate lower physical-inventory conservation chart.

- [ ] **Step 1: Add failing domain tests for the two positions**

Register:

```csharp
("Buffer signal separates planning and physical positions", TestBufferSignalSeparatesPlanningAndPhysicalPositions),
("Buffer signal omits physical position when evidence is missing", TestBufferSignalOmitsPhysicalPositionWhenEvidenceIsMissing),
("Buffer signal can recover NFP before physical receipt", TestBufferSignalShowsNfpRecoveryBeforePhysicalReceipt),
```

The complete-evidence test must compare every `BufferTrendSeriesPoint.PhysicalPosition.EndingOnHand` to the same case/SKU/week `InventoryFlowPoint.EndingOnHand`, compare ending backlog, and verify `OnHandStatus` against the point's own time-phased zones. The missing-evidence test uses the existing legacy/missing-flow path and asserts `PhysicalPosition is null` plus all optional physical KPIs are null, never zero. The long-DLT test must prove at least one week has recovered `EndNetFlowAfterReplenishment` while the physical position is still red or has backlog before the simulated receipt arrives.

Also update `TestBufferTrendWorkspaceSummarizesKpisHeatmapAndDetail`, `TestFutureBufferTrendUsesBackendPeriodSizing` and `TestPhysicalFlowDrivesMetricsAndBudget` to assert that the two positions remain joined by `(CaseId, Sku, Week)` and are not derived from each other.

- [ ] **Step 2: Run the red harness**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj -c Release --no-restore
```

Expected: compile errors for `PhysicalPosition` and physical KPI fields.

- [ ] **Step 3: Add compatible backend position records**

Add a generic classifier while preserving the old method:

```csharp
public static string GetPositionStatus(decimal position, BufferZones zones)
{
    if (position <= zones.TopOfRed) return "Red";
    if (position <= zones.TopOfYellow) return "Yellow";
    if (position <= zones.TopOfGreen) return "Green";
    return "OverTopOfGreen";
}

public static string GetBufferStatus(decimal netFlowPosition, BufferZones zones) =>
    GetPositionStatus(netFlowPosition, zones);
```

Add:

```csharp
public sealed record BufferPhysicalPosition(
    decimal EndingOnHand,
    decimal EndingBacklog,
    string OnHandStatus,
    string EvidenceStatus,
    string Source);
```

Append to `BufferTrendSeriesPoint`:

```csharp
BufferPhysicalPosition? PhysicalPosition = null
```

Append nullable fields to `BufferTrendKpis`:

```csharp
int? OnHandRedSkuCount = null,
int? OnHandYellowSkuCount = null,
int? OnHandStockoutWeekCount = null
```

In `BufferTrendWorkspaceService.Build`, create one unique `(Sku, Week) -> InventoryFlowPoint` map only when the flow status is complete. Populate `PhysicalPosition` from `EndingOnHand` and `EndingBacklog`; classify the on-hand quantity with that same week's `sizing.Zones`. Set `Source="InventoryFlowProjection"` and `EvidenceStatus="Complete"`. If the flow is incomplete, legacy, duplicated or missing for the week, leave the optional position and physical KPIs null. Do not coerce missing evidence to zero.

Make the compatible `Status` field consistently mean the **pre-replenishment NFP status** by classifying `EndNetFlowBeforeReplenishment`. Do not mix pre-replenishment red/yellow with post-replenishment green. Existing `EndNetFlowAfterReplenishment` remains the explicit post-order planning position.

Remove the `(TopOfYellow + TopOfGreen) / 2` target calculation from future buffer construction and set the compatible `TargetInventory` field to `0` only if changing its non-nullable internal DTO would break stored preview JSON; it must not be used in any status, KPI, table or renderer. Prefer changing it to an appended-compatible nullable field only if all existing JSON constructors and persistence tests remain readable.

- [ ] **Step 4: Add failing renderer assertions**

Update both future fixtures to assert:

```text
upper buffer chart contains an on-hand line sourced from physicalPosition.endingOnHand
the on-hand line shares the same week x coordinates and dynamic zones as NFP
missing physical evidence leaves NFP visible and omits the on-hand path
the upper chart has no target-inventory dots or target-inventory legend
the lower inventory-conservation chart still exists and renders receipts, fulfillment and backlog
case, SKU and week-range changes update both charts together
```

Update static UI assertions so labels are exactly:

```text
库存位置
净流量与在手库存
补货前净流量位置（下单判断）
补货后净流量位置（已释放供应）
期末在手库存（执行风险）
```

The KPI strip must distinguish `净流量红区 SKU`, `在手红区 SKU`, `净流量≤0周` and `在手短缺周`. A null physical KPI displays `证据缺失`, not `0`.

- [ ] **Step 5: Implement the UI without frontend business formulas**

Rename the first evidence panel from “缓冲信号 / 净流位置” to “库存位置 / 净流量与在手库存”. In `renderBufferTrendChart`, render the optional on-hand line using only `physicalPosition.endingOnHand`; do not derive it from NFP, orders or receipts. Delete `targetDots`, its legend and all visible target-inventory text. Keep the existing lower `renderInventoryFlowChart` unchanged except terminology needed for consistency.

Update `filterBufferTrendWorkspace` to aggregate the new physical KPI fields from complete `physicalPosition` rows only. If no complete physical rows remain in the filtered scope, all physical KPI fields are null. Status chips and heatmaps continue to show the explicit NFP trigger status unless a new column is visibly labeled “在手状态”.

- [ ] **Step 6: Verify and commit**

```powershell
node .\tests\AdaptiveSopDdsop.Tests\Js\future-buffer-charts.fixture.mjs
node .\tests\AdaptiveSopDdsop.Tests\Js\future-inventory-flow-charts.fixture.mjs
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj -c Release --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Domain/DdmrpCalculator.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs src/AdaptiveSopDdsop.Web/Domain/BufferTrendWorkspaceService.cs src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests/Program.cs tests/AdaptiveSopDdsop.Tests/Js/future-buffer-charts.fixture.mjs tests/AdaptiveSopDdsop.Tests/Js/future-inventory-flow-charts.fixture.mjs
git commit -m "fix: separate future buffer positions"
```

---

### Task 10: 删除未来时间缓冲独立页并保留击穿证据

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml:36-46,975-984`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js:80-115,5815-5962`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css:1621-1632`
- Modify: `tests/AdaptiveSopDdsop.Tests/Program.cs:5635-5835,6899-6960,9587-9614`

**Interfaces:**
- Removes: only `#future-scenario-panel/time-buffer`, `#time-buffer-panel` and its dedicated renderer.
- Keeps: `ExternalTimeDelay`, `TimeBufferResponseAdjustment`, frozen baseline evidence, backend analyzer, comparison/save JSON, unified breach rows and all history time-buffer UI.
- Produces: a time-buffer evidence detail embedded under `#future-scenario-panel/breach-analysis`.

- [ ] **Step 1: Replace the old page-presence test with failing consolidation assertions**

Rename the test registration to:

```csharp
("Future time-buffer evidence is consolidated into breach analysis", TestFutureTimeBufferEvidenceIsConsolidatedIntoBreachAnalysis),
```

The test must assert:

```text
no href or workspace route exists for #future-scenario-panel/time-buffer
no #time-buffer-panel exists
#time-buffer-breach-detail, selector, evidence chip, summary and weekly grid exist inside #variance-panel
renderTimeBufferView does not exist
renderTimeBufferBreachEvidence reads breaches, timeBufferProjection and frozen planningInputs.timeBuffers
renderFutureComparison invokes the consolidated renderer
the frontend contains no penetration calculation
NotApplicable, EvidenceMissing and complete non-breach remain distinct
```

Update hierarchical route expectations and canonical count from 30 to 29. Remove the old page IDs from required-panel lists, while retaining `external-time-control-point` and `external-time-delay-days`. Do not remove any backend time-buffer test registrations.

- [ ] **Step 2: Run the red harness**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj -c Release --no-restore
```

Expected: the old route/page/renderer still exists and the embedded detail does not.

- [ ] **Step 3: Add the consolidated evidence host first**

Immediately below `#future-breach-body` inside `#variance-panel`, add:

```html
<section id="time-buffer-breach-detail" class="breach-evidence-detail" aria-live="polite">
    <div class="panel-heading compact-heading">
        <div><span class="panel-kicker">时间保护证据</span><h2>时间缓冲明细</h2></div>
        <span id="time-buffer-breach-evidence-chip" class="status-chip neutral">等待场景比较</span>
    </div>
    <label class="breach-evidence-selector"><span>方案与控制点</span><select id="time-buffer-breach-select"><option value="">等待后端结果</option></select></label>
    <div id="time-buffer-breach-summary" class="metadata-list"></div>
    <div id="time-buffer-breach-weekly-grid" class="rccp-heatmap table-scroll" aria-label="时间缓冲周度侵入证据"></div>
</section>
```

This is not a route view and must remain nested within the existing breach-analysis page.

- [ ] **Step 4: Move the unique evidence into the consolidated renderer**

Replace `renderTimeBufferView` with `renderTimeBufferBreachEvidence(result, baselineDetail)`. It must:

1. collect only backend `TimeBuffer`/`Time` breaches and `timeBufferProjection` rows;
2. join frozen definitions by `bufferId` for control point, protected activity and buffer days;
3. build selector keys from `case responseId + bufferId`;
4. store `selectedTimeBufferBreachKey` in state, preserve it when it still exists, otherwise select the first complete result; if no result is complete, select the first `NotApplicable` or `EvidenceMissing` result so its state and reason remain visible;
5. show control point, protected activity, buffer days, maximum penetration, earliest red week, consecutive risk, recovery, affected products and evidence status;
6. render only the selected case/control-point weekly matrix from backend `penetrationPercent`, `status` and `cause`;
7. display “不适用” and “证据缺失” explicitly and never compute penetration or state in JavaScript.

Bind `#time-buffer-breach-select` change once in the existing event-registration block; update only `state.selectedTimeBufferBreachKey` and call `renderTimeBufferBreachEvidence(state.futureComparison, state.futureComparisonBaseline)`. Call the renderer from `renderFutureComparison`. Keep all time-buffer rows in `#future-breach-body` and all comparison-card breach counts.

- [ ] **Step 5: Delete only the independent page shell**

Delete:

- the sidebar “时间缓冲” link;
- the `workspaceRoutes` entry for `#future-scenario-panel/time-buffer`;
- the full `#time-buffer-panel` section;
- CSS rules tied only to `#time-buffer-weekly-grid` after moving equivalent presentation under the new ID.

Do not delete or change scenario inputs (`external-time-control-point`, `external-time-delay-days`), `buildScenarioComparisonRequest.timeDelays`, backend models/analyzers, frozen evidence, saved projection JSON, history time-buffer DOM/renderers, or FPGA exclusions.

- [ ] **Step 6: Verify and commit**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj -c Release --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "refactor: consolidate future time buffer evidence"
```

Expected: 29 canonical routes; no independent future time-buffer page; unified breach analysis retains the full time-buffer evidence chain; all backend and history time-buffer tests still pass.

---

### Task 11: 完整回归、运行检查和保护边界审计

**Files:**
- Verify only unless a failing in-scope test requires a focused correction.

**Interfaces:**
- Consumes: all prior tasks.
- Produces: verified DDAE build and evidence that protected repositories/files did not change.

- [ ] **Step 1: Run all executable and renderer tests**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
node .\tests\AdaptiveSopDdsop.Tests\Js\history-buffer-renderers.fixture.mjs
node .\tests\AdaptiveSopDdsop.Tests\Js\baseline-planning-evidence.fixture.mjs
node .\tests\AdaptiveSopDdsop.Tests\Js\future-buffer-charts.fixture.mjs
node .\tests\AdaptiveSopDdsop.Tests\Js\future-inventory-flow-charts.fixture.mjs
node .\tests\AdaptiveSopDdsop.Tests\Js\history-review-loader-race.fixture.mjs
```

Expected: every command exits `0`; the .NET harness ends with `test(s) passed`; each Node fixture prints its passed-group summary.

- [ ] **Step 2: Build the solution**

```powershell
dotnet build .\AdaptiveSopDdsop.sln --no-restore -m:1
```

Expected: `0 Warning(s)` and `0 Error(s)`.

- [ ] **Step 3: Start from the main workspace and inspect runtime APIs**

```powershell
dotnet run --project .\src\AdaptiveSopDdsop.Web\AdaptiveSopDdsop.Web.csproj --no-build --urls http://localhost:55525
```

Verify:

```text
GET http://localhost:55525/api/history-review?trendMonths=6
GET http://localhost:55525/api/history-review?trendMonths=12
GET http://localhost:55525/api/current-baselines/candidate
GET http://localhost:55525/api/scenario-workspace-data?horizonWeeks=52
```

Expected: both history windows use the same fact-set ID and different ranges; scenario workspace, both history responses and current baseline expose `DEMO-OPERATING-20260630-V1`, `2026-06-30T00:00:00.0000000+00:00` and `2026-06-30T08:00:00.0000000+00:00`; FPGA MOQ is 5; target NFP is null; current baseline lineage is complete with zero differences.

- [ ] **Step 4: Perform browser acceptance**

Open `http://localhost:55525/` and verify:

1. “经营结果”没有“剩余保护”。
2. “缓冲表现”显示“期末在手库存”和净流量位置，不显示目标 NFP。
3. 切换同一控制点 SKU 后需求标题、数值和形状改变；轴上界保持可比。
4. FPGA 定容快照显示 MOQ 5，红黄绿由后端重新计算。
5. 周证据详情区分当周事件、参数变更原因和逐项证据。
6. “能力约束”显示保护余额、最低余额、耗尽周、超载周。
7. “期间上游负荷分布”不存在；期间能力为四色观测与经验分布双联图。
8. 当前基线显示历史事实集、两个截止点和余额对账。
9. 未来库存主图同时显示补货前净流量、补货后净流量和期末在手库存；目标库存点不存在，下方库存守恒图仍存在。
10. 未来场景没有独立“时间缓冲”入口；“击穿分析”可查看时间缓冲控制点、保护活动、缓冲天数、最大侵入和周度矩阵。
11. 白盒追踪和公开演示闭环入口、按钮和运行行为不变。

- [ ] **Step 5: Audit repository boundaries and final diff**

```powershell
git diff --check
git status --short
git diff --name-only origin/main -- src/AdaptiveSopDdsop.Web/Domain/DdsopConfigInboundContract.cs src/AdaptiveSopDdsop.Web/Domain/DdsopRuntimePlanningInputContract.cs src/AdaptiveSopDdsop.Web/Domain/ProductionInventoryQualityEvidenceContract.cs src/AdaptiveSopDdsop.Web/Domain/ProductionSupplierIdentitySourceContract.cs src/AdaptiveSopDdsop.Web/Domain/SdbrExecutionObjectEvidenceContract.cs src/AdaptiveSopDdsop.Web/Domain/PublicDemoGoldenLoopService.cs src/AdaptiveSopDdsop.Web/Domain/AdventureWorksProductDemoProfileService.cs src/AdaptiveSopDdsop.Web/Domain/ContractRepositoryPathResolver.cs tests/AdaptiveSopDdsop.Tests/Fixtures
git -C ..\DDAE_INTERFACE_CONTRACT status --short
git -C ..\DDAE-NetworkStructure status --short
```

Expected: `git diff --check` is silent; DDAE worktree is clean after task commits; protected-file diff is empty; CONTRACT shows only its pre-existing untracked handoff if still present; Network is clean.

If an in-scope correction is necessary during this verification task, return to the owning Task 1–10 red/green cycle, add only that task's listed files, use that task's commit command, and then rerun Task 11 Steps 1–5. Do not commit unrelated user changes or files from sibling repositories.
