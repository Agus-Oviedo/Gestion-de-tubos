using System.Net.Http.Json;using GestionTubos.Shared.Operaciones;
namespace GestionTubos.Client.Services;
public sealed class IngresoClienteApiService(HttpClient http)
{
 public Task<IngresoClienteDto?>Obtener(long id)=>http.GetFromJsonAsync<IngresoClienteDto>($"api/ingresos-clientes/{id}");
 public Task<byte[]>Pdf(long id)=>http.GetByteArrayAsync($"api/ingresos-clientes/{id}/pdf");
 public async Task<IngresoClienteDto>Crear(IngresoClienteCrearDto dto){var r=await http.PostAsJsonAsync("api/ingresos-clientes",dto);if(r.IsSuccessStatusCode)return(await r.Content.ReadFromJsonAsync<IngresoClienteDto>())!;if(r.StatusCode==System.Net.HttpStatusCode.Conflict){var e=await r.Content.ReadFromJsonAsync<Dictionary<string,string>>();throw new InvalidOperationException(e?.GetValueOrDefault("mensaje"));}throw new InvalidOperationException($"Error de API ({(int)r.StatusCode}).");}
}
