using System.Net;
using System.Net.Http.Json;
using GestionTubos.Shared.Catalogos;

namespace GestionTubos.Client.Services;

public sealed class CatalogoApiService(HttpClient http)
{
    public async Task<List<TipoGasDto>> GasesAsync()=>await http.GetFromJsonAsync<List<TipoGasDto>>("api/catalogos/gases")??[];
    public Task CrearGasAsync(TipoGasGuardarDto x)=>Enviar("api/catalogos/gases",x);
    public Task EliminarGasAsync(int id)=>Eliminar($"api/catalogos/gases/{id}");
    public async Task<List<CapacidadDto>> CapacidadesAsync()=>await http.GetFromJsonAsync<List<CapacidadDto>>("api/catalogos/capacidades")??[];
    public Task CrearCapacidadAsync(CapacidadGuardarDto x)=>Enviar("api/catalogos/capacidades",x);
    public Task EliminarCapacidadAsync(int id)=>Eliminar($"api/catalogos/capacidades/{id}");
    public async Task<List<UbicacionDto>> UbicacionesAsync()=>await http.GetFromJsonAsync<List<UbicacionDto>>("api/catalogos/ubicaciones")??[];
    public Task CrearUbicacionAsync(UbicacionGuardarDto x)=>Enviar("api/catalogos/ubicaciones",x);
    public async Task EditarUbicacionAsync(int id,UbicacionGuardarDto x)=>await Verificar(await http.PutAsJsonAsync($"api/catalogos/ubicaciones/{id}",x));
    public Task EliminarUbicacionAsync(int id)=>Eliminar($"api/catalogos/ubicaciones/{id}");
    private async Task Enviar<T>(string url,T dto){var r=await http.PostAsJsonAsync(url,dto);await Verificar(r);}
    private async Task Eliminar(string url){var r=await http.DeleteAsync(url);await Verificar(r);}
    private static async Task Verificar(HttpResponseMessage r){if(r.IsSuccessStatusCode)return;if(r.StatusCode==HttpStatusCode.Conflict){var e=await r.Content.ReadFromJsonAsync<Error>();throw new InvalidOperationException(e?.Mensaje??"Registro duplicado.");}throw new InvalidOperationException($"Error de API ({(int)r.StatusCode}).");}
    private sealed record Error(string Mensaje);
}
