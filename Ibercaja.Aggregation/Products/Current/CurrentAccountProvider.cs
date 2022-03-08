using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Meniga.Core.BusinessModels;
using Ibercaja.Aggregation.Eurobits;
using Account = Ibercaja.Aggregation.Eurobits.Account;

namespace Ibercaja.Aggregation.Products.Current
{
    public class CurrentAccountProvider : IAccountsProvider
    {
        private const string AccountInformationParameterName = "AccountInformation";
        private const string AccountDebitCardParameterName = "DebitCard";
        private const string Relationship = "Relationship";
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CurrentAccountProvider));
        private readonly IAggregationService _aggregationService;
        private readonly string _userDocument;

        public CurrentAccountProvider(IAggregationService aggregationService, string userDocument)
        {
            _aggregationService = aggregationService;
            _userDocument = userDocument;
        }

        public IEnumerable<BankAccountInfo> GetBankAccountInfos()
        {
            var accounts = _aggregationService.GetAccounts();
            foreach (var account in accounts)
            {
                decimal amount;
                if (decimal.TryParse(account.Balance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount))
                {
                    var current = new BankAccountInfo
                    {
                        AccountCategory = AccountCategoryEnum.Current,
                        AccountCategoryDetails = IbercajaProducts.CurrentAccount,
                        AccountIdentifier = account.AccountNumber,
                        Balance = amount,
                        CurrencyCode = account.Balance.Currency,
                        Limit = 0,
                        Name = account.WebAlias,
                        AccountParameters = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>(
                                AccountInformationParameterName,
                                FormatAccountInformation(account.Bank, account.Branch, account.ControlDigits,
                                    account.AccountNumber))
                        }
                    };

                    current.AccountParameters = current.AccountParameters
                        .Union(GetRelationship(account, _userDocument))
                        .Union(ExtractDebitCards(account))
                        .ToList();

                    yield return current;
                }
                else
                {
                    Logger.Error($"Failed to parse Account info: {account.AccountNumber}");
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
        private static string FormatAccountInformation(string bank, string branch, string controlDigits,
            string accountNumber)
        {
            return $"{bank}-{branch}-{controlDigits}-{accountNumber.PadLeft(10, '0')}";
        }

        private IEnumerable<KeyValuePair<string, string>> GetRelationship(Account account, string userDocument)
        {
            var holders = _aggregationService.GetAccountHolders()
                .SingleOrDefault(h =>
                    h.Bank == account.Bank && h.Branch == account.Branch && h.AccountNumber == account.AccountNumber &&
                    h.ControlDigits == account.ControlDigits)
                ?.Holders
                ?? Enumerable.Empty<Holder>().ToArray();

            var relationshipFound = false;
            var documentNumber = "";

            foreach (var holder in holders.Select((data, index) => new { data, index }))
            {
                if (!string.IsNullOrEmpty(holder.data.Document))
                {
                    documentNumber = holder.data.Document.Substring(0, holder.data.Document.Length - 1);
                    if (userDocument.Contains(documentNumber) && !string.IsNullOrEmpty(documentNumber) && !string.IsNullOrEmpty(holder.data.Relation))
                    {
                        relationshipFound = true;
                        yield return new KeyValuePair<string, string>($"{Relationship}{holder.index}", $"{holder.data.Relation}");
                    }
                }
            }

            if (!relationshipFound)
            {
                yield return new KeyValuePair<string, string>($"{Relationship}0", "Unknown");
            }
        }

        // Create a list for the cards information: 
        private IEnumerable<KeyValuePair<string, string>> ExtractDebitCards(Account account)
        {
            var cards = _aggregationService.GetDebitCards()
                .Where(c =>
                    c.AssociatedAccount.Contains(FormatAccountWithBlanks(account))
                    || c.AssociatedAccount == FormatAccountWithHyphens(account));

            foreach (var item in cards.Select((card, index) => new {card, index}))
            {
                yield return new KeyValuePair<string, string>(
                    $"{AccountDebitCardParameterName}{item.index}",
                    $"{item.card.WebAlias} - {item.card.CardNumber}");
            }
        }

        private static string FormatAccountWithHyphens(Account account)
        {
            return $"{account.Bank}-{account.Branch}-{account.ControlDigits}-{account.AccountNumber}";
        }

        private static string FormatAccountWithBlanks(Account account)
        {
            return $"{account.Bank} {account.Branch} {account.ControlDigits}{account.AccountNumber.Substring(0, 2)} {account.AccountNumber.Substring(2, 4)} {account.AccountNumber.Substring(6, 4)}";
        }
    }
}