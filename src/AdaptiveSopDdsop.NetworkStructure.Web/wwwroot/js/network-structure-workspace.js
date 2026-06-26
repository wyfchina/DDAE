// Network structure scoring workspace module.
// Runs against a small host contract so this module can move between an embedded
// shell and the standalone network structure product without owning page chrome.

const itemTypeLabel = {
  FinishedGood: "成品",
  Subassembly: "半成品",
  PurchasedPart: "采购件",
  RawMaterial: "原材料",
};

const evidenceTypeLabel = {
  BomLine: "BOM 用量行",
  SupplierSource: "供应来源",
  RoutingLine: "资源路线行",
  LeadTimeProfile: "提前期档案",
  BufferSetting: "缓冲设置",
  InventoryLocation: "库存位置",
};

const validationRuleLabel = {
  MissingBomParent: "BOM 父项物料缺失",
  BomLineWithoutHeader: "BOM 用量行缺少主记录",
  MissingBomComponent: "BOM 组件物料缺失",
  MissingBufferItem: "缓冲设置物料缺失",
  BomCycle: "BOM 存在循环",
  PurchasedPartWithoutSupplier: "采购件缺少供应来源",
  ItemWithoutRouting: "成品或半成品缺少资源路线",
  BufferWithoutExecutableLocation: "解耦点缺少可执行库存位置",
  MissingAlternateItem: "替代料物料缺失",
  IsolatedItem: "孤立物料",
  InactiveBomIgnored: "未生效 BOM 已忽略",
};

const businessWordLabel = {
  Routing: "资源路线",
  routing: "资源路线",
  SupplierSource: "供应来源",
  Supplier: "供应",
  "Supplier source": "供应来源",
  LeadTimeProfile: "提前期档案",
  "lead time profile": "提前期档案",
  "Lead time profile": "提前期档案",
  BufferSetting: "缓冲设置",
  "Buffer setting": "缓冲设置",
  Subassembly: "半成品",
  FinishedGood: "成品",
  PurchasedPart: "采购件",
  RawMaterial: "原材料",
  Qualified: "已合格",
  EngineeringReview: "工程评审中",
  Current: "当前",
  TimeBuffer: "时间缓冲",
  "BOM header": "BOM 主记录",
  "BOM line": "BOM 用量行",
  "time buffer": "时间缓冲",
  "Time buffer": "时间缓冲",
  "profile": "档案",
  "Optimal": "最优",
  "Feasible": "可行",
  "Infeasible": "不可行",
  "Unavailable": "不可用",
  "Error": "异常",
  "CapacityBuffer": "能力缓冲",
  "InventoryBuffer": "库存缓冲",
  "TimeBuffer": "时间缓冲",
  "SupplierMasterSetting": "供应主设置",
  "DecouplingPoint": "解耦点",
  "需要专项评审": "需要管理取舍",
};

const solverStatusLabel = {
  Optimal: "最优",
  Feasible: "可行",
  Infeasible: "不可行",
  Unavailable: "不可用",
  Error: "异常",
};


const candidateCombinationControls = {
  solver: document.querySelector("#candidate-combination-solver-select"),
  status: document.querySelector("#candidate-combination-status"),
  list: document.querySelector("#candidate-combination-list"),
  button: document.querySelector("#select-candidate-combinations"),
};

const networkCollapseState = new Map();
const networkFocusState = {
  panel: null,
  parent: null,
  nextSibling: null,
  collapseKey: null,
  wasExpanded: null,
};

function networkHost() {
  return window.NetworkStructureProductHost || {};
}

function networkState() {
  return networkHost().state || {};
}

function networkValueOr(value, fallback) {
  const helper = networkHost().valueOr;
  return typeof helper === "function"
    ? helper(value, fallback)
    : value === null || value === undefined || value === "" ? fallback : value;
}

function networkNumber(value) {
  const helper = networkHost().number;
  return typeof helper === "function"
    ? helper(value)
    : new Intl.NumberFormat("zh-CN", { maximumFractionDigits: 1 }).format(Number(networkValueOr(value, 0)));
}

function networkMoney(value) {
  const helper = networkHost().money;
  return typeof helper === "function"
    ? helper(value)
    : new Intl.NumberFormat("zh-CN", { style: "currency", currency: "CNY", maximumFractionDigits: 0 }).format(Number(networkValueOr(value, 0)));
}

function networkById(id) {
  const helper = networkHost().byId;
  return typeof helper === "function" ? helper(id) : document.getElementById(id);
}

function networkEscapeHtml(value) {
  const helper = networkHost().escapeHtml;
  return typeof helper === "function"
    ? helper(value)
    : String(networkValueOr(value, "")).replace(/[&<>"']/g, character => ({
      "&": "&amp;",
      "<": "&lt;",
      ">": "&gt;",
      '"': "&quot;",
      "'": "&#39;",
    })[character]);
}

function networkMapLabel(dictionary, value) {
  const helper = networkHost().mapLabel;
  return typeof helper === "function" ? helper(dictionary, value) : dictionary[value] || networkValueOr(value, "-");
}

function networkStatusClass(status) {
  const helper = networkHost().statusClass;
  return typeof helper === "function" ? helper(status) : "status-chip neutral";
}

function networkStatusLabel(status) {
  const helper = networkHost().statusLabel;
  return typeof helper === "function" ? helper(status) : networkValueOr(status, "-");
}

function networkRow(cells) {
  const helper = networkHost().row;
  return typeof helper === "function" ? helper(cells) : `<tr>${cells.map(cell => `<td>${cell}</td>`).join("")}</tr>`;
}

function networkEmptyRow(message, colspan) {
  const helper = networkHost().emptyRow;
  return typeof helper === "function"
    ? helper(message, colspan)
    : `<tr><td colspan="${colspan}" class="table-empty"><strong>${networkEscapeHtml(message)}</strong></td></tr>`;
}

function networkRenderMultiScenarioComparison(result) {
  const helper = networkHost().renderMultiScenarioComparison;
  if (typeof helper === "function") {
    helper(result);
  }
}

function networkShowWorkspaceError(error) {
  const helper = networkHost().showWorkspaceError;
  if (typeof helper === "function") {
    helper(error);
  } else {
    console.error(error);
  }
}

function networkPanelKey(panel, index) {
  if (!panel.dataset.networkCollapseKey) {
    panel.dataset.networkCollapseKey = panel.id || `network-collapse-panel-${index}`;
  }
  return panel.dataset.networkCollapseKey;
}

