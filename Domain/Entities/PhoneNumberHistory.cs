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
    public class PhoneNumberHistory : BaseEntity, IHashedEntity
    {
        public int UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string OtpCode { get; set; }
        public bool IsConfirmed { get; set; }
        public string Hash { get; set; }
        public string SaltCode { get; set; }

        public User User { get; set; }
    }
    public class PhoneNumberHistoryConfiguration : IEntityTypeConfiguration<PhoneNumberHistory>
    {
        public void Configure(EntityTypeBuilder<PhoneNumberHistory> builder)
        {
            builder
                .HasOne(p => p.User)
                .WithMany(u => u.PhoneNumberHistories)
                .HasForeignKey(u => u.UserId);
            builder
                .HasOne(p => p.CreatorUser)
                .WithMany(u => u.CreatedPhoneNumberHistories)
                .HasForeignKey(u => u.CreatorUserId);
        }
    }
}
