# 网络结构评分产品拆分边界

## 目标

网络结构评分未来作为独立产品存在。它不替代 DDS&OP 场景运行，也不直接生成 DDMRP 计划；它负责读取已发布的网络主数据，发现控制点、解耦点、库存缓冲、时间缓冲和能力缓冲候选，并输出证据链、解释和不采纳风险。

DDS&OP 负责接收这些候选，进入场景预览、缓冲投影、RCCP、供应约束和主设置治理流程。最终业务结果必须由 DDS&OP / DDMRP 白盒引擎重算确认。

## 产品边界

| 产品 | 职责 | 不负责 |
|---|---|---|
| 网络结构评分产品 | 物料网络图、上游 / 下游影响范围、主数据校验、网络指标、控制点 / 缓冲点评分、候选动作库、证据链、不采纳风险 | 不运行 DDS&OP 场景、不发布主设置、不生成工单、不替代 DDMRP 公式、不直接生成计划 |
| DDS&OP 场景运行产品 | 异常识别、场景运行、缓冲 / 库存趋势、RCCP 与约束、供应商需求、场景留痕、主设置治理 | 不负责 BOM 网络结构评分算法，不直接把优化器结果当计划采纳 |

## 交互信息图

迁移后的两个产品通过边界契约传递信息：DDS&OP 向网络结构评分提供产品族 / SKU 范围、计划窗口、场景目标和采纳约束；网络结构评分返回网络指标、候选解耦点 / 缓冲建议、候选动作影响矩阵、组合选择结果、证据链和校验问题。最终库存金额、红区周、补货订单、RCCP 和供应缺口变化仍必须回到 DDS&OP 白盒引擎重算。

参考图位置：`material/DDSOP与网络结构评分迁移后交互信息图.png`。

## 网络结构评分产品输入

核心输入来自 PLM、ERP、MES、SRM 或主数据服务的已发布快照：

| 输入 | 业务含义 | 当前原型对应 |
|---|---|---|
| 物料主数据 | 成品、半成品、采购件、原材料，含产品族、成本和计量单位 | `NetworkItemMaster` |
| BOM 主记录与用量行 | 父项、组件、版本、生效日期、发布状态、用量、损耗、替代组 | `NetworkBomHeader` / `NetworkBomLine` |
| 替代料 | 主料、备料、优先级、替代比例、合格状态 | `NetworkAlternateItem` |
| 资源路线 | 物料、型号 / 产品族、工序、资源、单位能力消耗、生效期 | `NetworkRoutingLine` |
| 供应来源 | 多供应商、主供 / 备供、分配比例、固定提前期、波动率、周能力、MOQ | `NetworkSupplierSource` |
| 库存位置 | 半成品超市、待检区、合格库、线边库、质量状态、owner、是否共享 | `NetworkInventoryLocation` |
| 缓冲设置 | 当前已生效解耦点物料、库存缓冲、时间缓冲、MOQ、订货周期 | `NetworkBufferSetting` |
| 提前期档案 | 供应提前期、时间缓冲和波动率 | `NetworkLeadTimeProfile` |
| 外部运行信号 | ADU、需求规模、RCCP 负荷、供应缺口、库存水位等 | 当前通过 `NetworkStructureRuntimeSignals` 承载，迁移期由 DDS&OP 适配器填充 |

## 网络结构评分产品输出

| 输出 | 用途 | 下游消费方 |
|---|---|---|
| 物料关系图 | 解释物料上游 / 下游结构和路径累计用量 | 网络评分 UI、计划员 |
| 主数据校验报告 | 暴露缺 BOM、缺供应来源、缺资源路线、解耦点无库存位置等问题 | 主数据治理、计划员 |
| 网络指标 | 下游覆盖度、数量影响度、累计提前期、供应风险、资源约束、库存代价 | 网络评分服务 |
| 控制点 / 缓冲点候选 | 输出推荐类型、得分、证据和解释 | DDS&OP 场景验证、主设置治理 |
| 候选动作影响矩阵 | 描述动作成本、服务影响、库存影响、资源影响、供应缺口影响和约束 | 组合选择器 |
| 不采纳风险 | 解释不治理该候选可能产生的服务、供应、资源或现金风险 | 管理评审 |

## 迁移后的接口分层

