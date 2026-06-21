# Adaptive S&OP / DDS&OP 模型问题清单

本文件用于记录后续统一修改前的原则、问题、判断和拟修改方向。  
在问题全部确认前，不修改 C# 应用实现。

## 记录规则

- **原则判断**：认可 / 部分认可 / 反对。
- **回答口径**：同时从管理逻辑和当前模型实现两个层面判断。
- **拟修改方向**：只记录，不立即实施。
- **确认状态**：待确认 / 已确认 / 暂缓 / 不采纳。

---

## Q001：跨职能月度核心决策流程与计划输入展示

**用户原则**  
这个程序是由公司最高领导层（CEO 及 C-Level 高管）主导的跨职能、月度核心决策流程。根本目的在于打破企业内部各职能部门（销售、市场、运营、财务、研发）的孤岛效应，将所有人的局部目标拉齐到公司的整体全局战略中，达成“全公司一个计划”（Single Game Plan）的共识。

**用户问题**  
需求计划、供应计划等等为什么不能展示？这样各职能部门无法输入和评估。

**原则判断**  
认可。

**回答摘要**  
需求计划、供应计划、财务计划、产品组合计划不应该缺席。当前第一版更像 DDS&OP 战术情景驾驶舱，而不是完整的 CEO/C-Level 月度 Adaptive S&OP 决策流程。  
如果系统目标是形成 Single Game Plan，就必须让销售、市场、运营、财务、研发分别输入计划假设，并通过 Integrated Reconciliation 暴露冲突，最后由 Management Review 确认统一方案。

**当前模型缺口**  
- Adaptive S&OP 七步目前主要作为导航展示，没有形成每一步的输入表和评估表。
- Demand Plan、Supply Plan、Financial Plan、Portfolio Plan 缺少可编辑计划输入。
- Integrated Reconciliation 没有系统展示需求-供应-财务之间的差异与取舍。
- Management Review 还没有形成正式的 Single Game Plan 输出。

**拟修改方向**  
- 增加七步流程工作台。
- 为 Portfolio、Demand、Supply、Financial 分别增加可编辑输入区。
- 增加 Integrated Reconciliation 差异分析表。
- 增加 Management Review 决策记录与 Single Game Plan 输出。
- DDS&OP Scenario Lab 保留，但定位为战术投影与 DDOM 参数评估，不替代前面各职能计划。

**确认状态**  
待确认。

---

## Q017：DDOM 执行层回传、三性可视化、Act/Late 破防警告与 Demonstrated Capability

**用户原则**  
DDOM 作为实际的车间与供应执行层，在日常运转（按真实订单扣减水位、缺料补货）中，会将真实执行数据和异常情况通过偏差分析（Variance Analysis）回路逆向反馈给 DDS&OP 团队。

**用户问题**  
三性没有可视化信息：

- 可靠性（Reliability）：交期和订单承诺的兑现质量。
- 稳定性（Stability）：缓冲区实际库存水位波动是否平稳，有无反复穿透至红区（库存告急）或黑区（断料）。
- 流动速度（Velocity）：物料与车间工单在系统中的整体周转效率与产出速率。

同时缺失时间与库存缓冲区的破防警告。DDOM 应统计并回传各类例外发生频率。例如上游供应商或前道车间频繁延误，导致物料进入时间缓冲区红区（Act，必须采取加急行动）或晚期区（Late，流速已被破坏）的频率。这些数据应触发 DDS&OP 团队对上游供应链进行排查。  
还缺少真实资源负载与实际用量反馈，包括关键工作中心真实负载图表，以及各解耦点物料真实 ADU（Average Daily Usage）变化趋势。DDS&OP 团队应依据这些 demonstrated（已被证明的）实际能力数据修正未来补货策略。

**原则判断**  
认可。

**回答摘要**  
DDOM Feedback 必须从执行层事实出发，而不是从计划假设出发。DDS&OP 需要看到三性指标的历史趋势、异常频率、缓冲破防、Act/Late 警报、真实资源负载和 ADU 变化，才能判断 Master Settings 是否仍然适配现实。  
当前模型没有这些可视化和回传数据，因此无法用 demonstrated capability 修正未来补货策略和 DDOM 参数。

**当前模型缺口**  
- 没有 Reliability 可视化：承诺达成率、OTD、订单延期、客户承诺违约。
- 没有 Stability 可视化：库存水位波动、红区/黑区穿透、Net Flow 波动、反复破防频率。
- 没有 Velocity 可视化：Flow Index、工单周转、订单响应周期、缓冲恢复时间。
- 没有 Time Buffer Act / Late 警报统计。
- 没有 Inventory Buffer Red / Black 破防频率统计。
- 没有异常来源分类，例如供应商、前道车间、质量、运输、需求尖峰、Master Settings。
- 没有真实资源负载图表。
- 没有 Demonstrated ADU 趋势，无法与计划 ADU 或 DAF 后 ADU 对比。
- 没有基于实际能力数据修正补货策略的建议。

**拟修改方向**  
- 增加 `DDOM Feedback Dashboard`。
- Reliability 图表：
  - OTD / 承诺达成运行图
  - 延期订单帕累托
  - 客户承诺违约趋势
- Stability 图表：
  - 库存水位运行图
  - Red / Black 穿透频率
  - Net Flow 控制图
  - 反复破防热力图
- Velocity 图表：
  - Flow Index 运行图
  - 工单周转时间控制图
  - 缓冲恢复时间趋势
- Time Buffer 例外表：
  - Act 频次
  - Late 频次
  - 供应商/前道车间/运输/质量原因
  - 排查责任人
- Demonstrated Capability 表：
  - 关键资源实际负载
  - 实际产出速率
  - 实际 ADU
  - 计划 ADU
  - 偏差
  - 推荐补货策略调整。
- DDS&OP 依据这些反馈生成 Master Settings 修正建议：ADU、DLT、Variability Factor、Time Buffer、Capacity Buffer、供应商策略。

**确认状态**  
待确认。

---

