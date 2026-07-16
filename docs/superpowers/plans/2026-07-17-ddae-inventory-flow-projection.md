# DDAE 内部库存流量投影与缓冲语义修正 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在不触碰 CONTRACT、SDBR、Network 或完整执行排程的前提下，为 DDAE 增加周粒度物理库存流量投影，并修正历史库存、未来库存、时间缓冲、能力保护与标准算例的业务语义。

**Architecture:** 保留 `DemandDrivenPlanningEngine` 作为 DDMRP 净流动量信号引擎；新增 `InventoryFlowProjectionService`，在 `ScenarioRunPreviewService` 中并行计算物理现有量、积压、三类到货和库存金额。冻结基线用尾部可选 DTO 固化 52 周需求覆盖、确认到货和期初积压；旧快照继续返回原有分析，但物理投影明确返回 `EvidenceMissing`。前端只渲染后端结果，不复制业务公式。

**Tech Stack:** .NET 9 / C#、ASP.NET Core Razor Pages、SQLite JSON、原生 JavaScript、HTML/CSS、现有 SVG 图形、控制台测试 harness、Node.js fixtures。

## Global Constraints

- 只修改 `C:\Users\吴一帆\Documents\DDAE`；不修改同级 `DDAE_INTERFACE_CONTRACT` 和 `DDAE-NetworkStructure`。
- 不修改 SDBR 字段、状态、ACK、错误码、JSON 形状、端点、样例、fixtures 或契约测试。
- 不新增外部导入、外部协议、网络评分、完整执行排程、自动采纳、自动审批、自动生效或自动发布。
- `SimulatedReplenishment`、`PrebuildResponse` 只存在于投影日志/trace，不能成为冻结基线事实。
- FPGA 只属于库存控制点；不进入时间缓冲或能力保护。CCR 只显示利用率参考，不计算自身保护消耗。
- 能力颜色固定为 `0–60%` 绿、`>60–80%` 黄、`>80–100%` 红、`>100%` 深红。
- 能力颜色分段与保护消耗分开：保护起点固定为计划可用能力的 `80%`，保护能力固定为 `20%`。
- 新 record 参数只能追加在尾部并提供默认值，保持旧构造器与旧 JSON 兼容。
- SQLite 继续使用 `payload_json`、`result_json` 与只追加审计链；不删除、重建或新增周明细表。
- 不引入前端图表依赖；主导航继续点击切换。
- `#trace-panel`、公开演示闭环、受保护 DOM/JS 和 integration-contract 端点块保持原样。
- 每个任务先写失败测试，再最小实现，再完整验证并独立提交。

---

## File Map

### Create

- `Domain/PlanningEvidenceModels.cs`：确认到货、期初积压、覆盖和问题 DTO。
- `Domain/PlanningEvidenceValidator.cs`：冻结与投影证据验证。
- `Domain/InventoryFlowProjectionModels.cs`：物理库存结果、摘要、日志和 `ScenarioMetricEvidence`。
- `Domain/InventoryFlowProjectionService.cs`：守恒账本和模拟到货分配。
- `Domain/CapacityProtectionMath.cs`：历史/未来共享能力公式。
- `Domain/DdmrpStandardReferenceService.cs`：内部标准 DDMRP 算例。
- `tests/.../Js/future-inventory-flow-charts.fixture.mjs`
- `tests/.../Js/ddmrp-standard-reference.fixture.mjs`

### Modify

- `Domain/ScenarioWorkspaceData.cs`
- `Data/SeedData.cs`
- `Data/SeedScenarioWorkspaceDataSource.cs`
- `Data/SeedCurrentBaselineDataSource.cs`
- `Domain/CurrentBaselineService.cs`
- `Domain/ScenarioRunPreviewService.cs`
- `Domain/ProductFamilyDashboardService.cs`
- `Domain/BufferTrendWorkspaceService.cs`
- `Domain/ScenarioComparisonService.cs`
- `Domain/HistoryOperatingFacts.cs`
- `Domain/HistoryReviewModels.cs`
- `Domain/HistoryReviewProjectionBuilder.cs`
- `Domain/HistoryReviewWorkspaceService.cs`
- `Data/SeedHistoryOperatingFactSource.cs`
- `Domain/ProtectionAnalysis.cs`
- `Program.cs`、`Pages/Index.cshtml`、`wwwroot/js/app.js`、`wwwroot/css/site.css`
- `tests/AdaptiveSopDdsop.Tests/Program.cs`
- `tests/AdaptiveSopDdsop.Tests/Js/history-buffer-renderers.fixture.mjs`

### Never modify

- `DdsopConfigInboundContract.cs`、`DdsopRuntimePlanningInputContract.cs`
- `ProductionInventoryQualityEvidenceContract.cs`、`ProductionSupplierIdentitySourceContract.cs`
- `SdbrExecutionObjectEvidenceContract.cs`、`PublicDemoGoldenLoopService.cs`
- `AdventureWorksProductDemoProfileService.cs`、`ContractRepositoryPathResolver.cs`
- `appsettings.json`、两个 SDBR fixture JSON、十个现有契约测试函数体
- `Program.cs` integration-contract 保护块
- `Index.cshtml` trace/public-demo/Network 保护字节
- `app.js` trace 与 public-demo 保护函数

---

### Task 1: 冻结规划证据模型与验证器

**Files:**
- Create: `src/AdaptiveSopDdsop.Web/Domain/PlanningEvidenceModels.cs`
- Create: `src/AdaptiveSopDdsop.Web/Domain/PlanningEvidenceValidator.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs:871-891`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:** Produces `ValidateForFreeze`、`ValidateForProjection` and three optional workspace tail fields.

- [ ] **Step 1: Add failing tests**

