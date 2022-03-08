using System.Collections.Generic;
using System.Linq;
using System.Net;
using CoreTestBase.Common;
using CoreTestBase.Models;
using NUnit.Framework;
using RestSharp;

namespace Ibercaja.IntegrationTests
{
    [TestFixture]
    public class AccountAggregationTest : UserBaseNonDemo
    {
        private string _realmIdentifier;

        /// <summary>
        /// Before run test, in AdminWeb > Support > "Create a new user" with email 'test@meniga.com' and password '123456'
        /// This data can be modified in 'settings.txt' of the folder of this project
        /// NOTE: Execute the commands 'psake rebuilddb' and  'psake initialsetup' before run test
        /// </summary>
        [TestCase("abanca", 2, 1)]
        [TestCase("liberbank", 6, 0)]
        [TestCase("bancsabadell", 7, 2)]
        [TestCase("bankinter", 6, 0)]
        [TestCase("bankia", 17, 39)]
        [TestCase("ingdirect", 10, 11)]
        [TestCase("santander", 4, 18)]
        [TestCase("unicaja", 3, 2)]
        [TestCase("cajalaboral", 5, 2)]
        [TestCase("imaginbank", 1, 4)]
        [TestCase("bbva", 17, 15)]
        [TestCase("caixabank", 5, 2)]
        [TestCase("kutxabank", 4, 2)]
        [TestCase("ruralvia", 2, 0)]

        public void AccountAggregation(string realmIdentifier, int accountsCount, int transactionsCount)
        {
            // Get RealmId By Organizations Endpoint
            _realmIdentifier = realmIdentifier;
            var realmId = GetRealmIdByOrganizations();
            
            // Post Realm Authenticate and check the parameter 'authenticationDone'
            PostAuthByRealmId(realmId);

            // Create or authenticate realmUser by realmId and check the parameter 'authenticationDone'

            var sessionTokenAndRealmUserId = PostUserAuthByRealmId(realmId);
            var sessionToken = sessionTokenAndRealmUserId.sessionToken;
            var realmUserId = sessionTokenAndRealmUserId.realmUserId;

            //First GET accounts and then POST accounts
            GetPostAccountsSyncWithRealmUser(realmUserId, sessionToken, accountsCount);

            // Post synchronize transactions
            PostSyncTransactionsAndFinish(realmUserId, transactionsCount);
        }

        /// <summary>
        /// GET  | user/v1/sync/accounts/{realmUserId} | Get the accounts with realmUserId and sessionTokenFaile
        /// POST | user/v1/sync/accounts/{realmUserId}/authorize | Insert accounts
        /// GET  | user/v1/accounts | Get accounts by realm identifier
        /// </summary>
        /// <param name="realmUserId"></param>
        /// <param name="sessionToken"></param>
        private void GetPostAccountsSyncWithRealmUser(int realmUserId, string sessionToken, int accountsCount)
        {
            //Firstly, GET accounts of realmUser and sessionToken
            var responseAccountsRealm =
                this.SendRequest(
                    this.syncAccountsRealmByIdEndpoint,
                    Method.GET,
                    true,
                    new Dictionary<string, object> { ["realmUserId"] = realmUserId },
                    null,
                    new Dictionary<string, object> { ["sessionToken"] = sessionToken });

            Assert.AreEqual(HttpStatusCode.OK, responseAccountsRealm.StatusCode, "Wrong response code for get accounts. " + responseAccountsRealm.Content);
            var accountModelList = DeserializeResponse<List<AccountModel>>(responseAccountsRealm);
            Assert.IsNotNull(accountModelList, "Failed to deserialize response content for GET /accounts");

            //Secondly, POST this accounts with realmUser and sessionToken
            var responsePostAuthAccounts = 
                this.SendRequest(
                    this.syncAuthAccountsByEndpoint,
                    Method.POST,
                    true,
                    new Dictionary<string, object> { { "realmUserId", realmUserId } },
                    accountModelList, new Dictionary<string, object> { { "sessionToken", sessionToken } });

            Assert.AreNotEqual(HttpStatusCode.BadRequest, responsePostAuthAccounts.StatusCode, "[BadRequest] Wrong response code for start sync of GET accounts. " + responsePostAuthAccounts.Content);
            Assert.AreNotEqual(HttpStatusCode.InternalServerError, responsePostAuthAccounts.StatusCode, "[InternalServerError] Wrong response code for start sync of GET accounts. " + responsePostAuthAccounts.Content);

            // Verify that the list of accounts matches the accounts of the 'generic endpoint'
            var responseAccounts = this.SendRequest(this.accountsEndpoint, Method.GET, true, null, null,
                                                    new Dictionary<string, object> { { "realmIdentifier", _realmIdentifier } });
            Assert.AreEqual(HttpStatusCode.OK, responseAccounts.StatusCode, "Wrong response code for get accounts. " + responseAccounts.Content);
            var accountsList = DeserializeResponse<List<AccountModel>>(responseAccounts);
            Assert.IsNotNull(accountsList, "Failed to deserialize response content for GET /accounts");
            Assert.IsNotEmpty(accountsList, "No accounts retrieved");
            Assert.AreEqual(accountModelList.Count, accountsList.Count, "The list accounts of " + _realmIdentifier + " don't match with which we return in the synchronized call the endpoint 'user/v1/sync/accounts/{realmUserId}'");

            //Verify accounts haven't got metadata
            foreach (var account in accountModelList)
            {
                Assert.IsTrue(!account.Metadata.Any(), "The accounts have metadata, and they shouldn't have it.");
            }

            //Check accounts of test case:
            Assert.AreEqual(accountsCount, accountModelList.Count, "The accounts don't match with the second parameter of test case. Expected: " + accountsCount + " Actual: " + accountModelList.Count);
        }

