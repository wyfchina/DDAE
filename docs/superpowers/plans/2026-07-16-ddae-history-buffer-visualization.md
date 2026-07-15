# DDAE 历史缓冲可视化与标准定容 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在不改变任何外部契约的前提下，用统一的标准 DDMRP 后端定容驱动未来动态缓冲、补货判断和历史定容追溯，并为库存、能力、时间三类历史缓冲增加可核验图形。

**Architecture:** `DdmrpCalculator` 产生唯一权威的 `DdmrpSizingResult`，规划引擎、未来缓冲服务和历史回顾服务只消费该结果。历史数量与历史参数版本只通过 `IHistoryOperatingFactSource` 进入，当前场景源只补充名称等定义信息；Razor 页面和原生 SVG 只负责选择、缩放与绘制，不复制业务公式。

**Tech Stack:** .NET 8、ASP.NET Core Razor Pages、C# records、Microsoft.Data.Sqlite JSON 快照、原生 JavaScript/SVG、CSS、控制台测试项目 `AdaptiveSopDdsop.Tests`。

## Global Constraints

- 不修改 `DDAE_INTERFACE_CONTRACT`。
- 不修改任何 SDBR 字段、状态、ACK、错误码、端点、样例、fixture 或契约回归断言。
- 不修改 `DDAE-NetworkStructure`，也不读取网络评分作为输入。
- 不增加外部导入，不增加前端图表依赖。
- 不自动采纳、批准、生效或发布参数。
- `白盒追踪`和`公开演示闭环`继续作为独立入口，原页面 ID、按钮、API 和受保护 JavaScript 函数保持不变。
- FPGA 只属于`关键进口 FPGA 独立库存控制点`，不得进入时间缓冲或能力缓冲。
- 能力保护只对 CCR 前方且具有历史顺序证据的资源计算；CCR 自身只显示利用率参照。
- 六个月严格使用 26 周，十二个月严格使用 52 周，缺失证据不得按零绘制。
- 旧冻结 SQLite JSON 缺少 `LeadTimeFactor` 时保持可读、字段为空、禁止反算与场景重算；不得更新旧快照。
- 所有新增业务标题、图例、空状态和可访问性文本使用中文；允许 DDAE、DDMRP、DDOM、DDS&OP、ADU、DAF、DLT、MOQ、CCR、SKU。
- 已被契约实现消费的控制点值保持不变；DDAE 页面可把`关键进口 FPGA 库存控制点`显示为`关键进口 FPGA 独立库存控制点`，但不得改变传入契约实现的源值。
- 所有实现遵循 TDD；每个任务结束时完整测试并形成独立提交。

---

## File map

### Create

- `src/AdaptiveSopDdsop.Web/Domain/DdmrpSizingExplanation.cs`：把权威定容结果转换为后端中文解释行。
- `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewModels.cs`：历史库存、时间、能力和定容追溯的内部响应 DTO。
- `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewProjectionBuilder.cs`：把显式历史事实投影成图形数据并执行证据校验。

### Modify

- `src/AdaptiveSopDdsop.Web/Domain/Models.cs`：兼容地增加提前期因子、参数证据和逐期定容结果。
- `src/AdaptiveSopDdsop.Web/Domain/DdmrpCalculator.cs`：实现标准红区分解、三个绿区候选和统一计算入口。
- `src/AdaptiveSopDdsop.Web/Domain/DemandDrivenPlanningEngine.cs`：补货、状态、trace 与图形共享同一期定容。
- `src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs`：扩展内部 DDMRP 档案和缓冲趋势 DTO。
- `src/AdaptiveSopDdsop.Web/Domain/BufferTrendWorkspaceService.cs`：删除重复公式，只映射规划引擎定容结果。
- `src/AdaptiveSopDdsop.Web/Domain/HistoryOperatingFacts.cs`：增加库存分量、时间事实、历史参数和能力保护快照。
- `src/AdaptiveSopDdsop.Web/Data/SeedData.cs`：校准 DemoFixture 的提前期因子、波动因子和快照号。
- `src/AdaptiveSopDdsop.Web/Data/SeedScenarioWorkspaceDataSource.cs`：输出完整的新参数档案和后端定容解释。
- `src/AdaptiveSopDdsop.Web/Data/SeedHistoryOperatingFactSource.cs`：生成严格 52 周的显式历史事实及两个参数版本。
- `src/AdaptiveSopDdsop.Web/Data/SeedCurrentBaselineDataSource.cs`：增加阻断性的 DDMRP 定容证据检查。
- `src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs`：拒绝使用缺少新定容证据的旧冻结版本重算。
- `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewWorkspaceService.cs`：编排历史投影并保持旧聚合字段兼容。
- `src/AdaptiveSopDdsop.Web/Domain/MasterSettingsGovernanceService.cs`：内部参数说明加入提前期因子的新语义。
- `src/AdaptiveSopDdsop.Web/Program.cs`：让历史 seed 与已注册 `ValidationData` 同源；不触碰受保护协议端点块。
- `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`：增加`定容追溯`入口、历史图形容器和独立波动图容器。
- `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`：增加历史选择与 SVG 渲染，拆分未来上下两张图。
- `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`：增加历史可视化、竖向缓冲柱和独立波动图样式。
- `tests/AdaptiveSopDdsop.Tests/Program.cs`：领域、迁移、历史事实、UI、中文和保护边界回归。

### Verify unchanged

- `src/AdaptiveSopDdsop.Web/Domain/DdsopConfigInboundContract.cs`
- `src/AdaptiveSopDdsop.Web/Domain/DdsopRuntimePlanningInputContract.cs`
- `src/AdaptiveSopDdsop.Web/Domain/PublicDemoGoldenLoopService.cs`
- `src/AdaptiveSopDdsop.Web/Domain/AdventureWorksProductDemoProfileService.cs`
- `tests/AdaptiveSopDdsop.Tests/Fixtures/*`
- 上述文件以外由 `scripts/verify-protected-boundaries.ps1` 列出的全部受保护内容。

---

### Task 1: Establish one standard DDMRP sizing result

**Files:**

- Modify: `src/AdaptiveSopDdsop.Web/Domain/Models.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/DdmrpCalculator.cs`
- Create: `src/AdaptiveSopDdsop.Web/Domain/DdmrpSizingExplanation.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Data/SeedData.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Data/SeedScenarioWorkspaceDataSource.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/MasterSettingsGovernanceService.cs`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**

- Produces: `DdmrpCalculator.CalculateSizing(SkuBufferSetting, decimal?) -> DdmrpSizingResult`。
- Produces: `DdmrpSizingExplanation.Build(DdmrpSizingResult) -> IReadOnlyList<BufferSizingLine>`。
- Preserves: `DdmrpCalculator.CalculateZones(SkuBufferSetting) -> BufferZones` as a compatibility wrapper。

- [ ] **Step 1: Replace the old zone test with the standard 80/120/70 test and add validation tests.**

Register these test names at the beginning of `tests/AdaptiveSopDdsop.Tests/Program.cs` and replace `TestBufferZones` with the complete functions below:

```csharp
("Standard DDMRP sizing returns 80 120 70 with an explainable green driver", TestStandardDdmrpSizingReturns80_120_70),
("DDMRP sizing rejects missing or illegal lead-time factors", TestDdmrpSizingRejectsIllegalLeadTimeFactor),
```