Register tests for complete explicit 52 weeks、missing/duplicate/negative/expired evidence、receipt date/week mismatch、left-closed/right-open bucket boundaries、Asia/Shanghai timestamp conversion、out-of-coverage open receipts、generated receipt type rejection and legacy JSON null tails.

```csharp
("Planning evidence accepts complete 52 weeks", TestPlanningEvidenceAcceptsCompleteCoverage),
("Planning evidence rejects demand gaps", TestPlanningEvidenceRejectsDemandGaps),
("Planning evidence rejects receipt date mismatch", TestPlanningEvidenceRejectsReceiptDateMismatch),
("Frozen evidence rejects generated receipts", TestFrozenEvidenceRejectsGeneratedReceipts),
("Planning evidence preserves legacy JSON", TestPlanningEvidencePreservesLegacyJson),
```

- [ ] **Step 2: Run red test**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: missing type/service compile errors.

- [ ] **Step 3: Add exact DTOs**

```csharp
public sealed record PlanningEvidenceCoverage(DateOnly AnchorDate, int CoverageFromWeek, int CoverageThroughWeek, string EvidenceStatus);
public sealed record ConfirmedReceiptEvidence(
    string ReceiptId, string Sku, decimal Quantity, int ExpectedReceiptWeek,
    DateOnly? ExpectedReceiptDate, string ReceiptType, string SourceReference,
    string SupplySourceType, string? Supplier, string? MaterialFamily,
    string ConfirmationStatus, string EvidenceStatus, string AsOfUtc,
    string EvidenceLabel, string? SourceTimestampUtc = null);
public sealed record OpeningBacklogEvidence(
    string BacklogId, string Sku, decimal Quantity, string SourceReference,
    string EvidenceStatus, string AsOfUtc, string EvidenceLabel);
public sealed record PlanningEvidenceIssue(
    string Scope, string? Sku, int? Week, string Reason, string? SourceId,
    bool BlocksFreeze, bool BlocksProjection);
public sealed record PlanningEvidenceValidationResult(string Status, IReadOnlyList<PlanningEvidenceIssue> Issues);
```

Append to `ScenarioWorkspaceDataSet`:

```csharp
IReadOnlyList<ConfirmedReceiptEvidence>? ConfirmedReceipts = null,
IReadOnlyList<OpeningBacklogEvidence>? OpeningBacklog = null,
PlanningEvidenceCoverage? PlanningEvidenceCoverage = null
```

- [ ] **Step 4: Implement validation**

`ValidateForFreeze` requires anchor equality、coverage `1..52/Complete`、one inventory/backlog row per SKU、one nonnegative demand row per SKU/week、unique receipt ID、allowed receipt type、`Confirmed/Complete` status、valid week/date、fresh evidence、required supplier/material-family plus weekly capacity mapping、receipt total matching `OpenSupply` within `0.01m` and complete DDMRP parameters. `ValidateForProjection` applies the same rules through requested horizon. Missing evidence returns issues; never zero.

`ExpectedReceiptWeek` remains authoritative and may be greater than 52. Preserve every unreceived open-supply row beyond demand coverage and mark it `OutsideCoverage`; do not truncate it or inject it into the 52-week ledger. Convert source timestamps to business dates in `Asia/Shanghai` exactly once in the adapter and retain the original timestamp in `SourceTimestampUtc`.

```csharp
public static int WeekForDate(DateOnly anchor, DateOnly date)
{
    var days = date.DayNumber - anchor.DayNumber;
    return days < 0 ? 0 : days / 7 + 1;
}
```

- [ ] **Step 5: Verify and commit**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Domain/PlanningEvidenceModels.cs src/AdaptiveSopDdsop.Web/Domain/PlanningEvidenceValidator.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: define frozen planning evidence"
```

---

### Task 2: 建立 52 周演示事实并冻结到基线

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Data/SeedData.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Data/SeedScenarioWorkspaceDataSource.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Data/SeedCurrentBaselineDataSource.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/CurrentBaselineService.cs`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:** Produces a single explicit annual DemoFixture fact set and immutable frozen planning evidence.

- [ ] **Step 1: Add failing seed and baseline tests**

```csharp
("Seed planning evidence has 52 weeks", TestSeedPlanningEvidenceHasFiftyTwoWeeks),
("AV-COM-201 sizing is calibrated", TestAvCom201SizingIsCalibrated),
("Baseline derives typed planning evidence", TestBaselineDerivesTypedPlanningEvidence),
("Baseline freeze rejects incomplete planning evidence", TestBaselineFreezeRejectsIncompletePlanningEvidence),
("Frozen planning evidence round trips", TestFrozenPlanningEvidenceRoundTrips),
```

Assert every governed SKU has exactly weeks `1..52` including explicit zeroes; the annual sequence is not a repeated 4/8-week template; AV-COM-201 has MOQ `12` and standard zones `8/9/13`; candidate summaries are derived from typed evidence, not `Demand[week 1]` or `InventoryPosition.QualifiedDemand`.

- [ ] **Step 2: Run red test**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: missing planning-evidence fixtures and baseline validation failures.

- [ ] **Step 3: Build one coherent DemoFixture**

Use one annual demand fact set per SKU. Preserve explicit zero weeks, extend every governed `SkuBufferSetting.EffectiveThroughWeek` to 52, and set `PlanningEvidenceCoverage(anchor, 1, 52, "Complete")`. Derive the anchor as a timezone-free business date after converting source cutoff timestamps to `Asia/Shanghai`. Generate frozen receipts with:

- arrival week `max(1, ceil(DLT / 7))`;
- `ExpectedReceiptDate = anchor + 7 * (week - 1) + 2 days` so it lies inside the authoritative bucket;
- `ConfirmedInTransit` for weeks `<= 2`, otherwise `ConfirmedOpenSupply`;
- `Confirmed/Complete` statuses and supplier/material mapping for external supply;
- unique source references and truthful DemoFixture labels.

