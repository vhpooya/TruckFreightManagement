using MediatR;
using TruckFreight.Application.Common.Models;

namespace TruckFreight.Application.Features.Authentication.Commands.VerifyOtp
{
    public class VerifyOtpCommand : IRequest<Envelope<bool>>
    {
        public string PhoneNumber { get; set; }
        public string Otp { get; set; }
    }
} 