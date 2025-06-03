using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using TruckFreight.Application.Features.Deliveries.Commands.ConfirmDelivery;
using TruckFreight.Application.Features.Deliveries.Commands.CreateDelivery;
using TruckFreight.Application.Features.Deliveries.Commands.UpdateDeliveryStatus;
using TruckFreight.Application.Features.Deliveries.DTOs;
using TruckFreight.Application.Features.Deliveries.Queries.GetDeliveryDetails;
using TruckFreight.Application.Features.Deliveries.Queries.GetDeliveries;

namespace TruckFreight.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class DeliveryController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DeliveryController> _logger;

        public DeliveryController(IMediator mediator, ILogger<DeliveryController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a new delivery",
            Description = "Creates a new delivery for a cargo request",
            OperationId = "CreateDelivery",
            Tags = new[] { "Delivery" }
        )]
        [SwaggerResponse(200, "Delivery created successfully", typeof(DeliveryDto))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(401, "Unauthorized")]
        [SwaggerResponse(403, "Forbidden")]
        public async Task<IActionResult> CreateDelivery([FromBody] CreateDeliveryCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Error);
        }

        [HttpPut("{id}/status")]
        [SwaggerOperation(
            Summary = "Update delivery status",
            Description = "Updates the status of a delivery",
            OperationId = "UpdateDeliveryStatus",
            Tags = new[] { "Delivery" }
        )]
        [SwaggerResponse(200, "Delivery status updated successfully", typeof(DeliveryDto))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(401, "Unauthorized")]
        [SwaggerResponse(403, "Forbidden")]
        [SwaggerResponse(404, "Delivery not found")]
        public async Task<IActionResult> UpdateDeliveryStatus(Guid id, [FromBody] UpdateDeliveryStatusCommand command)
        {
            if (id != command.StatusUpdate.DeliveryId)
            {
                return BadRequest("Delivery ID mismatch");
            }

            var result = await _mediator.Send(command);
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Error);
        }

        [HttpPut("{id}/confirm")]
        [SwaggerOperation(
            Summary = "Confirm delivery",
            Description = "Confirms a delivery with a confirmation code",
            OperationId = "ConfirmDelivery",
            Tags = new[] { "Delivery" }
        )]
        [SwaggerResponse(200, "Delivery confirmed successfully", typeof(DeliveryDto))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(401, "Unauthorized")]
        [SwaggerResponse(403, "Forbidden")]
        [SwaggerResponse(404, "Delivery not found")]
        public async Task<IActionResult> ConfirmDelivery(Guid id, [FromBody] ConfirmDeliveryCommand command)
        {
            if (id != command.Confirmation.DeliveryId)
            {
                return BadRequest("Delivery ID mismatch");
            }

            var result = await _mediator.Send(command);
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Error);
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get deliveries",
            Description = "Gets a list of deliveries with optional filtering",
            OperationId = "GetDeliveries",
            Tags = new[] { "Delivery" }
        )]
        [SwaggerResponse(200, "Deliveries retrieved successfully", typeof(DeliveryListDto))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(401, "Unauthorized")]
        [SwaggerResponse(403, "Forbidden")]
        public async Task<IActionResult> GetDeliveries([FromQuery] GetDeliveriesQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Error);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get delivery details",
            Description = "Gets detailed information about a specific delivery",
            OperationId = "GetDeliveryDetails",
            Tags = new[] { "Delivery" }
        )]
        [SwaggerResponse(200, "Delivery details retrieved successfully", typeof(DeliveryDetailsDto))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(401, "Unauthorized")]
        [SwaggerResponse(403, "Forbidden")]
        [SwaggerResponse(404, "Delivery not found")]
        public async Task<IActionResult> GetDeliveryDetails(Guid id)
        {
            var query = new GetDeliveryDetailsQuery { DeliveryId = id };
            var result = await _mediator.Send(query);
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Error);
        }
    }
} 