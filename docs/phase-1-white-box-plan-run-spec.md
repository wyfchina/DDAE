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
| Done | 单 SKU 仿真工作台 | 已在缓冲 / 库存趋势区补齐选中 SKU 的活动列表、属性、缓冲 sizing、BOM、订单明细和 trace。活动解释需求消耗、订货周期复核等待、补货订单和提前建库；订单明细追溯需求订单、补货订单、供应商、资源能力占用和供应缺口。 | `BufferTrendWorkspaceService.cs`, `ScenarioWorkspaceData.cs`, `Index.cshtml`, `app.js`, `site.css` | 测试/构建 |
| Done | 订货周期复核规则纠偏 | 时间相位缓冲投影已修正为：净流动量位于黄区上沿及以下只是补货条件；只有到订货周期复核点才生成补货订单，并补到绿区上沿。非复核周进入黄区只保留 trace，不生成补货订单。依据参考 `D:\Documents\simio\DDMRP的文档.pdf` 中 DDMRP replenishment policy 说明，以及 `material` 目录缓冲趋势截图。 | `DemandDrivenPlanningEngine.cs`, `DdmrpCalculator.cs`, `Program.cs`, `app.js` | 新增订货周期测试 + 浏览器检查 |
| Done | Constrained vs Unconstrained 统一约束视图 | 已新增统一约束模型、`ConstraintWorkspaceService` 与 `GET /api/constraint-workspace`，并让 Scenario Preview 返回 `constraints`。RCCP tab 展示资源受限/不受限汇总、缺口热力格、资源明细、动作建议和 trace；预计供应页展示供应商/物料族/周的不受限需求、受限能力、缺口和风险。 | `ConstraintWorkspaceService.cs`, `ScenarioWorkspaceData.cs`, `Program.cs`, `Index.cshtml`, `app.js` | 测试/构建/浏览器检查 |
| Done | 目标流速与可定制采纳约束 | Scenario Preview 已补充流速指数，首页显示产品族目标流速；场景配置区新增采纳约束下拉，可按综合平衡、服务优先、流速优先、现金优先、产能优先、供应优先生成非持久化采纳建议。当前为预览判定，不替代后续审批流。 | `ScenarioWorkspaceData.cs`, `ScenarioRunPreviewService.cs`, `Index.cshtml`, `app.js`, `site.css` | 测试/构建 |
| Done | 状态解释与前端逻辑收敛 | 已补充缓冲趋势变化指标说明、供应商黄/红状态原因，并清理前端旧缓冲/RCCP/供应估算函数；前端只做筛选和展示，业务计算统一来自后端领域服务。 | `SupplierCollaborationWorkspaceService.cs`, `ScenarioWorkspaceData.cs`, `app.js`, `site.css`, `Program.cs` | 测试/构建/浏览器检查 |
| Done | Scenario 持久化与审计链 | 已新增 SQLite 本地库、场景保存 API、最近保存列表、详情快照和 append-only 审计链。保存时后端按同一请求重新运行 Scenario Preview，保存 request/result/trace 和关键指标；Preview API 仍保持非持久化。 | `ScenarioRunPersistenceService.cs`, `ScenarioWorkspaceData.cs`, `Program.cs`, `Index.cshtml`, `app.js`, `site.css` | 测试/构建 |
| Done | Master Settings Governance | 已新增 DDOM 主设置治理工作区，可从 Scenario Preview 生成主设置变更建议，保存为 SQLite 治理记录，查看详情、审计链并按 Proposed -> Reviewed -> Approved -> Effective -> Expired 顺序流转。 | `MasterSettingsGovernanceService.cs`, `ScenarioWorkspaceData.cs`, `Program.cs`, `Index.cshtml`, `app.js`, `site.css` | 测试/构建 |
| Done | IOptimizationSolver / Solver Adapter + Gurobi / OR-Tools | 已新增 Solver Adapter、Gurobi 与 OR-Tools 可选求解器、优化推荐服务和 `POST /api/scenario-runs/optimize`。求解器只选择候选动作组合，最终库存、RCCP、供应和约束结果仍由现有 Scenario Preview 白盒重算；推荐不会自动采纳、不会保存、不会审批。Gurobi 为默认选项，OR-Tools Adapter 第一版使用同一 0/1 候选组合约束模型的内置枚举求解实现，后续可替换为 `Google.OrTools` CP-SAT 实现。 | `GurobiOptimizationSolver.cs`, `OrToolsOptimizationSolver.cs`, `ScenarioOptimizationService.cs`, `ScenarioWorkspaceData.cs`, `Program.cs`, `Index.cshtml`, `app.js`, `site.css` | 测试/构建 |
| Done | 界面文案、解释与业务流程重排 | 已按“异常先导”重排左侧导航和页面运行时 DOM 顺序：总览、数据准备、异常识别、场景运行、方案比较、缓冲/库存趋势、RCCP 与约束、供应商需求、场景留痕、主设置治理、白盒追踪。界面状态链和主设置类型改为中文展示，运行预览字段与左侧导航新增悬浮业务解释。 | `Index.cshtml`, `app.js`, `site.css`, `tests/Program.cs` | 测试/构建 |
| Done | 通用折叠面板与导航提示优化 | 已为多层工作台区域增加通用折叠交互，覆盖数据准备、场景运行、缓冲/库存趋势、单 SKU 仿真、RCCP、供应商需求、异常识别、场景留痕和主设置治理。左侧导航不再显示问号图标，业务解释改为标题悬浮提示；字段级问号继续保留。 | `app.js`, `site.css`, `tests/Program.cs` | 测试/构建 |
| Done | DDMRP 参数完整性 | 已将 DDMRP 参数从字符串说明升级为结构化参数档案，覆盖每个 SKU 的解耦点、缓冲配置档案、ADU 来源与窗口、DLT 来源、变异因子、DAF、区域调整因子、MOQ、订货周期、生效窗口、参数状态和红/黄/绿上沿。数据准备区新增参数完整性检查表，单 SKU 仿真工作台同步显示完整参数和 sizing 公式；前端只展示后端计算结果。 | `Models.cs`, `ScenarioWorkspaceData.cs`, `SeedData.cs`, `SeedScenarioWorkspaceDataSource.cs`, `DdmrpCalculator.cs`, `BufferTrendWorkspaceService.cs`, `Index.cshtml`, `app.js`, `tests/Program.cs` | 测试/构建 |
| Done | 工作台体验优化 | 已新增模块专注视图、可调高度表格和右侧详情抽屉。可折叠二级模块支持“专注查看”，表格容器支持纵向拖动高度，DDMRP 参数默认紧凑展示并支持查看全部/仅看缺失，DDMRP 参数与业务栅栏支持点击行打开详情抽屉。第一版不做任意拖拽排序、不做自由布局、不持久化用户布局偏好。 | `Index.cshtml`, `app.js`, `site.css`, `tests/Program.cs` | 测试/构建 |
| Done | 产品族看板 | 已新增管理层产品族聚合视图，放在总览之后、数据准备之前。看板按产品族汇总服务、流速、库存、红黄绿风险、补货、RCCP、供应缺口和预算偏差，并支持产品族卡片、产品族×周风险网格、选中产品族风险/建议动作/RCCP贡献/供应需求下钻。Scenario Preview 会返回基准/预览两套产品族看板与 comparison；前端只展示后端结果，不重新计算业务指标。 | `ProductFamilyDashboardService.cs`, `ScenarioWorkspaceData.cs`, `ScenarioRunPreviewService.cs`, `Program.cs`, `Index.cshtml`, `app.js`, `site.css`, `tests/Program.cs` | 测试/构建 |
| Done | 多方案比较与候选动作影响矩阵 | 已将优化推荐从单张推荐卡升级为 DDS&OP 方案组合选择器：服务优先、库存资金优先、产能平衡优先三类推荐均输出 KPI、库存、服务、订单、供应缺口、峰值负荷和动作成本对比。候选动作库覆盖提前建库、产能缓冲、供应承诺和订货策略；每个候选动作输出服务影响、库存影响、资源负荷影响、供应缺口影响、补货订单影响、估算成本、约束说明和可行性状态。求解器依据候选动作影响矩阵、冲突约束、库存预算和成本预算选择组合，最终方案仍由 Scenario Preview 二次白盒重算；推荐卡片采用横向展开，不再纵向铺满页面。 | `ScenarioWorkspaceData.cs`, `ScenarioOptimizationService.cs`, `GurobiOptimizationSolver.cs`, `OrToolsOptimizationSolver.cs`, `Index.cshtml`, `app.js`, `tests/Program.cs` | 测试/构建 |
| Done | 专注查看与展开联动按钮规则 | 已实现体验规则：有“退出专注”按钮时不显示“收起”按钮；专注态标题不再触发展开/收起；“退出专注”直接退回到进入专注前的展开状态，此时恢复普通“收起”按钮。该修复只影响前端交互，不改业务计算。 | `app.js`, `site.css`, `tests/Program.cs` | 测试/构建 |
| Done | 专注视图右侧全宽展开 | 已将专注浮层宽度扩展到 `calc(100vw - 48px)`，优化推荐在专注态下按三列横向铺开，避免卡片被限制在左侧窄栏。普通页面状态仍保留横向滚动，不破坏主工作台布局。 | `site.css`, `tests/Program.cs` | 测试/构建 |
| Done | 产品族看板交互修正 | 已将产品族卡片从“全局筛选器行为”改为“看板选择器行为”：点击卡片只切换选中产品族详情，不再写入 `family-filter`，因此其它产品族卡片不会消失。看板提供“显示全部产品族”复位入口；选中产品族详情中的风险、RCCP 贡献和供应需求三栏按 SKU/周或供应商/物料族/周联动高亮。 | `app.js`, `site.css`, `tests/Program.cs` | 测试/构建 |
| Done | 主导航滚动感知 | 左侧导航已从单向点击跳转升级为双向感知：点击左侧仍滚动到右侧对应区域，右侧滚动到不同业务区时，左侧导航会通过 IntersectionObserver 自动高亮当前位置，降低长工作台迷失感。 | `app.js`, `tests/Program.cs` | 测试/构建 |

