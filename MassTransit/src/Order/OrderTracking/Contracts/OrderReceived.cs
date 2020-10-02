using System;
namespace OrderTracking.Contracts
{
    public class OrderReceived : IOrderReceived
    {
        public Guid OrderId { get; set;}   
    }
}