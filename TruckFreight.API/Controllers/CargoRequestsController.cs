using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.CargoRequests.Commands.CancelCargoRequest;
using TruckFreight.Application.Features.CargoRequests.Commands.CreateCargoRequest;
using TruckFreight.Application.Features.CargoRequests.Commands.UpdateCargoRequest;
using TruckFreight.Application.Features.CargoRequests.DTOs;
using TruckFreight.Application.Features.CargoRequests.Queries.GetCargoRequests;

namespace TruckFreight.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class CargoRequestsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CargoRequestsController> _logger;

        public CargoRequestsController(IMediator mediator, ILogger<CargoRequestsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a new cargo request",
            Description = "Creates a new cargo request with the provided details",
            OperationId = "CreateCargoRequest",
            Tags = new[] { "Cargo Requests" }
        )]
        [SwaggerResponse(200, "Cargo request created successfully", typeof(CargoRequestDto))]
        [SwaggerResponse(400, "Invalid input data")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized")]
        public async Task<ActionResult<Result<CargoRequestDto>>> CreateCargoRequest([FromBody] CreateCargoRequestCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                if (result.Succeeded)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cargo request");
                return StatusCode(500, Result.Failure("Error creating cargo request"));
            }
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Update an existing cargo request",
            Description = "Updates an existing cargo request with the provided details",
            OperationId = "UpdateCargoRequest",
            Tags = new[] { "Cargo Requests" }
        )]
        [SwaggerResponse(200, "Cargo request updated successfully", typeof(CargoRequestDto))]
        [SwaggerResponse(400, "Invalid input data")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized")]
        [SwaggerResponse(404, "Cargo request not found")]
        public async Task<ActionResult<Result<CargoRequestDto>>> UpdateCargoRequest(Guid id, [FromBody] UpdateCargoRequestDto cargoRequest)
        {
            try
            {
                var command = new UpdateCargoRequestCommand
                {
                    CargoRequest = cargoRequest
                };
                command.CargoRequest.Id = id;

                var result = await _mediator.Send(command);
                if (result.Succeeded)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cargo request");
                return StatusCode(500, Result.Failure("Error updating cargo request"));
            }
        }

        [HttpPost("{id}/cancel")]
        [SwaggerOperation(
            Summary = "Cancel a cargo request",
            Description = "Cancels an existing cargo request with the provided reason",
            OperationId = "CancelCargoRequest",
            Tags = new[] { "Cargo Requests" }
        )]
        [SwaggerResponse(200, "Cargo request cancelled successfully")]
        [SwaggerResponse(400, "Invalid input data")]
        [SwaggerResponse(401, "User not authenticated")]
        [SwaggerResponse(403, "User not authorized")]
        [SwaggerResponse(404, "Cargo request not found")]
        public async Task<ActionResult<Result>> CancelCargoRequest(Guid id, [FromBody] CancelCargoRequestCommand command)
        {
            try
            {
                command.Id = id;
                var result = await _mediator.Send(command);
                if (result.Succeeded)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling cargo request");
                return StatusCode(500, Result.Failure("Error cancelling cargo request"));
            }
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get cargo requests",
            Description = "Gets a paginated list of cargo requests with optional filtering",
            OperationId = "GetCargoRequests",
            Tags = new[] { "Cargo Requests" }
        )]
        [SwaggerResponse(200, "Cargo requests retrieved successfully", typeof(CargoRequestListDto))]
        [SwaggerResponse(400, "Invalid input data")]
        [SwaggerResponse(401, "User not authenticated")]
        public async Task<ActionResult<Result<CargoRequestListDto>>> GetCargoRequests([FromQuery] GetCargoRequestsQuery query)
        {
            try
            {
                var result = await _mediator.Send(query);
                if (result.Succeeded)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cargo requests");
                return StatusCode(500, Result.Failure("Error getting cargo requests"));
            }
        }
    }
} 