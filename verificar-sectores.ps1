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
$planta = $ubicaciones | Where-Object { $_.nombre -eq 'PLANTA' -and $_.esInterna } | Select-Object -First 1
if (-not $planta) { throw 'No se encontró PLANTA.' }
$r = Invoke-WebRequest -Method Put -Uri "$base/api/catalogos/ubicaciones/$($planta.id)" -Headers $headers -ContentType 'application/json' -Body (@{Nombre=$planta.nombre;Descripcion=$planta.descripcion;EsInterna=$planta.esInterna} | ConvertTo-Json) -SkipHttpErrorCheck
if ($r.StatusCode -ne 200) { throw "Falló edición sin cambios: $($r.StatusCode) $($r.Content)" }
Write-Output 'OK: edición sin cambios HTTP 200'
if (@($tubos | Where-Object ubicacionId -eq $planta.id).Count -gt 0) {
    $r = Invoke-WebRequest -Method Delete -Uri "$base/api/catalogos/ubicaciones/$($planta.id)" -Headers $headers -SkipHttpErrorCheck
    if ($r.StatusCode -ne 409) { throw "Falló protección de sector ocupado: $($r.StatusCode)" }
    Write-Output 'OK: desactivación de sector ocupado rechazada HTTP 409'
}
$despues = Invoke-RestMethod -Uri "$base/api/catalogos/ubicaciones" -Headers $headers
if (($ubicaciones | ConvertTo-Json -Compress) -cne ($despues | ConvertTo-Json -Compress)) { throw 'Las ubicaciones cambiaron durante la verificación.' }
Write-Output 'OK: ubicaciones sin cambios'
