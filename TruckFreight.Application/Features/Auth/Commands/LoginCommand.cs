using System.ComponentModel.DataAnnotations;

namespace TruckFreight.Application.Features.Auth.Commands
{
    public class LoginCommand
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
} 