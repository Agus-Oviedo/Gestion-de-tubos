using GestionTubos.Application.Interfaces;
using GestionTubos.Shared.Catalogos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionTubos.Api.Controllers;

[ApiController]
[Route("api/catalogos")]
[Authorize]
public sealed class CatalogosController(ICatalogoService service) : ControllerBase
{
    [HttpGet("gases")] public async Task<IActionResult> Gases(CancellationToken ct)=>Ok(await service.ObtenerGasesAsync(ct));
    [HttpPost("gases"), Authorize(Roles = "Superadministrador")] public async Task<IActionResult> CrearGas(TipoGasGuardarDto dto,CancellationToken ct)=>await Ejecutar(()=>service.CrearGasAsync(dto,ct));
    [HttpDelete("gases/{id:int}"), Authorize(Roles = "Superadministrador")] public async Task<IActionResult> EliminarGas(int id,CancellationToken ct)=>await service.EliminarGasAsync(id,ct)?NoContent():NotFound();
    [HttpGet("capacidades")] public async Task<IActionResult> Capacidades(CancellationToken ct)=>Ok(await service.ObtenerCapacidadesAsync(ct));
    [HttpPost("capacidades"), Authorize(Roles = "Superadministrador")] public async Task<IActionResult> CrearCapacidad(CapacidadGuardarDto dto,CancellationToken ct)=>await Ejecutar(()=>service.CrearCapacidadAsync(dto,ct));
    [HttpDelete("capacidades/{id:int}"), Authorize(Roles = "Superadministrador")] public async Task<IActionResult> EliminarCapacidad(int id,CancellationToken ct)=>await service.EliminarCapacidadAsync(id,ct)?NoContent():NotFound();
    [HttpGet("ubicaciones")] public async Task<IActionResult> Ubicaciones(CancellationToken ct)=>Ok(await service.ObtenerUbicacionesAsync(ct));
    [HttpPost("ubicaciones"), Authorize(Roles = "Superadministrador")] public async Task<IActionResult> CrearUbicacion(UbicacionGuardarDto dto,CancellationToken ct)=>await Ejecutar(()=>service.CrearUbicacionAsync(dto,ct));
    [HttpPut("ubicaciones/{id:int}"), Authorize(Roles = "Superadministrador")] public async Task<IActionResult> EditarUbicacion(int id,UbicacionGuardarDto dto,CancellationToken ct)
    {
        try { var x=await service.ActualizarUbicacionAsync(id,dto,ct);return x is null?NotFound():Ok(x); }
        catch(InvalidOperationException ex){return Conflict(new{mensaje=ex.Message});}
    }
    [HttpDelete("ubicaciones/{id:int}"), Authorize(Roles = "Superadministrador")] public async Task<IActionResult> EliminarUbicacion(int id,CancellationToken ct)
    {
        try { return await service.EliminarUbicacionAsync(id,ct)?NoContent():NotFound(); }
        catch(InvalidOperationException ex){return Conflict(new{mensaje=ex.Message});}
    }

    private async Task<IActionResult> Ejecutar<T>(Func<Task<T>> accion)
    {
        try { return Ok(await accion()); }
        catch(InvalidOperationException ex){return Conflict(new{mensaje=ex.Message});}
    }
}
