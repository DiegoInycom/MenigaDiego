using Meniga.Core.Accounts;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Linq;
using log4net;
using Meniga.Core.Data;
using Meniga.Core.Data.User;
using System.Data.SqlClient;

namespace Ibercaja.UserEvents.Notifications.UserEventTypes
{
    public class AccountsAvailableAmount : INotificationProvider
    {
        private readonly ICoreContextProvider _contextProvider;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AccountsAvailableAmount));
        private readonly IAccountsManager _accountsManager;

        public AccountsAvailableAmount(ICoreContextProvider contextProvider, IAccountsManager accountsManager)
        {
            _contextProvider = contextProvider;
            _accountsManager = accountsManager;
        }

        public Notification GetNotification(string userIdentifier, long personId, IDictionary<string, object> userEvent, long userEventId, DateTime? createdEvent, string message)
        {
            var typeOfBatch = GetTypeOfBatch(personId, userEventId);

            var accountName = userEvent["AccountName"] as string;
            var thresholdAmount = Convert.ToString(userEvent["ThresholdAmountTrigger"]);
            try
            {
                thresholdAmount = NormalizeThresholdAmount(thresholdAmount);
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex} with Amount: {thresholdAmount} for userId: {personId}");
                return null;
            }

            var account = _accountsManager.GetAccountsByUser(personId).FirstOrDefault(a => a.Name == accountName);
            var accountCategory = account.AccountCategory.ToString();
            var accountNumber = account.AccountIdentifier;

            var notification = new Notification
            {
                UserNici = userIdentifier,
                SourceId = $"{userIdentifier}-{userEventId}",
                NotificationType = IbercajaUserEventTypes.AccountsAvailableAmount + "." + typeOfBatch,
                NotificationMessage = message,
                NotificationMetadata = JsonConvert.SerializeObject(new { AccountName = accountName, AccountNumber = accountNumber, AccountCategory = accountCategory, ThresholdAmount = thresholdAmount }),
                CreatedEvent = createdEvent ?? DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                CategoryId = null,
                AccountTypeId = account.AccountTypeId,
                Identifier = account.AccountIdentifier
            };

            return notification;
        }

        public string NormalizeThresholdAmount(string thresholdAmount)
        {
            return Math.Abs(decimal.Parse(thresholdAmount)).ToString("0.00€");
        }

        public string GetTypeOfBatch(long userId, long userEventId)
        {
            string batchType = string.Empty;

            using (ICoreUserContext userContext = _contextProvider.UserContext(userId))
            {
                using (var batchContext = _contextProvider.BatchContext())
                {
                    Logger.Info($"AvailableAmount: batchType query for userId: {userId} and userEventId: {userEventId}");

                    string queryString = ";with MyCTE as ( " +
                                            "select realm_account_info_id from IbercajaBase.batch.realm_user_account_relations " +
                                                "where realm_user_info_id in ( " +
                                                    "select id from IbercajaBase.batch.realm_user_info  " +
                                                        "where usr_id = @usr_id)), " +
                                          "MyCTE2 as ( " +
                                            "select top(1) batch_id from IbercajaBase.batch.batch_transactions  " +
                                                "where account_identifier in (" +
                                                    "select account_identifier from IbercajaBase.batch.realm_account_info  " +
                                                       "where id in (Select realm_account_info_id from MyCTE)) order by id desc) " +
                                          "select SUBSTRING(identifier,1,2) as sub from IbercajaBase.batch.batches  " +
                                           " where id = (select batch_id from MyCTE2)";
                    string connectionString = "Data source=IBPFMPRU; initial catalog=IbercajaBase;integrated security=True; multipleactiveresultsets=True";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand(queryString, connection);
                        command.Parameters.AddWithValue("@usr_id", userId);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        try
                        {
                            while (reader.Read())
                            {

                                batchType = reader["sub"].ToString();
                                Logger.Info($"AvailableAmount batchType: {batchType}");
                                switch (batchType)
                                {
                                    case "BA":
                                        batchType = "1";
                                        Logger.Info($"AvailableAmount batchType Final: {batchType}");
                                        return batchType;
                                    case "BT":
                                        batchType = "1";
                                        Logger.Info($"AvailableAmount batchType Final: {batchType}");
                                        return batchType;
                                    case "AH":
                                        batchType = "2";
                                        Logger.Info($"AvailableAmount batchType Final: {batchType}");
                                        return batchType;
                                    case "TS":
                                        batchType = "2";
                                        Logger.Info($"AvailableAmount batchType Final: {batchType}");
                                        return batchType;
                                    case "CT":
                                        batchType = "2";
                                        Logger.Info($"AvailableAmount batchType Final: {batchType}");
                                        return batchType;
                                    case "CS":
                                        batchType = "2";
                                        Logger.Info($"AvailableAmount batchType Final: {batchType}");
                                        return batchType;
                                    case "FT":
                                        batchType = "2";
                                        Logger.Info($"AvailableAmount batchType Final: {batchType}");
                                        return batchType;
                                    case "":
                                        batchType = "";
                                        Logger.Info($"AvailableAmount batchType Final: {batchType}");
                                        return batchType;
                                    default:
                                        batchType = "2";
                                        Logger.Info($"AvailableAmount batchType Final: {batchType}");
                                        return batchType;
                                }
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }

                    //if (string.IsNullOrEmpty(batchType))
                    //{
                    //    Logger.Warn($"Something went wrong trying to get batchType for userId: {userId} and userEventId: {userEventId}");

                    //}
                }
            }
            Logger.Info($"AvailableAmount batchType Final: {batchType}");
            return batchType;
        }
    }
}
