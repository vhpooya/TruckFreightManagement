using System;
using FluentValidation;
using TruckFreight.Application.Features.Authentication.Models;
using TruckFreight.Domain.Enums;

namespace TruckFreight.Application.Features.Authentication.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.NationalId)
                .NotEmpty()
                .Matches("^[0-9]{10}$").WithMessage("NationalId must be 10 digits");
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .Matches("^09[0-9]{9}$").WithMessage("Invalid Iranian mobile format");
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8);
            RuleFor(x => x.Role).IsInEnum();

            When(x => x.Role == Role.Driver, () => {
                RuleFor(x => x.LicenseNumber).NotEmpty();
                RuleFor(x => x.LicenseExpiry)
                    .NotNull()
                    .GreaterThan(DateTime.UtcNow);
            });

            When(x => x.Role == Role.CargoOwner, () => {
                RuleFor(x => x.BusinessRegistrationNumber).NotEmpty();
                RuleFor(x => x.CompanyName).NotEmpty();
                RuleFor(x => x.CompanyAddress).NotEmpty();
            });
        }
    }
}

