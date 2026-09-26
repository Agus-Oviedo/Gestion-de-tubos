using GestionTubos.Application.Interfaces;
using GestionTubos.Shared.Pedidos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionTubos.Api.Controllers;
[ApiController,Route("api/pedidos"),Authorize]
public sealed class PedidosController(IPedidoService service):ControllerBase
{
    [HttpGet] public async Task<IActionResult> Obtener(CancellationToken ct)=>Ok(await service.ObtenerAsync(ct));
    [HttpPost] public Task<IActionResult> Crear(PedidoCrearDto dto,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.CrearAsync(dto,User.Identity?.Name??"Usuario",ct)));
    [HttpPost("{id:long}/remito")] public Task<IActionResult> Emitir(long id,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.EmitirRemitoAsync(id,User.Identity?.Name??"Usuario",ct)));
    [HttpPost("{id:long}/entrega")] public Task<IActionResult> Entregar(long id,PedidoEntregarDto dto,CancellationToken ct)=>Ejecutar(async()=>Ok(await service.ConfirmarEntregaAsync(id,dto,User.Identity?.Name??"Usuario",ct)));
    private static async Task<IActionResult> Ejecutar(Func<Task<IActionResult>> f){try{return await f();}catch(InvalidOperationException e){return new ConflictObjectResult(new{mensaje=e.Message});}}
}
