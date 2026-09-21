# Progress: 03.06-notifications-controller-migration

## Files modified
- `Controllers/NotificationsController.cs`

## Findings (confirmed before editing)
- `Services/NotificationService.cs` (landed in 03.04) exposes `Notification ReceiveNotification()` and `void MarkAsRead(int notificationId)` — exact match to what this controller already called. No service changes needed.
- `get_code_dependencies` on the controller returned 25 nodes; every dependency (BaseController, NotificationService, Notification/EntityOperation models, EF Core types) was already resolvable/migrated. No further cross-file work required.
- `Views/Notifications/Index.cshtml` only uses `ViewBag`, `Layout`, and `@Html.ActionLink(...)`, all of which work unchanged in ASP.NET Core Razor — no edits made to the view.

## Changes made
1. `using System.Web.Mvc;` → `using Microsoft.AspNetCore.Mvc;` (plus added `using System.Text.Json;`).
2. `ActionResult Index()` → `IActionResult Index()` (consistent with sibling controllers).
3. Removed `JsonRequestBehavior.AllowGet` (no longer exists; GET is always allowed for `Json()` in ASP.NET Core).
4. **Functional fix, not just a mechanical rename**: `wwwroot/Scripts/notifications.js` reads PascalCase properties off the JSON payload (`notification.Operation`, `.CreatedAt`, `.EntityType`, `.Message`, `.CreatedBy`). ASP.NET Core's `Json()` defaults to System.Text.Json with camelCase naming, which would have silently broken the polling client. Added a local `static readonly JsonSerializerOptions PascalCaseJsonOptions` (`PropertyNamingPolicy = null`) and passed it to the two `Json()` calls that return `Notification` data in `GetNotifications()`. Scoped to this controller only — did not touch global `Program.cs`/`AddJsonOptions`, since that would change casing for other controllers outside this task's scope.

## Build self-check
Ran `dotnet build ContosoUniversity.csproj -f net10.0`. Zero errors/warnings originate from `NotificationsController.cs` or `Views/Notifications/*`. Remaining 108 errors / 32 warnings in the build are all in unrelated, not-yet-migrated files (`StudentsController.cs`, `CoursesController.cs`, `DepartmentsController.cs`, `HomeController.cs`, and a generated Razor file for `Views/Students/Index.cshtml`) — out of this task's scope and owned by other tasks.

## Decomposition
Loaded `migrating-mvc-controllers` skill and assessed decomposition. Task scope is a single small controller (3 actions) + one already-compatible view — genuinely one coherent unit. No escalation to TaskBreaker needed.

## Done-when criteria
- [x] `NotificationsController.cs` and `Views/Notifications/` build with 0 API issues.
- [x] Correctly calls the replaced `NotificationService` API (signatures confirmed unchanged).
