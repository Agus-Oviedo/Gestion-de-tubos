namespace GestionTubos.Domain.Entities;
public sealed class AuditoriaRemito
{
 public long Id{get;set;} public long PedidoId{get;set;} public string NumeroRemito{get;set;}=""; public int Version{get;set;}
 public DateTime FechaUtc{get;set;}=DateTime.UtcNow;public string UsuarioId{get;set;}="";public string Usuario{get;set;}="";
 public string Accion{get;set;}="";public string Motivo{get;set;}="";public string? AntesJson{get;set;}public string? DespuesJson{get;set;}public string CambiosJson{get;set;}="[]";
}
