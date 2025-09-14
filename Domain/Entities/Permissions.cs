namespace Domain.Entities;

public static class Permissions
{
    public static List<Permission> All =
    [
        
        new() { Controller = "Role", ControllerLabel = "نقش",  Action = "Index", ActionLabel = "نمایش" },
        new() { Controller = "Role", ControllerLabel = "نقش", Action = "Create",  ActionLabel = "ایجاد" },
        new() { Controller = "Role", ControllerLabel = "نقش", Action = "Update",  ActionLabel = "ویرایش" },
        new() { Controller = "Role", ControllerLabel = "نقش", Action = "Delete", ActionLabel = "حذف" },
        
        new() { Controller = "User", Action = "Index", ControllerLabel = "کاربر", ActionLabel = "نمایش" },
        new() { Controller = "User", Action = "Create", ControllerLabel = "کاربر", ActionLabel = "ایجاد" },
        new() { Controller = "User", Action = "Update", ControllerLabel = "کاربر", ActionLabel = "ویرایش" },
        new() { Controller = "User", Action = "Delete", ControllerLabel = "کاربر", ActionLabel = "حذف" },
        new() { Controller = "User", Action = "GetTokens", ControllerLabel = "کاربر", ActionLabel = "لیست نشست ها" },
        new() { Controller = "User", Action = "DisableTokens", ControllerLabel = "کاربر", ActionLabel = "غیرفعال کردن توکن" },

        new() { Controller = "Log", Action = "LogIndex", ControllerLabel = "لاگ", ActionLabel = "لیست لاگ ها" },
        new() { Controller = "Log", Action = "AuditIndex", ControllerLabel = "لاگ", ActionLabel = "لیست ممیزی ها" },
        new() { Controller = "Log", Action = "ArchiveLogs", ControllerLabel = "لاگ", ActionLabel = "ثبت آرشیو" },
        new() { Controller = "Log", Action = "ArchiveLogsIndex", ControllerLabel = "لاگ", ActionLabel = "لیست آرشیو" },
        new() { Controller = "Log", Action = "RestoreArchive", ControllerLabel = "لاگ", ActionLabel = "بازگردانی آرشیو" },

        new() { Controller = "Setting", Action = "Index", ControllerLabel = "تنظیمات", ActionLabel = "نمایش" },
        new() { Controller = "Setting", Action = "Update", ControllerLabel = "تنظیمات", ActionLabel = "ویرایش" },

        new() { Controller = "Sms", Action = "Index", ControllerLabel = "پیامک", ActionLabel = "نمایش" },
        new() { Controller = "Sms", Action = "ReSend", ControllerLabel = "پیامک", ActionLabel = "ارسال مجدد" },

        new() { Controller = "SubSystem", Action = "Index", ControllerLabel = "زیرسیستم", ActionLabel = "نمایش" },
        new() { Controller = "SubSystem", Action = "Create", ControllerLabel = "زیرسیستم", ActionLabel = "ایجاد" },
        new() { Controller = "SubSystem", Action = "Update", ControllerLabel = "زیرسیستم", ActionLabel = "ویرایش" },
        new() { Controller = "SubSystem", Action = "Delete", ControllerLabel = "زیرسیستم", ActionLabel = "حذف" },

        new() { Controller = "System", Action = "BackupDatabase", ControllerLabel = "سیستم", ActionLabel = "بک آپ دیتابیس" },
        new() { Controller = "System", Action = "BackupsList", ControllerLabel = "سیستم", ActionLabel = "لیست بکآپ ها" },
        new() { Controller = "System", Action = "DownloadBackup", ControllerLabel = "سیستم", ActionLabel = "دانلود بکآپ" },
        new() { Controller = "System", Action = "AuditsCheckIndex", ControllerLabel = "سیستم", ActionLabel = "لیست بررسی جداول" },
        new() { Controller = "System", Action = "AuditsCheckDetailIndex", ControllerLabel = "سیستم", ActionLabel = "جزيیات لیست بررسی جداول" },
        new() { Controller = "System", Action = "AuditsCheck", ControllerLabel = "سیستم", ActionLabel = "چک کردن جداول" },

        new() { Controller = "UserGroup", Action = "Index", ControllerLabel = "گروه کابر", ActionLabel = "نمایش" },
        new() { Controller = "UserGroup", Action = "Create", ControllerLabel = "گروه کابر", ActionLabel = "ایجاد" },
        new() { Controller = "UserGroup", Action = "Update", ControllerLabel = "گروه کابر", ActionLabel = "ویرایش" },
        new() { Controller = "UserGroup", Action = "Delete", ControllerLabel = "گروه کابر", ActionLabel = "حذف" },

        new() { Controller = "ImportedRecord", Action = "Index", ControllerLabel = "تطبیق فایل", ActionLabel = "لیست" },
        new() { Controller = "ImportedRecord", Action = "Adapt", ControllerLabel = "تطبیق فایل", ActionLabel = "تطبیق دکورد" },
        new() { Controller = "ImportedRecord", Action = "Adapt", ControllerLabel = "تطبیق فایل", ActionLabel = "رد دکورد" },
        
        
        /////////////////////////////////
        
        new() { Controller = "ProjectKind", Action = "Index", ControllerLabel = "نوع پروژه", ActionLabel = "نمایش" },
        new() { Controller = "ProjectKind", Action = "Create", ControllerLabel = "نوع پروژه", ActionLabel = "ایجاد" },
        new() { Controller = "ProjectKind", Action = "Update", ControllerLabel = "نوع پروژه", ActionLabel = "ویرایش" },
        new() { Controller = "ProjectKind", Action = "Delete", ControllerLabel = "نوع پروژه", ActionLabel = "حذف" },
        
        new() { Controller = "FormType", Action = "Index", ControllerLabel = "نوع فرم", ActionLabel = "نمایش" },
        new() { Controller = "FormType", Action = "Create", ControllerLabel = "نوع فرم", ActionLabel = "ایجاد" },
        new() { Controller = "FormType", Action = "Update", ControllerLabel = "نوع فرم", ActionLabel = "ویرایش" },
        new() { Controller = "FormType", Action = "Delete", ControllerLabel = "نوع فرم", ActionLabel = "حذف" },
        
        new() { Controller = "Form", Action = "Index", ControllerLabel = " فرم", ActionLabel = "نمایش" },
        new() { Controller = "Form", Action = "Create", ControllerLabel = "فرم", ActionLabel = "ایجاد" },
        new() { Controller = "Form", Action = "Update", ControllerLabel = "فرم", ActionLabel = "ویرایش" },
        new() { Controller = "Form", Action = "Delete", ControllerLabel = "فرم", ActionLabel = "حذف" },
        
        new() { Controller = "FollowShapeKind", Action = "Index", ControllerLabel = "نوع فرآیند", ActionLabel = "نمایش" },
        new() { Controller = "FollowShapeKind", Action = "Create", ControllerLabel = "نوع فرآیند", ActionLabel = "ایجاد" },
        new() { Controller = "FollowShapeKind", Action = "Update", ControllerLabel = "نوع فرآیند", ActionLabel = "ویرایش" },
        new() { Controller = "FollowShapeKind", Action = "Delete", ControllerLabel = "نوع فرآیند", ActionLabel = "حذف" },
        
        new() { Controller = "FollowShape", Action = "Index", ControllerLabel = "فرآیند", ActionLabel = "نمایش" },
        new() { Controller = "FollowShape", Action = "Create", ControllerLabel = "فرآیند", ActionLabel = "ایجاد" },
        new() { Controller = "FollowShape", Action = "Update", ControllerLabel = "فرآیند", ActionLabel = "ویرایش" },
        new() { Controller = "FollowShape", Action = "Delete", ControllerLabel = "فرآیند", ActionLabel = "حذف" },
        
    ];

    public static bool AreValidPermissions(IEnumerable<string> permissions)
    {
        var validPermissionStrings = All
            .Select(p => $"{p.Controller}.{p.Action}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return permissions.All(p => validPermissionStrings.Contains(p));
    }
}

public class Permission
{
    public required string Controller { get; set; }
    public required string ControllerLabel { get; set; }
    public required string Action { get; set; }
    public required string ActionLabel { get; set; }
}