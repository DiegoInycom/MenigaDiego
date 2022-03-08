namespace Ibercaja.Aggregation.TransactionTextFormatter
{
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
