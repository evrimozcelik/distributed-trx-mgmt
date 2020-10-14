using System.Threading;
using System.Threading.Tasks;
using System;
using MassTransit;
using MassTransit.Saga;
using OrderCommon.Models;
using OrderCommon.Contracts;
using Serilog;
using Order.Service.Consumers;
using Serilog.Sinks.SystemConsole.Themes;
using GreenPipes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Order.Service
{
    class Program
    {
        static async Task Main(string[] args)
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

            var logger = provider.GetRequiredService<ILogger<Program>>();

            var token = new CancellationTokenSource().Token;

            try
            {
                logger.LogInformation("Order Service Started");

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
                    Uri schedulerEndpoint = new Uri("queue:scheduler");

                    services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();

                        x.AddMessageScheduler(schedulerEndpoint);

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.UseMessageScheduler(schedulerEndpoint);

                            cfg.ReceiveEndpoint("order-service", e =>
                            {
                                // TODO: scheduled re-delivery is not working
                                e.UseScheduledRedelivery(r => r.Intervals(TimeSpan.FromSeconds(10)));
                                e.UseMessageRetry(r => r.Immediate(3));

                                e.Consumer<AcceptOrderCommandConsumer>();
                                e.Consumer<PaymentFailedEventConsumer>();
                                e.Consumer<DeliveryFailedEventConsumer>();
                            });

                        });

                    });

                });
        }
    }
}
