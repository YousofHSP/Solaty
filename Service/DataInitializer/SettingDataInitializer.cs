using Common.Utilities;
using Data.Contracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Service.License;
using Shared.DTOs;

namespace Service.DataInitializer
{
    public class SettingDataInitializer(IRepository<Setting> repository) : IDataInitializer
    {
        public async Task InitializerData()
        {
            var items = typeof(SettingKey);
            var list = Enum.GetValues(typeof(SettingKey))
            .Cast<SettingKey>()
            .Select(key => new Setting
            {
                Id = (int)key,
                Title = key.ToString(),
                Value = GetValue(key),
                Description = key.ToDisplay(),
                IsSecurity = IsSecuritySetting(key)
            })
            .ToList();

            var settings = await repository.TableNoTracking.ToListAsync();
            var addList = list.Where(item => settings.All(s => s.Id != item.Id)).ToList();
            if (addList.Count != 0)
                await repository.AddRangeAsync(addList, default);
        }
        private static int IsSecuritySetting(SettingKey key)
        {
            var securitySettings = new[]
            {
                SettingKey.MaxLoginFail,
                SettingKey.PasswordRequireDigit,
                SettingKey.PasswordRequiredLength,
                SettingKey.PasswordRequireNonAlphanumeric,
                SettingKey.PasswordRequireUppercase,
                SettingKey.PasswordRequireLowercase,
                SettingKey.SimultaneousTokenActivation,
                SettingKey.MaxAccountDisableHour,
                SettingKey.JWTExpirationMinutes,
                SettingKey.PasswordHistoryCount,
            };

            return securitySettings.Contains(key) ? 1 : 0;
        }
        private static string GetValue(SettingKey key)
        {
            switch (key)
            {
                case (SettingKey.AppMode):
                    return "1";
                case (SettingKey.MaxLoginFail):
                    return "3";
                case (SettingKey.PasswordRequireDigit):
                    return "1";
                case (SettingKey.SimultaneousTokenActivation):
                    return "3";
                case (SettingKey.PasswordHistoryCount):
                    return "3";
                case (SettingKey.JWTExpirationMinutes):
                    return "20";
                case (SettingKey.MaxAccountDisableHour):
                    return "1";
                case (SettingKey.LoginType):
                    return "0";
                case (SettingKey.LoginWithPhoneNumber):
                    return "1";
                case (SettingKey.ShowSubSystem):
                    return "0";
                case (SettingKey.AllowedFileTypes):
                    return ".jpg,.png";
                case (SettingKey.AppName):
                    return "سامانه سرآمد";
                case (SettingKey.AppLogo):
                    return "https://api.radtender.local/logo.png";
                case (SettingKey.License):
                    return LicenseService.GenerateLicense();


            }
            return "";
        }
    }
}
