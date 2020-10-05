using System;
namespace OrderCommon.Contracts
{
    public interface IOrderAccepted
    {
         Guid OrderId { get; }   
    }
}