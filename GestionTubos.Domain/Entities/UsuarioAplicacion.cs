using Microsoft.AspNetCore.Identity;
namespace GestionTubos.Domain.Entities;
public sealed class UsuarioAplicacion : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