        /// <summary>
        /// POST | user/v1/sync/realm/{realmUserId} | Synchronize transactions
        /// GET | user/v1/transactions | Get transactions with accountIds
        /// </summary>
        public void PostSyncTransactionsAndFinish(int realmUserId, int transactionsCount)
        {
           var responsePostSyncTransactions = 
                this.SendRequest(
                    this.syncRealmByIdEndpoint,
                    Method.POST,
                    true,
                    new Dictionary<string, object> { { "realmUserId", realmUserId } },
                    new Dictionary<string, object>
                    {
                        ["waitForCompleteMilliseconds"] = 1000
                    }, null);

            Assert.AreEqual(HttpStatusCode.Created, responsePostSyncTransactions.StatusCode, "Wrong response code for start sync transactions. " + responsePostSyncTransactions.Content);
            var synchronizationStatusTrans = DeserializeResponse<SynchronizationStatus>(responsePostSyncTransactions);
            Assert.IsNotNull(synchronizationStatusTrans, "Failed to deserialize response for POST /sync");

            // Loop until synchronization is finished
            while (!synchronizationStatusTrans.IsSyncDone)
            {
                var synchronizatonStatusById = this.SendRequest(
                    this.syncByIdEndpoint,
                    Method.GET,
                    true,
                    new Dictionary<string, object> { { "id", synchronizationStatusTrans.SyncHistoryId } },
                    null,
                    null);
                synchronizationStatusTrans = DeserializeResponse<SynchronizationStatus>(synchronizatonStatusById);
                Assert.IsNotNull(synchronizationStatusTrans, "Failed to deserialize response for GET /sync/{id}");
            }

            //Call to GET /accounts
            var responseAccounts = this.SendRequest(this.accountsEndpoint, Method.GET, true, null, null,
                                        new Dictionary<string, object> { { "realmIdentifier", _realmIdentifier } });
            Assert.AreEqual(HttpStatusCode.OK, responseAccounts.StatusCode, "Wrong response code for get accounts. " + responseAccounts.Content);
            var accountsList = DeserializeResponse<List<AccountModel>>(responseAccounts);

            //Verify the accounts have got metadata
            foreach (var account in accountsList)
            {
                Assert.IsTrue(account.Metadata.Any(), "The accounts haven't metadata and they should have it.");
            }
            
            // Accounts synchronized
            var listAccounts = synchronizationStatusTrans.RealmSyncResponses.FirstOrDefault().AccountSyncStatuses.ToList();

            //Convert list in string separate with commas
            var accountIds = string.Join(",", listAccounts.Select(a => a.AccountId.ToString()));

            // Get the transactions with accountIds
            var responseGetTransactions = 
                this.SendRequest(
                     this.transactionsEndpoint,
                     Method.GET,
                     true,
                     null,
                     null,
                     new Dictionary<string, object>
                     {
                         ["accountIds"] = accountIds
                     });

            Assert.AreEqual(HttpStatusCode.OK, responseGetTransactions.StatusCode, "Wrong response code for get transactions. " + responseGetTransactions.Content);
            var transactionModelList = DeserializeResponse<List<TransactionModel>>(responseGetTransactions);
            Assert.IsNotNull(transactionModelList, "Failed to deserialize response for GET /transactions");

            //Check transactions of test case
            Assert.AreEqual(transactionsCount, transactionModelList.Count, "The transactions don't match with the third parameter of test case. Expected: " + transactionsCount + " Actual: " + transactionModelList.Count);

            //Delete the realm user and the connected accounts (user/v1/me/realmusers/{id})
            var responseDeleteRealmUser = this.SendRequest(this.usersRealmsUsersByIdEndpoint,
                                Method.DELETE, 
                                true,
                                null,
                                null,
                                new Dictionary<string, object>
                                {
                                    ["id"] = realmUserId
                                }, 
                                null, null);
            Assert.AreNotEqual(HttpStatusCode.BadRequest, responseDeleteRealmUser.StatusCode, "[BadRequest] Wrong response code for delete user's realm credentials. " + responseDeleteRealmUser.Content);
            Assert.AreNotEqual(HttpStatusCode.InternalServerError, responseDeleteRealmUser.StatusCode, "[InternalServerError] Wrong response code for delete user's realm credentials. " + responseDeleteRealmUser.Content);
            Assert.AreEqual(HttpStatusCode.NoContent, responseDeleteRealmUser.StatusCode, "Wrong response code for delete user's realm credentials. " + responseDeleteRealmUser.Content);
        }

