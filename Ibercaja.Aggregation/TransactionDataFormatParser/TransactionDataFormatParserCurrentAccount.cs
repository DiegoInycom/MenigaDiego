using System.Collections.Generic;

namespace Ibercaja.Aggregation.TransactionDataFormatParser
{
    public class TransactionDataFormatParserCurrentAccount : TransactionDataFormatParser
    {
        private bool _extended = false;

        private static readonly List<string> _dataFields = new List<string>
        {
            "Description",
            "Payee",
            "Payer",
            "Reference"
        };
        private static readonly List<string> _dataFieldsExtended = new List<string>
        {
            "Description",
            "Payee",
            "Payer",
            "Reference",
            "ValueDate",
            "OperationDate"
        };
        protected override List<string> DataFields => _extended ? _dataFieldsExtended : _dataFields;

        private static readonly Dictionary<string, string> _dataFieldNames = new Dictionary<string, string>
        {
            { "Description", "Description" },
            { "Payee", "Payee" },
            { "Payer", "Payer" },
            { "Reference", "Reference" }
        };
        private static readonly Dictionary<string, string> _dataFieldNamesExtended = new Dictionary<string, string>
        {
            { "Description", "Description" },
            { "Payee", "Payee" },
            { "Payer", "Payer" },
            { "Reference", "Reference" },
            { "ValueDate", "ValueDate" },
            { "OperationDate", "OperationDate" }
        };
        protected override Dictionary<string, string> DataFieldNames => _extended ? _dataFieldNamesExtended : _dataFieldNames;

        protected override void PrepareDataFields(List<string> splitData)
        {
            _extended = splitData.Count > 5;
        }
    }
}