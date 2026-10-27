using System.Data.Common;
using System.IO.Compression;
using System.Text.Json;
using GestionTubos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace GestionTubos.Api.Services;
// Solo se activa con un archivo privado creado por quien publica mediante FTP.
public static class ServerUpgrade
{
 public static async Task EjecutarAsync(WebApplication app,string operacion="upgrade-20261009")
 {
  var carpeta=Path.Combine(app.Environment.ContentRootPath,"App_Data",operacion);var marca=Path.Combine(carpeta,"run.flag");if(!File.Exists(marca))return;
  Directory.CreateDirectory(carpeta);
  void Estado(string etapa,object? detalle=null)=>File.WriteAllText(Path.Combine(carpeta,"status.json"),JsonSerializer.Serialize(new{etapa,fechaUtc=DateTime.UtcNow,detalle}));
  string Q(string valor)=>"["+valor.Replace("]","]]")+"]";
  try{
   Estado("Iniciando");using var scope=app.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<GestionTubosDbContext>();
   var aplicadas=(await db.Database.GetAppliedMigrationsAsync()).ToList();if(aplicadas.Count==0)throw new InvalidOperationException("La base no tiene historial de migraciones; requiere revisión manual.");
   var pendientes=(await db.Database.GetPendingMigrationsAsync()).ToList();var sql=db.Database.GetDbConnection();await sql.OpenAsync();
   var tablas=new List<(string Schema,string Name,List<string>Columns)>();
   await using(var cmd=sql.CreateCommand()){cmd.CommandText="SELECT s.name,t.name,c.name FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id JOIN sys.columns c ON c.object_id=t.object_id WHERE t.is_ms_shipped=0 ORDER BY s.name,t.name,c.column_id";await using var r=await cmd.ExecuteReaderAsync();while(await r.ReadAsync()){var schema=r.GetString(0);var name=r.GetString(1);if(tablas.Count==0||tablas[^1].Schema!=schema||tablas[^1].Name!=name)tablas.Add((schema,name,[]));tablas[^1].Columns.Add(r.GetString(2));}}
   async Task<List<string>> Leer((string Schema,string Name,List<string>Columns)t){await using var cmd=sql.CreateCommand();cmd.CommandTimeout=120;cmd.CommandText="SELECT "+string.Join(",",t.Columns.Select(Q))+" FROM "+Q(t.Schema)+"."+Q(t.Name);await using var r=await cmd.ExecuteReaderAsync();var rows=new List<string>();while(await r.ReadAsync()){var valores=new object?[r.FieldCount];for(var i=0;i<valores.Length;i++)valores[i]=r.IsDBNull(i)?null:r.GetValue(i);rows.Add(JsonSerializer.Serialize(valores));}rows.Sort(StringComparer.Ordinal);return rows;}
   var copia=new Dictionary<string,List<string>>();foreach(var t in tablas)copia[t.Schema+"."+t.Name]=await Leer(t);
   var respaldo=Path.Combine(carpeta,"database-before.json.gz");if(File.Exists(respaldo))throw new InvalidOperationException("Ya existe un respaldo de esta operación; revisar antes de repetirla.");
   await using(var file=new FileStream(respaldo,FileMode.CreateNew))await using(var gz=new GZipStream(file,CompressionLevel.Optimal)){await JsonSerializer.SerializeAsync(gz,new{fechaUtc=DateTime.UtcNow,aplicadas,pendientes,tablas=tablas.Select(t=>new{schema=t.Schema,name=t.Name,columns=t.Columns}),datos=copia});}
   Estado("Respaldo completo",new{aplicadas,pendientes,filas=copia.ToDictionary(x=>x.Key,x=>x.Value.Count)});
   await db.Database.MigrateAsync();
   foreach(var t in tablas.Where(t=>t.Name!="__EFMigrationsHistory")){var despues=await Leer(t);if(!despues.SequenceEqual(copia[t.Schema+"."+t.Name]))throw new InvalidOperationException("Los datos preexistentes cambiaron en "+t.Name+"; revisar el respaldo.");}
   File.Move(marca,Path.Combine(carpeta,"completed.flag"));
   Estado("Completado y verificado",new{migracionesAplicadas=pendientes,filasPreservadas=copia.ToDictionary(x=>x.Key,x=>x.Value.Count)});
  }catch(Exception ex){Estado("Error",new{tipo=ex.GetType().Name,mensaje=ex is InvalidOperationException?ex.Message:"No pudo completarse la actualización. Revisar el registro del servidor."});throw;}
 }
}
