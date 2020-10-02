using System;
using Automatonymous;
using MassTransit;
using MassTransit.Saga;
using System.Threading.Tasks;

namespace Sample
{

    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
    }

    public interface SubmitOrder
    {
        public Guid OrderId { get; set; }
    }

    public interface OrderAccepted
    {
        public Guid OrderId { get; set; }
    }

    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Submitted {get; private set;}
        public State Accepted {get; private set;}

        public Event<SubmitOrder> SubmitOrder {get; private set;}
        public Event<OrderAccepted> OrderAccepted {get; private set;}

        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => SubmitOrder, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => OrderAccepted, x => x.CorrelateById(context => context.Message.OrderId));

            Initially(
                When(SubmitOrder)
                .TransitionTo(Submitted)
                .Then(context => Console.WriteLine("State: {0}", context.Instance.CurrentState))
            );

            During(Submitted,
                When(OrderAccepted)
                .TransitionTo(Accepted)
                .Then(context => Console.WriteLine("State: {0}", context.Instance.CurrentState))
            );

        }

    }


    public class SimpleStateMachine 
    {
        public static async Task run()
        {

            var machine = new OrderStateMachine();
            var repository = new InMemorySagaRepository<OrderState>();

            var busControl = Bus.Factory.CreateUsingInMemory(cfg => 
            {
                cfg.ReceiveEndpoint("order", e => 
                {
                    e.StateMachineSaga(machine, repository);
                }
                );
            }
            );

            await busControl.StartAsync();

            var orderId = NewId.NextGuid();

            await busControl.Publish<SubmitOrder>(new 
            {
                OrderId = orderId
            });

            await busControl.Publish<OrderAccepted>(new 
            {
                OrderId = orderId
            });

            await busControl.StopAsync();

        }
    }
}