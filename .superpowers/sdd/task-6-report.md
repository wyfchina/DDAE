# Task 6 Report — Historical inventory evidence

## Plan and outcome

1. Updated the renderer fixture and C# static/DOM checks first, then captured RED:
   the weekly evidence host was absent.
2. Added a single selectable weekly evidence-detail host and resettable weekly selection state.
3. Removed target-NFP rendering semantics from the historical inventory position chart.
4. Added backend `openSupply` and `qualifiedDemand` evidence columns; renamed ending stock to `期末在手库存`.
5. Made demand charts share a control-point axis while keeping SKU curve/title evidence distinct.
6. Extended the historical sizing summary only with backend metadata and `sizingLines` evidence; no client-side sizing formula was added.

## Delivered files

- `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`
- `tests/AdaptiveSopDdsop.Tests/Program.cs`
- `tests/AdaptiveSopDdsop.Tests/Js/history-buffer-renderers.fixture.mjs`

## Verification

- RED: bundled Node fixture failed because `history-inventory-evidence-detail` did not exist.
- GREEN: bundled Node fixture completed `9/9 renderer fixture groups passed`.
- Full verification: `dotnet run -c Release --project .\\tests\\AdaptiveSopDdsop.Tests\\AdaptiveSopDdsop.Tests.csproj --no-restore` completed with `250 test(s) passed`.
- Diff check is recorded with the final commit verification.

## Concerns

None. The fixture injects a second same-control-point SKU when the real backend DTO does not contain one, so the shared-axis/selectable-SKU contract stays exercised without changing backend data.
