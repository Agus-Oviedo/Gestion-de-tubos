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
    public async Task EliminarRemito(long id,string motivo){using var request=new HttpRequestMessage(HttpMethod.Delete,$"api/pedidos/{id}/remito"){Content=JsonContent.Create(new RemitoEliminarDto{Motivo=motivo})};var r=await http.SendAsync(request);if(r.IsSuccessStatusCode)return;await LanzarError(r);}
    public async Task Editar(long id,RemitoEditarDto dto){await LanzarSiError(await http.PutAsJsonAsync($"api/pedidos/{id}/remito",dto));}
    private static async Task LanzarSiError(HttpResponseMessage r){if(!r.IsSuccessStatusCode)await LanzarError(r);}
    public async Task<PaginaAuditoriaDto> Auditoria(long? pedidoId=null,string?buscar=null,long?anteriorA=null)=>await http.GetFromJsonAsync<PaginaAuditoriaDto>($"api/pedidos/auditoria?pedidoId={pedidoId}&buscar={Uri.EscapeDataString(buscar??"")}&anteriorA={anteriorA}")??new([],false);
    public async Task<byte[]> DescargarPdf(long id)=>await http.GetByteArrayAsync($"api/pedidos/{id}/pdf");
    private async Task Post<T>(string url,T dto){var r=await http.PostAsJsonAsync(url,dto);if(r.IsSuccessStatusCode)return;await LanzarError(r);}
    private static async Task LanzarError(HttpResponseMessage r){if(r.StatusCode==HttpStatusCode.Conflict){var e=await r.Content.ReadFromJsonAsync<Dictionary<string,string>>();throw new InvalidOperationException(e?.GetValueOrDefault("mensaje"));}throw new InvalidOperationException($"Error de API ({(int)r.StatusCode}).");}
}
