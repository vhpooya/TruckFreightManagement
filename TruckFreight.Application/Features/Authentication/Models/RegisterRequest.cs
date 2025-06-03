using System;
using TruckFreight.Domain.Enums;

namespace TruckFreight.Application.Features.Authentication.Models
{
    public class RegisterRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }

    }

}

