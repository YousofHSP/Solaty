using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Common.Utilities;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Shared.Validations;

namespace Shared.DTOs;

public class UserDto : BaseDto<UserDto, User>
{
    [Display(Name = "نام و نام خانوادگی")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string FullName { get; set; } = null!;

    [Display(Name = "موبایل")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string PhoneNumber { get; set; } = null!;

    [Display(Name = "تاریخ تولید")]
    public string? BirthDate { get; set; } = null!;

    [Display(Name = "ایمیل")]
    public string? Email { get; set; }

    [Display(Name = "وضعیت")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public UserStatus Status { get; set; }

    [Display(Name = "گروه کاری")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    //[ExistsInDatabase<UserGroup>(nameof(UserGroup.Id), ErrorMessage = "{0} پیدا نشد")]
    public List<long> RoleIds { get; set; } = [];

    [Display(Name = "رمز")]
    public string? Password { get; set; }
}

public class UserResDto : BaseDto<UserResDto, User>
{
    [Display(Name = "نام و نام خانوادگی")]
    public string FullName { get; set; }
    [Display(Name = "موبایل")]
    public string PhoneNumber { get; set; }
    [Display(Name = "ایمیل")]
    public string Email { get; set; }
    [Display(Name = "تاریخ تولد")]
    public string BirthDate { get; set; }
    [Display(Name = "نقش ها")]
    public string Roles{ get; set; }
    public List<long> RoleIds{ get; set; }
    [Display(Name = "وضعیت")]
    public UserStatus Status { get; set; }

    protected override void CustomMappings(IMappingExpression<User, UserResDto> mapping)
    {
        mapping.ForMember(
            d => d.FullName,
            s => s.MapFrom(m => m.Info.FullName));
        mapping.ForMember(
            d => d.BirthDate,
            s => s.MapFrom(m => m.Info.BirthDate == null ? "" : m.Info.BirthDate.Value.ToShamsi(default)));
        mapping.ForMember(
            d => d.Roles,
            s => s.MapFrom(m => string.Join(", ", m.Roles.Select(i => i.Title))));
        mapping.ForMember(
            d => d.RoleIds,
            s => s.MapFrom(m => m.Roles.Select(i => i.Id)));
        base.CustomMappings(mapping);
    }
}

public class UserProfileResDto
{
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string BirthDate { get; set; }
    public string Email { get; set; }
    public string ProfileImage { get; set; }
    public string Roles { get; set; }
}
public class UserProfileDto
{
    [Display(Name = "نام و نام خانوادگی")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string FullName { get; set; }
    [Display(Name = "تاریخ تولد")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string BirthDate { get; set; }
}
public class SetNewPhoneNumberDto
{
    [Required]
    public string NewPhoneNumber { get; set; }
    public string? OtpCode { get; set; }

}
public class ConfirmeNewPhoneNumberDto
{
    [Required]
    public string NewPhoneNumber { get; set; }
    [Required]
    public string OtpCode { get; set; }

}
public class SetNewEmailDto
{
    public string NewEmail { get; set; }
    public string? OtpCode { get; set; }

}
public class ChangeProfileImageDto
{
    public IFormFile File { get; set; }
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}

public class CheckTokenDto
{
    public string Token { get; set; }
}
public class ChangeUserStatusDto
{
    public long UserId { get; set; }
    public UserStatus Status { get; set; }
}

public class DisableTokensDto
{
    [Required]
    public long Id{ get; set; }
}