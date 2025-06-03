using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Features.Drivers.Commands;
using TruckFreight.Application.Features.Drivers.Commands.UpdateDriverLocation;
using TruckFreight.Application.Features.Drivers.Commands.SetDriverAvailability;
using TruckFreight.Application.Features.Drivers.Queries.GetDriverProfile;
using TruckFreight.Application.Features.Drivers.Queries.GetNearbyDrivers;
using TruckFreight.Application.Services;

namespace TruckFreight.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DriversController : ControllerBase
    {
        private readonly IDriverApplicationService _driverService;
        private readonly ILogger<DriversController> _logger;

        public DriversController(
            IDriverApplicationService driverService,
            ILogger<DriversController> logger)
        {
            _driverService = driverService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverCommand command)
        {
            try
            {
                var result = await _driverService.CreateDriverAsync(command);
                return CreatedAtAction(nameof(GetDriver), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating driver");
                return StatusCode(500, "An error occurred while creating the driver");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDriver(int id)
        {
            try
            {
                var result = await _driverService.GetDriverByIdAsync(id);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver");
                return StatusCode(500, "An error occurred while retrieving the driver");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDrivers()
        {
            try
            {
                var result = await _driverService.GetDriversAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drivers");
                return StatusCode(500, "An error occurred while retrieving drivers");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDriver(int id, [FromBody] UpdateDriverCommand command)
        {
            try
            {
                if (id != command.Id)
                    return BadRequest("ID mismatch");

                var result = await _driverService.UpdateDriverAsync(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver");
                return StatusCode(500, "An error occurred while updating the driver");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            try
            {
                await _driverService.DeleteDriverAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting driver");
                return StatusCode(500, "An error occurred while deleting the driver");
            }
        }

        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VerifyDriver(int id)
        {
            try
            {
                var result = await _driverService.VerifyDriverAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying driver");
                return StatusCode(500, "An error occurred while verifying the driver");
            }
        }

        [HttpPost("documents")]
        public async Task<IActionResult> UploadDocuments([FromForm] UploadDriverDocumentsCommand command)
        {
            try
            {
                var result = await _driverService.UploadDocumentsAsync(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading documents");
                return StatusCode(500, "An error occurred while uploading documents");
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateDriverProfileCommand command)
        {
            try
            {
                var result = await _driverService.UpdateProfileAsync(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, "An error occurred while updating the profile");
            }
        }

        [HttpPut("vehicle")]
        public async Task<IActionResult> UpdateVehicleInfo([FromForm] UpdateVehicleInfoCommand command)
        {
            try
            {
                var result = await _driverService.UpdateVehicleInfoAsync(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle info");
                return StatusCode(500, "An error occurred while updating vehicle info");
            }
        }

        [HttpPut("location")]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateDriverLocationCommand command)
        {
            try
            {
                var result = await _driverService.UpdateLocationAsync(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location");
                return StatusCode(500, "An error occurred while updating location");
            }
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateDriverStatusCommand command)
        {
            try
            {
                var result = await _driverService.UpdateStatusAsync(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status");
                return StatusCode(500, "An error occurred while updating status");
            }
        }

        [HttpGet("{id}/nearby-cargo-requests")]
        public async Task<IActionResult> GetNearbyCargoRequests(int id, [FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double radius = 10)
        {
            try
            {
                var result = await _driverService.GetNearbyCargoRequestsAsync(id, latitude, longitude, radius);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nearby cargo requests");
                return StatusCode(500, "An error occurred while retrieving nearby cargo requests");
            }
        }

        /// <summary>
        /// Set driver availability status
        /// </summary>
        [HttpPost("availability")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> SetAvailability([FromBody] SetDriverAvailabilityCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Get driver profile and statistics
        /// </summary>
        [HttpGet("profile")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> GetProfile()
        {
            var query = new GetDriverProfileQuery();
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Get nearby drivers (for admin/cargo owners)
        /// </summary>
        [HttpGet("nearby")]
        [Authorize(Roles = "Admin,CargoOwner")]
        public async Task<ActionResult> GetNearbyDrivers([FromQuery] GetNearbyDriversQuery query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Get driver statistics
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> GetStatistics([FromQuery] GetDriverStatisticsQuery query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Update emergency contact
        /// </summary>
        [HttpPut("emergency-contact")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> UpdateEmergencyContact([FromBody] UpdateEmergencyContactCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }
}

/