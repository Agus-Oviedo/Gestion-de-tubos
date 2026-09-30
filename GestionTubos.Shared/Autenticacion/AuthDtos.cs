using System.ComponentModel.DataAnnotations;
namespace GestionTubos.Shared.Autenticacion;
public sealed class LoginDto { [Required, EmailAddress] public string Correo { get; set; } = string.Empty; [Required] public string Contrasena { get; set; } = string.Empty; }
public sealed record LoginRespuestaDto(string Token, DateTime ExpiraUtc, string Nombre, string Correo, IReadOnlyList<string> Roles);
public sealed class UsuarioCrearDto { [Required, StringLength(150)] public string NombreCompleto { get; set; } = string.Empty; [Required, EmailAddress] public string Correo { get; set; } = string.Empty; [Required, MinLength(8)] public string Contrasena { get; set; } = string.Empty; [Required] public string Rol { get; set; } = "Empleado"; }
public sealed record UsuarioDto(string Id, string NombreCompleto, string Correo, bool Activo, IReadOnlyList<string> Roles);
public sealed class UsuarioRolDto { [Required] public string Rol { get; set; } = "Empleado"; }
public sealed class RegistroDto
{
    [Required, StringLength(150)] public string NombreCompleto { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(256)] public string Correo { get; set; } = string.Empty;
    [Required, MinLength(8), StringLength(128)] public string Contrasena { get; set; } = string.Empty;
    [Required, Compare(nameof(Contrasena), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}
