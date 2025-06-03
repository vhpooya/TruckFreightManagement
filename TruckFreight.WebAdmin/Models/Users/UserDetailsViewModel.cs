using TruckFreight.Application.Features.Users.Queries.GetUserDetails;

namespace TruckFreight.WebAdmin.Models.Users
{
    public class UserDetailsViewModel : BaseViewModel
    {
        public UserDetailsDto User { get; set; } = new UserDetailsDto();
    }
}

/