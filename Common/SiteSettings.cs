using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class SiteSettings
    {
        public string ElmahPath { get; set; } = null!;
        public string Url { get; set; } = null!;
        public IdentitySettings IdentitySettings { get; set; } = null!;
    }

    public class IdentitySettings
    {
        public bool PasswordRequireDigit { get; set; }
        public int PasswordRequiredLength { get; set; }
        public bool PasswordRequireNonAlphanumeric { get; set; }
        public bool PasswordRequireUppercase { get; set; }
        public bool PasswordRequireLowercase { get; set; }
        public bool RequireUniqueEmail { get; set; }
    }

    public static class JwtSettings
    {
        public static string SecretKey { get; set; } = "LongEnoughSecretKeyForAES256!123";
        public static string EncryptKey { get; set; } = "AnotherStrongEncryptKeyForAES!12";
        public static string Issuer { get; set; } = "http://localhost";
        public static string Audience { get; set; } = "http://localhost";
        public static int NotBeforeMinutes { get; set; }
        public static int ExpirationMinutes { get; set; }
    }
}