const state = {
  data: null,
  filtered: null,
  preview: null,
  productFamilyDashboard: null,
  rccp: null,
  constraints: null,
  supplierCollaboration: null,
  exceptions: null,
  bufferTrend: null,
  baselineBufferTrend: null,
  optimization: null,
  masterSettings: null,
  masterSettingProposals: [],
  currentMasterSettingDetail: null,
  savedScenarioRuns: [],
  selectedBufferSku: null,
  selectedRccpResource: null,
  selectedSupplier: null,
  selectedExceptionSku: null,
  selectedScenarioRunId: null,
  selectedMasterProposalIndex: 0,
  selectedMasterChangeId: null,
  selectedProductFamily: null,
  selectedProductFamilyLink: null,
  activeTab: "buffer-trend-panel",
  focusedPanel: null,
  focusedPanelParent: null,
  focusedPanelNextSibling: null,
  focusedPanelCollapseKey: null,
  focusedPanelWasExpanded: null,
  ddmrpShowAll: false,
  ddmrpMissingOnly: false,
};

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
  overview: "查看全局 KPI 与当前运行状态，判断本次场景工作台是否可用于会议评审。",
  productFamilyDashboard: "按产品族聚合查看服务、流速、库存、RCCP、供应缺口和预算偏差，避免默认陷入 SKU 明细。",
  dataReadiness: "核对主数据覆盖、筛选对象和采纳约束，确保场景输入口径一致。",
  exceptions: "从异常 SKU 进入场景配置，优先处理服务损失、需求尖峰和缓冲风险。",
  scenarioRun: "配置模板、参数覆盖、供应限制并运行非持久化场景预览。",
  scenarioComparison: "比较基准方案与预览方案的服务、库存、产能、供应和预算影响。",
  bufferTrend: "查看 SKU 净流动量、预计库存水位、目标库存和补货触发。",
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
  { selector: "#product-family-dashboard-panel .product-family-block", defaultExpanded: false },
  { selector: "#product-family-dashboard-panel .product-family-block:first-of-type", defaultExpanded: true },
  { selector: "#product-family-dashboard-panel .product-family-detail", defaultExpanded: false },
  { selector: "#scenario-run-panel .scenario-config-panel", defaultExpanded: true },
  { selector: "#scenario-run-panel .scenario-run-layout > section:not(.scenario-config-panel)", defaultExpanded: true, title: "可选模板", kicker: "模板与动作" },
  { selector: "#scenario-run-panel #optimization-panel", defaultExpanded: false },
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
  { selector: "#saved-scenarios-panel .saved-run-list", defaultExpanded: true, title: "已保存场景列表", kicker: "场景记录" },
  { selector: "#saved-scenarios-panel .readiness-panel", defaultExpanded: false },
  { selector: "#master-settings-panel .master-settings-block", defaultExpanded: false },
  { selector: "#master-settings-panel .master-settings-block:first-child", defaultExpanded: true },
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
  title: document.querySelector("#saved-scenario-title"),
  detailStatus: document.querySelector("#saved-scenario-status"),
};

