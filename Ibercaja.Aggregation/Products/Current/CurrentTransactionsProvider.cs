using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Meniga.Core.BusinessModels;
using Newtonsoft.Json;
using Ibercaja.Aggregation.Eurobits;
using Ibercaja.Aggregation.UserDataConnector.Configuration;
using Account = Ibercaja.Aggregation.Eurobits.Account;

namespace Ibercaja.Aggregation.Products.Current
{
    public class CurrentTransactionsProvider : ITransactionsProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CurrentTransactionsProvider));

        private readonly IAggregationService _aggregationService;
        private readonly UserDataConnectorConfigurationRealm _configurationRealm;

        public CurrentTransactionsProvider(
            IAggregationService aggregationService,
            UserDataConnectorConfigurationRealm configurationRealm
        )
        {
            _aggregationService = aggregationService;
            _configurationRealm = configurationRealm;
        }

        public AccountStatement GetAccountStatement(string accountIdentifier)
        {
            var accounts = _aggregationService.GetAccounts();

            return GetCurrentTransactions(accounts, accountIdentifier);
        }

        private AccountStatement GetCurrentTransactions(IEnumerable<Account> accounts, string accountId)
        {
            var accountStatement = new AccountStatement();

            var current = accounts.FirstOrDefault(f => f.AccountNumber == accountId);
            if (current != null)
            {
                decimal accountAmount;
                decimal.TryParse(current.Balance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out accountAmount);
                accountStatement.Balance = accountAmount;

                accountStatement.Transactions = new List<BankTransaction>();
                foreach (var at in current.Transactions)
                {
                    try
                    {
                        var dataParts = new[] { at.Description, at.Payee, at.Payer, at.Reference, at.OperationDate, at.ValueDate };
                        decimal balanceAmount;
                        decimal.TryParse(at.Balance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out balanceAmount);
                        decimal amount;
                        decimal.TryParse(at.Amount.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount);
                        var bt = new BankTransaction
                        {
                            Amount = amount,
                            Currency = string.IsNullOrWhiteSpace(at.Amount.Currency)
                                ? current.Balance.Currency
                                : at.Amount.Currency,
                            Identifier = null,
                            Text = at.Description,
                            Timestamp = at.OperationDate.ToEurobitsDateTimeFormat(),
                            Date = string.IsNullOrEmpty(at.ValueDate) ? at.OperationDate.ToEurobitsDateTimeFormat() : at.ValueDate.ToEurobitsDateTimeFormat(),
                            Data = JsonConvert.SerializeObject(dataParts),
                            AccountBalance = balanceAmount
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