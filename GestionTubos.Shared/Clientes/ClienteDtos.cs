using System.ComponentModel.DataAnnotations;

namespace GestionTubos.Shared.Clientes;

public sealed class ClienteGuardarDto : IValidatableObject
{
    [Range(1, 2, ErrorMessage = "Seleccioná el tipo de cliente.")]
    public int Tipo { get; set; } = 1;

    [StringLength(100)] public string? Nombre { get; set; }
    [StringLength(100)] public string? Apellido { get; set; }
    [StringLength(200)] public string? RazonSocial { get; set; }
    [StringLength(200)] public string? NombreComercial { get; set; }
    [StringLength(20)] public string? Dni { get; set; }
    [StringLength(20)] public string? CuitCuil { get; set; }
    [StringLength(150)] public string? Responsable { get; set; }
    [StringLength(50)] public string? Telefono { get; set; }
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(254)] public string? Correo { get; set; }
    [Range(1, 5, ErrorMessage = "Seleccioná la condición fiscal.")]
    public int CondicionFiscal { get; set; } = 5;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Tipo == 1 && string.IsNullOrWhiteSpace(Nombre))
            yield return new ValidationResult("El nombre es obligatorio para una persona.", [nameof(Nombre)]);
        if (Tipo == 1 && string.IsNullOrWhiteSpace(Apellido))
            yield return new ValidationResult("El apellido es obligatorio para una persona.", [nameof(Apellido)]);
        if (Tipo == 2 && string.IsNullOrWhiteSpace(RazonSocial))
            yield return new ValidationResult("La razón social es obligatoria para una empresa.", [nameof(RazonSocial)]);
        if (Tipo == 2 && string.IsNullOrWhiteSpace(CuitCuil))
            yield return new ValidationResult("El CUIT es obligatorio para una empresa.", [nameof(CuitCuil)]);
    }
}

public sealed record ClienteDto(
    int Id, int Tipo, string NombreMostrar, string? Nombre, string? Apellido,
    string? RazonSocial, string? NombreComercial, string? Dni, string? CuitCuil,
    string? Responsable, string? Telefono, string? Correo, int CondicionFiscal);

public sealed record DireccionClienteDto(
    int Id, string Calle, string? Numero, string Localidad, string Provincia,
    bool EsFiscal, bool EsPrincipalEntrega);

public sealed class DireccionClienteGuardarDto
{
    [Required(ErrorMessage = "La calle es obligatoria.")]
    [StringLength(150)] public string Calle { get; set; } = string.Empty;
    [StringLength(20)] public string? Numero { get; set; }
    [StringLength(50)] public string? PisoDepartamento { get; set; }
    [Required(ErrorMessage = "La localidad es obligatoria.")]
    [StringLength(100)] public string Localidad { get; set; } = string.Empty;
    [Required(ErrorMessage = "La provincia es obligatoria.")]
    [StringLength(100)] public string Provincia { get; set; } = string.Empty;
    [StringLength(12)] public string? CodigoPostal { get; set; }
    public bool EsFiscal { get; set; }
    public bool EsPrincipalEntrega { get; set; }
}
