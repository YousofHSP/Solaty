using Domain.Entities;
using Domain.Model;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Shared;
using WebClient.Components;
using WebClient.Services;
using WebClient.Services.Api;
using WebClient.Services.Common;
using WebClient.Services.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpContextAccessor();
// Component Service
builder.Services.AddScoped<ToastService>();
builder.Services.AddSingleton<CircuitHandler, TrackingCircuitHandler>();

// Api Service
builder.Services.AddScoped<AuthService>();
builder.Services.AddControllers();
builder.Services.AddHttpClient<IBaseService, BaseService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7063/api/");
});



builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Auth/Login";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;

});
builder.Services.AddAuthorization(options =>
{
    foreach (var per in Permissions.All.Select(p => $"{p.Controller}.{p.Action}"))
        options.AddPolicy(per, policy => policy.RequireClaim("permission", per));
});
// builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthStateProvider>();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();