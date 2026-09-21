# 02-sdk-style-conversion: Convert ContosoUniversity.csproj to SDK-style

Convert the non-SDK-style `ContosoUniversity.csproj` (WAP, `packages.config`) to SDK-style format while remaining on `net48`. This is a structural change only — it must not be combined with the TFM upgrade in task 03, since SDK conversion and TFM/API changes have different failure modes. Conversion migrates `packages.config` references to `PackageReference` as part of the same step.

Watch for WAP-specific MSBuild constructs (`Microsoft.WebApplication.targets`, `Views` content globbing, `Web.config` transforms, `PublishProfiles`) that need SDK-style equivalents, and confirm the project still builds on `net48` after conversion before proceeding.

**Done when**: `ContosoUniversity.csproj` is SDK-style, uses `PackageReference` for all dependencies, still targets `net48`, and the project builds successfully.

## Research Findings (pre-conversion)

- **Solution**: `ContosoUniversity.sln` — single project, topological order confirmed via tool: `ContosoUniversity.csproj` only (no dependencies/dependants).
- **Current format**: legacy WAP project, `ToolsVersion="15.0"`, `TargetFrameworkVersion=v4.8`, `ProjectTypeGuids` include the WAP guid `{349c5851-...}`. Imports `Microsoft.WebApplication.targets` and has a `ProjectExtensions`/`FlavorProperties` block (IIS Express dev-server settings, port 58801).
- **packages.config**: 45 packages listed (`net482` marker). Cross-checked against the 39 `<Reference HintPath>` entries in the csproj — all HintPath-based references map to a packages.config entry. Notable groups:
  - Framework-replaced-by-SDK packages (assessment marks these "functionality included with framework reference", so they must NOT become PackageReference once SDK style is used, but per the skill's hard constraint no package version/behavior changes beyond what conversion implies — the conversion tool decides mapping): `Microsoft.AspNet.Mvc`, `Microsoft.AspNet.Razor`, `Microsoft.AspNet.WebPages`, `Microsoft.AspNet.Web.Optimization`, `Microsoft.CodeDom.Providers.DotNetCompilerPlatform`, `Microsoft.Web.Infrastructure`, `NETStandard.Library`, `System.Buffers`, `System.ComponentModel.Annotations`, `System.Memory`, `System.Numerics.Vectors`, `System.Threading.Tasks.Extensions`.
  - EF Core 3.1.32 stack (7 packages) + its transitive deps (Extensions.*, Bcl.*, Diagnostics, Collections.Immutable) — kept at current versions (version bumps are task 03/04 scope, not this task).
  - `Microsoft.Data.SqlClient` 2.1.4 + `Microsoft.Data.SqlClient.SNI.runtime` 2.1.1 — project has a custom `CopySQLClientNativeBinaries` AfterTargets copying SNI native DLLs from the `packages\` folder; this target references package-relative paths that will change once packages move to the NuGet global cache, needs verification after conversion.
  - Front-end packages with no explicit `<Reference>` (content-only): `bootstrap`, `jQuery`, `jQuery.Validation`, `Microsoft.jQuery.Unobtrusive.Validation`, `Modernizr`, `WebGrease` (WebGrease has a Reference), `Antlr` (has Reference, Antlr3.Runtime).
- **WAP-specific constructs to preserve/verify post-conversion**:
  - `Import` of `Microsoft.WebApplication.targets` (needed for Web.config transform packaging / `MvcBuildViews` target) — the SDK-style conversion tool is expected to retain or provide an equivalent; must confirm import survives.
  - `MvcBuildViews` custom target (`AfterTargets="AfterBuild"`) — kept as-is, no view-compile is currently enabled (`MvcBuildViews=false`).
  - `CopySQLClientNativeBinaries` custom target — kept as-is but paths depend on `packages\` folder existing; must verify after PackageReference migration (may need path update to NuGet cache, flagged as a Common Issue watch item).
  - Content globbing: legacy project explicitly lists every `Views\*.cshtml`, `Content\*.css`, `Scripts\*.js` file in `<Content>`/`<None>` ItemGroups. SDK-style implicit globbing will pick these up automatically — after conversion, check for the tool's "excluded from globbing" label on any ItemGroup and present any flagged files to the user before removing (per skill's Common Issues guidance).
  - `Web.config`, `Web.Debug.config`, `Web.Release.config` (config transforms) and `Views\Web.config` are already explicit `<Content>` items — SDK-style default globbing includes `*.config` as content automatically; confirm no duplication/build warning after conversion.
  - No `Properties\PublishProfiles` folder exists in this project (confirmed via directory listing) — nothing to migrate there.
  - `ProjectExtensions`/`FlavorProperties` (IIS Express dev server settings) — VS-only metadata, expected to be preserved or safely dropped by the conversion tool since it doesn't affect command-line build.
- **Target framework**: staying on `net48` (`TargetFrameworkVersion=v4.8` → `TargetFramework=net48` mapping only, per hard constraint — no TFM upgrade in this task).
- **Build tool**: legacy (non-SDK) project → per `building-projects` skill decision guide, must use `msbuild.exe` before conversion. After conversion to SDK-style targeting `net48`, still use `msbuild.exe` (net4xx TFM, no `Microsoft.NETFramework.ReferenceAssemblies` reference assemblies package present) rather than `dotnet build`, until/unless that package is added.
- **Approach**: use the dedicated `convert_project_to_sdk_style` MCP tool (per `converting-to-sdk-style` skill — no manual XML rewrite), one project only, then build directly against the project (not solution) and verify `packages.config` is removed.