```csharp
static void TestStandardDdmrpSizingReturns80_120_70()
{
    var sku = new SkuBufferSetting(
        "DDMRP-EXAMPLE", "标准定容算例", "测试", 10m, 12, 0.33m, 7, 50m, 1m, 100m,
        LeadTimeFactor: 0.5m,
        ParameterSnapshotId: "DDMRP-EXAMPLE-V1",
        ParameterEvidenceStatus: "Complete");

    var sizing = DdmrpCalculator.CalculateSizing(sku);

    AssertEqual(120m, sizing.LeadTimeDemand, "lead-time demand");
    AssertEqual(60m, sizing.RedBase, "red base");
    AssertEqual(19.8m, sizing.RedSafety, "red safety");
    AssertEqual(80m, sizing.Zones.Red, "red zone");
    AssertEqual(120m, sizing.Zones.Yellow, "yellow zone");
    AssertEqual(60m, sizing.GreenLeadTimeCandidate, "green lead-time candidate");
    AssertEqual(50m, sizing.GreenMoqCandidate, "green MOQ candidate");
    AssertEqual(70m, sizing.GreenOrderCycleCandidate, "green order-cycle candidate");
    AssertEqual("OrderCycle", sizing.GreenDriver, "green driver");
    AssertEqual(70m, sizing.Zones.Green, "green zone");
    AssertEqual(80m, sizing.Zones.TopOfRed, "top of red");
    AssertEqual(200m, sizing.Zones.TopOfYellow, "top of yellow");
    AssertEqual(270m, sizing.Zones.TopOfGreen, "top of green");
}

static void TestDdmrpSizingRejectsIllegalLeadTimeFactor()
{
    var missing = new SkuBufferSetting("MISSING-LTF", "缺少提前期因子", "测试", 10m, 12, 0.33m, 7, 50m, 1m, 100m);
    AssertInvalidOperationRejected(() => DdmrpCalculator.CalculateSizing(missing), "提前期因子");

    var zero = missing with { LeadTimeFactor = 0m };
    AssertInvalidOperationRejected(() => DdmrpCalculator.CalculateSizing(zero), "提前期因子");

    var greaterThanOne = missing with { LeadTimeFactor = 1.01m };
    AssertInvalidOperationRejected(() => DdmrpCalculator.CalculateSizing(greaterThanOne), "提前期因子");
}

static void TestPlanningRecommendation()
{
    var sku = new SkuBufferSetting(
        "DDMRP-EXAMPLE", "标准定容算例", "测试", 10m, 12, 0.33m, 7, 50m, 1m, 100m,
        LeadTimeFactor: 0.5m,
        ParameterSnapshotId: "DDMRP-EXAMPLE-V1",
        ParameterEvidenceStatus: "Complete");
    var position = new InventoryPosition(sku.Sku, 50m, 0m, 0m);

    var recommendation = DdmrpCalculator.CalculateRecommendation(sku, position);

    AssertEqual("Order", recommendation.Action, "action");
    AssertEqual(220m, recommendation.OrderQuantity, "order quantity");
    AssertEqual("Red", recommendation.BufferStatus, "buffer status");
}

static void AssertInvalidOperationRejected(Action action, string expectedMessage)
{
    try
    {
        action();
    }
    catch (InvalidOperationException exception)
    {
        AssertTrue(exception.Message.Contains(expectedMessage, StringComparison.Ordinal),
            $"rejection message should contain {expectedMessage}");
        return;
    }

    throw new InvalidOperationException($"expected InvalidOperationException containing {expectedMessage}");
}
```

- [ ] **Step 2: Run the console harness and verify RED.**

Run:

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: compilation fails because `LeadTimeFactor`, `DdmrpSizingResult`, and `CalculateSizing` do not exist.

- [ ] **Step 3: Append compatible parameter fields and define the unified result in `Models.cs`.**

Append these parameters after the existing `ParameterStatus` parameter of `SkuBufferSetting`; do not insert them into the existing positional portion:

```csharp
decimal? LeadTimeFactor = null,
string ParameterSnapshotId = "",
string ParameterEvidenceStatus = "EvidenceMissing");
```

Add this record immediately after `BufferZones`:

```csharp
public sealed record DdmrpSizingResult(
    decimal PeriodAdu,
    decimal EffectiveAdu,
    decimal LeadTimeDemand,
    decimal LeadTimeFactor,
    decimal VariabilityFactor,
    decimal ZoneAdjustmentFactor,
    decimal RedBase,
    decimal RedSafety,
    decimal GreenLeadTimeCandidate,
    decimal GreenMoqCandidate,
    decimal GreenOrderCycleCandidate,
    string GreenDriver,
    BufferZones Zones,
    string ParameterSnapshotId,
    string EvidenceStatus);
```

- [ ] **Step 4: Implement the standard calculator and retain the old public wrapper.**

Replace `CalculateZones` with these complete methods; leave net-flow and status methods unchanged:

```csharp
public static DdmrpSizingResult CalculateSizing(SkuBufferSetting sku, decimal? periodAdu = null)
{
    if (sku.Adu <= 0m) throw new InvalidOperationException("ADU 必须大于零。");
    if (sku.DecoupledLeadTimeDays <= 0) throw new InvalidOperationException("DLT 必须大于零。");
    if (sku.LeadTimeFactor is null or <= 0m or > 1m) throw new InvalidOperationException("提前期因子必须在 0 到 1 之间。");
    if (sku.VariabilityFactor < 0m) throw new InvalidOperationException("波动因子不得小于零。");
    if (sku.DemandAdjustmentFactor <= 0m) throw new InvalidOperationException("DAF 必须大于零。");
    if (sku.ZoneAdjustmentFactor <= 0m) throw new InvalidOperationException("区域调整因子必须大于零。");
    if (sku.MinimumOrderQuantity < 0m) throw new InvalidOperationException("MOQ 不得小于零。");
    if (sku.OrderCycleDays <= 0) throw new InvalidOperationException("订货周期必须大于零。");

    var selectedAdu = Math.Max(sku.Adu, periodAdu ?? sku.Adu);
    var effectiveAdu = selectedAdu * sku.DemandAdjustmentFactor;
    var leadTimeDemand = effectiveAdu * sku.DecoupledLeadTimeDays;
    var leadTimeFactor = sku.LeadTimeFactor.Value;
    var redBase = leadTimeDemand * leadTimeFactor;
    var redSafety = redBase * sku.VariabilityFactor;
    var greenLeadTime = leadTimeDemand * leadTimeFactor;
    var greenMoq = sku.MinimumOrderQuantity;
    var greenOrderCycle = effectiveAdu * sku.OrderCycleDays;
    var greenDriver = greenOrderCycle >= greenLeadTime && greenOrderCycle >= greenMoq
        ? "OrderCycle"
        : greenMoq >= greenLeadTime ? "MinimumOrderQuantity" : "LeadTime";
    var zones = new BufferZones(
        decimal.Round((redBase + redSafety) * sku.ZoneAdjustmentFactor, 0),
        decimal.Round(leadTimeDemand * sku.ZoneAdjustmentFactor, 0),
        decimal.Round(Math.Max(greenLeadTime, Math.Max(greenMoq, greenOrderCycle)) * sku.ZoneAdjustmentFactor, 0));
    var evidence = sku.ParameterEvidenceStatus == "Complete" && !string.IsNullOrWhiteSpace(sku.ParameterSnapshotId)
        ? "Complete"
        : "EvidenceMissing";

    return new DdmrpSizingResult(
        selectedAdu,
        effectiveAdu,
        leadTimeDemand,
        leadTimeFactor,
        sku.VariabilityFactor,
        sku.ZoneAdjustmentFactor,
        redBase,
        redSafety,
        greenLeadTime,
        greenMoq,
        greenOrderCycle,
        greenDriver,
        zones,
        sku.ParameterSnapshotId,
        evidence);
}

public static BufferZones CalculateZones(SkuBufferSetting sku) => CalculateSizing(sku).Zones;
```

- [ ] **Step 5: Create the shared backend explanation builder.**

Create `DdmrpSizingExplanation.cs` with this complete class so neither history nor future services construct formula strings independently:

```csharp
namespace AdaptiveSopDdsop.Web.Domain;

public static class DdmrpSizingExplanation
{
    public static IReadOnlyList<BufferSizingLine> Build(DdmrpSizingResult sizing)
    {
        var greenDriver = sizing.GreenDriver switch
        {
            "OrderCycle" => "订货周期",
            "MinimumOrderQuantity" => "MOQ",
            "LeadTime" => "提前期",
            _ => "证据缺失"
        };
        return new List<BufferSizingLine>
        {
            new("有效 ADU", "ADU × DAF", decimal.Round(sizing.EffectiveAdu, 1), "使用当期日均需求和需求调整因子。"),
            new("提前期需求", "有效 ADU × DLT", decimal.Round(sizing.LeadTimeDemand, 1), "覆盖解耦提前期内的需求。"),
            new("红区基础", "提前期需求 × 提前期因子", decimal.Round(sizing.RedBase, 1), "形成红区基础保护。"),
            new("红区安全", "红区基础 × 波动因子", decimal.Round(sizing.RedSafety, 1), "吸收需求与供应波动。"),
            new("红区", "（红区基础 + 红区安全）× 区域调整", sizing.Zones.Red, "红区厚度。"),
            new("黄区", "提前期需求 × 区域调整", sizing.Zones.Yellow, "黄区厚度。"),
            new("绿区提前期候选", "提前期需求 × 提前期因子", decimal.Round(sizing.GreenLeadTimeCandidate, 1), "提前期候选。"),
            new("绿区 MOQ 候选", "MOQ", decimal.Round(sizing.GreenMoqCandidate, 1), "最小订货量候选。"),
            new("绿区订货周期候选", "有效 ADU × 订货周期", decimal.Round(sizing.GreenOrderCycleCandidate, 1), "订货周期候选。"),
            new("绿区", "max（三个候选）× 区域调整", sizing.Zones.Green, $"决定项：{greenDriver}。"),
            new("总缓冲", "红区 + 黄区 + 绿区", sizing.Zones.TopOfGreen, "绿区上沿。")
        };
    }
}
```

- [ ] **Step 6: Extend internal parameter profiles and calibrate all new DemoFixture settings.**

Append these optional fields to `DdmrpParameterProfile` in `ScenarioWorkspaceData.cs`:

