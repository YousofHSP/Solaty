using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities
{
    public class ArchiveLog : BaseEntity, ISoftDelete
    {
        public int Id { get; set; }
        public string ArchiveFileName { get; set; }
        public DateTimeOffset ArchivedUntilDate { get; set; }
        public DateTimeOffset ArchivedAt { get; set; }
        public int LogCount { get; set; }
        public int AuditCount { get; set; }
        public DateTimeOffset? DeleteDate { get; set; }
    }

    public class ArivhiveLogConfiguration : IEntityTypeConfiguration<ArchiveLog>
    {
        public void Configure(EntityTypeBuilder<ArchiveLog> builder)
        {
            builder.HasOne(a => a.CreatorUser)
                .WithMany(u => u.CreatedArchiveLogs)
                .HasForeignKey(a => a.CreatorUserId);
        }
    }
}
