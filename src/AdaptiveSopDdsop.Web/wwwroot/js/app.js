const state = {
  result: window.initialScenarioResult,
  data: window.validationData,
};

const controls = {
  promotionPercent: document.querySelector("#promotionPercent"),
  supplyDisruptionWeeks: document.querySelector("#supplyDisruptionWeeks"),
  plannedShutdownDays: document.querySelector("#plannedShutdownDays"),
  newProductWeeklyDemand: document.querySelector("#newProductWeeklyDemand"),
};

const formatNumber = new Intl.NumberFormat("zh-CN", { maximumFractionDigits: 1 });
const formatMoney = new Intl.NumberFormat("zh-CN", { style: "currency", currency: "CNY", maximumFractionDigits: 0 });

function scenarioInput() {
  return {
    promotionPercent: Number(controls.promotionPercent.value),
    supplyDisruptionWeeks: Number(controls.supplyDisruptionWeeks.value),
    plannedShutdownDays: Number(controls.plannedShutdownDays.value),
    newProductWeeklyDemand: Number(controls.newProductWeeklyDemand.value),
  };
}

function money(value) {
  return formatMoney.format(Number(value ?? 0));
}

function number(value) {
  return formatNumber.format(Number(value ?? 0));
}

function percent(value) {
  return `${number(value)}%`;
}

function statusClass(status) {
  return `status status-${String(status).toLowerCase().replaceAll(" ", "-")}`;
}

function statusLabel(status) {
  return ({
    Red: "红区",
    Yellow: "黄区",
    Green: "绿区",
    OverTopOfGreen: "超绿区",
    Current: "当前",
    Proposed: "建议",
    Reviewed: "已评审",
    Approved: "已批准",
    Submitted: "已提交",
    Evaluate: "待评估",
    Candidate: "候选",
    Draft: "草案",
  })[status] ?? status;
}

function row(cells) {
  return `<tr>${cells.map(cell => `<td>${cell}</td>`).join("")}</tr>`;
}

function renderOutputs() {
  document.querySelector("#promotionPercentOut").textContent = `${controls.promotionPercent.value}%`;
  document.querySelector("#supplyDisruptionWeeksOut").textContent = `${controls.supplyDisruptionWeeks.value} 周`;
  document.querySelector("#plannedShutdownDaysOut").textContent = `${controls.plannedShutdownDays.value} 天`;
  document.querySelector("#newProductWeeklyDemandOut").textContent = number(controls.newProductWeeklyDemand.value);
}

function renderKpis(result) {
  const kpis = [
    ["Flow Index", result.flowIndex, "战略流动性指数"],
    ["Service Projection", `${result.serviceProjectionPercent}%`, "预计服务水平"],
    ["Buffer Health", `${result.bufferHealthPercent}%`, "绿区/超绿区缓冲占比"],
    ["Working Capital", money(result.totalWorkingCapital), "建议补货现金占用"],
    ["Capacity Utilization", `${result.capacityUtilizationPercent}%`, "关键能力利用率"],
  ];

  document.querySelector("#kpiGrid").innerHTML = kpis.map(([label, value, note]) => `
    <article class="kpi-card">
      <span>${label}</span>
      <strong>${value}</strong>
      <small>${note}</small>
    </article>
  `).join("");
}

function renderSkuProjection(result) {
  document.querySelector("#skuProjectionBody").innerHTML = result.skus.map(sku => row([
    `<strong>${sku.sku}</strong><br><small>${sku.name} / ${sku.family}</small>`,
    number(sku.adu),
    `<span class="${statusClass(sku.bufferStatus)}">${statusLabel(sku.bufferStatus)}</span>`,
    number(sku.netFlowPosition),
    number(sku.plannedOrder),
    money(sku.workingCapital),
  ])).join("");
}

function renderActions(result) {
  document.querySelector("#actionsList").innerHTML = result.managementActions
    .map(action => `<li>${action}</li>`)
    .join("");
}

function guardrailClass(status) {
  return ({
    Blocked: "status status-red",
    Reconcile: "status status-yellow",
    WithinFence: "status status-green",
  })[status] ?? "status";
}

function renderGuardrail(result) {
  const guardrail = result.guardrail;
  document.querySelector("#guardrailStatus").className = guardrailClass(guardrail.status);
  document.querySelector("#guardrailStatus").textContent = guardrail.statusLabel;
  document.querySelector("#guardrailDecision").textContent = guardrail.decision;
  document.querySelector("#guardrailBody").innerHTML = guardrail.checks.map(check => row([
    `<strong>${check.metric}</strong><br><small>${check.message}</small>`,
    `${number(check.value)} ${check.unit}`,
    `<span class="${statusClass(check.status)}">${statusLabel(check.status)}</span><br><small>黄线 ${number(check.yellowLimit)} / 红线 ${number(check.redLimit)}</small>`,
  ])).join("");
}

