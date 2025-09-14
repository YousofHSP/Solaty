

using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities;

public class Audit : IEntity<int>
{
    public int Id { get; set; }
    public string Ip { get; set; }
    public string ReferrerLink { get; set; }
    public string Protocol { get; set; }
    public string PhysicalPath { get; set; }
    public string RequestId { get; set; }
    public string Model { get; set; }
    public int ModelId { get; set; }
    public int? UserId { get; set; }
    public string Method { get; set; }
    public string NewValue { get; set; }
    public string? OldHash { get; set; }
    public string Hash { get; set; }
    public string SaltCode { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public string UserAgent { get; set; }

    public User User { get; set; }
}

public class AuditConfiguration : IEntityTypeConfiguration<Audit>
{
    public void Configure(EntityTypeBuilder<Audit> builder)
    {
        builder
            .HasOne(a => a.User)
            .WithMany(u => u.Audits)
            .HasForeignKey(a => a.UserId);
    }
}
