using GestionTubos.Application.Interfaces;
using GestionTubos.Shared.Pedidos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionTubos.Api.Controllers;
[ApiController,Route("api/pedidos"),Authorize]
public sealed class PedidosController(IPedidoService service, IClienteService clientes, GestionTubos.Api.Services.RemitoPdfGenerator pdf):ControllerBase
{
    [HttpGet] public async Task<IActionResult> Obtener(CancellationToken ct)=>Ok(await service.ObtenerAsync(ct));
    [HttpPost] public Task<IActionResult> Crear(PedidoCrearDto dto,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.CrearAsync(dto,User.Identity?.Name??"Usuario",ct)));
    [HttpPost("{id:long}/remito")] public Task<IActionResult> Emitir(long id,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.EmitirRemitoAsync(id,User.Identity?.Name??"Usuario",ct)));
    [HttpPost("{id:long}/entrega")] public Task<IActionResult> Entregar(long id,PedidoEntregarDto dto,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.ConfirmarEntregaAsync(id,dto,User.Identity?.Name??"Usuario",ct)));
    [HttpPut("{id:long}/remito"),Authorize(Roles="Superadministrador,Administrador")] public Task<IActionResult> Editar(long id,RemitoEditarDto dto,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.EditarAsync(id,dto,User.Identity?.Name??"Usuario",ct)));
    [HttpGet("auditoria"),Authorize(Roles="Superadministrador,Administrador")] public async Task<IActionResult> Auditoria([FromQuery]long? pedidoId,[FromQuery]string? buscar,[FromQuery]long? anteriorA,CancellationToken ct)=>Ok(await service.AuditoriaAsync(pedidoId,buscar,anteriorA,ct));
    [HttpDelete("{id:long}/remito"), Authorize(Roles="Superadministrador,Administrador")] public Task<IActionResult> EliminarRemito(long id,[FromBody]RemitoEliminarDto dto,CancellationToken ct)=>Ejecutar(async()=>{await service.EliminarRemitoAsync(id,ct,User.Identity?.Name??"Usuario",dto.Motivo);return NoContent();});
    [HttpGet("{id:long}/pdf")] public async Task<IActionResult> DescargarPdf(long id,CancellationToken ct){var remito=(await service.ObtenerAsync(ct)).FirstOrDefault(x=>x.Id==id);if(remito is null)return NotFound();var cliente=await clientes.ObtenerPorIdAsync(remito.ClienteId,ct);var documento=cliente?.Tipo==2?$"CUIT: {cliente.CuitCuil??"Sin registrar"}":$"DNI: {cliente?.Dni??"Sin registrar"}";return File(pdf.Entrega(remito,documento),"application/pdf",$"{remito.NumeroRemito??remito.Numero}.pdf");}
    private static async Task<IActionResult> Ejecutar(Func<Task<IActionResult>> f){try{return await f();}catch(Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException){return new ConflictObjectResult(new{mensaje="El remito cambió mientras trabajabas. Actualizá la pantalla."});}catch(InvalidOperationException e){return new ConflictObjectResult(new{mensaje=e.Message});}}
}
