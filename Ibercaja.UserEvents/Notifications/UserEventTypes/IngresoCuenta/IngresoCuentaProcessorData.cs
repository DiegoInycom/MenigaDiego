using Meniga.Core.UserEvents.BusinessModels;

namespace Ibercaja.UserEvents.Notifications.UserEventTypes.IngresoCuenta
{
	public class IngresoCuentaProcessorData : DefaultData
	{
		public long TransactionId { get; set; }

		public decimal Amount { get; set; }

		public string AccountIdentifier { get; set; }

		public string AccountName { get; set; }
	}
}
