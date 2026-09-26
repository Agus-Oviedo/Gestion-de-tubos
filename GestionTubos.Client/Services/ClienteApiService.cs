using System.Net;
using System.Net.Http.Json;
using GestionTubos.Shared.Clientes;

namespace GestionTubos.Client.Services;

public sealed class ClienteApiService(HttpClient httpClient)
{
    public async Task<IReadOnlyList<ClienteDto>> ObtenerTodosAsync(string? buscar = null)
    {
        var url = string.IsNullOrWhiteSpace(buscar)
            ? "api/clientes"
            : $"api/clientes?buscar={Uri.EscapeDataString(buscar)}";
        return await httpClient.GetFromJsonAsync<List<ClienteDto>>(url) ?? [];
    }

    public Task<ClienteDto?> ObtenerPorIdAsync(int id) =>
        httpClient.GetFromJsonAsync<ClienteDto>($"api/clientes/{id}");

    public async Task<ClienteDto> CrearAsync(ClienteGuardarDto dto)
    {
        var response = await httpClient.PostAsJsonAsync("api/clientes", dto);
        await VerificarRespuestaAsync(response);
        return (await response.Content.ReadFromJsonAsync<ClienteDto>())!;
    }

    public async Task<ClienteDto> ActualizarAsync(int id, ClienteGuardarDto dto)
    {
        var response = await httpClient.PutAsJsonAsync($"api/clientes/{id}", dto);
        await VerificarRespuestaAsync(response);
        return (await response.Content.ReadFromJsonAsync<ClienteDto>())!;
    }

    public async Task EliminarAsync(int id)
    {
        var response = await httpClient.DeleteAsync($"api/clientes/{id}");
        await VerificarRespuestaAsync(response);
    }

    public async Task<IReadOnlyList<DireccionClienteDto>> ObtenerDireccionesAsync(int clienteId) =>
        await httpClient.GetFromJsonAsync<List<DireccionClienteDto>>($"api/clientes/{clienteId}/direcciones") ?? [];

    public async Task CrearDireccionAsync(int clienteId, DireccionClienteGuardarDto dto)
    {
        var response = await httpClient.PostAsJsonAsync($"api/clientes/{clienteId}/direcciones", dto);
        await VerificarRespuestaAsync(response);
    }

    public async Task EliminarDireccionAsync(int clienteId, int direccionId)
    {
        var response = await httpClient.DeleteAsync($"api/clientes/{clienteId}/direcciones/{direccionId}");
        await VerificarRespuestaAsync(response);
    }

    private static async Task VerificarRespuestaAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorApi>();
            throw new InvalidOperationException(error?.Mensaje ?? "El registro está duplicado.");
        }
        throw new InvalidOperationException($"La API respondió con el estado {(int)response.StatusCode}.");
    }

    private sealed record ErrorApi(string Mensaje);
}
