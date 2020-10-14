using System;
using MassTransit;
using OrderCommon.Contracts;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using System.Linq;

namespace Order.Service.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<IPaymentFailed>
    {

        public async Task Consume(ConsumeContext<IPaymentFailed> context)
        {
            await Task.CompletedTask;

            var orderCommand = context.Message;

            Log.Information($"Payment failed event received for OrderId: {orderCommand.OrderId}");

        }
        
    }
}