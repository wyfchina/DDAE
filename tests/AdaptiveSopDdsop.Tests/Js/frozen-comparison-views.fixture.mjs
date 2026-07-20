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
    item.timeBufferProjection = [{
      bufferId: `${marker}-TIME`, controlPoint: marker, week: 1, evidenceStatus: "Complete",
      status: "Green", penetrationPercent: 1, cause: marker,
    }];
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
  ["multi-scenario-comparison-body", "candidate-impact-matrix-body", "future-capacity-protection-body", "future-ccr-utilization-body", "future-capacity-utilization-distribution", "future-breach-body", "future-result-case-select", "buffer-case-select", "rccp-case-select", "supplier-case-select", "breach-case-select"]
    .forEach(id => runtime.element(id));
  const failures = [];
  const expectedResponseIds = comparison.allCases.map(item => item.responseId);
  const selectedCase = comparison.allCases.find(item => item.responseId === comparison.responseCases[1].responseId);
  assert.ok(selectedCase, "fixture must resolve the selected response from comparison.allCases");
  const unselectedCases = comparison.allCases.filter(item => item.responseId !== selectedCase.responseId);
  const selectedTimeBreach = selectedCase.breaches.find(item => item.scopeType === "TimeBuffer");
  selectedTimeBreach.evidenceStatus = "EvidenceMissing";
  selectedCase.timeBufferProjection[0].evidenceStatus = "EvidenceMissing";

  runtime.context.__comparisonFixture = comparison;
  vm.runInContext("renderFutureComparison(__comparisonFixture);", runtime.context);
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
