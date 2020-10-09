using System;
namespace OrderCommon.Contracts
{
    public interface IOrderReceived
    {
         Guid OrderId { get; }   
    }
}