## 3. 已验证结果

当前在 `D:\Documents\DDAE` 下通过：

```text
46 test(s) passed.
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
- `product-family-dashboard-panel` 展示产品族 KPI、产品族卡片、周度风险网格和选中产品族下钻。
- `optimization-panel` 支持 Gurobi / OR-Tools 求解器选择，普通状态横向滚动推荐卡片，专注状态向右侧全宽三列展开。
- `multi-scenario-comparison-panel` 和 `candidate-impact-matrix-panel` 展示多方案 KPI 对比与候选动作影响矩阵。
- 专注查看状态隐藏“收起”，退出专注后恢复进入前的展开状态。
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
| Done | Scenario 持久化与审计链 | 已保存场景输入、服务端重算结果、关键指标、完整 result JSON 和审计事件；审批状态第一版固定为 `NotSubmitted`。 | 高 |
| Done | Master Settings Governance | 已将 Scenario Preview 参数和结果转化为 DDOM 主设置变更建议，并支持保存、看板、审计链和顺序状态流转；不回写 seed data，不推送 DDOM。 | 高 |
| Done | 优化引擎 Solver Adapter | 已实现 `IOptimizationSolver`、Gurobi Adapter 与 OR-Tools Adapter；界面可选择求解器，Gurobi 为默认。 | 高 |
| Done | Supplier Collaboration 视图（小范围） | 已完成供应商优先的需求钻取工作台：供应商汇总、供应商×周网格、选中供应商 SKU 贡献和建议动作；导出、协同状态、备注、供应商门户、保存与审批暂缓。 | 中 |
| Done | RCCP 图形化负载与热力格 | RCCP 已升级为资源负载图、周度热力格和 SKU 贡献明细。 | 中 |
| Done | 缓冲 / 库存趋势图形化 | 缓冲趋势已升级为趋势图、热力格、产品族汇总和 SKU 明细；真实 DDOM 历史运行图/控制图留待后续。 | 中 |
| Done | 解释性清理与单一业务逻辑 | 已解释 0 值变化指标和供应商黄/红状态原因；前端不再重算缓冲、RCCP 或供应需求。 | 高 |
| Pending | 数据导入 | 需要从 CSV/Excel 或后续数据库导入 SKU、库存、需求、routing、供应商来源。 | 中 |
| Deferred | 多用户权限 | 当前阶段暂不做登录、角色权限和审批权限细分。 | 后续 |
| Deferred | 主数据数据库持久化 | 当前阶段主数据仍用内存/seed data；场景运行记录已落 SQLite，后续主数据再接数据库/ERP/MES。 | 后续 |

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
4. 返回非持久化 trace 供预览审计。
5. 需要留痕时点击保存场景，后端重新运行同一请求并写入 SQLite 审计链。
6. 需要调整 DDOM 边界时，从预览生成主设置变更建议，保存后进入主设置治理看板。

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
| Done | Scenario 持久化与审计链 | 保存场景输入、服务端重算结果、关键指标和审计链；已保存场景可查看详情和审计事件。 | SQLite 表、保存/列表/详情/审计 API、保存场景 UI | 持久化服务测试 + UI 测试 |
| Done | Master Settings Governance | DDS&OP 不输出 MPS，而是把场景参数和结果转化为 DDOM 主设置治理记录。 | 主设置治理看板、当前主设置、预览生成建议、保存变更、审计链、顺序状态流转 | 服务/API/UI 测试 |
| Deferred | 审批流与权限 | 本阶段不做提交审批、审批意见、角色权限、登录和删除。 | 审批状态流转、用户身份、权限策略 | 后续治理专项 |
| Done | 优化推荐 Solver Adapter 实现 | 已支持 Gurobi / OR-Tools 可选推荐候选动作组合；OR-Tools 第一版为 Adapter 内置 0/1 组合求解实现，后续可替换为真实 CP-SAT。 | `IOptimizationSolver`、`GurobiOptimizationSolver`、`OrToolsOptimizationSolver`、`ScenarioOptimizationService` | 测试/构建 |
| Done | 产品级 RCCP | 已完成资源汇总、周度热力格、选中资源负载图、约束/非约束对比和规则型瓶颈动作建议第一版。 | RCCP 专项工作台 | 服务/API/UI 测试 |

## 6. 架构与界面约束

### 6.1 优化引擎 Solver Adapter

当前 `DemandDrivenPlanningEngine` 定位为白盒推演 / 仿真引擎，用于回答“某个场景会发生什么”。优化能力已通过 Solver Adapter 接入，不允许把具体求解器直接写死在业务服务或 UI 逻辑中。

推荐架构：

```text
Scenario Simulation Engine
-> explains what a scenario does