Include every unreceived open-supply row even when its expected week is later than 52; keep its authoritative week and `OutsideCoverage` presentation state while still reconciling its quantity to `OpenSupply`.

Extend every supplier/material-family commitment window required by a governed external SKU through week 52 with complete, unique and nonnegative evidence; do not let a 12-week capacity fixture masquerade as complete annual coverage.

Create one explicit opening-backlog row for every SKU. Use nonzero quantities only for `PAY-SAR = 1`, `AV-FPGA = 2` and `CBL-HAR = 8`; all other rows are explicit zero evidence.

- [ ] **Step 4: Freeze typed evidence and audit failures**

`SeedCurrentBaselineDataSource` exposes receipt, backlog and coverage sections with source, cutoff, freshness and completeness. `CurrentBaselineService.Freeze` calls `ValidateForFreeze` before persistence; any blocking issue prevents snapshot creation and returns the existing validation failure without pretending an audit row was stored. On success, copy all three fields into immutable snapshot JSON, increment the version and use the existing successful-freeze audit chain. Do not add or rebuild SQLite tables.

- [ ] **Step 5: Verify and commit**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Data/SeedData.cs src/AdaptiveSopDdsop.Web/Data/SeedScenarioWorkspaceDataSource.cs src/AdaptiveSopDdsop.Web/Data/SeedCurrentBaselineDataSource.cs src/AdaptiveSopDdsop.Web/Domain/CurrentBaselineService.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: freeze complete planning evidence"
```

---

### Task 3: 实现物理库存守恒投影

**Files:**
- Create: `src/AdaptiveSopDdsop.Web/Domain/InventoryFlowProjectionModels.cs`
- Create: `src/AdaptiveSopDdsop.Web/Domain/InventoryFlowProjectionService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:** Produces weekly physical on-hand/backlog, source-separated receipt logs, SKU summaries, trace and evidence metadata without replacing NFP.

- [ ] **Step 1: Add failing conservation tests**

```csharp
("Inventory flow conserves weekly quantity", TestInventoryFlowConservesWeeklyQuantity),
("Inventory flow fulfills oldest demand first", TestInventoryFlowFulfillsOldestDemandFirst),
("Simulated receipt respects DLT arrival", TestSimulatedReceiptRespectsDltArrival),
("Inventory flow separates receipt sources", TestInventoryFlowSeparatesReceiptSources),
("Prebuild receipt is counted once", TestPrebuildReceiptIsCountedOnce),
("Zero new demand service is not applicable", TestZeroDemandServiceIsNotApplicable),
("Projection beyond coverage is evidence missing", TestProjectionBeyondCoverageIsEvidenceMissing),
```

- [ ] **Step 2: Run red test**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: missing projection DTO/service compile errors.

- [ ] **Step 3: Add result records**

Define immutable records for:

- `InventoryFlowPoint`: SKU/week, opening on-hand/backlog, demand, frozen/simulated/prebuild receipts, fulfilled old backlog, on-time fulfilled new demand, total fulfilled demand, ending on-hand/backlog, ending inventory value and weekly service status;
- `InventoryReceiptLogEntry`: source kind, source ID, recommendation week, arrival week, requested/accepted/deferred/outside-horizon quantity and evidence;
- `InventoryFlowSkuSummary` and `InventoryFlowSummary`: physical on-time service, total fulfilled quantity, average/peak/ending inventory value, ending backlog, backlog-recovery week, receipt-source totals and outside-horizon quantity;
- `InventoryFlowTrace`: validated inputs, bucket equations and allocation decisions;
- `InventoryFlowProjectionResult`: `Status`, points, receipt log, summaries, trace and issues;
- `ScenarioMetricEvidence`: JSON path, evidence status, source and explanation.

Append optional physical fields at the tails of scenario result/metric/comparison records; default all new fields to `null` for legacy JSON and constructor compatibility.

- [ ] **Step 4: Implement the deterministic ledger**

```csharp
public static InventoryFlowProjectionResult Project(
    ScenarioWorkspaceDataSet data,
    string caseId,
    IReadOnlyList<SkuBufferSetting> skus,
    IReadOnlyList<WeeklyDemand> demand,
    IReadOnlyList<ProjectedReplenishmentOrder> orders,
    IReadOnlyList<PrebuildCampaign> prebuild,
    IReadOnlyList<SupplierCapacityLimit> supplierLimits,
    string? baselineSnapshotId = null)
```

For each SKU/week, in stable SKU/week/source-ID order:

```text
due       = opening backlog + current demand
available = opening on-hand + frozen receipts + simulated receipts + prebuild receipts
fulfilled = min(due, available)
ending backlog = max(0, due - fulfilled)
ending on-hand = max(0, available - fulfilled)
```

Record fulfillment in two fields: `FulfilledOpeningBacklog = min(opening backlog, fulfilled)` and `FulfilledNewDemandOnTime = min(current demand, fulfilled - fulfilled opening backlog)`. The summary service rate is total on-time fulfilled new demand divided by total new demand; when total new demand is zero it is `null / NotApplicable`, not 100%. Compute ending inventory value from ending physical on-hand times unit cost.

Simulated replenishment arrives at `recommendation week + max(1, ceil(DLT / 7))` and is labeled `SimulationAssumption`. Exclude every order whose `Trigger == "PrebuildCampaign"` from simulated receipts, then add the response campaign exactly once at its configured completion week as `ResponseAssumption`. Never deduct `InventoryPosition.QualifiedDemand` again.

If validation fails or the requested horizon exceeds complete evidence, return `EvidenceMissing` with issues and no fabricated points.

