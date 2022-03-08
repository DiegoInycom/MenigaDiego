using Ibercaja.Aggregation.TransactionDataFormatParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using Meniga.Core.Transactions;
using System.Linq;

namespace Ibercaja.UnitTests
{
    [TestClass]
    public class AggregationDataFormatParserTests
    {
        #region General_Test_AllFieldsInProducts
        // Accounts
        private readonly List<string> AccountDataFieldsIbercajaList = new List<string> { "description", "payee", "payer", "reference" };
        private readonly Dictionary<string, string> AccountDataFieldNamesDictionary = new Dictionary<string, string> { { "Description", "description" }, { "Payee", "payee" }, { "Payer", "payer" }, { "Reference", "reference" } };
        private readonly List<string> AccountData7FieldsSabadellList = new List<string> { "description", "payee", "payer", "reference", "value_date", "operation_date" };
        private readonly Dictionary<string, string> AccountData7FieldNamesDictionary = new Dictionary<string, string> { { "Description", "description" }, { "Payee", "payee" }, { "Payer", "payer" }, { "Reference", "reference" }, { "ValueDate", "value_date" }, { "OperationDate", "operation_date" } };

        // CreditCards 
        private readonly List<string> CreditCardDataFieldsIbercajaList = new List<string> { "description", "comments", "transaction_type" };
        private readonly Dictionary<string, string> CreditCardDataFieldNamesDictionary = new Dictionary<string, string> { { "Description", "description" }, { "Comments", "comments" }, { "TransactionType", "transaction_type" } };

        // Credits
        private readonly List<string> CreditDataFieldsIbercajaList = new List<string> { "description", "1", "payee", "payer", "reference" };
        private readonly Dictionary<string, string> CreditDataFieldNamesDictionary = new Dictionary<string, string> { { "Description", "description" }, { "Payee", "payee" }, { "Payer", "payer" }, { "Reference", "reference" } };

        // Funds
        private readonly List<string> FundDataFieldsIbercajaList = new List<string> { "description" };
        private readonly Dictionary<string, string> FundDataFieldNamesDictionary = new Dictionary<string, string> { { "Description", "description" } };

        // PensionPlans
        private readonly List<string> PensionPlanDataFieldsIbercajaList = new List<string> { "description" };
        private readonly Dictionary<string, string> PensionPlanDataFieldNamesDictionary = new Dictionary<string, string> { { "Description", "description" } };

        // Shares
        private readonly List<string> ShareDataFieldsIbercajaList = new List<string> { "description", "name", "market", "operation_type", "quantity_unit_price" };
        private readonly Dictionary<string, string> ShareDataFieldNamesDictionary = new Dictionary<string, string> { { "Description", "description" }, { "Name", "name" }, { "Market", "market" }, { "Operation_Type", "operation_type" }, { "Quantity_Unit_Price", "quantity_unit_price" } };

        [TestMethod]
        public void DataPartsWithNoEmptyFieldsCreatedDictionaryWithAllFields()
        {
            var values = new[]
            {
                new { dataFormatParser = new TransactionDataFormatParserCurrentAccount() as ITransactionDataFormatParser, input = AccountDataFieldsIbercajaList, output = AccountDataFieldNamesDictionary },
                new { dataFormatParser = new TransactionDataFormatParserCurrentAccount() as ITransactionDataFormatParser, input = AccountData7FieldsSabadellList, output = AccountData7FieldNamesDictionary },
                new { dataFormatParser = new TransactionDataFormatParserCreditCard() as ITransactionDataFormatParser, input = CreditCardDataFieldsIbercajaList, output = CreditCardDataFieldNamesDictionary },
                new { dataFormatParser = new TransactionDataFormatParserCreditCard() as ITransactionDataFormatParser, input = CreditDataFieldsIbercajaList, output = CreditDataFieldNamesDictionary },
                new { dataFormatParser = new TransactionDataFormatParserFund() as ITransactionDataFormatParser, input = FundDataFieldsIbercajaList, output = FundDataFieldNamesDictionary },
                new { dataFormatParser = new TransactionDataFormatParserPensionPlan() as ITransactionDataFormatParser, input = PensionPlanDataFieldsIbercajaList, output = PensionPlanDataFieldNamesDictionary },
                new { dataFormatParser = new TransactionDataFormatParserShare() as ITransactionDataFormatParser, input = ShareDataFieldsIbercajaList, output = ShareDataFieldNamesDictionary }
            };

            values.ToList().ForEach(val =>
            {
                var result = val.dataFormatParser.ParseData(JsonConvert.SerializeObject(val.input));

                Assert.AreEqual(val.output.Count, result.Count);
                foreach (var item in val.output)
                {
                    Assert.AreEqual(item.Value, result[item.Key]);
                }
            });
        }

