using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Identity.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Identity.Commands.RegisterDriver
{
    public class RegisterDriverCommand : IRequest<Result<IdentityResultDto>>
    {
        public RegisterDriverDto Driver { get; set; }
    }

    public class RegisterDriverCommandValidator : AbstractValidator<RegisterDriverCommand>
    {
        public RegisterDriverCommandValidator()
        {
            RuleFor(x => x.Driver.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

            RuleFor(x => x.Driver.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

            RuleFor(x => x.Driver.NationalId)
                .NotEmpty().WithMessage("National ID is required")
                .Length(10).WithMessage("National ID must be 10 digits");

            RuleFor(x => x.Driver.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^09[0-9]{9}$").WithMessage("Invalid phone number format");

            RuleFor(x => x.Driver.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Driver.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

            RuleFor(x => x.Driver.VehicleType)
                .NotEmpty().WithMessage("Vehicle type is required");

            RuleFor(x => x.Driver.VehiclePlateNumber)
                .NotEmpty().WithMessage("Vehicle plate number is required");

            RuleFor(x => x.Driver.VehicleRegistrationNumber)
                .NotEmpty().WithMessage("Vehicle registration number is required");

            RuleFor(x => x.Driver.VehicleInspectionCertificateNumber)
                .NotEmpty().WithMessage("Vehicle inspection certificate number is required");

            RuleFor(x => x.Driver.VehicleInspectionExpiryDate)
                .NotEmpty().WithMessage("Vehicle inspection expiry date is required")
                .GreaterThan(DateTime.Now).WithMessage("Vehicle inspection certificate must be valid");

            RuleFor(x => x.Driver.DriverLicenseNumber)
                .NotEmpty().WithMessage("Driver license number is required");

            RuleFor(x => x.Driver.DriverLicenseExpiryDate)
                .NotEmpty().WithMessage("Driver license expiry date is required")
                .GreaterThan(DateTime.Now).WithMessage("Driver license must be valid");

            RuleFor(x => x.Driver.ProfilePhotoUrl)
                .NotEmpty().WithMessage("Profile photo is required");

            RuleFor(x => x.Driver.VehiclePhotoUrl)
                .NotEmpty().WithMessage("Vehicle photo is required");

            RuleFor(x => x.Driver.NationalIdPhotoUrl)
                .NotEmpty().WithMessage("National ID photo is required");

            RuleFor(x => x.Driver.VehicleRegistrationPhotoUrl)
                .NotEmpty().WithMessage("Vehicle registration photo is required");

            RuleFor(x => x.Driver.VehicleInspectionPhotoUrl)
                .NotEmpty().WithMessage("Vehicle inspection photo is required");

            RuleFor(x => x.Driver.DriverLicensePhotoUrl)
                .NotEmpty().WithMessage("Driver license photo is required");
        }
    }

    public class RegisterDriverCommandHandler : IRequestHandler<RegisterDriverCommand, Result<IdentityResultDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly IIdentityService _identityService;
        private readonly ILogger<RegisterDriverCommandHandler> _logger;

        public RegisterDriverCommandHandler(
            UserManager<ApplicationUser> userManager,
            IApplicationDbContext context,
            IIdentityService identityService,
            ILogger<RegisterDriverCommandHandler> logger)
        {
            _userManager = userManager;
            _context = context;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Result<IdentityResultDto>> Handle(RegisterDriverCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Driver.Email);
                if (existingUser != null)
                {
                    return Result<IdentityResultDto>.Failure("User with this email already exists");
                }

                // Create user
                var user = new ApplicationUser
                {
                    UserName = request.Driver.Email,
                    Email = request.Driver.Email,
                    PhoneNumber = request.Driver.PhoneNumber,
                    FirstName = request.Driver.FirstName,
                    LastName = request.Driver.LastName,
                    NationalId = request.Driver.NationalId,
                    UserType = UserType.Driver
                };

                var result = await _userManager.CreateAsync(user, request.Driver.Password);
                if (!result.Succeeded)
                {
                    return Result<IdentityResultDto>.Failure(result.Errors.Select(e => e.Description).ToList());
                }

                // Add to Driver role
                await _userManager.AddToRoleAsync(user, "Driver");

                // Create driver profile
                var driver = new Driver
                {
                    UserId = user.Id,
                    VehicleType = request.Driver.VehicleType,
                    VehiclePlateNumber = request.Driver.VehiclePlateNumber,
                    VehicleRegistrationNumber = request.Driver.VehicleRegistrationNumber,
                    VehicleInspectionCertificateNumber = request.Driver.VehicleInspectionCertificateNumber,
                    VehicleInspectionExpiryDate = request.Driver.VehicleInspectionExpiryDate,
                    DriverLicenseNumber = request.Driver.DriverLicenseNumber,
                    DriverLicenseExpiryDate = request.Driver.DriverLicenseExpiryDate,
                    ProfilePhotoUrl = request.Driver.ProfilePhotoUrl,
                    VehiclePhotoUrl = request.Driver.VehiclePhotoUrl,
                    NationalIdPhotoUrl = request.Driver.NationalIdPhotoUrl,
                    VehicleRegistrationPhotoUrl = request.Driver.VehicleRegistrationPhotoUrl,
                    VehicleInspectionPhotoUrl = request.Driver.VehicleInspectionPhotoUrl,
                    DriverLicensePhotoUrl = request.Driver.DriverLicensePhotoUrl,
                    Status = DriverStatus.PendingVerification
                };

                _context.Drivers.Add(driver);
                await _context.SaveChangesAsync(cancellationToken);

                // Generate verification token
                var token = await _identityService.GenerateEmailConfirmationTokenAsync(user.Id);

                // Send verification email
                await _identityService.SendVerificationEmailAsync(user.Email, token);

                return Result<IdentityResultDto>.Success(new IdentityResultDto
                {
                    Succeeded = true,
                    UserId = user.Id,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering driver");
                return Result<IdentityResultDto>.Failure("Error registering driver");
            }
        }
    }
} 