建议未来拆分为以下项目或命名空间。当前原型可以先在同一 Web 项目内保持这些边界。

| 目标层 | 责任 | 当前可迁移代码 |
|---|---|---|
| `NetworkStructure.Core` | 网络主数据模型、请求、基础结果类型 | 已新增 `AdaptiveSopDdsop.NetworkStructure` 类库，承载 `NetworkDataSet` 及纯网络主数据 DTO；卫星制造多层 BOM seed 工厂 `SatelliteManufacturingNetworkSeedData` 也已迁入该类库 |
| `NetworkStructure.Graph` | 图构建、路径展开、主数据校验 | 已迁入 `AdaptiveSopDdsop.NetworkStructure`：`NetworkGraphService`、`NetworkGraphModels`、`INetworkGraphDataSource` |
| `NetworkStructure.Metrics` | 六项网络指标和证据链 | 已迁入 `AdaptiveSopDdsop.NetworkStructure`：`NetworkMetricsService`、`NetworkMetricsModels`、`INetworkMetricsDataSource` |
| `NetworkStructure.Scoring` | 候选点评分、推荐类型、解释、不采纳风险 | 已迁入 `AdaptiveSopDdsop.NetworkStructure`：`NetworkStructureScoringService`、`NetworkScoringModels`、`INetworkScoringDataSource` |
| `NetworkStructure.Optimization` | 候选动作组合选择，Gurobi / OR-Tools adapter | 已迁入 `AdaptiveSopDdsop.NetworkStructure`：`CandidateActionCombinationRequest`、`CandidateActionCombinationSelector`、`IOptimizationSolver`、`GurobiOptimizationSolver`、`OrToolsOptimizationSolver`、纯 0/1 组合求解模型 |
| `NetworkStructure.Web` | 物料关系图、指标表、评分候选、证据链 UI | 已新增 Razor Class Library：`AdaptiveSopDdsop.NetworkStructure.Web`。`NetworkStructure.cshtml`、`_NetworkStructureLayout.cshtml`、`_NetworkStructureWorkspace.cshtml`、`network-structure-shell.js`、`network-structure-workspace.js`、`network-structure-workspace.css` 均已迁入该项目；网络静态资源通过 `_content/AdaptiveSopDdsop.NetworkStructure.Web/...` 暴露。网络 partial / CSS 已改用 `network-*` 语义类名，不再借用 DDS&OP 的 `rccp-*`、`product-family-*`、`buffer-*` 等业务类名；`/network-structure` 使用专属 `_NetworkStructureLayout.cshtml`，只加载网络产品 CSS，不再通过 DDS&OP 默认 `_Layout` 拉入 `site.css`；当前已通过 `/api/network-structure-data` 读取窄网络数据包，并通过 `/api/network-structure-capabilities` 展示“产品能力边界”：独立能力、外部白盒依赖和边界说明；独立入口的返回按钮使用中性“返回业务平台”，不再表现为 DDS&OP 子页面。DDS&OP 首页不再挂载完整网络工作台，只保留跳转到独立产品入口的说明卡片 |
| `NetworkStructure.Host` | 独立部署外壳、网络产品 Razor 页面和纯网络 API | 已新增 `AdaptiveSopDdsop.NetworkStructure.Host`。该 Host 只引用 `AdaptiveSopDdsop.NetworkStructure` 与 `AdaptiveSopDdsop.NetworkStructure.Web`，不引用 `AdaptiveSopDdsop.Web`；它可独立启动 `/network-structure`，根路径 `/` 会重定向到网络结构评分工作台，并暴露 `/api/network-structure-capabilities`、`/api/network-structure-data`、`/api/network-structure-scoring`、`/api/network-metrics`、`/api/network-graph`。能力 API 已改为调用网络核心类库的 `NetworkStructureProductCapabilityCatalog`，避免 Host 手工维护能力清单。它不会执行外部 Scenario Preview；场景验证与候选组合采纳端点在独立 Host 中只返回中性边界说明，提示必须连接外部场景运行集成层进行白盒重算 |
| `Ddsop.Integration.NetworkStructure` | 将 DDS&OP 运行信号映射给网络产品，将网络候选转换为 DDS&OP 场景预览请求，并白盒重算验证 | 代码已集中到 `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration`，命名空间为 `AdaptiveSopDdsop.Web.NetworkStructureIntegration`。该边界包含 `NetworkStructureIntegrationContracts`、`NetworkStructureDataSourceAdapter`、`DdsopNetworkGraphDataSource`、`DdsopNetworkMetricsDataSource`、`DdsopNetworkScoringDataSource`、`NetworkCandidateRecalculationRequestBuilder`、`IDdsopWhiteBoxScenarioGateway`、`LocalDdsopWhiteBoxScenarioGateway`、`HttpDdsopWhiteBoxScenarioGateway`、`NetworkScenarioValidationService`、`CandidateActionCombinationService`；其中 `NetworkStructureDataSourceAdapter` 显式包装 `IScenarioWorkspaceDataSource`，不要求 seed 或生产 DDS&OP 数据源直接实现网络产品接口；`NetworkCandidateRecalculationRequestBuilder` 单独负责把网络候选动作翻译成 DDS&OP 白盒回算请求；`IDdsopWhiteBoxScenarioGateway` 是 DDS&OP 暴露给网络集成层的显式白盒重算接口，默认 `Local` 模式调用本地 Scenario Preview 服务，`Http` 模式可通过 `NetworkStructure:WhiteBoxGateway:BaseUrl` 和 `PreviewEndpoint` 跨进程调用 DDS&OP；`NetworkScenarioValidationService` 和 `CandidateActionCombinationService` 只依赖该接口，不直接依赖具体预览服务；DDS&OP 集成模块也暴露 `/api/network-structure-capabilities`，并复用网络核心能力 catalog，保证嵌入式入口和独立 Host 的边界说明一致 |