function ensureNetworkCollapseBody(panel, index) {
  const existing = panel.querySelector(":scope > [data-network-collapse-body]");
  if (existing) return existing;

  const body = document.createElement("div");
  body.className = "network-collapse-body";
  body.dataset.networkCollapseBody = "";
  body.id = `${networkPanelKey(panel, index)}-body`;

  Array.from(panel.children)
    .filter(child => !child.classList.contains("panel-heading"))
    .forEach(child => body.appendChild(child));

  panel.appendChild(body);
  return body;
}

function setNetworkCollapseState(panel, expanded) {
  const heading = panel.querySelector(":scope > [data-network-collapse-toggle]");
  const body = panel.querySelector(":scope > [data-network-collapse-body]");
  if (!heading || !body) return;

  heading.setAttribute("aria-expanded", String(expanded));
  body.hidden = !expanded;
  panel.classList.toggle("is-collapsed", !expanded);

  const indicator = heading.querySelector(".network-collapse-indicator");
  if (indicator) {
    indicator.textContent = expanded ? "收起" : "展开";
  }

  const action = heading.querySelector("[data-network-focus-panel]");
  if (action && !panel.classList.contains("is-focused-panel")) {
    action.hidden = !expanded;
    action.setAttribute("aria-hidden", String(!expanded));
  }
}

function initializeNetworkCollapsiblePanels() {
  const blocks = Array.from(document.querySelectorAll("#network-structure-scoring-panel .network-scoring-block"));
  if (blocks.some(panel => panel.dataset.collapsePanel !== undefined)) {
    return;
  }

  blocks.forEach((panel, index) => {
    if (panel.dataset.networkCollapsePanel !== undefined) return;

    const heading = panel.querySelector(":scope > .panel-heading");
    if (!heading) return;

    const key = networkPanelKey(panel, index);
    const body = ensureNetworkCollapseBody(panel, index);
    panel.dataset.networkCollapsePanel = "";
    panel.classList.add("collapsible-panel");
    heading.dataset.networkCollapseToggle = "";
    heading.classList.add("network-collapse-toggle");
    heading.setAttribute("role", "button");
    heading.setAttribute("tabindex", "0");
    heading.setAttribute("aria-controls", body.id);

    if (!heading.querySelector("[data-network-focus-panel]")) {
      const action = document.createElement("button");
      action.type = "button";
      action.className = "network-panel-action-button";
      action.dataset.networkFocusPanel = "";
      action.textContent = "专注查看";
      action.setAttribute("aria-label", "专注查看当前网络模块");
      heading.appendChild(action);
    }

    if (!heading.querySelector(".network-collapse-indicator")) {
      heading.insertAdjacentHTML("beforeend", `<span class="network-collapse-indicator" aria-hidden="true"></span>`);
    }

    const defaultExpanded = index <= 1;
    const expanded = networkCollapseState.has(key) ? networkCollapseState.get(key) : defaultExpanded;
    setNetworkCollapseState(panel, expanded);
  });
}

function toggleNetworkCollapsePanel(heading) {
  const panel = heading.closest("[data-network-collapse-panel]");
  if (!panel || networkFocusState.panel === panel) return;
  const key = networkPanelKey(panel, 0);
  const expanded = heading.getAttribute("aria-expanded") !== "true";
  networkCollapseState.set(key, expanded);
  setNetworkCollapseState(panel, expanded);
}

function openNetworkFocusedPanel(panel) {
  if (!panel || networkFocusState.panel === panel) return;
  const wasExpanded = panel.querySelector(":scope > [data-network-collapse-toggle]")?.getAttribute("aria-expanded") !== "false";
  if (!wasExpanded) return;
  if (networkFocusState.panel) {
    closeNetworkFocusedPanel();
  }

  const layer = networkById("network-workspace-focus-layer");
  const stage = layer?.querySelector(".network-focus-stage");
  if (!layer || !stage) return;

  networkFocusState.panel = panel;
  networkFocusState.parent = panel.parentNode;
  networkFocusState.nextSibling = panel.nextSibling;
  networkFocusState.collapseKey = networkPanelKey(panel, 0);
  networkFocusState.wasExpanded = wasExpanded;
  setNetworkCollapseState(panel, true);
  panel.classList.add("is-focused-panel");

  const action = panel.querySelector("[data-network-focus-panel]");
  if (action) {
    action.textContent = "退出专注";
    action.setAttribute("aria-label", "退出专注查看");
    action.hidden = false;
    action.setAttribute("aria-hidden", "false");
  }

  stage.appendChild(panel);
  layer.hidden = false;
  layer.setAttribute("aria-hidden", "false");
  document.body.classList.add("has-network-focus-panel");
  action?.focus();
}

function closeNetworkFocusedPanel() {
  const panel = networkFocusState.panel;
  if (!panel) return;

  const parent = networkFocusState.parent;
  const next = networkFocusState.nextSibling;
  panel.classList.remove("is-focused-panel");

  const action = panel.querySelector("[data-network-focus-panel]");
  if (action) {
    action.textContent = "专注查看";
    action.setAttribute("aria-label", "专注查看当前网络模块");
  }

  if (parent) {
    parent.insertBefore(panel, next && next.parentNode === parent ? next : null);
  }
  if (networkFocusState.collapseKey) {
    networkCollapseState.set(networkFocusState.collapseKey, networkFocusState.wasExpanded !== false);
  }
  setNetworkCollapseState(panel, networkFocusState.wasExpanded !== false);

  const layer = networkById("network-workspace-focus-layer");
  if (layer) {
    layer.hidden = true;
    layer.setAttribute("aria-hidden", "true");
  }
  document.body.classList.remove("has-network-focus-panel");

  networkFocusState.panel = null;
  networkFocusState.parent = null;
  networkFocusState.nextSibling = null;
  networkFocusState.collapseKey = null;
  networkFocusState.wasExpanded = null;

  if (action && !action.hidden) {
    action.focus();
  } else {
    panel.querySelector(":scope > [data-network-collapse-toggle]")?.focus();
  }
}

async function networkFetchJson(url, errorPrefix) {
  const response = await fetch(url, { headers: { Accept: "application/json" } });
  if (!response.ok) {
    throw new Error(`${errorPrefix}：${response.status}`);
  }
  return response.json();
}

