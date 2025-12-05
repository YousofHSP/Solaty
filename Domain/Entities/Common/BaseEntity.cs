using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Domain.Entities.Common;

public interface IEntity
{
    
}
public interface ISoftDelete : IEntity<long>
{
    DateTimeOffset? DeleteDate { get; set; }

}

public interface IEntity<TKey> : IEntity
{
    TKey Id { get; set; }
}
public interface IBaseEntity : IEntity
{
    DateTimeOffset CreateDate { get; set; }
}
public interface IBaseEntity<TKey> : IBaseEntity, IEntity<TKey>
{

}

public abstract class BaseEntity<TKey> : IBaseEntity<TKey>
{
    public virtual TKey Id { get; set; }
    public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset UpdateDate { get; set; } = DateTimeOffset.Now;
}

public abstract class BaseEntity : BaseEntity<long>
{
    
}
