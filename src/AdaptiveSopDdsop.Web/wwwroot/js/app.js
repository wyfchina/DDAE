const state = {
  data: null,
  filtered: null,
  preview: null,
  rccp: null,
  constraints: null,
  supplierCollaboration: null,
  exceptions: null,
  bufferTrend: null,
  baselineBufferTrend: null,
  selectedBufferSku: null,
  selectedRccpResource: null,
  selectedSupplier: null,
  selectedExceptionSku: null,
  activeTab: "buffer-trend-panel",
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
  const serviceGap = targetServiceLevel() - Number(metrics.serviceLevelPercent);
  const flowGap = targetFlowIndex() - Number(metrics.flowIndex);
  const budgetOver = result.scenario.budget.reduce((sum, item) => sum + Math.max(0, Number(item.budgetInventoryVariance)), 0);
  const totalBudget = result.scenario.budget.reduce((sum, item) => sum + Number(item.budgetInventoryValue), 0);
  const budgetOverPercent = totalBudget > 0 ? budgetOver * 100 / totalBudget : 0;
  const peakLoad = Number(metrics.peakLoadPercent);
  const supplyGap = Number(metrics.supplyGap);
  const redSkuCount = Number(metrics.redSkuCount);

  const fail = (message) => ({ status: "Red", label: "阻断采纳", message });
  const warn = (message) => ({ status: "Yellow", label: "需要协调", message });
  const pass = (message) => ({ status: "Green", label: "可采纳预览", message });

  if (mode === "ServiceFirst") {
    if (serviceGap > 3 || redSkuCount > 0) return fail(`服务优先口径：服务缺口 ${number(Math.max(0, serviceGap))} 点，红区 SKU ${number(redSkuCount)}。`);
    if (serviceGap > 0) return warn(`服务优先口径：服务水平低于目标 ${number(serviceGap)} 点，需要确认客户承诺。`);
    return pass("服务优先口径：服务水平达到目标，且没有红区 SKU。");
  }

  if (mode === "FlowFirst") {
    if (flowGap > 5 || peakLoad > 120) return fail(`流速优先口径：流速缺口 ${number(Math.max(0, flowGap))} 点，峰值负荷 ${percent(peakLoad)}。`);
    if (flowGap > 0 || peakLoad > 100) return warn(`流速优先口径：流速或峰值负荷接近约束，需要重审节奏。`);
    return pass("流速优先口径：流速指数达到目标，资源未超载。");
  }

  if (mode === "CashFirst") {
    if (budgetOverPercent > 5) return fail(`现金优先口径：库存预算超出 ${percent(budgetOverPercent)}，需要财务确认。`);
    if (budgetOver > 0) return warn(`现金优先口径：库存金额超过预算 ${money(budgetOver)}，建议协调预算。`);
    return pass("现金优先口径：预览库存未超过预算。");
  }

  if (mode === "CapacityFirst") {
    if (peakLoad > 120) return fail(`产能优先口径：峰值负荷 ${percent(peakLoad)}，超过硬约束。`);
    if (peakLoad > 100) return warn(`产能优先口径：资源已经超载，需要增班、外协或需求取舍。`);
    return pass("产能优先口径：资源负荷不超过可用能力。");
  }

  if (mode === "SupplyFirst") {
    if (supplyGap > 0) return fail(`供应优先口径：存在供应缺口 ${number(supplyGap)}，需要供应协调或替代方案。`);
    return pass("供应优先口径：供应承诺能力覆盖不受限需求。");
  }

  if (serviceGap > 3 || flowGap > 5 || peakLoad > 120 || supplyGap > 0) {
    return fail(`综合平衡口径：存在服务、流速、产能或供应红线，需要升级协调。`);
  }
  if (serviceGap > 0 || flowGap > 0 || peakLoad > 100 || budgetOver > 0) {
    return warn("综合平衡口径：方案可继续评审，但需处理黄色约束。");
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

function renderReadiness(data) {
  byId("data-status-chip").className = "status-chip is-valid";
  byId("data-status-chip").textContent = "可用";
  byId("data-readiness-list").innerHTML = [
    ["产品族", data.families.length],
    ["SKU", data.skus.length],
    ["资源", data.resources.length],
    ["目标流速", percent(targetFlowIndex())],
    ["供应商来源", data.supplierItemSources.length],
    ["历史需求", data.historicalDemand.length],
    ["场景模板", data.scenarioTemplates.length],
  ].map(([label, value]) => `<div><dt>${label}</dt><dd>${number(value)}</dd></div>`).join("");

  byId("guardrail-table-body").innerHTML = data.guardrails.length
    ? data.guardrails.map(item => row([
      `<strong>${item.metric}</strong><br><small>${item.decisionRule}</small>`,
      `黄线 ${number(item.yellowLimit)} ${item.unit}`,
      `红线 ${number(item.redLimit)} ${item.unit}`,
    ])).join("")
    : emptyRow("没有业务栅栏数据", 3);
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
  byId("scenario-comparison-result").innerHTML = `
    <div class="comparison-column adoption-decision">
      <h3>采纳建议</h3>
      <p>${adoptionConstraintLabel(previewControls.adoptionConstraint.value)}：${adoption.message}</p>
      <div class="comparison-metrics">
        <div><span>采纳状态</span><strong><span class="${statusClass(adoption.status)}">${adoption.label}</span></strong></div>
        <div><span>目标流速</span><strong>${percent(targetFlowIndex())}</strong></div>
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
  renderPreviewKpis(result);
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
  renderReadiness(data);
  renderScenarioTemplates(data);
  renderScenarioComparison(data);
  byId("budget-comparison-body").innerHTML = emptyRow("运行预览后显示预算与去年同期对照", 6);
  renderBufferTrend();
  renderProductRccp(state.rccp, "基准方案");
  renderConstraintWorkspace(state.constraints);
  renderProjectedSupply();
  renderExceptionWorkspace(state.exceptions);
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
  configureFilters(state.data);
  configurePreviewControls(state.data);
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

document.querySelectorAll("[data-tab]").forEach(button => {
  button.addEventListener("click", () => activateTab(button.dataset.tab));
});

document.addEventListener("click", event => {
  const button = event.target.closest("[data-template-id]");
  if (!button) return;
  previewControls.template.value = button.dataset.templateId;
  renderScenarioTemplates(valueOr(state.filtered, state.data));
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

Object.values(selectors).forEach(select => {
  select.addEventListener("change", applyFilters);
});

previewControls.sku.addEventListener("change", syncSkuPolicyDefaults);
previewControls.supplierLimit.addEventListener("change", syncSupplierLimitDefaults);

byId("clear-filters").addEventListener("click", () => {
  Object.values(selectors).forEach(select => { select.value = ""; });
  applyFilters();
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

byId("apply-exception-to-scenario").addEventListener("click", applyExceptionToScenario);

byId("navigation-toggle").addEventListener("click", () => {
  byId("scenario-workspace-app").classList.toggle("nav-collapsed");
});

loadWorkspace().catch(showWorkspaceError);
