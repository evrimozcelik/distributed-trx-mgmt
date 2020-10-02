using System;
using System.Collections.Generic;
using OrderTracking.Models;

namespace OrderTracking.Contracts
{
    public interface ISubmitOrder
    {
        Guid OrderId { get; }    
        int CustomerId { get; set; }
        List<OrderItem> Items { get; set; }
         
    }
}