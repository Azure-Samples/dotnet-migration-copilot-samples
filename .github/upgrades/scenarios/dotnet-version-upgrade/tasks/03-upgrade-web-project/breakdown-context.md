## Detected Hints

### hint: web-controller-migration-units
- **Status**: resolved
- **Priority**: MUST (7 controllers, >5 threshold)
- **Evidence**: Controllers/{BaseController,CoursesController,DepartmentsController,HomeController,InstructorsController,NotificationsController,StudentsController}.cs. Assessment per-file Api.0001 counts: Courses=146, Departments=94, Students=91, Instructors=67, Base=2, Home=0, Notifications=0.
- **Detected**: during task 03-upgrade-web-project breakdown, 2026-09-21

### hint: system-messaging-replacement
- **Status**: resolved
- **Priority**: MUST
- **Evidence**: Services/NotificationService.cs uses MessageQueue/MessageQueueException/MessageQueueAccessRights/MessageQueueErrorCode (Feature.0008, 63 total issues). Isolated to one file.
- **Detected**: during task 03-upgrade-web-project breakdown, 2026-09-21

### hint: web-config-to-appsettings
- **Status**: resolved
- **Priority**: SHOULD
- **Evidence**: Web.Debug.config/Web.Release.config transforms present; ConfigurationManager.ConnectionStrings/AppSettings used in SchoolContextFactory.cs, Global.asax.cs, NotificationService.cs.
- **Detected**: during task 03-upgrade-web-project breakdown, 2026-09-21

### hint: web-bundling-and-static-assets
- **Status**: resolved
- **Priority**: SHOULD
- **Evidence**: App_Start/BundleConfig.cs uses System.Web.Optimization (Feature.0001), referenced from 9 views incl. Views/Shared/_Layout.cshtml.
- **Detected**: during task 03-upgrade-web-project breakdown, 2026-09-21

### hint: large-package-replacement-batch
- **Status**: resolved
- **Priority**: SHOULD
- **Evidence**: 39 NuGet/Project issues on ContosoUniversity.csproj; Antlr→Antlr4 is the one true incompatible-package replacement requiring research.
- **Detected**: during task 03-upgrade-web-project breakdown, 2026-09-21

## Breakdown Decisions

### task: 03-upgrade-web-project
Broken into 10 subtasks based on hints: web-controller-migration-units, system-messaging-replacement, web-config-to-appsettings, web-bundling-and-static-assets, large-package-replacement-batch.

Grouping: (1) `03.01-tfm-packages-bindingredirects` — TFM/package/binding-redirect foundation gate. (2) `03.02-systemweb-adapters-scaffolding` — adapters hosting bridge + BaseController (shared by all 7 controllers) + Global.asax/App_Start (Feature.0002/0003/1000) + bundling/static assets (Feature.0001), foundational for all controller subtasks. (3) `03.03-legacy-config-migration` — connection-string/appSettings reads excluding NotificationService.cs (owned by MSMQ subtask instead, same file). (4) `03.04-msmq-replacement` — isolated single-file technology decision, must precede NotificationsController subtask. (5)-(10) one subtask per controller (per MUST hint, no grouping), ordered simplest-first by Api.0001 count: Home(0) → Notifications(0, depends on 03.04) → Instructors(67) → Departments(94) → Students(91) → Courses(146, largest, includes file-upload).
