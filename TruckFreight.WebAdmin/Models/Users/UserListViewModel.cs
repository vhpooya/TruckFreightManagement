using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Users.Queries.SearchUsers;

namespace TruckFreight.WebAdmin.Models.Users
{
    public class UserListViewModel : BaseViewModel
    {
        public PagedResponse<UserDto> Users { get; set; } = new PagedResponse<UserDto>(new List<UserDto>(), 1, 20, 0);
        public string SearchTerm { get; set; } = string.Empty;
        public string SelectedRole { get; set; } = string.Empty;
        public string SelectedStatus { get; set; } = string.Empty;
    }
}

/