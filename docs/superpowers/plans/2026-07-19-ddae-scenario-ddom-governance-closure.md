# DDAE Scenario Feasibility and DDOM Governance Closure Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make at least one internal scenario candidate realistically reviewable, persist a selected candidate across refreshes, and require a server-side white-box validation before a DDOM change package can be reviewed, approved, or made effective.

**Architecture:** Calibrate only DDAE DemoFixture inputs and the internal four-week RCCP workload bucket, then add one backend feasibility policy used by previews, persistence, selection, and package validation. Persist selected candidates and immutable DDOM packages in additive SQLite tables; the browser reads those records instead of reconstructing governance state from transient JavaScript comparison objects.

**Tech Stack:** .NET 9 minimal APIs and Razor Pages, C# records/services, Microsoft.Data.Sqlite, plain JavaScript/HTML/CSS, custom console test runner in `tests/AdaptiveSopDdsop.Tests/Program.cs`.

## Global Constraints

- Modify only the DDAE repository.
- Do not modify `DDAE_INTERFACE_CONTRACT`, SDBR DTOs, payloads, status values, ACKs, error codes, fixtures, endpoints, protocol tests, or contract repository files.
- Do not modify `DDAE-NetworkStructure` or consume Network scoring.
- Do not add external imports, connectors, authentication, or file protocols.
- Do not generate an execution schedule; the four-week RCCP workload bucket is an internal aggregate and must be labelled “非执行排程”.
- Do not auto-select, submit, review, approve, make effective, expire, or publish any result.
- Keep the independent white-box and public-demo pages, DOM IDs, buttons, APIs, and behavior unchanged.
- Keep `/api/scenario-runs/preview`, `/api/scenario-runs/compare`, and existing master-setting endpoints backward compatible.
- Make SQLite changes through additive tables or columns only; never delete or rebuild existing business records.
- Preserve FPGA MOQ at exactly `5`.
- A blocked scenario with complete physical inventory evidence remains saveable; evidence missing is the only feasibility-related save blocker.

---

### Task 1: Calibrate DemoFixture and the internal RCCP time bucket

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Data/SeedData.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Data/SeedScenarioWorkspaceDataSource.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/DemandDrivenPlanningEngine.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs`
- Modify: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**
- Preserve `DemandDrivenPlanningEngine.ProjectRoughCutCapacity(...)` public signature.
- Add internal helper `AllocateRccpWorkload(ProjectedReplenishmentOrder order, ResourceRouting routing, int horizonWeeks, int workWindowWeeks = 4)` returning `(int Week, decimal Required)` rows.
- Change `BuildBudgetBenchmarks` to accept `IReadOnlyList<InventoryPosition> inventory`.

- [ ] **Step 1: Register and write failing calibration tests.**

Add test registrations and implementations named:

```csharp
("RCCP spreads projected order workload across four aggregate weeks", TestRccpSpreadsOrderWorkloadAcrossFourWeeks),
("Demo scenario baseline is inside credible feasibility ranges", TestDemoScenarioBaselineFeasibilityRanges),
("Demo scenario templates include reviewable and blocked candidates", TestDemoScenarioTemplatesCoverFeasibilityOutcomes),
("Demo inventory budget derives from frozen stock facts", TestDemoInventoryBudgetUsesFrozenFacts),
```

The first test creates one order for quantity `100`, one routing with `CapacityPerUnit=1`, one resource with weekly capacity `100`, and a four-week horizon. Assert required workload is `25` in weeks 1–4 and totals `100`.

The seed test asserts these exact MOQs:

```csharp
var expected = new Dictionary<string, decimal>
{
    ["SAT-BUS-001"] = 2m, ["SAT-BUS-002"] = 2m, ["SAT-PROP-003"] = 6m,
    ["PAY-EO-101"] = 2m, ["PAY-SAR-102"] = 2m, ["AV-COM-201"] = 12m,
    ["AV-OBC-202"] = 8m, ["AV-FPGA-203"] = 5m, ["TC-MLI-301"] = 30m,
    ["TC-RAD-302"] = 20m, ["MECH-DEP-401"] = 8m, ["CBL-HAR-402"] = 30m
};
```

Assert resource capacities are AIT `270`, TVAC `144`, CLEAN `120`, HARNESS `240`. Assert default preview peak is `<=100`, average load is `>=30 && <=85`, flow is `>=87`, and average inventory is present. Assert at least one built-in template has peak `<=100` and at least one constrained template has peak `>100` or positive supply gap.

- [ ] **Step 2: Run the four new tests and confirm RED.**

Run:

```powershell
dotnet run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: the new four-week allocation, seed values, budget source, and template-outcome assertions fail against the current one-week/large-MOQ implementation.

