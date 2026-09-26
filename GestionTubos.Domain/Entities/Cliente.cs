using GestionTubos.Domain.Enums;

namespace GestionTubos.Domain.Entities;

public sealed class Cliente
{
    public int Id { get; set; }
    public TipoCliente Tipo { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? RazonSocial { get; set; }
    public string? NombreComercial { get; set; }
    public string? Dni { get; set; }
    public string? CuitCuil { get; set; }
    public string? Responsable { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public CondicionFiscal CondicionFiscal { get; set; }
    public DateTime FechaAltaUtc { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
    public ICollection<DireccionCliente> Direcciones { get; set; } = [];
}
