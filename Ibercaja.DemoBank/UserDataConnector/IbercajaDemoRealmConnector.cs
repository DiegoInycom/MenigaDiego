using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using Meniga.Core.Accounts;
using Meniga.Core.BusinessModels;
using Meniga.Core.DataConsolidation;
using Meniga.Pfm;
using Meniga.Runtime.IOC;

namespace Ibercaja.DemoBank.UserDataConnector
{
    public class IbercajaDemoRealmConnector : IUserDataConnector
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IbercajaDemoRealmConnector));

        private bool randomizeAmounts;

        public IbercajaDemoRealmConnector() { }

        /// <param name="randomizeAmount">Should amount be randomized. If true, amount is changed to 90% - 110% of original.</param>
        public IbercajaDemoRealmConnector(bool randomizeAmount)
        {
            randomizeAmounts = randomizeAmount;
        }

        private string demoProfileName = "IbercajaDemoProfile";


        public BankAccountInfo[] GetAccountInfo(string userIdentifier)
        {
            Logger.Info("IbercajaDemoRealmConnector - GetAccountInfo");
            using (var context = IoC.Resolve<IPfmContextProvider>().AppContext())
            {
                var accCache = IoC.Resolve<IAccountSetupCache>();
                List<BankAccountInfo> accInfoList = new List<BankAccountInfo>();
                var demoAccounts = context.DemoBankAccounts.Where(a => a.DemoProfile.Name == demoProfileName);
                foreach (var demoAccount in demoAccounts)
                {
                    var accType = accCache.GetAccountType(demoAccount.AccountTypeId);
                    accInfoList.Add(new BankAccountInfo()
                    {
                        AccountCategory = accType.AccountCategory,
                        AccountCategoryDetails = accType.AccountCategoryDetails,
                        AccountIdentifier = demoAccount.Id.ToString(),
                        Balance = demoAccount.Balance,
                        Limit = demoAccount.Limit,
                        Name = demoAccount.Description
                    });
                }
                return accInfoList.ToArray();
            }
        }

        public AccountStatement GetAccountStatement(AccountCategoryEnum accountCategory, string accountCategoryDetail, string accountId, DateTime from, DateTime to)
        {
            Logger.Info("IbercajaDemoRealmConnector - GetAccountStatement");
            using (var context = IoC.Resolve<IPfmContextProvider>().AppContext())
            {
                int accountIdInt;
                if (!int.TryParse(accountId, out accountIdInt))
                {
                    if (accountId.Contains("-"))
                    {
                        int.TryParse(accountId.Split('-')[0], out accountIdInt);
                    }
                    else if (accountId.Length > 4)
                    {
                        int.TryParse(accountId.Substring(accountId.Length - 4), out accountIdInt);
                    }
                }

                var account = (from acc in context.DemoBankAccounts where (acc.Id == accountIdInt) select acc).FirstOrDefault();

                if (account == null)
                {
                    throw new Exception("Demo account with Id=" + accountId + " was not found");
                }

                var statement = new AccountStatement();
                statement.Balance = account.Balance;
                statement.Limit = account.Limit;
                statement.Transactions = new List<BankTransaction>();

                var transList = context.DemoBankTransactions
                    .Where(tr => tr.TransactionDate >= from && tr.TransactionDate < to &&
                                 tr.DemoBankAccountId == accountIdInt)
                    .OrderByDescending(tr => tr.Id).ToList();

                //from tr in context.DemoBankTransactions.
                //where (tr.DemoBankAccount.Id == accountIdInt && tr.TransactionDate >= from && tr.TransactionDate < to)
                //orderby tr.Id descending select tr;

                foreach (var trans in transList)
                {
                    if (randomizeAmounts)
                    {
                        trans.Amount = RandomizeAmount(accountId, trans.Amount);
                    }
                    statement.Transactions.Add(new BankTransaction
                    {
                        Identifier = trans.Id.ToString(),
                        Amount = trans.Amount,
                        Data = trans.CustomData,
                        Date = trans.TransactionDate,
                        Mcc = trans.Mcc,
                        Text = trans.TransactionText.Trim(),
                        IsMerchant = trans.Mcc != null
                    });
                }

                return statement;
            }
        }

        public AccountStatement GetAccountStatementIncremental(AccountCategoryEnum accountCategory, string accountCategoryDetail,
            string accountId, string syncToken)
        {
            throw new NotImplementedException();
        }

        public AccountStatement GetAccountStatementIntraday(AccountCategoryEnum accountCategory, string accountCategoryDetail,
            string accountId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Changes amount randomly to 90% - 110%
        /// </summary>
        private decimal RandomizeAmount(string accountId, decimal amount)
        {
            Random rand = new Random(accountId.GetHashCode());
            double f = rand.NextDouble();           // Returns a random number from 0.0 to 1.0
            f = 0.9 + f / 5;                        // Create a multiplication factor from 0.9 - 1.1
            return (decimal)(f * (double)amount); // Multibly with amount
        }
    }
}
