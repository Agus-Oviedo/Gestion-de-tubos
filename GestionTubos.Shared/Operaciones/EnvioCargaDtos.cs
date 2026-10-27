using System.ComponentModel.DataAnnotations;
namespace GestionTubos.Shared.Operaciones;
public sealed class EnvioCargaCrearDto
{
 [Required,StringLength(200)]public string Destino{get;set;}="Cargadora Buenos Aires"; [Required,StringLength(200)]public string Transporte{get;set;}=""; [StringLength(1000)]public string?Observaciones{get;set;} [MinLength(1,ErrorMessage="Seleccioná al menos un tubo para enviar.")]public List<int>TuboIds{get;set;}=[];
}
public sealed class RecepcionCargaCrearDto
{
 [Range(1,int.MaxValue)]public int UbicacionDestinoId{get;set;} [StringLength(1000)]public string?Observaciones{get;set;} [MinLength(1,ErrorMessage="Seleccioná al menos un tubo recibido.")]public List<RecepcionCargaItemDto>Tubos{get;set;}=[];
}
public sealed class RecepcionCargaItemDto{public long DetalleId{get;set;} public string Resultado{get;set;}="Lleno";}
public sealed class ResolucionCargaDto{public long DetalleId{get;set;} [Required,StringLength(1000)]public string Motivo{get;set;}="";}
public sealed record VacioCargaDto(int Id,string Codigo,string Gas,string Capacidad,string Ubicacion,bool Apto,string Motivo,int?ClientePropietarioId=null,string Propietario="Oxígeno Norte");
public sealed record EnvioCargaTuboDto(long Id,int TuboId,string Codigo,string Gas,string Capacidad,string Estado,string?Resolucion,int?ClientePropietarioId=null,string Propietario="Oxígeno Norte");
public sealed record RecepcionCargaTuboDto(string Codigo,string Gas,string Capacidad,string Resultado,string Propietario="Oxígeno Norte");
public sealed record RecepcionCargaDto(long Id,string Numero,DateTime FechaUtc,string Usuario,string?Observaciones,IReadOnlyList<RecepcionCargaTuboDto>Tubos);
public sealed record EnvioCargaDto(long Id,string Numero,string Destino,string Transporte,string Estado,DateTime FechaCreacionUtc,DateTime?FechaSalidaUtc,string Usuario,string?Observaciones,IReadOnlyList<EnvioCargaTuboDto>Tubos,IReadOnlyList<RecepcionCargaDto>Recepciones);
