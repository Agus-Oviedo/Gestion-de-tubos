using System.ComponentModel.DataAnnotations;

namespace GestionTubos.Shared.Catalogos;

public sealed class TipoGasGuardarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)] public string Nombre { get; set; } = string.Empty;
    [StringLength(300)] public string? Descripcion { get; set; }
}
public sealed record TipoGasDto(int Id, string Nombre, string? Descripcion);

public sealed class CapacidadGuardarDto
{
    [Range(0.01, 999999, ErrorMessage = "Ingresá una capacidad mayor a cero.")]
    public decimal Valor { get; set; }
    [Required(ErrorMessage = "La unidad es obligatoria.")]
    [StringLength(20)] public string Unidad { get; set; } = "m³";
    [StringLength(200)] public string? Descripcion { get; set; }
}
public sealed record CapacidadDto(int Id, decimal Valor, string Unidad, string? Descripcion);

public sealed class UbicacionGuardarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)] public string Nombre { get; set; } = string.Empty;
    [StringLength(300)] public string? Descripcion { get; set; }
    public bool EsInterna { get; set; } = true;
}
public sealed record UbicacionDto(int Id, string Nombre, string? Descripcion, bool EsInterna);
