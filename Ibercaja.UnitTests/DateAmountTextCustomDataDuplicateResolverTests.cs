using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ibercaja.Aggregation.DuplicateResolver;
using Meniga.Core.BusinessModels;
using Meniga.Core.Data.User;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Practices.Unity;
using Meniga.Runtime.Cache;
using Meniga.Runtime.IOC;
using Meniga.Runtime.Configuration;
using Ibercaja.UnitTests.Helpers;

namespace Ibercaja.UnitTests
{
    [TestClass]
    public class DateAmountTextCustomDataDuplicateResolverTest
    {
        [TestInitialize]
        public void CreateAndInitializeContainer()
        {
            var container = new UnityContainer();
            container.RegisterType<IGlobalApplicationParameterCache, InMemoryGlobalApplicationParameterCache>();
            IoC.Initialize(new UnityDependencyResolver(container));
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_InputAllFieldsEqual_ShouldBeUpdated()
        {
            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
                new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, TransactionText = "Apple Pay: COMPRA EN LA EXQUISITA, CON LA TARJETA : 54XXXXXXXXXX4944 EL 2019 - 01 - 12", CustomData = "[\"Apple Pay: COMPRA EN LA EXQUISITA, CON LA TARJETA : 54XXXXXXXXXX4944 EL 2019 - 01 - 12\",\"\",\"\"]", IsRead = false, IsUncleared = false, IsMerchant = true, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { Date = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, Text = "LA EXQUISITA, CON LA TARJETA : 54XXXXXXXXXX4944 EL 2019 - 01 - 12", Data = "[\"Apple Pay: COMPRA EN LA EXQUISITA, CON LA TARJETA : 54XXXXXXXXXX4944 EL 2019 - 01 - 12\",\"\",\"\"]", IsUncleared = false, IsMerchant = true }
            };
            var dupRes = new DateAmountTextCustomDataDuplicateResolver(false);

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            Assert.AreEqual(0, result.TransactionsToAdd.Count());
            Assert.AreEqual(1, result.TransactionsToUpdate.Count());
            Assert.AreEqual(0, result.TransactionsToDelete.Count());
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_InputAllFieldsEqualWithMultipleExisting_ShouldBeUpdatedOnlyOne()
        {
            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
                new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, TransactionText = "Apple Pay: COMPRA EN LA EXQUISITA, CON LA TARJETA : 54XXXXXXXXXX4944 EL 2019 - 01 - 12", CustomData = "[\"Apple Pay: COMPRA EN LA EXQUISITA, CON LA TARJETA : 54XXXXXXXXXX4944 EL 2019 - 01 - 12\",\"\",\"\"]", IsRead = false, IsUncleared = false, IsMerchant = true, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") },
                new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, TransactionText = "Apple Pay: COMPRA EN LA EXQUISITA, CON LA TARJETA : 54XXXXXXXXXX4944 EL 2019 - 01 - 12", CustomData = "[\"Apple Pay: COMPRA EN LA EXQUISITA, CON LA TARJETA : 54XXXXXXXXXX4944 EL 2019 - 01 - 12\",\"\",\"\"]", IsRead = false, IsUncleared = false, IsMerchant = true, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { Date = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, Text = "LA EXQUISITA, CON LA TARJETA : 54XXXXXXXXXX4944 EL 2019 - 01 - 12", Data = "[\"Apple Pay: COMPRA EN LA EXQUISITA, CON LA TARJETA : 54XXXXXXXXXX4944 EL 2019 - 01 - 12\",\"\",\"\"]", IsUncleared = false, IsMerchant = true }
            };
            var dupRes = new Ibercaja.Aggregation.DuplicateResolver.DateAmountTextCustomDataDuplicateResolver(false);

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            Assert.AreEqual(0, result.TransactionsToAdd.Count());
            Assert.AreEqual(1, result.TransactionsToUpdate.Count());
            Assert.AreEqual(0, result.TransactionsToDelete.Count());
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_InputTransactionDateEqual_ShouldBeUpdated()
        {
            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
                new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2019, 01, 05), Timestamp = new DateTime(2019, 01, 05), Amount = -20, TransactionText = "Apple Pay: COMPRA EN LA EXQUISITA, CON LA TARJETA : 54XXXXXXXXXX4944 EL 2019 - 01 - 12", CustomData = "[\"Apple Pay: COMPRA EN LA EXQUISITA, CON LA TARJETA : 54XXXXXXXXXX4944 EL 2019 - 01 - 12\",\"\",\"\",\"\",\"2019-01-01\",\"2019-01-05\"]", IsRead = false, IsUncleared = false, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { Date = new DateTime(2019, 01, 05), Timestamp = new DateTime(2019, 01, 05), Amount = -20, Text = "LA EXQUISITA, CON LA TARJETA : 54XXXXXXXXXX4944 EL 2019 - 01 - 12", Data = "[\"Apple Pay: COMPRA EN LA EXQUISITA, CON LA TARJETA : 54XXXXXXXXXX4944 EL 2019 - 01 - 12\",\"\",\"\",\"\",\"2019-01-01\",\"2019-01-05\"]", IsUncleared = false }
            };

