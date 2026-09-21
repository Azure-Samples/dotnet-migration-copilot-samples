# Progress Details: 03.04-msmq-replacement

## Research findings

- `Services/NotificationService.cs` is the sole `System.Messaging` consumer in the codebase (confirmed via workspace search). `Controllers/BaseController.cs` instantiates it directly with `new NotificationService()` (no DI container involved), and `Controllers/NotificationsController.cs` calls `ReceiveNotification()` / `MarkAsRead(id)` — neither needed changes since public signatures were preserved.
- The dispatcher's guidance directed using the `migrating-to-msmq-messaging` skill's prescribed approach (MSMQ.Messaging NuGet package, drop-in namespace swap) rather than building a custom Channels/Sqlite-backed queue, since it's the closer match to the existing at-least-once local-queue semantics.
- Verified via `mcp_upgrade_get_supported_package_version` that `MSMQ.Messaging` 1.0.4 is the net10.0-supported version.
- Verified runtime feasibility: `Get-Service -Name MSMQ` shows the Message Queuing service is installed and **Running** on this machine, so the environment supports MSMQ at runtime (not just at compile time).
- 03.03 (`legacy-config-migration`) explicitly excluded this file's `ConfigurationManager` read and left it for this task, since the replacement technology could change what "queue path" even means. It also established a precedent in `Data/SchoolContextFactory.cs`: read config via `ConfigurationManager.OpenMappedExeConfiguration` against an explicit `legacy.config` file (copied from `Web.config` at build time via the `CopyLegacyConfigForConfigurationManager` MSBuild target), rather than relying on `ConfigurationManager`'s implicit `.exe/.dll.config` discovery convention. Since the queue path setting is unchanged (still a UNC-style MSMQ queue path, not a connection string), this same explicit-file-map pattern was reused here for consistency and reliability.

## Technology decision (Step 1)

Chose **`MSMQ.Messaging` NuGet package** (v1.0.4) as a drop-in replacement for `System.Messaging` — same class names (`MessageQueue`, `Message`, `XmlMessageFormatter`, `MessageQueueAccessRights`, `MessageQueueException`, `MessageQueueErrorCode`), only the namespace changed (`MSMQ.Messaging` instead of `System.Messaging`). This preserves exact at-least-once local-queue semantics with no behavior change, and required no changes to `SendNotification`/`ReceiveNotification`/`MarkAsRead` call sites. Documented in a code comment at the top of `NotificationService.cs`, flagging that the target deployment environment must have the MSMQ Windows feature installed (confirmed present/running in this environment via `Get-Service -Name MSMQ`).

## Changes made

1. **`Services/NotificationService.cs`**
   - Replaced `using System.Messaging;` with `using MSMQ.Messaging;`, with a comment documenting the technology decision and the MSMQ-feature-availability caveat.
   - Replaced the direct `ConfigurationManager.AppSettings["NotificationQueuePath"]` read with a private `GetQueuePath()` helper using `ConfigurationManager.OpenMappedExeConfiguration` against the explicit `legacy.config` file (same pattern as `SchoolContextFactory.GetConnectionString()` from 03.03), reading `AppSettings.Settings["NotificationQueuePath"]`.
   - No changes to `SendNotification`, `ReceiveNotification`, `MarkAsRead`, or `Dispose` — all MSMQ types they reference (`MessageQueueException`, `MessageQueueErrorCode`, `Message`, `MessagePriority`) resolve unchanged from the new namespace.
2. **`ContosoUniversity.csproj`**
   - Removed `<Reference Include="System.Messaging" />`.
   - Added `<PackageReference Include="MSMQ.Messaging" Version="1.0.4" />`.

## Validation

- `dotnet build -c Debug`: 124 pre-existing errors remain, all in `Controllers/*.cs` and `Views/*.cshtml` files using unmigrated `System.Web.Mvc` types (`HttpPostAttribute`, `BindAttribute`, `ActionResult`, `PaginatedList<>`, etc.) — explicitly out of scope for this task (owned by 03.06 and related subtasks). Confirmed via grep of the full build log that **zero** errors/warnings reference `NotificationService.cs`, `MSMQ`, or `System.Messaging` — the migration itself is clean.
- Restore succeeded for `MSMQ.Messaging` 1.0.4 with no NU1xxx warnings for the new package.

## Deviations from task.md

- Step 1 (technology choice) was pre-decided by the dispatcher's explicit note to follow the `migrating-to-msmq-messaging` skill's MSMQ.Messaging package approach rather than evaluate a custom Channels/Sqlite queue, since it's a closer semantic match. This overrides the task.md step 1 default guidance but is consistent with its "if genuinely ambiguous, implement the simplest working replacement and flag the decision" instruction — the decision was not ambiguous once the skill/dispatcher note was read.
- Reused the `OpenMappedExeConfiguration`/`legacy.config` config-read pattern from 03.03 (`SchoolContextFactory`) instead of a plain `ConfigurationManager.AppSettings[...]` call, per task.md's instruction to use "the config-access approach decided in 03.03 if already merged" — it was.

## Decomposition verdict

Loaded the scenario's Execution stage guidance and the `migrating-to-msmq-messaging` / `building-projects` task-related skills before editing; no scenario Breakdown Hints file applicable beyond what's already reflected in the task's own scope note ("this subtask... is isolated from all other work"). The task is confined to one file (`NotificationService.cs`) plus a two-line csproj change, with a single technology decision already resolved by the dispatcher — no independent sub-concerns, dependency ordering, or internal decision points remain. Verdict: **atomic** — executed directly, no `TaskBreaker` escalation.