```csharp
decimal? LeadTimeFactor = null,
string ParameterSnapshotId = "",
string EvidenceStatus = "EvidenceMissing",
DdmrpSizingResult? Sizing = null,
IReadOnlyList<BufferSizingLine>? SizingLines = null);
```

In `SeedData.Create`, replace the 12 old `VariabilityFactor` values with explicit safety proportions in this SKU order:

```csharp
0.35m, 0.40m, 0.30m, 0.55m, 0.60m, 0.40m,
0.50m, 0.65m, 0.35m, 0.40m, 0.55m, 0.30m
```

In `ApplyDdmrpParameterProfile`, add the following named values to the existing `with` expression:

```csharp
LeadTimeFactor = sku.DecoupledLeadTimeDays switch
{
    <= 5 => 0.80m,
    <= 8 => 0.60m,
    <= 12 => 0.50m,
    _ => 0.30m
},
ParameterSnapshotId = $"DDMRP-DEMO-2026-06-{sku.Sku}",
ParameterEvidenceStatus = "Complete",
```

Update every explicit `new SkuBufferSetting` in the test file with named `LeadTimeFactor`, `ParameterSnapshotId`, and `ParameterEvidenceStatus`. Replace the existing recommendation test with the version in Step 1. In `FixedScenarioWorkspaceDataSource`, derive the fixed profile's zone tops and appended sizing fields by calling `DdmrpCalculator.CalculateSizing(sku)` rather than retaining `750/1250/1950`. Update `BuildDdmrpParameters` to call `CalculateSizing`, require the new fields for completeness, and pass `sizing` plus `DdmrpSizingExplanation.Build(sizing)` into the appended profile properties. Update the two internal master-setting text builders to say`提前期因子`和`波动因子`, not the old multiplier semantics.

Keep the source control-point value unchanged for contract stability. Add this display alias later in the DDAE-only JavaScript renderer rather than changing `SeedData`:

```javascript
function historyControlPointLabel(value) {
  return value === "关键进口 FPGA 库存控制点"
    ? "关键进口 FPGA 独立库存控制点"
    : value;
}
```

After recalculating the seed zones, set the internal demo inventory position for `SAT-BUS-001` to on-hand `1`, open supply `1`, and qualified demand `2`. Together with the recalibrated settings this preserves the existing explicit Red/Yellow/Green/OverTopOfGreen variety test without changing any contract type or fixture.

- [ ] **Step 7: Run the full harness and verify GREEN.**

Run:

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: exit code 0; the new standard and validation tests print `PASS`, and no registered test prints `FAIL`.

- [ ] **Step 8: Commit the standard sizing unit.**

```powershell
git add src/AdaptiveSopDdsop.Web/Domain/Models.cs src/AdaptiveSopDdsop.Web/Domain/DdmrpCalculator.cs src/AdaptiveSopDdsop.Web/Domain/DdmrpSizingExplanation.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs src/AdaptiveSopDdsop.Web/Data/SeedData.cs src/AdaptiveSopDdsop.Web/Data/SeedScenarioWorkspaceDataSource.cs src/AdaptiveSopDdsop.Web/Domain/MasterSettingsGovernanceService.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: standardize internal DDMRP sizing"
```

---

### Task 2: Make replenishment and future charts share period sizing

**Files:**

- Modify: `src/AdaptiveSopDdsop.Web/Domain/Models.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/DemandDrivenPlanningEngine.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/BufferTrendWorkspaceService.cs`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**

- Consumes: `DdmrpCalculator.CalculateSizing` from Task 1.
- Produces: `BufferProjectionPoint.Sizing` as the single period result used by status, orders, traces and UI DTOs.
- Produces: `BufferTrendSeriesPoint.Sizing` and backend `DemandSpikeThreshold`.

- [ ] **Step 1: Add a failing dynamic-period regression.**

Register and add this test near the existing buffer-trend tests:

```csharp
("Future buffer projection uses the same backend period sizing for orders and charts", TestFutureBufferTrendUsesBackendPeriodSizing),
```

```csharp
static void TestFutureBufferTrendUsesBackendPeriodSizing()
{
    var data = new SeedScenarioWorkspaceDataSource(SeedData.Create())
        .Load(new ScenarioWorkspaceDataRequest(12, new DateOnly(2026, 6, 1)));
    var run = DemandDrivenPlanningEngine.ProjectBuffers(data.Skus, data.Inventory, data.Demand, 12);
    var plan = new DemandDrivenPlanResult(run.BufferProjections, run.ReplenishmentOrders,
        Array.Empty<CapacityLoadProjection>(), Array.Empty<ProjectedSupplyRequirement>(), run.Traces);
    var trend = BufferTrendWorkspaceService.Build(data, "baseline", "基准方案", data.Skus, plan);
    var detail = trend.SkuDetails.Single(item => item.Sku == "TC-MLI-301");

    AssertTrue(detail.Series.All(item => item.Sizing is not null), "every trend point should carry backend sizing");
    AssertTrue(detail.Series.Select(item => (item.TopOfRed, item.TopOfYellow, item.TopOfGreen)).Distinct().Count() >= 3,
        "planned demand should create at least three zone combinations");
    AssertTrue(detail.Series.All(item => item.TopOfRed <= item.TopOfYellow && item.TopOfYellow <= item.TopOfGreen),
        "zone tops should stay ordered");
    AssertTrue(detail.Series[3].Sizing!.PeriodAdu > detail.Series[4].Sizing!.PeriodAdu,
        "week four demand peak should exceed the following trough");
    AssertTrue(detail.Series.All(item => item.DemandSpikeThreshold is > 0m),
        "demand spike threshold should come from the backend");

    foreach (var point in detail.Series)
    {
        AssertEqual(point.Sizing!.Zones.TopOfRed, point.TopOfRed, $"top of red week {point.Week}");
        AssertEqual(point.Sizing.Zones.TopOfYellow, point.TopOfYellow, $"top of yellow week {point.Week}");
        AssertEqual(point.Sizing.Zones.TopOfGreen, point.TopOfGreen, $"top of green week {point.Week}");
    }
}
```

- [ ] **Step 2: Run the harness and verify RED.**

Run the full console harness. Expected: compilation fails because the projection and trend points do not expose `Sizing` or `DemandSpikeThreshold`.

- [ ] **Step 3: Append sizing to the internal projection records.**

Append to `BufferProjectionPoint` in `Models.cs`:

```csharp
DdmrpSizingResult? Sizing = null);
```

Append to `BufferTrendSeriesPoint` in `ScenarioWorkspaceData.cs`:

```csharp
DdmrpSizingResult? Sizing = null,
decimal? DemandSpikeThreshold = null);
```

These additions are internal and optional so previously serialized internal results remain readable as evidence-missing results.

- [ ] **Step 4: Move weekly sizing into the planning loop.**

In `DemandDrivenPlanningEngine.ProjectBuffers`, remove the one-time `CalculateZones` call. After computing `weeklyDemand`, use this block and use `zones` for status, order quantity and trace in that same iteration:

```csharp
var periodAdu = Math.Max(sku.Adu, weeklyDemand / 5m);
var sizing = DdmrpCalculator.CalculateSizing(sku, periodAdu);
var zones = sizing.Zones;
```

Pass `sizing` as the final argument of `BufferProjectionPoint`. Keep the existing order-cycle gate, prebuild handling and no-auto-publish behavior unchanged. Update `TestTimePhasedBufferProjectionCreatesReplenishmentTrace`, `TestTimePhasedBufferProjectionWaitsForOrderCycleReview`, and `TestPrebuildCampaignMovesReplenishmentBeforeFuturePeak` to derive expected starts, yellow/green tops, order quantities and trace text from each projection point's own `Sizing` instead of retaining `1250/1650/1950` constants.

- [ ] **Step 5: Remove the duplicate time-phased formula from `BufferTrendWorkspaceService`.**

Delete `CalculateTimePhasedAdu` and `CalculateTimePhasedZones`. In the `series` projection require the engine result:

```csharp
var sizing = point.Sizing ?? throw new InvalidOperationException(
    $"{point.Sku} 第 {point.Week} 周缺少后端定容结果，不能生成缓冲趋势。");
var timePhasedZones = sizing.Zones;
var targetInventory = (timePhasedZones.TopOfYellow + timePhasedZones.TopOfGreen) / 2m;
var demandSpikeThreshold = decimal.Round(sku.Adu * 5m * 1.5m, 1);
```

Map `sizing.PeriodAdu` to the existing `TimePhasedAdu`, append `sizing` and `demandSpikeThreshold` to `BufferTrendSeriesPoint`, and replace private `BuildBufferSizing` formula logic with:

```csharp
private static IReadOnlyList<BufferSizingLine> BuildBufferSizing(DdmrpSizingResult sizing) =>
    DdmrpSizingExplanation.Build(sizing);
```

Pass `DdmrpCalculator.CalculateSizing(sku)` when building the selected SKU's base sizing lines.

