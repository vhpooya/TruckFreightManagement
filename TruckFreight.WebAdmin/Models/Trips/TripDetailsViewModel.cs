using TruckFreight.Application.Features.Trips.Queries.GetTripDetails;

namespace TruckFreight.WebAdmin.Models.Trips
{
    public class TripDetailsViewModel : BaseViewModel
    {
        public TripDetailsDto Trip { get; set; } = new TripDetailsDto();
    }
}

/