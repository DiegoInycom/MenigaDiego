using Meniga.Runtime.Component;
using Microsoft.Practices.Unity;
using Meniga.Runtime.Events;
using Meniga.Extensions.NotificationFramework;
using Ibercaja.UserEvents.Dispatcher;
using Ibercaja.UserEvents.Notifications.UserEventTypes;
using Ibercaja.UserEvents.Notifications;
using Ibercaja.UserEvents.Iberfabric;
using System.Configuration;
using Ibercaja.UserEvents.Notifications.UserEventTypes.IngresoCuenta;
using Meniga.Core.UserEvents.Extensions;
using Ibercaja.UserEvents.Notifications.UserEventTypes.GastoCuenta;
using System;

namespace Ibercaja.UserEvents
{
    public class UserEventsComponent : IMenigaComponent
    {
        public void RegisterBindings(IUnityContainer container)
        {
            container.RegisterType<INotificationDispatcher, IberfabricNotificationDispatcher>("Ibercaja.UserEvents.Dispatcher.IberfabricNotificationDispatcher,Ibercaja.UserEvents");
            container.RegisterType<INotificationProviderFactory, IbercajaNotificationProviderFactory>();
            container.RegisterType<INotificationProvider, AccountsAvailableAmount>();
            container.RegisterType<INotificationProvider, IngresoCuentaDataAccess>();
            container.RegisterType<INotificationProvider, GastoCuentaDataAccess>();

            RegisterIberfabricChannel(container);
        }

        private static void RegisterIberfabricChannel(IUnityContainer container)
        {
            var endpoint = ConfigurationManager.AppSettings["Ibercaja.Notifications.Endpoint"];
            var channel = ConfigurationManager.AppSettings["Ibercaja.Notifications.Channel"];
            switch (channel.ToUpper())
            {
                case "API":
                    var identityAddress = ConfigurationManager.AppSettings["Ibercaja.Identity.Address"];
                    var identityClientId = ConfigurationManager.AppSettings["Ibercaja.Identity.Client.Id"];
                    var identityClientSecret = ConfigurationManager.AppSettings["Ibercaja.Identity.Client.Secret"];
                    var identityClientScopes = ConfigurationManager.AppSettings["Ibercaja.Identity.Client.Scopes"];

                    container.RegisterType<INotificationService, IberfabricApiNotificationService>(
                        new InjectionConstructor(endpoint, identityAddress, identityClientId, identityClientSecret, identityClientScopes));
                    break;
                case "BUS":
                    var destination = ConfigurationManager.AppSettings["Ibercaja.Notifications.Destination"];
                    container.RegisterType<INotificationService, IberfabricBusNotificationService>(
                        new InjectionConstructor(endpoint, destination));
                    break;
                default:
                    throw new ConfigurationErrorsException("Unknown channel to connect with Iberfabric");
            }
        }

        public void RegisterEventListeners(IUnityContainer container, IEventBus eventBus)
        {
        }

        public void Start(IUnityContainer container)
        {

        }

        public void Stop(IUnityContainer container)
        {

        }
    }
}
