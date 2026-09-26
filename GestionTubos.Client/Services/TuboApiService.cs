using System.Net;
using System.Net.Http.Json;
using GestionTubos.Shared.Tubos;

namespace GestionTubos.Client.Services;

public sealed class TuboApiService(HttpClient http)
{
    private const string Ruta = "api/cilindros";
    public async Task UbicarPropios(TubosUbicarDto dto) => await Verificar(await http.PostAsJsonAsync($"{Ruta}/ubicacion", dto));
    public async Task<List<TuboDto>> Todos(string? buscar = null) =>
        await http.GetFromJsonAsync<List<TuboDto>>(string.IsNullOrWhiteSpace(buscar) ? Ruta : $"{Ruta}?buscar={Uri.EscapeDataString(buscar)}") ?? [];
    public async Task<List<TuboDto>> PorCliente(int clienteId) => await http.GetFromJsonAsync<List<TuboDto>>($"{Ruta}?clientePropietarioId={clienteId}") ?? [];
    public Task<TuboDto?> Uno(int id) => http.GetFromJsonAsync<TuboDto>($"{Ruta}/{id}");
    public async Task<TuboDto> Crear(TuboGuardarDto dto) { var response=await http.PostAsJsonAsync(Ruta,dto);await Verificar(response);return (await response.Content.ReadFromJsonAsync<TuboDto>())!; }
    public async Task Editar(int id, TuboGuardarDto dto) => await Verificar(await http.PutAsJsonAsync($"{Ruta}/{id}", dto));
    public async Task Eliminar(int id) => await Verificar(await http.DeleteAsync($"{Ruta}/{id}"));
    private static async Task Verificar(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorApi>();
            throw new InvalidOperationException(error?.Mensaje);
        }
        throw new InvalidOperationException($"Error de API ({(int)response.StatusCode}).");
    }
    private sealed record ErrorApi(string Mensaje);
}
