using System;
using System.Threading.Tasks;
using MassTransit;

namespace Sample
{
    public class Message
    { 
        public string Text { get; set; }
    }

    class Program
    {
        public static async Task Main()
        {
            await SimpleQueue.run();
            //await SimpleStateMachine.run();

            



        }
    }
}
