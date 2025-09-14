using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Common;
using Common.Exceptions;
using Data.Contracts;
using Domain.Auth;
using Domain.Entities;
using Domain.Message;
using Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Auth;
using Service.Message;
using Service.Model.Contracts;
using Shared;
using Shared.DTOs;
using TradeBot.Models;
using WebFramework.Api;

namespace Api.Controllers.v1
{
    [ApiVersion("1")]

    [Display(Name = "حساب کاربری")]
    public class AuthController(
        IJwtService jwtService,
        UserManager<User> userManager,
        IUserRepository repository,
        IPasswordHistoryService passwordHistoryService,
        IHashEntityValidator hashEntityValidator,
        IUploadedFileService uploadedFileService,
        ISettingService settingService,
        IOtpService _otpService,
        ILogger<UserController> logger,
        IEmailService emailService,
        IMessageService messageService) : BaseController
    {
        [HttpPost("Token")]
        [AllowAnonymous]
        [Display(Name = "لاگین سوواگر")]
        public async Task<ActionResult> Token([FromForm] TokenRequest tokenRequest, CancellationToken cancellationToken)
        {
            var maxLoginFails = await settingService.GetValueAsync<int>(SettingKey.MaxLoginFail);
            var maxAccountDisableHours = await settingService.GetValueAsync<int>(SettingKey.MaxAccountDisableHour);
            var user = await repository.Table.FirstOrDefaultAsync(i => i.UserName == tokenRequest.username
                || i.PhoneNumber == tokenRequest.username, cancellationToken);

            if (user == null)
            {
                throw new BadRequestException("نام کاربری یا رمز عبور اشتباه است.");
            }

            await hashEntityValidator.IsValidAsync(user, user.Id, cancellationToken);
            if (user.LockoutEnd > DateTime.UtcNow || user.Status == UserStatus.Disable)
            {
                throw new BadRequestException($"کاربر ({user.UserName}) غیرفعال است");
            }
            if (user.AccessFailedCount >= maxLoginFails && user.LockoutEnabled)
            {
                user.LockoutEnd = DateTime.UtcNow.AddHours(maxAccountDisableHours);
                await repository.UpdateAsync(user, cancellationToken);
                throw new BadRequestException($"کاربر ({user.UserName}) غیرفعال است");
            }
            var isPasswordValid = await userManager.CheckPasswordAsync(user, tokenRequest.password);
            if (!isPasswordValid)
            {
                user.AccessFailedCount++;
                await repository.UpdateAsync(user, cancellationToken);
                throw new BadRequestException("نام کاربری یا رمز عبور اشتباه است.");
            }
            else if (user.AccessFailedCount != 0)
            {
                user.AccessFailedCount = 0;
                await repository.UpdateAsync(user, cancellationToken);
            }


            var jwt = await jwtService.GenerateAsync(user, cancellationToken);
            return new JsonResult(jwt);
        }
        private async Task<User> CheckUserNameAndPassword(string userName, string? password, string loginType,bool checkOtp , string? otpCode, CancellationToken ct)
        {
            var maxLoginFails = await settingService.GetValueAsync<int>(SettingKey.MaxLoginFail);
            var loginWithPhoneNumber = await settingService.GetValueAsync<string>(SettingKey.LoginWithPhoneNumber);
            var maxAccountDisableHours = await settingService.GetValueAsync<int>(SettingKey.MaxAccountDisableHour);

            var query = repository.Table
                .Include(u => u.Info)
                .AsQueryable();
            if (loginWithPhoneNumber == "1")
            {
                query = query.Where(i => i.UserName == userName || i.PhoneNumber == userName);
            }
            else
            {
                query = query.Where(i => i.UserName == userName);
            }
            var user = await query.FirstOrDefaultAsync(ct);

            if (user == null)
            {
                throw new BadRequestException("نام کاربری یا رمز عبور اشتباه است.");
            }
            await hashEntityValidator.IsValidAsync(user, user.Id, ct);
            if (user.LockoutEnd > DateTime.UtcNow || user.Status == UserStatus.Disable)
            {
                throw new BadRequestException($"کاربر ({user.UserName}) غیرفعال است");
            }
            if (user.AccessFailedCount >= maxLoginFails && user.LockoutEnabled)
            {
                user.LockoutEnd = DateTime.UtcNow.AddHours(maxAccountDisableHours);
                await repository.UpdateAsync(user, ct);
                throw new BadRequestException($"کاربر ({user.UserName}) غیرفعال است");
            }
            var isPasswordValid = true;
            var isValidOtpCode = true;
            if(loginType is "0" or "1")
            {
                if (password is null)
                    throw new BadRequestException("رمز را وارد کنید");
                isPasswordValid = await userManager.CheckPasswordAsync(user, password);

            }
            if (loginType is "1" or "2" && checkOtp)
            {
                if (otpCode is null)
                    throw new BadRequestException("کد یکبار مصرف الزامی است");
                isValidOtpCode = _otpService.VerifyOtp(user.PhoneNumber, otpCode);

            }
            if (!isPasswordValid || !isValidOtpCode)
            {
                user.AccessFailedCount++;
                await repository.UpdateAsync(user, ct);
                throw new BadRequestException("نام کاربری یا رمز عبور اشتباه است.");
            }
            else if (user.AccessFailedCount != 0)
            {
                user.AccessFailedCount = 0;
                await repository.UpdateAsync(user, ct);
            }
            return user;

        }
        [HttpPost("[action]")]
        [AllowAnonymous]
        [Display(Name = "لاگین")]
        public async Task<ActionResult<LoginResDto>> Login(LoginDto loginRequest, CancellationToken ct)
        {

            var loginType = await settingService.GetValueAsync<string>(SettingKey.LoginType);
            // 1 نام کاربری و رمز عبور و کد یکبار مصرف
            // 2 شماره موبایل و کد یکبار مصرف
            var checkOtp = loginType is "1" or "2"; 
            var user = await CheckUserNameAndPassword(loginRequest.UserName, loginRequest.Password, loginType, checkOtp, loginRequest.OtpCode, ct);
            var jwt = await jwtService.GenerateAsync(user, ct);

            var profileImage = await uploadedFileService.GetFilePath(nameof(UserInfo), user.Info?.Id ?? 0, UploadedFileType.UserProfile, ct);
            var result = new LoginResDto()
            {
                Token = jwt.access_token,
                AccessCode = jwt.access_code,
                UserFirstName =  user.Info?.FirstName ?? "",
                UserLastName =  user.Info?.LastName ?? "",
                ProfileImage = profileImage,
                UserName = user.UserName
            };
            return Ok(result);
        }


        [HttpPost("SendOtp")]
        [AllowAnonymous]
        [Display(Name = "ارسال کد یکبارمصرف")]
        public async Task<ActionResult> SendOtp(SendOtpRequest dto, CancellationToken ct)
        {

            var loginType = await settingService.GetValueAsync<string>(SettingKey.LoginType);
            if (loginType is "0" or "1")
                if (dto.Password is null)
                    throw new BadRequestException("رمز را وارد کنید");
            var user = await CheckUserNameAndPassword(dto.UserName, dto.Password, loginType, false, null, ct);
            var code = await _otpService.GenerateOtpAsync(user.PhoneNumber);
            //var result = await messageService.SendMessageAsync(dto.PhoneNumber, $"code: {code}");
            //await emailService.SendEmailAsync("moinrayat9544@gmail.com", "verify code", $"code: {code}");
            logger.LogInformation($"Send OTP Code for {user.PhoneNumber}");
            return Ok(new { Message = "کد ارسال شد" });

        }

        [HttpPost("VerifyOtp")]
        [AllowAnonymous]
        [Display(Name = "تایید کد یکبارمصرف")]
        public async Task<ActionResult> VerifyOtp(VerifyOtpRequest dto, CancellationToken ct)
        {
            var user = await repository.TableNoTracking
                .FirstOrDefaultAsync(i => i.UserName == dto.UserName || i.PhoneNumber == dto.UserName);
            if (user is null)
                throw new NotFoundException($"کاربر {dto.UserName} پیدا نشد");

            await hashEntityValidator.IsValidAsync(user, user.Id, ct);
            if (!_otpService.VerifyOtp(user.PhoneNumber, dto.OtpCode))
            {
                throw new AppException(ApiResultStatusCode.UnAuthorized, "کد صحیح نیست");
            }
            logger.LogInformation($"User logined with id: {user.Id} and phoneNumber: {user.PhoneNumber}");

            var jwt = await jwtService.GenerateAsync(user, ct);
            return new JsonResult(jwt);

        }
        [HttpPost("[action]")]
        [AllowAnonymous]
        [Display(Name = "ارسال کد یکبار مصرف برای فراموشی رمز")]
        public async Task<IActionResult> ResetPasswordSendOtp([FromBody]ResetPasswordSendOtpDto dto, CancellationToken ct)
        {
            var user = await repository.TableNoTracking.FirstOrDefaultAsync(i => i.UserName == dto.UserName);
            if(user is null)
                return Ok(new { message = "OTP sent." });
            await hashEntityValidator.IsValidAsync(user, user.Id, ct);
            var code = await _otpService.GenerateOtpAsync(user.PhoneNumber);
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            //var result = await messageService.SendMessageAsync(dto.PhoneNumber, $"code: {code}");
            //await emailService.SendEmailAsync("moinrayat9544@gmail.com", "verify code", $"code: {code}");
            logger.LogInformation($"Send OTP Code for {user.PhoneNumber} for reset password");
            return Ok(new { ResetPasswordToken = token});
        }
        [HttpPost("[action]")]
        [AllowAnonymous]
        [Display(Name = "فراموشی رمز")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto, CancellationToken ct)
        {
            var user = await repository.TableNoTracking
                .Include(i => i.Info)
                .FirstOrDefaultAsync(i => i.UserName == dto.UserName || i.PhoneNumber == dto.UserName);
            if (user is null)
                throw new NotFoundException($"کاربر {dto.UserName} پیدا نشد");

            await hashEntityValidator.IsValidAsync(user, user.Id, ct);
            if (!_otpService.VerifyOtp(user.PhoneNumber, dto.OtpCode))
            {
                throw new AppException(ApiResultStatusCode.UnAuthorized, "کد صحیح نیست");
            }


            var passwordHistory = await passwordHistoryService.CheckPasswordHistoryAsync(dto.Password, user, user.Id, ct);
            var result = await userManager.ResetPasswordAsync(user, dto.ResetPasswordToken, dto.Password);
            if(!result.Succeeded)
                throw new BadRequestException("توکن تغییر رمز اشتباه است");
            passwordHistory.PasswordHash = user.PasswordHash!;
            await passwordHistoryService.AddAsync(passwordHistory, ct);


            return Ok(new { Message = "رمز با موفقیت تغییر کرد"});
        }

    }
}
