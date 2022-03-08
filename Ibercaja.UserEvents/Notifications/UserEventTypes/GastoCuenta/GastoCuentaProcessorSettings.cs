using System.Collections.Generic;
using log4net;
using Meniga.Core.UserEvents.Attributes;
using Meniga.Core.UserEvents.BusinessModels;

namespace Ibercaja.UserEvents.Notifications.UserEventTypes.GastoCuenta
{
    public class GastoCuentaProcessorSettings : DefaultProcessorSettings
    {
		private static readonly ILog Logger = LogManager.GetLogger("GastoCuentaProcessorSettings");

		public GastoCuentaProcessorSettings()
		{
		}

		[UserEventTypeSettingIdentifier("LimitesGastoCuenta")]
		public Dictionary<string, decimal> LimitesGastoCuenta { get; set; }

		[UserEventTypeSettingIdentifier("CategoriasGastoCuenta")]
		public List<int> CategoriasGastoCuenta { get; set; }

		public GastoCuentaProcessorSettings(bool registerDefaults)
		{
			Logger.Debug("Inicializa el valor LimitesGastoCuenta"); //TODO: ¿Inicializarlo a cero?
			LimitesGastoCuenta = new Dictionary<string, decimal> 
			{
				{
					"38", 5
				}	 
			};
		}
	}
}
