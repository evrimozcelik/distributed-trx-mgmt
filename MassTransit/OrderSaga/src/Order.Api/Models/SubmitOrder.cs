using System.Collections.Generic;
using System;
using OrderCommon.Contracts;
using OrderCommon.Models;

namespace Order.Api.Models
{
    public class SubmitOrder : ISubmitOrder
    {
        public Guid OrderId { get; }    
        public int CustomerId { get; set; }
        public List<OrderItem> Items { get; set; }
        public int OrderServiceFailCount { get; set; }
    }
}