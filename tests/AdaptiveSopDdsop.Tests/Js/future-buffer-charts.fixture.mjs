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

function makeStandaloneBufferTrend() {
  const makeDetail = (sku, name, offset, replenishmentWeek) => {
    const series = Array.from({ length: 12 }, (_, index) => {
      const week = index + 1;
      const topOfRed = 38 + offset + week;
      const topOfYellow = topOfRed + 34 + (week % 3);
      const topOfGreen = topOfYellow + 42 + (week % 4);
      const endNetFlowBeforeReplenishment = topOfYellow - 18 + (week % 5) * 7;
      const replenishmentQuantity = week === replenishmentWeek ? 36 + offset : 0;
      const status = endNetFlowBeforeReplenishment <= topOfRed
        ? "Red"
        : endNetFlowBeforeReplenishment <= topOfYellow ? "Yellow" : "Green";
      const periodStartDate = new Date(Date.UTC(2026, 5, 1 + index * 7)).toISOString().slice(0, 10);
      return {
        sku,
        week,
        periodStartDate,
        timePhasedAdu: 9 + offset / 10 + (week % 4),
        startNetFlow: topOfYellow + 15 - week,
        demand: 16 + offset / 5 + (week * 7) % 17,
        endNetFlowBeforeReplenishment,
        endNetFlowAfterReplenishment: endNetFlowBeforeReplenishment + replenishmentQuantity,
        topOfRed,
        topOfYellow,
        topOfGreen,
        inventoryValue: (72 + offset - week) * 100,
        replenishmentQuantity,
        isReplenishment: replenishmentQuantity > 0,
        isPrebuild: false,
        status,
        demandSpikeThreshold: 38 + offset / 4,
        physicalPosition: {
          endingOnHand: 72 + offset - week,
          endingBacklog: week === 11 ? 3 : 0,
          onHandStatus: week >= 10 ? "Yellow" : "Green",
          evidenceStatus: "Complete",
          source: "InventoryFlowProjection",
        },
      };
    });
    return {
      sku,
      name,
      family: "独立测试产品族",
      adu: 10 + offset / 10,
      decoupledLeadTimeDays: 14 + offset / 2,
      minimumOrderQuantity: 12 + offset,
      orderCycleDays: 7,
      unitCost: 100,
      zone: {
        topOfRed: series[0].topOfRed,
        topOfYellow: series[0].topOfYellow,
        topOfGreen: series[0].topOfGreen,
      },
      series,
      replenishmentOrders: [{
        sku,
        week: replenishmentWeek,
        quantity: 36 + offset,
        value: (36 + offset) * 100,
        trigger: "OrderCycle",
      }],
      traces: [{ sku, week: replenishmentWeek, explanation: `${sku} standalone replenishment trace` }],
      activities: [],
      attributes: [],
      bufferSizing: [],
      bom: [],
      orderDetails: [],
    };
  };

  const skuDetails = [
    makeDetail("STANDALONE-A", "独立测试物料 A", 0, 4),
    makeDetail("STANDALONE-B", "独立测试物料 B", 18, 7),
  ];
  const series = skuDetails.flatMap(detail => detail.series);
  return {
    caseId: "standalone-buffer-fixture",
    name: "独立缓冲图测试",
    horizonWeeks: 12,
    selectedSku: "STANDALONE-A",
    kpis: {
      redSkuCount: 0,
      yellowSkuCount: 2,
      shortageCount: 0,
      averageInventoryValue: 0,
      peakInventoryValue: 0,
      replenishmentOrderCount: 2,
      inventoryValueDelta: 0,
      onHandRedSkuCount: 0,
      onHandYellowSkuCount: 2,
      onHandStockoutWeekCount: 2,
    },
    series,
    zoneBands: skuDetails.map(detail => ({ sku: detail.sku, ...detail.zone })),
    comparison: {
      averageInventoryValueDelta: 0,
      peakInventoryValueDelta: 0,
      redWeekDelta: 0,
      replenishmentOrderCountDelta: 0,
      replenishmentQuantityDelta: 0,
    },
    familySummaries: [],
    weeklyCells: series.map(point => ({
      sku: point.sku,
      family: "独立测试产品族",
      week: point.week,
      inventoryValue: point.inventoryValue,
      status: point.status,
    })),
    skuDetails,
  };
}

