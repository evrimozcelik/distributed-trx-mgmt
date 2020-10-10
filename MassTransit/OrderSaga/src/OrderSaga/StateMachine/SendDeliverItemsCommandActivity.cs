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
    public class SendExecutePaymentCommandActivity : Activity<OrderState>
    {
        private ILogger<SendExecutePaymentCommandActivity> _logger;

        public SendExecutePaymentCommandActivity(ILogger<SendExecutePaymentCommandActivity> logger)
        {
            _logger = logger;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("send-execute-payment-command");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderState> context, Behavior<OrderState> next)
        {
            await SendExecutePayment(context);     
            await next.Execute(context).ConfigureAwait(false);
        }

        public async Task Execute<T>(BehaviorContext<OrderState, T> context, Behavior<OrderState, T> next)
        {
            await SendExecutePayment(context);
            await next.Execute(context).ConfigureAwait(false);
        }

        private async Task SendExecutePayment(BehaviorContext<OrderState> context)
        {
            var sendEndpoint = await context.GetSendEndpoint(new Uri("queue:payment-service"));
            await sendEndpoint.Send<IExecutePayment>(new
            {
                OrderId = context.Instance.CorrelationId,
                CustomerId = context.Instance.CustomerId,
                Items = context.Instance.Items
            }).ConfigureAwait(false);

            _logger.LogInformation("IExecutePayment command was sent. CorrelationId: " + context.Instance.CorrelationId);
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