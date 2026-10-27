using GestionTubos.Shared.Pedidos;

namespace GestionTubos.Application.Interfaces;
public interface IPedidoService
{
    Task<IReadOnlyList<PedidoDto>> ObtenerAsync(CancellationToken ct);
    Task<PedidoDto> CrearAsync(PedidoCrearDto dto, string usuario, CancellationToken ct);
    Task<PedidoDto> CrearYEntregarAsync(PedidoCrearDto dto, string usuario, CancellationToken ct);
    Task<RemitoDto> EmitirRemitoAsync(long pedidoId, string usuario, CancellationToken ct);
    Task EliminarRemitoAsync(long pedidoId, CancellationToken ct,string usuario="Sistema",string motivo="Eliminación de borrador solicitada");
    Task RegistrarEstadosInicialesAsync(CancellationToken ct);
    Task<PedidoDto> EditarAsync(long id,RemitoEditarDto dto,string usuario,CancellationToken ct);
    Task<PaginaAuditoriaDto> AuditoriaAsync(long? pedidoId,string? buscar,long? anteriorA,CancellationToken ct);
    Task<PedidoDto> ConfirmarEntregaAsync(long pedidoId, PedidoEntregarDto dto, string usuario, CancellationToken ct);
}
