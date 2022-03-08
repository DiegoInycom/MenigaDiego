using System;
using Meniga.Core.BusinessModels;

namespace Ibercaja.Aggregation.Products.Unknown
{
    public class UnknownTransactionsProvider : ITransactionsProvider
    {
        public AccountStatement GetAccountStatement(string accountIdentifier)
        {
            throw new NotImplementedException();
        }
    }
}