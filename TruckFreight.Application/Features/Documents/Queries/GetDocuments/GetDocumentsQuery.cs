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
using TruckFreight.Application.Features.Documents.DTOs;

namespace TruckFreight.Application.Features.Documents.Queries.GetDocuments
{
    public class GetDocumentsQuery : IRequest<Result<DocumentListDto>>
    {
        public string SearchTerm { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string VerificationStatus { get; set; }
        public string ReferenceType { get; set; }
        public string ReferenceId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetDocumentsQueryValidator : AbstractValidator<GetDocumentsQuery>
    {
        public GetDocumentsQueryValidator()
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

    public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, Result<DocumentListDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetDocumentsQueryHandler> _logger;

        public GetDocumentsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetDocumentsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<DocumentListDto>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<DocumentListDto>.Failure("User not authenticated");
                }

                // Get user and check permissions
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return Result<DocumentListDto>.Failure("User not found");
                }

                // Build query
                var query = _context.Documents.AsQueryable();

                // Apply filters based on user role
                if (!user.Roles.Contains("Admin") && !user.Roles.Contains("Verifier"))
                {
                    // Regular users can only see their own documents
                    query = query.Where(d => d.CreatedBy == userId);
                }

                // Apply search term filter
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(d =>
                        d.Title.ToLower().Contains(searchTerm) ||
                        d.Description.ToLower().Contains(searchTerm) ||
                        d.Type.ToLower().Contains(searchTerm));
                }

                // Apply type filter
                if (!string.IsNullOrWhiteSpace(request.Type))
                {
                    query = query.Where(d => d.Type == request.Type);
                }

                // Apply status filter
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    query = query.Where(d => d.Status == request.Status);
                }

                // Apply verification status filter
                if (!string.IsNullOrWhiteSpace(request.VerificationStatus))
                {
                    query = query.Where(d => d.VerificationStatus == request.VerificationStatus);
                }

                // Apply reference filters
                if (!string.IsNullOrWhiteSpace(request.ReferenceType))
                {
                    query = query.Where(d => d.ReferenceType == request.ReferenceType);
                }

                if (!string.IsNullOrWhiteSpace(request.ReferenceId))
                {
                    query = query.Where(d => d.ReferenceId == request.ReferenceId);
                }

                // Apply date range filter
                if (request.StartDate.HasValue)
                {
                    query = query.Where(d => d.CreatedAt >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    query = query.Where(d => d.CreatedAt <= request.EndDate.Value);
                }

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var documents = await query
                    .OrderByDescending(d => d.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(d => new DocumentDto
                    {
                        Id = d.Id,
                        Type = d.Type,
                        Title = d.Title,
                        Description = d.Description,
                        FileUrl = d.FileUrl,
                        FileType = d.FileType,
                        FileSize = d.FileSize,
                        ReferenceId = d.ReferenceId,
                        ReferenceType = d.ReferenceType,
                        Status = d.Status,
                        VerificationStatus = d.VerificationStatus,
                        VerifiedBy = d.VerifiedBy,
                        VerifiedAt = d.VerifiedAt,
                        ExpiryDate = d.ExpiryDate,
                        CreatedAt = d.CreatedAt,
                        CreatedBy = d.CreatedBy,
                        UpdatedAt = d.UpdatedAt,
                        UpdatedBy = d.UpdatedBy,
                        Metadata = d.Metadata
                    })
                    .ToListAsync(cancellationToken);

                var result = new DocumentListDto
                {
                    Items = documents,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return Result<DocumentListDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents");
                return Result<DocumentListDto>.Failure("Error retrieving documents");
            }
        }
    }
} 