## Q016：DDS&OP 驾驶员职责、DAF/区域调整因子、库存/时间/产能缓冲配置

**用户原则**  
DDS&OP 作为“驾驶员”，根据未来的战术预测、企业最新的战略目标（来自 Adaptive S&OP）以及外部已知事件，通过更新主设置（Master Settings）来直接配置和调整 DDOM 的边界与运行能力。

**用户问题**  
DDS&OP 团队无法根据战术预测调整 DDOM 中各个解耦点物料的库存缓冲区（红、黄、绿区）大小；对未来短期已知事件（如季节性旺季、大型促销、工厂计划内停机检修、新产品导入节奏），DDS&OP 没有计算并向 DDOM 输入需求调整因子（DAF, Demand Adjustment Factor）或区域调整因子，临时主动拉伸或压缩 DDOM 的缓冲区厚度，提前让工厂防线处于安全水平；DDS&OP 无法指定 DDOM 中关键的鼓点资源（Drum/Pacing Resources，即瓶颈），并为其配置合理的时间缓冲区（Time Buffers）和保护性产能缓冲区（Capacity Buffers），从而划定车间在短期内的总体产能调配边界。

**原则判断**  
认可。

**回答摘要**  
这是 DDS&OP “驾驶员”角色的核心操作界面。DDS&OP 不是简单观察缓冲状态，也不是直接排车间工单，而是根据战术预测、战略目标和已知事件，主动配置 DDOM 的边界条件。  
当前模型只有情景输入和结果输出，没有形成“事件识别 -> DAF/区域调整因子 -> 缓冲区重算 -> 时间/产能缓冲配置 -> 生效窗口 -> 执行边界”的闭环。

**当前模型缺口**  
- 没有按解耦点物料维护 Red / Yellow / Green 缓冲区的可编辑主设置。
- 没有 DAF 或区域调整因子机制，无法针对季节性、促销、停机、NPI 节奏进行短期主动拉伸/压缩。
- 没有事件日历或事件库，无法将已知事件映射到 DDOM 参数调整。
- 没有 DAF 生效窗口，例如起始周/月、结束周/月、适用产品族/区域/SKU/解耦点。
- 没有 Drum / Pacing Resources 的指定界面。
- 没有 Time Buffer 主设置，例如保护提前期、异常触发阈值、恢复目标。
- 没有 Capacity Buffer 主设置，例如保留产能、最大短期负载、加班/班次边界、红黄绿阈值。
- 没有把这些设置转化为车间短期总体产能调配边界。

**拟修改方向**  
- 增加 `DDS&OP Driver Console`。
- 增加 `Known Events Calendar`：
  - 事件类型：Seasonality / Promotion / Planned Shutdown / NPI Ramp / Supplier Disruption / Regional Surge。
  - 生效窗口、影响区域、影响产品族、责任部门。
- 增加 `DAF / Zone Adjustment Factors` 表：
  - Factor ID
  - 适用对象：Region / Product Family / SKU / Decoupling Point
  - 调整类型：Demand Adjustment / Zone Adjustment / Buffer Stretch / Buffer Compression
  - 调整因子
  - 起止周期
  - 触发事件
  - 审批状态
- 增加 `Inventory Buffer Master Settings`：
  - 解耦点物料
  - ADU
  - DLT
  - Variability Factor
  - DAF
  - Red / Yellow / Green
  - Current / Proposed / Approved / Effective。
- 增加 `Drum / Pacing Resource Settings`：
  - 鼓点资源
  - 保护对象
  - 可用能力
  - Time Buffer
  - Capacity Buffer
  - 短期产能调配边界
  - 红黄绿负载阈值。
- 在 DDS&OP 输出中明确：
  - 哪些缓冲被拉伸/压缩
  - 为什么调整
  - 何时生效
  - 对服务、现金、空间、产能和流速的影响
  - 是否超出战术权限，需要升级 AS&OP。

**确认状态**  
待确认。

---

## Q015：战略情景运营可行性、资产资金投影、偏差成本与资本重构业务案例

**用户详细说明**  
战略情景的运营可行性校验结果（Operational Feasibility & Capacity Check）应在 Adaptive S&OP 团队提出远期战略方向时，由 DDS&OP 通过模拟推演反馈刚性限制数据。例如开辟新的国际销售渠道、导入全新系列产品、接入长期大单时，需要回答：

1. 现有 DDOM 设计能否在不进行大改造的前提下直接承接这个长期大单，即缓冲区 Tolerances 压力测试结果。
2. 战略规划中，高层关键瓶颈资源（Pacing Resources / 鼓点资源）的宏观利用率和负载预测。

不同宏观情景下的资产与资金占用投影数据（Model Projections）需要在战略时间框架下说明不同业务情景需要匹配何种幅度的缓冲区拉伸，并量化反馈：

- 营运资金占用投影（Working Capital Requirements）：为了保护战略增量，各解耦点缓冲区平均在手库存金额需要增加多少。
- 空间与仓储需求（Space Requirements）：实物库存体量变动对仓库和物流物理容积带来的空间占用。
- 产能需求（Capacity Requirements）：是否会在未来特定战略周期引发设备或人力硬性短缺。
- 错失的 ROI 机会与浪费成本（Lost ROI Opportunities & Deviations）：最近战术周期内因运营模型表现不佳而错失的 ROI 机会，以及为应付变数支付的 Fast Freight、Overtime、Partial Ship 等变动开支。
- 超越战术权限的资本/重构权责方案（Strategic Master Setting Recommendations）：当需要改变固定成本结构时，提供经过核算、可货币化评估的业务案例。例如关键约束资源每分钟贡献毛利、加开第三班或购买新机台后的固定开支增加、ROI Delta 和流速 Delta。

**原则判断**  
认可。

**回答摘要**  
这不是普通情景模拟，而是 DDS&OP 向 AS&OP 提供的战略决策包。它必须把战略情景转译成运营模型承压程度、资源瓶颈、缓冲拉伸、现金占用、空间占用、产能缺口、偏差成本和资本重构建议。  
如果没有这些内容，高管只能看到“增长目标”，看不到“运营模型是否承受得住、需要付出多少现金和固定成本、错失了多少 ROI、是否值得改造 DDOM”。

