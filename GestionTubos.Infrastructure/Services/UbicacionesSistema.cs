using GestionTubos.Domain.Entities;
using GestionTubos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionTubos.Infrastructure.Services;

internal static class UbicacionesSistema
{
    internal const string EnPoderDelCliente = "En poder del cliente";

    internal static async Task<int> ObtenerEnPoderDelClienteAsync(GestionTubosDbContext db, CancellationToken ct)
    {
        var ubicacion = await db.Ubicaciones.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Nombre == EnPoderDelCliente, ct);

        if (ubicacion is null)
        {
            ubicacion = new Ubicacion
            {
                Nombre = EnPoderDelCliente,
                Descripcion = "Ubicación lógica para cilindros que se encuentran con su propietario.",
                EsInterna = false
            };
            db.Ubicaciones.Add(ubicacion);
            await db.SaveChangesAsync(ct);
        }
        else if (!ubicacion.Activa)
        {
            ubicacion.Activa = true;
            await db.SaveChangesAsync(ct);
        }

        return ubicacion.Id;
    }
}
