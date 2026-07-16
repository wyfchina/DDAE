import assert from "node:assert/strict";
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

function createElementStore() {
  const elements = new Map();
  const create = (id = "") => {
    if (id && elements.has(id)) return elements.get(id);
    const classes = new Set();
    const attributes = new Map();
    const element = {
      id,
      innerHTML: "",
      textContent: "",
      className: "",
      hidden: false,
      disabled: false,
      value: "",
      checked: false,
      dataset: {},
      style: {},
      children: [],
      parentElement: null,
      nextSibling: null,
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
  return { create, elements };
}

function createRuntime(source) {
  const { create, elements } = createElementStore();
  const documentEvents = [];
  const windowEvents = [];
  const document = {
    readyState: "complete",
    getElementById: id => create(id),
    querySelector: selector => create(`selector:${selector}`),
    querySelectorAll: () => [],
    createElement: () => create(),
    addEventListener: (type, handler) => documentEvents.push({ type, handler }),
    removeEventListener() {},
    body: create("fixture-body"),
    documentElement: create("fixture-html"),
  };
  const window = {
    document,
    location: { hash: "", pathname: "/", search: "" },
    history: { replaceState() {}, pushState() {} },
    addEventListener: (type, handler) => windowEvents.push({ type, handler }),
    removeEventListener() {},
    matchMedia: () => ({ matches: false, addEventListener() {}, removeEventListener() {} }),
    scrollTo() {},
  };
  let fetchImplementation = () => new Promise(() => {});
  const context = vm.createContext({
    console,
    document,
    window,
    location: window.location,
    history: window.history,
    navigator: { clipboard: { writeText: async () => {} } },
    fetch: (...args) => fetchImplementation(...args),
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
  return {
    context,
    documentEvents,
    elements,
    setFetch: implementation => { fetchImplementation = implementation; },
  };
}

function clickHistorySelector(runtime, selector, dataset) {
  const handler = [...runtime.documentEvents].reverse().find(entry =>
    entry.type === "click" && entry.handler.toString().includes("[data-history-range-months]"));
  assert.ok(handler, "delegated history click handler should exist");
  handler.handler({
    target: { closest: candidate => candidate === selector ? { dataset } : null },
  });
}

function rendererFixtureWithInventoryGap(historyReview) {
  const fixture = structuredClone(historyReview);
  const inventoryPoints = fixture.inventoryBuffers[0].points;
  const gap = inventoryPoints[Math.floor(inventoryPoints.length / 2)];
  Object.assign(gap, {
    endingOnHand: null,
    netFlow: null,
    topOfRed: null,
    topOfYellow: null,
    topOfGreen: null,
    evidenceStatus: "EvidenceMissing",
    cause: "历史证据缺口",
  });
  return fixture;
}

export async function runHistoryBufferRendererFixtures(historyReview, alternateHistoryReview, scriptPath = defaultScriptPath) {
  assert.ok(historyReview?.inventoryBuffers?.length, "real history DTO should include inventory buffers");
  assert.ok(historyReview?.timeBuffers?.length, "real history DTO should include time buffers");
  assert.ok(historyReview?.capacityBuffers?.length, "real history DTO should include capacity buffers");
  const source = await readFile(scriptPath, "utf8");
  new vm.Script(source, { filename: scriptPath });
  console.log("PASS app.js syntax compiles");

  const runtime = createRuntime(source);
  const fixture = rendererFixtureWithInventoryGap(historyReview);
  runtime.context.__historyFixture = fixture;
  vm.runInContext("renderHistoryReview(__historyFixture)", runtime.context);
  const standardInput = runtime.elements.get("history-standard-ddmrp-input-summary").innerHTML;
  const standardChart = runtime.elements.get("history-standard-ddmrp-zone-chart").innerHTML;
  const historicalInput = runtime.elements.get("history-ddmrp-input-summary").innerHTML;
  const sizingTable = runtime.elements.get("history-ddmrp-sizing-body").innerHTML;
  const zoneChart = runtime.elements.get("history-ddmrp-zone-chart").innerHTML;
  const inventoryChart = runtime.elements.get("history-inventory-chart").innerHTML;
  const timeChart = runtime.elements.get("history-time-buffer-chart").innerHTML;
  const capacityChart = runtime.elements.get("history-capacity-buffer-chart").innerHTML;
  assert.ok(standardInput.includes("ADU") && standardInput.includes("10") && standardInput.includes("DLT") && standardInput.includes("12"));
  assert.ok(standardInput.includes("后端计算") && standardInput.includes("DDAE 后端标准定容算例"));
  assert.ok(standardChart.includes("红区 80") && standardChart.includes("黄区 120") && standardChart.includes("绿区 70"));
  assert.ok(standardChart.includes("订货周期驱动"));
  assert.ok(historicalInput.includes("登记校验证据"), "historical source should localize registered validation evidence");
  assert.ok(!historicalInput.includes("registered validation data"), "historical source must not expose ordinary English wording");
  assert.ok(sizingTable.includes("生效") || zoneChart.includes("生效周段"), "historical snapshot evidence should remain visible");
  assert.ok(zoneChart.includes("生效周段") && zoneChart.includes("证据"));
  assert.ok((inventoryChart.match(/history-series-line is-on-hand/g) || []).length >= 2, "inventory gap should split the line");
  assert.ok(inventoryChart.includes("证据缺失"), "missing inventory evidence should not become zero");
  assert.ok(["is-early", "is-green", "is-yellow", "is-red", "is-late"].every(css => timeChart.includes(css)));
  const realCostWeeks = fixture.timeBuffers[0].points
    .filter(point => point.abnormalCost !== null && point.abnormalCost !== undefined)
    .map(point => point.weekOffset);
  assert.deepEqual(realCostWeeks, [-18], "real six-month DTO should retain its single linked abnormal-cost week");
  const costMarkerWeeks = [...timeChart.matchAll(/<circle class="history-cost-marker" data-week-offset="(-?\d+)"/g)]
    .map(match => Number(match[1]));
  assert.deepEqual(costMarkerWeeks, realCostWeeks, "every real abnormal-cost point should have exactly one visible marker");
  assert.equal(fixture.timeBuffers[0].points.find(point => point.weekOffset === -17)?.abnormalCost, null,
    "week -17 should remain missing cost evidence");
  assert.ok(!costMarkerWeeks.includes(-17), "missing week -17 must not be plotted as zero");
  assert.ok(!timeChart.includes("history-cost-line"), "one isolated abnormal-cost point must not create a cross-gap line");
  assert.ok(["is-theoretical", "is-standard", "is-demonstrated", "is-planned", "is-protection-start"].every(css => capacityChart.includes(css)));
  assert.ok(capacityChart.includes("is-consumed-protection"), "AIT should show upstream protection consumption");
  assert.ok(runtime.elements.get("history-capacity-resource-options").innerHTML.includes("CCR 利用率参照"));
  const controlPointOptions = runtime.elements.get("history-inventory-control-point-options").innerHTML;
  assert.ok(controlPointOptions.includes("关键进口 FPGA 独立库存控制点"));
  assert.ok(controlPointOptions.includes('data-history-control-point="关键进口 FPGA 库存控制点"'),
    "FPGA selector data attribute should retain the source control-point value");
  const protectionTable = runtime.elements.get("history-protection-body").innerHTML;
  assert.ok(protectionTable.includes("关键进口 FPGA 独立库存控制点"),
    "protection table should use the independent-inventory display label");
  assert.ok(!protectionTable.includes("关键进口 FPGA 库存控制点"),
    "protection table must not expose the old source name as a visible label");
  const gapSegments = vm.runInContext("contiguousEvidenceSegments([{ value: 1 }, { value: null }, { value: 2 }], point => point.value !== null).map(segment => segment.map(point => point.value))", runtime.context);
  assert.equal(JSON.stringify(gapSegments), "[[1],[2]]");
  console.log("PASS backend sizing, gaps, five time bands, and capacity layers render");

  assert.ok(alternateHistoryReview?.standardDdmrpReference?.sizing?.zones,
    "alternate history DTO should include backend sizing evidence");
  const baseZones = historyReview.standardDdmrpReference.sizing.zones;
  const alternateZones = alternateHistoryReview.standardDdmrpReference.sizing.zones;
  assert.notDeepEqual(
    [alternateZones.red, alternateZones.yellow, alternateZones.green],
    [baseZones.red, baseZones.yellow, baseZones.green],
    "alternate backend sizing must differ from the standard 80/120/70 reference");
  const alternateRuntime = createRuntime(source);
  alternateRuntime.context.__historyFixture = structuredClone(alternateHistoryReview);
  vm.runInContext("renderHistoryReview(__historyFixture)", alternateRuntime.context);
  const alternateInput = alternateRuntime.elements.get("history-standard-ddmrp-input-summary").innerHTML;
  const alternateChart = alternateRuntime.elements.get("history-standard-ddmrp-zone-chart").innerHTML;
  assert.ok(alternateInput.includes(`>${alternateHistoryReview.standardDdmrpReference.setting.adu}<`),
    "standard input should follow the alternate backend ADU");
  assert.ok(alternateChart.includes(`红区 ${alternateZones.red}`));
  assert.ok(alternateChart.includes(`黄区 ${alternateZones.yellow}`));
  assert.ok(alternateChart.includes(`绿区 ${alternateZones.green}`));
  assert.ok(!alternateChart.includes(`红区 ${baseZones.red}`), "alternate chart must not retain the base red-zone result");
  console.log("PASS alternate backend sizing drives standard renderer");

  const missingStandardRuntime = createRuntime(source);
  const missingStandardFixture = structuredClone(historyReview);
  missingStandardFixture.standardDdmrpReference.sizing.zones.red = null;
  missingStandardRuntime.context.__historyFixture = missingStandardFixture;
  vm.runInContext("renderHistoryReview(__historyFixture)", missingStandardRuntime.context);
  const missingStandardChart = missingStandardRuntime.elements.get("history-standard-ddmrp-zone-chart").innerHTML;
  assert.ok(missingStandardChart.includes("证据缺失"), "partial standard sizing must render missing evidence");
  assert.ok(!missingStandardChart.includes("红区 0"), "missing standard sizing must not render a zero substitute");
  assert.ok(!missingStandardChart.includes("history-standard-zone-stack"), "partial standard sizing must not render a valid zone stack");
  console.log("PASS partial standard sizing stays missing instead of zero");

  clickHistorySelector(runtime, "[data-history-control-point]", { historyControlPoint: "关键进口 FPGA 库存控制点" });
  const fpgaSelection = vm.runInContext("({ controlPoint: state.selectedHistoryControlPoint, sku: state.selectedHistoryInventorySku, snapshot: state.selectedHistorySizingSnapshot })", runtime.context);
  assert.equal(fpgaSelection.controlPoint, "关键进口 FPGA 库存控制点", "source control-point value should remain unchanged");
  assert.equal(fpgaSelection.sku, "AV-FPGA-203");
  assert.ok(fpgaSelection.snapshot);
  assert.ok(runtime.elements.get("history-inventory-chart").innerHTML.includes("关键进口 FPGA 独立库存控制点"));
  clickHistorySelector(runtime, "[data-history-capacity-resource]", { historyCapacityResource: "RES-HARNESS" });
  const harnessChart = runtime.elements.get("history-capacity-buffer-chart").innerHTML;
  assert.ok(harnessChart.includes("CCR 利用率参照"));
  assert.ok(!harnessChart.includes("is-consumed-protection"), "HARNESS must not show protective consumption");
  console.log("PASS delegated selectors preserve FPGA source and keep HARNESS reference-only");

  let baselineUrl = null;
  runtime.setFetch(async url => {
    baselineUrl = url;
    return {
      ok: true,
      status: 200,
      json: async () => ({
        snapshotNumber: "BL-LEGACY-001",
        status: "Frozen",
        payload: { planningInputs: { ddmrpParameters: [
          { sku: "LEGACY-001", name: "旧版物料", leadTimeFactor: null, evidenceStatus: "EvidenceMissing" },
        ] } },
      }),
    };
  });
  await vm.runInContext("openBaselineSnapshotDetail('legacy-id')", runtime.context);
  assert.equal(baselineUrl, "/api/current-baselines/legacy-id");
  const drawer = runtime.elements.get("workspace-drawer-body").innerHTML;
  assert.ok(drawer.includes("旧版本缺少提前期因子；该快照保持只读，不能用于重算"));
  console.log("PASS legacy frozen snapshot stays read-only and visibly incomplete");

  const missingRuntime = createRuntime(source);
  const emptyFixture = structuredClone(historyReview);
  emptyFixture.inventoryBuffers = [];
  emptyFixture.ddmrpSizingSnapshots = [];
  emptyFixture.timeBuffers = [];
  emptyFixture.capacityBuffers = [];
  missingRuntime.context.__historyFixture = emptyFixture;
  vm.runInContext("renderHistoryReview(__historyFixture)", missingRuntime.context);
  for (const hostId of [
    "history-inventory-chart",
    "history-ddmrp-zone-chart",
    "history-time-buffer-chart",
    "history-capacity-buffer-chart",
  ]) {
    const markup = missingRuntime.elements.get(hostId).innerHTML;
    assert.ok(markup.includes("证据缺失"), `${hostId} should expose empty evidence`);
    assert.ok(!markup.includes("<svg"), `${hostId} should not draw an empty SVG`);
  }

  const allMissingRuntime = createRuntime(source);
  const allMissingFixture = structuredClone(historyReview);
  allMissingFixture.inventoryBuffers = [allMissingFixture.inventoryBuffers[0]];
  allMissingFixture.inventoryBuffers[0].points = allMissingFixture.inventoryBuffers[0].points.map(point => ({
    ...point,
    endingOnHand: null,
    netFlow: null,
    topOfRed: null,
    topOfYellow: null,
    topOfGreen: null,
    evidenceStatus: "EvidenceMissing",
  }));
  allMissingFixture.ddmrpSizingSnapshots = [allMissingFixture.ddmrpSizingSnapshots[0]];
  allMissingFixture.ddmrpSizingSnapshots[0].sizing = null;
  allMissingFixture.ddmrpSizingSnapshots[0].sizingLines = [];
  allMissingFixture.timeBuffers = [allMissingFixture.timeBuffers[0]];
  allMissingFixture.timeBuffers[0].points = allMissingFixture.timeBuffers[0].points.map(point => ({
    ...point,
    earlyCount: null,
    greenCount: null,
    yellowCount: null,
    redCount: null,
    lateCount: null,
    abnormalCost: null,
    evidenceStatus: "EvidenceMissing",
  }));
  allMissingFixture.capacityBuffers = [allMissingFixture.capacityBuffers[0]];
  allMissingFixture.capacityBuffers[0].points = allMissingFixture.capacityBuffers[0].points.map(point => ({
    ...point,
    committedLoad: null,
    theoreticalCapacity: null,
    standardCapacity: null,
    demonstratedCapacity: null,
    plannedAvailableCapacity: null,
    protectionStart: null,
    consumedProtection: null,
    evidenceStatus: "EvidenceMissing",
  }));
  allMissingRuntime.context.__historyFixture = allMissingFixture;
  vm.runInContext("renderHistoryReview(__historyFixture)", allMissingRuntime.context);
  for (const hostId of [
    "history-inventory-chart",
    "history-ddmrp-zone-chart",
    "history-time-buffer-chart",
    "history-capacity-buffer-chart",
  ]) {
    const markup = allMissingRuntime.elements.get(hostId).innerHTML;
    assert.ok(markup.includes("证据缺失"), `${hostId} should expose all-missing evidence`);
    assert.ok(!markup.includes("<svg"), `${hostId} should not draw an all-missing SVG`);
  }
  console.log("PASS empty collections and all-missing points render no SVG");

  const maliciousRuntime = createRuntime(source);
  maliciousRuntime.context.__maliciousParameter = {
    sku: "MAL-<SKU&",
    name: "危险<名称&",
    family: "测试",
    decouplingPoint: "测试",
    bufferProfile: "测试",
    parameterStatus: "Current",
    completenessStatus: "Complete",
    sizingLines: [{ component: "分区<标签&", formula: "后端证据", value: 1, explanation: "完整" }],
  };
  vm.runInContext("state.data = { ddmrpParameters: [__maliciousParameter] }; renderDdmrpParameterDetail(__maliciousParameter.sku)", maliciousRuntime.context);
  const parameterDrawer = maliciousRuntime.elements.get("workspace-drawer-body").innerHTML;
  assert.ok(parameterDrawer.includes("分区&lt;标签&amp;"), "parameter label should be escaped exactly once");
  assert.ok(!parameterDrawer.includes("分区&amp;lt;标签&amp;amp;"), "parameter label must not be double escaped");

  maliciousRuntime.setFetch(async () => ({
    ok: true,
    status: 200,
    json: async () => ({
      snapshotNumber: "BL-UNSAFE",
      status: "Frozen",
      payload: { planningInputs: { ddmrpParameters: [
        { sku: "LEGACY-<SKU&", name: "旧版<名称&", leadTimeFactor: null, evidenceStatus: "EvidenceMissing" },
      ] } },
    }),
  }));
  await vm.runInContext("openBaselineSnapshotDetail('unsafe-id')", maliciousRuntime.context);
  const baselineDrawer = maliciousRuntime.elements.get("workspace-drawer-body").innerHTML;
  assert.ok(baselineDrawer.includes("LEGACY-&lt;SKU&amp; 旧版&lt;名称&amp;"), "baseline label should be escaped exactly once");
  assert.ok(!baselineDrawer.includes("LEGACY-&amp;lt;SKU&amp;amp;"), "baseline label must not be double escaped");
  console.log("PASS drawer labels escape malicious text exactly once");

  console.log("8/8 renderer fixture groups passed");
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const dtoPath = process.argv[2];
  const alternateDtoPath = process.argv[3];
  assert.ok(dtoPath, "expected a serialized history-review DTO path");
  assert.ok(alternateDtoPath, "expected an alternate serialized history-review DTO path");
  const historyReview = JSON.parse(await readFile(dtoPath, "utf8"));
  const alternateHistoryReview = JSON.parse(await readFile(alternateDtoPath, "utf8"));
  await runHistoryBufferRendererFixtures(historyReview, alternateHistoryReview);
}
