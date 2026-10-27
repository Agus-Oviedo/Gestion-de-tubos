using GestionTubos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionTubos.Infrastructure.Configurations;
public sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> b) { b.ToTable("Pedidos"); b.HasKey(x=>x.Id); b.Property(x=>x.Numero).HasMaxLength(30).IsRequired(); b.HasIndex(x=>x.Numero).IsUnique(); b.Property(x=>x.Estado).HasMaxLength(30).IsRequired(); b.Property(x=>x.UsuarioCreador).HasMaxLength(150).IsRequired(); b.Property(x=>x.Observaciones).HasMaxLength(1000); b.Property(x=>x.ModalidadEntrega).HasMaxLength(30).IsRequired(); b.Property(x=>x.DireccionEntregaTexto).HasMaxLength(500); b.Property(x=>x.ContactoEntrega).HasMaxLength(150); b.Property(x=>x.TelefonoEntrega).HasMaxLength(50); b.HasOne(x=>x.Cliente).WithMany().HasForeignKey(x=>x.ClienteId).OnDelete(DeleteBehavior.Restrict); }
}
public sealed class PedidoDetalleConfiguration : IEntityTypeConfiguration<PedidoDetalle>
{
    public void Configure(EntityTypeBuilder<PedidoDetalle> b) { b.ToTable("PedidoDetalles"); b.HasKey(x=>x.Id); b.HasIndex(x=>new{x.PedidoId,x.TuboId}).IsUnique(); b.HasOne(x=>x.Pedido).WithMany(x=>x.Detalles).HasForeignKey(x=>x.PedidoId); b.HasOne(x=>x.Tubo).WithMany().HasForeignKey(x=>x.TuboId).OnDelete(DeleteBehavior.Restrict); }
}
public sealed class RemitoConfiguration : IEntityTypeConfiguration<Remito>
{
    public void Configure(EntityTypeBuilder<Remito> b) { b.Property(x=>x.Version).IsConcurrencyToken(); b.ToTable("Remitos"); b.HasKey(x=>x.Id); b.Property(x=>x.Numero).HasMaxLength(30).IsRequired(); b.HasIndex(x=>x.Numero).IsUnique(); b.HasIndex(x=>x.PedidoId).IsUnique(); b.Property(x=>x.Estado).HasMaxLength(30).IsRequired(); b.Property(x=>x.UsuarioEmisor).HasMaxLength(150).IsRequired(); b.Property(x=>x.PersonaRecibe).HasMaxLength(150); b.Property(x=>x.DniRecibe).HasMaxLength(20); b.Property(x=>x.UsuarioEntrega).HasMaxLength(150); b.Property(x=>x.HuellaDocumento).HasMaxLength(64); b.Property(x=>x.FirmaDataUrl).HasColumnType("nvarchar(max)"); b.HasOne(x=>x.Pedido).WithOne(x=>x.Remito).HasForeignKey<Remito>(x=>x.PedidoId).OnDelete(DeleteBehavior.Restrict); }
}
