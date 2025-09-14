using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; }
        public DateTimeOffset? SeenDate { get; set; }
        public int UserId { get; set; }

        public User User { get; set; }
        public NotificationStatus Status { get; set; }
    }
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasOne(i => i.CreatorUser)
                .WithMany(i => i.CreatedNotifications)
                .HasForeignKey(i => i.CreatorUserId);
            builder.HasOne(i => i.User)
                .WithMany(i => i.Notifications)
                .HasForeignKey(i => i.UserId);
        }
    }

    public enum NotificationStatus
    {
        [Display(Name = "دیده نشده")]
        Unseen,
        [Display(Name = "دیده شده")]
        Seen
    }
}
