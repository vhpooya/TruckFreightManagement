using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Users.Queries.SearchUsers;
using TruckFreight.Application.Features.Users.Commands.ApproveUser;
using TruckFreight.Application.Features.Users.Commands.RejectUser;
using TruckFreight.WebAdmin.Models.Users;

namespace TruckFreight.WebAdmin.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UsersController : BaseAdminController
    {
        public async Task<IActionResult> Index(int page = 1, string search = "", string role = "", string status = "")
        {
            var query = new SearchUsersQuery
            {
                SearchTerm = search,
                Role = string.IsNullOrEmpty(role) ? null : Enum.Parse<Domain.Enums.UserRole>(role),
                Status = string.IsNullOrEmpty(status) ? null : Enum.Parse<Domain.Enums.UserStatus>(status),
                PageNumber = page,
                PageSize = 20
            };

            var result = await Mediator.Send(query);

            var viewModel = new UserListViewModel
            {
                Users = result.IsSuccess ? result.Data : new Application.Common.Models.PagedResponse<UserDto>(
                    new List<UserDto>(), page, 20, 0),
                SearchTerm = search,
                SelectedRole = role,
                SelectedStatus = status,
                PageTitle = "مدیریت کاربران"
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var query = new GetUserDetailsQuery { UserId = id };
            var result = await Mediator.Send(query);

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.Message);
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new UserDetailsViewModel
            {
                User = result.Data,
                PageTitle = $"جزئیات کاربر - {result.Data.FullName}"
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(Guid id)
        {
            var command = new ApproveUserCommand { UserId = id };
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                SetSuccessMessage(result.Message);
            else
                SetErrorMessage(result.Message);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(Guid id, string reason)
        {
            var command = new RejectUserCommand { UserId = id, Reason = reason };
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                SetSuccessMessage(result.Message);
            else
                SetErrorMessage(result.Message);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Block(Guid id, string reason)
        {
            var command = new BlockUserCommand { UserId = id, Reason = reason };
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                SetSuccessMessage(result.Message);
            else
                SetErrorMessage(result.Message);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(Guid id)
        {
            var command = new UnblockUserCommand { UserId = id };
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