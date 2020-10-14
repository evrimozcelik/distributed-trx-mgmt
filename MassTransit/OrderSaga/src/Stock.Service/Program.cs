using System;
using System.Threading;
using System.Threading.Tasks;
using Stock.Service.Consumers;
using MassTransit;
using MassTransit.Saga;
using OrderCommon.Models;
using OrderCommon.Contracts;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Stock.Service
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                    theme: ConsoleTheme.None)
                .CreateLogger();

            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.ReceiveEndpoint("stock-service", e =>
                {
                    e.Consumer<DeliverItemsCommandConsumer>();
                });

            });

            var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await busControl.StartAsync(source.Token);

            try
            {
                Log.Information("Stock Service Started");
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
