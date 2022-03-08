using System.Collections.Generic;
using System.Linq;
using Meniga.Core.Accounts;
using Meniga.Core.Transactions;
using log4net;
using System;
using Newtonsoft.Json;
using System.Reflection;
using Meniga.Core.Data;
using Meniga.Core.Data.User;
using System.Data.SqlClient;

namespace Ibercaja.UserEvents.Notifications.UserEventTypes.IngresoCuenta
{
    public class IngresoCuentaDataAccess : INotificationProvider
    {
        private readonly ICoreContextProvider _contextProvider;
        private readonly IAccountsManager _accountsManager;
        private readonly ITransactionManager _transactionsManager;
        private static readonly ILog Logger = LogManager.GetLogger("IngresoCuentaDataAccess");

        public IngresoCuentaDataAccess(ICoreContextProvider contextProvider, IAccountsManager accountsManager, ITransactionManager transactionManager)
        {
            _contextProvider = contextProvider;
            _accountsManager = accountsManager;
            _transactionsManager = transactionManager;

        }

        public Notification GetNotification(string userIdentifier, long userId, IDictionary<string, object> userEvent, long userEventId, DateTime? createdEvent, string message)
        {
            var typeOfBatch = GetTypeOfBatch(userId, userEventId);

            var transactionId = userEvent["TransactionId"] as long? ?? 0;
            var transaction = _transactionsManager.GetTransaction(userId, transactionId);
            if (transaction == null)
            {
                Logger.Error($"Error retrieving the transaction for the userId: {userId} and TransactionId: {transactionId} ");
                return null;
            }

            var account = _accountsManager.GetAccountsByUserAndIds(userId, new List<long> { transaction.AccountId }).FirstOrDefault();
            if (account == null)
            {
                Logger.Error($"Error retrieving the Account for the userId: {userId} and AccountId: {transaction.AccountId} ");
                return null;
            }

            var amount = transaction.Amount.ToString("0.00");
            var notificationData = new
            {
                Date = transaction.Date,
                AccountName = $"{account.Name}",
                AccountNumber = $"{account.AccountIdentifier}",
                AccountCategory = $"{account.AccountCategory}",
                Amount = $"{amount}€",
                Description = transaction.Text, 
                NotificationType = IbercajaUserEventTypes.IngresoCuenta + "." + typeOfBatch
            };

            try
            {
                amount = NormalizeAmount(amount);
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex} with Amount: {amount} for userId: {userId}");
                return null;
            }

            var accountType = notificationData.AccountCategory == "Credit" ? "tarjeta" : "cuenta";
            message = string.Format(message, accountType, amount);

            var notification = new Notification
            {
                UserNici = userIdentifier,
                SourceId = $"{userIdentifier}-{userEventId}",
                NotificationType = IbercajaUserEventTypes.IngresoCuenta + "." + typeOfBatch,
                NotificationMessage = message,
                NotificationMetadata = JsonConvert.SerializeObject(notificationData),
                CreatedEvent = createdEvent ?? DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                CategoryId = transaction.CategoryId.ToString()
            };

            return notification;
        }

        public string NormalizeAmount(string amount)
        {
            return Math.Abs(decimal.Parse(amount)).ToString();
        }

        public string GetTypeOfBatch(long userId, long userEventId)
        {
            string batchType = string.Empty;

            using (ICoreUserContext userContext = _contextProvider.UserContext(userId))
            {
                using (var batchContext = _contextProvider.BatchContext())
                {
                    Logger.Info($"Ingreso: batchType query for userId: {userId} and userEventId: {userEventId}");

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
                                Logger.Info($"Ingreso batchType: {batchType}");
                                switch (batchType)
                                {
                                    case "BA":
                                        batchType = "1";
                                        Logger.Info($"Ingreso batchType Final: {batchType}");
                                        return batchType;
                                    case "BT":
                                        batchType = "1";
                                        Logger.Info($"Ingreso batchType Final: {batchType}");
                                        return batchType;
                                    case "AH":
                                        batchType = "2";
                                        Logger.Info($"Ingreso batchType Final: {batchType}");
                                        return batchType;
                                    case "TS":
                                        batchType = "2";
                                        Logger.Info($"Ingreso batchType Final: {batchType}");
                                        return batchType;
                                    case "CT":
                                        batchType = "2";
                                        Logger.Info($"Ingreso batchType Final: {batchType}");
                                        return batchType;
                                    case "CS":
                                        batchType = "2";
                                        Logger.Info($"Ingreso batchType Final: {batchType}");
                                        return batchType;
                                    case "FT":
                                        batchType = "2";
                                        Logger.Info($"Ingreso batchType Final: {batchType}");
                                        return batchType;
                                    case "":
                                        batchType = "";
                                        Logger.Info($"Ingreso batchType Final: {batchType}");
                                        return batchType;
                                    default:
                                        batchType = "2";
                                        Logger.Info($"Ingreso batchType Final: {batchType}");
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
            Logger.Info($"Ingreso batchType Final: {batchType}");
            return batchType;
        }
    }

}

