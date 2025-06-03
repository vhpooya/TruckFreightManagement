using System;
using MediatR;
using FluentValidation;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommand : IRequest<Guid>
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
    }

    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
            RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Role).NotEmpty().MaximumLength(20);
        }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreateUserCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var entity = new User(
                request.UserName,
                request.Email,
                request.PhoneNumber,
                request.FirstName,
                request.LastName,
                request.Role
            );
            _context.Users.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
    }
} 