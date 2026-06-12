# Migration Plan: ContosoUniversity — .NET Framework 4.8 → .NET 8

## Executive Summary

| Attribute | Current | Target |
|-----------|---------|--------|
| Runtime | .NET Framework 4.8 | .NET 8 LTS |
| Web Framework | ASP.NET MVC 5 | ASP.NET Core 8 MVC |
| ORM | EF Core 3.1 (on Framework) | EF Core 8.x |
| Messaging | MSMQ (System.Messaging) | Channel-based in-memory (dev) / Azure Service Bus (prod) |
| Config | Web.config / ConfigurationManager | appsettings.json / IConfiguration |
| DI | Manual (SchoolContextFactory) | Built-in ASP.NET Core DI |
| Auth | Windows Auth (IIS Express) | ASP.NET Core Identity (optional) |
| Client | Bootstrap 5.3 + jQuery 3.7 via bundles | wwwroot static files + LibMan/npm |
| Tests | None | xUnit + WebApplicationFactory integration tests |

**Estimated effort**: 3–5 days for an experienced developer.  
**Risk level**: Medium — MSMQ replacement and view migration are the largest tasks.

---

## Phase 1: Project & Infrastructure (SDK-Style Project)

**Goal**: Replace the legacy `.csproj` and `packages.config` with a minimal SDK-style project targeting `net8.0`.

### Actions
1. Create new `ContosoUniversity.csproj` (SDK-style)
2. Create `Program.cs` with minimal hosting model
3. Delete `Global.asax`, `Global.asax.cs`, `packages.config`, `Properties/AssemblyInfo.cs`
4. Delete `Web.config` (root and Views)
5. Remove `App_Start/` folder (BundleConfig, FilterConfig, RouteConfig)

### Before (Legacy .csproj — excerpt)
```xml
<Project ToolsVersion="15.0" DefaultTargets="Build"
         xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
    <ProjectTypeGuids>{349c5851-...}</ProjectTypeGuids>
  </PropertyGroup>
  <!-- 100+ lines of ItemGroup references -->
</Project>
```

### After (SDK-style)
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.11" />
  </ItemGroup>
</Project>
```

### After (Program.cs — minimal hosting)
```csharp
using ContosoUniversity.Data;
using ContosoUniversity.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<INotificationService, InMemoryNotificationService>();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SchoolContext>();
    DbInitializer.Initialize(context);
}

app.Run();
```

---

## Phase 2: Data Layer (EF Core 8 Upgrade)

**Goal**: Upgrade EF Core 3.1 → 8.x. The existing `SchoolContext` and models are already EF Core-compatible — minimal changes needed.

### Actions
1. Update `SchoolContext` — remove `SchoolContextFactory`, rely on DI
2. Update model configurations for EF Core 8 conventions
3. Generate initial EF Core migration
4. Update `DbInitializer` to work with DI-provided context

### Before (SchoolContextFactory — manual creation)
```csharp
public class SchoolContextFactory
{
    public static SchoolContext Create()
    {
        var connectionString = ConfigurationManager
            .ConnectionStrings["DefaultConnection"].ConnectionString;
        var optionsBuilder = new DbContextOptionsBuilder<SchoolContext>();
        optionsBuilder.UseSqlServer(connectionString);
        return new SchoolContext(optionsBuilder.Options);
    }
}
```

### After (DI-injected — no factory needed)
```csharp
// Registered in Program.cs:
builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers receive via constructor injection
public class StudentsController : Controller
{
    private readonly SchoolContext _context;
    public StudentsController(SchoolContext context) => _context = context;
}
```

---

## Phase 3: Configuration & Dependency Injection

**Goal**: Replace `Web.config` / `ConfigurationManager` with `appsettings.json` and the Options pattern.

### Actions
1. Create `appsettings.json` and `appsettings.Development.json`
2. Map connection strings and app settings
3. Register all services in `Program.cs`
4. Remove all `ConfigurationManager` references

### Before (Web.config)
```xml
<connectionStrings>
  <add name="DefaultConnection" connectionString="Data Source=(LocalDb)\MSSQLLocalDB;..." />
</connectionStrings>
<appSettings>
  <add key="NotificationQueuePath" value=".\Private$\ContosoUniversityNotifications"/>
