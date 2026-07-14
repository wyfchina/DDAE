# DDAE Five-Stage Workbench Revision Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox ( - [ ] ) syntax for tracking.

**Goal:** 在不触碰任何 CONTRACT、SDBR、Network 协议或既有验证页能力的前提下，把 DDAE 升级为具有可信历史、可冻结会前基线、四类风险分析、可追溯配置决策和行动闭环的五阶段切页工作台。

**Architecture:** 所有新增能力都留在 DDAE 内部。历史结果由独立历史事实源提供；当前基线冻结内部类型化证据；未来场景只接受人工录入或内置 DemoFixture，经后端白盒重算；库存、时间、能力保护和供应风险分别分析；03、04、05 用真实 SQLite 标识关联。Razor 页面只负责展示后端结果，左侧一级菜单展开二级菜单，URL hash 控制唯一可见视图。

**Tech Stack:** .NET 9、ASP.NET Core Razor Pages 与 Minimal API、Vanilla JavaScript/CSS、Microsoft.Data.Sqlite、现有单文件控制台测试运行器。

## Global constraints

- [ ] 只在功能 worktree **C:\Users\吴一帆\Documents\Codex\2026-07-14\q\ddae-five-stage-worktree**、分支 **codex/ddsop-five-stage-workbench** 中实施。
- [ ] 基准提交为 **4e39ec5**；每个任务开始前确认没有来自其它任务的未提交覆盖。
- [ ] 不修改 **C:\Users\吴一帆\Documents\DDAE_INTERFACE_CONTRACT** 和 **C:\Users\吴一帆\Documents\DDAE-NetworkStructure**。
- [ ] 不修改或引用以下契约实现：DdsopConfigInboundContract.cs、DdsopRuntimePlanningInputContract.cs、ProductionInventoryQualityEvidenceContract.cs、ProductionSupplierIdentitySourceContract.cs、SdbrExecutionObjectEvidenceContract.cs。
- [ ] 不修改 PublicDemoGoldenLoopService.cs、AdventureWorksProductDemoProfileService.cs、ContractRepositoryPathResolver.cs、两个 sdbr fixture、appsettings.json 中既有 Network 配置，以及 Program.cs 中现有 integration/public-demo 端点块。
- [ ] 不修改十个既有协议回归测试的方法体和断言。
- [ ] 白盒追踪的 DOM 标识和 renderTrace()/renderPreviewTrace() 保持；公开演示闭环的 DOM、按钮、API、payload 写出、适配器审计和渲染函数保持。
- [ ] 新业务服务不得读取 Network 评分，不得读取或解释 SDBR/DDOM 协议，不得出现 CSV/JSON/file import 或外部连接器。
- [ ] DDAE 只生成分析与治理建议；不生成执行排程，不自动采纳、审批、生效、发布或推进状态。
- [ ] SQLite 只做增量建表、增量加列和精确数据修复，不删除或重建用户已有业务数据。
- [ ] 每项功能遵守 RED → GREEN → REFACTOR：先添加单一失败测试，运行完整控制台测试确认失败原因，再实现最小代码，最后再次运行完整测试。
- [ ] 测试入口固定为：

~~~powershell
dotnet run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
~~~

## Target internal contracts

下列记录是本次内部边界。它们不得放进任何 Contract 类或公共演示 payload。

~~~csharp
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

public sealed record TimeBufferDefinition(
    string BufferId,
    string ControlPoint,
    string ProtectedActivity,
    decimal BufferDays,
    bool IsCritical,
    string Applicability,
    string EvidenceStatus);

public sealed record TimeBufferProductScope(
    string BufferId,
    IReadOnlyList<string> ProductFamilies,
    IReadOnlyList<string> Skus,
    string EvidenceStatus);

public sealed record ControlPointProgressFact(
    string BufferId,
    int Week,
    decimal? ObservedDelayDays,
    string Cause,
    string EvidenceStatus);

public sealed record CapacityProtectionDefinition(
    string ProtectionId,
    string UpstreamResourceCode,
    string ProtectedCcrResourceCode,
    decimal ReservePercent,
    string Applicability,
    string EvidenceStatus);

public sealed record ExternalTimeDelay(
    string BufferId,
    int StartWeek,
    int EndWeek,
    decimal DelayDays,
    string Reason);

public sealed record TimeBufferResponseAdjustment(
    string BufferId,
    int StartWeek,
    int EndWeek,
    decimal RecoveredDays,
    string Reason);
~~~

场景比较的 scope 固定为 **InventoryBuffer、TimeBuffer、CapacityBuffer、SupplyRisk**。前三项是 DDAE 内部保护分析，SupplyRisk 是独立风险，不得伪装成 DDOM 缓冲。

---

## Task 1: Lock protected boundaries and establish protection evidence primitives

**Files**

- Modify: tests/AdaptiveSopDdsop.Tests/Program.cs
- Create: src/AdaptiveSopDdsop.Web/Domain/ProtectionPlanningModels.cs
- Modify: src/AdaptiveSopDdsop.Web/Domain/Models.cs
- Modify: src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs
- Modify: src/AdaptiveSopDdsop.Web/Data/SeedData.cs
- Modify: src/AdaptiveSopDdsop.Web/Data/SeedScenarioWorkspaceDataSource.cs
- Create: scripts/verify-protected-boundaries.ps1

- [ ] **Step 1: Add the boundary and seed-scale tests to the runner.**

在 tests 数组注册并实现：

- TestFiveStageServicesDoNotReferenceExternalContractTypesOrEndpoints
- TestSeedScaleMatchesSatelliteManufacturingDemo
- TestFpgaBelongsOnlyToIndependentInventoryControlPoint
- TestThreeIndependentInventoryControlPointsAreExplicit
- TestCapacityProtectionRequiresSequencedUpstreamEvidence
- TestCapacityProtectionDoesNotInferWithoutSequenceEvidence

边界测试只扫描本次新增/修改的五阶段领域文件，拒绝下列文本：DdsopConfigInboundContract、DdsopRuntimePlanningInputContract、SdbrExecutionObjectEvidenceContract、PublicDemoGoldenLoopService、NetworkScore、network-scoring、SDBR payload。它不扫描既有公开演示闭环文件。

同时创建 scripts/verify-protected-boundaries.ps1，参数为 string Baseline = "4e39ec5"。脚本用 git show 读取 baseline 文件、Get-Content -Raw -Encoding utf8 读取当前文件，并实现两个确定性提取器：

~~~powershell
function Get-BracedBlock([string]$Text, [string]$Signature)
function Get-DelimitedBlock([string]$Text, [string]$StartMarker, [string]$EndMarker)
~~~

Get-BracedBlock 从 Signature 后第一个左花括号开始做字符级括号计数，返回完整 C#/JS 方法；Get-DelimitedBlock 返回两个唯一 marker 间的原文。任一 marker 不唯一、块缺失或 baseline/current 不相等时脚本 exit 1。

脚本逐一比较测试文件中的十个受保护方法：

- TestDdsopConfigInboundPayloadAndAckInterpreter
- TestDdsopFeedbackInboundLedgerAcceptsSdbrFixtures
- TestDdsopRuntimePlanningInputGeneratesDdaeOwnedPackage
- TestAdventureWorksSchedulingAdapterMetadataStaysNonDdaeOwned
- TestAdventureWorksProductDemoProfileExposesDdaeGovernanceReadModel
- TestContractRepositoryPathResolverPrefersConfiguredRoot
- TestContractRepositoryPathResolverDiscoversSiblingRepository
- TestDdsopRuntimePlanningInputCorrelatesFeedback
- TestPublicDemoGoldenLoopServiceWritesHandoffPayload
- TestIntegrationContractEndpointsAndRemovedOptimizationPath