- [ ] **Step 3: Apply the exact DemoFixture calibration.**

Set the twelve MOQs and four capacities to the exact values in Step 1. Keep current ADU, DLT, variability, routing sequence, protection relationship, inventory facts, and FPGA control point unchanged.

Implement workload allocation as:

```csharp
var bucketCount = Math.Min(workWindowWeeks, horizonWeeks - order.Week + 1);
if (bucketCount <= 0) return Array.Empty<(int Week, decimal Required)>();
var total = order.Quantity * routing.CapacityPerUnit;
var baseShare = decimal.Round(total / bucketCount, 4);
return Enumerable.Range(0, bucketCount)
    .Select(offset => (
        order.Week + offset,
        offset == bucketCount - 1 ? total - baseShare * (bucketCount - 1) : baseShare))
    .ToList();
```

Aggregate these rows by resource/week before dividing by available capacity. Do not change buffer ordering or create operation start dates.

Change budget basis to:

```csharp
var frozenInventoryValue = group.Sum(sku =>
    inventory.Where(item => item.Sku == sku.Sku)
        .Sum(item => (item.OnHand + item.OpenSupply) * sku.UnitCost));
var budgetInventory = decimal.Round(frozenInventoryValue * 1.10m, 0);
```

Change flow health to count all non-red buffer points as protected flow; yellow remains a coordination signal, not an automatic flow failure. Preserve the existing 40–100 display range for legacy compatibility.

- [ ] **Step 4: Align the four internal templates.**

- Prebuild: build in week 3 and protect demand weeks 6–8.
- Capacity relief: add three `CapacityMultiplier` actions for `RES-AIT`, `RES-TVAC`, and `RES-HARNESS`, each covering weeks 4–8 at exactly `1.20`; no action may exceed `1.25`.
- Order policy: set AV-OBC MOQ to `5` and order cycle to `4` days; never multiply MOQ upward.
- Constrained: retain a `0.55` TVAC loss in week 5 and an explicit supply limit so it remains the red example.

- [ ] **Step 5: Run the new tests and the complete runner; confirm GREEN.**

Expected: all calibration tests pass and every pre-existing test either passes unchanged or is updated only where it asserted the superseded DemoFixture numeric values.

- [ ] **Step 6: Commit the calibration.**

```powershell
git add src/AdaptiveSopDdsop.Web/Data/SeedData.cs src/AdaptiveSopDdsop.Web/Data/SeedScenarioWorkspaceDataSource.cs src/AdaptiveSopDdsop.Web/Domain/DemandDrivenPlanningEngine.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "fix: calibrate internal scenario feasibility"
```

---

### Task 2: Move feasibility rules to one backend policy

**Files:**
- Create: `src/AdaptiveSopDdsop.Web/Domain/ScenarioFeasibilityPolicy.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPersistenceService.cs`
- Modify: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**

```csharp
public sealed record ScenarioFeasibilityCheck(
    string Code, string Metric, decimal? Actual, decimal? YellowLimit,
    decimal? RedLimit, string Unit, string Status, string Message);

public sealed record ScenarioFeasibilityAssessment(
    string Status, string Label, bool IsBlocked, string ConstraintMode,
    IReadOnlyList<ScenarioFeasibilityCheck> Checks,
    IReadOnlyList<string> Violations,
    IReadOnlyList<string> CoordinationItems);

public static class ScenarioFeasibilityPolicy
{
    public static ScenarioFeasibilityAssessment Evaluate(
        ScenarioRunPreviewResult result,
        ScenarioWorkspaceDataSet data);
}
```

Add optional `ScenarioFeasibilityAssessment? Feasibility = null` to `ScenarioRunPreviewResult` and optional `string FeasibilityStatus = "Legacy"`, `string CandidateStatus = "Candidate"` to `ScenarioRunSummary`.

- [ ] **Step 1: Add failing policy tests.**

Register tests that prove:

