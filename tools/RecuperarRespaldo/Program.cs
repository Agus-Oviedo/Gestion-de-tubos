using GestionTubos.Infrastructure.Data;
using GestionTubos.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

if (args.Length != 1 || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GESTION_TUBOS_RECUPERACION")))
{
    Console.Error.WriteLine("Indicá el archivo ZIP y configurá GESTION_TUBOS_RECUPERACION con la conexión a una base nueva GestionTubos_Recuperacion_<identificador>.");
    return 1;
}
try
{
    var options = new DbContextOptionsBuilder<GestionTubosDbContext>()
        .UseSqlServer(Environment.GetEnvironmentVariable("GESTION_TUBOS_RECUPERACION")).Options;
    await using var db = new GestionTubosDbContext(options);
    await new RespaldoService(db).RecuperarEnBaseNuevaAsync(await File.ReadAllBytesAsync(args[0]));
    Console.WriteLine("Copia recuperada y verificada en la base nueva. Revisá el sistema antes de cambiar la conexión de producción.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex is InvalidOperationException ? ex.Message : "No se pudo recuperar la copia. Revisá el archivo, el acceso y la base nueva.");
    return 1;
}
