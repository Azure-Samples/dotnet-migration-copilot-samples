# 03.02-systemweb-adapters-scaffolding: Wire up System.Web Adapters hosting, migrate Global.asax/App_Start, BaseController, and bundling/static assets

## Objective
Stand up the System.Web Adapters bridge so the existing System.Web-based MVC surface can host on net10.0 in place (per the confirmed "Use System.Web Adapters" option — this is NOT a rewrite to ASP.NET Core MVC). Migrate the shared `BaseController` (used by all 7 controllers — must land before any per-controller subtask), convert `Global.asax.cs` application startup (`Feature.1000`), `App_Start/FilterConfig.cs` (`GlobalFilterCollection` → middleware, `Feature.0003`) and `App_Start/RouteConfig.cs` (`RouteCollection` → route mappings, `Feature.0002`), and replace `System.Web.Optimization` bundling (`App_Start/BundleConfig.cs`, `Feature.0001`) with a wwwroot/static-file-middleware + direct `<script>`/`<link>` tag approach across the 9 flagged views (`Views/Courses/Create.cshtml`, `Views/Courses/Edit.cshtml`, `Views/Departments/Create.cshtml`, `Views/Departments/Edit.cshtml`, `Views/Instructors/Create.cshtml`, `Views/Instructors/Edit.cshtml`, `Views/Shared/_Layout.cshtml`, `Views/Students/Create.cshtml`, `Views/Students/Edit.cshtml`).

This subtask must run after 03.01 (needs the project restoring/building on net10.0 with the `Microsoft.AspNetCore.SystemWebAdapters` package present) and before every per-controller subtask (03.05–03.10), which all depend on `BaseController` compiling and the hosting pipeline being in place.

## Scope
`Global.asax.cs`, `App_Start/FilterConfig.cs`, `App_Start/RouteConfig.cs`, `App_Start/BundleConfig.cs`, `Controllers/BaseController.cs`, a new `Program.cs` (net10.0 minimal-hosting entry point), `wwwroot/` setup, and the 9 views listed above (bundling references only — do not touch other controller-specific markup in those views).
Do NOT touch the `ConfigurationManager.ConnectionStrings` read that may remain in `Global.asax.cs` for connection-string bootstrapping — leave a clear marker/comment for subtask 03.03 to pick up after this subtask restructures the file, and do not remove that read yourself.

## Steps
1. Configure `Microsoft.AspNetCore.SystemWebAdapters` hosting: create `Program.cs` with the ASP.NET Core minimal-hosting model, register `services.AddSystemWebAdapters()` / `app.UseSystemWebAdapters()` (and any remote-app or in-process adapter set required — research current package docs for the exact API surface at the resolved version), so `HttpContext.Current`, `Session`, and MVC-5-style controller activation continue to work for the code migrated in later subtasks.
2. Port `Global.asax.cs` `Application_Start` logic (`Feature.1000`) into `Program.cs`: remove `GlobalFilterCollection` registration in favor of middleware/filter registration on the application builder (`Feature.0003`, replaces `App_Start/FilterConfig.cs`), and remove `RouteCollection` registration in favor of endpoint/route mapping on the application object (`Feature.0002`, replaces `App_Start/RouteConfig.cs`). Delete `FilterConfig.cs`/`RouteConfig.cs` once their logic is fully ported.
3. Migrate `Controllers/BaseController.cs` (2 `Api.0001` issues on `System.Web.Mvc.Controller.#ctor`) so it compiles against the adapters-bridged `Controller` base type. `SchoolContextFactory.Create()` and `NotificationService` wiring stay as-is — only the base-class/framework-API surface changes here.
4. Set up `wwwroot/` and static-file middleware (`app.UseStaticFiles()`); move/copy `Content/` and `Scripts/` assets as needed. Replace `App_Start/BundleConfig.cs` bundling logic and the `@Scripts.Render`/`@Styles.Render` calls in the 9 flagged views with direct `<script src="...">`/`<link rel="stylesheet" href="...">` tags. Delete `BundleConfig.cs` once no longer referenced.
5. Build the project. Expect remaining errors to be limited to per-controller `Api.0001` issues (owned by 03.05–03.10), `NotificationService.cs` (owned by 03.04), `SchoolContextFactory.cs`/config reads (owned by 03.03).

## Relevant skills
building-projects.

## Done when
- Project hosts via `Program.cs` + System.Web Adapters (no `Global.asax.cs` startup logic remains uncconverted; `Feature.1000`/`Feature.0002`/`Feature.0003` resolved).
- `BaseController` compiles with 0 API issues.
- Bundling (`Feature.0001`) is fully replaced across `BundleConfig.cs` and all 9 flagged views; `BundleConfig.cs` deleted.
- `dotnet build` shows no errors originating from `Global.asax.cs`, `FilterConfig.cs`, `RouteConfig.cs`, `BundleConfig.cs`, or `BaseController.cs`.

## Research Findings (03.02 execution)

