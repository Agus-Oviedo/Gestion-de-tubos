using GestionTubos.Shared.Pedidos;

namespace GestionTubos.Application.Interfaces;
public interface IPedidoService
{
    Task<IReadOnlyList<PedidoDto>> ObtenerAsync(CancellationToken ct);
    Task<PedidoDto> CrearAsync(PedidoCrearDto dto, string usuario, CancellationToken ct);
    Task<RemitoDto> EmitirRemitoAsync(long pedidoId, string usuario, CancellationToken ct);
    Task<PedidoDto> ConfirmarEntregaAsync(long pedidoId, PedidoEntregarDto dto, string usuario, CancellationToken ct);
}
