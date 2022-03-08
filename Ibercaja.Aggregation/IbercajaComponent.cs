using Ibercaja.Aggregation.UserDataConnector;
using Ibercaja.Aggregation.UserDataConnector.Configuration;
using Meniga.Runtime.Component;
using Meniga.Runtime.Events;
using Microsoft.Practices.Unity;
using System.Configuration;
using Ibercaja.Aggregation.Security;
using Ibercaja.Aggregation.Eurobits.Service;

namespace Ibercaja.Aggregation
{
    class IbercajaComponent : IMenigaComponent
    {
        public void RegisterBindings(IUnityContainer container)
        {
            container.RegisterType<IPersonAggregationErrors, PersonAggregationErrors>(new ContainerControlledLifetimeManager());
            container.RegisterType<IAccountRepository, StatelessCoreContextAccountRepository>();
            container.RegisterType<ISynchronizationStatusProvider, StatelessCoreContextSynchronizationStatusProvider>();
            container.RegisterType<IUserDataConnectorConfiguration, UserDataConnectorConfiguration>();
            RegisterSecurityService(container);
            RegisterEurobitsApi(container);
        }

        private void RegisterSecurityService(IUnityContainer container)
        {
            string euroBitsEncryptionFile = ConfigurationManager.AppSettings["EurobitsEncryptionFile"];
            string euroBitsCertificateFile = ConfigurationManager.AppSettings["EurobitsCertificateFile"];

            container.RegisterType<ISecurityService, SecurityService>(
                new InjectionConstructor(euroBitsEncryptionFile, euroBitsCertificateFile));
        }

        private static void RegisterEurobitsApi(IUnityContainer container)
        {
            string eurobitsCertificateAlias = ConfigurationManager.AppSettings["EurobitsCertificateAlias"];
            string urlEurobitsApiBaseAddress = ConfigurationManager.AppSettings["EurobitsApiUrlBase"];
            string eurobitsApiServiceId = ConfigurationManager.AppSettings["EurobitsApiServiceId"];
            string encryptedPassword = ConfigurationManager.AppSettings["EurobitsApiPassword"];

            string decryptedPassword = container.Resolve<ISecurityService>().DecryptValue(encryptedPassword);

            container.RegisterType<IEurobitsApiService, EurobitsApiService>("Default",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(urlEurobitsApiBaseAddress, eurobitsCertificateAlias, eurobitsApiServiceId, decryptedPassword));

            container.RegisterType<IEurobitsApiService, DummyEurobitsApiService>("Dummy",
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(urlEurobitsApiBaseAddress, eurobitsCertificateAlias, eurobitsApiServiceId, decryptedPassword));
        }

        public void RegisterEventListeners(IUnityContainer container, IEventBus eventBus)
        {
        }

        public void Start(IUnityContainer container)
        {
            log4net.GlobalContext.Properties["assemblyversion"] = new AssemblyVersionLogContextProperty();
        }

        public void Stop(IUnityContainer container)
        {
        }
    }
}