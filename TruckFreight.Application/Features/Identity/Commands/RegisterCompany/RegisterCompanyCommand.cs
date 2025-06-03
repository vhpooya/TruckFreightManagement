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

namespace TruckFreight.Application.Features.Identity.Commands.RegisterCompany
{
    public class RegisterCompanyCommand : IRequest<Result<IdentityResultDto>>
    {
        public RegisterCompanyDto Company { get; set; }
    }

    public class RegisterCompanyCommandValidator : AbstractValidator<RegisterCompanyCommand>
    {
        public RegisterCompanyCommandValidator()
        {
            // Company validation rules
            RuleFor(x => x.Company.CompanyName)
                .NotEmpty().WithMessage("Company name is required")
                .MaximumLength(100).WithMessage("Company name must not exceed 100 characters");

            RuleFor(x => x.Company.CompanyRegistrationNumber)
                .NotEmpty().WithMessage("Company registration number is required");

            RuleFor(x => x.Company.CompanyTaxId)
                .NotEmpty().WithMessage("Company tax ID is required");

            RuleFor(x => x.Company.CompanyAddress)
                .NotEmpty().WithMessage("Company address is required")
                .MaximumLength(200).WithMessage("Company address must not exceed 200 characters");

            RuleFor(x => x.Company.CompanyPhoneNumber)
                .NotEmpty().WithMessage("Company phone number is required")
                .Matches(@"^0[0-9]{10}$").WithMessage("Invalid company phone number format");

            RuleFor(x => x.Company.CompanyEmail)
                .NotEmpty().WithMessage("Company email is required")
                .EmailAddress().WithMessage("Invalid company email format");

            RuleFor(x => x.Company.CompanyWebsite)
                .MaximumLength(100).WithMessage("Company website must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Company.CompanyWebsite));

            RuleFor(x => x.Company.CompanyRegistrationPhotoUrl)
                .NotEmpty().WithMessage("Company registration photo is required");

            RuleFor(x => x.Company.CompanyTaxPhotoUrl)
                .NotEmpty().WithMessage("Company tax photo is required");

            // Representative validation rules
            RuleFor(x => x.Company.RepresentativeFirstName)
                .NotEmpty().WithMessage("Representative first name is required")
                .MaximumLength(50).WithMessage("Representative first name must not exceed 50 characters");

            RuleFor(x => x.Company.RepresentativeLastName)
                .NotEmpty().WithMessage("Representative last name is required")
                .MaximumLength(50).WithMessage("Representative last name must not exceed 50 characters");

            RuleFor(x => x.Company.RepresentativeNationalId)
                .NotEmpty().WithMessage("Representative national ID is required")
                .Length(10).WithMessage("Representative national ID must be 10 digits");

            RuleFor(x => x.Company.RepresentativePhoneNumber)
                .NotEmpty().WithMessage("Representative phone number is required")
                .Matches(@"^09[0-9]{9}$").WithMessage("Invalid representative phone number format");

            RuleFor(x => x.Company.RepresentativeEmail)
                .NotEmpty().WithMessage("Representative email is required")
                .EmailAddress().WithMessage("Invalid representative email format");

            RuleFor(x => x.Company.RepresentativePassword)
                .NotEmpty().WithMessage("Representative password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

            RuleFor(x => x.Company.RepresentativePosition)
                .NotEmpty().WithMessage("Representative position is required")
                .MaximumLength(100).WithMessage("Representative position must not exceed 100 characters");

            RuleFor(x => x.Company.RepresentativeProfilePhotoUrl)
                .NotEmpty().WithMessage("Representative profile photo is required");

            RuleFor(x => x.Company.RepresentativeNationalIdPhotoUrl)
                .NotEmpty().WithMessage("Representative national ID photo is required");
        }
    }

    public class RegisterCompanyCommandHandler : IRequestHandler<RegisterCompanyCommand, Result<IdentityResultDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly IIdentityService _identityService;
        private readonly ILogger<RegisterCompanyCommandHandler> _logger;

        public RegisterCompanyCommandHandler(
            UserManager<ApplicationUser> userManager,
            IApplicationDbContext context,
            IIdentityService identityService,
            ILogger<RegisterCompanyCommandHandler> logger)
        {
            _userManager = userManager;
            _context = context;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Result<IdentityResultDto>> Handle(RegisterCompanyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if company already exists
                var existingCompany = await _context.Companies
                    .FirstOrDefaultAsync(c => c.RegistrationNumber == request.Company.CompanyRegistrationNumber, cancellationToken);
                if (existingCompany != null)
                {
                    return Result<IdentityResultDto>.Failure("Company with this registration number already exists");
                }

                // Check if representative email already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Company.RepresentativeEmail);
                if (existingUser != null)
                {
                    return Result<IdentityResultDto>.Failure("User with this email already exists");
                }

                // Create company
                var company = new Company
                {
                    Name = request.Company.CompanyName,
                    RegistrationNumber = request.Company.CompanyRegistrationNumber,
                    TaxId = request.Company.CompanyTaxId,
                    Address = request.Company.CompanyAddress,
                    PhoneNumber = request.Company.CompanyPhoneNumber,
                    Email = request.Company.CompanyEmail,
                    Website = request.Company.CompanyWebsite,
                    RegistrationPhotoUrl = request.Company.CompanyRegistrationPhotoUrl,
                    TaxPhotoUrl = request.Company.CompanyTaxPhotoUrl,
                    Status = CompanyStatus.PendingVerification
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync(cancellationToken);

                // Create representative user
                var user = new ApplicationUser
                {
                    UserName = request.Company.RepresentativeEmail,
                    Email = request.Company.RepresentativeEmail,
                    PhoneNumber = request.Company.RepresentativePhoneNumber,
                    FirstName = request.Company.RepresentativeFirstName,
                    LastName = request.Company.RepresentativeLastName,
                    NationalId = request.Company.RepresentativeNationalId,
                    UserType = UserType.CompanyRepresentative
                };

                var result = await _userManager.CreateAsync(user, request.Company.RepresentativePassword);
                if (!result.Succeeded)
                {
                    // Rollback company creation
                    _context.Companies.Remove(company);
                    await _context.SaveChangesAsync(cancellationToken);
                    return Result<IdentityResultDto>.Failure(result.Errors.Select(e => e.Description).ToList());
                }

                // Add to CompanyRepresentative role
                await _userManager.AddToRoleAsync(user, "CompanyRepresentative");

                // Create company representative profile
                var representative = new CompanyRepresentative
                {
                    UserId = user.Id,
                    CompanyId = company.Id,
                    Position = request.Company.RepresentativePosition,
                    ProfilePhotoUrl = request.Company.RepresentativeProfilePhotoUrl,
                    NationalIdPhotoUrl = request.Company.RepresentativeNationalIdPhotoUrl,
                    Status = CompanyRepresentativeStatus.PendingVerification
                };

                _context.CompanyRepresentatives.Add(representative);
                await _context.SaveChangesAsync(cancellationToken);

                // Generate verification token
                var token = await _identityService.GenerateEmailConfirmationTokenAsync(user.Id);

                // Send verification email
                await _identityService.SendVerificationEmailAsync(user.Email, token);

                return Result<IdentityResultDto>.Success(new IdentityResultDto
                {
                    Succeeded = true,
                    UserId = user.Id,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering company");
                return Result<IdentityResultDto>.Failure("Error registering company");
            }
        }
    }
} 