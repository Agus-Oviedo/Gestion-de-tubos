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
    public async Task<OperacionDto> Recibir(RecepcionGuardarDto dto) { var response = await http.PostAsJsonAsync("api/operaciones/recepciones", dto); if (response.IsSuccessStatusCode) return (await response.Content.ReadFromJsonAsync<OperacionDto>())!; await LanzarError(response); throw new InvalidOperationException(); }
    public async Task<List<RemitoHistorialDto>> Remitos(int? clienteId = null, int? tuboId = null, string? tipo = null)
    {
        var partes = new List<string>(); if (clienteId.HasValue) partes.Add($"clienteId={clienteId}"); if (tuboId.HasValue) partes.Add($"tuboId={tuboId}"); if (!string.IsNullOrWhiteSpace(tipo)) partes.Add($"tipo={Uri.EscapeDataString(tipo)}");
        return await http.GetFromJsonAsync<List<RemitoHistorialDto>>("api/operaciones/remitos" + (partes.Count == 0 ? "" : "?" + string.Join("&", partes))) ?? [];
    }
    public async Task<DevolucionLoteDto> CrearDevolucionLote(DevolucionLoteCrearDto dto){var r=await http.PostAsJsonAsync("api/operaciones/devoluciones-lote",dto);if(r.IsSuccessStatusCode)return(await r.Content.ReadFromJsonAsync<DevolucionLoteDto>())!;await LanzarError(r);throw new InvalidOperationException();}
    public async Task<DevolucionLoteDto?> ObtenerDevolucionLote(long id)=>await http.GetFromJsonAsync<DevolucionLoteDto>($"api/operaciones/devoluciones-lote/{id}");
    public async Task<byte[]> DescargarDevolucionPdf(long id)=>await http.GetByteArrayAsync($"api/operaciones/devoluciones-lote/{id}/pdf");
    public async Task<DevolucionLoteDto> FirmarDevolucionLote(long id,DevolucionLoteFirmarDto dto){var r=await http.PostAsJsonAsync($"api/operaciones/devoluciones-lote/{id}/firma",dto);if(r.IsSuccessStatusCode)return(await r.Content.ReadFromJsonAsync<DevolucionLoteDto>())!;await LanzarError(r);throw new InvalidOperationException();}
    public Task<EstadisticasClienteDto?> EstadisticasCliente(int clienteId) => http.GetFromJsonAsync<EstadisticasClienteDto>($"api/operaciones/estadisticas-clientes/{clienteId}");
    public async Task<DashboardDto> Dashboard() => (await http.GetFromJsonAsync<DashboardDto>("api/operaciones/dashboard"))!;
    public Task IniciarCarga(CargaIniciarDto dto)=>Post("api/operaciones/cargas/iniciar",dto);
    public Task FinalizarCarga(CargaFinalizarDto dto)=>Post("api/operaciones/cargas/finalizar",dto);
    public Task CancelarCarga(CargaCancelarDto dto)=>Post("api/operaciones/cargas/cancelar",dto);
    public Task IngresarCilindroCliente(IngresoCilindroClienteDto dto)=>Post("api/operaciones/ingresos-cliente",dto);
    private async Task Post<T>(string url, T dto) { var response = await http.PostAsJsonAsync(url, dto); if (response.IsSuccessStatusCode) return; await LanzarError(response); }
    private static async Task LanzarError(HttpResponseMessage response) { if (response.StatusCode == HttpStatusCode.Conflict) { var error = await response.Content.ReadFromJsonAsync<ErrorApi>(); throw new InvalidOperationException(error?.Mensaje); } throw new InvalidOperationException($"Error de API ({(int)response.StatusCode})."); }
    private sealed record ErrorApi(string Mensaje);
}
