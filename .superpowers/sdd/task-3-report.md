# Task 3 Report: Historical Evidence Checks

## Delivered

- Added backend `HistoryEvidenceCheck` records and the compatibility tails on `HistoryInventoryPoint` for weekly events, parameter-change reasons, evidence checks, and explicit inventory movements.
- Added the optional `HistoricalDdmrpParameterFact.ChangeReason` snapshot field.
- The history projector now emits independent source-field, continuity, inventory-equation, net-flow-equation, parameter-snapshot, sizing, and demand checks. A point is complete only when all seven checks are complete.
- The earliest annual point requires explicit opening-on-hand evidence; later points reconcile opening stock to the immediately preceding ending stock.
- Historical ADU and projected actual demand use `WeeklyBufferFact.ActualDemand` exclusively. `QualifiedDemand` remains only the net-flow subtraction term.
- Removed history target-NFP derivation; every projected target-NFP value is null.
- Weekly business events come from `ExplicitCause`; parameter-change reasons come only from the matched parameter snapshot.

## Test coverage

- Added a poisoned-opening regression test proving that only the affected week loses evidence and that continuity, inventory, and net-flow checks remain independently observable.
- Added a transition-week test proving weekly event and parameter-change reason are distinct snapshot-backed evidence.
- Added a target-NFP absence test and a qualified-demand poison test proving historical zones do not derive ADU from qualified demand.
- Updated range, rolling-ADU, visual-renderer, and renderer-fixture expectations for the null target-NFP contract.

## Verification

- Release harness: `dotnet run --project .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj -c Release --no-restore` — 236 tests passed.
- Renderer fixture: Codex-bundled Node ran `tests\AdaptiveSopDdsop.Tests\Js\history-buffer-renderers.fixture.mjs` — 9/9 groups passed.
- `git diff --check` is clean.

## Note

The new parameter-change reason is intentionally sourced from the historical parameter snapshot. Existing seed construction still supplies the older compatibility field on weekly facts; it is not used as parameter-change evidence by this projector.

The frontend chart still consumes the compatibility target-NFP field. Task 6 is planned to remove its target series and gap predicate; Task 3 intentionally changes only the named renderer fixture, not frontend production files.

## Follow-up: Seeded V2 parameter-change evidence

- The normal historical seed now records `DDMRP 参数快照更新` on each V2 `HistoricalDdmrpParameterFact`; V1 retains no change reason.
- The transition regression now uses `HistoryReviewWorkspaceService.GetReview(12)` with the unmodified seed source. It proves the V2 week emits the normal weekly event (`无事件`) alongside the distinct, non-empty snapshot reason, and verifies V1 does not invent a reason.
- RED: before the seed change, the Release harness reported `expected DDMRP 参数快照更新, got` for that normal-source transition.
