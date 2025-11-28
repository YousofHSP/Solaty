using Common.Utilities;
using Domain.Entities;
using Domain.Entities.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Service.Interceptors;

public class PendingAuditEntry
{
    public EntityState State { get; set; }
    public string OldValueString { get; set; }
    public string NewValueString { get; set; }
    public string NewHash { get; set; }
    public string OldHash { get; set; }
    public string NewSaltCode { get; set; }
    public string OldSaltCode { get; set; }
    public object Entity { get; set; }
}

public class AuditInterceptor(
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuditInterceptor> logger
    //IRepository<Audit> auditRepository
    )
    : SaveChangesInterceptor
{

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChangesAsync(eventData, result);

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

        foreach (var entry in entries)
        {
            var state = entry.State;
            if (state == EntityState.Added)
            {
                if (entry.Entity is IBaseEntity entity)
                {
                    entity.CreateDate = DateTimeOffset.Now;
                }

                if (entry.Entity is BaseEntity baseEntity)
                {
                    baseEntity.UpdateDate = DateTimeOffset.Now;
                }
            }else if (state == EntityState.Modified)
            {
                if (entry.Entity is BaseEntity entity)
                {
                    entity.UpdateDate = DateTimeOffset.Now;
                }
            }


            if (state == EntityState.Deleted)
            {
                if (entry.Entity is ISoftDelete deletable)
                {
                    entry.State = EntityState.Modified;
                    deletable.DeleteDate = DateTimeOffset.Now;
                }
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var userId = httpContextAccessor.HttpContext?.User.Identity?.GetUserId<int>();
        userId = userId == 0 ? null : userId;
        var context = eventData.Context;
        if (context == null) return await base.SavedChangesAsync(eventData, result, cancellationToken);

        


        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private int _getEntityId(object entity)
    {
        var property = entity.GetType().GetProperty("Id");
        if (property != null && property.PropertyType == typeof(int))
        {
            return (int)(property.GetValue(entity) ?? 0);
        }

        return 0;
    }
}