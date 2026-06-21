# Phase 1 Spec: White-box DD S&OP Plan Run

日期：2026-06-20  
开发目录：`D:\Documents\DDAE`

## 1. 目标

Phase 1 的目标是把原型从“概念展示型驾驶舱”推进到“白盒计划推演引擎”。系统必须能展示输入参数如何沿时间轴转化为：

1. 未来缓冲水位。
2. 预计补货订单。
3. 需求驱动 RCCP 资源负载。
4. 供应商未来供应需求。
5. 可解释计算 trace。

核心链路：

```text
Net Flow + Buffer Parameters + Weekly Demand
-> Buffer Projection
-> Projected Replenishment Orders
-> Demand Driven RCCP
-> Projected Supply Requirements
-> Calculation Trace
```

## 2. 当前完成状态

| 状态 | 能力 | 说明 | 主要文件 | 验证 |
|---|---|---|---|---|
| Done | 用户级 .NET 9 SDK | 已安装在 `C:\Users\wyfch\.dotnet-sdk-9`，用于本项目构建测试。 | 无项目文件 | `dotnet run` / `dotnet build` |
| Done | 开发目录迁移 | 已将必要代码、测试、文档从 L2L4 同步到 `D:\Documents\DDAE`。 | 全项目 | 在 DDAE 下测试和构建通过 |
| Done | `.gitignore` | 忽略 `bin/`、`obj/`、日志、`node_modules/` 等生成内容。 | `.gitignore` | `git status --short` |
| Done | 白盒计划引擎 | 新增时间相位缓冲投影、预计补货订单和计算 trace。 | `DemandDrivenPlanningEngine.cs`, `Models.cs` | 测试覆盖 |
| Done | Demand Driven RCCP | RCCP 基于预计补货订单折算资源负载，而不是直接基于 forecast 爆炸。 | `DemandDrivenPlanningEngine.cs` | 测试覆盖 |
| Done | 服务层 Plan Run API | 新增 `GET /api/demand-driven-plan?horizonWeeks=12`。 | `DdsopScenarioService.cs`, `Program.cs` | API 检查 |
| Done | White-box Plan Run UI | 页面展示 Buffer Trend、Demand Driven RCCP、Projected Supply Requirements、Calculation Trace。 | `Index.cshtml`, `app.js` | 浏览器检查 |
| Done | Pre-build Campaign | 支持提前建库，把未来峰值补货压力前移到指定 build week。 | `DemandDrivenPlanningEngine.cs`, `Models.cs` | 测试覆盖 |
| Done | Resource Capacity Adjustment | 支持按周调整资源能力乘数，用于模拟加班、12 小时班、临时增班等。 | `DemandDrivenPlanningEngine.cs`, `Models.cs` | 测试覆盖 |
| Done | Projected Supply Requirements | 从补货订单按供应商、物料族、周聚合未来供应需求与价值。 | `DemandDrivenPlanningEngine.cs`, `Models.cs`, `SeedData.cs`, `app.js` | 测试/API/UI |
| Done | 产品级参考截图整理 | 已分析 `material` 目录下类似软件截图，并按业务内容重命名，作为后续产品级 UI/UX 与信息架构参考。 | `material/*.png` | 文件名与本 spec 记录 |

## 3. 已验证结果

当前在 `D:\Documents\DDAE` 下通过：

```text
19 test(s) passed.
已成功生成。
0 个警告
0 个错误
```

验证命令：

```powershell
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" build AdaptiveSopDdsop.sln
```

浏览器验证已确认：

- `White-box Plan Run` 区块存在。
- `Buffer Trend Graph` 有数据。
- `Demand Driven RCCP` 有数据。
- `Projected Supply Requirements` 有数据。
- `Calculation Trace` 有数据。
- 控制台无 error。

## 4. 未完成项

| 状态 | 能力 | 说明 | 建议优先级 |
|---|---|---|---|
| Pending | Scenario Run 对比 | 需要让用户在页面配置 Pre-build、资源日历、补货规则，运行 Baseline vs Scenario 对比。 | 高 |
| Pending | MOQ / 订货周期模拟 | 需要支持修改 MOQ、固定订货周期、指定周一/三/五补货等规则，并重算缓冲趋势。 | 高 |
| Pending | 预算 / 去年同期对照 | 需要把库存、服务、营运资金和年度预算、去年同期做对比。 | 高 |
| Pending | Constrained vs Unconstrained | 需要在 RCCP 上区分约束与非约束计划，并展示缺口与可行动作。 | 高 |
| Pending | Scenario 持久化 | 需要保存场景输入、运行结果、审批状态、计算 trace，形成审计链。 | 高 |
| Pending | 优化引擎 Solver Adapter | 需要在架构上预留 `IOptimizationSolver` / Solver Adapter，第一版默认 OR-Tools，未来可接 Gurobi。 | 高 |
| Pending | Supplier Collaboration 视图 | 需要按供应商筛选 projected supply requirements，并支持导出或协同材料。 | 中 |
| Pending | 图形化趋势图 | 当前是表格展示，需要升级为趋势图/负载图/供应需求图。 | 中 |
| Pending | 数据导入 | 需要从 CSV/Excel 或后续数据库导入 SKU、库存、需求、routing、供应商来源。 | 中 |
| Deferred | 多用户权限 | 当前阶段暂不做登录、角色权限和审批权限细分。 | 后续 |
| Deferred | 数据库持久化 | 当前阶段先用内存/seed data，后续再落数据库。 | 后续 |

