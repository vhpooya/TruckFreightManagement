using TruckFreight.Application.Features.Trips.Queries.GetTripTracking;

namespace TruckFreight.WebAdmin.Models.Trips
{
    public class TripTrackingViewModel : BaseViewModel
    {
        public Guid TripId { get; set; }
        public List<TripTrackingDto> TrackingPoints { get; set; } = new List<TripTrackingDto>();
    }
}

/