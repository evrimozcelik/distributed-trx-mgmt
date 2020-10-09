using System.Threading.Tasks;
using System;
using MassTransit;

namespace Sample
{

    public class SimpleQueue
    {

        public async static Task run()
        {
            Console.WriteLine("Hello World!");

            var inMemoryBus = Bus.Factory.CreateUsingInMemory(sbc => 
            {
                sbc.ReceiveEndpoint("test-queue", ep => 
                {
                    ep.Handler<Message>(context => 
                    {
                        return Console.Out.WriteLineAsync($"InMemory Received: {context.Message.Text}");
                    });
                });
            });

            var rabbitmqBus = Bus.Factory.CreateUsingRabbitMq(sbc => 
            {
                sbc.Host("rabbitmq://localhost");

                sbc.ReceiveEndpoint("test-queue", ep => 
                {
                    ep.Handler<Message>(context => 
                    {
                        return Console.Out.WriteLineAsync($"RabbitMQ Received: {context.Message.Text}");
                    });
                });
            });

            await inMemoryBus.StartAsync();

            await rabbitmqBus.StartAsync();


            await inMemoryBus.Publish(new Message() {Text="First message"});

            await rabbitmqBus.Publish(new Message() {Text="First message"});

            Console.WriteLine("Press 'q' to exit");

            string input = null;
            while(input != "q") 
            {
                input = Console.ReadLine();
                await inMemoryBus.Publish(new Message() {Text=input});
                await rabbitmqBus.Publish(new Message() {Text=input});
            }

            await inMemoryBus.StopAsync();
            await rabbitmqBus.StopAsync();
        }

    }


}




