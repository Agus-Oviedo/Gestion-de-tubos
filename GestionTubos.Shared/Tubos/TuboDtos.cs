using System.ComponentModel.DataAnnotations;
namespace GestionTubos.Shared.Tubos;

public sealed class TubosUbicarDto
{
 [Range(1,int.MaxValue)] public int UbicacionDestinoId { get; set; }
 [Required, MinLength(1), MaxLength(500)] public List<TuboUbicarItemDto> Tubos { get; set; } = [];
}

public sealed class TuboUbicarItemDto
{
 [Range(1,int.MaxValue)] public int Id { get; set; }
 [Range(1,int.MaxValue)] public int UbicacionOrigenId { get; set; }
}

public sealed class TuboGuardarDto:IValidatableObject
{
 [Required(ErrorMessage="El código de serie es obligatorio.")][StringLength(50)]public string CodigoInterno{get;set;}=string.Empty;
 [StringLength(100)]public string? NumeroSerie{get;set;} [StringLength(100)]public string? IdentificacionGrabada{get;set;}
 [Range(1,int.MaxValue,ErrorMessage="Seleccioná el gas.")]public int TipoGasId{get;set;} [Range(1,int.MaxValue,ErrorMessage="Seleccioná la capacidad.")]public int CapacidadTuboId{get;set;}
 [StringLength(100)]public string? Marca{get;set;} [StringLength(150)]public string? Fabricante{get;set;} public DateOnly? FechaFabricacion{get;set;}
 [Range(1,4)]public int TipoPropietario{get;set;}=1; public int? ClientePropietarioId{get;set;} [StringLength(200)]public string? NombrePropietarioExterno{get;set;}
 [Range(1,4)]public int EstadoFisico{get;set;}=1; [Range(1,4)]public int EstadoContenido{get;set;}=1; [Range(1,10)]public int EstadoDisponibilidad{get;set;}=6;
 public int UbicacionId{get;set;} [StringLength(1000)]public string? Observaciones{get;set;}
 public bool RegistrarPruebaInicial{get;set;}
 public DateOnly FechaPruebaHidraulica{get;set;}=DateOnly.FromDateTime(GestionTubos.Shared.HoraArgentina.Hoy);
 public DateOnly FechaVencimientoPrueba{get;set;}=DateOnly.FromDateTime(GestionTubos.Shared.HoraArgentina.Hoy.AddYears(5));
 public bool PruebaAprobada{get;set;}=true;
 [StringLength(200)]public string? EntidadPrueba{get;set;} [StringLength(100)]public string? NumeroCertificadoPrueba{get;set;} [StringLength(1000)]public string? ObservacionesPrueba{get;set;}
 public IEnumerable<ValidationResult> Validate(ValidationContext c)
 {
  if(TipoPropietario==2&&!ClientePropietarioId.HasValue)yield return new("Seleccioná el cliente propietario.",[nameof(ClientePropietarioId)]);
  if(TipoPropietario!=2&&UbicacionId<=0)yield return new("Seleccioná la ubicación.",[nameof(UbicacionId)]);
  if(TipoPropietario is 3 or 4&&string.IsNullOrWhiteSpace(NombrePropietarioExterno))yield return new("Indicá el propietario.",[nameof(NombrePropietarioExterno)]);
  if(RegistrarPruebaInicial&&string.IsNullOrWhiteSpace(EntidadPrueba))yield return new("Indicá la entidad que realizó la prueba.",[nameof(EntidadPrueba)]);
  if(RegistrarPruebaInicial&&FechaVencimientoPrueba<=FechaPruebaHidraulica)yield return new("El vencimiento debe ser posterior a la fecha de la prueba.",[nameof(FechaVencimientoPrueba)]);
 }
}

public sealed record TuboDto(int Id,string CodigoInterno,string? NumeroSerie,int TipoGasId,string Gas,int CapacidadTuboId,string Capacidad,int TipoPropietario,int? ClientePropietarioId,string Propietario,int EstadoFisico,int EstadoContenido,int EstadoDisponibilidad,int UbicacionId,string Ubicacion,string? Marca,string? Fabricante,DateOnly? FechaFabricacion,string? IdentificacionGrabada,string? NombrePropietarioExterno,string? Observaciones,DateOnly? FechaPruebaHidraulica=null,DateOnly? FechaVencimientoPrueba=null);
