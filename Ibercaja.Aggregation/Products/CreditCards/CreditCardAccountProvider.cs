using System.Collections.Generic;
using System.Globalization;
using log4net;
using Meniga.Core.BusinessModels;
using Ibercaja.Aggregation.Eurobits;

namespace Ibercaja.Aggregation.Products.CreditCards
{
    public class CreditCardAccountProvider : IAccountsProvider
    {
        private const string Relationship = "Relationship0";
        private const string CreditCardTypeParameterName = "CreditCardType";
        private const string CreditCardWebAliasParameterName = "CreditCardWebAlias";
        private const string CreditCardNumberParameterName = "CreditCardNumber";
        private const string CreditCardExpirationDateParameterName = "CreditCardExpirationDate";
        private const string CreditAccountFlagParameterName = "CreditAccount";
        private const string CreditInformationParameterName = "CreditInformation";
        private const string CreditAvailableBalance = "CreditAvailableAmount";
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CreditCardAccountProvider));
        private readonly IAggregationService _aggregationService;
        private readonly string _userDocument;

        public CreditCardAccountProvider(IAggregationService aggregationService, string userDocument)
        {
            _aggregationService = aggregationService;
            _userDocument = userDocument;
        }

        public IEnumerable<BankAccountInfo> GetBankAccountInfos()
        {
            var creditCards = _aggregationService.GetCreditCards();

            foreach (var c in creditCards)
            {
                decimal balanceAmount;
                if (!decimal.TryParse(c.Disposed.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out balanceAmount))
                {
                    Logger.Warn($"Failed to parse CreditCard balance. The disposed value is empty");
                }

                decimal limitAmount;
                if (!decimal.TryParse(c.Limit.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out limitAmount))
                {
                    Logger.Warn($"Failed to parse CreditCard balance. The limit value is empty");
                }

                var b = new BankAccountInfo
                {
                    AccountCategory = AccountCategoryEnum.Credit,
                    AccountCategoryDetails = IbercajaProducts.CreditCard,
                    AccountIdentifier = c.CardNumber,
                    Balance = balanceAmount,
                    CurrencyCode = c.Disposed.Currency,
                    Limit = limitAmount,
                    Name = c.WebAlias,
                    AccountParameters = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>(
                            CreditCardTypeParameterName,
                            c.CardType),
                        new KeyValuePair<string, string>(
                            CreditCardWebAliasParameterName,
                            c.WebAlias),
                        new KeyValuePair<string, string>(
                            CreditCardNumberParameterName,
                            c.CardNumber),
                        new KeyValuePair<string, string>(
                            CreditCardExpirationDateParameterName,
                            c.ExpirationDate),
                        new KeyValuePair<string, string>(
                            Relationship, 
                            ExtractRelation(_userDocument))
                    }
                };

                yield return b;
            }

            var credits = _aggregationService.GetCredits();

            foreach (var c in credits)
            {
                decimal amount;
                if (decimal.TryParse(c.Balance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount))
                {
                    var b = new BankAccountInfo
                    {
                        AccountCategory = AccountCategoryEnum.Credit,
                        AccountCategoryDetails = IbercajaProducts.Credit,
                        AccountIdentifier = c.AccountNumber,
                        Balance = amount,
                        CurrencyCode = c.Balance.Currency,
                        Limit = 0,
                        Name = c.WebAlias,
                        AccountParameters = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>(
                                CreditAccountFlagParameterName,
                                "true"),
                            new KeyValuePair<string, string>(
                                CreditInformationParameterName,
                                FormatAccountInformation(c.Bank, c.Branch, c.ControlDigits, c.AccountNumber)),
                            new KeyValuePair<string, string>(
                                CreditAvailableBalance,
                                $"{c.AvailableBalance.Value}{c.AvailableBalance.Currency}"),
                            new KeyValuePair<string, string>(
                                Relationship,
                                ExtractRelation(_userDocument))
                        }
                    };
                    yield return b;
                }
            }
        }

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