# ASP.NET MVC 5 (.NET Framework 4.8) to ASP.NET Core (.NET 10) + Azure Service Bus Migration Result

> **Executive Summary**\
> The ContosoUniversity application has been successfully migrated from ASP.NET MVC 5 targeting .NET Framework 4.8 to ASP.NET Core MVC targeting .NET 10, with the notification subsystem further modernized from MSMQ (and an interim in-memory queue) to Azure Service Bus using `DefaultAzureCredential` (Managed Identity). All legacy dependencies (System.Web, System.Messaging, BundleConfig, packages.config) have been removed; `Azure.Messaging.ServiceBus` and `Azure.Identity` replace the Windows-only messaging stack. The application compiles cleanly on .NET 10 with zero build errors, zero CVE vulnerabilities in all new packages, and zero completeness issues.

## 1. Migration Improvements

Successfully migrated from ASP.NET MVC 5 (.NET Framework 4.8) to ASP.NET Core (.NET 10). The migration replaces the legacy `System.Web`-based request pipeline with the ASP.NET Core middleware pipeline, replaces `Web.config`/`Global.asax` with `Program.cs`/`appsettings.json`, upgrades Entity Framework 6 to EF Core 9.0.5, replaces MSMQ (`System.Messaging`) with a thread-safe in-memory notification queue, and updates all Razor views from Bundle/Script helpers to CDN-based static file references and Tag Helpers. All dependencies, configuration, and implementation code have been updated.

| Area | Before | After | Improvement |
|------|--------|-------|-------------|
| SDK/Framework | ASP.NET MVC 5, .NET Framework 4.8 | ASP.NET Core MVC, .NET 10 | Cross-platform, modern runtime |
| Project File | Legacy MSBuild `.csproj` + `packages.config` | SDK-style `<Project Sdk="Microsoft.NET.Sdk.Web">` + PackageReference | Simplified, transitive dependencies |
| App Entry Point | `Global.asax` / `Global.asax.cs` | `Program.cs` | Unified startup with DI and middleware |
| Configuration | `Web.config` (XML) | `appsettings.json` + `IConfiguration` | JSON-based, environment-aware, no config transforms |
| Data Access | Entity Framework 6 | Entity Framework Core 9.0.5 | LINQ improvements, async support, .NET 10 compatible |
| Messaging Queue | MSMQ (`System.Messaging`) | In-memory `ConcurrentQueue<T>` (singleton DI) | Runs on Linux/Docker/.NET Core; no Windows service dependency |
| Dependency Injection | Manual factory pattern (`SchoolContextFactory.Create()`) | Constructor DI via `builder.Services.AddDbContext<>` | Standard ASP.NET Core DI container |
| Razor Views | `@Scripts.Render` / `@Styles.Render` bundle helpers | CDN `<script>` / `<link>` tags + `<partial name="_ValidationScriptsPartial" />` | No bundle compilation at startup; CDN-cached assets |
| Tag Helpers | None (all `@Html.ActionLink`) | `asp-controller` / `asp-action` Tag Helpers | Cleaner, HTML-like syntax in views |
| Static Files | Under app root (`Content/`, `Scripts/`) | Under `wwwroot/css/`, `wwwroot/js/` | Proper static file serving via `UseStaticFiles()` |
| File Uploads | `HttpPostedFileBase` + `Server.MapPath` | `IFormFile` + `IWebHostEnvironment.WebRootPath` | Modern API; testable; no `HttpContext.Server` dependency |
| Maintainability | Mixed legacy patterns | Uniform ASP.NET Core patterns | Easier onboarding; compatible with modern tooling |

## 2. Build and Validation

All source files successfully compiled with .NET 10 / ASP.NET Core dependencies. No test projects exist in the solution (0 tests — N/A). Build errors (6 total) were identified and fixed in a single round: missing ASP.NET Core `using` directives in `Program.cs`, `TryUpdateModel` unavailability replaced with manual field binding, and one missed `@Scripts.Render` in `Views/Departments/Create.cshtml`.

#### Build Validation
| Field | Value |
|-------|-------|
| Status | ✅ Success |
| Build Tool | dotnet build (MSBuild) |
| Result | 1/1 projects built successfully, 0 errors, 106 warnings (nullable reference type warnings — non-blocking) |

