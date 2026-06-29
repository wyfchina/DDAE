# DDS&OP Main Minimal Merge Package

Date: 2026-06-29

Purpose: identify the smallest safe package to promote from `codex/网络结构评分-feature` into `main` without merging the independent Network Structure Scoring product into the DDS&OP mainline.

## Decision Summary

`main` should become the stable DDS&OP product line. It should receive DDS&OP-owned scenario governance, SDBR contract consumers, removal of the old solver recommendation path, and a minimal entry point to the independent network scoring product.

`main` should not directly absorb the full network scoring product UI, host, graph, metrics, scoring engine, or standalone seed network data. Those remain in `codex/network-structure-product-line`.

## Package A: Must Merge Into Main

These files represent DDS&OP-owned capabilities and should be promoted to `main`.

### SDBR Contract Consumers And Ledgers

- `src/AdaptiveSopDdsop.Web/Domain/DdsopConfigInboundContract.cs`
- `src/AdaptiveSopDdsop.Web/Domain/ProductionSupplierIdentitySourceContract.cs`
- `src/AdaptiveSopDdsop.Web/Domain/ProductionInventoryQualityEvidenceContract.cs`
- `src/AdaptiveSopDdsop.Web/Domain/SdbrExecutionObjectEvidenceContract.cs`
- `tests/AdaptiveSopDdsop.Tests/Fixtures/sdbr-actual-planning-run-feedback.json`
- `tests/AdaptiveSopDdsop.Tests/Fixtures/sdbr-actual-variance-analysis-feedback.json`

Required `Program.cs` changes:

- register `DdsopConfigInboundContractService`
- register `DdsopFeedbackInboundLedger`
- register `ProductionSupplierIdentitySourceInboundLedger`
- register `ProductionInventoryQualityInboundLedger`
- register `SdbrExecutionObjectEvidenceInboundLedger`
- map `/api/integration-contracts/ddsop-config-inbound-v1`
- map `/api/integration-contracts/ddsop-feedback-outbound-v1`
- map `/api/integration-contracts/ddsop-feedback-outbound-v1/ledger`
- map `/api/integration-contracts/production-supplier-identity-source-v1`
- map `/api/integration-contracts/production-supplier-identity-source-v1/ledger`
- map `/api/integration-contracts/production-inventory-quality-evidence-v1`
- map `/api/integration-contracts/production-inventory-quality-evidence-v1/ledger`
- map `/api/integration-contracts/sdbr-execution-object-evidence-v1`
- map `/api/integration-contracts/sdbr-execution-object-evidence-v1/ledger`

Reason: this is DDS&OP to SDBR integration infrastructure, not network scoring product code.

### Remove Old DDS&OP Solver Recommendation Path

- delete `src/AdaptiveSopDdsop.Web/Domain/ScenarioOptimizationService.cs`
- remove `GurobiOptimizationSolver` and `OrToolsOptimizationSolver` from `src/AdaptiveSopDdsop.Web/Domain`
- remove `Gurobi.Optimizer` package reference from `src/AdaptiveSopDdsop.Web/AdaptiveSopDdsop.Web.csproj`
- remove `IOptimizationSolver`, `ScenarioOptimizationRequest`, `ScenarioOptimizationResponse`, and related old scenario optimization DTOs from `src/AdaptiveSopDdsop.Web/Domain/ScenarioWorkspaceData.cs`
- remove `/api/scenario-runs/optimize` from `Program.cs`
- remove optimization UI from `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
- remove optimization JavaScript from `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
- remove optimization CSS from `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`

Reason: solver must not be exposed as direct DDS&OP plan recommendation. Future Gurobi / OR-Tools use is only as candidate-action combination selection before DDS&OP white-box recalculation.

### Minimal Network Scoring Entry Point In DDS&OP

- `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml`
  - add overview card linking to the independent network scoring product
  - explain that network scoring returns candidates and evidence
  - state that DDS&OP must white-box recalculate before adoption
- `src/AdaptiveSopDdsop.Web/Pages/Index.cshtml.cs`
  - add `NetworkStructureProductUrl`
  - load `NetworkStructure:ProductUrl` from configuration
- `src/AdaptiveSopDdsop.Web/appsettings.json`
  - add `NetworkStructure:ProductUrl`

Reason: DDS&OP needs a managed entry point to the independent product, but not the product body itself.

### DDS&OP UI And Test Updates Required By The Above

- `src/AdaptiveSopDdsop.Web/wwwroot/js/app.js`
  - keep multi-scenario comparison display labels if they do not call solver APIs
  - ensure no call remains to `/api/scenario-runs/optimize`
  - ensure no `applyOptimizationRecommendation` behavior remains
- `src/AdaptiveSopDdsop.Web/wwwroot/css/site.css`
  - keep network entry card styles
  - remove old optimization card styles
- `tests/AdaptiveSopDdsop.Tests/Program.cs`
  - update assertions so DDS&OP does not expose old solver recommendation UI
  - keep SDBR contract consumer tests
  - keep scenario, persistence, main DDS&OP tests

## Package B: Do Not Merge Into Main

These belong to the independent network scoring product line.

- `src/AdaptiveSopDdsop.NetworkStructure`
- `src/AdaptiveSopDdsop.NetworkStructure.Web`
- `src/AdaptiveSopDdsop.NetworkStructure.Host`
- `tools/generate_network_structure_doc.py`
- `docs/network-structure-scoring-product-boundary.md`
- `docs/网络结构评分V2数据结构与开发过程.docx`
- network scoring material screenshots under `material/`
- `docs/ddsop-network-structure-sdbr-loop-large.png`
- `docs/ddsop-network-structure-sdbr-loop.svg`

Reason: these define, display, or document the separate network scoring product. They should remain in `codex/network-structure-product-line` until the product boundary and contract are formalized.

## Package C: Hold Until DDS&OP-Network Contract Exists

These are useful but should not be merged into `main` as-is.

- `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration/NetworkStructureIntegrationContracts.cs`
- `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration/NetworkStructureDataSourceAdapter.cs`
- `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration/DdsopNetworkGraphDataSource.cs`
- `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration/DdsopNetworkMetricsDataSource.cs`
- `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration/DdsopNetworkScoringDataSource.cs`
- `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration/NetworkScenarioValidationService.cs`
- `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration/NetworkCandidateRecalculationRequestBuilder.cs`
- `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration/LocalDdsopWhiteBoxScenarioGateway.cs`
- `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration/HttpDdsopWhiteBoxScenarioGateway.cs`
- `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration/NetworkStructureIntegrationModule.cs`
- `src/AdaptiveSopDdsop.Web/NetworkStructureIntegration/CandidateActionCombinationService.cs`

Reason: these are the current experimental DDS&OP to network scoring integration layer. They depend on network scoring project types and should be promoted only after a formal DDS&OP <-> Network Scoring contract exists.

## Main Merge Rule

Do not merge `codex/网络结构评分-feature` into `main` directly.

Use selective merge or a temporary branch from `main`, applying only Package A. Package B remains product-line. Package C waits for the future `codex/network-structure-contract` branch.

## Validation Required After Package A

Run:

```powershell
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj -p:UseSharedCompilation=false
& "$env:USERPROFILE\.dotnet-sdk-9\dotnet.exe" build AdaptiveSopDdsop.sln -p:UseSharedCompilation=false
```

Expected behavior:

- DDS&OP scenario workspace still loads.
- SDBR contract endpoints are available.
- Old DDS&OP optimization recommendation endpoint is absent.
- DDS&OP overview shows only a link to Network Structure Scoring.
- DDS&OP does not render the network scoring product workspace.

