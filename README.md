# Package Manager Newest First

An experimental, Editor-only workaround for Asset Store **My Assets → Purchased date** returning oldest purchases first.

Unity 6000.4 requests `orderBy=purchased_date&order=desc`. On the service tested on 20 September 2026, `desc` returned 2011 purchases first while `asc` returned September 2026 purchases first. This package detects that inversion and changes only that request parameter.

## Compatibility and status

- Supports **6000.4.x** and **6000.7.0b1**, tested in **6000.4.5f1 and 6000.7.0b1 on Windows**. Other versions, including later 6.7 betas, are deliberately disabled.
- Uses **unsupported internal Editor APIs via reflection** and a Mono transparent proxy. Unity updates can break it.
- No Editor binaries are modified. No external runtime dependencies; nothing is included in player builds.
- Not affiliated with or endorsed by Unity.
- **Not eligible for Asset Store submission in its current form:** guideline 2.5.g prohibits use of internal Editor APIs discovered through reflection. Intended for direct source/Git distribution.

## Unity 6.7 reproduction

**Unity report: IN-154571.** Unity acknowledged receipt; reproduction, a fix, and an ETA have not yet been confirmed. See the [report notes](Documentation~/UNITY-BUG-REPORT.md).

On 20 September 2026, the maintainer reproduced oldest-first ordering in a newly created URP project using **6000.7.0b1 (6f112f2bea37)** on Windows, without this workaround installed. The supplied screenshot shows Purchased date selected, an empty search, and RageSpline at the top with a purchase date of April 25, 2011. Project version and absence of the workaround were also checked on disk.

The 6000.7 reference source still sends `orderBy=purchased_date&order=desc`; surrounding refactoring does not change that direction. This confirms the UI symptom in that beta on the tested account, not the precise backend cause. **Version 0.1.1 adds tested support for 6000.7.0b1:** Auto detection, refresh, pagination across 67 purchases, and disable/re-enable passed. See the [compatibility test](Documentation~/UNITY-6000.7-TEST.md). See the [reproduction and source comparison](Documentation~/UNITY-BUG-REPORT.md#unity-600070b1-runtime-reproduction).

## Install

Download/clone this repository, then in Unity use **Window → Package Management → Package Manager → + → Install package from disk**, and select this repository's `package.json`.

Alternatively, choose **Install package from Git URL** and paste this release-pinned URL:

```text
https://github.com/marcel-silva/package-manager-newest-first.git#v0.1.2
```

Open **My Assets**, select **Purchased date**, and let Auto detection finish. If the window was already open, the extension requests a refresh after detection.

## Controls

**Tools → Package Manager Newest First**:

- **Auto (detect service order)** — default. Makes two read-only requests for five purchases each, compares `grantTime`, and corrects only when both responses consistently demonstrate reversed ordering.
- **Enabled (force asc)** — forces the workaround if detection is inconclusive. Turn it off when the service is fixed.
- **Disabled** — restores the original HTTP factory and refreshes the current My Assets page.
- **Recheck now** — retries detection/compatibility checks.
- **Status** — shows detection status and how many requests were changed; never displays purchase details or credentials.

The mode is saved per project on this machine in EditorPrefs. The extension initializes after Editor restarts and script reloads. Auto rechecks every 15 minutes while the Editor is idle outside Play mode. During a probe, requests pass through unchanged; the list is refreshed afterward. Offline/sign-out, inconsistent dates, or fewer than two distinct dates cause Auto to leave requests unchanged. Sign in and use Recheck to retry immediately.

## Scope and privacy

Only GET requests from Package Manager's own Asset Store service to the exact `/-/api/purchases` path, with `orderBy=purchased_date&order=desc`, are changed. Pagination offsets, filters, search text, tags, downloads, other sorts and registries retain Unity's behavior. The original factory still creates requests, authenticates, processes responses, and cancels requests.

The extension never extracts or stores tokens and has no telemetry, external server, persistent purchase cache, or analytics. Detection dates exist only in memory and are discarded after comparison. Installing enables the two small diagnostic requests to Unity automatically.

## Remove

Choose **Disabled**, then remove **Package Manager Newest First** in Package Manager. Removing the source package triggers an assembly reload, restoring Unity's original factory. No Editor installation repair or cache deletion is needed. The per-project mode preference may remain but is inert without the package.

## Tests and limitations

See [validation](Documentation~/VALIDATION.md), [architecture](Documentation~/ARCHITECTURE.md), and the [Unity bug report](Documentation~/UNITY-BUG-REPORT.md).

To expose package tests in a separate test project, add this package name to that project's UPM `testables` list, then run EditMode tests in Test Runner. This is test configuration only; normal users do not need it. A Windows standalone test script is also included under `Tests~`; see the validation document for usage. Tests cover query scope and preservation, detection outcomes, malformed input and proxy forwarding. Live validation is also necessary because internal APIs and service behavior are outside these unit tests.

Do not copy Unity reference source or ship modified Unity DLLs with this package. All implementation source here is original. Contributions should avoid recording account data in issues or fixtures.

## License

MIT; see [LICENSE](LICENSE).
