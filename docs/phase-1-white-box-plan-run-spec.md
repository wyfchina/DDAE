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
| Done | IOptimizationSolver / Solver Adapter + Gurobi / OR-Tools | 已将旧 Gurobi “场景推荐”链路重构为 Phase 8 候选动作组合选择器。`CandidateActionCombinationRequest`、`IOptimizationSolver`、Gurobi、OR-Tools 与纯 `CandidateActionCombinationSelector` 已迁入 `AdaptiveSopDdsop.NetworkStructure`，只在已验证候选动作影响矩阵上做 0/1 组合选择；DDS&OP 集成层负责构造候选动作矩阵，并把选中组合回到 Scenario Preview 白盒重算。组合不会自动采纳、不会保存、不会审批。旧 `ScenarioOptimizationService` 与 `/api/scenario-runs/optimize` 已删除。 | `NetworkOptimizationModels.cs`, `GurobiOptimizationSolver.cs`, `OrToolsOptimizationSolver.cs`, `CandidateActionCombinationService.cs`, `ScenarioWorkspaceData.cs`, `Program.cs`, `Index.cshtml`, `app.js`, `site.css` | 测试/构建 |
| Done | 界面文案、解释与业务流程重排 | 已按“异常先导”重排左侧导航和页面运行时 DOM 顺序：总览、数据准备、异常识别、场景运行、方案比较、缓冲/库存趋势、RCCP 与约束、供应商需求、场景留痕、主设置治理、白盒追踪。界面状态链和主设置类型改为中文展示，运行预览字段与左侧导航新增悬浮业务解释。 | `Index.cshtml`, `app.js`, `site.css`, `tests/Program.cs` | 测试/构建 |
| Done | 通用折叠面板与导航提示优化 | 已为多层工作台区域增加通用折叠交互，覆盖数据准备、场景运行、缓冲/库存趋势、单 SKU 仿真、RCCP、供应商需求、异常识别、场景留痕和主设置治理。左侧导航不再显示问号图标，业务解释改为标题悬浮提示；字段级问号继续保留。 | `app.js`, `site.css`, `tests/Program.cs` | 测试/构建 |
| Done | DDMRP 参数完整性 | 已将 DDMRP 参数从字符串说明升级为结构化参数档案，覆盖每个 SKU 的解耦点、缓冲配置档案、ADU 来源与窗口、DLT 来源、变异因子、DAF、区域调整因子、MOQ、订货周期、生效窗口、参数状态和红/黄/绿上沿。数据准备区新增参数完整性检查表，单 SKU 仿真工作台同步显示完整参数和 sizing 公式；前端只展示后端计算结果。 | `Models.cs`, `ScenarioWorkspaceData.cs`, `SeedData.cs`, `SeedScenarioWorkspaceDataSource.cs`, `DdmrpCalculator.cs`, `BufferTrendWorkspaceService.cs`, `Index.cshtml`, `app.js`, `tests/Program.cs` | 测试/构建 |
| Done | 工作台体验优化 | 已新增模块专注视图、可调高度表格和右侧详情抽屉。可折叠二级模块支持“专注查看”，表格容器支持纵向拖动高度，DDMRP 参数默认紧凑展示并支持查看全部/仅看缺失，DDMRP 参数与业务栅栏支持点击行打开详情抽屉。第一版不做任意拖拽排序、不做自由布局、不持久化用户布局偏好。 | `Index.cshtml`, `app.js`, `site.css`, `tests/Program.cs` | 测试/构建 |
| Done | 产品族看板 | 已新增管理层产品族聚合视图，放在总览之后、数据准备之前。看板按产品族汇总服务、流速、库存、红黄绿风险、补货、RCCP、供应缺口和预算偏差，并支持产品族卡片、产品族×周风险网格、选中产品族风险/建议动作/RCCP贡献/供应需求下钻。Scenario Preview 会返回基准/预览两套产品族看板与 comparison；前端只展示后端结果，不重新计算业务指标。 | `ProductFamilyDashboardService.cs`, `ScenarioWorkspaceData.cs`, `ScenarioRunPreviewService.cs`, `Program.cs`, `Index.cshtml`, `app.js`, `site.css`, `tests/Program.cs` | 测试/构建 |
| Done | 多方案比较与候选动作影响矩阵 | 已将原“求解器推荐”取消并改为 DDS&OP 候选动作组合选择器：服务优先、库存资金优先、产能平衡优先三类候选组合均输出 KPI、库存、服务、订单、供应缺口、峰值负荷和动作成本对比。候选动作来自网络候选场景验证结果；每个候选动作输出服务影响、库存影响、资源负荷影响、供应缺口影响、补货订单影响、估算成本、约束说明和可行性状态。求解器依据候选动作影响矩阵、冲突约束、库存预算和成本预算选择组合，最终方案仍由 Scenario Preview 二次白盒重算；候选组合卡片采用横向展开，不再纵向铺满页面。 | `ScenarioWorkspaceData.cs`, `CandidateActionCombinationService.cs`, `GurobiOptimizationSolver.cs`, `OrToolsOptimizationSolver.cs`, `Index.cshtml`, `app.js`, `tests/Program.cs` | 测试/构建 |
| Done | 专注查看与展开联动按钮规则 | 已实现体验规则：有“退出专注”按钮时不显示“收起”按钮；专注态标题不再触发展开/收起；“退出专注”直接退回到进入专注前的展开状态，此时恢复普通“收起”按钮。该修复只影响前端交互，不改业务计算。 | `app.js`, `site.css`, `tests/Program.cs` | 测试/构建 |
| Done | 专注视图右侧全宽展开 | 已将专注浮层宽度扩展到 `calc(100vw - 48px)`，候选组合卡片在专注态下按三列横向铺开，避免卡片被限制在左侧窄栏。普通页面状态仍保留横向滚动，不破坏主工作台布局。 | `site.css`, `tests/Program.cs` | 测试/构建 |
| Done | 产品族看板交互修正 | 已将产品族卡片从“全局筛选器行为”改为“看板选择器行为”：点击卡片只切换选中产品族详情，不再写入 `family-filter`，因此其它产品族卡片不会消失。看板提供“显示全部产品族”复位入口；选中产品族详情中的风险、RCCP 贡献和供应需求三栏按 SKU/周或供应商/物料族/周联动高亮。 | `app.js`, `site.css`, `tests/Program.cs` | 测试/构建 |
| Done | 主导航滚动感知 | 左侧导航已从单向点击跳转升级为双向感知：点击左侧仍滚动到右侧对应区域，右侧滚动到不同业务区时，左侧导航会通过 IntersectionObserver 自动高亮当前位置，降低长工作台迷失感。 | `app.js`, `tests/Program.cs` | 测试/构建 |
| Done | 迁移后交互信息图 | 已生成并保存 DDS&OP 与网络结构评分两个独立产品的交互信息图，明确 DDS&OP 负责场景运行、白盒回算、保存和主设置治理；网络结构评分负责 BOM 网络、指标、评分、候选动作和组合选择；Gurobi / OR-Tools 只选择候选动作组合，最终仍回到 DDS&OP 白盒引擎验证。 | `material/DDSOP与网络结构评分迁移后交互信息图.png`, `docs/network-structure-scoring-product-boundary.md` | 文件记录 |
| Done | 网络结构评分产品拆分边界 | 已建立独立核心类库、独立 Web 表现层类库和独立 Host：`AdaptiveSopDdsop.NetworkStructure` 拥有网络数据、图构建、指标、评分、组合选择器、纯网络产品数据入口合同和产品能力自描述 catalog；`AdaptiveSopDdsop.NetworkStructure.Web` 拥有网络结构评分页面、layout、partial、JS 和 CSS，独立入口使用中性“返回业务平台”文案，并在页面展示“产品能力边界”：独立能力、外部白盒依赖和边界说明；`AdaptiveSopDdsop.NetworkStructure.Host` 可脱离 DDS&OP Web 从 `/` 进入 `/network-structure` 并启动纯网络 API，且 Host 运行时边界文案使用“外部场景运行集成层 / 外部白盒引擎”等中性表达。DDS&OP Web 只在总览区保留跨产品入口卡片，入口 URL 来自 `NetworkStructure:ProductUrl`，不再把网络结构评分放入左侧主流程导航，也不再引用 `AdaptiveSopDdsop.NetworkStructure.Web` UI 包；DDS&OP 集成模块位于 `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration`，不再散落在主 `Domain` 目录，并复用同一能力 catalog 暴露 `/api/network-structure-capabilities`，保证集成边界说明与独立 Host 一致。场景验证、候选组合白盒重算、保存和审计仍属于 DDS&OP 集成层。详细边界见 `docs/network-structure-scoring-product-boundary.md`。 | `docs/network-structure-scoring-product-boundary.md`, `AdaptiveSopDdsop.NetworkStructure.csproj`, `AdaptiveSopDdsop.NetworkStructure.Web.csproj`, `AdaptiveSopDdsop.NetworkStructure.Host.csproj`, `NetworkDataSet.cs`, `SatelliteManufacturingNetworkSeedData.cs`, `NetworkGraphService.cs`, `NetworkMetricsService.cs`, `NetworkStructureScoringService.cs`, `NetworkOptimizationModels.cs`, `NetworkProductCapabilities.cs`, `NetworkStructureIntegrationContracts.cs`, `NetworkStructureIntegrationModule.cs`, `StandaloneNetworkStructureDataSource.cs` | 测试覆盖 + 独立 Host 运行检查 |
| Done | DDS&OP 白盒重算网关 | 已将网络候选验证和候选动作组合选择中的白盒回算调用收敛为 `IDdsopWhiteBoxScenarioGateway`。`NetworkScenarioValidationService` 与 `CandidateActionCombinationService` 不再直接依赖 `ScenarioRunPreviewService`；默认 `Local` 模式调用本地 Preview，`Http` 模式通过 `HttpDdsopWhiteBoxScenarioGateway` 调用配置的 DDS&OP Preview API，为未来跨进程部署预留接口。 | `NetworkStructureIntegrationContracts.cs`, `LocalDdsopWhiteBoxScenarioGateway.cs`, `HttpDdsopWhiteBoxScenarioGateway.cs`, `NetworkScenarioValidationService.cs`, `CandidateActionCombinationService.cs`, `NetworkStructureIntegrationModule.cs`, `appsettings.json`, `tests/Program.cs` | 测试覆盖 |

