using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Meniga.Core.BusinessModels;
using Newtonsoft.Json;
using Ibercaja.Aggregation.Eurobits;
using Ibercaja.Aggregation.UserDataConnector.Configuration;

namespace Ibercaja.Aggregation.Products.Funds
{
    public class FundTransactionsProvider : ITransactionsProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FundTransactionsProvider));

        private readonly IAggregationService _aggregationService;
        private readonly UserDataConnectorConfigurationRealm _configurationRealm;

        public FundTransactionsProvider(
            IAggregationService aggregationService,
            UserDataConnectorConfigurationRealm configurationRealm)
        {
            _aggregationService = aggregationService;
            _configurationRealm = configurationRealm;
        }

        public AccountStatement GetAccountStatement(string accountIdentifier)
        {
            var funds = _aggregationService.GetFunds();

            return GetFundTransactions(funds, accountIdentifier);
        }

        private AccountStatement GetFundTransactions(IEnumerable<Fund> funds, string accountId)
        {
            var accountStatement = new AccountStatement();

            var fund = funds.FirstOrDefault(f => f.AccountNumber == accountId);
            if (fund != null)
            {
                decimal accountAmount;
                decimal.TryParse(fund.Balance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out accountAmount);
                accountStatement.Balance = accountAmount;

                accountStatement.Transactions = new List<BankTransaction>();
                foreach (var ft in fund.Transactions)
                {
                    try
                    {
                        var dataParts = new[] { ft.OperationDescription };
                        decimal amount;
                        decimal.TryParse(ft.Amount?.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount);

                        var bt = new BankTransaction
                        {
                            Amount = amount,
                            Currency = string.IsNullOrWhiteSpace(ft.Amount?.Currency)
                                ? fund.Balance.Currency
                                : ft.Amount.Currency,
                            Identifier = null,
                            Text = ft.OperationDescription,
                            Timestamp = ft.OperationDate.ToEurobitsDateTimeFormat(),
                            Date = ft.OperationDate.ToEurobitsDateTimeFormat(),
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

        private void UpdateTextAndIsMerchant(BankTransaction trans)
        {
            if (_configurationRealm.TextReplacePatterns == null) return;

            foreach (var pattern in _configurationRealm.TextReplacePatterns) pattern.UpdateTextAndIsMerchant(trans);
        }
    }
}
