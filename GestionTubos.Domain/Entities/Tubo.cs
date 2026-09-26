using GestionTubos.Domain.Enums;
namespace GestionTubos.Domain.Entities;
public sealed class Tubo
{
 public int Id{get;set;} public string CodigoInterno{get;set;}=string.Empty; public string? NumeroSerie{get;set;} public string? IdentificacionGrabada{get;set;}
 public int TipoGasId{get;set;} public TipoGas TipoGas{get;set;}=null!; public int CapacidadTuboId{get;set;} public CapacidadTubo CapacidadTubo{get;set;}=null!;
 public string? Marca{get;set;} public string? Fabricante{get;set;} public DateOnly? FechaFabricacion{get;set;}
 public TipoPropietarioTubo TipoPropietario{get;set;} public int? ClientePropietarioId{get;set;} public Cliente? ClientePropietario{get;set;} public string? NombrePropietarioExterno{get;set;}
 public EstadoFisico EstadoFisico{get;set;}=EstadoFisico.Bueno; public EstadoContenido EstadoContenido{get;set;}=EstadoContenido.Desconocido; public EstadoDisponibilidad EstadoDisponibilidad{get;set;}=EstadoDisponibilidad.PendienteInspeccion;
 public int UbicacionId{get;set;} public Ubicacion Ubicacion{get;set;}=null!; public DateTime FechaAltaUtc{get;set;}=DateTime.UtcNow; public string? Observaciones{get;set;} public bool Activo{get;set;}=true;
}
