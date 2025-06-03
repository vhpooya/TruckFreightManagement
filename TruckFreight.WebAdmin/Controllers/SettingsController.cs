using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Administration.Queries.GetSystemConfigurations;
using TruckFreight.Application.Features.Administration.Commands.UpdateSystemConfiguration;
using TruckFreight.WebAdmin.Models.Settings;

namespace TruckFreight.WebAdmin.Controllers
{
   [Authorize(Roles = "Admin,SuperAdmin")]
   public class SettingsController : BaseAdminController
   {
       public async Task<IActionResult> Index()
       {
           var query = new GetSystemConfigurationsQuery();
           var result = await Mediator.Send(query);

           var viewModel = new SettingsViewModel
           {
               Configurations = result.IsSuccess ? result.Data : new List<SystemConfigurationDto>(),
               PageTitle = "تنظیمات سیستم"
           };

           return View(viewModel);
       }

       [HttpPost]
       public async Task<IActionResult> UpdateConfiguration(Guid id, string value)
       {
           var command = new UpdateSystemConfigurationCommand
           {
               ConfigurationId = id,
               Value = value
           };

           var result = await Mediator.Send(command);

           if (result.IsSuccess)
               SetSuccessMessage("تنظیمات با موفقیت به‌روزرسانی شد");
           else
               SetErrorMessage(result.Message);

           return RedirectToAction(nameof(Index));
       }

       public async Task<IActionResult> EmailTemplates()
       {
           var query = new GetEmailTemplatesQuery();
           var result = await Mediator.Send(query);

           var viewModel = new EmailTemplatesViewModel
           {
               Templates = result.IsSuccess ? result.Data : new List<EmailTemplateDto>(),
               PageTitle = "قالب‌های ایمیل"
           };

           return View(viewModel);
       }

       [HttpPost]
       public async Task<IActionResult> UpdateEmailTemplate(Guid id, string subject, string body)
       {
           var command = new UpdateEmailTemplateCommand
           {
               TemplateId = id,
               Subject = subject,
               Body = body
           };

           var result = await Mediator.Send(command);

           if (result.IsSuccess)
               SetSuccessMessage("قالب ایمیل با موفقیت به‌روزرسانی شد");
           else
               SetErrorMessage(result.Message);

           return RedirectToAction(nameof(EmailTemplates));
       }

       public async Task<IActionResult> SmsTemplates()
       {
           var query = new GetSmsTemplatesQuery();
           var result = await Mediator.Send(query);

           var viewModel = new SmsTemplatesViewModel
           {
               Templates = result.IsSuccess ? result.Data : new List<SmsTemplateDto>(),
               PageTitle = "قالب‌های پیامک"
           };

           return View(viewModel);
       }

       [HttpPost]
       public async Task<IActionResult> UpdateSmsTemplate(Guid id, string message)
       {
           var command = new UpdateSmsTemplateCommand
           {
               TemplateId = id,
               Message = message
           };

           var result = await Mediator.Send(command);

           if (result.IsSuccess)
               SetSuccessMessage("قالب پیامک با موفقیت به‌روزرسانی شد");
           else
               SetErrorMessage(result.Message);

           return RedirectToAction(nameof(SmsTemplates));
       }
   }
}
