using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Meniga.Core.BusinessModels;
using Newtonsoft.Json;
using Ibercaja.Aggregation.Eurobits;
using Ibercaja.Aggregation.UserDataConnector.Configuration;

namespace Ibercaja.Aggregation.Products.CreditCards
{
    public class CreditCardTransactionsProvider : ITransactionsProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CreditCardTransactionsProvider));

        /// <summary>
        ///     The credit card mapper hold all known credit card types and their regular expressions which identify their card
        ///     numbers.
        /// </summary>
        private static readonly IDictionary<string, string> CreditCardMapper = new Dictionary<string, string>
        {
            { "VISA", @"^4\d{5}[\d\*]{6}(?:\d{4})?$" },
            { "MASTERCARD", @"^5[1-5]\d{4}[\d\*]{6}\d{4}$" },
            { "AMEX", @"^3[47]\d{2}[\d\*]{6}\d{5}$" },
            { "DISCOVER", @"^6(?:011|5\d{2})[\d\*]{8}\d{4}$" },
            { "DINERS", @"^3(?:0[0-5]|[68]\d)[\d\*]{7}\d{4}$" },
            { "JCB", @"^(?:2131|1800|35\d{3})[\d\*]{7}\d{4}$" }
        };

        private readonly IAggregationService _aggregationService;
        private readonly UserDataConnectorConfigurationRealm _configurationRealm;
        private readonly IDictionary<string, string> _invertAmountConfiguration;

        public CreditCardTransactionsProvider(
            IAggregationService aggregationService,
            UserDataConnectorConfigurationRealm configurationRealm,
            IDictionary<string, string> invertAmountConfiguration)
        {
            _aggregationService = aggregationService;
            _configurationRealm = configurationRealm;
            _invertAmountConfiguration = invertAmountConfiguration;
        }

        public AccountStatement GetAccountStatement(string accountIdentifier)
        {
            var creditCards = _aggregationService.GetCreditCards();

            var accountStatement = GetCreditCardTransactions(creditCards, accountIdentifier);

            // If null transactions then no CreditCard recovered, need to check Credits
            if (accountStatement.Transactions == null)
            {
                var credits = _aggregationService.GetCredits();

                accountStatement = GetCreditTransactions(credits, accountIdentifier);
            }

            return accountStatement;
        }

        private AccountStatement GetCreditCardTransactions(IEnumerable<CreditCard> creditCards, string accountId)
        {
            var accountStatement = new AccountStatement();

            var creditCard = creditCards
                .FirstOrDefault(c => c.CardNumber.Substring(0, 4) == accountId.Substring(0, 4) &&
                                     c.CardNumber.Substring(c.CardNumber.Length - 4) == accountId.Substring(accountId.Length - 4));
            if (creditCard != null)
            {
                var invert = ShouldInvertAmount(_configurationRealm.InvertAmount);

                decimal balanceAmount;
                decimal.TryParse(creditCard.Disposed.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out balanceAmount);
                accountStatement.Balance = balanceAmount * invert;
                decimal limitAmount;
                decimal.TryParse(creditCard.Limit.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out limitAmount);
                accountStatement.Limit = limitAmount;

                accountStatement.Transactions = new List<BankTransaction>();
                var invertTransactions = ShouldInvertAmount(creditCard.CardNumber);
                foreach (var ct in creditCard.Transactions)
                {
                    try
                    {
                        var dataParts = new[] { ct.Description, ct.Comments, ct.TransactionType };
                        decimal amount;
                        decimal.TryParse(ct.Amount.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount);
                        var bt = new BankTransaction
                        {
                            Amount = amount * invertTransactions,
                            Currency = string.IsNullOrWhiteSpace(ct.Amount.Currency)
                                ? creditCard.Disposed.Currency
                                : ct.Amount.Currency,
                            Identifier = null,
                            Text = ct.Description,
                            Timestamp = ct.ValueDate.ToEurobitsDateTimeFormat(),
                            Date = ct.ValueDate.ToEurobitsDateTimeFormat(),
                            Data = JsonConvert.SerializeObject(dataParts)
                        };

                        UpdateTextAndIsMerchant(bt);
                        accountStatement.Transactions.Add(bt);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(
                            $"Reading transaction failed for bank: {_configurationRealm.Bank} and account: {accountId}", ex);
                    }
                }
            }

            return accountStatement;
        }

        /// <summary>
        ///     Determines whether the amount from Eurobits for a certain bank+credit card combo should be inverted or not.
        /// </summary>
        /// <param name="defaultBankInvert">Default invert option for this bank</param>
        /// <returns>-1 or 1 depending on whether or not the amount should be inverted or not</returns>
        private int ShouldInvertAmount(bool defaultBankInvert)
        {
            var invert = 1;
            if (defaultBankInvert) invert = -1;
            return invert;
        }

        /// <summary>
        ///     Determines whether the amount from Eurobits for a certain bank+credit card combo should be inverted or not.
        /// </summary>
        /// <param name="cardNumber">Credit card number</param>
        /// <returns>-1 or 1 depending on whether or not the amount should be inverted or not</returns>
        private int ShouldInvertAmount(string cardNumber)
        {
            var invert = 1;

            var cardType = GetCreditCardType(cardNumber.Replace(".", string.Empty).Replace("-", string.Empty)
                .Replace(" ", string.Empty));

            string configString;
            try
            {
                configString = _invertAmountConfiguration[
                    $"{_configurationRealm.Bank.ToUpper(CultureInfo.InvariantCulture)} {cardType.ToUpper(CultureInfo.InvariantCulture)}"];
            }
            catch (KeyNotFoundException)
            {
                configString = string.Empty;
            }

            bool bankCreditCardInvert;
            if (string.IsNullOrEmpty(configString) ||
                !bool.TryParse(configString.Trim('{', '}'), out bankCreditCardInvert))
            {
                bankCreditCardInvert = _configurationRealm.InvertAmount;
            }

            if (bankCreditCardInvert) invert = -1;

            return invert;
        }

        /// <summary>
        ///     Finds the credit card type based on the card number.
        ///     The information is taken from: http://www.regular-expressions.info/creditcard.html
        /// </summary>
        /// <param name="cardNumber">Credit card number to get the type by</param>
        /// <returns>The name of the card type.</returns>
        private string GetCreditCardType(string cardNumber)
        {
            foreach (var mapper in CreditCardMapper)
                if (Regex.IsMatch(cardNumber, mapper.Value))
                    return mapper.Key;

            Logger.Warn($"The credit card type wasn't found for card number {cardNumber}.");
            return string.Empty;
        }

        private AccountStatement GetCreditTransactions(IEnumerable<Credit> credits, string accountId)
        {
            var accountStatement = new AccountStatement();

            var credit = credits.FirstOrDefault(c => c.AccountNumber == accountId);
            if (credit != null)
            {
                decimal balanceAmount;
                decimal.TryParse(credit.Balance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out balanceAmount);
                accountStatement.Balance = balanceAmount;

                accountStatement.Transactions = new List<BankTransaction>();
                foreach (var ct in credit.Transactions)
                {
                    try
                    {
                        var dataParts = new[] { ct.Description, ct.Comments, ct.TransactionType };
                        decimal amount;
                        decimal.TryParse(ct.Amount.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount);
                        var transactionType = ct.TransactionType;
                        var bt = new BankTransaction
                        {
                            Amount = amount,
                            Currency = string.IsNullOrWhiteSpace(ct.Amount.Currency)
                                ? credit.Balance.Currency
                                : ct.Amount.Currency,
                            Identifier = null,
                            Text = ct.Description,
                            Timestamp = ct.OperationDate.ToEurobitsDateTimeFormat(),
                            Date = ct.OperationDate.ToEurobitsDateTimeFormat(),
                            Data = JsonConvert.SerializeObject(dataParts),
                            IsUncleared = (transactionType == "3")
                        };

                        UpdateTextAndIsMerchant(bt);
                        accountStatement.Transactions.Add(bt);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(
                            $"Reading transaction failed for bank: {_configurationRealm.Bank} and account: {accountId}", ex);
                    }
                }
            }

            return accountStatement;
        }

        private void UpdateTextAndIsMerchant(BankTransaction trans)
        {
            if (_configurationRealm.TextReplacePatterns == null) return;

            foreach (var pattern in _configurationRealm.TextReplacePatterns) pattern.UpdateTextAndIsMerchant(trans);
        }
    }
}