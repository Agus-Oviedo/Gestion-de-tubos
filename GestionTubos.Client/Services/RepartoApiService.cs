using System.Net;using System.Net.Http.Json;using GestionTubos.Shared.Repartos;
namespace GestionTubos.Client.Services;
public sealed class RepartoApiService(HttpClient http)
{
 public async Task<List<RepartoDto>>Obtener()=>await http.GetFromJsonAsync<List<RepartoDto>>("api/repartos")??[];
 public Task Crear(RepartoCrearDto d)=>Post("api/repartos",d);public Task Iniciar(long id)=>Post($"api/repartos/{id}/iniciar",new{});
 public Task Anular(long id,RepartoAnularDto d)=>Post($"api/repartos/{id}/anular",d);
 public Task NoEntregado(long id,long pedidoId,RepartoNoEntregadoDto d)=>Post($"api/repartos/{id}/pedidos/{pedidoId}/no-entregado",d);
 private async Task Post<T>(string u,T d){var r=await http.PostAsJsonAsync(u,d);if(r.IsSuccessStatusCode)return;if(r.StatusCode==HttpStatusCode.Conflict){var e=await r.Content.ReadFromJsonAsync<Dictionary<string,string>>();throw new InvalidOperationException(e?.GetValueOrDefault("mensaje"));}throw new InvalidOperationException($"Error de API ({(int)r.StatusCode}).");}
}
