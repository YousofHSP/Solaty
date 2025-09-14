using Common.Utilities;
using Data.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Attributes;

public class HasPermissionAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
{
    private string? _permission;
    private readonly RoleManager<Role> _roleManager;
    private readonly IUserRepository _userRepository;

    public HasPermissionAttribute(string permission, RoleManager<Role> roleManager, IUserRepository userRepository)
    {
        _permission = permission;
        _roleManager = roleManager;
        _userRepository = userRepository;
    }

    public HasPermissionAttribute(RoleManager<Role> roleManager, IUserRepository userRepository)
    {
        _permission = null;
        _roleManager = roleManager;
        _userRepository = userRepository;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userIdentity = context.HttpContext.User;
        var userId = userIdentity.Identity?.GetUserId<int>() ?? 0;
        var user = await _userRepository.TableNoTracking
            .Include(i => i.Roles)
            .FirstOrDefaultAsync(i => i.Id == userId);
        if (user is null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }


        if (_permission is null)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();
            _permission = $"{controller}.{action}";
        }

        var permissions = new List<string>();

        var roles = user.Roles.ToList();
        foreach (var role in roles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            permissions.AddRange(claims.Select(i => i.Value));
        }
        var hasPermission = permissions.Any(i => i == _permission);
        if (!hasPermission)
            context.Result = new ForbidResult();
    }
}