## 3. 已验证结果

当前在 `D:\Documents\DDAE` 下通过：

```text
58 test(s) passed.
已成功生成。
0 个警告
0 个错误
```

验证命令：

```powershell
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" build AdaptiveSopDdsop.sln
```

独立 Host 运行时验证：

- `GET http://127.0.0.1:5296/api/network-structure-capabilities` 返回 `Status=200`、`productName=网络结构评分`、`deploymentMode=独立 Host`、`capabilities.Count=6`、`externalDependencies.Count=2`，边界说明包含“本 Host 不生成执行计划”。
- `GET http://127.0.0.1:5296/api/network-structure-data?horizonWeeks=12` 返回 `Status=200`、`request.horizonWeeks=12`、`networkData.items.Count=52`。
- 响应不包含 `runtimeSignals`，证明独立 Host 通过纯网络产品数据合同返回网络主数据包，不返回 DDS&OP 运行信号。
- `GET http://127.0.0.1:5296/api/network-scenario-validation?horizonWeeks=12` 返回 `modelVersion=NetworkStandalone-NoExternalPreview`，提示“外部白盒场景重算 / 外部场景运行集成层”，不再使用 DDS&OP 专名。
- `POST http://127.0.0.1:5296/api/candidate-action-combinations/select` 返回中性边界说明：候选动作组合必须回到“外部白盒引擎”重算后才能比较，trace 说明 Host 只负责网络图、指标和候选证据，不生成外部执行计划。

浏览器验证已确认：

- `/` 首屏已经替换为 Scenario Run Workspace，不再展示教学 hero。
- `workspace-kpis` 加载 7 个 KPI。
- `scenario-template-list` 加载 4 个场景模板。
- `scenario-comparison` 展示 Baseline / Scenario 双列比较。
- `product-family-dashboard-panel` 展示产品族 KPI、产品族卡片、周度风险网格和选中产品族下钻。
- `candidate-action-combination-panel` 位于网络结构评分区，支持 Gurobi / OR-Tools 在候选动作影响矩阵上选择组合，普通状态横向滚动，专注状态向右侧全宽三列展开。
- `network-metrics-body` 展示 Phase 4 网络指标：下游覆盖度、数量影响度、累计提前期、供应风险、资源约束和库存代价，并可下钻证据链。
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
| Done | 候选组合 Solver Adapter 实现 | 已支持 Gurobi / OR-Tools 可选选择候选动作组合；OR-Tools 第一版为 Adapter 内置 0/1 组合求解实现，后续可替换为真实 CP-SAT。 | `IOptimizationSolver`、`GurobiOptimizationSolver`、`OrToolsOptimizationSolver`、`CandidateActionCombinationService` | 测试/构建 |
| Done | 产品级 RCCP | 已完成资源汇总、周度热力格、选中资源负载图、约束/非约束对比和规则型瓶颈动作建议第一版。 | RCCP 专项工作台 | 服务/API/UI 测试 |

