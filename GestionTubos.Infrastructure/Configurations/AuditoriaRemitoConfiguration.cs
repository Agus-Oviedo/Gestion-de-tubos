using GestionTubos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace GestionTubos.Infrastructure.Configurations;
public sealed class AuditoriaRemitoConfiguration:IEntityTypeConfiguration<AuditoriaRemito>
{
 public void Configure(EntityTypeBuilder<AuditoriaRemito>b){b.ToTable("AuditoriaRemitos");b.HasKey(x=>x.Id);b.HasIndex(x=>new{x.PedidoId,x.Version}).IsUnique();b.HasIndex(x=>x.FechaUtc);b.Property(x=>x.NumeroRemito).HasMaxLength(30);b.Property(x=>x.UsuarioId).HasMaxLength(450);b.Property(x=>x.Usuario).HasMaxLength(256);b.Property(x=>x.Accion).HasMaxLength(60);b.Property(x=>x.Motivo).HasMaxLength(1000);}
}
