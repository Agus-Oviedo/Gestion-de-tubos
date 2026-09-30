$ErrorActionPreference = "Stop"
$raiz = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Iniciando API en el puerto 5105" -ForegroundColor Cyan
Start-Process dotnet -ArgumentList @("run", "--project", "GestionTubos.Api", "--urls", "http://0.0.0.0:5105") -WorkingDirectory $raiz -WindowStyle Hidden

Write-Host "Iniciando frontend en el puerto 5000" -ForegroundColor Cyan
Start-Process dotnet -ArgumentList @("run", "--project", "GestionTubos.Client", "--urls", "http://0.0.0.0:5000") -WorkingDirectory $raiz -WindowStyle Hidden

Start-Sleep -Seconds 4
Write-Host "Sistema disponible en http://localhost:5000" -ForegroundColor Green
$ipLocal = [System.Net.Dns]::GetHostEntry([System.Net.Dns]::GetHostName()).AddressList |
    Where-Object { $_.AddressFamily -eq [System.Net.Sockets.AddressFamily]::InterNetwork -and $_.ToString() -notlike "169.254*" } |
    Select-Object -First 1
if ($ipLocal) {
    Write-Host "Desde otro dispositivo de la misma red: http://$($ipLocal):5000" -ForegroundColor Green
}