## 6. 架构与界面约束

### 6.1 优化引擎 Solver Adapter

当前 `DemandDrivenPlanningEngine` 定位为白盒推演 / 仿真引擎，用于回答“某个场景会发生什么”。优化能力已通过 Solver Adapter 接入，不允许把具体求解器直接写死在业务服务或 UI 逻辑中。

推荐架构：

```text
Scenario Preview / Network Scenario Validation
-> explains what each candidate action does

CandidateActionCombinationSelector
-> IOptimizationSolver
-> GurobiOptimizationSolver
-> OrToolsOptimizationSolver

DDS&OP CandidateActionCombinationService
-> Scenario Preview white-box recalculation
```

第一版默认选择：

- 使用 Gurobi 作为默认优化求解器，同时允许用户在界面选择 OR-Tools。
- OR-Tools Adapter 第一版使用同一 0/1 候选组合约束模型的内置枚举求解实现，不新增外部 NuGet 依赖；后续可替换为 `Google.OrTools` CP-SAT。
- 已实现“选择 3 类候选动作组合”，而不是不可解释的黑盒全自动最优解。
- 候选组合覆盖：服务优先、库存资金优先、产能平衡优先。
- 第一版优化模型使用 binary decision variable 选择候选动作组合；复杂 DDMRP 缓冲、RCCP、供应和约束结果仍由 Scenario Preview 白盒重算。
- `POST /api/candidate-action-combinations/select` 接收网络核心库定义的 `CandidateActionCombinationRequest` 和 `solverName`，返回候选动作组合、白盒重算 request、重新计算后的 preview result、候选动作影响矩阵和组合选择 trace；组合不自动采纳、不保存、不审批。纯组合选择由网络结构类库的 `CandidateActionCombinationSelector` 完成，DDS&OP `CandidateActionCombinationService` 不直接依赖 `IOptimizationSolver`。

Solver Adapter 的目的：

- 保持优化引擎可替换。
- 虽然 Gurobi 是首选，仍避免业务服务被单一许可、部署方式或求解器 API 绑定。
- 让场景推演与组合选择分层：推演负责解释结果，优化器只负责在候选动作很多且受预算、能力、服务目标约束时选择值得比较的动作组合。

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
- 场景对比：采用 Baseline / Candidate 两列比较、候选组合高亮、关键指标网格和决策动作区。
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
| `GET /api/network-structure-data` | Done | 独立 Host 中由 `INetworkStructureProductDataSource` 返回纯网络产品数据包 `request + networkData`，不包含 DDS&OP 运行信号；DDS&OP 集成模块中同名端点由 `INetworkStructureDataSource` 返回迁移期适配数据包 `networkData + runtimeSignals`，用于白盒验证和候选组合集成 |
| `GET /api/network-structure-capabilities` | Done | 网络结构评分产品能力自描述入口；独立 Host 与 DDS&OP 集成模块均复用核心能力 catalog 返回同一份产品能力、外部依赖和边界说明；网络结构评分页面已展示“产品能力边界” |
| `GET /api/network-structure-scoring` | Done | 网络结构白盒评分、控制点 / 缓冲点候选发现 |
| `GET /api/exception-workspace` | Done | 异常 SKU 驱动场景入口 |
| `GET /api/buffer-trend-workspace` | Done | 缓冲 / 库存趋势与单 SKU 仿真钻取 |
| `GET /api/rccp-workspace` | Done | 产品级 RCCP 工作台 |
| `GET /api/constraint-workspace` | Done | 受限 / 不受限约束视图 |
| `GET /api/supplier-collaboration-workspace` | Done | 供应商需求钻取 |
| `POST /api/scenario-runs/preview` | Done | 非持久化场景预览与白盒 trace |
| `GET /api/network-metrics` | Done | Phase 4 网络指标计算：下游覆盖度、数量影响度、累计提前期、供应风险、资源约束、库存代价和证据链 |
| `POST /api/candidate-action-combinations/select` | Done | Phase 8 Gurobi / OR-Tools 可选候选动作组合选择、多方案比较、候选动作影响矩阵和二次白盒重算 |
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
| `AdaptiveSopDdsop.NetworkStructure` | Done | 网络结构评分独立核心类库；已承载纯网络主数据契约、卫星制造多层 BOM seed 工厂、产品能力 catalog、物料图结果模型、网络指标结果模型、网络评分结果模型、纯求解器适配模型、`NetworkGraphService`、`NetworkMetricsService`、`NetworkStructureScoringService`、Gurobi / OR-Tools solver adapter 和候选组合选择器 |
| `NetworkStructureDataRequest` / `INetworkStructureDataSource` / `NetworkStructureDataSourceAdapter` | Done | DDS&OP 集成迁移边界；当前集成数据接口使用独立 `NetworkStructureDataRequest`，由显式适配器包装 DDS&OP `IScenarioWorkspaceDataSource`，把 `ScenarioWorkspaceDataSet` 的运行信号和 SKU seed 输入映射为 `NetworkStructureDataSet`；`ScenarioWorkspaceDataSet` 本身不再携带 `NetworkDataSet`，seed source 不再直接实现网络产品接口，DI 不再强转。独立 Host 不使用该接口，而使用网络核心的 `INetworkStructureProductDataSource` |
| `NetworkGraphService` | Done | 已迁入 `AdaptiveSopDdsop.NetworkStructure`，基于 `INetworkGraphDataSource` 构建物料上游 / 下游影响图，不直接依赖 DDS&OP 工作台数据源或请求模型 |
| `DdsopNetworkGraphDataSource` | Done | DDS&OP 集成适配器：把工作台网络数据映射为独立图构建所需 `NetworkGraphDataSet` |
| `NetworkMetricsService` | Done | 已迁入 `AdaptiveSopDdsop.NetworkStructure`，基于 `INetworkMetricsDataSource` 和网络主数据计算下游覆盖度、数量影响度、累计提前期、供应风险、资源约束、库存代价和证据链；不直接调用 DDS&OP 计划引擎 |
| `DdsopNetworkMetricsDataSource` | Done | DDS&OP 集成适配器：调用 DDMRP / RCCP / 供应约束链路，压缩为独立指标服务需要的 ADU、资源负荷和供应风险运行信号 |
| `NetworkStructureScoringService` | Done | 已迁入 `AdaptiveSopDdsop.NetworkStructure`，基于 `INetworkScoringDataSource` 消费评分专用信号和 Phase 4 网络指标，生成控制点 / 库存缓冲 / 时间缓冲 / 能力缓冲候选；不直接调用 DDS&OP 计划引擎，候选解释使用“外部场景运行系统 / 管理评审”等中性产品语言 |
| `DdsopNetworkScoringDataSource` | Done | DDS&OP 集成适配器：调用 DDMRP / RCCP / 供应约束链路，压缩为独立评分服务需要的 SKU、需求、红区周、缓冲上沿、资源负荷、供应需求和供应风险运行信号 |
| `NetworkCandidateRecalculationRequestBuilder` | Done | DDS&OP 集成层：把网络候选动作转换为非持久化 `ScenarioRunPreviewRequest`，把候选翻译逻辑从验证服务中拆出 |
| `NetworkScenarioValidationService` | Done | DDS&OP 集成层：调用 `NetworkCandidateRecalculationRequestBuilder` 生成请求，通过 `IDdsopWhiteBoxScenarioGateway` 回到 DDS&OP 白盒引擎重算并汇总验证结果；不直接依赖具体 `ScenarioRunPreviewService` |
| `NetworkStructureIntegrationModule` | Done | DDS&OP Web 集成模块：集中注册网络结构评分相关 DI，并集中映射网络评分、图、指标、场景验证和候选组合 API；`Program.cs` 只负责挂载模块 |
| `BufferTrendWorkspaceService` | Done | 缓冲趋势、库存金额、单 SKU 仿真明细 |
| `RccpWorkspaceService` | Done | 资源汇总、周度热力格、SKU 贡献、动作建议 |
| `ConstraintWorkspaceService` | Done | 资源和供应侧受限 / 不受限缺口 |
| `SupplierCollaborationWorkspaceService` | Done | 供应商优先需求钻取 |
| `ExceptionWorkspaceService` | Done | 异常 SKU 聚合、信号、预设模板 |
| `ScenarioRunPersistenceService` | Done | 场景保存、详情、append-only 审计链 |
| `MasterSettingsGovernanceService` | Done | 主设置建议、保存、审计和状态流转 |
| `CandidateActionCombinationSelector` | Done | 网络结构评分核心层：基于候选动作影响矩阵、预算和冲突约束调用 Gurobi / OR-Tools 选择动作组合，不认识 Scenario Preview |
| `CandidateActionCombinationService` | Done | DDS&OP 集成层：构造候选动作影响矩阵，委托 `CandidateActionCombinationSelector` 选择组合，并通过 `IDdsopWhiteBoxScenarioGateway` 触发二次白盒重算 |
| `IOptimizationSolver` / `GurobiOptimizationSolver` | Done | 已迁入 `AdaptiveSopDdsop.NetworkStructure`；Gurobi Adapter 作为首选求解器 |
| `OrToolsOptimizationSolver` | Done | 已迁入 `AdaptiveSopDdsop.NetworkStructure`；第一版为内置 0/1 候选组合求解，后续可替换真实 CP-SAT |

