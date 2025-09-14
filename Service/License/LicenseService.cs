using System.Management;
using System.Net.NetworkInformation;
using System.Text.Json;
using Common;
using Common.Exceptions;
using Common.Utilities;
using Data.Contracts;
using Shared;
using Shared.DTOs;

namespace Service.License;

public class LicenseService : ILicenseService
{
    private readonly ISettingService _settingService;

    public LicenseService(ISettingService settingService)
    {
        _settingService = settingService;
    }

    private static string GetCpuId()
    {
        try
        {
            using var mc = new ManagementClass("win32_processor");
            foreach (var mo in mc.GetInstances())
            {
                return mo.Properties["processorID"].Value.ToString();
            }
        }
        catch
        {
            return string.Empty;
        }

        return string.Empty;
    }

    private static string GetDiskId()
    {
        try
        {
            using var mc = new ManagementClass("Win32_DiskDrive");
            foreach (var mo in mc.GetInstances())
            {
                return mo.Properties["SerialNumber"].Value?.ToString().Trim();
            }
        }
        catch
        {
            return string.Empty;
        }

        return string.Empty;
    }

    private static string GetMacAddress()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
            .Select(nic => nic.GetPhysicalAddress().ToString())
            .FirstOrDefault() ?? "";
    }

    private static string GetServerId()
    {
        return $"{GetCpuId()}-{GetDiskId()}-{GetMacAddress()}";
    }

    public static string GenerateLicense(int days = 0)
    {
        var data = new LicenseInfo
        {
            ServerId = GetServerId(),
            IssuedDate = DateTimeOffset.Now,
            ExpireDate = DateTimeOffset.Now.AddDays(days)
        };
        var json = JsonSerializer.Serialize(data);
        return SecurityHelpers.EncryptAes(json);
    }

    public async Task SaveLicenseAsync(string license, CancellationToken ct)
    {
        await _settingService.SetValueAsync(SettingKey.License, license, ct);
    }

    public async Task<bool> VerifyLicenseAsync()
    {
        try
        {
            var license = await _settingService.GetValueAsync<string>(SettingKey.License);
            if (string.IsNullOrEmpty(license))
                throw new AppException(ApiResultStatusCode.PaymentRequired, "لایسنس نا معتبر است");
            var json = SecurityHelpers.DecryptAes(license);
            var data = JsonSerializer.Deserialize<LicenseInfo>(json);
            if (data is null)
                throw new AppException(ApiResultStatusCode.PaymentRequired, "لایسنس نا معتبر است");

            if (data.ExpireDate < DateTimeOffset.Now)
                throw new AppException(ApiResultStatusCode.PaymentRequired, "لایسنس منقضی شده است");

            var serverId = data.ServerId.Split('-');
            if (serverId.Length != 3)
                throw new AppException(ApiResultStatusCode.PaymentRequired, "لایسنس نا معتبر است");

            if (serverId[0] != GetCpuId() && serverId[1] != GetDiskId() && serverId[2] != GetMacAddress())
                throw new AppException(ApiResultStatusCode.PaymentRequired, "لایسنس نا معتبر است");

            return true;
        }
        catch
        {
            throw new AppException(ApiResultStatusCode.PaymentRequired, "لایسنس نا معتبر است");
        }
    }
}