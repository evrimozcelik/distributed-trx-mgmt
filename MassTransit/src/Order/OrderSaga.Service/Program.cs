using System.Threading;
using System.Threading.Tasks;
using System;
using MassTransit;
using MassTransit.Saga;
using OrderSaga.StateMachine;
using OrderCommon.Models;
using OrderCommon.Contracts;
using Serilog;

namespace OrderSaga.Service
{
    class Program
    {
        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

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
                Log.Information("Order Saga Service Started");
                Log.Information("Press enter to exit");

                await Task.Run(() => Console.ReadLine());
            }
            finally
            {
                await busControl.StopAsync();
            }
        }
    }
}
