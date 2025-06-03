using System;

namespace TruckFreight.Application.Features.Authentication.Models
{
    public class AuthResponse
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime Expiry { get; set; }
    }
}
