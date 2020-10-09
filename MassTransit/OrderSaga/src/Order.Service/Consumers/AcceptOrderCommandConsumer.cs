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

            Log.Information($"OrderId: {orderCommand.OrderId} is received.");

            var accepted = true;
            var reason = "";

            if (orderCommand.Items == null || orderCommand.Items.Count == 0)
            {
                accepted = false;
                reason = "missing items";
            }


            if (accepted)
            {
                await context.Publish<IOrderAccepted>(new { OrderId = orderCommand.OrderId });
                Log.Information($"OrderId: {orderCommand.OrderId} is accepted.");
            }
            else
            {
                await context.Publish<IOrderRejected>(new { OrderId = orderCommand.OrderId, Reason = reason });
                Log.Information($"OrderId: {orderCommand.OrderId} is rejected.");
            }
        }
        
    }
}