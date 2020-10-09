using System.Security.Cryptography.X509Certificates;
using System;
using Automatonymous;
using OrderCommon.Models;
using OrderCommon.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderSaga.StateMachine
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Submitted { get; private set; }
        public State Accepted { get; private set; }

        public Event<ISubmitOrder> SubmitOrder { get; private set; }
        public Event<IOrderAccepted> OrderAccepted { get; private set; }

        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => SubmitOrder, x => x.CorrelateById(context => context.Message.OrderId).SelectId(c => Guid.NewGuid()));
            Event(() => OrderAccepted, x => x.CorrelateById(context => context.Message.OrderId));

            Initially(
                When(SubmitOrder)
                    .Then(x => x.Instance.CustomerId = x.Data.CustomerId)
                    .Then(x => x.Instance.Items = x.Data.Items)
                    .TransitionTo(Submitted)
                    .Then(context => Console.WriteLine("Initially->SubmitOrder. Instance: {0}. Data: {1}", JsonSerializer.Serialize(context.Instance), JsonSerializer.Serialize(context.Data)))
                    .Publish(context => (IOrderReceived)new OrderReceived {OrderId = context.Instance.CorrelationId}),
                When(OrderAccepted)
                    .TransitionTo(Accepted)
                    .Then(context => Console.WriteLine("Initially->OrderAccepted. Instance: {0}. Data: {1}", JsonSerializer.Serialize(context.Instance), JsonSerializer.Serialize(context.Data))));

            During(Submitted,
                When(OrderAccepted)
                    .TransitionTo(Accepted)
                    .Then(context => Console.WriteLine("Submitted->OrderAccepted. Instance: {0}. Data: {1}", JsonSerializer.Serialize(context.Instance), JsonSerializer.Serialize(context.Data))));

            During(Accepted,
                When(SubmitOrder)
                    .Then(context => Console.WriteLine("Accepted->SubmitOrder. Instance: {0}. Data: {1}", JsonSerializer.Serialize(context.Instance), JsonSerializer.Serialize(context.Data))));
        }
    }
}