- [ ] **Step 6: Run the harness and verify GREEN.**

Run the full console harness. Expected: exit code 0, the dynamic-period test passes, and replenishment trace tests use the same period tops as the chart DTO.

- [ ] **Step 7: Commit the shared period-sizing unit.**

```powershell
git add src/AdaptiveSopDdsop.Web/Domain/Models.cs src/AdaptiveSopDdsop.Web/Domain/DemandDrivenPlanningEngine.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs src/AdaptiveSopDdsop.Web/Domain/BufferTrendWorkspaceService.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: share period sizing across planning and trends"
```

---

### Task 3: Guard new and legacy frozen baselines

**Files:**

- Modify: `src/AdaptiveSopDdsop.Web/Data/SeedCurrentBaselineDataSource.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**

- Consumes: nullable `SkuBufferSetting.LeadTimeFactor` and parameter evidence from Task 1.
- Produces: required candidate evidence section `DDMRP_SIZING`.
- Preserves: existing SQLite tables and immutable `payload_json`; no `ALTER TABLE`, delete, rebuild or backfill.

- [ ] **Step 1: Write failing candidate and legacy JSON tests.**

Add two registered tests:

```csharp
("Current baseline blocks incomplete DDMRP sizing evidence", TestCurrentBaselineBlocksIncompleteDdmrpSizingEvidence),
("Legacy frozen baseline keeps missing lead-time factor visible and cannot be recalculated", TestLegacyFrozenBaselineKeepsMissingLeadTimeFactor),
```

The first test must wrap the normal scenario source with a test adapter that sets one SKU's `LeadTimeFactor` to `null`, then assert the `DDMRP_SIZING` section is required, incomplete and contains a blocking item. The second test must serialize a complete `CurrentBaselineSnapshot`, remove only the JSON properties `leadTimeFactor`, `parameterSnapshotId`, and `parameterEvidenceStatus`, deserialize it, assert the restored setting has `LeadTimeFactor == null`, and assert `ScenarioRunPreviewService.LoadFrozenWorkspaceData` throws a message containing`旧版本缺少提前期因子`.

- [ ] **Step 2: Run the harness and verify RED.**

Expected: the candidate has no `DDMRP_SIZING` section and old frozen data is not rejected explicitly.

- [ ] **Step 3: Add item-level DDMRP evidence to the candidate.**

In `SeedCurrentBaselineDataSource.GetCandidate`, build items from `planningInputs.Skus` with this exact completeness rule:

```csharp
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
```

Append a required section created by this dedicated helper with code `DDMRP_SIZING`, name`DDMRP 定容证据`, source`DDAE Demo Planning Snapshot`, and `ddmrpSizingItems`:

```csharp
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
```

- [ ] **Step 4: Reject legacy inputs only at recalculation time.**

Immediately after loading `planningInputs` in `ScenarioRunPreviewService.LoadFrozenWorkspaceData`, add:

```csharp
var incomplete = planningInputs.Skus
    .Where(item => item.LeadTimeFactor is null or <= 0m or > 1m ||
        string.IsNullOrWhiteSpace(item.ParameterSnapshotId) ||
        item.ParameterEvidenceStatus != "Complete")
    .Select(item => item.Sku)
    .OrderBy(item => item, StringComparer.Ordinal)
    .ToList();
if (incomplete.Count > 0)
{
    throw new InvalidOperationException(
        $"旧版本缺少提前期因子或完整定容证据：{string.Join("、", incomplete)}；请从当前候选冻结新版本。");
}
```

Do not change `CurrentBaselineService.EnsureSchema`; the nullable record fields provide JSON backward compatibility and immutable old rows remain untouched.

- [ ] **Step 5: Run the harness and verify GREEN.**

Expected: both tests pass; all existing freeze, immutability, audit and scenario-comparison tests remain green.

- [ ] **Step 6: Commit the baseline compatibility unit.**

```powershell
git add src/AdaptiveSopDdsop.Web/Data/SeedCurrentBaselineDataSource.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: guard DDMRP baseline evidence"
```

---

### Task 4: Add explicit historical facts and parameter versions

**Files:**

- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryOperatingFacts.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Data/SeedHistoryOperatingFactSource.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Program.cs`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**

- Preserves: `IHistoryOperatingFactSource.Load(HistoryFactRequest)`.
- Produces: stock components, time-buffer counts, historical DDMRP snapshots and capacity-protection snapshots.
- Requires: every historical buffer point references exactly one effective parameter snapshot.

- [ ] **Step 1: Write a failing strict-history fact test.**

Register `TestHistoryFactsExposeVersionedInventoryTimeAndCapacityEvidence`. It must load 26 and 52 weeks and assert:

```csharp
AssertEqual(26, recent.BufferFacts.Select(item => item.WeekOffset).Distinct().Count(), "recent buffer weeks");
AssertEqual(52, annual.BufferFacts.Select(item => item.WeekOffset).Distinct().Count(), "annual buffer weeks");
AssertTrue(annual.BufferFacts.All(item => item.EndingOnHand is not null && item.OpenSupply is not null && item.QualifiedDemand is not null),
    "inventory components should be explicit");
AssertTrue(annual.BufferFacts.All(item => item.EndingNetFlow == item.EndingOnHand + item.OpenSupply - item.QualifiedDemand),
    "inventory components should reconcile to net flow");
AssertTrue(annual.TimeBufferFacts?.Select(item => item.WeekOffset).Distinct().Count() == 52,
    "time-buffer facts should cover 52 weeks");
AssertTrue(annual.DdmrpParameterFacts?.Select(item => item.SnapshotId).Distinct().Count() >= 2,
    "history should contain at least two parameter versions");
AssertTrue(annual.CapacityProtectionFacts?.Any(item => item.UpstreamResourceCode == "RES-AIT" && item.ProtectedCcrResourceCode == "RES-HARNESS") == true,
    "historical sequence evidence should protect the CCR from upstream");
```

- [ ] **Step 2: Run the harness and verify RED.**

Expected: the new fact properties and collections do not compile.

- [ ] **Step 3: Append compatible fields and new fact records.**

Append to `WeeklyBufferFact`:

```csharp
decimal? EndingOnHand = null,
decimal? OpenSupply = null,
decimal? QualifiedDemand = null,
string ControlPoint = "",
string? ParameterSnapshotId = null);
```

Add these records in `HistoryOperatingFacts.cs`:

```csharp
public sealed record WeeklyTimeBufferFact(
    string BufferId,
    string ControlPoint,
    string ProtectedActivity,
    int WeekOffset,
    int? EarlyCount,
    int? GreenCount,
    int? YellowCount,
    int? RedCount,
    int? LateCount,
    decimal? AbnormalCost,
    string? AbnormalCostEventId,
    string ExplicitCause,
    string EvidenceStatus);

public sealed record HistoricalDdmrpParameterFact(
    string SnapshotId,
    string Sku,
    string Name,
    string ControlPoint,
    int EffectiveFromWeekOffset,
    int EffectiveThroughWeekOffset,
    SkuBufferSetting Setting,
    string SourceAuthority,
    string AsOfUtc,
    string EvidenceStatus);

public sealed record HistoricalCapacityProtectionFact(
    string SnapshotId,
    string UpstreamResourceCode,
    string ProtectedCcrResourceCode,
    int UpstreamOperationSequence,
    int CcrOperationSequence,
    decimal ReservePercent,
    int EffectiveFromWeekOffset,
    int EffectiveThroughWeekOffset,
    string EvidenceStatus);
```

Append nullable collections to `HistoryFactSet` so existing test doubles remain source-compatible:

```csharp
IReadOnlyList<WeeklyTimeBufferFact>? TimeBufferFacts = null,
IReadOnlyList<HistoricalDdmrpParameterFact>? DdmrpParameterFacts = null,
IReadOnlyList<HistoricalCapacityProtectionFact>? CapacityProtectionFacts = null);
```

- [ ] **Step 4: Make the history seed use the registered validation data.**

Add constructors:

```csharp
private readonly ValidationData _data;

public SeedHistoryOperatingFactSource() : this(SeedData.Create()) { }

public SeedHistoryOperatingFactSource(ValidationData data)
{
    _data = data;
    _ddmrpParameterFacts = BuildDdmrpParameterFacts(data.Skus);
    _bufferFacts = BuildBufferFacts(_ddmrpParameterFacts);
    _timeBufferFacts = BuildTimeBufferFacts();
    _capacityProtectionFacts = BuildCapacityProtectionFacts();
}
```

Replace the existing static `BufferFacts` field with instance fields for the four data-dependent collections:

```csharp
private readonly IReadOnlyList<WeeklyBufferFact> _bufferFacts;
private readonly IReadOnlyList<WeeklyTimeBufferFact> _timeBufferFacts;
private readonly IReadOnlyList<HistoricalDdmrpParameterFact> _ddmrpParameterFacts;
private readonly IReadOnlyList<HistoricalCapacityProtectionFact> _capacityProtectionFacts;
```

