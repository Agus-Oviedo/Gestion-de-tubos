using GestionTubos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionTubos.Infrastructure.Configurations;

public sealed class MovimientoCilindroConfiguration : IEntityTypeConfiguration<MovimientoCilindro>
{
    public void Configure(EntityTypeBuilder<MovimientoCilindro> b)
    {
        b.ToTable("MovimientosCilindro"); b.HasKey(x => x.Id);
        b.Property(x => x.Tipo).HasMaxLength(50).IsRequired(); b.Property(x => x.EstadoAnterior).HasMaxLength(300);
        b.Property(x => x.EstadoPosterior).HasMaxLength(300).IsRequired(); b.Property(x => x.UsuarioResponsable).HasMaxLength(150).IsRequired(); b.Property(x => x.Observaciones).HasMaxLength(1000);
        b.HasOne(x => x.Tubo).WithMany().HasForeignKey(x => x.TuboId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.TuboId, x.FechaHoraUtc });
    }
}
public sealed class PruebaHidraulicaConfiguration : IEntityTypeConfiguration<PruebaHidraulica>
{
    public void Configure(EntityTypeBuilder<PruebaHidraulica> b)
    {
        b.ToTable("PruebasHidraulicas"); b.HasKey(x => x.Id); b.Property(x => x.EntidadRealizadora).HasMaxLength(200).IsRequired(); b.Property(x => x.NumeroCertificado).HasMaxLength(100); b.Property(x => x.Observaciones).HasMaxLength(1000);
        b.HasOne(x => x.Tubo).WithMany().HasForeignKey(x => x.TuboId).OnDelete(DeleteBehavior.Restrict); b.HasIndex(x => new { x.TuboId, x.FechaVencimiento });
    }
}
public sealed class OperacionCilindroConfiguration : IEntityTypeConfiguration<OperacionCilindro>
{
    public void Configure(EntityTypeBuilder<OperacionCilindro> b)
    {
        b.ToTable("OperacionesCilindro"); b.HasKey(x => x.Id); b.Property(x => x.Numero).HasMaxLength(30).IsRequired(); b.HasIndex(x => x.Numero).IsUnique();
        b.Property(x => x.Tipo).HasMaxLength(30).IsRequired(); b.Property(x => x.Estado).HasMaxLength(30).IsRequired(); b.Property(x => x.PersonaRecibe).HasMaxLength(150); b.Property(x => x.Observaciones).HasMaxLength(1000);
        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x => x.Tubo).WithMany().HasForeignKey(x => x.TuboId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.RemitoEntrega).WithMany().HasForeignKey(x => x.RemitoEntregaId).OnDelete(DeleteBehavior.Restrict); b.HasIndex(x => x.RemitoEntregaId);
    }
}
public sealed class RemitoDevolucionConfiguration : IEntityTypeConfiguration<RemitoDevolucion>
{
    public void Configure(EntityTypeBuilder<RemitoDevolucion> b)
    {
        b.ToTable("RemitosDevolucion"); b.HasKey(x => x.Id);
        b.Property(x => x.Numero).HasMaxLength(30).IsRequired(); b.HasIndex(x => x.Numero).IsUnique();
        b.HasIndex(x => x.OperacionCilindroId).IsUnique();
        b.Property(x => x.Estado).HasMaxLength(30).IsRequired();
        b.Property(x => x.UsuarioReceptor).HasMaxLength(150).IsRequired();
        b.Property(x => x.PersonaEntrega).HasMaxLength(150); b.Property(x => x.DniEntrega).HasMaxLength(20);
        b.Property(x => x.Observaciones).HasMaxLength(1000);
        b.HasOne(x => x.OperacionCilindro).WithOne(x => x.RemitoDevolucion).HasForeignKey<RemitoDevolucion>(x => x.OperacionCilindroId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class DevolucionLoteConfiguration : IEntityTypeConfiguration<DevolucionLote>
{
    public void Configure(EntityTypeBuilder<DevolucionLote> b)
    {
        b.ToTable("DevolucionesLote");b.HasKey(x=>x.Id);b.Property(x=>x.Numero).HasMaxLength(30).IsRequired();b.HasIndex(x=>x.Numero).IsUnique();b.Property(x=>x.Estado).HasMaxLength(30).IsRequired();b.Property(x=>x.UsuarioReceptor).HasMaxLength(150).IsRequired();b.Property(x=>x.PersonaEntrega).HasMaxLength(150);b.Property(x=>x.DniEntrega).HasMaxLength(20);b.Property(x=>x.FirmaDataUrl).HasColumnType("nvarchar(max)");b.Property(x=>x.Observaciones).HasMaxLength(1000);b.HasOne(x=>x.RemitoEntrega).WithMany().HasForeignKey(x=>x.RemitoEntregaId).OnDelete(DeleteBehavior.Restrict);b.HasOne(x=>x.Cliente).WithMany().HasForeignKey(x=>x.ClienteId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class DevolucionLoteDetalleConfiguration : IEntityTypeConfiguration<DevolucionLoteDetalle>
{
    public void Configure(EntityTypeBuilder<DevolucionLoteDetalle> b)
    {
        b.ToTable("DevolucionesLoteDetalle");b.HasKey(x=>x.Id);b.HasIndex(x=>x.OperacionCilindroId).IsUnique();b.HasOne(x=>x.DevolucionLote).WithMany(x=>x.Detalles).HasForeignKey(x=>x.DevolucionLoteId);b.HasOne(x=>x.OperacionCilindro).WithMany().HasForeignKey(x=>x.OperacionCilindroId).OnDelete(DeleteBehavior.Restrict);
    }
}
