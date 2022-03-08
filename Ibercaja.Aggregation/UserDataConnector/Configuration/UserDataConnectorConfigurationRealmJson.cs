using System.Collections.Generic;

namespace Ibercaja.Aggregation.UserDataConnector.Configuration
{
    public class UserDataConnectorConfigurationRealmJson
    {
        public string[] ProductsToFetch { get; set; }
        public string InvertAmount { get; set; }
        public string Bank { get; set; }
        public string UserIdentifier { get; set; }
        public List<TextReplacePatterns> TextReplacePatterns { get; set; }
        public string MonthlyLoginLimit { get; set; }
    }
}
