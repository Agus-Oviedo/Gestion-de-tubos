using GestionTubos.Domain.Enums;
namespace GestionTubos.Domain.Entities;
public sealed class EnvioCarga
{
 public long Id {get;set;} public string Numero {get;set;}=""; public string Destino {get;set;}=""; public string Transporte {get;set;}=""; public string Estado {get;set;}="Preparado"; public DateTime FechaCreacionUtc {get;set;}=DateTime.UtcNow; public DateTime? FechaSalidaUtc {get;set;} public string Usuario {get;set;}=""; public string? Observaciones {get;set;} public List<EnvioCargaDetalle> Detalles {get;set;}=[]; public List<RecepcionCarga> Recepciones {get;set;}=[];
}
public sealed class EnvioCargaDetalle
{
 public int TipoPropietario{get;set;}=1;public int?ClientePropietarioId{get;set;}public string Propietario{get;set;}="Oxígeno Norte";
 public long Id {get;set;} public long EnvioCargaId {get;set;} public EnvioCarga EnvioCarga {get;set;}=null!; public int TuboId {get;set;} public Tubo Tubo {get;set;}=null!; public int UbicacionOrigenId {get;set;} public string Codigo {get;set;}=""; public string Gas {get;set;}=""; public string Capacidad {get;set;}=""; public string Estado {get;set;}="Pendiente"; public string? Resolucion {get;set;}
}
public sealed class RecepcionCarga
{
 public long Id {get;set;} public string Numero {get;set;}=""; public long EnvioCargaId {get;set;} public EnvioCarga EnvioCarga {get;set;}=null!; public DateTime FechaUtc {get;set;}=DateTime.UtcNow; public string Usuario {get;set;}=""; public string? Observaciones {get;set;} public List<RecepcionCargaDetalle> Detalles {get;set;}=[];
}
public sealed class RecepcionCargaDetalle
{
 public long Id {get;set;} public long RecepcionCargaId {get;set;} public RecepcionCarga RecepcionCarga {get;set;}=null!; public long EnvioCargaDetalleId {get;set;} public EnvioCargaDetalle EnvioCargaDetalle {get;set;}=null!; public string Resultado {get;set;}="Lleno"; public int UbicacionDestinoId {get;set;}
}
