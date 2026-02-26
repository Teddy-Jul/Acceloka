using Acceloka.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Acceloka.Features.Auth
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
    {
        private readonly AccelokaContext _db;

        public RegisterCommandHandler(AccelokaContext db)
        {
            _db = db;
        }

        public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var exists = await _db.Users
                .AnyAsync(u => u.Username == request.Username, cancellationToken);

            if (exists)
                throw new InvalidOperationException($"Username '{request.Username}' is already taken.");

            var user = new User
            {
                Username = request.Username,
                Password = request.Password,
                Email = request.Email,
                CreatedAt = DateTimeOffset.Now
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);

            return new RegisterResponse { UserId = user.UserId, Username = user.Username };
        }
    }
}