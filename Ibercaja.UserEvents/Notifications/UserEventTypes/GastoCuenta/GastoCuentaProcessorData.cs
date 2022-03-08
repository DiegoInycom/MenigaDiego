using Meniga.Core.UserEvents.BusinessModels;

namespace Ibercaja.UserEvents.Notifications.UserEventTypes.GastoCuenta
{
    public class GastoCuentaProcessorData : DefaultData
    {
        
	    public string AccountName { get; set; }

        public long TransactionId { get; set; }

        public string CurrencyCode { get; set; }

        public decimal Amount { get; set; }
    
    }
}
