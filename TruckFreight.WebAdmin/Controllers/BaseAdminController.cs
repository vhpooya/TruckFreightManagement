using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TruckFreight.WebAdmin.Controllers
{
    [Authorize]
    public abstract class BaseAdminController : Controller
    {
        private ISender _mediator = null!;
        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        protected void SetInfoMessage(string message)
        {
            TempData["InfoMessage"] = message;
        }

        protected Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}

/