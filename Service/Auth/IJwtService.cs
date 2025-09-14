using Domain.Auth;
using Domain.Entities;

namespace Service.Auth
{
    public interface IJwtService
    {
        Task<AccessToken> GenerateAsync(User user, CancellationToken ct);
    }
}