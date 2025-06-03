using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Features.CargoOwners.Commands;
using TruckFreight.Application.Services;

namespace TruckFreight.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CargoOwnersController : ControllerBase
    {
        private readonly ICargoOwnerApplicationService _cargoOwnerService;
        private readonly ILogger<CargoOwnersController> _logger;

        public CargoOwnersController(
            ICargoOwnerApplicationService cargoOwnerService,
            ILogger<CargoOwnersController> logger)
        {
            _cargoOwnerService = cargoOwnerService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCargoOwner([FromBody] CreateCargoOwnerCommand command)
        {
            try
            {
                var result = await _cargoOwnerService.CreateCargoOwnerAsync(command);
                return CreatedAtAction(nameof(GetCargoOwner), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cargo owner");
                return StatusCode(500, "An error occurred while creating the cargo owner");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCargoOwner(int id)
        {
            try
            {
                var result = await _cargoOwnerService.GetCargoOwnerByIdAsync(id);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cargo owner");
                return StatusCode(500, "An error occurred while retrieving the cargo owner");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCargoOwners()
        {
            try
            {
                var result = await _cargoOwnerService.GetCargoOwnersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cargo owners");
                return StatusCode(500, "An error occurred while retrieving cargo owners");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCargoOwner(int id, [FromBody] UpdateCargoOwnerCommand command)
        {
            try
            {
                if (id != command.Id)
                    return BadRequest("ID mismatch");

                var result = await _cargoOwnerService.UpdateCargoOwnerAsync(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cargo owner");
                return StatusCode(500, "An error occurred while updating the cargo owner");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCargoOwner(int id)
        {
            try
            {
                await _cargoOwnerService.DeleteCargoOwnerAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cargo owner");
                return StatusCode(500, "An error occurred while deleting the cargo owner");
            }
        }

        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VerifyCargoOwner(int id)
        {
            try
            {
                var result = await _cargoOwnerService.VerifyCargoOwnerAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying cargo owner");
                return StatusCode(500, "An error occurred while verifying the cargo owner");
            }
        }

        [HttpPost("documents")]
        public async Task<IActionResult> UploadDocuments([FromForm] UploadCargoOwnerDocumentsCommand command)
        {
            try
            {
                var result = await _cargoOwnerService.UploadDocumentsAsync(command);
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
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateCargoOwnerProfileCommand command)
        {
            try
            {
                var result = await _cargoOwnerService.UpdateProfileAsync(command);
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

        [HttpPut("company")]
        public async Task<IActionResult> UpdateCompanyInfo([FromForm] UpdateCompanyInfoCommand command)
        {
            try
            {
                var result = await _cargoOwnerService.UpdateCompanyInfoAsync(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company info");
                return StatusCode(500, "An error occurred while updating company info");
            }
        }
    }
} 