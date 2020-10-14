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
    public class ExecutePaymentCommandConsumer : IConsumer<IExecutePayment>
    {

        public async Task Consume(ConsumeContext<IExecutePayment> context)
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
            else if(orderCommand.Items.Sum(item => item.Quantity) > 10)
            {
                accepted = false;
                reason = "not enough credit";
            }


            if (accepted)
            {
                await context.Publish<IPaymentCompleted>(new { OrderId = orderCommand.OrderId });
                Log.Information($"Payment for OrderId: {orderCommand.OrderId} is completed.");
            }
            else
            {
                await context.Publish<IPaymentFailed>(new { OrderId = orderCommand.OrderId, Reason = reason });
                Log.Information($"Payment for OrderId: {orderCommand.OrderId} is failed. Reason: {reason}");
            }
        }
        
    }
}