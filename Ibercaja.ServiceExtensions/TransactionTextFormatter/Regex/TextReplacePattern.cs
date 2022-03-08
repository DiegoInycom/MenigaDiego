namespace Ibercaja.ServiceExtensions.TransactionTextFormatter.Regex
{
    /// <summary>
    /// Class for individual Text Replacement Pattern used in the chained Transaction Text Formatter
    /// </summary>
    public class TextReplacePattern
    {
        /// <summary>
        /// The Regex search Pattern
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// The Replacement regex value (variable)
        /// </summary>
        public string Replace { get; set; }
    }
}