- [ ] **Step 5: Verify and commit**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Domain/InventoryFlowProjectionModels.cs src/AdaptiveSopDdsop.Web/Domain/InventoryFlowProjectionService.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: project physical inventory flow"
```

---

### Task 4: 约束模拟到货的供应能力

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Domain/InventoryFlowProjectionService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/InventoryFlowProjectionModels.cs`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:** Consumes only DDAE-internal supplier-capacity facts and constrains only `SimulatedReplenishment`.

- [ ] **Step 1: Add failing allocation tests**

```csharp
("Supplier capacity constrains simulated receipts only", TestSupplierCapacityConstrainsSimulatedOnly),
("Supplier capacity allocates proportionally", TestSupplierCapacityAllocatesProportionally),
("Deferred simulated receipt carries forward", TestDeferredSimulatedReceiptCarriesForward),
("Frozen receipts remain fixed under capacity loss", TestFrozenReceiptsRemainFixed),
("Prebuild remains unchanged under supplier limit", TestPrebuildRemainsUnchanged),
("Missing constrained supply mapping is not unlimited", TestMissingConstrainedSupplyMappingIsEvidenceMissing),
("Missing constrained capacity week is not unlimited", TestMissingConstrainedCapacityWeekIsEvidenceMissing),
```

Use a same-supplier example with requested `80/20` and capacity `50`; assert accepted `40/10`, deterministic residual handling and source-log trace.

- [ ] **Step 2: Run red test**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: simulated receipts currently ignore supplier capacity.

- [ ] **Step 3: Implement internal proportional allocation**

Use the already combined week-level limit produced by existing scenario logic (frozen commitment, external supply-risk multiplier and response limit). A constrained supplier/material family with missing mapping or capacity week returns `EvidenceMissing`; only an explicitly internal/unconstrained `NotApplicable` source is unbounded.

Group candidates by supplier/material family and arrival week, allocate proportionally to requested quantity using `decimal` arithmetic, and round only at the established output precision. Assign the final rounding residual to the last stable source ID and record it in trace so allocated totals equal available capacity.

Carry rejected simulated quantity into the next week while inside the projection horizon; log quantities beyond the horizon as `OutsideHorizon`. Do not move, reduce or relabel `ConfirmedInTransit`, `ConfirmedOpenSupply` or `PrebuildResponse`. Record RCCP/supply infeasibility as result evidence only; do not generate an execution schedule or external message.

- [ ] **Step 4: Verify and commit**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Domain/InventoryFlowProjectionService.cs src/AdaptiveSopDdsop.Web/Domain/InventoryFlowProjectionModels.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: constrain simulated receipt allocation"
```

---

### Task 5: 将物理口径接入场景预览、比较与持久化

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ProductFamilyDashboardService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/BufferTrendWorkspaceService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioComparisonService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPersistenceService.cs`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:** Keeps the existing preview endpoint compatible while adding optional `InventoryFlow` and path-addressed evidence.

- [ ] **Step 1: Add failing integration and legacy tests**

```csharp
("Preview returns complete physical inventory flow", TestPreviewReturnsCompleteInventoryFlow),
("Physical flow drives inventory metrics and budget", TestPhysicalFlowDrivesMetricsAndBudget),
("Legacy preview keeps legacy reference labels", TestLegacyPreviewKeepsLegacyReference),
("Comparison omits physical delta when evidence missing", TestComparisonOmitsIncompletePhysicalDelta),
("Frozen comparison preserves baseline lineage and source evidence", TestFrozenComparisonPreservesBaselineLineageAndEvidence),
("Inventory flow result JSON round trips", TestInventoryFlowResultJsonRoundTrips),
```

- [ ] **Step 2: Run red test**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: preview has no physical result and old builders still own inventory/cash values.

- [ ] **Step 3: Integrate after NFP calculation**

Keep `DemandDrivenPlanningEngine.ProjectBuffers` unchanged. Immediately after it returns, call `InventoryFlowProjectionService.Project` with frozen evidence, DDMRP order recommendations, response prebuild and scenario supplier limits. The immutable baseline `payload_json` retains all 52 demand weeks, every confirmed receipt, opening backlog and `1..52` coverage; an active preview case may crop demand/receipts to its requested horizon while retaining the same coverage and baseline lineage. Never mutate or duplicate the frozen payload into each result.

When `InventoryFlow.Status == "Complete"`, use it for top-level and product-family physical service, inventory value, cash occupation, budget inventory and every product-family/BufferTrend inventory aggregation. Keep NFP, buffer penetration and order-signal metrics on the existing engine.

Attach evidence entries to exact paths:

- `metrics.serviceLevelPercent`
- `metrics.averageInventoryValue`
- `budget[*].projectedInventoryValue`
- `budget[*].budgetInventoryVariance`
- `productFamilyDashboard.summaries[*].serviceLevelPercent`
- `productFamilyDashboard.summaries[*].averageInventoryValue`
- `productFamilyDashboard.summaries[*].peakInventoryValue`
- `productFamilyDashboard.summaries[*].budgetInventoryVariance`
- `productFamilyDashboard.details[*].weeklyCells[*].inventoryValue`
- `productFamilyDashboard.details[*].weeklyCells[*].budgetInventoryVariance`
- `productFamilyDashboard.weeklyCells[*].inventoryValue`
- `productFamilyDashboard.weeklyCells[*].budgetInventoryVariance`
- `bufferTrend.kpis.averageInventoryValue`
- `bufferTrend.kpis.peakInventoryValue`
- `bufferTrend.kpis.inventoryValueDelta`
- `bufferTrend.series[*].inventoryValue`
- `bufferTrend.familySummaries[*].averageInventoryValue`
- `bufferTrend.weeklyCells[*].inventoryValue`

