using System;
namespace OrderCommon.Contracts
{
    public interface IPaymentCompleted
    {
         Guid OrderId { get; }   
    }
}