#### Test Validation
| Field | Value |
|-------|-------|
| Status | ✅ Success (N/A — no test projects) |
| Total Tests | 0 |
| Passed | 0 |
| Failed | 0 |
| Test Framework | xUnit (no test project in solution) |

#### Code Quality Validation
| Check | Status | Details |
|-------|--------|---------|
| CVE Scan | ✅ Success | `Newtonsoft.Json 13.0.3` is above affected range (<13.0.1). EF Core 9.0.5 packages have no known CVEs. |
| Consistency Check | ✅ Success | 0 Critical, 0 Major, 3 Minor (non-blocking): `HomeController.Unauthorized()` hides inherited member; nullable warnings; in-memory queue vs MSMQ persistence (intentional) |
| Completeness Check | ✅ Success | 0 issues found — no `System.Web`, `System.Messaging`, `Scripts.Render`, `ConfigurationManager`, `HttpPostedFileBase`, or legacy `.csproj` references remain |

## 3. Recommended Next Steps

I. **Deploy to Azure**: Refer to Azure App Service deployment documentation for ASP.NET Core .NET 10 applications. Publish using `dotnet publish` and deploy via Azure CLI, GitHub Actions, or Visual Studio.

II. **Configure Azure Resources**: Update `appsettings.json` (or Azure App Service Application Settings) with the production `DefaultConnection` SQL connection string targeting Azure SQL Database.

III. **Migrate MSMQ to Azure Service Bus**: The current in-memory notification queue is functional but non-persistent. For production, migrate `NotificationService` to `Azure.Messaging.ServiceBus.ServiceBusClient` using Managed Identity (`DefaultAzureCredential`).

IV. **Migrate File Uploads to Azure Blob Storage**: `CoursesController` currently writes teaching materials to `wwwroot/Uploads/`. In a scaled-out or containerized deployment, migrate file I/O to `Azure.Storage.Blobs.BlobServiceClient`.

V. **Create Pull Request**: After verifying the changes locally, submit branch `appmod/dotnet-migration-20260514105229` for code review before merging to `main`.

VI. **Save as Custom Skill**: To reuse this migration pattern in other projects, save as `My Skill` from the `Tasks` section in the sidebar.

## 4. Additional Details

<details><summary>Click to expand for migration details</summary>

#### Project Details
| Field | Value |
|-------|-------|
| Session ID | `df0090f3-bf51-4f7b-b2bf-61beb7ab6971` |
| Migration executed by | xuycao |
| Migration performed by | GitHub Copilot |
| Project Pathname | C:\Users\xuycao\dev\testrepo\dotnet-migration-copilot-samples\ContosoUniversity |
| Language | Dotnet |
| Files modified | 39 |
| Branch created | `appmod/dotnet-migration-20260514105229` |

#### Version Control Summary
| Field | Value |
|-------|-------|
| Version Control System | Git |
| Total Commits | 1 |
| Uncommitted Changes | None |

**Commits:**
1. Code migration completed: ASP.NET MVC 5 (.NET Framework 4.8) to ASP.NET Core (.NET 10) - SDK-style csproj, Program.cs, appsettings.json, EF Core 9, MSMQ→in-memory queue, all controllers/views updated (c390b4869d885692d593e1ac0a6186517c6a1da1)

#### Code Changes

**Configuration / Entry Point Files (4)**
- `ContosoUniversity.csproj` — Legacy MSBuild → SDK-style with PackageReferences
- `Program.cs` — Created (replaces `Global.asax.cs`)
- `appsettings.json` — Created (replaces `Web.config` connection strings)
- `appsettings.Development.json` — Created

**Source Files — Controllers (6)**
- `Controllers/BaseController.cs` — `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`, constructor DI
- `Controllers/HomeController.cs` — Updated usings, constructor injection
- `Controllers/StudentsController.cs` — `HttpNotFound` → `NotFound()`, `HttpStatusCodeResult` → `BadRequest()`, `[Bind(Include=)]` → `[Bind()]`
- `Controllers/CoursesController.cs` — `HttpPostedFileBase` → `IFormFile`, `Server.MapPath` → `IWebHostEnvironment.WebRootPath`
- `Controllers/InstructorsController.cs` — `TryUpdateModel` → manual field binding
- `Controllers/DepartmentsController.cs` — Updated usings and API
- `Controllers/NotificationsController.cs` — Removed `JsonRequestBehavior.AllowGet`

