# Progress Details — 03.02-systemweb-adapters-scaffolding

## Summary
Scaffolded the System.Web Adapters hosting bridge for the net10.0 in-place rewrite: added
`Program.cs` (ASP.NET Core minimal hosting), ported `Global.asax.cs` startup logic into it,
migrated `BaseController` to `Microsoft.AspNetCore.Mvc.Controller`, and replaced
`System.Web.Optimization` bundling with a `wwwroot/` + `app.UseStaticFiles()` + direct
`<script>`/`<link>` tag approach.

## Files changed
- **Added**: `Program.cs`, `Views/_ViewImports.cshtml`
- **Deleted**: `Global.asax`, `Global.asax.cs`, `App_Start/FilterConfig.cs`,
  `App_Start/RouteConfig.cs`, `App_Start/BundleConfig.cs`
- **Modified**: `ContosoUniversity.csproj` (removed `<OutputType>Library</OutputType>` override,
  removed the `Global.asax.cs`/`DependentUpon` item, removed stale `Content`/`None` items for
  files that moved under `wwwroot/`), `Controllers/BaseController.cs` (base type switch),
  `Views/Shared/_Layout.cshtml` (all 4 bundle calls replaced), 8 flagged views (`jqueryval`
  bundle call replaced): `Views/Courses/Create.cshtml`, `Views/Courses/Edit.cshtml`,
  `Views/Departments/Create.cshtml`, `Views/Departments/Edit.cshtml`,
  `Views/Instructors/Create.cshtml`, `Views/Instructors/Edit.cshtml`,
  `Views/Students/Create.cshtml`, `Views/Students/Edit.cshtml`
- **Moved** (git-tracked renames, contents unchanged): `Content/` → `wwwroot/Content/`,
  `Scripts/` → `wwwroot/Scripts/` (`favicon.ico` referenced in the old csproj never existed on
  disk — dropped the dangling item, nothing to move)

## What was done (by task step)
1. **Program.cs / hosting**: `builder.Services.AddSystemWebAdapters()` +
   `app.UseSystemWebAdapters()` wired up (per the package's documented `AddSystemWebAdapters`/
   `UseSystemWebAdapters` API — in-process only, no remote-app config needed since this is an
   in-place rewrite, not a split old/new host). Also added `AddControllersWithViews()` +
   `MapControllerRoute` since `Microsoft.AspNetCore.SystemWebAdapters` only bridges
   `HttpContext`/`Session`/`Server`-family types (confirmed via package README/lib inspection) —
   it does not provide a `System.Web.Mvc.Controller` shim, so the MVC layer itself now runs on
   `Microsoft.AspNetCore.Mvc`.
2. **Global.asax.cs → Program.cs**: `AreaRegistration.RegisterAllAreas()` dropped (no `Areas/`
   folder exists — nothing to port). `FilterConfig`/`RouteConfig` replaced by
   `app.UseExceptionHandler("/Home/Error")` (non-dev) + `app.MapControllerRoute(...)` matching the
   original `{controller}/{action}/{id}` default route (the `{resource}.axd` ignore-route was
   dropped — no `.axd` handlers exist under ASP.NET Core hosting). `FilterConfig.cs`/
   `RouteConfig.cs` deleted.
3. **BaseController**: `using System.Web.Mvc;` → `using Microsoft.AspNetCore.Mvc;`. No other
   changes needed — the class only referenced the `Controller` base type, not other
   `System.Web.Mvc` APIs; `protected override void Dispose(bool disposing)` still overrides
   correctly since `Microsoft.AspNetCore.Mvc.Controller` also implements `IDisposable` via a
   virtual `Dispose(bool)`.
4. **Bundling → wwwroot**: `Content/` and `Scripts/` moved to `wwwroot/Content` and
   `wwwroot/Scripts` (`git mv`, no content changes). `~/bundles/jquery` → `~/Scripts/jquery-3.4.1.js`;
   `~/bundles/jqueryval` → `~/Scripts/jquery.validate.js` + `~/Scripts/jquery.validate.unobtrusive.js`;
   `~/bundles/modernizr` → `~/Scripts/modernizr-2.6.2.js`; `~/bundles/bootstrap` →
   `~/Scripts/bootstrap.js` + `~/Scripts/respond.js` (all physical, pre-existing files, just
   relocated). `~/Content/css` (bootstrap.css + Site.css) → `~/css/bootstrap.min.css` (physical
   `Content/bootstrap.css` never existed on disk — a pre-existing dangling csproj entry; the
   `bootstrap` 5.3.3 PackageReference's `contentFiles` auto-copy to `wwwroot/css/bootstrap.min.css`
   on restore is the correct replacement source) + `~/Content/Site.css` (physical, moved).
   `BundleConfig.cs` deleted. Added `app.UseStaticFiles()`.