**当前模型缺口**  
- 没有 DDOM Tolerance Stress Test，无法判断现有缓冲区是否可承接战略增量。
- 没有 Pacing Resources / 鼓点资源的长期负载预测。
- 没有 Working Capital Requirements 的战略情景投影。
- 没有 Space Requirements，例如库存体积、库位、托盘数、仓储面积、物流容积。
- 没有 Capacity Requirements 的月份级缺口判断。
- 没有 Lost ROI Opportunities，无法量化因瓶颈未打通造成的错失贡献毛利。
- 没有 Deviations Cost，例如 Fast Freight、Overtime、Partial Ship、停线损失、质量返工成本。
- 没有资本/重构业务案例，无法比较维持现状 vs 第三班次 vs 新机台 vs 工程重设计的 ROI 和流速差异。
- 没有把这些结果形成 AS&OP Management Review 可决策的结构化提案。

**拟修改方向**  
- 增加 `Operational Feasibility & Capacity Check` 模块：
  - 情景名称
  - 战略增量需求
  - 现有 DDOM 可承接量
  - Buffer Tolerance 使用率
  - Pacing Resource 负载率
  - Feasibility 状态：可行 / 有条件可行 / 不可行
- 增加 `Model Projections` 模块：
  - Working Capital Requirements
  - Space Requirements
  - Capacity Requirements
  - Buffer Stretch Requirement
  - DDOM Master Settings 变更幅度
- 增加 `Deviation Cost & Lost ROI` 模块：
  - Fast Freight
  - Overtime
  - Partial Ship
  - Lost Sales / Lost Contribution Margin
  - Lost ROI Opportunity
  - Root Cause：Demand / Supply / Capacity / Quality / Master Settings / Execution
- 增加 `Strategic Business Case` 模块：
  - 维持现状
  - 加开第三班
  - 购买新机台
  - 增加供应商
  - 工程重设计
  - 每个方案展示固定成本变化、贡献毛利变化、ROI Delta、Flow Delta、现金占用 Delta、服务水平 Delta。
- 在 Management Review 中将这些结果汇总成可审批提案。

**确认状态**  
待确认。

---

## Q014：Strategic Projection 与远期战略情景 Feasibility Check

**用户原则**  
战略预测/投影（Strategic Projection）用于协助评估高管层正在讨论阶段的各种远期战略情景，从运营现实的角度提供科学的可行性校验（Feasibility Check）。这是实现战略到执行双向自适应最核心的技术动作。

**用户问题**  
没有提供可行性校验。

**原则判断**  
认可。

**回答摘要**  
Adaptive S&OP 的战略情景不能停留在收入目标或市场假设层面。每个远期战略情景都需要由 DDS&OP 投影到 DDOM 能力、缓冲、供应商、资本、现金和服务风险上，形成可行性校验。  
当前模型只有短期情景计算，没有对 24-36 个月战略情景进行可行性评分、缺口识别和管理建议，因此无法支撑高管层战略投影决策。

**当前模型缺口**  
- 没有 Strategic Projection 输入，例如增长情景、市场扩张、产品组合变化、NPI 上市、产能策略。
- 没有 24-36 个月的情景对比。
- 没有 Feasibility Check 评分。
- 没有按维度校验可行性：需求、供应、资源、供应商、现金、ROI、服务、DDOM 稳健性。
- 没有展示战略情景对 Master Settings 的要求。
- 没有输出“可行 / 有条件可行 / 不可行”的判断。
- 没有列出使情景可行所需的战术配置或战略投资。

**拟修改方向**  
- 增加 `Strategic Projection` 工作台。
- 增加远期情景输入：
  - Baseline
  - Growth / Aggressive Growth
  - Downside
  - New Market Entry
  - NPI Acceleration
  - Supplier Disruption
  - Capacity Investment
- 增加 Feasibility Check 矩阵：
  - Demand Feasibility
  - Supply Feasibility
  - Capacity Feasibility
  - Supplier Feasibility
  - Financial Feasibility
  - DDOM Feasibility
  - Service Feasibility
- 每项输出红黄绿状态、缺口数值、风险说明、所需动作。
- 增加情景评分：总体可行性分数、主要约束、关键决策点。
- 输出建议：调整 Master Settings、增加班次、资本投资、供应商开发、产品组合调整、财务目标重设。
- 将 Strategic Projection 的结果送入 Management Review 和 Strategic Recommendation Board。

**确认状态**  
待确认。

---

## Q013：Strategic Recommendation 战略建议与向 AS&OP 升级通道

**用户原则**  
战略建议（Strategic Recommendation）指当战术团队发现 DDOM 面临系统性缺陷，或者优化方案超越了战术团队自身的常规权限时，需要形成结构化的决策提案递交给 Adaptive S&OP 高层决策会。

**用户问题**  
没有给出升级渠道，例如增设第三班次、购置新固定资产，或由于某物料严重扼杀流动性而建议研发部对其进行重新工程化设计。

**原则判断**  
认可。

**回答摘要**  
DDS&OP 不能只输出操作建议。对于超出战术权限的问题，必须形成结构化战略建议并升级到 Adaptive S&OP 管理评审。  
例如能力长期红区、关键物料持续扼杀流动性、单一供应商风险、产品设计导致缓冲长期失效、或现金约束与服务目标冲突，这些都不是简单调参能解决的，必须进入高层决策通道。

**当前模型缺口**  
- 没有 Strategic Recommendation / Escalation Channel。
- 管理建议只是文本列表，没有提案结构、责任人、财务影响和决策状态。
- 没有区分战术可处理事项与战略需升级事项。
- 没有资本投资建议，例如新增班次、新设备、新产线、新仓储。
- 没有工程重设计建议，例如关键物料导致流动性受限、BOM 复杂度过高、替代料不足。
- 没有将建议连接到 AS&OP Management Review 的议题池。

