using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GestionTubos.Client;
using GestionTubos.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrlConfigurada = builder.Configuration["ApiUrl"];
var direccionBase = new Uri(builder.HostEnvironment.BaseAddress);
var apiUrl = string.IsNullOrWhiteSpace(apiUrlConfigurada)
    ? (builder.HostEnvironment.IsDevelopment()
        ? new UriBuilder(direccionBase) { Port = 5105 }.Uri.AbsoluteUri
        : direccionBase.AbsoluteUri)
    : new Uri(direccionBase, apiUrlConfigurada).AbsoluteUri;
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<LocalAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<LocalAuthStateProvider>());
builder.Services.AddTransient<AuthMessageHandler>();
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler) { BaseAddress = new Uri(apiUrl) };
});
builder.Services.AddScoped<AutenticacionApiService>();
builder.Services.AddScoped<ClienteApiService>();
builder.Services.AddScoped<CatalogoApiService>();
builder.Services.AddScoped<TuboApiService>();
builder.Services.AddScoped<OperacionApiService>();
builder.Services.AddScoped<CargaLoteApiService>();
builder.Services.AddScoped<PlanchadaApiService>();
builder.Services.AddScoped<PedidoApiService>();
builder.Services.AddScoped<RepartoApiService>();

await builder.Build().RunAsync();