## 当前耦合点

| 耦合点 | 风险 | 迁移建议 |
|---|---|---|
| 网络服务直接依赖 `IScenarioWorkspaceDataSource` | 独立产品被 DDS&OP 数据包绑定 | 已处理：`NetworkGraphService` 已切换为 `INetworkGraphDataSource`；`NetworkMetricsService` 已切换为 `INetworkMetricsDataSource`；`NetworkStructureScoringService` 已切换为 `INetworkScoringDataSource` |
| 卫星制造多层 BOM seed 由 DDS&OP `SeedData` 直接构造 | 网络产品演示主数据形态被 DDS&OP Web 层拥有，未来迁移时会带走大量 Web seed 代码 | 已处理：完整网络 seed 构造迁入 `AdaptiveSopDdsop.NetworkStructure/SatelliteManufacturingNetworkSeedData.cs`；DDS&OP `SeedData` 不再构造或携带 `NetworkDataSet`，当前迁移期由 `NetworkStructureDataSourceAdapter` 把 DDS&OP SKU 运行上下文映射为 `NetworkFinishedGoodSeedInput` 并调用网络类库工厂 |
| 网络指标 / 评分服务直接调用 `DemandDrivenPlanningEngine`、RCCP 和供应约束计算 | 网络核心混入 DDS&OP 推演逻辑 | 已处理：`NetworkMetricsService` 与 `NetworkStructureScoringService` 均不再调用 DDS&OP 计划引擎；Web 侧 `DdsopNetworkMetricsDataSource` 和 `DdsopNetworkScoringDataSource` 负责准备 ADU、资源负荷、供应风险、红区周和缓冲区上沿等运行信号 |
| 场景验证与候选回算请求构造仍依赖 DDS&OP 场景模型 | 容易误以为网络产品负责计划重算，或把网络候选翻译逻辑混进验证服务 | 已处理为 DDS&OP 集成适配器：`NetworkCandidateRecalculationRequestBuilder` 负责候选到 `ScenarioRunPreviewRequest` 的翻译，`NetworkScenarioValidationService` 负责触发白盒回算和汇总结果；两者都不进入网络评分核心 |
| UI 混在 `Index.cshtml`、`app.js` 与 `site.css` 单页工作台 | 独立产品 UI 难迁移 | 已处理主要边界：完整网络工作台只在独立 Host 的 `/network-structure` 页面加载；DDS&OP 首页只保留跨产品入口卡片，入口地址来自 `NetworkStructure:ProductUrl`，不加载 `network-structure-workspace.css`、不加载 `network-structure-workspace.js`、不调用网络产品 API。网络结构渲染、图加载和候选组合选择已迁入 `network-structure-workspace.js`；网络图、候选组合、按钮、状态 chip、表格、面板标题等独立入口所需基础样式已迁入 `network-structure-workspace.css`；网络工作区自己的 KPI 条、摘要网格、筛选条、详情网格、详情摘要和动作条已使用 `network-kpi-strip`、`network-summary-grid`、`network-filter-bar`、`network-detail-grid`、`network-detail-summary`、`network-actions`，不再借用 DDS&OP 的 RCCP、产品族或缓冲类名；网络页面、layout、partial、JS 和 CSS 已物理迁入 `AdaptiveSopDdsop.NetworkStructure.Web`，DDS&OP Web 项目不再持有这些文件，也不再引用该 UI 包。`/network-structure` 独立页使用 `_NetworkStructureLayout.cshtml`，只加载网络 CSS 和网络脚本，不加载 DDS&OP 主 `app.js`、`site.css`，也不读取 `/api/scenario-workspace-data`。独立部署外壳 `AdaptiveSopDdsop.NetworkStructure.Host` 已建立，可脱离 DDS&OP Web 启动网络产品页面和纯网络 API |
| Gurobi / OR-Tools 入口仍在 DDS&OP 工作台中展示 | 容易与场景推荐混淆 | 只保留在候选动作组合选择区，不进入场景运行主流程 |
| Web 项目直接引用 Gurobi 包 | DDS&OP 产品部署被求解器许可和包依赖绑定 | 已处理：Gurobi 包、Gurobi / OR-Tools solver adapter 与纯 `CandidateActionCombinationSelector` 已迁入 `AdaptiveSopDdsop.NetworkStructure`；DDS&OP Web 不直接依赖 `IOptimizationSolver`，只调用网络结构类库的组合选择器 |
| 网络结构 API 与 DI 注册散落在 DDS&OP `Program.cs` | 独立产品 API 难以迁移，主程序耦合网络评分细节 | 已处理：网络结构评分 API 与 DI 已收敛到 `NetworkStructureIntegrationModule`；DDS&OP `Program.cs` 只挂载模块 |
| DDS&OP 网络集成文件散落在 `Domain` 主领域目录 | 主领域、跨产品适配器和网络产品边界仍容易混淆 | 已处理：`NetworkStructureIntegrationContracts`、网络数据源适配器、白盒网关、场景验证和候选组合服务均已移动到 `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration`，并使用 `AdaptiveSopDdsop.Web.NetworkStructureIntegration` 命名空间；`Domain` 目录不再承载网络结构评分集成合同或适配服务 |
| Web 项目通过 global using 暴露网络结构命名空间 | 让所有 DDS&OP 文件都天然依赖网络产品类型，后续迁移时难以判断真实依赖范围 | 已处理：删除 Web 项目的 `GlobalUsings.cs`，只在 seed 网络数据、网络集成合同、DDS&OP-to-network adapter、场景验证和候选组合服务等必要文件中显式 `using AdaptiveSopDdsop.NetworkStructure` |
| `ScenarioWorkspaceData.cs` 同时承载网络结构数据源接口 | 主场景数据模型继续“拥有”网络产品迁移合同，后续独立产品拆分时边界不清 | 已处理：网络结构集成数据包、运行信号和 `INetworkStructureDataSource` 已移入 `NetworkStructureIntegrationContracts.cs`，`ScenarioWorkspaceData.cs` 只保留 DDS&OP 场景工作台数据模型 |
| `ScenarioWorkspaceData.cs` 同时承载网络场景验证与候选组合 DTO | 主场景数据模型继续混入网络产品集成 API，后续独立迁移时合同边界不清 | 已处理：`NetworkScenarioValidationItem/Result`、`CandidateActionCombinationResult` 等带 DDS&OP 白盒结果的集成 API 合同已移入 `NetworkStructureIntegrationContracts.cs`；纯请求合同 `CandidateActionCombinationRequest` 与候选影响矩阵 `OptimizationCandidateImpact` 已移入 `AdaptiveSopDdsop.NetworkStructure` |
| DDS&OP 数据源通过强转注册为网络数据源 | 未来生产 DDS&OP 数据源只要没有实现网络接口，网络结构评分产品入口就会被 DDS&OP 数据源形态卡住 | 已处理：`SeedScenarioWorkspaceDataSource` 只实现 `IScenarioWorkspaceDataSource`；`ScenarioWorkspaceDataSet` 不再暴露 `NetworkData` 属性；`NetworkStructureIntegrationModule` 注册 `NetworkStructureDataSourceAdapter`，由适配器把 DDS&OP 工作台数据和 SKU seed 输入显式映射为网络窄数据包 |
| 网络数据源接口复用 `ScenarioWorkspaceDataRequest` | 网络结构评分产品的输入合同仍暴露 DDS&OP 场景工作台请求模型，后续迁移时会带出不必要依赖 | 已处理：新增 `NetworkStructureDataRequest`，`INetworkStructureDataSource.LoadNetworkStructure` 只接受网络请求模型；`NetworkStructureDataSourceAdapter` 内部负责转换为 DDS&OP 请求 |
| 独立 Host 的网络主数据 API 手工拼匿名响应 | 独立产品没有自己的纯网络数据入口合同，容易继续依赖 DDS&OP 集成数据包形态 | 已处理：`AdaptiveSopDdsop.NetworkStructure` 新增 `NetworkStructureProductDataRequest`、`NetworkStructureProductDataSet` 和 `INetworkStructureProductDataSource`；独立 Host 的 `StandaloneNetworkStructureDataSource` 实现该接口，`/api/network-structure-data` 通过纯网络产品合同返回 `request + networkData`，不引用 DDS&OP 集成合同 |
| 独立 Host 缺少产品能力自描述 | 未来独立部署或接入其它场景运行系统时，调用方不清楚哪些能力可由网络产品独立完成，哪些必须交给外部白盒系统 | 已处理：`AdaptiveSopDdsop.NetworkStructure` 新增 `NetworkStructureProductCapabilities`、`NetworkStructureCapability`、`NetworkStructureExternalDependency` 和 `NetworkStructureProductCapabilityCatalog`；独立 Host 与 DDS&OP 集成模块均暴露 `/api/network-structure-capabilities` 并复用同一 catalog。`/network-structure` 页面新增“产品能力边界”模块，展示独立能力、外部白盒依赖和“本 Host 不生成执行计划”等边界说明 |
| 网络前端模块隐式依赖 DDS&OP `app.js` 的状态和工具函数 | 独立页面运行时容易因缺少 `state`、`byId`、`money` 等主工作台变量而失败，也会让未来迁移边界变模糊 | 已处理：`/network-structure` 独立页由 `network-structure-shell.js` 显式注册 `window.NetworkStructureProductHost`；DDS&OP 首页不再注册网络宿主、不初始化网络模块。`network-structure-workspace.js` 只通过宿主契约访问状态、格式化、错误显示和多方案比较渲染，不再直接依赖 DDS&OP 主脚本词法变量；前端全局对象已改为中性 `NetworkStructureProduct*`，不再使用 DDAE 品牌前缀 |
| 网络二级模块折叠 / 专注查看只由 DDS&OP `app.js` 提供 | 独立网络页缺少产品级工作台交互，迁移后体验退化，且网络产品仍借用 DDS&OP 通用工作台行为 | 已处理：`network-structure-workspace.js` 已内置网络专属折叠与专注查看行为，使用 `data-network-collapse-*` / `data-network-focus-panel` 属性；`network-structure-workspace.css` 已承载对应样式；嵌入 DDS&OP 时若检测到主工作台已接管 `data-collapse-panel`，网络模块不会重复增强 |
| 网络核心评分解释写死 DDS&OP 业务语境 | 独立产品输出仍像 DDS&OP 内部模块，未来迁移到其它场景运行系统时解释不通用 | 已处理：`AdaptiveSopDdsop.NetworkStructure` 核心类库输出改为“外部场景运行系统”“管理评审”等中性产品语言；DDS&OP 字样只保留在 DDS&OP 集成层、边界说明和 DDS&OP 产品自身 |

