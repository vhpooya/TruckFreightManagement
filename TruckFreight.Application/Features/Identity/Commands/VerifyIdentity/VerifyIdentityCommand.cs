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

namespace TruckFreight.Application.Features.Identity.Commands.VerifyIdentity
{
    public class VerifyIdentityCommand : IRequest<Result<IdentityResultDto>>
    {
        public VerifyIdentityDto Verification { get; set; }
    }

    public class VerifyIdentityCommandValidator : AbstractValidator<VerifyIdentityCommand>
    {
        public VerifyIdentityCommandValidator()
        {
            RuleFor(x => x.Verification.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.Verification.VerificationCode)
                .NotEmpty().WithMessage("Verification code is required");

            RuleFor(x => x.Verification.DocumentType)
                .NotEmpty().WithMessage("Document type is required");

            RuleFor(x => x.Verification.DocumentNumber)
                .NotEmpty().WithMessage("Document number is required");

            RuleFor(x => x.Verification.DocumentPhotoUrl)
                .NotEmpty().WithMessage("Document photo is required");
        }
    }

    public class VerifyIdentityCommandHandler : IRequestHandler<VerifyIdentityCommand, Result<IdentityResultDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly IIdentityService _identityService;
        private readonly ILogger<VerifyIdentityCommandHandler> _logger;

        public VerifyIdentityCommandHandler(
            UserManager<ApplicationUser> userManager,
            IApplicationDbContext context,
            IIdentityService identityService,
            ILogger<VerifyIdentityCommandHandler> logger)
        {
            _userManager = userManager;
            _context = context;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Result<IdentityResultDto>> Handle(VerifyIdentityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get user
                var user = await _userManager.FindByIdAsync(request.Verification.UserId);
                if (user == null)
                {
                    return Result<IdentityResultDto>.Failure("User not found");
                }

                // Verify email
                var emailVerificationResult = await _identityService.VerifyEmailAsync(
                    user.Id,
                    request.Verification.VerificationCode);

                if (!emailVerificationResult.Succeeded)
                {
                    return Result<IdentityResultDto>.Failure(emailVerificationResult.Errors);
                }

                // Update user status based on user type
                switch (user.UserType)
                {
                    case UserType.Driver:
                        var driver = await _context.Drivers
                            .FirstOrDefaultAsync(d => d.UserId == user.Id, cancellationToken);
                        if (driver != null)
                        {
                            driver.Status = DriverStatus.Verified;
                            await _context.SaveChangesAsync(cancellationToken);
                        }
                        break;

                    case UserType.CargoOwner:
                        var cargoOwner = await _context.CargoOwners
                            .FirstOrDefaultAsync(c => c.UserId == user.Id, cancellationToken);
                        if (cargoOwner != null)
                        {
                            cargoOwner.Status = CargoOwnerStatus.Verified;
                            await _context.SaveChangesAsync(cancellationToken);
                        }
                        break;

                    case UserType.CompanyRepresentative:
                        var representative = await _context.CompanyRepresentatives
                            .FirstOrDefaultAsync(r => r.UserId == user.Id, cancellationToken);
                        if (representative != null)
                        {
                            representative.Status = CompanyRepresentativeStatus.Verified;
                            var company = await _context.Companies
                                .FirstOrDefaultAsync(c => c.Id == representative.CompanyId, cancellationToken);
                            if (company != null)
                            {
                                company.Status = CompanyStatus.Verified;
                            }
                            await _context.SaveChangesAsync(cancellationToken);
                        }
                        break;
                }

                // Generate JWT token
                var token = await _identityService.GenerateJwtTokenAsync(user);

                return Result<IdentityResultDto>.Success(new IdentityResultDto
                {
                    Succeeded = true,
                    UserId = user.Id,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying identity");
                return Result<IdentityResultDto>.Failure("Error verifying identity");
            }
        }
    }
} 