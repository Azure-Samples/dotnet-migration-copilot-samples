# Modernization Plan: ContosoUniversity

## Overview

This plan modernizes the **ContosoUniversity** ASP.NET MVC application from **.NET Framework 4.8** to **.NET 10** and migrates it to Azure cloud services. The plan is generated from the AppCAT assessment report (`report-20260514104511`) which identified **6 issue categories** with **25 total incidents** across the codebase, plus a user-requested .NET version upgrade.

| Property | Value |
|---|---|
| **Application** | ContosoUniversity |
| **Current Framework** | .NET Framework 4.8 (ASP.NET MVC) |
| **Target Framework** | .NET 10 (ASP.NET Core) |
| **Language** | C# |
| **Build Tool** | MSBuild |
| **Total Issues** | 7 (6 from assessment + 1 user-requested) |
| **Total Incidents** | 25+ |
| **Estimated Effort** | 75+ story points |

---

## Task Summary

| # | Task | Category | Severity | Incidents | Skill |
|---|---|---|---|---|---|
| 1 | Upgrade .NET Framework 4.8 → .NET 10 | Framework Upgrade | mandatory | — | `dotnet-upgrade` |
| 2 | Migrate MSMQ to Azure Service Bus | Queue | mandatory | 12 | `dotnet-azure-servicebus` |
| 3 | Migrate Windows Auth to Managed Identity | Identity | mandatory | 1 | `dotnet-managed-identity` |
| 4 | Migrate local file I/O to Azure Storage | Local | potential | 8 | `dotnet-azure-storage-blob` |
| 5 | Migrate secrets to Azure Key Vault | Security | optional | 2 | `dotnet-azure-keyvault-secret` |
| 6 | Configure Azure SQL Database | Database | potential | 1 | `dotnet-azure-sql-database` |
| 7 | Move static content to Azure CDN | Scale | optional | 1 | `manual` |

---

## Task Details

### Task 1: Upgrade .NET Framework 4.8 to .NET 10

**Priority:** 🔴 Mandatory — Must be completed first  
**Effort:** High  
**Category:** Framework Upgrade (user-requested)

**Description:**  
Upgrade the application from .NET Framework 4.8 (ASP.NET MVC 5) to .NET 10 (ASP.NET Core). This is a prerequisite for all other modernization tasks, as the Azure SDK libraries and modern authentication patterns target .NET Core/.NET 5+.

**Scope:**
- Convert `ContosoUniversity.csproj` from legacy format to SDK-style project
- Migrate from `packages.config` to `<PackageReference>` format
- Replace ASP.NET MVC 5 controllers/views with ASP.NET Core MVC patterns
- Replace `Web.config` with `appsettings.json` and `Program.cs` configuration
- Replace `Global.asax` with ASP.NET Core middleware pipeline
- Update Entity Framework 6 to Entity Framework Core
- Update all NuGet packages to .NET 10-compatible versions
- Replace `System.Web` dependencies with ASP.NET Core equivalents
- Update Razor views for ASP.NET Core Tag Helpers

**Files Affected:**
- `ContosoUniversity.csproj` — Project file conversion
- `packages.config` — Remove (migrate to PackageReference)
- `Web.config` — Replace with `appsettings.json`
- `Global.asax` / `Global.asax.cs` — Replace with `Program.cs`
- `Controllers/*.cs` — Update base classes and namespaces
- `Views/**/*.cshtml` — Update Razor syntax
- `Models/*.cs` — Update EF annotations
- `Data/*.cs` — Migrate to EF Core DbContext
- `App_Start/*.cs` — Migrate to middleware configuration

**Success Criteria:**
- Application compiles on .NET 10
- All existing functionality preserved
- EF Core migrations work against the database

---

### Task 2: Migrate MSMQ to Azure Service Bus

**Priority:** 🔴 Mandatory  
**Effort:** 3 story points per incident × 12 incidents = 36 story points  
**Category:** Queue  
**Rule:** Queue.0003 — MSMQ usage detected

**Description:**  
The application uses `System.Messaging.MessageQueue` (MSMQ) for notification processing. MSMQ is not supported on Azure App Service or containerized deployments. Migrate to Azure Service Bus for reliable cloud-native messaging.

