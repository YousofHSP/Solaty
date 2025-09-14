using Common.Exceptions;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Common;
using Data.Contracts;
using Data.Reprositories;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Data.Repositories
{
    public class UserRepository(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
        : Repository<User>(dbContext, httpContextAccessor), IUserRepository, IScopedDependency
    {
        public async Task<User?> GetByUserNameAndPass(string userName, string password, CancellationToken cancellationToken)
        {
            
            var user = await Table.Where(p => p.UserName == userName).SingleOrDefaultAsync(cancellationToken);
            if (user is null) return null;
            var isPasswordValid = await userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid) return null;
            await userManager.UpdateSecurityStampAsync(user);
            return user;

        }

        public Task UpdateSecurityStampAsync(User user, CancellationToken cancellationToken)
        {
            user.SecurityStamp = Guid.NewGuid().ToString();
            return UpdateAsync(user, cancellationToken);
        }

        public Task UpdateLastLoginDateAsync(User user, CancellationToken cancellationToken)
        {
            user.LastLoginDate = DateTimeOffset.Now;
            return UpdateAsync(user, cancellationToken);
        }

        public async Task AddAsync(User user, string password, CancellationToken cancellationToken)
        {
            var exists = await TableNoTracking.AnyAsync(u => u.UserName == user.UserName);
            if (exists)
                throw new BadRequestException("این نام کاربری تکراری است.");

            var passwordHash = SecurityHelpers.GetSha256Hash(password);
            user.PasswordHash = passwordHash;
            await base.AddAsync(user, cancellationToken);
        }
    }
}
