using Asp.Versioning;
using AutoMapper;
using Common;
using Common.Exceptions;
using Data.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Auth;
using Domain.Model;
using WebFramework.Api;
using Common.Utilities;
using AutoMapper.QueryableExtensions;
using Service.Model.Contracts;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Service.Auth;
using Shared;
using Shared.DTOs;

namespace Api.Controllers.v1;

[ApiVersion("1")]
[Display(Name = "کاربر")]
public class UserController(
    IJwtService jwtService,
    UserManager<User> userManager,
    IMapper mapper,
    IUserRepository repository,
    IRepository<UserInfo> userInfoRepository,
    IRepository<PhoneNumberHistory> phoneNumberHisoryRepository,
    IRepository<EmailHistory> emailHisoryRepository,
    IRepository<ApiToken> apiTokenRepository,
    IRepository<Notification> notificationRepository,
    IHashEntityValidator hashEntityValidator,
    IUploadedFileService uploadedFileService,
    IPasswordHistoryService passwordHistoryService,
    IOtpService _otpService,
    IRepository<Role> roleRepository,
    RoleManager<Role> roleManager)
    : CrudController<UserDto, UserResDto, User>(repository, mapper, hashEntityValidator)
{
    private readonly IJwtService _jwtService = jwtService;
    private readonly IMapper _mapper = mapper;

    protected override IQueryable<User> setIncludes(IQueryable<User> query)
    {
        query = query
            .Include(i => i.Info)
            .Include(i => i.Roles);
        return query;
    }

    protected override IQueryable<User> setSearch(string? search, IQueryable<User> query)
    {
        if (search is null)
            return query;
        query = query
            .Where(i => i.UserName.Contains(search)
                        || (i.Info.FirstName + " " + i.Info.LastName).Contains(search)
                        || i.PhoneNumber.Contains(search)
                        || i.Email.Contains(search)
            )
            .Where(i => i.UserName != "admin");
        return query;
    }


    [Display(Name = "ایجاد")]
    [HttpPost("[action]")]
    public override async Task<ApiResult<UserResDto>> Create(UserDto dto, CancellationToken cancellationToken)
    {
        var userEnable = dto.Status;
        if (string.IsNullOrEmpty(dto.Password))
            throw new BadRequestException("رمز را وارد کنید");
        var isExists =
            await repository.TableNoTracking.AnyAsync(i => i.UserName == dto.UserName, cancellationToken);
        if (isExists)
            throw new BadRequestException("این نام کاربری قبلا استفاده شده");
        if (string.IsNullOrEmpty(dto.PhoneNumber))
        {
            dto.PhoneNumber = "";
            userEnable = UserStatus.Imperfect;
        }
        else
        {
            isExists =
                await repository.TableNoTracking.AnyAsync(i => i.PhoneNumber == dto.PhoneNumber, cancellationToken);
            if (isExists)
                throw new BadRequestException("این  موبایل قبلا استفاده شده");
        }

        if (string.IsNullOrEmpty(dto.Email))
        {
            dto.Email = "";
            userEnable = UserStatus.Imperfect;
        }
        else
        {
            var normalizeEmail = userManager.NormalizeEmail(dto.Email);
            isExists =
                await repository.TableNoTracking.AnyAsync(i => i.NormalizedEmail == normalizeEmail, cancellationToken);
            if (isExists)
                throw new BadRequestException("این  ایمیل قبلا استفاده شده");
        }

        isExists =
            await userInfoRepository.TableNoTracking.AnyAsync(i => i.NationalCode == dto.NationalCode,
                cancellationToken);
        if (isExists)
            throw new BadRequestException("این کدملی قبلا استفاده شده");

        if (dto.Status == UserStatus.Disable)
            userEnable = UserStatus.Disable;
        var model = dto.ToEntity(_mapper);


        DateTime? birthDate = null;
        if (!string.IsNullOrEmpty(dto.BirthDate))
        {
            try
            {
                var parts = dto.BirthDate.Split('/');
                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);
                int day = int.Parse(parts[2]);

                var pc = new System.Globalization.PersianCalendar();
                birthDate = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
            }
            catch
            {
                throw new BadRequestException("تاریخ تولد معتبر نیست");
            }
        }

        model.Status = userEnable;
        model.Info = new UserInfo()
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            BirthDate = birthDate,
            NationalCode = dto.NationalCode,
        };
        var result = await userManager.CreateAsync(model, dto.Password);
        if (!result.Succeeded)
            throw new AppException(
                result.Errors.Select(i => i.Description).Aggregate((curr, next) => $"{curr}, {next}"));

        await userManager.RemovePasswordAsync(model);
        await userManager.AddPasswordAsync(model, dto.Password);
        await passwordHistoryService.AddAsync(new PasswordHistory
        {
            UserId = model.Id,
            CreatorUserId = User.Identity?.GetUserId<int>() ?? 0,
            PasswordHash = model.PasswordHash!
        }, cancellationToken);
        var resultDto = UserResDto.FromEntity(model, _mapper);
        return resultDto;
    }

    [Display(Name = "ویرایش")]
    public override async Task<ApiResult<UserResDto>> Update(UserDto dto, CancellationToken cancellationToken)
    {
        var user = await repository.Table
            .Include(i => i.Info)
            .AsTracking()
            .FirstOrDefaultAsync(i => i.Id.Equals(dto.Id), cancellationToken);
        if (user is null)
            throw new NotFoundException("کاربر پیدا نشد");

        var userId = User.Identity!.GetUserId<int>();
        await hashEntityValidator.IsValidAsync(user, userId, cancellationToken);
        if (repository.TableNoTracking.Any(i => i.UserName == dto.UserName && i.Id != dto.Id))
            throw new BadRequestException("این نام کاربری قبلا استفاده شده");
        if (repository.TableNoTracking.Any(i => i.PhoneNumber == dto.PhoneNumber && i.Id != dto.Id))
            throw new BadRequestException("این موبایل قبلا استفاده شده");
        var normalizeEmail = userManager.NormalizeEmail(dto.Email);
        if (repository.TableNoTracking.Any(i => i.NormalizedEmail == normalizeEmail && i.Id != dto.Id))
            throw new BadRequestException("این ایمیل قبلا استفاده شده");
        if (userInfoRepository.TableNoTracking.Any(i => i.UserId != user.Id && i.NationalCode == dto.NationalCode))
            throw new BadRequestException("این کدملی قبلا استفاده شده");

        await repository.UpdateAsync(user, cancellationToken);

        var parts = dto.BirthDate.Split('/');
        int year = int.Parse(parts[0]);
        int month = int.Parse(parts[1]);
        int day = int.Parse(parts[2]);

        var pc = new System.Globalization.PersianCalendar();

        DateTime birthDate;
        try
        {
            birthDate = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
        }
        catch
        {
            throw new BadRequestException("تاریخ تولد معتبر نیست");
        }

        if (!string.IsNullOrEmpty(dto.Password))
        {
            await userManager.RemovePasswordAsync(user);
            await userManager.AddPasswordAsync(user, dto.Password);
            await passwordHistoryService.AddAsync(new PasswordHistory
            {
                UserId = user.Id,
                CreatorUserId = User.Identity?.GetUserId<int>() ?? 0,
                PasswordHash = user.PasswordHash!
            }, cancellationToken);
        }

        user.UserName = dto.UserName;
        user.PhoneNumber = dto.PhoneNumber;
        user.Email = dto.Email;
        user.Status = dto.Status;
        var info = user.Info;
        if (info is not null)
        {
            info.FirstName = dto.FirstName;
            info.LastName = dto.LastName;
            info.NationalCode = dto.NationalCode;
            info.BirthDate = birthDate;
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new BadRequestException();


        return UserResDto.FromEntity(user, _mapper);

        //await _userManager.ResetPasswordAsync(user, token, dto.Password!);
        //return UserDto.FromEntity(user, _mapper);
    }

    [Display(Name = "تغییر وضعیت")]
    [HttpPost("[action]")]
    public async Task<ApiResult> ChangeStatus(ChangeUserStatusDto dto, CancellationToken ct)
    {
        var user = await repository.GetByIdAsync(ct, dto.UserId);
        if (user is null)
            throw new NotFoundException("کاربر پیدا نشد");

        var userId = User.Identity!.GetUserId<int>();
        await hashEntityValidator.IsValidAsync(user, userId, ct);
        user.Status = dto.Status;
        return Ok();
    }

    [Display(Name = "حذف")]
    public override async Task<ApiResult> Delete(int id, CancellationToken cancellationToken)
    {
        var isAdmin =
            await repository.TableNoTracking.AnyAsync(i => i.Id == id && i.UserName == "admin", cancellationToken);
        if (isAdmin)
            throw new BadRequestException("کاربر مدیر نباید حذف شود");
        var cantDelete = await repository.TableNoTracking
            .Where(i => i.Id == id)
            .AnyAsync(i =>
                    i.CreatedBackups.Any() ||
                    i.CreatedNotifications.Any() ||
                    i.CreatedArchiveLogs.Any() ||
                    i.CreatedEmailHistories.Any() 
                , cancellationToken);
        if (cantDelete)
            throw new BadRequestException("نمیتوان کاربر را به دلیل انجام عملیات در سیستم حذف کرد");
        var model = await Repository.GetByIdAsync(cancellationToken, id!);
        if (model is null) throw new NotFoundException();
        var userId = User.Identity!.GetUserId<int>();
        await HashEntityValidator.IsValidAsync(model, userId, cancellationToken);
        await Repository.DeleteAsync(model, cancellationToken);

        return Ok();
    }


    [Display(Name = "پروفایل")]
    [HttpGet("[action]")]
    public async Task<ApiResult<UserProfileResDto>> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.Identity!.GetUserId<int>();
        var user = await repository.TableNoTracking
            .Include(i => i.Info)
            .FirstOrDefaultAsync(i => i.Id == userId, cancellationToken);
        if (user is null)
            throw new NotFoundException("کاربر پیدا نشد");
        var profileImagePath = await uploadedFileService.GetFilePath(nameof(UserInfo), user.Info.Id,
            UploadedFileType.UserProfile, cancellationToken);
        var roles = "";
        if (user.Roles.Any())
            roles = user.Roles.Select(i => i.Title).Aggregate((curr, next) => $"{curr}, {next}");
        var result = new UserProfileResDto
        {
            FirstName = user.Info.FirstName,
            LastName = user.Info.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            UserName = user.UserName,
            NationalCode = user.Info.NationalCode,
            BirthDate = user.Info.BirthDate == null ? "" : user.Info.BirthDate.Value.ToShamsi(false),
            ProfileImage = profileImagePath,
            Roles = roles
        };
        return Ok(result);
    }

    [Display(Name = "ویرایش پروفایل")]
    [HttpPost("[action]")]
    public async Task<ApiResult> ChangeProfile(UserProfileDto dto, CancellationToken cancellationToken)
    {
        var userId = User.Identity!.GetUserId<int>();
        var user = await Repository.GetByIdAsync(cancellationToken, userId);
        var userInfo = await userInfoRepository.Table
            .FirstOrDefaultAsync(i => i.UserId == user.Id, cancellationToken);
        if (user is null || userInfo is null)
            throw new NotFoundException("اطلاعات کاربر پیدا نشد");
        await hashEntityValidator.IsValidAsync(user, userId, cancellationToken);
        await hashEntityValidator.IsValidAsync(userInfo, userId, cancellationToken);
        if (dto.BirthDate.Length != 10)
            throw new BadRequestException("فیلد تاریخ تولد معتبر نیست");

        var parts = dto.BirthDate.Split('/');
        int year = int.Parse(parts[0]);
        int month = int.Parse(parts[1]);
        int day = int.Parse(parts[2]);

        var pc = new System.Globalization.PersianCalendar();
        userInfo.FirstName = dto.FirstName;
        userInfo.LastName = dto.LastName;
        userInfo.BirthDate = pc.ToDateTime(year, month, day, 0, 0, 0, 0);


        await userInfoRepository.UpdateAsync(userInfo, cancellationToken);
        await repository.UpdateAsync(user, cancellationToken);
        return Ok();
    }

    [Display(Name = "ویرایش عکس پروفایل")]
    [HttpPost("[action]")]
    public async Task<ApiResult> ChangeProfileImage([FromForm] ChangeProfileImageDto dto, CancellationToken ct)
    {
        var userId = User.Identity?.GetUserId<int>() ?? 0;
        var userInfo = await userInfoRepository.TableNoTracking.FirstOrDefaultAsync(i => i.UserId == userId);
        if (userInfo is null)
            throw new NotFoundException("اطلاعات کاربر پیدا نشد");
        if (dto.File is not null)
        {
            await uploadedFileService.SetDisableFilesAsync(ct, nameof(UserInfo), userInfo.Id,
                UploadedFileType.UserProfile);
            var filePath = await uploadedFileService.UploadFileAsync(dto.File, UploadedFileType.UserProfile,
                nameof(UserInfo), userInfo.Id, userId, ct);
        }

        return Ok();
    }

    [Display(Name = "ارسال کد یکبارمصرف برای تغییر موبایل")]
    [HttpPost("[action]")]
    public async Task<ApiResult> SendPhoneNumberOtp(CancellationToken ct)
    {
        var userId = User.Identity?.GetUserId<int>() ?? 0;
        var user = await repository.TableNoTracking.FirstOrDefaultAsync(i => i.Id == userId);
        if (user is null)
            throw new NotFoundException("کاربر پیدا نشد");
        if (user.PhoneNumber is not null)
        {
            var code = await _otpService.GenerateOtpAsync(user.PhoneNumber);
        }

        return Ok();
    }

    [Display(Name = "ثبت شماره موبایل جدید")]
    [HttpPost("[action]")]
    public async Task<ApiResult> SetNewPhoneNumber(SetNewPhoneNumberDto dto, CancellationToken ct)
    {
        var userId = User.Identity!.GetUserId<int>();
        var user = await repository.Table.FirstOrDefaultAsync(i => i.Id == userId);
        if (user is null)
            throw new AppException(ApiResultStatusCode.UnAuthorized, "کاربر پیدا نشد");
        await hashEntityValidator.IsValidAsync(user, userId, ct);

        if (user.PhoneNumber is not null)
        {
            if (!_otpService.VerifyOtp(user.PhoneNumber, dto.OtpCode))
            {
                throw new BadRequestException("کد صحیح نیست");
            }
        }


        if (!string.IsNullOrEmpty(user.Email))
        {
            user.Status = UserStatus.Enable;
            await repository.UpdateAsync(user, ct);
        }

        var otpCode = await _otpService.GenerateOtpAsync(dto.NewPhoneNumber);

        var phoneNumberHistory = new PhoneNumberHistory
        {
            PhoneNumber = dto.NewPhoneNumber,
            UserId = user.Id,
            CreatorUserId = user.Id,
            OtpCode = otpCode,
            IsConfirmed = false,
        };

        await phoneNumberHisoryRepository.AddAsync(phoneNumberHistory, ct);
        return Ok();
    }

    [Display(Name = "تایید شماره موبایل جدید")]
    [HttpPost("[action]")]
    public async Task<ApiResult> ConfirmNewPhoneNumber(SetNewPhoneNumberDto dto, CancellationToken ct)
    {
        var userId = User.Identity!.GetUserId<int>();
        var user = await repository.GetByIdAsync(ct, userId);
        if (user is null)
            throw new AppException(ApiResultStatusCode.UnAuthorized, "کاربر پیدا نشد");
        await hashEntityValidator.IsValidAsync(user, userId, ct);

        if (!_otpService.VerifyOtp(dto.NewPhoneNumber, dto.OtpCode))
        {
            throw new AppException(ApiResultStatusCode.UnAuthorized, "کد صحیح نیست");
        }

        var phoneNumberHistory =
            await phoneNumberHisoryRepository.Table.FirstOrDefaultAsync(
                i => i.PhoneNumber == dto.NewPhoneNumber && i.OtpCode == dto.OtpCode, ct);
        if (phoneNumberHistory is null)
            throw new NotFoundException("شماره موبایل صحیح نیست");

        phoneNumberHistory.IsConfirmed = true;
        await phoneNumberHisoryRepository.UpdateAsync(phoneNumberHistory, ct);
        user.PhoneNumber = dto.NewPhoneNumber;
        user.PhoneNumberConfirmed = true;

        await repository.UpdateAsync(user, ct);
        return Ok();
    }

    [Display(Name = "ارسال کد یکبارمصرف برای تغییر ایمیل")]
    [HttpPost("[action]")]
    public async Task<ApiResult> SendEmailOtp(CancellationToken ct)
    {
        var userId = User.Identity?.GetUserId<int>() ?? 0;
        var user = await repository.TableNoTracking.FirstOrDefaultAsync(i => i.Id == userId);
        if (user is null)
            throw new NotFoundException("کاربر پیدا نشد");
        if (user.Email is not null)
        {
            var code = await _otpService.GenerateOtpAsync(user.Email);
        }

        return Ok();
    }

    [Display(Name = "ثبت ایمیل جدید")]
    [HttpPost("[action]")]
    public async Task<ApiResult> SetNewEmail(SetNewEmailDto dto, CancellationToken ct)
    {
        var userId = User.Identity!.GetUserId<int>();
        var user = await repository.TableNoTracking.FirstOrDefaultAsync(i => i.Id == userId, ct);
        if (user is null)
            throw new AppException(ApiResultStatusCode.UnAuthorized, "کاربر پیدا نشد");

        await hashEntityValidator.IsValidAsync(user, userId, ct);

        if (user.Email is not null)
        {
            if (!_otpService.VerifyOtp(user.Email, dto.OtpCode))
            {
                throw new BadRequestException("کد صحیح نیست");
            }
        }

        var otpCode = await _otpService.GenerateOtpAsync(dto.NewEmail);

        var emailHistory = new EmailHistory
        {
            Email = dto.NewEmail,
            UserId = user.Id,
            CreatorUserId = user.Id,
            OtpCode = otpCode,
            IsConfirmed = false,
        };

        await emailHisoryRepository.AddAsync(emailHistory, ct);
        return Ok();
    }

    [Display(Name = "تایید ایمیل جدید")]
    [HttpPost("[action]")]
    public async Task<ApiResult> ConfirmNewEmail(SetNewEmailDto dto, CancellationToken ct)
    {
        var userId = User.Identity!.GetUserId<int>();
        var user = await repository.GetByIdAsync(ct, userId);
        if (user is null)
            throw new AppException(ApiResultStatusCode.UnAuthorized, "کاربر پیدا نشد");

        await hashEntityValidator.IsValidAsync(user, userId, ct);
        if (!_otpService.VerifyOtp(dto.NewEmail, dto.OtpCode))
        {
            throw new AppException(ApiResultStatusCode.UnAuthorized, "کد صحیح نیست");
        }

        var emailHistory =
            await emailHisoryRepository.Table.FirstOrDefaultAsync(
                i => i.Email == dto.NewEmail && i.OtpCode == dto.OtpCode, ct);
        if (emailHistory is null)
            throw new NotFoundException("شماره موبایل صحیح نیست");

        emailHistory.IsConfirmed = true;
        await emailHisoryRepository.UpdateAsync(emailHistory, ct);
        user.Email = dto.NewEmail;
        user.EmailConfirmed = true;

        await repository.UpdateAsync(user, ct);
        return Ok();
    }


    [Display(Name = "تغییر رمز")]
    [HttpPost("[action]")]
    public async Task<ApiResult> ChangePassword(ChangePasswordDto dto, CancellationToken ct)
    {
        var userId = User.Identity!.GetUserId<int>();
        var user = await Repository.GetByIdAsync(ct, userId);
        if (user is null) throw new NotFoundException("کاربر پیدا نشد");

        await hashEntityValidator.IsValidAsync(user, userId, ct);

        var hasPassword = await userManager.HasPasswordAsync(user);
        if (hasPassword)
        {
            var passwordHistory =
                await passwordHistoryService.CheckPasswordHistoryAsync(dto.NewPassword, user, user.Id, ct);
            var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                throw new BadRequestException(
                    result.Errors.Select(i => i.Description).Aggregate((crr, next) => $"{crr}, {next}")
                );
            passwordHistory.PasswordHash = user.PasswordHash!;
            await passwordHistoryService.AddAsync(passwordHistory, ct);
        }
        else
        {
            var result = await userManager.AddPasswordAsync(user, dto.NewPassword);
            if (!result.Succeeded)
                throw new BadRequestException(
                    result.Errors.Select(i => i.Description).Aggregate((crr, next) => $"{crr}, {next}")
                );
        }

        var tokens = await apiTokenRepository.Table
            .Where(i => i.UserId == user.Id && i.Enable == true)
            .ToListAsync(ct);

        tokens = tokens.Select(i =>
        {
            i.Enable = false;
            return i;
        }).ToList();

        await apiTokenRepository.UpdateRangeAsync(tokens, ct);
        var codes = tokens.Select(i => i.Code).ToArray();

        // await hubContext.Clients.All.SendAsync("LogOut", codes, ct);
        return Ok();
    }

    [Display(Name = "توکن های کاربر")]
    [HttpPost("[action]")]
    public async Task<ApiResult<IndexResDto<ApiTokenResDto>>> GetUserTokens(IndexDto dto, CancellationToken ct)
    {
        dto.Page = Math.Max(dto.Page, 1);
        dto.Limit = Math.Max(dto.Limit, 10);
        var userId = User.Identity!.GetUserId<int>();
        var query = apiTokenRepository.TableNoTracking
            .Where(i => i.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(dto.Search))
        {
            query = query.Where(i => i.User.UserName.Contains(dto.Search)
                                     || $"{i.User.Info.FirstName} {i.User.Info.LastName}".Contains(dto.Search)
                                     || i.Ip.Contains(dto.Search));
        }

        if (dto.Sort is null || string.IsNullOrEmpty(dto.Sort.By) || string.IsNullOrEmpty(dto.Sort.Type))
        {
            query = query.OrderByDescending(i => i.Id);
        }
        else
        {
            query = setSort(dto.Sort, query);
        }

        var total = await query.CountAsync();
        var models = await query
            .Skip((dto.Page - 1) * dto.Limit)
            .Take(dto.Limit)
            .ProjectTo<ApiTokenResDto>(Mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(new IndexResDto<ApiTokenResDto>
        {
            Data = models,
            Page = dto.Page,
            Limit = dto.Limit,
            Total = total
        });
    }

    [Display(Name = "غیرفعال کردن توکن کاربر")]
    [HttpPost("[action]")]
    public async Task<ApiResult> DisableUserTokens(DisableTokensDto dto, CancellationToken ct)
    {
        var userId = User.Identity!.GetUserId<int>();
        var model = await apiTokenRepository.Table
            .Where(i => i.UserId == userId)
            .Where(i => i.Id == dto.Id)
            .FirstOrDefaultAsync(ct);

        if (model is not null)
        {
            model.Enable = true;
            await apiTokenRepository.UpdateAsync(model, ct);
            // await hubContext.Clients.All.SendAsync("LogOut", new[] { model.Code }, ct);
        }

        return Ok();
    }

    [Display(Name = "توکن ها")]
    [HttpPost("[action]")]
    public async Task<ApiResult<IndexResDto<ApiTokenResDto>>> GetTokens(IndexDto dto, CancellationToken ct)
    {
        dto.Page = Math.Max(dto.Page, 1);
        dto.Limit = Math.Max(dto.Limit, 10);
        var query = apiTokenRepository.TableNoTracking
            .AsQueryable();

        if (!string.IsNullOrEmpty(dto.Search))
        {
            query = query.Where(i => i.User.UserName.Contains(dto.Search)
                                     || $"{i.User.Info.FirstName} {i.User.Info.LastName}".Contains(dto.Search)
                                     || i.Ip.Contains(dto.Search));
        }

        if (dto.Sort is null || string.IsNullOrEmpty(dto.Sort.By) || string.IsNullOrEmpty(dto.Sort.Type))
        {
            query = query.OrderByDescending(i => i.Id);
        }
        else
        {
            query = setSort(dto.Sort, query);
        }

        var total = await query.CountAsync();
        var models = await query
            .OrderByDescending(i => i.Id)
            .Where(i => i.UserId != 1)
            .Skip((dto.Page - 1) * dto.Limit)
            .Take(dto.Limit)
            .ProjectTo<ApiTokenResDto>(Mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return Ok(new IndexResDto<ApiTokenResDto>
        {
            Data = models,
            Page = dto.Page,
            Limit = dto.Limit,
            Total = total
        });
    }

    [Display(Name = "غیرفعال کردن توکن")]
    [HttpPost("[action]")]
    public async Task<ApiResult> DisableTokens(DisableTokensDto dto, CancellationToken ct)
    {
        var model = await apiTokenRepository.Table
            .Where(i => i.Id == dto.Id)
            .Where(i => i.Enable == true)
            .FirstOrDefaultAsync(ct);

        if (model is not null)
        {
            model.Enable = false;
            await apiTokenRepository.UpdateAsync(model, ct);
            // await hubContext.Clients.All.SendAsync("LogOut", new[] { model.Code }, ct);
        }

        return Ok();
    }

    [Display(Name = "اعلان ها")]
    [HttpPost("[action]")]
    public async Task<ApiResult<IndexResDto<NotificationResDto>>> Notifications(IndexDto dto, CancellationToken ct)
    {
        dto.Page = Math.Max(dto.Page, 1);
        dto.Limit = Math.Max(dto.Limit, 10);
        var query = notificationRepository.TableNoTracking.AsQueryable();
        query = query.Include(i => i.User);

        if (!string.IsNullOrEmpty(dto.Search))
        {
            query = query.Where(i =>
                i.User.UserName.Contains(dto.Search)
                || i.Title.Contains(dto.Search)
            );
        }

        var total = await query.CountAsync(ct);
        var models = await query
            .OrderByDescending(i => i.Id)
            .Skip((dto.Page - 1) * dto.Limit)
            .Take(dto.Limit)
            .ProjectTo<NotificationResDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return Ok(new IndexResDto<NotificationResDto>
        {
            Data = models,
            Page = dto.Page,
            Limit = dto.Limit,
            Total = total
        });
    }

    [Display(Name = "اعلان های کاربر")]
    [HttpPost("[action]")]
    public async Task<ApiResult<IndexResDto<NotificationResDto>>> UserNotifications(IndexDto dto, CancellationToken ct)
    {
        dto.Page = Math.Max(dto.Page, 1);
        dto.Limit = Math.Max(dto.Limit, 10);
        var userId = User.Identity!.GetUserId<int>();
        var query = notificationRepository.Table.AsQueryable();
        query = query.Where(i => i.UserId == userId);
        query = query.Include(i => i.User);

        if (!string.IsNullOrEmpty(dto.Search))
        {
            query = query.Where(i =>
                i.User.UserName.Contains(dto.Search)
                || i.Title.Contains(dto.Search)
            );
        }

        var total = await query.CountAsync(ct);
        var models = await query
            .OrderByDescending(i => i.Id)
            .Skip((dto.Page - 1) * dto.Limit)
            .Take(dto.Limit)
            .ToListAsync(ct);

        var list = Mapper.Map<List<NotificationResDto>>(models);
        var changeEnable = models
            .Where(i => i.Status == NotificationStatus.Unseen)
            .Select(i =>
            {
                i.Status = NotificationStatus.Seen;
                i.SeenDate = DateTimeOffset.Now;
                return i;
            })
            .ToList();
        await notificationRepository.UpdateRangeAsync(changeEnable, ct);

        return Ok(new IndexResDto<NotificationResDto>
        {
            Data = list,
            Page = dto.Page,
            Limit = dto.Limit,
            Total = total
        });
    }


    public override async Task<ApiResult<List<SelectDto>>> GetSelectList(CancellationToken cancellationToken)
    {
        var models = await repository.TableNoTracking.Where(i => i.UserName != "admin")
            .Where(i => i.Status != UserStatus.Disable)
            .Include(i => i.Info)
            .ToListAsync(cancellationToken);
        return models.Select(i => new SelectDto
        {
            Id = i.Id,
            Title = $"{i.Info.FirstName} {i.Info.LastName}"
        }).ToList();
    }


    [HttpGet("[action]")]
    [Display(Name = "دسترسی های کاربر")]
    public async Task<ApiResult<List<string>>> Permissions(CancellationToken ct)
    {
        var userId = User.Identity!.GetUserId<int>();
        if (userId == 1)
            return Ok(Domain.Entities.Permissions.All.Select(i => $"{i.Controller}.{i.Action}").ToList());
        var roles = await roleRepository.TableNoTracking
            .Where(g => g.Users.Any(u => u.Id == userId))
            .ToListAsync(ct);
        var permissions = new List<string>();
        foreach (var role in roles)
        {
            var claims = await roleManager.GetClaimsAsync(role);
            permissions.AddRange(claims.Select(i => i.Value));
        }
        return Ok(permissions);
    }
}