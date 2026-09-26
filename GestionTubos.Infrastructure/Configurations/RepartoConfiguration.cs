using GestionTubos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace GestionTubos.Infrastructure.Configurations;
public sealed class RepartoConfiguration:IEntityTypeConfiguration<Reparto>
{
 public void Configure(EntityTypeBuilder<Reparto>b){b.ToTable("Repartos");b.HasKey(x=>x.Id);b.Property(x=>x.Numero).HasMaxLength(30).IsRequired();b.HasIndex(x=>x.Numero).IsUnique();b.Property(x=>x.Responsable).HasMaxLength(150).IsRequired();b.Property(x=>x.Estado).HasMaxLength(30).IsRequired();b.Property(x=>x.UsuarioCreador).HasMaxLength(150).IsRequired();b.Property(x=>x.Observaciones).HasMaxLength(1000);}
}
public sealed class RepartoDetalleConfiguration:IEntityTypeConfiguration<RepartoDetalle>
{
 public void Configure(EntityTypeBuilder<RepartoDetalle>b){b.ToTable("RepartoDetalles");b.HasKey(x=>x.Id);b.Property(x=>x.Estado).HasMaxLength(30).IsRequired();b.HasIndex(x=>x.PedidoId).IsUnique();b.HasOne(x=>x.Reparto).WithMany(x=>x.Detalles).HasForeignKey(x=>x.RepartoId);b.HasOne(x=>x.Pedido).WithOne(x=>x.RepartoDetalle).HasForeignKey<RepartoDetalle>(x=>x.PedidoId).OnDelete(DeleteBehavior.Restrict);}
}