The existing operating, capacity, constraint and abnormal-cost fixtures may remain static because they do not depend on `ValidationData`.

Change Program registration to:

```csharp
builder.Services.AddSingleton<IHistoryOperatingFactSource>(sp =>
    new SeedHistoryOperatingFactSource(sp.GetRequiredService<ValidationData>()));
```

Do not move or edit the integration-contract endpoint block.

- [ ] **Step 5: Generate two immutable parameter versions and reconciled weekly facts.**

Use exactly these historical inventory SKUs: `AV-COM-201`, `AV-OBC-202`, `AV-FPGA-203`, `TC-MLI-301`, `TC-RAD-302`. For each one, create a prior snapshot effective `-52..-27` and a current snapshot effective `-26..-1`. Derive both from `_data.Skus`; preserve the source control-point value and use distinct IDs `HIST-{SKU}-V1` and `HIST-{SKU}-V2`. Construct the versions with:

```csharp
var currentSetting = sourceSetting with
{
    ParameterSnapshotId = $"HIST-{sourceSetting.Sku}-V2",
    ParameterEvidenceStatus = "Complete"
};
var priorSetting = sourceSetting with
{
    Adu = decimal.Round(sourceSetting.Adu * 1.12m, 2),
    LeadTimeFactor = Math.Min(1m, sourceSetting.LeadTimeFactor!.Value + 0.10m),
    VariabilityFactor = Math.Min(1m, sourceSetting.VariabilityFactor + 0.10m),
    ParameterSnapshotId = $"HIST-{sourceSetting.Sku}-V1",
    ParameterEvidenceStatus = "Complete"
};
```

Set each snapshot's `AsOfUtc` to the request-independent fixture cutoff `2026-06-01T23:59:59Z`. Generate weekly buffer facts by selecting the effective snapshot, computing its zones through `DdmrpCalculator.CalculateSizing`, and deriving `targetNetFlow` from this deterministic eight-week pattern:

```csharp
var targetNetFlow = (week % 8) switch
{
    0 => sizing.Zones.TopOfRed * 0.55m,
    1 => sizing.Zones.TopOfRed * 0.90m,
    2 => (sizing.Zones.TopOfRed + sizing.Zones.TopOfYellow) / 2m,
    3 => sizing.Zones.TopOfYellow * 0.95m,
    4 => (sizing.Zones.TopOfYellow + sizing.Zones.TopOfGreen) / 2m,
    5 => sizing.Zones.TopOfGreen * 0.85m,
    6 => sizing.Zones.TopOfGreen * 1.12m,
    _ => sizing.Zones.TopOfGreen * 0.72m
};
```

Then set the reconciled inventory quantities:

```csharp
var endingOnHand = decimal.Round(targetNetFlow * 0.72m, 1);
var openSupply = decimal.Round(targetNetFlow * 0.38m, 1);
var qualifiedDemand = decimal.Round(endingOnHand + openSupply - targetNetFlow, 1);
var endingNetFlow = endingOnHand + openSupply - qualifiedDemand;
```

Generate one 52-week time-buffer series for `MS-TB-001`, with all five count bands occurring, and link weeks containing costs to existing `HistoryAbnormalCostEvent.EventId`; the fact's `AbnormalCost` must equal the linked event amount. Generate a historical protection snapshot for `RES-AIT -> RES-HARNESS` with upstream sequence lower than the CCR sequence and 20% reserve. Calibrate weekly committed load against planned available capacity with a repeatable pattern containing approximately 52%, 68%, 86%, and 104%, so safe, high-load, near-limit, and overload categories all have explicit samples. `Load` must filter all new collections with the same strict negative-week predicate as existing facts.

- [ ] **Step 6: Run the harness and verify GREEN.**

Expected: the new fact test passes; 26/52 aggregates remain distinct and all explicit-cost totals remain unchanged.

- [ ] **Step 7: Commit the historical fact unit.**

```powershell
git add src/AdaptiveSopDdsop.Web/Domain/HistoryOperatingFacts.cs src/AdaptiveSopDdsop.Web/Data/SeedHistoryOperatingFactSource.cs src/AdaptiveSopDdsop.Web/Program.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: add versioned historical buffer facts"
```

---

### Task 5: Project historical facts into inventory, time, capacity and sizing views

**Files:**

- Create: `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewModels.cs`
- Create: `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewProjectionBuilder.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewWorkspaceService.cs`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**

- Consumes: Task 4 fact collections and Task 1 calculator.
- Produces: non-null `InventoryBuffers`, `DdmrpSizingSnapshots`, `TimeBuffers`, `CapacityBuffers` on `HistoryReviewWorkspace`.
- Preserves: existing outcomes, protection relationships, zone residence, capacity summaries and constraint exposure.

- [ ] **Step 1: Write failing projection, snapshot-isolation and evidence-gap tests.**

Add registered tests with these exact responsibilities:

```csharp
("History review projects stock time capacity and sizing views from explicit facts", TestHistoryReviewProjectsExplicitBufferViews),
("History review uses the effective historical parameter snapshot for every point", TestHistoryReviewUsesEffectiveParameterSnapshot),
("History review exposes missing evidence instead of zero or current-parameter backfill", TestHistoryReviewDoesNotBackfillMissingEvidence),
```

The first test asserts every material has 26 points in the six-month view, every time point preserves five band counts and cost linkage, AIT is the only upstream protection example, and FPGA appears only in inventory. The second crosses week `-27/-26` and asserts snapshot IDs and zone tops change. The third removes one historical parameter snapshot and one time fact, then asserts the affected point has null tops or counts with `EvidenceMissing`; it must also compare normal and scenario-poisoned results to prove current/future quantities do not alter history.

Update the existing missing-capacity-evidence branch in `TestHistoryReviewUsesCumulativeLeadTimeAndProtectionEvidence`: replace `CapacityProtectionRemovingScenarioWorkspaceDataSource` with a `CapacityProtectionRemovingHistoryOperatingFactSource` wrapper whose `Load` returns `inner.Load(request) with { CapacityProtectionFacts = Array.Empty<HistoricalCapacityProtectionFact>() }`. The scenario definition is no longer authoritative for historical protection. Add a parallel assertion that removing only the scenario capacity definition leaves the historical projection unchanged.

- [ ] **Step 2: Run the harness and verify RED.**

Expected: the new response collections and builder types do not exist.

- [ ] **Step 3: Create the history response records.**

Create `HistoryReviewModels.cs` with records carrying these exact fields:

```csharp
namespace AdaptiveSopDdsop.Web.Domain;

public sealed record HistoryDistributionBucket(string Code, string Label, int Count, decimal Percent);

public sealed record HistoryInventoryPoint(
    int WeekOffset, string PeriodStartDate, decimal? EndingOnHand, decimal? OpenSupply,
    decimal? QualifiedDemand, decimal? NetFlow, decimal? TopOfRed, decimal? TopOfYellow,
    decimal? TopOfGreen, string Status, string Cause, string? ParameterSnapshotId, string EvidenceStatus);

public sealed record HistoryInventoryBufferView(
    string ControlPoint, string Sku, string Name, int DetailWindowWeeks,
    IReadOnlyList<HistoryInventoryPoint> Points,
    IReadOnlyList<HistoryDistributionBucket> Distribution,
    string EvidenceStatus);

public sealed record HistoryDdmrpSizingSnapshotView(
    string SnapshotId, string ControlPoint, string Sku, string Name,
    int EffectiveFromWeekOffset, int EffectiveThroughWeekOffset,
    SkuBufferSetting Setting, DdmrpSizingResult? Sizing,
    IReadOnlyList<BufferSizingLine> SizingLines, decimal? AverageOnHand,
    string SourceAuthority, string AsOfUtc, string EvidenceStatus);

public sealed record HistoryTimeBufferPoint(
    int WeekOffset, string PeriodStartDate, int? EarlyCount, int? GreenCount,
    int? YellowCount, int? RedCount, int? LateCount, decimal? AbnormalCost,
    string Cause, string EvidenceStatus);

public sealed record HistoryTimeBufferView(
    string BufferId, string ControlPoint, string ProtectedActivity,
    IReadOnlyList<HistoryTimeBufferPoint> Points,
    IReadOnlyList<HistoryDistributionBucket> Distribution,
    string EvidenceStatus);

public sealed record HistoryCapacityPoint(
    int WeekOffset, string PeriodStartDate, decimal? TheoreticalCapacity,
    decimal? StandardCapacity, decimal? DemonstratedCapacity,
    decimal? PlannedAvailableCapacity, decimal? CommittedLoad,
    decimal? ProtectionStart, decimal? ProtectiveCapacity,
    decimal? ConsumedProtection, decimal? RemainingProtection,
    string EvidenceStatus);

public sealed record HistoryCapacityBufferView(
    string ResourceCode, string ResourceName, string? ProtectedCcrResourceCode,
    string RelationshipRole, IReadOnlyList<HistoryCapacityPoint> Points,
    IReadOnlyList<HistoryDistributionBucket> Distribution, string EvidenceStatus);

public sealed record HistoryReviewProjection(
    IReadOnlyList<HistoryInventoryBufferView> InventoryBuffers,
    IReadOnlyList<HistoryDdmrpSizingSnapshotView> DdmrpSizingSnapshots,
    IReadOnlyList<HistoryTimeBufferView> TimeBuffers,
    IReadOnlyList<HistoryCapacityBufferView> CapacityBuffers);
```

