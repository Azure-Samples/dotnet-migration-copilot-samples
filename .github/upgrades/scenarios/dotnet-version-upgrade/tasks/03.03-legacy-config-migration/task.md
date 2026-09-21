# 03.03-legacy-config-migration: Migrate ConfigurationManager connection-string/appSettings reads and Web.config transforms (excludes NotificationService.cs)

## Objective
Resolve the legacy configuration API issues (16 `Api.0002` issues) for connection-string and app-setting reads, and reconcile the `Web.Debug.config`/`Web.Release.config` XML transforms with a net10.0-appropriate mechanism.

Runs after 03.02 (which restructures `Global.asax.cs` into `Program.cs` and leaves the `ConfigurationManager.ConnectionStrings` read as a marked hand-off point).

## Scope
`Data/SchoolContextFactory.cs` (6 `Api.0002` issues), the connection-string read left in `Global.asax.cs`/`Program.cs` by subtask 03.02, `Web.config` `<appSettings>`/`<connectionStrings>`, `Web.Debug.config`, `Web.Release.config`.
**Explicitly excludes** `Services/NotificationService.cs` — its `ConfigurationManager.AppSettings["NotificationQueuePath"]` read is owned by subtask 03.04 (MSMQ replacement) since it's part of the same file/technology-decision change; do not edit that file here.

## Steps
1. Decide the interim config-access approach: continue using `System.Configuration.ConfigurationManager` (works on net10.0 via the `System.Configuration.ConfigurationManager` NuGet package — simplest, lowest-risk bridge) vs. migrating to `Microsoft.Extensions.Configuration` (`appsettings.json` + `IConfiguration`). Given this is a single in-place project (not a rewrite), prefer the `ConfigurationManager` bridge unless a concrete blocker is found; document the choice.
2. Update `Data/SchoolContextFactory.cs` to use the chosen approach for the `DefaultConnection` connection string.
3. Update the connection-string read remaining in `Global.asax.cs`/`Program.cs` (left by 03.02) to match.
4. Reconcile `Web.Debug.config`/`Web.Release.config` transforms: if staying on `ConfigurationManager` + `Web.config`, confirm XML transforms still apply under the net10.0 SDK-style build (may need `Microsoft.NET.Sdk.Web` transform support verified) or migrate the debug/release-specific values to `appsettings.{Environment}.json` if `Microsoft.Extensions.Configuration` was chosen in step 1.
5. Build and verify connection-string resolution works for both Debug and Release configurations.

## Relevant skills
managing-package-references (if adding `System.Configuration.ConfigurationManager` package), building-projects.

## Done when
- All 16 `Api.0002` config-related issues outside `NotificationService.cs` are resolved.
- `SchoolContextFactory.cs` and the `Global.asax.cs`/`Program.cs` connection-string read compile and resolve the connection string correctly on net10.0.
- Debug/Release config-transform equivalence is confirmed or migrated, with the decision documented.
- `dotnet build` shows no errors from `SchoolContextFactory.cs` or config-related code (`NotificationService.cs` errors remain, owned by 03.04).

## Research findings (recorded before implementation)

- Assessment query for `ConfigurationManager` confirmed exactly 16 `Api.0002` issues in 3 files: `Services/NotificationService.cs` (4, excluded/03.04), `Data/SchoolContextFactory.cs` (6), `Global.asax.cs` (6). `Global.asax.cs` no longer exists — 03.02 folded its connection-string read into `Program.cs`'s `InitializeDatabase()`, marked with a `TODO(03.03)` comment.
- `Web.Debug.config`/`Web.Release.config` do not exist on disk, despite stale `<Content Include>` entries (with `DependentUpon="Web.config"`) still present in `ContosoUniversity.csproj`. No XDT transform values exist to reconcile/migrate — `Web.config` is the sole, shared source for both Debug and Release.
- The csproj had a legacy `<Reference Include="System.Configuration" />` and no `System.Configuration.ConfigurationManager` NuGet package. On net10.0 that bare reference resolves to a non-functional stub facade, which is why the reads were flagged even though they compiled. Confirmed via `get_supported_package_version` that `System.Configuration.ConfigurationManager` 10.0.12 is the net10.0-supported version.
- Decision (Step 1): kept the `ConfigurationManager` bridge (no concrete blocker for a single in-place project). To avoid the package's launch-method-dependent default config-file discovery convention, resolution uses an **explicit** `ExeConfigurationFileMap` pointing at a `legacy.config` copy of `Web.config`, produced by a new `CopyLegacyConfigForConfigurationManager` MSBuild target (`AfterTargets="Build;Publish"`) — see `progress-details.md` for full validation, including an isolated runtime test confirming the connection string resolves correctly.
