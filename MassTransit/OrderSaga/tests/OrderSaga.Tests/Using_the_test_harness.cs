using System;
using System.Threading.Tasks;
using MassTransit.Testing;
using NUnit.Framework;

namespace OrderSaga.Tests
{
    public class Using_the_test_harness
    {

        [Test]
        public async Task Should_run_InMemoryTestHarness()
        {
            var harness = new InMemoryTestHarness();

            await harness.Start();

            try
            {
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
}
