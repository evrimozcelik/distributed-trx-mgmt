using System;
using MassTransit;
using OrderCommon.Contracts;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using System.Linq;

namespace Payment.Service.Consumers
{
    public class DeliveryFailedEventConsumer : IConsumer<IDeliveryFailed>
    {

        public async Task Consume(ConsumeContext<IDeliveryFailed> context)
        {
            var orderCommand = context.Message;

            Log.Information($"Delivery failed event received for OrderId: {orderCommand.OrderId}");

        }
        
    }
}