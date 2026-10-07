using GestionTubos.Application.Interfaces;
using GestionTubos.Shared.Operaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionTubos.Api.Controllers;

[ApiController, Route("api/operaciones")]
[Authorize]
public sealed class OperacionesController(IOperacionService service, IClienteService clientes, GestionTubos.Api.Services.RemitoPdfGenerator pdf) : ControllerBase
{
    [HttpGet("movimientos")] public async Task<IActionResult> Movimientos([FromQuery] int? cilindroId, CancellationToken ct) => Ok(await service.MovimientosAsync(cilindroId, ct));
    [HttpGet("pruebas-hidraulicas")] public async Task<IActionResult> Pruebas(CancellationToken ct) => Ok(await service.PruebasAsync(ct));
    [HttpPost("pruebas-hidraulicas"), Authorize(Roles = "Superadministrador,Administrador")] public Task<IActionResult> RegistrarPrueba(PruebaHidraulicaGuardarDto dto, CancellationToken ct) => Ejecutar(async () => Ok(await service.RegistrarPruebaAsync(dto, ct)));
    [HttpGet] public async Task<IActionResult> Operaciones([FromQuery] bool soloAbiertas, CancellationToken ct) => Ok(await service.OperacionesAsync(soloAbiertas, ct));
    [HttpPost("entregas")] public Task<IActionResult> Entregar(EntregaGuardarDto dto, CancellationToken ct) => Ejecutar(async () => Ok(await service.EntregarAsync(dto, ct)));
    [HttpPost("recepciones")] public Task<IActionResult> Recibir(RecepcionGuardarDto dto, CancellationToken ct) => Ejecutar(async () => Ok(await service.RecibirAsync(dto, User.Identity?.Name ?? "Usuario", ct)));
    [HttpGet("remitos")] public async Task<IActionResult> Remitos([FromQuery] int? clienteId, [FromQuery] int? tuboId, [FromQuery] string? tipo, CancellationToken ct) => Ok(await service.RemitosAsync(clienteId, tuboId, tipo, ct));
    [HttpPost("devoluciones-lote")] public Task<IActionResult> CrearDevolucion(DevolucionLoteCrearDto dto,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.CrearDevolucionLoteAsync(dto,User.Identity?.Name??"Usuario",ct)));
    [HttpGet("devoluciones-lote/{id:long}")] public async Task<IActionResult> ObtenerDevolucion(long id,CancellationToken ct){var x=await service.ObtenerDevolucionLoteAsync(id,ct);return x is null?NotFound():Ok(x);}
    [HttpGet("devoluciones-lote/{id:long}/pdf")] public async Task<IActionResult> DescargarDevolucionPdf(long id,CancellationToken ct){var x=await service.ObtenerDevolucionLoteAsync(id,ct);if(x is null)return NotFound();var cliente=await clientes.ObtenerPorIdAsync(x.ClienteId,ct);var documento=cliente?.Tipo==2?$"CUIT: {cliente.CuitCuil??"Sin registrar"}":$"DNI: {cliente?.Dni??"Sin registrar"}";return File(pdf.Devolucion(x,documento),"application/pdf",$"{x.Numero}.pdf");}
    [HttpPost("devoluciones-lote/{id:long}/firma")] public Task<IActionResult> FirmarDevolucion(long id,DevolucionLoteFirmarDto dto,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.FirmarDevolucionLoteAsync(id,dto,User.Identity?.Name??"Usuario",ct)));
    [HttpGet("dashboard")] public async Task<IActionResult> Dashboard(CancellationToken ct) => Ok(await service.DashboardAsync(ct));
    [HttpPost("cargas/iniciar")] public Task<IActionResult> IniciarCarga(CargaIniciarDto dto,CancellationToken ct)=>Ejecutar(async()=>{await service.IniciarCargaAsync(dto,User.Identity?.Name??"Usuario",ct);return Ok();});
    [HttpPost("cargas/finalizar")] public Task<IActionResult> FinalizarCarga(CargaFinalizarDto dto,CancellationToken ct)=>Ejecutar(async()=>{await service.FinalizarCargaAsync(dto,User.Identity?.Name??"Usuario",ct);return Ok();});
    [HttpPost("cargas/cancelar")] public Task<IActionResult> CancelarCarga(CargaCancelarDto dto,CancellationToken ct)=>Ejecutar(async()=>{await service.CancelarCargaAsync(dto,User.Identity?.Name??"Usuario",ct);return Ok();});
    [HttpPost("ingresos-cliente")] public Task<IActionResult> IngresoCliente(IngresoCilindroClienteDto dto,CancellationToken ct)=>Ejecutar(async()=>{await service.IngresarCilindroClienteAsync(dto,User.Identity?.Name??"Usuario",ct);return Ok();});
    private static async Task<IActionResult> Ejecutar(Func<Task<IActionResult>> f) { try { return await f(); } catch (InvalidOperationException e) { return new ConflictObjectResult(new { mensaje = e.Message }); } }
}
