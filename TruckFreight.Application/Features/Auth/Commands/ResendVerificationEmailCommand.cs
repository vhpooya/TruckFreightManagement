using System.ComponentModel.DataAnnotations;

namespace TruckFreight.Application.Features.Auth.Commands
{
    public class ResendVerificationEmailCommand
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
} 