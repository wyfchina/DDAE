# Adaptive S&OP / DDS&OP 合并需求清单与资料对照

本文件是在 `adaptive_sop_ddsop_question_log.md` 的基础上进行去重、合并和资料对照后的统一需求清单。  
结论：当前 17 条原则之间**没有实质冲突**，主要是层级不同、粒度不同、对象不同；需要合并为一套从 AS&OP 到 DDS&OP 再到 DDOM 的闭环应用架构。

---

## 1. 冲突检查结论

### 1.1 无冲突项

以下看似重复或相近的原则，实际是同一 DDAE 机制的不同层级表达，不冲突：

- **Q001 跨职能月度流程** 与 **Q002 24-36 个月 Horizon**  
  前者定义治理机制，后者定义战略时间尺度。

- **Q001 展示需求/供应/财务计划** 与 **Q007 Adaptive S&OP 不输出 MPS**  
  不冲突。需要展示的是聚合业务计划、财务计划、能力计划和 Master Settings 决策，不是传统 MPS 明细排程。

- **Q005 资源剖面/RRP** 与 **Q016 鼓点资源 Time/Capacity Buffer**  
  不冲突。前者是战略/高层可行性检查，后者是 DDS&OP 对 DDOM 的战术主设置配置。

- **Q008 双向 DDS&OP hub** 与 **Q017 DDOM 执行层回传**  
  不冲突。Q017 是 Q008 中“运营向战术反馈”方向的详细数据要求。

- **Q012 Tactical Exploitation** 与 **Q013 Strategic Recommendation**  
  不冲突。前者处理战术权限内的机会利用，后者处理超出战术权限的升级提案。

### 1.2 需要明确边界的项

- **战略层计划展示**：可以展示 24-36 个月需求、供应、财务、组合和能力缺口，但不能变成远期 SKU/工单级排程。
- **DDS&OP 情景模拟**：可以计算计划订单建议作为 DDOM 执行结果，但 DDS&OP 的核心输出应是 Master Settings 变更建议，而不是 MPS。
- **高管视图**：默认按产品族、区域、资源族、财务科目聚合；SKU/物料级数据仅作为 drill-down 和 DDOM 诊断。

---

## 2. 重复项合并结果

| 合并模块 | 覆盖原问题 | 合并后的核心含义 |
|---|---:|---|
| M01 跨职能 AS&OP 月度决策流程 | Q001 | CEO/C-Level 主导，Portfolio、Demand、Supply、Financial、Reconciliation、DDS&OP、Management Review 形成 Single Game Plan。 |
| M02 战略 Horizon 与缺口管理 | Q002 | 24-36 个月滚动战略展望期，按月管理未来收入、能力、现金、服务、组合与创新缺口。 |
| M03 Portfolio / New Activities | Q003 | NPI/NPD 健康度、生命周期管理、呆滞 SKU 理性化、产品组合对未来业务增长的影响。 |
| M04 财务货币化与 ROI | Q004 | 将需求、供应和 DDOM 情景实时转为 Revenue、Contribution Margin、Cash、ROI 和风险成本。 |
| M05 Supply Feasibility / RRP | Q005 | 用 Resource Profiles/RRP 检查瓶颈资源、关键供应商和资本投资需求，使用红黄绿管理。 |
| M06 DDOM Master Settings Governance | Q006、Q007、Q011、Q016 | 不输出 MPS，改为配置 Decoupling Points、Inventory/Time/Capacity Buffers、DAF、区域调整因子、鼓点资源和生效窗口。 |
| M07 DDS&OP 双向战术 hub | Q008 | 向下接收 AS&OP 战略参数并微调 DDOM，向上基于 DDOM 偏差分析反馈运营弹性和修正建议。 |
| M08 DDOM Performance Feedback / Variance Analysis | Q009、Q010、Q017 | Reliability、Stability、Velocity 的历史健康度、帕累托图、运行图、控制图、Act/Late、Red/Black、真实 ADU 和资源负载。 |
| M09 Tactical Exploitation | Q012 | 发现富余产能、低成本物流、现金流机会，以最小变动成本换取最大流动红利。 |
| M10 Strategic Recommendation | Q013 | 对第三班次、新设备、供应商开发、工程重设计等超战术权限事项形成结构化高层提案。 |
| M11 Strategic Projection / Feasibility Check | Q014、Q015 | 对远期战略情景做 DDOM Tolerance、Pacing Resources、Working Capital、Space、Capacity、Lost ROI、Deviation Cost 和资本业务案例评估。 |

