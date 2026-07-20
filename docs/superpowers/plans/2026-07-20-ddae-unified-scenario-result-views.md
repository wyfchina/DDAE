# DDAE Unified Scenario Result Views Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make one frozen-baseline comparison selection drive the plan comparison, inventory, capacity, supply, and breach views so every new response plan can be inspected consistently.

**Architecture:** Keep `/api/scenario-runs/compare` and all backend calculations unchanged. Add one client-side selected `responseId`, derive the selected `ScenarioComparisonCase`, and route that case's existing `preview.scenario` workspaces into the current renderers; retain the legacy single-preview path when no frozen comparison exists.

**Tech Stack:** ASP.NET Core Razor, vanilla JavaScript, existing CSS, executable Node `vm` fixtures hosted by the custom .NET test runner.

## Global Constraints

- Do not modify `DDAE_INTERFACE_CONTRACT`, SDBR DTOs, ACKs, error codes, JSON shapes, endpoints, fixtures, or contract assertions.
- Do not modify `DDAE-NetworkStructure`.
- Do not add a chart dependency or duplicate DDMRP, RCCP, supply, or breach calculations in the browser.
- Viewing a case must never save, select, approve, publish, or make it effective.
- Evidence missing in the selected case must remain missing; never borrow evidence from another case or substitute zero.
- Preserve the legacy single-preview and initial baseline workspaces when no frozen comparison has been run.

---

### Task 1: Executable regression fixture for the disconnected result chain

**Files:**
- Create: `tests/AdaptiveSopDdsop.Tests/Js/frozen-comparison-views.fixture.mjs`
- Modify: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**
- Consumes: a real `ScenarioComparisonResult` produced by `ScenarioComparisonService` and the public functions defined by `app.js` in a VM/DOM fixture patterned after `future-inventory-flow-charts.fixture.mjs`.
- Produces: executable assertions proving comparison tables, selected-case state, inventory cases, capacity, supply, and breach rendering share one `responseId`.

- [ ] **Step 1: Add a real frozen-comparison fixture and failing assertions**

In `Program.cs`, add `Frozen comparison drives every future result workspace` to the test array. Build the same frozen seed baseline and two response configurations used by `TestScenarioComparisonSeparatesExternalEventsAndResponses`, serialize `{ comparison, request }` with `JsonSerializerDefaults.Web`, and invoke the new Node fixture.

Inside the Node fixture, clone the real DTO and add unique display sentinels to each case before rendering. The assertions must exercise real `app.js` functions:

```js
runtime.context.renderFutureComparison(comparison);
assert.match(runtime.elements.get("multi-scenario-comparison-body").innerHTML, /临时能力/);
assert.doesNotMatch(runtime.elements.get("multi-scenario-comparison-body").innerHTML, /选择候选组合后/);
assert.match(runtime.elements.get("candidate-impact-matrix-body").innerHTML, /RESP-CAPACITY/);
assert.doesNotMatch(runtime.elements.get("candidate-impact-matrix-body").innerHTML, /将在候选组合选择后/);
assert.deepEqual(
  Array.from(runtime.context.futureInventoryCases()).map(item => item.caseId),
  comparison.allCases.map(item => item.responseId),
);
runtime.context.selectFutureComparisonCase("RESP-CAPACITY");
assert.equal(runtime.context.state.futureComparisonSelection.responseId, "RESP-CAPACITY");
assert.equal(runtime.context.state.bufferTrend.fixtureMarker, "RESP");
assert.equal(runtime.context.state.rccp.fixtureMarker, "RESP");
assert.equal(runtime.context.state.constraints.fixtureMarker, "RESP");
assert.equal(runtime.context.state.supplierCollaboration.fixtureMarker, "RESP");
assert.match(runtime.elements.get("future-breach-body").innerHTML, /RESP-BREACH/);
assert.doesNotMatch(runtime.elements.get("future-breach-body").innerHTML, /NO-BREACH/);
```

Register the fixture outcome in `Program.cs` with the existing `RunFutureBufferChartFixture` path; no new test process abstraction is required.

- [ ] **Step 2: Run the fixture and verify RED**

Run:

```powershell
node tests/AdaptiveSopDdsop.Tests/Js/frozen-comparison-views.fixture.mjs <generated-comparison-dto.json>
```

Expected: FAIL because `renderFutureComparison` leaves the two tables empty, `futureInventoryCases()` does not return comparison cases, and `selectFutureComparisonCase` is not defined.

- [ ] **Step 3: Commit the failing test**

```powershell
git add tests/AdaptiveSopDdsop.Tests/Js/frozen-comparison-views.fixture.mjs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "test: expose disconnected scenario result views"
```

---