Also cover every existing response path displayed as cash occupation; where cash is represented by inventory value, use the same physical source but a separate path-addressed evidence entry. For complete data label all four compatible categories (projected service, physical inventory, cash occupation and inventory-budget variance) `PhysicalProjection`. For old snapshots, retain existing non-null compatibility numbers but label each path `LegacyReference`, return `InventoryFlow.Status = EvidenceMissing`, and never present a legacy value as a new physical fact. Do not relabel RCCP, supply, time, capacity or NFP evidence.

- [ ] **Step 4: Preserve comparison and persistence semantics**

Compute physical deltas only when both compared cases have complete physical evidence; otherwise set those deltas to `null` with an explanation. Persist optional result fields in existing `result_json` and snapshot evidence in existing `payload_json`; no SQL migration or table rebuild.

- [ ] **Step 5: Verify and commit**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs src/AdaptiveSopDdsop.Web/Domain/ProductFamilyDashboardService.cs src/AdaptiveSopDdsop.Web/Domain/BufferTrendWorkspaceService.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioComparisonService.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPersistenceService.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: expose physical inventory scenario evidence"
```

---

### Task 6: 在当前基线展示可冻结证据

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:** Reads candidate/frozen baseline DTOs only; it does not calculate or repair missing evidence in the browser.

- [ ] **Step 1: Add failing DOM/label tests**

Add assertions for `#baseline-coverage-evidence`, `#baseline-receipt-evidence` and `#baseline-backlog-evidence`; assert Chinese headings are at most six characters, missing evidence is explained, and no client-side zero fallback appears.

- [ ] **Step 2: Run red tests**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: the three typed evidence sections are absent.

- [ ] **Step 3: Add compact evidence cards**

Within the current-baseline stage, add sibling sections titled `覆盖证据`、`确认到货`、`期初积压`. For each receipt show SKU, quantity, expected week, type, source, confirmation, cutoff and evidence status; show coverage range/anchor, backlog rows, freshness, completeness and blocking issues. Frozen views show snapshot/version and immutable status; candidate views retain the existing freeze action and keep it disabled while any blocker exists.

Render only backend values. Missing evidence must not be backfilled or displayed as zero; a `Complete` row whose recorded quantity is explicitly zero must display truthful `0`. A missing SKU/week/quantity shows `证据缺失` plus its reason. Localize ordinary UI codes (`DemoFixture` → `演示数据`, status values to existing Chinese labels) without changing stored values. Keep existing navigation IDs and all validation pages unchanged.

- [ ] **Step 4: Verify and commit**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
node --check .\src\AdaptiveSopDdsop.Web\wwwroot\js\app.js
.\scripts\verify-protected-boundaries.ps1 -Baseline 4e39ec5
git diff --check
git add src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: show baseline planning evidence"
```

---

### Task 7: 将未来库存拆成三个同步证据面板

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`
- Create: `tests/AdaptiveSopDdsop.Tests/Js/future-inventory-flow-charts.fixture.mjs`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:** Renders `BufferTrends` and `InventoryFlow` as different facts; all formulas stay in backend services.

- [ ] **Step 1: Add failing JS fixture and DOM tests**

Fixture assertions:

- `#buffer-trend-chart` remains the NFP panel;
- `#inventory-flow-evidence` and `#inventory-flow-chart` exist as the physical panel;
- `#buffer-volatility-chart` remains an independent volatility panel;
- changing SKU/case/week range redraws all three from one selection;
- `EvidenceMissing` draws no physical line and exposes the backend issue;
- a missing individual week creates a labeled gap and no polyline crosses it;
- fixed color-to-field mapping and independent y-axes are present;
- table/detail selection retains a link to the corresponding white-box record;
- protected `renderPreviewTrace` and `renderTrace` bodies are unchanged.

- [ ] **Step 2: Run red tests**

```powershell
node .\tests\AdaptiveSopDdsop.Tests\Js\future-inventory-flow-charts.fixture.mjs
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: physical chart host/renderer is missing.

- [ ] **Step 3: Render three semantically separate panels**

1. `净流动量`: dynamic red/yellow/green buffer areas from backend trend points, black pre-replenishment NFP, blue post-replenishment NFP and white target markers.
2. `物理库存`: backend-only deep-green ending on-hand line/area, dark-blue frozen-confirmed receipt bars, light-blue simulated receipt bars, purple prebuild bars and red ending-backlog bars; do not reconstruct weekly conservation in JavaScript.
3. `需求波动`: retain the independent demand/threshold chart so volatility is not mistaken for a buffer zone.

Use one shared selection state and one x-domain but independent y-axes. Existing table/detail selection updates all three and must retain a link to the corresponding white-box record without changing the protected trace renderer. Add a compact evidence strip identifying physical projection status, baseline version and trace source. Hide legacy physical metric labels when the result is `LegacyReference`; leave the original NFP evidence visible.

- [ ] **Step 4: Verify and commit**

```powershell
node .\tests\AdaptiveSopDdsop.Tests\Js\future-inventory-flow-charts.fixture.mjs
node --check .\src\AdaptiveSopDdsop.Web\wwwroot\js\app.js
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
.\scripts\verify-protected-boundaries.ps1 -Baseline 4e39ec5
git diff --check
git add src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests/Js/future-inventory-flow-charts.fixture.mjs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: separate future NFP and physical inventory"
```

---

### Task 8: 用一套可追溯年度事实重建历史数据

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryOperatingFacts.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Data/SeedHistoryOperatingFactSource.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewWorkspaceService.cs`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:** Produces one irregular 52-week historical fact set; six months is its trailing 26-week view, not a second fabricated series.

- [ ] **Step 1: Add failing historical fact tests**

