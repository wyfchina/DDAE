# Task 8 实施报告：五阶段层级导航与哈希路由

## 范围

- 基线提交：`75bdbe8`
- 功能分支：`codex/ddsop-five-stage-workbench`
- 仅修改 `Index.cshtml`、`app.js`、`site.css`、测试入口，并新增本报告。
- 未启动 Web 应用，未读取或修改真实 `app/data` SQLite。
- 未修改 CONTRACT、SDBR、Network 外部仓库、接口端点、DTO、fixtures 或契约测试。

## TDD 证据

1. 先注册并实现以下三个源码回归测试：
   - `TestFiveStageNavigationUsesHierarchicalViewSwitching`
   - `TestWorkspaceNavigationRemovesScrollObserverAndUsesHashState`
   - `TestOnlySelectedStageOrChildViewIsVisible`
2. 首次 RED：原有 111 项全部通过；新增 3 项分别因缺少 5 个层级组、`workspaceRoutes` 和规范 route target 而失败。
3. DOM 与哈希运行时实现后，114/114 GREEN。
4. 为层级、焦点和独立滚动样式补充断言后再次得到预期 RED（缺少 `.nav-stage-group` 样式）；实现 CSS 后恢复 114/114 GREEN。
5. 独立代码审查发现路由切换时全局专注层与详情抽屉可能残留；先补充关闭顺序断言并得到 1 项预期 RED，再在 `applyWorkspaceRoute` 最前恢复/关闭浮层，恢复 114/114 GREEN。

## 实施结果

- 左侧导航改为 5 个一级阶段、22 个二级入口；一级按钮进入该阶段默认视图，并且只展开当前阶段。
- 白盒追踪与公开演示闭环仍是独立验证入口；公开演示闭环保持导航最后一项。
- 显式注册 29 条规范路由；实现解析、格式化、解析别名、导航、应用、展开、激活、面包屑和 `hashchange` 运行时。
- 无哈希、无效哈希和 11 个旧哈希均通过 `replaceState` 规范化，不新增历史记录。
- 每次路由先隐藏 29 个 target，再处理 trace host，最后只显示当前 target；白盒路由只显示其必要 host。
- 路由切换前先恢复被移入全局专注层的面板并关闭详情抽屉，避免浏览器前进/后退或程序化导航留下旧视图浮层。
- 删除运行时 DOM 搬移、`IntersectionObserver`、旧 tab 状态与监听器、阶段 `scrollIntoView`。
- 四个旧 tab 面板已静态成为顶层 route view；新增非伪造结果的时间缓冲占位视图。
- 已保存场景记录移入“行动跟踪”；旧 overview、产品族、数据准备和主设置 DOM 作为无 `hidden` 的嵌入内容保留，所有 renderer ID 唯一。
- DDOM A/B 保护卡按官方边界规则原样相邻保留；B 仅作规范哈希摘要跳转，实际参数治理仍位于参数决策 route。
- 桌面使用固定导航/顶栏与当前视图内部滚动；保留 900px 移动抽屉语义，并补充层级缩进、引导线、激活、悬停、焦点和展开指示。

## 保护与验证

- `dotnet run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore`
  - 114/114 通过。
- `dotnet build AdaptiveSopDdsop.sln --no-restore -m:1`
  - 0 个错误；2 个既有 NU1900 警告（隔离环境无法访问 NuGet 漏洞索引）。
- `scripts\verify-protected-boundaries.ps1 -Baseline 4e39ec5`
  - 全部 PASS，包括 trace、公开演示、Network 元素及受保护 JavaScript。
- `git diff --check`
  - PASS；仅 Git 提示工作区 LF 将按配置转换为 CRLF。
- 严格 UTF-8 解码：4 个修改的代码文件 PASS。
- DOM：29 个 route target 唯一；全页 264 个 DOM ID 无重复。
- 导航：22 个二级标题的数量、顺序和不超过 6 个字符均 PASS。
- 路由 registry：29 个显式条目 PASS。
- 外部仓库状态未变：
  - `DDAE_INTERFACE_CONTRACT` 仍仅有既存未跟踪交接文档。
  - `DDAE-NetworkStructure` 保持干净。