async function loadNetworkStructureWorkspaceData(options = {}) {
  const horizonWeeks = Number(options.horizonWeeks || networkState().data?.request?.horizonWeeks || 12);
  const query = `horizonWeeks=${encodeURIComponent(horizonWeeks)}`;

  if (options.includeNetworkData) {
    networkState().data = await networkFetchJson(`/api/network-structure-data?${query}`, "网络主数据接口失败");
  }

  networkState().networkCapabilities = await networkFetchJson("/api/network-structure-capabilities", "产品能力接口失败");
  networkState().networkScoring = await networkFetchJson(`/api/network-structure-scoring?${query}`, "网络结构评分接口失败");
  networkState().selectedNetworkCandidate = networkValueOr(networkState().networkScoring.selectedCandidateId, null);
  networkState().networkMetrics = await networkFetchJson(`/api/network-metrics?${query}`, "网络指标计算接口失败");
  await loadNetworkGraph(networkState().selectedNetworkItem, networkState().networkGraphMaxDepth);
  networkState().networkScenarioValidation = await networkFetchJson(`/api/network-scenario-validation?${query}`, "网络候选场景验证接口失败");

  return {
    data: networkState().data,
    capabilities: networkState().networkCapabilities,
    scoring: networkState().networkScoring,
    graph: networkState().networkGraph,
    metrics: networkState().networkMetrics,
    scenarioValidation: networkState().networkScenarioValidation,
  };
}

function renderNetworkCapabilities(result) {
  const status = networkById("network-capability-status");
  const capabilityList = networkById("network-capability-list");
  const dependencyList = networkById("network-external-dependency-list");
  const boundaryList = networkById("network-boundary-list");
  if (!status || !capabilityList || !dependencyList || !boundaryList) return;

  if (!result) {
    status.className = "status-chip neutral";
    status.textContent = "未加载";
    capabilityList.innerHTML = `<div class="table-empty"><strong>没有产品能力数据</strong></div>`;
    dependencyList.innerHTML = "";
    boundaryList.innerHTML = "";
    return;
  }

  status.className = "status-chip is-valid";
  status.textContent = `${networkValueOr(result.productName, "网络结构评分")} / ${networkValueOr(result.deploymentMode, "独立入口")}`;

  const capabilities = networkValueOr(result.capabilities, []);
  capabilityList.innerHTML = capabilities.map(item => `
    <article class="network-capability-card ${item.isAvailable ? "is-available" : "is-external"}">
      <div>
        <strong>${networkEscapeHtml(businessText(item.name))}</strong>
        <span>${networkEscapeHtml(businessText(item.description))}</span>
      </div>
      <em>${item.isAvailable ? "独立可用" : "需外部验证"}</em>
    </article>
  `).join("") || `<div class="table-empty"><strong>没有独立能力清单</strong></div>`;

  const dependencies = networkValueOr(result.externalDependencies, []);
  dependencyList.innerHTML = dependencies.map(item => `
    <article class="network-capability-card is-external">
      <div>
        <strong>${networkEscapeHtml(businessText(item.name))}</strong>
        <span>${networkEscapeHtml(businessText(item.purpose))}</span>
      </div>
      <em>${item.isRequiredForStandaloneHost ? "独立部署必接" : "可选接入"}</em>
    </article>
  `).join("") || `<div class="table-empty"><strong>没有外部依赖</strong></div>`;

  boundaryList.innerHTML = networkValueOr(result.boundaries, [])
    .map(item => `<span>${networkEscapeHtml(businessText(item))}</span>`)
    .join("") || `<span>没有边界说明</span>`;
}


function itemTypeName(value) {
  return networkMapLabel(itemTypeLabel, value);
}

function evidenceTypeName(value) {
  return networkMapLabel(evidenceTypeLabel, value);
}

function validationRuleName(value) {
  return networkMapLabel(validationRuleLabel, value);
}

function businessText(value) {
  let text = String(networkValueOr(value, ""));
  Object.entries(businessWordLabel)
    .sort((left, right) => right[0].length - left[0].length)
    .forEach(([from, to]) => {
      text = text.split(from).join(to);
    });
  return text.replaceAll(" -> ", " → ");
}

function solverStatusName(value) {
  return networkMapLabel(solverStatusLabel, value);
}


function renderNetworkStructureScoring(result) {
  if (!result) {
    networkById("network-score-model-chip").textContent = "未加载";
    networkById("network-scoring-kpis").innerHTML = "";
    networkById("network-score-summary-grid").innerHTML = `<div class="table-empty"><strong>没有网络结构评分数据</strong></div>`;
    networkById("network-score-candidate-body").innerHTML = networkEmptyRow("没有控制点候选", 9);
    renderNetworkCandidateDetail(null);
    return;
  }

  const candidates = networkValueOr(result.candidates, []);
  const redCount = candidates.filter(item => item.severity === "Red").length;
  const yellowCount = candidates.filter(item => item.severity === "Yellow").length;
  const topScore = candidates.length ? Math.max(...candidates.map(item => Number(item.score))) : 0;
  const typeCount = new Set(candidates.map(item => item.recommendedSettingType)).size;
  const selectedId = candidates.some(item => item.candidateId === networkState().selectedNetworkCandidate)
    ? networkState().selectedNetworkCandidate
    : networkValueOr(result.selectedCandidateId, candidates[0]?.candidateId);
  networkState().selectedNetworkCandidate = selectedId || null;

  networkById("network-score-model-chip").textContent = "评分模型已加载";
  networkById("network-scoring-kpis").innerHTML = [
    ["候选点", networkNumber(candidates.length), "SKU、资源、供应商和产品族候选"],
    ["红色候选", networkNumber(redCount), "优先进入主设置治理"],
    ["黄色候选", networkNumber(yellowCount), "需要会议评审"],
    ["最高得分", networkNumber(topScore), "白盒网络评分"],
    ["候选类型", networkNumber(typeCount), "库存 / 时间 / 能力 / 解耦点"],
    ["评分范围", `${networkNumber(result.horizonWeeks)} 周`, "当前滚动范围"],
  ].map(([label, value, note]) => `<div><span>${label}</span><strong>${value}</strong><small>${note}</small></div>`).join("");

  networkById("network-score-summary-grid").innerHTML = networkValueOr(result.summaries, []).length
    ? result.summaries.map(item => `
      <div class="network-score-summary-card">
        <span class="panel-kicker">${networkEscapeHtml(item.recommendedSettingType)}</span>
        <strong>${networkEscapeHtml(item.topTarget)}</strong>
        <small>${networkEscapeHtml(businessText(item.summary))}</small>
        <span class="family-metric"><span>候选</span><b>${networkNumber(item.candidateCount)}</b><i>平均 ${networkNumber(item.averageScore)}</i></span>
        <span class="family-metric"><span>最高</span><b>${networkNumber(item.topScore)}</b><i>建议优先评审</i></span>
      </div>
    `).join("")
    : `<div class="table-empty"><strong>没有评分摘要</strong></div>`;

  networkById("network-score-candidate-body").innerHTML = candidates.length
    ? candidates.map(item => `
      <tr class="interactive-row ${item.candidateId === selectedId ? "is-linked" : ""}" tabindex="0" data-network-candidate="${networkEscapeHtml(item.candidateId)}">
        <td><strong>${networkEscapeHtml(item.target)}</strong><br><small>${networkEscapeHtml(item.targetName)}</small></td>
        <td>${networkEscapeHtml(item.recommendedSettingType)}<br><small>${networkEscapeHtml(itemTypeName(item.targetType))}</small></td>
        <td>${networkEscapeHtml(item.family)}</td>
        <td><span class="${networkStatusClass(item.severity)}">${networkNumber(item.score)}</span></td>
        <td>${networkNumber(item.reuseScore)}</td>
        <td>${networkNumber(item.leadTimeScore)}</td>
        <td>${networkNumber(item.supplyRiskScore)}</td>
        <td>${networkNumber(item.resourceConstraintScore)}</td>
        <td>${networkNumber(item.inventoryCostPenalty)}</td>
      </tr>
    `).join("")
    : networkEmptyRow("没有控制点候选", 9);

  const selected = candidates.find(item => item.candidateId === selectedId) || candidates[0] || null;
  renderNetworkCandidateDetail(selected, result);
}

