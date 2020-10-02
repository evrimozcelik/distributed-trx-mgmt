using System;
using OrderTracking.Contracts;

namespace Order.Api.Models
{
    public class AcceptOrder : IOrderAccepted
    {
        public Guid OrderId { get; set; }   
    }
}