## 前端宿主契约

网络结构评分工作台前端采用同一个工作区模块：`network-structure-workspace.js`。当前完整工作台只由 `/network-structure` 独立页加载，页面宿主必须注册 `window.NetworkStructureProductHost`。

该宿主契约只提供通用 UI 能力和当前页面状态，例如：状态对象、数字 / 金额格式化、DOM 查询、HTML 转义、状态标签、表格空状态、错误显示和多方案比较刷新。网络模块不得直接访问 DDS&OP `app.js` 的词法变量，也不得要求独立页面加载 DDS&OP 主脚本。

网络模块对外只暴露 `window.NetworkStructureProductWorkspace` 模块 API，例如初始化、评分渲染、指标渲染、场景验证渲染、物料图加载和候选组合选择。独立网络页只能调用这些公开方法，不直接调用 `renderNetworkStructureScoring`、`loadNetworkGraph` 等模块内部函数。DDS&OP 首页不调用这些方法，只提供独立产品入口。

网络模块还拥有自身数据加载编排：`NetworkStructureProductWorkspace.loadData(...)` 负责调用产品能力、网络主数据、评分、指标、物料图和场景验证 API。DDS&OP 首页不维护、不调用这些网络 API；独立网络页通过 `includeNetworkData: true` 告诉模块需要加载窄网络数据包。

