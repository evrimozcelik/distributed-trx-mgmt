using System;
using MassTransit;
using OrderCommon.Contracts;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

namespace Order.Service.Consumers
{
    public class AcceptOrderCommandConsumer : IConsumer<IAcceptOrder>
    {

        public async Task Consume(ConsumeContext<IAcceptOrder> context)
        {
            var orderCommand = context.Message;

            Log.Information($"AcceptOrderConsumer. IAcceptOrder: {JsonSerializer.Serialize(orderCommand)} is received.");

            await context.Publish<IOrderAccepted>(new { OrderId = context.CorrelationId });
        }
        
    }
}