using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Meniga.Core.Accounts;
using Meniga.Core.Data;
using Meniga.Core.Data.User;
using Meniga.Core.UserEvents.Extensions;
using Meniga.Core.UserEvents.Helpers;
using Microsoft.Practices.Unity;

namespace Ibercaja.UserEvents.Notifications.UserEventTypes.IngresoCuenta
{
	public class IngresoCuentaProcessor : IUserEventProcessor<IngresoCuentaProcessorSettings, IngresoCuentaProcessorData>
	{
		public const string Identifier = "ingreso_cuenta";
		private static readonly ILog Logger = LogManager.GetLogger("IngresoCuentaProcessor");

		[Dependency]
		public ICoreContextProvider CoreContextProvider { get; set; }

		[Dependency]
		public IAccountSetupCache AccountSetupCache { get; set; }

		public ICollection<long> InitBatchProcess(IProcessingContext<IngresoCuentaProcessorSettings, IngresoCuentaProcessorData> context)
		{
			throw new NotSupportedException(string.Format("The {0} user event processor does not support batch processing where the affected user(s) is unknown.", "ingreso_cuenta"));
		}

		public ICollection<IngresoCuentaProcessorData> ProcessUserEvents(long userId, IProcessingContext<IngresoCuentaProcessorSettings, IngresoCuentaProcessorData> context)
		{
			List<long> accountIds = ProcessingContextHelper.GetTransactionAccountIds(context);
			List<long> transactionIds = ProcessingContextHelper.GetTransactionIds(context);
			if (!transactionIds.Any())
			{
				return null;
			}

			decimal? threshold;
			var dataEntries = new List<IngresoCuentaProcessorData>();
			foreach (var acc in accountIds)
			{
				if (context.UserSettings?.LimitesIngresoCuenta != null && context.UserSettings.LimitesIngresoCuenta.ContainsKey(acc.ToString()))
				{
					threshold = context.UserSettings.LimitesIngresoCuenta[acc.ToString()];
				}
				else if (context.SystemSettings?.LimitesIngresoCuenta != null && context.SystemSettings.LimitesIngresoCuenta.ContainsKey(acc.ToString()))
				{
					threshold = context.SystemSettings.LimitesIngresoCuenta[acc.ToString()];
				}
				else if (context.Settings?.LimitesIngresoCuenta != null && context.Settings.LimitesIngresoCuenta.ContainsKey(acc.ToString()))
				{
					threshold = context.Settings.LimitesIngresoCuenta[acc.ToString()];
				}
				else
				{
					Logger.Debug($"No income above threshold value found for account {acc}");
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
							if (context.SystemSettings?.CategoriasIngresoCuenta != null && context.SystemSettings.CategoriasIngresoCuenta.Contains((int)trx.CategoryId))
							{
								if (trx.Amount > 0 && trx.Amount >= threshold)
								{
									dataEntries.Add(new IngresoCuentaProcessorData()
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
