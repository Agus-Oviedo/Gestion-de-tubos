using System.Text.Json;
using GestionTubos.Domain.Entities;
using GestionTubos.Domain.Enums;
using GestionTubos.Shared;
using GestionTubos.Shared.Pedidos;
using Microsoft.EntityFrameworkCore;
namespace GestionTubos.Infrastructure.Services;
public sealed partial class PedidoService
{
 private async Task<PedidoDto?> Foto(long id,CancellationToken ct)=>(await ObtenerAsync(ct)).SingleOrDefault(x=>x.Id==id);
 private static Dictionary<string,string?> Campos(PedidoDto? x)=>x is null?[]:new(){
  ["Cliente"]=x.Cliente+" (#"+x.ClienteId+")",["Tubos"]=string.Join(", ",x.Cilindros.OrderBy(t=>t.CilindroId).Select(t=>t.Codigo+" (#"+t.CilindroId+")")),
  ["Lugar de entrega"]=x.ModalidadEntrega,["Dirección"]=x.DireccionEntregaTexto,["Fecha prevista"]=x.FechaPrevistaEntrega?.ToString("dd/MM/yyyy"),
  ["Contacto"]=x.ContactoEntrega,["Teléfono"]=x.TelefonoEntrega,["Descripción / observaciones"]=x.Observaciones,["Estado"]=x.Estado,
  ["Persona que recibe"]=x.PersonaRecibe,["DNI de quien recibe"]=x.DniRecibe,["Firma"]=x.FechaFirmaUtc.HasValue?$"Registrada el {x.FechaFirmaUtc.Value.EnArgentina():dd/MM/yyyy HH:mm}":"Pendiente"};
 private async Task Registrar(PedidoDto? antes,PedidoDto? despues,string accion,string motivo,string usuario,int version,CancellationToken ct,CambioRemitoDto? cambioExtra=null)
 {
  var referencia=despues??antes??throw new InvalidOperationException("Falta el remito a auditar.");
  var a=Campos(antes);var d=Campos(despues);var cambios=a.Keys.Union(d.Keys).Where(k=>a.GetValueOrDefault(k)!=d.GetValueOrDefault(k)).Select(k=>new CambioRemitoDto(k,a.GetValueOrDefault(k),d.GetValueOrDefault(k))).ToList();
  if(cambioExtra is not null)cambios.Add(cambioExtra);
  db.AuditoriaRemitos.Add(new(){PedidoId=referencia.Id,NumeroRemito=referencia.NumeroRemito??referencia.Numero,Version=version,UsuarioId=actor?.Id??"Sistema",Usuario=usuario,Accion=accion,Motivo=motivo,AntesJson=antes is null?null:JsonSerializer.Serialize(antes),DespuesJson=despues is null?null:JsonSerializer.Serialize(despues),CambiosJson=JsonSerializer.Serialize(cambios)});
  await db.SaveChangesAsync(ct);
 }
 internal async Task RegistrarAccionExternaAsync(PedidoDto antes,string accion,string motivo,string usuario,string repartoAnterior,string repartoNuevo,CancellationToken ct)
 {
  var remito=await db.Remitos.SingleAsync(x=>x.PedidoId==antes.Id,ct);remito.Version++;await db.SaveChangesAsync(ct);
  await Registrar(antes,await Foto(antes.Id,ct),accion,motivo,usuario,remito.Version,ct,new("Reparto",repartoAnterior,repartoNuevo));
 }
 public async Task RegistrarEstadosInicialesAsync(CancellationToken ct)
 {
  foreach(var p in await ObtenerAsync(ct)){
   if(p.NumeroRemito is null||await db.AuditoriaRemitos.AnyAsync(x=>x.PedidoId==p.Id,ct))continue;
   await Registrar(null,p,"Estado inicial","Remito existente al habilitar la auditoría; los cambios anteriores no se reconstruyen.","Sistema",p.Version,ct);
  }
 }
 public async Task<PaginaAuditoriaDto> AuditoriaAsync(long? pedidoId,string? buscar,long? anteriorA,CancellationToken ct)
 {
  var q=db.AuditoriaRemitos.AsNoTracking().AsQueryable();if(pedidoId.HasValue)q=q.Where(x=>x.PedidoId==pedidoId);if(anteriorA.HasValue)q=q.Where(x=>x.Id<anteriorA);
  if(!string.IsNullOrWhiteSpace(buscar)){var s=buscar.Trim();q=q.Where(x=>x.NumeroRemito.Contains(s)||x.Usuario.Contains(s)||x.Motivo.Contains(s)||x.Accion.Contains(s));}
  var lista=await q.OrderByDescending(x=>x.Id).Take(21).ToListAsync(ct);
  return new(lista.Take(20).Select(x=>new AuditoriaRemitoDto(x.Id,x.PedidoId,x.NumeroRemito,x.Version,x.FechaUtc,x.UsuarioId,x.Usuario,x.Accion,x.Motivo,JsonSerializer.Deserialize<List<CambioRemitoDto>>(x.CambiosJson)??[])).ToList(),lista.Count>20);
 }
 public async Task<PedidoDto> EditarAsync(long id,RemitoEditarDto dto,string usuario,CancellationToken ct)
 {
  var errores=new List<System.ComponentModel.DataAnnotations.ValidationResult>();
  if(!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(dto,new(dto),errores,true))throw new InvalidOperationException(errores.First().ErrorMessage);
  if(dto.CilindroIds.Distinct().Count()!=dto.CilindroIds.Count)throw new InvalidOperationException("Hay tubos repetidos.");
  await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
  var p=await db.Pedidos.Include(x=>x.Remito).Include(x=>x.Detalles).ThenInclude(x=>x.Tubo).SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new InvalidOperationException("El remito no existe.");
  if(p.Remito is null)throw new InvalidOperationException("El pedido no tiene remito.");
  if(p.Remito.FechaFirmaUtc.HasValue||p.Remito.Estado=="Firmado"||p.Estado=="Entregado")throw new InvalidOperationException("Un remito firmado no puede editarse ni anularse.");
  if(p.Remito.Version!=dto.VersionEsperada)throw new InvalidOperationException("Otra persona modificó el remito. Actualizá la pantalla antes de editar.");
  if(p.Estado!="Preparado"||await db.RepartoDetalles.AnyAsync(x=>x.PedidoId==id,ct))throw new InvalidOperationException("Quitá primero el remito del reparto para editarlo.");
  if(p.Detalles.Any(x=>x.Tubo.EstadoDisponibilidad!=EstadoDisponibilidad.Reservado))throw new InvalidOperationException("El estado de los tubos cambió. Actualizá la pantalla.");
  if(!await db.Clientes.AnyAsync(x=>x.Id==dto.ClienteId&&x.Activo,ct))throw new InvalidOperationException("Seleccioná un cliente activo.");
  DireccionCliente? direccion=null;
  if(dto.ModalidadEntrega=="Domicilio")direccion=await db.DireccionesCliente.SingleOrDefaultAsync(x=>x.Id==dto.DireccionEntregaId&&x.ClienteId==dto.ClienteId&&x.Activa,ct)??throw new InvalidOperationException("La dirección no pertenece al cliente.");
  var originales=p.Detalles.Select(x=>x.TuboId).ToHashSet();var seleccion=dto.CilindroIds.ToHashSet();
  var tubos=await db.Tubos.Where(x=>seleccion.Contains(x.Id)).ToListAsync(ct);
  if(tubos.Count!=seleccion.Count||tubos.Any(x=>!x.Activo||(!originales.Contains(x.Id)&&x.EstadoDisponibilidad!=EstadoDisponibilidad.Disponible)||(x.TipoPropietario==TipoPropietarioTubo.Empresa&&x.EstadoContenido!=EstadoContenido.Lleno)||(x.TipoPropietario==TipoPropietarioTubo.Cliente&&x.ClientePropietarioId!=dto.ClienteId)))throw new InvalidOperationException("Algún tubo no está disponible o no corresponde al cliente.");
  var antes=(await Foto(id,ct))!;
  foreach(var detalle in p.Detalles.Where(x=>!seleccion.Contains(x.TuboId)).ToList()){detalle.Tubo.EstadoDisponibilidad=EstadoDisponibilidad.Disponible;db.PedidoDetalles.Remove(detalle);p.Detalles.Remove(detalle);}
  foreach(var tubo in tubos.Where(x=>!originales.Contains(x.Id))){tubo.EstadoDisponibilidad=EstadoDisponibilidad.Reservado;p.Detalles.Add(new(){TuboId=tubo.Id});}
  p.ClienteId=dto.ClienteId;p.ModalidadEntrega=dto.ModalidadEntrega;p.DireccionEntregaId=direccion?.Id;p.DireccionEntregaTexto=direccion is null?null:$"{direccion.Calle} {direccion.Numero}, {direccion.Localidad}, {direccion.Provincia}".Replace("  "," ");
  p.FechaPrevistaEntrega=dto.FechaPrevistaEntrega;p.ContactoEntrega=Limpiar(dto.ContactoEntrega);p.TelefonoEntrega=TelefonoWhatsApp.Normalizar(dto.TelefonoEntrega);p.Observaciones=Limpiar(dto.Observaciones);
  p.Remito.Version++;await db.SaveChangesAsync(ct);var despues=(await Foto(id,ct))!;
  if(Campos(antes).All(kv=>Campos(despues).GetValueOrDefault(kv.Key)==kv.Value))throw new InvalidOperationException("No hay cambios para guardar.");
  await Registrar(antes,despues,"Edición",dto.Motivo.Trim(),usuario,p.Remito.Version,ct);await tx.CommitAsync(ct);return despues;
 }
}
