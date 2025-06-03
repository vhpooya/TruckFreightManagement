using System.Collections.Generic;
using System.Threading.Tasks;
using TruckFreight.Application.Features.Notifications.DTOs;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface INotificationTemplateRenderer
    {
        /// <summary>
        /// Renders a notification template with the provided variables
        /// </summary>
        /// <param name="template">The notification template to render</param>
        /// <param name="variables">Dictionary of variable names and their values</param>
        /// <returns>The rendered notification template</returns>
        Task<NotificationTemplateDto> RenderTemplateAsync(NotificationTemplateDto template, Dictionary<string, string> variables);

        /// <summary>
        /// Renders a notification template by its type with the provided variables
        /// </summary>
        /// <param name="templateType">The type of template to render</param>
        /// <param name="variables">Dictionary of variable names and their values</param>
        /// <returns>The rendered notification template</returns>
        Task<NotificationTemplateDto> RenderTemplateByTypeAsync(string templateType, Dictionary<string, string> variables);

        /// <summary>
        /// Validates if all required variables are provided for a template
        /// </summary>
        /// <param name="template">The notification template to validate</param>
        /// <param name="variables">Dictionary of variable names and their values</param>
        /// <returns>True if all required variables are provided, false otherwise</returns>
        Task<bool> ValidateVariablesAsync(NotificationTemplateDto template, Dictionary<string, string> variables);

        /// <summary>
        /// Gets a list of missing required variables for a template
        /// </summary>
        /// <param name="template">The notification template to check</param>
        /// <param name="variables">Dictionary of variable names and their values</param>
        /// <returns>List of missing required variable names</returns>
        Task<List<string>> GetMissingVariablesAsync(NotificationTemplateDto template, Dictionary<string, string> variables);
    }
} 