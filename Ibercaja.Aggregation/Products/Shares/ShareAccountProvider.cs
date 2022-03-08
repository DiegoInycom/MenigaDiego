using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Meniga.Core.BusinessModels;
using Ibercaja.Aggregation.Eurobits;

namespace Ibercaja.Aggregation.Products.Shares
{
    public class ShareAccountProvider : IAccountsProvider
    {
        private const string ShareAccountFlag = "ShareAccount";
        private const string ShareName = "ShareName";
        private const string ShareUnitPrice = "ShareUnitPrice";
        private const string ShareMarket = "ShareMarket";
        private const string ShareQuantity = "ShareQuantity";
        private const string ShareValuationDate = "ShareValuationDate";
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ShareAccountProvider));
        private readonly IAggregationService _aggregationService;
        private const string Relationship = "Relationship0";
        private readonly string _userDocument;


        public ShareAccountProvider(IAggregationService aggregationService, string userDocument)
        {
            _aggregationService = aggregationService;
            _userDocument = userDocument;
        }

        public IEnumerable<BankAccountInfo> GetBankAccountInfos()
        {
            var shares = _aggregationService.GetShares();

            foreach (var share in shares)
            {
                decimal amount;
                if (decimal.TryParse(share.Balance.Value, NumberStyles.Currency, CultureInfo.InvariantCulture, out amount))
                {
                    var b = new BankAccountInfo
                    {
                        AccountCategory = AccountCategoryEnum.Asset,
                        AccountCategoryDetails = IbercajaProducts.Share,
                        AccountIdentifier = share.AccountNumber,
                        Balance = amount,
                        CurrencyCode = share.Balance.Currency,
                        Limit = 0,
                        Name = share.WebAlias,
                        AccountParameters = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>(
                                ShareAccountFlag,
                                "true"),
                            new KeyValuePair<string, string>(
                                Relationship, 
                                ExtractRelation(_userDocument))

                        }
                    };
                    b.AccountParameters = b.AccountParameters
                        .Union(ExtractStocks(share))
                        .ToList();
                    yield return b;
                }
                else
                {
                    Logger.Error($"Failed to parse Share info: {share.AccountNumber}");
                }
            }
        }

        // Create List for Stocks information
        private IEnumerable<KeyValuePair<string, string>> ExtractStocks(Share share)
        {
            foreach (var item in share.Stocks.Select((stock, index) => new { stock, index }))
            {
                yield return new KeyValuePair<string, string>(
                    $"{ShareName}{item.index}",
                    $"{item.stock.Name}");
                yield return new KeyValuePair<string, string>(
                    $"{ShareUnitPrice}{item.index}",
                    $"{item.stock.UnitPrice.Value}{item.stock.UnitPrice.Currency}");
                yield return new KeyValuePair<string, string>(
                    $"{ShareMarket}{item.index}",
                    $"{item.stock.Market}");
                yield return new KeyValuePair<string, string>(
                    $"{ShareQuantity}{item.index}",
                    $"{item.stock.Quantity}");
                yield return new KeyValuePair<string, string>(
                    $"{ShareValuationDate}{item.index}",
                    $"{item.stock.ValuationDate}");
            }
        }

        private string ExtractRelation(string userDocument)
        {
            var document = _aggregationService.GetPersonalInfo()?.Document;

            if (string.IsNullOrWhiteSpace(document) || string.IsNullOrWhiteSpace(userDocument))
            {
                return "Unknown";
            }
            else
            {
                return userDocument.Contains(document) ? "Titular" : "Unknown";
            }
        }
    }
}