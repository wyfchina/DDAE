# DDAE Development Handoff

Generated: 2026-07-13
Purpose: continue DDAE / DDS&OP development on a new computer.

## Current Repository State

Main repository:
- Path on current machine: `D:\Documents\DDAE`
- Current branch: `main`
- Current commit: `675a035 Add ProductDemo public demo integration`
- Remote status at handoff time: `main` is synced with `origin/main`; working tree was clean after push.

Important sibling repository:
- Network Structure Scoring product line path: `D:\Documents\DDAE-NetworkStructure`
- Product-line branch: `codex/network-structure-product-line`
- Last known commit: `78db3cd Defer candidate combination UI in network product`

Important contract repository:
- Contract source of truth: `D:\Documents\DDAE_INTERFACE_CONTRACT`
- Rule: all interface fields, states, error codes, ACK behavior, examples, and acceptance criteria must come from this directory only. Do not reconstruct contract fields from chat memory.

## Current DDS&OP Functional Boundary

DDS&OP in DDAE is a governance, scenario simulation, and white-box recalculation layer. It is not the DDOM execution engine and does not own executable routing, executable operation duration, production resource calendars, work order execution state, supplier shipment execution, QMS release authority, or WMS inventory authority.

Current DDS&OP responsibilities:
- Run non-persistent scenario previews and compare baseline vs scenario.
- Recalculate preview results through white-box DDS&OP services rather than accepting optimizer output directly.
- Summarize buffer trends, RCCP and constraints, supplier demand, product-family KPI aggregation, and exception SKU signals.
- Save scenario records only through existing scenario-run persistence, where the server recalculates the preview result.
- Generate DDOM main-setting proposals from preview evidence, but not auto-approve or auto-effective them.
- Publish or display DDS&OP governance intent through contract-shaped outputs, especially public demo and runtime planning input flows.
- Consume SDBR feedback as review/governance context only; feedback must not mutate approved DDAE master settings.

Non-responsibilities / boundary guards:
- Do not turn DDS&OP scenarios into executable production schedules.
- Do not let Gurobi / OR-Tools generate a DDMRP plan or bypass DDMRP logic.
- Do not auto-save, auto-approve, or auto-adopt optimization/recommendation results.
- Do not interpret public demo, fixture, or reviewed evidence as ProductionValidated or Business Golden Loop readiness.

## Current DDS&OP UI Shape

The implemented UI still has the existing detailed workbench sections:
- 总览
- 产品族看板
- 数据准备
- 异常识别
- 场景运行
- 方案比较
- 缓冲 / 库存趋势
- RCCP 与约束
- 供应商需求
- 场景留痕
- 主设置治理
- 白盒追踪
- 公开演示闭环, intentionally placed at the bottom of left navigation and DOM/page flow

Important design direction, not yet implemented:
The user wants the future main DDS&OP pages to become exactly these five primary pages:
1. 历史回顾
2. 当前状态基线
3. 未来场景模拟
4. DDOM配置与参数决策
5. 问题协调、行动和决策记录

This is currently a product-design direction, not a completed implementation. When redesigning, preserve the DDS&OP business reasoning order:
`过去发生了什么 -> 当前真实状态是什么 -> 未来会不会击穿保护 -> 要改哪些 DDOM 配置 -> 谁负责行动和决策`.

Figma guidance:
- Figma is only a reference, not a forced reconstruction target.
- If Figma conflicts with business workflow, information density, or AGENTS.md principles, follow business workflow and AGENTS.md.

## Master Settings Governance

Current behavior:
- Preview results can generate master-setting proposals.
- The UI uses Chinese business labels for statuses and avoids exposing raw enum labels such as Current / Proposed / Reviewed / Approved / Effective / Expired directly to users.
- Proposed settings are governance candidates, not automatically approved settings.
- Saving scenarios is still separate from approving/effectuating master settings.
- SDBR feedback and Network candidates must remain review/governance context and must not auto-create approved/effective master-setting changes.

Important concept:
DDOM configuration decisions should distinguish three layers:
- Structure settings: decoupling point, control point, network/routing/flow structure decisions.
- Master parameters: buffer profile, MOQ, order cycle, time buffer, capacity protection policy.
- Temporary scenario adjustments: scenario-only assumptions used for simulation and comparison.

## Network Structure Scoring

Two related lines exist:
1. `codex/网络结构评分-feature` in the DDAE repo: historical validation/integration branch.
2. `codex/network-structure-product-line` in `D:\Documents\DDAE-NetworkStructure`: independent product-line branch.

Current product direction:
- Network Structure Scoring is being separated as an independent product line.
- DDAE main keeps only a necessary entry card / navigation path, not the full network product workspace.
- Network scoring output remains recommendation-only: candidate control points, buffer points, evidence chains, risk and non-adoption explanation.
- Candidates must go back to DDS&OP white-box scenario recalculation before any governance action.
- Candidate action combination selection is useful only when candidate actions are numerous and constrained by budget/capacity/service tradeoffs. The solver should choose combinations; DDAE/DDMRP must still recalculate and verify.

