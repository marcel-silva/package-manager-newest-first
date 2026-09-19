# Bug report draft: My Assets purchase-date ordering is reversed

**Suggested title:** Package Manager My Assets: Purchased date requests `desc` but receives oldest purchases first

## Environment

- Unity Editor: 6000.4.5f1 (cc83ebd631f8)
- OS: Windows
- Observed: 20 September 2026, Australia/Sydney
- Context: Package Manager → My Assets, signed in to an account with purchases across multiple dates
- Reproduction frequency: reproduced with both request directions during this investigation

## Reproduce without this workaround

1. Disable/uninstall Package Manager Newest First if present.
2. Open Package Manager, select My Assets.
3. Clear search and filters.
4. Choose Name (asc), then Purchased date, then Refresh.
5. Inspect the Purchased Date of the first few assets, and compare with recent acquisitions in the Asset Store account page.

**Expected:** The latest acquisitions appear first.

**Actual:** The oldest acquisitions appear first. The menu exposes Purchased date with no reverse-direction choice.

## Diagnostic evidence

Using Unity's existing authenticated Asset Store request service, two read-only requests were compared:

```text
GET /-/api/purchases?offset=0&limit=3&orderBy=purchased_date&order=desc
GET /-/api/purchases?offset=0&limit=3&orderBy=purchased_date&order=asc
```

Observed `grantTime` dates, in returned order:

| Request | First | Second | Third |
| --- | --- | --- | --- |
| desc | 2011-04-25 | 2012-04-18 | 2012-04-18 |
| asc | 2026-09-19 | 2026-09-19 | 2026-09-19 |

The second row also had descending timestamps within that date. Requests contained no offset changes or filtering differences. No credentials, account identifiers or raw purchase records are included here.

Unity's 6000.4 reference source maps the Purchased date option to `PurchasedDateDesc`, then `orderBy=purchased_date&order=desc`:

- https://github.com/Unity-Technologies/UnityCsReference/blob/6000.4/Modules/PackageManagerUI/Editor/Services/Pages/PageSortOption.cs
- https://github.com/Unity-Technologies/UnityCsReference/blob/6000.4/Modules/PackageManagerUI/Editor/Services/AssetStore/AssetStorePurchases.cs

**Inference:** the observed service semantics disagree with the Editor's request semantics. This does not establish the exact server-side cause or exclude account-specific behavior. The diagnostic does not establish cache corruption.

## Impact

Recent purchases require scrolling/searching through older purchases. Deprecated products can dominate the first page simply because they were acquired long ago.

## Temporary workaround

An unsupported local Editor extension changes only purchase-date requests from desc to asc. This restores newest-first order on the tested account. Disable it before reproducing or collecting Unity's bug-report logs. The correct product fix should restore the service/Editor contract and ideally expose an explicit direction control.

## Related reports

- https://discussions.unity.com/t/how-to-sort-packages-by-newest-first/1710144
- https://discussions.unity.com/t/cannot-sort-by-purchased-date-latest-first/934590

## Before submitting

- Reproduce in a small clean project using the same account, with the workaround absent. This has not yet been tested in a clean project.
- Add a screenshot showing the sort menu and an old purchase at the top.
- Review attached Editor logs for credentials/account data; do not attach command lines containing access tokens or full purchase-response dumps.
- Include the exact date/time because service behavior can change independently of Editor versions.

This is a prepared report, not a submitted Unity issue.
