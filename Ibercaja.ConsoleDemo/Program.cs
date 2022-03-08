using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Net.Http;
using System.Threading;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Ibercaja.ConsoleDemo
{
    class Program
    {
        private const string LOCALHOST = "http://localhost:8081/";
        private const string IBERCAJA = "https://pfmapp.ibercajadirecto.com/";

        static void Main(string[] args)
        {

            // Select Environment
            var environment = GetEnvironment();

            // Select Realm
            var realm = GetRealm();

            // Set UserId
            var user = GetUserId();

            // Test ApiUserDataConnector in some environment
            TestEnvironment(environment, realm, user);

            Console.WriteLine();
            Console.WriteLine("Press to continue...");
            Console.ReadLine();
            Console.Clear();
        }

        private static string GetEnvironment()
        {
            Console.WriteLine($"1. Localhost ({LOCALHOST})");
            Console.WriteLine($"2. Ibercaja ({IBERCAJA})");
            Console.WriteLine("Select Environment:");
            var env = Console.ReadLine();
            Console.WriteLine();

            switch (env)
            {
                case "1":
                    return LOCALHOST;
                case "2":
                    return IBERCAJA;
                default:
                    Environment.Exit(-1);
                    return string.Empty;
            }
        }

        private static string GetRealm()
        {
            Console.WriteLine();
            Console.WriteLine(" 2. Ibercaja");
            Console.WriteLine(" 8. BBVA");
            Console.WriteLine(" 9. Caixabank");
            Console.WriteLine(" 10. Kutxabank");
            Console.WriteLine(" 11. Abanca");
            Console.WriteLine(" 12. Liberbank");
            Console.WriteLine(" 15. Caja Laboral");
            Console.WriteLine(" 17. Bankia");
            Console.WriteLine(" 21. Bankinter");
            Console.WriteLine(" 23. ING Direct");
            Console.WriteLine(" 32. Banc Sabadell");
            Console.WriteLine(" 33. Santander");
            Console.WriteLine(" 35. Unicaja");
            Console.WriteLine(" 42. Ruralvia");
            Console.WriteLine(" 62. ImaginBank");
            Console.WriteLine("Select Realm:");
            var realm = Console.ReadLine();
            Console.WriteLine();

            int result;
            if (int.TryParse(realm, out result))
            {
                var realms = new List<int> { 2, 8, 9, 10, 11, 12, 15, 17, 21, 23, 32, 33, 35, 42, 62 };
                if (realms.Contains(result))
                {
                    return realm;
                }
            }

            Environment.Exit(-1);
            return string.Empty;
        }

        private static string GetUserId()
        {
            Console.WriteLine("Write userId:");
            var user = Console.ReadLine();
            Console.WriteLine();

            return user;
        }

        private static Dictionary<string, string> GetLoginParameters(string realm)
        {
            var dict = new Dictionary<string, string>();

            if (realm == "23")
            {
                // ING Direct
                Console.WriteLine();
                Console.WriteLine("Enter id:");
                dict["id"] = Console.ReadLine();
                Console.WriteLine();

                Console.WriteLine();
                Console.WriteLine("Enter birthDay:");
                dict["birthDay"] = Console.ReadLine();
                Console.WriteLine();

                Console.WriteLine();
                Console.WriteLine("Enter birthMonth:");
                dict["birthMonth"] = Console.ReadLine();
                Console.WriteLine();

                Console.WriteLine();
                Console.WriteLine("Enter birthYear:");
                dict["birthYear"] = Console.ReadLine();
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Enter user:");
                dict["username"] = Console.ReadLine();
                Console.WriteLine();
            }

            dict["password"] = GetPassword();

            return dict;
        }

        private static string GetPassword()
        {
            Console.WriteLine();
            Console.Write("Enter password: ");

            SecureString password = new SecureString();
            while (true)
            {
                ConsoleKeyInfo userInput = Console.ReadKey(true);
                if (userInput.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (userInput.Key == ConsoleKey.Escape)
                {
                    return string.Empty;
                }
                else if (userInput.Key == ConsoleKey.Backspace)
                {
                    if (password.Length != 0)
                    {
                        password.RemoveAt(password.Length - 1);
                    }
                }
                else
                {
                    password.AppendChar(userInput.KeyChar);
                    Console.Write("*");
                }
            }

            Console.WriteLine();
            return new System.Net.NetworkCredential(string.Empty, password).Password;
        }

        private static void TestEnvironment(string baseAddress, string realm, string userId)
        {
            using (HttpClient _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(baseAddress);
                _client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
                _client.DefaultRequestHeaders.Accept.Clear();
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var uri = new Uri(_client.BaseAddress, $"user/v1/authentication/sso/BE-Ibercaja?securityToken={userId}");
                // string securityToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJtZW5pZ2FpZCI6InByaXZhdGVfODk5OV8xOTgwMDEwMjAzNDU3OCJ9.mZxC5b7mLE6x4KrzOT7SkZHB6sCPONTWUnBTnYEUcx8";
                // var uri = new Uri(_client.BaseAddress, $"user/v1/authentication/sso/BE-Ibercaja?securityToken={Uri.EscapeDataString(securityToken)}");

                var response = Task.Run(() => _client.PostAsJsonAsync(uri, string.Empty)).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var errorReponse = response.Content.ReadAsAsync<dynamic>().Result;
                    Console.WriteLine(JsonConvert.SerializeObject(errorReponse, Formatting.Indented));
                    return;
                }

                var token = response.Content.ReadAsAsync<AuthorizationResponse>().Result.data.accessToken;
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                Console.WriteLine($"Realm {realm}");
                Console.WriteLine("-------");
                Console.WriteLine();

                // Start authentication to a Realm https://docs.meniga.com/docs/core/api/user/account-aggregation.html#start-authentication-to-a-realm
                // Get a list of credentials needed to authenticate with empty POST body object
                uri = new Uri(_client.BaseAddress, $"user/v1/sync/realm/{realm}/auth");
                response = Task.Run(() => _client.PostAsJsonAsync(uri, new { })).Result;//Tendria que saltar a EurobitsUserDataConnector
                if (!response.IsSuccessStatusCode)
                {
                    var errorReponse = response.Content.ReadAsAsync<dynamic>().Result;
                    Console.WriteLine(JsonConvert.SerializeObject(errorReponse, Formatting.Indented));
                    return;
                }
                var authorizationParameters = response.Content.ReadAsAsync<AuthenticationResponse>().Result;
                Console.WriteLine("List of credentials needed to authenticate:");
                Console.WriteLine(JsonConvert.SerializeObject(authorizationParameters.data, Formatting.Indented));

                var loginParameters = GetLoginParameters(realm);

                // Login to the realm with the parameters values entered by the end user
                uri = new Uri(_client.BaseAddress, $"user/v1/sync/realm/{realm}/auth");
                AuthorizationRequest credentials = null;
                if (authorizationParameters.data.requiredParameters.Any(p => p.name.Equals("user")))
                {
                    credentials = new AuthorizationRequest
                    {
                        parameters = new AuthorizationParameter[]
                        {
                            new AuthorizationParameter
                            {
                                name = "user",
                                value = loginParameters["username"]
                            },
                            new AuthorizationParameter
                            {
                                name = "password",
                                value = loginParameters["password"]
                            }
                        },
                        saveDetails = true,
                        realmUserIdentifier = loginParameters["username"]
                    };
                }
                else if (authorizationParameters.data.requiredParameters.Any(p => p.name.Equals("id")))
                {
                    credentials = new AuthorizationRequest
                    {
                        parameters = new AuthorizationParameter[]
                        {
                            new AuthorizationParameter
                            {
                                name = "id",
                                value = loginParameters["id"]
                            },
                            new AuthorizationParameter
                            {
                                name = "birthDay",
                                value = loginParameters["birthDay"]
                            },
                            new AuthorizationParameter
                            {
                                name = "birthMonth",
                                value = loginParameters["birthMonth"]
                            },
                            new AuthorizationParameter
                            {
                                name = "birthYear",
                                value = loginParameters["birthYear"]
                            },
                            new AuthorizationParameter
                            {
                                name = "password",
                                value = loginParameters["password"]
                            }
                        },
                        saveDetails = true,
                        realmUserIdentifier = loginParameters["id"]
                    };
                }
                else
                {
                    Console.WriteLine("Unkown Required Parameters");
                    return;
                }

                response = Task.Run(() => _client.PostAsJsonAsync(uri, credentials)).Result;
                if (!response.IsSuccessStatusCode)
                {
                    var errorReponse = response.Content.ReadAsAsync<dynamic>().Result;
                    Console.WriteLine(JsonConvert.SerializeObject(errorReponse, Formatting.Indented));
                    return;
                }
                var authorizationResponse = response.Content.ReadAsAsync<AuthenticationResponse>().Result; //Al no tener parametros authenticationDone=false
                Console.WriteLine("Login to the realm with the parameters values entered by the end user:");
                Console.WriteLine(JsonConvert.SerializeObject(authorizationResponse.data, Formatting.Indented));
                var realmUserId = 0;
                if (authorizationResponse.data.authenticationDone)
                {
                    realmUserId = authorizationResponse.data.realmUserId;
                }
                else
                {
                    credentials.parameters = credentials.parameters.Concat(new AuthorizationParameter[]
                        {
                            new AuthorizationParameter
                            {
                                name = "check_aggregation_status",
                                value = "true"
                            }
                        }).ToArray();

                    while (!authorizationResponse.data.authenticationDone &&
                        (authorizationResponse.data.requiredParameters.Length > 0 &&
                        authorizationResponse.data.requiredParameters[0].name == "check_aggregation_status"))
                    {
                        Thread.Sleep(5000);

                        response = Task.Run(() => _client.PostAsJsonAsync(uri, credentials)).Result;
                        if (!response.IsSuccessStatusCode)
                        {
                            var errorReponse = response.Content.ReadAsAsync<dynamic>().Result;
                            Console.WriteLine(JsonConvert.SerializeObject(errorReponse, Formatting.Indented));
                            return;
                        }
                        authorizationResponse = response.Content.ReadAsAsync<AuthenticationResponse>().Result;
                        Console.WriteLine("Response from Aggregation Status:");
                        Console.WriteLine(JsonConvert.SerializeObject(authorizationResponse.data, Formatting.Indented));
                    }
                    var content = response.Content.ReadAsStringAsync().Result;

                    Console.WriteLine($"Eurobits response Content: {content}");
                    realmUserId = authorizationResponse.data.realmUserId;

                    if (!authorizationResponse.data.authenticationDone)
                    {
                        Console.WriteLine($"Authentication fails with Eurobits response Content: {content}");
                        return;
                    }
                }
                //Change culture
                var culture = "es-ES";
                uri = new Uri(_client.BaseAddress, $"user/v1/me/culture?culture=es-ES");
                response = Task.Run(() => _client.PutAsJsonAsync(uri, culture)).Result;
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Culture NOT changed to: {culture}");
                    var errorReponse = response.Content.ReadAsAsync<dynamic>().Result;
                    Console.WriteLine(JsonConvert.SerializeObject(errorReponse, Formatting.Indented));
                    return;
                }
                Console.WriteLine($"Culture changed to: {culture}");
                Thread.Sleep(2300);
                //Subscribe to userEvents
                var foos = "accounts_available_amount,gasto_cuenta,ingreso_cuenta";// "accounts_available_amount,transactions_threshold_deposit,transactions_threshold_expenses";
                var fooArray = foos.Split(',');
                var userEventsSubscription = new UserEventSubscription
                {
                    isSubscribed = true,
                    channelName = "Iberfabric",
                    userEventTypeIdentifiers = fooArray
                };
                uri = new Uri(_client.BaseAddress, $"user/v1/userevents/subscription?sessionToken={Uri.EscapeDataString(authorizationResponse.data.sessionToken)}");
                response = Task.Run(() => _client.PutAsJsonAsync(uri, userEventsSubscription)).Result;
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"User NOT subscribed to userEvents");
                    var errorReponse = response.Content.ReadAsAsync<dynamic>().Result;
                    Console.WriteLine(JsonConvert.SerializeObject(errorReponse, Formatting.Indented));
                    return;
                }
                Console.WriteLine($"User subscribed to userEvents");
                Thread.Sleep(2300);

                //Update subscription with values

                var userEventsSubscriptionDetails = new UpdateSubscriptionSettingsRequest
                {
                    SubscriptionSettings = new UpdateUserEventTypeSubscriptionSettingModel[]
                    {
                        new UpdateUserEventTypeSubscriptionSettingModel
                        {
                            Identifier = "LimitesIngresoCuenta",
                            Value = "{\"2\":2,\"20\":22,\"38\":22,\"56\":22}"
                        },
                        new UpdateUserEventTypeSubscriptionSettingModel
                        {
                            Identifier = "LimitesGastoCuenta",
                            Value = "{\"2\":2,\"20\":29,\"38\":22,\"56\":22}"
                        }
                    }                     
                };
                uri = new Uri(_client.BaseAddress, $"user/v1/userevents/subscription/details?sessionToken={Uri.EscapeDataString(authorizationResponse.data.sessionToken)}");
                response = Task.Run(() => _client.PutAsJsonAsync(uri, userEventsSubscriptionDetails)).Result;
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"User has NOT updated subscription to userEvents");
                    var errorReponse = response.Content.ReadAsAsync<dynamic>().Result;
                    Console.WriteLine(JsonConvert.SerializeObject(errorReponse, Formatting.Indented));
                    return;
                }
                Console.WriteLine($"User has updated subscription to userEvents");
                Thread.Sleep(2300);
                // List bank accounts https://docs.meniga.com/docs/core/api/user/account-aggregation.html#list-bank-accounts
                uri = new Uri(_client.BaseAddress, $"user/v1/sync/accounts/{realmUserId}?sessionToken={Uri.EscapeDataString(authorizationResponse.data.sessionToken)}");
                response = Task.Run(() => _client.GetAsync(uri)).Result;
                if (!response.IsSuccessStatusCode)
                {
                    var errorReponse = response.Content.ReadAsAsync<dynamic>().Result;
                    Console.WriteLine(JsonConvert.SerializeObject(errorReponse, Formatting.Indented));
                    return;
                }
                var accountsReponse = response.Content.ReadAsAsync<GetAccountsResponse>().Result;
                Console.WriteLine("List bank accounts:");
                Console.WriteLine(JsonConvert.SerializeObject(accountsReponse.data, Formatting.Indented));

                

                var newAccounts = accountsReponse.data.AsQueryable().Where(a => !a.accountExists).ToArray();
                if (newAccounts.Length > 0)
                {
                    // Add accounts https://docs.meniga.com/docs/core/api/user/account-aggregation.html#add-accounts
                    uri = new Uri(_client.BaseAddress, $"user/v1/sync/accounts/{realmUserId}/authorize?sessionToken={Uri.EscapeDataString(authorizationResponse.data.sessionToken)}");
                    response = Task.Run(() => _client.PostAsJsonAsync(uri, newAccounts)).Result;
                    Console.WriteLine("Add accounts:");
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Failed (duplicated accounts?)");
                    }
                    else
                    {
                        Console.WriteLine("Success");
                    }
                }

                // Synchronize transactions https://docs.meniga.com/docs/core/api/user/account-aggregation.html#synchronize-transactions
                uri = new Uri(_client.BaseAddress, $"user/v1/sync/realm/{realmUserId}");
                var wait = new WaitRequest
                {
                    waitForCompleteMilliseconds = 5000,
                    sessionToken = authorizationResponse.data.sessionToken
                };
                response = Task.Run(() => _client.PostAsJsonAsync(uri, wait)).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var errorReponse = response.Content.ReadAsAsync<dynamic>().Result;
                    Console.WriteLine(JsonConvert.SerializeObject(errorReponse, Formatting.Indented));
                    return;
                }
                var syncReponse = response.Content.ReadAsAsync<SyncResponse>().Result;
                Console.WriteLine("Syncronize transactions:");
                Console.WriteLine(JsonConvert.SerializeObject(syncReponse.data, Formatting.Indented));
            }
        }

        #region MenigaAPIClasses
        public class MUsername
        {
            [JsonProperty("m-username")]
            public string Username { get; set; }
        }

        public class AuthorizationResponse
        {
            public AccessData data { get; set; }
        }

        public class AccessData
        {
            public string accessToken { get; set; }
            public string refreshToken { get; set; }
        }

        public class AuthenticationResponse
        {
            public AuthenticationData data { get; set; }
        }

        public class AuthenticationData
        {
            public bool authenticationDone { get; set; }
            public RequiredParameter[] requiredParameters { get; set; }
            public string contentType { get; set; }
            public string textChallenge { get; set; }
            public string binaryChallenge { get; set; }
            public string errorMessage { get; set; }
            public string userIdentifier { get; set; }
            public int realmUserId { get; set; }
            public bool canSave { get; set; }
            public string loginHelp { get; set; }
            public string sessionToken { get; set; }
        }

        public class RequiredParameter
        {
            public string name { get; set; }
            public string displayName { get; set; }
            public string regularExpression { get; set; }
            public object minLength { get; set; }
            public object maxLength { get; set; }
            public bool isPassword { get; set; }
            public bool isHidden { get; set; }
            public bool isDropDown { get; set; }
            public object dropDownValues { get; set; }
            public object parentId { get; set; }
            public bool canSave { get; set; }
            public bool isEncrypted { get; set; }
            public bool isIdentity { get; set; }
        }

        public class AuthorizationRequest
        {
            public AuthorizationParameter[] parameters { get; set; }
            public bool saveDetails { get; set; }
            public string sessionToken { get; set; }
            public string realmUserIdentifier { get; set; }
        }

        public class AuthorizationParameter
        {
            public string name { get; set; }
            public string value { get; set; }
        }

        public class AuthorizationAccountResponse
        {
            public AuthorizationAccountData data { get; set; }
        }

        public class AuthorizationAccountData
        {
            public bool authenticationDone { get; set; }
            public object[] requiredParameters { get; set; }
            public string contentType { get; set; }
            public string textChallence { get; set; }
            public string binaryChallenge { get; set; }
            public object errorMessage { get; set; }
            public string userIdentifier { get; set; }
            public int realmUserId { get; set; }
            public bool canSave { get; set; }
            public string loginHelp { get; set; }
            public string sessionToken { get; set; }
        }

        public class GetAccountsResponse
        {
            public Accounts[] data { get; set; }
        }

        public class Accounts
        {
            public string name { get; set; }
            public string accountIdentifier { get; set; }
            public int accountTypeId { get; set; }
            public bool accountExists { get; set; }
        }

        public class WaitRequest
        {
            public int waitForCompleteMilliseconds { get; set; }
            public string sessionToken { get; set; }
        }

        public class SyncResponse
        {
            public SyncData data { get; set; }
        }

        public class SyncData
        {
            public int syncHistoryId { get; set; }
            public bool isSyncDone { get; set; }
            public DateTime syncSessionStartTime { get; set; }
            public RealmSyncResponse[] realmSyncResponses { get; set; }
        }

        public class RealmSyncResponse
        {
            public int realmCredentialsId { get; set; }
            public string realmCredentialsDisplayName { get; set; }
            public int organizationId { get; set; }
            public string organizationName { get; set; }
            public string organizationBankCode { get; set; }
            public object[] accountSyncStatuses { get; set; }
            public object authenticationChallenge { get; set; }
            public bool isSyncDone { get; set; }
        }

        public class UserEventSubscription
        {
            public bool isSubscribed { get; set; }
        
            public string channelName { get; set; }
        
            public string[] userEventTypeIdentifiers { get; set; }
        }
    #endregion

        public class UpdateSubscriptionSettingsRequest
        {
        /// <summary>
        /// The subscription settings to update
        /// </summary>
            public UpdateUserEventTypeSubscriptionSettingModel[] SubscriptionSettings { get; set; }
        }

    public class UpdateUserEventTypeSubscriptionSettingModel
    {
        /// <summary>
        /// The identifier for the user event type setting
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// The value of the user event type settings in the format of the underlying data type
        /// </summary>
        public string Value { get; set; }
    }
}
}
