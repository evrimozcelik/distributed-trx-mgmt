using System;
namespace OrderTracking.Contracts
{
    public interface IOrderReceived
    {
         Guid OrderId { get; }   
    }
}