### 9.3 已实现并已记录的 UI 入口

| UI 区域 / DOM | 状态 | 说明 |
|---|---|---|
| `product-family-dashboard-panel` | Done | 产品族 KPI、卡片、周度风险网格、选中产品族详情 |
| `network-structure-entry-card` | Done | DDS&OP 总览区的跨产品入口卡片；完整网络结构评分工作台位于独立 `/network-structure` 页面 |
| `scenario-run-panel` | Done | 场景配置、运行预览、保存场景；不再放置旧求解器推荐入口 |
| `network-metrics-body` / `network-metric-evidence-list` | Done | 网络指标计算表和证据链 |
| `candidate-combination-solver-select` | Done | Gurobi / OR-Tools 可选 |
| `candidate-combination-list` | Done | 普通状态横向滚动；专注态右侧全宽三列展开 |
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
| 可拖拽 BOM 图形画布 | Deferred | 当前 Phase 3/4 采用表格和路径链表达网络图、指标和证据，后续再做可交互图形画布 |

### 9.6 网络结构评分当前边界

`NetworkStructureScoringService` 是白盒候选发现工具，不是优化引擎，也不生成并行计划。当前版本已消费 `NetworkMetricsService` 的 Phase 4 网络指标，不在评分服务内部重复计算网络代理指标。

产品拆分边界：

