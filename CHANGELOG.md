# Changelog

## 0.1.2 — 2026-09-20

- Annotated the Editor service with NoAutoStaticsCleanup on Unity 6000.5+ to resolve UAL0010/UAL0013 while preserving its explicit factory cleanup lifecycle.
- Verified recompilation on 6000.7.0b1 with zero assembly compiler messages. The annotation is excluded on 6000.4.

## 0.1.1 — 2026-09-20

- Added support for exactly Unity 6000.7.0b1 alongside 6000.4.x.
- Verified compilation, Auto detection, refresh, pagination across 67 purchases, and disable/re-enable on Windows in 6000.7.0b1.

- Documented the Unity 6000.7 source comparison: purchase-date requests still use `desc`.
- Recorded maintainer reproduction in a new URP project on 6000.7.0b1 without the workaround, followed by successful workaround testing.
- Updated the release-pinned Git installation URL.

## 0.1.0 — 2026-09-20

- Initial experimental Editor-only package.
- Automatic service-order detection; forced correction and disable modes.
- Narrow request decoration with normal pagination, search and filters.
- Unit tests, live validation notes, and reproducible Unity bug report.
