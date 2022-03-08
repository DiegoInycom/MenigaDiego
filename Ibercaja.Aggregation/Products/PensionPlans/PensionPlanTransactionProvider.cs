using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Meniga.Core.BusinessModels;
using Newtonsoft.Json;
using Ibercaja.Aggregation.Eurobits;
using Ibercaja.Aggregation.UserDataConnector.Configuration;

namespace Ibercaja.Aggregation.Products.PensionPlans
{
    public class PensionPlanTransactionsProvider : ITransactionsProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PensionPlanTransactionsProvider));

        private readonly IAggregationService _aggregationService;
        private readonly UserDataConnectorConfigurationRealm _configurationRealm;

        public PensionPlanTransactionsProvider(
            IAggregationService aggregationService,
            UserDataConnectorConfigurationRealm configurationRealm)
        {
            _aggregationService = aggregationService;
            _configurationRealm = configurationRealm;
        }

        public AccountStatement GetAccountStatement(string accountIdentifier)
        {
            var pensionPlans = _aggregationService.GetPensionPlans();

            return GetPensionPlansTransactions(pensionPlans, accountIdentifier);
        }

        private AccountStatement GetPensionPlansTransactions(IEnumerable<PensionPlan> pensionPlans, string accountId)
        {
            var accountStatement = new AccountStatement();

            var pensionPlan = pensionPlans.FirstOrDefault(pp => pp.PlanNumber == accountId);
            if (pensionPlan != null)
            {
                decimal accountAmount;
                decimal.TryParse(pensionPlan.Balance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out accountAmount);
                accountStatement.Balance = accountAmount;

                accountStatement.Transactions = new List<BankTransaction>();
                foreach (var ppt in pensionPlan.Transactions)
                {
                    try
                    {
                        var dataParts = new[] { ppt.Description };
                        decimal amount;
                        if (decimal.TryParse(ppt.Amount.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount))
                        {
                            var bt = new BankTransaction
                            {
                                Amount = amount,
                                Currency = string.IsNullOrWhiteSpace(ppt.Amount.Currency)
                                    ? pensionPlan.Balance.Currency
                                    : ppt.Amount.Currency,
                                Identifier = null,
                                Text = ppt.Description,
                                Timestamp = ppt.ValueDate.ToEurobitsDateTimeFormat(),
                                Date = ppt.ValueDate.ToEurobitsDateTimeFormat(),
                                Data = JsonConvert.SerializeObject(dataParts)
                            };
                            UpdateTextAndIsMerchant(bt);
                            accountStatement.Transactions.Add(bt);
                        }
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