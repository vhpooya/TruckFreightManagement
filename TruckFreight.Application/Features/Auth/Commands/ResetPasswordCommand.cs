using System.ComponentModel.DataAnnotations;

namespace TruckFreight.Application.Features.Auth.Commands
{
    public class ResetPasswordCommand
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }
    }
} 