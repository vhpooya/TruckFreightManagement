using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Trips.Commands.AcceptTrip;
using TruckFreight.Application.Features.Trips.Commands.RejectTrip;
using TruckFreight.Application.Features.Trips.Commands.StartTrip;
using TruckFreight.Application.Features.Trips.Commands.UpdateTripLocation;
using TruckFreight.Application.Features.Trips.Commands.CompleteTrip;
using TruckFreight.Application.Features.Trips.Queries.GetDriverActiveTrip;
using TruckFreight.Application.Features.Trips.Queries.GetTripDetails;
using TruckFreight.Application.Features.Trips.Queries.GetDriverTrips;

namespace TruckFreight.WebAPI.Controllers
{
    [Authorize]
    public class TripsController : BaseController
    {
        /// <summary>
        /// Accept assigned trip
        /// </summary>
        [HttpPost("{id}/accept")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> Accept(Guid id)
        {
            var command = new AcceptTripCommand { TripId = id };
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Reject assigned trip
        /// </summary>
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> Reject(Guid id, [FromBody] RejectTripCommand command)
        {
            command.TripId = id;
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Start accepted trip
        /// </summary>
        [HttpPost("{id}/start")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> Start(Guid id)
        {
            var command = new StartTripCommand { TripId = id };
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Update trip location (real-time tracking)
        /// </summary>
        [HttpPost("{id}/location")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> UpdateLocation(Guid id, [FromBody] UpdateTripLocationCommand command)
        {
            command.TripId = id;
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Mark trip as loading started
        /// </summary>
        [HttpPost("{id}/start-loading")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> StartLoading(Guid id)
        {
            var command = new StartLoadingCommand { TripId = id };
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Mark trip as loading completed
        /// </summary>
        [HttpPost("{id}/complete-loading")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> CompleteLoading(Guid id, [FromBody] CompleteLoadingCommand command)
        {
            command.TripId = id;
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Mark trip as arrived at destination
        /// </summary>
        [HttpPost("{id}/arrive")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> Arrive(Guid id)
        {
            var command = new ArriveAtDestinationCommand { TripId = id };
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Mark trip as delivered
        /// </summary>
        [HttpPost("{id}/deliver")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> Deliver(Guid id, [FromBody] DeliverTripCommand command)
        {
            command.TripId = id;
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Complete trip
        /// </summary>
        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> Complete(Guid id, [FromBody] CompleteTripCommand command)
        {
            command.TripId = id;
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Get driver's active trip
        /// </summary>
        [HttpGet("active")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> GetActiveTrip()
        {
            var query = new GetDriverActiveTripQuery();
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Get trip details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetDetails(Guid id)
        {
            var query = new GetTripDetailsQuery { TripId = id };
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Get driver's trip history
        /// </summary>
        [HttpGet("history")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> GetDriverTrips([FromQuery] GetDriverTripsQuery query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Get cargo owner's trips
        /// </summary>
        [HttpGet("cargo-owner-trips")]
        [Authorize(Roles = "CargoOwner")]
        public async Task<ActionResult> GetCargoOwnerTrips([FromQuery] GetCargoOwnerTripsQuery query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Get trip tracking history
        /// </summary>
        [HttpGet("{id}/tracking")]
        public async Task<ActionResult> GetTripTracking(Guid id)
        {
            var query = new GetTripTrackingQuery { TripId = id };
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Cancel trip
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> Cancel(Guid id, [FromBody] CancelTripCommand command)
        {
            command.TripId = id;
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }
}

/