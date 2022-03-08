using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Ibercaja.Aggregation.UserDataConnector.Configuration;

namespace Ibercaja.UnitTests
{
    [TestClass]
    public class UserDataConnectorConfigurationTests
    {
        [TestMethod]
        public void UserDataConnectorConfiguration_InsertValidConnectionClassData_ShouldReturnValidConfiguration()
        {
            //Arrange
            string connectionClassData =
                "{\"productsToFetch\": [\"Accounts\", \"AccountHolders\", \"DebitCards\", \"CreditCards\",\"Deposits\"], \"invertAmount\": \"0\", \"bank\": \"National Bank of Greece\", \"userIdentifier\": \"user\", \"textReplacePatterns\": [{\"pattern\": \"Pago (.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"CAJERO (.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"ABONO A COMPRADOR POR DEVOLUCION - (.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"RECIBO (.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"DEVOLUCION\\\\s*(TAR.)*\\\\s*\\\\d{4}X+\\\\d{4} \\\\d+.\\\\d+ (.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"COMISIONES[\\\\d\\\\s]+(.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"^\\\\d{2,}[-\\\\s]*(.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"^COMPRA TARJ. (.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"^[Xx\\\\d-]{4,}(?=[\\\\D]+)-*\\\\s*(.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"TRANSFERENCIA\\\\s*(A\\\\s+|a\\\\s+|DE\\\\s+|de\\\\s+|){0,1}\\\\d\\\\d\\\\w\\\\d{12}\\\\w\\\\d\\\\d(.*)\", \"isMerchant\": true, \"replace\": \"$2\"},{\"pattern\": \"TRANSFERENCIA\\\\s*(A\\\\s+|a\\\\s+|DE\\\\s+|de\\\\s+|(\\\\d{2,}[\\\\w\\\\d\\\\S]*)){0,1}\\\\s*(.*)\", \"isMerchant\": true, \"replace\": \"$3\"}]}";
            IUserDataConnectorConfiguration configuration = new UserDataConnectorConfiguration();
            configuration.TryDeserializeConfigurationFromJson(connectionClassData);
            List<string> expectedProducts = new List<string>()
                {"Accounts", "AccountHolders", "DebitCards", "CreditCards", "Deposits"};

            //Act
            UserDataConnectorConfigurationRealm configurationRealm = configuration.GetValidatedConfiguration();

            //Assert
            Assert.IsTrue(configurationRealm.ProductsToFetch.All(x => expectedProducts.Contains(x)));
            Assert.AreEqual(expectedProducts.Count, configurationRealm.ProductsToFetch.Count());
            Assert.IsFalse(configurationRealm.InvertAmount);
            Assert.AreEqual("National Bank of Greece", configurationRealm.Bank);
            Assert.AreEqual("user", configurationRealm.UserIdentifier);
            Assert.AreEqual(11, configurationRealm.TextReplacePatterns.Count);
            Assert.IsNull(configurationRealm.MonthlyLoginLimit);
        }

        [TestMethod]
        public void UserDataConnectorConfiguration_InsertValidUserIdentifiersInConnectionClassData_ShouldReturnValidConfiguration()
        {
            //Arrange
            var userIdentifiers = new List<string> { "user", "username" };

            userIdentifiers.ForEach(userIdentifier =>
            {
                string connectionClassData =
                    $"{{\"userIdentifier\": \"{userIdentifier}\", \"invertAmount\": \"0\", \"bank\": \"National Bank of Greece\"}}";
                IUserDataConnectorConfiguration configuration = new UserDataConnectorConfiguration();
                configuration.TryDeserializeConfigurationFromJson(connectionClassData);

                //Act
                UserDataConnectorConfigurationRealm configurationRealm = configuration.GetValidatedConfiguration();

                //Assert
                Assert.AreEqual("National Bank of Greece", configurationRealm.Bank);
                Assert.AreEqual(userIdentifier, configurationRealm.UserIdentifier);
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UserDataConnectorConfiguration_InputEmptyString_ShouldThrowArgumentException()
        {
            //Arrange
            string connectionClassData = String.Empty;

            //Act
            IUserDataConnectorConfiguration configuration = new UserDataConnectorConfiguration();
            configuration.TryDeserializeConfigurationFromJson(connectionClassData);

            //Assert
            Assert.Fail("No exception was thrown");
        }

        [TestMethod]
        [ExpectedException(typeof(JsonSerializationException))]
        public void UserDataConnectorConfiguration_InputInvalidJson_ShouldThrowJsonSerializationException()
        {
            //Arrange
            string connectionClassData = "\"abc\"";

            //Act
            IUserDataConnectorConfiguration configuration = new UserDataConnectorConfiguration();
            configuration.TryDeserializeConfigurationFromJson(connectionClassData);

            //Assert
            Assert.Fail("No exception was thrown");
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void UserDataConnectorConfiguration_InputInvalidProductsToFetch_ShouldThrowValidationException()
        {
            //Arrange
            string connectionClassData =
                "{\"productsToFetch\": [\"ThisIsInvalid\", \"AccountHolders\", \"DebitCards\", \"CreditCards\",\"Deposits\",\"Loans\"], \"invertAmount\": \"2\", \"bank\": \"Banc Sabadell\", \"userIdentifier\": \"UUUuser\", \"textReplacePatterns\": [{\"pattern\": \"Pago (.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"CAJERO (.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"ABONO A COMPRADOR POR DEVOLUCION - (.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"RECIBO (.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"DEVOLUCION\\\\s*(TAR.)*\\\\s*\\\\d{4}X+\\\\d{4} \\\\d+.\\\\d+ (.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"COMISIONES[\\\\d\\\\s]+(.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"^\\\\d{2,}[-\\\\s]*(.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"^COMPRA TARJ. (.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"^[Xx\\\\d-]{4,}(?=[\\\\D]+)-*\\\\s*(.*)\", \"isMerchant\": true, \"replace\": \"$1\"},{\"pattern\": \"TRANSFERENCIA\\\\s*(A\\\\s+|a\\\\s+|DE\\\\s+|de\\\\s+|){0,1}\\\\d\\\\d\\\\w\\\\d{12}\\\\w\\\\d\\\\d(.*)\", \"isMerchant\": true, \"replace\": \"$2\"},{\"pattern\": \"TRANSFERENCIA\\\\s*(A\\\\s+|a\\\\s+|DE\\\\s+|de\\\\s+|(\\\\d{2,}[\\\\w\\\\d\\\\S]*)){0,1}\\\\s*(.*)\", \"isMerchant\": true, \"replace\": \"$3\"}]}";
            IUserDataConnectorConfiguration configuration = new UserDataConnectorConfiguration();
            configuration.TryDeserializeConfigurationFromJson(connectionClassData);

            //Act
            configuration.GetValidatedConfiguration();

            //Assert
            Assert.Fail("No exception was thrown");
        }
    }
}
