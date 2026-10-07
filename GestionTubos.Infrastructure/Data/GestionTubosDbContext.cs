using GestionTubos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GestionTubos.Infrastructure.Data;

public sealed class GestionTubosDbContext(DbContextOptions<GestionTubosDbContext> options)
    : IdentityDbContext<UsuarioAplicacion, IdentityRole, string>(options)
{
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GestionTubosDbContext).Assembly);
    }
}
