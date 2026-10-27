using GestionTubos.Application.Interfaces;
using GestionTubos.Infrastructure.Data;
using GestionTubos.Shared.Repartos;
using Microsoft.EntityFrameworkCore;
namespace GestionTubos.Infrastructure.Services;
public sealed class RepartoServiceAuditado(GestionTubosDbContext db,IUsuarioActual? actor=null):IRepartoService
{
 private RepartoService Interno=>new(db);
 public Task<IReadOnlyList<RepartoDto>> ObtenerAsync(CancellationToken ct)=>Interno.ObtenerAsync(ct);
 private async Task<RepartoDto> Ejecutar(IReadOnlyList<long> ids,string accion,string motivo,string usuario,string anterior,Func<Task<RepartoDto>> f,CancellationToken ct)
 {
  await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
  if(await db.Remitos.AnyAsync(x=>ids.Contains(x.PedidoId)&&(x.FechaFirmaUtc!=null||x.Estado=="Firmado"),ct))throw new InvalidOperationException("Un remito firmado no puede modificarse ni anularse.");
  var pedidos=new PedidoService(db,actor);var antes=(await pedidos.ObtenerAsync(ct)).Where(x=>ids.Contains(x.Id)).ToList();
  var resultado=await f();foreach(var p in antes)await pedidos.RegistrarAccionExternaAsync(p,accion,motivo,usuario,anterior,$"{resultado.Numero} · {resultado.Estado}",ct);
  await tx.CommitAsync(ct);return resultado;
 }
 public Task<RepartoDto> CrearAsync(RepartoCrearDto dto,string usuario,CancellationToken ct)=>Ejecutar(dto.PedidoIds,"Asignación a reparto","Asignación del remito a reparto",usuario,"Sin reparto",()=>Interno.CrearAsync(dto,usuario,ct),ct);
 public async Task<RepartoDto> IniciarAsync(long id,string usuario,CancellationToken ct){var r=(await ObtenerAsync(ct)).SingleOrDefault(x=>x.Id==id)??throw new InvalidOperationException("El reparto no existe.");return await Ejecutar(r.Pedidos.Select(x=>x.PedidoId).ToList(),"Salida a reparto","Inicio del reparto",usuario,$"{r.Numero} · {r.Estado}",()=>Interno.IniciarAsync(id,usuario,ct),ct);}
 public async Task<RepartoDto> AnularAsync(long id,RepartoAnularDto dto,string usuario,CancellationToken ct){var r=(await ObtenerAsync(ct)).SingleOrDefault(x=>x.Id==id)??throw new InvalidOperationException("El reparto no existe.");return await Ejecutar(r.Pedidos.Select(x=>x.PedidoId).ToList(),"Reparto anulado",dto.Motivo,usuario,$"{r.Numero} · {r.Estado}",()=>Interno.AnularAsync(id,dto,usuario,ct),ct);}
 public async Task<RepartoDto> NoEntregadoAsync(long id,long pedidoId,RepartoNoEntregadoDto dto,string usuario,CancellationToken ct){var r=(await ObtenerAsync(ct)).SingleOrDefault(x=>x.Id==id)??throw new InvalidOperationException("El reparto no existe.");return await Ejecutar([pedidoId],dto.Accion=="CancelarPedido"?"Anulación sin entrega":"Reprogramación",dto.Motivo,usuario,$"{r.Numero} · {r.Estado}",()=>Interno.NoEntregadoAsync(id,pedidoId,dto,usuario,ct),ct);}
}
