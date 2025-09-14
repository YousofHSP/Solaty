using Common;
using Data;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WebFramework.Identity;

namespace WebFramework.Configuration
{
    public static class IdentityConfigurationExtensions
    {
        public static void AddCustomIdentity(this IServiceCollection services, IdentitySettings settings)
        {
            services.AddIdentity<User, Role>(options =>
                {
                    options.Password.RequireDigit = settings.PasswordRequireDigit;
                    options.Password.RequiredLength = settings.PasswordRequiredLength;
                    options.Password.RequireNonAlphanumeric = settings.PasswordRequireNonAlphanumeric;
                    options.Password.RequireUppercase = settings.PasswordRequireUppercase;
                    options.Password.RequireLowercase = settings.PasswordRequireLowercase;

                    // options.User.RequireUniqueEmail = settings.RequireUniqueEmail;

                    //options.SignIn.RequireConfirmedPhoneNumber = false;
                    //options.SignIn.RequireConfirmedEmail = false;

                    //options.Lockout.MaxFailedAccessAttempts = 5;
                })
                .AddErrorDescriber<PersianIdentityErrorDescriber>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }
    }
}