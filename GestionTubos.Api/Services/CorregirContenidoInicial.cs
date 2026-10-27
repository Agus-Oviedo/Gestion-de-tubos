using System.Text.Json;
using GestionTubos.Domain.Entities;
using GestionTubos.Domain.Enums;
using GestionTubos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionTubos.Api.Services;

public static class CorregirContenidoInicial
{
    public static async Task EjecutarAsync(WebApplication app)
    {
        var carpeta = Path.Combine(app.Environment.ContentRootPath, "App_Data", "contenido-20261014");
        var marca = Path.Combine(carpeta, "run.flag");
        if (!File.Exists(marca)) return;
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GestionTubosDbContext>();
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        // Desconocido era el valor del formulario anterior; las devoluciones reales son Vacío.
        var tubos = await db.Tubos.Where(x => x.TipoPropietario == TipoPropietarioTubo.Empresa && x.EstadoContenido == EstadoContenido.Desconocido).ToListAsync();
        var respaldo = Path.Combine(carpeta, "tubos-before.json");
        if (!File.Exists(respaldo)) await File.WriteAllTextAsync(respaldo, JsonSerializer.Serialize(tubos.Select(x => new { x.Id, x.CodigoInterno, x.EstadoContenido, x.EstadoDisponibilidad, x.UbicacionId })));
        foreach (var tubo in tubos)
        {
            tubo.EstadoContenido = EstadoContenido.Lleno;
            db.MovimientosCilindro.Add(new MovimientoCilindro { TuboId = tubo.Id, Tipo = "Corrección contenido inicial", EstadoAnterior = "Contenido=4", EstadoPosterior = "Contenido=1", UsuarioResponsable = "Sistema", Observaciones = "Corrección autorizada: tubos propios registrados con contenido desconocido pasan a llenos. Disponibilidad y ubicación conservadas." });
        }
        await db.SaveChangesAsync();
        if (await db.Tubos.AnyAsync(x => x.TipoPropietario == TipoPropietarioTubo.Empresa && x.EstadoContenido == EstadoContenido.Desconocido)) throw new InvalidOperationException("Quedaron tubos sin corregir.");
        await tx.CommitAsync();
        await File.WriteAllTextAsync(Path.Combine(carpeta, "status.json"), JsonSerializer.Serialize(new { estado = "Completado", corregidos = tubos.Count, ids = tubos.Select(x => x.Id), fechaUtc = DateTime.UtcNow }));
        File.Move(marca, Path.Combine(carpeta, "completed.flag"), true);
    }
}
