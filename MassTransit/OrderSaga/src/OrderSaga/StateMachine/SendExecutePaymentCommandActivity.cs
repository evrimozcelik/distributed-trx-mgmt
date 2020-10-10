using System;
using OrderCommon.Models;
using MassTransit;
using GreenPipes;
using Automatonymous;
using System.Threading.Tasks;
using OrderCommon.Contracts;
using Microsoft.Extensions.Logging;

namespace OrderSaga.StateMachine
{
    public class SendAcceptOrderCommandActivity : Activity<OrderState>
    {
        private ILogger<SendAcceptOrderCommandActivity> _logger;

        public SendAcceptOrderCommandActivity(ILogger<SendAcceptOrderCommandActivity> logger)
        {
            _logger = logger;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("send-accept-order-command");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderState> context, Behavior<OrderState> next)
        {
            await SendAcceptOrder(context);     
            await next.Execute(context).ConfigureAwait(false);
        }

        public async Task Execute<T>(BehaviorContext<OrderState, T> context, Behavior<OrderState, T> next)
        {
            await SendAcceptOrder(context);
            await next.Execute(context).ConfigureAwait(false);
        }

        private async Task SendAcceptOrder(BehaviorContext<OrderState> context)
        {
            var sendEndpoint = await context.GetSendEndpoint(new Uri("queue:order-service"));
            await sendEndpoint.Send<IAcceptOrder>(new
            {
                OrderId = context.Instance.CorrelationId,
                CustomerId = context.Instance.CustomerId,
                Items = context.Instance.Items
            }).ConfigureAwait(false);

            _logger.LogInformation("IAcceptOrder command was sent. CorrelationId: " + context.Instance.CorrelationId);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, TException> context, Behavior<OrderState> next) 
            where TException : Exception
        {
            return next.Faulted(context);
        }

        public Task Faulted<T, TException>(BehaviorExceptionContext<OrderState, T, TException> context, Behavior<OrderState, T> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}