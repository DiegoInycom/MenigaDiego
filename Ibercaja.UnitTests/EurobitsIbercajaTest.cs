using System;
using System.Linq;
using System.Net;
using Meniga.Core.BusinessModels;
using Meniga.Core.DataConsolidation;
using Meniga.Runtime.IOC;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ibercaja.Aggregation;
using Ibercaja.Aggregation.UserDataConnector;
using Ibercaja.Aggregation.Eurobits;
using Ibercaja.Aggregation.UserDataConnector.Configuration;
using Ibercaja.UnitTests.Helpers;
//using IberCaja.UnitTests.Mocks;
using Ibercaja.Aggregation.Eurobits.Service;
using System.Threading.Tasks;
using Ibercaja.Aggregation.Security;

namespace Ibercaja.UnitTests
{
    [TestClass]
    public class EurobitsUserDataConnectorTests
    {
        private Mock<IEurobitsApiService> _mockEurobits;

        [TestInitialize]
        public void Initialize()
        {
            _mockEurobits = new Mock<IEurobitsApiService>();
            _mockEurobits
                .Setup(_ => _.Login())
                .Returns(Task.FromResult(new JwtAuthResponse { Token = "123456" }));
        }

        [TestMethod]
        public void GetAccountInfoWithoutSessionTokenTest()
        {
            _mockEurobits
                .Setup(_ => _.GetAggregation(string.Empty))
                .Returns(Task.FromResult(null as AggregationResponse));
            var container = CreateAndInitializeContainer(DataMock.Data, DataMock.RealmUser, _mockEurobits.Object);
            var connector = container.Resolve<IAggregationAgent>();

            var response = connector.GetAccountInfo(DataMock.LoginParameters[0].Value);

            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Length);
        }

        [TestMethod]
        public void GetAccountInfoWithSessionTokenTest()
        {
            _mockEurobits
                .Setup(_ => _.GetAggregation(DataMock.SessionToken))
                .Returns(Task.FromResult(DataMock.AggregationResponse));
            var container = CreateAndInitializeContainer(DataMock.Data, DataMock.RealmUser, _mockEurobits.Object);
            var connector = container.Resolve<IAggregationAgent>();

            connector.SetSessionToken(DataMock.SessionToken);
            var response = connector.GetAccountInfo(DataMock.LoginParameters[0].Value);

            Assert.IsNotNull(response);
            Assert.AreEqual(3, response.Length);
            Assert.AreEqual(AccountCategoryEnum.Current, response[0].AccountCategory);
            Assert.AreEqual(AccountCategoryEnum.Savings, response[1].AccountCategory);
            Assert.AreEqual(AccountCategoryEnum.Credit, response[2].AccountCategory);
            //Assert.AreEqual(AccountCategoryEnum.Credit,  response[3].AccountCategory);
        }