- [ ] **Step 4: Implement one projection builder with explicit validation.**

Create `HistoryReviewProjectionBuilder` with this public signature:

```csharp
public static HistoryReviewProjection Build(
    HistoryFactSet facts,
    ScenarioWorkspaceDataSet definitions,
    int detailWindowWeeks)
```

Within it, resolve inventory points only by `WeeklyBufferFact.ParameterSnapshotId`; verify the snapshot covers the point's week and `OnHand + OpenSupply - QualifiedDemand == NetFlow` within `0.1m`. Call `DdmrpCalculator.CalculateSizing(snapshot.Setting)` only when the snapshot has complete evidence and a valid lead-time factor. Map an invalid point with null zone tops and `EvidenceMissing`.

Build distribution buckets deterministically: inventory uses point-status counts for `Red`, `Yellow`, `Green`, `OverTopOfGreen`; time uses sums of `Early`, `Green`, `Yellow`, `Red`, `Late`; capacity uses weekly committed-load ratios `Safe <= 60%`, `High <= 80%`, `NearLimit <= 100%`, and `Overload > 100%`. Percentages use only complete observations and total 100% within `0.2m` rounding tolerance.

Build time cost only when `AbnormalCostEventId` joins exactly one `HistoryAbnormalCostEvent.EventId` and the fact amount equals the event amount; use the event amount as authority and mark mismatches `EvidenceMissing`. Do not sum or invent a fallback cost. Build each sizing snapshot's `AverageOnHand` from complete inventory points linked to that snapshot. Build capacity protection only when a historical protection fact has complete evidence, `UpstreamOperationSequence < CcrOperationSequence`, and covers the point's week. Use:

```csharp
protective = planned * reservePercent / 100m;
protectionStart = planned - protective;
consumed = Math.Clamp(committed - protectionStart, 0m, protective);
remaining = protective - consumed;
```

Use `definitions.Resources` only to resolve resource names. Exclude FPGA from time and capacity lists through the existing `ProtectionProductEligibility` rule and an explicit test assertion, not by deleting its inventory facts.

- [ ] **Step 5: Append projection collections to the workspace and use historical snapshots for old summaries.**

Append optional collections to `HistoryReviewWorkspace` for source compatibility:

```csharp
IReadOnlyList<HistoryInventoryBufferView>? InventoryBuffers = null,
IReadOnlyList<HistoryDdmrpSizingSnapshotView>? DdmrpSizingSnapshots = null,
IReadOnlyList<HistoryTimeBufferView>? TimeBuffers = null,
IReadOnlyList<HistoryCapacityBufferView>? CapacityBuffers = null);
```

In `GetReview`, call the builder and pass its four collections. Change `BuildZoneResidence` to use each point's historical snapshot rather than current `definitions.DdmrpParameters`. Change capacity protection relationship logic to use the historical capacity-protection snapshot; scenario data remains name-only. Compute maximum cumulative lead time from complete historical parameter facts in the requested window.

- [ ] **Step 6: Run the harness and verify GREEN.**

Expected: all three new tests pass; the existing historical quantity-poisoning test still proves scenario quantities cannot alter historical results.

- [ ] **Step 7: Commit the historical projection unit.**

```powershell
git add src/AdaptiveSopDdsop.Web/Domain/HistoryReviewModels.cs src/AdaptiveSopDdsop.Web/Domain/HistoryReviewProjectionBuilder.cs src/AdaptiveSopDdsop.Web/Domain/HistoryReviewWorkspaceService.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: project historical buffer evidence"
```

---

### Task 6: Add the four history routes and visual workspace skeleton

**Files:**

- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**

- Produces route: `#history-review-panel/sizing-trace -> history-sizing-trace-view`.
- Preserves routes: operating-results, buffer-performance, capacity-constraints.
- Produces unique DOM targets consumed by Task 7.

- [ ] **Step 1: Update navigation tests before HTML.**

In the existing hierarchical navigation and route-switching tests, require this exact history order:

```csharp
new[]
{
    ("#history-review-panel/operating-results", "经营结果"),
    ("#history-review-panel/buffer-performance", "缓冲表现"),
    ("#history-review-panel/sizing-trace", "定容追溯"),
    ("#history-review-panel/capacity-constraints", "能力约束")
}
```

Increase total child-route and target counts by one. Add required unique IDs:

```csharp
"history-buffer-overview",
"history-inventory-control-point-options",
"history-inventory-sku-options",
"history-inventory-chart",
"history-time-buffer-options",
"history-time-buffer-chart",
"history-sizing-trace-view",
"history-sizing-control-point-options",
"history-sizing-sku-options",
"history-sizing-snapshot-options",
"history-ddmrp-input-summary",
"history-ddmrp-sizing-body",
"history-ddmrp-zone-chart",
"history-capacity-resource-options",
"history-capacity-buffer-chart",
"buffer-volatility-chart"
```

- [ ] **Step 2: Run the harness and verify RED.**

Expected: the new route and IDs are missing.

- [ ] **Step 3: Add the route and DOM structure without moving protected pages.**

Insert`定容追溯` between`缓冲表现` and`能力约束` in the sidebar. Add this route next to the existing history routes:

```javascript
"#history-review-panel/sizing-trace": Object.freeze({
  stageId: "history-review-panel",
  viewId: "sizing-trace",
  targetId: "history-sizing-trace-view",
  title: "定容追溯",
  parentTitle: "历史回顾",
  requiredHostId: null,
}),
```

Expand `history-buffer-performance-view` with an overview row and two selectable visual cards: inventory and time. Add a separate `history-sizing-trace-view` containing control-point, SKU and snapshot selectors, parameter summary, sizing table, vertical zone chart and evidence text. Expand the capacity view with resource options and a period chart before the existing average layers and constraint table. Add `buffer-volatility-chart` immediately after `buffer-trend-chart` in the future inventory page.

Replace the two history range IDs with repeated buttons using `data-history-range-months="6"` and `data-history-range-months="12"`; place the same compact range control in all four history views so the selected window is always visible. Every ID in Step 1 must occur exactly once. Insert all history markup before `#trace-panel`; do not edit trace or public-demo blocks.

- [ ] **Step 4: Add layout-only CSS.**

Add these classes using existing 8px spacing, green navigation palette, white cards and responsive breakpoints:

```css
.history-buffer-layout { display: grid; grid-template-columns: minmax(220px, .32fr) minmax(0, 1fr); gap: 16px; }
.history-object-options { display: grid; align-content: start; gap: 8px; }
.history-visual-card { border: 1px solid var(--color-border); border-radius: 8px; background: #fff; padding: 16px; }
.history-chart, .buffer-volatility-chart { min-height: 260px; overflow-x: auto; }
.history-buffer-svg, .buffer-volatility-svg { display: block; min-width: 760px; width: 100%; height: auto; }
.history-zone-stack { display: grid; align-content: end; min-height: 320px; max-width: 180px; margin: 0 auto; }
.history-zone-stack > div { display: grid; place-items: center; min-height: 42px; color: #10243d; font-weight: 700; }
.history-zone-stack-red { background: #dc624d; }
.history-zone-stack-yellow { background: #f4e76b; }
.history-zone-stack-green { background: #75bd7a; }
@media (max-width: 980px) { .history-buffer-layout { grid-template-columns: 1fr; } }
```

- [ ] **Step 5: Run the harness and verify GREEN.**

Expected: navigation, unique-ID, validation-page ordering and view-switching tests pass.

- [ ] **Step 6: Commit the history workspace skeleton.**

```powershell
git add src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: add history visualization workspaces"
```

---

### Task 7: Render historical buffer evidence without frontend formulas

**Files:**

- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**

- Consumes: Task 5 response collections.
- Produces: selected control point, SKU, snapshot, time buffer and capacity resource states.
- Constraint: JavaScript may read `sizingLines`, `zones` and point tops but may not contain DDMRP arithmetic.

- [ ] **Step 1: Add failing renderer and no-formula assertions.**

Register `TestHistoryVisualRenderersUseBackendEvidence`. Assert `app.js` contains these state keys and functions:

