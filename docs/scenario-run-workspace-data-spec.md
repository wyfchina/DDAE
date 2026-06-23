# Scenario Run Workspace 基础数据与接口规格

日期：2026-06-21  
开发目录：`D:\Documents\DDAE`

## 1. 目标

Scenario Run Workspace 需要在开发 UI 和 API 前准备一套可验证的基础数据。数据必须能支撑以下用例：

1. Baseline vs Scenario 对比。
2. Pre-build Campaign 提前建库。
3. 产能调整场景。
4. MOQ / 订货周期调整。
5. 异常 SKU 驱动分析。
6. Constrained vs Unconstrained 对比。
7. 预算 / 去年同期对照。
8. 供应商协同与供应约束分析。

设计原则：

- 底层计算以 SKU / 物料为基础。
- 管理视图按产品族、资源、供应商、物料族聚合。
- 数据来源必须可替换，不能把 UI、服务层或计算引擎绑死到 seed data。
- 测试系统数据与生产系统数据必须分离，但业务逻辑只能有一套。

## 1.1 当前数据管理机制

当前阶段没有数据库。系统使用内存数据机制：

- `SeedData.Create()` 在启动时创建验证数据。
- `SeedScenarioWorkspaceDataSource` 实现 `IScenarioWorkspaceDataSource`，把 seed data 映射为 `ScenarioWorkspaceDataSet`。
- `GET /api/scenario-workspace-data?horizonWeeks=12` 从 `IScenarioWorkspaceDataSource` 读取数据。
- 页面、API 和计算逻辑不应直接依赖生产数据库或文件路径。

当前机制定位：

- 适合测试、演示、产品级 UI 验证和高价值行为测试。
- 不代表未来生产数据管理方式。
- 后续生产环境应通过数据库、ERP/MES adapter、文件导入或数据服务替换 `IScenarioWorkspaceDataSource` 的实现。

## 1.2 测试数据与生产数据分离原则

必须保持：

- 测试系统数据源与生产系统数据源物理隔离。
- 测试系统可使用 seed data、fixture、CSV/Excel 样例或测试数据库。
- 生产系统使用受控生产数据源，不从测试 fixture 或 seed data 读取业务数据。
- 测试数据不得与生产数据共用同一导入目录、连接字符串、数据库 schema 或对象存储 bucket。
- 环境切换只能发生在 composition root / 配置层，例如依赖注入注册、连接字符串、adapter 选择。

同时必须保证：

- 业务逻辑只有一套。
- `DemandDrivenPlanningEngine`、RCCP、缓冲投影、Projected Supply、Guardrail、Trace、Scenario Preview 等核心逻辑不得为测试系统复制实现。
- 测试和生产只能替换数据来源，不能替换业务规则。
- 如果生产数据结构不同，应通过 adapter 映射到统一领域模型，而不是在业务服务中写生产专用分支。

## 2. 基础数据集合

当前新增 `ScenarioWorkspaceDataSet`，作为 Scenario Run Workspace 的标准输入数据包。

当前 seed data 假设企业为卫星制造企业，核心产品族包括：

- 卫星平台
- 有效载荷
- 星载电子
- 热控结构

核心资源包括：

- AIT 总装集成大厅
- 热真空试验舱
- 洁净载荷装配间
- 星上电缆束工位

关键供应约束样例包括进口空间级 FPGA、光学载荷组件、蜂窝板与热控材料、星载电子单机。

| 数据 | 用途 | 覆盖用例 |
|---|---|---|
| `Families` | 产品族聚合、管理层视图、预算对比。 | Baseline 对比、Constrained vs Unconstrained |
| `Skus` | SKU 缓冲参数、ADU、DLT、MOQ、Order Cycle、成本。 | 所有 SKU 级推演 |
| `Inventory` | 当前库存、开放供应、合格需求，计算 Net Flow。 | Baseline、Buffer Trend |
| `Demand` | 未来周度需求。 | 补货投影、RCCP、供应需求 |
| `Resources` | 资源能力和资源名称。 | RCCP、产能调整 |
| `ResourceRoutings` | SKU 到资源的能力消耗映射。 | Demand Driven RCCP |
| `SupplierItemSources` | SKU 到供应商/物料族/成本映射。 | Projected Supply Requirements |
| `HistoricalDemand` | 历史实际需求、预测、服务水平、期末 Net Flow。 | Variance Analysis、异常 SKU |
| `BudgetBenchmarks` | 预算收入、去年同期收入、预算库存、去年同期库存。 | 财务/预算对照 |
| `ResourceCalendar` | 周度资源日历、检修、增班、能力倍数。 | 产能调整、受限计划 |
| `SupplierCapacityWindows` | 供应商周度承诺能力、交期、风险状态。 | 供应约束、供应商协同 |
| `ScenarioTemplates` | 预设场景动作组合。 | 会议中快速启动 What-if |
| `Guardrails` | 服务、资金、产能、供应、红区穿透业务栅栏。 | 是否可采纳、是否升级 AS&OP |

