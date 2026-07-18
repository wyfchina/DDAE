import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import vm from "node:vm";

const fixtureDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(fixtureDirectory, "..", "..", "..");
const scriptPath = path.join(repositoryRoot, "src", "AdaptiveSopDdsop.Web", "wwwroot", "js", "app.js");
const pagePath = path.join(repositoryRoot, "src", "AdaptiveSopDdsop.Web", "Pages", "Index.cshtml");

function sourceBetween(source, start, end) {
  const startIndex = source.indexOf(start);
  const endIndex = source.indexOf(end, startIndex + start.length);
  assert.ok(startIndex >= 0 && endIndex > startIndex, `expected source block ${start}`);
  return source.slice(startIndex, endIndex);
}

function createElement(id) {
  const handlers = new Map();
  return {
    id,
    open: false,
    innerHTML: "",
    textContent: "",
    dataset: {},
    addEventListener(type, handler) {
      const entries = handlers.get(type) || [];
      entries.push(handler);
      handlers.set(type, entries);
    },
    dispatch(type) {
      for (const handler of handlers.get(type) || []) handler({ target: this });
    },
  };
}

function createRuntime(source) {
  const elements = new Map();
  const byId = id => {
    if (!elements.has(id)) elements.set(id, createElement(id));
    return elements.get(id);
  };
  let fetchImplementation = () => Promise.reject(new Error("unexpected fetch"));
  let referenceFetchCount = 0;
  const state = {
    ddmrpStandardReference: null,
    ddmrpStandardReferencePromise: null,
  };
  const context = vm.createContext({
    state,
    byId,
    row: cells => `<tr>${cells.map(cell => `<td>${cell}</td>`).join("")}</tr>`,
    emptyRow: (message, columns = 4) => `<tr><td class="empty-cell" colspan="${columns}">${message}</td></tr>`,
    metricOrEvidenceMissing: value => value === null || value === undefined || value === "" ? "证据缺失" : value,
    evidenceStatusLabel: value => ({ Complete: "完整", EvidenceMissing: "证据缺失", ReviewedEvidence: "已评审证据" })[value] || value || "证据缺失",
    escapeHtml: value => String(value)
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#39;"),
    number: value => String(value),
    isFiniteHistoryValue: value => value !== null && value !== undefined && Number.isFinite(Number(value)),
    historyGreenDriverLabel: value => value || "证据缺失",
    renderHistoryMissing: (id, message = "证据缺失") => {
      byId(id).innerHTML = `<div class="history-empty-chart"><strong>证据缺失</strong><span>${message}</span></div>`;
    },
    fetch: (...args) => {
      if (args[0] === "/api/ddmrp-standard-reference") referenceFetchCount++;
      return fetchImplementation(...args);
    },
    console,
    Promise,
  });
  const functions = [
    sourceBetween(source, "function renderDdmrpStandardReference(", "function loadDdmrpStandardReference("),
    sourceBetween(source, "function loadDdmrpStandardReference(", "function initializeDdmrpStandardReferencePanel("),
    sourceBetween(source, "function initializeDdmrpStandardReferencePanel(", "function renderHistoryTimeBuffer("),
  ].join("\n");
  new vm.Script(functions, { filename: scriptPath }).runInContext(context);
  return {
    context,
    elements,
    panel: byId("ddmrp-standard-reference-panel"),
    setFetch: implementation => { fetchImplementation = implementation; },
    referenceFetchCount: () => referenceFetchCount,
  };
}

function response(payload, ok = true, status = 200) {
  return {
    ok,
    status,
    json: async () => structuredClone(payload),
  };
}

function completeReference(overrides = {}) {
  return {
    referenceId: "DDMRP-EXAMPLE-V1",
    name: "标准定容算例",
    inputs: {
      adu: 10,
      decoupledLeadTimeDays: 12,
      leadTimeFactor: 0.5,
      variabilityFactor: 0.33,
      minimumOrderQuantity: 50,
      orderCycleDays: 7,
      demandAdjustmentFactor: 1,
      zoneAdjustmentFactor: 1,
    },
    redBase: 60,
    redSafety: 19.8,
    zones: { red: 80, yellow: 120, green: 70, topOfRed: 80, topOfYellow: 200, topOfGreen: 270 },
    totalBuffer: 270,
    greenDriver: "OrderCycle",
    derivations: [
      { component: "红区基础", formula: "后端公式", value: 60, explanation: "后端证据" },
      { component: "红区安全量", formula: "后端公式", value: 19.8, explanation: "后端证据" },
    ],
    sourceAuthority: "DDAE 后端标准定容算例",
    evidenceStatus: "Complete",
    ...overrides,
  };
}

const [source, page] = await Promise.all([
  readFile(scriptPath, "utf8"),
  readFile(pagePath, "utf8"),
]);
new vm.Script(source, { filename: scriptPath });

const savedHostIndex = page.indexOf('<section id="saved-scenarios-panel"');
const disclosureIndex = page.indexOf('<details id="ddmrp-standard-reference-panel"');
const traceIndex = page.indexOf('<section id="trace-panel" class="schedule-panel" data-tab-panel hidden>');
assert.ok(savedHostIndex >= 0 && disclosureIndex > savedHostIndex && traceIndex > disclosureIndex,
  "reference disclosure should be inside the saved host immediately before the protected trace panel");
