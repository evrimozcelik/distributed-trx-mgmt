using System;
using System.Collections.Generic;
using OrderCommon.Models;

namespace OrderCommon.Contracts
{
    public interface ISubmitOrder : IMessage
    {
        int CustomerId { get; set; }
        List<OrderItem> Items { get; set; }

        int OrderServiceFailCount { get; set; }

    }
}