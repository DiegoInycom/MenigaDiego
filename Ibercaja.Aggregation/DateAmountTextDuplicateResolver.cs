using log4net;
using Meniga.Core.BankConnections;
using Meniga.Core.BusinessModels;
using Meniga.Runtime.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace Ibercaja.Aggregation.DuplicateResolver
{
    public class DateAmountTextDuplicateResolver : BankTransactionDuplicateResolverBase
    {
        readonly bool _cleanupMode;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DateAmountTextDuplicateResolver));

        public DateAmountTextDuplicateResolver() : this(false)
        {
        }

        public DateAmountTextDuplicateResolver(bool cleanupMode)
        {
            _cleanupMode = cleanupMode;
        }

        #region IBankTransactionDuplicateResolver Members

        public override DuplicateResolverResults ResolveDuplicates(IEnumerable<BankTransaction> transList, IEnumerable<BankTransaction> existingTransList)
        {
            #region Generate identifier for existing transactions
            var transactionsToDelete = new List<BankTransaction>();
            var existingIdentifierCounter = new Dictionary<string, IList<BankTransaction>>();
            foreach (var existingTrans in existingTransList) //create a 'hash' with linked lists for duplicate entries
            {
                //this method works ok with JSON on Data
                //convert when comes with pipes into JSON to get same format for all 
                if (existingTrans.Data.Contains("|"))
                {
                    existingTrans.Data = ParseDataPipes(existingTrans.Data);
                }

                // check if uncleared and delete it 
                if (existingTrans.IsUncleared)
                {
                    transactionsToDelete.Add(existingTrans);
                }
                else
                {
                    var amount = existingTrans.AmountInCurrency.HasValue && existingTrans.AmountInCurrency.Value != 0
                        ? existingTrans.AmountInCurrency.Value : existingTrans.Amount;

                    var generatedId = BankConnectionUtil.GenerateId(existingTrans.Date, amount, "");

                    //There is an existing transaction with the same identifier, add to duplicates counter
                    if (existingIdentifierCounter.ContainsKey(generatedId))
                    {
                        existingIdentifierCounter[generatedId].Add(existingTrans);
                    }
                    else
                    {
                        existingIdentifierCounter[generatedId] = new List<BankTransaction> { existingTrans };
                    }
                }
            }
            #endregion

            #region Compare newly fetched transactions to existing transactions
            var transactionsToAdd = new List<BankTransaction>();
            var transactionsToUpdate = new List<BankTransaction>();
            foreach (var trans in transList) //for each transaction coming from Eurobits
            {
                //if comes with pipes then convert a JSON
                if (trans.Data.Contains("|"))
                {
                    trans.Data = ParseDataPipes(trans.Data);
                }

                var amount = trans.AmountInCurrency.HasValue && trans.AmountInCurrency.Value != 0 ? trans.AmountInCurrency.Value : trans.Amount;
                var generatedId = BankConnectionUtil.GenerateId(trans.Date, amount, "");
                if (existingIdentifierCounter.ContainsKey(generatedId))
                {
                    // Update a transaction if necessary. This is done because external banks can update their transaction texts later on.
                    bool isConsideredDuplicate = existingIdentifierCounter[generatedId].Any(oldTrans => CheckDuplicateRules(trans, oldTrans));
                    if (isConsideredDuplicate)
                    {
                        foreach (var duplicateTrans in existingIdentifierCounter[generatedId])
                        {
                            if (trans.Text.Length > duplicateTrans.Text.Length && trans.Text.StartsWith(duplicateTrans.Text))
                            {
                                duplicateTrans.Text = trans.Text;
                                transactionsToUpdate.Add(duplicateTrans);
                            }
                        }
                        continue;
                    }
                }
                transactionsToAdd.Add(trans);
            }
            #endregion

            #region Check for deleted transactions

            //extract remaining transactions
            var remainingTransactions = new List<BankTransaction>();
            foreach (var entry in existingIdentifierCounter)
            {
                remainingTransactions.AddRange(existingIdentifierCounter[entry.Key]);
            }

            //check for deleted transactions
            foreach (var unpairedTrans in remainingTransactions)
            {
                if (_cleanupMode || unpairedTrans.IsUncleared)
                {
                    transactionsToDelete.Add(unpairedTrans);
                }
            }
            #endregion


            return new DuplicateResolverResults
            {
                TransactionsToAdd = transactionsToAdd,
                TransactionsToUpdate = transactionsToUpdate,
                TransactionsToDelete = transactionsToDelete
            };
        }

        protected virtual bool CheckDuplicateRules(BankTransaction newTrans, BankTransaction oldTrans)
        {
            bool duplicated = false;
            int distance = 0;

            duplicated = CheckUsingTransactionTexts(newTrans.Text, oldTrans.Text);

            if (duplicated)
            {
                return duplicated;
            }

            duplicated = CheckUsingCleanTransactionTexts(newTrans.Text, oldTrans.Text);

            if (duplicated)
            {
                return duplicated;
            }

            duplicated = CheckUsingLevenstheinDistance(newTrans.Text, oldTrans.Text, out distance);

            if (duplicated)
            {
                return duplicated;
            }

            return duplicated;
        }

        private bool CheckUsingLevenstheinDistance(string newText, string oldText, out int distance)
        {
            distance = Levenshtein(newText, oldText);
            return distance <= 6;
        }

        private bool CheckUsingCleanTransactionTexts(string newText, string oldText)
        {
            return CleanTransactionTexts(newText).Equals(CleanTransactionTexts(oldText));
        }

        private bool CheckUsingTransactionTexts(string newText, string oldText)
        {
            return newText.Equals(oldText);
        }

        private string CleanTransactionTexts(string text)
        {
            // Remove all whitespace characters and point character
            return Regex.Replace(text, @"[\s\.]+", "");
        }

        private static int Levenshtein(string s, string t)
        {
            if (s == t) return 0;
            if (s.Length == 0) return t.Length;
            if (t.Length == 0) return s.Length;
            var tLength = t.Length;
            var columns = tLength + 1;
            var v0 = new int[columns];
            var v1 = new int[columns];
            for (var i = 0; i < columns; i++)
                v0[i] = i;
            for (var i = 0; i < s.Length; i++)
            {
                v1[0] = i + 1;
                for (var j = 0; j < tLength; j++)
                {
                    var cost = (s[i] == t[j]) ? 0 : 1;
                    v1[j + 1] = Math.Min(Math.Min(v1[+j] + 1, v0[j + 1] + 1), v0[j] + cost);
                    v0[j] = v1[j];
                }
                v0[tLength] = v1[tLength];
            }
            return v1[tLength];
        }
        #endregion

        private string ParseDataPipes(string data)
        {
            string[] parts = data.Split('|');
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < parts.Length; i++)
            {
                sb.AppendFormat("\"{1}\",", i + 1, parts[i]);
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");

            return sb.ToString();
        }
    }
}
