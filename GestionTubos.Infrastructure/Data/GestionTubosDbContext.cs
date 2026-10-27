using GestionTubos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GestionTubos.Infrastructure.Data;

public sealed class GestionTubosDbContext(DbContextOptions<GestionTubosDbContext> options)
    : IdentityDbContext<UsuarioAplicacion, IdentityRole, string>(options)
{
    public DbSet<UsoBateria> UsosBateria => Set<UsoBateria>();
    public DbSet<AuditoriaRemito> AuditoriaRemitos => Set<AuditoriaRemito>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<DireccionCliente> DireccionesCliente => Set<DireccionCliente>();
    public DbSet<TipoGas> TiposGas => Set<TipoGas>();
    public DbSet<CapacidadTubo> CapacidadesTubo => Set<CapacidadTubo>();
    public DbSet<Ubicacion> Ubicaciones => Set<Ubicacion>();
    public DbSet<Tubo> Tubos => Set<Tubo>();
    public DbSet<MovimientoCilindro> MovimientosCilindro => Set<MovimientoCilindro>();
    public DbSet<PruebaHidraulica> PruebasHidraulicas => Set<PruebaHidraulica>();
    public DbSet<OperacionCilindro> OperacionesCilindro => Set<OperacionCilindro>();
    public DbSet<RemitoDevolucion> RemitosDevolucion => Set<RemitoDevolucion>();
    public DbSet<DevolucionLote> DevolucionesLote => Set<DevolucionLote>();
    public DbSet<DevolucionLoteDetalle> DevolucionesLoteDetalle => Set<DevolucionLoteDetalle>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoDetalle> PedidoDetalles => Set<PedidoDetalle>();
    public DbSet<Remito> Remitos => Set<Remito>();
    public DbSet<Reparto> Repartos => Set<Reparto>();
    public DbSet<RepartoDetalle> RepartoDetalles => Set<RepartoDetalle>();

    public DbSet<EnvioCarga> EnviosCarga => Set<EnvioCarga>();
    public DbSet<EnvioCargaDetalle> EnviosCargaDetalle => Set<EnvioCargaDetalle>();
    public DbSet<RecepcionCarga> RecepcionesCarga => Set<RecepcionCarga>();
    public DbSet<RecepcionCargaDetalle> RecepcionesCargaDetalle => Set<RecepcionCargaDetalle>();
    private void ValidarAuditoria(){if(ChangeTracker.Entries<UsoBateria>().Any(x=>x.State==EntityState.Deleted||(x.State==EntityState.Modified&&x.OriginalValues.GetValue<DateTime?>(nameof(UsoBateria.FechaRetiroUtc)).HasValue)))throw new InvalidOperationException("Un ciclo de batería cerrado no puede modificarse ni eliminarse.");if(ChangeTracker.Entries<AuditoriaRemito>().Any(x=>x.State is EntityState.Modified or EntityState.Deleted))throw new InvalidOperationException("La auditoría no puede modificarse ni eliminarse.");}
    private long[] RemitosAfectados()=>ChangeTracker.Entries().Where(e=>e.State is EntityState.Modified or EntityState.Deleted or EntityState.Added).Select(e=>e.Entity switch{Pedido p when e.State!=EntityState.Added=>p.Id,Remito r when e.State!=EntityState.Added=>r.PedidoId,PedidoDetalle d=>d.PedidoId,_=>0L}).Where(id=>id>0).Distinct().ToArray();
    public override int SaveChanges(bool aceptar){ValidarAuditoria();var ids=RemitosAfectados();if(ids.Length>0&&Remitos.AsNoTracking().Any(x=>ids.Contains(x.PedidoId)&&(x.FechaFirmaUtc!=null||x.Estado=="Firmado")))throw new InvalidOperationException("Un remito firmado no puede modificarse ni anularse.");return base.SaveChanges(aceptar);}
    public override async Task<int> SaveChangesAsync(bool aceptar,CancellationToken ct=default){ValidarAuditoria();var ids=RemitosAfectados();if(ids.Length>0&&await Remitos.AsNoTracking().AnyAsync(x=>ids.Contains(x.PedidoId)&&(x.FechaFirmaUtc!=null||x.Estado=="Firmado"),ct))throw new InvalidOperationException("Un remito firmado no puede modificarse ni anularse.");return await base.SaveChangesAsync(aceptar,ct);}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GestionTubosDbContext).Assembly);
    }
}