</appSettings>
```

### After (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ContosoUniversity;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Notifications": {
    "Provider": "InMemory"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

---

## Phase 4: MSMQ Replacement

**Goal**: Replace `System.Messaging` (MSMQ) with a cross-platform abstraction. Use `System.Threading.Channels` for development, with an option to swap in Azure Service Bus for production.

### Actions
1. Define `INotificationService` interface
2. Implement `InMemoryNotificationService` using `Channel<Notification>`
3. (Optional) Implement `AzureServiceBusNotificationService`
4. Delete `NotificationService.cs` (MSMQ-based)
5. Register via DI with provider selection

### Before (MSMQ — Windows-only)
```csharp
using System.Messaging;

public class NotificationService
{
    private readonly MessageQueue _queue;
    public NotificationService()
    {
        _queuePath = ConfigurationManager.AppSettings["NotificationQueuePath"];
        if (!MessageQueue.Exists(_queuePath))
            _queue = MessageQueue.Create(_queuePath);
        else
            _queue = new MessageQueue(_queuePath);
    }
    public void SendNotification(...) { _queue.Send(message); }
}
```

### After (Cross-platform Channel-based)
```csharp
public interface INotificationService
{
    Task SendNotificationAsync(string entityType, string entityId,
        string? displayName, EntityOperation operation, string? userName = null);
    Task<Notification?> ReceiveNotificationAsync(CancellationToken ct = default);
    Task<List<Notification>> GetRecentNotificationsAsync(int count = 50);
}

public class InMemoryNotificationService : INotificationService
{
    private readonly Channel<Notification> _channel =
        Channel.CreateBounded<Notification>(1000);
    private readonly List<Notification> _history = new();

    public async Task SendNotificationAsync(...)
    {
        var notification = new Notification { /* ... */ };
        _history.Add(notification);
        await _channel.Writer.WriteAsync(notification);
    }

    public async Task<Notification?> ReceiveNotificationAsync(CancellationToken ct)
    {
        if (await _channel.Reader.WaitToReadAsync(ct))
            return await _channel.Reader.ReadAsync(ct);
        return null;
    }

    public Task<List<Notification>> GetRecentNotificationsAsync(int count)
        => Task.FromResult(_history.TakeLast(count).Reverse().ToList());
}
```

---

## Phase 5: MVC → ASP.NET Core MVC

**Goal**: Convert controllers from `System.Web.Mvc` to `Microsoft.AspNetCore.Mvc`. Migrate Razor views from MVC 5 syntax to ASP.NET Core Tag Helpers.

### Actions
1. Update all controllers to inherit from `Microsoft.AspNetCore.Mvc.Controller`
2. Replace `BaseController` with constructor DI
3. Convert `ActionResult` patterns (mostly compatible)
4. Replace `@Html.ActionLink` with `<a asp-controller asp-action>` Tag Helpers
5. Replace `@Styles.Render` / `@Scripts.Render` with `<link>` / `<script>` tags
6. Create `_ViewImports.cshtml` with Tag Helper imports
7. Update `_Layout.cshtml` for ASP.NET Core conventions

### Before (Controller)
```csharp
using System.Web.Mvc;

public class StudentsController : BaseController
{
    public ActionResult Index(...) { /* uses db field from BaseController */ }
}
```

### After (Controller)
```csharp
using Microsoft.AspNetCore.Mvc;

public class StudentsController : Controller
{
    private readonly SchoolContext _context;
    private readonly INotificationService _notifications;

    public StudentsController(SchoolContext context, INotificationService notifications)
    {
        _context = context;
        _notifications = notifications;
    }

    public async Task<IActionResult> Index(...) { /* uses _context */ }
}
```

### Before (View — _Layout.cshtml)
```html
@Styles.Render("~/Content/css")
@Scripts.Render("~/bundles/jquery")
@Html.ActionLink("Students", "Index", "Students", null, new { @class = "nav-link" })
```

### After (View — _Layout.cshtml)
```html
<link rel="stylesheet" href="~/css/site.css" />
<script src="~/lib/jquery/jquery.min.js"></script>
<a asp-controller="Students" asp-action="Index" class="nav-link">Students</a>
```

---

## Phase 6: Client Assets & Styling

**Goal**: Move static files from `Content/` and `Scripts/` to `wwwroot/` following ASP.NET Core conventions.

### Actions
1. Create `wwwroot/css/`, `wwwroot/js/`, `wwwroot/lib/`
2. Move `Site.css`, `notifications.css` → `wwwroot/css/`
3. Move `notifications.js` → `wwwroot/js/`
4. Use LibMan or npm for Bootstrap and jQuery
5. Delete `Scripts/`, `Content/`, bundle config
6. Remove WebGrease, Modernizr, Antlr dependencies

### File Structure (After)
```
wwwroot/
├── css/
│   ├── site.css
│   └── notifications.css
├── js/
│   └── notifications.js
└── lib/
    ├── bootstrap/
    │   ├── css/bootstrap.min.css
    │   └── js/bootstrap.bundle.min.js
    ├── jquery/
    │   └── jquery.min.js
    └── jquery-validation/
        └── jquery.validate.min.js
