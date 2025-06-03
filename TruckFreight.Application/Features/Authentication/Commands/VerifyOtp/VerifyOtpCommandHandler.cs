using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Authentication.Commands.VerifyOtp;
using TruckFreight.Application.Services;

namespace TruckFreight.Application.Features.Authentication.Commands.VerifyOtp
{
    public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, Envelope<bool>>
    {
        private readonly AuthService _authService;
        private readonly ILogger<VerifyOtpCommandHandler> _logger;

        public VerifyOtpCommandHandler(AuthService authService, ILogger<VerifyOtpCommandHandler> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<Envelope<bool>> Handle(VerifyOtpCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Verifying OTP for {PhoneNumber}", command.PhoneNumber);
            var result = await _authService.VerifyOtpAsync(command.PhoneNumber, command.Otp);
            return Envelope<bool>.Success(result);
        }
    }
} 