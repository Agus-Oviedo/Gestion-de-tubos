using GestionTubos.Shared.Repartos;
namespace GestionTubos.Application.Interfaces;
public interface IRepartoService
{
 Task<IReadOnlyList<RepartoDto>> ObtenerAsync(CancellationToken ct);
 Task<RepartoDto> CrearAsync(RepartoCrearDto dto,string usuario,CancellationToken ct);
 Task<RepartoDto> IniciarAsync(long id,string usuario,CancellationToken ct);
 Task<RepartoDto> AnularAsync(long id,RepartoAnularDto dto,string usuario,CancellationToken ct);
 Task<RepartoDto> NoEntregadoAsync(long id,long pedidoId,RepartoNoEntregadoDto dto,string usuario,CancellationToken ct);
}
