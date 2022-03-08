using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Meniga.Core.BusinessModels;
using Meniga.Core.BankConnections;
using Meniga.Runtime.Utils;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using Meniga.Runtime.Cache;
using Meniga.Runtime.IOC;
using Meniga.Runtime.Configuration;

namespace Ibercaja.Aggregation.DuplicateResolver
{
    public class DateAmountTextCustomDataDuplicateResolver : BankTransactionDuplicateResolverBase
    {
        private static bool _cleanupMode = IoC.Resolve<IGlobalApplicationParameterCache>().GetOrCreateParameter("DuplicateResolverCleanupMode", () => false);
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DateAmountTextCustomDataDuplicateResolver));

        public DateAmountTextCustomDataDuplicateResolver() : this(false)
        {
        }

        public DateAmountTextCustomDataDuplicateResolver(bool cleanupMode)
        {
            _cleanupMode |= cleanupMode;
        }

        public void ReloadCleanupModeFromSettings()
        {
            _cleanupMode = IoC.Resolve<IGlobalApplicationParameterCache>().GetOrCreateParameter("DuplicateResolverCleanupMode", () => false);
        }

        #region Generation Id Methods
        private static string GenerateId(BankTransaction transaction)
        {
            var amount = transaction.AmountInCurrency.HasValue && transaction.AmountInCurrency.Value != 0
                                        ? transaction.AmountInCurrency.Value : transaction.Amount;

            var trx = JArray.Parse(transaction.Data);
            DateTime valueDate;
            DateTime operationDate;
            if (trx.Count > 5 &&
                DateTime.TryParseExact(trx[5].ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out valueDate) &&
                DateTime.TryParseExact(trx[4].ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out operationDate))
            {
                return $"{valueDate.ToString("yyyyMMdd")}|{operationDate.ToString("yyyyMMdd")}|{amount.ToString("0.##")}";
            }

            return BankConnectionUtil.GenerateId(transaction.Date, amount, string.Empty);
        }

        #endregion

        #region IBankTransactionDuplicateResolver Members

        public override DuplicateResolverResults ResolveDuplicates(IEnumerable<BankTransaction> transList, IEnumerable<BankTransaction> existingTransList)
        {
            #region Generate identifier for existing transactions
            var transactionsToDelete = new List<BankTransaction>();
            var existingIdentifierCounter = new Dictionary<string, IList<BankTransaction>>();
            foreach (var existingTrans in existingTransList) //create a 'hash' with linked lists for duplicate entries
            {
                // check if is uncleared and delete it 
                try
                {
                    if (existingTrans.IsUncleared)
                    {
                        transactionsToDelete.Add(existingTrans);
                    }
                    else
                    {
                        var id = GenerateId(existingTrans);
                        //There is an existing transaction with the same identifier, add to duplicates counter
                        if (existingIdentifierCounter.ContainsKey(id))
                        {
                            existingIdentifierCounter[id].Add(existingTrans);
                        }
                        else
                        {
                            existingIdentifierCounter[id] = new List<BankTransaction> { existingTrans };
                        }
                    }
                }
                catch
                {
                    Logger.Error($"Can't detect duplicates on transaction {existingTrans.Identifier} due to incorrect JSON format in Data value: {existingTrans.Data}");
                }

            }
            #endregion

            #region Compare newly fetched transactions to existing transactions
            var transactionsToAdd = new List<BankTransaction>();
            var transactionsToUpdate = new List<BankTransaction>();
            foreach (var trans in transList) //for each transaction coming from Eurobits
            {
                var id = GenerateId(trans);

                if (existingIdentifierCounter.ContainsKey(id))
                {
                    // Update a transaction if necessary. This is done because external banks can update their transaction texts later on.
                    bool isConsideredDuplicate = existingIdentifierCounter[id].Any(oldTrans => CheckDuplicateRules(trans, oldTrans));
                    if (isConsideredDuplicate)
                    {
                        foreach (var duplicateTrans in existingIdentifierCounter[id])
                        {
                            if (CheckDuplicateRules(trans, duplicateTrans))
                            {
                                duplicateTrans.AccountBalance = trans.AccountBalance;
                                duplicateTrans.Text = trans.Text;
                                duplicateTrans.Data = trans.Data;
                                duplicateTrans.IsMerchant = trans.IsMerchant;
                                transactionsToUpdate.Add(duplicateTrans);
                                existingIdentifierCounter[id].Remove(duplicateTrans);
                                break;
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
                if (_cleanupMode)
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

        #endregion

        #region Check Duplicate Rules

        protected virtual bool CheckDuplicateRules(BankTransaction newTrans, BankTransaction oldTrans)
        {
            bool duplicated = false;
            int distance = 0;

            var newTransData = JArray.Parse(newTrans.Data);
            var oldTransData = JArray.Parse(oldTrans.Data);

            string newTransOriginalText = newTransData[0].ToString();
            string oldTransOriginalText = oldTransData[0].ToString();

            if (oldTrans.AccountBalance.HasValue && newTrans.AccountBalance.HasValue && newTrans.AccountBalance.Value != oldTrans.AccountBalance.Value)
            {
                return duplicated;
            }

            return CheckUsingTransactionTexts(newTransOriginalText, oldTransOriginalText)
                || CheckUsingCleanTransactionTexts(newTransOriginalText, oldTransOriginalText)
                || CheckUsingLevenstheinDistance(newTransOriginalText, oldTransOriginalText, out distance);
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
    }
}
