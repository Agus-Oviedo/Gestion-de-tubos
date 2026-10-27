using GestionTubos.Infrastructure.Services;using GestionTubos.Shared.Operaciones;using GestionTubos.Api.Services;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;
namespace GestionTubos.Api.Controllers;
[ApiController,Route("api/ingresos-clientes"),Authorize]
public sealed class IngresosClientesController(IngresoClienteService service,RemitoPdfGenerator pdf):ControllerBase
{
 [HttpPost]public async Task<IActionResult>Crear(IngresoClienteCrearDto dto,CancellationToken ct){try{return Ok(await service.Recibir(dto,User.Identity?.Name??"Usuario",ct));}catch(InvalidOperationException e){return Conflict(new{mensaje=e.Message});}}
 [HttpGet("{id:long}")]public async Task<IActionResult>Obtener(long id,CancellationToken ct){var x=await service.Obtener(id,ct);return x is null?NotFound():Ok(x);}
 [HttpGet("{id:long}/pdf")]public async Task<IActionResult>Pdf(long id,CancellationToken ct){var x=await service.Obtener(id,ct);return x is null?NotFound():File(pdf.IngresoCliente(x),"application/pdf",x.Numero+".pdf");}
}
