using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Shared.DTOs;

namespace Shared.Validations
{
    public class PasswordPolicyAttribute: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;
            var passwordPoliciesKeys = new List<int>
            {
                (int)SettingKey.PasswordRequiredLength,
                (int)SettingKey.PasswordRequireDigit,
                (int)SettingKey.PasswordRequireNonAlphanumeric,
                (int)SettingKey.PasswordRequireLowercase,
                (int)SettingKey.PasswordRequireUppercase,
            };

            if (string.IsNullOrEmpty(password))
                return new ValidationResult("رمز اجباری است");

            var service = validationContext.GetService<ISettingService>();

            if (service == null)
                throw new InvalidOperationException("سرویس نامعتبر است");

            var passwordRequiredLength = service.GetValue<int>(SettingKey.PasswordRequiredLength);
            var passwordRequiredDgigt = service.GetValue<string>(SettingKey.PasswordRequireDigit);
            var passwordRequiredNonAlphanumeric = service.GetValue<string>(SettingKey.PasswordRequireNonAlphanumeric);
            var passwordRequiredLowercase = service.GetValue<string>(SettingKey.PasswordRequireLowercase);
            var passwordRequiredUppercase = service.GetValue<string>(SettingKey.PasswordRequireUppercase);



            if (password.Length < passwordRequiredLength)
                return new ValidationResult($"رمز عبور باید حداقل {passwordRequiredLength} کاراکتر باشد.");

            if (passwordRequiredDgigt == "1" && !password.Any(char.IsDigit))
                return new ValidationResult("رمز عبور باید حداقل شامل یک رقم باشد.");

            if (passwordRequiredNonAlphanumeric == "1" && password.All(char.IsLetterOrDigit))
                return new ValidationResult("رمز عبور باید حداقل شامل یک کاراکتر غیرعددی-حروفی (مثل @، #، ! و ...) باشد.");

            if (passwordRequiredUppercase == "1" && !password.Any(char.IsUpper))
                return new ValidationResult("رمز عبور باید حداقل شامل یک حرف بزرگ انگلیسی باشد.");

            if (passwordRequiredLowercase == "1" && !password.Any(char.IsLower))
                return new ValidationResult("رمز عبور باید حداقل شامل یک حرف کوچک انگلیسی باشد.");


            return ValidationResult.Success;
        }
    }
}
