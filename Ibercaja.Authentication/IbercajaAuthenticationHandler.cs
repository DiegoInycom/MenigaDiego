using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ibercaja.Utils.Config;
using log4net;
using Meniga.Core.Accounts;
using Meniga.Core.Authentication;
using Meniga.Core.Batch;
using Meniga.Core.BusinessModels;
using Meniga.Core.Data;
using Meniga.Core.Transactions;
using Meniga.Core.TransactionsEngine;
using Meniga.Core.Users;
using Meniga.Runtime.AuditTrail;
using Account = Meniga.Core.Data.User.Account;

namespace Ibercaja.Authentication
{
    public class IbercajaAuthenticationHandler : IAuthenticationHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICoreContextProvider _dataContextProvider;
        private readonly IAccountSetupCache _accountSetupCache;
        private readonly IAccountsManager _accountsManager;
        private readonly IBatchManager _batchManager;
        private readonly IUserManager _userManager;


        public IbercajaAuthenticationHandler(ICoreContextProvider dataContextProvider, IAccountSetupCache accSetupCache,
            IAccountsManager accountsManager, IBatchManager batchManager, IUserManager userManager)
        {
            _dataContextProvider = dataContextProvider;
            _accountSetupCache = accSetupCache;
            _accountsManager = accountsManager;
            _userManager = userManager;
            _batchManager = batchManager;
        }


        public void OnAfterAuthentication(string userIdentifier, string realmIdentifier, long userId, long personId, IDictionary<string, string> parameters)
        {
            try
            {
                //if(userIdentifier.Contains("@")) // TODO: Temporary code, only used for testing with MenigaWeb
                //{
                //    userIdentifier = userIdentifier.Substring(0, userIdentifier.IndexOf("@"));
                //}

                if (realmIdentifier.Equals("default") || realmIdentifier == "") // TODO: Temporary code, only used for testing with MenigaWeb and for Real
                {
                    realmIdentifier = "Ibercaja";
                }

                var realm = _accountSetupCache.GetRealm(realmIdentifier);
                if (realm == null)
                {
                    _logger.ErrorFormat("Realm with realmIdentifier {0} was not found", realmIdentifier);
                    return;
                }

                var realmUser = _userManager.GetRealmUsers(personId).FirstOrDefault(u => u.RealmId == realm.Id);
                if (realmUser == null)
                {
                    _logger.ErrorFormat("RealmUser was not found for user with personId {0}", personId);
                    return;
                }

                if (string.IsNullOrWhiteSpace(userIdentifier))
                {
                    userIdentifier = realmUser.UserIdentifier;
                }

                var allRetrievedAccounts = RetrieveBankAccounts(userIdentifier);
                bool userNeedsSynchronization = false;

                foreach (var retrievedAccountIdentifier in allRetrievedAccounts)
                {
                    _logger.DebugFormat("About to process account with identifier {0}", retrievedAccountIdentifier);

                    // Does the account exist in any of the PFM user databases, if so then we can clone it
                    var existingAccounts = new List<Account>();
                    foreach (var ctx in _dataContextProvider.AllUserContextInstances(false))
                    {
                        var acc = ctx.Accounts.Where(a => a.Identifier == retrievedAccountIdentifier).ToList();
                        existingAccounts.AddRange(acc);
                    }

                    if (existingAccounts.Count > 0)
                    {
                        var accountInMenigaDatabase = existingAccounts[0];
                        var accountType = _accountSetupCache.GetAccountType(accountInMenigaDatabase.AccountTypeId);

                        var bankAccountInfo = new BankAccountInfo
                        {
                            AccountIdentifier = accountInMenigaDatabase.Identifier,
                            AccountCategory = accountType.AccountCategory,
                            AccountCategoryDetails = accountType.AccountCategoryDetails,
                            Name = accountInMenigaDatabase.Name,
                            CurrencyCode = null
                        };

                        // Add single user to the account (along with keeping existing users)
                        var usersWithNewClonedAccounts = _accountsManager.UpdateUserAccountsRelations(realm.Id, bankAccountInfo, new[] { userIdentifier }, DateTime.Now, null, null, null, null, false);
                        if (usersWithNewClonedAccounts.Length > 0)
                        {
                            userNeedsSynchronization = true;
                        }
                    }
                }

                var userAccountsToDisable = _accountsManager.GetAccountsForUser(userIdentifier, realm).ToList();
                userAccountsToDisable.RemoveAll(a => allRetrievedAccounts.Contains(a.AccountIdentifier));

                if (userAccountsToDisable.Count > 0)
                {
                    foreach (var account in userAccountsToDisable)
                    {
                        _accountsManager.DisableAccountForUser(account.Id, userId);
                        _logger.InfoFormat("Removed user with id {0} from account with accountIdentifier {1}", userId, account.AccountIdentifier);
                    }
                }

                if (userNeedsSynchronization)
                {
                    _logger.InfoFormat("Starting synchronization for user with UserId[{0}], UserIdentifier [{1}]", userId, userIdentifier);
                    _batchManager.StartBatchSynchronization(false, null, new[] { userId });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("OnAfterauthentication failed for user with userIdentifier {0}, error is {1} ", userIdentifier, ex.Message), ex);
            }
        }

        private List<string> RetrieveBankAccounts(string userIdentifier)
        {
            var accountIdentifiers = new List<string>();

            string connectionString = ConfigurationManager.ConnectionStrings["MenigaBatchEntities"].ConnectionString;
            string query = string.Format("select account_identifier from batch.ibercaja_user_account_relations where user_identifier = '{0}'", userIdentifier);
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        accountIdentifiers.Add(reader[0].ToString());
                    }
                }
            }

            return accountIdentifiers;
        }
    }
}
