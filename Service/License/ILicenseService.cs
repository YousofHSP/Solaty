namespace Service.License;

public interface ILicenseService
{
    Task SaveLicenseAsync(string license, CancellationToken ct);
    Task<bool> VerifyLicenseAsync();
}