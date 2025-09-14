using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Service.DataInitializer;

public class RoleDataInitializer( RoleManager<Role> roleManager): IDataInitializer
{
    public async Task InitializerData()
    {
        var role = await roleManager.Roles.Where(i => i.Name == "Administrator").FirstOrDefaultAsync();
        if (role is null)
        {
            role = new Role{ Name = "Administrator", Title = "سیستم"};
            await roleManager.CreateAsync(role);
        }
        
    }
    
}