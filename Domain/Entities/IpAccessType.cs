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
    public class IpAccessType: BaseEntity, IHashedEntity
    {
        public int IpRuleId { get; set; }
        public AccessType AccessType { get; set; }
        public string Hash { get; set; }
        public string SaltCode { get; set; }

        public IpRule IpRule { get; set; }
    }

    public class IpAccessTypeConfiguration : IEntityTypeConfiguration<IpAccessType>
    {
        public void Configure(EntityTypeBuilder<IpAccessType> builder)
        {
            builder.HasOne(i => i.IpRule)
                .WithMany(i => i.IpAccessTypes)
                .HasForeignKey(i => i.IpRuleId);

            builder.HasOne(i => i.CreatorUser)
                .WithMany(u => u.CreatedIpAccessTypes)
                .HasForeignKey(i => i.CreatorUserId);
        }
    }

    public enum AccessType
    {
        WhiteList,
        BlackList,
        RestrictedAccess
    }
}
