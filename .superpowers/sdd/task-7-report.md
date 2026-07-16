# Task 7 Report: Render Historical Buffer Evidence

Base commit: `05dccce130744bd1db1a457bbb06b57ed8bc5c9e`

Commit message: `feat: render historical buffer evidence`

## Delivered

- Replaced placeholder historical-buffer panels with native SVG/DOM evidence views for inventory, historical DDMRP sizing, five-band time buffers, and capacity layers.
- Kept business calculations in the backend. JavaScript reads the Task 5 DTO and performs only pixel scaling, stacking, path construction, selection, and display formatting.
- Added persistent selectors for control point, inventory SKU, sizing snapshot, time-buffer ID, and capacity resource. Valid selections survive 6↔12-month refreshes.
- Inventory history reads the backend red/yellow/green tops, ending on-hand, and net-flow values. Missing evidence splits paths instead of becoming zero.
- Historical DDMRP shows backend sizing lines, the standard 80/120/70 case, green driver, effective weeks, source cutoff, evidence status, and average on-hand.
- Time history renders all five backend bands and a gap-aware abnormal-cost line.
- Capacity history renders committed load plus theoretical, standard, demonstrated, planned-available, and protection-start layers. AIT defaults to upstream protection; HARNESS is labelled `CCR 利用率参照` and never renders protective consumption.
- The source control-point value `关键进口 FPGA 库存控制点` remains unchanged while the UI maps it to `关键进口 FPGA 独立库存控制点`.
- Empty collections and all-missing points show `证据缺失` without producing an empty SVG.
- Frozen-baseline detail reads only `/api/current-baselines/{snapshotId}`; legacy missing lead-time factors show the exact read-only warning and are never substituted with a current candidate.
- Drawer labels now cross one escaping boundary. Malicious parameter and frozen-baseline labels are escaped exactly once.

## Standard Runtime Fixture

The C# harness now:

1. Builds the real six-month Task 5 DTO through `HistoryReviewWorkspaceService`.
2. Serializes it with web/camelCase JSON options.
3. Discovers Node from `NODE_BINARY`, `PATH`, the installed Codex runtime, or standard install locations.
4. Executes `tests/AdaptiveSopDdsop.Tests/Js/history-buffer-renderers.fixture.mjs`.
5. Fails explicitly if Node is absent, times out, exits nonzero, or does not report completion.

The Node fixture compiles the real `app.js` with `vm.Script` and executes six runtime groups:

1. Backend sizing, evidence gaps, five time bands, and capacity layers.
2. Delegated selectors, FPGA source preservation/display mapping, and HARNESS reference-only behavior.
3. Frozen legacy baseline endpoint and exact warning.
4. Empty collections and all-missing points, with no SVG.
5. Malicious drawer labels escaped exactly once.
6. Overall fixture completion against the serialized real DTO.

## TDD Evidence

- Initial renderer RED: the complete harness failed only the newly registered history-renderer test because the renderer state/selectors did not exist.
- Initial renderer GREEN: the implementation brought the complete suite to `142/142`.
- Review RED: after wiring the Node fixture into the standard harness and adding malicious-label assertions, the suite reported `141 PASS / 1 FAIL` on the double-escaped parameter label.
- Review GREEN: passing raw labels into `openWorkspaceDrawer` restored one escaping boundary; the complete suite returned to `142/142`, including all six Node runtime groups.

## Final Verification

```text
dotnet restore src\AdaptiveSopDdsop.Web\AdaptiveSopDdsop.Web.csproj -p:NuGetAudit=false
dotnet restore tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj -p:NuGetAudit=false
Both restores succeeded with projects up to date.
```

```text
dotnet build AdaptiveSopDdsop.sln --no-restore -m:1
Build succeeded.
0 warnings, 0 errors.
```

```text
dotnet run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
142 test(s) passed.
Node renderer fixture: 6/6 groups passed.
```

```text
& .\scripts\verify-protected-boundaries.ps1
PASS 11 whole-file protected boundaries.
Protected CONTRACT, SDBR, Network, validation, trace, and public-demo blocks match baseline 4e39ec5.
```

- `git diff --check`: no whitespace errors; only Git's informational LF→CRLF notices.
- CSS delimiter check: 567 opening and 567 closing braces.
- JavaScript syntax: the standard Node fixture compiled the real `app.js`.
- Frontend-formula gate: historical renderers contain no copied DDMRP sizing or capacity-protection business formulas.
- Independent re-review after fixes: no Critical, Important, or Minor findings; ready to merge.

## Environment Note

No in-app browser session was available in this worker. Runtime DOM behavior was therefore verified through the standard Node fixture using the real serialized Task 5 DTO. This did not block the required gates.
