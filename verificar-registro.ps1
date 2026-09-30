param([string]$BaseUrl = 'http://localhost:5117')
$ErrorActionPreference = 'Stop'
$cfg = Get-Content "$PSScriptRoot/GestionTubos.Api/appsettings.json" -Raw | ConvertFrom-Json
$correo = 'prueba-registro-' + [Guid]::NewGuid().ToString('N') + '@example.invalid'
$clave = 'Prueba-' + [Guid]::NewGuid().ToString('N') + 'Aa1'
function Llamar($ruta, $datos, $token = '') {
    $headers = @{}
    if ($token) { $headers.Authorization = "Bearer $token" }
    Invoke-WebRequest "$BaseUrl/api/autenticacion/$ruta" -Method Post -ContentType 'application/json' -Body ($datos | ConvertTo-Json) -Headers $headers -SkipHttpErrorCheck
}
function Verificar($condicion, $nombre) {
    if (!$condicion) { throw "Falló: $nombre" }
    Write-Host "OK: $nombre"
}
try {
    $registro = @{NombreCompleto='Prueba temporal'; Correo=$correo; Contrasena=$clave; ConfirmarContrasena=$clave; Rol='Superadministrador'; Activo=$true}
    $r = Llamar 'registro' $registro
    Verificar ($r.StatusCode -eq 200) 'Registro público'
    $r = Llamar 'registro' $registro
    Verificar ($r.StatusCode -eq 409) 'Correo duplicado rechazado'
    $r = Llamar 'login' @{Correo=$correo; Contrasena=$clave}
    Verificar ($r.StatusCode -eq 401) 'Cuenta pendiente no puede ingresar'
    $admin = Llamar 'login' @{Correo=$cfg.UsuarioInicial.Correo; Contrasena=$cfg.UsuarioInicial.Contrasena}
    Verificar ($admin.StatusCode -eq 200) 'Ingreso de administrador'
    $token = ($admin.Content | ConvertFrom-Json).Token
    $lista = Invoke-RestMethod "$BaseUrl/api/autenticacion/usuarios" -Headers @{Authorization="Bearer $token"}
    $usuario = $lista | Where-Object Correo -eq $correo
    Verificar (!$usuario.Activo -and $usuario.Roles.Count -eq 1 -and $usuario.Roles[0] -eq 'Empleado') 'No admite escalamiento de privilegios desde registro'
    $r = Llamar "usuarios/$($usuario.Id)/aprobar" @{}
    Verificar ($r.StatusCode -eq 401) 'Aprobación anónima rechazada'
    $r = Llamar "usuarios/$($usuario.Id)/aprobar" @{} $token
    Verificar ($r.StatusCode -eq 200) 'Administrador aprueba la cuenta'
    $r = Llamar 'login' @{Correo=$correo; Contrasena=$clave}
    Verificar ($r.StatusCode -eq 200) 'Cuenta aprobada puede ingresar'
    $empleado = ($r.Content | ConvertFrom-Json).Token
    $r = Llamar "usuarios/$($usuario.Id)/aprobar" @{} $empleado
    Verificar ($r.StatusCode -eq 403) 'Empleado no puede aprobar cuentas'
} finally {
    # Elimina únicamente la cuenta temporal creada por esta ejecución.
    $conexion = [System.Data.SqlClient.SqlConnection]::new($cfg.ConnectionStrings.GestionTubos)
    $conexion.Open()
    try {
        $cmd = $conexion.CreateCommand()
        $cmd.CommandText = 'DELETE FROM AspNetUsers WHERE Email = @correo'
        [void]$cmd.Parameters.AddWithValue('@correo', $correo)
        [void]$cmd.ExecuteNonQuery()
    } finally { $conexion.Dispose() }
}
