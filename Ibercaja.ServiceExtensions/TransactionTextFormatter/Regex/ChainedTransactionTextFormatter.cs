using System;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;
using Meniga.Core.BusinessModels;
using Meniga.Core.Transactions;
using Newtonsoft.Json;
using Meniga.Core.Extensions;

namespace Ibercaja.ServiceExtensions.TransactionTextFormatter.Regex
{
    /// <summary>
    /// Chained Transaction Text Formatter class based on Regex pattern matching
    /// Is generic for JSON text replacement strings where the output of first pattern is used as input for the next.
    /// </summary>
    public class ChainedTransactionTextFormatter : ITransactionTextFormatter, IStringConfigurable
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
                _logger.Error(string.Format("Unable to parse config: {0}", config), ex);
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

            if (_formatConfig != null)
            {
                // Go through ALL of the TextReplacementPatterns, don't stop until all have been evaluated.
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
            if (string.IsNullOrEmpty(regexPattern.Pattern) || string.IsNullOrEmpty(regexPattern.Replace))
            {
                _logger.ErrorFormat("Invalid TextReplacePattern, probably caused by wrong property name in the Config JSON string. Pattern:[{0}], Replace:[{1}]",
                    string.IsNullOrEmpty(regexPattern.Pattern) ? "NullOrEmptyString" : regexPattern.Pattern,
                    string.IsNullOrEmpty(regexPattern.Replace) ? "NullOrEmpty" : regexPattern.Replace);

                return transactionText;
            }

            if (string.IsNullOrEmpty(transactionText))
            {
                return transactionText;
            }

            // Do the Regex cleanup.
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern.Pattern);
            Match match = regex.Match(transactionText);
            if (match.Success)
            {
                transactionText = match.Result(regexPattern.Replace);
            }

            return transactionText;
        }

    }
}
