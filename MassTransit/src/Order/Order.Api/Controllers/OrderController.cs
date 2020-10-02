using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MassTransit;
using OrderTracking.Contracts;
using Order.Api.Models;

namespace Order.Api.Controllers
{
    [ApiController]
    public class OrderController : ControllerBase
    {

        private readonly IPublishEndpoint _publishEndpoint;
        
        private readonly ILogger<OrderController> _logger;

        public OrderController(IPublishEndpoint publishEndpoint, ILogger<OrderController> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        [HttpPost("/submit")]
        public void Submit(SubmitOrder order)
        {
            _publishEndpoint.Publish<ISubmitOrder>(order);
        }

        [HttpPost("/accept")]
        public void Accept(AcceptOrder accept)
        {
            _publishEndpoint.Publish<IOrderAccepted>(accept);
        }
    }
}
