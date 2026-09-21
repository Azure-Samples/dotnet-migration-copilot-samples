# .NET Version Upgrade Plan

## Overview

**Target**: ContosoUniversity.csproj — ASP.NET MVC 5 Web Application Project (WAP), currently `net48`, non-SDK-style, `packages.config` — upgraded in place to `net10.0`.
**Scope**: 1 project, ~3.4k LOC, 569+ LOC estimated to change (≥16.8% of the codebase), 636 total issues (532 binary-incompatible / 37 source-incompatible APIs, 37 package issues, 12 binding-redirect issues).

## Upgrade Options

| Option | Selected | Why |
|--------|----------|-----|
| Upgrade Strategy | All-at-Once | Single project — no dependency graph to phase across. |
| Project Approach (Web Project) | In-place rewrite | Single WAP project; side-by-side scaffolding is unnecessary overhead here. |
| Unsupported Packages | Resolve Inline | 37 package issues, mostly framework-included or straightforward version bumps — researched and fixed within the upgrade task. |
| Unsupported API Handling | Fix Inline | Resolve all 569 API issues (System.Web MVC, MSMQ, config) directly, no stubs. |
| System.Web Adapters | Use System.Web Adapters | Bridges `System.Web`-dependent MVC surface (495 API hits, 87% of issues) onto ASP.NET Core hosting. |
| Assembly Binding Redirects | Document and Review Before Removing | 12 binding-redirect issues (6 mandatory conflicts, 6 potential downgrades) recorded for review rather than blind removal. |
| Nullable Reference Types | Leave Disabled | No change to nullable annotations during this upgrade. |
| Test Coverage | Skip | No behavior-locking test baseline generated before upgrading. |

### Selected Strategy
**All-At-Once** — The single project is upgraded in one pass.
**Rationale**: 1 project total, no dependency graph to manage — the .NET Framework override rule for single-project solutions selects All-at-Once even though the source is `net48`.

## Tasks

### 01-prerequisites: Verify toolchain and target SDK

Confirm the .NET 10 SDK is installed and available to the build, and that no `global.json` pins an incompatible SDK version. Record the current project state (non-SDK-style WAP, `packages.config`, `net48`) as the starting point for the conversion and upgrade tasks that follow.

**Done when**: .NET 10 SDK is confirmed installed and any `global.json` is compatible with `net10.0`, or updated to be.

---

### 02-sdk-style-conversion: Convert ContosoUniversity.csproj to SDK-style

Convert the non-SDK-style `ContosoUniversity.csproj` (WAP, `packages.config`) to SDK-style format while remaining on `net48`. This is a structural change only — it must not be combined with the TFM upgrade in task 03, since SDK conversion and TFM/API changes have different failure modes. Conversion migrates `packages.config` references to `PackageReference` as part of the same step.

Watch for WAP-specific MSBuild constructs (`Microsoft.WebApplication.targets`, `Views` content globbing, `Web.config` transforms, `PublishProfiles`) that need SDK-style equivalents, and confirm the project still builds on `net48` after conversion before proceeding.

**Done when**: `ContosoUniversity.csproj` is SDK-style, uses `PackageReference` for all dependencies, still targets `net48`, and the project builds successfully.

---

### 03-upgrade-web-project: Upgrade ContosoUniversity to net10.0

Retarget `ContosoUniversity.csproj` to `net10.0` and resolve all resulting package and API issues in the same pass (Resolve Inline / Fix Inline — no stubs, no deferred work).

**Package work** (37 issues): several packages (`Microsoft.AspNet.Mvc`, `Microsoft.AspNet.Razor`, `Microsoft.AspNet.WebPages`, `Microsoft.AspNet.Web.Optimization`, `Microsoft.CodeDom.Providers.DotNetCompilerPlatform`, `Microsoft.Web.Infrastructure`, `NETStandard.Library`, and several `System.*` BCL packages) are framework-included on `net10.0` and should be removed rather than upgraded; `Antlr` needs replacing with `Antlr4`; the `Microsoft.EntityFrameworkCore.*` family, `Microsoft.Extensions.*` family, `Microsoft.Data.SqlClient`, `Microsoft.Bcl.*`, `Newtonsoft.Json`, and remaining `System.*` packages upgrade to their `net10.0`-compatible versions per [the project's package table](assessment/projects/ContosoUniversity.md#nuget-package-issues).

**API work** (569 issues, dominated by `System.Web.*`): the ASP.NET MVC 5 surface (`ViewResult`, `ActionResult`, `Controller.View`, `ModelState`, `ModelStateDictionary`, `ViewBag`, `SelectList`, `RedirectToAction`, etc. — 87% of all API issues) has no direct .NET 10 equivalent; introduce the **System.Web Adapters** package to bridge the remaining `System.Web`-dependent code onto ASP.NET Core hosting rather than a full architectural rewrite of the controller layer. `System.Messaging.MessageQueue` (MSMQ, 57 issues / 10%) has no .NET 10 API — replace usage with a supported message-queue client (e.g., an Azure Service Bus or RabbitMQ SDK) or isolate it behind an abstraction if a queue migration is out of scope for this pass; flag any MSMQ code where the concrete queue replacement needs a follow-up decision. Legacy `app.config`/`Web.config`-based configuration reads (16 issues) move to `System.Configuration.ConfigurationManager` (as an interim bridge) or `Microsoft.Extensions.Configuration` where practical.

**Binding redirects**: 12 issues exist today (6 mandatory manual-redirect/auto-generation conflicts for `Newtonsoft.Json`, `System.Threading.Tasks.Extensions`, `System.ComponentModel.Annotations`, `System.Runtime.CompilerServices.Unsafe`, `System.Memory`, `Microsoft.Data.SqlClient`; 6 potential downgrade issues on the same set). Per the confirmed option, document each existing redirect and its conflict/downgrade status before deciding whether to remove it — SDK-style + `net10.0` projects do not use `bindingRedirect`/`assemblyBinding` the same way as .NET Framework, so most existing entries are expected to become obsolete, but confirm none masks a real runtime dependency before deleting.

**Done when**: `ContosoUniversity.csproj` targets `net10.0`, all package references are resolved (upgraded, replaced, or removed as framework-included), all flagged API issues are fixed inline (including System.Web Adapters integration and MSMQ replacement/isolation), binding redirects are documented with a removal recommendation per entry, and the project builds with 0 errors.

---

### 04-final-validation: Build, smoke-test, and document follow-ups

Perform the full solution build after task 03 completes and confirm 0 build errors. Since Test Coverage was skipped, no automated regression suite exists — perform a basic manual/smoke check of core MVC flows (home, courses, students, instructors, departments, notifications) if feasible.

Document any deferred recommendations surfaced during the upgrade: the MSMQ replacement decision (if isolated rather than fully replaced), any binding-redirect entries that were kept rather than removed and why, and any System.Web Adapters limitations encountered.

**Done when**: The solution builds with 0 errors on `net10.0`, and deferred recommendations (MSMQ, binding redirects, adapter limitations) are documented for the user.