**拟修改方向**  
- 增加 `Strategic Recommendation Board`。
- 提案字段：
  - Recommendation ID
  - 类型：Capacity / Capital / Supplier / Engineering / Portfolio / Financial
  - 触发信号：长期红区、能力超载、现金超限、供应商风险、设计复杂度
  - 问题描述
  - 推荐动作：第三班次、购置固定资产、供应商开发、研发重设计、产品退出等
  - 财务影响：收入保护、贡献毛利、投资额、ROI、回收期
  - 运营影响：服务水平、Flow Index、缓冲健康、风险降低
  - 决策人：CEO/COO/CFO/CTO/VP Supply Chain 等
  - 状态：Draft / Submitted / Approved / Rejected / Deferred / Implemented
- 增加升级规则：
  - 连续 N 期红区穿透 -> 战略建议
  - 资源负载连续超过阈值 -> 班次/资本投资建议
  - 单一物料导致多产品族流动受阻 -> 工程重设计建议
  - 缓冲调整无法恢复服务 -> AS&OP 管理评审
- Management Review 中增加战略建议议题池和决策记录。

**确认状态**  
待确认。

---

## Q012：Tactical Exploitation 战术开拓与流动红利机会

**用户原则**  
战术开拓（Tactical Exploitation）指当运营模型显现出富余产能或临时机会时，DDS&OP 团队需要寻找“用最少变动成本换取最大流动红利”的解法。例如，利用现有空闲产能安排短期促销以拉动现金流，或优化货运路线（在不破坏交期的前提下将加急空运改为低成本慢运）以精简变动支出。

**用户问题**  
没有给出这方面的内容。

**原则判断**  
认可。

**回答摘要**  
DDS&OP 不只是风险防御和缓冲纠偏，也应主动识别可利用的机会。  
当 DDOM 显示某些资源有富余能力、缓冲充裕、服务风险低或物流路径可优化时，DDS&OP 应提出战术开拓建议，把短期机会转化为现金流、收入、成本节约或流动性提升。当前模型只有风险型管理建议，没有机会型建议和收益测算。

**当前模型缺口**  
- 没有识别富余产能或低负荷资源。
- 没有判断缓冲充裕时是否可支持短期促销或需求创造。
- 没有将机会转化为现金流、贡献毛利或 Flow Index 改善。
- 没有物流成本优化情景，例如空运转海运/陆运、合并运输、延迟发运。
- 没有 Opportunity Backlog 或 Tactical Exploitation 建议清单。
- 管理建议偏风险防御，缺少“主动开拓”维度。

**拟修改方向**  
- 增加 `Tactical Exploitation` 区域。
- 增加机会识别规则：
  - 资源负载低于阈值且缓冲健康 -> 推荐短期促销或抢单。
  - 缓冲高于 Top of Green 且服务风险低 -> 推荐降库存/降低补货频率/释放现金。
  - 加急运输比例高但交期风险低 -> 推荐切换低成本运输方式。
  - 供应商能力富余且价格窗口有利 -> 推荐策略性采购或锁价。
- 增加机会收益测算：
  - 增量收入
  - 增量贡献毛利
  - 变动成本
  - 现金流影响
  - Flow Index 改善
  - 服务风险
- 增加机会建议状态：Candidate、Evaluate、Approved、Executed、Expired。
- 在 Management Review 中区分 `Risk Mitigation` 与 `Tactical Exploitation` 两类行动。

**确认状态**  
待确认。

---

## Q011：Tactical Configuration and Reconciliation、DAF 与动态 Master Settings 对齐

**用户原则**  
战术配置与对齐（Tactical Configuration and Reconciliation）是将业务计划转化为模型参数的关键。DDAE 模型取消了传统精确定时定量的主生产计划（MPS），改由主设置（Master Settings）替代。DDS&OP 团队通过动态调整解耦点位置、重塑库存/时间/产能缓冲区的参数大小（例如应用需求调整因子 DAF），确保运营模型的能力弹性与最新的业务需求保持对齐。

**用户问题**  
前面描述过，这里再次确认。

**原则判断**  
认可。

**回答摘要**  
确认：后续统一修改时，应把 DDS&OP 的核心定位明确为 Tactical Configuration and Reconciliation，而不是短期 MPS 或单纯情景计算。  
AS&OP 给出业务目标、战略参数和财务边界；DDS&OP 将其转化为 DDOM Master Settings 的变更，包括解耦点、库存缓冲、时间缓冲、产能缓冲，以及 DAF 等动态调整因子；DDOM 则通过 Net Flow Equation 接收真实需求拉动。

**当前模型缺口**  
- 缺少 Tactical Configuration and Reconciliation 的专门页面/流程。
- 缺少 DAF（Demand Adjustment Factor）等参数机制。
- 缺少业务计划参数到 DDOM Master Settings 的映射逻辑。
- 缺少解耦点位置动态调整。
- 缺少库存/时间/产能缓冲区参数重塑的操作记录和影响分析。
- 当前情景输入直接影响结果，但没有明确形成“参数变更建议 -> 批准 -> 生效”的治理闭环。

**拟修改方向**  
- 增加 `Tactical Configuration` 工作台。
- 增加业务计划参数输入：增长率、促销、产品组合、新品、服务目标、现金约束、供应风险。
- 增加 DAF 表：适用产品族/SKU、起止月份、调整因子、触发原因、影响 ADU。
- 增加 Master Settings Mapping：
  - 业务计划变化 -> DAF / ADU
  - 供应风险 -> DLT / Time Buffer
  - 波动增加 -> Variability Factor
  - 能力约束 -> Capacity Buffer / Resource Guardrail
  - 组合变化 -> Decoupling Point Review
- 增加变更治理：Proposed、Reviewed、Approved、Effective、Expired。
- 在 UI 文案和数据流中明确：DDS&OP 不是生成 MPS，而是配置 DDOM 主设置并保持模型弹性与业务需求对齐。

**确认状态**  
待确认。

---

