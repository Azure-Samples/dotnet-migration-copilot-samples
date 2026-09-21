# Progress Details: 03.10-courses-controller-migration

## Files modified

- `ContosoUniversity/Controllers/CoursesController.cs` — full migration from `System.Web.Mvc` to `Microsoft.AspNetCore.Mvc`.
- `ContosoUniversity/Program.cs` — added a second `app.UseStaticFiles(...)` mapping `/Uploads` to the `Uploads/` folder (outside `wwwroot`), required for the teaching-material image feature to keep working (see deviation note below).
- `Views/Courses/*.cshtml` — **no changes required**; verified by build (see below).

## Changes made to `CoursesController.cs`

- Namespaces: `System.Web.Mvc`/`System.Web`/`System.Net` removed; added `Microsoft.AspNetCore.Mvc`, `Microsoft.AspNetCore.Mvc.Rendering` (for `SelectList`), `Microsoft.AspNetCore.Http` (for `IFormFile`), `Microsoft.AspNetCore.Hosting` (for `IWebHostEnvironment`).
- All action return types: `ActionResult` → `IActionResult` (matches already-migrated `InstructorsController`/`StudentsController` convention).
- `new HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`; `HttpNotFound()` → `NotFound()`.
- `[Bind(Include = "CourseID,Title,Credits,DepartmentID,TeachingMaterialImagePath")]` → `[Bind("CourseID", "Title", "Credits", "DepartmentID", "TeachingMaterialImagePath")]` (matches the multi-arg `[Bind]` form already used in `InstructorsController`).
- File upload (`Create`/`Edit`, the two `HttpPostedFileBase teachingMaterialImage` parameters): converted to `IFormFile teachingMaterialImage` — native ASP.NET Core model binding, not the System.Web Adapters bridge (see decision below). `ContentLength` → `Length`; `SaveAs(filePath)` → `using (var stream = new FileStream(filePath, FileMode.Create)) { teachingMaterialImage.CopyTo(stream); }`.
- `Server.MapPath(...)` (4 call sites) → new private `MapPath(string virtualPath)` helper backed by an injected `IWebHostEnvironment.ContentRootPath` (parity with classic `Server.MapPath("~/...")`, which resolves against the app root, not `wwwroot`). `CoursesController` now has a constructor (`IWebHostEnvironment environment`) — the first controller in this codebase to need one.

## File-upload bridge decision (HttpPostedFileBase vs adapters vs IFormFile)

Confirmed via repo-wide grep that `CoursesController.cs` was the only file left referencing `System.Web`/`System.Web.Mvc`. `Microsoft.AspNetCore.SystemWebAdapters` is referenced and wired in `Program.cs`, but its shimmed surface is `HttpContext`/`Session`/etc. — not `System.Web.Mvc` controller/model-binding types (`ActionResult`, `SelectList`, `HttpPostedFileBase`). Every other controller in the repo already fully rewrote to native ASP.NET Core MVC types rather than using the adapters bridge for MVC surface. Decision: convert `HttpPostedFileBase` → `IFormFile` (native ASP.NET Core upload binding), consistent with that established pattern. Full reasoning also recorded in `task.md`.

## Deviation from stated file scope

The task's "Done when" requires the teaching-material image upload to **continue to function**, not just compile. `Uploads/TeachingMaterials/` is at the project root, not under `wwwroot/`, and `Program.cs` only served the default `wwwroot` static files. Without an additional static-file mapping, images referenced via `~/Uploads/...` (rendered with `@Url.Content(...)` in the views) would 404 at runtime even though the code compiles. Added a second `app.UseStaticFiles(...)` block in `Program.cs` (`PhysicalFileProvider` over `ContentRootPath/Uploads`, `RequestPath = "/Uploads"`) to satisfy the functional acceptance criterion. This is a small, necessary addition outside the stated `Controllers/CoursesController.cs`, `Views/Courses/*.cshtml` scope.

## Views

Read all 5 views (`Index`, `Details`, `Create`, `Edit`, `Delete`). None reference `System.Web.Mvc` types or contain `@Scripts.Render`/`@Styles.Render` bundling calls (nothing to avoid re-touching from 03.02). All Html-helper calls used (`Html.BeginForm`, `LabelFor`/`EditorFor`/`ValidationMessageFor`, `DropDownList`, `DisplayFor`/`DisplayNameFor`, `Url.Content`) have compatible ASP.NET Core overloads, already proven working unchanged in the other migrated controllers' views. No edits were needed; confirmed by the build below.

## Decomposition verdict

Loaded the scenario's `execution.md` (Execution stage) and `breakdown-hints/framework-web-migration.md`. The `web-controller-migration-units` hint (**MUST** if >5 controllers, **SHOULD** otherwise) specifies each controller is its own subtask, no grouping — already satisfied since this task is scoped to exactly one controller (the last of the per-controller subtasks: 03.07/03.08/03.09/03.10). No other hint (DI container, auth, config, bundling, middleware, MSMQ, EF6-init) applies to this controller. Verdict: **atomic** — executed directly, no `TaskBreaker` escalation.

## Build/test result

Clean rebuild (`rm -rf obj bin && dotnet build ContosoUniversity.csproj`): **0 errors, 0 warnings**. Repo-wide grep for `using System\.Web`, `HttpPostedFileBase`, `Server\.MapPath` returns zero matches — this was the last controller with `System.Web` dependencies; the whole project now compiles clean of API-migration issues.

No automated test project exists (Test Coverage was skipped per `scenario-instructions.md`); no tests to run.
