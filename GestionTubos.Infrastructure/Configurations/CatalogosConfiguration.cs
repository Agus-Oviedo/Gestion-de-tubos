using GestionTubos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionTubos.Infrastructure.Configurations;

public sealed class TipoGasConfiguration : IEntityTypeConfiguration<TipoGas>
{
    public void Configure(EntityTypeBuilder<TipoGas> b)
    {
        b.ToTable("TiposGas"); b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(300);
        b.HasIndex(x => x.Nombre).IsUnique(); b.HasQueryFilter(x => x.Activo);
    }
}
public sealed class CapacidadTuboConfiguration : IEntityTypeConfiguration<CapacidadTubo>
{
    public void Configure(EntityTypeBuilder<CapacidadTubo> b)
    {
        b.ToTable("CapacidadesTubo"); b.HasKey(x => x.Id);
        b.Property(x => x.Valor).HasPrecision(10, 2);
        b.Property(x => x.Unidad).HasMaxLength(20).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(200);
        b.HasIndex(x => new { x.Valor, x.Unidad }).IsUnique(); b.HasQueryFilter(x => x.Activa);
    }
}
public sealed class UbicacionConfiguration : IEntityTypeConfiguration<Ubicacion>
{
    public void Configure(EntityTypeBuilder<Ubicacion> b)
    {
        b.ToTable("Ubicaciones"); b.Property(x=>x.GasBateria).HasMaxLength(20); b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(300);
        b.HasIndex(x => x.Nombre).IsUnique(); b.HasQueryFilter(x => x.Activa);
    }
}
