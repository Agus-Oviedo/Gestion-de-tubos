using GestionTubos.Shared.Operaciones;

namespace GestionTubos.Application.Interfaces;

public interface IOperacionService
{
    Task<IReadOnlyList<MovimientoDto>> MovimientosAsync(int? cilindroId, CancellationToken ct);
    Task<IReadOnlyList<PruebaHidraulicaDto>> PruebasAsync(CancellationToken ct);
    Task<PruebaHidraulicaDto> RegistrarPruebaAsync(PruebaHidraulicaGuardarDto dto, CancellationToken ct);
    Task<IReadOnlyList<OperacionDto>> OperacionesAsync(bool soloAbiertas, CancellationToken ct);
    Task<OperacionDto> EntregarAsync(EntregaGuardarDto dto, CancellationToken ct);
    Task<OperacionDto> RecibirAsync(RecepcionGuardarDto dto, string usuario, CancellationToken ct);
    Task<IReadOnlyList<RemitoHistorialDto>> RemitosAsync(int? clienteId, int? tuboId, string? tipo, CancellationToken ct);
    Task<DevolucionLoteDto> CrearDevolucionLoteAsync(DevolucionLoteCrearDto dto,string usuario,CancellationToken ct);
    Task<DevolucionLoteDto?> ObtenerDevolucionLoteAsync(long id,CancellationToken ct);
    Task<DevolucionLoteDto> FirmarDevolucionLoteAsync(long id,DevolucionLoteFirmarDto dto,string usuario,CancellationToken ct);
    Task<DashboardDto> DashboardAsync(CancellationToken ct);
    Task IniciarCargaAsync(CargaIniciarDto dto, string usuario, CancellationToken ct);
    Task FinalizarCargaAsync(CargaFinalizarDto dto, string usuario, CancellationToken ct);
    Task CancelarCargaAsync(CargaCancelarDto dto, string usuario, CancellationToken ct);
    Task IngresarCilindroClienteAsync(IngresoCilindroClienteDto dto, string usuario, CancellationToken ct);
}
