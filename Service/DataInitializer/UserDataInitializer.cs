using Data.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Service.DataInitializer;

public class UserDataInitializer(UserManager<User> userManager, IRepository<UserInfo> userInfoRepository)
    : IDataInitializer
{
    public async Task InitializerData()
    {
        var isExists = await userManager.FindByNameAsync("admin");
        if (isExists is null)
        {
            
            var user = new User
            {
                UserName = "admin",
                PhoneNumber = "09140758738",
                Status = UserStatus.Enable
            };
            await userManager.CreateAsync(user, "1qaz@WSX3edc");
            await userManager.AddToRoleAsync(user, "Administrator");
            var info = new UserInfo { FirstName = "سیستم", BirthDate = DateTime.Today, LastName = "", UserId = user.Id, NationalCode = "1234567890"};
            await userInfoRepository.AddAsync(info, default);
        }
    }
}