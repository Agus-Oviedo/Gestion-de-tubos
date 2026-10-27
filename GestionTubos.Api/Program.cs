using GestionTubos.Infrastructure;
using GestionTubos.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

PdfSharp.Fonts.GlobalFontSettings.UseWindowsFontsUnderWindows = true;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GestionTubos.Application.Interfaces.IUsuarioActual,GestionTubos.Api.Services.UsuarioActual>();
builder.Services.AddSingleton<GestionTubos.Api.Services.RemitoPdfGenerator>();
builder.Services.AddSingleton<GestionTubos.Api.Services.InventarioPdfGenerator>();
builder.Services.AddInfrastructure(builder.Configuration);
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Falta Jwt:Key");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), ClockSkew = TimeSpan.FromMinutes(1)
    };
});
builder.Services.AddAuthorization(options => options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .RequireRole(IdentitySeeder.Roles)
    .Build());
builder.Services.AddCors(options => options.AddPolicy("Blazor", policy =>
    policy.SetIsOriginAllowed(origin =>
        Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.Port == 5000)
        .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseCors("Blazor");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.MapGet("/api/estado", () => Results.Ok(new
{
    aplicacion = "Control de Stock de Tubos",
    estado = "activa"
}));

await GestionTubos.Api.Services.ServerUpgrade.EjecutarAsync(app);
await GestionTubos.Api.Services.ServerUpgrade.EjecutarAsync(app,"auditoria-20261015");
await GestionTubos.Api.Services.ServerUpgrade.EjecutarAsync(app,"planchadas-20261015");
await GestionTubos.Api.Services.ServerUpgrade.EjecutarAsync(app,"clientes-carga-20261027");
using(var planchadasScope=app.Services.CreateScope())await planchadasScope.ServiceProvider.GetRequiredService<GestionTubos.Infrastructure.Services.PlanchadaService>().InicializarAsync();
using(var auditoriaScope=app.Services.CreateScope())await auditoriaScope.ServiceProvider.GetRequiredService<GestionTubos.Application.Interfaces.IPedidoService>().RegistrarEstadosInicialesAsync(default);
await GestionTubos.Api.Services.CorregirContenidoInicial.EjecutarAsync(app);

await IdentitySeeder.SeedAsync(app.Services,
    builder.Configuration["UsuarioInicial:Correo"] ?? "admin@oxigenonorte.local",
    builder.Configuration["UsuarioInicial:Contrasena"] ?? throw new InvalidOperationException("Falta UsuarioInicial:Contrasena"));

app.Run();
