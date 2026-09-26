namespace GestionTubos.Domain.Entities;

public sealed class DireccionCliente
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Calle { get; set; } = string.Empty;
    public string? Numero { get; set; }
    public string? PisoDepartamento { get; set; }
    public string Localidad { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string? CodigoPostal { get; set; }
    public bool EsFiscal { get; set; }
    public bool EsPrincipalEntrega { get; set; }
    public bool Activa { get; set; } = true;
    public Cliente Cliente { get; set; } = null!;
}