```text
historyTrendMonths
selectedHistoryControlPoint
selectedHistoryInventorySku
selectedHistorySizingSnapshot
selectedHistoryTimeBufferId
selectedHistoryCapacityResource
renderHistoryBufferOverview
renderHistoryInventoryBuffer
renderHistoryDdmrpSizingTrace
renderHistoryTimeBuffer
renderHistoryCapacityBuffer
renderHistoryDdmrpZoneSvg
historyControlPointLabel
```

Extract `renderHistoryDdmrpSizingTrace` and assert it reads `item.sizingLines`; assert it does not contain `leadTimeFactor *`, `variabilityFactor *`, `Math.max(item.minimumOrderQuantity`, or hard-coded English status labels. Extend the mojibake test to reject `??`, Unicode replacement characters, `EvidenceMissing`, `UpstreamProtection`, and `Trace` inside the business-page region.

- [ ] **Step 2: Run the harness and verify RED.**

Expected: state and renderer assertions fail.

- [ ] **Step 3: Add selection state and central history rendering.**

Append these fields to the existing `state` object:

```javascript
historyTrendMonths: 6,
selectedHistoryControlPoint: null,
selectedHistoryInventorySku: null,
selectedHistorySizingSnapshot: null,
selectedHistoryTimeBufferId: null,
selectedHistoryCapacityResource: null,
```

Refactor `renderHistoryReview(history)` to keep the existing KPI/table rendering and then call:

```javascript
renderHistoryBufferOverview(history);
renderHistoryInventoryBuffer(history);
renderHistoryDdmrpSizingTrace(history);
renderHistoryTimeBuffer(history);
renderHistoryCapacityBuffer(history);
```

Each renderer must retain a still-valid prior selection or select the first valid object after a 6/12-month switch. When no valid object exists, render`证据缺失` and do not construct a zero-valued SVG.

Use `historyControlPointLabel` for history selectors, cards and headings so the FPGA control point is visibly independent inside DDAE while the source value passed to protected contract code remains unchanged.

Render selector buttons with `data-history-control-point`, `data-history-inventory-sku`, `data-history-sizing-snapshot`, `data-history-time-buffer-id`, and `data-history-capacity-resource`. Add delegated click handling that writes only the matching state field and reruns the corresponding history renderer with `state.historyReview`; selecting a control point must also select its first valid SKU and snapshot. Do not navigate or scroll the workspace from these object selectors.

- [ ] **Step 4: Implement inventory, time, capacity and vertical-zone SVGs from DTO values.**

Use these helpers for history charts; split at evidence gaps before building paths so missing points are neither zero-filled nor connected across:

```javascript
function contiguousEvidenceSegments(points, predicate) {
  return points.reduce((segments, point) => {
    if (!predicate(point)) {
      if (segments.length && segments[segments.length - 1].length) segments.push([]);
      return segments;
    }
    if (!segments.length) segments.push([]);
    segments[segments.length - 1].push(point);
    return segments;
  }, []).filter(segment => segment.length > 0);
}

function buildLinearAreaPath(lowerPoints, upperPoints) {
  const points = [...lowerPoints, ...upperPoints];
  if (lowerPoints.length !== upperPoints.length || lowerPoints.length === 0 ||
      points.some(point => !Number.isFinite(point.x) || !Number.isFinite(point.y))) return "";
  const upper = upperPoints.map((point, index) => `${index ? "L" : "M"} ${point.x},${point.y}`).join(" ");
  const lower = [...lowerPoints].reverse().map(point => `L ${point.x},${point.y}`).join(" ");
  return `${upper} ${lower} Z`;
}
```

Inventory reads each point's `topOfRed`, `topOfYellow`, `topOfGreen`, `endingOnHand` and `netFlow`. Time draws five backend count bands and a cost line only where `abnormalCost` is not null. Capacity draws committed-load columns and backend theoretical, standard, demonstrated, planned-available and protection-start lines; only `UpstreamProtection` shows protective consumption, and `CcrUtilization` is labelled`CCR 利用率参照`.

For `renderHistoryDdmrpZoneSvg`, calculate only pixel ratios from `item.sizing.zones.red/yellow/green`; display values, total buffer, `averageOnHand`, effective week range, source cutoff and `greenDriver` through Chinese labels. Render table rows directly from `item.sizingLines`:

```javascript
byId("history-ddmrp-sizing-body").innerHTML = item.sizingLines.length
  ? item.sizingLines.map(line => row([
      escapeHtml(line.component),
      escapeHtml(line.formula),
      number(line.value),
      escapeHtml(businessEvidenceLabel(line.explanation)),
    ])).join("")
  : emptyRow("旧版本缺少提前期因子，不能生成定容明细", 4);
```

Update `renderDdmrpParameterDetail` in the current-baseline area to show `leadTimeFactor`, snapshot/evidence, and backend `sizingLines`; remove its three hard-coded legacy formula strings.

Add this detail loader; it reads only the selected frozen payload and never substitutes the current candidate:

```javascript
async function openBaselineSnapshotDetail(snapshotId) {
  const response = await fetch(`/api/current-baselines/${snapshotId}`, { headers: { Accept: "application/json" } });
  if (!response.ok) throw new Error(`冻结基线详情接口失败：${response.status}`);
  const snapshot = await response.json();
  const parameters = valueOr(snapshot.payload?.planningInputs?.ddmrpParameters, []);
  const items = parameters.map(item => [
    `${escapeHtml(item.sku)} ${escapeHtml(item.name)}`,
    item.leadTimeFactor == null
      ? "旧版本缺少提前期因子；该快照保持只读，不能用于重算"
      : `提前期因子 ${number(item.leadTimeFactor)} · ${escapeHtml(evidenceStatusLabel(item.evidenceStatus))}`,
  ]);
  openWorkspaceDrawer("冻结基线定容证据", [{
    title: `${snapshot.snapshotNumber} · ${baselineStatusLabel(snapshot.status)}`,
    items: items.length ? items : [["定容证据", "旧版本未保存 DDMRP 参数明细"]],
  }]);
}
```

Change the existing `[data-baseline-snapshot-id]` click listener to call both `loadBaselineAudit` and `openBaselineSnapshotDetail`. Extend the UI test to require the detail endpoint and the exact legacy warning.

- [ ] **Step 5: Replace direct history range listeners with delegated range controls.**

Set `state.historyTrendMonths` in `loadHistoryReview`. Toggle every `[data-history-range-months]` button and add one delegated click listener that parses 6 or 12 and calls `loadHistoryReview`. Remove the two `byId("history-range-6")` and `byId("history-range-12")` listeners.

- [ ] **Step 6: Run UI/static regressions and JavaScript syntax.**

Run:

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
node --check .\src\AdaptiveSopDdsop.Web\wwwroot\js\app.js
```

Expected: both commands exit 0; renderer, Chinese, navigation and no-formula assertions pass.

- [ ] **Step 7: Commit the historical rendering unit.**

```powershell
git add src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css src/AdaptiveSopDdsop.Web/Pages/Index.cshtml tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: render historical buffer evidence"
```

---

### Task 8: Draw a smooth dynamic upper buffer and retain an independent lower volatility chart

**Files:**

- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**

- Consumes: period `topOfRed/topOfYellow/topOfGreen`, demand and backend `demandSpikeThreshold` from Task 2.
- Produces: `renderBufferTrendChart(detail)` for the upper chart and `renderBufferVolatilityChart(detail)` for the independent lower chart.
- Constraint: smoothing changes only SVG geometry between backend points and never changes data values.

- [ ] **Step 1: Add failing chart-separation assertions.**

Register `TestFutureBufferChartsUseBackendSizingAndSeparateVolatility`. Assert:

- `#buffer-trend-chart` and `#buffer-volatility-chart` each occur once.
- `renderBufferTrendChart` calls `buildMonotoneAreaPath` and contains no `demand-pulse-bar`, `pulseTop`, or frontend `topOfRed * 0.5` threshold.
- `renderBufferVolatilityChart` reads `item.demand` and `item.demandSpikeThreshold`.
- `renderBufferTrendWorkspace` invokes the two renderers in upper-then-lower order.
- Page text contains`动态红黄绿缓冲带`、`需求波动` and does not contain`订单尖峰阈值` or`需求脉冲`.

- [ ] **Step 2: Run the harness and verify RED.**

Expected: the current combined SVG still contains pulse bars and a frontend-derived threshold.

- [ ] **Step 3: Add a shape-preserving monotone path helper.**

Add these complete helpers. They use shape-preserving cubic Hermite slopes, pass every backend point, clamp a direction change to zero slope, and return an empty path for invalid coordinates:

