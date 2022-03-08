using Meniga.Core.BusinessModels;

namespace Ibercaja.Aggregation.Products
{
    /// <summary>
    ///     General purpose interface for implementing product specific logic
    ///     for transaction information retrieval
    /// </summary>
    public interface ITransactionsProvider
    {
        AccountStatement GetAccountStatement(string accountIdentifier);
    }
}