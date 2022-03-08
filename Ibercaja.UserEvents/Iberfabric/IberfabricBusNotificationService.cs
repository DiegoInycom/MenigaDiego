using log4net;
using NServiceBus;
using System;
using System.Configuration;
using System.Threading.Tasks;
using ICommand = Iberfabric.ServiceBus.Messages.ICommand;
using IEvent = Iberfabric.ServiceBus.Messages.IEvent;
using IMessage = Iberfabric.ServiceBus.Messages.IMessage;
using Iberfabric.Notifications;

namespace Ibercaja.UserEvents.Iberfabric
{
    public class IberfabricBusNotificationService : INotificationService
    {
        private readonly IEndpointInstance _bus;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(IberfabricBusNotificationService));

        public IberfabricBusNotificationService(string endpoint, string destination)
        {
            Logger.Debug($"EndpointName ICommand: {endpoint}");
            var endpointConfiguration = new EndpointConfiguration(endpoint);
            endpointConfiguration.UsePersistence<LearningPersistence>();
            endpointConfiguration.ConfigureConventions();
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.UseConventionalRoutingTopology();
            var routingConfig = transport.Routing();
            routingConfig.RouteToEndpoint(
                assembly:typeof(NotificationCommand).Assembly, 
                destination: destination);
            transport.ConnectionString(endpoint);
            transport.Transactions(TransportTransactionMode.None);
            Logger.Debug($"EndpointDestination ICommand: {destination}");

            endpointConfiguration.SendFailedMessagesTo($"{endpoint}.error");
            endpointConfiguration.ConfigureErrorHandling(Logger);
            endpointConfiguration.EnableInstallers();
            
            //This Method can throw new exception from RabbitMq if the endpoint is not available.
            _bus = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
        }

        public async Task<bool> SendNotification(Notification notification)
        {
            var notificationRequest = NotificationCommand.CreateNew(
                notification.NotificationType, 
                Convert.ToInt32(notification.UserNici), 
                notification.NotificationMessage,
                notification.NotificationMetadata, 
                notification.Sender, 
                notification.CreatedOn,
                notification.CreatedEvent, 
                notification.SourceId);

            try
            {
                await _bus.Send(notificationRequest).ConfigureAwait(false);
                Logger.Debug($"Ibercaja.UserEvents.NewNotification: Sended with notification type: {notification.NotificationType}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Ibercaja.UserEvents.NewNotification: Cant send with notification type: {notification.NotificationType}", ex);
                return false;
            }
        }
    }

    public static class IberfabricBusExtensions
    {
        private static ILog Logger;

        public static void ConfigureErrorHandling(this EndpointConfiguration endpointConfiguration, ILog logger)
        {
            Logger = logger;

            var errors = endpointConfiguration.Notifications.Errors;
            errors.MessageSentToErrorQueue += Errors_MessageSentToErrorQueue;

            endpointConfiguration.DefineCriticalErrorAction(context =>
            {
                Logger.Error(context.Error);
                return Task.FromResult(true);
            });
        }

        public static void Errors_MessageSentToErrorQueue(object sender, NServiceBus.Faults.FailedMessage e)
        {
            Logger.Error(e.Exception.Message);
        }

        public static void ConfigureConventions(this EndpointConfiguration endpointConfiguration)
        {
            var conventions = endpointConfiguration.Conventions();

            conventions.DefiningCommandsAs(
                type => typeof(ICommand).IsAssignableFrom(type));
            conventions.DefiningMessagesAs(
                type => typeof(IMessage).IsAssignableFrom(type));
            conventions.DefiningEventsAs(
                type => typeof(IEvent).IsAssignableFrom(type));
        }
    }
}