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

namespace TruckFreight.Application.Features.Documents.Commands.UploadDocument
{
    public class UploadDocumentCommand : IRequest<Result<DocumentDto>>
    {
        public UploadDocumentDto Document { get; set; }
    }

    public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
    {
        public UploadDocumentCommandValidator()
        {
            RuleFor(x => x.Document.Type)
                .NotEmpty().WithMessage("Document type is required")
                .MaximumLength(50).WithMessage("Document type must not exceed 50 characters");

            RuleFor(x => x.Document.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(100).WithMessage("Title must not exceed 100 characters");

            RuleFor(x => x.Document.FileUrl)
                .NotEmpty().WithMessage("File URL is required");

            RuleFor(x => x.Document.FileType)
                .NotEmpty().WithMessage("File type is required")
                .MaximumLength(50).WithMessage("File type must not exceed 50 characters");

            RuleFor(x => x.Document.FileSize)
                .GreaterThan(0).WithMessage("File size must be greater than 0");

            RuleFor(x => x.Document.ReferenceId)
                .NotEmpty().WithMessage("Reference ID is required");

            RuleFor(x => x.Document.ReferenceType)
                .NotEmpty().WithMessage("Reference type is required")
                .MaximumLength(50).WithMessage("Reference type must not exceed 50 characters");

            RuleFor(x => x.Document.ExpiryDate)
                .NotEmpty().WithMessage("Expiry date is required")
                .GreaterThan(DateTime.UtcNow).WithMessage("Expiry date must be in the future");
        }
    }

    public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Result<DocumentDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UploadDocumentCommandHandler> _logger;

        public UploadDocumentCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<UploadDocumentCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<DocumentDto>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<DocumentDto>.Failure("User not authenticated");
                }

                // Get document type
                var documentType = await _context.DocumentTypes
                    .FirstOrDefaultAsync(dt => dt.Name == request.Document.Type, cancellationToken);

                if (documentType == null)
                {
                    return Result<DocumentDto>.Failure("Invalid document type");
                }

                // Validate file type
                if (!documentType.AllowedFileTypes.Contains(request.Document.FileType))
                {
                    return Result<DocumentDto>.Failure("File type not allowed");
                }

                // Validate file size
                if (request.Document.FileSize > documentType.MaxFileSize)
                {
                    return Result<DocumentDto>.Failure("File size exceeds maximum allowed size");
                }

                // Create document
                var document = new Document
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = request.Document.Type,
                    Title = request.Document.Title,
                    Description = request.Document.Description,
                    FileUrl = request.Document.FileUrl,
                    FileType = request.Document.FileType,
                    FileSize = request.Document.FileSize,
                    ReferenceId = request.Document.ReferenceId,
                    ReferenceType = request.Document.ReferenceType,
                    Status = "Pending",
                    VerificationStatus = "Pending",
                    ExpiryDate = request.Document.ExpiryDate,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    Metadata = request.Document.Metadata
                };

                _context.Documents.Add(document);

                // Create document history
                var history = new DocumentHistory
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = document.Id,
                    Action = "Uploaded",
                    Status = "Pending",
                    Comments = "Document uploaded",
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
                    ExpiryDate = document.ExpiryDate,
                    CreatedAt = document.CreatedAt,
                    CreatedBy = document.CreatedBy,
                    Metadata = document.Metadata
                };

                return Result<DocumentDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return Result<DocumentDto>.Failure("Error uploading document");
            }
        }
    }
} 