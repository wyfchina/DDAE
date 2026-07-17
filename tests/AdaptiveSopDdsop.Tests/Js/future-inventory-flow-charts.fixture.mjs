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
      setAttribute: (name, value) => attributes.set(name, String(value)),
      getAttribute: name => attributes.get(name) ?? null,
      removeAttribute: name => attributes.delete(name),
      addEventListener() {},
      removeEventListener() {},
      appendChild(child) {
        element.children.push(child);
        child.parentElement = element;
        return child;
      },
      remove() {},
      focus() {},
      click() {},
      scrollIntoView() {},
      querySelector: () => null,
      querySelectorAll: () => [],
      closest: () => null,
      contains: () => false,
      getBoundingClientRect: () => ({ top: 0, left: 0, width: 100, height: 20, bottom: 20, right: 100 }),
    };
    if (id) elements.set(id, element);
    return element;
  };
  const document = {
    readyState: "complete",
    getElementById: id => createElement(id),
    querySelector: selector => createElement(`selector:${selector}`),
    querySelectorAll: () => [],
    createElement: () => createElement(),
    addEventListener() {},
    removeEventListener() {},
    body: createElement("fixture-body"),
    documentElement: createElement("fixture-html"),
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
  return { context, elements };
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
    targetInventory: 74 + week,
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
  });
  return {
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
  return createHash("sha256").update(sourceFunctionBody(source, functionName)).digest("hex");
}

function runPreview(runtime, preview) {
  runtime.context.__previewFixture = structuredClone(preview);
  vm.runInContext("state.preview = __previewFixture; renderPreviewBufferTrend(__previewFixture);", runtime.context);
}

export async function runFutureInventoryFlowChartFixtures(preview, scriptPath = defaultScriptPath) {
  assert.ok(preview?.baseline?.bufferTrend && preview?.scenario?.bufferTrend,
    "fixture should contain backend NFP cases");
  assert.ok(preview?.scenario?.inventoryFlow, "fixture should contain backend physical flow");
  const source = await readFile(scriptPath, "utf8");
  new vm.Script(source, { filename: scriptPath });
  assert.equal(hashBody(source, "renderPreviewTrace"),
    "6883419526949ff4088a757ac695b2ca13866564d42b960fe31dd103973e3849",
    "renderPreviewTrace body must remain protected");
  assert.equal(hashBody(source, "renderTrace"),
    "b8701faf165b61269769cf4156941df521685468888a6c0d24f41367e5418adf",
    "renderTrace body must remain protected");

  const runtime = createRuntime(source);
  runPreview(runtime, preview);
  const nfpMarkup = runtime.elements.get("buffer-trend-chart").innerHTML;
  const physicalHost = runtime.elements.get("inventory-flow-chart");
  assert.ok(physicalHost, "physical chart host and renderer should exist");
  const physicalMarkup = physicalHost.innerHTML;
  const volatilityMarkup = runtime.elements.get("buffer-volatility-chart").innerHTML;
  assert.ok(nfpMarkup.includes("buffer-net-flow-line") && nfpMarkup.includes('data-case-id="scenario"'),
    "first panel should retain scenario NFP evidence");
  assert.ok(physicalMarkup.includes("physical-on-hand-line") && physicalMarkup.includes('data-case-id="scenario"'),
    "second panel should render backend scenario physical on-hand");
  assert.ok(volatilityMarkup.includes("buffer-demand-area") && volatilityMarkup.includes('data-case-id="scenario"'),
    "third panel should retain independent scenario volatility");
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
  const selectedSku = preview.scenario.bufferTrend.selectedSku
    ?? preview.scenario.bufferTrend.skuDetails[0].sku;
  assert.ok(runtime.elements.get("buffer-trace-list").innerHTML.includes('href="#trace-panel"')
    && runtime.elements.get("buffer-trace-list").innerHTML.includes(`scenario:${selectedSku}`),
  "selected detail should retain its corresponding white-box record link");

  runtime.context.__secondSku = preview.scenario.bufferTrend.skuDetails.find(item => item.sku !== selectedSku)?.sku
    ?? selectedSku;
  vm.runInContext(
    "state.futureInventorySelection.sku = __secondSku; state.selectedBufferSku = __secondSku; "
      + "renderSelectedFutureInventoryWorkspace();",
    runtime.context,
  );
  assert.ok(runtime.elements.get("buffer-trend-chart").innerHTML.includes(preview.scenario.bufferTrend.skuDetails[1].sku),
    "SKU selection should redraw NFP");
  assert.ok(runtime.elements.get("inventory-flow-chart").innerHTML.includes(preview.scenario.bufferTrend.skuDetails[1].sku),
    "SKU selection should redraw physical stock");
  assert.ok(runtime.elements.get("buffer-volatility-chart").innerHTML.includes(preview.scenario.bufferTrend.skuDetails[1].sku),
    "SKU selection should redraw volatility");

  vm.runInContext(
    "state.futureInventorySelection.caseId = 'baseline'; state.futureInventorySelection.weekFrom = 2; "
      + "state.futureInventorySelection.weekThrough = 3; renderSelectedFutureInventoryWorkspace();",
    runtime.context,
  );
  for (const id of ["buffer-trend-chart", "inventory-flow-chart", "buffer-volatility-chart"]) {
    const markup = runtime.elements.get(id).innerHTML;
    assert.ok(markup.includes('data-case-id="baseline"'), `${id} should redraw the selected case`);
    assert.ok(markup.includes('data-week-from="2"') && markup.includes('data-week-through="3"'),
      `${id} should redraw the shared selected week range`);
  }

  const missing = structuredClone(preview);
  missing.scenario.inventoryFlow.status = "EvidenceMissing";
  missing.scenario.inventoryFlow.points = [];
  missing.scenario.inventoryFlow.issues = [{
    scope: "InventoryFlow",
    sku: "SKU-A",
    week: 2,
    reason: "后端确认到货证据缺失",
    sourceId: null,
    blocksFreeze: true,
    blocksProjection: true,
  }];
  runPreview(runtime, missing);
  const missingMarkup = runtime.elements.get("inventory-flow-chart").innerHTML;
  assert.ok(!missingMarkup.includes("physical-on-hand-line"),
    "EvidenceMissing must draw no physical on-hand line");
  assert.ok(missingMarkup.includes("后端确认到货证据缺失"),
    "EvidenceMissing should expose the backend issue");

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

  const legacy = structuredClone(preview);
  legacy.scenario.inventoryFlow.status = "EvidenceMissing";
  legacy.scenario.inventoryFlow.points = [];
  legacy.scenario.scenarioMetricEvidence = legacy.scenario.scenarioMetricEvidence.map(item => ({
    ...item,
    evidenceStatus: "EvidenceMissing",
    source: "LegacyReference",
  }));
  runPreview(runtime, legacy);
  assert.ok(runtime.elements.get("buffer-trend-chart").innerHTML.includes("buffer-net-flow-line"),
    "LegacyReference should keep original NFP evidence visible");
  assert.ok(!runtime.elements.get("inventory-flow-evidence").innerHTML.includes("physical-summary-metric"),
    "LegacyReference should hide physical metric labels");
  assert.ok(runtime.elements.get("inventory-flow-evidence").innerHTML.includes("历史兼容记录"),
    "LegacyReference should be explicitly identified");

  console.log("6/6 future inventory flow chart fixture groups passed");
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const dtoPath = process.argv[2];
  const preview = dtoPath ? JSON.parse(await readFile(dtoPath, "utf8")) : makePreview();
  await runFutureInventoryFlowChartFixtures(preview);
}