function renderProcess(data) {
  document.querySelector("#horizonBadge").textContent = `${data.strategicMonths.length} 个月滚动展望`;
  document.querySelector("#asopStepsBody").innerHTML = data.asopSteps.map(step => row([
    `<strong>${step.sequence}. ${step.name}</strong><br><small>${step.owner}</small>`,
    step.purpose,
    `<span class="soft-chip">${step.primaryOutput}</span>`,
  ])).join("");

  document.querySelector("#ddsopElementsBody").innerHTML = data.ddsopElements.map(step => row([
    `<strong>${step.sequence}. ${step.name}</strong><br><small>${step.owner}</small>`,
    step.purpose,
    `<span class="soft-chip">${step.primaryOutput}</span>`,
  ])).join("");
}

function renderPortfolio(data) {
  document.querySelector("#portfolioBody").innerHTML = [
    row(["<strong>项目</strong>", "<strong>产品族</strong>", "<strong>阶段/决策</strong>", "<strong>收入目标</strong>", "<strong>贡献毛利</strong>", "<strong>风险</strong>"]),
    ...data.portfolioItems.map(item => row([
      `<strong>${item.name}</strong><br><small>${item.code}</small>`,
      item.family,
      `${item.lifecycleStage}<br><span class="${statusClass(item.healthStatus)}">${item.decision}</span>`,
      money(item.targetRevenue),
      percent(item.contributionMarginPercent),
      item.riskNote,
    ])),
  ].join("");
}

function renderFinancial(data) {
  const selected = data.financialProjections
    .filter(item => item.monthIndex <= 36 && (item.monthIndex % 6 === 0 || item.monthIndex === 1))
    .slice(0, 28);

  document.querySelector("#financialProjectionBody").innerHTML = [
    row(["<strong>月份</strong>", "<strong>产品族</strong>", "<strong>需求</strong>", "<strong>收入</strong>", "<strong>贡献毛利</strong>", "<strong>营运资金</strong>", "<strong>ROI</strong>", "<strong>现金缺口</strong>"]),
    ...selected.map(item => row([
      item.monthLabel,
      item.family,
      number(item.demandUnits),
      money(item.revenue),
      money(item.contributionMargin),
      money(item.workingCapital),
      percent(item.roiPercent),
      item.cashGap > 0 ? `<span class="status status-yellow">${money(item.cashGap)}</span>` : "-",
    ])),
  ].join("");
}

function renderResourceGovernance(data) {
  const profiles = data.resourceProfiles.filter(item => item.monthIndex % 3 === 0 || item.status !== "Green").slice(0, 20);
  document.querySelector("#resourceProfileBody").innerHTML = profiles.map(item => row([
    `<strong>${item.resource}</strong><br><small>${item.monthLabel} / ${item.family}</small>`,
    `${number(item.requiredUnits)} / ${number(item.availableUnits)}`,
    `<span class="${statusClass(item.status)}">${percent(item.loadPercent)}</span>`,
  ])).join("");

  document.querySelector("#supplierBody").innerHTML = data.supplierConstraints.map(item => row([
    `<strong>${item.supplier}</strong><br><small>${item.materialFamily}</small>`,
    `${number(item.monthlyRequirement)} / ${number(item.monthlyCapacity)}`,
    `<span class="${statusClass(item.riskStatus)}">${item.riskStatus}</span><br><small>${item.mitigation}</small>`,
  ])).join("");

  document.querySelector("#capitalBody").innerHTML = data.capitalRequirements.map(item => row([
    `<strong>${item.name}</strong><br><small>${item.code} / ${item.triggerMonth}</small>`,
    `${money(item.investment)}<br><small>+${number(item.capacityIncrease)} 能力</small>`,
    `<span class="${statusClass(item.decisionStatus)}">${statusLabel(item.decisionStatus)}</span><br><small>ROI ${percent(item.roiPercent)}</small>`,
  ])).join("");
}

