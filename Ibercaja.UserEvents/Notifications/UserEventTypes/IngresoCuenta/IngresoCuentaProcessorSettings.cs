using System.Collections.Generic;
using log4net;
using Meniga.Core.UserEvents.Attributes;
using Meniga.Core.UserEvents.BusinessModels;

namespace Ibercaja.UserEvents.Notifications.UserEventTypes.IngresoCuenta
{
	public class IngresoCuentaProcessorSettings : DefaultProcessorSettings
	{
		private static readonly ILog Logger = LogManager.GetLogger("IngresoCuentaProcessorSettings");

		public IngresoCuentaProcessorSettings()
		{
		}

		[UserEventTypeSettingIdentifier("LimitesIngresoCuenta")]
		//[SystemOnlySetting]
		public Dictionary<string, decimal> LimitesIngresoCuenta { get; set; }

		[UserEventTypeSettingIdentifier("CategoriasIngresoCuenta")]
		//[SystemOnlySetting]
		public List<int> CategoriasIngresoCuenta { get; set; }

		public IngresoCuentaProcessorSettings(bool registerDefaults)
		{
			Logger.Debug("Inicializa el valor LimitesIngresoCuenta"); //TODO: ¿Inicializarlo a cero?
			LimitesIngresoCuenta = new Dictionary<string, decimal> 
			{
				{
				   "44", 50
				} 
			};
		}



	}
}
