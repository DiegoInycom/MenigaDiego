using System.Collections.Generic;
using Meniga.Core.BusinessModels;

namespace Ibercaja.Aggregation.Products
{
    /// <summary>
    ///     General purpose interface for implementing product specific logic
    ///     for account information retrieval
    /// </summary>
    public interface IAccountsProvider
    {
        /// <summary>
        ///     Product specific logic for bank account retrieval
        /// </summary>
        /// <returns></returns>
        IEnumerable<BankAccountInfo> GetBankAccountInfos();
    }
}