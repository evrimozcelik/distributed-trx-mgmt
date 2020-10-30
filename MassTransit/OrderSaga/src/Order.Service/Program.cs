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
using Quartz;

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
                    Uri schedulerEndpoint = new Uri("queue:order-service-scheduler");

                    services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();
                        
                        x.AddMessageScheduler(schedulerEndpoint);

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            // Puts failed messages in schedulerEndpoint
                            cfg.UseMessageScheduler(schedulerEndpoint);

                            // Runs Quartz service to listen schedulerEndpoint
                            // Use persistent store in order not to loose any message
                            cfg.UseInMemoryScheduler(schedulerEndpoint.AbsolutePath);

                            cfg.ReceiveEndpoint("order-service", e =>
                            {
                                // Scheduled redelivery configuration
                                e.UseScheduledRedelivery(r => r.Intervals(TimeSpan.FromSeconds(30)));

                                // Retry configuration before fail
                                e.UseMessageRetry(r => r.Immediate(2));
                                
                                e.Consumer<AcceptOrderCommandConsumer>();
                                e.Consumer<PaymentFailedEventConsumer>();
                                e.Consumer<DeliveryFailedEventConsumer>();

                            });

                            cfg.ConfigureEndpoints(context);

                        });

                    });

                });
        }
    }
}