Current network product runtime:
- Default URL: `http://127.0.0.1:5296/network-structure`
- Run command from `D:\Documents\DDAE-NetworkStructure`:
  ```powershell
  & "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run `
    --project src\AdaptiveSopDdsop.NetworkStructure.Host\AdaptiveSopDdsop.NetworkStructure.Host.csproj `
    --urls http://127.0.0.1:5296
  ```

Current DDS&OP entry:
- DDS&OP page has an “打开网络结构评分工作台” button that targets the network product URL.

Important wording rule:
- In the independent Network product, use neutral wording such as “外部治理平台” rather than hard-coding DDS&OP/DDAE as the only consumer.
- Do not claim ProductionValidated or Business Golden Loop readiness.

## SDBR / Contract Interface Dependencies

Contract repo:
- `D:\Documents\DDAE_INTERFACE_CONTRACT`

Hard rule:
- This directory is the only source of truth for all contract fields and validation rules.
- Do not use chat memory to add fields.
- If a contract is insufficient or conflicting, write a review/blocker under `reviews/` for Contract Agent adjudication rather than inventing fields.

Key contract families used recently:
- `ddsop-config-inbound-v1`
- `ddsop-feedback-outbound-v1`
- `ddsop-runtime-planning-input-v1`
- `supplier-execution-evidence-v1`
- `production-inventory-quality-evidence-v1`
- `sdbr-execution-object-evidence-v1`
- `adventureworks-product-demo-v1`

Important public demo paths:
- Frozen public demo data package: `D:\Documents\DDAE_INTERFACE_CONTRACT\data\public-demo-golden-data-v1\`
- DDAE to SDBR config payload handoff:
  `D:\Documents\DDAE_INTERFACE_CONTRACT\data\public-demo-golden-data-v1\handoff\ddae-to-sdbr\ddsop-config-inbound-v1-payload.json`
- DDAE to SDBR runtime planning input package:
  `D:\Documents\DDAE_INTERFACE_CONTRACT\data\public-demo-golden-data-v1\handoff\ddae-to-sdbr\ddsop-runtime-planning-input-v1-package-corrected.json`
- SDBR to DDAE feedback examples:
  `D:\Documents\DDAE_INTERFACE_CONTRACT\data\public-demo-golden-data-v1\handoff\sdbr-to-ddae\planning-run-feedback.json`
  `D:\Documents\DDAE_INTERFACE_CONTRACT\data\public-demo-golden-data-v1\handoff\sdbr-to-ddae\variance-analysis-feedback.json`

Recent implemented DDAE public demo behavior:
- `/api/public-demo-golden-loop` reads the public demo package and feedback artifacts.
- `/api/public-demo-golden-loop/write-payload` writes the DDAE handoff payload.
- `/api/adventureworks-product-demo-v1` exposes the AdventureWorks ProductDemo profile read model.
- Public demo UI now uses sample object IDs from the public demo package, not legacy satellite fixture names such as `PART-FPGA-SPACE / WH-ELEC-QA / EA`.
- Demo labels must remain visible: `DemoFixture`, `ReviewedEvidence`, `Controlled Contract Golden Loop Demo`, `PublicDemoOnly` or `ProductDemoOnly` as applicable.

## Current Main Service Runtime

DDAE Web app:
```powershell
cd D:\Documents\DDAE
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run `
  --project src\AdaptiveSopDdsop.Web\AdaptiveSopDdsop.Web.csproj `
  --urls http://127.0.0.1:5188
```

Open:
```text
http://127.0.0.1:5188
```

Stop service by port if needed:
```powershell
Get-NetTCPConnection -LocalAddress 127.0.0.1 -LocalPort 5188 -State Listen
Stop-Process -Id <OwningProcess> -Force
```

Network product service:
```powershell
cd D:\Documents\DDAE-NetworkStructure
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run `
  --project src\AdaptiveSopDdsop.NetworkStructure.Host\AdaptiveSopDdsop.NetworkStructure.Host.csproj `
  --urls http://127.0.0.1:5296
