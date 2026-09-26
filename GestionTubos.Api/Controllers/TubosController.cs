using GestionTubos.Application.Interfaces;using GestionTubos.Shared.Tubos;using Microsoft.AspNetCore.Mvc;using Microsoft.AspNetCore.Authorization;
namespace GestionTubos.Api.Controllers;
[ApiController][Route("api/cilindros")][Authorize]public sealed class TubosController(ITuboService s):ControllerBase
{
 [HttpPost("ubicacion")][Authorize(Roles="Superadministrador,Administrador")]public Task<IActionResult> Ubicar(TubosUbicarDto d,CancellationToken ct)=>Ejecutar(async()=>{await s.UbicarPropiosAsync(d,User.Identity?.Name??"Sistema",ct);return NoContent();});
 [HttpGet]public async Task<IActionResult> Todos([FromQuery]string? buscar,[FromQuery]int? clientePropietarioId,CancellationToken ct){var xs=await s.ObtenerTodosAsync(buscar,ct);return Ok(clientePropietarioId.HasValue?xs.Where(x=>x.ClientePropietarioId==clientePropietarioId):xs);}
 [HttpGet("{id:int}")]public async Task<IActionResult> Uno(int id,CancellationToken ct){var x=await s.ObtenerAsync(id,ct);return x is null?NotFound():Ok(x);}
 [HttpPost][Authorize(Roles="Superadministrador,Administrador")]public Task<IActionResult> Crear(TuboGuardarDto d,CancellationToken ct)=>Ejecutar(async()=>Created("",await s.CrearAsync(d,ct)));
 [HttpPut("{id:int}")][Authorize(Roles="Superadministrador,Administrador")]public Task<IActionResult> Editar(int id,TuboGuardarDto d,CancellationToken ct)=>Ejecutar(async()=>{var x=await s.ActualizarAsync(id,d,ct);return x is null?NotFound():Ok(x);});
 [HttpDelete("{id:int}")][Authorize(Roles="Superadministrador,Administrador")]public async Task<IActionResult> Eliminar(int id,CancellationToken ct)=>await s.EliminarAsync(id,ct)?NoContent():NotFound();
 private static async Task<IActionResult> Ejecutar(Func<Task<IActionResult>>f){try{return await f();}catch(InvalidOperationException e){return new ConflictObjectResult(new{mensaje=e.Message});}}
}
