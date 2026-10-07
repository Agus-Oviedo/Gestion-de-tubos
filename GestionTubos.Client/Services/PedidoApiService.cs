using System.Net;
using System.Net.Http.Json;
using GestionTubos.Shared.Pedidos;
namespace GestionTubos.Client.Services;
public sealed class PedidoApiService(HttpClient http)
{
    public async Task<List<PedidoDto>> Obtener()=>await http.GetFromJsonAsync<List<PedidoDto>>("api/pedidos")??[];
    public async Task<PedidoDto> Crear(PedidoCrearDto dto){var r=await http.PostAsJsonAsync("api/pedidos",dto);if(r.IsSuccessStatusCode)return(await r.Content.ReadFromJsonAsync<PedidoDto>())!;await LanzarError(r);throw new InvalidOperationException();}
    public Task EmitirRemito(long id)=>Post($"api/pedidos/{id}/remito",new{});
    public Task ConfirmarEntrega(long id,PedidoEntregarDto dto)=>Post($"api/pedidos/{id}/entrega",dto);
    public async Task EliminarRemito(long id){var r=await http.DeleteAsync($"api/pedidos/{id}/remito");if(r.IsSuccessStatusCode)return;await LanzarError(r);}
    public async Task<byte[]> DescargarPdf(long id)=>await http.GetByteArrayAsync($"api/pedidos/{id}/pdf");
    private async Task Post<T>(string url,T dto){var r=await http.PostAsJsonAsync(url,dto);if(r.IsSuccessStatusCode)return;await LanzarError(r);}
    private static async Task LanzarError(HttpResponseMessage r){if(r.StatusCode==HttpStatusCode.Conflict){var e=await r.Content.ReadFromJsonAsync<Dictionary<string,string>>();throw new InvalidOperationException(e?.GetValueOrDefault("mensaje"));}throw new InvalidOperationException($"Error de API ({(int)r.StatusCode}).");}
}
