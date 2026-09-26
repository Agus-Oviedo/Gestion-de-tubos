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

namespace GestionTubos.Api.Controllers;

[ApiController, Route("api/autenticacion")]
public sealed class AutenticacionController(UserManager<UsuarioAplicacion> usuarios, IConfiguration config) : ControllerBase
{
    [AllowAnonymous, HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var usuario = await usuarios.FindByEmailAsync(dto.Correo);
        if (usuario is null || !usuario.Activo || !await usuarios.CheckPasswordAsync(usuario, dto.Contrasena)) return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
        var roles = await usuarios.GetRolesAsync(usuario); var expira = DateTime.UtcNow.AddHours(config.GetValue("Jwt:HorasExpiracion", 8));
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
}
