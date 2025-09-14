using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common;
using Data.Contracts;
using Domain.Auth;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared;
using Shared.DTOs;

namespace Service.Auth
{
    public class JwtService(
        IOptionsSnapshot<SiteSettings> settings,
        IRepository<ApiToken> apiTokenRepository,
        IRepository<Role> roleRepository,
        IHttpContextAccessor httpContextAccessor,
        SignInManager<User> signInManager,
        RoleManager<Role> roleManager,
        ISettingService settingService)
        : IJwtService, IScopedDependency
    {
        private readonly SiteSettings _siteSettings = settings.Value;

        public async Task<AccessToken> GenerateAsync(User user, CancellationToken ct)
        {
            var code = Guid.NewGuid().ToString("N");
            var apiToken = new ApiToken
            {
                Code = code,
                UserId = user.Id,
                Enable = true,
                LastUsedDate = DateTimeOffset.Now,
                Ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "",
                UserAgent = httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? ""
            };
            await apiTokenRepository.AddAsync(apiToken, ct);
            var simultaneousTokenActivation =
                await settingService.GetValueAsync<int>(SettingKey.SimultaneousTokenActivation);
            var apiTokens = await apiTokenRepository.Table
                .Where(i => i.Enable == true && i.Id != apiToken.Id)
                .OrderByDescending(i => i.LastUsedDate)
                .Skip(simultaneousTokenActivation - 1)
                .ToListAsync(ct);
            apiTokens = apiTokens.Select(i =>
            {
                i.Enable = false;
                return i;
            }).ToList();
            await apiTokenRepository.UpdateRangeAsync(apiTokens, ct);
            var claims = await _getClaimsAsync(user, code);
            var secretKey = Encoding.UTF8.GetBytes(JwtSettings.SecretKey);
            var signingCredentials =
                new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256);

            var encryptionKey = Encoding.UTF8.GetBytes(JwtSettings.EncryptKey);
            var encryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(encryptionKey),
                SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512);

            var expirationMin = await settingService.GetValueAsync<int>(SettingKey.JWTExpirationMinutes);
            var descriptor = new SecurityTokenDescriptor()
            {
                Issuer = JwtSettings.Issuer,
                Audience = JwtSettings.Audience,
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(expirationMin),
                SigningCredentials = signingCredentials,
                Subject = new ClaimsIdentity(claims),
                EncryptingCredentials = encryptingCredentials,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateJwtSecurityToken(descriptor);
            

            return new AccessToken(securityToken, code);
        }

        private async Task<IEnumerable<Claim>> _getClaimsAsync(User user, string code)
        {
            var result = await signInManager.ClaimsFactory.CreateAsync(user);

            var permissions = new List<string>();
            if (user.Id == 1)
            {
                permissions = Permissions.All.Select(i => $"{i.Controller}.{i.Action}").ToList();
            }
            else
            {
                var roles = await roleRepository.TableNoTracking
                    .Where(g => g.Users.Any(u => u.Id == user.Id))
                    .ToListAsync();
                foreach (var role in roles)
                {
                    var claims = await roleManager.GetClaimsAsync(role);
                    permissions.AddRange(claims.Select(i => i.Value));
                }
            }

            // add custom claims
            var list = new List<Claim>(result.Claims)
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new("TokenCode", code),
            };
            foreach (var per in permissions)
            {
                list.Add(new Claim("permission", per));
            }

            return list;
        }
    }
}