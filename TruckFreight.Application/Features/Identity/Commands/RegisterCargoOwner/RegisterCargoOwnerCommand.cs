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

namespace TruckFreight.Application.Features.Identity.Commands.RegisterCargoOwner
{
    public class RegisterCargoOwnerCommand : IRequest<Result<IdentityResultDto>>
    {
        public RegisterCargoOwnerDto CargoOwner { get; set; }
    }

    public class RegisterCargoOwnerCommandValidator : AbstractValidator<RegisterCargoOwnerCommand>
    {
        public RegisterCargoOwnerCommandValidator()
        {
            RuleFor(x => x.CargoOwner.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

            RuleFor(x => x.CargoOwner.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

            RuleFor(x => x.CargoOwner.NationalId)
                .NotEmpty().WithMessage("National ID is required")
                .Length(10).WithMessage("National ID must be 10 digits");

            RuleFor(x => x.CargoOwner.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^09[0-9]{9}$").WithMessage("Invalid phone number format");

            RuleFor(x => x.CargoOwner.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.CargoOwner.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

            RuleFor(x => x.CargoOwner.Occupation)
                .NotEmpty().WithMessage("Occupation is required")
                .MaximumLength(100).WithMessage("Occupation must not exceed 100 characters");

            RuleFor(x => x.CargoOwner.ProfilePhotoUrl)
                .NotEmpty().WithMessage("Profile photo is required");

            RuleFor(x => x.CargoOwner.NationalIdPhotoUrl)
                .NotEmpty().WithMessage("National ID photo is required");
        }
    }

    public class RegisterCargoOwnerCommandHandler : IRequestHandler<RegisterCargoOwnerCommand, Result<IdentityResultDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly IIdentityService _identityService;
        private readonly ILogger<RegisterCargoOwnerCommandHandler> _logger;

        public RegisterCargoOwnerCommandHandler(
            UserManager<ApplicationUser> userManager,
            IApplicationDbContext context,
            IIdentityService identityService,
            ILogger<RegisterCargoOwnerCommandHandler> logger)
        {
            _userManager = userManager;
            _context = context;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Result<IdentityResultDto>> Handle(RegisterCargoOwnerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.CargoOwner.Email);
                if (existingUser != null)
                {
                    return Result<IdentityResultDto>.Failure("User with this email already exists");
                }

                // Create user
                var user = new ApplicationUser
                {
                    UserName = request.CargoOwner.Email,
                    Email = request.CargoOwner.Email,
                    PhoneNumber = request.CargoOwner.PhoneNumber,
                    FirstName = request.CargoOwner.FirstName,
                    LastName = request.CargoOwner.LastName,
                    NationalId = request.CargoOwner.NationalId,
                    UserType = UserType.CargoOwner
                };

                var result = await _userManager.CreateAsync(user, request.CargoOwner.Password);
                if (!result.Succeeded)
                {
                    return Result<IdentityResultDto>.Failure(result.Errors.Select(e => e.Description).ToList());
                }

                // Add to CargoOwner role
                await _userManager.AddToRoleAsync(user, "CargoOwner");

                // Create cargo owner profile
                var cargoOwner = new CargoOwner
                {
                    UserId = user.Id,
                    Occupation = request.CargoOwner.Occupation,
                    ProfilePhotoUrl = request.CargoOwner.ProfilePhotoUrl,
                    NationalIdPhotoUrl = request.CargoOwner.NationalIdPhotoUrl,
                    Status = CargoOwnerStatus.PendingVerification
                };

                _context.CargoOwners.Add(cargoOwner);
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
                _logger.LogError(ex, "Error registering cargo owner");
                return Result<IdentityResultDto>.Failure("Error registering cargo owner");
            }
        }
    }
} 