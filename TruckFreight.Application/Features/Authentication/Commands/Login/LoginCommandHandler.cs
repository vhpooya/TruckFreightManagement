using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Authentication.Commands.Login;
using TruckFreight.Application.Features.Authentication.Models;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Authentication.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Envelope<AuthResponse>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IUnitOfWork uow,
            IPasswordHasher<User> passwordHasher,
            IJwtService jwtService,
            ILogger<LoginCommandHandler> logger)
        {
            _uow = uow;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<Envelope<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var req = command.Request;
            var user = await _uow.Users.GetByPhoneAsync(req.PhoneNumber, cancellationToken);
            if (user == null)
                return Envelope<AuthResponse>.Failure(new[] { "Invalid credentials." }.ToList());

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Failed login attempt for {PhoneNumber}", req.PhoneNumber);
                return Envelope<AuthResponse>.Failure(new[] { "Invalid credentials." }.ToList());
            }

            var token = _jwtService.GenerateToken(user);
            var expiry = _jwtService.GetExpiry();
            var response = new AuthResponse { UserId = user.Id, Token = token, Expiry = expiry };
            return Envelope<AuthResponse>.Success(response);
        }
    }
}
