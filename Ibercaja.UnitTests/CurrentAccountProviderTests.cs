using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Ibercaja.Aggregation.Eurobits;
using Ibercaja.Aggregation.Products;
using Ibercaja.Aggregation.Products.Current;
using Ibercaja.Aggregation.Eurobits.Service;
using Account = Meniga.Core.Data.User.Account;
using Ibercaja.Aggregation.UserDataConnector.Configuration;
using Ibercaja.UnitTests.Helpers;

namespace Ibercaja.UnitTests
{
    [TestClass]
    public class CurrentAccountProviderTests
    {
        private IAccountsProvider _accountsProvider;
        private List<Account> _existingAccounts;
        private Mock<IEurobitsApiService> _api;

        [TestMethod]
        public void NoAccountReturnEmptyCollection()
        {
            _api.Setup(x => x.GetAggregation(It.IsAny<string>()))
                .Returns(Task.FromResult(new AggregationResponse()));
            Assert.AreEqual(0, _accountsProvider.GetBankAccountInfos().Count());
        }

        [TestMethod]
        public void SingleAccount()
        {
            Assert.AreEqual(1, _accountsProvider.GetBankAccountInfos().Count());
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _existingAccounts = new List<Account>();
            _api = new Mock<IEurobitsApiService>();
            _api.Setup(x => x.GetAggregation(It.IsAny<string>()))
                .Returns(Task.FromResult(new AggregationResponse
                {
                    Accounts = new[]
                    {
                        new Aggregation.Eurobits.Account
                        {
                            AccountNumber = "a1",
                            Balance = new Amount
                            {
                                Value = "2"
                            }
                        }
                    }
                }));

            var configuration = new UserDataConnectorConfiguration();
            configuration.TryDeserializeConfigurationFromJson(DataMock.Data);

            var aggregationService = new EurobitsAggregationService(_api.Object, "111", configuration.GetValidatedConfiguration());
            var userDocument = "000814788@pfm.ibercaja.es";

            _accountsProvider = new CurrentAccountProvider(aggregationService, userDocument);
        }
    }
}
