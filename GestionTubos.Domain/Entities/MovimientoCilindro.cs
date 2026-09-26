namespace GestionTubos.Domain.Entities;

public sealed class MovimientoCilindro
{
    public long Id { get; set; }
    public int TuboId { get; set; }
    public Tubo Tubo { get; set; } = null!;
    public DateTime FechaHoraUtc { get; set; } = DateTime.UtcNow;
    public string Tipo { get; set; } = string.Empty;
    public int? UbicacionOrigenId { get; set; }
    public int? UbicacionDestinoId { get; set; }
    public int? ClienteId { get; set; }
    public string? EstadoAnterior { get; set; }
    public string EstadoPosterior { get; set; } = string.Empty;
    public string UsuarioResponsable { get; set; } = "Sistema";
    public string? Observaciones { get; set; }
}
