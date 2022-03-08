using Ibercaja.UserEvents.Iberfabric;
using NServiceBus;
using System;

namespace Ibercaja.UserEventsTests
{
    class Program
    {
        private static IEndpointInstance _bus;
        
        static void Main(string[] args)
        {
            var endpoint = "host=localhost";
            ConfigureNServiceBusWithRabbitMQ(endpoint);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            _bus.Stop().ConfigureAwait(false).GetAwaiter().GetResult();
        }


         private static void ConfigureNServiceBusWithRabbitMQ(string endpoint)
        {            
            Console.WriteLine($"***Start configuration of Ibercaja.UserEventsTests; Endpoint: {endpoint}");

            const string endpointName = "Ibercaja.PFM.UserEventsTests";
            //const string endpointName = "Ibercaja.PFM.UserEvents.IberfabricBusNotificationService";

            /* Configuration to IEvent
             var endpointConfiguration = new EndpointConfiguration(endpointName);
             endpointConfiguration.UsePersistence<LearningPersistence>();
             endpointConfiguration.ConfigureConventions();
             endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

             var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
             var transportTopology = transport.UseConventionalRoutingTopology();
             transport.ConnectionString(endpoint);
             transport.Transactions(TransportTransactionMode.None);

             endpointConfiguration.SendFailedMessagesTo($"{endpointName}.error");
             endpointConfiguration.EnableInstallers();
             */

            /*Configuration ICommand*/
            var endpointConfiguration = new EndpointConfiguration(endpointName);
            endpointConfiguration.UsePersistence<LearningPersistence>();
            endpointConfiguration.ConfigureConventions();
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            var transportTopology = transport.UseConventionalRoutingTopology();
            var routingConfig = transport.Routing();
            routingConfig.RouteToEndpoint(
                assembly: typeof(NotificationCommand).Assembly,
                destination: endpointName);
            transport.ConnectionString(endpoint);
            transport.Transactions(TransportTransactionMode.None);

            endpointConfiguration.SendFailedMessagesTo($"{endpointName}.error");
            endpointConfiguration.EnableInstallers();

            _bus = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

            Console.WriteLine($"***End configuration of Ibercaja.UserEvents.UserEventsTests");
            Console.WriteLine();
        }
    }
}
