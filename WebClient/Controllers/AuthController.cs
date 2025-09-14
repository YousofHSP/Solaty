using Microsoft.AspNetCore.Mvc;
using WebClient.Services.Api;

namespace WebClient.Controllers;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("signin")]
    public async Task<IActionResult> SignIn([FromQuery] string jwtToken)
    {
        await _authService.SignInAsync(jwtToken);
        return LocalRedirect("/");
    }

    [HttpGet("signout")]
    public async Task<IActionResult> SignOut()
    {
        await _authService.SignOutAsync();
        return LocalRedirect("/Auth/Login");
    }
}