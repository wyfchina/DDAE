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

function createStandaloneHistoryReview(weeks = 26, alternateStandard = false) {
  const periodStartDate = weekOffset => {
    const date = new Date(Date.UTC(2026, 5, 1));
    date.setUTCDate(date.getUTCDate() + weekOffset * 7);
    return date.toISOString().slice(0, 10);
  };
  const makeInventoryPoints = (multiplier, parameterSnapshotId) => Array.from({ length: weeks }, (_, index) => {
    const weekOffset = -weeks + index;
    const topOfRed = Math.round((42 + 5 * Math.sin(index / 3)) * multiplier);
    const topOfYellow = topOfRed + Math.round((58 + 7 * Math.sin(index / 4)) * multiplier);
    const topOfGreen = topOfYellow + Math.round((38 + 5 * Math.cos(index / 3)) * multiplier);
    return {
      weekOffset,
      periodStartDate: periodStartDate(weekOffset),
      endingOnHand: Math.round((72 + 18 * Math.sin(index / 2)) * multiplier),
      openSupply: Math.round((34 + 5 * Math.cos(index / 3)) * multiplier),
      qualifiedDemand: Math.round((31 + 12 * Math.sin(index / 2.5)) * multiplier),
      netFlow: Math.round((88 + 22 * Math.sin(index / 2.2)) * multiplier),
      topOfRed,
      topOfYellow,
      topOfGreen,
      status: "Green",
      cause: "无事件",
      parameterSnapshotId,
      evidenceStatus: "Complete",
      actualDemand: Math.round((38 + 16 * Math.sin(index / 2.4)) * multiplier),
      demandSpikeThreshold: Math.round(55 * multiplier),
      targetNetFlowPosition: Math.round((topOfYellow + topOfGreen) / 2),
    };
  });
  const makeSnapshot = (sku, name, controlPoint, multiplier = 1) => ({
    snapshotId: `HIST-${sku}-V2`,
    controlPoint,
    sku,
    name,
    effectiveFromWeekOffset: -weeks,
    effectiveThroughWeekOffset: -1,
    setting: {
      sku,
      name,
      adu: 10 * multiplier,
      decoupledLeadTimeDays: 12,
      leadTimeFactor: 0.5,
      variabilityFactor: 0.33,
      orderCycleDays: 7,
      minimumOrderQuantity: 50 * multiplier,
      demandAdjustmentFactor: 1,
      zoneAdjustmentFactor: 1,
    },
    sizing: {
      zones: {
        red: 80 * multiplier,
        yellow: 120 * multiplier,
        green: 70 * multiplier,
        topOfRed: 80 * multiplier,
        topOfYellow: 200 * multiplier,
        topOfGreen: 270 * multiplier,
      },
      greenDriver: "OrderCycle",
      evidenceStatus: "Complete",
    },
    sizingLines: [{ component: "红区", formula: "后端定容", value: 80 * multiplier, explanation: "Complete" }],
    averageOnHand: 56 * multiplier,
    sourceAuthority: "registered validation data",
    asOfUtc: "2026-06-01T23:59:59Z",
    evidenceStatus: "Complete",
  });
  const timePoints = Array.from({ length: weeks }, (_, index) => {
    const weekOffset = -weeks + index;
    return {
      weekOffset,
      periodStartDate: periodStartDate(weekOffset),
      earlyCount: index % 3,
      greenCount: 7 + (index % 4),
      yellowCount: 3 + (index % 3),
      redCount: index % 5 === 0 ? 2 : 1,
      lateCount: index % 7 === 0 ? 1 : 0,
      abnormalCost: weekOffset === -16 ? 420000 : null,
      cause: weekOffset === -16 ? "返工" : "无事件",
      evidenceStatus: "Complete",
    };
  });
  const recentCost = {
    eventId: "HAC-2026-002",
    weekOffset: -16,
    periodStartDate: periodStartDate(-16),
    costAmount: 420000,
    costType: "返工费用",
    cause: "返工",
    targetType: "时间缓冲",
    targetId: "MS-TB-001",
    controlPoint: "热真空试验准备控制点",
    sourceAuthority: "DDAE 演示历史事实台账",
    evidenceStatus: "Complete",
  };
  const annualCosts = [
    { ...recentCost, eventId: "HAC-2025-004", weekOffset: -46, periodStartDate: periodStartDate(-46), costAmount: 180000, costType: "需求应对费用", cause: "需求波动", targetType: "需求对象", targetId: "星载电子", controlPoint: "星载电子需求控制点" },
    { ...recentCost, eventId: "HAC-2025-003", weekOffset: -39, periodStartDate: periodStartDate(-39), costAmount: 240000, costType: "供应加急费用", cause: "进口延迟", targetType: "库存控制点", targetId: "AV-FPGA-203", controlPoint: "关键进口 FPGA 库存控制点" },
    { ...recentCost, eventId: "HAC-2026-001", weekOffset: -33, periodStartDate: periodStartDate(-33), costAmount: 360000, costType: "能力恢复费用", cause: "能力损失", targetType: "能力对象", targetId: "RES-AIT", controlPoint: "AIT 总装集成大厅" },
    recentCost,
  ];
  const makeCapacityPoints = isUpstream => Array.from({ length: weeks }, (_, index) => {
    const weekOffset = -weeks + index;
    return {
      weekOffset,
      periodStartDate: periodStartDate(weekOffset),
      theoreticalCapacity: 200,
      standardCapacity: 180,
      demonstratedCapacity: 165,
      plannedAvailableCapacity: 160,
      committedLoad: 120 + index % 7,
      protectionStart: 128,
      protectiveCapacity: isUpstream ? 32 : null,
      consumedProtection: isUpstream ? 8 + index % 5 : null,
      remainingProtection: isUpstream ? 20 : null,
      evidenceStatus: "Complete",
    };
  });
  const standardZones = alternateStandard
    ? { red: 96, yellow: 156, green: 84, topOfRed: 96, topOfYellow: 252, topOfGreen: 336 }
    : { red: 80, yellow: 120, green: 70, topOfRed: 80, topOfYellow: 200, topOfGreen: 270 };
  const standardSetting = {
    adu: alternateStandard ? 13 : 10,
    decoupledLeadTimeDays: 12,
    leadTimeFactor: 0.5,
    variabilityFactor: 0.33,
    orderCycleDays: 7,
    minimumOrderQuantity: 50,
    demandAdjustmentFactor: 1,
    zoneAdjustmentFactor: 1,
  };
  const inventoryBuffers = [
    { controlPoint: "星载电子半成品库存控制点", sku: "AV-COM-201", name: "星载通信机", detailWindowWeeks: 3, points: makeInventoryPoints(1, "HIST-AV-COM-201-V2"), distribution: [], evidenceStatus: "Complete" },
    { controlPoint: "关键进口 FPGA 库存控制点", sku: "AV-FPGA-203", name: "抗辐照 FPGA", detailWindowWeeks: 3, points: makeInventoryPoints(0.24, "HIST-AV-FPGA-203-V2"), distribution: [], evidenceStatus: "Complete" },
  ];
  const ddmrpSizingSnapshots = [
    makeSnapshot("AV-COM-201", "星载通信机", "星载电子半成品库存控制点"),
    makeSnapshot("AV-FPGA-203", "抗辐照 FPGA", "关键进口 FPGA 库存控制点", 0.24),
  ];
  return {
    maximumCumulativeLeadTimeDays: 21,
    detailWindowWeeks: 3,
    trendMonths: weeks === 52 ? 12 : 6,
    observedTrendWeeks: weeks,
    operatingOutcomes: { serviceLevelPercent: 96.8, inventoryValue: 386000000, workInProcessUnits: 1088, averageFlowTimeDays: 8.9, cashOccupied: 410000000, expediteCost: 420000, remainingProtectionPercent: 37, evidenceStatus: "Complete" },
    protectionRelationships: [{ controlPoint: "关键进口 FPGA 库存控制点", protectedObject: "AV-FPGA-203", protectionType: "库存缓冲", designStatus: "有保护设计", availabilityStatus: "保护可用", effectivenessStatus: "保护有效", evidence: "Complete" }],
    zoneResidence: [{ sku: "AV-COM-201", name: "星载通信机", observedPeriods: 3, redPeriods: 1, yellowPeriods: 1, greenPeriods: 1, overTopOfGreenPeriods: 0, redPercent: 33.3, yellowPercent: 33.3, greenPercent: 33.4, overTopOfGreenPercent: 0, redEntryCount: 1, maximumRedStreak: 1, recoveryPeriods: 1, primaryCause: "无事件" }],
    capacityProtection: [{ resourceCode: "RES-AIT", resourceName: "AIT 总装集成大厅", protectedCcrResourceCode: "RES-TVAC", relationshipRole: "UpstreamProtection", theoreticalCapacity: 200, standardCapacity: 180, demonstratedCapacity: 165, plannedAvailableCapacity: 160, committedLoad: 124, protectiveCapacity: 32, consumedProtection: 10, remainingProtection: 22, lossReason: "换线", evidenceStatus: "Complete" }],
    constraintExposure: [],
    evidenceLabel: `DemoFixture / TrendWindow=${weeks}`,
    inventoryBuffers,
    ddmrpSizingSnapshots,
    timeBuffers: [{ bufferId: "MS-TB-001", controlPoint: "热真空试验准备控制点", protectedActivity: "试验件到位与热真空窗口准备", points: timePoints, distribution: [], evidenceStatus: "Complete", abnormalCostEvents: weeks === 52 ? annualCosts : [recentCost] }],
    capacityBuffers: [
      { resourceCode: "RES-AIT", resourceName: "AIT 总装集成大厅", protectedCcrResourceCode: "RES-TVAC", relationshipRole: "UpstreamProtection", points: makeCapacityPoints(true), distribution: [], evidenceStatus: "Complete" },
      { resourceCode: "RES-HARNESS", resourceName: "线束集成工位", protectedCcrResourceCode: null, relationshipRole: "CcrUtilization", points: makeCapacityPoints(false), distribution: [], evidenceStatus: "Complete" },
    ],
    standardDdmrpReference: {
      snapshotId: alternateStandard ? "DDMRP-EXAMPLE-V2" : "DDMRP-EXAMPLE-V1",
      controlPoint: "标准定容参考",
      sku: "DDMRP-EXAMPLE",
      name: "标准定容算例",
      setting: standardSetting,
      sizing: { zones: standardZones, greenDriver: "OrderCycle", evidenceStatus: "Complete" },
      sizingLines: [],
      averageOnHand: null,
      sourceAuthority: "DDAE 后端标准定容算例",
      asOfUtc: "2026-06-01",
      evidenceStatus: "Complete",
    },
  };
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
    targetNetFlowPosition: null,
    actualDemand: null,
    demandSpikeThreshold: null,
    evidenceStatus: "EvidenceMissing",
    cause: "历史证据缺口",
  });
  return fixture;
}

