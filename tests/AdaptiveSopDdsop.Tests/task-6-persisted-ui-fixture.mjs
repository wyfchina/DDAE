import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import vm from "node:vm";

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

function functionSource(name) {
  const signatures = [`async function ${name}`, `function ${name}`];
  const start = signatures
    .map(signature => script.indexOf(signature))
    .filter(index => index >= 0)
    .sort((left, right) => left - right)[0];
  expect(start !== undefined, `missing executable function ${name}`);
  const bodyStart = script.indexOf("{", start);
  let depth = 0;
  for (let index = bodyStart; index < script.length; index += 1) {
    if (script[index] === "{") depth += 1;
    if (script[index] === "}") depth -= 1;
    if (depth === 0) return script.slice(start, index + 1);
  }
  throw new Error(`missing executable function end ${name}`);
}

function deferred() {
  let resolvePromise;
  const promise = new Promise(resolve => { resolvePromise = resolve; });
  return { promise, resolve: resolvePromise };
}

function jsonResponse(payload, status = 200) {
  return {
    ok: status >= 200 && status < 300,
    status,
    json: async () => payload,
  };
}

async function executableNestedSaveResponseUsesSummary() {
  const elements = new Map([
    ["preview-persistence-chip", { className: "", textContent: "" }],
    ["preview-candidate-chip", { className: "", textContent: "" }],
  ]);
  const loaded = [];
  const context = vm.createContext({
    console,
    state: {
      preview: {
        request: { templateId: "TPL-CONSTRAINED" },
        baseline: { metrics: { averageInventoryValue: 10 } },
        scenario: { metrics: { averageInventoryValue: 11 } },
      },
    },
    saveControls: {
      status: { className: "", textContent: "" },
      name: { value: "阻断方案" },
      description: { value: "回归测试" },
      createdBy: { value: "计划员" },
    },
    byId: id => elements.get(id),
    hasCompletePhysicalInventoryEvidence: () => true,
    isFiniteChartValue: Number.isFinite,
    fetch: async () => jsonResponse({
      runId: "RUN-BLOCKED",
      runNumber: "SR-EXEC-001",
      summary: {
        runId: "RUN-BLOCKED",
        feasibilityStatus: "Blocked",
        baselineSnapshotId: null,
        externalScenarioId: null,
        responseId: null,
      },
    }),
    loadSavedScenarioRuns: async runId => loaded.push(runId),
  });
  vm.runInContext(`${functionSource("saveScenarioRun")}\nglobalThis.__saveScenarioRun = saveScenarioRun;`, context);
  await context.__saveScenarioRun();

  expect(elements.get("preview-candidate-chip").textContent.includes("已保存留痕但不可选定"),
    "nested blocked save response must remain non-selectable");
  expect(loaded.length === 1 && loaded[0] === "RUN-BLOCKED",
    "nested save response must reload the persisted run");
}

async function executableScenarioDetailDropsOutOfOrder404() {
  const oldResponses = Array.from({ length: 4 }, () => deferred());
  let oldIndex = 0;
  const rendered = [];
  const context = vm.createContext({
    console,
    state: {
      savedScenarioRuns: [],
      selectedScenarioRunId: null,
      scenarioDetailRequestGeneration: 0,
    },
    renderSavedScenarioRuns() {},
    renderScenarioAudit: detail => rendered.push(detail?.summary?.runId || null),
    fetch: url => {
      if (url.includes("RUN-A")) return oldResponses[oldIndex++].promise;
      if (url.endsWith("/RUN-B")) return Promise.resolve(jsonResponse({ summary: { runId: "RUN-B" } }));
      return Promise.resolve(jsonResponse([]));
    },
  });
  vm.runInContext(`${functionSource("loadScenarioRunDetail")}\nglobalThis.__loadScenarioRunDetail = loadScenarioRunDetail;`, context);

  const oldLoad = context.__loadScenarioRunDetail("RUN-A");
  await context.__loadScenarioRunDetail("RUN-B");
  oldResponses[0].resolve(jsonResponse(null, 404));
  oldResponses[1].resolve(jsonResponse([]));
  oldResponses[2].resolve(jsonResponse([]));
  oldResponses[3].resolve(jsonResponse([]));
  await oldLoad;

  expect(context.state.selectedScenarioRunId === "RUN-B",
    "an older A 404 must not clear the newer selected run B");
  expect(rendered.length === 1 && rendered[0] === "RUN-B",
    "an older A response must not overwrite the rendered B detail");
}

