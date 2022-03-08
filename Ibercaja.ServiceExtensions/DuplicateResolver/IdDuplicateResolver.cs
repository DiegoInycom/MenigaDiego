using System;
using System.Collections.Generic;
using System.Linq;
using Meniga.Core.BusinessModels;
using Meniga.Core.Data.User;
using Meniga.Core.Transactions;
using Meniga.Core.TransactionsEngine;

namespace Ibercaja.ServiceExtensions.DuplicateResolver
{
    public class IdDuplicateResolver : IBankTransactionDuplicateResolver<ICoreUserContext>
    {

        public DuplicateResolverResults ResolveDuplicates(ICoreUserContext context,
                                                          IEnumerable<BankTransaction> transList, long accountId,
                                                          DateTime? minFromDate, DateTime? maxToDate)
        {
            if (minFromDate == null)
            {
                minFromDate = transList.Min(t => t.Date).AddDays(-1);
            }

            List<Meniga.Core.Data.User.Transaction> transactions;
            if (maxToDate.HasValue)
            {
                transactions = (from t in context.Transactions
                                where (t.AccountId == accountId && t.TransactionDate >= minFromDate && t.TransactionDate <= maxToDate)
                                select t).ToList();
            }
            else
            {
                transactions = (from t in context.Transactions
                                where (t.AccountId == accountId && t.TransactionDate >= minFromDate)
                                select t).ToList();

            }
            var existingTransList = new List<BankTransaction>();
            if (transactions.Count > 0)
            {
                var parentTransToTransLookup = transactions.ToLookup(t => t.ParentIdentifier);
                foreach (var parentToTrans in parentTransToTransLookup)
                {
                    existingTransList.Add(new DatabaseBankTransaction(parentToTrans.First()));
                }
            }
            return ResolveDuplicates(transList, existingTransList);
        }


        public DuplicateResolverResults ResolveDuplicates(IEnumerable<BankTransaction> newTrans,
                                                          IEnumerable<BankTransaction> existingTrans)
        {
            var toAdd = new List<BankTransaction>();
            var toDelete = new List<BankTransaction>();
            var toUpdate = new List<BankTransaction>();

            existingTrans = existingTrans.ToList();
            newTrans = newTrans.ToList();

            var lookupByIdentifier = existingTrans.ToLookup(t => t.Identifier);
            var foundIds = new HashSet<string>();

            foreach (var trans in newTrans)
            {
                if (string.IsNullOrEmpty(trans.Identifier))
                {
                    toAdd.Add(trans);
                }
                else
                {
                    var transWithSameId = lookupByIdentifier[trans.Identifier].ToList();
                    if (transWithSameId.Count == 0)
                        toAdd.Add(trans);
                    else
                    {
                        foundIds.Add(trans.Identifier);
                        var first = (DatabaseBankTransaction) transWithSameId[0];
                        if (!IsSame(trans, first))
                            toUpdate.Add(first);
                    }
                }
            }

            // Deleting existing transactions that are Uncleared and are not coming in
            toDelete.AddRange(
                existingTrans.Where(t => !string.IsNullOrEmpty(t.Identifier) && !foundIds.Contains(t.Identifier) && t.IsUncleared == true));

            return new DuplicateResolverResults
                {
                    TransactionsToAdd = toAdd,
                    TransactionsToDelete = toDelete,
                    TransactionsToUpdate = toUpdate
                };
        }

        private static bool IsSame(BankTransaction trans, DatabaseBankTransaction existing)
        {
            bool isSame = true;
            if (trans.Amount != existing.Amount)
            {
                existing.Amount = trans.Amount;
                isSame = false;
            }
            if (trans.AmountInCurrency != existing.AmountInCurrency)
            {
                existing.AmountInCurrency = trans.AmountInCurrency;
                isSame = false;
            }
            if (trans.Currency != existing.Currency)
            {
                existing.Currency = trans.Currency;
                isSame = false;
            }
            if (trans.CounterpartyAccountId != existing.CounterpartyAccountId)
            {
                existing.CounterpartyAccountId = trans.CounterpartyAccountId;
                isSame = false;
            }
            if (trans.Date != existing.Date)
            {
                existing.Date = trans.Date;
                isSame = false;
            }
            if (trans.IsOwnAccountTransfer != existing.IsOwnAccountTransfer)
            {
                existing.IsOwnAccountTransfer = trans.IsOwnAccountTransfer;
                isSame = false;
            }
            if (trans.Mcc != existing.Mcc)
            {
                existing.Mcc = trans.Mcc;
                isSame = false;
            }
            if (trans.IsUncleared != existing.IsUncleared)
            {
                existing.IsUncleared = trans.IsUncleared;
                isSame = false;
            }
            if (!CompareDescriptions(trans.Text, existing.Text))
            {
                existing.Text = trans.Text;
                isSame = false;
            }
            if (!CompareDescriptions(trans.Data, existing.Data))
            {
                existing.Data = trans.Data;
                isSame = false;
            }
            return isSame;
        }

        private static bool CompareDescriptions(string text1, string text2)
        {
            text1 = string.IsNullOrWhiteSpace(text1) ? null : text1.Trim();
            text2 = string.IsNullOrWhiteSpace(text2) ? null : text2.Trim();
            return text1 == text2;
        }

    }
}