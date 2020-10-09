﻿using System.Threading;
using System.Threading.Tasks;
using System;
using MassTransit;
using MassTransit.Saga;
using OrderCommon.Models;
using OrderCommon.Contracts;
using Serilog;
using Order.Service.Consumers;
using Serilog.Sinks.SystemConsole.Themes;

namespace Order.Service
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
                cfg.ReceiveEndpoint("order-service", e => 
                {
                    e.Consumer<AcceptOrderCommandConsumer>();
                });

            });

            var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await busControl.StartAsync(source.Token);

            try
            {
                Log.Information("Order Service Started");
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
