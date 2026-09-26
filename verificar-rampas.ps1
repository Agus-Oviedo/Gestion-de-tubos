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
$candidato = $tubos | Where-Object { $_.tipoPropietario -eq 1 -and $_.estadoDisponibilidad -in 1,6,7,8 -and $_.ubicacionId -in $internas } | Select-Object -First 1
if (-not $candidato) { throw 'No hay tubos propios elegibles para verificar.' }
$movimientosAntes = @(Invoke-RestMethod -Uri "$base/api/operaciones/movimientos" -Headers $headers | ConvertTo-Json -Depth 20)

function Probar($nombre, $body, $esperado, $autenticado = $true) {
    $opciones = @{ Method = 'Post'; Uri = "$base/api/cilindros/ubicacion"; ContentType = 'application/json'; Body = ($body | ConvertTo-Json -Depth 5); SkipHttpErrorCheck = $true }
    if ($autenticado) { $opciones.Headers = $headers }
    $respuesta = Invoke-WebRequest @opciones
    if ([int]$respuesta.StatusCode -ne $esperado) { throw "$nombre : HTTP $($respuesta.StatusCode), se esperaba $esperado. $($respuesta.Content)" }
    Write-Output "OK: $nombre (HTTP $esperado)"
}

$item = @{ Id = $candidato.id; UbicacionOrigenId = $candidato.ubicacionId }
Probar 'Requiere autenticación' @{ UbicacionDestinoId = $candidato.ubicacionId; Tubos = @($item) } 401 $false
Probar 'Selección vacía' @{ UbicacionDestinoId = $candidato.ubicacionId; Tubos = @() } 400
Probar 'Destino inexistente' @{ UbicacionDestinoId = 2147483647; Tubos = @($item) } 409
Probar 'Tubos repetidos' @{ UbicacionDestinoId = $candidato.ubicacionId; Tubos = @($item,$item) } 409
Probar 'Ubicación desactualizada' @{ UbicacionDestinoId = $candidato.ubicacionId; Tubos = @(@{ Id = $candidato.id; UbicacionOrigenId = 2147483647 }) } 409
$ajeno = $tubos | Where-Object { $_.tipoPropietario -ne 1 } | Select-Object -First 1
if ($ajeno) { Probar 'Rechaza lote con tubo ajeno' @{ UbicacionDestinoId = $candidato.ubicacionId; Tubos = @($item,@{ Id = $ajeno.id; UbicacionOrigenId = $ajeno.ubicacionId }) } 409 }
Probar 'Misma ubicación: operación sin cambios' @{ UbicacionDestinoId = $candidato.ubicacionId; Tubos = @($item) } 204
$despues = Invoke-RestMethod -Uri "$base/api/cilindros" -Headers $headers
if (($tubos | ConvertTo-Json -Depth 20 -Compress) -cne ($despues | ConvertTo-Json -Depth 20 -Compress)) { throw 'El inventario cambió durante la verificación.' }
$movimientosDespues = @(Invoke-RestMethod -Uri "$base/api/operaciones/movimientos" -Headers $headers | ConvertTo-Json -Depth 20)
if ([string]$movimientosAntes -cne [string]$movimientosDespues) { throw 'El historial cambió durante la verificación.' }
Write-Output 'OK: inventario e historial sin modificaciones.'