function renderSettings(data) {
  document.querySelector("#masterSettingsBody").innerHTML = data.masterSettings.slice(0, 18).map(item => row([
    `<strong>${item.target}</strong><br><small>${item.settingType}</small>`,
    `${item.currentValue}<br><span class="soft-chip">${item.proposedValue}</span>`,
    `<span class="${statusClass(item.status)}">${statusLabel(item.status)}</span><br><small>${item.trigger}</small>`,
  ])).join("");

  document.querySelector("#eventsBody").innerHTML = data.knownEvents.map(item => row([
    `<strong>${item.name}</strong><br><small>${item.window}</small>`,
    `${item.appliesTo}<br><small>DAF ${item.demandAdjustmentFactor} / Zone ${item.zoneAdjustmentFactor}</small>`,
    `<span class="${statusClass(item.status)}">${statusLabel(item.status)}</span><br><small>${item.owner}</small>`,
  ])).join("");

  document.querySelector("#skillBufferBody").innerHTML = data.skillBuffers.map(item => row([
    `<strong>${item.team}</strong><br><small>${item.criticalSkill}</small>`,
    `${item.certifiedPeople} / ${item.requiredPeople}`,
    `<span class="${statusClass(item.status)}">${item.status}</span><br><small>${item.trainingAction}</small>`,
  ])).join("");
}

function renderFeedback(data) {
  const latest = data.ddomFeedback.slice(-15);
  document.querySelector("#feedbackBody").innerHTML = latest.map(item => row([
    `<strong>${item.period}</strong><br><small>${item.target}</small>`,
    `<div class="metric-bars">
      <span style="--w:${item.reliability}%">可靠 ${percent(item.reliability)}</span>
      <span style="--w:${item.stability}%">稳定 ${percent(item.stability)}</span>
      <span style="--w:${item.velocity}%">流速 ${percent(item.velocity)}</span>
    </div>`,
    `红区 ${item.redPenetrations} / 黑区 ${item.blackPenetrations}<br><small>Act ${item.actAlerts}, Late ${item.lateAlerts}</small>`,
    `${item.rootCause}<br><small>ADU ${number(item.demonstratedAdu)}, 负载 ${percent(item.demonstratedResourceLoad)}</small>`,
  ])).join("");

  const counts = data.ddomFeedback.reduce((acc, point) => {
    acc[point.rootCause] = (acc[point.rootCause] ?? 0) + point.redPenetrations + point.blackPenetrations + point.actAlerts + point.lateAlerts;
    return acc;
  }, {});
  const max = Math.max(...Object.values(counts));
  document.querySelector("#paretoBody").innerHTML = Object.entries(counts)
    .sort((a, b) => b[1] - a[1])
    .map(([cause, count]) => `
      <div class="bar-row">
        <div><strong>${cause}</strong><small>${count} 次例外</small></div>
        <span style="--w:${(count / max) * 100}%"></span>
      </div>
    `).join("");
}

function renderDecisionLoop(data) {
  document.querySelector("#tacticalOpportunityBody").innerHTML = data.tacticalOpportunities.map(item => row([
    `<strong>${item.name}</strong><br><small>${item.trigger}</small>`,
    `${money(item.incrementalRevenue)}<br><small>贡献 ${money(item.contributionMargin)}</small>`,
    `<span class="${statusClass(item.status)}">${statusLabel(item.status)}</span><br><small>流速 +${number(item.flowDelta)}</small>`,
  ])).join("");

  document.querySelector("#strategicRecommendationBody").innerHTML = data.strategicRecommendations.map(item => row([
    `<strong>${item.name}</strong><br><small>${item.type} / ${item.decisionOwner}</small>`,
    `${money(item.investment)}<br><small>贡献变化 ${money(item.contributionMarginDelta)}</small>`,
    `<span class="${statusClass(item.status)}">${statusLabel(item.status)}</span><br><small>ROI Δ ${percent(item.roiDelta)}, Flow Δ ${number(item.flowDelta)}</small>`,
  ])).join("");

  document.querySelector("#feasibilityBody").innerHTML = data.feasibilityChecks.map(item => row([
    `<strong>${item.scenario}</strong><br><small>${item.horizon}</small>`,
    `缓冲承压 ${percent(item.ddomToleranceUsePercent)}<br><small>鼓点负载 ${percent(item.pacingResourceLoadPercent)}</small>`,
    `<span class="${item.status === "可行" ? "status status-green" : item.status === "有条件可行" ? "status status-yellow" : "status status-red"}">${item.status}</span><br><small>${item.requiredAction}</small>`,
  ])).join("");
}