function renderNetworkCandidateDetail(candidate, result = networkState().networkScoring) {
  if (!candidate) {
    networkById("network-score-selected-title").textContent = "选中候选详情";
    networkById("network-score-detail-summary").innerHTML = "";
    networkById("network-score-evidence-list").innerHTML = `<div class="table-empty"><strong>请选择一个候选点</strong></div>`;
    networkById("network-score-recommendation-list").innerHTML = `<div class="table-empty"><strong>暂无建议</strong></div>`;
    return;
  }

  networkById("network-score-selected-title").textContent = `${candidate.targetName || candidate.target} 评分详情`;
  networkById("network-score-detail-summary").innerHTML = [
    ["建议类型", candidate.recommendedSettingType],
    ["对象类型", itemTypeName(candidate.targetType)],
    ["产品族", candidate.family],
    ["综合得分", `<span class="${networkStatusClass(candidate.severity)}">${networkNumber(candidate.score)}</span>`],
    ["复用性", networkNumber(candidate.reuseScore)],
    ["累计提前期", networkNumber(candidate.leadTimeScore)],
    ["需求变异性", networkNumber(candidate.demandVariabilityScore)],
    ["供应风险", networkNumber(candidate.supplyRiskScore)],
    ["资源约束", networkNumber(candidate.resourceConstraintScore)],
    ["库存成本惩罚", networkNumber(candidate.inventoryCostPenalty)],
  ].map(([label, value]) => `<div><span>${label}</span><strong>${value}</strong></div>`).join("");

  networkById("network-score-evidence-list").innerHTML = `
    <div class="diagnostic-item">
      <strong>评分原因</strong>
      <span>${networkEscapeHtml(businessText(candidate.rationale))}</span>
    </div>
    ${(candidate.evidence || []).map((item, index) => `
      <div class="diagnostic-item">
        <strong>证据 ${index + 1}</strong>
        <span>${networkEscapeHtml(businessText(item))}</span>
      </div>
    `).join("")}
  `;

  const weights = networkValueOr(result?.factorWeights, []);
  networkById("network-score-recommendation-list").innerHTML = `
    <div class="diagnostic-item ${candidate.severity === "Red" ? "is-error" : ""}">
      <strong>建议动作</strong>
      <span>${networkEscapeHtml(businessText(candidate.recommendedAction))}</span>
    </div>
    <div class="diagnostic-item ${candidate.severity === "Red" ? "is-error" : candidate.severity === "Yellow" ? "is-warning" : ""}">
      <strong>不采纳风险</strong>
      <span>${networkEscapeHtml(businessText(candidate.notAdoptingRisk || "未提供不采纳风险。"))}</span>
    </div>
    ${weights.map(item => `
      <div class="diagnostic-item">
        <strong>${networkEscapeHtml(item.factor)} / 权重 ${networkNumber(Number(item.weight) * 100)}%</strong>
        <span>${networkEscapeHtml(businessText(item.explanation))}</span>
      </div>
    `).join("")}
  `;
}

