# Gestión de Cilindros - Oxígeno Norte

Sistema web interno para administrar clientes, cilindros, stock y trazabilidad. La solución utiliza una API ASP.NET Core independiente y un frontend Blazor WebAssembly que se comunican mediante HTTP/JSON.

## Componentes

- `GestionTubos.Api`: API REST en .NET 8.
- `GestionTubos.Client`: frontend Blazor WebAssembly.
- `GestionTubos.Domain`: entidades y estados del negocio.
- `GestionTubos.Application`: interfaces de casos de uso.
- `GestionTubos.Infrastructure`: EF Core, SQL Server y servicios.
- `GestionTubos.Shared`: contratos JSON compartidos.

## Funcionalidad disponible

- Clientes persona y empresa, con múltiples direcciones.
- Catálogos de gases, capacidades y ubicaciones.
- Registro individual de cilindros y baja lógica.
- Estados físico, de contenido y de disponibilidad separados.
- Pruebas hidráulicas, certificados y vencimientos calculados.
- Entregas por alquiler, comodato o intercambio.
- Recepciones con estado físico, contenido e inspección requerida.
- Historial inalterable de movimientos.
- Stock derivado de cilindros individuales.
- Alertas de pruebas vencidas o próximas a vencer.
- Panel con indicadores y últimos movimientos.

## Base de datos

Configurar el servidor SQL Server de cada PC en `GestionTubos.Api/appsettings.json`, con autenticación de Windows.

Base: `GestionTubosDb`.

Las migraciones se aplican con:

```powershell
dotnet ef database update --project GestionTubos.Infrastructure --startup-project GestionTubos.Api
```

También se mantiene el script idempotente `database/GestionTubosDb.sql` para SSMS.

## Instalar en otra PC

Requisitos: Git, SDK de .NET 8 y SQL Server (Express o Developer para desarrollo).

1. Clonar este repositorio y abrir PowerShell en su carpeta.
2. Copiar la configuración de ejemplo:

   ```powershell
   Copy-Item GestionTubos.Api/appsettings.example.json GestionTubos.Api/appsettings.json
   ```

3. Editar el archivo copiado: indicar la instancia de SQL Server en `ConnectionStrings:GestionTubos`, completar `Jwt:Key` con una clave aleatoria de al menos 32 bytes y elegir `UsuarioInicial:Correo` y `UsuarioInicial:Contrasena`. La contraseña debe tener mayúsculas, minúsculas, números y un símbolo. Este archivo es privado y Git lo ignora.
4. Restaurar dependencias y crear o actualizar la base:

   ```powershell
   dotnet restore
   dotnet tool install --global dotnet-ef --version 8.0.21
   dotnet ef database update --project GestionTubos.Infrastructure --startup-project GestionTubos.Api
   ```

   Si `dotnet-ef` ya está instalado, omitir su instalación. Las migraciones son la fuente actualizada del esquema.

5. Ejecutar `./iniciar.ps1` y abrir http://localhost:5000. En una base nueva, ingresar con el usuario inicial configurado.

Git no copia los datos de SQL Server. Para conservar clientes, cilindros, usuarios y movimientos existentes, hacer un backup de `GestionTubosDb` y restaurarlo en la otra PC antes de aplicar migraciones. Los backups se transfieren por separado y no se suben al repositorio. Las dos instalaciones con bases locales tendrán datos independientes.

## Trabajar desde dos PCs

Antes de empezar, ejecutar `git pull --ff-only`. Al terminar:

```powershell
git add .
git commit -m "Descripción de los cambios"
git push
```

Subir los cambios de una PC antes de continuar en la otra. La configuración privada se mantiene por separado en cada equipo.

## Ejecución

Desde PowerShell en la carpeta raíz:

```powershell
.\iniciar.ps1
```

O en dos terminales:

```powershell
dotnet run --project GestionTubos.Api --urls http://localhost:5105
dotnet run --project GestionTubos.Client --urls http://localhost:5000
```

Frontend: http://localhost:5000

API: http://localhost:5105

## Flujo de uso recomendado

1. Crear gases, capacidades y ubicaciones en Catálogos.
2. Registrar clientes y sus direcciones.
3. Registrar cada cilindro individualmente.
4. Registrar su prueba hidráulica vigente.
5. Confirmar una entrega.
6. Registrar la recepción cuando regrese.
7. Consultar movimientos y alertas desde el panel.
