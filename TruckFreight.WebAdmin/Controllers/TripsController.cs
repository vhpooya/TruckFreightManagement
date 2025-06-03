using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Trips.Queries.SearchTrips;
using TruckFreight.Application.Features.Trips.Commands.AssignDriverToTrip;
using TruckFreight.WebAdmin.Models.Trips;

namespace TruckFreight.WebAdmin.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,Operator")]
    public class TripsController : BaseAdminController
    {
        public async Task<IActionResult> Index(int page = 1, string search = "", string status = "", 
                                              DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = new SearchTripsQuery
            {
                SearchTerm = search,
                Status = string.IsNullOrEmpty(status) ? null : Enum.Parse<Domain.Enums.TripStatus>(status),
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = page,
                PageSize = 20
            };

            var result = await Mediator.Send(query);

            var viewModel = new TripListViewModel
            {
                Trips = result.IsSuccess ? result.Data : new Application.Common.Models.PagedResponse<TripDto>(
                    new List<TripDto>(), page, 20, 0),
                SearchTerm = search,
                SelectedStatus = status,
                FromDate = fromDate,
                ToDate = toDate,
                PageTitle = "مدیریت سفرها"
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var query = new GetTripDetailsQuery { TripId = id };
            var result = await Mediator.Send(query);

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.Message);
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new TripDetailsViewModel
            {
                Trip = result.Data,
                PageTitle = $"جزئیات سفر - {result.Data.TripNumber}"
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Tracking(Guid id)
        {
            var query = new GetTripTrackingQuery { TripId = id };
            var result = await Mediator.Send(query);

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.Message);
                return RedirectToAction(nameof(Details), new { id });
            }

            var viewModel = new TripTrackingViewModel
            {
                TripId = id,
                TrackingPoints = result.Data,
                PageTitle = "ردیابی سفر"
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignDriver(Guid tripId, Guid driverId)
        {
            var command = new AssignDriverToTripCommand 
            { 
                TripId = tripId, 
                DriverId = driverId 
            };
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                SetSuccessMessage(result.Message);
            else
                SetErrorMessage(result.Message);

            return RedirectToAction(nameof(Details), new { id = tripId });
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(Guid id, string reason)
        {
            var command = new CancelTripCommand { TripId = id, Reason = reason };
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                SetSuccessMessage(result.Message);
            else
                SetErrorMessage(result.Message);

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

/