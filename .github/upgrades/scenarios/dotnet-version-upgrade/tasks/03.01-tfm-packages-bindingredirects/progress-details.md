# Progress Details — 03.01-tfm-packages-bindingredirects

## Files modified
- `ContosoUniversity/ContosoUniversity.csproj`
- `ContosoUniversity/Web.config`

## What was done

1. **TFM**: `<TargetFramework>` changed `net48` → `net10.0`.
2. **Removed (12, framework-included under net10.0 per assessment)**: `Microsoft.AspNet.Mvc`,
   `Microsoft.AspNet.Razor`, `Microsoft.AspNet.WebPages`, `Microsoft.AspNet.Web.Optimization`,
   `Microsoft.CodeDom.Providers.DotNetCompilerPlatform`, `Microsoft.Web.Infrastructure`,
   `NETStandard.Library`, `System.Buffers`, `System.ComponentModel.Annotations`,
   `System.Memory`, `System.Numerics.Vectors`, `System.Threading.Tasks.Extensions`.
3. **Replaced**: `Antlr` 3.4.1.9004 → `Antlr4` 4.6.6 (version confirmed via
   `get_supported_package_version`).
4. **Upgraded (24 → versions confirmed via assessment + spot-checked via
   `get_supported_package_version`)**: all `Microsoft.EntityFrameworkCore*`,
   `Microsoft.Extensions.*`, `Microsoft.Data.SqlClient` (2.1.4→7.1.0), `Microsoft.Bcl.*`,
   `Newtonsoft.Json` (13.0.3→13.0.4), `System.Collections.Immutable`,
   `System.Diagnostics.DiagnosticSource`, `System.Runtime.CompilerServices.Unsafe` — see the
   per-package version table added to `task.md`'s Research Findings section.
5. **Added**: `Microsoft.AspNetCore.SystemWebAdapters` 2.3.0 (version-only reference; wiring
   deferred to subtask 03.02 per the task).
6. **Binding redirects**: investigated all 12 flagged issues (6 `<dependentAssembly>` entries)
   in `Web.config`. Full per-entry findings recorded in `binding-redirects.md` alongside this
   file. Verdict: all 6 confirmed obsolete (`<runtime><assemblyBinding>` has no effect once the
   project targets `net10.0` — the CLR feature it depends on doesn't exist there — and 3 of the
   6 redirect assemblies whose packages were removed outright in step 2). All 6 removed from
   `Web.config`. The remaining, non-flagged `<dependentAssembly>` entries (for packages this task
   also removes/replaces/upgrades, e.g. MVC/WebPages/WebGrease/Antlr/Web.Infrastructure) were left
   untouched — out of the 12-issue scope for this task; noted as a follow-up cleanup candidate.
7. **Restore/build validation** (`dotnet build ContosoUniversity.csproj`) surfaced two package
   issues not captured in the static assessment, both fixed as they are direct, blocking
   consequences of the packages upgraded/removed in steps 2–4 (not new scope, just discovered by
   running restore, exactly as task step 7 calls for):
   - **NU1605 (blocking error)**: the explicit `Microsoft.Data.SqlClient.SNI.runtime` 2.1.1 pin
     downgrade-conflicted with the SqlClient 7.1.0 upgrade (which transitively requires
     `SNI.runtime` ≥ 7.1.0). Fix: removed the explicit `PackageReference` — it now flows
     transitively from the upgraded `Microsoft.Data.SqlClient`. Also removed the
     `CopySQLClientNativeBinaries` custom MSBuild target, which hardcoded copy paths into the old
     `packages\Microsoft.Data.SqlClient.SNI.runtime.2.1.1\...` folder — obsolete once that pinned
     version is gone; modern SqlClient resolves native assets via the NuGet runtime graph instead.
   - **NU1510 (56 warnings)**: 14 of the upgraded packages (`Microsoft.Extensions.Caching.*`,
     `Microsoft.Extensions.Configuration*`, `Microsoft.Extensions.DependencyInjection*`,
     `Microsoft.Extensions.Logging*`, `Microsoft.Extensions.Options`,
     `Microsoft.Extensions.Primitives`, `System.Collections.Immutable`,
     `System.Diagnostics.DiagnosticSource`, `System.Runtime.CompilerServices.Unsafe`) are now
     provided automatically by the net10.0 SDK once bumped to the 10.0.12/matching version —
     NuGet flagged them "will not be pruned... remove the PackageReference item". Removed all 14
     `PackageReference` entries; they resolve implicitly via the SDK's shared framework.

## Build result (self-check)

`dotnet build ContosoUniversity.csproj`:
- **Restore**: clean — no `NU1xxx` errors.
- **Build**: **FAILED** (expected/acceptable per task) — 276 errors, all `CS0246`/`CS0234`
  (missing type/namespace), 100% attributable to `System.Web.Mvc`, `System.Web.Routing`,
  `System.Web.Optimization`, and `System.Messaging` — the API-surface work explicitly deferred to
  sibling subtasks (03.02 SystemWebAdapters wiring, 03.04 MSMQ replacement, 03.05–03.10 controller
  migrations). No `NU1xxx` or unexpected `MSBxxxx`/`NETSDKxxxx` errors remain.
- **Warnings**: `MSB3245`/`MSB3243` (legacy `<Reference Include="System.Web" />` etc. items at the
  top of the csproj don't resolve under net10.0) and `NU1701` (×8, `WebGrease` restored via its
  .NET Framework compat shim — pre-existing/compatible per assessment, not one of the 37 issues).
  These are consequences of the TFM change tied to the same deferred System.Web/System.Messaging
  API surface — left as-is per the task's explicit scope boundary (do not touch `.cs`/`.cshtml`
  or the System.Web reference/adapter wiring here).

## Deviations from task.md
None in scope/approach. Two additional, in-scope fixes (SNI.runtime pin + native-copy target,
14 SDK-pruned `Microsoft.Extensions.*`/`System.*` packages) were required beyond the assessment's
static 37-issue list, surfaced only by actually running restore/build as task step 7 requires —
documented above and in `task.md`.
