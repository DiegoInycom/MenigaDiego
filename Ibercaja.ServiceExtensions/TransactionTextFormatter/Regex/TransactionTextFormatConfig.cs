﻿using System.Collections.Generic;

namespace Ibercaja.ServiceExtensions.TransactionTextFormatter.Regex
{
    /// <summary>
    /// Class that stores the TransactionTextFormat config from the dbo.Transaction_Text_Formatter table
    /// Example JSON: {"textReplacePatterns": [ {"pattern": "^Recibo\\s(.*)", "replace": "$1"}, {"pattern": "(Compra en)(.*)", "replace": "$2"} ]}
    /// </summary>
    public class TransactionTextFormatConfig
    {
        public TransactionTextFormatConfig()
        {
            TextReplacePatterns = new List<TextReplacePattern>();
        }

        /// <summary>
        /// List of all the TextReplacementPatterns
        /// </summary>
        public IList<TextReplacePattern> TextReplacePatterns { get; set; }
    }
}
