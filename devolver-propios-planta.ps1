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
$planta = @($ubicaciones | Where-Object { $_.nombre -eq 'PLANTA' -and $_.esInterna })
if ($planta.Count -ne 1) { throw 'No se encontró una única ubicación PLANTA.' }
$plantaId = $planta[0].id
$propios = @($tubos | Where-Object tipoPropietario -eq 1)
$ops = Invoke-RestMethod -Uri "$base/api/operaciones?soloAbiertas=true" -Headers $headers
foreach ($t in $propios | Where-Object estadoDisponibilidad -eq 4) {
    $abiertas = @($ops | Where-Object cilindroId -eq $t.id)
    if ($abiertas.Count -ne 1) { throw "El tubo $($t.codigoInterno) no tiene una única operación abierta." }
}
foreach ($t in $propios | Where-Object estadoDisponibilidad -eq 4) {
    $op = $ops | Where-Object cilindroId -eq $t.id
    $body = @{ OperacionId = $op.id; EstadoFisico = $t.estadoFisico; EstadoContenido = $t.estadoContenido; UbicacionDestinoId = $plantaId; RequiereInspeccion = $false; Observaciones = 'Devolución a planta solicitada para testear el sistema.' } | ConvertTo-Json
    $null = Invoke-RestMethod -Method Post -Uri "$base/api/operaciones/recepciones" -Headers $headers -ContentType 'application/json' -Body $body
    Write-Output "Devuelto a PLANTA: $($t.codigoInterno)"
}
$actuales = Invoke-RestMethod -Uri "$base/api/cilindros" -Headers $headers
$mover = @($actuales | Where-Object { $_.tipoPropietario -eq 1 -and $_.ubicacionId -ne $plantaId })
if ($mover.Count -gt 0) {
    $items = @($mover | ForEach-Object { @{ Id = $_.id; UbicacionOrigenId = $_.ubicacionId } })
    $body = @{ UbicacionDestinoId = $plantaId; Tubos = $items } | ConvertTo-Json -Depth 5
    $null = Invoke-RestMethod -Method Post -Uri "$base/api/cilindros/ubicacion" -Headers $headers -ContentType 'application/json' -Body $body
    Write-Output "Trasladados desde otras ubicaciones: $($mover.Count)"
}
$final = Invoke-RestMethod -Uri "$base/api/cilindros" -Headers $headers
$finalPropios = @($final | Where-Object tipoPropietario -eq 1)
if (@($finalPropios | Where-Object { $_.ubicacionId -ne $plantaId -or $_.estadoDisponibilidad -ne 1 }).Count) { throw 'Quedan tubos propios fuera de planta o no disponibles.' }
$ajenosAntes = $tubos | Where-Object tipoPropietario -ne 1 | ConvertTo-Json -Depth 10 -Compress
$ajenosDespues = $final | Where-Object tipoPropietario -ne 1 | ConvertTo-Json -Depth 10 -Compress
if ($ajenosAntes -cne $ajenosDespues) { throw 'Cambió un tubo ajeno durante la operación.' }
$abiertasFinal = Invoke-RestMethod -Uri "$base/api/operaciones?soloAbiertas=true" -Headers $headers
if (@($abiertasFinal | Where-Object { $_.cilindroId -in $finalPropios.id }).Count) { throw 'Quedan operaciones abiertas de tubos propios.' }
Write-Output "Verificado: $($finalPropios.Count) tubos propios en PLANTA, disponibles; $(@($finalPropios | Where-Object estadoContenido -eq 1).Count) llenos. Tubos ajenos sin cambios."