---

## 3. 合并后的应用功能蓝图

### M01：Adaptive S&OP 月度决策工作台

必须包含七步流程：

1. Portfolio / New Activities
2. Demand Plan
3. Supply Plan
4. Financial Plan
5. Integrated Reconciliation
6. DDS&OP
7. Management Review

每一步都应有：

- 输入假设
- 责任部门
- 输出结果
- 红黄绿例外
- 与 Single Game Plan 的连接

### M02：24-36 个月战略 Horizon

战略层默认按月展示：

- 产品族需求
- 收入与贡献毛利
- 关键资源能力
- 关键供应商能力
- 工作资本
- 空间/仓储需求
- 服务水平
- ROI
- Gap：Revenue、Margin、Capacity、Cash、Service、Portfolio、Innovation

### M03：Portfolio / New Activities

需要管理：

- NPI/NPD pipeline
- 阶段门：Concept、Development、Validation、Launch、Ramp-up
- 产品生命周期：Growth、Mature、Decline、End-of-Life
- SKU rationalization：保留、改造、合并、退出、最后采购、清库存
- Portfolio impact：收入、毛利、能力、库存、现金、复杂度

### M04：Financial Plan 与货币化

需要计算：

- Revenue Projection
- Contribution Margin
- Contribution Margin %
- Working Capital
- Cash Flow
- Inventory Investment
- Capacity Cost
- Expedite Cost
- Lost Sales / Lost Contribution Margin
- ROI
- ROI Delta

### M05：Supply Feasibility / RRP

需要包含：

- Resource Profile：产品族需求 × 单位资源需求 = 资源负荷
- Pacing Resources / Drum Resources
- 资源负载红黄绿
- Key Supplier Constraints
- Capital Requirements
- 24-36 个月资源/供应商/资本缺口曲线

### M06：DDOM Master Settings Governance

DDS&OP 应配置和治理：

- Strategic Decoupling Points
- Strategic Control Points
- Inventory Buffer：ADU、DLT、Variability、MOQ、DAF、Red/Yellow/Green
- Time Buffer：Act、Late、保护提前期、恢复目标
- Capacity Buffer：保护资源、保留能力、负载阈值
- DAF / Zone Adjustment Factors
- Known Events Calendar
- Master Settings Change Board：Current、Proposed、Approved、Effective、Expired

关键原则：

> Adaptive S&OP 不下传 MPS；DDS&OP 下传和治理 DDOM Master Settings；DDOM 执行层由 Net Flow Equation 拉动。

### M07：DDS&OP 双向 Hub

向下：

- 接收 AS&OP 的战略增长、组合、服务、现金、能力边界
- 转换为 DDOM Master Settings 调整

向上：

- 接收 DDOM 执行层实际表现
- 形成 Operational Resilience Report
- 向 AS&OP 提供 Strategic Recommendations

必须可视化：

`AS&OP -> DDS&OP -> DDOM -> Variance -> DDS&OP -> AS&OP`

### M08：DDOM Performance Feedback / Variance Analysis

三性指标：

- Reliability：OTD、承诺达成、延期、缺货
- Stability：库存水位、Net Flow 波动、Red/Black 穿透
- Velocity：Flow Index、工单周转、缓冲恢复时间、订单响应周期

图表要求：

- Pareto：根因与损失主因
- Run Chart：趋势
- Control Chart：异常波动
- Heatmap：反复破防点

执行层反馈：

- Stock Buffer Red / Black
- Time Buffer Act / Late
- Capacity Buffer overload
- Demonstrated ADU
- Demonstrated resource load
- Root Cause：Demand、Supply、Capacity、Quality、Master Settings、Execution

### M09：Tactical Exploitation

机会识别：

- 富余产能支持短期促销
- 缓冲过高释放现金
- 低风险场景下由空运改慢运
- 供应商产能/价格窗口
- 高贡献毛利产品优先开拓

收益测算：

- 增量收入
- 增量贡献毛利
- 变动成本
- Flow Index 改善
- 现金流影响
- 服务风险

### M10：Strategic Recommendation

结构化提案字段：

- Recommendation ID
- 类型：Capacity、Capital、Supplier、Engineering、Portfolio、Financial
- 触发信号
- 推荐动作
- 财务影响
- 运营影响
- 决策人
- 状态

典型提案：

