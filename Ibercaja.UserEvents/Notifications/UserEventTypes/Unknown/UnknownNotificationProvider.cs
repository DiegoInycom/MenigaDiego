using System;
using System.Collections.Generic;

namespace Ibercaja.UserEvents.Notifications
{
    public class UnknownNotificationProvider : INotificationProvider
    {
        Notification INotificationProvider.GetNotification(string userIdentifier, long personId, IDictionary<string, object> userEvent, long userEventId, DateTime? createdEvent, string message)
        {
            //We return null Notification beacuse we want to ignore all Unknown Notifications
            return null;
        }
    }
}