## Q010：Reliability / Stability / Velocity 的可视化变异分析

**用户原则**  
三个核心指标为可靠性（Reliability）、稳定性（Stability）和流速（Velocity）。应通过帕累托图、运行图和控制图进行变异分析（Variance Analysis），找出导致缓冲区破防、触发黑区或产生额外支出的根本原因。

**用户问题**  
没有任何可视化信息设计。

**原则判断**  
认可。

**回答摘要**  
DDOM Performance Feedback 不能只用表格。可靠性、稳定性和流速的变异分析需要图形化，因为高管和跨职能团队需要快速看到趋势、异常、主因和系统性波动。  
当前模型没有帕累托图、运行图、控制图，也没有把缓冲区破防、黑区触发、额外支出与根因关联起来，因此无法支持 DDS&OP 的偏差分析会议。

**当前模型缺口**  
- 没有 Reliability / Stability / Velocity 的专门可视化区域。
- 没有帕累托图来识别主要根因，例如供应延迟、需求尖峰、质量问题、能力不足、参数错误。
- 没有运行图来展示指标随周期变化的趋势。
- 没有控制图来识别正常波动与异常波动。
- 没有黑区/红区穿透次数的可视化。
- 没有额外支出可视化，例如加急费、空运费、停线损失、过量库存资金成本。
- 没有将可视化与管理行动建议连接。

**拟修改方向**  
- 增加 `Variance Analytics` 区域。
- 增加 Reliability 图组：
  - 服务水平运行图
  - 缺货/红区穿透帕累托
  - 客户承诺达成控制图
- 增加 Stability 图组：
  - ADU 波动控制图
  - Net Flow Position 运行图
  - 异常需求/供应波动帕累托
- 增加 Velocity 图组：
  - Flow Index 运行图
  - 缓冲恢复时间控制图
  - 订单响应周期趋势图
- 增加 Cost of Variance 图组：
  - 加急费、空运费、停线损失、库存资金成本的帕累托。
- 增加根因分类维度：Demand、Supply、Quality、Capacity、Master Settings、Execution。
- 可视化输出直接驱动 DDS&OP 行动建议和 AS&OP 升级议题。

**确认状态**  
待确认。

---

## Q009：DDOM 历史健康度、Performance Feedback、变异分析与审计报告

**用户原则**  
运营模型历史健康度数据（DDOM Performance Feedback）应反馈 DDOM 在过去周期的可靠性、稳定性和流动速度相关的变异分析与审计报告。这能让战略层清晰地知道公司目前的“身体素质”处于何种状态，以及安全底线是否有被反复穿透的风险。

**用户问题**  
运营模型历史健康度数据缺失，无法看到相关的变异分析。

**原则判断**  
认可。

**回答摘要**  
只看当前缓冲状态无法判断运营模型是否健康。战略层需要看到过去周期中 DDOM 的表现趋势：可靠性是否提升、稳定性是否恶化、流动速度是否变慢、红区穿透是否反复发生、哪些解耦点或缓冲配置长期失效。  
当前模型没有历史周期数据，也没有把实际表现与目标/阈值进行变异分析，因此无法形成 DDOM Performance Feedback。

**当前模型缺口**  
- 没有历史周期数据，例如过去 12-26 周或过去 12 个月的缓冲状态。
- 没有可靠性指标历史，例如服务水平、缺货次数、红区穿透次数。
- 没有稳定性指标历史，例如 Net Flow 波动、ADU 波动、供应波动、计划调整频率。
- 没有流动速度指标历史，例如 Flow Index、平均恢复时间、订单响应周期、缓冲恢复周期。
- 没有审计报告，无法判断哪些缓冲区反复穿透安全底线。
- 没有将历史表现与 Master Settings 版本关联，无法知道参数调整是否有效。
- 没有为 AS&OP 提供“运营身体素质”视图。

**拟修改方向**  
- 增加 `DDOM Performance Feedback` 工作台。
- 增加历史健康度数据集：按周或月记录 SKU/产品族/缓冲点的实际状态。
- 增加三类历史指标：
  - Reliability：服务水平、缺货次数、红区穿透次数、客户承诺达成。
  - Stability：ADU 变异、Net Flow 变异、供应可靠性、异常订单比例。
  - Velocity：Flow Index、缓冲恢复时间、订单响应周期、库存周转。
- 增加 `Variance Audit` 表：目标、实际、偏差、连续偏差次数、安全底线穿透次数、责任原因。
- 增加趋势图或热力表：识别反复红区、反复供应失败、反复现金超限的模型薄弱点。
- 将历史健康度与 Master Settings 版本关联：显示参数调整前后 DDOM 表现是否改善。
- DDS&OP 向 AS&OP 输出 `Operational Fitness Report`，作为战略层决策输入。

**确认状态**  
待确认。

---

## Q008：DDS&OP 双向战术枢纽、偏差分析与战略自适应闭环

**用户原则**  
Adaptive S&OP 深度依赖一个双向的、完全解耦的战术衔接 hub（枢纽）——需求驱动 S&OP（DDS&OP）。DDS&OP 夹在战略（AS&OP）和运营（DDOM）中间，一方面基于运营层实际缓冲区的偏差分析（Variance Analysis）向战略层输送真实的运营弹性报告与修正推荐；另一方面根据 AS&OP 给出的长期战略参数来平滑地微调底层缓冲区，形成复杂系统特有的双向自适应闭环（Strategic Adaptive Loop）。

**用户问题**  
这种反馈没有建立，看不到真实的运营弹性报告，无法根据长期战略参数来平滑地微调底层缓冲区。

**原则判断**  
认可。

**回答摘要**  
DDS&OP 不是单向的情景计算器，而是战略层和运营层之间的双向战术适配机制。  
向上，它应该把 DDOM 实际运行的缓冲偏差、流动性、服务、库存和能力异常转化为运营弹性报告，并反馈给 Adaptive S&OP。向下，它应该把战略层给出的增长、组合、服务、财务和能力参数转化为 DDOM Master Settings 的渐进式调整。当前模型只有“输入情景 -> 输出结果/建议”的单向模拟，没有形成真实的偏差分析、弹性报告和双向闭环。