5. **`Views/_ViewImports.cshtml` (new)**: registers `Microsoft.AspNetCore.Mvc.TagHelpers` — without
   it, the `~/`-prefixed `href`/`src` attributes in the replaced `<link>`/`<script>` tags would not
   resolve to the correct URLs at runtime under ASP.NET Core's Razor view engine (this repo had no
   `_ViewImports.cshtml` at all previously; the legacy `Views/Web.config` MVC5 Razor-host
   configuration is now inert under ASP.NET Core hosting and was left untouched, out of scope).
6. **ConfigurationManager marker**: the `ConfigurationManager.ConnectionStrings["DefaultConnection"]`
   read was moved verbatim into `Program.cs`'s `InitializeDatabase()` local function with a
   `// TODO(03.03)` marker comment, per the task's explicit instruction not to remove/fix it here.
7. **csproj**: removed `<OutputType>Library</OutputType>` (added in 03.01 only because there was
   no entry point yet) so the SDK's normal `Exe` default for `Microsoft.NET.Sdk.Web` +
   `Program.cs` applies. Removed the `Global.asax.cs`/`DependentUpon` compile item and the
   `Content`/`None` items pointing at the old `Content\*`/`Scripts\*`/`favicon.ico` paths (files
   under `wwwroot/` are auto-included by the Web SDK's implicit glob, no explicit items needed).

## Build/self-check result
`dotnet build ContosoUniversity.csproj` — errors are now confined to files explicitly **out of
this task's scope** (per the task description, owned by 03.04–03.10):
`Controllers/StudentsController.cs`, `Controllers/DepartmentsController.cs`,
`Controllers/CoursesController.cs`, `Controllers/InstructorsController.cs`,
`Controllers/NotificationsController.cs`, `Controllers/HomeController.cs`,
`Services/NotificationService.cs`, and the two Razor views whose errors cascade from those
controllers' broken model/API surface (`Views/Students/Index.cshtml`, `Views/Shared/Error.cshtml`).
**Zero errors or warnings** in `Program.cs`, `BaseController.cs`, or any of the deleted/replaced
`Global.asax.cs`/`FilterConfig.cs`/`RouteConfig.cs`/`BundleConfig.cs` files (confirmed — they no
longer appear anywhere in the build output).

One pre-existing warning newly surfaced in `HomeController.cs` (`CS0114`: `Unauthorized()` hides
`ControllerBase.Unauthorized()`) — a direct, expected consequence of `BaseController` now
inheriting from `Microsoft.AspNetCore.Mvc.Controller` (which defines its own `Unauthorized()`
helper). `HomeController.cs` itself is still on `System.Web.Mvc`/broken and unrelated to this
task's scope; flagging for whichever subtask migrates `HomeController`.

No new MSBuild reference-conflict warnings (`MSB3243`/`MSB3245` for `System.Web`,
`System.Configuration`, etc.) were introduced — those come from the pre-existing `<Reference>`
item group in the csproj, which this task did not touch.

## Deviations from task.md
None of substance. Two additions beyond the literal step list, both necessary for the scaffolded
pipeline to actually function (not just compile): (1) `AddControllersWithViews()` +
`MapControllerRoute` in `Program.cs`, since the adapters package alone doesn't provide MVC
controller activation; (2) `Views/_ViewImports.cshtml` to register tag helpers so `~/`-prefixed
asset paths resolve under ASP.NET Core's Razor engine.

## Decomposition assessment
Loaded the scenario's `execution.md` (Execution stage) and `breakdown-hints/framework-web-migration.md`.
No `// STUB:` markers found in scope. The `web-bundling-and-static-assets` hint (SHOULD priority)
recommends separating asset-pipeline work from Razor view migration — not applicable here since
the only view touches in this task are the bundling-reference swap itself (no broader view
migration is happening in this task). `web-controller-migration-units` doesn't apply — controller
body/API migration is explicitly deferred to 03.05–03.10. Verdict: **atomic** — executed directly.
