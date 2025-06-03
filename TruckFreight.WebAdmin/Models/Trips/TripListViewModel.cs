using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Trips.Queries.SearchTrips;

namespace TruckFreight.WebAdmin.Models.Trips
{
    public class TripListViewModel : BaseViewModel
    {
        public PagedResponse<TripDto> Trips { get; set; } = new PagedResponse<TripDto>(new List<TripDto>(), 1, 20, 0);
        public string SearchTerm { get; set; } = string.Empty;
        public string SelectedStatus { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

/