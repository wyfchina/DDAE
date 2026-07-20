# Task 3 report — bind scenario results across future workspaces

## RED / GREEN evidence

- RED: the frozen-comparison fixture failed because `renderSelectedFutureComparisonViews` did not exist, the exception workspace had no baseline-source note, and `loadScenarioRunDetail(..., { activateResults: true })` left the live three-case comparison active instead of opening the saved one-case result.
- RED follow-up: a selected response with no `bufferTrend` incorrectly fell back to a different comparison case's inventory evidence.
- GREEN: the fixture now proves the selected response supplies the inventory, lower RCCP, constraints, supplier, budget, capacity, breach, and time-buffer workspaces; missing selected inventory evidence remains missing. It also proves automatic saved-run detail loading leaves live comparison and legacy preview state intact, while an explicit saved-run click opens a read-only one-case comparison without writing `state.preview`.

## Changed files

- `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- `tests/AdaptiveSopDdsop.Tests/Js/frozen-comparison-views.fixture.mjs`

## Verification

- `node --check src/AdaptiveSopDdsop.Web/wwwroot/js/app.js` — pass (Codex bundled Node runtime).
- Custom runner focused fixture — `PASS Frozen comparison drives every future result workspace`.
- Full custom runner — pass; the harness reports `PASS Frozen comparison drives every future result workspace` and no failed tests. `NU1900` warnings are limited to the unavailable NuGet vulnerability feed.
- `git diff --check` — pass (only Git's LF-to-CRLF working-copy notices were emitted).

## Self-review

- Viewing a selected case is renderer-only: no save, selection, approval, publish, or effective-state request is issued.
- Saved result activation creates one outer response case from `detail.summary`, `detail.result`, and `detail.result.protectionAnalysis`; it does not label it as no-response or reuse a live baseline/request.
- Saved activation clears stale comparison request/baseline context and disables comparison save. Automatic audit/detail refreshes do not activate it.
- No endpoint, DTO, SDBR, network-structure, or browser planning calculation was changed.

## Concern

- The environment cannot resolve `node` from PATH; verification uses the bundled absolute Node executable. The .NET runner still executes its fixture successfully via its configured runtime.

## Review follow-up — stale selected-case evidence

- RED: poisoned frozen-case DOM regression checks showed that choosing a case without `bufferTrend` retained the previous case's SKU options and comparison strip, and choosing a case without RCCP/constraints retained its resource load detail chart.
- GREEN: null inventory, RCCP, and constraint render paths now clear titles, selectors, option lists, comparison/detail regions, and selected-resource state while visibly identifying missing selected-case evidence. Valid inventory rendering re-enables the week selector.
- Saved read-only activation now preserves `savedFutureComparisons` metadata; it only clears the request/baseline needed to prevent stale comparison saving.
- Verification: bundled Node syntax checks, the frozen-comparison fixture through the full custom runner, and `git diff --check` passed after the follow-up.
