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
    public class SmsLog : IBaseEntity<long>
    {
        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public long? ReceiverUserId { get; set; }
        public long? CreatorUserId { get; set; }


        public User? ReceiverUser { get; set; }
        public User? CreatorUser { get; set; }
        public DateTimeOffset CreateDate { get; set; }
    }
    public class SmsLogConfiguration : IEntityTypeConfiguration<SmsLog>
    {
        public void Configure(EntityTypeBuilder<SmsLog> builder)
        {
            builder.HasOne(i => i.ReceiverUser)
                .WithMany(i => i.ReceivedSms)
                .HasForeignKey(i => i.ReceiverUserId);
        }
    }
}
