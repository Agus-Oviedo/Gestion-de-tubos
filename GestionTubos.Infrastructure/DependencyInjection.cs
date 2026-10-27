using GestionTubos.Application.Interfaces;
using GestionTubos.Infrastructure.Data;
using GestionTubos.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GestionTubos.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace GestionTubos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("GestionTubos")
            ?? throw new InvalidOperationException("No se configuró la conexión 'GestionTubos'.");

        services.AddDbContext<GestionTubosDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddIdentityCore<UsuarioAplicacion>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        }).AddRoles<IdentityRole>().AddEntityFrameworkStores<GestionTubosDbContext>().AddDefaultTokenProviders();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<ITuboService, TuboService>();
        services.AddScoped<CargaLoteService>();
        services.AddScoped<IngresoClienteService>();
        services.AddScoped<PlanchadaService>();
        services.AddScoped<IOperacionService, OperacionService>();
        services.AddScoped<IPedidoService, PedidoService>();
        services.AddScoped<IRepartoService, RepartoServiceAuditado>();

        return services;
    }
}
