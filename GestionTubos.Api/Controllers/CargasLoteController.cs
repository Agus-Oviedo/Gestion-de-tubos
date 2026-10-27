using GestionTubos.Infrastructure.Services;
using GestionTubos.Shared.Operaciones;
using GestionTubos.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GestionTubos.Api.Controllers;
[ApiController,Route("api/cargas-lote"),Authorize]
public sealed class CargasLoteController(CargaLoteService service,RemitoPdfGenerator pdf):ControllerBase
{
 string Usuario=>User.Identity?.Name??"Usuario";
 [HttpGet("vacios")]public async Task<IActionResult> Vacios(CancellationToken ct)=>Ok(await service.Vacios(ct));
 [HttpGet]public async Task<IActionResult> Envios(CancellationToken ct)=>Ok(await service.Envios(ct));
 [HttpGet("{id:long}")]public async Task<IActionResult> Obtener(long id,CancellationToken ct){var e=await service.Obtener(id,ct);return e is null?NotFound():Ok(e);}
 [HttpPost]public Task<IActionResult> Crear(EnvioCargaCrearDto dto,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.Crear(dto,Usuario,ct)));
 [HttpPost("confirmar-envio")]public Task<IActionResult> ConfirmarEnvio(EnvioCargaCrearDto dto,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.Crear(dto,Usuario,ct,true)));
 [HttpPost("{id:long}/salida")]public Task<IActionResult> Salida(long id,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.Salida(id,Usuario,ct)));
 [HttpPost("{id:long}/editar")]public Task<IActionResult> Editar(long id,EnvioCargaCrearDto dto,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.ActualizarPendiente(id,dto,ct)));
 [HttpPost("{id:long}/cancelar")]public Task<IActionResult> Cancelar(long id,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.Cancelar(id,ct)));
 [HttpPost("{id:long}/recepciones")]public Task<IActionResult> Recibir(long id,RecepcionCargaCrearDto dto,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.Recibir(id,dto,Usuario,ct)));
 [HttpPost("{id:long}/faltante")]public Task<IActionResult> Resolver(long id,ResolucionCargaDto dto,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.Resolver(id,dto,Usuario,ct)));
 [HttpGet("{id:long}/pdf")]public async Task<IActionResult> EnvioPdf(long id,CancellationToken ct){var e=await service.Obtener(id,ct);if(e is null)return NotFound();return File(pdf.Carga("ENVÍO A CARGA",e.Numero,e.FechaSalidaUtc??e.FechaCreacionUtc,e.Destino,e.Transporte,e.Estado,e.Tubos.Select(t=>$"{t.Codigo} | {t.Gas} | {t.Capacidad} | {t.Propietario.Replace("|","/")}").ToList(),e.Observaciones,e.Usuario,true),"application/pdf",e.Numero+".pdf");}
 [HttpGet("{id:long}/recepciones/{recepcionId:long}/pdf")]public async Task<IActionResult> RecepcionPdf(long id,long recepcionId,CancellationToken ct){var e=await service.Obtener(id,ct);var r=e?.Recepciones.SingleOrDefault(x=>x.Id==recepcionId);if(e is null||r is null)return NotFound();return File(pdf.Carga("RECEPCIÓN DE CARGA",r.Numero,r.FechaUtc,e.Destino,e.Transporte,"Envío original: "+e.Numero,r.Tubos.Select(t=>$"{t.Codigo} | {t.Gas} | {t.Capacidad} | {t.Propietario.Replace("|","/")} | {(t.Resultado=="Danado"?"Dañado":t.Resultado=="Vacio"?"Vacío / rechazado":"Lleno")}").ToList(),r.Observaciones,r.Usuario,true),"application/pdf",r.Numero+".pdf");}
 static async Task<IActionResult> Ejecutar(Func<Task<IActionResult>> f){try{return await f();}catch(InvalidOperationException ex){return new ConflictObjectResult(new{Mensaje=ex.Message});}}
}
