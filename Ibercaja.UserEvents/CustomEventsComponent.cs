using Meniga.Runtime.Component;
using Microsoft.Practices.Unity;
using Meniga.Runtime.Events;
using Meniga.Core.UserEvents.Extensions;
using Ibercaja.UserEvents.Notifications.UserEventTypes.IngresoCuenta;
using Ibercaja.UserEvents.Notifications.UserEventTypes.GastoCuenta;
using System;

namespace Ibercaja.UserEvents
{
    public class CustomEventsComponent : IMenigaComponent
    {
        public void RegisterBindings(IUnityContainer container)
        {
            container.RegisterType<IUserEventTypeFactory, IngresoCuentaFactory>("IngresoCuentaFactory", new ContainerControlledLifetimeManager());
            container.RegisterType<IUserEventTypeFactory, GastoCuentaTypeFactory>("GastoCuentaTypeFactory", new ContainerControlledLifetimeManager(), Array.Empty<InjectionMember>());//, new InjectionConstructor(new InjectionParameter<int?>(null))
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
