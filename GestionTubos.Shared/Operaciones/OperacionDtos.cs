using System.ComponentModel.DataAnnotations;

namespace GestionTubos.Shared.Operaciones;

public sealed record MovimientoDto(long Id, int CilindroId, string CodigoCilindro, DateTime FechaHoraUtc, string Tipo, string? Origen, string? Destino, string? Cliente, string? EstadoAnterior, string EstadoPosterior, string Usuario, string? Observaciones);

public sealed class PruebaHidraulicaGuardarDto
{
    [Range(1, int.MaxValue)] public int CilindroId { get; set; }
    public DateOnly FechaRealizacion { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly FechaVencimiento { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddYears(5));
    public bool Aprobada { get; set; } = true;
    [Required, StringLength(200)] public string EntidadRealizadora { get; set; } = string.Empty;
    [StringLength(100)] public string? NumeroCertificado { get; set; }
    [StringLength(1000)] public string? Observaciones { get; set; }
}
public sealed record PruebaHidraulicaDto(int Id, int CilindroId, string CodigoCilindro, DateOnly FechaRealizacion, DateOnly FechaVencimiento, bool Aprobada, string Estado, string EntidadRealizadora, string? NumeroCertificado, string? Observaciones);

public sealed class EntregaGuardarDto
{
    [Range(1, int.MaxValue)] public int ClienteId { get; set; }
    [Range(1, int.MaxValue)] public int CilindroId { get; set; }
    [Required] public string Modalidad { get; set; } = "Alquiler";
    [Range(0, 3650)] public int DiasSinCargo { get; set; }
    [StringLength(150)] public string? PersonaRecibe { get; set; }
    [StringLength(1000)] public string? Observaciones { get; set; }
}
public sealed class RecepcionGuardarDto
{
    [Range(1, int.MaxValue)] public long OperacionId { get; set; }
    [Range(1, 4)] public int EstadoFisico { get; set; } = 1;
    [Range(1, 4)] public int EstadoContenido { get; set; } = 2;
    [Range(1, int.MaxValue)] public int UbicacionDestinoId { get; set; }
    public bool RequiereInspeccion { get; set; }
    [StringLength(1000)] public string? Observaciones { get; set; }
}
public sealed record OperacionDto(long Id, string Numero, string Tipo, int ClienteId, string Cliente, int CilindroId, string CodigoCilindro, DateTime FechaHoraUtc, string Estado, DateOnly? FechaLimite, DateTime? FechaDevolucionUtc, string? PersonaRecibe, string? Observaciones);

public sealed record DashboardDto(int InventarioPropioTotal,int PropiosEnPlanta,int PropiosEnClientes,int CilindrosClientesEnPlanta,int EnCarga,int EnReparto,int PendientesInspeccion,int PruebasVencidas,int PruebasProximas,int OperacionesAbiertas,IReadOnlyList<MovimientoDto>UltimosMovimientos);

public sealed class CargaIniciarDto
{
    [Range(1,int.MaxValue)] public int CilindroId { get; set; }
    [StringLength(1000)] public string? Observaciones { get; set; }
}
public sealed class CargaFinalizarDto
{
    [Range(1,int.MaxValue)] public int CilindroId { get; set; }
    [StringLength(1000)] public string? Observaciones { get; set; }
}
public sealed class CargaCancelarDto
{
    [Range(1,int.MaxValue)] public int CilindroId { get; set; }
    [Required] public string Destino { get; set; } = "Disponible";
    [Required,StringLength(500)] public string Motivo { get; set; } = string.Empty;
}
public sealed class IngresoCilindroClienteDto
{
    [Range(1,int.MaxValue)] public int ClienteId { get; set; }
    [Range(1,int.MaxValue)] public int CilindroId { get; set; }
    [Required] public string Motivo { get; set; } = "Carga";
    [Range(1,4)] public int EstadoFisico { get; set; } = 1;
    [Range(1,4)] public int EstadoContenido { get; set; } = 2;
    [Range(1,int.MaxValue)] public int UbicacionDestinoId { get; set; }
    [StringLength(1000)] public string? Observaciones { get; set; }
}
