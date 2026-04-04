# .NET 10.0 Upgrade Plan - ContosoUniversity

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Plans](#project-by-project-plans)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Risk Management](#risk-management)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

## Executive Summary

### Scenario Description

This plan outlines the migration of the **ContosoUniversity** ASP.NET Framework 4.8 application to **ASP.NET Core on .NET 10.0 (LTS)**. This is a comprehensive architectural migration, not just a framework version upgrade.

### Scope

**Projects Affected**: 1 project (ContosoUniversity.csproj)

**Current State**:
- Target Framework: .NET Framework 4.8
- Project Type: ASP.NET MVC Web Application (classic .csproj format)
- Codebase: 3,392 lines of code across 56 files
- Dependencies: 45 NuGet packages
- Technologies: ASP.NET MVC, Entity Framework Core 3.1, MSMQ (System.Messaging)

**Target State**:
- Target Framework: .NET 10.0
- Project Type: ASP.NET Core Web Application (SDK-style .csproj)
- Technologies: ASP.NET Core MVC, Entity Framework Core 10.0, modern message queue (RabbitMQ/Azure Service Bus)

### Selected Strategy

**All-At-Once Strategy** - Single comprehensive migration operation

**Rationale**:
- Single project (no dependency ordering concerns)
- Complete architectural shift requires coordinated changes across all layers
- Incremental migration not practical when changing from ASP.NET Framework to ASP.NET Core
- All changes interdependent (project format, framework, packages, code patterns)

### Complexity Assessment

**Discovered Metrics**:
- **API Compatibility**: 92 issues (63 binary incompatible, 29 source incompatible)
- **Package Updates**: 26 of 45 packages require updates/replacement
- **Security Issues**: 1 package with known vulnerabilities (Microsoft.Data.SqlClient 2.1.4)
- **Code Impact**: Estimated 92+ lines requiring modification (2.7% minimum)
- **Architectural Changes**: Major (ASP.NET Framework → ASP.NET Core)

**Classification: Critical Complexity**

**Key Risk Factors**:
1. **Architectural Migration**: Complete redesign from System.Web to ASP.NET Core
2. **MSMQ Dependency**: System.Messaging not available in .NET Core - requires infrastructure replacement
3. **Framework-Specific Features**: Bundling/minification, routing, Global.asax patterns must be replaced
4. **Security Vulnerabilities**: Must upgrade Microsoft.Data.SqlClient from 2.1.4 to 7.0.0
5. **Legacy Configuration**: web.config → appsettings.json + environment variables

### Critical Issues

**Immediate Security Concerns**:
- Microsoft.Data.SqlClient 2.1.4 contains security vulnerabilities → upgrade to 7.0.0

**Blocking Technical Issues**:
- System.Messaging (MSMQ) is not supported in .NET Core → requires alternative implementation
- System.Web.Optimization (bundling/minification) → requires replacement strategy
- ASP.NET Framework routing patterns → must convert to ASP.NET Core endpoint routing
- Global.asax application lifecycle → must convert to Program.cs/Startup.cs patterns

### Recommended Approach

**Phase 0: Preparation**
- Install .NET 10.0 SDK
- Set up new ASP.NET Core project structure
- Plan MSMQ replacement strategy

**Phase 1: Atomic Migration**
- Convert project to SDK-style format
- Update all framework and package references
- Replace incompatible packages and patterns
- Migrate configuration system
- Convert routing and bundling
- Fix all compilation errors

**Phase 2: Testing & Validation**
- Build verification
- Functional testing
- MSMQ replacement validation
- Security vulnerability verification

### Expected Remaining Iterations

- **Iteration 2.1**: Dependency Analysis (Foundation)
- **Iteration 2.2**: Migration Strategy Details (Foundation)
- **Iteration 2.3**: Project Stubs & Risk Overview (Foundation)
- **Iteration 3.1**: MSMQ Replacement Strategy (High-Risk Feature)
- **Iteration 3.2**: ASP.NET Core Conversion Details (High-Risk Feature)
- **Iteration 3.3**: Package Updates & Breaking Changes (Detail)
- **Iteration 3.4**: Configuration & Routing Migration (Detail)
- **Iteration 3.5**: Final Sections (Success Criteria, Source Control)

## Migration Strategy

### Approach Selection

**Selected: All-At-Once Strategy**

**Justification**:
1. **Single Project**: No dependency coordination needed
2. **Architectural Change**: ASP.NET Framework → ASP.NET Core is not incremental
3. **Interdependent Changes**: Project format, framework, packages, and code patterns all change together
4. **No Multi-Targeting**: Cannot run ASP.NET MVC and ASP.NET Core simultaneously
5. **Clean Migration**: Side-by-side approach would require duplicating entire application

**Alternative Considered**: Incremental with System.Web.Adapters
- **Rejected because**: Adapters are temporary bridge, not long-term solution; adds complexity without reducing risk for single-project solution

### All-At-Once Strategy Rationale

**Why This Strategy Fits**:
- Small solution (1 project, ~3400 LOC)
- Clear migration path (ASP.NET Framework → ASP.NET Core is well-documented)
- All changes validated in single comprehensive testing phase
- Faster total completion time
- No interim states to maintain

**Risk Factors Specific to All-At-Once**:
- Larger testing surface (all changes at once)
- Cannot validate framework upgrade separately from package upgrades
- MSMQ replacement impact affects entire application
- All developers must adapt to ASP.NET Core patterns simultaneously

**Mitigation**:
- Comprehensive breaking changes catalog (see section below)
- Detailed test strategy with multiple validation levels
- MSMQ abstraction layer to isolate message queue implementation
- Clear rollback plan (revert entire branch)

### Dependency-Based Ordering

**Not Applicable** - Single project has no inter-project dependencies.

Within the project, migration order follows these principles:

1. **Project Infrastructure First**
   - Convert to SDK-style project
   - Update target framework
   - Update package references

2. **Foundation Libraries Second**
   - Configuration system migration
   - Dependency injection setup
   - Logging infrastructure

3. **Application Code Third**
   - Controllers and views
   - Models and data access
   - Business logic

4. **Integration Features Last**
   - MSMQ replacement
   - Bundling/minification replacement
   - Routing configuration

### Execution Approach

**Atomic Operation**: All updates applied in single coordinated batch

**Sequence**:
1. Convert project file to SDK-style format
2. Update TargetFramework to net10.0
3. Update all package references (remove incompatible, upgrade existing, add new)
4. Replace incompatible patterns (System.Web → ASP.NET Core)
5. Migrate configuration (web.config → appsettings.json)
6. Convert routing (RouteCollection → endpoint routing)
7. Replace bundling (System.Web.Optimization → direct references or build-time bundler)
8. Implement MSMQ replacement
9. Convert Global.asax to Program.cs
10. Fix all compilation errors
11. Build and verify

**No Parallel Work**: Single project, all changes sequential

### Phase Definitions

This migration has **no phases** in the traditional sense (no Tier 1, Tier 2, etc.). Instead, it has a single comprehensive migration with logical groupings:

**Phase 0: Preparation**
- Verify .NET 10.0 SDK installed
- Review MSMQ replacement options
- Backup current state (Git branch already created)
- Document current application behavior

**Phase 1: Atomic Migration** (All changes combined)
- Project conversion
- Framework and package updates
- Code pattern replacements
- Configuration migration
- Feature migrations (MSMQ, bundling, routing)
- Compilation error fixes
- Build verification

**Phase 2: Testing & Validation**
- Unit tests (if present)
- Integration tests
- Manual functional testing
- Security vulnerability verification
- Performance validation

**Phase 3: Deployment Preparation** (Out of scope for this plan)
- Deployment configuration
- Environment setup
- Rollout strategy

## Detailed Dependency Analysis

### Dependency Graph Summary

ContosoUniversity is a **standalone project** with no project-to-project dependencies.

```
ContosoUniversity.csproj (net48 → net10.0)
├── No project dependencies
└── No dependent projects
```

**Implications**:
- No dependency ordering concerns
- No multi-targeting requirements
- All changes can be applied in single atomic operation
- No risk of breaking other projects

### Project Grouping

**Single Migration Unit**: ContosoUniversity.csproj

All migration activities will be performed on this one project simultaneously:
- Project file conversion
- Framework update
- Package updates
- Code modifications
- Configuration migration
- Feature replacements

### Critical Path

**Single Linear Path**: Preparation → Migration → Testing

No parallel execution possible (single project). All steps must be performed in sequence within the atomic migration phase.

### External Dependencies

**Infrastructure Dependencies**:
- **MSMQ**: Currently uses Windows Message Queuing (System.Messaging)
  - Migration impact: HIGH
  - Requires replacement with modern message queue (RabbitMQ, Azure Service Bus, etc.)
  - Decision needed: Which message queue technology to adopt

**Database Dependencies**:
- SQL Server (via Entity Framework Core)
- Migration impact: MEDIUM
- EF Core 3.1 → EF Core 10.0 upgrade required
- Database migrations may need regeneration

**Configuration Dependencies**:
- web.config → appsettings.json conversion
- Migration impact: MEDIUM
- All configuration keys must be mapped to new system

## Project-by-Project Plans

### Project: ContosoUniversity.csproj

**Current State**:
- Target Framework: net48
- Project Type: ASP.NET MVC Web Application (classic .csproj)
- SDK-Style: No (old-style project file)
- Dependencies: 45 NuGet packages
- Lines of Code: 3,392
- Code Files: 56
- Files with Issues: 15
- Risk Level: **Critical**

**Target State**:
- Target Framework: net10.0
- Project Type: ASP.NET Core MVC Web Application (SDK-style .csproj)
- SDK-Style: Yes
- Dependencies: ~35 packages (after removals/consolidations)
- Expected Code Changes: 92+ lines minimum (2.7%)

#### Migration Steps

##### 1. Prerequisites

**Required Tools**:
- .NET 10.0 SDK installed and verified
- Visual Studio 2022 (17.12 or later) or VS Code with C# Dev Kit
- Git (branch `upgrade-to-NET10` already created)

**Required Decisions**:
- **MSMQ Replacement Technology**: Choose one of:
  - RabbitMQ (popular, open-source)
  - Azure Service Bus (cloud-native, enterprise features)
  - In-memory queue (development/testing only)
  - MassTransit/NServiceBus (abstraction layer)

**Dependencies**: None (standalone project)

##### 2. Project File Conversion

**Convert to SDK-Style Format**:

Current project file is old-style .csproj (references every file, verbose XML). Must convert to SDK-style (minimal, glob-based).

**Steps**:
1. Use upgrade assistant or manual conversion
2. Replace `<Project>` element: `<Project Sdk="Microsoft.NET.Sdk.Web">`
3. Remove explicit file references (replaced by glob patterns)
4. Update TargetFramework: `<TargetFramework>net10.0</TargetFramework>`
5. Remove obsolete elements (AssemblyInfo settings, web.config transforms)
6. Migrate build configurations

**Key Changes**:
```xml
<!-- OLD (Classic) -->
<Project ToolsVersion="15.0" ...>
  <PropertyGroup>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Controllers\HomeController.cs" />
    <!-- hundreds of lines -->
  </ItemGroup>
</Project>

<!-- NEW (SDK-Style) -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <!-- Files included automatically by glob patterns -->
</Project>
```

##### 3. Package Updates

See [Package Update Reference](#package-update-reference) for complete matrix.

**Summary of Changes**:
- **Remove**: 10 packages (functionality now in framework)
- **Replace**: 2 packages (incompatible with .NET Core)
- **Upgrade**: 24 packages (to .NET 10.0 compatible versions)
- **Add**: ~5 new packages (ASP.NET Core specific)
- **Keep**: 9 packages (already compatible)

**Critical Security Update**:
- Microsoft.Data.SqlClient: 2.1.4 → 7.0.0 (addresses vulnerabilities)

##### 4. MSMQ Migration (High-Risk)

**Current Implementation**:
- Uses System.Messaging (59 API calls across codebase)
- MessageQueue creation, sending, receiving
- Message formatters, priorities, access rights
- Queue management (Create, Exists, SetPermissions)

**Migration Strategy**:

**Option A: Abstraction Layer** (Recommended)
1. Create `IMessageQueue` interface abstracting message queue operations
2. Implement `MsmqMessageQueue` wrapper (temporary, Windows-only)
3. Implement target queue adapter (RabbitMQ, Azure Service Bus, etc.)
4. Update all System.Messaging calls to use interface
5. Switch implementation via dependency injection

**Option B: Direct Replacement**
1. Choose target message queue technology
2. Install NuGet packages (e.g., RabbitMQ.Client, Azure.Messaging.ServiceBus)
3. Replace all System.Messaging calls directly
4. Update message serialization (XmlMessageFormatter → JSON/Protocol Buffers)
5. Update queue naming conventions (Windows paths → queue names)

**Abstraction Layer Example**:
```csharp
// Interface
public interface IMessageQueue
{
    void Send(object message, MessagePriority priority = MessagePriority.Normal);
    T Receive<T>(TimeSpan timeout);
    bool Exists(string queueName);
    void Create(string queueName);
}

// ASP.NET Core registration (Program.cs)
builder.Services.AddSingleton<IMessageQueue, RabbitMqMessageQueue>();
// Or: builder.Services.AddSingleton<IMessageQueue, AzureServiceBusMessageQueue>();
```

**Code Changes Required** (59 locations):
- Replace `MessageQueue` instantiation with `IMessageQueue` injection
- Replace `MessageQueueAccessRights` with abstracted permissions model
- Replace `XmlMessageFormatter` with JSON serialization
- Replace Windows queue paths (`.\Private$\queue`) with logical queue names
- Update exception handling (`MessageQueueException` → abstracted exceptions)

##### 5. ASP.NET Framework → ASP.NET Core Conversion

**Global.asax → Program.cs**:

Current Global.asax.cs handles:
- Application start (Application_Start)
- Session start/end
- Request begin/end
- Error handling (Application_Error)

Target Program.cs pattern:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Services configuration (replaces Application_Start registrations)
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolContext")));

// Message queue abstraction
builder.Services.AddSingleton<IMessageQueue, RabbitMqMessageQueue>();

var app = builder.Build();

// Middleware pipeline (replaces Global.asax event handlers)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Endpoint routing (replaces RouteCollection)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

**Routing Migration**:

Current (ASP.NET MVC):
```csharp
RouteTable.Routes.MapRoute(
    name: "Default",
    url: "{controller}/{action}/{id}",
    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
);
```

Target (ASP.NET Core):
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

**Bundling & Minification**:

Current (System.Web.Optimization):
```csharp
bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
    "~/Scripts/jquery-{version}.js"));
bundles.Add(new StyleBundle("~/Content/css").Include(
    "~/Content/bootstrap.css",
    "~/Content/site.css"));
```

Target Options:
- **Option 1**: Direct HTML references (simplest)
  ```html
  <script src="~/lib/jquery/dist/jquery.min.js"></script>
  <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
  ```
- **Option 2**: Build-time bundling (Webpack, Vite, esbuild)
- **Option 3**: ASP.NET Core bundling libraries (WebOptimizer)

Recommended: **Option 1** for initial migration (simplest), consider build-time bundling later.

##### 6. Configuration Migration

**web.config → appsettings.json**:

Current (web.config):
```xml
<configuration>
  <appSettings>
    <add key="Setting1" value="Value1" />
  </appSettings>
  <connectionStrings>
    <add name="SchoolContext" connectionString="..." providerName="..." />
  </connectionStrings>
</configuration>
```

Target (appsettings.json):
```json
{
  "Settings": {
    "Setting1": "Value1"
  },
  "ConnectionStrings": {
    "SchoolContext": "..."
  }
}
```

**Code Changes**:
```csharp
// OLD
var setting = ConfigurationManager.AppSettings["Setting1"];
var connStr = ConfigurationManager.ConnectionStrings["SchoolContext"].ConnectionString;

// NEW (inject IConfiguration)
private readonly IConfiguration _configuration;
var setting = _configuration["Settings:Setting1"];
var connStr = _configuration.GetConnectionString("SchoolContext");
```

##### 7. Expected Breaking Changes

See [Breaking Changes Catalog](#breaking-changes-catalog) for comprehensive list.

**Major Categories**:
1. **System.Web APIs** (16 issues) - HttpContext, HttpPostedFileBase, routing
2. **System.Messaging** (59 issues) - Complete MSMQ replacement
3. **Configuration** (16 issues) - ConfigurationManager → IConfiguration
4. **Type/API Changes** - Binary incompatibilities in 63 APIs

##### 8. Code Modifications

**Namespace Changes**:
- `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`
- `System.Web.Routing` → `Microsoft.AspNetCore.Routing`
- `System.Configuration` → `Microsoft.Extensions.Configuration`

**Controller Changes**:
```csharp
// OLD
using System.Web.Mvc;
public class HomeController : Controller { }

// NEW
using Microsoft.AspNetCore.Mvc;
public class HomeController : Controller { }
```

**File Upload Changes**:
```csharp
// OLD (HttpPostedFileBase)
public ActionResult Upload(HttpPostedFileBase file)
{
    if (file != null && file.ContentLength > 0)
    {
        file.SaveAs(path);
    }
}

// NEW (IFormFile)
public async Task<IActionResult> Upload(IFormFile file)
{
    if (file != null && file.Length > 0)
    {
        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
    }
}
```

**Dependency Injection**:
- ASP.NET Framework: Manual instantiation or constructor injection via DI framework
- ASP.NET Core: Built-in DI, constructor injection everywhere

**View Changes**:
- Minimal changes to Razor views (.cshtml files)
- Update `@using` directives if needed
- Update HTML helper methods (most work as-is)

##### 9. Testing Strategy

**Unit Testing**:
- Run existing unit tests (if present)
- Add tests for MSMQ abstraction layer
- Test configuration access patterns

**Integration Testing**:
- Test database operations (EF Core upgrade)
- Test message queue operations
- Test file upload functionality
- Test all controller actions

**Manual Testing**:
- Navigate all pages
- Test all CRUD operations
- Test error handling
- Test authentication/authorization (if present)

**Security Testing**:
- Verify Microsoft.Data.SqlClient upgrade fixes vulnerabilities
- Test SQL injection prevention
- Validate input validation

##### 10. Validation Checklist

- [ ] Project builds without errors
- [ ] Project builds without warnings
- [ ] All controllers compile
- [ ] All views render
- [ ] Database connection works
- [ ] EF Core migrations apply successfully
- [ ] Message queue abstraction works
- [ ] Configuration values load correctly
- [ ] Static files serve correctly
- [ ] Routing works for all URLs
- [ ] File uploads work
- [ ] No security vulnerabilities in packages
- [ ] Application starts and runs

## Package Update Reference

### Package Update Strategy

**Categories**:
1. **Remove**: Packages whose functionality is now included in .NET 10.0 framework
2. **Replace**: Packages incompatible with .NET Core requiring alternatives
3. **Upgrade**: Packages compatible but requiring newer versions
4. **Add**: New packages required for ASP.NET Core
5. **Keep**: Packages already compatible as-is

### Packages to Remove (10 packages)

These packages are no longer needed because their functionality is included in the .NET 10.0 framework:

| Package | Current Version | Reason |
|---------|----------------|--------|
| Microsoft.AspNet.Mvc | 5.2.9 | ASP.NET Core MVC is built-in |
| Microsoft.AspNet.Razor | 3.2.9 | Razor is built-in to ASP.NET Core |
| Microsoft.AspNet.WebPages | 3.2.9 | Functionality integrated into ASP.NET Core |
| Microsoft.CodeDom.Providers.DotNetCompilerPlatform | 2.0.1 | Not needed in .NET Core |
| Microsoft.Web.Infrastructure | 2.0.1 | ASP.NET Core handles this |
| NETStandard.Library | 2.0.3 | Framework reference handles this |
| System.Buffers | 4.5.1 | Built into .NET 10.0 |
| System.ComponentModel.Annotations | 4.7.0 | Built into .NET 10.0 |
| System.Memory | 4.5.4 | Built into .NET 10.0 |
| System.Numerics.Vectors | 4.5.0 | Built into .NET 10.0 |
| System.Threading.Tasks.Extensions | 4.5.4 | Built into .NET 10.0 |

**Action**: Remove these `<PackageReference>` elements from project file

### Packages to Replace (2 packages)

These packages are incompatible with .NET Core and must be replaced:

| Package | Current Version | Replacement | Replacement Version | Migration Notes |
|---------|----------------|-------------|---------------------|-----------------|
| Antlr | 3.4.1.9004 | Antlr4 | 4.6.6 | Update parser code if using Antlr directly |
| Microsoft.AspNet.Web.Optimization | 1.1.3 | (Remove - use direct HTML) | N/A | Replace with direct script/link tags or build-time bundler |

**Microsoft.AspNet.Web.Optimization Migration**:
- This package provided bundling and minification for ASP.NET Framework
- Not supported in ASP.NET Core
- Options:
  1. Direct HTML references (recommended for initial migration)
  2. WebOptimizer package (ASP.NET Core equivalent)
  3. Build-time bundling (Webpack, Vite, esbuild)

### Packages to Upgrade (24 packages)

These packages are compatible with .NET 10.0 but require version updates:

| Package | Current Version | Target Version | Update Reason |
|---------|----------------|----------------|---------------|
| **Microsoft.EntityFrameworkCore** | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.EntityFrameworkCore.Abstractions | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.EntityFrameworkCore.Analyzers | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.EntityFrameworkCore.Relational | 3.1.32 | 10.0.5 | Framework alignment |
| **Microsoft.EntityFrameworkCore.SqlServer** | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.EntityFrameworkCore.Tools | 3.1.32 | 10.0.5 | Framework alignment |
| **Microsoft.Data.SqlClient** | 2.1.4 | 7.0.0 | **SECURITY: Fixes vulnerabilities** |
| Microsoft.Extensions.Caching.Abstractions | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.Extensions.Caching.Memory | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.Extensions.Configuration | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.Extensions.Configuration.Abstractions | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.Extensions.Configuration.Binder | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.Extensions.DependencyInjection | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.Extensions.DependencyInjection.Abstractions | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.Extensions.Logging | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.Extensions.Logging.Abstractions | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.Extensions.Options | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.Extensions.Primitives | 3.1.32 | 10.0.5 | Framework alignment |
| Microsoft.Bcl.AsyncInterfaces | 1.1.1 | 10.0.5 | Framework alignment |
| Microsoft.Bcl.HashCode | 1.1.1 | 6.0.0 | Latest stable version |
| Newtonsoft.Json | 13.0.3 | 13.0.4 | Latest stable version |
| System.Collections.Immutable | 1.7.1 | 10.0.5 | Framework alignment |
| System.Diagnostics.DiagnosticSource | 4.7.1 | 10.0.5 | Framework alignment |
| System.Runtime.CompilerServices.Unsafe | 4.5.3 | 6.1.2 | Latest stable version |

**Priority Updates**:
- **Microsoft.Data.SqlClient**: Critical security update
- **Entity Framework Core packages**: Must all update together to same version
- **Microsoft.Extensions.*** packages: Should all align to .NET 10.0 version

### Packages to Add (New for ASP.NET Core)

These packages may need to be added for ASP.NET Core functionality:

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation | 10.0.0 | Enable Razor view compilation at runtime (development) |

**Note**: Most ASP.NET Core packages are included via framework reference and don't need explicit PackageReferences.

### Packages to Keep (9 packages)

These packages are already compatible with .NET 10.0:

| Package | Version | Status |
|---------|---------|--------|
| bootstrap | 5.3.3 | ✅ Compatible (static assets) |
| jQuery | 3.7.1 | ✅ Compatible (static assets) |
| jQuery.Validation | 1.21.0 | ✅ Compatible (static assets) |
| Microsoft.jQuery.Unobtrusive.Validation | 4.0.0 | ✅ Compatible |
| Modernizr | 2.6.2 | ✅ Compatible (static assets) |
| WebGrease | 1.5.2 | ✅ Compatible (if still needed) |
| Microsoft.Data.SqlClient.SNI.runtime | 2.1.1 | ✅ Compatible (dependency) |

**Note**: Microsoft.Identity.Client 4.21.1 is marked as deprecated. If authentication is needed, migrate to Microsoft.Identity.Web for ASP.NET Core.

### Package Update Summary

| Action | Count | Impact |
|--------|-------|--------|
| Remove | 11 | Simplifies dependencies |
| Replace | 2 | Requires code changes |
| Upgrade | 24 | Version bumps, test carefully |
| Add | 1-2 | New functionality |
| Keep | 9 | No changes needed |
| **Total Changes** | **38-39 of 45** | **84-87% of packages** |

---

## Breaking Changes Catalog

### Entity Framework Core 3.1 → 10.0

**Major Breaking Changes**:

1. **Query Evaluation**
   - More queries may fail client evaluation
   - Must be explicitly enabled: `.EnableRetryOnFailure()`

2. **Migrations**
   - May need to regenerate existing migrations
   - Review migration code for obsolete APIs

3. **Shadow Properties**
   - API changes in shadow property access

4. **Nullable Reference Types**
   - EF Core 10.0 fully embraces nullable reference types
   - May require code updates for null handling

**Mitigation**:
- Review all LINQ queries for client evaluation warnings
- Test all database operations thoroughly
- Regenerate migrations if issues arise
- Enable nullable reference types: `<Nullable>enable</Nullable>`

### Microsoft.Data.SqlClient 2.1 → 7.0

**Breaking Changes**:

1. **Connection String Changes**
   - Some connection string keywords changed
   - Encryption defaults changed (now encrypted by default)

2. **API Changes**
   - Some methods signature changes
   - Async patterns improved

**Security Improvements**:
- Vulnerabilities patched
- Better encryption defaults
- Improved connection security

**Mitigation**:
- Review connection strings
- Test all SQL operations
- Verify encryption settings

### ASP.NET Framework → ASP.NET Core (System.Web APIs)

**Critical Namespace/API Changes**:

| Old API (ASP.NET Framework) | New API (ASP.NET Core) | Notes |
|------------------------------|------------------------|-------|
| `System.Web.Mvc.Controller` | `Microsoft.AspNetCore.Mvc.Controller` | Namespace change |
| `System.Web.HttpContext` | `Microsoft.AspNetCore.Http.HttpContext` | Different interface |
| `System.Web.HttpPostedFileBase` | `Microsoft.AspNetCore.Http.IFormFile` | Different API, async |
| `System.Web.Routing.RouteCollection` | Endpoint routing (`app.MapControllerRoute`) | Complete redesign |
| `System.Configuration.ConfigurationManager` | `Microsoft.Extensions.Configuration.IConfiguration` | DI-based |
| `Server.MapPath()` | `IWebHostEnvironment.ContentRootPath` | Different pattern |
| `Response.Write()` | `await Response.WriteAsync()` | Now async |
| `FilterAttribute` | `IActionFilter`, `IResultFilter` | Interface-based |

**File Upload Breaking Changes**:

```csharp
// OLD (ASP.NET Framework)
public ActionResult Upload(HttpPostedFileBase file)
{
    int length = file.ContentLength;  // Property
    string filename = file.FileName;
    file.SaveAs(path);  // Synchronous
}

// NEW (ASP.NET Core)
public async Task<IActionResult> Upload(IFormFile file)
{
    long length = file.Length;  // Property (long, not int)
    string filename = file.FileName;
    using var stream = new FileStream(path, FileMode.Create);
    await file.CopyToAsync(stream);  // Asynchronous
}
```

**Configuration Breaking Changes**:

```csharp
// OLD
var value = ConfigurationManager.AppSettings["Key"];
var connStr = ConfigurationManager.ConnectionStrings["Name"].ConnectionString;

// NEW (inject IConfiguration)
public class MyClass
{
    private readonly IConfiguration _configuration;

    public MyClass(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void UseConfig()
    {
        var value = _configuration["AppSettings:Key"];
        var connStr = _configuration.GetConnectionString("Name");
    }
}
```

### System.Messaging → Modern Message Queue

**All APIs Incompatible** (59 instances):

The entire System.Messaging namespace is not available in .NET Core. Complete replacement required.

**Common API Replacements**:

| System.Messaging API | Replacement Concept | Implementation |
|---------------------|---------------------|----------------|
| `MessageQueue` | Message sender/receiver | RabbitMQ: `IConnection`, `IModel`<br>Azure: `ServiceBusClient` |
| `MessageQueue.Send(object)` | Send message | Queue-specific send methods |
| `MessageQueue.Receive(TimeSpan)` | Receive message | Queue-specific receive methods |
| `MessageQueue.Create(string)` | Create queue | Admin APIs or configuration |
| `MessageQueue.Exists(string)` | Check queue exists | Admin APIs |
| `XmlMessageFormatter` | Serialization | `JsonSerializer`, Protocol Buffers |
| `MessagePriority` | Priority queue | Queue-specific priority mechanisms |
| `MessageQueueAccessRights` | Permissions | Queue-specific permission models |

**Abstraction Required**: Due to the number of instances (59), creating an abstraction layer is strongly recommended.

### Configuration System Migration

**web.config → appsettings.json**:

All 16 instances of `ConfigurationManager` must be replaced:

**Steps**:
1. Extract all `<appSettings>` from web.config → appsettings.json
2. Extract all `<connectionStrings>` → appsettings.json
3. Create strongly-typed configuration classes (recommended)
4. Inject `IConfiguration` or typed options
5. Update all `ConfigurationManager.AppSettings["key"]` calls
6. Update all `ConfigurationManager.ConnectionStrings["name"]` calls

**Example Migration**:

web.config:
```xml
<appSettings>
  <add key="MaxUploadSize" value="10485760" />
  <add key="AllowedExtensions" value=".jpg,.png,.pdf" />
</appSettings>
```

appsettings.json:
```json
{
  "FileUpload": {
    "MaxUploadSize": 10485760,
    "AllowedExtensions": [".jpg", ".png", ".pdf"]
  }
}
```

Strongly-typed configuration:
```csharp
public class FileUploadOptions
{
    public int MaxUploadSize { get; set; }
    public string[] AllowedExtensions { get; set; }
}

// Program.cs
builder.Services.Configure<FileUploadOptions>(
    builder.Configuration.GetSection("FileUpload"));

// Controller
public class UploadController : Controller
{
    private readonly FileUploadOptions _options;

    public UploadController(IOptions<FileUploadOptions> options)
    {
        _options = options.Value;
    }
}
```

### Routing Migration

**RouteCollection → Endpoint Routing**:

ASP.NET Framework used `RouteCollection` with URL patterns. ASP.NET Core uses endpoint routing.

**Old (RouteConfig.cs)**:
```csharp
public class RouteConfig
{
    public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

        routes.MapRoute(
            name: "Default",
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
        );
    }
}
```

**New (Program.cs)**:
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

**Changes**:
- No more `RouteCollection` class
- No more separate `RouteConfig` file
- Routes configured in `Program.cs`
- Simpler syntax: `id?` instead of `id = UrlParameter.Optional`

### Summary of Breaking Changes

| Category | Instances | Severity | Mitigation Complexity |
|----------|-----------|----------|----------------------|
| System.Messaging (MSMQ) | 59 | Critical | High - requires infrastructure change |
| System.Configuration | 16 | High | Medium - systematic replacement |
| System.Web APIs | 16 | High | Medium - well-documented migration |
| Entity Framework Core | N/A | Medium | Low - mostly version compatibility |
| Microsoft.Data.SqlClient | N/A | Low | Low - test SQL operations |

**Total Estimated Code Changes**: 92+ lines minimum (assessment estimate), likely 150-200 lines for complete migration

### Project Complexity Table

| Project | Complexity | Dependencies | Packages | LOC | Risk Level | Key Challenges |
|---------|-----------|--------------|----------|-----|-----------|----------------|
| ContosoUniversity.csproj | **High** | 0 projects | 45 → 35 | 3,392 | **Critical** | ASP.NET Framework→Core, MSMQ, Security vulns |

### Overall Assessment

**Relative Complexity: High**

**Factors Contributing to High Complexity**:
1. **Architectural Migration**: Complete platform change (ASP.NET Framework → ASP.NET Core)
2. **Infrastructure Dependency**: MSMQ replacement requires new infrastructure
3. **API Compatibility**: 92 API issues (68% binary incompatible)
4. **Package Ecosystem**: 26 of 45 packages require updates or replacements
5. **Framework-Specific Features**: Multiple ASP.NET Framework patterns to replace
6. **Security Urgency**: Active vulnerabilities requiring immediate remediation

**Resource Requirements**:
- **Skill Level**: Senior/expert .NET developer with ASP.NET Core experience
- **Specializations Needed**:
  - ASP.NET Core migration expertise
  - Message queue architecture (for MSMQ replacement)
  - Entity Framework Core
  - Security vulnerability remediation
- **Parallel Capacity**: Not applicable (single project, sequential work)

---

## Risk Management

### High-Risk Changes

| Project | Risk Level | Description | Mitigation |
|---------|-----------|-------------|------------|
| ContosoUniversity.csproj | **Critical** | Complete ASP.NET Framework to ASP.NET Core migration | Comprehensive testing, phased rollout, documented rollback |
| ContosoUniversity.csproj | **High** | MSMQ replacement - infrastructure change | Abstract message queue interface, parallel testing environment, feature flags |
| ContosoUniversity.csproj | **High** | Security vulnerabilities in Microsoft.Data.SqlClient | Immediate upgrade to 7.0.0, validate all SQL operations |
| ContosoUniversity.csproj | **Medium** | Entity Framework Core 3.1 → 10.0 upgrade | Regenerate migrations, test all database operations |
| ContosoUniversity.csproj | **Medium** | Configuration system migration | Document all config keys, validate at startup |

### Security Vulnerabilities

| Package | Current Version | Vulnerability | Remediation |
|---------|----------------|---------------|-------------|
| Microsoft.Data.SqlClient | 2.1.4 | Known security vulnerabilities | Upgrade to 7.0.0 immediately |

**Impact**: High - SQL client vulnerabilities could affect data security
**Priority**: Critical - Address in Phase 1 (Atomic Migration)

### Contingency Plans

**MSMQ Replacement Blocking Issues**:
- **Alternative 1**: Use message queue abstraction with in-memory implementation for initial migration
- **Alternative 2**: Defer MSMQ replacement - stub out message queue calls initially
- **Alternative 3**: Consider System.Messaging compatibility package (if available) as temporary bridge

**Performance Problems After Migration**:
- **Investigation**: Profile application to identify bottlenecks
- **Mitigation**: ASP.NET Core is typically faster; performance issues likely in custom code
- **Alternative**: Review EF Core query patterns, consider compiled queries

**Breaking Changes in EF Core 10.0**:
- **Investigation**: Review EF Core 10.0 breaking changes documentation
- **Mitigation**: Test all database operations thoroughly
- **Alternative**: Stay on EF Core 9.0 temporarily if blocking issues found

**Build/Compilation Failures**:
- **Investigation**: Review compiler errors systematically, consult breaking changes catalog
- **Mitigation**: Fix errors by category (namespace, API changes, behavior changes)
- **Alternative**: Consult ASP.NET Core migration documentation, community resources

---

## Testing & Validation Strategy

### Multi-Level Testing Approach

Testing is critical for this migration due to the architectural changes from ASP.NET Framework to ASP.NET Core.

### Phase 1: Build Verification

**Objective**: Ensure project compiles successfully

**Steps**:
1. Build project: `dotnet build`
2. Verify zero errors
3. Review warnings (address critical warnings)
4. Verify all controllers compile
5. Verify all views compile

**Success Criteria**:
- ✅ `dotnet build` completes successfully
- ✅ Zero compilation errors
- ✅ No critical warnings (warnings acceptable for initial build)
- ✅ All .cs files compile
- ✅ All .cshtml files compile

**Exit Criteria**: Cannot proceed to testing until build succeeds

### Phase 2: Project Structure Validation

**Objective**: Verify project structure and configuration

**Steps**:
1. Verify SDK-style project file format
2. Verify TargetFramework is net10.0
3. Verify all required packages installed
4. Verify no deprecated packages remain
5. Verify appsettings.json exists and is valid
6. Verify Program.cs exists and configures services
7. Verify wwwroot folder structure correct

**Success Criteria**:
- ✅ Project file is SDK-style format
- ✅ `<TargetFramework>net10.0</TargetFramework>` present
- ✅ All packages restored (no restore errors)
- ✅ Configuration files valid JSON
- ✅ Program.cs contains valid service registration

### Phase 3: Application Start Validation

**Objective**: Verify application starts without runtime errors

**Steps**:
1. Run application: `dotnet run`
2. Verify application starts
3. Verify no startup exceptions
4. Verify logging works
5. Verify configuration loads
6. Verify database connection (if configured)

**Success Criteria**:
- ✅ Application starts without exceptions
- ✅ Console shows "Now listening on: http://..." message
- ✅ No errors in startup logs
- ✅ Can navigate to home page (http://localhost:5000)
- ✅ Home page renders without errors

**Common Issues**:
- Configuration errors (missing appsettings.json keys)
- Database connection failures (connection string issues)
- Dependency injection errors (unregistered services)
- Middleware ordering issues

### Phase 4: Functional Testing

**Objective**: Verify all application features work

#### 4.1 Navigation Testing

Test all major pages and routes:

- [ ] Home page (`/`)
- [ ] About page (if exists)
- [ ] Contact page (if exists)
- [ ] Student pages (list, create, edit, delete, details)
- [ ] Course pages
- [ ] Instructor pages
- [ ] Department pages
- [ ] Error page (`/Home/Error`)

**Success Criteria**: All pages load without errors, all links work

#### 4.2 Database Operations (CRUD)

Test all Create, Read, Update, Delete operations:

**Students**:
- [ ] List all students
- [ ] View student details
- [ ] Create new student
- [ ] Edit existing student
- [ ] Delete student
- [ ] Search/filter students (if exists)

**Courses**:
- [ ] List all courses
- [ ] View course details
- [ ] Create new course
- [ ] Edit existing course
- [ ] Delete course

**Instructors**:
- [ ] List all instructors
- [ ] View instructor details
- [ ] Create new instructor
- [ ] Edit existing instructor
- [ ] Delete instructor

**Departments**:
- [ ] List all departments
- [ ] View department details
- [ ] Create new department
- [ ] Edit existing department
- [ ] Delete department

**Success Criteria**:
- ✅ All read operations return correct data
- ✅ All create operations save to database
- ✅ All update operations persist changes
- ✅ All delete operations remove records
- ✅ Relationships work correctly (foreign keys)
- ✅ Validation works (client and server-side)

#### 4.3 Message Queue Testing

⚠️ **High Priority - Infrastructure Dependency**

Test MSMQ replacement implementation:

**Queue Operations**:
- [ ] Create queue (if applicable)
- [ ] Send message to queue
- [ ] Receive message from queue
- [ ] Verify message content
- [ ] Test message priority (if implemented)
- [ ] Test timeout scenarios
- [ ] Test queue permissions (if applicable)

**Integration Tests**:
- [ ] Test end-to-end scenarios using message queue
- [ ] Verify message serialization/deserialization
- [ ] Test error handling (queue unavailable, timeout, etc.)
- [ ] Verify message queue abstraction works with chosen implementation

**Success Criteria**:
- ✅ Messages send successfully
- ✅ Messages received correctly
- ✅ Message content accurate (serialization works)
- ✅ Error handling works appropriately
- ✅ No System.Messaging dependencies remain in code

**Rollback Plan**: If queue replacement fails, consider:
- In-memory queue for temporary operation
- Stub out queue calls temporarily
- Consult with infrastructure team for alternative solutions

#### 4.4 File Upload Testing

Test file upload functionality (uses HttpPostedFileBase → IFormFile):

- [ ] Select file for upload
- [ ] Upload file
- [ ] Verify file saved to correct location
- [ ] Verify file size restrictions work
- [ ] Verify allowed file type restrictions work
- [ ] Test large file upload
- [ ] Test invalid file types rejected

**Success Criteria**:
- ✅ Files upload successfully
- ✅ File validation works (size, type)
- ✅ Error messages display correctly
- ✅ No runtime exceptions

#### 4.5 Configuration Testing

Test configuration system migration (ConfigurationManager → IConfiguration):

- [ ] Verify all app settings load correctly
- [ ] Verify connection strings load correctly
- [ ] Test environment-specific settings (Development, Production)
- [ ] Verify default values work
- [ ] Test missing configuration handling

**Success Criteria**:
- ✅ All configuration values accessible
- ✅ Connection strings work
- ✅ No `ConfigurationManager` calls remain
- ✅ Appropriate errors for missing required config

### Phase 5: Security Validation

**Objective**: Verify security improvements and no regressions

#### 5.1 Package Vulnerability Scan

```bash
dotnet list package --vulnerable
```

**Success Criteria**:
- ✅ Zero vulnerable packages reported
- ✅ Microsoft.Data.SqlClient is version 7.0.0 or higher
- ✅ No deprecated packages in use (except explicitly documented)

#### 5.2 SQL Injection Testing

Test data access for SQL injection vulnerabilities:

- [ ] Test parameterized queries work
- [ ] Test EF Core query generation
- [ ] Verify no string concatenation in SQL
- [ ] Test special characters in inputs (', --, ;, etc.)

**Success Criteria**:
- ✅ All queries use parameterization
- ✅ No SQL injection vulnerabilities found
- ✅ Input validation working

#### 5.3 Cross-Site Scripting (XSS) Testing

Test view rendering for XSS vulnerabilities:

- [ ] Test user input rendering in views
- [ ] Verify HTML encoding works
- [ ] Test JavaScript injection attempts
- [ ] Verify `@Html.Raw()` used appropriately only

**Success Criteria**:
- ✅ User input automatically encoded
- ✅ No XSS vulnerabilities
- ✅ Proper use of encoding helpers

### Phase 6: Performance Validation

**Objective**: Ensure migration didn't degrade performance

**Tests**:
- [ ] Measure page load times (before/after)
- [ ] Measure database query performance
- [ ] Test concurrent user load (if applicable)
- [ ] Monitor memory usage
- [ ] Monitor CPU usage

**Success Criteria**:
- ✅ Page load times similar or better than before
- ✅ Database queries perform similarly
- ✅ No memory leaks
- ✅ Acceptable resource usage

**Note**: ASP.NET Core typically performs better than ASP.NET Framework. If performance degrades, investigate:
- Inefficient LINQ queries (client evaluation)
- Configuration issues
- Logging overhead
- Custom code issues

### Phase 7: Integration Testing

**Objective**: Test system as a whole

**End-to-End Scenarios**:
1. **Student Enrollment Flow**:
   - Create new student
   - Enroll in courses
   - View enrollments
   - Update grades
   - Generate reports

2. **Course Management Flow**:
   - Create new course
   - Assign instructor
   - Enroll students
   - Track attendance/grades

3. **Message Queue Flow**:
   - Trigger message send operation
   - Verify message processing
   - Confirm expected outcome

**Success Criteria**:
- ✅ All end-to-end scenarios complete successfully
- ✅ Data consistency maintained
- ✅ Business logic works as expected
- ✅ No errors in any workflow

### Phase 8: Regression Testing

**Objective**: Ensure existing functionality not broken

**Tests**:
- [ ] Run all existing unit tests (if any)
- [ ] Run all existing integration tests (if any)
- [ ] Verify all previously working features still work
- [ ] Test edge cases
- [ ] Test error scenarios

**Success Criteria**:
- ✅ All existing tests pass (or are updated appropriately)
- ✅ No functionality regressions
- ✅ Error handling works as before

### Testing Checklist Summary

**Before Proceeding to Production**:

✅ **Build & Structure**
- [ ] Project builds without errors
- [ ] Project builds without critical warnings
- [ ] SDK-style project format verified
- [ ] Target framework is net10.0
- [ ] All packages restored successfully

✅ **Application Start**
- [ ] Application starts without errors
- [ ] Home page loads
- [ ] Configuration loads correctly
- [ ] Database connection works

✅ **Functionality**
- [ ] All pages navigate correctly
- [ ] All CRUD operations work
- [ ] Message queue operations work
- [ ] File uploads work
- [ ] Configuration values accessible

✅ **Security**
- [ ] No vulnerable packages (`dotnet list package --vulnerable`)
- [ ] Microsoft.Data.SqlClient ≥ 7.0.0
- [ ] No SQL injection vulnerabilities
- [ ] No XSS vulnerabilities

✅ **Performance**
- [ ] Page load times acceptable
- [ ] Database queries perform well
- [ ] No memory leaks
- [ ] Resource usage acceptable

✅ **Integration**
- [ ] End-to-end scenarios work
- [ ] Data consistency maintained
- [ ] Business logic correct

✅ **Regression**
- [ ] All existing tests pass
- [ ] No functionality broken
- [ ] Error handling works

### Automated Testing Recommendations

If no automated tests exist, consider adding:

1. **Unit Tests**:
   - Controller action tests
   - Business logic tests
   - Validation tests

2. **Integration Tests**:
   - Database operation tests
   - Message queue tests
   - End-to-end scenario tests

3. **Smoke Tests**:
   - Quick health checks
   - Critical path validation
   - Can run on every build

**Tools**:
- xUnit, NUnit, or MSTest for unit testing
- ASP.NET Core TestHost for integration testing
- Selenium or Playwright for UI testing (optional)

---

## Source Control Strategy

### Branching Strategy

**Current State**:
- Main branch: `main`
- Upgrade branch: `upgrade-to-NET10` (already created)

**Strategy**: Feature branch workflow with single comprehensive commit

**Rationale**:
- All-at-once migration creates one cohesive change
- Single atomic commit makes rollback simple
- Clear before/after comparison
- Easy to review as complete unit

### Commit Strategy

**Recommended Approach**: Single comprehensive commit after all changes complete and tested

**Commit Structure**:

```
Migrate ContosoUniversity from .NET Framework 4.8 to .NET 10.0

BREAKING CHANGE: Complete architectural migration from ASP.NET Framework to ASP.NET Core

Changes:
- Convert project to SDK-style format
- Update target framework: net48 → net10.0
- Upgrade Entity Framework Core: 3.1.32 → 10.0.5
- Upgrade Microsoft.Data.SqlClient: 2.1.4 → 7.0.0 (fixes security vulnerabilities)
- Replace System.Messaging with RabbitMQ/Azure Service Bus abstraction
- Migrate configuration: web.config → appsettings.json
- Convert routing: RouteCollection → endpoint routing
- Replace bundling: System.Web.Optimization → direct HTML references
- Migrate application initialization: Global.asax → Program.cs
- Update ASP.NET Framework APIs → ASP.NET Core APIs
- Fix 92+ breaking API changes

Package Changes:
- Removed 11 packages (functionality in framework)
- Replaced 2 incompatible packages
- Upgraded 24 packages to .NET 10.0 versions
- Added 1-2 ASP.NET Core specific packages

Testing:
- All builds pass with 0 errors
- All functional tests pass
- All CRUD operations validated
- Message queue replacement tested
- Security vulnerabilities resolved
- No vulnerable packages remain

Resolves: #[issue-number] (if tracking in issues)
```

**Alternative Approach**: Multiple checkpoint commits (if preferred)

If single commit is too large, consider:

1. **Commit 1**: Project conversion and package updates
   ```
   Convert project to SDK-style and update packages

   - Convert to SDK-style project file
   - Update TargetFramework to net10.0
   - Update/remove/add packages per migration plan
   ```

2. **Commit 2**: Configuration and routing migration
   ```
   Migrate configuration and routing to ASP.NET Core

   - Convert web.config → appsettings.json
   - Update ConfigurationManager → IConfiguration
   - Convert RouteCollection → endpoint routing
   - Migrate Global.asax → Program.cs
   ```

3. **Commit 3**: MSMQ replacement
   ```
   Replace System.Messaging with message queue abstraction

   - Create IMessageQueue abstraction
   - Implement RabbitMQ/Azure Service Bus adapter
   - Update all 59 System.Messaging call sites
   ```

4. **Commit 4**: Breaking changes and compilation fixes
   ```
   Fix breaking API changes and compilation errors

   - Update System.Web APIs → ASP.NET Core APIs
   - Fix HttpPostedFileBase → IFormFile
   - Fix other API incompatibilities
   ```

5. **Commit 5**: Testing and validation
   ```
   Add tests and validate migration

   - Add/update tests
   - Validate all functionality
   - Verify security fixes
   ```

### Review and Merge Process

**Pull Request Requirements**:

**Title**: `[MIGRATION] .NET Framework 4.8 → .NET 10.0`

**Description Template**:
```markdown
## Migration Summary

Migrates ContosoUniversity from ASP.NET Framework 4.8 to ASP.NET Core on .NET 10.0.

## Changes

### Project Structure
- [x] Converted to SDK-style project
- [x] Updated target framework to net10.0
- [x] Updated 38 of 45 packages

### Architecture
- [x] Migrated from ASP.NET MVC to ASP.NET Core MVC
- [x] Replaced System.Messaging with [RabbitMQ/Azure Service Bus]
- [x] Migrated configuration system
- [x] Converted routing to endpoint routing
- [x] Removed bundling/minification (using direct references)

### Security
- [x] Upgraded Microsoft.Data.SqlClient 2.1.4 → 7.0.0
- [x] Resolved all package vulnerabilities
- [x] Zero vulnerable packages

### Breaking Changes
- [x] Fixed 92+ API compatibility issues
- [x] Updated all System.Web APIs
- [x] Migrated all configuration access
- [x] Replaced all MSMQ calls

## Testing

### Build
- [x] Project builds with 0 errors
- [x] Project builds with 0 warnings

### Functionality
- [x] All pages load correctly
- [x] All CRUD operations work
- [x] Message queue operations work
- [x] File uploads work
- [x] Configuration loads correctly

### Security
- [x] No vulnerable packages
- [x] SQL injection tests pass
- [x] XSS tests pass

### Performance
- [x] Page load times acceptable
- [x] Database queries perform well

## Rollback Plan

If issues found after merge:
1. Revert merge commit: `git revert -m 1 <merge-commit>`
2. Return to `main` branch
3. Original code remains in `upgrade-to-NET10` branch for future retry

## Deployment Notes

- [ ] MSMQ infrastructure no longer required
- [ ] New message queue infrastructure needed ([RabbitMQ/Azure Service Bus])
- [ ] .NET 10.0 runtime required on hosting environment
- [ ] Update deployment scripts/pipelines
- [ ] Update monitoring/logging configuration

## Reviewer Checklist

- [ ] All files reviewed
- [ ] Breaking changes understood
- [ ] Test coverage adequate
- [ ] Security concerns addressed
- [ ] Performance acceptable
- [ ] Documentation updated
```

**Merge Criteria**:
- ✅ All reviewers approved
- ✅ All tests pass (CI/CD if configured)
- ✅ No unresolved comments
- ✅ Security scan passes
- ✅ Code quality checks pass

**Merge Method**: Squash or Merge Commit (depending on commit strategy chosen)
- **Squash**: If multiple checkpoint commits → single commit on main
- **Merge Commit**: If single comprehensive commit → preserve commit history

### Post-Merge Actions

After successful merge to `main`:

1. **Tag the Release**:
   ```bash
   git tag -a v2.0.0-net10 -m "Migrated to .NET 10.0"
   git push origin v2.0.0-net10
   ```

2. **Delete Feature Branch** (optional):
   ```bash
   git branch -d upgrade-to-NET10
   git push origin --delete upgrade-to-NET10
   ```

3. **Update Documentation**:
   - Update README.md with new framework version
   - Update deployment documentation
   - Update developer setup instructions

4. **Notify Team**:
   - Announce migration completion
   - Provide migration notes
   - Schedule knowledge sharing session (if needed)

---

## Success Criteria

### Technical Criteria

The migration is complete and successful when ALL of the following are true:

#### Build & Compilation
- ✅ Project uses SDK-style project file format
- ✅ Target framework is `<TargetFramework>net10.0</TargetFramework>`
- ✅ `dotnet build` completes with 0 errors
- ✅ `dotnet build` completes with 0 warnings (or only acceptable warnings documented)
- ✅ All controllers compile successfully
- ✅ All Razor views compile successfully

#### Package Management
- ✅ All package updates from plan applied:
  - 11 packages removed (functionality in framework)
  - 2 packages replaced (Antlr4, bundling alternative)
  - 24 packages upgraded to .NET 10.0 versions
  - 1-2 packages added (ASP.NET Core specific)
- ✅ `dotnet restore` completes successfully
- ✅ `dotnet list package --vulnerable` reports **zero vulnerabilities**
- ✅ Microsoft.Data.SqlClient is version **7.0.0 or higher**
- ✅ No deprecated packages remain (except documented exceptions)

#### Application Functionality
- ✅ Application starts without runtime errors
- ✅ Home page loads and renders correctly
- ✅ All major pages accessible (Student, Course, Instructor, Department)
- ✅ All CRUD operations work:
  - Create (save to database)
  - Read (display correct data)
  - Update (persist changes)
  - Delete (remove records)
- ✅ Database connection works
- ✅ Entity Framework Core operations succeed
- ✅ File uploads work (HttpPostedFileBase → IFormFile migration)
- ✅ Configuration system works (web.config → appsettings.json)
- ✅ Routing works (all URLs resolve correctly)
- ✅ Static files serve correctly (CSS, JavaScript, images)

#### Architecture Migration
- ✅ No System.Web dependencies remain
- ✅ No System.Messaging (MSMQ) dependencies remain
- ✅ Message queue abstraction implemented and working
- ✅ Global.asax removed, Program.cs contains application initialization
- ✅ RouteCollection removed, endpoint routing configured
- ✅ System.Web.Optimization removed, alternative bundling in place
- ✅ ConfigurationManager removed, IConfiguration used throughout
- ✅ All ASP.NET Framework patterns replaced with ASP.NET Core patterns

#### Code Quality
- ✅ No `System.Web.*` using directives remain
- ✅ No `System.Messaging.*` using directives remain
- ✅ No `System.Configuration.ConfigurationManager` calls remain
- ✅ All 92+ identified API issues resolved
- ✅ Code follows ASP.NET Core patterns and conventions
- ✅ Dependency injection used appropriately

#### Security
- ✅ Zero packages with security vulnerabilities
- ✅ Microsoft.Data.SqlClient vulnerability CVEs resolved
- ✅ SQL injection tests pass (parameterized queries)
- ✅ XSS tests pass (proper HTML encoding)
- ✅ Input validation works correctly

#### Testing
- ✅ All existing unit tests pass (if any)
- ✅ All existing integration tests pass (if any)
- ✅ Manual functional testing complete (all features tested)
- ✅ Message queue integration tested
- ✅ Database operations tested
- ✅ File upload tested
- ✅ End-to-end scenarios tested

#### Performance
- ✅ Application startup time acceptable
- ✅ Page load times similar or better than before
- ✅ Database query performance acceptable
- ✅ No memory leaks detected
- ✅ Resource usage (CPU, memory) acceptable

### Quality Criteria

#### Code Quality Standards
- ✅ Code compiles without warnings
- ✅ Code follows .NET naming conventions
- ✅ Nullable reference types enabled: `<Nullable>enable</Nullable>`
- ✅ Async/await used appropriately (especially for I/O operations)
- ✅ Dependency injection configured correctly
- ✅ Configuration strongly-typed (IOptions<T> pattern)

#### Test Coverage
- ✅ Existing test coverage maintained or improved
- ✅ New tests added for message queue abstraction
- ✅ Critical paths have tests
- ✅ All tests pass

#### Documentation
- ✅ README.md updated with .NET 10.0 requirements
- ✅ Setup instructions updated
- ✅ MSMQ replacement documented
- ✅ Configuration keys documented
- ✅ Breaking changes documented (if affecting API consumers)
- ✅ Migration notes captured

### Process Criteria

#### All-At-Once Strategy Principles Applied
- ✅ All project files updated simultaneously
- ✅ All package updates applied atomically
- ✅ All code changes made together
- ✅ Single comprehensive testing phase
- ✅ Clean migration (no multi-targeting or interim states)

#### Source Control
- ✅ All changes committed to `upgrade-to-NET10` branch
- ✅ Commit messages follow conventions
- ✅ Pull request created with complete description
- ✅ Code reviewed and approved
- ✅ Merged to `main` branch

#### Deployment Readiness
- ✅ .NET 10.0 SDK available on target environment
- ✅ Message queue infrastructure available (RabbitMQ/Azure Service Bus)
- ✅ Database migrations ready (if needed)
- ✅ Configuration files prepared for each environment
- ✅ Deployment documentation updated

### Verification Commands

Run these commands to verify success criteria:

```bash
# Build verification
dotnet build
# Expected: Build succeeded. 0 Error(s), 0 Warning(s)

# Package vulnerability scan
dotnet list package --vulnerable
# Expected: No vulnerable packages

# Restore verification
dotnet restore
# Expected: Restore succeeded

# Run application
dotnet run
# Expected: Now listening on: http://localhost:5000
# Navigate to http://localhost:5000 and verify home page loads

# Run tests (if exists)
dotnet test
# Expected: All tests pass
```

### Definition of Done

The migration is **DONE** when:

1. ✅ All Technical Criteria met
2. ✅ All Quality Criteria met
3. ✅ All Process Criteria met
4. ✅ All Verification Commands pass
5. ✅ Code merged to `main` branch
6. ✅ Team acknowledges completion

### Acceptance

Final acceptance requires:
- **Technical Lead**: Approves technical implementation
- **Security Team**: Confirms vulnerability resolution (if required)
- **QA Team**: Validates testing (if required)
- **Product Owner**: Accepts functionality (if required)

---

**END OF PLAN**
