using GestionTubos.Infrastructure.Services;
using GestionTubos.Shared.Operaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GestionTubos.Api.Controllers;
[ApiController,Route("api/planchadas"),Authorize]
public sealed class PlanchadasController(PlanchadaService service):ControllerBase
{
 [HttpGet("sectores")]public async Task<IActionResult>Sectores(CancellationToken ct)=>Ok(await service.Sectores(ct));
 [HttpGet("activos")]public async Task<IActionResult>Activos(int?sectorId,CancellationToken ct)=>Ok(await service.Activos(sectorId,ct));
 [HttpGet("candidatos")]public Task<IActionResult>Candidatos(int sectorId,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.Candidatos(sectorId,ct)));
 [HttpPost("ingresos")]public Task<IActionResult>Ingresar(BateriaIngresoDto dto,CancellationToken ct)=>Ejecutar(async()=>{await service.Ingresar(dto,User.Identity?.Name??"Usuario",ct);return NoContent();});
 [HttpPost("retiros")]public Task<IActionResult>Retirar(BateriaRetiroDto dto,CancellationToken ct)=>Ejecutar(async()=>{await service.Retirar(dto,User.Identity?.Name??"Usuario",ct);return NoContent();});
 [HttpGet("consumo")]public Task<IActionResult>Consumo(DateOnly desde,DateOnly hasta,int?sectorId,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.Consumo(desde,hasta,sectorId,ct)));
 [HttpGet("historial")]public Task<IActionResult>Historial(DateOnly desde,DateOnly hasta,int?sectorId,long?anteriorA,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.Historial(desde,hasta,sectorId,anteriorA,ct)));
 private static async Task<IActionResult>Ejecutar(Func<Task<IActionResult>> f){try{return await f();}catch(InvalidOperationException e){return new ConflictObjectResult(new{mensaje=e.Message});}}
}
