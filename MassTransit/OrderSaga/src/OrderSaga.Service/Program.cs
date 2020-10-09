using System.Threading;
using System.Threading.Tasks;
using System;
using MassTransit;
using MassTransit.Saga;
using OrderCommon.Models;
using OrderSaga.StateMachine;
using Serilog;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;

namespace OrderSaga.Service
{
    class Program
    {
        
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                    theme: ConsoleTheme.None)
                .CreateLogger();

            var host = CreateHostBuilder(args)
                .UseSerilog()
                .Build();

            var provider = host.Services;

            var busControl = provider.GetRequiredService<IBusControl>();

            var logger = provider.GetRequiredService< ILogger<Program>>();

            var token = new CancellationTokenSource().Token;

            try
            {
                logger.LogInformation("Order Saga Service Started");

                await busControl.StartAsync(token);
                await host.RunAsync(token);
            }
            finally
            {
                await busControl.StopAsync();
            }

        }


        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {

                    services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();

                        x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                            .InMemoryRepository();

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.ReceiveEndpoint("order-saga", e =>
                            {
                                e.ConfigureSagas(context);
                            });

                        });

                    });

                    services.AddScoped<SendAcceptOrderCommandActivity>();
                    services.RegisterSagaStateMachine<OrderStateMachine, OrderState>();

                });
        }

    }
}
