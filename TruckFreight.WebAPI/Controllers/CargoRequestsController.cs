using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.CargoRequests.Commands;
using TruckFreight.Application.Features.CargoRequests.Queries;
using TruckFreight.Application.Services;

namespace TruckFreight.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CargoRequestsController : ControllerBase
    {
        private readonly ICargoRequestApplicationService _cargoRequestService;
        private readonly ILogger<CargoRequestsController> _logger;

        public CargoRequestsController(
            ICargoRequestApplicationService cargoRequestService,
            ILogger<CargoRequestsController> logger)
        {
            _cargoRequestService = cargoRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new cargo request
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "CargoOwner")]
        public async Task<ActionResult> Create([FromBody] CreateCargoRequestCommand command)
        {
            try
            {
                var result = await _cargoRequestService.CreateCargoRequestAsync(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating cargo request");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Publish a cargo request
        /// </summary>
        [HttpPost("{id}/publish")]
        [Authorize(Roles = "CargoOwner")]
        public async Task<ActionResult> Publish(Guid id, [FromBody] PublishCargoRequestRequest request)
        {
            try
            {
                var command = new PublishCargoRequestCommand 
                { 
                    CargoRequestId = id,
                    ExpiresAt = request.ExpiresAt
                };
                var result = await _cargoRequestService.PublishCargoRequestAsync(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while publishing cargo request {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get cargo requests with filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetCargoRequests([FromQuery] GetCargoRequestsQuery query)
        {
            try
            {
                var result = await _cargoRequestService.GetCargoRequestsAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching cargo requests");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get cargo request details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetDetails(Guid id)
        {
            try
            {
                var query = new GetCargoRequestDetailsQuery { Id = id };
                var result = await _cargoRequestService.GetCargoRequestByIdAsync(query);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching cargo request {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Search cargo requests near driver's location
        /// </summary>
        [HttpGet("nearby")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> GetNearbyRequests([FromQuery] GetNearbyCargoRequestsQuery query)
        {
            try
            {
                var result = await _cargoRequestService.GetNearbyCargoRequestsAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching nearby cargo requests");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get cargo owner's requests
        /// </summary>
        [HttpGet("my-requests")]
        [Authorize(Roles = "CargoOwner")]
        public async Task<ActionResult> GetMyRequests([FromQuery] GetMyCargoRequestsQuery query)
        {
            try
            {
                var result = await _cargoRequestService.GetMyCargoRequestsAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching my cargo requests");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update cargo request
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "CargoOwner")]
        public async Task<ActionResult> Update(Guid id, [FromBody] UpdateCargoRequestCommand command)
        {
            try
            {
                command.Id = id;
                var result = await _cargoRequestService.UpdateCargoRequestAsync(command);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating cargo request {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Cancel cargo request
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "CargoOwner")]
        public async Task<ActionResult> Cancel(Guid id, [FromBody] CancelCargoRequestCommand command)
        {
            try
            {
                command.Id = id;
                var result = await _cargoRequestService.CancelCargoRequestAsync(command);
                if (!result)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while canceling cargo request {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Upload cargo images
        /// </summary>
        [HttpPost("{id}/images")]
        [Authorize(Roles = "CargoOwner")]
        public async Task<ActionResult> UploadImages(Guid id, List<IFormFile> files)
        {
            try
            {
                var command = new UploadCargoImagesCommand 
                { 
                    CargoRequestId = id,
                    Files = files 
                };
                var result = await _cargoRequestService.UploadCargoImagesAsync(command);
                if (string.IsNullOrEmpty(result))
                {
                    return NotFound();
                }
                return Ok(new { imageUrls = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading cargo images for request {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Assign driver to cargo request
        /// </summary>
        [HttpPost("{id}/assign-driver")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AssignDriver(Guid id, [FromBody] AssignDriverCommand command)
        {
            try
            {
                command.CargoRequestId = id;
                var result = await _cargoRequestService.AssignDriverAsync(command);
                if (!result)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning driver to cargo request {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update cargo request status
        /// </summary>
        [HttpPost("{id}/status")]
        public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateCargoRequestStatusCommand command)
        {
            try
            {
                command.CargoRequestId = id;
                var result = await _cargoRequestService.UpdateStatusAsync(command);
                if (!result)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating status for cargo request {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update cargo request location
        /// </summary>
        [HttpPost("{id}/location")]
        public async Task<ActionResult> UpdateLocation(Guid id, [FromBody] UpdateCargoRequestLocationCommand command)
        {
            try
            {
                command.CargoRequestId = id;
                var result = await _cargoRequestService.UpdateLocationAsync(command);
                if (!result)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating location for cargo request {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class PublishCargoRequestRequest
    {
        public DateTime? ExpiresAt { get; set; }
    }
}

/