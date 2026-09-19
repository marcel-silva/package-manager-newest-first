# Prepare a GitHub release

This folder is a standalone UPM package and repository. Publish only this folder, not the parent game project.

1. Review the source, license, supported-version limit, and validation notes.
2. Create a GitHub repository named `package-manager-newest-first` in the desired account.
3. Push this repository to that remote. Avoid adding any game assets, local paths, Editor logs, access tokens or purchase-response files.
4. After validation, create a `v0.1.0` tag and a release marked **pre-release**.
5. Add the actual repository URL and Git installation example to README. The Git URL should end in `.git#v0.1.0` to pin the version.
6. Attach the source ZIP if desired. Users can extract it and install its `package.json` from disk.

Suggested repository description:

> Experimental Unity Editor workaround for reversed My Assets purchase-date sorting. Automatic detection, per-project controls, and normal pagination. Unity 6000.4; unsupported internal APIs.

Suggested release notes:

> Initial experimental release. Tested on Windows with Unity 6000.4.5f1. Auto detects reversed purchase-date order and changes only affected requests. Includes forced/disabled modes and documentation. Uses unsupported Unity internal APIs and is not suitable for Asset Store submission. See validation notes for tested and untested behavior.

Before claiming broader compatibility, test another Editor version and platform. If the service fixes its order, verify Auto becomes inactive. Keep the disable control available.

This package has not been published and its bug report has not been submitted automatically.
