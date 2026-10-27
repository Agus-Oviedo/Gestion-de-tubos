namespace GestionTubos.Domain.Entities;
public sealed class IngresoCliente
{
 public long Id{get;set;}public string Numero{get;set;}="";public int ClienteId{get;set;}public Cliente Cliente{get;set;}=null!;public string ClienteNombre{get;set;}="";public string DocumentoCliente{get;set;}="";
 public DateTime FechaUtc{get;set;}=DateTime.UtcNow;public int UbicacionId{get;set;}public string PersonaEntrega{get;set;}="";public string?DocumentoEntrega{get;set;}public string Usuario{get;set;}="";public string UsuarioId{get;set;}="";public string?Observaciones{get;set;}public List<IngresoClienteDetalle>Detalles{get;set;}=[];
}
public sealed class IngresoClienteDetalle
{
 public long Id{get;set;}public long IngresoClienteId{get;set;}public IngresoCliente IngresoCliente{get;set;}=null!;public int TuboId{get;set;}public Tubo Tubo{get;set;}=null!;
 public string Codigo{get;set;}="";public string Gas{get;set;}="";public string Capacidad{get;set;}="";public DateTime?FechaRetiroUtc{get;set;}public long?RemitoEntregaId{get;set;}public Remito?RemitoEntrega{get;set;}
}