网络模块拥有独立工作台交互：在 `/network-structure` 独立页中，网络评分块可以自行展开 / 收起、专注查看和退出专注；这些行为不要求加载 DDS&OP 主 `app.js`。历史嵌入兼容逻辑仍保留为防护，但 DDS&OP 首页当前不再加载网络模块。

当前两个宿主：

- DDS&OP 首页：只在总览区显示网络结构评分入口卡片和跨产品信息流说明；左侧主流程导航不再把网络结构评分列为 DDS&OP 流程步骤，不注册网络宿主、不初始化网络模块、不调用网络产品 API。
- 网络结构评分独立页：由 `network-structure-shell.js` 注册宿主契约，并通过 `loadData({ includeNetworkData: true })` 加载网络窄数据包。

## 第一阶段迁移策略

本阶段不直接拆项目，先建立可迁移结构。

1. 新增独立类库 `AdaptiveSopDdsop.NetworkStructure`，先承载纯网络主数据契约。状态：已完成第一步。
2. 新增 `NetworkStructureDataRequest` 与 `INetworkStructureDataSource`，返回 `NetworkStructureDataSet`。状态：已完成，并已从 `ScenarioWorkspaceData.cs` 移入 `NetworkStructureIntegrationContracts.cs`；接口不再暴露 DDS&OP 的 `ScenarioWorkspaceDataRequest`。
3. 新增 DDS&OP 适配器，把 `ScenarioWorkspaceDataSet` 的运行信号和 SKU seed 输入映射为 `NetworkStructureDataSet`。状态：已完成；`ScenarioWorkspaceDataSet` 不再携带 `NetworkDataSet`。
3.1 将网络场景验证、候选组合结果和白盒重算集成合同从 `ScenarioWorkspaceData.cs` 移入 `NetworkStructureIntegrationContracts.cs`；将候选动作组合请求和候选动作影响矩阵合同移入 `AdaptiveSopDdsop.NetworkStructure`。状态：已完成。
4. 让网络图、指标、评分服务逐步改为依赖各自专用数据源接口，避免核心服务读取 DDS&OP 完整工作台数据包。状态：图、指标、评分均已完成切换。
5. 将 `NetworkGraphService`、物料图结果模型和图构建数据源接口迁入独立类库，Web 侧只保留 `DdsopNetworkGraphDataSource` 适配器。状态：已完成。
6. 将 `NetworkMetricsService`、网络指标结果模型和指标数据源接口迁入独立类库，Web 侧只保留 `DdsopNetworkMetricsDataSource` 适配器。状态：已完成。
7. 将 `NetworkStructureScoringService`、评分结果模型和评分数据源接口迁入独立类库，Web 侧只保留 `DdsopNetworkScoringDataSource` 适配器。状态：已完成。
8. 将 `IOptimizationSolver`、Gurobi / OR-Tools adapter、纯求解输入输出模型和 `CandidateActionCombinationSelector` 迁入独立类库。状态：已完成。
9. 保留 `NetworkCandidateRecalculationRequestBuilder`、`NetworkScenarioValidationService` 与 `CandidateActionCombinationService` 对 DDS&OP 场景模型的依赖，并明确它们属于 DDS&OP 集成层：builder 负责把网络候选动作转换为白盒重算请求；validation service 和 combination service 只通过 `IDdsopWhiteBoxScenarioGateway` 触发白盒回算，不直接依赖 `ScenarioRunPreviewService`。状态：已完成。
10. 将网络结构评分 API 与 DI 注册收敛到 `NetworkStructureIntegrationModule`，避免 DDS&OP 主 `Program.cs` 直接维护网络产品端点。状态：已完成。
11. UI 已从 DDS&OP 首页嵌入模式调整为独立产品入口模式。DDS&OP 首页只在总览区保留跳转到配置项 `NetworkStructure:ProductUrl` 的说明卡片，且左侧主流程导航不再把网络结构评分列为 DDS&OP 流程步骤；完整网络结构评分 Razor 由独立 Host 的 `/network-structure` 入口加载。网络结构评分页面、layout、partial、专属脚本和专属样式已迁入 `AdaptiveSopDdsop.NetworkStructure.Web` Razor Class Library；DDS&OP Web 不再引用该 UI 包，也不再物理托管网络结构评分页面。`/network-structure` 使用 `_NetworkStructureLayout.cshtml`，不再通过 DDS&OP 默认 `_Layout` 加载 `site.css`；独立入口读取 `/api/network-structure-data` 而不是 DDS&OP 全量工作台数据，作为可迁移视图模块。
12. 新增独立 Host `AdaptiveSopDdsop.NetworkStructure.Host`。状态：已完成第一版；该 Host 只引用网络核心类库和网络 Web 表现层，不引用 DDS&OP Web。它可以独立提供网络结构评分页面、网络数据、网络图、网络指标与评分 API；根路径 `/` 直接进入 `/network-structure`；Host 运行时边界文案使用“外部场景运行集成层 / 外部白盒引擎”等中性表达，不硬编码 DDS&OP 产品名。DDS&OP 白盒场景验证和候选组合采纳仍明确留在 DDS&OP 集成层。

