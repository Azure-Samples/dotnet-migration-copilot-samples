# 03.05-home-controller-migration: Migrate HomeController and its Views/Home folder to net10.0 (minimal API surface)

## Objective
Migrate `Controllers/HomeController.cs` and its `Views/Home/` views to build and run correctly on net10.0 with the System.Web Adapters bridge in place (from 03.02). Per the assessment, this controller has no significant flagged API issues of its own — this subtask verifies it end-to-end and is a good simplest-first starting point.

Runs after 03.02 (needs `BaseController` and hosting scaffolding in place).

## Scope
`Controllers/HomeController.cs`, `Views/Home/*.cshtml`, `Views/Shared/Error.cshtml` if referenced by `HomeController.Error()`.

## Steps
1. Use `get_code_dependencies` on `HomeController.cs` to confirm its full dependency tree (services, models, views).
2. Build and fix any residual `System.Web.Mvc` API issues on this controller/its views (expected to be minimal/none per assessment).
3. Verify `Index`, `About`, `Contact`, `Error`, `Unauthorized` actions compile and return views correctly under the adapters bridge.

## Done when
`HomeController.cs` and `Views/Home/` build with 0 API issues and no remaining assessment findings for this controller.

## Research findings

- `get_code_dependencies` on `HomeController.cs`: 26 nodes, all internal (`BaseController`, `SchoolContext`/`SchoolContextFactory`, model graph, `NotificationService`, `EnrollmentDateGroup`). `BaseController` was already migrated in an earlier task (already inherits `Microsoft.AspNetCore.Mvc.Controller`), so `HomeController` only needed its own namespace/return-type migration.
- Assessment query (`search` for "HomeController" / "HandleErrorInfo") returned 0 matches — confirms assessment has no flagged findings for this controller, matching the task description.
- `HomeController.cs` before: `using System.Web.Mvc;`, all actions returned `ActionResult`. Migrated to `using Microsoft.AspNetCore.Mvc;` + `IActionResult` per `migrating-mvc-controllers` skill (MVC controllers keep `Controller` base class, `ActionResult`→`IActionResult` is the idiomatic Core rewrite).
- `Error()` action: original returned a bare `View()`. `Views/Shared/Error.cshtml` was strongly typed to `@model System.Web.Mvc.HandleErrorInfo` and used `HttpContext.Current.IsDebuggingEnabled` — both framework-only APIs with no adapter equivalent (System.Web Adapters cover `System.Web`, not the removed `System.Web.Mvc` types). `Program.cs` already wires `app.UseExceptionHandler("/Home/Error")` (from the 03.02 hosting task) and an unused `ContosoUniversity.Models.ErrorViewModel` already existed in the repo. Rewrote `Error()` to follow the standard ASP.NET Core template: pass `new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }` with `[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]`, and rewrote `Error.cshtml` to `@model ContosoUniversity.Models.ErrorViewModel` showing `Model.RequestId` when `Model.ShowRequestId`.
- `Index.cshtml`, `About.cshtml`, `Contact.cshtml`: no System.Web.Mvc-specific APIs (`@Url.Action`, `@Html.DisplayFor`, `ViewBag` all work unchanged under ASP.NET Core Razor) — left as-is.
- `Unauthorized()` action has no matching `Views/Home/Unauthorized.cshtml` and nothing in the repo links to `Home/Unauthorized` — this is a pre-existing gap in the original app, not introduced by this migration and outside this task's scope (task scope is `Controllers/HomeController.cs`, `Views/Home/*.cshtml`, `Views/Shared/Error.cshtml`). Left unchanged.

