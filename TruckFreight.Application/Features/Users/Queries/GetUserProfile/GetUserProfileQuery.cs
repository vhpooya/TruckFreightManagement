using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Users.DTOs;

namespace TruckFreight.Application.Features.Users.Queries.GetUserProfile
{
    public class GetUserProfileQuery : IRequest<Result<UserProfileDto>>
    {
        public string UserId { get; set; }
    }

    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetUserProfileQueryHandler> _logger;

        public GetUserProfileQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetUserProfileQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Result<UserProfileDto>.Failure("User not authenticated");
                }

                // If no specific user ID is provided, return the current user's profile
                var userId = string.IsNullOrEmpty(request.UserId) ? currentUserId : request.UserId;

                // Get user with related data
                var user = await _context.Users
                    .Include(u => u.Company)
                        .ThenInclude(c => c.Vehicles)
                    .Include(u => u.Company)
                        .ThenInclude(c => c.Drivers)
                    .Include(u => u.Driver)
                        .ThenInclude(d => d.AssignedVehicles)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return Result<UserProfileDto>.Failure("User not found");
                }

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
                        CreatedAt = user.Company.CreatedAt,
                        Vehicles = user.Company.Vehicles.Select(v => new VehicleDto
                        {
                            Id = v.Id,
                            Number = v.Number,
                            Type = v.Type,
                            Model = v.Model,
                            Color = v.Color,
                            RegistrationNumber = v.RegistrationNumber,
                            RegistrationCardPicture = v.RegistrationCardPicture,
                            InspectionCertificatePicture = v.InspectionCertificatePicture,
                            Status = v.Status,
                            MaintenanceRequired = v.MaintenanceRequired,
                            LastMaintenanceDate = v.LastMaintenanceDate,
                            NextMaintenanceDate = v.NextMaintenanceDate,
                            TotalDistance = v.TotalDistance,
                            AverageFuelConsumption = v.AverageFuelConsumption,
                            CreatedAt = v.CreatedAt
                        }).ToList(),
                        Drivers = user.Company.Drivers.Select(d => new DriverDto
                        {
                            Id = d.Id,
                            FirstName = d.FirstName,
                            LastName = d.LastName,
                            PhoneNumber = d.PhoneNumber,
                            Email = d.Email,
                            LicenseNumber = d.LicenseNumber,
                            Status = d.Status,
                            Rating = d.Rating,
                            TotalDeliveries = d.TotalDeliveries,
                            OnTimeDeliveryRate = d.OnTimeDeliveryRate
                        }).ToList()
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
                        CreatedAt = user.Driver.CreatedAt,
                        AssignedVehicles = user.Driver.AssignedVehicles.Select(v => new VehicleDto
                        {
                            Id = v.Id,
                            Number = v.Number,
                            Type = v.Type,
                            Model = v.Model,
                            Color = v.Color,
                            RegistrationNumber = v.RegistrationNumber,
                            RegistrationCardPicture = v.RegistrationCardPicture,
                            InspectionCertificatePicture = v.InspectionCertificatePicture,
                            Status = v.Status,
                            MaintenanceRequired = v.MaintenanceRequired,
                            LastMaintenanceDate = v.LastMaintenanceDate,
                            NextMaintenanceDate = v.NextMaintenanceDate,
                            TotalDistance = v.TotalDistance,
                            AverageFuelConsumption = v.AverageFuelConsumption,
                            CreatedAt = v.CreatedAt
                        }).ToList()
                    };
                }

                return Result<UserProfileDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return Result<UserProfileDto>.Failure("Error retrieving user profile");
            }
        }
    }
} 