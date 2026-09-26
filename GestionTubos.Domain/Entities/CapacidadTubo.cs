namespace GestionTubos.Domain.Entities;

public sealed class CapacidadTubo
{
    public int Id { get; set; }
    public decimal Valor { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activa { get; set; } = true;
}