        /// <summary>
        /// Get endpoint /organizations and filter for 'realmIdentifier'
        /// </summary>
        /// <returns>The realmId</returns>
        private int GetRealmIdByOrganizations()
        {
            //Get the organization
            var responseOrganization = this.SendRequest(this.organizationsEndpoint, Method.GET, true, null, null, null);

            Assert.AreEqual(HttpStatusCode.OK, responseOrganization.StatusCode, "Wrong response code for get organizations. " + responseOrganization.Content);
            var organizationModelList = DeserializeResponse<List<OrganizationModel>>(responseOrganization);
            Assert.IsNotNull(organizationModelList, "Failed to deserialize response for GET /organizations");

            //Get the realm with organization
            var organization = organizationModelList.FirstOrDefault(o => o.Identifier.Equals(_realmIdentifier));
            var realmId = organization.Realms.FirstOrDefault(r => r.Identifier.Equals(_realmIdentifier)
                                                  && r.AuthorizationType.Equals(AccountAuthorizationTypeEnum.Internal)).Id;

            return realmId;
        }

        /// <summary>
        /// GET | user/v1/sync/{id} | Check of synchronize status to verify that the process was completed successfully
        /// </summary>
        /// <param name="syncHistoryId">syncHistoryId</param>
        public void CheckGetSyncCompleted(long syncHistoryId)
        {
            var responseGetSync = this.SendRequest(
                        this.syncByIdEndpoint,
                        Method.GET,
                        true,
                        new Dictionary<string, object>
                        {
                            ["id"] = syncHistoryId
                        },
                        null, null);

            Assert.AreEqual(HttpStatusCode.OK, responseGetSync.StatusCode, "Wrong response code for get sync. " + responseGetSync.Content);
            var synchronizationStatusSync = DeserializeResponse<SynchronizationStatus>(responseGetSync);
            Assert.IsNotNull(synchronizationStatusSync, "Failed to deserialize response for GET /sync/{id}");
            Assert.IsTrue(synchronizationStatusSync.IsSyncDone, "The parameter 'IsSyncDone' is false, the synchronization not completed or there has been an error with realmUser, surely it is not configured as 'Fake'.");
        }

        /// <summary>
        /// POST | user/v1/sync/realm/{realm}/auth | Authenticate Realm by realmId
        /// </summary>
        /// <param name="realmId"></param>
        private void PostAuthByRealmId(int realmId)
        {
            var responseAuth = this.SendRequest(
                        this.syncAuthRealmByIdEndpoint,
                        Method.POST,
                        true,
                        new Dictionary<string, object> { { "realmId", realmId } },
                        new Dictionary<string, object>(), null);

            Assert.AreEqual(HttpStatusCode.OK, responseAuth.StatusCode, "Wrong response code for first POST of user. " + responseAuth.Content);
            var listAuth = DeserializeResponse<Dictionary<string, object>>(responseAuth);
            Assert.IsNotNull(listAuth, "Failed to deserialize response content for first POST of user.");

            //Get the parameter authenticationDone
            var authenticationDone = (bool)listAuth["authenticationDone"];

            //Check with authenticationDone is false, else run error
            Assert.IsFalse(authenticationDone, "Error, I shouldn't authenticate, because I haven't got 'requiredParameters'");
        }

