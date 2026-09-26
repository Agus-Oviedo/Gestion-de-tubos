namespace GestionTubos.Domain.Entities;

public sealed class Ubicacion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EsInterna { get; set; } = true;
    public bool Activa { get; set; } = true;
}
