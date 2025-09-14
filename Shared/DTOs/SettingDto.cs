using System.ComponentModel.DataAnnotations;
using Domain.Entities;

namespace Shared.DTOs
{
    public class SettingDto : BaseDto<SettingDto, Setting>
    {
        [Display(Name = "مقدار")]
        public string Value { get; set; }
        public bool Enable{ get; set; }
    }
    public class SettingResDto : BaseDto<SettingResDto, Setting>
    {
        [Display(Name = "عنوان")]
        public string Title { get; set; }
        [Display(Name = "توضیحات")]
        public string Description { get; set; }
        [Display(Name = "مقدار")]
        public string Value { get; set; }
        [Display(Name = "امنیتی")]
        public int IsSecurity { get; set; }
        [Display(Name = "وضعیت")]
        public bool Enable{ get; set; }

    }
    
    public enum SettingKey

    {
        [Display(Name = "حالت برنامه")]
        AppMode = 1,
        [Display(Name = "حداکثر تعداد تلاش ناموق برای ورود")]
        MaxLoginFail,
        [Display(Name = "الزام به وجود عدد در رمز عبور")]
        PasswordRequireDigit,
        [Display(Name = "حداقل طول رمز عبور")]
        PasswordRequiredLength,
        [Display(Name = "الزام به وجود کاراکتر غیرحروفی(مثل @، #، %) در رمز عبور")]
        PasswordRequireNonAlphanumeric,
        [Display(Name = "الزام به وجود حروف بزرگ انگلیسی در رمز عبور")]
        PasswordRequireUppercase,
        [Display(Name = "الزام به وجود حروف کوچک انگلیسی در رمز عبور")]
        PasswordRequireLowercase,
        [Display(Name = "امکان فعال بودن هم‌زمان چند نشست (توکن)")]
        SimultaneousTokenActivation,
        [Display(Name = "مدت زمان غیرفعال شدن حساب کاربری پس از خطاهای امنیتی (به ساعت)")]
        MaxAccountDisableHour,
        [Display(Name = "مدت اعتبار توکن JWT (بر حسب دقیقه)")]
        JWTExpirationMinutes,
        [Display(Name = "سطح لاگ‌گیری (Info, Warning, Error, Critical)")]
        LoggingLevel,
        [Display(Name = "فعال بودن لاگ‌گیری در MongoDB")]
        LoggingStorageMongoEnabled,
        [Display(Name = "اجبار استفاده از HTTPS")]
        SecurityRequireHttps,
        [Display(Name = "فعال بودن محافظ CSRF")]
        SecurityEnableCSRFProtection,
        [Display(Name = "کلید رمزنگاری اطلاعات حساس")]
        SecurityDataEncryptionKey,
        [Display(Name = "برنامه زمان‌بندی پشتیبان‌گیری")]
        SystemBackupSchedule,
        [Display(Name = "ایمیل دریافت هشدارها")]
        SystemAdminNotificationEmail,
        [Display(Name = "نوع لاگین")]
        LoginType,
        [Display(Name = "ثبت گزارش اقدامات کاربران")]
        AuditLogEnabled,
        [Display(Name = "نگهداری گزارش‌ها به مدت X روز")]
        AuditLogRetentionDays,
        [Display(Name = "ثبت IP برای هر نشست کاربر")]
        UserSessionTrackIP,
        [Display(Name = "امکان تعریف نقش‌های جدید")]
        AccessControlRolesEditable,
        [Display(Name = "ورژن سیستم")]
        SystemVersion,
        [Display(Name = "جلوگیری از نمایش سایت داخل iframe")]
        SecurityXFrameOptions,
        [Display(Name = "سیاست امنیتی محتوا")]
        SecurityContentSecurityPolicy,
        [Display(Name = "فعال‌سازی HSTS برای اجبار HTTPS")]
        SecurityHSTSEnabled,
        [Display(Name = "مدت اعتبار HSTS (روز)")]
        SecurityHSTSMaxAgeDays,
        [Display(Name = "ارسال ایمیل خودکار (مثلاً در رخداد خطا یا هشدار امنیتی)")]
        SystemSupportContact,
        [Display(Name = "آدرس سرور SMTP")]
        EmailSMTPHost,
        [Display(Name = "پورت SMTP")]
        EmailSMTPPort,
        [Display(Name = "فعال‌سازی SSL برای ایمیل")]
        EmailSMTPSSL,
        [Display(Name = "تعداد چک رمزهای قبلی برای تکراری نبودن")]
        PasswordHistoryCount,
        [Display(Name = "ورود با شماره موبایل")]
        LoginWithPhoneNumber,
        [Display(Name = "اسم برنامه")]
        AppName,
        [Display(Name = "لوگو برنامه")]
        AppLogo,
        [Display(Name = "حجم فایل (MB)")]
        MaxFileSizeInMB,
        [Display(Name = "نوع های مجاز")]
        AllowedFileTypes,
        [Display(Name = "حداکثر طول عکس")]
        MaxImageWidth,
        [Display(Name = "حداکثر ارتفاع عکس")]
        MaxImageHeight,
        [Display(Name = "نمایش زیرسیستم")]
        ShowSubSystem,
        [Display(Name = "لایسنس")]
        License
    }
}
