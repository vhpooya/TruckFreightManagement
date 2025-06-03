using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Violations.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Violations.Commands.CreateViolation
{
    public class CreateViolationCommand : IRequest<Result<ViolationDto>>
    {
        public CreateViolationDto Violation { get; set; }
    }

    public class CreateViolationCommandValidator : AbstractValidator<CreateViolationCommand>
    {
        public CreateViolationCommandValidator()
        {
            RuleFor(x => x.Violation.DeliveryId)
                .NotEmpty().WithMessage("Delivery ID is required");

            RuleFor(x => x.Violation.Type)
                .NotEmpty().WithMessage("Violation type is required")
                .MaximumLength(100).WithMessage("Violation type must not exceed 100 characters");

            RuleFor(x => x.Violation.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

            RuleFor(x => x.Violation.FineAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Fine amount must be greater than or equal to 0");

            RuleFor(x => x.Violation.Evidence)
                .NotEmpty().WithMessage("Evidence is required")
                .MaximumLength(500).WithMessage("Evidence must not exceed 500 characters");

            RuleFor(x => x.Violation.Location)
                .NotEmpty().WithMessage("Location is required")
                .MaximumLength(500).WithMessage("Location must not exceed 500 characters");

            RuleFor(x => x.Violation.ViolationDate)
                .NotEmpty().WithMessage("Violation date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Violation date cannot be in the future");
        }
    }

    public class CreateViolationCommandHandler : IRequestHandler<CreateViolationCommand, Result<ViolationDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateViolationCommandHandler> _logger;

        public CreateViolationCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<CreateViolationCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<ViolationDto>> Handle(CreateViolationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<ViolationDto>.Failure("User not authenticated");
                }

                // Verify user is admin
                if (!_currentUserService.IsInRole("Admin"))
                {
                    return Result<ViolationDto>.Failure("Only administrators can create violations");
                }

                // Get delivery with related data
                var delivery = await _context.Deliveries
                    .Include(d => d.Driver)
                    .Include(d => d.CargoRequest)
                        .ThenInclude(cr => cr.CargoOwner)
                    .FirstOrDefaultAsync(d => d.Id == request.Violation.DeliveryId, cancellationToken);

                if (delivery == null)
                {
                    return Result<ViolationDto>.Failure("Delivery not found");
                }

                // Create violation record
                var violation = new Violation
                {
                    Id = Guid.NewGuid(),
                    DeliveryId = delivery.Id,
                    Type = request.Violation.Type,
                    Description = request.Violation.Description,
                    FineAmount = request.Violation.FineAmount,
                    Evidence = request.Violation.Evidence,
                    Location = request.Violation.Location,
                    ViolationDate = request.Violation.ViolationDate,
                    Status = ViolationStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                _context.Violations.Add(violation);

                // Add violation history
                var violationHistory = new ViolationHistory
                {
                    Id = Guid.NewGuid(),
                    ViolationId = violation.Id,
                    Status = ViolationStatus.Pending,
                    Description = "Violation created",
                    Timestamp = DateTime.UtcNow,
                    UpdatedBy = userId
                };

                _context.ViolationHistories.Add(violationHistory);

                await _context.SaveChangesAsync(cancellationToken);

                var result = new ViolationDto
                {
                    Id = violation.Id,
                    DeliveryId = violation.DeliveryId,
                    Type = violation.Type,
                    Description = violation.Description,
                    FineAmount = violation.FineAmount,
                    Evidence = violation.Evidence,
                    Location = violation.Location,
                    ViolationDate = violation.ViolationDate,
                    Status = violation.Status,
                    CreatedAt = violation.CreatedAt,
                    CreatedBy = violation.CreatedBy,
                    DriverName = $"{delivery.Driver.FirstName} {delivery.Driver.LastName}",
                    CargoOwnerName = $"{delivery.CargoRequest.CargoOwner.FirstName} {delivery.CargoRequest.CargoOwner.LastName}"
                };

                return Result<ViolationDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating violation");
                return Result<ViolationDto>.Failure("Error creating violation");
            }
        }
    }
} 