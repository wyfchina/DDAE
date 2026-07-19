import { readFileSync } from "node:fs";
import { resolve } from "node:path";

const root = resolve(process.argv[2] || process.cwd());
const page = readFileSync(resolve(root, "src/AdaptiveSopDdsop.Web/Pages/Index.cshtml"), "utf8");
const script = readFileSync(resolve(root, "src/AdaptiveSopDdsop.Web/wwwroot/js/app.js"), "utf8");

function expect(value, message) {
  if (!value) throw new Error(message);
}

function body(name, nextName) {
  const start = script.indexOf(`function ${name}`);
  const end = nextName ? script.indexOf(`function ${nextName}`, start) : -1;
  expect(start >= 0 && end > start, `missing bounded function ${name}`);
  return script.slice(start, end);
}

for (const id of new Set([...script.matchAll(/byId\("([^"]+)"\)/g)].map(match => match[1]))) {
  expect(page.includes(`id="${id}"`), `static byId target is missing from Index.cshtml: ${id}`);
}
for (const legacyId of ["save-master-setting-change", "advance-master-setting-status"]) {
  expect(!script.includes(legacyId) && !page.includes(legacyId), `legacy single-change control must be removed: ${legacyId}`);
}

const preview = script.slice(
  script.indexOf("function renderPreviewResult(result)"),
  script.indexOf("function showScenarioSavePanel"),
);
expect(preview.includes("result.feasibility") && !preview.includes("，未保存"),
  "preview-status must only show backend feasibility, not unsaved state");
expect(preview.includes('feasibility.status === "Blocked"')
  && preview.includes("阻断候选：可保存留痕，但不可选定；可创建协调事项或修订后重算"),
  "blocked previews must describe save-for-evidence without promising DDOM selection");
expect(page.includes("计算范围") && page.includes("措施对象 SKU") && page.includes('id="scenario-scope-summary"') && page.includes('id="preview-candidate-chip"'),
  "calculation scope and action SKU must be distinct in the markup");
expect(script.includes("function renderScenarioScopeSummary") && script.includes("familyFilter:") && script.includes("skuFilter:"),
  "scope summary must retain existing request serialization");
expect(script.includes("function loadDdomPackages") && script.includes("/api/ddom-change-packages"),
  "DDOM packages must reload from persisted API");
const ddomWorkflow = script.slice(script.indexOf("function loadDdomPackages"), script.indexOf("function renderBufferTrend", script.indexOf("function loadDdomPackages")));
expect(!ddomWorkflow.includes("savedFutureComparisons"),
  "new DDOM workflow cannot use transient savedFutureComparisons");
for (const label of ["创建变更包", "提交评审", "运行白盒验证", "标记已评审", "批准", "生效", "失效"]) {
  expect(page.includes(label), `missing explicit DDOM action: ${label}`);
}
expect(script.includes("relatedDdomPackageId") && page.includes('id="coordination-related-ddom-package"'),
  "coordination form must support a DDOM package link");

const savedRuns = body("renderSavedScenarioRuns", "renderScenarioLineage");
expect(savedRuns.includes('item.feasibilityStatus === "Blocked"') && savedRuns.includes("data-select-ddom-run-id") && savedRuns.includes("创建协调事项并修订方案"),
  "blocked saved runs must offer coordination/revision rather than DDOM selection");
expect(savedRuns.includes('item.feasibilityStatus === "Adoptable"') && savedRuns.includes('item.feasibilityStatus === "Reconcile"') && savedRuns.includes("后端可行性缺失/需重新计算"),
  "only backend Adoptable/Reconcile runs may expose DDOM selection; Legacy and unknown values must be recalculated");
expect(savedRuns.includes("hasCompleteFrozenLineage") && savedRuns.includes("item.baselineSnapshotId")
  && savedRuns.includes("item.externalScenarioId") && savedRuns.includes("item.responseId"),
  "DDOM selection must require complete frozen comparison lineage in addition to backend feasibility");
expect(savedRuns.includes("data-revise-blocked-run-id"),
  "blocked saved runs must keep a one-click coordination entry");

const savePanel = body("showScenarioSavePanel", "renderSavedScenarioRuns");
expect(savePanel.includes("saveControls.button.disabled = !physicalInventoryComplete") && !savePanel.includes("feasibilityStatus"),
  "save availability must remain an evidence gate, including for blocked results");
const saveRun = body("saveScenarioRun", "ddomPackageGate");
expect(saveRun.includes('saved.feasibilityStatus === "Blocked"')
  && saveRun.includes("已保存留痕但不可选定"),
  "saving a blocked run must preserve its non-selectable candidate message");

const selection = body("selectScenarioForDdom", "createDdomPackage");
expect(selection.includes("/selection") && selection.includes('status: "Selected"') && selection.includes("await loadSavedScenarioRuns"),
  "selection must be an explicit persisted operation before entering DDOM");

const packageAction = body("ddomPackageAction", "renderBufferTrend");
expect(packageAction.includes('action === "submit" ? "submit"') && packageAction.includes('action === "validate" ? "validate" : "status"') && (packageAction.match(/fetch\(/g) || []).length === 1,
  "each DDOM action must call only its own single endpoint");
expect(packageAction.includes("ddomActionInFlight") && packageAction.includes("try") && packageAction.includes("finally")
  && packageAction.includes("renderDdomPackageActions"),
  "DDOM actions must hold and release an in-flight gate around their request");
for (const [id, action] of [["submit-ddom-package", "submit"], ["validate-ddom-package", "validate"], ["review-ddom-package", "review"], ["approve-ddom-package", "approve"], ["effective-ddom-package", "effective"], ["expire-ddom-package", "expire"]]) {
  expect(script.includes(`byId("${id}").addEventListener("click", () => ddomPackageAction("${action}")`),
    `${id} must only invoke its explicit workflow action`);
}

const packageDetail = body("renderDdomPackageDetail", "loadDdomPackages");
expect(packageDetail.includes("detail.latestValidation") && packageDetail.includes("failureReasons") && packageDetail.includes("validatedBy") && packageDetail.includes("validatedAtUtc") && packageDetail.includes("feasibilityStatus"),
  "DDOM detail must render the exact latest validation DTO fields");
expect(!packageDetail.includes("JSON.stringify(detail.finalParameters") && !packageDetail.includes("proposal.rationale")
  && packageDetail.includes("ddomParameterSummary") && packageDetail.includes("ddomProposalReasonLabel"),
  "business DDOM detail must show localized parameter and action summaries instead of raw JSON or internal rationale codes");
expect(packageDetail.includes("businessEvidenceLabel(item.message)"),
  "DDOM business audit messages must translate stored internal status codes before display");
expect(packageDetail.includes("data-coordinate-ddom-package-id"),
  "failed DDOM validation must expose a one-click coordination entry");
for (const key of ["PackageCreated", "PackageSubmitted", "WhiteBoxRecalculated", "ValidationPassed", "ValidationFailed", "PackageReviewed", "PackageApproved", "PackageEffective", "PackageExpired"]) {
  expect(body("auditEventLabel", "baselineAuditMessage").includes(key), `audit event should be localized: ${key}`);
}
expect(body("traceStageLabel", "adoptionConstraintLabel").includes('Engine: "白盒引擎"') && body("traceStageLabel", "adoptionConstraintLabel").includes('Governance: "治理"') && body("traceStageLabel", "adoptionConstraintLabel").includes('Validation: "验证"'),
  "DDOM audit stage labels must not leak ordinary English codes");
const businessLabels = body("businessEvidenceLabel", "historyEvidenceSummary");
for (const code of ["Draft", "Submitted", "Passed", "Failed", "NotRun", "Adoptable", "Reconcile", "Blocked"]) {
  expect(businessLabels.includes(code), `business evidence localization must cover ${code}`);
}
expect(businessLabels.includes('.replace(/\\brun\\b/gi, "场景运行")'),
  "DDOM audit evidence must localize the ordinary run token");

const packageLoad = body("loadDdomPackages", "loadDdomPackageDetail");
const packageDetailLoad = body("loadDdomPackageDetail", "selectScenarioForDdom");
const runLoad = body("loadSavedScenarioRuns", "loadScenarioRunDetail");
const runDetailLoad = body("loadScenarioRunDetail", "saveScenarioRun");
expect(packageLoad.includes("state.ddomPackages.some") && packageDetailLoad.includes("status === 404") && runLoad.includes("runs.some") && runDetailLoad.includes("status === 404"),
  "empty and stale persisted selections must clear to a safe empty detail state");
expect(packageDetailLoad.includes("ddomDetailRequestGeneration")
  && packageDetailLoad.includes("packageId !== state.selectedDdomPackageId"),
  "DDOM detail loading must ignore an older A response after B becomes selected");

const actions = script.slice(script.indexOf('document.addEventListener("click", event => {', script.indexOf("data-select-ddom-run-id")), script.indexOf('byId("create-ddom-package")'));
expect(actions.includes("openScenarioCoordination") && actions.includes("openDdomValidationCoordination"),
  "scenario and DDOM validation failures must prefill their coordination lineage and reasons");

const coordinationLineage = body("renderCoordinationLineage", "loadCoordinationDetail");
expect(coordinationLineage.includes("relatedDdomPackageId") && coordinationLineage.includes("data-lineage-ddom-package-id") && coordinationLineage.includes("relatedDdomPackageId=${encodeURIComponent(item.relatedDdomPackageId)}") && coordinationLineage.includes("同一关联对象下共有"),
  "package-only coordination lineage must query its package filter and use accurate count wording");
const lineageSelectors = body("configureCoordinationLineageSelectors", "loadCoordinationItems");
expect(lineageSelectors.includes("coordinationRuns") && lineageSelectors.includes("state.savedScenarioRuns || []")
  && lineageSelectors.includes("selectedRuns") && lineageSelectors.includes("item.baselineSnapshotId")
  && lineageSelectors.includes('item.feasibilityStatus === "Adoptable"'),
  "coordination must list every saved run while DDOM source stays limited to selected complete non-blocked lineage");
const coordinationPrefill = body("prefillCoordinationFailure", "openScenarioCoordination");
expect(coordinationPrefill.includes('scenarioSelect.value = ""') && coordinationPrefill.includes('packageSelect.value = ""'),
  "failure prefill must clear stale lineage selections before applying the requested blocked run or package");

const outcome = body("recordCoordinationOutcome", "loadWorkspace");
expect(!outcome.includes("/api/ddom-change-packages") && !outcome.includes("ddomPackageAction"),
  "coordination outcome must remain record-only and not mutate DDOM governance");
console.log("task-6 persisted UI fixture passed");
