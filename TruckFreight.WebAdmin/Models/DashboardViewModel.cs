using TruckFreight.Application.Features.Administration.Queries.GetSystemOverview;

namespace TruckFreight.WebAdmin.Models
{
    public class DashboardViewModel : BaseViewModel
    {
        public SystemOverviewDto SystemOverview { get; set; } = new SystemOverviewDto();
    }
}

/