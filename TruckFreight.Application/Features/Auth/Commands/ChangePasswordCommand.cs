using System.ComponentModel.DataAnnotations;

namespace TruckFreight.Application.Features.Auth.Commands
{
    public class ChangePasswordCommand
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }
    }
} 