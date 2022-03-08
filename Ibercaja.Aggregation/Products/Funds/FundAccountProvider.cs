using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Meniga.Core.BusinessModels;
using Ibercaja.Aggregation.Eurobits;

namespace Ibercaja.Aggregation.Products.Funds
{
    public class FundAccountProvider : IAccountsProvider
    {
        private const string AccountInformation = "AccountInformation";
        private const string FundAccountFlag = "FundAccount";
        private const string FundName = "FundName";
        private const string FundNumber = "FundNumber";
        private const string FundPerformance = "FundPerformance";
        private const string FundPerformanceDescription = "FundPerformanceDescription";
        private const string FundQuantity = "FundQuatity";
        private const string FundValueDate = "FundValueDate";
        private const string FundYield = "FundYield";
        private const string FundISIN = "FundISIN";
        private const string FundCategory = "FundCategory";
        private const string FundUnitPrice = "FundUnitPrice";
        private readonly IAggregationService _aggregationService;
        private const string Relationship = "Relationship0";
        private readonly string _userDocument;

        public FundAccountProvider(IAggregationService aggregationService, string userDocument)
        {
            _aggregationService = aggregationService;
            _userDocument = userDocument;
        }

        public IEnumerable<BankAccountInfo> GetBankAccountInfos()
        {
            var funds = _aggregationService.GetFunds();

            foreach (var fundAccount in funds)
            {
                decimal amount;
                if (decimal.TryParse(fundAccount.Balance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount))
                {
                    var fund = new BankAccountInfo
                    {
                        AccountCategory = AccountCategoryEnum.Asset,
                        AccountCategoryDetails = IbercajaProducts.Fund,
                        AccountIdentifier = fundAccount.AccountNumber,
                        Balance = amount,
                        CurrencyCode = fundAccount.Balance.Currency,
                        Limit = 0,
                        Name = fundAccount.WebAlias,
                        AccountParameters = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>(
                                FundAccountFlag,
                                "true"),
                            new KeyValuePair<string, string>(
                                AccountInformation,
                                FormatAccountInformation(string.Empty, fundAccount.Branch, fundAccount.ControlDigits,
                                    fundAccount.AccountNumber)),
                            new KeyValuePair<string, string>(
                                FundYield,
                                $"{fundAccount.Yield.Value}{fundAccount.Yield.Currency}"),
                            new KeyValuePair<string, string>(
                                FundName,
                                $"{fundAccount.FundName}"),
                            new KeyValuePair<string, string>(
                                FundNumber,
                                $"{fundAccount.Number}"),
                            new KeyValuePair<string, string>(
                                FundPerformance,
                                $"{fundAccount.Performance}"),
                            new KeyValuePair<string, string>(
                                FundPerformanceDescription,
                                $"{fundAccount.PerformanceDescription}"),
                            new KeyValuePair<string, string>(
                                FundQuantity,
                                $"{fundAccount.Quantity}"),
                            new KeyValuePair<string, string>(
                                FundValueDate,
                                $"{fundAccount.ValueDate}"),
                            new KeyValuePair<string, string>(
                                Relationship, 
                                ExtractRelation(_userDocument))

                        }
                    };
                    fund.AccountParameters = fund.AccountParameters
                        .Union(ExtractFundExtendedInfo(fundAccount))
                        .ToList();

                    yield return fund;
                }
            }
        }

        // Extract FundExtendedInfo product related 
        private IEnumerable<KeyValuePair<string, string>> ExtractFundExtendedInfo(Fund fund)
        {
            var info = _aggregationService.GetFundsExtendedInfo().FirstOrDefault(
                fei => fei.AccountNumber.Equals(fund.AccountNumber));

            if (info == null) yield break;

            yield return new KeyValuePair<string, string>(
                FundISIN,
                info.ISIN);
            yield return new KeyValuePair<string, string>(
                FundCategory,
                info.Category);
            yield return new KeyValuePair<string, string>(
                FundUnitPrice,
                $"{info.UnitPrice.Value}{info.UnitPrice.Currency} at {info.UnitPrice.ValueDate}");
        }

        /// <summary>
        ///     Returns the spanish account information on the form
        ///     xxxx-yyyy-zz-oooooooooo
        ///     xxx = Bank
        ///     yyyy = Branch
        ///     zz = Control digits
        ///     oooooooooo = Zero padded account number
        /// </summary>
        /// <param name="bank">Bank of the account</param>
        /// <param name="branch">Branch of the account</param>
        /// <param name="controlDigits">Control digits of the account</param>
        /// <param name="accountNumber">Account number</param>
        /// <returns></returns>
        private static string FormatAccountInformation(string bank, string branch, string controlDigits, string accountNumber)
        {
            return $"{bank}-{branch}-{controlDigits}-{accountNumber.PadLeft(10, '0')}";
        }

        private string ExtractRelation(string userDocument)
        {
            var document = _aggregationService.GetPersonalInfo()?.Document;

            if (string.IsNullOrWhiteSpace(document) || string.IsNullOrWhiteSpace(userDocument))
            {
                return "Unknown";
            }
            else
            {
                return userDocument.Contains(document) ? "Titular" : "Unknown";
            }
        }
    }
}
