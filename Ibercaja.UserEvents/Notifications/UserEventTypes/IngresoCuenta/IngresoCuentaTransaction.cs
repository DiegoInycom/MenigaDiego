using System;

namespace Ibercaja.UserEvents.Notifications.UserEventTypes.IngresoCuenta
{

	public class IngresoCuentaTransaction
	{
		public long Id { get; set; }

		public DateTime? SubDate { get; set; }

		public decimal Amount { get; set; }

		public string TransactionText { get; set; }

		public string AccountName { get; set; }
	}
}
