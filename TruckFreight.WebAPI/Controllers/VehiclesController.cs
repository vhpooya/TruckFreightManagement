using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Vehicles.Commands.AddVehicle;
using TruckFreight.Application.Features.Vehicles.Commands.UpdateVehicle;
using TruckFreight.Application.Features.Vehicles.Commands.DeleteVehicle;
using TruckFreight.Application.Features.Vehicles.Queries.GetDriverVehicles;
using TruckFreight.Application.Features.Vehicles.Queries.GetVehicleDetails;

namespace TruckFreight.WebAPI.Controllers
{
    [Authorize(Roles = "Driver")]
    public class VehiclesController : BaseController
    {
        /// <summary>
        /// Add a new vehicle
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] AddVehicleCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Get driver's vehicles
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetDriverVehicles()
        {
            var query = new GetDriverVehiclesQuery();
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Get vehicle details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetDetails(Guid id)
        {
            var query = new GetVehicleDetailsQuery { VehicleId = id };
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Update vehicle information
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] UpdateVehicleCommand command)
        {
            command.VehicleId = id;
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Delete vehicle
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var command = new DeleteVehicleCommand { VehicleId = id };
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Upload vehicle documents
        /// </summary>
        [HttpPost("{id}/documents")]
        public async Task<ActionResult> UploadDocuments(Guid id, List<IFormFile> files, [FromForm] string documentType)
        {
            var command = new UploadVehicleDocumentsCommand 
            { 
                VehicleId = id,
                Files = files,
                DocumentType = documentType
            };
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Update vehicle insurance
        /// </summary>
        [HttpPost("{id}/insurance")]
        public async Task<ActionResult> UpdateInsurance(Guid id, [FromBody] UpdateVehicleInsuranceCommand command)
        {
            command.VehicleId = id;
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Update vehicle inspection
        /// </summary>
        [HttpPost("{id}/inspection")]
        public async Task<ActionResult> UpdateInspection(Guid id, [FromBody] UpdateVehicleInspectionCommand command)
        {
            command.VehicleId = id;
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }
}

/