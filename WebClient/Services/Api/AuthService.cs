using System.IdentityModel.Tokens.Jwt;
using Microsoft.JSInterop;
using Shared.DTOs;
using WebClient.Services.Common;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;

namespace WebClient.Services.Api;

public class AuthService
{
    private readonly IBaseService _baseService;
    private readonly IJSRuntime _jsRuntime;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public string Token = "";

    public AuthService(IBaseService baseService, IJSRuntime jsRuntime, IHttpContextAccessor httpContextAccessor)
    {
        _baseService = baseService;
        _jsRuntime = jsRuntime;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// ورود کاربر و ذخیره توکن در localStorage
    /// </summary>
    public async Task<bool> LoginAsync(string userName, string password, string captchaToken)
    {
        var dto = new LoginDto
        {
            UserName = userName,
            Password = password,
        };

        var result = await _baseService.Post<LoginDto, LoginResDto>("v1/Auth/Login", dto);

        if (result is null)
            return false;

        Token = result.Token;
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "jwt_token", Token);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", nameof(result.UserFullName), result.UserFullName);
        return true;
    }

    public async Task SignInAsync(string jwtToken)
    {
        // Claims رو از JWT بخون
        var handler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(JwtSettings.SecretKey); // همون کلید SigningCredentials
        var encryptionKey = Encoding.UTF8.GetBytes(JwtSettings.EncryptKey); // همون کلید EncryptingCredentials

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = JwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = JwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            TokenDecryptionKey = new SymmetricSecurityKey(encryptionKey)
        };
        var token = handler.ValidateToken(jwtToken, validationParameters, out var validatedToken);

        var claims = token.Claims.ToList();

        // ClaimsIdentity بساز
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await _httpContextAccessor.HttpContext!.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
    }

    public async Task SignOutAsync()
    {
        await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// ارسال درخواست فراموشی رمز عبور (ارسال OTP)
    /// </summary>
    public async Task SendResetPasswordOtpAsync(string userName)
    {
        var dto = new ResetPasswordSendOtpDto
        {
            UserName = userName
        };

        // فرض: API مسیر v1/Auth/SendResetPasswordOtp دارد
        await _baseService.Post<ResetPasswordSendOtpDto, object>("v1/Auth/SendResetPasswordOtp", dto);
    }

    /// <summary>
    /// ریست کردن رمز عبور بعد از دریافت OTP
    /// </summary>
    public async Task ResetPasswordAsync(string userName, string otpCode, string newPassword, string resetToken)
    {
        var dto = new ResetPasswordDto
        {
            UserName = userName,
            OtpCode = otpCode,
            Password = newPassword,
            ResetPasswordToken = resetToken
        };

        // فرض: API مسیر v1/Auth/ResetPassword دارد
        await _baseService.Post<ResetPasswordDto, object>("v1/Auth/ResetPassword", dto);
    }
}