```csharp
foreach (var mode in new[] { "Balanced", "ServiceFirst", "FlowFirst", "CashFirst", "CapacityFirst", "SupplyFirst" })
{
    var assessment = ScenarioFeasibilityPolicy.Evaluate(deepRedResult with
    {
        Request = deepRedResult.Request with { AdoptionConstraintMode = mode }
    }, data);
    AssertTrue(assessment.IsBlocked, $"{mode} must not bypass a hard red line");
}
```

Also assert:

- evidence missing is blocked;
- peak `100` is not red and `100.1` is red;
- supply gap ratio `15%` is not red and `>15%` is red;
- inventory increase `12%` is not red and `>12%` is red;
- three consecutive red weeks are not red by duration alone and four are red;
- changing priority mode changes check ordering/summary but not the hard result;
- a blocked, evidence-complete preview can still be persisted.

- [ ] **Step 2: Run the policy tests and confirm RED.**

Expected: types/policy do not exist, previews have no backend assessment, and SupplyFirst bypasses hard red checks in current JavaScript behavior.

- [ ] **Step 3: Implement common hard and yellow checks.**

Use these exact red/yellow values:

| Check | Yellow | Red |
| --- | ---: | ---: |
| service target/baseline loss | 1 pp | 3 pp |
| peak capacity load | 85% | 100% |
| supply gap / required supply | 5% | 15% |
| average inventory increase vs baseline | 5% | 12% |
| maximum consecutive red weeks | 1 | 3 |

Flow target gap is a coordination check only. Compute evidence completeness with `InventoryFlowEvidenceValidator`. Compute supply denominator from `Scenario.SupplierCapacity.Sum(RequiredQuantity)`. Return `Blocked` if any check is red, `Reconcile` if none are red and any is yellow, otherwise `Adoptable`.

Order checks by the selected constraint mode, but always include every check.

- [ ] **Step 4: Attach the policy to every preview and saved summary.**

In `PreviewInternal`, construct the preview result, evaluate it against the exact scoped `data`, and return `result with { Feasibility = assessment }`. Persist the result unchanged. Populate summary `FeasibilityStatus` from `preview.Feasibility?.Status ?? "Legacy"`; do not use feasibility to reject `Save`.

- [ ] **Step 5: Run policy and persistence tests; confirm GREEN.**

Expected: all six priority modes enforce the same hard red checks, and red/evidence-complete saves still return `Saved / NotSubmitted`.

- [ ] **Step 6: Commit the backend policy.**

```powershell
git add src/AdaptiveSopDdsop.Web/Domain/ScenarioFeasibilityPolicy.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPreviewService.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPersistenceService.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: centralize scenario feasibility policy"
```

---

### Task 3: Persist and audit candidate selection

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPersistenceService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Program.cs`
- Modify: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**

```csharp
public sealed record ScenarioCandidateSelectionRequest(string Status, string? UpdatedBy, string? Note);
public sealed record ScenarioCandidateSelectionResponse(ScenarioRunSummary Summary, bool IsPersisted);

public ScenarioCandidateSelectionResponse UpdateCandidateStatus(
    string runId,
    ScenarioCandidateSelectionRequest request);
```

- [ ] **Step 1: Add failing selection tests.**

Cover these cases with a temporary SQLite database:

- a `Saved` frozen comparison with feasibility `Adoptable` or `Reconcile` can move `Candidate → Selected`;
- a `Blocked` run cannot be selected but remains saved;
- a run missing baseline/external/response lineage cannot be selected;
- selecting another run in the same baseline/external scenario changes the former selected run to `Superseded`;
- selection survives service reconstruction and appends `CandidateSelected`/`CandidateSuperseded` audit events;
- `Selected → Withdrawn` is allowed; direct `Candidate → Superseded` is rejected.

- [ ] **Step 2: Run the selection tests and confirm RED.**

Expected: no selection columns, method, API, or audit events exist.

- [ ] **Step 3: Add columns and transaction-safe state changes.**

Use additive columns:

```sql
candidate_status TEXT NOT NULL DEFAULT 'Candidate',
feasibility_status TEXT NOT NULL DEFAULT 'Legacy',
selected_by TEXT NULL,
selected_at_utc TEXT NULL,
selection_note TEXT NULL
```

When selecting, update prior selected siblings and append their audit events in the same transaction. Never alter `request_json` or `result_json`.

- [ ] **Step 4: Add the internal API.**

```csharp
app.MapPost("/api/scenario-runs/{runId}/selection", ...)
```

Return 404 for unknown runs, 400 for invalid transitions/lineage, and 409 when feasibility blocks selection.

- [ ] **Step 5: Run tests and confirm GREEN.**

- [ ] **Step 6: Commit candidate persistence.**

```powershell
git add src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioRunPersistenceService.cs src/AdaptiveSopDdsop.Web/Program.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: persist selected scenario candidates"
```

---

### Task 4: Add immutable DDOM change packages and validation gate

**Files:**
- Create: `src/AdaptiveSopDdsop.Web/Domain/DdomChangePackageService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/MasterSettingsGovernanceService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Program.cs`
- Modify: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**

```csharp
public sealed record DdomChangePackageCreateRequest(
    string SourceScenarioRunId, string Name, string? Description,
    string? CreatedBy, GovernanceDecisionContext GovernanceContext);
