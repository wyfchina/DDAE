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
| Done | Scenario Workspace 基础数据接口 | 新增 `ScenarioWorkspaceDataSet`、`IScenarioWorkspaceDataSource`、`IScenarioWorkspaceDataAdapter<TSource>` 和 seed data source，用于支撑后续 Scenario Run Workspace 开发。 | `ScenarioWorkspaceData.cs`, `SeedScenarioWorkspaceDataSource.cs` | 测试覆盖 |
| Done | SDBR 界面风格基线 | 已确认后续 DDAE UI 需参考 SDBR 计划员工作台的布局、密度、颜色和组件风格。 | `development_principles.md`, 本 spec | 文档记录 |
| Done | Scenario Run Workspace 第一部分 | 已将首页替换为只读 Scenario Run Workspace，采用 SDBR 风格，接入 `/api/scenario-workspace-data`，展示 KPI、模板、Baseline vs Scenario、RCCP、缓冲趋势、供应需求、偏差分析和 trace。 | `Index.cshtml`, `Index.cshtml.cs`, `site.css`, `app.js` | 测试/构建/浏览器检查 |
| Done | Scenario Run Preview 工作台 | 已将工作台升级为可运行预览：支持模板选择、Pre-build、产能倍率、MOQ、订货周期、供应限制、预算对照和非持久化 trace；仍不做保存、审批、Solver 和产品级 RCCP 热力格。 | `ScenarioWorkspaceData.cs`, `ScenarioRunPreviewService.cs`, `Program.cs`, `Index.cshtml`, `app.js`, `site.css` | 测试/构建/浏览器检查 |
| Done | 产品级 RCCP 工作台 | 已将 RCCP 从表格/简单负载条升级为资源汇总、周度热力格、选中资源明细、SKU 贡献、动作建议和 Baseline/Scenario RCCP 对比。 | `RccpWorkspaceService.cs`, `ScenarioWorkspaceData.cs`, `Program.cs`, `Index.cshtml`, `app.js`, `site.css` | 测试/构建/浏览器检查 |
| Done | 异常列表驱动场景运行 | 已将 Variance Analysis 从只读历史偏差表升级为异常 SKU 驱动入口，支持异常聚合、严重度、推荐模板、信号明细和一键带入场景配置。 | `ExceptionWorkspaceService.cs`, `ScenarioWorkspaceData.cs`, `Program.cs`, `Index.cshtml`, `app.js`, `site.css` | 测试/构建/浏览器检查 |
| Done | 缓冲 / 库存趋势图形化 | 已将缓冲趋势从单表格升级为 KPI、经典 SKU 净流动量趋势图、SKU×周热力格、产品族汇总、选中 SKU 参数/订单/trace 明细，并接入 Scenario Preview 对比。图形遵循 material 中业内常见表达：左侧库存/物料选项，右侧红/黄/绿山形缓冲区，叠加净流动量位置、预计库存水位、目标库存点、基准/预览库存曲线和需求脉冲。山形缓冲区由服务端按每周时间相位 ADU 和 DDMRP zone 公式计算，不再由前端做视觉扰动。 | `BufferTrendWorkspaceService.cs`, `ScenarioWorkspaceData.cs`, `Program.cs`, `Index.cshtml`, `app.js`, `site.css` | 测试/构建/浏览器检查 |
| Done | 订货周期复核规则纠偏 | 时间相位缓冲投影已修正为：净流动量位于黄区上沿及以下只是补货条件；只有到订货周期复核点才生成补货订单，并补到绿区上沿。非复核周进入黄区只保留 trace，不生成补货订单。依据参考 `D:\Documents\simio\DDMRP的文档.pdf` 中 DDMRP replenishment policy 说明，以及 `material` 目录缓冲趋势截图。 | `DemandDrivenPlanningEngine.cs`, `DdmrpCalculator.cs`, `Program.cs`, `app.js` | 新增订货周期测试 + 浏览器检查 |
| Done | Constrained vs Unconstrained 统一约束视图 | 已新增统一约束模型、`ConstraintWorkspaceService` 与 `GET /api/constraint-workspace`，并让 Scenario Preview 返回 `constraints`。RCCP tab 展示资源受限/不受限汇总、缺口热力格、资源明细、动作建议和 trace；预计供应页展示供应商/物料族/周的不受限需求、受限能力、缺口和风险。 | `ConstraintWorkspaceService.cs`, `ScenarioWorkspaceData.cs`, `Program.cs`, `Index.cshtml`, `app.js` | 测试/构建/浏览器检查 |
| Done | 目标流速与可定制采纳约束 | Scenario Preview 已补充流速指数，首页显示产品族目标流速；场景配置区新增采纳约束下拉，可按综合平衡、服务优先、流速优先、现金优先、产能优先、供应优先生成非持久化采纳建议。当前为预览判定，不替代后续审批流。 | `ScenarioWorkspaceData.cs`, `ScenarioRunPreviewService.cs`, `Index.cshtml`, `app.js`, `site.css` | 测试/构建 |
| Done | 状态解释与前端逻辑收敛 | 已补充缓冲趋势变化指标说明、供应商黄/红状态原因，并清理前端旧缓冲/RCCP/供应估算函数；前端只做筛选和展示，业务计算统一来自后端领域服务。 | `SupplierCollaborationWorkspaceService.cs`, `ScenarioWorkspaceData.cs`, `app.js`, `site.css`, `Program.cs` | 测试/构建/浏览器检查 |

