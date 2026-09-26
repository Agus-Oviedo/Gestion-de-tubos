using GestionTubos.Shared.Operaciones;

namespace GestionTubos.Application.Interfaces;

public interface IOperacionService
{
    Task<IReadOnlyList<MovimientoDto>> MovimientosAsync(int? cilindroId, CancellationToken ct);
    Task<IReadOnlyList<PruebaHidraulicaDto>> PruebasAsync(CancellationToken ct);
    Task<PruebaHidraulicaDto> RegistrarPruebaAsync(PruebaHidraulicaGuardarDto dto, CancellationToken ct);
    Task<IReadOnlyList<OperacionDto>> OperacionesAsync(bool soloAbiertas, CancellationToken ct);
    Task<OperacionDto> EntregarAsync(EntregaGuardarDto dto, CancellationToken ct);
    Task<OperacionDto> RecibirAsync(RecepcionGuardarDto dto, CancellationToken ct);
    Task<DashboardDto> DashboardAsync(CancellationToken ct);
    Task IniciarCargaAsync(CargaIniciarDto dto, string usuario, CancellationToken ct);
    Task FinalizarCargaAsync(CargaFinalizarDto dto, string usuario, CancellationToken ct);
    Task CancelarCargaAsync(CargaCancelarDto dto, string usuario, CancellationToken ct);
    Task IngresarCilindroClienteAsync(IngresoCilindroClienteDto dto, string usuario, CancellationToken ct);
}
