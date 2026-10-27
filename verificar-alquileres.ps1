$ErrorActionPreference = 'Stop'
$raizAlquiler = $PSScriptRoot
$carpetaPrueba = Join-Path ([System.IO.Path]::GetTempPath()) ('GestionTubos-Alquiler-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $carpetaPrueba | Out-Null
$horaFuente = [System.Security.SecurityElement]::Escape((Join-Path $raizAlquiler 'GestionTubos.Shared\HoraArgentina.cs'))
$alquilerFuente = [System.Security.SecurityElement]::Escape((Join-Path $raizAlquiler 'GestionTubos.Shared\Operaciones\CalculadoraAlquiler.cs'))
$proyectoPrueba = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><Compile Include="' + $horaFuente + '" Link="HoraArgentina.cs"/><Compile Include="' + $alquilerFuente + '" Link="CalculadoraAlquiler.cs"/></ItemGroup></Project>'
Set-Content -Encoding utf8 (Join-Path $carpetaPrueba 'Prueba.csproj') $proyectoPrueba
@"
using GestionTubos.Shared.Operaciones;

var inicio = new DateTime(2026, 10, 1, 15, 0, 0, DateTimeKind.Utc);
var corte = new DateTime(2026, 10, 8, 15, 0, 0, DateTimeKind.Utc);
var pruebas = 0;
void Verificar(string caso, DateTime entrega, DateTime? devolucion, int? gratis, string tipo, DateTime ahora, int dias, int cobrables)
{
    var resultado = CalculadoraAlquiler.Calcular(entrega, devolucion, gratis, tipo, ahora);
    if (resultado.DiasTranscurridos != dias || resultado.DiasACobrar != cobrables)
        throw new Exception(caso + ": " + resultado);
    pruebas++;
}
Verificar("Abierto", inicio, null, 0, "Alquiler", corte, 7, 7);
Verificar("Mismo día", inicio, inicio.AddHours(3), 0, "Alquiler", corte, 0, 0);
Verificar("Bonificación parcial", inicio, null, 3, "Alquiler", corte, 7, 4);
Verificar("Dentro de bonificación", inicio, null, 10, "Alquiler", corte, 7, 0);
Verificar("Bonificación exacta", inicio, null, 7, "Alquiler", corte, 7, 0);
Verificar("Devolución detiene conteo", inicio, inicio.AddDays(2), 1, "Alquiler", corte.AddMonths(1), 2, 1);
Verificar("Devolución parcial: tubo pendiente", inicio, null, 1, "Alquiler", corte, 7, 6);
Verificar("Comodato no cobra", inicio, null, 0, "Comodato", corte, 7, 0);
Verificar("Registro anterior sin bonificación", inicio, null, null, "Alquiler", corte, 7, 7);
Verificar("Fecha futura no negativa", corte.AddDays(1), null, 0, "Alquiler", corte, 0, 0);
var antesMedianoche = new DateTime(2026, 10, 2, 2, 50, 0, DateTimeKind.Utc);
Verificar("Medianoche argentina", antesMedianoche, antesMedianoche.AddMinutes(20), 0, "Alquiler", corte, 1, 1);
var antesMedianocheUtc = new DateTime(2026, 10, 1, 23, 50, 0, DateTimeKind.Utc);
Verificar("Medianoche UTC no suma", antesMedianocheUtc, antesMedianocheUtc.AddMinutes(20), 0, "Alquiler", corte, 0, 0);
Console.WriteLine(pruebas + " casos de alquiler verificados.");
"@ | Set-Content -Encoding utf8 (Join-Path $carpetaPrueba 'Program.cs')
dotnet run --project (Join-Path $carpetaPrueba 'Prueba.csproj')
if ($LASTEXITCODE -ne 0) { throw 'Falló la verificación de alquileres.' }
