namespace Ibercaja.Aggregation.UserDataConnector.Configuration
{
    public class TextReplacePatterns
    {
        public string Pattern { get; set; }
        public string Replace { get; set; }

        public bool? IsMerchant { get; set; }

        public string ExternalPrefix { get; set; }

        public string ExternalReplace { get; set; }

        public bool? IsOwnAccountTransfer { get; set; }
    }
}
