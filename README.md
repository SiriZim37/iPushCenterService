# iPushCenterService - Push Notification Service

`iPushCenterService` is a VB.NET service designed for sending push notifications to users based on data stored in a database. It handles the validation of request keys, retrieves necessary customer details from the database, and sends notifications via Firebase Cloud Messaging (FCM).

## Features

- **Key Validation**: Ensures the provided request key is valid before processing the notification.
- **Database Integration**: Retrieves customer-specific details, such as FCM tokens and customer IDs, from the database.
- **Multi-Language Support**: Sends notifications in multiple languages (e.g., Thai, English).
- **Error Handling**: Gracefully handles errors during data retrieval and notification sending.

## Methods

### TestPush
Triggers a test push notification to verify the notification process.

### CenterPushNotification
- **Parameters**: Accepts a `key` (string) for validation and a `jsonStr` (string) containing the notification details.
- **Functionality**:
  - Validates the provided key.
  - Deserializes the `jsonStr` to extract notification data.
  - Retrieves FCM tokens and customer details from the database.
  - Sends notifications to customers with the relevant message.

## Dependencies
- **.NET Framework**: Ensure you're using the correct .NET version for compatibility.
- **Database**: Requires a SQL Server database to store and retrieve notification data.

## Usage

1. **Test Push Notification**:
   - Call the `TestPush` method to send a test notification.
   
2. **Send Push Notification**:
   - Call the `CenterPushNotification` method with the appropriate parameters to send a notification to one or more users.

### Example

```vb.net
Dim service As New iPushCenterService()
service.TestPush() ' Call to trigger a test notification

Dim result As String = service.CenterPushNotification("your-validation-key", yourJsonString)
Console.WriteLine(result)

//
