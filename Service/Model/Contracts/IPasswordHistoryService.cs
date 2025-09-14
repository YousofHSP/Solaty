using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Service.Model.Contracts
{
    public interface IPasswordHistoryService
    {
        Task<PasswordHistory> CheckPasswordHistoryAsync(string password, User user, int createrUserId, CancellationToken ct);
        Task AddAsync(PasswordHistory model, CancellationToken ct);
    }
}
