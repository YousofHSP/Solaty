using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Common.Utilities;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Shared.Validations;

namespace Shared.DTOs;

public class UserDto : BaseDto<UserDto, User>
{
    [Display(Name = "نام")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string FirstName { get; set; } = null!;
    [Display(Name = "نام خانوادگی")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string LastName { get; set; } = null!;

    [Display(Name = "نام کاربری")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string UserName { get; set; } = null!;
    [Display(Name = "موبایل")]
    public string? PhoneNumber { get; set; } = null!;
    [Display(Name = "کدملی")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string NationalCode { get; set; } = null!;

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
    public List<int> RoleIds { get; set; } = [];

    [Display(Name = "رمز")]
    public string? Password { get; set; }
}

public class UserResDto : BaseDto<UserResDto, User>
{
    [Display(Name = "نام")]
    public string FirstName { get; set; }
    [Display(Name = "نام خانوادگی")]
    public string LastName { get; set; }
    public string NationalCode { get; set; }
    [Display(Name = "نام کاربری")]
    public string UserName { get; set; } = null!;
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string BirthDate { get; set; }
    [Display(Name = "نقش ها")]
    public string Roles{ get; set; }
    public List<int> RoleIds{ get; set; }
    [Display(Name = "وضعیت")]
    public UserStatus Status { get; set; }

    protected override void CustomMappings(IMappingExpression<User, UserResDto> mapping)
    {
        mapping.ForMember(
            d => d.FirstName,
            s => s.MapFrom(m => m.Info.FirstName));
        mapping.ForMember(
            d => d.LastName,
            s => s.MapFrom(m => m.Info.LastName));
        mapping.ForMember(
            d => d.NationalCode,
            s => s.MapFrom(m => m.Info.NationalCode));
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
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string BirthDate { get; set; }
    public string Email { get; set; }
    public string NationalCode { get; set; }
    public string ProfileImage { get; set; }
    public string Roles { get; set; }
}
public class UserProfileDto
{
    [Display(Name = "نام")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string FirstName { get; set; }
    [Display(Name = "نام خانوادگی")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string LastName { get; set; }
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
    public int UserId { get; set; }
    public UserStatus Status { get; set; }
}

public class DisableTokensDto
{
    [Required]
    public int Id{ get; set; }
}