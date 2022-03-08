using System.Collections.Generic;
using Meniga.Core.UserEvents.BusinessModels;
using Meniga.Core.UserEvents.Extensions;

namespace Ibercaja.UserEvents.Notifications.UserEventTypes.IngresoCuenta
{
    public class IngresoCuentaFactory : IUserEventTypeFactory
    {
        public ICollection<IUserEventTypeInfo> GetUserEventTypes()
        {
            return new[]
                {
                    new UserEventTypeInfo
                    {
                        Name = "ingreso cuenta",
                        Identifier = IngresoCuentaProcessor.Identifier,
                        ParentIdentifier = "transactions",
                        ProcessorType = typeof(IngresoCuentaProcessor),
                        TopicName = "Transaction"
                    }
                };
        }
    }
}