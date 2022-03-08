using System.Collections.Generic;
using Ibercaja.UserEvents.Notifications;
using log4net;
using Meniga.Component.NotificationFramework.Contracts.Extensions;
using Meniga.Core.Users;
using Meniga.Extensions.NotificationFramework;
using Ibercaja.UserEvents.Iberfabric;
using System.Configuration;
using System.Linq;
using Meniga.Core.UserEvents.Core.Models;
using Meniga.Core.UserEvents.Core.DataAccess;
using System;
using Newtonsoft.Json;
using Meniga.Core.BusinessModels;
using Ibercaja.UserEvents.Notifications.UserEventTypes;
using Meniga.Component.NotificationFramework.Framework;

namespace Ibercaja.UserEvents.Dispatcher
{
    public class IberfabricNotificationDispatcher : INotificationDispatcher
    {
        private readonly IUserEventsDataAccess _userEventDataAccess;
        private readonly ICoreUserManager _coreUserManager;
        private readonly INotificationProviderFactory _notificationProviderFactory;
        private readonly INotificationService _notificationService;
        private readonly INotificationResourceService _notificationResourceService;
        private readonly INotificationDispatcherContext _notificationDispatcherContext;
        private static readonly IEnumerable<string> _silencedAccountsIds = ConfigurationManager.AppSettings["Ibercaja.Notifications.SilencedAccountsIdTypes"]?.Split(',').AsEnumerable() ?? Enumerable.Empty<string>();
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IberfabricNotificationDispatcher));
        
        public IberfabricNotificationDispatcher(
            IUserEventsDataAccess userEventDataAccess,
            ICoreUserManager coreUserManager,
            INotificationProviderFactory notificationProviderFactory,
            INotificationService notificationService,
            INotificationResourceService notificationResourceService,
            INotificationDispatcherContext notificationDispatcherContext)
        {
            _userEventDataAccess = userEventDataAccess;
            _coreUserManager = coreUserManager;
            _notificationProviderFactory = notificationProviderFactory;
            _notificationService = notificationService;
            _notificationResourceService = notificationResourceService;
            _notificationDispatcherContext = notificationDispatcherContext;
        }

        public void Dispatch(string context, long personId)
        {
        }

        IEnumerable<long> INotificationDispatcher.Dispatch(List<Alert> alerts)
        {
            Logger.Debug("ENTRA EN DISPATCH(ALERT)");
            List<long> fails = new List<long>();
            foreach (var alert in alerts)
            {
                // Obtain userIdentifier = Nici Ibercaja
                var realmUser = GetRealmUser(alert);
                var notificationType = alert.UserEventMessage.UserEventTypeId.ToString();
                // Obtain notificationProvider
                var notificationProvider = _notificationProviderFactory.CreateNotificationProvider(notificationType);
                //Create Delegate Function to Retrieve the EventType Object
                Func<int, UserEventTypeModel> getUserEventType = delegate (int id)
                {
                    return _userEventDataAccess.GetUserEventTypes(false).First(a => a.Id == id);
                };
                // Serialize alert.Data
                var userEvent = _userEventDataAccess.GetUserEvent(alert.PersonId, realmUser.UserId, alert.UserEventMessage.UserEventId, alert.UserEventMessage.AlertChannelId, getUserEventType);
                var userEventData = JsonConvert.DeserializeObject<Dictionary<string, object>>(userEvent.UserEventDataSerialized);
                var message = GetNotificationMessage(alert, userEvent);
                // Create new notification
                var notification = notificationProvider.GetNotification(realmUser.UserIdentifier, realmUser.UserId, userEventData, alert.UserEventMessage.UserEventId, userEvent.Date, message);
                if (notification != null)
                {
                    if (CheckNotificationNotSilenced(notification))
                    {
                        
                         // Send notification to Ibercaja Bus
                         var sended = _notificationService.SendNotification(notification).GetAwaiter().GetResult();
                         if (!sended)
                         {
                             throw new Exception($"The message for the notification type: {notification.NotificationType},  userNici {realmUser.UserIdentifier}, notificationMessage: {notification.NotificationMessage}, notificationMetadata: {notification.NotificationMetadata}, sourceId:{notification.SourceId} has NOT been sent correctly.");
                         }
                         else 
                         {
                             Logger.Debug($"The message for the notification type: {notification.NotificationType},  userNici {realmUser.UserIdentifier}, notificationMessage: {notification.NotificationMessage}, notificationMetadata: {notification.NotificationMetadata}, sourceId:{notification.SourceId} has been sent.");
                         }
                     
                    }
                }
                else
                {
                    fails.Add(alert.Id);
                }
            }
            return fails;
        }

        private string GetNotificationMessage(Alert alert, UserEventModel userEvent)
        {
            Logger.Debug("ENTRA EN GETNOTIFICATIONMESSAGE");
            var dispatcherContext = _notificationDispatcherContext.GetDispatcherContext(alert.PersonId, userEvent.Id, userEvent.ChannelId);
            var message = _notificationResourceService.GetTranslation(dispatcherContext.UserEventContentData.TitleResourceKey, dispatcherContext.PersonCulture);
            if (!string.IsNullOrEmpty(message) && dispatcherContext.UserEventData != null)
            {
                message = TemplateRender.StringToString(message, dispatcherContext.UserEventData);
            }

            return message;
        }

        private static bool CheckNotificationNotSilenced(Notification notification)
        {
            if (IbercajaUserEventTypes.AccountsAvailableAmount == notification.NotificationType && notification.AccountTypeId == 3)
            {
                if (!_silencedAccountsIds.Any(a => a == notification.Identifier?.Substring(5, 2)))
                    return true;
                else
                    return false;
            }

            return true;
        }

        public RealmUser GetRealmUser(Alert alert)
        {
            var personId = alert.PersonId;

            // Used to get the identifier
            var allRealmUsers = _coreUserManager.GetRealmUsers(personId);
            var realmUser = allRealmUsers.Find(r => r.RealmId == 2);

            return realmUser;
        }
    }
}
