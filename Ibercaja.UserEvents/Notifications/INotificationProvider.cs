using System;
using System.Collections.Generic;

namespace Ibercaja.UserEvents.Notifications
{
    /// <summary>
    ///     General purpose interface for implementing notification specific logic
    ///     for each user event type
    /// </summary>
    public interface INotificationProvider
    {
        /// <summary>
        ///     Notification specific logic for each user event type
        /// </summary>
        /// <returns></returns>
        Notification GetNotification(string userIdentifier, long userId, IDictionary<string,object> userEvent, long userEventId, DateTime? createdEvent, string message);
    }
}