```

---

## Phase 7: Authentication & Security

**Goal**: Replace Windows Authentication with ASP.NET Core middleware. Add CSRF protection (built-in with Tag Helpers).

### Actions
1. Configure `app.UseAuthentication()` / `app.UseAuthorization()` in pipeline
2. Anti-forgery tokens are automatic with Tag Helpers `<form>` — verify all forms
3. Add HTTPS redirection (already in template)
4. (Optional) Add ASP.NET Core Identity if real auth is needed
5. Replace `"System"` user placeholder with actual user context

### Notes
- The current app uses Windows Auth at IIS level only — no `[Authorize]` attributes
- For dev/demo purposes, anonymous access is acceptable
- CSRF: ASP.NET Core automatically validates `[ValidateAntiForgeryToken]` when using Tag Helpers

---

## Phase 8: Testing & Validation

**Goal**: Add automated tests to validate the migration is functionally equivalent.

### Actions
1. Create `ContosoUniversity.Tests` project (xUnit)
2. Add integration tests using `WebApplicationFactory<Program>`
3. Test all CRUD endpoints return 200
4. Test database seeding works
5. Test notification service (in-memory)
6. Validate build with `dotnet build --warnaserror`

### Test Example
```csharp
public class StudentEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public StudentEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_StudentsIndex_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Students");
        response.EnsureSuccessStatusCode();
    }
}
```

---

## Phase 9: Cleanup & Optimization

**Goal**: Remove all legacy artifacts and resolve warnings.

### Actions
1. Delete: `Global.asax`, `Global.asax.cs`, `packages.config`, `AssemblyInfo.cs`
2. Delete: `Web.config` (root + Views), `App_Start/` folder
3. Delete: `SchoolContextFactory.cs`
4. Delete: `Scripts/` and `Content/` folders (moved to wwwroot)
5. Remove `System.Messaging` reference
6. Run `dotnet format` to standardize code style
7. Ensure zero build warnings
8. Update solution file for new project structure

---

## Dependency Migration Map

| Legacy Package | .NET 8 Replacement |
|---|---|
| Microsoft.AspNet.Mvc 5.2.9 | Built-in (Microsoft.NET.Sdk.Web) |
| Microsoft.AspNet.Razor 3.2.9 | Built-in |
| Microsoft.AspNet.WebPages 3.2.9 | Built-in |
| Microsoft.AspNet.Web.Optimization 1.1.3 | Removed (use static files) |
| Microsoft.EntityFrameworkCore 3.1.32 | 8.0.11 |
| Microsoft.EntityFrameworkCore.SqlServer 3.1.32 | 8.0.11 |
| Microsoft.Data.SqlClient 2.1.4 | Transitive via EF Core 8 |
| Newtonsoft.Json 13.0.3 | System.Text.Json (built-in) |
| jQuery 3.7.1 | wwwroot/lib (LibMan) |
| Bootstrap 5.3.3 | wwwroot/lib (LibMan) |
| Modernizr 2.6.2 | Removed (unnecessary) |
| WebGrease 1.5.2 | Removed |
| Antlr 3.4.1 | Removed |
| System.Messaging | System.Threading.Channels |

---

## Risk Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| MSMQ removal breaks notifications | High | Interface abstraction; in-memory impl preserves behavior |
| EF Core 3.1 → 8 breaking changes | Medium | Minimal — models already use EF Core conventions |
| View syntax differences | Medium | Methodical find-replace of @Html helpers → Tag Helpers |
| No existing tests | High | Build integration tests before migration to establish baseline |
| 44 NuGet packages | Low | Most are transitive; only ~6 direct packages needed in .NET 8 |
