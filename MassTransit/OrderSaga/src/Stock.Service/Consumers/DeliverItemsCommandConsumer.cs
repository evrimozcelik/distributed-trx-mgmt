using System;
using MassTransit;
using OrderCommon.Contracts;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using System.Linq;

namespace Stock.Service.Consumers
{
    public class DeliverItemsCommandConsumer : IConsumer<IDeliverItems>
    {

        public async Task Consume(ConsumeContext<IDeliverItems> context)
        {
            var orderCommand = context.Message;

            Log.Information($"OrderId: {orderCommand.OrderId} is received.");

            var accepted = true;
            var reason = "";

            if (orderCommand.Items?.Count == 0)
            {
                accepted = false;
                reason = "no items";
            }
            else if(orderCommand.Items.Any(item => item.Quantity > 5))
            {
                accepted = false;
                reason = "not enough stock";
            }


            if (accepted)
            {
                await context.Publish<IDeliveryCompleted>(new { OrderId = orderCommand.OrderId });
                Log.Information($"Delivery for OrderId: {orderCommand.OrderId} is completed.");
            }
            else
            {
                await context.Publish<IDeliveryFailed>(new { OrderId = orderCommand.OrderId, Reason = reason });
                Log.Information($"Delivery for OrderId: {orderCommand.OrderId} is failed. Reason: {reason}");
            }
        }
        
    }
}