- 网络结构评分正在按独立产品方向迁移，目标是以后可以从 DDS&OP 工作台中拆出单独部署。
- 新增 `AdaptiveSopDdsop.NetworkStructure` 独立类库，已承载纯网络主数据契约：物料、BOM、替代料、资源路线、供应来源、库存位置、缓冲设置、提前期档案和 `NetworkDataSet`。
- 卫星制造多层 BOM seed 工厂已迁入 `AdaptiveSopDdsop.NetworkStructure/SatelliteManufacturingNetworkSeedData.cs`；DDS&OP `SeedData` 不再直接构造网络物料、BOM、供应来源、routing 或 `NetworkDataSet`，当前成品 SKU 种子输入只在 `NetworkStructureDataSourceAdapter` 边界传入网络类库。
- `NetworkGraphService` 与物料图结果模型已迁入 `AdaptiveSopDdsop.NetworkStructure`；Web 侧通过 `DdsopNetworkGraphDataSource` 提供图构建数据。
- `NetworkMetricsService` 与网络指标结果模型已迁入 `AdaptiveSopDdsop.NetworkStructure`；Web 侧通过 `DdsopNetworkMetricsDataSource` 提供 ADU、资源负荷和供应风险运行信号。
- Web 域模型不再拥有这些纯网络主数据 DTO，而是通过项目引用消费网络结构评分核心契约。
- 新增两层数据入口：`AdaptiveSopDdsop.NetworkStructure` 拥有纯网络产品入口 `INetworkStructureProductDataSource`，只返回 `NetworkStructureProductDataSet(request + networkData)`；DDS&OP 集成层保留 `INetworkStructureDataSource`，负责把 DDS&OP 场景工作台数据和运行信号适配给网络图、指标、评分和组合验证。
- 新增独立产品能力合同与 catalog：`NetworkStructureProductCapabilities`、`NetworkStructureCapability`、`NetworkStructureExternalDependency` 和 `NetworkStructureProductCapabilityCatalog` 位于 `AdaptiveSopDdsop.NetworkStructure`。独立 Host 与 DDS&OP 集成模块的 `/api/network-structure-capabilities` 都复用该 catalog；`/network-structure` 页面已经展示“产品能力边界”，避免调用方通过 DDS&OP 语境猜测网络产品能力边界。
- 新增 `NetworkStructureDataSet`，只承载网络结构评分需要的两类输入：网络主数据快照和运行时信号。
- `NetworkGraphService`、`NetworkMetricsService`、`NetworkStructureScoringService` 已分别切换为 `INetworkGraphDataSource`、`INetworkMetricsDataSource`、`INetworkScoringDataSource`，不再直接依赖 `IScenarioWorkspaceDataSource`。
- 当前 DDS&OP 仍通过 `NetworkStructureDataSourceAdapter` 提供适配数据；这属于迁移期集成层，不是网络结构评分核心逻辑。适配器显式依赖 `IScenarioWorkspaceDataSource`，并负责在产品边界构造网络 seed 输入和运行时信号，避免要求 seed data 或未来生产 DDS&OP 数据源直接实现 DDS&OP-to-network 适配接口，也避免 DDS&OP 场景数据包携带网络主数据快照。独立 Host 则通过 `StandaloneNetworkStructureDataSource : INetworkStructureProductDataSource` 直接提供纯网络主数据包。
- `NetworkCandidateRecalculationRequestBuilder` 与 `NetworkScenarioValidationService` 保留 DDS&OP 集成职责，因为前者负责把网络候选翻译为 Scenario Preview 请求，后者负责通过 `IDdsopWhiteBoxScenarioGateway` 送回白盒重算；二者都位于 `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration`，属于产品间集成层，不属于网络结构评分核心，也不属于 DDS&OP 主领域模型。
- `/network-structure` 独立入口已由 `AdaptiveSopDdsop.NetworkStructure.Web` 提供，使用 `_NetworkStructureLayout.cshtml`，不再通过 DDS&OP 默认 `_Layout` 加载 `site.css`；`network-structure-workspace.css` 已承载独立入口需要的基础按钮、状态、表格、KPI、面板和网络图样式，并通过 `_content/AdaptiveSopDdsop.NetworkStructure.Web/...` 加载；页面返回入口采用中性“返回业务平台”，不再显示“返回 DDS&OP”。DDS&OP 首页只在总览区保留 `network-structure-entry-card`，入口 URL 来自 `NetworkStructure:ProductUrl`，左侧导航不再显示网络结构评分流程项，DDS&OP Web 项目不再引用网络结构评分 UI 包。
- 网络结构前端模块通过 `window.NetworkStructureProductHost` 访问宿主状态、格式化、错误显示和多方案比较刷新；当前只由 `/network-structure` 独立页注册该宿主契约，DDS&OP 首页不再注册网络宿主，`network-structure-workspace.js` 不再直接依赖 DDS&OP 主 `app.js` 的词法变量；前端全局对象已移除 `Ddae*` 品牌前缀，改为中性 `NetworkStructureProduct*`。
- `/network-structure` 独立页通过 `window.NetworkStructureProductWorkspace` 的公开模块 API 渲染评分、指标、场景验证并加载物料图，不再直接调用网络模块内部的全局渲染函数；DDS&OP 首页只保留独立产品入口。
- 网络结构评分模块通过 `NetworkStructureProductWorkspace.loadData(...)` 自己编排网络主数据、评分、指标、物料图和场景验证 API；DDS&OP 首页不再硬编码或调用网络 API，独立网络页只传入 horizon 和是否需要加载窄网络数据包。
- `/network-structure` 独立页已拥有网络专属折叠 / 专注查看交互和 `network-workspace-focus-layer`，不再依赖 DDS&OP `app.js` 提供二级模块工作台行为；嵌入 DDS&OP 时网络模块会检测 `data-collapse-panel`，避免与主工作台重复增强。
- 网络结构评分 partial 与 `network-structure-workspace.css` 已迁入 `AdaptiveSopDdsop.NetworkStructure.Web`，并清理 DDS&OP/RCCP/产品族/缓冲语义类名，网络工作区自己的 KPI、摘要、筛选、详情和动作布局统一使用 `network-*` 类名，避免独立产品迁移时残留 `rccp-*`、`product-family-*`、`buffer-*` 等业务耦合。
- `AdaptiveSopDdsop.NetworkStructure` 核心类库不再在评分范围、候选解释和不采纳风险中硬编码 DDS&OP 产品名；核心输出改用外部场景运行系统、管理评审等中性语言。DDS&OP 字样只允许出现在 DDS&OP 集成层、边界说明和 DDS&OP 产品自身。
- 独立产品边界文档记录在 `docs/network-structure-scoring-product-boundary.md`，后续拆分项目或 NuGet 包时以该文件为迁移准绳。

当前输入：

- SKU 主数据与 DDMRP 参数：ADU、DLT、变异因子、MOQ、订货周期、单位成本、红黄绿缓冲区。
- 库存位置：现有库存、开放供应、合格需求。
- 未来需求与补货投影：通过 `DemandDrivenPlanningEngine.ProjectBuffers` 生成。
- Phase 4 网络指标：下游覆盖度、数量影响度、累计提前期、供应风险、资源约束、库存代价及其证据链。
- Routing：预计补货订单乘以 `ResourceRouting.CapacityPerUnit` 后形成资源约束影响；网络指标中的资源约束证据追溯到 `NetworkRoutingLine`。
- 供应来源与供应能力窗口：供应商、物料族、承诺能力、风险状态和供应缺口。
- 产品族目标：服务水平与目标流速。

当前输出：

- `库存缓冲` 候选：适合重审库存缓冲、MOQ、订货周期和红黄绿区的 SKU。
- `时间缓冲` 候选：长交期、高供应风险或高单位成本的 SKU / 供应物料族。
- `能力缓冲` 候选：由 routing 折算后出现峰值负荷或多 SKU 共用的资源。
- `解耦点` 候选：以产品族为粒度识别可能需要控制点 / 半成品解耦点治理的位置。
- 白盒解释：每个候选点给出下游覆盖度、数量影响度、累计提前期、供应风险、资源约束、库存代价、服务影响分数和证据链。

后续升级：

- 后续可增加更复杂的图形画布、替代料路径选择策略和关键路径可视化。
- 评分结果进入候选动作库后，再由 Gurobi / OR-Tools 做组合选择；优化器仍只选候选动作组合，最终必须回到 DDS&OP / DDMRP 白盒引擎重算验证。

