# Progress Details: 03.03-legacy-config-migration

## Research findings

- Assessment query (`ConfigurationManager` search) confirmed exactly 16 `Api.0002` issues across 3 files:
  - `Services/NotificationService.cs` — 4 issues (excluded, owned by 03.04).
  - `Data/SchoolContextFactory.cs` — 6 issues (in scope).
  - `Global.asax.cs` — 6 issues; this file no longer exists — 03.02 already restructured it into `Program.cs`'s `InitializeDatabase()` local function, which carried a `TODO(03.03)` marker at the same `ConfigurationManager.ConnectionStrings["DefaultConnection"]` read (in scope).
- `Web.Debug.config` / `Web.Release.config` do **not exist on disk**, but `ContosoUniversity.csproj` still had stale `<Content Include="...">` entries for both (with `DependentUpon="Web.config"`). No XDT transform content to reconcile — the base `Web.config` is the sole source of `DefaultConnection`/`appSettings` for both Debug and Release.
- The project had a legacy `<Reference Include="System.Configuration" />` (framework reference) and no `System.Configuration.ConfigurationManager` NuGet package — on net10.0 this reference resolves to a stub facade with no real `ConfigurationManager` implementation (returns empty settings), which is why the reads were flagged as source-incompatible (`Api.0002`) despite compiling.
- Confirmed via `mcp_upgrade_get_supported_package_version` that `System.Configuration.ConfigurationManager` 10.0.12 is the net10.0-supported version.

## Decision (Step 1)

Chose the **`System.Configuration.ConfigurationManager` NuGet package bridge** over migrating to `Microsoft.Extensions.Configuration`/`appsettings.json`, per the task's guidance for a single in-place project. No concrete blocker found for the bridge — however, the package's *default* config-file discovery convention (`<entryAssembly>.dll.config`/`.exe.config` next to the app) is documented to vary depending on how the process is launched (`dotnet app.dll` vs. apphost), which is a real risk for a web app that can be launched multiple ways. To avoid that ambiguity, connection-string resolution uses `ConfigurationManager.OpenMappedExeConfiguration` with an **explicit** `ExeConfigurationFileMap` path instead of relying on the implicit naming convention.

## Changes made

1. **`ContosoUniversity.csproj`**
   - Removed the legacy `<Reference Include="System.Configuration" />` (stub facade, conflicted with real config functionality).
   - Added `<PackageReference Include="System.Configuration.ConfigurationManager" Version="10.0.12" />`.
   - Removed the stale `<Content Include="Web.Debug.config">` / `<Content Include="Web.Release.config">` items (files don't exist; no transforms to reconcile — single `Web.config` serves Debug and Release identically).
   - Added a `CopyLegacyConfigForConfigurationManager` target (`AfterTargets="Build;Publish"`) that copies `Web.config` to `$(OutDir)legacy.config` — a name that can't collide with the SDK's own IIS `web.config` generation — so `ConfigurationManager` has a reliable, explicit file to read at runtime regardless of launch method or build configuration.
2. **`Data/SchoolContextFactory.cs`** — added a private `GetConnectionString()` helper that opens `legacy.config` (next to the app, via `AppContext.BaseDirectory`) through `ConfigurationManager.OpenMappedExeConfiguration` and reads `DefaultConnection` from it. `Create()` now calls this helper instead of reading `ConfigurationManager.ConnectionStrings` directly.
3. **`Program.cs`** — `InitializeDatabase()` now calls `SchoolContextFactory.Create()` instead of duplicating the connection-string/`DbContextOptionsBuilder` setup; removed the `TODO(03.03)` marker, the direct `ConfigurationManager` call, and the now-unused `Microsoft.EntityFrameworkCore` using directive. This also means the connection-string logic exists in exactly one place going forward.

`Services/NotificationService.cs` was **not modified** (owned by subtask 03.04).

## Validation

- `dotnet build` (Debug) and `dotnet build -c Release`: no errors or warnings from `ContosoUniversity.csproj`, `Data/SchoolContextFactory.cs`, or `Program.cs`. Remaining build errors are 100% pre-existing/out-of-scope: `Services/NotificationService.cs` (MSMQ/`System.Messaging`, owned by 03.04) and several `Controllers/*.cs` + `Views/*.cshtml` files still using unmigrated `System.Web.Mvc` attributes/types (`HttpPostAttribute`, `BindAttribute`, `ValidateAntiForgeryToken`, `PaginatedList<>`, etc.) — explicitly called out as expected/acceptable residuals for this task.
- Confirmed via `grep` that the only remaining `ConfigurationManager` usage outside `NotificationService.cs` is the intentional bridge call in `SchoolContextFactory.cs`; `Program.cs` no longer references `ConfigurationManager` directly.
- Directly invoked the new MSBuild target (`dotnet msbuild -t:CopyLegacyConfigForConfigurationManager`, since a full build can't complete yet due to the unrelated controller/view errors above) and confirmed `bin/net10.0/legacy.config` is produced with the correct `<connectionStrings>`/`<appSettings>` content from `Web.config`.
- Built an isolated throwaway console app (outside the repo, cleaned up afterward) replicating `SchoolContextFactory.GetConnectionString()` exactly, pointed at the copied `legacy.config`, and confirmed it resolves the exact expected value:
  `Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=ContosoUniversityNoAuthEFCore;Integrated Security=True;MultipleActiveResultSets=True`
- Debug/Release equivalence: trivially satisfied — there are no environment-specific transform values; the same `Web.config` (and therefore the same copied `legacy.config`) is used for both configurations.

## Deviations from task.md

- None in approach. One implementation detail beyond the task's literal steps: introduced the `CopyLegacyConfigForConfigurationManager` MSBuild target and explicit `OpenMappedExeConfiguration` file-map (rather than relying on `ConfigurationManager`'s implicit default config-file convention), to make connection-string resolution deterministic regardless of how the app is launched — this was necessary to satisfy "resolve the connection string correctly on net10.0," not just "compiles."
- Consolidated the duplicate connection-string/`DbContextOptionsBuilder` logic in `Program.cs` into a single call to `SchoolContextFactory.Create()` rather than reimplementing the same config-read logic twice.

## Decomposition verdict

Evaluated the scenario's Execution stage and Breakdown Hints for the `mvc`/config-migration flavor before editing. The task scope (one connection-string read fix + a stale config-transform cleanup, confined to 2 in-scope files plus the project file) is a single coherent unit of work with no independent sub-concerns, dependency ordering, or internal decision points beyond the single approach decision already resolved in Step 1. Verdict: **atomic** — executed directly, no `TaskBreaker` escalation.
