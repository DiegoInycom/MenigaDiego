using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Meniga.Core.BusinessModels;
using Newtonsoft.Json;
using Ibercaja.Aggregation.Eurobits;
using Ibercaja.Aggregation.UserDataConnector.Configuration;

namespace Ibercaja.Aggregation.Products.Shares
{
    public class ShareTransactionsProvider : ITransactionsProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ShareTransactionsProvider));

        private readonly IAggregationService _aggregationService;
        private readonly UserDataConnectorConfigurationRealm _configurationRealm;

        public ShareTransactionsProvider(
            IAggregationService aggregationService,
            UserDataConnectorConfigurationRealm configurationRealm
        )
        {
            _aggregationService = aggregationService;
            _configurationRealm = configurationRealm;
        }

        public AccountStatement GetAccountStatement(string accountIdentifier)
        {
            var shares = _aggregationService.GetShares();

            return GetShareTransactions(shares, accountIdentifier);
        }

        private AccountStatement GetShareTransactions(IEnumerable<Share> shares, string accountId)
        {
            var accountStatement = new AccountStatement();

            var share = shares.FirstOrDefault(f => f.AccountNumber == accountId);
            if (share != null)
            {
                decimal accountBalance;
                decimal.TryParse(share.Balance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out accountBalance);
                accountStatement.Balance = accountBalance;

                accountStatement.Transactions = new List<BankTransaction>();
                foreach (var st in share.Transactions)
                {
                    try
                    {
                        var dataParts = new[] { st.OperationDescription, st.Name, st.Market, st.OperationType, $"{st.Quantity}|{st.UnitPrice.Value}{st.UnitPrice.Currency}" };
                        decimal amount;
                        decimal.TryParse(st.Amount.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount);
                        decimal quantity;
                        decimal.TryParse(st.Quantity, NumberStyles.Currency, CultureInfo.InvariantCulture, out quantity);
                        var bt = new BankTransaction
                        {
                            Amount = amount,
                            Currency = string.IsNullOrWhiteSpace(st.Amount.Currency)
                                ? share.Balance.Currency
                                : st.Amount.Currency,
                            Identifier = null,
                            Text = st.OperationDescription,
                            Timestamp = st.OperationDate.ToEurobitsDateTimeFormat(),
                            Date = st.OperationDate.ToEurobitsDateTimeFormat(),
                            Data = JsonConvert.SerializeObject(dataParts)
                        };

                        UpdateTextAndIsMerchant(bt);
                        accountStatement.Transactions.Add(bt);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(
                            $"Reading transaction failed for bank: {_configurationRealm.Bank} and share: {accountId}", ex);
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