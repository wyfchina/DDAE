# Task 8 Report: Separate Dynamic Buffer and Volatility Charts

Base commit: `2cfe670`

Planned commit message: `feat: separate dynamic buffer and volatility charts`

## Delivered

- Replaced the future inventory chart's three straight polygons with native SVG red/yellow/green stacked-area paths driven only by each backend period's `topOfRed`, `topOfYellow`, and `topOfGreen`.
- Added shape-preserving monotone cubic Hermite interpolation. Every valid backend point remains a path endpoint; smoothing changes only geometry between points.
- Added a shared per-segment crossing guard. If any adjacent buffer boundary's Bézier control hull could invert, all three zones use a linear fallback for that segment while the remaining segments stay smooth. This preserves shared boundaries and never changes DTO values.
- Split zone paths at missing evidence before numeric conversion, so `null` is never coerced to zero or bridged across.
- Retained the upper chart's net-flow line, baseline/preview/current inventory lines, target dots, replenishment markers, and dates. Removed demand bars, pulse wording, frontend-derived threshold logic, and all demand-threshold markup from the upper chart.
- Added an independent `940 x 190` lower demand-volatility SVG with its own scale. It reads only backend `demand` and `demandSpikeThreshold`, draws a smooth demand area and backend threshold line, and retains the same period dates.
- Split demand and threshold geometry independently at evidence gaps. Full or partial threshold gaps show `尖峰阈值证据缺失`; missing demand shows `计划需求证据缺失`.
- Added a visible marker for every valid demand and threshold point. Continuous geometry is drawn only for segments with at least two points, so singleton evidence remains visible without masquerading as a line or area.
- Reorganized the static legend into separate `动态红黄绿缓冲带` and `需求波动` groups without adding a chart library or dependency.

## TDD Evidence

- Initial RED: the full harness passed every existing test and failed only the new Task 8 test with `upper chart should use shape-preserving stacked area paths`.
- Runtime RED: the new Node fixture loaded the real `app.js` with a serialized 12-week `BufferTrendWorkspaceResult`; the old renderer failed because the upper chart did not contain three smooth zone paths.
- Initial GREEN: Task 8 runtime/static coverage passed. Two old assertions still required the superseded `红 / 黄 / 绿山形缓冲区` and `需求脉冲` labels; those expectations were updated to the confirmed `动态红黄绿缓冲带` and `需求波动` design. The full suite reached `143/143`.
- Singleton RED: a fixture with exactly one valid backend threshold/demand point proved the old degenerate `M`/area geometry was invisible.
- Singleton GREEN: per-point markers plus length-gated lines/areas restored visible evidence; the suite returned to `143/143`.
- Coordinate RED: direct helper tests proved `null` and blank coordinates would be coerced to zero.
- Coordinate GREEN: the helper now rejects null, undefined, blank, non-finite, duplicate, decreasing, and mismatched coordinates; the suite returned to `143/143`.
- Alignment RED: the upper SVG used a `900px` minimum width while the lower SVG inherited `760px`, so the same period x positions diverged in 760-899px containers.
- Alignment GREEN: both future SVGs now use a `900px` minimum width while historical SVGs remain at `760px`; the suite returned to `143/143`.

## Standard Runtime Fixture

`tests/AdaptiveSopDdsop.Tests/Js/future-buffer-charts.fixture.mjs` is invoked by the standard C# harness and:

1. Compiles and executes the real `app.js` in Node `vm`.
2. Renders a real backend 12-week buffer DTO into both DOM hosts.
3. Verifies upper/lower separation, retained upper operating evidence, smooth SVG paths, and absence of pulse/threshold content in the upper chart.
4. Samples cubic segments for backend-point passage, finite geometry, no overshoot, and ordered real buffer boundaries.
5. Exercises the known independent-PCHIP crossing counterexample and verifies the adaptive linear fallback stays ordered.
6. Exercises empty, singleton, two-point, duplicate/decreasing x, `NaN`/`Infinity`, null/blank, different-length, and different-x helper inputs.
7. Clones the real DTO to verify zone, demand, full/partial threshold, and singleton evidence gaps are never zero-filled or bridged.

## Verification

- `dotnet run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore`: `143 test(s) passed`.
- Codex Node `--check src\AdaptiveSopDdsop.Web\wwwroot\js\app.js`: exit `0`.
- `dotnet build AdaptiveSopDdsop.sln --no-restore -m:1`: build succeeded, `0` warnings, `0` errors.
- `scripts\verify-protected-boundaries.ps1`: all 11 whole-file boundaries and every protected CONTRACT, SDBR, Network, trace, and public-demo block passed against baseline `4e39ec5`.
- CSS delimiter check: `574/574` braces.
- `git diff --check`: exit `0`; only informational LF-to-CRLF notices.
- No `demand-pulse`, `pulseTop`, `需求脉冲`, `订单尖峰阈值`, `order-spike`, or frontend `topOfRed * 0.5` remains in the changed UI paths.
- The main-directory service on port `5074` was not started, stopped, or modified.

## Review

Independent read-only review initially found one Important responsive alignment issue (the `900px`/`760px` future SVG mismatch). After its RED/GREEN correction, the same reviewer rechecked the current worktree and reported:

- Critical: `0`
- Important: `0`
- Minor: `0`
- Specification compliance: **Approved**
- Code quality: **Approved**