## 完成判据

第一阶段完成时，应满足：

- 网络结构评分核心服务不再直接读取完整 `ScenarioWorkspaceDataSet`。
- 纯网络主数据 DTO 不再由 DDS&OP Web Domain 拥有，而由 `AdaptiveSopDdsop.NetworkStructure` 拥有。
- 卫星制造多层 BOM、关键采购件、替代料、供应来源、routing、库存位置和缓冲设置的演示 seed 不再由 DDS&OP `SeedData` 直接拥有，而由 `SatelliteManufacturingNetworkSeedData` 拥有；DDS&OP 场景数据包不携带网络数据，当前迁移期只由 `NetworkStructureDataSourceAdapter` 在产品边界传入成品 SKU 种子输入。
- 物料图构建服务不再由 DDS&OP Web Domain 拥有，而由 `AdaptiveSopDdsop.NetworkStructure` 拥有。
- 网络指标服务不再由 DDS&OP Web Domain 拥有，而由 `AdaptiveSopDdsop.NetworkStructure` 拥有。
- 网络结构评分服务不再由 DDS&OP Web Domain 拥有，而由 `AdaptiveSopDdsop.NetworkStructure` 拥有。
- Gurobi / OR-Tools 求解器适配器不再由 DDS&OP Web Domain 拥有，而由 `AdaptiveSopDdsop.NetworkStructure` 拥有。
- 候选动作组合选择的纯请求合同与纯选择器不再由 DDS&OP Web Domain 拥有，而由 `AdaptiveSopDdsop.NetworkStructure` 拥有；DDS&OP 只保留白盒重算集成服务和含 DDS&OP 预览结果的响应合同。
- DDS&OP 白盒重算已收敛为 `IDdsopWhiteBoxScenarioGateway`。网络候选验证与候选组合服务不直接依赖 `ScenarioRunPreviewService`；本地实现 `LocalDdsopWhiteBoxScenarioGateway` 可以在未来替换为跨进程或 HTTP 接口。
- 白盒重算网关已具备可配置实现：`Mode=Local` 是当前默认同进程模式；`Mode=Http` 使用 `HttpDdsopWhiteBoxScenarioGateway` 调用 DDS&OP `/api/scenario-runs/preview`，为未来网络结构评分独立部署后连接 DDS&OP 服务预留接口。
- 网络结构评分 API 和 DI 注册不再散落在 DDS&OP 主程序，而由 `NetworkStructureIntegrationModule` 集中挂载；该模块位于 `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration`，不是 DDS&OP 主领域目录。
- 网络结构集成输入合同不再夹在 `ScenarioWorkspaceData.cs` 中，而由独立的 DDS&OP 集成契约文件承载。
- 网络产品自身拥有纯数据入口合同：`NetworkStructureProductDataRequest`、`NetworkStructureProductDataSet` 和 `INetworkStructureProductDataSource` 位于 `AdaptiveSopDdsop.NetworkStructure`，只承载网络主数据快照，不携带 DDS&OP 运行信号类型；DDS&OP 集成层的 `NetworkStructureDataSet` 继续作为迁移期适配数据包，负责附带外部运行信号。
- DDS&OP Web 项目不再通过 global using 暴露网络结构命名空间；网络产品类型只在必要适配器和集成合同中显式引用，DDS&OP 主数据模型与 seed 数据不直接引用网络产品命名空间。
- DDS&OP 首页不再挂载完整网络结构评分 partial，只在总览区保留独立产品入口卡片；左侧导航不再出现网络结构评分流程项。
- 网络结构评分 Razor / JS / CSS 不再混在 DDS&OP 主 `Pages`、`app.js` / `site.css` 中，而由 `AdaptiveSopDdsop.NetworkStructure.Web` 独立 Web 表现层承载；全局布局和 DDS&OP 首页都不加载网络 CSS / JS，DDS&OP Web 也不引用网络 UI 包；`/network-structure` 页面由独立 Host 提供，使用专属 `_NetworkStructureLayout.cshtml`，不加载 DDS&OP 主 `app.js` 和 `site.css`，只加载网络轻壳、网络工作台模块和网络 CSS，并通过 `/api/network-structure-data` 获取窄网络数据包。
- 网络结构评分 Razor partial 和专属 CSS 不再使用 DDS&OP 的 RCCP、产品族或缓冲语义类名；网络产品自有布局语义统一使用 `network-*` 命名，降低未来迁移时的业务语义残留。
- 网络结构评分具备独立 Host：`AdaptiveSopDdsop.NetworkStructure.Host` 不引用 `AdaptiveSopDdsop.Web`，可从 `/` 进入 `/network-structure`，并独立启动纯网络数据 API、网络图 API、网络指标 API 与网络评分 API。
- 独立 Host 的 `/api/network-structure-data` 通过 `INetworkStructureProductDataSource` 读取网络产品数据，不依赖 DDS&OP 集成接口，也不手工拼装匿名数据包。
- 独立 Host 和 DDS&OP 集成模块的 `/api/network-structure-capabilities` 均通过网络核心类库的能力 catalog 返回产品能力、外部依赖和边界说明，便于未来其它场景运行系统接入；`/network-structure` 页面已直接展示这些边界。
- 独立 Host 不执行外部白盒场景重算，不调用 `ScenarioRunPreviewService`、`DemandDrivenPlanningEngine` 或 DDS&OP seed；需要库存金额、红区周、补货订单、RCCP、供应缺口变化时，仍必须通过 DDS&OP 集成层验证。Host 程序代码本身不出现 DDS&OP / Ddsop 专名，避免独立部署时呈现为 DDS&OP 子服务。
- 网络核心类库不在候选解释、评分范围或不采纳风险中硬编码 DDS&OP 产品名；跨产品关系只在 DDS&OP 集成层和边界文档中表达。
- DDS&OP 与网络评分通过明确接口传递候选、指标、证据和场景验证结果。
- 优化器只在候选动作影响矩阵上做组合选择。
- DDS&OP 白盒引擎仍是库存、RCCP、供应缺口和主设置治理结果的最终验证者。
- UI 上不把网络结构评分表现成 DDS&OP 内部子功能，而表现成可独立迁移的产品工作区。当前 Razor、网络专属 JS、独立页轻壳、专属 layout 和网络专属 CSS 已迁入 `AdaptiveSopDdsop.NetworkStructure.Web`，并已具备 `/network-structure` 独立入口；DDS&OP 首页只作为跨产品入口，不再承担网络工作台宿主职责。
