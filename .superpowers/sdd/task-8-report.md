# Task 8 Report: Baseline History Reconciliation

Base commit: `28c42b9`

## Delivered

- Added one current-baseline reconciliation card with backend-provided fact-set, cutoffs, scope, line count, status, and balance-bridge rows.
- Rendered the same frozen-snapshot lineage in the immutable evidence drawer. Legacy frozen snapshots explicitly show `旧版本未保存历史衔接证据`; no lineage is backfilled.
- Added the client-side freeze safety mirror for absent/incomplete lineage, incomplete lines, invalid or reversed cutoffs, non-finite differences, and differences over `0.01`. Backend validation remains authoritative.
- Corrected baseline KPI subtitles for rolling service, cutoff inventory, WIP provenance, and forward-horizon peak load.
- Escaped reconciliation labels/reasons and retained backend-provided values without client balance recomputation.

## TDD evidence

- RED: the new Node fixture failed because the reconciliation DOM hosts and renderer did not exist.
- GREEN: the fixture passes complete lineage, nonzero-difference rejection, reversed-cutoff rejection, KPI wording, and legacy frozen-lineage coverage.

## Verification

- Bundled Node fixture: passed all 8 baseline planning evidence fixture groups.
- Release harness: completed successfully with the reconciliation UI coverage and all existing tests.
- `git diff --check`: completed without whitespace errors before the fix commit.

## Review hardening

- Frozen baseline drawers now retain the complete backend bridge per reconciliation line: metric/object, historical closing balance, interval increase/decrease, adjustment, baseline balance, reported difference, evidence status, and escaped difference reason.
- The frozen renderer returns an ordered set of drawer sections (summary plus line detail), while a legacy null lineage still yields exactly one immutable `旧版本未保存历史衔接证据` section.
- The executable fixture now verifies the complete frozen bridge, HTML-safe difference reasons, and candidate freeze blocking for absent, incomplete, null/non-finite, invalid-cutoff, and reversed-cutoff reconciliation lineage.
