using GestionTubos.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GestionTubos.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static readonly string[] Roles = ["Superadministrador", "Administrador", "Empleado"];
    public static async Task SeedAsync(IServiceProvider services, string correo, string contrasena)
    {
        using var scope = services.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in Roles) if (!await roles.RoleExistsAsync(rol)) await roles.CreateAsync(new IdentityRole(rol));
        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<UsuarioAplicacion>>();
        var usuario = await usuarios.FindByEmailAsync(correo);
        if (usuario is null)
        {
            usuario = new UsuarioAplicacion { UserName = correo, Email = correo, EmailConfirmed = true, NombreCompleto = "Superadministrador" };
            var resultado = await usuarios.CreateAsync(usuario, contrasena);
            if (!resultado.Succeeded) throw new InvalidOperationException(string.Join("; ", resultado.Errors.Select(x => x.Description)));
        }
        if (!await usuarios.IsInRoleAsync(usuario, "Superadministrador")) await usuarios.AddToRoleAsync(usuario, "Superadministrador");
    }
}
