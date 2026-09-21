# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: .NET 10 (net10.0)

## Upgrade Options
- **Upgrade Strategy**: All-at-Once
- **Project Approach (Web Project)**: In-place rewrite
- **Unsupported Packages**: Resolve Inline
- **Unsupported API Handling**: Fix Inline
- **System.Web Adapters**: Use System.Web Adapters
- **Assembly Binding Redirects**: Document and Review Before Removing
- **Nullable Reference Types**: Leave Disabled
- **Test Coverage**: Skip

## Source Control
- **Source Branch**: main
- **Working Branch**: upgrade-to-net10-vscode-aug-21
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## Strategy
**Selected**: All-At-Once
**Rationale**: Single project (ContosoUniversity.csproj) — no dependency graph to phase across; the .NET Framework single-project override selects All-at-Once.

### Execution Constraints
- One atomic upgrade pass: update project file, update packages, restore, then build and fix all compilation errors in a single bounded pass — not an iterative retry loop
- SDK-style conversion is a separate task from the TFM upgrade and must complete (and build on `net48`) before the TFM upgrade task starts
- Unsupported packages and API changes are resolved inline in the upgrade task — no stubs, no deferred resolution subtasks
- System.Web Adapters is the bridging mechanism for the `System.Web`-dependent MVC surface — do not attempt a full architectural rewrite of controllers
- Binding redirects are documented per-entry (conflict/downgrade status + recommendation) before any removal; do not blanket-delete
- Testing/validation happens after the atomic upgrade completes, not interleaved with it — no automated test baseline exists (Test Coverage was skipped)
