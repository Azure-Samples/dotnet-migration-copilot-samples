# 03.04-msmq-replacement: Replace MSMQ (System.Messaging) usage in NotificationService.cs with a net10.0-compatible queue implementation

## Objective
Resolve `Feature.0008` and the 62 associated `Api.0001`/`Api.0002` issues in `Services/NotificationService.cs` by replacing `System.Messaging` (MSMQ — `MessageQueue`, `MessageQueueException`, `MessageQueueAccessRights`, `MessageQueueErrorCode`) with a net10.0-supported implementation. This file is the sole MSMQ consumer in the codebase and is isolated from all other work — a technology decision is required per the `system-messaging-replacement` MUST hint.

This subtask must complete before subtask 03.06 (`NotificationsController` migration), which calls `NotificationService.ReceiveNotification()` / `MarkAsRead()` directly.

## Scope
`Services/NotificationService.cs` only, including its own `ConfigurationManager.AppSettings["NotificationQueuePath"]` read (kept here rather than in 03.03 since it's the same file and the replacement technology likely changes what this setting even means — e.g. a connection string vs. a queue name). Remove the plain `<Reference Include="System.Messaging">` from the csproj once no longer used.

## Steps
1. Choose a replacement: since this is a single-project, single-file in-place migration (no broker infrastructure evidenced elsewhere in the repo), default to the simplest supported option that preserves at-least-once local queueing semantics (e.g. `System.Threading.Channels` backed by a lightweight persistent store, or `Microsoft.Data.Sqlite`-backed queue) **unless** the user/assessment indicates a real broker (Azure Service Bus / RabbitMQ) is expected — if genuinely ambiguous, implement the simplest working replacement and flag the decision explicitly in a code comment plus a note in this task's completion summary, per the parent task's "flag any MSMQ code where the concrete queue replacement needs a follow-up decision" instruction.
2. Replace `MessageQueue.Exists`/`.Create`/`.Send`/`.Receive` and related MSMQ types with the chosen implementation, preserving the existing public method signatures (`SendNotification`, `ReceiveNotification`, `MarkAsRead`) so `BaseController` and `NotificationsController` require no changes beyond what 03.02/03.06 already handle.
3. Replace the `ConfigurationManager.AppSettings["NotificationQueuePath"]` read with whatever config value the new implementation needs (path, connection string, etc.), using the config-access approach decided in 03.03 if already merged, or `ConfigurationManager` directly otherwise.
4. Remove the `System.Messaging` `<Reference>` from `ContosoUniversity.csproj`.
5. Build and verify `NotificationService.cs` compiles with 0 API issues.

## Done when
- All 63 issues on `Services/NotificationService.cs` (`Feature.0008` + 57 `Api.0001` + 5 `Api.0002`) are resolved.
- Public method signatures (`SendNotification`, `ReceiveNotification`, `MarkAsRead`) are unchanged.
- `System.Messaging` reference is removed from the csproj.
- The replacement technology decision is documented (comment + task note).

## Research findings (added during execution)
- `NotificationService.cs` is the sole `System.Messaging` consumer; `BaseController` instantiates it directly (`new NotificationService()`, no DI), so the parameterless constructor must be preserved.
- Per the dispatcher's guidance, followed the `migrating-to-msmq-messaging` skill's default: `MSMQ.Messaging` NuGet package (v1.0.4, confirmed net10.0-supported), a namespace-only drop-in replacement — no custom Channels/Sqlite queue was built.
- Confirmed the MSMQ Windows service is installed and running on this dev machine (`Get-Service -Name MSMQ`), so the migrated code is also runtime-testable here, not just compile-testable.
- Reused 03.03's `ConfigurationManager.OpenMappedExeConfiguration` + explicit `legacy.config` pattern (from `SchoolContextFactory`) for the `NotificationQueuePath` read, instead of the implicit `ConfigurationManager.AppSettings[...]` convention, for consistency with the rest of the codebase.
- See `progress-details.md` for full detail.