**Baseline build (before this task)**: `dotnet build` = 242× CS0246 + 34× CS0234, concentrated in
`StudentsController.cs`(54), `DepartmentsController.cs`(54), `CoursesController.cs`(54),
`InstructorsController.cs`(50), `NotificationsController.cs`(16), `HomeController.cs`(12) — all
owned by 03.04–03.10 — plus 6 in `Global.asax.cs`, 6 in `RouteConfig.cs`, 4 each in
`FilterConfig.cs`/`BundleConfig.cs`/`BaseController.cs` (this task's scope), and a couple of
generated Razor `.g.cs` errors that trace back to `HomeController`/`StudentsController` API issues.

**Package surface** (`Microsoft.AspNetCore.SystemWebAdapters` 2.3.0, already restored from 03.01):
provides `System.Web.HttpContext`/`HttpRequest`/`HttpResponse`/`HttpSessionState`/`Cache`/
`HttpServerUtility`/`IHttpHandler`/`IHttpModule` compat shims via `AddSystemWebAdapters()` +
`UseSystemWebAdapters()`. It does **not** provide a `System.Web.Mvc.Controller` compat shim —
confirmed via the package README (`lib/net10.0` targets HttpContext-family types only). This means
`BaseController` (and every other controller, later subtasks) must switch its base type to
`Microsoft.AspNetCore.Mvc.Controller`; `System.Web` API calls used inside controller bodies
(`Server.MapPath`, `HttpPostedFileBase`, `HttpContext.Current`, `Session[...]`) are what the
adapters package bridges, not the MVC base class itself. This is a scaffolding-only implication —
no other controller's action-method bodies are touched here (03.05–03.10 own that).

**Static assets**: `bootstrap` 5.3.3 (PackageReference) ships `contentFiles/any/any/wwwroot/css/*`
and `wwwroot/js/*` (nuspec `<contentFiles><files include="**/*" buildAction="Content" /></contentFiles>`)
— these auto-copy into the project's `wwwroot/` on restore once it exists, replacing the (already
dangling/nonexistent) `Content\bootstrap.css` / `Content\bootstrap.min.css` csproj entries. `jQuery`
3.7.1, `jQuery.Validation` 1.21.0, `Microsoft.jQuery.Unobtrusive.Validation` 4.0.0 and `Modernizr`
2.6.2 packages use the legacy `Content/Scripts/*.js` layout (no `contentFiles` element) and do
**not** auto-copy for PackageReference/SDK projects — the repo's committed physical files under
`Scripts/` and `Content/` (`Site.css`, `notifications.css`, `jquery-3.4.1.js`, `bootstrap.js`,
`respond.js`, `modernizr-2.6.2.js`, `jquery.validate*.js`) are the real source of truth for those
and are moved as-is into `wwwroot/Scripts` / `wwwroot/Content` (no version/content changes).

**Bundle → direct-tag mapping** (from `BundleConfig.cs`):
- `~/bundles/jquery` → `~/Scripts/jquery-3.4.1.js`
- `~/bundles/jqueryval` → `~/Scripts/jquery.validate.js` + `~/Scripts/jquery.validate.unobtrusive.js`
- `~/bundles/modernizr` → `~/Scripts/modernizr-2.6.2.js`
- `~/bundles/bootstrap` → `~/Scripts/bootstrap.js` + `~/Scripts/respond.js` (physical files, kept as-is)
- `~/Content/css` (bootstrap.css + site.css) → `~/css/bootstrap.min.css` (from the bootstrap
  PackageReference's auto-copied wwwroot assets, since physical `Content/bootstrap.css` never
  existed on disk — a pre-existing dangling csproj entry) + `~/Content/Site.css` (physical, moved)

Flagged views needing only the `@Scripts.Render("~/bundles/jqueryval")` → two `<script>` tags swap
(identical pattern in all 8): `Views/Courses/Create.cshtml`, `Views/Courses/Edit.cshtml`,
`Views/Departments/Create.cshtml`, `Views/Departments/Edit.cshtml`, `Views/Instructors/Create.cshtml`,
`Views/Instructors/Edit.cshtml`, `Views/Students/Create.cshtml`, `Views/Students/Edit.cshtml`.
`Views/Shared/_Layout.cshtml` needs all four bundle calls (`Styles.Render`/3×`Scripts.Render`) replaced.

**csproj**: `<OutputType>Library</OutputType>` (added in 03.01 because there was no `Program.cs`
yet) must be removed now that a real ASP.NET Core hosting entry point is added, so the SDK's normal
`Exe` default for `Microsoft.NET.Sdk.Web` applies. The explicit `Content`/`None` items pointing at
old `Content\*`/`Scripts\*`/`favicon.ico` paths are removed since files move under `wwwroot/` where
`Microsoft.NET.Sdk.Web`'s implicit `wwwroot/**` glob picks them up automatically (no explicit items
needed). The `Global.asax`/`Global.asax.cs` `DependentUpon` item is removed along with the files.

**`Global.asax.cs` connection-string read**: per scope note, the
`System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString`
read is preserved verbatim (moved into `Program.cs`'s `InitializeDatabase` local function) with an
explicit `// TODO(03.03)` marker comment; not touched/removed by this task.
