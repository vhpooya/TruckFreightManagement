using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Users.DTOs;

namespace TruckFreight.Application.Features.Users.Commands.UpdateUserProfile
{
    public class UpdateUserProfileCommand : IRequest<Result<UserProfileDto>>
    {
        public UpdateProfileDto Profile { get; set; }
    }

    public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
    {
        public UpdateUserProfileCommandValidator()
        {
            RuleFor(x => x.Profile.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

            RuleFor(x => x.Profile.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

            RuleFor(x => x.Profile.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^[0-9]{10}$").WithMessage("Phone number must be 10 digits");

            RuleFor(x => x.Profile.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

            RuleFor(x => x.Profile.NationalId)
                .NotEmpty().WithMessage("National ID is required")
                .Matches(@"^[0-9]{10}$").WithMessage("National ID must be 10 digits");

            RuleFor(x => x.Profile.Address)
                .NotEmpty().WithMessage("Address is required")
                .MaximumLength(200).WithMessage("Address must not exceed 200 characters");
        }
    }

    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result<UserProfileDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

        public UpdateUserProfileCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<UpdateUserProfileCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<UserProfileDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<UserProfileDto>.Failure("User not authenticated");
                }

                var user = await _context.Users
                    .Include(u => u.Company)
                    .Include(u => u.Driver)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return Result<UserProfileDto>.Failure("User not found");
                }

                // Update user profile
                user.FirstName = request.Profile.FirstName;
                user.LastName = request.Profile.LastName;
                user.PhoneNumber = request.Profile.PhoneNumber;
                user.Email = request.Profile.Email;
                user.NationalId = request.Profile.NationalId;
                user.Address = request.Profile.Address;
                user.ProfilePicture = request.Profile.ProfilePicture;
                user.AdditionalInfo = request.Profile.AdditionalInfo;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = userId;

                await _context.SaveChangesAsync(cancellationToken);

                // Map to DTO
                var result = new UserProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    NationalId = user.NationalId,
                    Address = user.Address,
                    ProfilePicture = user.ProfilePicture,
                    UserType = user.UserType,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    AdditionalInfo = user.AdditionalInfo
                };

                // Add company profile if exists
                if (user.Company != null)
                {
                    result.Company = new CompanyProfileDto
                    {
                        Id = user.Company.Id,
                        Name = user.Company.Name,
                        RegistrationNumber = user.Company.RegistrationNumber,
                        EconomicCode = user.Company.EconomicCode,
                        Address = user.Company.Address,
                        PhoneNumber = user.Company.PhoneNumber,
                        Email = user.Company.Email,
                        Website = user.Company.Website,
                        Logo = user.Company.Logo,
                        Status = user.Company.Status,
                        CreatedAt = user.Company.CreatedAt
                    };
                }

                // Add driver profile if exists
                if (user.Driver != null)
                {
                    result.Driver = new DriverProfileDto
                    {
                        Id = user.Driver.Id,
                        LicenseNumber = user.Driver.LicenseNumber,
                        LicenseType = user.Driver.LicenseType,
                        LicenseExpiryDate = user.Driver.LicenseExpiryDate,
                        LicensePicture = user.Driver.LicensePicture,
                        NationalIdPicture = user.Driver.NationalIdPicture,
                        Status = user.Driver.Status,
                        Rating = user.Driver.Rating,
                        TotalDeliveries = user.Driver.TotalDeliveries,
                        CompletedDeliveries = user.Driver.CompletedDeliveries,
                        CanceledDeliveries = user.Driver.CanceledDeliveries,
                        OnTimeDeliveryRate = user.Driver.OnTimeDeliveryRate,
                        CreatedAt = user.Driver.CreatedAt
                    };
                }

                return Result<UserProfileDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return Result<UserProfileDto>.Failure("Error updating user profile");
            }
        }
    }
} 