using Ibercaja.UserEvents.Notifications.UserEventTypes;
using Ibercaja.UserEvents.Notifications.UserEventTypes.GastoCuenta;
using Ibercaja.UserEvents.Notifications.UserEventTypes.IngresoCuenta;
using Meniga.Core.Accounts;
using Meniga.Core.Data;
using Meniga.Core.Transactions;

namespace Ibercaja.UserEvents.Notifications
{
    /// <summary>
    /// Contains mapping between Meniga User Events and Ibercaja Notifications
    /// Creates all available notifications
    /// </summary>
    public class IbercajaNotificationProviderFactory : INotificationProviderFactory
    {
        private readonly IAccountsManager _accountsManager;
        private readonly ITransactionManager _transactionsManager;
        private readonly ICoreContextProvider _contextProvider;

        public IbercajaNotificationProviderFactory(IAccountsManager accountsManager, ITransactionManager transactionManager, ICoreContextProvider contextProvider)
        {
            _accountsManager = accountsManager;
            _transactionsManager = transactionManager;
            _contextProvider = contextProvider;
        }

        public virtual INotificationProvider CreateNotificationProvider(string userEventTypeId)
        {
            switch (userEventTypeId)
            {
                case "6":
                    return new AccountsAvailableAmount(_contextProvider, _accountsManager);
                case "12":
                    return new TransactionsDepositNotification(_contextProvider, _accountsManager, _transactionsManager);
                case "13":
                    return new TransactionsThresholdExpensesNotification(_contextProvider, _accountsManager, _transactionsManager);
                case "25":
                    return new IngresoCuentaDataAccess(_contextProvider, _accountsManager, _transactionsManager);
                case "26":
                    return new GastoCuentaDataAccess(_contextProvider, _accountsManager, _transactionsManager);
            }

            return new UnknownNotificationProvider();
        }
    }
}