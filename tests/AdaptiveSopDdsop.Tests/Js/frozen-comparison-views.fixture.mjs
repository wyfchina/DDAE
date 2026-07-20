import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

const fixtureDirectory = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(fixtureDirectory, "..", "..", "..");
const defaultScriptPath = path.join(root, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js");
const defaultPagePath = path.join(root, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml");

function createRuntime(source, scriptPath) {
  const elements = new Map();
  const fetchCalls = [];
  const createElement = (id = "") => {
    if (id && elements.has(id)) return elements.get(id);
    const classes = new Set();
    const listeners = new Map();
    const attributes = new Map();
    const element = {
      id,
      innerHTML: "",
      textContent: "",
      value: "",
      checked: false,
      disabled: false,
      hidden: false,
      dataset: {},
      style: {},
      children: [],
      parentElement: null,
      classList: {
        add: (...names) => names.forEach(name => classes.add(name)),
        remove: (...names) => names.forEach(name => classes.delete(name)),
        contains: name => classes.has(name),
        toggle: (name, force) => {
          const enabled = force === undefined ? !classes.has(name) : Boolean(force);
          if (enabled) classes.add(name); else classes.delete(name);
          return enabled;
        },
      },
      addEventListener(type, handler) {
        listeners.set(type, [...(listeners.get(type) || []), handler]);
      },
      setAttribute(name, value) { attributes.set(name, String(value)); },
      getAttribute(name) { return attributes.get(name) ?? null; },
      removeAttribute(name) { attributes.delete(name); },
      removeEventListener(type, handler) {
        listeners.set(type, (listeners.get(type) || []).filter(item => item !== handler));
      },
      appendChild(child) { element.children.push(child); child.parentElement = element; return child; },
      remove() {},
      focus() {},
      click() { element.dispatchEvent({ type: "click", target: element }); },
      dispatchEvent(event) {
        event.target ??= element;
        event.currentTarget = element;
        event.preventDefault ??= () => { event.defaultPrevented = true; };
        for (const handler of listeners.get(event.type) || []) handler(event);
        return !event.defaultPrevented;
      },
      querySelector: () => null,
      querySelectorAll: () => [],
      closest(selector) {
        const dataMatch = selector.match(/^\[data-([a-z0-9-]+)\]$/i);
        if (!dataMatch) return null;
        const datasetKey = dataMatch[1].replace(/-([a-z])/g, (_match, letter) => letter.toUpperCase());
        return element.dataset[datasetKey] === undefined ? null : element;
      },
      contains: () => false,
      getBoundingClientRect: () => ({ top: 0, left: 0, width: 100, height: 20, bottom: 20, right: 100 }),
    };
    if (id) elements.set(id, element);
    return element;
  };
  const documentListeners = new Map();
  const document = {
    readyState: "complete",
    getElementById: id => createElement(id),
    querySelector: selector => createElement(`selector:${selector}`),
    querySelectorAll: () => [],
    createElement: () => createElement(),
    body: createElement("fixture-body"),
    documentElement: createElement("fixture-html"),
    addEventListener(type, handler) { documentListeners.set(type, [...(documentListeners.get(type) || []), handler]); },
    removeEventListener() {},
    dispatchEvent(event) {
      event.target ??= document;
      event.currentTarget = document;
      event.preventDefault ??= () => { event.defaultPrevented = true; };
      for (const handler of documentListeners.get(event.type) || []) handler(event);
      return !event.defaultPrevented;
    },
  };
  const window = {
    document,
    location: { hash: "", pathname: "/", search: "" },
    history: { replaceState() {}, pushState() {} },
    addEventListener() {},
    removeEventListener() {},
    matchMedia: () => ({ matches: false, addEventListener() {}, removeEventListener() {} }),
    scrollTo() {},
  };
  const context = vm.createContext({
    console, document, window, location: window.location, history: window.history,
    navigator: { clipboard: { writeText: async () => {} } },
    fetch: (...args) => {
      fetchCalls.push(args);
      return new Promise(() => {});
    },
    setTimeout, clearTimeout, setInterval, clearInterval, URL, URLSearchParams,
    TextEncoder, TextDecoder, AbortController, Response, Request, Headers,
    structuredClone, crypto: globalThis.crypto, performance: globalThis.performance,
    requestAnimationFrame: () => 0, cancelAnimationFrame() {},
    getComputedStyle: () => ({ display: "block" }), CSS: { escape: String },
    MutationObserver: class { observe() {} disconnect() {} },
    IntersectionObserver: class { observe() {} disconnect() {} },
  });
  context.globalThis = context;
  new vm.Script(source, { filename: scriptPath }).runInContext(context);
  return { context, document, elements, element: createElement, fetchCalls };
}

function readVm(runtime, expression) {
  return JSON.parse(vm.runInContext(`JSON.stringify(${expression})`, runtime.context));
}

function addDisplaySentinels(comparison) {
  const cases = comparison.allCases;
  cases.forEach((item, index) => {
    const marker = `FROZEN-${item.responseId}-${index}`;
    const capacityBand = ["Red", "Yellow", "Green"][index] || "DeepRed";
    item.name = `${item.name} ${marker}`;
    item.preview.scenario.bufferTrend = { ...item.preview.scenario.bufferTrend, marker };
    item.preview.scenario.rccp = { ...item.preview.scenario.rccp, marker };
    item.preview.scenario.constraints = { ...item.preview.scenario.constraints, marker };
    item.preview.scenario.supplierCollaboration = { ...item.preview.scenario.supplierCollaboration, marker };
    item.preview.scenario.bufferTrend.name = marker;
    item.preview.scenario.rccp.resourceSummaries[0].resourceName = marker;
    item.preview.scenario.constraints.capacitySummaries[0].resourceName = marker;
    item.preview.scenario.supplierCollaboration.summaries[0].supplier = marker;
    item.preview.scenario.budget[0].family = marker;
    item.capacityProtectionProjection = [{
      upstreamResourceCode: marker,
      protectedCcrResourceCode: marker,
      week: 1,
      plannedAvailableCapacity: 1,
      committedLoad: 1,
      measure: { evidenceStatus: "Complete", utilizationPercent: 1, protectionStart: 1, protectionCapacity: 1, consumedProtection: 0, remainingProtection: 1, overload: 0, utilizationBand: capacityBand },
    }];
    item.preview.scenario.plan = {
      ...item.preview.scenario.plan,
      capacityLoads: [{
        relationshipRole: "CcrUtilization",
        resourceName: marker,
        resourceCode: marker,
        week: 1,
        availableCapacity: 1,
        requiredCapacity: 1,
        capacityProtectionMeasure: { utilizationPercent: 1, utilizationBand: capacityBand },
      }],
    };
    item.breaches = [{
      scopeType: "CapacityBuffer", target: `${marker}-BREACH`, evidenceStatus: "Complete",
      isBreached: true, earliestRedWeek: 1, consecutiveRiskWeeks: 1, isUnrecovered: false,
      recoveryWeek: 2, affectedProducts: [marker], primaryCause: marker,
    }, {
      scopeType: "TimeBuffer", target: `${marker}-TIME`, evidenceStatus: "Complete",
      isBreached: true, earliestRedWeek: 1, consecutiveRiskWeeks: 1, isUnrecovered: false,
      recoveryWeek: 2, affectedProducts: [marker], primaryCause: `${marker}-TIME`,
    }];
    const horizonWeeks = Number(item.preview?.request?.horizonWeeks) || 12;
    item.timeBufferProjection = Array.from({ length: horizonWeeks }, (_unused, weekIndex) => ({
      bufferId: `${marker}-TIME`, controlPoint: marker, week: weekIndex + 1, evidenceStatus: "Complete",
      status: "Green", penetrationPercent: 1, cause: marker,
    }));
  });
  return comparison;
}

function collect(failures, label, assertion) {
  try { assertion(); } catch (error) { failures.push(new Error(`${label}: ${error.message}`)); }
}

export async function runFrozenComparisonViewsFixture(payload, scriptPath = defaultScriptPath) {
  assert.ok(payload?.comparison?.allCases?.length >= 3,
    "fixture must receive a real ScenarioComparisonResult with no-response and two response cases");
  assert.equal(payload?.request?.responseOptions?.length, 2,
    "fixture must receive the two real response configurations that produced the comparison");

  const source = await readFile(scriptPath, "utf8");
  const page = await readFile(defaultPagePath, "utf8");
  const comparison = addDisplaySentinels(structuredClone(payload.comparison));
  const runtime = createRuntime(source, scriptPath);
  ["multi-scenario-comparison-body", "candidate-impact-matrix-body", "future-capacity-protection-body", "future-ccr-utilization-body", "future-capacity-utilization-distribution", "future-breach-body", "future-result-case-select", "buffer-case-select", "rccp-case-select", "supplier-case-select", "breach-case-select", "buffer-trend-case-chip", "rccp-resource-summary-body", "constraint-capacity-summary-body", "supplier-summary-body", "budget-comparison-body"]
    .forEach(id => runtime.element(id));
  const failures = [];
  const expectedResponseIds = comparison.allCases.map(item => item.responseId);
  const selectedCase = comparison.allCases.find(item => item.responseId === comparison.responseCases[1].responseId);
  assert.ok(selectedCase, "fixture must resolve the selected response from comparison.allCases");
  const unselectedCases = comparison.allCases.filter(item => item.responseId !== selectedCase.responseId);
  const selectedTimeBreach = selectedCase.breaches.find(item => item.scopeType === "TimeBuffer");
  selectedTimeBreach.evidenceStatus = "EvidenceMissing";
  selectedCase.timeBufferProjection[0].evidenceStatus = "EvidenceMissing";
  const completeTimeCase = comparison.responseCases[0];
  const timeBufferDefinitions = comparison.allCases.map(item => {
    const breach = item.breaches.find(candidate => candidate.scopeType === "TimeBuffer");
    const projection = item.timeBufferProjection[0];
    return {
      bufferId: breach.target,
      controlPoint: projection.controlPoint,
      protectedActivity: `${projection.controlPoint}-ACTIVITY`,
      bufferDays: 1,
      evidenceStatus: "Complete",
    };
  });

  runtime.context.__comparisonFixture = comparison;
  runtime.context.__comparisonBaseline = { payload: { planningInputs: { timeBuffers: timeBufferDefinitions } } };
  vm.runInContext("state.futureComparisonBaseline = __comparisonBaseline; renderFutureComparison(__comparisonFixture);", runtime.context);
  const comparisonMarkup = runtime.elements.get("multi-scenario-comparison-body").innerHTML;
  const matrixMarkup = runtime.elements.get("candidate-impact-matrix-body").innerHTML;
  collect(failures, "comparison table", () => {
    comparison.allCases.forEach(item => assert.ok(comparisonMarkup.includes(item.name), `must include ${item.name}`));
    assert.ok(!comparisonMarkup.includes("选择候选组合后"), "must not retain the candidate-combination placeholder");
  });
  collect(failures, "candidate impact matrix", () => {
    comparison.responseCases.forEach(item => {
      assert.ok(matrixMarkup.includes(item.name), `must include response name ${item.name}`);
      assert.ok(matrixMarkup.includes(item.responseId), `must include response ID ${item.responseId}`);
    });
    assert.ok(!matrixMarkup.includes("将在候选组合选择后"), "must not retain the candidate-combination placeholder");
  });

  runtime.context.__stalePreview = structuredClone(comparison.allCases[0].preview);
  runtime.context.__stalePreview.baseline.caseId = "STALE-BASELINE";
  runtime.context.__stalePreview.scenario.caseId = "STALE-SCENARIO";
  vm.runInContext("state.preview = __stalePreview; state.futureComparison = __comparisonFixture;", runtime.context);
  collect(failures, "future inventory case source", () => assert.deepEqual(
    readVm(runtime, "futureInventoryCases().map(item => item.responseId || item.caseId)"),
    expectedResponseIds,
    "future inventory cases must come from frozen comparison.allCases and exclude state.preview"));
  vm.runInContext("state.futureComparison = null;", runtime.context);
  collect(failures, "legacy inventory preview fallback", () => assert.deepEqual(
    readVm(runtime, "futureInventoryCases().map(item => item.caseId)"),
    ["STALE-BASELINE", "STALE-SCENARIO"],
    "without a frozen comparison, legacy preview baseline/scenario cases must remain available"));
  vm.runInContext("state.futureComparison = __comparisonFixture;", runtime.context);

  vm.runInContext("state.futureComparisonSelection = { responseId: __comparisonFixture.responseCases[1].responseId };", runtime.context);
  vm.runInContext("renderFutureCapacityProtection(__comparisonFixture); renderFutureComparison(__comparisonFixture);", runtime.context);
  collect(failures, "selected capacity workspace", () => {
    const capacityMarkup = runtime.elements.get("future-capacity-protection-body").innerHTML;
    const ccrMarkup = runtime.elements.get("future-ccr-utilization-body").innerHTML;
    const distributionMarkup = runtime.elements.get("future-capacity-utilization-distribution").innerHTML;
    assert.ok(capacityMarkup.includes(selectedCase.name), "capacity protection must show the selected-case marker");
    assert.ok(ccrMarkup.includes(selectedCase.preview.scenario.plan.capacityLoads[0].resourceName), "CCR utilization must show the selected-case marker");
    unselectedCases.forEach(item => {
      assert.ok(!capacityMarkup.includes(item.name), `capacity protection must exclude ${item.responseId}`);
      assert.ok(!ccrMarkup.includes(item.preview.scenario.plan.capacityLoads[0].resourceName), `CCR utilization must exclude ${item.responseId}`);
    });
    assert.ok(distributionMarkup.includes("绿区（0–60%）<strong>1</strong>"), "capacity distribution must retain only the selected Green sentinel");
    assert.ok(distributionMarkup.includes("黄区（>60–80%）<strong>0</strong>"), "capacity distribution must exclude the Yellow sentinel");
    assert.ok(distributionMarkup.includes("红区（>80–100%）<strong>0</strong>"), "capacity distribution must exclude the Red sentinel");
  });
  collect(failures, "selected breach workspace", () => {
    const markup = runtime.elements.get("future-breach-body").innerHTML;
    assert.ok(markup.includes(selectedCase.breaches[0].target), "must show the selected-case breach marker");
    unselectedCases.forEach(item => assert.ok(!markup.includes(item.breaches[0].target), `must exclude ${item.responseId} breach marker`));
  });

  collect(failures, "unified response selection", () => {
    assert.equal(vm.runInContext("typeof selectFutureComparisonCase", runtime.context), "function",
      "selectFutureComparisonCase(responseId) must select a frozen response workspace");
    const fetchCallCount = runtime.fetchCalls.length;
    const workflowBefore = readVm(runtime, "({ savedFutureComparisons: state.savedFutureComparisons, savedScenarioRuns: state.savedScenarioRuns, selectedScenarioRunId: state.selectedScenarioRunId, ddomPackages: state.ddomPackages, selectedDdomPackageId: state.selectedDdomPackageId, currentDdomPackageDetail: state.currentDdomPackageDetail, currentMasterSettingDetail: state.currentMasterSettingDetail, selectedMasterChangeId: state.selectedMasterChangeId })");
    vm.runInContext("selectFutureComparisonCase(__comparisonFixture.responseCases[1].responseId);", runtime.context);
    const selected = readVm(runtime, "({ responseId: state.futureComparisonSelection.responseId, buffer: state.bufferTrend.marker, rccp: state.rccp.marker, constraints: state.constraints.marker, supplier: state.supplierCollaboration.marker })");
    assert.deepEqual(selected, {
      responseId: selectedCase.responseId,
      buffer: selectedCase.preview.scenario.bufferTrend.marker,
      rccp: selectedCase.preview.scenario.rccp.marker,
      constraints: selectedCase.preview.scenario.constraints.marker,
      supplier: selectedCase.preview.scenario.supplierCollaboration.marker,
    }, "one response selection must switch every future workspace to that response");
    assert.equal(runtime.fetchCalls.length, fetchCallCount, "viewing a frozen response must not fetch, save, select, approve, publish, or make it effective");
    assert.deepEqual(readVm(runtime, "({ savedFutureComparisons: state.savedFutureComparisons, savedScenarioRuns: state.savedScenarioRuns, selectedScenarioRunId: state.selectedScenarioRunId, ddomPackages: state.ddomPackages, selectedDdomPackageId: state.selectedDdomPackageId, currentDdomPackageDetail: state.currentDdomPackageDetail, currentMasterSettingDetail: state.currentMasterSettingDetail, selectedMasterChangeId: state.selectedMasterChangeId })"), workflowBefore,
      "viewing a frozen response must not mutate save/select/approve/publish/effective workflow state");
  });

  collect(failures, "selected detail workspaces", () => {
    assert.equal(vm.runInContext("typeof renderSelectedFutureComparisonViews", runtime.context), "function",
      "a selected frozen case must have one explicit detail-workspace render entry point");
    vm.runInContext("selectFutureComparisonCase(__comparisonFixture.responseCases[1].responseId);", runtime.context);
    const detailMarkup = {
      inventory: runtime.elements.get("buffer-trend-case-chip").textContent,
      rccp: runtime.elements.get("rccp-resource-summary-body").innerHTML,
      constraints: runtime.elements.get("constraint-capacity-summary-body").innerHTML,
      supply: runtime.elements.get("supplier-summary-body").innerHTML,
      budget: runtime.elements.get("budget-comparison-body").innerHTML,
    };
    assert.equal(detailMarkup.inventory, selectedCase.preview.scenario.bufferTrend.name,
      "inventory workspace must render the selected response buffer evidence");
    assert.ok(detailMarkup.rccp.includes(selectedCase.preview.scenario.rccp.resourceSummaries[0].resourceName),
      "lower RCCP workbench must render the selected response resource evidence");
    assert.ok(detailMarkup.constraints.includes(selectedCase.preview.scenario.constraints.capacitySummaries[0].resourceName),
      "constraint workbench must render the selected response constraint evidence");
    assert.ok(detailMarkup.supply.includes(selectedCase.preview.scenario.supplierCollaboration.summaries[0].supplier),
      "supply workbench must render the selected response supplier evidence");
    assert.ok(detailMarkup.budget.includes(selectedCase.preview.scenario.budget[0].family),
      "budget workbench must render the selected response budget evidence");
    assert.equal(readVm(runtime, "state.futureInventorySelection.caseId"), selectedCase.responseId,
      "inventory selection must retain the outer frozen response ID");
  });

  collect(failures, "baseline exception source note", () => {
    assert.match(page,
      /<p id="baseline-exception-source-note" class="muted-note">以下异常信号来自当前基线，用于形成场景输入；上方击穿结果来自所选方案。<\/p>/,
      "exception workspace must explicitly identify baseline input evidence");
  });

  const savedCase = structuredClone(selectedCase);
  savedCase.responseId = "SAVED-RESP";
  savedCase.name = "已保存响应方案 SAVED-RESP";
  savedCase.preview.scenario.bufferTrend.name = "SAVED-BUFFER";
  savedCase.preview.scenario.rccp.resourceSummaries[0].resourceName = "SAVED-RCCP";
  savedCase.preview.scenario.constraints.capacitySummaries[0].resourceName = "SAVED-CONSTRAINT";
  savedCase.preview.scenario.supplierCollaboration.summaries[0].supplier = "SAVED-SUPPLIER";
  savedCase.preview.scenario.budget[0].family = "SAVED-BUDGET";
  const savedDetail = {
    summary: {
      runId: "SAVED-RUN",
      runNumber: "RUN-SAVED-RESULT",
      name: savedCase.name,
      baselineSnapshotId: "BASELINE-SAVED",
      externalScenarioId: "EXT-SAVED",
      responseId: savedCase.responseId,
    },
    result: {
      ...savedCase.preview,
      protectionAnalysis: {
        breaches: savedCase.breaches,
        timeBufferProjection: savedCase.timeBufferProjection,
        capacityProtectionProjection: savedCase.capacityProtectionProjection,
      },
    },
  };
  runtime.context.fetch = async url => ({
    ok: true,
    status: 200,
    json: async () => String(url).includes("/audit") ? [] : String(url).includes("/master-settings/") ? []
      : String(url).includes("/coordination-items") ? [] : savedDetail,
  });
  collect(failures, "saved result automatic activation guard", () => {
    vm.runInContext("state.savedFutureComparisons = { 'saved-cache': { runId: 'CACHED-RUN' } };", runtime.context);
    const liveComparison = readVm(runtime, "state.futureComparison");
    const stalePreview = { scenario: { marker: "STALE-PREVIEW" } };
    const saveState = {
      comparisonRequest: readVm(runtime, "state.futureComparisonRequest"),
      comparisonBaseline: readVm(runtime, "state.futureComparisonBaseline"),
      savedFutureComparisons: readVm(runtime, "state.savedFutureComparisons"),
      disabled: runtime.elements.get("save-future-comparison").disabled,
      statusClassName: runtime.elements.get("future-comparison-save-status").className,
      statusText: runtime.elements.get("future-comparison-save-status").textContent,
    };
    runtime.context.__liveComparison = liveComparison;
    runtime.context.__staleSavedPreview = stalePreview;
    runtime.context.__liveSaveState = saveState;
    vm.runInContext("state.preview = __staleSavedPreview;", runtime.context);
  });
  await vm.runInContext("(async () => { await loadScenarioRunDetail('SAVED-RUN'); })()", runtime.context);
  collect(failures, "saved result automatic activation guard", () => {
    assert.deepEqual(readVm(runtime, "state.futureComparison"), runtime.context.__liveComparison,
      "automatic saved-run detail loading must not overwrite a live frozen comparison");
    assert.deepEqual(readVm(runtime, "state.preview"), runtime.context.__staleSavedPreview,
      "automatic saved-run detail loading must not overwrite the legacy preview");
    assert.deepEqual({
      comparisonRequest: readVm(runtime, "state.futureComparisonRequest"),
      comparisonBaseline: readVm(runtime, "state.futureComparisonBaseline"),
      savedFutureComparisons: readVm(runtime, "state.savedFutureComparisons"),
      disabled: runtime.elements.get("save-future-comparison").disabled,
      statusClassName: runtime.elements.get("future-comparison-save-status").className,
      statusText: runtime.elements.get("future-comparison-save-status").textContent,
    }, runtime.context.__liveSaveState,
    "automatic saved-run detail loading must not alter live comparison save state");
  });
  await vm.runInContext("(async () => { await loadScenarioRunDetail('SAVED-RUN', { activateResults: true }); })()", runtime.context);
  collect(failures, "saved result explicit activation", () => {
    assert.equal(readVm(runtime, "state.futureComparison.allCases.length"), 1,
      "explicit saved-run activation must create a one-case frozen result view");
    assert.equal(readVm(runtime, "state.futureComparison.allCases[0].responseId"), savedCase.responseId,
      "saved selector must expose the saved response rather than a synthetic no-response case");
    assert.ok(!runtime.elements.get("future-comparison-cards").innerHTML.includes("不采取企业措施"),
      "saved response card must not be presented as a no-response case");
    assert.equal(readVm(runtime, "state.futureComparisonSelection.responseId"), savedCase.responseId,
      "explicit saved-run activation must select its only response");
    assert.equal(readVm(runtime, "state.preview.scenario.marker"), "STALE-PREVIEW",
      "saved result activation must not copy the saved result into state.preview");
    assert.equal(runtime.elements.get("save-future-comparison").disabled, true,
      "saved read-only result activation must disable comparison saving without a live request");
    assert.deepEqual(readVm(runtime, "state.savedFutureComparisons"), runtime.context.__liveSaveState.savedFutureComparisons,
      "saved result activation must preserve cached save metadata");
  });
  vm.runInContext("state.futureComparisonBaseline = __comparisonBaseline; renderFutureComparison(__comparisonFixture);", runtime.context);

  collect(failures, "delegated result selectors", () => {
    const selectorIds = ["future-result-case-select", "buffer-case-select", "rccp-case-select", "supplier-case-select", "breach-case-select"];
    const selectors = selectorIds.map(id => runtime.element(id));
    selectors.forEach(control => { control.dataset.futureResultCaseSelect = ""; });
    const switchedResponseId = comparison.responseCases[0].responseId;
    selectors[2].value = switchedResponseId;
    runtime.document.dispatchEvent({ type: "change", target: selectors[2] });
    const optionIds = selectors.map(control => [...control.innerHTML.matchAll(/<option value="([^"]+)"/g)].map(match => match[1]));
    optionIds.forEach(ids => assert.deepEqual(ids, expectedResponseIds,
      "every selector must expose the identical frozen response IDs"));
    selectors.forEach(control => assert.equal(control.value, switchedResponseId,
      "delegated selection must synchronize every result selector"));
    assert.equal(switchedResponseId, completeTimeCase.responseId,
      "delegated selector sentinel must switch to the complete time-buffer case");
    assert.ok(runtime.elements.get("time-buffer-breach-evidence-chip").className.includes("is-valid"),
      "unselected sentinel must be genuinely Complete under renderer rules before the missing-evidence switch");
    assert.ok(runtime.elements.get("time-buffer-breach-summary").innerHTML.includes("后端证据完整"),
      "complete sentinel must have a matching definition and full-horizon projections");
  });

  collect(failures, "selected time-buffer evidence isolation", () => {
    vm.runInContext("selectFutureComparisonCase(__comparisonFixture.responseCases[1].responseId);", runtime.context);
    const markup = ["time-buffer-breach-select", "time-buffer-breach-summary", "time-buffer-breach-weekly-grid"]
      .map(id => runtime.elements.get(id).innerHTML).join("\n");
    assert.ok(markup.includes(selectedCase.name), "selected missing-evidence case must remain visible in time-buffer detail");
    assert.ok(markup.includes("证据缺失"), "selected missing evidence must remain missing");
    unselectedCases.forEach(item => assert.ok(!markup.includes(item.name),
      `time-buffer detail must not retain ${item.responseId} evidence after switching`));
  });

  collect(failures, "selected missing inventory evidence isolation", () => {
    vm.runInContext("__comparisonFixture.allCases.find(item => item.responseId === __comparisonFixture.responseCases[1].responseId).preview.scenario.bufferTrend = null; selectFutureComparisonCase(__comparisonFixture.responseCases[1].responseId);", runtime.context);
    assert.equal(readVm(runtime, "state.bufferTrend"), null,
      "a selected response without inventory evidence must remain missing instead of borrowing another case trend");
    assert.ok(runtime.elements.get("buffer-trend-chart").innerHTML.includes("没有缓冲趋势图数据"),
      "inventory workspace must report the selected response evidence gap");
  });

  collect(failures, "selected missing inventory clears stale workspace DOM", () => {
    const stale = "STALE-INVENTORY-EVIDENCE";
    ["buffer-inventory-options", "buffer-comparison-strip", "buffer-trend-kpis", "buffer-trend-chart", "inventory-flow-evidence", "inventory-flow-chart", "buffer-volatility-chart", "buffer-trend-heatmap", "buffer-family-summary-body", "buffer-trend-body", "buffer-replenishment-body", "buffer-sku-metadata", "buffer-trace-list"]
      .forEach(id => { runtime.element(id).innerHTML = stale; });
    runtime.element("buffer-trend-case-chip").textContent = stale;
    runtime.element("buffer-selected-title").textContent = stale;
    runtime.element("buffer-week-range-select").innerHTML = `<option value="1-12">${stale}</option>`;
    runtime.element("buffer-week-range-select").value = "1-12";
    runtime.element("buffer-week-range-select").disabled = false;
    vm.runInContext("selectFutureComparisonCase(__comparisonFixture.responseCases[1].responseId);", runtime.context);
    ["buffer-inventory-options", "buffer-comparison-strip", "buffer-trend-kpis", "buffer-trend-chart", "inventory-flow-evidence", "inventory-flow-chart", "buffer-volatility-chart", "buffer-trend-heatmap", "buffer-family-summary-body", "buffer-trend-body", "buffer-replenishment-body", "buffer-sku-metadata", "buffer-trace-list"]
      .forEach(id => assert.ok(!runtime.element(id).innerHTML.includes(stale), `${id} must clear stale inventory evidence`));
    assert.ok(runtime.element("buffer-trend-case-chip").textContent.includes("证据缺失"),
      "inventory case chip must identify the selected case evidence gap");
    assert.equal(runtime.element("buffer-selected-title").textContent, "选中 SKU 水位趋势",
      "inventory detail title must reset when the selected case has no evidence");
    assert.ok(runtime.element("buffer-week-range-select").innerHTML.includes("证据缺失"),
      "week control must show an evidence-missing option instead of a stale range");
    assert.equal(runtime.element("buffer-week-range-select").disabled, true,
      "week control must be disabled without selected-case inventory evidence");
  });

  collect(failures, "selected missing capacity clears stale workspace DOM", () => {
    const stale = "STALE-CAPACITY-EVIDENCE";
    ["rccp-resource-summary-body", "rccp-heatmap", "rccp-sku-contribution-body", "rccp-action-list", "rccp-load-chart", "constraint-capacity-summary-body", "constraint-heatmap", "constraint-gap-chart", "constraint-action-list", "constraint-trace-list"]
      .forEach(id => { runtime.element(id).innerHTML = stale; });
    runtime.element("rccp-case-chip").textContent = stale;
    runtime.element("rccp-selected-title").textContent = stale;
    runtime.element("constraint-selected-title").textContent = stale;
    vm.runInContext("__comparisonFixture.allCases.find(item => item.responseId === __comparisonFixture.responseCases[1].responseId).preview.scenario.rccp = null; __comparisonFixture.allCases.find(item => item.responseId === __comparisonFixture.responseCases[1].responseId).preview.scenario.constraints = null; selectFutureComparisonCase(__comparisonFixture.responseCases[1].responseId);", runtime.context);
    ["rccp-resource-summary-body", "rccp-heatmap", "rccp-sku-contribution-body", "rccp-action-list", "rccp-load-chart", "constraint-capacity-summary-body", "constraint-heatmap", "constraint-gap-chart", "constraint-action-list", "constraint-trace-list"]
      .forEach(id => assert.ok(!runtime.element(id).innerHTML.includes(stale), `${id} must clear stale capacity evidence`));
    assert.ok(runtime.element("rccp-case-chip").textContent.includes("证据缺失"),
      "RCCP case chip must identify the selected case evidence gap");
    assert.equal(runtime.element("rccp-selected-title").textContent, "选中资源明细",
      "RCCP resource detail title must reset without selected-case RCCP evidence");
    assert.equal(runtime.element("constraint-selected-title").textContent, "选中资源受限 / 不受限明细",
      "constraint detail title must reset without selected-case constraint evidence");
  });

  for (const id of ["future-result-case-select", "buffer-case-select", "rccp-case-select", "supplier-case-select", "breach-case-select"]) {
    collect(failures, `result selector ${id}`, () => {
      const control = page.match(new RegExp(`<[^>]*\\bid=\\"${id}\\"[^>]*>`, "i"))?.[0];
      assert.ok(control, "must exist in result markup");
      assert.ok(control.includes("data-future-result-case-select"), "must declare the shared future-result selector attribute");
    });
  }
  if (failures.length) throw new AggregateError(failures, "Frozen comparison view regression failures");
  console.log("frozen comparison view fixture passed");
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const dtoPath = process.argv[2];
  await runFrozenComparisonViewsFixture(JSON.parse(await readFile(dtoPath, "utf8")));
}