        [TestMethod]
        public void GetAccountStatementWithoutSessionTokenTest()
        {
            _mockEurobits
                .Setup(_ => _.GetAggregationStatus(null))
                .Returns(Task.FromResult(HttpStatusCode.OK));
            _mockEurobits
                .Setup(_ => _.GetAggregation(null))
                .Returns(Task.FromResult(DataMock.AggregationResponseEmpty));
            var container = CreateAndInitializeContainer(DataMock.Data, DataMock.RealmUser, _mockEurobits.Object);
            var connector = container.Resolve<IAggregationAgent>();

            var response = connector.GetAccountStatement(AccountCategoryEnum.Current, null, null, DateTime.Now, DateTime.Now);

            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Balance);
            Assert.IsNull(response.LastSyncToken);
            Assert.AreEqual(0, response.Limit);
            Assert.IsNull(response.Payable);
            Assert.AreEqual(DateTime.MinValue, response.PayableDate);
            Assert.IsNull(response.Transactions);
        }

        [TestMethod]
        public void SetSessionTokenDontStartNewAggregationTest()
        {
            _mockEurobits.SetupGet(_ => _.ConfigurationIsCorrect).Returns(true);
            var executionId = DataMock.SessionToken;
            _mockEurobits
                .Setup(_ => _.GetAggregationStatus(executionId))
                .Returns(Task.FromResult(HttpStatusCode.OK));
            _mockEurobits
                .Setup(_ => _.GetAggregation(executionId))
                .Returns(Task.FromResult(DataMock.AggregationResponse));
            var container = CreateAndInitializeContainer(DataMock.Data, DataMock.RealmUser, _mockEurobits.Object);
            var connector = container.Resolve<IAggregationAgent>();

            connector.SetSessionToken(DataMock.SessionToken);
            var response = connector.Authenticate(DataMock.LoginParameters);

            Assert.IsNotNull(response);
            Assert.AreEqual(DataMock.SessionToken, response.SessionToken);
        }

        [TestMethod]
        public void StartSessionReturnsSessionTokenTest()
        {
            var container = CreateAndInitializeContainer(DataMock.Data, DataMock.RealmUser, _mockEurobits.Object);
            var connector = container.Resolve<IAggregationAgent>();

            connector.SetSessionToken(DataMock.SessionToken);
            var response = connector.StartSession(DataMock.LoginParameters);

            Assert.IsNotNull(response);
            Assert.AreEqual(DataMock.SessionToken, response);
        }

        [TestMethod]
        public void EndSessionRemovePreviousSessionTokenTest()
        {
            var container = CreateAndInitializeContainer(DataMock.Data, DataMock.RealmUser, _mockEurobits.Object);
            var connector = container.Resolve<IAggregationAgent>();

            connector.SetSessionToken(DataMock.SessionToken);
            connector.EndSession();
            var response = connector.StartSession(DataMock.LoginParameters);

            Assert.IsNotNull(response);
            Assert.AreNotEqual(DataMock.SessionToken, response);
        }

        [TestMethod]
        public void UserIsRequiredParameterTest()
        {
            _mockEurobits.SetupGet(_ => _.ConfigurationIsCorrect).Returns(true);
            _mockEurobits
                .Setup(_ => _.GetRobotInfo(DataMock.BankName))
                .Returns(Task.FromResult(new RobotDetailsResponse
                {
                    GlobalParameters = new GlobalParameters
                    {
                        Params = new Param[]
                        {
                            new Param
                            {
                                Name = "User",
                                Description = "User",
                                Required = true
                            }
                        }
                    }
                }));
            _mockEurobits
                .Setup(_ => _.GetAggregation(It.IsAny<string>()))
                .Returns(Task.FromResult(new AggregationResponse
                {
                    AggregationInfo = new AggregationInfo
                    {
                        Code = "400",
                        Message = "Bad Request"
                    }
                }));
            var container = CreateAndInitializeContainer(DataMock.Data, DataMock.RealmUser, _mockEurobits.Object);
            var connector = container.Resolve<IAggregationAgent>();

            var response = connector.Authenticate(new Parameter[] { });

            Assert.AreEqual(1, response.RequiredParameters.Length); // Usuario should be the only required parameter
        }

        [TestMethod]
        public void ParametersStoredInAuthenticationTest()
        {
            _mockEurobits.SetupGet(_ => _.ConfigurationIsCorrect).Returns(true);
            _mockEurobits
                .Setup(_ => _.GetRobotInfo(DataMock.BankName))
                .Returns(Task.FromResult(new RobotDetailsResponse
                {
                    GlobalParameters = new GlobalParameters
                    {
                        Params = new Param[]
                        {
                            new Param
                            {
                                Name = "User",
                                Description = "User",
                                Encoded = true,
                                Required = true
                            }
                        }
                    }
                }));
            _mockEurobits
                .Setup(_ => _.GetAggregationStatus(It.IsAny<string>()))
                .Returns(Task.FromResult(HttpStatusCode.OK));
            _mockEurobits
                .Setup(_ => _.GetAggregation(It.IsAny<string>()))
                .Returns(Task.FromResult(new AggregationResponse
                {
                    AggregationInfo = new AggregationInfo
                    {
                        Code = "R000",
                        Message = "OK"
                    }
                }));
            var container = CreateAndInitializeContainer(DataMock.Data, DataMock.RealmUser, _mockEurobits.Object);
            var connector = container.Resolve<IAggregationAgent>();

            connector.Authenticate(DataMock.LoginParameters);
            var response = connector.GetParameters();

            Assert.AreEqual(1, response.Length);
            Assert.AreEqual(DataMock.LoginParameters[0].Name, response[0].Name);
            Assert.AreEqual(DataMock.LoginParameters[0].Value, response[0].Value);
        }

        [TestMethod]
        public void GetBasicInfoTest()
        {
            var container = CreateAndInitializeContainer(DataMock.Data, DataMock.RealmUser, _mockEurobits.Object);
            var connector = container.Resolve<IAggregationAgent>();

            var response = connector.GetBasicInfo();

            Assert.IsNotNull(response);
            Assert.AreNotEqual(DataMock.LoginParameters[0].Name, response);
        }

        [TestMethod]
        public void TransactionsHaveIdentifierNullTest()
        {
            _mockEurobits
                .Setup(_ => _.GetAggregationStatus(DataMock.SessionTokenAccounts))
                .Returns(Task.FromResult(HttpStatusCode.OK));
            _mockEurobits
                .Setup(_ => _.GetAggregation(DataMock.SessionTokenAccounts))
                .Returns(Task.FromResult(DataMock.AggregationResponseOnlyAccounts));
            var container = CreateAndInitializeContainer(DataMock.Data, DataMock.RealmUser, _mockEurobits.Object);
            var connector = container.Resolve<IAggregationAgent>();

            connector.SetSessionToken(DataMock.SessionTokenAccounts);
            var response = connector.GetAccountStatement(AccountCategoryEnum.Current, null, "Account", DateTime.Now, DateTime.Now);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Transactions);
            Assert.AreEqual(2, response.Transactions.Count(t => t.Identifier == null));
            Assert.IsFalse(response.Transactions.Where(t => !string.IsNullOrEmpty(t.Identifier)).GroupBy(t => t.Identifier).Any());
        }

        [TestMethod]
        public void AccountsRecoveredWhenCreditCardsFromEBAreNullTest()
        {
            var executionId = DataMock.SessionTokenAccounts;
            _mockEurobits
                .Setup(_ => _.GetAggregationStatus(executionId))
                .Returns(Task.FromResult(HttpStatusCode.OK));
            _mockEurobits
                .Setup(_ => _.GetAggregation(executionId))
                .Returns(Task.FromResult(DataMock.AggregationResponseOnlyAccounts));
            var container = CreateAndInitializeContainer(DataMock.Data, DataMock.RealmUser, _mockEurobits.Object);
            var connector = container.Resolve<IAggregationAgent>();

            connector.SetSessionToken(DataMock.SessionTokenAccounts);
            var response = connector.GetAccountStatement(AccountCategoryEnum.Current, null, "Account", DateTime.Now, DateTime.Now);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Transactions);
        }

        [TestMethod]
        public void CreditCardsRecoveredWhenAccountsFromEBAreNullTest()
        {
            var executionId = DataMock.SessionTokenCreditCards;
            _mockEurobits
                .Setup(_ => _.GetAggregationStatus(executionId))
                .Returns(Task.FromResult(HttpStatusCode.OK));
            _mockEurobits
                .Setup(_ => _.GetAggregation(executionId))
                .Returns(Task.FromResult(DataMock.AggregationResponseOnlyCreditCards));
            var container = CreateAndInitializeContainer(DataMock.Data, DataMock.RealmUser, _mockEurobits.Object);
            var connector = container.Resolve<IAggregationAgent>();

            connector.SetSessionToken(DataMock.SessionTokenCreditCards);
            var response = connector.GetAccountStatement(AccountCategoryEnum.Credit, null, "0000-0000-0000-0000", DateTime.Now, DateTime.Now);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Transactions);
        }

        private static UnityContainer CreateAndInitializeContainer(string data, RealmUser realmUser, IEurobitsApiService handleEurobitsAPI = null)
        {
            var mockPersonAggregationErrors = new Mock<IPersonAggregationErrors>();
            var container = new UnityContainer();
            container.RegisterType<IAccountRepository, InMemoryAccountRepository>();
            container.RegisterType<ISecurityService, NoEncryptionSecurityService>();
            container.RegisterType<ISynchronizationStatusProvider, InMemorySychronizationStatusProvider>();
            container.RegisterType<IUserDataConnectorConfiguration, UserDataConnectorConfiguration>();

            container.RegisterInstance(mockPersonAggregationErrors.Object);
            container.RegisterInstance("Default", handleEurobitsAPI);
            IoC.Initialize(new UnityDependencyResolver(container));
            container.RegisterInstance<IAggregationAgent>(new EurobitsUserDataConnectorDefault(data, realmUser));

            return container;
        }
    }
}
