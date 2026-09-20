# GitHub releases

Repository: https://github.com/marcel-silva/package-manager-newest-first

This folder is a standalone UPM package. Publish only this folder, not the parent game project.

## Version 0.1.1

Adds tested support for Unity 6000.7.0b1 alongside 6000.4.x. On Windows, installation, compilation, automatic detection, refresh, pagination across 67 purchases, and disable/re-enable passed. Other 6.7 versions remain disabled. Uses unsupported internal APIs and is not suitable for Asset Store submission.

Install URL:

```text
https://github.com/marcel-silva/package-manager-newest-first.git#v0.1.1
```

When creating the GitHub release, select the existing v0.1.1 tag and mark it **pre-release**. Do not move v0.1.0; existing installations can remain pinned to it. GitHub automatically provides source archives suitable for installing from disk.

For future releases, update package.json, CHANGELOG.md, compatibility notes and the installation URL together. Validate the exact supported versions, commit, then create and push a new version tag. Avoid adding game assets, Editor logs, access tokens or purchase-response files.

The maintainer submitted the Unity bug report and received acknowledgement as IN-154571. Unity has not yet confirmed reproduction or a fix.
