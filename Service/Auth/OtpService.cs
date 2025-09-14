using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Domain.Auth
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;

        public OtpService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<string> GenerateOtpAsync(string key)
        {
            //var otp = new Random().Next(1000, 9999).ToString();
            var otp = "1111";
            _cache.Set(key, otp, TimeSpan.FromMinutes(2));


            return Task.FromResult(otp);
        }

        public bool VerifyOtp(string key, string? otp)
        {
            if (otp is null)
                return false;
            if (_cache.TryGetValue(key, out string? cachedOtp))
            {
                return cachedOtp == otp;
            }
            return false;
        }
    }
}
