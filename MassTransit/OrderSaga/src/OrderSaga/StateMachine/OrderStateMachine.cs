using System.Security.Cryptography.X509Certificates;
using System;
using Automatonymous;
using OrderCommon.Models;
using OrderCommon.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace OrderSaga.StateMachine
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Submitted { get; private set; }
        public State Accepted { get; private set; }

        public Event<ISubmitOrder> SubmitOrder { get; private set; }
        public Event<IOrderAccepted> OrderAccepted { get; private set; }

        private ILogger<OrderStateMachine> _logger;

        public OrderStateMachine(ILogger<OrderStateMachine> logger)
        {
            _logger = logger;

            InstanceState(x => x.CurrentState);

            Event(() => SubmitOrder, x => x.CorrelateById(context => context.Message.OrderId).SelectId(c => Guid.NewGuid()));
            Event(() => OrderAccepted, x => x.CorrelateById(context => context.Message.OrderId));

            Initially(
                When(SubmitOrder)
                    .Then(x => x.Instance.CustomerId = x.Data.CustomerId)
                    .Then(x => x.Instance.Items = x.Data.Items)
                    .Then(context => LogStateChange(context.Event.Name, context.Instance, context.Data))
                    .Activity(x => x.OfInstanceType<SendAcceptOrderCommandActivity>())
                    .Publish(context => (IOrderReceived)new OrderReceived { OrderId = context.Instance.CorrelationId })
                    .TransitionTo(Submitted)
                );

            During(Submitted,
                When(OrderAccepted)
                    .TransitionTo(Accepted)
                    .Then(context => LogStateChange(context.Event.Name, context.Instance, context.Data))
                );

            During(Accepted,
                When(SubmitOrder)
                    .Then(context => LogStateChange(context.Event.Name, context.Instance, context.Data))
                );
        }

        private void LogStateChange(string eventName, OrderState state, IMessage message)
        {
            _logger.LogInformation("{0}->{1}. Instance: {2}. Data: {3}", state.CurrentState, eventName, JsonSerializer.Serialize(state), JsonSerializer.Serialize(message));
        }
    }
}