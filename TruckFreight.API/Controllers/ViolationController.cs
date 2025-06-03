using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Violations.Commands.CreateViolation;
using TruckFreight.Application.Features.Violations.Commands.UpdateViolationStatus;
using TruckFreight.Application.Features.Violations.DTOs;
using TruckFreight.Application.Features.Violations.Queries.GetViolationDetails;
using TruckFreight.Application.Features.Violations.Queries.GetViolations;

namespace TruckFreight.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ViolationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ViolationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new violation record
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Create a new violation",
            Description = "Creates a new violation record. Only administrators can create violations.",
            OperationId = "CreateViolation",
            Tags = new[] { "Violations" }
        )]
        [SwaggerResponse(200, "Violation created successfully", typeof(ViolationDto))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(401, "Unauthorized")]
        [SwaggerResponse(403, "Forbidden")]
        public async Task<ActionResult<Result<ViolationDto>>> CreateViolation([FromBody] CreateViolationDto violation)
        {
            var command = new CreateViolationCommand { Violation = violation };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Updates the status of a violation
        /// </summary>
        [HttpPut("status")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Update violation status",
            Description = "Updates the status of a violation. Only administrators can update violation status.",
            OperationId = "UpdateViolationStatus",
            Tags = new[] { "Violations" }
        )]
        [SwaggerResponse(200, "Violation status updated successfully", typeof(ViolationDto))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(401, "Unauthorized")]
        [SwaggerResponse(403, "Forbidden")]
        [SwaggerResponse(404, "Violation not found")]
        public async Task<ActionResult<Result<ViolationDto>>> UpdateViolationStatus([FromBody] UpdateViolationStatusDto statusUpdate)
        {
            var command = new UpdateViolationStatusCommand { StatusUpdate = statusUpdate };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Gets a list of violations with optional filtering
        /// </summary>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get violations",
            Description = "Retrieves a paginated list of violations with optional filtering by status and date range.",
            OperationId = "GetViolations",
            Tags = new[] { "Violations" }
        )]
        [SwaggerResponse(200, "Violations retrieved successfully", typeof(ViolationListDto))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(401, "Unauthorized")]
        public async Task<ActionResult<Result<ViolationListDto>>> GetViolations(
            [FromQuery] string status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetViolationsQuery
            {
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets detailed information about a specific violation
        /// </summary>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get violation details",
            Description = "Retrieves detailed information about a specific violation.",
            OperationId = "GetViolationDetails",
            Tags = new[] { "Violations" }
        )]
        [SwaggerResponse(200, "Violation details retrieved successfully", typeof(ViolationDetailsDto))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(401, "Unauthorized")]
        [SwaggerResponse(403, "Forbidden")]
        [SwaggerResponse(404, "Violation not found")]
        public async Task<ActionResult<Result<ViolationDetailsDto>>> GetViolationDetails(Guid id)
        {
            var query = new GetViolationDetailsQuery { ViolationId = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
} 