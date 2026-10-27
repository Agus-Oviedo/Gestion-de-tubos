using System.ComponentModel.DataAnnotations;

namespace GestionTubos.Shared.Operaciones;

public sealed record MovimientoDto(long Id, int CilindroId, string CodigoCilindro, DateTime FechaHoraUtc, string Tipo, string? Origen, string? Destino, string? Cliente, string? EstadoAnterior, string EstadoPosterior, string Usuario, string? Observaciones);

public sealed class PruebaHidraulicaGuardarDto
{
    [Range(1, int.MaxValue)] public int CilindroId { get; set; }
    public DateOnly FechaRealizacion { get; set; } = DateOnly.FromDateTime(GestionTubos.Shared.HoraArgentina.Hoy);
    public DateOnly FechaVencimiento { get; set; } = DateOnly.FromDateTime(GestionTubos.Shared.HoraArgentina.Hoy.AddYears(5));
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
    [StringLength(150)] public string? PersonaEntrega { get; set; }
    [StringLength(20)] public string? DniEntrega { get; set; }
    [StringLength(1000)] public string? Observaciones { get; set; }
}
public sealed record OperacionDto(long Id, string Numero, string Tipo, int ClienteId, string Cliente, int CilindroId, string CodigoCilindro, DateTime FechaHoraUtc, string Estado, DateOnly? FechaLimite, DateTime? FechaDevolucionUtc, string? PersonaRecibe, string? Observaciones, string? NumeroRemitoDevolucion = null, string? NumeroRemitoEntrega = null, long? RemitoEntregaId = null, string Gas = "", string Capacidad = "");

public sealed class DevolucionLoteCrearDto
{
    [Range(1,long.MaxValue)] public long RemitoEntregaId { get; set; }
    [MinLength(1,ErrorMessage="Seleccioná al menos un tubo.")] public List<long> OperacionIds { get; set; }=[];
    [Range(1,4)] public int EstadoFisico { get; set; }=1;
    [Range(1,4)] public int EstadoContenido { get; set; }=2;
    [Range(1,int.MaxValue)] public int UbicacionDestinoId { get; set; }
    public bool RequiereInspeccion { get; set; }
    [StringLength(1000)] public string? Observaciones { get; set; }
}
public sealed class DevolucionLoteFirmarDto
{
    [Required,StringLength(150)] public string PersonaEntrega { get; set; }=string.Empty;
    [Required,StringLength(20)] public string DniEntrega { get; set; }=string.Empty;
    [StringLength(400000)] public string FirmaDataUrl { get; set; }=string.Empty;
}
public sealed record DevolucionLoteDetalleDto(long OperacionId,int TuboId,string CodigoCilindro,string Gas,string Capacidad,int DiasEnPoderCliente = 0);
public sealed record DevolucionLoteDto(long Id,string Numero,long RemitoEntregaId,string NumeroRemitoEntrega,int ClienteId,string Cliente,DateTime FechaCreacionUtc,DateTime? FechaConfirmacionUtc,string Estado,string? PersonaEntrega,string? DniEntrega,string? FirmaDataUrl,string? Observaciones,IReadOnlyList<DevolucionLoteDetalleDto> Tubos);

public sealed record RemitoHistorialDto(long Id, string Tipo, string Numero, DateTime FechaUtc, string Estado, int ClienteId, string Cliente, long? PedidoId, string? NumeroPedido, long? OperacionId, string Usuario, string? Persona, string? Documento, string? Observaciones, IReadOnlyList<RemitoTuboDto> Tubos);
public sealed record RemitoTuboDto(int TuboId, string CodigoInterno, string Gas, string Capacidad);

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
