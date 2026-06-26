// Standalone shell for the Network Structure Scoring product page.
// This keeps the network product runnable without any external scenario workspace script.

const state = {
  data: null,
  networkScoring: null,
  networkGraph: null,
  networkMetrics: null,
  networkCapabilities: null,
  networkScenarioValidation: null,
  candidateCombinations: null,
  selectedNetworkCandidate: null,
  selectedNetworkItem: "PART-FPGA-SPACE",
  networkGraphDirection: "both",
  networkGraphMaxDepth: 3,
  networkGraphRiskOnly: false,
};

const numberFormat = new Intl.NumberFormat("zh-CN", {
  maximumFractionDigits: 1,
});

const moneyFormat = new Intl.NumberFormat("zh-CN", {
  style: "currency",
  currency: "CNY",
  maximumFractionDigits: 0,
});

function valueOr(value, fallback) {
  return value === undefined || value === null || value === "" ? fallback : value;
}

function number(value) {
  return numberFormat.format(Number(valueOr(value, 0)));
}

function money(value) {
  return moneyFormat.format(Number(valueOr(value, 0)));
}

function byId(id) {
  return document.getElementById(id);
}

function escapeHtml(value) {
  return String(valueOr(value, "")).replace(/[&<>"']/g, character => ({
    "&": "&amp;",
    "<": "&lt;",
    ">": "&gt;",
    '"': "&quot;",
    "'": "&#39;",
  })[character]);
}

function mapLabel(dictionary, value) {
  return dictionary[value] || valueOr(value, "-");
}

function statusLabel(status) {
  return ({
    Red: "红色",
    Yellow: "黄色",
    Green: "绿色",
    Blue: "超绿",
    Optimal: "最优",
    Feasible: "可行",
    Unavailable: "不可用",
    Error: "错误",
  })[status] || valueOr(status, "-");
}

function statusClass(status) {
  return status === "Red" || status === "Error" || status === "Unavailable"
    ? "status-chip is-invalid"
    : status === "Yellow"
      ? "status-chip is-warning"
      : status === "Green" || status === "Optimal" || status === "Feasible"
        ? "status-chip is-valid"
        : "status-chip neutral";
}

function row(cells) {
  return `<tr>${cells.map(cell => `<td>${cell}</td>`).join("")}</tr>`;
}

function emptyRow(message, colspan) {
  return `<tr><td colspan="${colspan}" class="table-empty"><strong>${escapeHtml(message)}</strong></td></tr>`;
}

function setNetworkProductStatus(status, message) {
  const chip = byId("network-product-status");
  if (!chip) return;
  chip.className = statusClass(status);
  chip.textContent = message;
}

function showWorkspaceError(error) {
  byId("workspace-loading").hidden = true;
  byId("workspace-error").hidden = false;
  byId("workspace-error-message").textContent = error.message;
  setNetworkProductStatus("Red", "数据不可用");
}

function renderMultiScenarioComparison() {
  // The standalone network page does not render external scenario comparison tables.
}

window.NetworkStructureProductHost = {
  state,
  valueOr,
  number,
  money,
  byId,
  escapeHtml,
  mapLabel,
  statusLabel,
  statusClass,
  row,
  emptyRow,
  renderMultiScenarioComparison,
  showWorkspaceError,
};

async function loadNetworkStructureProduct() {
  byId("workspace-loading").hidden = false;
  byId("workspace-error").hidden = true;
  setNetworkProductStatus("Yellow", "正在加载");

  await window.NetworkStructureProductWorkspace?.loadData({
    horizonWeeks: 12,
    includeNetworkData: true,
  });

  window.NetworkStructureProductWorkspace?.renderScoring(state.networkScoring);
  window.NetworkStructureProductWorkspace?.renderCapabilities(state.networkCapabilities);
  window.NetworkStructureProductWorkspace?.renderMetrics(state.networkMetrics);
  window.NetworkStructureProductWorkspace?.renderScenarioValidation(state.networkScenarioValidation);

  byId("workspace-loading").hidden = true;
  setNetworkProductStatus("Green", "网络评分已就绪");
}

document.addEventListener("DOMContentLoaded", () => {
  window.NetworkStructureProductWorkspace?.initialize();
  byId("refresh-network-structure")?.addEventListener("click", () => {
    loadNetworkStructureProduct().catch(showWorkspaceError);
  });
  loadNetworkStructureProduct().catch(showWorkspaceError);
});
