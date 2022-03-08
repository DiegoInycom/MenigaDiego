using System.Collections.Generic;

namespace Ibercaja.Aggregation.TransactionDataFormatParser
{
    public class TransactionDataFormatParserPensionPlan : TransactionDataFormatParser
    {

        private static readonly List<string> _dataFields = new List<string>
        {
             "Description"
        };
        protected override List<string> DataFields => _dataFields;

        private static readonly Dictionary<string, string> _dataFieldNames = new Dictionary<string, string>
        {
            { "Description", "Description" }
        };
        protected override Dictionary<string, string> DataFieldNames => _dataFieldNames;
    }
}