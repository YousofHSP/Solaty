using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Domain.Entities.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities;

[Display(Name = "نقش")]
public class Role : IdentityRole<long>, IBaseEntity<long>
{
    public string? Title { get; set; }
    public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? DeleteDate { get; set; } = null;
    [IgnoreDataMember]
    public List<User> Users { get; set; } = [];
}
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasMany(i => i.Users)
            .WithMany(i => i.Roles);
    }
}