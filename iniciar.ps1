$ErrorActionPreference = "Stop"
$raiz = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Iniciando API en http://localhost:5105" -ForegroundColor Cyan
Start-Process dotnet -ArgumentList @("run", "--project", "GestionTubos.Api", "--urls", "http://localhost:5105") -WorkingDirectory $raiz -WindowStyle Hidden

Write-Host "Iniciando frontend en http://localhost:5000" -ForegroundColor Cyan
Start-Process dotnet -ArgumentList @("run", "--project", "GestionTubos.Client", "--urls", "http://localhost:5000") -WorkingDirectory $raiz -WindowStyle Hidden

Start-Sleep -Seconds 4
Write-Host "Sistema disponible en http://localhost:5000" -ForegroundColor Green
