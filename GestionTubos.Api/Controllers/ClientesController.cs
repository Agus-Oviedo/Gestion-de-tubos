using GestionTubos.Application.Interfaces;
using GestionTubos.Shared.Clientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionTubos.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize]
public sealed class ClientesController(IClienteService clienteService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClienteDto>>> ObtenerTodos(
        [FromQuery] string? buscar, CancellationToken cancellationToken) =>
        Ok(await clienteService.ObtenerTodosAsync(buscar, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var cliente = await clienteService.ObtenerPorIdAsync(id, cancellationToken);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpPost, Authorize(Roles = "Superadministrador,Administrador")]
    public async Task<ActionResult<ClienteDto>> Crear(ClienteGuardarDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var cliente = await clienteService.CrearAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = cliente.Id }, cliente);
        }
        catch (InvalidOperationException ex) { return Conflict(new { mensaje = ex.Message }); }
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Superadministrador,Administrador")]
    public async Task<ActionResult<ClienteDto>> Actualizar(int id, ClienteGuardarDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var cliente = await clienteService.ActualizarAsync(id, dto, cancellationToken);
            return cliente is null ? NotFound() : Ok(cliente);
        }
        catch (InvalidOperationException ex) { return Conflict(new { mensaje = ex.Message }); }
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Superadministrador,Administrador")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken) =>
        await clienteService.EliminarAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpGet("{id:int}/direcciones")]
    public async Task<ActionResult<IReadOnlyList<DireccionClienteDto>>> ObtenerDirecciones(int id, CancellationToken cancellationToken)
    {
        var direcciones = await clienteService.ObtenerDireccionesAsync(id, cancellationToken);
        return direcciones is null ? NotFound() : Ok(direcciones);
    }

    [HttpPost("{id:int}/direcciones"), Authorize(Roles = "Superadministrador,Administrador")]
    public async Task<ActionResult<DireccionClienteDto>> CrearDireccion(
        int id, DireccionClienteGuardarDto dto, CancellationToken cancellationToken)
    {
        var direccion = await clienteService.CrearDireccionAsync(id, dto, cancellationToken);
        return direccion is null ? NotFound() : Ok(direccion);
    }

    [HttpDelete("{id:int}/direcciones/{direccionId:int}"), Authorize(Roles = "Superadministrador,Administrador")]
    public async Task<IActionResult> EliminarDireccion(int id, int direccionId, CancellationToken cancellationToken) =>
        await clienteService.EliminarDireccionAsync(id, direccionId, cancellationToken) ? NoContent() : NotFound();
}
