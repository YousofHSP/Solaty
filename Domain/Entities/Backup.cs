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
    public class Backup : BaseEntity<int>
    {
        public string FileName { get; set; }
    }

    public class BackupConfiguration : IEntityTypeConfiguration<Backup>
    {
        public void Configure(EntityTypeBuilder<Backup> builder)
        {
            builder.HasOne(i => i.CreatorUser)
                .WithMany(u => u.CreatedBackups)
                .HasForeignKey(i => i.CreatorUserId);
        }
    }
}