const optimizationControls = {
  solver: document.querySelector("#optimization-solver-select"),
  status: document.querySelector("#optimization-status"),
  list: document.querySelector("#optimization-recommendation-list"),
  button: document.querySelector("#run-optimization"),
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

function normalizeWorkspaceFlow() {
  const workspace = byId("workspace");
  const order = [
    "overview-panel",
    "product-family-dashboard-panel",
    "data-readiness-panel",
    "variance-panel",
    "scenario-run-panel",
    "scenario-comparison",
    "buffer-trend-panel",
    "rccp-panel",
    "projected-supply-panel",
    "saved-scenarios-panel",
    "master-settings-panel",
    "trace-panel",
  ];

  document.querySelector(".schedule-tabs")?.remove();
  order.forEach(id => {
    const section = byId(id);
    if (!section) return;
    section.classList.add("workspace-section");
    section.classList.remove("schedule-panel");
    section.removeAttribute("data-tab-panel");
    section.hidden = true;
    workspace.appendChild(section);
  });
  document.querySelector(".tab-workspace")?.remove();
}

function bindNavigationState() {
  const navItems = Array.from(document.querySelectorAll(".nav-item[href^='#']"));
  const setActiveNav = id => {
    navItems.forEach(navItem => {
      navItem.classList.toggle("is-active", navItem.getAttribute("href") === `#${id}`);
    });
  };

  navItems.forEach(item => {
    item.addEventListener("click", () => {
      const id = item.getAttribute("href")?.slice(1);
      if (id) setActiveNav(id);
    });
  });

  const sections = navItems
    .map(item => byId(item.getAttribute("href")?.slice(1)))
    .filter(Boolean);
  const observer = new IntersectionObserver(entries => {
    const visible = entries
      .filter(entry => entry.isIntersecting)
      .sort((left, right) => right.intersectionRatio - left.intersectionRatio)[0];
    if (visible?.target?.id) {
      setActiveNav(visible.target.id);
    }
  }, { root: null, rootMargin: "-18% 0px -62% 0px", threshold: [0.08, 0.18, 0.35, 0.6] });

  sections.forEach(section => observer.observe(section));
}

function initializeWorkspaceUi() {
  normalizeWorkspaceFlow();
  attachInlineHelp();
  initializeCollapsiblePanels();
  initializePanelWorkspaceActions();
  initializeResizableTables();
  bindNavigationState();
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
    healthy: "is-valid",
    warning: "is-warning",
    blocked: "is-invalid",
    blue: "is-overgreen",
  })[normalized] || "neutral"}`;
}

function statusLabel(status) {
  return ({
    Green: "绿色",
    Yellow: "黄色",
    Red: "红色",
    Blue: "超绿",
    Healthy: "健康",
    Warning: "预警",
    Blocked: "阻塞",
  })[status] || valueOr(status, "-");
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
    Engine: "引擎",
    Result: "结果",
    Persistence: "保存状态",
    Demand: "不受限需求",
    Capacity: "受限能力",
    Supply: "受限供应",
    Action: "动作建议",
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
  const metrics = result.scenario.metrics;
  const mode = previewControls.adoptionConstraint.value || "Balanced";
  const targetService = targetServiceLevel();
  const targetFlow = targetFlowIndex();
  const serviceGap = targetServiceLevel() - Number(metrics.serviceLevelPercent);
  const flowGap = targetFlowIndex() - Number(metrics.flowIndex);
  const budgetOver = result.scenario.budget.reduce((sum, item) => sum + Math.max(0, Number(item.budgetInventoryVariance)), 0);
  const totalBudget = result.scenario.budget.reduce((sum, item) => sum + Number(item.budgetInventoryValue), 0);
  const budgetOverPercent = totalBudget > 0 ? budgetOver * 100 / totalBudget : 0;
  const peakLoad = Number(metrics.peakLoadPercent);
  const supplyGap = Number(metrics.supplyGap);
  const redSkuCount = Number(metrics.redSkuCount);

  const rule = (name, current, limit, reason, action) => ({ name, current, limit, reason, action });
  const serviceRule = rule("服务红线", `${percent(metrics.serviceLevelPercent)} / 红区 SKU ${number(redSkuCount)}`, `目标 ${percent(targetService)}，服务缺口 <= 3 点且红区 SKU = 0`, "服务水平或红区 SKU 未满足采纳口径。", "先处理红区 SKU、客户承诺或需求优先级。");
  const flowRule = rule("流速红线", `${percent(metrics.flowIndex)} / 峰值负荷 ${percent(peakLoad)}`, `目标 ${percent(targetFlow)}，流速缺口 <= 5 点且峰值负荷 <= 120%`, "流速不足或峰值负荷超过流速优先硬约束。", "重排补货节奏，检查提前建库、产能调整或需求取舍。");
  const budgetRule = rule("库存预算红线", percent(budgetOverPercent), "<= 5%", "预览库存金额超过预算容忍度。", "需要财务确认预算或降低预建库存。");
  const capacityRule = rule("产能硬约束", percent(peakLoad), "<= 120%", "资源峰值负荷超过硬约束。", "需要增班、外协、调整日历或削峰。");
  const supplyRule = rule("供应硬约束", number(supplyGap), "= 0", "供应承诺能力不能覆盖不受限需求。", "需要供应商协调、替代料、提前下单或需求取舍。");

  const fail = (message, violations) => ({ status: "Red", label: "阻断采纳", message, violations });
  const warn = (message, warnings = []) => ({ status: "Yellow", label: "需要协调", message, violations: warnings });
  const pass = (message) => ({ status: "Green", label: "可采纳预览", message, violations: [] });

  if (mode === "ServiceFirst") {
    if (serviceGap > 3 || redSkuCount > 0) return fail(`服务优先口径：服务缺口 ${number(Math.max(0, serviceGap))} 点，红区 SKU ${number(redSkuCount)}。`, [serviceRule]);
    if (serviceGap > 0) return warn(`服务优先口径：服务水平低于目标 ${number(serviceGap)} 点，需要确认客户承诺。`, [serviceRule]);
    return pass("服务优先口径：服务水平达到目标，且没有红区 SKU。");
  }

  if (mode === "FlowFirst") {
    if (flowGap > 5 || peakLoad > 120) return fail(`流速优先口径：流速缺口 ${number(Math.max(0, flowGap))} 点，峰值负荷 ${percent(peakLoad)}。`, [flowRule]);
    if (flowGap > 0 || peakLoad > 100) return warn("流速优先口径：流速或峰值负荷接近约束，需要重审节奏。", [flowRule]);
    return pass("流速优先口径：流速指数达到目标，资源未超载。");
  }

  if (mode === "CashFirst") {
    if (budgetOverPercent > 5) return fail(`现金优先口径：库存预算超出 ${percent(budgetOverPercent)}，需要财务确认。`, [budgetRule]);
    if (budgetOver > 0) return warn(`现金优先口径：库存金额超过预算 ${money(budgetOver)}，建议协调预算。`, [budgetRule]);
    return pass("现金优先口径：预览库存未超过预算。");
  }

  if (mode === "CapacityFirst") {
    if (peakLoad > 120) return fail(`产能优先口径：峰值负荷 ${percent(peakLoad)}，超过硬约束。`, [capacityRule]);
    if (peakLoad > 100) return warn("产能优先口径：资源已经超载，需要增班、外协或需求取舍。", [capacityRule]);
    return pass("产能优先口径：资源负荷不超过可用能力。");
  }

  if (mode === "SupplyFirst") {
    if (supplyGap > 0) return fail(`供应优先口径：存在供应缺口 ${number(supplyGap)}，需要供应协调或替代方案。`, [supplyRule]);
    return pass("供应优先口径：供应承诺能力覆盖不受限需求。");
  }

  const redViolations = [
    serviceGap > 3 ? serviceRule : null,
    flowGap > 5 ? flowRule : null,
    peakLoad > 120 ? capacityRule : null,
    supplyGap > 0 ? supplyRule : null,
  ].filter(Boolean);
  if (serviceGap > 3 || flowGap > 5 || peakLoad > 120 || supplyGap > 0) {
    return fail("综合平衡口径：存在服务、流速、产能或供应红线，需要升级协调。", redViolations);
  }
  if (serviceGap > 0 || flowGap > 0 || peakLoad > 100 || budgetOver > 0) {
    const yellowWarnings = [
      serviceGap > 0 ? serviceRule : null,
      flowGap > 0 ? flowRule : null,
      peakLoad > 100 ? capacityRule : null,
      budgetOver > 0 ? budgetRule : null,
    ].filter(Boolean);
    return warn("综合平衡口径：方案可继续评审，但需处理黄色约束。", yellowWarnings);
  }
  return pass("综合平衡口径：核心约束均满足，可作为候选方案。");
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
  })[eventType] || valueOr(eventType, "-");
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
  return `buffer-heat-cell ${normalized === "Red" ? "is-red" : normalized === "Yellow" ? "is-yellow" : normalized === "Blue" ? "is-blue" : "is-green"}`;
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
  document.querySelectorAll(".workspace-section").forEach(section => { section.hidden = false; });
  activateTab(state.activeTab);
}

function showWorkspaceError(error) {
  byId("workspace-loading").hidden = true;
  byId("workspace-error").hidden = false;
  byId("workspace-error-message").textContent = error.message;
  setWorkspaceStatus("Red", "数据不可用");
}

function unique(values) {
  return [...new Set(values.filter(Boolean))].sort((left, right) => String(left).localeCompare(String(right), "zh-CN"));
}

function fillSelect(select, label, values) {
  select.innerHTML = [`<option value="">全部${label}</option>`, ...values.map(value => `<option value="${value}">${value}</option>`)].join("");
}

function configureFilters(data) {
  fillSelect(selectors.family, "产品族", data.families.map(item => item.code));
  fillSelect(selectors.sku, "SKU", data.skus.map(item => item.sku));
  fillSelect(selectors.resource, "资源", data.resources.map(item => item.code));
  fillSelect(selectors.risk, "风险", unique(data.supplierCapacityWindows.map(item => item.riskStatus)));
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
  const averageInventoryValue = valueOr(trend?.kpis?.averageInventoryValue, 0);
  const peakLoad = valueOr(rccp?.maxPeakLoadPercent, 0);
  const averageLoad = valueOr(rccp?.averageLoadPercent, 0);
  const supplyGap = valueOr(supplier?.totalSupplyGap, 0);

  byId("workspace-kpis").innerHTML = [
    ["服务水平", percent(service), "历史实际平均"],
    ["目标流速", percent(targetFlowIndex()), "当前产品族目标"],
    ["平均库存金额", money(averageInventoryValue), "来自缓冲趋势服务"],
    ["峰值负荷", percent(peakLoad), "来自 RCCP 服务"],
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
  const averageInventory = summaries.length ? summaries.reduce((sum, item) => sum + Number(item.averageInventoryValue), 0) / summaries.length : 0;
  const comparison = valueOr(filteredDashboard.comparison, {});
  byId("product-family-kpis").innerHTML = [
    ["红色产品族", number(redFamilies), "存在红区 SKU、供应缺口或产能超载"],
    ["黄色产品族", number(yellowFamilies), "存在黄区 SKU、预算偏差或负荷预警"],
    ["平均库存金额", money(averageInventory), `变化 ${money(valueOr(comparison.averageInventoryValueDelta, 0))}`],
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
        <span class="family-metric"><span>库存</span><b>${money(item.averageInventoryValue)}</b><i>峰值 ${money(item.peakInventoryValue)}</i></span>
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
                  ? `<td><button class="${bufferCellClass(cell.status)}" type="button" data-product-family="${escapeHtml(cell.family)}" data-product-family-week="${cell.week}"><strong>${statusLabel(cell.status)}</strong><span>库存 ${money(cell.inventoryValue)}</span><small>供 ${number(cell.supplyGap)} / 产 ${number(cell.capacityGap)}</small></button></td>`
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
    ["平均库存", money(summary.averageInventoryValue)],
    ["预算偏差", money(summary.budgetInventoryVariance)],
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
        <span>${escapeHtml(item.message)}</span>
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
        <td>黄线 ${number(item.yellowLimit)} ${escapeHtml(item.unit)}</td>
        <td>红线 ${number(item.redLimit)} ${escapeHtml(item.unit)}</td>
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
        <td><span class="${statusClass(item.completenessStatus === "Complete" ? "Green" : "Yellow")}" title="${escapeHtml(item.validationMessage)}">${item.completenessStatus === "Complete" ? "完整" : "缺失"}</span><br><small>${masterSettingStatusLabel(item.parameterStatus)}</small></td>
      </tr>
    `).join("")
    : emptyRow("没有 DDMRP 参数档案", 9);
}

