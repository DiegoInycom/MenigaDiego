using System.Collections.Generic;
using System.Configuration;
using Meniga.Core.BusinessModels;
using Meniga.Runtime.IOC;
using Newtonsoft.Json;
using Ibercaja.Aggregation.Eurobits.Service;
using Ibercaja.Aggregation.UserDataConnector.Configuration;
using Meniga.Core.Users;
using Ibercaja.Aggregation.Security;

namespace Ibercaja.Aggregation.UserDataConnector
{
    public class EurobitsUserDataConnectorFake : EurobitsUserDataConnector
    {
        public EurobitsUserDataConnectorFake(string data, RealmUser realmUser)
            : base(
                  GetEurobitsApi(),
                  GetConfigurationRealm(data),
                  GetInvertConfigurationConfig(),
                  realmUser, 
                  GetSynchronizationStatusProvider(),
                  GetUserManager(),
                  GetSecurityService()
                  )
        {
        }

        private static ISynchronizationStatusProvider GetSynchronizationStatusProvider()
        {
            return IoC.Resolve<ISynchronizationStatusProvider>();
        }

        private static IEurobitsApiService GetEurobitsApi()
        {
            return new FakeEurobitsApiService();
        }

        private static IDictionary<string, string> GetInvertConfigurationConfig()
        {
            string invertAmountConfigJson = ConfigurationManager.AppSettings["InvertTrxAmountConfiguration"] ?? "{}";
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(invertAmountConfigJson);
        }

        private static UserDataConnectorConfigurationRealm GetConfigurationRealm(string data)
        {
            var userDataConnectorConfiguration = IoC.Resolve<IUserDataConnectorConfiguration>();
            userDataConnectorConfiguration.TryDeserializeConfigurationFromJson(data);
            return userDataConnectorConfiguration.GetValidatedConfiguration();
        }

        private static ICoreUserManager GetUserManager()
        {
            return IoC.Resolve<ICoreUserManager>();
        }
        private static ISecurityService GetSecurityService()
        {
            return IoC.Resolve<ISecurityService>();
        }
    }
}