### 9.7 网络结构评分 V2 数据结构与开发路径

详细规格文档：

- `docs/网络结构评分V2数据结构与开发过程.docx`

开发原则：

- 数据结构先行，算法其次，优化器最后。
- 解耦点在本系统中定义为“物料节点”。
- 库存位置不决定解耦点，但决定该解耦点是否可执行：能否存放、由谁负责、是否可共享、质量状态和有效期如何控制。
- DDAE 读取 PLM 发布的 BOM 快照，不替代 PLM 管理 BOM 版本、生效日期和替代料。
- DDAE 负责网络评分、场景验证、候选动作组合选择和主设置治理；DDOM / SDBR 负责详细资源日历、日能力、排程、工单释放和现场反馈。

V2 最小网络数据结构：

| 对象 | 作用 |
|---|---|
| `ItemMaster` | 定义成品、半成品、采购件和原材料物料节点 |
| `BomHeader` / `BomLine` | 承接 PLM BOM 版本、生效期、父子物料、用量和替代组 |
| `AlternateItem` | 表达替代料路径、优先级和替代比例 |
| `RoutingLine` | 按物料、型号/产品族、版本和生效期定义资源消耗 |
| `SupplierSource` | 表达多供应商、主供/备供、分配比例、固定 lead time、波动率和周能力 |
| `InventoryLocation` | 表达物料库存承载位置、质量状态、责任组织、有效期和可共享性 |
| `BufferSetting` | 标记物料是否为解耦点，并保存库存缓冲、时间缓冲、MOQ 和订货周期 |
| `LeadTimeProfile` | 保存标准 lead time 与波动率，用于时间缓冲评分 |
| `NetworkScoreResult` | 保存候选评分、建议类型、证据、解释和后续动作 |

Phase 1 数据模型状态：

| 项目 | 状态 | 说明 |
|---|---|---|
| `NetworkItemMaster` | Done | 已覆盖成品、半成品、采购件、原材料物料节点 |
| `NetworkBomHeader` / `NetworkBomLine` | Done | 已支持 BOM 版本、生效日期、发布状态、父子物料、用量、损耗和替代组 |
| `NetworkAlternateItem` | Done | 已支持替代组、主料、替代料、优先级、替代比例和合格状态 |
| `NetworkRoutingLine` | Done | 已支持物料、型号 / 产品族、routing 版本、工序、资源和单位能力消耗 |
| `NetworkSupplierSource` | Done | 已支持多供应商、主供 / 备供、固定 lead time、波动率、周能力和 MOQ |
| `NetworkInventoryLocation` | Done | 已支持半成品库、待检区、合格库、线边库、质量状态、owner、有效期和可共享性 |
| `NetworkBufferSetting` | Done | 已支持物料解耦点、库存缓冲、时间缓冲、MOQ、订货周期、生效期和状态 |
| `NetworkLeadTimeProfile` | Done | 已支持供应 lead time 与控制点前时间缓冲 profile |
| `NetworkDataSet` | Done | 已由 `AdaptiveSopDdsop.NetworkStructure` 独立类库拥有，并通过 `NetworkStructureDataSourceAdapter` 生成 `NetworkStructureDataSet`；不再挂接到 `ValidationData` 或 `ScenarioWorkspaceDataSet`，也不再通过 `/api/scenario-workspace-data` 返回 |

V2 开发过程：

1. 数据模型：新增网络数据 DTO 和 data source adapter。状态：Done。
2. Seed 数据：构建卫星制造多层 BOM、替代料、多供应商、型号化 routing 和库存位置。状态：Done。
3. 图构建服务：建立正向 / 反向邻接表，校验循环、孤儿节点、版本生效和缺失主数据。状态：Done。
4. 网络指标计算：计算下游覆盖度、数量影响度、累计提前期、供应风险、资源约束影响和库存代价，并保证每个指标追溯到 BOM line、supplier source、routing line、lead time profile、buffer setting 或 inventory location。状态：Done。
5. V2 评分服务：消费 Phase 4 网络指标，生成解耦点、库存缓冲、时间缓冲、能力缓冲和只监控候选。状态：Done。
6. 场景验证：把候选作为拟议主设置运行 DDS&OP / DDMRP 白盒推演。状态：Done。
7. 主设置治理：通过评审后保存为主设置变更请求，批准后进入下一轮 plan run。状态：Pending。
8. 优化器组合选择：当候选动作很多且存在预算、能力、服务目标等组合约束时，再由 Gurobi / OR-Tools 选择动作组合；最终仍必须回到白盒引擎重算。状态：Done。

Phase 2 Seed 数据状态：

| 项目 | 状态 | 说明 |
|---|---|---|
| 卫星平台多层 BOM | Done | 已构造平台核心舱、电源、姿控、星载电子和电缆束的多层父子关系 |
| 有效载荷多层 BOM | Done | 已构造光学载荷、SAR 射频前端、探测器、镜头组、T/R 组件和波导 |
| 星载电子多层 BOM | Done | 已构造星载电子半成品、计算模块、射频模块、FPGA、CPU 和抗辐照存储 |
| 热控结构多层 BOM | Done | 已构造 MLI 包覆、热控散热板、热管、蜂窝板、导热胶和展开机构 |
| 电缆束多层 BOM | Done | 已构造测试后电缆束、线束组件、线缆、连接器和屏蔽编织层 |
| 物料复用与用量差异 | Done | 空间级 FPGA、连接器、电缆等关键物料已被多个产品族复用，并体现不同 `QuantityPer` |
| 替代料 | Done | FPGA、连接器、探测器、热控材料已表达主料、备料、优先级、替代比例和合格状态 |
| 多供应商 | Done | FPGA、OBC、探测器、连接器等关键件已表达主供 / 备供、分配比例、固定 lead time、波动率、周能力和 MOQ |
| 型号化 Routing | Done | 成品 SKU 和关键半成品已按型号 / 产品族表达不同资源消耗 |
| 库存位置可执行性 | Done | 已表达半成品超市、来料待检、合格库、线边库和受控材料库；库存位置只判断解耦点是否可执行 |
| 物料解耦点与时间缓冲 | Done | 缓冲设置全部指向物料节点，不指向资源、工序或库存位置；时间缓冲通过 lead time profile 表达 |

