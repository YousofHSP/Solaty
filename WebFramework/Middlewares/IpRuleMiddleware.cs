using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Data;
using Data.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace WebFramework.Middlewares
{
    public class IpRuleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRepository<IpRule> _ipRuleRepository;
        public IpRuleMiddleware(RequestDelegate next, IRepository<IpRule> ipRuleRepository)
        {
            _next = next;
            _ipRuleRepository = ipRuleRepository;
        }
        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            if (remoteIp == null)
            {
                await _next(context);
                return;
            }

            var ipRules = await _ipRuleRepository.TableNoTracking
                .Include(x => x.IpAccessTypes)
                .Where(i => i.IpAccessTypes.Any(a => a.AccessType == AccessType.BlackList))
                .ToListAsync();

            foreach (var rule in ipRules)
            {
                if (IsIpBlocked(remoteIp, rule))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Access Denied: Your IP is blocked.");
                    return;
                }
            }

            await _next(context);
        }

        private bool IsIpBlocked(IPAddress remoteIp, IpRule rule)
        {
            if (!string.IsNullOrEmpty(rule.Cidr))
            {
                if (IsInCidr(remoteIp, rule.Cidr)) return true;
            }

            if (!string.IsNullOrEmpty(rule.EndIp))
            {
                if (IsInRange(remoteIp, IPAddress.Parse(rule.Ip), IPAddress.Parse(rule.EndIp))) return true;
            }

            if (remoteIp.Equals(IPAddress.Parse(rule.Ip))) return true;

            return false;
        }

        private bool IsInCidr(IPAddress ip, string cidr)
        {
            var parts = cidr.Split('/');
            var baseIp = IPAddress.Parse(parts[0]);
            var prefix = int.Parse(parts[1]);

            var ipBytes = ip.GetAddressBytes();
            var baseBytes = baseIp.GetAddressBytes();

            int byteCount = prefix / 8;
            int bitRemainder = prefix % 8;

            for (int i = 0; i < byteCount; i++)
            {
                if (ipBytes[i] != baseBytes[i]) return false;
            }

            if (bitRemainder != 0)
            {
                int mask = (byte)(~(0xFF >> bitRemainder));
                if ((ipBytes[byteCount] & mask) != (baseBytes[byteCount] & mask)) return false;
            }

            return true;
        }

        private bool IsInRange(IPAddress ip, IPAddress startIp, IPAddress endIp)
        {
            var ipBytes = ip.GetAddressBytes();
            var startBytes = startIp.GetAddressBytes();
            var endBytes = endIp.GetAddressBytes();

            bool lowerBound = true, upperBound = true;

            for (int i = 0; i < ipBytes.Length && (lowerBound || upperBound); i++)
            {
                if (lowerBound && ipBytes[i] < startBytes[i]) return false;
                if (upperBound && ipBytes[i] > endBytes[i]) return false;

                lowerBound &= (ipBytes[i] == startBytes[i]);
                upperBound &= (ipBytes[i] == endBytes[i]);
            }

            return true;
        }
    }
}
