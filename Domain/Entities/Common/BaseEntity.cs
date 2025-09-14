using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Domain.Entities.Common;

public interface IEntity
{
    
}
public interface ISoftDelete : IEntity<int>
{
    DateTimeOffset? DeleteDate { get; set; }

}

public interface IHashedEntity: IEntity<int>
{
    string Hash { get; set; }
    string SaltCode { get; set; }
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
    public int CreatorUserId { get; set; }
    [IgnoreDataMember]
    public User CreatorUser { get; set; }
}

public abstract class BaseEntity : BaseEntity<int>
{
    
}
