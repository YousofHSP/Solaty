using Domain.Entities;

namespace Data.Contracts
{
    public interface IUserRepository: IRepository<User>
    {
        Task<User?> GetByUserNameAndPass(string userName, string password, CancellationToken cancellationToken);

        Task AddAsync(User user, string password, CancellationToken cancellationToken);
        Task UpdateSecurityStampAsync(User user, CancellationToken cancellationToken);
    }
}