## 5. 后续开发建议

下一步应进入：

```text
Scenario Run Workspace
```

最小产品级范围：

1. 页面上配置一个场景：
   - Pre-build 数量与 build week。
   - 资源能力调整。
   - MOQ / 订货周期规则。
2. 点击运行。
3. 同屏比较 Baseline 与 Scenario：
   - 缓冲水位。
   - 补货订单。
   - RCCP 峰值与平均负载。
   - 供应商需求。
   - 营运资金。
4. 保存 trace 供审计。

## 6. 架构与界面约束

### 6.1 优化引擎 Solver Adapter

当前 `DemandDrivenPlanningEngine` 定位为白盒推演 / 仿真引擎，用于回答“某个场景会发生什么”。后续如果加入优化能力，必须通过 Solver Adapter 接入，不允许把具体求解器直接写死在业务服务或 UI 逻辑中。

推荐架构：

```text
Scenario Simulation Engine
-> explains what a scenario does

IOptimizationSolver
-> OrToolsOptimizationSolver
-> FutureGurobiOptimizationSolver
```

第一版默认选择：

- 使用 OR-Tools 作为首选 C# 优化求解器。
- 先实现“推荐 3 个可解释方案”，而不是不可解释的黑盒全自动最优解。
- 推荐方案至少覆盖：服务优先、库存资金优先、产能平衡优先。
- Gurobi 作为未来可选高级求解器，仅在模型规模、求解性能、商业许可和部署条件明确后接入。

Solver Adapter 的目的：

- 保持优化引擎可替换。
- 避免产品早期被商业求解器许可和部署绑定。
- 让场景推演与优化推荐分层：推演负责解释结果，优化负责推荐值得比较的方案。

### 6.2 中文界面要求

产品界面默认使用中文。后续新增页面、按钮、表格列、图表标题、提示信息、空状态、错误信息和业务标签，均应优先使用中文表达。

允许保留英文的情况：

- 行业通用缩写：DD S&OP、DDS&OP、DDOM、RCCP、MOQ、ADU、FDU、ROI、SKU。
- 已作为模型字段或技术 trace 的英文标识，但 UI 层应尽量提供中文解释。
- 外部资料截图中的原始英文，不影响本系统自身界面要求。

## 7. 产品级参考截图资产

`D:\Documents\DDAE\material` 下的截图已经按业务内容重命名。后续开发 Scenario Run Workspace、RCCP、Buffer Trend、Constrained/Unconstrained 和异常分析页面时，应优先参考以下资产。

