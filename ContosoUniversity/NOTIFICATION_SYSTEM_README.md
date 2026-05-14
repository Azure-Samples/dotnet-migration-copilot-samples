# Real-Time Admin Notification System

This project includes a real-time notification system that alerts administrators whenever entity operations (create, update, delete) are performed in the system.

## Overview

The notification system uses **Azure Service Bus** as the underlying technology to provide reliable, cloud-native notifications to administrators.

## Features

- **Real-time notifications**: Admins receive immediate notifications when entities are modified
- **Entity coverage**: Monitors Students, Courses, Instructors, and Departments
- **Operation tracking**: Tracks CREATE, UPDATE, and DELETE operations
- **Admin-only**: Only users with administrator role receive notifications
- **Non-intrusive UI**: Notifications appear in the top-right corner with auto-dismiss
- **Reliable delivery**: Uses Azure Service Bus for guaranteed, cloud-scale message delivery
- **Secure access**: Uses Azure DefaultAzureCredential (Managed Identity) — no connection strings

## How It Works

### Backend Components

1. **NotificationService**: Handles Azure Service Bus send/receive using `DefaultAzureCredential`
2. **BaseController**: Base class that all controllers inherit from to send notifications
3. **Notification Model**: Entity to represent notification data
4. **NotificationsController**: API endpoints for retrieving notifications (async)

### Frontend Components

1. **notifications.css**: Styling for notification UI elements
2. **notifications.js**: JavaScript polling system that checks for new notifications
3. **Layout integration**: Admin-only inclusion of notification assets

### Technology Stack

- **Azure Service Bus**: Managed cloud message queue
- **Azure.Identity / DefaultAzureCredential**: Passwordless authentication
- **Entity Framework Core**: Data access
- **ASP.NET Core MVC**: Web framework
- **JavaScript/jQuery**: Frontend polling and UI updates
- **Bootstrap**: UI styling

## Configuration

Add the following to `appsettings.json` (replace placeholders with real values or use environment variables):

```json
{
  "AzureServiceBus": {
    "FullyQualifiedNamespace": "<YOUR_SERVICE_BUS_NAMESPACE>.servicebus.windows.net",
    "QueueName": "contoso-notifications"
  }
}
```

## Queue Details

- **Queue Name**: `contoso-notifications` (configurable via `AzureServiceBus:QueueName`)
- **Authentication**: `DefaultAzureCredential` — supports Managed Identity, environment credentials, Azure CLI, etc.
- **Message Format**: JSON-serialized `Notification` objects (`Newtonsoft.Json`)
- **Message Completion**: Messages are completed (removed) after successful deserialization and processing

## Usage

### For Administrators

1. Log in with an administrator account
2. Navigate to **Notifications** in the main menu to view the dashboard
3. Perform any CRUD operation on entities (Students, Courses, Instructors, Departments)
4. Watch for notifications appearing in the top-right corner
5. Notifications auto-dismiss after 1 minute or can be manually closed

### For Developers

To add notification support to a new controller:

1. Inherit from `BaseController` instead of `Controller`
2. Call `SendEntityNotification()` after successful save operations:

```csharp
// Example: After creating a student
db.Students.Add(student);
db.SaveChanges();
SendEntityNotification("Student", student.ID.ToString(), EntityOperation.CREATE);
```

## Notification Types

- **CREATE**: Green notification for entity creation
- **UPDATE**: Blue notification for entity updates
- **DELETE**: Orange notification for entity deletion

## System Requirements

- .NET 10
- Azure Service Bus namespace with a queue named `contoso-notifications`
- Azure RBAC role `Azure Service Bus Data Owner` (or `Data Sender`/`Data Receiver`) assigned to the app identity
- Azure credential available at runtime (Managed Identity in Azure, or `az login` / environment variables locally)

## Azure Setup

### Create Service Bus Queue

```bash
az servicebus namespace create --name <NAMESPACE> --resource-group <RG> --location <LOCATION>
az servicebus queue create --name contoso-notifications --namespace-name <NAMESPACE> --resource-group <RG>
```

### Assign RBAC Role

```bash
# Assign Azure Service Bus Data Owner to your app identity
az role assignment create \
  --assignee <PRINCIPAL_ID> \
  --role "Azure Service Bus Data Owner" \
  --scope /subscriptions/<SUB>/resourceGroups/<RG>/providers/Microsoft.ServiceBus/namespaces/<NAMESPACE>
```

## Troubleshooting

### Common Issues

1. **Missing namespace config**: Ensure `AzureServiceBus:FullyQualifiedNamespace` is set in `appsettings.json` or environment
2. **Auth failure**: Ensure the app identity has the correct RBAC role on the Service Bus namespace
3. **Queue not found**: Create the queue with the matching name (`contoso-notifications` by default)
4. **No notifications appearing**: Check browser console for JavaScript errors; verify network connectivity to Azure
5. **Local dev auth**: Run `az login` or set `AZURE_CLIENT_ID` / `AZURE_CLIENT_SECRET` / `AZURE_TENANT_ID` env vars

### Development Notes

- Notifications are sent synchronously (blocking) but any failure is swallowed — main operations are unaffected
- Message receive uses a 2-second wait window to batch up to 10 messages per poll
- JavaScript polling occurs every 5 seconds
- Maximum of 5 notifications are displayed simultaneously

## Architecture Benefits

- **Cloud-native**: Fully managed, no local MSMQ Windows dependency
- **Secure**: Passwordless access via Managed Identity / DefaultAzureCredential
- **Reliable**: At-least-once delivery with dead-letter support
- **Scalable**: Azure Service Bus scales to millions of messages per second
- **Observable**: Built-in metrics and diagnostics in Azure Portal

## Future Enhancements

Potential improvements for production use:

1. **SignalR integration**: Real-time push notifications instead of polling
2. **Email notifications**: Send email alerts for critical operations
3. **Notification persistence**: Store notifications in database for audit trail
4. **User preferences**: Allow admins to configure notification types
5. **Topic/Subscription model**: Fan-out to multiple subscribers using Service Bus Topics
6. **Dead-letter monitoring**: Alert on messages that fail processing