async function executableDdomCreateHasInFlightGate() {
  const createResponse = deferred();
  const elements = new Map([
    ["ddom-source-run", { value: "RUN-SELECTED" }],
    ["ddom-package-name", { value: "并发门禁测试" }],
    ["create-ddom-package", { disabled: false, title: "" }],
  ]);
  let fetchCount = 0;
  const loaded = [];
  const context = vm.createContext({
    console,
    state: { ddomCreateInFlight: false },
    byId: id => elements.get(id),
    governanceDecisionContext: () => ({ owner: "运营经理" }),
    fetch: () => {
      fetchCount += 1;
      return createResponse.promise;
    },
    loadDdomPackages: async packageId => loaded.push(packageId),
  });
  vm.runInContext(`${functionSource("createDdomPackage")}\nglobalThis.__createDdomPackage = createDdomPackage;`, context);

  const first = context.__createDdomPackage();
  const second = context.__createDdomPackage();
  await Promise.resolve();
  expect(fetchCount === 1, "double click must issue only one DDOM create request");
  expect(elements.get("create-ddom-package").disabled === true,
    "DDOM create button must stay disabled while its request is in flight");
  createResponse.resolve(jsonResponse({ packageId: "PKG-001" }));
  await Promise.all([first, second]);
  expect(context.state.ddomCreateInFlight === false && elements.get("create-ddom-package").disabled === false,
    "DDOM create gate and button must recover after the request settles");
  expect(loaded.length === 1 && loaded[0] === "PKG-001", "created package must reload once");
}

async function executableFailurePrefillClearsStaleMasterSettingChange() {
  const elements = new Map([
    ["coordination-related-scenario", {
      value: "",
      options: [{ value: "" }, { value: "RUN-BLOCKED" }],
    }],
    ["coordination-related-ddom-package", {
      value: "",
      options: [{ value: "" }, { value: "PKG-FAILED" }],
    }],
    ["coordination-related-change", { value: "MSC-STALE" }],
    ["coordination-title", { value: "" }],
    ["coordination-impact-objects", { value: "" }],
    ["coordination-decision-required", { value: "" }],
  ]);
  const context = vm.createContext({
    console,
    state: {
      currentDdomPackageDetail: {
        summary: { packageId: "PKG-FAILED", sourceScenarioRunId: "RUN-BLOCKED" },
        latestValidation: { failureReasons: ["服务门槛未通过"] },
      },
    },
    byId: id => elements.get(id),
    valueOr: (value, fallback) => value ?? fallback,
    configureCoordinationLineageSelectors() {},
    navigateWorkspace() {},
    fetch: async () => jsonResponse({
      result: { feasibility: { checks: [{ status: "Red", message: "产能门槛未通过" }] } },
    }),
  });
  const prefillSource = body("prefillCoordinationFailure", "openScenarioCoordination").replace(/\s*async\s*$/, "");
  vm.runInContext(`${prefillSource}\n${functionSource("openScenarioCoordination")}\n${functionSource("openDdomValidationCoordination")}\nglobalThis.__openScenarioCoordination = openScenarioCoordination;\nglobalThis.__openDdomValidationCoordination = openDdomValidationCoordination;`, context);

  await context.__openScenarioCoordination("RUN-BLOCKED");
  expect(elements.get("coordination-related-change").value === "",
    "scenario failure prefill must clear a stale master-setting change link");

  elements.get("coordination-related-change").value = "MSC-STALE-AGAIN";
  await context.__openDdomValidationCoordination("PKG-FAILED");
  expect(elements.get("coordination-related-change").value === "",
    "DDOM validation failure prefill must clear a stale master-setting change link");
}