function renderNetworkGraph(result) {
  const statusChip = networkById("network-graph-status-chip");
  if (!result) {
    statusChip.textContent = "未加载";
    const map = networkById("network-graph-map");
    if (map) {
      map.innerHTML = `<div class="table-empty"><strong>没有物料关系图数据</strong></div>`;
    }
    networkById("network-graph-upstream-body").innerHTML = networkEmptyRow("没有上游影响数据", 7);
    networkById("network-graph-downstream-body").innerHTML = networkEmptyRow("没有下游影响数据", 7);
    networkById("network-graph-edge-body").innerHTML = networkEmptyRow("没有路径明细", 6);
    networkById("network-validation-body").innerHTML = networkEmptyRow("没有校验报告", 5);
    return;
  }

  networkState().selectedNetworkItem = result.selectedItemCode;
  const selector = networkById("network-graph-item-select");
  const items = networkValueOr(networkState().data?.networkData?.items, []);
  selector.innerHTML = items
    .slice()
    .sort((left, right) => left.itemCode.localeCompare(right.itemCode, "zh-CN"))
    .map(item => `<option value="${networkEscapeHtml(item.itemCode)}">${networkEscapeHtml(item.itemCode)} / ${networkEscapeHtml(item.itemName)} / ${networkEscapeHtml(itemTypeName(item.itemType))}</option>`)
    .join("");
  selector.value = result.selectedItemCode;
  const direction = networkById("network-graph-direction-select");
  if (direction) direction.value = networkState().networkGraphDirection;
  const depth = networkById("network-graph-depth-select");
  if (depth) depth.value = String(networkState().networkGraphMaxDepth);
  const riskOnly = networkById("network-graph-risk-only");
  if (riskOnly) riskOnly.checked = networkState().networkGraphRiskOnly;

  const report = result.validationReport;
  statusChip.className = report.redCount > 0 ? "status-chip is-invalid" : report.yellowCount > 0 ? "status-chip is-warning" : "status-chip is-valid";
  statusChip.textContent = `${networkEscapeHtml(result.selectedItemName)}：红 ${networkNumber(report.redCount)} / 黄 ${networkNumber(report.yellowCount)} / 提示 ${networkNumber(report.infoCount)}`;
  renderNetworkGraphMap(result);

  renderNetworkImpactRows("network-graph-upstream-body", result.upstream?.paths, "没有上游影响数据");
  renderNetworkImpactRows("network-graph-downstream-body", result.downstream?.paths, "没有下游影响数据");

  const selectedEdges = networkValueOr(result.edges, []).filter(edge =>
    networkValueOr(result.upstream?.paths, []).some(path => path.itemCodes.includes(edge.parentItemCode) && path.itemCodes.includes(edge.componentItemCode))
    || networkValueOr(result.downstream?.paths, []).some(path => path.itemCodes.includes(edge.parentItemCode) && path.itemCodes.includes(edge.componentItemCode)));
  networkById("network-graph-edge-body").innerHTML = selectedEdges.length
    ? selectedEdges.slice(0, 80).map(edge => networkRow([
      `<strong>${networkEscapeHtml(edge.parentItemCode)}</strong><br><small>${networkEscapeHtml(edge.parentItemName)}</small>`,
      `<strong>${networkEscapeHtml(edge.componentItemCode)}</strong><br><small>${networkEscapeHtml(edge.componentItemName)}</small>`,
      networkNumber(edge.quantityPer),
      networkNumber(edge.scrapFactor),
      networkNumber(edge.effectiveQuantity),
      networkEscapeHtml(edge.alternateGroup || "-"),
    ])).join("")
    : networkEmptyRow("没有路径明细", 6);

  networkById("network-validation-body").innerHTML = networkValueOr(report.issues, []).length
    ? report.issues.slice(0, 80).map(issue => networkRow([
      `<span class="${networkStatusClass(issue.severity)}">${networkStatusLabel(issue.severity)}</span>`,
      networkEscapeHtml(validationRuleName(issue.ruleCode)),
      `<strong>${networkEscapeHtml(issue.itemCode)}</strong><br><small>${networkEscapeHtml(issue.itemName)}</small>`,
      networkEscapeHtml(businessText(issue.message)),
      networkEscapeHtml(businessText(issue.evidence)),
    ])).join("")
    : networkEmptyRow("没有发现网络主数据问题", 5);
}

function renderNetworkGraphMap(result) {
  const container = networkById("network-graph-map");
  if (!container) return;

  const selectedCode = result.selectedItemCode;
  const nodeLookup = new Map(networkValueOr(result.nodes, []).map(node => [node.itemCode, node]));
  const selectedNode = nodeLookup.get(selectedCode) || {
    itemCode: selectedCode,
    itemName: result.selectedItemName,
    itemType: "",
    isDecouplingPoint: false,
  };
  const direction = networkState().networkGraphDirection;
  const maxDepth = Number(networkState().networkGraphMaxDepth || 3);
  const riskOnly = networkState().networkGraphRiskOnly;
  const paths = [];
  if (direction !== "downstream") {
    paths.push(...networkValueOr(result.upstream?.paths, [])
      .filter(path => path.depth <= maxDepth)
      .map(path => ({ ...path, graphDirection: "upstream" })));
  }
  if (direction !== "upstream") {
    paths.push(...networkValueOr(result.downstream?.paths, [])
      .filter(path => path.depth <= maxDepth)
      .map(path => ({ ...path, graphDirection: "downstream" })));
  }

  const graphNodes = new Map([[selectedCode, createGraphNode(selectedNode, "selected", 0)]]);
  const graphEdges = new Map();
  paths.forEach(path => {
    networkValueOr(path.itemCodes, []).forEach((code, index) => {
      const rawNode = nodeLookup.get(code) || { itemCode: code, itemName: code, itemType: "" };
      const signedDepth = path.graphDirection === "upstream" ? -index : index;
      const existing = graphNodes.get(code);
      if (!existing || Math.abs(signedDepth) < Math.abs(existing.depth)) {
        graphNodes.set(code, createGraphNode(rawNode, code === selectedCode ? "selected" : path.graphDirection, signedDepth));
      }
      if (index > 0) {
        const previous = path.itemCodes[index - 1];
        const current = code;
        const key = path.graphDirection === "upstream"
          ? `${current}|${previous}`
          : `${previous}|${current}`;
        if (!graphEdges.has(key)) {
          graphEdges.set(key, {
            from: path.graphDirection === "upstream" ? current : previous,
            to: path.graphDirection === "upstream" ? previous : current,
            quantity: path.cumulativeQuantity,
          });
        }
      }
    });
  });

  let visibleNodes = Array.from(graphNodes.values());
  if (riskOnly) {
    visibleNodes = visibleNodes.filter(node => node.itemCode === selectedCode || node.status !== "green");
  }
  visibleNodes = visibleNodes.slice(0, 42);
  const visibleCodes = new Set(visibleNodes.map(node => node.itemCode));
  const positionedNodes = positionGraphNodes(visibleNodes);
  const nodeByCode = new Map(positionedNodes.map(node => [node.itemCode, node]));
  const visibleEdges = Array.from(graphEdges.values()).filter(edge => visibleCodes.has(edge.from) && visibleCodes.has(edge.to));

  if (positionedNodes.length <= 1 && paths.length === 0) {
    container.innerHTML = `<div class="table-empty"><strong>${networkEscapeHtml(result.selectedItemName)} 暂无可展开的物料关系</strong></div>`;
    return;
  }

  const lines = visibleEdges.map(edge => {
    const from = nodeByCode.get(edge.from);
    const to = nodeByCode.get(edge.to);
    if (!from || !to) return "";
    return `<line x1="${from.x}%" y1="${from.y}%" x2="${to.x}%" y2="${to.y}%" />
      <text x="${(from.x + to.x) / 2}%" y="${(from.y + to.y) / 2}%" dominant-baseline="middle">${networkEscapeHtml(networkNumber(edge.quantity))}</text>`;
  }).join("");

  container.innerHTML = `
    <svg class="network-graph-lines" viewBox="0 0 100 100" preserveAspectRatio="none" aria-hidden="true">${lines}</svg>
    ${positionedNodes.map(node => `
      <button type="button"
        class="network-graph-node ${nodeCssClass(node)}"
        style="left:${node.x}%; top:${node.y}%"
        data-network-graph-node="${networkEscapeHtml(node.itemCode)}"
        title="${networkEscapeHtml(node.itemCode)} / ${networkEscapeHtml(node.itemName)}">
        <strong>${networkEscapeHtml(node.itemCode)}</strong>
        <span>${networkEscapeHtml(node.itemName)}</span>
        <small>${networkEscapeHtml(itemTypeName(node.itemType))}${node.isDecouplingPoint ? " / 缓冲点" : ""}</small>
      </button>
    `).join("")}
    ${graphNodes.size > visibleNodes.length ? `<div class="network-graph-truncation">已按当前筛选显示 ${visibleNodes.length} 个节点，可缩小层级或关闭风险过滤后继续查看。</div>` : ""}
  `;
}

