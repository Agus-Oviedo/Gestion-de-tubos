using GestionTubos.Shared.Clientes;

namespace GestionTubos.Application.Interfaces;

public interface IClienteService
{
    Task<IReadOnlyList<ClienteDto>> ObtenerTodosAsync(string? buscar, CancellationToken cancellationToken);
    Task<ClienteDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken);
    Task<ClienteDto> CrearAsync(ClienteGuardarDto dto, CancellationToken cancellationToken);
    Task<ClienteDto?> ActualizarAsync(int id, ClienteGuardarDto dto, CancellationToken cancellationToken);
    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<DireccionClienteDto>?> ObtenerDireccionesAsync(int clienteId, CancellationToken cancellationToken);
    Task<DireccionClienteDto?> CrearDireccionAsync(int clienteId, DireccionClienteGuardarDto dto, CancellationToken cancellationToken);
    Task<bool> EliminarDireccionAsync(int clienteId, int direccionId, CancellationToken cancellationToken);
}