**当前模型缺口**  
- 没有运营实际数据与计划/模型期望之间的 Variance Analysis。
- 没有 Buffer Variance，例如实际缓冲区状态 vs 目标缓冲区状态。
- 没有 Flow Variance、Service Variance、Inventory Variance、Capacity Variance。
- 没有 Operational Resilience Report，例如在需求/供应扰动下系统维持服务和流动性的能力。
- 没有从 DDOM 实际表现向 AS&OP 反馈的修正建议。
- 没有从 AS&OP 长期战略参数向 DDOM Master Settings 平滑调整的机制。
- 没有 Strategic Adaptive Loop 的可视化数据流：AS&OP -> DDS&OP -> DDOM -> Variance -> DDS&OP -> AS&OP。

**拟修改方向**  
- 增加 `DDS&OP Hub` 工作台，明确显示双向数据流。
- 增加 `Variance Analysis` 表：
  - 指标：Buffer Health、Flow Index、Service、Inventory、Capacity、Working Capital。
  - 目标值、实际值、偏差、偏差原因、建议动作。
- 增加 `Operational Resilience Report`：
  - 需求上行弹性
  - 供应中断承受周数
  - 缓冲恢复时间
  - 服务保持能力
  - 现金消耗速度
- 增加 `Strategic Parameter Intake`：
  - 增长率目标
  - 服务水平目标
  - 现金约束
  - 产品组合变化
  - 能力投资边界
- 增加 `Smooth Buffer Adjustment` 机制：
  - 根据战略参数建议 ADU、DLT、Variability、Order Cycle、Capacity Buffer 的渐进调整。
  - 显示调整幅度、影响、风险和生效月份。
- 增加闭环可视化：
  - AS&OP 战略参数下传
  - DDS&OP 参数评估
  - DDOM 运行结果
  - 偏差分析上报
  - 战略层修正建议

**确认状态**  
待确认。

---

## Q007：Adaptive S&OP 不下传 MPS，而下传 DDOM Master Settings

**用户原则**  
Adaptive S&OP 明确指出“主生产计划（MPS）在自适应计划流程中已经消失了”（"the Master Production Schedule is gone from the S&OP process"）。Adaptive S&OP 向下传递的不是精确定期定量的排程表，而是通过战术层去配置和更改解耦点（Decoupling Points）、库存/时间/产能缓冲区的主设置（Master Settings），在实物执行上完全由最真实的销售订单（Net Flow Equation）动态拉动，以此实现“大致正确，而不是精准错误”。

**用户问题**  
配置和更改解耦点、库存/时间/产能缓冲区的 Master Settings 没有看到。

**原则判断**  
认可。

**回答摘要**  
Adaptive S&OP 不能被设计成远期 MPS 生成器。它的输出应是战略目标、能力边界、财务约束和 DDOM Master Settings 的变更决策。  
DDS&OP 承接这些战略输入，评估 DDOM 是否需要重构，并把被批准的主设置传递给运营模型。实际执行则由真实订单、库存位置、开放供应和合格需求通过 Net Flow Equation 拉动，而不是由 S&OP 生成精确的远期生产订单。

**当前模型缺口**  
- 当前 UI 仍容易被理解为“情景需求变化 -> 计算订单建议”，没有明确区分 Adaptive S&OP 输出与 DDMRP 执行输出。
- 没有 Master Settings Change Request 表，无法显示谁提出、为什么提出、改什么、何时生效、影响是什么。
- 没有 Decoupling Point Change 视图，无法新增、移动、取消或重设解耦点。
- 没有 Inventory Buffer / Time Buffer / Capacity Buffer 三类缓冲主设置配置。
- 没有显示 Master Settings 的 Current / Proposed / Approved / Effective 状态。
- 没有把 Net Flow Equation 明确作为运营执行的拉动机制展示出来。
- 没有把 MPS 从 Adaptive S&OP 输出中显式排除，容易与传统 S&OP/MPS 混淆。

**拟修改方向**  
- 将 Adaptive S&OP 输出定义为 `Strategic Direction + Financial Guardrails + Master Settings Decisions`，不输出 MPS。
- 增加 `Master Settings Change Board`：
  - Change ID
  - 变更类型：Decoupling Point / Inventory Buffer / Time Buffer / Capacity Buffer
  - 当前设置
  - 建议设置
  - 触发原因：需求变化、供应风险、NPI、生命周期、资本约束
  - 影响评估：服务、流动性、现金、能力、ROI
  - 状态：Proposed / Approved / Rejected / Effective
- 增加 `Decoupling Point Map`：展示每个解耦点保护的产品族、物料族、供应链位置和 DLT。
- 增加三类缓冲配置表：
  - Inventory Buffer：ADU、DLT、Variability、MOQ、Red/Yellow/Green。
  - Time Buffer：保护提前期、可靠性、异常触发规则。
  - Capacity Buffer：保护资源、保留能力、红黄绿负载阈值。
- 增加 `Net Flow Equation` 执行视图：On Hand + Open Supply - Qualified Demand = Net Flow Position，并说明它是执行拉动机制，不是战略层 MPS。
- 在页面文案和数据流上明确：Adaptive S&OP 不生成远期生产排程，DDS&OP 只调整 DDOM 主设置，执行由真实需求拉动。

**确认状态**  
待确认。

---

## Q006：CAS 视角下的 DDOM Master Settings 配置与重构

**用户原则**  
Adaptive S&OP（DDAE 体系）站在复杂自适应系统（CAS）的角度。在现代 VUCA 环境下，细颗粒度的长期预测必然是“精确错误的”。因此，Adaptive S&OP 的情景模拟核心不关注未来某天具体的生产订单数量，而是关注如何配置和重构需求驱动运营模型（DDOM）的资源能力和参数范围（Master Settings）。

**用户问题**  
没有看到如何配置运营模型（DDOM）的资源能力和参数范围。

