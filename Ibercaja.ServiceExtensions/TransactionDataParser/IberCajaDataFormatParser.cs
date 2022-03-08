using System;
using System.Collections.Generic;
using System.Linq;
using Meniga.Core.BusinessModels;
using Meniga.Core.Transactions;

namespace Ibercaja.ServiceExtensions.TransactionDataParser
{
    public class IbercajaDataFormatParser : ITransactionDataFormatParser
    {
        public const char DataFieldSeperator = '|';

        private static readonly List<string> DataFieldsIbercaja = new List<string>
        {
            "SIGNOOP_CONCEPTOOP","SIGNOOP", "CONCEPTOOP", "DESCCONCE","REFERMOV1","REFERMOV2","REFERMOV3", "TEXTOC", "TEXTO1","TEXTO123", "FUC", "MCC2","CODOP", "DESCCODOP", "FINALIDAD", "LIBRE"
        };

        private static readonly Dictionary<string, string> DataFieldNames = new Dictionary<string, string>
        {
            { "SIGNOOP_CONCEPTOOP", "SIGNOOP_CONCEPTOOP" },
            { "SIGNOOP", "SIGNOOP" },
            { "CONCEPTOOP", "CONCEPTOOP" },
            { "DESCCONCE", "DESCCONCE" },
            { "REFERMOV1", "REFERMOV1" },
            { "REFERMOV2", "REFERMOV2" },
            { "REFERMOV3", "REFERMOV3" },
            { "TEXTOC", "TEXTOC" },
            { "TEXTO1", "TEXTO1" },
            { "TEXTO123", "TEXTO123" },
            { "FUC", "FUC" },
            { "MCC2", "MCC2" },
            { "CODOP", "CODOP" },
            { "DESCCODOP", "DESCCODOP" },
            { "FINALIDAD", "FINALIDAD" },
            { "LIBRE", "LIBRE" }
        };

        public IDictionary<string, string> ParseData(string data)
        {
            var dict = new Dictionary<string, string>();
            if (String.IsNullOrWhiteSpace(data)) return dict;

            var splitData = data.Split(DataFieldSeperator);

            if (splitData.Length == DataFieldsIbercaja.Count())
            {
                for (var i = 0; i < DataFieldsIbercaja.Count(); i++)
                {
                    if (!string.IsNullOrWhiteSpace(splitData[i])) { dict.Add(DataFieldsIbercaja[i], splitData[i]); }
                }
            }
            else if (splitData.Length == DataFieldsIbercaja.Count() - 1) // Es el campo libre y no está informado
            {
                for (var i = 0; i < DataFieldsIbercaja.Count() - 1; i++)
                {
                    if (!string.IsNullOrWhiteSpace(splitData[i])) { dict.Add(DataFieldsIbercaja[i], splitData[i]); }
                }
                //dict.Add(DataFieldsIbercaja[DataFieldsIbercaja.Count() - 1], "");
            }
            else
            {
                throw new Exception(
                    String.Format("Incorrect number of fields in Data field. Is {0} instead of {1}",
              splitData.Length, DataFieldsIbercaja.Count()));
            }

            return dict;
        }

        public IDictionary<string, string> GetKeyDisplayNames()
        {
            return DataFieldNames;
        }

        public bool AreCounterpartyTransactions(BankTransaction bankTrans1, BankTransaction bankTrans2)
        {
            return false;
        }
    }
}
