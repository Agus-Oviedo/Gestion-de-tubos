namespace GestionTubos.Domain.Entities;

public sealed class OperacionCilindro
{
    public long Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public int TuboId { get; set; }
    public Tubo Tubo { get; set; } = null!;
    public DateTime FechaHoraUtc { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "Confirmada";
    public int? DiasSinCargo { get; set; }
    public DateOnly? FechaLimite { get; set; }
    public DateTime? FechaDevolucionUtc { get; set; }
    public string? PersonaRecibe { get; set; }
    public string? Observaciones { get; set; }
    public long? RemitoEntregaId { get; set; }
    public Remito? RemitoEntrega { get; set; }
    public RemitoDevolucion? RemitoDevolucion { get; set; }
}

public sealed class RemitoDevolucion
{
    public long Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public long OperacionCilindroId { get; set; }
    public OperacionCilindro OperacionCilindro { get; set; } = null!;
    public DateTime FechaEmisionUtc { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "Emitido";
    public string UsuarioReceptor { get; set; } = string.Empty;
    public string? PersonaEntrega { get; set; }
    public string? DniEntrega { get; set; }
    public string? Observaciones { get; set; }
}

public sealed class DevolucionLote
{
    public long Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public long RemitoEntregaId { get; set; }
    public Remito RemitoEntrega { get; set; } = null!;
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaConfirmacionUtc { get; set; }
    public string Estado { get; set; } = "Pendiente de firma";
    public string UsuarioReceptor { get; set; } = string.Empty;
    public string? PersonaEntrega { get; set; }
    public string? DniEntrega { get; set; }
    public string? FirmaDataUrl { get; set; }
    public string? Observaciones { get; set; }
    public ICollection<DevolucionLoteDetalle> Detalles { get; set; } = [];
}

public sealed class DevolucionLoteDetalle
{
    public long Id { get; set; }
    public long DevolucionLoteId { get; set; }
    public DevolucionLote DevolucionLote { get; set; } = null!;
    public long OperacionCilindroId { get; set; }
    public OperacionCilindro OperacionCilindro { get; set; } = null!;
    public int EstadoFisico { get; set; }
    public int EstadoContenido { get; set; }
    public int UbicacionDestinoId { get; set; }
    public bool RequiereInspeccion { get; set; }
}