**Scope:**
- Replace `System.Messaging.MessageQueue` with `Azure.Messaging.ServiceBus.ServiceBusClient`
- Replace `System.Messaging.XmlMessageFormatter` with JSON serialization
- Replace `System.Messaging.MessageQueueAccessRights` with Azure Service Bus RBAC
- Configure Service Bus connection via Managed Identity (DefaultAzureCredential)
- Update notification send/receive patterns to use Service Bus SDK

**Files Affected:**
- `Services/NotificationService.cs` — Lines 19, 21, 22, 26, 30 (all 12 MSMQ incidents)

**References:**
- [Azure Service Bus queues](https://go.microsoft.com/fwlink/?LinkID=2243266)
- [Azure Queue Storage vs Service Bus comparison](https://go.microsoft.com/fwlink/?LinkID=2249263)

**Success Criteria:**
- All MSMQ references removed
- Notifications sent and received via Azure Service Bus
- Authentication uses DefaultAzureCredential

---

### Task 3: Migrate Authentication to Managed Identity

**Priority:** 🔴 Mandatory  
**Effort:** 3 story points  
**Category:** Identity  
**Rule:** Identity.0002 — Windows authentication detected

**Description:**  
The application uses connection-string-based authentication which includes Windows authentication patterns not supported on Azure App Service, ACA, or AKS. Migrate to Azure Managed Identity with `DefaultAzureCredential` for all Azure service connections.

**Scope:**
- Replace connection string authentication with `DefaultAzureCredential`
- Configure Azure SQL connection to use Managed Identity token-based auth
- Ensure all Azure SDK clients (Service Bus, Storage, Key Vault) use `DefaultAzureCredential`

**Files Affected:**
- `Web.config` → `appsettings.json` — Connection string configuration

**References:**
- [Azure App Service authentication](https://go.microsoft.com/fwlink/?LinkID=2242883)

**Success Criteria:**
- No passwords or secrets in connection strings
- All Azure services accessed via Managed Identity

---

### Task 4: Migrate Local File I/O to Azure Storage

**Priority:** 🟡 Potential  
**Effort:** 3 story points per incident × 8 incidents = 24 story points  
**Category:** Local  
**Rule:** Local.0003 — Local or network IO operations detected

**Description:**  
The application uses `System.IO.File` and `System.IO.Directory` for file operations (teaching material uploads). Local file system access may not be persistent or scalable on Azure App Service. Migrate to Azure Blob Storage or Azure Storage mount paths.

**Scope:**
- Replace `System.IO.Directory` calls with Azure Blob Storage container operations or mounted storage paths
- Replace `System.IO.File` calls with blob upload/download operations
- Update file upload handling in `CoursesController` to use `BlobServiceClient`
- Configure storage access via Managed Identity

**Files Affected:**
- `Controllers/CoursesController.cs` — Lines 76, 78, 159, 161 (`System.IO.Directory`)
- `Controllers/CoursesController.cs` — Lines 172, 174, 229, 233 (`System.IO.File`)

**References:**
- [Azure Blob Storage](https://go.microsoft.com/fwlink/?linkid=2250574)
- [Azure File Shares](https://go.microsoft.com/fwlink/?LinkID=2242591)
- [Storage mounts for Managed Instance](https://go.microsoft.com/fwlink/?linkid=2346952)

**Success Criteria:**
- All file operations use Azure Blob Storage or mounted storage
- File uploads persist across app restarts and scale-out
- Authentication uses DefaultAzureCredential

---

### Task 5: Migrate Secrets to Azure Key Vault

**Priority:** 🟢 Optional (recommended)  
**Effort:** 3 story points per incident × 2 incidents = 6 story points  
**Category:** Security  
**Rule:** Security.0002 — Connection strings without configuration builders detected

**Description:**  
The application stores connection strings and app settings directly in `Web.config` without configuration builders. This is a security risk and violates compliance standards (PCI DSS, GDPR). Migrate secrets to Azure Key Vault.

**Scope:**
- Move `<connectionStrings>` values to Azure Key Vault
- Move `<appSettings>` secrets to Azure Key Vault
- Configure ASP.NET Core to load secrets from Key Vault via `Azure.Extensions.AspNetCore.Configuration.Secrets`
- Access Key Vault via Managed Identity

**Files Affected:**
- `Web.config` → `appsettings.json` — `<appSettings>` section
- `Web.config` → `appsettings.json` — `<connectionStrings>` section

**References:**
- [Configuration builders](https://go.microsoft.com/fwlink/?LinkID=2250915)
- [Storing application secrets](https://go.microsoft.com/fwlink/?LinkID=2250916)
- [Centralized app configuration and security](https://go.microsoft.com/fwlink/?LinkID=2250733)

**Success Criteria:**
- No secrets stored in configuration files or source code
- All secrets retrieved from Azure Key Vault at runtime
- Key Vault access uses DefaultAzureCredential

---

### Task 6: Configure Azure SQL Database

**Priority:** 🟡 Potential  
**Effort:** 3 story points  
**Category:** Database  
**Rule:** Database.0002 — SQL database connection detected

**Description:**  
Ensure the SQL database is available on Azure. Migrate the on-premises SQL Server database to Azure SQL Database and update connection configuration for cloud deployment.

**Scope:**
- Update `DefaultConnection` connection string for Azure SQL Database
- Configure Entity Framework Core to connect to Azure SQL with Managed Identity
- Validate database schema compatibility with Azure SQL

**Files Affected:**
- `Web.config` → `appsettings.json` — `DefaultConnection` connection string
- `Data/*.cs` — DbContext configuration

**References:**
- [Migrate SQL Server database to Azure](https://go.microsoft.com/fwlink/?LinkID=2251731)
- [Azure SQL Managed Instance](https://go.microsoft.com/fwlink/?LinkID=2251613)
- [Azure Migrate](https://go.microsoft.com/fwlink/?linkid=2252410)

**Success Criteria:**
- Application connects to Azure SQL Database
- Connection uses Managed Identity (no password in connection string)
- All queries and migrations work on Azure SQL

---

### Task 7: Move Static Content to Azure CDN

**Priority:** 🟢 Optional  
**Effort:** 3 story points  
**Category:** Scale  
**Rule:** Scale.0001 — Static content detected

**Description:**  
The application bundles 16 static files (CSS, JavaScript, images) directly in the project. Serving static content from the application increases costs, reduces performance, and requires redeployment for content changes. Consider offloading to Azure Blob Storage with Azure CDN.

**Scope:**
- Move static files (Content/, Scripts/, favicon.ico) to Azure Blob Storage
- Configure Azure CDN for global content delivery
- Update Razor views to reference CDN URLs
- Keep local fallback for development

**Files Affected:**
- `Content/bootstrap.css`, `Content/bootstrap.min.css`, `Content/Site.css`
- `Scripts/bootstrap.js`, `Scripts/bootstrap.min.js`
- `Scripts/jquery-*.js`, `Scripts/jquery.validate*.js`
- `Scripts/modernizr-2.6.2.js`, `Scripts/respond.js`, `Scripts/respond.min.js`
- `favicon.ico`
- `Uploads/TeachingMaterials/*`

**References:**
- [Azure Blob Storage](https://go.microsoft.com/fwlink/?linkid=2250574)
- [Azure CDN](https://go.microsoft.com/fwlink/?linkid=2250392)

**Success Criteria:**
- Static files served via CDN
- Improved load times for end users
- Content updates possible without app redeployment

---

## Execution Order

The tasks should be executed in the following dependency order:

```
Task 1: .NET Upgrade (prerequisite for all Azure SDK tasks)
  ├── Task 2: MSMQ → Azure Service Bus (mandatory)
  ├── Task 3: Managed Identity (mandatory, enables other Azure tasks)
  │     ├── Task 4: Local I/O → Azure Storage
  │     ├── Task 5: Secrets → Azure Key Vault
  │     └── Task 6: Azure SQL Database
  └── Task 7: Static Content → CDN (independent, optional)
```

## Assessment Source

- **Report:** `.github/modernize/assessment/reports/report-20260514104511/report.json`
- **Producer:** .NET AppCAT CLI v1.0.0
- **Analysis Date:** 2026-05-14
- **Target Platforms:** Azure App Service, AKS, ACA, App Service Container, App Service Managed Instance