public sealed record DdomPackageActionRequest(string? UpdatedBy, string? Note);
public sealed record DdomPackageStatusRequest(string Status, string? UpdatedBy, string? Note);
public sealed record DdomChangePackageSummary(
    string PackageId, string PackageNumber, string Name,
    string SourceBaselineId, string SourceScenarioRunId,
    string ExternalScenarioId, string ResponseId,
    string Status, string ValidationStatus, string FeasibilityStatus,
    string Owner, string Approver, string CreatedBy, string CreatedAtUtc,
    string? ValidatedAtUtc);
```

`DdomChangePackageService` must expose `Create`, `List`, `GetDetail`, `GetAuditEvents`, `Submit`, `Validate`, and `UpdateStatus`.

- [ ] **Step 1: Add failing package persistence and gate tests.**

Tests must prove:

- only a selected, saved, frozen-comparison run can create a package;
- package lines are all proposals regenerated from the stored run, never supplied by the browser;
- package source baseline/external scenario/response match the run;
- package final request and parameters round-trip after service reconstruction;
- create is `Draft/NotRun`, submit is explicit `Draft→Submitted`;
- validation before submit is rejected;
- validation reloads the frozen baseline and calls backend preview; client result is not accepted;
- latest `Blocked` validation writes evidence but prevents `Submitted→Reviewed`;
- latest `Adoptable` or `Reconcile` validation stores `Passed` and permits explicit review;
- configured approver is required for `Reviewed→Approved`;
- `Approved→Effective` requires effective date, review date, rollback condition, and a still-matching input fingerprint;
- no operation automatically advances the next status;
- audit sequence contains create, submit, validate, review, approve, and effective events only after their explicit calls.

- [ ] **Step 2: Run package tests and confirm RED.**

Expected: service, DTOs, tables, and endpoints do not exist.

- [ ] **Step 3: Add package DTOs and proposal regeneration by saved run ID.**

Add `MasterSettingsGovernanceService.ProposeFromSavedRun(string runId, CurrentBaselineSnapshot baseline, GovernanceDecisionContext context)`. It must read `IScenarioRunLineageReader.GetDetail`, validate saved lineage, run `PreviewAgainstFrozenBaseline`, and call the existing private proposal builder. It must not accept a browser-supplied comparison/result.

- [ ] **Step 4: Implement additive package tables.**

Create exactly:

```sql
ddom_change_packages
ddom_change_package_lines
ddom_change_package_validations
ddom_change_package_audit_events
```

Store `final_request_json`, `final_parameters_json`, `proposal_json`, and a SHA-256 lowercase hex fingerprint of the canonical web-default JSON for the final request. Use transactions for package header+lines and for validation+audit+package-head update.

- [ ] **Step 5: Implement the explicit state machine.**

Allowed package transitions:

```csharp
Draft -> Submitted
Submitted -> Reviewed
Reviewed -> Approved
Approved -> Effective
Effective -> Expired
```

`Submit` handles only `Draft→Submitted`. `UpdateStatus` handles the remaining transitions and applies the validation/approver/effective-data gates from the design.

- [ ] **Step 6: Register service and internal APIs.**

Add the eight `/api/ddom-change-packages` endpoints from the design. Map argument errors to 400, missing IDs to 404, and gate/state conflicts to 409.

- [ ] **Step 7: Run package tests and complete runner; confirm GREEN.**

- [ ] **Step 8: Commit packages.**

```powershell
git add src/AdaptiveSopDdsop.Web/Domain/DdomChangePackageService.cs src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs src/AdaptiveSopDdsop.Web/Domain/MasterSettingsGovernanceService.cs src/AdaptiveSopDdsop.Web/Program.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: gate DDOM packages with white-box validation"
```

---

### Task 5: Link DDOM packages to actions and baseline lineage

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Domain/CoordinationLedgerService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Domain/BaselineLineageQueryService.cs`
- Modify: `src/AdaptiveSopDdsop.Web/Program.cs`
- Modify: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**

