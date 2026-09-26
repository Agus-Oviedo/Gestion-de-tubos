using GestionTubos.Application.Interfaces;
using GestionTubos.Shared.Operaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionTubos.Api.Controllers;

[ApiController, Route("api/operaciones")]
[Authorize]
public sealed class OperacionesController(IOperacionService service) : ControllerBase
{
    [HttpGet("movimientos")] public async Task<IActionResult> Movimientos([FromQuery] int? cilindroId, CancellationToken ct) => Ok(await service.MovimientosAsync(cilindroId, ct));
    [HttpGet("pruebas-hidraulicas")] public async Task<IActionResult> Pruebas(CancellationToken ct) => Ok(await service.PruebasAsync(ct));
    [HttpPost("pruebas-hidraulicas"), Authorize(Roles = "Superadministrador,Administrador")] public Task<IActionResult> RegistrarPrueba(PruebaHidraulicaGuardarDto dto, CancellationToken ct) => Ejecutar(async () => Ok(await service.RegistrarPruebaAsync(dto, ct)));
    [HttpGet] public async Task<IActionResult> Operaciones([FromQuery] bool soloAbiertas, CancellationToken ct) => Ok(await service.OperacionesAsync(soloAbiertas, ct));
    [HttpPost("entregas")] public Task<IActionResult> Entregar(EntregaGuardarDto dto, CancellationToken ct) => Ejecutar(async () => Ok(await service.EntregarAsync(dto, ct)));
    [HttpPost("recepciones")] public Task<IActionResult> Recibir(RecepcionGuardarDto dto, CancellationToken ct) => Ejecutar(async () => Ok(await service.RecibirAsync(dto, ct)));
    [HttpGet("dashboard")] public async Task<IActionResult> Dashboard(CancellationToken ct) => Ok(await service.DashboardAsync(ct));
    [HttpPost("cargas/iniciar")] public Task<IActionResult> IniciarCarga(CargaIniciarDto dto,CancellationToken ct)=>Ejecutar(async()=>{await service.IniciarCargaAsync(dto,User.Identity?.Name??"Usuario",ct);return Ok();});
    [HttpPost("cargas/finalizar")] public Task<IActionResult> FinalizarCarga(CargaFinalizarDto dto,CancellationToken ct)=>Ejecutar(async()=>{await service.FinalizarCargaAsync(dto,User.Identity?.Name??"Usuario",ct);return Ok();});
    [HttpPost("cargas/cancelar")] public Task<IActionResult> CancelarCarga(CargaCancelarDto dto,CancellationToken ct)=>Ejecutar(async()=>{await service.CancelarCargaAsync(dto,User.Identity?.Name??"Usuario",ct);return Ok();});
    [HttpPost("ingresos-cliente")] public Task<IActionResult> IngresoCliente(IngresoCilindroClienteDto dto,CancellationToken ct)=>Ejecutar(async()=>{await service.IngresarCilindroClienteAsync(dto,User.Identity?.Name??"Usuario",ct);return Ok();});
    private static async Task<IActionResult> Ejecutar(Func<Task<IActionResult>> f) { try { return await f(); } catch (InvalidOperationException e) { return new ConflictObjectResult(new { mensaje = e.Message }); } }
}
