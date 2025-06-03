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
using TruckFreight.Application.Features.Settings.DTOs;

namespace TruckFreight.Application.Features.Settings.Queries.GetSystemSettings
{
    public class GetSystemSettingsQuery : IRequest<Result<SystemSettingsListDto>>
    {
        public SystemSettingsFilterDto Filter { get; set; }
    }

    public class GetSystemSettingsQueryValidator : AbstractValidator<GetSystemSettingsQuery>
    {
        public GetSystemSettingsQueryValidator()
        {
            RuleFor(x => x.Filter.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.Filter.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

            RuleFor(x => x.Filter.SearchTerm)
                .MaximumLength(100).WithMessage("Search term must not exceed 100 characters");
        }
    }

    public class GetSystemSettingsQueryHandler : IRequestHandler<GetSystemSettingsQuery, Result<SystemSettingsListDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetSystemSettingsQueryHandler> _logger;
        private readonly IEncryptionService _encryptionService;

        public GetSystemSettingsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetSystemSettingsQueryHandler> logger,
            IEncryptionService encryptionService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
            _encryptionService = encryptionService;
        }

        public async Task<Result<SystemSettingsListDto>> Handle(GetSystemSettingsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<SystemSettingsListDto>.Failure("User not authenticated");
                }

                // Get user and check permissions
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return Result<SystemSettingsListDto>.Failure("User not found");
                }

                // Build base query
                var query = _context.SystemSettings.AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(request.Filter.SearchTerm))
                {
                    var searchTerm = request.Filter.SearchTerm.ToLower();
                    query = query.Where(s =>
                        s.Key.ToLower().Contains(searchTerm) ||
                        s.Description.ToLower().Contains(searchTerm));
                }

                if (!string.IsNullOrWhiteSpace(request.Filter.Category))
                {
                    query = query.Where(s => s.Category == request.Filter.Category);
                }

                if (!string.IsNullOrWhiteSpace(request.Filter.DataType))
                {
                    query = query.Where(s => s.DataType == request.Filter.DataType);
                }

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply sorting
                query = request.Filter.SortBy?.ToLower() switch
                {
                    "key" => request.Filter.SortDescending
                        ? query.OrderByDescending(s => s.Key)
                        : query.OrderBy(s => s.Key),
                    "category" => request.Filter.SortDescending
                        ? query.OrderByDescending(s => s.Category)
                        : query.OrderBy(s => s.Category),
                    "datatype" => request.Filter.SortDescending
                        ? query.OrderByDescending(s => s.DataType)
                        : query.OrderBy(s => s.DataType),
                    "createdat" => request.Filter.SortDescending
                        ? query.OrderByDescending(s => s.CreatedAt)
                        : query.OrderBy(s => s.CreatedAt),
                    "updatedat" => request.Filter.SortDescending
                        ? query.OrderByDescending(s => s.UpdatedAt)
                        : query.OrderBy(s => s.UpdatedAt),
                    _ => query.OrderBy(s => s.Key)
                };

                // Apply pagination
                var settings = await query
                    .Skip((request.Filter.PageNumber - 1) * request.Filter.PageSize)
                    .Take(request.Filter.PageSize)
                    .ToListAsync(cancellationToken);

                // Map to DTOs
                var settingsDto = settings.Select(s => new SystemSettingsDto
                {
                    Id = s.Id,
                    Key = s.Key,
                    Value = s.IsEncrypted
                        ? _encryptionService.Decrypt(s.Value)
                        : s.Value,
                    Description = s.Description,
                    Category = s.Category,
                    DataType = s.DataType,
                    IsEncrypted = s.IsEncrypted,
                    IsReadOnly = s.IsReadOnly,
                    ValidationRules = s.ValidationRules,
                    DefaultValue = s.DefaultValue,
                    CreatedAt = s.CreatedAt,
                    CreatedBy = s.CreatedBy,
                    UpdatedAt = s.UpdatedAt,
                    UpdatedBy = s.UpdatedBy
                }).ToList();

                var result = new SystemSettingsListDto
                {
                    Settings = settingsDto,
                    TotalCount = totalCount,
                    PageNumber = request.Filter.PageNumber,
                    PageSize = request.Filter.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.Filter.PageSize)
                };

                return Result<SystemSettingsListDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system settings");
                return Result<SystemSettingsListDto>.Failure("Error retrieving system settings");
            }
        }
    }
} 