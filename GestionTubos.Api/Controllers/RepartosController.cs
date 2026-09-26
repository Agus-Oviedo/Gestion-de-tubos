using GestionTubos.Application.Interfaces;using GestionTubos.Shared.Repartos;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;
namespace GestionTubos.Api.Controllers;
[ApiController,Route("api/repartos"),Authorize]public sealed class RepartosController(IRepartoService s):ControllerBase
{
 [HttpGet]public async Task<IActionResult>Obtener(CancellationToken ct)=>Ok(await s.ObtenerAsync(ct));
 [HttpPost]public Task<IActionResult>Crear(RepartoCrearDto d,CancellationToken ct)=>Ejecutar(async()=>Ok(await s.CrearAsync(d,User.Identity?.Name??"Usuario",ct)));
 [HttpPost("{id:long}/iniciar")]public Task<IActionResult>Iniciar(long id,CancellationToken ct)=>Ejecutar(async()=>Ok(await s.IniciarAsync(id,User.Identity?.Name??"Usuario",ct)));
 [HttpPost("{id:long}/anular")]public Task<IActionResult>Anular(long id,RepartoAnularDto d,CancellationToken ct)=>Ejecutar(async()=>Ok(await s.AnularAsync(id,d,User.Identity?.Name??"Usuario",ct)));
 [HttpPost("{id:long}/pedidos/{pedidoId:long}/no-entregado")]public Task<IActionResult>NoEntregado(long id,long pedidoId,RepartoNoEntregadoDto d,CancellationToken ct)=>Ejecutar(async()=>Ok(await s.NoEntregadoAsync(id,pedidoId,d,User.Identity?.Name??"Usuario",ct)));
 private static async Task<IActionResult>Ejecutar(Func<Task<IActionResult>>f){try{return await f();}catch(InvalidOperationException e){return new ConflictObjectResult(new{mensaje=e.Message});}}
}
