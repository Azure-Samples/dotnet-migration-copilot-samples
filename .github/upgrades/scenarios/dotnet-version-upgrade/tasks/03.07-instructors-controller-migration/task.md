# 03.07-instructors-controller-migration: Migrate InstructorsController (67 API issues) and its Views/Instructors folder to net10.0

## Objective
Resolve the 67 `Api.0001` issues in `Controllers/InstructorsController.cs` (ASP.NET MVC 5 surface — `ActionResult`, `ModelState`, `ViewBag`, `SelectList`, `RedirectToAction`, etc.) and migrate its `Views/Instructors/` views (including the two bundling-reference views already handled by 03.02 — do not re-touch those bundling lines here).

Runs after 03.02.

## Scope
`Controllers/InstructorsController.cs`, `Views/Instructors/*.cshtml`.

## Steps
1. Use `get_code_dependencies` on `InstructorsController.cs` to get its full dependency tree (models: `Instructor`, `OfficeAssignment`, `CourseAssignment`; services; views) before making changes.
2. Work through the `Index(int?, int?)`, `Details`, `Create` (x2), `Edit` (x2), `Delete`, `DeleteConfirmed` actions, fixing each flagged `System.Web.Mvc` API usage against the net10.0 + adapters surface (most should resolve via the adapters bridge from 03.02; fix any that don't compile).
3. Build and verify 0 API issues remain for this controller/its views.

## Done when
`InstructorsController.cs` and `Views/Instructors/` build with 0 API issues.
