using System.Collections.Generic;

namespace Ibercaja.Aggregation.TransactionDataFormatParser
{
    public class TransactionDataFormatParserShare : TransactionDataFormatParser
    {

        private static readonly List<string> _dataFields = new List<string>
        {
            "Description",
            "Name",
            "Market",
            "Operation_Type",
            "Quantity_Unit_Price"
        };

        protected override List<string> DataFields => _dataFields;

        private static readonly Dictionary<string, string> _dataFieldNames = new Dictionary<string, string>
        {
            { "Description", "Description" },
            { "Name", "Name" },
            { "Market", "Market" },
            { "Operation_Type", "Operation_Type" },
            { "Quantity_Unit_Price", "Quantity_Unit_Price" }
        };

        protected override Dictionary<string, string> DataFieldNames => _dataFieldNames;
    }
}
