using System.Threading.Tasks;

namespace TruckFreight.Domain.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string token);
        Task SendPasswordResetAsync(string email, string token);
        Task SendEmailAsync(string to, string subject, string body);
    }
} 