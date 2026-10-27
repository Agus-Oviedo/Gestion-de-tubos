using System.ComponentModel.DataAnnotations;
namespace GestionTubos.Shared.Operaciones;
public sealed class IngresoClienteCrearDto:IValidatableObject
{
 [Range(1,int.MaxValue)]public int ClienteId{get;set;}[Required]public List<int>TuboIds{get;set;}=[];[Range(1,int.MaxValue)]public int UbicacionId{get;set;}
 [Required,StringLength(150)]public string PersonaEntrega{get;set;}="";[StringLength(20)]public string?DocumentoEntrega{get;set;}[StringLength(1000)]public string?Observaciones{get;set;}
 public IEnumerable<ValidationResult>Validate(ValidationContext c){if(TuboIds is null||TuboIds.Count is <1 or >500)yield return new("Seleccioná entre 1 y 500 tubos.");if(string.IsNullOrWhiteSpace(PersonaEntrega))yield return new("Indicá quién entrega los tubos.",[nameof(PersonaEntrega)]);}
}
public sealed record IngresoClienteTuboDto(int TuboId,string Codigo,string Gas,string Capacidad,DateTime?FechaRetiroUtc,string?RemitoEntrega);
public sealed record IngresoClienteDto(long Id,string Numero,int ClienteId,string Cliente,string DocumentoCliente,DateTime FechaUtc,string Ubicacion,string PersonaEntrega,string?DocumentoEntrega,string Usuario,string?Observaciones,IReadOnlyList<IngresoClienteTuboDto>Tubos);
