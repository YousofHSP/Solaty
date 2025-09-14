namespace Shared.DTOs
{
    public class MenuDto
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public string Iconf { get; set; }
        public string Type { get; set; }
        public string? ParentName { get; set; }
        public List<MenuDto>? Children { get; set; }

    }

    public class MenusDto
    {
        public MenusDto()
        {
            Items = new List<MenuDto>()
            {
                new MenuDto { Title = "داشبورد", Path = "/", Icon = "stroke-home", Iconf ="fill-learning", Type = "link", Name = "Dashboard"},
                new MenuDto { Title = "مدیریت معدن", Path = "", Icon = "stroke-learning", Iconf ="fill-learning", Type = "sub", Name = "Mine"},
                new MenuDto { Title = "اطلاعات پایه", Path = "", Icon = "stroke-animation", Iconf = "fill-animation", Type = "sub", Name = "BasicInfo"},
                new MenuDto { Title = "مدیریت کاربران", Path = "", Icon = "stroke-learning", Iconf ="fill-learning", Type = "sub", Name = "MangeUsers"},
                new MenuDto { Title = "تنظیمات امنیتی", Path = "", Icon = "stroke-file", Iconf = "fill-file", Type = "sub", Name = "System"},

                new MenuDto { Title = "زیرسیستم", Path = "/subsystem/list", Icon = "", Name = "SubSystem.Index", ParentName = "System", Type = "link"},
                new MenuDto { Title = "پشتیبان گیری", Path = "/system/listBackUp", Icon = "", Name = "System.BackupsList", ParentName = "System", Type = "link"},
                new MenuDto { Title = "لاگ", Path = "/system/logList", Icon = "", Name = "Log.LogIndex", ParentName = "System", Type = "link"},
                new MenuDto { Title = "ممیزی", Path = "/system/auditList", Icon = "", Name = "Log.AuditIndex", ParentName = "System", Type = "link"},
                new MenuDto { Title = "پیامک", Path = "/system/smsList", Icon = "", Name = "Sms.Index", ParentName = "System", Type = "link"},
                new MenuDto { Title = "اعلان", Path = "/system/adminNotificationsList", Icon = "", Name = "User.Notifications", ParentName = "System", Type = "link"},
                new MenuDto { Title = "بررسی جداول", Path = "/system/auditsCheckIndex", Icon = "", Name = "System.AuditsCheckIndex", ParentName = "System", Type = "link"},
                new MenuDto { Title = "تنظیمات", Path = "/Setting/index", Icon = "", Name = "Setting.Index", ParentName = "System", Type = "link"},

                new MenuDto { Title = "لیست نقش", Path = "/role/index", Icon = "", Name = "Role.Index", ParentName = "MangeUsers", Type = "link"},
                new MenuDto { Title = "ایجاد نقش", Path = "/role/create", Icon = "", Name = "Role.Create", ParentName = "MangeUsers", Type = "link"},
                new MenuDto { Title = "لیست گروه کابران", Path = "/userGroup/index", Icon = "", Name = "UserGroup.Index", ParentName = "MangeUsers", Type = "link"},
                new MenuDto { Title = "ایجاد گروه کابران", Path = "/userGroup/create", Icon = "", Name = "UserGroup.Create", ParentName = "MangeUsers", Type = "link"},
                new MenuDto { Title = "لیست کاربر", Path = "/users/index", Icon = "", Name = "User.Index", ParentName = "MangeUsers", Type = "link"},
                new MenuDto { Title = "ایجاد کاربر", Path = "/users/create", Icon = "", Name = "User.Create", ParentName = "MangeUsers", Type = "link"},
                new MenuDto { Title = "لیست اعلان", Path = "/system/notificationsList", Icon = "", Name = "User.UserNotifications", ParentName = "MangeUsers", Type = "link"},
                new MenuDto { Title = "لیست نشست", Path = "/system/tokensList", Icon = "", Name = "User.GetTokens", ParentName = "MangeUsers", Type = "link"},

                new MenuDto { Title = "لیست گروه معادن", Path = "/groupMine/index", Icon = "", Name = "GroupMine.Index", ParentName = "BasicInfo", Type = "link"},
                new MenuDto { Title = "ایجاد گروه معادن", Path = "/groupMine/create", Icon = "", Name = "GroupMine.Create", ParentName = "BasicInfo", Type = "link"},
                new MenuDto { Title = "لیست انواع درآمد", Path = "/paymentKind/index", Icon = "", Name = "PaymentKind.Index", ParentName = "BasicInfo", Type = "link"},
                new MenuDto { Title = "ایجاد نوع درآمد", Path = "/paymentKind/create", Icon = "", Name = "PaymentKind.Create", ParentName = "BasicInfo", Type = "link"},
                new MenuDto { Title = "لیست مهلت پرداخت", Path = "/respite/index", Icon = "", Name = "Respite.Index", ParentName = "BasicInfo", Type = "link"},
                new MenuDto { Title = "ایجاد مهلت پرداخت", Path = "/respite/create", Icon = "", Name = "Respite.Create", ParentName = "BasicInfo", Type = "link"},
                new MenuDto { Title = "لیست جریمه", Path = "/penaltyDefination/index", Icon = "", Name = "PenaltyDefination.Index", ParentName = "BasicInfo", Type = "link"},
                new MenuDto { Title = "ایجاد جریمه", Path = "/penaltyDefination/create", Icon = "", Name = "PenaltyDefination.Create", ParentName = "BasicInfo", Type = "link"},
                new MenuDto { Title = "لیست دسته بندی معادن", Path = "/mineKind/index", Icon = "", Name = "MineKind.Index", ParentName = "BasicInfo", Type = "link"},
                new MenuDto { Title = "ایجاد دسته بندی معادن", Path = "/mineKind/create", Icon = "", Name = "MineKind.Create", ParentName = "BasicInfo", Type = "link"},
                new MenuDto { Title = "لیست انواع پرداخت", Path = "/payoffCategory/index", Icon = "", Name = "PayoffCategory.Index", ParentName = "BasicInfo", Type = "link"},
                new MenuDto { Title = "ایجاد انواع پرداخت", Path = "/payoffCategory/create", Icon = "", Name = "PayoffCategory.Create", ParentName = "BasicInfo", Type = "link"},
                new MenuDto { Title = "نوع مبلغ ابلاغیه", Path = "/minePaymentItemType/index", Icon = "", Name = "MinePaymentItemType.Index", ParentName = "BasicInfo", Type = "link"},
                new MenuDto { Title = "تطبیق فایل", Path = "/matchFilePage/index", Icon = "", Name = "ImportedRecord.Index", ParentName = "BasicInfo", Type = "link"},

                new MenuDto { Title = "لیست معدن", Path = "/mine/index", Icon = "", Name = "Mine.Index", ParentName = "Mine", Type = "link"},
                new MenuDto { Title = "ایجاد معدن", Path = "/mine/create", Icon = "", Name = "Mine.Create", ParentName = "Mine", Type = "link"},
                new MenuDto { Title = "گزارش مالی معادن", Path = "/mineDeptReport/index", Icon = "", Name = "Mine.MineDeptReport", ParentName = "Mine", Type = "link"},
                new MenuDto { Title = "گزارش تجمعی مالی معادن", Path = "/mineCumulative/index", Icon = "", Name = "Mine.MineDeptCumulativeReport", ParentName = "Mine", Type = "link"},
                new MenuDto { Title = "گزارش  ابلاغیه", Path = "/minePaymentIssue/index", Icon = "", Name = "MinePaymentIssue.Index", ParentName = "Mine", Type = "link"},
                new MenuDto { Title = "ایجاد ابلاغیه", Path = "/minePaymentIssue/create", Icon = "", Name = "MinePaymentIssue.Create", ParentName = "Mine", Type = "link"},
                new MenuDto { Title = "گزارش بدهی های قبلی", Path = "/before/index", Icon = "", Name = "Before.Index", ParentName = "Mine", Type = "link"},
                new MenuDto { Title = "ایجاد بدهی  قبلی", Path = "/before/create", Icon = "", Name = "Before.Create", ParentName = "Mine", Type = "link"},
                new MenuDto { Title = "گزارش بدهی های ثبت نشده", Path = "/before/notIncludedBefore", Icon = "", Name = "Before.NotIncludedBefore", ParentName = "Mine", Type = "link"},
                new MenuDto { Title = "گزارش پرداخت ها", Path = "/payoff/index", Icon = "", Name = "Payoff.Index", ParentName = "Mine", Type = "link"},
                new MenuDto { Title = "بخشش جریمه", Path = "/bounty/index", Icon = "", Name = "MinePaymentIssue.PenaltyBountyChangeHistories", ParentName = "Mine", Type = "link"},
                new MenuDto { Title = "لیست چک ها", Path = "/check/index", Icon = "", Name = "Payoff.Checks", ParentName = "Mine", Type = "link"},
            };
        }
        public List<MenuDto> Items { get; set; }
        public List<MenuDto> BuildMenuTree(List<MenuDto> flatMenu)
        {
            var lookup = flatMenu.ToDictionary(m => m.Name);
            var rootMenus = new List<MenuDto>();

            foreach (var menu in flatMenu)
            {
                if (!string.IsNullOrWhiteSpace(menu.ParentName) && lookup.TryGetValue(menu.ParentName, out var parent))
                {
                    if (parent.Children == null)
                        parent.Children = new List<MenuDto>();

                    parent.Children.Add(menu);
                }
                else
                {
                    rootMenus.Add(menu);
                }
            }

            return rootMenus;
        }
    }
}
