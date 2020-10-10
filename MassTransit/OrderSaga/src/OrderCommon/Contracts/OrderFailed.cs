using System;
namespace OrderCommon.Contracts
{
    public class OrderFailed : IOrderFailed
    {
        public Guid OrderId { get; set;}   
    }
}