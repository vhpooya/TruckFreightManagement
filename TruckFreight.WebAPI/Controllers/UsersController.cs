using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Users.Queries.GetUserProfile;
using TruckFreight.Application.Features.Users.Commands.UpdateUserProfile;

namespace TruckFreight.WebAPI.Controllers
{
    [Authorize]
    public class UsersController : BaseController
    {
        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult> GetProfile()
        {
            var query = new GetUserProfileQuery { UserId = GetCurrentUserId() };
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("profile")]
        public async Task<ActionResult> UpdateProfile([FromBody] UpdateUserProfileCommand command)
        {
            command.UserId = GetCurrentUserId();
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Upload profile image
        /// </summary>
        [HttpPost("profile/image")]
        public async Task<ActionResult> UploadProfileImage(IFormFile file)
        {
            var command = new UploadProfileImageCommand 
            { 
                UserId = GetCurrentUserId(),
                File = file 
            };
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Change password
        /// </summary>
        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            command.UserId = GetCurrentUserId();
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}

/