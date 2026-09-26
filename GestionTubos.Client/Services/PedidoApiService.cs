using System.Net;
using System.Net.Http.Json;
using GestionTubos.Shared.Pedidos;
namespace GestionTubos.Client.Services;
public sealed class PedidoApiService(HttpClient http)
{
    public async Task<List<PedidoDto>> Obtener()=>await http.GetFromJsonAsync<List<PedidoDto>>("api/pedidos")??[];
    public Task Crear(PedidoCrearDto dto)=>Post("api/pedidos",dto);
    public Task EmitirRemito(long id)=>Post($"api/pedidos/{id}/remito",new{});
    public Task ConfirmarEntrega(long id,PedidoEntregarDto dto)=>Post($"api/pedidos/{id}/entrega",dto);
    private async Task Post<T>(string url,T dto){var r=await http.PostAsJsonAsync(url,dto);if(r.IsSuccessStatusCode)return;if(r.StatusCode==HttpStatusCode.Conflict){var e=await r.Content.ReadFromJsonAsync<Dictionary<string,string>>();throw new InvalidOperationException(e?.GetValueOrDefault("mensaje"));}throw new InvalidOperationException($"Error de API ({(int)r.StatusCode}).");}
}