function renderDdmrpParameterDetail(skuCode) {
  const item = state.data?.ddmrpParameters?.find(parameter => parameter.sku === skuCode);
  if (!item) return;
  openWorkspaceDrawer("参数详情", [
    {
      title: `${item.sku} ${item.name}`,
      items: [
        ["产品族", escapeHtml(item.family)],
        ["解耦点", escapeHtml(item.decouplingPoint)],
        ["缓冲档案", escapeHtml(item.bufferProfile)],
        ["参数状态", escapeHtml(masterSettingStatusLabel(item.parameterStatus))],
        ["完整性", escapeHtml(item.completenessStatus === "Complete" ? "完整" : "缺失")],
        ["验证信息", escapeHtml(item.validationMessage)],
      ],
    },
    {
      title: "基础参数",
      items: [
        ["ADU", number(item.adu)],
        ["ADU 来源", `${escapeHtml(item.aduSource)} / ${number(item.aduCalculationWindowDays)} 天窗口`],
        ["DLT", `${number(item.decoupledLeadTimeDays)} 天`],
        ["DLT 来源", escapeHtml(item.dltSource)],
        ["变异因子", number(item.variabilityFactor)],
        ["DAF", number(item.demandAdjustmentFactor)],
        ["区域调整因子", number(item.zoneAdjustmentFactor)],
        ["MOQ", number(item.minimumOrderQuantity)],
        ["订货周期", `${number(item.orderCycleDays)} 天`],
        ["单位成本", money(item.unitCost)],
        ["周能力参考", number(item.weeklyCapacityUnits)],
        ["生效窗口", `第 ${number(item.effectiveFromWeek)}-${number(item.effectiveThroughWeek)} 周`],
      ],
    },
    {
      title: "缓冲 sizing 结果",
      items: [
        ["红区上沿", number(item.topOfRed)],
        ["黄区上沿", number(item.topOfYellow)],
        ["绿区上沿", number(item.topOfGreen)],
        ["红区公式", "ADU × DAF × DLT × 区域调整因子 × 变异因子"],
        ["黄区公式", "红区上沿 + ADU × DAF × DLT × 区域调整因子"],
        ["绿区公式", "黄区上沿 + max(MOQ, ADU × DAF × 订货周期) × 区域调整因子"],
      ],
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
        ["黄线", `${number(item.yellowLimit)} ${escapeHtml(item.unit)}`],
        ["红线", `${number(item.redLimit)} ${escapeHtml(item.unit)}`],
        ["决策规则", escapeHtml(item.decisionRule)],
        ["当前方案", escapeHtml(guardrailTriggerStatus(item))],
      ],
    },
    {
      title: "使用说明",
      items: [
        ["作用", "用于在 Scenario Preview 后判断方案可采纳、需协调或阻断采纳。"],
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
              <small>${number(action.value)} ${action.unit}</small>
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
      ["峰值负荷", percent(baselinePeak)],
      ["红色供应窗口", supplyRisk],
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
    ["平均库存金额", money(metrics.averageInventoryValue), `Δ ${money(result.comparison.averageInventoryValueDelta)}`],
    ["峰值负荷", percent(metrics.peakLoadPercent), `Δ ${percent(result.comparison.peakLoadPercentDelta)}`],
    ["平均负荷", percent(metrics.averageLoadPercent), `Δ ${percent(result.comparison.averageLoadPercentDelta)}`],
    ["红区 SKU", number(metrics.redSkuCount), `Δ ${number(result.comparison.redSkuCountDelta)}`],
    ["供应缺口", number(metrics.supplyGap), `Δ ${number(result.comparison.supplyGapDelta)}`],
  ].map(([label, value, note]) => `<div><span>${label}</span><strong>${value}</strong><small>${note}</small></div>`).join("");
}

function renderPreviewComparison(result) {
  const adoption = evaluateAdoption(result);
  const violations = adoption.violations || [];
  byId("scenario-comparison-result").innerHTML = `
    <div class="comparison-column adoption-decision">
      <h3>采纳建议</h3>
      <p>${adoptionConstraintLabel(previewControls.adoptionConstraint.value)}：${adoption.message}</p>
      <div class="comparison-metrics">
        <div><span>采纳状态</span><strong><span class="${statusClass(adoption.status)}">${adoption.label}</span></strong></div>
        <div><span>目标流速</span><strong>${percent(targetFlowIndex())}</strong></div>
      </div>
      <div class="adoption-rule-list">
        <strong>${adoption.status === "Red" ? "违反规则" : adoption.status === "Yellow" ? "需协调规则" : "规则检查"}</strong>
        ${violations.length
          ? violations.map(item => `
            <div class="adoption-rule-item ${adoption.status === "Red" ? "is-red" : "is-yellow"}">
              <span>${item.name}</span>
              <p>当前值：${item.current}；阈值：${item.limit}</p>
              <p>原因：${item.reason}</p>
              <p>建议：${item.action}</p>
            </div>
          `).join("")
          : `<div class="adoption-rule-item is-green"><span>未违反采纳规则</span><p>核心约束均满足当前采纳口径。</p></div>`}
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
        <div><span>平均库存</span><strong>${money(metrics.averageInventoryValue)}</strong></div>
        <div><span>峰值负荷</span><strong>${percent(metrics.peakLoadPercent)}</strong></div>
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

  const comparisons = valueOr(result?.scenarioComparisons, []);
  comparisonBody.innerHTML = comparisons.length
    ? comparisons.map(item => row([
      `<strong>${escapeHtml(item.profileName)}</strong><br><small>${escapeHtml(item.profileId)}</small>`,
      `${number(item.serviceLevelDelta)}pp`,
      `${number(item.flowIndexDelta)}pp`,
      money(item.averageInventoryValueDelta),
      `${number(item.peakLoadPercentDelta)}pp`,
      number(item.redSkuCountDelta),
      number(item.supplyGapDelta),
      number(item.replenishmentOrderCountDelta),
      money(item.estimatedActionCost),
      `<span class="${item.managementDecision === "需要管理取舍" ? "status-chip is-invalid" : item.managementDecision.includes("复核") || item.managementDecision.includes("评审") ? "status-chip is-warning" : "status-chip is-valid"}">${escapeHtml(item.managementDecision)}</span>`,
    ])).join("")
    : emptyRow("生成优化推荐后显示多方案 KPI、库存、服务和订单变化。", 10);

  const matrix = valueOr(result?.candidateImpactMatrix, []);
  matrixBody.innerHTML = matrix.length
    ? matrix.map(item => row([
      `<strong>${escapeHtml(item.actionType)}</strong><br><small>${escapeHtml(item.candidateId)}</small>`,
      escapeHtml(item.target),
      `${number(item.serviceImpactPercent)}pp`,
      money(item.inventoryImpactValue),
      `${number(item.peakLoadImpactPercent)}pp`,
      number(item.supplyGapImpact),
      number(item.replenishmentOrderImpact),
      `<strong>${money(item.estimatedCost)}</strong><br><small>${escapeHtml(item.costBasis)}</small>`,
      escapeHtml(item.constraintNote),
      `<span class="${item.feasibilityStatus === "需要管理取舍" ? "status-chip is-invalid" : item.feasibilityStatus.includes("复核") ? "status-chip is-warning" : "status-chip is-valid"}">${escapeHtml(item.feasibilityStatus)}</span>`,
    ])).join("")
    : emptyRow("候选动作影响矩阵将在优化推荐生成后显示。", 10);
}

function filterBufferTrendWorkspace(trend) {
  if (!trend) return null;

  const allowedSkus = new Set(valueOr(valueOr(state.filtered?.skus, state.data?.skus), []).map(item => item.sku));
  const series = trend.series.filter(item => allowedSkus.size === 0 || allowedSkus.has(item.sku));
  const weeklyCells = trend.weeklyCells.filter(item => allowedSkus.size === 0 || allowedSkus.has(item.sku));
  const zoneBands = trend.zoneBands.filter(item => allowedSkus.size === 0 || allowedSkus.has(item.sku));
  const skuDetails = trend.skuDetails.filter(item => allowedSkus.size === 0 || allowedSkus.has(item.sku));
  const replenishmentOrderCount = skuDetails.reduce((sum, detail) => sum + detail.replenishmentOrders.length, 0);
  const familySummaries = [...new Set(weeklyCells.map(item => item.family))].map(family => {
    const cells = weeklyCells.filter(item => item.family === family);
    return {
      family,
      averageInventoryValue: cells.length ? cells.reduce((sum, item) => sum + Number(item.inventoryValue), 0) / cells.length : 0,
      redWeekCount: cells.filter(item => item.status === "Red").length,
      yellowWeekCount: cells.filter(item => item.status === "Yellow").length,
      overGreenWeekCount: cells.filter(item => item.status === "Blue").length,
      replenishmentOrderCount: skuDetails
        .filter(item => item.family === family)
        .reduce((sum, item) => sum + item.replenishmentOrders.length, 0),
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
    kpis: {
      redSkuCount: new Set(series.filter(item => item.status === "Red").map(item => item.sku)).size,
      yellowSkuCount: new Set(series.filter(item => item.status === "Yellow").map(item => item.sku)).size,
      shortageCount: series.filter(item => Number(item.endNetFlowBeforeReplenishment) <= 0).length,
      averageInventoryValue: series.length ? series.reduce((sum, item) => sum + Number(item.inventoryValue), 0) / series.length : 0,
      peakInventoryValue: series.length ? Math.max(...series.map(item => Number(item.inventoryValue))) : 0,
      replenishmentOrderCount,
      inventoryValueDelta: valueOr(trend.comparison?.averageInventoryValueDelta, 0),
    }
  };
}

function renderBufferTrendWorkspace(trend) {
  const filteredTrend = filterBufferTrendWorkspace(trend);
  if (!filteredTrend) {
    byId("buffer-trend-kpis").innerHTML = "";
    byId("buffer-trend-chart").innerHTML = `<div class="table-empty"><strong>没有缓冲趋势图数据</strong></div>`;
    byId("buffer-trend-heatmap").innerHTML = `<div class="table-empty"><strong>没有缓冲热力格数据</strong></div>`;
    byId("buffer-family-summary-body").innerHTML = emptyRow("没有产品族汇总数据", 6);
    byId("buffer-trend-body").innerHTML = emptyRow("没有缓冲趋势数据", 10);
    byId("buffer-replenishment-body").innerHTML = emptyRow("没有补货订单", 4);
    byId("buffer-sku-metadata").innerHTML = "";
    byId("buffer-trace-list").innerHTML = "";
    return;
  }

  const detail = valueOr(filteredTrend.skuDetails.find(item => item.sku === filteredTrend.selectedSku), filteredTrend.skuDetails[0]);
  byId("buffer-trend-case-chip").textContent = caseLabel(filteredTrend.name);
  byId("buffer-trend-kpis").innerHTML = [
    ["红区 SKU", number(filteredTrend.kpis.redSkuCount), "预计穿透红区"],
    ["黄区 SKU", number(filteredTrend.kpis.yellowSkuCount), "进入补货警戒"],
    ["预计短缺", number(filteredTrend.kpis.shortageCount), "净流动量小于等于 0"],
    ["平均库存金额", money(filteredTrend.kpis.averageInventoryValue), `变化 ${money(filteredTrend.kpis.inventoryValueDelta)}`],
    ["峰值库存金额", money(filteredTrend.kpis.peakInventoryValue), "计划范围内最高"],
    ["补货订单", number(filteredTrend.kpis.replenishmentOrderCount), "按订货周期复核生成"],
  ].map(([label, value, note]) => `<div><span>${label}</span><strong>${value}</strong><small>${note}</small></div>`).join("");

  renderBufferTrendChart(detail);
  renderBufferInventoryOptions(filteredTrend, detail);
  renderBufferComparison(filteredTrend);
  renderBufferHeatmap(filteredTrend);
  renderBufferFamilySummary(filteredTrend);
  renderBufferSkuDetail(detail);
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
          <button class="inventory-option ${family === selectedFamily ? "is-selected" : ""}" type="button" data-buffer-family="${family}">
            <span class="option-radio"></span><span>${family}</span>
          </button>
        `).join("")}
      </div>
    </div>
    <div class="inventory-option-block">
      <div class="inventory-option-title">库存物料</div>
      <div class="inventory-option-list is-scrollable">
        ${skus.map(item => `
          <button class="inventory-option ${item.sku === detail?.sku ? "is-selected" : ""}" type="button" data-buffer-sku="${item.sku}">
            <span class="option-radio"></span><span><strong>${item.sku}</strong><small>${item.name}</small></span>
          </button>
        `).join("")}
      </div>
    </div>`;
}

function renderBufferTrendChart(detail) {
  if (!detail || detail.series.length === 0) {
    byId("buffer-selected-title").textContent = "选中 SKU 水位趋势";
    byId("buffer-trend-chart").innerHTML = `<div class="table-empty"><strong>没有选中 SKU 趋势数据</strong></div>`;
    return;
  }

  const baselineDetail = state.baselineBufferTrend?.skuDetails?.find(item => item.sku === detail.sku);
  const showPreview = state.bufferTrend?.caseId && state.baselineBufferTrend?.caseId && state.bufferTrend.caseId !== state.baselineBufferTrend.caseId;
  byId("buffer-selected-title").textContent = `${detail.sku} 库存与净流动量趋势`;
  const width = 940;
  const height = 430;
  const left = 62;
  const right = 26;
  const top = 24;
  const mainHeight = 250;
  const pulseTop = 318;
  const pulseHeight = 72;
  const plotWidth = width - left - right;
  const chartSeries = detail.series;
  const baselineSeries = valueOr(baselineDetail?.series, []);
  const allNetFlowValues = [
    ...chartSeries.flatMap(item => [Number(item.endNetFlowBeforeReplenishment), Number(item.endNetFlowAfterReplenishment)]),
    ...baselineSeries.flatMap(item => [Number(item.endNetFlowBeforeReplenishment), Number(item.endNetFlowAfterReplenishment)]),
    ...chartSeries.flatMap(item => [Number(item.topOfRed), Number(item.topOfYellow), Number(item.topOfGreen), Number(item.targetInventory)]),
    0,
  ];
  const yMax = Math.max(...allNetFlowValues) * 1.08;
  const y = value => top + (yMax - Math.max(0, Number(value))) * mainHeight / Math.max(1, yMax);
  const x = index => left + index * plotWidth / Math.max(1, chartSeries.length - 1);
  const barWidth = Math.max(10, Math.min(24, plotWidth / Math.max(1, chartSeries.length) * 0.42));
  const redTop = chartSeries.map(item => Number(item.topOfRed));
  const yellowTop = chartSeries.map(item => Number(item.topOfYellow));
  const greenTop = chartSeries.map(item => Number(item.topOfGreen));
  const areaPoints = (lowerValues, upperValues) => [
    ...upperValues.map((value, index) => `${x(index)},${y(value)}`),
    ...lowerValues.map((value, index) => `${x(index)},${y(value)}`).reverse(),
  ].join(" ");
  const zeroLine = chartSeries.map(() => 0);
  const zoneAreas = `
    <polygon class="buffer-zone-red" points="${areaPoints(zeroLine, redTop)}"></polygon>
    <polygon class="buffer-zone-yellow" points="${areaPoints(redTop, yellowTop)}"></polygon>
    <polygon class="buffer-zone-green" points="${areaPoints(yellowTop, greenTop)}"></polygon>`;
  const linePoints = (series, valueSelector) => series.map((item, index) => `${x(index)},${y(valueSelector(item))}`).join(" ");
  const baselineLine = showPreview && baselineSeries.length
    ? `<polyline class="buffer-baseline-line" points="${linePoints(baselineSeries, item => item.endNetFlowAfterReplenishment)}"><title>基准库存水位</title></polyline>`
    : "";
  const previewLine = showPreview
    ? `<polyline class="buffer-preview-line" points="${linePoints(chartSeries, item => item.endNetFlowAfterReplenishment)}"><title>预览库存水位</title></polyline>`
    : "";
  const currentLine = showPreview
    ? ""
    : `<polyline class="buffer-inventory-line" points="${linePoints(chartSeries, item => item.endNetFlowAfterReplenishment)}"><title>预计库存水位</title></polyline>`;
  const netFlowLine = `<polyline class="buffer-net-flow-line" points="${linePoints(chartSeries, item => item.endNetFlowBeforeReplenishment)}"><title>净流动量位置</title></polyline>`;
  const targetDots = chartSeries.map((item, index) => `<circle class="target-inventory-dot" cx="${x(index)}" cy="${y(item.targetInventory)}" r="4"><title>${item.periodStartDate} 目标库存：${number(item.targetInventory)}</title></circle>`).join("");
  const demandThreshold = Number(detail.zone.topOfRed) * 0.5;
  const maxDemand = Math.max(demandThreshold, ...chartSeries.map(item => Number(item.demand)), 1);
  const pulseY = value => pulseTop + (maxDemand - Number(value)) * pulseHeight / Math.max(1, maxDemand);
  const pulseBars = chartSeries.map((item, index) => {
    const yTop = pulseY(item.demand);
    const yBottom = pulseY(0);
    return `<rect class="demand-pulse-bar" x="${x(index) - barWidth / 2}" y="${yTop}" width="${barWidth}" height="${Math.max(1, yBottom - yTop)}"><title>第 ${item.week} 周需求脉冲：${number(item.demand)}</title></rect>`;
  }).join("");
  const reviewMarkers = chartSeries
    .map((item, index) => item.isReplenishment
      ? `<line class="${item.isPrebuild ? "review-marker prebuild" : "review-marker"}" x1="${x(index)}" y1="${top}" x2="${x(index)}" y2="${top + mainHeight}"><title>${item.isPrebuild ? "提前建库订单" : "订货周期补货订单"}：${number(item.replenishmentQuantity)}</title></line>`
      : "")
    .join("");
  const timeGrid = chartSeries.map((item, index) => `<line class="time-grid-line" x1="${x(index)}" y1="${top}" x2="${x(index)}" y2="${top + mainHeight}"></line>`).join("");
  const monthLabels = chartSeries.map((item, index) => `<text class="buffer-week-label" x="${x(index)}" y="${height - 10}">${item.periodStartDate}</text>`).join("");

  byId("buffer-trend-chart").innerHTML = `
    <svg class="buffer-svg" viewBox="0 0 ${width} ${height}" role="img" aria-label="${detail.sku} 缓冲趋势图">
      <rect class="buffer-plot-bg" x="${left}" y="${top}" width="${plotWidth}" height="${mainHeight}"></rect>
      <text class="axis-title vertical" transform="translate(16 ${top + mainHeight / 2}) rotate(-90)">缓冲区 / 净流动量</text>
      <text class="axis-label" x="${left - 8}" y="${y(Math.max(...greenTop))}">${number(Math.max(...greenTop))}</text>
      <text class="axis-label" x="${left - 8}" y="${y(Math.max(...yellowTop))}">${number(Math.max(...yellowTop))}</text>
      <text class="axis-label" x="${left - 8}" y="${y(Math.max(...redTop))}">${number(Math.max(...redTop))}</text>
      <line class="axis-line" x1="${left}" y1="${y(0)}" x2="${width - right}" y2="${y(0)}"></line>
      ${timeGrid}
      ${zoneAreas}
      ${reviewMarkers}
      ${targetDots}
      ${netFlowLine}
      ${baselineLine}
      ${currentLine}
      ${previewLine}
      <rect class="buffer-pulse-bg" x="${left}" y="${pulseTop}" width="${plotWidth}" height="${pulseHeight}"></rect>
      <text class="axis-title vertical" transform="translate(16 ${pulseTop + pulseHeight / 2}) rotate(-90)">需求脉冲</text>
      ${pulseBars}
      <line class="order-spike-line" x1="${left}" y1="${pulseY(demandThreshold)}" x2="${width - right}" y2="${pulseY(demandThreshold)}"></line>
      <text class="order-spike-label" x="${width - right - 120}" y="${pulseY(demandThreshold) - 6}">订单尖峰阈值</text>
      ${monthLabels}
    </svg>
    <div class="buffer-chart-legend">
      <span><i class="zone red"></i>红区</span>
      <span><i class="zone yellow"></i>黄区</span>
      <span><i class="zone green"></i>绿区</span>
      <span><i class="line net-flow"></i>净流动量位置</span>
      ${showPreview
        ? `<span><i class="line baseline"></i>基准库存水位</span><span><i class="line preview"></i>预览库存水位</span>`
        : `<span><i class="line inventory"></i>预计库存水位</span>`}
      <span><i class="dot target"></i>目标库存</span>
      <span><i class="bar pulse"></i>需求脉冲</span>
    </div>`;
}

function renderBufferComparison(trend) {
  const comparison = valueOr(trend.comparison, {});
  const deltas = [
    Number(valueOr(comparison.averageInventoryValueDelta, 0)),
    Number(valueOr(comparison.peakInventoryValueDelta, 0)),
    Number(valueOr(comparison.redWeekDelta, 0)),
    Number(valueOr(comparison.replenishmentOrderCountDelta, 0)),
    Number(valueOr(comparison.replenishmentQuantityDelta, 0)),
  ];
  const hasPreview = state.baselineBufferTrend?.caseId && trend.caseId !== state.baselineBufferTrend.caseId;
  const note = !hasPreview
    ? "尚未运行预览，变化按 0 显示"
    : deltas.every(value => value === 0)
      ? "预览与基准一致"
      : "预览方案 - 基准方案";
  byId("buffer-comparison-strip").innerHTML = [
    ["平均库存金额变化", money(valueOr(comparison.averageInventoryValueDelta, 0))],
    ["峰值库存金额变化", money(valueOr(comparison.peakInventoryValueDelta, 0))],
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
        <thead><tr><th>SKU</th>${weeks.map(week => `<th>第 ${week} 周</th>`).join("")}</tr></thead>
        <tbody>
          ${rows.map(detail => `
            <tr>
              <th><button class="link-button" type="button" data-buffer-sku="${detail.sku}"><strong>${detail.sku}</strong><small>${detail.name}</small></button></th>
              ${weeks.map(week => {
                const cell = trend.weeklyCells.find(item => item.sku === detail.sku && item.week === week);
                return cell
                  ? `<td><button class="${bufferCellClass(cell.status)}" type="button" data-buffer-sku="${cell.sku}"><strong>${statusLabel(cell.status)}</strong><span>${money(cell.inventoryValue)}</span></button></td>`
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
      item.family,
      money(item.averageInventoryValue),
      number(item.redWeekCount),
      number(item.yellowWeekCount),
      number(item.overGreenWeekCount),
      number(item.replenishmentOrderCount),
    ])).join("")
    : emptyRow("没有产品族汇总数据", 6);
}

