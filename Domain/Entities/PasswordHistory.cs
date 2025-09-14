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
    public class PasswordHistory : BaseEntity, IHashedEntity
    {
        public int UserId { get; set; }
        public string PasswordHash { get; set; }
        public string Hash { get; set; }
        public string SaltCode { get; set; }

        public User User { get; set; }
    }

    public class PasswordHistoryConfiguration : IEntityTypeConfiguration<PasswordHistory>
    {
        public void Configure(EntityTypeBuilder<PasswordHistory> builder)
        {
            builder.HasOne(i => i.User)
                .WithMany(i => i.PasswordHistoriess)
                .HasForeignKey(i => i.UserId);

            builder.HasOne(i => i.CreatorUser)
                .WithMany(i => i.CreatedPasswordHistories)
                .HasForeignKey(i => i.CreatorUserId);
        }
    }
}
