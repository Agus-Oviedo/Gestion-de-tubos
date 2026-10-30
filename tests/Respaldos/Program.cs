using System.IO.Compression;
using System.Text.Json;
using GestionTubos.Domain.Entities;
using GestionTubos.Domain.Enums;
using GestionTubos.Infrastructure.Data;
using GestionTubos.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var conexion = Environment.GetEnvironmentVariable("RESPALDOS_TEST_SQL");
if (string.IsNullOrEmpty(conexion))
{
    using var cfg = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(args[0], "GestionTubos.Api", "appsettings.json")));
    conexion = cfg.RootElement.GetProperty("ConnectionStrings").GetProperty("GestionTubos").GetString();
}
var prefijo = new SqlConnectionStringBuilder(conexion);
var id = Guid.NewGuid().ToString("N");
GestionTubosDbContext Crear(string nombre) => new(new DbContextOptionsBuilder<GestionTubosDbContext>()
    .UseSqlServer(new SqlConnectionStringBuilder(prefijo.ConnectionString) { InitialCatalog = nombre }.ConnectionString).Options);
await using var origen = Crear("GestionTubos_PruebaRespaldo_" + id);
await using var destino = Crear("GestionTubos_Recuperacion_" + id);
int casos = 0;
void Check(bool condition, string message) { if (!condition) throw new Exception(message); casos++; }
async Task Rechaza(Func<Task> action, string mensaje)
{
    try { await action(); } catch (InvalidOperationException) { casos++; return; }
    throw new Exception(mensaje);
}
try
{
    // Espera únicamente al SQL Server del entorno de pruebas.
    for (var intento = 0; ; intento++)
    {
        try { await origen.Database.MigrateAsync(); break; }
        catch (SqlException) when (intento < 25) { await Task.Delay(2000); }
    }
    origen.Clientes.Add(new Cliente { Tipo = (TipoCliente)1, Nombre = "Recuperación Ñ", Apellido = "Prueba", Dni = "99887766", Telefono = "123", FechaAltaUtc = new DateTime(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc) });
    var usuario = new UsuarioAplicacion { Id = "usuario-prueba", UserName = "prueba@local", NormalizedUserName = "PRUEBA@LOCAL", NombreCompleto = "Usuario prueba", PasswordHash = "HASH_SOLO_PRUEBA", SecurityStamp = "conservar-sello", Activo = true };
    var rol = new IdentityRole { Id = "rol-prueba", Name = "Rol prueba", NormalizedName = "ROL PRUEBA" };
    origen.AddRange(usuario, rol);
    await origen.SaveChangesAsync();
    origen.Add(new IdentityUserRole<string> { UserId = usuario.Id, RoleId = rol.Id });
    var claim = new IdentityUserClaim<string> { UserId = usuario.Id, ClaimType = "prueba", ClaimValue = "conservar" };
    var eliminado = new IdentityRoleClaim<string> { RoleId = rol.Id, ClaimType = "borrar", ClaimValue = "no debe reaparecer" };
    origen.AddRange(claim, eliminado); await origen.SaveChangesAsync();
    var ultimaIdentidadEliminada = eliminado.Id;
    origen.Remove(eliminado); await origen.SaveChangesAsync();
    var tubo = new Tubo {
        CodigoInterno = "PRUEBA-COPIA", NumeroSerie = "SERIE-COPIA", IdentificacionGrabada = "SERIE-COPIA",
        TipoGas = new TipoGas { Nombre = "Gas copia" },
        CapacidadTubo = new CapacidadTubo { Valor = 12.50m, Unidad = "m3" },
        Ubicacion = new Ubicacion { Nombre = "Ubicación copia", EsInterna = true },
        TipoPropietario = (TipoPropietarioTubo)1, FechaFabricacion = new DateOnly(2020, 2, 3)
    };
    var remito = new Remito { Numero = "COPIA-R-1", Estado = "Firmado", PersonaRecibe = "Persona Ñ",
        FirmaDataUrl = "data:image/png;base64,AQIDBA==", FechaFirmaUtc = DateTime.UtcNow, UsuarioEmisor = "Prueba" };
    origen.Pedidos.Add(new Pedido { Numero = "COPIA-P-1", Cliente = await origen.Clientes.SingleAsync(c => c.Dni == "99887766"),
        UsuarioCreador = "Prueba", Remito = remito, Detalles = [new PedidoDetalle { Tubo = tubo }] });
    await origen.SaveChangesAsync();

    var bytes = await new RespaldoService(origen).CrearAsync();
    var tablas = RespaldoService.Validar(bytes, out var manifiesto);
    Check(tablas.Any(t => t.Nombre == "Clientes") && tablas.Any(t => t.Nombre == "AspNetUsers") && tablas.Any(t => t.Nombre == "__EFMigrationsHistory"), "Incluye datos, usuarios e historial de migraciones");
    Check(manifiesto.Archivos.All(a => a.Sha256.Length == 64), "Cada tabla tiene suma de verificación");
    await new RespaldoService(destino).RecuperarEnBaseNuevaAsync(bytes);
    Check((await destino.Clientes.SingleAsync(c => c.Dni == "99887766")).Nombre == "Recuperación Ñ", "Conserva texto y datos del cliente");
    Check((await destino.Users.SingleAsync(u => u.Id == usuario.Id)).PasswordHash == usuario.PasswordHash, "Conserva credenciales para recuperación");
    Check(await destino.UserRoles.AnyAsync(r => r.UserId == usuario.Id && r.RoleId == rol.Id), "Conserva roles");
    Check(await destino.UserClaims.AnyAsync(c => c.Id == claim.Id && c.ClaimValue == "conservar"), "Conserva claves de identidad");
    var recuperado = await destino.Tubos.Include(t => t.CapacidadTubo).SingleAsync(t => t.CodigoInterno == "PRUEBA-COPIA");
    Check(recuperado.FechaFabricacion == tubo.FechaFabricacion && recuperado.CapacidadTubo.Valor == 12.50m, "Conserva fechas y cantidades decimales");
    Check((await destino.Remitos.SingleAsync(r => r.Numero == "COPIA-R-1")).FirmaDataUrl == remito.FirmaDataUrl, "Conserva la firma del remito");
    Check(await destino.PedidoDetalles.AnyAsync(d => d.TuboId == tubo.Id && d.Pedido.Numero == "COPIA-P-1"), "Conserva relaciones de entrega y stock");
    var siguiente = new IdentityRoleClaim<string> { RoleId = rol.Id, ClaimType = "nuevo", ClaimValue = "nuevo" };
    destino.Add(siguiente); await destino.SaveChangesAsync();
    Check(siguiente.Id > ultimaIdentidadEliminada, "No reutiliza identidades eliminadas de una tabla vacía");
    await Rechaza(() => new RespaldoService(destino).RecuperarEnBaseNuevaAsync(bytes), "No debe sobrescribir una base existente");
    await Rechaza(() => new RespaldoService(origen).RecuperarEnBaseNuevaAsync(bytes), "No debe restaurar sobre la base origen");

    using var modificado = new MemoryStream(); modificado.Write(bytes);
    using (var zip = new ZipArchive(modificado, ZipArchiveMode.Update, true))
    {
        var entry = zip.GetEntry(manifiesto.Archivos[0].Ruta)!;
        entry.Delete(); using var writer = new StreamWriter(zip.CreateEntry(manifiesto.Archivos[0].Ruta).Open()); writer.Write("{}");
    }
    try { RespaldoService.Validar(modificado.ToArray(), out _); throw new Exception("Aceptó una copia dañada"); }
    catch (InvalidOperationException) { casos++; }
    Console.WriteLine($"Respaldos: {casos} verificaciones correctas; exportación y recuperación real en SQL Server.");
}
finally
{
    // Solo se eliminan las dos bases de prueba, con nombres construidos arriba.
    SqlConnection.ClearAllPools();
    await destino.Database.EnsureDeletedAsync();
    await origen.Database.EnsureDeletedAsync();
}
