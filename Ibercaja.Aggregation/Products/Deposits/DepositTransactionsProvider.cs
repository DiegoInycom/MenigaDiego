using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Meniga.Core.BusinessModels;
using Ibercaja.Aggregation.Eurobits;

namespace Ibercaja.Aggregation.Products.Deposits
{
    public class DepositTransactionsProvider : ITransactionsProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DepositTransactionsProvider));

        private readonly IAggregationService _aggregationService;

        public DepositTransactionsProvider(IAggregationService aggregationService)
        {
            _aggregationService = aggregationService;
        }

        public AccountStatement GetAccountStatement(string accountIdentifier)
        {
            var deposits = _aggregationService.GetDeposits();

            return GetDepositTransactions(deposits, accountIdentifier);
        }

        private AccountStatement GetDepositTransactions(IEnumerable<Deposit> deposits, string accountId)
        {
            var accountStatement = new AccountStatement();

            var deposit = deposits.FirstOrDefault(x => x.AccountNumber == accountId);
            if (deposit != null)
            {
                decimal amount;
                decimal.TryParse(deposit.Balance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount);
                accountStatement.Balance = amount;

                accountStatement.Transactions = new List<BankTransaction>();
            }

            return accountStatement;
        }
    }
}