function renderBufferSkuDetail(detail) {
  if (!detail) {
    byId("buffer-sku-metadata").innerHTML = "";
    byId("buffer-trend-body").innerHTML = emptyRow("没有缓冲趋势数据", 10);
    byId("buffer-replenishment-body").innerHTML = emptyRow("没有补货订单", 4);
    byId("buffer-trace-list").innerHTML = "";
    byId("single-sku-activity-body").innerHTML = emptyRow("没有活动数据", 10);
    byId("single-sku-attribute-body").innerHTML = emptyRow("没有属性数据", 4);
    byId("single-sku-sizing-body").innerHTML = emptyRow("没有缓冲 sizing 数据", 4);
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
  ].map(([label, value]) => `<div><span>${label}</span><strong>${value}</strong></div>`).join("");

  renderSingleSkuSimulation(detail);

  byId("buffer-trend-body").innerHTML = detail.series.length
    ? detail.series.map(item => row([
      item.periodStartDate,
      `第 ${item.week} 周`,
      number(item.timePhasedAdu),
      number(item.startNetFlow),
      number(item.demand),
      `${number(item.endNetFlowBeforeReplenishment)} / ${number(item.endNetFlowAfterReplenishment)}`,
      `${number(item.topOfRed)} / ${number(item.topOfYellow)} / ${number(item.topOfGreen)}`,
      money(item.inventoryValue),
      item.isReplenishment ? (item.isPrebuild ? "提前建库订单" : "订货周期补货订单") : "-",
      `<span class="${statusClass(item.status)}">${statusLabel(item.status)}</span>`,
    ])).join("")
    : emptyRow("没有缓冲趋势数据", 10);

  byId("buffer-replenishment-body").innerHTML = detail.replenishmentOrders.length
    ? detail.replenishmentOrders.map(item => row([
      `第 ${item.week} 周`,
      number(item.quantity),
      money(item.value),
      triggerLabel(item.trigger),
    ])).join("")
    : emptyRow("没有补货订单", 4);

  byId("buffer-trace-list").innerHTML = detail.traces.length
    ? detail.traces.slice(0, 8).map(item => `
      <div class="diagnostic-item">
        <strong>第 ${item.week} 周</strong>
        <span>${item.explanation}</span>
      </div>
    `).join("")
    : `<div class="table-empty"><strong>没有计算追踪</strong></div>`;
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
      escapeHtml(item.value),
      escapeHtml(item.explanation),
    ])).join("")
    : emptyRow("没有属性数据", 4);

  byId("single-sku-sizing-body").innerHTML = detail.bufferSizing?.length
    ? detail.bufferSizing.map(item => row([
      escapeHtml(item.component),
      escapeHtml(item.formula),
      number(item.value),
      escapeHtml(item.explanation),
    ])).join("")
    : emptyRow("没有缓冲 sizing 数据", 4);

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
      const status = item.budgetInventoryVariance > 0 ? "Yellow" : "Green";
      return row([
        item.family,
        `第 ${item.week} 周`,
        money(item.budgetInventoryValue),
        money(item.lastYearInventoryValue),
        money(item.projectedInventoryValue),
        `<span class="${statusClass(status)}">${money(item.budgetInventoryVariance)}</span>`,
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
  const adoption = evaluateAdoption(result);
  byId("preview-status").className = statusClass(adoption.status);
  byId("preview-status").textContent = `${adoption.label}，未保存`;
  byId("route-status").className = "status-chip is-valid";
  byId("route-status").textContent = "预览结果已生成";
  showScenarioSavePanel(result);
}

function showScenarioSavePanel(result) {
  saveControls.panel.hidden = false;
  const template = state.data?.scenarioTemplates?.find(item => item.templateId === result.request.templateId);
  const templateName = valueOr(template?.name, "场景预览");
  const now = new Date().toLocaleString("zh-CN", { hour12: false });
  if (!saveControls.name.value) {
    saveControls.name.value = `${templateName} ${now}`;
  }
  saveControls.status.className = "status-chip is-paused";
  saveControls.status.textContent = "预览结果，未保存";
  byId("preview-persistence-chip").className = "status-chip is-paused";
  byId("preview-persistence-chip").textContent = "预览结果，未保存";
}

function renderSavedScenarioRuns(runs) {
  state.savedScenarioRuns = valueOr(runs, []);
  saveControls.listBody.innerHTML = state.savedScenarioRuns.length
    ? state.savedScenarioRuns.map(item => `
      <tr class="saved-run-row ${item.runId === state.selectedScenarioRunId ? "is-selected" : ""}">
        <td><button class="link-button" type="button" data-scenario-run-id="${escapeHtml(item.runId)}"><strong>${escapeHtml(item.runNumber)}</strong><small>${escapeHtml(new Date(item.createdAtUtc).toLocaleString("zh-CN", { hour12: false }))}</small></button></td>
        <td>${escapeHtml(item.name)}</td>
        <td>${escapeHtml(item.createdBy)}</td>
        <td><span class="status-chip is-valid">已保存，未提交审批</span></td>
        <td>${percent(item.serviceLevelPercent)}</td>
        <td>${percent(item.peakLoadPercent)}</td>
        <td>${number(item.supplyGap)}</td>
      </tr>
    `).join("")
    : emptyRow("暂无已保存场景。运行预览后可以保存为审计记录。", 7);
}

function renderScenarioAudit(detail, events) {
  const summary = detail?.summary;
  saveControls.title.textContent = summary ? `${summary.runNumber} 审计链` : "审计链";
  saveControls.detailStatus.className = summary ? "status-chip is-valid" : "status-chip neutral";
  saveControls.detailStatus.textContent = summary ? "已保存，未提交审批" : "未选择";
  saveControls.auditList.innerHTML = events.length
    ? events.map(item => `
      <div class="diagnostic-item">
        <strong>${item.sequence}. ${traceStageLabel(item.stage)} / ${escapeHtml(auditEventLabel(item.eventType))}</strong>
        <span>${escapeHtml(item.message)}</span>
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
  state.selectedScenarioRunId = selectedRunId || state.selectedScenarioRunId || valueOr(runs[0]?.runId, null);
  renderSavedScenarioRuns(runs);
  if (state.selectedScenarioRunId) {
    await loadScenarioRunDetail(state.selectedScenarioRunId);
  } else {
    renderScenarioAudit(null, []);
  }
}

async function loadScenarioRunDetail(runId) {
  state.selectedScenarioRunId = runId;
  renderSavedScenarioRuns(state.savedScenarioRuns);
  const [detailResponse, auditResponse] = await Promise.all([
    fetch(`/api/scenario-runs/${encodeURIComponent(runId)}`, { headers: { Accept: "application/json" } }),
    fetch(`/api/scenario-runs/${encodeURIComponent(runId)}/audit`, { headers: { Accept: "application/json" } }),
  ]);
  if (!detailResponse.ok) {
    throw new Error(`场景详情接口失败：${detailResponse.status}`);
  }
  if (!auditResponse.ok) {
    throw new Error(`场景审计链接口失败：${auditResponse.status}`);
  }
  renderScenarioAudit(await detailResponse.json(), await auditResponse.json());
}

async function saveScenarioRun() {
  if (!state.preview) {
    saveControls.status.className = "status-chip is-warning";
    saveControls.status.textContent = "请先运行预览";
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
  saveControls.status.className = "status-chip is-valid";
  saveControls.status.textContent = `已保存，未提交审批：${saved.runNumber}`;
  byId("preview-persistence-chip").className = "status-chip is-valid";
  byId("preview-persistence-chip").textContent = `已保存，未提交审批：${saved.runNumber}`;
  await loadSavedScenarioRuns(saved.runId);
}

function solverStatusClass(status) {
  return status === "Optimal" || status === "Feasible"
    ? "status-chip is-valid"
    : status === "Unavailable" || status === "Error"
      ? "status-chip is-invalid"
      : "status-chip is-warning";
}

function renderOptimizationResponse(result) {
  state.optimization = result;
  optimizationControls.status.className = solverStatusClass(result.solverStatus);
  optimizationControls.status.textContent = `${result.solverName || "Gurobi"}：${result.message || result.solverStatus}`;
  renderMultiScenarioComparison(result);

  optimizationControls.list.innerHTML = valueOr(result.recommendations, []).length
    ? result.recommendations.map((item, index) => {
      const comparison = item.previewResult.comparison;
      const optimizationComparison = item.comparison || {};
      return `
        <article class="optimization-card">
          <div class="optimization-card-heading">
            <div><span class="panel-kicker">${escapeHtml(item.profileId)}</span><h3>${escapeHtml(item.profileName)}</h3></div>
            <span class="${solverStatusClass(item.solverStatus)}">${escapeHtml(item.solverStatus)}</span>
          </div>
          <p>${escapeHtml(item.summary)}</p>
          <div class="optimization-metrics">
            <span>流速变化 <strong>${number(comparison.flowIndexDelta)}pp</strong></span>
            <span>峰值负荷变化 <strong>${number(comparison.peakLoadPercentDelta)}pp</strong></span>
            <span>供应缺口变化 <strong>${number(comparison.supplyGapDelta)}</strong></span>
            <span>动作成本 <strong>${money(valueOr(item.estimatedActionCost, 0))}</strong></span>
            <span>管理判断 <strong>${escapeHtml(valueOr(optimizationComparison.managementDecision, "待评审"))}</strong></span>
          </div>
          <div class="optimization-actions">
            ${item.actions.length
              ? item.actions.map(action => `<span>${escapeHtml(action.actionType)}：${escapeHtml(action.target)} / ${money(valueOr(action.estimatedCost, 0))}</span>`).join("")
              : `<span>未选择候选动作</span>`}
          </div>
          <button class="button secondary" type="button" data-optimization-index="${index}">带入场景</button>
        </article>
      `;
    }).join("")
    : `<div class="table-empty"><strong>没有优化推荐</strong><p>${escapeHtml(result.message || "当前求解器未返回可采纳候选。")}</p></div>`;
}

function applyOptimizationRecommendation(index) {
  const recommendation = state.optimization?.recommendations?.[index];
  if (!recommendation) {
    return;
  }

  const request = recommendation.previewRequest;
  const parameters = request.parameters || {};
  previewControls.template.value = request.templateId || previewControls.template.value;
  previewControls.adoptionConstraint.value = request.adoptionConstraintMode || previewControls.adoptionConstraint.value;

  const prebuild = valueOr(parameters.prebuildCampaigns, [])[0];
  if (prebuild) {
    previewControls.sku.value = prebuild.sku;
    previewControls.prebuildWeek.value = prebuild.buildWeek;
    previewControls.prebuildQuantity.value = prebuild.quantity;
  }

  const capacity = valueOr(parameters.capacityAdjustments, [])[0];
  if (capacity) {
    previewControls.capacityResource.value = capacity.resourceCode;
    previewControls.capacityWeek.value = capacity.week;
    previewControls.capacityMultiplier.value = capacity.capacityMultiplier;
  }

  const policy = valueOr(parameters.skuPolicyOverrides, [])[0];
  if (policy) {
    previewControls.sku.value = policy.sku;
    if (policy.minimumOrderQuantity !== null && policy.minimumOrderQuantity !== undefined) {
      previewControls.moqOverride.value = policy.minimumOrderQuantity;
    }
    if (policy.orderCycleDays !== null && policy.orderCycleDays !== undefined) {
      previewControls.orderCycleOverride.value = Math.max(1, Number(policy.orderCycleDays));
    }
  }

  const supplierLimit = valueOr(parameters.supplierCapacityLimits, [])[0];
  if (supplierLimit) {
    const value = `${supplierLimit.supplier}|${supplierLimit.materialFamily}`;
    const optionExists = Array.from(previewControls.supplierLimit.options).some(option => option.value === value);
    if (optionExists) {
      previewControls.supplierLimit.value = value;
    }
    previewControls.supplierLimitStartWeek.value = supplierLimit.startWeek;
    previewControls.supplierLimitEndWeek.value = supplierLimit.endWeek;
    previewControls.supplierCapacityLimit.value = supplierLimit.committedCapacity;
  }

  renderScenarioTemplates(valueOr(state.filtered, state.data));
  byId("preview-status").className = "status-chip is-warning";
  byId("preview-status").textContent = `${recommendation.profileName}已带入，尚未运行预览`;
}

async function runOptimization() {
  const solverName = optimizationControls.solver?.value || "Gurobi";
  optimizationControls.status.className = "status-chip is-warning";
  optimizationControls.status.textContent = `${solverName} 正在求解`;
  optimizationControls.list.innerHTML = `<div class="table-empty"><strong>正在生成优化推荐</strong></div>`;
  const payload = {
    baseRequest: state.preview?.request || buildPreviewRequest(),
    recommendationCount: 3,
    maxActionsPerRecommendation: 3,
    targetMode: null,
    solverName,
  };
  const response = await fetch("/api/scenario-runs/optimize", {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify(payload),
  });
  if (!response.ok) {
    throw new Error(`优化推荐接口失败：${response.status}`);
  }

  renderOptimizationResponse(await response.json());
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
    ["红区资源", number(rccp.redResourceCount), "峰值负荷超过 100%"],
    ["最大峰值", percent(rccp.maxPeakLoadPercent), "资源周度峰值"],
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
        <span>${item.message}</span>
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
        <span>${item.message}</span>
      </div>
    `).join("")
    : `<div class="table-empty"><strong>没有约束动作建议</strong></div>`;

  byId("constraint-trace-list").innerHTML = constraints?.trace?.length
    ? constraints.trace.map(item => `
      <div class="diagnostic-item ${item.severity === "Warning" ? "is-error" : ""}">
        <strong>${traceStageLabel(item.stage)}</strong>
        <span>${item.message}</span>
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
        <span>${item.message}</span>
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
  document.getElementById("scenario-run-panel")?.scrollIntoView({ behavior: "smooth", block: "start" });
}

function renderMasterSettings(workspace) {
  if (!workspace) {
    masterSettingControls.kpis.innerHTML = "";
    masterSettingControls.board.innerHTML = `<div class="table-empty"><strong>主设置治理数据尚未加载</strong></div>`;
    masterSettingControls.currentBody.innerHTML = emptyRow("没有当前主设置", 6);
    masterSettingControls.proposalBody.innerHTML = emptyRow("运行预览后可生成主设置变更建议", 6);
    masterSettingControls.changeBody.innerHTML = emptyRow("没有已保存变更", 6);
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
              <strong>${escapeHtml(item.target)}</strong>
              <span>${escapeHtml(masterSettingTypeLabel(item.settingType))} / ${escapeHtml(item.changeNumber)}</span>
              <small>${escapeHtml(statusLabel(item.riskLevel))} · ${escapeHtml(item.effectiveWindow)}</small>
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
      escapeHtml(item.currentValue),
      escapeHtml(item.proposedValue),
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
      escapeHtml(item.currentValue),
      escapeHtml(item.proposedValue),
      `<span class="${statusClass(item.riskLevel)}">${statusLabel(item.riskLevel)}</span>`,
      `${percent(item.serviceImpact)} / ${money(item.cashImpact)}`,
    ])).join("")
    : emptyRow("运行预览后点击“生成主设置变更建议”", 6);
}

function renderMasterSettingChanges(changes) {
  masterSettingControls.changeBody.innerHTML = changes.length
    ? changes.map(item => row([
      `<button class="link-button" type="button" data-master-change-id="${escapeHtml(item.changeId)}"><strong>${escapeHtml(item.changeNumber)}</strong><small>${escapeHtml(item.createdBy)}</small></button>`,
      escapeHtml(masterSettingTypeLabel(item.settingType)),
      escapeHtml(item.target),
      `<span class="${statusClass(item.status)}">${masterSettingStatusLabel(item.status)}</span>`,
      `<span class="${statusClass(item.riskLevel)}">${statusLabel(item.riskLevel)}</span>`,
      escapeHtml(item.effectiveWindow),
    ])).join("")
    : emptyRow("没有已保存主设置变更", 6);
}

function renderMasterSettingProposalDetail(proposal) {
  state.currentMasterSettingDetail = null;
  masterSettingControls.detailTitle.textContent = proposal ? "待保存主设置变更建议" : "主设置变更详情";
  masterSettingControls.detail.innerHTML = proposal
    ? [
      ["类型", masterSettingTypeLabel(proposal.settingType)],
      ["目标", proposal.target],
      ["当前值", proposal.currentValue],
      ["建议值", proposal.proposedValue],
      ["触发原因", proposal.trigger],
      ["生效窗口", proposal.effectiveWindow],
      ["风险", statusLabel(proposal.riskLevel)],
      ["影响", `${percent(proposal.serviceImpact)} / ${money(proposal.cashImpact)}`],
    ].map(([label, value]) => `<div><span>${label}</span><strong>${escapeHtml(value)}</strong></div>`).join("")
    : `<div class="table-empty"><strong>请选择主设置变更建议或已保存记录</strong></div>`;
  masterSettingControls.auditList.innerHTML = `<div class="table-empty"><strong>保存后生成审计链</strong></div>`;
}

function renderMasterSettingDetail(detail, auditEvents) {
  state.currentMasterSettingDetail = detail;
  const summary = detail.summary;
  const nextStatus = nextMasterSettingStatus(summary.status);
  masterSettingControls.detailTitle.textContent = `${summary.changeNumber} 主设置变更详情`;
  byId("advance-master-setting-status").disabled = !nextStatus;
  byId("advance-master-setting-status").textContent = nextStatus ? `推进为${masterSettingStatusLabel(nextStatus)}` : "已到终态";
  masterSettingControls.detail.innerHTML = [
    ["编号", summary.changeNumber],
    ["类型", masterSettingTypeLabel(summary.settingType)],
    ["目标", summary.target],
    ["当前值", summary.currentValue],
    ["建议值", summary.proposedValue],
    ["触发原因", summary.trigger],
    ["生效窗口", summary.effectiveWindow],
    ["状态", masterSettingStatusLabel(summary.status)],
    ["风险", statusLabel(summary.riskLevel)],
    ["服务 / 现金影响", `${percent(summary.serviceImpact)} / ${money(summary.cashImpact)}`],
  ].map(([label, value]) => `<div><span>${label}</span><strong>${escapeHtml(value)}</strong></div>`).join("");

  masterSettingControls.auditList.innerHTML = auditEvents.length
    ? auditEvents.map(item => `
      <div class="diagnostic-item ${item.severity === "Warning" ? "is-error" : ""}">
        <strong>${item.sequence}. ${escapeHtml(auditEventLabel(item.eventType))} / ${traceStageLabel(item.stage)}</strong>
        <span>${escapeHtml(item.message)}</span>
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

  masterSettingControls.status.className = "status-chip is-warning";
  masterSettingControls.status.textContent = "正在生成建议";
  const response = await fetch("/api/master-settings/proposals/from-preview", {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify(state.preview.request),
  });
  if (!response.ok) {
    throw new Error(`主设置建议接口失败：${response.status}`);
  }

  const result = await response.json();
  state.masterSettingProposals = result.proposals || [];
  state.selectedMasterProposalIndex = 0;
  masterSettingControls.status.className = state.masterSettingProposals.length ? "status-chip is-valid" : "status-chip is-warning";
  masterSettingControls.status.textContent = state.masterSettingProposals.length ? "已生成主设置建议" : "没有可生成的建议";
  renderMasterSettings(state.masterSettings);
  renderMasterSettingProposalDetail(state.masterSettingProposals[0]);
  byId("master-settings-panel")?.scrollIntoView({ behavior: "smooth", block: "start" });
}

async function saveSelectedMasterSettingChange() {
  const proposal = state.masterSettingProposals[state.selectedMasterProposalIndex];
  if (!proposal) {
    masterSettingControls.status.className = "status-chip is-warning";
    masterSettingControls.status.textContent = "请选择待保存建议";
    return;
  }

  masterSettingControls.status.className = "status-chip is-warning";
  masterSettingControls.status.textContent = "正在保存变更";
  const response = await fetch("/api/master-settings/changes", {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify({ createdBy: "计划员", change: proposal }),
  });
  if (!response.ok) {
    throw new Error(`保存主设置变更接口失败：${response.status}`);
  }

  const saved = await response.json();
  state.selectedMasterChangeId = saved.changeId;
  masterSettingControls.status.className = "status-chip is-valid";
  masterSettingControls.status.textContent = `已保存：${saved.changeNumber}`;
  await loadMasterSettingsWorkspace();
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
  renderMasterSettingDetail(await detailResponse.json(), await auditResponse.json());
}

async function advanceMasterSettingStatus() {
  const detail = state.currentMasterSettingDetail;
  const nextStatus = nextMasterSettingStatus(detail?.summary?.status);
  if (!detail || !nextStatus) {
    return;
  }

  const response = await fetch(`/api/master-settings/changes/${encodeURIComponent(detail.summary.changeId)}/status`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify({ status: nextStatus, updatedBy: "计划员", note: "工作台状态流转" }),
  });
  if (!response.ok) {
    throw new Error(`主设置状态流转接口失败：${response.status}`);
  }
  await loadMasterSettingsWorkspace();
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

function renderWorkspace() {
  const data = state.filtered;

  renderKpis(data);
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

  byId("snapshot-freshness").textContent = `${data.request.anchorDate} / ${data.request.horizonWeeks} 周`;
  setWorkspaceStatus("Green", "工作台已就绪");
  showWorkspaceContent();
}

function activateTab(tabId) {
  state.activeTab = tabId;
  document.querySelectorAll("[data-tab]").forEach(button => {
    const active = button.dataset.tab === tabId;
    button.classList.toggle("is-active", active);
    button.setAttribute("aria-selected", String(active));
  });
  document.querySelectorAll("[data-tab-panel]").forEach(panel => {
    panel.hidden = panel.id !== tabId;
  });
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
  state.bufferTrend = await bufferTrendResponse.json();
  state.baselineBufferTrend = state.bufferTrend;
  state.selectedBufferSku = valueOr(state.bufferTrend.selectedSku, null);
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
  configureFilters(state.data);
  configurePreviewControls(state.data);
  await loadSavedScenarioRuns();
  applyFilters();
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

document.querySelectorAll("[data-tab]").forEach(button => {
  button.addEventListener("click", () => activateTab(button.dataset.tab));
});

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
    renderBufferTrendWorkspace(state.bufferTrend);
  }
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
  loadScenarioRunDetail(button.dataset.scenarioRunId).catch(error => {
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
  loadMasterSettingChangeDetail(button.dataset.masterChangeId).catch(error => {
    masterSettingControls.status.className = "status-chip is-invalid";
    masterSettingControls.status.textContent = "变更详情加载失败";
    showWorkspaceError(error);
  });
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-optimization-index]");
  if (!button) return;
  applyOptimizationRecommendation(Number(button.dataset.optimizationIndex));
});

Object.values(selectors).forEach(select => {
  select.addEventListener("change", applyFilters);
});

previewControls.sku.addEventListener("change", syncSkuPolicyDefaults);
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

byId("run-preview").addEventListener("click", () => {
  runPreview().catch(error => {
    byId("preview-status").className = "status-chip is-invalid";
    byId("preview-status").textContent = "预览失败";
    showWorkspaceError(error);
  });
});

byId("run-optimization").addEventListener("click", () => {
  runOptimization().catch(error => {
    optimizationControls.status.className = "status-chip is-invalid";
    optimizationControls.status.textContent = "优化推荐失败";
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

byId("refresh-scenario-runs").addEventListener("click", () => {
  loadSavedScenarioRuns().catch(error => {
    saveControls.detailStatus.className = "status-chip is-invalid";
    saveControls.detailStatus.textContent = "记录加载失败";
    showWorkspaceError(error);
  });
});

byId("generate-master-settings").addEventListener("click", () => {
  generateMasterSettingProposals().catch(error => {
    masterSettingControls.status.className = "status-chip is-invalid";
    masterSettingControls.status.textContent = "生成建议失败";
    showWorkspaceError(error);
  });
});

byId("save-master-setting-change").addEventListener("click", () => {
  saveSelectedMasterSettingChange().catch(error => {
    masterSettingControls.status.className = "status-chip is-invalid";
    masterSettingControls.status.textContent = "保存变更失败";
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

byId("advance-master-setting-status").addEventListener("click", () => {
  advanceMasterSettingStatus().catch(error => {
    masterSettingControls.status.className = "status-chip is-invalid";
    masterSettingControls.status.textContent = "状态流转失败";
    showWorkspaceError(error);
  });
});

byId("apply-exception-to-scenario").addEventListener("click", applyExceptionToScenario);

byId("navigation-toggle").addEventListener("click", () => {
  byId("scenario-workspace-app").classList.toggle("nav-collapsed");
});

loadWorkspace().catch(showWorkspaceError);