function createGraphNode(node, direction, depth) {
  const status = graphNodeStatus(node);
  return {
    itemCode: node.itemCode,
    itemName: node.itemName,
    itemType: node.itemType,
    direction,
    depth,
    isDecouplingPoint: Boolean(node.isDecouplingPoint),
    hasInventoryLocation: Boolean(node.hasInventoryLocation),
    hasSupplierSource: Boolean(node.hasSupplierSource),
    hasRouting: Boolean(node.hasRouting),
    status,
  };
}

function graphNodeStatus(node) {
  if (node.isDecouplingPoint && !node.hasInventoryLocation) return "red";
  if ((node.itemType === "FinishedGood" || node.itemType === "Subassembly") && !node.hasRouting) return "yellow";
  if ((node.itemType === "PurchasedPart" || node.itemType === "RawMaterial") && !node.hasSupplierSource) return "yellow";
  if (node.isDecouplingPoint) return "buffer";
  return "green";
}

function positionGraphNodes(nodes) {
  const columns = new Map();
  nodes.forEach(node => {
    const key = String(Math.max(-4, Math.min(4, node.depth)));
    if (!columns.has(key)) columns.set(key, []);
    columns.get(key).push(node);
  });

  const minDepth = Math.min(-1, ...nodes.map(node => node.depth));
  const maxDepth = Math.max(1, ...nodes.map(node => node.depth));
  const span = Math.max(1, maxDepth - minDepth);
  return nodes.map(node => {
    const key = String(Math.max(-4, Math.min(4, node.depth)));
    const column = columns.get(key) || [node];
    const index = column.findIndex(item => item.itemCode === node.itemCode);
    const count = column.length;
    const x = 8 + ((node.depth - minDepth) / span) * 84;
    const y = count === 1 ? 50 : 12 + (index / Math.max(1, count - 1)) * 76;
    return { ...node, x: Number(x.toFixed(2)), y: Number(y.toFixed(2)) };
  });
}

function nodeCssClass(node) {
  return [
    node.direction === "selected" ? "is-selected" : "",
    node.direction === "upstream" ? "is-upstream" : "",
    node.direction === "downstream" ? "is-downstream" : "",
    node.isDecouplingPoint ? "is-buffer" : "",
    node.status === "red" ? "is-risk-red" : "",
    node.status === "yellow" ? "is-risk-yellow" : "",
  ].filter(Boolean).join(" ");
}

function renderNetworkImpactRows(targetId, paths, emptyText) {
  const rows = networkValueOr(paths, []);
  networkById(targetId).innerHTML = rows.length
    ? rows.slice(0, 80).map(path => networkRow([
      `第 ${networkNumber(path.depth)} 层`,
      networkEscapeHtml(businessText(path.pathText)),
      networkNumber(path.cumulativeQuantity),
      path.hasBuffer ? "是" : "否",
      path.hasInventoryLocation ? "是" : "否",
      path.hasSupplierSource ? "是" : "否",
      path.hasRouting ? "是" : "否",
    ])).join("")
    : networkEmptyRow(emptyText, 7);
}

function renderNetworkMetrics(result) {
  const chip = networkById("network-metrics-status-chip");
  if (!result) {
    chip.className = "status-chip neutral";
    chip.textContent = "未加载";
    networkById("network-metrics-body").innerHTML = networkEmptyRow("没有网络指标数据", 10);
    renderNetworkMetricEvidence(null);
    return;
  }

  const metrics = networkValueOr(result.itemMetrics, []);
  const selectedCode = metrics.some(item => item.itemCode === networkState().selectedNetworkItem)
    ? networkState().selectedNetworkItem
    : networkValueOr(result.selectedItemCode, metrics[0]?.itemCode);
  networkState().selectedNetworkItem = selectedCode || networkState().selectedNetworkItem;
  chip.className = metrics.length ? "status-chip is-valid" : "status-chip is-warning";
  chip.textContent = metrics.length ? `${metrics.length} 个物料已计算` : "没有网络指标";
  networkById("network-metrics-body").innerHTML = metrics.length
    ? metrics.slice(0, 80).map(item => `
      <tr class="interactive-row ${item.itemCode === selectedCode ? "is-linked" : ""}" tabindex="0" data-network-metric-item="${networkEscapeHtml(item.itemCode)}">
        <td><strong>${networkEscapeHtml(item.itemCode)}</strong><br><small>${networkEscapeHtml(item.itemName)}</small></td>
        <td>${networkEscapeHtml(itemTypeName(item.itemType))}</td>
        <td>${networkEscapeHtml(item.family)}</td>
        <td>${networkNumber(item.downstreamCoverageScore)}</td>
        <td>${networkNumber(item.quantityImpactScore)}</td>
        <td>${networkNumber(item.cumulativeLeadTimeScore)}</td>
        <td><span class="${networkStatusClass(scoreStatus(item.supplyRiskScore))}">${networkNumber(item.supplyRiskScore)}</span></td>
        <td><span class="${networkStatusClass(scoreStatus(item.resourceConstraintScore))}">${networkNumber(item.resourceConstraintScore)}</span></td>
        <td>${networkNumber(item.inventoryCostScore)}</td>
        <td>${networkEscapeHtml(businessText(item.summary))}</td>
      </tr>
    `).join("")
    : networkEmptyRow("没有网络指标数据", 10);

  renderNetworkMetricEvidence(metrics.find(item => item.itemCode === selectedCode) || metrics[0] || null);
}