**原则判断**  
认可。

**回答摘要**  
Adaptive S&OP 不应把长期情景模拟做成远期订单排程，而应评估不同未来条件下 DDOM 是否仍然稳健，以及需要如何调整主设置。  
DDOM Master Settings 应该是系统中的核心治理对象，包括解耦点、缓冲配置、ADU 调整规则、DLT、变异因子、订单周期、MOQ、能力保护、关键资源/供应商约束和执行优先级规则。当前模型只在计算中隐含使用了部分 SKU 缓冲参数，没有提供 Master Settings 的配置、版本、影响评估和重构建议。

**当前模型缺口**  
- DDOM 参数只存在于 seed data 和计算结果中，没有形成可配置的 Master Settings 工作台。
- 没有显示解耦点（Strategic Decoupling Points）与控制点（Strategic Control Points）。
- 没有展示参数范围，例如 ADU 区间、DLT 区间、Variability Factor 区间、Order Cycle 区间。
- 没有资源能力 Master Settings，例如资源保护能力、容量缓冲、供应商能力上限。
- 没有对不同情景下的 Master Settings 版本进行对比，例如 Current、Promotion、Supply Disruption、Growth、Downturn。
- 没有说明“重构 DDOM”到底改哪些参数、为什么改、对流动性/现金/服务有什么影响。

**拟修改方向**  
- 增加 `DDOM Master Settings` 工作台。
- 增加 Decoupling Point 表：位置、保护对象、DLT、缓冲类型、控制规则、关联产品族。
- 增加 Buffer Profile 表：ADU、DLT、Variability Factor、Order Cycle、MOQ、Red/Yellow/Green、参数上下限。
- 增加 Capacity Master Settings 表：关键资源、保护能力、最大负荷、能力缓冲、扩容触发点。
- 增加 Supplier Master Settings 表：关键供应商、可承诺能力、交期区间、风险等级、替代策略。
- 增加 Master Settings Version 对比：当前设置 vs 情景建议设置 vs 管理批准设置。
- DDS&OP Scenario Lab 输出不只显示结果，还要显示“建议重构哪些 Master Settings”。
- Management Review 增加 DDOM 重构决策：批准/拒绝/延迟参数调整，并记录生效月份。

**确认状态**  
待确认。

---

## Q005：战略层资源剖面、RRP、瓶颈负载红黄绿、关键供应商与资本投资需求

**用户原则**  
战略层不进行精细的车间派工计算，而是使用资源剖面（Resource Profiles）或高层产能模型（如资源需求计划 RRP），对工厂的关键瓶颈设备、供应链关键供应商或核心资本投资需求进行宏观 feasibility（可行性）核对。

**用户问题**  
对工厂的关键瓶颈设备给出了负载率，但是没有红黄绿标志，没有供应链关键供应商或核心资本投资需求。

**原则判断**  
认可。

**回答摘要**  
Adaptive S&OP 的供应计划不应做车间派工，但必须做宏观可行性检查。高管层需要看到哪些资源在未来 24-36 个月会成为约束，哪些关键供应商会限制增长，哪些资本投资必须提前决策。  
当前模型只给出一个总体 Capacity Utilization，没有形成资源剖面、红黄绿例外标识、供应商约束或资本投资需求，因此无法支撑战略层 feasibility 评估。

**当前模型缺口**  
- 当前只有总体能力利用率，没有按关键资源/资源族展示负载率。
- 没有红黄绿阈值，例如绿色 <= 85%、黄色 85%-100%、红色 > 100%。
- 没有 Resource Profile / RRP 逻辑，把产品族需求转换为关键瓶颈资源需求。
- 没有供应链关键供应商视图，例如供应商产能、交期、风险等级、单一来源风险。
- 没有资本投资需求视图，例如需要新增设备、模具、产线、仓储或认证能力。
- 没有把投资需求连接到财务计划和 ROI。

**拟修改方向**  
- 增加 Supply Plan / Feasibility 工作台。
- 增加 Resource Profile 表：产品族、资源、每单位资源需求、月度需求、月度资源负荷、可用能力、负载率、红黄绿状态。
- 增加 Key Supplier Constraint 表：供应商、物料族、供应能力、需求、缺口、交期、风险等级、缓解动作。
- 增加 Capital Requirement 表：投资项、触发月份、约束资源、投资额、上线时间、产能增量、ROI/回收期。
- 在 24-36 个月 Horizon 中展示瓶颈资源、供应商和资本投资的未来缺口曲线。
- Integrated Reconciliation 中增加 Supply Feasibility Gap：需求可达但产能/供应/资本不可达时，生成管理取舍议题。

**确认状态**  
待确认。

---

## Q004：财务货币化、营业收入、贡献毛利、现金流与 ROI 风险测算

**用户原则**  
传统 S&OP 仅仅对齐“实物量（如吨数、箱数、工时）”存在局限。财务代表不是事后记账员，而是深度参与到每个步骤里，将产品需求和供应能力实时货币化，转变为对现金流、贡献毛利（Contribution Margin）、营业收入和整体投资回报率（ROI）的全面评估和风险测算。

**用户问题**  
没有给出预期营业收入和整体投资回报率（ROI）等的全面评估和风险测算。

**原则判断**  
认可。

**回答摘要**  
Adaptive S&OP / DDS&OP 不能只回答“能不能生产、有没有库存、缓冲是否健康”，还必须回答“这个计划是否值得做、是否支撑财务目标、现金是否承受得住、ROI 是否合理”。  
如果没有收入、贡献毛利、现金流和 ROI 的动态评估，系统仍然停留在运营计划工具，而不是 C-Level 可用的企业经营决策流程。

