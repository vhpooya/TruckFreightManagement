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
using TruckFreight.Application.Features.Ratings.DTOs;

namespace TruckFreight.Application.Features.Ratings.Queries.GetRatings
{
    public class GetRatingsQuery : IRequest<Result<RatingListDto>>
    {
        public RatingFilterDto Filter { get; set; }
    }

    public class GetRatingsQueryValidator : AbstractValidator<GetRatingsQuery>
    {
        public GetRatingsQueryValidator()
        {
            RuleFor(x => x.Filter.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.Filter.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

            RuleFor(x => x.Filter.StartDate)
                .LessThanOrEqualTo(x => x.Filter.EndDate)
                .When(x => x.Filter.StartDate.HasValue && x.Filter.EndDate.HasValue)
                .WithMessage("Start date must be less than or equal to end date");

            RuleFor(x => x.Filter.MinRating)
                .LessThanOrEqualTo(x => x.Filter.MaxRating)
                .When(x => x.Filter.MinRating.HasValue && x.Filter.MaxRating.HasValue)
                .WithMessage("Minimum rating must be less than or equal to maximum rating");
        }
    }

    public class GetRatingsQueryHandler : IRequestHandler<GetRatingsQuery, Result<RatingListDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetRatingsQueryHandler> _logger;

        public GetRatingsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetRatingsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<RatingListDto>> Handle(GetRatingsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<RatingListDto>.Failure("User not authenticated");
                }

                // Get user and check permissions
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return Result<RatingListDto>.Failure("User not found");
                }

                // Build base query
                var query = _context.Ratings.AsQueryable();

                // Apply company filter if user is not admin
                if (!user.Roles.Contains("Admin"))
                {
                    var companyId = user.CompanyId;
                    if (string.IsNullOrEmpty(companyId))
                    {
                        return Result<RatingListDto>.Failure("User not associated with any company");
                    }

                    // Filter ratings based on company association
                    query = query.Where(r =>
                        (r.RatedEntityType == "Driver" && _context.Drivers.Any(d => d.Id == r.RatedEntityId && d.CompanyId == companyId)) ||
                        (r.RatedEntityType == "Customer" && _context.Customers.Any(c => c.Id == r.RatedEntityId && c.CompanyId == companyId)) ||
                        (r.RatedEntityType == "Company" && r.RatedEntityId == companyId)
                    );
                }

                // Apply filters
                if (!string.IsNullOrEmpty(request.Filter.RatedEntityId))
                {
                    query = query.Where(r => r.RatedEntityId == request.Filter.RatedEntityId);
                }

                if (!string.IsNullOrEmpty(request.Filter.RatedEntityType))
                {
                    query = query.Where(r => r.RatedEntityType == request.Filter.RatedEntityType);
                }

                if (!string.IsNullOrEmpty(request.Filter.DeliveryId))
                {
                    query = query.Where(r => r.DeliveryId == request.Filter.DeliveryId);
                }

                if (request.Filter.MinRating.HasValue)
                {
                    query = query.Where(r => r.Rating >= request.Filter.MinRating.Value);
                }

                if (request.Filter.MaxRating.HasValue)
                {
                    query = query.Where(r => r.Rating <= request.Filter.MaxRating.Value);
                }

                if (request.Filter.StartDate.HasValue)
                {
                    query = query.Where(r => r.CreatedAt >= request.Filter.StartDate.Value);
                }

                if (request.Filter.EndDate.HasValue)
                {
                    query = query.Where(r => r.CreatedAt <= request.Filter.EndDate.Value);
                }

                if (request.Filter.IsVerified.HasValue)
                {
                    query = query.Where(r => r.IsVerified == request.Filter.IsVerified.Value);
                }

                if (!string.IsNullOrEmpty(request.Filter.VerificationStatus))
                {
                    query = query.Where(r => r.VerificationStatus == request.Filter.VerificationStatus);
                }

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply sorting
                query = request.Filter.SortBy?.ToLower() switch
                {
                    "rating" => request.Filter.SortDescending
                        ? query.OrderByDescending(r => r.Rating)
                        : query.OrderBy(r => r.Rating),
                    "createdat" => request.Filter.SortDescending
                        ? query.OrderByDescending(r => r.CreatedAt)
                        : query.OrderBy(r => r.CreatedAt),
                    "verifiedat" => request.Filter.SortDescending
                        ? query.OrderByDescending(r => r.VerifiedAt)
                        : query.OrderBy(r => r.VerifiedAt),
                    _ => query.OrderByDescending(r => r.CreatedAt)
                };

                // Apply pagination
                var items = await query
                    .Skip((request.Filter.PageNumber - 1) * request.Filter.PageSize)
                    .Take(request.Filter.PageSize)
                    .Select(r => new RatingDto
                    {
                        Id = r.Id,
                        DeliveryId = r.DeliveryId,
                        RatedEntityId = r.RatedEntityId,
                        RatedEntityType = r.RatedEntityType,
                        RatedByName = r.RatedByName,
                        RatedById = r.RatedById,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CategoryRatings = r.CategoryRatings,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        IsVerified = r.IsVerified,
                        VerificationStatus = r.VerificationStatus,
                        VerificationComment = r.VerificationComment,
                        VerifiedAt = r.VerifiedAt,
                        VerifiedBy = r.VerifiedBy
                    })
                    .ToListAsync(cancellationToken);

                var result = new RatingListDto
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.Filter.PageNumber,
                    PageSize = request.Filter.PageSize
                };

                return Result<RatingListDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ratings");
                return Result<RatingListDto>.Failure("Error retrieving ratings");
            }
        }
    }
} 