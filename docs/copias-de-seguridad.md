# Copias locales al abrir el sistema

La copia se intenta una vez por día (fecha de Argentina) al entrar al sistema
con una cuenta Administrador o Superadministrador. La configuración pertenece
a ese perfil de Chrome en esa PC. No se ejecuta con el navegador cerrado.

## Configurar en la PC con Windows 7

1. Abrir el sitio por HTTPS en Chrome 109 e iniciar sesión como administrador.
2. En **Copia de seguridad en esta PC**, pulsar **Elegir carpeta de copias**.
3. Seleccionar una carpeta destinada a los respaldos y autorizar la escritura.
4. Esperar **Copia de hoy guardada y descarga verificada**.

Al volver a abrir el sistema, se usa la carpeta guardada. Si Chrome conserva
el permiso, la copia se realiza automáticamente. Chrome 109 puede pedirlo de
nuevo al reiniciar: pulsar **Autorizar / reintentar copia**. El sitio no puede
saltarse ese permiso. La falta de permiso o internet, un disco lleno o una
descarga dañada dejan la copia pendiente, con un aviso visible.

La fecha de éxito se guarda solo después de cerrar el archivo y verificar su
SHA-256 leyendo de nuevo el archivo local. Dos pestañas del mismo perfil no
generan copias simultáneas. Cambiar de carpeta genera una copia en la nueva.
No se borran ni sobrescriben las copias anteriores. La PC debe tener espacio
disponible y la pestaña debe permanecer abierta hasta terminar.

## Contenido y límites

Es un ZIP de recuperación propio de Gestión de Tubos, no un `.bak` nativo de
SQL Server. Incluye todas las tablas de la base, sus columnas y tipos, valores
de identidad, historial de migraciones, usuarios y roles, stock, clientes,
movimientos, firmas y datos de los remitos. Los PDF se vuelven a generar desde
esos datos. Un manifiesto registra la versión, fecha UTC, filas y SHA-256 de
cada tabla. La lectura se realiza dentro de una transacción consistente.

No incluye el código ni la configuración privada del hosting (conexión SQL,
clave JWT, etc.), que se conservan por separado. La versión del programa usada
para recuperar debe coincidir con las migraciones del respaldo. El ZIP contiene
información privada y credenciales de usuarios de la aplicación: restringir el
acceso a la carpeta y conservar además una copia fuera de esa PC.

La exportación admite hasta 64 MB descomprimidos. Si la base crece por encima
de ese límite, debe usarse una estrategia nativa en el servidor. No es una
copia continua: lo registrado después del respaldo se guarda al día siguiente.

## Recuperación controlada

Se realiza desde una PC técnica con .NET 8 o posterior y acceso a SQL Server,
no desde Windows 7 ni con un botón que sobrescriba producción.

Configurar la variable de entorno `GESTION_TUBOS_RECUPERACION` con una conexión
a una base **que todavía no exista**, llamada
`GestionTubos_Recuperacion_<identificador>`. El usuario SQL necesita permiso
para crear esa base. Ejecutar desde el código de la versión correspondiente:

```powershell
dotnet run --project tools/RecuperarRespaldo -- "C:\Copias\oxigeno-norte-fecha.zip"
```

La herramienta valida el ZIP y todas sus sumas antes de crear la base, aplica
las migraciones, recupera datos e identidades, comprueba las relaciones y
compara todas las filas. Rechaza bases existentes y nombres de producción.
Si falla, la transacción revierte los datos importados; puede quedar la base
nueva con su esquema inicial para revisión técnica. Nunca elimina una base.

Después de recuperar, verificar inicio de sesión, stock y remitos en una
instancia separada. Solo después se decide cambiar la conexión de producción.

## Verificación técnica

`tests/Respaldos` crea dos bases temporales separadas, exporta y recupera datos,
verifica usuarios, roles e identidades y rechaza archivos dañados y destinos
existentes. Las dos bases de prueba se eliminan al finalizar.
`node tests/Navegador/respaldo-local.cjs` verifica permisos, frecuencia diaria,
concurrencia, integridad y errores de guardado sin tocar carpetas reales.
