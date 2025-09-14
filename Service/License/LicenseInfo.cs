namespace Service.License;

public class LicenseInfo
{

    public string ServerId { get; set; }
    public DateTimeOffset IssuedDate { get; set; }
    public DateTimeOffset ExpireDate { get; set; }
}