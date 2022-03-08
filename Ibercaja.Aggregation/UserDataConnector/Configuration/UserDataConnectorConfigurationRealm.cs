using System.Collections.Generic;
using System.Linq;

namespace Ibercaja.Aggregation.UserDataConnector.Configuration
{
    public class UserDataConnectorConfigurationRealm
    {
        public string[] ProductsToFetch { get; }
        public bool InvertAmount { get; }
        public string Bank { get; }
        public string UserIdentifier { get; }
        public List<RegexpReplaceIsMerchant> TextReplacePatterns { get; }
        public string MonthlyLoginLimit { get; }

        public UserDataConnectorConfigurationRealm(UserDataConnectorConfigurationRealmJson json)
        {
            ProductsToFetch = json.ProductsToFetch;
            if (json.InvertAmount == "1" || json.InvertAmount.ToLower() == "true")
            {
                InvertAmount = true;
            }
            else
            {
                InvertAmount = false;
            }
            Bank = json.Bank;
            UserIdentifier = json.UserIdentifier;
            TextReplacePatterns = json.TextReplacePatterns?
                .Select(x => new RegexpReplaceIsMerchant(x.Pattern)
                {
                    ExternalPrefix = x.ExternalPrefix,
                    ExternalReplace = x.ExternalReplace,
                    IsMerchant = x.IsMerchant,
                    IsOwnAccountTransfer = x.IsOwnAccountTransfer,
                    Replace = x.Replace
                })?.ToList();
            MonthlyLoginLimit = json.MonthlyLoginLimit;
        }

        public UserDataConnectorConfigurationRealm(
            UserDataConnectorConfigurationRealm configuration,
            string[] productsToFetch = null,
            bool? invertAmount = null,
            string bank = null,
            string userIdentifier = null,
            List<RegexpReplaceIsMerchant> textReplacePatterns = null,
            string monthlyLoginLimit = null)
        {
            ProductsToFetch = productsToFetch ?? configuration.ProductsToFetch;
            InvertAmount = invertAmount ?? configuration.InvertAmount;
            Bank = bank ?? configuration.Bank;
            UserIdentifier = userIdentifier ?? configuration.UserIdentifier;
            TextReplacePatterns = textReplacePatterns ?? configuration.TextReplacePatterns;
            MonthlyLoginLimit = monthlyLoginLimit ?? configuration.MonthlyLoginLimit;
        }
    }
}
