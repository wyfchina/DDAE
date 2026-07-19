import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

const fixtureDirectory = path.dirname(fileURLToPath(import.meta.url));
const defaultScriptPath = path.resolve(
  fixtureDirectory,
  "..",
  "..",
  "..",
  "src",
  "AdaptiveSopDdsop.Web",
  "wwwroot",
  "js",
  "app.js",
);

function createRuntime(source) {
  const elements = new Map();
  const createElement = (id = "") => {
    if (id && elements.has(id)) return elements.get(id);
    const classes = new Set();
    const attributes = new Map();
    const listeners = new Map();
    const dataKey = name => name
      .replace(/^data-/, "")
      .replace(/-([a-z])/g, (_, letter) => letter.toUpperCase());
    const element = {
      id,
      innerHTML: "",
      textContent: "",
      hidden: false,
      disabled: false,
      value: "",
      checked: false,
      dataset: {},
      style: {},
      parentElement: null,
      children: [],
      classList: {
        add: (...names) => names.forEach(name => classes.add(name)),
        remove: (...names) => names.forEach(name => classes.delete(name)),
        contains: name => classes.has(name),
        toggle: (name, force) => {
          const selected = force === undefined ? !classes.has(name) : Boolean(force);
          if (selected) classes.add(name);
          else classes.delete(name);
          return selected;
        },
      },
      focused: false,
      scrolledIntoView: false,
      setAttribute: (name, value) => {
        attributes.set(name, String(value));
        if (name.startsWith("data-")) element.dataset[dataKey(name)] = String(value);
      },
      getAttribute: name => attributes.get(name) ?? null,
      removeAttribute: name => attributes.delete(name),
      addEventListener(type, handler) {
        const handlers = listeners.get(type) ?? [];
        handlers.push(handler);
        listeners.set(type, handlers);
      },
      removeEventListener(type, handler) {
        listeners.set(type, (listeners.get(type) ?? []).filter(item => item !== handler));
      },
      dispatchEvent(event) {
        event.target ??= element;
        event.currentTarget = element;
        event.preventDefault ??= () => { event.defaultPrevented = true; };
        for (const handler of listeners.get(event.type) ?? []) handler(event);
        return !event.defaultPrevented;
      },
      appendChild(child) {
        element.children.push(child);
        child.parentElement = element;
        return child;
      },
      remove() {},
      focus() { element.focused = true; },
      click() { element.dispatchEvent({ type: "click", target: element }); },
      scrollIntoView() { element.scrolledIntoView = true; },
      querySelector: () => null,
      querySelectorAll: () => [],
      closest(selector) {
        if (selector.startsWith("#")) return element.id === selector.slice(1) ? element : null;
        const dataMatch = selector.match(/^\[data-([a-z0-9-]+)\]$/i);
        if (dataMatch) return element.dataset[dataKey(`data-${dataMatch[1]}`)] === undefined ? null : element;
        return null;
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
    addEventListener(type, handler) {
      const handlers = documentListeners.get(type) ?? [];
      handlers.push(handler);
      documentListeners.set(type, handlers);
    },
    removeEventListener(type, handler) {
      documentListeners.set(type, (documentListeners.get(type) ?? []).filter(item => item !== handler));
    },
    dispatchEvent(event) {
      event.target ??= document;
      event.currentTarget = document;
      event.preventDefault ??= () => { event.defaultPrevented = true; };
      for (const handler of documentListeners.get(event.type) ?? []) handler(event);
      return !event.defaultPrevented;
    },
    body: createElement("fixture-body"),
    documentElement: createElement("fixture-html"),
  };
  const window = {
    document,
    location: { hash: "", pathname: "/", search: "" },
    history: {
      replaceState(_state, _title, url) {
        if (typeof url === "string" && url.includes("#")) window.location.hash = url.slice(url.indexOf("#"));
      },
      pushState(_state, _title, url) {
        if (typeof url === "string" && url.includes("#")) window.location.hash = url.slice(url.indexOf("#"));
      },
    },
    addEventListener() {},
    removeEventListener() {},
    matchMedia: () => ({ matches: false, addEventListener() {}, removeEventListener() {} }),
    scrollTo() {},
  };
  const context = vm.createContext({
    console,
    document,
    window,
    location: window.location,
    history: window.history,
    navigator: { clipboard: { writeText: async () => {} } },
    fetch: () => new Promise(() => {}),
    setTimeout,
    clearTimeout,
    setInterval,
    clearInterval,
    URL,
    URLSearchParams,
    TextEncoder,
    TextDecoder,
    AbortController,
    Response,
    Request,
    Headers,
    structuredClone,
    crypto: globalThis.crypto,
    performance: globalThis.performance,
    requestAnimationFrame: () => 0,
    cancelAnimationFrame() {},
    getComputedStyle: () => ({ display: "block" }),
    CSS: { escape: value => String(value) },
    MutationObserver: class { observe() {} disconnect() {} },
    IntersectionObserver: class { observe() {} disconnect() {} },
  });
  context.globalThis = context;
  new vm.Script(source, { filename: defaultScriptPath }).runInContext(context);
  return { context, elements, createElement, document };
}

function makeDetail(sku, name, family, caseOffset = 0) {
  const series = [1, 2, 3, 4].map(week => ({
    sku,
    week,
    periodStartDate: `2026-06-${String(1 + (week - 1) * 7).padStart(2, "0")}`,
    startNetFlow: 80 + caseOffset + week,
    demand: 20 + week,
    demandSpikeThreshold: 30,
    endNetFlowBeforeReplenishment: 65 + caseOffset - week,
    endNetFlowAfterReplenishment: 85 + caseOffset - week,
    topOfRed: 40 + week,
    topOfYellow: 75 + week,
    topOfGreen: 110 + week,
    physicalPosition: {
      endingOnHand: 90 + caseOffset - week,
      endingBacklog: week === 4 ? 2 : 0,
      onHandStatus: week === 4 ? "Yellow" : "Green",
      evidenceStatus: "Complete",
      source: "InventoryFlowProjection",
    },
    timePhasedAdu: 10,
    inventoryValue: 1000 + week * 10,
    replenishmentQuantity: week === 2 ? 20 : 0,
    isReplenishment: week === 2,
    isPrebuild: false,
    status: "Green",
  }));
  return {
    sku,
    name,
    family,
    adu: 10,
    decoupledLeadTimeDays: 12,
    minimumOrderQuantity: 10,
    orderCycleDays: 7,
    unitCost: 100,
    zone: { topOfRed: 40, topOfYellow: 75, topOfGreen: 110 },
    series,
    replenishmentOrders: [{ week: 2, quantity: 20, value: 2000, trigger: "OrderCycle" }],
    traces: [{ week: 2, explanation: "后端补货复核记录" }],
    activities: [],
    attributes: [],
    bufferSizing: [],
    bom: [],
    orderDetails: [],
  };
}

function makeTrend(caseId, name, caseOffset = 0) {
  const details = [
    makeDetail("SKU-A", "物料甲", "产品族甲", caseOffset),
    makeDetail("SKU-B", "物料乙", "产品族乙", caseOffset + 4),
  ];
  const series = details.flatMap(item => item.series);
  return {
    caseId,
    name,
    selectedSku: "SKU-A",
    series,
    weeklyCells: series.map(item => ({
      sku: item.sku,
      family: item.sku === "SKU-A" ? "产品族甲" : "产品族乙",
      week: item.week,
      status: item.status,
      inventoryValue: item.inventoryValue,
    })),
    zoneBands: [],
    skuDetails: details,
    comparison: {
      averageInventoryValueDelta: 0,
      peakInventoryValueDelta: 0,
      redWeekDelta: 0,
      replenishmentOrderCountDelta: 0,
      replenishmentQuantityDelta: 0,
    },
  };
}

function makeFlow(caseId, caseOffset = 0) {
  return {
    caseId,
    status: "Complete",
    baselineSnapshotId: "BASE-20260717-001",
    points: ["SKU-A", "SKU-B"].flatMap((sku, skuIndex) => [1, 2, 3, 4].map(week => ({
      sku,
      week,
      openingOnHand: 100,
      openingBacklog: 0,
      demand: 20 + week,
      frozenReceiptQuantity: week === 1 ? 12 + skuIndex : 0,
      simulatedReceiptQuantity: week === 2 ? 16 + caseOffset : 0,
      prebuildReceiptQuantity: week === 3 ? 8 : 0,
      fulfilledOpeningBacklog: 0,
      fulfilledNewDemandOnTime: 20 + week,
      totalFulfilledDemand: 20 + week,
      endingOnHand: 90 + caseOffset + skuIndex * 5 - week * 4,
      endingBacklog: week === 4 ? skuIndex * 2 : 0,
      endingInventoryValue: 9000,
      weeklyServicePercent: 100,
      weeklyServiceStatus: "Complete",
    }))),
    receiptLog: [],
    skuSummaries: [],
    summary: {
      onTimeServicePercent: 100,
      averageInventoryValue: 9000,
      endingInventoryValue: 8500,
      endingBacklog: 0,
    },
    trace: [{ stage: "ProjectedLedger", sku: null, week: null, sourceId: null, explanation: "后端物理库存投影" }],
    issues: [],
  };
}

function makePreview() {
  const makeCase = (caseId, name, caseOffset) => ({
    caseId,
    name,
    bufferTrend: makeTrend(caseId, name, caseOffset),
    inventoryFlow: makeFlow(caseId, caseOffset),
    scenarioMetricEvidence: [{
      jsonPath: "metrics.averageInventoryValue",
      evidenceStatus: "Complete",
      source: "PhysicalProjection",
      explanation: "来自后端库存守恒投影",
      projectionCaseId: caseId,
      baselineSnapshotId: "BASE-20260717-001",
    }],
    plan: {
      bufferProjections: ["SKU-A", "SKU-B"].flatMap(sku => [1, 2, 3, 4].map(week => ({ sku, week }))),
      traces: ["SKU-A", "SKU-B"].flatMap(sku => [1, 2, 3, 4].map(week => ({
        sku,
        week,
        explanation: `${caseId} ${sku} 第 ${week} 周后端计算记录`,
      }))),
    },
  });
  return {
    trace: [],
    baseline: makeCase("baseline", "基准方案", 0),
    scenario: makeCase("scenario", "响应方案", 8),
  };
}

function sourceFunctionBody(source, functionName) {
  const functionStart = source.indexOf(`function ${functionName}(`);
  assert.ok(functionStart >= 0, `expected protected function ${functionName}`);
  const bodyStart = source.indexOf("{", functionStart);
  let depth = 0;
  for (let index = bodyStart; index < source.length; index += 1) {
    if (source[index] === "{") depth += 1;
    else if (source[index] === "}" && --depth === 0) return source.slice(bodyStart + 1, index);
  }
  return "";
}

function hashBody(source, functionName) {
  const normalizedBody = sourceFunctionBody(source, functionName)
    .replaceAll("\r\n", "\n")
    .replaceAll("\r", "\n");
  return createHash("sha256").update(normalizedBody).digest("hex");
}

function runPreview(runtime, preview) {
  runtime.context.__previewFixture = structuredClone(preview);
  vm.runInContext("state.preview = __previewFixture; renderPreviewBufferTrend(__previewFixture);", runtime.context);
}

function readVm(runtime, expression) {
  return JSON.parse(vm.runInContext(`JSON.stringify(${expression})`, runtime.context));
}

function polylinePoints(markup, cssClass) {
  const match = markup.match(new RegExp(`<polyline[^>]*class="${cssClass}"[^>]*points="([^"]+)"`));
  assert.ok(match, `expected ${cssClass} polyline`);
  return match[1].trim().split(/\s+/).map(pair => pair.split(",").map(Number));
}

async function verifyAtomicWorkspaceRefresh(runtime, preview) {
  const previousPreview = structuredClone(preview);
  const previousTrend = structuredClone(preview.scenario.bufferTrend);
  const refreshedTrend = structuredClone(preview.scenario.bufferTrend);
  refreshedTrend.caseId = "current-refresh";
  refreshedTrend.name = "当前刷新";
  refreshedTrend.selectedSku = refreshedTrend.skuDetails.at(-1).sku;
  const previousSelection = {
    caseId: preview.scenario.caseId,
    sku: previousTrend.selectedSku,
    weekFrom: 2,
    weekThrough: 3,
  };
  runtime.context.__previousPreview = previousPreview;
  runtime.context.__previousTrend = previousTrend;
  runtime.context.__previousSelection = previousSelection;
  runtime.context.__refreshedTrend = refreshedTrend;
  runtime.context.__lateRefreshFailure = true;
  runtime.context.__loadEvents = [];
  vm.runInContext(`
    state.preview = __previousPreview;
    state.bufferTrend = __previousTrend;
    state.baselineBufferTrend = __previousPreview.baseline.bufferTrend;
    state.selectedBufferSku = __previousSelection.sku;
    state.futureInventorySelection = { ...__previousSelection };
    configureFiveStageScenarioControls = () => {};
    loadHistoryReview = async () => null;
    loadCurrentBaselineWorkspace = async () => null;
    loadCoordinationItems = async () => null;
    loadScenarioAssumptionTemplates = async () => null;
    configureFilters = () => { __loadEvents.push("configure-filters"); };
    configurePreviewControls = () => { __loadEvents.push("configure-preview"); };
    loadPublicDemoGoldenLoop = async () => { __loadEvents.push("public-demo"); };
    loadAdventureWorksProductDemo = async () => { __loadEvents.push("adventure-works"); };
    loadSavedScenarioRuns = async () => {
      __loadEvents.push("saved-runs");
      if (__lateRefreshFailure) throw new Error("late required loader failed");
    };
    applyFilters = () => { __loadEvents.push("apply-filters"); };
  `, runtime.context);
  runtime.context.fetch = async url => {
    const path = String(url);
    const payload = path.startsWith("/api/scenario-workspace-data")
      ? {}
      : path.startsWith("/api/product-family-dashboard")
        ? { selectedFamily: null }
        : path.startsWith("/api/rccp-workspace")
          ? { resourceSummaries: [] }
          : path.startsWith("/api/constraint-workspace")
            ? {}
            : path.startsWith("/api/supplier-collaboration-workspace")
              ? { selectedSupplier: null }
              : path.startsWith("/api/buffer-trend-workspace")
                ? refreshedTrend
                : path.startsWith("/api/exception-workspace")
                  ? { exceptions: [] }
                  : path.startsWith("/api/master-settings-workspace")
                    ? {}
                    : {};
    return {
      ok: true,
      status: 200,
      json: async () => structuredClone(payload),
    };
  };

  await assert.rejects(vm.runInContext("loadWorkspace()", runtime.context), /late required loader failed/,
    "late required refresh failure should reject loadWorkspace");
  assert.deepEqual(readVm(runtime,
    "({ preview: state.preview, bufferTrend: state.bufferTrend, selection: state.futureInventorySelection })"),
  { preview: previousPreview, bufferTrend: previousTrend, selection: previousSelection },
  "failed refresh must not commit the new buffer trend or clear the old preview selection");
  assert.ok(!readVm(runtime, "__loadEvents").includes("apply-filters"),
    "failed refresh must not render partially refreshed state");

  runtime.context.__lateRefreshFailure = false;
  await vm.runInContext("loadWorkspace()", runtime.context);
  assert.deepEqual(readVm(runtime,
    "({ preview: state.preview, caseId: state.bufferTrend.caseId, selectedBufferSku: state.selectedBufferSku, "
      + "selection: state.futureInventorySelection })"), {
    preview: null,
    caseId: refreshedTrend.caseId,
    selectedBufferSku: refreshedTrend.selectedSku,
    selection: {
      caseId: refreshedTrend.caseId,
      sku: refreshedTrend.selectedSku,
      weekFrom: 1,
      weekThrough: null,
    },
  }, "successful refresh should atomically accept current buffer evidence before rendering");
  const successEvents = readVm(runtime, "__loadEvents");
  assert.ok(successEvents.lastIndexOf("saved-runs") < successEvents.lastIndexOf("apply-filters"),
    "successful refresh should render only after the last required loader completes");
}

export async function runFutureInventoryFlowChartFixtures(preview, scriptPath = defaultScriptPath) {
  assert.ok(preview?.baseline?.bufferTrend && preview?.scenario?.bufferTrend,
    "fixture should contain backend NFP cases");
  assert.ok(preview?.scenario?.inventoryFlow, "fixture should contain backend physical flow");
  const source = await readFile(scriptPath, "utf8");
  new vm.Script(source, { filename: scriptPath });
  assert.equal(hashBody(source, "renderPreviewTrace"),
    "5d717644be89f0bd29351b73d0621aa4413c39ba4b98f319e4a9fd664c54da35",
    "renderPreviewTrace body must remain protected");
  assert.equal(hashBody(source, "renderTrace"),
    "86418e85eb2880a6f776f69f5796a35d98097892b8a3575972eff89ccb7ab3ff",
    "renderTrace body must remain protected");

  const runtime = createRuntime(source);
  await verifyAtomicWorkspaceRefresh(runtime, preview);
  runPreview(runtime, preview);
  const nfpMarkup = runtime.elements.get("buffer-trend-chart").innerHTML;
  const physicalHost = runtime.elements.get("inventory-flow-chart");
  assert.ok(physicalHost, "physical chart host and renderer should exist");
  const physicalMarkup = physicalHost.innerHTML;
  const volatilityMarkup = runtime.elements.get("buffer-volatility-chart").innerHTML;
  assert.ok(nfpMarkup.includes("buffer-net-flow-line") && nfpMarkup.includes('data-case-id="scenario"'),
    "first panel should retain scenario NFP evidence");
  assert.ok(nfpMarkup.includes('class="buffer-net-flow-line" data-field="endNetFlowBeforeReplenishment"'),
    "pre-replenishment NFP must retain its explicit black-line field mapping");
  assert.ok(nfpMarkup.includes('class="buffer-preview-line" data-field="endNetFlowAfterReplenishment"'),
    "selected scenario post-replenishment NFP must retain its explicit blue-line field mapping");
  assert.ok(nfpMarkup.includes('class="buffer-baseline-line" data-field="endNetFlowAfterReplenishment"'),
    "baseline comparison must retain its explicit gray-line field mapping");
  assert.ok(nfpMarkup.includes('class="buffer-on-hand-line" data-field="physicalPosition.endingOnHand"'),
    "upper chart must map the physical on-hand line directly from the optional backend position");
  assert.ok(!nfpMarkup.includes("target-inventory-dot") && !nfpMarkup.includes("目标库存"),
    "upper chart must not retain target-inventory markers or legend");
  assert.ok(physicalMarkup.includes("physical-on-hand-line") && physicalMarkup.includes('data-case-id="scenario"'),
    "second panel should render backend scenario physical on-hand");
  assert.ok(volatilityMarkup.includes("buffer-demand-area") && volatilityMarkup.includes('data-case-id="scenario"'),
    "third panel should retain independent scenario volatility");

  const baselineUpperAxisY = nfpMarkup.match(/<text class="axis-label" x="54" y="([^"]+)">/)?.[1];
  assert.ok(baselineUpperAxisY, "upper chart should expose a stable zone-axis geometry marker");
  const missingScaleEvidence = structuredClone(preview);
  const selectedScaleSku = missingScaleEvidence.scenario.bufferTrend.selectedSku;
  for (const series of [
    missingScaleEvidence.scenario.bufferTrend.series,
    ...missingScaleEvidence.scenario.bufferTrend.skuDetails.map(detail => detail.series),
  ]) {
    series.filter(point => point.sku === selectedScaleSku && point.week === 1).forEach(point => {
      point.physicalPosition = {
        ...point.physicalPosition,
        endingOnHand: 1000000,
        evidenceStatus: "EvidenceMissing",
      };
    });
  }
  runPreview(runtime, missingScaleEvidence);
  const missingScaleMarkup = runtime.elements.get("buffer-trend-chart").innerHTML;
  const missingScaleAxisY = missingScaleMarkup.match(/<text class="axis-label" x="54" y="([^"]+)">/)?.[1];
  assert.equal(missingScaleAxisY, baselineUpperAxisY,
    "an EvidenceMissing on-hand value must not expand or rescale the upper chart axis");
  runPreview(runtime, preview);

  const rangeScopedPhysicalEvidence = structuredClone(preview);
  const scopedTrend = rangeScopedPhysicalEvidence.scenario.bufferTrend;
  scopedTrend.skuDetails.forEach(detail => {
    detail.replenishmentOrders = detail.replenishmentOrders.map(order => ({ ...order, week: 2 }));
  });
  for (const series of [scopedTrend.series, ...scopedTrend.skuDetails.map(detail => detail.series)]) {
    series.forEach(point => {
      point.physicalPosition = {
        ...point.physicalPosition,
        evidenceStatus: point.week === 1 ? "Complete" : "EvidenceMissing",
        onHandStatus: "Red",
      };
    });
  }
  runPreview(runtime, rangeScopedPhysicalEvidence);
  const scopedWeekSelect = runtime.elements.get("buffer-week-range-select");
  scopedWeekSelect.value = "2-3";
  scopedWeekSelect.dispatchEvent({ type: "change", target: scopedWeekSelect });
  const rangeScopedKpis = runtime.elements.get("buffer-trend-kpis").innerHTML;
  assert.ok(rangeScopedKpis.includes("在手红区 SKU") && rangeScopedKpis.includes("证据缺失"),
    "a selected range with no Complete physical positions must show missing physical KPI evidence, not zero");
  assert.ok(runtime.elements.get("buffer-trend-chart").innerHTML.includes("buffer-net-flow-line")
    && !runtime.elements.get("buffer-trend-chart").innerHTML.includes("buffer-on-hand-line"),
  "EvidenceMissing physical-position objects must not render an upper on-hand line");
  scopedWeekSelect.value = "3-4";
  scopedWeekSelect.dispatchEvent({ type: "change", target: scopedWeekSelect });
  const rangeScopedFamilies = readVm(runtime, "filterBufferTrendWorkspace(state.bufferTrend).familySummaries");
  assert.ok(rangeScopedFamilies.every(item => item.replenishmentOrderCount === 0),
    "family replenishment summaries must exclude orders outside the selected week range");
  runPreview(runtime, preview);

  for (const [cssClass, field] of [
    ["physical-on-hand-line", "endingOnHand"],
    ["physical-frozen-receipt", "frozenReceiptQuantity"],
    ["physical-simulated-receipt", "simulatedReceiptQuantity"],
    ["physical-prebuild-receipt", "prebuildReceiptQuantity"],
    ["physical-ending-backlog", "endingBacklog"],
  ]) {
    assert.ok(physicalMarkup.includes(`class="${cssClass}"`) && physicalMarkup.includes(`data-field="${field}"`),
      `${field} should retain its fixed renderer mapping`);
  }
  assert.ok(physicalMarkup.includes('data-axis="physical-on-hand"')
    && physicalMarkup.includes('data-axis="physical-events"'),
  "physical stock and event quantities should use labeled independent axes");
  const initialScenarioSku = preview.scenario.bufferTrend.selectedSku
    ?? preview.scenario.bufferTrend.skuDetails[0].sku;
  const initialScenarioRecordKey = runtime.elements.get("buffer-trace-list").innerHTML
    .match(/data-white-box-record="([^"]+)"/)?.[1];
  assert.ok(initialScenarioRecordKey?.startsWith("scenario|")
    && initialScenarioRecordKey.includes(`|${initialScenarioSku}|`),
  "scenario detail should expose a compound key from its actual plan trace");

  const negative = structuredClone(preview);
  const negativeSku = negative.scenario.bufferTrend.selectedSku
    ?? negative.scenario.bufferTrend.skuDetails[0].sku;
  const negativeDetail = negative.scenario.bufferTrend.skuDetails.find(item => item.sku === negativeSku);
  negativeDetail.series[1].endNetFlowBeforeReplenishment = -1000;
  const negativeSeriesItem = negative.scenario.bufferTrend.series.find(item =>
    item.sku === negativeSku && item.week === 2);
  negativeSeriesItem.endNetFlowBeforeReplenishment = -1000;
  runPreview(runtime, negative);
  const negativeMarkup = runtime.elements.get("buffer-trend-chart").innerHTML;
  const zeroAxisY = Number(negativeMarkup.match(/<line class="axis-line"[^>]*y1="([^"]+)"/)?.[1]);
  const negativeNfpYs = polylinePoints(negativeMarkup, "buffer-net-flow-line").map(point => point[1]);
  assert.ok(Number.isFinite(zeroAxisY) && Math.max(...negativeNfpYs) > zeroAxisY,
    "negative pre-replenishment NFP must plot below the zero axis instead of flattening to zero");

  const staleCurrent = makeTrend("current-refresh", "当前刷新", 30);
  staleCurrent.selectedSku = staleCurrent.skuDetails[1].sku;
  runtime.context.__staleCurrent = structuredClone(staleCurrent);
  vm.runInContext("renderBufferTrendWorkspace(__staleCurrent);", runtime.context);
  assert.ok(runtime.elements.get("buffer-trend-chart").innerHTML.includes('data-case-id="current-refresh"'),
    "an explicit current trend refresh must not be overridden by stale preview cases");
  assert.equal(runtime.elements.get("buffer-case-select").innerHTML.includes('value="scenario"'), false,
    "current trend refresh must not mix stale scenario choices into its selector");
  vm.runInContext("acceptCurrentBufferTrend(__staleCurrent); renderBufferTrendWorkspace(state.bufferTrend);", runtime.context);
  assert.equal(readVm(runtime, "state.preview"), null,
    "a successful current refresh must invalidate the old preview result");
  assert.deepEqual(readVm(runtime,
    "({ caseId: state.futureInventorySelection.caseId, sku: state.futureInventorySelection.sku })"),
  { caseId: staleCurrent.caseId, sku: staleCurrent.selectedSku },
  "a successful current refresh must reset the case and SKU selection to current evidence");

  const filterMigration = structuredClone(preview);
  const firstDetail = filterMigration.scenario.bufferTrend.skuDetails[0];
  const secondDetail = filterMigration.scenario.bufferTrend.skuDetails[1];
  firstDetail.series = firstDetail.series.filter(item => item.week >= 2 && item.week <= 3);
  filterMigration.scenario.bufferTrend.series = filterMigration.scenario.bufferTrend.skuDetails.flatMap(item => item.series);
  filterMigration.scenario.bufferTrend.weeklyCells = filterMigration.scenario.bufferTrend.weeklyCells.filter(item =>
    item.sku !== firstDetail.sku || (item.week >= 2 && item.week <= 3));
  runPreview(runtime, filterMigration);
  runtime.context.__allowedSku = firstDetail.sku;
  runtime.context.__removedSku = secondDetail.sku;
  vm.runInContext(
    "state.futureInventorySelection.sku = __removedSku; state.selectedBufferSku = __removedSku; "
      + "state.futureInventorySelection.weekFrom = 1; state.futureInventorySelection.weekThrough = 4; "
      + "state.filtered = { skus: [{ sku: __allowedSku }] }; renderBufferTrendWorkspace(state.bufferTrend);",
    runtime.context,
  );
  let migratedSelection = readVm(runtime,
    "({ sku: state.futureInventorySelection.sku, selectedBufferSku: state.selectedBufferSku, "
      + "weekFrom: state.futureInventorySelection.weekFrom, weekThrough: state.futureInventorySelection.weekThrough })");
  assert.deepEqual(migratedSelection,
    { sku: firstDetail.sku, selectedBufferSku: firstDetail.sku, weekFrom: 2, weekThrough: 3 },
    "filtering out the selected SKU must migrate both selections and derive weeks from filtered detail");
  runtime.context.__allSkus = [{ sku: firstDetail.sku }, { sku: secondDetail.sku }];
  vm.runInContext("state.filtered = { skus: __allSkus }; renderBufferTrendWorkspace(state.bufferTrend);", runtime.context);
  migratedSelection = readVm(runtime,
    "({ sku: state.futureInventorySelection.sku, selectedBufferSku: state.selectedBufferSku })");
  assert.deepEqual(migratedSelection, { sku: firstDetail.sku, selectedBufferSku: firstDetail.sku },
    "clearing the filter must keep the migrated SKU instead of jumping back to the stale selection");
  runPreview(runtime, preview);
  const selectedSku = preview.scenario.bufferTrend.selectedSku
    ?? preview.scenario.bufferTrend.skuDetails[0].sku;
  const secondSku = preview.scenario.bufferTrend.skuDetails.find(item => item.sku !== selectedSku)?.sku
    ?? selectedSku;
  const skuButton = runtime.createElement();
  skuButton.dataset.bufferSku = secondSku;
  runtime.document.dispatchEvent({ type: "click", target: skuButton });
  assert.equal(readVm(runtime, "state.futureInventorySelection.sku"), secondSku,
    "the registered SKU click handler should update the shared selection");
  for (const id of ["buffer-trend-chart", "inventory-flow-chart", "buffer-volatility-chart"]) {
    assert.ok(runtime.elements.get(id).innerHTML.includes(secondSku),
      `${id} should redraw from the registered SKU click handler`);
  }

  const firstFilteredFamilyDetail = readVm(runtime,
    "filterBufferTrendWorkspace(state.bufferTrend).skuDetails[0]");
  const familyButton = runtime.createElement();
  familyButton.dataset.bufferFamily = firstFilteredFamilyDetail.family;
  runtime.document.dispatchEvent({ type: "click", target: familyButton });
  assert.equal(readVm(runtime, "state.futureInventorySelection.sku"), firstFilteredFamilyDetail.sku,
    "the registered family click handler should select the first filtered family SKU");

  const caseSelect = runtime.elements.get("buffer-case-select");
  caseSelect.value = "baseline";
  caseSelect.dispatchEvent({ type: "change", target: caseSelect });
  const weekSelect = runtime.elements.get("buffer-week-range-select");
  weekSelect.value = "2-3";
  weekSelect.dispatchEvent({ type: "change", target: weekSelect });
  for (const id of ["buffer-trend-chart", "inventory-flow-chart", "buffer-volatility-chart"]) {
    const markup = runtime.elements.get(id).innerHTML;
    assert.ok(markup.includes('data-case-id="baseline"'), `${id} should redraw the selected case`);
    assert.ok(markup.includes('data-week-from="2"') && markup.includes('data-week-through="3"'),
      `${id} should redraw the shared selected week range`);
  }

  const whiteBoxMarkup = runtime.elements.get("buffer-trace-list").innerHTML;
  const whiteBoxRecordKey = whiteBoxMarkup.match(/data-white-box-record="([^"]+)"/)?.[1];
  assert.ok(whiteBoxRecordKey
    && whiteBoxRecordKey.includes(`|${firstFilteredFamilyDetail.sku}|`)
    && !whiteBoxRecordKey.includes(`baseline:${firstFilteredFamilyDetail.sku}`),
  "selected detail should link to a compound key built from an actual baseline plan trace");
  const whiteBoxLink = runtime.createElement();
  whiteBoxLink.dataset.whiteBoxRecord = whiteBoxRecordKey;
  runtime.document.dispatchEvent({ type: "click", target: whiteBoxLink });
  assert.equal(runtime.context.window.location.hash, "#trace-panel",
    "white-box link click should navigate through the registered handler to the trace panel");
  assert.equal(readVm(runtime, "state.selectedWhiteBoxTraceKey"), whiteBoxRecordKey,
    "white-box link click should select the corresponding actual trace record");
  const focusedTrace = runtime.elements.get("trace-list").children.at(-1);
  assert.equal(focusedTrace?.dataset.whiteBoxTraceKey, whiteBoxRecordKey,
    "the selected white-box record must be materialized under #trace-list");
  assert.ok(focusedTrace?.focused && focusedTrace?.scrolledIntoView,
    "the actual white-box record should receive focus and be scrolled into view");

  const missing = structuredClone(preview);
  missing.scenario.inventoryFlow.status = "EvidenceMissing";
  missing.scenario.inventoryFlow.points = [];
  missing.scenario.inventoryFlow.issues = [{
    scope: "InventoryFlow",
    sku: "SKU-A",
    week: 2,
    reason: "后端确认到货证据缺失 <img src=x onerror=alert(1)>",
    sourceId: null,
    blocksFreeze: true,
    blocksProjection: true,
  }];
  missing.scenario.scenarioMetricEvidence = missing.scenario.scenarioMetricEvidence.map(item => ({
    ...item,
    evidenceStatus: "EvidenceMissing",
    source: "LegacyReference",
  }));
  missing.scenario.inventoryFlow.trace = [{
    stage: "ValidatedInputs",
    sku: "SKU-A",
    week: 2,
    sourceId: null,
    explanation: "当前投影输入检查",
  }];
  runPreview(runtime, missing);
  const missingMarkup = runtime.elements.get("inventory-flow-chart").innerHTML;
  assert.ok(!missingMarkup.includes("physical-on-hand-line"),
    "EvidenceMissing must draw no physical on-hand line");
  assert.ok(missingMarkup.includes("后端确认到货证据缺失")
    && missingMarkup.includes("&lt;img")
    && !missingMarkup.includes("<img"),
    "EvidenceMissing should expose the backend issue");
  assert.ok(!runtime.elements.get("inventory-flow-evidence").innerHTML.includes("历史兼容记录"),
    "current EvidenceMissing must not be mislabeled as a legacy record from metric source alone");
  assert.ok(!runtime.elements.get("inventory-flow-evidence").innerHTML.includes("LegacyReference"),
    "current EvidenceMissing trace source must come from current flow validation, not legacy metric fallback");

  const missingInventoryUi = structuredClone(preview);
  const missingMetrics = {
    serviceLevelPercent: 96,
    flowIndex: 88,
    averageInventoryValue: null,
    peakLoadPercent: 85,
    averageLoadPercent: 72,
    redSkuCount: 0,
    supplyGap: 0,
    replenishmentValue: 1000,
    replenishmentOrderCount: 1,
  };
  missingInventoryUi.request = { horizonWeeks: 4, templateId: null };
  missingInventoryUi.baseline.metrics = { ...missingMetrics };
  missingInventoryUi.scenario.metrics = { ...missingMetrics };
  missingInventoryUi.comparison = {
    serviceLevelDelta: 0,
    flowIndexDelta: 0,
    averageInventoryValueDelta: null,
    peakLoadPercentDelta: 0,
    averageLoadPercentDelta: 0,
    redSkuCountDelta: 0,
    supplyGapDelta: 0,
    replenishmentValueDelta: 0,
    replenishmentOrderCountDelta: 0,
  };
  missingInventoryUi.scenario.inventoryFlow.status = "EvidenceMissing";
  missingInventoryUi.scenario.budget = [{
    family: "FAM-A",
    week: 1,
    budgetRevenue: 1000,
    lastYearRevenue: 900,
    budgetInventoryValue: 500,
    lastYearInventoryValue: 450,
    projectedInventoryValue: null,
    budgetInventoryVariance: null,
  }];
  const familyCell = {
    family: "FAM-A",
    week: 1,
    demand: 10,
    replenishmentQuantity: 5,
    inventoryValue: null,
    redSkuCount: 0,
    yellowSkuCount: 0,
    supplyGap: 0,
    capacityGap: 0,
    peakLoadPercent: 70,
    budgetInventoryVariance: null,
    status: "Green",
  };
  missingInventoryUi.scenario.productFamilyDashboard = {
    caseId: "scenario",
    name: "缺失物理库存证据",
    horizonWeeks: 1,
    selectedFamily: "FAM-A",
    summaries: [{
      family: "FAM-A",
      name: "产品族甲",
      skuCount: 1,
      targetServiceLevel: 95,
      targetFlowIndex: 85,
      serviceLevelPercent: 96,
      flowIndex: 88,
      averageInventoryValue: null,
      peakInventoryValue: null,
      redSkuCount: 0,
      redWeekCount: 0,
      yellowWeekCount: 0,
      replenishmentOrderCount: 1,
      replenishmentValue: 1000,
      supplyGap: 0,
      capacityGap: 0,
      peakLoadPercent: 70,
      budgetInventoryVariance: null,
      status: "Green",
      recommendedAction: "保持",
    }],
    weeklyCells: [familyCell],
    details: [{
      family: "FAM-A",
      name: "产品族甲",
      weeklyCells: [familyCell],
      riskItems: [],
      recommendations: [],
      bufferSummaries: [{ family: "FAM-A", averageInventoryValue: null }],
      rccpContributions: [],
      supplierRequirements: [],
    }],
    comparison: {
      serviceLevelDelta: 0,
      flowIndexDelta: 0,
      averageInventoryValueDelta: null,
      supplyGapDelta: 0,
      capacityGapDelta: 0,
      redWeekDelta: 0,
      budgetInventoryVarianceDelta: null,
      physicalDeltaEvidenceStatus: "EvidenceMissing",
    },
  };
  missingInventoryUi.feasibility = null;
  runtime.context.__missingInventoryUi = missingInventoryUi;
  vm.runInContext(`
    state.preview = __missingInventoryUi;
    state.data = null;
    state.filtered = null;
    renderPreviewKpis(__missingInventoryUi);
    renderPreviewComparison(__missingInventoryUi);
    renderPreviewBudget(__missingInventoryUi);
    renderProductFamilyDashboard(__missingInventoryUi.scenario.productFamilyDashboard);
    showScenarioSavePanel(__missingInventoryUi);
    __missingInventoryAdoption = evaluateAdoption(__missingInventoryUi);
  `, runtime.context);
  for (const id of [
    "workspace-kpis",
    "scenario-comparison-result",
    "budget-comparison-body",
    "product-family-kpis",
    "product-family-card-grid",
    "product-family-weekly-grid",
    "product-family-detail-summary",
  ]) {
    assert.ok(runtime.elements.get(id).innerHTML.includes("证据缺失"),
      `${id} must show missing inventory evidence instead of a zero amount`);
  }
  assert.equal(runtime.elements.get("selector:#save-scenario").disabled, true,
    "missing physical inventory evidence must disable scenario saving");
  assert.deepEqual(readVm(runtime, "__missingInventoryAdoption.status"), "Red",
    "missing backend feasibility must block adoption instead of falling back to local evidence rules");
  assert.ok(readVm(runtime, "__missingInventoryAdoption.message").includes("后端可行性结果缺失"),
    "adoption blocker must explain the missing backend feasibility assessment");

  const poisonedSaveEvidence = structuredClone(preview);
  poisonedSaveEvidence.request = { horizonWeeks: 4, templateId: null };
  poisonedSaveEvidence.baseline.metrics = { ...missingMetrics, averageInventoryValue: 9000 };
  poisonedSaveEvidence.scenario.metrics = { ...missingMetrics, averageInventoryValue: 9000 };
  poisonedSaveEvidence.scenario.budget = [{
    family: "FAM-A",
    week: 1,
    budgetInventoryValue: 8000,
    projectedInventoryValue: 9000,
    budgetInventoryVariance: 1000,
  }];
  poisonedSaveEvidence.scenario.inventoryFlow.points = poisonedSaveEvidence.scenario.inventoryFlow.points.slice(1);
  poisonedSaveEvidence.feasibility = null;
  runtime.context.__poisonedSaveEvidence = poisonedSaveEvidence;
  vm.runInContext(`
    state.preview = __poisonedSaveEvidence;
    showScenarioSavePanel(__poisonedSaveEvidence);
    __poisonedAdoption = evaluateAdoption(__poisonedSaveEvidence);
  `, runtime.context);
  assert.equal(runtime.elements.get("selector:#save-scenario").disabled, true,
    "a Complete envelope with one missing SKU-week must still disable scenario saving");
  assert.deepEqual(readVm(runtime, "__poisonedAdoption.status"), "Red",
    "partial physical key coverage must block adoption even when stale amounts are non-null");

  poisonedSaveEvidence.scenario.inventoryFlow.points = structuredClone(preview.scenario.inventoryFlow.points);
  poisonedSaveEvidence.baseline.inventoryFlow.points = poisonedSaveEvidence.baseline.inventoryFlow.points.slice(1);
  poisonedSaveEvidence.feasibility = null;
  runtime.context.__poisonedSaveEvidence = poisonedSaveEvidence;
  vm.runInContext(`
    showScenarioSavePanel(__poisonedSaveEvidence);
    __poisonedAdoption = evaluateAdoption(__poisonedSaveEvidence);
  `, runtime.context);
  assert.equal(runtime.elements.get("selector:#save-scenario").disabled, true,
    "partial baseline physical evidence must disable scenario saving");
  assert.deepEqual(readVm(runtime, "__poisonedAdoption.status"), "Red",
    "partial baseline physical evidence must block adoption as well as saving");

  poisonedSaveEvidence.baseline.inventoryFlow.points = structuredClone(preview.baseline.inventoryFlow.points);
  poisonedSaveEvidence.scenario.inventoryFlow.points = [
    ...structuredClone(preview.scenario.inventoryFlow.points),
    structuredClone(preview.scenario.inventoryFlow.points[0]),
  ];
  runtime.context.__poisonedSaveEvidence = poisonedSaveEvidence;
  vm.runInContext("showScenarioSavePanel(__poisonedSaveEvidence);", runtime.context);
  assert.equal(runtime.elements.get("selector:#save-scenario").disabled, true,
    "a Complete envelope with duplicate SKU-week evidence must disable scenario saving");

  const missingUpper = structuredClone(preview);
  missingUpper.scenario.bufferTrend.skuDetails.forEach(detail => detail.series.forEach(point => { delete point.physicalPosition; }));
  missingUpper.scenario.bufferTrend.series.forEach(point => { delete point.physicalPosition; });
  runPreview(runtime, missingUpper);
  const missingUpperMarkup = runtime.elements.get("buffer-trend-chart").innerHTML;
  assert.ok(missingUpperMarkup.includes("buffer-net-flow-line") && !missingUpperMarkup.includes("buffer-on-hand-line"),
    "missing physical position must leave NFP visible while omitting the upper on-hand path");
  assert.ok(runtime.elements.get("buffer-trend-kpis").innerHTML.includes("证据缺失"),
    "physical KPI strips must display missing evidence rather than zero when no physical positions remain");

  const partialUpper = structuredClone(preview);
  const partialDetail = partialUpper.scenario.bufferTrend.skuDetails.find(item =>
    item.sku === partialUpper.scenario.bufferTrend.selectedSku)
    ?? partialUpper.scenario.bufferTrend.skuDetails[0];
  const partialPoint = partialDetail.series[0];
  partialPoint.physicalPosition = null;
  partialPoint.inventoryValue = null;
  const partialSeriesPoint = partialUpper.scenario.bufferTrend.series.find(item =>
    item.sku === partialPoint.sku && item.week === partialPoint.week);
  partialSeriesPoint.physicalPosition = null;
  partialSeriesPoint.inventoryValue = null;
  const partialWeeklyCell = partialUpper.scenario.bufferTrend.weeklyCells.find(item =>
    item.sku === partialPoint.sku && item.week === partialPoint.week);
  partialWeeklyCell.inventoryValue = null;
  partialUpper.scenario.bufferTrend.comparison.averageInventoryValueDelta = 12345;
  partialUpper.scenario.bufferTrend.comparison.peakInventoryValueDelta = 23456;
  partialUpper.scenario.bufferTrend.comparison.physicalAverageInventoryValueDelta = 12345;
  partialUpper.scenario.bufferTrend.comparison.physicalPeakInventoryValueDelta = 23456;
  partialUpper.scenario.bufferTrend.comparison.physicalDeltaEvidenceStatus = "Complete";
  runPreview(runtime, partialUpper);
  const partialKpis = runtime.elements.get("buffer-trend-kpis").innerHTML;
  assert.match(partialKpis, /<span>平均库存金额<\/span><strong>证据缺失<\/strong>/,
    "one missing physical week must invalidate the aggregate average inventory amount");
  assert.match(partialKpis, /<span>峰值库存金额<\/span><strong>证据缺失<\/strong>/,
    "one missing physical week must invalidate the aggregate peak inventory amount");
  assert.match(partialKpis, /<span>在手红区 SKU<\/span><strong>证据缺失<\/strong>/,
    "one missing physical week must invalidate aggregate on-hand status KPIs");
  assert.ok(runtime.elements.get("buffer-family-summary-body").innerHTML.includes("证据缺失"),
    "one missing physical week must invalidate family inventory-value summaries");
  const partialComparison = runtime.elements.get("buffer-comparison-strip").innerHTML;
  assert.match(partialComparison, /<span>平均库存金额变化<\/span><strong>证据缺失<\/strong>/,
    "partial physical evidence must not expose a poisoned average inventory delta");
  assert.match(partialComparison, /<span>峰值库存金额变化<\/span><strong>证据缺失<\/strong>/,
    "partial physical evidence must not expose a poisoned peak inventory delta");

  const gap = structuredClone(preview);
  const gapWeek = gap.scenario.inventoryFlow.points.find(item => item.sku === gap.scenario.bufferTrend.selectedSku)?.week + 1;
  gap.scenario.inventoryFlow.points = gap.scenario.inventoryFlow.points.filter(item =>
    item.sku !== gap.scenario.bufferTrend.selectedSku || item.week !== gapWeek);
  runPreview(runtime, gap);
  const gapMarkup = runtime.elements.get("inventory-flow-chart").innerHTML;
  assert.ok(gapMarkup.includes("physical-evidence-gap") && gapMarkup.includes(`第 ${gapWeek} 周证据缺口`),
    "one missing backend week should render a labeled gap");
  const physicalSegments = [...gapMarkup.matchAll(/<path class="physical-on-hand-line"[^>]*data-week-from="(\d+)"[^>]*data-week-through="(\d+)"/g)];
  assert.ok(physicalSegments.every(match => !(Number(match[1]) < gapWeek && Number(match[2]) > gapWeek)),
    "no physical on-hand path may cross a missing week");

  const detailDomainGap = structuredClone(preview);
  const domainDetail = detailDomainGap.scenario.bufferTrend.skuDetails.find(item =>
    item.sku === detailDomainGap.scenario.bufferTrend.selectedSku)
    ?? detailDomainGap.scenario.bufferTrend.skuDetails[0];
  const sharedDomainWeeks = [...new Set(detailDomainGap.scenario.bufferTrend.series.map(item => Number(item.week)))]
    .filter(Number.isFinite)
    .sort((left, right) => left - right);
  const sharedWeekFrom = sharedDomainWeeks[0];
  const sharedWeekThrough = sharedDomainWeeks.at(-1);
  const missingDomainWeeks = new Set([sharedWeekFrom, sharedWeekThrough]);
  if (sharedDomainWeeks.length >= 6) {
    missingDomainWeeks.add(sharedDomainWeeks[2]);
    missingDomainWeeks.add(sharedDomainWeeks[3]);
  }
  const sharedDates = new Map(detailDomainGap.scenario.bufferTrend.series.map(item =>
    [Number(item.week), item.periodStartDate]));
  domainDetail.series = domainDetail.series.filter(item => !missingDomainWeeks.has(Number(item.week)));
  detailDomainGap.scenario.bufferTrend.series = detailDomainGap.scenario.bufferTrend.skuDetails.flatMap(item => item.series);
  runPreview(runtime, detailDomainGap);
  const domainPanels = [
    { id: "buffer-trend-chart", gapClass: "nfp-evidence-gap" },
    { id: "inventory-flow-chart", gapClass: "physical-evidence-gap" },
    { id: "buffer-volatility-chart", gapClass: "volatility-evidence-gap" },
  ].map(panel => ({ ...panel, markup: runtime.elements.get(panel.id).innerHTML }));
  for (const panel of domainPanels) {
    assert.ok(panel.markup.includes(`data-week-from="${sharedWeekFrom}"`)
      && panel.markup.includes(`data-week-through="${sharedWeekThrough}"`),
    `${panel.id} must retain the common explicit week domain when selected SKU evidence has gaps`);
  }
  for (const missingWeek of missingDomainWeeks) {
    const positions = domainPanels.map(panel => {
      const match = panel.markup.match(new RegExp(
        `<g class="${panel.gapClass}" data-week="${missingWeek}" data-x="([^"]+)"`));
      assert.ok(match, `${panel.id} should label gap week ${missingWeek} with its shared x coordinate`);
      assert.ok(panel.markup.includes(`第 ${missingWeek} 周证据缺口`),
        `${panel.id} should expose the Chinese gap label for week ${missingWeek}`);
      return Number(match[1]);
    });
    assert.ok(positions.every(position => Number.isFinite(position) && position === positions[0]),
      `gap week ${missingWeek} must use the same x coordinate in all three panels`);
  }
  for (const panel of domainPanels) {
    for (const week of sharedDomainWeeks) {
      assert.ok(panel.markup.includes(sharedDates.get(week)),
        `${panel.id} should retain the same-case backend date label for week ${week}`);
    }
    const segments = [...panel.markup.matchAll(/<(?:path|polyline)[^>]*data-week-from="(\d+)"[^>]*data-week-through="(\d+)"[^>]*>/g)]
      .filter(match => !match[0].includes('class="buffer-baseline-line"'));
    for (const missingWeek of missingDomainWeeks) {
      assert.ok(segments.every(match => !(Number(match[1]) < missingWeek && Number(match[2]) > missingWeek)),
        `${panel.id} must not draw a continuous line or area across gap week ${missingWeek}`);
    }
  }
  const nfpDomainMarkup = domainPanels.find(panel => panel.id === "buffer-trend-chart").markup;
  assert.ok(nfpDomainMarkup.includes(
    `class="buffer-baseline-line" data-field="endNetFlowAfterReplenishment" data-week-from="${sharedWeekFrom}" data-week-through="${sharedWeekThrough}"`),
  "complete baseline NFP evidence must remain continuous across gaps in the independently selected scenario");

  const legacy = structuredClone(preview);
  legacy.scenario.inventoryFlow.status = "EvidenceMissing";
  legacy.scenario.inventoryFlow.points = [];
  legacy.scenario.scenarioMetricEvidence = legacy.scenario.scenarioMetricEvidence.map(item => ({
    ...item,
    evidenceStatus: "EvidenceMissing",
    source: "LegacyReference",
  }));
  legacy.scenario.inventoryFlow.trace = [{
    stage: "LegacyResult",
    sku: null,
    week: null,
    sourceId: null,
    explanation: "历史结果没有物理投影",
  }];
  runPreview(runtime, legacy);
  assert.ok(runtime.elements.get("buffer-trend-chart").innerHTML.includes("buffer-net-flow-line"),
    "LegacyReference should keep original NFP evidence visible");
  assert.ok(!runtime.elements.get("inventory-flow-evidence").innerHTML.includes("physical-summary-metric"),
    "LegacyReference should hide physical metric labels");
  assert.ok(runtime.elements.get("inventory-flow-evidence").innerHTML.includes("历史兼容记录"),
    "LegacyReference should be explicitly identified");

  const noTrace = structuredClone(preview);
  noTrace.scenario.plan.traces = noTrace.scenario.plan.traces.filter(item =>
    item.sku !== noTrace.scenario.bufferTrend.selectedSku);
  runPreview(runtime, noTrace);
  assert.ok(runtime.elements.get("buffer-trace-list").innerHTML.includes("无可定位记录")
    && !runtime.elements.get("buffer-trace-list").innerHTML.includes("data-white-box-record="),
  "a selected SKU without a real plan trace must not expose an invented white-box link");

  const hostile = structuredClone(preview);
  const hostileDetail = hostile.scenario.bufferTrend.skuDetails[0];
  const originalSku = hostileDetail.sku;
  const hostileSku = 'SKU-X"><image href=x onerror=alert(1)>';
  hostileDetail.sku = hostileSku;
  hostileDetail.name = "名称</text><image href=x onerror=alert(2)>";
  hostileDetail.family = "产品族</button><img src=x onerror=alert(3)>";
  hostileDetail.series.forEach(item => {
    item.sku = hostileSku;
    item.periodStartDate = "</text><image href=x onerror=alert(4)>";
  });
  hostile.scenario.bufferTrend.selectedSku = hostileSku;
  hostile.scenario.bufferTrend.series.forEach(item => {
    if (item.sku === originalSku) {
      item.sku = hostileSku;
      item.periodStartDate = "</text><image href=x onerror=alert(4)>";
    }
  });
  hostile.scenario.bufferTrend.weeklyCells.forEach(item => {
    if (item.sku === originalSku) {
      item.sku = hostileSku;
      item.family = hostileDetail.family;
    }
  });
  hostile.scenario.inventoryFlow.points.forEach(item => {
    if (item.sku === originalSku) item.sku = hostileSku;
  });
  hostile.scenario.plan.traces.forEach(item => {
    if (item.sku === originalSku) item.sku = hostileSku;
  });
  vm.runInContext("state.filtered = null;", runtime.context);
  runPreview(runtime, hostile);
  const hostileHosts = [
    "buffer-trend-chart",
    "inventory-flow-chart",
    "buffer-volatility-chart",
    "buffer-inventory-options",
    "buffer-trend-heatmap",
    "buffer-sku-metadata",
    "buffer-trend-body",
    "buffer-family-summary-body",
    "buffer-trace-list",
  ];
  const hostileMarkup = hostileHosts.map(id => runtime.elements.get(id).innerHTML).join("\n");
  assert.ok(!/<(?:image|img|script)\b/i.test(hostileMarkup) && !hostileMarkup.includes("</text><image"),
    "future inventory panels, options, heatmap, and details must not emit hostile DTO markup");
  assert.ok(hostileMarkup.includes("&lt;image") && !hostileMarkup.includes("&amp;lt;image"),
    "hostile future inventory values should be escaped exactly once");

  console.log("13/13 future inventory flow chart fixture groups passed");
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const dtoPath = process.argv[2];
  const preview = dtoPath ? JSON.parse(await readFile(dtoPath, "utf8")) : makePreview();
  await runFutureInventoryFlowChartFixtures(preview);
}
