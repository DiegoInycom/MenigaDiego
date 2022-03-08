using Meniga.Core.BusinessModels;
using Meniga.Core.Transactions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Ibercaja.Aggregation.TransactionDataFormatParser
{
    public abstract class TransactionDataFormatParser : ITransactionDataFormatParser
    {
        protected abstract List<string> DataFields { get; }

        protected abstract Dictionary<string, string> DataFieldNames { get; }

        protected virtual void PrepareDataFields(List<string> splitData) { }


        public IDictionary<string, string> ParseData(string data)
        {
            var dict = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(data))
            {
                return dict;
            }

            var splitData = JsonConvert.DeserializeObject<List<string>>(data);

            PrepareDataFields(splitData);

            dict = MergeWithDataFields(splitData);

            return dict;
        }

        private Dictionary<string, string> MergeWithDataFields(List<string> splitData)
        {
            var dic = new Dictionary<string, string>();

            for (int index = 0; index < DataFields.Count; index++)
            {
                var aKey = DataFields.Skip(index).First();
                var aValue = splitData.Skip(index).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(aValue))
                {
                    dic[aKey] = aValue;
                }
            }
            return dic;
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