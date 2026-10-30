using GestionTubos.Infrastructure.Data;
using GestionTubos.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace GestionTubos.Api.Controllers;

[ApiController, Route("api/respaldos"), Authorize(Roles = "Superadministrador,Administrador")]
public sealed class RespaldosController(GestionTubosDbContext db, ILogger<RespaldosController> logger) : ControllerBase
{
    private static readonly SemaphoreSlim EnCurso = new(1, 1);

    [HttpPost]
    public async Task<IActionResult> Crear(CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        if (!await EnCurso.WaitAsync(0, ct)) return Conflict(new { mensaje = "Hay otra copia en curso. Intentá nuevamente en unos minutos." });
        try
        {
            var copia = await new RespaldoService(db).CrearAsync(ct);
            Response.Headers["X-Respaldo-Sha256"] = Convert.ToHexString(SHA256.HashData(copia));
            return File(copia, "application/zip", $"oxigeno-norte-{DateTime.UtcNow:yyyyMMdd-HHmmss}-UTC.zip");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo crear el respaldo solicitado por {Usuario}", User.Identity?.Name);
            return Problem("No se pudo generar la copia. No se marcó como realizada; podés reintentar.", statusCode: 503);
        }
        finally { EnCurso.Release(); }
    }
}
