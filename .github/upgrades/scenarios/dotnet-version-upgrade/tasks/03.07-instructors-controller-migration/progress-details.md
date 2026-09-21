# Progress: 03.07-instructors-controller-migration

## Files modified
- `ContosoUniversity/Controllers/InstructorsController.cs`

## Research (before editing)
- `get_code_dependencies` on `InstructorsController.cs`: 28 nodes — `BaseController` (already migrated to `Microsoft.AspNetCore.Mvc.Controller` in 03.02), `SchoolContext`/`SchoolContextFactory`, the full `Instructor`/`Course`/`Department`/`Enrollment`/`Student`/`OfficeAssignment`/`CourseAssignment`/`Person` model graph, `NotificationService`, `InstructorIndexData`/`AssignedCourseData` view models, and EF Core types. All internal dependencies were already resolvable; no cross-file work required beyond the controller itself.
- `query_dotnet_assessment` search for "InstructorsController" returned 0 matching issues (assessment data doesn't have a fresh match for this file at this point in the flow) — proceeded directly from the task description's known API surface (`ActionResult`, `ModelState`, `ViewBag`, `RedirectToAction`, `HttpNotFound`, `HttpStatusCodeResult`, `[Bind(Include=...)]`, `TryUpdateModel`).
- Confirmed `Views/Instructors/*.cshtml` (`Index`, `Details`, `Create`, `Edit`, `Delete`) use only `Html.BeginForm`, `Html.AntiForgeryToken`, `Html.LabelFor`/`EditorFor`/`ValidationMessageFor`/`ValidationSummary`/`HiddenFor`/`DisplayFor`/`DisplayNameFor`, `Html.ActionLink`, and `ViewBag` — all supported unchanged by ASP.NET Core's `HtmlHelper` extensions. `Create.cshtml`/`Edit.cshtml` `@section Scripts` blocks already use plain `<script>` tags (bundling swap already done by 03.02) — no view edits were needed or made.
- No `SelectList` usage in this controller (unlike Courses/Departments), so no `Microsoft.AspNetCore.Mvc.Rendering.SelectList` migration was needed here.

## Changes made
1. `using System.Web.Mvc;` + `using System.Net;` → `using Microsoft.AspNetCore.Mvc;` + `using System.Threading.Tasks;` (needed for the new async `Edit` POST action).
2. All action return types `ActionResult` → `IActionResult` (`Index`, `Details`, `Create` GET, `Create` POST, `Edit` GET, `Edit` POST, `Delete`, `DeleteConfirmed`).
3. `new HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()` (4 occurrences: `Details`, `Edit` GET, `Edit` POST, `Delete`).
4. `HttpNotFound()` → `NotFound()` (3 occurrences: `Details`, `Edit` GET, `Delete`).
5. `[Bind(Include = "LastName,FirstMidName,HireDate,OfficeAssignment")]` → `[Bind("LastName", "FirstMidName", "HireDate", "OfficeAssignment")]` on `Create` POST — `Microsoft.AspNetCore.Mvc.BindAttribute.Include` is a get-only `string[]` populated from the `params string[]` constructor, so the MVC5 named-argument single comma-separated-string form doesn't compile under Core; switched to individual positional property-name strings.
6. `Edit` POST: `TryUpdateModel(instructorToUpdate, "", new string[] {...})` (synchronous, framework-only) → `await TryUpdateModelAsync<Instructor>(instructorToUpdate, "", i => i.LastName, i => i.FirstMidName, i => i.HireDate, i => i.OfficeAssignment)` using the expression-based overload on `ControllerBase`; action signature changed from `public ActionResult Edit(...)` to `public async Task<IActionResult> Edit(...)`.
7. `RedirectToAction`, `ModelState.IsValid`/`AddModelError`, `ViewBag`, `db.Entry(...).State = EntityState.Deleted` were already Core-compatible / already EF Core — left unchanged.

## Build self-check
`dotnet build ContosoUniversity.csproj -f net10.0`: 0 errors/0 warnings originate from `InstructorsController.cs` or `Views/Instructors/*.cshtml`. Remaining 166 errors / 66 warnings in the whole-project build are all pre-existing, in files owned by other tasks (`StudentsController.cs` (03.09), `DepartmentsController.cs` (03.08), `CoursesController.cs` (03.10), `NotificationService.cs`/config reads, etc.) — none trace back to this task's scope. `get_errors` on the touched file: no errors.

## Decomposition verdict
Loaded `migrating-mvc-controllers` skill (Execution stage guidance) and assessed decomposition. Task scope is one controller (8 actions) + 5 already-compatible views, no branching decision points, no independent concerns, and no stub markers found. Verdict: **atomic** — executed directly, no `TaskBreaker` dispatch.

## Done-when criteria
- [x] `InstructorsController.cs` and `Views/Instructors/` build with 0 API issues.
