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

                new MenuDto { Title = "لاگ", Path = "/system/logList", Icon = "", Name = "Log.LogIndex", ParentName = "System", Type = "link"},
                new MenuDto { Title = "پیامک", Path = "/system/smsList", Icon = "", Name = "Sms.Index", ParentName = "System", Type = "link"},
                new MenuDto { Title = "اعلان", Path = "/system/adminNotificationsList", Icon = "", Name = "User.Notifications", ParentName = "System", Type = "link"},
                new MenuDto { Title = "تنظیمات", Path = "/Setting/index", Icon = "", Name = "Setting.Index", ParentName = "System", Type = "link"},

                new MenuDto { Title = "لیست نقش", Path = "/role/index", Icon = "", Name = "Role.Index", ParentName = "MangeUsers", Type = "link"},
                new MenuDto { Title = "ایجاد نقش", Path = "/role/create", Icon = "", Name = "Role.Create", ParentName = "MangeUsers", Type = "link"},
                new MenuDto { Title = "لیست گروه کابران", Path = "/userGroup/index", Icon = "", Name = "UserGroup.Index", ParentName = "MangeUsers", Type = "link"},
                new MenuDto { Title = "ایجاد گروه کابران", Path = "/userGroup/create", Icon = "", Name = "UserGroup.Create", ParentName = "MangeUsers", Type = "link"},
                new MenuDto { Title = "لیست کاربر", Path = "/users/index", Icon = "", Name = "User.Index", ParentName = "MangeUsers", Type = "link"},
                new MenuDto { Title = "ایجاد کاربر", Path = "/users/create", Icon = "", Name = "User.Create", ParentName = "MangeUsers", Type = "link"},
                new MenuDto { Title = "لیست اعلان", Path = "/system/notificationsList", Icon = "", Name = "User.UserNotifications", ParentName = "MangeUsers", Type = "link"},
                new MenuDto { Title = "لیست نشست", Path = "/system/tokensList", Icon = "", Name = "User.GetTokens", ParentName = "MangeUsers", Type = "link"},
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