Add optional `RelatedDdomPackageId` to coordination create/item records and optional package list to baseline lineage result without removing existing `RelatedMasterSettingChangeId`.

- [ ] **Step 1: Add failing lineage tests.**

Assert a coordination item can reference a DDOM package, list filters use parameterized equality on `related_ddom_package_id`, package-linked outcomes do not alter package status, and frozen-baseline references return all linked packages beyond public list limits.

- [ ] **Step 2: Run lineage tests and confirm RED.**

- [ ] **Step 3: Add the SQLite column, index, DTO property, list filter, and baseline query.**

Use:

```sql
ALTER TABLE coordination_items ADD COLUMN related_ddom_package_id TEXT NULL;
CREATE INDEX IF NOT EXISTS ix_coordination_items_ddom_package
ON coordination_items(related_ddom_package_id);
```

Do not remove or reinterpret old single-change links.

- [ ] **Step 4: Run tests and confirm GREEN.**

- [ ] **Step 5: Commit lineage.**

```powershell
git add src/AdaptiveSopDdsop.Web/Domain/CoordinationLedgerService.cs src/AdaptiveSopDdsop.Web/Domain/BaselineLineageQueryService.cs src/AdaptiveSopDdsop.Web/Program.cs tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: link DDOM packages to action evidence"
```

---

### Task 6: Replace transient browser governance with the persisted workflow

**Files:**
- Modify: `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- Modify: `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`
- Modify: `tests/AdaptiveSopDdsop.Tests/Program.cs`

**Interfaces:**
- Frontend reads `preview.feasibility`; it does not recompute thresholds.
- Frontend calls selection and package endpoints using saved `runId`/`packageId` only.
- Existing protected validation-page selectors and endpoints remain byte-for-byte unchanged where checked by boundary tests.

- [ ] **Step 1: Add failing UI source and executable fixture tests.**

Tests must assert:

- `preview-status` displays only feasibility and never interpolates `，未保存`;
- `preview-persistence-chip`/`scenario-save-status` separately display unsaved/saved state;
- save remains enabled for blocked, evidence-complete results;
- `evaluateAdoption` no longer owns hard threshold constants or final status;
- all priority modes render the backend checks;
- top filter label contains “计算范围”, action select label contains “措施对象 SKU”, and a scope summary element exists;
- saved run rows display feasibility, candidate, and persistence states separately;
- only green/yellow saved candidates expose “选定为 DDOM 候选”;
- refresh loads run detail from `/api/scenario-runs/{id}` and packages from `/api/ddom-change-packages`; DDOM creation does not require `savedFutureComparisons`;
- DDOM buttons are present with exact Chinese labels: `创建变更包`, `提交评审`, `运行白盒验证`, `标记已评审`, `批准`, `生效`, `失效`;
- blocked candidates offer coordination/revision, not DDOM selection;
- no button automatically calls the next workflow operation;
- public demo and white-box IDs/buttons/API strings remain unchanged.

- [ ] **Step 2: Run UI tests and confirm RED.**

- [ ] **Step 3: Separate feasibility, persistence, and candidate rendering.**

Keep `preview-persistence-chip`. Change `renderPreviewResult` to render `result.feasibility` into `preview-status`. Keep `showScenarioSavePanel` evidence-only disabling. After save, display the real run number and load its candidate state.

- [ ] **Step 4: Clarify scope and action-object language.**

Rename the context bar to “计算范围”; rename “目标 SKU” to “措施对象 SKU”; add `id="scenario-scope-summary"` and render the exact active family/SKU scope. Do not change request serialization: global filters still populate `familyFilter/skuFilter`, while the action SKU populates only prebuild/MOQ/order-cycle parameters.

- [ ] **Step 5: Implement candidate and package workflow controls.**

After a saved green/yellow run, call selection only when the user clicks. Navigate to `#ddom-decision-panel/parameter-decision` only after selection succeeds. On DDOM load, fetch selected runs and packages from SQLite APIs, show the lineage card, and drive each explicit package operation from its own button.

