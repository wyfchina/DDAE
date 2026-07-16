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

function extractFunctionSource(source, functionName, required = true) {
  const signatures = [`async function ${functionName}`, `function ${functionName}`];
  const start = signatures
    .map(signature => source.indexOf(signature))
    .filter(index => index >= 0)
    .sort((left, right) => left - right)[0];

  if (start === undefined) {
    if (!required) return null;
    throw new Error(`Unable to find ${functionName} in app.js`);
  }

  const bodyStart = source.indexOf("{", start);
  let depth = 0;
  for (let index = bodyStart; index < source.length; index += 1) {
    if (source[index] === "{") depth += 1;
    if (source[index] === "}") depth -= 1;
    if (depth === 0) return source.slice(start, index + 1);
  }

  throw new Error(`Unable to find the end of ${functionName} in app.js`);
}

function deferred() {
  let resolve;
  let reject;
  const promise = new Promise((resolvePromise, rejectPromise) => {
    resolve = resolvePromise;
    reject = rejectPromise;
  });
  return { promise, reject, resolve };
}

function jsonResponse(payload, status = 200) {
  return {
    ok: status >= 200 && status < 300,
    status,
    json: async () => payload,
  };
}

function createDom() {
  const elements = new Map([
    ["workspace-loading", { hidden: false }],
    ["workspace-error", { hidden: true }],
    ["workspace-error-message", { textContent: "" }],
  ]);
  return {
    byId: id => {
      if (!elements.has(id)) elements.set(id, {});
      return elements.get(id);
    },
    elements,
  };
}

function createRuntime(source, fetchImplementation, { includeOwnedErrorHelpers = false } = {}) {
  const rendered = [];
  const cleared = [];
  const statusChanges = [];
  const buttons = [6, 12].map(months => ({
    dataset: { historyRangeMonths: String(months) },
    classList: { toggle() {} },
    setAttribute() {},
  }));
  const dom = createDom();
  const context = vm.createContext({
    console,
    fetch: fetchImplementation,
    state: {
      historyRequestGeneration: 0,
      historyTrendMonths: 6,
      workspaceErrorSource: null,
    },
    document: { querySelectorAll: () => buttons },
    renderHistoryReview: history => rendered.push(history.id),
    clearWorkspaceError: sourceName => cleared.push(sourceName),
    byId: dom.byId,
    setWorkspaceStatus: (status, message) => statusChanges.push({ message, status }),
  });

  const generationHelper = extractFunctionSource(source, "isStaleHistoryRequest", false);
  const loadHistoryReview = extractFunctionSource(source, "loadHistoryReview");
  const showWorkspaceError = includeOwnedErrorHelpers
    ? extractFunctionSource(source, "showWorkspaceError")
    : null;
  const clearWorkspaceError = includeOwnedErrorHelpers
    ? extractFunctionSource(source, "clearWorkspaceError", false)
    : null;

  if (includeOwnedErrorHelpers && clearWorkspaceError) {
    delete context.clearWorkspaceError;
  }

  vm.runInContext(
    [
      generationHelper,
      showWorkspaceError,
      clearWorkspaceError,
      loadHistoryReview,
      "globalThis.__loadHistoryReview = loadHistoryReview;",
      showWorkspaceError ? "globalThis.__showWorkspaceError = showWorkspaceError;" : null,
    ].filter(Boolean).join("\n"),
    context,
  );

  return {
    clearHelperPresent: Boolean(clearWorkspaceError),
    cleared,
    context,
    dom,
    load: context.__loadHistoryReview,
    rendered,
    showError: context.__showWorkspaceError,
    statusChanges,
  };
}

async function oldFetchRejectionIsIgnored(source) {
  const oldFetch = deferred();
  let fetchCount = 0;
  const runtime = createRuntime(source, () => {
    fetchCount += 1;
    return fetchCount === 1
      ? oldFetch.promise
      : Promise.resolve(jsonResponse({ id: "newer-12-month" }));
  });

  const oldLoad = runtime.load(6);
  await runtime.load(12);
  oldFetch.reject(new Error("obsolete fetch failed"));
  await oldLoad;

  assert.deepEqual(runtime.rendered, ["newer-12-month"]);
  assert.equal(runtime.context.state.historyTrendMonths, 12);
}

