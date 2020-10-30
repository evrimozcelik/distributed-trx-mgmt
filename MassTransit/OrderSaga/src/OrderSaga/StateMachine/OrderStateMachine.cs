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
        public State Rejected { get; private set; }
        public State PaymentCompleted { get; private set; }
        public State PaymentFailed { get; private set; }
        public State Delivered { get; private set; }
        public State DeliveryFailed { get; private set; }

        public Event<ISubmitOrder> SubmitOrderEvent { get; private set; }
        public Event<IOrderAccepted> OrderAcceptedEvent { get; private set; }
        public Event<IOrderRejected> OrderRejectedEvent { get; private set; }
        public Event<IPaymentCompleted> PaymentCompletedEvent { get; private set; }
        public Event<IPaymentFailed> PaymentFailedEvent { get; private set; }
        public Event<IDeliveryCompleted> DeliveryCompletedEvent { get; private set; }
        public Event<IDeliveryFailed> DeliveryFailedEvent { get; private set; }

        private ILogger<OrderStateMachine> _logger;

        public OrderStateMachine(ILogger<OrderStateMachine> logger)
        {
            _logger = logger;

            InstanceState(x => x.CurrentState);

            Event(() => SubmitOrderEvent, x => x.CorrelateById(context => context.Message.OrderId).SelectId(c =>
                {
                    if (c.Message.OrderId != null && c.Message.OrderId != Guid.Empty)
                        return c.Message.OrderId;
                    else
                        return Guid.NewGuid();
                }));

            Event(() => OrderAcceptedEvent, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => OrderRejectedEvent, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => PaymentCompletedEvent, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => PaymentFailedEvent, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => DeliveryCompletedEvent, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => DeliveryFailedEvent, x => x.CorrelateById(context => context.Message.OrderId));

            // TODO: write handler classes to delegate actions
            Initially(
                When(SubmitOrderEvent)
                    .Then(x => x.Instance.CustomerId = x.Data.CustomerId)
                    .Then(x => x.Instance.Items = x.Data.Items)
                    .Then(x => x.Instance.OrderServiceFailCount = x.Data.OrderServiceFailCount)
                    .Then(context => LogStateChange<ISubmitOrder>(context))
                    .TransitionTo(Submitted)
                    .Activity(x => x.OfInstanceType<SendAcceptOrderCommandActivity>())
                    .Publish(context => (IOrderReceived)new OrderReceived { OrderId = context.Instance.CorrelationId })
                );

            // TODO: check for eliminate the need to create a class while publishing events
            During(Submitted,
                When(OrderAcceptedEvent)
                    .Then(context => LogStateChange<IOrderAccepted>(context))
                    .TransitionTo(Accepted)
                    .Activity(x => x.OfInstanceType<SendExecutePaymentCommandActivity>()),
                When(OrderRejectedEvent)
                    .Then(context => LogStateChange<IOrderRejected>(context))
                    .TransitionTo(Rejected)
                    .Publish(context => (IOrderFailed)new OrderFailed { OrderId = context.Instance.CorrelationId })
                    .Finalize()
                );

            During(Accepted,
                When(PaymentCompletedEvent)
                    .Then(context => LogStateChange<IPaymentCompleted>(context))
                    .TransitionTo(PaymentCompleted)
                    .Activity(x => x.OfInstanceType<SendDeliverItemsCommandActivity>()),
                When(PaymentFailedEvent)
                    .Then(context => LogStateChange<IPaymentFailed>(context))
                    .TransitionTo(PaymentFailed)
                    .Publish(context => (IOrderFailed)new OrderFailed { OrderId = context.Instance.CorrelationId })
                    .Finalize()
                );

            During(PaymentCompleted,
                When(DeliveryCompletedEvent)
                    .Then(context => LogStateChange<IDeliveryCompleted>(context))
                    .TransitionTo(Delivered)
                    .Publish(context => (IOrderCompleted)new OrderCompleted { OrderId = context.Instance.CorrelationId })
                    .Finalize(),
                When(DeliveryFailedEvent)
                    .Then(context => LogStateChange<IDeliveryFailed>(context))
                    .TransitionTo(DeliveryFailed)
                    .Publish(context => (IOrderFailed)new OrderFailed { OrderId = context.Instance.CorrelationId })
                    .Finalize()
                );

            // TODO: Log state changes effectively
            WhenEnterAny(b => b.Then(context => LogStateChange2(context.Event.Name, context.Instance)));

        }

        private void LogStateChange<T>(BehaviorContext<OrderState,T> context)
        {
            var eventName = context.Event.Name;
            var state = context.Instance;
            var currentState = context.Instance.CurrentState;
            var eventMessage = context.Data;

            _logger.LogInformation("State Changed: {0} -> {1}. Instance: {2}. Event: {3}", currentState, eventName, JsonSerializer.Serialize(state), JsonSerializer.Serialize(eventMessage));
        }

        private void LogStateChange2(string eventName, OrderState state)
        {
            _logger.LogInformation("State Enter: {0}. Instance: {1}", eventName, JsonSerializer.Serialize(state));
        }
    }
}