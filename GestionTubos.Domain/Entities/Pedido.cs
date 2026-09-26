namespace GestionTubos.Domain.Entities;

public sealed class Pedido
{
    public long Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public DateTime FechaHoraUtc { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "Pendiente";
    public string UsuarioCreador { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string ModalidadEntrega { get; set; } = "RetiroPlanta";
    public int? DireccionEntregaId { get; set; }
    public string? DireccionEntregaTexto { get; set; }
    public DateOnly? FechaPrevistaEntrega { get; set; }
    public string? ContactoEntrega { get; set; }
    public string? TelefonoEntrega { get; set; }
    public ICollection<PedidoDetalle> Detalles { get; set; } = [];
    public Remito? Remito { get; set; }
    public RepartoDetalle? RepartoDetalle { get; set; }
}

public sealed class Reparto
{
    public long Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string Responsable { get; set; } = string.Empty;
    public string Estado { get; set; } = "Preparado";
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaSalidaUtc { get; set; }
    public string UsuarioCreador { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public ICollection<RepartoDetalle> Detalles { get; set; } = [];
}

public sealed class RepartoDetalle
{
    public long Id { get; set; }
    public long RepartoId { get; set; }
    public Reparto Reparto { get; set; } = null!;
    public long PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public string Estado { get; set; } = "Asignado";
}

public sealed class PedidoDetalle
{
    public long Id { get; set; }
    public long PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public int TuboId { get; set; }
    public Tubo Tubo { get; set; } = null!;
}

public sealed class Remito
{
    public long Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public long PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public DateTime FechaEmisionUtc { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "Emitido";
    public string UsuarioEmisor { get; set; } = string.Empty;
    public string? PersonaRecibe { get; set; }
    public string? DniRecibe { get; set; }
    public string? FirmaDataUrl { get; set; }
    public DateTime? FechaFirmaUtc { get; set; }
    public string? UsuarioEntrega { get; set; }
    public string? HuellaDocumento { get; set; }
}
