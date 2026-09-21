# 03.08-departments-controller-migration: Migrate DepartmentsController (94 API issues) and its Views/Departments folder to net10.0

## Objective
Resolve the 94 `Api.0001` issues in `Controllers/DepartmentsController.cs` and migrate its `Views/Departments/` views (including the two bundling-reference views already handled by 03.02 — do not re-touch those bundling lines here).

Runs after 03.02.

## Scope
`Controllers/DepartmentsController.cs`, `Views/Departments/*.cshtml`.

## Steps
1. Use `get_code_dependencies` on `DepartmentsController.cs` to get its full dependency tree (models: `Department`, `Instructor`; concurrency handling via `RowVersion`; views) before making changes.
2. Work through the `Index`, `Details`, `Create` (x2), `Edit` (x2, includes optimistic-concurrency `RowVersion` handling), `Delete`, `DeleteConfirmed` actions, fixing each flagged `System.Web.Mvc` API usage.
3. Build and verify 0 API issues remain for this controller/its views.

## Done when
`DepartmentsController.cs` and `Views/Departments/` build with 0 API issues.

## Research findings (2026-09-21)

- `get_code_dependencies` on `DepartmentsController.cs` confirms `BaseController` (already migrated to
  `Microsoft.AspNetCore.Mvc`/`SchoolContextFactory`), `SchoolContext`, `Department`, `Instructor` and
  `DbUpdateConcurrencyException` already resolve to `Microsoft.EntityFrameworkCore` — EF Core is already
  in place, only the MVC surface (`System.Web.Mvc`) needs migrating.
- Assessment shows all 94 `Api.0001` issues are confined to `Controllers/DepartmentsController.cs`
  (types/attributes: `ActionResult`, `HttpStatusCodeResult`, `HttpNotFoundResult`/`HttpNotFound()`,
  `BindAttribute`, `ValidateAntiForgeryTokenAttribute`, `HttpPostAttribute`, `ActionNameAttribute`,
  `RedirectToRouteResult`/`RedirectToAction`, `ViewResult`/`View`). No `Api.0002` (source-incompatible)
  issues.
- `Views/Departments/*.cshtml` have **no** `Api.0001`/`Api.0002` issues per the assessment. Only
  `Create.cshtml` and `Edit.cshtml` carry a `Feature.0001` (bundling script tag) issue, already resolved
  by task 03.02 — confirmed unchanged (`~/Scripts/jquery.validate.js` script tags, matching the pattern
  left in place for already-migrated `Views/Instructors/Create.cshtml` and `Edit.cshtml`). No view edits
  required for this task.
- Migration mirrors the pattern already applied in `InstructorsController.cs`/`CoursesController.cs`:
  `ActionResult` → `IActionResult`; `new HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`;
  `HttpNotFound()` → `NotFound()`; drop `using System.Web.Mvc;` and `using System.Net;`; add
  `using Microsoft.AspNetCore.Mvc;`. `[HttpPost]`, `[ValidateAntiForgeryToken]`, `[Bind(Include = "...")]`
  (→ `[Bind("...")]` positional args), `[HttpPost, ActionName("Delete")]` all have direct
  `Microsoft.AspNetCore.Mvc` equivalents — no behavior changes needed.
- `Edit`'s optimistic-concurrency handling (`DbUpdateConcurrencyException`, `entry.GetDatabaseValues()`,
  `RowVersion`) already uses EF Core APIs — left as-is, no change required beyond the `ActionResult` →
  `IActionResult` return-type swap.
- `DeleteConfirmed` does not catch `DbUpdateConcurrencyException` today even though `Delete.cshtml`
  renders a hidden `RowVersion` field and `ViewBag.ConcurrencyErrorMessage` placeholder — this is
  pre-existing behavior in the source controller, not something introduced by this migration, so it is
  left unchanged (no new features added during a framework migration task).