Phase 2 仍不实现 BOM 图遍历、累计提前期算法、网络评分算法、UI 网络图或优化器。当前 V2 网络数据仍作为并行 seed 数据层存在，不替代现有 DDMRP / DDS&OP 白盒推演输入。

Phase 3 图构建服务状态：

| 项目 | 状态 | 说明 |
|---|---|---|
| `NetworkGraphService` | Done | 已基于 `NetworkDataSet` 构造已发布 BOM 快照的正向 / 反向物料图 |
| `/api/network-graph` | Done | 支持 `itemCode` 与 `maxDepth`，默认展开 `PART-FPGA-SPACE`，返回上游、下游、边列表和校验报告 |
| 上游影响范围 | Done | 可按物料展开其组件、子组件和采购件，并返回路径深度与累计用量 |
| 下游影响范围 | Done | 可按物料展开受影响父项、半成品和成品 SKU，并返回路径深度与累计用量 |
| 路径累计用量 | Done | 按 `QuantityPer × (1 + ScrapFactor)` 逐层累乘 |
| 校验报告 | Done | 已输出 Red / Yellow / Info issue，包括缺失物料、循环 BOM、缺供应、缺 routing、解耦点无可执行库存位置、替代料缺失等 |
| UI 展示 | Done | 在网络结构评分区新增“物料网络展开”，以表格 / 路径链展示上游影响、下游影响、路径明细和校验报告 |
| 第一版物料关系图 | Done | 已新增受控局部关系图，支持物料选择、上游 / 下游 / 双向、最大层级和只看风险节点；点击图节点会同步选中物料、网络指标和证据链 |
| 中文业务名称展示 | Done | 图、指标和校验报告界面不直接暴露 `SupplierSource`、`LeadTimeProfile`、`BufferSetting`、`Routing`、`Subassembly` 等内部枚举，而映射为供应来源、提前期档案、缓冲设置、资源路线、半成品等中文业务名称 |

Phase 3 仍不实现网络评分算法、累计提前期综合评分、优化器、数据库持久化或全量自由拖拽图形画布。图形第一版只做受控局部网络解释，避免在大规模 BOM 下默认展开造成信息混乱；资源、供应商和库存位置在本阶段作为物料节点属性和校验证据，不作为解耦点。

Phase 4 网络指标计算状态：

| 项目 | 状态 | 说明 |
|---|---|---|
| `NetworkMetricsService` | Done | 已基于 Phase 3 物料图与统一数据源计算物料级网络指标 |
| `/api/network-metrics` | Done | 返回 `NetworkMetricsWorkspaceResult`，默认 12 周计划范围 |
| 下游覆盖度 | Done | 基于下游父项、成品 SKU、产品族数量和路径数量计算，并追溯到 `BomLine` |
| 数量影响度 | Done | 按 BOM 路径累计 `QuantityPer × (1 + ScrapFactor)`，结合下游 SKU ADU / 需求规模，并追溯到 `BomLine` |
| 累计提前期 | Done | 汇总供应 lead time、lead time profile 和 time buffer，并追溯到 `SupplierSource`、`LeadTimeProfile` 和 `BufferSetting` |
| 供应风险 | Done | 基于供应资格、lead time 波动率、周能力、供应缺口和供应来源数量，并追溯到 `SupplierSource` |
| 资源约束 | Done | 基于 network routing 与 RCCP 周度负荷，并追溯到 `RoutingLine` |
| 库存代价 | Done | 基于单位成本、MOQ、缓冲设置、库存位置和累计数量影响，并追溯到 `BufferSetting` 或 `InventoryLocation` |
| UI 展示 | Done | 网络结构评分区新增“网络指标计算”，支持物料表格、指标解释和证据链下钻 |
| 指标类型中文化 | Done | 网络指标表中的物料类型、证据类型和后端证据说明统一通过 UI 映射为中文业务名称，保留内部英文枚举作为数据契约但不面向计划员直接展示 |

Phase 4 仍不写数据库、不调用优化器。图形展示只作为网络关系定位入口，指标计算仍以表格、解释和证据链为审计主视图；它只把网络结构转成可解释指标，供 Phase 5 评分和 Phase 8 候选动作组合选择使用。

Phase 5 V2 评分服务状态：

| 项目 | 状态 | 说明 |
|---|---|---|
| `NetworkScore-V2` | Done | 已从代理评分升级为消费 Phase 4 网络指标的白盒评分 |
| 物料节点候选 | Done | 已基于 BOM 上游 / 下游展开生成 `NET-*` 物料节点候选 |
| 推荐类型 | Done | 已输出解耦点、库存缓冲、时间缓冲、能力缓冲、只监控等推荐类型 |
| 得分 | Done | 已综合下游覆盖度、数量影响度、累计提前期、供应风险、资源约束、服务影响和库存成本惩罚 |
| 证据 | Done | 已输出 BOM line、供应来源、routing 行、lead time profile、缓冲设置、库存位置等证据 |
| 解释 | Done | 已输出候选为何成立的白盒 `Rationale` |
| 不采纳风险 | Done | 已为每个候选输出 `NotAdoptingRisk`，说明不治理该候选可能导致的服务、供应、资源或治理风险 |
| UI 展示 | Done | 网络结构评分详情区已展示建议动作与不采纳风险 |

Phase 5 仍不自动采纳候选、不写回 DDOM、不调用 Gurobi / OR-Tools。优化器后续只能在候选动作组合选择层使用，最终仍必须回到 DDS&OP / DDMRP 白盒引擎重算。

Phase 6 场景验证状态：

| 项目 | 状态 | 说明 |
|---|---|---|
| `NetworkCandidateRecalculationRequestBuilder` | Done | 已把 V2 网络候选转换为非持久化 `ScenarioRunPreviewRequest`，并独立承载候选目标、下游 SKU、供应来源和资源的解析逻辑 |
| `NetworkScenarioValidationService` | Done | 已调用候选回算请求构建器，并通过现有 DDS&OP / DDMRP 白盒引擎重算 |
| `/api/network-scenario-validation` | Done | 已返回候选级场景验证结果，默认按 12 周计划范围输出 |
| 库存金额变化 | Done | 输出候选方案相对基准方案的 `AverageInventoryValueDelta` |
| 红区周变化 | Done | 输出缓冲趋势中的 `RedWeekDelta` |
| 补货订单变化 | Done | 输出 `ReplenishmentOrderCountDelta` 与 `ReplenishmentQuantityDelta` |
| RCCP 变化 | Done | 输出峰值负荷、平均负荷和 RCCP 红区周变化 |
| 供应缺口变化 | Done | 输出 `SupplyGapDelta` |
| 验证证据 | Done | 输出候选对应的场景请求摘要、关键 KPI 变化和验证结论 |
| UI 展示 | Done | 网络结构评分区新增“场景验证”表格，显示库存、红区周、补货、RCCP 和供应缺口变化 |

