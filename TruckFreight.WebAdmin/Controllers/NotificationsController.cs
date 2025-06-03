using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Notifications.Commands.SendNotification;
using TruckFreight.WebAdmin.Models.Notifications;

namespace TruckFreight.WebAdmin.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,Operator")]
    public class NotificationsController : BaseAdminController
    {
        public IActionResult Index()
        {
            var viewModel = new NotificationIndexViewModel
            {
                PageTitle = "مدیریت اعلانات"
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendBroadcast(SendBroadcastViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetErrorMessage("لطفا تمام فیلدهای ضروری را تکمیل کنید");
                return RedirectToAction(nameof(Index));
            }

            var command = new SendNotificationCommand
            {
                Title = model.Title,
                Message = model.Message,
                Type = model.Type,
                Role = model.TargetRole
            };

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                SetSuccessMessage($"اعلان به {model.TargetRole} ارسال شد");
            else
                SetErrorMessage(result.Message);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Templates()
        {
            var query = new GetNotificationTemplatesQuery();
            var result = await Mediator.Send(query);

            var viewModel = new NotificationTemplatesViewModel
            {
                Templates = result.IsSuccess ? result.Data : new List<NotificationTemplateDto>(),
                PageTitle = "قالب‌های اعلانات"
            };

            return View(viewModel);
        }
    }
}

/