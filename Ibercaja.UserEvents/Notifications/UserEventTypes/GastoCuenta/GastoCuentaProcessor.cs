using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Meniga.Core.Accounts;
using Meniga.Core.Data;
using Meniga.Core.Data.User;
using Meniga.Core.UserEvents.Extensions;
using Meniga.Core.UserEvents.Helpers;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;

namespace Ibercaja.UserEvents.Notifications.UserEventTypes.GastoCuenta
{
    public class GastoCuentaProcessor : IUserEventProcessor<GastoCuentaProcessorSettings, GastoCuentaProcessorData>
    {
		[Dependency]
		public ICoreContextProvider CoreContextProvider { get; set; }

		public const string Identifier = "gasto_cuenta";

		private static readonly ILog Logger = LogManager.GetLogger("GastoCuentaProcessor");

		[Dependency]
		public IAccountSetupCache AccountSetupCache { get; set; }

		public ICollection<long> InitBatchProcess(IProcessingContext<GastoCuentaProcessorSettings, GastoCuentaProcessorData> context)
		{
			throw new NotSupportedException(string.Format("The {0} user event processor does not support batch processing where the affected user(s) is unknown.", "transactions_threshold_expenses"));
		}

		public ICollection<GastoCuentaProcessorData> ProcessUserEvents(long userId, IProcessingContext<GastoCuentaProcessorSettings, GastoCuentaProcessorData> context)
		{
			List<long> accountIds = ProcessingContextHelper.GetTransactionAccountIds(context);
			List<long> transactionIds = ProcessingContextHelper.GetTransactionIds(context);
			if (!transactionIds.Any())
			{
				return null;
			}

			decimal? threshold;
			var dataEntries = new List<GastoCuentaProcessorData>();

			foreach (var acc in accountIds)
			{
				if (context.UserSettings?.LimitesGastoCuenta != null && context.UserSettings.LimitesGastoCuenta.ContainsKey(acc.ToString()))
				{
					threshold = context.UserSettings.LimitesGastoCuenta[acc.ToString()];
				}
				else if (context.SystemSettings?.LimitesGastoCuenta != null && context.SystemSettings.LimitesGastoCuenta.ContainsKey(acc.ToString()))
				{
					threshold = context.SystemSettings.LimitesGastoCuenta[acc.ToString()];
				}
				else if (context.Settings?.LimitesGastoCuenta != null && context.Settings.LimitesGastoCuenta.ContainsKey(acc.ToString()))
				{
					threshold = context.Settings.LimitesGastoCuenta[acc.ToString()];
				}
				else
				{
					Logger.Debug($"No expenses above threshold value found for account {acc}");
					continue;
				}
				
				using (var dbContext = CoreContextProvider.UserContext(userId))
				{
					Transaction trx;
					foreach (var tr in transactionIds)
					{
						trx = dbContext.Transactions.Where(t => t.Id == tr).First();
						if (trx.Account.Id == acc) 
						{
							if (context.SystemSettings?.CategoriasGastoCuenta != null && context.SystemSettings.CategoriasGastoCuenta.Contains((int)trx.CategoryId))
							{
								if (trx.Amount < 0 && Math.Abs(trx.Amount) >= threshold)
								{
									dataEntries.Add(new GastoCuentaProcessorData()
									{
										TransactionId = trx.Id,
										TopicId = 5,
										Date = trx.Timestamp,
										AccountName = trx.Account.Name,
										ResourceIdentifier = "Transactions",
										Amount = trx.Amount
									});
								}
								else
								{
									continue;
								}
							}
							else
							{
								continue;
							}

							
						}
						else
						{
							continue;
						}
						
					}

				}
					
			}

			return dataEntries;
		}
	}
}