            var dupRes = new DateAmountTextCustomDataDuplicateResolver(false);

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            // Verify that is updated
            Assert.AreEqual(0, result.TransactionsToAdd.Count());
            Assert.AreEqual(1, result.TransactionsToUpdate.Count());
            Assert.AreEqual(0, result.TransactionsToDelete.Count());
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_InputTransactionTextEqual_ShouldBeUpdated()
        {
            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
                new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2018, 4, 13), Timestamp = new DateTime(2019, 01, 31), Amount = -112, TransactionText = "Köp CAFE PUBLIK GOTEBORG", CustomData = "[\"Köp CAFE PUBLIK GOTEBORG\",\" BARS/TAVERNS/LOUNGES/DISCOS \",\"3\"]", IsRead = false, IsUncleared = false, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { Date = new DateTime(2018, 4, 13), Timestamp = new DateTime(2019, 01, 31), Amount = -112, Text = "Köp CAFE PUBLIK GOTEBORG", Data = "[\"Köp CAFE PUBLIK GOTEBORG\",\"Drinking Places (Alcoholic Beverages\",\"3\"]", IsUncleared = false }
            };
            var dupRes = new DateAmountTextCustomDataDuplicateResolver(false);

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            Assert.AreEqual(0, result.TransactionsToAdd.Count());
            Assert.AreEqual(1, result.TransactionsToUpdate.Count());
            Assert.AreEqual(0, result.TransactionsToDelete.Count());
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_InputIsUnclearedTrue_ShouldBeDeleted()
        {
            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
               new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, TransactionText = "Cine DEL", CustomData = "[\"Cine DEL\",\"\",\",\"5\"]", IsRead = false, IsUncleared = true, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { Date = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, Text = "Cine", Data = "[\"Cine DEL\",\"\",\"5\"]", IsUncleared = true }
            };
            var dupRes = new DateAmountTextCustomDataDuplicateResolver(false);

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            Assert.AreEqual(1, result.TransactionsToAdd.Count());
            Assert.AreEqual(0, result.TransactionsToUpdate.Count());
            Assert.AreEqual(1, result.TransactionsToDelete.Count());
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_InputIsMerchantTrue_ShouldBeUpdated()
        {
            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
               new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, TransactionText = "Cine DEL", CustomData = "[\"Cine\",\"\",\"\",\"5\"]", IsMerchant = true , IsRead = false, IsUncleared = false, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { Date = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, Text = "Cine", Data = "[\"Cine\",\"\",\"\",\"5\"]", IsUncleared = false, IsMerchant = true }
            };
            var dupRes = new DateAmountTextCustomDataDuplicateResolver(false);

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            Assert.AreEqual(0, result.TransactionsToAdd.Count());
            Assert.AreEqual(1, result.TransactionsToUpdate.Count());
            Assert.AreEqual(0, result.TransactionsToDelete.Count());
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_InputDistinctAmount_ShouldBeAdded()
        {
            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
               new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -200, TransactionText = "Cine DIFFERENT", CustomData = "[\"Cine\",\"\",\"5\"]", IsRead = false, IsUncleared = false, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { Date = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, Text = "Cine", Data = "[\"Cine\",\"\",\"5\"]", IsUncleared = false }
            };
            var dupRes = new DateAmountTextCustomDataDuplicateResolver(false);

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            Assert.AreEqual(1, result.TransactionsToAdd.Count());
            Assert.AreEqual(0, result.TransactionsToUpdate.Count());
            Assert.AreEqual(0, result.TransactionsToDelete.Count());
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_InputDistinctBalance_ShouldBeAdded()
        {
            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
               new Meniga.Core.Data.User.Transaction { AccountId = 110, Balance = 10, TransactionDate = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -200, TransactionText = "Cine DIFFERENT", CustomData = "[\"Cine\",\"\",\"\",\"5\"]", IsRead = false, IsUncleared = false, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { AccountBalance = 20, Date = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, Text = "Cine", Data = "[\"Cine\",\"\",\"\",\"5\"]", IsUncleared = false }
            };
            var dupRes = new DateAmountTextCustomDataDuplicateResolver(false);

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            Assert.AreEqual(1, result.TransactionsToAdd.Count());
            Assert.AreEqual(0, result.TransactionsToUpdate.Count());
            Assert.AreEqual(0, result.TransactionsToDelete.Count());
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_InputDistinctDate_ShouldBeAdded()
        {
            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
               new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2019, 01, 29), Timestamp = new DateTime(2019, 01, 29), Amount = -20, TransactionText = "Cine", CustomData = "[\"Cine\",\"\",\"\",\"5\"]", IsRead = false, IsUncleared = false, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { Date = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, Text = "Cine", Data = "[\"Cine\",\"\",\"\",\"5\"]", IsUncleared = false }
            };
            var dupRes = new DateAmountTextCustomDataDuplicateResolver(false);

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            Assert.AreEqual(1, result.TransactionsToAdd.Count());
            Assert.AreEqual(0, result.TransactionsToUpdate.Count());
            Assert.AreEqual(0, result.TransactionsToDelete.Count());
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_InputDistinctCustomDataText_ShouldBeAdded()
        {
            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
                new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2017, 12, 13), Timestamp = new DateTime(2019, 01, 31), Amount = -20, TransactionText = "Transaction Text", CustomData = "[\"Transaction CustomData\",\"\",\"\",\"0000000140\"]", IsRead = false, IsUncleared = false, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { AccountBalance = 10, Date = new DateTime(2017, 12, 13), Timestamp = new DateTime(2019, 01, 31), Amount = -20, Text = "Transaction Text", Data = "[\"No trans consider\",\"\",\"\",\"00000003808623050\"]", IsUncleared = false }
            };
            var dupRes = new DateAmountTextCustomDataDuplicateResolver(false);

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            Assert.AreEqual(1, result.TransactionsToAdd.Count());
            Assert.AreEqual(0, result.TransactionsToUpdate.Count());
            Assert.AreEqual(0, result.TransactionsToDelete.Count());
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_InputCustomDataWithDatesExistingWithoutDates_ShouldBeUpdated()
        {
            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
                new Meniga.Core.Data.User.Transaction { AccountId = 110, Balance=10, TransactionDate = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, TransactionText = "Transaction Text", CustomData = "[\"Transaction Text\",\"\",\"\",\"00000003808623050\"]", IsRead = false, IsUncleared = false, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { AccountBalance = 10, Date = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, Text = "Transaction Text", Data = "[\"Transaction Text\",\"\",\"\",\"00000003808623050\",\"31/01/2019\",,\"31/01/2019\"]", IsUncleared = false }
            };
            var dupRes = new DateAmountTextCustomDataDuplicateResolver(false);

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            Assert.AreEqual(0, result.TransactionsToAdd.Count());
            Assert.AreEqual(1, result.TransactionsToUpdate.Count());
            Assert.AreEqual(0, result.TransactionsToDelete.Count());
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_InputSameOriginalTextWithoutDates_ShouldBeUpdated()
        {
            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
                new Meniga.Core.Data.User.Transaction { AccountId = 110, Balance = 10, Amount = -20, Timestamp = new DateTime(2019, 01, 31), TransactionDate = new DateTime(2019, 01, 31), TransactionText = "Transaction Text", CustomData = "[\"Transaction Text\",\"\",\"\",\"00000003808623050\"]", IsRead = false, IsUncleared = false, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { AccountBalance = 0, Date = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, Text = "Transaction Text", Data = "[\"Transaction Text\",\"\",\"\",\"00000003808623050\"]", IsUncleared = false }
            };
            var dupRes = new DateAmountTextCustomDataDuplicateResolver(false);

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            Assert.AreEqual(1, result.TransactionsToAdd.Count());
            Assert.AreEqual(0, result.TransactionsToUpdate.Count());
            Assert.AreEqual(0, result.TransactionsToDelete.Count());
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_InputWithBalanceExistingWithoutBalance_ShouldBeUpdated()
        {
            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
                new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, TransactionText = "Transaction Text", CustomData = "[\"Transaction Text\",\"\",\"\",\"00000003808623050\"]", IsRead = false, IsUncleared = false, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { AccountBalance = 10, Date = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, Text = "Transaction Text", Data = "[\"Transaction Text\",\"\",\"\",\"00000003808623050\",\"31/01/2019\",,\"31/01/2019\"]", IsUncleared = false }
            };
            var dupRes = new DateAmountTextCustomDataDuplicateResolver(false);

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            Assert.AreEqual(0, result.TransactionsToAdd.Count());
            Assert.AreEqual(1, result.TransactionsToUpdate.Count());
            Assert.AreEqual(0, result.TransactionsToDelete.Count());
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_InputWithBalanceTwoExistingWithoutBalance_ShouldBeUpdatedOnlyOne()
        {
            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
                new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, TransactionText = "Transaction Text", CustomData = "[\"Transaction Text\",\"\",\"\",\"00000003808623050\"]", IsRead = false, IsUncleared = false, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") },
                new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, TransactionText = "Transaction Text", CustomData = "[\"Transaction Text\",\"\",\"\",\"00000003808623050\"]", IsRead = false, IsUncleared = false, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a01") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { AccountBalance = 10, Date = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, Text = "Transaction Text", Data = "[\"Transaction Text\",\"\",\"\",\"00000003808623050\",\"31/01/2019\",,\"31/01/2019\"]", IsUncleared = false }
            };
            var dupRes = new DateAmountTextCustomDataDuplicateResolver(false);

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            Assert.AreEqual(0, result.TransactionsToAdd.Count());
            Assert.AreEqual(1, result.TransactionsToUpdate.Count());
            Assert.AreEqual(0, result.TransactionsToDelete.Count());
        }

        [TestMethod]
        public void DateAmountTextCustomDataDuplicateResolver_CleanupMode_ShouldDeleteUnmatchedTransactions()
        {
            IoC.Resolve<IGlobalApplicationParameterCache>().GetOrCreateParameter("DuplicateResolverCleanupMode", () => true);
            IoC.Resolve<IGlobalApplicationParameterCache>().SetParameterValue("DuplicateResolverCleanupMode", true);

            var existingTransactions = new List<Meniga.Core.Data.User.Transaction>
            {
                new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2019, 01, 30), Timestamp = new DateTime(2019, 01, 30), Amount = -20, TransactionText = "Transaction Text", CustomData = "[\"Transaction Text\",\"\",\"\",\"00000003808623050\"]", IsRead = false, IsUncleared = false, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a00") },
                new Meniga.Core.Data.User.Transaction { AccountId = 110, TransactionDate = new DateTime(2019, 01, 30), Timestamp = new DateTime(2019, 01, 30), Amount = -30, TransactionText = "Transaction Text", CustomData = "[\"Transaction Text\",\"\",\"\",\"00000003808623050\"]", IsRead = false, IsUncleared = false, ParentIdentifier = new Guid("d99737dd-6a99-4487-bc1b-6f8d78ef8a01") }
            };
            var userDataContext = CreateUserDataContext(existingTransactions);
            var incomingTransactions = new List<BankTransaction>
            {
                new BankTransaction { AccountBalance = 10, Date = new DateTime(2019, 01, 31), Timestamp = new DateTime(2019, 01, 31), Amount = -20, Text = "Transaction Text", Data = "[\"Transaction Text\",\"\",\"\",\"00000003808623050\",\"31/01/2019\",,\"31/01/2019\"]", IsUncleared = false }
            };
            var dupRes = new DateAmountTextCustomDataDuplicateResolver(false);
            dupRes.ReloadCleanupModeFromSettings();

            var result = dupRes.ResolveDuplicates(userDataContext, incomingTransactions, 110, null, null);

            Assert.AreEqual(1, result.TransactionsToAdd.Count());
            Assert.AreEqual(0, result.TransactionsToUpdate.Count());
            Assert.AreEqual(2, result.TransactionsToDelete.Count());

            IoC.Resolve<IGlobalApplicationParameterCache>().SetParameterValue("DuplicateResolverCleanupMode", false);
        }

        #region Helpers
        private static ICoreUserContext CreateUserDataContext(List<Meniga.Core.Data.User.Transaction> transactions)
        {
            var userDataContext = new Mock<ICoreUserContext>();
            var transactionRepository = new Mock<DbSet<Meniga.Core.Data.User.Transaction>>();

            var data = transactions.AsQueryable();
            transactionRepository.As<IQueryable<Meniga.Core.Data.User.Transaction>>().Setup(m => m.Provider).Returns(data.Provider);
            transactionRepository.As<IQueryable<Meniga.Core.Data.User.Transaction>>().Setup(m => m.Expression).Returns(data.Expression);
            transactionRepository.As<IQueryable<Meniga.Core.Data.User.Transaction>>().Setup(m => m.ElementType).Returns(data.ElementType);
            transactionRepository.As<IQueryable<Meniga.Core.Data.User.Transaction>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            userDataContext.SetupGet(x => x.Transactions).Returns(transactionRepository.Object);
            return userDataContext.Object;
        }
        #endregion
    }
}
