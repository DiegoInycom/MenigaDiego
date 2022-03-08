using System;
using System.Collections.Generic;
using Meniga.Core.BusinessModels;

namespace Ibercaja.Aggregation.Products.Unknown
{
    public class UnknownAccountProvider : IAccountsProvider
    {
        public IEnumerable<BankAccountInfo> GetBankAccountInfos()
        {
            throw new NotImplementedException();
        }
    }
}