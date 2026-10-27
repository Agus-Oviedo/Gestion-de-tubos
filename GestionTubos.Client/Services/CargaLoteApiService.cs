using System.Net.Http.Json;
using GestionTubos.Shared.Operaciones;
namespace GestionTubos.Client.Services;
public sealed class CargaLoteApiService(HttpClient http)
{
 public async Task<List<VacioCargaDto>> Vacios()=>await http.GetFromJsonAsync<List<VacioCargaDto>>("api/cargas-lote/vacios")??[];
 public async Task<List<EnvioCargaDto>> Envios()=>await http.GetFromJsonAsync<List<EnvioCargaDto>>("api/cargas-lote")??[];
 public Task<EnvioCargaDto> Crear(EnvioCargaCrearDto dto)=>Post("api/cargas-lote",dto);
 public Task<EnvioCargaDto> ConfirmarEnvio(EnvioCargaCrearDto dto)=>Post("api/cargas-lote/confirmar-envio",dto);
 public Task<EnvioCargaDto> Salida(long id)=>Post($"api/cargas-lote/{id}/salida",new{});
 public Task<EnvioCargaDto> Editar(long id,EnvioCargaCrearDto dto)=>Post($"api/cargas-lote/{id}/editar",dto);
 public Task<EnvioCargaDto> Cancelar(long id)=>Post($"api/cargas-lote/{id}/cancelar",new{});
 public Task<EnvioCargaDto> Recibir(long id,RecepcionCargaCrearDto dto)=>Post($"api/cargas-lote/{id}/recepciones",dto);
 public Task<EnvioCargaDto> Resolver(long id,ResolucionCargaDto dto)=>Post($"api/cargas-lote/{id}/faltante",dto);
 public Task<byte[]> Pdf(long id,long? recepcion=null)=>http.GetByteArrayAsync(recepcion.HasValue?$"api/cargas-lote/{id}/recepciones/{recepcion}/pdf":$"api/cargas-lote/{id}/pdf");
 private async Task<EnvioCargaDto> Post<T>(string url,T dto){var r=await http.PostAsJsonAsync(url,dto);if(r.IsSuccessStatusCode)return(await r.Content.ReadFromJsonAsync<EnvioCargaDto>())!;var error=await r.Content.ReadFromJsonAsync<ErrorCarga>();throw new InvalidOperationException(error?.Mensaje??"No se pudo registrar la operación.");}
 private sealed record ErrorCarga(string Mensaje);
}