IOptimizationSolver
-> GurobiOptimizationSolver
-> OrToolsOptimizationSolver
```

第一版默认选择：

- 使用 Gurobi 作为默认优化求解器，同时允许用户在界面选择 OR-Tools。
- OR-Tools Adapter 第一版使用同一 0/1 候选组合约束模型的内置枚举求解实现，不新增外部 NuGet 依赖；后续可替换为 `Google.OrTools` CP-SAT。
- 已实现“推荐 3 个可解释方案”，而不是不可解释的黑盒全自动最优解。
- 推荐方案覆盖：服务优先、库存资金优先、产能平衡优先。
- 第一版优化模型使用 binary decision variable 选择候选动作组合；复杂 DDMRP 缓冲、RCCP、供应和约束结果仍由 Scenario Preview 白盒重算。
- `POST /api/scenario-runs/optimize` 接收 `solverName`，返回推荐方案、可运行 preview request、重新计算后的 preview result 和优化 trace；推荐不自动采纳、不保存、不审批。

Solver Adapter 的目的：

- 保持优化引擎可替换。
- 虽然 Gurobi 是首选，仍避免业务服务被单一许可、部署方式或求解器 API 绑定。
- 让场景推演与优化推荐分层：推演负责解释结果，优化负责推荐值得比较的方案。

### 6.2 数据环境边界

当前阶段主数据使用内存 seed data，不替代生产主数据数据库。`SeedData.Create()` 提供验证数据，`SeedScenarioWorkspaceDataSource` 通过 `IScenarioWorkspaceDataSource` 供 Scenario Run Workspace 读取。

Scenario 运行记录已进入第一版本地持久化：

- 默认 SQLite 数据库路径：`src/AdaptiveSopDdsop.Web/data/ddae-scenario-runs.db`。
- 数据库文件不纳入 Git：`.gitignore` 排除 `data/*.db` 和 `data/*.db-*`。
- 使用 `Microsoft.Data.Sqlite`，不引入 EF Core，不做复杂 migration；应用启动时由 `ScenarioRunPersistenceService` 自动建表。
- `POST /api/scenario-runs/preview` 仍保持非持久化，返回 `isPersisted=false`。
- `POST /api/scenario-runs` 保存时只接收 `name`、`description`、`createdBy` 和 `previewRequest`，后端重新运行 `ScenarioRunPreviewService.Preview(previewRequest)`，再保存 request/result/trace。
- `GET /api/scenario-runs?limit=50` 返回最近保存列表。
- `GET /api/scenario-runs/{runId}` 返回完整场景快照。
- `GET /api/scenario-runs/{runId}/audit` 返回 append-only 审计链。
- 第一版固定审计事件：`RunRequested`、`PreviewRecalculated`、`TraceCaptured`、`RunSaved`。
- 第一版状态：`status=Saved`，`approvalStatus=NotSubmitted`；提交审批、审批意见、权限和删除均留待后续。

Master Settings Governance 记录同样使用该 SQLite 本地库：

- 新增表：`master_setting_changes`、`master_setting_change_audit_events`。
- `GET /api/master-settings-workspace` 返回当前主设置、状态聚合、类型聚合和最近治理记录。
- `POST /api/master-settings/proposals/from-preview` 以后端重新运行的 Scenario Preview 生成主设置变更建议，不信任前端结果。
- `POST /api/master-settings/changes` 保存主设置变更建议。
- `GET /api/master-settings/changes?limit=50`、`GET /api/master-settings/changes/{changeId}`、`GET /api/master-settings/changes/{changeId}/audit` 分别读取列表、详情和审计链。
- `POST /api/master-settings/changes/{changeId}/status` 只允许 `Proposed -> Reviewed -> Approved -> Effective -> Expired` 顺序流转。
- 第一版固定审计事件：`ChangeProposed`、`PreviewRecalculated`、`ImpactCaptured`、`ChangeSaved`、`StatusChanged`。
- 第一版只保存治理记录和建议快照，不回写 seed data，不推送 DDOM，不做真实审批权限、删除或导出。

后续生产化必须遵守：

- 测试系统数据与生产系统数据分离。
- 业务逻辑只有一套。
- 测试与生产只能通过 data source / adapter / 配置切换数据来源。
- UI、API、计划引擎和场景服务不得直接绑定 seed data、测试 fixture 或生产数据库表结构。
- 生产数据结构如与测试数据不同，必须映射到统一领域模型后再进入业务逻辑。

### 6.3 DDAE 与 SDBR / DDOM 模块分工

以下分工是当前产品架构边界。它用于避免把 SDBR / DDOM 已承担的执行层能力重复建设到 DDAE / DDS&OP 中。资源日历、多班次、维护窗口、假期、加班、日能力和详细排程不作为 DDAE 缺口处理；DDAE 后续应通过 read model / API 消费 SDBR 的聚合结果、风险信号和执行反馈。

DDAE / DDS&OP 负责：

- 场景运行。
- 中期缓冲与库存趋势。
- 需求驱动 RCCP 周度聚合。
- 受限 / 不受限对比。
- 供应商需求钻取。
- 异常 SKU 驱动。
- Gurobi / OR-Tools 场景推荐。
- 主设置治理建议。

SDBR / DDOM 负责：

- 资源日历。
- 多班次 / 维护 / 假期 / 加班。
- 日能力。
- 详细 CP-SAT 排程。
- 工单释放。
- 现场执行反馈。
- 缓冲执行看板。
- Planning Run 发布治理。

### 6.4 中文界面要求

产品界面默认使用中文。后续新增页面、按钮、表格列、图表标题、提示信息、空状态、错误信息和业务标签，均应优先使用中文表达。

允许保留英文的情况：

- 行业通用缩写：DD S&OP、DDS&OP、DDOM、RCCP、MOQ、ADU、FDU、ROI、SKU。
- 已作为模型字段或技术 trace 的英文标识，但 UI 层应尽量提供中文解释。
- 外部资料截图中的原始英文，不影响本系统自身界面要求。

### 6.5 SDBR 风格基线

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
| Simulation Item Detail | `D:\Documents\DDAE\material\SOP仿真日用量-活动列表.png`<br>`D:\Documents\DDAE\material\SOP仿真日用量-状态列表.png`<br>`D:\Documents\DDAE\material\SOP仿真物料-预计缓冲趋势.png`<br>`D:\Documents\DDAE\material\SOP仿真物料-属性详情.png` | SKU 风险列表、状态标签、底部 All Activity / Supply Orders / Demand Allocations / Analytics / Projection / Properties / Buffer Sizing / BOM / Notes 标签页。 | 已完成单 SKU 仿真工作台第一版：从左侧库存/SKU 列表钻取后，可查看活动列表、属性、缓冲 sizing、BOM、订单明细、投影明细和计算 trace。 |
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

## 9. 实现 / Spec 对账记录

本节用于确认当前开发内容与本 spec 的一致性，避免能力只存在于代码或只存在于文档中。

### 9.1 已实现并已记录的 API

| API | 状态 | 对应能力 |
|---|---|---|
| `GET /api/scenario-workspace-data` | Done | 工作台基础数据、筛选、主数据上下文 |
| `GET /api/product-family-dashboard` | Done | 产品族看板 |
| `GET /api/exception-workspace` | Done | 异常 SKU 驱动场景入口 |
| `GET /api/buffer-trend-workspace` | Done | 缓冲 / 库存趋势与单 SKU 仿真钻取 |
| `GET /api/rccp-workspace` | Done | 产品级 RCCP 工作台 |
| `GET /api/constraint-workspace` | Done | 受限 / 不受限约束视图 |
| `GET /api/supplier-collaboration-workspace` | Done | 供应商需求钻取 |
| `POST /api/scenario-runs/preview` | Done | 非持久化场景预览与白盒 trace |
| `POST /api/scenario-runs/optimize` | Done | Gurobi / OR-Tools 可选优化推荐、多方案比较、候选动作影响矩阵 |
| `POST /api/scenario-runs` | Done | 保存场景，后端重算并写入 SQLite |
| `GET /api/scenario-runs` / `{runId}` / `{runId}/audit` | Done | 已保存场景列表、详情快照和审计链 |
| `GET /api/master-settings-workspace` | Done | 主设置治理工作区 |
| `POST /api/master-settings/proposals/from-preview` | Done | 从预览生成 DDOM 主设置变更建议 |
| `POST /api/master-settings/changes` | Done | 保存主设置变更请求 |
| `GET /api/master-settings/changes` / `{changeId}` / `{changeId}/audit` | Done | 主设置治理列表、详情和审计链 |
| `POST /api/master-settings/changes/{changeId}/status` | Done | 主设置状态顺序流转 |

### 9.2 已实现并已记录的领域服务

| 服务 / Adapter | 状态 | 说明 |
|---|---|---|
| `ScenarioRunPreviewService` | Done | 场景预览主链路，返回 baseline/scenario/comparison/trace |
| `ProductFamilyDashboardService` | Done | 管理层产品族聚合视图 |
| `BufferTrendWorkspaceService` | Done | 缓冲趋势、库存金额、单 SKU 仿真明细 |
| `RccpWorkspaceService` | Done | 资源汇总、周度热力格、SKU 贡献、动作建议 |
| `ConstraintWorkspaceService` | Done | 资源和供应侧受限 / 不受限缺口 |
| `SupplierCollaborationWorkspaceService` | Done | 供应商优先需求钻取 |
| `ExceptionWorkspaceService` | Done | 异常 SKU 聚合、信号、预设模板 |
| `ScenarioRunPersistenceService` | Done | 场景保存、详情、append-only 审计链 |
| `MasterSettingsGovernanceService` | Done | 主设置建议、保存、审计和状态流转 |
| `ScenarioOptimizationService` | Done | 候选动作库、动作影响矩阵、组合选择、二次白盒重算 |
| `GurobiOptimizationSolver` | Done | Gurobi Adapter，默认求解器 |
| `OrToolsOptimizationSolver` | Done | OR-Tools Adapter 第一版；内置 0/1 候选组合求解，后续可替换真实 CP-SAT |

### 9.3 已实现并已记录的 UI 入口

| UI 区域 / DOM | 状态 | 说明 |
|---|---|---|
| `product-family-dashboard-panel` | Done | 产品族 KPI、卡片、周度风险网格、选中产品族详情 |
| `scenario-run-panel` | Done | 场景配置、运行预览、求解器选择、优化推荐、保存场景 |
| `optimization-solver-select` | Done | Gurobi / OR-Tools 可选 |
| `optimization-recommendation-list` | Done | 普通状态横向滚动；专注态右侧全宽三列展开 |
| `scenario-comparison` | Done | Baseline vs Scenario、采纳建议、多方案比较、候选动作影响矩阵、预算对照 |
| `buffer-trend-panel` | Done | 经典山形缓冲趋势、需求脉冲、热力格、产品族汇总、单 SKU 仿真工作台 |
| `rccp-panel` | Done | RCCP 与约束、资源汇总、负荷热力格、资源明细、动作建议 |
| `projected-supply-panel` | Done | 供应商需求钻取 |
| `variance-panel` | Done | 异常 SKU 列表、信号明细、带入场景 |
| `saved-scenarios-panel` | Done | 已保存场景与审计链 |
| `master-settings-panel` | Done | 主设置治理、变更看板、当前主设置、建议、治理记录、审计链 |
| `workspace-focus-layer` | Done | 专注查看、退出专注、右侧全宽展开 |
| `workspace-detail-drawer` | Done | DDMRP 参数和业务栅栏详情抽屉 |

### 9.4 已确认留待后续的边界

| 后续项 | 状态 | 说明 |
|---|---|---|
| 生产主数据数据库 | Deferred | 当前主数据仍为 seed data + data source adapter；场景/治理记录已落 SQLite |
| 数据导入 | Pending | CSV/Excel/数据库/ERP/MES/Simio 数据接入后续专项 |
| 审批流与权限 | Deferred | 当前只保存与状态留痕，不做真实权限、提交审批和审批意见 |
| 供应商门户 / 回复 / 导出 | Deferred | 当前只做内部计划员供应商需求钻取 |
| 真实 OR-Tools CP-SAT NuGet 接入 | Deferred | 当前 OR-Tools Adapter 已可选，内部为 0/1 组合求解；后续可替换真实 CP-SAT |
| DDOM 写回 / 推送 | Deferred | 主设置治理只保存建议快照，不回写 seed data 或外部 DDOM |

### 9.5 当前对账结论

当前代码中的主要 API、领域服务、UI 入口、求解器选择、持久化与审计链均已在本 spec 中记录。`docs/development_principles.md` 已同步当前数据边界：主数据仍是 seed data，场景运行记录和主设置治理记录已使用 SQLite 本地持久化。当前无发现“已实现但未记录”的核心业务能力；后续新增能力必须继续同时更新代码、测试和 spec 状态。
