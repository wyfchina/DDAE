const state = {
  data: null,
  filtered: null,
  preview: null,
  productFamilyDashboard: null,
  publicDemoGoldenLoop: null,
  adventureWorksProductDemo: null,
  rccp: null,
  constraints: null,
  supplierCollaboration: null,
  exceptions: null,
  bufferTrend: null,
  baselineBufferTrend: null,
  candidateCombinations: null,
  masterSettings: null,
  masterSettingProposals: [],
  currentMasterSettingDetail: null,
  savedScenarioRuns: [],
  ddomPackages: [],
  selectedDdomPackageId: null,
  currentDdomPackageDetail: null,
  ddomCreateInFlight: false,
  ddomActionInFlight: false,
  ddomDetailRequestGeneration: 0,
  scenarioDetailRequestGeneration: 0,
  historyReview: null,
  ddmrpStandardReference: null,
  ddmrpStandardReferencePromise: null,
  historyTrendMonths: 6,
  selectedHistoryControlPoint: null,
  selectedHistoryInventorySku: null,
  selectedHistoryInventoryWeekOffset: null,
  selectedHistorySizingSnapshot: null,
  selectedHistoryTimeBufferId: null,
  selectedHistoryCapacityResource: null,
  historyRequestGeneration: 0,
  workspaceErrorSource: null,
  historySelection: {
    inventoryControlPoint: null,
    inventorySku: null,
    timeBufferId: null,
    sizingControlPoint: null,
    sizingSku: null,
    sizingSnapshotId: null,
    capacityResourceCode: null,
  },
  currentBaselineCandidate: null,
  currentBaselines: [],
  scenarioAssumptionTemplates: [],
  futureComparison: null,
  futureComparisonRequest: null,
  futureComparisonBaseline: null,
  selectedTimeBufferBreachKey: null,
  savedFutureComparisons: {},
  coordinationItems: [],
  selectedCoordinationItemId: null,
  selectedBufferSku: null,
  selectedRccpResource: null,
  selectedSupplier: null,
  selectedExceptionSku: null,
  selectedScenarioRunId: null,
  selectedMasterProposalIndex: 0,
  selectedMasterChangeId: null,
  selectedProductFamily: null,
  selectedProductFamilyLink: null,
  futureInventorySelection: {
    caseId: null,
    sku: null,
    weekFrom: 1,
    weekThrough: null,
  },
  selectedWhiteBoxTraceKey: null,
  focusedPanel: null,
  focusedPanelParent: null,
  focusedPanelNextSibling: null,
  focusedPanelCollapseKey: null,
  focusedPanelWasExpanded: null,
  ddmrpShowAll: false,
  ddmrpMissingOnly: false,
};

const workspaceRoutes = Object.freeze({
  "#history-review-panel": Object.freeze({ stageId: "history-review-panel", viewId: null, targetId: "history-review-panel", title: "历史回顾", parentTitle: "主业务流程", requiredHostId: null }),
  "#history-review-panel/operating-results": Object.freeze({ stageId: "history-review-panel", viewId: "operating-results", targetId: "history-operating-results-view", title: "经营结果", parentTitle: "历史回顾", requiredHostId: null }),
  "#history-review-panel/buffer-performance": Object.freeze({ stageId: "history-review-panel", viewId: "buffer-performance", targetId: "history-buffer-performance-view", title: "缓冲表现", parentTitle: "历史回顾", requiredHostId: null }),
  "#history-review-panel/sizing-trace": Object.freeze({ stageId: "history-review-panel", viewId: "sizing-trace", targetId: "history-sizing-trace-view", title: "定容追溯", parentTitle: "历史回顾", requiredHostId: null }),
  "#history-review-panel/capacity-constraints": Object.freeze({ stageId: "history-review-panel", viewId: "capacity-constraints", targetId: "history-capacity-constraints-view", title: "能力约束", parentTitle: "历史回顾", requiredHostId: null }),
  "#current-baseline-panel": Object.freeze({ stageId: "current-baseline-panel", viewId: null, targetId: "current-baseline-panel", title: "当前状态基线", parentTitle: "主业务流程", requiredHostId: null }),
  "#current-baseline-panel/meeting-snapshot": Object.freeze({ stageId: "current-baseline-panel", viewId: "meeting-snapshot", targetId: "baseline-meeting-snapshot-view", title: "会前快照", parentTitle: "当前状态基线", requiredHostId: null }),
  "#current-baseline-panel/evidence-review": Object.freeze({ stageId: "current-baseline-panel", viewId: "evidence-review", targetId: "baseline-evidence-review-view", title: "证据检查", parentTitle: "当前状态基线", requiredHostId: null }),
  "#current-baseline-panel/version-freeze": Object.freeze({ stageId: "current-baseline-panel", viewId: "version-freeze", targetId: "baseline-version-freeze-view", title: "版本冻结", parentTitle: "当前状态基线", requiredHostId: null }),
  "#current-baseline-panel/audit-records": Object.freeze({ stageId: "current-baseline-panel", viewId: "audit-records", targetId: "baseline-audit-records-view", title: "审计记录", parentTitle: "当前状态基线", requiredHostId: null }),
  "#future-scenario-panel": Object.freeze({ stageId: "future-scenario-panel", viewId: null, targetId: "future-scenario-panel", title: "未来场景模拟", parentTitle: "主业务流程", requiredHostId: null }),
  "#future-scenario-panel/scenario-config": Object.freeze({ stageId: "future-scenario-panel", viewId: "scenario-config", targetId: "scenario-run-panel", title: "场景配置", parentTitle: "未来场景模拟", requiredHostId: null }),
  "#future-scenario-panel/plan-comparison": Object.freeze({ stageId: "future-scenario-panel", viewId: "plan-comparison", targetId: "scenario-comparison", title: "方案比较", parentTitle: "未来场景模拟", requiredHostId: null }),
  "#future-scenario-panel/inventory-buffer": Object.freeze({ stageId: "future-scenario-panel", viewId: "inventory-buffer", targetId: "buffer-trend-panel", title: "库存缓冲", parentTitle: "未来场景模拟", requiredHostId: null }),
  "#future-scenario-panel/capacity-buffer": Object.freeze({ stageId: "future-scenario-panel", viewId: "capacity-buffer", targetId: "rccp-panel", title: "能力缓冲", parentTitle: "未来场景模拟", requiredHostId: null }),
  "#future-scenario-panel/supply-risk": Object.freeze({ stageId: "future-scenario-panel", viewId: "supply-risk", targetId: "projected-supply-panel", title: "供应风险", parentTitle: "未来场景模拟", requiredHostId: null }),
  "#future-scenario-panel/breach-analysis": Object.freeze({ stageId: "future-scenario-panel", viewId: "breach-analysis", targetId: "variance-panel", title: "击穿分析", parentTitle: "未来场景模拟", requiredHostId: null }),
  "#ddom-decision-panel": Object.freeze({ stageId: "ddom-decision-panel", viewId: null, targetId: "ddom-decision-panel", title: "DDOM 配置决策", parentTitle: "主业务流程", requiredHostId: null }),
  "#ddom-decision-panel/structure-settings": Object.freeze({ stageId: "ddom-decision-panel", viewId: "structure-settings", targetId: "ddom-structure-settings-view", title: "结构设置", parentTitle: "DDOM 配置决策", requiredHostId: null }),
  "#ddom-decision-panel/parameter-decision": Object.freeze({ stageId: "ddom-decision-panel", viewId: "parameter-decision", targetId: "ddom-parameter-decision-view", title: "参数决策", parentTitle: "DDOM 配置决策", requiredHostId: null }),
  "#ddom-decision-panel/temporary-adjustment": Object.freeze({ stageId: "ddom-decision-panel", viewId: "temporary-adjustment", targetId: "ddom-temporary-adjustment-view", title: "临时调整", parentTitle: "DDOM 配置决策", requiredHostId: null }),
  "#ddom-decision-panel/change-records": Object.freeze({ stageId: "ddom-decision-panel", viewId: "change-records", targetId: "ddom-change-records-view", title: "变更记录", parentTitle: "DDOM 配置决策", requiredHostId: null }),
  "#coordination-panel": Object.freeze({ stageId: "coordination-panel", viewId: null, targetId: "coordination-panel", title: "行动和决策", parentTitle: "主业务流程", requiredHostId: null }),
  "#coordination-panel/issue-list": Object.freeze({ stageId: "coordination-panel", viewId: "issue-list", targetId: "coordination-issue-list-view", title: "问题清单", parentTitle: "行动和决策", requiredHostId: null }),
  "#coordination-panel/action-tracking": Object.freeze({ stageId: "coordination-panel", viewId: "action-tracking", targetId: "coordination-action-tracking-view", title: "行动跟踪", parentTitle: "行动和决策", requiredHostId: null }),
  "#coordination-panel/decision-records": Object.freeze({ stageId: "coordination-panel", viewId: "decision-records", targetId: "coordination-decision-records-view", title: "决策记录", parentTitle: "行动和决策", requiredHostId: null }),
  "#coordination-panel/outcome-validation": Object.freeze({ stageId: "coordination-panel", viewId: "outcome-validation", targetId: "coordination-outcome-validation-view", title: "结果验证", parentTitle: "行动和决策", requiredHostId: null }),
  "#trace-panel": Object.freeze({ stageId: "validation", viewId: "white-box-trace", targetId: "trace-panel", title: "白盒追踪", parentTitle: "验证与追踪", requiredHostId: "saved-scenarios-panel" }),
  "#public-demo-golden-loop-panel": Object.freeze({ stageId: "validation", viewId: "public-demo", targetId: "public-demo-golden-loop-panel", title: "公开演示闭环", parentTitle: "验证与追踪", requiredHostId: null }),
});

const workspaceRouteAliases = Object.freeze({
  "#overview-panel": "#ddom-decision-panel/structure-settings",
  "#product-family-dashboard-panel": "#history-review-panel/operating-results",
  "#data-readiness-panel": "#current-baseline-panel/meeting-snapshot",
  "#scenario-run-panel": "#future-scenario-panel/scenario-config",
  "#scenario-comparison": "#future-scenario-panel/plan-comparison",
  "#buffer-trend-panel": "#future-scenario-panel/inventory-buffer",
  "#rccp-panel": "#future-scenario-panel/capacity-buffer",
  "#projected-supply-panel": "#future-scenario-panel/supply-risk",
  "#variance-panel": "#future-scenario-panel/breach-analysis",
  "#master-settings-panel": "#ddom-decision-panel/parameter-decision",
  "#saved-scenarios-panel": "#coordination-panel/action-tracking",
});

const workspaceTargetIds = Object.freeze([...new Set(Object.values(workspaceRoutes).map(route => route.targetId))]);

const selectors = {
  family: document.querySelector("#family-filter"),
  sku: document.querySelector("#sku-filter"),
  resource: document.querySelector("#resource-filter"),
  risk: document.querySelector("#risk-filter"),
};

const previewControls = {
  template: document.querySelector("#template-select"),
  adoptionConstraint: document.querySelector("#adoption-constraint-select"),
  sku: document.querySelector("#preview-sku-select"),
  prebuildWeek: document.querySelector("#prebuild-week"),
  prebuildQuantity: document.querySelector("#prebuild-quantity"),
  capacityResource: document.querySelector("#capacity-resource-select"),
  capacityWeek: document.querySelector("#capacity-week"),
  capacityMultiplier: document.querySelector("#capacity-multiplier"),
  moqOverride: document.querySelector("#moq-override"),
  orderCycleOverride: document.querySelector("#order-cycle-override"),
  supplierLimit: document.querySelector("#supplier-limit-select"),
  supplierLimitStartWeek: document.querySelector("#supplier-limit-start-week"),
  supplierLimitEndWeek: document.querySelector("#supplier-limit-end-week"),
  supplierCapacityLimit: document.querySelector("#supplier-capacity-limit"),
};

const navigationHelp = {
  historyReview: "用累计提前期确定详细窗口，并回顾经营结果、保护关系、区域停留、能力保护和约束类型。",
  currentBaseline: "核对库存、在途、积压、在制品、供应承诺、资源能力、临时措施和当前主设置证据，再冻结不可变版本。",
  futureScenario: "严格分开外部事件和企业响应，在同一冻结基线下比较不采取措施与多个响应方案。",
  ddomDecision: "将结构设置、主参数和临时调整组成可审计变更包，由人工评审、批准和生效。",
  coordination: "记录问题、影响、负责人、截止日期、升级、决策和最终实际效果。",
  overview: "查看全局 KPI 与当前运行状态，判断本次场景工作台是否可用于会议评审。",
  publicDemoGoldenLoop: "读取 PUBLIC-DEMO-GOLDEN-DATA-V1 文件化演示包，生成 DDAE 到 SDBR 的配置 payload，并解释 SDBR 回传 feedback。",
  productFamilyDashboard: "按产品族聚合查看服务、流速、库存、RCCP、供应缺口和预算偏差，避免默认陷入 SKU 明细。",
  networkScoring: "打开独立网络结构评分产品，基于已发布 BOM、供应来源、资源路线、库存位置和缓冲设置发现候选控制点与缓冲点。",
  dataReadiness: "核对主数据覆盖、筛选对象和采纳约束，确保场景输入口径一致。",
  exceptions: "从异常 SKU 进入场景配置，优先处理服务损失、需求尖峰和缓冲风险。",
  scenarioRun: "配置模板、参数覆盖、供应限制并运行非持久化场景预览。",
  scenarioComparison: "比较基准方案与预览方案的服务、库存、产能、供应和预算影响。",
  bufferTrend: "查看 SKU 补货前后净流量、期末在手库存和补货触发。",
  rccpConstraint: "查看资源负荷、受限 / 不受限缺口、瓶颈资源和动作建议。",
  supplierDemand: "钻取供应商、物料族、SKU 与补货订单造成的供应需求。",
  scenarioTrace: "查看已保存场景、保存状态和场景运行审计链。",
  masterSettings: "将预览结果转成 DDOM 主设置变更建议，并治理状态流转。",
  whiteBoxTrace: "查看输入、计算、约束、建议和结果之间的白盒解释链。",
};

const previewFieldHelp = {
  场景模板: "选择一组预设业务动作，例如提前建库、产能调整或供应受限对比。",
  采纳约束: "选择本次方案评审偏好，如服务优先、流速优先、现金优先、产能优先或供应优先。",
  "目标 SKU": "指定本次场景动作主要作用的 SKU，异常带入场景时会自动填充。",
  提前建库周: "指定提前释放补货订单的目标周，用于在峰值前建立保护库存。",
  提前建库数量: "本次场景要提前建库的数量，影响库存水位、现金占用和资源负荷。",
  产能资源: "指定要临时调整能力的关键资源，例如热真空试验舱或总装工位。",
  产能调整周: "指定产能倍率生效的周，用于验证瓶颈周是否可通过增班、外协或日历调整缓解。",
  能力倍率: "资源可用能力的临时倍率。1.20 表示该周能力提升 20%，0.80 表示能力降低 20%。",
  "MOQ 覆盖值": "临时覆盖该 SKU 的最小订货量，用于评估批量规则变化对库存和补货频率的影响。",
  订货周期覆盖值: "临时覆盖该 SKU 的复核 / 订货节奏，必须大于 0；进入黄区后也要到达订货周期才生成补货建议。",
  供应限制: "指定供应商与物料族的能力约束对象，用于计算供应侧受限 / 不受限缺口。",
  供应限制开始周: "该供应能力约束开始生效的周。只影响指定供应商与物料族在该周段内的受限能力。",
  供应限制结束周: "该供应能力约束结束生效的周。开始周到结束周之间都会应用供应承诺能力。",
  供应承诺能力: "供应商在指定周段内可承诺交付的数量上限，用于受限 / 不受限缺口计算。",
};

const collapseState = new Map();
const ddmrpCompactLimit = 6;

const collapsiblePanelConfigs = [
  { selector: "#data-readiness-panel .readiness-panel", defaultExpanded: true },
  { selector: "#public-demo-golden-loop-panel .public-demo-card", defaultExpanded: false },
  { selector: "#public-demo-golden-loop-panel .public-demo-card:first-of-type", defaultExpanded: true },
  { selector: "#product-family-dashboard-panel .product-family-block", defaultExpanded: false },
  { selector: "#product-family-dashboard-panel .product-family-block:first-of-type", defaultExpanded: true },
  { selector: "#product-family-dashboard-panel .product-family-detail", defaultExpanded: false },
  { selector: "#scenario-run-panel .scenario-config-panel", defaultExpanded: true },
  { selector: "#scenario-run-panel .scenario-run-layout > section:not(.scenario-config-panel)", defaultExpanded: true, title: "可选模板", kicker: "模板与动作" },
  { selector: "#scenario-run-panel #scenario-save-panel", defaultExpanded: false },
  { selector: "#scenario-comparison .budget-panel", defaultExpanded: false },
  { selector: "#buffer-trend-panel .buffer-visual-panel", defaultExpanded: false },
  { selector: "#buffer-trend-panel .buffer-visual-panel:first-of-type", defaultExpanded: true },
  { selector: "#buffer-trend-panel .single-sku-card", defaultExpanded: false },
  { selector: "#buffer-trend-panel .single-sku-card:first-child", defaultExpanded: true },
  { selector: "#rccp-panel .rccp-block", defaultExpanded: false },
  { selector: "#rccp-panel .rccp-block:first-child", defaultExpanded: true },
  { selector: "#rccp-panel .rccp-detail", defaultExpanded: false },
  { selector: "#projected-supply-panel .rccp-block", defaultExpanded: false },
  { selector: "#projected-supply-panel .rccp-block:first-child", defaultExpanded: true },
  { selector: "#projected-supply-panel .rccp-detail", defaultExpanded: false },
  { selector: "#variance-panel .rccp-block", defaultExpanded: false },
  { selector: "#variance-panel .rccp-block:first-child", defaultExpanded: true },
  { selector: "#scenario-comparison .saved-run-list", defaultExpanded: true, title: "已保存场景列表", kicker: "场景记录" },
  { selector: "#scenario-comparison .readiness-panel", defaultExpanded: false },
  { selector: "#master-settings-panel .master-settings-block", defaultExpanded: false },
  { selector: "#master-settings-panel .master-settings-block:first-child", defaultExpanded: true },
  { selector: "#ddom-change-records-view .master-settings-block", defaultExpanded: false },
];

const saveControls = {
  panel: document.querySelector("#scenario-save-panel"),
  name: document.querySelector("#scenario-save-name"),
  description: document.querySelector("#scenario-save-description"),
  createdBy: document.querySelector("#scenario-save-created-by"),
  button: document.querySelector("#save-scenario"),
  status: document.querySelector("#scenario-save-status"),
  listBody: document.querySelector("#saved-scenario-body"),
  auditList: document.querySelector("#scenario-audit-list"),
  summary: document.querySelector("#scenario-detail-summary"),
  lineageList: document.querySelector("#scenario-lineage-list"),
  title: document.querySelector("#saved-scenario-title"),
  detailStatus: document.querySelector("#saved-scenario-status"),
};

const masterSettingControls = {
  status: document.querySelector("#master-setting-status"),
  kpis: document.querySelector("#master-settings-kpis"),
  board: document.querySelector("#master-setting-board"),
  currentBody: document.querySelector("#master-current-settings-body"),
  proposalBody: document.querySelector("#master-setting-proposal-body"),
  changeBody: document.querySelector("#master-setting-change-body"),
  detail: document.querySelector("#master-setting-detail"),
  detailTitle: document.querySelector("#master-setting-detail-title"),
  lineageList: document.querySelector("#master-setting-lineage-list"),
  auditList: document.querySelector("#master-setting-audit-list"),
};

const numberFormat = new Intl.NumberFormat("zh-CN", { maximumFractionDigits: 1 });
const moneyFormat = new Intl.NumberFormat("zh-CN", { style: "currency", currency: "CNY", maximumFractionDigits: 0 });

function valueOr(value, fallback) {
  return value === null || value === undefined ? fallback : value;
}

function number(value) {
  return numberFormat.format(Number(valueOr(value, 0)));
}

function money(value) {
  return moneyFormat.format(Number(valueOr(value, 0)));
}

function percent(value) {
  return `${number(value)}%`;
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

function helpTrigger(text) {
  const safeText = escapeHtml(text);
  return `<span class="help-trigger" tabindex="0" aria-label="${safeText}">?<span class="help-tooltip" role="tooltip">${safeText}</span></span>`;
}

function attachInlineHelp() {
  document.querySelectorAll(".form-grid.compact label > span:first-child").forEach(label => {
    const key = label.textContent.trim();
    const help = previewFieldHelp[key];
    if (!help || label.querySelector(".help-trigger")) return;
    label.insertAdjacentHTML("beforeend", helpTrigger(help));
  });

  document.querySelectorAll(".nav-item[data-help-key]").forEach(item => {
    const help = navigationHelp[item.dataset.helpKey];
    if (!help) return;
    item.setAttribute("title", help);
    const label = item.querySelector("span:not(.nav-index)")?.textContent.trim() || item.textContent.trim();
    item.setAttribute("aria-label", `${label}：${help}`);
  });
}

function panelKey(panel, index) {
  if (panel.id) return panel.id;
  if (!panel.dataset.collapseKey) {
    panel.dataset.collapseKey = `collapse-panel-${index}`;
  }
  return panel.dataset.collapseKey;
}

function ensurePanelHeading(panel, config) {
  const existing = Array.from(panel.children).find(child => child.classList?.contains("panel-heading"));
  if (existing) return existing;

  const heading = document.createElement("div");
  heading.className = "panel-heading compact-heading";
  heading.innerHTML = `<div><span class="panel-kicker">${escapeHtml(config.kicker || "明细")}</span><h2>${escapeHtml(config.title || "工作区")}</h2></div>`;
  panel.insertBefore(heading, panel.firstChild);
  return heading;
}

function ensureCollapseBody(panel, heading, key) {
  const existing = panel.querySelector(":scope > [data-collapse-body]");
  if (existing) return existing;

  const body = document.createElement("div");
  body.className = "collapse-body";
  body.dataset.collapseBody = "";
  body.id = `${key}-body`;

  const nodes = Array.from(panel.childNodes).filter(node => node !== heading);
  nodes.forEach(node => body.appendChild(node));
  panel.appendChild(body);
  return body;
}

function setCollapseState(panel, expanded) {
  const heading = panel.querySelector(":scope > [data-collapse-toggle]");
  const body = panel.querySelector(":scope > [data-collapse-body]");
  if (!heading || !body) return;

  panel.classList.toggle("is-collapsed", !expanded);
  heading.setAttribute("aria-expanded", String(expanded));
  body.hidden = !expanded;
  const indicator = heading.querySelector(".collapse-indicator");
  if (indicator) {
    indicator.textContent = expanded ? "收起" : "展开";
  }
  const focusAction = heading.querySelector("[data-focus-panel]");
  if (focusAction && !panel.classList.contains("is-focused-panel")) {
    focusAction.hidden = !expanded;
    focusAction.setAttribute("aria-hidden", String(!expanded));
  }
}

function initializeCollapsiblePanels() {
  const panelDefaults = new Map();
  collapsiblePanelConfigs.forEach(config => {
    document.querySelectorAll(config.selector).forEach(panel => {
      panelDefaults.set(panel, config);
    });
  });

  Array.from(panelDefaults.entries()).forEach(([panel, config], index) => {
    const key = panelKey(panel, index);
    const heading = ensurePanelHeading(panel, config);
    const body = ensureCollapseBody(panel, heading, key);
    panel.dataset.collapsePanel = "";
    panel.classList.add("collapsible-panel");
    heading.dataset.collapseToggle = "";
    heading.classList.add("collapse-toggle");
    heading.setAttribute("role", "button");
    heading.setAttribute("tabindex", "0");
    heading.setAttribute("aria-controls", body.id);
    if (!heading.querySelector(".collapse-indicator")) {
      heading.insertAdjacentHTML("beforeend", `<span class="collapse-indicator" aria-hidden="true"></span>`);
    }

    const expanded = collapseState.has(key) ? collapseState.get(key) : config.defaultExpanded !== false;
    setCollapseState(panel, expanded);
  });
}

function initializePanelWorkspaceActions() {
  document.querySelectorAll("[data-collapse-panel]").forEach(panel => {
    const heading = panel.querySelector(":scope > [data-collapse-toggle]");
    if (!heading || heading.querySelector("[data-focus-panel]")) return;
    const action = document.createElement("button");
    action.type = "button";
    action.className = "panel-action-button";
    action.dataset.focusPanel = "";
    action.textContent = "专注查看";
    action.setAttribute("aria-label", "专注查看当前模块");
    const indicator = heading.querySelector(".collapse-indicator");
    heading.insertBefore(action, indicator || null);
    const expanded = heading.getAttribute("aria-expanded") !== "false";
    action.hidden = !expanded;
    action.setAttribute("aria-hidden", String(!expanded));
  });
}

function initializeResizableTables() {
  document.querySelectorAll(".table-scroll").forEach(container => {
    container.classList.add("resizable-table-shell");
  });
}

function toggleCollapsiblePanel(heading) {
  const panel = heading.closest("[data-collapse-panel]");
  if (!panel) return;
  if (state.focusedPanel === panel) return;
  const key = panelKey(panel, 0);
  const expanded = heading.getAttribute("aria-expanded") !== "true";
  collapseState.set(key, expanded);
  setCollapseState(panel, expanded);
}

function openFocusedPanel(panel) {
  if (!panel || state.focusedPanel === panel) return;
  const wasExpanded = panel.querySelector(":scope > [data-collapse-toggle]")?.getAttribute("aria-expanded") !== "false";
  if (!wasExpanded) return;
  if (state.focusedPanel) {
    closeFocusedPanel();
  }

  const layer = byId("workspace-focus-layer");
  const stage = layer?.querySelector(".focus-stage");
  if (!layer || !stage) return;

  state.focusedPanel = panel;
  state.focusedPanelParent = panel.parentNode;
  state.focusedPanelNextSibling = panel.nextSibling;
  state.focusedPanelCollapseKey = panelKey(panel, 0);
  state.focusedPanelWasExpanded = wasExpanded;
  setCollapseState(panel, true);
  panel.classList.add("is-focused-panel");
  const action = panel.querySelector("[data-focus-panel]");
  if (action) {
    action.textContent = "退出专注";
    action.setAttribute("aria-label", "退出专注查看");
    action.hidden = false;
    action.setAttribute("aria-hidden", "false");
  }
  stage.appendChild(panel);
  layer.hidden = false;
  layer.setAttribute("aria-hidden", "false");
  document.body.classList.add("has-focus-panel");
  action?.focus();
}

function closeFocusedPanel() {
  const panel = state.focusedPanel;
  if (!panel) return;

  const parent = state.focusedPanelParent;
  const next = state.focusedPanelNextSibling;
  panel.classList.remove("is-focused-panel");
  const action = panel.querySelector("[data-focus-panel]");
  if (action) {
    action.textContent = "专注查看";
    action.setAttribute("aria-label", "专注查看当前模块");
  }

  if (parent) {
    parent.insertBefore(panel, next && next.parentNode === parent ? next : null);
  }
  if (state.focusedPanelCollapseKey) {
    collapseState.set(state.focusedPanelCollapseKey, state.focusedPanelWasExpanded !== false);
  }
  setCollapseState(panel, state.focusedPanelWasExpanded !== false);

  const layer = byId("workspace-focus-layer");
  if (layer) {
    layer.hidden = true;
    layer.setAttribute("aria-hidden", "true");
  }
  document.body.classList.remove("has-focus-panel");
  state.focusedPanel = null;
  state.focusedPanelParent = null;
  state.focusedPanelNextSibling = null;
  state.focusedPanelCollapseKey = null;
  state.focusedPanelWasExpanded = null;
  if (action && !action.hidden) {
    action.focus();
  } else {
    panel.querySelector(":scope > [data-collapse-toggle]")?.focus();
  }
}

function openWorkspaceDrawer(title, sections) {
  const drawer = byId("workspace-detail-drawer");
  const titleNode = byId("workspace-drawer-title");
  const body = byId("workspace-drawer-body");
  if (!drawer || !titleNode || !body) return;

  titleNode.textContent = title;
  body.innerHTML = sections.map(section => `
    <section class="drawer-section">
      <h3>${escapeHtml(section.title)}</h3>
      <dl class="drawer-definition-list">
        ${section.items.map(([label, value]) => `
          <div>
            <dt>${escapeHtml(label)}</dt>
            <dd>${value}</dd>
          </div>
        `).join("")}
      </dl>
    </section>
  `).join("");
  drawer.hidden = false;
  drawer.setAttribute("aria-hidden", "false");
  document.body.classList.add("has-workspace-drawer");
  byId("workspace-drawer-close")?.focus();
}

function closeWorkspaceDrawer() {
  const drawer = byId("workspace-detail-drawer");
  if (!drawer) return;
  drawer.hidden = true;
  drawer.setAttribute("aria-hidden", "true");
  document.body.classList.remove("has-workspace-drawer");
}

function parseWorkspaceRoute(hash) {
  const requestedHash = typeof hash === "string" ? hash.trim() : "";
  const normalizedHash = requestedHash && !requestedHash.startsWith("#") ? `#${requestedHash}` : requestedHash;
  const aliasHash = workspaceRouteAliases[normalizedHash];
  const canonicalHash = workspaceRoutes[normalizedHash]
    ? normalizedHash
    : aliasHash || "#history-review-panel";
  const route = workspaceRoutes[canonicalHash];
  return {
    ...route,
    hash: canonicalHash,
    requestedHash: normalizedHash,
    isCanonical: normalizedHash === canonicalHash,
  };
}

function formatWorkspaceHash(route) {
  if (typeof route === "string") return parseWorkspaceRoute(route).hash;
  if (!route) return "#history-review-panel";
  if (route.hash && workspaceRoutes[route.hash]) return route.hash;
  const viewId = route.viewId === undefined ? null : route.viewId;
  const match = Object.entries(workspaceRoutes).find(([, candidate]) => (
    candidate.stageId === route.stageId && candidate.viewId === viewId
  ));
  return match ? match[0] : "#history-review-panel";
}

function resolveWorkspaceRoute(route) {
  return parseWorkspaceRoute(formatWorkspaceHash(route));
}

function navigateWorkspace(stageId, viewId, replace) {
  const route = resolveWorkspaceRoute({ stageId, viewId });
  const hash = formatWorkspaceHash(route);
  if (replace) {
    history.replaceState(null, "", hash);
    applyWorkspaceRoute(route);
    return;
  }
  if (window.location.hash === hash) {
    applyWorkspaceRoute(route);
    return;
  }
  window.location.hash = hash;
}

function setExpandedStageNavigation(stageId) {
  document.querySelectorAll(".nav-stage-group").forEach(group => {
    const expanded = group.dataset.stageId === stageId;
    const toggle = group.querySelector(".nav-stage-toggle");
    const submenu = group.querySelector(".nav-submenu");
    if (toggle) toggle.setAttribute("aria-expanded", String(expanded));
    if (submenu) submenu.hidden = !expanded;
    group.classList.toggle("is-expanded", expanded);
  });
}

function setActiveWorkspaceNavigation(route) {
  document.querySelectorAll(".nav-stage-toggle, .nav-subitem, .validation-group ~ .nav-item").forEach(item => {
    item.classList.remove("is-active", "is-stage-active");
    item.removeAttribute("aria-current");
  });
  if (!route) return;

  const stageToggle = document.querySelector(`.nav-stage-toggle[data-stage-route="#${route.stageId}"]`);
  if (stageToggle) {
    stageToggle.classList.add(route.viewId === null ? "is-active" : "is-stage-active");
    if (route.viewId === null) stageToggle.setAttribute("aria-current", "page");
  }
  const routeItem = document.querySelector(`.nav-subitem[href="${route.hash}"], .validation-group ~ .nav-item[href="${route.hash}"]`);
  if (routeItem) {
    routeItem.classList.add("is-active");
    routeItem.setAttribute("aria-current", "page");
  }
}

function renderWorkspaceBreadcrumb(route) {
  const breadcrumb = byId("workspace-breadcrumb");
  if (!breadcrumb) return;
  breadcrumb.innerHTML = `<span>${escapeHtml(route.parentTitle)}</span><span aria-hidden="true">/</span><strong>${escapeHtml(route.title)}</strong>`;
}

function applyWorkspaceRoute(route) {
  closeFocusedPanel();
  closeWorkspaceDrawer();
  const resolved = resolveWorkspaceRoute(route);
  workspaceTargetIds.forEach(targetId => {
    const target = byId(targetId);
    if (target) target.hidden = true;
  });
  setActiveWorkspaceNavigation(null);
  document.querySelectorAll(".workspace-route-host").forEach(host => {
    host.hidden = true;
  });
  document.querySelectorAll(".workspace-route-extension").forEach(extension => {
    extension.hidden = extension.dataset.workspaceRouteExtension !== resolved.hash;
  });
  const requiredHost = resolved.requiredHostId ? byId(resolved.requiredHostId) : null;
  if (requiredHost) requiredHost.hidden = false;
  const target = byId(resolved.targetId);
  if (target) target.hidden = false;
  setExpandedStageNavigation(resolved.stageId);
  setActiveWorkspaceNavigation(resolved);
  renderWorkspaceBreadcrumb(resolved);
  document.title = `${resolved.title} - DDAE 五阶段工作台`;
  const workspace = byId("workspace");
  if (workspace) workspace.scrollTop = 0;
  return resolved;
}

function handleWorkspaceHashChange() {
  const route = parseWorkspaceRoute(window.location.hash);
  if (window.location.hash !== route.hash) {
    history.replaceState(null, "", route.hash);
  }
  applyWorkspaceRoute(route);
}

function initializeWorkspaceUi() {
  attachInlineHelp();
  initializeCollapsiblePanels();
  initializeDdmrpStandardReferencePanel();
  initializePanelWorkspaceActions();
  initializeResizableTables();
  document.querySelectorAll(".nav-stage-toggle").forEach(toggle => {
    toggle.addEventListener("click", () => {
      const route = parseWorkspaceRoute(toggle.dataset.stageRoute);
      navigateWorkspace(route.stageId, route.viewId, false);
    });
  });
  window.addEventListener("hashchange", handleWorkspaceHashChange);
  handleWorkspaceHashChange();
}

function row(cells) {
  return `<tr>${cells.map(cell => `<td>${cell}</td>`).join("")}</tr>`;
}

function emptyRow(message, columns = 4) {
  return `<tr><td class="empty-cell" colspan="${columns}">${message}</td></tr>`;
}

function statusClass(status) {
  const normalized = String(valueOr(status, "neutral")).toLowerCase();
  return `status-chip ${({
    green: "is-valid",
    yellow: "is-warning",
    red: "is-invalid",
    deepred: "is-critical",
    healthy: "is-valid",
    warning: "is-warning",
    blocked: "is-invalid",
    blue: "is-overgreen",
    overtopofgreen: "is-overgreen",
  })[normalized] || "neutral"}`;
}

function statusLabel(status) {
  return ({
    Green: "绿色",
    Yellow: "黄色",
    Red: "红色",
    DeepRed: "深红色",
    Blue: "超绿",
    OverTopOfGreen: "超绿",
    Healthy: "健康",
    Warning: "预警",
    Blocked: "阻塞",
    Complete: "完整",
    EvidenceMissing: "证据缺失",
    Adoptable: "可评审",
    Reconcile: "待协调",
    Candidate: "未选定",
    Selected: "已选定",
    Superseded: "已替代",
    Withdrawn: "已撤回",
    Draft: "草稿",
    Submitted: "已提交",
    Reviewed: "已评审",
    Approved: "已批准",
    Effective: "已生效",
    Expired: "已失效",
    NotRun: "未验证",
    Passed: "验证通过",
    Failed: "验证失败",
    NotApplicable: "不适用",
  })[status] || valueOr(status, "-");
}

function baselineSourceLabel(source) {
  const normalized = String(valueOr(source, ""));
  const mapped = ({
    DemoFixture: "演示数据",
    Manual: "人工录入",
    "DDAE Governance": "DDAE 治理记录",
    "DDAE Demo Inventory Adapter": "DDAE 库存演示适配器",
    "DDAE Demo Supply Adapter": "DDAE 供应演示适配器",
    "DDAE Demo Demand Adapter": "DDAE 需求演示适配器",
    "DDAE Demo WIP Evidence": "DDAE 在制品演示证据",
    "DDAE Demo Supplier Evidence": "DDAE 供应商演示证据",
    "DDAE Demo Capacity Evidence": "DDAE 能力演示证据",
    "DDAE Demo Governance": "DDAE 治理演示证据",
    "DDAE Demo Planning Snapshot": "DDAE 计划快照演示证据",
    "DDAE Demo Meeting Snapshot": "DDAE 会前快照演示证据",
    "DDAE Demo Time Evidence": "DDAE 时间缓冲演示证据",
    "DDAE DemoFixture explicit historical operating ledger": "DDAE 历史演示台账",
    "DDAE 演示历史事实台账": "DDAE 历史演示台账",
    "DDAE Internal Operating Fact Set": "DDAE 内部经营事实集",
  })[source];
  if (mapped) return mapped;
  if (normalized.includes("DemoFixture")) return "演示数据";
  if (normalized.startsWith("DDAE Demo")) return "DDAE 演示证据";
  return valueOr(source, "证据缺失");
}

function baselineActorLabel(value) {
  if (value === `Codex ${String.fromCharCode(63, 63)}`) return "Codex 烟测";
  return ({
    "Codex verification": "Codex 验证",
    "Codex final smoke": "Codex 最终烟测",
  })[value] || valueOr(value, "-");
}

function freshnessLabel(status) {
  return ({
    Fresh: "截止时间有效",
    Stale: "已过截止时间",
    EvidenceMissing: "证据缺失",
    NotApplicable: "不适用",
  })[status] || valueOr(status, "证据缺失");
}

function completenessLabel(status) {
  return ({
    Complete: "完整",
    Incomplete: "不完整",
    Missing: "证据缺失",
    EvidenceMissing: "证据缺失",
    NotApplicable: "不适用",
  })[status] || valueOr(status, "证据缺失");
}

function baselineStatusLabel(status) {
  return ({
    Frozen: "已冻结",
    Proposed: "待评审",
  })[status] || valueOr(status, "证据缺失");
}

function coordinationStatusLabel(status) {
  return ({
    Open: "待处理",
    InProgress: "进行中",
    Escalated: "已升级",
    Completed: "已完成",
  })[status] || valueOr(status, "证据缺失");
}

function businessUnitLabel(unit) {
  const normalized = String(unit || "").trim();
  return ({
    units: "件",
    "units/week": "件/周",
    factor: "倍",
    days: "天",
    week: "周",
    weeks: "周",
    capacity: "能力单位",
  })[normalized] || normalized;
}

function breachScopeLabel(scopeType) {
  return ({
    InventoryBuffer: "库存缓冲",
    TimeBuffer: "时间缓冲",
    CapacityBuffer: "能力缓冲",
    SupplyRisk: "供应风险",
    Inventory: "库存缓冲",
    Time: "时间缓冲",
    Capacity: "能力缓冲",
    Supply: "供应风险",
  })[scopeType] || valueOr(scopeType, "证据缺失");
}

function metricOrEvidenceMissing(value, formatter = item => String(item)) {
  return value === null || value === undefined || value === ""
    ? "证据缺失"
    : formatter(value);
}

function evidenceStatusLabel(status) {
  return completenessLabel(status);
}

function baselineSectionLabel(name) {
  return ({
    "Meeting snapshot KPIs": "会前状态指标",
    "Sequenced resource-routing evidence": "资源顺序证据",
    "Upstream capacity-protection evidence": "上游能力保护证据",
    "Time-buffer definitions": "时间缓冲定义",
    "Time-buffer product scopes": "时间缓冲产品范围",
    "Time-buffer control-point progress": "时间缓冲控制点进度",
  })[name] || valueOr(name, "证据缺失");
}

function businessEvidenceLabel(value) {
  return String(metricOrEvidenceMissing(value))
    .replace(/\bDemoFixture\b/g, "演示数据")
    .replace(/\bregistered validation data\b/gi, "登记校验证据")
    .replace(/\bEvidenceMissing\b/g, "证据缺失")
    .replace(/\bNotApplicable\b/g, "不适用")
    .replace(/\bInProgress\b/g, "进行中")
    .replace(/\bEscalated\b/g, "已升级")
    .replace(/\bCompleted\b/g, "已完成")
    .replace(/\bProposed\b/g, "待评审")
    .replace(/\bDraft\b/g, "草稿")
    .replace(/\bSubmitted\b/g, "已提交")
    .replace(/\bPassed\b/g, "已通过")
    .replace(/\bFailed\b/g, "未通过")
    .replace(/\bNotRun\b/g, "未运行")
    .replace(/\bAdoptable\b/g, "可采纳")
    .replace(/\bReconcile\b/g, "需协同")
    .replace(/\bBlocked\b/g, "已阻断")
    .replace(/\bCurrent\b/g, "当前")
    .replace(/\bReviewed\b/g, "已评审")
    .replace(/\bApproved\b/g, "已批准")
    .replace(/\bEffective\b/g, "已生效")
    .replace(/\bExpired\b/g, "已失效")
    .replace(/\bIncomplete\b/g, "不完整")
    .replace(/\bStale\b/g, "已过截止时间")
    .replace(/\bApplicable\b/g, "适用")
    .replace(/\bCritical\b/g, "关键")
    .replace(/\bFrozen\b/g, "已冻结")
    .replace(/\bOpen\b/g, "待处理")
    .replace(/\bComplete\b/g, "完整")
    .replace(/\bFresh\b/g, "截止时间有效")
    .replace(/(\d+)-week rolling window/g, "$1 周滚动窗口")
    .replace(/Week (\d+): ObservedDelayDays is missing/g, "第 $1 周：实测延误天数缺失")
    .replace(/Week (\d+): EvidenceStatus=/g, "第 $1 周：证据状态=")
    .replace(/\bScenario Preview\b/g, "场景预览")
    .replace(/\brun\b/gi, "场景运行")
    .replaceAll("已从冻结基线和保存 场景运行 白盒复算。", "已从冻结基线和已保存场景运行进行白盒复算。")
    .replaceAll("Historical closing balances to current baseline", "历史期末余额衔接至当前基线")
    .replace(/\bBalanced\b/g, "综合平衡")
    .replace(/\bServiceFirst\b/g, "服务优先")
    .replace(/\bFlowFirst\b/g, "流速优先")
    .replace(/\bCashFirst\b/g, "现金优先")
    .replace(/\bCapacityFirst\b/g, "产能优先")
    .replace(/\bSupplyFirst\b/g, "供应优先")
    .replace(/\btrace\b/gi, "追踪记录")
    .replace(/\bdemonstrated ADU\b/g, "经验证 ADU")
    .replace(/\bZone Adjustment Factor\b/g, "区域调整因子")
    .replace(/\bZone (?=\d)/g, "区域调整因子 ")
    .replace(/\bVF (?=\d)/g, "变异因子 ")
    .replace(/\bsizing\b/g, "定容")
    .replace(/(\d+(?:\.\d+)?)d\b/g, "$1 天")
    .replaceAll("BufferDays is missing or invalid", "缓冲天数缺失或无效")
    .replaceAll("Product scope evidence is missing", "产品范围证据缺失")
    .replaceAll("Control-point progress evidence is missing", "控制点进度证据缺失")
    .replaceAll("No time-buffer definitions are configured.", "未配置时间缓冲定义。")
    .replaceAll("Resource-routing sequence evidence is missing.", "资源顺序证据缺失。")
    .replaceAll("Sequenced upstream capacity-protection evidence is missing.", "上游能力保护顺序证据缺失。");
}

function historyEvidenceSummary(value) {
  const evidence = String(valueOr(value, ""));
  if (!evidence) return "证据缺失";
  const sourceAuthority = evidence.match(/(?:^| \/ )SourceAuthority=([^/]+?)(?= \/ |$)/)?.[1]?.trim();
  const asOf = evidence.match(/(?:^| \/ )AsOf=([^/]+?)(?= \/ |$)/)?.[1]?.trim();
  const summary = [baselineSourceLabel(evidence.split(" / ")[0])];
  if (sourceAuthority) summary.push(`来源：${baselineSourceLabel(sourceAuthority)}`);
  if (asOf) summary.push(`截止：${asOf}`);
  return summary.join(" · ");
}

function protectionStateLabel(status) {
  return ({
    Designed: "已设计",
    Available: "可用",
    Effective: "有效",
    NotApplicable: "不适用",
    EvidenceMissing: "证据缺失",
    Complete: "完整",
  })[status] || valueOr(status, "证据缺失");
}

function capacityRoleLabel(role, protectedCcrResourceCode) {
  return ({
    UpstreamProtection: `上游保护资源 · 保护 ${valueOr(protectedCcrResourceCode, "CCR")}`,
    CcrUtilization: "CCR 利用率（单列观察）",
    ObservedResource: "一般观察资源",
  })[role] || valueOr(role, "证据缺失");
}

function constraintExposureLabel(type) {
  return ({
    CurrentCcr: "当前 CCR",
    HighLoadResource: "高负荷资源",
    PotentialCcr: "场景潜在 CCR",
    EventConstraint: "事件型约束",
    ExternalConstraint: "外部约束",
  })[type] || valueOr(type, "证据缺失");
}

function mapLabel(dictionary, value) {
  const key = String(valueOr(value, ""));
  return dictionary[key] || key || "-";
}

function caseLabel(name) {
  return ({
    Baseline: "基准方案",
    Scenario: "预览方案",
  })[name] || valueOr(name, "-");
}

function actionTypeLabel(actionType) {
  return ({
    Prebuild: "提前建库",
    CapacityMultiplier: "产能倍率",
    MoqOverride: "MOQ 覆盖",
    OrderCycleOverride: "订货周期覆盖",
    SupplierCapacityLimit: "供应能力限制",
    DemandEvent: "需求事件",
  })[actionType] || valueOr(actionType, "-");
}

function traceStageLabel(stage) {
  return ({
    Data: "数据",
    Scenario: "场景",
    Engine: "白盒引擎",
    Validation: "验证",
    Result: "结果",
    Persistence: "保存状态",
    Demand: "不受限需求",
    Capacity: "受限能力",
    Supply: "受限供应",
    Action: "动作建议",
    Smoke: "历史烟测",
    Governance: "治理",
    Impact: "影响",
    FrozenBaseline: "冻结基线",
    ExternalScenario: "外部场景",
    ResponseConfiguration: "响应配置",
    MasterSettings: "主设置",
    Trace: "追踪记录",
  })[stage] || valueOr(stage, "-");
}

function adoptionConstraintLabel(mode) {
  return ({
    Balanced: "综合平衡",
    ServiceFirst: "服务优先",
    FlowFirst: "流速优先",
    CashFirst: "现金优先",
    CapacityFirst: "产能优先",
    SupplyFirst: "供应优先",
  })[mode] || "综合平衡";
}

function targetFamilies() {
  const data = state.data;
  if (!data) return [];
  if (selectors.family.value) {
    return data.families.filter(item => item.code === selectors.family.value || item.name === selectors.family.value);
  }

  const activeFamilies = new Set(valueOr(state.filtered?.skus, data.skus).map(item => item.family));
  return data.families.filter(item => activeFamilies.has(item.code) || activeFamilies.has(item.name));
}

function averageTarget(selector, fallback = 0) {
  const families = targetFamilies();
  return families.length
    ? families.reduce((sum, item) => sum + Number(valueOr(selector(item), 0)), 0) / families.length
    : fallback;
}

function targetServiceLevel() {
  return averageTarget(item => item.targetServiceLevel, 95);
}

function targetFlowIndex() {
  return averageTarget(item => item.targetFlowIndex, 85);
}

function evaluateAdoption(result) {
  const feasibility = result?.feasibility;
  if (!feasibility) {
    return {
      status: "Red",
      label: "后端可行性结果缺失",
      message: "后端可行性结果缺失；该预览不能作为采纳建议。",
      constraintMode: "Balanced",
      violations: [{
        name: "后端可行性结果缺失",
        current: "-",
        limit: "后端评估",
        reason: "后端未返回统一可行性评估结果。",
        action: "请重新运行后端预览。",
      }],
    };
  }

  const display = {
    Blocked: { status: "Red", label: feasibility.label || "阻断候选", message: "后端可行性评估已阻断该候选。", checkStatus: "Red" },
    Reconcile: { status: "Yellow", label: feasibility.label || "需要协调", message: "后端可行性评估要求协调后继续评审。", checkStatus: "Yellow" },
    Adoptable: { status: "Green", label: feasibility.label || "可作为候选", message: "后端可行性评估允许其作为候选。", checkStatus: null },
  }[feasibility.status] || {
    status: "Red",
    label: "后端可行性结果无效",
    message: "后端返回了无法识别的可行性状态；该预览不能作为采纳建议。",
    checkStatus: "Red",
  };
  const checks = Array.isArray(feasibility.checks) ? feasibility.checks : [];
  const violations = display.checkStatus
    ? checks.filter(item => item.status === display.checkStatus).map(item => ({
      name: item.metric || item.code || "后端可行性检查",
      current: item.actual === null || item.actual === undefined ? "-" : `${number(item.actual)} ${item.unit || ""}`.trim(),
      limit: item.redLimit === null || item.redLimit === undefined
        ? (item.yellowLimit === null || item.yellowLimit === undefined ? "后端评估" : `${number(item.yellowLimit)} ${item.unit || ""}`.trim())
        : `${number(item.redLimit)} ${item.unit || ""}`.trim(),
      reason: item.message || "后端可行性检查需要处理。",
      action: "请依据后端可行性评估处理。",
    }))
    : [];
  return { ...display, constraintMode: feasibility.constraintMode || "Balanced", violations };
}

function triggerLabel(trigger) {
  return ({
    BelowTopOfYellow: "订货周期复核",
    PrebuildCampaign: "提前建库订单",
  })[trigger] || valueOr(trigger, "-");
}

function recommendationTypeLabel(actionType) {
  return ({
    CapacityRelief: "释放产能",
    Prebuild: "提前建库",
    CalendarPolicy: "资源日历",
    DemandUpside: "承接增量",
    Monitor: "持续监控",
    SupplierCoordination: "供应协调",
    CapacityConfirmation: "能力确认",
  })[actionType] || valueOr(actionType, "-");
}

function masterSettingStatusLabel(status) {
  return ({
    Current: "当前",
    Proposed: "待评审",
    Reviewed: "已评审",
    Approved: "已批准",
    Effective: "已生效",
    Expired: "已失效",
  })[status] || valueOr(status, "-");
}

function masterSettingTypeLabel(type) {
  return ({
    "Inventory Buffer": "库存缓冲",
    "Decoupling Point": "解耦点",
    "Time Buffer": "时间缓冲",
    "Capacity Buffer": "产能缓冲",
    "Supplier Master Setting": "供应主设置",
    "SystemSuggested": "系统建议",
  })[type] || valueOr(type, "-");
}

function masterSettingDisplayValue(changeId, field, value) {
  const knownLegacySmokeId = "c65faf1804f64a84b4350868123830b7";
  if (changeId !== knownLegacySmokeId) return valueOr(value, "-");
  const labels = {
    target: "历史烟测目标（非业务数据）",
    createdBy: "Codex 最终烟测",
    currentValue: "历史烟测旧值（非业务数据）",
    proposedValue: "历史烟测建议值（非业务数据）",
    trigger: "历史烟测触发条件",
    effectiveWindow: "历史烟测窗口",
    owner: "历史烟测负责人",
    approver: "历史烟测审批人",
    expectedEffect: "历史烟测预期效果",
    rollbackCondition: "历史烟测回滚条件",
    auditMessage: "历史烟测审计已保留（非业务数据）",
  };
  if (field === "target" && value !== String.fromCharCode(63).repeat(6)) return valueOr(value, "-");
  return labels[field] || valueOr(value, "-");
}

function auditEventLabel(eventType) {
  return ({
    RunRequested: "收到保存请求",
    PreviewRecalculated: "服务端重新计算",
    TraceCaptured: "追踪信息已捕获",
    RunSaved: "场景已保存",
    ChangeProposed: "变更已提出",
    ImpactCaptured: "影响已捕获",
    ChangeSaved: "变更已保存",
    StatusChanged: "状态已流转",
    CoordinationItemCreated: "协调事项已创建",
    DecisionRecorded: "决策已记录",
    OutcomeRecorded: "实际效果已记录",
    BaselineFrozen: "基线已冻结",
    DataRepairApplied: "历史数据纠正已留痕",
    SmokeAudit: "历史烟测审计",
    PackageCreated: "变更包已创建",
    PackageSubmitted: "已提交评审",
    WhiteBoxRecalculated: "白盒已复算",
    ValidationPassed: "白盒验证通过",
    ValidationFailed: "白盒验证失败",
    PackageReviewed: "已标记评审",
    PackageApproved: "已批准",
    PackageEffective: "已生效",
    PackageExpired: "已失效",
  })[eventType] || valueOr(eventType, "-");
}

function baselineAuditMessage(event) {
  const message = valueOr(event.message, "");
  const isKnownCorruptFreeze = event.eventType === "BaselineFrozen"
    && (/\?{2,}/.test(message) || message.includes("\uFFFD"));
  return isKnownCorruptFreeze ? "历史烟测记录已由纠正审计保留" : message;
}

function nextMasterSettingStatus(status) {
  return ({
    Proposed: "Reviewed",
    Reviewed: "Approved",
    Approved: "Effective",
    Effective: "Expired",
  })[status] || null;
}

function bufferCellClass(status) {
  const normalized = String(valueOr(status, "Green"));
  return `buffer-heat-cell ${normalized === "Red" ? "is-red" : normalized === "Yellow" ? "is-yellow" : normalized === "Blue" || normalized === "OverTopOfGreen" ? "is-blue" : normalized === "EvidenceMissing" ? "is-missing" : "is-green"}`;
}

function setWorkspaceStatus(status, message) {
  const chip = byId("route-status");
  chip.className = statusClass(status);
  chip.textContent = message;
  byId("system-health").className = `status-inline ${status === "Red" ? "is-error" : status === "Yellow" ? "is-loading" : ""}`;
  byId("system-health").textContent = status === "Red" ? "不可用" : status === "Yellow" ? "预警" : "健康";
}

function showWorkspaceContent() {
  byId("workspace-loading").hidden = true;
  byId("workspace-error").hidden = true;
  byId("workspace-error-message").textContent = "";
  state.workspaceErrorSource = null;
  applyWorkspaceRoute(parseWorkspaceRoute(window.location.hash));
}

function showWorkspaceError(error, source = "workspace") {
  byId("workspace-loading").hidden = true;
  byId("workspace-error").hidden = false;
  byId("workspace-error-message").textContent = error.message;
  state.workspaceErrorSource = source;
  setWorkspaceStatus("Red", "数据不可用");
}

function clearWorkspaceError(source) {
  if (state.workspaceErrorSource !== source) return;
  state.workspaceErrorSource = null;
  byId("workspace-error").hidden = true;
  byId("workspace-error-message").textContent = "";
  setWorkspaceStatus("Green", "工作台已就绪");
}

function unique(values) {
  return [...new Set(values.filter(Boolean))].sort((left, right) => String(left).localeCompare(String(right), "zh-CN"));
}

function fillSelect(select, label, values, valueLabel = value => value) {
  select.innerHTML = [`<option value="">全部${label}</option>`, ...values.map(value => `<option value="${escapeHtml(value)}">${escapeHtml(valueLabel(value))}</option>`)].join("");
}

function configureFilters(data) {
  fillSelect(selectors.family, "产品族", data.families.map(item => item.code));
  fillSelect(selectors.sku, "SKU", data.skus.map(item => item.sku));
  fillSelect(selectors.resource, "资源", data.resources.map(item => item.code));
  fillSelect(selectors.risk, "风险", unique(data.supplierCapacityWindows.map(item => item.riskStatus)), statusLabel);
  renderScenarioScopeSummary();
}

function renderScenarioScopeSummary() {
  const family = selectors.family?.value || "全部产品族";
  const sku = selectors.sku?.value || "全部 SKU";
  const actionSku = previewControls.sku?.value || "未选择";
  byId("scenario-scope-summary").textContent = `计算范围：${family} / ${sku}（影响汇总并由后端重算）；措施对象 SKU：${actionSku}（只影响提前建库、MOQ 和订货周期动作）。`;
}

function configurePreviewControls(data) {
  previewControls.template.innerHTML = data.scenarioTemplates
    .map(template => `<option value="${template.templateId}">${template.name}</option>`)
    .join("");
  fillSelect(previewControls.sku, "SKU", data.skus.map(item => item.sku));
  fillSelect(previewControls.capacityResource, "资源", data.resources.map(item => item.code));
  previewControls.supplierLimit.innerHTML = [
    `<option value="">不限制供应能力</option>`,
    ...unique(data.supplierCapacityWindows.map(item => `${item.supplier}|${item.materialFamily}`))
      .map(value => {
        const [supplier, materialFamily] = value.split("|");
        return `<option value="${value}">${supplier} / ${materialFamily}</option>`;
      })
  ].join("");

  const defaultTemplate = data.scenarioTemplates[0];
  if (defaultTemplate) {
    previewControls.template.value = defaultTemplate.templateId;
  }
  const defaultSku = valueOr(data.skus.find(item => item.family === "星载电子"), data.skus[0]);
  if (defaultSku) {
    previewControls.sku.value = defaultSku.sku;
    previewControls.orderCycleOverride.value = Math.max(1, Number(defaultSku.orderCycleDays || 1));
  }
  if (data.resources[0]) {
    previewControls.capacityResource.value = data.resources[0].code;
  }
  syncSupplierLimitDefaults();
  renderScenarioScopeSummary();
}

function syncSkuPolicyDefaults() {
  const sku = state.data?.skus?.find(item => item.sku === previewControls.sku.value);
  if (!sku) return;
  previewControls.orderCycleOverride.value = Math.max(1, Number(sku.orderCycleDays || 1));
}

function syncSupplierLimitDefaults() {
  const value = previewControls.supplierLimit.value;
  if (!state.data || !value) {
    previewControls.supplierLimitStartWeek.value = 1;
    previewControls.supplierLimitEndWeek.value = valueOr(state.data?.request?.horizonWeeks, 12);
    previewControls.supplierCapacityLimit.value = 0;
    return;
  }

  const [supplier, materialFamily] = value.split("|");
  const windows = state.data.supplierCapacityWindows
    .filter(item => item.supplier === supplier && item.materialFamily === materialFamily)
    .sort((left, right) => left.week - right.week);
  if (!windows.length) return;

  previewControls.supplierLimitStartWeek.value = windows[0].week;
  previewControls.supplierLimitEndWeek.value = windows[windows.length - 1].week;
  previewControls.supplierCapacityLimit.value = Math.min(...windows.map(item => Number(item.committedCapacity)));
}

function applyFilters() {
  const data = state.data;
  const familyValue = selectors.family.value;
  const skuValue = selectors.sku.value;
  const resourceValue = selectors.resource.value;
  const riskValue = selectors.risk.value;

  const skus = data.skus.filter(sku =>
    (!familyValue || sku.family === familyValue) &&
    (!skuValue || sku.sku === skuValue));
  const skuSet = new Set(skus.map(sku => sku.sku));
  const familySet = new Set(skus.map(sku => sku.family));
  const routingSet = new Set(data.resourceRoutings
    .filter(route => skuSet.has(route.sku) && (!resourceValue || route.resourceCode === resourceValue))
    .map(route => route.resourceCode));
  const sourceKeys = data.supplierCapacityWindows
    .filter(window => !riskValue || window.riskStatus === riskValue)
    .map(window => `${window.supplier}|${window.materialFamily}`);
  const sourceSet = new Set(sourceKeys);

  state.filtered = {
    ...data,
    families: data.families.filter(family => familySet.has(family.code)),
    skus,
    inventory: data.inventory.filter(item => skuSet.has(item.sku)),
    demand: data.demand.filter(item => skuSet.has(item.sku)),
    resources: data.resources.filter(item => routingSet.has(item.code)),
    resourceRoutings: data.resourceRoutings.filter(item => skuSet.has(item.sku) && routingSet.has(item.resourceCode)),
    supplierItemSources: data.supplierItemSources.filter(item => skuSet.has(item.sku) && sourceSet.has(`${item.supplier}|${item.materialFamily}`)),
    historicalDemand: data.historicalDemand.filter(item => skuSet.has(item.sku)),
    budgetBenchmarks: data.budgetBenchmarks.filter(item => familySet.has(item.family)),
    resourceCalendar: data.resourceCalendar.filter(item => routingSet.has(item.resourceCode)),
    supplierCapacityWindows: data.supplierCapacityWindows.filter(item => sourceSet.has(`${item.supplier}|${item.materialFamily}`)),
  };

  renderWorkspace();
}

function renderKpis(data) {
  const trend = filterBufferTrendWorkspace(state.bufferTrend);
  const rccp = state.rccp;
  const supplier = state.supplierCollaboration;
  const service = data.historicalDemand.length
    ? data.historicalDemand.reduce((sum, item) => sum + Number(item.serviceLevelPercent), 0) / data.historicalDemand.length
    : 0;
  const redSkuCount = valueOr(trend?.kpis?.redSkuCount, 0);
  const averageInventoryValue = trend?.kpis?.averageInventoryValue;
  const peakLoad = valueOr(rccp?.maxPeakLoadPercent, 0);
  const averageLoad = valueOr(rccp?.averageLoadPercent, 0);
  const supplyGap = valueOr(supplier?.totalSupplyGap, 0);

  byId("workspace-kpis").innerHTML = [
    ["服务水平", percent(service), "历史实际平均"],
    ["目标流速", percent(targetFlowIndex()), "当前产品族目标"],
    ["平均库存金额", metricOrEvidenceMissing(averageInventoryValue, money), "来自缓冲趋势服务"],
    ["补货释放峰值", percent(peakLoad), "预计补货订单的释放周压力"],
    ["平均负荷", percent(averageLoad), "来自 RCCP 服务"],
    ["红区 SKU", number(redSkuCount), "来自缓冲趋势服务"],
    ["供应缺口", number(supplyGap), "来自供应商钻取服务"],
  ].map(([label, value, note]) => `<div><span>${label}</span><strong>${value}</strong><small>${note}</small></div>`).join("");
}

function filterProductFamilyDashboard(dashboard) {
  if (!dashboard) return null;
  const summaries = dashboard.summaries;
  const familySet = new Set(summaries.map(item => item.family));
  const weeklyCells = dashboard.weeklyCells.filter(item => familySet.has(item.family));
  const details = dashboard.details.filter(item => familySet.has(item.family));
  const selectedFamily = familySet.has(state.selectedProductFamily)
    ? state.selectedProductFamily
    : (familySet.has(dashboard.selectedFamily) ? dashboard.selectedFamily : valueOr(summaries[0]?.family, ""));

  if (selectedFamily) {
    state.selectedProductFamily = selectedFamily;
  }

  return { ...dashboard, summaries, weeklyCells, details, selectedFamily };
}

function renderProductFamilyDashboard(dashboard) {
  const filteredDashboard = filterProductFamilyDashboard(dashboard);
  if (!filteredDashboard) {
    byId("product-family-kpis").innerHTML = "";
    byId("product-family-card-grid").innerHTML = `<div class="table-empty"><strong>没有产品族看板数据</strong></div>`;
    byId("product-family-weekly-grid").innerHTML = `<div class="table-empty"><strong>没有产品族周度风险数据</strong></div>`;
    byId("product-family-detail-summary").innerHTML = "";
    byId("product-family-risk-body").innerHTML = emptyRow("没有产品族风险数据", 5);
    byId("product-family-action-list").innerHTML = `<div class="table-empty"><strong>没有建议动作</strong></div>`;
    byId("product-family-rccp-body").innerHTML = emptyRow("没有 RCCP 贡献数据", 6);
    byId("product-family-supply-body").innerHTML = emptyRow("没有供应需求数据", 6);
    return;
  }

  byId("product-family-case-chip").textContent = caseLabel(filteredDashboard.name);
  const summaries = filteredDashboard.summaries;
  const redFamilies = summaries.filter(item => item.status === "Red").length;
  const yellowFamilies = summaries.filter(item => item.status === "Yellow").length;
  const totalSupplyGap = summaries.reduce((sum, item) => sum + Number(item.supplyGap), 0);
  const totalCapacityGap = summaries.reduce((sum, item) => sum + Number(item.capacityGap), 0);
  const inventoryEvidenceComplete = summaries.length > 0
    && summaries.every(item => isFiniteChartValue(item.averageInventoryValue));
  const averageInventory = inventoryEvidenceComplete
    ? summaries.reduce((sum, item) => sum + Number(item.averageInventoryValue), 0) / summaries.length
    : null;
  const comparison = valueOr(filteredDashboard.comparison, {});
  byId("product-family-kpis").innerHTML = [
    ["红色产品族", number(redFamilies), "存在红区 SKU、供应缺口或产能超载"],
    ["黄色产品族", number(yellowFamilies), "存在黄区 SKU、预算偏差或负荷预警"],
    ["平均库存金额", metricOrEvidenceMissing(averageInventory, money), `变化 ${metricOrEvidenceMissing(comparison.averageInventoryValueDelta, money)}`],
    ["供应缺口", number(totalSupplyGap), `变化 ${number(valueOr(comparison.supplyGapDelta, 0))}`],
    ["产能缺口", number(totalCapacityGap), `变化 ${number(valueOr(comparison.capacityGapDelta, 0))}`],
    ["红色周变化", number(valueOr(comparison.redWeekDelta, 0)), "预览方案 - 基准方案"],
  ].map(([label, value, note]) => `<div><span>${label}</span><strong>${value}</strong><small>${note}</small></div>`).join("");

  renderProductFamilyCards(filteredDashboard);
  renderProductFamilyWeeklyGrid(filteredDashboard);
  renderSelectedProductFamily(filteredDashboard);
}

function renderProductFamilyCards(dashboard) {
  byId("product-family-card-grid").innerHTML = dashboard.summaries.length
    ? `
      <div class="product-family-card-toolbar">
        <span>点击卡片只切换右侧详情，不会过滤掉其它产品族。</span>
        <button class="button secondary compact-button" type="button" data-product-family-reset>显示全部产品族</button>
      </div>
      ${dashboard.summaries.map(item => `
      <button class="product-family-card ${item.family === dashboard.selectedFamily ? "is-selected" : ""} ${statusClass(item.status).replace("status-chip ", "")}" type="button" data-product-family="${escapeHtml(item.family)}">
        <span class="panel-kicker">${escapeHtml(statusLabel(item.status))}</span>
        <strong>${escapeHtml(item.name || item.family)}</strong>
        <small>${escapeHtml(item.family)} / ${number(item.skuCount)} 个 SKU</small>
        <span class="family-metric"><span>服务</span><b>${percent(item.serviceLevelPercent)}</b><i>目标 ${percent(item.targetServiceLevel)}</i></span>
        <span class="family-metric"><span>流速</span><b>${percent(item.flowIndex)}</b><i>目标 ${percent(item.targetFlowIndex)}</i></span>
        <span class="family-metric"><span>库存</span><b>${metricOrEvidenceMissing(item.averageInventoryValue, money)}</b><i>峰值 ${metricOrEvidenceMissing(item.peakInventoryValue, money)}</i></span>
        <span class="family-metric"><span>缺口</span><b>${number(Number(item.supplyGap) + Number(item.capacityGap))}</b><i>${escapeHtml(item.recommendedAction)}</i></span>
      </button>
    `).join("")}`
    : `<div class="table-empty"><strong>没有产品族总览数据</strong></div>`;
}

function renderProductFamilyWeeklyGrid(dashboard) {
  const weeks = [...new Set(dashboard.weeklyCells.map(item => item.week))].sort((left, right) => left - right);
  byId("product-family-weekly-grid").innerHTML = dashboard.summaries.length
    ? `
      <table class="buffer-heatmap-table product-family-weekly-table">
        <thead><tr><th>产品族</th>${weeks.map(week => `<th>第 ${week} 周</th>`).join("")}</tr></thead>
        <tbody>
          ${dashboard.summaries.map(summary => `
            <tr>
              <th><button class="link-button" type="button" data-product-family="${escapeHtml(summary.family)}"><strong>${escapeHtml(summary.name || summary.family)}</strong><small>${escapeHtml(summary.family)}</small></button></th>
              ${weeks.map(week => {
                const cell = dashboard.weeklyCells.find(item => item.family === summary.family && item.week === week);
                return cell
                  ? `<td><button class="${bufferCellClass(cell.status)}" type="button" data-product-family="${escapeHtml(cell.family)}" data-product-family-week="${cell.week}"><strong>${statusLabel(cell.status)}</strong><span>库存 ${metricOrEvidenceMissing(cell.inventoryValue, money)}</span><small>供 ${number(cell.supplyGap)} / 产 ${number(cell.capacityGap)}</small></button></td>`
                  : `<td class="empty-cell">-</td>`;
              }).join("")}
            </tr>
          `).join("")}
        </tbody>
      </table>`
    : `<div class="table-empty"><strong>没有产品族周度风险数据</strong></div>`;
}

function renderSelectedProductFamily(dashboard) {
  const detail = dashboard.details.find(item => item.family === dashboard.selectedFamily) || dashboard.details[0];
  const summary = dashboard.summaries.find(item => item.family === detail?.family);
  if (!detail || !summary) {
    byId("product-family-selected-title").textContent = "选中产品族详情";
    byId("product-family-detail-summary").innerHTML = "";
    byId("product-family-risk-body").innerHTML = emptyRow("没有产品族风险数据", 5);
    byId("product-family-action-list").innerHTML = `<div class="table-empty"><strong>没有建议动作</strong></div>`;
    byId("product-family-rccp-body").innerHTML = emptyRow("没有 RCCP 贡献数据", 6);
    byId("product-family-supply-body").innerHTML = emptyRow("没有供应需求数据", 6);
    return;
  }

  byId("product-family-selected-title").textContent = `${detail.name || detail.family} 详情`;
  byId("product-family-detail-summary").innerHTML = [
    ["产品族", detail.family],
    ["状态", `<span class="${statusClass(summary.status)}">${statusLabel(summary.status)}</span>`],
    ["服务 / 目标", `${percent(summary.serviceLevelPercent)} / ${percent(summary.targetServiceLevel)}`],
    ["流速 / 目标", `${percent(summary.flowIndex)} / ${percent(summary.targetFlowIndex)}`],
    ["平均库存", metricOrEvidenceMissing(summary.averageInventoryValue, money)],
    ["预算偏差", metricOrEvidenceMissing(summary.budgetInventoryVariance, money)],
    ["供应缺口", number(summary.supplyGap)],
    ["产能缺口", number(summary.capacityGap)],
  ].map(([label, value]) => `<div><span>${label}</span><strong>${value}</strong></div>`).join("");

  byId("product-family-risk-body").innerHTML = detail.riskItems.length
    ? detail.riskItems.map(item => {
      const link = productFamilyRiskLink(item);
      return `
        <tr class="interactive-row ${productFamilyLinkClass(link)}" tabindex="0" title="点击联动定位相关 RCCP 与供应需求" ${productFamilyLinkAttributes(link)}>
          <td>${escapeHtml(item.scope)}</td>
          <td>${escapeHtml(item.target)}</td>
          <td>第 ${item.week} 周</td>
          <td>${escapeHtml(item.reason)}</td>
          <td><span class="${statusClass(item.severity)}">${statusLabel(item.severity)}</span></td>
        </tr>`;
    }).join("")
    : emptyRow("没有产品族风险数据", 5);

  byId("product-family-action-list").innerHTML = detail.recommendations.length
    ? detail.recommendations.map(item => `
      <div class="diagnostic-item ${item.severity === "Red" ? "is-error" : ""}">
        <strong>${escapeHtml(item.actionType)}</strong>
        <span>${escapeHtml(businessEvidenceLabel(item.message))}</span>
      </div>
    `).join("")
    : `<div class="table-empty"><strong>没有建议动作</strong></div>`;

  byId("product-family-rccp-body").innerHTML = detail.rccpContributions.length
    ? detail.rccpContributions.map(item => {
      const link = { sku: item.sku, week: String(item.week), resource: item.resourceCode };
      return `
        <tr class="interactive-row ${productFamilyLinkClass(link)}" tabindex="0" title="点击联动定位相关风险和供应需求" ${productFamilyLinkAttributes(link)}>
          <td><strong>${escapeHtml(item.sku)}</strong><br><small>${escapeHtml(item.skuName)}</small></td>
          <td>第 ${item.week} 周</td>
          <td>${number(item.orderQuantity)}</td>
          <td>${escapeHtml(item.resourceCode)}</td>
          <td>${number(item.requiredCapacity)}</td>
          <td>${triggerLabel(item.trigger)}</td>
        </tr>`;
    }).join("")
    : emptyRow("没有 RCCP 贡献数据", 6);

  byId("product-family-supply-body").innerHTML = detail.supplierRequirements.length
    ? detail.supplierRequirements.map(item => {
      const link = { sku: item.sku, week: String(item.week), supplier: item.supplier, material: item.materialFamily };
      return `
        <tr class="interactive-row ${productFamilyLinkClass(link)}" tabindex="0" title="点击联动定位相关风险和 RCCP 贡献" ${productFamilyLinkAttributes(link)}>
          <td>${escapeHtml(item.supplier)}</td>
          <td>${escapeHtml(item.materialFamily)}</td>
          <td><strong>${escapeHtml(item.sku)}</strong><br><small>${escapeHtml(item.skuName)}</small></td>
          <td>第 ${item.week} 周</td>
          <td>${number(item.orderQuantity)}</td>
          <td>${money(item.projectedValue)}</td>
        </tr>`;
    }).join("")
    : emptyRow("没有供应需求数据", 6);
}

function productFamilyRiskLink(item) {
  const link = { week: String(item.week) };
  if (item.scope === "缓冲") {
    link.sku = item.target;
  }
  if (item.scope === "供应") {
    const [supplier, material] = item.target.split(" / ");
    link.supplier = supplier;
    link.material = material;
  }
  return link;
}

function productFamilyLinkAttributes(link) {
  return [
    ["data-family-link-week", link.week],
    ["data-family-link-sku", link.sku],
    ["data-family-link-supplier", link.supplier],
    ["data-family-link-material", link.material],
    ["data-family-link-resource", link.resource],
  ]
    .filter(([, value]) => value)
    .map(([name, value]) => `${name}="${escapeHtml(value)}"`)
    .join(" ");
}

function productFamilyLinkClass(link) {
  return productFamilyLinkMatches(link, state.selectedProductFamilyLink) ? "is-linked" : "";
}

function productFamilyLinkMatches(candidate, selected) {
  if (!candidate || !selected) return false;
  const sameWeek = !selected.week || !candidate.week || selected.week === candidate.week;
  if (!sameWeek) return false;
  const candidateSpecific = candidate.sku || candidate.supplier || candidate.resource;
  const selectedSpecific = selected.sku || selected.supplier || selected.resource;
  if (!candidateSpecific || !selectedSpecific) return true;
  if (selected.sku && candidate.sku && selected.sku === candidate.sku) return true;
  if (selected.supplier && candidate.supplier && selected.supplier === candidate.supplier) {
    return !selected.material || !candidate.material || selected.material === candidate.material;
  }
  if (selected.resource && candidate.resource && selected.resource === candidate.resource) return true;
  return false;
}

function productFamilyLinkFromElement(element) {
  return {
    week: element.dataset.familyLinkWeek,
    sku: element.dataset.familyLinkSku,
    supplier: element.dataset.familyLinkSupplier,
    material: element.dataset.familyLinkMaterial,
    resource: element.dataset.familyLinkResource,
  };
}

function renderReadiness(data) {
  byId("data-status-chip").className = "status-chip is-valid";
  byId("data-status-chip").textContent = "可用";
  const completeParameters = valueOr(data.ddmrpParameters, []).filter(item => item.completenessStatus === "Complete").length;
  const totalParameters = valueOr(data.ddmrpParameters, []).length;
  byId("data-readiness-list").innerHTML = [
    ["产品族", data.families.length],
    ["SKU", data.skus.length],
    ["资源", data.resources.length],
    ["DDMRP 参数完整", `${completeParameters}/${totalParameters}`],
    ["目标流速", percent(targetFlowIndex())],
    ["供应商来源", data.supplierItemSources.length],
    ["历史需求", data.historicalDemand.length],
    ["场景模板", data.scenarioTemplates.length],
  ].map(([label, value]) => `<div><dt>${label}</dt><dd>${typeof value === "number" ? number(value) : escapeHtml(value)}</dd></div>`).join("");

  byId("guardrail-table-body").innerHTML = data.guardrails.length
    ? data.guardrails.map((item, index) => `
      <tr class="interactive-row" data-guardrail-index="${index}" tabindex="0" title="点击查看业务栅栏详情">
        <td><strong>${escapeHtml(item.metric)}</strong><br><small>${escapeHtml(item.decisionRule)}</small></td>
        <td>黄线 ${number(item.yellowLimit)} ${escapeHtml(businessUnitLabel(item.unit))}</td>
        <td>红线 ${number(item.redLimit)} ${escapeHtml(businessUnitLabel(item.unit))}</td>
      </tr>
    `).join("")
    : emptyRow("没有业务栅栏数据", 3);

  renderDdmrpParameterCompleteness(data.ddmrpParameters || []);
}

function renderDdmrpParameterCompleteness(parameters) {
  const filteredParameters = state.ddmrpMissingOnly
    ? parameters.filter(item => item.completenessStatus !== "Complete")
    : parameters;
  const displayedParameters = state.ddmrpShowAll
    ? filteredParameters
    : filteredParameters.slice(0, ddmrpCompactLimit);
  const completeCount = parameters.filter(item => item.completenessStatus === "Complete").length;
  const chip = byId("ddmrp-completeness-chip");
  chip.className = completeCount === parameters.length && parameters.length > 0
    ? "status-chip is-valid"
    : "status-chip is-warning";
  chip.textContent = parameters.length ? `${completeCount}/${parameters.length} 完整` : "无参数";
  const toggleAll = byId("ddmrp-toggle-all");
  const missingOnly = byId("ddmrp-missing-only");
  if (toggleAll) {
    toggleAll.textContent = state.ddmrpShowAll ? "收起" : "查看全部";
    toggleAll.setAttribute("aria-expanded", String(state.ddmrpShowAll));
  }
  if (missingOnly) {
    missingOnly.classList.toggle("is-active", state.ddmrpMissingOnly);
    missingOnly.setAttribute("aria-pressed", String(state.ddmrpMissingOnly));
  }

  byId("ddmrp-parameter-body").innerHTML = displayedParameters.length
    ? displayedParameters.map(item => `
      <tr class="interactive-row" data-ddmrp-sku="${escapeHtml(item.sku)}" tabindex="0" title="点击查看参数详情">
        <td><strong>${escapeHtml(item.sku)}</strong><br><small>${escapeHtml(item.name)}</small></td>
        <td>${escapeHtml(item.decouplingPoint)}</td>
        <td>${escapeHtml(item.bufferProfile)}</td>
        <td>${number(item.adu)} / DAF ${number(item.demandAdjustmentFactor)}</td>
        <td>${number(item.decoupledLeadTimeDays)} 天 / ${number(item.variabilityFactor)}</td>
        <td>${number(item.minimumOrderQuantity)} / ${number(item.orderCycleDays)} 天</td>
        <td>${number(item.zoneAdjustmentFactor)}</td>
        <td>${number(item.topOfRed)} / ${number(item.topOfYellow)} / ${number(item.topOfGreen)}</td>
        <td><span class="${statusClass(item.completenessStatus === "Complete" ? "Green" : "Yellow")}" title="${escapeHtml(businessEvidenceLabel(item.validationMessage))}">${item.completenessStatus === "Complete" ? "完整" : "缺失"}</span><br><small>${masterSettingStatusLabel(item.parameterStatus)}</small></td>
      </tr>
    `).join("")
    : emptyRow("没有 DDMRP 参数档案", 9);
}

function renderDdmrpParameterDetail(skuCode) {
  const item = state.data?.ddmrpParameters?.find(parameter => parameter.sku === skuCode);
  if (!item) return;
  const sizingLines = valueOr(item.sizingLines, []);
  openWorkspaceDrawer("参数详情", [
    {
      title: `${item.sku} ${item.name}`,
      items: [
        ["产品族", escapeHtml(item.family)],
        ["解耦点", escapeHtml(item.decouplingPoint)],
        ["缓冲档案", escapeHtml(item.bufferProfile)],
        ["参数状态", escapeHtml(masterSettingStatusLabel(item.parameterStatus))],
        ["完整性", escapeHtml(item.completenessStatus === "Complete" ? "完整" : "缺失")],
        ["验证信息", escapeHtml(businessEvidenceLabel(item.validationMessage))],
      ],
    },
    {
      title: "基础参数",
      items: [
        ["ADU", number(item.adu)],
        ["ADU 来源", `${escapeHtml(businessEvidenceLabel(item.aduSource))} / ${number(item.aduCalculationWindowDays)} 天窗口`],
        ["DLT", `${number(item.decoupledLeadTimeDays)} 天`],
        ["DLT 来源", escapeHtml(item.dltSource)],
        ["提前期因子", item.leadTimeFactor == null ? "证据缺失" : number(item.leadTimeFactor)],
        ["变异因子", number(item.variabilityFactor)],
        ["DAF", number(item.demandAdjustmentFactor)],
        ["区域调整因子", number(item.zoneAdjustmentFactor)],
        ["MOQ", number(item.minimumOrderQuantity)],
        ["订货周期", `${number(item.orderCycleDays)} 天`],
        ["单位成本", money(item.unitCost)],
        ["周能力参考", number(item.weeklyCapacityUnits)],
        ["生效窗口", `第 ${number(item.effectiveFromWeek)}-${number(item.effectiveThroughWeek)} 周`],
        ["参数快照", escapeHtml(item.parameterSnapshotId || "证据缺失")],
        ["证据状态", escapeHtml(evidenceStatusLabel(item.evidenceStatus))],
      ],
    },
    {
      title: "缓冲定容结果",
      items: [
        ["红区上沿", number(item.topOfRed)],
        ["黄区上沿", number(item.topOfYellow)],
        ["绿区上沿", number(item.topOfGreen)],
      ],
    },
    {
      title: "后端定容明细",
      items: sizingLines.length
        ? sizingLines.map(line => [
            line.component,
            `${escapeHtml(line.formula)} · ${number(line.value)} · ${escapeHtml(businessEvidenceLabel(line.explanation))}`,
          ])
        : [["定容明细", "旧版本缺少提前期因子，不能生成定容明细"]],
    },
  ]);
}

function guardrailTriggerStatus(item) {
  if (!state.preview) {
    return "尚未运行预览";
  }
  const adoption = evaluateAdoption(state.preview);
  const names = adoption.violations?.map(rule => `${rule.name} ${rule.reason}`) || [];
  const metric = item.metric || "";
  const matched = names.some(name => {
    if (metric.includes("服务")) return name.includes("服务");
    if (metric.includes("营运资金")) return name.includes("库存") || name.includes("预算");
    if (metric.includes("资源")) return name.includes("产能") || name.includes("负荷");
    if (metric.includes("供应")) return name.includes("供应");
    if (metric.includes("红区")) return name.includes("红区") || name.includes("服务");
    return name.includes(metric);
  });
  return matched ? `${adoption.label}：已触发` : `${adoption.label}：未命中当前采纳建议`;
}

function renderGuardrailDetail(index) {
  const item = state.data?.guardrails?.[index];
  if (!item) return;
  openWorkspaceDrawer("业务栅栏详情", [
    {
      title: item.metric,
      items: [
        ["黄线", `${number(item.yellowLimit)} ${escapeHtml(businessUnitLabel(item.unit))}`],
        ["红线", `${number(item.redLimit)} ${escapeHtml(businessUnitLabel(item.unit))}`],
        ["决策规则", escapeHtml(item.decisionRule)],
        ["当前方案", escapeHtml(guardrailTriggerStatus(item))],
      ],
    },
    {
      title: "使用说明",
      items: [
        ["作用", "用于在场景预览后判断方案可采纳、需协调或阻断采纳。"],
        ["位置", "方案比较区会展示违反的具体规则；本抽屉用于解释规则口径。"],
        ["边界", "本详情不重新计算业务结果，只读取后端结果和现有采纳建议。"],
      ],
    },
  ]);
}

function renderScenarioTemplates(data) {
  byId("scenario-template-list").innerHTML = data.scenarioTemplates.length
    ? data.scenarioTemplates.map(template => `
      <article class="case-card ${previewControls.template?.value === template.templateId ? "is-selected" : ""}" data-template-card="${template.templateId}">
        <div class="panel-heading">
          <div><span class="panel-kicker">${template.templateId}</span><h2>${template.name}</h2></div>
          <button class="button secondary template-select-action" type="button" data-template-id="${template.templateId}">选择</button>
        </div>
        <p>${template.purpose}</p>
        <div class="case-card-meta">
          ${template.actions.map(action => `
            <div>
              <span>${actionTypeLabel(action.actionType)} / 第 ${action.startWeek}-${action.endWeek} 周</span>
              <strong>${action.target}</strong>
              <small>${number(action.value)} ${escapeHtml(businessUnitLabel(action.unit))}</small>
            </div>
          `).join("")}
        </div>
      </article>
    `).join("")
    : `<article class="case-card"><p>没有可用场景模板。</p></article>`;
}

function renderScenarioComparison(data) {
  const template = data.scenarioTemplates[0];
  const baselinePeak = valueOr(state.rccp?.maxPeakLoadPercent, 0);
  const supplyRisk = state.supplierCollaboration?.weeklyCells?.filter(item => item.status === "Red").length || 0;
  byId("scenario-comparison-result").innerHTML = [
    ["基准方案", "当前主数据基准", [
      ["覆盖 SKU", data.skus.length],
      ["补货释放峰值", percent(baselinePeak)],
      ["供应风险周", supplyRisk],
      ["业务栅栏", data.guardrails.length],
    ], false],
    ["预览方案", valueOr(template?.name, "候选方案"), [
      ["模板动作", valueOr(template?.actions.length, 0)],
      ["目标对象", unique(valueOr(template?.actions.map(item => item.target), [])).length],
      ["状态", "待运行"],
      ["说明", "只读预览"],
    ], true],
  ].map(([title, subtitle, metrics, recommended]) => `
    <div class="comparison-column ${recommended ? "is-recommended" : ""}">
      <h3>${title}</h3>
      <p>${subtitle}</p>
      <div class="comparison-metrics">
        ${metrics.map(([label, value]) => `<div><span>${label}</span><strong>${value}</strong></div>`).join("")}
      </div>
    </div>
  `).join("");
  renderMultiScenarioComparison(null);
}

function renderPreviewKpis(result) {
  const metrics = result.scenario.metrics;
  byId("workspace-kpis").innerHTML = [
    ["服务水平", percent(metrics.serviceLevelPercent), `Δ ${percent(result.comparison.serviceLevelDelta)}`],
    ["流速指数", percent(metrics.flowIndex), `目标 ${percent(targetFlowIndex())} / Δ ${percent(result.comparison.flowIndexDelta)}`],
    ["平均库存金额", metricOrEvidenceMissing(metrics.averageInventoryValue, money), `Δ ${metricOrEvidenceMissing(result.comparison.averageInventoryValueDelta, money)}`],
    ["补货释放峰值", percent(metrics.peakLoadPercent), `Δ ${percent(result.comparison.peakLoadPercentDelta)}`],
    ["平均负荷", percent(metrics.averageLoadPercent), `Δ ${percent(result.comparison.averageLoadPercentDelta)}`],
    ["红区 SKU", number(metrics.redSkuCount), `Δ ${number(result.comparison.redSkuCountDelta)}`],
    ["供应缺口", number(metrics.supplyGap), `Δ ${number(result.comparison.supplyGapDelta)}`],
  ].map(([label, value, note]) => `<div><span>${label}</span><strong>${value}</strong><small>${note}</small></div>`).join("");
}

function renderPreviewComparison(result) {
  const adoption = evaluateAdoption(result);
  const checks = Array.isArray(result.feasibility?.checks) ? result.feasibility.checks : [];
  byId("scenario-comparison-result").innerHTML = `
    <div class="comparison-column adoption-decision">
      <h3>采纳建议</h3>
      <p>${adoptionConstraintLabel(adoption.constraintMode)}：${adoption.message}</p>
      <div class="comparison-metrics">
        <div><span>采纳状态</span><strong><span class="${statusClass(adoption.status)}">${adoption.label}</span></strong></div>
        <div><span>目标流速</span><strong>${percent(targetFlowIndex())}</strong></div>
      </div>
      <div class="adoption-rule-list">
        <strong>${adoption.status === "Red" ? "违反规则" : adoption.status === "Yellow" ? "需协调规则" : "规则检查"}</strong>
        ${checks.length
          ? checks.map(item => `
            <div class="adoption-rule-item ${item.status === "Red" ? "is-red" : item.status === "Yellow" ? "is-yellow" : "is-green"}">
              <span>${escapeHtml(item.metric || item.code || "后端可行性检查")}：${escapeHtml(statusLabel(item.status))}</span>
              <p>当前值：${item.actual === null || item.actual === undefined ? "-" : `${number(item.actual)} ${escapeHtml(item.unit || "")}`.trim()}；阈值：${item.redLimit === null || item.redLimit === undefined ? "后端评估" : `${number(item.redLimit)} ${escapeHtml(item.unit || "")}`.trim()}</p>
              <p>原因：${escapeHtml(item.message || "后端可行性评估")}</p>
            </div>
          `).join("")
          : `<div class="adoption-rule-item is-red"><span>后端可行性证据缺失</span><p>请重新运行预览。</p></div>`}
      </div>
    </div>
    ${[
    [caseLabel(result.baseline.name), result.baseline.metrics, false],
    [caseLabel(result.scenario.name), result.scenario.metrics, true],
  ].map(([title, metrics, recommended]) => `
    <div class="comparison-column ${recommended ? "is-recommended" : ""}">
      <h3>${title}</h3>
      <p>${recommended ? "预览结果，未保存" : "原始基准"}</p>
      <div class="comparison-metrics">
        <div><span>服务水平</span><strong>${percent(metrics.serviceLevelPercent)}</strong></div>
        <div><span>流速指数</span><strong>${percent(metrics.flowIndex)}</strong></div>
        <div><span>平均库存</span><strong>${metricOrEvidenceMissing(metrics.averageInventoryValue, money)}</strong></div>
        <div><span>补货释放峰值</span><strong>${percent(metrics.peakLoadPercent)}</strong></div>
        <div><span>供应缺口</span><strong>${number(metrics.supplyGap)}</strong></div>
        <div><span>补货订单</span><strong>${number(metrics.replenishmentOrderCount)}</strong></div>
        <div><span>补货价值</span><strong>${money(metrics.replenishmentValue)}</strong></div>
      </div>
    </div>
  `).join("")}`;
}

function renderMultiScenarioComparison(result) {
  const comparisonBody = byId("multi-scenario-comparison-body");
  const matrixBody = byId("candidate-impact-matrix-body");
  if (!comparisonBody || !matrixBody) {
    return;
  }

  const comparisons = valueOr(result?.combinationComparisons, []);
  comparisonBody.innerHTML = comparisons.length
    ? comparisons.map(item => row([
      `<strong>${escapeHtml(item.profileName)}</strong><br><small>${escapeHtml(item.profileId)}</small>`,
      `${number(item.serviceLevelDelta)}pp`,
      `${number(item.flowIndexDelta)}pp`,
      metricOrEvidenceMissing(item.averageInventoryValueDelta, money),
      `${number(item.peakLoadPercentDelta)}pp`,
      number(item.redSkuCountDelta),
      number(item.supplyGapDelta),
      number(item.replenishmentOrderCountDelta),
      money(item.estimatedActionCost),
      `<span class="${item.managementDecision === "需要管理取舍" ? "status-chip is-invalid" : item.managementDecision.includes("复核") || item.managementDecision.includes("评审") ? "status-chip is-warning" : "status-chip is-valid"}">${escapeHtml(item.managementDecision)}</span>`,
    ])).join("")
    : emptyRow("选择候选组合后显示多方案 KPI、库存、服务和订单变化。", 10);

  const matrix = valueOr(result?.candidateImpactMatrix, []);
  matrixBody.innerHTML = matrix.length
    ? matrix.map(item => row([
      `<strong>${escapeHtml(item.actionType)}</strong><br><small>${escapeHtml(item.candidateId)}</small>`,
      escapeHtml(item.target),
      `${number(item.serviceImpactPercent)}pp`,
      metricOrEvidenceMissing(item.inventoryImpactValue, money),
      `${number(item.peakLoadImpactPercent)}pp`,
      number(item.supplyGapImpact),
      number(item.replenishmentOrderImpact),
      `<strong>${money(item.estimatedCost)}</strong><br><small>${escapeHtml(item.costBasis)}</small>`,
      escapeHtml(item.constraintNote),
      `<span class="${item.feasibilityStatus === "需要管理取舍" ? "status-chip is-invalid" : item.feasibilityStatus.includes("复核") ? "status-chip is-warning" : "status-chip is-valid"}">${escapeHtml(item.feasibilityStatus)}</span>`,
    ])).join("")
    : emptyRow("候选动作影响矩阵将在候选组合选择后显示。", 10);
}

function futureInventoryCases(fallbackTrend = null) {
  const previewCases = [state.preview?.baseline, state.preview?.scenario]
    .filter(item => item?.bufferTrend);
  const fallbackBelongsToPreview = fallbackTrend && previewCases.some(item => item.bufferTrend === fallbackTrend);
  if (previewCases.length && (!fallbackTrend || fallbackBelongsToPreview)) return previewCases;
  if (!fallbackTrend) return [];
  return [{
    caseId: valueOr(fallbackTrend.caseId, "baseline"),
    name: valueOr(fallbackTrend.name, "基准方案"),
    bufferTrend: fallbackTrend,
    inventoryFlow: null,
    scenarioMetricEvidence: [],
  }];
}

function normalizeFutureInventorySelection(trend, scopedTrend = null) {
  const cases = futureInventoryCases(trend);
  const requestedCaseId = state.futureInventorySelection.caseId;
  let previewCase = cases.find(item => item.caseId === requestedCaseId);
  if (!previewCase) previewCase = cases.find(item => item.caseId === trend?.caseId);
  if (!previewCase) previewCase = cases.find(item => item.caseId === "scenario");
  if (!previewCase) previewCase = valueOr(cases[0], null);
  const selectedTrend = valueOr(previewCase?.bufferTrend, trend);
  const selectionTrend = valueOr(scopedTrend, selectedTrend);
  const skuDetails = valueOr(selectionTrend?.skuDetails, []);
  const requestedSku = valueOr(state.futureInventorySelection.sku, state.selectedBufferSku);
  const sku = skuDetails.some(item => item.sku === requestedSku)
    ? requestedSku
    : (skuDetails.some(item => item.sku === selectionTrend?.selectedSku)
      ? selectionTrend.selectedSku
      : valueOr(skuDetails[0]?.sku, ""));
  const selectedDetail = skuDetails.find(item => item.sku === sku);
  const weeks = [...new Set(valueOr(selectionTrend?.series, valueOr(selectedDetail?.series, []))
    .map(item => Number(item.week))
    .filter(Number.isFinite))].sort((left, right) => left - right);
  const minimumWeek = valueOr(weeks[0], 1);
  const maximumWeek = valueOr(weeks[weeks.length - 1], minimumWeek);
  let weekFrom = Number(state.futureInventorySelection.weekFrom);
  let weekThrough = Number(state.futureInventorySelection.weekThrough);
  if (!Number.isFinite(weekFrom) || weekFrom < minimumWeek || weekFrom > maximumWeek) weekFrom = minimumWeek;
  if (!Number.isFinite(weekThrough) || weekThrough < weekFrom || weekThrough > maximumWeek) weekThrough = maximumWeek;

  state.futureInventorySelection.caseId = valueOr(previewCase?.caseId, valueOr(selectedTrend?.caseId, "baseline"));
  state.futureInventorySelection.sku = sku;
  state.futureInventorySelection.weekFrom = weekFrom;
  state.futureInventorySelection.weekThrough = weekThrough;
  state.selectedBufferSku = sku;
  return {
    cases,
    previewCase,
    trend: selectedTrend,
    caseId: state.futureInventorySelection.caseId,
    sku,
    weekFrom,
    weekThrough,
    minimumWeek,
    maximumWeek,
  };
}

function scopeFutureBufferDetail(detail, selection, trend) {
  if (!detail) return null;
  const inRange = item => Number(item.week) >= selection.weekFrom && Number(item.week) <= selection.weekThrough;
  const series = valueOr(detail.series, []).filter(inRange);
  const itemByWeek = new Map(series.map(item => [Number(item.week), item]));
  const dateByWeek = new Map(valueOr(trend?.series, [])
    .filter(inRange)
    .map(item => [Number(item.week), item.periodStartDate]));
  const chartDomain = Array.from(
    { length: Math.max(0, selection.weekThrough - selection.weekFrom + 1) },
    (_, index) => {
      const week = selection.weekFrom + index;
      return {
        index,
        week,
        periodStartDate: valueOr(itemByWeek.get(week)?.periodStartDate, valueOr(dateByWeek.get(week), "")),
        item: valueOr(itemByWeek.get(week), null),
      };
    });
  return {
    ...detail,
    series,
    chartDomain,
    replenishmentOrders: valueOr(detail.replenishmentOrders, []).filter(inRange),
    traces: valueOr(detail.traces, []).filter(inRange),
    activities: valueOr(detail.activities, []).filter(inRange),
    orderDetails: valueOr(detail.orderDetails, []).filter(inRange),
  };
}

function futureInventoryChartX(index, domainLength) {
  const width = 940;
  const left = 62;
  const right = 26;
  return left + index * (width - left - right) / Math.max(1, domainLength - 1);
}

function futureInventoryDateLabel(domainPoint) {
  return domainPoint.periodStartDate || `第 ${domainPoint.week} 周`;
}

function scopedFutureBufferDetailForCharts(detail) {
  if (!detail || Array.isArray(detail.chartDomain)) return detail;
  const weeks = valueOr(detail.series, [])
    .map(item => Number(item.week))
    .filter(Number.isFinite);
  if (weeks.length === 0) return { ...detail, chartDomain: [] };
  return scopeFutureBufferDetail(detail, {
    weekFrom: Math.min(...weeks),
    weekThrough: Math.max(...weeks),
  }, state.bufferTrend);
}

function renderFutureInventorySelectionControls(selection) {
  const caseSelect = byId("buffer-case-select");
  const weekSelect = byId("buffer-week-range-select");
  caseSelect.innerHTML = selection.cases.map(item =>
    `<option value="${escapeHtml(item.caseId)}">${escapeHtml(caseLabel(item.name))}</option>`).join("");
  caseSelect.value = selection.caseId;

  const ranges = [{ from: selection.minimumWeek, through: selection.maximumWeek }];
  for (let from = selection.minimumWeek; from <= selection.maximumWeek; from += 4) {
    ranges.push({ from, through: Math.min(from + 3, selection.maximumWeek) });
  }
  const uniqueRanges = [...new Map(ranges.map(item => [`${item.from}-${item.through}`, item])).values()];
  weekSelect.innerHTML = uniqueRanges.map(item =>
    `<option value="${item.from}-${item.through}">第 ${item.from} 至 ${item.through} 周</option>`).join("");
  weekSelect.value = `${selection.weekFrom}-${selection.weekThrough}`;
  if (!uniqueRanges.some(item => item.from === selection.weekFrom && item.through === selection.weekThrough)) {
    weekSelect.innerHTML += `<option value="${selection.weekFrom}-${selection.weekThrough}">第 ${selection.weekFrom} 至 ${selection.weekThrough} 周</option>`;
    weekSelect.value = `${selection.weekFrom}-${selection.weekThrough}`;
  }
}

function renderSelectedFutureInventoryWorkspace() {
  const selectedCase = futureInventoryCases(state.bufferTrend)
    .find(item => item.caseId === state.futureInventorySelection.caseId);
  if (selectedCase?.bufferTrend) state.bufferTrend = selectedCase.bufferTrend;
  renderBufferTrendWorkspace(valueOr(selectedCase?.bufferTrend, state.bufferTrend));
}

function acceptCurrentBufferTrend(trend) {
  state.preview = null;
  state.bufferTrend = trend;
  state.baselineBufferTrend = trend;
  state.selectedBufferSku = valueOr(trend?.selectedSku, null);
  state.futureInventorySelection.caseId = valueOr(trend?.caseId, "baseline");
  state.futureInventorySelection.sku = state.selectedBufferSku;
  state.futureInventorySelection.weekFrom = 1;
  state.futureInventorySelection.weekThrough = null;
}

function filterBufferTrendWorkspace(trend) {
  if (!trend) return null;

  const allowedSkus = new Set(valueOr(valueOr(state.filtered?.skus, state.data?.skus), []).map(item => item.sku));
  const selectedWeekFrom = state.futureInventorySelection?.weekFrom;
  const selectedWeekThrough = state.futureInventorySelection?.weekThrough;
  const weekFrom = selectedWeekFrom === null || selectedWeekFrom === undefined ? null : Number(selectedWeekFrom);
  const weekThrough = selectedWeekThrough === null || selectedWeekThrough === undefined ? null : Number(selectedWeekThrough);
  const isSelectedWeek = item => {
    const week = Number(item.week);
    return (weekFrom === null || !Number.isFinite(weekFrom) || week >= weekFrom)
      && (weekThrough === null || !Number.isFinite(weekThrough) || week <= weekThrough);
  };
  const series = trend.series.filter(item =>
    (allowedSkus.size === 0 || allowedSkus.has(item.sku)) && isSelectedWeek(item));
  const weeklyCells = trend.weeklyCells.filter(item =>
    (allowedSkus.size === 0 || allowedSkus.has(item.sku)) && isSelectedWeek(item));
  const physicalInventoryEvidenceComplete = series.length > 0
    && series.every(item => item.physicalPosition?.evidenceStatus === "Complete"
      && isFiniteChartValue(item.inventoryValue));
  const physicalInventoryValues = physicalInventoryEvidenceComplete
    ? series.map(item => Number(item.inventoryValue))
    : [];
  const zoneBands = trend.zoneBands.filter(item => allowedSkus.size === 0 || allowedSkus.has(item.sku));
  const skuDetails = trend.skuDetails.filter(item => allowedSkus.size === 0 || allowedSkus.has(item.sku));
  const replenishmentOrderCount = skuDetails.reduce((sum, detail) => sum + detail.replenishmentOrders.filter(isSelectedWeek).length, 0);
  const familySummaries = [...new Set(weeklyCells.map(item => item.family))].map(family => {
    const cells = weeklyCells.filter(item => item.family === family);
    return {
      family,
      averageInventoryValue: physicalInventoryEvidenceComplete
        && cells.length > 0
        && cells.every(item => isFiniteChartValue(item.inventoryValue))
        ? cells.reduce((sum, item) => sum + Number(item.inventoryValue), 0) / cells.length
        : null,
      redWeekCount: cells.filter(item => item.status === "Red").length,
      yellowWeekCount: cells.filter(item => item.status === "Yellow").length,
      overGreenWeekCount: cells.filter(item => item.status === "OverTopOfGreen").length,
      replenishmentOrderCount: skuDetails
        .filter(item => item.family === family)
        .reduce((sum, item) => sum + item.replenishmentOrders.filter(isSelectedWeek).length, 0),
    };
  }).sort((left, right) => right.redWeekCount - left.redWeekCount || right.yellowWeekCount - left.yellowWeekCount || left.family.localeCompare(right.family, "zh-CN"));

  const selectedSku = skuDetails.some(item => item.sku === state.selectedBufferSku)
    ? state.selectedBufferSku
    : (skuDetails.some(item => item.sku === trend.selectedSku) ? trend.selectedSku : valueOr(skuDetails[0]?.sku, ""));

  if (selectedSku) {
    state.selectedBufferSku = selectedSku;
  }

  return {
    ...trend,
    series,
    weeklyCells,
    zoneBands,
    familySummaries,
    skuDetails,
    selectedSku,
    physicalInventoryEvidenceStatus: physicalInventoryEvidenceComplete ? "Complete" : "EvidenceMissing",
    kpis: {
      redSkuCount: new Set(series.filter(item => item.status === "Red").map(item => item.sku)).size,
      yellowSkuCount: new Set(series.filter(item => item.status === "Yellow").map(item => item.sku)).size,
      shortageCount: series.filter(item => Number(item.endNetFlowBeforeReplenishment) <= 0).length,
      onHandRedSkuCount: physicalInventoryEvidenceComplete
        ? new Set(series.filter(item => item.physicalPosition?.evidenceStatus === "Complete" && item.physicalPosition.onHandStatus === "Red").map(item => item.sku)).size
        : null,
      onHandYellowSkuCount: physicalInventoryEvidenceComplete
        ? new Set(series.filter(item => item.physicalPosition?.evidenceStatus === "Complete" && item.physicalPosition.onHandStatus === "Yellow").map(item => item.sku)).size
        : null,
      onHandStockoutWeekCount: physicalInventoryEvidenceComplete
        ? series.filter(item => item.physicalPosition?.evidenceStatus === "Complete" && Number(item.physicalPosition.endingBacklog) > 0).length
        : null,
      averageInventoryValue: physicalInventoryEvidenceComplete
        ? physicalInventoryValues.reduce((sum, item) => sum + item, 0) / physicalInventoryValues.length
        : null,
      peakInventoryValue: physicalInventoryEvidenceComplete ? Math.max(...physicalInventoryValues) : null,
      replenishmentOrderCount,
      inventoryValueDelta: physicalInventoryEvidenceComplete
        && trend.comparison?.physicalDeltaEvidenceStatus === "Complete"
        ? valueOr(trend.comparison?.physicalAverageInventoryValueDelta, trend.comparison?.averageInventoryValueDelta)
        : null,
    }
  };
}

function renderBufferTrendWorkspace(trend) {
  const initialSelection = normalizeFutureInventorySelection(trend);
  const filteredTrend = filterBufferTrendWorkspace(initialSelection.trend);
  if (!filteredTrend) {
    byId("buffer-trend-kpis").innerHTML = "";
    byId("buffer-trend-chart").innerHTML = `<div class="table-empty"><strong>没有缓冲趋势图数据</strong></div>`;
    byId("inventory-flow-evidence").innerHTML = `<div class="table-empty"><strong>没有物理库存投影证据</strong></div>`;
    byId("inventory-flow-chart").innerHTML = `<div class="table-empty"><strong>没有物理库存投影数据</strong></div>`;
    byId("buffer-volatility-chart").innerHTML = `<div class="table-empty"><strong>没有需求波动证据</strong></div>`;
    byId("buffer-trend-heatmap").innerHTML = `<div class="table-empty"><strong>没有缓冲热力格数据</strong></div>`;
    byId("buffer-family-summary-body").innerHTML = emptyRow("没有产品族汇总数据", 6);
    byId("buffer-trend-body").innerHTML = emptyRow("没有缓冲趋势数据", 10);
    byId("buffer-replenishment-body").innerHTML = emptyRow("没有补货订单", 4);
    byId("buffer-sku-metadata").innerHTML = "";
    byId("buffer-trace-list").innerHTML = "";
    return;
  }

  const selection = normalizeFutureInventorySelection(trend, filteredTrend);

  state.bufferTrend = selection.trend;
  const fullDetail = valueOr(filteredTrend.skuDetails.find(item => item.sku === filteredTrend.selectedSku), filteredTrend.skuDetails[0]);
  const detail = scopeFutureBufferDetail(fullDetail, selection, filteredTrend);
  renderFutureInventorySelectionControls(selection);
  byId("buffer-trend-case-chip").textContent = caseLabel(filteredTrend.name);
  byId("buffer-trend-kpis").innerHTML = [
    ["净流量红区 SKU", number(filteredTrend.kpis.redSkuCount), "补货前净流量位置"],
    ["在手红区 SKU", filteredTrend.kpis.onHandRedSkuCount === null ? "证据缺失" : number(filteredTrend.kpis.onHandRedSkuCount), "期末在手库存位置"],
    ["净流量≤0周", number(filteredTrend.kpis.shortageCount), "补货前净流量小于等于 0"],
    ["在手短缺周", filteredTrend.kpis.onHandStockoutWeekCount === null ? "证据缺失" : number(filteredTrend.kpis.onHandStockoutWeekCount), "期末积压大于 0"],
    ["平均库存金额", metricOrEvidenceMissing(filteredTrend.kpis.averageInventoryValue, money), `变化 ${metricOrEvidenceMissing(filteredTrend.kpis.inventoryValueDelta, money)}`],
    ["峰值库存金额", metricOrEvidenceMissing(filteredTrend.kpis.peakInventoryValue, money), "计划范围内最高"],
    ["补货订单", number(filteredTrend.kpis.replenishmentOrderCount), "按订货周期复核生成"],
  ].map(([label, value, note]) => `<div><span>${label}</span><strong>${value}</strong><small>${note}</small></div>`).join("");

  renderBufferTrendChart(detail);
  renderInventoryFlowChart(selection.previewCase, detail);
  renderBufferVolatilityChart(detail);
  renderBufferInventoryOptions(filteredTrend, detail);
  renderBufferComparison(filteredTrend);
  renderBufferHeatmap(filteredTrend);
  renderBufferFamilySummary(filteredTrend);
  renderBufferSkuDetail(detail, selection.previewCase);
}

function renderBufferInventoryOptions(trend, detail) {
  const families = [...new Set(trend.skuDetails.map(item => item.family))].sort((left, right) => left.localeCompare(right, "zh-CN"));
  const selectedFamily = valueOr(valueOr(detail?.family, families[0]), "");
  const skus = trend.skuDetails
    .filter(item => !selectedFamily || item.family === selectedFamily)
    .sort((left, right) => left.sku.localeCompare(right.sku, "zh-CN"));

  byId("buffer-inventory-options").innerHTML = `
    <div class="inventory-option-block">
      <div class="inventory-option-title">产品族</div>
      <div class="inventory-option-list">
        ${families.map(family => `
          <button class="inventory-option ${family === selectedFamily ? "is-selected" : ""}" type="button" data-buffer-family="${escapeHtml(family)}">
            <span class="option-radio"></span><span>${escapeHtml(family)}</span>
          </button>
        `).join("")}
      </div>
    </div>
    <div class="inventory-option-block">
      <div class="inventory-option-title">库存物料</div>
      <div class="inventory-option-list is-scrollable">
        ${skus.map(item => `
          <button class="inventory-option ${item.sku === detail?.sku ? "is-selected" : ""}" type="button" data-buffer-sku="${escapeHtml(item.sku)}">
            <span class="option-radio"></span><span><strong>${escapeHtml(item.sku)}</strong><small>${escapeHtml(item.name)}</small></span>
          </button>
        `).join("")}
      </div>
    </div>`;
}

function monotonePathParts(points) {
  if (!Array.isArray(points)) return null;
  const isBlankCoordinate = value => value === null || value === undefined ||
    (typeof value === "string" && value.trim() === "");
  if (points.some(point => !point || isBlankCoordinate(point.x) || isBlankCoordinate(point.y))) return null;
  const values = points.map(point => ({ x: Number(point.x), y: Number(point.y) }));
  if (values.length === 0 || values.some(point => !Number.isFinite(point.x) || !Number.isFinite(point.y))) return null;
  if (values.length === 1) return { start: values[0], end: values[0], segments: [] };
  if (values.some((point, index) => index > 0 && point.x <= values[index - 1].x)) return null;

  const h = values.slice(0, -1).map((point, index) => values[index + 1].x - point.x);
  const delta = values.slice(0, -1).map((point, index) => (values[index + 1].y - point.y) / h[index]);
  const slopes = new Array(values.length).fill(0);

  const endpointSlope = (h0, h1, delta0, delta1) => {
    let slope = ((2 * h0 + h1) * delta0 - h0 * delta1) / (h0 + h1);
    if (Math.sign(slope) !== Math.sign(delta0)) slope = 0;
    else if (Math.sign(delta0) !== Math.sign(delta1) && Math.abs(slope) > Math.abs(3 * delta0)) slope = 3 * delta0;
    return slope;
  };

  if (values.length === 2) {
    slopes[0] = delta[0];
    slopes[1] = delta[0];
  } else {
    slopes[0] = endpointSlope(h[0], h[1], delta[0], delta[1]);
    slopes[values.length - 1] = endpointSlope(
      h[h.length - 1], h[h.length - 2], delta[delta.length - 1], delta[delta.length - 2]);
    for (let index = 1; index < values.length - 1; index += 1) {
      if (delta[index - 1] === 0 || delta[index] === 0 || Math.sign(delta[index - 1]) !== Math.sign(delta[index])) {
        slopes[index] = 0;
      } else {
        const firstWeight = 2 * h[index] + h[index - 1];
        const secondWeight = h[index] + 2 * h[index - 1];
        slopes[index] = (firstWeight + secondWeight) /
          (firstWeight / delta[index - 1] + secondWeight / delta[index]);
      }
    }
  }

  const segments = values.slice(0, -1).map((point, index) => {
    const next = values[index + 1];
    const width = next.x - point.x;
    const firstControl = { x: point.x + width / 3, y: point.y + slopes[index] * width / 3 };
    const secondControl = { x: next.x - width / 3, y: next.y - slopes[index + 1] * width / 3 };
    return { start: point, firstControl, secondControl, end: next };
  });
  return { start: values[0], end: values[values.length - 1], segments };
}

function buildMonotonePath(points) {
  const parts = monotonePathParts(points);
  if (!parts) return "";
  const commands = parts.segments.map(segment =>
    `C ${segment.firstControl.x},${segment.firstControl.y} ` +
    `${segment.secondControl.x},${segment.secondControl.y} ${segment.end.x},${segment.end.y}`);
  return `M ${parts.start.x},${parts.start.y} ${commands.join(" ")}`;
}

function buildMonotoneAreaPath(lowerPoints, upperPoints, linearSegmentIndexes = new Set()) {
  const upper = monotonePathParts(upperPoints);
  const lower = monotonePathParts(lowerPoints);
  if (!upper || !lower || upperPoints.length !== lowerPoints.length ||
      lowerPoints.some((point, index) => Number(point.x) !== Number(upperPoints[index].x))) return "";
  const fallback = linearSegmentIndexes instanceof Set
    ? linearSegmentIndexes
    : new Set(valueOr(linearSegmentIndexes, []));
  const upperCommands = upper.segments.map((segment, index) => fallback.has(index)
    ? `L ${segment.end.x},${segment.end.y}`
    : `C ${segment.firstControl.x},${segment.firstControl.y} ` +
      `${segment.secondControl.x},${segment.secondControl.y} ${segment.end.x},${segment.end.y}`);
  const reverseLowerCommands = [...lower.segments].reverse().map((segment, reverseIndex) => {
    const index = lower.segments.length - reverseIndex - 1;
    return fallback.has(index)
      ? `L ${segment.start.x},${segment.start.y}`
      : `C ${segment.secondControl.x},${segment.secondControl.y} ` +
        `${segment.firstControl.x},${segment.firstControl.y} ${segment.start.x},${segment.start.y}`;
  });
  return `M ${upper.start.x},${upper.start.y} ${upperCommands.join(" ")} ` +
    `L ${lower.end.x},${lower.end.y} ${reverseLowerCommands.join(" ")} Z`;
}

function monotoneCrossingSegments(lowerPoints, upperPoints) {
  const lower = monotonePathParts(lowerPoints);
  const upper = monotonePathParts(upperPoints);
  const crossings = new Set();
  if (!lower || !upper || lowerPoints.length !== upperPoints.length ||
      lowerPoints.some((point, index) => Number(point.x) !== Number(upperPoints[index].x))) return crossings;
  lower.segments.forEach((segment, index) => {
    const upperSegment = upper.segments[index];
    const clearances = [
      segment.start.y - upperSegment.start.y,
      segment.firstControl.y - upperSegment.firstControl.y,
      segment.secondControl.y - upperSegment.secondControl.y,
      segment.end.y - upperSegment.end.y,
    ];
    if (clearances.some(value => value < -1e-7)) crossings.add(index);
  });
  return crossings;
}

function isFiniteChartValue(value) {
  return value !== null && value !== undefined &&
    !(typeof value === "string" && value.trim() === "") &&
    Number.isFinite(Number(value));
}

function hasCompletePhysicalInventoryEvidence(previewCase) {
  const caseId = typeof previewCase?.caseId === "string" ? previewCase.caseId : "";
  const flow = previewCase?.inventoryFlow;
  const expectedPoints = previewCase?.plan?.bufferProjections;
  if (!caseId || flow?.status !== "Complete" || flow?.caseId !== caseId || !flow?.summary
    || !Array.isArray(flow.points) || !Array.isArray(expectedPoints) || expectedPoints.length === 0) {
    return false;
  }

  const keyOf = item => `${String(valueOr(item?.sku, ""))}\u0000${String(valueOr(item?.week, ""))}`;
  const expectedKeys = expectedPoints.map(keyOf);
  const actualKeys = flow.points.map(keyOf);
  const expectedSet = new Set(expectedKeys);
  const actualSet = new Set(actualKeys);
  return expectedSet.size === expectedKeys.length
    && actualSet.size === actualKeys.length
    && expectedSet.size === actualSet.size
    && expectedKeys.every(key => actualSet.has(key));
}

function hasValidBufferZoneEvidence(item) {
  return isFiniteChartValue(item.topOfRed) &&
    isFiniteChartValue(item.topOfYellow) &&
    isFiniteChartValue(item.topOfGreen) &&
    Number(item.topOfRed) >= 0 &&
    Number(item.topOfRed) <= Number(item.topOfYellow) &&
    Number(item.topOfYellow) <= Number(item.topOfGreen);
}

function renderBufferTrendChart(detail) {
  detail = scopedFutureBufferDetailForCharts(detail);
  const chartDomain = valueOr(detail?.chartDomain, []);
  if (!detail || chartDomain.length === 0) {
    byId("buffer-selected-title").textContent = "选中 SKU 水位趋势";
    byId("buffer-trend-chart").innerHTML = `<div class="table-empty"><strong>没有选中 SKU 趋势数据</strong></div>`;
    return;
  }

  const baselineDetail = state.baselineBufferTrend?.skuDetails?.find(item => item.sku === detail.sku);
  const showPreview = state.bufferTrend?.caseId && state.baselineBufferTrend?.caseId && state.bufferTrend.caseId !== state.baselineBufferTrend.caseId;
  byId("buffer-selected-title").textContent = `${detail.sku} 三项同步证据`;
  const width = 940;
  const height = 310;
  const left = 62;
  const right = 26;
  const top = 24;
  const mainHeight = 250;
  const plotWidth = width - left - right;
  const caseId = valueOr(state.futureInventorySelection.caseId, valueOr(state.bufferTrend?.caseId, "baseline"));
  const weekFrom = chartDomain[0].week;
  const weekThrough = chartDomain[chartDomain.length - 1].week;
  const baselineByWeek = new Map(valueOr(baselineDetail?.series, [])
    .filter(item => Number(item.week) >= weekFrom && Number(item.week) <= weekThrough)
    .map(item => [Number(item.week), item]));
  const baselineItem = point => valueOr(baselineByWeek.get(point.week), null);
  const allNetFlowValues = [
    ...chartDomain.flatMap(point => point.item
      ? [point.item.endNetFlowBeforeReplenishment, point.item.endNetFlowAfterReplenishment]
      : []),
    ...chartDomain.flatMap(point => baselineItem(point)
      ? [baselineItem(point).endNetFlowBeforeReplenishment, baselineItem(point).endNetFlowAfterReplenishment]
      : []),
    ...chartDomain.flatMap(point => point.item
      ? [
        point.item.topOfRed,
        point.item.topOfYellow,
        point.item.topOfGreen,
        point.item.physicalPosition?.evidenceStatus === "Complete"
          ? point.item.physicalPosition.endingOnHand
          : null,
      ]
      : []),
    0,
  ].filter(isFiniteChartValue).map(Number);
  const valueMinimum = Math.min(0, ...allNetFlowValues);
  const valueMaximum = Math.max(0, ...allNetFlowValues);
  const valueSpan = Math.max(1, valueMaximum - valueMinimum);
  const yMinimum = valueMinimum < 0 ? valueMinimum - valueSpan * 0.08 : 0;
  const yMaximum = valueMaximum + valueSpan * 0.08;
  const y = value => top + (yMaximum - Number(value)) * mainHeight / Math.max(1, yMaximum - yMinimum);
  const x = index => futureInventoryChartX(index, chartDomain.length);
  const zoneSegments = contiguousEvidenceSegments(
    chartDomain,
    point => point.item && hasValidBufferZoneEvidence(point.item));
  const zoneAreas = zoneSegments.map(segment => {
    if (segment.length < 2) return "";
    const zeroPoints = segment.map(point => ({ x: x(point.index), y: y(0) }));
    const redPoints = segment.map(point => ({ x: x(point.index), y: y(point.item.topOfRed) }));
    const yellowPoints = segment.map(point => ({ x: x(point.index), y: y(point.item.topOfYellow) }));
    const greenPoints = segment.map(point => ({ x: x(point.index), y: y(point.item.topOfGreen) }));
    const linearSegments = new Set([
      ...monotoneCrossingSegments(zeroPoints, redPoints),
      ...monotoneCrossingSegments(redPoints, yellowPoints),
      ...monotoneCrossingSegments(yellowPoints, greenPoints),
    ]);
    return `
      <path class="buffer-zone-red" d="${buildMonotoneAreaPath(zeroPoints, redPoints, linearSegments)}" data-week-from="${segment[0].week}" data-week-through="${segment[segment.length - 1].week}"></path>
      <path class="buffer-zone-yellow" d="${buildMonotoneAreaPath(redPoints, yellowPoints, linearSegments)}" data-week-from="${segment[0].week}" data-week-through="${segment[segment.length - 1].week}"></path>
      <path class="buffer-zone-green" d="${buildMonotoneAreaPath(yellowPoints, greenPoints, linearSegments)}" data-week-from="${segment[0].week}" data-week-through="${segment[segment.length - 1].week}"></path>`;
  }).join("");
  const zoneEvidenceMarkers = zoneSegments.filter(segment => segment.length === 1).map(segment => {
    const point = segment[0];
    return `
      <circle class="buffer-zone-evidence-marker is-red" data-week="${point.week}" data-x="${x(point.index)}" cx="${x(point.index)}" cy="${y(point.item.topOfRed)}" r="4"><title>${escapeHtml(futureInventoryDateLabel(point))} 红区上沿：${point.item.topOfRed}</title></circle>
      <circle class="buffer-zone-evidence-marker is-yellow" data-week="${point.week}" data-x="${x(point.index)}" cx="${x(point.index)}" cy="${y(point.item.topOfYellow)}" r="4"><title>${escapeHtml(futureInventoryDateLabel(point))} 黄区上沿：${point.item.topOfYellow}</title></circle>
      <circle class="buffer-zone-evidence-marker is-green" data-week="${point.week}" data-x="${x(point.index)}" cx="${x(point.index)}" cy="${y(point.item.topOfGreen)}" r="4"><title>${escapeHtml(futureInventoryDateLabel(point))} 绿区上沿：${point.item.topOfGreen}</title></circle>`;
  }).join("");
  const zoneEvidenceMissing = chartDomain.some(point => !point.item || !hasValidBufferZoneEvidence(point.item));
  const redTop = chartDomain.map(point => point.item?.topOfRed).filter(isFiniteChartValue).map(Number);
  const yellowTop = chartDomain.map(point => point.item?.topOfYellow).filter(isFiniteChartValue).map(Number);
  const greenTop = chartDomain.map(point => point.item?.topOfGreen).filter(isFiniteChartValue).map(Number);
  const netFlowBeforeField = 'data-field="endNetFlowBeforeReplenishment"';
  const netFlowAfterField = 'data-field="endNetFlowAfterReplenishment"';
  const renderEvidenceLine = (cssClass, markerClass, fieldAttribute, valueSelector, title) => {
    const segments = contiguousEvidenceSegments(chartDomain, point => isFiniteChartValue(valueSelector(point)));
    const lines = segments.map(segment => segment.length < 2
      ? ""
      : `<polyline class="${cssClass}" ${fieldAttribute} data-week-from="${segment[0].week}" data-week-through="${segment[segment.length - 1].week}" points="${segment.map(point => `${x(point.index)},${y(valueSelector(point))}`).join(" ")}"><title>${title}</title></polyline>`).join("");
    const markers = segments.filter(segment => segment.length === 1).map(segment => {
      const point = segment[0];
      return `<circle class="${markerClass}" ${fieldAttribute} data-week="${point.week}" data-x="${x(point.index)}" cx="${x(point.index)}" cy="${y(valueSelector(point))}" r="3"><title>${title}</title></circle>`;
    }).join("");
    return lines + markers;
  };
  const baselineLine = showPreview
    ? renderEvidenceLine(
      "buffer-baseline-line",
      "buffer-baseline-marker",
      netFlowAfterField,
      point => baselineItem(point)?.endNetFlowAfterReplenishment,
      "基准补货后净流位置")
    : "";
  const previewLine = showPreview
    ? renderEvidenceLine(
      "buffer-preview-line",
      "buffer-preview-marker",
      netFlowAfterField,
      point => point.item?.endNetFlowAfterReplenishment,
      "所选方案补货后净流位置")
    : "";
  const currentLine = showPreview
    ? ""
    : renderEvidenceLine(
      "buffer-inventory-line",
      "buffer-inventory-marker",
      netFlowAfterField,
      point => point.item?.endNetFlowAfterReplenishment,
      "补货后净流位置");
  const netFlowLine = renderEvidenceLine(
    "buffer-net-flow-line",
    "buffer-net-flow-marker",
    netFlowBeforeField,
    point => point.item?.endNetFlowBeforeReplenishment,
    "补货前净流位置");
  const onHandLine = renderEvidenceLine(
    "buffer-on-hand-line",
    "buffer-on-hand-marker",
    'data-field="physicalPosition.endingOnHand"',
    point => point.item?.physicalPosition?.evidenceStatus === "Complete"
      ? point.item.physicalPosition.endingOnHand
      : null,
    "期末在手库存（执行风险）");
  const reviewMarkers = chartDomain
    .map(point => point.item?.isReplenishment
      ? `<line class="${point.item.isPrebuild ? "review-marker prebuild" : "review-marker"}" x1="${x(point.index)}" y1="${top}" x2="${x(point.index)}" y2="${top + mainHeight}"><title>${point.item.isPrebuild ? "提前建库订单" : "订货周期补货订单"}：${number(point.item.replenishmentQuantity)}</title></line>`
      : "")
    .join("");
  const timeGrid = chartDomain.map(point => `<line class="time-grid-line" x1="${x(point.index)}" y1="${top}" x2="${x(point.index)}" y2="${top + mainHeight}"></line>`).join("");
  const gaps = chartDomain.filter(point => !point.item).map(point => `
    <g class="nfp-evidence-gap" data-week="${point.week}" data-x="${x(point.index)}">
      <line x1="${x(point.index)}" y1="${top}" x2="${x(point.index)}" y2="${top + mainHeight}"></line>
      <text x="${x(point.index)}" y="${top + 12}">第 ${point.week} 周证据缺口</text>
    </g>`).join("");
  const monthLabels = chartDomain.map(point => `<text class="buffer-week-label" x="${x(point.index)}" y="${height - 10}">${escapeHtml(futureInventoryDateLabel(point))}</text>`).join("");

  byId("buffer-trend-chart").innerHTML = `
    <svg class="buffer-svg" viewBox="0 0 ${width} ${height}" role="img" aria-label="${escapeHtml(detail.sku)} 动态红黄绿缓冲带与净流位置" data-case-id="${escapeHtml(caseId)}" data-week-from="${weekFrom}" data-week-through="${weekThrough}">
      <rect class="buffer-plot-bg" x="${left}" y="${top}" width="${plotWidth}" height="${mainHeight}"></rect>
      <text class="axis-title vertical" transform="translate(16 ${top + mainHeight / 2}) rotate(-90)">缓冲区 / 净流动量</text>
      ${greenTop.length ? `<text class="axis-label" x="${left - 8}" y="${y(Math.max(...greenTop))}">${number(Math.max(...greenTop))}</text>` : ""}
      ${yellowTop.length ? `<text class="axis-label" x="${left - 8}" y="${y(Math.max(...yellowTop))}">${number(Math.max(...yellowTop))}</text>` : ""}
      ${redTop.length ? `<text class="axis-label" x="${left - 8}" y="${y(Math.max(...redTop))}">${number(Math.max(...redTop))}</text>` : ""}
      <line class="axis-line" x1="${left}" y1="${y(0)}" x2="${width - right}" y2="${y(0)}"></line>
      ${timeGrid}
      ${zoneAreas}
      ${zoneEvidenceMarkers}
      ${gaps}
      ${reviewMarkers}
      ${netFlowLine}
      ${onHandLine}
      ${baselineLine}
      ${currentLine}
      ${previewLine}
      ${zoneEvidenceMissing ? `<text class="buffer-zone-evidence-note" x="${left + 8}" y="${top + 14}">缓冲区证据缺失</text>` : ""}
      ${monthLabels}
    </svg>
    <div class="buffer-chart-legend">
      <span><i class="zone red"></i>红区</span>
      <span><i class="zone yellow"></i>黄区</span>
      <span><i class="zone green"></i>绿区</span>
      <span><i class="line net-flow"></i>补货前净流量位置</span>
      <span><i class="line on-hand"></i>期末在手库存（执行风险）</span>
      ${showPreview
        ? `<span><i class="line baseline"></i>基准补货后净流</span><span><i class="line preview"></i>预览补货后净流</span>`
        : `<span><i class="line inventory"></i>补货后净流位置</span>`}
    </div>`;
}

function renderInventoryFlowChart(previewCase, detail) {
  detail = scopedFutureBufferDetailForCharts(detail);
  const flow = previewCase?.inventoryFlow;
  const metricEvidence = valueOr(previewCase?.scenarioMetricEvidence, []);
  const isLegacyReference = valueOr(flow?.trace, []).some(item => item.stage === "LegacyResult");
  const caseId = valueOr(previewCase?.caseId, valueOr(state.futureInventorySelection.caseId, "baseline"));
  const baselineSnapshotId = valueOr(
    flow?.baselineSnapshotId,
    valueOr(metricEvidence.find(item => item.baselineSnapshotId)?.baselineSnapshotId, "未关联冻结版本"));
  const traceSource = valueOr(
    flow?.trace?.[0]?.stage,
    valueOr(metricEvidence.find(item => item.source)?.source, "等待后端投影"));
  const statusText = flow?.status === "Complete" ? "证据完整" : "证据缺失";
  const statusClassName = flow?.status === "Complete" ? "is-valid" : "is-invalid";
  const summary = flow?.summary;
  byId("inventory-flow-evidence").innerHTML = isLegacyReference
    ? `
      <span class="status-chip is-warning">历史兼容记录</span>
      <span>旧记录没有物理库存投影，物理指标不展示</span>
      <span>净流位置证据仍可查看</span>`
    : `
      <span class="status-chip ${statusClassName}">${statusText}</span>
      <span>冻结版本：${escapeHtml(baselineSnapshotId)}</span>
      <span>追踪来源：${escapeHtml(traceSource)}</span>
      ${flow?.status === "Complete" && summary ? `
        <span class="physical-summary-metric">期末现有金额：${money(summary.endingInventoryValue)}</span>
        <span class="physical-summary-metric">期末积压：${number(summary.endingBacklog)}</span>
        <span class="physical-summary-metric">准时满足率：${summary.onTimeServicePercent === null ? "不适用" : percent(summary.onTimeServicePercent)}</span>` : ""}`;

  const chartDomain = valueOr(detail?.chartDomain, []);
  if (!detail || chartDomain.length === 0) {
    byId("inventory-flow-chart").innerHTML = `<div class="table-empty"><strong>没有选中 SKU 的物理库存范围</strong></div>`;
    return;
  }

  if (!flow || flow.status !== "Complete" || isLegacyReference) {
    const issueText = isLegacyReference
      ? "历史兼容记录未保存物理库存投影"
      : valueOr(flow?.issues, []).length
        ? flow.issues.map(item => escapeHtml(item.reason)).join("；")
        : "后端物理库存证据缺失";
    byId("inventory-flow-chart").innerHTML = `
      <div class="inventory-flow-empty" data-case-id="${escapeHtml(caseId)}">
        <strong>${isLegacyReference ? "历史兼容记录" : "物理库存不作图"}</strong>
        <span>${issueText}</span>
      </div>`;
    return;
  }

  const width = 940;
  const height = 270;
  const left = 62;
  const right = 26;
  const top = 24;
  const plotHeight = 190;
  const plotBottom = top + plotHeight;
  const plotWidth = width - left - right;
  const weekFrom = chartDomain[0].week;
  const weekThrough = chartDomain[chartDomain.length - 1].week;
  const pointByWeek = new Map(valueOr(flow.points, [])
    .filter(point => point.sku === detail.sku)
    .map(point => [Number(point.week), point]));
  const physicalPoint = domainPoint => domainPoint.item ? valueOr(pointByWeek.get(domainPoint.week), null) : null;
  const x = index => futureInventoryChartX(index, chartDomain.length);
  const onHandValues = chartDomain
    .map(item => physicalPoint(item)?.endingOnHand)
    .filter(isFiniteChartValue)
    .map(Number);
  const eventFields = [
    { field: "frozenReceiptQuantity", cssClass: "physical-frozen-receipt", label: "冻结确认到货" },
    { field: "simulatedReceiptQuantity", cssClass: "physical-simulated-receipt", label: "模拟到货" },
    { field: "prebuildReceiptQuantity", cssClass: "physical-prebuild-receipt", label: "提前建库" },
    { field: "endingBacklog", cssClass: "physical-ending-backlog", label: "期末积压" },
  ];
  const eventValues = chartDomain.flatMap(item => eventFields
    .map(config => physicalPoint(item)?.[config.field])
    .filter(isFiniteChartValue)
    .map(Number));
  const onHandMaximum = Math.max(1, ...onHandValues) * 1.08;
  const eventMaximum = Math.max(1, ...eventValues) * 1.12;
  const yOnHand = value => top + (onHandMaximum - Number(value)) * plotHeight / onHandMaximum;
  const yEvent = value => top + (eventMaximum - Number(value)) * plotHeight / eventMaximum;
  const validPhysicalPoint = item => physicalPoint(item) && isFiniteChartValue(physicalPoint(item).endingOnHand);
  const physicalSegments = contiguousEvidenceSegments(chartDomain, validPhysicalPoint);
  const onHandPaths = physicalSegments.map(segment => {
    if (segment.length < 2) return "";
    const upperPoints = segment.map(item => ({ x: x(item.index), y: yOnHand(physicalPoint(item).endingOnHand) }));
    const lowerPoints = segment.map(item => ({ x: x(item.index), y: plotBottom }));
    return `
      <path class="physical-on-hand-area" data-field="endingOnHand" data-week-from="${segment[0].week}" data-week-through="${segment[segment.length - 1].week}" d="${buildLinearAreaPath(lowerPoints, upperPoints)}"></path>
      <path class="physical-on-hand-line" data-field="endingOnHand" data-week-from="${segment[0].week}" data-week-through="${segment[segment.length - 1].week}" d="${buildMonotonePath(upperPoints)}"></path>`;
  }).join("");
  const onHandMarkers = chartDomain.filter(validPhysicalPoint).map(item =>
    `<circle class="physical-on-hand-marker" data-field="endingOnHand" data-week="${item.week}" data-x="${x(item.index)}" cx="${x(item.index)}" cy="${yOnHand(physicalPoint(item).endingOnHand)}" r="3"><title>第 ${item.week} 周期末现有量：${number(physicalPoint(item).endingOnHand)}</title></circle>`).join("");
  const barWidth = Math.max(3, Math.min(11, plotWidth / Math.max(1, chartDomain.length) / 6));
  const eventBars = chartDomain.flatMap(item => eventFields.map((config, fieldIndex) => {
    const value = physicalPoint(item)?.[config.field];
    if (!isFiniteChartValue(value)) return "";
    const barX = x(item.index) + (fieldIndex - 1.5) * (barWidth + 1);
    const barY = yEvent(value);
    return `<rect class="${config.cssClass}" data-field="${config.field}" x="${barX}" y="${barY}" width="${barWidth}" height="${Math.max(0, plotBottom - barY)}"><title>第 ${item.week} 周${config.label}：${number(value)}</title></rect>`;
  })).join("");
  const gaps = chartDomain.filter(item => !validPhysicalPoint(item)).map(item => `
    <g class="physical-evidence-gap" data-week="${item.week}" data-x="${x(item.index)}">
      <line x1="${x(item.index)}" y1="${top}" x2="${x(item.index)}" y2="${plotBottom}"></line>
      <text x="${x(item.index)}" y="${top + 12}">第 ${item.week} 周证据缺口</text>
    </g>`).join("");
  const dateLabels = chartDomain.map(item =>
    `<text class="buffer-week-label" x="${x(item.index)}" y="${height - 10}">${escapeHtml(futureInventoryDateLabel(item))}</text>`).join("");

  byId("inventory-flow-chart").innerHTML = `
    <svg class="inventory-flow-svg" viewBox="0 0 ${width} ${height}" role="img" aria-label="${escapeHtml(detail.sku)} 物理库存投影" data-case-id="${escapeHtml(caseId)}" data-week-from="${weekFrom}" data-week-through="${weekThrough}">
      <rect class="inventory-flow-bg" x="${left}" y="${top}" width="${plotWidth}" height="${plotHeight}"></rect>
      <text class="axis-title vertical" data-axis="physical-on-hand" transform="translate(16 ${top + plotHeight / 2}) rotate(-90)">期末现有量</text>
      <text class="axis-title vertical right" data-axis="physical-events" transform="translate(${width - 12} ${top + plotHeight / 2}) rotate(90)">到货 / 积压</text>
      <line class="axis-line" x1="${left}" y1="${plotBottom}" x2="${width - right}" y2="${plotBottom}"></line>
      ${eventBars}
      ${onHandPaths}
      ${onHandMarkers}
      ${gaps}
      ${dateLabels}
    </svg>
    <div class="buffer-chart-legend">
      <span><i class="area physical-on-hand"></i>期末现有量</span>
      <span><i class="bar frozen-receipt"></i>冻结确认到货</span>
      <span><i class="bar simulated-receipt"></i>模拟到货</span>
      <span><i class="bar prebuild-receipt"></i>提前建库</span>
      <span><i class="bar ending-backlog"></i>期末积压</span>
    </div>`;
}

function renderBufferVolatilityChart(detail) {
  detail = scopedFutureBufferDetailForCharts(detail);
  const chartDomain = valueOr(detail?.chartDomain, []);
  if (!detail || chartDomain.length === 0) {
    byId("buffer-volatility-chart").innerHTML = `<div class="table-empty"><strong>没有需求波动证据</strong></div>`;
    return;
  }

  const width = 940;
  const height = 190;
  const left = 62;
  const right = 26;
  const top = 22;
  const plotHeight = 108;
  const plotBottom = top + plotHeight;
  const plotWidth = width - left - right;
  const caseId = valueOr(state.futureInventorySelection.caseId, valueOr(state.bufferTrend?.caseId, "baseline"));
  const weekFrom = chartDomain[0].week;
  const weekThrough = chartDomain[chartDomain.length - 1].week;
  const x = index => futureInventoryChartX(index, chartDomain.length);
  const demandSegments = contiguousEvidenceSegments(
    chartDomain,
    point => point.item && isFiniteChartValue(point.item.demand) && Number(point.item.demand) >= 0);
  const thresholdSegments = contiguousEvidenceSegments(
    chartDomain,
    point => point.item && isFiniteChartValue(point.item.demandSpikeThreshold) && Number(point.item.demandSpikeThreshold) >= 0);
  const scaleValues = [
    ...demandSegments.flatMap(segment => segment.map(point => Number(point.item.demand))),
    ...thresholdSegments.flatMap(segment => segment.map(point => Number(point.item.demandSpikeThreshold))),
    0,
  ];
  const yMax = Math.max(1, ...scaleValues) * 1.08;
  const y = value => top + (yMax - Number(value)) * plotHeight / yMax;
  const demandAreas = demandSegments.map(segment => {
    if (segment.length < 2) return "";
    const lowerPoints = segment.map(point => ({ x: x(point.index), y: y(0) }));
    const upperPoints = segment.map(point => ({ x: x(point.index), y: y(point.item.demand) }));
    return `<path class="buffer-demand-area" d="${buildMonotoneAreaPath(lowerPoints, upperPoints)}" data-week-from="${segment[0].week}" data-week-through="${segment[segment.length - 1].week}"></path>`;
  }).join("");
  const thresholdLines = thresholdSegments.map(segment => {
    if (segment.length < 2) return "";
    const points = segment.map(point => ({ x: x(point.index), y: y(point.item.demandSpikeThreshold) }));
    return `<path class="buffer-demand-threshold" d="${buildMonotonePath(points)}" data-week-from="${segment[0].week}" data-week-through="${segment[segment.length - 1].week}"></path>`;
  }).join("");
  const demandMarkers = chartDomain.filter(point => point.item && isFiniteChartValue(point.item.demand)).map(point =>
    `<circle class="buffer-demand-marker" data-week="${point.week}" data-x="${x(point.index)}" cx="${x(point.index)}" cy="${y(point.item.demand)}" r="2.5"><title>${escapeHtml(futureInventoryDateLabel(point))} 计划需求：${number(point.item.demand)}</title></circle>`).join("");
  const thresholdMarkers = chartDomain.filter(point => point.item && isFiniteChartValue(point.item.demandSpikeThreshold)).map(point =>
    `<circle class="buffer-demand-threshold-marker" data-week="${point.week}" data-x="${x(point.index)}" cx="${x(point.index)}" cy="${y(point.item.demandSpikeThreshold)}" r="2.5"><title>${escapeHtml(futureInventoryDateLabel(point))} 后端尖峰阈值：${number(point.item.demandSpikeThreshold)}</title></circle>`).join("");
  const thresholdMissing = chartDomain.some(point => !point.item || !isFiniteChartValue(point.item.demandSpikeThreshold));
  const demandMissing = chartDomain.some(point => !point.item || !isFiniteChartValue(point.item.demand));
  const gaps = chartDomain.filter(point => !point.item).map(point => `
    <g class="volatility-evidence-gap" data-week="${point.week}" data-x="${x(point.index)}">
      <line x1="${x(point.index)}" y1="${top}" x2="${x(point.index)}" y2="${plotBottom}"></line>
      <text x="${x(point.index)}" y="${top + 12}">第 ${point.week} 周证据缺口</text>
    </g>`).join("");
  const dateLabels = chartDomain.map(point =>
    `<text class="buffer-week-label" x="${x(point.index)}" y="${height - 10}">${escapeHtml(futureInventoryDateLabel(point))}</text>`).join("");

  byId("buffer-volatility-chart").innerHTML = `
    <svg class="buffer-volatility-svg" viewBox="0 0 ${width} ${height}" role="img" aria-label="${escapeHtml(detail.sku)} 需求波动图" data-case-id="${escapeHtml(caseId)}" data-week-from="${weekFrom}" data-week-through="${weekThrough}">
      <rect class="buffer-volatility-bg" x="${left}" y="${top}" width="${plotWidth}" height="${plotHeight}"></rect>
      <text class="axis-title vertical" transform="translate(16 ${top + plotHeight / 2}) rotate(-90)">计划需求</text>
      <line class="axis-line" x1="${left}" y1="${plotBottom}" x2="${width - right}" y2="${plotBottom}"></line>
      ${demandAreas}
      ${demandMarkers}
      ${thresholdLines}
      ${thresholdMarkers}
      ${gaps}
      ${thresholdMissing ? `<text class="buffer-demand-evidence-note" x="${left + 8}" y="${top + 14}">尖峰阈值证据缺失</text>` : ""}
      ${demandMissing ? `<text class="buffer-demand-evidence-note" x="${left + 8}" y="${top + 28}">计划需求证据缺失</text>` : ""}
      ${dateLabels}
    </svg>
    <div class="buffer-chart-legend">
      <span><i class="area demand"></i>计划需求</span>
      <span><i class="line demand-threshold"></i>后端尖峰阈值</span>
    </div>`;
}

function renderBufferComparison(trend) {
  const comparison = valueOr(trend.comparison, {});
  const physicalEvidenceComplete = trend.physicalInventoryEvidenceStatus === "Complete"
    && comparison.physicalDeltaEvidenceStatus === "Complete";
  const averageInventoryValueDelta = physicalEvidenceComplete
    ? valueOr(comparison.physicalAverageInventoryValueDelta, comparison.averageInventoryValueDelta)
    : null;
  const peakInventoryValueDelta = physicalEvidenceComplete
    ? valueOr(comparison.physicalPeakInventoryValueDelta, comparison.peakInventoryValueDelta)
    : null;
  const deltas = [
    Number(valueOr(averageInventoryValueDelta, 0)),
    Number(valueOr(peakInventoryValueDelta, 0)),
    Number(valueOr(comparison.redWeekDelta, 0)),
    Number(valueOr(comparison.replenishmentOrderCountDelta, 0)),
    Number(valueOr(comparison.replenishmentQuantityDelta, 0)),
  ];
  const hasPreview = state.baselineBufferTrend?.caseId && trend.caseId !== state.baselineBufferTrend.caseId;
  const note = !hasPreview
    ? "尚未运行预览，变化按 0 显示"
    : !physicalEvidenceComplete
      ? "物理库存证据缺失；其他变化仍可用"
      : deltas.every(value => value === 0)
        ? "预览与基准一致"
        : "预览方案 - 基准方案";
  byId("buffer-comparison-strip").innerHTML = [
    ["平均库存金额变化", metricOrEvidenceMissing(averageInventoryValueDelta, money)],
    ["峰值库存金额变化", metricOrEvidenceMissing(peakInventoryValueDelta, money)],
    ["红区周变化", number(valueOr(comparison.redWeekDelta, 0))],
    ["补货订单变化", number(valueOr(comparison.replenishmentOrderCountDelta, 0))],
    ["补货数量变化", number(valueOr(comparison.replenishmentQuantityDelta, 0))],
  ].map(([label, value]) => `<div><span>${label}</span><strong>${value}</strong><small>${note}</small></div>`).join("");
}

function renderBufferHeatmap(trend) {
  const weeks = [...new Set(trend.weeklyCells.map(item => item.week))].sort((left, right) => left - right);
  const rows = trend.skuDetails;
  byId("buffer-trend-heatmap").innerHTML = rows.length
    ? `
      <table class="buffer-heatmap-table">
        <thead><tr><th>SKU</th>${weeks.map(week => `<th>第 ${number(week)} 周</th>`).join("")}</tr></thead>
        <tbody>
          ${rows.map(detail => `
            <tr>
              <th><button class="link-button" type="button" data-buffer-sku="${escapeHtml(detail.sku)}"><strong>${escapeHtml(detail.sku)}</strong><small>${escapeHtml(detail.name)}</small></button></th>
              ${weeks.map(week => {
                const cell = trend.weeklyCells.find(item => item.sku === detail.sku && item.week === week);
                return cell
                  ? `<td><button class="${bufferCellClass(cell.status)}" type="button" data-buffer-sku="${escapeHtml(cell.sku)}"><strong>${escapeHtml(statusLabel(cell.status))}</strong><span>${metricOrEvidenceMissing(cell.inventoryValue, money)}</span></button></td>`
                  : `<td class="empty-cell">-</td>`;
              }).join("")}
            </tr>
          `).join("")}
        </tbody>
      </table>`
    : `<div class="table-empty"><strong>没有缓冲热力格数据</strong></div>`;
}

function renderBufferFamilySummary(trend) {
  byId("buffer-family-summary-body").innerHTML = trend.familySummaries.length
    ? trend.familySummaries.map(item => row([
      escapeHtml(item.family),
      metricOrEvidenceMissing(item.averageInventoryValue, money),
      number(item.redWeekCount),
      number(item.yellowWeekCount),
      number(item.overGreenWeekCount),
      number(item.replenishmentOrderCount),
    ])).join("")
    : emptyRow("没有产品族汇总数据", 6);
}

function whiteBoxTraceRecords(previewCase) {
  if (!previewCase) return [];
  const caseId = valueOr(previewCase.caseId, "baseline");
  const occurrences = new Map();
  return valueOr(previewCase.plan?.traces, []).map(trace => {
    const sku = valueOr(trace.sku, "未指定 SKU");
    const week = Number(trace.week);
    const occurrenceGroup = JSON.stringify([sku, week]);
    const occurrence = valueOr(occurrences.get(occurrenceGroup), 0) + 1;
    occurrences.set(occurrenceGroup, occurrence);
    return {
      key: `${encodeURIComponent(caseId)}|${encodeURIComponent(sku)}|${week}|${occurrence}`,
      caseId,
      caseName: valueOr(previewCase.name, caseId),
      sku,
      week,
      occurrence,
      explanation: valueOr(trace.explanation, "后端计算记录"),
      trace,
    };
  });
}

function selectedWhiteBoxTraceRecord(previewCase, detail) {
  const visibleWeeks = new Set(valueOr(detail?.series, []).map(item => Number(item.week)));
  return valueOr(whiteBoxTraceRecords(previewCase).find(record =>
    record.sku === detail?.sku && (visibleWeeks.size === 0 || visibleWeeks.has(record.week))), null);
}

function findWhiteBoxTraceRecord(result, recordKey) {
  return [result?.baseline, result?.scenario]
    .flatMap(whiteBoxTraceRecords)
    .find(record => record.key === recordKey);
}

function focusWhiteBoxTraceRecord(recordKey) {
  const record = findWhiteBoxTraceRecord(state.preview, recordKey);
  if (!record) return false;
  state.selectedWhiteBoxTraceKey = record.key;
  navigateWorkspace("validation", "white-box-trace", false);
  renderPreviewTrace(state.preview);
  const host = byId("trace-list");
  const item = document.createElement("div");
  item.className = "diagnostic-item white-box-trace-record is-selected";
  item.dataset.whiteBoxTraceKey = record.key;
  item.setAttribute("data-white-box-trace-key", record.key);
  item.setAttribute("tabindex", "-1");
  const title = document.createElement("strong");
  title.textContent = `${caseLabel(record.caseName)} · ${record.sku} / 第 ${record.week} 周`;
  const explanation = document.createElement("span");
  explanation.textContent = businessEvidenceLabel(record.explanation);
  const identity = document.createElement("small");
  identity.textContent = `实际记录：${record.key}`;
  item.appendChild(title);
  item.appendChild(explanation);
  item.appendChild(identity);
  host.appendChild(item);
  item.focus();
  item.scrollIntoView({ block: "center" });
  return true;
}

function renderBufferSkuDetail(detail, previewCase = null) {
  if (!detail) {
    byId("buffer-sku-metadata").innerHTML = "";
    byId("buffer-trend-body").innerHTML = emptyRow("没有缓冲趋势数据", 10);
    byId("buffer-replenishment-body").innerHTML = emptyRow("没有补货订单", 4);
    byId("buffer-trace-list").innerHTML = "";
    byId("single-sku-activity-body").innerHTML = emptyRow("没有活动数据", 10);
    byId("single-sku-attribute-body").innerHTML = emptyRow("没有属性数据", 4);
    byId("single-sku-sizing-body").innerHTML = emptyRow("没有缓冲定容数据", 4);
    byId("single-sku-bom-body").innerHTML = emptyRow("没有 BOM 数据", 9);
    byId("single-sku-order-body").innerHTML = emptyRow("没有订单明细", 14);
    return;
  }

  byId("buffer-sku-metadata").innerHTML = [
    ["SKU", detail.sku],
    ["产品族", detail.family],
    ["ADU", number(detail.adu)],
    ["DLT", `${number(detail.decoupledLeadTimeDays)} 天`],
    ["MOQ", number(detail.minimumOrderQuantity)],
    ["订货周期", `${number(detail.orderCycleDays)} 天`],
    ["单位成本", money(detail.unitCost)],
    ["缓冲区", `红 ${number(detail.zone.topOfRed)} / 黄 ${number(detail.zone.topOfYellow)} / 绿 ${number(detail.zone.topOfGreen)}`],
  ].map(([label, value]) => `<div><span>${label}</span><strong>${escapeHtml(value)}</strong></div>`).join("");

  renderSingleSkuSimulation(detail);

  byId("buffer-trend-body").innerHTML = detail.series.length
    ? detail.series.map(item => row([
      escapeHtml(item.periodStartDate),
      `第 ${number(item.week)} 周`,
      number(item.timePhasedAdu),
      number(item.startNetFlow),
      number(item.demand),
      `${number(item.endNetFlowBeforeReplenishment)} / ${number(item.endNetFlowAfterReplenishment)}`,
      `${number(item.topOfRed)} / ${number(item.topOfYellow)} / ${number(item.topOfGreen)}`,
      metricOrEvidenceMissing(item.inventoryValue, money),
      item.isReplenishment ? (item.isPrebuild ? "提前建库订单" : "订货周期补货订单") : "-",
      `<span class="${statusClass(item.status)}">${escapeHtml(statusLabel(item.status))}</span>`,
    ])).join("")
    : emptyRow("没有缓冲趋势数据", 10);

  byId("buffer-replenishment-body").innerHTML = detail.replenishmentOrders.length
    ? detail.replenishmentOrders.map(item => row([
      `第 ${number(item.week)} 周`,
      number(item.quantity),
      money(item.value),
      escapeHtml(triggerLabel(item.trigger)),
    ])).join("")
    : emptyRow("没有补货订单", 4);

  const whiteBoxRecord = selectedWhiteBoxTraceRecord(previewCase, detail);
  const whiteBoxLink = whiteBoxRecord
    ? `
      <div class="diagnostic-item white-box-record-link">
        <strong>白盒记录</strong>
        <span><a href="#trace-panel" data-white-box-record="${escapeHtml(whiteBoxRecord.key)}">查看对应计算记录</a></span>
        <small>${escapeHtml(whiteBoxRecord.caseId)} · ${escapeHtml(whiteBoxRecord.sku)} · 第 ${whiteBoxRecord.week} 周 · 第 ${whiteBoxRecord.occurrence} 条</small>
      </div>`
    : `
      <div class="diagnostic-item white-box-record-link is-unavailable">
        <strong>白盒记录</strong>
        <span>无可定位记录</span>
        <small>所选方案、物料与周范围没有对应的后端 plan trace</small>
      </div>`;
  byId("buffer-trace-list").innerHTML = whiteBoxLink + (detail.traces.length
    ? detail.traces.slice(0, 8).map(item => `
      <div class="diagnostic-item">
        <strong>第 ${item.week} 周</strong>
        <span>${escapeHtml(businessEvidenceLabel(item.explanation))}</span>
      </div>
    `).join("")
    : `<div class="table-empty"><strong>没有计算追踪</strong></div>`);
}

function renderSingleSkuSimulation(detail) {
  byId("single-sku-activity-body").innerHTML = detail.activities?.length
    ? detail.activities.map(item => row([
      `第 ${item.week} 周`,
      escapeHtml(item.periodStartDate),
      escapeHtml(item.activityType),
      number(item.quantity),
      escapeHtml(item.direction),
      escapeHtml(item.source),
      escapeHtml(item.triggerReason),
      number(item.resultingNetFlow),
      `<span class="${statusClass(item.bufferStatus)}">${statusLabel(item.bufferStatus)}</span>`,
      escapeHtml(item.relatedObject),
    ])).join("")
    : emptyRow("没有活动数据", 10);

  byId("single-sku-attribute-body").innerHTML = detail.attributes?.length
    ? detail.attributes.map(item => row([
      escapeHtml(item.group),
      escapeHtml(item.name),
      escapeHtml(businessEvidenceLabel(item.value)),
      escapeHtml(businessEvidenceLabel(item.explanation)),
    ])).join("")
    : emptyRow("没有属性数据", 4);

  byId("single-sku-sizing-body").innerHTML = detail.bufferSizing?.length
    ? detail.bufferSizing.map(item => row([
      escapeHtml(item.component),
      escapeHtml(businessEvidenceLabel(item.formula)),
      number(item.value),
      escapeHtml(businessEvidenceLabel(item.explanation)),
    ])).join("")
    : emptyRow("没有缓冲定容数据", 4);

  byId("single-sku-bom-body").innerHTML = detail.bom?.length
    ? detail.bom.map(item => row([
      escapeHtml(item.componentSku),
      escapeHtml(item.componentName),
      `第 ${number(item.level)} 层`,
      escapeHtml(item.componentType),
      number(item.quantityPer),
      escapeHtml(item.supplier),
      `${number(item.leadTimeDays)} 天`,
      `<span class="${statusClass(item.bufferStatus)}">${statusLabel(item.bufferStatus)}</span>`,
      escapeHtml(item.constraintNote),
    ])).join("")
    : emptyRow("没有 BOM 数据", 9);

  byId("single-sku-order-body").innerHTML = detail.orderDetails?.length
    ? detail.orderDetails.map(item => row([
      escapeHtml(item.orderId),
      escapeHtml(item.orderType),
      `第 ${item.week} 周`,
      `第 ${item.releaseWeek} 周`,
      `第 ${item.dueWeek} 周`,
      number(item.quantity),
      money(item.value),
      escapeHtml(item.status),
      escapeHtml(item.sourceRule),
      escapeHtml(item.supplier),
      escapeHtml(item.resource),
      number(item.capacityLoad),
      number(item.supplyGap),
      escapeHtml(item.trace),
    ])).join("")
    : emptyRow("没有订单明细", 14);
}

function renderPreviewBufferTrend(result) {
  state.baselineBufferTrend = result.baseline.bufferTrend;
  state.bufferTrend = result.scenario.bufferTrend;
  state.selectedBufferSku = valueOr(state.bufferTrend?.selectedSku, state.selectedBufferSku);
  const selectedWeeks = valueOr(state.bufferTrend?.series, [])
    .map(item => Number(item.week))
    .filter(Number.isFinite);
  state.futureInventorySelection = {
    caseId: valueOr(result.scenario.caseId, "scenario"),
    sku: state.selectedBufferSku,
    weekFrom: selectedWeeks.length ? Math.min(...selectedWeeks) : 1,
    weekThrough: selectedWeeks.length ? Math.max(...selectedWeeks) : null,
  };
  renderBufferTrendWorkspace(state.bufferTrend);
}

function renderPreviewRccp(result) {
  renderProductRccp(result.scenario.rccp, "预览方案");
  state.constraints = result.scenario.constraints;
  renderConstraintWorkspace(state.constraints);
}

function renderPreviewSupply(result) {
  state.supplierCollaboration = result.scenario.supplierCollaboration;
  state.selectedSupplier = valueOr(state.supplierCollaboration?.selectedSupplier, state.selectedSupplier);
  renderSupplierCollaborationWorkspace(state.supplierCollaboration);
}

function renderPreviewBudget(result) {
  byId("budget-comparison-body").innerHTML = result.scenario.budget.length
    ? result.scenario.budget.slice(0, 60).map(item => {
      const evidenceComplete = isFiniteChartValue(item.projectedInventoryValue)
        && isFiniteChartValue(item.budgetInventoryVariance);
      const status = evidenceComplete && item.budgetInventoryVariance > 0 ? "Yellow" : "Green";
      const variance = evidenceComplete
        ? `<span class="${statusClass(status)}">${money(item.budgetInventoryVariance)}</span>`
        : `<span class="status-chip is-paused">证据缺失</span>`;
      return row([
        item.family,
        `第 ${item.week} 周`,
        money(item.budgetInventoryValue),
        money(item.lastYearInventoryValue),
        metricOrEvidenceMissing(item.projectedInventoryValue, money),
        variance,
      ]);
    }).join("")
    : emptyRow("没有预算对照数据", 6);
}

function renderPreviewTrace(result) {
  const audit = result.trace.map((item, index) => `
    <div class="diagnostic-item ${item.severity === "Warning" ? "is-error" : ""}">
      <strong>${traceStageLabel(item.stage)}</strong>
      <span>${item.message}</span>
    </div>
  `).join("");
  const engineTrace = result.scenario.plan.traces.slice(0, 12).map(item => `
    <div class="diagnostic-item">
      <strong>${item.sku} / 第 ${item.week} 周</strong>
      <span>${item.explanation}</span>
    </div>
  `).join("");
  byId("trace-list").innerHTML = audit + engineTrace;
}

function renderPreviewVariance(result) {
  renderExceptionWorkspace(state.exceptions);
}

function renderPreviewResult(result) {
  state.preview = result;
  state.productFamilyDashboard = result.scenario.productFamilyDashboard;
  state.selectedProductFamily = valueOr(state.productFamilyDashboard?.selectedFamily, state.selectedProductFamily);
  renderPreviewKpis(result);
  renderProductFamilyDashboard(state.productFamilyDashboard);
  renderPreviewComparison(result);
  renderPreviewBufferTrend(result);
  renderPreviewRccp(result);
  renderPreviewSupply(result);
  renderPreviewBudget(result);
  renderPreviewVariance(result);
  renderPreviewTrace(result);
  const feasibility = result.feasibility;
  const adoption = evaluateAdoption({ feasibility });
  byId("preview-status").className = statusClass(adoption.status);
  byId("preview-status").textContent = `可行性：${adoption.label}`;
  byId("preview-candidate-chip").className = feasibility.status === "Blocked" ? "status-chip is-invalid" : "status-chip neutral";
  byId("preview-candidate-chip").textContent = feasibility.status === "Blocked"
    ? "阻断候选：可保存留痕，但不可选定；可创建协调事项或修订后重算"
    : "预览结果：保存仅用于审计；仅在冻结比较中保存的方案可人工选定";
  byId("route-status").className = "status-chip is-valid";
  byId("route-status").textContent = "预览结果已生成";
  showScenarioSavePanel(result);
}

function showScenarioSavePanel(result) {
  saveControls.panel.hidden = false;
  const physicalInventoryComplete = hasCompletePhysicalInventoryEvidence(result.baseline)
    && hasCompletePhysicalInventoryEvidence(result.scenario)
    && isFiniteChartValue(result.baseline?.metrics?.averageInventoryValue)
    && isFiniteChartValue(result.scenario?.metrics?.averageInventoryValue);
  const template = state.data?.scenarioTemplates?.find(item => item.templateId === result.request.templateId);
  const templateName = valueOr(template?.name, "场景预览");
  const now = new Date().toLocaleString("zh-CN", { hour12: false });
  if (!saveControls.name.value) {
    saveControls.name.value = `${templateName} ${now}`;
  }
  saveControls.button.disabled = !physicalInventoryComplete;
  saveControls.status.className = physicalInventoryComplete ? "status-chip is-paused" : "status-chip is-invalid";
  saveControls.status.textContent = physicalInventoryComplete
    ? "预览结果，未保存"
    : "物理库存证据缺失，禁止保存";
  byId("preview-persistence-chip").className = physicalInventoryComplete
    ? "status-chip is-paused"
    : "status-chip is-invalid";
  byId("preview-persistence-chip").textContent = physicalInventoryComplete
    ? "预览结果，未保存"
    : "物理库存证据缺失，禁止保存";
}

function renderSavedScenarioRuns(runs) {
  state.savedScenarioRuns = valueOr(runs, []);
  saveControls.listBody.innerHTML = state.savedScenarioRuns.length
    ? state.savedScenarioRuns.map(item => {
      const hasCompleteFrozenLineage = Boolean(item.baselineSnapshotId && item.externalScenarioId && item.responseId);
      const isReviewable = item.feasibilityStatus === "Adoptable" || item.feasibilityStatus === "Reconcile";
      const action = item.candidateStatus === "Selected" && hasCompleteFrozenLineage && isReviewable
        ? `<button class="button secondary compact-button" type="button" data-enter-ddom-run-id="${escapeHtml(item.runId)}">进入 DDOM 配置决策</button>`
        : item.feasibilityStatus === "Blocked"
          ? `<button class="button secondary compact-button" type="button" data-revise-blocked-run-id="${escapeHtml(item.runId)}">创建协调事项并修订方案</button>`
          : item.candidateStatus !== "Candidate"
            ? `<span class="muted-note">${escapeHtml(statusLabel(item.candidateStatus || "Candidate"))}，不可选定</span>`
          : isReviewable && hasCompleteFrozenLineage
            ? `<button class="button primary compact-button" type="button" data-select-ddom-run-id="${escapeHtml(item.runId)}">选定为 DDOM 候选</button>`
            : isReviewable
              ? `<span class="muted-note">缺少冻结比较血缘，不可选定</span>`
              : `<span class="muted-note">后端可行性缺失/需重新计算</span>`;
      return `
      <tr class="saved-run-row ${item.runId === state.selectedScenarioRunId ? "is-selected" : ""}">
        <td><button class="link-button" type="button" data-scenario-run-id="${escapeHtml(item.runId)}"><strong>${escapeHtml(item.runNumber)}</strong><small>${escapeHtml(new Date(item.createdAtUtc).toLocaleString("zh-CN", { hour12: false }))}</small></button></td>
        <td>${escapeHtml(item.name)}</td>
        <td>${escapeHtml(item.createdBy)}</td>
        <td><span class="${statusClass(item.feasibilityStatus === "Blocked" ? "Red" : item.feasibilityStatus === "Reconcile" ? "Yellow" : item.feasibilityStatus === "Adoptable" ? "Green" : "Red")}">${escapeHtml(item.feasibilityStatus === "Adoptable" || item.feasibilityStatus === "Reconcile" || item.feasibilityStatus === "Blocked" ? statusLabel(item.feasibilityStatus) : "后端可行性缺失")}</span></td>
        <td><span class="status-chip is-valid">已保存 ${escapeHtml(item.runNumber)}</span></td>
        <td><span class="status-chip ${item.candidateStatus === "Selected" ? "is-valid" : "neutral"}">${escapeHtml(statusLabel(item.candidateStatus || "Candidate"))}</span></td>
        <td>${action}</td>
        <td>${percent(item.serviceLevelPercent)}</td>
        <td>${percent(item.peakLoadPercent)}</td>
        <td>${number(item.supplyGap)}</td>
      </tr>`;
    }).join("")
    : emptyRow("暂无已保存场景。运行预览后可以保存为审计记录。", 10);
  configureCoordinationLineageSelectors();
}

function renderScenarioLineage(changes, coordinationItems) {
  const changeLinks = valueOr(changes, []).map(item => `
    <button class="link-button" type="button" data-lineage-master-change-id="${escapeHtml(item.changeId)}">
      <strong>${escapeHtml(item.changeNumber)}</strong><small>${escapeHtml(masterSettingDisplayValue(item.changeId, "target", item.target))}</small>
    </button>`).join("");
  const coordinationLinks = valueOr(coordinationItems, []).map(item => `
    <button class="link-button" type="button" data-lineage-coordination-item-id="${escapeHtml(item.itemId)}">
      <strong>${escapeHtml(item.itemNumber)}</strong><small>${escapeHtml(item.title)}</small>
    </button>`).join("");
  saveControls.lineageList.innerHTML = `
    <div class="diagnostic-item">
      <strong>04 DDOM 配置决策</strong>
      <span>${changeLinks || "尚无由本场景生成的主设置变更"}</span>
      <small>${number(valueOr(changes, []).length)} 个关联变更</small>
    </div>
    <div class="diagnostic-item">
      <strong>05 行动和决策</strong>
      <span>${coordinationLinks || "尚无关联行动事项"}</span>
      <small>${number(valueOr(coordinationItems, []).length)} 个关联行动</small>
    </div>`;
}

function renderScenarioAudit(detail, events, changes = [], coordinationItems = []) {
  const summary = detail?.summary;
  saveControls.title.textContent = summary ? `${summary.runNumber} · ${summary.name}` : "尚未选择";
  saveControls.detailStatus.className = summary ? "status-chip is-valid" : "status-chip neutral";
  saveControls.detailStatus.textContent = summary ? "已保存，未提交审批" : "未选择";
  saveControls.summary.innerHTML = summary
    ? [
      ["场景编号", summary.runNumber],
      ["场景名称", summary.name],
      ["冻结基线", summary.baselineSnapshotId || "未关联"],
      ["外部场景", summary.externalScenarioId || "未关联"],
      ["响应方案", summary.responseId || "未关联"],
      ["创建人", summary.createdBy],
    ].map(([label, value]) => `<div><span>${escapeHtml(label)}</span><strong>${escapeHtml(value)}</strong></div>`).join("")
    : `<div class="table-empty"><strong>选择一条已保存场景后查看详情</strong></div>`;
  renderScenarioLineage(changes, coordinationItems);
  saveControls.auditList.innerHTML = events.length
    ? events.map(item => `
      <div class="diagnostic-item">
        <strong>${item.sequence}. ${traceStageLabel(item.stage)} / ${escapeHtml(auditEventLabel(item.eventType))}</strong>
        <span>${escapeHtml(businessEvidenceLabel(item.message))}</span>
      </div>
    `).join("")
    : `<div class="table-empty"><strong>选择一条已保存场景后查看审计链</strong></div>`;
}

async function loadSavedScenarioRuns(selectedRunId) {
  const response = await fetch("/api/scenario-runs?limit=50", {
    headers: { Accept: "application/json" },
  });
  if (!response.ok) {
    throw new Error(`已保存场景列表接口失败：${response.status}`);
  }
  const runs = await response.json();
  const requestedRunId = selectedRunId || state.selectedScenarioRunId;
  state.selectedScenarioRunId = runs.some(item => item.runId === requestedRunId) ? requestedRunId : valueOr(runs[0]?.runId, null);
  renderSavedScenarioRuns(runs);
  if (state.selectedScenarioRunId) {
    await loadScenarioRunDetail(state.selectedScenarioRunId);
  } else {
    renderScenarioAudit(null, []);
  }
}

async function loadScenarioRunDetail(runId) {
  const requestGeneration = ++state.scenarioDetailRequestGeneration;
  state.selectedScenarioRunId = runId;
  renderSavedScenarioRuns(state.savedScenarioRuns);
  const [detailResponse, auditResponse, changesResponse, coordinationResponse] = await Promise.all([
    fetch(`/api/scenario-runs/${encodeURIComponent(runId)}`, { headers: { Accept: "application/json" } }),
    fetch(`/api/scenario-runs/${encodeURIComponent(runId)}/audit`, { headers: { Accept: "application/json" } }),
    fetch(`/api/master-settings/changes?limit=50&sourceScenarioRunId=${encodeURIComponent(runId)}`, { headers: { Accept: "application/json" } }),
    fetch(`/api/coordination-items?limit=50&relatedScenarioRunId=${encodeURIComponent(runId)}`, { headers: { Accept: "application/json" } }),
  ]);
  if (requestGeneration !== state.scenarioDetailRequestGeneration || runId !== state.selectedScenarioRunId) return;
  if (detailResponse.status === 404) {
    state.selectedScenarioRunId = null;
    renderSavedScenarioRuns(state.savedScenarioRuns);
    renderScenarioAudit(null, []);
    return;
  }
  if (!detailResponse.ok) {
    throw new Error(`场景详情接口失败：${detailResponse.status}`);
  }
  if (!auditResponse.ok) {
    throw new Error(`场景审计链接口失败：${auditResponse.status}`);
  }
  if (!changesResponse.ok || !coordinationResponse.ok) {
    throw new Error("场景关联记录查询失败。");
  }
  renderScenarioAudit(
    await detailResponse.json(),
    await auditResponse.json(),
    await changesResponse.json(),
    await coordinationResponse.json());
}

async function saveScenarioRun() {
  if (!state.preview) {
    saveControls.status.className = "status-chip is-warning";
    saveControls.status.textContent = "请先运行预览";
    return;
  }
  if (!hasCompletePhysicalInventoryEvidence(state.preview.baseline)
    || !hasCompletePhysicalInventoryEvidence(state.preview.scenario)
    || !isFiniteChartValue(state.preview.baseline?.metrics?.averageInventoryValue)
    || !isFiniteChartValue(state.preview.scenario?.metrics?.averageInventoryValue)) {
    saveControls.status.className = "status-chip is-invalid";
    saveControls.status.textContent = "物理库存证据缺失，禁止保存";
    return;
  }

  saveControls.status.className = "status-chip is-warning";
  saveControls.status.textContent = "正在保存";
  const payload = {
    name: saveControls.name.value || "未命名场景",
    description: saveControls.description.value || null,
    createdBy: saveControls.createdBy.value || "计划员",
    previewRequest: state.preview.request,
  };

  const response = await fetch("/api/scenario-runs", {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify(payload),
  });
  if (!response.ok) {
    throw new Error(`保存场景接口失败：${response.status}`);
  }

  const saved = await response.json();
  const savedSummary = saved.summary;
  const hasCompleteFrozenLineage = Boolean(savedSummary?.baselineSnapshotId && savedSummary?.externalScenarioId && savedSummary?.responseId);
  saveControls.status.className = "status-chip is-valid";
  saveControls.status.textContent = `已保存，未提交审批：${saved.runNumber}`;
  byId("preview-persistence-chip").className = "status-chip is-valid";
  byId("preview-persistence-chip").textContent = `已保存，未提交审批：${saved.runNumber}`;
  byId("preview-candidate-chip").className = saved.summary.feasibilityStatus === "Blocked" ? "status-chip is-invalid" : "status-chip neutral";
  byId("preview-candidate-chip").textContent = saved.summary.feasibilityStatus === "Blocked"
    ? "阻断候选：已保存留痕但不可选定；请创建协调事项或修订后重算"
    : hasCompleteFrozenLineage
      ? "候选：未选定（需人工点击选定）"
      : "已保存为审计记录；仅冻结比较方案可人工选定";
  await loadSavedScenarioRuns(saved.runId);
}

function ddomPackageGate(summary) {
  if (!summary) return "请选择变更包。";
  if (summary.status === "Draft") return "门禁：草稿包需先人工提交评审。";
  if (summary.status === "Submitted" && summary.validationStatus === "NotRun") return "门禁：已提交后需先运行白盒验证。";
  if (summary.status === "Submitted" && summary.validationStatus !== "Passed") return "门禁：最新白盒验证未通过，不能标记已评审。";
  if (summary.status === "Submitted") return "门禁已满足：可由人工标记已评审。";
  if (summary.status === "Reviewed") return `门禁：仅配置审批人 ${summary.approver || "未指定"} 可以批准。`;
  if (summary.status === "Approved") return "门禁：需保留匹配的白盒验证与完整生效、复查、回滚信息。";
  return "该包已到达当前治理状态；不会自动推进下一步。";
}

function packageActionEnabled(summary, action) {
  if (!summary || state.ddomActionInFlight) return false;
  return ({ submit: summary.status === "Draft", validate: summary.status === "Submitted" && summary.validationStatus === "NotRun", review: summary.status === "Submitted" && summary.validationStatus === "Passed" && summary.feasibilityStatus !== "Blocked", approve: summary.status === "Reviewed", effective: summary.status === "Approved", expire: summary.status === "Effective" })[action] || false;
}

function renderDdomPackageActions(summary) {
  const bindings = [["submit-ddom-package", "submit"], ["validate-ddom-package", "validate"], ["review-ddom-package", "review"], ["approve-ddom-package", "approve"], ["effective-ddom-package", "effective"], ["expire-ddom-package", "expire"]];
  const reason = ddomPackageGate(summary);
  bindings.forEach(([id, action]) => {
    const button = byId(id);
    button.disabled = !packageActionEnabled(summary, action);
    button.title = state.ddomActionInFlight ? "当前操作正在处理，请等待返回。" : button.disabled ? reason : "此操作只执行当前一步，不会自动推进后续状态。";
  });
  byId("ddom-package-gate").textContent = state.ddomActionInFlight ? "正在处理当前人工步骤，请勿重复提交。" : reason;
}

function renderDdomPackageList(packages) {
  byId("ddom-package-list").innerHTML = packages.length
    ? packages.map(item => `<button class="master-setting-card" type="button" data-ddom-package-id="${escapeHtml(item.packageId)}"><strong>${escapeHtml(item.packageNumber)} · ${escapeHtml(item.name)}</strong><span>${escapeHtml(statusLabel(item.status))} / ${escapeHtml(statusLabel(item.validationStatus))} / ${escapeHtml(statusLabel(item.feasibilityStatus))}</span><small>${escapeHtml(item.sourceScenarioRunId)} · ${escapeHtml(item.createdBy)} · ${escapeHtml(item.createdAtUtc)}</small></button>`).join("")
    : `<div class="table-empty"><strong>尚无 DDOM 变更包</strong><span>先在第 03 阶段保存并人工选定可评审候选。</span></div>`;
}

function ddomParameterSummary(parameters) {
  if (!parameters) return "无响应参数调整";
  const groups = [
    ["提前建库", parameters.prebuildCampaigns],
    ["能力临调", parameters.capacityAdjustments],
    ["SKU 策略", parameters.skuPolicyOverrides],
    ["供应能力", parameters.supplierCapacityLimits],
    ["时间保护", parameters.timeBufferAdjustments],
  ].filter(([, items]) => Array.isArray(items) && items.length > 0);
  return groups.length ? groups.map(([label, items]) => `${label} ${items.length} 项`).join("；") : "无响应参数调整";
}

function ddomProposalReasonLabel(proposal) {
  const risk = ({ Red: "高", Yellow: "中", Green: "低" })[proposal?.riskLevel] || "待复核";
  const trigger = businessEvidenceLabel(proposal?.trigger || "冻结基线与已选场景白盒重算");
  return `${trigger}；风险等级：${risk}`;
}

function renderDdomPackageDetail(detail, auditEvents = []) {
  const summary = detail?.summary;
  state.currentDdomPackageDetail = detail;
  byId("ddom-package-title").textContent = summary ? `${summary.packageNumber} · ${summary.name}` : "尚未选择";
  byId("ddom-package-status").className = summary ? statusClass(summary.status === "Effective" ? "Green" : summary.validationStatus === "Failed" ? "Red" : "Yellow") : "status-chip neutral";
  byId("ddom-package-status").textContent = summary ? `${statusLabel(summary.status)} / ${statusLabel(summary.validationStatus)}` : "未加载";
  byId("ddom-package-lineage").innerHTML = summary ? [
    ["来源基线", summary.sourceBaselineId], ["场景运行", summary.sourceScenarioRunId], ["外部场景", summary.externalScenarioId], ["响应方案", summary.responseId],
    ["可行性", statusLabel(summary.feasibilityStatus)], ["负责人 / 审批人", `${summary.owner} / ${summary.approver}`], ["创建人 / 日期", `${summary.createdBy} / ${summary.createdAtUtc}`],
    ["响应参数摘要", ddomParameterSummary(detail.finalParameters)],
  ].map(([label, value]) => `<div><dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value)}</dd></div>`).join("") : "";
  byId("ddom-package-line-body").innerHTML = summary ? detail.lines.map(line => row([
    number(line.sequence), escapeHtml(masterSettingTypeLabel(line.proposal.settingType)), escapeHtml(line.proposal.target), escapeHtml(line.proposal.currentValue), escapeHtml(line.proposal.proposedValue),
    escapeHtml(ddomProposalReasonLabel(line.proposal)),
  ])).join("") : emptyRow("请选择变更包", 6);
  const latestValidation = detail && detail.latestValidation;
  const validationCoordinationItems = Array.isArray(latestValidation?.coordinationItems)
    ? latestValidation.coordinationItems.filter(Boolean)
    : [];
  const validationCoordinationHtml = validationCoordinationItems.length
    ? `<span>待协调事项：${escapeHtml(validationCoordinationItems.map(businessEvidenceLabel).join("；"))}</span>`
    : "";
  byId("ddom-package-validation").innerHTML = latestValidation
    ? `<div class="diagnostic-item ${latestValidation.validationStatus === "Failed" ? "is-error" : ""}"><strong>最新白盒验证：${escapeHtml(statusLabel(latestValidation.validationStatus))}</strong><span>可行性：${escapeHtml(statusLabel(latestValidation.feasibilityStatus))}；失败原因：${escapeHtml((latestValidation.failureReasons || []).join("；") || "无")}</span>${validationCoordinationHtml}<small>验证人：${escapeHtml(latestValidation.validatedBy)} · ${escapeHtml(latestValidation.validatedAtUtc)}</small>${latestValidation.validationStatus === "Failed" ? `<button class="button secondary compact-button" type="button" data-coordinate-ddom-package-id="${escapeHtml(summary.packageId)}">创建协调事项</button>` : ""}</div>`
    : `<div class="diagnostic-item"><strong>最新验证</strong><span>尚未运行白盒验证。</span></div>`;
  byId("ddom-package-audit").innerHTML = auditEvents.length ? auditEvents.map(item => `<div class="diagnostic-item ${item.severity === "Warning" ? "is-error" : ""}"><strong>${number(item.sequence)}. ${escapeHtml(auditEventLabel(item.eventType))}</strong><span>${escapeHtml(businessEvidenceLabel(item.message))}</span><small>${escapeHtml(traceStageLabel(item.stage))} · ${escapeHtml(item.createdAtUtc)}</small></div>`).join("") : "";
  renderDdomPackageActions(summary);
}

async function loadDdomPackages(selectedPackageId) {
  const response = await fetch("/api/ddom-change-packages", { headers: { Accept: "application/json" } });
  if (!response.ok) throw new Error(`DDOM 变更包接口失败：${response.status}`);
  const packages = await response.json();
  state.ddomPackages = Array.isArray(packages) ? packages : [];
  const requestedPackageId = selectedPackageId || state.selectedDdomPackageId;
  state.selectedDdomPackageId = state.ddomPackages.some(item => item.packageId === requestedPackageId) ? requestedPackageId : state.ddomPackages[0]?.packageId || null;
  renderDdomPackageList(state.ddomPackages);
  configureCoordinationLineageSelectors();
  if (state.selectedDdomPackageId) await loadDdomPackageDetail(state.selectedDdomPackageId);
  else renderDdomPackageDetail(null);
}

async function loadDdomPackageDetail(packageId) {
  const requestGeneration = ++state.ddomDetailRequestGeneration;
  state.selectedDdomPackageId = packageId;
  const [detailResponse, auditResponse] = await Promise.all([
    fetch(`/api/ddom-change-packages/${encodeURIComponent(packageId)}`, { headers: { Accept: "application/json" } }),
    fetch(`/api/ddom-change-packages/${encodeURIComponent(packageId)}/audit`, { headers: { Accept: "application/json" } }),
  ]);
  if (requestGeneration !== state.ddomDetailRequestGeneration || packageId !== state.selectedDdomPackageId) return;
  if (detailResponse.status === 404 || auditResponse.status === 404) {
    state.selectedDdomPackageId = null;
    renderDdomPackageDetail(null);
    return;
  }
  if (!detailResponse.ok || !auditResponse.ok) throw new Error("DDOM 变更包详情接口失败。");
  renderDdomPackageDetail(await detailResponse.json(), await auditResponse.json());
}

async function selectScenarioForDdom(runId) {
  const response = await fetch(`/api/scenario-runs/${encodeURIComponent(runId)}/selection`, { method: "POST", headers: { "Content-Type": "application/json", Accept: "application/json" }, body: JSON.stringify({ status: "Selected", updatedBy: "DDS&OP 计划员", note: "工作台人工选定为 DDOM 候选" }) });
  const payload = await response.json();
  if (!response.ok) throw new Error(payload.message || `选定 DDOM 候选失败：${response.status}`);
  await loadSavedScenarioRuns(runId);
  await loadDdomPackages();
  byId("preview-candidate-chip").className = "status-chip is-valid";
  byId("preview-candidate-chip").textContent = "候选：已选定为 DDOM 来源";
  byId("ddom-source-run").value = runId;
  navigateWorkspace("ddom-decision-panel", "parameter-decision", false);
}

async function createDdomPackage() {
  if (state.ddomCreateInFlight) return;
  const sourceScenarioRunId = byId("ddom-source-run").value;
  if (!sourceScenarioRunId) throw new Error("请先在第 03 阶段人工选定一个可评审候选。");
  const createButton = byId("create-ddom-package");
  state.ddomCreateInFlight = true;
  createButton.disabled = true;
  createButton.title = "正在创建变更包，请等待返回。";
  try {
    const response = await fetch("/api/ddom-change-packages", { method: "POST", headers: { "Content-Type": "application/json", Accept: "application/json" }, body: JSON.stringify({ sourceScenarioRunId, name: byId("ddom-package-name").value || "DDOM 场景变更包", description: "由已选场景创建", createdBy: "DDS&OP 计划员", governanceContext: governanceDecisionContext(sourceScenarioRunId) }) });
    const payload = await response.json();
    if (!response.ok) throw new Error(payload.message || `创建 DDOM 变更包失败：${response.status}`);
    await loadDdomPackages(payload.packageId);
  } finally {
    state.ddomCreateInFlight = false;
    createButton.disabled = false;
    createButton.title = "创建草稿变更包，不会自动提交或生效。";
  }
}

async function ddomPackageAction(action) {
  if (state.ddomActionInFlight) return;
  const packageId = state.selectedDdomPackageId;
  if (!packageId) throw new Error("请先选择变更包。");
  const endpoint = action === "submit" ? "submit" : action === "validate" ? "validate" : "status";
  const body = action === "submit" || action === "validate"
    ? { updatedBy: action === "validate" ? "白盒验证员" : "DDS&OP 计划员", note: "工作台人工触发当前步骤" }
    : { status: ({ review: "Reviewed", approve: "Approved", effective: "Effective", expire: "Expired" })[action], updatedBy: action === "approve" ? byId("governance-approver").value : "DDS&OP 计划员", note: "工作台人工触发当前步骤" };
  state.ddomActionInFlight = true;
  renderDdomPackageActions(state.currentDdomPackageDetail?.summary);
  try {
    const response = await fetch(`/api/ddom-change-packages/${encodeURIComponent(packageId)}/${endpoint}`, { method: "POST", headers: { "Content-Type": "application/json", Accept: "application/json" }, body: JSON.stringify(body) });
    const payload = await response.json();
    if (!response.ok) throw new Error(payload.message || `DDOM 操作失败：${response.status}`);
    await loadDdomPackages(packageId);
  } finally {
    state.ddomActionInFlight = false;
    renderDdomPackageActions(state.currentDdomPackageDetail?.summary);
  }
}

function renderBufferTrend(trends) {
  renderBufferTrendWorkspace(state.bufferTrend);
}

function heatmapClass(status) {
  const normalized = String(valueOr(status, "Green"));
  return `rccp-heat-cell ${normalized === "Red" ? "is-red" : normalized === "Yellow" ? "is-yellow" : "is-green"}`;
}

function renderRccpDetailChart(detail) {
  const chartRows = valueOr(detail?.weeklyLoad, []);
  byId("rccp-load-chart").innerHTML = chartRows.length
    ? chartRows.map(item => `
      <div class="load-row">
        <div class="load-row-label"><strong>第 ${item.week} 周</strong><span>负荷 ${number(item.requiredCapacity)} / 能力 ${number(item.availableCapacity)}</span></div>
        <div class="load-track"><span class="load-bar ${item.status === "Red" ? "overload" : ""}" style="--load-width:${Math.min(item.loadPercent, 140) / 1.4}%"></span></div>
        <div class="load-value">${percent(item.loadPercent)}</div>
      </div>
    `).join("")
    : `<div class="table-empty"><strong>没有资源负荷数据</strong></div>`;
}

function renderProductRccp(rccp, rccpCaseLabel = "基准方案") {
  if (!rccp) {
    byId("rccp-kpis").innerHTML = "";
    byId("rccp-resource-summary-body").innerHTML = emptyRow("没有 RCCP 数据", 8);
    byId("rccp-heatmap").innerHTML = `<div class="table-empty"><strong>没有 RCCP 热力格数据</strong></div>`;
    byId("rccp-sku-contribution-body").innerHTML = emptyRow("没有 SKU 贡献数据", 7);
    byId("rccp-action-list").innerHTML = "";
    return;
  }

  state.rccp = rccp;
  const firstResource = rccp.resourceSummaries[0]?.resourceCode;
  if (!state.selectedRccpResource || !rccp.resourceSummaries.some(item => item.resourceCode === state.selectedRccpResource)) {
    state.selectedRccpResource = firstResource;
  }

  byId("rccp-case-chip").textContent = caseLabel(rccpCaseLabel);
  byId("rccp-kpis").innerHTML = [
    ["约束资源", number(rccp.resourceSummaries.length), "参与 RCCP 的关键资源"],
    ["红区资源", number(rccp.redResourceCount), "补货释放峰值超过 100%"],
    ["最大释放峰值", percent(rccp.maxPeakLoadPercent), "预计补货订单的资源周度压力"],
    ["最大缺口", number(rccp.maxCapacityGap), "需求负荷 - 可用能力"],
    ["超载周", number(rccp.redWeekCount), "资源 × 周红区数"],
    ["可释放能力", number(rccp.releasableCapacity), "低于 85% 的可用余量"],
  ].map(([label, value, note]) => `<div><span>${label}</span><strong>${value}</strong><small>${note}</small></div>`).join("");

  byId("rccp-resource-summary-body").innerHTML = rccp.resourceSummaries.length
    ? rccp.resourceSummaries.map(item => row([
      `<button class="link-button" type="button" data-rccp-resource="${item.resourceCode}"><strong>${item.resourceName}</strong><br><small>${item.resourceCode}</small></button>`,
      item.resourceType,
      percent(item.averageLoadPercent),
      percent(item.peakLoadPercent),
      number(item.overloadWeeks),
      number(item.maxCapacityGap),
      `<span class="${statusClass(item.status)}">${statusLabel(item.status)}</span>`,
      item.recommendedAction,
    ])).join("")
    : emptyRow("没有资源汇总数据", 8);

  const weeks = [...new Set(rccp.weeklyCells.map(item => item.week))].sort((a, b) => a - b);
  byId("rccp-heatmap").innerHTML = rccp.resourceSummaries.length
    ? `
      <table class="rccp-heatmap-table">
        <thead><tr><th>资源</th>${weeks.map(week => `<th>第 ${week} 周</th>`).join("")}</tr></thead>
        <tbody>
          ${rccp.resourceSummaries.map(resource => `
            <tr>
              <th><button class="link-button" type="button" data-rccp-resource="${resource.resourceCode}">${resource.resourceName}</button></th>
              ${weeks.map(week => {
                const cell = rccp.weeklyCells.find(item => item.resourceCode === resource.resourceCode && item.week === week);
                return cell
                  ? `<td><button class="${heatmapClass(cell.status)}" type="button" data-rccp-resource="${cell.resourceCode}" data-rccp-week="${cell.week}"><strong>${percent(cell.loadPercent)}</strong><span>${number(cell.variance)}</span></button></td>`
                  : `<td class="empty-cell">-</td>`;
              }).join("")}
            </tr>
          `).join("")}
        </tbody>
      </table>`
    : `<div class="table-empty"><strong>没有 RCCP 热力格数据</strong></div>`;

  renderSelectedRccpResource(rccp);
}

function renderSelectedRccpResource(rccp) {
  const detail = valueOr(rccp.resourceDetails.find(item => item.resourceCode === state.selectedRccpResource), rccp.resourceDetails[0]);
  if (!detail) {
    byId("rccp-selected-title").textContent = "选中资源明细";
    renderRccpDetailChart(null);
    byId("rccp-sku-contribution-body").innerHTML = emptyRow("没有 SKU 贡献数据", 7);
    byId("rccp-action-list").innerHTML = "";
    return;
  }

  state.selectedRccpResource = detail.resourceCode;
  byId("rccp-selected-title").textContent = `${detail.resourceName} 明细`;
  renderRccpDetailChart(detail);
  byId("rccp-sku-contribution-body").innerHTML = detail.skuContributions.length
    ? detail.skuContributions.slice(0, 80).map(item => row([
      `<strong>${item.sku}</strong><br><small>${item.skuName}</small>`,
      item.family,
      `第 ${item.week} 周`,
      number(item.orderQuantity),
      number(item.capacityPerUnit),
      number(item.requiredCapacity),
      triggerLabel(item.trigger),
    ])).join("")
    : emptyRow("没有 SKU 贡献数据", 7);
  byId("rccp-action-list").innerHTML = detail.recommendations.length
    ? detail.recommendations.map(item => `
      <div class="diagnostic-item ${item.severity === "Red" ? "is-error" : ""}">
        <strong>${recommendationTypeLabel(item.actionType)}</strong>
        <span>${escapeHtml(businessEvidenceLabel(item.message))}</span>
      </div>
    `).join("")
    : `<div class="table-empty"><strong>没有动作建议</strong></div>`;
}

function renderConstraintWorkspace(constraints) {
  if (!constraints) {
    byId("constraint-capacity-summary-body").innerHTML = emptyRow("没有受限 / 不受限数据", 8);
    byId("constraint-heatmap").innerHTML = `<div class="table-empty"><strong>没有约束缺口热力格数据</strong></div>`;
    byId("constraint-gap-chart").innerHTML = `<div class="table-empty"><strong>没有约束明细数据</strong></div>`;
    byId("constraint-action-list").innerHTML = "";
    byId("constraint-trace-list").innerHTML = "";
    return;
  }

  state.constraints = constraints;
  const firstResource = constraints.capacitySummaries[0]?.resourceCode;
  if (!state.selectedRccpResource || !constraints.capacitySummaries.some(item => item.resourceCode === state.selectedRccpResource)) {
    state.selectedRccpResource = firstResource;
  }

  byId("constraint-capacity-summary-body").innerHTML = constraints.capacitySummaries.length
    ? constraints.capacitySummaries.map(item => row([
      `<button class="link-button" type="button" data-constraint-resource="${item.resourceCode}"><strong>${item.resourceName}</strong><br><small>${item.resourceCode}</small></button>`,
      percent(item.averageLoadPercent),
      percent(item.peakLoadPercent),
      number(item.overloadWeeks),
      number(item.maxGap),
      number(item.totalGap),
      `<span class="${statusClass(item.status)}">${statusLabel(item.status)}</span>`,
      item.recommendedAction,
    ])).join("")
    : emptyRow("没有资源约束汇总数据", 8);

  const weeks = [...new Set(constraints.capacityCells.map(item => item.week))].sort((a, b) => a - b);
  byId("constraint-heatmap").innerHTML = constraints.capacitySummaries.length
    ? `
      <table class="rccp-heatmap-table">
        <thead><tr><th>资源</th>${weeks.map(week => `<th>第 ${week} 周</th>`).join("")}</tr></thead>
        <tbody>
          ${constraints.capacitySummaries.map(resource => `
            <tr>
              <th><button class="link-button" type="button" data-constraint-resource="${resource.resourceCode}">${resource.resourceName}</button></th>
              ${weeks.map(week => {
                const cell = constraints.capacityCells.find(item => item.resourceCode === resource.resourceCode && item.week === week);
                return cell
                  ? `<td><button class="${heatmapClass(cell.status)}" type="button" data-constraint-resource="${cell.resourceCode}"><strong>${number(cell.gap)}</strong><span>${number(cell.unconstrainedRequired)} / ${number(cell.constrainedAvailable)}</span></button></td>`
                  : `<td class="empty-cell">-</td>`;
              }).join("")}
            </tr>
          `).join("")}
        </tbody>
      </table>`
    : `<div class="table-empty"><strong>没有约束缺口热力格数据</strong></div>`;

  renderSelectedConstraintResource(constraints);
}

function renderSelectedConstraintResource(constraints) {
  const cells = constraints?.capacityCells
    ?.filter(item => item.resourceCode === state.selectedRccpResource)
    .sort((a, b) => a.week - b.week) || [];
  const summary = constraints?.capacitySummaries?.find(item => item.resourceCode === state.selectedRccpResource);
  byId("constraint-selected-title").textContent = summary ? `${summary.resourceName} 受限 / 不受限明细` : "选中资源受限 / 不受限明细";

  byId("constraint-gap-chart").innerHTML = cells.length
    ? cells.map(item => `
      <div class="load-row">
        <div class="load-row-label"><strong>第 ${item.week} 周</strong><span>不受限 ${number(item.unconstrainedRequired)} / 受限 ${number(item.constrainedAvailable)} / 缺口 ${number(item.gap)}</span></div>
        <div class="load-track"><span class="load-bar ${item.status === "Red" ? "overload" : ""}" style="--load-width:${Math.min(item.loadPercent, 140) / 1.4}%"></span></div>
        <div class="load-value">${percent(item.loadPercent)}</div>
      </div>
    `).join("")
    : `<div class="table-empty"><strong>没有选中资源约束数据</strong></div>`;

  const resourceActions = constraints?.recommendations?.filter(item =>
    item.target === state.selectedRccpResource || item.scopeType === "供应" || item.scopeType === "全局") || [];
  byId("constraint-action-list").innerHTML = resourceActions.length
    ? resourceActions.map(item => `
      <div class="diagnostic-item ${item.severity === "Red" ? "is-error" : ""}">
        <strong>${recommendationTypeLabel(item.actionType)}</strong>
        <span>${escapeHtml(businessEvidenceLabel(item.message))}</span>
      </div>
    `).join("")
    : `<div class="table-empty"><strong>没有约束动作建议</strong></div>`;

  byId("constraint-trace-list").innerHTML = constraints?.trace?.length
    ? constraints.trace.map(item => `
      <div class="diagnostic-item ${item.severity === "Warning" ? "is-error" : ""}">
        <strong>${traceStageLabel(item.stage)}</strong>
        <span>${escapeHtml(businessEvidenceLabel(item.message))}</span>
      </div>
    `).join("")
    : `<div class="table-empty"><strong>没有约束审计追踪</strong></div>`;
}

function renderProjectedSupply() {
  renderSupplierCollaborationWorkspace(state.supplierCollaboration);
}

function renderSupplierCollaborationWorkspace(workspace) {
  if (!workspace) {
    byId("supplier-collaboration-kpis").innerHTML = "";
    byId("supplier-summary-body").innerHTML = emptyRow("没有供应商钻取数据", 8);
    byId("supplier-weekly-grid").innerHTML = `<div class="table-empty"><strong>没有供应商周度网格数据</strong></div>`;
    byId("supplier-sku-requirement-body").innerHTML = emptyRow("没有 SKU 需求贡献", 7);
    byId("supplier-action-list").innerHTML = "";
    byId("supplier-selected-title").textContent = "选中供应商明细";
    return;
  }

  state.supplierCollaboration = workspace;
  if (!state.selectedSupplier || !workspace.summaries.some(item => item.supplier === state.selectedSupplier)) {
    state.selectedSupplier = workspace.selectedSupplier || valueOr(workspace.summaries[0]?.supplier, null);
  }

  byId("supplier-collaboration-kpis").innerHTML = [
    ["红色供应商", number(workspace.redSupplierCount), "存在供应缺口"],
    ["黄色供应商", number(workspace.yellowSupplierCount), "接近能力或有风险"],
    ["总供应缺口", number(workspace.totalSupplyGap), "不受限需求 - 受限能力"],
    ["缺口周数", number(workspace.gapWeekCount), "供应商 × 周"],
    ["受影响 SKU", number(workspace.affectedSkuCount), "由补货订单追溯"],
    ["建议动作", number(workspace.actions.length), "供应商级动作"],
  ].map(([label, value, note]) => `<div><span>${label}</span><strong>${value}</strong><small>${note}</small></div>`).join("");

  byId("supplier-summary-body").innerHTML = workspace.summaries.length
    ? workspace.summaries.map(item => row([
      `<button class="link-button" type="button" data-supplier="${item.supplier}"><strong>${item.supplier}</strong></button>`,
      number(item.totalUnconstrainedRequired),
      number(item.totalConstrainedAvailable),
      number(item.totalGap),
      number(item.gapWeeks),
      number(item.affectedSkuCount),
      `<span class="${statusClass(item.status)}">${statusLabel(item.status)}</span>`,
      `${item.recommendedAction}<br><small>${valueOr(item.statusReason, "")}</small>`,
    ])).join("")
    : emptyRow("没有供应商汇总数据", 8);

  renderSupplierWeeklyGrid(workspace);
  renderSelectedSupplier(workspace);
}

function renderSupplierWeeklyGrid(workspace) {
  const weeks = [...new Set(workspace.weeklyCells.map(item => item.week))].sort((a, b) => a - b);
  byId("supplier-weekly-grid").innerHTML = workspace.summaries.length
    ? `
      <table class="rccp-heatmap-table">
        <thead><tr><th>供应商</th>${weeks.map(week => `<th>第 ${week} 周</th>`).join("")}</tr></thead>
        <tbody>
          ${workspace.summaries.map(summary => `
            <tr>
              <th><button class="link-button" type="button" data-supplier="${summary.supplier}">${summary.supplier}</button></th>
              ${weeks.map(week => {
                const cell = workspace.weeklyCells.find(item => item.supplier === summary.supplier && item.week === week);
                return cell
                  ? `<td><button class="${heatmapClass(cell.status)}" type="button" data-supplier="${cell.supplier}" title="${valueOr(cell.statusReason, "")}"><strong>缺口 ${number(cell.gap)}</strong><span>需求 / 能力 ${number(cell.unconstrainedRequired)} / ${number(cell.constrainedAvailable)}</span><small>${valueOr(cell.statusReason, "")}</small></button></td>`
                  : `<td class="empty-cell">-</td>`;
              }).join("")}
            </tr>
          `).join("")}
        </tbody>
      </table>`
    : `<div class="table-empty"><strong>没有供应商周度网格数据</strong></div>`;
}

function renderSelectedSupplier(workspace) {
  const supplier = state.selectedSupplier;
  const summary = workspace?.summaries?.find(item => item.supplier === supplier);
  const requirements = workspace?.skuRequirements
    ?.filter(item => item.supplier === supplier)
    .sort((a, b) => a.week - b.week || a.materialFamily.localeCompare(b.materialFamily, "zh-CN") || a.sku.localeCompare(b.sku, "zh-CN")) || [];
  const actions = workspace?.actions?.filter(item => item.supplier === supplier || item.supplier === "全部供应商") || [];
  const supplierCells = workspace?.weeklyCells?.filter(item => item.supplier === supplier) || [];

  byId("supplier-selected-title").textContent = summary ? `${summary.supplier} 明细` : "选中供应商明细";
  byId("supplier-sku-requirement-body").innerHTML = requirements.length
    ? requirements.slice(0, 100).map(item => row([
      item.materialFamily,
      `<strong>${item.sku}</strong><br><small>${item.skuName}</small>`,
      item.family,
      `第 ${item.week} 周`,
      number(item.orderQuantity),
      money(item.projectedValue),
      triggerLabel(item.trigger),
    ])).join("")
    : emptyRow("没有 SKU 需求贡献", 7);

  byId("supplier-action-list").innerHTML = actions.length
    ? actions.map(item => `
      <div class="diagnostic-item ${item.severity === "Red" ? "is-error" : ""}">
        <strong>${recommendationTypeLabel(item.actionType)}</strong>
        <span>${escapeHtml(businessEvidenceLabel(item.message))}</span>
      </div>
    `).join("")
    : `<div class="table-empty"><strong>没有供应商建议动作</strong></div>`;

  const reasonItems = supplierCells
    .filter(item => item.status !== "Green")
    .slice(0, 6)
    .map(item => `
      <div class="diagnostic-item ${item.status === "Red" ? "is-error" : ""}">
        <strong>第 ${item.week} 周：${statusLabel(item.status)}</strong>
        <span>缺口 ${number(item.gap)}；需求 / 能力 ${number(item.unconstrainedRequired)} / ${number(item.constrainedAvailable)}。${valueOr(item.statusReason, "")}</span>
      </div>
    `).join("");
  if (reasonItems) {
    byId("supplier-action-list").innerHTML += reasonItems;
  }
}

function exceptionReasonLabel(reason) {
  return ({
    DemandSpike: "需求尖峰",
    ServiceLoss: "服务损失",
    BufferRisk: "缓冲风险",
  })[reason] || valueOr(reason, "-");
}

function renderExceptionWorkspace(exceptions) {
  if (!exceptions) {
    byId("exception-kpis").innerHTML = "";
    byId("exception-summary-body").innerHTML = emptyRow("没有异常数据", 9);
    byId("exception-signal-body").innerHTML = emptyRow("没有异常信号", 8);
    return;
  }

  if (!state.selectedExceptionSku || !exceptions.exceptions.some(item => item.sku === state.selectedExceptionSku)) {
    state.selectedExceptionSku = valueOr(exceptions.exceptions[0]?.sku, null);
  }

  const selected = exceptions.exceptions.find(item => item.sku === state.selectedExceptionSku);
  byId("exception-kpis").innerHTML = [
    ["红色异常 SKU", number(exceptions.redSkuCount), "需要优先评审"],
    ["黄色异常 SKU", number(exceptions.yellowSkuCount), "需要监控或模拟"],
    ["需求尖峰", number(exceptions.demandSpikeCount), "实际需求高于预测 12%"],
    ["服务损失", number(exceptions.serviceLossCount), "服务水平低于 95%"],
    ["缓冲风险", number(exceptions.bufferRiskCount), "净流动量低于黄区"],
    ["已带入场景", valueOr(selected?.sku, "-"), selected ? selected.recommendedTemplateId : "尚未选择"],
  ].map(([label, value, note]) => `<div><span>${label}</span><strong>${value}</strong><small>${note}</small></div>`).join("");

  byId("exception-selected-chip").className = selected ? statusClass(selected.severity) : "status-chip neutral";
  byId("exception-selected-chip").textContent = selected ? `${selected.sku} / ${statusLabel(selected.severity)}` : "未选择";
  byId("exception-summary-body").innerHTML = exceptions.exceptions.length
    ? exceptions.exceptions.map(item => row([
      `<button class="link-button" type="button" data-exception-sku="${item.sku}"><strong>${item.sku}</strong><br><small>${item.name}</small></button>`,
      item.family,
      item.latestExceptionWeekOffset,
      percent(item.maxDemandVariancePercent),
      percent(item.lowestServiceLevelPercent),
      number(item.lowestNetFlow),
      number(item.exceptionCount),
      `<span class="${statusClass(item.severity)}">${statusLabel(item.severity)}</span><br><small>${exceptionReasonLabel(item.primaryReason)}</small>`,
      `<strong>${item.recommendedTemplateId}</strong><br><small>${item.recommendedAction}</small>`,
    ])).join("")
    : emptyRow("没有异常 SKU", 9);

  renderSelectedException(selected);
}

function renderSelectedException(selected) {
  byId("exception-detail-title").textContent = selected ? `${selected.sku} 异常信号明细` : "异常信号明细";
  byId("exception-signal-body").innerHTML = selected?.signals?.length
    ? selected.signals.map(signal => row([
      signal.weekOffset,
      exceptionReasonLabel(signal.reason),
      number(signal.actualDemand),
      number(signal.forecastDemand),
      percent(signal.demandVariancePercent),
      percent(signal.serviceLevelPercent),
      number(signal.endingNetFlow),
      `<span class="${statusClass(signal.severity)}">${statusLabel(signal.severity)}</span>`,
    ])).join("")
    : emptyRow("没有异常信号", 8);
}

function applyExceptionToScenario() {
  const selected = state.exceptions?.exceptions?.find(item => item.sku === state.selectedExceptionSku);
  if (!selected) return;

  selectors.sku.value = selected.sku;
  previewControls.sku.value = selected.sku;
  previewControls.template.value = selected.recommendedTemplateId;
  applyFilters();
  byId("preview-status").className = "status-chip is-warning";
  byId("preview-status").textContent = "已从异常 SKU 带入，尚未运行";
  byId("route-status").className = "status-chip is-warning";
  byId("route-status").textContent = "异常 SKU 已带入场景";
  navigateWorkspace("future-scenario-panel", "scenario-config", false);
}

function renderMasterSettings(workspace) {
  if (!workspace) {
    masterSettingControls.kpis.innerHTML = "";
    masterSettingControls.board.innerHTML = `<div class="table-empty"><strong>主设置治理数据尚未加载</strong></div>`;
    masterSettingControls.currentBody.innerHTML = emptyRow("没有当前主设置", 6);
    masterSettingControls.proposalBody.innerHTML = emptyRow("运行预览后可生成主设置变更建议", 6);
    masterSettingControls.changeBody.innerHTML = emptyRow("没有已保存变更", 8);
    return;
  }

  masterSettingControls.status.className = "status-chip is-valid";
  masterSettingControls.status.textContent = "治理记录可用";
  masterSettingControls.kpis.innerHTML = [
    ["待评审", number(workspace.pendingReviewCount), "待评审 / 已评审"],
    ["已批准", number(workspace.approvedCount), "等待生效"],
    ["已生效", number(workspace.effectiveCount), "已进入执行边界"],
    ["高风险", number(workspace.highRiskCount), "红色影响"],
    ["服务影响", percent(workspace.serviceImpact), "正值改善服务"],
    ["现金影响", money(workspace.cashImpact), "库存 / 能力占用"],
  ].map(([label, value, note]) => `<div><span>${label}</span><strong>${value}</strong><small>${note}</small></div>`).join("");

  renderMasterSettingBoard(workspace);
  renderCurrentMasterSettings(workspace.currentSettings);
  renderMasterSettingProposals();
  renderMasterSettingChanges(workspace.recentChanges);
}

function renderMasterSettingBoard(workspace) {
  const statuses = ["Current", "Proposed", "Reviewed", "Approved", "Effective", "Expired"];
  masterSettingControls.board.innerHTML = statuses.map(status => {
    const count = workspace.statusCounts.find(item => item.status === status)?.count || 0;
    const changes = workspace.recentChanges.filter(item => item.status === status).slice(0, 4);
    return `
      <div class="master-setting-board-column">
        <h3>${masterSettingStatusLabel(status)}<span>${number(count)}</span></h3>
        ${changes.length
          ? changes.map(item => `
            <button class="master-setting-card" type="button" data-master-change-id="${escapeHtml(item.changeId)}">
              <strong>${escapeHtml(masterSettingDisplayValue(item.changeId, "target", item.target))}</strong>
              <span>${escapeHtml(masterSettingTypeLabel(item.settingType))} / ${escapeHtml(item.changeNumber)}</span>
              <small>基线 ${escapeHtml(item.sourceBaselineId || "未关联")} · ${item.creationMethod === "ScenarioDerived" ? `运行 ${escapeHtml(item.sourceScenarioRunId || "证据缺失")}` : "人工创建"}</small>
              <small>${escapeHtml(statusLabel(item.riskLevel))} · ${escapeHtml(masterSettingDisplayValue(item.changeId, "effectiveWindow", item.effectiveWindow))}</small>
            </button>
          `).join("")
          : `<div class="master-setting-empty">暂无保存记录</div>`}
      </div>
    `;
  }).join("");
}

function renderCurrentMasterSettings(settings) {
  masterSettingControls.currentBody.innerHTML = settings.length
    ? settings.map(item => row([
      escapeHtml(masterSettingTypeLabel(item.settingType)),
      escapeHtml(item.target),
      escapeHtml(businessEvidenceLabel(item.currentValue)),
      escapeHtml(businessEvidenceLabel(item.proposedValue)),
      escapeHtml(item.trigger),
      `<span class="${statusClass(item.status)}">${masterSettingStatusLabel(item.status)}</span>`,
    ])).join("")
    : emptyRow("没有当前主设置", 6);
}

function renderMasterSettingProposals() {
  const proposals = state.masterSettingProposals || [];
  masterSettingControls.proposalBody.innerHTML = proposals.length
    ? proposals.map((item, index) => row([
      `<button class="link-button" type="button" data-master-proposal-index="${index}"><strong>${escapeHtml(masterSettingTypeLabel(item.settingType))}</strong></button>`,
      escapeHtml(item.target),
      escapeHtml(businessEvidenceLabel(item.currentValue)),
      escapeHtml(businessEvidenceLabel(item.proposedValue)),
      `<span class="${statusClass(item.riskLevel)}">${statusLabel(item.riskLevel)}</span>`,
      `${percent(item.serviceImpact)} / ${money(item.cashImpact)}`,
    ])).join("")
    : emptyRow("历史提案只读保留；场景派生变更请使用 DDOM 变更包", 6);
}

function renderMasterSettingChanges(changes) {
  masterSettingControls.changeBody.innerHTML = changes.length
    ? changes.map(item => row([
      `<button class="link-button" type="button" data-master-change-id="${escapeHtml(item.changeId)}"><strong>${escapeHtml(item.changeNumber)}</strong><small>${escapeHtml(masterSettingDisplayValue(item.changeId, "createdBy", item.createdBy))}</small></button>`,
      escapeHtml(masterSettingTypeLabel(item.settingType)),
      escapeHtml(masterSettingDisplayValue(item.changeId, "target", item.target)),
      escapeHtml(item.sourceBaselineId || "未关联"),
      escapeHtml(item.creationMethod === "ScenarioDerived" ? item.sourceScenarioRunId || "证据缺失" : "不适用"),
      `<span class="${statusClass(item.status)}">${masterSettingStatusLabel(item.status)}</span>`,
      `<span class="${statusClass(item.riskLevel)}">${statusLabel(item.riskLevel)}</span>`,
      escapeHtml(masterSettingDisplayValue(item.changeId, "effectiveWindow", item.effectiveWindow)),
    ])).join("")
    : emptyRow("没有已保存主设置变更", 8);
  configureCoordinationLineageSelectors();
}

function renderMasterSettingProposalDetail(proposal) {
  state.currentMasterSettingDetail = null;
  masterSettingControls.detailTitle.textContent = proposal ? "待保存主设置变更建议" : "主设置变更详情";
  masterSettingControls.detail.innerHTML = proposal
    ? [
      ["类型", masterSettingTypeLabel(proposal.settingType)],
      ["目标", proposal.target],
      ["当前值", businessEvidenceLabel(proposal.currentValue)],
      ["建议值", businessEvidenceLabel(proposal.proposedValue)],
      ["触发原因", proposal.trigger],
      ["来源基线", proposal.sourceBaselineId || "未关联冻结基线"],
      ["来源场景", proposal.sourceScenarioRunId || "未关联"],
      ["负责人 / 审批人", `${proposal.owner || "未指定"} / ${proposal.approver || "未指定"}`],
      ["生效窗口", proposal.effectiveWindow],
      ["生效 / 失效", `${proposal.effectiveFrom || "未指定"} / ${proposal.effectiveThrough || "未指定"}`],
      ["复查日期", proposal.reviewOn || "未指定"],
      ["预期效果", proposal.expectedEffect || "未指定"],
      ["回滚条件", proposal.rollbackCondition || "未指定"],
      ["风险", statusLabel(proposal.riskLevel)],
      ["影响", `${percent(proposal.serviceImpact)} / ${money(proposal.cashImpact)}`],
    ].map(([label, value]) => `<div><span>${label}</span><strong>${escapeHtml(value)}</strong></div>`).join("")
    : `<div class="table-empty"><strong>请选择主设置变更建议或已保存记录</strong></div>`;
  masterSettingControls.auditList.innerHTML = `<div class="table-empty"><strong>保存后生成审计链</strong></div>`;
  masterSettingControls.lineageList.innerHTML = `<div class="table-empty"><strong>保存后可查询关联场景与行动</strong></div>`;
}

function renderMasterSettingLineage(detail, sourceScenario, coordinationItems) {
  const sourceScenarioRunId = detail.proposal.sourceScenarioRunId;
  const sourceSummary = sourceScenario?.summary;
  const scenarioLink = sourceScenarioRunId
    ? `<button class="link-button" type="button" data-lineage-scenario-run-id="${escapeHtml(sourceScenarioRunId)}"><strong>${escapeHtml(sourceSummary?.runNumber || sourceScenarioRunId)}</strong><small>${escapeHtml(sourceSummary?.name || "场景详情证据缺失")}</small></button>`
    : "未关联来源场景";
  const coordinationLinks = valueOr(coordinationItems, []).map(item => `
    <button class="link-button" type="button" data-lineage-coordination-item-id="${escapeHtml(item.itemId)}">
      <strong>${escapeHtml(item.itemNumber)}</strong><small>${escapeHtml(item.title)}</small>
    </button>`).join("");
  masterSettingControls.lineageList.innerHTML = `
    <div class="diagnostic-item"><strong>03 来源场景</strong><span>${scenarioLink}</span></div>
    <div class="diagnostic-item"><strong>05 关联行动</strong><span>${coordinationLinks || "尚无关联行动事项"}</span><small>${number(valueOr(coordinationItems, []).length)} 个关联行动</small></div>`;
}

function renderMasterSettingDetail(detail, auditEvents, sourceScenario = null, coordinationItems = []) {
  state.currentMasterSettingDetail = detail;
  const summary = detail.summary;
  const proposal = detail.proposal;
  masterSettingControls.detailTitle.textContent = `${summary.changeNumber} 主设置变更详情`;
  masterSettingControls.detail.innerHTML = [
    ["编号", summary.changeNumber],
    ["类型", masterSettingTypeLabel(summary.settingType)],
    ["目标", masterSettingDisplayValue(summary.changeId, "target", summary.target)],
    ["当前值", masterSettingDisplayValue(summary.changeId, "currentValue", businessEvidenceLabel(summary.currentValue))],
    ["建议值", masterSettingDisplayValue(summary.changeId, "proposedValue", businessEvidenceLabel(summary.proposedValue))],
    ["触发原因", masterSettingDisplayValue(summary.changeId, "trigger", summary.trigger)],
    ["来源基线", proposal.sourceBaselineId || "未关联冻结基线"],
    ["来源场景", proposal.sourceScenarioRunId || "未关联"],
    ["负责人 / 审批人", `${masterSettingDisplayValue(summary.changeId, "owner", proposal.owner || "未指定")} / ${masterSettingDisplayValue(summary.changeId, "approver", proposal.approver || "未指定")}`],
    ["生效窗口", masterSettingDisplayValue(summary.changeId, "effectiveWindow", summary.effectiveWindow)],
    ["生效 / 失效", `${proposal.effectiveFrom || "未指定"} / ${proposal.effectiveThrough || "未指定"}`],
    ["复查日期", proposal.reviewOn || "未指定"],
    ["预期效果", masterSettingDisplayValue(summary.changeId, "expectedEffect", proposal.expectedEffect || "未指定")],
    ["回滚条件", masterSettingDisplayValue(summary.changeId, "rollbackCondition", proposal.rollbackCondition || "未指定")],
    ["状态", masterSettingStatusLabel(summary.status)],
    ["风险", statusLabel(summary.riskLevel)],
    ["服务 / 现金影响", `${percent(summary.serviceImpact)} / ${money(summary.cashImpact)}`],
  ].map(([label, value]) => `<div><span>${label}</span><strong>${escapeHtml(value)}</strong></div>`).join("");
  renderMasterSettingLineage(detail, sourceScenario, coordinationItems);

  masterSettingControls.auditList.innerHTML = auditEvents.length
    ? auditEvents.map(item => `
      <div class="diagnostic-item ${item.severity === "Warning" ? "is-error" : ""}">
        <strong>${item.sequence}. ${escapeHtml(auditEventLabel(item.eventType))} / ${traceStageLabel(item.stage)}</strong>
        <span>${escapeHtml(masterSettingDisplayValue(summary.changeId, "auditMessage", businessEvidenceLabel(item.message)))}</span>
      </div>
    `).join("")
    : `<div class="table-empty"><strong>没有审计事件</strong></div>`;
}

async function loadMasterSettingsWorkspace() {
  const response = await fetch("/api/master-settings-workspace?limit=50", {
    headers: { Accept: "application/json" },
  });
  if (!response.ok) {
    throw new Error(`主设置治理工作台接口失败：${response.status}`);
  }
  state.masterSettings = await response.json();
  if (!state.selectedMasterChangeId) {
    state.selectedMasterChangeId = valueOr(state.masterSettings.recentChanges[0]?.changeId, null);
  }
  renderMasterSettings(state.masterSettings);
  if (state.selectedMasterChangeId) {
    await loadMasterSettingChangeDetail(state.selectedMasterChangeId);
  }
}

async function generateMasterSettingProposals() {
  if (!state.preview) {
    masterSettingControls.status.className = "status-chip is-warning";
    masterSettingControls.status.textContent = "请先运行预览";
    return;
  }

  const sourceBaselineId = byId("governance-baseline-id").value;
  if (!sourceBaselineId) throw new Error("人工创建配置建议必须选择冻结基线。");
  masterSettingControls.status.className = "status-chip is-warning";
  masterSettingControls.status.textContent = "正在生成建议";
  const request = {
    ...state.preview.request,
    governanceContext: governanceDecisionContext(null, sourceBaselineId),
  };
  const response = await fetch("/api/master-settings/proposals/from-preview", {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify(request),
  });
  if (!response.ok) {
    throw new Error(`主设置建议接口失败：${response.status}`);
  }

  const result = await response.json();
  useMasterSettingProposalResponse(result, "人工配置建议已关联冻结基线，可人工保存");
}

function governanceDecisionContext(sourceScenarioRunId = null, sourceBaselineId = null) {
  return {
    sourceBaselineId: sourceBaselineId || state.futureComparisonRequest?.baselineSnapshotId || null,
    sourceScenarioRunId,
    owner: byId("governance-owner").value || null,
    approver: byId("governance-approver").value || null,
    effectiveFrom: byId("governance-effective-from").value || null,
    effectiveThrough: byId("governance-effective-through").value || null,
    reviewOn: byId("governance-review-on").value || null,
    expectedEffect: byId("governance-expected-effect").value || null,
    rollbackCondition: byId("governance-rollback-condition").value || null,
  };
}

function useMasterSettingProposalResponse(result, successMessage) {
  state.masterSettingProposals = result.proposals || [];
  state.selectedMasterProposalIndex = 0;
  renderMasterSettings(state.masterSettings);
  masterSettingControls.status.className = state.masterSettingProposals.length ? "status-chip is-valid" : "status-chip is-warning";
  masterSettingControls.status.textContent = state.masterSettingProposals.length ? successMessage : "没有可生成的建议";
  renderMasterSettingProposalDetail(state.masterSettingProposals[0]);
  navigateWorkspace("ddom-decision-panel", "parameter-decision", false);
}

async function generateMasterSettingProposalsFromComparison() {
  if (!state.futureComparisonRequest || !state.futureComparison) {
    throw new Error("请先在未来场景模拟中运行冻结基线比较。");
  }
  const responseId = byId("governance-response-id").value;
  if (!responseId) throw new Error("请选择冻结比较方案。");
  const savedRun = savedFutureComparison(responseId);
  if (!savedRun?.runId) throw new Error("所选方案必须先在 03 中显式保存为真实场景运行。");
  if (savedRun.summary?.baselineSnapshotId !== state.futureComparisonRequest.baselineSnapshotId || savedRun.summary?.responseId !== responseId) {
    throw new Error("已保存运行与当前冻结基线或响应方案不一致，请重新保存。");
  }
  masterSettingControls.status.className = "status-chip is-warning";
  masterSettingControls.status.textContent = "正在从冻结比较重算建议";
  const response = await fetch("/api/master-settings/proposals/from-comparison", {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify({
      comparison: state.futureComparisonRequest,
      responseId,
      governanceContext: governanceDecisionContext(savedRun.runId, state.futureComparisonRequest.baselineSnapshotId),
      sourceScenarioRunId: savedRun.runId,
    }),
  });
  const result = await response.json();
  if (!response.ok) throw new Error(result.message || `冻结比较治理建议接口失败：${response.status}`);
  useMasterSettingProposalResponse(result, "冻结比较治理建议已生成，可人工保存");
}

async function loadMasterSettingChangeDetail(changeId) {
  state.selectedMasterChangeId = changeId;
  const [detailResponse, auditResponse] = await Promise.all([
    fetch(`/api/master-settings/changes/${encodeURIComponent(changeId)}`, { headers: { Accept: "application/json" } }),
    fetch(`/api/master-settings/changes/${encodeURIComponent(changeId)}/audit`, { headers: { Accept: "application/json" } }),
  ]);
  if (!detailResponse.ok) {
    throw new Error(`主设置变更详情接口失败：${detailResponse.status}`);
  }
  if (!auditResponse.ok) {
    throw new Error(`主设置变更审计链接口失败：${auditResponse.status}`);
  }
  const detail = await detailResponse.json();
  const auditEvents = await auditResponse.json();
  const sourceScenarioRunId = detail.proposal.sourceScenarioRunId;
  const [sourceScenarioResponse, coordinationResponse] = await Promise.all([
    sourceScenarioRunId
      ? fetch(`/api/scenario-runs/${encodeURIComponent(sourceScenarioRunId)}`, { headers: { Accept: "application/json" } })
      : Promise.resolve(null),
    fetch(`/api/coordination-items?limit=50&relatedMasterSettingChangeId=${encodeURIComponent(changeId)}`, { headers: { Accept: "application/json" } }),
  ]);
  if (!coordinationResponse.ok) throw new Error("主设置关联行动查询失败。");
  if (sourceScenarioResponse && !sourceScenarioResponse.ok && sourceScenarioResponse.status !== 404) {
    throw new Error("主设置来源场景查询失败。");
  }
  const sourceScenario = sourceScenarioResponse?.ok ? await sourceScenarioResponse.json() : null;
  renderMasterSettingDetail(detail, auditEvents, sourceScenario, await coordinationResponse.json());
}

function renderTrace(data) {
  const trend = filterBufferTrendWorkspace(state.bufferTrend);
  const redBufferWeeks = trend?.series?.filter(item => item.status === "Red").length || 0;
  const redResourceWeeks = state.rccp?.weeklyCells?.filter(item => item.status === "Red").length || 0;
  const redSupplierWeeks = state.supplierCollaboration?.weeklyCells?.filter(item => item.status === "Red").length || 0;
  const traces = [
    `读取 ${data.skus.length} 个 SKU、${data.resources.length} 个资源、${data.supplierItemSources.length} 条供应来源。`,
    `缓冲趋势、RCCP、受限 / 不受限和供应商需求钻取均来自后端领域服务，前端只做筛选和展示。`,
    `未来 ${data.request.horizonWeeks} 周缓冲趋势按净流动量、红黄绿缓冲区与订货周期复核点进行投影。`,
    `RCCP 负荷来自预计补货订单与资源路由折算，不使用前端需求估算。`,
    `当前筛选范围内红区水位 ${redBufferWeeks} 条，超载资源周 ${redResourceWeeks} 条，红色供应窗口 ${redSupplierWeeks} 条。`,
  ];

  byId("trace-list").innerHTML = traces.map((trace, index) => `
    <div class="diagnostic-item ${index === traces.length - 1 ? "is-error" : ""}">
      <strong>追踪 ${index + 1}</strong>
      <span>${trace}</span>
    </div>
  `).join("");
}

function localizeLegacyTraceWording() {
  byId("trace-list")?.querySelectorAll(".diagnostic-item span").forEach(item => {
    item.textContent = item.textContent.replaceAll("红色供应窗口", "供应能力不足周");
  });
}

function renderWorkspace() {
  const data = state.filtered;

  renderKpis(data);
  renderPublicDemoGoldenLoop(state.publicDemoGoldenLoop);
  renderAdventureWorksProductDemo(state.adventureWorksProductDemo);
  renderProductFamilyDashboard(state.productFamilyDashboard);
  renderReadiness(data);
  renderScenarioTemplates(data);
  renderScenarioComparison(data);
  byId("budget-comparison-body").innerHTML = emptyRow("运行预览后显示预算与去年同期对照", 6);
  renderBufferTrend();
  renderProductRccp(state.rccp, "基准方案");
  renderConstraintWorkspace(state.constraints);
  renderProjectedSupply();
  renderExceptionWorkspace(state.exceptions);
  renderMasterSettings(state.masterSettings);
  renderTrace(data);
  renderScenarioScopeSummary();
  localizeLegacyTraceWording();

  byId("snapshot-freshness").textContent = `${data.request.anchorDate} / ${data.request.horizonWeeks} 周`;
  setWorkspaceStatus("Green", "工作台已就绪");
  showWorkspaceContent();
}

function renderPublicDemoGoldenLoop(workspace) {
  if (!workspace) return;
  const packageChip = byId("public-demo-package-chip");
  packageChip.className = `status-chip ${workspace.packageChecksumMatches ? "is-valid" : "is-invalid"}`;
  packageChip.textContent = workspace.packageChecksumMatches ? "数据包校验一致" : "数据包校验不一致";
  const sampleObject = [
    workspace.packageContext?.sampleItem,
    workspace.packageContext?.sampleLocation,
    workspace.packageContext?.sampleUom,
  ].filter(Boolean).join(" / ") || "未读取";

  byId("public-demo-kpis").innerHTML = [
    ["数据包", workspace.packageAvailable ? "已读取" : "不可用", workspace.packagePath],
    ["数据包校验", workspace.packageChecksumMatches ? "一致" : "不一致", workspace.expectedPackageChecksum],
    ["映射置信", workspace.mappingConfidence, "仅用于公开演示"],
    ["物料 / 地点 / 计量单位", sampleObject, "来自公开演示数据包样例"],
    ["DDAE 交付", workspace.handoff.payloadWritten ? "已写出" : "未写出", workspace.handoff.ddaeToSdbrPayloadPath],
    ["SDBR 反馈", `${workspace.feedback.filter(item => item.exists).length}/${workspace.feedback.length}`, "从约定交付路径读取"],
  ].map(([label, value, hint]) => `<div><span>${escapeHtml(label)}</span><strong>${escapeHtml(value)}</strong><small>${escapeHtml(hint)}</small></div>`).join("");

  byId("public-demo-package-summary").innerHTML = [
    ["PackageID", "PUBLIC-DEMO-GOLDEN-DATA-V1"],
    ["受控标签", workspace.evidenceLabels.join(" / ")],
    ["期望 checksum", workspace.expectedPackageChecksum],
    ["manifest checksum", workspace.manifestPackageChecksum || "未读取"],
    ["样例物料", workspace.packageContext.sampleItem || "未读取"],
    ["样例地点", workspace.packageContext.sampleLocation || "未读取"],
    ["样例数量", `${number(workspace.packageContext.sampleQuantity)} ${workspace.packageContext.sampleUom}`],
    ["非声明", workspace.nonClaimsSummary],
  ].map(([label, value]) => `<div><dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value)}</dd></div>`).join("");

  byId("public-demo-package-file-body").innerHTML = workspace.packageFiles.map(item => row([
    escapeHtml(item.fileName),
    escapeHtml(item.role),
    item.rowCount === null || item.rowCount === undefined ? "文本文件" : number(item.rowCount),
    `<code>${escapeHtml((item.checksum || "").slice(0, 12))}</code>`,
  ])).join("") || emptyRow("未读取到 package file role map", 4);

  byId("public-demo-mapping-body").innerHTML = workspace.reviewedMappings.map(item => row([
    `<strong>${escapeHtml(item.demoObject)}</strong>`,
    escapeHtml(item.boundary),
    escapeHtml(item.allowedUse),
    escapeHtml(item.forbiddenUse),
  ])).join("");

  renderPublicDemoSchedulingAdapter(workspace.schedulingAdapter);
  renderPublicDemoPayload(workspace.payloadPreview, workspace.handoff);
  renderPublicDemoFeedback(workspace.feedback, workspace.nonClaimsSummary);
  renderPublicDemoBusinessUserView(workspace);
}

function renderAdventureWorksProductDemo(workspace) {
  if (!workspace) return;
  const profile = workspace.profile || {};
  const summary = byId("product-demo-profile-summary");
  if (!summary) return;

  summary.innerHTML = [
    ["演示档案编号（ProfileID）", profile.profileID || "-"],
    ["运行模式（Mode）", profile.mode || "-"],
    ["场景标签（ScenarioLabel）", profile.scenarioLabel || "-"],
    ["映射置信", profile.mappingConfidence || "-"],
    ["底层公开数据包（BasePackageID）", profile.basePackageID || "-"],
    ["演示补全包（DemoAuthorityPackageID）", profile.demoAuthorityPackageID || "-"],
    ["DDAE 演示适配器", profile.ddaeAdapterVersion || "-"],
    ["默认面板策略（PanelPolicy）", productDemoPanelPolicyDefaultLabel(profile.panelPolicyDefault)],
    ["fallback 防护", workspace.fallbackToCoreSampleBlocked ? "禁止回退 DDAE_CORE_SAMPLE" : "未确认"],
    ["主设置防护", workspace.feedbackMutationBlocked && workspace.networkCandidateMutationBlocked ? "feedback / candidates 不自动改主设置" : "未确认"],
  ].map(([label, value]) => `<div><dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value)}</dd></div>`).join("");

  const coveredPanels = (workspace.panelPolicies || [])
    .filter(item => item.handling === "ProductDemoMode")
    .map(item => item.displayLabel || item.panelID);
  const placeholderPanels = (workspace.panelPolicies || [])
    .filter(item => item.handling === "Placeholder")
    .map(item => item.displayLabel || item.panelID);
  byId("product-demo-scope-summary").innerHTML = `
    <div class="diagnostic-item">
      <strong>本次产品演示覆盖范围</strong>
      <span>已接入 ProductDemoMode 的区域：${escapeHtml(coveredPanels.join("、") || "未确认")}。</span>
      <small>占位区域：${escapeHtml(placeholderPanels.join("、") || "无")}。占位只表示本轮尚未接入 AdventureWorks 产品演示数据，不代表产品能力缺失。</small>
    </div>
    <div class="diagnostic-item">
      <strong>证据术语说明</strong>
      <span>来源类型表示数据来自 AdventureWorks、派生计算或 DemoAuthority 补全；证据编号用于追踪具体演示依据；面板策略用于防止未适配区域回退旧样例。</span>
    </div>`;

  byId("product-demo-ddae-authority-body").innerHTML = (workspace.ddaeAuthorityRows || []).map(item => row([
    escapeHtml(productDemoGroupLabel(item.groupName)),
    `<strong>${escapeHtml(item.businessObject || "-")}</strong>`,
    escapeHtml(item.valueSummary || "-"),
    `<span class="status-chip neutral" title="来源类型：说明该治理值来自公开数据、派生计算或 DemoAuthority 显式补全。">${escapeHtml(productDemoSourceClassLabel(item.sourceClass))}</span>`,
    `<code title="证据编号：用于追踪该治理值的演示依据。">${escapeHtml(item.evidenceRef || "-")}</code>`,
    escapeHtml(item.owner || "-"),
  ])).join("") || emptyRow("未读取到 DDAE DemoAuthority 治理行", 6);

  byId("product-demo-panel-policy-body").innerHTML = (workspace.panelPolicies || []).map(item => row([
    `<strong>${escapeHtml(item.displayLabel || item.panelID)}</strong><small>${escapeHtml(item.panelID)}</small>`,
    `<span class="status-chip ${item.handling === "ProductDemoMode" ? "is-valid" : "is-paused"}">${escapeHtml(productDemoHandlingLabel(item.handling))}</span>`,
    escapeHtml(productDemoPanelPolicyText(item)),
  ])).join("") || emptyRow("未读取到 ProductDemoMode 面板策略", 3);

  const validations = workspace.validation || [];
  byId("product-demo-validation-list").innerHTML = validations.slice(0, 8).map(item => `
    <div class="diagnostic-item ${item.status === "通过" ? "" : "is-error"}">
      <strong>${escapeHtml(item.rule)}</strong>
      <span>${escapeHtml(item.status)}：${escapeHtml(item.message)}</span>
      <small>${escapeHtml(item.evidenceRef || "")}</small>
    </div>
  `).join("") + `
    <div class="diagnostic-item">
      <strong>非声明</strong>
      <span>${escapeHtml((workspace.nonClaims || []).join(" "))}</span>
    </div>`;
}

function productDemoSourceClassLabel(sourceClass) {
  const labels = {
    AdventureWorks: "AdventureWorks 原始公开数据",
    DerivedFromAdventureWorks: "由 AdventureWorks 派生",
    DemoAuthority: "DemoAuthority 显式补全",
    Missing: "缺失",
    Placeholder: "占位",
  };
  return labels[sourceClass] || sourceClass || "-";
}

function productDemoPanelPolicyDefaultLabel(policy) {
  if (policy === "Placeholder") return "未适配区域显示占位，不回退旧样例";
  if (policy === "ProductDemoMode") return "按产品演示模式展示";
  if (policy === "SampleModeOnly") return "仅样例模式";
  return policy || "-";
}

function productDemoPanelPolicyText(item) {
  if (item.handling === "ProductDemoMode") return "已接入 AdventureWorks 产品演示数据，按 ProductDemoMode 展示。";
  if (item.handling === "Placeholder") return "本区尚未接入 AdventureWorks 产品演示数据，当前仅显示占位说明；这不代表产品能力缺失。";
  if (item.handling === "SampleModeOnly") return "仅样例模式可见，ProductDemoMode 不允许静默回退。";
  return item.placeholderText || "-";
}

function productDemoGroupLabel(groupName) {
  const labels = {
    ServiceTargets: "服务目标",
    DemandProxies: "需求代理",
    DDMRPBufferSettings: "DDMRP 缓冲设置",
    PlanningWindows: "计划窗口",
    ControlPointGovernance: "控制点治理",
    ResourceRoleGovernance: "资源角色治理",
    ReleasePolicies: "释放策略",
    PriorityPolicies: "优先级策略",
    ApprovalEvidence: "批准证据",
    EffectivityEvidence: "生效证据",
    RuleVersions: "规则版本",
  };
  return labels[groupName] || groupName || "-";
}

function productDemoHandlingLabel(handling) {
  if (handling === "ProductDemoMode") return "ProductDemoMode 展示";
  if (handling === "SampleModeOnly") return "仅样例模式";
  if (handling === "Placeholder") return "占位";
  return handling || "-";
}

function renderPublicDemoSchedulingAdapter(adapter) {
  if (!adapter) return;
  const chip = byId("public-demo-adapter-chip");
  chip.className = "status-chip is-paused";
  chip.textContent = "非 DDAE 执行权威";

  byId("public-demo-scheduling-governance").innerHTML = [
    ["适配器档案", adapter.adapterProfileID],
    ["场景标签", adapter.scenarioLabel],
    ["映射置信", adapter.mappingConfidence],
    ["反馈边界", adapter.feedbackBoundary],
  ].map(([label, value]) => `<div><dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value)}</dd></div>`).join("");

  byId("public-demo-adapter-metadata-body").innerHTML = adapter.adapterMetadata.map(item => row([
    `<code>${escapeHtml(item.fieldName)}</code>`,
    escapeHtml(item.value),
    escapeHtml(item.owner),
    escapeHtml(item.ddaeUse),
    escapeHtml(item.forbiddenUse),
  ])).join("") || emptyRow("未读取到适配器元数据", 5);

  const policyItems = adapter.governancePolicies.map(item => `
    <div class="diagnostic-item">
      <strong>${escapeHtml(item.policyArea)}</strong>
      <span>${escapeHtml(item.ddaeResponsibility)}</span>
      <small>${escapeHtml(item.evidenceFieldGroup)} / ${escapeHtml(item.ruleVersionID)}</small>
    </div>
  `).join("");
  const boundaryItems = adapter.nonDdaeOwnedExecutionMetadata.map(item => `
    <div class="diagnostic-item">
      <strong>${escapeHtml(item.executionObject)}</strong>
      <span>${escapeHtml(item.owner)}：${escapeHtml(item.ddaeDisplayUse)}</span>
      <small>${escapeHtml(item.forbiddenUse)}</small>
    </div>
  `).join("");
  byId("public-demo-adapter-boundary-list").innerHTML = `${policyItems}${boundaryItems}`;
}

function renderPublicDemoPayload(payload, handoff) {
  if (!payload?.payload) return;
  byId("public-demo-handoff-chip").className = `status-chip ${handoff.payloadWritten ? "is-valid" : "neutral"}`;
  byId("public-demo-handoff-chip").textContent = handoff.payloadWritten ? "payload 已写出" : "payload 未写出";
  byId("public-demo-payload-summary").innerHTML = [
    ["ContractID", payload.contractID],
    ["MessageID", payload.messageID],
    ["OperatingModelConfigurationID", payload.payload.operatingModelConfigurationID],
    ["Fingerprint", payload.payload.fingerprint],
    ["写出路径", handoff.ddaeToSdbrPayloadPath],
    ["写出时间", handoff.payloadWrittenAt || "尚未写出"],
  ].map(([label, value]) => `<div><dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value)}</dd></div>`).join("");
  byId("public-demo-operating-model").textContent = JSON.stringify({
    OperatingModelConfigurationID: payload.payload.operatingModelConfigurationID,
    Status: payload.payload.status,
    Scope: payload.payload.scope,
    Approval: payload.payload.approval,
    ChangeReason: payload.payload.changeReason,
    Fingerprint: payload.payload.fingerprint,
  }, null, 2);
  byId("public-demo-scheduling").textContent = JSON.stringify(payload.payload.schedulingConfiguration, null, 2);
  byId("public-demo-ddmrp").textContent = JSON.stringify(payload.payload.ddmrpConfiguration, null, 2);
}

function renderPublicDemoFeedback(feedback, nonClaimsSummary) {
  byId("public-demo-feedback-body").innerHTML = feedback.map(item => row([
    escapeHtml(item.feedbackName),
    `<span class="status-chip ${item.exists ? "is-valid" : "neutral"}">${escapeHtml(item.processingStatus)}</span>`,
    escapeHtml(item.messageID || "未回传"),
    escapeHtml(item.planningRunID || "-"),
    escapeHtml(item.operatingModelConfigurationID || "-"),
    `<code>${escapeHtml((item.operatingModelFingerprint || "-").slice(0, 24))}</code>`,
    escapeHtml([item.runStatus, item.solverStatus].filter(Boolean).join(" / ") || "-"),
    escapeHtml([
      item.overallStatus ? `总体 ${item.overallStatus}` : null,
      item.reliabilityStatus ? `可靠 ${item.reliabilityStatus}` : null,
      item.speedStatus ? `速度 ${item.speedStatus}` : null,
      item.stabilityStatus ? `稳定 ${item.stabilityStatus}` : null,
      item.validationStatus ? `验证 ${item.validationStatus}` : null,
    ].filter(Boolean).join(" / ") || "-"),
    escapeHtml(item.mappingConfidence || (item.labels || []).find(label => label.includes("MappingConfidence")) || "-"),
    escapeHtml(item.message),
  ])).join("");
  byId("public-demo-non-claims").innerHTML = `
    <div class="diagnostic-item">
      <strong>非生产声明已保留</strong>
      <span>${escapeHtml(nonClaimsSummary)}</span>
      <span>不自动更新 DDAE 主数据、Operating Model、主设置、buffer、supplier-source facts、lead time、MOQ 或 order cycle。</span>
    </div>`;
}

function renderPublicDemoBusinessUserView(workspace) {
  const payload = workspace?.payloadPreview?.payload;
  const scheduling = payload?.payload?.schedulingConfiguration;
  const ddmrp = payload?.payload?.ddmrpConfiguration;
  const operatingModel = payload?.payload;
  const adapter = workspace?.schedulingAdapter;
  const feedback = workspace?.feedback || [];
  const planningFeedback = feedback.find(item => item.feedbackName === "PlanningRunFeedback");
  const varianceFeedback = feedback.find(item => item.feedbackName === "VarianceAnalysisFeedback");
  const validationSummary = feedback.find(item => item.feedbackName === "ValidationSummary");
  const controlPoint = scheduling?.controlPoints?.[0];
  const bufferPoint = ddmrp?.decouplingPoints?.[0];
  const policies = adapter?.governancePolicies || [];
  const materialMode = (adapter?.adapterMetadata || []).find(item => item.fieldName === "MaterialConstraintsMode")?.value || "OmittedForPublicDemo";
  const sampleItem = workspace.packageContext?.sampleItem || "未读取物料";
  const sampleLocation = workspace.packageContext?.sampleLocation || "未读取地点";
  const sampleUom = workspace.packageContext?.sampleUom || "未读取单位";

  byId("public-demo-business-summary").innerHTML = [
    {
      step: "1",
      title: "当前业务场景",
      body: "公开演示关注同一份文件化数据包中的样例物料、地点和计量单位。业务上它用于说明 DDS&OP 参数发布、受控交付和 SDBR 评审反馈如何衔接，不代表生产主数据权威。",
      trace: [
        `关键对象：${bufferPoint?.itemID || sampleItem} / ${bufferPoint?.locationID || sampleLocation} / ${bufferPoint?.uom || sampleUom}`,
        `映射置信：${workspace.mappingConfidence || "PublicDemoOnly"}`,
      ],
    },
    {
      step: "2",
      title: "DDAE 批准了什么",
      body: "DDAE 批准的是运营模型、DDMRP 缓冲治理和排程治理口径。它表达计划意图和约束边界，不发布 SDBR 的可执行工艺路线、工序时长或资源日历。",
      trace: [
        `Operating Model：${operatingModel?.operatingModelConfigurationID || "-"}`,
        `DDMRP：${ddmrp?.ddmrpConfigurationID || "-"}`,
        `控制点：${controlPoint?.resourceID || sampleLocation}`,
        `治理策略：${policies.map(item => item.policyArea).join("、") || "控制点、资源角色、释放策略、时间缓冲、优先级、计划窗口"}`,
      ],
    },
    {
      step: "3",
      title: "DDAE 交付给 SDBR 什么",
      body: "DDAE 交付的是受控的配置 payload 和 runtime planning input。SDBR 可以读取它们作为演示级计划输入，但不能把它们当作 DDAE 对可执行 routing 或生产排程能力的授权。",
      trace: [
        "DDSOP-CONFIG-INBOUND-V1",
        "DDSOP-RUNTIME-PLANNING-INPUT-V1",
        `MaterialConstraintsMode：${materialMode}`,
        "MaterialConstraints[]：空列表",
      ],
    },
    {
      step: "4",
      title: "SDBR 反馈对 DDAE 意味着什么",
      body: "Planning Run 反馈说明 SDBR 是否能消费本次受控输入；Variance Analysis 反馈说明演示链路的稳定性、速度和可靠性状态。DDAE 把这些结果作为治理评审上下文，而不是自动批准新的主设置。",
      trace: [
        `Planning Run：${planningFeedback?.planningRunID || "等待回传"}`,
        `运行状态：${[planningFeedback?.runStatus, planningFeedback?.solverStatus].filter(Boolean).join(" / ") || "-"}`,
        `偏差状态：${[varianceFeedback?.overallStatus, validationSummary?.validationStatus].filter(Boolean).join(" / ") || "-"}`,
      ],
    },
  ].map(item => `
    <article class="business-demo-step">
      <span>${escapeHtml(item.step)}</span>
      <div>
        <h3>${escapeHtml(item.title)}</h3>
        <p>${escapeHtml(item.body)}</p>
        <ul>${item.trace.map(trace => `<li>${escapeHtml(trace)}</li>`).join("")}</ul>
      </div>
    </article>
  `).join("");

  byId("public-demo-business-boundary").innerHTML = [
    "不是 ProductionValidated。",
    "不是 Business Golden Loop Readiness。",
    "SDBR 反馈不会自动修改 DDAE 已批准主设置。",
    "没有生产级物料可行性声明。",
    "DDAE 不拥有 SDBR 可执行 routing、工序时长、资源日历或工单执行状态。",
  ].map(item => `<span>${escapeHtml(item)}</span>`).join("");
}

async function loadPublicDemoGoldenLoop() {
  const response = await fetch("/api/public-demo-golden-loop", {
    headers: { Accept: "application/json" },
  });
  if (!response.ok) {
    throw new Error(`公开演示闭环接口失败：${response.status}`);
  }

  state.publicDemoGoldenLoop = await response.json();
  renderPublicDemoGoldenLoop(state.publicDemoGoldenLoop);
}

async function loadAdventureWorksProductDemo() {
  const response = await fetch("/api/adventureworks-product-demo-v1", {
    headers: { Accept: "application/json" },
  });
  if (!response.ok) {
    throw new Error(`AdventureWorks ProductDemoMode 接口失败：${response.status}`);
  }

  state.adventureWorksProductDemo = await response.json();
  renderAdventureWorksProductDemo(state.adventureWorksProductDemo);
}

async function writePublicDemoPayload() {
  const status = byId("public-demo-handoff-chip");
  status.className = "status-chip is-warning";
  status.textContent = "正在写出";
  const response = await fetch("/api/public-demo-golden-loop/write-payload", {
    method: "POST",
    headers: { Accept: "application/json" },
  });
  if (!response.ok) {
    throw new Error(`公开演示 payload 写出失败：${response.status}`);
  }

  const result = await response.json();
  state.publicDemoGoldenLoop = {
    ...state.publicDemoGoldenLoop,
    payloadPreview: result.payload,
    handoff: result.handoff,
  };
  renderPublicDemoGoldenLoop(state.publicDemoGoldenLoop);
}

function stageKpi(label, value, note) {
  return `<div><span>${escapeHtml(label)}</span><strong>${value}</strong><small>${escapeHtml(note)}</small></div>`;
}

function syncHistorySelectionState(history) {
  const inventory = history.inventoryBuffers || [];
  const sizing = history.ddmrpSizingSnapshots || [];
  const timeBuffers = history.timeBuffers || [];
  const capacity = history.capacityBuffers || [];
  const selectAvailable = (selected, values) => values.includes(selected) ? selected : valueOr(values[0], null);

  const controlPoints = [...new Set([
    ...inventory.map(item => item.controlPoint),
    ...sizing.map(item => item.controlPoint),
  ])];
  state.selectedHistoryControlPoint = selectAvailable(state.selectedHistoryControlPoint, controlPoints);

  const skus = [...new Set([
    ...inventory
      .filter(item => item.controlPoint === state.selectedHistoryControlPoint)
      .map(item => item.sku),
    ...sizing
      .filter(item => item.controlPoint === state.selectedHistoryControlPoint)
      .map(item => item.sku),
  ])];
  state.selectedHistoryInventorySku = selectAvailable(state.selectedHistoryInventorySku, skus);

  const selectedInventory = inventory.find(item =>
    item.controlPoint === state.selectedHistoryControlPoint && item.sku === state.selectedHistoryInventorySku);
  const inventoryWeeks = (selectedInventory?.points || [])
    .filter(point => isFiniteHistoryValue(point.weekOffset))
    .map(point => Number(point.weekOffset));
  state.selectedHistoryInventoryWeekOffset = inventoryWeeks.includes(state.selectedHistoryInventoryWeekOffset)
    ? state.selectedHistoryInventoryWeekOffset
    : valueOr(inventoryWeeks[inventoryWeeks.length - 1], null);

  const sizingSnapshots = sizing
    .filter(item => item.controlPoint === state.selectedHistoryControlPoint && item.sku === state.selectedHistoryInventorySku)
    .map(item => item.snapshotId);
  state.selectedHistorySizingSnapshot = selectAvailable(state.selectedHistorySizingSnapshot, sizingSnapshots);
  state.selectedHistoryTimeBufferId = selectAvailable(
    state.selectedHistoryTimeBufferId,
    timeBuffers.map(item => item.bufferId));

  const capacityCodes = capacity.map(item => item.resourceCode);
  if (!capacityCodes.includes(state.selectedHistoryCapacityResource)) {
    const defaultCapacity = capacity.find(item => item.resourceCode === "RES-AIT" && item.relationshipRole === "UpstreamProtection")
      || capacity.find(item => item.relationshipRole === "UpstreamProtection")
      || capacity[0];
    state.selectedHistoryCapacityResource = valueOr(defaultCapacity?.resourceCode, null);
  }

  state.historySelection.inventoryControlPoint = state.selectedHistoryControlPoint;
  state.historySelection.inventorySku = state.selectedHistoryInventorySku;
  state.historySelection.timeBufferId = state.selectedHistoryTimeBufferId;
  state.historySelection.sizingControlPoint = state.selectedHistoryControlPoint;
  state.historySelection.sizingSku = state.selectedHistoryInventorySku;
  state.historySelection.sizingSnapshotId = state.selectedHistorySizingSnapshot;
  state.historySelection.capacityResourceCode = state.selectedHistoryCapacityResource;
}

function historyControlPointLabel(controlPoint) {
  if (controlPoint === "关键进口 FPGA 库存控制点") return "关键进口 FPGA 独立库存控制点";
  return controlPoint || "未命名控制点";
}

function historyCapacityRoleLabel(item) {
  if (item.relationshipRole === "UpstreamProtection") return "上游保护";
  if (item.relationshipRole === "CcrUtilization") return "CCR 利用率参照";
  return capacityRoleLabel(item.relationshipRole, item.protectedCcrResourceCode);
}

function historyOptionButton(attribute, value, label, note, isSelected) {
  return `
    <button class="inventory-option${isSelected ? " is-selected" : ""}" type="button" data-${attribute}="${escapeHtml(value)}" aria-pressed="${isSelected}">
      <span class="option-radio" aria-hidden="true"></span>
      <span><strong>${escapeHtml(label)}</strong><small>${escapeHtml(note)}</small></span>
    </button>`;
}

function renderHistoryWorkspaceOptions(history) {
  const inventory = history.inventoryBuffers || [];
  const sizing = history.ddmrpSizingSnapshots || [];
  const timeBuffers = history.timeBuffers || [];
  const capacity = history.capacityBuffers || [];
  const emptyOptions = label => `<p class="history-empty-options">${escapeHtml(label)}</p>`;
  const controlPoints = [...new Set([
    ...inventory.map(item => item.controlPoint),
    ...sizing.map(item => item.controlPoint),
  ])];
  const controlPointOptions = controlPoints.length
    ? controlPoints.map(controlPoint => historyOptionButton(
        "history-control-point",
        controlPoint,
        historyControlPointLabel(controlPoint),
        `${new Set([...inventory, ...sizing].filter(item => item.controlPoint === controlPoint).map(item => item.sku)).size} 个 SKU`,
        controlPoint === state.selectedHistoryControlPoint)).join("")
    : emptyOptions("暂无库存控制点证据");
  byId("history-inventory-control-point-options").innerHTML = controlPointOptions;
  byId("history-sizing-control-point-options").innerHTML = controlPointOptions;

  const inventorySkus = inventory.filter(item => item.controlPoint === state.selectedHistoryControlPoint);
  byId("history-inventory-sku-options").innerHTML = inventorySkus.length
    ? inventorySkus.map(item => historyOptionButton(
        "history-inventory-sku",
        item.sku,
        item.sku,
        item.name,
        item.sku === state.selectedHistoryInventorySku)).join("")
    : emptyOptions("暂无库存 SKU 证据");

  const sizingSkus = [...new Map(sizing
    .filter(item => item.controlPoint === state.selectedHistoryControlPoint)
    .map(item => [item.sku, item])).values()];
  byId("history-sizing-sku-options").innerHTML = sizingSkus.length
    ? sizingSkus.map(item => historyOptionButton(
        "history-inventory-sku",
        item.sku,
        item.sku,
        item.name,
        item.sku === state.selectedHistoryInventorySku)).join("")
    : emptyOptions("暂无定容 SKU 证据");

  const sizingSnapshots = sizing.filter(item =>
    item.controlPoint === state.selectedHistoryControlPoint && item.sku === state.selectedHistoryInventorySku);
  byId("history-sizing-snapshot-options").innerHTML = sizingSnapshots.length
    ? sizingSnapshots.map(item => historyOptionButton(
        "history-sizing-snapshot",
        item.snapshotId,
        item.snapshotId,
        `生效周第 ${number(item.effectiveFromWeekOffset)} 至 ${number(item.effectiveThroughWeekOffset)} 周`,
        item.snapshotId === state.selectedHistorySizingSnapshot)).join("")
    : emptyOptions("暂无历史定容快照");

  byId("history-time-buffer-options").innerHTML = timeBuffers.length
    ? timeBuffers.map(item => historyOptionButton(
        "history-time-buffer-id",
        item.bufferId,
        historyControlPointLabel(item.controlPoint),
        item.protectedActivity,
        item.bufferId === state.selectedHistoryTimeBufferId)).join("")
    : emptyOptions("暂无时间缓冲证据");

  byId("history-capacity-resource-options").innerHTML = capacity.length
    ? capacity.map(item => historyOptionButton(
        "history-capacity-resource",
        item.resourceCode,
        item.resourceName,
        `${item.resourceCode} · ${historyCapacityRoleLabel(item)}`,
        item.resourceCode === state.selectedHistoryCapacityResource)).join("")
    : emptyOptions("暂无能力资源证据");
}

function contiguousEvidenceSegments(points, predicate) {
  return points.reduce((segments, point) => {
    if (!predicate(point)) {
      if (segments.length && segments[segments.length - 1].length) segments.push([]);
      return segments;
    }
    if (!segments.length) segments.push([]);
    segments[segments.length - 1].push(point);
    return segments;
  }, []).filter(segment => segment.length > 0);
}

function buildLinearAreaPath(lowerPoints, upperPoints) {
  const points = [...lowerPoints, ...upperPoints];
  if (lowerPoints.length !== upperPoints.length || lowerPoints.length === 0 ||
      points.some(point => !Number.isFinite(point.x) || !Number.isFinite(point.y))) return "";
  const upper = upperPoints.map((point, index) => `${index ? "L" : "M"} ${point.x},${point.y}`).join(" ");
  const lower = [...lowerPoints].reverse().map(point => `L ${point.x},${point.y}`).join(" ");
  return `${upper} ${lower} Z`;
}

function buildHistoryLinePath(points) {
  if (!points.length || points.some(point => !Number.isFinite(point.x) || !Number.isFinite(point.y))) return "";
  return points.map((point, index) => `${index ? "L" : "M"} ${point.x},${point.y}`).join(" ");
}

function isFiniteHistoryValue(value) {
  return value !== null && value !== undefined && Number.isFinite(Number(value));
}

function historyValueScale(values, top, bottom) {
  const finiteValues = values.filter(isFiniteHistoryValue).map(Number);
  if (!finiteValues.length) return null;
  const minimum = Math.min(0, ...finiteValues);
  const maximum = Math.max(0, ...finiteValues);
  const span = Math.max(1, maximum - minimum);
  return {
    minimum,
    maximum,
    y: value => bottom - ((Number(value) - minimum) / span) * (bottom - top),
  };
}

function renderHistoryMissing(hostId, detail = "所选对象在当前历史窗口内没有可绘制证据") {
  byId(hostId).innerHTML = `<div class="history-chart-empty"><strong>证据缺失</strong><span>${escapeHtml(detail)}</span></div>`;
}

function renderHistoryBufferOverview(history) {
  const inventory = history.inventoryBuffers || [];
  const sizing = history.ddmrpSizingSnapshots || [];
  const timeBuffers = history.timeBuffers || [];
  const capacity = history.capacityBuffers || [];
  const controlPoints = [...new Set(inventory.map(item => item.controlPoint))];
  byId("history-buffer-overview").innerHTML = [
    ["库存控制点", number(controlPoints.length), controlPoints.length ? controlPoints.map(historyControlPointLabel).join("、") : "证据缺失"],
    ["库存 SKU", number(inventory.length), "图、表和原因使用同一选择"],
    ["时间缓冲", number(timeBuffers.length), "五段历史与异常费用"],
    ["定容快照", number(sizing.length), "读取后端定容证据"],
    ["能力资源", number(capacity.length), "AIT 上游保护优先"],
  ].map(item => stageKpi(...item)).join("");
}

function renderHistoryInventoryBuffer(history) {
  const item = (history.inventoryBuffers || []).find(candidate =>
    candidate.controlPoint === state.selectedHistoryControlPoint && candidate.sku === state.selectedHistoryInventorySku);
  if (!item || !item.points?.length) {
    renderHistoryMissing("history-inventory-position-chart");
    renderHistoryMissing("history-inventory-volatility-chart");
    renderHistoryInventoryEvidenceDetail(history, null, null);
    return;
  }
  const weekScale = historyWeekXScale(item.points);
  renderHistoryInventoryPositionChart(history, item, weekScale);
  renderHistoryInventoryVolatilityChart(history, item, weekScale, historyDemandAxisMaximum(history, item.controlPoint));
  renderHistoryInventoryEvidenceDetail(history, item, item.points.find(point =>
    Number(point.weekOffset) === state.selectedHistoryInventoryWeekOffset));
}

function historyDemandAxisMaximum(history, controlPoint) {
  const values = (history.inventoryBuffers || [])
    .filter(item => item.controlPoint === controlPoint)
    .flatMap(item => (item.points || []).flatMap(point => [point.actualDemand, point.demandSpikeThreshold]))
    .filter(isFiniteHistoryValue)
    .map(Number);
  return values.length ? Math.ceil(Math.max(...values) * 1.08) : null;
}

function renderHistoryInventoryEvidenceDetail(history, item, point) {
  const host = byId("history-inventory-evidence-detail");
  if (!item || !point) {
    host.innerHTML = `<div class="table-empty"><strong>证据缺失</strong></div>`;
    return;
  }
  const checks = (point.evidenceChecks || []).map(check => `
    <li class="history-evidence-check ${check.status === "Complete" ? "is-complete" : "is-missing"}">
      <strong>${escapeHtml(check.label)}</strong><span>${escapeHtml(check.detail)}</span>
    </li>`).join("");
  host.innerHTML = `<div class="history-chart-heading"><strong>${escapeHtml(point.periodStartDate)} · ${escapeHtml(item.sku)} ${escapeHtml(item.name)}</strong><span>${escapeHtml(evidenceStatusLabel(point.evidenceStatus))}</span></div>
    <dl class="history-zone-metadata"><div><span>参数快照</span><strong>${escapeHtml(point.parameterSnapshotId || "证据缺失")}</strong></div><div><span>当周事件</span><strong>${escapeHtml(point.weeklyEvent || "无事件")}</strong></div><div><span>参数变更原因</span><strong>${escapeHtml(point.parameterChangeReason || "本周无参数变更")}</strong></div></dl>
    <ul class="history-evidence-check-list">${checks || "<li class=\"history-evidence-check is-missing\"><strong>证据检查</strong><span>证据缺失</span></li>"}</ul>`;
}

function capacityUtilizationBandClass(band) {
  return ({
    Green: "is-green",
    Yellow: "is-yellow",
    Red: "is-red",
    DeepRed: "is-deep-red",
  })[band] || "is-evidence-missing";
}

function capacityUtilizationBandLabel(band) {
  return ({
    Green: "绿区（0–60%）",
    Yellow: "黄区（>60–80%）",
    Red: "红区（>80–100%）",
    DeepRed: "深红区（>100%）",
    EvidenceMissing: "证据缺失",
  })[band] || "证据缺失";
}

function capacityUtilizationBandChip(measure) {
  const band = measure?.utilizationBand || "EvidenceMissing";
  return `<span class="capacity-band-chip ${capacityUtilizationBandClass(band)}">${escapeHtml(capacityUtilizationBandLabel(band))}</span>`;
}

function renderCapacityBandDistribution(hostId, observations) {
  const host = byId(hostId);
  if (!host) return;
  const bands = ["Green", "Yellow", "Red", "DeepRed"];
  const counts = Object.fromEntries(bands.map(band => [band, 0]));
  observations.forEach(observation => {
    if (Object.hasOwn(counts, observation.band)) counts[observation.band] += Number(observation.count || 0);
  });
  const total = bands.reduce((sum, band) => sum + counts[band], 0);
  if (!total) {
    host.innerHTML = `<div class="table-empty"><strong>没有完整的上游能力观测证据</strong></div>`;
    return;
  }
  const segments = bands.map(band => {
    const share = counts[band] * 100 / total;
    return `<span class="capacity-distribution-segment ${capacityUtilizationBandClass(band)}" style="width:${share}%" title="${escapeHtml(capacityUtilizationBandLabel(band))} · ${number(counts[band])} 个观测"></span>`;
  }).join("");
  const legend = bands.map(band => `<span><i class="${capacityUtilizationBandClass(band)}"></i>${escapeHtml(capacityUtilizationBandLabel(band))}<strong>${number(counts[band])}</strong></span>`).join("");
  host.innerHTML = `<div class="capacity-distribution-bar">${segments}</div><div class="capacity-distribution-legend">${legend}</div>`;
}

function historyWeekXScale(points, width = 920, left = 58, right = 20) {
  const coordinates = points.map((point, index) => ({
    weekOffset: point.weekOffset,
    x: left + (index * (width - left - right)) / Math.max(1, points.length - 1),
  }));
  return {
    width,
    left,
    right,
    domain: coordinates.map(point => point.weekOffset).join(","),
    x: index => coordinates[index] ? coordinates[index].x : left,
  };
}

function renderHistoryInventoryPositionChart(history, item, weekScale) {
  const host = byId("history-inventory-position-chart");
  const points = item.points;
  const indexed = points.map((point, index) => ({ point, index }));
  const values = points.flatMap(point => [
    point.topOfRed,
    point.topOfYellow,
    point.topOfGreen,
    point.endingOnHand,
    point.netFlow,
  ]).filter(isFiniteHistoryValue);
  const height = 320;
  const top = 22;
  const bottom = 270;
  const scale = historyValueScale(values, top, bottom);
  if (!scale) {
    renderHistoryMissing("history-inventory-position-chart", "所选对象在当前历史窗口内没有库存位置证据");
    return;
  }

  const x = weekScale.x;
  const zoneSegments = contiguousEvidenceSegments(indexed, entry =>
    hasValidBufferZoneEvidence(entry.point));
  const endingSegments = contiguousEvidenceSegments(indexed, entry =>
    isFiniteHistoryValue(entry.point.endingOnHand));
  const netFlowSegments = contiguousEvidenceSegments(indexed, entry =>
    isFiniteHistoryValue(entry.point.netFlow));

  const zonePaths = zoneSegments.filter(segment => segment.length > 1).map((segment, segmentIndex) => {
    const redLower = segment.map(entry => ({ x: x(entry.index), y: scale.y(0) }));
    const redUpper = segment.map(entry => ({ x: x(entry.index), y: scale.y(entry.point.topOfRed) }));
    const yellowUpper = segment.map(entry => ({ x: x(entry.index), y: scale.y(entry.point.topOfYellow) }));
    const greenUpper = segment.map(entry => ({ x: x(entry.index), y: scale.y(entry.point.topOfGreen) }));
    const linearSegments = new Set([
      ...monotoneCrossingSegments(redLower, redUpper),
      ...monotoneCrossingSegments(redUpper, yellowUpper),
      ...monotoneCrossingSegments(yellowUpper, greenUpper),
    ]);
    const attributes = `data-series-segment="${segmentIndex}" data-week-start="${number(segment[0].point.weekOffset)}" data-week-end="${number(segment[segment.length - 1].point.weekOffset)}"`;
    return `
      <path class="history-zone-fill is-red" ${attributes} d="${buildMonotoneAreaPath(redLower, redUpper, linearSegments)}"></path>
      <path class="history-zone-fill is-yellow" ${attributes} d="${buildMonotoneAreaPath(redUpper, yellowUpper, linearSegments)}"></path>
      <path class="history-zone-fill is-green" ${attributes} d="${buildMonotoneAreaPath(yellowUpper, greenUpper, linearSegments)}"></path>`;
  }).join("");
  const zoneMarkers = zoneSegments.filter(segment => segment.length === 1).map(segment => {
    const entry = segment[0];
    const attributes = `data-week-offset="${number(entry.point.weekOffset)}" cx="${x(entry.index)}" r="3.5"`;
    return `
      <circle class="history-zone-point is-red" ${attributes} cy="${scale.y(entry.point.topOfRed)}"><title>${escapeHtml(entry.point.periodStartDate)} 红区上沿 ${number(entry.point.topOfRed)}</title></circle>
      <circle class="history-zone-point is-yellow" ${attributes} cy="${scale.y(entry.point.topOfYellow)}"><title>${escapeHtml(entry.point.periodStartDate)} 黄区上沿 ${number(entry.point.topOfYellow)}</title></circle>
      <circle class="history-zone-point is-green" ${attributes} cy="${scale.y(entry.point.topOfGreen)}"><title>${escapeHtml(entry.point.periodStartDate)} 绿区上沿 ${number(entry.point.topOfGreen)}</title></circle>`;
  }).join("");
  const linePaths = (segments, cssClass, read) => segments.filter(segment => segment.length > 1).map((segment, segmentIndex) =>
    `<path class="history-series-line ${cssClass}" data-series-segment="${segmentIndex}" data-week-start="${number(segment[0].point.weekOffset)}" data-week-end="${number(segment[segment.length - 1].point.weekOffset)}" d="${buildMonotonePath(segment.map(entry => ({ x: x(entry.index), y: scale.y(read(entry.point)) })))}"></path>`).join("");
  const lineMarkers = (segments, cssClass, read, label) => segments.filter(segment => segment.length === 1).map(segment => {
    const entry = segment[0];
    return `<circle class="history-series-point ${cssClass}" data-week-offset="${number(entry.point.weekOffset)}" cx="${x(entry.index)}" cy="${scale.y(read(entry.point))}" r="3.5"><title>${escapeHtml(entry.point.periodStartDate)} ${escapeHtml(label)} ${number(read(entry.point))}</title></circle>`;
  }).join("");
  const endingPaths = linePaths(endingSegments, "is-on-hand", point => point.endingOnHand);
  const netFlowPaths = linePaths(netFlowSegments, "is-net-flow", point => point.netFlow);
  const endingMarkers = lineMarkers(endingSegments, "is-on-hand", point => point.endingOnHand, "期末在手库存");
  const netFlowMarkers = lineMarkers(netFlowSegments, "is-net-flow", point => point.netFlow, "净流量位置");
  const gapMarkers = indexed
    .filter(entry => !hasValidBufferZoneEvidence(entry.point) || [
      entry.point.endingOnHand,
      entry.point.netFlow,
    ].some(value => !isFiniteHistoryValue(value)))
    .map(entry => `<g class="history-evidence-gap" data-week-offset="${number(entry.point.weekOffset)}"><line x1="${x(entry.index)}" y1="${top}" x2="${x(entry.index)}" y2="${bottom}"></line><text x="${x(entry.index)}" y="${top + 12}" text-anchor="middle">证据缺口</text></g>`)
    .join("");
  const periodLabels = points.map((point, index) => index % Math.max(1, Math.ceil(points.length / 8)) === 0 || index === points.length - 1
    ? `<text class="history-axis-label" x="${x(index)}" y="296" text-anchor="middle">${escapeHtml(point.periodStartDate)}</text>`
    : "").join("");
  const evidenceRows = points.map(point => `<tr data-history-inventory-week="${number(point.weekOffset)}"${Number(point.weekOffset) === state.selectedHistoryInventoryWeekOffset ? " class=\"is-selected\"" : ""}>
    <td>${escapeHtml(point.periodStartDate)}</td><td>${metricOrEvidenceMissing(point.endingOnHand)}</td><td>${metricOrEvidenceMissing(point.openSupply)}</td><td>${metricOrEvidenceMissing(point.qualifiedDemand)}</td><td>${metricOrEvidenceMissing(point.netFlow)}</td><td>${escapeHtml(metricOrEvidenceMissing(point.parameterSnapshotId))}</td><td>${escapeHtml(businessEvidenceLabel(point.cause))}</td><td>${escapeHtml(evidenceStatusLabel(point.evidenceStatus))}</td></tr>`).join("");

  host.innerHTML = `
    <div class="history-chart-heading"><strong>${escapeHtml(historyControlPointLabel(item.controlPoint))} · ${escapeHtml(item.sku)} ${escapeHtml(item.name)} · ${number(history.observedTrendWeeks)} 周历史趋势</strong><span>累计提前期详细证据窗口：${number(item.detailWindowWeeks)} 周</span></div>
    <svg class="history-evidence-svg" data-history-week-domain="${escapeHtml(weekScale.domain)}" viewBox="0 0 ${weekScale.width} ${height}" role="img" aria-label="库存缓冲历史水位与动态区域">
      <line class="history-axis-line" x1="${weekScale.left}" y1="${bottom}" x2="${weekScale.width - weekScale.right}" y2="${bottom}"></line>
      ${zonePaths}${zoneMarkers}${endingPaths}${endingMarkers}${netFlowPaths}${netFlowMarkers}${gapMarkers}${periodLabels}
      <text class="history-axis-label" x="${weekScale.left - 8}" y="${scale.y(scale.maximum) + 4}" text-anchor="end">${number(scale.maximum)}</text>
      <text class="history-axis-label" x="${weekScale.left - 8}" y="${scale.y(0) + 4}" text-anchor="end">0</text>
    </svg>
    <div class="history-chart-legend"><span><i class="zone red"></i>红区</span><span><i class="zone yellow"></i>黄区</span><span><i class="zone green"></i>绿区</span><span><i class="line on-hand"></i>期末在手库存</span><span><i class="line net-flow"></i>净流量位置</span></div>
    <div class="table-scroll history-chart-table"><table class="data-table"><thead><tr><th>期间</th><th>期末在手库存</th><th>开放供应</th><th>合格需求</th><th>净流量位置</th><th>参数快照</th><th>原因</th><th>证据</th></tr></thead><tbody>${evidenceRows}</tbody></table></div>`;
}

function renderHistoryInventoryVolatilityChart(history, item, weekScale, demandAxisMaximum) {
  const host = byId("history-inventory-volatility-chart");
  const points = item.points;
  const indexed = points.map((point, index) => ({ point, index }));
  const validDemandValue = value => isFiniteHistoryValue(value) && Number(value) >= 0;
  const validThresholdValue = value => isFiniteHistoryValue(value) && Number(value) > 0;
  const values = points.flatMap(point => [
    ...(validDemandValue(point.actualDemand) ? [point.actualDemand] : []),
    ...(validThresholdValue(point.demandSpikeThreshold) ? [point.demandSpikeThreshold] : []),
  ]);
  const height = 260;
  const top = 20;
  const bottom = 212;
  const scale = historyValueScale([0, ...(isFiniteHistoryValue(demandAxisMaximum) ? [demandAxisMaximum] : values)], top, bottom);
  if (!scale) {
    renderHistoryMissing("history-inventory-volatility-chart", "所选对象在当前历史窗口内没有需求波动证据");
    return;
  }

  const x = weekScale.x;
  const demandSegments = contiguousEvidenceSegments(indexed, entry =>
    validDemandValue(entry.point.actualDemand));
  const thresholdSegments = contiguousEvidenceSegments(indexed, entry =>
    validThresholdValue(entry.point.demandSpikeThreshold));
  if (!demandSegments.length && !thresholdSegments.length) {
    renderHistoryMissing("history-inventory-volatility-chart", "所选对象在当前历史窗口内没有需求波动证据");
    return;
  }

  const demandAreas = demandSegments.filter(segment => segment.length > 1).map((segment, segmentIndex) => {
    const lower = segment.map(entry => ({ x: x(entry.index), y: scale.y(0) }));
    const upper = segment.map(entry => ({ x: x(entry.index), y: scale.y(entry.point.actualDemand) }));
    return `<path class="history-demand-area" data-series-segment="${segmentIndex}" data-week-start="${number(segment[0].point.weekOffset)}" data-week-end="${number(segment[segment.length - 1].point.weekOffset)}" d="${buildMonotoneAreaPath(lower, upper, monotoneCrossingSegments(lower, upper))}"></path>`;
  }).join("");
  const demandMarkers = demandSegments.flat().map(entry =>
    `<circle class="history-demand-point" data-week-offset="${number(entry.point.weekOffset)}" data-value="${number(entry.point.actualDemand)}" cx="${x(entry.index)}" cy="${scale.y(entry.point.actualDemand)}" r="2.8"></circle>`).join("");
  const thresholdPaths = thresholdSegments.filter(segment => segment.length > 1).map((segment, segmentIndex) =>
    `<path class="history-demand-threshold" data-series-segment="${segmentIndex}" data-week-start="${number(segment[0].point.weekOffset)}" data-week-end="${number(segment[segment.length - 1].point.weekOffset)}" d="${buildMonotonePath(segment.map(entry => ({ x: x(entry.index), y: scale.y(entry.point.demandSpikeThreshold) })))}"></path>`).join("");
  const thresholdMarkers = thresholdSegments.filter(segment => segment.length === 1).map(segment => {
    const entry = segment[0];
    return `<circle class="history-demand-threshold-point" data-week-offset="${number(entry.point.weekOffset)}" cx="${x(entry.index)}" cy="${scale.y(entry.point.demandSpikeThreshold)}" r="3.5"><title>${escapeHtml(entry.point.periodStartDate)} 需求尖峰阈值 ${number(entry.point.demandSpikeThreshold)}</title></circle>`;
  }).join("");
  const gapMarkers = indexed
    .filter(entry => !validDemandValue(entry.point.actualDemand)
      || !validThresholdValue(entry.point.demandSpikeThreshold))
    .map(entry => `<g class="history-evidence-gap" data-week-offset="${number(entry.point.weekOffset)}"><line x1="${x(entry.index)}" y1="${top}" x2="${x(entry.index)}" y2="${bottom}"></line><text x="${x(entry.index)}" y="${top + 12}" text-anchor="middle">证据缺口</text></g>`)
    .join("");
  const periodLabels = points.map((point, index) => index % Math.max(1, Math.ceil(points.length / 8)) === 0 || index === points.length - 1
    ? `<text class="history-axis-label" x="${x(index)}" y="238" text-anchor="middle">${escapeHtml(point.periodStartDate)}</text>`
    : "").join("");
  const evidenceRows = points.map(point => row([
    escapeHtml(point.periodStartDate),
    metricOrEvidenceMissing(point.actualDemand),
    metricOrEvidenceMissing(point.demandSpikeThreshold),
    escapeHtml(evidenceStatusLabel(point.evidenceStatus)),
  ])).join("");

  host.innerHTML = `
    <div class="history-chart-heading"><strong>${escapeHtml(historyControlPointLabel(item.controlPoint))} · ${escapeHtml(item.sku)} ${escapeHtml(item.name)} · 实际需求波动与尖峰阈值 · ${number(history.observedTrendWeeks)} 周历史趋势</strong><span>阈值来自后端历史证据，不在前端重算</span></div>
    <svg class="history-evidence-svg history-volatility-svg" data-history-week-domain="${escapeHtml(weekScale.domain)}" data-history-demand-axis-max="${metricOrEvidenceMissing(demandAxisMaximum)}" data-zero-y="${scale.y(0)}" viewBox="0 0 ${weekScale.width} ${height}" role="img" aria-label="历史实际需求波动与后端尖峰阈值">
      <line class="history-axis-line" x1="${weekScale.left}" y1="${bottom}" x2="${weekScale.width - weekScale.right}" y2="${bottom}"></line>
      ${demandAreas}${thresholdPaths}${thresholdMarkers}${demandMarkers}${gapMarkers}${periodLabels}
    </svg>
    <div class="history-chart-legend"><span><i class="area demand"></i>实际需求波动</span><span><i class="line demand-threshold"></i>后端需求尖峰阈值</span></div>
    <div class="table-scroll history-chart-table"><table class="data-table"><thead><tr><th>期间</th><th>实际需求</th><th>尖峰阈值</th><th>证据</th></tr></thead><tbody>${evidenceRows}</tbody></table></div>`;
}

function historyGreenDriverLabel(value) {
  const labels = {
    OrderCycle: "订货周期",
    MinimumOrderQuantity: "最小订购量",
    LeadTime: "提前期",
  };
  return labels[value] || businessEvidenceLabel(value);
}

function renderHistoryDdmrpZoneSvg(item) {
  if (!item?.sizing?.zones) {
    renderHistoryMissing("history-ddmrp-zone-chart", "旧版本缺少提前期因子，不能生成定容图");
    return;
  }
  const red = item.sizing.zones.red;
  const yellow = item.sizing.zones.yellow;
  const green = item.sizing.zones.green;
  const total = item.sizing.zones.topOfGreen;
  if (![red, yellow, green, total].every(isFiniteHistoryValue) || Number(total) <= 0) {
    renderHistoryMissing("history-ddmrp-zone-chart", "所选快照没有完整的后端定容区域证据");
    return;
  }

  const width = 660;
  const height = 320;
  const top = 24;
  const bottom = 282;
  const plotHeight = bottom - top;
  const barX = 74;
  const barWidth = 190;
  const redHeight = Number(red) * plotHeight / Number(total);
  const yellowHeight = Number(yellow) * plotHeight / Number(total);
  const greenHeight = Number(green) * plotHeight / Number(total);
  const redY = bottom - redHeight;
  const yellowY = redY - yellowHeight;
  const greenY = yellowY - greenHeight;
  const averageOnHand = item.averageOnHand;
  const averageY = isFiniteHistoryValue(averageOnHand)
    ? Math.max(top, Math.min(bottom, bottom - Number(averageOnHand) * plotHeight / Number(total)))
    : null;
  const averageMarker = averageY === null ? "" : `
    <line class="history-average-marker" x1="${barX - 12}" y1="${averageY}" x2="${barX + barWidth + 28}" y2="${averageY}"></line>
    <text class="history-average-label" x="${barX + barWidth + 36}" y="${averageY + 4}">平均现有量 ${number(averageOnHand)}</text>`;
  const metadata = [
    ["控制点", historyControlPointLabel(item.controlPoint)],
    ["SKU", `${item.sku} ${item.name}`],
    ["快照", item.snapshotId],
    ["生效周段", `第 ${number(item.effectiveFromWeekOffset)} 至 ${number(item.effectiveThroughWeekOffset)} 周`],
    ["来源截止", item.asOfUtc],
    ["绿色区驱动", historyGreenDriverLabel(item.sizing.greenDriver)],
    ["总缓冲", number(total)],
    ["平均现有量", metricOrEvidenceMissing(item.averageOnHand)],
    ["证据", evidenceStatusLabel(item.evidenceStatus)],
  ].map(([label, value]) => `<div><span>${escapeHtml(label)}</span><strong>${escapeHtml(value)}</strong></div>`).join("");

  byId("history-ddmrp-zone-chart").innerHTML = `
    <div class="history-zone-layout">
      <svg class="history-zone-svg" viewBox="0 0 ${width} ${height}" role="img" aria-label="历史 DDMRP 红黄绿定容区">
        <rect class="history-zone-block is-green" x="${barX}" y="${greenY}" width="${barWidth}" height="${greenHeight}"></rect>
        <rect class="history-zone-block is-yellow" x="${barX}" y="${yellowY}" width="${barWidth}" height="${yellowHeight}"></rect>
        <rect class="history-zone-block is-red" x="${barX}" y="${redY}" width="${barWidth}" height="${redHeight}"></rect>
        <text class="history-zone-value" x="${barX + barWidth / 2}" y="${greenY + greenHeight / 2 + 4}">绿区 ${number(green)}</text>
        <text class="history-zone-value" x="${barX + barWidth / 2}" y="${yellowY + yellowHeight / 2 + 4}">黄区 ${number(yellow)}</text>
        <text class="history-zone-value is-light" x="${barX + barWidth / 2}" y="${redY + redHeight / 2 + 4}">红区 ${number(red)}</text>
        ${averageMarker}
      </svg>
      <div class="history-zone-metadata">${metadata}</div>
    </div>`;
}

function renderHistoryDdmrpSizingTrace(history) {
  const item = (history.ddmrpSizingSnapshots || []).find(candidate =>
    candidate.controlPoint === state.selectedHistoryControlPoint
      && candidate.sku === state.selectedHistoryInventorySku
      && candidate.snapshotId === state.selectedHistorySizingSnapshot);
  if (!item) {
    byId("history-ddmrp-input-summary").innerHTML = `<div><dt>定容证据</dt><dd>证据缺失</dd></div>`;
    byId("history-ddmrp-sizing-body").innerHTML = emptyRow("旧版本缺少提前期因子，不能生成定容明细", 4);
    renderHistoryMissing("history-ddmrp-zone-chart", "所选控制点、SKU 或快照没有历史定容证据");
    return;
  }

  const setting = item.setting || {};
  byId("history-ddmrp-input-summary").innerHTML = [
    ["控制点", historyControlPointLabel(item.controlPoint)],
    ["SKU", `${item.sku} ${item.name}`],
    ["参数快照", item.snapshotId],
    ["提前期因子", setting.leadTimeFactor == null ? "证据缺失" : number(setting.leadTimeFactor)],
    ["变异因子", metricOrEvidenceMissing(setting.variabilityFactor)],
    ["最小订购量", metricOrEvidenceMissing(setting.minimumOrderQuantity)],
    ["需求调整因子", metricOrEvidenceMissing(setting.demandAdjustmentFactor)],
    ["区域调整因子", metricOrEvidenceMissing(setting.zoneAdjustmentFactor)],
    ["ADU 来源", metricOrEvidenceMissing(setting.aduSource || item.aduSource)],
    ["DLT 来源", metricOrEvidenceMissing(setting.dltSource)],
    ["参数变更原因", metricOrEvidenceMissing(item.parameterChangeReason)],
    ["生效周段", `第 ${number(item.effectiveFromWeekOffset)} 至 ${number(item.effectiveThroughWeekOffset)} 周`],
    ["订货周期", setting.orderCycleDays == null ? "证据缺失" : `${number(setting.orderCycleDays)} 天`],
    ["来源", businessEvidenceLabel(item.sourceAuthority)],
    ["来源截止", item.asOfUtc],
    ["证据状态", evidenceStatusLabel(item.evidenceStatus)],
  ].map(([label, value]) => `<div><dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value)}</dd></div>`).join("");
  byId("history-ddmrp-sizing-body").innerHTML = item.sizingLines.length
    ? item.sizingLines.map(line => row([
        escapeHtml(line.component),
        escapeHtml(line.formula),
        number(line.value),
        escapeHtml(businessEvidenceLabel(line.explanation)),
      ])).join("")
    : emptyRow("旧版本缺少提前期因子，不能生成定容明细", 4);
  renderHistoryDdmrpZoneSvg(item);
}

function renderDdmrpStandardReference(reference) {
  const inputs = reference?.inputs || {};
  const zones = reference?.zones || {};
  const source = metricOrEvidenceMissing(reference?.sourceAuthority);
  const evidence = evidenceStatusLabel(reference?.evidenceStatus);
  byId("ddmrp-standard-reference-status").innerHTML = `<strong>${escapeHtml(source)}</strong><span>${escapeHtml(evidence)}</span>`;

  const inputRows = [
    ["计算方式", "后端计算"],
    ["ADU", metricOrEvidenceMissing(inputs.adu)],
    ["DLT", inputs.decoupledLeadTimeDays == null ? "证据缺失" : `${number(inputs.decoupledLeadTimeDays)} 天`],
    ["提前期因子", metricOrEvidenceMissing(inputs.leadTimeFactor)],
    ["变异因子", metricOrEvidenceMissing(inputs.variabilityFactor)],
    ["订货周期", inputs.orderCycleDays == null ? "证据缺失" : `${number(inputs.orderCycleDays)} 天`],
    ["MOQ", metricOrEvidenceMissing(inputs.minimumOrderQuantity)],
    ["DAF", metricOrEvidenceMissing(inputs.demandAdjustmentFactor)],
    ["区域调整", metricOrEvidenceMissing(inputs.zoneAdjustmentFactor)],
    ["红区基础", metricOrEvidenceMissing(reference?.redBase)],
    ["红区安全量", metricOrEvidenceMissing(reference?.redSafety)],
    ["总缓冲", metricOrEvidenceMissing(reference?.totalBuffer)],
  ];
  byId("ddmrp-standard-reference-inputs").innerHTML = inputRows
    .map(([label, value]) => `<div><dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value)}</dd></div>`)
    .join("");

  const derivations = Array.isArray(reference?.derivations) ? reference.derivations : [];
  byId("ddmrp-standard-reference-derivation-body").innerHTML = derivations.length
    ? derivations.map(line => row([
        escapeHtml(metricOrEvidenceMissing(line.component)),
        escapeHtml(metricOrEvidenceMissing(line.formula)),
        escapeHtml(metricOrEvidenceMissing(line.value)),
        escapeHtml(metricOrEvidenceMissing(line.explanation)),
      ])).join("")
    : emptyRow("证据缺失：后端未提供推导明细", 4);

  const hasCompleteZones = reference?.evidenceStatus === "Complete"
    && [zones.red, zones.yellow, zones.green].every(isFiniteHistoryValue);
  if (!hasCompleteZones) {
    renderHistoryMissing("ddmrp-standard-reference-zones", "标准缓冲定容证据缺失");
    return;
  }
  const greenDriver = reference.greenDriver === "OrderCycle"
    ? "订货周期驱动"
    : `${historyGreenDriverLabel(reference.greenDriver)}驱动`;
  byId("ddmrp-standard-reference-zones").innerHTML = `
    <div class="history-standard-zone-stack" role="img" aria-label="标准缓冲参考：红区 ${number(zones.red)}，黄区 ${number(zones.yellow)}，绿区 ${number(zones.green)}">
      <div class="is-green"><span>绿区</span><strong>${number(zones.green)}</strong></div>
      <div class="is-yellow"><span>黄区</span><strong>${number(zones.yellow)}</strong></div>
      <div class="is-red"><span>红区</span><strong>${number(zones.red)}</strong></div>
    </div>
    <div class="history-standard-zone-caption"><strong>${escapeHtml(greenDriver)}</strong><span>${escapeHtml(source)} · ${escapeHtml(evidence)}</span></div>`;
}

function loadDdmrpStandardReference() {
  if (state.ddmrpStandardReference) {
    renderDdmrpStandardReference(state.ddmrpStandardReference);
    return Promise.resolve(state.ddmrpStandardReference);
  }
  if (state.ddmrpStandardReferencePromise) return state.ddmrpStandardReferencePromise;

  byId("ddmrp-standard-reference-status").textContent = "正在读取后端计算参考";
  const request = fetch("/api/ddmrp-standard-reference", { headers: { Accept: "application/json" } })
    .then(response => {
      if (!response.ok) throw new Error(`缓冲计算参考接口失败：${response.status}`);
      return response.json();
    })
    .then(reference => {
      state.ddmrpStandardReference = reference;
      state.ddmrpStandardReferencePromise = null;
      renderDdmrpStandardReference(reference);
      return reference;
    })
    .catch(error => {
      state.ddmrpStandardReferencePromise = null;
      byId("ddmrp-standard-reference-status").innerHTML = `<strong>读取失败</strong><span>${escapeHtml(error.message)}</span>`;
      throw error;
    });
  state.ddmrpStandardReferencePromise = request;
  return request;
}

function initializeDdmrpStandardReferencePanel() {
  const panel = byId("ddmrp-standard-reference-panel");
  if (!panel || panel.dataset.standardReferenceReady === "true") return;
  panel.dataset.standardReferenceReady = "true";
  panel.addEventListener("toggle", () => {
    if (!panel.open) return;
    loadDdmrpStandardReference().catch(() => {});
  });
}

function renderHistoryTimeBuffer(history) {
  const item = (history.timeBuffers || []).find(candidate => candidate.bufferId === state.selectedHistoryTimeBufferId);
  if (!item || !item.points?.length) {
    renderHistoryMissing("history-time-status-chart");
    renderHistoryMissing("history-time-cost-strip", "所选时间缓冲没有异常费用事实证据");
    return;
  }

  renderHistoryTimeStatusChart(history, item);
  renderHistoryTimeCostStrip(history, item);
}

function renderHistoryTimeStatusChart(history, item) {
  const host = byId("history-time-status-chart");
  const points = item.points;
  const indexed = points.map((point, index) => ({ point, index }));
  const validBandValue = value => isFiniteHistoryValue(value) && Number(value) >= 0;
  const bandPoints = indexed.filter(entry => {
    const point = entry.point;
    return [point.earlyCount, point.greenCount, point.yellowCount, point.redCount, point.lateCount]
      .every(validBandValue);
  });
  if (!bandPoints.length) {
    renderHistoryMissing("history-time-status-chart", "所选时间缓冲在当前历史窗口内没有五段状态证据");
    return;
  }

  const totals = bandPoints.map(entry => {
    const point = entry.point;
    return Number(point.earlyCount) + Number(point.greenCount) + Number(point.yellowCount) + Number(point.redCount) + Number(point.lateCount);
  });

  const width = 920;
  const height = 320;
  const left = 56;
  const right = 24;
  const top = 20;
  const bottom = 270;
  const x = index => left + (index * (width - left - right)) / Math.max(1, points.length - 1);
  const countScale = historyValueScale([0, ...totals], top, bottom);
  const barWidth = Math.max(3, Math.min(22, (width - left - right) / Math.max(1, points.length) * 0.7));
  const bands = [
    ["is-early", point => point.earlyCount],
    ["is-green", point => point.greenCount],
    ["is-yellow", point => point.yellowCount],
    ["is-red", point => point.redCount],
    ["is-late", point => point.lateCount],
  ];
  const bars = countScale ? bandPoints.map(entry => {
    let cumulative = 0;
    return bands.map(([cssClass, read]) => {
      const next = cumulative + Number(read(entry.point));
      const y = countScale.y(next);
      const bandHeight = countScale.y(cumulative) - y;
      cumulative = next;
      return `<rect class="history-time-band ${cssClass}" x="${x(entry.index) - barWidth / 2}" y="${y}" width="${barWidth}" height="${Math.max(0, bandHeight)}"></rect>`;
    }).join("");
  }).join("") : "";
  const gapMarkers = indexed
    .filter(entry => [
      entry.point.earlyCount,
      entry.point.greenCount,
      entry.point.yellowCount,
      entry.point.redCount,
      entry.point.lateCount,
    ].some(value => !validBandValue(value)))
    .map(entry => `<g class="history-evidence-gap" data-week-offset="${number(entry.point.weekOffset)}"><line x1="${x(entry.index)}" y1="${top}" x2="${x(entry.index)}" y2="${bottom}"></line><text x="${x(entry.index)}" y="${top + 12}" text-anchor="middle">证据缺口</text></g>`)
    .join("");
  const periodLabels = points.map((point, index) => index % Math.max(1, Math.ceil(points.length / 8)) === 0 || index === points.length - 1
    ? `<text class="history-axis-label" x="${x(index)}" y="296" text-anchor="middle">${escapeHtml(point.periodStartDate)}</text>`
    : "").join("");
  const evidenceRows = points.map(point => row([
    escapeHtml(point.periodStartDate),
    metricOrEvidenceMissing(point.earlyCount),
    metricOrEvidenceMissing(point.greenCount),
    metricOrEvidenceMissing(point.yellowCount),
    metricOrEvidenceMissing(point.redCount),
    metricOrEvidenceMissing(point.lateCount),
    escapeHtml(businessEvidenceLabel(point.cause)),
    escapeHtml(evidenceStatusLabel(point.evidenceStatus)),
  ])).join("");

  host.innerHTML = `
    <div class="history-chart-heading"><strong>${escapeHtml(historyControlPointLabel(item.controlPoint))} · ${number(history.observedTrendWeeks)} 周历史趋势</strong><span>${escapeHtml(item.protectedActivity)}</span></div>
    <svg class="history-evidence-svg" viewBox="0 0 ${width} ${height}" role="img" aria-label="时间缓冲五段历史">
      <line class="history-axis-line" x1="${left}" y1="${bottom}" x2="${width - right}" y2="${bottom}"></line>
      ${bars}${gapMarkers}${periodLabels}
    </svg>
    <div class="history-chart-legend"><span><i class="band early"></i>提前</span><span><i class="band green"></i>绿色</span><span><i class="band yellow"></i>黄色</span><span><i class="band red"></i>红色</span><span><i class="band late"></i>延误</span></div>
    <div class="table-scroll history-chart-table"><table class="data-table"><thead><tr><th>期间</th><th>提前</th><th>绿色</th><th>黄色</th><th>红色</th><th>延误</th><th>原因</th><th>证据</th></tr></thead><tbody>${evidenceRows}</tbody></table></div>`;
}

function renderHistoryTimeCostStrip(history, item) {
  const host = byId("history-time-cost-strip");
  const events = Array.isArray(item.abnormalCostEvents) ? item.abnormalCostEvents : [];
  const heading = `
    <div class="history-chart-heading"><strong>全对象异常费用事实台账 · ${number(history.observedTrendWeeks)} 周历史趋势</strong><span>所选历史窗口内全部有效事件</span></div>`;
  if (!events.length) {
    host.innerHTML = `${heading}<div class="history-cost-empty">本窗口无异常费用事实</div>`;
    return;
  }

  const cards = events.map(event => {
    const target = `${metricOrEvidenceMissing(event.targetType)} · ${metricOrEvidenceMissing(event.targetId)}`;
    return `
      <article class="history-cost-event-card" data-event-id="${escapeHtml(metricOrEvidenceMissing(event.eventId))}">
        <div class="history-cost-event-heading"><span>${escapeHtml(metricOrEvidenceMissing(event.periodStartDate))}</span><strong>${escapeHtml(metricOrEvidenceMissing(event.costAmount, money))}</strong></div>
        <dl class="history-cost-event-facts">
          <div><dt>事实编号</dt><dd>${escapeHtml(metricOrEvidenceMissing(event.eventId))}</dd></div>
          <div><dt>费用类型</dt><dd>${escapeHtml(metricOrEvidenceMissing(event.costType))}</dd></div>
          <div><dt>原因</dt><dd>${escapeHtml(metricOrEvidenceMissing(event.cause))}</dd></div>
          <div><dt>控制点</dt><dd>${escapeHtml(historyControlPointLabel(metricOrEvidenceMissing(event.controlPoint)))}</dd></div>
          <div><dt>影响对象</dt><dd>${escapeHtml(target)}</dd></div>
          <div><dt>来源</dt><dd>${escapeHtml(baselineSourceLabel(metricOrEvidenceMissing(event.sourceAuthority)))}</dd></div>
          <div><dt>证据</dt><dd>${escapeHtml(evidenceStatusLabel(metricOrEvidenceMissing(event.evidenceStatus)))}</dd></div>
        </dl>
      </article>`;
  }).join("");
  host.innerHTML = `${heading}<div class="history-cost-event-grid">${cards}</div>`;
}

function resolveHistoryCapacityPair(history) {
  const views = history.capacityBuffers || [];
  const selected = views.find(item => item.resourceCode === state.selectedHistoryCapacityResource) || null;
  const upstream = selected?.relationshipRole === "UpstreamProtection"
    ? selected
    : selected?.relationshipRole === "CcrUtilization"
      ? views.find(item => item.relationshipRole === "UpstreamProtection" && item.protectedCcrResourceCode === selected.resourceCode) || null
      : views.find(item => item.relationshipRole === "UpstreamProtection") || null;
  const ccr = upstream?.protectedCcrResourceCode
    ? views.find(item => item.resourceCode === upstream.protectedCcrResourceCode && item.relationshipRole === "CcrUtilization") || null
    : null;
  return { upstream, ccr };
}

function renderHistoryCapacityProtectionPair(history) {
  const { upstream, ccr } = resolveHistoryCapacityPair(history);
  const summaryByCode = new Map((history.capacityProtection || []).map(item => [item.resourceCode, item]));
  const latestPoint = view => [...(view?.points || [])]
    .reverse()
    .find(point => point.measure?.evidenceStatus === "Complete") || null;
  const upstreamPoint = latestPoint(upstream);
  const ccrPoint = latestPoint(ccr);
  const upstreamMeasure = upstreamPoint?.measure;
  const ccrMeasure = ccrPoint?.measure;
  const upstreamSummary = upstream ? summaryByCode.get(upstream.resourceCode) : null;
  const cardFacts = facts => `<dl class="capacity-role-facts">${facts.map(([label, value]) => `<div><dt>${escapeHtml(label)}</dt><dd>${value}</dd></div>`).join("")}</dl>`;

  byId("history-capacity-upstream-card").innerHTML = upstream && upstreamPoint
    ? `<div class="capacity-role-heading"><div><span>上游保护资源</span><strong>${escapeHtml(upstream.resourceName)} · ${escapeHtml(upstream.resourceCode)}</strong><small>保护 CCR：${escapeHtml(ccr?.resourceName || upstream.protectedCcrResourceCode || "证据缺失")} · ${escapeHtml(upstream.protectedCcrResourceCode || "证据缺失")}</small></div>${capacityUtilizationBandChip(upstreamMeasure)}</div>${cardFacts([
        ["期间", escapeHtml(upstreamPoint.periodStartDate)],
        ["计划可用", metricOrEvidenceMissing(upstreamPoint.plannedAvailableCapacity)],
        ["保护起点", metricOrEvidenceMissing(upstreamMeasure?.protectionStart)],
        ["承诺负荷", metricOrEvidenceMissing(upstreamPoint.committedLoad)],
        ["利用率", metricOrEvidenceMissing(upstreamMeasure?.utilizationPercent, percent)],
        ["保护能力", metricOrEvidenceMissing(upstreamMeasure?.protectionCapacity)],
        ["已消耗", metricOrEvidenceMissing(upstreamMeasure?.consumedProtection)],
        ["剩余保护", metricOrEvidenceMissing(upstreamMeasure?.remainingProtection)],
        ["超载", metricOrEvidenceMissing(upstreamMeasure?.overload)],
        ["损失原因", escapeHtml(businessEvidenceLabel(upstreamSummary?.lossReason))],
      ])}`
    : `<div class="history-chart-empty"><strong>上游保护证据缺失</strong><span>需要完整的后序 CCR 路由与周度能力证据</span></div>`;

  byId("history-capacity-ccr-card").innerHTML = ccr && ccrPoint
    ? `<div class="capacity-role-heading"><div><span>CCR 利用率参照</span><strong>${escapeHtml(ccr.resourceName)} · ${escapeHtml(ccr.resourceCode)}</strong><small>仅观察 CCR 负荷，不计算保护消耗</small></div>${capacityUtilizationBandChip(ccrMeasure)}</div>${cardFacts([
        ["期间", escapeHtml(ccrPoint.periodStartDate)],
        ["计划可用", metricOrEvidenceMissing(ccrPoint.plannedAvailableCapacity)],
        ["承诺负荷", metricOrEvidenceMissing(ccrPoint.committedLoad)],
        ["利用率参照", metricOrEvidenceMissing(ccrMeasure?.utilizationPercent, percent)],
      ])}`
    : `<div class="history-chart-empty"><strong>CCR 参照证据缺失</strong><span>没有找到与所选上游资源配对的 CCR</span></div>`;

}

function renderHistoryCapacityProtectionKpis(history) {
  const host = byId("history-capacity-protection-kpis");
  const { upstream } = resolveHistoryCapacityPair(history);
  const summary = history.capacityProtectionSummary;
  const summaryMatchesUpstream = summary && upstream && summary.resourceCode === upstream.resourceCode;
  const value = (metric, formatter = percent) => summaryMatchesUpstream && summary.evidenceStatus === "Complete"
    ? metricOrEvidenceMissing(summary[metric], formatter)
    : "证据缺失";
  host.innerHTML = [
    ["上游保护带余额率", value("balancePercent"), "后端期间汇总"],
    ["最低余额率", value("minimumBalancePercent"), "后端期间汇总"],
    ["保护耗尽周数", value("exhaustedWeekCount", number), "后端期间汇总"],
    ["超载周数", value("overloadWeekCount", number), "后端期间汇总"],
  ].map(item => stageKpi(...item)).join("");
}

function buildHistoryCapacityFrequency(values, axisMaximum, binWidth = 10) {
  const binCount = Math.max(1, Math.ceil(axisMaximum / binWidth));
  const bins = Array.from({ length: binCount }, (_, index) => ({
    from: index * binWidth,
    through: (index + 1) * binWidth,
    count: 0,
  }));
  values.forEach(value => {
    const index = Math.min(bins.length - 1, Math.max(0, Math.floor(Number(value) / binWidth)));
    bins[index].count += 1;
  });
  return bins;
}

function historyCapacityZoneLabel(from, through) {
  if (from === 0) return "绿区（0–60%）";
  if (from === 60) return "黄区（>60–80%）";
  if (from === 80) return "红区（>80–100%）";
  return "深红区（>100%）";
}

function historyCapacityZoneClass(from) {
  if (from === 0) return "is-green";
  if (from === 60) return "is-yellow";
  if (from === 80) return "is-red";
  return "is-deep-red";
}

function renderHistoryCapacityBuffer(history) {
  renderHistoryCapacityProtectionPair(history);
  renderHistoryCapacityProtectionKpis(history);
  const { upstream: item } = resolveHistoryCapacityPair(history);
  const points = item?.points || [];
  const utilizationPoints = points.filter(point =>
    point.measure?.evidenceStatus === "Complete" && isFiniteHistoryValue(point.measure?.utilizationPercent));
  if (!item || !utilizationPoints.length) {
    renderHistoryMissing("history-capacity-buffer-chart");
    return;
  }

  const indexed = points.map((point, index) => ({ point, index }));
  const values = utilizationPoints.map(point => Number(point.measure.utilizationPercent));
  const peak = Math.max(...values);
  const average = values.reduce((sum, value) => sum + value, 0) / values.length;
  const axisMaximum = Math.max(120, Math.ceil(peak / 10) * 10);
  const zones = [[0, 60], [60, 80], [80, 100], [100, axisMaximum]];
  const width = 760;
  const height = 320;
  const left = 58;
  const right = 24;
  const top = 20;
  const bottom = 262;
  const y = value => bottom - Number(value) * (bottom - top) / axisMaximum;
  const x = index => left + (index * (width - left - right)) / Math.max(1, points.length - 1);
  const barWidth = Math.max(3, Math.min(22, (width - left - right) / Math.max(1, points.length) * 0.58));
  const bars = indexed.filter(entry => entry.point.measure?.evidenceStatus === "Complete" && isFiniteHistoryValue(entry.point.measure?.utilizationPercent)).map(entry => {
    const value = Number(entry.point.measure.utilizationPercent);
    return `<rect class="history-capacity-observation ${historyCapacityZoneClass(value <= 60 ? 0 : value <= 80 ? 60 : value <= 100 ? 80 : 100)}" data-week-offset="${number(entry.point.weekOffset)}" data-utilization-percent="${number(value)}" x="${x(entry.index) - barWidth / 2}" y="${y(value)}" width="${barWidth}" height="${bottom - y(value)}"></rect>`;
  }).join("");
  const horizontalZones = zones.map(([from, through]) => {
    const clippedThrough = Math.min(through, axisMaximum);
    if (clippedThrough <= from) return "";
    return `<rect class="history-capacity-zone ${historyCapacityZoneClass(from)}" x="${left}" y="${y(clippedThrough)}" width="${width - left - right}" height="${y(from) - y(clippedThrough)}"></rect><text class="history-capacity-zone-label" x="${left + 5}" y="${y(clippedThrough) + 13}">${historyCapacityZoneLabel(from, clippedThrough)}</text>`;
  }).join("");
  const periodLabels = points.map((point, index) => index % Math.max(1, Math.ceil(points.length / 8)) === 0 || index === points.length - 1
    ? `<text class="history-axis-label" x="${x(index)}" y="288" text-anchor="middle">${escapeHtml(point.periodStartDate)}</text>`
    : "").join("");
  const frequencies = buildHistoryCapacityFrequency(values, axisMaximum);
  const frequencyMaximum = Math.max(1, ...frequencies.map(bin => bin.count));
  const frequencyWidth = 760;
  const frequencyLeft = 42;
  const frequencyRight = 20;
  const frequencyTop = 20;
  const frequencyBottom = 236;
  const frequencyX = value => frequencyLeft + Number(value) * (frequencyWidth - frequencyLeft - frequencyRight) / axisMaximum;
  const frequencyY = value => frequencyBottom - Number(value) * (frequencyBottom - frequencyTop) / frequencyMaximum;
  const verticalZones = zones.map(([from, through]) => {
    const clippedThrough = Math.min(through, axisMaximum);
    if (clippedThrough <= from) return "";
    return `<rect class="history-capacity-zone ${historyCapacityZoneClass(from)}" x="${frequencyX(from)}" y="${frequencyTop}" width="${frequencyX(clippedThrough) - frequencyX(from)}" height="${frequencyBottom - frequencyTop}"></rect><text class="history-capacity-zone-label" x="${frequencyX(from) + 4}" y="${frequencyTop + 13}">${historyCapacityZoneLabel(from, clippedThrough)}</text>`;
  }).join("");
  const curve = buildMonotonePath(frequencies.map(bin => ({ x: frequencyX((bin.from + bin.through) / 2), y: frequencyY(bin.count) })));
  const capacitySummary = (history.capacityProtection || []).find(candidate => candidate.resourceCode === item.resourceCode);
  const evidenceRows = points.map(point => {
    const measure = point.measure;
    return row([
    escapeHtml(point.periodStartDate),
    metricOrEvidenceMissing(point.theoreticalCapacity),
    metricOrEvidenceMissing(point.standardCapacity),
    metricOrEvidenceMissing(point.demonstratedCapacity),
    metricOrEvidenceMissing(point.plannedAvailableCapacity),
    metricOrEvidenceMissing(point.committedLoad),
    metricOrEvidenceMissing(measure?.utilizationPercent, percent),
    metricOrEvidenceMissing(measure?.protectionCapacity),
    metricOrEvidenceMissing(measure?.consumedProtection),
    metricOrEvidenceMissing(measure?.remainingProtection),
    metricOrEvidenceMissing(measure?.overload),
    escapeHtml(businessEvidenceLabel(capacitySummary?.lossReason)),
    capacityUtilizationBandChip(measure),
  ]);
  }).join("");

  byId("history-capacity-buffer-chart").innerHTML = `
    <div class="history-chart-heading"><strong>${escapeHtml(item.resourceName)} · ${escapeHtml(item.resourceCode)}</strong><span>上游保护利用率历史证据</span></div>
    <div class="history-capacity-composite">
      <section id="history-capacity-period-observations" class="history-capacity-subpanel" aria-label="上游周度利用率观测">
        <div class="history-chart-heading"><strong>周度上游利用率</strong><span>纵轴为利用率，不推演负荷</span></div>
        <svg class="history-evidence-svg" viewBox="0 0 ${width} ${height}" role="img" aria-label="周度上游利用率和保护带">
      ${horizontalZones}
      <line class="history-axis-line" x1="${left}" y1="${bottom}" x2="${width - right}" y2="${bottom}"></line>
      <line class="history-capacity-average-marker" x1="${left}" y1="${y(average)}" x2="${width - right}" y2="${y(average)}"></line><text class="history-average-label" x="${width - right}" y="${y(average) - 5}" text-anchor="end">平均 ${percent(average)}</text>
      ${bars}<circle class="history-capacity-peak-marker" cx="${x(points.findIndex(point => Number(point.measure?.utilizationPercent) === peak))}" cy="${y(peak)}" r="4"></circle><text class="history-capacity-peak-label" x="${x(points.findIndex(point => Number(point.measure?.utilizationPercent) === peak))}" y="${y(peak) - 8}" text-anchor="middle">峰值 ${percent(peak)}</text>${periodLabels}
        </svg>
      </section>
      <section id="history-capacity-empirical-distribution" class="history-capacity-subpanel" aria-label="上游利用率历史频率分布">
        <div class="history-chart-heading"><strong>历史频率曲线</strong><span>利用率分箱计数，不代表概率预测</span></div>
        <svg class="history-evidence-svg" viewBox="0 0 ${frequencyWidth} 300" role="img" aria-label="上游利用率历史频率曲线">
          ${verticalZones}<line class="history-axis-line" x1="${frequencyLeft}" y1="${frequencyBottom}" x2="${frequencyWidth - frequencyRight}" y2="${frequencyBottom}"></line><path class="history-capacity-empirical-curve" d="${curve}"></path>
          <text class="history-capacity-note is-headroom" x="${frequencyX(2)}" y="270">可用于吸收波动或扩大产量的余量</text><text class="history-capacity-note is-risk" x="${frequencyX(Math.min(82, axisMaximum - 5))}" y="286">可能成为流程干扰点的风险</text>
        </svg>
      </section>
    </div>
    <div class="table-scroll history-chart-table"><table class="data-table"><thead><tr><th>期间</th><th>理论</th><th>标准</th><th>经验证</th><th>计划可用</th><th>承诺负荷</th><th>利用率</th><th>保护带宽</th><th>已用保护</th><th>未用保护余量</th><th>超载</th><th>损失原因</th><th>负荷区</th></tr></thead><tbody>${evidenceRows}</tbody></table></div>`;
}

function renderHistoryReview(history) {
  state.historyReview = history;
  syncHistorySelectionState(history);
  renderHistoryWorkspaceOptions(history);
  byId("history-evidence-chip").className = "status-chip is-valid";
  byId("history-evidence-chip").textContent = `${historyEvidenceSummary(history.evidenceLabel)} · ${history.observedTrendWeeks} 周实际`;
  const outcomes = history.operatingOutcomes;
  byId("history-review-kpis").innerHTML = [
    ["服务水平", metricOrEvidenceMissing(outcomes.serviceLevelPercent, percent), `${history.observedTrendWeeks} 周历史实际`],
    ["平均库存金额", metricOrEvidenceMissing(outcomes.inventoryValue, money), "所选历史窗口平均"],
    ["在制品", metricOrEvidenceMissing(outcomes.workInProcessUnits, number), "所选历史窗口平均"],
    ["流动时间", metricOrEvidenceMissing(outcomes.averageFlowTimeDays, value => `${number(value)} 天`), `累计提前期详细证据窗口：${history.detailWindowWeeks} 周`],
    ["现金占用", metricOrEvidenceMissing(outcomes.cashOccupied, money), "历史经营结果"],
    ["异常费用", metricOrEvidenceMissing(outcomes.expediteCost, money), "可追溯异常处置"],
  ].map(item => stageKpi(...item)).join("");

  byId("history-protection-body").innerHTML = history.protectionRelationships.length
    ? history.protectionRelationships.map(item => row([
        escapeHtml(historyControlPointLabel(item.controlPoint)),
        escapeHtml(item.protectedObject),
        escapeHtml(breachScopeLabel(item.protectionType)),
        escapeHtml(protectionStateLabel(item.designStatus)),
        escapeHtml(protectionStateLabel(item.availabilityStatus)),
        escapeHtml(protectionStateLabel(item.effectivenessStatus)),
        escapeHtml(businessEvidenceLabel(item.evidence)),
      ])).join("")
    : emptyRow("没有保护关系证据", 7);
  byId("history-zone-body").innerHTML = history.zoneResidence.length
    ? history.zoneResidence.map(item => row([
        `<strong>${escapeHtml(item.sku)}</strong><small>${escapeHtml(item.name)}</small>`,
        number(item.observedPeriods),
        `<span class="status-chip is-invalid">${number(item.redPeriods)} / ${percent(item.redPercent)}</span>`,
        `<span class="status-chip is-warning">${number(item.yellowPeriods)} / ${percent(item.yellowPercent)}</span>`,
        `<span class="status-chip is-valid">${number(item.greenPeriods)} / ${percent(item.greenPercent)}</span>`,
        `${number(item.maximumRedStreak)} 周 · 进入 ${number(item.redEntryCount)} 次`,
        item.recoveryPeriods == null ? "历史窗口内未恢复" : `${number(item.recoveryPeriods)} 周期`,
        escapeHtml(metricOrEvidenceMissing(item.primaryCause)),
      ])).join("")
    : emptyRow("没有区域停留证据", 8);

  byId("history-capacity-layers").innerHTML = history.capacityProtection.map(item => {
    const scaleCandidate = [item.theoreticalCapacity, item.standardCapacity, item.demonstratedCapacity, item.plannedAvailableCapacity]
      .find(value => value !== null && value !== undefined);
    const scale = Math.max(1, Number(valueOr(scaleCandidate, 1)));
    const bar = (label, value, cssClass) => value === null || value === undefined
      ? `<div class="capacity-layer-row"><span>${label}</span><div class="capacity-layer-track is-missing"></div><strong>证据缺失</strong></div>`
      : `<div class="capacity-layer-row"><span>${label}</span><div class="capacity-layer-track"><i class="${cssClass}" style="width:${Math.min(100, Number(value) * 100 / scale)}%"></i></div><strong>${number(value)}</strong></div>`;
    const protectionRows = item.relationshipRole === "UpstreamProtection"
      ? `${bar("保护能力", item.protectiveCapacity, "is-protection")}${bar("已消耗保护", item.consumedProtection, "is-load")}${bar("剩余保护", item.remainingProtection, "is-available")}`
      : "";
    return `<article class="capacity-layer-item"><div><strong>${escapeHtml(item.resourceName)}</strong><small>${escapeHtml(item.resourceCode)} · ${escapeHtml(capacityRoleLabel(item.relationshipRole, item.protectedCcrResourceCode))}</small><small>${escapeHtml(businessEvidenceLabel(item.lossReason))} · ${escapeHtml(evidenceStatusLabel(item.evidenceStatus))}</small></div>${bar("理论", item.theoreticalCapacity, "is-theoretical")}${bar("标准", item.standardCapacity, "is-standard")}${bar("经验证", item.demonstratedCapacity, "is-demonstrated")}${bar("计划可用", item.plannedAvailableCapacity, "is-available")}${bar(item.relationshipRole === "CcrUtilization" ? "CCR 利用率参照" : "承诺负荷", item.committedLoad, "is-load")}${protectionRows}</article>`;
  }).join("");
  byId("history-constraint-body").innerHTML = history.constraintExposure.length
    ? history.constraintExposure.map(item => row([
        escapeHtml(constraintExposureLabel(item.exposureType)),
        escapeHtml(item.target),
        `<span class="${statusClass(item.status)}">${statusLabel(item.status)}</span>`,
        metricOrEvidenceMissing(item.loadPercent, percent),
        escapeHtml(businessEvidenceLabel(item.evidence)),
      ])).join("")
    : emptyRow("没有约束暴露证据", 5);

  renderHistoryBufferOverview(history);
  renderHistoryInventoryBuffer(history);
  renderHistoryDdmrpSizingTrace(history);
  renderHistoryTimeBuffer(history);
  renderHistoryCapacityBuffer(history);
}

function isStaleHistoryRequest(requestGeneration, currentGeneration) {
  return requestGeneration !== currentGeneration;
}

async function loadHistoryReview(trendMonths = state.historyTrendMonths) {
  trendMonths = trendMonths === 12 ? 12 : 6;
  const requestGeneration = ++state.historyRequestGeneration;
  try {
    const response = await fetch(`/api/history-review?trendMonths=${trendMonths}`, { headers: { Accept: "application/json" } });
    if (!response.ok) throw new Error(`历史回顾接口失败：${response.status}`);
    const history = await response.json();
    if (isStaleHistoryRequest(requestGeneration, state.historyRequestGeneration)) return;
    state.historyTrendMonths = trendMonths;
    document.querySelectorAll("[data-history-range-months]").forEach(button => {
      const isSelected = Number(button.dataset.historyRangeMonths) === state.historyTrendMonths;
      button.classList.toggle("is-selected", isSelected);
      button.setAttribute("aria-pressed", String(isSelected));
    });
    clearWorkspaceError("history-review");
    renderHistoryReview(history);
  } catch (error) {
    if (isStaleHistoryRequest(requestGeneration, state.historyRequestGeneration)) return;
    throw error;
  }
}

function configureFutureBaselineSelect() {
  ["future-baseline-select", "governance-baseline-id"].forEach(id => {
    const select = byId(id);
    const previous = select.value;
    select.innerHTML = state.currentBaselines.length
      ? [`<option value="">请选择冻结基线</option>`, ...state.currentBaselines.map(item => `<option value="${escapeHtml(item.snapshotId)}">${escapeHtml(item.snapshotNumber)} · ${escapeHtml(item.asOfUtc)} · ${baselineStatusLabel(item.status)}</option>`)].join("")
      : `<option value="">请先冻结当前基线</option>`;
    if (state.currentBaselines.some(item => item.snapshotId === previous)) select.value = previous;
  });
}

function baselineFreezeBlockingIssues(sections) {
  const issues = [];
  for (const section of sections || []) {
    if (Array.isArray(section.items) && section.items.length > 0) {
      for (const item of section.items) {
        const complete = item.freshnessStatus === "Fresh" && item.completenessStatus === "Complete";
        if (!complete && item.blocksFreeze === true) issues.push({ section, item });
      }
      continue;
    }

    const complete = section.freshnessStatus === "Fresh" && section.completenessStatus === "Complete";
    if (!complete && section.isRequired === true) issues.push({ section, item: null });
  }
  return issues;
}

function baselineCandidateFreezeBlockingIssues(candidate) {
  const issues = baselineFreezeBlockingIssues(candidate?.sections);
  const planningInputs = candidate?.payload?.planningInputs;
  if (planningInputs === null || planningInputs === undefined) {
    issues.push({
      section: null,
      item: null,
      missingReason: "后端未提供类型化计划输入",
    });
  }
  const reconciliation = candidate?.payload?.historyReconciliation;
  if (!reconciliation) {
    issues.push({ section: null, item: null, missingReason: "后端未提供历史衔接证据" });
    return issues;
  }
  if (!reconciliation.factSetId || reconciliation.evidenceStatus !== "Complete") {
    issues.push({ section: null, item: null, missingReason: "历史衔接总体证据不完整" });
  }
  const historyThrough = Date.parse(reconciliation.historyThroughUtc);
  const baselineAsOf = Date.parse(reconciliation.baselineAsOfUtc);
  if (!Number.isFinite(historyThrough) || !Number.isFinite(baselineAsOf) || historyThrough >= baselineAsOf) {
    issues.push({ section: null, item: null, missingReason: "历史截止时间必须早于基线时点" });
  }
  if (!Array.isArray(reconciliation.lines) || reconciliation.lines.length === 0) {
    issues.push({ section: null, item: null, missingReason: "后端未提供历史衔接对账行" });
  } else {
    reconciliation.lines.forEach(line => {
      const difference = Number(line?.difference);
      if (line?.evidenceStatus !== "Complete" || line?.difference === null || line?.difference === undefined ||
          line?.difference === "" || !Number.isFinite(difference) || Math.abs(difference) > 0.01) {
        issues.push({ section: null, item: null, missingReason: "历史衔接对账行证据不完整或差异超限" });
      }
    });
  }
  return issues;
}

function baselineEvidenceMissing(reason, label = "证据缺失") {
  const detail = reason === null || reason === undefined || reason === ""
    ? "后端未提供缺失原因"
    : businessEvidenceLabel(reason);
  return `<span class="baseline-evidence-missing"><strong>${escapeHtml(label)}</strong><small>${escapeHtml(detail)}</small></span>`;
}

function baselineEvidenceValue(value, reason, formatter = item => String(item)) {
  return value === null || value === undefined || value === ""
    ? baselineEvidenceMissing(reason, "证据缺失")
    : escapeHtml(formatter(value));
}

function baselineEvidenceNumber(value, reason) {
  return baselineEvidenceValue(value, reason, item => numberFormat.format(Number(item)));
}

function baselineReconciliationNumber(value, reason) {
  const numeric = Number(value);
  return value === null || value === undefined || value === "" || !Number.isFinite(numeric)
    ? baselineEvidenceMissing(reason)
    : escapeHtml(numberFormat.format(numeric));
}

function baselineReconciliationMetricLabel(metricCode) {
  return ({
    ON_HAND: "在手库存",
    INVENTORY_VALUE: "库存金额",
    WORK_IN_PROCESS: "在制品",
    BACKLOG: "积压需求",
    RESOURCE_AVAILABLE_CAPACITY: "资源可用能力",
  })[metricCode] || valueOr(metricCode, "证据缺失");
}

function baselineReconciliationItemLabel(itemKey) {
  return itemKey === "ALL" ? "全部" : valueOr(itemKey, "证据缺失");
}

function renderBaselineHistoryReconciliation(baseline, viewKind = "Candidate", updatePage = true) {
  const reconciliation = baseline?.payload?.historyReconciliation;
  const isFrozen = viewKind === "Frozen";
  if (!reconciliation) {
    const message = isFrozen ? "旧版本未保存历史衔接证据" : "后端未提供历史衔接证据";
    if (updatePage) {
      byId("baseline-history-reconciliation-summary").innerHTML = `<span>${escapeHtml(message)}</span>`;
      byId("baseline-history-reconciliation-body").innerHTML = `<tr><td class="empty-cell" colspan="9">${escapeHtml(message)}</td></tr>`;
    }
    return { drawerSections: [{ title: "历史衔接", items: [["历史衔接", message]] }] };
  }

  const summaryItems = [
    ["事实集", baselineEvidenceValue(reconciliation.factSetId, "后端未提供事实集标识")],
    ["最近历史截止", baselineEvidenceValue(reconciliation.historyThroughUtc, "后端未提供历史截止时间")],
    ["基线截止", baselineEvidenceValue(reconciliation.baselineAsOfUtc, "后端未提供基线截止时间")],
    ["范围", baselineEvidenceValue(reconciliation.scopeLabel, "后端未提供对账范围", businessEvidenceLabel)],
    ["对账行", Array.isArray(reconciliation.lines) ? escapeHtml(number(reconciliation.lines.length)) : baselineEvidenceMissing("后端未提供对账行")],
    ["证据状态", baselineEvidenceStatus(reconciliation.evidenceStatus, "后端未提供对账证据状态")],
  ];
  const summaryHtml = summaryItems.map(([label, value]) => `<span><strong>${escapeHtml(label)}</strong>${value}</span>`).join("");
  const lines = Array.isArray(reconciliation.lines) ? reconciliation.lines : [];
  const drawerLineItems = lines.length ? lines.map(line => {
    const reason = line?.differenceReason === null || line?.differenceReason === undefined || line?.differenceReason === ""
      ? ""
      : ` · 原因 ${escapeHtml(businessEvidenceLabel(line.differenceReason))}`;
    return [
      `${baselineEvidenceValue(line?.metricCode, "后端未提供指标", baselineReconciliationMetricLabel)} / ${baselineEvidenceValue(line?.itemKey, "后端未提供对象", baselineReconciliationItemLabel)}`,
      `历史期末 ${baselineReconciliationNumber(line?.historyClosingBalance, "后端未提供历史期末余额")} · 增加 ${baselineReconciliationNumber(line?.intervalIncrease, "后端未提供增加值")} · 减少 ${baselineReconciliationNumber(line?.intervalDecrease, "后端未提供减少值")} · 调整 ${baselineReconciliationNumber(line?.adjustment, "后端未提供调整值")} · 基线 ${baselineReconciliationNumber(line?.baselineBalance, "后端未提供基线余额")} · 差异 ${baselineReconciliationNumber(line?.difference, "后端未提供有效差异")} · 状态 ${baselineEvidenceStatus(line?.evidenceStatus, "后端未提供对账行证据状态")}${reason}`,
    ];
  }) : [["对账行", baselineEvidenceMissing("后端未提供历史衔接对账行")]];
  const rows = lines.length ? lines.map(line => {
    const difference = baselineReconciliationNumber(line?.difference, "后端未提供有效差异");
    const reason = line?.differenceReason === null || line?.differenceReason === undefined || line?.differenceReason === ""
      ? ""
      : `<small>${escapeHtml(businessEvidenceLabel(line.differenceReason))}</small>`;
    return row([
      baselineEvidenceValue(line?.metricCode, "后端未提供指标", baselineReconciliationMetricLabel),
      baselineEvidenceValue(line?.itemKey, "后端未提供对象", baselineReconciliationItemLabel),
      baselineReconciliationNumber(line?.historyClosingBalance, "后端未提供历史期末余额"),
      baselineReconciliationNumber(line?.intervalIncrease, "后端未提供增加值"),
      baselineReconciliationNumber(line?.intervalDecrease, "后端未提供减少值"),
      baselineReconciliationNumber(line?.adjustment, "后端未提供调整值"),
      baselineReconciliationNumber(line?.baselineBalance, "后端未提供基线余额"),
      `<span>差异 ${difference}</span>${reason}`,
      baselineEvidenceStatus(line?.evidenceStatus, "后端未提供对账行证据状态"),
    ]);
  }).join("") : `<tr><td class="empty-cell" colspan="9">${baselineEvidenceMissing("后端未提供历史衔接对账行")}</td></tr>`;

  if (updatePage) {
    byId("baseline-history-reconciliation-summary").innerHTML = summaryHtml;
    byId("baseline-history-reconciliation-body").innerHTML = rows;
  }
  return {
    drawerSections: [
      { title: "历史衔接", items: summaryItems },
      { title: "对账行明细", items: drawerLineItems },
    ],
  };
}

function confirmedReceiptTypeLabel(type) {
  return ({
    ConfirmedInTransit: "已确认在途",
    ConfirmedOpenSupply: "已确认开放供应",
  })[type] || businessEvidenceLabel(type);
}

function supplySourceTypeLabel(type) {
  return ({
    ExternalSupplier: "外部供应商",
    InternalSupply: "内部供应",
  })[type] || businessEvidenceLabel(type);
}

function receiptConfirmationLabel(status) {
  return ({
    Confirmed: "已确认",
    Pending: "待确认",
    Rejected: "已拒绝",
  })[status] || businessEvidenceLabel(status);
}

function baselineEvidenceStatus(value, missingReason) {
  if (value === null || value === undefined || value === "") {
    return baselineEvidenceMissing(missingReason);
  }
  const color = value === "Complete" ? "Green" : value === "NotApplicable" ? "neutral" : "Red";
  return `<span class="${statusClass(color)}">${escapeHtml(evidenceStatusLabel(value))}</span>`;
}

function baselinePlanningEvidenceSection(baseline, sectionCode) {
  if (!Array.isArray(baseline?.sections)) return undefined;
  return baseline.sections.find(section => section.sectionCode === sectionCode);
}

function baselineEvidenceBlockerText(section, blockers) {
  if (!section) return "证据缺失：后端未提供证据分区";
  const matching = blockers.filter(issue => issue.section === section);
  if (matching.length === 0) return "无";
  return matching.map(issue => {
    const reason = issue.item?.missingReason || issue.section?.missingReason;
    if (reason) return businessEvidenceLabel(reason);
    const freshness = issue.item?.freshnessStatus || issue.section?.freshnessStatus;
    const completeness = issue.item?.completenessStatus || issue.section?.completenessStatus;
    return `${freshnessLabel(freshness)} / ${completenessLabel(completeness)}（后端未提供阻断原因）`;
  }).join("；");
}

function baselineEvidenceSectionSummary(section, blockers) {
  if (!section) return baselineEvidenceMissing("后端未提供证据分区");
  return [
    `<span><strong>新鲜度</strong>${baselineEvidenceValue(section.freshnessStatus, "后端未提供新鲜度", freshnessLabel)}</span>`,
    `<span><strong>完整性</strong>${baselineEvidenceValue(section.completenessStatus, "后端未提供完整性", completenessLabel)}</span>`,
    `<span><strong>来源</strong>${baselineEvidenceValue(section.evidenceLabel, "后端未提供证据来源", baselineSourceLabel)}</span>`,
    `<span><strong>阻断项</strong>${escapeHtml(baselineEvidenceBlockerText(section, blockers))}</span>`,
  ].join("");
}

function baselineEmptyEvidenceCollection(section, blockers, emptyMessage, fallbackReason) {
  const blocked = blockers.some(issue => issue.section === section);
  const complete = section?.completenessStatus === "Complete";
  if (complete && !blocked) {
    return { isMissing: false, message: emptyMessage };
  }
  const reason = section?.missingReason || (blocked
    ? baselineEvidenceBlockerText(section, blockers)
    : fallbackReason);
  return { isMissing: true, message: baselineEvidenceMissing(reason) };
}

function renderBaselinePlanningEvidence(baseline, viewKind = "Candidate", updatePage = true) {
  const planningInputs = baseline?.payload?.planningInputs;
  const coverage = planningInputs ? planningInputs.planningEvidenceCoverage : undefined;
  const receipts = planningInputs ? planningInputs.confirmedReceipts : undefined;
  const openingBacklog = planningInputs ? planningInputs.openingBacklog : undefined;
  const blockers = viewKind === "Candidate"
    ? baselineCandidateFreezeBlockingIssues(baseline)
    : baselineFreezeBlockingIssues(baseline?.sections);
  const coverageSection = baselinePlanningEvidenceSection(baseline, "PLANNING_EVIDENCE_COVERAGE");
  const receiptSection = baselinePlanningEvidenceSection(baseline, "CONFIRMED_RECEIPTS");
  const backlogSection = baselinePlanningEvidenceSection(baseline, "OPENING_BACKLOG");
  const isFrozen = viewKind === "Frozen";
  const identityLabel = isFrozen ? "快照" : "候选";
  const identity = isFrozen ? baseline?.snapshotNumber : baseline?.candidateId;
  const identityReason = isFrozen ? "后端未提供快照号" : "后端未提供候选号";
  const immutableLabel = isFrozen ? "不可变" : "待冻结";
  const contextItems = [
    [identityLabel, baselineEvidenceValue(identity, identityReason)],
    ["版本", baselineEvidenceValue(baseline?.masterSettingVersion, "后端未提供主设置版本")],
    ["状态", isFrozen
      ? baselineEvidenceValue(baseline?.status, "后端未提供冻结状态", baselineStatusLabel)
      : immutableLabel],
    ["可变性", immutableLabel],
  ];
  const contextHtml = `<strong>${identityLabel} ${baselineEvidenceValue(identity, identityReason)}</strong><span>版本 ${baselineEvidenceValue(baseline?.masterSettingVersion, "后端未提供主设置版本")} · ${immutableLabel}</span>`;

  const coverageRange = coverage === null || coverage === undefined
    ? baselineEvidenceMissing("后端未提供覆盖记录")
    : `第 ${baselineEvidenceNumber(coverage.coverageFromWeek, "后端未提供起始周")} 周至第 ${baselineEvidenceNumber(coverage.coverageThroughWeek, "后端未提供截止周")} 周`;
  const coverageItems = [
    ["范围", coverageRange],
    ["锚点", baselineEvidenceValue(coverage?.anchorDate, "后端未提供锚点日期")],
    ["证据状态", baselineEvidenceStatus(coverage?.evidenceStatus, "后端未提供覆盖证据状态")],
    ["新鲜度", baselineEvidenceValue(coverageSection?.freshnessStatus, "后端未提供覆盖新鲜度", freshnessLabel)],
    ["完整性", baselineEvidenceValue(coverageSection?.completenessStatus, "后端未提供覆盖完整性", completenessLabel)],
    ["阻断项", escapeHtml(baselineEvidenceBlockerText(coverageSection, blockers))],
  ];
  const coverageHtml = coverageItems.map(([label, value]) => `<div><dt>${escapeHtml(label)}</dt><dd>${value}</dd></div>`).join("");

  let receiptRows;
  let receiptDrawerItems;
  if (!Array.isArray(receipts)) {
    const missing = baselineEvidenceMissing(planningInputs ? "后端未提供确认到货列表" : "后端未提供类型化计划输入");
    receiptRows = `<tr><td class="empty-cell" colspan="8">${missing}</td></tr>`;
    receiptDrawerItems = [["确认到货", missing]];
  } else if (receipts.length === 0) {
    const emptyState = baselineEmptyEvidenceCollection(
      receiptSection,
      blockers,
      "无确认到货记录",
      "后端未提供完整的确认到货证据",
    );
    receiptRows = emptyState.isMissing
      ? `<tr><td class="empty-cell" colspan="8">${emptyState.message}</td></tr>`
      : emptyRow(emptyState.message, 8);
    receiptDrawerItems = [["确认到货", emptyState.message]];
  } else {
    receiptRows = receipts.map(item => {
      const source = `${baselineEvidenceValue(item.sourceReference, "后端未提供来源引用")}<small>${baselineEvidenceValue(item.supplySourceType, "后端未提供来源类型", supplySourceTypeLabel)} · ${baselineEvidenceValue(item.evidenceLabel, "后端未提供证据来源", baselineSourceLabel)}</small>`;
      return row([
        baselineEvidenceValue(item.sku, "后端未提供 SKU"),
        baselineEvidenceNumber(item.quantity, "后端未提供数量"),
        baselineEvidenceNumber(item.expectedReceiptWeek, "后端未提供预计周"),
        baselineEvidenceValue(item.receiptType, "后端未提供到货类型", confirmedReceiptTypeLabel),
        source,
        baselineEvidenceValue(item.confirmationStatus, "后端未提供确认状态", receiptConfirmationLabel),
        baselineEvidenceValue(item.asOfUtc, "后端未提供截止时间"),
        baselineEvidenceStatus(item.evidenceStatus, "后端未提供证据状态"),
      ]);
    }).join("");
    receiptDrawerItems = receipts.map((item, index) => [
      item.receiptId === null || item.receiptId === undefined || item.receiptId === "" ? `记录 ${index + 1}` : String(item.receiptId),
      `SKU ${baselineEvidenceValue(item.sku, "后端未提供 SKU")} · 数量 ${baselineEvidenceNumber(item.quantity, "后端未提供数量")} · 预计周 ${baselineEvidenceNumber(item.expectedReceiptWeek, "后端未提供预计周")}<br>类型 ${baselineEvidenceValue(item.receiptType, "后端未提供到货类型", confirmedReceiptTypeLabel)} · 来源 ${baselineEvidenceValue(item.sourceReference, "后端未提供来源引用")} · 确认 ${baselineEvidenceValue(item.confirmationStatus, "后端未提供确认状态", receiptConfirmationLabel)}<br>截止 ${baselineEvidenceValue(item.asOfUtc, "后端未提供截止时间")} · 证据 ${baselineEvidenceStatus(item.evidenceStatus, "后端未提供证据状态")}`,
    ]);
  }

  const backlogBlocker = baselineEvidenceBlockerText(backlogSection, blockers);
  let backlogRows;
  let backlogDrawerItems;
  if (!Array.isArray(openingBacklog)) {
    const missing = baselineEvidenceMissing(planningInputs ? "后端未提供期初积压列表" : "后端未提供类型化计划输入");
    backlogRows = `<tr><td class="empty-cell" colspan="6">${missing}</td></tr>`;
    backlogDrawerItems = [["期初积压", missing]];
  } else if (openingBacklog.length === 0) {
    const emptyState = baselineEmptyEvidenceCollection(
      backlogSection,
      blockers,
      "无期初积压记录",
      "后端未提供完整的期初积压证据",
    );
    backlogRows = emptyState.isMissing
      ? `<tr><td class="empty-cell" colspan="6">${emptyState.message}</td></tr>`
      : emptyRow(emptyState.message, 6);
    backlogDrawerItems = [["期初积压", emptyState.message]];
  } else {
    backlogRows = openingBacklog.map(item => row([
      baselineEvidenceValue(item.backlogId, "后端未提供积压行号"),
      baselineEvidenceValue(item.sku, "后端未提供 SKU"),
      baselineEvidenceNumber(item.quantity, "后端未提供数量"),
      baselineEvidenceValue(backlogSection?.freshnessStatus, "后端未提供新鲜度", freshnessLabel),
      baselineEvidenceStatus(item.evidenceStatus, "后端未提供完整性"),
      escapeHtml(backlogBlocker),
    ])).join("");
    backlogDrawerItems = openingBacklog.map((item, index) => [
      item.backlogId === null || item.backlogId === undefined || item.backlogId === "" ? `行 ${index + 1}` : String(item.backlogId),
      `SKU ${baselineEvidenceValue(item.sku, "后端未提供 SKU")} · 数量 ${baselineEvidenceNumber(item.quantity, "后端未提供数量")}<br>新鲜度 ${baselineEvidenceValue(backlogSection?.freshnessStatus, "后端未提供新鲜度", freshnessLabel)} · 完整性 ${baselineEvidenceStatus(item.evidenceStatus, "后端未提供完整性")} · 阻断项 ${escapeHtml(backlogBlocker)}`,
    ]);
  }

  if (updatePage) {
    byId("baseline-planning-evidence-context").innerHTML = contextHtml;
    byId("baseline-coverage-evidence-list").innerHTML = coverageHtml;
    byId("baseline-receipt-evidence-summary").innerHTML = baselineEvidenceSectionSummary(receiptSection, blockers);
    byId("baseline-receipt-evidence-body").innerHTML = receiptRows;
    byId("baseline-backlog-evidence-summary").innerHTML = baselineEvidenceSectionSummary(backlogSection, blockers);
    byId("baseline-backlog-evidence-body").innerHTML = backlogRows;
  }

  return {
    drawerSections: [
      { title: "快照信息", items: contextItems },
      { title: "覆盖证据", items: coverageItems },
      { title: "确认到货", items: receiptDrawerItems },
      { title: "期初积压", items: backlogDrawerItems },
    ],
  };
}

function renderCurrentBaselineWorkspace() {
  const candidate = state.currentBaselineCandidate;
  if (!candidate) return;
  const missing = baselineCandidateFreezeBlockingIssues(candidate);
  byId("current-baseline-chip").className = `status-chip ${missing.length ? "is-invalid" : "is-valid"}`;
  byId("current-baseline-chip").textContent = missing.length ? `阻断 ${missing.length} 项关键证据` : `${baselineSourceLabel(candidate.evidenceLabel)} · 可冻结`;
  byId("freeze-current-baseline").disabled = missing.length > 0;
  byId("baseline-candidate-title").textContent = `截止 ${candidate.asOfUtc} · 主设置 ${candidate.masterSettingVersion}`;
  const kpis = candidate.payload?.kpis;
  const wipSource = baselineSourceLabel(kpis?.sourceAuthority);
  const wipNote = /derived|推导/i.test(String(kpis?.sourceAuthority || "")) ? `${wipSource} · 演示推导值` : wipSource;
  byId("current-baseline-kpis").innerHTML = kpis ? [
    ["会前截止时刻", metricOrEvidenceMissing(kpis.asOfUtc), baselineSourceLabel(kpis.sourceAuthority)],
    ["服务统计窗口", businessEvidenceLabel(kpis.serviceWindow), evidenceStatusLabel(kpis.evidenceStatus)],
    ["服务水平", metricOrEvidenceMissing(kpis.serviceLevelPercent, percent), "截至会前的 52 周滚动实际"],
    ["库存金额", metricOrEvidenceMissing(kpis.inventoryValue, money), "会前截止时点余额"],
    ["在制品", metricOrEvidenceMissing(kpis.workInProcessUnits, number), wipNote],
    ["积压", metricOrEvidenceMissing(kpis.backlogUnits, number), "会前事实"],
    ["供应覆盖", metricOrEvidenceMissing(kpis.supplyCoverageWeeks, value => `${number(value)} 周`), "会前事实"],
    ["峰值负荷", metricOrEvidenceMissing(kpis.peakResourceLoadPercent, percent), "冻结计划输入的展望期峰值"],
  ].map(item => stageKpi(...item)).join("") : stageKpi("会前证据", "证据缺失", "未按零处理");
  renderBaselinePlanningEvidence(candidate, "Candidate");
  renderBaselineHistoryReconciliation(candidate, "Candidate");
  byId("baseline-evidence-body").innerHTML = candidate.sections.map(item => row([
    `<strong>${escapeHtml(baselineSectionLabel(item.name))}</strong><small>${item.isRequired ? "关键" : "辅助"}</small>`,
    escapeHtml(baselineSourceLabel(item.sourceAuthority)),
    escapeHtml(item.asOfUtc),
    escapeHtml(freshnessLabel(item.freshnessStatus)),
    `<span class="${statusClass(item.completenessStatus === "Complete" ? "Green" : "Red")}">${escapeHtml(completenessLabel(item.completenessStatus))}</span>`,
    item.completenessStatus === "Complete" ? number(item.itemCount) : item.completenessStatus === "NotApplicable" ? "不适用" : "证据缺失",
    escapeHtml(baselineSourceLabel(item.evidenceLabel)),
    escapeHtml(item.missingReason ? businessEvidenceLabel(item.missingReason) : "不适用"),
  ])).join("");
  byId("baseline-snapshot-body").innerHTML = state.currentBaselines.length
    ? state.currentBaselines.map(item => `<tr class="interactive-row" tabindex="0" data-baseline-snapshot-id="${escapeHtml(item.snapshotId)}"><td><strong>${escapeHtml(item.snapshotNumber)}</strong></td><td>${escapeHtml(item.asOfUtc)}</td><td>${escapeHtml(item.masterSettingVersion)}</td><td>${escapeHtml(baselineActorLabel(item.createdBy))}</td><td>${number(item.completeSectionCount)} / ${number(item.sectionCount)}</td><td><span class="status-chip is-valid">${escapeHtml(baselineStatusLabel(item.status))}</span></td></tr>`).join("")
    : emptyRow("尚未冻结基线", 6);
  configureFutureBaselineSelect();
  configureCoordinationLineageSelectors();
}

async function loadCurrentBaselineWorkspace() {
  const [candidateResponse, listResponse] = await Promise.all([
    fetch("/api/current-baselines/candidate", { headers: { Accept: "application/json" } }),
    fetch("/api/current-baselines?limit=50", { headers: { Accept: "application/json" } }),
  ]);
  if (!candidateResponse.ok || !listResponse.ok) throw new Error("当前基线接口失败。");
  state.currentBaselineCandidate = await candidateResponse.json();
  state.currentBaselines = await listResponse.json();
  renderCurrentBaselineWorkspace();
  if (state.currentBaselines.length) {
    await loadBaselineAudit(state.currentBaselines[0].snapshotId);
  } else {
    byId("baseline-audit-list").innerHTML = `<div class="table-empty"><strong>冻结基线后生成审计记录</strong></div>`;
    byId("baseline-reference-list").innerHTML = `<div class="table-empty"><strong>尚无可反查的冻结基线</strong></div>`;
  }
}

async function freezeCurrentBaseline() {
  const response = await fetch("/api/current-baselines", {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify({ createdBy: byId("baseline-created-by").value, note: byId("baseline-note").value || null }),
  });
  const payload = await response.json();
  if (!response.ok) throw new Error(payload.message || `冻结基线失败：${response.status}`);
  byId("current-baseline-chip").className = "status-chip is-valid";
  byId("current-baseline-chip").textContent = `${payload.snapshotNumber} 已冻结`;
  await loadCurrentBaselineWorkspace();
  await loadBaselineAudit(payload.snapshotId);
}

async function loadBaselineAudit(snapshotId) {
  const [auditResponse, referenceResponse] = await Promise.all([
    fetch(`/api/current-baselines/${snapshotId}/audit`, { headers: { Accept: "application/json" } }),
    fetch(`/api/current-baselines/${snapshotId}/references`, { headers: { Accept: "application/json" } }),
  ]);
  if (!auditResponse.ok) throw new Error(`基线审计接口失败：${auditResponse.status}`);
  if (!referenceResponse.ok) throw new Error(`基线关联接口失败：${referenceResponse.status}`);
  const audit = await auditResponse.json();
  const references = await referenceResponse.json();
  byId("baseline-audit-list").innerHTML = audit.length
    ? audit.map(item => `<div class="diagnostic-item"><strong>${number(item.sequence)}. ${escapeHtml(auditEventLabel(item.eventType))}</strong><span>${escapeHtml(baselineAuditMessage(item))}</span><small>${escapeHtml(item.createdAtUtc)}</small></div>`).join("")
    : `<div class="table-empty"><strong>没有审计记录</strong></div>`;
  renderBaselineReferences(references);
}

async function openBaselineSnapshotDetail(snapshotId) {
  const response = await fetch(`/api/current-baselines/${snapshotId}`, { headers: { Accept: "application/json" } });
  if (!response.ok) throw new Error(`冻结基线详情接口失败：${response.status}`);
  const snapshot = await response.json();
  const planningEvidence = renderBaselinePlanningEvidence(snapshot, "Frozen", false);
  const historyReconciliation = renderBaselineHistoryReconciliation(snapshot, "Frozen", false);
  const parameters = valueOr(snapshot.payload?.planningInputs?.ddmrpParameters, []);
  const items = parameters.map(item => [
    `${item.sku} ${item.name}`,
    item.leadTimeFactor == null
      ? "旧版本缺少提前期因子；该快照保持只读，不能用于重算"
      : `提前期因子 ${number(item.leadTimeFactor)} · ${escapeHtml(evidenceStatusLabel(item.evidenceStatus))}`,
  ]);
  openWorkspaceDrawer("冻结基线证据", [...planningEvidence.drawerSections, ...historyReconciliation.drawerSections, {
    title: `${snapshot.snapshotNumber} · ${baselineStatusLabel(snapshot.status)}`,
    items: items.length ? items : [["定容证据", "旧版本未保存 DDMRP 参数明细"]],
  }]);
}

function renderBaselineReferences(references) {
  const groups = [
    ["场景运行", references.scenarioRuns || [], item => `${item.runNumber} · ${item.name}`],
    ["配置变更", references.masterSettingChanges || [], item => `${item.changeNumber} · ${masterSettingDisplayValue(item.changeId, "target", item.target)}`],
    ["行动事项", references.coordinationItems || [], item => `${item.itemNumber} · ${item.title}`],
  ];
  byId("baseline-reference-list").innerHTML = groups.map(([label, items, describe]) => `
    <div class="diagnostic-item"><strong>${label} · ${number(items.length)}</strong><span>${items.length ? items.map(describe).map(escapeHtml).join("；") : "未关联"}</span></div>
  `).join("");
}

function configureFiveStageScenarioControls() {
  const resources = state.data?.resources || [];
  byId("external-capacity-resource").innerHTML = resources.map(item => `<option value="${escapeHtml(item.code)}">${escapeHtml(item.name)} (${escapeHtml(item.code)})</option>`).join("");
  const suppliers = unique((state.data?.supplierCapacityWindows || []).map(item => `${item.supplier}|${item.materialFamily}`));
  byId("external-supplier-risk").innerHTML = suppliers.map(value => {
    const [supplier, family] = value.split("|");
    return `<option value="${escapeHtml(value)}">${escapeHtml(supplier)} / ${escapeHtml(family)}</option>`;
  }).join("");
  const timeBuffers = state.data?.timeBuffers || [];
  byId("external-time-control-point").innerHTML = timeBuffers.length
    ? timeBuffers.map(item => `<option value="${escapeHtml(item.bufferId)}">${escapeHtml(item.controlPoint)} · ${escapeHtml(item.protectedActivity)}</option>`).join("")
    : `<option value="">没有可用时间控制点</option>`;
  setAssumptionModeUi();
}

async function loadScenarioAssumptionTemplates() {
  const response = await fetch("/api/scenario-assumptions/templates", { headers: { Accept: "application/json" } });
  if (!response.ok) throw new Error(`演示场景模板接口失败：${response.status}`);
  state.scenarioAssumptionTemplates = await response.json();
  byId("future-assumption-template").innerHTML = state.scenarioAssumptionTemplates.length
    ? state.scenarioAssumptionTemplates.map(item => `<option value="${escapeHtml(item.templateId)}">${escapeHtml(item.name)}</option>`).join("")
    : `<option value="">没有内置演示模板</option>`;
  setAssumptionModeUi();
}

function setAssumptionModeUi() {
  const isDemo = byId("future-assumption-mode").value === "DemoFixture";
  document.querySelectorAll("[data-assumption-template-field]").forEach(field => { field.hidden = !isDemo; });
  document.querySelectorAll("[data-assumption-manual-field]").forEach(field => { field.hidden = isDemo; });
  const evidence = byId("future-assumption-evidence");
  if (!evidence.readOnly || !evidence.dataset.manualValue) evidence.dataset.manualValue = evidence.value;
  if (isDemo) {
    const template = state.scenarioAssumptionTemplates.find(item => item.templateId === byId("future-assumption-template").value);
    evidence.value = template ? `版本 ${template.templateVersion} · ${baselineSourceLabel("DemoFixture")}` : baselineSourceLabel("DemoFixture");
    evidence.readOnly = true;
  } else {
    evidence.value = evidence.dataset.manualValue;
    evidence.readOnly = false;
  }
}

function buildResponseConfigurations(resourceCode, supplyRisk) {
  const sku = state.data?.skus?.[0];
  const frozenSupplyWindows = state.futureComparisonBaseline?.payload?.planningInputs?.supplierCapacityWindows || [];
  const responseOptions = [];
  if (byId("response-temporary-capacity").checked && resourceCode) {
    responseOptions.push({ responseId: "RESP-TEMP-CAPACITY", name: "临时能力", parameters: { capacityAdjustments: [3, 4, 5, 6, 7, 8, 9].map(week => ({ resourceCode, week, capacityMultiplier: 1.35, reason: "临时能力响应" })) } });
  }
  if (byId("response-policy-cover").checked && sku) {
    responseOptions.push({ responseId: "RESP-POLICY-COVER", name: "MOQ / 订货周期覆盖", parameters: { skuPolicyOverrides: [{ sku: sku.sku, minimumOrderQuantity: Math.max(1, Number(sku.minimumOrderQuantity) * 0.8), orderCycleDays: Math.max(1, Number(sku.orderCycleDays) - 2) }] } });
  }
  if (byId("response-supply-recovery").checked && supplyRisk) {
    const startWeek = Math.max(1, Number(supplyRisk.startWeek));
    const endWeek = Math.max(startWeek, Number(supplyRisk.endWeek));
    const supplierCapacityLimits = frozenSupplyWindows
      .filter(item => item.supplier === supplyRisk.supplier && item.materialFamily === supplyRisk.materialFamily && Number(item.week) >= startWeek && Number(item.week) <= endWeek)
      .map(item => ({ supplier: item.supplier, materialFamily: item.materialFamily, startWeek: Number(item.week), endWeek: Number(item.week), committedCapacity: Number(item.committedCapacity) }));
    if (supplierCapacityLimits.length) {
      responseOptions.push({ responseId: "RESP-SUPPLY-RECOVERY", name: "供应响应", parameters: { supplierCapacityLimits } });
    }
  }
  if (byId("response-prebuild").checked && sku) {
    responseOptions.push({ responseId: "RESP-PREBUILD", name: "提前建库", parameters: { prebuildCampaigns: [{ campaignId: "FUTURE-PREBUILD", sku: sku.sku, buildWeek: 1, protectFromWeek: 3, protectThroughWeek: 8, quantity: Math.max(1, Number(sku.minimumOrderQuantity)) }] } });
  }
  return responseOptions;
}

function buildScenarioComparisonRequest() {
  const baselineSnapshotId = byId("future-baseline-select").value;
  if (!baselineSnapshotId) throw new Error("请先在当前状态基线中冻结一个版本。");
  const resourceCode = byId("external-capacity-resource").value;
  const [supplier, materialFamily] = (byId("external-supplier-risk").value || "|").split("|");
  if (byId("future-assumption-mode").value === "DemoFixture") {
    const template = state.scenarioAssumptionTemplates.find(item => item.templateId === byId("future-assumption-template").value);
    if (!template) throw new Error("请选择 DDAE 内置演示模板。");
    const externalScenario = JSON.parse(JSON.stringify(template.externalScenario));
    const responseResource = externalScenario.capacityLosses?.[0]?.resourceCode || resourceCode;
    const responseSupplyRisk = externalScenario.supplyRisks?.[0] || null;
    return {
      baselineSnapshotId,
      horizonWeeks: 12,
      externalScenario,
      responseOptions: buildResponseConfigurations(responseResource, responseSupplyRisk),
    };
  }
  const recordedBy = byId("future-assumption-entered-by").value.trim();
  const effectiveFrom = byId("future-assumption-effective-from").value;
  const effectiveThrough = byId("future-assumption-effective-through").value;
  const rationale = byId("future-assumption-evidence").value.trim();
  if (!recordedBy || !effectiveFrom || !effectiveThrough || !rationale) {
    throw new Error("人工场景必须填写记录人、有效期和依据。");
  }
  const timeBufferId = byId("external-time-control-point").value;
  const delayDays = Number(byId("external-time-delay-days").value);
  const externalScenario = {
      scenarioId: `EXT-UI-${Date.now()}`,
      name: byId("external-scenario-name").value || "外部风险场景",
      demandChanges: [{ sku: null, family: null, startWeek: 2, endWeek: 8, demandMultiplier: Number(byId("external-demand-multiplier").value), reason: "外部需求变化" }],
      supplyRisks: supplier ? [{ supplier, materialFamily, startWeek: 3, endWeek: 8, availableCapacityMultiplier: Number(byId("external-supply-multiplier").value), reason: "供应能力风险" }] : [],
      capacityLosses: resourceCode ? [{ resourceCode, startWeek: 3, endWeek: 6, availableCapacityMultiplier: Number(byId("external-capacity-multiplier").value), reason: "已知能力损失" }] : [],
      knownEvents: [{ eventId: "EVENT-UI-001", name: "已知业务窗口", startWeek: 2, endWeek: 8 }],
      timeDelays: timeBufferId && delayDays > 0 ? [{ bufferId: timeBufferId, startWeek: 2, endWeek: 8, delayDays, reason: rationale }] : [],
      metadata: {
        sourceKind: "Manual",
        templateId: null,
        templateVersion: null,
        recordedBy,
        recordedAtUtc: new Date().toISOString(),
        effectiveFrom,
        effectiveThrough,
        rationale,
        evidenceLabel: `人工录入：${rationale}`,
      },
    };
  return {
    baselineSnapshotId,
    horizonWeeks: 12,
    externalScenario,
    responseOptions: buildResponseConfigurations(resourceCode, externalScenario.supplyRisks[0] || null),
  };
}

function renderTimeBufferBreachEvidence(result, baselineDetail) {
  const select = byId("time-buffer-breach-select");
  const chip = byId("time-buffer-breach-evidence-chip");
  const summary = byId("time-buffer-breach-summary");
  const weeklyGrid = byId("time-buffer-breach-weekly-grid");
  if (!result) {
    state.selectedTimeBufferBreachKey = null;
    select.innerHTML = `<option value="">等待后端结果</option>`;
    chip.className = "status-chip neutral";
    chip.textContent = "等待场景比较";
    summary.innerHTML = "";
    weeklyGrid.innerHTML = `<div class="table-empty"><strong>运行冻结基线比较后显示时间缓冲证据</strong></div>`;
    return;
  }
  const cases = result.allCases || [result.noResponse, ...(result.responseCases || [])];
  const planningInputs = baselineDetail?.payload?.planningInputs;
  const definitions = planningInputs && planningInputs.timeBuffers ? planningInputs.timeBuffers : [];
  const timeBufferResults = cases.flatMap(item => (item.breaches || [])
    .filter(breach => breach.scopeType === "TimeBuffer" || breach.scopeType === "Time")
    .map(breach => {
      const definitionMatches = definitions.filter(item => item.bufferId === breach.target);
      const points = (item.timeBufferProjection || [])
        .filter(point => point.bufferId === breach.target)
        .sort((left, right) => left.week - right.week);
      const projectionWeekKeys = points.map(point => point.week);
      const projectionWeeksUnique = new Set(projectionWeekKeys).size === projectionWeekKeys.length;
      const expectedHorizonWeeks = Number(item.preview?.request?.horizonWeeks);
      const horizonIsValid = Number.isInteger(expectedHorizonWeeks) && expectedHorizonWeeks > 0;
      const projectionWeekSet = new Set(projectionWeekKeys);
      const missingWeekCandidate = horizonIsValid
        ? Array.from({ length: expectedHorizonWeeks }, (_, index) => index + 1)
          .find(week => !projectionWeekSet.has(week))
        : undefined;
      const firstMissingWeek = missingWeekCandidate === undefined ? null : missingWeekCandidate;
      const outOfRangeWeekCandidate = horizonIsValid
        ? projectionWeekKeys.find(week => !Number.isInteger(week) || week < 1 || week > expectedHorizonWeeks)
        : undefined;
      const firstOutOfRangeWeek = outOfRangeWeekCandidate === undefined ? null : outOfRangeWeekCandidate;
      const projectionCoversHorizon = horizonIsValid
        && projectionWeekKeys.length === expectedHorizonWeeks
        && firstMissingWeek === null
        && firstOutOfRangeWeek === null;
      const completeEnvelope = breach.evidenceStatus === "Complete"
        && definitionMatches.length === 1
        && definitionMatches[0].evidenceStatus === "Complete"
        && projectionWeeksUnique
        && projectionCoversHorizon
        && points.every(point => point.evidenceStatus === "Complete");
      const baseResult = {
        key: `${item.responseId}|${breach.target}`,
        responseId: item.responseId,
        caseName: item.name,
        breach,
        definition: definitionMatches.length === 1 ? definitionMatches[0] : null,
        points,
        hasDuplicateWeeks: !projectionWeeksUnique,
        evidenceReason: breach.evidenceStatus !== "Complete"
          ? evidenceStatusLabel(breach.evidenceStatus)
          : definitionMatches.length === 0 ? "冻结基线缺少时间缓冲定义"
            : definitionMatches.length > 1 ? "冻结基线存在重复时间缓冲定义"
              : definitionMatches[0].evidenceStatus !== "Complete" ? "冻结时间缓冲定义证据缺失"
                : points.length === 0 ? "后端未返回周度侵入证据"
                  : !projectionWeeksUnique ? "周度证据包含重复周"
                    : !horizonIsValid ? "后端未提供有效展望期"
                      : firstOutOfRangeWeek !== null ? `周度证据包含展望期外的第 ${firstOutOfRangeWeek} 周`
                        : firstMissingWeek !== null ? `周度证据缺少第 ${firstMissingWeek} 周`
                          : !projectionCoversHorizon ? "周度证据未完整覆盖展望期"
                            : points.some(point => point.evidenceStatus !== "Complete") ? "周度侵入证据不完整"
                              : "后端证据完整",
      };
      if (breach.evidenceStatus === "NotApplicable") {
        return { ...baseResult, effectiveEvidenceStatus: "NotApplicable" };
      }
      return { ...baseResult, effectiveEvidenceStatus: completeEnvelope ? "Complete" : "EvidenceMissing" };
    }));

  if (!timeBufferResults.length) {
    state.selectedTimeBufferBreachKey = null;
    select.innerHTML = `<option value="">没有后端时间缓冲结果</option>`;
    chip.className = "status-chip is-warning";
    chip.textContent = "证据缺失";
    summary.innerHTML = `<div><dt>证据状态</dt><dd>证据缺失</dd></div><div><dt>说明</dt><dd>当前比较未返回时间缓冲击穿证据</dd></div>`;
    weeklyGrid.innerHTML = `<div class="table-empty"><strong>没有可显示的后端周度侵入证据</strong></div>`;
    return;
  }

  const retained = timeBufferResults.find(item => item.key === state.selectedTimeBufferBreachKey);
  const firstComplete = timeBufferResults.find(item => item.effectiveEvidenceStatus === "Complete");
  const firstUnavailable = timeBufferResults.find(item => item.effectiveEvidenceStatus === "NotApplicable" || item.effectiveEvidenceStatus === "EvidenceMissing");
  const selected = retained || firstComplete || firstUnavailable || timeBufferResults[0];
  state.selectedTimeBufferBreachKey = selected.key;
  select.innerHTML = timeBufferResults.map(item => {
    const definition = item.definition;
    const controlPoint = definition?.controlPoint || item.points[0]?.controlPoint || item.breach.target;
    return `<option value="${escapeHtml(item.key)}">${escapeHtml(item.caseName)} · ${escapeHtml(controlPoint)}</option>`;
  }).join("");
  select.value = selected.key;

  const evidenceStatus = selected.effectiveEvidenceStatus;
  const evidenceComplete = evidenceStatus === "Complete";
  const evidenceUnavailable = evidenceStatus === "NotApplicable" ? "不适用" : "证据缺失";
  chip.className = `status-chip ${evidenceComplete ? "is-valid" : evidenceStatus === "NotApplicable" ? "neutral" : "is-warning"}`;
  chip.textContent = evidenceStatusLabel(evidenceStatus);

  const definition = selected.definition;
  const earliestRedWeek = !evidenceComplete
    ? evidenceUnavailable
    : selected.breach.isBreached
      ? metricOrEvidenceMissing(selected.breach.earliestRedWeek, value => `第 ${number(value)} 周`)
      : "未击穿";
  const consecutiveRisk = !evidenceComplete
    ? evidenceUnavailable
    : selected.breach.isBreached ? `${number(selected.breach.consecutiveRiskWeeks)} 周` : "不适用";
  const recovery = !evidenceComplete
    ? evidenceUnavailable
    : !selected.breach.isBreached ? "不适用"
      : selected.breach.isUnrecovered ? "展望期未恢复"
        : metricOrEvidenceMissing(selected.breach.recoveryWeek, value => `第 ${number(value)} 周`);
  const summaryItems = [
    ["控制点", definition?.controlPoint || evidenceUnavailable],
    ["保护活动", definition?.protectedActivity || evidenceUnavailable],
    ["缓冲天数", evidenceComplete && definition?.bufferDays !== null && definition?.bufferDays !== undefined ? `${number(definition.bufferDays)} 天` : evidenceUnavailable],
    ["最大侵入", evidenceComplete ? metricOrEvidenceMissing(selected.breach.maximumPenetrationPercent, percent) : evidenceUnavailable],
    ["最早红周", earliestRedWeek],
    ["连续风险", consecutiveRisk],
    ["恢复周期", recovery],
    ["影响产品", evidenceComplete ? (selected.breach.affectedProducts || []).join("、") || "不适用" : evidenceUnavailable],
    ["证据状态", evidenceStatusLabel(evidenceStatus)],
    ["证据说明", selected.evidenceReason],
  ];
  summary.innerHTML = summaryItems.map(item => `<div><dt>${escapeHtml(item[0])}</dt><dd>${escapeHtml(item[1])}</dd></div>`).join("");

  const statusText = status => ({ Early: "提前", Green: "绿色", Yellow: "黄色", Red: "红色", Late: "迟到", EvidenceMissing: "证据缺失" })[status] || statusLabel(status);
  if (evidenceStatus === "NotApplicable") {
    weeklyGrid.innerHTML = `<div class="table-empty"><strong>该控制点不适用时间缓冲</strong></div>`;
  } else if (selected.hasDuplicateWeeks) {
    weeklyGrid.innerHTML = `<div class="table-empty"><strong>证据缺失：周度证据包含重复周，未生成周矩阵</strong></div>`;
  } else if (!selected.points.length) {
    weeklyGrid.innerHTML = `<div class="table-empty"><strong>证据缺失：该后端结果未返回周度侵入证据</strong></div>`;
  } else {
    const evidenceNotice = evidenceComplete
      ? ""
      : `<p class="muted-note">证据缺失：${escapeHtml(selected.evidenceReason)}；以下仅保留后端原始周度行。</p>`;
    weeklyGrid.innerHTML = `${evidenceNotice}<table class="heatmap-table"><thead><tr><th>方案 / 控制点</th>${selected.points.map(point => `<th>第 ${number(point.week)} 周</th>`).join("")}</tr></thead><tbody><tr><th>${escapeHtml(selected.caseName)}<small>${escapeHtml(definition?.controlPoint || selected.points[0]?.controlPoint || selected.breach.target)}</small></th>${selected.points.map(point => {
      const pointEvidenceComplete = point.evidenceStatus === "Complete";
      return `<td><span class="${bufferCellClass(pointEvidenceComplete ? point.status : "EvidenceMissing")}" title="${escapeHtml(point.cause)}"><strong>${pointEvidenceComplete ? metricOrEvidenceMissing(point.penetrationPercent, percent) : "证据缺失"}</strong><small>${escapeHtml(statusText(point.status))} / ${escapeHtml(evidenceStatusLabel(point.evidenceStatus))} · ${escapeHtml(point.cause)}</small></span></td>`;
    }).join("")}</tr></tbody></table>`;
  }
}

function renderFutureCapacityProtection(result) {
  const cases = result.allCases || [result.noResponse, ...(result.responseCases || [])];
  const projectionRows = cases.flatMap(item => (item.capacityProtectionProjection || []).map(point => ({ caseName: item.name, ...point })));
  byId("future-capacity-protection-body").innerHTML = projectionRows.length
    ? projectionRows.map(item => {
      const measure = item.measure;
      return row([
      escapeHtml(item.caseName), escapeHtml(item.upstreamResourceCode), escapeHtml(item.protectedCcrResourceCode), `第 ${number(item.week)} 周`,
      metricOrEvidenceMissing(item.plannedAvailableCapacity, number), metricOrEvidenceMissing(item.committedLoad, number),
      metricOrEvidenceMissing(measure?.utilizationPercent, percent), metricOrEvidenceMissing(measure?.protectionStart, number),
      metricOrEvidenceMissing(measure?.protectionCapacity, number), metricOrEvidenceMissing(measure?.consumedProtection, number),
      metricOrEvidenceMissing(measure?.remainingProtection, number), metricOrEvidenceMissing(measure?.overload, number),
      capacityUtilizationBandChip(measure),
    ]);
    }).join("")
    : emptyRow("没有上游能力保护证据", 13);
  const ccrRows = cases.flatMap(item => (item.preview?.scenario?.plan?.capacityLoads || [])
    .filter(resource => resource.relationshipRole === "CcrUtilization")
    .map(resource => ({ caseName: item.name, ...resource })));
  byId("future-ccr-utilization-body").innerHTML = ccrRows.length
    ? ccrRows.map(item => {
      const measure = item.capacityProtectionMeasure;
      return row([
        escapeHtml(item.caseName),
        `${escapeHtml(item.resourceName)}<small>${escapeHtml(item.resourceCode)}</small>`,
        `第 ${number(item.week)} 周`,
        metricOrEvidenceMissing(item.availableCapacity, number),
        metricOrEvidenceMissing(item.requiredCapacity, number),
        metricOrEvidenceMissing(measure?.utilizationPercent, percent),
        capacityUtilizationBandChip(measure),
      ]);
    }).join("")
    : emptyRow("没有 CCR 利用率证据", 7);
  renderCapacityBandDistribution(
    "future-capacity-utilization-distribution",
    projectionRows
      .filter(item => item.measure?.evidenceStatus === "Complete")
      .map(item => ({ band: item.measure.utilizationBand, count: 1 })));
}

function futureComparisonKey(responseId) {
  return `${state.futureComparisonRequest?.baselineSnapshotId || ""}|${state.futureComparisonRequest?.externalScenario?.scenarioId || ""}|${responseId || ""}`;
}

function savedFutureComparison(responseId) {
  return state.savedFutureComparisons[futureComparisonKey(responseId)] || null;
}

function renderFutureComparison(result) {
  state.futureComparison = result;
  const cases = result.allCases || [result.noResponse, ...(result.responseCases || [])];
  const options = cases.map(item => `<option value="${escapeHtml(item.responseId)}">${escapeHtml(item.name)}</option>`).join("");
  byId("governance-response-id").innerHTML = options;
  byId("future-comparison-save-response-id").innerHTML = options;
  byId("governance-baseline-id").value = result.baselineSnapshotId;
  byId("save-future-comparison").disabled = false;
  byId("future-comparison-save-status").className = "status-chip is-paused";
  byId("future-comparison-save-status").textContent = "比较完成，尚未保存";
  byId("future-compare-status").className = "status-chip is-valid";
  byId("future-compare-status").textContent = `${result.baselineSnapshotNumber} · ${cases.length} 个方案`;
  byId("future-comparison-cards").innerHTML = cases.map(item => {
    const metrics = item.preview.scenario.metrics;
    const breachEvidenceComplete = item.breaches.length > 0 && item.breaches.every(breach => breach.evidenceStatus === "Complete" || breach.evidenceStatus === "NotApplicable");
    const allBreachEvidenceNotApplicable = item.breaches.length > 0 && item.breaches.every(breach => breach.evidenceStatus === "NotApplicable");
    const breached = item.breaches.filter(breach => breach.evidenceStatus === "Complete" && breach.isBreached).length;
    const breachCountLabel = !breachEvidenceComplete ? "证据缺失" : allBreachEvidenceNotApplicable ? "不适用" : number(breached);
    return `<div class="comparison-column ${item.responseId === "NO_RESPONSE" ? "no-response-case" : "is-recommended"}"><h3>${escapeHtml(item.name)}</h3><p>${item.responseId === "NO_RESPONSE" ? "外部场景，不采取企业措施" : "外部场景 + 企业响应配置"}</p><div class="comparison-metrics"><div><span>服务</span><strong>${percent(metrics.serviceLevelPercent)}</strong></div><div><span>平均库存</span><strong>${metricOrEvidenceMissing(metrics.averageInventoryValue, money)}</strong></div><div><span>补货释放峰值</span><strong>${percent(metrics.peakLoadPercent)}</strong></div><div><span>供应缺口</span><strong>${number(metrics.supplyGap)}</strong></div><div><span>击穿对象</span><strong>${breachCountLabel}</strong></div></div></div>`;
  }).join("");
  const breachRows = cases.flatMap(item => item.breaches.map(breach => ({ caseName: item.name, ...breach })));
  byId("future-breach-body").innerHTML = breachRows.length
    ? breachRows.map(item => {
      const breachEvidenceAvailable = item.evidenceStatus === "Complete";
      const unavailableEvidence = item.evidenceStatus === "NotApplicable" ? "不适用" : "证据缺失";
      return row([
        escapeHtml(item.caseName),
        escapeHtml(breachScopeLabel(item.scopeType)),
        escapeHtml(item.target),
        !breachEvidenceAvailable ? unavailableEvidence : item.isBreached ? `第 ${number(item.earliestRedWeek)} 周` : "未击穿",
        !breachEvidenceAvailable ? unavailableEvidence : item.isBreached ? `${number(item.consecutiveRiskWeeks)} 周` : "不适用",
        !breachEvidenceAvailable ? unavailableEvidence : !item.isBreached ? "不适用" : item.isUnrecovered ? `<span class="status-chip is-invalid">展望期未恢复</span>` : metricOrEvidenceMissing(item.recoveryWeek, value => `第 ${number(value)} 周`),
        !breachEvidenceAvailable ? unavailableEvidence : escapeHtml((item.affectedProducts || []).join("、") || "不适用"),
        !breachEvidenceAvailable ? unavailableEvidence : escapeHtml(businessEvidenceLabel(item.primaryCause)),
      ]);
    }).join("")
    : emptyRow("没有可计算的保护击穿证据", 8);
  renderTimeBufferBreachEvidence(result, state.futureComparisonBaseline);
  renderFutureCapacityProtection(result);
}

async function saveFutureComparison() {
  if (!state.futureComparison || !state.futureComparisonRequest) throw new Error("请先运行冻结基线比较。");
  const responseId = byId("future-comparison-save-response-id").value;
  if (!responseId) throw new Error("请选择要保存的比较方案。");
  byId("future-comparison-save-status").className = "status-chip is-warning";
  byId("future-comparison-save-status").textContent = "后端重新计算并保存中";
  const response = await fetch("/api/scenario-runs/compare/save", {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify({
      comparison: state.futureComparisonRequest,
      responseId,
      name: byId("future-comparison-save-name").value,
      description: "由未来场景比较显式保存",
      createdBy: byId("future-comparison-save-created-by").value,
    }),
  });
  const payload = await response.json();
  if (!response.ok) throw new Error(payload.message || `冻结比较保存失败：${response.status}`);
  state.savedFutureComparisons[futureComparisonKey(responseId)] = payload;
  state.selectedScenarioRunId = payload.runId;
  byId("future-comparison-save-status").className = "status-chip is-valid";
  byId("future-comparison-save-status").textContent = `${payload.runNumber} 已保存`;
  await loadSavedScenarioRuns(payload.runId);
  configureCoordinationLineageSelectors();
}

async function runScenarioComparison() {
  byId("future-compare-status").className = "status-chip is-warning";
  byId("future-compare-status").textContent = "后端白盒重算中";
  const baselineSnapshotId = byId("future-baseline-select").value;
  if (!baselineSnapshotId) throw new Error("请先在当前状态基线中冻结一个版本。");
  const baselineResponse = await fetch(`/api/current-baselines/${encodeURIComponent(baselineSnapshotId)}`, { headers: { Accept: "application/json" } });
  if (!baselineResponse.ok) throw new Error(`冻结基线详情接口失败：${baselineResponse.status}`);
  state.futureComparisonBaseline = await baselineResponse.json();
  const request = buildScenarioComparisonRequest();
  const response = await fetch("/api/scenario-runs/compare", {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify(request),
  });
  const payload = await response.json();
  if (!response.ok) throw new Error(payload.message || `场景比较失败：${response.status}`);
  state.futureComparisonRequest = request;
  state.savedFutureComparisons = {};
  renderFutureComparison(payload);
}

function renderCoordinationItems() {
  byId("coordination-status-chip").className = "status-chip is-valid";
  byId("coordination-status-chip").textContent = `${state.coordinationItems.length} 个事项`;
  byId("coordination-item-body").innerHTML = state.coordinationItems.length
    ? state.coordinationItems.map(item => `<tr class="interactive-row" tabindex="0" data-coordination-item-id="${escapeHtml(item.itemId)}"><td><strong>${escapeHtml(item.itemNumber)}</strong></td><td>${escapeHtml(item.title)}</td><td>${escapeHtml(item.owner)}</td><td>${escapeHtml(item.dueDate)}</td><td>${escapeHtml(item.escalationLevel)}</td><td>${escapeHtml(item.nextReviewDate)}</td><td>${escapeHtml(item.relatedScenarioRunId || "-")}</td><td>${escapeHtml(item.relatedDdomPackageId || item.relatedMasterSettingChangeId || "-")}</td><td><span class="${statusClass(item.status === "Completed" ? "Green" : item.status === "Escalated" ? "Red" : "Yellow")}">${escapeHtml(coordinationStatusLabel(item.status))}</span></td></tr>`).join("")
    : emptyRow("尚未创建协调事项", 9);
}

function configureCoordinationLineageSelectors() {
  const scenarioSelect = byId("coordination-related-scenario");
  const changeSelect = byId("coordination-related-change");
  const packageSelect = byId("coordination-related-ddom-package");
  const sourceRunSelect = byId("ddom-source-run");
  if (!scenarioSelect || !changeSelect || !packageSelect || !sourceRunSelect) return;
  const previousScenario = scenarioSelect.value;
  const previousChange = changeSelect.value;
  const previousPackage = packageSelect.value;
  const previousSourceRun = sourceRunSelect.value;
  const coordinationRuns = state.savedScenarioRuns || [];
  scenarioSelect.innerHTML = [`<option value="">不关联</option>`, ...coordinationRuns.map(item => `<option value="${escapeHtml(item.runId)}">${escapeHtml(item.runNumber)} · ${escapeHtml(item.name)}</option>`)].join("");
  const changes = state.masterSettings?.recentChanges || [];
  const packages = Array.isArray(state.ddomPackages) ? state.ddomPackages : [];
  const selectedRuns = (state.savedScenarioRuns || []).filter(item => item.candidateStatus === "Selected"
    && item.baselineSnapshotId && item.externalScenarioId && item.responseId
    && (item.feasibilityStatus === "Adoptable" || item.feasibilityStatus === "Reconcile"));
  changeSelect.innerHTML = [`<option value="">不关联</option>`, ...changes.map(item => `<option value="${escapeHtml(item.changeId)}">${escapeHtml(item.changeNumber)} · ${escapeHtml(masterSettingDisplayValue(item.changeId, "target", item.target))}</option>`)].join("");
  packageSelect.innerHTML = [`<option value="">不关联</option>`, ...packages.map(item => `<option value="${escapeHtml(item.packageId)}">${escapeHtml(item.packageNumber)} · ${escapeHtml(item.name)}</option>`)].join("");
  sourceRunSelect.innerHTML = [`<option value="">请先在第 03 阶段人工选定候选</option>`, ...selectedRuns.map(item => `<option value="${escapeHtml(item.runId)}">${escapeHtml(item.runNumber)} · ${escapeHtml(item.name)} · ${escapeHtml(statusLabel(item.feasibilityStatus))}</option>`)].join("");
  if (coordinationRuns.some(item => item.runId === previousScenario)) scenarioSelect.value = previousScenario;
  if (changes.some(item => item.changeId === previousChange)) changeSelect.value = previousChange;
  if (packages.some(item => item.packageId === previousPackage)) packageSelect.value = previousPackage;
  if (selectedRuns.some(item => item.runId === previousSourceRun)) sourceRunSelect.value = previousSourceRun;
}

async function loadCoordinationItems(selectFirst = false) {
  const response = await fetch("/api/coordination-items?limit=50", { headers: { Accept: "application/json" } });
  if (!response.ok) throw new Error(`协调事项接口失败：${response.status}`);
  state.coordinationItems = await response.json();
  renderCoordinationItems();
  if (selectFirst && state.coordinationItems.length) await loadCoordinationDetail(state.coordinationItems[0].itemId);
}

function renderCoordinationDetail(item, audit) {
  state.selectedCoordinationItemId = item.itemId;
  byId("coordination-detail-title").textContent = `${item.itemNumber} · ${item.title}`;
  byId("coordination-detail-status").className = statusClass(item.status === "Completed" ? "Green" : item.status === "Escalated" ? "Red" : "Yellow");
  byId("coordination-detail-status").textContent = coordinationStatusLabel(item.status);
  byId("coordination-detail-summary").innerHTML = [
    ["影响对象", item.impactObjects.join("、")], ["负责人", item.owner], ["截止日期", item.dueDate], ["升级层级", item.escalationLevel],
    ["决策要求", item.decisionRequired], ["场景运行", item.relatedScenarioRunId || "未关联"], ["DDOM 变更包", item.relatedDdomPackageId || item.relatedMasterSettingChangeId || "未关联"], ["实际效果", item.actualOutcome || "待验证"],
  ].map(([label, value]) => `<div><dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value)}</dd></div>`).join("");
  byId("coordination-decision").value = item.decision || "";
  byId("coordination-rationale").value = item.decisionRationale || "";
  byId("coordination-outcome").value = item.actualOutcome || "";
  byId("coordination-audit-list").innerHTML = audit.map(event => `<div class="diagnostic-item"><strong>${number(event.sequence)}. ${escapeHtml(auditEventLabel(event.eventType))}</strong><span>${escapeHtml(businessEvidenceLabel(event.message))}</span><small>${escapeHtml(event.actor)} · ${escapeHtml(event.createdAtUtc)}</small></div>`).join("");
}

async function renderCoordinationLineage(item) {
  if (!item.relatedScenarioRunId && !item.relatedMasterSettingChangeId && !item.relatedDdomPackageId) {
    byId("coordination-lineage-list").innerHTML = `<div class="diagnostic-item"><strong>只读反查</strong><span>当前事项未关联场景或配置变更</span></div>`;
    return;
  }

  const [scenarioDetailResponse, changeDetailResponse, scenarioItemsResponse, changeItemsResponse, packageItemsResponse] = await Promise.all([
    item.relatedScenarioRunId
      ? fetch(`/api/scenario-runs/${encodeURIComponent(item.relatedScenarioRunId)}`, { headers: { Accept: "application/json" } })
      : Promise.resolve(null),
    item.relatedMasterSettingChangeId
      ? fetch(`/api/master-settings/changes/${encodeURIComponent(item.relatedMasterSettingChangeId)}`, { headers: { Accept: "application/json" } })
      : Promise.resolve(null),
    item.relatedScenarioRunId
      ? fetch(`/api/coordination-items?limit=50&relatedScenarioRunId=${encodeURIComponent(item.relatedScenarioRunId)}`, { headers: { Accept: "application/json" } })
      : Promise.resolve(null),
    item.relatedMasterSettingChangeId
      ? fetch(`/api/coordination-items?limit=50&relatedMasterSettingChangeId=${encodeURIComponent(item.relatedMasterSettingChangeId)}`, { headers: { Accept: "application/json" } })
      : Promise.resolve(null),
    item.relatedDdomPackageId
      ? fetch(`/api/coordination-items?limit=50&relatedDdomPackageId=${encodeURIComponent(item.relatedDdomPackageId)}`, { headers: { Accept: "application/json" } })
      : Promise.resolve(null),
  ]);
  const responses = [scenarioDetailResponse, changeDetailResponse, scenarioItemsResponse, changeItemsResponse, packageItemsResponse].filter(Boolean);
  if (responses.some(response => !response.ok && response.status !== 404)) throw new Error("协调事项只读反查失败。");
  const scenarioDetail = scenarioDetailResponse?.ok ? await scenarioDetailResponse.json() : null;
  const changeDetail = changeDetailResponse?.ok ? await changeDetailResponse.json() : null;
  const relatedGroups = await Promise.all([scenarioItemsResponse, changeItemsResponse, packageItemsResponse]
    .filter(response => response?.ok)
    .map(response => response.json()));
  const relatedCount = new Set(relatedGroups.flat().map(related => related.itemId)).size;
  const scenarioSummary = scenarioDetail?.summary;
  const changeSummary = changeDetail?.summary;
  const scenarioLink = item.relatedScenarioRunId
    ? `<button class="link-button" type="button" data-lineage-scenario-run-id="${escapeHtml(item.relatedScenarioRunId)}"><strong>${escapeHtml(scenarioSummary?.runNumber || item.relatedScenarioRunId)}</strong><small>${escapeHtml(scenarioSummary?.name || "场景详情证据缺失")}</small></button>`
    : "未关联场景";
  const changeLink = item.relatedMasterSettingChangeId
    ? `<button class="link-button" type="button" data-lineage-master-change-id="${escapeHtml(item.relatedMasterSettingChangeId)}"><strong>${escapeHtml(changeSummary?.changeNumber || item.relatedMasterSettingChangeId)}</strong><small>${escapeHtml(changeSummary ? masterSettingDisplayValue(changeSummary.changeId, "target", changeSummary.target) : "变更详情证据缺失")}</small></button>`
    : "未关联主设置变更";
  const package = (state.ddomPackages || []).find(candidate => candidate.packageId === item.relatedDdomPackageId);
  const packageLink = item.relatedDdomPackageId
    ? `<button class="link-button" type="button" data-lineage-ddom-package-id="${escapeHtml(item.relatedDdomPackageId)}"><strong>${escapeHtml(package?.packageNumber || item.relatedDdomPackageId)}</strong><small>${escapeHtml(package?.name || "DDOM 变更包详情待加载")}</small></button>`
    : "未关联 DDOM 变更包";
  byId("coordination-lineage-list").innerHTML = `
    <div class="diagnostic-item"><strong>03 关联场景</strong><span>${scenarioLink}</span></div>
    <div class="diagnostic-item"><strong>04 关联变更</strong><span>${packageLink !== "未关联 DDOM 变更包" ? packageLink : changeLink}</span></div>
    <div class="diagnostic-item"><strong>只读反查</strong><span>同一关联对象下共有 ${number(relatedCount)} 条行动事项</span></div>`;
}

async function loadCoordinationDetail(itemId) {
  const [detailResponse, auditResponse] = await Promise.all([
    fetch(`/api/coordination-items/${itemId}`, { headers: { Accept: "application/json" } }),
    fetch(`/api/coordination-items/${itemId}/audit`, { headers: { Accept: "application/json" } }),
  ]);
  if (!detailResponse.ok || !auditResponse.ok) throw new Error("协调事项详情接口失败。");
  const detail = await detailResponse.json();
  renderCoordinationDetail(detail, await auditResponse.json());
  await renderCoordinationLineage(detail);
}

async function openScenarioLineageDetail(runId) {
  navigateWorkspace("future-scenario-panel", "plan-comparison", false);
  await loadScenarioRunDetail(runId);
}

async function openMasterSettingLineageDetail(changeId) {
  navigateWorkspace("ddom-decision-panel", "change-records", false);
  await loadMasterSettingChangeDetail(changeId);
}

async function openDdomPackageLineageDetail(packageId) {
  navigateWorkspace("ddom-decision-panel", "parameter-decision", false);
  await loadDdomPackageDetail(packageId);
}

async function openCoordinationLineageDetail(itemId) {
  navigateWorkspace("coordination-panel", "action-tracking", false);
  await loadCoordinationDetail(itemId);
}

function prefillCoordinationFailure({ scenarioRunId = null, ddomPackageId = null, failureReasons = [], sourceLabel }) {
  configureCoordinationLineageSelectors();
  const reasonText = valueOr(failureReasons, []).filter(Boolean).join("；") || "需要跨部门复核阻断原因并修订方案";
  const scenarioSelect = byId("coordination-related-scenario");
  const packageSelect = byId("coordination-related-ddom-package");
  const changeSelect = byId("coordination-related-change");
  scenarioSelect.value = "";
  packageSelect.value = "";
  changeSelect.value = "";
  if (scenarioRunId && [...scenarioSelect.options].some(option => option.value === scenarioRunId)) scenarioSelect.value = scenarioRunId;
  if (ddomPackageId && [...packageSelect.options].some(option => option.value === ddomPackageId)) packageSelect.value = ddomPackageId;
  byId("coordination-title").value = `${sourceLabel}需协调修订`;
  byId("coordination-impact-objects").value = ddomPackageId ? "DDOM 变更包,场景响应" : "场景方案,受影响对象";
  byId("coordination-decision-required").value = `处理以下阻断并决定修订方案：${reasonText}`;
  navigateWorkspace("coordination-panel", "issue-list", false);
}

async function openScenarioCoordination(runId) {
  const response = await fetch(`/api/scenario-runs/${encodeURIComponent(runId)}`, { headers: { Accept: "application/json" } });
  if (!response.ok) throw new Error(`场景阻断证据读取失败：${response.status}`);
  const detail = await response.json();
  const failureReasons = valueOr(detail?.result?.feasibility?.checks, [])
    .filter(check => check.status === "Red")
    .map(check => check.message);
  prefillCoordinationFailure({ scenarioRunId: runId, failureReasons, sourceLabel: "场景阻断" });
}

async function openDdomValidationCoordination(packageId) {
  let detail = state.currentDdomPackageDetail;
  if (detail?.summary?.packageId !== packageId) {
    const response = await fetch(`/api/ddom-change-packages/${encodeURIComponent(packageId)}`, { headers: { Accept: "application/json" } });
    if (!response.ok) throw new Error(`DDOM 验证证据读取失败：${response.status}`);
    detail = await response.json();
  }
  prefillCoordinationFailure({
    scenarioRunId: detail?.summary?.sourceScenarioRunId || null,
    ddomPackageId: packageId,
    failureReasons: detail?.latestValidation?.failureReasons || [],
    sourceLabel: "DDOM 验证失败",
  });
}

async function createCoordinationItem() {
  const request = {
    title: byId("coordination-title").value,
    impactObjects: byId("coordination-impact-objects").value.split(/[,，]/).map(item => item.trim()).filter(Boolean),
    relatedScenarioRunId: byId("coordination-related-scenario").value || null,
    relatedMasterSettingChangeId: byId("coordination-related-change").value || null,
    relatedDdomPackageId: byId("coordination-related-ddom-package").value || null,
    serviceImpact: "服务影响待验证",
    inventoryImpact: "库存与保护带影响待验证",
    cashImpact: null,
    riskImpact: "协调事项风险",
    decisionRequired: byId("coordination-decision-required").value,
    owner: byId("coordination-owner").value,
    dueDate: byId("coordination-due-date").value,
    escalationLevel: byId("coordination-escalation-level").value,
    nextReviewDate: byId("coordination-next-review").value,
    createdBy: "DDS&OP 计划员",
  };
  const response = await fetch("/api/coordination-items", { method: "POST", headers: { "Content-Type": "application/json", Accept: "application/json" }, body: JSON.stringify(request) });
  const payload = await response.json();
  if (!response.ok) throw new Error(payload.message || `创建协调事项失败：${response.status}`);
  await loadCoordinationItems();
  await loadCoordinationDetail(payload.itemId);
}

async function updateCoordinationStatus(status) {
  if (!state.selectedCoordinationItemId) throw new Error("请先选择协调事项。");
  const response = await fetch(`/api/coordination-items/${state.selectedCoordinationItemId}/status`, { method: "POST", headers: { "Content-Type": "application/json", Accept: "application/json" }, body: JSON.stringify({ status, updatedBy: "DDS&OP 计划员", note: "工作台人工推进" }) });
  const payload = await response.json();
  if (!response.ok) throw new Error(payload.message || `状态推进失败：${response.status}`);
  await loadCoordinationItems();
  await loadCoordinationDetail(payload.itemId);
}

async function recordCoordinationDecision() {
  if (!state.selectedCoordinationItemId) throw new Error("请先选择协调事项。");
  const response = await fetch(`/api/coordination-items/${state.selectedCoordinationItemId}/decision`, { method: "POST", headers: { "Content-Type": "application/json", Accept: "application/json" }, body: JSON.stringify({ decision: byId("coordination-decision").value, rationale: byId("coordination-rationale").value, updatedBy: "DDS&OP 计划员" }) });
  const payload = await response.json();
  if (!response.ok) throw new Error(payload.message || `记录决策失败：${response.status}`);
  await loadCoordinationDetail(payload.itemId);
}

async function recordCoordinationOutcome() {
  if (!state.selectedCoordinationItemId) throw new Error("请先选择协调事项。");
  const response = await fetch(`/api/coordination-items/${state.selectedCoordinationItemId}/outcome`, { method: "POST", headers: { "Content-Type": "application/json", Accept: "application/json" }, body: JSON.stringify({ actualOutcome: byId("coordination-outcome").value, updatedBy: "DDS&OP 计划员" }) });
  const payload = await response.json();
  if (!response.ok) throw new Error(payload.message || `记录结果失败：${response.status}`);
  await loadCoordinationDetail(payload.itemId);
}

async function loadWorkspace() {
  byId("workspace-loading").hidden = false;
  setWorkspaceStatus("Yellow", "正在加载");

  const response = await fetch("/api/scenario-workspace-data?horizonWeeks=12", {
    headers: { Accept: "application/json" },
  });

  if (!response.ok) {
    throw new Error(`场景工作台数据接口失败：${response.status}`);
  }

  state.data = await response.json();
  configureFiveStageScenarioControls();
  const fiveStageDataPromise = Promise.allSettled([
    loadHistoryReview(),
    loadCurrentBaselineWorkspace(),
    loadCoordinationItems(true),
    loadScenarioAssumptionTemplates(),
  ]);
  const productFamilyDashboardResponse = await fetch("/api/product-family-dashboard?horizonWeeks=12", {
    headers: { Accept: "application/json" },
  });
  if (!productFamilyDashboardResponse.ok) {
    throw new Error(`产品族看板接口失败：${productFamilyDashboardResponse.status}`);
  }
  state.productFamilyDashboard = await productFamilyDashboardResponse.json();
  state.selectedProductFamily = valueOr(state.productFamilyDashboard.selectedFamily, null);
  const rccpResponse = await fetch("/api/rccp-workspace?horizonWeeks=12", {
    headers: { Accept: "application/json" },
  });
  if (!rccpResponse.ok) {
    throw new Error(`RCCP 工作台接口失败：${rccpResponse.status}`);
  }
  state.rccp = await rccpResponse.json();
  state.selectedRccpResource = valueOr(state.rccp.resourceSummaries[0]?.resourceCode, null);
  const constraintResponse = await fetch("/api/constraint-workspace?horizonWeeks=12", {
    headers: { Accept: "application/json" },
  });
  if (!constraintResponse.ok) {
    throw new Error(`受限 / 不受限工作台接口失败：${constraintResponse.status}`);
  }
  state.constraints = await constraintResponse.json();
  const supplierCollaborationResponse = await fetch("/api/supplier-collaboration-workspace?horizonWeeks=12", {
    headers: { Accept: "application/json" },
  });
  if (!supplierCollaborationResponse.ok) {
    throw new Error(`供应商需求钻取接口失败：${supplierCollaborationResponse.status}`);
  }
  state.supplierCollaboration = await supplierCollaborationResponse.json();
  state.selectedSupplier = valueOr(state.supplierCollaboration.selectedSupplier, null);
  const bufferTrendResponse = await fetch("/api/buffer-trend-workspace?horizonWeeks=12", {
    headers: { Accept: "application/json" },
  });
  if (!bufferTrendResponse.ok) {
    throw new Error(`缓冲趋势工作台接口失败：${bufferTrendResponse.status}`);
  }
  const refreshedBufferTrend = await bufferTrendResponse.json();
  const exceptionResponse = await fetch("/api/exception-workspace?horizonWeeks=12", {
    headers: { Accept: "application/json" },
  });
  if (!exceptionResponse.ok) {
    throw new Error(`异常工作台接口失败：${exceptionResponse.status}`);
  }
  state.exceptions = await exceptionResponse.json();
  state.selectedExceptionSku = valueOr(state.exceptions.exceptions[0]?.sku, null);
  const masterSettingsResponse = await fetch("/api/master-settings-workspace?limit=50", {
    headers: { Accept: "application/json" },
  });
  if (!masterSettingsResponse.ok) {
    throw new Error(`主设置治理工作台接口失败：${masterSettingsResponse.status}`);
  }
  state.masterSettings = await masterSettingsResponse.json();
  await loadPublicDemoGoldenLoop();
  await loadAdventureWorksProductDemo();
  configureFilters(state.data);
  configurePreviewControls(state.data);
  await loadSavedScenarioRuns();
  await loadDdomPackages();
  acceptCurrentBufferTrend(refreshedBufferTrend);
  applyFilters();
  fiveStageDataPromise.then(fiveStageResults => {
    const fiveStageChips = ["history-evidence-chip", "current-baseline-chip", "coordination-status-chip", "future-compare-status"];
    fiveStageResults.forEach((result, index) => {
      if (result.status === "rejected") {
        const chip = byId(fiveStageChips[index]);
        chip.className = "status-chip is-invalid";
        chip.textContent = "读取失败，可独立重试";
      }
    });
  });
}

function buildPreviewRequest() {
  const sku = previewControls.sku.value || state.data.skus[0]?.sku;
  const prebuildQuantity = Number(previewControls.prebuildQuantity.value);
  const capacityMultiplier = Number(previewControls.capacityMultiplier.value);
  const moqOverride = Number(previewControls.moqOverride.value);
  const orderCycleOverride = Number(previewControls.orderCycleOverride.value);
  const supplierLimitValue = previewControls.supplierLimit.value;
  const [supplier, materialFamily] = supplierLimitValue ? supplierLimitValue.split("|") : ["", ""];
  const prebuildWeek = Number(previewControls.prebuildWeek.value || 1);
  const capacityWeek = Number(previewControls.capacityWeek.value || 1);
  const supplierStartWeek = Math.max(1, Number(previewControls.supplierLimitStartWeek.value || 1));
  const supplierEndWeek = Math.max(supplierStartWeek, Number(previewControls.supplierLimitEndWeek.value || supplierStartWeek));

  return {
    horizonWeeks: 12,
    templateId: previewControls.template.value || null,
    skuFilter: selectors.sku.value ? [selectors.sku.value] : null,
    familyFilter: selectors.family.value ? [selectors.family.value] : null,
    adoptionConstraintMode: previewControls.adoptionConstraint.value || "Balanced",
    parameters: {
      prebuildCampaigns: prebuildQuantity > 0 && sku ? [{
        campaignId: "UI-PREBUILD",
        sku,
        buildWeek: prebuildWeek,
        protectFromWeek: prebuildWeek,
        protectThroughWeek: Math.min(prebuildWeek + 4, 12),
        quantity: prebuildQuantity,
      }] : [],
      capacityAdjustments: capacityMultiplier !== 1 && previewControls.capacityResource.value ? [{
        resourceCode: previewControls.capacityResource.value,
        week: capacityWeek,
        capacityMultiplier,
        reason: "场景运行工作台预览",
      }] : [],
      skuPolicyOverrides: sku && (moqOverride > 0 || orderCycleOverride > 0) ? [{
        sku,
        minimumOrderQuantity: moqOverride > 0 ? moqOverride : null,
        orderCycleDays: orderCycleOverride > 0 ? orderCycleOverride : null,
      }] : [],
      supplierCapacityLimits: supplierLimitValue && Number(previewControls.supplierCapacityLimit.value) > 0 ? [{
        supplier,
        materialFamily,
        startWeek: supplierStartWeek,
        endWeek: supplierEndWeek,
        committedCapacity: Number(previewControls.supplierCapacityLimit.value),
      }] : [],
    },
  };
}

async function runPreview() {
  byId("preview-status").className = "status-chip is-warning";
  byId("preview-status").textContent = "正在运行预览";

  const response = await fetch("/api/scenario-runs/preview", {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify(buildPreviewRequest()),
  });

  if (!response.ok) {
    throw new Error(`场景预览接口失败：${response.status}`);
  }

  renderPreviewResult(await response.json());
}

initializeWorkspaceUi();

document.addEventListener("click", event => {
  const focusButton = event.target.closest("[data-focus-panel]");
  if (focusButton) {
    const panel = focusButton.closest("[data-collapse-panel]");
    if (state.focusedPanel === panel) {
      closeFocusedPanel();
    } else {
      openFocusedPanel(panel);
    }
    return;
  }

  if (event.target.id === "workspace-focus-layer") {
    closeFocusedPanel();
    return;
  }

  const collapseHeading = event.target.closest("[data-collapse-toggle]");
  if (collapseHeading && !event.target.closest("button, a, input, select, textarea")) {
    toggleCollapsiblePanel(collapseHeading);
    return;
  }

  const button = event.target.closest("[data-template-id]");
  if (!button) return;
  previewControls.template.value = button.dataset.templateId;
  renderScenarioTemplates(valueOr(state.filtered, state.data));
});

document.addEventListener("click", event => {
  const rangeButton = event.target.closest("[data-history-range-months]");
  if (rangeButton) {
    loadHistoryReview(Number(rangeButton.dataset.historyRangeMonths)).catch(error => showWorkspaceError(error, "history-review"));
    return;
  }
  if (!state.historyReview) return;

  const inventoryWeek = event.target.closest("[data-history-inventory-week]");
  if (inventoryWeek) {
    state.selectedHistoryInventoryWeekOffset = Number(inventoryWeek.dataset.historyInventoryWeek);
    syncHistorySelectionState(state.historyReview);
    renderHistoryInventoryBuffer(state.historyReview);
    return;
  }

  const controlPoint = event.target.closest("[data-history-control-point]");
  const inventorySku = event.target.closest("[data-history-inventory-sku]");
  const timeBuffer = event.target.closest("[data-history-time-buffer-id]");
  const sizingSnapshot = event.target.closest("[data-history-sizing-snapshot]");
  const capacityResource = event.target.closest("[data-history-capacity-resource]");
  if (controlPoint) {
    state.selectedHistoryControlPoint = controlPoint.dataset.historyControlPoint;
    state.selectedHistoryInventorySku = null;
    state.selectedHistoryInventoryWeekOffset = null;
    state.selectedHistorySizingSnapshot = null;
  } else if (inventorySku) {
    state.selectedHistoryInventorySku = inventorySku.dataset.historyInventorySku;
    state.selectedHistoryInventoryWeekOffset = null;
    state.selectedHistorySizingSnapshot = null;
  } else if (timeBuffer) {
    state.selectedHistoryTimeBufferId = timeBuffer.dataset.historyTimeBufferId;
  } else if (sizingSnapshot) {
    state.selectedHistorySizingSnapshot = sizingSnapshot.dataset.historySizingSnapshot;
  } else if (capacityResource) {
    state.selectedHistoryCapacityResource = capacityResource.dataset.historyCapacityResource;
  } else {
    return;
  }

  syncHistorySelectionState(state.historyReview);
  renderHistoryWorkspaceOptions(state.historyReview);
  if (controlPoint || inventorySku) {
    renderHistoryBufferOverview(state.historyReview);
    renderHistoryInventoryBuffer(state.historyReview);
    renderHistoryDdmrpSizingTrace(state.historyReview);
  } else if (timeBuffer) {
    renderHistoryTimeBuffer(state.historyReview);
  } else if (sizingSnapshot) {
    renderHistoryDdmrpSizingTrace(state.historyReview);
  } else if (capacityResource) {
    renderHistoryCapacityBuffer(state.historyReview);
  }
});

document.addEventListener("keydown", event => {
  if (event.key === "Escape") {
    if (state.focusedPanel) {
      closeFocusedPanel();
      return;
    }
    closeWorkspaceDrawer();
    return;
  }

  const interactiveRow = event.target.closest(".interactive-row");
  if (interactiveRow && (event.key === "Enter" || event.key === " ")) {
    event.preventDefault();
    interactiveRow.click();
    return;
  }

  if (event.key !== "Enter" && event.key !== " ") return;
  if (event.target.closest("button, a, input, select, textarea")) return;
  const collapseHeading = event.target.closest("[data-collapse-toggle]");
  if (!collapseHeading) return;
  event.preventDefault();
  toggleCollapsiblePanel(collapseHeading);
});

document.addEventListener("click", event => {
  const row = event.target.closest("[data-ddmrp-sku]");
  if (!row) return;
  renderDdmrpParameterDetail(row.dataset.ddmrpSku);
});

document.addEventListener("click", event => {
  const row = event.target.closest("[data-guardrail-index]");
  if (!row) return;
  renderGuardrailDetail(Number(row.dataset.guardrailIndex));
});

document.addEventListener("click", event => {
  if (event.target.closest("#workspace-drawer-close")) {
    closeWorkspaceDrawer();
  }
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-rccp-resource]");
  if (!button || !state.rccp) return;
  state.selectedRccpResource = button.dataset.rccpResource;
  renderSelectedRccpResource(state.rccp);
  if (state.constraints) {
    renderSelectedConstraintResource(state.constraints);
  }
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-constraint-resource]");
  if (!button || !state.constraints) return;
  state.selectedRccpResource = button.dataset.constraintResource;
  renderSelectedConstraintResource(state.constraints);
  if (state.rccp) {
    renderSelectedRccpResource(state.rccp);
  }
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-supplier]");
  if (!button || !state.supplierCollaboration) return;
  state.selectedSupplier = button.dataset.supplier;
  renderSelectedSupplier(state.supplierCollaboration);
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-product-family]");
  if (!button || !state.productFamilyDashboard) return;
  state.selectedProductFamily = button.dataset.productFamily;
  state.selectedProductFamilyLink = null;
  renderProductFamilyDashboard(state.productFamilyDashboard);
});

document.addEventListener("click", event => {
  if (!event.target.closest("[data-product-family-reset]")) return;
  state.selectedProductFamily = state.productFamilyDashboard?.selectedFamily || null;
  state.selectedProductFamilyLink = null;
  renderProductFamilyDashboard(state.productFamilyDashboard);
});

document.addEventListener("click", event => {
  const row = event.target.closest("[data-family-link-week]");
  if (!row || !state.productFamilyDashboard) return;
  state.selectedProductFamilyLink = productFamilyLinkFromElement(row);
  renderProductFamilyDashboard(state.productFamilyDashboard);
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-buffer-sku]");
  if (!button || !state.bufferTrend) return;
  state.selectedBufferSku = button.dataset.bufferSku;
  state.futureInventorySelection.sku = button.dataset.bufferSku;
  renderBufferTrendWorkspace(state.bufferTrend);
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-buffer-family]");
  if (!button || !state.bufferTrend) return;
  const family = button.dataset.bufferFamily;
  const trend = filterBufferTrendWorkspace(state.bufferTrend);
  const firstSku = trend?.skuDetails.find(item => item.family === family)?.sku;
  if (firstSku) {
    state.selectedBufferSku = firstSku;
    state.futureInventorySelection.sku = firstSku;
    renderBufferTrendWorkspace(state.bufferTrend);
  }
});

document.addEventListener("click", event => {
  const link = event.target.closest("[data-white-box-record]");
  if (!link) return;
  event.preventDefault();
  focusWhiteBoxTraceRecord(link.dataset.whiteBoxRecord);
});

byId("buffer-case-select").addEventListener("change", event => {
  state.futureInventorySelection.caseId = event.target.value;
  state.futureInventorySelection.weekFrom = 1;
  state.futureInventorySelection.weekThrough = null;
  renderSelectedFutureInventoryWorkspace();
});

byId("buffer-week-range-select").addEventListener("change", event => {
  const [weekFrom, weekThrough] = event.target.value.split("-").map(Number);
  if (!Number.isFinite(weekFrom) || !Number.isFinite(weekThrough)) return;
  state.futureInventorySelection.weekFrom = weekFrom;
  state.futureInventorySelection.weekThrough = weekThrough;
  renderSelectedFutureInventoryWorkspace();
});

byId("time-buffer-breach-select").addEventListener("change", event => {
  state.selectedTimeBufferBreachKey = event.target.value;
  renderTimeBufferBreachEvidence(state.futureComparison, state.futureComparisonBaseline);
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-exception-sku]");
  if (!button || !state.exceptions) return;
  state.selectedExceptionSku = button.dataset.exceptionSku;
  renderExceptionWorkspace(state.exceptions);
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-scenario-run-id]");
  if (!button) return;
  openScenarioLineageDetail(button.dataset.scenarioRunId).catch(error => {
    saveControls.detailStatus.className = "status-chip is-invalid";
    saveControls.detailStatus.textContent = "审计链加载失败";
    showWorkspaceError(error);
  });
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-master-proposal-index]");
  if (!button) return;
  state.selectedMasterProposalIndex = Number(button.dataset.masterProposalIndex);
  renderMasterSettingProposalDetail(state.masterSettingProposals[state.selectedMasterProposalIndex]);
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-master-change-id]");
  if (!button) return;
  openMasterSettingLineageDetail(button.dataset.masterChangeId).catch(error => {
    masterSettingControls.status.className = "status-chip is-invalid";
    masterSettingControls.status.textContent = "变更详情加载失败";
    showWorkspaceError(error);
  });
});

document.addEventListener("click", event => {
  const row = event.target.closest("[data-baseline-snapshot-id]");
  if (!row) return;
  Promise.all([
    loadBaselineAudit(row.dataset.baselineSnapshotId),
    openBaselineSnapshotDetail(row.dataset.baselineSnapshotId),
  ]).catch(showWorkspaceError);
});

document.addEventListener("click", event => {
  const row = event.target.closest("[data-coordination-item-id]");
  if (!row) return;
  openCoordinationLineageDetail(row.dataset.coordinationItemId).catch(showWorkspaceError);
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-lineage-scenario-run-id]");
  if (!button) return;
  openScenarioLineageDetail(button.dataset.lineageScenarioRunId).catch(showWorkspaceError);
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-lineage-master-change-id]");
  if (!button) return;
  openMasterSettingLineageDetail(button.dataset.lineageMasterChangeId).catch(showWorkspaceError);
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-lineage-coordination-item-id]");
  if (!button) return;
  openCoordinationLineageDetail(button.dataset.lineageCoordinationItemId).catch(showWorkspaceError);
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-coordination-status]");
  if (!button) return;
  updateCoordinationStatus(button.dataset.coordinationStatus).catch(error => {
    byId("coordination-detail-status").className = "status-chip is-invalid";
    byId("coordination-detail-status").textContent = "状态推进失败";
    showWorkspaceError(error);
  });
});

Object.values(selectors).forEach(select => {
  select.addEventListener("change", applyFilters);
});

previewControls.sku.addEventListener("change", syncSkuPolicyDefaults);
previewControls.sku.addEventListener("change", renderScenarioScopeSummary);
previewControls.supplierLimit.addEventListener("change", syncSupplierLimitDefaults);

byId("clear-filters").addEventListener("click", () => {
  Object.values(selectors).forEach(select => { select.value = ""; });
  applyFilters();
});

byId("ddmrp-toggle-all").addEventListener("click", () => {
  state.ddmrpShowAll = !state.ddmrpShowAll;
  renderDdmrpParameterCompleteness(state.data?.ddmrpParameters || []);
});

byId("ddmrp-missing-only").addEventListener("click", () => {
  state.ddmrpMissingOnly = !state.ddmrpMissingOnly;
  state.ddmrpShowAll = state.ddmrpMissingOnly ? true : state.ddmrpShowAll;
  renderDdmrpParameterCompleteness(state.data?.ddmrpParameters || []);
});

byId("refresh-workspace").addEventListener("click", () => {
  loadWorkspace().catch(showWorkspaceError);
});

byId("refresh-current-baseline").addEventListener("click", () => loadCurrentBaselineWorkspace().catch(showWorkspaceError));
byId("freeze-current-baseline").addEventListener("click", () => freezeCurrentBaseline().catch(showWorkspaceError));
byId("run-scenario-comparison").addEventListener("click", () => runScenarioComparison().catch(error => {
  byId("future-compare-status").className = "status-chip is-invalid";
  byId("future-compare-status").textContent = "比较失败";
  showWorkspaceError(error);
}));
byId("future-assumption-mode").addEventListener("change", setAssumptionModeUi);
byId("future-assumption-template").addEventListener("change", setAssumptionModeUi);
byId("save-future-comparison").addEventListener("click", () => saveFutureComparison().catch(error => {
  byId("future-comparison-save-status").className = "status-chip is-invalid";
  byId("future-comparison-save-status").textContent = "保存失败";
  showWorkspaceError(error);
}));
byId("create-coordination-item").addEventListener("click", () => createCoordinationItem().catch(showWorkspaceError));
byId("refresh-coordination-items").addEventListener("click", () => loadCoordinationItems().catch(showWorkspaceError));
byId("record-coordination-decision").addEventListener("click", () => recordCoordinationDecision().catch(showWorkspaceError));
byId("record-coordination-outcome").addEventListener("click", () => recordCoordinationOutcome().catch(showWorkspaceError));

byId("refresh-public-demo").addEventListener("click", () => {
  loadPublicDemoGoldenLoop().catch(showWorkspaceError);
});

byId("write-public-demo-payload").addEventListener("click", () => {
  writePublicDemoPayload().catch(error => {
    byId("public-demo-handoff-chip").className = "status-chip is-invalid";
    byId("public-demo-handoff-chip").textContent = "写出失败";
    showWorkspaceError(error);
  });
});

byId("run-preview").addEventListener("click", () => {
  runPreview().catch(error => {
    byId("preview-status").className = "status-chip is-invalid";
    byId("preview-status").textContent = "预览失败";
    showWorkspaceError(error);
  });
});

byId("save-scenario").addEventListener("click", () => {
  saveScenarioRun().catch(error => {
    saveControls.status.className = "status-chip is-invalid";
    saveControls.status.textContent = "保存失败";
    showWorkspaceError(error);
  });
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-lineage-ddom-package-id]");
  if (!button) return;
  openDdomPackageLineageDetail(button.dataset.lineageDdomPackageId).catch(showWorkspaceError);
});

document.addEventListener("click", event => {
  const selectButton = event.target.closest("[data-select-ddom-run-id]");
  if (selectButton) selectScenarioForDdom(selectButton.dataset.selectDdomRunId).catch(showWorkspaceError);
  const enterButton = event.target.closest("[data-enter-ddom-run-id]");
  if (enterButton) {
    byId("ddom-source-run").value = enterButton.dataset.enterDdomRunId;
    navigateWorkspace("ddom-decision-panel", "parameter-decision", false);
  }
  const reviseButton = event.target.closest("[data-revise-blocked-run-id]");
  if (reviseButton) {
    openScenarioCoordination(reviseButton.dataset.reviseBlockedRunId).catch(showWorkspaceError);
  }
  const validationButton = event.target.closest("[data-coordinate-ddom-package-id]");
  if (validationButton) openDdomValidationCoordination(validationButton.dataset.coordinateDdomPackageId).catch(showWorkspaceError);
  const packageButton = event.target.closest("[data-ddom-package-id]");
  if (packageButton) loadDdomPackageDetail(packageButton.dataset.ddomPackageId).catch(showWorkspaceError);
});

byId("create-ddom-package").addEventListener("click", () => createDdomPackage().catch(showWorkspaceError));
byId("refresh-ddom-packages").addEventListener("click", () => loadDdomPackages().catch(showWorkspaceError));
byId("submit-ddom-package").addEventListener("click", () => ddomPackageAction("submit").catch(showWorkspaceError));
byId("validate-ddom-package").addEventListener("click", () => ddomPackageAction("validate").catch(showWorkspaceError));
byId("review-ddom-package").addEventListener("click", () => ddomPackageAction("review").catch(showWorkspaceError));
byId("approve-ddom-package").addEventListener("click", () => ddomPackageAction("approve").catch(showWorkspaceError));
byId("effective-ddom-package").addEventListener("click", () => ddomPackageAction("effective").catch(showWorkspaceError));
byId("expire-ddom-package").addEventListener("click", () => ddomPackageAction("expire").catch(showWorkspaceError));

byId("refresh-scenario-runs").addEventListener("click", () => {
  loadSavedScenarioRuns().catch(error => {
    saveControls.detailStatus.className = "status-chip is-invalid";
    saveControls.detailStatus.textContent = "记录加载失败";
    showWorkspaceError(error);
  });
});

byId("refresh-master-settings").addEventListener("click", () => {
  loadMasterSettingsWorkspace().catch(error => {
    masterSettingControls.status.className = "status-chip is-invalid";
    masterSettingControls.status.textContent = "治理记录加载失败";
    showWorkspaceError(error);
  });
});

byId("apply-exception-to-scenario").addEventListener("click", applyExceptionToScenario);

byId("navigation-toggle").addEventListener("click", () => {
  byId("scenario-workspace-app").classList.toggle("nav-collapsed");
});

loadWorkspace().catch(showWorkspaceError);