```javascript
function monotonePathParts(points) {
  const values = points.map(point => ({ x: Number(point.x), y: Number(point.y) }));
  if (values.length === 0 || values.some(point => !Number.isFinite(point.x) || !Number.isFinite(point.y))) return null;
  if (values.length === 1) return { start: values[0], end: values[0], segments: [] };
  if (values.some((point, index) => index > 0 && point.x <= values[index - 1].x)) return null;

  const h = values.slice(0, -1).map((point, index) => values[index + 1].x - point.x);
  const delta = values.slice(0, -1).map((point, index) => (values[index + 1].y - point.y) / h[index]);
  const slopes = new Array(values.length).fill(0);

  const endpointSlope = (h0, h1, delta0, delta1) => {
    let slope = ((2 * h0 + h1) * delta0 - h0 * delta1) / (h0 + h1);
    if (Math.sign(slope) !== Math.sign(delta0)) slope = 0;
    else if (Math.sign(delta0) !== Math.sign(delta1) && Math.abs(slope) > Math.abs(3 * delta0)) slope = 3 * delta0;
    return slope;
  };

  if (values.length === 2) {
    slopes[0] = delta[0];
    slopes[1] = delta[0];
  } else {
    slopes[0] = endpointSlope(h[0], h[1], delta[0], delta[1]);
    slopes[values.length - 1] = endpointSlope(
      h[h.length - 1], h[h.length - 2], delta[delta.length - 1], delta[delta.length - 2]);
    for (let index = 1; index < values.length - 1; index += 1) {
      if (delta[index - 1] === 0 || delta[index] === 0 || Math.sign(delta[index - 1]) !== Math.sign(delta[index])) {
        slopes[index] = 0;
      } else {
        const firstWeight = 2 * h[index] + h[index - 1];
        const secondWeight = h[index] + 2 * h[index - 1];
        slopes[index] = (firstWeight + secondWeight) /
          (firstWeight / delta[index - 1] + secondWeight / delta[index]);
      }
    }
  }

  const segments = values.slice(0, -1).map((point, index) => {
    const next = values[index + 1];
    const width = next.x - point.x;
    const firstControl = { x: point.x + width / 3, y: point.y + slopes[index] * width / 3 };
    const secondControl = { x: next.x - width / 3, y: next.y - slopes[index + 1] * width / 3 };
    return { start: point, firstControl, secondControl, end: next };
  });
  return { start: values[0], end: values[values.length - 1], segments };
}

function buildMonotonePath(points) {
  const parts = monotonePathParts(points);
  if (!parts) return "";
  const commands = parts.segments.map(segment =>
    `C ${segment.firstControl.x},${segment.firstControl.y} ` +
    `${segment.secondControl.x},${segment.secondControl.y} ${segment.end.x},${segment.end.y}`);
  return `M ${parts.start.x},${parts.start.y} ${commands.join(" ")}`;
}

function buildMonotoneAreaPath(lowerPoints, upperPoints) {
  const upper = monotonePathParts(upperPoints);
  const lower = monotonePathParts(lowerPoints);
  if (!upper || !lower || upperPoints.length !== lowerPoints.length) return "";
  const upperCommands = upper.segments.map(segment =>
    `C ${segment.firstControl.x},${segment.firstControl.y} ` +
    `${segment.secondControl.x},${segment.secondControl.y} ${segment.end.x},${segment.end.y}`);
  const reverseLowerCommands = [...lower.segments].reverse().map(segment =>
    `C ${segment.secondControl.x},${segment.secondControl.y} ` +
    `${segment.firstControl.x},${segment.firstControl.y} ${segment.start.x},${segment.start.y}`);
  return `M ${upper.start.x},${upper.start.y} ${upperCommands.join(" ")} ` +
    `L ${lower.end.x},${lower.end.y} ${reverseLowerCommands.join(" ")} Z`;
}
```

Add a static UI test that extracts this helper and checks for the sign-change clamp, harmonic-mean calculation and closed area path. Do not use a chart library.

- [ ] **Step 4: Reduce the upper chart to the dynamic buffer and operating lines.**

In `renderBufferTrendChart`, keep the red/yellow/green areas, net-flow line, baseline/preview inventory lines, target dots, replenishment markers and dates. Replace the three `<polygon>` elements with three `<path>` elements generated from backend zone tops through `buildMonotoneAreaPath`. Remove pulse background, bars, threshold and pulse legend. Set the upper SVG view box height to the main chart plus labels.

- [ ] **Step 5: Implement the independent lower volatility chart.**

`renderBufferVolatilityChart(detail)` must use its own 940×190 SVG, its own y scale, the same period x positions, a filled monotone demand area, a backend threshold line and date labels. A null threshold produces a visible`尖峰阈值证据缺失` note rather than a zero line. Its legend must say`计划需求` and`后端尖峰阈值`.

Call both functions whenever the selected SKU changes. Update the static legend in `Index.cshtml` to separate`动态红黄绿缓冲带` from`需求波动`.

- [ ] **Step 6: Run tests and syntax verification.**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
node --check .\src\AdaptiveSopDdsop.Web\wwwroot\js\app.js
```

Expected: both commands exit 0 and the chart-separation test passes.

- [ ] **Step 7: Commit the future chart unit.**

```powershell
git add src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css src/AdaptiveSopDdsop.Web/Pages/Index.cshtml tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: separate dynamic buffer and volatility charts"
```

---

### Task 9: Complete localization, protection and browser acceptance

**Files:**

- Modify only if a regression is found: files already listed in Tasks 1-8.
- Verify unchanged: all files and blocks covered by `scripts/verify-protected-boundaries.ps1`.
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs` only for a newly reproduced regression.

**Interfaces:**

- Verifies: `/api/history-review?trendMonths=6`, `/api/history-review?trendMonths=12`, `/api/buffer-trend-workspace?horizonWeeks=12`.
- Verifies: all five business stages and both validation pages remain selectable.

- [ ] **Step 1: Run the full automated verification from a fresh process.**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
dotnet build .\AdaptiveSopDdsop.sln --no-restore -m:1
node --check .\src\AdaptiveSopDdsop.Web\wwwroot\js\app.js
.\scripts\verify-protected-boundaries.ps1 -Baseline 4e39ec5
git diff --check
```

Expected: tests report no failures, build reports 0 warnings and 0 errors, JavaScript syntax exits 0, every protected-boundary line prints `PASS`, and `git diff --check` has no output.

- [ ] **Step 2: Start the feature worktree service on an unused port.**

```powershell
$env:Logging__EventLog__LogLevel__Default = 'None'
dotnet run --project .\src\AdaptiveSopDdsop.Web\AdaptiveSopDdsop.Web.csproj --no-restore --urls http://127.0.0.1:5075
```

Expected: the content root is the feature worktree and the service listens on `http://127.0.0.1:5075` without stopping the user's main service on port 5074.

- [ ] **Step 3: Perform browser acceptance.**

Using the in-app browser, verify:

1. 历史回顾展开顺序是`经营结果 → 缓冲表现 → 定容追溯 → 能力约束`，点击只切换主视图。
2. 近六个月与近十二个月均显示严格 26/52 周且结果不同。
3. 缓冲表现可选择库存控制点、物料和时间控制点；图、表和原因联动。
4. 定容追溯按三个独立库存控制点分组；FPGA 只在独立库存控制点出现。
5. 标准定容显示红 80、黄 120、绿 70，历史版本显示生效周段和证据。
6. 能力约束默认选择 CCR 前方保护资源；CCR 自身只显示利用率参照。
7. 未来库存上图是连续起伏的动态红黄绿堆叠面积，下方需求波动图独立存在。
8. 白盒追踪和公开演示闭环入口、按钮及运行行为保持；公开演示仍在验证分组底部。
9. 页面无普通英文状态码、`??`、乱码、自动采纳、自动审批或自动发布操作。

Capture screenshots of the four history child views and the future inventory view for review evidence.

- [ ] **Step 4: Inspect the final diff boundary.**

```powershell
git status --short --branch
git diff --stat f057d41...HEAD
git diff --name-only f057d41...HEAD
```

Expected: only DDAE internal domain/data/UI/test files and the approved docs are listed; no sibling repository, CONTRACT fixture, protected contract implementation or Network file appears.

- [ ] **Step 5: Fix only reproduced acceptance failures with RED/GREEN tests.**

For each observed failure, first add one narrowly named test to `tests/AdaptiveSopDdsop.Tests/Program.cs`, run the full harness to observe its `FAIL`, apply the smallest in-scope correction, then rerun the complete verification set from Step 1. Do not change visual taste or business rules that are not covered by the confirmed design.

- [ ] **Step 6: Commit final acceptance corrections.**

If Step 5 changed files:

```powershell
git add src tests
git commit -m "fix: close history buffer acceptance gaps"
```

If Step 5 changed nothing, do not create an empty commit.

---

## Execution handoff checkpoints

1. After Task 3: review standard formula, dynamic order/chart consistency and legacy baseline behavior.
2. After Task 5: review historical fact authority, snapshot matching and FPGA/capacity boundaries.
3. After Task 8: review all history views plus the separated future charts.
4. After Task 9: review automated evidence, browser screenshots and final protected-boundary diff before merge or push.
