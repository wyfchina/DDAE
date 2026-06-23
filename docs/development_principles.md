# DDAE Development Principles

本文档记录本项目后续开发必须遵守的工程原则。它补充需求规格与实现计划，用于避免原则只存在于对话中。

## 1. 测试协作原则

当前项目采用以下测试协作方式：

> 人负责高价值测试设计，机器负责高频验证。

含义：

- 高价值测试优先覆盖业务规则、计算引擎、计划链路、权限边界、财务/产能/供应链风险和回归高风险区域。
- 高频验证交给本地命令执行，包括测试项目、构建、API 检查和必要的浏览器检查。
- 不为每个低风险微调机械展开完整 TDD 叙述，避免把开发注意力和上下文消耗在低价值过程上。
- UI 文案、轻量样式、简单布局调整可以用较轻的验证方式；计划引擎、RCCP、供应需求、缓冲投影、场景审批等核心逻辑必须有明确测试保护。
- 每次重要实现后，最终状态必须能用可重复命令验证。

推荐验证命令：

```powershell
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" build AdaptiveSopDdsop.sln
```

## 2. 开发目录原则

- `D:\Documents\DDAE` 是唯一开发目录。
- `D:\BaiduSyncdisk\日常资料\Python\L2L4` 只作为参考资料源。
- 后续代码、测试、规格、计划和状态记录均应写入 `D:\Documents\DDAE`。

## 3. 架构原则

- 场景推演和优化推荐必须分层：推演引擎负责解释某个场景会发生什么，优化引擎负责推荐值得比较的方案。
- 后续优化能力必须通过 Solver Adapter 接入，例如 `IOptimizationSolver`。
- 第一版优化求解器默认 OR-Tools；Gurobi 仅作为未来可选高级求解器，不在业务服务或 UI 中写死具体求解器。
- Solver Adapter 的实现必须保留可替换性，避免产品早期被商业许可、部署方式或单一求解器 API 绑定。

## 4. 数据环境原则

当前系统使用内存 seed data：`SeedData.Create()` 生成验证数据，`SeedScenarioWorkspaceDataSource` 通过 `IScenarioWorkspaceDataSource` 提供 Scenario Run Workspace 数据。当前还没有数据库持久化。

后续必须遵守：

- 测试系统数据与生产系统数据必须分离，不能共用同一份物理数据源、数据库 schema 或导入目录。
- 测试数据可以使用 seed data、fixture、CSV/Excel 样例或测试数据库；生产数据必须来自受控生产数据源。
- 业务逻辑只能有一套。计划引擎、场景推演、RCCP、缓冲投影、供应需求、guardrail 判断和 trace 逻辑不得为测试环境复制一套实现。
- 前端不得复制缓冲、RCCP、供应缺口、约束判断等业务公式；前端只负责筛选、展示、交互状态和用户输入组装，计算结果必须来自后端领域服务或明确的 API DTO。
- 测试与生产的差异只能存在于数据源 adapter、配置、连接字符串、权限和环境开关中。
- 所有 UI、API 和计算服务应依赖接口，例如 `IScenarioWorkspaceDataSource`，而不是直接依赖 `SeedData` 或某个数据库表结构。
- 测试 fixture 必须覆盖生产业务规则，但不能把测试用捷径写进业务服务。

## 5. 界面语言原则

- 产品界面默认使用中文。
- 新增页面、按钮、表格列、图表标题、提示信息、空状态、错误信息和业务标签，均应优先使用中文表达。
- 行业通用缩写可以保留英文，例如 DD S&OP、DDS&OP、DDOM、RCCP、MOQ、ADU、FDU、ROI、SKU。
- 技术 trace 或模型字段如保留英文，UI 层应尽量提供中文解释。

## 6. 界面风格原则

后续 DDAE 界面设计以 SDBR 计划员工作台为同源风格参考：

- 参考文件：`D:\Documents\SDBR\sdbr\web\planner-workbench.html`、`D:\Documents\SDBR\sdbr\web\planner-workbench.css`、`D:\Documents\SDBR\sdbr\web\planner-workbench.js`。
- 整体布局采用运营工作台风格：深色左侧导航、白色顶部上下文条、浅灰画布、居中内容工作区。
- 页面应优先使用紧凑的 KPI 条、状态 chip、工具条、筛选器、表格、分页、tab、抽屉和对比面板。
- DDAE 与 SDBR 属于同系列产品，结构和信息密度保持一致，但主色使用深绿/青绿供应链调性；SDBR 可继续使用蓝色计划调性。
- 颜色应克制并服务于业务状态：DDAE 深绿/青绿表示主操作和选中态，绿色/黄色/红色分别表达健康、预警、风险。
- 卡片半径保持 8px 或更小，避免营销式大面积渐变、装饰图形和过大的 hero 文案。
- Scenario Run Workspace 应沿用 SDBR 的工作台密度与信息架构，而不是重新设计成独立视觉风格。

## 7. 状态记录原则

每个阶段必须维护状态清单：

- `Done`：已实现并通过验证。
- `In Progress`：正在实现，已有部分代码或测试。
- `Pending`：明确需要做，但尚未开始。
- `Deferred`：已识别但暂缓，不属于当前阶段。

状态清单必须记录：

- 业务能力。
- 涉及文件。
- 验证方式。
- 未完成项和下一步。
