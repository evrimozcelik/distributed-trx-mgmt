using System;
using System.Collections.Generic;
using OrderCommon.Models;

namespace OrderCommon.Contracts
{
    public interface ISubmitOrder
    {
        Guid OrderId { get; }    
        int CustomerId { get; set; }
        List<OrderItem> Items { get; set; }
         
    }
}