const disclosureOpening = page.slice(disclosureIndex, page.indexOf(">", disclosureIndex) + 1);
assert.ok(!/\sopen(?:\s|=|>)/.test(disclosureOpening), "reference disclosure should be closed by default");
assert.ok(page.includes("缓冲计算参考") && page.includes("计算参考，非当前物料"));
assert.ok(!page.includes("history-standard-ddmrp-input-summary") && !page.includes("history-standard-ddmrp-zone-chart"),
  "history should no longer host the standard reference");
assert.ok(!sourceBetween(source, "const collapsiblePanelConfigs", "function initializeCollapsiblePanels(")
  .includes("ddmrp-standard-reference-panel"), "native disclosure must not join the generic collapsible registry");
assert.ok(!sourceBetween(source, "async function loadWorkspace(", "function buildPreviewRequest(")
  .includes("ddmrp-standard-reference"), "workspace startup must not fetch the standard reference");
console.log("PASS disclosure placement and default closed state");

const firstOpen = createRuntime(source);
firstOpen.context.initializeDdmrpStandardReferencePanel();
assert.equal(firstOpen.referenceFetchCount(), 0, "initialization should not fetch");
firstOpen.setFetch(async url => {
  assert.equal(url, "/api/ddmrp-standard-reference");
  return response(completeReference());
});
firstOpen.panel.open = true;
firstOpen.panel.dispatch("toggle");
await new Promise(resolve => setTimeout(resolve, 0));
assert.equal(firstOpen.referenceFetchCount(), 1, "first open should fetch exactly once");
assert.equal(firstOpen.context.state.ddmrpStandardReference.referenceId, "DDMRP-EXAMPLE-V1");
console.log("PASS first open lazy loads the backend reference");

const concurrent = createRuntime(source);
let resolveFetch;
concurrent.setFetch(() => new Promise(resolve => { resolveFetch = resolve; }));
const firstPromise = concurrent.context.loadDdmrpStandardReference();
const secondPromise = concurrent.context.loadDdmrpStandardReference();
assert.strictEqual(firstPromise, secondPromise, "concurrent loads should share one in-flight promise");
assert.equal(concurrent.referenceFetchCount(), 1);
resolveFetch(response(completeReference()));
await firstPromise;
await concurrent.context.loadDdmrpStandardReference();
assert.equal(concurrent.referenceFetchCount(), 1, "success should remain cached");
console.log("PASS concurrent loading shares one promise and caches success");

const retry = createRuntime(source);
retry.setFetch(async () => response({}, false, 503));
await assert.rejects(retry.context.loadDdmrpStandardReference(), /503/);
assert.equal(retry.context.state.ddmrpStandardReferencePromise, null, "failure should clear in-flight state");
retry.setFetch(async () => response(completeReference()));
await retry.context.loadDdmrpStandardReference();
assert.equal(retry.referenceFetchCount(), 2, "a failed request should be retryable");
console.log("PASS failed loading clears in-flight state and retries");

const alternate = createRuntime(source);
alternate.context.renderDdmrpStandardReference(completeReference({
  inputs: { ...completeReference().inputs, adu: 13 },
  redBase: 72,
  redSafety: 24,
  zones: { red: 96, yellow: 156, green: 84, topOfRed: 96, topOfYellow: 252, topOfGreen: 336 },
  totalBuffer: 336,
  sourceAuthority: "替代后端证据来源",
  derivations: [{ component: "替代推导", formula: "后端替代公式", value: 24, explanation: "替代证据" }],
}));
const alternateInputs = alternate.elements.get("ddmrp-standard-reference-inputs").innerHTML;
const alternateZones = alternate.elements.get("ddmrp-standard-reference-zones").innerHTML;
const alternateStatus = alternate.elements.get("ddmrp-standard-reference-status").innerHTML;
const alternateDerivations = alternate.elements.get("ddmrp-standard-reference-derivation-body").innerHTML;
assert.ok(alternateInputs.includes(">13<") && alternateInputs.includes(">72<") && alternateInputs.includes(">24<") && alternateInputs.includes(">336<"));
assert.ok(alternateZones.includes("红区 96") && alternateZones.includes("黄区 156") && alternateZones.includes("绿区 84"));
assert.ok(alternateStatus.includes("替代后端证据来源") && alternateStatus.includes("完整"));
assert.ok(alternateDerivations.includes("替代推导") && alternateDerivations.includes("后端替代公式") && alternateDerivations.includes("替代证据"));
assert.ok(!sourceBetween(source, "function renderDdmrpStandardReference(", "function loadDdmrpStandardReference(")
  .match(/leadTimeFactor\s*\*|variabilityFactor\s*\*|Math\.max\s*\(/), "frontend must not recalculate DDMRP sizing");
console.log("PASS alternate backend values source and evidence render without frontend formulas");

const partial = createRuntime(source);
partial.context.renderDdmrpStandardReference(completeReference({
  inputs: { ...completeReference().inputs, adu: null },
  redBase: null,
  zones: { red: null, yellow: 120, green: 70 },
  evidenceStatus: "EvidenceMissing",
  derivations: [{ component: "红区基础", formula: "后端公式", value: null, explanation: "EvidenceMissing" }],
}));
const partialMarkup = [...partial.elements.values()].map(element => `${element.innerHTML}${element.textContent}`).join("\n");
assert.ok(partialMarkup.includes("证据缺失"));
assert.ok(!partialMarkup.includes("红区 0") && !partialMarkup.includes("history-standard-zone-stack"),
  "partial reference must not substitute zero or draw a complete zone stack");
console.log("PASS partial evidence remains missing instead of zero");

console.log("6/6 DDMRP standard reference fixture groups passed");
