using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using GestionTubos.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GestionTubos.Infrastructure.Services;

// Exportación de datos del sistema. No sustituye un BACKUP DATABASE de SQL Server.
public sealed class RespaldoService(GestionTubosDbContext db)
{
    public const int Version = 1;
    private const long Limite = 64L * 1024 * 1024;
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = false };
    public sealed record Columna(string Nombre, string Tipo, bool Identidad, bool Generada, string? IncrementoIdentidad);
    public sealed record Tabla(string Esquema, string Nombre, List<Columna> Columnas,
        string? UltimaIdentidad, List<JsonElement[]> Filas);
    public sealed record Archivo(string Ruta, string Sha256, int Filas);
    public sealed record Manifiesto(string Aplicacion, int Version, DateTime FechaUtc,
        string[] Migraciones, List<Archivo> Archivos);
    private static string Q(string name) => "[" + name.Replace("]", "]]") + "]";
    private static string Nombre(Tabla t) => Q(t.Esquema) + "." + Q(t.Nombre);

    public async Task<byte[]> CrearAsync(CancellationToken ct = default)
    {
        await db.Database.OpenConnectionAsync(ct);
        // Lectura consistente de todas las tablas, incluso si hay otros usuarios.
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var tablas = await LeerEsquemaAsync(ct);
        var migraciones = (await db.Database.GetAppliedMigrationsAsync(ct)).ToArray();
        if (migraciones.Length == 0 || (await db.Database.GetPendingMigrationsAsync(ct)).Any())
            throw new InvalidOperationException("La base debe tener todas las migraciones aplicadas antes de respaldar.");
        using var output = new MemoryStream();
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, true))
        {
            var archivos = new List<Archivo>();
            long total = 0;
            foreach (var tabla in tablas)
            {
                var filas = await LeerFilasAsync(tabla, ct);
                // El contador se lee después de bloquear/leer los datos, para no
                // guardar un valor anterior a las filas que acaba de incluir la copia.
                await using var identidad = Comando("SELECT CONVERT(nvarchar(100),last_value) FROM sys.identity_columns WHERE object_id=OBJECT_ID(@tabla)");
                var parametro = identidad.CreateParameter(); parametro.ParameterName = "@tabla"; parametro.Value = Nombre(tabla); identidad.Parameters.Add(parametro);
                var ultima = await identidad.ExecuteScalarAsync(ct);
                var datos = JsonSerializer.SerializeToUtf8Bytes(tabla with { Filas = filas, UltimaIdentidad = ultima is null or DBNull ? null : (string)ultima }, Json);
                total += datos.Length;
                if (total > Limite) throw new InvalidOperationException("El respaldo supera el tamaño admitido; requiere respaldo nativo del servidor.");
                var ruta = $"datos/{archivos.Count:D4}.json";
                await using (var stream = zip.CreateEntry(ruta, CompressionLevel.Optimal).Open())
                    await stream.WriteAsync(datos, ct);
                archivos.Add(new(ruta, Convert.ToHexString(SHA256.HashData(datos)), filas.Count));
            }
            await using var manifest = zip.CreateEntry("manifiesto.json").Open();
            await JsonSerializer.SerializeAsync(manifest,
                new Manifiesto("GestionTubos", Version, DateTime.UtcNow, migraciones, archivos), Json, ct);
        }
        await tx.CommitAsync(ct);
        return output.ToArray();
    }

    private async Task<List<Tabla>> LeerEsquemaAsync(CancellationToken ct)
    {
        await using var cmd = Comando("""
            SELECT s.name,t.name,c.name,ty.name,c.is_identity,
                   CAST(CASE WHEN c.is_computed=1 OR c.generated_always_type<>0 OR ty.name='timestamp' THEN 1 ELSE 0 END AS bit),
                   CONVERT(nvarchar(100),ic.last_value),CONVERT(nvarchar(100),ic.increment_value)
            FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
            JOIN sys.columns c ON c.object_id=t.object_id
            JOIN sys.types ty ON ty.user_type_id=c.user_type_id
            LEFT JOIN sys.identity_columns ic ON ic.object_id=c.object_id AND ic.column_id=c.column_id
            WHERE t.is_ms_shipped=0 ORDER BY s.name,t.name,c.column_id
            """);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var tablas = new List<Tabla>();
        while (await reader.ReadAsync(ct))
        {
            var esquema = reader.GetString(0); var nombre = reader.GetString(1);
            if (tablas.Count == 0 || tablas[^1].Esquema != esquema || tablas[^1].Nombre != nombre)
                tablas.Add(new(esquema, nombre, [], null, []));
            var t = tablas[^1];
            t.Columnas.Add(new(reader.GetString(2), reader.GetString(3), reader.GetBoolean(4), reader.GetBoolean(5), reader.IsDBNull(7) ? null : reader.GetString(7)));
            if (!reader.IsDBNull(6)) tablas[^1] = t with { UltimaIdentidad = reader.GetString(6) };
        }
        return tablas;
    }

    private DbCommand Comando(string sql)
    {
        var cmd = db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = sql; cmd.CommandTimeout = 120;
        cmd.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
        return cmd;
    }

    private async Task<List<JsonElement[]>> LeerFilasAsync(Tabla tabla, CancellationToken ct)
    {
        var columnas = tabla.Columnas.Where(c => !c.Generada).ToArray();
        await using var cmd = Comando($"SELECT {string.Join(',', columnas.Select(c => Q(c.Nombre)))} FROM {Nombre(tabla)}");
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var filas = new List<JsonElement[]>();
        while (await reader.ReadAsync(ct))
        {
            var valores = new JsonElement[columnas.Length];
            for (var i = 0; i < valores.Length; i++)
                valores[i] = JsonSerializer.SerializeToElement(reader.IsDBNull(i) ? null : reader.GetValue(i));
            filas.Add(valores);
        }
        return filas;
    }

    public static List<Tabla> Validar(byte[] contenido, out Manifiesto manifiesto)
    {
        if (contenido.LongLength > Limite) throw new InvalidOperationException("Archivo demasiado grande.");
        using var input = new MemoryStream(contenido);
        using var zip = new ZipArchive(input, ZipArchiveMode.Read);
        if (zip.Entries.Count > 200 || zip.Entries.Sum(e => e.Length) > Limite)
            throw new InvalidOperationException("Contenido del respaldo demasiado grande.");
        var entry = zip.GetEntry("manifiesto.json") ?? throw new InvalidOperationException("Falta el manifiesto.");
        using (var stream = entry.Open())
            manifiesto = JsonSerializer.Deserialize<Manifiesto>(stream, Json) ?? throw new InvalidOperationException("Manifiesto inválido.");
        if (manifiesto.Aplicacion != "GestionTubos" || manifiesto.Version != Version || manifiesto.Archivos.Count == 0)
            throw new InvalidOperationException("Formato de respaldo incompatible.");
        if (zip.Entries.Count != manifiesto.Archivos.Count + 1 || manifiesto.Archivos.Select(a => a.Ruta).Distinct().Count() != manifiesto.Archivos.Count)
            throw new InvalidOperationException("La lista de archivos del respaldo no coincide.");
        var tablas = new List<Tabla>();
        foreach (var file in manifiesto.Archivos)
        {
            var data = zip.GetEntry(file.Ruta) ?? throw new InvalidOperationException("Falta una tabla del respaldo.");
            using var buffer = new MemoryStream(); using (var stream = data.Open()) stream.CopyTo(buffer);
            var bytes = buffer.ToArray();
            if (Convert.ToHexString(SHA256.HashData(bytes)) != file.Sha256)
                throw new InvalidOperationException("El respaldo está dañado: no coincide la suma de verificación.");
            var tabla = JsonSerializer.Deserialize<Tabla>(bytes, Json) ?? throw new InvalidOperationException("Tabla inválida.");
            if (tabla.Filas.Count != file.Filas || tabla.Filas.Any(f => f.Length != tabla.Columnas.Count(c => !c.Generada)))
                throw new InvalidOperationException("Cantidad de filas o columnas inválida.");
            tablas.Add(tabla);
        }
        if (tablas.Select(t => (t.Esquema, t.Nombre)).Distinct().Count() != tablas.Count)
            throw new InvalidOperationException("El respaldo contiene tablas repetidas.");
        return tablas;
    }

    // Recuperación controlada: solo crea una base NUEVA con un nombre reconocible.
    // No hay endpoint de restauración sobre la base de producción.
    public async Task RecuperarEnBaseNuevaAsync(byte[] contenido, CancellationToken ct = default)
    {
        var tablas = Validar(contenido, out var manifest);
        var cs = new SqlConnectionStringBuilder(db.Database.GetConnectionString());
        if (!cs.InitialCatalog.StartsWith("GestionTubos_Recuperacion_", StringComparison.Ordinal)
            || cs.InitialCatalog.Length < 32)
            throw new InvalidOperationException("La base destino debe ser nueva y llamarse GestionTubos_Recuperacion_ seguido de un identificador.");
        if (!manifest.Migraciones.SequenceEqual(db.Database.GetMigrations()))
            throw new InvalidOperationException("Usá la misma versión del programa que creó esta copia.");
        var master = new SqlConnectionStringBuilder(cs.ConnectionString) { InitialCatalog = "master" };
        await using (var connection = new SqlConnection(master.ConnectionString))
        {
            await connection.OpenAsync(ct);
            await using var check = new SqlCommand("SELECT DB_ID(@nombre)", connection);
            check.Parameters.AddWithValue("@nombre", cs.InitialCatalog);
            if (await check.ExecuteScalarAsync(ct) is not DBNull)
                throw new InvalidOperationException("La base destino ya existe. Nunca se sobrescribe una base existente.");
        }
        await db.Database.MigrateAsync(ct);
        await db.Database.OpenConnectionAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var destino = await LeerEsquemaAsync(ct);
        if (destino.Count != tablas.Count) throw new InvalidOperationException("El esquema del respaldo no coincide con esta versión.");
        foreach (var table in tablas)
        {
            var actual = destino.SingleOrDefault(t => t.Esquema == table.Esquema && t.Nombre == table.Nombre);
            if (actual is null || !actual.Columnas.SequenceEqual(table.Columnas))
                throw new InvalidOperationException("El esquema del respaldo no coincide con esta versión.");
        }
        foreach (var t in destino) await EjecutarAsync($"ALTER TABLE {Nombre(t)} NOCHECK CONSTRAINT ALL", ct);
        // Elimina solo los datos iniciales de las migraciones de la base recién creada.
        foreach (var t in destino) await EjecutarAsync($"DELETE FROM {Nombre(t)}", ct);
        foreach (var t in tablas)
        {
            var columnas = t.Columnas.Where(c => !c.Generada).ToArray();
            using var datos = new DataTable();
            foreach (var column in columnas) datos.Columns.Add(column.Nombre, TipoClr(column.Tipo));
            foreach (var fila in t.Filas)
                datos.Rows.Add(fila.Select((v, i) => v.ValueKind == JsonValueKind.Null ? DBNull.Value : v.Deserialize(TipoClr(columnas[i].Tipo))!).ToArray());
            using var bulk = new SqlBulkCopy((SqlConnection)db.Database.GetDbConnection(), SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls,
                (SqlTransaction)tx.GetDbTransaction()) { DestinationTableName = Nombre(t), BulkCopyTimeout = 120 };
            foreach (var c in columnas) bulk.ColumnMappings.Add(c.Nombre, c.Nombre);
            await bulk.WriteToServerAsync(datos, ct);
            if (t.UltimaIdentidad is not null)
            {
                var ultima = decimal.Parse(t.UltimaIdentidad, CultureInfo.InvariantCulture);
                // SQL Server usa el valor de RESEED directamente en una tabla nunca poblada.
                if (t.Filas.Count == 0 && destino.Single(d => Nombre(d) == Nombre(t)).UltimaIdentidad is null)
                    ultima += decimal.Parse(t.Columnas.Single(c => c.Identidad).IncrementoIdentidad!, CultureInfo.InvariantCulture);
                var valor = ultima.ToString(CultureInfo.InvariantCulture);
                await EjecutarAsync($"DBCC CHECKIDENT ('{Nombre(t).Replace("'", "''")}', RESEED, {valor}) WITH NO_INFOMSGS", ct);
            }
        }
        foreach (var t in destino) await EjecutarAsync($"ALTER TABLE {Nombre(t)} WITH CHECK CHECK CONSTRAINT ALL", ct);
        foreach (var t in tablas)
        {
            var recuperadas = await LeerFilasAsync(t, ct);
            static string[] Canonicas(List<JsonElement[]> filas) => filas.Select(f => JsonSerializer.Serialize(f)).Order(StringComparer.Ordinal).ToArray();
            if (!Canonicas(t.Filas).SequenceEqual(Canonicas(recuperadas)))
                throw new InvalidOperationException("La verificación de los datos recuperados no coincide.");
        }
        await tx.CommitAsync(ct);
    }

    private async Task EjecutarAsync(string sql, CancellationToken ct)
    {
        await using var cmd = Comando(sql); await cmd.ExecuteNonQueryAsync(ct);
    }

    private static Type TipoClr(string tipo) => tipo switch
    {
        "bigint" => typeof(long), "int" => typeof(int), "smallint" => typeof(short), "tinyint" => typeof(byte),
        "bit" => typeof(bool), "decimal" or "numeric" or "money" or "smallmoney" => typeof(decimal),
        "float" => typeof(double), "real" => typeof(float), "uniqueidentifier" => typeof(Guid),
        "datetime" or "datetime2" or "smalldatetime" or "date" => typeof(DateTime),
        "datetimeoffset" => typeof(DateTimeOffset), "time" => typeof(TimeSpan),
        "binary" or "varbinary" or "image" => typeof(byte[]),
        "char" or "nchar" or "varchar" or "nvarchar" or "text" or "ntext" or "xml" => typeof(string),
        _ => throw new InvalidOperationException("Tipo de columna no soportado para recuperación: " + tipo)
    };
}