### Task 2: Unified case state, selectors, and comparison tables

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css` only if the existing `.inline-select` style is insufficient.
- Test: `tests/AdaptiveSopDdsop.Tests/Js/frozen-comparison-views.fixture.mjs`

**Interfaces:**
- Produces: `futureComparisonCases(result)`, `selectedFutureComparisonCase()`, `selectFutureComparisonCase(responseId)`, `renderFutureComparisonSelectors(cases)`, and `renderFutureComparisonTables(result)`.
- Consumes: `ScenarioComparisonResult.allCases`, `ScenarioComparisonCase.responseId/name/preview/feasibility`.

- [ ] **Step 1: Add selector markup assertions and verify RED**

Add a static .NET assertion that `Index.cshtml` contains exactly one selector on plan comparison, capacity, supply, and breach pages plus the existing inventory selector:

```csharp
foreach (var selectorId in new[]
{
    "future-result-case-select", "buffer-case-select", "rccp-case-select",
    "supplier-case-select", "breach-case-select"
})
{
    AssertTrue(markup.Contains($"id=\"{selectorId}\"", StringComparison.Ordinal),
        $"future result view must expose {selectorId}");
}
```

Expected: FAIL for the four new IDs.

- [ ] **Step 2: Add minimal state and markup**

Extend state:

```js
futureComparisonSelection: { responseId: null },
```

Add an `.inline-select` control to the four headings, for example:

```html
<label class="inline-select"><span>查看方案</span><select id="future-result-case-select" data-future-result-case-select><option value="">等待比较</option></select></label>
```

Use the same label and placeholder for capacity, supply, and breach IDs. Keep `buffer-case-select` in its existing evidence bar.

- [ ] **Step 3: Implement selection helpers and selector synchronization**

```js
const futureComparisonSelectorIds = [
  "future-result-case-select", "buffer-case-select", "rccp-case-select",
  "supplier-case-select", "breach-case-select",
];

function futureComparisonCases(result = state.futureComparison) {
  return result?.allCases || (result ? [result.noResponse, ...(result.responseCases || [])].filter(Boolean) : []);
}

function selectedFutureComparisonCase() {
  const cases = futureComparisonCases();
  return cases.find(item => item.responseId === state.futureComparisonSelection.responseId) || cases[0] || null;
}
```

`renderFutureComparisonSelectors(cases)` populates all five selects with the same options and value. A delegated `change` listener calls `selectFutureComparisonCase(event.target.value)` only for these IDs. The existing `future-comparison-save-response-id` remains the explicit save target and is synchronized to the viewed case, but changing a result selector must not invoke any save or selection endpoint.

- [ ] **Step 4: Replace the obsolete table data contract**

Refactor `renderMultiScenarioComparison` to consume `ScenarioComparisonResult`. Compute display-only deltas against `result.noResponse.preview.scenario.metrics`; do not calculate planning outputs. For missing action-cost evidence render `证据缺失`.

The impact matrix excludes `NO_RESPONSE`, uses `responseId` and `name`, and reads feasibility from `case.preview.feasibility`. It must no longer reference `combinationComparisons` or `candidateImpactMatrix`.

Guard the legacy initialization call: `renderScenarioComparison(data)` may clear these tables only while `state.futureComparison` is null, so an ordinary filter refresh cannot erase a completed comparison.

- [ ] **Step 5: Run the fixture and static tests to verify GREEN**

Run the Node fixture and then:

```powershell
dotnet run --project tests/AdaptiveSopDdsop.Tests/AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: the new assertions pass; any remaining failures must concern the still-unimplemented detail-workspace synchronization in Task 3.

- [ ] **Step 6: Commit**

```powershell
git add src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests
git commit -m "feat: unify future scenario case selection"
```

---

### Task 3: Bind the selected case to every detailed result workspace

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- Test: `tests/AdaptiveSopDdsop.Tests/Js/frozen-comparison-views.fixture.mjs`

**Interfaces:**
- Consumes: `selectedFutureComparisonCase()` from Task 2.
- Produces: `renderSelectedFutureComparisonViews()` and a single-case wrapper for existing capacity/time/breach renderers.

- [ ] **Step 1: Verify the executable fixture is RED for detail synchronization**

Expected failing assertions are the marker checks on `state.bufferTrend`, `state.rccp`, `state.supplierCollaboration`, and the filtered breach table.

- [ ] **Step 2: Make comparison cases available to the inventory workbench**

At the start of `futureInventoryCases` return mapped comparison cases when they exist:

```js
const comparisonCases = futureComparisonCases().map(item => ({
  caseId: item.responseId,
  name: item.name,
  bufferTrend: item.preview?.scenario?.bufferTrend,
  inventoryFlow: item.preview?.scenario?.inventoryFlow,
  scenarioMetricEvidence: item.preview?.scenario?.scenarioMetricEvidence || [],
})).filter(item => item.bufferTrend);
if (comparisonCases.length) return comparisonCases;
```

Set `state.futureInventorySelection.caseId` to the selected `responseId` before calling `renderBufferTrendWorkspace`.

- [ ] **Step 3: Render one selected capacity case throughout the page**

`renderSelectedFutureComparisonViews()` must:

