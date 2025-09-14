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
    private readonly List<PendingAuditEntry> _pendingAuditEntries = new();

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
            if (entry.Entity is Audit or Log or ArchiveLog)
                continue;
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
            if (entry.Entity is IHashedEntity hashedEntity)
            {
                var currentValues = entry.CurrentValues.Properties
                        .OrderBy(p => p.Name)
                        .Where(p => p.Name != "Id" &&
                                    p.Name != nameof(IHashedEntity.Hash) &&
                                    p.Name != nameof(IHashedEntity.SaltCode))
                        .ToDictionary(p => p.Name, p => entry.CurrentValues[p]?.ToString() ?? "");
                var saltCode = SecurityHelpers.GenerateSalt();
                var json = System.Text.Json.JsonSerializer.Serialize(currentValues);
                var hash = SecurityHelpers.GetSha256Hash(json, saltCode);


                var oldHash = hashedEntity.Hash;
                var oldSaltCode = hashedEntity.SaltCode;
                hashedEntity.SaltCode = saltCode;
                hashedEntity.Hash = hash;


                _pendingAuditEntries.Add(new PendingAuditEntry
                {
                    State = state,
                    NewValueString = JsonConvert.SerializeObject(entry.Entity),
                    NewHash = hash,
                    OldHash = oldHash,
                    OldSaltCode = oldSaltCode,
                    NewSaltCode = saltCode,
                    Entity = entry.Entity
                });
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
        var entries = new List<Audit>();

        foreach (var pending in _pendingAuditEntries)
        {
            var entry = new Audit
            {
                Method = pending.State.ToString(),
                NewValue = pending.NewValueString,
                Hash = pending.NewHash,
                OldHash= pending.OldHash,
                SaltCode = pending.NewSaltCode,
                CreateDate = DateTimeOffset.UtcNow,
                Model = pending.Entity.GetType().Name,
                ModelId = _getEntityId(pending.Entity),
                UserId = userId,
                Ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "",
                ReferrerLink = httpContextAccessor.HttpContext?.Request.Headers["Referer"].ToString() ?? "",
                Protocol = httpContextAccessor.HttpContext?.Request.Protocol ?? "",
                PhysicalPath = httpContextAccessor.HttpContext?.Request.Path.ToString() ?? "",
                RequestId = httpContextAccessor.HttpContext?.TraceIdentifier ?? "",
                UserAgent = httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "",

            };
            entries.Add(entry);

        }
        _pendingAuditEntries.Clear();
        if (entries.Count > 0)
        {
            await context.AddRangeAsync(entries);
            await context.SaveChangesAsync();
        }



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