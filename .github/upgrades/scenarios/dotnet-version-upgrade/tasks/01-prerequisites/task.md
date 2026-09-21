# 01-prerequisites: Verify toolchain and target SDK

Confirm the .NET 10 SDK is installed and available to the build, and that no `global.json` pins an incompatible SDK version. Record the current project state (non-SDK-style WAP, `packages.config`, `net48`) as the starting point for the conversion and upgrade tasks that follow.

**Done when**: .NET 10 SDK is confirmed installed and any `global.json` is compatible with `net10.0`, or updated to be.
