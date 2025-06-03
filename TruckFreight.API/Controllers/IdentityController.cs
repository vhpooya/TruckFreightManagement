using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Identity.Commands.RegisterCargoOwner;
using TruckFreight.Application.Features.Identity.Commands.RegisterCompany;
using TruckFreight.Application.Features.Identity.Commands.RegisterDriver;
using TruckFreight.Application.Features.Identity.Commands.VerifyIdentity;
using TruckFreight.Application.Features.Identity.DTOs;

namespace TruckFreight.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class IdentityController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IdentityController> _logger;

        public IdentityController(IMediator mediator, ILogger<IdentityController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Register a new driver
        /// </summary>
        /// <param name="command">Driver registration details</param>
        /// <returns>Registration result with user ID and verification token</returns>
        [HttpPost("register/driver")]
        [SwaggerOperation(
            Summary = "Register a new driver",
            Description = "Registers a new driver with the provided details and sends a verification email",
            OperationId = "RegisterDriver",
            Tags = new[] { "Identity" }
        )]
        [ProducesResponseType(typeof(Result<IdentityResultDto>), 200)]
        [ProducesResponseType(typeof(Result<IdentityResultDto>), 400)]
        [ProducesResponseType(typeof(Result<IdentityResultDto>), 500)]
        public async Task<ActionResult<Result<IdentityResultDto>>> RegisterDriver(RegisterDriverCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Register a new cargo owner
        /// </summary>
        /// <param name="command">Cargo owner registration details</param>
        /// <returns>Registration result with user ID and verification token</returns>
        [HttpPost("register/cargo-owner")]
        [SwaggerOperation(
            Summary = "Register a new cargo owner",
            Description = "Registers a new cargo owner with the provided details and sends a verification email",
            OperationId = "RegisterCargoOwner",
            Tags = new[] { "Identity" }
        )]
        [ProducesResponseType(typeof(Result<IdentityResultDto>), 200)]
        [ProducesResponseType(typeof(Result<IdentityResultDto>), 400)]
        [ProducesResponseType(typeof(Result<IdentityResultDto>), 500)]
        public async Task<ActionResult<Result<IdentityResultDto>>> RegisterCargoOwner(RegisterCargoOwnerCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Register a new company
        /// </summary>
        /// <param name="command">Company registration details</param>
        /// <returns>Registration result with user ID and verification token</returns>
        [HttpPost("register/company")]
        [SwaggerOperation(
            Summary = "Register a new company",
            Description = "Registers a new company with the provided details and sends a verification email to the representative",
            OperationId = "RegisterCompany",
            Tags = new[] { "Identity" }
        )]
        [ProducesResponseType(typeof(Result<IdentityResultDto>), 200)]
        [ProducesResponseType(typeof(Result<IdentityResultDto>), 400)]
        [ProducesResponseType(typeof(Result<IdentityResultDto>), 500)]
        public async Task<ActionResult<Result<IdentityResultDto>>> RegisterCompany(RegisterCompanyCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Verify user identity
        /// </summary>
        /// <param name="command">Verification details</param>
        /// <returns>Verification result with JWT token</returns>
        [HttpPost("verify")]
        [SwaggerOperation(
            Summary = "Verify user identity",
            Description = "Verifies the user's identity using the provided verification code and updates their status",
            OperationId = "VerifyIdentity",
            Tags = new[] { "Identity" }
        )]
        [ProducesResponseType(typeof(Result<IdentityResultDto>), 200)]
        [ProducesResponseType(typeof(Result<IdentityResultDto>), 400)]
        [ProducesResponseType(typeof(Result<IdentityResultDto>), 404)]
        [ProducesResponseType(typeof(Result<IdentityResultDto>), 500)]
        public async Task<ActionResult<Result<IdentityResultDto>>> VerifyIdentity(VerifyIdentityCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
            {
                if (result.Errors.Contains("User not found"))
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
} 