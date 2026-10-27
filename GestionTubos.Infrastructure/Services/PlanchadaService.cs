using System.Data;
using System.ComponentModel.DataAnnotations;
using GestionTubos.Application.Interfaces;
using GestionTubos.Domain.Entities;
using GestionTubos.Domain.Enums;
using GestionTubos.Infrastructure.Data;
using GestionTubos.Shared;
using GestionTubos.Shared.Operaciones;
using Microsoft.EntityFrameworkCore;
namespace GestionTubos.Infrastructure.Services;
public sealed class PlanchadaService(GestionTubosDbContext db,IUsuarioActual? actor=null)
{
 public async Task InicializarAsync(CancellationToken ct=default)
 {
  foreach(var (nombre,gas) in new[]{("Planchada de oxígeno","O2"),("Planchada de nitrógeno","N2"),("Planchada de argón","Ar")}){
   var u=await db.Ubicaciones.IgnoreQueryFilters().SingleOrDefaultAsync(x=>x.Nombre==nombre,ct);
   if(u is null)db.Ubicaciones.Add(new(){Nombre=nombre,EsInterna=true,GasBateria=gas,Descripcion="Sector interno para baterías. Gestionar desde Planchadas y baterías."});
   else if(!u.EsInterna||!u.Activa)throw new InvalidOperationException($"Revisá la ubicación {nombre}: debe ser interna y activa.");
   else u.GasBateria=gas;
  }
  await db.SaveChangesAsync(ct);
 }
 public async Task<List<SectorBateriaDto>> Sectores(CancellationToken ct){var activos=await db.UsosBateria.Where(x=>x.FechaRetiroUtc==null).GroupBy(x=>x.UbicacionBateriaId).Select(g=>new{Id=g.Key,Total=g.Count()}).ToDictionaryAsync(x=>x.Id,x=>x.Total,ct);return (await db.Ubicaciones.AsNoTracking().Where(x=>x.GasBateria!=null).OrderBy(x=>x.Id).ToListAsync(ct)).Select(x=>new SectorBateriaDto(x.Id,x.Nombre,x.GasBateria!,activos.GetValueOrDefault(x.Id))).ToList();}
 private async Task<Ubicacion> Sector(int id,CancellationToken ct)=>await db.Ubicaciones.SingleOrDefaultAsync(x=>x.Id==id&&x.EsInterna&&x.GasBateria!=null,ct)??throw new InvalidOperationException("Seleccioná una planchada activa.");
 private static string Unidad(string u)=>u.Trim().ToLowerInvariant() is "m3" or "m³" or "m^3"?"m³":u.Trim();
 private static string Estado(Tubo t)=>$"Físico={(int)t.EstadoFisico};Contenido={(int)t.EstadoContenido};Disponibilidad={(int)t.EstadoDisponibilidad};Ubicación={t.UbicacionId}";
 private static void Validar(object dto){var errores=new List<ValidationResult>();if(!Validator.TryValidateObject(dto,new(dto),errores,true))throw new InvalidOperationException(errores.First().ErrorMessage);}
 private IQueryable<Tubo> BaseCandidatos()=>db.Tubos.Include(x=>x.TipoGas).Include(x=>x.CapacidadTubo).Include(x=>x.Ubicacion).Where(t=>t.TipoPropietario==TipoPropietarioTubo.Empresa&&t.EstadoDisponibilidad==EstadoDisponibilidad.Disponible&&t.EstadoContenido==EstadoContenido.Lleno&&t.EstadoFisico!=EstadoFisico.Danado&&t.EstadoFisico!=EstadoFisico.Inutilizable&&t.Ubicacion.EsInterna&&t.Ubicacion.GasBateria==null&&!db.UsosBateria.Any(b=>b.TuboId==t.Id&&b.FechaRetiroUtc==null));
 private async Task<HashSet<int>> Vigentes(CancellationToken ct){var hoy=DateOnly.FromDateTime(HoraArgentina.Hoy);return (await db.PruebasHidraulicas.Where(x=>x.Aprobada&&x.FechaVencimiento>=hoy).Select(x=>x.TuboId).Distinct().ToListAsync(ct)).ToHashSet();}
 public async Task<List<CandidatoBateriaDto>> Candidatos(int sectorId,CancellationToken ct){var s=await Sector(sectorId,ct);var ph=await Vigentes(ct);return (await BaseCandidatos().AsNoTracking().OrderBy(x=>x.CodigoInterno).ToListAsync(ct)).Where(t=>ph.Contains(t.Id)&&BusquedaTubos.Coincide(s.GasBateria,t.TipoGas.Nombre)).Select(t=>new CandidatoBateriaDto(t.Id,t.CodigoInterno,t.TipoGas.Nombre,t.CapacidadTubo.Valor,Unidad(t.CapacidadTubo.Unidad),t.Ubicacion.Nombre)).ToList();}
 public async Task Ingresar(BateriaIngresoDto dto,string usuario,CancellationToken ct)
 {
  Validar(dto);if(dto.TuboIds.Distinct().Count()!=dto.TuboIds.Count)throw new InvalidOperationException("Hay tubos repetidos.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);var s=await Sector(dto.SectorId,ct);var ph=await Vigentes(ct);
  var tubos=await BaseCandidatos().Where(x=>dto.TuboIds.Contains(x.Id)).ToListAsync(ct);
  if(tubos.Count!=dto.TuboIds.Count||tubos.Any(t=>!ph.Contains(t.Id)||!BusquedaTubos.Coincide(s.GasBateria,t.TipoGas.Nombre)))throw new InvalidOperationException("Algún tubo ya no está disponible, no está lleno, no es propio, no tiene PH vigente o no corresponde al gas de la planchada.");
  var ahora=DateTime.UtcNow;
  foreach(var t in tubos){var anterior=Estado(t);var origen=t.UbicacionId;db.UsosBateria.Add(new(){TuboId=t.Id,UbicacionBateriaId=s.Id,UbicacionOrigenId=origen,CodigoTubo=t.CodigoInterno,Gas=t.TipoGas.Nombre,CapacidadNominal=t.CapacidadTubo.Valor,Unidad=Unidad(t.CapacidadTubo.Unidad),FechaIngresoUtc=ahora,UsuarioIngreso=usuario,UsuarioIngresoId=actor?.Id??"Sistema"});t.UbicacionId=s.Id;t.EstadoDisponibilidad=EstadoDisponibilidad.EnBateria;db.MovimientosCilindro.Add(new(){TuboId=t.Id,Tipo="Ingreso a batería",UbicacionOrigenId=origen,UbicacionDestinoId=s.Id,EstadoAnterior=anterior,EstadoPosterior=Estado(t),UsuarioResponsable=usuario,Observaciones=$"Incorporación a {s.Nombre}. Reservado para consumo interno."});}
  await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
 }
 public async Task Retirar(BateriaRetiroDto dto,string usuario,CancellationToken ct)
 {
  Validar(dto);if(dto.UsoIds.Distinct().Count()!=dto.UsoIds.Count)throw new InvalidOperationException("Hay tubos repetidos.");
  await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
  if(!await db.Ubicaciones.AnyAsync(x=>x.Id==dto.UbicacionDestinoId&&x.EsInterna&&x.GasBateria==null,ct))throw new InvalidOperationException("Seleccioná una ubicación interna de stock, fuera de las planchadas.");
  var usos=await db.UsosBateria.IgnoreQueryFilters().Include(x=>x.Tubo).Include(x=>x.UbicacionBateria).Where(x=>dto.UsoIds.Contains(x.Id)).ToListAsync(ct);
  if(usos.Count!=dto.UsoIds.Count||usos.Any(x=>x.FechaRetiroUtc!=null||!x.Tubo.Activo||x.Tubo.EstadoDisponibilidad!=EstadoDisponibilidad.EnBateria||x.Tubo.UbicacionId!=x.UbicacionBateriaId))throw new InvalidOperationException("Algún tubo ya fue retirado o cambió de estado. Actualizá la lista.");
  var ph=await Vigentes(ct);var ahora=DateTime.UtcNow;var motivo=string.IsNullOrWhiteSpace(dto.Motivo)?"Tubo agotado en batería.":dto.Motivo.Trim();
  foreach(var u in usos){var t=u.Tubo;var anterior=Estado(t);var origen=t.UbicacionId;u.FechaRetiroUtc=ahora;u.ResultadoRetiro=dto.Resultado;u.UbicacionRetiroId=dto.UbicacionDestinoId;u.MotivoRetiro=motivo;u.UsuarioRetiro=usuario;u.UsuarioRetiroId=actor?.Id??"Sistema";t.EstadoContenido=dto.Resultado switch{"Agotado"=>EstadoContenido.Vacio,"Lleno"=>EstadoContenido.Lleno,_=>EstadoContenido.ParcialmenteLleno};t.EstadoDisponibilidad=t.EstadoFisico is EstadoFisico.Danado or EstadoFisico.Inutilizable?EstadoDisponibilidad.EnMantenimiento:ph.Contains(t.Id)?EstadoDisponibilidad.Disponible:EstadoDisponibilidad.PendienteInspeccion;t.UbicacionId=dto.UbicacionDestinoId;db.MovimientosCilindro.Add(new(){TuboId=t.Id,Tipo="Retiro de batería",UbicacionOrigenId=origen,UbicacionDestinoId=t.UbicacionId,EstadoAnterior=anterior,EstadoPosterior=Estado(t),UsuarioResponsable=usuario,Observaciones=$"{u.UbicacionBateria.Nombre} · {dto.Resultado}. {motivo}"[..Math.Min(1000,$"{u.UbicacionBateria.Nombre} · {dto.Resultado}. {motivo}".Length)]});}
  await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
 }
 private static UsoBateriaDto Mapear(UsoBateria x)=>new(x.Id,x.TuboId,x.UbicacionBateriaId,x.UbicacionBateria.Nombre,x.CodigoTubo,x.Gas,x.CapacidadNominal,x.Unidad,x.FechaIngresoUtc,x.FechaRetiroUtc,x.ResultadoRetiro,x.UsuarioIngreso,x.UsuarioRetiro,x.MotivoRetiro);
 public async Task<List<UsoBateriaDto>> Activos(int? sectorId,CancellationToken ct)=> (await db.UsosBateria.IgnoreQueryFilters().AsNoTracking().Include(x=>x.UbicacionBateria).Where(x=>x.FechaRetiroUtc==null&&(!sectorId.HasValue||x.UbicacionBateriaId==sectorId)).OrderBy(x=>x.FechaIngresoUtc).ToListAsync(ct)).Select(Mapear).ToList();
 private static (DateTime Desde,DateTime Hasta) Periodo(DateOnly desde,DateOnly hasta){if(hasta<desde||hasta.Year>=9999)throw new InvalidOperationException("Revisá el período de consulta.");var zona=TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");return(TimeZoneInfo.ConvertTimeToUtc(desde.ToDateTime(TimeOnly.MinValue),zona),TimeZoneInfo.ConvertTimeToUtc(hasta.AddDays(1).ToDateTime(TimeOnly.MinValue),zona));}
 public async Task<ConsumoInternoDto> Consumo(DateOnly desde,DateOnly hasta,int?sectorId,CancellationToken ct)
 {
  var p=Periodo(desde,hasta);var xs=await db.UsosBateria.AsNoTracking().Where(x=>x.ResultadoRetiro=="Agotado"&&x.FechaRetiroUtc>=p.Desde&&x.FechaRetiroUtc<p.Hasta&&(!sectorId.HasValue||x.UbicacionBateriaId==sectorId)).ToListAsync(ct);
  return new(desde,hasta,xs.GroupBy(x=>new{x.Gas,x.Unidad}).Select(g=>new ConsumoGasDto(g.Key.Gas,g.Key.Unidad,g.Count(),g.Sum(x=>x.CapacidadNominal),Math.Round(g.Average(x=>(x.FechaRetiroUtc!.Value-x.FechaIngresoUtc).TotalDays),2))).OrderBy(x=>x.Gas).ToList(),xs.GroupBy(x=>new{Mes=x.FechaRetiroUtc!.Value.EnArgentina().ToString("yyyy-MM"),x.Gas,x.Unidad}).Select(g=>new ConsumoMensualDto(g.Key.Mes,g.Key.Gas,g.Key.Unidad,g.Count(),g.Sum(x=>x.CapacidadNominal))).OrderByDescending(x=>x.Mes).ThenBy(x=>x.Gas).ToList());
 }
 public async Task<HistorialBateriaDto> Historial(DateOnly desde,DateOnly hasta,int?sectorId,long?anteriorA,CancellationToken ct){var p=Periodo(desde,hasta);var xs=await db.UsosBateria.IgnoreQueryFilters().AsNoTracking().Include(x=>x.UbicacionBateria).Where(x=>x.FechaRetiroUtc>=p.Desde&&x.FechaRetiroUtc<p.Hasta&&(!sectorId.HasValue||x.UbicacionBateriaId==sectorId)&&(!anteriorA.HasValue||x.Id<anteriorA)).OrderByDescending(x=>x.Id).Take(21).ToListAsync(ct);return new(xs.Take(20).Select(Mapear).ToList(),xs.Count>20);}
}
