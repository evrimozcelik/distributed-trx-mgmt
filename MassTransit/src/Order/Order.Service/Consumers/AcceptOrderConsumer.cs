using System;
using MassTransit;
using OrderCommon.Contracts;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Order.Service.Consumers
{
    public class AcceptOrderConsumer : IConsumer<IAcceptOrder>
    {

        public async Task Consume(ConsumeContext<IAcceptOrder> context)
        {
            var orderCommand = context.Message;

            await Console.Out.WriteLineAsync($"AcceptOrderConsumer. orderCommand: {JsonSerializer.Serialize(orderCommand)} is received.");

        }
        
    }
}