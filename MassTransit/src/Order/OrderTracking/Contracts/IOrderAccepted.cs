using System;
namespace OrderTracking.Contracts
{
    public interface IOrderAccepted
    {
         Guid OrderId { get; }   
    }
}