```csharp
("History annual facts contain 52 irregular weeks", TestHistoryAnnualFactsContainFiftyTwoIrregularWeeks),
("History six month view is annual tail", TestHistorySixMonthViewIsAnnualTail),
("History facts avoid repeated fixture cycles", TestHistoryFactsAvoidRepeatedFixtureCycles),
("History event costs are explicit evidence", TestHistoryEventCostsAreExplicitEvidence),
("History magnitudes reconcile across views", TestHistoryMagnitudesReconcileAcrossViews),
```

Append optional `ActualDemand`, `DemandSpikeThreshold` and `TargetNetFlowPosition` to weekly operating facts. Assert 6-month values exactly equal annual weeks 27–52 and reject the current `%4/%8` periodic template.

- [ ] **Step 2: Run red test**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: annual facts are repetitive and lack explicit demand/target evidence.

- [ ] **Step 3: Replace repeated generators with named events**

Build deterministic but irregular weekly facts around these offsets from the history cutoff:

- `-46` demand change;
- `-39` import delay;
- `-33` AIT capacity loss;
- `-29` recovery;
- `-21` peak demand;
- `-16` rework;
- `-11` supply recovery;
- `-6` pacing recovery.

Attach abnormal costs only to evidence-bearing events: `240,000`, `180,000`, `360,000` and `420,000` yuan respectively where applicable; all other weeks have explicit no-event status, not a repeated placeholder. Keep service, on-hand, NFP, backlog, WIP, capacity loss and recovery changes internally coherent and in plausible demo magnitudes.

`HistoryReviewWorkspaceService` always requests the annual set, then returns either all 52 weeks or the trailing 26 weeks. Do not generate separate 6/12-month facts. Keep the selected-object cumulative-lead-time detail window as its own 3-week evidence label; never describe the whole 26/52-week trend as 3 weeks.

- [ ] **Step 4: Verify and commit**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
git diff --check
git add src/AdaptiveSopDdsop.Web/Domain/HistoryOperatingFacts.cs src/AdaptiveSopDdsop.Web/Data/SeedHistoryOperatingFactSource.cs src/AdaptiveSopDdsop.Web/Domain/HistoryReviewWorkspaceService.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "fix: use traceable annual history facts"
```

---

### Task 9: 修正历史库存山形图与时间事件表达

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewModels.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewProjectionBuilder.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`
- Modify: `tests/AdaptiveSopDdsop.Tests/Js/history-buffer-renderers.fixture.mjs`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:** Separates inventory positioning, demand volatility, time-buffer status and abnormal-cost events.

- [ ] **Step 1: Add failing projection and renderer tests**

Test new `HistoryAbnormalCostEventView` and optional event collection on the time view. Assert these DOM hosts:

- `#history-inventory-volatility-chart`
- `#history-time-status-chart`
- `#history-time-cost-strip`

Renderer fixture must verify continuous stacked buffer boundaries, gaps on missing evidence, separate demand threshold, five-color time bars, no abnormal-cost polyline, complete date/amount/reason/control-point/evidence event fields, and selected control-point/SKU/snapshot labels. It must assert the trend title is `26 周历史趋势` or `52 周历史趋势`, the selected-object detail is separately labeled `累计提前期详细证据窗口：3 周`, and the whole trend never displays `3 周证据`.

- [ ] **Step 2: Run red tests**

```powershell
node .\tests\AdaptiveSopDdsop.Tests\Js\history-buffer-renderers.fixture.mjs
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: old inventory zones are flat/repetitive and time cost is mixed into the status chart.

- [ ] **Step 3: Build two inventory charts from annual facts**

Upper chart: continuous stacked red/yellow/green zone areas whose weekly heights come from the selected historical snapshot, overlaid with actual on-hand, NFP and target position. Do not repeat triangular peaks or interpolate through missing evidence.

Lower chart: actual weekly demand as the retained independent volatility area/bar view with its backend demand-spike threshold. Keep both charts synchronized to the selected control point, material/SKU, snapshot and 6/12-month range.

Render the 26/52-week title from the selected historical range. Move the 3-week wording to the cumulative-lead-time detail label only; remove it from the upper/lower trend header.

- [ ] **Step 4: Separate time status from cost events**

Render the time-buffer chart as five-status weekly bars only (`蓝/绿/黄/红/深红`). Render each abnormal-cost fact below it as a dated event card/marker containing date, amount, reason, control point and evidence source. Remove the cost curve and any implication that currency shares the status axis.

- [ ] **Step 5: Verify and commit**

```powershell
node .\tests\AdaptiveSopDdsop.Tests\Js\history-buffer-renderers.fixture.mjs
node --check .\src\AdaptiveSopDdsop.Web\wwwroot\js\app.js
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
.\scripts\verify-protected-boundaries.ps1 -Baseline 4e39ec5
git diff --check
git add src/AdaptiveSopDdsop.Web/Domain/HistoryReviewModels.cs src/AdaptiveSopDdsop.Web/Domain/HistoryReviewProjectionBuilder.cs src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests/Js/history-buffer-renderers.fixture.mjs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "fix: separate history buffer evidence charts"
```

---

### Task 10: 统一上游能力保护公式与颜色

**Files:**
- Create: `src/AdaptiveSopDdsop.Web/Domain/CapacityProtectionMath.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ProtectionAnalysis.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewModels.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewProjectionBuilder.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/Models.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:** Produces one backend-owned `CapacityProtectionMeasure` used by historical and future views.

- [ ] **Step 1: Add failing formula and eligibility tests**

```csharp
("Capacity protection math uses 80 percent start", TestCapacityProtectionStartsAtEightyPercent),
("Capacity protection bands honor exact boundaries", TestCapacityProtectionBandsHonorBoundaries),
("Capacity protection separates consumption and overload", TestCapacityProtectionSeparatesConsumptionAndOverload),
("Capacity protection requires upstream CCR evidence", TestCapacityProtectionRequiresUpstreamEvidence),
("CCR utilization is reference only", TestCcrUtilizationIsReferenceOnly),
("FPGA is excluded from capacity and time protection", TestFpgaIsInventoryOnlyControlPoint),
("History and future capacity measures agree", TestCapacityMeasuresAgreeAcrossViews),
("Capacity A layout pairs AIT upstream with HARNESS CCR", TestCapacityALayoutPairsUpstreamAndCcr),
```