function renderNetworkMetricEvidence(metric) {
  const breakdownList = networkById("network-metric-breakdown-list");
  const evidenceList = networkById("network-metric-evidence-list");
  if (!metric) {
    breakdownList.innerHTML = `<div class="table-empty"><strong>请选择物料查看指标解释</strong></div>`;
    evidenceList.innerHTML = `<div class="table-empty"><strong>暂无证据链</strong></div>`;
    return;
  }

  const groups = [
    ["下游覆盖度", metric.downstreamCoverage],
    ["数量影响度", metric.quantityImpact],
    ["累计提前期", metric.cumulativeLeadTime],
    ["供应风险", metric.supplyRisk],
    ["资源约束", metric.resourceConstraint],
    ["库存代价", metric.inventoryCost],
  ];
  breakdownList.innerHTML = groups.map(([label, value]) => `
    <div class="diagnostic-item">
      <strong>${label}：${networkNumber(value?.score)}</strong>
      <span>${networkEscapeHtml(businessText(value?.explanation || "未提供解释"))}</span>
    </div>
  `).join("");
  const evidence = groups.flatMap(([label, value]) => networkValueOr(value?.evidence, []).map(item => ({ label, ...item })));
  evidenceList.innerHTML = evidence.length
    ? evidence.slice(0, 30).map(item => `
      <div class="diagnostic-item">
        <strong>${networkEscapeHtml(item.label)} / ${networkEscapeHtml(evidenceTypeName(item.evidenceType))}</strong>
        <span>${networkEscapeHtml(businessText(item.evidenceKey))}：${networkEscapeHtml(businessText(item.description))}；数量 ${networkNumber(item.quantity)}，贡献 ${networkNumber(item.scoreContribution)}</span>
      </div>
    `).join("")
    : `<div class="table-empty"><strong>${networkEscapeHtml(metric.itemName)} 暂无证据链</strong></div>`;
}

function scoreStatus(score) {
  const value = Number(networkValueOr(score, 0));
  return value >= 75 ? "Red" : value >= 55 ? "Yellow" : "Green";
}

function renderNetworkScenarioValidation(result) {
  const chip = networkById("network-scenario-validation-chip");
  if (!result) {
    chip.className = "status-chip neutral";
    chip.textContent = "未加载";
    networkById("network-scenario-validation-body").innerHTML = networkEmptyRow("没有场景验证结果", 10);
    return;
  }

  const validations = networkValueOr(result.validations, []);
  chip.className = validations.length ? "status-chip is-valid" : "status-chip is-warning";
  chip.textContent = validations.length ? `${validations.length} 个候选已验证` : "没有候选验证结果";
  networkById("network-scenario-validation-body").innerHTML = validations.length
    ? validations.map(item => networkRow([
      `<strong>${networkEscapeHtml(item.target)}</strong><br><small>${networkEscapeHtml(item.targetName)}</small>`,
      networkEscapeHtml(item.recommendedSettingType),
      networkMoney(item.averageInventoryValueDelta),
      networkNumber(item.redWeekDelta),
      networkNumber(item.replenishmentOrderCountDelta),
      networkNumber(item.replenishmentQuantityDelta),
      `${networkNumber(item.rccpPeakLoadDelta)}pp`,
      networkNumber(item.rccpRedWeekDelta),
      networkNumber(item.supplyGapDelta),
      networkEscapeHtml(businessText(item.validationSummary)),
    ])).join("")
    : networkEmptyRow("没有场景验证结果", 10);
}


function solverStatusClass(status) {
  return status === "Optimal" || status === "Feasible"
    ? "status-chip is-valid"
    : status === "Unavailable" || status === "Error"
      ? "status-chip is-invalid"
      : "status-chip is-warning";
}

function renderCandidateActionCombinations(result) {
  networkState().candidateCombinations = result;
  candidateCombinationControls.status.className = solverStatusClass(result.solverStatus);
  candidateCombinationControls.status.textContent = `${result.solverName || "Gurobi"}：${businessText(result.message || solverStatusName(result.solverStatus))}`;
  networkRenderMultiScenarioComparison(result);

  candidateCombinationControls.list.innerHTML = networkValueOr(result.combinations, []).length
    ? result.combinations.map(item => {
      const comparison = item.whiteBoxPreviewResult.comparison;
      const combinationComparison = item.comparison || {};
      return `
        <article class="candidate-combination-card">
          <div class="optimization-card-heading">
            <div><span class="panel-kicker">${networkEscapeHtml(item.profileId)}</span><h3>${networkEscapeHtml(item.profileName)}</h3></div>
            <span class="${solverStatusClass(item.solverStatus)}">${networkEscapeHtml(solverStatusName(item.solverStatus))}</span>
          </div>
          <p>${networkEscapeHtml(businessText(item.summary))}</p>
          <div class="optimization-metrics">
            <span>流速变化 <strong>${networkNumber(comparison.flowIndexDelta)}pp</strong></span>
            <span>峰值负荷变化 <strong>${networkNumber(comparison.peakLoadPercentDelta)}pp</strong></span>
            <span>供应缺口变化 <strong>${networkNumber(comparison.supplyGapDelta)}</strong></span>
            <span>动作成本 <strong>${networkMoney(networkValueOr(item.estimatedActionCost, 0))}</strong></span>
            <span>管理判断 <strong>${networkEscapeHtml(networkValueOr(combinationComparison.managementDecision, "待评审"))}</strong></span>
          </div>
          <div class="optimization-actions">
            ${item.selectedActions.length
              ? item.selectedActions.map(action => `<span>${networkEscapeHtml(businessText(action.actionType))}：${networkEscapeHtml(action.target)} / ${networkMoney(networkValueOr(action.estimatedCost, 0))}</span>`).join("")
              : `<span>未选择候选动作</span>`}
          </div>
          <small class="muted-note">已白盒重算：不自动带入场景、不保存、不审批。</small>
        </article>
      `;
    }).join("")
    : `<div class="table-empty"><strong>没有候选组合</strong><p>${networkEscapeHtml(result.message || "当前求解器未返回可进入评审的候选组合。")}</p></div>`;
}

