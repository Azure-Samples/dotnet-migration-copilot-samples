# Binding Redirect Findings — 03.01-tfm-packages-bindingredirects

Investigation of the 12 flagged binding-redirect issues (6 mandatory + 6 potential, across 6
`<dependentAssembly>` entries) in [Web.config](../../../../../ContosoUniversity/Web.config)
per the "Document and Review Before Removing" strategy.

## Context

`<runtime><assemblyBinding>` / `<bindingRedirect>` is a .NET Framework CLR assembly-loading
feature. It has no equivalent in .NET (Core) 5+ / `net10.0` — the runtime does not read or act
on these elements at all under the SDK-style project + `net10.0` TFM this task establishes.
That makes every entry in this section structurally obsolete the moment the TFM changes,
regardless of the version numbers it references.

## Per-entry findings

| Assembly | Old redirect (`oldVersion` → `newVersion`) | Mandatory issue | Potential issue | Current package version (this task) | Recommendation |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `Newtonsoft.Json` | `0.0.0.0-13.0.0.0` → `13.0.0.0` | Manual redirect (13.0.0.0) conflicts with auto-generated version (13.0.3) — MSB3836 | Redirect forces downgrade to 13.0.0.0 vs package-provided 13.0.3 | 13.0.4 | **Obsolete — remove.** Redirect target (13.0.0.0) is already stale even under net48; under net10.0 the element is inert regardless. |
| `System.Threading.Tasks.Extensions` | `0.0.0.0-4.2.0.1` → `4.2.0.1` | Manual redirect (4.2.0.1) conflicts with auto-generated (4.5.4) — MSB3836 | Redirect forces downgrade to 4.2.0.1 vs package-provided 4.5.4 | Package removed this task (framework-included in net10.0) | **Obsolete — remove.** Package reference itself is gone; no assembly to redirect. |
| `System.ComponentModel.Annotations` | `0.0.0.0-4.2.1.0` → `4.2.1.0` | Manual redirect (4.2.1.0) conflicts with auto-generated (4.7.0) — MSB3836 | Redirect forces downgrade to 4.2.1.0 vs package-provided 4.7.0 | Package removed this task (framework-included in net10.0) | **Obsolete — remove.** Package reference itself is gone; no assembly to redirect. |
| `System.Runtime.CompilerServices.Unsafe` | `0.0.0.0-4.0.6.0` → `4.0.6.0` | Manual redirect (4.0.6.0) conflicts with auto-generated (4.5.3) — MSB3836 | Redirect forces downgrade to 4.0.6.0 vs package-provided 4.5.3 | 6.1.2 | **Obsolete — remove.** Redirect target predates even the pre-upgrade package version; under net10.0 it is also inert. |
| `System.Memory` | `0.0.0.0-4.0.1.1` → `4.0.1.1` | Manual redirect (4.0.1.1) conflicts with auto-generated (4.5.4) — MSB3836 | Redirect forces downgrade to 4.0.1.1 vs package-provided 4.5.4 | Package removed this task (framework-included in net10.0) | **Obsolete — remove.** Package reference itself is gone; no assembly to redirect. |
| `Microsoft.Data.SqlClient` | `0.0.0.0-2.0.20168.4` → `2.0.20168.4` | Manual redirect (2.0.20168.4) conflicts with auto-generated (2.1.4) — MSB3836 | Redirect forces downgrade to 2.0.20168.4 vs package-provided 2.1.4 | 7.1.0 | **Obsolete — remove.** Redirect target is 5 major versions behind the upgraded package; under net10.0 it is also inert. |

## Verdict

All 12 issues (6 `<dependentAssembly>` elements) are confirmed obsolete — none masks a real
runtime dependency that must be preserved:
- Three (`System.Threading.Tasks.Extensions`, `System.ComponentModel.Annotations`, `System.Memory`)
  redirect assemblies whose packages are removed outright in this task (net10.0-framework-included).
- The other three (`Newtonsoft.Json`, `System.Runtime.CompilerServices.Unsafe`,
  `Microsoft.Data.SqlClient`) redirect to versions already stale relative to the package versions
  this task installs, and in any case `<bindingRedirect>` has no effect once the project targets
  `net10.0` (non-.NET-Framework) — the CLR feature it depends on does not exist there.

All 6 `<dependentAssembly>` elements covering these 12 issues were removed from `Web.config`.

## Out of scope (not among the 12 flagged issues, left untouched)

`Web.config`'s `<assemblyBinding>` also contains entries for `Microsoft.Web.Infrastructure`,
`Antlr3.Runtime`, `System.Web.Optimization`, `WebGrease`, `System.Web.Helpers`,
`System.Web.WebPages`, `System.Web.Mvc`, `Microsoft.Extensions.DependencyInjection[.Abstractions]`,
`Microsoft.EntityFrameworkCore.Abstractions`, `Microsoft.Extensions.Caching.Abstractions`,
`Microsoft.Extensions.Configuration.Abstractions`, `Microsoft.Extensions.Logging.Abstractions`,
`Microsoft.Extensions.Options`, `Microsoft.Extensions.Primitives`, and `netstandard`. These were
not among the assessment's 12 flagged binding-redirect issues, so per this task's scope they were
left in place. They are equally inert under `net10.0` and reference packages this task removes,
replaces, or upgrades — cleanup of the remaining `<assemblyBinding>` block (or removal of the
whole block, since none of it functions on `net10.0`) should be picked up by a sibling/follow-up
task rather than expanded here.
