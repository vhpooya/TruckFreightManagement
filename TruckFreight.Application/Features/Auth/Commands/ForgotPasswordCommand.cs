using System.ComponentModel.DataAnnotations;

namespace TruckFreight.Application.Features.Auth.Commands
{
    public class ForgotPasswordCommand
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
} 