function cubicValue(start, firstControl, secondControl, end, t) {
  const oneMinus = 1 - t;
  return oneMinus ** 3 * start
    + 3 * oneMinus ** 2 * t * firstControl
    + 3 * oneMinus * t ** 2 * secondControl
    + t ** 3 * end;
}

function assertNoOvershoot(parts, label) {
  for (const segment of parts.segments) {
    const minimum = Math.min(segment.start.y, segment.end.y) - 1e-7;
    const maximum = Math.max(segment.start.y, segment.end.y) + 1e-7;
    for (let step = 0; step <= 100; step += 1) {
      const value = cubicValue(
        segment.start.y,
        segment.firstControl.y,
        segment.secondControl.y,
        segment.end.y,
        step / 100,
      );
      assert.ok(value >= minimum && value <= maximum, `${label} overshot its backend interval`);
    }
  }
}

function sampleSegment(segment, t) {
  return cubicValue(
    segment.start.y,
    segment.firstControl.y,
    segment.secondControl.y,
    segment.end.y,
    t,
  );
}

function sampleAdaptiveSegment(segment, t, useLinear) {
  if (useLinear) return segment.start.y + (segment.end.y - segment.start.y) * t;
  return sampleSegment(segment, t);
}

export async function runFutureBufferChartFixtures(trend, scriptPath = defaultScriptPath) {
  assert.ok(trend?.skuDetails?.length, "real backend DTO should contain future buffer details");
  const source = await readFile(scriptPath, "utf8");
  new vm.Script(source, { filename: scriptPath });
  const runtime = createRuntime(source);
  runtime.context.__trendFixture = structuredClone(trend);
  vm.runInContext(
    "state.bufferTrend = __trendFixture; state.baselineBufferTrend = __trendFixture; "
      + "state.selectedBufferSku = __trendFixture.selectedSku; renderBufferTrendWorkspace(__trendFixture);",
    runtime.context,
  );

  const upperMarkup = runtime.elements.get("buffer-trend-chart").innerHTML;
  const lowerMarkup = runtime.elements.get("buffer-volatility-chart").innerHTML;
  const renderedZonePaths = [...upperMarkup.matchAll(/<path class="buffer-zone-[^"]+" d="([^"]+)"/g)]
    .map(match => match[1]);
  assert.equal(renderedZonePaths.length, 3,
    "upper chart should contain three smooth stacked buffer paths");
  assert.ok(renderedZonePaths.every(pathValue => pathValue.includes("C ")),
    "real backend zone bands should retain smooth cubic segments");
  assert.ok(!upperMarkup.includes("demand-pulse-bar"), "upper chart must not contain demand pulse bars");
  assert.ok(!upperMarkup.includes("order-spike"), "upper chart must not contain a demand threshold");
  assert.ok(!upperMarkup.includes("需求脉冲"), "upper chart must not contain pulse wording");
  assert.ok(upperMarkup.includes("buffer-net-flow-line"), "upper chart should retain net flow");
  assert.ok(upperMarkup.includes("buffer-on-hand-line"), "upper chart should render backend physical on-hand evidence");
  assert.ok(upperMarkup.includes('data-field="physicalPosition.endingOnHand"'),
    "upper on-hand line must map only the physical-position field");
  assert.ok(!upperMarkup.includes("target-inventory-dot") && !upperMarkup.includes("目标库存"),
    "upper chart must not retain target inventory markers or legend");
  assert.ok(upperMarkup.includes("buffer-week-label"), "upper chart should retain dates");
  if (trend.skuDetails.some(item => item.series.some(point => point.isReplenishment))) {
    const detailWithOrder = trend.skuDetails.find(item => item.series.some(point => point.isReplenishment));
    runtime.context.__detailWithOrder = detailWithOrder;
    vm.runInContext("renderBufferTrendChart(__detailWithOrder)", runtime.context);
    assert.ok(runtime.elements.get("buffer-trend-chart").innerHTML.includes("review-marker"),
      "upper chart should retain replenishment markers");
  }
  assert.ok(lowerMarkup.includes('viewBox="0 0 940 190"'), "lower chart should use an independent 940 by 190 view box");
  assert.ok(lowerMarkup.includes("buffer-demand-area"), "lower chart should contain planned-demand area");
  assert.ok(/<path class="buffer-demand-area" d="[^"]*C /.test(lowerMarkup),
    "real planned demand should render as a smooth area");
  assert.ok(lowerMarkup.includes("buffer-demand-threshold"), "lower chart should contain the backend threshold line");
  assert.ok(!lowerMarkup.includes("buffer-zone-"), "lower chart must not contain buffer zones");
  assert.ok(!lowerMarkup.includes("buffer-net-flow-line"), "lower chart must not contain net flow");

  const detail = trend.skuDetails.find(item => item.sku === trend.selectedSku) ?? trend.skuDetails[0];
  const points = detail.series.map((item, index) => ({ x: index, y: Number(item.topOfGreen) }));
  runtime.context.__points = points;
  const path = vm.runInContext("buildMonotonePath(__points)", runtime.context);
  assert.ok(path && !/NaN|Infinity/.test(path), "monotone line should contain only finite geometry");
  for (const point of points) {
    assert.ok(path.includes(`${point.x},${point.y}`), "monotone line should pass every backend point");
  }
  const parts = vm.runInContext("structuredClone(monotonePathParts(__points))", runtime.context);
  assertNoOvershoot(parts, "green boundary");
  assert.equal(vm.runInContext("buildMonotonePath([{ x: 0, y: Number.NaN }])", runtime.context), "",
    "invalid geometry should produce no path");

  const boundaries = ["topOfRed", "topOfYellow", "topOfGreen"].map(field => {
    runtime.context.__boundary = detail.series.map((item, index) => ({ x: index, y: Number(item[field]) }));
    return vm.runInContext("structuredClone(monotonePathParts(__boundary))", runtime.context);
  });
  for (let segmentIndex = 0; segmentIndex < boundaries[0].segments.length; segmentIndex += 1) {
    for (let step = 0; step <= 100; step += 1) {
      const t = step / 100;
      const red = sampleSegment(boundaries[0].segments[segmentIndex], t);
      const yellow = sampleSegment(boundaries[1].segments[segmentIndex], t);
      const green = sampleSegment(boundaries[2].segments[segmentIndex], t);
      assert.ok(red <= yellow + 1e-7 && yellow <= green + 1e-7,
        "smoothed backend buffer boundaries must not cross");
    }
  }

  runtime.context.__lowerCounterexample = [
    { x: 0, y: -100 },
    { x: 1, y: -100 },
    { x: 2, y: -105 },
  ];
  runtime.context.__upperCounterexample = [
    { x: 0, y: -115 },
    { x: 1, y: -101 },
    { x: 2, y: -106 },
  ];
  const crossingSegments = vm.runInContext(
    "[...monotoneCrossingSegments(__lowerCounterexample, __upperCounterexample)]",
    runtime.context,
  );
  assert.ok(crossingSegments.includes(1), "known independent-PCHIP crossing should trigger a linear segment fallback");
  const counterexamplePath = vm.runInContext(
    "buildMonotoneAreaPath(__lowerCounterexample, __upperCounterexample, new Set(monotoneCrossingSegments(__lowerCounterexample, __upperCounterexample)))",
    runtime.context,
  );
  assert.ok(counterexamplePath && !/NaN|Infinity/.test(counterexamplePath), "fallback area should stay finite");
  const lowerCounterexampleParts = vm.runInContext(
    "structuredClone(monotonePathParts(__lowerCounterexample))",
    runtime.context,
  );
  const upperCounterexampleParts = vm.runInContext(
    "structuredClone(monotonePathParts(__upperCounterexample))",
    runtime.context,
  );
  for (let segmentIndex = 0; segmentIndex < lowerCounterexampleParts.segments.length; segmentIndex += 1) {
    for (let step = 0; step <= 100; step += 1) {
      const t = step / 100;
      const lower = sampleAdaptiveSegment(
        lowerCounterexampleParts.segments[segmentIndex],
        t,
        crossingSegments.includes(segmentIndex),
      );
      const upper = sampleAdaptiveSegment(
        upperCounterexampleParts.segments[segmentIndex],
        t,
        crossingSegments.includes(segmentIndex),
      );
      assert.ok(upper <= lower + 1e-7, "adaptive area geometry should not invert between backend points");
    }
  }

  runtime.context.__singleLower = [{ x: 0, y: 10 }];
  runtime.context.__singleUpper = [{ x: 0, y: 5 }];
  assert.ok(vm.runInContext("buildMonotoneAreaPath(__singleLower, __singleUpper)", runtime.context).endsWith("Z"),
    "a single finite backend point should produce a closed degenerate area");
  assert.equal(vm.runInContext("buildMonotonePath([])", runtime.context), "", "an empty line should produce no path");
  assert.equal(vm.runInContext("buildMonotoneAreaPath([], [])", runtime.context), "", "an empty area should produce no path");
  const twoPointPath = vm.runInContext(
    "buildMonotonePath([{ x: 0, y: 1 }, { x: 2, y: 3 }])",
    runtime.context,
  );
  assert.ok(twoPointPath.startsWith("M 0,1 C ") && twoPointPath.endsWith("2,3"),
    "two points should produce one finite cubic that reaches the second backend point");
  assert.equal(vm.runInContext("buildMonotonePath([{ x: 0, y: 1 }, { x: 0, y: 2 }])", runtime.context), "",
    "duplicate x values should produce no path");
  assert.equal(vm.runInContext("buildMonotonePath([{ x: 1, y: 1 }, { x: 0, y: 2 }])", runtime.context), "",
    "decreasing x values should produce no path");
  assert.equal(vm.runInContext("buildMonotonePath([{ x: 0, y: Infinity }])", runtime.context), "",
    "infinite coordinates should produce no path");
  assert.equal(vm.runInContext("buildMonotonePath([{ x: null, y: 1 }])", runtime.context), "",
    "null coordinates must not be coerced to zero");
  assert.equal(vm.runInContext("buildMonotonePath([{ x: '  ', y: 1 }])", runtime.context), "",
    "blank coordinates must not be coerced to zero");
  assert.equal(vm.runInContext(
    "buildMonotoneAreaPath([{ x: 0, y: 2 }], [{ x: 1, y: 1 }])",
    runtime.context,
  ), "", "mismatched lower and upper x values should produce no area");
  assert.equal(vm.runInContext(
    "buildMonotoneAreaPath([{ x: 0, y: 2 }, { x: 1, y: 2 }], [{ x: 0, y: 1 }])",
    runtime.context,
  ), "", "different lower and upper lengths should produce no area");

  const singletonZone = structuredClone(detail);
  const singletonZoneIndex = 3;
  const singletonZonePoint = structuredClone(singletonZone.series[singletonZoneIndex]);
  singletonZone.series = singletonZone.series.map((item, index) => index === singletonZoneIndex
    ? item
    : { ...item, topOfRed: null, topOfYellow: null, topOfGreen: null });
  runtime.context.__singletonZone = singletonZone;
  vm.runInContext("renderBufferTrendChart(__singletonZone)", runtime.context);
  const singletonZoneMarkup = runtime.elements.get("buffer-trend-chart").innerHTML;
  assert.ok(!singletonZoneMarkup.includes('<path class="buffer-zone-'),
    "one valid zone week should not masquerade as a visible stacked area");
  assert.equal((singletonZoneMarkup.match(/buffer-zone-evidence-marker/g) || []).length, 3,
    "one valid zone week should expose exactly three visible boundary markers");
  for (const [cssClass, label, value] of [
    ["is-red", "红区上沿", singletonZonePoint.topOfRed],
    ["is-yellow", "黄区上沿", singletonZonePoint.topOfYellow],
    ["is-green", "绿区上沿", singletonZonePoint.topOfGreen],
  ]) {
    assert.ok(singletonZoneMarkup.includes(`buffer-zone-evidence-marker ${cssClass}`),
      `${label} should have a visible singleton marker`);
    assert.ok(singletonZoneMarkup.includes(`${singletonZonePoint.periodStartDate} ${label}：${value}`),
      `${label} marker should identify its backend period and value`);
  }
  assert.ok(singletonZoneMarkup.includes("缓冲区证据缺失"),
    "singleton zone evidence should retain the surrounding gap warning");

  const missingZone = structuredClone(detail);
  const missingZoneIndex = Math.floor(missingZone.series.length / 2);
  missingZone.series[missingZoneIndex] = {
    ...missingZone.series[missingZoneIndex],
    topOfRed: null,
    topOfYellow: null,
    topOfGreen: null,
  };
  runtime.context.__missingZone = missingZone;
  vm.runInContext("renderBufferTrendChart(__missingZone)", runtime.context);
  const missingZoneMarkup = runtime.elements.get("buffer-trend-chart").innerHTML;
  assert.equal((missingZoneMarkup.match(/<path class="buffer-zone-/g) || []).length, 6,
    "one missing zone week should split every stacked area instead of bridging or zero-filling it");
  assert.ok(missingZoneMarkup.includes("缓冲区证据缺失"),
    "missing zone evidence should produce a visible Chinese warning");
  assert.ok(!/NaN|Infinity/.test(missingZoneMarkup), "missing zone evidence should not create invalid SVG geometry");

  const invalidZone = structuredClone(detail);
  const invalidZoneIndexes = [2, 5, 8];
  invalidZone.series[invalidZoneIndexes[0]].topOfRed = "  ";
  invalidZone.series[invalidZoneIndexes[1]].topOfYellow = Number.NaN;
  invalidZone.series[invalidZoneIndexes[2]].topOfGreen = Number.POSITIVE_INFINITY;
  runtime.context.__invalidZone = invalidZone;
  vm.runInContext("renderBufferTrendChart(__invalidZone)", runtime.context);
  const invalidZoneMarkup = runtime.elements.get("buffer-trend-chart").innerHTML;
  assert.equal((invalidZoneMarkup.match(/<path class="buffer-zone-/g) || []).length, 12,
    "blank, NaN, and infinite zone weeks should each split all three stacked areas");
  assert.ok(invalidZoneMarkup.includes("缓冲区证据缺失"),
    "invalid zone values should remain visible as missing evidence");
  assert.ok(!/NaN|Infinity/.test(invalidZoneMarkup),
    "invalid zone values must not enter SVG geometry");
  const chartX = index => 62 + index * (940 - 62 - 26) / Math.max(1, detail.series.length - 1);
  const invalidZonePaths = [...invalidZoneMarkup.matchAll(/<path class="buffer-zone-[^"]+" d="([^"]+)"/g)]
    .map(match => match[1]);
  for (const index of invalidZoneIndexes) {
    assert.ok(invalidZonePaths.every(pathValue => !pathValue.includes(`${chartX(index)},`)),
      `invalid zone week ${index} must not enter a stacked-area endpoint`);
  }

  const invalidDemand = structuredClone(detail);
  const invalidDemandIndexes = [2, 5, 8];
  invalidDemand.series[invalidDemandIndexes[0]].demand = "";
  invalidDemand.series[invalidDemandIndexes[1]].demand = Number.NaN;
  invalidDemand.series[invalidDemandIndexes[2]].demand = Number.NEGATIVE_INFINITY;
  runtime.context.__invalidDemand = invalidDemand;
  vm.runInContext("renderBufferVolatilityChart(__invalidDemand)", runtime.context);
  const invalidDemandMarkup = runtime.elements.get("buffer-volatility-chart").innerHTML;
  assert.equal((invalidDemandMarkup.match(/buffer-demand-marker/g) || []).length,
    invalidDemand.series.length - invalidDemandIndexes.length,
    "blank, NaN, and infinite demand values must not create zero or invalid markers");
  assert.equal((invalidDemandMarkup.match(/<path class="buffer-demand-area"/g) || []).length, 4,
    "blank, NaN, and infinite demand weeks should split the demand area");
  assert.ok(invalidDemandMarkup.includes("计划需求证据缺失"),
    "invalid demand values should remain visible as missing evidence");
  assert.ok(!/NaN|Infinity/.test(invalidDemandMarkup),
    "invalid demand values must not enter SVG geometry or marker titles");
  const invalidDemandPaths = [...invalidDemandMarkup.matchAll(/<path class="buffer-demand-area" d="([^"]+)"/g)]
    .map(match => match[1]);
  for (const index of invalidDemandIndexes) {
    assert.ok(invalidDemandPaths.every(pathValue => !pathValue.includes(`${chartX(index)},`)),
      `invalid demand week ${index} must not enter a demand-area endpoint`);
    assert.ok(!invalidDemandMarkup.includes(`class="buffer-demand-marker" cx="${chartX(index)}"`),
      `invalid demand week ${index} must not create a zero or invalid marker`);
  }

  const invalidThreshold = structuredClone(detail);
  const invalidThresholdIndexes = [2, 5, 8];
  invalidThreshold.series[invalidThresholdIndexes[0]].demandSpikeThreshold = " ";
  invalidThreshold.series[invalidThresholdIndexes[1]].demandSpikeThreshold = Number.NaN;
  invalidThreshold.series[invalidThresholdIndexes[2]].demandSpikeThreshold = Number.POSITIVE_INFINITY;
  runtime.context.__invalidThreshold = invalidThreshold;
  vm.runInContext("renderBufferVolatilityChart(__invalidThreshold)", runtime.context);
  const invalidThresholdMarkup = runtime.elements.get("buffer-volatility-chart").innerHTML;
  assert.equal((invalidThresholdMarkup.match(/buffer-demand-threshold-marker/g) || []).length,
    invalidThreshold.series.length - invalidThresholdIndexes.length,
    "blank, NaN, and infinite thresholds must not create zero or invalid markers");
  assert.equal((invalidThresholdMarkup.match(/<path class="buffer-demand-threshold"/g) || []).length, 4,
    "blank, NaN, and infinite threshold weeks should split the backend threshold line");
  assert.ok(invalidThresholdMarkup.includes("尖峰阈值证据缺失"),
    "invalid threshold values should remain visible as missing evidence");
  assert.ok(!/NaN|Infinity/.test(invalidThresholdMarkup),
    "invalid thresholds must not enter SVG geometry or marker titles");
  const invalidThresholdPaths = [...invalidThresholdMarkup.matchAll(/<path class="buffer-demand-threshold" d="([^"]+)"/g)]
    .map(match => match[1]);
  for (const index of invalidThresholdIndexes) {
    assert.ok(invalidThresholdPaths.every(pathValue => !pathValue.includes(`${chartX(index)},`)),
      `invalid threshold week ${index} must not enter a threshold-line endpoint`);
    assert.ok(!invalidThresholdMarkup.includes(`class="buffer-demand-threshold-marker" cx="${chartX(index)}"`),
      `invalid threshold week ${index} must not create a zero or invalid marker`);
  }
  assert.equal(vm.runInContext("isFiniteChartValue('')", runtime.context), false,
    "blank chart evidence must not be coerced to zero");
  assert.equal(vm.runInContext("isFiniteChartValue('   ')", runtime.context), false,
    "whitespace chart evidence must not be coerced to zero");
  assert.equal(vm.runInContext("isFiniteChartValue(NaN)", runtime.context), false,
    "NaN chart evidence should be rejected");
  assert.equal(vm.runInContext("isFiniteChartValue(Infinity)", runtime.context), false,
    "infinite chart evidence should be rejected");

  const missingThreshold = structuredClone(detail);
  missingThreshold.series = missingThreshold.series.map(item => ({ ...item, demandSpikeThreshold: null }));
  runtime.context.__missingThreshold = missingThreshold;
  vm.runInContext("renderBufferVolatilityChart(__missingThreshold)", runtime.context);
  const missingThresholdMarkup = runtime.elements.get("buffer-volatility-chart").innerHTML;
  assert.ok(missingThresholdMarkup.includes("尖峰阈值证据缺失"),
    "null backend threshold should produce a visible evidence warning");
  assert.ok(!missingThresholdMarkup.includes("buffer-demand-threshold"),
    "null backend threshold must not be rendered as zero");

  const partialThreshold = structuredClone(detail);
  partialThreshold.series[Math.floor(partialThreshold.series.length / 2)].demandSpikeThreshold = null;
  runtime.context.__partialThreshold = partialThreshold;
  vm.runInContext("renderBufferVolatilityChart(__partialThreshold)", runtime.context);
  const partialThresholdMarkup = runtime.elements.get("buffer-volatility-chart").innerHTML;
  assert.equal((partialThresholdMarkup.match(/<path class="buffer-demand-threshold"/g) || []).length, 2,
    "one missing threshold week should split valid backend threshold segments");
  assert.ok(partialThresholdMarkup.includes("尖峰阈值证据缺失"),
    "a partial threshold gap should remain visible as missing evidence");

  const singletonThreshold = structuredClone(detail);
  const singletonThresholdIndex = 2;
  singletonThreshold.series = singletonThreshold.series.map((item, index) => ({
    ...item,
    demandSpikeThreshold: index === singletonThresholdIndex ? item.demandSpikeThreshold : null,
  }));
  runtime.context.__singletonThreshold = singletonThreshold;
  vm.runInContext("renderBufferVolatilityChart(__singletonThreshold)", runtime.context);
  const singletonThresholdMarkup = runtime.elements.get("buffer-volatility-chart").innerHTML;
  assert.equal((singletonThresholdMarkup.match(/buffer-demand-threshold-marker/g) || []).length, 1,
    "one valid backend threshold should remain visible as exactly one marker");
  assert.ok(!singletonThresholdMarkup.includes('<path class="buffer-demand-threshold"'),
    "one threshold point should not masquerade as a visible line");
  assert.ok(singletonThresholdMarkup.includes("尖峰阈值证据缺失"),
    "singleton threshold evidence should retain the surrounding gap warning");

  const missingDemand = structuredClone(detail);
  missingDemand.series[Math.floor(missingDemand.series.length / 2)].demand = null;
  runtime.context.__missingDemand = missingDemand;
  vm.runInContext("renderBufferVolatilityChart(__missingDemand)", runtime.context);
  const missingDemandMarkup = runtime.elements.get("buffer-volatility-chart").innerHTML;
  assert.equal((missingDemandMarkup.match(/<path class="buffer-demand-area"/g) || []).length, 2,
    "missing demand should split the area instead of becoming zero or bridging the gap");

  const singletonDemand = structuredClone(detail);
  const singletonDemandIndex = 3;
  singletonDemand.series = singletonDemand.series.map((item, index) => ({
    ...item,
    demand: index === singletonDemandIndex ? item.demand : null,
  }));
  runtime.context.__singletonDemand = singletonDemand;
  vm.runInContext("renderBufferVolatilityChart(__singletonDemand)", runtime.context);
  const singletonDemandMarkup = runtime.elements.get("buffer-volatility-chart").innerHTML;
  assert.equal((singletonDemandMarkup.match(/buffer-demand-marker/g) || []).length, 1,
    "one valid planned-demand point should remain visible as exactly one marker");
  assert.ok(!singletonDemandMarkup.includes('<path class="buffer-demand-area"'),
    "one demand point should not masquerade as a visible area");
  assert.ok(singletonDemandMarkup.includes("计划需求证据缺失"),
    "singleton demand evidence should retain the surrounding gap warning");
  console.log("4/4 future buffer chart fixture groups passed");
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const dtoPath = process.argv[2];
  const trend = dtoPath
    ? JSON.parse(await readFile(dtoPath, "utf8"))
    : makeStandaloneBufferTrend();
  await runFutureBufferChartFixtures(trend);
}
