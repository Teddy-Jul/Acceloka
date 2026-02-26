using Acceloka.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Acceloka.Features.Auth
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, LoginResponse>
    {
        private readonly AccelokaContext _db;

        public LoginQueryHandler(AccelokaContext db)
        {
            _db = db;
        }

        public async Task<LoginResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

            if (user == null || user.Password != request.Password)
                throw new UnauthorizedAccessException("Invalid username or password.");

            return new LoginResponse { UserId = user.UserId, Username = user.Username };
        }
    }
}