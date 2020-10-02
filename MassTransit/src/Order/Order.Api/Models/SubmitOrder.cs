using System.Collections.Generic;
using System;
using OrderTracking.Contracts;
using OrderTracking.Models;

namespace Order.Api.Models
{
    public class SubmitOrder : ISubmitOrder
    {
        public Guid OrderId { get; }    
        public int CustomerId { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}