async function oldJsonRejectionIsIgnored(source) {
  const oldJson = deferred();
  const jsonStarted = deferred();
  let fetchCount = 0;
  const runtime = createRuntime(source, () => {
    fetchCount += 1;
    if (fetchCount === 1) {
      return Promise.resolve({
        ok: true,
        status: 200,
        json: () => {
          jsonStarted.resolve();
          return oldJson.promise;
        },
      });
    }
    return Promise.resolve(jsonResponse({ id: "newer-after-json" }));
  });

  const oldLoad = runtime.load(6);
  await jsonStarted.promise;
  await runtime.load(12);
  oldJson.reject(new Error("obsolete json failed"));
  await oldLoad;

  assert.deepEqual(runtime.rendered, ["newer-after-json"]);
  assert.equal(runtime.context.state.historyTrendMonths, 12);
}

async function latestFailurePropagatesAndDisplays(source) {
  const runtime = createRuntime(
    source,
    () => Promise.reject(new Error("latest history failed")),
    { includeOwnedErrorHelpers: true },
  );
  let propagated = false;

  try {
    await runtime.load(12);
  } catch (error) {
    propagated = true;
    runtime.showError(error, "history-review");
  }

  assert.equal(propagated, true, "the current generation must still reject");
  assert.equal(runtime.dom.elements.get("workspace-error").hidden, false);
  assert.equal(runtime.dom.elements.get("workspace-error-message").textContent, "latest history failed");
  assert.equal(runtime.context.state.workspaceErrorSource, "history-review");
  assert.deepEqual(runtime.statusChanges.at(-1), { message: "数据不可用", status: "Red" });
}

async function latestSuccessClearsOnlyItsOwnError(source) {
  const historyOwned = createRuntime(
    source,
    () => Promise.resolve(jsonResponse({ id: "history-recovered" })),
    { includeOwnedErrorHelpers: true },
  );
  assert.equal(historyOwned.clearHelperPresent, true, "clearWorkspaceError must exist");
  historyOwned.showError(new Error("old history error"), "history-review");
  await historyOwned.load(12);

  assert.deepEqual(historyOwned.rendered, ["history-recovered"]);
  assert.equal(historyOwned.dom.elements.get("workspace-error").hidden, true);
  assert.equal(historyOwned.dom.elements.get("workspace-error-message").textContent, "");
  assert.equal(historyOwned.context.state.workspaceErrorSource, null);

  const otherOwned = createRuntime(
    source,
    () => Promise.resolve(jsonResponse({ id: "history-loaded-beside-other-error" })),
    { includeOwnedErrorHelpers: true },
  );
  otherOwned.showError(new Error("scenario workspace failed"), "scenario-workspace");
  await otherOwned.load(6);

  assert.deepEqual(otherOwned.rendered, ["history-loaded-beside-other-error"]);
  assert.equal(otherOwned.dom.elements.get("workspace-error").hidden, false);
  assert.equal(otherOwned.dom.elements.get("workspace-error-message").textContent, "scenario workspace failed");
  assert.equal(otherOwned.context.state.workspaceErrorSource, "scenario-workspace");
  assert.deepEqual(otherOwned.statusChanges.at(-1), { message: "数据不可用", status: "Red" });
}

export async function runHistoryReviewRaceFixtures(scriptPath = defaultScriptPath) {
  const source = await readFile(scriptPath, "utf8");
  const fixtures = [
    ["old fetch rejection is stale/no-op", oldFetchRejectionIsIgnored],
    ["old JSON rejection is stale/no-op", oldJsonRejectionIsIgnored],
    ["latest failure propagates and displays", latestFailurePropagatesAndDisplays],
    ["latest success clears only its own error", latestSuccessClearsOnlyItsOwnError],
  ];
  const failures = [];

  for (const [name, fixture] of fixtures) {
    try {
      await fixture(source);
      console.log(`PASS ${name}`);
    } catch (error) {
      failures.push({ error, name });
      console.error(`FAIL ${name}: ${error.message}`);
    }
  }

  if (failures.length > 0) {
    throw new AggregateError(
      failures.map(failure => failure.error),
      `${failures.length} history race fixture(s) failed`,
    );
  }
}
