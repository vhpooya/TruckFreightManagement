using MediatR;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Authentication.Models;

namespace TruckFreight.Application.Features.Authentication.Commands.Login
{
    public class LoginCommand : IRequest<Envelope<AuthResponse>>
    {
        public LoginRequest Request { get; }
        public LoginCommand(LoginRequest request) => Request = request;
    }
}
