using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Documents.DTOs;

namespace TruckFreight.Application.Features.Documents.Commands.VerifyDocument
{
    public class VerifyDocumentCommand : IRequest<Result<DocumentDto>>
    {
        public string DocumentId { get; set; }
        public DocumentVerificationDto Verification { get; set; }
    }

    public class VerifyDocumentCommandValidator : AbstractValidator<VerifyDocumentCommand>
    {
        public VerifyDocumentCommandValidator()
        {
            RuleFor(x => x.DocumentId)
                .NotEmpty().WithMessage("Document ID is required");

            RuleFor(x => x.Verification.Status)
                .NotEmpty().WithMessage("Verification status is required")
                .MaximumLength(20).WithMessage("Verification status must not exceed 20 characters");

            RuleFor(x => x.Verification.Comments)
                .MaximumLength(500).WithMessage("Comments must not exceed 500 characters");
        }
    }

    public class VerifyDocumentCommandHandler : IRequestHandler<VerifyDocumentCommand, Result<DocumentDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<VerifyDocumentCommandHandler> _logger;

        public VerifyDocumentCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<VerifyDocumentCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<DocumentDto>> Handle(VerifyDocumentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<DocumentDto>.Failure("User not authenticated");
                }

                // Get document
                var document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);

                if (document == null)
                {
                    return Result<DocumentDto>.Failure("Document not found");
                }

                // Check if user has permission to verify documents
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null || !user.Roles.Contains("Admin") && !user.Roles.Contains("Verifier"))
                {
                    return Result<DocumentDto>.Failure("Unauthorized to verify documents");
                }

                // Update document verification status
                document.VerificationStatus = request.Verification.Status;
                document.VerifiedBy = userId;
                document.VerifiedAt = DateTime.UtcNow;
                document.Status = request.Verification.Status == "Approved" ? "Active" : "Rejected";
                document.UpdatedAt = DateTime.UtcNow;
                document.UpdatedBy = userId;

                // Create document history
                var history = new DocumentHistory
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = document.Id,
                    Action = "Verified",
                    Status = request.Verification.Status,
                    Comments = request.Verification.Comments,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                _context.DocumentHistories.Add(history);

                await _context.SaveChangesAsync(cancellationToken);

                // Map to DTO
                var result = new DocumentDto
                {
                    Id = document.Id,
                    Type = document.Type,
                    Title = document.Title,
                    Description = document.Description,
                    FileUrl = document.FileUrl,
                    FileType = document.FileType,
                    FileSize = document.FileSize,
                    ReferenceId = document.ReferenceId,
                    ReferenceType = document.ReferenceType,
                    Status = document.Status,
                    VerificationStatus = document.VerificationStatus,
                    VerifiedBy = document.VerifiedBy,
                    VerifiedAt = document.VerifiedAt,
                    ExpiryDate = document.ExpiryDate,
                    CreatedAt = document.CreatedAt,
                    CreatedBy = document.CreatedBy,
                    UpdatedAt = document.UpdatedAt,
                    UpdatedBy = document.UpdatedBy,
                    Metadata = document.Metadata
                };

                return Result<DocumentDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying document");
                return Result<DocumentDto>.Failure("Error verifying document");
            }
        }
    }
} 