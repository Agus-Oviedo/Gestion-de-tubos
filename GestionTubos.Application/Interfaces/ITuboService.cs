using GestionTubos.Shared.Tubos;
namespace GestionTubos.Application.Interfaces;
public interface ITuboService
{
 Task UbicarPropiosAsync(TubosUbicarDto dto,string usuario,CancellationToken ct);
 Task<IReadOnlyList<TuboDto>> ObtenerTodosAsync(string? buscar,CancellationToken ct);Task<TuboDto?> ObtenerAsync(int id,CancellationToken ct);Task<TuboDto> CrearAsync(TuboGuardarDto dto,CancellationToken ct);Task<TuboDto?> ActualizarAsync(int id,TuboGuardarDto dto,CancellationToken ct);Task<bool> EliminarAsync(int id,CancellationToken ct);
}
