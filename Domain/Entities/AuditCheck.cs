using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities
{
    public class AuditCheck : BaseEntity
    {
        public int TablesCheckCount { get; set; }

        public List<AuditCheckDetail> AuditCheckDetails { get; set; }
    }
    public class AuditCheckDetail : IBaseEntity<int>
    {
        public DateTimeOffset CreateDate { get; set; }
        public int AuditCheckId { get; set; }
        public int ModelId { get; set; }
        public string Model { get; set; }
        public AuditCheckDetailStatus Status { get; set; }
        public DateTimeOffset? AuditCreateDate { get; set; }

        public AuditCheck AuditCheck { get; set; }
        public int Id { get; set; }
    }

    public class AuditCheckConfiguration : IEntityTypeConfiguration<AuditCheck>
    {
        public void Configure(EntityTypeBuilder<AuditCheck> builder)
        {
            builder.HasMany(i => i.AuditCheckDetails)
                .WithOne(i => i.AuditCheck)
                .HasForeignKey(i => i.AuditCheckId);
            builder.HasOne(i => i.CreatorUser)
                .WithMany(i => i.CreatedAuditChecks)
                .HasForeignKey(i => i.CreatorUserId);
        }
    }
    public class AuditCheckDetailConfiguration : IEntityTypeConfiguration<AuditCheckDetail>
    {
        public void Configure(EntityTypeBuilder<AuditCheckDetail> builder)
        {
            builder.HasOne(i => i.AuditCheck)
                .WithMany(i => i.AuditCheckDetails)
                .HasForeignKey(i => i.AuditCheckId);
        }
    }

    public enum AuditCheckDetailStatus
    {
        Invalid,
        Valid
    }
}
