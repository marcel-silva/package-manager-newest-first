# Compatibility test: Unity 6000.7.0b1

Tested on Windows in BugFound on 20 September 2026.

Tested first in a separate local checkout and incorporated into **v0.1.1**. The version guard now permits **6000.7.0b1 only** in addition to 6000.4.x. No other implementation changes were needed. The v0.1.0 release remains unchanged and blocks 6.7.

- Local UPM installation and compilation passed.
- Auto detected the service inversion and installed the HTTP factory decorator.
- First page: 17 purchases, newest-first, starting 2026-09-19.
- Normal pagination appended 50 purchases; all 67 dates remained newest-first across the boundary.
- Disabled restored the original factory; the list returned to oldest-first.
- Restored Auto after testing.

This validates the checked flows on this exact beta/OS/account, not every 6.7 release or platform. Full process restart, removal, and all search/filter UI combinations remain untested. The implementation uses unsupported Editor APIs.
