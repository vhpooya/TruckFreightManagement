using MediatR;
using TruckFreight.Application.Common.Models;

namespace TruckFreight.Application.Features.Authentication.Commands.SendOtp
{
    public class SendOtpCommand : IRequest<Envelope<bool>>
    {
        public string PhoneNumber { get; set; }
    }
} 