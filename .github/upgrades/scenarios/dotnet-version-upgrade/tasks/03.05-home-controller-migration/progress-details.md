# Progress: 03.05-home-controller-migration

## Summary
Migrated `HomeController.cs` from `System.Web.Mvc` to `Microsoft.AspNetCore.Mvc`, and fixed the one downstream compile issue this uncovered in `Views/Shared/Error.cshtml`.

## Files modified
- `ContosoUniversity/Controllers/HomeController.cs` — `using System.Web.Mvc;` → `using Microsoft.AspNetCore.Mvc;`; all action return types `ActionResult` → `IActionResult` (per `migrating-mvc-controllers` skill; `BaseController` already inherits `Microsoft.AspNetCore.Mvc.Controller` from an earlier task). `Error()` rewritten to the standard ASP.NET Core template pattern: `[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]` + `return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier })`. Added `using System.Diagnostics;` and `using ContosoUniversity.Models;`; dropped unused `using System.Collections.Generic;`.
- `ContosoUniversity/Views/Shared/Error.cshtml` — `@model System.Web.Mvc.HandleErrorInfo` (framework-only type, no adapter equivalent) → `@model ContosoUniversity.Models.ErrorViewModel`; replaced `HttpContext.Current.IsDebuggingEnabled` / `Model.Exception` / `Model.ControllerName` / `Model.ActionName` block (all Framework-only APIs) with the standard Core template's `@if (Model.ShowRequestId) { ... Model.RequestId ... }`.

## Not modified (verified no issues)
- `Views/Home/Index.cshtml`, `Views/Home/About.cshtml`, `Views/Home/Contact.cshtml` — use only `@Url.Action`, `@Html.DisplayFor`, `ViewBag`, all of which work unchanged under ASP.NET Core Razor. No `System.Web.Mvc` references.

## Research
- `get_code_dependencies` on `HomeController.cs`: 26 nodes, all internal to the project (BaseController, SchoolContext/Factory, model graph, NotificationService, EnrollmentDateGroup) — no external migration-relevant dependencies.
- `query_dotnet_assessment` search for "HomeController" and "HandleErrorInfo" both returned 0 matching issues, consistent with the task description ("no significant flagged API issues").
- No `// STUB:` markers found in `Controllers/HomeController.cs`, `Views/Home/*.cshtml`, or `Views/Shared/Error.cshtml` — no stub-resolution decomposition triggered.

## Known pre-existing gap (out of scope)
- `HomeController.Unauthorized()` has no matching `Views/Home/Unauthorized.cshtml`, and nothing in the repo links to `Home/Unauthorized`. This predates the migration (not introduced by this task) and is outside the declared scope (`HomeController.cs`, `Views/Home/*.cshtml`, `Views/Shared/Error.cshtml`) — left unchanged. Flagging for user awareness.

## Build/test result
`dotnet build ContosoUniversity.sln -f net10.0` — clean, 0 errors/warnings related to `HomeController.cs`, `Views/Home/*.cshtml`, or `Views/Shared/Error.cshtml`. `get_errors` on all touched files: no errors.

## Decomposition verdict
Assessed per execution.md (Sections 0 & 3) — no stubs found, single small controller + 3 views, no branching decision points or independent concerns. Verdict: **atomic**, executed directly (no `TaskBreaker` dispatch).