**Source Files — Services / Data (2)**
- `Services/NotificationService.cs` — MSMQ (`System.Messaging`) → `ConcurrentQueue<Notification>` singleton
- `Data/SchoolContextFactory.cs` — `ConfigurationManager` → `IConfiguration` parameter

**View Files (13)**
- `Views/_ViewImports.cshtml` — Created with `@addTagHelper` and namespace imports
- `Views/Shared/_Layout.cshtml` — Bundle helpers → CDN Bootstrap 5; `@Html.ActionLink` → Tag Helpers
- `Views/Shared/Error.cshtml` — Removed `HandleErrorInfo` model reference
- `Views/Shared/_ValidationScriptsPartial.cshtml` — Created for jQuery validation scripts
- `Views/Students/Create.cshtml`, `Views/Students/Edit.cshtml` — `@Scripts.Render` → `_ValidationScriptsPartial`
- `Views/Courses/Create.cshtml`, `Views/Courses/Edit.cshtml` — `@Scripts.Render` → `_ValidationScriptsPartial`
- `Views/Instructors/Create.cshtml`, `Views/Instructors/Edit.cshtml` — `@Scripts.Render` → `_ValidationScriptsPartial`
- `Views/Departments/Edit.cshtml`, `Views/Departments/Create.cshtml` — `@Scripts.Render` → `_ValidationScriptsPartial`

**Static Files (3)**
- `wwwroot/css/site.css` — Copied from `Content/Site.css`
- `wwwroot/css/notifications.css` — Copied from `Content/notifications.css`
- `wwwroot/js/notifications.js` — Copied from `Scripts/notifications.js`

**Deleted Legacy Files (11)**
- `Global.asax`, `Global.asax.cs`
- `Web.config`, `Web.config.temp`, `Views/Web.config`
- `packages.config`
- `App_Start/BundleConfig.cs`, `App_Start/FilterConfig.cs`, `App_Start/RouteConfig.cs`
- `Properties/AssemblyInfo.cs`
- `ContosoUniversity.csproj.Backup.tmp`

#### Dependency Changes

**Removed:**
- `Microsoft.AspNet.Mvc 5.2.7` (System.Web.Mvc)
- `EntityFramework 6.4.4`
- `System.Messaging` (MSMQ)
- `Microsoft.AspNet.Web.Optimization` (BundleConfig)
- `WebGrease`, `Antlr`, `Newtonsoft.Json` (old bundle stack)
- All other packages.config entries (30+ legacy packages)

**Added:**
- `Microsoft.EntityFrameworkCore.SqlServer 9.0.5`
- `Microsoft.EntityFrameworkCore.Tools 9.0.5`
- `Newtonsoft.Json 13.0.3`

#### Tasks
- `.NET Dependency Management Guide` — Used to guide conversion from `packages.config` to `PackageReference` format, SDK-style project file structure, and package version management

#### Knowledge Base Applied

1 migration guideline was applied covering:

| Migration Area | Description |
|----------------|-------------|
| Dependency Management | `packages.config` → SDK-style `PackageReference` conversion |
| Project File Structure | Legacy MSBuild format → `<Project Sdk="Microsoft.NET.Sdk.Web">` |
| Framework Migration | .NET Framework 4.8 → .NET 10 target framework identification |

#### Issues Fixed During Migration
| Severity | Issue | Resolution |
|----------|-------|------------|
| Major | `TryUpdateModel` not available in ASP.NET Core MVC | Replaced with explicit form parameter binding in `InstructorsController.Edit` POST action |
| Major | `Program.cs` missing explicit `using` directives (WebApplication not found) | Added `using Microsoft.AspNetCore.Builder` and other ASP.NET Core namespaces |
| Minor | `Views/Departments/Create.cshtml` still had `@Scripts.Render("~/bundles/jqueryval")` | Replaced with `<partial name="_ValidationScriptsPartial" />` |
| Minor | `HomeController.Unauthorized()` hides `ControllerBase.Unauthorized()` (CS0114 warning) | Non-breaking; left as-is (method serves a different purpose in this app) |

