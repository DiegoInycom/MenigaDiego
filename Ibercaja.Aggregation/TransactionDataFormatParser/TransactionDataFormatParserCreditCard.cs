using System.Collections.Generic;
using System.Linq;

namespace Ibercaja.Aggregation.TransactionDataFormatParser
{
    public class TransactionDataFormatParserCreditCard : TransactionDataFormatParser
    {
        private bool _isCredit = false;

        private static readonly List<string> _dataFieldsCredit = new List<string>
        {
            "Description",
            "Payee",
            "Payer",
            "Reference"
        };
        private static readonly List<string> _dataFieldsCreditCard = new List<string>
        {
            "Description",
            "Comments",
            "TransactionType"
        };
        protected override List<string> DataFields => _isCredit ? _dataFieldsCredit : _dataFieldsCreditCard;

        private static readonly Dictionary<string, string> _dataFieldNamesCredit = new Dictionary<string, string>
        {
            { "Description", "Description" },
            { "Payee", "Payee" },
            { "Payer", "Payer" },
            { "Reference", "Reference" }
        };
        private static readonly Dictionary<string, string> _dataFieldNamesCreditCard = new Dictionary<string, string>
        {
            { "Description", "Description" },
            { "Comments", "Comments" },
            { "TransactionType", "TransactionType"}
        };
        protected override Dictionary<string, string> DataFieldNames => _isCredit ? _dataFieldNamesCredit : _dataFieldNamesCreditCard;

        protected override void PrepareDataFields(List<string> splitData)
        {
            splitData[1] = splitData[1] ?? "";
            _isCredit = splitData[1].Equals("1");

            if (_isCredit)
            {
                splitData.Remove(splitData.FirstOrDefault(t => t.Contains("1")));
            }
        }
    }
}