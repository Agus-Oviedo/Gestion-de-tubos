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

## Stock de vacíos y carga externa

En **Gestión → Vacíos y carga**, los tubos propios devueltos quedan vacíos en planta. Los dañados pasan a mantenimiento. Los vacíos aptos se pueden seleccionar individualmente o seleccionar todos para preparar un remito con destino/cargadora y transporte. La preparación reserva los tubos en planta; **Confirmar salida** los mueve a carga externa y los excluye del stock físico en planta. Un remito preparado se puede cancelar y libera sus tubos.

Cada envío permite varias recepciones. Seleccionar únicamente los tubos que regresaron y registrar su resultado: lleno, vacío/rechazado o dañado. Cada confirmación genera un remito de recepción PDF independiente. Los no recibidos permanecen pendientes con los días desde la salida. Los llenos aptos quedan disponibles, los vacíos vuelven al stock de vacíos y los dañados pasan a mantenimiento. Una prueba hidráulica vencida al regreso obliga a inspección. Los tubos propios vacíos no se ofrecen para entregar a clientes.

Un faltante definitivo se puede resolver con un motivo y confirmación expresa; el tubo queda extraviado. El envío se cierra cuando todos fueron recibidos o resueltos. Los remitos conservan los códigos, gases y capacidades originales. Las reservas y recepciones usan transacciones y se rechazan tubos repetidos o ya recibidos. No se permite editar ni dar de baja tubos con un envío activo.

`./verificar-cargas.ps1` verifica el circuito en una base SQL Server temporal separada, que se elimina al finalizar. Usa la configuración local de conexión y requiere permisos para crear esa base de prueba.

### Apartados de carga

- **Stock de vacíos**: búsqueda por código, gas, capacidad o ubicación; filtro por aptitud y listas de 25 tubos. La selección se conserva entre búsquedas y páginas. Seleccionar todos se aplica a los aptos encontrados en toda la búsqueda.
- **Envíos a carga**: solo envíos preparados, enviados o parciales. Cada envío tiene su detalle y un apartado separado de recepción, con búsqueda de tubos pendientes.
- **Remitos de carga**: historial independiente de envíos y recepciones, incluidos envíos completos y cancelados. Permite buscar por remito o código de tubo, filtrar por tipo y descargar los PDF. Los remitos incluyen logo, tablas y páginas numeradas; los preparados se identifican como borrador.

Al preparar un nuevo envío desde el stock se abre una vista previa emergente. Volver a editar no crea un remito ni reserva tubos. Confirmar envío guarda el remito y registra la salida en una sola transacción, conservando el documento confirmado en la ventana para descargarlo. Los borradores existentes siguen pudiendo confirmarse desde su detalle.


## Tubos propiedad de clientes

- Al registrar un tubo, seleccionar **Cliente (vinculado a su ficha)** y elegir el propietario. El registro por sí solo no simula una recepción física.
- Desde los tubos del cliente o desde **Devoluciones**, usar **Recibir para carga** para generar el remito de ingreso de los tubos vacíos.
- Los tubos recibidos y aptos aparecen en **Carga externa**, junto con los propios. Los remitos de envío y recepción conservan el propietario de cada tubo.
- Al regresar llenos se entregan mediante un remito al mismo propietario. La firma de entrega cierra el ingreso y no genera alquiler por un tubo del cliente.
- Los remitos de ingreso están disponibles en el historial y en PDF. Sus datos confirmados no se reescriben.

La migración `TubosClientesEnCargaExterna` agrega las recepciones y los datos de propiedad de los envíos, sin reemplazar el inventario existente. Las correcciones de registros antiguos requieren identificar expresamente el tubo y su propietario; no se vinculan por coincidencias parciales de nombres.
