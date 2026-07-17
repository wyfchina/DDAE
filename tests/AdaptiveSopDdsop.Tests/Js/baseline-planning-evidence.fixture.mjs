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
  const document = {
    readyState: "complete",
    getElementById: id => create(id),
    querySelector: selector => create(`selector:${selector}`),
    querySelectorAll: () => [],
    createElement: () => create(),
    addEventListener() {},
    removeEventListener() {},
    body: create("fixture-body"),
    documentElement: create("fixture-html"),
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

function evidenceSection(sectionCode, overrides = {}) {
  return {
    sectionCode,
    name: sectionCode,
    sourceAuthority: "DDAE Demo Planning Snapshot",
    asOfUtc: "2026-06-30T08:00:00Z",
    freshnessStatus: "Fresh",
    completenessStatus: "Complete",
    itemCount: 0,
    evidenceLabel: "DemoFixture",
    isRequired: true,
    missingReason: null,
    items: null,
    ...overrides,
  };
}

function completePlanningInputs(overrides = {}) {
  return {
    planningEvidenceCoverage: {
      anchorDate: "2026-06-30",
      coverageFromWeek: 1,
      coverageThroughWeek: 52,
      evidenceStatus: "Complete",
    },
    confirmedReceipts: [],
    openingBacklog: [],
    ...overrides,
  };
}

function candidate(planningInputs, sectionOverrides = {}) {
  return {
    candidateId: "BASE-CANDIDATE-FIXTURE",
    asOfUtc: "2026-06-30T08:00:00Z",
    masterSettingVersion: "SETTINGS-FIXTURE",
    evidenceLabel: "DemoFixture",
    sections: [
      evidenceSection("PLANNING_EVIDENCE_COVERAGE", sectionOverrides.coverage),
      evidenceSection("CONFIRMED_RECEIPTS", sectionOverrides.receipts),
      evidenceSection("OPENING_BACKLOG", sectionOverrides.backlog),
    ],
    payload: { planningInputs, kpis: null },
  };
}

function renderCandidate(source, baseline) {
  const runtime = createRuntime(source);
  runtime.context.__baselineFixture = structuredClone(baseline);
  vm.runInContext(
    "state.currentBaselineCandidate = __baselineFixture; state.currentBaselines = []; renderCurrentBaselineWorkspace();",
    runtime.context,
  );
  return runtime;
}

function planningInputsNullBlocksFreeze(source) {
  const runtime = renderCandidate(source, candidate(null));
  assert.equal(runtime.elements.get("freeze-current-baseline").disabled, true,
    "missing typed planning inputs must disable the existing freeze action");
  assert.ok(runtime.elements.get("current-baseline-chip").textContent.includes("阻断"),
    "missing typed planning inputs must display a blocking candidate state");
  assert.ok(!runtime.elements.get("current-baseline-chip").textContent.includes("可冻结"),
    "missing typed planning inputs must not be described as freezable");
}

function blockingEmptyCollectionsStayMissing(source) {
  const runtime = renderCandidate(source, candidate(
    completePlanningInputs(),
    {
      receipts: {
        completenessStatus: "EvidenceMissing",
        missingReason: "确认到货列表缺少权威记录",
      },
      backlog: {
        completenessStatus: "EvidenceMissing",
        missingReason: "期初积压列表缺少权威记录",
      },
    },
  ));
  const receiptBody = runtime.elements.get("baseline-receipt-evidence-body").innerHTML;
  const backlogBody = runtime.elements.get("baseline-backlog-evidence-body").innerHTML;
  assert.ok(receiptBody.includes("证据缺失") && receiptBody.includes("确认到货列表缺少权威记录"));
  assert.ok(!receiptBody.includes("无确认到货记录"));
  assert.ok(backlogBody.includes("证据缺失") && backlogBody.includes("期初积压列表缺少权威记录"));
  assert.ok(!backlogBody.includes("无期初积压记录"));
}

function completeNonblockingEmptyCollectionsShowNoRecords(source) {
  const runtime = renderCandidate(source, candidate(completePlanningInputs()));
  assert.ok(runtime.elements.get("baseline-receipt-evidence-body").innerHTML.includes("无确认到货记录"));
  assert.ok(runtime.elements.get("baseline-backlog-evidence-body").innerHTML.includes("无期初积压记录"));
  assert.equal(runtime.elements.get("freeze-current-baseline").disabled, false);
}

function explicitZeroRowsRemainZero(source) {
  const runtime = renderCandidate(source, candidate(completePlanningInputs({
    confirmedReceipts: [{
      receiptId: "RECEIPT-ZERO",
      sku: "SKU-ZERO",
      quantity: 0,
      expectedReceiptWeek: 1,
      receiptType: "ConfirmedInTransit",
      sourceReference: "SOURCE-ZERO",
      supplySourceType: "ExternalSupplier",
      confirmationStatus: "Confirmed",
      evidenceStatus: "Complete",
      asOfUtc: "2026-06-30T08:00:00Z",
      evidenceLabel: "DemoFixture",
    }],
    openingBacklog: [{
      backlogId: "BACKLOG-ZERO",
      sku: "SKU-ZERO",
      quantity: 0,
      sourceReference: "SOURCE-ZERO",
      evidenceStatus: "Complete",
      asOfUtc: "2026-06-30T08:00:00Z",
      evidenceLabel: "DemoFixture",
    }],
  })));
  const receiptBody = runtime.elements.get("baseline-receipt-evidence-body").innerHTML;
  const backlogBody = runtime.elements.get("baseline-backlog-evidence-body").innerHTML;
  assert.ok(receiptBody.includes("<td>0</td>"), "explicit receipt zero should render as zero");
  assert.ok(backlogBody.includes("<td>0</td>"), "explicit backlog zero should render as zero");
  assert.ok(!receiptBody.includes("后端未提供数量"));
  assert.ok(!backlogBody.includes("后端未提供数量"));
}

function missingFieldsExplainAbsenceWithoutZero(source) {
  const runtime = renderCandidate(source, candidate(completePlanningInputs({
    confirmedReceipts: [{
      receiptId: "RECEIPT-MISSING",
      receiptType: "ConfirmedInTransit",
      sourceReference: "SOURCE-MISSING",
      supplySourceType: "ExternalSupplier",
      confirmationStatus: "Confirmed",
      evidenceStatus: "EvidenceMissing",
      asOfUtc: "2026-06-30T08:00:00Z",
      evidenceLabel: "DemoFixture",
    }],
    openingBacklog: [{
      backlogId: "BACKLOG-MISSING",
      sourceReference: "SOURCE-MISSING",
      evidenceStatus: "EvidenceMissing",
      asOfUtc: "2026-06-30T08:00:00Z",
      evidenceLabel: "DemoFixture",
    }],
  })));
  const receiptBody = runtime.elements.get("baseline-receipt-evidence-body").innerHTML;
  const backlogBody = runtime.elements.get("baseline-backlog-evidence-body").innerHTML;
  assert.ok(receiptBody.includes("证据缺失"));
  assert.ok(receiptBody.includes("后端未提供 SKU"));
  assert.ok(receiptBody.includes("后端未提供数量"));
  assert.ok(receiptBody.includes("后端未提供预计周"));
  assert.ok(backlogBody.includes("证据缺失"));
  assert.ok(backlogBody.includes("后端未提供 SKU"));
  assert.ok(backlogBody.includes("后端未提供数量"));
  assert.ok(!receiptBody.includes("<td>0</td>"));
  assert.ok(!backlogBody.includes("<td>0</td>"));
}

export async function runBaselinePlanningEvidenceFixtures(scriptPath = defaultScriptPath) {
  const source = await readFile(scriptPath, "utf8");
  new vm.Script(source, { filename: scriptPath });
  const fixtures = [
    ["planningInputs null blocks freeze", planningInputsNullBlocksFreeze],
    ["blocking empty collections stay missing", blockingEmptyCollectionsStayMissing],
    ["complete nonblocking empty collections show no records", completeNonblockingEmptyCollectionsShowNoRecords],
    ["explicit zero rows remain zero", explicitZeroRowsRemainZero],
    ["missing fields explain absence without zero", missingFieldsExplainAbsenceWithoutZero],
  ];
  const failures = [];

  for (const [name, fixture] of fixtures) {
    try {
      fixture(source);
      console.log(`PASS ${name}`);
    } catch (error) {
      failures.push({ error, name });
      console.error(`FAIL ${name}: ${error.message}`);
    }
  }

  if (failures.length > 0) {
    throw new AggregateError(
      failures.map(failure => failure.error),
      `${failures.length} baseline planning evidence fixture(s) failed`,
    );
  }
  console.log("baseline planning evidence fixture groups passed");
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  await runBaselinePlanningEvidenceFixtures();
}
