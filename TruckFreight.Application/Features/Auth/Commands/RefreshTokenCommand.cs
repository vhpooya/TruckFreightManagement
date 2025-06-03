using System.ComponentModel.DataAnnotations;

namespace TruckFreight.Application.Features.Auth.Commands
{
    public class RefreshTokenCommand
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
} 