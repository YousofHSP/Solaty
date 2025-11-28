using System.ComponentModel.DataAnnotations;
using Domain.Entities;
using Shared.Validations;

namespace Shared.DTOs
{
    public class LoginDto
    {
        
        [Display(Name = "نام کاربری")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string UserName { get; set; }

        [Display(Name = "رمز")]
        public string? Password { get; set; }

        public string? OtpCode { get; set; }
    }
    public class AuthDto
    {
        [Required] public string grant_type { get; set; }
        [Required] public string username { get; set; }
        [Required] public string password { get; set; }
        public string? refresh_token { get; set; }
        public string? scope { get; set; }

        public string? client_id { get; set; }
        public string? client_secret { get; set; }
    }

    public class SendOtpRequest
    {
        [Display(Name = "نام کاربری")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string UserName { get; set; }

        [Display(Name = "رمز")]
        public string? Password { get; set; }
    }
    public class VerifyOtpRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string OtpCode { get; set; }

    }

    public class ResetPasswordDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string OtpCode { get; set; }
        [Required]
        [PasswordPolicy]
        public string Password { get; set; }
        [Required]
        public string ResetPasswordToken { get; set; }
    }
    public class LoginResDto
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string AccessCode { get; set; }
        public string UserFullName { get; set; }
        public string ProfileImage { get; set; }
        public string UserName { get; set; }
    }
    public class ResetPasswordSendOtpDto
    {
        public string UserName { get; set; }
    }

    public class RegisterDto
    {
        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; }
        [Display(Name = "ایمیل")]
        public string Email { get; set; }
        [Display(Name = "شماره مویایل")]
        public string PhoneNumber { get; set; }
        [Display(Name = "رمز")]
        public string Password { get; set; }
        [Display(Name = "کد اتصال")]
        public string ConnectCode { get; set; }
        [Display(Name = "جنسیت")]
        public GenderType Gender { get; set; }
        [Display(Name = "کد یکبار مصرف")]
        public string OtpCode{ get; set; }
    }

}