## 3. 已验证结果

当前在 `D:\Documents\DDAE` 下通过：

```text
35 test(s) passed.
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

- `/` 首屏已经替换为 Scenario Run Workspace，不再展示教学 hero。
- `workspace-kpis` 加载 7 个 KPI。
- `scenario-template-list` 加载 4 个场景模板。
- `scenario-comparison` 展示 Baseline / Scenario 双列比较。
- `buffer-trend-panel` 已升级为图形化缓冲 / 库存趋势工作区，包含 KPI、左侧库存选项、经典山形缓冲图、需求脉冲副图、热力格、产品族汇总和选中 SKU 明细。
- 基准状态下缓冲趋势图显示 1 条基准净流动量线；运行 Scenario Preview 后显示基准净流动量线与预览净流动量线。
- `rccp-panel`、`projected-supply-panel`、`variance-panel`、`trace-panel` 均存在。
- 缓冲趋势变化指标显示“尚未运行预览 / 预览与基准一致 / 预览方案 - 基准方案”的业务口径。
- 供应商周度格显示“缺口、需求 / 能力、状态原因”，可解释无缺口但黄色的供应风险。
- 前端 trace 明确说明缓冲趋势、RCCP、约束和供应商钻取来自后端领域服务。
- 移动宽度下页面本身无横向溢出，宽表在表格容器内滚动。
- 控制台无 error。

## 4. 未完成项

| 状态 | 能力 | 说明 | 建议优先级 |
|---|---|---|---|
| Done | Scenario Run 对比（预览版） | 已支持用户配置 Pre-build、资源能力倍率、MOQ、订货周期、供应限制，并运行 Baseline vs Scenario 非持久化对比。 | 高 |
| Done | MOQ / 订货周期模拟（预览版） | 已支持在 Scenario Preview 中覆盖 MOQ 与订货周期，并按订货周期复核点重算缓冲趋势和补货订单；订货倍数、周一/三/五固定补货仍留给后续产品化规则。 | 高 |
| Done | 预算 / 去年同期对照（预览版） | 已在预览结果中按产品族/周对照预算库存、去年同期库存与预览库存。 | 高 |
| Done | Constrained vs Unconstrained（统一约束视图） | 已统一表达资源和供应的不受限需求、受限能力、缺口、状态、动作建议和审计 trace；不截断补货订单、不改写供应计划。 | 高 |
| Pending | Scenario 持久化 | 需要保存场景输入、运行结果、审批状态、计算 trace，形成审计链。 | 高 |
| Deferred | 优化引擎 Solver Adapter | 架构原则已记录；本阶段不实现 `IOptimizationSolver`、OR-Tools 或 Gurobi 接入。 | 高 |
| Done | Supplier Collaboration 视图（小范围） | 已完成供应商优先的需求钻取工作台：供应商汇总、供应商×周网格、选中供应商 SKU 贡献和建议动作；导出、协同状态、备注、供应商门户、保存与审批暂缓。 | 中 |
| Done | RCCP 图形化负载与热力格 | RCCP 已升级为资源负载图、周度热力格和 SKU 贡献明细。 | 中 |
| Done | 缓冲 / 库存趋势图形化 | 缓冲趋势已升级为趋势图、热力格、产品族汇总和 SKU 明细；真实 DDOM 历史运行图/控制图留待后续。 | 中 |
| Done | 解释性清理与单一业务逻辑 | 已解释 0 值变化指标和供应商黄/红状态原因；前端不再重算缓冲、RCCP 或供应需求。 | 高 |
| Pending | 数据导入 | 需要从 CSV/Excel 或后续数据库导入 SKU、库存、需求、routing、供应商来源。 | 中 |
| Deferred | 多用户权限 | 当前阶段暂不做登录、角色权限和审批权限细分。 | 后续 |
| Deferred | 数据库持久化 | 当前阶段先用内存/seed data，后续再落数据库。 | 后续 |

## 5. 后续开发建议

下一步应进入：

```text
数据治理与生产化专项
```

已完成的 Scenario Run Workspace 预览版范围：

1. 页面上配置一个场景：
   - Pre-build 数量与 build week。
   - 资源能力调整。
   - MOQ / 订货周期规则。
2. 点击运行预览，不保存、不审批。
3. 同屏比较 Baseline 与 Scenario：
   - 缓冲水位。
   - 补货订单。
   - RCCP 峰值与平均负载。
   - 供应商需求。
   - 营运资金。
4. 返回非持久化 trace 供审计。

### 5.1 Scenario Run Workspace 工作清单

本清单用于约束 Scenario Run Workspace 当前阶段已经完成、仍需后续开发或明确暂缓的工作。产品级 RCCP、缓冲趋势、受限/不受限、异常驱动和供应商需求钻取均已完成第一版；后续重点转向数据治理、生产数据接入、持久化审批和更完整的 DDOM Master Settings。

| 状态 | 工作项 | 目标 | 主要输出 | 验证方式 |
|---|---|---|---|---|
| Done | 基础数据接口 | 让 Scenario Run Workspace 脱离固定 seed data，后续可替换 CSV/Excel/数据库/ERP/MES/Simio。 | `ScenarioWorkspaceDataSet`、`IScenarioWorkspaceDataSource`、`IScenarioWorkspaceDataAdapter<TSource>`、`GET /api/scenario-workspace-data` | 测试覆盖 seed data 与 adapter |
| Done | 卫星制造样例数据 | 把原验证数据改为卫星制造企业语境，避免用不相关行业数据做 UI 验收。 | 卫星平台、有效载荷、星载电子、热控结构、AIT、热真空试验、进口空间级 FPGA 等数据 | 测试覆盖产品族与用例数据 |
| Done | SDBR 风格基线 | 统一产品级 UI 风格。 | 深色左侧导航、顶部上下文条、浅灰画布、紧凑 KPI / 表格 / tab / chip | 文档记录 + 浏览器检查 |
| Done | DDAE 深绿供应链主题 | 保持 SDBR 系列结构，但形成 DDAE 自身产品识别。 | 深绿/青绿主色、深色导航、浅灰画布、风险色保留绿/黄/红 | CSS + 浏览器检查 |
| Done | 只读工作台替换首页 | 第一屏变成产品工作台，不再是教学/营销式驾驶舱。 | `scenario-workspace-app`、中文导航、顶部上下文、loading/error 状态 | DOM 测试 + 浏览器检查 |
| Done | 只读数据加载与筛选 | 前端从 workspace API 拉取数据，并支持计划员按维度收敛视图。 | 产品族、SKU、资源、供应风险筛选；刷新与清除筛选 | 脚本测试 + 浏览器检查 |
| Done | 只读 KPI 与模板预览 | 让会议一打开就能看到当前数据健康和可用情景模板。 | 服务水平、平均库存金额、峰值负荷、平均负荷、红区 SKU、供应缺口；4 类模板卡片 | DOM/浏览器检查 |
| Done | 静态 Baseline vs Scenario 框架 | 先建立对比面板的信息架构，暂不执行重算。 | Baseline / Scenario 双列比较、候选模板摘要、只读状态标识 | DOM/浏览器检查 |
| Done | Tab 工作区骨架 | 为后续可运行场景提供固定信息位。 | Buffer Trend、Demand Driven RCCP、Projected Supply、Variance Analysis、Calculation Trace | DOM/浏览器检查 |
| Done | Scenario Run Preview 输入模型 | 把模板动作和用户参数转为可提交的统一请求。 | `ScenarioRunPreviewRequest`，包含 horizon、template、filters、Pre-build、capacity、MOQ、order cycle、supplier limit、adoption constraint mode | 高价值模型/API 测试 |
| Done | Preview API | 支持不保存、不审批的动态预览。 | `POST /api/scenario-runs/preview`，返回 Baseline 与 Scenario 两套可比较结果 | 服务/API 行为测试 |
| Done | 场景配置面板 | 把只读模板卡片升级为可选择、可调整、可运行预览。 | 模板选择、SKU、Pre-build、能力倍率、MOQ、订货周期、供应限制、运行预览按钮、未保存状态提示 | UI DOM + 浏览器检查 |
| Done | 动态 Baseline vs Scenario | 点击运行后刷新 KPI、Buffer Trend、RCCP、供应需求、预算和 trace 对比。 | 双列 KPI 差异、趋势差异、关键风险提示 | API + 浏览器检查 |
| Done | MOQ / 订货周期模拟（预览版） | 让计划员显式修改 MOQ 与订货周期，订单倍数和固定周几补货暂缓。 | MOQ override、order cycle override、库存水位影响 | 引擎/服务行为测试 |
| Done | Pre-build 与产能调整预览 | 支持提前建库和资源能力倍率在 UI 中配置并重算。 | Pre-build 参数、CapacityMultiplier 参数、结果 trace | 引擎/API/UI 测试 |
| Done | 预算 / 去年同期对照（预览版） | 把场景结果与财务目标放在同一工作台判断。 | 预算库存、去年同期库存、预览库存、预算偏差状态 | 数据聚合测试 |
| Done | 异常 SKU 驱动入口 | 已从历史偏差数据聚合异常 SKU，支持点击异常行查看信号明细，并一键带入目标 SKU、全局筛选和推荐模板。 | 异常 KPI、异常 SKU 列表、异常信号明细、带入场景按钮 | UI/服务测试 |
| Done | 目标流速与采纳口径 | 目标流速从产品族主数据读取，预览指标返回流速指数；采纳约束下拉决定预览建议口径。 | 目标流速、流速指数、综合/服务/流速/现金/产能/供应优先 | UI/服务测试 |
| Done | 供应商需求钻取 | 已按供应商优先展示汇总、供应商×周网格、SKU 需求贡献、相关补货订单和建议动作；运行预览后同步刷新。 | 供应商能力限制、供应缺口、受影响 SKU、补货订单追溯 | 服务/API/UI 测试 |
| Done | 状态解释与口径说明 | 变化指标明确为“预览方案 - 基准方案”；未运行预览时 0 值显示为“尚未运行预览”；供应商黄/红格显示状态原因。 | `statusReason`、变化指标说明、服务端 trace 说明 | 服务/UI 测试 |
| Done | 前端业务逻辑收敛 | 清理旧版前端缓冲/RCCP/供应估算，首页 KPI、静态对比和 trace 消费后端服务结果。 | 单一业务逻辑、前端只筛选展示 | 脚本测试 |
| Deferred | 供应商协同门户 | 本阶段不做供应商回复、协同状态、备注、导出、保存、审批、权限和外部门户。 | 后续供应商协同专项 | 后续 |
| Done | Trace 审计包（非持久化） | 运行预览后返回并展示本次计算链路，先不落数据库。 | 输入参数、补货订单、负荷、供应需求、guardrail 判断的只读审计包 | API/快照测试 |
| Deferred | Scenario 持久化与审批 | 保存场景输入、结果、审批状态和审计链。 | 数据库表、审批流、权限 | 产品级 RCCP 后或并行专项 |
| Deferred | 优化推荐 Solver Adapter 实现 | 由 OR-Tools CP-SAT 推荐候选方案。 | `IOptimizationSolver`、`OrToolsCpSatOptimizationSolver` | 产品级场景推荐阶段 |
| Done | 产品级 RCCP | 已完成资源汇总、周度热力格、选中资源负载图、约束/非约束对比和规则型瓶颈动作建议第一版。 | RCCP 专项工作台 | 服务/API/UI 测试 |

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

### 6.2 数据环境边界

当前阶段使用内存 seed data，不使用数据库。`SeedData.Create()` 提供验证数据，`SeedScenarioWorkspaceDataSource` 通过 `IScenarioWorkspaceDataSource` 供 Scenario Run Workspace 读取。

后续生产化必须遵守：

- 测试系统数据与生产系统数据分离。
- 业务逻辑只有一套。
- 测试与生产只能通过 data source / adapter / 配置切换数据来源。
- UI、API、计划引擎和场景服务不得直接绑定 seed data、测试 fixture 或生产数据库表结构。
- 生产数据结构如与测试数据不同，必须映射到统一领域模型后再进入业务逻辑。

### 6.3 中文界面要求

产品界面默认使用中文。后续新增页面、按钮、表格列、图表标题、提示信息、空状态、错误信息和业务标签，均应优先使用中文表达。

允许保留英文的情况：

- 行业通用缩写：DD S&OP、DDS&OP、DDOM、RCCP、MOQ、ADU、FDU、ROI、SKU。
- 已作为模型字段或技术 trace 的英文标识，但 UI 层应尽量提供中文解释。
- 外部资料截图中的原始英文，不影响本系统自身界面要求。

### 6.4 SDBR 风格基线

后续 Scenario Run Workspace 和 DDAE 产品级页面应保持与 `D:\Documents\SDBR\sdbr\web` 下 SDBR 计划员工作台基本一致的界面风格。

参考文件：

- `D:\Documents\SDBR\sdbr\web\planner-workbench.html`
- `D:\Documents\SDBR\sdbr\web\planner-workbench.css`
- `D:\Documents\SDBR\sdbr\web\planner-workbench.js`

可复用的界面模式：

- 应用壳：深色左侧主导航 + 白色 sticky 顶部上下文条 + 浅灰画布 + 最大宽度工作区。
- 信息密度：面向运营会议和计划员操作，优先紧凑 KPI 条、筛选工具条、表格、分页、tab、状态 chip 和对比面板。
- 状态表达：蓝色表示主操作/选中态，绿色表示健康或推荐，黄色表示预警，红色表示风险或超载。
- 场景对比：采用 Baseline / Candidate 两列比较、推荐方案高亮、关键指标网格和决策动作区。
- 负荷与缓冲：资源负荷使用水平条、阈值线和超载红色提示；缓冲状态使用 Green / Yellow / Red chip 或矩阵。
- 响应式：小屏下左侧导航收起，KPI 和对比网格改为两列或单列，表格保留横向滚动。

对 DDAE 的具体约束：

- 不新增独立的营销式首页；第一屏就是 DD S&OP 工作台。
- 新页面应复用 SDBR 的组件语义和视觉节奏，必要时把样式变量迁移为 DDAE 自有 design tokens。
- Scenario Run Workspace 首版应优先实现“左侧导航 / 顶部上下文 / 场景配置 / KPI / Baseline vs Scenario / RCCP 与缓冲趋势 tab”的工作台结构。

## 7. 产品级参考截图资产

`D:\Documents\DDAE\material` 下的截图已经按业务内容重命名。后续开发 Scenario Run Workspace、RCCP、Buffer Trend、Constrained/Unconstrained 和异常分析页面时，应优先参考以下资产。

| 参考方向 | 参考图片 | 可供借鉴内容 | 对后续开发的影响 |
|---|---|---|---|
| S&OP 总览 / Buffer Trend Dashboard | `D:\Documents\DDAE\material\SOP缓冲趋势看板.png`<br>`D:\Documents\DDAE\material\SOP缓冲趋势看板-筛选器.png`<br>`D:\Documents\DDAE\material\SOP缓冲趋势看板-需求用量指标.png` | 顶部 KPI 卡片、当前库存金额、目标库存金额、短缺比例、红黄绿蓝缓冲分布、当前库存 vs 目标库存趋势、右侧业务筛选器。 | 当前 White-box Plan Run 不能只是一组表格，应逐步升级为“管理层 KPI + 计划员明细 + 筛选联动”的工作台。 |
| Past Period / Variance Analysis | `D:\Documents\DDAE\material\SOP历史期间分析-异常表.png`<br>`D:\Documents\DDAE\material\SOP历史期间分析-选中SKU图表.png`<br>`D:\Documents\DDAE\material\SOP历史期间分析-筛选后短缺明细.png` | 上方 SKU 异常表，下方选中 SKU 趋势图；异常字段包括 shortage days、service level、min exec、high demand、net flow、lead time、spike。 | 后续应做“异常列表驱动场景分析”，让用户先定位异常 SKU，再进入场景模拟，而不是从空白参数表开始。 |
| Duration / Zone Stability | `D:\Documents\DDAE\material\SOP计划区停留时长分析.png` | 按 SKU 统计进入某个缓冲区的次数、最长/最短/平均停留期、总天数。 | 可作为缓冲稳定性分析的补充指标，用于识别长期蓝区、长期红区或频繁穿越边界的 SKU。 |
| Simulation Properties | `D:\Documents\DDAE\material\SOP仿真属性-SOP设置.png`<br>`D:\Documents\DDAE\material\SOP仿真属性-RCCP阈值设置.png` | S&OP 和 RCCP 分页配置、last run、starting date、weeks to simulate、daily usage horizon、order spike、critical/high/medium 阈值、Save then Run / Save w/o Running。 | Scenario Run Workspace 需要明确“配置、保存、运行”的操作语义，并显示最近一次运行状态。 |
| Item Simulation / Part Sandbox | `D:\Documents\DDAE\material\SOP物料仿真设置-空图表.png`<br>`D:\Documents\DDAE\material\SOP物料仿真结果-缓冲趋势.png`<br>`D:\Documents\DDAE\material\SOP物料仿真-日用量类型设置.png` | 单 SKU 选择、仿真结果指标、缓冲趋势图、service level、average inventory、days stocked out、orders generated、MOQ、order multiple、order cycle override、ADU/FDU/Blend。 | MOQ、订货倍数、订货周期、ADU/FDU 等参数应做成计划员可操作配置，而不是隐藏在后端 seed data 中。 |
| Simulation Item Detail | `D:\Documents\DDAE\material\SOP仿真日用量-活动列表.png`<br>`D:\Documents\DDAE\material\SOP仿真日用量-状态列表.png`<br>`D:\Documents\DDAE\material\SOP仿真物料-预计缓冲趋势.png`<br>`D:\Documents\DDAE\material\SOP仿真物料-属性详情.png` | SKU 风险列表、状态标签、底部 All Activity / Supply Orders / Demand Allocations / Analytics / Projection / Properties / Buffer Sizing / BOM / Notes 标签页。 | 产品级页面应形成“列表选择 + 下方面板详情”的工作台结构，支持从风险列表钻取到订单、属性、缓冲和 BOM。 |
| Demand Driven RCCP | `D:\Documents\DDAE\material\SOP粗能力负荷热力图.png`<br>`D:\Documents\DDAE\material\SOP粗能力负荷图.png` | 资源汇总表、平均负载、峰值负载、状态、周度负载热力格、Load/Capacity/Variance 明细、负载柱状图与能力线。 | RCCP 应从当前表格升级为“资源汇总 + 周度热力格 + 选中资源负载图”的三层表达。 |
| Projected Supply / Inventory Grid | `D:\Documents\DDAE\material\SOP预计供应周度网格-筛选后.png`<br>`D:\Documents\DDAE\material\SOP预计供应周度网格-缓冲趋势.png`<br>`D:\Documents\DDAE\material\SOP预计供应与库存-产品族网格.png` | 周度 projected supply/inventory 网格、物料族筛选、供应与库存行、合计行、选中 SKU 下方缓冲趋势。 | Projected Supply Requirements 后续应支持按供应商、物料族、SKU 钻取，并把网格与趋势图联动。 |
| Constrained vs Unconstrained S&OP | `D:\Documents\DDAE\material\SOP产品族受限与不受限对比看板.png`<br>`D:\Documents\DDAE\material\SOP产品族看板-筛选器.png` | 受限/不受限供给与库存对比、family aggregation、SKU projected inventory 热力表、预算/期间/单位/风险筛选。 | 后续必须补 Constrained vs Unconstrained 视图，帮助管理层看到产能约束对库存、供给和营运资金的影响。 |
| Master Data / Resource Editor | `D:\Documents\DDAE\material\资源主数据编辑器.png` | 资源主数据列表、resource calendar、resource count、创建/编辑资源入口。 | Resource Capacity Adjustment 应逐步连接到资源主数据和日历，而不是只作为一次性场景参数。 |

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
