$ErrorActionPreference = 'Stop'
dotnet run --project (Join-Path $PSScriptRoot 'tests\CargaLote\CargaLote.csproj') -- $PSScriptRoot
if ($LASTEXITCODE -ne 0) { throw 'Falló la verificación de cargas.' }