**当前模型缺口**  
- 当前只计算了 Working Capital Impact，财务维度过窄。
- 没有按产品族和月份计算营业收入、贡献毛利、毛利率、现金占用和 ROI。
- 没有把需求情景和供应情景实时货币化。
- 没有比较财务目标与计划结果之间的缺口，例如 Revenue Gap、Margin Gap、ROI Gap、Cash Gap。
- 没有风险测算，例如悲观/基准/乐观情景、供应中断对收入损失的影响、促销对毛利和现金的影响。
- Management Review 中缺少财务取舍建议，例如服务水平提升是否值得、库存增加是否超过现金约束、NPI 投入是否满足回报门槛。

**拟修改方向**  
- 增加 Financial Plan 工作台，按 24-36 个月滚动 Horizon 展示收入、贡献毛利、现金流、库存现金占用和 ROI。
- 为 SKU / 产品族增加财务参数：售价、变动成本、贡献毛利率、固定投入、NPI 投资、库存资金成本。
- 将 Demand Plan 货币化为 Revenue Projection 和 Contribution Margin Projection。
- 将 Supply Plan 货币化为 Capacity Cost、Inventory Investment、Expedite Cost 和 Lost Sales Risk。
- 增加情景财务评估：Baseline / Upside / Downside。
- 增加 ROI 计算：`ROI = (贡献毛利 - 增量运营成本 - 投资额) / 投资额`，并允许按产品族/NPI 项目/情景查看。
- Integrated Reconciliation 增加财务缺口矩阵：Revenue Gap、Margin Gap、Cash Gap、ROI Gap。
- Management Review 输出中增加财务决策建议：批准、推迟、削减、加投、退出。

**确认状态**  
待确认。

---

## Q003：产品组合、新活动、NPI/NPD 健康度与 SKU 理性化

**用户原则**  
标准的流程闭环都始于产品组合与新活动审查（Product Portfolio / New Activities）。流程都需要每月评估新产品研发流（NPI/NPD）的健康度、评估现有产品的生命周期改造以及呆滞 SKU 的淘汰理性化计划，确保产品线支撑公司长期业务增长。

**用户问题**  
没有每月评估新产品研发流（NPI/NPD）的健康度，也没有退出产品、呆滞 SKU 的淘汰理性化计划。

**原则判断**  
认可。

**回答摘要**  
Adaptive S&OP 的第一步必须是 Portfolio / New Activities，因为未来需求、供应能力、财务表现和复杂度都先由产品组合决定。  
如果没有 NPI/NPD 健康度、生命周期管理和 SKU 理性化，后面的 Demand Plan、Supply Plan、Financial Plan 会被动接受一个未经治理的产品组合，最终导致预测膨胀、长尾 SKU 占用能力和现金、研发项目与商业目标脱节。

**当前模型缺口**  
- 当前只有一个 NPI Sensor Pack SKU 作为示例，没有 NPI/NPD 项目漏斗。
- 没有 NPI 阶段门管理，例如 Concept、Development、Validation、Launch、Ramp-up。
- 没有按月评估 NPI 项目的进度、风险、目标收入、上市时间和资源占用。
- 没有产品生命周期状态，例如 Growth、Mature、Decline、End-of-Life。
- 没有呆滞 SKU、低毛利 SKU、低服务贡献 SKU 的淘汰/理性化计划。
- 没有展示产品组合变化对需求、供应能力、库存现金占用和复杂度的影响。

**拟修改方向**  
- 在 Adaptive S&OP 七步中的第一步增加 `Portfolio / New Activities` 工作台。
- 增加 NPI/NPD Pipeline 表：项目、阶段、上市月份、目标收入、研发风险、供应风险、商业负责人、下一阶段门决策。
- 增加产品生命周期表：SKU、生命周期阶段、近 12 个月需求、毛利、库存、呆滞风险、建议动作。
- 增加 SKU Rationalization 逻辑：保留、改造、合并、退出、最后采购、清库存。
- 增加 Portfolio Impact 评估：新产品/退出产品对未来 24-36 个月收入、能力、库存和现金的影响。
- Management Review 输出中增加组合决策：批准上市、延迟、终止、退出、资源重分配。

**确认状态**  
待确认。

---

## Q002：战略展望期 Horizon 与未来缺口管理

**用户原则**  
长期、宏观的战略与财务规划，其展望期（Horizon）通常至少为滚动的 24 个月到 36 个月。它们都强制要求高管层“退出细节磨炼，留在全局之上（Stay out of the weeds）”，重点管理未来的变化和缺口。

**用户问题**  
没有展望期，无法管理未来的变化和缺口。

**原则判断**  
认可。

**回答摘要**  
Adaptive S&OP 的管理对象不是短期执行明细，而是战略相关范围内的组合、需求、供应、财务能力和风险缺口。  
如果系统没有 24-36 个月滚动展望期，就只能看到当前状态或短期战术情景，无法回答高管真正关心的问题：未来增长是否被能力支撑、现金是否足够、产品组合是否合理、关键资源何时形成缺口、战略目标与 DDOM 能力之间是否匹配。

**当前模型缺口**  
- 当前验证数据只有 12 周需求，更偏 DDS&OP / DDOM 战术投影，不足以支撑 Adaptive S&OP 战略层。
- 页面没有按月展示 24-36 个月的 Portfolio、Demand、Supply、Financial projection。
- 没有未来缺口视图，例如 Revenue Gap、Capacity Gap、Working Capital Gap、Service Gap、Innovation Gap。
- 当前滑块情景是短期事件模拟，没有区分战略 Horizon 与战术 Relevant Range。

**拟修改方向**  
- 增加 24-36 个月滚动 Horizon，战略层按月展示，不按 SKU/日/周陷入细节。
- Adaptive S&OP 页面增加月度趋势表：产品族需求、供应能力、收入、毛利、库存/现金占用。
- 增加 Gap Management 视图：目标 vs 当前计划 vs 情景结果。
- 将 DDS&OP 的 tactical range 与 Adaptive S&OP 的 strategic horizon 明确分层：DDS&OP 做参数投影和近期模型重构，Adaptive S&OP 做长期目标、能力和财务取舍。
- 高管视图默认按产品族/资源族/财务科目汇总，避免陷入 SKU 明细；SKU 明细只作为 drill-down。

**确认状态**  
待确认。
