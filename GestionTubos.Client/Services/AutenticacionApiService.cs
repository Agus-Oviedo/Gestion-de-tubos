using System.Net;
using System.Net.Http.Json;
using GestionTubos.Shared.Autenticacion;

namespace GestionTubos.Client.Services;

public sealed class AutenticacionApiService(HttpClient http, LocalAuthStateProvider auth)
{
    public async Task LoginAsync(LoginDto dto)
    {
        var response = await http.PostAsJsonAsync("api/autenticacion/login", dto);
        if (response.StatusCode == HttpStatusCode.Unauthorized) throw new InvalidOperationException("Correo o contraseña incorrectos.");
        response.EnsureSuccessStatusCode();
        var sesion = await response.Content.ReadFromJsonAsync<LoginRespuestaDto>() ?? throw new InvalidOperationException("Respuesta inválida de la API.");
        await auth.IniciarSesionAsync(sesion.Token);
    }

    public Task LogoutAsync() => auth.CerrarSesionAsync();
    public async Task<IReadOnlyList<UsuarioDto>> UsuariosAsync() => await http.GetFromJsonAsync<List<UsuarioDto>>("api/autenticacion/usuarios") ?? [];
    public async Task CrearUsuarioAsync(UsuarioCrearDto dto)
    {
        var response = await http.PostAsJsonAsync("api/autenticacion/usuarios", dto);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            throw new InvalidOperationException(error?.GetValueOrDefault("mensaje") ?? "No se pudo crear el usuario.");
        }
    }
}
