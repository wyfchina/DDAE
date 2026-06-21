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

## 4. 界面语言原则

- 产品界面默认使用中文。
- 新增页面、按钮、表格列、图表标题、提示信息、空状态、错误信息和业务标签，均应优先使用中文表达。
- 行业通用缩写可以保留英文，例如 DD S&OP、DDS&OP、DDOM、RCCP、MOQ、ADU、FDU、ROI、SKU。
- 技术 trace 或模型字段如保留英文，UI 层应尽量提供中文解释。

## 5. 状态记录原则

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
