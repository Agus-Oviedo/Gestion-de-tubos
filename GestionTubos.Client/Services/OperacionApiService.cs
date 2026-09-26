using System.Net;
using System.Net.Http.Json;
using GestionTubos.Shared.Operaciones;

namespace GestionTubos.Client.Services;

public sealed class OperacionApiService(HttpClient http)
{
    public async Task<List<MovimientoDto>> Movimientos(int? cilindroId = null) => await http.GetFromJsonAsync<List<MovimientoDto>>(cilindroId.HasValue ? $"api/operaciones/movimientos?cilindroId={cilindroId}" : "api/operaciones/movimientos") ?? [];
    public async Task<List<PruebaHidraulicaDto>> Pruebas() => await http.GetFromJsonAsync<List<PruebaHidraulicaDto>>("api/operaciones/pruebas-hidraulicas") ?? [];
    public Task RegistrarPrueba(PruebaHidraulicaGuardarDto dto) => Post("api/operaciones/pruebas-hidraulicas", dto);
    public async Task<List<OperacionDto>> Operaciones(bool abiertas = false) => await http.GetFromJsonAsync<List<OperacionDto>>($"api/operaciones?soloAbiertas={abiertas.ToString().ToLowerInvariant()}") ?? [];
    public Task Entregar(EntregaGuardarDto dto) => Post("api/operaciones/entregas", dto);
    public Task Recibir(RecepcionGuardarDto dto) => Post("api/operaciones/recepciones", dto);
    public async Task<DashboardDto> Dashboard() => (await http.GetFromJsonAsync<DashboardDto>("api/operaciones/dashboard"))!;
    public Task IniciarCarga(CargaIniciarDto dto)=>Post("api/operaciones/cargas/iniciar",dto);
    public Task FinalizarCarga(CargaFinalizarDto dto)=>Post("api/operaciones/cargas/finalizar",dto);
    public Task CancelarCarga(CargaCancelarDto dto)=>Post("api/operaciones/cargas/cancelar",dto);
    public Task IngresarCilindroCliente(IngresoCilindroClienteDto dto)=>Post("api/operaciones/ingresos-cliente",dto);
    private async Task Post<T>(string url, T dto) { var response = await http.PostAsJsonAsync(url, dto); if (response.IsSuccessStatusCode) return; if (response.StatusCode == HttpStatusCode.Conflict) { var error = await response.Content.ReadFromJsonAsync<ErrorApi>(); throw new InvalidOperationException(error?.Mensaje); } throw new InvalidOperationException($"Error de API ({(int)response.StatusCode})."); }
    private sealed record ErrorApi(string Mensaje);
}
