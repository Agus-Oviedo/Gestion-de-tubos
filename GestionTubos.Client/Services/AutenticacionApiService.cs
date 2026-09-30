using System.Net;
using System.Net.Http.Json;
using GestionTubos.Shared.Autenticacion;

namespace GestionTubos.Client.Services;

public sealed class AutenticacionApiService(HttpClient http, LocalAuthStateProvider auth)
{
    public async Task RegistrarAsync(RegistroDto dto) => await Comprobar(await http.PostAsJsonAsync("api/autenticacion/registro", dto));
    public async Task AprobarAsync(string id) => await Comprobar(await http.PostAsync($"api/autenticacion/usuarios/{Uri.EscapeDataString(id)}/aprobar", null));
    public async Task CambiarRolAsync(string id, string rol) => await Comprobar(await http.PutAsJsonAsync($"api/autenticacion/usuarios/{Uri.EscapeDataString(id)}/rol", new UsuarioRolDto { Rol = rol }));
    private static async Task Comprobar(HttpResponseMessage respuesta)
    {
        using (respuesta)
        {
            if (respuesta.IsSuccessStatusCode) return;
            var texto = await respuesta.Content.ReadAsStringAsync();
            try
            {
                using var json = System.Text.Json.JsonDocument.Parse(texto);
                if (json.RootElement.TryGetProperty("mensaje", out var mensaje))
                    throw new InvalidOperationException(mensaje.GetString());
            }
            catch (System.Text.Json.JsonException) { }
            throw new InvalidOperationException("No se pudo completar la solicitud. Revisá los datos e intentá nuevamente.");
        }
    }
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
