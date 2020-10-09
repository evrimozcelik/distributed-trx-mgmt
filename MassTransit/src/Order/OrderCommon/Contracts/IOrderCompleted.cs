using System;
namespace OrderCommon.Contracts
{
    public interface IOrderCompleted
    {
         Guid OrderId { get; }   
    }
}