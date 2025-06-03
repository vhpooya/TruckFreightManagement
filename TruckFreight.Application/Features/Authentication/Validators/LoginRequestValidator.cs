using FluentValidation;
using TruckFreight.Application.Features.Authentication.Models;

namespace TruckFreight.Application.Features.Authentication.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .Matches("^09[0-9]{9}$").WithMessage("Invalid Iranian mobile format");
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