async function selectCandidateActionCombinations() {
  const solverName = candidateCombinationControls.solver?.value || "Gurobi";
  candidateCombinationControls.status.className = "status-chip is-warning";
  candidateCombinationControls.status.textContent = `${solverName} 正在选择`;
  candidateCombinationControls.list.innerHTML = `<div class="table-empty"><strong>正在选择候选动作组合</strong></div>`;
  const payload = {
    horizonWeeks: 12,
    combinationCount: 3,
    maxActionsPerCombination: 3,
    targetMode: null,
    solverName,
  };
  const response = await fetch("/api/candidate-action-combinations/select", {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify(payload),
  });
  if (!response.ok) {
    throw new Error(`候选组合选择接口失败：${response.status}`);
  }

  renderCandidateActionCombinations(await response.json());
}


async function loadNetworkGraph(itemCode = networkState().selectedNetworkItem || "PART-FPGA-SPACE", maxDepth = networkState().networkGraphMaxDepth || 3) {
  const depth = Math.max(1, Math.min(12, Number(maxDepth || 3)));
  networkState().networkGraphMaxDepth = depth;
  const url = `/api/network-graph?itemCode=${encodeURIComponent(itemCode)}&maxDepth=${encodeURIComponent(depth)}`;
  const response = await fetch(url, {
    headers: { Accept: "application/json" },
  });

  if (!response.ok) {
    throw new Error(`物料网络图接口失败：${response.status}`);
  }

  networkState().networkGraph = await response.json();
  networkState().selectedNetworkItem = networkState().networkGraph.selectedItemCode;
  renderNetworkGraph(networkState().networkGraph);
}


function initializeNetworkStructureWorkspace() {
  initializeNetworkCollapsiblePanels();

  document.addEventListener("click", event => {
    const focusButton = event.target.closest("[data-network-focus-panel]");
    if (focusButton) {
      const panel = focusButton.closest("[data-network-collapse-panel]");
      if (networkFocusState.panel === panel) {
        closeNetworkFocusedPanel();
      } else {
        openNetworkFocusedPanel(panel);
      }
      return;
    }

    if (event.target.id === "network-workspace-focus-layer") {
      closeNetworkFocusedPanel();
      return;
    }

    const collapseHeading = event.target.closest("[data-network-collapse-toggle]");
    if (collapseHeading && !event.target.closest("button, a, input, select, textarea")) {
      toggleNetworkCollapsePanel(collapseHeading);
      return;
    }

    const row = event.target.closest("[data-network-candidate]");
    if (!row || !networkState().networkScoring) return;
    networkState().selectedNetworkCandidate = row.dataset.networkCandidate;
    renderNetworkStructureScoring(networkState().networkScoring);
  });

  document.addEventListener("keydown", event => {
    if (event.key === "Escape" && networkFocusState.panel) {
      closeNetworkFocusedPanel();
      return;
    }

    const collapseHeading = event.target.closest("[data-network-collapse-toggle]");
    if (!collapseHeading) return;
    if (event.key !== "Enter" && event.key !== " ") return;
    event.preventDefault();
    toggleNetworkCollapsePanel(collapseHeading);
  });

  document.addEventListener("click", event => {
    const row = event.target.closest("[data-network-metric-item]");
    if (!row || !networkState().networkMetrics) return;
    networkState().selectedNetworkItem = row.dataset.networkMetricItem;
    renderNetworkMetrics(networkState().networkMetrics);
    loadNetworkGraph(networkState().selectedNetworkItem, networkState().networkGraphMaxDepth).catch(error => {
      networkById("network-graph-status-chip").className = "status-chip is-invalid";
      networkById("network-graph-status-chip").textContent = "物料网络加载失败";
      networkShowWorkspaceError(error);
    });
  });

  document.addEventListener("click", event => {
    const node = event.target.closest("[data-network-graph-node]");
    if (!node) return;
    networkState().selectedNetworkItem = node.dataset.networkGraphNode;
    renderNetworkMetrics(networkState().networkMetrics);
    loadNetworkGraph(networkState().selectedNetworkItem, networkState().networkGraphMaxDepth).catch(error => {
      networkById("network-graph-status-chip").className = "status-chip is-invalid";
      networkById("network-graph-status-chip").textContent = "物料网络加载失败";
      networkShowWorkspaceError(error);
    });
  });

  document.addEventListener("change", event => {
    if (event.target.id === "network-graph-direction-select") {
      networkState().networkGraphDirection = event.target.value;
      renderNetworkGraph(networkState().networkGraph);
      return;
    }
    if (event.target.id === "network-graph-risk-only") {
      networkState().networkGraphRiskOnly = event.target.checked;
      renderNetworkGraph(networkState().networkGraph);
      return;
    }
    if (event.target.id === "network-graph-depth-select") {
      networkState().networkGraphMaxDepth = Number(event.target.value || 3);
      loadNetworkGraph(networkState().selectedNetworkItem, networkState().networkGraphMaxDepth).catch(error => {
        networkById("network-graph-status-chip").className = "status-chip is-invalid";
        networkById("network-graph-status-chip").textContent = "物料网络加载失败";
        networkShowWorkspaceError(error);
      });
      return;
    }
    if (event.target.id !== "network-graph-item-select") return;
    networkState().selectedNetworkItem = event.target.value;
    renderNetworkMetrics(networkState().networkMetrics);
    loadNetworkGraph(event.target.value, networkState().networkGraphMaxDepth).catch(error => {
      networkById("network-graph-status-chip").className = "status-chip is-invalid";
      networkById("network-graph-status-chip").textContent = "物料网络加载失败";
      networkShowWorkspaceError(error);
    });
  });

  networkById("select-candidate-combinations")?.addEventListener("click", () => {
    selectCandidateActionCombinations().catch(error => {
      candidateCombinationControls.status.className = "status-chip is-invalid";
      candidateCombinationControls.status.textContent = "组合选择失败";
      networkShowWorkspaceError(error);
    });
  });
}

window.NetworkStructureProductWorkspace = {
  initialize: initializeNetworkStructureWorkspace,
  renderScoring: renderNetworkStructureScoring,
  renderCapabilities: renderNetworkCapabilities,
  renderGraph: renderNetworkGraph,
  renderMetrics: renderNetworkMetrics,
  renderScenarioValidation: renderNetworkScenarioValidation,
  loadGraph: loadNetworkGraph,
  loadData: loadNetworkStructureWorkspaceData,
  selectCandidateCombinations: selectCandidateActionCombinations,
};