```

Open:
```text
http://127.0.0.1:5296/network-structure
```

## Verification Commands

DDAE regression tests:
```powershell
cd D:\Documents\DDAE
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj
```

Expected at handoff time:
```text
51 test(s) passed.
```

Build:
```powershell
cd D:\Documents\DDAE
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" build AdaptiveSopDdsop.sln
```

Expected at handoff time:
```text
0 warnings, 0 errors
```

Git sync check:
```powershell
cd D:\Documents\DDAE
git status -sb
git rev-list --left-right --count HEAD...origin/main
```

Expected at handoff time:
```text
## main...origin/main
0 0
```

GitHub access note:
- Direct GitHub access may time out.
- Use one-time proxy when needed:
  ```powershell
  git -c https.proxy=http://127.0.0.1:7890 fetch origin
  git -c https.proxy=http://127.0.0.1:7890 push origin main
  ```
- Avoid setting global proxy unless necessary. If global proxy is set, remember to unset it afterwards.

## Important Files / Artifacts To Inspect

Code and app:
- `D:\Documents\DDAE\src\AdaptiveSopDdsop.Web\Program.cs`
- `D:\Documents\DDAE\src\AdaptiveSopDdsop.Web\Pages\Index.cshtml`
- `D:\Documents\DDAE\src\AdaptiveSopDdsop.Web\wwwroot\js\app.js`
- `D:\Documents\DDAE\src\AdaptiveSopDdsop.Web\Domain\PublicDemoGoldenLoopService.cs`
- `D:\Documents\DDAE\src\AdaptiveSopDdsop.Web\Domain\AdventureWorksProductDemoProfileService.cs`
- `D:\Documents\DDAE\tests\AdaptiveSopDdsop.Tests\Program.cs`
- `D:\Documents\DDAE\runme.md`

Docs and materials:
- `D:\Documents\DDAE\docs\phase-1-white-box-plan-run-spec.md`
- `D:\Documents\DDAE\docs\ddsop-main-minimal-merge-package.md`
- `D:\Documents\DDAE\docs\网络结构评分规则说明.docx`
- `D:\Documents\DDAE\material\DDSOP-网络结构评分-当前边界总览.png`
- `D:\Documents\DDAE\material\DDSOP-网络结构评分-数据适配与白盒回算流程草案.png`
- `D:\Documents\DDAE\material\DDSOP-网络结构评分-白盒接口与组合选择边界.png`
- `D:\Documents\DDAE\material\ERP DDS&OP DDOM如何应对插单.png`

## Next Tasks

Highest-priority product design task:
- Redesign DDS&OP main UI into five primary pages:
  1. 历史回顾
  2. 当前状态基线
  3. 未来场景模拟
  4. DDOM配置与参数决策
  5. 问题协调、行动和决策记录
- Do not implement this until a clear design/spec is approved.
- Treat Figma as reference only.

DDS&OP design details to preserve:
- Historical review should cover the past cumulative lead time, not merely last month.
- Current baseline must freeze inventory, in-transit, backlog, WIP, supplier commitments, resource availability, and current DDOM parameter version.
- Future simulation must separate external scenario assumptions from response/configuration actions.
- Time buffer governance is tactical/aggregate: control-point/product-family/material-family trends over CLT, not daily dispatch control.
- Capacity protection must show theoretical/standard/demonstrated/planned available capacity, committed load, protective capacity, consumed protection, and loss reasons.
- Control point governance should consume evidence from inventory, time buffer, capacity protection, supply, and network scoring; it should not duplicate/recalculate all those modules.
- Coordination/action ledger should record owner, action, due date, decision, escalation, and next DDS&OP verification point.

Contract / SDBR next-work pattern:
- Follow Coordination Agent / Contract Agent NEXT_ACTIONS files when contract work resumes.
- Only write requested reports or implement strictly scoped code when explicitly dispatched.
- Do not modify contract schemas/examples/tests/changelog unless the dispatch explicitly authorizes it.

Network scoring next-work pattern:
- Continue product-line work in `D:\Documents\DDAE-NetworkStructure` / `codex/network-structure-product-line`.
- Keep network product independent and neutral.
- Keep scenario validation as placeholder until a formal DDS&OP <-> Network contract is defined.
- Candidate actions remain recommendation-only and require external white-box recalculation before governance.

Potential future contract line:
- Create a dedicated DDS&OP <-> Network Structure Scoring protocol/contract line, similar to the SDBR/DDS&OP contract workflow, if the user decides to formalize cross-product communication.

## Suggested Skills For Next Agent

Use these skills when appropriate:
- `superpowers:brainstorming` for redesigning the five-page DDS&OP workflow before any implementation.
- `superpowers:writing-plans` once a UI/workflow design is approved and needs an implementation plan.
- `superpowers:test-driven-development` for scoped implementation changes.
- `superpowers:systematic-debugging` when UI/API/test behavior is unexpected.
- `superpowers:verification-before-completion` before claiming implementation success, committing, or pushing.
- `figma:figma-generate-diagram` only when the user asks for diagrams; remember Figma is reference, not a forced implementation target.
- `simio-integration-guide` only for Simio model/data integration discussions; do not mix Simio work into DDS&OP unless explicitly requested.
- `handoff` again if transferring to another machine/thread.

## Sensitive Information

No secrets, API keys, passwords, or personal credentials are included in this handoff. Repository paths are local machine paths and may need to be recreated on the new computer.
