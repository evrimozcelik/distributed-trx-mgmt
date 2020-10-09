using System;
namespace OrderCommon.Contracts
{
    public interface IItemsDelivered
    {
         Guid OrderId { get; }   
    }
}