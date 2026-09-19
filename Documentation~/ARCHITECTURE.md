# Architecture and maintenance

This implementation is original code. It uses reflection to discover a small set of Unity Editor objects; it does not copy, redistribute, or patch Unity's implementation.

## Request flow

1. `[InitializeOnLoad]` schedules initialization through `EditorApplication.update`.
2. Resolve Package Manager's existing `IAssetStoreRestAPI` service from `ServicesContainer`.
3. Check the version and expected API shape before replacing that service instance's `m_HttpClientFactory` field with a transparent decorator.
4. The decorator forwards every method to the original factory. Only `GetASyncHTTPClient` can change its URL argument, and only for the exact purchase endpoint and purchase-date descending sort.
5. Unity continues to own the resulting HTTP client, authentication, headers, callbacks, cancellation tags, cache, pagination and rendering. We do not synthesize UI results or truncate the purchase list.
6. Restore the original factory on disable, assembly reload or Editor exit, only if the field still contains our own decorator.

The wrapper uses Mono `RealProxy`, available in the tested Editor. This is an unsupported integration and is not a guarantee for future Unity runtimes. No Harmony dependency or binary rewriting is used.

## Automatic detection

Auto temporarily passes requests through, calls Unity's existing authenticated purchase service with both directions and limit 5, and examines `grantTime` without retaining product names or IDs. Ascending dates under `desc` and descending dates under `asc` enable correction. The opposite disables correction. Equal, absent, malformed, mixed, timed-out or failed responses leave correction off. Callbacks carry a generation token so results from a disabled/restarted probe cannot reactivate the workaround.

Recheck occurs after initialization, on explicit request, and every 15 minutes. A successful probe refreshes the active My Assets page, preventing a mixed list after the direction changes. The first page can briefly show Unity's normal order while detection runs. Do not treat this as an offline cache.

## Extending support

Before admitting another Editor version, test startup and assembly reload, all mode transitions, refresh, pagination across page boundaries, search/filters, other sorts, request errors and removal. Check every required reflected member. If Unity introduces a supported public extension point, replace this integration rather than relaxing compatibility checks blindly.

The pure policy and factory decorator have EditMode unit tests. End-to-end testing still requires an authenticated account with purchases on distinct dates; never include account credentials or purchase dumps in fixtures or release archives.