        /// <summary>
        /// POST | user/v1/sync/realm/{realm}/auth | Create/Authenticate realmUser and check the parameter 'authenticationDone' 
        /// </summary>
        /// <param name="realmId"></param>
        /// <returns>2 variables in dynamic object: sessionToken and realmUserId</returns>
        private dynamic PostUserAuthByRealmId(int realmId)
        {
            var isINGAuthentication = _realmIdentifier.Equals("ingdirect");

            var responseAuthWithParameters = PostUserApi("user123", "pass123", realmId, isINGAuthentication);

            Assert.AreEqual(HttpStatusCode.OK, responseAuthWithParameters.StatusCode, "Wrong response code for start sync of call 'sync/realm/{realm}/auth'. " + responseAuthWithParameters.Content);
            var synchronizationStatus = DeserializeResponse<SynchronizationStatus>(responseAuthWithParameters);
            Assert.IsNotNull(synchronizationStatus, "Failed to deserialize response for POST /sync");

            var listAuthParameters = DeserializeResponse<Dictionary<string, object>>(responseAuthWithParameters);

            //Get of sessionToken of authentication for next response
            var sessionToken = listAuthParameters["sessionToken"].ToString();

            //Check with authenticationDone is true, else run error
            var authenticationDoneParameter = (bool)listAuthParameters["authenticationDone"];

            //Get the 'realmUserId' of the authentication response
            var realmUserId = int.Parse(listAuthParameters["realmUserId"].ToString());

            Assert.IsTrue(authenticationDoneParameter, "Error, fail the parameters. Username/Password or SessionToken.");

            return new { sessionToken = sessionToken, realmUserId = realmUserId};
        }

        /// <summary>
        /// POST | user/v1/sync/realm/{realm}/auth
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="realmId"></param>
        /// <returns>IRestResponse para objeto respuesta</returns>
        private IRestResponse PostUserApi(string user, string pass, int realmId, bool isINGDirect)
        {
            IRestResponse responseAuthenticate = new RestResponse();
            if (!isINGDirect)
            {
                responseAuthenticate = this.SendRequest(
                      this.syncAuthRealmByIdEndpoint,
                      Method.POST,
                      true,
                      new Dictionary<string, object> { { "realmId", realmId } },
                      new Dictionary<string, object>
                      {
                          ["parameters"] = new List<Dictionary<string, object>>
                          {
                              new Dictionary<string, object>
                              {
                                ["name"] = "user",
                                ["value"] = user
                              },
                              new Dictionary<string, object>
                              {
                                ["name"] = "password",
                                ["value"] = pass
                              }
                          },
                          ["saveDetails"] = true,
                          ["realmUserIdentifier"] = user
                      }, null);
            }

            if(isINGDirect)
            {
                responseAuthenticate = this.SendRequest(
                      this.syncAuthRealmByIdEndpoint,
                      Method.POST,
                      true,
                      new Dictionary<string, object> { { "realmId", realmId } },
                      new Dictionary<string, object>
                      {
                          ["parameters"] = new List<Dictionary<string, object>>
                          {
                              new Dictionary<string, object>
                                  {
                                    ["name"] = "id",
                                    ["value"] = user
                                  },
                                  new Dictionary<string, object>
                                  {
                                    ["name"] = "birthDay",
                                    ["value"] = 01
                                  },
                                  new Dictionary<string, object>
                                  {
                                    ["name"] = "birthMonth",
                                    ["value"] = 02
                                  },
                                  new Dictionary<string, object>
                                  {
                                    ["name"] = "birthYear",
                                    ["value"] = 2019
                                  },
                                  new Dictionary<string, object>
                                  {
                                    ["name"] = "password",
                                    ["value"] = pass
                                  }
                          },
                          ["saveDetails"] = true,
                          ["realmUserIdentifier"] = user
                      }, null);
            }

            return responseAuthenticate;
        }
    }
}