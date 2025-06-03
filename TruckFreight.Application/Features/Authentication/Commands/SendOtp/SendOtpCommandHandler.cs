using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Authentication.Commands.SendOtp;
using TruckFreight.Application.Services;

namespace TruckFreight.Application.Features.Authentication.Commands.SendOtp
{
    public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, Envelope<bool>>
    {
        private readonly AuthService _authService;
        private readonly ILogger<SendOtpCommandHandler> _logger;

        public SendOtpCommandHandler(AuthService authService, ILogger<SendOtpCommandHandler> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<Envelope<bool>> Handle(SendOtpCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending OTP to {PhoneNumber}", command.PhoneNumber);
            var result = await _authService.SendOtpAsync(command.PhoneNumber);
            return Envelope<bool>.Success(result);
        }
    }
} 