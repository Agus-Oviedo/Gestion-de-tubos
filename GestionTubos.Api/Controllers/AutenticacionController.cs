using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestionTubos.Domain.Entities;
using GestionTubos.Infrastructure.Identity;
using GestionTubos.Shared.Autenticacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using GestionTubos.Infrastructure.Data;

namespace GestionTubos.Api.Controllers;

[ApiController, Route("api/autenticacion")]
public sealed class AutenticacionController(UserManager<UsuarioAplicacion> usuarios, IConfiguration config, GestionTubosDbContext db) : ControllerBase
{
    [AllowAnonymous, HttpPost("registro")]
    public async Task<IActionResult> Registro(RegistroDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NombreCompleto)) return BadRequest(new { mensaje = "Ingresá tu nombre completo." });
        var correo = dto.Correo.Trim();
        if (await usuarios.FindByEmailAsync(correo) is not null) return Conflict(new { mensaje = "Ya existe una cuenta con ese correo." });
        var usuario = new UsuarioAplicacion { UserName = correo, Email = correo, NombreCompleto = dto.NombreCompleto.Trim(), Activo = false, EmailConfirmed = false };
        var creado = await usuarios.CreateAsync(usuario, dto.Contrasena);
        if (!creado.Succeeded) return BadRequest(new { mensaje = "No se pudo crear la cuenta. Revisá el correo y usá una contraseña de al menos 8 caracteres, con mayúscula, minúscula y número." });
        return Ok(new { mensaje = "Cuenta creada. Un administrador debe asignarte un rol y habilitarla antes de que puedas ingresar." });
    }

    [Authorize(Roles = "Superadministrador,Administrador"), HttpPost("usuarios/{id}/aprobar")]
    public async Task<IActionResult> Aprobar(string id)
    {
        var usuario = await usuarios.FindByIdAsync(id);
        if (usuario is null) return NotFound();
        if (usuario.Activo) return Conflict(new { mensaje = "La cuenta ya está habilitada." });
        var roles = await usuarios.GetRolesAsync(usuario);
        if (roles.Count != 1 || !IdentitySeeder.Roles.Contains(roles[0])) return BadRequest(new { mensaje = "Primero asigná un rol válido a la cuenta." });
        if (roles[0] == "Superadministrador" && !User.IsInRole("Superadministrador")) return Forbid();
        usuario.Activo = true;
        var resultado = await usuarios.UpdateAsync(usuario);
        if (!resultado.Succeeded) return BadRequest(new { mensaje = "No se pudo habilitar la cuenta." });
        return Ok(new { mensaje = "Cuenta habilitada." });
    }
    [AllowAnonymous, HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var usuario = await usuarios.FindByEmailAsync(dto.Correo);
        if (usuario is null || !usuario.Activo || !await usuarios.CheckPasswordAsync(usuario, dto.Contrasena)) return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
        var roles = await usuarios.GetRolesAsync(usuario);
        if (roles.Count == 0) return Unauthorized(new { mensaje = "Tu cuenta todavía no tiene un rol asignado." });
        var expira = DateTime.UtcNow.AddHours(config.GetValue("Jwt:HorasExpiracion", 8));
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, usuario.Id), new(ClaimTypes.NameIdentifier, usuario.Id), new(ClaimTypes.Name, usuario.NombreCompleto), new(ClaimTypes.Email, usuario.Email!) };
        claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x)));
        var token = new JwtSecurityToken(config["Jwt:Issuer"], config["Jwt:Audience"], claims, expires: expira, signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)), SecurityAlgorithms.HmacSha256));
        return Ok(new LoginRespuestaDto(new JwtSecurityTokenHandler().WriteToken(token), expira, usuario.NombreCompleto, usuario.Email!, roles.ToList()));
    }
    [Authorize(Roles = "Superadministrador,Administrador"), HttpGet("usuarios")]
    public async Task<IActionResult> ListaUsuarios() { var resultado = new List<UsuarioDto>(); foreach (var u in usuarios.Users.OrderBy(x => x.NombreCompleto)) resultado.Add(new(u.Id, u.NombreCompleto, u.Email!, u.Activo, (await usuarios.GetRolesAsync(u)).ToList())); return Ok(resultado); }
    [Authorize(Roles = "Superadministrador,Administrador"), HttpPost("usuarios")]
    public async Task<IActionResult> CrearUsuario(UsuarioCrearDto dto)
    {
        if (!IdentitySeeder.Roles.Contains(dto.Rol)) return BadRequest(new { mensaje = "Rol inválido." });
        if (dto.Rol == "Superadministrador" && !User.IsInRole("Superadministrador")) return Forbid();
        if (await usuarios.FindByEmailAsync(dto.Correo) is not null) return Conflict(new { mensaje = "Ya existe un usuario con ese correo." });
        var u = new UsuarioAplicacion { UserName = dto.Correo.Trim(), Email = dto.Correo.Trim(), EmailConfirmed = true, NombreCompleto = dto.NombreCompleto.Trim() };
        var creado = await usuarios.CreateAsync(u, dto.Contrasena); if (!creado.Succeeded) return BadRequest(new { mensaje = string.Join("; ", creado.Errors.Select(x => x.Description)) });
        await usuarios.AddToRoleAsync(u, dto.Rol); return Ok(new UsuarioDto(u.Id, u.NombreCompleto, u.Email, true, [dto.Rol]));
    }

    [Authorize(Roles = "Superadministrador,Administrador"), HttpPut("usuarios/{id}/rol")]
    public async Task<IActionResult> CambiarRol(string id, UsuarioRolDto dto)
    {
        if (!IdentitySeeder.Roles.Contains(dto.Rol)) return BadRequest(new { mensaje = "Rol inválido." });
        if (dto.Rol == "Superadministrador" && !User.IsInRole("Superadministrador")) return Forbid();

        var usuario = await usuarios.FindByIdAsync(id);
        if (usuario is null) return NotFound();
        var rolesActuales = await usuarios.GetRolesAsync(usuario);
        if (rolesActuales.Contains("Superadministrador") && !User.IsInRole("Superadministrador")) return Forbid();
        if (rolesActuales.Count == 1 && rolesActuales[0] == dto.Rol) return Ok();

        await using var transaccion = await db.Database.BeginTransactionAsync();
        if (rolesActuales.Count > 0)
        {
            var quitar = await usuarios.RemoveFromRolesAsync(usuario, rolesActuales);
            if (!quitar.Succeeded) return BadRequest(new { mensaje = "No se pudo actualizar el rol." });
        }
        var agregar = await usuarios.AddToRoleAsync(usuario, dto.Rol);
        if (!agregar.Succeeded) return BadRequest(new { mensaje = "No se pudo actualizar el rol." });
        await transaccion.CommitAsync();
        return Ok(new { mensaje = "Rol actualizado." });
    }
}
