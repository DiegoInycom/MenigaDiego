using Meniga.Runtime.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meniga.Core.Authentication;
using Microsoft.Practices.Unity;

namespace Ibercaja.Authentication
{
    public class AuthenticationComponent : IMenigaComponent
    {
        public void RegisterBindings(IUnityContainer container)
        {
            container.RegisterType<IAuthenticationHandler, IbercajaAuthenticationHandler>("IbercajaAuthenticationHandler");
        }

        public void RegisterEventListeners(IUnityContainer container, Meniga.Runtime.Events.IEventBus eventBus)
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

