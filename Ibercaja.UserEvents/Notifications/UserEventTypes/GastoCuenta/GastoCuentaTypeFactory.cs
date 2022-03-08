using System.Collections.Generic;
using Meniga.Core.UserEvents.BusinessModels;
using Meniga.Core.UserEvents.Extensions;

namespace Ibercaja.UserEvents.Notifications.UserEventTypes.GastoCuenta
{
    public class GastoCuentaTypeFactory : IUserEventTypeFactory
    {
        public ICollection<IUserEventTypeInfo> GetUserEventTypes()
        {
            return new[]
                {
                    new UserEventTypeInfo
                    {
                        Name = "gasto cuenta",
                        Identifier = GastoCuentaProcessor.Identifier,
                        ParentIdentifier = "transactions",
                        ProcessorType = typeof(GastoCuentaProcessor),
                        TopicName = "Transaction"
                    }
                };
        }
    }
}
