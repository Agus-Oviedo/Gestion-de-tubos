# Control de Stock de Tubos - Oxígeno Norte

Sistema web interno para registrar los tubos existentes y mantener un stock claro por gas, capacidad, propietario, ubicación y estado.

La solución utiliza una API ASP.NET Core en .NET 8, un cliente Blazor WebAssembly y SQL Server. La autenticación se realiza con ASP.NET Identity y JWT.

## Funcionalidad principal

- Inicio de sesión mediante correo y contraseña.
- Alta de cada tubo con un ID interno generado automáticamente.
- Código de serie grabado, obligatorio y único para cada tubo.
- Clasificación por gas, capacidad, propietario, ubicación y estado.
- Consulta del stock con búsqueda y filtros.
- Edición y baja lógica de tubos.
- Resumen del stock por estado, gas y ubicación.
- Interfaz responsive para computadoras, tablets y celulares.
- Configuración de gases, capacidades y ubicaciones.

## Datos de cada tubo

- ID interno automático, que no debe ingresarse manualmente.
- Código de serie grabado en el tubo.
- Tipo de gas.
- Capacidad.
- Marca opcional.
- Propietario.
- Ubicación.
- Estado: disponible, en uso, mantenimiento o extraviado.
- Fecha de la última prueba hidráulica y fecha de vencimiento.
- Observaciones opcionales.

## Componentes

- `GestionTubos.Api`: API REST.
- `GestionTubos.Client`: interfaz web.
- `GestionTubos.Domain`: entidades del dominio.
- `GestionTubos.Application`: contratos de servicios.
- `GestionTubos.Infrastructure`: acceso a SQL Server, Identity y servicios.
- `GestionTubos.Shared`: contratos compartidos entre cliente y API.

## Configuración y ejecución

1. Copiar `GestionTubos.Api/appsettings.example.json` como `GestionTubos.Api/appsettings.json`.
2. Configurar la conexión `GestionTubos`, la clave JWT y el usuario inicial.
3. Restaurar dependencias y actualizar la base:

   ```powershell
   dotnet restore
   dotnet ef database update --project GestionTubos.Infrastructure --startup-project GestionTubos.Api
   ```

4. Ejecutar:

   ```powershell
   .\iniciar.ps1
   ```

Frontend: http://localhost:5000

API: http://localhost:5105

## Publicación en MonsterASP

### Horarios y registro de cuentas

Los instantes se conservan en UTC en la base y se muestran en la zona `America/Argentina/Buenos_Aires`. Los vencimientos, las fechas sugeridas y los números diarios utilizan la fecha argentina, sin depender de la zona horaria del servidor o del navegador.

Desde el inicio de sesión, **Crear una cuenta** permite registrarse con nombre, correo y contraseña. Las cuentas nuevas quedan inactivas con rol Empleado. Un Administrador o Superadministrador las habilita en **Usuarios → Aprobar cuenta**. La aprobación no implica verificar la propiedad del correo. El registro público no acepta elegir roles ni habilitar la cuenta.

`verificar-registro.ps1` prueba registro, rechazo de duplicados, bloqueo de acceso pendiente, aprobación y permisos contra una instancia local; elimina su cuenta temporal al terminar.

La API también sirve la interfaz Blazor, de modo que ambos componentes se publican en un único sitio IIS. En producción, el cliente utiliza la dirección del sitio para conectarse a la API; en desarrollo conserva el puerto 5105. `ApiUrl` permite configurar una dirección alternativa.

```powershell
dotnet publish GestionTubos.Api/GestionTubos.Api.csproj -c Release -o .publish/monsterasp
```

Antes de cargar el contenido de `.publish/monsterasp`, configurar allí la conexión SQL Server de MonsterASP, la clave JWT y el usuario inicial. No colocar secretos en `wwwroot`: sus archivos son públicos. La configuración local de SQL Server no sirve en el hosting.

Restaurar una copia de seguridad de la base existente en una base separada de MonsterASP antes de iniciar la aplicación. Esto conserva usuarios, catálogos y movimientos. Habilitar HTTPS y verificar el inicio de sesión y el stock después de publicar. La carpeta `.publish` está excluida del control de versiones porque puede contener configuraciones privadas y copias de seguridad.
