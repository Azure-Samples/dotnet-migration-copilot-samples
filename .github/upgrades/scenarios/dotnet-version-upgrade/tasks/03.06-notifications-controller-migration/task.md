# 03.06-notifications-controller-migration: Migrate NotificationsController and its Views/Notifications folder to net10.0

## Objective
Migrate `Controllers/NotificationsController.cs` and its `Views/Notifications/` views to net10.0. This controller calls `notificationService.ReceiveNotification()` and `.MarkAsRead()` directly, so it depends on subtask 03.04 (MSMQ replacement) having landed first with unchanged public signatures.

Runs after 03.02 and 03.04.

## Scope
`Controllers/NotificationsController.cs`, `Views/Notifications/*.cshtml`.

## Steps
1. Confirm `Services/NotificationService.cs` (from 03.04) exposes `ReceiveNotification()`, `MarkAsRead(int)` with the same signatures this controller already calls — no changes needed if 03.04 preserved them.
2. Use `get_code_dependencies` to confirm the full dependency tree.
3. Build and fix any residual `System.Web.Mvc`/`JsonResult` API issues (expected to be minimal per assessment — this controller mostly uses JSON actions, not flagged directly).
4. Verify `GetNotifications`, `MarkAsRead`, `Index` actions compile and function under the adapters bridge.

## Done when
`NotificationsController.cs` and `Views/Notifications/` build with 0 API issues, and correctly call the replaced `NotificationService` API.

## Research findings

- **03.04 signature confirmation**: `Services/NotificationService.cs` exposes `Notification ReceiveNotification()` and `void MarkAsRead(int notificationId)` — identical signatures to what `NotificationsController.GetNotifications()`/`MarkAsRead(int)` already call. No service-side changes needed.
- **`get_code_dependencies` result**: 25 nodes, all resolve cleanly — `BaseController` (already on `Microsoft.AspNetCore.Mvc`), `NotificationService`, `Notification`/`EntityOperation` models, EF Core types. No `System.Web` dependency chain outside the controller file itself.
- **Controller migration required** (still on `System.Web.Mvc`):
  - `using System.Web.Mvc;` → `using Microsoft.AspNetCore.Mvc;`
  - `Json(data, JsonRequestBehavior.AllowGet)` → `Json(data)` (second param removed, GET always allowed in Core)
  - `ActionResult Index()` → `IActionResult Index()` for consistency with sibling controllers (e.g. `HomeController`)
- **JSON casing gotcha (functional bug, not just a style nit)**: `wwwroot/Scripts/notifications.js` reads `notification.Operation`, `.CreatedAt`, `.EntityType`, `.Message`, `.CreatedBy` — all PascalCase, matching `Notification` model properties and ASP.NET MVC 5's default JSON serializer. ASP.NET Core's `Json()` defaults to System.Text.Json with camelCase property naming, which would silently break the polling client. Fix locally in this controller only (not global `Program.cs` options, to avoid touching JSON casing for other controllers out of scope): pass a `JsonSerializerOptions` with `PropertyNamingPolicy = null` to the `Json()` calls returning `Notification` data.
- **View**: `Views/Notifications/Index.cshtml` only uses `ViewBag`, `Layout`, and `@Html.ActionLink(...)` — all supported as-is in ASP.NET Core Razor/`IHtmlHelper`. No changes required.
