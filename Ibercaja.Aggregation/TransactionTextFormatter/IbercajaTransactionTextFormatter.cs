using Meniga.Core.BusinessModels;
using Meniga.Core.Extensions;
using Meniga.Core.Transactions;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace Ibercaja.Aggregation.TransactionTextFormatter
{
    /// <summary>
    /// Transaction Text Formatter class for Ibercaja.
    /// Is generic for JSON text replacement strings.
    /// </summary>
    public class IbercajaTransactionTextFormatter : ITransactionTextFormatter, IStringConfigurable
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger("IbercajaTransactionTextFormatter");
        private TransactionTextFormatConfig _formatConfig = null;

        /// <summary>
        /// Convert the JSON config string of the dbo.Transaction_Text_Formatter table into a TransactionTextFormatConfig object
        /// Example JSON: {"textReplacePatterns": [ {"pattern": "^Recibo\\s(.*)", "replace": "$1"}, {"pattern": "(Compra en)(.*)", "replace": "$2"} ]}
        /// </summary>
        /// <param name="config"></param>
        public void Configure(string config)
        {
            try
            {
                _formatConfig = JsonConvert.DeserializeObject<TransactionTextFormatConfig>(config);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Unable to parse TransactionTextFormatConfig config: {0}", config), ex);
                _formatConfig = null;
            }
        }

        /// <summary>
        /// Function that cleans up the text field of the Transaction, based on Regex rules in the TransactionTextFormatConfig
        /// </summary>
        /// <param name="bankTransaction">The incoming transaction</param>
        /// <param name="merchant">The incoming merchant</param>
        /// <returns>Cleaned up transaction text</returns>
        public string FormatText(BankTransaction bankTransaction, Merchant merchant)
        {
            string bankTransactionText = bankTransaction.Text;
            if (merchant != null)
            {
                // if the merchant is known the we use the merchant name (or parent name)...
                bankTransactionText = !string.IsNullOrEmpty(merchant.ParentName) ? merchant.ParentName : merchant.Name;
            }
            else if (_formatConfig != null)
            {
                // if merchant is not known then we go through ALL of the TextReplacementPatterns until all have been evaluated.
                foreach (TextReplacePattern replacePattern in _formatConfig.TextReplacePatterns)
                {
                    bankTransactionText = RegexCleanText(replacePattern, bankTransactionText);
                }
            }

            // Until we clean it up, return the original text
            return bankTransactionText;
        }

        /// <summary>
        /// Regex function to clean the text 
        /// </summary>
        /// <param name="regexPattern">The Regex pattern and replacement parameters</param>
        /// <param name="transactionText">The text to clean</param>
        /// <returns>Cleaned up transaction Text (in case any of the regex pattern rules matched.</returns>
        private string RegexCleanText(TextReplacePattern regexPattern, string transactionText)
        {
            // Make sure the regex and incoming transactionText is OK
            if (string.IsNullOrEmpty(regexPattern.Pattern))
            {
                _logger.ErrorFormat(
                    "Invalid TextReplacePattern, probably caused by wrong property name in23 the Config JSON string. Pattern:[{0}], Replace:[{1}]",
                    string.IsNullOrEmpty(regexPattern.Pattern) ? "NullOrEmptyString" : regexPattern.Pattern);
                return transactionText;
            }

            if (string.IsNullOrEmpty(transactionText))
            {
                return transactionText;
            }

            // Do the Regex cleanup.
            Regex regex = new Regex(regexPattern.Pattern);
            Match match = regex.Match(transactionText);
            if (match.Success)
            {
                transactionText = transactionText.Replace(match.ToString(), regexPattern.Replace); // match.Result(regexPattern.Replace);
            }

            return transactionText;
        }
    }
}
