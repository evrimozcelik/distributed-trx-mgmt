using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OrderCommon.Contracts;
using OrderCommon.Models;
using OrderSaga.StateMachine;

namespace OrderSaga.Tests
{
    public class OrderSagaTest
    {
        [Test]
        public async Task Should_test_order_saga()
        {

            var collection = new ServiceCollection();
            collection.AddLogging(c => c.AddConsole());
            collection.RegisterSagaStateMachine<OrderStateMachine, OrderState>();
            collection.AddScoped<SendAcceptOrderCommandActivity>();
            collection.AddMassTransitInMemoryTestHarness(x =>
            {
                x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                    .InMemoryRepository();

                x.AddSagaStateMachineTestHarness<OrderStateMachine, OrderState>();
            });

            var serviceProvider = collection.BuildServiceProvider();

            var harness = serviceProvider.GetRequiredService<InMemoryTestHarness>();
            var machine = serviceProvider.GetRequiredService<OrderStateMachine>();

            harness.OnConfigureInMemoryReceiveEndpoint += cfg =>
                cfg.StateMachineSaga(machine, serviceProvider);

            await harness.Start();

            var sagaHarness = serviceProvider.GetRequiredService<IStateMachineSagaTestHarness<OrderState, OrderStateMachine>>();

            try
            {
                Guid sagaId = NewId.NextGuid();
                Console.WriteLine($"sagaId: {sagaId}");

                //var sendEndpoint = await harness.Bus.GetSendEndpoint(new Uri("queue:order-saga"));
                //await sendEndpoint.Send<ISubmitOrder>(new {Customer="1"});

                await harness.Bus.Publish<ISubmitOrder>(new { OrderId = sagaId, Customer = "1" });
                //await harness.BusSendEndpoint.Send<ISubmitOrder>(new { OrderId = sagaId, Customer = "1" });

                // did the endpoint consume the message
                Assert.IsTrue(harness.Consumed.Select<ISubmitOrder>().Any());

                // did the actual consumer consume the message
                Assert.IsTrue(sagaHarness.Consumed.Select<ISubmitOrder>().Any());

                var instance = sagaHarness.Created.ContainsInState(sagaId, machine, machine.Submitted);
                Assert.IsNotNull(instance, "Saga instance not found");

            }
            finally
            {
                await harness.Stop();
            }

        }

    }
}
