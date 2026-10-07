using System.ComponentModel.DataAnnotations;
namespace GestionTubos.Shared.Pedidos;
public sealed class PedidoCrearDto:IValidatableObject
{
 [Range(1,int.MaxValue)]public int ClienteId{get;set;}
 [MinLength(1,ErrorMessage="Seleccioná al menos un cilindro.")]public List<int>CilindroIds{get;set;}=[];
 [Required]public string ModalidadEntrega{get;set;}="RetiroPlanta"; public int?DireccionEntregaId{get;set;} public DateOnly?FechaPrevistaEntrega{get;set;}
 [StringLength(150)]public string?ContactoEntrega{get;set;} [StringLength(50)]public string?TelefonoEntrega{get;set;} [StringLength(1000)]public string?Observaciones{get;set;}
 [StringLength(150)]public string PersonaRecibe{get;set;}=string.Empty;[StringLength(20)]public string DniRecibe{get;set;}=string.Empty;
 [StringLength(400000)]public string FirmaDataUrl{get;set;}=string.Empty;
 public IEnumerable<ValidationResult>Validate(ValidationContext context){if(ModalidadEntrega=="Domicilio"&&!DireccionEntregaId.HasValue)yield return new("Seleccioná la dirección de entrega.",[nameof(DireccionEntregaId)]);if(ModalidadEntrega is not ("RetiroPlanta" or "Domicilio"))yield return new("La modalidad de entrega no es válida.",[nameof(ModalidadEntrega)]);}
}
public sealed record PedidoDetalleDto(int CilindroId,string Codigo,string Gas,string Capacidad,int TipoPropietario,int? ClientePropietarioId);
public sealed record PedidoDto(long Id,string Numero,int ClienteId,string Cliente,DateTime FechaHoraUtc,string Estado,string UsuarioCreador,string?Observaciones,IReadOnlyList<PedidoDetalleDto>Cilindros,string?NumeroRemito,string ModalidadEntrega,int?DireccionEntregaId,string?DireccionEntregaTexto,DateOnly?FechaPrevistaEntrega,string?ContactoEntrega,string?TelefonoEntrega,string?PersonaRecibe,string?DniRecibe,string?FirmaDataUrl,DateTime?FechaFirmaUtc,string?UsuarioEntrega,string?HuellaDocumento);
public sealed record RemitoDto(long Id,string Numero,long PedidoId,DateTime FechaEmisionUtc,string Estado,string UsuarioEmisor);
public sealed class PedidoEntregarDto{[Required]public string Modalidad{get;set;}="Alquiler";[Range(0,3650)]public int DiasSinCargo{get;set;}[Required,StringLength(150)]public string PersonaRecibe{get;set;}=string.Empty;[Required,StringLength(20)]public string DniRecibe{get;set;}=string.Empty;[StringLength(400000)]public string FirmaDataUrl{get;set;}=string.Empty;[StringLength(1000)]public string?Observaciones{get;set;}}