function renderPlanRun(plan) {
  const bufferRows = plan.bufferProjections
    .filter(item => item.week <= 4 && item.bufferStatus !== "OverTopOfGreen")
    .slice(0, 18);
  document.querySelector("#bufferTrendBody").innerHTML = [
    row(["<strong>SKU / Week</strong>", "<strong>Start</strong>", "<strong>Demand</strong>", "<strong>Before / After</strong>", "<strong>Status</strong>"]),
    ...bufferRows.map(item => row([
      `<strong>${item.sku}</strong><br><small>W${item.week}</small>`,
      number(item.startNetFlow),
      number(item.demand),
      `${number(item.endNetFlowBeforeReplenishment)} / ${number(item.endNetFlowAfterReplenishment)}`,
      `<span class="${statusClass(item.bufferStatus)}">${statusLabel(item.bufferStatus)}</span>`,
    ])),
  ].join("");

  const rccpRows = plan.capacityLoads
    .filter(item => item.week <= 6 && item.loadPercent > 0)
    .slice(0, 18);
  document.querySelector("#rccpLoadBody").innerHTML = [
    row(["<strong>Resource / Week</strong>", "<strong>Required</strong>", "<strong>Available</strong>", "<strong>Load</strong>"]),
    ...rccpRows.map(item => row([
      `<strong>${item.resourceName}</strong><br><small>${item.resourceCode} / W${item.week}</small>`,
      number(item.requiredCapacity),
      number(item.availableCapacity),
      `<span class="${statusClass(item.status)}">${percent(item.loadPercent)}</span>`,
    ])),
  ].join("");

  const supplyRows = plan.supplyRequirements
    .filter(item => item.week <= 6)
    .slice(0, 18);
  document.querySelector("#supplyRequirementBody").innerHTML = [
    row(["<strong>Supplier / Week</strong>", "<strong>Material</strong>", "<strong>Required</strong>", "<strong>Value</strong>"]),
    ...supplyRows.map(item => row([
      `<strong>${item.supplier}</strong><br><small>W${item.week}</small>`,
      item.materialFamily,
      number(item.requiredQuantity),
      money(item.projectedValue),
    ])),
  ].join("");

  document.querySelector("#planTraceBody").innerHTML = plan.traces
    .filter(item => item.explanation.includes("below top of yellow"))
    .slice(0, 12)
    .map(item => row([
      `<strong>${item.sku}</strong><br><small>W${item.week}</small>`,
      item.explanation,
    ])).join("");
}

function renderValidationData(data) {
  renderProcess(data);
  renderPortfolio(data);
  renderFinancial(data);
  renderResourceGovernance(data);
  renderSettings(data);
  renderFeedback(data);
  renderDecisionLoop(data);

  document.querySelector("#dataBadge").textContent = `${data.families.length} 产品族 / ${data.skus.length} SKU / ${data.strategicMonths.length} 月`;
  document.querySelector("#familyBody").innerHTML = data.families.map(family => row([
    `<strong>${family.code}</strong><br>${family.name}`,
    `服务 ${family.targetServiceLevel}%`,
    `Flow ${family.targetFlowIndex}`,
  ])).join("");

  document.querySelector("#resourceBody").innerHTML = data.resources.map(resource => row([
    `<strong>${resource.code}</strong><br>${resource.name}`,
    number(resource.weeklyAvailableUnits),
    `Load ${resource.unitLoad}`,
  ])).join("");

  document.querySelector("#settingsBody").innerHTML = data.skus.map(sku => row([
    `<strong>${sku.sku}</strong><br>${sku.name}`,
    `ADU ${number(sku.adu)}`,
    `DLT ${sku.decoupledLeadTimeDays}d`,
    `MOQ ${number(sku.minimumOrderQuantity)}`,
  ])).join("");
}

function render(result) {
  state.result = result;
  renderOutputs();
  renderKpis(result);
  renderSkuProjection(result);
  renderActions(result);
  renderGuardrail(result);
}

async function refreshScenario() {
  renderOutputs();
  const response = await fetch("/api/scenario", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(scenarioInput()),
  });

  if (!response.ok) {
    throw new Error(`Scenario API failed: ${response.status}`);
  }

  render(await response.json());
}

async function loadPlanRun() {
  const response = await fetch("/api/demand-driven-plan?horizonWeeks=12");
  if (!response.ok) {
    throw new Error(`Plan run API failed: ${response.status}`);
  }

  renderPlanRun(await response.json());
}

Object.values(controls).forEach(input => input.addEventListener("input", refreshScenario));
document.querySelector("#resetBtn").addEventListener("click", () => {
  controls.promotionPercent.value = 0;
  controls.supplyDisruptionWeeks.value = 0;
  controls.plannedShutdownDays.value = 0;
  controls.newProductWeeklyDemand.value = 0;
  refreshScenario();
});

renderValidationData(state.data);
render(state.result);
loadPlanRun();