Phase 6 仍不自动采纳候选、不保存验证结果、不写回 DDOM、不调用优化器。它的职责是把网络评分候选送回同一套白盒推演链路，帮助计划员判断“这个候选如果作为临时主设置参数运行，会对库存、服务风险、资源负荷和供应缺口产生什么影响”。

Phase 8 优化器组合选择状态：

| 项目 | 状态 | 说明 |
|---|---|---|
| 原 Gurobi 求解器推荐入口 | Done | 已取消面向用户的“优化推荐 / 生成优化推荐”表达，避免让用户误以为 Gurobi 直接生成 DDMRP 计划 |
| 候选动作组合选择器 | Done | 界面改为“候选动作组合选择”，定位为在候选动作很多时辅助选择动作组合 |
| Gurobi / OR-Tools 可选 | Done | 保留 `candidate-combination-solver-select`，用户可选择 Gurobi 或 OR-Tools |
| 候选动作影响矩阵 | Done | 组合选择基于网络候选场景验证后的服务、库存、RCCP、供应缺口、补货订单和成本影响 |
| 组合约束 | Done | 求解器约束包括最大动作数、冲突键、库存预算和动作成本预算 |
| 三类管理目标 | Done | 输出服务优先、库存资金优先、产能平衡优先三类候选组合 |
| 二次白盒重算 | Done | 求解器选中动作组合后，必须合并为 `ScenarioRunPreviewRequest` 并调用 Scenario Preview 重新计算 |
| 不自动采纳 | Done | 候选组合不保存、不审批、不写回 DDOM；界面显示白盒重算结果和组合对比，不再提供旧“带入场景”推荐链路 |

Phase 8 的边界：Gurobi / OR-Tools 不是 DDMRP 计算引擎，也不替代网络评分、缓冲投影、RCCP 或供应约束计算。它只在“已经存在候选动作及影响矩阵”的前提下做 0/1 组合选择；所有业务结果以白盒引擎二次重算为准。

### 9.8 当前对账结论

当前代码中的主要 API、领域服务、UI 入口、求解器选择、持久化与审计链均已在本 spec 中记录。网络结构评分产品拆分已补充独立边界文档、窄数据源接口和 DDS&OP 适配器记录；图构建、网络指标和网络评分核心服务已切换到网络专属数据源接口，且网络核心类库不再硬编码 DDS&OP 产品名。`/network-structure` 独立入口已改用 `/api/network-structure-data`，不再读取 DDS&OP 全量工作台数据包；独立页面返回按钮使用“返回业务平台”，不再显示为 DDS&OP 子页面；`AdaptiveSopDdsop.NetworkStructure.Host` 根路径 `/` 会进入 `/network-structure`，且 `/api/network-structure-data` 已通过核心类库的 `INetworkStructureProductDataSource` 返回纯网络产品数据包，不再手工拼匿名响应；Host 程序的边界响应已改为外部系统中性文案，不再出现 DDS&OP / Ddsop 专名。`ScenarioWorkspaceDataSet` 与 `ValidationData` 不再携带 `NetworkDataSet`，DDS&OP `SeedData` 不再引用网络产品命名空间，网络主数据快照只在 `NetworkStructureDataSourceAdapter` 边界生成；网络 Razor、CSS、JS 已从 DDS&OP Web 项目迁入 `AdaptiveSopDdsop.NetworkStructure.Web`，DDS&OP Web 项目不再引用该 UI 包，DDS&OP 首页和全局布局不加载网络 CSS/JS；DDS&OP 首页只保留独立产品入口卡片，入口 URL 由 `NetworkStructure:ProductUrl` 配置，不注册网络宿主、不调用网络 API；Web 项目不再通过 global using 暴露网络结构命名空间。DDS&OP 网络集成文件已集中到 `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration`，使用 `AdaptiveSopDdsop.Web.NetworkStructureIntegration` 命名空间，主 `Domain` 目录不再承载网络结构评分集成合同、数据源适配器、白盒网关、验证服务或候选组合服务。当前已新增 `AdaptiveSopDdsop.NetworkStructure.Host` 独立部署外壳，可脱离 DDS&OP Web 启动网络产品页面和纯网络 API；DDS&OP 白盒验证已收敛为 `IDdsopWhiteBoxScenarioGateway` 显式跨产品接口，并已提供 `Local` / `Http` 两种可配置实现。场景验证与候选组合采纳仍保留为 DDS&OP 集成边界，后续生产化重点是替换独立 Host 的生产网络数据源，并将 `HttpDdsopWhiteBoxScenarioGateway` 连接到真实 DDS&OP 服务。`docs/development_principles.md` 已同步当前数据边界：主数据仍是 seed data，场景运行记录和主设置治理记录已使用 SQLite 本地持久化。当前无发现“已实现但未记录”的核心业务能力；后续新增能力必须继续同时更新代码、测试和 spec 状态。

### 2026-06-29 候选动作组合选择边界调整

用户确认后，网络结构评分工作台不再暴露“候选动作组合选择”操作入口。当前产品主线收敛为：物料网络展开、网络指标计算、网络结构评分、候选证据链、场景验证边界说明。

边界调整：

- 网络结构评分负责发现候选点、解释证据链和说明不采纳风险。
- 网络结构评分不直接选择动作组合、不生成计划、不保存、不审批、不下传 SDBR。
- Gurobi / OR-Tools 不作为网络结构评分当前界面功能暴露。
- 未来当 DDS&OP 多方案比较阶段出现大量候选动作时，求解器可以作为“候选动作组合筛选器”恢复使用。
- 求解器筛出的组合仍必须回到 DDS&OP / DDMRP 白盒引擎重新计算库存、红区周、补货订单、RCCP 和供应缺口。

状态：

| 项目 | 状态 | 说明 |
| --- | --- | --- |
| 网络结构评分候选动作组合选择 UI | Deferred | 已从网络结构评分工作台移除，避免误解为网络评分直接决策方案 |
| Solver Adapter | Reserved | 底层架构可保留，但当前不作为网络结构评分主流程能力 |
| DDS&OP 多方案组合选择 | Future | 后续在 DDS&OP 多方案比较阶段使用候选动作影响矩阵进行组合筛选 |
