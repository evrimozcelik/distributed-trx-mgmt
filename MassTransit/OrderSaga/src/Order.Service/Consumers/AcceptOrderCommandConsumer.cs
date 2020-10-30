using System;
using MassTransit;
using OrderCommon.Contracts;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using System.Linq;
using System.Collections.Generic;

namespace Order.Service.Consumers
{
    public class AcceptOrderCommandConsumer : IConsumer<IAcceptOrder>
    {
        private static Dictionary<Guid, int> counterMap = new Dictionary<Guid, int>();

        public async Task Consume(ConsumeContext<IAcceptOrder> context)
        {
            var orderCommand = context.Message;

            Log.Information($"OrderId: {orderCommand.OrderId} is received. FailCount: {orderCommand.OrderServiceFailCount}");

            var accepted = true;
            var reason = "";

            if (orderCommand.Items?.Count == 0)
            {
                accepted = false;
                reason = "no items";
            }
            else if(orderCommand.Items.Any(item => item.Quantity < 0))
            {
                accepted = false;
                reason = "negative quantity not accepted";
            }
            else if (orderCommand.Items.Any(item => item.Quantity == 0))
            {
                accepted = false;
                reason = "zero quantity not accepted";
            }

            // TODO: Parametrized counter from input message
            var counter = counterMap.GetValueOrDefault(orderCommand.OrderId, 1);
            if(counter < orderCommand.OrderServiceFailCount)
            {
                var msg = $"Error happened when processing OrderId: {orderCommand.OrderId}. Counter: {counter}";
                Log.Information(msg);

                counterMap[orderCommand.OrderId] = counter + 1;

                throw new Exception(msg);
            }


            if (accepted)
            {
                await context.Publish<IOrderAccepted>(new { OrderId = orderCommand.OrderId });
                Log.Information($"OrderId: {orderCommand.OrderId} is accepted.");
            }
            else
            {
                await context.Publish<IOrderRejected>(new { OrderId = orderCommand.OrderId, Reason = reason });
                Log.Information($"OrderId: {orderCommand.OrderId} is rejected. Reason: {reason}");
            }
        }
        
    }
}