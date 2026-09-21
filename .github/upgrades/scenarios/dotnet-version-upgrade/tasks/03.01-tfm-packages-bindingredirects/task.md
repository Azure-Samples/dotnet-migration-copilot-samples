# 03.01-tfm-packages-bindingredirects: Retarget csproj to net10.0, resolve 37 package issues, document binding redirects

## Objective
Retarget `ContosoUniversity.csproj` to `net10.0` and resolve all package-level issues (NuGet.0001/0002/0003/0004, 39 total incl. Project.0001/0002) plus document the 12 binding-redirect issues (Binding.0006 x6 mandatory, Binding.0007 x6 potential). This is the foundation gate — nothing else in the parent task can proceed until the project restores and the TFM is correct.

## Scope
`ContosoUniversity.csproj` only. Do NOT touch any `.cs`/`.cshtml` files — API-surface (`Api.0001`/`Api.0002`) and feature (`Feature.*`) issues are handled by later sibling subtasks. `Web.config` only for the binding-redirect documentation step (read-only investigation, edits only if a redirect is confirmed truly obsolete).

## Steps
1. Change `<TargetFramework>` from `net48` to `net10.0`.
2. Remove framework-included packages (net10.0 already provides them): `Microsoft.AspNet.Mvc`, `Microsoft.AspNet.Razor`, `Microsoft.AspNet.WebPages`, `Microsoft.AspNet.Web.Optimization`, `Microsoft.CodeDom.Providers.DotNetCompilerPlatform`, `Microsoft.Web.Infrastructure`, `NETStandard.Library`, and the `System.*` BCL packages flagged `NuGet.0003` in [assessment/projects/ContosoUniversity.md](../../../assessment/projects/ContosoUniversity.md#nuget-package-issues) — verify each against the assessment table before removing, do not guess.
3. Replace the one true incompatible package: `Antlr` → `Antlr4` (research the correct net10.0-compatible version via `get_supported_package_version`; this is the `large-package-replacement-batch` hint item).
4. Upgrade the remaining 24 `NuGet.0002` packages (`Microsoft.EntityFrameworkCore.*`, `Microsoft.Extensions.*`, `Microsoft.Data.SqlClient`, `Microsoft.Bcl.*`, `Newtonsoft.Json`, remaining `System.*`) to net10.0-compatible versions.
5. Add a `Microsoft.AspNetCore.SystemWebAdapters` package reference at a net10.0-compatible version (resolve the version here; the actual wiring/configuration happens in subtask 03.02 — do not configure it in this subtask).
6. Investigate `Web.config` for `<runtime><assemblyBinding>` entries. For each of the 6 mandatory (manual-redirect-conflicts-with-auto-generated) and 6 potential (forces-downgrade) issues on `Newtonsoft.Json`, `System.Threading.Tasks.Extensions`, `System.ComponentModel.Annotations`, `System.Runtime.CompilerServices.Unsafe`, `System.Memory`, `Microsoft.Data.SqlClient`: confirm current state, and record in a short "Binding Redirect Findings" note (append to this task's own notes, e.g. a `binding-redirects.md` alongside this task.md) whether each entry is obsolete under SDK-style/net10.0 (expected for most) or masks a real runtime dependency that must be preserved. Remove only entries confirmed obsolete.
7. Run `dotnet restore` then `dotnet build`. Expect build errors to remain — they should be limited to `System.Web.Mvc`/`System.Messaging`/`ConfigurationManager` API surface (handled by later subtasks). Do not attempt to fix those errors here; confirm no *package resolution* errors remain (all `PackageReference`s resolve cleanly for `net10.0`).

## Relevant skills
modifying-project-properties, managing-package-references, building-projects.

## Done when
- `ContosoUniversity.csproj` targets `net10.0`.
- All 39 NuGet/Project issues are resolved (removed, replaced, or upgraded) — `dotnet restore` succeeds with no incompatible/missing package errors.
- `Antlr4` reference resolves and replaces `Antlr`.
- `Microsoft.AspNetCore.SystemWebAdapters` reference is added (version only, unconfigured).
- All 12 binding-redirect issues are documented with an explicit removal/keep recommendation per entry.
- Remaining `dotnet build` errors are only API-surface errors (`System.Web.*`, `System.Messaging`, `ConfigurationManager`) deferred to sibling subtasks.

## Research Findings (pre-execution)

Single project, no CPM (`get_project_dependencies` confirms all `PackageReference`s are defined directly in `ContosoUniversity.csproj`; no `Directory.Packages.props`). Standard-mode package edits apply.

**Removals (12, framework-included/incompatible per [assessment/projects/ContosoUniversity.md](../../../assessment/projects/ContosoUniversity.md)):**
`Microsoft.AspNet.Mvc`, `Microsoft.AspNet.Razor`, `Microsoft.AspNet.WebPages`, `Microsoft.AspNet.Web.Optimization`, `Microsoft.CodeDom.Providers.DotNetCompilerPlatform`, `Microsoft.Web.Infrastructure`, `NETStandard.Library`, `System.Buffers`, `System.ComponentModel.Annotations`, `System.Memory`, `System.Numerics.Vectors`, `System.Threading.Tasks.Extensions`.

**Replace (1):** `Antlr` 3.4.1.9004 → `Antlr4` **4.6.6** (confirmed via `get_supported_package_version` for net10.0).

**Upgrade (24, versions confirmed via assessment + spot-checked with `get_supported_package_version`):**
`Microsoft.Bcl.AsyncInterfaces`→10.0.12, `Microsoft.Bcl.HashCode`→6.0.0, `Microsoft.Data.SqlClient`→7.1.0 (spot-checked), `Microsoft.EntityFrameworkCore`→10.0.12, `.Abstractions`→10.0.12, `.Analyzers`→10.0.12, `.Relational`→10.0.12, `.SqlServer`→10.0.12 (spot-checked), `.Tools`→10.0.12, `Microsoft.Extensions.Caching.Abstractions`→10.0.12, `.Caching.Memory`→10.0.12, `.Configuration`→10.0.12 (spot-checked), `.Configuration.Abstractions`→10.0.12, `.Configuration.Binder`→10.0.12, `.DependencyInjection`→10.0.12, `.DependencyInjection.Abstractions`→10.0.12, `.Logging`→10.0.12, `.Logging.Abstractions`→10.0.12, `.Options`→10.0.12, `.Primitives`→10.0.12, `Newtonsoft.Json`→13.0.4 (spot-checked), `System.Collections.Immutable`→10.0.12, `System.Diagnostics.DiagnosticSource`→10.0.12, `System.Runtime.CompilerServices.Unsafe`→6.1.2 (spot-checked).

**Add:** `Microsoft.AspNetCore.SystemWebAdapters` **2.3.0** (confirmed via `get_supported_package_version`), version-only reference — no `web.config`/`Startup` wiring here (subtask 03.02).

**Untouched (compatible, not in the 37-issue list):** `bootstrap`, `jQuery`, `jQuery.Validation`, `Microsoft.Data.SqlClient.SNI.runtime`, `Microsoft.Identity.Client`, `Microsoft.jQuery.Unobtrusive.Validation`, `Modernizr`, `WebGrease`. Note: the `CopySQLClientNativeBinaries` target references the legacy `packages\Microsoft.Data.SqlClient.SNI.runtime.2.1.1\...` path — out of scope for this task (not a listed package issue); left as-is, flagged as a build risk in progress-details.md.

**Binding redirects (12, in `Web.config`):** all 6 mandatory (MSB3836 conflicts) and 6 potential (forced downgrades) entries target packages that are being upgraded in this task (`Newtonsoft.Json`, `System.Threading.Tasks.Extensions`, `System.ComponentModel.Annotations`, `System.Runtime.CompilerServices.Unsafe`, `System.Memory`, `Microsoft.Data.SqlClient`) plus several `System.Web.*`/WebPages/MVC/WebGrease/Antlr/Microsoft.Web.Infrastructure entries tied to packages being removed/replaced. Under SDK-style + `net10.0`, `<runtime><assemblyBinding>` is meaningless (no .NET Framework binding-redirect model) — full findings recorded in `binding-redirects.md` alongside this file.
