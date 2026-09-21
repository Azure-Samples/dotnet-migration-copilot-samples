# 03.09-students-controller-migration: Migrate StudentsController (91 API issues) and its Views/Students folder to net10.0

## Objective
Resolve the 91 `Api.0001` issues in `Controllers/StudentsController.cs` and migrate its `Views/Students/` views (including the two bundling-reference views already handled by 03.02 — do not re-touch those bundling lines here).

Runs after 03.02.

## Scope
`Controllers/StudentsController.cs`, `Views/Students/*.cshtml`.

## Steps
1. Use `get_code_dependencies` on `StudentsController.cs` to get its full dependency tree (models: `Student`, `Enrollment`; paging via `PaginatedList`; sort/filter query-string handling) before making changes.
2. Work through the `Index(string, string, string, int?)` (sorting/paging/filtering), `Details`, `Create` (x2), `Edit` (x2), `Delete`, `DeleteConfirmed` actions, fixing each flagged `System.Web.Mvc` API usage.
3. Build and verify 0 API issues remain for this controller/its views.

## Done when
`StudentsController.cs` and `Views/Students/` build with 0 API issues.
