using System.ComponentModel.DataAnnotations;
namespace GestionTubos.Shared.Repartos;
public sealed class RepartoCrearDto
{
    public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [Required,StringLength(150)] public string Responsable { get; set; } = string.Empty;
    [MinLength(1,ErrorMessage="Seleccioná al menos un pedido.")] public List<long> PedidoIds { get; set; }=[];
    [StringLength(1000)] public string? Observaciones { get; set; }
}
public sealed record RepartoPedidoDto(long PedidoId,string NumeroPedido,string NumeroRemito,string Cliente,int CantidadCilindros,string Estado);
public sealed record RepartoDto(long Id,string Numero,DateOnly Fecha,string Responsable,string Estado,DateTime FechaCreacionUtc,DateTime? FechaSalidaUtc,string UsuarioCreador,string? Observaciones,IReadOnlyList<RepartoPedidoDto> Pedidos);
public sealed class RepartoAnularDto{[Required,StringLength(500)]public string Motivo{get;set;}=string.Empty;}
public sealed class RepartoNoEntregadoDto
{
 [Required,StringLength(500)]public string Motivo{get;set;}=string.Empty;
 [Range(1,int.MaxValue)]public int UbicacionRetornoId{get;set;}
 [Required]public string Accion{get;set;}="Reprogramar";
}
