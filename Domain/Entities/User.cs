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
    public string Hash { get; set; }
    public string SaltCode { get; set; }

    [IgnoreDataMember] public List<Role> Roles { get; set; } = [];
    [IgnoreDataMember] public UserInfo? Info { get; set; }

    [IgnoreDataMember] public List<PasswordHistory> PasswordHistoriess { get; set; }


    [IgnoreDataMember] public List<PhoneNumberHistory> PhoneNumberHistories { get; set; }
    [IgnoreDataMember] public List<EmailHistory> EmailHistories { get; set; }
    [IgnoreDataMember] public List<ApiToken> ApiTokens { get; set; }
    [IgnoreDataMember] public List<SmsLog> ReceivedSms { get; set; }
    [IgnoreDataMember] public List<Notification> Notifications { get; set; }

    #region CreatedModels

    [IgnoreDataMember] public List<Audit> Audits { get; set; }
    [IgnoreDataMember] public List<PasswordHistory> CreatedPasswordHistories { get; set; }
    [IgnoreDataMember] public List<PhoneNumberHistory> CreatedPhoneNumberHistories { get; set; }
    [IgnoreDataMember] public List<EmailHistory> CreatedEmailHistories { get; set; }
    [IgnoreDataMember] public List<ArchiveLog> CreatedArchiveLogs { get; set; }
    [IgnoreDataMember] public List<UploadedFile> CreatedUploadedFiles { get; set; }
    [IgnoreDataMember] public List<IpAccessType> CreatedIpAccessTypes { get; set; }
    [IgnoreDataMember] public List<IpRule> CreatedIpRules { get; set; }
    [IgnoreDataMember] public List<Backup> CreatedBackups { get; set; }
    [IgnoreDataMember] public List<SmsLog> CreatedSmsLogs { get; set; }
    [IgnoreDataMember] public List<Notification> CreatedNotifications { get; set; }
    [IgnoreDataMember] public List<AuditCheck> CreatedAuditChecks { get; set; }
    [IgnoreDataMember] public List<ImportedRecord> CreatedImportedRecords { get; set; }
    [IgnoreDataMember] public List<ImportedFile> CreatedImportedFiles { get; set; }
    #endregion
}

public class UserInfo : IBaseEntity<long> 
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string FullName { get; set; }
    public string Address { get; set; }

    public DateTime? BirthDate { get; set; }
    public DateTimeOffset? DeleteDate { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public string Hash { get; set; }
    public string SaltCode { get; set; }
    [IgnoreDataMember] public User User { get; set; }
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

        builder.HasMany(u => u.PhoneNumberHistories)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId);
        builder.HasMany(u => u.EmailHistories)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId);
        builder.HasMany(i => i.ReceivedSms)
            .WithOne(i => i.ReceiverUser)
            .HasForeignKey(i => i.ReceiverUserId);
        builder.HasMany(i => i.Notifications)
            .WithOne(i => i.User)
            .HasForeignKey(i => i.UserId);

        #region CreatedModels

        builder.HasMany(u => u.Audits)
            .WithOne(a => a.User)
            .HasForeignKey(u => u.UserId);
        builder.HasMany(u => u.PasswordHistoriess)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId);

        builder.HasMany(u => u.CreatedPasswordHistories)
            .WithOne(p => p.CreatorUser)
            .HasForeignKey(p => p.CreatorUserId);

        builder.HasMany(u => u.CreatedPhoneNumberHistories)
            .WithOne(p => p.CreatorUser)
            .HasForeignKey(p => p.CreatorUserId);
        builder.HasMany(u => u.CreatedEmailHistories)
            .WithOne(p => p.CreatorUser)
            .HasForeignKey(p => p.CreatorUserId);

        builder.HasMany(u => u.CreatedUploadedFiles)
            .WithOne(f => f.CreatorUser)
            .HasForeignKey(f => f.CreatorUserId);
        builder.HasMany(u => u.CreatedArchiveLogs)
            .WithOne(a => a.CreatorUser)
            .HasForeignKey(a => a.CreatorUserId);
        builder.HasMany(u => u.Audits)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId);
        builder.HasMany(u => u.CreatedIpAccessTypes)
            .WithOne(i => i.CreatorUser)
            .HasForeignKey(i => i.CreatorUserId);
        builder.HasMany(u => u.CreatedIpRules)
            .WithOne(i => i.CreatorUser)
            .HasForeignKey(i => i.CreatorUserId);
        builder.HasMany(u => u.CreatedBackups)
            .WithOne(i => i.CreatorUser)
            .HasForeignKey(i => i.CreatorUserId);
        builder.HasMany(u => u.CreatedSmsLogs)
            .WithOne(i => i.CreatorUser)
            .HasForeignKey(i => i.CreatorUserId);
        builder.HasMany(u => u.CreatedNotifications)
            .WithOne(i => i.CreatorUser)
            .HasForeignKey(i => i.CreatorUserId);
        builder.HasMany(u => u.CreatedAuditChecks)
            .WithOne(i => i.CreatorUser)
            .HasForeignKey(i => i.CreatorUserId);
        builder.HasMany(u => u.CreatedImportedRecords)
            .WithOne(i => i.CreatorUser)
            .HasForeignKey(i => i.CreatorUserId);
        builder.HasMany(i => i.CreatedImportedFiles)
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