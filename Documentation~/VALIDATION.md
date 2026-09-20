# Validation — 20 September 2026

## Environment

Workaround validation: Windows, Unity 6000.4.5f1, existing signed-in project and Asset Store account. Separate bug reproduction: Windows, Unity 6000.7.0b1, new URP project without the workaround. Other operating systems and workaround compatibility on other Unity releases have not been tested.

## Unity 6000.7: source review and maintainer reproduction

The 6000.7 reference source was inspected on 20 September 2026. `PurchasedDateDesc` still produces `orderBy=purchased_date&order=desc`; surrounding query/filter refactoring does not change that direction. See the [pinned source comparison](UNITY-BUG-REPORT.md#unity-60007-source-comparison).

The maintainer then reproduced the UI symptom in a new URP project using 6000.7.0b1 (6f112f2bea37), without the workaround. Their screenshot shows Purchased date selected and a 2011 purchase first. The project version and absence of workaround references in Assets/Packages were checked on disk. This is maintainer-provided runtime evidence, not a repeat of the asc/desc API probe on 6.7. The extension was not installed or tested there; package support remains limited to 6000.4.x.

## Confirmed in the running Editor

- Local UPM installation and compilation succeeded without package compile errors.
- Transparent factory decoration succeeded without patching Unity binaries.
- Auto observed opposite ordering for asc/desc and enabled correction.
- The normal My Assets first page contained 16 items, with purchase dates in descending order; the first date was 2026-09-19.
- Normal `LoadMore(50)` appended the next page. All 66 dates remained monotonically newest-first across the page boundary. The corrected-request counter increased from 1 to 2.
- Automatic detection and installation resumed after script/domain reloads during development.

## Automated policy and decorator tests

Eight test cases passed using the included standalone runner on Windows/.NET Framework, compiled using Unity's bundled Roslyn and reference assemblies with the project's NUnit framework:

- Pagination, search and filter preservation.
- Name and update-date sorts left unchanged.
- Other endpoints, disabled mode and invalid input left unchanged.
- Ambiguous parameters and already-ascending requests left unchanged.
- Reversed and correct server-order detection.
- Equal, missing and inconsistent dates treated as inconclusive.
- Transparent proxy forwards GET, POST, cancellation tags, properties and original exception types; turning its correction off restores unchanged requests.

These are the same synchronous NUnit fixtures shipped under `Tests/Editor`; the standalone runner is not a replacement for Unity's broader Test Runner behavior.

## Remaining checks

The Unity Test Runner attempt was blocked while the Editor stopped servicing main-thread commands. No successful Unity Test Runner result is claimed. Full Editor process restart, end-to-end UI mode toggles/removal, sign-out/relogin, long-duration periodic detection, and macOS/Linux remain unverified. Search/filter URL preservation and non-purchase sort behavior were tested at policy level, not by manually exercising every UI control. Clean-project reproduction of the original bug was subsequently supplied by the maintainer on 6000.7.0b1 as described above.

## Reproduce standalone checks on Windows

Run `Tests~/Run-Standalone.ps1` with `-UnityEditorData` pointing to the Editor's `Data` directory and `-NUnitFramework` pointing to the project's `com.unity.ext.nunit` package's `nunit.framework.dll`. The script builds into ignored `TestResults~`, executes the test fixtures, and fails on a nonzero test result. The machine needs .NET Framework, as standard on the tested Windows host.

For normal EditMode testing, expose package tests through UPM `testables` in a disposable test project and run the `NewestFirst.Tests` assembly. Do not test against an unsaved production scene.