Cover exactly `60%`, `80%`, `100%` and `>100%`, missing/zero capacity, eligible upstream routing, self-referential CCR and FPGA.

- [ ] **Step 2: Run red test**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: history/future duplicate formula differences and incorrect bands.

- [ ] **Step 3: Implement the shared calculation**

```csharp
public sealed record CapacityProtectionMeasure(
    decimal? UtilizationPercent,
    decimal? ProtectionStart,
    decimal? ProtectionCapacity,
    decimal? ConsumedProtection,
    decimal? RemainingProtection,
    decimal? Overload,
    string UtilizationBand,
    string EvidenceStatus,
    string? EvidenceIssue);
```

For eligible upstream resources with complete routing and planned-available-capacity evidence:

```text
utilization percent = committed load / planned available capacity * 100
protection start    = planned available capacity * 0.80
protection capacity = planned available capacity - protection start
consumed protection = clamp(committed load - protection start, 0, protection capacity)
remaining protection = max(0, protection capacity - consumed protection)
overload            = max(0, committed load - planned available capacity)
```

Bands are exactly `0..60 Green`, `>60..80 Yellow`, `>80..100 Red` and `>100 DeepRed`. Return `EvidenceMissing` with nullable mapped values when planned capacity or a later protected-CCR routing relationship is absent; do not infer zero.

Append any new mapped DTO fields at record tails. Show theoretical, standard, verified, planned available, committed load, protection capacity, consumed protection, remaining protection and loss reason as distinct columns. CCR rows expose utilization and threshold reference only; all protection values are not applicable. FPGA appears only in inventory-control views.

- [ ] **Step 4: Update the visual hierarchy**

Use backend `UtilizationBand` classes only. Implement the confirmed A layout: the top row must place AIT eligible upstream protection beside the HARNESS CCR utilization reference, and the bottom must show the distribution of every eligible upstream observation across the period. The upstream card names its protected CCR; the CCR card is visually distinct and has no protection consumption. Do not draw client-side threshold formulas or label the CCR itself as consumed protection.

- [ ] **Step 5: Verify and commit**

```powershell
node .\tests\AdaptiveSopDdsop.Tests\Js\history-buffer-renderers.fixture.mjs
node .\tests\AdaptiveSopDdsop.Tests\Js\future-inventory-flow-charts.fixture.mjs
node --check .\src\AdaptiveSopDdsop.Web\wwwroot\js\app.js
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
.\scripts\verify-protected-boundaries.ps1 -Baseline 4e39ec5
git diff --check
git add src/AdaptiveSopDdsop.Web/Domain/CapacityProtectionMath.cs src/AdaptiveSopDdsop.Web/Domain/ProtectionAnalysis.cs src/AdaptiveSopDdsop.Web/Domain/HistoryReviewModels.cs src/AdaptiveSopDdsop.Web/Domain/HistoryReviewProjectionBuilder.cs src/AdaptiveSopDdsop.Web/Domain/Models.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "fix: unify capacity protection semantics"
```

---

### Task 11: 将标准 DDMRP 算例移到白盒宿主

**Files:**
- Create: `src/AdaptiveSopDdsop.Web/Domain/DdmrpStandardReferenceService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Program.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/HistoryReviewWorkspaceService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`
- Create: `tests/AdaptiveSopDdsop.Tests/Js/ddmrp-standard-reference.fixture.mjs`
- Test: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:** Adds internal `GET /api/ddmrp-standard-reference` only; it has no CONTRACT/SDBR/Network DTO or adapter.

- [ ] **Step 1: Add failing service, endpoint and placement tests**

Assert the canonical example:

```text
ADU 10; DLT 12 days; lead-time factor 0.5; variability factor 0.33
MOQ 50; desired order cycle 7 days; DAF 1; zone adjustment 1
red 80; yellow 120; green 70; total buffer 270
```

The JS fixture asserts one closed-by-default `<details id="ddmrp-standard-reference-panel">` inside `#saved-scenarios-panel` immediately before `#trace-panel`, titled `缓冲计算参考` and labeled `计算参考，非当前物料`, with backend source/evidence and an independently loaded zone stack. Assert the history payload no longer supplies this card, while its old optional field remains deserializable as `null`.

- [ ] **Step 2: Run red tests**

```powershell
node .\tests\AdaptiveSopDdsop.Tests\Js\ddmrp-standard-reference.fixture.mjs
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: service/endpoint/white-box host are missing and the card still belongs to history.

- [ ] **Step 3: Implement the internal reference service**

Return input parameters, red-base/red-safety/yellow/green derivation rows, zone totals, source `DDAE 后端标准定容算例` and `Complete` evidence. Register the service and map `/api/ddmrp-standard-reference` after the existing `/api/history-review` block, outside every integration-contract protected range.

Keep this endpoint internal and read-only. Do not reuse contract records, fixtures, endpoints or repository resolvers.

- [ ] **Step 4: Move only the presentation**

Remove the visible standard card from history. Add the closed disclosure immediately before the protected trace panel and load it independently when first opened. Keep `#trace-panel` itself, its Chinese labels, calculation stages, DOM IDs, button behavior and `renderPreviewTrace`/`renderTrace` byte-for-byte unchanged.

- [ ] **Step 5: Verify and commit**

