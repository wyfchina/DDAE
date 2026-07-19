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

const savePanel = body("showScenarioSavePanel", "renderSavedScenarioRuns");
expect(savePanel.includes("saveControls.button.disabled = !physicalInventoryComplete") && !savePanel.includes("feasibilityStatus"),
  "save availability must remain an evidence gate, including for blocked results");

const selection = body("selectScenarioForDdom", "createDdomPackage");
expect(selection.includes("/selection") && selection.includes('status: "Selected"') && selection.includes("await loadSavedScenarioRuns"),
  "selection must be an explicit persisted operation before entering DDOM");

const packageAction = body("ddomPackageAction", "renderBufferTrend");
expect(packageAction.includes('action === "submit" ? "submit"') && packageAction.includes('action === "validate" ? "validate" : "status"') && (packageAction.match(/fetch\(/g) || []).length === 1,
  "each DDOM action must call only its own single endpoint");
for (const [id, action] of [["submit-ddom-package", "submit"], ["validate-ddom-package", "validate"], ["review-ddom-package", "review"], ["approve-ddom-package", "approve"], ["effective-ddom-package", "effective"], ["expire-ddom-package", "expire"]]) {
  expect(script.includes(`byId("${id}").addEventListener("click", () => ddomPackageAction("${action}")`),
    `${id} must only invoke its explicit workflow action`);
}

const packageDetail = body("renderDdomPackageDetail", "loadDdomPackages");
expect(packageDetail.includes("detail.latestValidation") && packageDetail.includes("failureReasons") && packageDetail.includes("validatedBy") && packageDetail.includes("validatedAtUtc") && packageDetail.includes("feasibilityStatus"),
  "DDOM detail must render the exact latest validation DTO fields");
for (const key of ["PackageCreated", "PackageSubmitted", "WhiteBoxRecalculated", "ValidationPassed", "ValidationFailed", "PackageReviewed", "PackageApproved", "PackageEffective", "PackageExpired"]) {
  expect(body("auditEventLabel", "baselineAuditMessage").includes(key), `audit event should be localized: ${key}`);
}
expect(body("traceStageLabel", "adoptionConstraintLabel").includes('Engine: "白盒引擎"') && body("traceStageLabel", "adoptionConstraintLabel").includes('Governance: "治理"'),
  "DDOM audit stage labels must not leak ordinary English codes");

const packageLoad = body("loadDdomPackages", "loadDdomPackageDetail");
const packageDetailLoad = body("loadDdomPackageDetail", "selectScenarioForDdom");
const runLoad = body("loadSavedScenarioRuns", "loadScenarioRunDetail");
const runDetailLoad = body("loadScenarioRunDetail", "saveScenarioRun");
expect(packageLoad.includes("state.ddomPackages.some") && packageDetailLoad.includes("status === 404") && runLoad.includes("runs.some") && runDetailLoad.includes("status === 404"),
  "empty and stale persisted selections must clear to a safe empty detail state");

const coordinationLineage = body("renderCoordinationLineage", "loadCoordinationDetail");
expect(coordinationLineage.includes("relatedDdomPackageId") && coordinationLineage.includes("data-lineage-ddom-package-id"),
  "package-only coordination lineage must display and navigate to its DDOM package");

const outcome = body("recordCoordinationOutcome", "loadWorkspace");
expect(!outcome.includes("/api/ddom-change-packages") && !outcome.includes("ddomPackageAction"),
  "coordination outcome must remain record-only and not mutate DDOM governance");
console.log("task-6 persisted UI fixture passed");
