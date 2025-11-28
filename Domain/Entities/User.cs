using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Domain.Entities.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities;

public enum UserStatus
{
    [Display(Name = "غیرفعال")] Disable,
    [Display(Name = "فعال")] Enable,
    [Display(Name = "ناقص")] Imperfect
}

[Display(Name = "کاربران")]
public class User : IdentityUser<long>, IBaseEntity<long> 
{
    public DateTimeOffset LastLoginDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? DeleteDate { get; set; } = null;
    public UserStatus Status { get; set; }

    [IgnoreDataMember] public List<Role> Roles { get; set; } = [];
    [IgnoreDataMember] public UserInfo? Info { get; set; }

    [IgnoreDataMember] public List<ApiToken> ApiTokens { get; set; } = [];
    [IgnoreDataMember] public List<SmsLog> ReceivedSms { get; set; } = [];
    [IgnoreDataMember] public List<Notification> Notifications { get; set; } = [];

    #region CreatedModels

    [IgnoreDataMember] public List<UploadedFile> CreatedUploadedFiles { get; set; } = [];
    [IgnoreDataMember] public List<IpAccessType> CreatedIpAccessTypes { get; set; } = [];
    [IgnoreDataMember] public List<IpRule> CreatedIpRules { get; set; } = [];
    [IgnoreDataMember] public List<SmsLog> CreatedSmsLogs { get; set; } = [];
    [IgnoreDataMember] public List<Notification> CreatedNotifications { get; set; } = [];
    #endregion
}

public class UserInfo : IBaseEntity<long> 
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    [IgnoreDataMember] public User User { get; set; } = null!;
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(user => user.UserName).IsRequired().HasMaxLength(100);
        builder.HasMany(u => u.Roles)
            .WithMany(r => r.Users);
        builder.HasOne(u => u.Info)
            .WithOne(i => i.User)
            .HasForeignKey<UserInfo>(i => i.UserId);
        builder.HasMany(u => u.ApiTokens)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId);

        builder.HasMany(i => i.ReceivedSms)
            .WithOne(i => i.ReceiverUser)
            .HasForeignKey(i => i.ReceiverUserId);
        builder.HasMany(i => i.Notifications)
            .WithOne(i => i.User)
            .HasForeignKey(i => i.UserId);

        #region CreatedModels

        builder.HasMany(u => u.CreatedUploadedFiles)
            .WithOne(f => f.CreatorUser)
            .HasForeignKey(f => f.CreatorUserId);
        builder.HasMany(u => u.CreatedIpAccessTypes)
            .WithOne(i => i.CreatorUser)
            .HasForeignKey(i => i.CreatorUserId);
        builder.HasMany(u => u.CreatedIpRules)
            .WithOne(i => i.CreatorUser)
            .HasForeignKey(i => i.CreatorUserId);
        builder.HasMany(u => u.CreatedSmsLogs)
            .WithOne(i => i.CreatorUser)
            .HasForeignKey(i => i.CreatorUserId);
        builder.HasMany(u => u.CreatedNotifications)
            .WithOne(i => i.CreatorUser)
            .HasForeignKey(i => i.CreatorUserId);
        #endregion
    }
}

public class UserInfoConfiguration : IEntityTypeConfiguration<UserInfo>
{
    public void Configure(EntityTypeBuilder<UserInfo> builder)
    {
        builder.Property(i => i.Address).HasDefaultValue("");
        builder.HasOne(i => i.User)
            .WithOne(u => u.Info)
            .HasForeignKey<UserInfo>(i => i.UserId);
    }
}