## 3. 接口模块

新增接口：

```csharp
public interface IScenarioWorkspaceDataSource
{
    ScenarioWorkspaceDataSet Load(ScenarioWorkspaceDataRequest request);
}

public interface IScenarioWorkspaceDataAdapter<in TSource>
{
    ScenarioWorkspaceDataSet Map(TSource source, ScenarioWorkspaceDataRequest request);
}
```

当前默认实现：

```csharp
SeedScenarioWorkspaceDataSource : IScenarioWorkspaceDataSource
```

后续可替换实现：

```text
CsvScenarioWorkspaceDataSource
ExcelScenarioWorkspaceDataSource
DatabaseScenarioWorkspaceDataSource
ErpScenarioWorkspaceAdapter
MesScenarioWorkspaceAdapter
SimioScenarioWorkspaceAdapter
```

接口目的：

- UI 和 API 只依赖 `IScenarioWorkspaceDataSource`。
- 外部数据结构通过 adapter 映射到 `ScenarioWorkspaceDataSet`。
- 未来更换 CSV、Excel、数据库、ERP、MES、Simio 时，不改 Scenario Run Workspace 的页面逻辑。

## 4. 预设场景模板

当前 seed 数据提供 4 类模板：

| 模板 | 动作 | 目的 |
|---|---|---|
| 促销峰值提前建库 | `Prebuild`, `DemandEvent` | 用淡季库存吸收未来需求峰值。 |
| 瓶颈资源临时增班 | `CapacityMultiplier` | 验证加班、增班或外协能否消除 RCCP 红区。 |
| MOQ 与订货周期调整 | `MoqOverride`, `OrderCycleOverride` | 比较订单频率、平均库存和服务风险。 |
| 受限与不受限计划对比 | `SupplierCapacityLimit`, `CapacityMultiplier` | 验证供应和资源约束对业务计划的影响。 |

## 5. Scenario Run Workspace 界面基线

界面开发参考 `D:\Documents\SDBR\sdbr\web` 下的 SDBR 计划员工作台，保持同一类运营工具风格。

参考文件：

- `D:\Documents\SDBR\sdbr\web\planner-workbench.html`
- `D:\Documents\SDBR\sdbr\web\planner-workbench.css`
- `D:\Documents\SDBR\sdbr\web\planner-workbench.js`

首版 Scenario Run Workspace 应采用以下结构：

1. 深色左侧导航：总览、数据准备、场景运行、方案比较、RCCP、缓冲趋势、供应需求、异常中心。
2. 白色顶部上下文条：主数据版本、运行快照、计划范围、系统健康、当前用户。
3. 场景配置区：Baseline、Scenario、模板选择、Pre-build、产能调整、MOQ / 订货周期、供应约束。
4. KPI 条：服务水平、平均库存金额、峰值负荷、平均负荷、红区 SKU、供应缺口。
5. Baseline vs Scenario 双列比较：指标网格、推荐标识、采纳 / 保存 / 重新运行动作。
6. Tab 工作区：缓冲趋势、Demand Driven RCCP、Projected Supply、Variance Analysis、Calculation Trace。

视觉与交互约束：

- 默认中文界面，行业缩写可保留英文。
- 使用紧凑表格、状态 chip、筛选工具条和 tab，不使用营销式 hero 页面。
- 风险颜色与 SDBR 一致：绿色健康，黄色预警，红色超载或阻塞，蓝色主操作。
- 表格、热力格和趋势图必须支持横向滚动或响应式降级，避免小屏内容溢出。

## 6. 验证要求

新增测试覆盖：

- Scenario Workspace seed data 必须包含 SKU、库存、需求、routing、供应商来源、历史实际、预算、资源日历、供应商能力窗口、场景模板和业务栅栏。
- 场景模板必须覆盖 Pre-build、产能调整、MOQ、订货周期和供应约束。
- Adapter 接口必须能把替代来源映射成 `ScenarioWorkspaceDataSet`，并支持范围过滤。

当前验证命令：

```powershell
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" build AdaptiveSopDdsop.sln
```
