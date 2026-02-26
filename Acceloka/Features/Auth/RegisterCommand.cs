using MediatR;

namespace Acceloka.Features.Auth
{
    public class RegisterCommand : IRequest<RegisterResponse>
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class RegisterResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}