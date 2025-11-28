using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities
{
    public class ApiToken : IBaseEntity<long>
    {
        public long Id { get; set; }
        public string Ip { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTimeOffset CreateDate { get; set; }
        public string Code { get; set; } = string.Empty;
        public long UserId { get; set; }
        public bool Enable { get; set; }
        public DateTimeOffset LastUsedDate { get; set; }

        [IgnoreDataMember] public User User { get; set; } = null!;

    }

    public class ApiTokenConfiguration : IEntityTypeConfiguration<ApiToken>
    {
        public void Configure(EntityTypeBuilder<ApiToken> builder)
        {
            builder.HasOne(a => a.User)
                .WithMany(u => u.ApiTokens)
                .HasForeignKey(a => a.UserId);
        }
    }

}
