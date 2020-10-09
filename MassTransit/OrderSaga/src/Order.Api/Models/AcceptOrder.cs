using System;
using OrderCommon.Contracts;

namespace Order.Api.Models
{
    public class AcceptOrder : IOrderAccepted
    {
        public Guid OrderId { get; set; }   
    }
}