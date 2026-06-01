using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;
using Sln_Lidermax.Repositories;
using Sln_Lidermax.Services;

var builder = WebApplication.CreateBuilder(args);

var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados)); //pasamos la política para que se aplique globalmente a todos los controladores y métodos.
});
builder.Services.AddHttpContextAccessor(); //permite acceder al HttpContext actual (la petición en curso) desde clases donde normalmente no lo tendrías disponible como repositorios,servicios,etc , en cambio en el controlador si tenemos acceso porque hereda de ControllerBase 

//Usamos Identity
builder.Services.AddTransient<IUserStore<UsuarioModel>, UsuarioStore>(); //Cada vez que alguien pida un IUserStore<Usuario>, entregamos una instancia de nuestra clase UsuarioStore

builder.Services.AddIdentityCore<UsuarioModel>(opciones =>
{
    opciones.Password.RequireDigit = false;
    opciones.Password.RequireLowercase = false;
    opciones.Password.RequireUppercase = false;
    opciones.Password.RequireNonAlphanumeric = false;
})
.AddSignInManager()                // <-- IMPORTANTE si usas SignInManager
.AddDefaultTokenProviders()
.AddUserStore<UsuarioStore>();

builder.Services.AddScoped<IPasswordHasher<UsuarioModel>, PlainComparePasswordHasher>();

//Aplicamos Cookie, este codigo permite que nuestra aplicacion entienda el uso de cookies para autenticacion
/* En resumen hace lo siguiente: 
        Voy a usar autenticación basada en cookies para toda la aplicación.
        Cuando un usuario se loguee, guardaré un ticket de autenticación en una cookie (Identity.Application).
        En cada request, leeré esa cookie para saber quién es el usuario.
        Si no tiene cookie y entra a una página protegida, lo reto (Challenge) mandándolo al login.
        Y cuando cierre sesión, borraré esa cookie.
  */
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme; //cómo autenticar al usuario en cada request (aquí: usando la cookie de Identity).
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme; //qué hacer si un usuario no está autenticado y entra a una página [Authorize] (Identity lo redirige al login).
    options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme; //qué esquema usar para cerrar sesión (también cookies).
}).AddCookie(IdentityConstants.ApplicationScheme, opciones =>
{
    opciones.LoginPath = "/Auth/login"; // Si alguien intenta acceder a una acción protegida con [Authorize] y no está autenticado, lo redirigimos a esta URL”.
    opciones.AccessDeniedPath = "/Home/Index";    // <- Redirige al Home si no tiene permiso
});


builder.Services.AddSingleton<DapperContext>();

builder.Services.AddTransient<IHojasRutaService, HojasRutaService>();
builder.Services.AddTransient<IHojasRutaRepository, HojasRutaRepository>();

builder.Services.AddTransient<ITicketsService, TicketsService>();
builder.Services.AddTransient<ITicketsRepository, TicketsRepository>();

builder.Services.AddTransient<ITicketsOperarioService, TicketsOperarioService>();
builder.Services.AddTransient<ITicketsOperarioRepository, TicketsOperarioRepository>();

builder.Services.AddTransient<ITicketsCoordinadosService, TicketsCoordinadosService>();
builder.Services.AddTransient<ITicketsCoordinadosRepository, TicketsCoordinadosRepository>();

//LOGIN
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();



builder.Services.AddAuthorization();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); //LOGIN
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
