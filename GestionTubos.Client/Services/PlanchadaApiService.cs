using System.Net.Http.Json;
using GestionTubos.Shared.Operaciones;
namespace GestionTubos.Client.Services;
public sealed class PlanchadaApiService(HttpClient http)
{
 public async Task<List<SectorBateriaDto>>Sectores()=>await http.GetFromJsonAsync<List<SectorBateriaDto>>("api/planchadas/sectores")??[];
 public async Task<List<UsoBateriaDto>>Activos()=>await http.GetFromJsonAsync<List<UsoBateriaDto>>("api/planchadas/activos")??[];
 public async Task<List<CandidatoBateriaDto>>Candidatos(int sectorId)=>await http.GetFromJsonAsync<List<CandidatoBateriaDto>>($"api/planchadas/candidatos?sectorId={sectorId}")??[];
 public Task Ingresar(BateriaIngresoDto dto)=>Post("api/planchadas/ingresos",dto);public Task Retirar(BateriaRetiroDto dto)=>Post("api/planchadas/retiros",dto);
 public async Task<ConsumoInternoDto>Consumo(DateOnly desde,DateOnly hasta,int?sectorId)=>await Leer<ConsumoInternoDto>($"api/planchadas/consumo?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}&sectorId={sectorId}");
 public async Task<HistorialBateriaDto>Historial(DateOnly desde,DateOnly hasta,int?sectorId,long?anteriorA)=>await Leer<HistorialBateriaDto>($"api/planchadas/historial?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}&sectorId={sectorId}&anteriorA={anteriorA}");
 private async Task<T>Leer<T>(string url){var r=await http.GetAsync(url);await Revisar(r);return(await r.Content.ReadFromJsonAsync<T>())!;}
 private async Task Post<T>(string url,T dto){var r=await http.PostAsJsonAsync(url,dto);await Revisar(r);}
 private static async Task Revisar(HttpResponseMessage r){if(r.IsSuccessStatusCode)return;if(r.StatusCode==System.Net.HttpStatusCode.Conflict){var e=await r.Content.ReadFromJsonAsync<Dictionary<string,string>>();throw new InvalidOperationException(e?.GetValueOrDefault("mensaje")??"La operación no pudo completarse.");}throw new InvalidOperationException($"Error de API ({(int)r.StatusCode}).");}
}