        #endregion General_Test_AllFieldsInProducts

        #region General_Test_SomeEmptyFieldsInProducts
        // Accounts
        private readonly List<string> AccountData1EmptyFieldsIbercajaList = new List<string> { "", "payee", "payer", "reference", "value_date", "operation_date" };
        private readonly Dictionary<string, string> AccountData1EmptyFieldsNamesDictionary = new Dictionary<string, string> { { "Payee", "payee" }, { "Payer", "payer" }, { "Reference", "reference" }, { "ValueDate", "value_date" }, { "OperationDate", "operation_date" } };

        // CreditCards
        private readonly List<string> CreditCardData1EmptyFieldsIbercajaList = new List<string> { "", "comments", "transaction_type" };
        private readonly Dictionary<string, string> CreditCardData1EmptyFieldNamesDictionary = new Dictionary<string, string> { { "Comments", "comments" }, { "TransactionType", "transaction_type" } };

        // Credits
        private readonly List<string> CreditData1EmptyFieldIbercajaList = new List<string> { "", "1", "payee", "payer", "reference" };
        private readonly Dictionary<string, string> CreditData1EmptyFieldNamesDictionary = new Dictionary<string, string> { { "Payee", "payee" }, { "Payer", "payer" }, { "Reference", "reference" } };

        // Funds
        private readonly List<string> FundData1EmptyFieldIbercajaList = new List<string> { "" };
        private readonly Dictionary<string, string> FundData1EmptyFieldNamesDictionary = new Dictionary<string, string> { };

        // PensionPlans
        private readonly List<string> PensionPlanData1EmptyFieldIbercajalList = new List<string> { "" };
        private readonly Dictionary<string, string> PensionPlanData1EmptyFieldNamesDictionary = new Dictionary<string, string> { };

        // Shares
        private readonly List<string> ShareData1EmptyFieldIbercajaList = new List<string> { "", "name", "market", "operation_type", "quantity_unit_price" };
        private readonly Dictionary<string, string> ShareData1EmptyFieldNamesDictionary = new Dictionary<string, string> { { "Name", "name" }, { "Market", "market" }, { "Operation_Type", "operation_type" }, { "Quantity_Unit_Price", "quantity_unit_price" } };

        [TestMethod]
        public void DataPartsWithSomeEmptyFieldsCreatedDictionaryWithAllFields()
        {
            var values = new[]
            {
                new { dataFormatParser = new TransactionDataFormatParserCurrentAccount() as ITransactionDataFormatParser, input = AccountData1EmptyFieldsIbercajaList, output = AccountData1EmptyFieldsNamesDictionary },
                new { dataFormatParser = new TransactionDataFormatParserCreditCard() as ITransactionDataFormatParser, input = CreditCardData1EmptyFieldsIbercajaList, output = CreditCardData1EmptyFieldNamesDictionary },
                new { dataFormatParser = new TransactionDataFormatParserCreditCard() as ITransactionDataFormatParser, input = CreditData1EmptyFieldIbercajaList, output = CreditData1EmptyFieldNamesDictionary },
                new { dataFormatParser = new TransactionDataFormatParserFund() as ITransactionDataFormatParser, input = FundData1EmptyFieldIbercajaList, output = FundData1EmptyFieldNamesDictionary },
                new { dataFormatParser = new TransactionDataFormatParserPensionPlan() as ITransactionDataFormatParser, input = PensionPlanData1EmptyFieldIbercajalList, output = PensionPlanData1EmptyFieldNamesDictionary },
                new { dataFormatParser = new TransactionDataFormatParserShare() as ITransactionDataFormatParser, input = ShareData1EmptyFieldIbercajaList, output = ShareData1EmptyFieldNamesDictionary }
            };

            values.ToList().ForEach(val =>
            {
                var result = val.dataFormatParser.ParseData(JsonConvert.SerializeObject(val.input));

                Assert.AreNotEqual(val.input.Count, result.Count);
                foreach (var item in val.output)
                {
                    Assert.AreEqual(item.Value, result[item.Key]);
                }
                Assert.IsFalse(result.Keys.Contains("Description"));
            });
        }
        #endregion General_Test_SomeEmptyFieldsInProducts
    }
}