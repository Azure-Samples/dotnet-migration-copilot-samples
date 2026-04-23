# Controller Generator for Contoso University

## Context
This is an ASP.NET MVC 5 application using EF Core 3.1 with SQL Server.
All controllers inherit from `BaseController` which provides:
- `db` (SchoolContext) for database access
- `SendEntityNotification(entityType, entityId, displayName, operation)` for MSMQ notifications

## Template Variables
- {{EntityName}}: The entity class name (e.g., "Scholarship")
- {{Properties}}: The entity properties list
- {{RelatedEntities}}: Foreign keys and navigation properties

## Requirements
1. Inherit from `BaseController`
2. Implement full CRUD: `Index`, `Details`, `Create` (GET/POST), `Edit` (GET/POST), `Delete` (GET/POST via `DeleteConfirmed`)
3. Use `[ValidateAntiForgeryToken]` on all POST actions
4. Use `[Bind(Include = "...")]` to prevent over-posting
5. Call `SendEntityNotification()` on Create, Edit, and Delete
6. Include proper null/id checks — return `HttpStatusCodeResult(HttpStatusCode.BadRequest)` for null ids, `HttpNotFound()` for missing records
7. Use `.Include()` for eager loading related entities in Detail/Index views
8. Use `try/catch` with `ModelState.AddModelError` for save failures
9. Dispose pattern: override `Dispose(bool)` and let `BaseController` handle context disposal

## Reference Implementation
See `Controllers/StudentsController.cs` for the canonical pattern.

## Generate
A complete controller for {{EntityName}} following all conventions above.