- 第三班次
- 新设备
- 新供应商
- 工程重设计
- 产品退出
- 服务承诺调整

### M11：Strategic Projection / Feasibility Check

必须回答：

- 现有 DDOM 是否可直接承接战略增量？
- Buffer Tolerance 压力是否超限？
- Pacing Resources 在 24-36 个月内何时变红？
- Working Capital 需要增加多少？
- Space / warehouse / logistics volume 需要增加多少？
- Capacity 是否产生硬缺口？
- 哪些 Lost ROI / Deviation Cost 正在发生？
- 维持现状、第三班次、新机台、供应商开发、工程重设计的 ROI Delta 和 Flow Delta 是多少？

---

## 4. 对照资料后的覆盖检查

### 4.1 已覆盖的书中关键机制

从 PDF 关键章节对照，当前合并清单已经覆盖：

- DDS&OP 是 AS&OP 与 DDOM 之间的双向战术 hub。
- Master Settings 配置 DDOM，Variance Analysis 反馈给 DDS&OP。
- DDS&OP 需要管理 tactical relevant range。
- DDS&OP 六类核心活动：
  - Tactical Review
  - Tactical Projection
  - Tactical Configuration and Reconciliation
  - Tactical Exploitation
  - Strategic Recommendation
  - Strategic Projection
- AS&OP 七步：
  - Portfolio / New Activities
  - Demand Plan
  - Supply Plan
  - Financial Plan
  - Integrated Reconciliation
  - DDS&OP
  - Management Review
- 软件合规重点：
  - Master Settings Access
  - DDOM Variance Analysis
  - DDOM Simulation Capability
  - capacity and working capital simulation

### 4.2 仍需补充的缺失项

以下内容在用户已提问题中涉及较少，但资料中明确重要，建议纳入后续统一修改：

#### G01：Tactical Relevant Range 的明确定义

资料中 DDS&OP 的战术相关范围通常覆盖：

- 过去一个累计提前期
- 未来一个累计提前期

应用需要明确：

- operational range = DLT
- tactical range = CLT past + CLT future
- strategic range = one CLT future 之后的 24-36 个月

#### G02：DDS&OP 六要素的显式导航

目前我们记录了很多能力，但界面上应显式按六要素组织：

1. Tactical Review
2. Tactical Projection
3. Tactical Configuration and Reconciliation
4. Tactical Exploitation
5. Strategic Recommendation
6. Strategic Projection

#### G03：统计能力指数 Cp / Cpk

资料提到可以用统计能力指数识别缓冲尺寸是否需要调整。  
建议在 Variance Analysis 中增加：

- buffer performance Cp
- buffer performance Cpk
- process capability status

#### G04：DDSM / Skill Buffers

Appendix A 提到 DDS&OP 也可以连接技能缓冲（skill buffers）。  
如果应用要更贴近 DDAE 全模型，可增加：

- 关键技能矩阵
- 技能缓冲红黄绿
- 培训资源分配
- 技能缺口对流动性的影响

这不是第一优先级，但属于资料中的扩展缺口。

#### G05：合规软件能力边界

资料中提到 DDS&OP 软件至少应支持：

- all relevant master settings access
- reports on past stock/time/capacity buffer performance
- statistical run charts
- simulation of capacity and working capital under user-defined scenarios

后续验收应把这些变成明确验收标准。

---

## 5. 建议后的统一修改优先级

### Phase 1：结构纠偏

- 增加 AS&OP 七步工作台
- 增加 DDS&OP 六要素导航
- 增加 24-36 月 Horizon
- 明确不输出 MPS，只输出 Master Settings decisions

### Phase 2：DDOM Master Settings

- Decoupling Point Map
- Inventory / Time / Capacity Buffer Settings
- DAF / Zone Adjustment Factors
- Known Events Calendar
- Drum / Pacing Resource Settings
- Change Board

### Phase 3：战略与财务可行性

- Financial Plan
- Resource Profile / RRP
- Key Supplier Constraints
- Capital Requirements
- Strategic Projection / Feasibility Check
- Strategic Business Case

### Phase 4：DDOM Feedback 与可视化

- Reliability / Stability / Velocity
- Pareto / Run / Control Charts
- Act / Late / Red / Black
- Demonstrated ADU
- Demonstrated Resource Load
- Cp / Cpk

### Phase 5：闭环与决策

- Operational Resilience Report
- Tactical Exploitation
- Strategic Recommendation Board
- Integrated Reconciliation
- Management Review / Single Game Plan

