using MediatR;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Authentication.Models;

namespace TruckFreight.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommand : IRequest<Envelope<AuthResponse>>
    {
        public RegisterRequest Request { get; }
        public RegisterCommand(RegisterRequest request) => Request = request;
    }
}