它还比较 Program.cs 从 app.MapGet("/api/integration-contracts/ddsop-config-inbound-v1" 到 app.MapGet("/api/history-review" 之前的端点块；Index.cshtml 的 trace-panel inner block、public-demo-golden-loop-panel 完整 block和既有 Network 链接元素；app.js 的 renderPreviewTrace、renderTrace、renderPublicDemoGoldenLoop、renderAdventureWorksProductDemo、renderPublicDemoSchedulingAdapter、renderPublicDemoPayload、renderPublicDemoFeedback、renderPublicDemoBusinessUserView、loadPublicDemoGoldenLoop、writePublicDemoPayload。受保护文件使用 git diff --exit-code 完整比较。

- [ ] **Step 2: Run the complete test runner and confirm RED.**

预期失败必须分别指向：库存量级过大、FPGA 被作为 Time Buffer、ResourceRouting 没有顺序/保护对象证据。若先因编译失败，先只添加测试所需的内部记录签名，不填实现，再重跑得到行为失败。

- [ ] **Step 3: Add backward-compatible evidence records.**

在 Models.cs 给 ResourceRouting 追加可选参数，保留所有现有三参数调用可编译：

~~~csharp
public sealed record ResourceRouting(
    string Sku,
    string ResourceCode,
    decimal CapacityPerUnit,
    int OperationSequence = 1,
    string? ProtectsCcrResourceCode = null,
    string EvidenceStatus = "Complete");
~~~

在 ProtectionPlanningModels.cs 定义本计划 Target internal contracts 中的 TimeBufferDefinition、TimeBufferProductScope、ControlPointProgressFact、CapacityProtectionDefinition、ExternalTimeDelay、TimeBufferResponseAdjustment。所有类型位于 AdaptiveSopDdsop.Web.Domain，不引用集成契约 namespace。

- [ ] **Step 4: Extend only internal scenario data records.**

在 ScenarioRunParameterSet 末尾增加可选 TimeBufferAdjustments。在 ExternalScenarioDefinition 末尾只增加可选 TimeDelays；Metadata 字段等 ScenarioAssumptions.cs 于 Task 4 存在后再追加，保证 Task 1 独立编译。在 ScenarioWorkspaceDataSet 末尾增加以下可选集合，保证旧构造函数继续工作：

~~~csharp
IReadOnlyList<CapacityProtectionDefinition>? CapacityProtections = null,
IReadOnlyList<TimeBufferDefinition>? TimeBuffers = null,
IReadOnlyList<ControlPointProgressFact>? ControlPointProgress = null,
IReadOnlyList<TimeBufferProductScope>? TimeBufferProductScopes = null
~~~

不得把这些字段加入任何 Ddsop、Sdbr 或 public-demo DTO。

- [ ] **Step 5: Recalibrate SeedData to a credible satellite-manufacturing scale.**

将当前库存金额控制在人民币 6000 万至 1 亿元。数量量级固定为：整星/载荷个位至低两位，星载组件几十，材料/线缆几十至数百。用测试计算 InventoryPosition.OnHand × SkuBufferSetting.UnitCost，不在测试中硬编码最终总额。

Seed 的 OnHand/OpenSupply/QualifiedDemand 固定为：

~~~text
SAT-BUS-001   4 / 1 / 2
SAT-BUS-002   3 / 1 / 1
SAT-PROP-003 12 / 3 / 4
PAY-EO-101    3 / 1 / 1
PAY-SAR-102   2 / 1 / 1
AV-COM-201   28 / 8 / 10
AV-OBC-202   20 / 6 / 8
AV-FPGA-203  22 / 4 / 6
TC-MLI-301   75 / 20 / 24
TC-RAD-302   48 / 12 / 16
MECH-DEP-401 12 / 3 / 4
CBL-HAR-402 120 / 30 / 36
~~~

保留现有单位成本时，OnHand 总金额为 61,718,000 元。ADU 固定为 0.20、0.12、0.80、0.10、0.08、1.20、0.80、0.18、4.00、2.50、0.60、5.00，顺序与上述 SKU 一致。

Resource.WeeklyAvailableUnits 统一解释为“标准小时/周”，固定为 RES-AIT 160、RES-TVAC 96、RES-CLEAN 120、RES-HARNESS 180。调整每 SKU 的 CapacityPerUnit 后必须满足测试范围：HARNESS 基线负荷 95%–105%，AIT 负荷高于其 80% 保护起点但不超过计划可用能力，TVAC 基线低于 90% 且在内置能力损失场景中超过 100%。不得继续把几百/上千的混合“单位”与小时负荷相除。

FPGA 规则固定为：

- AV-FPGA-203 的 DecouplingPoint 为“关键进口 FPGA 库存控制点”；
- 它仍有库存缓冲和控制点证据；
- 它不能出现在 TimeBufferDefinition；
- 它不能出现在 CapacityProtectionDefinition；
- MS-TB-001 改为真实的“热真空试验准备控制点”时间保护；
- CCR 本体可以显示利用率，但不能把自己的未使用能力当作“保护消耗”。

库存控制点名称和归属固定为三条独立证据：

- 热控结构件库存控制点：TC-MLI-301、TC-RAD-302；
- 星载电子半成品库存控制点：AV-COM-201、AV-OBC-202；
- 关键进口 FPGA 库存控制点：只包含 AV-FPGA-203。

Seed 和测试都必须断言不存在“热控结构件超市”“星载电子半成品超市”等旧名称，也不得让 AV-FPGA-203 同时落入星载电子半成品控制点。

- [ ] **Step 6: Seed explicit sequence and protection evidence.**

选定 HARNESS 为当前 CCR 示例；AIT 为有完整顺序证据的上游保护资源。TVAC 保持“场景潜在 CCR”，不是默认当前保护资源。SeedScenarioWorkspaceDataSource 返回显式 CapacityProtectionDefinition；无定义的资源只拥有“未承诺能力余量”，不能被历史或场景服务推断成保护能力。

- [ ] **Step 7: Run tests and confirm GREEN.**

除新增测试外，确认现有 ScenarioWorkspaceDataSet 三参数 ResourceRouting 用法、preview 和 63 项基线行为仍可编译运行。

- [ ] **Step 8: Commit the evidence primitives.**

~~~powershell
git add src/AdaptiveSopDdsop.Web/Domain/ProtectionPlanningModels.cs src/AdaptiveSopDdsop.Web/Domain/Models.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs src/AdaptiveSopDdsop.Web/Data/SeedData.cs src/AdaptiveSopDdsop.Web/Data/SeedScenarioWorkspaceDataSource.cs scripts/verify-protected-boundaries.ps1 tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "refactor: establish internal protection evidence"
~~~

---

## Task 2: Replace synthetic history with explicit 52-week operating facts

**Files**

- Create: src/AdaptiveSopDdsop.Web/Domain/HistoryOperatingFacts.cs
- Create: src/AdaptiveSopDdsop.Web/Data/SeedHistoryOperatingFactSource.cs
- Modify: src/AdaptiveSopDdsop.Web/Domain/HistoryReviewWorkspaceService.cs
- Modify: src/AdaptiveSopDdsop.Web/Program.cs
- Modify: tests/AdaptiveSopDdsop.Tests/Program.cs

- [ ] **Step 1: Add explicit-history failure tests.**

注册并实现：

- TestHistoryReviewAggregatesDistinctTwentySixAndFiftyTwoWeekFacts
- TestHistoricalOutcomesUseExplicitFactsAndTraceableCosts
- 扩展 TestHistoryReviewUsesCumulativeLeadTimeAndProtectionEvidence

断言：

- 六个月严格读取最近 26 周，十二个月严格读取 52 周；
- 两个窗口至少在服务、平均库存、在制品、流动时间、现金占用、异常费用之一不同；
- 异常费用等于显式事件 CostAmount 之和，未来 SupplierCapacityWindow 的 Red 数量变化不能影响历史费用；
- CashOccupied 不能通过复制 InventoryValue 得出；
- 历史页没有“当前占用”语义；
- 累计提前期只决定详细缓冲窗口，不截断 26/52 周经营趋势。

- [ ] **Step 2: Run tests and confirm RED against the current synthetic calculation.**

预期当前实现会因 26/52 相同、现金等于库存、Red 供应周乘 2500 而失败。

- [ ] **Step 3: Define the historical fact boundary.**

在 HistoryOperatingFacts.cs 定义：

~~~csharp
public sealed record HistoryFactRequest(int Weeks, DateOnly AsOfDate);

public sealed record WeeklyOperatingFact(
    int WeekOffset,
    decimal? ServiceLevelPercent,
    decimal? InventoryValue,
    decimal? WorkInProcessUnits,
    decimal? AverageFlowTimeDays,
    decimal? CashOccupied,
    string EvidenceStatus);

public sealed record WeeklyBufferFact(
    string Sku,
    int WeekOffset,
    decimal? EndingNetFlow,
    string ExplicitCause,
    string EvidenceStatus);

public sealed record WeeklyCapacityFact(
    string ResourceCode,
    int WeekOffset,
    decimal? TheoreticalCapacity,
    decimal? StandardCapacity,
    decimal? DemonstratedCapacity,
    decimal? PlannedAvailableCapacity,
    decimal? CommittedLoad,
    string LossReason,
    string EvidenceStatus);

public sealed record HistoryConstraintFact(
    string ExposureType,
    string Target,
    int? WeekOffset,
    string Status,
    decimal? LoadPercent,
    string Evidence,
    string SourceKind,
    string EvidenceStatus);

public sealed record HistoryAbnormalCostEvent(
    string EventId,
    int WeekOffset,
    decimal CostAmount,
    string CostType,
    string Cause,
    string EvidenceStatus);

public sealed record HistoryFactSet(
    HistoryFactRequest Request,
    IReadOnlyList<WeeklyOperatingFact> OperatingFacts,
    IReadOnlyList<WeeklyBufferFact> BufferFacts,
    IReadOnlyList<WeeklyCapacityFact> CapacityFacts,
    IReadOnlyList<HistoryConstraintFact> ConstraintFacts,
    IReadOnlyList<HistoryAbnormalCostEvent> AbnormalCosts,
    string SourceAuthority,
    string AsOfUtc,
    string EvidenceLabel);

public interface IHistoryOperatingFactSource
{
    HistoryFactSet Load(HistoryFactRequest request);
}
~~~

- [ ] **Step 4: Implement deterministic DemoFixture facts.**

SeedHistoryOperatingFactSource 始终生成同一组 52 周经营、缓冲和能力事实，再按 request.Weeks 取最近窗口。最近 26 周与较早 26 周采用不同但连续的趋势；缓冲事实带显式异常原因；能力事实带每周理论/标准/经验证/计划可用/承诺负荷和损失原因；异常费用只由明确的“加急运输、临时外协、返工、替代料认证”事件组成。HistoryConstraintFact 显式覆盖“当前 CCR、高负荷资源、场景潜在 CCR、事件型约束、外部约束”：除场景潜在 CCR 之外的四类历史观察使用 SourceKind == HistoricalFact，场景潜在 CCR 使用 SourceKind == InternalScenarioDefinition。不得读取 ScenarioTemplate、SupplierCapacityWindow、未来 Demand 或当前 InventoryPosition 来制造历史。

演示聚合验收范围固定为：

| 指标 | 最近 26 周 | 最近 52 周 |
| --- | ---: | ---: |
| 服务水平 | 96.5%–97.5% | 95.5%–96.5% |
| 平均库存金额 | 6500万–7500万元 | 7200万–8200万元 |
| 平均在制品 | 55–70 | 65–80 |
| 平均流动时间 | 17–20天 | 20–24天 |
| 平均现金占用 | 7800万–9000万元 | 9000万–1.05亿元 |
| 异常费用 | 420,000元 | 1,200,000元 |

异常费用事件固定为最近 26 周两笔 180,000/240,000 元，较早 26 周两笔 360,000/420,000 元；测试直接 sum HistoryAbnormalCostEvent，不硬编码“红色窗口”。

- [ ] **Step 5: Refactor HistoryReviewWorkspaceService.**

构造函数同时注入 IHistoryOperatingFactSource 和 IScenarioWorkspaceDataSource。前者负责 52 周经营、缓冲、能力和异常费用事实；后者只提供累计提前期参数、控制点定义、资源名称和显式工艺/保护关系，不提供历史数量。

将现有输出记录改成以下签名，保证“证据缺失”不会被 decimal 默认值伪装成零：

~~~csharp
public sealed record HistoryOperatingOutcomes(
    decimal? ServiceLevelPercent,
    decimal? InventoryValue,
    decimal? WorkInProcessUnits,
    decimal? AverageFlowTimeDays,
    decimal? CashOccupied,
    decimal? ExpediteCost,
    decimal? RemainingProtectionPercent,
    string EvidenceStatus);

public sealed record CapacityProtectionLayer(
    string ResourceCode,
    string ResourceName,
    string? ProtectedCcrResourceCode,
    string RelationshipRole,
    decimal? TheoreticalCapacity,
    decimal? StandardCapacity,
    decimal? DemonstratedCapacity,
    decimal? PlannedAvailableCapacity,
    decimal? CommittedLoad,
    decimal? ProtectiveCapacity,
    decimal? ConsumedProtection,
    decimal? RemainingProtection,
    string LossReason,
    string EvidenceStatus);

public sealed record ConstraintExposureItem(
    string ExposureType,
    string Target,
    string Status,
    decimal? LoadPercent,
    string Evidence,
    string EvidenceStatus);
~~~

聚合时忽略缺失周，但任一指标没有有效事实就返回 null + EvidenceMissing，不能返回零。RemainingProtectionPercent 只汇总有完整 CapacityProtectionDefinition 的上游资源。

能力保护公式固定为：

~~~text
保护能力 = 计划可用能力 × 保护比例
保护起点 = 计划可用能力 - 保护能力
已消耗保护 = clamp(承诺负荷 - 保护起点, 0, 保护能力)
剩余保护 = 保护能力 - 已消耗保护
~~~

区域停留和恢复必须使用 WeeklyBufferFact，能力层必须使用 WeeklyCapacityFact，五类约束只使用 HistoryConstraintFact；不得再从当前 Demand、ResourceCalendar、ScenarioTemplate 或未来 SupplierCapacityWindow 倒推出历史。CCR 本体单列 utilization；上游保护资源才有 ProtectiveCapacity、ConsumedProtection 和 RemainingProtection。没有工序顺序或保护定义时显示 EvidenceMissing，相关数字为空。

- [ ] **Step 6: Register SeedHistoryOperatingFactSource in Program.cs outside the protected endpoint block.**

只增加 DI 注册，不移动、不格式化 Program.cs 的现有协议端点。

- [ ] **Step 7: Run tests and confirm GREEN.**

核对六个月/十二个月量级合理，异常费用为可解释的显式事件合计，且所有历史字段都有来源与窗口。

- [ ] **Step 8: Commit the history source.**

~~~powershell
git add src/AdaptiveSopDdsop.Web/Domain/HistoryOperatingFacts.cs src/AdaptiveSopDdsop.Web/Data/SeedHistoryOperatingFactSource.cs src/AdaptiveSopDdsop.Web/Domain/HistoryReviewWorkspaceService.cs src/AdaptiveSopDdsop.Web/Program.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: base history review on explicit operating facts"
~~~

---

## Task 3: Make the current baseline a complete, immutable meeting snapshot

**Files**

- Modify: src/AdaptiveSopDdsop.Web/Domain/CurrentBaselineService.cs
- Modify: src/AdaptiveSopDdsop.Web/Data/SeedCurrentBaselineDataSource.cs
- Modify: src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs
- Modify: tests/AdaptiveSopDdsop.Tests/Program.cs

- [ ] **Step 1: Add baseline KPI and evidence-rule tests.**

注册并实现：

- TestCurrentBaselineExposesSnapshotKpisWithSourceAndAsOf
- TestTimeBufferEvidenceRulesControlBaselineFreeze
- 扩展 TestCurrentBaselineFreezesCompleteEvidence
- 扩展 TestCurrentBaselineRejectsMissingCriticalEvidence

会前指标必须包含：服务水平及统计窗口、库存金额、在制品、积压、供应覆盖、资源峰值负荷、来源、截止时间和证据状态。历史页不再承担这些会前时点指标。

时间证据规则：

- 没有任何时间缓冲定义 → NotApplicable，不阻止冻结；
- 已定义且关键控制点进展缺失 → EvidenceMissing，阻止冻结；
- 已定义且非关键控制点进展缺失 → 允许冻结，但 TimeBuffer 分析状态为 EvidenceMissing；
- 缺失值不能作为零进入场景计算。

- [ ] **Step 2: Run tests and confirm RED.**

确认失败来自 CurrentBaselinePayload 无 KPI、Freshness 未纳入拦截、时间证据无规则。

- [ ] **Step 3: Add internal baseline snapshot records.**

在 CurrentBaselineService.cs 增加：

~~~csharp
public sealed record BaselineKpiSnapshot(
    decimal? ServiceLevelPercent,
    string ServiceWindow,
    decimal? InventoryValue,
    decimal? WorkInProcessUnits,
    decimal? BacklogUnits,
    decimal? SupplyCoverageWeeks,
    decimal? PeakResourceLoadPercent,
    string SourceAuthority,
    string AsOfUtc,
    string EvidenceStatus);

public sealed record BaselineAnalysisAvailability(
    string AnalysisCode,
    string Status,
    string Reason);

public sealed record BaselineEvidenceItem(
    string ItemKey,
    string Name,
    string FreshnessStatus,
    string CompletenessStatus,
    bool BlocksFreeze,
    string? MissingReason = null);
~~~

给 BaselineEvidenceSection 末尾依次追加 string? MissingReason = null 和 IReadOnlyList<BaselineEvidenceItem>? Items = null。给 CurrentBaselinePayload 末尾追加 BaselineKpiSnapshot? Kpis 和 IReadOnlyList<BaselineAnalysisAvailability>? AnalysisAvailability，保持旧测试构造兼容。

给 CurrentBaselineAuditEvent 末尾追加 string? PayloadJson = null；用 PRAGMA table_info 为 current_baseline_audit_events 增量增加 payload_json，旧审计保持 null。

- [ ] **Step 4: Build explicit seed sections and KPI facts.**

SeedCurrentBaselineDataSource 增加以下 section code：

- CURRENT_KPIS
- TIME_BUFFER_DEFINITIONS
- TIME_BUFFER_PRODUCT_SCOPES
- CONTROL_POINT_PROGRESS
- ROUTING_SEQUENCE
- CAPACITY_PROTECTION

每个 section 明确 SourceAuthority、AsOfUtc、FreshnessStatus、CompletenessStatus、EvidenceLabel、IsRequired 和 MissingReason。内部状态仍可使用稳定英文 code，页面负责中文映射。

ICurrentBaselineDataSource 继续作为唯一候选基线抽象；本次只注册 SeedCurrentBaselineDataSource，且 EvidenceLabel 明确为 DemoFixture，不增加外部实现。

- [ ] **Step 5: Tighten freeze validation without rebuilding tables.**

冻结阻断算法固定为：

1. section.Items 为 null/空时，沿用 section.IsRequired；required section 必须 Fresh + Complete。
2. section.Items 非空时，不用 section 汇总状态决定冻结；只检查 BlocksFreeze == true 的 item 是否 Fresh + Complete。
3. 时间缓冲未配置：定义、范围、进展 section 均为 NotApplicable，Items 为空，TimeBuffer availability 为 NotApplicable。
4. 时间缓冲已配置：每个 BufferId 在定义、产品范围和进展 section 各有 item；TimeBufferDefinition.IsCritical 决定三类 item 的 BlocksFreeze。
5. 任一非关键 item 缺失：section 汇总可为 EvidenceMissing，但不阻止冻结；TimeBuffer availability 为 EvidenceMissing。
6. 任一关键 item 缺失或过期：阻止冻结并在错误中列出 SectionCode/ItemKey/MissingReason。

PlanningInputs 仍必须存在。冻结仍插入新 snapshot 和新 audit；audit message/payload 记录 CandidateId、SnapshotNumber、操作者、时间、完整/缺失 section/item 摘要和冻结结果。既有 snapshot update/delete 触发器保持。List/GetDetail 不修改冻结数据。

- [ ] **Step 6: Preserve missing-evidence semantics in frozen preview.**

ScenarioRunPreviewService.LoadFrozenWorkspaceData 必须把 CapacityProtections、TimeBuffers、TimeBufferProductScopes、ControlPointProgress 从冻结 Payload.PlanningInputs 原样带入；不能回退读取当前 seed 来填缺失证据。

- [ ] **Step 7: Run tests and confirm GREEN.**

额外断言连续冻结产生递增版本、旧快照保持字节等价、缺失值 JSON 往返后仍为 null。

- [ ] **Step 8: Commit the baseline revision.**

~~~powershell
git add src/AdaptiveSopDdsop.Web/Domain/CurrentBaselineService.cs src/AdaptiveSopDdsop.Web/Data/SeedCurrentBaselineDataSource.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: strengthen frozen current baselines"
~~~

---

## Task 4: Add internal-only scenario assumptions and real frozen-run lineage

**Files**

- Create: src/AdaptiveSopDdsop.Web/Domain/ScenarioAssumptions.cs
- Create: src/AdaptiveSopDdsop.Web/Data/SeedScenarioAssumptionSource.cs
- Modify: src/AdaptiveSopDdsop.Web/Domain/ScenarioComparisonService.cs
- Modify: src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPersistenceService.cs
- Modify: src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs
- Modify: src/AdaptiveSopDdsop.Web/Program.cs
- Modify: tests/AdaptiveSopDdsop.Tests/Program.cs

- [ ] **Step 1: Add assumption and lineage tests.**

注册并实现：

- TestScenarioAssumptionSourceProvidesOnlyManualAndDemoInputs
- TestScenarioAssumptionSourceRejectsExternalProtocolSources
- TestFrozenComparisonSavePersistsBaselineScenarioAndResponseLineage
- 扩展 TestScenarioComparisonSeparatesExternalEventsAndResponses
- 扩展 TestScenarioComparisonUsesFrozenSnapshotValues

允许的 SourceKind 仅 Manual 和 DemoFixture，大小写标准化后仍只允许这两个值。拒绝 ExternalImport、File、Csv、Json、Network、NetworkScore、SDBR、DDOM、Contract。演示模板必须有稳定 TemplateId、TemplateVersion、EvidenceLabel。

- [ ] **Step 2: Run tests and confirm RED.**

预期无 IScenarioAssumptionSource，现有 scenario_runs 也没有 baseline/external/response 列。

- [ ] **Step 3: Define the internal assumption source.**

ScenarioAssumptions.cs 定义 ScenarioAssumptionMetadata、ScenarioAssumptionTemplate、IScenarioAssumptionSource：

~~~csharp
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
~~~

SeedScenarioAssumptionSource 只返回进程内 DemoFixture。它不读取文件、网络、环境变量或其它仓库。Manual 假设由请求携带并保留 RecordedBy、RecordedAtUtc、EffectiveFrom、EffectiveThrough、Rationale。

在该文件定义 ScenarioAssumptionMetadata 后，才在 ExternalScenarioDefinition 末尾追加 ScenarioAssumptionMetadata? Metadata = null。nullable 只为 JSON/旧构造函数反序列化兼容；下一步的 Compare 对 null 明确返回 400，不推断来源。

- [ ] **Step 4: Validate every comparison input.**

ScenarioComparisonService 注入 IScenarioAssumptionSource。Compare 在冻结基线校验后校验 Metadata：

- Metadata == null 时拒绝，错误为“场景来源缺失；仅允许人工录入或演示模板”；
- DemoFixture 必须能由 TemplateId 找到且版本匹配；
- Manual 必须有记录人、记录时间、有效期和理由；
- 其它 source 立即抛 ArgumentException；
- ExternalScenario 的需求、供应、能力、时间事件和 ResponseConfiguration 继续严格分开。

- [ ] **Step 5: Add a frozen-comparison save command.**

在 ScenarioWorkspaceData.cs 增加：

~~~csharp
public sealed record ScenarioComparisonSaveRequest(
    ScenarioComparisonRequest Comparison,
    string ResponseId,
    string Name,
    string? Description,
    string? CreatedBy);
~~~

扩展 ScenarioRunSummary，按下列顺序追加末尾可选字段，保持旧构造兼容：

~~~csharp
string? BaselineSnapshotId = null,
string? ExternalScenarioId = null,
string? ResponseId = null
~~~

ScenarioRunPersistenceService 增加 SaveFrozenComparison：

1. 重新调用后端 ScenarioComparisonService；
2. 只按 ResponseId 选 NO_RESPONSE 或一个 response case；
3. 保存该 frozen preview，不重新读取当前 seed；
4. scenario_runs 增量添加 baseline_snapshot_id、external_scenario_id、response_id；
5. 返回真实 RunId，供 04/05 关联；
6. 不改变 Status 或 ApprovalStatus，不触发任何治理状态。

使用 PRAGMA table_info 检查列是否存在，再执行单列 ALTER TABLE；不能删除或重建 scenario_runs。

ScenarioRunPersistenceService 同时实现只读查询接口，供治理校验使用：

~~~csharp
public interface IScenarioRunLineageReader
{
    ScenarioRunSummary? GetSummary(string runId);
}
~~~

- [ ] **Step 6: Register only internal APIs.**

在 Program.cs 非保护区域新增：

- GET /api/scenario-assumptions/templates
- POST /api/scenario-runs/compare/save

保持 POST /api/scenario-runs/compare 的路由和既有响应字段；新增来源字段保持可选反序列化，但缺 Metadata 的 compare 请求按上述规则返回 400。/api/scenario-runs/preview 的既有行为保持向后兼容，且不能保存 frozen lineage 或生成配置建议。不增加 import、network 或 protocol endpoint。

- [ ] **Step 7: Run tests and confirm GREEN.**

验证同一冻结基线、同一外部场景的 NO_RESPONSE 与多个 response 保存后有不同 ResponseId，但共享 BaselineSnapshotId 和 ExternalScenarioId。

- [ ] **Step 8: Commit the internal assumption boundary.**

~~~powershell
git add src/AdaptiveSopDdsop.Web/Domain/ScenarioAssumptions.cs src/AdaptiveSopDdsop.Web/Data/SeedScenarioAssumptionSource.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioComparisonService.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPersistenceService.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs src/AdaptiveSopDdsop.Web/Program.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: add internal scenario assumptions and lineage"
~~~

---

## Task 5: Separate inventory, time, capacity protection and supply risk analysis

**Files**

- Create: src/AdaptiveSopDdsop.Web/Domain/ProtectionAnalysis.cs
- Modify: src/AdaptiveSopDdsop.Web/Domain/ScenarioComparisonService.cs
- Modify: src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs
- Modify: tests/AdaptiveSopDdsop.Tests/Program.cs

- [ ] **Step 1: Add the four-scope analyzer tests.**

注册并实现：

- TestProtectionAnalysisSeparatesInventoryTimeCapacityAndSupply
- TestTimeBufferBreachReportsPenetrationRecoveryAndUnrecoveredHorizon
- TestTimeBufferMissingEvidenceIsNotReportedAsZero
- TestTimeBufferRequiresExplicitProductScopeEvidence
- TestSupplyRiskIsNotClassifiedAsDdomBuffer
- TestFpgaNeverAppearsInTimeOrCapacityBufferResults
- 扩展 TestProtectionBreachAnalysisReportsRecovery

把现有只期待 Capacity/Inventory/Supply 的断言改为四个固定 scope。删除“供应限制会生成 Time Buffer 建议”的错误断言。

- [ ] **Step 2: Run tests and confirm RED.**

当前 ProtectionBreachAnalyzer 没有 Time，把所有 CapacityCell 当保护资源，并把 Supply 当保护击穿，测试应因此失败。

- [ ] **Step 3: Define typed projection and result records.**

将 ProtectionBreachResult 和 ProtectionBreachAnalyzer 从 ScenarioComparisonService.cs 移出，避免重复类型；ProtectionAnalysis.cs 定义：

~~~csharp
public sealed record ProtectionBreachResult(
    string ScopeType,
    string Target,
    bool IsBreached,
    int? EarliestRedWeek,
    int ConsecutiveRiskWeeks,
    int? RecoveryWeek,
    bool IsUnrecovered,
    IReadOnlyList<string> AffectedProducts,
    string PrimaryCause,
    decimal? BufferSize = null,
    decimal? MaximumPenetrationPercent = null,
    string? Unit = null,
    string EvidenceStatus = "Complete");

public sealed record TimeBufferProjectionPoint(
    string BufferId,
    string ControlPoint,
    int Week,
    decimal? DelayDays,
    decimal BufferDays,
    decimal? PenetrationPercent,
    string Status,
    string EvidenceStatus,
    string Cause);

public sealed record CapacityProtectionProjectionPoint(
    string ProtectionId,
    string UpstreamResourceCode,
    string ProtectedCcrResourceCode,
    int Week,
    decimal? PlannedAvailableCapacity,
    decimal? CommittedLoad,
    decimal? ProtectionCapacity,
    decimal? ConsumedProtection,
    decimal? RemainingProtection,
    string Status,
    string EvidenceStatus);

public sealed record ProtectionScopeAnalysis<TPoint>(
    IReadOnlyList<TPoint> Projection,
    IReadOnlyList<ProtectionBreachResult> Breaches);

public interface ITimeBufferProtectionAnalyzer
{
    ProtectionScopeAnalysis<TimeBufferProjectionPoint> Analyze(
        ScenarioWorkspaceDataSet frozenData,
        ExternalScenarioDefinition externalScenario,
        ScenarioRunParameterSet? response,
        int horizonWeeks);
}

public interface ICapacityBufferProtectionAnalyzer
{
    ProtectionScopeAnalysis<CapacityProtectionProjectionPoint> Analyze(
        ScenarioWorkspaceDataSet frozenData,
        ScenarioRunPreviewCase previewCase,
        int horizonWeeks);
}
~~~

ScenarioComparisonCase 末尾追加以下可选字段以保持现有构造兼容，只保存后端计算结果：

~~~csharp
IReadOnlyList<TimeBufferProjectionPoint>? TimeBufferProjection = null,
IReadOnlyList<CapacityProtectionProjectionPoint>? CapacityProtectionProjection = null
~~~

- [ ] **Step 4: Implement inventory and supply analyzers as separate components.**

InventoryProtectionAnalyzer 暴露 IReadOnlyList<ProtectionBreachResult> Analyze(ScenarioRunPreviewCase previewCase)，继续从后端 BufferTrend.WeeklyCells 分析红区。SupplyRiskAnalyzer 暴露相同签名，只读取 SupplyCells，返回 ScopeType == SupplyRisk；它不构造 TimeBufferDefinition、CapacityProtectionDefinition 或主设置建议。TimeBufferProtectionAnalyzer 和 CapacityBufferProtectionAnalyzer 分别实现上述接口；共享的内部 StatusSeriesBreachCalculator 只接受已经形成的 (Week, Status) 序列，不知道任何业务协议。

- [ ] **Step 5: Implement time-buffer calculation.**

每周延迟计算固定为：

~~~text
基础延迟 = 冻结基线 ControlPointProgressFact.ObservedDelayDays
场景延迟 = 匹配周和 BufferId 的 ExternalTimeDelay.DelayDays
响应恢复 = 匹配周和 BufferId 的 TimeBufferResponseAdjustment.RecoveredDays
净延迟 = max(0, 基础延迟 + 场景延迟 - 响应恢复)
侵入比例 = 净延迟 / BufferDays × 100
红色 = 侵入比例 >= 100
黄色 = 67 <= 侵入比例 < 100
绿色 = 侵入比例 < 67
~~~

最早红周、最长连续红周、恢复周、展望期未恢复和最大侵入比例都从周序列计算。AffectedProducts 只能来自冻结的 TimeBufferProductScope；映射缺失时不得按名称或工艺猜测产品族。无定义为 NotApplicable；有定义但进展或产品范围证据缺失为 EvidenceMissing，DelayDays、PenetrationPercent、击穿数值和受影响产品保持 null/空集合。

- [ ] **Step 6: Implement upstream-only capacity protection.**

只遍历 CapacityProtectionDefinition。先按 ResourceRouting.OperationSequence 和 ProtectsCcrResourceCode 验证上游关系，再使用 Task 2 的保护公式。CCR 自身利用率进入 ConstraintExposureItem，但不产生“CCR 自我保护消耗”。没有显式顺序或 protection definition 的普通资源不得出现在 CapacityBuffer 结果。

- [ ] **Step 7: Wire analyzers into frozen comparison.**

ScenarioComparisonService.BuildCase 必须同时接收冻结的 ScenarioWorkspaceDataSet、ExternalScenarioDefinition 和 response parameters。任何分析都只能使用该冻结数据与当前请求，不得回读 live seed。ScenarioRunPreviewService 继续做 DDMRP、RCCP 和供应约束白盒重算，前端不复制公式。

- [ ] **Step 8: Run tests and confirm GREEN.**

测试至少覆盖一个在展望期恢复的时间缓冲、一个未恢复的时间缓冲、一个证据缺失时间缓冲，以及 AIT→HARNESS 的能力保护消耗。确认 AV-FPGA-203 只出现在 InventoryBuffer。

- [ ] **Step 9: Commit the analyzer split.**

~~~powershell
git add src/AdaptiveSopDdsop.Web/Domain/ProtectionAnalysis.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioComparisonService.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: separate four future risk analyses"
~~~

---

## Task 6: Enforce 03 → 04 → 05 lineage and bidirectional queries

**Files**

- Modify: src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs
- Modify: src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPersistenceService.cs
- Modify: src/AdaptiveSopDdsop.Web/Domain/MasterSettingsGovernanceService.cs
- Modify: src/AdaptiveSopDdsop.Web/Domain/CoordinationLedgerService.cs
- Create: src/AdaptiveSopDdsop.Web/Domain/BaselineLineageQueryService.cs
- Modify: src/AdaptiveSopDdsop.Web/Program.cs
- Modify: tests/AdaptiveSopDdsop.Tests/Program.cs

- [ ] **Step 1: Add lineage and non-automation tests.**

注册并实现：

- TestManualGovernanceChangeRequiresBaselineAndAllowsNoScenario
- TestScenarioDerivedGovernanceChangeRequiresBaselineAndScenario
- TestScenarioChangeAndCoordinationLinksAreQueryableBothDirections
- TestBaselineReferencesExposeRunsChangesAndActions
- TestCoordinationOutcomeDoesNotAdvanceGovernanceStatus
- TestUnlinkedHistoricalRecordsRemainExplicitlyUnlinked

测试关系：

- 一个 scenario run 可对应多个 change 和 coordination item；
- 一个 change 可对应多个 coordination item；
- 一个 coordination item 最多一个 scenario run、最多一个 change；
- 历史旧记录缺少来源时返回 Unlinked，不推断或回填；
- 记录 05 的 ActualOutcome 不改变 04 的 Proposed/Reviewed/Approved/Effective 状态。
- 记录 05 的 ActualOutcome 不改变当前冻结基线或已保存场景；它只能成为下一周期人工确认的只读证据。

- [ ] **Step 2: Run tests and confirm RED.**

当前实现允许无基线新建 change，冻结比较把 ExternalScenarioId/ResponseId 拼成伪 run id，列表也不能反查。

- [ ] **Step 3: Add governance creation metadata.**

MasterSettingChangeRequest 已有 SourceBaselineId；只在其末尾追加：

~~~csharp
string CreationMethod = "Legacy"
~~~

给 MasterSettingChangeSummary 末尾追加：

~~~csharp
string? SourceBaselineId = null,
string CreationMethod = "Legacy"
~~~

允许 CreationMethod 为 Manual、ScenarioDerived、Legacy。保存新记录时：

- Manual：SourceBaselineId 必填，SourceScenarioRunId 可空；
- ScenarioDerived：SourceBaselineId 和 SourceScenarioRunId 都必填，且 run 的 BaselineSnapshotId 必须一致；
- Legacy：只用于读取迁移前记录，API 不允许新建 Legacy；
- 所有创建仍以 Proposed 开始，后续只走既有人工状态机。

MasterSettingsGovernanceService 构造函数新增 IScenarioRunLineageReader 依赖。SaveChange 在写 SQLite 前调用 GetSummary；不存在的 run、非冻结比较保存的 run、基线不一致或 ResponseId 为空都拒绝。测试使用 FixedScenarioRunLineageReader，不读取真实外部仓库。

- [ ] **Step 4: Incrementally migrate master_setting_changes.**

用 PRAGMA table_info 增量添加 source_baseline_id 和 creation_method，不重建表。给 source_baseline_id、source_scenario_run_id 建索引。旧行 creation_method 标记 Legacy，source_baseline_id 保持 null。

FrozenComparisonGovernanceProposalRequest 在现有可选 GovernanceContext 之后增加可选 string? SourceScenarioRunId = null，以保持反序列化兼容；端点在生成 ScenarioDerived 建议前要求它非空，并验证该 run 属于相同冻结基线和 response。ProposeFromFrozenComparison 使用该真实 run id，不再构造 externalScenario/response 复合字符串。

- [ ] **Step 5: Add filtered query methods.**

MasterSettingsGovernanceService.ListChanges 接收可选 sourceBaselineId、sourceScenarioRunId。CoordinationLedgerService.List 接收可选 relatedScenarioRunId、relatedMasterSettingChangeId，并给两个已有列建索引。所有 SQL 使用参数，不拼接用户值。

ScenarioRunPersistenceService.List 接收可选 baselineSnapshotId、externalScenarioId。新增 BaselineLineageQueryService，返回一个冻结基线直接关联的 scenario runs、master-setting changes，以及通过这些 run/change 关联的 coordination items；按 ID 去重，不猜测空关联。

类型和方法签名固定为：

~~~csharp
public sealed record BaselineLineageResult(
    string BaselineSnapshotId,
    IReadOnlyList<ScenarioRunSummary> ScenarioRuns,
    IReadOnlyList<MasterSettingChangeSummary> MasterSettingChanges,
    IReadOnlyList<CoordinationItem> CoordinationItems);

public interface IBaselineLineageQueryService
{
    BaselineLineageResult Get(string baselineSnapshotId);
}
~~~

BaselineLineageQueryService 实现该接口，构造函数精确注入 ScenarioRunPersistenceService、MasterSettingsGovernanceService、CoordinationLedgerService。Get 先按 baseline 查询 runs 和 changes，再分别按每个 run/change 查询 coordination，最后按 ItemId 去重和 CreatedAtUtc 排序。baselineSnapshotId 为空时抛 ArgumentException；没有关联返回三个空集合。

- [ ] **Step 6: Expose internal query filters.**

扩展现有 GET：

- /api/master-settings/changes?sourceBaselineId=&sourceScenarioRunId=
- /api/coordination-items?relatedScenarioRunId=&relatedMasterSettingChangeId=
- /api/scenario-runs?baselineSnapshotId=&externalScenarioId=
- /api/current-baselines/{snapshotId}/references

保持无筛选参数时现有响应兼容。不得增加任何发布、自动批准或协议转发 endpoint。

- [ ] **Step 7: Run tests and confirm GREEN.**

额外验证 scenario comparison 保存 → 生成两个 ScenarioDerived change → 创建三个 coordination item → 两个方向查询都返回正确集合，且所有状态仍不自动变化。

- [ ] **Step 8: Commit the lineage model.**

~~~powershell
git add src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPersistenceService.cs src/AdaptiveSopDdsop.Web/Domain/MasterSettingsGovernanceService.cs src/AdaptiveSopDdsop.Web/Domain/CoordinationLedgerService.cs src/AdaptiveSopDdsop.Web/Domain/BaselineLineageQueryService.cs src/AdaptiveSopDdsop.Web/Program.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: link scenarios decisions and actions"
~~~

---

## Task 7: Repair only the known mojibake smoke records and prove Unicode round trips

**Files**

- Create: src/AdaptiveSopDdsop.Web/Domain/LocalDatabaseRepairService.cs
- Modify: src/AdaptiveSopDdsop.Web/Program.cs
- Modify: tests/AdaptiveSopDdsop.Tests/Program.cs

- [ ] **Step 1: Add scoped, idempotent repair tests.**

注册并实现：

- TestKnownSmokeRecordRepairIsScopedAuditedAndIdempotent
- TestSqliteRoundTripsChineseWithoutQuestionMarks

在临时 SQLite 中种入精确目标、相似但不相同的控制记录和正常中文记录。第一次 repair 必须：

- 删除 coordination item 09944ca75dfa4efab765d1481c860709 及其审计；
- 删除 coordination item 8aead2083210423db98e9f35924c7f8e 及其审计；
- 只在 snapshot_number == BASE-20260714-002 且 created_by == Codex ?? 时改为 Codex 烟测；
- 不改写已有 baseline audit message；
- 新增一条 DataRepairApplied corrective audit；
- 记录 repair journal。

第二次 repair 必须零变化。相似 ID、其它 BASE 版本和正常中文必须完全不变。

- [ ] **Step 2: Run tests and confirm RED.**

当前没有 repair service；SQLite 的两个事项和基线创建人仍会显示乱码。

- [ ] **Step 3: Implement an explicit one-time transaction.**

LocalDatabaseRepairService 创建 local_data_repairs 表，repair id 固定为 **2026-07-15-smoke-mojibake-v1**。一个事务内先检查 journal，再精确删除两个事项审计与事项；基线只在 number 和旧值同时匹配时修复。

公开签名固定为：

~~~csharp
public sealed record LocalDatabaseRepairResult(
    string RepairId,
    bool WasAlreadyApplied,
    int DeletedCoordinationItems,
    int DeletedCoordinationAuditEvents,
    int RepairedBaselines,
    int AddedBaselineAuditEvents);

public interface ILocalDatabaseRepairService
{
    LocalDatabaseRepairResult Apply();
}
~~~

LocalDatabaseRepairService 实现该接口，构造函数只接收 string databasePath。

由于 current_baseline_snapshots 有不可变 update trigger，修复事务只能：

1. 读取并保存触发器 SQL；
2. 暂时删除该单一 update trigger；
3. 执行带 snapshot_number 和 created_by 双条件的单行 UPDATE；
4. 立即用保存的原 SQL 恢复触发器；
5. 插入 corrective audit；
6. 插入 repair journal；
7. 提交。

任何异常必须回滚，触发器状态不得遗失。不得通用化为任意 SQL repair API。

- [ ] **Step 4: Register repair once at application startup.**

在所有四个现有 SQLite service 已 EnsureSchema 之后调用 repair.Apply()。不改公开演示或 contract 初始化。若本机不存在目标行，repair journal 仍记录 0 行修复，之后不重复扫描。

- [ ] **Step 5: Run tests and confirm GREEN.**

中文往返覆盖 title、owner、decision、rationale、outcome、created_by。断言不含 Unicode 替换字符和连续问号。

- [ ] **Step 6: Commit the repair.**

~~~powershell
git add src/AdaptiveSopDdsop.Web/Domain/LocalDatabaseRepairService.cs src/AdaptiveSopDdsop.Web/Program.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "fix: repair known local smoke records safely"
~~~

---

## Task 8: Replace stage scrolling with hierarchical hash-routed view switching

**Files**

- Modify: src/AdaptiveSopDdsop.Web/Pages/Index.cshtml
- Modify: src/AdaptiveSopDdsop.Web/wwwroot/js/app.js
- Modify: src/AdaptiveSopDdsop.Web/wwwroot/css/site.css
- Modify: tests/AdaptiveSopDdsop.Tests/Program.cs

- [ ] **Step 1: Add hierarchical-navigation failure tests.**

注册并实现：

- TestFiveStageNavigationUsesHierarchicalViewSwitching
- TestWorkspaceNavigationRemovesScrollObserverAndUsesHashState
- TestOnlySelectedStageOrChildViewIsVisible
- 更新 TestFiveStageNavigationPreservesValidationPages
- 更新 TestScenarioRunWorkspaceReplacesTeachingPageShell
- 更新 TestScenarioRunWorkspaceScriptFetchesWorkspaceData

断言五个一级入口顺序、22 个二级标题、每个标题最多六个汉字、aria-expanded/aria-controls、验证分组位于五阶段之后且公开演示为最末项。断言 app.js 不包含 IntersectionObserver，不用 scrollIntoView 做阶段导航，监听 hashchange，切页只修改 hidden/active/aria 状态，不重建表单 DOM。

- [ ] **Step 2: Run tests and confirm RED.**

当前平铺导航、IntersectionObserver 和 showWorkspaceContent() 会让测试失败。

- [ ] **Step 3: Build the exact sidebar hierarchy.**

一级及二级名称固定为：

| 一级 | 二级 |
| --- | --- |
| 历史回顾 | 经营结果、缓冲表现、能力约束 |
| 当前状态基线 | 会前快照、证据检查、版本冻结、审计记录 |
| 未来场景模拟 | 场景配置、方案比较、库存缓冲、时间缓冲、能力缓冲、供应风险、击穿分析 |
| DDOM 配置决策 | 结构设置、参数决策、临时调整、变更记录 |
| 行动和决策 | 问题清单、行动跟踪、决策记录、结果验证 |

每个一级使用 .nav-stage-group 和 button.nav-stage-toggle，带 data-stage-route、aria-expanded、aria-controls；二级使用 .nav-submenu 和 a.nav-subitem。只展开当前一级。一级点击同时打开 overview，二级点击打开其独立视图。

- [ ] **Step 4: Use the exact route map.**

| 页面 | Hash | 唯一 route target ID |
| --- | --- | --- |
| 历史一级 | #history-review-panel | history-review-panel |
| 经营结果 | #history-review-panel/operating-results | history-operating-results-view |
| 缓冲表现 | #history-review-panel/buffer-performance | history-buffer-performance-view |
| 能力约束 | #history-review-panel/capacity-constraints | history-capacity-constraints-view |
| 基线一级 | #current-baseline-panel | current-baseline-panel |
| 会前快照 | #current-baseline-panel/meeting-snapshot | baseline-meeting-snapshot-view |
| 证据检查 | #current-baseline-panel/evidence-review | baseline-evidence-review-view |
| 版本冻结 | #current-baseline-panel/version-freeze | baseline-version-freeze-view |
| 审计记录 | #current-baseline-panel/audit-records | baseline-audit-records-view |
| 场景一级 | #future-scenario-panel | future-scenario-panel |
| 场景配置 | #future-scenario-panel/scenario-config | scenario-run-panel |
| 方案比较 | #future-scenario-panel/plan-comparison | scenario-comparison |
| 库存缓冲 | #future-scenario-panel/inventory-buffer | buffer-trend-panel |
| 时间缓冲 | #future-scenario-panel/time-buffer | time-buffer-panel |
| 能力缓冲 | #future-scenario-panel/capacity-buffer | rccp-panel |
| 供应风险 | #future-scenario-panel/supply-risk | projected-supply-panel |
| 击穿分析 | #future-scenario-panel/breach-analysis | variance-panel |
| DDOM 一级 | #ddom-decision-panel | ddom-decision-panel |
| 结构设置 | #ddom-decision-panel/structure-settings | ddom-structure-settings-view |
| 参数决策 | #ddom-decision-panel/parameter-decision | ddom-parameter-decision-view |
| 临时调整 | #ddom-decision-panel/temporary-adjustment | ddom-temporary-adjustment-view |
| 变更记录 | #ddom-decision-panel/change-records | ddom-change-records-view |
| 行动一级 | #coordination-panel | coordination-panel |
| 问题清单 | #coordination-panel/issue-list | coordination-issue-list-view |
| 行动跟踪 | #coordination-panel/action-tracking | coordination-action-tracking-view |
| 决策记录 | #coordination-panel/decision-records | coordination-decision-records-view |
| 结果验证 | #coordination-panel/outcome-validation | coordination-outcome-validation-view |
| 白盒追踪 | #trace-panel | trace-panel |
| 公开演示闭环 | #public-demo-golden-loop-panel | public-demo-golden-loop-panel |

当前 buffer-trend-panel、rccp-panel、projected-supply-panel、variance-panel 嵌套在 saved-scenarios-panel。Task 8 必须把这四个完整 section 按原 ID 剪切为 main 下的顶层 sibling，不能复制 DOM；现有按 ID 的渲染逻辑不变。trace-panel 保持原 DOM 位置和内部结构，其父 saved-scenarios-panel 改为纯 .workspace-route-host；白盒 route 激活时只额外解除该 host 的 hidden，不把 host 标为 active route。公开演示 section 不移动、不重建。

每个业务 target 带 class="workspace-route-view"、data-workspace-route 和 hidden；两个保护页不加属性。移动后四个场景 target 的 requiredHostId 为 null，只有 trace-panel 的 requiredHostId 为 saved-scenarios-panel。

旧业务 hash 作为只读别名解析到新 route，再用 history.replaceState 规范化。测试从 workspaceRoutes 逐项解析 29 个 target，断言 document.getElementById(targetId) 唯一存在。

- [ ] **Step 5: Implement route state in app.js.**

新增：

- parseWorkspaceRoute(hash)
- formatWorkspaceHash(route)
- resolveWorkspaceRoute(route)
- navigateWorkspace(stageId, viewId, replace)
- applyWorkspaceRoute(route)
- setExpandedStageNavigation(stageId)
- setActiveWorkspaceNavigation(route)
- renderWorkspaceBreadcrumb(route)
- handleWorkspaceHashChange()

workspaceRoutes 为 Object.freeze 的显式常量；每项包含 stageId、viewId、targetId、title、parentTitle、requiredHostId。只有 trace-panel 的 requiredHostId 为 saved-scenarios-panel，其余为 null。applyWorkspaceRoute 的顺序固定为：遍历 workspaceRoutes 中 29 个唯一 targetId 并全部设 hidden → 移除 active/aria-current → 隐藏所有 .workspace-route-host → 解除目标 host → 解除唯一 target → 更新菜单、面包屑和 document.title。这样无需给两个保护页添加 class 或 data 属性。

删除 IntersectionObserver、旧 setActiveNav、activateTab、state.activeTab 和 [data-tab] 监听。showWorkspaceContent() 改为只显示 route target。applyExceptionToScenario() 和 useMasterSettingProposalResponse() 改为 navigateWorkspace()。

首次无 hash 时 replaceState 到 #history-review-panel；hashchange 支持浏览器前进/后退；只通过 hidden 切换，输入值在 DOM 中保留。

- [ ] **Step 6: Preserve protected page structures.**

以下内容不改 ID、按钮和内部结构：

- trace-panel、trace-list；
- public-demo-golden-loop-panel；
- refresh-public-demo、write-public-demo-payload；
- public-demo-scheduling-governance；
- public-demo-adapter-metadata-body、public-demo-adapter-boundary-list；
- public-demo-feedback-body、public-demo-business-user-view。

路由层只可对这两个顶层 section 切换 hidden。既有 Network 产品链接作为不透明旧入口保留，不读取其评分或响应。

- [ ] **Step 7: Style hierarchy without changing the visual language.**

保持深绿导航、白顶栏、浅灰画布、8px 卡片、现有 chip/table/drawer。新增子菜单缩进、连接线或左边界、当前一级/二级状态、键盘 focus、aria 展开指示。桌面侧栏固定，主视图自身滚动；移动端沿用现有折叠方式。

- [ ] **Step 8: Run tests and confirm GREEN.**

确认 DOM 中五个一级、22 个二级、两个验证入口都存在；任一 route 只显示一个顶层 view；公共演示仍最后。

- [ ] **Step 9: Commit navigation.**

~~~powershell
git add src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: switch workbench through hierarchical navigation"
~~~

---

## Task 9: Render credible Chinese business views and the independent time-buffer page

**Files**

- Modify: src/AdaptiveSopDdsop.Web/Pages/Index.cshtml
- Modify: src/AdaptiveSopDdsop.Web/wwwroot/js/app.js
- Modify: src/AdaptiveSopDdsop.Web/wwwroot/css/site.css
- Modify: tests/AdaptiveSopDdsop.Tests/Program.cs

- [ ] **Step 1: Add business-view and no-import tests.**

注册并实现：

- TestBusinessViewsTranslateInternalCodesWithoutMojibake
- TestFiveStageUiHasNoExternalImportOrProtocolInput
- TestTimeBufferViewUsesBackendResultsOnly
- 更新 TestScenarioRunWorkspaceExposesRequiredPanels

业务五阶段不得直接显示 DemoFixture、Fresh、Complete、Frozen、Open、InProgress、Escalated、Completed、InventoryBuffer、TimeBuffer、CapacityBuffer、SupplyRisk 等内部 code。公开演示闭环不受该断言影响。

页面不得包含 type=file、CSV/JSON import、网络评分输入、SDBR 输入、自动采纳、自动审批、自动生效或自动保存操作。

- [ ] **Step 2: Run tests and confirm RED.**

当前历史、基线和协调页面会直接显示英文 code；时间缓冲页面不存在。

- [ ] **Step 3: Split history and baseline content by intent.**

历史页面：

- 经营结果：只显示所选 26/52 周历史平均与趋势；
- 缓冲表现：控制点—保护关系、区域停留比例、进入红区次数、连续时长、恢复期、异常原因；
- 能力约束：CCR 利用率、上游保护消耗、理论/标准/经验证/计划可用/承诺负荷/保护能力/损失原因。

删除“当前占用”“库存口径”“红色供应窗口”；WIP 改为“在制品”；“缓冲 sizing”改为“缓冲定容”；Trace 表头改为“追踪”。null 显示“证据缺失”，不得经过 money()/number() 变为零。

当前状态基线：

- current-baseline-kpis 显示会前截止时刻、服务统计窗口、库存、在制品、积压、供应覆盖、峰值负荷；
- 证据检查显示来源、截止时间、新鲜度、完整性、缺失原因；
- 版本冻结显示完整性拦截和不可变说明；
- 审计记录显示中文事件标签，并列出该基线关联的场景运行、配置变更和行动事项；
- 对已知损坏的旧 audit message 不改数据库原文，业务 UI 根据 EventType 显示“历史烟测记录已由纠正审计保留”，并展示新的 DataRepairApplied 记录。

- [ ] **Step 4: Add manual/Demo scenario configuration controls.**

新增且只新增：

- future-assumption-mode：人工录入 / 演示模板；
- future-assumption-template；
- future-assumption-entered-by；
- future-assumption-effective-from；
- future-assumption-effective-through；
- future-assumption-evidence；
- external-time-control-point；
- external-time-delay-days。

选择演示模板后显示模板版本和“演示数据”；人工模式显示记录人、有效期、理由。所有表单提交到内部 compare API；没有文件选择或外部协议字段。

- [ ] **Step 5: Add the independent time-buffer view.**

DOM ID 固定为：

- time-buffer-panel
- time-buffer-evidence-chip
- time-buffer-kpis
- time-buffer-summary-body
- time-buffer-weekly-grid

前端只显示后端返回的控制点、保护活动、缓冲天数、最早红周、最大侵入、连续风险、恢复周、未恢复标识、影响产品和证据状态。不得在 JavaScript 计算侵入比例、红黄绿阈值或恢复周期。

- [ ] **Step 6: Separate future scenario visualizations.**

- 库存缓冲：红黄绿保护带叠加库存/净流量折线；
- 时间缓冲：控制点周矩阵和侵入比例；
- 能力缓冲：上游保护资源负荷柱、保护起点与计划可用阈值线；CCR 利用率另列；
- 供应风险：需求与承诺差距，不称为保护缓冲；
- 击穿分析：四类结果并列，未恢复明确标注。

不引入新图表依赖，继续使用现有 HTML/CSS/SVG 或 canvas 能力。

- [ ] **Step 7: Clarify 04 and 05 relationships in the UI.**

将页面标题改为“DDOM 配置决策”和“行动和决策”。04 不显示或解释 SDBR 反馈；结构设置区只保留 DDAE 内部结构设置和既有不透明 Network 入口。每个新 change 显示来源基线；ScenarioDerived 再显示真实来源运行。

03 的方案必须先通过内部 compare/save 保存成真实 run，之后才能“形成配置建议”或“创建行动事项”；前端不得把 ExternalScenarioId/ResponseId 当 run id。04 的人工创建必须选择冻结基线，场景派生创建必须带同基线的真实 run。05 显示可选关联场景/变更，并提供从 scenario/change 反查的只读列表。结果验证只记录人工确认的实际效果，不回写当前冻结基线、不自动改变 04 状态；下一周期由用户明确带入新的历史事实或候选基线。

- [ ] **Step 8: Centralize business-code translation.**

app.js 增加 baselineSourceLabel、freshnessLabel、completenessLabel、baselineStatusLabel、coordinationStatusLabel、breachScopeLabel、metricOrEvidenceMissing。至少映射：

- DemoFixture → 演示数据
- Fresh → 截止时间有效
- Complete → 完整
- Frozen → 已冻结
- Open → 待处理
- InProgress → 进行中
- Escalated → 已升级
- Completed → 已完成
- Proposed → 待评审
- InventoryBuffer → 库存缓冲
- TimeBuffer → 时间缓冲
- CapacityBuffer → 能力缓冲
- SupplyRisk → 供应风险

这些映射只用于五阶段业务区，不改公开演示 payload 和协议原值。

- [ ] **Step 9: Run tests and confirm GREEN.**

确认六个月/十二个月页面数字不同且合理、FPGA 只显示独立库存控制点、能力保护对象是 CCR 前面的资源、当前 KPI 只出现在当前状态基线。

- [ ] **Step 10: Commit the business views.**

~~~powershell
git add src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: render five-stage business views in Chinese"
~~~

---

## Task 10: Full regression, browser acceptance and protected-boundary audit

**Files**

- Verify: all modified files
- Verify unchanged: protected files and external repositories listed in Global constraints

- [ ] **Step 1: Run the complete test runner from a clean process.**

~~~powershell
dotnet run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
~~~

预期所有既有与新增测试通过。测试数量以最终 runner 输出为准，不硬编码为旧的 63。

- [ ] **Step 2: Build the solution with the required command.**

~~~powershell
dotnet build AdaptiveSopDdsop.sln --no-restore -m:1
~~~

预期 0 warnings、0 errors。

- [ ] **Step 3: Run static diff checks.**

~~~powershell
git diff --check
git status --short
git diff --name-status 4e39ec5
~~~

确认没有临时数据库、日志、截图、bin、obj 或 payload 文件被纳入提交。

- [ ] **Step 4: Prove protected DDAE files did not change.**

~~~powershell
git diff --exit-code 4e39ec5 -- src/AdaptiveSopDdsop.Web/Domain/DdsopConfigInboundContract.cs src/AdaptiveSopDdsop.Web/Domain/DdsopRuntimePlanningInputContract.cs src/AdaptiveSopDdsop.Web/Domain/ProductionInventoryQualityEvidenceContract.cs src/AdaptiveSopDdsop.Web/Domain/ProductionSupplierIdentitySourceContract.cs src/AdaptiveSopDdsop.Web/Domain/SdbrExecutionObjectEvidenceContract.cs src/AdaptiveSopDdsop.Web/Domain/PublicDemoGoldenLoopService.cs src/AdaptiveSopDdsop.Web/Domain/AdventureWorksProductDemoProfileService.cs src/AdaptiveSopDdsop.Web/Domain/ContractRepositoryPathResolver.cs src/AdaptiveSopDdsop.Web/appsettings.json tests/AdaptiveSopDdsop.Tests/Fixtures/sdbr-actual-planning-run-feedback.json tests/AdaptiveSopDdsop.Tests/Fixtures/sdbr-actual-variance-analysis-feedback.json
.\scripts\verify-protected-boundaries.ps1 -Baseline 4e39ec5
~~~

脚本必须输出每个受保护方法/DOM/端点块的 PASS 名称；任何块缺失、marker 不唯一或内容差异都使验收失败。只允许外围行号因插入内容变化。

- [ ] **Step 5: Prove external repositories retained their pre-existing state.**

~~~powershell
git -C C:\Users\吴一帆\Documents\DDAE_INTERFACE_CONTRACT status --short
git -C C:\Users\吴一帆\Documents\DDAE-NetworkStructure status --short
~~~

预期 CONTRACT 仍只保留实施前用户已有的未跟踪 docs/DDAE_INTERFACE_CONTRACT_HANDOFF_20260713.md；NetworkStructure 仍干净。不得删除、添加或格式化任何外部仓库文件。

- [ ] **Step 6: Start the application and run browser acceptance.**

使用独立本地端口启动应用。使用 browser:control-in-app-browser skill 验证：

1. 一级点击展开当前组并打开 overview；
2. 二级点击只切换主视图，不滚动到长页；
3. 刷新保持 hash，浏览器前进/后退恢复页面；
4. 场景表单在切页后值仍保留；
5. 6/12 月历史指标和趋势不同；
6. 当前会前 KPI 只在基线页；
7. 时间缓冲独立展示，供应风险不叫缓冲；
8. FPGA 只显示独立库存控制点；
9. 能力页分开显示 CCR 利用率和上游保护消耗；
10. 业务区无连续问号或非专有英文状态；
11. 白盒追踪页面可打开，现有 trace 可运行；
12. 公开演示闭环位于验证分组最底部，刷新和 payload 写出按钮行为不变。

- [ ] **Step 7: Inspect application logs and SQLite after browser acceptance.**

确认无 4xx/5xx、JavaScript exception 或 SQLite migration error。确认 repair journal 只有一条，两个烟测事项不存在，BASE-20260714-002 显示“Codex 烟测”且存在 corrective audit；其它快照和事项未变化。

- [ ] **Step 8: Run final verification again after stopping the service.**

再次运行测试、build、git diff --check 和 protected diff。只有第二次干净验证结果可用于完成声明。

- [ ] **Step 9: Request code review before integration.**

使用 superpowers:requesting-code-review 对照本计划逐项检查：数据来源、公式边界、缺失证据语义、03/04/05 关联、导航可访问性、验证页保护、协议零改动。修复审查问题后重新执行 Step 1–8。

- [ ] **Step 10: Finish the branch only after review and verification.**

使用 superpowers:finishing-a-development-branch 提供集成选择。除非用户明确要求，不自行 merge、push 或创建 PR。

## Final definition of done

- [ ] 历史 26/52 周来自明确事实且结果不同，异常费用可追溯，量级合理。
- [ ] 当前时点 KPI 位于“当前状态基线”，来源、截止时间、完整性、冻结版本和审计清楚。
- [ ] FPGA 是独立库存控制点，不是时间或能力保护。
- [ ] 能力保护只评估 CCR 上游、有工序证据的资源；CCR 利用率单独展示。
- [ ] 时间缓冲独立于能力和供应，缺证据不补零。
- [ ] 场景输入只有人工录入和内部 DemoFixture；没有外部导入或协议。
- [ ] 03、04、05 用真实基线/run/change/item ID 关联，历史未关联记录明确显示 Unlinked。
- [ ] 所有治理与行动状态只人工推进。
- [ ] 五个一级入口可展开 22 个二级入口，主区域通过 hash 切换而非阶段滚动。
- [ ] “DDOM 配置决策”“行动和决策”及全部二级标题符合确认命名。
- [ ] 白盒追踪和公开演示闭环的入口、DOM、按钮、API 和行为保持。
- [ ] CONTRACT、SDBR、Network、fixtures、协议回归断言无变化。
- [ ] 完整测试通过，build 0 warnings/0 errors，浏览器验收通过，工作树只含预期提交。
