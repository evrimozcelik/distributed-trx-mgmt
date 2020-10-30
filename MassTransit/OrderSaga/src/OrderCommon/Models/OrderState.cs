using System.Collections.Generic;
using System;
using Automatonymous;

namespace OrderCommon.Models
{

    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }

        public int CustomerId { get; set; }
        public List<OrderItem> Items { get; set; }
        public int OrderServiceFailCount { get; set; }

    }

}