# Validation — 20 September 2026

## Environment

Windows, Unity 6000.4.5f1, existing signed-in project and Asset Store account. Other operating systems and Unity releases have not been tested.

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

The Unity Test Runner attempt was blocked while the Editor stopped servicing main-thread commands. No successful Unity Test Runner result is claimed. Clean-project reproduction, full Editor process restart, end-to-end UI mode toggles/removal, sign-out/relogin, long-duration periodic detection, and macOS/Linux remain unverified. Search/filter URL preservation and non-purchase sort behavior were tested at policy level, not by manually exercising every UI control.

## Reproduce standalone checks on Windows

Run `Tests~/Run-Standalone.ps1` with `-UnityEditorData` pointing to the Editor's `Data` directory and `-NUnitFramework` pointing to the project's `com.unity.ext.nunit` package's `nunit.framework.dll`. The script builds into ignored `TestResults~`, executes the test fixtures, and fails on a nonzero test result. The machine needs .NET Framework, as standard on the tested Windows host.

For normal EditMode testing, expose package tests through UPM `testables` in a disposable test project and run the `NewestFirst.Tests` assembly. Do not test against an unsaved production scene.