```powershell
node .\tests\AdaptiveSopDdsop.Tests\Js\ddmrp-standard-reference.fixture.mjs
node --check .\src\AdaptiveSopDdsop.Web\wwwroot\js\app.js
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
.\scripts\verify-protected-boundaries.ps1 -Baseline 4e39ec5
git diff --check
git add src/AdaptiveSopDdsop.Web/Domain/DdmrpStandardReferenceService.cs src/AdaptiveSopDdsop.Web/Program.cs src/AdaptiveSopDdsop.Web/Domain/HistoryReviewWorkspaceService.cs src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests/Js/ddmrp-standard-reference.fixture.mjs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "refactor: move DDMRP reference to white-box host"
```

---

### Task 12: 执行全量回归与边界审计

**Files:**
- Modify: `tests/AdaptiveSopDdsop.Tests/Program.cs`
- Verify only: `scripts/verify-protected-boundaries.ps1`
- Verify only: all changed production/test files

**Interfaces:** Adds new internal source files to the existing forbidden-token guard and proves no protected boundary changed.

- [ ] **Step 1: Extend the internal-boundary source scan**

In `TestFiveStageServicesDoNotReferenceExternalContractTypesOrEndpoints`, append these new files to `fiveStageFiles`:

```text
PlanningEvidenceModels.cs
PlanningEvidenceValidator.cs
InventoryFlowProjectionModels.cs
InventoryFlowProjectionService.cs
CapacityProtectionMath.cs
DdmrpStandardReferenceService.cs
```

Keep the protected token list unchanged or stricter. Add an assertion that `/api/ddmrp-standard-reference` occurs outside the delimited integration-contract block.

Add a business-UI localization assertion for `DemoFixture`, `InProgress`, `Open`, `Completed` and `Escalated` so ordinary English codes do not leak into the five business stages; do not modify persistence or contract values.

- [ ] **Step 2: Run every JavaScript fixture and syntax check**

```powershell
node .\tests\AdaptiveSopDdsop.Tests\Js\history-review-loader-race.fixture.mjs
node .\tests\AdaptiveSopDdsop.Tests\Js\history-buffer-renderers.fixture.mjs
node .\tests\AdaptiveSopDdsop.Tests\Js\future-inventory-flow-charts.fixture.mjs
node .\tests\AdaptiveSopDdsop.Tests\Js\ddmrp-standard-reference.fixture.mjs
node --check .\src\AdaptiveSopDdsop.Web\wwwroot\js\app.js
```

Expected: every command exits `0`.

- [ ] **Step 3: Run C# tests, build and protected-boundary verification**

```powershell
dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
dotnet build .\AdaptiveSopDdsop.sln --no-restore -m:1
.\scripts\verify-protected-boundaries.ps1 -Baseline 4e39ec5
```

Expected: all tests pass, build reports zero warnings/zero errors, and every protected whole file/block/function matches baseline.

- [ ] **Step 4: Audit scope and diffs**

```powershell
git diff --check
git status --short
git diff --name-status 4e39ec5
rg -n "DdsopConfigInboundContract|DdsopRuntimePlanningInputContract|SdbrExecutionObjectEvidenceContract|PublicDemoGoldenLoopService|NetworkScore|network-scoring|SDBR payload" .\src\AdaptiveSopDdsop.Web\Domain\PlanningEvidenceModels.cs .\src\AdaptiveSopDdsop.Web\Domain\PlanningEvidenceValidator.cs .\src\AdaptiveSopDdsop.Web\Domain\InventoryFlowProjectionModels.cs .\src\AdaptiveSopDdsop.Web\Domain\InventoryFlowProjectionService.cs .\src\AdaptiveSopDdsop.Web\Domain\CapacityProtectionMath.cs .\src\AdaptiveSopDdsop.Web\Domain\DdmrpStandardReferenceService.cs
git -C ..\DDAE_INTERFACE_CONTRACT status --short
git -C ..\DDAE-NetworkStructure status --short
```

Expected: `rg` has no match; adjacent-repository status and HEAD exactly match the pre-Task-1 audit (existing user changes, if any, remain untouched); no SDBR fixture, contract file/test, Network file, protected page/function, or unrelated user file appears in the DDAE diff.

- [ ] **Step 5: Commit the final guard if it changed**

```powershell
git add tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "test: guard internal projection boundaries"
```

Skip this commit if Task 11 already included the exact final guard and the worktree is clean; never create an empty commit.

- [ ] **Step 6: Inspect only from the merged Documents checkout**

After the feature branch has passed review and been merged into `main`, start the inspection service from `C:\Users\吴一帆\Documents\DDAE`, not from a feature worktree:

```powershell
Set-Location C:\Users\吴一帆\Documents\DDAE
dotnet run --project .\src\AdaptiveSopDdsop.Web\AdaptiveSopDdsop.Web.csproj --no-build --urls http://localhost:55525
```

Confirm the five business-stage entries still switch the main page, `白盒追踪` remains independent, and `公开演示闭环` remains the last validation entry with its original controls.

---

## Execution Notes

- Execute Tasks 1–12 in order; Tasks 1–5 establish authoritative data semantics before any UI consumes them.
- Use a dedicated `codex/` feature branch/worktree for implementation, but merge and inspect from `C:\Users\吴一帆\Documents\DDAE` as required.
- Before Task 1, record `git status --short` and `git rev-parse HEAD` for DDAE and both adjacent repositories; final scope audit compares against that record rather than assuming the user's other repositories are clean.
- At each task checkpoint, review the diff for tail-only DTO changes and run the listed focused tests before committing.
- If an existing test conflicts with the confirmed physical-flow equations, stop and reconcile the business invariant; do not weaken the test or fall back to legacy metrics silently.
- Any need to touch CONTRACT, SDBR, Network, protected endpoint blocks, protected renderer bodies or full execution scheduling is out of scope and must be raised rather than implemented.