async function executableOnlyCandidateRunsExposeDdomSelection() {
  const context = vm.createContext({
    console,
    state: { savedScenarioRuns: [], selectedScenarioRunId: null },
    saveControls: { listBody: { innerHTML: "" } },
    valueOr: (value, fallback) => value ?? fallback,
    escapeHtml: value => String(value),
    statusClass: () => "status-chip",
    statusLabel: value => value,
    percent: value => String(value),
    number: value => String(value),
    emptyRow: message => message,
    configureCoordinationLineageSelectors() {},
  });
  vm.runInContext(`${functionSource("renderSavedScenarioRuns")}\nglobalThis.__renderSavedScenarioRuns = renderSavedScenarioRuns;`, context);
  const common = {
    feasibilityStatus: "Adoptable",
    baselineSnapshotId: "BASE-001",
    externalScenarioId: "EXT-001",
    responseId: "RESP-001",
    createdAtUtc: "2026-07-20T00:00:00Z",
    name: "候选状态回归",
    createdBy: "计划员",
    serviceLevelPercent: 98,
    peakLoadPercent: 90,
    supplyGap: 0,
  };
  context.__renderSavedScenarioRuns([
    { ...common, runId: "RUN-CANDIDATE", runNumber: "SR-001", candidateStatus: "Candidate" },
    { ...common, runId: "RUN-SUPERSEDED", runNumber: "SR-002", candidateStatus: "Superseded" },
    { ...common, runId: "RUN-WITHDRAWN", runNumber: "SR-003", candidateStatus: "Withdrawn" },
  ]);

  const html = context.saveControls.listBody.innerHTML;
  const selectionButtons = html.match(/data-select-ddom-run-id=/g) || [];
  expect(selectionButtons.length === 1 && html.includes('data-select-ddom-run-id="RUN-CANDIDATE"'),
    "only Candidate runs may expose the DDOM selection action");
  expect(!html.includes('data-select-ddom-run-id="RUN-SUPERSEDED"')
    && !html.includes('data-select-ddom-run-id="RUN-WITHDRAWN"'),
    "Superseded and Withdrawn runs must never expose DDOM selection");
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
expect(preview.includes("保存仅用于审计") && preview.includes("仅在冻结比较中保存的方案可人工选定"),
  "ordinary preview must not imply that a non-comparison save can become a DDOM candidate");
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
expect(saveRun.includes('saved.summary.feasibilityStatus === "Blocked"')
  && saveRun.includes("已保存留痕但不可选定")
  && saveRun.includes("仅冻结比较方案可人工选定"),
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
expect(packageDetail.includes("coordinationItems") && packageDetail.includes("待协调事项"),
  "reconcilable DDOM validation must show its persisted coordination items in Chinese");
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
expect(runDetailLoad.includes("scenarioDetailRequestGeneration")
  && runDetailLoad.includes("runId !== state.selectedScenarioRunId")
  && runDetailLoad.indexOf("runId !== state.selectedScenarioRunId") < runDetailLoad.indexOf("status === 404"),
  "scenario detail loading must discard an older A response before its stale 404 can clear B");

const createPackage = body("createDdomPackage", "ddomPackageAction");
expect(createPackage.includes("ddomCreateInFlight") && createPackage.includes('byId("create-ddom-package")')
  && createPackage.includes("try") && createPackage.includes("finally"),
  "DDOM package creation must hold an independent in-flight button gate");

const sourceLabels = body("baselineSourceLabel", "baselineActorLabel");
expect(sourceLabels.includes('"DDAE Internal Operating Fact Set": "DDAE 内部经营事实集"'),
  "internal operating fact source must be localized");
expect(script.includes('Historical closing balances to current baseline')
  && script.includes("历史期末余额衔接至当前基线"),
  "history-to-baseline scope must be localized");
for (const [code, label] of [
  ["ON_HAND", "在手库存"],
  ["INVENTORY_VALUE", "库存金额"],
  ["WORK_IN_PROCESS", "在制品"],
  ["BACKLOG", "积压需求"],
  ["RESOURCE_AVAILABLE_CAPACITY", "资源可用能力"],
  ["ALL", "全部"],
]) {
  expect(script.includes(code) && script.includes(label), `baseline reconciliation code must be localized: ${code}`);
}
expect(businessLabels.includes("已从冻结基线和已保存场景运行进行白盒复算。"),
  "DDOM audit wording must use natural Chinese for the saved scenario run");

const actionsStart = script.indexOf('document.addEventListener("click", event => {', script.indexOf("data-select-ddom-run-id"));
const actions = script.slice(actionsStart, script.indexOf('byId("create-ddom-package").addEventListener', actionsStart));
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
await executableNestedSaveResponseUsesSummary();
await executableScenarioDetailDropsOutOfOrder404();
await executableDdomCreateHasInFlightGate();
await executableOnlyCandidateRunsExposeDdomSelection();
await executableFailurePrefillClearsStaleMasterSettingChange();
console.log("task-6 persisted UI fixture passed");
