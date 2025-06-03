using System.ComponentModel.DataAnnotations;

namespace TruckFreight.Application.Features.Auth.Commands
{
    public class VerifyEmailCommand
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }
    }
} 