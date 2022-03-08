using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Meniga.Core.BusinessModels;
using Ibercaja.Aggregation.Eurobits;

namespace Ibercaja.Aggregation.Products.Mortgages
{
    public class MortgageAccountProvider : IAccountsProvider
    {
        private const string AccountInformationParameterName = "AccountInformation";
        private const string EurobitsMortgageAccountType = "H";
        private const string Relationship = "Relationship0";
        private readonly string _userDocument;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(MortgageAccountProvider));

        private readonly IAggregationService _aggregationService;

        public MortgageAccountProvider(IAggregationService aggregationService, string userDocument)
        {
            _aggregationService = aggregationService;
            _userDocument = userDocument;
        }

        public IEnumerable<BankAccountInfo> GetBankAccountInfos()
        {
            var loans = _aggregationService.GetLoans();

            foreach (var loan in loans.Where(x => x.AccountType == EurobitsMortgageAccountType))
            {
                decimal amount;
                decimal limit;
                if (decimal.TryParse(loan.Debt.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount)
                    && decimal.TryParse(loan.InitialBalance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out limit))
                {
                    var b = new BankAccountInfo
                    {
                        AccountCategory = AccountCategoryEnum.Loan,
                        AccountCategoryDetails = IbercajaProducts.Mortgage,
                        AccountIdentifier = loan.AccountNumber,
                        Balance = amount,
                        CurrencyCode = loan.Debt.Currency,
                        Limit = limit,
                        Name = loan.WebAlias,
                        AccountParameters = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>(
                                AccountInformationParameterName,
                                FormatAccountInformation(string.Empty, loan.Branch, loan.ControlDigits, loan.AccountNumber)),
                            new KeyValuePair<string, string>(
                                Relationship, 
                                ExtractRelation(_userDocument))
                        }
                    };
                    yield return b;
                }
                else
                {
                    Logger.Error($"Failed to parse Account info: {loan.AccountNumber}");
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