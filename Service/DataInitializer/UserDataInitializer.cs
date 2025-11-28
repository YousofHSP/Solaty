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
                PhoneNumberConfirmed = true,
                Enable= true
            };
            await userManager.CreateAsync(user, "1qaz@WSX3edc");
            var info = new UserInfo { FullName = "سیستم", BirthDate = DateTime.Today, UserId = user.Id, ConnectCode = ""};
            await userInfoRepository.AddAsync(info, default);
        }
    }
}