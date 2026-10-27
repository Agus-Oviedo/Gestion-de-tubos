namespace GestionTubos.Domain.Entities;
public sealed class UsoBateria
{
 public long Id{get;set;}public int TuboId{get;set;}public Tubo Tubo{get;set;}=null!;
 public int UbicacionBateriaId{get;set;}public Ubicacion UbicacionBateria{get;set;}=null!;public int UbicacionOrigenId{get;set;}public int?UbicacionRetiroId{get;set;}
 public string CodigoTubo{get;set;}="";public string Gas{get;set;}="";public decimal CapacidadNominal{get;set;}public string Unidad{get;set;}="";
 public DateTime FechaIngresoUtc{get;set;}=DateTime.UtcNow;public DateTime?FechaRetiroUtc{get;set;}public string?ResultadoRetiro{get;set;}
 public string UsuarioIngreso{get;set;}="";public string UsuarioIngresoId{get;set;}="";public string?UsuarioRetiro{get;set;}public string?UsuarioRetiroId{get;set;}public string?MotivoRetiro{get;set;}
}
