using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MassTransit;
using OrderCommon.Contracts;
using Order.Api.Models;

namespace Order.Api.Controllers
{
    [ApiController]
    public class OrderController : ControllerBase
    {

        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        
        private readonly ILogger<OrderController> _logger;

        public OrderController(IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpointProvider, ILogger<OrderController> logger)
        {
            _publishEndpoint = publishEndpoint;
            _sendEndpointProvider = sendEndpointProvider;
            _logger = logger;
        }

        [HttpPost("/submit")]
        public async Task Submit(SubmitOrder order)
        {
            //_publishEndpoint.Publish<ISubmitOrder>(order);
            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:order"));
            await sendEndpoint.Send<ISubmitOrder>(order);
        }

        [HttpPost("/accept")]
        public void Accept(AcceptOrder accept)
        {
            _publishEndpoint.Publish<IOrderAccepted>(accept);
        }
    }
}
