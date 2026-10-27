using System.ComponentModel.DataAnnotations;
namespace GestionTubos.Shared.Operaciones;
public sealed class BateriaIngresoDto:IValidatableObject
{
 [Range(1,int.MaxValue)]public int SectorId{get;set;}[Required]public List<int>TuboIds{get;set;}=[];
 public IEnumerable<ValidationResult>Validate(ValidationContext c){if(TuboIds is null||TuboIds.Count is <1 or >500)yield return new("Seleccioná entre 1 y 500 tubos.");}
}
public sealed class BateriaRetiroDto:IValidatableObject
{
 [Required]public List<long>UsoIds{get;set;}=[];[Required]public string Resultado{get;set;}="Agotado";[Range(1,int.MaxValue)]public int UbicacionDestinoId{get;set;}[StringLength(1000)]public string?Motivo{get;set;}
 public IEnumerable<ValidationResult>Validate(ValidationContext c){if(UsoIds is null||UsoIds.Count is <1 or >500)yield return new("Seleccioná entre 1 y 500 tubos de la batería.");if(Resultado is not("Agotado" or "Lleno" or "Parcial"))yield return new("Seleccioná el resultado del retiro.");if(Resultado!="Agotado"&&string.IsNullOrWhiteSpace(Motivo))yield return new("Indicá el motivo para retirar un tubo que aún tiene gas.",[nameof(Motivo)]);}
}
public sealed record SectorBateriaDto(int Id,string Nombre,string Gas,int TubosEnBateria);
public sealed record UsoBateriaDto(long Id,int TuboId,int SectorId,string Sector,string Codigo,string Gas,decimal CapacidadNominal,string Unidad,DateTime FechaIngresoUtc,DateTime?FechaRetiroUtc,string?ResultadoRetiro,string UsuarioIngreso,string?UsuarioRetiro,string?MotivoRetiro);
public sealed record ConsumoGasDto(string Gas,string Unidad,int TubosAgotados,decimal CapacidadNominalTotal,double DiasPromedioEnBateria);
public sealed record ConsumoMensualDto(string Mes,string Gas,string Unidad,int TubosAgotados,decimal CapacidadNominalTotal);
public sealed record ConsumoInternoDto(DateOnly Desde,DateOnly Hasta,IReadOnlyList<ConsumoGasDto>PorGas,IReadOnlyList<ConsumoMensualDto>PorMes);
public sealed record HistorialBateriaDto(IReadOnlyList<UsoBateriaDto>Registros,bool HayMas);

public sealed record CandidatoBateriaDto(int Id,string Codigo,string Gas,decimal CapacidadNominal,string Unidad,string Ubicacion);
