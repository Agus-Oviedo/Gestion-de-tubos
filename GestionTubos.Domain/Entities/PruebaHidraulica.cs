namespace GestionTubos.Domain.Entities;

public sealed class PruebaHidraulica
{
    public int Id { get; set; }
    public int TuboId { get; set; }
    public Tubo Tubo { get; set; } = null!;
    public DateOnly FechaRealizacion { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public bool Aprobada { get; set; }
    public string EntidadRealizadora { get; set; } = string.Empty;
    public string? NumeroCertificado { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaRegistroUtc { get; set; } = DateTime.UtcNow;
}
