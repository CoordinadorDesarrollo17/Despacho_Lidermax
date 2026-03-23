using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Repositories;
using Sln_Lidermax.Services;

using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<DapperContext>();

builder.Services.AddTransient<IHojasRutaService, HojasRutaService>();
builder.Services.AddTransient<IHojasRutaRepository, HojasRutaRepository>();

builder.Services.AddTransient<ITicketsService, TicketsService>();
builder.Services.AddTransient<ITicketsRepository, TicketsRepository>();

//LOGIN
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Denied";
    });
builder.Services.AddAuthorization();


var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); //LOGIN
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
