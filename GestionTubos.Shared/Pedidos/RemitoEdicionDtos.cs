using System.ComponentModel.DataAnnotations;
namespace GestionTubos.Shared.Pedidos;
public sealed class RemitoEditarDto:IValidatableObject
{
 [Range(0,int.MaxValue)]public int VersionEsperada{get;set;}
 [Range(1,int.MaxValue)]public int ClienteId{get;set;}
 [Required]public List<int>CilindroIds{get;set;}=[];
 [Required]public string ModalidadEntrega{get;set;}="RetiroPlanta";public int?DireccionEntregaId{get;set;}public DateOnly?FechaPrevistaEntrega{get;set;}
 [StringLength(150)]public string?ContactoEntrega{get;set;}[StringLength(50)]public string?TelefonoEntrega{get;set;}[StringLength(1000)]public string?Observaciones{get;set;}
 [Required(ErrorMessage="Indicá el motivo del cambio."),StringLength(1000),MinLength(3)]public string Motivo{get;set;}="";
 public IEnumerable<ValidationResult>Validate(ValidationContext c){if(CilindroIds is null||CilindroIds.Count==0)yield return new("Seleccioná al menos un tubo.",[nameof(CilindroIds)]);if(string.IsNullOrWhiteSpace(Motivo)||Motivo.Trim().Length<3)yield return new("Indicá el motivo del cambio.",[nameof(Motivo)]);if(ModalidadEntrega is not("RetiroPlanta" or "Domicilio"))yield return new("Lugar de entrega inválido.");if(ModalidadEntrega=="Domicilio"&&!DireccionEntregaId.HasValue)yield return new("Seleccioná una dirección.");}
}
public sealed record CambioRemitoDto(string Campo,string? Antes,string? Despues);
public sealed record AuditoriaRemitoDto(long Id,long PedidoId,string NumeroRemito,int Version,DateTime FechaUtc,string UsuarioId,string Usuario,string Accion,string Motivo,IReadOnlyList<CambioRemitoDto>Cambios);
public sealed record PaginaAuditoriaDto(IReadOnlyList<AuditoriaRemitoDto>Registros,bool HayMas);

public sealed class RemitoEliminarDto{[Required,MinLength(3),StringLength(1000)]public string Motivo{get;set;}="";}
