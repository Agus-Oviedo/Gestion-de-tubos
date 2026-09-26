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
}
