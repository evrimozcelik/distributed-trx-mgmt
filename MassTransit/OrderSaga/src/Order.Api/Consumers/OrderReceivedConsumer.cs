using System;
using MassTransit;
using OrderCommon.Contracts;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Order.Api.Consumers
{
    public class OrderReceivedConsumer : IConsumer<IOrderReceived>
    {

        public async Task Consume(ConsumeContext<IOrderReceived> context)
        {
            var orderCommand = context.Message;

            await Console.Out.WriteLineAsync($"OrderReceivedConsumer. OrderId: {orderCommand.OrderId} is received.");

        }
        
    }
}