Remove the dependency of the new path on `state.savedFutureComparisons`; retain legacy comparison rendering only for the current unsaved session.

- [ ] **Step 6: Render package validation and audit.**

Show latest validation status, feasibility violations/coordination items, source baseline/run/external scenario/response, package lines, actor/date metadata, and audit events. Disabled buttons must include the missing gate reason in visible text or `title`.

- [ ] **Step 7: Extend coordination form with package association.**

Add an optional DDOM package select and post `relatedDdomPackageId`. Outcome updates remain record-only and must not call package status APIs.

- [ ] **Step 8: Run UI fixture tests and full runner; confirm GREEN.**

- [ ] **Step 9: Commit the UI workflow.**

```powershell
git add src/AdaptiveSopDdsop.Web/Pages/Index.cshtml src/AdaptiveSopDdsop.Web/wwwroot/js/app.js src/AdaptiveSopDdsop.Web/wwwroot/css/site.css tests/AdaptiveSopDdsop.Tests/Program.cs
git commit -m "feat: guide scenarios through DDOM validation"
```

---

### Task 7: Full regression, browser acceptance, and protected-boundary audit

**Files:**
- Verify: all modified DDAE files
- Verify unchanged: protected contract/public-demo files and sibling repositories

- [ ] **Step 1: Run the complete test runner.**

```powershell
dotnet run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
```

Expected: all pre-existing 259 tests and every new test pass.

- [ ] **Step 2: Build with the required command.**

```powershell
dotnet build AdaptiveSopDdsop.sln --no-restore -m:1
```

Expected: 0 warnings, 0 errors.

- [ ] **Step 3: Run static and protected checks.**

```powershell
git diff --check
git status --short
.\scripts\verify-protected-boundaries.ps1 -Baseline d876139
```

Expected: no temporary databases/logs/build outputs are tracked and every protected boundary reports PASS.

- [ ] **Step 4: Prove sibling repositories were not modified.**

```powershell
git -C C:\Users\吴一帆\Documents\DDAE_INTERFACE_CONTRACT status --short
git -C C:\Users\吴一帆\Documents\DDAE-NetworkStructure status --short
```

Record the pre-existing status without modifying either repository.

- [ ] **Step 5: Start the feature worktree on an unused local port and run browser acceptance.**

Verify:

1. a reviewable candidate can be previewed, saved, selected, and recovered after refresh;
2. a constrained candidate is blocked but remains saveable and offers coordination/revision;
3. SupplyFirst cannot bypass a capacity hard red;
4. the selected run creates one DDOM package with complete lineage and lines;
5. package submit and validation are separate explicit actions;
6. a failed validation blocks review; a passed validation permits only the next explicit action;
7. refresh/restart restores run, package, validation, and audit state;
8. a coordination item can reference the package without changing its status;
9. white-box tracking still runs and public-demo refresh/payload controls retain their behavior;
10. no ordinary English workflow code or mojibake is visible in the five business stages.

- [ ] **Step 6: Stop the feature service and rerun tests/build/diff checks.**

Only this second clean result may support completion claims.

- [ ] **Step 7: Request final code review and fix all Critical/Important findings.**

Use `superpowers:requesting-code-review` with the full merge-base diff package, then rerun Steps 1–6 after fixes.

- [ ] **Step 8: Finish the branch without merging or pushing unless the user explicitly asks.**

Use `superpowers:finishing-a-development-branch` and report the branch, worktree, commits, verification evidence, and integration choices.

## Definition of Done

- [ ] Default DemoFixture no longer guarantees `flow=40/peak=2100%/budget over 1270%`.
- [ ] At least one built-in scenario is reviewable and at least one remains intentionally blocked.
- [ ] Backend feasibility is authoritative and all priority modes share hard red lines.
- [ ] Feasibility, persistence, candidate, and governance states are independently visible and persisted.
- [ ] Blocked evidence-complete candidates can be saved but cannot be selected.
- [ ] Selected candidates and DDOM packages survive refresh and service restart.
- [ ] DDOM review/approval/effectiveness requires the latest matching white-box validation and explicit human actions.
- [ ] Actions can reference packages without changing governance automatically.
- [ ] Existing external contracts, Network, public demo, and white-box tracking are unchanged.
- [ ] Tests pass, build has zero warnings/errors, browser acceptance passes, and Git diff contains only intended DDAE changes.
