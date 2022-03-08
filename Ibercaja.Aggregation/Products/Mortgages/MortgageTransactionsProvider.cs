using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Meniga.Core.BusinessModels;
using Ibercaja.Aggregation.Eurobits;

namespace Ibercaja.Aggregation.Products.Mortgages
{
    public class MortgageTransactionsProvider : ITransactionsProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MortgageTransactionsProvider));

        private readonly IAggregationService _aggregationService;

        public MortgageTransactionsProvider(IAggregationService aggregationService)
        {
            _aggregationService = aggregationService;
        }

        public AccountStatement GetAccountStatement(string accountIdentifier)
        {
            var loans = _aggregationService.GetLoans();

            return GetLoansTransactions(loans, accountIdentifier);
        }

        private AccountStatement GetLoansTransactions(IEnumerable<Loan> loans, string accountId)
        {
            var accountStatement = new AccountStatement();

            var loan = loans.FirstOrDefault(x => x.AccountNumber == accountId);
            if (loan != null)
            {
                decimal amount;
                decimal.TryParse(loan.Debt.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount);
                decimal limit;
                decimal.TryParse(loan.InitialBalance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out limit);
                accountStatement.Balance = amount;
                accountStatement.Limit = limit;

                accountStatement.Transactions = new List<BankTransaction>();
            }

            return accountStatement;
        }
    }
}