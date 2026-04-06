# ContosoUniversity .NET 10.0 Upgrade Tasks

## Overview

This document tracks the migration of ContosoUniversity from ASP.NET Framework 4.8 to ASP.NET Core on .NET 10.0. The migration involves comprehensive architectural changes including project conversion, framework updates, MSMQ replacement, and configuration system migration.

**Progress**: 3/4 tasks complete (75%) ![0%](https://progress-bar.xyz/75)

---

## Tasks

### [✓] TASK-001: Verify prerequisites and environment readiness *(Completed: 2026-04-03 22:05)*
**References**: Plan §Phase 0, Plan §Prerequisites

- [✓] (1) Verify .NET 10.0 SDK installed and accessible
- [✓] (2) .NET 10.0 SDK version ≥10.0.0 confirmed (**Verify**)
- [✓] (3) Verify Visual Studio 2022 (17.12+) or VS Code with C# Dev Kit available
- [✓] (4) Development environment meets minimum requirements (**Verify**)

---

### [✓] TASK-002: Execute atomic framework and architecture migration *(Completed: 2026-04-04 02:27)*
**References**: Plan §Phase 1, Plan §Project-by-Project Plans, Plan §Package Update Reference, Plan §Breaking Changes Catalog, Plan §MSMQ Migration

- [✓] (1) Convert ContosoUniversity.csproj to SDK-style format per Plan §2. Project File Conversion
- [✓] (2) Project file uses `<Project Sdk="Microsoft.NET.Sdk.Web">` format (**Verify**)
- [✓] (3) Update TargetFramework to net10.0: `<TargetFramework>net10.0</TargetFramework>`
- [✓] (4) Target framework is net10.0 (**Verify**)
- [✓] (5) Update package references per Plan §Package Update Reference: Remove 11 packages, replace 2 packages, upgrade 24 packages to .NET 10.0 versions
- [✓] (6) All package updates applied per plan (**Verify**)
- [✓] (7) Implement message queue abstraction per Plan §MSMQ Migration Option A: Create IMessageQueue interface, implement adapter for chosen technology, replace all 59 System.Messaging calls
- [✓] (8) System.Messaging references removed, abstraction implemented (**Verify**)
- [✓] (9) Migrate configuration system per Plan §6. Configuration Migration: Convert web.config to appsettings.json, replace ConfigurationManager calls with IConfiguration
- [✓] (10) web.config converted, all configuration accessible via IConfiguration (**Verify**)
- [✓] (11) Convert application initialization per Plan §5. ASP.NET Framework → ASP.NET Core Conversion: Migrate Global.asax to Program.cs, convert RouteCollection to endpoint routing, remove System.Web.Optimization
- [✓] (12) Global.asax removed, Program.cs contains service registration and middleware pipeline (**Verify**)
- [✓] (13) Update ASP.NET Framework APIs to ASP.NET Core equivalents per Plan §Breaking Changes Catalog: System.Web.Mvc → Microsoft.AspNetCore.Mvc, HttpPostedFileBase → IFormFile, update all namespace references
- [✓] (14) No System.Web.* using directives remain (**Verify**)
- [✓] (15) Build solution and fix all compilation errors per Plan §Breaking Changes Catalog
- [✓] (16) Solution builds with 0 errors (**Verify**)

---

### [✓] TASK-003: Run validation tests and security scan *(Completed: 2026-04-04 02:28)*
**References**: Plan §Phase 2, Plan §Testing & Validation Strategy, Plan §Security Vulnerabilities

- [✓] (1) Run application: `dotnet run` and verify startup without errors
- [✓] (2) Application starts and listens on configured port without exceptions (**Verify**)
- [✓] (3) Run security vulnerability scan: `dotnet list package --vulnerable` per Plan §Phase 5.1
- [✓] (4) Zero vulnerable packages and Microsoft.Data.SqlClient ≥7.0.0 (**Verify**)
- [✓] (5) If test projects exist: Run all tests with `dotnet test` per Plan §Phase 2
- [✓] (6) All tests pass with 0 failures OR no test projects exist (**Verify**)

---

### [▶] TASK-004: Create final migration commit
**References**: Plan §Source Control Strategy

- [▶] (1) Commit all migration changes with message: "Migrate ContosoUniversity from .NET Framework 4.8 to .NET 10.0\n\nBREAKING CHANGE: Complete architectural migration from ASP.NET Framework to ASP.NET Core\n\nChanges:\n- Convert project to SDK-style format\n- Update target framework: net48 → net10.0\n- Upgrade Entity Framework Core: 3.1.32 → 10.0.5\n- Upgrade Microsoft.Data.SqlClient: 2.1.4 → 7.0.0 (fixes security vulnerabilities)\n- Replace System.Messaging with message queue abstraction\n- Migrate configuration: web.config → appsettings.json\n- Convert routing: RouteCollection → endpoint routing\n- Replace bundling: System.Web.Optimization → direct HTML references\n- Migrate application initialization: Global.asax → Program.cs\n- Update ASP.NET Framework APIs → ASP.NET Core APIs\n- Fix 92+ breaking API changes\n\nPackage Changes:\n- Removed 11 packages (functionality in framework)\n- Replaced 2 incompatible packages\n- Upgraded 24 packages to .NET 10.0 versions\n\nTesting:\n- All builds pass with 0 errors\n- Security vulnerabilities resolved\n- No vulnerable packages remain"

---






