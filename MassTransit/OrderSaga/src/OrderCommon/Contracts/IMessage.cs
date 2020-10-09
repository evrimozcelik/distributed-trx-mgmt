using System;
namespace OrderCommon.Contracts
{
    public interface IMessage
    {
        Guid OrderId { get; }   
    }
}