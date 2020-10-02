using System.Threading;
using System.Threading.Tasks;
using System;
using MassTransit;
using MassTransit.Saga;
using OrderTracking.Saga;
using OrderTracking.Models;
using OrderTracking.Contracts;

namespace OrderBus
{
    class Program
    {
        public static async Task Main()
        {
            var machine = new OrderStateMachine();
            var repository = new InMemorySagaRepository<OrderState>();

            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.ReceiveEndpoint("order", e => 
                {
                    e.StateMachineSaga(machine, repository);
                });
            });

            var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await busControl.StartAsync(source.Token);

            try
            {
                Console.WriteLine("Press enter to exit");

                await Task.Run(() => Console.ReadLine());
            }
            finally
            {
                await busControl.StopAsync();
            }
        }
    }
}
