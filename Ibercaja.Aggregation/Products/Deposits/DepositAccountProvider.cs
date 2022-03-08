using System.Collections.Generic;
using System.Globalization;
using log4net;
using Meniga.Core.BusinessModels;
using Ibercaja.Aggregation.Eurobits;

namespace Ibercaja.Aggregation.Products.Deposits
{
    public class DepositAccountProvider : IAccountsProvider
    {
        private const string DepositAccountFlagParameterName = "DepositAccount";
        private const string DepositExpirationDateParameterName = "DepositExpirationDate";
        private const string DepositInterestRateParameterName = "DepositInterestRate";
        private const string AccountInformationParameterName = "AccountInformation";
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DepositAccountProvider));
        private readonly IAggregationService _aggregationService;
        private const string Relationship = "Relationship0";
        private readonly string _userDocument;

        public DepositAccountProvider(IAggregationService aggregationService, string userDocument)
        {
            _aggregationService = aggregationService;
            _userDocument = userDocument;
        }

        public IEnumerable<BankAccountInfo> GetBankAccountInfos()
        {
            var deposits = _aggregationService.GetDeposits();

            foreach (var depositAccount in deposits)
            {
                decimal amount;
                if (decimal.TryParse(depositAccount.Balance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount))
                {
                    var deposit = new BankAccountInfo
                    {
                        AccountCategory = AccountCategoryEnum.Savings,
                        AccountCategoryDetails = IbercajaProducts.Deposit,
                        AccountIdentifier = depositAccount.AccountNumber,
                        Balance = amount,
                        CurrencyCode = depositAccount.Balance.Currency,
                        Limit = 0,
                        Name = depositAccount.WebAlias,
                        AccountParameters = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>(
                                DepositAccountFlagParameterName,
                                "true"),
                            new KeyValuePair<string, string>(
                                AccountInformationParameterName,
                                FormatAccountInformation(depositAccount.Bank, depositAccount.Branch,
                                    depositAccount.ControlDigits, depositAccount.AccountNumber)),
                            new KeyValuePair<string, string>(
                                DepositExpirationDateParameterName,
                                depositAccount.Duration.EndDate),
                            new KeyValuePair<string, string>(
                                DepositInterestRateParameterName,
                                $"{depositAccount.Interest.Rate}% {depositAccount.Interest.Type}"),
                            new KeyValuePair<string, string>(
                                Relationship, ExtractRelation(_userDocument))

                        }
                    };
                    yield return deposit;
                }
                else
                {
                    Logger.Error($"Failed to parse deposit account info: {depositAccount.AccountNumber}");
                }
            }
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