using System;
namespace OrderCommon.Contracts
{
    public class OrderCompleted : IOrderCompleted
    {
        public Guid OrderId { get; set;}   
    }
}