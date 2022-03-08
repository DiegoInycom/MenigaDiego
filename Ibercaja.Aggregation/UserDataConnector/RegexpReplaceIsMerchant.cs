using System.Text.RegularExpressions;
using Meniga.Core.BusinessModels;

namespace Ibercaja.Aggregation.UserDataConnector
{
    public class RegexpReplaceIsMerchant
    {
        private Regex _compiledExpression = null;

        public RegexpReplaceIsMerchant(string pattern)
        {
            _compiledExpression = new Regex(pattern);
        }

        public string Replace { get; set; }

        public bool? IsMerchant { get; set; }

        public string ExternalPrefix { get; set; }

        public string ExternalReplace { get; set; }

        public bool? IsOwnAccountTransfer { get; set; }

        public void UpdateTextAndIsMerchant(BankTransaction trans)
        {
            Match match = _compiledExpression.Match(trans.Text);
            if (match.Success)
            {
                if (IsMerchant.HasValue)
                {
                    trans.IsMerchant = IsMerchant;
                }
                trans.Text = match.Result(Replace == null ? string.Empty : Replace);

                if (!string.IsNullOrEmpty(ExternalReplace))
                {
                    trans.ExternalMerchantIdentifier = $"{ExternalPrefix}{match.Result(ExternalReplace)}";
                }
                if (IsOwnAccountTransfer.HasValue)
                {
                    trans.IsOwnAccountTransfer = IsOwnAccountTransfer;
                }
            }
        }

        public override string ToString()
        {
            return "Pattern: " + _compiledExpression + ", Replace: " + Replace + " IsMerchant: " + IsMerchant;
        }
    }
}
