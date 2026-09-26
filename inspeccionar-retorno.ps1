$ErrorActionPreference = 'Stop'
$base = 'http://localhost:5105'
$config = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'GestionTubos.Api/appsettings.json') -Raw | ConvertFrom-Json
$correo = $config.UsuarioInicial.Correo
$clave = $config.UsuarioInicial.Contrasena
if (-not $correo) { $correo = 'admin@oxigenonorte.local' }
if (-not $clave) { throw 'Falta UsuarioInicial.Contrasena en appsettings.json.' }
$login = Invoke-RestMethod -Method Post -Uri "$base/api/autenticacion/login" -ContentType 'application/json' -Body (@{ Correo = $correo; Contrasena = $clave } | ConvertTo-Json)
$headers = @{ Authorization = "Bearer $($login.token)" }
$tubos = Invoke-RestMethod -Uri "$base/api/cilindros" -Headers $headers
$ubicaciones = Invoke-RestMethod -Uri "$base/api/catalogos/ubicaciones" -Headers $headers
$internas = @($ubicaciones | Where-Object esInterna | Select-Object -ExpandProperty id)
$ops = Invoke-RestMethod -Uri "$base/api/operaciones?soloAbiertas=true" -Headers $headers
$tubos | Where-Object tipoPropietario -eq 1 | Select-Object id,codigoInterno,ubicacionId,ubicacion,estadoContenido,estadoDisponibilidad | ConvertTo-Json
$ubicaciones | Select-Object id,nombre,esInterna | ConvertTo-Json
$ops | Select-Object id,cilindroId,estado | ConvertTo-Json
