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
    public class SmsLog : IBaseEntity<int>
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Mobile { get; set; }
        public int? ReceiverUserId { get; set; }
        public int? CreatorUserId { get; set; }


        public User? ReceiverUser { get; set; }
        public User? CreatorUser { get; set; }
        public DateTimeOffset CreateDate { get; set; }
    }
    public class SmsLogConfiguration : IEntityTypeConfiguration<SmsLog>
    {
        public void Configure(EntityTypeBuilder<SmsLog> builder)
        {
            builder.HasOne(i => i.CreatorUser)
                .WithMany(i => i.CreatedSmsLogs)
                .HasForeignKey(i => i.CreatorUserId);
            builder.HasOne(i => i.ReceiverUser)
                .WithMany(i => i.ReceivedSms)
                .HasForeignKey(i => i.ReceiverUserId);
        }
    }
}