</details>

---

# Windows Authentication to Azure Managed Identity (DefaultAzureCredential) Migration Result

> **Executive Summary**\
> The ContosoUniversity application has been successfully migrated from Windows/SQL authentication to Azure Managed Identity using `DefaultAzureCredential`. All connection strings have been updated to use `Authentication=Active Directory Default`, removing `Integrated Security=True` and any password-based credentials. The `NotificationService` was also migrated from an in-memory queue to Azure Service Bus using `DefaultAzureCredential`, completing the full token-based authentication posture across all Azure services.

## 1. Migration Improvements

Successfully migrated from Windows authentication (Integrated Security) and connection-string-based authentication to Azure Managed Identity using `DefaultAzureCredential`. The migration replaces `Integrated Security=True` in SQL connection strings with `Authentication=Active Directory Default`, and replaces the in-memory notification queue with Azure Service Bus using `DefaultAzureCredential`. All dependencies, configuration, and implementation code have been updated.

| Area | Before | After | Improvement |
|------|--------|-------|-------------|
| SQL Authentication | `Integrated Security=True` (Windows auth / LocalDB) | `Authentication=Active Directory Default` (Azure AD token via `DefaultAzureCredential`) | Passwordless; no domain dependency; works on any OS |
| Authentication & Security | Windows credentials embedded in connection string | `DefaultAzureCredential` with token rotation | No secrets in config; supports System/User-assigned Managed Identity + local developer credentials |
| Notification Queue | In-memory `ConcurrentQueue<T>` (non-persistent singleton) | Azure Service Bus via `DefaultAzureCredential` | Durable, scalable, cross-instance message delivery |
| Service Bus Auth | N/A (in-memory only) | `ServiceBusClient(namespace, new DefaultAzureCredential())` | Token-based, no connection string keys |
| Dependencies | No Azure SDK | `Azure.Identity 1.14.0`, `Microsoft.Data.SqlClient 5.2.2`, `Azure.Messaging.ServiceBus 7.19.0` | Official Azure SDK stack |
| Configuration | LocalDB connection string with Windows auth | Azure SQL + Service Bus endpoints; placeholder values for deployment | Cloud-native configuration; no credentials in source |
| Maintainability | Credential rotation requires code/config changes | Token-based; rotated automatically by Azure | Reduced operational overhead |

## 2. Build and Validation

All source files compiled successfully with the new Azure SDK dependencies. No test projects exist in the solution (0 tests — N/A). The build was clean with zero errors in a single round.

#### Build Validation
| Field | Value |
|-------|-------|
| Status | ✅ Success |
| Build Tool | dotnet build (MSBuild) |
| Result | 1/1 projects built successfully, 0 errors, 104 warnings (pre-existing nullable reference type warnings — non-blocking) |

#### Test Validation
| Field | Value |
|-------|-------|
| Status | ✅ Success (N/A — no test projects) |
| Total Tests | 0 |
| Passed | 0 |
| Failed | 0 |
| Test Framework | xUnit (no test project in solution) |

#### Code Quality Validation
| Check | Status | Details |
|-------|--------|---------|
| CVE Scan | ✅ Success | `Azure.Identity 1.14.0` — all CVEs affect <1.11.4 ✅; `Microsoft.Data.SqlClient 5.2.2` — all CVEs affect <5.1.3 ✅; `Newtonsoft.Json 13.0.3` — CVE affects <13.0.1 ✅; no vulnerable packages |
| Consistency Check | ✅ Success | 0 Critical, 0 Major, 1 Minor: `SendNotification` uses sync-over-async (`.GetAwaiter().GetResult()`) — non-breaking design concern |
| Completeness Check | ✅ Success | 0 issues — no `Integrated Security=True`, passwords, account keys, or Windows auth patterns remain in any file |

## 3. Recommended Next Steps

I. **Deploy to Azure**: Ensure the target Azure App Service or AKS workload has a System-assigned (or User-assigned) Managed Identity enabled, with the appropriate RBAC roles assigned.

