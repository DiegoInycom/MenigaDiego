using System.Collections.Generic;
using Meniga.Core.BusinessModels;

namespace Ibercaja.Aggregation.Products
{
    /// <summary>
    ///     Encapsulates required dependencies for all product providers construction
    ///     Should contain mappings between meniga products and customer products
    /// </summary>
    public interface IProductProviderFactory
    {
        ITransactionsProvider
            GetTransactionsProvider(AccountCategoryEnum accountCategory, string accountCategoryDetail);

        IAccountsProvider GetAccountsProvider(AccountCategoryEnum accountCategory, string accountCategoryDetail);

        /// <summary>
        ///     Returns all available account providers
        /// </summary>
        /// <returns></returns>
        IEnumerable<IAccountsProvider> GetAllAccountsProviders();
    }
}