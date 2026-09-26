using GestionTubos.Shared.Catalogos;

namespace GestionTubos.Application.Interfaces;

public interface ICatalogoService
{
    Task<IReadOnlyList<TipoGasDto>> ObtenerGasesAsync(CancellationToken ct);
    Task<TipoGasDto> CrearGasAsync(TipoGasGuardarDto dto, CancellationToken ct);
    Task<bool> EliminarGasAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<CapacidadDto>> ObtenerCapacidadesAsync(CancellationToken ct);
    Task<CapacidadDto> CrearCapacidadAsync(CapacidadGuardarDto dto, CancellationToken ct);
    Task<bool> EliminarCapacidadAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<UbicacionDto>> ObtenerUbicacionesAsync(CancellationToken ct);
    Task<UbicacionDto> CrearUbicacionAsync(UbicacionGuardarDto dto, CancellationToken ct);
    Task<UbicacionDto?> ActualizarUbicacionAsync(int id, UbicacionGuardarDto dto, CancellationToken ct);
    Task<bool> EliminarUbicacionAsync(int id, CancellationToken ct);
}
