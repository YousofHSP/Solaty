using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Common;

namespace Domain.Model
{
    public interface IHashEntityValidator
    {
        Task<bool > IsValidAsync(IHashedEntity entity, int userId, CancellationToken ct, bool stopApp = true);
        Task<DateTimeOffset> RestoreEncryptedFieldsAsync(IHashedEntity entity);
        Task<AuditCheck> ValidateAllHashedEntitiesAsync(int userId, CancellationToken ct);
    }
}
