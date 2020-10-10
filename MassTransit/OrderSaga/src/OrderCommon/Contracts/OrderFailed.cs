using System;
namespace OrderCommon.Contracts
{
    public class OrderReceived : IOrderReceived
    {
        public Guid OrderId { get; set;}   
    }
}