using System.Data;
using GestionTubos.Domain.Entities;
using GestionTubos.Domain.Enums;
using GestionTubos.Infrastructure.Data;
using GestionTubos.Shared;
using GestionTubos.Shared.Operaciones;
using Microsoft.EntityFrameworkCore;
namespace GestionTubos.Infrastructure.Services;
public sealed class CargaLoteService(GestionTubosDbContext db)
{
 private IQueryable<EnvioCarga> Consulta()=>db.EnviosCarga.IgnoreQueryFilters().Include(x=>x.Detalles).ThenInclude(x=>x.Tubo).ThenInclude(x=>x.TipoGas).Include(x=>x.Detalles).ThenInclude(x=>x.Tubo).ThenInclude(x=>x.CapacidadTubo).Include(x=>x.Recepciones).ThenInclude(x=>x.Detalles);
 public async Task<List<EnvioCargaDto>> Envios(CancellationToken ct)=>(await Consulta().AsNoTracking().OrderByDescending(x=>x.Id).ToListAsync(ct)).Select(Mapear).ToList();
 public async Task<EnvioCargaDto?> Obtener(long id,CancellationToken ct){var e=await Consulta().AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id,ct);return e is null?null:Mapear(e);}
 public async Task<List<VacioCargaDto>> Vacios(CancellationToken ct)
 {
  var tubos=await db.Tubos.AsNoTracking().Include(x=>x.TipoGas).Include(x=>x.CapacidadTubo).Include(x=>x.Ubicacion).Include(x=>x.ClientePropietario).Where(x=>(x.TipoPropietario==TipoPropietarioTubo.Empresa||(x.TipoPropietario==TipoPropietarioTubo.Cliente&&x.ClientePropietarioId!=null&&db.IngresosClientesDetalle.Any(d=>d.TuboId==x.Id&&d.FechaRetiroUtc==null)))&&x.EstadoContenido==EstadoContenido.Vacio&&x.Ubicacion.EsInterna&&x.EstadoDisponibilidad!=EstadoDisponibilidad.DadoDeBaja&&x.EstadoDisponibilidad!=EstadoDisponibilidad.Extraviado).OrderBy(x=>x.CodigoInterno).ToListAsync(ct);
  var hoy=DateOnly.FromDateTime(HoraArgentina.Hoy);var vigentes=await db.PruebasHidraulicas.Where(x=>x.Aprobada&&x.FechaVencimiento>=hoy).Select(x=>x.TuboId).Distinct().ToListAsync(ct);
  return tubos.Select(t=>{var motivo=t.EstadoDisponibilidad!=EstadoDisponibilidad.Disponible?"Reservado o pendiente de revisión":t.EstadoFisico is EstadoFisico.Danado or EstadoFisico.Inutilizable?"Dañado o inutilizable":!vigentes.Contains(t.Id)?"Prueba hidráulica no vigente":"Apto para enviar";return new VacioCargaDto(t.Id,t.CodigoInterno,t.TipoGas.Nombre,Capacidad(t),t.Ubicacion.Nombre,motivo=="Apto para enviar",motivo,t.ClientePropietarioId,Propietario(t));}).ToList();
 }
 public async Task<EnvioCargaDto> Crear(EnvioCargaCrearDto dto,string usuario,CancellationToken ct,bool confirmarSalida=false)
 {
  if(string.IsNullOrWhiteSpace(dto.Destino)||string.IsNullOrWhiteSpace(dto.Transporte)||dto.TuboIds.Count==0||dto.TuboIds.Distinct().Count()!=dto.TuboIds.Count)throw new InvalidOperationException("Seleccioná tubos y completá destino y transporte.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
  var aptos=(await Vacios(ct)).Where(x=>x.Apto).Select(x=>x.Id).ToHashSet();if(dto.TuboIds.Any(id=>!aptos.Contains(id)))throw new InvalidOperationException("Un tubo ya no está disponible o no es apto para carga. Actualizá el stock.");
  var tubos=await db.Tubos.Include(x=>x.TipoGas).Include(x=>x.CapacidadTubo).Include(x=>x.ClientePropietario).Where(x=>dto.TuboIds.Contains(x.Id)).ToListAsync(ct);
  var envio=new EnvioCarga{Numero="CAR-"+Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),Destino=dto.Destino.Trim(),Transporte=dto.Transporte.Trim(),Observaciones=dto.Observaciones,Usuario=usuario};
  foreach(var tubo in tubos){envio.Detalles.Add(new(){TuboId=tubo.Id,UbicacionOrigenId=tubo.UbicacionId,Codigo=tubo.CodigoInterno,Gas=tubo.TipoGas.Nombre,Capacidad=Capacidad(tubo),TipoPropietario=(int)tubo.TipoPropietario,ClientePropietarioId=tubo.ClientePropietarioId,Propietario=Propietario(tubo)});tubo.EstadoDisponibilidad=EstadoDisponibilidad.PreparadoParaCarga;}
  db.EnviosCarga.Add(envio);await db.SaveChangesAsync(ct);if(confirmarSalida){await ConfirmarSalida(envio,usuario,ct);await db.SaveChangesAsync(ct);}await tx.CommitAsync(ct);return(await Obtener(envio.Id,ct))!;
 }
 public async Task<EnvioCargaDto> Salida(long id,string usuario,CancellationToken ct)
 {
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);var e=await Consulta().FirstOrDefaultAsync(x=>x.Id==id,ct)??throw new InvalidOperationException("No existe el envío.");if(e.Estado!="Preparado")throw new InvalidOperationException("El envío no está preparado.");
  await ConfirmarSalida(e,usuario,ct);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return Mapear(e);
 }
 private async Task ConfirmarSalida(EnvioCarga e,string usuario,CancellationToken ct)
 {
  var hoy=DateOnly.FromDateTime(HoraArgentina.Hoy);var vigentes=await db.PruebasHidraulicas.Where(x=>x.Aprobada&&x.FechaVencimiento>=hoy).Select(x=>x.TuboId).ToListAsync(ct);
  var ubicacion=await db.Ubicaciones.FirstOrDefaultAsync(x=>x.Nombre=="Carga externa - Buenos Aires",ct);if(ubicacion is null){ubicacion=new Ubicacion{Nombre="Carga externa - Buenos Aires",EsInterna=false};db.Ubicaciones.Add(ubicacion);await db.SaveChangesAsync(ct);}
  foreach(var d in e.Detalles){var t=d.Tubo;if(!t.Activo||t.EstadoDisponibilidad!=EstadoDisponibilidad.PreparadoParaCarga||t.EstadoContenido!=EstadoContenido.Vacio||t.EstadoFisico is EstadoFisico.Danado or EstadoFisico.Inutilizable||!vigentes.Contains(t.Id))throw new InvalidOperationException("Un tubo dejó de ser apto para enviar.");var origen=t.UbicacionId;t.UbicacionId=ubicacion.Id;t.EstadoDisponibilidad=EstadoDisponibilidad.EnviadoACarga;Movimiento(t,"Envío a carga",origen,usuario,e.Numero+" / "+e.Destino);}
  e.Estado="Enviado";e.FechaSalidaUtc=DateTime.UtcNow;
 }
 public async Task<EnvioCargaDto> ActualizarPendiente(long id,EnvioCargaCrearDto dto,CancellationToken ct)
 {
  if(dto.TuboIds.Count==0||dto.TuboIds.Distinct().Count()!=dto.TuboIds.Count||string.IsNullOrWhiteSpace(dto.Destino)||string.IsNullOrWhiteSpace(dto.Transporte))throw new InvalidOperationException("Completá los datos y seleccioná al menos un tubo.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
  var e=await Consulta().FirstOrDefaultAsync(x=>x.Id==id,ct)??throw new InvalidOperationException("No existe el envío.");if(e.Estado!="Preparado")throw new InvalidOperationException("Solo se puede corregir un envío pendiente de confirmación.");
  if(e.Detalles.Any(d=>d.Tubo.EstadoDisponibilidad!=EstadoDisponibilidad.PreparadoParaCarga))throw new InvalidOperationException("Un tubo cambió de estado. Actualizá el envío.");
  var actuales=e.Detalles.Select(d=>d.TuboId).ToHashSet();var nuevos=dto.TuboIds.Where(x=>!actuales.Contains(x)).ToList();var aptos=(await Vacios(ct)).Where(x=>x.Apto).Select(x=>x.Id).ToHashSet();if(nuevos.Any(x=>!aptos.Contains(x)))throw new InvalidOperationException("Un tubo agregado no está disponible o no es apto para enviar.");
  foreach(var d in e.Detalles.Where(d=>!dto.TuboIds.Contains(d.TuboId)).ToList()){d.Tubo.EstadoDisponibilidad=EstadoDisponibilidad.Disponible;e.Detalles.Remove(d);db.EnviosCargaDetalle.Remove(d);}
  var agregados=await db.Tubos.Include(x=>x.TipoGas).Include(x=>x.CapacidadTubo).Include(x=>x.ClientePropietario).Where(x=>nuevos.Contains(x.Id)).ToListAsync(ct);
  foreach(var t in agregados){e.Detalles.Add(new(){Tubo=t,UbicacionOrigenId=t.UbicacionId,Codigo=t.CodigoInterno,Gas=t.TipoGas.Nombre,Capacidad=Capacidad(t),TipoPropietario=(int)t.TipoPropietario,ClientePropietarioId=t.ClientePropietarioId,Propietario=Propietario(t)});t.EstadoDisponibilidad=EstadoDisponibilidad.PreparadoParaCarga;}
  e.Destino=dto.Destino.Trim();e.Transporte=dto.Transporte.Trim();e.Observaciones=dto.Observaciones;await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return Mapear(e);
 }
 public async Task<EnvioCargaDto> Cancelar(long id,CancellationToken ct)
 {
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);var e=await Consulta().FirstOrDefaultAsync(x=>x.Id==id,ct)??throw new InvalidOperationException("No existe el envío.");if(e.Estado!="Preparado")throw new InvalidOperationException("Solo se puede cancelar un envío preparado.");foreach(var d in e.Detalles){if(d.Tubo.EstadoDisponibilidad!=EstadoDisponibilidad.PreparadoParaCarga)throw new InvalidOperationException("Un tubo cambió de estado.");d.Tubo.EstadoDisponibilidad=EstadoDisponibilidad.Disponible;}e.Estado="Cancelado";await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return Mapear(e);
 }
 public async Task<EnvioCargaDto> Recibir(long id,RecepcionCargaCrearDto dto,string usuario,CancellationToken ct)
 {
  if(dto.Tubos.Count==0||dto.Tubos.Select(x=>x.DetalleId).Distinct().Count()!=dto.Tubos.Count)throw new InvalidOperationException("Seleccioná tubos sin repetir.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);var e=await Consulta().FirstOrDefaultAsync(x=>x.Id==id,ct)??throw new InvalidOperationException("No existe el envío.");if(e.Estado is not ("Enviado" or "Parcial"))throw new InvalidOperationException("El envío no tiene recepción pendiente.");
  if(!await db.Ubicaciones.AnyAsync(x=>x.Id==dto.UbicacionDestinoId&&x.EsInterna&&x.GasBateria==null,ct))throw new InvalidOperationException("Seleccioná una ubicación interna.");
  var r=new RecepcionCarga{Numero="REC-CAR-"+Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),EnvioCarga=e,Usuario=usuario,Observaciones=dto.Observaciones};
  var hoy=DateOnly.FromDateTime(HoraArgentina.Hoy);var vigentes=await db.PruebasHidraulicas.Where(x=>x.Aprobada&&x.FechaVencimiento>=hoy).Select(x=>x.TuboId).ToListAsync(ct);
  foreach(var item in dto.Tubos){var d=e.Detalles.SingleOrDefault(x=>x.Id==item.DetalleId)??throw new InvalidOperationException("El tubo no pertenece al envío.");if(d.Estado!="Pendiente"||d.Tubo.EstadoDisponibilidad!=EstadoDisponibilidad.EnviadoACarga)throw new InvalidOperationException("El tubo ya fue recibido o cambió de estado.");if(item.Resultado is not ("Lleno" or "Vacio" or "Danado"))throw new InvalidOperationException("Resultado inválido.");var t=d.Tubo;var origen=t.UbicacionId;t.UbicacionId=dto.UbicacionDestinoId;t.EstadoContenido=item.Resultado=="Lleno"?EstadoContenido.Lleno:EstadoContenido.Vacio;if(item.Resultado=="Danado")t.EstadoFisico=EstadoFisico.Danado;t.EstadoDisponibilidad=item.Resultado=="Danado"?EstadoDisponibilidad.EnMantenimiento:!vigentes.Contains(t.Id)?EstadoDisponibilidad.PendienteInspeccion:EstadoDisponibilidad.Disponible;d.Estado="Recibido";r.Detalles.Add(new(){EnvioCargaDetalle=d,Resultado=item.Resultado,UbicacionDestinoId=dto.UbicacionDestinoId});Movimiento(t,"Recepción de carga",origen,usuario,r.Numero+" / "+e.Numero+" / "+item.Resultado);}
  e.Recepciones.Add(r);Cerrar(e);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return Mapear(e);
 }
 public async Task<EnvioCargaDto> Resolver(long id,ResolucionCargaDto dto,string usuario,CancellationToken ct)
 {
  if(string.IsNullOrWhiteSpace(dto.Motivo))throw new InvalidOperationException("Indicá el motivo del faltante definitivo.");await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);var e=await Consulta().FirstOrDefaultAsync(x=>x.Id==id,ct)??throw new InvalidOperationException("No existe el envío.");if(e.Estado is not ("Enviado" or "Parcial"))throw new InvalidOperationException("El envío no tiene pendientes.");var d=e.Detalles.SingleOrDefault(x=>x.Id==dto.DetalleId)??throw new InvalidOperationException("El tubo no pertenece al envío.");if(d.Estado!="Pendiente"||d.Tubo.EstadoDisponibilidad!=EstadoDisponibilidad.EnviadoACarga)throw new InvalidOperationException("El tubo ya fue resuelto.");d.Estado="Faltante resuelto";d.Resolucion=dto.Motivo.Trim();d.Tubo.EstadoDisponibilidad=EstadoDisponibilidad.Extraviado;Movimiento(d.Tubo,"Faltante de carga",d.Tubo.UbicacionId,usuario,e.Numero+" / "+d.Resolucion);Cerrar(e);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return Mapear(e);
 }
 private static void Cerrar(EnvioCarga e)=>e.Estado=e.Detalles.Any(x=>x.Estado=="Pendiente")?"Parcial":e.Detalles.Any(x=>x.Estado=="Faltante resuelto")?"Cerrado con faltantes":"Completo";
 private void Movimiento(Tubo t,string tipo,int origen,string usuario,string obs)=>db.MovimientosCilindro.Add(new(){TuboId=t.Id,Tipo=tipo,UbicacionOrigenId=origen,UbicacionDestinoId=t.UbicacionId,EstadoPosterior=$"Contenido={(int)t.EstadoContenido};Disponibilidad={(int)t.EstadoDisponibilidad};Ubicación={t.UbicacionId}",UsuarioResponsable=usuario,Observaciones=obs});
 private static string Propietario(Tubo t)=>t.TipoPropietario==TipoPropietarioTubo.Empresa?"Oxígeno Norte":t.ClientePropietario?.RazonSocial??$"{t.ClientePropietario?.Apellido}, {t.ClientePropietario?.Nombre}";
 private static string Capacidad(Tubo t)=>$"{t.CapacidadTubo.Valor} {t.CapacidadTubo.Unidad}";
 private static EnvioCargaDto Mapear(EnvioCarga e)=>new(e.Id,e.Numero,e.Destino,e.Transporte,e.Estado,e.FechaCreacionUtc,e.FechaSalidaUtc,e.Usuario,e.Observaciones,e.Detalles.Select(d=>new EnvioCargaTuboDto(d.Id,d.TuboId,d.Codigo,d.Gas,d.Capacidad,d.Estado,d.Resolucion,d.ClientePropietarioId,d.Propietario)).ToList(),e.Recepciones.OrderBy(r=>r.FechaUtc).Select(r=>new RecepcionCargaDto(r.Id,r.Numero,r.FechaUtc,r.Usuario,r.Observaciones,r.Detalles.Select(d=>{var t=e.Detalles.Single(x=>x.Id==d.EnvioCargaDetalleId);return new RecepcionCargaTuboDto(t.Codigo,t.Gas,t.Capacidad,d.Resultado,t.Propietario);}).ToList())).ToList());
}