export async function runHistoryBufferRendererFixtures(
  historyReview,
  alternateHistoryReview,
  annualHistoryReview,
  scriptPath = defaultScriptPath,
) {
  assert.ok(historyReview?.inventoryBuffers?.length, "real history DTO should include inventory buffers");
  assert.ok(historyReview?.timeBuffers?.length, "real history DTO should include time buffers");
  assert.ok(historyReview?.capacityBuffers?.length, "real history DTO should include capacity buffers");
  assert.ok(annualHistoryReview?.observedTrendWeeks === 52, "annual history DTO should expose 52 weeks");
  for (const inventory of historyReview.inventoryBuffers) {
    const snapshotIds = new Set(historyReview.ddmrpSizingSnapshots
      .filter(snapshot => snapshot.sku === inventory.sku && snapshot.controlPoint === inventory.controlPoint)
      .map(snapshot => snapshot.snapshotId));
    assert.ok(inventory.points.every(point => !point.parameterSnapshotId || snapshotIds.has(point.parameterSnapshotId)),
      `${inventory.sku} weekly parameter IDs should resolve to its supplied sizing snapshots`);
  }
  const source = await readFile(scriptPath, "utf8");
  new vm.Script(source, { filename: scriptPath });
  console.log("PASS app.js syntax compiles");

  const positionRendererSource = source.slice(
    source.indexOf("function renderHistoryInventoryPositionChart"),
    source.indexOf("function renderHistoryInventoryVolatilityChart"),
  );
  assert.ok(positionRendererSource.includes("const linearSegments = new Set([")
    && positionRendererSource.includes("...monotoneCrossingSegments(redLower, redUpper)")
    && positionRendererSource.includes("...monotoneCrossingSegments(redUpper, yellowUpper)")
    && positionRendererSource.includes("...monotoneCrossingSegments(yellowUpper, greenUpper)")
    && positionRendererSource.includes("buildMonotoneAreaPath(redLower, redUpper, linearSegments)")
    && positionRendererSource.includes("buildMonotoneAreaPath(redUpper, yellowUpper, linearSegments)")
    && positionRendererSource.includes("buildMonotoneAreaPath(yellowUpper, greenUpper, linearSegments)"),
  "all three stacked history zones must share one union of monotone crossing fallbacks");

  const runtime = createRuntime(source);
  const fixture = rendererFixtureWithInventoryGap(historyReview);
  fixture.inventoryBuffers[0].points[0].actualDemand = 0;
  runtime.context.__historyFixture = fixture;
  vm.runInContext("renderHistoryReview(__historyFixture)", runtime.context);
  const standardInput = runtime.elements.get("history-standard-ddmrp-input-summary").innerHTML;
  const standardChart = runtime.elements.get("history-standard-ddmrp-zone-chart").innerHTML;
  const historicalInput = runtime.elements.get("history-ddmrp-input-summary").innerHTML;
  const sizingTable = runtime.elements.get("history-ddmrp-sizing-body").innerHTML;
  const zoneChart = runtime.elements.get("history-ddmrp-zone-chart").innerHTML;
  const inventoryPositionChart = runtime.elements.get("history-inventory-position-chart").innerHTML;
  const inventoryVolatilityChart = runtime.elements.get("history-inventory-volatility-chart").innerHTML;
  const timeStatusChart = runtime.elements.get("history-time-status-chart").innerHTML;
  const timeCostStrip = runtime.elements.get("history-time-cost-strip").innerHTML;
  const capacityChart = runtime.elements.get("history-capacity-buffer-chart").innerHTML;
  assert.ok(standardInput.includes("ADU") && standardInput.includes("10") && standardInput.includes("DLT") && standardInput.includes("12"));
  assert.ok(standardInput.includes("后端计算") && standardInput.includes("DDAE 后端标准定容算例"));
  assert.ok(standardChart.includes("红区 80") && standardChart.includes("黄区 120") && standardChart.includes("绿区 70"));
  assert.ok(standardChart.includes("订货周期驱动"));
  assert.ok(historicalInput.includes("登记校验证据"), "historical source should localize registered validation evidence");
  assert.ok(!historicalInput.includes("registered validation data"), "historical source must not expose ordinary English wording");
  assert.ok(sizingTable.includes("生效") || zoneChart.includes("生效周段"), "historical snapshot evidence should remain visible");
  assert.ok(zoneChart.includes("生效周段") && zoneChart.includes("证据"));
  assert.ok((inventoryPositionChart.match(/history-series-line is-on-hand/g) || []).length >= 2, "inventory gap should split the line");
  assert.ok((inventoryVolatilityChart.match(/history-demand-area/g) || []).length >= 2, "demand gap should split the area");
  assert.ok(inventoryPositionChart.includes("history-evidence-gap") && inventoryVolatilityChart.includes("history-evidence-gap"),
    "both inventory charts should mark the backend evidence gap");
  assert.ok(inventoryPositionChart.includes("history-zone-fill is-red")
    && inventoryPositionChart.includes("history-zone-fill is-yellow")
    && inventoryPositionChart.includes("history-zone-fill is-green")
    && /history-zone-fill is-red[^>]+d="[^"]*C /.test(inventoryPositionChart),
    "weekly backend zones should render as monotone stacked areas");
  assert.ok(inventoryPositionChart.includes("history-series-line is-target-nfp"), "target NFP should be visible");
  assert.ok(inventoryVolatilityChart.includes("history-demand-threshold"), "backend demand-spike threshold should be visible");
  const positionDomain = inventoryPositionChart.match(/data-history-week-domain="([^"]+)"/)?.[1];
  const volatilityDomain = inventoryVolatilityChart.match(/data-history-week-domain="([^"]+)"/)?.[1];
  assert.ok(positionDomain && positionDomain === volatilityDomain, "inventory position and volatility must share one weekly x domain");
  assert.ok(inventoryPositionChart.includes(`${fixture.observedTrendWeeks} 周历史趋势`)
    && inventoryVolatilityChart.includes(`${fixture.observedTrendWeeks} 周历史趋势`),
    "both charts should identify the 26/52-week trend range");
  assert.ok(inventoryPositionChart.includes(`累计提前期详细证据窗口：${fixture.inventoryBuffers[0].detailWindowWeeks} 周`)
    && !inventoryPositionChart.includes(`${fixture.inventoryBuffers[0].detailWindowWeeks} 周证据`),
    "detail window must not masquerade as the trend range");
  assert.ok(inventoryPositionChart.includes(fixture.inventoryBuffers[0].points[0].parameterSnapshotId),
    "inventory evidence table should retain the weekly parameter snapshot ID");
  const zeroY = inventoryVolatilityChart.match(/data-zero-y="([^"]+)"/)?.[1];
  const zeroDemandY = inventoryVolatilityChart.match(/class="history-demand-point"[^>]+data-value="0"[^>]+cy="([^"]+)"/)?.[1];
  assert.ok(zeroY && zeroDemandY === zeroY, "explicit zero demand should render on the zero line");

  const singletonRuntime = createRuntime(source);
  const singletonFixture = structuredClone(historyReview);
  singletonFixture.inventoryBuffers[0].points = singletonFixture.inventoryBuffers[0].points.map((point, index) => index === 0 || index === 2
    ? point
    : {
        ...point,
        endingOnHand: null,
        netFlow: null,
        topOfRed: null,
        topOfYellow: null,
        topOfGreen: null,
        targetNetFlowPosition: null,
        actualDemand: null,
        demandSpikeThreshold: null,
        evidenceStatus: "EvidenceMissing",
      });
  singletonRuntime.context.__historyFixture = singletonFixture;
  vm.runInContext("renderHistoryReview(__historyFixture)", singletonRuntime.context);
  const singletonPosition = singletonRuntime.elements.get("history-inventory-position-chart").innerHTML;
  const singletonVolatility = singletonRuntime.elements.get("history-inventory-volatility-chart").innerHTML;
  for (const cssClass of [
    "history-zone-point is-red",
    "history-zone-point is-yellow",
    "history-zone-point is-green",
    "history-series-point is-on-hand",
    "history-series-point is-net-flow",
    "history-series-point is-target-nfp",
  ]) {
    assert.equal((singletonPosition.match(new RegExp(cssClass, "g")) || []).length, 2,
      `valid-missing-valid position evidence should expose two ${cssClass} singleton markers`);
  }
  assert.equal((singletonVolatility.match(/history-demand-point/g) || []).length, 2,
    "valid-missing-valid demand evidence should retain both singleton markers");
  assert.equal((singletonVolatility.match(/history-demand-threshold-point/g) || []).length, 2,
    "valid-missing-valid threshold evidence should expose both singleton markers");
  assert.ok(!singletonPosition.includes("NaN") && !singletonPosition.includes("undefined")
    && !singletonVolatility.includes("NaN") && !singletonVolatility.includes("undefined"),
  "singleton evidence markers must contain finite coordinates and values");
  assert.ok(["is-early", "is-green", "is-yellow", "is-red", "is-late"].every(css => timeStatusChart.includes(css)));
  assert.ok(!timeStatusChart.includes("history-cost-line")
    && !timeStatusChart.includes("history-cost-marker")
    && !timeStatusChart.includes("异常费用"),
    "time status chart should contain only the five status bands");
  const realCostWeeks = fixture.timeBuffers[0].points
    .filter(point => point.abnormalCost !== null && point.abnormalCost !== undefined)
    .map(point => point.weekOffset);
  assert.deepEqual(realCostWeeks, [-16], "real six-month DTO should retain its single linked abnormal-cost week");
  assert.equal(fixture.timeBuffers[0].points.find(point => point.weekOffset === -15)?.abnormalCost, null,
    "week -15 should remain missing cost evidence");
  assert.equal((timeCostStrip.match(/history-cost-event-card/g) || []).length, 1,
    "the 26-week global abnormal-cost strip should render one event card");
  for (const field of ["返工费用", "返工", "热真空试验准备控制点", "时间缓冲", "MS-TB-001", "DDAE 历史演示台账", "完整"])
    assert.ok(timeCostStrip.includes(field), `cost event card should include ${field}`);
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

  const annualRuntime = createRuntime(source);
  annualRuntime.context.__historyFixture = structuredClone(annualHistoryReview);
  vm.runInContext("renderHistoryReview(__historyFixture)", annualRuntime.context);
  const annualPosition = annualRuntime.elements.get("history-inventory-position-chart").innerHTML;
  const annualCostStrip = annualRuntime.elements.get("history-time-cost-strip").innerHTML;
  assert.ok(annualPosition.includes("52 周历史趋势"), "annual inventory chart should identify the 52-week trend");
  assert.equal((annualCostStrip.match(/history-cost-event-card/g) || []).length, 4,
    "annual global abnormal-cost strip should render all four valid events");
  const annualCostHeading = annualCostStrip.match(/<div class="history-chart-heading">([\s\S]*?)<\/div>/)?.[1] || "";
  assert.ok(annualCostHeading.includes("全对象异常费用事实台账"),
    "the global cost strip heading should state its all-object scope");
  assert.ok(!annualCostHeading.includes(annualHistoryReview.timeBuffers[0].controlPoint),
    "the global cost strip heading must not mislabel all-object events as the selected time-buffer control point");

  const noCostRuntime = createRuntime(source);
  const noCostFixture = structuredClone(historyReview);
  noCostFixture.timeBuffers[0].abnormalCostEvents = [];
  noCostRuntime.context.__historyFixture = noCostFixture;
  vm.runInContext("renderHistoryReview(__historyFixture)", noCostRuntime.context);
  assert.ok(noCostRuntime.elements.get("history-time-cost-strip").innerHTML.includes("本窗口无异常费用事实"));
  assert.ok(!noCostRuntime.elements.get("history-time-cost-strip").innerHTML.includes("history-cost-event-card"));
  noCostFixture.timeBuffers[0].abnormalCostEvents = null;
  vm.runInContext("renderHistoryReview(__historyFixture)", noCostRuntime.context);
  assert.ok(noCostRuntime.elements.get("history-time-cost-strip").innerHTML.includes("本窗口无异常费用事实"));
  assert.ok(!noCostRuntime.elements.get("history-time-cost-strip").innerHTML.includes("history-cost-event-card"),
    "null event collections should clear stale cards without a zero substitute");

  const unsafeCostRuntime = createRuntime(source);
  const unsafeCostFixture = structuredClone(historyReview);
  unsafeCostFixture.timeBuffers[0].abnormalCostEvents[0] = {
    ...unsafeCostFixture.timeBuffers[0].abnormalCostEvents[0],
    eventId: "事件<编号&",
    periodStartDate: "日期<值&",
    costType: "费用<类型&",
    cause: "原因<值&",
    targetType: "对象<类型&",
    targetId: "对象<编号&",
    controlPoint: "控制点<值&",
    sourceAuthority: "来源<值&",
    evidenceStatus: "证据<值&",
  };
  unsafeCostRuntime.context.__historyFixture = unsafeCostFixture;
  vm.runInContext("renderHistoryReview(__historyFixture)", unsafeCostRuntime.context);
  const unsafeCostStrip = unsafeCostRuntime.elements.get("history-time-cost-strip").innerHTML;
  for (const escaped of ["事件&lt;编号&amp;", "日期&lt;值&amp;", "费用&lt;类型&amp;", "原因&lt;值&amp;", "对象&lt;类型&amp;", "对象&lt;编号&amp;", "控制点&lt;值&amp;", "来源&lt;值&amp;", "证据&lt;值&amp;"])
    assert.ok(unsafeCostStrip.includes(escaped), `cost strip should escape ${escaped}`);
  assert.ok(!unsafeCostStrip.includes("<编号&") && !unsafeCostStrip.includes("<值&") && !unsafeCostStrip.includes("<类型&"),
    "cost cards must not emit raw dynamic markup");
  console.log("PASS history ranges, global cost cards, empty evidence, and escaping render");

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
  assert.ok(runtime.elements.get("history-inventory-position-chart").innerHTML.includes("关键进口 FPGA 独立库存控制点"));
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
    "history-inventory-position-chart",
    "history-inventory-volatility-chart",
    "history-ddmrp-zone-chart",
    "history-time-status-chart",
    "history-time-cost-strip",
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
    targetNetFlowPosition: null,
    actualDemand: null,
    demandSpikeThreshold: null,
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
    "history-inventory-position-chart",
    "history-inventory-volatility-chart",
    "history-ddmrp-zone-chart",
    "history-time-status-chart",
    "history-capacity-buffer-chart",
  ]) {
    const markup = allMissingRuntime.elements.get(hostId).innerHTML;
    assert.ok(markup.includes("证据缺失"), `${hostId} should expose all-missing evidence`);
    assert.ok(!markup.includes("<svg"), `${hostId} should not draw an all-missing SVG`);
  }

  const positionMissingRuntime = createRuntime(source);
  const positionMissingFixture = structuredClone(historyReview);
  positionMissingFixture.inventoryBuffers[0].points = positionMissingFixture.inventoryBuffers[0].points.map(point => ({
    ...point,
    endingOnHand: null,
    netFlow: null,
    topOfRed: null,
    topOfYellow: null,
    topOfGreen: null,
    targetNetFlowPosition: null,
    evidenceStatus: "EvidenceMissing",
  }));
  positionMissingRuntime.context.__historyFixture = positionMissingFixture;
  vm.runInContext("renderHistoryReview(__historyFixture)", positionMissingRuntime.context);
  assert.ok(positionMissingRuntime.elements.get("history-inventory-position-chart").innerHTML.includes("证据缺失"));
  assert.ok(positionMissingRuntime.elements.get("history-inventory-volatility-chart").innerHTML.includes("<svg"),
    "missing inventory-position evidence must not suppress valid demand volatility");

  const volatilityMissingRuntime = createRuntime(source);
  const volatilityMissingFixture = structuredClone(historyReview);
  volatilityMissingFixture.inventoryBuffers[0].points = volatilityMissingFixture.inventoryBuffers[0].points.map(point => ({
    ...point,
    actualDemand: null,
    demandSpikeThreshold: null,
    evidenceStatus: "EvidenceMissing",
  }));
  volatilityMissingRuntime.context.__historyFixture = volatilityMissingFixture;
  vm.runInContext("renderHistoryReview(__historyFixture)", volatilityMissingRuntime.context);
  assert.ok(volatilityMissingRuntime.elements.get("history-inventory-position-chart").innerHTML.includes("<svg"));
  assert.ok(volatilityMissingRuntime.elements.get("history-inventory-volatility-chart").innerHTML.includes("证据缺失"),
    "missing demand volatility must not suppress valid inventory-position evidence");

  const invalidZoneRuntime = createRuntime(source);
  const invalidZoneFixture = structuredClone(historyReview);
  const invalidZonePoint = invalidZoneFixture.inventoryBuffers[0].points[Math.floor(invalidZoneFixture.inventoryBuffers[0].points.length / 2)];
  invalidZonePoint.topOfRed = invalidZonePoint.topOfYellow + 1;
  invalidZonePoint.evidenceStatus = "Complete";
  invalidZoneRuntime.context.__historyFixture = invalidZoneFixture;
  vm.runInContext("renderHistoryReview(__historyFixture)", invalidZoneRuntime.context);
  const invalidZoneChart = invalidZoneRuntime.elements.get("history-inventory-position-chart").innerHTML;
  assert.ok(invalidZoneChart.includes(`class="history-evidence-gap" data-week-offset="${invalidZonePoint.weekOffset}"`),
    "an invalid 0 <= red <= yellow <= green ordering must render as an evidence gap");
  assert.ok((invalidZoneChart.match(/history-zone-fill is-red/g) || []).length >= 2,
    "invalid zone ordering should split the stacked zone evidence instead of drawing through it");

  const invalidVolatilityRuntime = createRuntime(source);
  const invalidVolatilityFixture = structuredClone(historyReview);
  const negativeDemandPoint = invalidVolatilityFixture.inventoryBuffers[0].points[3];
  const zeroThresholdPoint = invalidVolatilityFixture.inventoryBuffers[0].points[4];
  negativeDemandPoint.actualDemand = -1;
  negativeDemandPoint.evidenceStatus = "Complete";
  zeroThresholdPoint.demandSpikeThreshold = 0;
  zeroThresholdPoint.evidenceStatus = "Complete";
  invalidVolatilityRuntime.context.__historyFixture = invalidVolatilityFixture;
  vm.runInContext("renderHistoryReview(__historyFixture)", invalidVolatilityRuntime.context);
  const invalidVolatilityChart = invalidVolatilityRuntime.elements.get("history-inventory-volatility-chart").innerHTML;
  assert.ok(invalidVolatilityChart.includes(`class="history-evidence-gap" data-week-offset="${negativeDemandPoint.weekOffset}"`)
    && invalidVolatilityChart.includes(`class="history-evidence-gap" data-week-offset="${zeroThresholdPoint.weekOffset}"`),
  "negative demand and a non-positive threshold must render as volatility evidence gaps");
  assert.ok(!invalidVolatilityChart.includes('data-value="-1"'),
    "negative demand must not render as an actual-demand point");
  assert.equal(Number(invalidVolatilityChart.match(/data-zero-y="([^"]+)"/)?.[1]), 212,
    "invalid negative demand must not distort the volatility scale below zero");

  const negativeTimeRuntime = createRuntime(source);
  const negativeTimeFixture = structuredClone(historyReview);
  const negativeTimePoint = negativeTimeFixture.timeBuffers[0].points[Math.floor(negativeTimeFixture.timeBuffers[0].points.length / 2)];
  negativeTimePoint.earlyCount = -1;
  negativeTimePoint.evidenceStatus = "Complete";
  negativeTimeRuntime.context.__historyFixture = negativeTimeFixture;
  vm.runInContext("renderHistoryReview(__historyFixture)", negativeTimeRuntime.context);
  const negativeTimeChart = negativeTimeRuntime.elements.get("history-time-status-chart").innerHTML;
  assert.ok(negativeTimeChart.includes(`class="history-evidence-gap" data-week-offset="${negativeTimePoint.weekOffset}"`),
    "a negative five-band count must render as an evidence gap");
  assert.equal((negativeTimeChart.match(/history-time-band /g) || []).length, (historyReview.observedTrendWeeks - 1) * 5,
    "a week containing a negative count must not render reverse status bars");
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

  console.log("9/9 renderer fixture groups passed");
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const dtoPath = process.argv[2];
  const alternateDtoPath = process.argv[3];
  const annualDtoPath = process.argv[4];
  const historyReview = dtoPath
    ? JSON.parse(await readFile(dtoPath, "utf8"))
    : createStandaloneHistoryReview(26);
  const alternateHistoryReview = alternateDtoPath
    ? JSON.parse(await readFile(alternateDtoPath, "utf8"))
    : createStandaloneHistoryReview(26, true);
  const annualHistoryReview = annualDtoPath
    ? JSON.parse(await readFile(annualDtoPath, "utf8"))
    : createStandaloneHistoryReview(52);
  await runHistoryBufferRendererFixtures(historyReview, alternateHistoryReview, annualHistoryReview);
}
