using GestionTubos.Application.Interfaces;
using GestionTubos.Domain.Entities;
using GestionTubos.Infrastructure.Data;
using GestionTubos.Shared.Catalogos;
using Microsoft.EntityFrameworkCore;

namespace GestionTubos.Infrastructure.Services;

public sealed class CatalogoService(GestionTubosDbContext db) : ICatalogoService
{
    public async Task<IReadOnlyList<TipoGasDto>> ObtenerGasesAsync(CancellationToken ct) =>
        await db.TiposGas.AsNoTracking().OrderBy(x => x.Nombre).Select(x => new TipoGasDto(x.Id,x.Nombre,x.Descripcion)).ToListAsync(ct);
    public async Task<TipoGasDto> CrearGasAsync(TipoGasGuardarDto dto, CancellationToken ct)
    {
        var nombre=dto.Nombre.Trim();
        var existente=await db.TiposGas.IgnoreQueryFilters().FirstOrDefaultAsync(x=>x.Nombre==nombre,ct);
        if(existente is not null)
        {
            if(existente.Activo) throw new InvalidOperationException("Ya existe ese tipo de gas.");
            existente.Activo=true; existente.Descripcion=Limpiar(dto.Descripcion); await db.SaveChangesAsync(ct);
            return new(existente.Id,existente.Nombre,existente.Descripcion);
        }
        var x=new TipoGas{Nombre=nombre,Descripcion=Limpiar(dto.Descripcion)}; db.Add(x); await db.SaveChangesAsync(ct); return new(x.Id,x.Nombre,x.Descripcion);
    }
    public async Task<bool> EliminarGasAsync(int id,CancellationToken ct){var x=await db.TiposGas.FirstOrDefaultAsync(x=>x.Id==id,ct);if(x is null)return false;x.Activo=false;await db.SaveChangesAsync(ct);return true;}

    public async Task<IReadOnlyList<CapacidadDto>> ObtenerCapacidadesAsync(CancellationToken ct) =>
        await db.CapacidadesTubo.AsNoTracking().OrderBy(x=>x.Valor).Select(x=>new CapacidadDto(x.Id,x.Valor,x.Unidad,x.Descripcion)).ToListAsync(ct);
    public async Task<CapacidadDto> CrearCapacidadAsync(CapacidadGuardarDto dto,CancellationToken ct)
    {
        var unidad=dto.Unidad.Trim(); var existente=await db.CapacidadesTubo.IgnoreQueryFilters().FirstOrDefaultAsync(x=>x.Valor==dto.Valor&&x.Unidad==unidad,ct);
        if(existente is not null)
        {
            if(existente.Activa)throw new InvalidOperationException("Ya existe esa capacidad.");
            existente.Activa=true; existente.Descripcion=Limpiar(dto.Descripcion); await db.SaveChangesAsync(ct);
            return new(existente.Id,existente.Valor,existente.Unidad,existente.Descripcion);
        }
        var x=new CapacidadTubo{Valor=dto.Valor,Unidad=unidad,Descripcion=Limpiar(dto.Descripcion)};db.Add(x);await db.SaveChangesAsync(ct);return new(x.Id,x.Valor,x.Unidad,x.Descripcion);
    }
    public async Task<bool> EliminarCapacidadAsync(int id,CancellationToken ct){var x=await db.CapacidadesTubo.FirstOrDefaultAsync(x=>x.Id==id,ct);if(x is null)return false;x.Activa=false;await db.SaveChangesAsync(ct);return true;}

    public async Task<IReadOnlyList<UbicacionDto>> ObtenerUbicacionesAsync(CancellationToken ct) =>
        await db.Ubicaciones.AsNoTracking().OrderBy(x=>x.Nombre).Select(x=>new UbicacionDto(x.Id,x.Nombre,x.Descripcion,x.EsInterna,x.GasBateria)).ToListAsync(ct);
    public async Task<UbicacionDto> CrearUbicacionAsync(UbicacionGuardarDto dto,CancellationToken ct)
    {
        var nombre=dto.Nombre.Trim();var existente=await db.Ubicaciones.IgnoreQueryFilters().FirstOrDefaultAsync(x=>x.Nombre==nombre,ct);
        if(existente is not null)
        {
            if(existente.Activa)throw new InvalidOperationException("Ya existe esa ubicación.");
            existente.Activa=true; existente.Descripcion=Limpiar(dto.Descripcion); existente.EsInterna=dto.EsInterna; await db.SaveChangesAsync(ct);
            return new(existente.Id,existente.Nombre,existente.Descripcion,existente.EsInterna);
        }
        var x=new Ubicacion{Nombre=nombre,Descripcion=Limpiar(dto.Descripcion),EsInterna=dto.EsInterna};db.Add(x);await db.SaveChangesAsync(ct);return new(x.Id,x.Nombre,x.Descripcion,x.EsInterna);
    }
    public async Task<UbicacionDto?> ActualizarUbicacionAsync(int id,UbicacionGuardarDto dto,CancellationToken ct)
    {
        var x=await db.Ubicaciones.FirstOrDefaultAsync(x=>x.Id==id,ct);
        if(x is null)return null;
        if(x.GasBateria is not null)throw new InvalidOperationException("Las planchadas del sistema se gestionan desde Planchadas y baterías.");
        var nombre=dto.Nombre.Trim();
        if(string.IsNullOrWhiteSpace(nombre))throw new InvalidOperationException("El nombre es obligatorio.");
        if(await db.Ubicaciones.IgnoreQueryFilters().AnyAsync(u=>u.Id!=id&&u.Nombre==nombre,ct))throw new InvalidOperationException("Ya existe una ubicación con ese nombre.");
        if(x.EsInterna!=dto.EsInterna&&await db.Tubos.AnyAsync(t=>t.UbicacionId==id,ct))throw new InvalidOperationException("No se puede cambiar entre interna y externa mientras tenga tubos asignados.");
        x.Nombre=nombre;x.Descripcion=Limpiar(dto.Descripcion);x.EsInterna=dto.EsInterna;
        await db.SaveChangesAsync(ct);
        return new(x.Id,x.Nombre,x.Descripcion,x.EsInterna);
    }
    public async Task<bool> EliminarUbicacionAsync(int id,CancellationToken ct)
    {
        var x=await db.Ubicaciones.FirstOrDefaultAsync(x=>x.Id==id,ct);if(x is null)return false;
        if(x.GasBateria is not null)throw new InvalidOperationException("No se pueden eliminar las planchadas del sistema.");
        if(await db.Tubos.AnyAsync(t=>t.UbicacionId==id,ct))throw new InvalidOperationException("Primero trasladá los tubos asignados a otra ubicación.");
        x.Activa=false;await db.SaveChangesAsync(ct);return true;
    }
    private static string? Limpiar(string? x)=>string.IsNullOrWhiteSpace(x)?null:x.Trim();
}
