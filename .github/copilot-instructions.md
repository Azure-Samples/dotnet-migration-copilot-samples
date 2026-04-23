# Contoso University — Copilot Instructions

## Tech Stack
- ASP.NET MVC 5 on .NET Framework 4.8
- Entity Framework Core 3.1.32 with SQL Server LocalDB
- Razor views with Bootstrap 5.3.3 and jQuery 3.7.1
- MSMQ for real-time notifications

## Coding Conventions
- All controllers inherit from `BaseController` (provides `db` context and `SendEntityNotification`)
- Use `[Bind(Include = "...")]` on POST actions to prevent over-posting
- Use `[ValidateAntiForgeryToken]` on all POST actions
- ViewBag is used for passing dropdown data (e.g., `ViewBag.DepartmentID`)
- Pagination uses the `PaginatedList<T>` helper class (page size = 10)
- Send MSMQ notifications for CREATE, UPDATE, DELETE operations via `SendEntityNotification()`

## Patterns to Follow
- Follow existing controller patterns (see `StudentsController` as the reference implementation)
- Use EF Core `.Include()` / `.ThenInclude()` for eager loading relationships
- Validate DateTime fields for SQL Server `datetime2` range (1753–9999)
- For file uploads, validate extension and size, generate unique filenames with GUID
- Use `[HttpPost, ActionName("Delete")]` with `DeleteConfirmed` method name for delete POST actions
- Return `HttpStatusCodeResult(HttpStatusCode.BadRequest)` for null id parameters
- Return `HttpNotFound()` for entities not found in the database

## Data Model
- Person (base class) → Student, Instructor (TPH inheritance with discriminator)
- Course belongs to Department, has Enrollments and CourseAssignments
- Enrollment links Student to Course with an optional Grade enum (A–F)
- CourseAssignment links Instructor to Course (composite key)
- OfficeAssignment is one-to-one with Instructor
- Department has an optional Administrator (Instructor) and uses RowVersion for concurrency

## Project Structure
```
ContosoUniversity/
├── Controllers/    # MVC controllers inheriting BaseController
├── Data/           # SchoolContext, SchoolContextFactory, DbInitializer
├── Models/         # Entity classes and ViewModels
├── Services/       # NotificationService, LoggingService
├── Views/          # Razor views organized by controller
├── Content/        # CSS files
├── Scripts/        # JavaScript files
└── App_Start/      # Route, Bundle, Filter configuration
```