| 参考方向 | 参考图片 | 可供借鉴内容 | 对后续开发的影响 |
|---|---|---|---|
| S&OP 总览 / Buffer Trend Dashboard | `D:\Documents\DDAE\material\sop-buffer-trend-dashboard.png`<br>`D:\Documents\DDAE\material\sop-buffer-trend-dashboard-filters.png`<br>`D:\Documents\DDAE\material\sop-buffer-trend-demand-usage-metrics.png` | 顶部 KPI 卡片、当前库存金额、目标库存金额、短缺比例、红黄绿蓝缓冲分布、当前库存 vs 目标库存趋势、右侧业务筛选器。 | 当前 White-box Plan Run 不能只是一组表格，应逐步升级为“管理层 KPI + 计划员明细 + 筛选联动”的工作台。 |
| Past Period / Variance Analysis | `D:\Documents\DDAE\material\sop-past-period-analysis-exception-table.png`<br>`D:\Documents\DDAE\material\sop-past-period-analysis-selected-chart.png`<br>`D:\Documents\DDAE\material\sop-past-period-analysis-filtered-shortage-detail.png` | 上方 SKU 异常表，下方选中 SKU 趋势图；异常字段包括 shortage days、service level、min exec、high demand、net flow、lead time、spike。 | 后续应做“异常列表驱动场景分析”，让用户先定位异常 SKU，再进入场景模拟，而不是从空白参数表开始。 |
| Duration / Zone Stability | `D:\Documents\DDAE\material\sop-duration-by-planning-zone.png` | 按 SKU 统计进入某个缓冲区的次数、最长/最短/平均停留期、总天数。 | 可作为缓冲稳定性分析的补充指标，用于识别长期蓝区、长期红区或频繁穿越边界的 SKU。 |
| Simulation Properties | `D:\Documents\DDAE\material\sop-simulation-properties-sop-settings.png`<br>`D:\Documents\DDAE\material\sop-simulation-properties-rccp-thresholds.png` | S&OP 和 RCCP 分页配置、last run、starting date、weeks to simulate、daily usage horizon、order spike、critical/high/medium 阈值、Save then Run / Save w/o Running。 | Scenario Run Workspace 需要明确“配置、保存、运行”的操作语义，并显示最近一次运行状态。 |
| Item Simulation / Part Sandbox | `D:\Documents\DDAE\material\sop-part-simulation-settings-empty-graph.png`<br>`D:\Documents\DDAE\material\sop-part-simulation-results-buffer-trend.png`<br>`D:\Documents\DDAE\material\sop-part-simulation-daily-usage-type-settings.png` | 单 SKU 选择、仿真结果指标、缓冲趋势图、service level、average inventory、days stocked out、orders generated、MOQ、order multiple、order cycle override、ADU/FDU/Blend。 | MOQ、订货倍数、订货周期、ADU/FDU 等参数应做成计划员可操作配置，而不是隐藏在后端 seed data 中。 |
| Simulation Item Detail | `D:\Documents\DDAE\material\sop-simulation-daily-usage-activity-list.png`<br>`D:\Documents\DDAE\material\sop-simulation-daily-usage-status-list.png`<br>`D:\Documents\DDAE\material\sop-simulation-item-projected-buffer-trend.png`<br>`D:\Documents\DDAE\material\sop-simulation-item-properties-detail.png` | SKU 风险列表、状态标签、底部 All Activity / Supply Orders / Demand Allocations / Analytics / Projection / Properties / Buffer Sizing / BOM / Notes 标签页。 | 产品级页面应形成“列表选择 + 下方面板详情”的工作台结构，支持从风险列表钻取到订单、属性、缓冲和 BOM。 |
| Demand Driven RCCP | `D:\Documents\DDAE\material\sop-rough-cut-capacity-load-heatmap.png`<br>`D:\Documents\DDAE\material\sop-rough-cut-capacity-load-graph.png` | 资源汇总表、平均负载、峰值负载、状态、周度负载热力格、Load/Capacity/Variance 明细、负载柱状图与能力线。 | RCCP 应从当前表格升级为“资源汇总 + 周度热力格 + 选中资源负载图”的三层表达。 |
| Projected Supply / Inventory Grid | `D:\Documents\DDAE\material\sop-projected-supply-weekly-grid-filtered.png`<br>`D:\Documents\DDAE\material\sop-projected-supply-weekly-grid-buffer-trend.png`<br>`D:\Documents\DDAE\material\sop-projected-supply-inventory-family-grid.png` | 周度 projected supply/inventory 网格、物料族筛选、供应与库存行、合计行、选中 SKU 下方缓冲趋势。 | Projected Supply Requirements 后续应支持按供应商、物料族、SKU 钻取，并把网格与趋势图联动。 |
| Constrained vs Unconstrained S&OP | `D:\Documents\DDAE\material\sop-family-constrained-unconstrained-dashboard.png`<br>`D:\Documents\DDAE\material\sop-family-dashboard-with-filters.png` | 受限/不受限供给与库存对比、family aggregation、SKU projected inventory 热力表、预算/期间/单位/风险筛选。 | 后续必须补 Constrained vs Unconstrained 视图，帮助管理层看到产能约束对库存、供给和营运资金的影响。 |
| Master Data / Resource Editor | `D:\Documents\DDAE\material\editor-resource-master-data.png` | 资源主数据列表、resource calendar、resource count、创建/编辑资源入口。 | Resource Capacity Adjustment 应逐步连接到资源主数据和日历，而不是只作为一次性场景参数。 |

本批截图对开发思路的结论：

- 后续产品级实现应以“异常列表驱动场景”为主路径。
- Scenario Run Workspace 应支持 Baseline vs Scenario 对比，而不是只显示单次计划结果。
- RCCP 应采用热力格和负载图表达瓶颈，直接暴露平均负载、峰值负载和超载周。
- MOQ、订货倍数、订货周期、ADU/FDU、预建库存、产能调整都应成为显式可配置参数。
- 页面应保留白盒 trace，让用户能追踪“输入参数 -> 补货订单 -> RCCP 负载 -> 供应需求”的计算链路。

## 8. 测试策略

遵守 `docs/development_principles.md`：

- 高价值测试由人设计。
- 高频验证由机器执行。
- 新增引擎能力必须有行为测试。
- UI 只为关键业务入口和数据链路加测试，避免低价值测试膨胀。
