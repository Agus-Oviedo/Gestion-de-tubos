using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace GestionTubos.Client.Services;

public sealed class LocalAuthStateProvider(IJSRuntime js) : AuthenticationStateProvider
{
    private const string TokenKey = "gestionCilindros.token";
    private string? token;

    public async Task<string?> GetTokenAsync()
    {
        token ??= await js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        return token;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var jwt = await GetTokenAsync();
        var identity = string.IsNullOrWhiteSpace(jwt) ? new ClaimsIdentity() : new ClaimsIdentity(ParseClaims(jwt), "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task IniciarSesionAsync(string jwt)
    {
        token = jwt;
        await js.InvokeVoidAsync("localStorage.setItem", TokenKey, jwt);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task CerrarSesionAsync()
    {
        token = null;
        await js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    private static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var claims = new List<Claim>();
        try
        {
            var payload = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Convert.FromBase64String(payload)) ?? [];
            foreach (var pair in values)
            {
                if (pair.Value.ValueKind == JsonValueKind.Array)
                    foreach (var item in pair.Value.EnumerateArray()) claims.Add(new Claim(MapType(pair.Key), item.ToString()));
                else claims.Add(new Claim(MapType(pair.Key), pair.Value.ToString()));
            }
        }
        catch { }
        return claims;
    }

    private static string MapType(string type) => type switch
    {
        "unique_name" => ClaimTypes.Name,
        "email" => ClaimTypes.Email,
        "role" => ClaimTypes.Role,
        _ => type
    };
}
