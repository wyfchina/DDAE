# Task 7 Report: Historical Capacity Buffer Composite

Base commit: `6377ff4`

Commit message: `feat: render historical capacity buffer distribution`

## Delivered

- Replaced the history-only period distribution wrapper with one stable composite host containing upstream weekly utilization and empirical frequency panels.
- Added `resolveHistoryCapacityPair` for the upstream/CCR cards, backend summary KPIs, and composite renderer.
- Rendered the four fixed utilization zones, weekly bars, prescribed axis floor, average and peak markers, and a 10-point-bin historical-frequency curve.
- Kept theoretical, standard, demonstrated, planned, and committed values in the evidence table; renamed protection columns to `保护带宽`、`已用保护`、`未用保护余量`.
- Kept CCR as a utilization reference with no protection fields. The future scenario distribution renderer and shared distribution CSS remain in use.
- Added responsive two-column composite layout with a one-column layout below 980px.

## TDD Evidence

- RED: the revised Node fixture failed because `history-capacity-protection-kpis` did not exist in the old renderer.
- GREEN: the fixture asserts all four bands, weekly observations, average/peak markers, empirical curve, risk notes, retained table evidence, no old capacity-line paths, explicit missing utilization evidence, and CCR reference-only behavior.

## Verification

- `C:\Users\吴一帆\AppData\Local\OpenAI\Codex\bin\node.exe .\tests\AdaptiveSopDdsop.Tests\Js\history-buffer-renderers.fixture.mjs` — 9/9 groups passed.
- `dotnet build .\tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj -c Release --no-restore` — 0 warnings, 0 errors.
- `dotnet .\tests\AdaptiveSopDdsop.Tests\bin\Release\net9.0\AdaptiveSopDdsop.Tests.dll` — 250 tests passed.
- `git diff --check` — passed.

## Note

The normal Debug harness could not rebuild while a user-owned `AdaptiveSopDdsop.Web.exe` held the Debug apphost lock. The Release build and full Release harness completed cleanly.

## Selection Review Fix

- `resolveHistoryCapacityPair` now honors the selected history resource: an upstream selection is used directly, and a CCR selection resolves to the upstream resource that explicitly protects it. With no selected pair it still falls back to the first upstream resource.
- CCR resolution requires the named resource and the `CcrUtilization` role, so the cards and composite remain tied to one explicit relationship.
- RED: the new second-pair fixture failed because the old resolver always used the first upstream resource.
- GREEN: the fixture now proves default-first behavior, selecting a second upstream changes the cards/chart/weekly observations, selecting its paired CCR resolves back to the same upstream, and an unmatched backend summary remains explicit as missing evidence.