```js
const selected = selectedFutureComparisonCase();
const scenario = selected?.preview?.scenario;
state.rccp = scenario.rccp;
state.constraints = scenario.constraints;
renderFutureCapacityProtection({ allCases: [selected] });
renderProductRccp(state.rccp, selected.name);
renderConstraintWorkspace(state.constraints);
renderPreviewBudget(selected.preview);
```

Preserve the selected resource only if it exists in the selected workspace; otherwise allow the existing renderer to choose the first valid resource.

- [ ] **Step 4: Render selected supply and breach evidence**

Set `state.supplierCollaboration = scenario.supplierCollaboration`, preserve a valid supplier when possible, and call `renderSupplierCollaborationWorkspace`.

Extract the breach table body from `renderFutureComparison` into `renderFutureBreaches({ allCases: [selected] })`, and call `renderTimeBufferBreachEvidence({ allCases: [selected] }, state.futureComparisonBaseline)`.

Add this note above the legacy exception area:

```html
<p id="baseline-exception-source-note" class="muted-note">以下异常信号来自当前基线，用于形成场景输入；上方击穿结果来自所选方案。</p>
```

- [ ] **Step 5: Support saved frozen-run detail without a new endpoint**

Change `loadScenarioRunDetail` to accept `{ activateResults = false }`. Startup, refresh, and post-save list reloads keep the default so they update audit detail without replacing an active comparison. Only an explicit click on a saved run calls `loadScenarioRunDetail(runId, { activateResults: true })`.

When `activateResults` is true and the summary has `baselineSnapshotId`, `externalScenarioId`, and `responseId`, create a one-case view context from `detail.result` and render it:

```js
function renderSavedFrozenScenarioResult(detail) {
  const summary = detail?.summary;
  if (!summary?.baselineSnapshotId || !summary?.externalScenarioId || !summary?.responseId) return;
  const savedCase = {
    responseId: summary.responseId,
    name: summary.name,
    externalScenarioId: summary.externalScenarioId,
    preview: detail.result,
    breaches: detail.result.protectionAnalysis?.breaches || [],
    timeBufferProjection: detail.result.protectionAnalysis?.timeBufferProjection || [],
    capacityProtectionProjection: detail.result.protectionAnalysis?.capacityProtectionProjection || [],
  };
  renderFutureComparison({
    baselineSnapshotId: summary.baselineSnapshotId,
    baselineSnapshotNumber: summary.baselineSnapshotId,
    noResponse: savedCase,
    responseCases: [],
    allCases: [savedCase],
  });
}
```

Call it only after all four detail/audit/lineage responses pass the existing stale-request guard. Do not treat `preview.baseline` as the saved no-response case; the selector contains only the saved response case. Do not write the saved result into `state.preview`.

- [ ] **Step 6: Run the executable fixture and verify GREEN**

Expected: both comparison tables contain response rows; all five selectors share the same value; inventory, RCCP, supply, and breach marker assertions match the selected case.

- [ ] **Step 7: Commit**

```powershell
git add src/AdaptiveSopDdsop.Web tests/AdaptiveSopDdsop.Tests
git commit -m "fix: bind scenario results across future workspaces"
```

---

### Task 4: Full verification and protected-boundary audit

**Files:**
- Verify only; modify implementation or tests only when an observed failure identifies a defect.

**Interfaces:**
- Consumes: completed Tasks 1-3.
- Produces: evidence that the feature works and protected repositories/contracts are untouched.

- [ ] **Step 1: Run syntax and whitespace checks**

```powershell
node --check src/AdaptiveSopDdsop.Web/wwwroot/js/app.js
git diff --check
```

Expected: no output and exit code 0.

- [ ] **Step 2: Run all application tests**

```powershell
dotnet run --project tests/AdaptiveSopDdsop.Tests/AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: all tests pass with no failed test.

- [ ] **Step 3: Build the solution**

```powershell
dotnet build AdaptiveSopDdsop.sln --no-restore -m:1
```

Expected: 0 warnings and 0 errors.

- [ ] **Step 4: Browser regression on the running local app**

Run a frozen comparison with at least two cases, then verify:

- both comparison tables have rows;
- every result-page selector contains the same cases;
- switching to one response updates inventory title/data, capacity case chip and RCCP, supplier detail, and breach rows;
- switching back to no response restores all five views;
- no save, selection, approval, or effective-state request is issued by a view switch.

- [ ] **Step 5: Audit repository boundaries and final diff**

```powershell
git status --short
git diff --name-only HEAD~2..HEAD
rg -n "DDAE_INTERFACE_CONTRACT|SDBR|NetworkStructure" src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/Pages/Index.cshtml
```

Expected: only DDAE UI/test/docs files changed; no protected contract or sibling-repository file changed.

- [ ] **Step 6: Commit any verification-only test adjustment**

Only if verification exposed a genuine missing assertion:

```powershell
git add tests/AdaptiveSopDdsop.Tests
git commit -m "test: cover unified scenario result views"
```
