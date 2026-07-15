# Task 7 Report: Known Local Smoke Record Repair

Base commit: `35d9e1e`

Commit message: `fix: repair known local smoke records safely`

## Delivered

- Added `LocalDatabaseRepairService` with the fixed repair ID `2026-07-15-smoke-mojibake-v1` and the required public result/interface signatures.
- The repair uses one SQLite transaction to create/check its journal, delete audits for only the two exact coordination IDs, delete only those two items, repair only `BASE-20260714-002 + Codex ??`, append one `DataRepairApplied` audit when that update occurs, write the journal last, and commit.
- Enabled `PRAGMA foreign_keys = ON` before beginning the repair transaction.
- Read the original `trg_current_baseline_snapshots_no_update` name and SQL from `sqlite_master`, dropped only that trigger, restored it immediately from its stored SQL, and left the other three immutable-baseline triggers untouched.
- Used `COALESCE(MAX(sequence), 0) + 1` for the corrective baseline audit sequence. Existing audit rows and messages are never updated.
- The corrective audit message contains correct Chinese and does not repeat the old consecutive-question-mark value.
- Registered the repair as an internal singleton. After `builder.Build()`, startup explicitly resolves `CurrentBaselineService`, `CoordinationLedgerService`, `ScenarioRunPersistenceService`, and `MasterSettingsGovernanceService` in order, then invokes repair `Apply()` once. No repair endpoint or contract/public-demo initialization was added.
- Added real-service Unicode round-trip coverage for coordination `title`, `owner`, `decision`, `decision_rationale`, `actual_outcome`, and `created_by`, plus current-baseline `created_by`.

The actual `snapshot_number` column is `UNIQUE`. Therefore the exact target and a same-number normal-Chinese control cannot coexist in one database. With controller confirmation, the test uses a second isolated temporary SQLite database for the same-number normal-Chinese control while preserving the requested product semantics.

## Review Follow-up

The independent code review found no Critical or Important issues. Its one Minor finding was that the rollback test initially accepted any `SqliteException`. The test now captures the exception message and requires the fixed test-trigger text `test blocks corrective audit`, so an unrelated earlier SQLite failure cannot falsely prove atomic rollback. After that change, the full `111/111` suite, serial build, and protected-boundary verification were rerun successfully.

## TDD Evidence

- Initial RED: `dotnet run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore` exited 1 with `CS0246` for the intentionally absent `ILocalDatabaseRepairService` and `LocalDatabaseRepairService`.
- Initial GREEN: both required tests passed and the complete suite reported `111/111`.
- Review RED: after adding an assertion that the new corrective audit must not contain `??` or `U+FFFD`, only `TestKnownSmokeRecordRepairIsScopedAuditedAndIdempotent` failed because the first message version repeated the old match value.
- Review GREEN: changed the new audit message to `已将已知本地烟测创建人乱码修复为 Codex 烟测。`; the complete suite returned to `111/111`.
- Atomicity control: a third temporary database adds a test-only trigger that rejects only `DataRepairApplied` inserts. `Apply()` throws, and assertions prove rollback restored both exact items, all three item audits, the old baseline creator, the original baseline audit, the exact snapshot no-update trigger SQL, and the journal/table state. The complete suite remains `111/111`.

Added the two planned tests:

- `TestKnownSmokeRecordRepairIsScopedAuditedAndIdempotent`
- `TestSqliteRoundTripsChineseWithoutQuestionMarks`

## Apply Results

| Case | WasAlreadyApplied | Deleted Items | Deleted Item Audits | Repaired Baselines | Added Baseline Audits |
| --- | ---: | ---: | ---: | ---: | ---: |
| Exact-target database, first `Apply()` | `false` | 2 | 3 | 1 | 1 |
| Exact-target database, second `Apply()` | `true` | 0 | 0 | 0 | 0 |
| Same-number normal-Chinese control, first `Apply()` | `false` | 0 | 0 | 0 | 0 |
| Same-number normal-Chinese control, second `Apply()` | `true` | 0 | 0 | 0 | 0 |

Both successful first calls write exactly one journal row. Both second calls produce an identical logical database fingerprint before and after the call.

## Trigger and Rollback Verification

- Success path: the saved and restored `trg_current_baseline_snapshots_no_update` SQL strings compare exactly, all four `trg_current_baseline*` triggers remain, and a direct baseline update is still blocked.
- Zero-match path: the trigger SQL also compares exactly and no corrective audit is added.
- Failure path: the test-only audit rejection aborts the transaction; SQLite rolls back item/audit deletions, baseline update, trigger DDL, corrective audit, and journal creation. The original no-update trigger still blocks updates.

## Final Verification

```text
dotnet run --project tests\AdaptiveSopDdsop.Tests\AdaptiveSopDdsop.Tests.csproj --no-restore
111 test(s) passed.
```

```text
dotnet build AdaptiveSopDdsop.sln --no-restore -m:1
Build succeeded: 0 errors, 2 NU1900 warnings because the offline environment could not query the NuGet vulnerability service.
```

```text
& .\scripts\verify-protected-boundaries.ps1 -Baseline 4e39ec5
Protected boundaries match baseline 4e39ec5.
```

`git diff --check` reported no whitespace errors. Strict UTF-8 decoding passed for every changed implementation/test file. The replacement-character/common-mojibake scan found none; `Codex ??` appears in production only as the single fixed old-value match constant and in tests only as explicit seed/assertion data. The new corrective audit is separately asserted free of `??` and `U+FFFD`.
