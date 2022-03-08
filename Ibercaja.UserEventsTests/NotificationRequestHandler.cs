using System;
using System.Threading.Tasks;
using Ibercaja.UserEvents.Iberfabric;
using NServiceBus;

namespace Ibercaja.UserEventsTests
{
    public class NotificationCommandHandler : IHandleMessages<NotificationCommand>
    {
        public Task Handle(NotificationCommand message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Message received: {message.Message}");

            return Task.CompletedTask;
        }
    }
}