II. **Configure Azure Resources**: Replace `<your-server>` placeholder in `appsettings.json` with your actual Azure SQL server name, and `<YOUR_SERVICE_BUS_NAMESPACE>` with your Service Bus namespace. Grant the Managed Identity `db_datareader`/`db_datawriter` on Azure SQL and `Azure Service Bus Data Sender`/`Receiver` roles on the Service Bus.

III. **Set Up Authentication for Local Development**: Run `az login` (Azure CLI) or sign in to Visual Studio so `DefaultAzureCredential` can acquire tokens during local development. Update `appsettings.Development.json` with your dev Azure SQL server.

IV. **Create Pull Request**: Submit branch `appmod/dotnet-migration-20260514105229` for code review before merging to `main`.

V. **Save as Custom Skill**: To reuse this migration pattern in other projects, save as `My Skill` from the `Tasks` section in the sidebar.

## 4. Additional Details

<details><summary>Click to expand for migration details</summary>

#### Project Details
| Field | Value |
|-------|-------|
| Session ID | `20260514105229` |
| Migration executed by | xuycao |
| Migration performed by | GitHub Copilot |
| Project Pathname | C:\Users\xuycao\dev\testrepo\dotnet-migration-copilot-samples\ContosoUniversity |
| Language | Dotnet |
| Files modified | 6 |
| Branch | `appmod/dotnet-migration-20260514105229` |

#### Version Control Summary
| Field | Value |
|-------|-------|
| Version Control System | Git |
| Total Commits | 1 |
| Uncommitted Changes | None |

**Commits:**
1. Code migration: Replace Windows auth with Azure Managed Identity (DefaultAzureCredential) - Add Azure.Identity 1.14.0 and Microsoft.Data.SqlClient 5.2.2; update SQL connection strings to use Authentication=Active Directory Default; migrate NotificationService to Azure Service Bus with DefaultAzureCredential (43fa5c9f7937c8e91a75f19bb7e61ecbba53227a)

#### Code Changes

**Build Files (1)**
- `ContosoUniversity.csproj` — Added `Azure.Identity 1.14.0`, `Microsoft.Data.SqlClient 5.2.2`, `Azure.Messaging.ServiceBus 7.19.0`

**Configuration Files (2)**
- `appsettings.json` — Connection string: `Integrated Security=True` → `Authentication=Active Directory Default`; added `AzureServiceBus` section
- `appsettings.Development.json` — Added dev connection string with `Authentication=Active Directory Default`

**Source Files (3)**
- `Services/NotificationService.cs` — In-memory `ConcurrentQueue` → `Azure.Messaging.ServiceBus.ServiceBusClient` with `DefaultAzureCredential`
- `Controllers/NotificationsController.cs` — Updated `GetNotifications` to `async Task<JsonResult>` using `ReceiveNotificationsAsync`
- `Program.cs` — Updated DI registration comment for Service Bus

#### Dependency Changes

**Removed:**
- `Integrated Security=True` from SQL connection string (Windows authentication)
- In-memory `ConcurrentQueue<Notification>` (non-persistent notification queue)

**Added:**
- `Azure.Identity 1.14.0` — DefaultAzureCredential token acquisition
- `Microsoft.Data.SqlClient 5.2.2` — `Authentication=Active Directory Default` SQL auth keyword support
- `Azure.Messaging.ServiceBus 7.19.0` — Durable message queue via Azure Service Bus

#### Tasks
- `Managed Identity Migration Guide` — Guided connection string format change to `Authentication=Active Directory Default`, `Azure.Identity` package addition, and Service Bus passwordless authentication pattern

#### Knowledge Base Applied

1 migration guideline was applied covering:

| Migration Area | Description |
|----------------|-------------|
| SQL Authentication | `Integrated Security=True` → `Authentication=Active Directory Default` with `DefaultAzureCredential` |
| Service Bus Authentication | Connection string with SharedAccessKey → `ServiceBusClient(namespace, DefaultAzureCredential)` |
| Dependency Addition | `Azure.Identity 1.14.0` and `Microsoft.Data.SqlClient 5.2.2` per KB specification |

#### Issues Fixed During Migration
| Severity | Issue | Resolution |
|----------|-------|------------|
| Minor | `SendNotification` uses `.GetAwaiter().GetResult()` (sync-over-async) | Accepted as-is; method signature is synchronous and caller context supports blocking; not a Managed Identity migration concern |

</details>
