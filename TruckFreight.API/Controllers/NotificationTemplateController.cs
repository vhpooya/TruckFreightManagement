using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Notifications.Commands.CreateNotificationTemplate;
using TruckFreight.Application.Features.Notifications.Commands.UpdateNotificationTemplate;
using TruckFreight.Application.Features.Notifications.DTOs;
using TruckFreight.Application.Features.Notifications.Queries.GetNotificationTemplates;

namespace TruckFreight.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationTemplateController : ApiControllerBase
    {
        /// <summary>
        /// Gets a paginated list of notification templates
        /// </summary>
        /// <param name="filter">Filter criteria for the templates</param>
        /// <returns>Paginated list of notification templates</returns>
        [HttpGet]
        public async Task<ActionResult<Result<NotificationTemplateListDto>>> GetTemplates([FromQuery] NotificationTemplateFilterDto filter)
        {
            var query = new GetNotificationTemplatesQuery { Filter = filter };
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new notification template
        /// </summary>
        /// <param name="template">The template to create</param>
        /// <returns>The created template</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<NotificationTemplateDto>>> CreateTemplate([FromBody] CreateNotificationTemplateDto template)
        {
            var command = new CreateNotificationTemplateCommand { Template = template };
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing notification template
        /// </summary>
        /// <param name="id">The ID of the template to update</param>
        /// <param name="template">The updated template data</param>
        /// <returns>The updated template</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<NotificationTemplateDto>>> UpdateTemplate(string id, [FromBody] UpdateNotificationTemplateDto template)
        {
            var command = new UpdateNotificationTemplateCommand
            {
                Id = id,
                Template = template
            };
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Renders a notification template with the provided variables
        /// </summary>
        /// <param name="id">The ID of the template to render</param>
        /// <param name="variables">The variables to use for rendering</param>
        /// <returns>The rendered template</returns>
        [HttpPost("{id}/render")]
        public async Task<ActionResult<Result<NotificationTemplateDto>>> RenderTemplate(string id, [FromBody] Dictionary<string, string> variables)
        {
            var template = await Mediator.Send(new GetNotificationTemplateByIdQuery { Id = id });
            if (!template.Succeeded)
            {
                return NotFound(template);
            }

            var renderCommand = new RenderNotificationTemplateCommand
            {
                Template = template.Data,
                Variables = variables
            };
            var result = await Mediator.Send(renderCommand);
            return Ok(result);
        }

        /// <summary>
        /// Renders a notification template by its type with the provided variables
        /// </summary>
        /// <param name="type">The type of template to render</param>
        /// <param name="variables">The variables to use for rendering</param>
        /// <returns>The rendered template</returns>
        [HttpPost("render/{type}")]
        public async Task<ActionResult<Result<NotificationTemplateDto>>> RenderTemplateByType(string type, [FromBody] Dictionary<string, string> variables)
        {
            var renderCommand = new RenderNotificationTemplateByTypeCommand
            {
                Type = type,
                Variables = variables
            };
            var result = await Mediator.Send(renderCommand);
            return Ok(result);
        }

        /// <summary>
        /// Validates if all required variables are provided for a template
        /// </summary>
        /// <param name="id">The ID of the template to validate</param>
        /// <param name="variables">The variables to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("{id}/validate")]
        public async Task<ActionResult<Result<bool>>> ValidateVariables(string id, [FromBody] Dictionary<string, string> variables)
        {
            var template = await Mediator.Send(new GetNotificationTemplateByIdQuery { Id = id });
            if (!template.Succeeded)
            {
                return NotFound(template);
            }

            var validateCommand = new ValidateNotificationTemplateVariablesCommand
            {
                Template = template.Data,
                Variables = variables
            };
            var result = await Mediator.Send(validateCommand);
            return Ok(result);
        }

        /// <summary>
        /// Gets a list of missing required variables for a template
        /// </summary>
        /// <param name="id">The ID of the template to check</param>
        /// <param name="variables">The variables to check</param>
        /// <returns>List of missing required variables</returns>
        [HttpPost("{id}/missing-variables")]
        public async Task<ActionResult<Result<List<string>>>> GetMissingVariables(string id, [FromBody] Dictionary<string, string> variables)
        {
            var template = await Mediator.Send(new GetNotificationTemplateByIdQuery { Id = id });
            if (!template.Succeeded)
            {
                return NotFound(template);
            }

            var getMissingCommand = new GetMissingNotificationTemplateVariablesCommand
            {
                Template = template.Data,
                Variables = variables
            };
            var result = await Mediator.Send(getMissingCommand);
            return Ok(result);
        }
    }
} 