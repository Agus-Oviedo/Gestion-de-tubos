using GestionTubos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace GestionTubos.Infrastructure.Configurations;
public sealed class EnvioCargaConfiguration:IEntityTypeConfiguration<EnvioCarga>
{
 public void Configure(EntityTypeBuilder<EnvioCarga>b){b.HasIndex(x=>x.Numero).IsUnique();b.Property(x=>x.Numero).HasMaxLength(50);b.Property(x=>x.Destino).HasMaxLength(200);b.Property(x=>x.Transporte).HasMaxLength(200);b.Property(x=>x.Estado).HasMaxLength(30);b.Property(x=>x.Usuario).HasMaxLength(254);b.Property(x=>x.Observaciones).HasMaxLength(1000);}
}
public sealed class EnvioCargaDetalleConfiguration:IEntityTypeConfiguration<EnvioCargaDetalle>
{
 public void Configure(EntityTypeBuilder<EnvioCargaDetalle>b){b.Property(x=>x.Propietario).HasMaxLength(250).HasDefaultValue("Oxígeno Norte");b.Property(x=>x.TipoPropietario).HasDefaultValue(1);b.HasIndex(x=>new{x.EnvioCargaId,x.TuboId}).IsUnique();b.Property(x=>x.Codigo).HasMaxLength(50);b.Property(x=>x.Gas).HasMaxLength(200);b.Property(x=>x.Capacidad).HasMaxLength(100);b.HasOne(x=>x.Tubo).WithMany().HasForeignKey(x=>x.TuboId).OnDelete(DeleteBehavior.Restrict);b.Property(x=>x.Estado).HasMaxLength(30);b.Property(x=>x.Resolucion).HasMaxLength(1000);}
}
public sealed class RecepcionCargaConfiguration:IEntityTypeConfiguration<RecepcionCarga>
{
 public void Configure(EntityTypeBuilder<RecepcionCarga>b){b.HasIndex(x=>x.Numero).IsUnique();b.Property(x=>x.Numero).HasMaxLength(50);b.Property(x=>x.Usuario).HasMaxLength(254);b.Property(x=>x.Observaciones).HasMaxLength(1000);b.HasOne(x=>x.EnvioCarga).WithMany(x=>x.Recepciones).HasForeignKey(x=>x.EnvioCargaId).OnDelete(DeleteBehavior.Restrict);}
}
public sealed class RecepcionCargaDetalleConfiguration:IEntityTypeConfiguration<RecepcionCargaDetalle>
{
 public void Configure(EntityTypeBuilder<RecepcionCargaDetalle>b){b.HasIndex(x=>x.EnvioCargaDetalleId).IsUnique();b.HasOne(x=>x.EnvioCargaDetalle).WithMany().HasForeignKey(x=>x.EnvioCargaDetalleId).OnDelete(DeleteBehavior.Restrict);b.Property(x=>x.Resultado).HasMaxLength(30);}
}
