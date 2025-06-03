using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Vehicles.DTOs;

namespace TruckFreight.Application.Features.Vehicles.Queries.GetVehicles
{
    public class GetVehiclesQuery : IRequest<Result<VehicleListDto>>
    {
        public string SearchTerm { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public bool? MaintenanceRequired { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetVehiclesQueryValidator : AbstractValidator<GetVehiclesQuery>
    {
        public GetVehiclesQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("Start date must be less than or equal to end date");
        }
    }

    public class GetVehiclesQueryHandler : IRequestHandler<GetVehiclesQuery, Result<VehicleListDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetVehiclesQueryHandler> _logger;

        public GetVehiclesQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetVehiclesQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<VehicleListDto>> Handle(GetVehiclesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<VehicleListDto>.Failure("User not authenticated");
                }

                // Get company ID for the current user
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

                if (company == null)
                {
                    return Result<VehicleListDto>.Failure("Company not found");
                }

                // Build query
                var query = _context.Vehicles
                    .Include(v => v.Company)
                    .Where(v => v.CompanyId == company.Id);

                // Apply filters
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(v =>
                        v.Number.ToLower().Contains(searchTerm) ||
                        v.Type.ToLower().Contains(searchTerm) ||
                        v.Model.ToLower().Contains(searchTerm) ||
                        v.RegistrationNumber.ToLower().Contains(searchTerm));
                }

                if (!string.IsNullOrEmpty(request.Type))
                {
                    query = query.Where(v => v.Type == request.Type);
                }

                if (!string.IsNullOrEmpty(request.Status))
                {
                    query = query.Where(v => v.Status == request.Status);
                }

                if (request.MaintenanceRequired.HasValue)
                {
                    query = query.Where(v => v.MaintenanceRequired == request.MaintenanceRequired.Value);
                }

                if (request.StartDate.HasValue)
                {
                    query = query.Where(v => v.CreatedAt >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    query = query.Where(v => v.CreatedAt <= request.EndDate.Value);
                }

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var vehicles = await query
                    .OrderByDescending(v => v.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(v => new VehicleDto
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
                        CreatedAt = v.CreatedAt,
                        CreatedBy = v.CreatedBy,
                        UpdatedAt = v.UpdatedAt,
                        UpdatedBy = v.UpdatedBy,
                        CompanyId = v.CompanyId,
                        CompanyName = v.Company.Name,
                        DriverId = v.DriverId,
                        AdditionalInfo = v.AdditionalInfo
                    })
                    .ToListAsync(cancellationToken);

                var result = new VehicleListDto
                {
                    Items = vehicles,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return Result<VehicleListDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles");
                return Result<VehicleListDto>.Failure("Error retrieving vehicles");
            }
        }
    }
} 